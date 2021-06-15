using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        public static void RenderArrow(Vector2 pos, uint col, ImGuiDir dir, float scale)
        {
            var draw = ImGui.GetWindowDrawList();
            float h = ImGui.GetFontSize();
            float r = h * 0.40f * scale;
            Vector2 center = pos + new Vector2(h * 0.50f, h * 0.50f * scale);
    
            Vector2 a, b, c;
            switch (dir)
            {
                case ImGuiDir.Up:
                case ImGuiDir.Down:
                    if (dir == ImGuiDir.Up) r = -r;
                    a = new Vector2(+0.000f, +0.750f) * r;
                    b = new Vector2(-0.866f, -0.750f) * r;
                    c = new Vector2(+0.866f, -0.750f) * r;
                    break;
                case ImGuiDir.Left:
                case ImGuiDir.Right:
                    if (dir == ImGuiDir.Left) r = -r;
                    a = new Vector2(+0.750f, +0.000f) * r;
                    b = new Vector2(-0.750f, +0.866f) * r;
                    c = new Vector2(-0.750f, -0.866f) * r;
                    break;
                default:
                    a = new Vector2();
                    b = new Vector2();
                    c = new Vector2();
                    break;
            }
            draw.AddTriangleFilled(center + a, center + b, center + c, col);
        }
    }
}