using GeomPad.Helpers3D;
using OpenTK;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeomPad
{
    public class ObjLoader : IMeshLoader
    {
        public MeshHelper[] Load(string path)
        {
            List<MeshHelper> ret = new List<MeshHelper>();
            MeshHelper mh = new MeshHelper();
            ret.Add(mh);
            mh.Name = Path.GetFileNameWithoutExtension(path);
            var ln = File.ReadAllLines(path).Where(z => !z.Trim().StartsWith("#")).ToArray();
            List<Vector3d> vv = new List<Vector3d>();
            List<Vector3d> vn = new List<Vector3d>();
            foreach (var l in ln)
            {
                if (l.StartsWith("o "))
                {
                    if (mh.Mesh.Triangles.Count == 0)
                    {
                        ret.Remove(mh);
                    }
                    mh = new MeshHelper();
                    ret.Add(mh);
                    //vv = new List<Vector3d>();
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
                    mh.Name = spl[0];
                }
                else
                    if (l.StartsWith("v "))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vv.Add(new Vector3d() { X = spl[0], Y = spl[1], Z = spl[2] });
                }
                else
                    if (l.StartsWith("vn "))
                {
                    var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(StaticHelpers.ParseDouble).ToArray();
                    vn.Add(new Vector3d() { X = spl[0], Y = spl[1], Z = spl[2] });
                }
                else if (l.StartsWith("f "))
                {
                    TriangleInfo t = new TriangleInfo();
                    if (l.Contains("//"))// vert//normal
                    {

                    }
                    else if (l.Contains("/"))//vert/vtext/normal
                    {
                        mh.DrawWireframe = false;
                        mh.FlatShading = false;
                        var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[0]);
                        }).ToArray();
                        var spln = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(zz =>
                        {
                            return int.Parse(zz.Split('/')[2]);
                        }).ToArray();
                        t.Vertices = new VertexInfo[spl.Length];
                        for (int k = 0; k < spl.Length; k++)
                        {
                            int zz = spl[k] - 1;
                            t.Vertices[k] = new VertexInfo() { Position = vv[zz], Normal = vn[spln[k] - 1] };
                        }
                    }
                    else
                    {
                        var spl = l.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(int.Parse).ToArray();

                        t.Vertices = new VertexInfo[spl.Length];
                        for (int k = 0; k < spl.Length; k++)
                        {
                            int zz = spl[k] - 1;
                            t.Vertices[k] = new VertexInfo() { Position = vv[zz] };
                        }
                    }
                    mh.Mesh.Triangles.Add(t);
                }
            }
            return ret.ToArray();
        }
    }
}
