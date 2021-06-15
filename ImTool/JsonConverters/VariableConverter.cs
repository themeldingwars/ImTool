using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ImTool.JsonConverters
{
    class VariableConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object obj = serializer.Deserialize(reader);
            Type type = obj.GetType();

            if (type == typeof(System.Double))
            {
                return Convert.ToSingle((double)obj);
            }
            else if (type == typeof(System.String))
            {
                string dir = (string)obj;
                if(dir.StartsWith("Direction."))
                {
                    dir = dir.Replace("Direction.", "");
                    if(Enum.IsDefined(typeof(ImGuiDir), dir))
                    {
                        return (ImGuiDir)Enum.Parse(typeof(ImGuiDir), dir);
                    }             
                }
                return obj;
            }
            else if (type == typeof(JArray))
            {
                JArray array = obj as JArray;
                if(!array.HasValues)
                {
                    return null;
                }

                if(array[0].Type == JTokenType.Float)
                {
                    float[] floats = array.ToObject<float[]>();
                    return new Vector2(floats[0], floats[1]);
                }

                return obj;
            }
            else
            {
                return obj;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Type type = value.GetType();

            if (type == typeof(Vector2))
            {
                Vector2 val = (Vector2)value;
                string key = writer.Path.Split('.').Last();

                string x = ($"{val.X:0.0#####################}").Replace(',','.');
                string y = ($"{val.Y:0.0#####################}").Replace(',', '.');

                writer.WriteRawValue($"[ {x}, {y} ]");
                //serializer.Serialize(writer, new float[] {val.X, val.Y });
            }
            else if(type == typeof(ImGuiDir))
            {
                serializer.Serialize(writer, "Direction." + ((ImGuiDir)value).ToString());
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }
}
