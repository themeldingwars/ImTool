using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Newtonsoft.Json;
using Octokit;

namespace ImTool
{
    public class Updater
    {
        private Configuration config;
        private GitHubClient github;
        private string updatePath;
        private int releaseCheckMinimumIntervalSeconds = 60;

        private bool stableUpdateAvailable;
        private bool includingPreUpdateAvailable;
        
        private Version targetVersion;
        private bool beginUpdate;
        private bool updating;
        private float progress = 0;
        
        
        public Updater(Configuration config)
        {
            this.config = config;
            github = new GitHubClient(new ProductHeaderValue("ImTool"));
            updatePath = Path.Combine(config.ToolDataPath, "Update");
            Directory.CreateDirectory(updatePath);
            ReleaseNamePrefix = $"{config.GithubReleaseName}-{OperatingSystem.GetRuntimeIdentifier}";
        }

        public Version CurrentVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;
        public Release CurrentRelease { get; private set; }

        public string ReleaseNamePrefix { get; private set; }
        
        public bool IsCheckingForUpdates { get; private set; }
        public bool UpdateAvailable => config.GithubGetPrereleases ? includingPreUpdateAvailable : stableUpdateAvailable;
        public long LastUpdateCheck { get; private set; }
        public Dictionary<Version, Release> Releases { get; private set; } = new (10);

        public async Task CheckForUpdates()
        {
            // rate limit update checks
            if (Environment.TickCount64 - LastUpdateCheck < releaseCheckMinimumIntervalSeconds * 1000 || IsCheckingForUpdates)
                return;
            
            IsCheckingForUpdates = true;
            LastUpdateCheck = Environment.TickCount64;
            stableUpdateAvailable = false;
            includingPreUpdateAvailable = false;
            
            Releases.Clear();
            IReadOnlyList<Release> releases = await github.Repository.Release.GetAll(config.GithubRepositoryOwner, config.GithubRepositoryName);
            foreach (Release release in releases.OrderByDescending(x => x.CreatedAt))
            {
                try
                {
                    Version version = Version.Parse(release.TagName.TrimStart('v', 'V'));

                    if (version == CurrentVersion)
                    {
                        CurrentRelease = release;
                        Releases.Add(version, release);
                    }
                    else
                    {
                        if (release.Assets.Any(asset => asset.Name.StartsWith(ReleaseNamePrefix)))
                        {
                            Releases.Add(version, release);
                        }
                    }
                }
                catch (Exception e) { }
            }
            
            stableUpdateAvailable = Releases.Any(kvp => kvp.Key > CurrentVersion && !kvp.Value.Prerelease);
            includingPreUpdateAvailable = Releases.Any(kvp => kvp.Key > CurrentVersion && (!kvp.Value.Prerelease || config.GithubGetPrereleases));
            IsCheckingForUpdates = false;
        }


        internal void OpenDialog(Version version)
        {
            targetVersion = null;
            if (version != CurrentVersion)
            {
                targetVersion = version;
                ImGui.OpenPopup("Update?");
            }
        }
        
        internal void DrawDialogs()
        {
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            if (ImGuiEx.BeginPopupModal("Update?", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"Are you sure you want to \n{(targetVersion > CurrentVersion ? "upgrade" : "downgrade")} to version {targetVersion}? \n\n");
                ImGui.Separator();

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    beginUpdate = true;
                }
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    targetVersion = null;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (beginUpdate)
            {
                beginUpdate = false;
                if (targetVersion != null && updating == false)
                {
                    updating = true;
                    ImGui.OpenPopup("Downloading update");
                    Update();
                }
            }

            
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new Vector2(250, 85));
            if (ImGuiEx.BeginPopupModal("Downloading update", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
            {
                ImGui.Text($"{(targetVersion > CurrentVersion ? "Upgrading" : "Downgrading")} to version {targetVersion}... \n\n");
                ImGui.Separator();
                ThemeManager.PushFont(Font.FreeSans);
                ImGui.ProgressBar(progress, new Vector2(ImGui.GetWindowWidth() - 8, 25), ProgressEmoji(progress));
                ThemeManager.PopFont();
                if (!updating)
                {
                    targetVersion = null;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }

        private string ProgressEmoji(float progress)
        {
            if(progress < 0.1) return "( •_•)";
            if(progress < 0.3) return "(•_• )";
            if(progress < 0.35) return "( •_•)";
            if(progress < 0.40) return "(•_• )";
            if(progress < 0.42) return "( •_•)";
            if(progress < 0.44) return "( •_•)>⌐■-■";
            return "(⌐■_■)";
        }
        internal async Task Update()
        {
            Console.WriteLine("Beginning update.");

            for (int i = 0; i < 101; i++)
            {
                progress = 1f/100*i;
                await Task.Delay(50);
            }

            updating = false;
            
            Console.WriteLine("Update done");
        }
        
        internal void EmergencyUpdate()
        {
            // oops, something is broken! try to update
            
        }
    }
}