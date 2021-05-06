using System;

namespace GeomPad
{
    public class GeometryUtil
    {
        // returns true if points are within the given distance
        public static bool _withinDistance(SvgPoint p1, SvgPoint p2, double distance)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return ((dx * dx + dy * dy) < distance * distance);
        }

        public static double polygonArea(NFP polygon)
        {
            double area = 0;
            int i, j;
            for (i = 0, j = polygon.Points.Length - 1; i < polygon.Points.Length; j = i++)
            {
                area += (polygon.Points[j].X + polygon.Points[i].X) * (polygon.Points[j].Y
                    - polygon.Points[i].Y);
            }
            return 0.5f * area;
        }
        public static bool _almostEqual(double a, double b, double? tolerance = null)
        {
            if (tolerance == null)
            {
                tolerance = TOL;
            }
            return Math.Abs(a - b) < tolerance;
        }
        static double TOL = (float)Math.Pow(10, -9); // Floating point error is likely to be above 1 epsilon
                                                     // returns true if p lies on the line segment defined by AB, but not at any endpoints
                                                     // may need work!
    }
}