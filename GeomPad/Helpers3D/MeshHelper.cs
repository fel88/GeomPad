using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public ICommand[] Commands => new[] { new MeshHelperSplitByRayCommand() };

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

        public Color Color = Color.Orange;
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
