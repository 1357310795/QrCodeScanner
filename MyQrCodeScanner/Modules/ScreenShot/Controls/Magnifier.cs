using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;
using HandyScreenshot.Models;

namespace HandyScreenshot.Controls
{
    public class Magnifier : DrawingControlBase
    {
        public static readonly DependencyProperty MagnifiedTargetProperty = DependencyProperty.Register(
            "MagnifiedTarget", typeof(Visual), typeof(Magnifier),
            new PropertyMetadata(default(Visual), OnMagnifiedTargetChanged));

        public static readonly DependencyProperty ColorGetterProperty = DependencyProperty.Register(
            "ColorGetter", typeof(Func<int, int, Color>), typeof(Magnifier),
            new PropertyMetadata(default(Func<int, int, Color>)));

        public static readonly DependencyProperty ScreenshotStateProperty = DependencyProperty.Register(
            "ScreenshotState", typeof(ScreenshotState), typeof(Magnifier),
            new PropertyMetadata(default(ScreenshotState), OnScreenshotStateChanged));

        public static readonly DependencyProperty MonitorInfoProperty = DependencyProperty.Register(
            "MonitorInfo", typeof(MonitorInfo), typeof(Magnifier),
            new PropertyMetadata(default(MonitorInfo), OnMonitorInfoChanged));

        private static void OnMonitorInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, MonitorInfo>(e,
                (self, newValue) => self.UpdateScale(newValue.ScaleX));
        }

