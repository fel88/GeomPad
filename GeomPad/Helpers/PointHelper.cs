﻿using OpenTK;
using System.Drawing;

namespace GeomPad.Helpers
{
    public class PointHelper : HelperItem
    {
        public Vector2d Point;

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
            if (Selected)
            {
                br = Brushes.Red;
            }
            var tr = dc.Transform(Point.ToPointF());
            dc.gr.FillEllipse(br, tr.X - r, tr.Y - r, 2 * r, 2 * r);

        }
    }
}
