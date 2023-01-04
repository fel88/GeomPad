using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace GeomPad
{
    public static class DialogHelpers
    {
        public static string StringDialog(string caption)
        {
            Form f = new Form();

            f.Text = caption;

            Button b1 = new Button();
            b1.Text = "Ok";
            TextBox tb1 = new System.Windows.Forms.TextBox();
            b1.Click += (s, e) =>
            {
                f.DialogResult = DialogResult.OK;
                f.Close();
            };
            f.Controls.Add(tb1);
            f.Controls.Add(b1);
            tb1.Left = 5;

            tb1.Top = 5;
            b1.Left = 5;
            b1.Top = tb1.Bottom + 5;
            f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Width = 400;
            f.Height = 100;

            tb1.Width = f.Width - 30;
            f.ShowDialog();
            return tb1.Text;
        }
    }
}