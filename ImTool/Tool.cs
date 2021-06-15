
using System;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ImTool
{
    public abstract class Tool<TTool, TConfig> where TConfig : ImToolConfiguration<TConfig> where TTool : Tool<TTool, TConfig>
    {
        protected TConfig config;
        protected Window<TConfig> window;
        protected Updater updater;
        
        public Tool()
        {
            config =  (TConfig) Configuration<TConfig>.Config;
            updater = new Updater();
            
            if(!Initialize(Environment.GetCommandLineArgs()))
                return;
            
            updater.CheckForUpdates();
            
            window = Window<TConfig>.Create().Result;
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