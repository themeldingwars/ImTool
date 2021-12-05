using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ImTool
{
    public static class ThemeManager
    {
        private static Configuration config;
        
        public delegate void OnThemeChangedDelegate();
        public static OnThemeChangedDelegate OnThemeChanged;
        
        public static readonly Theme ImGuiLight = new Theme();
        public static readonly Theme ImGuiDark = new Theme();

        public static Theme Current { get; private set; }
        public static Dictionary<string, Theme> Themes { get; private set; }
        public static Dictionary<string, FieldInfo> VariableFields { get; private set; }
        public static Dictionary<ImGuiCol, FieldInfo> ColorFields { get; private set; }
        public static JsonSerializer JsonSerializer { get; private set; }
        
        private static bool initialized;
        private static string themesDirectory;

        public static void Initialize(Configuration configuration)
        {
            if (initialized)
            {
                return;
            }
            config = configuration;
            themesDirectory = Path.Combine(configuration.ToolDataPath, "Themes");
            
            if(!config.DisableJsonThemes)
                Directory.CreateDirectory(themesDirectory);
            
            initialized = true;
            
            VariableFields = new Dictionary<string, FieldInfo>();
            ColorFields = new Dictionary<ImGuiCol, FieldInfo>();

            FieldInfo[] fields = typeof(ImGuiStyle).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.StartsWith("Colors_"))
                {
                    int i;
                    if (int.TryParse(field.Name.Substring(7), out i))
                    {
                        
                        ColorFields.Add((ImGuiCol)i, field);
                    }
                }
                else
                {
                    VariableFields.Add(field.Name, field);
                }
            }
            
            Themes = new Dictionary<string, Theme>();

            ImGuiLight.Name = "ImGuiLight";
            ImGuiLight.Author = "ImGui";
            ImGuiLight.IsDarkSkin = false;
            ImGuiLight.WindowBackgroundColor = new Vector4(1f, 1f, 1f, 1f);
            ImGuiLight.WindowBorderGradientBegin = ImGuiLight.WindowBackgroundColor;
            ImGuiLight.WindowBorderGradientEnd = ImGuiLight.WindowBackgroundColor;
            ImGuiLight.TitlebarBackgroundColor = ImGuiLight.WindowBackgroundColor;
            ImGuiLight.Initialized = true;

            ImGuiDark.Name = "ImGuiDark";
            ImGuiDark.Author = "ImGui";
            ImGuiDark.IsDarkSkin = true;
            ImGuiDark.WindowBackgroundColor = new Vector4(0f, 0f, 0f, 1f);
            ImGuiDark.WindowBorderGradientBegin = ImGuiDark.WindowBackgroundColor;
            ImGuiDark.WindowBorderGradientEnd = ImGuiDark.WindowBackgroundColor;
            ImGuiDark.TitlebarBackgroundColor = ImGuiDark.WindowBackgroundColor;
            ImGuiDark.Initialized = true;


            unsafe
            {
                ImGuiStyle* stylePtr = ImGui.GetStyle().NativePtr;

                ImGui.StyleColorsLight();
                ImGuiLight.Style = (ImGuiStyle)Marshal.PtrToStructure((IntPtr)stylePtr, typeof(ImGuiStyle));

                ImGui.StyleColorsDark();
                ImGuiDark.Style = (ImGuiStyle)Marshal.PtrToStructure((IntPtr)stylePtr, typeof(ImGuiStyle));
            }

            Themes.Add(ImGuiLight.Name, ImGuiLight);
            Themes.Add(ImGuiDark.Name, ImGuiDark);
            Themes.Add("CorporateGrey", CorporateGrey.Generate(config.DisableJsonThemes ? null : Path.Combine(config.ToolDataPath, "Themes", "CorporateGrey.json")));
            
            ReloadThemes();
            SetTheme(config.Theme);
        }

        private static void ApplyTheme(string name)
        {
            if (Themes.ContainsKey(name))
            {
                Current = Themes[name];
            }
            else
            {
                if (Themes.ContainsKey("Default"))
                {
                    Current = Themes["Default"];
                }
                else
                {
                    Current = ImGuiDark;
                }
            }

            config.Theme = Current.Name;
            config.Save();

            ApplyTheme(Current);
        }
        
        // Apply a custom theme from a theme class
        // ! No need for this, please use this instead:
        // ! ThemeManager.Themes.Add("name", theme)
        // ! ThemeManager.SetTheme("name");
        public static void ApplyTheme(Theme theme)
        {
            unsafe
            {
                ImGuiStyle* stylePtr = ImGui.GetStyle().NativePtr;
                Marshal.StructureToPtr(theme.Style, (IntPtr)stylePtr, true);
            }

            OnThemeChanged?.Invoke();
        }

        public static unsafe void ApplyOverride(ImGuiStyleVar variable, object value)
        {
            TypedReference reference = __makeref((*ImGui.GetStyle().NativePtr));
            VariableFields[variable.ToString()].SetValueDirect(reference, value);
        }
        public static unsafe void ApplyOverride(ImGuiCol color, object value)
        {
            TypedReference reference = __makeref((*ImGui.GetStyle().NativePtr));
            ColorFields[color].SetValueDirect(reference, value);
        }
        public static unsafe void ApplyOverride(string field, object value)
        {
            TypedReference reference = __makeref((*ImGui.GetStyle().NativePtr));
            if (VariableFields.ContainsKey(field))
            {
                VariableFields[field].SetValueDirect(reference, value);
            }
            else if (Enum.IsDefined(typeof(ImGuiCol), field))
            {
                ColorFields[(ImGuiCol)Enum.Parse(typeof(ImGuiCol), field)].SetValueDirect(reference, value);
            }
        }

        public static void ResetOverride(ImGuiStyleVar variable)
        {
            ApplyOverride(variable, Current[variable]);
        }
        public static void ResetOverride(ImGuiCol color)
        {
            ApplyOverride(color, Current[color]);
        }
        public static void ResetOverride(string field)
        {
            ApplyOverride(field, Current[field]);
        }
        
        public static void Save(Theme theme)
        {

        }
        public static void SetTheme(string name)
        {
            ApplyTheme(name);
            if (config.Theme != name && Current.Name == name)
            {
                config.Theme = name;
                config.Save();
            }
        }
        public static void ReloadThemes()
        {
            if (config.DisableJsonThemes)
            {
                ApplyTheme(config.Theme);
                return;
            }
            
            Directory.CreateDirectory(themesDirectory);
            string[] themePaths = Directory.GetFiles(themesDirectory, "*.json");

            foreach (string themePath in themePaths)
            {
                Theme theme = (Theme)Theme.DeserializeFromFile(themePath);
                if(theme != null && theme.Name != ImGuiLight.Name && theme.Name != ImGuiDark.Name)
                {
                    if(Themes.ContainsKey(theme.Name))
                    {
                        Themes[theme.Name] = theme;
                    }
                    else
                    {
                        Themes.Add(theme.Name, theme);
                    }
                }
            }

            ApplyTheme(config.Theme);
        }

    }
}
