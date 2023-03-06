using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Veldrid;

namespace ImTool
{
    public class Configuration
    {
        public Configuration() { }
        
        // these get automatically assigned
        [JsonIgnore] public string ConfigurationFilePath;
        [JsonIgnore] public string ToolDataPath;
        [JsonIgnore] public bool FirstLaunch = false;
        
        [JsonIgnore] public string Title = "ImTool";

        // github info is optional, but the built in updater relies on these
        [JsonIgnore] public string GithubRepositoryOwner;
        [JsonIgnore] public string GithubRepositoryName;
        [JsonIgnore] public string GithubReleaseName;
        
        // user changeable values
        public string Theme = "CorporateGrey";
        public bool GithubGetPrereleases;
        public WindowState WindowState = WindowState.Normal;
        public WindowState PreviousWindowState = WindowState.Maximized;
        public int Monitor = 0;
        public Vector2 MinimumWindowSize = new Vector2(800, 600);
        public Rect NormalWindowBounds = new Rect(50, 50, 1280, 720);
        public int BorderSize = 1;
        public bool VSync = false;
        public int FpsLimit = 144;
        public GraphicsBackend GraphicsBackend = Window.GetDefaultGraphicsBackend();
        public bool PowerSaving = true;
        public bool AllowFloatingWindows = true;
        
        // dev overrides
        [JsonIgnore] public bool DisableFloatingWindows = false;
        [JsonIgnore] public bool DisableResizing = false;
        [JsonIgnore] public bool DisableSettingsPane = false;
        [JsonIgnore] public bool DisableUserPersistence = false;
        [JsonIgnore] public bool DisableImGuiPersistence = false;
        [JsonIgnore] public bool DisableJsonThemes = false;
        [JsonIgnore] public bool DisableUpdater = false;
        [JsonIgnore] public bool HideImToolSettings = false;
        
        public static T Load<T>(string toolDataPath = "") where T : Configuration, new()
        {
            T defaultInstance = (T) Activator.CreateInstance(typeof(T));
            T instance = null;
            
            string configPath = Path.Combine(toolDataPath, "Config");
            
            if (defaultInstance is {DisableUserPersistence: false})
                Directory.CreateDirectory(configPath);
            
            string name = typeof(T).FullName;
            string file = Path.Join(configPath, name + ".json");
            
            if (defaultInstance is {DisableUserPersistence: false})
                instance = (T) Serializer<T>.DeserializeFromFile(file);

            if (instance == null)
            {
                instance = defaultInstance;
                instance.FirstLaunch = true;
            }
            
            instance.ConfigurationFilePath = file;
            instance.ToolDataPath = toolDataPath;
            return instance;
        }
    }

    public static class ConfigurationExtensions
    {
        public static void Save<T>(this T instance) where T : Configuration
        {
            if (instance.DisableUserPersistence)
                return;
            
            Serializer<T>.SerializeToFile(instance, instance.ConfigurationFilePath);
        }
    }
}