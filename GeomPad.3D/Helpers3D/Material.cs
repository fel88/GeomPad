using OpenTK;

namespace GeomPad.Helpers3D
{
    public class Material
    {
        public string Name { get; set; }
        public Vector3 AmbientColor = new Vector3();
        public Vector3 DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);
        public Vector3 SpecularColor = new Vector3();
        public float SpecularExponent = 1;
        public float Opacity = 1.0f;
    }
}
