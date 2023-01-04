using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace GeomPad.Helpers3D.BRep
{
    public abstract class AbstractBRepFaceHelper : HelperItem
    {
        public Mesh Mesh;
        public double DrawSize { get; set; } = 1;
        public bool ShowMesh { get; set; } = true;
        [EditField]
        public Vector3d Location;

        public ProjectPolygon[] ProjectPolygons;
        public List<Contour3d> Contours = new List<Contour3d>();

        public bool ShowGismos { get; set; } = true;
        protected void drawContours()
        {
            if (Mesh == null) return;

            GL.Disable(EnableCap.Lighting);

            foreach (var item in Contours)
            {
                GL.Begin(PrimitiveType.LineStrip);
                foreach (var pp in item.Points)
                {
                    GL.Vertex3(pp);
                }
                GL.End();
            }

        }
        protected void drawMesh()
        {
            if (Mesh == null) return;

            GL.Enable(EnableCap.Lighting);

            GL.Begin(PrimitiveType.Triangles);
            foreach (var item in Mesh.Triangles)
            {
                foreach (var v in item.Vertices)
                {
                    GL.Normal3(v.Normal);
                    GL.Vertex3(v.Position);
                }
            }
            GL.End();
            GL.Disable(EnableCap.Lighting);
        }

        public override void Draw(IDrawingContext gr)
        {

        }

        public abstract void UpdateMesh(ProjectPolygon[] polygons);

    }
}
