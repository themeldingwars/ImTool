using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class Resources
    {
        public static ShaderDescription LoadShader(string path, ShaderStages stage, string entryPoint = "main")
        {
            var filePath     = Path.Combine("Shaders", path);
            var txt          = File.ReadAllText(filePath);
            var shadersBytes = Encoding.UTF8.GetBytes(txt);
            var shaderDesc   = new ShaderDescription(stage, shadersBytes, entryPoint);
            return shaderDesc;
        }
    }
}
