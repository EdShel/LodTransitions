using System;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering.Lods
{
    public class GeomorphedMeshPart : IDisposable
    {
        public VertexBuffer VertexBuffer { get; set; } = null!;
        public IndexBuffer IndexBuffer { get; set; } = null!;
        public int VertexOffset { get; set; }
        public int StartIndex { get; set; }
        public int PrimitiveCount { get; set; }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}