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

    }
}