using LodTransitions.Rendering;
using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace LodTransitions.Experiments
{
    public class PoppingValueExperiment : IDisposable
    {
        private PoppingValueExperimentConfig config;
        private Scene scene;
        private RenderTarget2D renderTarget;
        private SpriteBatch? spriteBatch;

        private Vector3 movementAxis = new Vector3(0, 0, 1f);
        private PerspectiveLookAtTargetCamera camera;

        private int iteration = 0;

        private Color[]? previousFrame;
        private Color[]? currentFrame;

        public List<float> Results = new List<float>();
        public bool IsFinished { get; private set; }

        public PoppingValueExperiment(MyGame game, PoppingValueExperimentConfig config)
        {
            this.config = config;

            var content = game.Content;
            var graphicsDevice = game.GraphicsDevice;

            var mainShader = content.Load<Effect>("main_shader");
            var mainMaterial = new MainMaterial(mainShader);

            ILodTransition transition = config.Transition switch
            {
                LodTransitionKind.Alpha => new AlphaTransition(mainMaterial),
                LodTransitionKind.Noise => new NoiseTransition(mainMaterial),
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

        public void Dispose()
        {
            this.renderTarget.Dispose();
            this.spriteBatch?.Dispose();
            this.spriteBatch = null!;
        }

        public void Draw(GameTime gameTime)
        {
            if (IsFinished)
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
                DrawInBackBuffer(graphicsDevice);
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

        private void DrawInBackBuffer(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.SetRenderTarget(null);
            int windowWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int windowHeight = graphicsDevice.PresentationParameters.BackBufferHeight;

            float scaleX = ((float)windowWidth) / this.renderTarget.Width;
            float scaleY = ((float)windowHeight) / this.renderTarget.Height;
            float scale = Math.Min(scaleX, scaleY);

            float destWidth = this.renderTarget.Width * scale;
            float destHeight = this.renderTarget.Height * scale;
            float paddingLeft = (windowWidth - destWidth) / 2;
            float paddingTop = (windowHeight - destHeight) / 2;

            if (this.spriteBatch == null)
            {
                this.spriteBatch = new SpriteBatch(graphicsDevice);
            }

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(this.renderTarget, new Rectangle((int)paddingLeft, (int)paddingTop, (int)destWidth, (int)destHeight), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            this.spriteBatch.End();
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