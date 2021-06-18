
using System;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ImTool
{
    public abstract class Tool<TTool, TConfig> where TConfig : Configuration where TTool : Tool<TTool, TConfig>
    {
        protected TConfig config;
        protected Window window;
        protected Updater updater;
        
        public Tool()
        {
            config = Configuration.Load<TConfig>();
            updater = new Updater();
            
            if(!Initialize(Environment.GetCommandLineArgs()))
                return;
            
            updater.CheckForUpdates();
            
            window = Window.Create(config).Result;
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