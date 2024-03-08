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

        public Matrix World => Matrix.CreateTranslation(this.Position);

        public ModelMesh FindMesh(Vector3 cameraPosition)
        {
            float distanceToCameraSqr = (this.Position - cameraPosition).LengthSquared();
            return this.LodModel.FindMesh(distanceToCameraSqr);
        }
    }

    public class LodModel
    {
        internal List<LodMesh> Lods { get; private set; }

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
                .Zip(model.Meshes, (lodId, mesh) => new LodMesh
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

        public ModelMesh FindMesh(float distanceSqr)
        {
            foreach (var lod in this.Lods)
            {
                if (distanceSqr >= lod.DistanceSqr)
                {
                    return lod.Mesh;
                }
            }

            return this.Lods.Last().Mesh;
        }
    }

    public class LodMesh
    {
        public int LodId;
        public ModelMesh Mesh;
        public float DistanceSqr;
    }
}
