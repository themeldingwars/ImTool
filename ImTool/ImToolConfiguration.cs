using System.Numerics;

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
    }
}