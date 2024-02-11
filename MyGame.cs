using ImGuiNET;
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

        public MyGame()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            var model = this.Content.Load<Model>("castle");

            var sb = new StringBuilder();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect meshEffect in mesh.Effects)
                {
                    meshEffect.EnableDefaultLighting();
                    meshEffect.PreferPerPixelLighting = true;
                    meshEffect.World = Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateRotationZ(this.rotation);
                    this.rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;

                    var view = Matrix.CreateLookAt(new Vector3(0f, 10f, 3f), Vector3.Zero, new Vector3(0, 0, 1f));
                    var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 1, 200);

                    meshEffect.View = view;
                    meshEffect.Projection = proj;

                    sb.AppendLine(meshEffect.Name ?? "<null>");
                }

                mesh.Draw();
            }

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
