using System;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;

namespace ImTool
{
    public static class ImGuiEx
    {
        public static bool BeginPopupModal(string name, ImGuiWindowFlags flags)
        {
            unsafe
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* nativeName = null;
                if (byteCount > 0)
                {
                    Span<byte> alloc = stackalloc byte[byteCount + 1];
                    nativeName = (byte*)Unsafe.AsPointer(ref alloc.GetPinnableReference());
                    Encoding.UTF8.GetBytes(name, alloc);
                }
                byte ret = ImGuiNative.igBeginPopupModal(nativeName, null, flags);
                return ret != 0;
            }
        }
    }
}