using ImGuiNET;
using ImGuizmoNET;
using ImTool.Scene3D;
using System;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
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
        protected Texture ActorIdTex;
        protected Texture StagingTex;
        protected IntPtr SceneTexBinding;
        protected IntPtr SceneTexBindingTest;
        protected CommandList CommandList;
        protected double LastFrameTime;
        protected double AvgFPS = 0f;

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
            ImGui.Image(SceneTexBindingTest, size / 4);
            ImGui.SetCursorPos(cursorPos);

            DrawOverlays(dt);
        }

        private void Init(Vector2 size)
        {
            if (SceneTex != null)
            {
                MainWindow.GetImGuiController().RemoveImGuiBinding(SceneTex);
                MainWindow.GetImGuiController().RemoveImGuiBinding(ActorIdTex);
            }

            SceneTex?.Dispose();
            DepthTex?.Dispose();
            ActorIdTex?.Dispose();
            StagingTex?.Dispose();
            FrameBuff?.Dispose();

            CommandList ??= GfxDevice.ResourceFactory.CreateCommandList();

            GfxDevice.WaitForIdle();

            SceneTex   = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            DepthTex   = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil));
            ActorIdTex = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R32_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
            StagingTex = GfxDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)size.X, (uint)size.Y, 1, 1, PixelFormat.R32_Float, TextureUsage.Staging));

            FrameBuff = GfxDevice.ResourceFactory.CreateFramebuffer(new FramebufferDescription()
            {
                DepthTarget = new FramebufferAttachmentDescription(DepthTex, 0),
                ColorTargets = new FramebufferAttachmentDescription[]
                {
                    new FramebufferAttachmentDescription(SceneTex, 0),
                    new FramebufferAttachmentDescription(ActorIdTex, 0)
                }
            });


            SceneTexBinding     = MainWindow.GetImGuiController().GetOrCreateImGuiBinding(GfxDevice.ResourceFactory, SceneTex);
            SceneTexBindingTest = MainWindow.GetImGuiController().GetOrCreateImGuiBinding(GfxDevice.ResourceFactory, ActorIdTex);

            NeedsToInit = false;
        }

        // Render intot he commandlist here
        public virtual void Render(double dt)
        {
            CommandList.ClearColorTarget(0, new RgbaFloat(0.69f, 0.61f, 0.85f, 1.0f));
            CommandList.ClearColorTarget(1, new RgbaFloat(-float.MaxValue, -float.NaN, -float.NaN, -float.NaN));
        }

        public SelectableID GetScreenSelectedId(Vector2? pos = null)
        {
            pos           = pos ?? ImGui.GetMousePos() - ImGui.GetWindowPos();

            var cmdList = GfxDevice.ResourceFactory.CreateCommandList();
            cmdList.Begin();
            cmdList.CopyTexture(ActorIdTex, StagingTex);
            cmdList.End();
            GfxDevice.SubmitCommands(cmdList);
            GfxDevice.WaitForIdle();
            cmdList.Dispose();

            var mappedTex = GfxDevice.Map(StagingTex, MapMode.Read);
            var texData   = new MappedResourceView<SelectableID>(mappedTex);
            var selId     = texData[(int)pos.Value.X, (int)pos.Value.Y];
            GfxDevice.Unmap(ActorIdTex);

            return selId;
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
