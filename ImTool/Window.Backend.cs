using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImTool
{
    public partial class Window
    {
        private bool floatersAllowed;
        
        public static readonly IReadOnlyList<GraphicsBackend> SupportedGraphicsBackends = GetSupportedGraphicsBackends();
        private static GraphicsBackend[] GetSupportedGraphicsBackends()
        {
            List<GraphicsBackend> supported = new List<GraphicsBackend>();

            foreach (GraphicsBackend backend in Enum.GetValues<GraphicsBackend>())
            {
                if(IsGraphicsBackendSupported(backend))
                    supported.Add(backend);
            }

            return supported.ToArray();
        }

        public static bool IsGraphicsBackendSupported(GraphicsBackend backend) => backend != GraphicsBackend.OpenGLES && GraphicsDevice.IsBackendSupported(backend);

        public static GraphicsBackend GetDefaultGraphicsBackend()
        {
            if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
                return GraphicsBackend.Vulkan;

            if (OperatingSystem.IsWindows)
                return GraphicsBackend.Direct3D11;

            if (OperatingSystem.IsMacOS && GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
                return GraphicsBackend.Metal;
            
            return GraphicsBackend.OpenGL;
        }
        
        
        private void SetupGD()
        {
            if(controller != null)
                controller.Dispose();
            
            if(commandList != null)
                commandList.Dispose();
            
            if(graphicsDevice != null)
                graphicsDevice.Dispose();

            if (window != null)
            {
                window.Close();
                window = null;
            }

            if (!IsGraphicsBackendSupported(config.GraphicsBackend))
            {
                config.GraphicsBackend = GetDefaultGraphicsBackend();
                config.Save();
            }


            floatersAllowed = !config.DisableFloatingWindows && config.AllowFloatingWindows && config.GraphicsBackend != GraphicsBackend.OpenGL;
            string iniFile = Path.Combine(config.ToolDataPath, "Config", AppDomain.CurrentDomain.FriendlyName + ".ImGui.ini");
            SDL_WindowFlags flags =  SDL_WindowFlags.Shown | SDL_WindowFlags.Borderless | (config.GraphicsBackend == GraphicsBackend.OpenGL ? SDL_WindowFlags.OpenGL : 0);
            window = new Sdl2Window(config.Title, config.NormalWindowBounds.X, config.NormalWindowBounds.Y, config.NormalWindowBounds.Width, config.NormalWindowBounds.Height, flags, false);
            graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, new GraphicsDeviceOptions(false, null, config.VSync, ResourceBindingModel.Improved, true, false), config.GraphicsBackend);
            commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            controller = new ImGuiController(graphicsDevice, window, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height, iniFile);
        }
        
        private void Draw()
        {
            commandList.Begin();
            controller.Render(graphicsDevice, commandList, config.PowerSaving);
            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
            controller.Swap(graphicsDevice, config.PowerSaving);
        }



    }
}