namespace GeomPad.Helpers3D
{
    public class FaceItem
    {
        public TempVertex[] V = new TempVertex[3];
        public TempVertex Item1
        {
            get { return V[0]; }
        }
        public TempVertex Item2
        {
            get { return V[1]; }
        }

        public TempVertex Item3
        {
            get { return V[2]; }
        }
        public TempVertex Item4
        {
            get { return V[3]; }
        }
        public string Material;

    }
}
