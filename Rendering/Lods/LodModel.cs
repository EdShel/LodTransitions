using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LodTransitions.Rendering.Lods
{
    public class LodModelRenderer
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
        public ILodTransition Transition { get; set; }

        public LodModelRenderer(Vector3 position, LodModel lodModel, ILodTransition transition, MainMaterial mainMaterial)
        {
            this.Position = position;
            this.LodModel = lodModel;
            this.Transition = transition;
            this.mainMaterial = mainMaterial;
        }

        public void Draw(World3D world)
        {
            float distanceToCameraSqr = (this.Position - world.ObserverPosition).LengthSquared();

            for (int i = 0; i < this.LodModel.Lods.Count - 1; i++)
            {
                LodLevel lod = this.LodModel.Lods[i];
                if (distanceToCameraSqr >= lod.DistanceSqr)
                {
                    DrawSimpleLod(world, lod);
                    return;
                }

                LodLevel nextLod = this.LodModel.Lods[i + 1];
                float distanceToNextLod = lod.DistanceSqr - nextLod.DistanceSqr;
                float transitionDistanceSqr = distanceToNextLod * this.transitionThreshold;
                float transitionStart = lod.DistanceSqr - transitionDistanceSqr;
                if (distanceToCameraSqr > transitionStart)
                {
                    float progress = (distanceToCameraSqr - transitionStart) / transitionDistanceSqr;
                    this.Transition.Draw(progress, lod, nextLod, Matrix.CreateTranslation(this.Position), world);
                    return;
                }
            }

            DrawSimpleLod(world, this.LodModel.Lods.Last());
        }

        private void DrawSimpleLod(World3D world, LodLevel lod)
        {
            var graphicsDevice = this.mainMaterial.Effect.GraphicsDevice;
            foreach (ModelMeshPart part in lod.Mesh.MeshParts)
            {
                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;

                this.mainMaterial.WorldViewProjection = Matrix.CreateTranslation(this.Position) * world.Camera.View.Matrix * world.Camera.Projection.Matrix;
                this.mainMaterial.MainPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }

    public interface ILodTransitionFactory
    {
        ILodTransition CreateTransition(LodLevel start, LodLevel end, float progress = 0.0f);
    }

    public interface ILodTransition
    {
        void Draw(float progress, LodLevel start, LodLevel end, Matrix transform, World3D world);
    }


    // public class LodTransition : ILodTransition
    // {
    //     public LodLevel Start { get; set; }
    //     public LodLevel End { get; set; }
    //     public float Progress { get; set; }

    //     public LodTransition(LodLevel start, LodLevel end, float progress = 0.0f)
    //     {
    //         Start = start;
    //         End = end;
    //         Progress = progress;
    //     }

    //     public void Draw(Matrix transform, World3D world)
    //     {
    //         BlendState oldBlend = world.Graphics.BlendState;
    //         DepthStencilState oldDepth = world.Graphics.DepthStencilState;
    //         world.Graphics.BlendState = BlendState.AlphaBlend;
    //         world.Graphics.DepthStencilState = DepthStencilState.None;

    //         foreach (BasicEffect meshEffect in this.Start.Mesh.Effects)
    //         {
    //             meshEffect.EnableDefaultLighting();
    //             meshEffect.PreferPerPixelLighting = true;
    //             meshEffect.World = transform;
    //             meshEffect.View = world.Camera.View.Matrix;
    //             meshEffect.Projection = world.Camera.Projection.Matrix;

    //             meshEffect.Alpha = Math.Clamp(1f - this.Progress, 0f, 1f);
    //         }
    //         this.Start.Mesh.Draw();

    //         foreach (BasicEffect meshEffect in this.End.Mesh.Effects)
    //         {
    //             meshEffect.EnableDefaultLighting();
    //             meshEffect.PreferPerPixelLighting = true;
    //             meshEffect.World = transform;
    //             meshEffect.View = world.Camera.View.Matrix;
    //             meshEffect.Projection = world.Camera.Projection.Matrix;

    //             meshEffect.Alpha = Math.Clamp(this.Progress, 0f, 1f);
    //         }
    //         this.End.Mesh.Draw();

    //         world.Graphics.BlendState = oldBlend;
    //         world.Graphics.DepthStencilState = oldDepth;
    //     }
    // }

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

    public class DisposableLruCache<TKey, TVal>
        where TKey : notnull
        where TVal : IDisposable
    {
        private Dictionary<TKey, CacheEntry> cache = new Dictionary<TKey, CacheEntry>();

        public int maxCapacity;

        public DisposableLruCache(int maxCapacity)
        {
            if (maxCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            }
            this.maxCapacity = maxCapacity;
        }

        public TVal GetOrCreate(TKey key, Func<TVal> creator)
        {
            long time = DateTime.Now.Ticks;
            if (this.cache.TryGetValue(key, out CacheEntry? cacheEntry))
            {
                cacheEntry.LastUsedTime = time;
                return cacheEntry.Value;
            }

            while (this.cache.Count >= this.maxCapacity)
            {
                var lruEntry = this.cache.MinBy(v => v.Value.LastUsedTime);
                this.cache.Remove(lruEntry.Key);
                lruEntry.Value.Value.Dispose();
            }

            TVal newValue = creator();
            this.cache[key] = new CacheEntry(time, newValue);
            return newValue;
        }

        private class CacheEntry
        {
            public long LastUsedTime;
            public TVal Value;

            public CacheEntry(long lastUsedTime, TVal value)
            {
                this.LastUsedTime = lastUsedTime;
                this.Value = value;
            }
        }
    }

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

    public class LodLevel
    {
        public long Id;
        public int LevelIndex;
        public ModelMesh Mesh;
        public float DistanceSqr;
    }
}
