using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImTool.Scene3D
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SelectableID
    {
        public const uint NO_ID_VALUE = 8388607;
        public const uint MAX_ID      = NO_ID_VALUE - 1;

        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
        [FieldOffset(2)]
        public byte B2;
        [FieldOffset(3)]
        public byte B3;

        public uint Id    => (uint)((B2 << 16) | ((B1) << 8) | ((B0)));
        public byte SubId => B3;

        public SelectableID(uint mainId, byte subId)
        {
            B0 = (byte)(mainId & 0xff);
            B1 = (byte)(mainId >> 8 & 0xff);
            B2 = (byte)(mainId >> 16 & 0xff);

            B3 = subId;
        }   
    }
}
