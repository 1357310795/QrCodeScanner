using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;

namespace QRCodeMagic.Helpers
{
    //[Serializable]
    //[StructLayout(LayoutKind.Sequential)]
    //public struct RECT
    //{
    //    public int Left;
    //    public int Top;
    //    public int Right;
    //    public int Bottom;

    //    public RECT(int left, int top, int right, int bottom)
    //    {
    //        Left = left;
    //        Top = top;
    //        Right = right;
    //        Bottom = bottom;
    //    }
    //}

    //// POINT structure required by WINDOWPLACEMENT structure
    //[Serializable]
    //[StructLayout(LayoutKind.Sequential)]
    //public struct POINT
    //{
    //    public int X;
    //    public int Y;

    //    public POINT(int x, int y)
    //    {
    //        X = x;
    //        Y = y;
    //    }
    //}

    //// WINDOWPLACEMENT stores the position, size, and state of a window
    //[Serializable]
    //[StructLayout(LayoutKind.Sequential)]
    //public struct WINDOWPLACEMENT
    //{
    //    public int length;
    //    public int flags;
    //    public int showCmd;
    //    public POINT minPosition;
    //    public POINT maxPosition;
    //    public RECT normalPosition;
    //}

    public static class WindowPlacementHelper
    {
        class WindowPlacement
        {
            public double Left;
            public double Top;
            public double Width;
            public double Height;
            public WindowState State;
        }
        //private static Encoding encoding = new UTF8Encoding();
        //private static XmlSerializer serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));
        private static string ConfigFilePath => Path.Combine(PathHelper.AppDataPath, "placement.json");

        //[DllImport("user32.dll")]
        //private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        //[DllImport("user32.dll")]
        //private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        //private const int SW_SHOWNORMAL = 1;
        //private const int SW_SHOWMINIMIZED = 2;

        //public static void SetPlacement(Window window, string placementXml)
        //{
        //    IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        //    if (string.IsNullOrEmpty(placementXml))
        //    {
        //        return;
        //    }

        //    WINDOWPLACEMENT placement;
        //    byte[] xmlBytes = encoding.GetBytes(placementXml);

        //    try
        //    {
        //        using (MemoryStream memoryStream = new MemoryStream(xmlBytes))
        //        {
        //            placement = (WINDOWPLACEMENT)serializer.Deserialize(memoryStream);
        //        }

        //        placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
        //        placement.flags = 0;
        //        placement.showCmd = placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd;
        //        SetWindowPlacement(windowHandle, ref placement);
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        // Parsing placement XML failed. Fail silently.
        //    }
        //}

        //public static string GetPlacement(Window window)
        //{
        //    IntPtr windowHandle = new WindowInteropHelper(window).Handle;
        //    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        //    GetWindowPlacement(windowHandle, out placement);

        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
        //        {
        //            serializer.Serialize(xmlTextWriter, placement);
        //            byte[] xmlBytes = memoryStream.ToArray();
        //            return encoding.GetString(xmlBytes);
        //        }
        //    }
        //}
        public static string GetPlacement(Window window)
        {
            WindowPlacement placement = new WindowPlacement();
            placement.Width = window.Width; placement.Height = window.Height;
            placement.Top = window.Top; placement.Left = window.Left;
            placement.State = window.WindowState;
            return JsonConvert.SerializeObject(placement);
        }

        public static void SetPlacement(Window window, string placementJson)
        {
            var json = JsonConvert.DeserializeObject<WindowPlacement>(placementJson);
            window.Width = json.Width; window.Height = json.Height;
            window.Top = json.Top; window.Left = json.Left;
            window.WindowState = json.State;
        }

        public static void ReadPlacement(Window window)
        {
            if (File.Exists(ConfigFilePath))
            {
                var text = File.ReadAllText(ConfigFilePath);
                SetPlacement(window, text);
            }
        }

        public static void SavePlacement(Window window)
        {
            Directory.CreateDirectory(PathHelper.AppDataPath);
            File.WriteAllText(ConfigFilePath, GetPlacement(window));
        }
    }
}
