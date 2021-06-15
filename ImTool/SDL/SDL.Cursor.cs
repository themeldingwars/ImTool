using System;
using System.Runtime.InteropServices;

namespace ImTool.SDL
{
    using SDL_Cursor = IntPtr;

    public static unsafe partial class SDL
    {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Cursor SDL_CreateSystemCursor_t(SDL_SystemCursor cursor);
        private static SDL_CreateSystemCursor_t p_sdl_CreateSystemCursor = LoadFunction<SDL_CreateSystemCursor_t>();
        public static SDL_Cursor SDL_CreateSystemCursor(SDL_SystemCursor cursor) => p_sdl_CreateSystemCursor(cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_FreeCursor_t(SDL_Cursor cursor);
        private static SDL_FreeCursor_t p_sdl_FreeCursor = LoadFunction<SDL_FreeCursor_t>();
        public static void SDL_FreeCursor(SDL_Cursor cursor) => p_sdl_FreeCursor(cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetCursor_t(SDL_Cursor cursor);
        private static SDL_SetCursor_t p_sdl_SetCursor = LoadFunction<SDL_SetCursor_t>();
        public static void SDL_SetCursor(SDL_Cursor cursor) => p_sdl_SetCursor(cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_ShowCursor_t(int toggle);
        private static SDL_ShowCursor_t p_sdl_ShowCursor = LoadFunction<SDL_ShowCursor_t>();
        public static void SDL_ShowCursor(int toggle) => p_sdl_ShowCursor(toggle);

    }

    public enum SDL_SystemCursor : uint
    {
        SDL_SYSTEM_CURSOR_ARROW,
        SDL_SYSTEM_CURSOR_IBEAM,
        SDL_SYSTEM_CURSOR_WAIT,
        SDL_SYSTEM_CURSOR_CROSSHAIR,
        SDL_SYSTEM_CURSOR_WAITARROW,
        SDL_SYSTEM_CURSOR_SIZENWSE,
        SDL_SYSTEM_CURSOR_SIZENESW,
        SDL_SYSTEM_CURSOR_SIZEWE,
        SDL_SYSTEM_CURSOR_SIZENS,
        SDL_SYSTEM_CURSOR_SIZEALL,
        SDL_SYSTEM_CURSOR_NO,
        SDL_SYSTEM_CURSOR_HAND,
        SDL_NUM_SYSTEM_CURSORS
    }
}
