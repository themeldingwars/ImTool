using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace ImTool
{
    public static class CorporateGrey
    {
        public static void Generate()
        {
            Theme theme = new Theme("CorporateGrey", "malamanteau", true);

            theme.TitlebarBackgroundColor = new Vector4(1f, 0.15f, 0.15f, 0.15f);
            theme.WindowBorderGradientBegin = new Vector4(Color.DarkMagenta.A / 255f, Color.DarkMagenta.R / 255f, Color.DarkMagenta.G / 255f, Color.DarkMagenta.B / 255f);
            theme.WindowBorderGradientEnd = new Vector4(Color.Teal.A / 255f, Color.Teal.R / 255f, Color.Teal.G / 255f, Color.Teal.B / 255f);
            theme.WindowBackgroundColor = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);

            theme[ImGuiStyleVar.PopupRounding] = 3;
            theme[ImGuiStyleVar.WindowPadding] = new Vector2(4, 4);
            theme[ImGuiStyleVar.FramePadding] = new Vector2(6, 4);
            theme[ImGuiStyleVar.ItemSpacing] = new Vector2(6, 2);
            theme[ImGuiStyleVar.ScrollbarSize] = 18;
            theme[ImGuiStyleVar.WindowBorderSize] = 1;
            theme[ImGuiStyleVar.ChildBorderSize] = 1;
            theme[ImGuiStyleVar.PopupBorderSize] = 1;
            theme[ImGuiStyleVar.FrameBorderSize] = 0;
            theme[ImGuiStyleVar.WindowRounding] = 3;
            theme[ImGuiStyleVar.ChildRounding] = 3;
            theme[ImGuiStyleVar.FrameRounding] = 3;
            theme[ImGuiStyleVar.ScrollbarRounding] = 2;
            theme[ImGuiStyleVar.GrabRounding] = 3;
            theme[ImGuiStyleVar.TabRounding] = 3;

            theme[ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            theme[ImGuiCol.TextDisabled] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
            theme[ImGuiCol.ChildBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            theme[ImGuiCol.WindowBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            theme[ImGuiCol.PopupBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            theme[ImGuiCol.Border] = new Vector4(0.12f, 0.12f, 0.12f, 0.71f);
            theme[ImGuiCol.BorderShadow] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            theme[ImGuiCol.FrameBg] = new Vector4(0.42f, 0.42f, 0.42f, 0.54f);
            theme[ImGuiCol.FrameBgHovered] = new Vector4(0.42f, 0.42f, 0.42f, 0.40f);
            theme[ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.67f);
            theme[ImGuiCol.TitleBg] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
            theme[ImGuiCol.TitleBgActive] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
            theme[ImGuiCol.TitleBgCollapsed] = new Vector4(0.17f, 0.17f, 0.17f, 0.90f);
            theme[ImGuiCol.MenuBarBg] = new Vector4(0.335f, 0.335f, 0.335f, 1.000f);
            theme[ImGuiCol.ScrollbarBg] = new Vector4(0.24f, 0.24f, 0.24f, 0.53f);
            theme[ImGuiCol.ScrollbarGrab] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            theme[ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
            theme[ImGuiCol.ScrollbarGrabActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
            theme[ImGuiCol.CheckMark] = new Vector4(0.65f, 0.65f, 0.65f, 1.00f);
            theme[ImGuiCol.SliderGrab] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
            theme[ImGuiCol.SliderGrabActive] = new Vector4(0.64f, 0.64f, 0.64f, 1.00f);
            theme[ImGuiCol.Button] = new Vector4(0.54f, 0.54f, 0.54f, 0.35f);
            theme[ImGuiCol.ButtonHovered] = new Vector4(0.52f, 0.52f, 0.52f, 0.59f);
            theme[ImGuiCol.ButtonActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
            theme[ImGuiCol.Header] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
            theme[ImGuiCol.HeaderHovered] = new Vector4(0.47f, 0.47f, 0.47f, 1.00f);
            theme[ImGuiCol.HeaderActive] = new Vector4(0.76f, 0.76f, 0.76f, 0.77f);
            theme[ImGuiCol.Separator] = new Vector4(0.000f, 0.000f, 0.000f, 0.137f);
            theme[ImGuiCol.SeparatorHovered] = new Vector4(0.700f, 0.671f, 0.600f, 0.290f);
            theme[ImGuiCol.SeparatorActive] = new Vector4(0.702f, 0.671f, 0.600f, 0.674f);
            theme[ImGuiCol.ResizeGrip] = new Vector4(0.26f, 0.59f, 0.98f, 0.25f);
            theme[ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
            theme[ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
            theme[ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            theme[ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            theme[ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            theme[ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            theme[ImGuiCol.TextSelectedBg] = new Vector4(0.73f, 0.73f, 0.73f, 0.35f);
            theme[ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
            theme[ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            theme[ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
            theme[ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            theme[ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            //theme[ImGuiCol.DockingEmptyBg] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
            theme[ImGuiCol.Tab] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            theme[ImGuiCol.TabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
            theme[ImGuiCol.TabActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
            theme[ImGuiCol.TabUnfocused] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            theme[ImGuiCol.TabUnfocusedActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
            //theme[ImGuiCol.DockingPreview] = new Vector4(0.85f, 0.85f, 0.85f, 0.28f);

            File.WriteAllText("Themes\\CorporateGrey.json", Theme.Serialize(theme));
            
        }
    }
}
