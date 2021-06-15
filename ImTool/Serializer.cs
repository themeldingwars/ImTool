using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ImTool
{

    public class Serializer<T> where T : Serializer<T>
    {
        protected static JsonSerializer jsonSerializer;
        static Serializer()
        {
            jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;
        }
        public static string Serialize(object obj)
        {
            using (StringWriter writer = new StringWriter())
            {
                Type type = typeof(T);
                Type[] types = type.GetGenericArguments();
                if (types.Length > 0)
                {
                    type = types[0];
                }

                jsonSerializer.Serialize(writer, obj, type);
                return writer.ToString();
            }
        }

        public static void SerializeToFile(object obj, string file)
        {
            File.WriteAllText(file, Serialize(obj));
        }

        public static object Deserialize(string json)
        {
            object obj = null;
            Type type = typeof(T);
            
            try
            {
                using (StringReader text = new StringReader(json))
                using (JsonTextReader reader = new JsonTextReader(text))
                {
                    obj = (T) jsonSerializer.Deserialize(reader, type.GetGenericArguments()[0]);
                    return obj;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (StringReader text = new StringReader(json))
                using (JsonTextReader reader = new JsonTextReader(text))
                {
                    obj = (T) jsonSerializer.Deserialize(reader, type);
                    return obj;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static object DeserializeFromFile(string file)
        {
            try
            {
                return Deserialize(File.ReadAllText(file));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}