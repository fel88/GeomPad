using GeomPad.Common;
using OpenTK;
using System.Drawing;
using System.Linq;

namespace GeomPad.Helpers
{
    public class OrthogonalDist2Segments : HelperItem
    {

        public SegmentHelper Segment1;
        public SegmentHelper Segment2;
        public SegmentHelper DistSegment;


        public bool Horizontal { get; set; } = true;
        public double RequiredOffset { get; set; }

        public override void Draw(IDrawingContext gr)
        {
            if (!Visible) return;

            if (Segment1 == null) return;
            if (Segment2 == null) return;
            if (DistSegment == null) return;

            DistSegment.DrawArrowCap = true;
            var pen = new Pen(Color.Red, 3);
            pen.DashPattern = new float[] { 3, 3 };
            Segment1.Color = pen.Color;
            Segment2.Color = pen.Color;
            Segment2.Thickness = pen.Width;
            Segment1.Thickness = pen.Width;
            Segment1.Draw(gr);
            Segment2.Draw(gr);
            DistSegment.Color = Color.Green;
            DistSegment.Thickness = 3;
            DistSegment.Draw(gr);
        }

        public static SegmentHelper SegmentsXDist(SegmentHelper s0, SegmentHelper s1)
        {
            var hline1 = new SegmentHelper() { Point = s1.Point - new Vector2d(-1000, 0), Point2 = s1.Point + new Vector2d(1000, 0) };
            var hline2 = new SegmentHelper() { Point = s1.Point2 - new Vector2d(-1000, 0), Point2 = s1.Point2 + new Vector2d(1000, 0) };
            var hline3 = new SegmentHelper() { Point = s0.Point - new Vector2d(-1000, 0), Point2 = s0.Point + new Vector2d(1000, 0) };
            var hline4 = new SegmentHelper() { Point = s0.Point2 - new Vector2d(-1000, 0), Point2 = s0.Point2 + new Vector2d(1000, 0) };
            var inter1 = IntersectLineWithHLine(s0, hline1);
            var inter2 = IntersectLineWithHLine(s0, hline2);
            var inter3 = IntersectLineWithHLine(s1, hline3);
            var inter4 = IntersectLineWithHLine(s1, hline4);

            SegmentHelper distSegm = null;
            double minDist = float.MaxValue;
            var pnts = new[] { s1.Point, s1.Point2, s0.Point, s0.Point2 };
            var alls = new[] { inter1, inter2, inter3, inter4 };
            for (int i = 0; i < alls.Length; i++)
            {
                if (alls[i] != null && alls[i].Any())
                {
                    //Items.Add(new PointHelper() { Point = alls[i][0].Point });
                    var dist = (pnts[i] - alls[i][0].Point).Length;
                    if (dist < minDist)
                    {
                        distSegm = new SegmentHelper();
                        distSegm.Point = pnts[i];
                        distSegm.Point2 = alls[i][0].Point;
                        if (i > 1)
                        {
                            distSegm.Point2 = pnts[i];
                            distSegm.Point = alls[i][0].Point;
                        }
                        minDist = dist;
                    }
                }
            }


            return distSegm;
        }

        public static SegmentHelper SegmentsYDist(SegmentHelper s0, SegmentHelper s1)
        {
            var vline1 = new SegmentHelper() { Point = s1.Point - new Vector2d(0, -1000), Point2 = s1.Point + new Vector2d(0, 1000) };
            var vline2 = new SegmentHelper() { Point = s1.Point2 - new Vector2d(0, -1000), Point2 = s1.Point2 + new Vector2d(0, 1000) };
            var vline3 = new SegmentHelper() { Point = s0.Point - new Vector2d(0, -1000), Point2 = s0.Point + new Vector2d(0, 1000) };
            var vline4 = new SegmentHelper() { Point = s0.Point2 - new Vector2d(0, -1000), Point2 = s0.Point2 + new Vector2d(0, 1000) };
            var inter1 = IntersectLineWithVLine(s0, vline1);
            var inter2 = IntersectLineWithVLine(s0, vline2);
            var inter3 = IntersectLineWithVLine(s1, vline3);
            var inter4 = IntersectLineWithVLine(s1, vline4);

            SegmentHelper distSegm = null;
            double minDist = float.MaxValue;
            var pnts = new[] { s1.Point, s1.Point2, s0.Point, s0.Point2 };
            var alls = new[] { inter1, inter2, inter3, inter4 };
            for (int i = 0; i < alls.Length; i++)
            {
                if (alls[i] != null && alls[i].Any())
                {
                    //Items.Add(new PointHelper() { Point = alls[i][0].Point });
                    var dist = (pnts[i] - alls[i][0].Point).Length;
                    if (dist < minDist)
                    {
                        distSegm = new SegmentHelper();
                        distSegm.Point = pnts[i];
                        distSegm.Point2 = alls[i][0].Point;
                        if (i > 1)
                        {
                            distSegm.Point2 = pnts[i];
                            distSegm.Point = alls[i][0].Point;
                        }
                        minDist = dist;
                    }
                }
            }
            return distSegm;
        }

        public class ElementPoint
        {
            public Vector2d Point;
            public double Ratio;

            public ElementPoint(Vector2d point, double ratio)
            {
                Point = point;
                Ratio = ratio;
            }

        }

        static public ElementPoint[] IntersectLineWithHLine(SegmentHelper line, SegmentHelper hline)
        {
            RectangleF bb = line.BoundingBox().Value;
            double y0 = hline.Point.Y;
            //if (y0 > bb.Bottom|| y0 < bb.Top) return new udElementPoint[0];
            //if (bb.Height < 0.001) return new udElementPoint[] { new udElementPoint(line.Start, 0), new udElementPoint(line.End, 1)};
            var ys = line.Point.Y;
            var ye = line.Point2.Y;
            var ratio = (y0 - ys) / (ye - ys);
            if (ratio < 0 || ratio > 1 || double.IsNaN(ratio)) return new ElementPoint[0];
            return new ElementPoint[] { new ElementPoint(line.PointOfElement(ratio), ratio) };
        }

        static public ElementPoint[] IntersectLineWithVLine(SegmentHelper line, SegmentHelper vline)
        {
            RectangleF bb = line.BoundingBox().Value;
            double x0 = vline.Point.X;
            //if (y0 > bb.Bottom|| y0 < bb.Top) return new udElementPoint[0];
            //if (bb.Height < 0.001) return new udElementPoint[] { new udElementPoint(line.Start, 0), new udElementPoint(line.End, 1)};
            var xs = line.Point.X;
            var xe = line.Point2.X;
            var ratio = (x0 - xs) / (xe - xs);
            if (ratio < 0 || ratio > 1 || double.IsNaN(ratio)) return new ElementPoint[0];
            return new ElementPoint[] { new ElementPoint(line.PointOfElement(ratio), ratio) };
        }
    }
}
