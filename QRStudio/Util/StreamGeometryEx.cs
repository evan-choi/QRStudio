using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace QRStudio.Util
{
    public struct GeoPoint
    {
        public Point Point { get; set; }
        public bool IsBezier { get; set; }

        public GeoPoint(double x, double y, bool isBezier = false)
        {
            this.Point = new Point(x, y);
            this.IsBezier = isBezier;
        }
    }


    public static class StreamGeometryEx
    {
        private static Point PointEmpty = new Point(0, 0);

        public static StreamGeometry Create(params GeoPoint[] datas)
        {
            var sg = new StreamGeometry();

            if (datas.Length > 0)
            {
                using (var gc = sg.Open())
                {
                    gc.BeginFigure(datas[0].Point, true, true);
                    
                    for (int i = 1; i < datas.Length; i++)
                    {
                        if (datas[i].IsBezier)
                        {
                            gc.BezierTo(datas[i].Point, datas[i + 1].Point, datas[i + 2].Point, true, true);
                            i += 2;
                        }
                        else
                        {
                            gc.LineTo(datas[i].Point, true, true);
                        }
                    }
                }
            }

            return sg;
        }
    }
}
