using GeomPad.Dialogs;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "polyline")]
    public class PolylineHelper : HelperItem, ICommandsContainer
    {
        public override AbstractHelperItem Clone()
        {
            PolylineHelper pl = new PolylineHelper();
            pl.Name = "clone_" + Name;
            foreach (var item in Points)
            {
                pl.Points.Add(item);
            }
            pl.DrawPoints = DrawPoints;
            return pl;
        }
        public int PointsCount => Points.Count;
        public bool DrawPoints { get; set; }
        public ICommand[] Commands => new ICommand[] { new ApproxMajorLine(), new ApproxMajorLine(false), new ConvertToPolygon() };

        public class ConvertToPolygon : ICommand
        {
            public ConvertToPolygon()
            {

            }

            public string Name => "convert to polygon";

            public Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (e, e2, e3) =>
            {
                PolygonHelper mh = new PolygonHelper();
                var ee = e as PolylineHelper;
                mh.Polygon = new NFP() { };
                //check first and last points are equal?
                mh.Polygon.Points = ee.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                e3.AddHelper(mh);
            };
        }

        public class ApproxMajorLine : ICommand
        {
            public ApproxMajorLine(bool single = true)
            {
                _single = single;
            }
            bool _single;
            public string Name => "approx by major line (" + (_single ? "single" : "many") + ")";

            public System.Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (e, e2, e3) =>
            {
                var ee = e as PolylineHelper;


                DoubleInputDialog did = new DoubleInputDialog();
                double koef = 15;
                did.Init(koef);
                did.Caption = "Approx height koef";
                if (did.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                koef = did.Value;

                while (true)
                {
                    int ti = 0;
                    int te = 0;
                    double maxApproxDist = 0;
                    double bestMeanDist = double.MaxValue;
                    bool was = false;
                    Line2D approxBest = null;
                    for (int i = 0; i < ee.Points.Count; i++)
                    {
                        for (int j = i + 1; j < ee.Points.Count; j++)
                        {
                            var p1 = ee.Points[i];
                            var j2 = j % ee.Points.Count;
                            if (j2 == i) continue;
                            var p2 = ee.Points[j2];
                            //get approx line, calc mean line
                            var approx = new Line2D() { Start = p1, End = p2 };
                            double meanDist = 0;
                            double maxDist = 0;
                            int cntr = 0;
                            for (int i1 = i; i1 <= j; i1++)
                            {
                                cntr++;
                                var pz = ee.Points[i1 % ee.Points.Count];
                                var proj = approx.GetProj(pz);
                                var d1 = (proj - pz).Length;
                                meanDist += d1;
                                maxDist = Math.Max(maxDist, d1);
                            }
                            meanDist /= cntr;

                            if ((j - i) > 10 && meanDist > 0.5f && meanDist < koef && maxDist < koef && (int)(approx.Len / 50) >= (int)(maxApproxDist / 50) && meanDist < bestMeanDist)
                            {
                                bestMeanDist = meanDist;
                                approxBest = approx;
                                maxApproxDist = approx.Len;
                                ti = i;
                                te = j;
                                was = true;
                            }
                        }
                    }

                    if (was)
                    {

                        double meanDist = 0;
                        double maxDist = 0;
                        int cntr = 0;
                        for (int i1 = ti; i1 <= te; i1++)
                        {
                            cntr++;
                            var pz = ee.Points[i1 % ee.Points.Count];
                            var proj = approxBest.GetProj(pz);
                            var d1 = (proj - pz).Length;
                            meanDist += d1;
                            maxDist = Math.Max(maxDist, d1);
                        }
                        meanDist /= cntr;

                        List<Vector2d> newp = new List<Vector2d>(0);

                        for (int i = 0; i < ee.Points.Count; i++)
                        {
                            if (i > ti && i < te)
                            {
                                //var prj = approxBest.GetProj(ee.Points[i]);
                                //    newp.Add(prj);
                                continue;
                            }
                            newp.Add(ee.Points[i]);
                        }
                        ee.Points = newp;
                    }
                    else
                    {
                        e3.SetStatus(DateTime.Now.ToLongTimeString() + ": not found", StatusMessageType.Warning);
                        break;
                    }
                    if (_single) break;
                }
            };
        }
        public override RectangleF? BoundingBox()
        {
            var minx = (float)Points.Min(z => z.X);
            var maxx = (float)Points.Max(z => z.X);
            var miny = (float)Points.Min(z => z.Y);
            var maxy = (float)Points.Max(z => z.Y);

            return new RectangleF(minx, miny, maxx - minx, maxy - miny);

        }
        public List<Vector2d> Points = new List<Vector2d>();
        public Pen Color { get; set; } = Pens.Black;
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<polyline name=\"{Name}\" visible=\"{Visible}\" >");
            foreach (var item in Points)
            {
                sb.AppendLine($"<point x=\"{item.X}\" y=\"{item.Y}\"/>");
            }
            sb.AppendLine($"</polyline>");
        }

        public override void ParseXml(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            foreach (var pitem in item.Elements("point"))
            {
                var xx = StaticHelpers.ParseDouble(pitem.Attribute("x").Value);
                var yy = StaticHelpers.ParseDouble(pitem.Attribute("y").Value);
                Points.Add(new Vector2d(xx, yy));
            }
        }

        public override void Draw(IDrawingContext idc)
        {
            var dc = idc as DrawingContext;
            if (!Visible) return;

            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            Pen pen = Color;
            if (Selected)
            {
                br = Brushes.Red;
                pen = Pens.Red;
            }
            for (int i = 1; i < Points.Count; i++)
            {
                var tr1 = dc.Transform(Points[i - 1]);
                var tr2 = dc.Transform(Points[i]);
                dc.gr.DrawLine(pen, tr1, tr2);
            }
            if (DrawPoints)
            {
                foreach (var item in Points)
                {
                    var tr1 = dc.Transform(item);
                    dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);

                }
            }

        }
    }
}
