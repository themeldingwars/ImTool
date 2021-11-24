using System;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Veldrid;

namespace ImTool
{
    public class Configuration
    {
        [JsonIgnore] public string File;
        [JsonIgnore] public string ToolDataPath;
        
        public string Title = "ImTool";
        public string Theme = "CorporateGrey";
        public string GithubRepositoryOwner;
        public string GithubRepositoryName;
        public string GithubReleaseName;
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
        [JsonIgnore] public bool DisableFloatingWindows = false;
        [JsonIgnore] public bool DisableResizing = false;
        [JsonIgnore] public bool DisableSettingsPane = false;
        [JsonIgnore] public bool DisableUserPersistence = false;
        [JsonIgnore] public bool DisableImGuiPersistence = false;
        [JsonIgnore] public bool DisableJsonThemes = false;
        

        public static T Load<T>(string toolDataPath = "") where T : Configuration
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
                instance = defaultInstance;
            
            instance.File = file;
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
            
            Serializer<T>.SerializeToFile(instance, instance.File);
        }
    }
}