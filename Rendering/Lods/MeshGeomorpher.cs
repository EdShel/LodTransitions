using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LodTransitions.Rendering.Lods
{
    public class MeshGeomorpher
    {
        public static GeomorphedMesh Create(ModelMesh lowDetail, ModelMesh highDetail)
        {
            if (lowDetail.MeshParts.Count != highDetail.MeshParts.Count)
            {
                throw new ArgumentException("Meshes must have equal number of parts.");
            }

            var geomorphedParts = new List<GeomorphedMeshPart>();

            for (int i = 0; i < highDetail.MeshParts.Count; i++)
            {
                ModelMeshPart highDetailPart = highDetail.MeshParts[i];
                ModelMeshPart lowDetailPart = lowDetail.MeshParts[i];
                Vector3[] hdPositions = GetVertexPositions(highDetailPart);
                Vector3[] hdNormals = GetVertexNormals(highDetailPart);
                Vector3[] ldPositions = GetVertexPositions(lowDetailPart);
                Vector3[] ldNormals = GetVertexNormals(lowDetailPart);

                GeomorphVertex[] geomorphPositions = new GeomorphVertex[highDetailPart.NumVertices];
                for (int j = 0; j < hdPositions.Length; j++)
                {
                    int closestLdPointIndex = FindClosestPointIndex(ldPositions, hdPositions[j]);

                    geomorphPositions[j].StartPosition = hdPositions[j];
                    geomorphPositions[j].StartNormal = hdNormals[j];
                    geomorphPositions[j].EndPosition = ldPositions[closestLdPointIndex];
                    geomorphPositions[j].EndNormal = ldNormals[closestLdPointIndex];
                }

                GraphicsDevice graphicsDevice = highDetailPart.VertexBuffer.GraphicsDevice;
                var geomorphVertexBuffer = new VertexBuffer(graphicsDevice, GeomorphVertex.VertexDeclaration, geomorphPositions.Length, BufferUsage.WriteOnly);
                geomorphVertexBuffer.SetData(geomorphPositions);

                uint[] geomorphIndices = CopyIndexBufferAsUnsigned32(highDetailPart.IndexBuffer, highDetailPart.StartIndex, highDetailPart.PrimitiveCount * 3, highDetailPart.VertexOffset);
                var geomorphIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, geomorphIndices.Length, BufferUsage.WriteOnly);
                geomorphIndexBuffer.SetData(geomorphIndices);

                var part = new GeomorphedMeshPart()
                {
                    VertexBuffer = geomorphVertexBuffer,
                    IndexBuffer = geomorphIndexBuffer,
                    PrimitiveCount = highDetailPart.PrimitiveCount,
                    StartIndex = 0,
                    VertexOffset = 0,
                };
                geomorphedParts.Add(part);
            }

            return new GeomorphedMesh(geomorphedParts);
        }

        private static Vector3[] GetVertexPositions(ModelMeshPart meshPart)
        {
            VertexDeclaration vertexInfo = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexAttributes = vertexInfo.GetVertexElements();
            VertexElement positionAttribute = vertexAttributes
                .Cast<VertexElement?>()
                .FirstOrDefault(a => a!.Value.VertexElementUsage == VertexElementUsage.Position)
                ?? throw new ArgumentException("Vertex buffer is missing POSITION attribute.");

            Vector3[] positions = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData(positionAttribute.Offset, positions, meshPart.VertexOffset, positions.Length, vertexInfo.VertexStride);
            return positions;
        }

        private static Vector3[] GetVertexNormals(ModelMeshPart meshPart)
        {
            VertexDeclaration vertexInfo = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexAttributes = vertexInfo.GetVertexElements();
            VertexElement normalAttribute = vertexAttributes
                .Cast<VertexElement?>()
                .FirstOrDefault(a => a!.Value.VertexElementUsage == VertexElementUsage.Normal)
                ?? throw new ArgumentException("Vertex buffer is missing NORMAL attribute.");

            Vector3[] normals = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData(normalAttribute.Offset, normals, meshPart.VertexOffset, normals.Length, vertexInfo.VertexStride);
            return normals;
        }

        private static int FindClosestPointIndex(Vector3[] array, Vector3 point)
        {
            if (array.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(array));
            }
            int minIndex = 0;
            float minDistSqr = (array[minIndex] - point).LengthSquared();
            for (int i = 1; i < array.Length; i++)
            {
                float distSqr = (array[i] - point).LengthSquared();
                if (distSqr < minDistSqr)
                {
                    minIndex = i;
                    minDistSqr = distSqr;
                }
            }

            return minIndex;
        }

        private static uint[] CopyIndexBufferAsUnsigned32(IndexBuffer indexBuffer, int startIndex, int count, int baseVertex)
        {
            if (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
                ushort[] shortIndices = new ushort[count];
                indexBuffer.GetData(0, shortIndices, startIndex, count);

                uint[] indices = new uint[count];

                checked
                {
                    for (int i = 0; i < indices.Length; i++)
                    {
                        indices[i] = shortIndices[i] - (uint)baseVertex;
                    }
                }

                return indices;
            }

            if (indexBuffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
            {
                uint[] indices = new uint[count];
                indexBuffer.GetData(0, indices, startIndex, count);

                if (baseVertex > 0)
                {
                    checked
                    {
                        for (int i = 0; i < indices.Length; i++)
                        {
                            indices[i] -= (uint)baseVertex;
                        }
                    }
                }

                return indices;
            }

            throw new ArgumentException("Unknown index buffer format", nameof(indexBuffer));
        }
    }

    public class GeomorphedMesh
    {
        public IReadOnlyList<GeomorphedMeshPart> Parts { get; set; }

        public GeomorphedMesh(IReadOnlyList<GeomorphedMeshPart> parts)
        {
            this.Parts = parts;
        }
    }

    public class GeomorphedMeshPart
    {
        public VertexBuffer VertexBuffer { get; set; } = null!;
        public IndexBuffer IndexBuffer { get; set; } = null!;
        public int VertexOffset { get; set; }
        public int StartIndex { get; set; }
        public int PrimitiveCount { get; set; }
    }

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