using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;

namespace ImTool
{

    public class Theme<T> : Serializer<Theme<T>> where T : ImToolConfiguration<T>
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

                    foreach (var val in ThemeManager<T>.VariableFields)
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

        public Theme()
        {
            deValues = new Dictionary<string, object>();
            deColors = new Dictionary<string, Vector4>();
            Style = new ImGuiStyle();
            Initialized = false;

        }

        public Theme(string name, string author, bool isDarkSkin = false)
        {
            Theme<T> template = isDarkSkin ? ThemeManager<T>.ImGuiDark : ThemeManager<T>.ImGuiLight;

            Name = name;
            Author = author;
            IsDarkSkin = isDarkSkin;
            Style = template.Style;
            WindowBackgroundColor = template.WindowBackgroundColor;
            WindowBorderGradientBegin = template.WindowBorderGradientBegin;
            WindowBorderGradientEnd = template.WindowBorderGradientEnd;
            TitlebarBackgroundColor = template.TitlebarBackgroundColor;
            Initialized = true;
        }

        public object this[ImGuiStyleVar variable]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                return ThemeManager<T>.VariableFields[variable.ToString()].GetValueDirect(reference);
            }
            set
            {
                TypedReference reference = __makeref(Style);
                ThemeManager<T>.VariableFields[variable.ToString()].SetValueDirect(reference, value);
            }
        }

        public object this[string field]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                if(ThemeManager<T>.VariableFields.ContainsKey(field))
                {
                    return ThemeManager<T>.VariableFields[field].GetValueDirect(reference);
                }
                else if(Enum.IsDefined(typeof(ImGuiCol), field))
                {
                    ThemeManager<T>.ColorFields[(ImGuiCol)Enum.Parse(typeof(ImGuiCol), field)].GetValueDirect(reference);
                }
                return null;
            }
            set
            {
                TypedReference reference = __makeref(Style);
                if (ThemeManager<T>.VariableFields.ContainsKey(field))
                {
                    ThemeManager<T>.VariableFields[field].SetValueDirect(reference, value);
                }
                else if (Enum.IsDefined(typeof(ImGuiCol), field))
                {
                    ThemeManager<T>.ColorFields[(ImGuiCol)Enum.Parse(typeof(ImGuiCol), field)].SetValueDirect(reference, value);
                }
            }
        }

        public object this[ImGuiCol color]
        {
            get
            {
                TypedReference reference = __makeref(Style);
                return ThemeManager<T>.ColorFields[color].GetValueDirect(reference);
            }
            set
            {
                TypedReference reference = __makeref(Style);
                ThemeManager<T>.ColorFields[color].SetValueDirect(reference, value);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {

            Initialized = true;

            foreach (var val in ThemeManager<T>.VariableFields)
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
                    this[val.Key] = IsDarkSkin ? ThemeManager<T>.ImGuiDark[val.Key] : ThemeManager<T>.ImGuiLight[val.Key];
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
                    this[variable] = IsDarkSkin ? ThemeManager<T>.ImGuiDark[variable] : ThemeManager<T>.ImGuiLight[variable];
                }
            }

            deValues = null;
            deColors = null;
        }
    }
}
