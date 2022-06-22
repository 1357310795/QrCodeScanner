using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using HandyScreenshot.Common;
using HandyScreenshot.Models;
using HandyScreenshot.Views;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Extensions;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class ScreenshotHelper
    {
        public static ClipWindow mw;
        public static void StartScreenshot()
        {
            var monitorInfos = MonitorHelper.GetMonitorInfos();

            foreach (var monitorInfo in monitorInfos)
            {
                mw = new ClipWindow(
                    CaptureScreen(monitorInfo.PhysicalScreenRect),
                    monitorInfo);
                SetWindowRect(mw, monitorInfo.PhysicalScreenRect);
                mw.Initialize();
                mw.Show();
            }
        }

        public static void SetWindowRect(Window window, ReadOnlyRect rect)
        {
            SetWindowPos(
                window.GetHandle(),
                (IntPtr)HWND_TOPMOST,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                SWP_NOZORDER);
        }

        public static Bitmap CaptureScreen(ReadOnlyRect rect)
        {
            var hdcSrc = GetAllMonitorsDC();

            var width = rect.Width;
            var height = rect.Height;
            var hdcDest = CreateCompatibleDC(hdcSrc);
            var hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            _ = SelectObject(hdcDest, hBitmap);

            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, rect.X, rect.Y,
                TernaryRasterOperations.SRCCOPY | TernaryRasterOperations.CAPTUREBLT);

            var image = System.Drawing.Image.FromHbitmap(hBitmap);

            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            DeleteDC(hdcSrc);

            return image;
        }

        public static MemoryStream ToMemoryStream(this System.Drawing.Image image)
        {
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public static BitmapImage ToBitmapImage(this MemoryStream memoryStream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        public static IntPtr GetAllMonitorsDC()
        {
            return CreateDC("DISPLAY", null, null, IntPtr.Zero);
        }
    }
}