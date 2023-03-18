using OpenTK;

namespace GeomPad.Helpers3D
{
    public class FaceVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoord;

        public TempVertex Temp;
        public FaceVertex()
        {

        }

        public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texcoord, TempVertex temp = null)
        {
            Position = pos;
            Normal = norm;
            TextureCoord = texcoord;
            Temp = temp;
        }
    }
}
