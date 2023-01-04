using GeomPad.Common;
using OpenTK;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace GeomPad.Helpers
{
    public static class Geometry
    {
        public static PolygonHelper ExtractMinAreaRect(IHelperItem[] items)
        {
            List<Vector2d> pnts = new List<Vector2d>();
            foreach (var item in items)
            {
                if (!(item is PolygonHelper ph2))
                    continue;

                var hull = DeepNest.getHull(new NFP() { Points = ph2.TransformedPoints().ToArray() });
                PolygonHelper ph = new PolygonHelper();
                ph.Polygon = hull;
                pnts.AddRange(hull.Points.Select(z => new Vector2d(z.X, z.Y)));
            }

            if (pnts.Count < 2)
                return null;

            var mar = GeometryUtil.GetMinimumBox(pnts.ToArray());
            PolygonHelper ph3 = new PolygonHelper();

            ph3.Polygon = new NFP()
            {
                Points = mar.Select(z => new SvgPoint(z.X, z.Y)).ToArray()
            };

            ph3.Name = "minRect";
            return ph3;
        }
        public static PolygonHelper ExtractAABB(IHelperItem[] items)
        {
            List<NFP> hulls = new List<NFP>();
            NFP hull = null;
            RectangleF? bbox = null;
            foreach (var item in items)
            {
                if (item is PolygonHelper ph2)
                {
                    hull = DeepNest.getHull(ph2.TransformedNfp());
                }
                else if (item is PolylineHelper plh2)
                {
                    NFP nfp = new NFP() { Points = plh2.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() };
                    hull = DeepNest.getHull(nfp);
                }

                if (hull == null) continue;
                hulls.Add(hull);
                PolygonHelper ph = new PolygonHelper();
                ph.Polygon = hull;

                var box1 = ph.BoundingBox().Value;
                if (bbox == null)
                    bbox = box1;
                else
                    bbox = RectangleF.Union(bbox.Value, box1);
            }

            if (bbox == null)
                return null;

            var box = bbox.Value;

            PolygonHelper ph3 = new PolygonHelper();
            ph3.Polygon = new NFP()
            {
                Points = new SvgPoint[] {
                    new SvgPoint (box.Left,box.Top),
                    new SvgPoint (box.Left,box.Bottom),
                    new SvgPoint (box.Right,box.Bottom),
                    new SvgPoint (box.Right,box.Top),
            }
            };

            ph3.Name = "AABB";
            ph3.RecalcArea();
            return ph3;
        }
    }
}