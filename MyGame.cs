using ImGuiNET;
using LodTransitions.Cameras;
using LodTransitions.ImGuiRendering;
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
            this.camera = new DebugLookAroundCamera(this, position: new Vector3(0, 0.1f, 0), rotation: Vector3.Zero);
        }

        private Effect axisShader;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.imGuiRenderer = new ImGuiRenderer(this);
            this.imGuiRenderer.RebuildFontAtlas();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.axisShader = Content.Load<Effect>("axis_shader");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            this.camera.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            var model = this.Content.Load<Model>("castle");

            var sb = new StringBuilder();

            var view = this.camera.Transform;
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 1, 200);

            this.axisShader.Parameters["WorldViewProjection"].SetValue(view * proj);
            this.axisShader.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new[]
            {
                new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
                new VertexPositionColor(new Vector3(1f, 0, 0), Color.Red),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Green),
                new VertexPositionColor(new Vector3(0, 1f, 0), Color.Green),

                new VertexPositionColor(new Vector3(0, 0, 0), Color.Blue),
                new VertexPositionColor(new Vector3(0, 0, 1f), Color.Blue),
            }, 0, 3);

            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (BasicEffect meshEffect in mesh.Effects)
            //    {
            //        meshEffect.EnableDefaultLighting();
            //        meshEffect.PreferPerPixelLighting = true;
            //        meshEffect.World = Matrix.CreateTranslation(0, 0, 0);
            //        this.rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;

            //        var view = this.camera.Transform;
            //        var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 1, 200);

            //        meshEffect.View = view;
            //        meshEffect.Projection = proj;

            //        sb.AppendLine(mesh.Name ?? "<null>");
            //    }

            //    mesh.Draw();
            //}

            this.imGuiRenderer.BeforeLayout(gameTime);

            ImGui.Begin("Debug");

            var result = sb.ToString();
            ImGui.Text(string.IsNullOrEmpty(result) ? "N/A" : result);

            ImGui.End();

            this.imGuiRenderer.AfterLayout();

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }
    }
}
