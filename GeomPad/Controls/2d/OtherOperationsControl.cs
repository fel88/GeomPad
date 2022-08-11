using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeomPad.Helpers;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using OpenTK;

namespace GeomPad.Controls._2d
{
    public partial class OtherOperationsControl : UserControl
    {
        public OtherOperationsControl()
        {
            InitializeComponent();
        }

        public void Init(Pad2DDataModel dm)
        {
            dataModel = dm;
        }
        Pad2DDataModel dataModel;

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(dataModel.SelectedItem is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(new NFP() { Points = ph2.TransformedPoints() });
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;
            ph.Name = "hull";
            dataModel.AddItem(ph);
        }
        Random rand = new Random();
        private void button5_Click(object sender, EventArgs e)
        {

            var poly2 = new TriangleNet.Geometry.Polygon();
            var plh = dataModel.SelectedItem as PolygonHelper;

            //foreach (var item in plh.Polygon.Points)
            {
                var pn = plh.TransformedPoints();
                if (StaticHelpers.signed_area(pn) < 0) { pn = pn.Reverse().ToArray(); }
                var a = pn.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();

                poly2.Add(new Contour(a));
            }

            var rev = plh.Polygon.Childrens.ToArray();
            rev = rev.Reverse().ToArray();
            foreach (var item in rev)
            {
                var pn = item.Points.Select(z => plh.Transform(z)).ToArray();
                var ar = StaticHelpers.signed_area(pn);
                /*if (StaticHelpers.signed_area(pn) > 0) 
                {
                    pn = pn.Reverse().ToArray(); 
                }*/
                var a = pn.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                var p0 = plh.Transform(item.Points[0]);
                PointF test = new PointF((float)p0.X, (float)p0.Y);
                var maxx = pn.Max(z => z.X);
                var minx = pn.Min(z => z.X);
                var maxy = pn.Max(z => z.Y);
                var miny = pn.Min(z => z.Y);

                var tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
                var ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
                while (true)
                {
                    if (StaticHelpers.pnpoly(pn, test.X, test.Y))
                    {
                        break;
                    }

                    tx = rand.Next((int)(minx * 100),
                       (int)(maxx * 100)) / 100f;
                    ty = rand.Next((int)(miny * 100),
                       (int)(maxy * 100)) / 100f;
                    test = new PointF(tx, ty);
                }

                poly2.Add(new Contour(a), new TriangleNet.Geometry.Point(test.X, test.Y));
                //poly2.Add(new Contour(a), true);
            }

            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions(), new QualityOptions());

            var tr = trng.Triangles.Select(z => new Vector2d[] {
                  new Vector2d(z.GetVertex(0).X, z.GetVertex(0).Y),
                  new Vector2d(z.GetVertex(1).X, z.GetVertex(1).Y),
                  new Vector2d(z.GetVertex(2).X, z.GetVertex(2).Y)
            }).ToArray();
            dataModel.AddItem(new MeshHelper(tr) { Name = "triangulate" });
        }

        private void button17_Click(object sender, EventArgs e)
        {
            List<SegmentHelper> ar1 = new List<SegmentHelper>();
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                if (dataModel.SelectedItems[i] is SegmentHelper ph1)
                {
                    ar1.Add(ph1);
                }
            }
            if (ar1.Count != 2) { dataModel.ParentForm.StatusMessage("there are no 2 line segments selected", StatusMessageType.Warning); return; }

            Vector2d ret = Vector2d.Zero;
            if (!GeometryUtils.IntersectSegments(ar1[0].Point, ar1[0].Point2, ar1[1].Point, ar1[1].Point2, ref ret))
            {
                dataModel.ParentForm.StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }

