namespace GeomPad.Helpers3D
{
    public class TempVertex
    {
        public int Vertex;
        public int Normal;
        public int Texcoord;

        public TempVertex(int vert = 0, int norm = 0, int tex = 0)
        {
            Vertex = vert;
            Normal = norm;
            Texcoord = tex;
        }
    }
}
