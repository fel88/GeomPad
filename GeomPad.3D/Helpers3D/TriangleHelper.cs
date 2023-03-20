using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System;
using GeomPad.Common;
using System.Runtime.InteropServices;

namespace GeomPad.Helpers3D
{
    public class TriangleHelper : HelperItem, IEditFieldsContainer, ICommandsContainer, IPointsProvider
    {
        [EditField]
        public Vector3d V0;
        [EditField]
        public Vector3d V1;
        [EditField]
        public Vector3d V2;


        public TriangleHelper() { }
        public double Area
        {
            get
            {
                return Vector3d.Cross(V1 - V0, V2 - V0).Length / 2;
            }
        }
        public TriangleHelper(XElement item)
        {
            int ind = 0;
            foreach (var vitem in item.Elements("vertex"))
            {
                var pos = vitem.Attribute("pos").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
                if (ind == 0) V0 = new Vector3d(pos[0], pos[1], pos[2]);
                if (ind == 1) V1 = new Vector3d(pos[0], pos[1], pos[2]);
                if (ind == 2) V2 = new Vector3d(pos[0], pos[1], pos[2]);
                ind++;
            }
        }

        public Vector3d[] Vertices
        {
            get
            {
                return new[] { V0, V1, V2 };
            }
            set
            {
                V0 = value[0];
                V1 = value[1];
                V2 = value[2];
            }
        }

        public ICommand[] Commands => new[] { new TriangleHelperSplitByPlaneCommand() };

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<triangle>");
            foreach (var item in Vertices)
            {
                sb.AppendLine($"<vertex pos=\"{item.X};{item.Y};{item.Z}\"/>");
            }
            sb.AppendLine($"</triangle>");
        }

