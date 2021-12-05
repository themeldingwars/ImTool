using System;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        // Draw a button with a font awesome icon
        public static bool IconButton(string icon, string text = null, Vector4? iconColor = null)
        {
            var result = false;

            if (text == null) {
                FontManager.PushFont("FAS");
                result = ImGui.Button(icon);
                FontManager.PopFont();
            }
            else {
                FontManager.PushFont("FAS");
                var sizeOfSpace = ImGui.CalcTextSize(" ").X;
                var iconSize    = ImGui.CalcTextSize(icon);
                var iconWidth   = (int) (Math.Ceiling(iconSize.X) / sizeOfSpace) + 2;
                FontManager.PopFont();

                var pos = ImGui.GetCursorPosX();

                result = ImGui.Button(text.PadLeft(text.Length + iconWidth));
                ImGui.SameLine();
                var secondPos = ImGui.GetCursorPosX();
                FontManager.PushFont("FAS");
                ImGui.PushStyleColor(ImGuiCol.Text, iconColor ?? System.Numerics.Vector4.One);
                ImGui.RenderText(ImGui.GetCursorScreenPos() - new Vector2(secondPos - pos - 5, -3), icon);
                ImGui.PopStyleColor(1);
                FontManager.PopFont();   
            }

            return result;
        }
    }
}