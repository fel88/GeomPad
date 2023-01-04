using System.Net.NetworkInformation;

namespace GeomPad.Common
{
    public interface IScript
    {        
        void Run(IPad2DDataModel model, IPadContainer padContainer);
    }
}
