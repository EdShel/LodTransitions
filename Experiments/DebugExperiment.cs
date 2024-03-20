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
        private RenderingPipeline screenRenderer;
        private Scene scene;
        private PerspectiveLookAtTargetCamera camera;
        private PerspectiveLookAtTargetCamera? previewCamera;

        private float distance = 0.5f;
        private float previewDistance = 1f;

        private FpsCounter fpsCounter = new FpsCounter();
        private CloseUpPreviewWindow? closeUpPreviewWindow;

        private ImGuiRenderer imGuiRenderer;

        private int previewWidth = 200;
        private int previewHeight = 200;

        public DebugExperiment(MyGame game)
        {
            this.imGuiRenderer = game.ImGuiRenderer;

            var graphicsDevice = game.GraphicsDevice;
            var content = game.Content;

            var mainShader = content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);

            var transitionKind = LodTransitionKind.Alpha;
            ILodTransition? transition = transitionKind switch
            {
                LodTransitionKind.Discrete => null,
                LodTransitionKind.Alpha => new AlphaTransition(mainMaterial),
                LodTransitionKind.Noise => new NoiseTransition(mainMaterial, game.Content.Load<Texture2D>("dither")),
                LodTransitionKind.Geomorphing => new GeomorphTransition(mainMaterial),
                _ => throw new ArgumentException("Invalid LOD transition.")
            };

            //var model = content.Load<Model>("cube");
            var model = content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 9);
            var lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition, mainMaterial);

            this.scene = new Scene();
            this.scene.SkyColor = Color.CornflowerBlue;
            this.scene.AddInstance(lodModelRenderer);

            this.screenRenderer = new RenderingPipeline(graphicsDevice, null);
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
                    AspectRatio = ((float)this.screenRenderer.Width) / this.screenRenderer.Height,
                }
            );
            this.screenRenderer.Camera = this.camera;
        }

        public override void Draw(GameTime gameTime)
        {
            var graphicsDevice = this.screenRenderer.Graphics;

            this.camera.ViewConfig.Position = new Vector3(0, 0, 1f) * this.distance;
            this.screenRenderer.RedrawMainTexture(this.scene);

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
                    var previewRenderer = new RenderingPipeline(graphicsDevice, new Point(this.previewWidth, this.previewHeight));
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
                            AspectRatio = ((float)previewRenderer.Width) / previewRenderer.Height,
                        }
                    );
                    previewRenderer.Camera = this.previewCamera;
                    this.closeUpPreviewWindow = new CloseUpPreviewWindow(this.imGuiRenderer, previewRenderer);
                }
                else if (this.closeUpPreviewWindow != null)
                {
                    this.closeUpPreviewWindow.Dispose();
                    this.closeUpPreviewWindow = null;
                    this.previewCamera = null;
                }
            }

            if (this.closeUpPreviewWindow != null && this.previewCamera != null)
            {
                this.previewCamera.ViewConfig.Position = new Vector3(0, 0, 1f) * this.previewDistance;
                this.closeUpPreviewWindow.Pipeline.DebugObserverPosition = this.camera.View.Position;
                this.closeUpPreviewWindow.Redraw(this.scene);
            }
            this.screenRenderer.PutOnScreen();

            ImGui.End();
        }

        public override void DisposeCore()
        {
            this.screenRenderer.Dispose();
            this.closeUpPreviewWindow?.Dispose();
        }
    }
}
