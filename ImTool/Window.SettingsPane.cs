using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Veldrid;

namespace ImTool
{
    public partial class Window
    {
        private void SubmitSettingPane()
        {
            if (!config.HideImToolSettings)
            {
                ImTool.Widgets.RenderTitle("ImTool");

                if (SubmitAboutButton())
                    ImGui.OpenPopup("About ImTool");
                
                SubmitAboutModal();

                ImGui.SetNextItemWidth(218);
                if (ImGui.BeginCombo("Theme", ThemeManager.Current.Name))
                {
                    foreach (string theme in ThemeManager.Themes.Keys)
                    {
                        if (ImGui.Selectable(theme, ThemeManager.Current.Name == theme))
                        {
                            ThemeManager.SetTheme(theme);
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SetNextItemWidth(218);
                ImGui.SliderInt("Target FPS", ref config.FpsLimit, 20, 200);
                
                ImGui.SetNextItemWidth(218);
                if (ImGui.BeginCombo("Graphics backend", config.GraphicsBackend.ToString()))
                {
                    foreach (GraphicsBackend backend in SupportedGraphicsBackends)
                    {
                        if (ImGui.Selectable(backend.ToString(), config.GraphicsBackend == backend))
                        {
                            config.GraphicsBackend = backend;
                            restartGD = true;
                        }
                    }

                    ImGui.EndCombo();
                }
                
                ImGui.Checkbox("Enable VSync  ", ref vsync);
                ImGui.SameLine();
                ImGui.Checkbox("Experimental power saving", ref config.PowerSaving);

                if (!config.DisableFloatingWindows)
                {
                    if (config.GraphicsBackend == GraphicsBackend.OpenGL)
                    {
                        ThemeManager.ApplyOverride(ImGuiCol.Text, ImToolColors.LogWarn);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("OpenGl does not support floating windows!");
                        ThemeManager.ResetOverride(ImGuiCol.Text);
                    }
                    else
                    {
                        if (ImGui.Checkbox("Allow floating windows", ref config.AllowFloatingWindows))
                            restartGD = true;
                    }
                }

                if (ImGui.Button("Reset DockSpace", new Vector2(-1, 0)))
                {
                    Tab tab = tabs.FirstOrDefault(tab => tab == activeTab);
                    if (tab != null)
                    {
                        tab.ResetDockSpace();
                    }
                }

                ImGui.NewLine();
            }

            Widgets.RenderTitle(config.Title ?? "Unknown");
            
            
            if (ImGui.BeginTable("VersionTable", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text("Version");
                ImGui.TableNextColumn(); ImGui.Text(updater.CurrentVersion.ToString());

                if (config.GithubRepositoryOwner != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn(); ImGui.Text("Author");
                    ImGui.TableNextColumn(); Widgets.Hyperlink(config.GithubRepositoryOwner, $"https://github.com/{config.GithubRepositoryOwner}");
                }

                if (config.GithubRepositoryName != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn(); ImGui.Text("Repository");
                    ImGui.TableNextColumn(); Widgets.Hyperlink(config.GithubRepositoryName, $"https://github.com/{config.GithubRepositoryOwner}/{config.GithubRepositoryName}");
                }
                
                if (config.GithubReleaseName != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn(); ImGui.Text("Release");
                    ImGui.TableNextColumn(); Widgets.Hyperlink(config.GithubReleaseName, $"https://github.com/{config.GithubRepositoryOwner}/{config.GithubRepositoryName}/releases");
                }
                
                ImGui.EndTable();
            }
            
            if (!config.DisableUpdater && updater.ValidConfig)
            {
                ImGui.NewLine();
                Widgets.RenderTitle("Releases");
                if (!updater.IsCheckingForUpdates)
                {
                    if (updater.Releases.Count > 0)
                    {
                        ImGuiTableFlags releaseTableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;
                        if (ImGui.BeginTable("ReleasesTable", 3, releaseTableFlags, new Vector2(0, 85)))
                        {
                            ImGui.TableSetupColumn("Version");
                            ImGui.TableSetupColumn("Published");
                            ImGui.TableSetupColumn("");
                            ImGui.TableSetupScrollFreeze(0, 1);
                            ImGui.TableHeadersRow();
                            
                            foreach (var kvp  in updater.Releases)
                            {
                                if ((!kvp.Value.Prerelease || config.GithubGetPrereleases ) || kvp.Value == updater.CurrentRelease)
                                {
                                    string published = kvp.Value.PublishedAt != null ? kvp.Value.PublishedAt.Value.LocalDateTime.ToString(CultureInfo.CurrentCulture) : "";
                                    
                                    ImGui.TableNextRow();
                                    ImGui.TableNextColumn();
                                    
                                    if (ImGui.Selectable($"{kvp.Key}{(kvp.Value.Prerelease ? "*" : string.Empty)}", false, ImGuiSelectableFlags.SpanAllColumns))
                                    {
                                        updater.OpenDialog(kvp.Key);
                                    }
                                    
                                    ImGui.TableNextColumn(); ImGui.Text(published);
                                    ImGui.TableNextColumn();
                                    
                                    if (kvp.Key > updater.CurrentVersion)
                                        ImGui.TextColored(ImToolColors.ToolVersionUpgrade, "upgrade");
                                    else if (kvp.Key < updater.CurrentVersion)
                                        ImGui.TextColored(ImToolColors.ToolVersionDowngrade, "downgrade");
                                    else
                                        ImGui.TextColored(ImToolColors.ToolVersionSame, "current version");
                                }
                            }
                            updater.DrawDialogs();
                            ImGui.EndTable();
                        }
                    }
                    ImGui.Checkbox("Include pre-releases", ref config.GithubGetPrereleases);
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 46);
                    if (ImGui.Button(" Check for updates "))
                    {
                        updater.CheckForUpdates();
                    }
                }
                
                ImGui.Separator();
                ImGui.NewLine();

                foreach (Tab tab in tabs)
                {
                    tab.InternalSubmitSettings(tab == activeTab);
                }                
            }
        }

        private bool SubmitAboutButton()
        {
            bool clicked;
            Vector2 curPos = ImGui.GetCursorPos();
            Vector2 textPos = new Vector2(310, -21) + curPos;

            ImGui.SetCursorPos(textPos);
            ThemeManager.ApplyOverride(ImGuiCol.HeaderHovered, ThemeManager.Current[ImGuiCol.TitleBg]);
            ThemeManager.ApplyOverride(ImGuiCol.HeaderActive, ThemeManager.Current[ImGuiCol.TitleBg]);
            FontManager.PushFont("FAS");
            clicked = ImGui.Selectable("\uf05a", false, ImGuiSelectableFlags.DontClosePopups, new Vector2(10, 18));
            FontManager.PopFont();
            ThemeManager.ResetOverride(ImGuiCol.HeaderHovered);
            ThemeManager.ResetOverride(ImGuiCol.HeaderActive);

            if (ImGui.IsItemHovered())
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                
            ImGui.SetCursorPos(curPos);

            return clicked;
        }
        private void SubmitAboutModal()
        {
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            if (ImGuiEx.BeginPopupModal("About ImTool", ImGuiWindowFlags.AlwaysAutoResize))
            {
                FontManager.PushFont("Bold"); ImGui.Text($"ImTool " + typeof(ImTool.Updater).GetTypeInfo().Assembly.GetName().Version); FontManager.PopFont();

                ImGui.NewLine();  Widgets.Hyperlink("ImTool", "https://github.com/themeldingwars/ImTool");
                ImGui.SameLine(); ImGui.Text("is made available to you under the");
                ImGui.SameLine(); Widgets.Hyperlink("MIT", "https://github.com/themeldingwars/ImTool/blob/master/LICENSE");
                ImGui.SameLine(); ImGui.Text("License and");
                
                ImGui.Text("includes"); ImGui.SameLine();
                if (Widgets.TextButton("open-source software", "Show Third-Party Software", true))
                    ImGui.OpenPopup("Third-Party Software used by ImTool");
                
                ImGui.SameLine(); ImGui.Text("under a variety of other licenses.");
                
                ImGui.NewLine();  ImGui.Text($"Copyright"); 
                ImGui.SameLine(); ImGui.SetCursorPosY(ImGui.GetCursorPosY()+2); FontManager.PushFont("FAS"); ImGui.Text("\uf1f9"); FontManager.PopFont();
                ImGui.SameLine(); ImGui.SetCursorPosY(ImGui.GetCursorPosY()-2); ImGui.Text($"2021-{DateTime.Now.Year} The Melding Wars");
                ImGui.SameLine(); ImGui.SetCursorPosY(ImGui.GetCursorPosY()+2); FontManager.PushFont("FAS"); ImGui.Text("\uf004"); FontManager.PopFont();

                ImGui.Separator();

                ImGui.SetCursorPos(ImGui.GetCursorPos() + ImGui.GetItemRectSize() - new Vector2(128, 0));
                if (ImGui.Button("Close", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SetItemDefaultFocus();
                
                SubmitThirdPartyModal();
                ImGui.EndPopup();
            }
        }

        private void SubmitThirdPartyModal()
        {
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            if (ImGuiEx.BeginPopupModal("Third-Party Software used by ImTool", ImGuiWindowFlags.AlwaysAutoResize))
            {
                
                ImGuiTableFlags thirdPartyTableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;
                if (ImGui.BeginTable("ThirdPartyTable", 2, thirdPartyTableFlags, new Vector2(440, 440)))
                {
                    ImGui.TableSetupColumn("Software", ImGuiTableColumnFlags.None, 230);
                    ImGui.TableSetupColumn("License");
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();
                    
                    foreach (var info in ThirdPartySoftware.Info)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn(); Widgets.Hyperlink(info.Name, info.ProjectUrl);
                        ImGui.TableNextColumn(); Widgets.Hyperlink(info.License, info.LicenseUrl);
                    }
                    updater.DrawDialogs();
                    ImGui.EndTable();
                }
                
                
                ImGui.Separator();

                ImGui.SetCursorPos(ImGui.GetCursorPos() + ImGui.GetItemRectSize() - new Vector2(128, 0));
                if (ImGui.Button("Close", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SetItemDefaultFocus();
                ImGui.EndPopup();
            }
        }
    }
}