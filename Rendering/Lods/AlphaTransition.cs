using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LodTransitions.Rendering.Lods
{
    public class AlphaTransition : ILodTransition
    {
        private readonly MainMaterial mainMaterial;

        public AlphaTransition(MainMaterial mainMaterial)
        {
            this.mainMaterial = mainMaterial;
        }

        public void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            var oldBlendState = graphicsDevice.BlendState;
            var oldDepthStencilState = graphicsDevice.DepthStencilState;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;

            this.mainMaterial.WorldViewProjection = transform * world.Camera.View.Matrix * world.Camera.Projection.Matrix;

            foreach (var part in start.Mesh.MeshParts)
            {
                this.mainMaterial.Progress = progress;
                this.mainMaterial.AlphaPass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
            foreach (var part in end.Mesh.MeshParts)
            {
                this.mainMaterial.Progress = 1f - progress;
                this.mainMaterial.AlphaPass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }

            graphicsDevice.BlendState = oldBlendState;
            graphicsDevice.DepthStencilState = oldDepthStencilState;
        }
    }
}
