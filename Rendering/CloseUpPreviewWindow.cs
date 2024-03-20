using ImGuiNET;
using LodTransitions.ImGuiRendering;
using System;

namespace LodTransitions.Rendering
{
    public class CloseUpPreviewWindow : IDisposable
    {
        private readonly RenderingPipeline pipeline;
        private readonly ImGuiRenderer imGuiRenderer;

        private readonly IntPtr renderTargetImguiHandle;

        public RenderingPipeline Pipeline => pipeline;

        public CloseUpPreviewWindow(ImGuiRenderer imGuiRenderer, RenderingPipeline pipeline)
        {
            this.imGuiRenderer = imGuiRenderer;
            this.pipeline = pipeline;

            this.renderTargetImguiHandle = this.imGuiRenderer.BindTexture(this.pipeline.MainTexture);
        }

        public void Redraw(Scene scene)
        {
            this.pipeline.RedrawMainTexture(scene);
            ImGui.Image(this.renderTargetImguiHandle, new System.Numerics.Vector2(this.pipeline.Width, this.pipeline.Height));
        }

        public void Dispose()
        {
            this.imGuiRenderer.UnbindTexture(this.renderTargetImguiHandle);
            this.pipeline.Dispose();
        }
    }
}
