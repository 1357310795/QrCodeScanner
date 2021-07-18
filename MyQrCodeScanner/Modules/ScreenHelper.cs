using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFCaptureScreenShot
{
	[StandardModule]
	public sealed class WindowsMonitorAPI
	{
		// Token: 0x0600009E RID: 158
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool GetMonitorInfo(HandleRef hmonitor, [In][Out] WindowsMonitorAPI.MONITORINFOEX info);

		// Token: 0x0600009F RID: 159
		[DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern bool EnumDisplayMonitors(HandleRef hdc, WindowsMonitorAPI.COMRECT rcClip, WindowsMonitorAPI.MonitorEnumProc lpfnEnum, IntPtr dwData);

		// Token: 0x04000049 RID: 73
		private const string User32 = "user32.dll";

		// Token: 0x0400004A RID: 74
		public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

		// Token: 0x0200001D RID: 29
		// (Invoke) Token: 0x06000126 RID: 294
		public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

		// Token: 0x0200001E RID: 30
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
		public class MONITORINFOEX
		{
			// Token: 0x06000127 RID: 295 RVA: 0x000056EC File Offset: 0x000038EC
			public MONITORINFOEX()
			{
				this.cbSize = Marshal.SizeOf(typeof(WindowsMonitorAPI.MONITORINFOEX));
				this.rcMonitor = default(WindowsMonitorAPI.RECT1);
				this.rcWork = default(WindowsMonitorAPI.RECT1);
				this.dwFlags = 0;
				this.szDevice = new char[32];
			}

			// Token: 0x04000095 RID: 149
			internal int cbSize;

			// Token: 0x04000096 RID: 150
			internal WindowsMonitorAPI.RECT1 rcMonitor;

			// Token: 0x04000097 RID: 151
			internal WindowsMonitorAPI.RECT1 rcWork;

			// Token: 0x04000098 RID: 152
			internal int dwFlags;

			// Token: 0x04000099 RID: 153
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			internal char[] szDevice;
		}

		// Token: 0x0200001F RID: 31
		public struct RECT1
		{
			// Token: 0x06000128 RID: 296 RVA: 0x00005744 File Offset: 0x00003944
			public RECT1(Rect r)
			{
				this = default(WindowsMonitorAPI.RECT1);
				checked
				{
					this.left = (int)Math.Round(r.Left);
					this.top = (int)Math.Round(r.Top);
					this.right = (int)Math.Round(r.Right);
					this.bottom = (int)Math.Round(r.Bottom);
				}
			}

			// Token: 0x0400009A RID: 154
			public int left;

			// Token: 0x0400009B RID: 155
			public int top;

			// Token: 0x0400009C RID: 156
			public int right;

			// Token: 0x0400009D RID: 157
			public int bottom;
		}

		// Token: 0x02000020 RID: 32
		[StructLayout(LayoutKind.Sequential)]
		public class COMRECT
		{
			// Token: 0x0400009E RID: 158
			public int left;

			// Token: 0x0400009F RID: 159
			public int top;

			// Token: 0x040000A0 RID: 160
			public int right;

			// Token: 0x040000A1 RID: 161
			public int bottom;
		}
	}

	public class ScreenHelper
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00002E00 File Offset: 0x00001000
		public static double GetScalingRatio()
		{
			double logicalHeight = ScreenHelper.GetLogicalHeight();
			double actualHeight = ScreenHelper.GetActualHeight();
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

		// Token: 0x060000A2 RID: 162 RVA: 0x00002E58 File Offset: 0x00001058
		public static double GetActualHeight()
		{
			return SystemParameters.PrimaryScreenHeight;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00002E70 File Offset: 0x00001070
		public static double GetActualWidth()
		{
			return SystemParameters.PrimaryScreenWidth;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00002E88 File Offset: 0x00001088
		public static double GetLogicalHeight()
		{
			double logicalHeight = 0.0;
			WindowsMonitorAPI.MonitorEnumProc proc = delegate (IntPtr m, IntPtr h, IntPtr lm, IntPtr lp)
			{
				WindowsMonitorAPI.MONITORINFOEX info = new WindowsMonitorAPI.MONITORINFOEX();
				WindowsMonitorAPI.GetMonitorInfo(new HandleRef(null, m), info);
				bool flag = (info.dwFlags & 1) != 0;
				bool flag2 = flag;
				if (flag2)
				{
					logicalHeight = (double)(checked(info.rcMonitor.bottom - info.rcMonitor.top));
				}
				return true;
			};
			WindowsMonitorAPI.EnumDisplayMonitors(WindowsMonitorAPI.NullHandleRef, null, proc, IntPtr.Zero);
			return logicalHeight;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00002ED4 File Offset: 0x000010D4
		public static double GetLogicalWidth()
		{
			double logicalWidth = 0.0;
			WindowsMonitorAPI.MonitorEnumProc proc = delegate (IntPtr m, IntPtr h, IntPtr lm, IntPtr lp)
			{
				WindowsMonitorAPI.MONITORINFOEX info = new WindowsMonitorAPI.MONITORINFOEX();
				WindowsMonitorAPI.GetMonitorInfo(new HandleRef(null, m), info);
				bool flag = (info.dwFlags & 1) != 0;
				bool flag2 = flag;
				if (flag2)
				{
					logicalWidth = (double)(checked(info.rcMonitor.right - info.rcMonitor.left));
				}
				return true;
			};
			WindowsMonitorAPI.EnumDisplayMonitors(WindowsMonitorAPI.NullHandleRef, null, proc, IntPtr.Zero);
			return logicalWidth;
		}
	}
}
