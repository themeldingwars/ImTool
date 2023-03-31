using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class Resources
    {
        public static ShaderDescription LoadEmbeddedShader(string resourceName, ShaderStages stage, string entryPoint = "main")
        {
            var shadersBytes = GetEmbeddedResourceBytes(resourceName);
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
    }
}
