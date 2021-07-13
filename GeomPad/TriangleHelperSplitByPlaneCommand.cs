using System;
using System.Linq;

namespace GeomPad
{
    public class TriangleHelperSplitByPlaneCommand : ICommand
    {
        public string Name => "split by plane";

        public Action<HelperItem3D, HelperItem3D[], I3DPadContainer> Process => (z, arr, cc) =>
         {
             var tr = z as TriangleHelper;
             var pl = arr.First(t => t is PlaneHelper) as PlaneHelper;
             var res = tr.SplitByPlane(pl);
             cc.AddHelpers(res);
         };
    }
}