            PointHelper ph = new PointHelper();
            ph.Point = ret;
            dataModel.AddItem(ph);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!(dataModel.SelectedItem is PolygonHelper ph2)) { return; }

            double clipperScale = 10000000;
            var hull = DeepNest.simplifyFunction(new NFP() { Points = ph2.TransformedPoints() }, false, clipperScale);
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;
            ph.Name = "simplify";

            dataModel.AddItem(ph);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!(dataModel.SelectedItem is PolygonHelper ph2)) { return; }
            var c = ph2.CenterOfMass();
            ph2.Translate(-c);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (!(dataModel.SelectedItem is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(new NFP() { Points = ph2.TransformedPoints().ToArray() });
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;

            var mar = GeometryUtil.GetMinimumBox(hull.Points.Select(z => new Vector2d(z.X, z.Y)).ToArray());
            PolygonHelper ph3 = new PolygonHelper();

            ph3.Polygon = new NFP()
            {
                Points = mar.Select(z => new SvgPoint(z.X, z.Y)).ToArray()
            };

            ph3.Name = "minRect";
            dataModel.AddItem(ph3);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
            if (!(dataModel.SelectedItem is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(ph2.TransformedNfp());
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;            
            
            var box = ph.BoundingBox().Value;
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
            dataModel.AddItem(ph3);
        }
        public void calcOrthogonalFor2Polygons(bool xAxis = true)
        {
            var hh = dataModel.SelectedItems.OfType<PolygonHelper>().ToArray();
            var p1 = hh[0] as PolygonHelper;
            var p2 = hh[1] as PolygonHelper;

            List<SegmentHelper> s1 = new List<SegmentHelper>();
            List<SegmentHelper> s2 = new List<SegmentHelper>();

            //skip all segments outside boundingBox

            var tr1 = p1.GetTrasformed(p1.Polygon);
            var tr2 = p2.GetTrasformed(p2.Polygon);
            foreach (var trr1 in tr1)
            {
                for (int i = 1; i < trr1.Points.Length; i++)
                {
                    var p00 = trr1.Points[i - 1];
                    var p11 = trr1.Points[i];
                    s1.Add(new SegmentHelper() { Point = new Vector2d(p00.X, p00.Y), Point2 = new Vector2d(p11.X, p11.Y) });
                }
            }

            foreach (var trr2 in tr2)
            {
                for (int i = 1; i < trr2.Points.Length; i++)
                {
                    var p00 = trr2.Points[i - 1];
                    var p11 = trr2.Points[i];
                    s2.Add(new SegmentHelper() { Point = new Vector2d(p00.X, p00.Y), Point2 = new Vector2d(p11.X, p11.Y) });
                }
            }

            double mindist = double.MaxValue;
            SegmentHelper mins1 = null;
            SegmentHelper minss1 = null;
            SegmentHelper minss2 = null;
            foreach (var seg1 in s1)
            {
                foreach (var seg2 in s2)
                {

                    SegmentHelper dist = null;
                    if (xAxis)
                        dist = OrthogonalDist2Segments.SegmentsXDist(seg1, seg2);
                    else
                        dist = OrthogonalDist2Segments.SegmentsYDist(seg1, seg2);

                    if (dist != null)
                    {
                        if (mindist > dist.Length)
                        {
                            minss1 = seg1;
                            minss2 = seg2;
                            mindist = dist.Length;
                            mins1 = dist;
                        }
                    }
                }
            }
            if (mins1 != null)
            {
                mins1.Reverse();
                double reqDist = (mins1.Point.X > mins1.Point2.X ? -1 : 1) * mins1.Length;
                if (!xAxis)
                {
                    reqDist = (mins1.Point.Y > mins1.Point2.Y ? -1 : 1) * mins1.Length;
                }
                dataModel.AddItem(new OrthogonalDist2Segments()
                {
                    Name = $"{p1.Name} --> {p2.Name}",
                    Segment1 = minss1.Clone(),
                    Segment2 = minss2.Clone(),
                    DistSegment = mins1
                ,
                    RequiredOffset = reqDist
                });
                //p1.OffsetX += (mins1.Point.X > mins1.Point2.X ? 1 : -1) * mins1.Length;


            }
            else
            {

                dataModel.ParentForm.StatusMessage("no orthogonal intersection", StatusMessageType.Warning);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            calcOrthogonalFor2Polygons();

        }

        private void button18_Click(object sender, EventArgs e)
        {
            calcOrthogonalFor2Polygons(false);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //largest interior rectangle
        }
    }
}
