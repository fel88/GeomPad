using System.Collections.Generic;
using System.Windows.Forms;

namespace GeomPad.Common
{
    public interface IPadContainer
    {
        void OpenChildWindow(Form f);
        void AddHelper(IHelperItem h);
        void UpdateHelpersList();
        void AddHelpers(IEnumerable<IHelperItem> h);
        void SetStatus(string v, StatusMessageType type);
    }
}
