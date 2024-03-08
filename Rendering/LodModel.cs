using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LodTransitions.Rendering
{
    public class LodModelRenderer
    {
        public Vector3 Position { get; set; }
        public LodModel LodModel { get; private set; }

        public LodModelRenderer(Vector3 position, LodModel lodModel)
        {
            this.Position = position;
            this.LodModel = lodModel;
        }

        public void Draw(World3D world)
        {
            float distanceToCameraSqr = (this.Position - world.ObserverPosition).LengthSquared();
            LodLevel currentLevel = this.LodModel.FindLodLevel(distanceToCameraSqr);

        }
    }


    public class LodTransition
    {
        public LodLevel Start { get; set; }
        public LodLevel End { get; set; }
        public float Progress { get; set; }

        public void Draw(Matrix transform, World3D world)
        {
            BlendState oldBlend = world.Graphics.BlendState;
            DepthStencilState oldDepth = world.Graphics.DepthStencilState;
            world.Graphics.BlendState = BlendState.AlphaBlend;
            world.Graphics.DepthStencilState = DepthStencilState.None;

            foreach (BasicEffect meshEffect in this.Start.Mesh.Effects)
            {
                meshEffect.EnableDefaultLighting();
                meshEffect.PreferPerPixelLighting = true;
                meshEffect.World = transform;
                meshEffect.View = world.Camera.View.Matrix;
                meshEffect.Projection = world.Camera.Projection.Matrix;

                meshEffect.Alpha = Math.Clamp(1f - this.Progress, 0f, 1f);
            }
            this.Start.Mesh.Draw();

            foreach (BasicEffect meshEffect in this.End.Mesh.Effects)
            {
                meshEffect.EnableDefaultLighting();
                meshEffect.PreferPerPixelLighting = true;
                meshEffect.World = transform;
                meshEffect.View = world.Camera.View.Matrix;
                meshEffect.Projection = world.Camera.Projection.Matrix;

                meshEffect.Alpha = Math.Clamp(this.Progress, 0f, 1f);
            }
            this.End.Mesh.Draw();

            world.Graphics.BlendState = oldBlend;
            world.Graphics.DepthStencilState = oldDepth;
        }
    }

    public class LodModel
    {
        internal List<LodLevel> Lods { get; private set; }

        private static Regex lodMeshPattern = new Regex(@"\.LOD(\d+)$");

        public static LodModel CreateWithAutomaticDistances(Model model, float maxDistance)
        {
            if (model.Meshes.Count < 2)
            {
                throw new ArgumentException("Model must have at least 2 meshes.");
            }

            var lodMatches = model.Meshes.Select(m => lodMeshPattern.Match(m.Name)).ToList();
            var improperlyNamedMeshes = lodMatches.Where(m => !m.Success).ToList();
            if (improperlyNamedMeshes.Count > 0)
            {
                throw new ArgumentException($"Model meshes must be suffixed with LOD identifier: {string.Join(", ", improperlyNamedMeshes.Select(m => m.Value))}.");
            }

            var lods = lodMatches
                .Select(m => int.Parse(m.Groups[1].ValueSpan))
                .Zip(model.Meshes, (lodId, mesh) => new LodLevel
                {
                    LodId = lodId,
                    Mesh = mesh,
                    DistanceSqr = MathF.Pow(maxDistance * lodId / (model.Meshes.Count - 1f), 2),
                })
                .OrderByDescending(m => m.LodId)
                .ToList();


            return new LodModel
            {
                Lods = lods,
            };
        }

        public LodLevel FindLodLevel(float distanceSqr)
        {
            foreach (var lod in this.Lods)
            {
                if (distanceSqr >= lod.DistanceSqr)
                {
                    return lod;
                }
            }

            return this.Lods.Last();
        }
    }

    public class LodLevel
    {
        public int LodId;
        public ModelMesh Mesh;
        public float DistanceSqr;
    }
}
