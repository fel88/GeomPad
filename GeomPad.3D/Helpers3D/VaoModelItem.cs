using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad.Helpers3D
{
    public class VaoModelItem
    {
        public int tcount;
        public int VAO, VBO;

        Material Material;
        MaterialStuff MatStuff;

        public VaoModelItem(Material item, FaceItem2[] faceItem2, MaterialStuff ms)
        {
            MatStuff = ms;

            this.Material = item;
            //tcount = md.getVertices().Count() / 3;

            float[] vres = new float[faceItem2.Length * 3 * 11];
            tcount = 0;


            long index = 0;
            foreach (var objVolumeFace in faceItem2)
            {
                for (int i = 0; i < 3; i++)
                {
                    var pos = objVolumeFace.Vertexes[i].Position;
                    var txt = objVolumeFace.Vertexes[i].TextureCoord;
                    var nrm = objVolumeFace.Vertexes[i].Normal;
                    var clr = objVolumeFace.Material.DiffuseColor;
                    vres[index++] = pos.X; vres[index++] = pos.Y; vres[index++] = pos.Z;
                    vres[index++] = txt.X; vres[index++] = txt.Y;
                    vres[index++] = nrm.X; vres[index++] = nrm.Y; vres[index++] = nrm.Z;
                    vres[index++] = clr.X; vres[index++] = clr.Y; vres[index++] = clr.Z;
                    tcount++;
                }
            }

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vres.Length, vres, BufferUsageHint.StaticDraw);

            int cnt = 11;
            GL.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                cnt * sizeof(float),
                0
            );
            GL.EnableVertexAttribArray(0);




            GL.VertexAttribPointer(
                1,
                2,
                VertexAttribPointerType.Float,
                false,
                cnt * sizeof(float),
                3 * sizeof(float)
            );
            GL.EnableVertexAttribArray(1);


            GL.VertexAttribPointer(
                2,
                3,
                VertexAttribPointerType.Float,
                false,
                cnt * sizeof(float),
                5 * sizeof(float)
            );
            GL.EnableVertexAttribArray(2);


            GL.VertexAttribPointer(
                3,
                3,
                VertexAttribPointerType.Float,
                false,
                cnt * sizeof(float),
                8 * sizeof(float)
            );
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);
        }

        public void Draw(int shaderProgram)
        {
            GL.PushMatrix();
            var slocation = GL.GetUniformLocation(shaderProgram, "diffuseMap");

            //var fr = this;
            var mater = Material;

            bool istexture = false;
            if (mater != null)
            {

                //GL.Color3(mater.DiffuseColor);
                //if (MatStuff.textures.ContainsKey(mater.AmbientMap))
                //{
                //    GL.Uniform1(slocation, 0);
                //    GL.ActiveTexture(TextureUnit.Texture0);
                //    //GL.BindTexture(TextureTarget.Texture2D, tid);

                //    GL.BindTexture(TextureTarget.Texture2D, MatStuff.textures[mater.AmbientMap]);
                //    istexture = true;
                //}
                //if (MatStuff.textures.ContainsKey(mater.DiffuseMap))
                //{
                //    GL.Uniform1(slocation, 0);
                //    GL.ActiveTexture(TextureUnit.Texture0);
                //    //GL.BindTexture(TextureTarget.Texture2D, tid);

                //    GL.BindTexture(TextureTarget.Texture2D, MatStuff.textures[mater.DiffuseMap]);
                //    istexture = true;
                //}
            }


            var uc = GL.GetUniformLocation(shaderProgram, "useColors");

            if (istexture)
            {
                GL.Uniform1(uc, 0);
            }
            else
            {
                GL.Uniform1(uc, 1);
            }

            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, tcount);


            GL.PopMatrix();
        }
    }
}
