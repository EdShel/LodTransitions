using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ImGuiNET;
using LodTransitions.Experiments;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using LodTransitions.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        private FpsCounter fpsCounter = new FpsCounter();

        public MyGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;

            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
        }

        private Effect axisShader;
        private PerspectiveLookAtTargetCamera camera;
        private PerspectiveLookAtTargetCamera previewCamera;


        private PoppingValueExperiment experiment;

        protected override void Initialize()
        {
            this.imGuiRenderer = new ImGuiRenderer(this);
            this.imGuiRenderer.RebuildFontAtlas();

            experiment = new PoppingValueExperiment(this);

            base.Initialize();
        }

        private Scene scene;

        private CloseUpPreviewWindow? closeUpPreviewWindow;

        protected override void LoadContent()
        {
            // this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            // this.axisShader = this.Content.Load<Effect>("axis_shader");
            // var mainShader = this.Content.Load<Effect>("main_shader");
            // var mainMaterial = new MainMaterial(mainShader);
            // var transition = new GeomorphTransition(mainMaterial);
            //var transition = new AlphaTransition(mainMaterial);
            //var transition = new NoiseTransition(mainMaterial);

            //var model = this.Content.Load<Model>("debug_tri");
            // var model = this.Content.Load<Model>("stanford-bunny");
            // var lodModel = LodModel.CreateWithAutomaticDistances(model, 8f);
            // var lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition, mainMaterial);

            // this.scene = new Scene();
            // this.scene.AddInstance(lodModelRenderer);
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


            // ImGui.Begin("Debug");

            // ImGui.Text($"FPS: {this.fpsCounter.Fps}");
            // ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.DrawCount)}: {this.GraphicsDevice.Metrics.DrawCount}");
            // ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.ClearCount)}: {this.GraphicsDevice.Metrics.ClearCount}");
            // ImGui.Text($"{nameof(this.GraphicsDevice.Metrics.PrimitiveCount)}: {this.GraphicsDevice.Metrics.PrimitiveCount}");

            // ImGui.SliderFloat("Progress", ref this.progress, 0, 10);

            // bool previewEnabled = this.closeUpPreviewWindow != null;
            // if (ImGui.Checkbox("Close up preview", ref previewEnabled))
            // {
            //     if (previewEnabled)
            //     {
            //         this.closeUpPreviewWindow = new CloseUpPreviewWindow(this.imGuiRenderer, this.GraphicsDevice, this.previewWidth, this.previewHeight);
            //     }
            //     else if (this.closeUpPreviewWindow != null)
            //     {
            //         this.closeUpPreviewWindow.Dispose();
            //         this.closeUpPreviewWindow = null;
            //     }
            // }

            // if (this.closeUpPreviewWindow != null)
            // {
            //     this.closeUpPreviewWindow.DrawImguiImage();
            // }

            // ImGui.End();

            experiment.Draw(gameTime);

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
