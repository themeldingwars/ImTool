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
        public static void Hyperlink(string text, string url, bool hovered = false)
        {
            Vector2 pos = ImGui.GetCursorPos();
            ImGuiCol col = hovered ? ImGuiCol.Text : ImGuiCol.ButtonActive;
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(col));
            ImGui.Text(text);
            bool isHovered = ImGui.IsItemHovered();
            bool isClicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
            Underline(col);
            ImGui.PopStyleColor();

            if (!hovered && isHovered)
            {
                ImGui.SetCursorPos(pos);
                Hyperlink(text, url, true);
            }
            else if (hovered && isHovered)
            {
                ImGui.SetTooltip(url);
                
                if (isClicked)
                {
                    OpenUrlInBrowser(url);
                }
            }
        }

        public static void Underline(ImGuiCol col)
        {
            Vector2 min = ImGui.GetItemRectMin();
            Vector2 max = ImGui.GetItemRectMax();
            min.Y = max.Y;
            ImGui.GetWindowDrawList().AddLine( min, max, ImGui.GetColorU32(col), 1.0f );
        }

        public static bool OpenUrlInBrowser(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start (psi);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}