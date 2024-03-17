using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LodTransitions.Rendering.Lods
{
    public class LodModel
    {
        /// <summary>
        /// Returns LODs starting from the least detailed (the furthest).
        /// </summary>
        internal List<LodLevel> Lods { get; private set; }

        private static Regex lodMeshPattern = new Regex(@"\.LOD(\d+)$");

        private static long nextLodId = 1;

        private LodModel(List<LodLevel> lods)
        {
            this.Lods = lods;
        }

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
                .Zip(model.Meshes, (levelIndex, mesh) => new LodLevel
                {
                    Id = nextLodId++,
                    LevelIndex = levelIndex,
                    Mesh = mesh,
                    DistanceSqr = MathF.Pow(maxDistance * levelIndex / (model.Meshes.Count - 1f), 2),
                })
                .OrderByDescending(m => m.LevelIndex)
                .ToList();


            return new LodModel(lods);
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
}
