using System;
using System.Diagnostics;
using System.Windows;

namespace HandyScreenshot.Common
{
    [DebuggerDisplay("({X}, {Y}) [{Width}, {Height}]")]
    public readonly struct ReadOnlyRect
    {
        public static readonly ReadOnlyRect Zero = (0, 0, 0, 0);
        public static readonly ReadOnlyRect Empty = (int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly bool IsEmpty;

        public ReadOnlyRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsEmpty = width < 0;
        }

        public bool Contains(int x, int y)
        {
            return X <= x && Y <= y && x <= X + Width && y <= Y + Height;
        }

        public bool IntersectsWith(ReadOnlyRect rect)
        {
            return !IsEmpty && !rect.IsEmpty &&
                   rect.X <= X + Width &&
                   rect.X + rect.Width >= X &&
                   rect.Y <= Y + Height &&
                   rect.Y + rect.Height >= Y;
        }

        public ReadOnlyRect Intersect(ReadOnlyRect rect)
        {
            if (!IntersectsWith(rect)) return Empty;

            int x = Math.Max(X, rect.X);
            int y = Math.Max(Y, rect.Y);
            int width = Math.Max(Math.Min(X + Width, rect.X + rect.Width) - x, 0);
            int height = Math.Max(Math.Min(Y + Height, rect.Y + rect.Height) - y, 0);
            return (x, y, width, height);
        }

        public ReadOnlyRect Union(ReadOnlyRect rect)
        {
            if (IsEmpty || rect.IsEmpty) return Empty;

            int x = Math.Min(X, rect.X);
            int y = Math.Min(Y, rect.Y);
            int width = rect.Width == int.MaxValue || Width == int.MaxValue
                ? int.MaxValue
                : Math.Max(Math.Max(X + Width, rect.X + rect.Width) - x, 0);
            int height = rect.Height == int.MaxValue || Height == int.MaxValue
                ? int.MaxValue
                : Math.Max(Math.Max(Y + Height, rect.Y + Height) - y, 0);

            return (x, y, width, height);
        }

        public override string ToString() => $"({X}, {Y}) [{Width}, {Height}]";

        internal void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        public static implicit operator ReadOnlyRect((int, int, int, int) rect)
        {
            var (x, y, w, h) = rect;
            return new ReadOnlyRect(x, y, w, h);
        }

        public static implicit operator ReadOnlyRect(Rect rect) => new ReadOnlyRect(
            (int)rect.X,
            (int)rect.Y,
            (int)rect.Width,
            (int)rect.Height);

        public bool Equals(ReadOnlyRect other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj) => obj is ReadOnlyRect other && Equals(other);

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Height.GetHashCode() ^ this.Width.GetHashCode();
        }

        public static bool operator ==(ReadOnlyRect left, ReadOnlyRect right) => left.Equals(right);

        public static bool operator !=(ReadOnlyRect left, ReadOnlyRect right) => !(left == right);
    }
}
