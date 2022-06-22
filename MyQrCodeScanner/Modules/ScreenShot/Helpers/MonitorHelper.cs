using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class MonitorHelper
    {
        private const double DefaultDpi = 96.0;
        private const uint MonitorDefaultToNull = 0;

        private static readonly bool DpiApiLevel3 = Environment.OSVersion.Version >= new Version(6, 3);

        public static IReadOnlyCollection<MonitorInfo> GetMonitorInfos()
        {
            var monitors = new List<MonitorInfo>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr monitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr data) =>
                {
                    var monitorInfoEx = new MONITORINFOEX { Size = Marshal.SizeOf(typeof(MONITORINFOEX)) };
                    GetMonitorInfo(monitor, ref monitorInfoEx);
                    var info = new MonitorInfo(monitor,
                        monitorInfoEx.Flags == 1,
                        monitorInfoEx.WorkArea.ToReadOnlyRect(),
                        monitorInfoEx.Monitor.ToReadOnlyRect());

                    monitors.Add(info);

                    return true;
                },
                IntPtr.Zero);

            return monitors;
        }

        public static (double scaleX, double scaleY) GetScaleFactorFromMonitor(IntPtr hMonitor)
        {
            if (!DpiApiLevel3) throw new NotSupportedException();

            var ret = GetDpiForMonitor(hMonitor, MonitorDpiType.MdtEffectiveDpi, out var dpiX, out var dpiY);
            if (ret != 0)
                throw new Win32Exception("Queries DPI of a display failed", Marshal.GetExceptionForHR(ret));

            return (DefaultDpi / dpiX, DefaultDpi / dpiY);
        }

        public static (double scaleX, double scaleY) GetScaleFactorFromWindow(IntPtr hWnd)
        {
            if (DpiApiLevel3)
            {
                var hMonitor = MonitorFromWindow(hWnd, MonitorDefaultToNull);
                return GetScaleFactorFromMonitor(hMonitor);
            }

            var windowDc = GetWindowDC(hWnd);
            if (windowDc == IntPtr.Zero)
                throw new Win32Exception("Getting window device context failed");

            var dpiX = GetDeviceCaps(windowDc, DeviceCap.Logpixelsx);
            var dpiY = GetDeviceCaps(windowDc, DeviceCap.Logpixelsy);

            if (ReleaseDC(hWnd, windowDc) == 0)
                throw new Win32Exception("Releasing window device context failed");

            return (DefaultDpi / dpiX, DefaultDpi / dpiY);
        }
    }
}
