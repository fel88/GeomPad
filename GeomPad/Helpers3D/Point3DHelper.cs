using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;

namespace GeomPad.Helpers3D
{
    public class PointHelper : HelperItem
    {
        [EditField]
        public Vector3d Position;        

        public PointHelper() { }
        public PointHelper(XElement item)
        {
            var pos = item.Attribute("position").Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3d(pos[0], pos[1], pos[2]);
        }

        public int DrawSize { get; set; } = 2;
        public double X { get => Position.X; set => Position.X = value; }
        public double Y { get => Position.Y; set => Position.Y = value; }
        public double Z { get => Position.Z; set => Position.Z = value; }

        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<point position=\"{Position.X};{Position.Y};{Position.Z}\" drawSize=\"{DrawSize}\"/>");
        }
                
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            if (Selected) GL.Color3(Color.Red);
            DrawHelpers.DrawCross(Position, DrawSize);
        }
    }
}
