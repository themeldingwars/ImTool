using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Vulkan;
using static System.Net.Mime.MediaTypeNames;

namespace ImTool.Scene3D
{
    public static class Resources
    {
        public static GraphicsDevice GD { get; private set; }
        private static Texture MissingTextureTex = null;
        public static ResourceLayout ProjViewLayout { get; private set; }
        public static OutputDescription MainFrameBufferOutputDescription { get; private set; }

        public static void SetGD(GraphicsDevice gd)
        {
            GD = gd;

            ProjViewLayout = GD.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewStateBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            MainFrameBufferOutputDescription = new OutputDescription()
            {
                DepthAttachment  = new OutputAttachmentDescription(PixelFormat.R16_UNorm),
                ColorAttachments = new OutputAttachmentDescription[]
                {
                    new OutputAttachmentDescription(PixelFormat.R8_G8_B8_A8_UNorm)
                }
            };
        }

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

        public static unsafe Texture GetMissingTex()
        {
            if (MissingTextureTex != null)
            {
                return MissingTextureTex;
            }

            MissingTextureTex = GD.ResourceFactory.CreateTexture(new TextureDescription(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
            RgbaByte color = RgbaByte.Pink;
            GD.UpdateTexture(MissingTextureTex, (IntPtr)(&color), 4, 0, 0, 0, 1, 1, 1, 0, 0);

            return MissingTextureTex;
        }
    }
}
