using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class Win32Helper
    {
        public static IDisposable SubscribeMouseHook(Action<MouseMessage, MOUSEHOOKSTRUCT> callback)
        {
            var gcHandle = GCHandle.Alloc(new HookProc(WndProc));

            var hookId = SetWindowsHookEx(
                HookType.WH_MOUSE_LL,
                (HookProc)gcHandle.Target,
                // ReSharper disable once PossibleNullReferenceException
                Process.GetCurrentProcess().MainModule.BaseAddress,
                0);

            if (hookId == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return Disposable.Create(() =>
            {
                _ = UnhookWindowsHookEx(hookId);
                gcHandle.Free();
            });

            IntPtr WndProc(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0)
                {
                    var mouseHookStruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
                    callback((MouseMessage)wParam, mouseHookStruct);
                }

                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
        }

        public static POINT GetPhysicalMousePosition()
        {
            var position = new POINT();
            GetPhysicalCursorPos(ref position);
            return position;
        }
    }
}
