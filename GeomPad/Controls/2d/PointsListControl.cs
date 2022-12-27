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
using OpenTK;

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
            if ((dataModel.SelectedItem is PolygonHelper ph))
            {
                for (int i = 0; i < ph.Polygon.Points.Length; i++)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { ph.Polygon.Points[i].X + "", ph.Polygon.Points[i].Y + "" })
                    {
                        Tag = new PolygonPointEditorWrapper(ph.Polygon, i)
                    });
                }
            }
            if ((dataModel.SelectedItem is PolylineHelper plh))
            {
                for (int i = 0; i < plh.Points.Count; i++)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { plh.Points[i].X.ToString(), plh.Points[i].Y.ToString() })
                    {
                        Tag = new PolylinePointEditorWrapper(plh, i)
                    });
                }
            }
        }
        internal void Init(Pad2DDataModel dataModel)
        {
            this.dataModel = dataModel;
            dataModel.OnSelectedChanged += DataModel_OnSelectedChanged;
        }

        private void DataModel_OnSelectedChanged(HelperItem obj)
        {
            UpdateList();
        }

        private void addPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItem is PolygonHelper ph)
            {
                var list = ph.Polygon.Points.ToList();
                list.Add(new SvgPoint(0, 0));
                ph.Polygon.Points = list.ToArray();
            }
            if (dataModel.SelectedItem is PolylineHelper plh)
            {
                plh.Points.Add(new Vector2d());
            }
            UpdateList();

            var wr = listView2.Items[listView2.Items.Count - 1].Tag as IPoint;
            if (wr == null) 
                return;

            VectorSetValuesDialog svd = new VectorSetValuesDialog();
            svd.Init(new Vector3d(wr.X, wr.Y, 0));
            svd.ShowDialog();
            wr.X = svd.Vector.X;
            wr.Y = svd.Vector.Y;

            UpdateList();
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) 
                return;

            var wr = listView2.SelectedItems[0].Tag as IPoint;
            VectorSetValuesDialog svd = new VectorSetValuesDialog();
            svd.Init(new Vector3d(wr.X, wr.Y, 0));
            svd.ShowDialog();
            wr.X = svd.Vector.X;
            wr.Y = svd.Vector.Y;
        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}
