using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ImTool.SDL
{
    public partial class SDL
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate IntPtr SDL_GetGlobalMouseState_t(int* x, int* y);
        private static SDL_GetGlobalMouseState_t p_sdl_GetGlobalMouseState = LoadFunction<SDL_GetGlobalMouseState_t>();

        public static void SDL_GetGlobalMouseState(out int x, out int y)
        {
            int _x, _y;
            unsafe { p_sdl_GetGlobalMouseState(&_x, &_y); }
            x = _x;
            y = _y;
        }
        
        public static void SDL_GetGlobalMouseState(out Vector2 vec2)
        {
            int _x, _y;
            unsafe { p_sdl_GetGlobalMouseState(&_x, &_y); }
            vec2 = new Vector2(_x, _y);
        }
    }
}