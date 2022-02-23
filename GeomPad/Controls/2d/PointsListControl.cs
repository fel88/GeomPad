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
    public partial class PointsListControl : UserControl
    {
        public PointsListControl()
        {
            InitializeComponent();
        }

        Pad2DDataModel dataModel;

        public void UpdateList()
        {
            listView2.Items.Clear();
            if (!(dataModel.SelectedItem is PolygonHelper ph)) return;
            for (int i = 0; i < ph.Polygon.Points.Length; i++)
            {
                listView2.Items.Add(new ListViewItem(new string[] { ph.Polygon.Points[i].X + "", ph.Polygon.Points[i].Y + "" })
                {
                    Tag = new PolygonPointEditorWrapper(ph.Polygon, i)
                });
            }
        }
        internal void Init(Pad2DDataModel dataModel)
        {
            this.dataModel = dataModel;
        }
    }
}
