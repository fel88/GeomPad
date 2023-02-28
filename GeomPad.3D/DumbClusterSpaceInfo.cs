using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public class DumbClusterSpaceInfo : SpaceInfo
    {
        public ClusterInfo[] GetClusters()
        {
            return _clusters;
        }
        ClusterInfo[] _clusters;
        Dictionary<string, List<int>> ds = new Dictionary<string, List<int>>();
        PointIndexer _indexer;
        double _step;
        public void Create(PointIndexer indexer, double step = 1000)
        {
            _step = step;
            _indexer = indexer;
            var pts = indexer.Points;
            ds = new Dictionary<string, List<int>>();

            for (int i = 0; i < pts.Length; i++)
            {
                if (!indexer.Contains(i)) continue;
                var p = pts[i] * step;
                var key = (int)(p.X) + ";" + (int)p.Y + ";" + (int)p.Z;
                if (!ds.ContainsKey(key)) ds.Add(key, new List<int>());
                ds[key].Add(i);
            }
            _clusters = ds.Select(z => new ClusterInfo() { Key = z.Key, Indicies = z.Value.ToArray() }).ToArray();
        }
        public override string GetKey(Vector3d v)
        {
            var p = v * _step;
            var key = (int)(p.X) + ";" + (int)p.Y + ";" + (int)p.Z;
            return key;
        }
        public override PointIndexer GetNeighbours(Vector3d v)
        {
            var key = GetKey(v);
            if (!ds.ContainsKey(key)) return null;
            return new PointIndexer() { Points = _indexer.Points, Indicies = ds[key].ToArray() };
        }

        internal void Recreate(PointIndexer pin)
        {
            Create(pin, _step);
        }
    }
}