        private static void OnMagnifiedTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, Visual>(e,
                (self, newValue) => self._magnifiedRegionBrush.Visual = newValue);
        }

        private static void OnScreenshotStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, ScreenshotState>(e,
                (self, newValue) => newValue.MousePosition.PointChanged += self.OnMousePositionChanged,
                (self, oldValue) => oldValue.MousePosition.PointChanged -= self.OnMousePositionChanged);
        }

        public Visual MagnifiedTarget
        {
            set => SetValue(MagnifiedTargetProperty, value);
        }

        public ScreenshotState ScreenshotState
        {
            get => (ScreenshotState)GetValue(ScreenshotStateProperty);
            set => SetValue(ScreenshotStateProperty, value);
        }

        public MonitorInfo MonitorInfo
        {
            get => (MonitorInfo)GetValue(MonitorInfoProperty);
            set => SetValue(MonitorInfoProperty, value);
        }

        public Func<int, int, Color> ColorGetter
        {
            get => (Func<int, int, Color>)GetValue(ColorGetterProperty);
            set => SetValue(ColorGetterProperty, value);
        }

        #region Constants: Magnifier Drawing Data

        private const int OffsetFromMouse = 20;
        private const int TargetRegionWidth = 19;
        private const int TargetRegionHeight = 13;
        private const int HalfTargetRegionWidth = TargetRegionWidth / 2;
        private const int HalfTargetRegionHeight = TargetRegionHeight / 2;
        private const int OnePixelMagnified = 8;
        private const int HalfMagnifiedOnePixelSize = OnePixelMagnified / 2;
        private const int InfoBoardHeight = 72;

        private const int MagnifierWidth = TargetRegionWidth * OnePixelMagnified;
        private const int MagnifierHeight = TargetRegionHeight * OnePixelMagnified;

        #endregion

        private static readonly Brush CrossLineBrush;
        private static readonly Brush InfoBackgroundBrush;

        static Magnifier()
        {
            CrossLineBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x20, 0x80, 0xf0));
            CrossLineBrush.Freeze();
            InfoBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0, 0, 0));
            InfoBackgroundBrush.Freeze();
        }

        private readonly Pen _whiteThinPen;
        private readonly Pen _blackThinPen;
        private readonly Pen _crossLinePen;
        private readonly VisualBrush _magnifiedRegionBrush;

        private double _offsetFromMouse = OffsetFromMouse;
        private double _targetRegionWidth = TargetRegionWidth;
        private double _targetRegionHeight = TargetRegionHeight;
        private double _halfTargetRegionWidth = HalfTargetRegionWidth;
        private double _halfTargetRegionHeight = HalfTargetRegionHeight;
        private double _halfPixelMagnified = HalfMagnifiedOnePixelSize;
        private double _magnifierWidth = MagnifierWidth;
        private double _magnifierHeight = MagnifierHeight;
        private double _infoBoardHeight = InfoBoardHeight;

        public Magnifier()
        {
            _whiteThinPen = new Pen(Brushes.White, 1);
            _blackThinPen = new Pen(Brushes.Black, 1);
            _crossLinePen = new Pen(CrossLineBrush, OnePixelMagnified);

            _magnifiedRegionBrush = new VisualBrush { ViewboxUnits = BrushMappingMode.Absolute };
        }

        private void UpdateScale(double scale)
        {
            _whiteThinPen.Thickness = scale;
            _blackThinPen.Thickness = scale;
            _crossLinePen.Thickness = OnePixelMagnified * scale;

            _offsetFromMouse = OffsetFromMouse * scale;
            _targetRegionWidth = TargetRegionWidth * scale;
            _targetRegionHeight = TargetRegionHeight * scale;
            _halfTargetRegionWidth = HalfTargetRegionWidth * scale;
            _halfTargetRegionHeight = HalfTargetRegionHeight * scale;
            _halfPixelMagnified = HalfMagnifiedOnePixelSize * scale;
            _magnifierWidth = MagnifierWidth * scale;
            _magnifierHeight = MagnifierHeight * scale;
            _infoBoardHeight = InfoBoardHeight * scale;
        }

        private void OnMousePositionChanged(int x, int y)
        {
            try
            {
                Dispatcher.Invoke(RefreshMagnifier);
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }

        private void RefreshMagnifier()
        {
            GetDrawingVisual().Using(DrawMagnifier);
        }

        private void DrawMagnifier(DrawingContext dc)
        {
            if (ScreenshotState.Mode == ScreenshotMode.Fixed &&
                ScreenshotState.Orientation != PointOrientation.Center) return;

            var physicalX = ScreenshotState.MousePosition.X;
            var physicalY = ScreenshotState.MousePosition.Y;

            if (!MonitorInfo.PhysicalScreenRect.Contains(physicalX, physicalY)) return;

            var (originalX, originalY) = MonitorInfo.ToWpfAxis(physicalX, physicalY);

            _magnifiedRegionBrush.Viewbox = new Rect(
                originalX - _halfTargetRegionWidth,
                originalY - _halfTargetRegionHeight,
                _targetRegionWidth,
                _targetRegionHeight);
            var (offsetX, offsetY) = CalculateOffsets(originalX, originalY);
            DrawMagnifier(
                dc,
                originalX,
                originalY,
                physicalX - MonitorInfo.PhysicalScreenRect.X,
                physicalY - MonitorInfo.PhysicalScreenRect.Y,
                offsetX,
                offsetY);
        }

        private (double offsetX, double offsetY) CalculateOffsets(double originalX, double originalY)
        {
            var width = _magnifierWidth;
            var height = _magnifierHeight + _infoBoardHeight;
            var screenBound = MonitorInfo.WpfAxisScreenRect;

            var offsetX = originalX + width + _offsetFromMouse > screenBound.Right
                ? -_offsetFromMouse - width
                : _offsetFromMouse;
            var offsetY = originalY + height + _offsetFromMouse > screenBound.Bottom
                ? -_offsetFromMouse - height
                : _offsetFromMouse;

            return (offsetX, offsetY);
        }

        private void DrawMagnifier(
            DrawingContext dc,
            double originalX,
            double originalY,
            int physicalX,
            int physicalY,
            double offsetX,
            double offsetY)
        {
            var x = originalX + offsetX;
            var y = originalY + offsetY;
            var width = _magnifierWidth;
            var height = _magnifierHeight;

            // 1. and 2. Prepare drawing data and add guidelines.

            var guidelines = new GuidelineSet();
            var scale = MonitorInfo.ScaleX;
            var halfPixel = scale / 2;

            // Draw magnified region box

            var magnifierRect = new Rect(x, y, width, height);
            var innerRect = new Rect(x - scale, y - scale, width + 2 * scale, height + 2 * scale);
            var outlineRect = new Rect(x - 2 * scale, y - 2 * scale, width + 4 * scale, height + 4 * scale);

            guidelines.GuidelinesX.Add(outlineRect.Left);
            guidelines.GuidelinesX.Add(outlineRect.Right);
            guidelines.GuidelinesY.Add(outlineRect.Top);
            guidelines.GuidelinesY.Add(outlineRect.Bottom);

            // Draw cross line * 4

            var centerLineX = (innerRect.Left + innerRect.Right) / 2;
            var centerLineY = (innerRect.Top + innerRect.Bottom) / 2;

            guidelines.GuidelinesX.Add(centerLineX + _halfPixelMagnified);
            guidelines.GuidelinesX.Add(centerLineX - _halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY + _halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY - _halfPixelMagnified);

            // Draw center point box

            var centerInnerRect = new Rect(
                centerLineX - _halfPixelMagnified + halfPixel - scale,
                centerLineY - _halfPixelMagnified + halfPixel - scale,
                2 * (_halfPixelMagnified - halfPixel) + 2 * scale,
                2 * (_halfPixelMagnified - halfPixel) + 2 * scale);
            var centerOutlineRect = new Rect(
                centerLineX - _halfPixelMagnified + halfPixel - 2 * scale,
                centerLineY - _halfPixelMagnified + halfPixel - 2 * scale,
                2 * (_halfPixelMagnified - halfPixel) + 4 * scale,
                2 * (_halfPixelMagnified - halfPixel) + 4 * scale);

            guidelines.GuidelinesX.Add(centerInnerRect.Left - halfPixel);
            guidelines.GuidelinesX.Add(centerInnerRect.Right + halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Top - halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Bottom + halfPixel);

            var infoBackgroundRect =
                new Rect(outlineRect.Left, outlineRect.Bottom, outlineRect.Width, _infoBoardHeight);
            var color = ColorGetter(physicalX, physicalY);
            var colorText = GetText($"#{color.R:X2}{color.G:X2}{color.B:X2}", 1 / scale);
            var positionText = GetText($"({originalX / scale:0}, {originalY / scale:0})", 1 / scale);

            var positionTextX = centerLineX - positionText.Width / 2;
            var positionTextY = outlineRect.Bottom + 12 * scale;
            var colorBlockSize = 14 * scale;
            var colorComponentWidth = colorBlockSize + 8 * scale + colorText.Width;
            var colorX = centerLineX - colorComponentWidth / 2;
            var colorTextX = colorX + colorComponentWidth - colorText.Width;
            var colorY = positionTextY + positionText.Height + 12 * scale;
            var colorTextY = colorY + (colorBlockSize - colorText.Height) / 2;
            var positionTextPoint = new Point(positionTextX, positionTextY);
            var colorTextPoint = new Point(colorTextX, colorTextY);
            var colorBlockRect = new Rect(colorX, colorY, colorBlockSize, colorBlockSize);

            guidelines.GuidelinesX.Add(infoBackgroundRect.Bottom);
            guidelines.GuidelinesX.Add(colorBlockRect.Left - halfPixel);
            guidelines.GuidelinesX.Add(colorBlockRect.Right + halfPixel);
            guidelines.GuidelinesY.Add(colorBlockRect.Top - halfPixel);
            guidelines.GuidelinesY.Add(colorBlockRect.Bottom + halfPixel);

            // 3. Start Drawing

            dc.PushGuidelineSet(guidelines);

            // Draw magnified region box

            dc.DrawRectangle(Brushes.Black, null, outlineRect);
            dc.DrawRectangle(Brushes.White, null, innerRect);
            dc.DrawRectangle(_magnifiedRegionBrush, null, magnifierRect);

            // Draw cross line * 4

            dc.DrawLine(
                _crossLinePen,
                new Point(centerLineX, innerRect.Top),
                new Point(centerLineX, centerLineY - _halfPixelMagnified - 2 * scale));
            dc.DrawLine(
                _crossLinePen,
                new Point(centerLineX, innerRect.Bottom),
                new Point(centerLineX, centerLineY + _halfPixelMagnified + 2 * scale));
            dc.DrawLine(
                _crossLinePen,
                new Point(innerRect.Left, centerLineY),
                new Point(centerLineX - _halfPixelMagnified - 2 * scale, centerLineY));
            dc.DrawLine(
                _crossLinePen,
                new Point(innerRect.Right, centerLineY),
                new Point(centerLineX + _halfPixelMagnified + 2 * scale, centerLineY));

            // Draw center point box

            dc.DrawRectangle(Brushes.Transparent, _blackThinPen, centerOutlineRect);
            dc.DrawRectangle(Brushes.Transparent, _whiteThinPen, centerInnerRect);

            // Draw info board

            dc.DrawRectangle(InfoBackgroundBrush, null, infoBackgroundRect);
            dc.DrawText(positionText, positionTextPoint);
            dc.DrawRectangle(new SolidColorBrush(color), _whiteThinPen, colorBlockRect);
            dc.DrawText(colorText, colorTextPoint);

            dc.Pop();
        }

        private static FormattedText GetText(string text, double pixelsPerDip)
        {
            return new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                12,
                Brushes.White,
                pixelsPerDip);
        }
    }
}