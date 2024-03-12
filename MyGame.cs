using ImGuiNET;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
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

            base.Initialize();
        }

        private LodModelRenderer lodModelRenderer = null!;

        private Effect noiseShader;
        private Effect geomorphShader;
        private GeomorphedMesh geomorphedMesh;

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.axisShader = this.Content.Load<Effect>("axis_shader");
            this.noiseShader = this.Content.Load<Effect>("noise_shader");
            this.geomorphShader = this.Content.Load<Effect>("geomorph_shader");
            var model = this.Content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 8f);
            var transition = new GeomorphTransition(this.geomorphShader);
            this.lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition);
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

            this.camera.ViewConfig.Position = new Vector3(0, 0, 1f) * progress;

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

            this.lodModelRenderer.Draw(world3d);

            // this.lodTransition.Progress = progress;
            // this.lodTransition.Draw(Matrix.Identity, world3d);

            // foreach (var part in this.lodTransition.End.Mesh.MeshParts)
            // {
            //     GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
            //     GraphicsDevice.Indices = part.IndexBuffer;

            //     noiseShader.Parameters["Progress"].SetValue(progress);
            //     noiseShader.Parameters["Albedo"].SetValue(Color.Red.ToVector3());
            //     noiseShader.Parameters["WorldViewProjection"].SetValue(Matrix.CreateTranslation(-0.5f, 0.2f, 0) * this.camera.View.Matrix * this.camera.Projection.Matrix);
            //     noiseShader.CurrentTechnique.Passes[0].Apply();
            //     GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            // }

            // foreach (var part in this.lodTransition.End.Mesh.MeshParts)
            // {
            //     GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
            //     GraphicsDevice.Indices = part.IndexBuffer;

            //     noiseShader.Parameters["Progress"].SetValue(progress);
            //     noiseShader.Parameters["Albedo"].SetValue(Color.Yellow.ToVector3());
            //     noiseShader.Parameters["WorldViewProjection"].SetValue(this.camera.View.Matrix * this.camera.Projection.Matrix);
            //     noiseShader.CurrentTechnique.Passes[0].Apply();
            //     GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            // }

            // foreach (var part in this.geomorphedMesh.Parts)
            // {
            //     this.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
            //     this.GraphicsDevice.Indices = part.IndexBuffer;

            //     this.geomorphShader.Parameters["Progress"].SetValue(progress);
            //     this.geomorphShader.Parameters["WorldViewProjection"].SetValue(this.camera.View.Matrix * this.camera.Projection.Matrix);
            //     this.geomorphShader.CurrentTechnique.Passes[0].Apply();
            //     this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            // }

            this.imGuiRenderer.BeforeLayout(gameTime);

            ImGui.Begin("Debug");
            ImGui.SliderFloat("Progress", ref this.progress, 0, 10);

            ImGui.End();

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
