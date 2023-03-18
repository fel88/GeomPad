using GeomPad.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

namespace GeomPad.Helpers3D
{
    public class MeshGpuHelper : HelperItem, ICommandsContainer, IPointsProvider
    {
        public static FxShader ModelDrawShader = new Model3DrawShader("model3.vs", "model3.fs");
        VaoModel vmod;

        List<ObjVolume> models = new List<ObjVolume>();

        public Camera Camera;
        MeshHelper mesh;
        public MeshGpuHelper(MeshHelper model)
        {
            mesh = model;
            models.Clear();

            ObjVolume vol = new ObjVolume();
            Material mat = new Material();
            foreach (var item in model.Mesh.Triangles)
            {
                FaceItem2 f = new FaceItem2();
                f.Material = mat;
                f.Vertexes = new FaceVertex[3];
                for (int i = 0; i < 3; i++)
                {
                    f.Vertexes[i] = new FaceVertex();
                }
                f.Item1.Position = item.Vertices[0].Position.ToVector3();
                f.Item2.Position = item.Vertices[1].Position.ToVector3();
                f.Item3.Position = item.Vertices[2].Position.ToVector3();

                f.Item1.Normal = item.Vertices[0].Normal.ToVector3();
                f.Item2.Normal = item.Vertices[1].Normal.ToVector3();
                f.Item3.Normal = item.Vertices[2].Normal.ToVector3();

                vol.faces.Add(f);
            }
            models.Add(vol);

            vmod = new VaoModel();
            ModelDrawShader.Init("model.vs", "model.fs");
            vmod.ModelInit(models.ToArray());
        }
        public ICommand[] Commands => new ICommand[] {

            new SliceByPlaneCommand()
        };
        public class SliceByPlaneCommand : ICommand
        {
            public string Name => "slice by plane";

            public Action<ICommandContext> Process => (cc) =>
            {
                var tr = cc.Source as MeshGpuHelper;
                var pl = cc.Operands.First(t => t is PlaneHelper) as PlaneHelper;

                var rr = tr.SliceByPlane(pl);
                cc.Parent.AddHelpers(rr);
            };
        }

        private IHelperItem[] SliceByPlane(PlaneHelper pl)
        {
            return mesh.SliceByPlane(pl);
        }

        public IEnumerable<Vector3d> GetPoints()
        {
            return mesh.GetPoints();
        }
        internal void LoadFromFile(string fileName)
        {
            models.Clear();

            //models.AddRange(ObjVolume.LoadFromFile(fileName, Matrix4.Identity));

            var vol = models.Last();
            vmod = new VaoModel();
            ModelDrawShader.Init("model.vs", "model.fs");
            vmod.ModelInit(models.ToArray());
        }

        public override void Draw(IDrawingContext gr)
        {
            GL.PushMatrix();
            //GL.Scale(new Vector3d(Scale, Scale, Scale));
            var sh3 = ModelDrawShader as Model3DrawShader;
            sh3.lightDir = -Camera.Dir.ToVector3();
            sh3.viewPos = Camera.CamFrom.ToVector3();
            var rotation = 0;
            
            sh3.Model = Matrix4.CreateRotationZ((float)(rotation * Math.PI / 180f));
            if (vmod != null)
                vmod.DrawVao(ModelDrawShader);

            GL.PopMatrix();
        }
    }
}
