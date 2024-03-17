using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering.Lods
{
    public class LodLevel
    {
        public long Id;
        public int LevelIndex;
        public ModelMesh Mesh = null!;
        public float DistanceSqr;
    }
}
