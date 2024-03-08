using ImGuiNET;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
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

        public MyGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        private Effect axisShader;
        private PerspectiveLookAtTargetCamera camera;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.imGuiRenderer = new ImGuiRenderer(this);
            this.imGuiRenderer.RebuildFontAtlas();

            camera = new PerspectiveLookAtTargetCamera(
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

            base.Initialize();
        }

        private LodModelRenderer lodModelRenderer = null!;
        private LodTransition lodTransition;

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.axisShader = this.Content.Load<Effect>("axis_shader");
            var model = this.Content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 15f);
            this.lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel);
            this.lodTransition = new LodTransition
            {
                Start = lodModel.Lods[0],
                End = lodModel.Lods[1],
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        private float cameraOffset = 1.0f;

        private float progress;

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            var world3d = new World3D
            {
                Camera = this.camera,
                Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                Graphics = this.GraphicsDevice
            };

            this.camera.ViewConfig.Position = new Vector3(0, 0, 1f) * 1.5f;

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

            this.lodTransition.Progress = progress;
            this.lodTransition.Draw(Matrix.Identity, world3d);

            this.imGuiRenderer.BeforeLayout(gameTime);

            ImGui.Begin("Debug");
            ImGui.SliderFloat("Progress", ref progress, 0, 1);

            ImGui.End();

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
