using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeomPad
{
    public class DeepNest
    {
        public static NFP cleanPolygon2(NFP polygon, double clipperScale, double curveTolerance = 0.72)
        {
            var p = svgToClipper(polygon, clipperScale);
            // remove self-intersections and find the biggest polygon that's left
            var simple = ClipperLib.Clipper.SimplifyPolygon(p.ToList(), ClipperLib.PolyFillType.pftNonZero);

            if (simple == null || simple.Count == 0)
            {
                return null;
            }

            var biggest = simple[0];
            var biggestarea = Math.Abs(ClipperLib.Clipper.Area(biggest));
            for (var i = 1; i < simple.Count; i++)
            {
                var area = Math.Abs(ClipperLib.Clipper.Area(simple[i]));
                if (area > biggestarea)
                {
                    biggest = simple[i];
                    biggestarea = area;
                }
            }

            // clean up singularities, coincident points and edges
            var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 *
                curveTolerance * clipperScale);

            if (clean == null || clean.Count == 0)
            {
                return null;
            }
            var cleaned = clipperToSvg(clean, clipperScale);

            // remove duplicate endpoints
            var start = cleaned[0];
            var end = cleaned[cleaned.Length - 1];
            if (start == end || (GeometryUtil._almostEqual(start.X, end.X)
                && GeometryUtil._almostEqual(start.Y, end.Y)))
            {
                cleaned.Points = cleaned.Points.Take(cleaned.Points.Count() - 1).ToArray();
            }

            return cleaned;

        }
        // returns true if any complex vertices fall outside the simple polygon
        public static bool exterior(NFP simple, NFP complex, bool inside)
        {
            // find all protruding vertices
            for (var i = 0; i < complex.Length; i++)
            {
                var v = complex[i];
                if (!inside && !pointInPolygon(v, simple) && find(v, simple) == null)
                {
                    return true;
                }
                if (inside && pointInPolygon(v, simple) && find(v, simple) != null)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool pointInPolygon(SvgPoint point, NFP polygon)
        {
            // scaling is deliberately coarse to filter out points that lie *on* the polygon

            var p = svgToClipper2(polygon, 1000);
            var pt = new ClipperLib.IntPoint(1000 * point.X, 1000 * point.Y);

            return ClipperLib.Clipper.PointInPolygon(pt, p.ToList()) > 0;
        }

        // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
        public static IntPoint[] svgToClipper2(NFP polygon, double clipperScale, double? scale = null)
        {
            var d = ClipperHelper.ScaleUpPaths(polygon, scale == null ? clipperScale : scale.Value);
            return d.ToArray();

        }
        public static int? find(SvgPoint v, NFP p, double curveTolerance = 0.72)
        {
            for (var i = 0; i < p.Length; i++)
            {
                if (GeometryUtil._withinDistance(v, p[i], curveTolerance / 1000))
                {
                    return i;
                }
            }
            return null;
        }
        public class InrangeItem
        {
            public SvgPoint point;
            public double distance;
        }
        public static SvgPoint getTarget(SvgPoint o, NFP simple, double tol)
        {
            List<InrangeItem> inrange = new List<InrangeItem>();
            // find closest points within 2 offset deltas
            for (var j = 0; j < simple.Length; j++)
            {
                var s = simple[j];
                var d2 = (o.X - s.X) * (o.X - s.X) + (o.Y - s.Y) * (o.Y - s.Y);
                if (d2 < tol * tol)
                {
                    inrange.Add(new InrangeItem() { point = s, distance = d2 });
                }
            }

            SvgPoint target = null;
            if (inrange.Count > 0)
            {
                var filtered = inrange.Where((p) =>
                {
                    return p.point.exact;
                }).ToList();

                // use exact points when available, normal points when not
                inrange = filtered.Count > 0 ? filtered : inrange;


                inrange = inrange.OrderBy((b) =>
                {
                    return b.distance;
                }).ToList();

                target = inrange[0].point;
            }
            else
            {
                double? mind = null;
                for (int j = 0; j < simple.Length; j++)
                {
                    var s = simple[j];
                    var d2 = (o.X - s.X) * (o.X - s.X) + (o.Y - s.Y) * (o.Y - s.Y);
                    if (mind == null || d2 < mind)
                    {
                        target = s;
                        mind = d2;
                    }
                }
            }

            return target;
        }
        public static NFP getFrame(NFP A)
        {
            var bounds = GeometryUtil.GetPolygonBounds(A);

            // expand bounds by 10%
            bounds.Width *= 1.1;
            bounds.Height *= 1.1;
            bounds.X -= 0.5 * (bounds.Width - (bounds.Width / 1.1));
            bounds.Y -= 0.5 * (bounds.Height - (bounds.Height / 1.1));

            var frame = new NFP();
            frame.push(new SvgPoint(bounds.X, bounds.Y));
            frame.push(new SvgPoint(bounds.X + bounds.Width, bounds.Y));
            frame.push(new SvgPoint(bounds.X + bounds.Width, bounds.Y + bounds.Height));
            frame.push(new SvgPoint(bounds.X, bounds.Y + bounds.Height));


            frame.Childrens = new List<NFP>() { (NFP)A };



            return frame;
        }
        public static NFP[] Convolve(NFP A, NFP B)
        {
            return new[] { ClipperHelper.MinkowskiSum(A, B, true, false) };
        }

        //public static NFP[] Convolve(NFP A, NFP B)
        //{
        //    Dictionary<string, List<PointF>> dic1 = new Dictionary<string, List<PointF>>();
        //    Dictionary<string, List<double>> dic2 = new Dictionary<string, List<double>>();
        //    dic2.Add("A", new List<double>());
        //    foreach (var item in A.Points)
        //    {
        //        var target = dic2["A"];
        //        target.Add(item.X);
        //        target.Add(item.Y);
        //    }
        //    dic2.Add("B", new List<double>());
        //    foreach (var item in B.Points)
        //    {
        //        var target = dic2["B"];
        //        target.Add(item.X);
        //        target.Add(item.Y);
        //    }


        //    List<double> hdat = new List<double>();

        //    foreach (var item in A.Childrens)
        //    {
        //        foreach (var pitem in item.Points)
        //        {
        //            hdat.Add(pitem.X);
        //            hdat.Add(pitem.Y);
        //        }
        //    }

        //    var aa = dic2["A"];
        //    var bb = dic2["B"];
        //    var arr1 = A.Childrens.Select(z => z.Points.Count() * 2).ToArray();

        //    MinkowskiWrapper.setData(aa.Count, aa.ToArray(), A.Childrens.Count, arr1, hdat.ToArray(), bb.Count, bb.ToArray());
        //    MinkowskiWrapper.calculateNFP();



        //    int[] sizes = new int[2];
        //    MinkowskiWrapper.getSizes1(sizes);
        //    int[] sizes1 = new int[sizes[0]];
        //    int[] sizes2 = new int[sizes[1]];
        //    MinkowskiWrapper.getSizes2(sizes1, sizes2);
        //    double[] dat1 = new double[sizes1.Sum()];
        //    double[] hdat1 = new double[sizes2.Sum()];

        //    MinkowskiWrapper.getResults(dat1, hdat1);

        //    if (sizes1.Count() > 1)
        //    {
        //        throw new ArgumentException("sizes1 cnt >1");
        //    }


        //    //convert back to answer here
        //    bool isa = true;
        //    List<PointF> Apts = new List<PointF>();



        //    List<List<double>> holesval = new List<List<double>>();
        //    bool holes = false;

        //    for (int i = 0; i < dat1.Length; i += 2)
        //    {
        //        var x1 = (float)dat1[i];
        //        var y1 = (float)dat1[i + 1];
        //        Apts.Add(new PointF(x1, y1));
        //    }

        //    int index = 0;
        //    for (int i = 0; i < sizes2.Length; i++)
        //    {
        //        holesval.Add(new List<double>());
        //        for (int j = 0; j < sizes2[i]; j++)
        //        {
        //            holesval.Last().Add(hdat1[index]);
        //            index++;
        //        }
        //    }

        //    List<List<PointF>> holesout = new List<List<PointF>>();
        //    foreach (var item in holesval)
        //    {
        //        holesout.Add(new List<PointF>());
        //        for (int i = 0; i < item.Count; i += 2)
        //        {
        //            var x = (float)item[i];
        //            var y = (float)item[i + 1];
        //            holesout.Last().Add(new PointF(x, y));
        //        }
        //    }

        //    NFP ret = new NFP();
        //    ret.Points = new SvgPoint[] { };
        //    foreach (var item in Apts)
        //    {
        //        ret.AddPoint(new SvgPoint(item.X, item.Y));
        //    }


        //    foreach (var item in holesout)
        //    {
        //        if (ret.Childrens == null)
        //            ret.Childrens = new List<NFP>();

        //        ret.Childrens.Add(new NFP());
        //        ret.Childrens.Last().Points = new SvgPoint[] { };
        //        foreach (var hitem in item)
        //        {
        //            ret.Childrens.Last().AddPoint(new SvgPoint(hitem.X, hitem.Y));
        //        }
        //    }

        //    var res = new NFP[] { ret };
        //    return res;
        //}

        public static NFP getOuterNfp(NFP A, NFP B, bool inside = false)
        {
            NFP[] nfp = null;


            if (inside || (A.Childrens != null && A.Childrens.Count > 0))
            {
                nfp = DeepNest.Convolve(A, B);
            }
            else
            {
                var Ac = ClipperHelper.ScaleUpPaths(A, 10000000);

                var Bc = ClipperHelper.ScaleUpPaths(B, 10000000);
                for (var i = 0; i < Bc.Length; i++)
                {
                    Bc[i].X *= -1;
                    Bc[i].Y *= -1;
                }
                var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(Ac), new List<IntPoint>(Bc), true);
                NFP clipperNfp = null;

                double? largestArea = null;
                for (int i = 0; i < solution.Count(); i++)
                {
                    var n = toNestCoordinates(solution[i].ToArray(), 10000000);
                    var sarea = GeometryUtil.polygonArea(n);
                    if (largestArea == null || largestArea > sarea)
                    {
                        clipperNfp = n;
                        largestArea = sarea;
                    }
                }

                for (var i = 0; i < clipperNfp.Length; i++)
                {
                    clipperNfp[i].X += B[0].X;
                    clipperNfp[i].Y += B[0].Y;
                }
                nfp = new NFP[] { new NFP() { Points = clipperNfp.Points } };


            }

            if (nfp == null || nfp.Length == 0)
            {
                //console.log('holy shit', nfp, A, B, JSON.stringify(A), JSON.stringify(B));
                return null;
            }

            NFP nfps = nfp.First();

            if (nfps == null || nfps.Length == 0)
            {
                return null;
            }

            return nfps;



        }
        public static IntPoint[][] nfpToClipperCoordinates(NFP nfp, double clipperScale = 10000000)
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

                    var childNfp = ClipperHelper.ScaleUpPaths(nfp.Childrens[j], clipperScale);
                    clipperNfp.Add(childNfp);
                }
            }

            if (GeometryUtil.polygonArea(nfp) > 0)
            {
                nfp.reverse();
            }
            var outerNfp = ClipperHelper.ScaleUpPaths(nfp, clipperScale);

            clipperNfp.Add(outerNfp);
            return clipperNfp.ToArray();
        }
        private static IntPoint[][] innerNfpToClipperCoordinates(NFP[] nfp, double clipperScale = 10000000)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                var clip = nfpToClipperCoordinates(nfp[i], clipperScale);
                clipperNfp.AddRange(clip);
            }

            return clipperNfp.ToArray();
        }
        public static NFP[] getInnerNfp(NFP A, NFP B, double clipperScale = 10000000)
        {

            var frame = getFrame(A);

            var nfp = getOuterNfp(frame, B, true);

            if (nfp == null || nfp.Childrens == null || nfp.Childrens.Count == 0)
            {
                return null;
            }
            List<NFP> holes = new List<NFP>();
            if (A.Childrens != null && A.Childrens.Count > 0)
            {
                for (var i = 0; i < A.Childrens.Count; i++)
                {
                    var hnfp = getOuterNfp(A.Childrens[i], B);
                    if (hnfp != null)
                    {
                        holes.Add(hnfp);
                    }
                }
            }

            if (holes.Count == 0)
            {
                return nfp.Childrens.ToArray();
            }
            var clipperNfp = innerNfpToClipperCoordinates(nfp.Childrens.ToArray(), clipperScale);
            var clipperHoles = innerNfpToClipperCoordinates(holes.ToArray(), clipperScale);

            List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
            var clipper = new Clipper();

            clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
            clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

            if (!clipper.Execute(ClipType.ctDifference, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
            {
                return nfp.Childrens.ToArray();
            }

            if (finalNfp.Count == 0)
            {
                return null;
            }

            List<NFP> f = new List<NFP>();
            for (var i = 0; i < finalNfp.Count; i++)
            {
                f.Add(toNestCoordinates(finalNfp[i].ToArray(), clipperScale));
            }



            return f.ToArray();

        }

        // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
        // note that this returns an array of polygons
        public static NFP[] polygonOffsetDeepNest(NFP polygon, double offset, double clipperScale, double curveTolerance = 0.72)
        {

            if (offset == 0 || GeometryUtil._almostEqual(offset, 0))
            {
                return new[] { polygon };
            }

            var p = svgToClipper(polygon, clipperScale).ToList();

            var miterLimit = 4;
            var co = new ClipperLib.ClipperOffset(miterLimit, curveTolerance * clipperScale);
            co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

            var newpaths = new List<List<ClipperLib.IntPoint>>();
            co.Execute(ref newpaths, offset * clipperScale);


            var result = new List<NFP>();
            for (var i = 0; i < newpaths.Count; i++)
            {
                result.Add(clipperToSvg(newpaths[i], clipperScale));
            }

            return result.ToArray();
        }
        public static NFP clipperToSvg(IList<IntPoint> polygon, double clipperScale)
        {
            List<SvgPoint> ret = new List<SvgPoint>();

            for (var i = 0; i < polygon.Count; i++)
            {
                ret.Add(new SvgPoint(polygon[i].X / clipperScale, polygon[i].Y / clipperScale));
            }

            return new NFP() { Points = ret.ToArray() };
        }
        // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
        private static IntPoint[] svgToClipper(NFP polygon, double clipperScale)
        {
            var d = ClipperHelper.ScaleUpPaths(polygon, clipperScale);
            return d.ToArray();
        }
        private static NFP toNestCoordinates(IntPoint[] polygon, double scale)
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
        public static NFP clone2(NFP nfp)
        {
            NFP newnfp = new NFP();

            for (var i = 0; i < nfp.Length; i++)
            {
                newnfp.AddPoint(new SvgPoint(nfp[i].X, nfp[i].Y));
            }

            if (nfp.Childrens != null && nfp.Childrens.Count > 0)
            {
                newnfp.Childrens = new List<NFP>();
                for (int i = 0; i < nfp.Childrens.Count; i++)
                {
                    var child = nfp.Childrens[i];
                    NFP newchild = new NFP();
                    for (var j = 0; j < child.Length; j++)
                    {
                        newchild.AddPoint(new SvgPoint(child[j].X, child[j].Y));
                    }
                    newnfp.Childrens.Add(newchild);
                }
            }

            return newnfp;
        }
        public static NFP clone(NFP p)
        {
            var newp = new NFP();
            for (var i = 0; i < p.Length; i++)
            {
                newp.AddPoint(new SvgPoint(

                     p[i].X,
                     p[i].Y

                ));
            }

            return newp;
        }
        public static NFP simplifyFunction(NFP polygon, bool inside, double clipperScale, double curveTolerance = 0.72, bool hullSimplify = false)
        {
            var tolerance = 4 * curveTolerance;

            // give special treatment to line segments above this length (squared)
            var fixedTolerance = 40 * curveTolerance * 40 * curveTolerance;
            int i, j, k;


            if (hullSimplify)
            {
                // use convex hull			
                var hull = getHull(polygon);
                if (hull != null)
                {
                    return hull;
                }
                else
                {
                    return polygon;
                }
            }

            var cleaned = cleanPolygon2(polygon, clipperScale);
            if (cleaned != null && cleaned.Length > 1)
            {
                polygon = cleaned;
            }
            else
            {
                return polygon;
            }
            // polygon to polyline
            var copy = polygon.slice(0);
            copy.push(copy[0]);
            // mark all segments greater than ~0.25 in to be kept
            // the PD simplification algo doesn't care about the accuracy of long lines, only the absolute distance of each point
            // we care a great deal
            for (i = 0; i < copy.Length - 1; i++)
            {
                var p1 = copy[i];
                var p2 = copy[i + 1];
                var sqd = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
                if (sqd > fixedTolerance)
                {
                    p1.marked = true;
                    p2.marked = true;
                }
            }

            var simple = Simplify.simplify(copy, tolerance, true);
            // now a polygon again
            //simple.pop();
            simple.Points = simple.Points.Take(simple.Points.Count() - 1).ToArray();

            // could be dirty again (self intersections and/or coincident points)
            simple = cleanPolygon2(simple, clipperScale);

            // simplification process reduced poly to a line or point
            if (simple == null)
            {
                simple = polygon;
            }



            var offsets = polygonOffsetDeepNest(simple, inside ? -tolerance : tolerance, clipperScale);

            NFP offset = null;
            double offsetArea = 0;
            List<NFP> holes = new List<NFP>();
            for (i = 0; i < offsets.Length; i++)
            {
                var area = GeometryUtil.polygonArea(offsets[i]);
                if (offset == null || area < offsetArea)
                {
                    offset = offsets[i];
                    offsetArea = area;
                }
                if (area > 0)
                {
                    holes.Add(offsets[i]);
                }
            }

            // mark any points that are exact
            for (i = 0; i < simple.Length; i++)
            {
                var seg = new NFP();
                seg.AddPoint(simple[i]);
                seg.AddPoint(simple[i + 1 == simple.Length ? 0 : i + 1]);

                var index1 = find(seg[0], polygon);
                var index2 = find(seg[1], polygon);

                if (index1 + 1 == index2 || index2 + 1 == index1 || (index1 == 0 && index2 == polygon.Length - 1) || (index2 == 0 && index1 == polygon.Length - 1))
                {
                    seg[0].exact = true;
                    seg[1].exact = true;
                }
            }
            var numshells = 4;
            NFP[] shells = new NFP[numshells];

            for (j = 1; j < numshells; j++)
            {
                var delta = j * (tolerance / numshells);
                delta = inside ? -delta : delta;
                var shell = polygonOffsetDeepNest(simple, delta, clipperScale);
                if (shell.Count() > 0)
                {
                    shells[j] = shell.First();
                }
                else
                {
                    //shells[j] = shell;
                }
            }

            if (offset == null)
            {
                return polygon;
            }
            // selective reversal of offset
            for (i = 0; i < offset.Length; i++)
            {
                var o = offset[i];
                var target = getTarget(o, simple, 2 * tolerance);

                // reverse point offset and try to find exterior points
                var test = clone(offset);
                test.Points[i] = new SvgPoint(target.X, target.Y);

                if (!exterior(test, polygon, inside))
                {
                    o.X = target.X;
                    o.Y = target.Y;
                }
                else
                {
                    // a shell is an intermediate offset between simple and offset
                    for (j = 1; j < numshells; j++)
                    {
                        if (shells[j] != null)
                        {
                            var shell = shells[j];
                            var delta = j * (tolerance / numshells);
                            target = getTarget(o, shell, 2 * delta);
                            test = clone(offset);
                            test.Points[i] = new SvgPoint(target.X, target.Y);
                            if (!exterior(test, polygon, inside))
                            {
                                o.X = target.X;
                                o.Y = target.Y;
                                break;
                            }
                        }
                    }
                }
            }

            // straighten long lines
            // a rounded rectangle would still have issues at this point, as the long sides won't line up straight

            var straightened = false;

            for (i = 0; i < offset.Length; i++)
            {
                var p1 = offset[i];
                var p2 = offset[i + 1 == offset.Length ? 0 : i + 1];

                var sqd = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);

                if (sqd < fixedTolerance)
                {
                    continue;
                }
                for (j = 0; j < simple.Length; j++)
                {
                    var s1 = simple[j];
                    var s2 = simple[j + 1 == simple.Length ? 0 : j + 1];

                    var sqds = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);

                    if (sqds < fixedTolerance)
                    {
                        continue;
                    }

                    if ((GeometryUtil._almostEqual(s1.X, s2.X) || GeometryUtil._almostEqual(s1.Y, s2.Y)) && // we only really care about vertical and horizontal lines
                    GeometryUtil._withinDistance(p1, s1, 2 * tolerance) &&
                    GeometryUtil._withinDistance(p2, s2, 2 * tolerance) &&
                    (!GeometryUtil._withinDistance(p1, s1, curveTolerance / 1000) ||
                    !GeometryUtil._withinDistance(p2, s2, curveTolerance / 1000)))
                    {
                        p1.X = s1.X;
                        p1.Y = s1.Y;
                        p2.X = s2.X;
                        p2.Y = s2.Y;
                        straightened = true;
                    }
                }
            }

            //if(straightened){

            var Ac = ClipperHelper.ScaleUpPaths(offset, 10000000);
            var Bc = ClipperHelper.ScaleUpPaths(polygon, 10000000);

            var combined = new List<List<IntPoint>>();
            var clipper = new ClipperLib.Clipper();

            clipper.AddPath(Ac.ToList(), ClipperLib.PolyType.ptSubject, true);
            clipper.AddPath(Bc.ToList(), ClipperLib.PolyType.ptSubject, true);

            // the line straightening may have made the offset smaller than the simplified
            if (clipper.Execute(ClipperLib.ClipType.ctUnion, combined, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
            {
                double? largestArea = null;
                for (i = 0; i < combined.Count; i++)
                {
                    var n = toNestCoordinates(combined[i].ToArray(), 10000000);
                    var sarea = -GeometryUtil.polygonArea(n);
                    if (largestArea == null || largestArea < sarea)
                    {
                        offset = n;
                        largestArea = sarea;
                    }
                }
            }
            //}

            cleaned = cleanPolygon2(offset, clipperScale);
            if (cleaned != null && cleaned.Length > 1)
            {
                offset = cleaned;
            }

            // mark any points that are exact (for line merge detection)
            for (i = 0; i < offset.Length; i++)
            {
                var seg = new SvgPoint[] { offset[i], offset[i + 1 == offset.Length ? 0 : i + 1] };
                var index1 = find(seg[0], polygon);
                var index2 = find(seg[1], polygon);
                if (index1 == null)
                {
                    index1 = 0;
                }
                if (index2 == null)
                {
                    index2 = 0;
                }
                if (index1 + 1 == index2 || index2 + 1 == index1
                    || (index1 == 0 && index2 == polygon.Length - 1) ||
                    (index2 == 0 && index1 == polygon.Length - 1))
                {
                    seg[0].exact = true;
                    seg[1].exact = true;
                }
            }

            if (!inside && holes != null && holes.Count > 0)
            {
                offset.Childrens = holes;
            }

            return offset;

        }
        public class HullInfoPoint
        {
            public double x;
            public double y;
            public int index;
        }
        public static double cross(double[] a, double[] b, double[] c)
        {
            return (b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0]);
        }
        public static NFP getHull(NFP polygon)
        {

            double[][] points = new double[polygon.Length][];
            for (var i = 0; i < polygon.Length; i++)
            {
                points[i] = (new double[] { polygon[i].X, polygon[i].Y });
            }

            var hullpoints = polygonHull(points);

            if (hullpoints == null)
            {
                return polygon;
            }

            NFP hull = new NFP();
            for (int i = 0; i < hullpoints.Count(); i++)
            {
                hull.AddPoint(new SvgPoint(hullpoints[i][0], hullpoints[i][1]));
            }
            return hull;
        }

        public static int[] computeUpperHullIndexes(double[][] points)
        {
            Dictionary<int, int> indexes = new Dictionary<int, int>();
            indexes.Add(0, 0);
            indexes.Add(1, 1);
            var n = points.Count();
            var size = 2;

            for (var i = 2; i < n; ++i)
            {
                while (size > 1 && cross(points[indexes[size - 2]], points[indexes[size - 1]], points[i]) <= 0) --size;

                if (!indexes.ContainsKey(size))
                {
                    indexes.Add(size, -1);
                }
                indexes[size++] = i;
            }
            List<int> ret = new List<int>();
            for (int i = 0; i < size; i++)
            {
                ret.Add(indexes[i]);
            }
            return ret.ToArray();
            //return indexes.slice(0, size); // remove popped points
        }
        public static double[][] polygonHull(double[][] points)
        {
            int n;
            n = points.Count();
            if ((n) < 3) return null;



            HullInfoPoint[] sortedPoints = new HullInfoPoint[n];
            double[][] flippedPoints = new double[n][];



            for (int i = 0; i < n; ++i) sortedPoints[i] = new HullInfoPoint { x = points[i][0], y = points[i][1], index = i };
            sortedPoints = sortedPoints.OrderBy(x => x.x).ThenBy(z => z.y).ToArray();

            for (int i = 0; i < n; ++i) flippedPoints[i] = new double[] { sortedPoints[i].x, -sortedPoints[i].y };

            var upperIndexes = computeUpperHullIndexes(sortedPoints.Select(z => new double[] { z.x, z.y, z.index }).ToArray());
            var lowerIndexes = computeUpperHullIndexes(flippedPoints);


            // Construct the hull polygon, removing possible duplicate endpoints.
            var skipLeft = lowerIndexes[0] == upperIndexes[0];
            var skipRight = lowerIndexes[lowerIndexes.Length - 1] == upperIndexes[upperIndexes.Length - 1];
            List<double[]> hull = new List<double[]>();

            // Add upper hull in right-to-l order.
            // Then add lower hull in left-to-right order.
            for (int i = upperIndexes.Length - 1; i >= 0; --i)
                hull.Add(points[sortedPoints[upperIndexes[i]].index]);
            //for (int i = +skipLeft; i < lowerIndexes.Length - skipRight; ++i) hull.push(points[sortedPoints[lowerIndexes[i]][2]]);
            for (int i = skipLeft ? 1 : 0; i < lowerIndexes.Length - (skipRight ? 1 : 0); ++i) hull.Add(points[sortedPoints[lowerIndexes[i]].index]);

            return hull.ToArray();
        }

    }
}