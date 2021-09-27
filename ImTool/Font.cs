using System.Collections.Generic;

namespace ImTool
{
    public enum Font
    {
        Default,
        FAB,
        FAR,
        FAS,
        FreeSans
    }

    internal class FontRange
    {
        internal ushort Min;
        internal ushort Max;
        
        internal static Dictionary<Font, FontRange> Ranges = new()
        {
            {Font.FAB, new FontRange {Min = 0xE000, Max = 0xF8E8}},
            {Font.FAR, new FontRange {Min = 0xF000, Max = 0xF5C8}},
            {Font.FAS, new FontRange {Min = 0xE000, Max = 0xF8FF}},
            {Font.FreeSans, new FontRange {Min = 0x0001, Max = 0xFFFF}},
        };
    }
}