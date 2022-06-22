using System;

namespace HandyScreenshot.Common
{
    /// <summary>
    /// It represents the orientation of a point relative to a rectangle.
    /// </summary>
    [Flags]
    public enum PointOrientation
    {
        Center = 0b0000,
        Left = 0b0001,
        Top = 0b0010,
        Right = 0b0100,
        Bottom = 0b1000,
    }
}
