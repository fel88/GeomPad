using OpenTK;
using System.Drawing;
using System.Globalization;

namespace GeomPad.Helpers
{
    public static class Extensions
    {
        public static PointF ToPointF(this Vector2d d)
        {
            return new PointF((float)d.X, (float)d.Y);
        }
        public static double ParseDouble(this string z)
        {
            return double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }
}
