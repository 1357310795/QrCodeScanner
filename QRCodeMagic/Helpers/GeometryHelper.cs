using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeMagic.Helpers
{
    public class GeometryHelper
    {
        public static System.Windows.Point Center(IReadOnlyList<System.Windows.Point> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            switch (points.Count)
            {
                case 0:
                    throw new ArgumentException("至少需要有一个点才能计算几何中心。", nameof(points));
                case 1:
                    return points[0];
                case 2:
                    return new System.Windows.Point((points[0].X + points[1].X) / 2, (points[0].Y + points[1].Y) / 2);
                default:
                    return GeometryCenter(points);
            }
        }

        private static System.Windows.Point GeometryCenter(IReadOnlyList<System.Windows.Point> points)
        {
            var center = new System.Windows.Point(points.Average(x => x.X), points.Average(x => x.Y));
            return center;
        }
    }
}
