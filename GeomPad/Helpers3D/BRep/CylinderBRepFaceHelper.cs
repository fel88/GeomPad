using GeomPad.Helpers3D.BRep;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeomPad.Helpers3D.BRep
{
    public class CylinderBRepFaceHelper : AbstractBRepFaceHelper, ICommandsContainer
    {

     
        [EditField]
        public double Radius = 10;

        [EditField]
        public double AngleStep = 15;
        [EditField]
        public Vector3d Axis = Vector3d.UnitZ;

        public double Lenght { get; set; } = 100;
     

        public ICommand[] Commands => new ICommand[] { new CylinderBRepHelperSwitchNormal(), new CylinderBRepHelperEditProjectionMap() };

        public class CylinderBRepHelperEditProjectionMap : ICommand
        {
            public string Name => "edit map";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as CylinderBRepFaceHelper;
                ProjectMapEditor pme = new ProjectMapEditor();
                pme.Init(tr);
                cc.Parent.OpenChildWindow(pme);
            };
        }
        public class CylinderBRepHelperSwitchNormal : ICommand
        {
            public string Name => "switch normal";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as CylinderBRepFaceHelper;
                if (tr.Mesh != null)
                    tr.Mesh.SwitchNormals();
            };
        }
        public override void UpdateMesh(ProjectPolygon[] p)
        {
            ProjectPolygons = p;

            PlaneSurface ps = new PlaneSurface() { Position = Location, Normal = Axis };
            var bs = ps.GetBasis();
            var vec0 = bs[0] * Radius;

            //stage1
            //check nesting
            List<ProjectPolygon> tops = new List<ProjectPolygon>();
            foreach (var item in ProjectPolygons)
            {
                bool good = true;
                foreach (var item2 in ProjectPolygons)
                {
                    if (item == item2) continue;
                    var pnts2 = item2.Points.ToArray();

                    if (GeometryUtils.pnpoly(pnts2, item.Points[0].X, item.Points[0].Y))
                    {
                        good = false; break;
                    }
                }
                if (good) tops.Add(item);
            }

            List<Vector2d[]> triangls = new List<Vector2d[]>();
            double step = AngleStep / 180f * Math.PI;

            //extract 3d contours
            Contours.Clear();
            foreach (var item in tops)
            {
                var cc = new Contour3d();
                Contours.Add(cc);
                var maxy = item.Points.Max(z => z.Y) + 1;
                var miny = item.Points.Min(z => z.Y) - 1;
                for (int i = 0; i < item.Points.Count; i++)
                {
                    var p0 = item.Points[i];
                    var p1 = item.Points[(i + 1) % item.Points.Count];
                    double last = 0;
                    List<Vector2d> cutPoints = new List<Vector2d>();
                    List<Vector2d> tempPoints = new List<Vector2d>();

                    cutPoints.Add(p0);

                    while (true)
                    {
                        var p00 = last;
                        var p11 = p00 + step;
                        last += step;

                        p00 = Math.Min(p00, 10 * Math.PI * 2);
                        p11 = Math.Min(p11, 10 * Math.PI * 2);

                        if (Math.Abs(p00 - p11) < 1e-8) break;
                        Vector2d ret1 = Vector2d.Zero;


                        if (GeometryUtils.IntersectSegments(p0, p1, new Vector2d(p11, miny), new Vector2d(p11, maxy), ref ret1))
                        {
                            tempPoints.Add(ret1);

                        }

                    }
                    tempPoints.Add(p1);
                    while (tempPoints.Any())
                    {
                        var fr = tempPoints.OrderBy(z => (z - cutPoints.Last()).Length).First();
                        cutPoints.Add(fr);
                        tempPoints.Remove(fr);
                    }
                    for (int j = 0; j < cutPoints.Count; j++)
                    {
                        var ang = cutPoints[j].X;
                        var mtr = Matrix4d.CreateFromAxisAngle(Axis, -ang);
                        var rot0 = Vector3d.Transform(vec0 + Axis * cutPoints[j].Y * Lenght, mtr);
                        cc.Points.Add(Location + rot0);
                    }
                }
            }

            foreach (var item in tops)
            {
                List<ProjectPolygon> holes = new List<ProjectPolygon>();
                var pnts2 = item.Points.ToArray();

                foreach (var xitem in ProjectPolygons.Except(tops))
                    if (GeometryUtils.pnpoly(pnts2, xitem.Points[0].X, xitem.Points[0].Y))
                        holes.Add(xitem);


                PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();
                PolyBoolCS.Polygon p1 = new PolyBoolCS.Polygon();
                var pl1 = new PolyBoolCS.PointList();
                p1.regions = new List<PolyBoolCS.PointList>();

                pl1.AddRange(item.Points.Select(z => new PolyBoolCS.Point(z.X, z.Y)).ToArray());
                p1.regions.Add(pl1);
                var maxy = pl1.Max(z => z.y) + 1;
                var miny = pl1.Min(z => z.y) - 1;
                double last = 0;
                while (true)
                //for (double i = step; i < (Math.PI * 2); i += step)
                {

                    var p0 = last;
                    var p11 = p0 + step;
                    last += step;

                    p0 = Math.Min(p0, 10 * Math.PI * 2);
                    p11 = Math.Min(p11, 10 * Math.PI * 2);

                    if (Math.Abs(p0 - p11) < 1e-8) break;


                    PolyBoolCS.Polygon p2 = new PolyBoolCS.Polygon();
                    p2.regions = new List<PolyBoolCS.PointList>();
                    var pl2 = new PolyBoolCS.PointList();

                    pl2.Add(new PolyBoolCS.Point(p0, miny));
                    pl2.Add(new PolyBoolCS.Point(p0, maxy));
                    pl2.Add(new PolyBoolCS.Point(p11, maxy));
                    pl2.Add(new PolyBoolCS.Point(p11, miny));


                    p2.regions.Add(pl2);

                    if (holes.Any(z => GeometryUtils.AlmostEqual(z.Area(), 0)))
                    {
                        throw new GeomPadException("zero area contour detected");
                    }
                    var res = pb.intersect(p1, p2);
                    if (res.regions.Any())
                    {
                        foreach (var region in res.regions)
                        {
                            var triangls2 = GeometryUtils.TriangulateWithHoles(
                                new[] { region.Select(z => new Vector2d(z.x, z.y)).ToArray() }
                                ,
                  holes.Select(z => z.Points.ToArray()).ToArray(), true);
                            triangls.AddRange(triangls2);
                        }
                    }
                }
            }

            //stage2
            List<TriangleInfo> tt = new List<TriangleInfo>();
            foreach (var item in triangls)
            {
                TriangleInfo tin = new TriangleInfo();
                List<VertexInfo> v = new List<VertexInfo>();
                foreach (var d in item)
                {
                    var ang = d.X;
                    var mtr = Matrix4d.CreateFromAxisAngle(Axis, -ang);

                    var rot0 = Vector3d.Transform(vec0 + Axis * d.Y * Lenght, mtr);
                    v.Add(new VertexInfo() { Position = Location + rot0 });
                }
                var v01 = v[1].Position - v[0].Position;
                var v11 = v[2].Position - v[0].Position;
                var crs = Vector3d.Cross(v01, v11).Normalized();
                if (double.IsNaN(crs.X)) throw new GeomPadException("normal is NaN");
                foreach (var item0 in v)
                {
                    item0.Normal = crs;
                }
                tin.Vertices = v.ToArray();

                tt.Add(tin);
            }
            Mesh = new Mesh() { Triangles = tt };
        }

        public override void Draw(IDrawingContext gr)
        {
            if (!Visible) return;
            GL.Color3(Color.Blue);
            if (Selected)
                GL.Color3(Color.Red);
            if (ShowGismos)
            {
                
                DrawHelpers.DrawCross(Location, DrawSize);
                PlaneSurface ps = new PlaneSurface() { Position = Location, Normal = Axis };
                var bs = ps.GetBasis();
                var dir = bs[0] * Radius;
                List<Vector3d> pnts = new List<Vector3d>();
                var step = Math.PI * AngleStep / 180f;
                for (double i = 0; i <= Math.PI * 2; i += step)
                {
                    var mtr4 = Matrix4d.CreateFromAxisAngle(Axis, i);
                    var res = Vector4d.Transform(new Vector4d(dir), mtr4);
                    pnts.Add(Location + res.Xyz);
                }

                GL.Begin(PrimitiveType.LineStrip);
                for (int i = 0; i < pnts.Count; i++)
                {
                    GL.Vertex3(pnts[i]);
                }
                GL.End();
                pnts.Clear();

                for (double i = 0; i <= Math.PI * 2; i += step)
                {
                    var mtr4 = Matrix4d.CreateFromAxisAngle(Axis, i);
                    var res = Vector4d.Transform(new Vector4d(dir), mtr4);
                    pnts.Add(Location + res.Xyz + Axis * Lenght);
                }

                GL.Begin(PrimitiveType.LineStrip);
                for (int i = 0; i < pnts.Count; i++)
                {
                    GL.Vertex3(pnts[i]);
                }
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(Location);
                GL.Vertex3(Location + Axis * Lenght);
                GL.End();
            }
            if (ShowMesh)
                drawMesh();
            drawContours();
        }
    }

    public class Contour3d
    {
        public List<Vector3d> Points = new List<Vector3d>();
    }
}
