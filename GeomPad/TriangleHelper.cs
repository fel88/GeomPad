using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GeomPad
{
    public class TriangleHelper : HelperItem3D, IEditFieldsContainer
    {
        [EditField]
        public Vector3d V0;
        [EditField]
        public Vector3d V1;
        [EditField]
        public Vector3d V2;
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

        public override void AppendXml(StringBuilder sb)
        {
            
        }

        public override void Draw()
        {
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.LineLoop);
            foreach (var item in Verticies)
            {
                GL.Vertex3(item);
            }
            GL.End();
            GL.Color3(Color.Orange);
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


    }

    

}
