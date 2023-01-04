using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class SplineHelper : HelperItem
    {
        public SplineHelper() { }
        public SplineHelper(XElement item)
        {
            var deg = int.Parse(item.Attribute("degree").Value);
            Degree = deg;
            IsPeriodic = bool.Parse(item.Attribute("isPeriodic").Value);
            IsNonPeriodic = bool.Parse(item.Attribute("isNonPeriodic").Value);
            IsPolynomial = bool.Parse(item.Attribute("isPoly").Value);
            IsBSpline = bool.Parse(item.Attribute("isBspline").Value);

            var skip = new string[] { ";", Environment.NewLine, "\r", "\n" };
            Knots = item.Element("knots").Value.Split(skip, StringSplitOptions.RemoveEmptyEntries).Select(StaticHelpers.ParseDouble).ToList();
            Weights = item.Element("weights").Value.Split(skip, StringSplitOptions.RemoveEmptyEntries).Select(StaticHelpers.ParseDouble).ToList();
            Multiplicities = item.Element("multiplicities").Value.Split(skip, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            foreach (var ii in item.Element("poles").Elements())
            {
                var pos = ii.Value.Split(skip, StringSplitOptions.RemoveEmptyEntries).Select(StaticHelpers.ParseDouble).ToList();
                Poles.Add(new Vector3d(pos[0], pos[1], pos[2]));
            }

            if (item.Attribute("drawSize") != null)
                DrawSize = int.Parse(item.Attribute("drawSize").Value);
        }

        public int Degree;
        public List<double> Knots = new List<double>();
        public void SetKnots(double[] v)
        {
            Knots = v.ToList();
        }
        public void SetWeights(double[] v)
        {
            Weights = v.ToList();
        }
        public void SetMultiplicities(int[] v)
        {
            Multiplicities = v.ToList();
        }
        public void SetPoles(Vector3d[] v)
        {
            Poles = v.ToList();
        }
        public List<double> Weights = new List<double>();
        public List<int> Multiplicities = new List<int>();
        public List<Vector3d> Poles = new List<Vector3d>();

        public bool DrawPoles { get; set; } = false;
        public bool DrawPoints { get; set; } = false;
        public float DrawPointSize { get; set; } = 1;
        public double DrawSize { get; set; } = 1;
        public override void Draw(IDrawingContext gr)
        {
            if (!Visible) return;
            if (DrawPoles)
            {
                foreach (var item in Poles)
                {
                    DrawHelpers.DrawCross(item, DrawSize);
                }
            }
            var p = GetPoints();
            GL.Begin(PrimitiveType.LineStrip);
            foreach (var item in p)
            {
                GL.Vertex3(item);
            }
            GL.End();

            if (DrawPoints)
            {
                GL.PointSize(DrawPointSize);
                GL.Begin(PrimitiveType.Points);
                foreach (var item in p)
                {
                    GL.Vertex3(item);
                }
                GL.End();
                GL.PointSize(1f);
            }


        }
        public Vector3d[] CachedPoints;

        public bool IsPolynomial { get; set; }
        public bool IsPeriodic { get; set; }
        public bool IsBSpline { get; set; }
        public bool IsNonPeriodic { get; set; }
        public Vector3d[] GetPoints()
        {
            if (CachedPoints != null) return CachedPoints;

            NURBS n = new NURBS();
            n.IsBSpline = IsBSpline;
            double stepSize = 0.01;
            for (int i = 0; i < Poles.Count; i++)
            {
                var vv = Poles[i];
                n.WeightedPointSeries.Add(new RationalBSplinePoint(new Vector3d(vv.X, vv.Y, vv.Z), Weights[i]));
            }

            int deg = Degree;

            //if (n.WeightedPointSeries.Count <= 3) return null;

            List<double> knots = new List<double>();
            for (int i = 0; i < Multiplicities.Count; i++)
            {
                for (int j = 0; j < Multiplicities[i]; j++)
                {
                    knots.Add(Knots[i]);
                }
            }
            if (!IsNonPeriodic)
            {
                knots.Insert(0, 0);
                knots.Add(1);
            }
            var knt = knots.ToArray();
            var first = knt[deg];
            var last = knt[knt.Length - deg - 1];

            var wpnts = n.WeightedPointSeries.ToList();
            if (!IsNonPeriodic)
            {
                wpnts.Add(n.WeightedPointSeries.First());
            }
            stepSize *= Math.Abs(last - first);
            //first = edge.Param1;
            //last = edge.Param2;
            first = Knots.First();
            last = knots.Last();

            var pnts = n.BSplineCurve(wpnts.ToArray(), deg, knt, stepSize, first, last);
            double epsilon = 1e-3;
            /*if (!((pnts[0] - edge.Start).Length < epsilon || (pnts[pnts.Length - 1] - edge.Start).Length < epsilon))
            {
                throw new GeomPadException("wrong bspline points");
            }
            if (!((pnts[0] - edge.End).Length < epsilon || (pnts[pnts.Length - 1] - edge.End).Length < epsilon))
            {
                throw new GeomPadException("wrong bspline points");
            }*/
            CachedPoints = pnts;
            return pnts;
        }
    }
}

