using System.Collections.Generic;
using System.Linq;

namespace GeomPad
{
    public class NFP
    {
        public SvgPoint[] Points = new SvgPoint[] { };
        public List<NFP> Childrens = new List<NFP>();
        public NFP Parent;
        public int Length
        {
            get
            {
                return Points.Length;
            }
        }
        public void push(SvgPoint svgPoint)
        {
            List<SvgPoint> points = new List<SvgPoint>();
            if (Points == null)
            {
                Points = new SvgPoint[] { };
            }
            points.AddRange(Points);
            points.Add(svgPoint);
            Points = points.ToArray();

        }
        public NFP slice(int v)
        {
            var ret = new NFP();
            List<SvgPoint> pp = new List<SvgPoint>();
            for (int i = v; i < Length; i++)
            {
                pp.Add(new SvgPoint(this[i].X, this[i].Y));

            }
            ret.Points = pp.ToArray();
            return ret;
        }
        public SvgPoint this[int ind]
        {
            get
            {
                return Points[ind];
            }
        }
        public void AddPoint(SvgPoint point)
        {
            var list = Points.ToList();
            list.Add(point);
            Points = list.ToArray();
        }
    }
}
