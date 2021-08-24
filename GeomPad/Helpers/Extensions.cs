using OpenTK;
using System.Drawing;

namespace GeomPad.Helpers
{
    public static class Extensions
    {
        public static PointF ToPointF(this Vector2d d)
        {
            return new PointF((float)d.X, (float)d.Y);
        }
    }
}
