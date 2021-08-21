using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace GeomPad.Helpers3D
{
    public class PointCloudHelper : HelperItem
    {
        public float DrawSize { get; set; } = 2;

        public PointIndexer Cloud;
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
