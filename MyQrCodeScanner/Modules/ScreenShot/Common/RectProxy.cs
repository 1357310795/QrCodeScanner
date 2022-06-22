using System;

namespace HandyScreenshot.Common
{
    public delegate void RectChangedEventHandler(int x, int y, int width, int height);

    public class RectProxy
    {
        public event RectChangedEventHandler RectChanged;

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool Contains(int x, int y) => X <= x && Y <= y && x <= X + Width && y <= Y + Height;

        public void Offset(int x1, int y1, int x2, int y2)
        {
            X = X + x2 - x1;
            Y = Y + y2 - y1;
            OnRectChanged();
        }

        public void Union(int x, int y)
        {
            Width = Math.Max(Width, X < x ? x - X : X - x + Width);
            Height = Math.Max(Height, Y < y ? y - Y : Y - y + Height);
            X = Math.Min(X, x);
            Y = Math.Min(Y, y);
            OnRectChanged();
        }

        public void Set(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            OnRectChanged();
        }

        public void SetLeft(int left)
        {
            if (left > X + Width) return;

            Width = X + Width - left;
            X = left;
            OnRectChanged();
        }

        public void SetRight(int right)
        {
            if (right < X) return;

            Width = right - X;
            OnRectChanged();
        }

        public void SetTop(int top)
        {
            if (top > Y + Height) return;

            Height = Y + Height - top;
            Y = top;
            OnRectChanged();
        }

        public void SetBottom(int bottom)
        {
            if (bottom < Y) return;

            Height = bottom - Y;
            OnRectChanged();
        }

        protected virtual void OnRectChanged() => RectChanged?.Invoke(X, Y, Width, Height);

        public override string ToString()
        {
            return $"({X},{Y}),({X+Width},{Y+Height})";
        }

        public ReadOnlyRect ToReadOnlyRect()
        {
            return new ReadOnlyRect(X, Y, Width, Height);
        }
    }
}
