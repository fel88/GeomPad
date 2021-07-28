using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class EllipseHelper : HelperItem, ICommandsContainer
    {
        public EllipseHelper()
        { }
        public EllipseHelper(XElement item)
        {

        }

        [EditField]
        public double MajorRadius { get; set; } = 10;
        [EditField]
        public double MinorRadius { get; set; } = 5;
        [EditField]
        public Vector3d Location { get; set; }
        [EditField]
        public Vector3d Normal { get; set; }
        [EditField]
        public double SweepAngle { get; set; }
        [EditField]

        public Vector3d AuxPoint { get; set; }//start contour point
        [EditField]
        public bool FullEllipse { get; set; } = true;
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<ellipse full=\"{FullEllipse}\" sweep=\"{SweepAngle}\" major=\"{MajorRadius}\" minor=\"{MinorRadius}\">");
            sb.AppendLine($"<location pos=\"{Location.X};{Location.Y};{Location.Z}\"/>");
            sb.AppendLine($"<normal pos=\"{Normal.X};{Normal.Y};{Normal.Z}\"/>");
            sb.AppendLine($"<aux pos=\"{AuxPoint.X};{AuxPoint.Y};{AuxPoint.Z}\"/>");
            sb.AppendLine($"</ellipse>");
        }
        [EditField]
        public int DrawSize { get; set; } = 2;

        public ICommand[] Commands => new ICommand[] { new EllipseFixAuxPointCommand() };
        public class EllipseFixAuxPointCommand : ICommand
        {
            public string Name => "fix aux point";

            public Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process => (z, arr, cc) =>
            {
                var ln = (z as EllipseHelper);
                PlaneSurface ps = new PlaneSurface() { Normal = ln.Normal, Position = ln.Location };
                var proj = ps.ProjPoint(ln.AuxPoint);
                ln.AuxPoint = proj;
            };
        }
        private Vector3d norm;

        Vector3d point(double ang)
        {
            var mtr4 = Matrix4d.CreateFromAxisAngle(Normal, ang);
            var rad = MajorRadius * MinorRadius / (Math.Sqrt(Math.Pow(MajorRadius * Math.Sin(ang), 2) + Math.Pow(MinorRadius * Math.Cos(ang), 2)));
            var res = Vector4d.Transform(new Vector4d(norm * rad), mtr4);
            return (Location + res.Xyz);
        }
        public override void Draw(IDrawingContext ctx)
        {
            DrawHelpers.DrawCross(Location, DrawSize);
            DrawHelpers.DrawCross(AuxPoint, DrawSize);
            GL.Begin(PrimitiveType.Lines);
            var pnts = GetPointsD();

            for (int i = 1; i < pnts.Length; i++)
            {
                var pp0 = pnts[i - 1];
                var pp1 = pnts[i];
                GL.Vertex3(pp0);
                GL.Vertex3(pp1);
            }
            GL.End();
        }

        internal Vector3d[] GetPointsD()
        {
            PlaneSurface ps = new PlaneSurface() { Normal = Normal, Position = Location };
            if (!ps.IsOnPlane(AuxPoint)) return new Vector3d[] { };
            var dir = AuxPoint - Location;
            norm = dir.Normalized();

            List<Vector3d> pnts = new List<Vector3d>();
            var step = Math.PI * 5 / 180f;

            double sweepAngle = SweepAngle;
            if (FullEllipse)
            {
                sweepAngle = Math.PI * 2;
            }
            for (double i = 0; i < sweepAngle; i += step)
            {
                pnts.Add(point(i));
            }
            pnts.Add(point(sweepAngle));

            return pnts.ToArray();
        }
    }
}
