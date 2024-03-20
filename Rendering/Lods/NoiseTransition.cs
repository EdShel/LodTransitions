using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LodTransitions.Rendering.Lods
{
    public class NoiseTransition : ILodTransition, IDisposable
    {
        private readonly MainMaterial mainMaterial;
        private readonly Texture2D dither;
        private readonly RasterizerState rasterizerState;

        public NoiseTransition(MainMaterial mainMaterial, Texture2D dither)
        {
            this.mainMaterial = mainMaterial;
            this.dither = dither;
            this.rasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
            };
        }

        public void Draw(float progress, LodLevel to, LodLevel from, Matrix transform, RenderingPipeline pipeline)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            graphicsDevice.RasterizerState = this.rasterizerState;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.mainMaterial.WorldViewProjection = transform * pipeline.Camera.View.Matrix * pipeline.Camera.Projection.Matrix;
            this.mainMaterial.Effect.Parameters["NoiseTexture"].SetValue(this.dither);
            this.mainMaterial.Effect.Parameters["NoiseTextureScale"].SetValue(new Vector2(16f, 16f));
            this.mainMaterial.Effect.Parameters["ScreenSize"].SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
            this.mainMaterial.Progress = progress;

            foreach (var part in from.Mesh.MeshParts)
            {
                this.mainMaterial.Effect.Parameters["InvertNoiseTexture"].SetValue(false);
                this.mainMaterial.NoisePass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
            foreach (var part in to.Mesh.MeshParts)
            {
                this.mainMaterial.Effect.Parameters["InvertNoiseTexture"].SetValue(true);
                this.mainMaterial.NoisePass.Apply();
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }

        public void Dispose()
        {
            this.rasterizerState.Dispose();
        }
    }
}
