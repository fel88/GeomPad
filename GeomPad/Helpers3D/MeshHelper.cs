using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class MeshHelper : HelperItem, IEditFieldsContainer, ICommandsContainer
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

            public Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (z, arr, cc) =>
            {
                var tr = z as MeshHelper;
                var ray = arr.First(t => t is LineHelper) as LineHelper;

                MouseRay mr = new MouseRay(ray.Start, ray.End);
                var dd = Intersection.CheckIntersect(mr, tr.Mesh.Triangles.ToArray());

                if (dd != null)
                {
                    cc.AddHelper(new PointHelper() { Position = dd.Point });
                    cc.SetStatus("intersection found: " + dd.Point.ToString(), StatusTypeEnum.Information);
                }
                else
                    cc.SetStatus("intersection not found.", StatusTypeEnum.Warning);
            };
        }
        public override void AppendToXml(StringBuilder sb)
        {

        }
        public bool DrawFill { get; set; } = true;
        public Color Color = Color.Orange;
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);

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
    }
}
