using Veldrid.Sdl2;
using SDL_SystemCursor = ImTool.SDL.SDL_SystemCursor;

namespace ImTool
{
    public static class SystemCursors
    {
        public static SDL_Cursor Arrow = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        public static SDL_Cursor IBeam = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
        public static SDL_Cursor Wait = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT);
        public static SDL_Cursor Crosshair = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR);
        public static SDL_Cursor WaitArrow = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAITARROW);
        public static SDL_Cursor SizeNWSE = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
        public static SDL_Cursor SizeNESW = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
        public static SDL_Cursor SizeWE = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
        public static SDL_Cursor SizeNS = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
        public static SDL_Cursor SizeALL = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
        public static SDL_Cursor No = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO);
        public static SDL_Cursor Hand = SDL.SDL.SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND);
    }
}