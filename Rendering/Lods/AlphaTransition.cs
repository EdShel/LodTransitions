using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LodTransitions.Rendering.Lods
{
    public class AlphaTransition : ILodTransition, IDisposable
    {
        private readonly MainMaterial mainMaterial;
        private readonly RasterizerState rasterizerState;
        private readonly BlendState blendDepthOnly;

        public AlphaTransition(MainMaterial mainMaterial)
        {
            this.mainMaterial = mainMaterial;
            this.rasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
            };
            this.blendDepthOnly = new BlendState
            {
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.One
            };
        }

        public void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            var oldBlendState = graphicsDevice.BlendState;
            var oldDepthStencilState = graphicsDevice.DepthStencilState;

            graphicsDevice.RasterizerState = this.rasterizerState;

            this.mainMaterial.WorldViewProjection = transform * world.Camera.View.Matrix * world.Camera.Projection.Matrix;
            DrawTransparentModelDoublePass(start, graphicsDevice, progress);
            DrawTransparentModelDoublePass(end, graphicsDevice, 1f - progress);
   
            graphicsDevice.BlendState = oldBlendState;
            graphicsDevice.DepthStencilState = oldDepthStencilState;
        }

        private void DrawTransparentModelDoublePass(LodLevel lod, GraphicsDevice graphicsDevice, float alpha)
        {
            graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = this.blendDepthOnly;
            this.mainMaterial.AlphaPass.Apply();
            foreach (var part in lod.Mesh.MeshParts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
            var startBlendState = new BlendState
            {
                ColorSourceBlend = Blend.BlendFactor,
                ColorDestinationBlend = Blend.InverseBlendFactor,
                BlendFactor = new Color(alpha, alpha, alpha, alpha),
                ColorBlendFunction = BlendFunction.Add,
            };
            graphicsDevice.BlendState = startBlendState;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            this.mainMaterial.MainPass.Apply();
            foreach (var part in lod.Mesh.MeshParts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }

        public void Dispose()
        {
            this.rasterizerState.Dispose();
            this.blendDepthOnly.Dispose();
        }
    }
}
