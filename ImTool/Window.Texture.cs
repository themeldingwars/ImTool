using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.ImageSharp;

namespace ImTool
{
    public partial class Window
    {
        private Dictionary<ImageSharpTexture, Texture> textures = new();

        public IntPtr GetOrCreateTextureBinding(ImageSharpTexture texture)
        {
            Texture tex;
            if (!textures.TryGetValue(texture, out tex))
            {
                tex = texture.CreateDeviceTexture(graphicsDevice, graphicsDevice.ResourceFactory);
                textures.Add(texture, tex);
            }
            return controller.GetOrCreateImGuiBinding(graphicsDevice.ResourceFactory, tex);
        }

        public void DisposeTextureBinding(ImageSharpTexture texture)
        {
            if (textures.TryGetValue(texture, out Texture tex))
            {
                tex.Dispose();
                textures.Remove(texture);
            }
        }
        
        private void ClearTextureBindings()
        {
            foreach (Texture texture in textures.Values)
            {
                texture.Dispose();
            }
            textures.Clear();
        }
    }
}