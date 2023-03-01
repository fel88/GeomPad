using OpenTK;

namespace GeomPad.Helpers
{
    public class Line2D
    {
        public Vector2d Start;
        public Vector2d End;
        public double Len => (End - Start).Length;

        public Vector2d GetProj(Vector2d pz)
        {
            return GeometryUtils.point_on_line(Start, End, pz);
        }
    }
}
