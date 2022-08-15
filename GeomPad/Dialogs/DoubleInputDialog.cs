using System;
using System.Windows.Forms;

namespace GeomPad.Dialogs
{
    public partial class DoubleInputDialog : Form
    {
        public DoubleInputDialog()
        {
            InitializeComponent();
            DialogResult = DialogResult.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        public void Init(double val)
        {
            Value = val;
            textBox1.Text = val.ToString();
        }

        public string Caption { get => Text; set => Text = value; }

        public double Value { get; set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Value = StaticHelpers.ParseDouble(textBox1.Text);
                textBox1.SetNormalStyle();
            }
            catch (Exception ex)
            {
                textBox1.SetErrorStyle();
            }
        }
    }
}
