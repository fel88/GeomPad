using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class EllipseHelper : HelperItem, ICommandsContainer
    {
        public EllipseHelper()
        { }


        public EllipseHelper(XElement item) : base(item)
        {
            ShowAux = parseBool(ShowAux, item, "showAux");
            ShowLocation = parseBool(ShowLocation, item, "showLocation");
            FullEllipse = parseBool(FullEllipse, item, "full");
            SweepAngle = parseDouble(SweepAngle, item, "sweep");
            MinorRadius = parseDouble(MinorRadius, item, "minor");
            MajorRadius = parseDouble(MajorRadius, item, "major");

            DrawSize = parseDouble(DrawSize, item, "drawSize");
            Normal = parseVector(Normal, item, "normal");
            Location = parseVector(Location, item, "location");
            AuxPoint = parseVector(AuxPoint, item, "aux");

            RefDir = parseVector(RefDir, item, "refDir");

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
        public Vector3d RefDir { get; set; }


        public Vector3d EndPoint
        {
            get
            {
                return GetPointsD().Last();
            }
        }
        public Vector3d StartPoint
        {
            get
            {
                return GetPointsD().First();
            }
        }
        [EditField]
        public bool FullEllipse { get; set; } = true;
        public override void AppendToXml(StringBuilder sb)
        {
            sb.AppendLine($"<ellipse name=\"{Name}\" showLocation=\"{ShowLocation}\" showAux=\"{ShowAux}\" drawSize=\"{DrawSize}\" full=\"{FullEllipse}\" sweep=\"{SweepAngle}\" major=\"{MajorRadius}\" minor=\"{MinorRadius}\">");
            sb.AppendLine($"<location pos=\"{Location.X};{Location.Y};{Location.Z}\"/>");
            sb.AppendLine($"<normal pos=\"{Normal.X};{Normal.Y};{Normal.Z}\"/>");
            sb.AppendLine($"<aux pos=\"{AuxPoint.X};{AuxPoint.Y};{AuxPoint.Z}\"/>");
            sb.AppendLine($"<refDir pos=\"{RefDir.X};{RefDir.Y};{RefDir.Z}\"/>");
            sb.AppendLine($"</ellipse>");
        }
        [EditField]
        public double DrawSize { get; set; } = 2;

        public ICommand[] Commands => new ICommand[] { new EllipseFixAuxPointCommand() };
        public class EllipseFixAuxPointCommand : ICommand
        {
            public string Name => "fix aux point";

            public Action<ICommandContext> Process => (cc) =>
            {
                var ln = cc.Source as EllipseHelper;
                PlaneSurface ps = new PlaneSurface() { Normal = ln.Normal, Position = ln.Location };
                var proj = ps.ProjPoint(ln.AuxPoint);
                ln.AuxPoint = proj;
            };
        }
        private Vector3d norm;

        Vector3d point(double ang)
        {
            var maxr = Math.Max(MajorRadius, MinorRadius);
            var minr = Math.Min(MajorRadius, MinorRadius);
            MajorRadius = maxr;
            MinorRadius = minr;
            
            var mtr4 = Matrix4d.CreateFromAxisAngle(Normal, ang);
            var res = Vector4d.Transform(new Vector4d(norm), mtr4);
            var realAng = Vector3d.CalculateAngle(res.Xyz, RefDir);
            var rad = MajorRadius * MinorRadius / (Math.Sqrt(Math.Pow(MajorRadius * Math.Sin(realAng), 2) + Math.Pow(MinorRadius * Math.Cos(realAng), 2)));
            res *= rad;
            return (Location + res.Xyz);
        }
        public bool ShowLocation { get; set; } = true;
        public bool ShowAux { get; set; } = true;
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            if (ShowLocation) DrawHelpers.DrawCross(Location, DrawSize);
            if (ShowAux) DrawHelpers.DrawCross(AuxPoint, DrawSize);

            GL.Color3(Color.Blue);
            if (Selected)
                GL.Color3(Color.Red);
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
