using ImGuiNET;
using LodTransitions.Cameras;
using LodTransitions.ImGuiRendering;
using LodTransitions.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace LodTransitions
{
    public class MyGame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private ImGuiRenderer imGuiRenderer;
        private float rotation = 0f;

        private DebugLookAroundCamera camera;

        public MyGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.camera = new DebugLookAroundCamera(this, position: new Vector3(0, 5f, -10f), rotation: Vector3.Zero);
        }

        private Effect axisShader;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.imGuiRenderer = new ImGuiRenderer(this);
            this.imGuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        private LodModelRenderer lodModelRenderer = null!;

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.axisShader = this.Content.Load<Effect>("axis_shader");
            var model = this.Content.Load<Model>("stanford-bunny");
            var lodModel = LodModel.CreateWithAutomaticDistances(model, 15f);
            this.lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.camera.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        private float cameraOffset = 1.0f;

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            this.cameraOffset += (float)gameTime.ElapsedGameTime.TotalSeconds * 6;
            this.cameraOffset = this.cameraOffset % 20f;

            //var view = this.camera.Transform;
            Vector3 cameraPosition = new Vector3(0, 0, 1f) * cameraOffset;
            var view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 0.01f, 200f);

            this.axisShader.Parameters["WorldViewProjection"].SetValue(view * proj);
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

            var mesh = this.lodModelRenderer.FindMesh(cameraPosition);
            foreach (BasicEffect meshEffect in mesh.Effects)
            {
                meshEffect.EnableDefaultLighting();
                meshEffect.PreferPerPixelLighting = true;
                meshEffect.World = this.lodModelRenderer.World;

                meshEffect.View = view;
                meshEffect.Projection = proj;
            }

            mesh.Draw();

            this.imGuiRenderer.BeforeLayout(gameTime);

            ImGui.Begin("Debug");
            ImGui.Text("Camera dist " + cameraPosition.Length());

            ImGui.End();

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
