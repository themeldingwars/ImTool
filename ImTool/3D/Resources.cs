using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static System.Net.Mime.MediaTypeNames;

namespace ImTool.Scene3D
{
    public class Resources
    {
        public static ShaderDescription LoadEmbeddedShader(string resourceName, ShaderStages stage, string entryPoint = "main")
        {
            var txt          = GetEmbeddedResourceString(resourceName);
            var shadersBytes = Encoding.UTF8.GetBytes(txt);
            var shaderDesc   = new ShaderDescription(stage, shadersBytes, entryPoint);
            return shaderDesc;
        }

        public static ShaderDescription LoadShader(string path, ShaderStages stage, string entryPoint = "main")
        {
            var filePath     = Path.Combine("D:\\NonWindows\\Projects\\FauFau\\ImTool\\ImTool\\Shaders", path);
            var txt          = File.ReadAllText(filePath);
            var shadersBytes = Encoding.UTF8.GetBytes(txt);
            var shaderDesc   = new ShaderDescription(stage, shadersBytes, entryPoint);
            return shaderDesc;
        }

        public static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            Assembly assembly = typeof(Resources).Assembly;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }

        public static string GetEmbeddedResourceString(string resourceName)
        {
            Assembly assembly = typeof(Resources).Assembly;
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(s))
                    return reader.ReadToEnd();
            }
        }
    }
}
