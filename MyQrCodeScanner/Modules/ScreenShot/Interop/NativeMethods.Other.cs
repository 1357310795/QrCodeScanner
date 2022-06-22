using System;
using System.Runtime.InteropServices;

namespace HandyScreenshot.Interop
{
    public static partial class NativeMethods
    {
        [DllImport(DllNames.SHCore)]
        internal static extern int GetDpiForMonitor(IntPtr hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
    }
}
