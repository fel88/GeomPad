using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeomPad
{
    public class PolygonHelper : HelperItem
    {
        public List<PointF> Points = new List<PointF>();

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
        public Brush FillBrush = Brushes.Green;
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
            foreach (var item in Points)
            {
                var tr1 = dc.Transform(item);
                dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
            }
            for (int i = 0; i < Points.Count; i++)
            {
                var j = (i + 1) % Points.Count;
                var tr1 = dc.Transform(Points[i]);
                var tr2 = dc.Transform(Points[j]);
                dc.gr.DrawLine(pen, tr1, tr2);
            }
            if (Fill && Points.Count >= 3)
            {
                dc.gr.FillPolygon(FillBrush, Points.Select(z => dc.Transform(z)).ToArray());
            }
        }
    }
}
