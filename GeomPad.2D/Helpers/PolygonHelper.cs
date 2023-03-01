using GeomPad.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "polygonHelper")]
    public class PolygonHelper : HelperItem, ICommandsContainer
    {
        public PolygonHelper() { }
        public NFP Polygon = new NFP();

        public override void Shift(Vector2d vector)
        {
            Polygon.Shift(vector);
            foreach (var item in Polygon.Childrens)
            {
                item.Shift(vector);
            }
        }

        public override AbstractHelperItem Clone()
        {
            PolygonHelper pl = new PolygonHelper();
            pl.Name = "clone_" + Name;
            pl.Polygon = Polygon.Clone();
            pl.DrawPoints = DrawPoints;
            pl.Fill = Fill;
            return pl;
        }

        bool _fill = false;
        public bool Fill { get => _fill; set { _fill = value; Changed?.Invoke(); } }
        public Vector2d CenterOfMass()
        {
            var b = BoundingBox().Value;
            return new Vector2d(b.X + OffsetX + b.Width / 2, b.Y + OffsetY + b.Height / 2);
        }

        public int PointsCount
        {
            get
            {
                return Polygon.Points.Length;
            }
        }



        public Color FillColor
        {
            get
            {
                return (FillBrush as SolidBrush).Color;
            }
            set
            {
                FillBrush = new SolidBrush(value);
                Changed?.Invoke();
            }
        }
        public Brush FillBrush = SystemBrushes.Highlight;

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        /// <summary>
        /// Rotation angle (deg)
        /// </summary>
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

        public NFP[] GetTrasformed(NFP polygon)
        {
            List<NFP> rets = new List<NFP>();
            Queue<NFP> q = new Queue<NFP>();
            List<NFP> alls = new List<NFP>();
            q.Enqueue(polygon);
            while (q.Any())
            {
                var deq = q.Dequeue();
                alls.Add(deq);
                foreach (var d in deq.Childrens)
                {
                    q.Enqueue(d);
                }
            }

            foreach (var item in alls)
            {
                NFP ret = new NFP();
                rets.Add(ret);
                foreach (var p in item.Points)
                {
                    Matrix mtr = new Matrix();
                    mtr.RotateAt((float)Rotation, new PointF(0, 0));
                    PointF[] pnt = new PointF[] { new PointF((float)p.X, (float)p.Y) };
                    mtr.TransformPoints(pnt);
                    var p2 = new SvgPoint(pnt[0].X, pnt[0].Y);
                    ret.AddPoint(new SvgPoint(p2.X + OffsetX, p2.Y + OffsetY));
                }
            }

            return rets.ToArray();
        }

        public bool DrawPoints { get; set; } = false;
        public bool Dashed { get; set; } = false;
        public float PenWidth { get; set; } = 1;
        public override void Draw(IDrawingContext idc)
        {
            var dc = idc as DrawingContext;
            if (!Visible) return;

            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            Pen pen = Pens.Black;
            if (Selected)
            {
                br = Brushes.Red;
                pen = Pens.Red;
            }

            if (Dashed)
            {
                pen = new Pen(pen.Color, PenWidth);
                pen.DashPattern = new float[] { 5, 5 };
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
            if (DrawPoints)
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
                if (DrawPoints)
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
            sb.AppendLine($"<polygonHelper name=\"{Name}\" offsetX=\"{OffsetX}\" offsetY=\"{OffsetY}\" rotation=\"{Rotation}\">");
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

        public override void ParseXml(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;
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
                ret.Add(Transform(item));
            }
            return ret.ToArray();
        }

        public SvgPoint Transform(SvgPoint p)
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

        public void Translate(Vector2d c)
        {
            Polygon.Translate(-c);
        }

        public NFP TransformedNfp()
        {
            return new NFP() { Points = TransformedPoints() };
        }
        public double Area { get; set; }

        public class AreaRecalcCommand : ICommand
        {
            public string Name => "recalc area";

            public Action<ICommandContext> Process => (cc) =>
            {
                var ph = cc.Source as PolygonHelper;
                ph.RecalcArea();
            };
        }
        public class AlignByMinimumRectangleCommand : ICommand
        {
            public string Name => "align by minimum rectangle";

            public Action<ICommandContext> Process => (cc) =>
            {
                var ph = cc.Source as PolygonHelper;
                var ang = GeometryUtil.GetMinimumBoxAngle(ph.TransformedPoints().Select(z => new Vector2d(z.X, z.Y)).ToArray());
                ph.Rotation = GeometryUtil.ToDegrees(ang);
            };
        }

        public ICommand[] Commands => new ICommand[] { new AreaRecalcCommand(), new AlignByMinimumRectangleCommand() };

        public void RecalcArea()
        {
            Area = Math.Abs(StaticHelpers.signed_area(Polygon.Points));
        }
    }
}