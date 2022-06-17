using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MyQrCodeScanner
{
    public class StarInfo
    {
        public double X { get; set; }
        public double XV { get; set; }
        public int XT { get; set; }
        public double Y { get; set; }
        public double YV { get; set; }
        public int YT { get; set; }
        public Path StarRef { get; set; }
        public Dictionary<StarInfo, Line> StarLines { get; set; }
        public StarAnimationLoop animationLoop { get; set; }
    }

    public class StarAnimationLoop
    {
        public void RestartAnimation(object sender, EventArgs e)
        {
            sa.NewStarAnimation(this.path, starInfo);
        }
        public StarAnimation sa;
        public StarInfo starInfo;
        public Path path;
        public Storyboard storyboard;
    }

    public class AfterLineAnimation
    {
        internal void DelLine(object sender, EventArgs e)
        {
            StarAnimation.grid.Children.Remove(this.line);
        }
        public Line line;
    }

    public class StarAnimation
    {
        public static Grid grid;
        public static Canvas canvas;
        public static Window window;
        private readonly Geometry geo = Geometry.Parse("M16.001007,0L20.944,10.533997 32,12.223022 23.998993,20.421997 25.889008,32 16.001007,26.533997 6.1109924,32 8,20.421997 0,12.223022 11.057007,10.533997z");
        private static int count = 50;
        private static readonly int minSize = 9;
        private static readonly int maxSize = 16;
        private static int speed = 30;
        private readonly int minAngle = 45;
        private readonly int maxAngle = 90;
        private static int maxlength = 4;
        private static bool ison;
        private readonly Random random = new Random();
        private static StarInfo[] stars;
    
        private LinearGradientBrush NewBrush(Path p1, Path p2)
        {
            return new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop
                    {
                        Offset = 0.0,
                        Color = (p1.Fill as SolidColorBrush).Color
                    },
                    new GradientStop
                    {
                        Offset = 1.0,
                        Color = (p2.Fill as SolidColorBrush).Color
                    }
                }
            };
        }
        public StarAnimation()
        {
        }
        public StarAnimation(Grid grid, Canvas canvas, Window targeteWindow)
        {
            if (StarAnimation.grid == null)
            {
                CompositionTarget.Rendering += this.Onrender;
            }
            StarAnimation.grid = grid;
            StarAnimation.canvas = canvas;
            StarAnimation.window = targeteWindow;
        }
        public void Start()
        {
            ison = true;
            foreach (StarInfo starInfo in stars)
            {
                starInfo.animationLoop.storyboard.Begin(StarAnimation.window);
            }
        }
        public void Close()
        {
            StarAnimation.canvas.Children.Clear();
            StarAnimation.grid.Children.Clear();
            StarAnimation.stars = null;
            ison = false;
        }
        public void Pause()
        {
            ison = false;
            foreach (StarInfo starInfo in stars)
            {
                starInfo.animationLoop.storyboard.Stop(StarAnimation.window);
            }
        }
        public void SetStarNumber(int number)
        {
            if (number != StarAnimation.count)
            {
                StarAnimation.count = number;
                //if (StarAnimation.grid != null)
                //{
                //    this.Starinit();
                //}
            }
        }
        public void SetStarSpeed(int speed)
        {
            if (speed != StarAnimation.speed)
            {
                StarAnimation.speed = speed;
            }
        }
        public void SetLineMaxLength(int maxlength)
        {
            if (maxlength != StarAnimation.maxlength)
            {
                StarAnimation.maxlength = maxlength;
            }
        }
        private void Onrender(object sender, EventArgs e)
        {
            if (!ison) return;
            this.UpdateStars();
            this.UpdateLines();
        }
        public void Init()
        {
            StarAnimation.stars = new StarInfo[StarAnimation.count];
            StarAnimation.canvas.Children.Clear();
            StarAnimation.grid.Children.Clear();
            for (int i = 0; i < StarAnimation.count; i++)
            {
                double num = (double)this.random.Next(StarAnimation.minSize, StarAnimation.maxSize + 1);
                StarInfo starInfo = new StarInfo
                {
                    X = (double)this.random.Next(0, (int)StarAnimation.canvas.ActualWidth),
                    XV = (double)this.random.Next(-speed, speed) / 60.0,
                    XT = this.random.Next(6, 301),
                    Y = (double)this.random.Next(0, (int)StarAnimation.canvas.ActualHeight),
                    YV = (double)this.random.Next(-speed, speed) / 60.0,
                    YT = this.random.Next(6, 301),
                    StarLines = new Dictionary<StarInfo, Line>()
                };
                Path path = new Path
                {
                    Data = this.geo,
                    Width = num,
                    Height = num,
                    Stretch = Stretch.Uniform,
                    Fill = this.RandomColor(),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform
                    {
                        Angle = 0.0
                    }
                };
                Canvas.SetLeft(path, starInfo.X);
                Canvas.SetTop(path, starInfo.Y);
                starInfo.StarRef = path;
                this.NewStarAnimation(path, starInfo);
                StarAnimation.stars[i] = starInfo;
                StarAnimation.canvas.Children.Add(path);
            }
        }
        private SolidColorBrush RandomColor()
        {
            byte r = (byte)this.random.Next(100, 256);
            byte b = (byte)this.random.Next(100, 256);
            byte b2 = (byte)this.random.Next(100, 256);
            return new SolidColorBrush(Color.FromRgb(r, b, b2));
        }
        public void NewStarAnimation(Path path, StarInfo starInfo)
        {
            StarAnimationLoop loop = new StarAnimationLoop();
            loop.sa = this;
            loop.path = path;
            loop.starInfo = starInfo;
            double num = (double)this.random.Next(this.minAngle, this.maxAngle + 1);
            double num2 = (double)this.random.Next(150, 720);
            int num3 = (int)(num2 / num * 1000.0);
            Storyboard storyboard = new Storyboard();
            storyboard.Duration = TimeSpan.FromMilliseconds((double)num3);
            storyboard.Completed += loop.RestartAnimation;
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                To = new double?(num2),
                Duration = TimeSpan.FromMilliseconds((double)num3)
            };
            Storyboard.SetTarget(doubleAnimation, loop.path);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)", Array.Empty<object>()));
            storyboard.Children.Add(doubleAnimation);
            loop.storyboard = storyboard;
            starInfo.animationLoop = loop;
        }
        private void UpdateStars()
        {
            if (StarAnimation.stars == null)
            {
                return;
            }
            foreach (StarInfo starInfo in StarAnimation.stars)
            {
                if (starInfo.XT > 0)
                {
                    if (starInfo.X >= StarAnimation.canvas.ActualWidth || starInfo.X <= 0.0)
                    {
                        starInfo.XV = -starInfo.XV;
                    }
                    starInfo.X += starInfo.XV;
                    StarInfo starInfo2 = starInfo;
                    int num = starInfo2.XT;
                    starInfo2.XT = num - 1;
                    Canvas.SetLeft(starInfo.StarRef, starInfo.X);
                }
                else
                {
                    starInfo.XV = (double)this.random.Next(-speed, speed) / 60.0;
                    starInfo.XT = this.random.Next(100, 1001);
                }
                if (starInfo.YT > 0)
                {
                    if (starInfo.Y >= StarAnimation.canvas.ActualHeight || starInfo.Y <= 0.0)
                    {
                        starInfo.YV = -starInfo.YV;
                    }
                    starInfo.Y += starInfo.YV;
                    StarInfo starInfo3 = starInfo;
                    int num = starInfo3.YT;
                    starInfo3.YT = num - 1;
                    Canvas.SetTop(starInfo.StarRef, starInfo.Y);
                }
                else
                {
                    starInfo.YV = (double)this.random.Next(-speed, speed) / 60.0;
                    starInfo.YT = this.random.Next(100, 1001);
                }
            }
        }
        private void UpdateLines()
        {
            if (StarAnimation.stars == null)
            {
                return;
            }
            for (int i = 0; i < StarAnimation.count - 1; i++)
            {
                for (int j = i + 1; j < StarAnimation.count; j++)
                {
                    StarInfo starInfo = StarAnimation.stars[i];
                    double num = starInfo.X + starInfo.StarRef.Width / 2.0;
                    double num2 = starInfo.Y + starInfo.StarRef.Height / 2.0;
                    StarInfo starInfo2 = StarAnimation.stars[j];
                    double num3 = starInfo2.X + starInfo2.StarRef.Width / 2.0;
                    double num4 = starInfo2.Y + starInfo2.StarRef.Height / 2.0;
                    double num5 = System.Math.Sqrt((num4 - num2) * (num4 - num2) + (num3 - num) * (num3 - num));
                    double num6 = starInfo.StarRef.Width * (double)StarAnimation.maxlength + starInfo2.StarRef.Width * (double)StarAnimation.maxlength;
                    if (num5 <= num6)
                    {
                        if (!starInfo.StarLines.ContainsKey(starInfo2))
                        {
                            Line line = new Line
                            {
                                X1 = num,
                                Y1 = num2,
                                X2 = num3,
                                Y2 = num4,
                                Stroke = this.NewBrush(starInfo.StarRef, starInfo2.StarRef)
                            };
                            DoubleAnimation animation = new DoubleAnimation
                            {
                                From = new double?(0.0),
                                To = new double?((double)1),
                                Duration = new Duration(TimeSpan.FromMilliseconds(800.0))
                            };
                            line.BeginAnimation(UIElement.OpacityProperty, animation);
                            starInfo.StarLines.Add(starInfo2, line);
                            StarAnimation.grid.Children.Add(line);
                        }
                    }
                    else if (starInfo.StarLines.ContainsKey(starInfo2))
                    {
                        AfterLineAnimation b = new AfterLineAnimation();
                        b.line = starInfo.StarLines[starInfo2];
                        DoubleAnimation doubleAnimation = new DoubleAnimation
                        {
                            To = new double?(0.0),
                            Duration = new Duration(TimeSpan.FromMilliseconds(200.0))
                        };
                        doubleAnimation.Completed += b.DelLine;
                        b.line.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
                        starInfo.StarLines.Remove(starInfo2);
                    }
                }
            }
            foreach (StarInfo starInfo in StarAnimation.stars)
            {
                foreach (KeyValuePair<StarInfo, Line> keyValuePair in starInfo.StarLines)
                {
                    Line value = keyValuePair.Value;
                    value.X1 = starInfo.X + starInfo.StarRef.Width / 2.0;
                    value.Y1 = starInfo.Y + starInfo.StarRef.Height / 2.0;
                    value.X2 = keyValuePair.Key.X + keyValuePair.Key.StarRef.Width / 2.0;
                    value.Y2 = keyValuePair.Key.Y + keyValuePair.Key.StarRef.Height / 2.0;
                }
            }
        }
    }
}
