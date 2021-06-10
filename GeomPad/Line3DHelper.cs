using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad
{
    public class Line3DHelper : HelperItem3D, IEditFieldsContainer, ICommandsContainer
    {
        [EditField]
        public Vector3d Start;
        [EditField]
        public Vector3d End;


        public Line3DHelper() { }
        public Line3DHelper(XElement item)
        {
            var pos = item.Attribute("start").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Start = new Vector3d(pos[0], pos[1], pos[2]);
            var nrm = item.Attribute("end").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            End = new Vector3d(nrm[0], nrm[1], nrm[2]);
            if (item.Attribute("drawSize") != null)
                DrawSize = float.Parse(item.Attribute("drawSize").Value.Replace(",", "."));
        }

        public float DrawSize { get; set; } = 2;

        public ICommand[] Commands => new ICommand[] { new Line3DExpandAlongCommand(), new Line3DSwitchStartEndCommand() };
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public double Length
        {
            get
            {
                return (End - Start).Length;
            }
            set
            {
                End = Start + Dir * value;
            }
        }

        public override void Draw()
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            if (Selected)
                GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(Start);
            GL.Vertex3(End);
            GL.End();

            DrawHelpers.DrawCross(Start, DrawSize);
            DrawHelpers.DrawCross(End, DrawSize);

        }

        public override void AppendXml(StringBuilder sb)
        {
            sb.AppendLine($"<line start=\"{Start.X};{Start.Y};{Start.Z}\" end=\"{End.X};{End.Y};{End.Z}\" drawSize=\"{DrawSize}\"/>");
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

    public class Line3DExpandAlongCommand : ICommand
    {
        public string Name => "expand along direction";

        public Action<HelperItem3D, HelperItem3D[], I3DPadContainer> Process => (z, arr, cc) =>
          {
              var ln = (z as Line3DHelper);
              ln.Start += -ln.Dir * 100;
              ln.End += ln.Dir * 100;
          };
    }
    public class Line3DSwitchStartEndCommand : ICommand
    {
        public string Name => "switch start and end";

        public Action<HelperItem3D, HelperItem3D[], I3DPadContainer> Process => (z, arr, cc) =>
          {
              var ln = (z as Line3DHelper);
              var t = ln.Start;
              ln.Start = ln.End;
              ln.End = t;
          };
    }
}
