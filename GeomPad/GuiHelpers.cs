using System.Windows.Forms;

namespace GeomPad
{
    public static class GuiHelpers
    {
        public static void ShowMessage(this Form f, string text, MessageBoxIcon type)
        {
            MessageBox.Show(text, f.Text, MessageBoxButtons.OK, type);
        }
        public static void ShowMessage(string caption, string text, MessageBoxIcon type)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, type);
        }

        public static DialogResult ShowQuestion(this Form f, string text)
        {
            return MessageBox.Show(text, f.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static bool Question(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}