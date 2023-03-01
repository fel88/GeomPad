using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class MeshHelper : HelperItem, IEditFieldsContainer, ICommandsContainer, IFitAllable
    {
        public Mesh Mesh = new Mesh();

        public MeshHelper() { }

        public MeshHelper(XElement item)
        {

        }
        public ICommand[] Commands => new ICommand[] {
            new MeshHelperSplitByRayCommand(),
            new ExportMeshToObjCommand(),
            new SplitByPlaneCommand(),
            new SplitSectionByPlaneCommand()
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

        public class SplitSectionByPlaneCommand : ICommand
        {
            public string Name => "section by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;
                List<Line3D> lines = new List<Line3D>();
                foreach (var item in tr.Mesh.Triangles)
                {
                    var pp = item.Vertices.Where(z => pl.IsOnPlane(z.Position)).ToArray();
                    if (pp.Length == 2)
                    {
                        lines.Add(new Line3D() { Start = pp[0].Position, End = pp[1].Position });
                    }
                }

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
                        cc.Parent.AddHelper(ret);
                        contour = new List<Line3D>();
                        ret = new PolylineHelper();
                        contour.Add(lines.First());
                        lines.RemoveAt(0);
                    }

                }
                if (contour.Any())
                {
                    ret.Verticies.AddRange(contour.Select(z => z.End));
                    cc.Parent.AddHelper(ret);
                }

            };
        }

        public class SplitByPlaneCommand : ICommand
        {
            public string Name => "split by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;
                List<TriangleInfo> toDel = new List<TriangleInfo>();
                List<TriangleInfo> toAdd2 = new List<TriangleInfo>();
                foreach (var item in tr.Mesh.Triangles)
                {
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

                    toDel.Add(item);
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

                foreach (var ttt in tr.Mesh.Triangles)
                {
                    var vv1 = ttt.Vertices.Where(z => !pl.IsOnPlane(z.Position)).ToArray();
                    if (vv1.Length == 0)
                    {
                        toDel.Add(ttt);
                        continue;
                    }

                    if (pl.SideOfPlane(vv1[0].Position) < 0)
                    {
                        toDel.Add(ttt);
                    }
                }
                foreach (var item in toDel)
                {
                    tr.Mesh.Triangles.Remove(item);
                }
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

        public override void AppendToXml(StringBuilder sb)
        {

        }

        public bool DrawFill { get; set; } = true;
        public bool DrawWireframe { get; set; } = true;

        public Color Color { get; set; } = Color.Orange;
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
                GL.Begin(PrimitiveType.Triangles);

                foreach (var item in Mesh.Triangles)
                {
                    foreach (var vv in item.Vertices)
                    {
                        GL.Vertex3(vv.Position);
                    }
                }
                GL.End();
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
    }
}
