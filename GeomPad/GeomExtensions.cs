using GeomPad.Helpers;
using PolyBoolCS;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace GeomPad
{
    public static class GeomExtensions
    {
        public static PointList GetPointList(this  PointF[] pnts)
        {
            PointList plist = new PointList();

            foreach (var blankPoint in pnts)
            {
                plist.Add(new PolyBoolCS.Point(blankPoint.X, blankPoint.Y));
            }

            return plist;
        }
        public static PointList GetPointList(this SvgPoint[] pnts)
        {
            PointList plist = new PointList();
            foreach (var blankPoint in pnts)
            {
                plist.Add(new PolyBoolCS.Point(blankPoint.X, blankPoint.Y));
            }

            return plist;
        }

        public static PolyBoolCS.Polygon GetPolygon(this  PointF[] pnts)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }

        public static PolyBoolCS.Polygon GetPolygon(this  SvgPoint[] pnts)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }
        public static PolyBoolCS.Polygon GetPolygon(this  PolygonHelper ph)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            var pnts = ph.Polygon.Points.Select(z => ph.Transform(z)).ToArray();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            foreach (var item in ph.Polygon.Childrens)
            {

                pnts = item.Points.Select(z => ph.Transform(z)).ToArray();
                plist = GetPointList(pnts);
                p.regions.Add(plist);
            }
            return p;
        }
    }
}