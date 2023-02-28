using GeomPad.Common;
using GeomPad.Helpers3D;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public class HingeHelper : HelperItem, ICommandsContainer
    {
        Line3D edge = new Line3D();
        public ICommand[] Commands => new[] { new HingeHelperCalcOutterNormal() };

        [EditField]
        public Vector3d EdgePoint0 { get { return edge.Start; } set { edge.Start = value; recreatePlanes(); } }
        [EditField]
        public Vector3d EdgePoint1 { get { return edge.End; } set { edge.End = value; recreatePlanes(); } }
        Vector3d _auxPoint0;
        Vector3d _auxPoint1;
        [EditField]
        public Vector3d AuxPoint0 { get { return _auxPoint0; } set { _auxPoint0 = value; recreatePlanes(); } }
        [EditField]
        public Vector3d AuxPoint1 { get { return _auxPoint1; } set { _auxPoint1 = value; recreatePlanes(); } }



        public HingeHelper() { }
        public HingeHelper(XElement item)
        {

        }
        void recreatePlanes()
        {
            tring2.V0 = edge.Start;
            tring2.V1 = edge.End;
            tring2.V2 = AuxPoint1;

            normal1 = tring2.GetPlane().Normal;
        }
        [EditField]
        public float DrawSize { get; set; } = 1;


        LineHelper lineHelper = new LineHelper();
        LineHelper normalHelper1 = new LineHelper();
        LineHelper normalHelper2 = new LineHelper();
        TriangleHelper tring1 = new TriangleHelper();
        TriangleHelper tring2 = new TriangleHelper();

        public Vector3d Normal0 { get; set; }
        [EditField]
        public bool ShowNormals { get; set; }
        [EditField]
        public bool ShowCrosses { get; set; }
        [EditField]
        public bool FaceCulling { get; set; } = false;

        public Color Color { get; set; } = Color.LightGreen;
        Vector3d normal1;
        public override void Draw(IDrawingContext ctx)
        {
            if (!Visible) return;
            var cface = GL.GetBoolean(GetPName.CullFace);
            var cfaceMode = GL.GetInteger(GetPName.CullFaceMode);
            if (FaceCulling)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
            }
            tring1.Color = Color;
            tring2.Color = Color;
            
            tring1.V0 = edge.Start;
            tring1.V1 = edge.End;
            tring1.V2 = AuxPoint0;
            tring1.Draw(ctx);

            tring2.V0 = edge.Start;
            tring2.V1 = edge.End;
            tring2.V2 = AuxPoint1;
            tring2.Draw(ctx);

            lineHelper.Start = edge.Start;
            lineHelper.End = edge.End;
            lineHelper.ShowCrosses = ShowCrosses;
            lineHelper.Draw(ctx);

            var cnt = tring1.Center();
            var cnt2 = tring2.Center();

            Normal0 = tring1.GetPlane().Normal;

            normalHelper1.DrawSize = DrawSize;
            normalHelper2.DrawSize = DrawSize;
            normalHelper1.Start = cnt;
            normalHelper1.End = cnt + Normal0;
            normalHelper2.Start = cnt2;
            normalHelper2.End = cnt2 + normal1;
            if (ShowNormals)
            {
                normalHelper2.Draw(ctx);
                normalHelper1.Draw(ctx);
            }
            if (ShowCrosses)
            {
                DrawHelpers.DrawCross(AuxPoint0, DrawSize);
                DrawHelpers.DrawCross(AuxPoint1, DrawSize);
            }

            if (FaceCulling)
            {
                if (!cface)
                {
                    GL.Disable(EnableCap.CullFace);
                }
                GL.CullFace((CullFaceMode)cfaceMode);
            }
        }

        public bool Fill { get; set; }

        
        internal Vector3d CalcConjugateNormal()
        {
            var nm = tring1.GetPlane().Normal.Normalized();

            var point0 = AuxPoint0;
            var point1 = AuxPoint1;
            var neg = -nm;
            var axis = (edge.End - edge.Start).Normalized();
            PlaneSurface pp = new PlaneSurface() { Position = edge.Start, Normal = axis };
            var prj0 = pp.ProjPoint(point0) - edge.Start;
            var prj1 = pp.ProjPoint(point1) - edge.Start;

            var crs1 = Vector3d.Cross(prj0, prj1) / prj0.Length / prj1.Length;            
            var ang2 = Vector3d.CalculateAngle(prj0, prj1);
            if (crs1.Length > 1e-8)
            {
                axis = -crs1.Normalized();
            }
            var mtr = Matrix4d.CreateFromAxisAngle(axis, ang2);
            var mtr2 = Matrix4d.CreateFromAxisAngle(axis, -ang2);
            var trans = Vector3d.Transform(neg, mtr2);
            var check = Vector3d.Transform(prj0.Normalized(), mtr);
            var check2 = Vector3d.Transform(prj1.Normalized(), mtr);
            
            if (!(Vector3d.Cross(tring2.GetPlane().Normal, trans).Length < 1e-8 || Vector3d.Cross(tring2.GetPlane().Normal, -trans).Length < 1e-8))
            {
                DebugHelper.Error?.Invoke("inconsistent normal was calculated");
            }

            normal1 = trans;
            return trans;
        }
        public class HingeHelperCalcOutterNormal : ICommand
        {
            public string Name => "calc outter normal";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as HingeHelper;
                tr.CalcConjugateNormal();
            };
        }
    }  
}
