using GeomPad.Helpers;
using System.Drawing;

namespace GeomPad
{
    public class PolylinePointEditorWrapper : IPoint
    {
        public PolylinePointEditorWrapper(PolylineHelper ph, int index)
        {
            this._index = index;
            _target = ph;
        }
        public PointF Point;
        public double X { get => _target.Points[_index].X; set => _target.Points[_index] = new OpenTK.Vector2d(value, _target.Points[_index].Y); }
        public double Y { get => _target.Points[_index].Y; set => _target.Points[_index] = new OpenTK.Vector2d(_target.Points[_index].X, value); }
        int _index;
        PolylineHelper _target;
    }
}
