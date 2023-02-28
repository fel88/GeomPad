using OpenTK;

namespace GeomPad
{
    public abstract class SpaceInfo
    {
        public abstract string GetKey(Vector3d v);
        public abstract PointIndexer GetNeighbours(Vector3d v);
    }
}