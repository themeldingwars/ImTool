using System.Numerics;
using Veldrid;

namespace ImTool
{
    public class ImToolConfiguration<T> : Configuration<T> where T : ImToolConfiguration<T>
    {
        public string Title = "Pyre";
        public string Theme = "CorporateGrey";
        public WindowState WindowState = WindowState.Normal;
        public WindowState PreviousWindowState = WindowState.Maximized;
        public int Monitor = 0;
        public Vector2 MinimumWindowSize = new Vector2(800, 600);
        public Rect NormalWindowBounds = new Rect(50, 50, 1280, 720);
        public int BorderSize = 1;
        public bool VSync = false;
        public int FpsLimit = 144;
        public GraphicsBackend GraphicsBackend = GraphicsBackend.Vulkan;
        public bool PowerSaving = true;
    }
}