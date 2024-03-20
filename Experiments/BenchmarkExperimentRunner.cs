using ImGuiNET;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using LodTransitions.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LodTransitions.Experiments
{
    public class BenchmarkExperimentRunner : BaseRenderingExperiment
    {
        private int currentExperimentIndex = 0;
        private BenchmarkExperiment? currentExperiment;
        private Func<BenchmarkExperiment>[] expirementCreator;

        public BenchmarkExperimentRunner(MyGame game)
        {
            var baseConfig = new BenchmarkExperimentConfig
            (
                Transition: LodTransitionKind.Unknown,
                ImageWidth: 800,
                ImageHeight: 600,
                Model: "stanford-bunny",
                LowestLodDistance: 9,
                ObjectsCount: 0,
                Iterations: 60 * 60,
                WarmupIterations: 3 * 60
            );
            this.expirementCreator = new[]
            {
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Discrete,
                //    ObjectsCount = 50,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Alpha,
                //    ObjectsCount = 50,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Noise,
                //    ObjectsCount = 50,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Geomorphing,
                //    ObjectsCount = 50,
                //}),

                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Discrete,
                //    ObjectsCount = 250,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Alpha,
                //    ObjectsCount = 250,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Noise,
                //    ObjectsCount = 250,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Geomorphing,
                //    ObjectsCount = 250,
                //}),

                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Discrete,
                //    ObjectsCount = 500,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Alpha,
                //    ObjectsCount = 500,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Noise,
                //    ObjectsCount = 500,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Geomorphing,
                //    ObjectsCount = 500,
                //}),

                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Discrete,
                //    ObjectsCount = 1000,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Alpha,
                //    ObjectsCount = 1000,
                //}),
                () => new BenchmarkExperiment(game, baseConfig with {
                    Transition = LodTransitionKind.Noise,
                    ObjectsCount = 1000,
                }),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Geomorphing,
                //    ObjectsCount = 1000,
                //}),

                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Discrete,
                //    ObjectsCount = 2000,
                //}),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Alpha,
                //    ObjectsCount = 2000,
                //}),
                () => new BenchmarkExperiment(game, baseConfig with {
                    Transition = LodTransitionKind.Noise,
                    ObjectsCount = 2000,
                }),
                //() => new BenchmarkExperiment(game, baseConfig with {
                //    Transition = LodTransitionKind.Geomorphing,
                //    ObjectsCount = 2000,
                //}),
            };
        }

        public override void DisposeCore()
        {
        }

        private List<BenchmarkResult> Results = new List<BenchmarkResult>();

        public override void Draw(GameTime gameTime)
        {
            if (this.currentExperimentIndex >= this.expirementCreator.Length)
            {
                ImGui.Begin("Benchmark");
                ImGui.Text("Done!");
                ImGui.End();
                return;
            }

            if (this.currentExperiment == null)
            {
                this.currentExperiment = this.expirementCreator[this.currentExperimentIndex]();
                GC.Collect();
            }


            ImGui.Begin("Benchmark");
            ImGui.Text("Running #" + this.currentExperimentIndex);
            ImGui.End();

            this.currentExperiment.Draw(gameTime);

            if (this.currentExperiment.IsFinished)
            {
                this.Results.Add(new BenchmarkResult($"{this.currentExperiment}", this.currentExperiment.Results));
                this.currentExperiment.Dispose();
                this.currentExperiment = null;
                this.currentExperimentIndex++;

                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", this.Results.Select(r => r.Name)));

                int rows = this.Results[0].Time.Count;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < this.Results.Count; j++)
                    {
                        if (j != 0)
                        {
                            sb.Append(',');
                        }
                        sb.Append(this.Results[j].Time[i].ToStringWithDot());
                    }
                    sb.AppendLine();
                }
                System.IO.File.WriteAllText("./benchmark.csv", sb.ToString());

                if (this.currentExperimentIndex >= this.expirementCreator.Length)
                {
                    System.Diagnostics.Debug.WriteLine("Done!");

                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        private record BenchmarkResult(string Name, List<double> Time);
    }

    public class BenchmarkExperiment : BaseRenderingExperiment
    {
        private Scene scene;
        private PerspectiveLookAtTargetCamera camera;
        private RenderingPipeline pipeline;

        private BenchmarkExperimentConfig config;
        private int iterationsMade;
        private int warmupIterationsMade;

        private Stopwatch stopwatch = new Stopwatch();

        public BenchmarkExperiment(MyGame game, BenchmarkExperimentConfig config)
        {
            this.config = config;

            var content = game.Content;
            var graphicsDevice = game.GraphicsDevice;

            var mainShader = content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);

            ILodTransition? transition = config.Transition switch
            {
                LodTransitionKind.Discrete => null,
                LodTransitionKind.Alpha => new AlphaTransition(mainMaterial),
                LodTransitionKind.Noise => new NoiseTransition(mainMaterial, game.Content.Load<Texture2D>("dither")),
                LodTransitionKind.Geomorphing => new GeomorphTransition(mainMaterial),
                _ => throw new ArgumentException("Invalid LOD transition.", nameof(config.Transition))
            };

            var model = content.Load<Model>(config.Model);
            var lodModel = LodModel.CreateWithAutomaticDistances(model, config.LowestLodDistance);

            this.scene = new Scene();
            float startZ = 1f;
            float endZ = 20f;

            for (int i = 0; i < config.ObjectsCount; i++)
            {
                float z = (endZ - startZ) * ((float)i) / config.ObjectsCount;

                var lodModelRenderer = new LodModelRenderer(new Vector3(0, 0, z), lodModel, transition, mainMaterial);
                this.scene.AddInstance(lodModelRenderer);
            }

            this.camera = new PerspectiveLookAtTargetCamera(
                ViewConfig: new LookAtTargetCameraViewConfig
                {
                    LookAt = new Vector3(0, 0, endZ),
                    Position = Vector3.Zero,
                    Up = Vector3.Up,
                },
                ProjectionConfig: new PerspectiveCameraProjectionConfig
                {
                    Fov = MathHelper.PiOver2,
                    NearPlane = 0.001f,
                    FarPlane = 200f,
                    AspectRatio = ((float)config.ImageWidth) / config.ImageHeight,
                }
            );

            this.pipeline = new RenderingPipeline(graphicsDevice, new Point(config.ImageWidth, config.ImageHeight));
            this.pipeline.Camera = this.camera;
        }

        public bool IsFinished { get; private set; }
        public List<double> Results = new List<double>();

        public override void Draw(GameTime gameTime)
        {
            this.stopwatch.Restart();
            this.pipeline.RedrawMainTexture(this.scene);
            this.stopwatch.Stop();

            this.pipeline.PutOnScreen();

            this.warmupIterationsMade++;
            if (this.warmupIterationsMade < this.config.WarmupIterations)
            {
                return;
            }

            double renderTime = this.stopwatch.ElapsedTicks / 10_000d;
            this.Results.Add(renderTime);

            this.iterationsMade++;
            if (this.iterationsMade >= this.config.Iterations)
            {
                this.IsFinished = true;
            }
        }

        public override void DisposeCore()
        {
            this.pipeline.Dispose();
        }

        public override string? ToString()
        {
            return $"{this.config.Transition} {this.config.ObjectsCount}";
        }

    }

    public record BenchmarkExperimentConfig(
        LodTransitionKind Transition,
        string Model,
        float LowestLodDistance,
        int ImageWidth,
        int ImageHeight,
        int ObjectsCount,
        int Iterations,
        int WarmupIterations
    );
}
