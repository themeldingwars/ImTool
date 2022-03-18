using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Bitter;
using ImGuiNET;

namespace ImTool
{
    public class HexView
    {
        private static        uint[] Lookup  = CreateLookup();
        private static unsafe byte*  LookupB = (byte*) GCHandle.Alloc(Lookup, GCHandleType.Pinned).AddrOfPinnedObject();

        private static uint[] CreateLookup()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++) {
                string s = i.ToString("X2");
                if (BitConverter.IsLittleEndian) {
                    result[i] = ((uint) s[0]) + ((uint) s[1] << 8);
                }
                else {
                    result[i] = ((uint) s[1]) + ((uint) s[0] << 8);
                }
            }

            return result;
        }

        const int BYTE_WIDTH = 16;

        public byte[]                   Bytes;
        public ByteData[]               ByteDataArr;
        public HighlightSection[]       HighlightsArr;
        public int                      SelectedHighlight = -1;
        public int                      HoveredIdx        = -1;
        public Action<int>              OnByteHoverTooltip;
        public Action<HighlightSection> OnHighlightSectionHover;
        public int                      LastHoveredIdx;

        // Visual settings
        public bool ShowSideParsedValues = true;
        public bool ShowParsedValuesInTT = true;

        private float    AddrColSize               = ImGui.CalcTextSize("0000: ").X;
        private Vector2  ByteDisplaySize           = ImGui.CalcTextSize("00") + new Vector2(ImGui.GetStyle().ItemSpacing.X, 0);
        private Vector2  ByteDisplaySizeWithMiddle = ImGui.CalcTextSize("00") + new Vector2(ImGui.GetStyle().ItemSpacing.X * 2, 0);
        private string[] AsciiLines                = null;
        private string[] AddrStrings               = null;

        private ParsedValuesStruct ParsedValues;

        private bool FontSizesNeedSetupAgain = true;
        public HexView()
        {
            SetupSizes();
        }
        
        ~HexView()
        {
            
        }

        // Given an array of bytes and an array of highlighs setups the hex display
        public void SetData(byte[] bytes, HighlightSection[] highlights)
        {
            if (bytes == null) {
                return;
            }

            Bytes         = bytes;
            ByteDataArr   = new ByteData[bytes.Length];
            HighlightsArr = highlights;

            for (int i = 0; i < bytes.Length; i++) {
                var b         = bytes[i];
                var highlight = highlights.FirstOrDefault(x => x.Offset <= i && (x.Offset + x.Length) > i);
                var idx       = Array.IndexOf(highlights, highlight);
                ByteDataArr[i] = new ByteData()
                {
                    Byte         = b,
                    HexStr       = $"{b:X}".PadLeft(2, '0'),
                    HighlightIdx = idx == -1 ? ushort.MaxValue : (ushort) idx
                };
            }

            var numLines = (int) Math.Ceiling((decimal) (ByteDataArr.Length / BYTE_WIDTH)) + 1;
            AsciiLines  = new string[numLines];
            AddrStrings = new string[numLines];
            var start = 0;
            for (int i = 0; i < numLines; i++) {
                var len        = Math.Min(ByteDataArr.Length - start, BYTE_WIDTH);
                var asciibytes = new byte[len];
                Array.Copy(bytes, start, asciibytes, 0, len);
                var asciiLine = string.Join("", asciibytes.Select(x => (x >= 32 && x < 128) ? Convert.ToChar(x) : '.').ToArray());
                AsciiLines[i]  =  asciiLine;
                AddrStrings[i] =  $"{start}: ";
                start          += BYTE_WIDTH;
            }
        }

        public void SetupSizes()
        {
            AddrColSize               = ImGui.CalcTextSize("0000: ").X;
            ByteDisplaySize           = ImGui.CalcTextSize("00") + new Vector2(ImGui.GetStyle().ItemSpacing.X, 0);
            ByteDisplaySizeWithMiddle = ImGui.CalcTextSize("00") + new Vector2(ImGui.GetStyle().ItemSpacing.X * 2, 0);
        }

        public unsafe void Draw()
        {
            FontManager.PushFont("ProggyClean");
            if (ByteDataArr == null) {
                return;
            }

            if (FontSizesNeedSetupAgain) {
                SetupSizes();
                FontSizesNeedSetupAgain = false;
            }

            int numFullLines   = ByteDataArr.Length / BYTE_WIDTH;
            int remainingBytes = ByteDataArr.Length % BYTE_WIDTH;
            int totalLines     = numFullLines     + (remainingBytes > 0 ? 1 : 0);
            int middleByte     = (BYTE_WIDTH / 2) - 1;

            if (ImGui.BeginTable("Hex table", 4)) {
                ImGui.TableSetupColumn("Addr", ImGuiTableColumnFlags.WidthFixed, AddrColSize);
                ImGui.TableSetupColumn("Bytes", ImGuiTableColumnFlags.WidthFixed, (ByteDisplaySize.X * BYTE_WIDTH) + 15);
                ImGui.TableSetupColumn("Ascii", ImGuiTableColumnFlags.WidthFixed, (ByteDisplaySize.X * 6));
                ImGui.TableSetupColumn("ParsedValues", ImGuiTableColumnFlags.WidthFixed, 300);

                ImGui.TableNextColumn();


                for (int i = 0; i < totalLines; i++) {
                    ImGui.Text(AddrStrings[i]);
                }
                
                ImGui.TableNextColumn();

                for (int y = 0; y < numFullLines; y++) {
                    DrawHexLine(y, middleByte, BYTE_WIDTH);
                }

                if (remainingBytes > 0) {
                    DrawHexLine(numFullLines, middleByte, remainingBytes);
                }
                
                ImGui.TableNextColumn();

                for (int i = 0; i < totalLines; i++) {
                    ImGui.TextUnformatted(AsciiLines[i]);
                }

                if (ShowSideParsedValues) {
                    ImGui.NextColumn();
                    ImGui.TableNextColumn();
                    DrawParsedValues(1);
                }

                ImGui.EndTable();
                FontManager.PopFont();
            }
        }

        public void DrawParsedValues(int perRow = 2)
        {
            if (ImGui.BeginTable("HexViewParsedValues", perRow * 2, ImGuiTableFlags.Borders)) {
                for (int i = 0; i < perRow; i++) {
                    ImGui.TableSetupColumn($"Name_{i}", ImGuiTableColumnFlags.WidthFixed, 80);
                    ImGui.TableSetupColumn($"Value_{i}", ImGuiTableColumnFlags.None, 100);
                }

                AddParsedValueRow("Offset", $"{LastHoveredIdx}");
                AddParsedValueRow("length", $"{Bytes.Length}");
                AddParsedValueRow("SByte", $"{ParsedValues.SByte}");
                AddParsedValueRow("Byte", $"{ParsedValues.SByte}");
                AddParsedValueRow("Short", $"{ParsedValues.Short}");
                AddParsedValueRow("UShort", $"{ParsedValues.SByte}");
                AddParsedValueRow("Int", $"{ParsedValues.Int}");
                AddParsedValueRow("Uint", $"{ParsedValues.Uint}");
                AddParsedValueRow("Long", $"{ParsedValues.Long}");
                AddParsedValueRow("ULong", $"{ParsedValues.ULong}");
                AddParsedValueRow("Float", $"{ParsedValues.Float}");
                AddParsedValueRow("Double", $"{ParsedValues.Double}");
                AddParsedValueRow("QuantFloat", $"{ParsedValues.QuantFloat}");
                AddParsedValueRow("HalfFloat", $"{ParsedValues.HalfFloat}");

                ImGui.EndTable();
            }
        }

        public void SetHighlightAsSelected(int highlightIdx)
        {
            if (highlightIdx >= 0 && highlightIdx < HighlightsArr.Length) {
                HighlightsArr[highlightIdx].IsSelected = true;
            }
        }

        public void ClearSelectedHighlights()
        {
            for (int i = 0; i < HighlightsArr.Length; i++) {
                HighlightsArr[i].IsSelected = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void DrawHexLine(int line, int middleByte, int numBytes)
        {
            HoveredIdx = -1;

            for (int x = 0; x < numBytes; x++) {
                int index     = (line * BYTE_WIDTH) + x;
                var pos       = ImGui.GetCursorScreenPos();
                var byteWidth = x == middleByte ? pos + ByteDisplaySizeWithMiddle : pos + ByteDisplaySize;

                // Highlight values
                if (ByteDataArr[index].HighlightIdx != ushort.MaxValue) {
                    var drawList = ImGui.GetWindowDrawList();
                    drawList.AddRectFilled
                    (
                        pos,
                        byteWidth,
                        ImGui.ColorConvertFloat4ToU32(HighlightsArr[ByteDataArr[index].HighlightIdx].Color)
                    );

                    // If this highlight was selected underline it
                    if (SelectedHighlight == ByteDataArr[index].HighlightIdx || HighlightsArr[ByteDataArr[index].HighlightIdx].IsSelected) {
                        drawList.AddRectFilled
                        (
                            pos + new Vector2(0, 15f),
                            byteWidth,
                            ImGui.ColorConvertFloat4ToU32(ImToolColors.HexSelectedUnderline)
                        );
                    }
                }

                if (ImGui.IsMouseHoveringRect(pos, pos + ByteDisplaySize)) {
                    var drawList = ImGui.GetWindowDrawList();
                    drawList.AddRectFilled
                    (
                        pos + new Vector2(0, 15f),
                        pos + ByteDisplaySize,
                        ImGui.ColorConvertFloat4ToU32(ImToolColors.HexHovered)
                    );

                    HoveredIdx = index;

                    if (HoveredIdx != LastHoveredIdx) {
                        SetParsedValues(HoveredIdx);
                        LastHoveredIdx = HoveredIdx;
                    }

                    // Tooltip
                    var hasHighlight = ByteDataArr[index].HighlightIdx != ushort.MaxValue;
                    if (hasHighlight || ShowParsedValuesInTT) {
                        if (ShowParsedValuesInTT) {
                            ImGui.SetNextWindowSize(new Vector2(500f, 0));
                        }

                        ImGui.BeginTooltip();

                        SelectedHighlight = -1;
                        if (hasHighlight) {
                            SelectedHighlight = ByteDataArr[index].HighlightIdx;
                            ImGui.Text(HighlightsArr[ByteDataArr[index].HighlightIdx].HoverName);
                            ImGui.SameLine();
                            ImGui.Text($"({HighlightsArr[ByteDataArr[index].HighlightIdx].Offset}, {HighlightsArr[ByteDataArr[index].HighlightIdx].Length})");
                            OnHighlightSectionHover?.Invoke(HighlightsArr[ByteDataArr[index].HighlightIdx]);
                        }

                        if (ShowParsedValuesInTT) {
                            DrawParsedValues(2);
                        }
                    }


                    OnByteHoverTooltip?.Invoke(index);

                    if (hasHighlight || ShowParsedValuesInTT) {
                        ImGui.EndTooltip();
                    }
                }

                ImGuiNative.igText(LookupB + (ByteDataArr[index].Byte * 4));

                if (x < BYTE_WIDTH - 1) {
                    ImGui.SameLine();
                    if (x == middleByte) {
                        ImGui.Spacing();
                        ImGui.SameLine();
                    }
                }
            }
        }

        private void AddParsedValueRow(string name, string value)
        {
            ImGui.TableNextColumn();
            ImGui.Text(name);
            ImGui.TableNextColumn();
            ImGui.Text(value);
        }

        private void SetParsedValues(int idx)
        {
            var length = idx + 8 >= Bytes.Length ? Bytes.Length - idx : 8;
            var data   = new byte[8];
            Bytes.AsSpan().Slice(idx, length).CopyTo(data.AsSpan());

            ParsedValues = MemoryMarshal.Cast<byte, ParsedValuesStruct>(data)[0];
        }

        public struct HighlightSection
        {
            public int     Offset;
            public int     Length;
            public Vector4 Color;
            public string  HoverName;
            public bool    IsSelected;
        }

        public struct ByteData
        {
            public byte   Byte;
            public string HexStr;
            public ushort HighlightIdx;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ParsedValuesStruct
        {
            [FieldOffset(0)] public sbyte  SByte;
            [FieldOffset(0)] public byte   Byte;
            [FieldOffset(0)] public short  Short;
            [FieldOffset(0)] public ushort Ushort;
            [FieldOffset(0)] public int    Int;
            [FieldOffset(0)] public uint   Uint;
            [FieldOffset(0)] public long   Long;
            [FieldOffset(0)] public ulong  ULong;
            [FieldOffset(0)] public float  Float;
            [FieldOffset(0)] public double Double;

            public float QuantFloat
            {
                get
                {
                    var result = (1.0f / 32767f) * Ushort;
                    if (result >= 0) {
                        result = (result - 1f) * -1f;
                    }

                    return result;
                }
            }

            public float HalfFloat
            {
                get
                {
                    HalfLookup.FloatUIntMapLookup.UInt =
                        HalfLookup.Mantissa[HalfLookup.Offset[Ushort >> 10] + (Ushort & 0x3ff)] +
                        HalfLookup.Exponent[Ushort >> 10];
                    return HalfLookup.FloatUIntMapLookup.Float;
                }
            }
        }
    }
}


// Taken from Bitter

namespace Bitter
{
    static class HalfLookup
    {
        public static FloatByteMap FloatByteMapLookup = new FloatByteMap();
        public static FloatUIntMap FloatUIntMapLookup = new FloatUIntMap();

        public static uint[]   Mantissa = new uint[2048];
        public static uint[]   Exponent = new uint[64];
        public static ushort[] Offset   = new ushort[64];
        public static ushort[] Base     = new ushort[512];
        public static sbyte[]  Shift    = new sbyte[512];

        static HalfLookup()
        {
            // mantissa
            uint _fraction           = 0x800000;
            uint _fractionComplement = 0xFF7FFFFF;
            uint _bias               = 0x38800000;
            Mantissa[0]    = 0;
            Mantissa[1024] = 0x38000000;
            for (uint i = 1; i < 1024; i++) {
                uint mantissa = i << 13;

                Mantissa[i + 1024] = (0x38000000 + mantissa);

                uint exponent = 0;
                while ((mantissa & _fraction) == 0) {
                    mantissa <<= 1;
                    exponent -=  _fraction;
                }

                mantissa &= _fractionComplement;
                exponent += _bias;

                Mantissa[i] = mantissa | exponent;
            }

            // exponents
            Exponent[0]  = 0;
            Exponent[31] = 0x47800000;
            Exponent[32] = 0x80000000;
            Exponent[63] = 0xc7800000;
            for (uint i = 1; i < 31; i++) {
                uint x = i << 23;
                Exponent[i]      = x;
                Exponent[i + 32] = (0x80000000 + x);
            }

            // offsets
            Offset[0]  = 0;
            Offset[32] = 0;
            for (uint i = 1; i < 32; i++) {
                Offset[i]      = 1024;
                Offset[i + 32] = 1024;
            }

            // base and shift
            for (uint i = 0; i < 256; ++i) {
                sbyte e = (sbyte) (127 - i);
                if (e > 24) {
                    Base[i  | 0x000] = 0x0;
                    Base[i  | 0x100] = 0x8000;
                    Shift[i | 0x000] = 24;
                    Shift[i | 0x100] = 24;
                }
                else if (e > 14) {
                    Base[i  | 0x000] = (ushort) (0x0400 >> (18 + e));
                    Base[i  | 0x100] = (ushort) ((0x0400 >> (18 + e)) | 0x8000);
                    Shift[i | 0x000] = (sbyte) (e - 1);
                    Shift[i | 0x100] = (sbyte) (e - 1);
                }
                else if (e > -16) {
                    Base[i  | 0x000] = (ushort) ((15 - e) << 10);
                    Base[i  | 0x100] = (ushort) (((15 - e) << 10) | 0x8000);
                    Shift[i | 0x000] = 13;
                    Shift[i | 0x100] = 13;
                }
                else if (e > -128) {
                    Base[i  | 0x000] = 0x7C00;
                    Base[i  | 0x100] = 0xFC00;
                    Shift[i | 0x000] = 24;
                    Shift[i | 0x100] = 24;
                }
                else {
                    Base[i  | 0x000] = 0x7C00;
                    Base[i  | 0x100] = 0xFC00;
                    Shift[i | 0x000] = 13;
                    Shift[i | 0x100] = 13;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatByteMap
    {
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public byte  b0;
        [FieldOffset(1)] public byte  b1;
        [FieldOffset(2)] public byte  b2;
        [FieldOffset(3)] public byte  b3;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatUIntMap
    {
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public uint  UInt;
    }
}