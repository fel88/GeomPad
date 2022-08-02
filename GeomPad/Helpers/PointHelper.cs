using OpenTK;
using System.Drawing;

namespace GeomPad.Helpers
{
    public class PointHelper : HelperItem
    {
        public Vector2d Point;

        public string X
        {
            get => Point.X + "";
            set => Point.X = value.ParseDouble();
        }
        public string Y
        {
            get => Point.Y + "";
            set => Point.Y = value.ParseDouble();
        }

        public bool DrawCross { get; set; }
        public float CrossSize { get; set; } = 5;
        public bool CrossSizeScaleRelative { get; set; } = true;
        public float CrossLineThickness { get; set; } = 2;
        public override void Draw(IDrawingContext idc)
        {
            var dc = idc as DrawingContext;
            if (!Visible) return;
            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            if (Selected)
            {
                br = Brushes.Red;
            }
            var tr = dc.Transform(Point.ToPointF());
            dc.gr.FillEllipse(br, tr.X - r, tr.Y - r, 2 * r, 2 * r);
            if (DrawCross)
            {
                var csz = CrossSize;

                if (CrossSizeScaleRelative)
                    csz *= dc.zoom;

                var clr = Color.Black;
                if (Selected) clr = Color.Red;

                var pen = new Pen(clr, CrossLineThickness);
                dc.gr.DrawLine(pen, tr.X - csz, tr.Y, tr.X + csz, tr.Y);
                dc.gr.DrawLine(pen, tr.X, tr.Y - csz, tr.X, tr.Y + csz);
            }
        }
    }
}
