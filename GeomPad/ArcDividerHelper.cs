using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GeomPad
{
    public class ArcDividerHelper : HelperItem3D, IEditFieldsContainer
    {
        public override void AppendXml(StringBuilder sb)
        {

        }

        [EditField]
        public Vector3d Start;

        [EditField]
        public Vector3d Center;


        public Vector3d Axis
        {
            get
            {
                var v0 = Start - Center;
                var v1 = Aux - Center;
                return Vector3d.Cross(v0, v1).Normalized();
            }
        }
        [EditField]

        public Vector3d Aux;

        public Vector3d End
        {
            get
            {
                var ang = SweepAng * Math.PI / 180f;
                var mtr = Matrix4d.CreateFromAxisAngle(Axis.Normalized(), ang);
                var d = (Start - Center);
                var trn = Vector3d.Transform(d, mtr);
                return trn + Center;
            }
        }
        public Vector3d Value
        {
            get
            {
                var ang = (SweepAng / 2) * Math.PI / 180f;
                var mtr = Matrix4d.CreateFromAxisAngle(Axis.Normalized(), ang);
                var d = (Start - Center);
                var trn = Vector3d.Transform(d, mtr);
                return trn + Center;
            }
        }

        int _divider = 4;
        public int Divider
        {
            get
            {
                if (_divider <= 0) _divider = 1;
                return _divider;
            }
            set
            {
                _divider = value;
            }
        } 
        public double SweepAng { get; set; }

        public Vector3[] GetPoints()
        {
            return RecDividePoint(0, Divider, Start.ToVector3(), Value.ToVector3(), End.ToVector3(), Center.ToVector3()).ToArray();
        }
        public Vector3d[] GetPointsD()
        {
            return GetPoints().Select(z => z.ToVector3d()).ToArray();
        }

        public override void Draw()
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.LineStrip);
            var res = GetPoints();
            foreach (var item in res)
            {
                GL.Color3(Color.Blue);
                GL.Vertex3(item);
            }

            GL.End();

            GL.Color3(Color.Yellow);
            foreach (var item in res)
            {
                DrawHelpers.DrawCross(item.ToVector3d(), DrawSize / 2);
            }
            GL.Color3(Color.Blue);

            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Center);
            GL.Vertex3(Center + Axis * 5);
            GL.End();

            DrawHelpers.DrawCross(Start, DrawSize);
            DrawHelpers.DrawCross(End, DrawSize);
            DrawHelpers.DrawCross(Center, DrawSize);
            DrawHelpers.DrawCross(Value, DrawSize);
        }
        public static List<Vector3> RecDividePoint(int level, int maxLevel, Vector3 start, Vector3 middle, Vector3 end, Vector3 location, List<Vector3> res = null)
        {
            if (level >= maxLevel) { res.Add(middle); return res; }
            if (res == null)
            {
                res = new List<Vector3>();
            }
            res.Add(start);
            var p1 = DividePoint(start, location, middle);
            var p2 = DividePoint(middle, location, end);
            RecDividePoint(level + 1, maxLevel, start, p1, middle, location, res);
            res.Add(middle);
            RecDividePoint(level + 1, maxLevel, middle, p2, end, location, res);
            res.Add(end);

            return res;
        }

        public static Vector3 DividePoint(Vector3 start, Vector3 location, Vector3 middle)
        {
            var vv1 = start - location;
            var vv2 = middle - location;
            var cross1 = Vector3.Cross(vv1, vv2);
            var naxis = cross1.Normalized();
            var dot = Vector3.Dot(vv1, vv2);
            dot /= vv1.Length;
            dot /= vv2.Length;
            //var ang1 = (float)Math.Asin(cross1.Length / vv1.Length / vv2.Length);
            var ang1 = (float)Math.Acos(dot);
            var add1 = ang1 / 2;


            var mm1 = Matrix4.CreateFromAxisAngle(naxis, add1);
            var newp = new Vector4(start - location) * mm1;
            return newp.Xyz + location;
        }

        public int DrawSize { get; set; } = 2;

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
