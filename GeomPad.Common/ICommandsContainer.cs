using System.Windows.Input;

namespace GeomPad.Common
{
    public interface ICommandsContainer
    {
        ICommand[] Commands { get; }
    }
}
