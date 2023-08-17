using System;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        // Draw a button with a font awesome icon
        public static bool IconButton(string icon, string text = null, Vector4? iconColor = null, Vector4? bgColor = null)
        {
            var result = false;

            if (text == null)
                text = "";
            
            var sizeOfSpace = ImGui.CalcTextSize(" ").X;
            
            FontManager.PushFont("FAS");
            var iconSize    = ImGui.CalcTextSize(icon);
            var iconWidth   = (int) (Math.Ceiling(iconSize.X) / sizeOfSpace) + 2;
            FontManager.PopFont();

            var pos = ImGui.GetCursorScreenPos();

            if (bgColor != null)
                ImGui.PushStyleColor(ImGuiCol.Button, bgColor.Value);

            result = ImGui.Button(text.PadLeft(text.Length + iconWidth));

            if (bgColor != null)
                ImGui.PopStyleColor();

                ImGui.SameLine();
            var secondPos = ImGui.GetCursorPosX();
            FontManager.PushFont("FAS");
            ImGui.PushStyleColor(ImGuiCol.Text, iconColor ?? System.Numerics.Vector4.One);
            ImGui.RenderText(pos + new Vector2(text == "" ? 8 : 6, (ImGui.GetItemRectSize().Y - iconSize.Y)/2), icon);
            ImGui.PopStyleColor(1);
            FontManager.PopFont();

            return result;
        }
    }
}