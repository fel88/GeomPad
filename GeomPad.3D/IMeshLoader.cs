using GeomPad.Helpers3D;

namespace GeomPad
{
    public interface IMeshLoader
    {
        MeshHelper Load(string path);
    }
}
