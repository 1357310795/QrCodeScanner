// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace HandyScreenshot.Interop
{
    public static partial class NativeMethods
    {
        internal delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
        internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        #region Constants

        public const uint SWP_ASYNCWINDOWPOS = 0x4000;
        public const uint SWP_DEFERERASE = 0x2000;
        public const uint SWP_DRAWFRAME = 0x0020;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOREPOSITION = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;

        public const int HWND_TOP = 0;
        public const int HWND_BOTTOM = 1;
        public const int HWND_TOPMOST = -1;
        public const int HWND_NOTOPMOST = -2;

        #endregion

        #region Methods

        [DllImport(DllNames.User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

        [DllImport(DllNames.User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport(DllNames.User32)]
        internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport(DllNames.User32, CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport(DllNames.User32)]
        internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport(DllNames.User32)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport(DllNames.User32)]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport(DllNames.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetPhysicalCursorPos(ref POINT pt);

        [DllImport(DllNames.User32, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(DllNames.User32, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport(DllNames.User32, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport(DllNames.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport(DllNames.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        #endregion

        #region Structs

        // Size of a device name string
        private const int CCHDEVICENAME = 32;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            /// <summary>
            /// The size, in bytes, of the structure. Set this member to sizeof(MONITORINFOEX) (72) before calling the GetMonitorInfo function.
            /// Doing so lets the function determine the type of structure you are passing to it.
            /// </summary>
            public int Size;

            /// <summary>
            /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT Monitor;

            /// <summary>
            /// A RECT structure that specifies the work area rectangle of the display monitor that can be used by applications,
            /// expressed in virtual-screen coordinates. Windows uses this rectangle to maximize an application on the monitor.
            /// The rest of the area in rcMonitor contains system windows such as the task bar and side bars.
            /// Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
            /// </summary>
            public RECT WorkArea;

            /// <summary>
            /// The attributes of the display monitor.
            ///
            /// This member can be the following value:
            ///   1 : MONITORINFO_OF_PRIMARY
            /// </summary>
            public uint Flags;

            /// <summary>
            /// A string that specifies the device name of the monitor being used. Most applications have no use for a display monitor name,
            /// and so can save some bytes by using a MONITORINFO structure.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEHOOKSTRUCT
        {
            public POINT pt;
            public IntPtr hwnd;
            public uint wHitTestCode;
            public IntPtr dwExtraInfo;
        }

        #endregion

        #region Enums

        public enum MonitorDpiType : uint
        {
            MdtDefault = 0,
            MdtEffectiveDpi = 0,
            MdtAngularDpi = 1,
            MdtRawDpi = 2,
        }

        public enum HookType
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        #endregion
    }
}
