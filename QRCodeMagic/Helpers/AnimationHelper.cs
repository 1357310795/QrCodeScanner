using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace QRCodeMagic.Helpers
{
    public class AnimationHelper
    {
        public static DoubleAnimationUsingKeyFrames CubicBezierDoubleAnimation(TimeSpan d, double s, double t, string Bezier)
        {
            DoubleKeyFrame dkf = new LinearDoubleKeyFrame();
            dkf.KeyTime = TimeSpan.FromSeconds(0.0);
            dkf.Value = s;
            SplineDoubleKeyFrame sp = new SplineDoubleKeyFrame();
            sp.KeyTime = d;
            string[] p = Bezier.Split(new char[]
            {
                ','
            });
            Point controlPoint = new Point(double.Parse(p[0]), double.Parse(p[1]));
            Point controlPoint2 = new Point(double.Parse(p[2]), double.Parse(p[3]));
            sp.KeySpline = new KeySpline
            {
                ControlPoint1 = controlPoint,
                ControlPoint2 = controlPoint2
            };
            sp.Value = t;
            return new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    dkf,
                    sp
                },
            };
        }

        public static DoubleAnimationUsingKeyFrames CubicBezierDoubleAnimation(TimeSpan st, TimeSpan d, double s, double t, string Bezier)
        {
            DoubleKeyFrame dkf = new LinearDoubleKeyFrame();
            dkf.KeyTime = TimeSpan.FromSeconds(0.0);
            dkf.Value = s;
            DoubleKeyFrame dkf2 = new LinearDoubleKeyFrame();
            dkf2.KeyTime = st;
            dkf2.Value = s;
            SplineDoubleKeyFrame sp = new SplineDoubleKeyFrame();
            sp.KeyTime = st + d;
            string[] p = Bezier.Split(new char[]
            {
                ','
            });
            Point controlPoint = new Point(double.Parse(p[0]), double.Parse(p[1]));
            Point controlPoint2 = new Point(double.Parse(p[2]), double.Parse(p[3]));
            sp.KeySpline = new KeySpline
            {
                ControlPoint1 = controlPoint,
                ControlPoint2 = controlPoint2
            };
            sp.Value = t;
            return new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    dkf,
                    dkf2,
                    sp
                },
                FillBehavior = FillBehavior.HoldEnd
            };
        }
    }
}
