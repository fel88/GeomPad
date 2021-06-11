using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace GeomPad
{
    public class TriangleHelper : HelperItem3D, IEditFieldsContainer, ICommandsContainer
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

        public Vector3d[] Verticies
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

        public override void AppendXml(StringBuilder sb)
        {
            sb.AppendLine($"<triangle>");
            foreach (var item in Verticies)
            {
                sb.AppendLine($"<vertex pos=\"{item.X};{item.Y};{item.Z}\"/>");
            }
            sb.AppendLine($"</triangle>");
        }

        public override void Draw()
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.LineLoop);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();
            GL.Color3(Color.Orange);
            if (Selected)
            {
                GL.Color3(Color.Red);
            }
            GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();
        }

        public IName[] GetObjects()
        {
            List<VectorEditor> ret = new List<VectorEditor>();
            var fld = GetType().GetFields();
            for (int i = 0; i < fld.Length; i++)
            {
                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    ret.Add(new VectorEditor(fld[i]) { Object = this });
                }
            }
            return ret.ToArray();
        }

        internal HelperItem3D[] SplitByPlane(PlaneHelper pl)
        {
            var pl0 = GetPlane();
            var ln = pl0.Intersect(pl);
            if (ln == null) return null;
            var lns = GetLines().Cast<Line3DHelper>();
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in lns)
            {
                var l3 = item.Get3DLine();
                var inter = Geometry.Intersect3dCrossedLines(ln, l3);
                if (inter != null && l3.IsPointInsideSegment(inter.Value)) pp.Add(inter.Value);
            }
            List<HelperItem3D> ret = new List<HelperItem3D>();
            var pnts = pp.Select(z => new Point3DHelper() { Position = z }).ToArray();
            List<Point3DHelper> pnts3 = new List<Point3DHelper>();
            foreach (var item in pnts)
            {
                bool good = true;
                for (int i = 0; i < pnts3.Count; i++)
                {
                    if ((pnts[i].Position - item.Position).Length < 1e-6)
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
                ret.Add(new Line3DHelper() { Start = pnts[0].Position, End = pnts[1].Position });
                var segs = lns.Where(z => z.Get3DLine().IsPointInsideSegment(pnts[0].Position) || z.Get3DLine().IsPointInsideSegment(pnts[1].Position)).ToArray();
                bool b1 = false;
                if (segs.Length == 3)
                {
                    b1 = true;
                    var a1 = lns.Where(z => z.Get3DLine().IsPointInsideSegment(pnts[0].Position)).ToArray();
                    var a2 = lns.Where(z => z.Get3DLine().IsPointInsideSegment(pnts[1].Position)).ToArray();
                    if (a1.Length == 1)
                    {
                        segs = a1.Union(a2.Take(1)).ToArray();
                    }
                    else
                    if (a2.Length == 1)
                    {
                        segs = a2.Union(a1.Take(1)).ToArray();
                    }
                }
                // else
                {
                    var pnts2 = segs.Select(z => new[] { z.Start, z.End }).SelectMany(z => z).ToArray();
                    var last = lns.Except(segs).First();

                    Vector3d? common = null;
                    for (int i = 0; i < pnts2.Length; i++)
                    {
                        Vector3d item = pnts2[i];
                        for (int i1 = 0; i1 < pnts2.Length; i1++)
                        {
                            if (i1 == i) continue;
                            Vector3d p = pnts2[i1];
                            if ((item - p).Length < 1e-6)
                            {
                                common = item;
                            }
                        }
                    }

                    if (common != null)
                    {
                        List<TriangleHelper> ttt = new List<TriangleHelper>();
                        ttt.Add(new TriangleHelper()
                        {
                            V0 = pnts[0].Position,
                            V1 = pnts[1].Position,
                            V2 = common.Value
                        });


                        ttt.Add(new TriangleHelper()
                        {
                            V0 = pnts[0].Position,
                            V1 = last.Start,
                            V2 = last.End
                        });

                        if ((pnts[0].Position - last.Start).LengthFast > (pnts[0].Position - last.End).LengthFast)
                        {
                            ttt.Add(new TriangleHelper()
                            {
                                V0 = pnts[0].Position,
                                V1 = last.Start,
                                V2 = pnts[1].Position
                            });
                        }
                        else
                        {
                            ttt.Add(new TriangleHelper()
                            {
                                V0 = pnts[0].Position,
                                V1 = last.End,
                                V2 = pnts[1].Position
                            });
                        }
                        ret.AddRange(ttt.Where(z => z.Area > 1e-8));
                    }
                }
            }
            else
            {
                ret.AddRange(pnts);
            }
            return ret.ToArray();
        }

        internal HelperItem3D[] GetLines()
        {
            List<HelperItem3D> ret = new List<HelperItem3D>();
            ret.Add(new Line3DHelper() { Start = V0, End = V1 });
            ret.Add(new Line3DHelper() { Start = V1, End = V2 });
            ret.Add(new Line3DHelper() { Start = V2, End = V0 });
            return ret.ToArray();
        }

        internal PlaneHelper GetPlane()
        {
            var n0 = V2 - V0;
            var n1 = V1 - V0;
            var normal = Vector3d.Cross(n0, n1);
            return (new PlaneHelper() { Position = V0, Normal = normal });
        }
    }

}
