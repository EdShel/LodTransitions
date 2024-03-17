using LodTransitions.Rendering.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering
{
    public class World3D
    {
        public GraphicsDevice Graphics { get; set; } = null!;
        public ICamera Camera { get; set; } = null!;
        public float Dt { get; set; }
        public Vector3? DebugObserverPosition { get; set; }

        public Vector3 ObserverPosition => this.DebugObserverPosition ?? this.Camera.View.Position;
    }
}