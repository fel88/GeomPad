using System.Collections.Generic;

namespace GeomPad.Helpers3D
{
    public class Model
    {
        public List<TriangleInfo> Polygons = new List<TriangleInfo>();

        public object Tag { get; set; }
    }
}
