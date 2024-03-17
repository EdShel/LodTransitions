using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering.Lods
{
    public class GeomorphTransition : ILodTransition
    {
        private DisposableLruCache<(long, long), GeomorphedMesh> geomorphCache = new(maxCapacity: 10);

        private MainMaterial mainMaterial;

        public GeomorphTransition(MainMaterial mainMaterial)
        {
            this.mainMaterial = mainMaterial;
        }

        public void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world)
        {
            var cacheEntry = (start.Id, end.Id);
            GeomorphedMesh geomorphedMesh = this.geomorphCache.GetOrCreate(cacheEntry, () => MeshGeomorpher.Create(start.Mesh, end.Mesh));

            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            foreach (var part in geomorphedMesh.Parts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;

                this.mainMaterial.Progress = progress;
                this.mainMaterial.WorldViewProjection = transform * world.Camera.View.Matrix * world.Camera.Projection.Matrix;
                this.mainMaterial.GeomorphPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }
}
