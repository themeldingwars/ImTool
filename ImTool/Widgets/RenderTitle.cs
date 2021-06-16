using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        public static void RenderTitle(string title)
        {
            ImDrawListPtr dl = ImGui.GetWindowDrawList();
            Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
            ImGuiStylePtr styles = ImGui.GetStyle();
            dl.AddRectFilled(cursorScreenPos, cursorScreenPos + new Vector2(ImGui.GetColumnWidth(), 24), ImGui.GetColorU32(ImGuiCol.TitleBg), styles.WindowRounding);
            dl.AddText(cursorScreenPos + new Vector2(5f, 6f), ImGui.GetColorU32(ImGuiCol.Text), title);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY()+26);
        }
    }
}