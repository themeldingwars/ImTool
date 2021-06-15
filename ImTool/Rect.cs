using System;
using System.Numerics;
using Newtonsoft.Json;

namespace ImTool
{
    public struct Rect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        [JsonIgnore]
        public int Left => X;
        [JsonIgnore]
        public int Right => X + Width;
        [JsonIgnore]
        public int Top => Y;
        [JsonIgnore]
        public int Bottom => Y + Height;
        [JsonIgnore]
        public Vector2 Position { get => new Vector2(X, Y); set { X = (int)value.X; Y = (int)value.Y; }}
        [JsonIgnore]
        public Vector2 MaxPosition { get => new Vector2(Right, Bottom); }
        [JsonIgnore]
        public Vector2 Size { get => new Vector2(Width, Height); set { Width = (int)value.X; Height = (int)value.Y; } }

        public bool Contains(Vector2 point)
        {
            return Contains((int)point.X, (int)point.Y);
        }
        public bool Contains(int x, int y)
        {
            return (Left <= x && Right > x) && (Top <= y && Bottom > y);
        }
        private bool Contains(int x, int y, int Left, int Top, int Right, int Bottom)
        {
            return (Left <= x && Right > x) && (Top <= y && Bottom > y);
        }

        public bool Equals(Rect other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        public override bool Equals(object obj) => obj is Rect r && Equals(r);

        public Edge EdgeContains(Vector2 point, int edgeWidth = 10)
        {
            return EdgeContains((int)point.X, (int)point.Y, edgeWidth);
        }
        public Edge EdgeContains(int x, int y, int edgeWidth = 10)
        {
            Edge ret = Edge.None;
            if (Contains(x, y, Left, Top, Right, Top+edgeWidth)) { ret |= Edge.Top; }
            if (Contains(x, y, Left, Top, Left+edgeWidth, Bottom)) { ret |= Edge.Left; }
            if (Contains(x, y, Left, Bottom-edgeWidth, Right, Bottom)) { ret |= Edge.Bottom; }
            if (Contains(x, y, Right-edgeWidth, Top, Right, Bottom)) { ret |= Edge.Right; }
            return ret;
        }


        public static bool operator ==(Rect left, Rect right) => left.Equals(right);
        public static bool operator !=(Rect left, Rect right) => !left.Equals(right);

        [Flags]
        public enum Edge : int
        {
            None = 0,
            Top = 1,
            Left = 2,
            Bottom = 4,
            Right = 8,
            TopLeftCorner = Top | Left,
            BottomLeftCorner = Bottom | Left,
            TopRightCorner = Top | Right,
            BottomRightCorner = Bottom | Right
        }
    }
}