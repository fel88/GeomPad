using System.Drawing;
using System.Text;
using System;
using OpenTK;

namespace GeomPad
{
    public interface IHelperItem
    {
        string Name { get; set; }
        int ZIndex { get; set; }

        bool Selected { get; set; }
        bool Visible { get; set; }
        void Draw(IDrawingContext gr);
        Action Changed { get; set; }
        void Shift(Vector2d vector);

        void ClearSelection();

        RectangleF? BoundingBox();

        void AppendToXml(StringBuilder sb);
    }
}
