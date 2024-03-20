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

        public void Draw(float progress, LodLevel to, LodLevel from, Matrix transform, RenderingPipeline pipeline)
        {
            var cacheEntry = (to.Id, from.Id);
            GeomorphedMesh geomorphedMesh = this.geomorphCache.GetOrCreate(cacheEntry, () => MeshGeomorpher.Create(to.Mesh, from.Mesh));

            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            foreach (var part in geomorphedMesh.Parts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;

                this.mainMaterial.Progress = progress;
                this.mainMaterial.WorldViewProjection = transform * pipeline.Camera.View.Matrix * pipeline.Camera.Projection.Matrix;
                this.mainMaterial.GeomorphPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }
}
