using Microsoft.Xna.Framework;

namespace LodTransitions.Rendering.Cameras
{
    public interface ICameraViewConfig
    {
        Vector3 Position { get; }
        Matrix Matrix { get; }
    }
}
