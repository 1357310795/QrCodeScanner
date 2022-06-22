using System;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;
using static HandyScreenshot.Controls.ClipBox;

namespace HandyScreenshot.Controls
{
    internal class ClipBoxVisual : DrawingControlBase
    {
        private const int BackgroundIndex = 0;
        private const int ClipBoxIndex = 1;

        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBoxVisual),
            new PropertyMetadata(default(RectProxy), OnRectProxyChanged));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(ImageSource), typeof(ClipBoxVisual),
            new PropertyMetadata(default(ImageSource), OnBackgroundChanged));

        public static readonly DependencyProperty MonitorInfoProperty = DependencyProperty.Register(
            "MonitorInfo", typeof(MonitorInfo), typeof(ClipBoxVisual),
            new PropertyMetadata(default(MonitorInfo), OnMonitorInfoChanged));

        private static void OnRectProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxVisual, RectProxy>(e,
                (self, newValue) => newValue.RectChanged += self.OnRectChanged,
                (self, oldValue) => oldValue.RectChanged -= self.OnRectChanged);
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxVisual, ImageSource>(e,
                (self, newValue) => self.RefreshBackground());
        }

        private static void OnMonitorInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxVisual, MonitorInfo>(e,
                (self, newValue) => self._primaryPen = CreatePrimaryPen(newValue.ScaleX));
        }

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        public ImageSource Background
        {
            get => (ImageSource)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public MonitorInfo MonitorInfo => (MonitorInfo)GetValue(MonitorInfoProperty);

        private Pen _primaryPen;

        public ClipBoxVisual() : base(2)
        {
            _primaryPen = CreatePrimaryPen(1);
        }

        private void OnRectChanged(int x, int y, int w, int h) => Dispatcher.Invoke(RefreshClipBox);

        // ReSharper disable once RedundantArgumentDefaultValue
        private void RefreshBackground() => GetDrawingVisual(BackgroundIndex).Using(DrawBackground);

        private void RefreshClipBox() => GetDrawingVisual(ClipBoxIndex).Using(DrawClipBox);

        private void DrawBackground(DrawingContext dc)
        {
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.NearestNeighbor);
            var groupDc = group.Open();
            groupDc.DrawImage(Background, new Rect(0, 0, ActualWidth, ActualHeight));
            groupDc.Close();
            dc.DrawDrawing(group);
        }

        private void DrawClipBox(DrawingContext dc)
        {
            var halfPenThickness = _primaryPen.Thickness / 2;

            var (x, y, w, h) = MonitorInfo.ToWpfAxis(
                RectProxy.X,
                RectProxy.Y,
                RectProxy.Width,
                RectProxy.Height);

            x -= halfPenThickness;
            y -= halfPenThickness;
            w += _primaryPen.Thickness + halfPenThickness;
            h += _primaryPen.Thickness + halfPenThickness;

            var x0 = Math.Max(x, 0);
            var y0 = Math.Max(y, 0);
            var w0 = Math.Max(w, 0);
            var h0 = Math.Max(h, 0);

            var r = x + w;
            var b = y + h;

            var leftRect = new Rect(0, y, x0, h0);
            var topRect = new Rect(0, 0, ActualWidth, y0);
            var rightRect = new Rect(r, y, Math.Max(ActualWidth - r, 0), h0);
            var bottomRect = new Rect(0, b, ActualWidth, Math.Max(ActualHeight - b, 0));
            var centralRect = new Rect(x, y, w0, h0);

            var guidelines = new GuidelineSet(
                new[]
                {
                    centralRect.Left + halfPenThickness,
                    centralRect.Right - halfPenThickness
                },
                new[]
                {
                    centralRect.Top + halfPenThickness,
                    centralRect.Bottom - halfPenThickness
                });

            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(MaskBrush, null, leftRect);
            dc.DrawRectangle(MaskBrush, null, topRect);
            dc.DrawRectangle(MaskBrush, null, rightRect);
            dc.DrawRectangle(MaskBrush, null, bottomRect);
            dc.DrawRectangle(Brushes.Transparent, _primaryPen, centralRect);

            dc.Pop();
        }
    }
}
