using OpenTK;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "polyline")]
    public class PolylineHelper : HelperItem
    {
        public bool DrawPoints { get; set; }

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
