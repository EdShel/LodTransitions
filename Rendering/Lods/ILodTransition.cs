using Microsoft.Xna.Framework;

namespace LodTransitions.Rendering.Lods
{
    public interface ILodTransition
    {
        void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world);
    }
}
