﻿using GeomPad.Helpers;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public class GeometryUtil
    {
        public class PolygonBounds
        {
            public double X;
            public double Y;
            public double Width;
            public double Height;
            public PolygonBounds(double _x, double _y, double _w, double _h)
            {
                X = _x;
                Y = _y;
                Width = _w;
                Height = _h;
            }
        }
        public static PolygonBounds GetPolygonBounds(NFP _polygon)
        {
            return GetPolygonBounds(_polygon.Points);
        }
        public static PolygonBounds GetPolygonBounds(SvgPoint[] polygon)
        {

            if (polygon == null || polygon.Count() < 3)
            {
                throw new ArgumentException("null");
            }

            var xmin = polygon[0].X;
            var xmax = polygon[0].X;
            var ymin = polygon[0].Y;
            var ymax = polygon[0].Y;

            for (var i = 1; i < polygon.Length; i++)
            {
                if (polygon[i].X > xmax)
                {
                    xmax = polygon[i].X;
                }
                else if (polygon[i].X < xmin)
                {
                    xmin = polygon[i].X;
                }

                if (polygon[i].Y > ymax)
                {
                    ymax = polygon[i].Y;
                }
                else if (polygon[i].Y < ymin)
                {
                    ymin = polygon[i].Y;
                }
            }

            var w = xmax - xmin;
            var h = ymax - ymin;
            //return new rectanglef(xmin, ymin, xmax - xmin, ymax - ymin);
            return new PolygonBounds(xmin, ymin, w, h);


        }

        // returns true if points are within the given distance
        public static bool _withinDistance(SvgPoint p1, SvgPoint p2, double distance)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return ((dx * dx + dy * dy) < distance * distance);
        }
        public static Vector2d RotatePoint(Vector2d p, double cx, double cy, double angle)
        {
            return new Vector2d(Math.Cos(angle) * (p.X - cx) - Math.Sin(angle) * (p.Y - cy) + cx,
                         Math.Sin(angle) * (p.X - cx) + Math.Cos(angle) * (p.Y - cy) + cy);
        }

        public static double GetMinimumBoxAngle(Vector2d[] vv)
        {
            var hull = DeepNest.getHull(new NFP() { Points = vv.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            double minArea = double.MaxValue;
            
            double ret = 0;
            for (int i = 0; i < hull.Length; i++)
            {
                var p0 = hull.Points[i];
                var p1 = hull.Points[(i + 1) % hull.Length];
                var dx = p1.X - p0.X;
                var dy = p1.Y - p0.Y;
                var atan = Math.Atan2(dy, dx);

                List<Vector2d> dd = new List<Vector2d>();
                for (int j = 0; j < vv.Length; j++)
                {
                    var r = RotatePoint(new Vector2d(vv[j].X, vv[j].Y), 0, 0, -atan);
                    dd.Add(r);
                }
                var maxx = dd.Max(z => z.X);
                var maxy = dd.Max(z => z.Y);
                var minx = dd.Min(z => z.X);
                var miny = dd.Min(z => z.Y);

                var area = (maxx - minx) * (maxy - miny);

                if (area < minArea)
                {
                    minArea = area;                    
                    ret = atan;
                }
            }

            return -ret;
        }
        public static Vector2d[] GetMinimumBox(Vector2d[] vv)
        {
            var hull = DeepNest.getHull(new NFP() { Points = vv.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            double minArea = double.MaxValue;
            List<Vector2d> rect = new List<Vector2d>();
            for (int i = 0; i < hull.Length; i++)
            {
                var p0 = hull.Points[i];
                var p1 = hull.Points[(i + 1) % hull.Length];
                var dx = p1.X - p0.X;
                var dy = p1.Y - p0.Y;
                var atan = Math.Atan2(dy, dx);

                List<Vector2d> dd = new List<Vector2d>();
                for (int j = 0; j < vv.Length; j++)
                {
                    var r = RotatePoint(new Vector2d(vv[j].X, vv[j].Y), 0, 0, -atan);
                    dd.Add(r);
                }
                var maxx = dd.Max(z => z.X);
                var maxy = dd.Max(z => z.Y);
                var minx = dd.Min(z => z.X);
                var miny = dd.Min(z => z.Y);

                var area = (maxx - minx) * (maxy - miny);

                if (area < minArea)
                {
                    minArea = area;
                    rect.Clear();

                    rect.Add(new Vector2d(minx, miny));
                    rect.Add(new Vector2d(maxx, miny));
                    rect.Add(new Vector2d(maxx, maxy));
                    rect.Add(new Vector2d(minx, maxy));
                    for (int j = 0; j < rect.Count; j++)
                    {
                        rect[j] = RotatePoint(new Vector2d(rect[j].X, rect[j].Y), 0, 0, atan);
                    }
                }
            }

            return rect.ToArray();
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

        public static double ToDegrees(double ang)
        {
            return ang * 180f / Math.PI;
        }

        static double TOL = (float)Math.Pow(10, -9); // Floating point error is likely to be above 1 epsilon
                                                     // returns true if p lies on the line segment defined by AB, but not at any endpoints
                                                     // may need work!
    }
}