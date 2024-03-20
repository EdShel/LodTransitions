using LodTransitions.Rendering.Cameras;
using LodTransitions.Rendering.Lods;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace LodTransitions.Rendering
{
    public class RenderingPipeline : IDisposable
    {
        public GraphicsDevice Graphics { get; private set; }
        public ICamera Camera { get; set; } = null!;
        public Vector3? DebugObserverPosition { get; set; }

        public Vector3 ObserverPosition => this.DebugObserverPosition ?? this.Camera.View.Position;

        private List<ITransparentRenderable> defferedTransparents = new List<ITransparentRenderable>();

        private RenderTarget2D mainRenderTarget;
        private RenderTarget2D? transparentRenderTarget;
        private SpriteBatch? spriteBatch;

        public Texture2D MainTexture => mainRenderTarget;
        public int Width => mainRenderTarget.Width;
        public int Height => mainRenderTarget.Height;

        public RenderingPipeline(GraphicsDevice graphics, Point? resolution)
        {
            this.Graphics = graphics;
            Point size = resolution ?? new Point(Graphics.Viewport.Width, Graphics.Viewport.Height);
            this.mainRenderTarget = new RenderTarget2D(Graphics, size.X, size.Y, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
        }

        public void DefferTransparentRendering(ITransparentRenderable renderable)
        {
            this.defferedTransparents.Add(renderable);
        }

        public void RedrawMainTexture(Scene scene)
        {
            Graphics.SetRenderTarget(this.mainRenderTarget);

            scene.Draw(this);

            if (defferedTransparents.Count > 0)
            {
                DoTransparentPass(defferedTransparents);
                defferedTransparents.Clear();
            }
        }

        private void DoTransparentPass(IEnumerable<ITransparentRenderable> transparents)
        {
            if (this.transparentRenderTarget == null)
            {
                this.transparentRenderTarget = new RenderTarget2D(Graphics, this.mainRenderTarget.Width, this.mainRenderTarget.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            }

            Graphics.SetRenderTarget(this.transparentRenderTarget);
            Graphics.Clear(new Color(0, 0, 0, 0));

            foreach (var transparent in transparents)
            {
                transparent.DrawTransparent(this);
            }

            Graphics.SetRenderTargets(this.mainRenderTarget);

            if (this.spriteBatch == null)
            {
                this.spriteBatch = new SpriteBatch(Graphics);
            }

            this.spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            this.spriteBatch.Draw(this.transparentRenderTarget, new Vector2(0, 0), null, Color.White);
            this.spriteBatch.End();
        }

        public void PutOnScreen()
        {
            DrawInBackBuffer(Graphics, this.mainRenderTarget);
        }

        private void DrawInBackBuffer(GraphicsDevice graphicsDevice, Texture2D texture)
        {
            graphicsDevice.SetRenderTarget(null);
            int windowWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int windowHeight = graphicsDevice.PresentationParameters.BackBufferHeight;

            float scaleX = ((float)windowWidth) / texture.Width;
            float scaleY = ((float)windowHeight) / texture.Height;
            float scale = Math.Min(scaleX, scaleY);

            float destWidth = texture.Width * scale;
            float destHeight = texture.Height * scale;
            float paddingLeft = (windowWidth - destWidth) / 2;
            float paddingTop = (windowHeight - destHeight) / 2;

            if (this.spriteBatch == null)
            {
                this.spriteBatch = new SpriteBatch(graphicsDevice);
            }

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(texture, new Rectangle((int)paddingLeft, (int)paddingTop, (int)destWidth, (int)destHeight), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            this.spriteBatch.End();
        }

        public void Dispose()
        {
            mainRenderTarget?.Dispose();
            transparentRenderTarget?.Dispose();
            spriteBatch?.Dispose();
        }
    }
}