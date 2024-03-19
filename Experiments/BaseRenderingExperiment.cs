using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LodTransitions.Experiments
{
    public abstract class BaseRenderingExperiment : IDisposable
    {
        private SpriteBatch? spriteBatch;

        public void Dispose()
        {
            DisposeCore();

            spriteBatch?.Dispose();
            spriteBatch = null;
        }

        public abstract void Draw(GameTime gameTime);

        public abstract void DisposeCore();

        protected void DrawInBackBuffer(GraphicsDevice graphicsDevice, Texture2D texture)
        {
            graphicsDevice.SetRenderTarget(null);
            int windowWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int windowHeight = graphicsDevice.PresentationParameters.BackBufferHeight;

            float scaleX = ((float)windowWidth) / texture.Width;
            float scaleY = ((float)windowHeight) / texture.Height;
            float scale = Math.Min(scaleX, scaleY);

            float destWidth = texture.Width * scale;
            float destHeight = texture.Height * scale;
            float paddingLeft = (windowWidth - destWidth) / 2;
            float paddingTop = (windowHeight - destHeight) / 2;

            if (this.spriteBatch == null)
            {
                this.spriteBatch = new SpriteBatch(graphicsDevice);
            }

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(texture, new Rectangle((int)paddingLeft, (int)paddingTop, (int)destWidth, (int)destHeight), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            this.spriteBatch.End();
        }
    }
}