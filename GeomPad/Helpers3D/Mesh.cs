using System;
using System.Collections.Generic;

namespace GeomPad.Helpers3D
{
    public class Mesh
    {
        public List<TriangleInfo> Triangles = new List<TriangleInfo>();

        internal void SwitchNormals()
        {
            foreach (var item in Triangles)
            {
                foreach (var v in item.Vertices)
                {
                    v.Normal *= -1;
                }
            }
        }
    }
}
