using System;
using System.Collections.Generic;

namespace GeomPad.Helpers
{
    public class CircleGenerator : PolygonHelper
    {
        public static int Index;
        public CircleGenerator()
        {
            Name = $"circle{Index++}";
            _update();
        }
        double _radius = 100;
        double _x;
        double _y;
        int _steps = 24;
        public double Radius { get => _radius; set { _radius = value; _update(); } }
        public double X { get => _x; set { _x = value; _update(); } }
        public double Y { get => _y; set { _y = value; _update(); } }

        public int Steps { get => _steps; set { _steps = value; _update(); } }
        void _update()
        {
            List<SvgPoint> pnts = new List<SvgPoint>();
            var step = 360 / Steps;
            for (int i = 0; i < 360; i += step)
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
