using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GeomPad
{
    public static class DrawHelpers
    {
        public static void DrawCross(Vector3d pos, double g, bool beginEnd = true, bool is3d = true)
        {
            if (beginEnd)
            {
                GL.Begin(PrimitiveType.Lines);
            }

            GL.Vertex3(pos.X, pos.Y - g, pos.Z);
            GL.Vertex3(pos.X, pos.Y + g, pos.Z);
            if (is3d)
            {
                GL.Vertex3(pos.X, pos.Y, pos.Z - g);
                GL.Vertex3(pos.X, pos.Y, pos.Z + g);
            }

            GL.Vertex3(pos.X + g, pos.Y, pos.Z);
            GL.Vertex3(pos.X - g, pos.Y, pos.Z);
            if (beginEnd)
            {
                GL.End();
            }
        }
    }
}
