using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using GeomPad.Helpers;
using System.Globalization;
using ClipperLib;
using OpenTK;
using GeomPad.Common;

namespace GeomPad.Controls._2d
{
    public partial class ClipperOperationsControl : UserControl
    {
        public ClipperOperationsControl()
        {
            InitializeComponent();
            var vls = Enum.GetValues(typeof(JoinType));
            foreach (var item in vls)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.ToString() });
            }
            comboBox1.SelectedIndex = Array.IndexOf(vls, JoinType.jtRound);
        }
        Pad2DDataModel dataModel;

        public void Init(Pad2DDataModel dm)
        {
            this.dataModel = dm;
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            var jType = (JoinType)comboBox1.SelectedIndex;
            double offset = textBox2.Text.ParseDouble();
            double miterLimit = textBox3.Text.ParseDouble();
            double curveTolerance = textBox4.Text.ParseDouble();

            if (dataModel.SelectedItem is PolygonHelper ph2)
            {
                var ph = Geometry.Offset(ph2, offset, jType, curveTolerance, miterLimit);
                dataModel.AddItem(ph);
            }
            else if (dataModel.SelectedItem is PolylineHelper plh2)
            {
                var ph = Geometry.Offset(plh2, offset, jType, curveTolerance, miterLimit);
                dataModel.AddItem(ph);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            //if (!checkBox1.Checked)
            {
                if (dataModel.SelectedItems.Length < 2) { dataModel.ParentForm.StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

                foreach (var item in dataModel.SelectedItems)
                {
                    phhs.Add(item as PolygonHelper);
                }
            }
            //  else
            {
                //   phhs.Add((comboBox2.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
                //   phhs.Add((comboBox3.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
            }

            var ar1 = phhs.ToArray();


            NFP p = new NFP();
            NFP p2 = new NFP();


            var jType = (JoinType)comboBox1.SelectedIndex;
            double offset = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double miterLimit = double.Parse(textBox3.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double curveTolerance = double.Parse(textBox4.Text.Replace(",", "."), CultureInfo.InvariantCulture);

            p.Points = ar1[0].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            p2.Points = ar1[1].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            var offs = ClipperHelper.intersection(p, p2, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
            PolygonHelper ph = new PolygonHelper();


            if (offs.Any())
            {
                ph.Polygon.Points = offs.First().Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            }

            foreach (var item in offs.Skip(1))
            {
                var nfp2 = new NFP();

                nfp2.Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon.Childrens.Add(nfp2);
            }


            dataModel.AddItem(ph);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            var res = dataModel.GetPairOfSelectedNfps();
            if (res == null)
                return;

            NFP offs = ClipperHelper.MinkowskiSum(res[0], res[1], checkBox2.Checked, checkBox3.Checked);
            if (offs != null)
            {
                PolygonHelper ph = new PolygonHelper();
                //ph.Polygon.Points = offs.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon = DeepNest.clone2(offs);

                dataModel.AddItem(ph);
            }
        }
    }
}
