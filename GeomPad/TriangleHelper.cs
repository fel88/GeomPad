using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
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
            var lns = GetLines();
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in lns.Cast<Line3DHelper>())
            {
                var l3 = new Line3D() { Start = item.Start, End = item.End };
                var inter = Geometry.Intersect3dCrossedLines(ln, l3);
                if (inter != null) pp.Add(inter.Value);
            }
            return pp.Select(z => new Point3DHelper() { Position = z }).ToArray();

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


    public class TriangleHelperSplitByPlaneCommand : ICommand
    {
        public string Name => "split by plane";

        public Action<HelperItem3D, HelperItem3D[],I3DPadContainer> Process => (z, arr,cc) =>
        {
            var tr = z as TriangleHelper;
            var pl = arr.First(t => t is PlaneHelper) as PlaneHelper;
            var res = tr.SplitByPlane(pl);
            cc.AddHelpers(res);
        };
    }

}
