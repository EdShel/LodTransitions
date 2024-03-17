using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering.Lods
{
    public class NoiseTransition : ILodTransition
    {
        private readonly MainMaterial mainMaterial;

        public NoiseTransition(MainMaterial mainMaterial)
        {
            this.mainMaterial = mainMaterial;
        }

        public void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            this.mainMaterial.WorldViewProjection = transform * world.Camera.View.Matrix * world.Camera.Projection.Matrix;

            foreach (var part in start.Mesh.MeshParts)
            {
                this.mainMaterial.Progress = 1f - progress;
                this.mainMaterial.NoisePass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
            foreach (var part in end.Mesh.MeshParts)
            {
                this.mainMaterial.Progress = progress;
                this.mainMaterial.NoisePass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }
}
