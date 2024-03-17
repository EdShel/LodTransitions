using ImGuiNET;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace LodTransitions
{
    public class MyGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private ImGuiRenderer imGuiRenderer;
        private float rotation = 0f;

        private int previewWidth = 200;
        private int previewHeight = 200;

        public MyGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        private Effect axisShader;
        private PerspectiveLookAtTargetCamera camera;
        private PerspectiveLookAtTargetCamera previewCamera;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.imGuiRenderer = new ImGuiRenderer(this);
            this.imGuiRenderer.RebuildFontAtlas();

            this.camera = new PerspectiveLookAtTargetCamera(
                ViewConfig: new LookAtTargetCameraViewConfig
                {
                    LookAt = Vector3.Zero,
                    Position = Vector3.Zero,
                    Up = Vector3.Up,
                },
                ProjectionConfig: new PerspectiveCameraProjectionConfig
                {
                    Fov = MathHelper.PiOver2,
                    NearPlane = 0.001f,
                    FarPlane = 200f,
                    AspectRatio = this.GraphicsDevice.Viewport.AspectRatio,
                }
            );

            this.previewCamera = new PerspectiveLookAtTargetCamera(
                ViewConfig: new LookAtTargetCameraViewConfig
                {
                    LookAt = Vector3.Zero,
                    Position = new Vector3(0.75f, 0, 0),
                    Up = Vector3.Up,
                },
                ProjectionConfig: new PerspectiveCameraProjectionConfig
                {
                    Fov = MathHelper.PiOver2,
                    NearPlane = 0.001f,
                    FarPlane = 200f,
                    AspectRatio = ((float)this.previewWidth) / this.previewHeight,
                }
            );

            base.Initialize();
        }

        private Scene scene;

        private CloseUpPreviewWindow? closeUpPreviewWindow;

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.axisShader = this.Content.Load<Effect>("axis_shader");
            var mainShader = this.Content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);
            var transition = new GeomorphTransition(mainMaterial);
            //var transition = new AlphaTransition(mainMaterial);
            //var transition = new NoiseTransition(mainMaterial);

            //var model = this.Content.Load<Model>("debug_tri");
            var model = this.Content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 8f);
            var lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition, mainMaterial);

            this.scene = new Scene();
            this.scene.AddInstance(lodModelRenderer);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        private float cameraOffset = 1.0f;

        private float progress;

        private bool drawMetrics;
        protected override void Draw(GameTime gameTime)
        {
            this.imGuiRenderer.BeforeLayout(gameTime);

            var world3d = new World3D
            {
                Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                Graphics = this.GraphicsDevice,
                Camera = this.camera,
            };
            var previewWorld3d = new World3D
            {
                Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                Graphics = this.GraphicsDevice,
                Camera = this.previewCamera,
                DebugObserverPosition = this.camera.View.Position
            };

            this.camera.ViewConfig.Position = new Vector3(0, 0, 1f) * Math.Max(0.0001f, this.progress);

            this.axisShader.Parameters["WorldViewProjection"].SetValue(this.camera.View.Matrix * this.camera.Projection.Matrix);
            this.axisShader.CurrentTechnique.Passes[0].Apply();
            this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new[]
            {
                new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(1f, 0, 0), Color.Red),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Green),
                new VertexPositionColor(new Vector3(0, 1f, 0), Color.Green),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1f), Color.Blue),
            }, 0, 3);

            if (this.closeUpPreviewWindow != null)
            {
                this.closeUpPreviewWindow.RedrawImage(this.scene, previewWorld3d);
            }
            this.scene.Draw(world3d);


            ImGui.Begin("Debug");

            ImGui.Checkbox("GPU Metrics", ref this.drawMetrics);

            if (this.drawMetrics)
            {
                ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.DrawCount)}: {this.GraphicsDevice.Metrics.DrawCount}");
                ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.ClearCount)}: {this.GraphicsDevice.Metrics.ClearCount}");
                ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.PrimitiveCount)}: {this.GraphicsDevice.Metrics.PrimitiveCount}");
            }

            ImGui.SliderFloat("Progress", ref this.progress, 0, 10);

            bool previewEnabled = this.closeUpPreviewWindow != null;
            if (ImGui.Checkbox("Close up preview", ref previewEnabled))
            {
                if (previewEnabled)
                {
                    this.closeUpPreviewWindow = new CloseUpPreviewWindow(this.imGuiRenderer, this.GraphicsDevice, this.previewWidth, this.previewHeight);
                }
                else if (this.closeUpPreviewWindow != null)
                {
                    this.closeUpPreviewWindow.Dispose();
                    this.closeUpPreviewWindow = null;
                }
            }

            if (this.closeUpPreviewWindow != null)
            {
                this.closeUpPreviewWindow.DrawImguiImage();
            }

            ImGui.End();

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
