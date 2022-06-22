// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

using System;
using System.Runtime.InteropServices;

namespace HandyScreenshot.Interop
{
    public static partial class NativeMethods
    {
        #region Methods

        [DllImport(DllNames.Gdi32, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport(DllNames.Gdi32)]
        internal static extern bool DeleteDC(IntPtr hdc);

        [DllImport(DllNames.Gdi32)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport(DllNames.Gdi32)]
        internal static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport(DllNames.Gdi32)]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport(DllNames.Gdi32)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport(DllNames.Gdi32)]
        internal static extern int SelectObject(IntPtr hdc, IntPtr hGdiObj);

        [DllImport(DllNames.Gdi32)]
        internal static extern int GetDeviceCaps(IntPtr hDc, DeviceCap nIndex);

        #endregion

        #region Enums

        public enum DeviceCap
        {
            /// <summary>
            /// Device driver version
            /// </summary>
            Driverversion = 0,
            /// <summary>
            /// Device classification
            /// </summary>
            Technology = 2,
            /// <summary>
            /// Horizontal size in millimeters
            /// </summary>
            Horzsize = 4,
            /// <summary>
            /// Vertical size in millimeters
            /// </summary>
            Vertsize = 6,
            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            Horzres = 8,
            /// <summary>
            /// Vertical height in pixels
            /// </summary>
            Vertres = 10,
            /// <summary>
            /// Number of bits per pixel
            /// </summary>
            Bitspixel = 12,
            /// <summary>
            /// Number of planes
            /// </summary>
            Planes = 14,
            /// <summary>
            /// Number of brushes the device has
            /// </summary>
            Numbrushes = 16,
            /// <summary>
            /// Number of pens the device has
            /// </summary>
            Numpens = 18,
            /// <summary>
            /// Number of markers the device has
            /// </summary>
            Nummarkers = 20,
            /// <summary>
            /// Number of fonts the device has
            /// </summary>
            Numfonts = 22,
            /// <summary>
            /// Number of colors the device supports
            /// </summary>
            Numcolors = 24,
            /// <summary>
            /// Size required for device descriptor
            /// </summary>
            Pdevicesize = 26,
            /// <summary>
            /// Curve capabilities
            /// </summary>
            Curvecaps = 28,
            /// <summary>
            /// Line capabilities
            /// </summary>
            Linecaps = 30,
            /// <summary>
            /// Polygonal capabilities
            /// </summary>
            Polygonalcaps = 32,
            /// <summary>
            /// Text capabilities
            /// </summary>
            Textcaps = 34,
            /// <summary>
            /// Clipping capabilities
            /// </summary>
            Clipcaps = 36,
            /// <summary>
            /// Bitblt capabilities
            /// </summary>
            Rastercaps = 38,
            /// <summary>
            /// Length of the X leg
            /// </summary>
            Aspectx = 40,
            /// <summary>
            /// Length of the Y leg
            /// </summary>
            Aspecty = 42,
            /// <summary>
            /// Length of the hypotenuse
            /// </summary>
            Aspectxy = 44,
            /// <summary>
            /// Shading and Blending caps
            /// </summary>
            Shadeblendcaps = 45,

            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            Logpixelsx = 88,
            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            Logpixelsy = 90,

            /// <summary>
            /// Number of entries in physical palette
            /// </summary>
            Sizepalette = 104,
            /// <summary>
            /// Number of reserved entries in palette
            /// </summary>
            Numreserved = 106,
            /// <summary>
            /// Actual color resolution
            /// </summary>
            Colorres = 108,

            // Printing related DeviceCaps. These replace the appropriate Escapes
            /// <summary>
            /// Physical Width in device units
            /// </summary>
            Physicalwidth = 110,
            /// <summary>
            /// Physical Height in device units
            /// </summary>
            Physicalheight = 111,
            /// <summary>
            /// Physical Printable Area x margin
            /// </summary>
            Physicaloffsetx = 112,
            /// <summary>
            /// Physical Printable Area y margin
            /// </summary>
            Physicaloffsety = 113,
            /// <summary>
            /// Scaling factor x
            /// </summary>
            Scalingfactorx = 114,
            /// <summary>
            /// Scaling factor y
            /// </summary>
            Scalingfactory = 115,

            /// <summary>
            /// Current vertical refresh rate of the display device (for displays only) in Hz
            /// </summary>
            Vrefresh = 116,
            /// <summary>
            /// Vertical height of entire desktop in pixels
            /// </summary>
            Desktopvertres = 117,
            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            Desktophorzres = 118,
            /// <summary>
            /// Preferred blt alignment
            /// </summary>
            Bltalignment = 119
        }

        [Flags]
        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
        }

        #endregion
    }
}
