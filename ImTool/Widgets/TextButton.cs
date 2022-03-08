using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace ImTool
{
    public static partial class Widgets
    {
        public static bool TextButton(string text, string? tooltip = null, bool underline = false, bool hovered = false)
        {
            Vector2 pos = ImGui.GetCursorPos();
            ImGuiCol col = hovered ? ImGuiCol.Text : ImGuiCol.ButtonActive;
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(col));
            ImGui.Text(text);
            bool isHovered = ImGui.IsItemHovered();
            bool isClicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
            
            if(underline)
                Underline(col);
            
            ImGui.PopStyleColor();

            if (!hovered && isHovered)
            {
                ImGui.SetCursorPos(pos);
                return TextButton(text, tooltip, underline, true);
            }

            if(hovered && isHovered && tooltip != null)
                ImGui.SetTooltip(tooltip);
            
            return isClicked;
        }
    }
}