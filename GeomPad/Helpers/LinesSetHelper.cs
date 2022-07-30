using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "linesSet")]
    public class LinesSetHelper : HelperItem
    {
        public List<Line2D> Lines = new List<Line2D>();


        public bool DrawPoints { get; set; }

        public override RectangleF? BoundingBox()
        {
            return null;
        }

        public Pen Color { get; set; } = Pens.Black;




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
