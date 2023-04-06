using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace GeomPad.Helpers3D
{
    public class MeshHelper : HelperItem, IEditFieldsContainer, ICommandsContainer, IPointsProvider, ITrianglesProvider
    {
        public Mesh Mesh = new Mesh();

        public MeshHelper() { }

        public MeshHelper(XElement elem)
        {
            Mesh = new Mesh();
            foreach (var item in elem.Elements("triangle"))
            {
                List<VertexInfo> pnts = new List<VertexInfo>();
                foreach (var point in item.Descendants("vertex"))
                {
                    var x = point.Attribute("x").Value.ParseDouble();
                    var y = point.Attribute("y").Value.ParseDouble();
                    var z = point.Attribute("z").Value.ParseDouble();

                    var nx = point.Attribute("nx").Value.ParseDouble();
                    var ny = point.Attribute("ny").Value.ParseDouble();
                    var nz = point.Attribute("nz").Value.ParseDouble();
                    VertexInfo vi = new VertexInfo();
                    vi.Position = new Vector3d(x, y, z);
                    vi.Normal = new Vector3d(nx, ny, nz);
                    pnts.Add(vi);
                }
                Mesh.Triangles.Add(new TriangleInfo() { Vertices = pnts.ToArray() });
            }
        }
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<mesh >");
            foreach (var item in Mesh.Triangles)
            {
                sb.AppendLine($"<triangle>");
                foreach (var vv in item.Vertices)
                {
                    sb.AppendLine($"<vertex x=\"{vv.Position.X}\" y=\"{vv.Position.Y}\" z=\"{vv.Position.Z}\" nx=\"{vv.Normal.X}\" ny=\"{vv.Normal.Y}\" nz=\"{vv.Normal.Z}\" />");
                }
                sb.AppendLine($"</triangle>");

            }
            sb.AppendLine($"</mesh >");
        }

        public ICommand[] Commands => new ICommand[] {
            new MeshHelperSplitByRayCommand(),
            new ExportMeshToObjCommand(),
            new SplitByPlaneCommand(),
            new SliceByPlaneCommand()
        };
        public class ExportMeshToObjCommand : ICommand
        {
            public string Name => "export to .obj";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "obj models|*.obj";
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                const float tolerance = 1e-8f;
                StringBuilder sb = new StringBuilder();
                List<Vector3d> vvv = new List<Vector3d>();
                foreach (var item in tr.Mesh.Triangles)
                {
                    foreach (var v in item.Vertices)
                    {
                        if (vvv.Any(z => (z - v.Position).Length < tolerance))
                            continue;

                        vvv.Add(v.Position);
                        sb.AppendLine($"v {v.Position.X} {v.Position.Y} {v.Position.Z}".Replace(",", "."));
                    }
                }
                int counter = 1;
                foreach (var item in tr.Mesh.Triangles)
                {
                    if (item.Vertices.Length != 3)
                        continue;

                    List<int> indc = new List<int>();

                    foreach (var vitem in item.Vertices)
                    {
                        for (int k = 0; k < vvv.Count; k++)
                        {
                            if ((vvv[k] - vitem.Position).Length < tolerance)
                            {
                                indc.Add(k + 1);
                            }
                            else
                                continue;
                        }
                    }
                    if (indc.GroupBy(z => z).Any(z => z.Count() > 1))
                    {
                        continue;
                        //throw duplicate face vertex
                    }
                    sb.AppendLine($"f {indc[0]} {indc[1]} {indc[2]}");
                }

                File.WriteAllText(sfd.FileName, sb.ToString());
                cc.Parent.SetStatus("exported: " + sfd.FileName, StatusMessageType.Info);
            };
        }

        public IHelperItem[] SliceByPlane(PlaneHelper pl)
        {
            List<IHelperItem> rets = new List<IHelperItem>();
            var tr = this;
            tr.SplitByPlane(pl);
            List<Line3D> lines = new List<Line3D>();
            foreach (var item in tr.Mesh.Triangles)
            {
                var pp = item.Vertices.Where(z => pl.IsOnPlane(z.Position)).ToArray();
                if (pp.Length == 2)
                {
                    lines.Add(new Line3D() { Start = pp[0].Position, End = pp[1].Position });
                }
            }

            if (lines.Count == 0)
                return new IHelperItem[] { };

            PolylineHelper ret = new PolylineHelper();
            List<Line3D> contour = new List<Line3D>();

            contour.Add(lines.First());
            lines.RemoveAt(0);
            float eps = 1e-3f;
            while (lines.Any())
            {
                Line3D todel = null;
                foreach (var line in lines)
                {
                    var l1 = (contour.Last().End - line.Start).Length;
                    var l2 = (contour.Last().End - line.End).Length;

                    if (l1 < eps)
                    {
                        contour.Add(new Line3D() { Start = line.Start, End = line.End });
                        todel = line;
                        break;
                    }
                    else
                    if (l2 < eps)
                    {
                        contour.Add(new Line3D() { Start = line.End, End = line.Start });
                        todel = line;
                        break;
                    }
                }
                if (todel != null)
                {
                    lines.Remove(todel);
                }
                else
                {
                    ret.Verticies.AddRange(contour.Select(z => z.End));
                    rets.Add(ret);
                    contour = new List<Line3D>();
                    ret = new PolylineHelper();
                    contour.Add(lines.First());
                    lines.RemoveAt(0);
                }

            }
            if (contour.Any())
            {
                ret.Verticies.AddRange(contour.Select(z => z.End));
                rets.Add(ret);
            }
            return rets.ToArray();
        }

        public class SliceByPlaneCommand : ICommand
        {
            public string Name => "slice by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;

                var rr = tr.SliceByPlane(pl);
                tr.dirty = true;
                cc.Parent.AddHelpers(rr);
                cc.Parent.UpdateHelpersList();
            };
        }

        public void SplitByPlane(PlaneHelper pl)
        {
            var tr = this;
            List<int> toDel = new List<int>();
            List<int> toDelStrict = new List<int>();

            List<TriangleInfo> toAdd2 = new List<TriangleInfo>();
            for (int i1 = 0; i1 < tr.Mesh.Triangles.Count; i1++)
            {
                TriangleInfo item = tr.Mesh.Triangles[i1];
                var th = new TriangleHelper()
                {
                    V0 = item.Vertices[0].Position,
                    V1 = item.Vertices[1].Position,
                    V2 = item.Vertices[2].Position
                };

                var res = th.SplitByPlane(pl);

                if (res == null)
                    continue;

                res = res.Where(z => z is TriangleHelper).ToArray();

                if (res.Length == 0)
                    continue;

                toDel.Add(i1);
                toDelStrict.Add(i1);
                List<TriangleHelper> toAdd = new List<TriangleHelper>();
                foreach (var ttt in res.OfType<TriangleHelper>())
                {
                    var vv1 = ttt.Vertices.Where(z => !pl.IsOnPlane(z)).ToArray();
                    if (pl.SideOfPlane(vv1[0]) >= 0)
                    {
                        toAdd.Add(ttt);
                    }
                }

                foreach (var zitem in toAdd)
                {
                    var t1 = new TriangleInfo() { Vertices = new VertexInfo[3] };

                    toAdd2.Add(t1);
                    for (int i = 0; i < 3; i++)
                    {
                        t1.Vertices[i] = new VertexInfo();
                    }
                    t1.Vertices[0].Position = zitem.V0;
                    t1.Vertices[1].Position = zitem.V1;
                    t1.Vertices[2].Position = zitem.V2;
                }
            }
            tr.Mesh.Triangles.AddRange(toAdd2);

            for (int i = 0; i < tr.Mesh.Triangles.Count; i++)
            {
                TriangleInfo ttt = tr.Mesh.Triangles[i];
                var vv1 = ttt.Vertices.Where(z => !pl.IsOnPlane(z.Position)).ToArray();
                if (vv1.Length == 0)
                {
                    toDel.Add(i);
                    continue;
                }

                if (pl.SideOfPlane(vv1[0].Position) < 0)
                {
                    toDel.Add(i);
                }
            }
            var ar = toDel.Union(toDelStrict).Distinct().ToArray();
            Array.Sort(ar);
            Array.Reverse(ar);
            foreach (var item in ar)
            {
                tr.Mesh.Triangles.RemoveAt(item);
            }
        }

        public class SplitByPlaneCommand : ICommand
        {
            public string Name => "split by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;
                tr.SplitByPlane(pl);
                tr.dirty = true;
                cc.Parent.UpdateHelpersList();
            };
        }


        public class MeshHelperSplitByRayCommand : ICommand
        {
            public string Name => "split by ray";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;
                var ray = cc.Operands.First(t => t is LineHelper) as LineHelper;

                MouseRay mr = new MouseRay(ray.Start, ray.End);
                var dd = Intersection.CheckIntersect(mr, tr.Mesh.Triangles.ToArray());

                if (dd != null)
                {
                    cc.Parent.AddHelper(new PointHelper() { Position = dd.Point });
                    cc.Parent.SetStatus("intersection found: " + dd.Point.ToString(), StatusMessageType.Info);
                }
                else
                    cc.Parent.SetStatus("intersection not found.", StatusMessageType.Warning);
            };
        }

        public bool DrawFill { get; set; } = true;
        public bool DrawWireframe { get; set; } = true;

        public Color Color { get; set; } = Color.Orange;

        static int NextGlList = 1;
        int GlList;
        bool dirty;
        bool first = true;

        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);

            if (DrawWireframe)
            {
                GL.LineWidth(1);
                foreach (var item in Mesh.Triangles)
                {
                    GL.Begin(PrimitiveType.LineLoop);
                    foreach (var vv in item.Vertices)
                    {
                        GL.Vertex3(vv.Position);
                    }
                    GL.End();
                }
            }

            GL.Color3(Color);
            if (Selected)
            {
                GL.Color3(Color.Red);
            }
            if (DrawFill)
            {
                if (!FlatShading)
                    GL.Enable(EnableCap.Lighting);

                if (first)
                    GlList = NextGlList++;

                if (dirty || first)
                {
                    GL.NewList(GlList, ListMode.Compile);
                    GL.Begin(PrimitiveType.Triangles);

                    foreach (var item in Mesh.Triangles)
                    {
                        foreach (var vv in item.Vertices)
                        {
                            if (!FlatShading)
                                GL.Normal3(vv.Normal);

                            GL.Vertex3(vv.Position);
                        }
                    }
                    GL.End();
                    GL.EndList();
                    first = false;
                    dirty = false;
                }
                else
                    GL.CallList(GlList);

                if (!FlatShading)
                    GL.Disable(EnableCap.Lighting);
            }
        }

        bool flatShading = true;
        public bool FlatShading
        {
            get => flatShading; set
            {
                if (flatShading != value)
                    dirty = true;
                flatShading = value;
            }
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            foreach (var item in Mesh.Triangles)
            {
                foreach (var vert in item.Vertices)
                {
                    yield return vert.Position;
                }
            }
        }

        public IEnumerable<TriangleInfo> GetTriangles()
        {
            return Mesh.Triangles;
        }
    }
}
