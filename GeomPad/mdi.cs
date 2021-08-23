using GeomPad.Helpers3D.BRep;
using System;
using System.Windows.Forms;

namespace GeomPad
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.MdiParent = this;
            f.Show();
        }
    }
}
