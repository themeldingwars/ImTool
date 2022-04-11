using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImTool
{
    public static class FontManager
    {
        public static Dictionary<string, Font> Fonts { get; private set; }

        public static string DefaultFont = "";

        static FontManager()
        {
            Fonts = new Dictionary<string, Font>();
            
            AddFont(new Font("Regular", 18, new FontFile("ImTool.Fonts.SourceSansPro-Regular.ttf", new Vector2(0, -1))));
            AddFont(new Font("Bold", 18, new FontFile("ImTool.Fonts.SourceSansPro-Bold.ttf", new Vector2(0, -1))));
            AddFont(new Font("Italic", 18, new FontFile("ImTool.Fonts.SourceSansPro-Italic.ttf", new Vector2(0, -1))));
            AddFont(new Font("BoldItalic", 18, new FontFile("ImTool.Fonts.SourceSansPro-BoldItalic.ttf", new Vector2(0, -1))));
            AddFont(new Font("ProggyClean", 13, new FontFile("ImGui.Default")));
            AddFont(new Font("FAS", 13, new FontFile("ImTool.Fonts.FAS.ttf", new GlyphRange(0xE000, 0xF8FF))));
            AddFont(new Font("FAR", 13, new FontFile("ImTool.Fonts.FAR.ttf", new GlyphRange(0xF000, 0xF5C8))));
            AddFont(new Font("FAB", 13, new FontFile("ImTool.Fonts.FAB.ttf", new GlyphRange(0xE000, 0xF8E8))));
            AddFont(new Font("FreeSans", 18, new FontFile("ImTool.Fonts.FreeSans.ttf", BuildExtendedRange(), new Vector2(0, -1))));
            
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

        private static GlyphRange[] BuildExtendedRange()
        {
            return new[]
            {
                new GlyphRange(0x0020, 0x052F), // 0020 — 007F  	Basic Latin
                                                // 00A0 — 00FF  	Latin-1 Supplement
                                                // 0100 — 017F  	Latin Extended-A
                                                // 0180 — 024F  	Latin Extended-B
                                                // 0250 — 02AF  	IPA Extensions
                                                // 02B0 — 02FF  	Spacing Modifier Letters
                                                // 0300 — 036F  	Combining Diacritical Marks
                                                // 0370 — 03FF  	Greek and Coptic
                                                // 0400 — 04FF  	Cyrillic
                                                // 0500 — 052F  	Cyrillic Supplementary
                                                

                new GlyphRange(0x2000, 0x2BFF), // 2000 — 206F  	General Punctuation
                                                // 2070 — 209F  	Superscripts and Subscripts
                                                // 20A0 — 20CF  	Currency Symbols
                                                // 20D0 — 20FF  	Combining Diacritical Marks for Symbols	
                                                // 2100 — 214F  	Letterlike Symbols
                                                // 2150 — 218F  	Number Forms
                                                // 2190 — 21FF  	Arrows
                                                // 2200 — 22FF  	Mathematical Operators
                                                // 2300 — 23FF  	Miscellaneous Technical
                                                // 2400 — 243F  	Control Pictures
                                                // 2440 — 245F  	Optical Character Recognition
                                                // 2460 — 24FF  	Enclosed Alphanumerics
                                                // 2500 — 257F  	Box Drawing
                                                // 2580 — 259F  	Block Elements
                                                // 25A0 — 25FF  	Geometric Shapes
                                                // 2600 — 26FF  	Miscellaneous Symbols
                                                // 2700 — 27BF  	Dingbats
                                                // 27C0 — 27EF  	Miscellaneous Mathematical Symbols-A
                                                // 27F0 — 27FF  	Supplemental Arrows-A
                                                // 2800 — 28FF  	Braille Patterns
                                                // 2900 — 297F  	Supplemental Arrows-B
                                                // 2980 — 29FF  	Miscellaneous Mathematical Symbols-B
                                                // 2A00 — 2AFF  	Supplemental Mathematical Operators
                                                // 2B00 — 2BFF  	Miscellaneous Symbols and Arrows

            };
        }
    }
}
