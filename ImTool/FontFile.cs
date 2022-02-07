using System;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace ImTool
{
    public class FontFile
    {
        public string Path { get; }
        public GlyphRange[] Ranges { get; }
        public Vector2 GlyphOffset { get; } = new Vector2(0, 0);


        public FontFile(string path, GlyphRange[] ranges = null, Vector2? glyphOffset = null)
        {
            Path = path;
            Ranges = ranges;
            
            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;
        }
        
        public FontFile(string path, Vector2? glyphOffset = null, GlyphRange[] ranges = null)
        {
            Path = path;
            Ranges = ranges;

            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;
        }
        
        public FontFile(string path, GlyphRange range, Vector2? glyphOffset = null)
        {
            Path = path;
            Ranges = new [] { range };
            
            if (glyphOffset != null)
                GlyphOffset = (Vector2) glyphOffset;
        }
        
        public FontFile(string path)
        {
            Path = path;
        }

        public bool TryGetStream(out Stream stream)
        {
            stream = null;
            
            if (string.IsNullOrWhiteSpace(Path))
                return false;
            
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
                    return true;
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
    }
}