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

        public ImToolColors(bool isDark)
        {
            if (isDark) {
                HexHovered           = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x009DDCFF));
                HexSelectedUnderline = RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0xF26430FF));
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