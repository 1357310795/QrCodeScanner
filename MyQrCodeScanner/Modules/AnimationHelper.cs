using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace MyQrCodeScanner
{
    public class AnimationHelper
    {
        // Token: 0x06000075 RID: 117 RVA: 0x000038B8 File Offset: 0x00001AB8
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
            Point controlPoint = new Point(Conversions.ToDouble(p[0]), Conversions.ToDouble(p[1]));
            Point controlPoint2 = new Point(Conversions.ToDouble(p[2]), Conversions.ToDouble(p[3]));
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

        // Token: 0x06000076 RID: 118 RVA: 0x00003990 File Offset: 0x00001B90
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
            Point controlPoint = new Point(Conversions.ToDouble(p[0]), Conversions.ToDouble(p[1]));
            Point controlPoint2 = new Point(Conversions.ToDouble(p[2]), Conversions.ToDouble(p[3]));
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
