using LodTransitions.Rendering.Lods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace LodTransitions.Rendering
{
    public class Scene : IRenderable, IDisposable
    {
        private List<IRenderable> renderables = new List<IRenderable>();

        public Color SkyColor { get; set; } = Color.CornflowerBlue;

        public void AddInstance(IRenderable renderable)
        {
#if DEBUG
            if (this.renderables.Contains(renderable))
            {
                throw new ArgumentException("The renderable is already in the scene.");
            }
#endif

            this.renderables.Add(renderable);
        }

        public void RemoveInstance(IRenderable renderable)
        {
            bool removed = this.renderables.Remove(renderable);
#if DEBUG
            if (!removed)
            {
                throw new ArgumentException("The renderable isn't in the scene.");
            }
#endif
        }

        private RenderTarget2D? transparentRenderTarget;
        private SpriteBatch? spriteBatch;

        public void Draw(RenderingPipeline pipeline)
        {
            pipeline.Graphics.Clear(this.SkyColor);

            foreach (var renderable in this.renderables)
            {
                renderable.Draw(pipeline);
            }
        }

        public void Dispose()
        {
            this.transparentRenderTarget?.Dispose();
        }
    }
}
