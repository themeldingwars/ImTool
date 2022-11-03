using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ImTool
{
    public class FontFile
    {
        
        private byte[] data;
        private GCHandle handle;
        
        public string Path { get; }
        public GlyphRange[] Ranges { get; }
        public Vector2 GlyphOffset { get; } = new Vector2(0, 0);
        public bool IsValid { get; private set; } = false;
        public FontFile(string path, GlyphRange[] ranges = null, Vector2? glyphOffset = null)
        {
            Path = path;
            Ranges = ranges;
            
            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;

            ValidateAndLoad();
        }
        
        public FontFile(string path, Vector2? glyphOffset = null, GlyphRange[] ranges = null)
        {
            Path = path;
            Ranges = ranges;

            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;
            
            ValidateAndLoad();
        }
        
        public FontFile(string path, GlyphRange range, Vector2? glyphOffset = null)
        {
            Path = path;
            Ranges = new [] { range };
            
            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;
            
            ValidateAndLoad();
        }
        
        public FontFile(string path)
        {
            Path = path;
            
            ValidateAndLoad();
        }
        
        private object key = new object();
        private void ValidateAndLoad()
        {
            if (Path == "ImGui.Default")
            {
                IsValid = true;
                return;
            }
            
            lock (key)
            {
                Stream stream;
                if(!TryGetStream(out stream) || stream == null || stream.Length == 0)
                    return;

                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                IsValid = true;
                stream.Dispose();
            }
        }

        private bool TryGetStream(out Stream stream)
        {
            stream = null;
            
            if (string.IsNullOrWhiteSpace(Path))
            {
                return false;
            }
            
            try
            {
                // first try load from file
                if (File.Exists(Path))
                {
                    stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
                    return true;
                }
                
                // try load from embedded resource
                Assembly a = Assembly.GetExecutingAssembly();
                stream = a.GetManifestResourceStream(Path);
                if (stream != null)
                {
                    return true;
                }

                foreach (var ResourceAssembly in FontManager.ResourceAssemblies)
                {
                    stream = ResourceAssembly.GetManifestResourceStream(Path);
                    if (stream != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        internal ushort[] GetGlyphRanges()
        {
            if (Ranges == null || Ranges.Length == 0)
                return null;

            int i = 0;
            ushort[] ret = new ushort[(Ranges.Length * 2) + 1];
            foreach (GlyphRange range in Ranges)
            {
                ret[i] = range.Min;
                ret[i+1] = range.Max;
                i += 2;
            }
            return ret;
        }

        internal IntPtr GetPinnedData() => handle.AddrOfPinnedObject();
        internal int GetPinnedDataLength() => data?.Length ?? 0;
    }
}