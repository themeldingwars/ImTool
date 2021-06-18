using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;

namespace ImTool
{
    public class ImToolColors
    {
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 HexHovered;
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 HexSelectedUnderline;
        
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 LogTrace;
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 LogInfo;
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 LogWarn;
        [JsonConverter(typeof(JsonConverters.ColorConverter))]
        public static Vector4 LogError;

        public ImToolColors(bool isDark)
        {
            if (isDark) {
                HexHovered           = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x009DDCFF));
                HexSelectedUnderline = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0xF26430FF));
                
                LogTrace = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x848484FF));
                LogInfo  = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x3D3D3D87));
                LogWarn  = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0xF5830F79));
                LogError = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0xE6171779));
            }
            else {
                
            }
        }
        
        public static Vector4 RGBAToBGR(Vector4 Color)
        {
            return new Vector4(Color.W, Color.Z, Color.Y, Color.X);
        }
    }
}