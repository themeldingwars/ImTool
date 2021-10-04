using System;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public partial class Window
    {
        private void UpdateWindowBorderThickness()
        {
            borderThickness = WindowState == WindowState.Maximized ? 0 : config.BorderSize;
        }
        private void UpdateBorderColor()
        {
            byte[] begin = NormalizedVector4ToBytes(ThemeManager.Current.WindowBorderGradientBegin);
            byte[] end = NormalizedVector4ToBytes(ThemeManager.Current.WindowBorderGradientEnd);
            byte[] middle = new byte[]
            {
                (byte)((begin[0] + end[0]) / 2),
                (byte)((begin[1] + end[1]) / 2),
                (byte)((begin[2] + end[2]) / 2),
                (byte)((begin[3] + end[3]) / 2)
            };

            windowBorderColor[0] = BitConverter.ToUInt32(begin);
            windowBorderColor[1] = BitConverter.ToUInt32(middle);
            windowBorderColor[2] = BitConverter.ToUInt32(end);
            windowBorderColor[3] = BitConverter.ToUInt32(middle);
        }

        private void OnThemeChange()
        {
            UpdateBorderColor();
            byte[] btbc = NormalizedVector4ToBytes(ThemeManager.Current.TitlebarBackgroundColor);
            titlebarColor = BitConverter.ToUInt32(btbc);
        }

        public static void MainWindowStyleOverrides(bool apply)
        {
            if(apply)
            {
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowBorderSize, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowPadding, default(Vector2));
            }
            else
            {
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowPadding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowBorderSize);
            }
        }
        public static void TabStyleOverrides(bool apply)
        {
            if (apply)
            {
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowBorderSize, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.TabRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.ItemInnerSpacing, new Vector2(1, 0));
                ThemeManager.ApplyOverride(ImGuiCol.TabActive, ThemeManager.Current[ImGuiCol.WindowBg]);
                ThemeManager.ApplyOverride(ImGuiCol.MenuBarBg, ThemeManager.Current[ImGuiCol.WindowBg]);
                ThemeManager.ApplyOverride(ImGuiCol.Tab, new Vector4(0.18f, 0.18f, 0.18f, 1.00f));
                
            }
            else
            {
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowBorderSize);
                ThemeManager.ResetOverride(ImGuiStyleVar.TabRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.ItemInnerSpacing);
                ThemeManager.ResetOverride(ImGuiCol.TabActive);
                ThemeManager.ResetOverride(ImGuiCol.MenuBarBg);
                ThemeManager.ResetOverride(ImGuiCol.Tab);
            }
        }
        
        private void HandleMouseCursor()
        {
            switch (ImGui.GetMouseCursor())
            {
                case ImGuiMouseCursor.None:
                    //SDL.SDL.SDL_SetCursor(SystemCursors.None);
                    break;
                case ImGuiMouseCursor.Arrow:
                    SDL.SDL.SDL_SetCursor(SystemCursors.Arrow);
                    break;
                case ImGuiMouseCursor.TextInput:
                    SDL.SDL.SDL_SetCursor(SystemCursors.IBeam);
                    break;
                case ImGuiMouseCursor.ResizeAll:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeALL);
                    break;
                case ImGuiMouseCursor.ResizeNS:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNS);
                    break;
                case ImGuiMouseCursor.ResizeEW:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeWE);
                    break;
                case ImGuiMouseCursor.ResizeNESW:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNESW);
                    break;
                case ImGuiMouseCursor.ResizeNWSE:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNWSE);
                    break;
                case ImGuiMouseCursor.Hand:
                    SDL.SDL.SDL_SetCursor(SystemCursors.Hand);
                    break;
                case ImGuiMouseCursor.NotAllowed:
                    SDL.SDL.SDL_SetCursor(SystemCursors.No);
                    break;
            }
        }
        
        private static byte[] NormalizedVector4ToBytes(Vector4 v)
        {
            return new byte[]
            {
                (byte)(v.Y * byte.MaxValue),
                (byte)(v.Z * byte.MaxValue),
                (byte)(v.W * byte.MaxValue),
                (byte)(v.X * byte.MaxValue),
            };
        }
    }
}