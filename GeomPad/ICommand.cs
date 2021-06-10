using System;

namespace GeomPad
{
    public interface ICommand
    {
        string Name { get; }
        Action<HelperItem3D, HelperItem3D[], I3DPadContainer> Process { get; }
    }

}
