using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using imnodesNET;
using ImPlotNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImTool
{
    public partial class Window : IDisposable
    {
        private Configuration config;
        private Updater updater;
        public delegate void ExitDelegate();
        public delegate void GlobalMenuBarOverrideDelegate();
        public delegate void SubmitUIExtensionDelegate();

        public GlobalMenuBarOverrideDelegate OnSubmitGlobalMenuBarOverride;

        private bool disposed = false;

        private Sdl2Window window;
        private GraphicsDevice graphicsDevice;
        private CommandList commandList;
        private ImGuiController controller;
        
        private bool vsync;
        private bool restartGD;

        private int titlebarHeight => TitlebarDisabled ? 0 : 28;
        private int windowBtnWidth = 30;
        private int borderThickness = 0;
        private int currentMonitor = -1;
        private int settingsWidth = 340;

        private Rect windowBounds;
        private Rect titlebarBounds;
        private Vector2 windowButtonSize;
        private Rect contentBounds;

        private uint[] windowBorderColor;
        private uint titlebarColor;
        private Vector2? windowStartDragPosition;
        private Vector2 windowStartDragMousePosition;
        private Vector2? windowStartResizePosition;
        private Vector2 windowStartResizeMousePosition;
        private Vector2? windowStartResizeSize;
        private Vector2 windowMinSize;
        private bool mouseDownOnTitlebar;
        private Rect.Edge mouseDownOnEdge;

        private int dockingMonitor = -1;
        private Rect.Edge dockingEdge;
        
        public ExitDelegate OnExit;
        public SubmitUIExtensionDelegate OnSubmitUIExtension;

        public string Title;

        public GraphicsDevice GetGraphicsDevice() => graphicsDevice;
        public ImGuiController GetImGuiController() => controller;

        internal Window(Configuration config)
        {
            this.config = config;
            Title = config.Title ?? "Untitled";
            
            ThemeManager.OnThemeChanged += OnThemeChange;

            windowBounds = new Rect();
            windowMinSize = config.MinimumWindowSize;
            contentBounds = new Rect();
            windowBorderColor = new uint[4];
            mouseDownOnEdge = Rect.Edge.None;

            windowButtonSize = new Vector2(windowBtnWidth, titlebarHeight - borderThickness - 2);
            
            SetupGD();
            
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
            
            ThemeManager.Initialize(config);
        }
        
        public static async Task<Window> Create(Configuration config)
        {
            Exception ex = null;
            Window instance = null;
            Task.Run(async ()=>
            {
                await Task.Delay(16);
                try
                {
                    instance = new Window(config);
                }
                catch (Exception e)
                {
                    ex = e;
                    return;
                }
                
                instance.Run();
            });

            while (instance == null && ex == null)
            {
                await Task.Delay(16);
            }

            if (ex != null)
            {
                Console.WriteLine("An exception was thrown while initializing the ImTool window :<");
                Console.WriteLine(ex);
                return null;
            }
            
            return instance;
        }
        private void Run()
        {
            vsync = config.VSync;

            long previousFrameTicks = 0;
            Stopwatch sw = new Stopwatch();        
            sw.Start();
            MonitorInfo.Update();
            UpdateWindowBorderThickness();
            Draw();
            UpdateWindowState();

            try
            {
                while (window.Exists)
                {
                    // update window title
                    if (window.Title != Title)
                        window.Title = Title;
                    
                    long currentFrameTicks = sw.ElapsedTicks;
                    float deltaSeconds = (currentFrameTicks - previousFrameTicks) / (float)Stopwatch.Frequency;


                    if (!vsync)
                    {
                        float targetDelta = 1000f / config.FpsLimit;
                        float deltaMs = deltaSeconds * 1000;

                        if (deltaMs < targetDelta)
                        {
                            Thread.Sleep((int)(targetDelta-deltaMs));
                            currentFrameTicks = sw.ElapsedTicks;
                            deltaSeconds = (currentFrameTicks - previousFrameTicks) / (float)Stopwatch.Frequency;
                        }
                    }
                
                
                    previousFrameTicks = currentFrameTicks;
                
                
                    InputSnapshot snapshot = window.PumpEvents();
                    MonitorInfo.Update();

                    UpdateWindow();
                    controller.Update(deltaSeconds, snapshot);
                    SubmitUI();
                    if (!window.Exists)
                    {
                        break;
                    }

                    Draw();
                    if (restartGD)
                    {
                        config.WindowState = WindowState;
                        config.NormalWindowBounds = new Rect(window.X, window.Y, window.Width, window.Height);
                        config.Monitor = currentMonitor;
                    
                        restartGD = false;
                        SetupGD();
                    
                        ThemeManager.SetTheme(config.Theme);
      
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                throw;
            }

        }
        private void Exit()
        {
            config.WindowState = WindowState;
            config.NormalWindowBounds = new Rect(window.X, window.Y, window.Width, window.Height);
            config.Monitor = currentMonitor;
            
            OnExit?.Invoke();

            config.Save();
            Environment.Exit(-1);
        }
        
        private unsafe void SubmitUI()
        {
            if (FontManager.DefaultFont != "" && FontManager.Fonts.ContainsKey(FontManager.DefaultFont))
            {
                FontManager.PushFont(FontManager.DefaultFont);
            }

            ImGui.SetNextWindowSize(windowBounds.Size);
            ImGui.SetNextWindowPos(windowBounds.Position);
            MainWindowStyleOverrides(true);
            ImGui.Begin("MainWindow",  ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoDocking);
            MainWindowStyleOverrides(false);

            if (borderThickness > 0)
            {
                ImGui.GetWindowDrawList().AddRectFilledMultiColor(windowBounds.Position, windowBounds.MaxPosition, windowBorderColor[0], windowBorderColor[1], windowBorderColor[2], windowBorderColor[3]);
            }

            if (!TitlebarDisabled)
            {
                ImGui.GetWindowDrawList().AddRectFilled(titlebarBounds.Position, titlebarBounds.MaxPosition, titlebarColor);
                SubmitWindowButtons();
            }


            int tabHeight = 4 + (int)ImGui.CalcTextSize("ABCD").Y;
            int yStart = TitlebarDisabled ? 0 : titlebarHeight - tabHeight;
            
            ImGui.SetCursorPos(new Vector2(borderThickness + 1, yStart));
            ImGui.BeginTabBar("Tabs");
            
            ImGui.GetWindowDrawList().AddRectFilled(contentBounds.Position, contentBounds.MaxPosition, ImGui.GetColorU32(ImGuiCol.WindowBg));

            SubmitTabs();

            ImGui.EndTabBar();
            ImGui.End();

            if(dockingMonitor != -1)
            {
                Rect bounds = GetDockingBounds(dockingMonitor, dockingEdge);
                if (bounds.Size != new Vector2())
                {
                    // here
                }
            }

            OnSubmitUIExtension?.Invoke();
        }

        private void BeginMainMenuBar()
        {
            ImGuiViewportPtr vp = ImGui.GetMainViewport();
            vp.Pos.Y += titlebarHeight + borderThickness;
            vp.Pos.X += borderThickness;
            vp.Size.X -= borderThickness * 2;
            ImGui.BeginMainMenuBar();
        }
        private void EndMainMenuBar()
        {
            ImGui.EndMainMenuBar();
            ImGuiViewportPtr vp = ImGui.GetMainViewport();
            vp.Pos.Y -= titlebarHeight + borderThickness;
            vp.Pos.X -= borderThickness;
            vp.Size.X += borderThickness * 2;
        }
        
        public void SetUpdater(Updater updater)
        {
            this.updater = updater;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                graphicsDevice.Dispose();
                commandList.Dispose();
                controller.Dispose();
            }
            disposed = true;
        }
        ~Window()
        {
            Dispose(false);
        }
    }
}
