using System.Drawing;
using System.Windows.Forms;

namespace GeomPad
{
    public static class GuiExtensions
    {
        public static void SetErrorStyle(this TextBox c)
        {
            c.BackColor = Color.Red;
            c.ForeColor = Color.White;
        }
        public static void SetNormalStyle(this TextBox c)
        {
            c.BackColor = Color.White;
            c.ForeColor = Color.Black;
        }
    }
}
