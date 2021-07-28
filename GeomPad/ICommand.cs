using System;

namespace GeomPad
{
    public interface ICommand
    {
        string Name { get; }
        Action<AbstractHelperItem, AbstractHelperItem[], IPadContainer> Process { get; }
    }
}
