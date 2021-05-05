using System.Collections.Generic;

namespace GeomPad
{
    public class NFP
    {
        public SvgPoint[] Points = new SvgPoint[] { };
        public List<NFP> Childrens = new List<NFP>();
        public NFP Parent;
    }
}
