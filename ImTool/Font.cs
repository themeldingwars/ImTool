using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;

namespace ImTool
{
    public class Font
    {
        private Dictionary<byte, ImFontPtr> pointers;
        private List<byte> sizes;
        public string Name { get; private set; }
        public IReadOnlyList<FontFile> Files { get; private set; }
        public IReadOnlyList<byte> Sizes { get => sizes; }


        public Font(string name, byte size, FontFile file) => Init(name, new List<byte> {size}, new List<FontFile> {file});

        public Font(string name, byte size, List<FontFile> files) => Init(name, new List<byte> {size}, files);

        public Font(string name, List<byte> sizes, FontFile file) => Init(name, sizes, new List<FontFile> {file});
        public Font(string name, List<byte> sizes, List<FontFile> files) => Init(name, sizes, files);
        
        private void Init(string name, List<byte> sizes, List<FontFile> files)
        {
            pointers = new();
            this.sizes = ValidateSizes(sizes);
            Files = ValidateFiles(files);
            Name = name;
        }
        
        public ImFontPtr GetPointer(byte fontSize = 0)
        {
            if (fontSize == 0)
                fontSize = Sizes[0];
                    
            if (pointers.ContainsKey(fontSize))
                return pointers[fontSize];

            return ImGui.GetDefaultFont();
        }

        public void AddSize(byte size)
        {
            if(!sizes.Contains(size))
                sizes.Add(size);
        }
        
        public void Clear()
        {
            pointers.Clear();
        }

        private IReadOnlyList<FontFile> ValidateFiles(List<FontFile> files)
        {
            if (files == null)
                return new List<FontFile>();

            if (files.Count == 0)
                return files;
            
            List<FontFile> ret = new List<FontFile>();
            
            foreach (FontFile file in files)
            {
                if (file.Path == "ImGui.Default")
                {
                    ret.Add(file);
                    continue;
                }

                if (!file.TryGetStream(out Stream stream) || stream == null || stream.Length == 0)
                {
                    Console.WriteLine($"Could not find font file \"{file.Path}\" when creating font {Name}.");
                    continue;
                }
                
                stream.Dispose();
                ret.Add(file);
            }

            Console.WriteLine(ret.Count);
            return ret;
        }
        private List<byte> ValidateSizes(List<byte> sizes)
        {
            if (sizes == null || sizes.Count == 0)
                return new List<byte>{13};
            
            List<byte> ret = new List<byte>();
            foreach (byte size in sizes)
            {
                if(!ret.Contains(size))
                    ret.Add(size);
            }

            return ret;
        }
        internal void Build()
        {
            Clear();
            
            foreach (byte size in Sizes)
                pointers.Add(size, Create(size));
        }
        private unsafe ImFontPtr Create(byte fontSize)
        {
            ImFontAtlasPtr atlasPtr = ImGui.GetIO().Fonts;
            ImFontPtr imFontPtr = null;
            
            bool first = true;
            foreach (FontFile file in Files)
            {
                ImFontConfigPtr configPtr = CreateImFontConfigPtr(fontSize, !first);
                if (file.Path == "ImGui.Default")
                {
                    imFontPtr = atlasPtr.AddFontDefault();
                }
                else
                {
                    Stream stream;
                    if(!file.TryGetStream(out stream))
                        continue;
                
                    byte[] ba = new byte[stream.Length];
                    stream.Read(ba, 0, ba.Length);
                    GCHandle pinnedArray = GCHandle.Alloc(ba, GCHandleType.Pinned);
                
                    ushort[] ranges = file.GetGlyphRanges();
                    if (ranges == null || ranges.Length == 0)
                    {
                        imFontPtr = atlasPtr.AddFontFromMemoryTTF(pinnedArray.AddrOfPinnedObject(), ba.Length, fontSize, configPtr);
                    }
                    else
                    {
                        GCHandle rangeHandle = GCHandle.Alloc(ranges, GCHandleType.Pinned);
                        imFontPtr = atlasPtr.AddFontFromMemoryTTF(pinnedArray.AddrOfPinnedObject(), ba.Length, fontSize, configPtr, rangeHandle.AddrOfPinnedObject());
                        rangeHandle.Free();
                    }
                    pinnedArray.Free();
                    stream.Dispose();
                }
                
                first = false;
                ImGuiNative.ImFontConfig_destroy(configPtr);
            }

            return imFontPtr;
        }
        private unsafe ImFontConfigPtr CreateImFontConfigPtr(byte fontSize, bool mergeMode)
        {
            ImFontConfigPtr configPtr = ImGuiNative.ImFontConfig_ImFontConfig();
            configPtr.OversampleH = 1;
            configPtr.OversampleV = 1;
            configPtr.PixelSnapH = true;
            configPtr.RasterizerMultiply = 1f;
            configPtr.GlyphExtraSpacing = new System.Numerics.Vector2(0, 0);
            configPtr.MergeMode = mergeMode;
            configPtr.SizePixels = fontSize;
            return configPtr;
        }


    }
    
    public struct GlyphRange
    {
        public ushort Min;
        public ushort Max;

        public GlyphRange(ushort min, ushort max)
        {
            Min = min;
            Max = max;
        }
    }
}