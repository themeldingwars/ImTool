using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        private int releaseCheckMinimumIntervalSeconds = 60;

        private bool stableUpdateAvailable;
        private bool includingPreUpdateAvailable;
        
        private Version targetVersion;
        private bool beginUpdate;
        private bool updating;
        private float progress = 0;
        
        private bool updateFailed;
        private string updateFailedReason;
        private string currentExePath;
        private FileInfo currentExeInfo;
        
        
        public Updater(Configuration config)
        {
            this.config = config;
            github = new GitHubClient(new ProductHeaderValue("ImTool"));
            ReleaseNamePrefix = $"{config.GithubReleaseName}-{OperatingSystem.GetRuntimeIdentifier}";
            currentExePath = Process.GetCurrentProcess().MainModule.FileName;
            
            if (string.IsNullOrWhiteSpace(currentExePath) || !File.Exists(currentExePath))
                return;

            currentExeInfo = new FileInfo(currentExePath);

            #if(RELEASE)
                StartupChecks();
            #endif
        }

        public Version CurrentVersion { get; } = Assembly.GetEntryAssembly().GetName().Version;
        public Release CurrentRelease { get; private set; }

        public string ReleaseNamePrefix { get; private set; }
        
        public bool IsCheckingForUpdates { get; private set; }
        public bool UpdateAvailable => config.GithubGetPrereleases ? includingPreUpdateAvailable : stableUpdateAvailable;
        public long LastUpdateCheck { get; private set; }
        public Dictionary<Version, Release> Releases { get; private set; } = new (10);

        public bool ValidConfig => !(string.IsNullOrWhiteSpace(config.GithubReleaseName) || string.IsNullOrWhiteSpace(config.GithubRepositoryName) ||
                                     string.IsNullOrWhiteSpace(config.GithubRepositoryOwner));

        public async Task CheckForUpdates()
        {
            if(config.DisableUpdater || !ValidConfig)
                return;

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
                    Update(targetVersion);
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

            if (updateFailed)
            {
                updateFailed = false;
                ImGui.OpenPopup("Update failed");
            }
            
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            if (ImGuiEx.BeginPopupModal("Update failed", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(updateFailedReason);
                ImGui.Separator();

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionMax().X - 80);
                if (ImGui.Button("Ooooops!", new Vector2(80, 0)))
                {
                    updateFailedReason = string.Empty;
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
        
        private void StartupChecks()
        {
            if (currentExeInfo == null)
                return;

            if (currentExeInfo.Name.StartsWith("imt_update_"))
            {
                try
                {
                    string realExeName = currentExeInfo.Name.Remove(0, 11);
                    string realDir = Path.GetDirectoryName(currentExePath) ?? "";
                    string realExePath = Path.Combine(realDir, realExeName);

                    if (File.Exists(realExePath))
                    {
                        string backupExePath = Path.Combine(realDir, $"imt_backup_{realExeName}");
                        
                        if(File.Exists(backupExePath))
                            File.Delete(backupExePath);
                        
                        File.Copy(currentExePath, backupExePath);
                        File.Delete(realExePath);
                    }
                    
                    File.Copy(currentExePath, realExePath);
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = realExePath,
                    });
                    
                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Derped while installing update!");
                    Console.WriteLine(e);
                }
            }
            else
            {
                string realDir = Path.GetDirectoryName(currentExePath) ?? "";
                string updateExePath = Path.Combine(realDir, $"imt_update_{currentExeInfo.Name}");

                if (File.Exists(updateExePath))
                    File.Delete(updateExePath);
            }
        }

        internal void PostStartupChecks()
        {
            if (currentExeInfo == null)
                return;
            
            string realDir = Path.GetDirectoryName(currentExePath) ?? "";
            string backupExePath = Path.Combine(realDir, $"imt_backup_{currentExeInfo.Name}");

            if (File.Exists(backupExePath))
                File.Delete(backupExePath);
        }

        internal async Task Update(Version version)
        {
            progress = 0;

            if (string.IsNullOrWhiteSpace(currentExePath))
            {
                UpdateFailed($"Could not determine executable path");
                return;
            }

            if (!Releases.TryGetValue(version, out Release release))
            {
                UpdateFailed($"Unknown version {version}");
                return;
            }

            ReleaseAsset asset = release.Assets.FirstOrDefault(asset => asset.Name.StartsWith(ReleaseNamePrefix));
            if (asset == null)
            {
                UpdateFailed($"Could not find an asset matching your runtime \"{ReleaseNamePrefix}\"");
                return;
            }

            Uri uri = new Uri(asset.BrowserDownloadUrl);
            string fileName = Path.GetFileName(uri.AbsolutePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                UpdateFailed($"Could not find an asset matching your runtime \"{ReleaseNamePrefix}\"");
                return;
            }
            
            byte[] data = null;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (obj, args) =>
                    {
                        progress = (1f / args.TotalBytesToReceive) * args.BytesReceived;
                    };
                    data = await webClient.DownloadDataTaskAsync(uri);
                }
            }
            catch (Exception e)
            {
                UpdateFailed($"Failed to download update");
                return;
            }

            string realDir = Path.GetDirectoryName(currentExePath) ?? "";
            string updateExePath = Path.Combine(realDir, $"imt_update_{currentExeInfo.Name}");
            
            try
            {
                if(File.Exists(updateExePath))
                    File.Delete(updateExePath);
            
                File.WriteAllBytes(updateExePath, data);
            }
            catch (Exception e)
            {
                UpdateFailed($"Failed installing update");
                return;
            }
            
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = updateExePath,
                });
            }
            catch (Exception e)
            {
                UpdateFailed($"Failed to start new process");
                return;
            }
            
            updating = false;
            Environment.Exit(0);
            
        }
        
        private void UpdateFailed(string reason)
        {
            progress = 0;
            updateFailed = true;
            updateFailedReason = $"Ooops! Failed to {(targetVersion > CurrentVersion ? "upgrade" : "downgrade")} to version {targetVersion}! \n{reason} :< \n\n";
            updating = false;
        }
        
        internal async Task EmergencyUpdate()
        {
            #if(DEBUG)
                Console.ReadLine();
                return;
            #endif
            
            Console.WriteLine("------------------------------");
            Console.WriteLine("Oooops, something broke badly!");
            Console.WriteLine("Attempting emergency update!");

            while (IsCheckingForUpdates)
            {
                await Task.Delay(500);
            }

            Version emergencyTarget = null;
            
            foreach (var version in Releases.Keys)
            {
                if (version != CurrentVersion)
                {
                    emergencyTarget = version;
                    break;
                }
            }
            
            if (emergencyTarget != null)
            {
                Console.WriteLine($"Attempting emergency update to version {emergencyTarget}!");
                await Update(emergencyTarget);
            }
            else
            {
                updateFailed = true;
                updateFailedReason = "No update target version";
            }

            if (updateFailed)
            {
                Console.WriteLine($"Emergency update failed! {updateFailedReason}");
                Console.ReadLine();
            }
        }
    }
}