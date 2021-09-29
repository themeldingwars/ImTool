
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
        protected TConfig config;
        protected Window window;
        protected Updater updater;
        
        public Tool()
        {
            string toolDataBasePath = typeof(TTool).FullName != null ? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImTool", typeof(TTool).FullName) : "";

            Directory.CreateDirectory(toolDataBasePath);
            
            config = Configuration.Load<TConfig>(toolDataBasePath);
            updater = new Updater(config);
            
            if(!Initialize(Environment.GetCommandLineArgs()))
                return;
            
            updater.CheckForUpdates();
            
            window = Window.Create(config).Result;

            if (window == null)
            {
                updater.EmergencyUpdate().Wait();
                Environment.Exit(1);
            }
            
            updater.PostStartupChecks();
            window.SetUpdater(updater);

            if (IsMainMenuOverridden)
            {
                window.OnSubmitGlobalMenuBarOverride = SubmitMainMenu;
            }
            
            window.OnExit = () =>
            {
                Unload();
                window = null;
                updater = null;
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
            while (instance.window != null)
            {
                await Task.Delay(50);
            }
        }
    }
}