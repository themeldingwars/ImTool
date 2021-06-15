using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ImTool
{
    public class Configuration<T> : Serializer<Configuration<T>>
    {
        public static T Config;
        private static string name;
        private static string path;
        static Configuration()
        {
            Directory.CreateDirectory("Config");
            name = typeof(T).FullName;
            path = Path.Join("Config", name+".json");
            if (!Load())
            {
                Config = (T)Activator.CreateInstance(typeof(T));
            }
        }
        private static bool Load()
        {
            if (File.Exists(path))
            {
                try
                {
                    object obj = DeserializeFromFile(path);

                    if(obj != null)
                    {
                        Config = (T)obj;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public void Reload()
        {
            // maybe do some checking if it failed
            if (!Load())
            {
                Config = (T)Activator.CreateInstance(typeof(T));
            }
        }
        public void Save()
        {
            SerializeToFile(Config, path);
        }
    }
}
