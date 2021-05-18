using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

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
                gp.AddPolygon(Polygon.Points.Select(z => dc.Transform(z)).ToArray());

                foreach (var item in Polygon.Childrens)
                {
                    gp.AddPolygon(item.Points.Select(z => dc.Transform(z)).ToArray());
                }

                dc.gr.FillPath(FillBrush, gp);

            }
            foreach (var item in Polygon.Points)
            {
                var tr1 = dc.Transform(item);
                dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
            }

            for (int i = 0; i < Polygon.Points.Length; i++)
            {
                var j = (i + 1) % Polygon.Points.Length;
                var tr1 = dc.Transform(Polygon.Points[i]);
                var tr2 = dc.Transform(Polygon.Points[j]);
                dc.gr.DrawLine(pen, tr1, tr2);
            }
            foreach (var ch in Polygon.Childrens)
            {
                foreach (var item in ch.Points)
                {
                    var tr1 = dc.Transform(item);
                    dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                }
                for (int i = 0; i < ch.Points.Length; i++)
                {
                    var j = (i + 1) % ch.Points.Length;
                    var tr1 = dc.Transform(ch.Points[i]);
                    var tr2 = dc.Transform(ch.Points[j]);
                    dc.gr.DrawLine(pen, tr1, tr2);
                }
            }
        }
    }
}
