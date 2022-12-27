using System.Drawing;

namespace GeomPad
{
    public class PolygonPointEditorWrapper : IPoint
    {
        public PolygonPointEditorWrapper(NFP ph, int index)
        {
            this._index = index;
            _polygon = ph;
        }
        public PointF Point;
        public double X { get => _polygon.Points[_index].X; set => _polygon.Points[_index].X = value; }
        public double Y { get => _polygon.Points[_index].Y; set => _polygon.Points[_index].Y = value; }
        int _index;
        NFP _polygon;
    }
}
