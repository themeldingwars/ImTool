using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class Window : IDisposable
    {
        private Configuration config;
        public delegate void ExitDelegate();
        public delegate void GlobalMenuBarOverrideDelegate();

        public GlobalMenuBarOverrideDelegate OnSubmitGlobalMenuBarOverride;
        
        private List<Tab> tabs = new();
        private List<WindowButton> windowButtons = new();
        private Tab activeTab;
        private bool activeTabGotMainMenu;
        private bool disposed = false;

        private Sdl2Window window;
        private GraphicsDevice graphicsDevice;
        private CommandList commandList;
        private ImGuiController controller;
        
        private bool vsync;
        private bool restartGD;

        private int titlebarHeight = 28;
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
        private Vector2? windowStartResizePosition;
        private Vector2? windowStartResizeSize;
        private Vector2 windowMinSize;
        private bool mouseDownOnTitlebar;
        private Rect.Edge mouseDownOnEdge;

        private int dockingMonitor = -1;
        private Rect.Edge dockingEdge;

        public ExitDelegate OnExit;
        public WindowState WindowState
        {
            get { return config.WindowState; }
            set
            {
                if (value != config.WindowState)
                {
                    config.PreviousWindowState = config.WindowState;
                    config.WindowState = value;
                    config.Save();
                    UpdateWindowState();
                }
            }
        }
        
        internal Window(Configuration config)
        {
            this.config = config;
            ThemeManager.OnThemeChanged += OnThemeChange;
            MonitorInfo.OnChange += OnMonitorInfoChange;

            windowBounds = new Rect();
            windowMinSize = config.MinimumWindowSize;
            contentBounds = new Rect();
            windowBorderColor = new uint[4];
            mouseDownOnEdge = Rect.Edge.None;

            windowButtonSize = new Vector2(windowBtnWidth, titlebarHeight - borderThickness - 2);
            
            SetupGD();
            
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
            
            ThemeManager.Initialize(config);
            CorporateGrey.Generate(Path.Combine(config.ToolDataPath, "Themes", "CorporateGrey.json"));
            ThemeManager.ReloadThemes();
            ThemeManager.SetTheme(config.Theme);
        }

        private void SetupGD()
        {
            if(controller != null)
                controller.Dispose();
            
            if(commandList != null)
                commandList.Dispose();
            
            if(graphicsDevice != null)
                graphicsDevice.Dispose();

            if (window != null)
            {
                window.Close();
                window = null;
            }

            string iniFile = Path.Combine(config.ToolDataPath, "Config", AppDomain.CurrentDomain.FriendlyName + ".ImGui.ini");
            SDL_WindowFlags flags =  SDL_WindowFlags.Shown  | SDL_WindowFlags.Borderless | (config.GraphicsBackend == GraphicsBackend.OpenGL || config.GraphicsBackend == GraphicsBackend.OpenGLES ? SDL_WindowFlags.OpenGL : 0);
            window = new Sdl2Window(config.Title, config.NormalWindowBounds.X, config.NormalWindowBounds.Y, config.NormalWindowBounds.Width, config.NormalWindowBounds.Height, flags, false);
            graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, new GraphicsDeviceOptions(false, null, config.VSync), config.GraphicsBackend);
            commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            controller = new ImGuiController(graphicsDevice, window, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height, iniFile);
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

            while (window.Exists)
            {
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
                
                
                InputSnapshot snapshot = null;
                Sdl2Events.ProcessEvents();
                snapshot = window.PumpEvents();
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

        private Stopwatch sw = Stopwatch.StartNew();

        private void Draw()
        {
            commandList.Begin();
            controller.Render(graphicsDevice, commandList, config.PowerSaving);
            commandList.End();
            graphicsDevice.SubmitCommands(commandList);
            controller.Swap(graphicsDevice, config.PowerSaving);
        }

        private void UpdateWindow()
        {
            if(window.Focused)
            {
                HandleWindowResizing();
                HandleWindowDragging();
                HandleMousePointer();

                if (!mouseDownOnTitlebar && mouseDownOnEdge == Rect.Edge.None)
                {
                    if(window.Y < 0)
                    {
                        window.Y = 0;
                    }
                    else
                    {
                        int bottom = MonitorInfo.UsableBounds[currentMonitor].Bottom - titlebarHeight;
                        if(window.Y > bottom)
                        {
                            window.Y = bottom;
                        }
                    }
                }
            }

            if (vsync != config.VSync)
            {
                config.VSync = vsync;
                graphicsDevice.SyncToVerticalBlank = vsync;
            }

            bool resized = windowBounds.Width != window.Width || windowBounds.Height != window.Height;

            if (resized || windowBounds.X != window.Bounds.Left || windowBounds.Y != window.Bounds.Top)
            {
                windowBounds.X = window.Bounds.Left;
                windowBounds.Y = window.Bounds.Top;
                windowBounds.Width = window.Bounds.Width;
                windowBounds.Height = window.Bounds.Height;

                titlebarBounds.X = windowBounds.Left + borderThickness;
                titlebarBounds.Y = windowBounds.Top + borderThickness;
                titlebarBounds.Width = windowBounds.Width - (borderThickness * 2);
                titlebarBounds.Height = titlebarHeight;

                contentBounds.X = windowBounds.Left + borderThickness;
                contentBounds.Y = titlebarBounds.Bottom;
                contentBounds.Width = windowBounds.Width - (borderThickness * 2);
                contentBounds.Height = windowBounds.Height - titlebarBounds.Height - (borderThickness * 2);
            }

            if (resized)
            {
                graphicsDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                controller.WindowResized(window.Width, window.Height);
            }

            int old = currentMonitor;
            currentMonitor = -1;
            for(int i = 0; i < MonitorInfo.UsableBounds.Length; i++ )
            {
                if(MonitorInfo.UsableBounds[i].Contains((int)windowBounds.X, (int)windowBounds.Y))
                {
                    currentMonitor = i;
                    break;
                }
            }
            if(currentMonitor == -1)
            {
                currentMonitor = old;
            }
        }
        
        private void HandleMousePointer()
        {
            switch (ImGui.GetMouseCursor())
            {
                case ImGuiMouseCursor.None:
                    //SDL.SDL.SDL_SetCursor(SystemCursors.None);
                    break;
                case ImGuiMouseCursor.Arrow:
                    SDL.SDL.SDL_SetCursor(SystemCursors.Arrow);
                    break;
                case ImGuiMouseCursor.TextInput:
                    SDL.SDL.SDL_SetCursor(SystemCursors.IBeam);
                    break;
                case ImGuiMouseCursor.ResizeAll:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeALL);
                    break;
                case ImGuiMouseCursor.ResizeNS:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNS);
                    break;
                case ImGuiMouseCursor.ResizeEW:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeWE);
                    break;
                case ImGuiMouseCursor.ResizeNESW:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNESW);
                    break;
                case ImGuiMouseCursor.ResizeNWSE:
                    SDL.SDL.SDL_SetCursor(SystemCursors.SizeNWSE);
                    break;
                case ImGuiMouseCursor.Hand:
                    SDL.SDL.SDL_SetCursor(SystemCursors.Hand);
                    break;
                case ImGuiMouseCursor.NotAllowed:
                    SDL.SDL.SDL_SetCursor(SystemCursors.No);
                    break;
            }
        }
        private void HandleWindowResizing()
        {
            if(WindowState != WindowState.Normal || mouseDownOnTitlebar)
            {
                return;
            }

            Rect.Edge edge = windowBounds.EdgeContains(ImGui.GetMousePos(), 4);
            Rect.Edge lastEdge = edge;
            if(mouseDownOnEdge != Rect.Edge.None)
            {
                lastEdge = mouseDownOnEdge;
            }
            
            switch (lastEdge)
            {
                case Rect.Edge.Top:
                case Rect.Edge.Bottom:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
                    break;
                case Rect.Edge.Left:
                case Rect.Edge.Right:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                    break;
                case Rect.Edge.TopLeftCorner:
                case Rect.Edge.BottomRightCorner:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNWSE);
                    break;
                case Rect.Edge.TopRightCorner:
                case Rect.Edge.BottomLeftCorner:
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNESW);
                    break;
                default:
                    break;
            }

            if (windowStartResizePosition == null && ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {           
                if (mouseDownOnEdge == Rect.Edge.None && edge != Rect.Edge.None )
                {
                    mouseDownOnEdge = edge;
                    if (!ImGui.IsAnyItemHovered())
                    {
                        windowStartResizePosition = window.Bounds.Position;
                        windowStartResizeSize = window.Bounds.Size;
                    }
                }
            }
            else if (mouseDownOnEdge != Rect.Edge.None && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                mouseDownOnEdge = Rect.Edge.None;
                windowStartResizePosition = null;
                windowStartResizeSize = null;
            }

            if (windowStartResizePosition != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 delta = ImGui.GetMouseDragDelta();

                float width = windowStartResizeSize.Value.X;
                float height = windowStartResizeSize.Value.Y;
                Vector2 pos = (Vector2)windowStartResizePosition;

                if (mouseDownOnEdge.HasFlag(Rect.Edge.Right))
                {
                    width += delta.X;
                }
                else if (mouseDownOnEdge.HasFlag(Rect.Edge.Left))
                {
                    width -= delta.X;
                    pos.X += delta.X;
                }

                if (mouseDownOnEdge.HasFlag(Rect.Edge.Bottom))
                {
                    height += delta.Y;
                }
                else if (mouseDownOnEdge.HasFlag(Rect.Edge.Top))
                {
                    height -= delta.Y;
                    pos.Y += delta.Y;
                }

                if(width >= windowMinSize.X)
                {
                    window.Width = (int)width;
                    window.X = (int)pos.X;
                }

                if(height >= windowMinSize.Y)
                {
                    window.Height = (int)height;
                    window.Y = (int)pos.Y;
                }
            }
        }
        private void HandleWindowDragging()
        {
            if(mouseDownOnEdge != Rect.Edge.None)
            {
                return;
            }

            if (windowStartDragPosition == null && ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                if (!mouseDownOnTitlebar && titlebarBounds.Contains(ImGui.GetMousePos()))
                {
                    mouseDownOnTitlebar = true;
                    if (!ImGui.IsAnyItemHovered())
                    {
                        windowStartDragPosition = window.Bounds.Position;
                    }
                }
            }
            else if (mouseDownOnTitlebar && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                mouseDownOnTitlebar = false;
                windowStartDragPosition = null;

                if (dockingMonitor != -1)
                {
                    switch (dockingEdge)
                    {
                        case Rect.Edge.Top:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.Maximized;
                            break;
                        case Rect.Edge.Left:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedLeft;
                            break;
                        case Rect.Edge.Right:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedRight;
                            break;
                        case Rect.Edge.TopLeftCorner:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedUpperLeft;
                            break;
                        case Rect.Edge.BottomRightCorner:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedLowerRight;
                            break;
                        case Rect.Edge.TopRightCorner:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedUpperRight;
                            break;
                        case Rect.Edge.BottomLeftCorner:
                            currentMonitor = dockingMonitor;
                            WindowState = WindowState.DockedLowerLeft;
                            break;
                        default:
                            break;
                    }

                    dockingMonitor = -1;
                    dockingEdge = Rect.Edge.None;
                }

            }

            if (windowStartDragPosition != null && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                if (WindowState != WindowState.Normal)
                {
                    Vector2 mousePos = ImGui.GetMousePos();
                    float xf = (mousePos.X - window.Bounds.Left) / (window.Bounds.Right - window.Bounds.Left);

                    WindowState = WindowState.Normal;

                    window.X = (int)(mousePos.X - (window.Width * xf));
                    window.Y = (int)windowStartDragPosition.Value.Y;
                    windowStartDragPosition = window.Bounds.Position;

                }

                Vector2 pos = (Vector2)windowStartDragPosition;
                pos += ImGui.GetMouseDragDelta();
                window.X = (int)pos.X;
                window.Y = (int)pos.Y;

                dockingMonitor = -1;
                for (int i = 0; i < MonitorInfo.Bounds.Length; i++)
                {
                    Rect.Edge edge = MonitorInfo.Bounds[i].EdgeContains(ImGui.GetMousePos(), 8);
                    if(edge != Rect.Edge.None)
                    {
                        dockingMonitor = i;
                        dockingEdge = edge;
                        break;
                    }
                }
            }
        }
        private void UpdateWindowState()
        {
            if(config.WindowState != WindowState.Normal)
            {
                StoreNormalBounds();
            }

            UpdateWindowBorderThickness();

            if(currentMonitor == -1 && MonitorInfo.Count > 0)
            {
                currentMonitor = 0;
            }

            Rect bounds = GetDockingBounds(currentMonitor, config.WindowState);
            window.X = bounds.X;
            window.Y = bounds.Y;
            window.Height = bounds.Height;
            window.Width = bounds.Width;
        }
        
        private Rect GetDockingBounds(int monitor, WindowState windowState)
        {
            Rect rect = new Rect();
            Rect bounds = MonitorInfo.UsableBounds[monitor];

            switch (windowState)
            {
                case WindowState.Normal:
                    rect.X = config.NormalWindowBounds.X;
                    rect.Y = config.NormalWindowBounds.Y;
                    rect.Width = config.NormalWindowBounds.Width;
                    rect.Height = config.NormalWindowBounds.Height;
                    break;

                case WindowState.Maximized:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width;
                    rect.Height = bounds.Height;
                    break;

                case WindowState.DockedLeft:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height;
                    break;

                case WindowState.DockedUpperLeft:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case WindowState.DockedLowerLeft:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y + bounds.Height / 2;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case WindowState.DockedRight:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height;
                    break;

                case WindowState.DockedUpperRight:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case WindowState.DockedLowerRight:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y + bounds.Height / 2;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;
            }
            return rect;
        }
        private Rect GetDockingBounds(int monitor, Rect.Edge edge)
        {
            Rect rect = new Rect();
            Rect bounds = MonitorInfo.UsableBounds[monitor];

            switch (edge)
            {
                case Rect.Edge.Top:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width;
                    rect.Height = bounds.Height;
                    break;

                case Rect.Edge.Left:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height;
                    break;

                case Rect.Edge.TopLeftCorner:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case Rect.Edge.BottomLeftCorner:
                    rect.X = bounds.X;
                    rect.Y = bounds.Y + bounds.Height / 2;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case Rect.Edge.Right:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height;
                    break;

                case Rect.Edge.TopRightCorner:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;

                case Rect.Edge.BottomRightCorner:
                    rect.X = bounds.X + bounds.Width / 2;
                    rect.Y = bounds.Y + bounds.Height / 2;
                    rect.Width = bounds.Width / 2;
                    rect.Height = bounds.Height / 2;
                    break;
            }
            return rect;
        }

        private void UpdateWindowBorderThickness()
        {
            borderThickness = WindowState == WindowState.Maximized ? 0 : config.BorderSize;
        }

        private void StoreNormalBounds()
        {
            config.NormalWindowBounds.X = window.Bounds.X;
            config.NormalWindowBounds.Y = window.Bounds.Y;
            config.NormalWindowBounds.Width = window.Bounds.Width;
            config.NormalWindowBounds.Height = window.Bounds.Height;
            config.Save();
        }
        private void UpdateBorderColor()
        {
            byte[] begin = NormalizedVector4ToBytes(ThemeManager.Current.WindowBorderGradientBegin);
            byte[] end = NormalizedVector4ToBytes(ThemeManager.Current.WindowBorderGradientEnd);
            byte[] middle = new byte[]
            {
                (byte)((begin[0] + end[0])/2),
                (byte)((begin[1] + end[1])/2),
                (byte)((begin[2] + end[2])/2),
                (byte)((begin[3] + end[3])/2)
            };

            windowBorderColor[0] = BitConverter.ToUInt32(begin);
            windowBorderColor[1] = BitConverter.ToUInt32(middle);
            windowBorderColor[2] = BitConverter.ToUInt32(end);
            windowBorderColor[3] = BitConverter.ToUInt32(middle);
        }
        private void OnMonitorInfoChange()
        {

            /*Console.WriteLine($"MonitorInfo changed");
            Console.WriteLine($"  Count : {MonitorInfo.Count}");
            Rect[] bounds = MonitorInfo.UsableBounds;
            for (int i = 0; i < MonitorInfo.Count; i++)
            {
                Console.WriteLine($"  [{i}] : {bounds[i].Position.X}, {bounds[i].Position.Y}");
            }
            */
        }

        private void SubmitWindowButtons()
        {
            ThemeManager.ApplyOverride(ImGuiStyleVar.FrameRounding, 0);
            ThemeManager.ApplyOverride(ImGuiCol.Button, new Vector4());
            ImGui.SetCursorPos(WindowButtonPosition(1));

            if (ImGui.Button("×", windowButtonSize))
            {
                Exit();
            }

            ImGui.SetCursorPos(WindowButtonPosition(2));
            if (ImGui.Button("¤", windowButtonSize))
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = config.PreviousWindowState;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
            }

            ImGui.SetCursorPos(WindowButtonPosition(3));
            if (ImGui.Button("-", windowButtonSize))
            {
                window.WindowState = Veldrid.WindowState.Minimized;
            }
            
            ImGui.SetCursorPos(WindowButtonPosition(4));
            if (ImGui.Button("«", windowButtonSize))
            {
                ImGui.OpenPopup("imtool_setting_popup");
            }

            if (windowButtons.Count > 0)
            {
                Vector2 pos = WindowButtonPosition(4);
                Vector2 separatorTop = windowBounds.Position + pos + new Vector2(-2, 1);
                Vector2 separatorBottom = separatorTop + new Vector2(0, windowButtonSize.Y - 2);
                ImGui.GetWindowDrawList().AddLine(separatorTop, separatorBottom, 0x33000000, 1);
                pos.X -= 3;
                
                foreach (WindowButton windowButton in windowButtons)
                {
                    Vector2 size = windowButtonSize;
                    size.X = ImGui.CalcTextSize(windowButton.Text).X + 24;
                    pos.X -= (size.X + 1);
                
                    ImGui.SetCursorPos(pos);
                    if (ImGui.Button(windowButton.Text, size))
                    {
                        windowButton.OnClicked?.Invoke();
                    }

                    pos.X -= 1;
                }
            }
            
            ThemeManager.ResetOverride(ImGuiStyleVar.FrameRounding);
            ThemeManager.ResetOverride(ImGuiCol.Button);
            
            if(ImGui.IsPopupOpen("imtool_setting_popup"))
            {
                ImGui.SetNextWindowPos(new Vector2(windowBounds.Right - settingsWidth - 2, windowBounds.Top + titlebarHeight + 2));
                ImGui.SetNextWindowSize(new Vector2(settingsWidth, windowBounds.Height - titlebarHeight - 4));
            }
            
            if (ImGui.BeginPopup("imtool_setting_popup", ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove))
            {
                SubmitSettingPopup();
                ImGui.EndPopup();
            }
            
        }

        private void SubmitSettingPopup()
        {
            ImTool.Widgets.RenderTitle("ImTool Settings");
            if (ImGui.BeginCombo("Theme", ThemeManager.Current.Name))
            {
                foreach (string theme in ThemeManager.Themes.Keys)
                {
                    if (ImGui.Selectable(theme, ThemeManager.Current.Name == theme))
                    {
                        ThemeManager.SetTheme(theme);
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SliderInt("Target FPS", ref config.FpsLimit, 20, 200);
            if (ImGui.BeginCombo("Graphics backend", config.GraphicsBackend.ToString()))
            {
                foreach (GraphicsBackend backend in Enum.GetValues<GraphicsBackend>())
                {
                    if (ImGui.Selectable(backend.ToString(), config.GraphicsBackend == backend))
                    {
                        config.GraphicsBackend = backend;
                        restartGD = true;
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.Checkbox("Enable VSync  ", ref vsync);
            ImGui.SameLine();
            ImGui.Checkbox("Experimental power saving", ref config.PowerSaving);
            ImGui.Separator();
            if (ImGui.Button("Check for updates :) "))
            {
                
            }

            ImGui.NewLine();
            
            Widgets.RenderTitle($"{config.Title} Settings");
            foreach (Tab tab in tabs)
            {
                tab.SubmitSettings(tab == activeTab);
            }
        }
        
        private unsafe void SubmitUI()
        {
            
            ImGui.SetNextWindowSize(windowBounds.Size);
            ImGui.SetNextWindowPos(windowBounds.Position);
            MainWindowStyleOverrides(true);
            ImGui.Begin("MainWindow",  ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoFocusOnAppearing |  ImGuiWindowFlags.NoBringToFrontOnFocus);
            MainWindowStyleOverrides(false);

            if (borderThickness > 0)
            {
                ImGui.GetWindowDrawList().AddRectFilledMultiColor(windowBounds.Position, windowBounds.MaxPosition, windowBorderColor[0], windowBorderColor[1], windowBorderColor[2], windowBorderColor[3]);
            }

            ImGui.GetWindowDrawList().AddRectFilled(titlebarBounds.Position, titlebarBounds.MaxPosition, titlebarColor);
            
            SubmitWindowButtons();

            ImGui.SetCursorPos(new Vector2(borderThickness + 1, titlebarHeight - 19));
            ImGui.BeginTabBar("Tabs");
            
            ImGui.GetWindowDrawList().AddRectFilled(contentBounds.Position, contentBounds.MaxPosition, ImGui.GetColorU32(ImGuiCol.WindowBg));

            foreach (Tab tab in tabs)
            {
                TabStyleOverrides(true);
                if (ImGui.BeginTabItem(tab.Name))
                {
                    if (activeTab != tab)
                    {
                        activeTab = tab;
                    }

                    bool hasMainMenuBar = false;
                    if (tab.IsMainMenuOverridden)
                    {
                        hasMainMenuBar = true;
                        BeginMainMenuBar();
                        tab.SubmitMainMenu();
                        EndMainMenuBar();
                    }
                    else if (OnSubmitGlobalMenuBarOverride != null)
                    {
                        hasMainMenuBar = true;
                        BeginMainMenuBar();
                        OnSubmitGlobalMenuBarOverride();
                        EndMainMenuBar();
                    }

                    Vector2 dockPos = ImGui.GetCursorPos() + (hasMainMenuBar ? new Vector2(1, 17) : new Vector2(1, -3));
                    Vector2 dockSize = hasMainMenuBar ? contentBounds.Size - new Vector2(0, 20): contentBounds.Size;
                    
                    ImGui.SetCursorPos(dockPos);
                    ImGui.DockSpace(ImGui.GetID(tab.Name+"TabDockspace"), dockSize, ImGuiDockNodeFlags.None);
                    
                    //ImGui.SetNextWindowSize(contentBounds.Size);
                    //ImGui.SetNextWindowPos(contentBounds.Position);
                    
                    //ImGui.Begin(tab.Name + "Window", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.DockNodeHost | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoFocusOnAppearing |  ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.AlwaysAutoResize);
                    
                    //ImGui.SetNextWindowSize(contentBounds.Size);
                    //ImGui.SetNextWindowPos(contentBounds.Position);
                    
                    

                    TabStyleOverrides(false);
                    tab.SubmitContent();
                    //ImGui.End();
                    ImGui.EndTabItem();
                }
            }
            TabStyleOverrides(false);
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
        
        public static void MainWindowStyleOverrides(bool apply)
        {
            if(apply)
            {
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowBorderSize, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowPadding, default(Vector2));
            }
            else
            {
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowPadding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowBorderSize);
            }
        }
        public static void TabStyleOverrides(bool apply)
        {
            if (apply)
            {
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.WindowBorderSize, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.TabRounding, 0);
                ThemeManager.ApplyOverride(ImGuiStyleVar.ItemInnerSpacing, new Vector2(1, 0));
                ThemeManager.ApplyOverride(ImGuiCol.TabActive, ThemeManager.Current[ImGuiCol.WindowBg]);
                ThemeManager.ApplyOverride(ImGuiCol.MenuBarBg, ThemeManager.Current[ImGuiCol.WindowBg]);
                ThemeManager.ApplyOverride(ImGuiCol.Tab, new Vector4(0.18f, 0.18f, 0.18f, 1.00f));
                
            }
            else
            {
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.WindowBorderSize);
                ThemeManager.ResetOverride(ImGuiStyleVar.TabRounding);
                ThemeManager.ResetOverride(ImGuiStyleVar.ItemInnerSpacing);
                ThemeManager.ResetOverride(ImGuiCol.TabActive);
                ThemeManager.ResetOverride(ImGuiCol.MenuBarBg);
                ThemeManager.ResetOverride(ImGuiCol.Tab);
            }
        }
        private void SubmitDemoTab()
        {
            ImGui.ShowMetricsWindow();
            ImGui.ShowDemoWindow();
            ImGui.Begin("Nodes test");
            imnodes.BeginNodeEditor();

            int t = 0;
            imnodes.BeginNode(0);
            imnodes.BeginNodeTitleBar();
            ImGui.TextUnformatted("Yes! :D");
            imnodes.EndNodeTitleBar();

            imnodes.BeginInputAttribute(0);
            ImGui.SetNextItemWidth(120);
            ImGui.DragInt("input stuff :>", ref t);
            imnodes.EndInputAttribute();
            imnodes.EndNode();

            imnodes.EndNodeEditor();
            ImGui.End();

            ImGui.Begin("Plot test");
            ImPlot.BeginPlot("Plot");

            ImPlot.PlotDummy("test?");
            var flaotValues = new float[] { 0.0f, 1f, 1.5f, 2.1f };
            ImPlot.PlotBars("test", ref flaotValues[0], flaotValues.Length);
            ImPlot.PlotLine("test line thing", ref flaotValues[0], flaotValues.Length);

            ImPlot.EndPlot();
            ImGui.End();

        }

        private void OnThemeChange()
        {
            UpdateBorderColor();
            byte[] btbc = NormalizedVector4ToBytes(ThemeManager.Current.TitlebarBackgroundColor);
            titlebarColor = BitConverter.ToUInt32(btbc);
        }

        private static byte[] NormalizedVector4ToBytes(Vector4 v)
        {
            return new byte[]
            {
                (byte)(v.Y * byte.MaxValue),
                (byte)(v.Z * byte.MaxValue),
                (byte)(v.W * byte.MaxValue),
                (byte)(v.X * byte.MaxValue),
            };
        }
        private Vector2 WindowButtonPosition(int n)
        {
            if (WindowState == WindowState.Maximized)
            {
                return new Vector2((windowBounds.Width - (windowBtnWidth + 1) * n), borderThickness + 1);
            }
            else
            {
                return new Vector2((windowBounds.Width - (windowBtnWidth + 1) * n) - 1, borderThickness + 1);
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
        
        public void AddTab(Tab tab)
        {
            if(!tabs.Contains(tab))
                tabs.Add(tab);
        }
        
        public void RemoveTab(Tab tab)
        {
            if(tabs.Contains(tab))
                tabs.Remove(tab);
        }

        
        public void AddWindowButton(string text, WindowButton.ClickedDelegate onClicked)
        {
            AddWindowButton(new WindowButton(text, onClicked));
        }
        
        public void AddWindowButton(WindowButton windowButton)
        {
            if(!windowButtons.Contains(windowButton))
                windowButtons.Add(windowButton);
        }
        
        public void RemoveWindowButton(WindowButton windowButton)
        {
            if(windowButtons.Contains(windowButton))
                windowButtons.Remove(windowButton);
        }
    }
}
