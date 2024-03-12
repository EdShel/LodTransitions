using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace LodTransitions.Rendering.Lods
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GeomorphVertex : IVertexType
    {
        public Vector3 StartPosition;
        public Vector3 StartNormal;
        public Vector3 EndPosition;
        public Vector3 EndNormal;

        public static VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => GeomorphVertex.VertexDeclaration;

        static GeomorphVertex()
        {
            VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
                new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
            );
        }

        public GeomorphVertex(Vector3 startPosition, Vector3 startNormal, Vector3 endPosition, Vector3 endNormal)
        {
            StartPosition = startPosition;
            StartNormal = startNormal;
            EndPosition = endPosition;
            EndNormal = endNormal;
        }
    }
}