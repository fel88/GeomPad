using OpenTK;

namespace GeomPad.Helpers3D
{
    public class Plane
    {
        public Vector3d Normal;
        public double W;

        public Plane(Vector3d normal, double w)
        {
            Normal = normal;
            W = w;
        }

        public static Plane FromPoints(Vector3d a, Vector3d b, Vector3d c)
        {
            var n = Vector3d.Cross((b - a), (c - a)).Normalized();
            return new Plane(n, Vector3d.Dot(n, a));
        }
    }
}
