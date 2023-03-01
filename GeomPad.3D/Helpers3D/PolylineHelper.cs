using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class PolylineHelper : HelperItem, IEditFieldsContainer, IFitAllable
    {
        public List<Vector3d> Verticies = new List<Vector3d>();
        public PolylineHelper() { }
        public PolylineHelper(XElement item)
        {
            Verticies.Clear();
            foreach (var vitem in item.Elements("vertex"))
            {
                var pos = vitem.Attribute("pos").Value.Split(new char[] { ';' },
                    System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."),
                    CultureInfo.InvariantCulture)).ToArray();
                Verticies.Add(new Vector3d(pos[0], pos[1], pos[2]));
            }
        }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<polyline>");
            foreach (var item in Verticies)
            {
                sb.AppendLine($"<vertex pos=\"{item.X};{item.Y};{item.Z}\"/>");
            }

            sb.AppendLine($"</polyline>");
        }

        public bool DrawCubes { get; set; } = false;
        public float DrawCubeSize { get; set; } = 0.03f;
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible)
                return;

            //if (Verticies.Count < 3) return;

            GL.Color3(Color.Blue);
            if (Selected)
            {
                GL.Color3(Color.Red);
            }
            GL.Begin(PrimitiveType.LineLoop);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();
            GL.Color3(Color.Orange);

            if (DrawCubes)
                foreach (var item in Verticies)
                {
                    DrawHelpers.DrawCube(item, DrawCubeSize);
                }

            /*GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();*/
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            return Verticies;
        }

        internal HelperItem[] Triangulate()
        {
            return null;
        }
    }

}
