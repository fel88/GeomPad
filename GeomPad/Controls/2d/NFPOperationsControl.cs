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

namespace GeomPad.Controls._2d
{
    public partial class NFPOperationsControl : UserControl
    {
        public NFPOperationsControl()
        {
            InitializeComponent();
        }

        Pad2DDataModel dataModel;
        public void Init(Pad2DDataModel dm)
        {
            dataModel = dm;
        }
        private void button13_Click(object sender, EventArgs e)
        {
            var res = dataModel.GetPairOfSelectedNfps();
            if (res == null) return;
            var p = res[0];
            var p2 = res[1];
            var offs = DeepNest.getOuterNfp(p, p2);

            if (offs != null)
            {
                PolygonHelper ph = new PolygonHelper();
                //ph.Polygon.Points = offs.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon = DeepNest.clone2(offs);

                dataModel.AddItem(ph);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var res = dataModel.GetPairOfSelectedNfps();
            if (res == null) return;
            var p = res[0];
            var p2 = res[1];
            var offs = DeepNest.getInnerNfp(p, p2);

            if (offs != null)
            {
                foreach (var item in offs)
                {
                    PolygonHelper ph = new PolygonHelper();
                    //ph.Polygon.Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                    ph.Polygon = DeepNest.clone2(item);
                    dataModel.AddItem(ph);
                }                
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var res = dataModel.GetPairOfSelectedNfps();
            if (res == null) return;
            var p = res[0];
            var p2 = res[1];
            var offs = DeepNest.Convolve(p2, p);

            if (offs != null)
            {
                foreach (var item in offs)
                {
                    PolygonHelper ph = new PolygonHelper();
                    //ph.Polygon.Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                    ph.Polygon = DeepNest.clone2(item);
                    dataModel.AddItem(ph);
                }
            }
        }
    }
}
