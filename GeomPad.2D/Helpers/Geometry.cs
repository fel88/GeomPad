using ClipperLib;
using GeomPad.Common;
using OpenTK;
using System;
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
        public static PolylineHelper Offset(PolylineHelper plh2, double offset, JoinType jType, double curveTolerance, double miterLimit)
        {
            NFP p = new NFP();

            p.Points = plh2.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();

            var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);

            PolylineHelper ph = new PolylineHelper();

            if (offs.Any())
            {
                ph.Points = offs.First().Points.Select(z => new Vector2d(z.X, z.Y)).ToList();
                ph.Points.Add(ph.Points[0]);
            }
            return ph;
        }
        public static PolygonHelper Offset(PolygonHelper ph2, double offset, JoinType jType, double curveTolerance, double miterLimit)
        {
            NFP p = new NFP();
            p.Points = ph2.Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();

            var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
            //if (offs.Count() > 1) throw new NotImplementedException();
            PolygonHelper ph = new PolygonHelper();
            foreach (var item in ph2.Polygon.Childrens)
            {
                var offs2 = ClipperHelper.offset(item, -offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                var nfp1 = new NFP();
                if (offs2.Any())
                {
                    //if (offs2.Count() > 1) throw new NotImplementedException();
                    foreach (var zitem in offs2)
                    {
                        nfp1.Points = zitem.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                        ph.Polygon.Childrens.Add(nfp1);
                    }
                }
            }

            if (offs.Any())
            {
                ph.Polygon.Points = offs.First().Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            }

            foreach (var item in offs.Skip(1))
            {
                var nfp2 = new NFP();

                nfp2.Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon.Childrens.Add(nfp2);

            }

            ph.OffsetX = ph2.OffsetX;
            ph.OffsetY = ph2.OffsetY;
            ph.Rotation = ph2.Rotation;
            return ph;

        }
    }
}