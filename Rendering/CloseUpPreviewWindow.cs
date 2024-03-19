using ImGuiNET;
using LodTransitions.ImGuiRendering;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LodTransitions.Rendering
{
    public class CloseUpPreviewWindow : IDisposable
    {
        private readonly RenderTarget2D renderTarget;
        private readonly ImGuiRenderer imGuiRenderer;

        private readonly IntPtr renderTargetImguiHandle;

        public CloseUpPreviewWindow(ImGuiRenderer imGuiRenderer, GraphicsDevice device, int width, int height)
        {
            this.imGuiRenderer = imGuiRenderer;
            this.renderTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);

            this.renderTargetImguiHandle = this.imGuiRenderer.BindTexture(this.renderTarget);
        }

        public void DrawImguiImage()
        {
            ImGui.Image(this.renderTargetImguiHandle, new System.Numerics.Vector2(this.renderTarget.Width, this.renderTarget.Height));
        }

        public void RedrawImage(Scene scene, World3D world)
        {
            var graphicsDevice = world.Graphics;
            var oldRenderTargets = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTarget(this.renderTarget);
            scene.Draw(world);
            graphicsDevice.SetRenderTargets(oldRenderTargets);
        }

        public void Dispose()
        {
            this.imGuiRenderer.UnbindTexture(this.renderTargetImguiHandle);
            this.renderTarget.Dispose();
        }
    }
}
