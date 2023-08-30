using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ZXing.Common;

namespace QRCodeMagic.Controls
{
    public class QRCodeView : FrameworkElement
    {
        public BitMatrix Code
        {
            get { return (BitMatrix)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }
         
        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(BitMatrix), typeof(QRCodeView), new PropertyMetadata(null, OnCodeChanged));

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
         
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(QRCodeView), new PropertyMetadata(null));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
         
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(QRCodeView), new PropertyMetadata(null));

        public Thickness CornerRadius
        {
            get { return (Thickness)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
         
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(Thickness), typeof(QRCodeView), new PropertyMetadata(new Thickness(0)));

        private GeometryGroup codeGeometry;

        private static void OnCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (QRCodeView)d;
            control.codeGeometry = control.GetGraphic(new Size(100,100));
            control.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (codeGeometry != null)
            {
                var scale = Math.Min(this.ActualWidth, this.ActualHeight) / 100d;
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawGeometry(Foreground, null, codeGeometry);
                drawingContext.Pop();
            }
        }

        public GeometryGroup GetGraphic(Size viewBox)
        {
            int drawableModulesCount = GetDrawableModulesCount();
            int positionMarkerCount = GetPositionMarkerCount(drawableModulesCount);
            double unitsPerModule = GetUnitsPerModule(viewBox, drawableModulesCount);
            const int offsetModules = 0;

            GeometryGroup group = new GeometryGroup();
            double x = 0d;
            for (int xi = offsetModules; xi < drawableModulesCount + offsetModules; xi++)
            {
                double y = 0d;
                for (int yi = offsetModules; yi < drawableModulesCount + offsetModules; yi++)
                {
                    //if (Code[yi, xi])
                    //{
                    //    RadiusFilterKind filterKind = GetRadiusFilterKind(xi, yi);
                    //    group.Children.Add(
                    //        new PathGeometry
                    //        {
                    //            Figures = new PathFigureCollection
                    //            {
                    //                CreateRoundedRectanglePath(x, y, unitsPerModule, filterKind)
                    //            }
                    //        });
                    //}
                    //else
                    //{
                    //    RadiusFilterKind filterKind = GetAntiRadiusFilterKind(xi, yi);
                    //    group.Children.Add(
                    //        new PathGeometry
                    //        {
                    //            Figures = CreateEntiRoundedRectanglePath(x, y, unitsPerModule, filterKind)
                    //        });
                    //}
                    if (Code[yi, xi])
                        group.Children.Add(
                            new PathGeometry
                            {
                                Figures = new PathFigureCollection
                                {
                                        CreateRectanglePath(x, y, unitsPerModule)
                                }
                            });
                    y += unitsPerModule;
                }
                x += unitsPerModule;
            }
            return group;
        }

        public GeometryGroup GetGraphic(Size viewBox, out GeometryGroup markerGroup, out GeometryGroup contentGroup)
        {
            int drawableModulesCount = GetDrawableModulesCount();
            int positionMarkerCount = GetPositionMarkerCount(drawableModulesCount);
            double unitsPerModule = GetUnitsPerModule(viewBox, drawableModulesCount);
            const int offsetModules = 0;

            GeometryGroup group = new GeometryGroup();
            markerGroup = new GeometryGroup();
            contentGroup = new GeometryGroup();
            double x = 0d;
            for (int xi = offsetModules; xi < drawableModulesCount + offsetModules; xi++)
            {
                double y = 0d;
                for (int yi = offsetModules; yi < drawableModulesCount + offsetModules; yi++)
                {
                    if (Code[yi, xi])
                    {
                        RadiusFilterKind filterKind = GetRadiusFilterKind(xi, yi);
                        PathGeometry geometry = new PathGeometry
                        {
                            Figures = new PathFigureCollection
                                {
                                    CreateRoundedRectanglePath(x, y, unitsPerModule, filterKind)
                                }
                        };
                        group.Children.Add(geometry);
                        if (CheckIsPositionMarker(xi, yi, drawableModulesCount, positionMarkerCount))
                        {
                            markerGroup.Children.Add(geometry);
                        }
                        else
                        {
                            contentGroup.Children.Add(geometry);
                        }
                    }
                    else
                    {
                        RadiusFilterKind filterKind = GetAntiRadiusFilterKind(xi, yi);
                        PathGeometry geometry = new PathGeometry
                        {
                            Figures = CreateEntiRoundedRectanglePath(x, y, unitsPerModule, filterKind)
                        };
                        group.Children.Add(geometry);
                        if (CheckIsPositionMarker(xi, yi, drawableModulesCount, positionMarkerCount))
                        {
                            markerGroup.Children.Add(geometry);
                        }
                        else
                        {
                            contentGroup.Children.Add(geometry);
                        }
                    }
                    y += unitsPerModule;
                }
                x += unitsPerModule;
            }
            return group;
        }

        private int GetDrawableModulesCount()
        {
            return Code.Height;
        }

        private int GetPositionMarkerCount(int drawableModulesCount)
        {
            const int offsetModules = 0;
            for (int xi = offsetModules; xi < drawableModulesCount + offsetModules; xi++)
            {
                if (!Code[offsetModules, xi])
                {
                    return xi - offsetModules;
                }
            }
            return -1;
        }

        private static double GetUnitsPerModule(Size viewBox, int drawableModulesCount)
        {
            double qrSize = Math.Min(viewBox.Width, viewBox.Height);
            return qrSize / drawableModulesCount;
        }

        private static bool CheckIsPositionMarker(int xi, int yi, int drawableModulesCount, int positionMarkerCount)
        {
            const int offsetModules = 0;
            return xi >= offsetModules && yi >= offsetModules
&& ((xi < positionMarkerCount + offsetModules && yi < positionMarkerCount + offsetModules)
|| (xi >= drawableModulesCount + offsetModules - positionMarkerCount
                                                    && xi < drawableModulesCount + offsetModules
                                                    && yi < positionMarkerCount + offsetModules)
                || (yi >= drawableModulesCount + offsetModules - positionMarkerCount
                                                                    && yi < drawableModulesCount + offsetModules
                                                                    && xi < positionMarkerCount + offsetModules));
        }

        private static PathFigure CreateRectanglePath(double x, double y, double unitsPerModule)
        {
            PathFigure PathFigure = new PathFigure
            {
                IsClosed = true,
                StartPoint = new Point(x, y),
                Segments = new PathSegmentCollection()
            };

            PathFigure.Segments.Add(
                new LineSegment
                {
                    Point = new Point(x + unitsPerModule, y)
                }
            );
            PathFigure.Segments.Add(
                new LineSegment
                {
                    Point = new Point(x + unitsPerModule, y+ unitsPerModule)
                }
            );
            PathFigure.Segments.Add(
                new LineSegment
                {
                    Point = new Point(x, y + unitsPerModule)
                }
            );
            return PathFigure;
        }

        private static PathFigure CreateRoundedRectanglePath(double x, double y, double unitsPerModule, RadiusFilterKind filterKind)
        {
            double x_middle = x + (unitsPerModule / 2);
            double x_end = x + unitsPerModule;
            double y_middle = y + (unitsPerModule / 2);
            double y_end = y + unitsPerModule;
            Size arcSize = new Size(unitsPerModule / 2, unitsPerModule / 2);

            PathFigure PathFigure = new PathFigure
            {
                IsClosed = true,
                StartPoint = new Point(x_middle, y),
                Segments = new PathSegmentCollection()
            };

            if (filterKind.HasFlag(RadiusFilterKind.TopLeft))
            {
                PathFigure.Segments.Add(
                    new ArcSegment
                    {
                        Point = new Point(x, y_middle),
                        Size = arcSize
                    });
            }
            else
            {
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x, y)
                    });
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x, y_middle)
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.BottomLeft))
            {
                PathFigure.Segments.Add(
                    new ArcSegment
                    {
                        Point = new Point(x_middle, y_end),
                        Size = arcSize
                    });
            }
            else
            {
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x, y_end)
                    });
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x_middle, y_end)
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.BottomRight))
            {
                PathFigure.Segments.Add(
                    new ArcSegment
                    {
                        Point = new Point(x_end, y_middle),
                        Size = arcSize
                    });
            }
            else
            {
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x_end, y_end)
                    });
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x_end, y_middle)
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.TopRight))
            {
                PathFigure.Segments.Add(
                    new ArcSegment
                    {
                        Point = new Point(x_middle, y),
                        Size = arcSize
                    });
            }
            else
            {
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x_end, y)
                    });
                PathFigure.Segments.Add(
                    new LineSegment
                    {
                        Point = new Point(x_middle, y)
                    });
            }

            return PathFigure;
        }

        private static PathFigureCollection CreateEntiRoundedRectanglePath(double x, double y, double unitsPerModule, RadiusFilterKind filterKind)
        {
            double x_middle = x + (unitsPerModule / 2);
            double x_end = x + unitsPerModule;
            double y_middle = y + (unitsPerModule / 2);
            double y_end = y + unitsPerModule;
            Size arcSize = new Size(unitsPerModule / 2, unitsPerModule / 2);

            PathFigureCollection pathFigures = new PathFigureCollection();

            if (filterKind.HasFlag(RadiusFilterKind.TopLeft))
            {
                pathFigures.Add(
                    new PathFigure
                    {
                        IsClosed = true,
                        StartPoint = new Point(x_middle, y),
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(x, y_middle),
                                Size = arcSize
                            },
                            new LineSegment
                            {
                                Point = new Point(x, y)
                            },
                            new LineSegment
                            {
                                Point = new Point(x_middle, y)
                            }
                        }
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.BottomLeft))
            {
                pathFigures.Add(
                    new PathFigure
                    {
                        IsClosed = true,
                        StartPoint = new Point(x, y_middle),
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(x_middle, y_end),
                                Size = arcSize
                            },
                            new LineSegment
                            {
                                Point = new Point(x, y_end)
                            },
                            new LineSegment
                            {
                                Point = new Point(x, y_middle)
                            }
                        }
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.BottomRight))
            {
                pathFigures.Add(
                    new PathFigure
                    {
                        IsClosed = true,
                        StartPoint = new Point(x_middle, y_end),
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(x_end, y_middle),
                                Size = arcSize
                            },
                            new LineSegment
                            {
                                Point = new Point(x_end, y_end)
                            },
                            new LineSegment
                            {
                                Point = new Point(x_middle, y_end)
                            }
                        }
                    });
            }

            if (filterKind.HasFlag(RadiusFilterKind.TopRight))
            {
                pathFigures.Add(
                    new PathFigure
                    {
                        IsClosed = true,
                        StartPoint = new Point(x_end, y_middle),
                        Segments = new PathSegmentCollection
                        {
                            new ArcSegment
                            {
                                Point = new Point(x_middle, y),
                                Size = arcSize
                            },
                            new LineSegment
                            {
                                Point = new Point(x_end, y)
                            },
                            new LineSegment
                            {
                                Point = new Point(x_end, y_middle)
                            }
                        }
                    });
            }

            return pathFigures;
        }

        private RadiusFilterKind GetRadiusFilterKind(int xi, int yi)
        {
            RadiusFilterKind radiusFilterKind = RadiusFilterKind.None;
            if (yi - 1 < 0 || !Code[yi - 1, xi])
            {
                if (xi - 1 < 0 || !Code[yi, xi - 1])
                {
                    radiusFilterKind |= RadiusFilterKind.TopLeft;
                }
                if (xi + 1 >= Code.Width || !Code[yi, xi + 1])
                {
                    radiusFilterKind |= RadiusFilterKind.TopRight;
                }
            }
            if (yi + 1 >= Code.Width || !Code[yi + 1, xi])
            {
                if (xi - 1 < 0 || !Code[yi, xi - 1])
                {
                    radiusFilterKind |= RadiusFilterKind.BottomLeft;
                }
                if (xi + 1 >= Code.Width || !Code[yi, xi + 1])
                {
                    radiusFilterKind |= RadiusFilterKind.BottomRight;
                }
            }
            return radiusFilterKind;
        }

        private RadiusFilterKind GetAntiRadiusFilterKind(int xi, int yi)
        {
            RadiusFilterKind radiusFilterKind = RadiusFilterKind.None;
            if (yi - 1 >= 0 && Code[yi - 1, xi])
            {
                if (xi - 1 >= 0 && (Code[yi, xi - 1] && Code[yi - 1, xi - 1]))
                {
                    radiusFilterKind |= RadiusFilterKind.TopLeft;
                }
                if (xi + 1 < Code.Width && (Code[yi, xi + 1] && Code[yi - 1, xi + 1]))
                {
                    radiusFilterKind |= RadiusFilterKind.TopRight;
                }
            }
            if (yi + 1 < Code.Height && Code[yi + 1, xi])
            {
                if (xi - 1 >= 0 && (Code[yi, xi - 1] && Code[yi + 1, xi - 1]))
                {
                    radiusFilterKind |= RadiusFilterKind.BottomLeft;
                }
                if (xi + 1 < Code.Width && (Code[yi, xi + 1] && Code[yi + 1, xi + 1]))
                {
                    radiusFilterKind |= RadiusFilterKind.BottomRight;
                }
            }
            return radiusFilterKind;
        }

    }

    [Flags]
    public enum RadiusFilterKind : byte
    {
        None = 0x00,
        TopLeft = 0x01,
        TopRight = 0x02,
        BottomLeft = 0x04,
        BottomRight = 0x08,
    }
}
