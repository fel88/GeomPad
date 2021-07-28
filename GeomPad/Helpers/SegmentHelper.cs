using System.Drawing;

namespace GeomPad.Helpers
{
    public class SegmentHelper : HelperItem
    {
        public PointF Point;
        public PointF Point2;

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
            var tr1 = dc.Transform(Point);
            var tr2 = dc.Transform(Point2);
            dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
            dc.gr.DrawLine(pen, tr1, tr2);
            dc.gr.FillEllipse(br, tr2.X - r, tr2.Y - r, 2 * r, 2 * r);
        }
    }
}
