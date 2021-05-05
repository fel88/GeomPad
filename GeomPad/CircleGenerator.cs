using System;
using System.Collections.Generic;

namespace GeomPad
{
    public class CircleGenerator : PolygonHelper
    {
        public CircleGenerator()
        {
            _update();
        }
        double _radius = 100;
        double _x;
        double _y;
        public double Radius { get => _radius; set { _radius = value; _update(); } }
        public double X { get => _x; set { _x = value; _update(); } }
        public double Y { get => _y; set { _y = value; _update(); } }

        void _update()
        {
            List<SvgPoint> pnts = new List<SvgPoint>();
            for (int i = 0; i < 360; i += 15)
            {
                var ang = i / 180f * Math.PI;
                var x = X + Radius * Math.Cos(ang);
                var y = Y + Radius * Math.Sin(ang);
                pnts.Add(new SvgPoint(x, y));
            }
            Polygon.Points = pnts.ToArray();
        }
    }
}
