using System.Diagnostics;

namespace HandyScreenshot.Common
{
    public delegate void PointChangedEventHandler(int x, int y);

    [DebuggerDisplay("({X}, {Y})")]
    public class PointProxy
    {
        public event PointChangedEventHandler PointChanged;

        public int X { get; private set; }

        public int Y { get; private set; }

        public void Set(int x, int y)
        {
            X = x;
            Y = y;
            OnPointChanged();
        }

        protected virtual void OnPointChanged() => PointChanged?.Invoke(X, Y);
    }
}
