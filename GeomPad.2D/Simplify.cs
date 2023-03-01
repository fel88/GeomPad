namespace GeomPad
{
    public class Simplify
    {

        // to suit your point format, run search/replace for '.x' and '.y';
        // for 3D version, see 3d branch (configurability would draw significant performance overhead)

        // square distance between 2 points
        public static double getSqDist(SvgPoint p1, SvgPoint p2)
        {

            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;

            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        public static double getSqSegDist(SvgPoint p, SvgPoint p1, SvgPoint p2)
        {

            var x = p1.X;
            var y = p1.Y;
            var dx = p2.X - x;
            var dy = p2.Y - y;

            if (dx != 0 || dy != 0)
            {

                var t = ((p.X - x) * dx + (p.Y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;

                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;

            return dx * dx + dy * dy;
        }
        // rest of the code doesn't care about point format

        // basic distance-based simplification
        public static NFP simplifyRadialDist(NFP points, double? sqTolerance)
        {

            var prevPoint = points[0];
            var newPoints = new NFP();
            newPoints.AddPoint(prevPoint);

            SvgPoint point = null;
            int i = 1;
            for (var len = points.Length; i < len; i++)
            {
                point = points[i];

                if (point.marked || getSqDist(point, prevPoint) > sqTolerance)
                {
                    newPoints.AddPoint(point);
                    prevPoint = point;
                }
            }

            if (prevPoint != point) newPoints.AddPoint(point);
            return newPoints;
        }


        public static void simplifyDPStep(NFP points, int first, int last, double? sqTolerance, NFP simplified)
        {
            var maxSqDist = sqTolerance;
            var index = -1;
            var marked = false;
            for (var i = first + 1; i < last; i++)
            {
                var sqDist = getSqSegDist(points[i], points[first], points[last]);

                if (sqDist > maxSqDist)
                {
                    index = i;
                    maxSqDist = sqDist;
                }
            }


            if (maxSqDist > sqTolerance || marked)
            {
                if (index - first > 1) simplifyDPStep(points, first, index, sqTolerance, simplified);
                simplified.push(points[index]);
                if (last - index > 1) simplifyDPStep(points, index, last, sqTolerance, simplified);
            }
        }

        // simplification using Ramer-Douglas-Peucker algorithm
        public static NFP simplifyDouglasPeucker(NFP points, double? sqTolerance)
        {
            var last = points.Length - 1;

            var simplified = new NFP();
            simplified.AddPoint(points[0]);
            simplifyDPStep(points, 0, last, sqTolerance, simplified);
            simplified.push(points[last]);

            return simplified;
        }

        // both algorithms combined for awesome performance
        public static NFP simplify(NFP points, double? tolerance, bool highestQuality)
        {

            if (points.Length <= 2) return points;

            var sqTolerance = (tolerance != null) ? (tolerance * tolerance) : 1;

            points = highestQuality ? points : simplifyRadialDist(points, sqTolerance);
            points = simplifyDouglasPeucker(points, sqTolerance);

            return points;
        }
    }
}