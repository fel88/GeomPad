using GeomPad.Helpers3D;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeomPad
{
    public class ObjLoader : IMeshLoader
    {
        public MeshHelper Load(string path)
        {
            MeshHelper mh = new MeshHelper();
            var ln = File.ReadAllLines(path).Where(z => !z.Trim().StartsWith("#")).ToArray();
            List<Vector3d> vv = new List<Vector3d>();
            foreach (var l in ln)
            {
                if (l.StartsWith("v"))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vv.Add(new Vector3d() { X = spl[0], Y = spl[1], Z = spl[2] });
                }
                else if (l.StartsWith("f"))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse).ToArray();
                    TriangleInfo t = new TriangleInfo();
                    t.Vertices = new VertexInfo[spl.Length];
                    for (int k = 0; k < spl.Length; k++)
                    {
                        int zz = spl[k] - 1;
                        t.Vertices[k] = new VertexInfo() { Position = vv[zz] };
                    }
                    mh.Mesh.Triangles.Add(t);
                }
            }
            return mh;
        }
    }
}
