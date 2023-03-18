using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Reflection;

namespace GeomPad.Helpers3D
{
    public class FxShader : IShader
    {

        public void SetVec3(string nm, Vector3 v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.Uniform3(loc, v);
        }
        public void SetVec4(string nm, Vector4 v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.Uniform4(loc, v);
        }

        public void SetMatrix4(string nm, Matrix4 v)
        {
            var loc = GL.GetUniformLocation(shaderProgram, nm);
            GL.UniformMatrix4(loc, false, ref v);
        }

        public int GetProgramId()
        {
            return shaderProgram;
        }
        public int shaderProgram;

        public virtual void Init()
        {

        }

        public void Init(string nm1 = "vertexshader1", string nm2 = "fragmentshader1")
        {
            // build and compile our shader program
            // ------------------------------------
            // vertex shader
            var asm = Assembly.GetAssembly(typeof(Model3DrawShader));
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var vertexShaderSource = ResourceFile.GetFileText(nm1, asm);
            var fragmentShaderSource = ResourceFile.GetFileText(nm2, asm);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            // check for shader compile errors
            int success;
            string infoLog;

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(vertexShader, out infoLog);
                throw new Exception(infoLog);
                //std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
            }
            // fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            // check for shader compile errors
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(fragmentShader, out infoLog);
                throw new Exception(infoLog);
                // glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
                //std::cout << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << infoLog << std::endl;
            }
            // link shaders
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            // check for linking errors
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out success);
            //glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(shaderProgram, out infoLog);
                throw new Exception(infoLog);
                // glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
                //std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
            }
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);




        }

        public virtual void SetUniformsData()
        {

        }

        public void Use()
        {
            GL.UseProgram(shaderProgram);
        }
    }
}
