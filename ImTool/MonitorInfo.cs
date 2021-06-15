using Veldrid.Sdl2;

namespace ImTool
{
    public static class MonitorInfo
    {
        private unsafe delegate int SDL_GetDisplayBounds_t(int displayIndex, Rect* rect);
        private static SDL_GetDisplayUsableBounds_t p_sdl_GetDisplayBounds_t;

        private unsafe delegate int SDL_GetDisplayUsableBounds_t(int displayIndex, Rect* rect);
        private static SDL_GetDisplayUsableBounds_t p_sdl_GetDisplayUsableBounds_t;

        private delegate int SDL_GetNumVideoDisplays_t();
        private static SDL_GetNumVideoDisplays_t p_sdl_GetNumVideoDisplays;

        public delegate void Change();
        public static Change OnChange;

        public static int Count { get; private set; }

        public static Rect[] Bounds;
        public static Rect[] UsableBounds;

        private static Rect[] bounds;
        private static Rect[] usable;

        static MonitorInfo()
        {
            bounds = new Rect[0];
            usable = new Rect[0];
            p_sdl_GetNumVideoDisplays = Sdl2Native.LoadFunction<SDL_GetNumVideoDisplays_t>("SDL_GetNumVideoDisplays");
            p_sdl_GetDisplayBounds_t = Sdl2Native.LoadFunction<SDL_GetDisplayUsableBounds_t>("SDL_GetDisplayBounds");
            p_sdl_GetDisplayUsableBounds_t = Sdl2Native.LoadFunction<SDL_GetDisplayUsableBounds_t>("SDL_GetDisplayUsableBounds");
        }


        public static unsafe void Update()
        {

            int count = p_sdl_GetNumVideoDisplays();

            if(count != bounds.Length)
            {
                bounds = new Rect[count];
                usable = new Rect[count];
            }

            if (count > 0)
            {

                Rect b = new Rect();
                for (int i = 0; i < count; i++)
                {
                    p_sdl_GetDisplayBounds_t(i, &b);
                    bounds[i] = b;

                    p_sdl_GetDisplayUsableBounds_t(i, &b);                 
                    usable[i] = b;
                }
            }

            if(Count != count)
            {
                Count = count;
                Bounds = bounds;
                UsableBounds = usable;
                OnChange?.Invoke();
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if(usable[i] != UsableBounds[i] || bounds[i] != Bounds[i])
                {
                    Bounds = bounds;
                    UsableBounds = usable;
                    OnChange?.Invoke();
                    return;
                }
            }
        }
    }
}
