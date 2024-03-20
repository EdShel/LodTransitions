using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace LodTransitions.Rendering.Lods
{
    public class LodModelRenderer : IRenderable
    {
        private float transitionThreshold = 0.4f;
        private MainMaterial mainMaterial;

        public Vector3 Position { get; set; }
        public LodModel LodModel { get; private set; }
        public float TransitionThreshold
        {
            get => this.transitionThreshold;
            set
            {
                if (value <= 0 || value >= 1f)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                this.transitionThreshold = value;
            }
        }
        public ILodTransition? Transition { get; set; }

        public LodModelRenderer(Vector3 position, LodModel lodModel, ILodTransition? transition, MainMaterial mainMaterial)
        {
            this.Position = position;
            this.LodModel = lodModel;
            this.Transition = transition;
            this.mainMaterial = mainMaterial;
        }

        public void Draw(RenderingPipeline pipeline)
        {
            float distanceToCameraSqr = (this.Position - pipeline.ObserverPosition).LengthSquared();

            for (int i = 0; i < this.LodModel.Lods.Count - 1; i++)
            {
                LodLevel lod = this.LodModel.Lods[i];
                if (distanceToCameraSqr >= lod.DistanceSqr)
                {
                    DrawSimpleLod(pipeline, lod);
                    return;
                }

                if (this.Transition == null)
                {
                    // Discrete LOD
                    continue;
                }

                LodLevel nextLod = this.LodModel.Lods[i + 1];
                float distanceToNextLod = lod.DistanceSqr - nextLod.DistanceSqr;
                float transitionDistanceSqr = distanceToNextLod * this.transitionThreshold;
                float transitionStart = lod.DistanceSqr - transitionDistanceSqr;
                if (distanceToCameraSqr > transitionStart)
                {
                    float progress = (distanceToCameraSqr - transitionStart) / transitionDistanceSqr;
                    this.Transition.Draw(progress, lod, nextLod, Matrix.CreateTranslation(this.Position), pipeline);
                    return;
                }
            }

            DrawSimpleLod(pipeline, this.LodModel.Lods.Last());
        }

        private void DrawSimpleLod(RenderingPipeline pipeline, LodLevel lod)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            foreach (ModelMeshPart part in lod.Mesh.MeshParts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;

                this.mainMaterial.WorldViewProjection = Matrix.CreateTranslation(this.Position) * pipeline.Camera.View.Matrix * pipeline.Camera.Projection.Matrix;
                this.mainMaterial.MainPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }
}
