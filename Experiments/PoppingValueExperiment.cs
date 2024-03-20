using ImGuiNET;
using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using LodTransitions.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LodTransitions.Experiments
{
    public class PoppingValueExperiment : BaseRenderingExperiment
    {
        private PoppingValueExperimentStage[] experiments;
        private int currentExperiment;

        private FpsCounter fpsCounter = new FpsCounter();

        private List<List<float>> results = new List<List<float>>();

        private GraphicsDevice graphicsDevice;

        public PoppingValueExperiment(MyGame game)
        {
            this.graphicsDevice = game.GraphicsDevice;
            this.experiments = new[] {
                new PoppingValueExperimentStage(game, new PoppingValueExperimentConfig
                {
                    Transition = LodTransitionKind.Alpha,
                    Model = "stanford-bunny",
                    ImageWidth = this.graphicsDevice.PresentationParameters.BackBufferWidth,
                    ImageHeight = this.graphicsDevice.PresentationParameters.BackBufferHeight,
                    StartDistance = 0.5f,
                    FinishDistance = 10f,
                    LowestLodDistance = 9f,
                    SnapshotIterations = 1000,
                    OutputToBackBuffer = true,
                }),
                new PoppingValueExperimentStage(game, new PoppingValueExperimentConfig
                {
                    Transition = LodTransitionKind.Noise,
                    Model = "stanford-bunny",
                    ImageWidth = this.graphicsDevice.PresentationParameters.BackBufferWidth,
                    ImageHeight = this.graphicsDevice.PresentationParameters.BackBufferHeight,
                    StartDistance = 0.5f,
                    FinishDistance = 10f,
                    LowestLodDistance = 9f,
                    SnapshotIterations = 1000,
                    OutputToBackBuffer = true,
                }),
                new PoppingValueExperimentStage(game, new PoppingValueExperimentConfig
                {
                    Transition = LodTransitionKind.Geomorphing,
                    Model = "stanford-bunny",
                    ImageWidth = this.graphicsDevice.PresentationParameters.BackBufferWidth,
                    ImageHeight = this.graphicsDevice.PresentationParameters.BackBufferHeight,
                    StartDistance = 0.5f,
                    FinishDistance = 10f,
                    LowestLodDistance = 9f,
                    SnapshotIterations = 1000,
                    OutputToBackBuffer = true,
                })
            };
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.currentExperiment < this.experiments.Length)
            {
                var experiment = this.experiments[this.currentExperiment];
                experiment.Draw(gameTime);
                this.fpsCounter.Tick(gameTime.ElapsedGameTime);

                if (experiment.IsFinished)
                {
                    this.results.Add(experiment.Results);
                    this.currentExperiment++;

                    if (this.currentExperiment >= this.experiments.Length)
                    {
                        var sb = new StringBuilder("Alpha,Noise,Geomorphing");
                        int els = this.results[0].Count;
                        for (int i = 0; i < els; i++)
                        {
                            for (int j = 0; j < this.results.Count; j++)
                            {
                                if (j != 0)
                                {
                                    sb.Append(',');
                                }
                                sb.Append(this.results[j][i].ToStringWithDot());
                            }
                            sb.AppendLine();
                        }
                        System.IO.File.WriteAllText("./result.csv", sb.ToString());
                    }
                }
            }

            ImGui.Begin("Debug");

            ImGui.Text($"FPS: {this.fpsCounter.Fps}");
            ImGui.Text($"{nameof(this.graphicsDevice.Metrics.DrawCount)}: {this.graphicsDevice.Metrics.DrawCount}");
            ImGui.Text($"{nameof(this.graphicsDevice.Metrics.ClearCount)}: {this.graphicsDevice.Metrics.ClearCount}");
            ImGui.Text($"{nameof(this.graphicsDevice.Metrics.PrimitiveCount)}: {this.graphicsDevice.Metrics.PrimitiveCount}");

            ImGui.End();
        }

        public override void DisposeCore()
        {
            foreach (var experiment in this.experiments)
            {
                experiment.Dispose();
            }
        }
    }

    public class PoppingValueExperimentStage : BaseRenderingExperiment
    {
        private PoppingValueExperimentConfig config;
        private Scene scene;
        private RenderTarget2D renderTarget;

        private Vector3 movementAxis = new Vector3(0, 0, 1f);
        private PerspectiveLookAtTargetCamera camera;

        private int iteration = 0;

        private Color[]? previousFrame;
        private Color[]? currentFrame;

        public List<float> Results = new List<float>();
        public bool IsFinished { get; private set; }

        public PoppingValueExperimentStage(MyGame game, PoppingValueExperimentConfig config)
        {
            this.config = config;

            var content = game.Content;
            var graphicsDevice = game.GraphicsDevice;

            var mainShader = content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);

            ILodTransition transition = config.Transition switch
            {
                LodTransitionKind.Alpha => new AlphaTransition(mainMaterial),
                LodTransitionKind.Noise => new NoiseTransition(mainMaterial, game.Content.Load<Texture2D>("dither")),
                LodTransitionKind.Geomorphing => new GeomorphTransition(mainMaterial),
                _ => throw new ArgumentException("Invalid LOD transition.", nameof(config.Transition))
            };

            var model = content.Load<Model>(config.Model);
            var lodModel = LodModel.CreateWithAutomaticDistances(model, config.LowestLodDistance);
            var lodModelRenderer = new LodModelRenderer(Vector3.Zero, lodModel, transition, mainMaterial);

            this.scene = new Scene();
            this.scene.AddInstance(lodModelRenderer);

            this.renderTarget = new RenderTarget2D(graphicsDevice, config.ImageWidth, config.ImageHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);

            this.camera = new PerspectiveLookAtTargetCamera(
                ViewConfig: new LookAtTargetCameraViewConfig
                {
                    LookAt = Vector3.Zero,
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
        }

        public override void DisposeCore()
        {
            this.renderTarget.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.IsFinished)
            {
                throw new InvalidOperationException("The experiment is already finished.");
            }

            var graphicsDevice = this.renderTarget.GraphicsDevice;

            this.camera.ViewConfig.Position = this.movementAxis * (this.config.StartDistance + (this.config.FinishDistance - this.config.StartDistance) / this.config.SnapshotIterations * this.iteration);
            this.iteration++;

            this.IsFinished = this.iteration >= this.config.SnapshotIterations;
            if (this.IsFinished)
            {
                System.Diagnostics.Debug.WriteLine("Done");
            }

            graphicsDevice.SetRenderTarget(this.renderTarget);
            var world = new World3D
            {
                Graphics = graphicsDevice,
                Dt = (float)gameTime.ElapsedGameTime.TotalSeconds,
                Camera = this.camera,
            };
            this.scene.Draw(world);

            if (this.currentFrame == null)
            {
                this.currentFrame = new Color[this.config.ImageWidth * this.config.ImageHeight];
            }
            this.renderTarget.GetData(this.currentFrame);

            if (this.previousFrame != null)
            {
                float difference = FindDifferenceValue(this.currentFrame, this.previousFrame);
                this.Results.Add(difference);
                System.Diagnostics.Debug.WriteLine(difference);
            }
            var t = this.previousFrame;
            this.previousFrame = this.currentFrame;
            this.currentFrame = t;

            if (this.config.OutputToBackBuffer)
            {
                DrawInBackBuffer(graphicsDevice, this.renderTarget);
            }
        }

        private float FindDifferenceValue(Color[] a, Color[] b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Pixel buffers have different size.");
            }
            float sum = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                Vector3 aRgb = a[i].ToVector3();
                Vector3 bRgb = b[i].ToVector3();

                sum += (aRgb - bRgb).LengthSquared();
            }

            return MathF.Sqrt(sum);
        }

    }

    public class PoppingValueExperimentConfig
    {
        public LodTransitionKind Transition { get; set; }
        public string Model { get; set; } = null!;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public float StartDistance { get; set; }
        public float FinishDistance { get; set; }
        public float LowestLodDistance { get; set; }
        public int SnapshotIterations { get; set; }
        public bool OutputToBackBuffer { get; set; }
    }
}