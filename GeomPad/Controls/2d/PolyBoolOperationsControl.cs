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
using PolyBoolCS;
using GeomPad.Common;

namespace GeomPad.Controls._2d
{
    public partial class PolyBoolOperationsControl : UserControl
    {
        public PolyBoolOperationsControl()
        {
            InitializeComponent();
        }
        Pad2DDataModel dataModel;
        public void Init(Pad2DDataModel dataModel)
        {
            this.dataModel = dataModel;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> ar1 = new List<PolygonHelper>();
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                ar1.Add(dataModel.SelectedItems[i] as PolygonHelper);
            }

            if (ar1.Count != 2) { dataModel.ParentForm.StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

            PolyBool pb = new PolyBool();

            //var poly1 = GetPolygon(ar1[0].TransformedPoints().ToArray());
            //var poly2 = GetPolygon(ar1[1].TransformedPoints().ToArray());
            var poly1 = ar1[0].GetPolygon();
            var poly2 = ar1[1].GetPolygon();
            var r = pb.intersect(poly1, poly2);
            if (r.regions.Count == 0)
            {
                dataModel.ParentForm.StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }
            var pnts = r.regions.ToArray()[0].ToArray();
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon.Points = pnts.Select(z => new SvgPoint(z.x, z.y)).ToArray();
            dataModel.AddItem(ph);            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            if (!checkBox1.Checked)
            {
                if (dataModel.SelectedItems.Length < 2) { dataModel.ParentForm. StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

                foreach (var item in dataModel.SelectedItems)
                {
                    phhs.Add(item as PolygonHelper);
                }
            }
            else
            {
                phhs.Add((comboBox2.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
                phhs.Add((comboBox3.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
            }

            var ar1 = phhs.ToArray();

            PolyBool pb = new PolyBool();

            var poly1 = ar1[0].GetPolygon();
            foreach (var item in ar1.Skip(1))
            {
                poly1 = pb.difference(poly1, item.GetPolygon());
            }

            if (poly1.regions.Count == 0)
            {
                dataModel.ParentForm.StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }
            var r = poly1;

            var nfps = r.regions.Select(z => new NFP() { Points = z.Select(y => new SvgPoint(y.x, y.y)).ToArray() }).ToArray();

            for (int i = 0; i < nfps.Length; i++)
            {
                for (int j = 0; j < nfps.Length; j++)
                {
                    if (i != j)
                    {
                        var d2 = nfps[i];
                        var d3 = nfps[j];
                        var f0 = d3.Points[0];
                        if (StaticHelpers.pnpoly(d2.Points.ToArray(), f0.X, f0.Y))
                        {
                            d3.Parent = d2;
                            if (!d2.Childrens.Contains(d3))
                            {
                                d2.Childrens.Add(d3);
                            }
                        }
                    }
                }
            }

            foreach (var item in nfps)
            {
                if (item.Parent != null) continue;
                PolygonHelper phh = new PolygonHelper();
                dataModel.AddItem(phh);
                phh.Polygon = item;
            }            
        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            foreach (var item in dataModel.Items)
            {
                comboBox2.Items.Add(new ComboBoxItem() { Name = $"{item.Name ?? string.Empty} ({item.GetType().Name})", Tag = item });
            }
        }

        private void comboBox3_DropDown(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();
            foreach (var item in dataModel.Items)
            {
                comboBox3.Items.Add(new ComboBoxItem() { Name = $"{item.Name ?? string.Empty} ({item.GetType().Name})", Tag = item });
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = checkBox1.Checked;
            comboBox3.Enabled = checkBox1.Checked;
        }
    }
}
