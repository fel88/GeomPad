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
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using OpenTK;

namespace GeomPad.Controls._2d
{
    public partial class HelpersListControl : UserControl
    {
        public HelpersListControl()
        {
            InitializeComponent();
        }

        Pad2DDataModel dataModel;
        internal void Init(Pad2DDataModel dataModel)
        {
            this.dataModel = dataModel;
            dataModel.OnListUpdated += DataModel_OnListUpdated;
        }

        private void DataModel_OnListUpdated()
        {
            UpdateList();
        }

        void changed()
        {
            UpdateList();
        }

        void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in dataModel.Items)
            {
                string sub = string.Empty;
                item.Changed = changed;

                if (item is PolygonHelper ph)
                {
                    sub = ph.Polygon.Childrens.Count.ToString();
                }
                var lvi = new ListViewItem(new string[] { string.Empty, item.Name, item.GetType().Name, sub }) { Tag = item, UseItemStyleForSubItems = false };
                if (item is PolygonHelper ph2)
                {
                    if (ph2.Fill)
                    {
                        lvi.BackColor = ph2.FillColor;
                    }
                }
                if (item is MeshHelper mh)
                {
                    if (mh.Fill)
                    {
                        lvi.BackColor = mh.FillColor;
                    }
                }
                listView1.Items.Add(lvi);
            }
        }

        private void pointToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new PointHelper());
            UpdateList();
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new RectangleGenerator() { Changed = changed };
            dataModel.AddItem(ph);
            UpdateList();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataModel.Clear();
            UpdateList();
        }
        void deleteItems()
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (dataModel.ParentForm.ShowQuestion($"Are you to sure to delete {listView1.SelectedItems.Count} items?") == DialogResult.No) return;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                dataModel.RemoveItem(listView1.SelectedItems[i].Tag as HelperItem);
            }

            UpdateList();
        }
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteItems();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            dataModel.ClearSelection();

            if (listView1.SelectedItems.Count == 0) return;
            List<HelperItem> sel = new List<HelperItem>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                sel.Add(listView1.SelectedItems[i].Tag as HelperItem);
            }
            dataModel.ChangeSelectedItems(sel.ToArray());
        }

        private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                var maxz = dataModel.Items.Max(z => z.Z);
                dataModel.SelectedItems[i].Z = maxz + 1;
            }

            dataModel.UpdateList();
        }
        HelperItem[] loadXml(string content)
        {
            var doc = XDocument.Parse(content);

            List<NFP> nfps = new List<NFP>();
            foreach (var item in doc.Descendants("region"))
            {
                //  PolygonHelper ph = new PolygonHelper();
                //  Items.Add(ph);
                List<SvgPoint> pnts = new List<SvgPoint>();
                foreach (var point in item.Descendants("point"))
                {
                    var x = float.Parse(point.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    var y = float.Parse(point.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    pnts.Add(new SvgPoint(x, y));
                }
                nfps.Add(new NFP() { Points = pnts.Select(y => new SvgPoint(y.X, y.Y)).ToArray() });
                //  ph.Polygon.Points = pnts.ToArray();

            }

            //UpdateList();

            //var nfps = r.regions.Select(z => new NFP() { Points = z.Select(y => new SvgPoint(y.x, y.y)).ToArray() }).ToArray();

            for (int i = 0; i < nfps.Count; i++)
            {
                for (int j = 0; j < nfps.Count; j++)
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

            List<HelperItem> ret = new List<HelperItem>();
            foreach (var item in nfps)
            {
                if (item.Parent != null) continue;
                PolygonHelper phh = new PolygonHelper();
                ret.Add(phh);
                phh.Polygon = item;
            }

            return ret.ToArray();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var ret = loadXml(File.ReadAllText(ofd.FileName));
                dataModel.AddItems(ret);
                var fin = new FileInfo(ofd.FileName);
                foreach (var item in ret)
                {
                    item.Name = fin.Name;
                }
                UpdateList();
                dataModel.ParentForm.StatusMessage("succesfully loaded.", StatusMessageType.Info);

            }
            catch (Exception ex)
            {
                dataModel.ParentForm.StatusMessage(ex.Message, StatusMessageType.Error);
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList();
            dataModel.UpdateList();
        }

        private void dxfFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "dxf files (*.dxf)|*.dxf";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var r = DxfParser.LoadDxf(ofd.FileName);

            List<NFP> nfps = new List<NFP>();
            foreach (var rr in r)
            {
                nfps.Add(new NFP() { Points = rr.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            }

            for (int i = 0; i < nfps.Count; i++)
            {
                for (int j = 0; j < nfps.Count; j++)
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

            List<HelperItem> ret = new List<HelperItem>();
            foreach (var item in nfps)
            {
                if (item.Parent != null) continue;
                PolygonHelper phh = new PolygonHelper();
                phh.Name = new FileInfo(ofd.FileName).Name;
                ret.Add(phh);
                phh.Polygon = item;
            }

            dataModel.AddItems(ret.ToArray());
        }

        private void extractChildsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            if (!(hh is PolygonHelper ph)) return;

            foreach (var item in ph.Polygon.Childrens)
            {
                PolygonHelper ph2 = new PolygonHelper();
                ph2.Polygon = DeepNest.clone(item);
                dataModel.AddItem(ph2);
            }
        }

        private void fitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            List<PointF> pp = new List<PointF>();


            var rect = hh.BoundingBox();
            if (rect == null) return;
            pp.Add(rect.Value.Location);
            pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));

            if (pp.Count == 0) return;
            dataModel.dc.FitToPoints(pp.ToArray(), 5);
        }

        private void segmentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new SegmentHelper() { Point2 = new Vector2d(10, 10) });            
        }

        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var hh = loadXml(Clipboard.GetText());
                dataModel.AddItems(hh.ToArray());
                UpdateList();
                dataModel.ParentForm.StatusMessage("succesfully loaded.", StatusMessageType.Info);

            }
            catch (Exception ex)
            {
              dataModel.ParentForm.  StatusMessage(ex.Message, StatusMessageType.Error);
            }
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            if (hh is SegmentHelper sh)
            {
                dataModel.AddItem(new SegmentHelper() { X = sh.X, Y = sh.Y, X2 = sh.X2, Y2 = sh.Y2 });
            }            
        }
        Random rand = new Random();
        private void randomizePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            if (hh is PolygonHelper ph)
            {
                for (int i = 0; i < ph.Polygon.Points.Length; i++)
                {
                    ph.Polygon.Points[i] = new SvgPoint(rand.Next(-100, 100), rand.Next(-100, 100));
                }
            }
        }

        private void moveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            VectorSetValuesDialog d = new VectorSetValuesDialog();
            d.ShowDialog();
            hh.Shift(d.Vector.Xy);
        }

        private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                var minz = dataModel.Items.Min(z => z.Z);
                dataModel.SelectedItems[i].Z = minz - 1;
            }            
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteItems();
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new CircleGenerator();
            dataModel.AddItem(ph);            
        }

        private void polygonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new PolygonHelper());            
        }
    }
}
