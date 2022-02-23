using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeomPad.Controls._2d
{
    public partial class DebugControl : UserControl
    {
        public DebugControl()
        {
            InitializeComponent();
        }
        Pad2DDataModel dataModel;
        public void Init(Pad2DDataModel dm)
        {
            dataModel = dm;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //dc.scale = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            dataModel.drawAxis = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            dataModel.bubbleUpSelected = checkBox5.Checked;

        }
    }
}
