namespace GeomPad.Helpers3D
{
    public class FaceItem2
    {
        public FaceItem ParentFace;
        public TextureDescriptor Texture { get; set; }
        public ObjVolume Parent { get; set; }
        public FaceVertex[] Vertexes;
        public Material Material { get; set; }

        public FaceVertex Item1
        {
            get { return Vertexes[0]; }
        }
        public FaceVertex Item2
        {
            get { return Vertexes[1]; }
        }
        public FaceVertex Item3
        {
            get { return Vertexes[2]; }
        }
        public FaceVertex Item4
        {
            get { return Vertexes[3]; }
        }
    }
}
