using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public class PointIndexer
    {
        public void ChangeAxis(int mode)
        {
            if (mode == 1)
            {
                for (int i = 0; i < Points.Length; i++)
                {
                    var xt = Points[i].X;
                    Points[i].X = Points[i].Z;
                    Points[i].Z = xt;
                }
            }
            else
            {
                for (int i = 0; i < Points.Length; i++)
                {
                    var xt = Points[i].Y;
                    Points[i].Y = Points[i].Z;
                    Points[i].Z = xt;
                }
            }
        }

        public PointIndexer GetSubIndexer(int[] inds)
        {
            var r = new PointIndexer() { Points = Points, Indicies = inds };
            r.Init();
            return r;
        }

        public static PointIndexer FromPoints(Vector3d[] points)
        {
            PointIndexer ret = new PointIndexer();
            ret.Points = points;
            ret.Indicies = points.Select((z, i) => i).ToArray();
            return ret;
        }
        public Vector3d[] Points;
        public int[] Indicies;
        HashSet<int> indHash = new HashSet<int>();
        public bool Contains(int v)
        {
            return indHash.Contains(v);
        }
        public static double ClusterDefaultStep = 0.02;
        public double ClusterStep = ClusterDefaultStep;
        public void Init()
        {
            indHash = new HashSet<int>();
            foreach (var item in Indicies)
            {
                indHash.Add(item);
            }

            csi.Create(this, ClusterStep);
        }

        public int[] GetRandomIndecies(int cnt, Random r)
        {
            int[] ret = new int[cnt];
            while (ret.GroupBy(z => z).Count() < cnt)
            {
                for (int i = 0; i < cnt; i++)
                {
                    ret[i] = Indicies[r.Next(Indicies.Length)];
                }
            }
            return ret;
        }

        public Vector3d[] GetPoints(int[] rnd)
        {
            return rnd.Select(z => Points[z]).ToArray();
        }

        public DumbClusterSpaceInfo csi = new DumbClusterSpaceInfo();

        public void Centralized()
        {
            var maxx = Points.Max(z => z.X);
            var maxy = Points.Max(z => z.Y);
            var maxz = Points.Max(z => z.Z);
            var minx = Points.Min(z => z.X);
            var miny = Points.Min(z => z.Y);
            var minz = Points.Min(z => z.Z);

            var cx = (maxx + minx) / 2;
            var cy = (maxy + miny) / 2;
            var cz = (maxz + minz) / 2;
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].X -= cx;
                Points[i].Y -= cy;
                Points[i].Z -= cz;
            }
            csi.Recreate(this);
        }

        public class BoundingBoxInfo
        {
            public Vector3d Center;
            public double[] Sizes;
        }

        public BoundingBoxInfo BoundingBox()
        {
            BoundingBoxInfo bb = new BoundingBoxInfo();
            var maxx = Points.Max(z => z.X);
            var maxy = Points.Max(z => z.Y);
            var maxz = Points.Max(z => z.Z);

            var minx = Points.Min(z => z.X);
            var miny = Points.Min(z => z.Y);
            var minz = Points.Min(z => z.Z);

            bb.Center = new Vector3d((maxx + minx) / 2, (maxy + miny) / 2, (maxz + minz) / 2);
            bb.Sizes = new double[] { maxx - minx, maxy - miny, maxz - minz };
            return bb;
        }

        internal void ScalePoints(double koef)
        {

            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] *= koef;
            }
        }

        internal void TranslatePoints(Vector3d vector3d)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] += vector3d;
            }
        }

        internal void Merge(PointIndexer pin)
        {
            Points = Points.Union(pin.Points).ToArray();
            Indicies = Indicies.Concat(pin.Indicies.Select(u => u + Indicies.Length)).ToArray();
            Init();
        }
    }
}