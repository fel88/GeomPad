using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GeomPad
{
    public class PolygonleHelper : HelperItem3D, IEditFieldsContainer, ICommandsContainer
    {

        public List<Vector3d> Verticies = new List<Vector3d>();


        public ICommand[] Commands => new[] { new TriangleHelperSplitByPlaneCommand() };

        public override void AppendXml(StringBuilder sb)
        {
            sb.AppendLine($"<triangle>");
            foreach (var item in Verticies)
            {
                sb.AppendLine($"<vertex pos=\"{item.X};{item.Y};{item.Z}\">");
            }

            sb.AppendLine($"</triangle>");
        }

        public override void Draw()
        {
            if (!Visible) return;
            if (Verticies.Count < 3) return;
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
           
            /*GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();*/
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

        internal HelperItem3D[] Triangulate()
        {
            return null;
        }
    }

}
