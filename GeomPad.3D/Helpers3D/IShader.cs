namespace GeomPad.Helpers3D
{
    public interface IShader
    {
        int GetProgramId();
        void SetUniformsData();
        void Init();
        void Use();
    }
}
