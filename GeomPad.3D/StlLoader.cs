using GeomPad.Helpers3D;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeomPad
{
    public class StlLoader : IMeshLoader
    {
        public MeshHelper[] Load(string path)
        {
            List<MeshHelper> ret = new List<MeshHelper>();
            MeshHelper mm = new MeshHelper() { Name = Path.GetFileNameWithoutExtension(path) };
            ret.Add(mm);

            var txt = File.ReadLines(path);
            if (txt.First().StartsWith("solid"))
            {
                //text format
                TriangleInfo tr = null;
                Vector3d normal = Vector3d.Zero;

                List<VertexInfo> verts = new List<VertexInfo>();
                foreach (var item in txt)
                {
                    var line = item.Trim().ToLower();
                    if (line.StartsWith("facet"))
                    {
                        var spl = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(2).Select(z => StaticHelpers.ParseDouble(z)).ToArray();
                        normal = new Vector3d(db[0], db[1], db[2]);
                        tr = new TriangleInfo();
                        mm.Mesh.Triangles.Add(tr);
                    }
                    if (line.StartsWith("endfacet"))
                    {
                        tr.Vertices = verts.ToArray();
                        verts.Clear();
                    }
                    if (line.StartsWith("vertex"))
                    {
                        var spl = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                        var db = spl.Skip(1).Select(z => StaticHelpers.ParseDouble(z)).ToArray();
                        verts.Add(new VertexInfo()
                        {
                            Normal = normal,
                            Position = new Vector3d(db[0], db[1], db[2])
                        });

                    }
                }
            }
            else
            {
                using (var rdr = File.OpenRead(path))
                {
                    byte[] data = new byte[50];
                    rdr.Seek(80, SeekOrigin.Begin);
                    rdr.Read(data, 0, 4);
                    var cnt = BitConverter.ToInt32(data, 0);
                    for (int i = 0; i < cnt; i++)
                    {
                        TriangleInfo tr = new TriangleInfo();
                        mm.Mesh.Triangles.Add(tr);

                        tr.Vertices = new VertexInfo[3];
                        Vector3d normal = new Vector3d();
                        Vector3d v1 = new Vector3d();
                        Vector3d v2 = new Vector3d();
                        Vector3d v3 = new Vector3d();
                        rdr.Read(data, 0, 50);

                        for (int j = 0; j < 3; j++)
                        {
                            normal[j] = BitConverter.ToSingle(data, j * 4);
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            v1[j] = BitConverter.ToSingle(data, 12 + j * 4);
                        }

                        for (int j = 0; j < 3; j++)
                        {
                            v2[j] = BitConverter.ToSingle(data, 24 + j * 4);
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            v3[j] = BitConverter.ToSingle(data, 36 + j * 4);
                        }
                        tr.Vertices[0] = new VertexInfo() { Position = v1, Normal = normal };
                        tr.Vertices[1] = new VertexInfo() { Position = v2, Normal = normal };
                        tr.Vertices[2] = new VertexInfo() { Position = v3, Normal = normal };
                    }
                }
            }
            mm.FlatShading = false;
            mm.DrawWireframe = false;
            mm.PickEnabled = false;
            return ret.ToArray();
        }
    }
}
