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
        private static Configuration config;

        public static Dictionary<string, ImFontPtr> RuntimeFonts { get; private set; }

        public static Dictionary<string, ImToolFontObj> QueuedFonts;

        public static string DefaultFont = "";

        private static bool initialized;

        public static void Initialize(Configuration configuration)
        {
            if (initialized)
            {
                return;
            }

            config = configuration;

            RuntimeFonts = new Dictionary<string, ImFontPtr>();
            QueuedFonts = new Dictionary<string, ImToolFontObj>();

            initialized = true;
        }

        public static void LoadFont(string fontName, string filePath, float fontSize)
        {
            if (RuntimeFonts.ContainsKey(fontName) == false && QueuedFonts.ContainsKey(fontName) == false && System.IO.File.Exists(filePath))
            {
                QueuedFonts.Add(fontName, new ImToolFontObj(fontName, filePath, fontSize));
            }
        }

        public static void UnloadFont(string fontName)
        {
            if (RuntimeFonts.ContainsKey(fontName))
            {

            }
        }

        static public ImFontPtr GetFont(string fontName)
        {
            if (RuntimeFonts.ContainsKey(fontName))
            {
                return RuntimeFonts[fontName];
            }

            return ImGui.GetIO().FontDefault;
        }
    }

    public class ImToolFontObj
    {
        public string FontName = "";
        public string FontFilePath = "";
        public float FontSize = 13.0f;

        public ImToolFontObj()
        {

        }

        public ImToolFontObj(string fontName, string fontFilePath, float fontSize)
        {
            FontName = fontName;
            FontFilePath = fontFilePath;
            FontSize = fontSize;
        }
    }
}
