using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;
using HandyScreenshot.Interop;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.Models
{
    public class ScreenshotState : INotifyPropertyChanged
    {
        private ScreenshotMode _mode;
        private PointOrientation _orientation;
        private int _previousX;
        private int _previousY;

        public ScreenshotMode Mode
        {
            get => _mode;
            private set => SetProperty(ref _mode, value);
        }

        public PointOrientation Orientation
        {
            get => _orientation;
            private set => SetProperty(ref _orientation, value);
        }

        public PointProxy MousePosition { get; } = new PointProxy();

        public RectProxy ScreenshotRect { get; } = new RectProxy();

        public delegate void CaptureOKHandler(ScreenshotState vm);

        public event CaptureOKHandler CaptureOK;

        public ScreenshotState()
        {
        }

        public void PushState(MouseMessage message, int physicalX, int physicalY)
        {
            if (Mode == ScreenshotMode.Done) return;
            bool canUpdateMousePosition = true;

            switch (message)
            {
                case MouseMessage.LeftButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.Wait:
                            Mode = ScreenshotMode.Resizing;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed when ScreenshotRect.Contains(physicalX, physicalY):
                            Mode = ScreenshotMode.Moving;
                            Orientation = PointOrientation.Center;
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed:
                            {
                                Mode = ScreenshotMode.Resizing;
                                Orientation = GetPointOrientation(physicalX, physicalY);
                                var right = ScreenshotRect.X + ScreenshotRect.Width;
                                var bottom = ScreenshotRect.Y + ScreenshotRect.Height;
                                switch(Orientation)
                                {
                                    case PointOrientation.Left | PointOrientation.Top:
                                        (_previousX, _previousY) = (right, bottom);
                                        break;
                                    case PointOrientation.Right | PointOrientation.Top:
                                        (_previousX, _previousY) = (ScreenshotRect.X, bottom);
                                        break;
                                    case PointOrientation.Left | PointOrientation.Bottom:
                                        (_previousX, _previousY) = (right, ScreenshotRect.Y);
                                        break;
                                    case PointOrientation.Right | PointOrientation.Bottom:
                                        (_previousX, _previousY) = (ScreenshotRect.X, ScreenshotRect.Y);
                                        break;
                                }

                                Resize(Orientation, physicalX, physicalY);
                                break;
                            }
                    }
                    break;
                case MouseMessage.LeftButtonUp:
                    Mode = ScreenshotMode.Fixed;
                    // [防抖]：在截图完成，释放鼠标时，人的手很可能会抖，造成大约一两个像素的偏移，尤其是用触控板时，对强迫症患者造成极大的伤害；
                    // 所以 Resizing 或 Moving 结束的最后一个鼠标点不应当被绘制（要丢弃掉），这里是指 ClipBox 和 Magnifier 控件都不要绘制。
                    // P.S. 当然，具体偏移多少，和释放时的手速有关，坐标点毕竟是离散的。
                    if (ScreenshotRect.Width > 0 && ScreenshotRect.Height > 0)
                    {
                        CaptureOK.Invoke(this);
                        Mode = ScreenshotMode.Done;
                        canUpdateMousePosition = false;
                    }
                    else
                    {
                        Mode = ScreenshotMode.Wait;
                        Orientation = PointOrientation.Center;
                        ScreenshotRect.Set(0, 0, 0, 0);
                        canUpdateMousePosition = true;
                    }
                    
                    break;
                case MouseMessage.RightButtonDown:
                    switch (Mode)
                    {
                        case ScreenshotMode.Fixed:
                            Mode = ScreenshotMode.Wait;
                            Orientation = PointOrientation.Center;
                            break;
                        case ScreenshotMode.Wait:
                            ExitShot();
                            break;
                    }
                    break;
                case MouseMessage.MouseMove:
                    switch (Mode)
                    {
                        case ScreenshotMode.Wait:
                            break;
                        case ScreenshotMode.Resizing:
                            Resize(Orientation, physicalX, physicalY);
                            break;
                        case ScreenshotMode.Moving:
                            ScreenshotRect.Offset(_previousX, _previousY, physicalX, physicalY);
                            (_previousX, _previousY) = (physicalX, physicalY);
                            break;
                        case ScreenshotMode.Fixed:
                            Orientation = GetPointOrientation(physicalX, physicalY);
                            break;
                    }
                    break;
            }

            if (canUpdateMousePosition)
            {
                MousePosition.Set(physicalX, physicalY);
            }
            else
            {
                // 接上面的[防抖]所述，最后一个鼠标坐标虽然没有影响绘制，但是实际上 Cursor 还是抖动到了真实的位置，
                // 这样就会使得 Cursor 与绘制的图像不重合，就很丑，显得十分不专业。
                // 所以应当重置鼠标位置到上一个鼠标位置上。（我是不是考虑的很细致？（￣︶￣）↗）
                NativeMethods.SetCursorPos(MousePosition.X, MousePosition.Y);
            }
        }

        private void Resize(PointOrientation orientation, int x, int y)
        {
            var newOrientation = GetPointOrientation(x, y);

            switch (orientation)
            {
                case PointOrientation.Left:
                    if (newOrientation.HasFlag(PointOrientation.Right))
                    {
                        Orientation = PointOrientation.Right;
                        ScreenshotRect.Set(ScreenshotRect.X + ScreenshotRect.Width, ScreenshotRect.Y, 0, ScreenshotRect.Height);
                    }
                    else
                    {
                        ScreenshotRect.SetLeft(x);
                    }
                    break;
                case PointOrientation.Top:
                    if (newOrientation.HasFlag(PointOrientation.Bottom))
                    {
                        Orientation = PointOrientation.Bottom;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y + ScreenshotRect.Height, ScreenshotRect.Width, 0);
                    }
                    else
                    {
                        ScreenshotRect.SetTop(y);
                    }
                    break;
                case PointOrientation.Right:
                    if (newOrientation.HasFlag(PointOrientation.Left))
                    {
                        Orientation = PointOrientation.Left;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y, 0, ScreenshotRect.Height);
                    }
                    else
                    {
                        ScreenshotRect.SetRight(x);
                    }
                    break;
                case PointOrientation.Bottom:
                    if (newOrientation.HasFlag(PointOrientation.Top))
                    {
                        Orientation = PointOrientation.Top;
                        ScreenshotRect.Set(ScreenshotRect.X, ScreenshotRect.Y, ScreenshotRect.Width, 0);
                    }
                    else
                    {
                        ScreenshotRect.SetBottom(y);
                    }
                    break;
                default:
                    if (IsVertex(newOrientation))
                    {
                        Orientation = newOrientation;
                    }

                    var (newX, newY, newWidth, newHeight) = GetRectByTwoPoint(_previousX, _previousY, x, y);
                    ScreenshotRect.Set(newX, newY, newWidth, newHeight);
                    break;
            }
        }

        private PointOrientation GetPointOrientation(double x, double y)
        {
            return GetPointOrientation(
                x,
                y,
                ScreenshotRect.X,
                ScreenshotRect.Y,
                ScreenshotRect.Width,
                ScreenshotRect.Height);
        }

        private static PointOrientation GetPointOrientation(
            double pointX,
            double pointY,
            double rectX,
            double rectY,
            double rectWidth,
            double rectHeight)
        {
            var horizontal = pointX <= rectX
                ? PointOrientation.Left
                : pointX < rectX + rectWidth
                    ? PointOrientation.Center
                    : PointOrientation.Right;
            var vertical = pointY <= rectY
                ? PointOrientation.Top
                : pointY < rectY + rectHeight
                    ? PointOrientation.Center
                    : PointOrientation.Bottom;

            return horizontal | vertical;
        }

        private static ReadOnlyRect GetRectByTwoPoint(int x1, int y1, int x2, int y2)
        {
            var x = Math.Min(x1, x2);
            var y = Math.Min(y1, y2);
            return (
                x,
                y,
                Math.Max(Math.Max(x1, x2) - x, 0),
                Math.Max(Math.Max(y1, y2) - y, 0));
        }

        private static bool IsVertex(PointOrientation orientation)
        {
            if (orientation is (PointOrientation.Left | PointOrientation.Top)) return true;
            if (orientation is (PointOrientation.Right | PointOrientation.Top)) return true;
            if (orientation is (PointOrientation.Left | PointOrientation.Bottom)) return true;
            if (orientation is (PointOrientation.Right | PointOrientation.Bottom)) return true;
            return false;
        }

        private void ExitShot()
        {
             CaptureOK.Invoke(this);
        }

        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
