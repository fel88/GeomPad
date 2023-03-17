using OpenTK;
using System.Collections.Generic;

namespace GeomPad.Common
{
    public interface IPointsProvider
    {
        IEnumerable<Vector3d> GetPoints();
        bool Visible { get; }
    }
}
