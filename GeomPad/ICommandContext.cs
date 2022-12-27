using System;

namespace GeomPad
{
    public interface ICommandContext
    {
        Action<string> InfoMessage { get; }
        IHelperItem Source { get; }
        IHelperItem[] Operands { get; }
        IPadContainer Parent { get; }
    }
}
