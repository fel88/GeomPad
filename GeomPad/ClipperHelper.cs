using ClipperLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public class ClipperHelper
    {
        public static NFP clipperToSvg(IList<IntPoint> polygon, double clipperScale = 10000000)
        {
            List<SvgPoint> ret = new List<SvgPoint>();

            for (var i = 0; i < polygon.Count; i++)
            {
                ret.Add(new SvgPoint(polygon[i].X / clipperScale, polygon[i].Y / clipperScale));
            }

            return new NFP() { Points = ret.ToArray() };
        }

        public static IntPoint[] ScaleUpPaths(NFP p, double scale = 10000000)
        {
            List<IntPoint> ret = new List<IntPoint>();

            for (int i = 0; i < p.Points.Count(); i++)
            {
                ret.Add(new ClipperLib.IntPoint(
                    (long)Math.Round((decimal)p.Points[i].X * (decimal)scale),
                    (long)Math.Round((decimal)p.Points[i].Y * (decimal)scale)
                ));

            }
            return ret.ToArray();
        }

        public static NFP[] offset(NFP polygon, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ScaleUpPaths(polygon, clipperScale).ToList();

            var co = new ClipperLib.ClipperOffset(miterLimit, curveTolerance * clipperScale);
            co.AddPath(p.ToList(), jType, ClipperLib.EndType.etClosedPolygon);

            var newpaths = new List<List<ClipperLib.IntPoint>>();
            co.Execute(ref newpaths, offset * clipperScale);

            var result = new List<NFP>();
            for (var i = 0; i < newpaths.Count; i++)
            {
                result.Add(clipperToSvg(newpaths[i]));
            }
            return result.ToArray();
        }
        public static IntPoint[][] nfpToClipperCoordinates(NFP nfp, double clipperScale)
        {

            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

            // children first
            if (nfp.Childrens != null && nfp.Childrens.Count > 0)
            {
                for (var j = 0; j < nfp.Childrens.Count; j++)
                {
                    if (GeometryUtil.polygonArea(nfp.Childrens[j]) < 0)
                    {
                        nfp.Childrens[j].reverse();
                    }
                    //var childNfp = SvgNest.toClipperCoordinates(nfp.children[j]);
                    var childNfp = ScaleUpPaths(nfp.Childrens[j], clipperScale);
                    clipperNfp.Add(childNfp);
                }
            }

            if (GeometryUtil.polygonArea(nfp) > 0)
            {
                nfp.reverse();
            }


            //var outerNfp = SvgNest.toClipperCoordinates(nfp);

            // clipper js defines holes based on orientation

            var outerNfp = ScaleUpPaths(nfp, clipperScale);

            //var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);

            clipperNfp.Add(outerNfp);
            //var area = Math.abs(ClipperLib.Clipper.Area(cleaned));

            return clipperNfp.ToArray();
        }
        public static IntPoint[][] ToClipperCoordinates(NFP[] nfp, double clipperScale)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                var clip = nfpToClipperCoordinates(nfp[i], clipperScale);
                clipperNfp.AddRange(clip);
            }

            return clipperNfp.ToArray();
        }
        public static NFP toNestCoordinates(IntPoint[] polygon, double scale)
        {
            var clone = new List<SvgPoint>();

            for (var i = 0; i < polygon.Count(); i++)
            {
                clone.Add(new SvgPoint(
                     polygon[i].X / scale,
                             polygon[i].Y / scale
                        ));
            }
            return new NFP() { Points = clone.ToArray() };
        }
        public static NFP[] intersection(NFP polygon, NFP polygon1, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ToClipperCoordinates(new[] { polygon }, clipperScale).ToList();
            var p1 = ToClipperCoordinates(new[] { polygon1 }, clipperScale).ToList();

            Clipper clipper = new Clipper();
            clipper.AddPaths(p.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
            clipper.AddPaths(p1.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

            List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
            if (clipper.Execute(ClipType.ctIntersection, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero) && finalNfp != null && finalNfp.Count > 0)
            {
                return finalNfp.Select(z=>toNestCoordinates(z.ToArray(), clipperScale)).ToArray();
            }
            return null;
        }
    }
}