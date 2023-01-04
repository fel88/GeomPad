using GeomPad.Common;
using OpenTK;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "segmentHelper")]
    public class SegmentHelper : HelperItem, ICommandsContainer
    {
        public Vector2d Point;
        public Vector2d Point2;

        public bool DrawArrowCap { get; set; }
        public bool DrawPoints { get; set; }

        public class SegmentExpandAlongCommand : ICommand
        {
            public string Name => "expand along direction";

            public Action<ICommandContext> Process => (cc) =>
            {
                var ln = cc.Source as SegmentHelper;
                ln.Point += -ln.Dir * 100;
                ln.Point2 += ln.Dir * 100;
            };
        }

        public override RectangleF? BoundingBox()
        {
            var maxx = (float)Math.Max(Point.X, Point2.X);
            var maxy = (float)Math.Max(Point.Y, Point2.Y);
            var minx = (float)Math.Min(Point.X, Point2.X);
            var miny = (float)Math.Min(Point.Y, Point2.Y);
            return new RectangleF(minx, miny, maxx - minx, maxy - miny);
        }
        public Vector2d PointOfElement(double t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            double dx = Point2.X - Point.X;
            double dy = Point2.Y - Point.Y;
            return new Vector2d(Point.X + dx * t, Point.Y + dy * t);
        }
        public string X2
        {
            get => Point2.X + "";
            set => Point2.X = value.ParseFloat();
        }
        public string Y2
        {
            get => Point2.Y + "";
            set => Point2.Y = value.ParseFloat();
        }
        public string X
        {
            get => Point.X + "";
            set => Point.X = value.ParseFloat();
        }
        public string Y
        {
            get => Point.Y + "";
            set => Point.Y = value.ParseFloat();
        }
        public Color Color { get; set; } = Color.Black;
        public float Thickness { get; set; } = 1;
        public double Length { get => (Point - Point2).Length; }
        public Vector2d Dir { get => (Point2 - Point).Normalized(); }
        public bool ArrowZoomRelative { get; set; } = false;

        public double ArrowLen { get; set; } = 3;
        public int ArrowAng { get; set; } = 35;

        public ICommand[] Commands => new ICommand[] { new SegmentExpandAlongCommand() };


        public override void Draw(IDrawingContext idc)
        {
            var dc = idc as DrawingContext;
            if (!Visible) return;

            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            Pen pen = new Pen(Color, Thickness);
            if (Selected)
            {
                br = Brushes.Red;
                pen = new Pen(Color.Red, Thickness);
            }
            var tr1 = dc.Transform(Point.ToPointF());
            var tr2 = dc.Transform(Point2.ToPointF());
            dc.gr.DrawLine(pen, tr1, tr2);
            if (DrawPoints)
            {
                dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                dc.gr.FillEllipse(br, tr2.X - r, tr2.Y - r, 2 * r, 2 * r);
            }

            var arrowLen = ArrowLen;
            if (!ArrowZoomRelative)
                arrowLen /= dc.zoom;

            if (DrawArrowCap)
            {
                var p11 = -Dir * arrowLen;
                Matrix mtr = new Matrix();
                mtr.RotateAt((float)ArrowAng, new PointF(0, 0));
                PointF[] pnt = new PointF[] {
                    new PointF((float)p11.X, (float)p11.Y) };
                mtr.TransformPoints(pnt);
                p11 = new Vector2d(pnt[0].X, pnt[0].Y);
                p11 += Point2;
                var tp11 = dc.Transform(p11.ToPointF());
                dc.gr.DrawLine(pen, tr2, tp11);

                mtr = new Matrix();
                p11 = -Dir * arrowLen;
                pnt = new PointF[] {
                    new PointF((float)p11.X, (float)p11.Y) };
                mtr.RotateAt((float)-ArrowAng, new PointF(0, 0));
                mtr.TransformPoints(pnt);
                p11 = new Vector2d(pnt[0].X, pnt[0].Y);
                p11 += Point2;
                var tp22 = dc.Transform(p11.ToPointF());
                dc.gr.DrawLine(pen, tr2, tp22);
                dc.gr.FillPolygon(Brushes.Blue, new PointF[] { tr2, tp22, tp11 });
            }
        }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<segmentHelper name=\"{Name}\" x1=\"{X}\" x2=\"{X2}\" y1=\"{Y}\" y2=\"{Y2}\" thickness=\"{Thickness}\" color=\"{Color.ToArgb()}\" />");
        }

        public override void ParseXml(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;

            if (item.Attribute("thickness") != null)
                Thickness = StaticHelpers.ParseFloat(item.Attribute("thickness").Value);

            if (item.Attribute("color") != null)
                Color = Color.FromArgb(int.Parse(item.Attribute("thickness").Value));

            var x1 = double.Parse(item.Attribute("x1").Value);
            var x2 = double.Parse(item.Attribute("x2").Value);
            var y1 = double.Parse(item.Attribute("y1").Value);
            var y2 = double.Parse(item.Attribute("y2").Value);

            Point = new Vector2d(x1, y1);
            Point2 = new Vector2d(x2, y2);
        }

        public override AbstractHelperItem Clone()
        {
            SegmentHelper ret = new SegmentHelper();
            ret.Point = Point;
            ret.Point2 = Point2;
            return ret;
        }

        internal void Reverse()
        {
            var temp = Point2;
            Point2 = Point;
            Point = temp;
        }
    }
}
