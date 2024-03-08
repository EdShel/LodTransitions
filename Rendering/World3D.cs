using LodTransitions.Rendering.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering
{
    public class World3D
    {
        public GraphicsDevice Graphics { get; set; }
        public ICamera Camera { get; set; }
        public float Dt { get; set; }

        public Vector3 ObserverPosition => Camera.View.Position;
    }
}