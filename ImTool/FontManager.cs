using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImTool
{
    public static class FontManager
    {
        public static Dictionary<string, Font> Fonts { get; private set; }

        static FontManager()
        {
            Fonts = new Dictionary<string, Font>();
            
            AddFont(new Font("Regular", 18, new FontFile("ImTool.Fonts.SourceSansPro-Regular.ttf")));
            AddFont(new Font("Bold", 18, new FontFile("ImTool.Fonts.SourceSansPro-Bold.ttf")));
            AddFont(new Font("Italic", 18, new FontFile("ImTool.Fonts.SourceSansPro-Italic.ttf")));
            AddFont(new Font("BoldItalic", 18, new FontFile("ImTool.Fonts.SourceSansPro-BoldItalic.ttf")));
            AddFont(new Font("ProggyClean", 13, new FontFile("ImGui.Default")));
            AddFont(new Font("FAS", 13, new FontFile("ImTool.Fonts.FAS.ttf", new GlyphRange(0xE000, 0xF8FF))));
            AddFont(new Font("FAR", 13, new FontFile("ImTool.Fonts.FAR.ttf", new GlyphRange(0xF000, 0xF5C8))));
            AddFont(new Font("FAB", 13, new FontFile("ImTool.Fonts.FAB.ttf", new GlyphRange(0xE000, 0xF8E8))));
            AddFont(new Font("FreeSans", 18, new FontFile("ImTool.Fonts.FreeSans.ttf", new GlyphRange(0x0001, 0xFFFF))));
        }
        public static void AddFont(Font font)
        {
            if(!Fonts.ContainsKey(font.Name))
                Fonts.Add(font.Name, font);
        }

        public static ImFontPtr GetImFontPointer(string font, byte fontSize = 0)
        {
            if (Fonts.ContainsKey(font))
                return Fonts[font].GetPointer(fontSize);
            
            return ImGui.GetIO().FontDefault;
        }
        
        public static void PushFont(string font, byte fontSize = 0) => ImGui.PushFont(GetImFontPointer(font, fontSize));
        public static void PopFont() => ImGui.PopFont();
        public static void Clear()
        {
            if(Fonts == null)
                return;
            
            foreach (Font font in Fonts.Values)
                font.Clear();
            
           // ImGui.GetIO().Fonts.Clear();
        }
    }
}
