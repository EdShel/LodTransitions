using Microsoft.Xna.Framework;

namespace LodTransitions.Rendering.Lods
{
    public interface ILodTransition
    {
        void Draw(float progress, LodLevel to, LodLevel from, Matrix transform, World3D world);
    }
}
