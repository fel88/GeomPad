using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "linesSet")]
    public class LinesSetHelper : HelperItem
    {
        public List<Line2D> Lines = new List<Line2D>();


        public bool DrawPoints { get; set; }
        public bool DrawArrows { get; set; }

        public override RectangleF? BoundingBox()
        {
            var maxx = (float)Lines.Max(z => Math.Max(z.Start.X, z.End.X));
            var maxy = (float)Lines.Max(z => Math.Max(z.Start.Y, z.End.Y));
            var minx = (float)Lines.Min(z => Math.Min(z.Start.X, z.End.X));
            var miny = (float)Lines.Min(z => Math.Min(z.Start.Y, z.End.Y));

            return new RectangleF(minx, miny, maxx - minx, maxy - miny);

        }

        public Pen Color { get; set; } = Pens.Black;

        public double ArrowLen { get; set; } = 3;
        public int ArrowAng { get; set; } = 35;

        public bool ArrowZoomRelative { get; set; } = false;

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
            var arrowLen = ArrowLen;
            if (!ArrowZoomRelative)
                arrowLen /= dc.zoom;

            foreach (var item in Lines)
            {
                var tr1 = dc.Transform(item.Start);
                var tr2 = dc.Transform(item.End);
                dc.gr.DrawLine(pen, tr1, tr2);
                if (DrawPoints)
                {
                    dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                    dc.gr.FillEllipse(br, tr2.X - r, tr2.Y - r, 2 * r, 2 * r);
                }

                if (DrawArrows)
                {
                    var dir = (item.End - item.Start).Normalized();

                    var p11 = -dir * arrowLen;
                    Matrix mtr = new Matrix();
                    mtr.RotateAt((float)ArrowAng, new PointF(0, 0));
                    PointF[] pnt = new PointF[] {
                    new PointF((float)p11.X, (float)p11.Y) };
                    mtr.TransformPoints(pnt);
                    p11 = new Vector2d(pnt[0].X, pnt[0].Y);
                    p11 += item.End;
                    var tp11 = dc.Transform(p11.ToPointF());
                    dc.gr.DrawLine(pen, tr2, tp11);

                    mtr = new Matrix();
                    p11 = -dir * arrowLen;
                    pnt = new PointF[] {
                    new PointF((float)p11.X, (float)p11.Y) };
                    mtr.RotateAt((float)-ArrowAng, new PointF(0, 0));
                    mtr.TransformPoints(pnt);
                    p11 = new Vector2d(pnt[0].X, pnt[0].Y);
                    p11 += item.End;
                    var tp22 = dc.Transform(p11.ToPointF());
                    dc.gr.DrawLine(pen, tr2, tp22);
                    dc.gr.FillPolygon(Brushes.Blue, new PointF[] { tr2, tp22, tp11 });
                }
            }
        }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<linesSet name=\"{Name}\" >");
            foreach (var item in Lines)
            {

            }
            sb.AppendLine($"</linesSet name=\"{Name}\" >");
        }

        public override void ParseXml(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            var x1 = double.Parse(item.Attribute("x1").Value);
            var x2 = double.Parse(item.Attribute("x2").Value);
            var y1 = double.Parse(item.Attribute("y1").Value);
            var y2 = double.Parse(item.Attribute("y2").Value);


        }

        internal LinesSetHelper Clone()
        {
            LinesSetHelper ret = new LinesSetHelper();

            return ret;
        }


    }
}
