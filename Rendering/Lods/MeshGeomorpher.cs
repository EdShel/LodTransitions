using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LodTransitions.Rendering.Lods
{
    public class MeshGeomorpher
    {
        public static GeomorphedMesh Create(ModelMesh lowDetail, ModelMesh highDetail, float normalContribution = 0.001f)
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

                Parallel.For(0, hdPositions.Length, j =>
                {
                    int closestLdPointIndex = FindClosestPointIndex(ldPositions, ldNormals, hdPositions[j], hdNormals[j], normalContribution);

                    geomorphPositions[j].StartPosition = hdPositions[j];
                    geomorphPositions[j].StartNormal = hdNormals[j];
                    geomorphPositions[j].EndPosition = ldPositions[closestLdPointIndex];
                    geomorphPositions[j].EndNormal = ldNormals[closestLdPointIndex];
                });

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

        private static int FindClosestPointIndex(Vector3[] positions, Vector3[] normals, Vector3 point, Vector3 normal, float normalContribution)
        {
            if (positions.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(positions));
            }
            if (normals.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(positions));
            }
            if (positions.Length != normals.Length)
            {
                throw new ArgumentException("Invalid arrays sizes.");
            }

            Vector3 firstPointNormal = normals[0];

            int minIndex = 0;
            float minDistSqr = VertexDistanceSqr(point, normal, positions[minIndex], normals[minIndex], normalContribution);
            for (int i = 1; i < positions.Length; i++)
            {
                float distSqr = VertexDistanceSqr(point, normal, positions[i], normals[i], normalContribution);
                if (distSqr < minDistSqr)
                {
                    minIndex = i;
                    minDistSqr = distSqr;
                }
            }

            return minIndex;
        }

        private static float VertexDistanceSqr(Vector3 pos1, Vector3 norm1, Vector3 pos2, Vector3 norm2, float normalContribution)
        {
            float px = pos1.X - pos2.X;
            float py = pos1.Y - pos2.Y;
            float pz = pos1.Z - pos2.Z;
            float nx = normalContribution * (norm1.X - norm2.X);
            float ny = normalContribution * (norm1.Y - norm2.Y);
            float nz = normalContribution * (norm1.Z - norm2.Z);

            return px * px
                + py * py
                + pz * pz
                + nx * nx
                + ny * ny
                + nz * nz;
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
}
