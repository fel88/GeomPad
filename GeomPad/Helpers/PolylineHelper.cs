using OpenTK;
using System.Collections.Generic;
using System.Drawing;

namespace GeomPad.Helpers
{
    [XmlParse(XmlKey = "polyline")]
    public class PolylineHelper : HelperItem
    {
        public bool DrawPoints { get; set; }

        public List<Vector2d> Points = new List<Vector2d>();
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
