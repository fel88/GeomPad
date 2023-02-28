using System;

namespace GeomPad
{
    public interface ICommand
    {
        string Name { get; }
        Action<ICommandContext> Process { get; }
    }
}
