using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class FrameBufferResourceBase
    {
        public OutputDescription OutputDescription { get; protected set; }
        public Framebuffer FrameBuffer { get; protected set; }
        public Texture SceneTex { get; protected set; }
        public Texture DepthTex { get; protected set; }

        public FrameBufferResourceBase()
        {

        }

        public virtual void InitFramebuffer(uint width, uint height)
        {
            SceneTex?.Dispose();
            DepthTex?.Dispose();
            FrameBuffer?.Dispose();

            Resources.GD.WaitForIdle();

            SceneTex = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            DepthTex = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));

            FrameBuffer = Resources.GD.ResourceFactory.CreateFramebuffer(new FramebufferDescription()
            {
                DepthTarget = new FramebufferAttachmentDescription(DepthTex, 0),
                ColorTargets = new FramebufferAttachmentDescription[]
                {
                    new FramebufferAttachmentDescription(SceneTex, 0),
                }
            });
        }
    }

    public class MainFrameBufferResource : FrameBufferResourceBase
    {
        public Texture ActorIdTex { get; protected set; }
        public Texture SelectedMaskTex { get; protected set; }

        public override void InitFramebuffer(uint width, uint height)
        {
            SceneTex?.Dispose();
            DepthTex?.Dispose();
            ActorIdTex?.Dispose();
            SelectedMaskTex?.Dispose();
            FrameBuffer?.Dispose();

            Resources.GD.WaitForIdle();

            SceneTex        = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
            DepthTex        = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));
            ActorIdTex      = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R32_UInt, TextureUsage.RenderTarget | TextureUsage.Sampled));
            SelectedMaskTex = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R8_UInt, TextureUsage.RenderTarget | TextureUsage.Sampled));

            FrameBuffer = Resources.GD.ResourceFactory.CreateFramebuffer(new FramebufferDescription()
            {
                DepthTarget = new FramebufferAttachmentDescription(DepthTex, 0),
                ColorTargets = new FramebufferAttachmentDescription[]
                {
                    new FramebufferAttachmentDescription(SceneTex, 0),
                    new FramebufferAttachmentDescription(ActorIdTex, 0),
                    new FramebufferAttachmentDescription(SelectedMaskTex, 0),
                }
            });
        }

        public SelectableID GetScreenSelectedId(Vector2? pos = null)
        {
            pos         = pos ?? ImGui.GetMousePos() - ImGui.GetWindowPos();
            uint posX   = (uint)pos.Value.X;
            uint posY   = (uint)pos.Value.Y;
            uint width  = 1;
            uint height = 1;

            try
            {
                var stagingTex = Resources.GD.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R32_UInt, TextureUsage.Staging));
                Resources.GD.WaitForIdle();

                var cmdList = Resources.GD.ResourceFactory.CreateCommandList();
                cmdList.Begin();
                cmdList.CopyTexture(ActorIdTex, posX, posY, 0, 0, 0, stagingTex, 0, 0, 0, 0, 0, width, height, 1, 1);
                cmdList.End();
                Resources.GD.SubmitCommands(cmdList);
                Resources.GD.WaitForIdle();
                cmdList.Dispose();

                var mappedTex = Resources.GD.Map(stagingTex, MapMode.Read);
                var texData = new MappedResourceView<SelectableID>(mappedTex);
                var selId = texData[0, 0];
                Resources.GD.Unmap(stagingTex);

                stagingTex.Dispose();

                return selId;
            }
            catch (Exception ex)
            {
                return new SelectableID(SelectableID.NO_ID_VALUE, 0);
            }
        }
    }
}
