using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeomPad.Helpers;
using System.Globalization;
using ClipperLib;
using OpenTK;

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
            NFP p = new NFP();
            var jType = (JoinType)comboBox1.SelectedIndex;
            double offset = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double miterLimit = double.Parse(textBox3.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double curveTolerance = double.Parse(textBox4.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            if ((dataModel.SelectedItem is PolygonHelper ph2))
            {
                p.Points = ph2.Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
        
                var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                //if (offs.Count() > 1) throw new NotImplementedException();
                PolygonHelper ph = new PolygonHelper();
                foreach (var item in ph2.Polygon.Childrens)
                {
                    var offs2 = ClipperHelper.offset(item, -offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                    var nfp1 = new NFP();
                    if (offs2.Any())
                    {
                        //if (offs2.Count() > 1) throw new NotImplementedException();
                        foreach (var zitem in offs2)
                        {
                            nfp1.Points = zitem.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                            ph.Polygon.Childrens.Add(nfp1);
                        }
                    }
                }

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

                ph.OffsetX = ph2.OffsetX;
                ph.OffsetY = ph2.OffsetY;
                ph.Rotation = ph2.Rotation;
                dataModel.AddItem(ph);
            }
            if ((dataModel.SelectedItem is PolylineHelper plh2))
            {
                p.Points = plh2.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                
                var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                
                PolylineHelper ph = new PolylineHelper();              

                if (offs.Any())
                {
                    ph.Points = offs.First().Points.Select(z => new Vector2d(z.X, z.Y)).ToList();
                    ph.Points.Add(ph.Points[0]);
                }
                
                dataModel.AddItem(ph);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            //if (!checkBox1.Checked)
            {
                if (dataModel.SelectedItems.Length < 2) {dataModel.ParentForm. StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

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
            if (res == null) return;
            NFP offs = null;
            offs = ClipperHelper.MinkowskiSum(res[0], res[1], checkBox2.Checked, checkBox3.Checked);
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
