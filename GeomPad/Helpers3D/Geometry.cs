using OpenTK;
using System;

namespace GeomPad.Helpers3D
{
    public static class Geometry
    {
        public static Vector3d? Intersect3dCrossedLines(Line3D ln0, Line3D ln1)
        {
            var v0 = ln0.Start;
            var v1 = ln1.Start;
            var d0 = ln0.Dir;
            var d1 = ln1.Dir;
            var d0n = ln0.Dir.Normalized();
            var d1n = ln1.Dir.Normalized();
            var check1 = Vector3d.Dot(Vector3d.Cross(d0n, d1n), v0 - v1);
            if (Math.Abs(check1) > 10e-6) return null;//parallel


            var cd = v1 - v0;
            var a1 = Vector3d.Cross(d1, cd).Length;
            var a2 = Vector3d.Cross(d1, d0).Length;
            var vv0 = v0 + d0n * 10000;
            var vv1 = v1 + d1n * 10000;
            var m1 = v0 + (a1 / a2) * d0;

            Line3D l1 = new Line3D() { Start = v0, End = vv0 };
            Line3D l2 = new Line3D() { Start = v1, End = vv1 };

            var m2 = v0 - (a1 / a2) * d0;
            if (Vector3d.Distance(m1, m2) < 10e-6) return m1;
            float epsilon = 10e-6f;
            if (l1.IsPointOnLine(m1, epsilon) && l2.IsPointOnLine(m1, epsilon)) return m1;

            if (l1.IsPointOnLine(m2, epsilon) && l2.IsPointOnLine(m2, epsilon)) return m2;

            return m1;
        }

    }
}
