using System.Collections.Generic;

namespace GeomPad.Helpers
{
    public class RectangleGenerator : PolygonHelper
    {
        public static int Index;
        public RectangleGenerator()
        {
            Name = $"rect{Index++}";
            _update();
        }
        double _width = 100;
        double _height = 50;
        double _x;
        double _y;
        public double Width { get => _width; set { _width = value; _update(); } }
        public double Height { get => _height; set { _height = value; _update(); } }
        public double X { get => _x; set { _x = value; _update(); } }
        public double Y { get => _y; set { _y = value; _update(); } }

        void _update()
        {
            List<SvgPoint> pnts = new List<SvgPoint>();
            pnts.Add(new SvgPoint(X, Y));
            pnts.Add(new SvgPoint(X + Width, Y));
            pnts.Add(new SvgPoint(X + Width, Y + Height));
            pnts.Add(new SvgPoint(X, Y + Height));
            Polygon.Points = pnts.ToArray();
        }
    }
}
