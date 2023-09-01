using ImGuiNET;
using ImGuizmoNET;
using ImTool.Scene3D;
using System;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Veldrid;
using Veldrid.OpenGLBinding;

namespace ImTool
{
    // A scene widget to render a commandlist to a framebuffer in ImGUI
    public class SceneWidget<T> where T : FrameBufferResourceBase, new()
    {
        protected Window MainWindow;
        protected GraphicsDevice GfxDevice;
        protected T FrameBufferResource;

        protected IntPtr SceneTexBinding;
        protected CommandList CommandList;
        protected double LastFrameTime;
        protected double AvgFPS = 0f;

        private bool NeedsToInit = true;

        public GraphicsDevice GetGfxDevice() => GfxDevice;
        public Framebuffer GetFramebuffer()  => FrameBufferResource.FrameBuffer;
        public bool IsHovered                = false;

        public SceneWidget(Window win)
        {
            MainWindow          = win;
            GfxDevice           = win.GetGraphicsDevice();
            LastFrameTime       = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);
            FrameBufferResource = new ();

            Init(new Vector2(256, 256));
        }

        public void DrawWindow(string title)
        {
            if (ImGui.Begin(title))
            {
                var size = ImGui.GetContentRegionAvail();
                size.X   = size.X <= 0 ? 1 : size.X;
                size.Y   = size.Y <= 0 ? 1 : size.Y;

                Draw(size);
            }

            ImGui.End();
        }

        public virtual void Draw(Vector2 size)
        {
            if (NeedsToInit || size.X != FrameBufferResource.FrameBuffer.Width || size.Y != FrameBufferResource.FrameBuffer.Height)
                Init(size);

            IsHovered = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + size);

            CommandList.Begin();
            CommandList.SetFramebuffer(FrameBufferResource.FrameBuffer);

            double dt = (DateTime.UtcNow.Ticks - LastFrameTime) / TimeSpan.TicksPerSecond;
            CalcFPS(dt);
            Render(dt);
            LastFrameTime = DateTime.UtcNow.Ticks;

            //CommandList.CopyTexture(ActorIdTex, StagingTex);
            CommandList.End();
            GfxDevice.SubmitCommands(CommandList);
            GfxDevice.WaitForIdle();

            var cursorPos = ImGui.GetCursorPos();
            ImGui.Image(SceneTexBinding, size);

            ImGui.SetCursorPos(cursorPos);

            DrawOverlays(dt);
        }

        protected virtual void Init(Vector2 size)
        {
            if (FrameBufferResource.SceneTex != null)
            {
                MainWindow.GetImGuiController().RemoveImGuiBinding(FrameBufferResource.SceneTex);
            }

            CommandList ??= GfxDevice.ResourceFactory.CreateCommandList();

            FrameBufferResource.InitFramebuffer((uint)size.X, (uint)size.Y);
            SceneTexBinding     = MainWindow.GetImGuiController().GetOrCreateImGuiBinding(GfxDevice.ResourceFactory, FrameBufferResource.SceneTex);

            NeedsToInit = false;
        }

        // Render intot he commandlist here
        public virtual void Render(double dt)
        {
            CommandList.ClearColorTarget(0, new RgbaFloat(0.69f, 0.61f, 0.85f, 1.0f));
        }

        public virtual void DrawOverlays(double dt)
        {
            ImGui.Text($"Delta Time: {dt:0.#####}, FPS: {AvgFPS:0}");
        }

        private void CalcFPS(double dt)
        {
            var expSmoothing = 0.9f;
            AvgFPS = expSmoothing * AvgFPS + (1f - expSmoothing) * 1f / dt;
        }
    }
}
