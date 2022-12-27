using System;

namespace GeomPad
{
    public class CommandContext : ICommandContext
    {
        public CommandContext(IHelperItem s, IHelperItem[] ops, IPadContainer pad)
        {
            Source = s;
            Operands = ops;
            Parent = pad;

        }

        public Action<string> InfoMessage { get; set; }

        public IHelperItem Source { get; set; }

        public IHelperItem[] Operands { get; set; }

        public IPadContainer Parent { get; set; }
    }
}
