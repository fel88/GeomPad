using System.Windows.Forms;

namespace GeomPad
{
    public interface IPadContainer
    {
        void OpenChildWindow(Form f);
        void AddHelper(AbstractHelperItem h);
        void AddHelpers(AbstractHelperItem[] h);
        void SetStatus(string v, StatusTypeEnum type);
    }
}
