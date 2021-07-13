using System;

namespace GeomPad
{
    public class HingeHelperCalcOutterNormal : ICommand
    {
        public string Name => "calc outter normal";

        public Action<HelperItem3D, HelperItem3D[], I3DPadContainer> Process => (z, arr, cc) =>
        {
            var tr = z as HingeHelper;
            tr.CalcConjugateNormal();            
        };
    }

    public static class DebugHelper
    {
        public static Action<string> Error;
    }
}
