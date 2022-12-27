using OpenTK;
using System;
using System.Drawing;
using System.Linq;

namespace GeomPad.Helpers
{
    public class PointHelper : HelperItem, ICommandsContainer
    {
        public Vector2d Point;
        public ICommand[] Commands => new ICommand[] { new ProjectionPoint() };
        public class ProjectionPoint : ICommand
        {
            public ProjectionPoint()
            {

            }

            public string Name => "project point to line (polyline)";

            public Action<ICommandContext> Process => (cc) =>
            {
                PointHelper mh = new PointHelper();
                var ee = cc.Source as PointHelper;
                if (cc.Operands.Length == 0)
                {
                    cc.Parent.SetStatus("no operands", StatusMessageType.Warning);
                    return;
                }

                if (cc.Operands[0] is SegmentHelper sh)
                {
                    Line2D l2d = new Line2D()
                    {
                        Start = sh.Point,
                        End = sh.Point2
                    };

                    var proj = l2d.GetProj(ee.Point);
                    mh.Point = proj;
                    cc.Parent.AddHelper(mh);

                }
                else
                {
                    cc.Parent.SetStatus("unknown operand", StatusMessageType.Warning);
                }
            };
        }
        public string X
        {
            get => Point.X.ToString();
            set => Point.X = value.ParseDouble();
        }
        public string Y
        {
            get => Point.Y.ToString();
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
