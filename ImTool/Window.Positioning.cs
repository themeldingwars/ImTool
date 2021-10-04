using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public partial class Window
    {
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
        
        private void UpdateWindow()
        {
            if(window.Focused)
            {
                HandleWindowResizing();
                HandleWindowDragging();
                HandleMouseCursor();

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
        
        private void StoreNormalBounds()
        {
            config.NormalWindowBounds.X = window.Bounds.X;
            config.NormalWindowBounds.Y = window.Bounds.Y;
            config.NormalWindowBounds.Width = window.Bounds.Width;
            config.NormalWindowBounds.Height = window.Bounds.Height;
            config.Save();
        }
    }
}