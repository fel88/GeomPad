using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace GeomPad.Helpers3D
{
    public class VectorHelper : HelperItem, IFitAllable
    {
        [EditField]
        public Vector3d Start;
        [EditField]
        public Vector3d Dir;

        public bool ShowCrosses { get; set; } = true;

        public double Length { get; set; }
        public float CrossDrawSize { get; set; } = 2;
        public float PointDrawSize { get; set; } = 10;

        public Color Color { get; set; } = Color.Blue;
        public override void Draw(IDrawingContext gr)
        {
            if (!Visible)
                return;

            GL.Color3(Color);

            if (Selected)
                GL.Color3(Color.Red);

            GL.Begin(PrimitiveType.Lines);
            var end = Start + Dir.Normalized() * Length;
            GL.Vertex3(Start);
            GL.Vertex3(end);
            GL.End();

            if (ShowCrosses)
            {
                DrawHelpers.DrawCross(Start, CrossDrawSize);
            }
            GL.PointSize(PointDrawSize);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex3(end);
            GL.End();
        }

        public override AbstractHelperItem Clone()
        {
            VectorHelper h = new VectorHelper();
            h.Start = Start;
            h.ShowCrosses = ShowCrosses;
            h.CrossDrawSize = CrossDrawSize;
            h.PointDrawSize= PointDrawSize;
            h.Dir = Dir;
            h.Length = Length;

            return h;
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            return new[] { Start, Start+Dir.Normalized()*Length };
        }
    }
}
