using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace ImTool
{
    public partial class Window
    {
        private void SubmitSettingPane()
        {
            ImTool.Widgets.RenderTitle("ImTool");
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

            ImGui.SliderInt("Target FPS", ref config.FpsLimit, 20, 200);
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
                    if(ImGui.Checkbox("Allow floating windows", ref config.AllowFloatingWindows))
                        restartGD = true;
                }
            }
            
            if (ImGui.Button("Reset DockSpace", new Vector2(-1, 20)))
            {
                Tab tab = tabs.FirstOrDefault(tab => tab == activeTab);
                if (tab != null)
                {
                    tab.ResetDockSpace();   
                }
            }
            
            ImGui.NewLine();
            
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
            
            if (updater.ValidConfig)
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
                
                    if (ImGui.Button("  Check for updates  "))
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
    }
}