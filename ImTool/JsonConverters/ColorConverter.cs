using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ImTool.JsonConverters
{
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector4);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Vector4 vec = default;
            TryConvertHexToVector4((string)reader.Value, out vec);
            return vec;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ConvertVector4ToHex((Vector4)value));
        }

        public static bool TryConvertHexToVector4(string hex, out Vector4 vector)
        {
            Vector4 ret = new Vector4();
            string trimmed = hex.TrimStart('#');

            try
            {
                if (trimmed.Length >= 6)
                {
                    ret.X = (1f / 255f) * Convert.ToInt32(trimmed.Substring(0, 2), 16);
                    ret.Y = (1f / 255f) * Convert.ToInt32(trimmed.Substring(2, 2), 16);
                    ret.Z = (1f / 255f) * Convert.ToInt32(trimmed.Substring(4, 2), 16);

                    if (trimmed.Length >= 8)
                    {
                        ret.W = (1f / 255f) * Convert.ToInt32(trimmed.Substring(6, 2), 16);
                    }
                    else
                    {
                        ret.W = 1f;
                    }
                }
            }
            catch (Exception ex)
            {
                vector = default;
                return false;
            }
            vector = ret;
            return true;
        }

        public static string ConvertVector4ToHex(Vector4 vector)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#");
            sb.Append(((byte)(vector.X * 255)).ToString("X2"));
            sb.Append(((byte)(vector.Y * 255)).ToString("X2"));
            sb.Append(((byte)(vector.Z * 255)).ToString("X2"));
            sb.Append(((byte)(vector.W * 255)).ToString("X2"));
            return sb.ToString();
        }

    }
}
