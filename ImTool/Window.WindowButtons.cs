using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public partial class Window
    {
        private List<WindowButton> windowButtons = new();
        
        private void SubmitWindowButtons()
        {
            
            int nBtn = 1;
            ThemeManager.ApplyOverride(ImGuiStyleVar.FrameRounding, 0);
            ThemeManager.ApplyOverride(ImGuiCol.Button, new Vector4());

            if (!FullscreenMode)
            {
                ImGui.SetCursorPos(WindowButtonPosition(nBtn));
                FontManager.PushFont("FAS");
                if (ImGui.Button("\uf410", windowButtonSize))
                {
                    Exit();
                }
                nBtn++;
                
                if (!config.DisableResizing)
                {
                    ImGui.SetCursorPos(WindowButtonPosition(nBtn));
                    if (ImGui.Button(WindowState == WindowState.Maximized ? "\uf2d2" : "\uf2d0", windowButtonSize))
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            WindowState = config.PreviousWindowState;
                        }
                        else
                        {
                            WindowState = WindowState.Maximized;
                        }
                    }
                    nBtn++;
                }

                ImGui.SetCursorPos(WindowButtonPosition(nBtn));
                if (ImGui.Button("\uf2d1", windowButtonSize))
                {
                    window.WindowState = Veldrid.WindowState.Minimized;
                }

                if (!config.DisableSettingsPane)
                {
                    nBtn++;
                    ImGui.SetCursorPos(WindowButtonPosition(nBtn));
                    if (updater != null && updater.UpdateAvailable)
                    {
                        ThemeManager.ApplyOverride(ImGuiCol.Button, ImToolColors.ToolVersionUpgrade);
                        
                        if (ImGui.Button("\uf013", windowButtonSize))
                            ImGui.OpenPopup("imtool_setting_popup");
                        
                        ThemeManager.ResetOverride(ImGuiCol.Button);
                    }
                    else
                    {
                        if (ImGui.Button("\uf013", windowButtonSize))
                            ImGui.OpenPopup("imtool_setting_popup");
                    }
                }
                
                FontManager.PopFont();
                ThemeManager.ApplyOverride(ImGuiCol.Button, new Vector4());
            }
            
            if (windowButtons.Count > 0)
            {
                Vector2 pos = WindowButtonPosition(nBtn);
                Vector2 separatorTop = windowBounds.Position + pos + new Vector2(-2, 1);
                Vector2 separatorBottom = separatorTop + new Vector2(0, windowButtonSize.Y - 2);
                ImGui.GetWindowDrawList().AddLine(separatorTop, separatorBottom, 0x33000000, 1);
                pos.X -= 3;
                
                foreach (WindowButton windowButton in windowButtons)
                {
                    Vector2 size = windowButtonSize;
                    size.X = ImGui.CalcTextSize(windowButton.Text).X + 24;
                    pos.X -= (size.X + 1);
                
                    ImGui.SetCursorPos(pos);
                    if (ImGui.Button(windowButton.Text, size))
                    {
                        windowButton.OnClicked?.Invoke();
                    }

                    pos.X -= 1;
                }
            }
            
            ThemeManager.ResetOverride(ImGuiStyleVar.FrameRounding);
            ThemeManager.ResetOverride(ImGuiCol.Button);

            if (!config.DisableSettingsPane)
            {
                if (ImGui.IsPopupOpen("imtool_setting_popup"))
                {
                    ImGui.SetNextWindowPos(new Vector2(windowBounds.Right - settingsWidth - 2,
                        windowBounds.Top + titlebarHeight + 2));
                    ImGui.SetNextWindowSize(new Vector2(settingsWidth, windowBounds.Height - titlebarHeight - 4));
                }

                if (ImGui.BeginPopup("imtool_setting_popup", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove))
                {
                    SubmitSettingPane();
                    ImGui.EndPopup();
                }
            }
        }
                
        public void AddWindowButton(string text, WindowButton.ClickedDelegate onClicked)
        {
            AddWindowButton(new WindowButton(text, onClicked));
        }
        public void AddWindowButton(WindowButton windowButton)
        {
            if(!windowButtons.Contains(windowButton))
                windowButtons.Add(windowButton);
        }
        
        public void RemoveWindowButton(WindowButton windowButton)
        {
            if(windowButtons.Contains(windowButton))
                windowButtons.Remove(windowButton);
        }
        
        private Vector2 WindowButtonPosition(int n)
        {
            if (WindowState == WindowState.Maximized || FullscreenMode)
            {
                return new Vector2((windowBounds.Width - (windowBtnWidth + 1) * n), borderThickness + 1);
            }
            else
            {
                return new Vector2((windowBounds.Width - (windowBtnWidth + 1) * n) - 1, borderThickness + 1);
            }        
        }
    }
}