using OpenTK;
using System.Collections.Generic;

namespace GeomPad.Helpers3D
{
    public interface ITrianglesProvider
    {
        IEnumerable<TriangleInfo> GetTriangles();
        bool Visible { get; }
    }
}
