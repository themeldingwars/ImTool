using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid.Sdl2;

namespace ImTool.SDL
{
    public static unsafe partial class SDL
    {
        public const int SDL_ENABLE = 1;
        public const int SDL_DISABLE = 0;
        public const int SDL_IGNORE = 0;
        public const int SDL_QUERY = -1;

        public static T LoadFunction<T>(string name)
        {
            return Sdl2Native.LoadFunction<T>(name);
        }
        public static T LoadFunction<T>()
        {
            string name = typeof(T).Name;
            if(name.EndsWith("_t", StringComparison.InvariantCulture))
            {
                name = name.Substring(0, name.Length - 2);
            }
            return LoadFunction<T>(name);
        }
    }

    public enum SDL_bool
    {
        SDL_FALSE,
        SDL_TRUE
    }
}
