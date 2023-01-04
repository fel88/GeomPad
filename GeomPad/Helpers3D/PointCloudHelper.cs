using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class PointCloudHelper : HelperItem
    {
        public PointCloudHelper() { }
        public PointCloudHelper(XElement item) : base(item)
        {
            Cloud = new PointIndexer();
            List<Vector3d> ret = new List<Vector3d>();
            foreach (var pitem in item.Elements("point"))
            {
                var pos = pitem.Attribute("pos").Value.Split(new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."),
                CultureInfo.InvariantCulture)).ToArray();
                ret.Add(new Vector3d(pos[0], pos[1], pos[2]));
            }
            Cloud.Points = ret.ToArray();
        }
        public float DrawSize { get; set; } = 2;

        public PointIndexer Cloud;

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<cloud name=\"{Name}\"  drawSize=\"{DrawSize}\">");
            foreach (var item in Cloud.Points)
            {
                sb.AppendLine($"<point pos=\"{item.X};{item.Y};{item.Z}\"/>");
            }
            sb.AppendLine($"</cloud>");

        }
        public override void Draw(IDrawingContext gr)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            var temp = GL.GetFloat(GetPName.PointSize);
            if (Selected)
            {
                GL.Color3(Color.Red);
            }
            GL.PointSize(DrawSize);
            GL.Begin(PrimitiveType.Points);
            foreach (var item in Cloud.Points)
            {
                GL.Vertex3(item);
            }
            GL.End();
            GL.PointSize(temp);
        }
    }
}