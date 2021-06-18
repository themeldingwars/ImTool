using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;

namespace ImTool
{

    public class Theme : Serializer<Theme>
    {   
        [JsonIgnore]
        public bool Initialized;

        private Dictionary<string, object> deValues;
        private Dictionary<string, Vector4> deColors;

        public string Name { get; set; }
        public string Author { get; set; }
        public bool IsDarkSkin { get; set; }

        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Vector4 WindowBorderGradientBegin { get; set; }

        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Vector4 WindowBorderGradientEnd { get; set; }

        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Vector4 WindowBackgroundColor { get; set; }

        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public Vector4 TitlebarBackgroundColor { get; set; }

        [JsonProperty(ItemConverterType = typeof(JsonConverters.VariableConverter))]
        public Dictionary<string, object> Variables 
        {
            get
            {
                if(Initialized)
                {
                    Dictionary<string, object> variables = new Dictionary<string, object>();

                    foreach (var val in ThemeManager.VariableFields)
                    {
                        variables.Add(val.Key.ToString(CultureInfo.InvariantCulture), this[val.Key]);
                    }
                    return variables;
                }
                else
                {
                    return deValues;
                }
            }
        }

        
        [JsonProperty(ItemConverterType = typeof(JsonConverters.ColorConverter))]
        public Dictionary<string, Vector4> Colors
        {
            get
            {
                if (Initialized)
                {
                    Dictionary<string, Vector4> colors = new Dictionary<string, Vector4>();

                    for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                    {
                        ImGuiCol color = (ImGuiCol)i;
                        colors.Add(color.ToString(), (Vector4)this[color]);
                    }
                    return colors;
                }
                else
                {
                    return deColors;
                }
            }
        }

        [JsonIgnore]
        public ImGuiStyle Style;

        public ImToolColors ImToolColors { get; set; }

        public Theme()
        {
            deValues = new Dictionary<string, object>();
            deColors = new Dictionary<string, Vector4>();
            Style = new ImGuiStyle();
            Initialized = false;

        }

        public Theme(string name, string author, bool isDarkSkin = false)
        {
            Theme template = isDarkSkin ? ThemeManager.ImGuiDark : ThemeManager.ImGuiLight;

            Name = name;
            Author = author;
            IsDarkSkin = isDarkSkin;
            Style = template.Style;
            WindowBackgroundColor = template.WindowBackgroundColor;
            WindowBorderGradientBegin = template.WindowBorderGradientBegin;
            WindowBorderGradientEnd = template.WindowBorderGradientEnd;
            TitlebarBackgroundColor = template.TitlebarBackgroundColor;
            Initialized = true;

            ImToolColors = new ImToolColors(isDarkSkin);
        }

        public object this[ImGuiStyleVar variable]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                return ThemeManager.VariableFields[variable.ToString()].GetValueDirect(reference);
            }
            set
            {
                TypedReference reference = __makeref(Style);
                ThemeManager.VariableFields[variable.ToString()].SetValueDirect(reference, value);
            }
        }

        public object this[string field]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                if(ThemeManager.VariableFields.ContainsKey(field))
                {
                    return ThemeManager.VariableFields[field].GetValueDirect(reference);
                }
                else if(Enum.IsDefined(typeof(ImGuiCol), field))
                {
                    ThemeManager.ColorFields[(ImGuiCol)Enum.Parse(typeof(ImGuiCol), field)].GetValueDirect(reference);
                }
                return null;
            }
            set
            {
                TypedReference reference = __makeref(Style);
                if (ThemeManager.VariableFields.ContainsKey(field))
                {
                    ThemeManager.VariableFields[field].SetValueDirect(reference, value);
                }
                else if (Enum.IsDefined(typeof(ImGuiCol), field))
                {
                    ThemeManager.ColorFields[(ImGuiCol)Enum.Parse(typeof(ImGuiCol), field)].SetValueDirect(reference, value);
                }
            }
        }

        public object this[ImGuiCol color]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                return ThemeManager.ColorFields[color].GetValueDirect(reference);
            }
            set
            {
                TypedReference reference = __makeref(Style);
                ThemeManager.ColorFields[color].SetValueDirect(reference, value);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {

            Initialized = true;

            foreach (var val in ThemeManager.VariableFields)
            {
                if (deValues.ContainsKey(val.Key))
                {
                    try
                    {
                        object value = Convert.ChangeType(deValues[val.Key], this[val.Key].GetType());
                        this[val.Key] = value;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(this[val.Key]);
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    this[val.Key] = IsDarkSkin ? ThemeManager.ImGuiDark[val.Key] : ThemeManager.ImGuiLight[val.Key];
                }
            }

            for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
            {
                ImGuiCol variable = (ImGuiCol)i;

                if (deColors.ContainsKey(variable.ToString()))
                {
                    this[variable] = deColors[variable.ToString()];
                }
                else
                {
                    this[variable] = IsDarkSkin ? ThemeManager.ImGuiDark[variable] : ThemeManager.ImGuiLight[variable];
                }
            }

            deValues = null;
            deColors = null;
        }
    }
}
