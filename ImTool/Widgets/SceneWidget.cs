using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Numerics;
using Veldrid;

namespace ImTool
{
    // A scene widget to render a commandlist to a framebuffer in ImGUI
    public class SceneWidget
    {
        protected Window MainWindow;
        protected GraphicsDevice GfxDevice;
        protected Framebuffer FrameBuff;
        protected Texture SceneTex;
        protected Texture DepthTex;
        protected IntPtr SceneTexBinding;
        protected CommandList CommandList;
        protected double LastFrameTime;

        private bool NeedsToInit = true;

        public GraphicsDevice GetGfxDevice() => GfxDevice;
        public Framebuffer GetFramebuffer()  => FrameBuff;
        public bool IsHovered                = false;

        public SceneWidget(Window win)
        {
            MainWindow = win;
            GfxDevice = win.GetGraphicsDevice();
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

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
            if (NeedsToInit || size.X != FrameBuff.Width || size.Y != FrameBuff.Height)
                Init(size);

            IsHovered = ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + size);

            CommandList.Begin();
            CommandList.SetFramebuffer(FrameBuff);

            double dt = (DateTime.UtcNow.Ticks - LastFrameTime) / TimeSpan.TicksPerSecond;
            Render(dt);
            LastFrameTime = DateTime.UtcNow.Ticks;

            CommandList.End();
            GfxDevice.SubmitCommands(CommandList);
            GfxDevice.WaitForIdle();

            var cursorPos = ImGui.GetCursorPos();
            ImGui.Image(SceneTexBinding, size);

            ImGui.SetCursorPos(cursorPos);
            DrawOverlays(dt);
        }

        private void Init(Vector2 size)
        {
            if (SceneTex != null)
                MainWindow.GetImGuiController().RemoveImGuiBinding(SceneTex);

            SceneTex?.Dispose();
            DepthTex?.Dispose();
            FrameBuff?.Dispose();

            CommandList ??= GfxDevice.ResourceFactory.CreateCommandList();

            GfxDevice.WaitForIdle();

            SceneTex = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            DepthTex = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R16_UNorm, TextureUsage.DepthStencil));

            FrameBuff = GfxDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription()
            {
                DepthTarget = new FramebufferAttachmentDescription(DepthTex, 0),
                ColorTargets = new FramebufferAttachmentDescription[]
                {
                    new FramebufferAttachmentDescription(SceneTex, 0)
                }
            });


            SceneTexBinding = MainWindow.GetImGuiController().GetOrCreateImGuiBinding(GfxDevice.ResourceFactory, SceneTex);

            NeedsToInit = false;
        }

        // Render intot he commandlist here
        public virtual void Render(double dt)
        {
            CommandList.ClearColorTarget(0, new RgbaFloat(0.69f, 0.61f, 0.85f, 1.0f));
        }

        public virtual void DrawOverlays(double dt)
        {
            ImGui.Text($"Delta Time: {dt:0.#####}");
        }
    }
}
