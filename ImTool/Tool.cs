
using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace ImTool
{
    public abstract class Tool<TTool, TConfig> where TConfig : Configuration where TTool : Tool<TTool, TConfig>
    {
        public TConfig Config;
        public Window Window;
        public Updater Updater;
        
        public Tool()
        {
            string toolDataBasePath = typeof(TTool).FullName != null ? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImTool", typeof(TTool).FullName) : "";

            Config = Configuration.Load<TConfig>(toolDataBasePath);
            
            if(!Config.DisableJsonThemes || !Config.DisableUserPersistence || !Config.DisableImGuiPersistence)
                Directory.CreateDirectory(toolDataBasePath);
            
            Updater = new Updater(Config);
            
            if(!Initialize(Environment.GetCommandLineArgs()))
                return;
            
            Updater.CheckForUpdates();
            
            Window = Window.Create(Config).Result;

            if (Window == null)
            {
                Updater.EmergencyUpdate().Wait();
                Environment.Exit(1);
            }
            
            Updater.PostStartupChecks();
            Window.SetUpdater(Updater);

            if (IsMainMenuOverridden)
            {
                Window.OnSubmitGlobalMenuBarOverride = SubmitMainMenu;
            }
            
            Window.OnExit = () =>
            {
                Unload();
                Window = null;
                Updater = null;
            };
            
            Load();
        }

        protected virtual bool Initialize(string[] args)
        {
            return true;
        }

        protected virtual void Load() { }

        protected virtual void Unload() { }
        public virtual void SubmitMainMenu() { }

        internal bool IsMainMenuOverridden
        {
            get
            {
                MethodInfo m = GetType().GetMethod("SubmitMainMenu");
                return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }
        
        public static async Task Run()
        {
            TTool instance = Activator.CreateInstance<TTool>();
            while (instance.Window != null)
            {
                await Task.Delay(50);
            }
        }
    }
}