using LodTransitions.Rendering.Lods;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LodTransitions.Rendering
{
    public class Scene : IRenderable
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

        public void Draw(World3D world)
        {
            world.Graphics.Clear(this.SkyColor);

            foreach (var renderable in this.renderables)
            {
                renderable.Draw(world);
            }
        }
    }
}
