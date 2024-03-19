using ImGuiNET;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using LodTransitions.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LodTransitions.Experiments
{
    public class DebugExperiment : BaseRenderingExperiment
    {
        private Scene scene;
        private RenderTarget2D renderTarget;
        private PerspectiveLookAtTargetCamera camera;
        private PerspectiveLookAtTargetCamera previewCamera;

        private float distance = 0.5f;
        private float previewDistance = 0.75f;

        private FpsCounter fpsCounter = new FpsCounter();
        private CloseUpPreviewWindow? closeUpPreviewWindow;

        private ImGuiRenderer imGuiRenderer;

        private int previewWidth = 400;
        private int previewHeight = 400;

        public DebugExperiment(MyGame game)
        {
            this.imGuiRenderer = game.ImGuiRenderer;

            var graphicsDevice = game.GraphicsDevice;
            var content = game.Content;

            var mainShader = content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);

            var transitionKind = LodTransitionKind.Alpha;
            ILodTransition transition = transitionKind switch
            {
                LodTransitionKind.Alpha => new AlphaTransition(mainMaterial),
                LodTransitionKind.Noise => new NoiseTransition(mainMaterial),
                LodTransitionKind.Geomorphing => new GeomorphTransition(mainMaterial),
                _ => throw new ArgumentException("Invalid LOD transition.")
            };

            //var model = content.Load<Model>("cube");
            var model = content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 9);
            var lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition, mainMaterial);

            this.scene = new Scene();
            scene.SkyColor = Color.Red;
            this.scene.AddInstance(lodModelRenderer);

            // DepthFormat depthFormat = transitionKind == LodTransitionKind.Alpha ? DepthFormat.None : DepthFormat.Depth16;
            this.renderTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

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
                    AspectRatio = ((float)this.renderTarget.Width) / this.renderTarget.Height,
                }
            );

            this.previewCamera = new PerspectiveLookAtTargetCamera(
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
                    AspectRatio = ((float)this.renderTarget.Width) / this.renderTarget.Height,
                }
            );
        }

        public override void Draw(GameTime gameTime)
        {
            var graphicsDevice = this.renderTarget.GraphicsDevice;

            graphicsDevice.SetRenderTarget(this.renderTarget);
            this.camera.ViewConfig.Position = new Vector3(0, 0, 1f) * this.distance;
            var world = new World3D
            {
                Graphics = graphicsDevice,
                Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                Camera = this.camera,
            };

            if (this.closeUpPreviewWindow != null)
            {
                this.previewCamera.ViewConfig.Position = new Vector3(1f, 0, 0) * this.previewDistance;
                var previewWorld3d = new World3D
                {
                    Graphics = graphicsDevice,
                    Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                    Camera = this.previewCamera,
                    DebugObserverPosition = this.camera.View.Position
                };
                this.closeUpPreviewWindow.RedrawImage(this.scene, previewWorld3d);
            }

            this.scene.Draw(world);

            DrawInBackBuffer(graphicsDevice, this.renderTarget);

            ImGui.Begin("Debug");

            ImGui.Text($"FPS: {this.fpsCounter.Fps}");
            ImGui.Text($"{nameof(graphicsDevice.Metrics.DrawCount)}: {graphicsDevice.Metrics.DrawCount}");
            ImGui.Text($"{nameof(graphicsDevice.Metrics.ClearCount)}: {graphicsDevice.Metrics.ClearCount}");
            ImGui.Text($"{nameof(graphicsDevice.Metrics.PrimitiveCount)}: {graphicsDevice.Metrics.PrimitiveCount}");

            ImGui.SliderFloat("Distance", ref this.distance, 0, 10);

            bool previewEnabled = this.closeUpPreviewWindow != null;
            if (ImGui.Checkbox("Close up preview", ref previewEnabled))
            {
                if (previewEnabled)
                {
                    this.closeUpPreviewWindow = new CloseUpPreviewWindow(this.imGuiRenderer, graphicsDevice, this.previewWidth, this.previewHeight);
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

        }

        public override void DisposeCore()
        {
            this.renderTarget.Dispose();
        }
    }
}
