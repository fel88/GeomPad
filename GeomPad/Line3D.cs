using OpenTK;
using System;

namespace GeomPad
{
    public class Line3D
    {
        public Vector3d Start;
        public Vector3d End;
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public bool IsPointOnLine(Vector3d pnt, float epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - Start;
            if (d1.Length < tolerance) return true;
            if ((End - Start).Length < tolerance) throw new Exception("degenerated 3d line");
            var crs = Vector3d.Cross(d1.Normalized(), (End - Start).Normalized());
            return Math.Abs(crs.Length) < epsilon;
        }

        public bool IsSameLine(Line3D l)
        {
            return IsPointOnLine(l.Start) && IsPointOnLine(l.End);
        }

        public void Shift(Vector3d vector3)
        {
            Start += vector3;
            End += vector3;
        }
    }
}
