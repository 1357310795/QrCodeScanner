using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace QRCodeMagic.Helpers
{
    public sealed class WindowsMonitorAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In][Out] MONITORINFOEX info);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern bool EnumDisplayMonitors(HandleRef hdc, COMRECT rcClip, MonitorEnumProc lpfnEnum, nint dwData);

        private const string User32 = "user32.dll";
        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public delegate bool MonitorEnumProc(nint monitor, nint hdc, nint lprcMonitor, nint lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MONITORINFOEX
        {
            public MONITORINFOEX()
            {
                cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                rcMonitor = default;
                rcWork = default;
                dwFlags = 0;
                szDevice = new char[32];
            }

            internal int cbSize;
            internal RECT1 rcMonitor;
            internal RECT1 rcWork;
            internal int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal char[] szDevice;
        }

        public struct RECT1
        {
            public RECT1(Rect r)
            {
                this = default;
                checked
                {
                    left = (int)Math.Round(r.Left);
                    top = (int)Math.Round(r.Top);
                    right = (int)Math.Round(r.Right);
                    bottom = (int)Math.Round(r.Bottom);
                }
            }

            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }

    public class MonitorHelper
    {
        public static double GetScalingRatio()
        {
            double logicalHeight = GetLogicalHeight();
            double actualHeight = GetActualHeight();
            bool flag = logicalHeight > 0.0 && actualHeight > 0.0;
            bool flag2 = flag;
            double result;
            if (flag2)
            {
                result = logicalHeight / actualHeight;
            }
            else
            {
                result = 1.0;
            }
            return result;
        }

        public static double GetActualHeight()
        {
            return SystemParameters.PrimaryScreenHeight;
        }

        public static double GetActualWidth()
        {
            return SystemParameters.PrimaryScreenWidth;
        }

        public static double GetLogicalHeight()
        {
            double logicalHeight = 0.0;
            WindowsMonitorAPI.MonitorEnumProc proc = delegate (nint m, nint h, nint lm, nint lp)
            {
                WindowsMonitorAPI.MONITORINFOEX info = new WindowsMonitorAPI.MONITORINFOEX();
                WindowsMonitorAPI.GetMonitorInfo(new HandleRef(null, m), info);
                bool flag = (info.dwFlags & 1) != 0;
                bool flag2 = flag;
                if (flag2)
                {
                    logicalHeight = checked(info.rcMonitor.bottom - info.rcMonitor.top);
                }
                return true;
            };
            WindowsMonitorAPI.EnumDisplayMonitors(WindowsMonitorAPI.NullHandleRef, null, proc, 0);
            return logicalHeight;
        }

        public static double GetLogicalWidth()
        {
            double logicalWidth = 0.0;
            WindowsMonitorAPI.MonitorEnumProc proc = delegate (nint m, nint h, nint lm, nint lp)
            {
                WindowsMonitorAPI.MONITORINFOEX info = new WindowsMonitorAPI.MONITORINFOEX();
                WindowsMonitorAPI.GetMonitorInfo(new HandleRef(null, m), info);
                bool flag = (info.dwFlags & 1) != 0;
                bool flag2 = flag;
                if (flag2)
                {
                    logicalWidth = checked(info.rcMonitor.right - info.rcMonitor.left);
                }
                return true;
            };
            WindowsMonitorAPI.EnumDisplayMonitors(WindowsMonitorAPI.NullHandleRef, null, proc, 0);
            return logicalWidth;
        }
    }
}
