using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class PlaneHelper : HelperItem
    {
        [EditField]
        public Vector3d Position { get { return plane.Position; } set { plane.Position = value; } }
        [EditField]
        public Vector3d Normal { get { return plane.Normal; } set { plane.Normal = value; } }
        PlaneSurface plane = new PlaneSurface();


        public PlaneHelper() { }
        public PlaneHelper(XElement item)
        {
            var pos = item.Attribute("position").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3d(pos[0], pos[1], pos[2]);
            var nrm = item.Attribute("normal").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Normal = new Vector3d(nrm[0], nrm[1], nrm[2]);
            DrawSize = int.Parse(item.Attribute("drawSize").Value);
        }

        public int DrawSize { get; set; } = 10;

        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;

            var basis = plane.GetBasis();
            if (Fill)
            {
                GL.Color3(Color.LightGreen);

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (-DrawSize));
                GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (DrawSize));
                GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (DrawSize));
                GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (-DrawSize));
                GL.End();
            }
            GL.Color3(Color.Blue);
            if (Selected) GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (-DrawSize));
            GL.Vertex3(Position + basis[0] * (-DrawSize) + basis[1] * (DrawSize));
            GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (DrawSize));
            GL.Vertex3(Position + basis[0] * (DrawSize) + basis[1] * (-DrawSize));
            GL.End();

            GL.Color3(Color.Orange);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Position);
            GL.Vertex3(Position + Normal.Normalized() * 10);

            GL.End();
        }

        public bool Fill { get; set; }
        public double[] GetKoefs()
        {
            double[] ret = new double[4];
            ret[0] = Normal.X;
            ret[1] = Normal.Y;
            ret[2] = Normal.Z;
            ret[3] = -(ret[0] * Position.X + Position.Y * ret[1] + Position.Z * ret[2]);

            return ret;
        }

        public Line3D Intersect(PlaneHelper ps)
        {
            Line3D ret = new Line3D();

            var dir = Vector3d.Cross(ps.Normal, Normal);


            var k1 = ps.GetKoefs();
            var k2 = GetKoefs();
            var a1 = k1[0];
            var b1 = k1[1];
            var c1 = k1[2];
            var d1 = k1[3];

            var a2 = k2[0];
            var b2 = k2[1];
            var c2 = k2[2];
            var d2 = k2[3];



            var res1 = det2(new[] { a1, a2 }, new[] { b1, b2 }, new[] { -d1, -d2 });
            var res2 = det2(new[] { a1, a2 }, new[] { c1, c2 }, new[] { -d1, -d2 });
            var res3 = det2(new[] { b1, b2 }, new[] { c1, c2 }, new[] { -d1, -d2 });

            List<Vector3d> vvv = new List<Vector3d>();

            if (res1 != null)
            {
                Vector3d v1 = new Vector3d((float)res1[0], (float)res1[1], 0);
                vvv.Add(v1);

            }

            if (res2 != null)
            {
                Vector3d v1 = new Vector3d((float)res2[0], 0, (float)res2[1]);
                vvv.Add(v1);
            }
            if (res3 != null)
            {
                Vector3d v1 = new Vector3d(0, (float)res3[0], (float)res3[1]);
                vvv.Add(v1);
            }

            if (vvv.Count == 0)
                return null;

            var pnt = vvv.OrderBy(z => z.Length).First();


            var r1 = plane.IsOnPlane(pnt);
            var r2 = plane.IsOnPlane(pnt);

            ret.Start = pnt;
            ret.End = pnt + dir * 100;
            return ret;
        }
        double[] det2(double[] a, double[] b, double[] c)
        {
            var d = a[0] * b[1] - a[1] * b[0];
            if (d == 0) return null;
            var d1 = c[0] * b[1] - c[1] * b[0];
            var d2 = a[0] * c[1] - a[1] * c[0];
            var x = d1 / d;
            var y = d2 / d;
            return new[] { x, y };
        }

               
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<plane position=\"{Position.X};{Position.Y};{Position.Z}\" normal=\"{Normal.X};{Normal.Y};{Normal.Z}\" drawSize=\"{DrawSize}\"/>");
        }

        public bool IsOnPlane(Vector3d p, float eps = 1e-6f)
        {
            var k = GetKoefs();
            return Math.Abs((k[0] * p.X + k[1] * p.Y) + k[2] * p.Z + k[3]) < eps;
        }

        public int SideOfPlane(Vector3d p)
        {
            PlaneSurface ps = new PlaneSurface();
            ps.Normal = Normal;
            ps.Position = Position;
            var proj = ps.ProjPoint(p);
            var v0 = (p - proj).Normalized();
            var dot = Vector3d.Dot(Normal, v0);
            return Math.Sign(dot);
        }
    }
}
