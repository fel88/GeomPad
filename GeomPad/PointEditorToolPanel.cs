using System;
using System.Windows.Forms;
using OpenTK;
using System.Globalization;

namespace GeomPad
{
    public partial class PointEditorToolPanel : UserControl
    {
        public PointEditorToolPanel()
        {
            InitializeComponent();
        }

        internal void SetPoint(Vector2d vector2d)
        {
            p = vector2d;
            allowFire = false;
            textBox1.Text = vector2d.X.ToString("N5");
            textBox2.Text = vector2d.Y.ToString("N5");
            allowFire = true;
        }
        bool allowFire = false;
        Vector2d p;
        public event Action<Vector2d> PointChanged;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            try
            {
                p.X = double.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);                
                textBox1.SetNormalStyle();
            }
            catch
            {
                textBox1.SetErrorStyle();
            }
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!allowFire) return;
            try
            {
                p.Y = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                textBox2.SetNormalStyle();
            }
            catch
            {
                textBox2.SetErrorStyle();
            }
        }

        public event Action Set;
        private void button1_Click(object sender, EventArgs e)
        {
            PointChanged?.Invoke(p);
            Set?.Invoke();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PointChanged?.Invoke(p);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PointChanged?.Invoke(p);
            }
        }
    }
}
