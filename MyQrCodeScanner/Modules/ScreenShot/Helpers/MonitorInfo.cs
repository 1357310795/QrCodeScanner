using System;
using System.Diagnostics;
using System.Windows;
using HandyScreenshot.Common;

namespace HandyScreenshot.Helpers
{
    [DebuggerDisplay("[{PhysicalScreenRect}], [{PhysicalWorkArea}]")]
    public class MonitorInfo
    {
        public IntPtr Handle { get; }

        public bool IsPrimaryScreen { get; }

        public ReadOnlyRect PhysicalWorkArea { get; }

        public ReadOnlyRect PhysicalScreenRect { get; }

        public Rect WpfAxisScreenRect { get; }

        public double ScaleX { get; }

        public double ScaleY { get; }

        public MonitorInfo(IntPtr handle, bool isPrimaryScreen, ReadOnlyRect workArea, ReadOnlyRect screenRect)
        {
            Handle = handle;
            IsPrimaryScreen = isPrimaryScreen;
            PhysicalWorkArea = workArea;
            PhysicalScreenRect = screenRect;
            (ScaleX, ScaleY) = MonitorHelper.GetScaleFactorFromMonitor(handle);

            var (x, y, width, height) = ToWpfAxis(
                screenRect.X,
                screenRect.Y,
                screenRect.Width,
                screenRect.Height);
            WpfAxisScreenRect = new Rect(x, y, width, height);
        }

        public (double x, double y, double width, double height) ToWpfAxis(int x, int y, int width, int height)
        {
            return (
                (x - PhysicalScreenRect.X) * ScaleX,
                (y - PhysicalScreenRect.Y) * ScaleY,
                width * ScaleX,
                height * ScaleY);
        }

        public (double x, double y) ToWpfAxis(int x, int y)
        {
            return (
                (x - PhysicalScreenRect.X) * ScaleX,
                (y - PhysicalScreenRect.Y) * ScaleY);
        }
    }
}
