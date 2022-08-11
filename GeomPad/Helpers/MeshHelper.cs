using OpenTK;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace GeomPad.Helpers
{
    public class MeshHelper : HelperItem
    {
        public MeshHelper()
        {

        }

        public MeshHelper(Vector2d[][] triangles)
        {
            _mesh = triangles;
        }

        Vector2d[][] _mesh;
        public Vector2d[][] Mesh { get => _mesh; }
        bool _fill = true;
        public bool Fill { get => _fill; set { _fill = value; Changed?.Invoke(); } }

        public int TianglesCount
        {
            get
            {
                if (_mesh == null) return 0;
                return _mesh.Length;
            }
        }

        public Color FillColor
        {
            get
            {
                return (FillBrush as SolidBrush).Color;
            }
            set
            {
                FillBrush = new SolidBrush(value);
                Changed?.Invoke();
            }
        }
        public bool DrawPoints { get; set; } = false;
        public bool DrawWireframe { get; set; } = true;
        public Brush FillBrush = SystemBrushes.Highlight;
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

            foreach (var item in _mesh)
            {
                GraphicsPath gp = new GraphicsPath();
                if (Fill && item.Length >= 3)
                {
                    gp.AddPolygon(item.Select(z => dc.Transform(z)).ToArray());

                    dc.gr.FillPath(FillBrush, gp);

                }
                if (DrawPoints)
                    foreach (var item2 in item)
                    {
                        var tr1 = dc.Transform(item2);
                        dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                    }

                if (DrawWireframe)
                    for (int i = 0; i < item.Length; i++)
                    {
                        var j = (i + 1) % item.Length;
                        var tr1 = dc.Transform(item[i]);
                        var tr2 = dc.Transform(item[j]);
                        dc.gr.DrawLine(pen, tr1, tr2);
                    }
            }
        }
    }
}