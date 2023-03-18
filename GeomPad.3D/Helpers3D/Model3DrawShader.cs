using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GeomPad.Helpers3D
{
    public class Model3DrawShader : FxShader
    {
        string Vs;
        string Fs;
        public Model3DrawShader(string vs, string fs)
        {
            Fs = fs;
            Vs = vs;
        }
        public static int ShaderProgram;
        public int shaderProg
        {
            get
            {
                return shaderProgram;
            }
        }

        public override void Init()
        {
            Init(Vs, Fs);
            ShaderProgram = shaderProgram;
        }

        public Vector3 lightColor = new Vector3(1, 1, 1);

        public Vector3 lightPos = new Vector3(-100.0f, -100.0f, 100.0f);
        public Vector3 lightDir = new Vector3(-100.0f, -100.0f, 100.0f);
        public Vector3 viewPos;
        public Matrix4 Model;

        public override void SetUniformsData()
        {
            SetVec3("lightColor", lightColor);
            SetVec3("lightPos", lightPos);
            SetVec3("lightDirection", lightDir.Normalized());
            SetVec3("viewPos", viewPos);
            SetMatrix4("model", Model);
            int unif_mult;

            var unif_name = "mult";
            unif_mult = GL.GetUniformLocation(shaderProgram, unif_name);
            if (unif_mult == -1)
            {

            }

            GL.Uniform1(unif_mult, ColorMultipler);

            unif_name = "opacity";
            unif_mult = GL.GetUniformLocation(shaderProgram, unif_name);
            if (unif_mult == -1)
            {

            }

            GL.Uniform1(unif_mult, Opacity);
        }
        public float ColorMultipler = 1;
        public float Opacity = 1.0f;


    }
}
