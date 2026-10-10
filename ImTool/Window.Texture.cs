using System;
using System.Collections.Generic;
using Veldrid;

namespace ImTool
{
    public partial class Window
    {
        private Dictionary<ImageTexture, Texture> textures = new();

        public IntPtr GetOrCreateTextureBinding(ImageTexture texture)
        {
            Texture tex;
            if (!textures.TryGetValue(texture, out tex))
            {
                tex = texture.CreateDeviceTexture(graphicsDevice, graphicsDevice.ResourceFactory);
                textures.Add(texture, tex);
            }
            return controller.GetOrCreateImGuiBinding(graphicsDevice.ResourceFactory, tex);
        }

        public void DisposeTextureBinding(ImageTexture texture)
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