        public Color Color = Color.Orange;
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.LineLoop);
            foreach (var item in Vertices)
            {
                GL.Vertex3(item);
            }
            GL.End();
            GL.Color3(Color);
            if (Selected)
            {
                GL.Color3(Color.Red);
            }
            GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Vertices)
            {
                GL.Vertex3(item);
            }
            GL.End();
        }

        internal Vector3d Center()
        {
            var s = Vector3d.Zero;
            foreach (var item in Vertices)
            {
                s += item;
            }
            return s / 3;
        }

        internal HelperItem[] SplitByPlane(PlaneHelper pl)
        {
            if (Vertices.Count(z => pl.IsOnPlane(z)) >= 2)
            {
                return null;
            }
            var pl0 = GetPlane();
            var ln = pl0.Intersect(pl);
            if (ln == null)
                return null;

            var lns = GetLines().Cast<LineHelper>();
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in lns)
            {
                var l3 = item.Get3DLine();
                var inter = Geometry.Intersect3dCrossedLines(ln, l3);
                if (inter != null && l3.IsPointInsideSegment(inter.Value)) pp.Add(inter.Value);
            }
            List<HelperItem> ret = new List<HelperItem>();
            var pnts = pp.Select(z => new PointHelper() { Position = z }).ToArray();
            List<PointHelper> pnts3 = new List<PointHelper>();
            foreach (var item in pnts)
            {
                bool good = true;
                for (int i = 0; i < pnts3.Count; i++)
                {
                    if ((pnts3[i].Position - item.Position).Length < 1e-6)
                    {
                        good = false;
                        break;
                    }
                }
                if (good) pnts3.Add(item);
            }
            pnts = pnts3.ToArray();
            if (pnts.Length == 2)
            {
                ret.Add(new LineHelper() { Start = pnts[0].Position, End = pnts[1].Position });

                var up = Vertices.Where(z => pl.SideOfPlane(z) >= 0).ToArray();
                var down = Vertices.Where(z => pl.SideOfPlane(z) < 0).ToArray();
                var ss1 = up.Union(pnts.Select(zz => zz.Position)).ToArray();
                var ss2 = down.Union(pnts.Select(zz => zz.Position)).ToArray();
                Vector3d[][] array = new[] { ss1, ss2 };
                for (int i = 0; i < array.Length; i++)
                {
                    Vector3d[] item2 = array[i];
                    NFP nfp = new NFP();
                    var item = item2.ToArray();
                    for (int k = 0; k < item.Length; k++)
                    {
                        var proj = GetUV(item[k]);
                        nfp.AddPoint(new SvgPoint(proj.X, proj.Y));
                    }
                    var res = Simplify.simplifyDouglasPeucker(nfp, 1e-6);
                    if (res.Length != item.Length)
                    {
                        List<Vector3d> rr = new List<Vector3d>();
                        foreach (var zz in item)
                        {
                            if (rr.Any(t => (t - zz).Length < 1e-6f))
                                continue;

                            rr.Add(zz);
                        }
                        item = rr.ToArray();
                    }
                    if (item.Length == 3)
                    {
                        ret.Add(new TriangleHelper()
                        {
                            V0 = item[0],
                            V1 = item[1],
                            V2 = item[2]
                        });
                    }
                    else if (item.Length == 4)
                    {
                        PlaneSurface ps = new PlaneSurface();
                        ps.Normal = pl.Normal;
                        ps.Position = pl.Position;
                        var pl2 = GetPlane();

                        List<Vector3d> rr = new List<Vector3d>();
                        rr.Add(pnts[0].Position);
                        rr.Add(pnts[1].Position);
                        Vector3d[] addp = i == 0 ? up : down; ;
                        rr.AddRange(addp);

                        var trs = BuildTriangles(rr.ToArray());
                        ret.AddRange(new[]{
                            new TriangleHelper()
                        {
                            V0 = trs[0][0],
                            V1 = trs[0][1],
                            V2 = trs[0][2],
                        } ,new TriangleHelper()
                        {
                            V0 = trs[1][0],
                            V1 = trs[1][1],
                            V2 = trs[1][2],
                        }}
                        );
                    }
                }
            }
            else
            {
                ret.AddRange(pnts);
            }
            return ret.ToArray();
        }

        public Vector2d GetUV(Vector3d vector)
        {

            Vector3d u = new Vector3d();
            Vector3d v = new Vector3d();
            var vv = Vertices;
            for (int k = 2; k < vv.Length; k++)
            {
                var cross = Vector3d.Cross(vv[k] - vv[0], vv[1] - vv[0]);
                if (cross.Length < 1e-6)
                    continue;

                var normal = cross.Normalized();
                u = (vv[k] - vv[0]).Normalized();
                v = Vector3d.Cross(normal, u).Normalized();
                break;
            }
            var ucoord = Vector3d.Dot(u, vector - vv[0]);
            var vcoord = Vector3d.Dot(v, vector - vv[0]);
            return new Vector2d(ucoord, vcoord);
        }

        public Vector3d[][] BuildTriangles(Vector3d[] pnts)
        {
            List<Vector3d[]> ret = new List<Vector3d[]>();
            List<Vector3d> r = new List<Vector3d>();

            int startIdx = 0;
            var start = pnts[startIdx];
            byte[] processed = new byte[pnts.Length];
            r.Add(pnts[0]);
            processed[0] = 1;
            int cntr = 0;
            while (r.Count < pnts.Length)
            {
                cntr++;
                if (cntr > 30)                
                    throw new Exception("looped");
                
                for (int i = 0; i < pnts.Length; i++)
                {
                    if (processed[i] == 1)
                        continue;

                    var v0 = pnts[i] - start;
                    List<int> signs = new List<int>();
                    for (int j = 0; j < pnts.Length; j++)
                    {
                        if (j == startIdx)
                            continue;
                        if (i == j)
                            continue;

                        var v1 = pnts[j] - start;
                        signs.Add(Math.Sign(Vector3d.Cross(v0, v1).Z));

                    }
                    if (signs.GroupBy(z => z).Count() == 1)
                    {
                        r.Add(pnts[i]);
                        start = pnts[i];
                        startIdx = i;
                        processed[i] = 1;
                        break;
                    }

                }
            }
            ret.Add(r.Take(3).ToArray());
            ret.Add(new Vector3d[] { r[2], r[3], r[0] });
            return ret.ToArray();
        }

        public HelperItem[] GetLines()
        {
            List<HelperItem> ret = new List<HelperItem>();
            ret.Add(new LineHelper() { Start = V0, End = V1 });
            ret.Add(new LineHelper() { Start = V1, End = V2 });
            ret.Add(new LineHelper() { Start = V2, End = V0 });
            return ret.ToArray();
        }

        public PlaneHelper GetPlane()
        {
            var n0 = V2 - V0;
            var n1 = V1 - V0;
            var normal = Vector3d.Cross(n0, n1).Normalized();
            return (new PlaneHelper() { Position = V0, Normal = normal });
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            return Vertices;
        }

        public class TriangleHelperSplitByPlaneCommand : ICommand
        {
            public string Name => "split by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as TriangleHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;
                var res = tr.SplitByPlane(pl);
                cc.Parent.AddHelpers(res);
            };
        }
    }
}
