using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    public class PolygonHelper : HelperItem
    {
        public NFP Polygon = new NFP();

        public bool Fill { get; set; } = false;

        public Color FillColor
        {
            get
            {
                return (FillBrush as SolidBrush).Color;
            }
            set
            {
                FillBrush = new SolidBrush(value);
            }
        }
        public Brush FillBrush = SystemBrushes.Highlight;

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Rotation { get; set; }

        PointF transform(DrawingContext dc, SvgPoint p)
        {
            Matrix mtr = new Matrix();
            mtr.RotateAt((float)Rotation, new PointF(0, 0));
            PointF[] pnt = new PointF[] { new PointF((float)p.X, (float)p.Y) };
            mtr.TransformPoints(pnt);
            p = new SvgPoint(pnt[0].X, pnt[0].Y);
            return dc.Transform(new SvgPoint(p.X + OffsetX, p.Y + OffsetY));
        }

        public override void Draw(DrawingContext dc)
        {
            if (!Visible) return;

            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            Pen pen = Pens.Black;
            if (Selected)
            {
                br = Brushes.Red;
                pen = Pens.Red;
            }

            GraphicsPath gp = new GraphicsPath();
            if (Fill && Polygon.Points.Length >= 3)
            {
                gp.AddPolygon(Polygon.Points.Select(z => transform(dc, z)).ToArray());

                foreach (var item in Polygon.Childrens)
                {
                    gp.AddPolygon(item.Points.Select(z => transform(dc, z)).ToArray());
                }

                dc.gr.FillPath(FillBrush, gp);

            }
            foreach (var item in Polygon.Points)
            {
                var tr1 = transform(dc, item);
                dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
            }

            for (int i = 0; i < Polygon.Points.Length; i++)
            {
                var j = (i + 1) % Polygon.Points.Length;
                var tr1 = transform(dc, Polygon.Points[i]);
                var tr2 = transform(dc, Polygon.Points[j]);
                dc.gr.DrawLine(pen, tr1, tr2);
            }
            foreach (var ch in Polygon.Childrens)
            {
                foreach (var item in ch.Points)
                {
                    var tr1 = transform(dc, item);
                    dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                }
                for (int i = 0; i < ch.Points.Length; i++)
                {
                    var j = (i + 1) % ch.Points.Length;
                    var tr1 = transform(dc, ch.Points[i]);
                    var tr2 = transform(dc, ch.Points[j]);
                    dc.gr.DrawLine(pen, tr1, tr2);
                }
            }
        }

        void appendPolygon(StringBuilder sb, NFP nfp)
        {
            sb.AppendLine("<polygon>");
            foreach (var item in nfp.Points)
            {
                sb.AppendLine($"<point x=\"{item.X}\" y=\"{item.Y}\"/>");
            }
            sb.AppendLine("<childs>");
            foreach (var item in nfp.Childrens)
            {
                appendPolygon(sb, item);
            }
            sb.AppendLine("</childs>");
            sb.AppendLine("</polygon>");
        }
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<polygonHelper offsetX=\"{OffsetX}\" offsetY=\"{OffsetY}\" rotation=\"{Rotation}\">");
            appendPolygon(sb, Polygon);
            sb.AppendLine("</polygonHelper>");
        }

        NFP parsePolygon(XElement item)
        {
            NFP ret = new NFP();
            List<SvgPoint> points = new List<SvgPoint>();

            foreach (var point in item.Elements("point"))
            {
                var xx = double.Parse(point.Attribute("x").Value);
                var yy = double.Parse(point.Attribute("y").Value);
                points.Add(new SvgPoint(xx, yy));
                ret.Points = points.ToArray();

            }
            var childs = item.Element("childs");
            foreach (var pitem in childs.Elements("polygon"))
            {
                ret.Childrens.Add(parsePolygon(pitem));
            }
            return ret;
        }

        internal void ParseXml(XElement item)
        {
            OffsetX = double.Parse(item.Attribute("offsetX").Value);
            OffsetY = double.Parse(item.Attribute("offsetY").Value);
            Rotation = double.Parse(item.Attribute("rotation").Value);
            var fr = item.Elements().First();
            Polygon = parsePolygon(fr);
        }

        public SvgPoint[] TransformedPoints()
        {
            List<SvgPoint> ret = new List<SvgPoint>();
            foreach (var item in Polygon.Points)
            {
                ret.Add(transform(item));
            }
            return ret.ToArray();
        }

        SvgPoint transform(SvgPoint p)
        {
            Matrix mtr = new Matrix();
            mtr.RotateAt((float)Rotation, new PointF(0, 0));

            var pnt = new[] { p }.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
            mtr.TransformPoints(pnt);
            return new SvgPoint(pnt[0].X + OffsetX, pnt[0].Y + OffsetY);
        }

        public override RectangleF? BoundingBox()
        {
            Matrix mtr = new Matrix();
            mtr.RotateAt((float)Rotation, new PointF(0, 0));

            var pnt = Polygon.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
            mtr.TransformPoints(pnt);
            var maxx = pnt.Max(z => z.X);
            var maxy = pnt.Max(z => z.Y);
            var minx = pnt.Min(z => z.X);
            var miny = pnt.Min(z => z.Y);

            return new RectangleF((float)(minx + OffsetX), (float)(miny + OffsetY), maxx - minx, maxy - miny);
        }
    }
}