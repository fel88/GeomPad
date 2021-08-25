using OpenTK;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace GeomPad
{
    public partial class VectorSetValuesDialog : Form
    {
        public VectorSetValuesDialog()
        {
            InitializeComponent();
        }

        public Vector3d Vector;
        public void Init(Vector3d vector)
        {
            Vector = vector;
            updLabels();
            textBox1.Text = $"{Vector.X};{Vector.Y};{Vector.Z}";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        void updLabels()
        {
            textBox2.Text = $"{Vector.X}";
            textBox3.Text = $"{Vector.Y}";
            textBox4.Text = $"{Vector.Z}";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var ar = textBox1.Text.Split(new char[] { ';', '{', '}', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var vals = ar.Select(z => double.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
                Vector = new Vector3d(vals[0], vals[1], vals[2]);
                updLabels();
                textBox1.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox1.SetErrorStyle();
            }
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Vector.X = StaticHelpers.ParseDouble(textBox2.Text);
                (sender as TextBox).SetNormalStyle();
            }
            catch
            {
                (sender as TextBox).SetErrorStyle();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Vector.Y = StaticHelpers.ParseDouble(textBox3.Text);
                (sender as TextBox).SetNormalStyle();
            }
            catch
            {
                (sender as TextBox).SetErrorStyle();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Vector.Z = StaticHelpers.ParseDouble(textBox4.Text);
                (sender as TextBox).SetNormalStyle();
            }
            catch
            {
                (sender as TextBox).SetErrorStyle();
            }
        }
    }
}
