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

    }
}