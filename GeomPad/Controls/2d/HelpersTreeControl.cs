using BrightIdeasSoftware;
using GeomPad.Common;
using GeomPad.Helpers;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad.Controls._2d
{
    public partial class HelpersTreeControl : UserControl
    {
        public HelpersTreeControl()
        {
            InitializeComponent();

            treeListView1.CanExpandGetter = (xx) =>
            {
                if (xx is Group gr)
                    return gr.Items.Count > 0;
                return false;
            };
            treeListView1.ChildrenGetter = (xx) =>
            {
                if (xx is Group g)
                {
                    return g.Items;
                }
                return null;
            };

            treeListView1.ShowGroups = false;
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
            treeListView1.SetObjects(dataModel.Items);
        }

        private void pointToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new PointHelper());
            UpdateList();
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
                dataModel.ParentForm.StatusMessage(ex.Message, StatusMessageType.Error);
            }
        }
        HelperItem loadPolyline(XElement el)
        {
            PolylineHelper lsh = new PolylineHelper();

            foreach (var point in el.Descendants("point"))
            {
                var x = point.Attribute("x").Value.ParseDouble();
                var y = point.Attribute("y").Value.ParseDouble();
                lsh.Points.Add(new Vector2d(x, y));
            }

            return lsh;
        }

        HelperItem loadPoint(XElement el)
        {
            PointHelper lsh = new PointHelper();
            var x = el.Attribute("x").Value.ParseDouble();
            var y = el.Attribute("y").Value.ParseDouble();
            lsh.Point.X = x;
            lsh.Point.Y = y;
            return lsh;
        }

        HelperItem loadLineSet(XElement el)
        {
            LinesSetHelper lsh = new LinesSetHelper();
            foreach (var item in el.Elements("line"))
            {
                List<Vector2d> pnts = new List<Vector2d>();
                foreach (var point in item.Descendants("point"))
                {
                    var x = point.Attribute("x").Value.ParseDouble();
                    var y = point.Attribute("y").Value.ParseDouble();
                    pnts.Add(new Vector2d(x, y));
                }
                lsh.Lines.Add(new Line2D() { Start = pnts[0], End = pnts[1] });
            }
            return lsh;
        }

        HelperItem loadMesh(XElement el)
        {

            List<Vector2d[]> trs = new List<Vector2d[]>();
            foreach (var tr in el.Descendants("triangle"))
            {
                List<Vector2d> pnts = new List<Vector2d>();
                foreach (var pitem in tr.Elements("point"))
                {
                    var x = pitem.Attribute("x").Value.ParseDouble();
                    var y = pitem.Attribute("y").Value.ParseDouble();
                    pnts.Add(new Vector2d(x, y));
                }
                trs.Add(pnts.ToArray());
            }


            MeshHelper msh = new MeshHelper(trs.ToArray());

            return msh;
        }

        HelperItem loadLine(XElement el)
        {
            SegmentHelper lsh = new SegmentHelper();

            List<Vector2d> pnts = new List<Vector2d>();
            foreach (var point in el.Descendants("point"))
            {
                var x = point.Attribute("x").Value.ParseDouble();
                var y = point.Attribute("y").Value.ParseDouble();
                pnts.Add(new Vector2d(x, y));
            }
            lsh.Point = pnts[0];
            lsh.Point2 = pnts[1];
            return lsh;
        }

        HelperItem[] loadXml(string content)
        {
            List<HelperItem> ret = new List<HelperItem>();

            var doc = XDocument.Parse(content);
            var root = doc.Element("root");

            //todo: make recursive here
            foreach (var pitem in root.Elements("group"))
            {
                Group gr = new Group();
                foreach (var el in pitem.Elements())
                {
                    if (el.Name == "polyline")
                        gr.Items.Add(loadPolyline(el));
                    if (el.Name == "lineSet")
                        gr.Items.Add(loadLineSet(el));
                    if (el.Name == "mesh")
                        gr.Items.Add(loadMesh(el));
                    if (el.Name == "line")
                        gr.Items.Add(loadLine(el));
                    if (el.Name == "point")
                        gr.Items.Add(loadPoint(el));
                }
                ret.Add(gr);
            }
            foreach (var pitem in root.Elements("polyline"))
            {
                ret.Add(loadPolyline(pitem));
            }
            foreach (var pitem in root.Elements("point"))
            {
                ret.Add(loadPoint(pitem));
            }
            foreach (var pitem in root.Elements("lineSet"))
            {
                ret.Add(loadLineSet(pitem));
            }
            foreach (var pitem in root.Elements("line"))
            {
                ret.Add(loadLine(pitem));
            }
            foreach (var pitem in root.Elements("mesh"))
            {
                ret.Add(loadMesh(pitem));
            }
            foreach (var pitem in root.Elements("polygon"))
            {
                List<NFP> nfps = new List<NFP>();
                foreach (var item in pitem.Elements("region"))
                {
                    List<SvgPoint> pnts = new List<SvgPoint>();
                    foreach (var point in item.Descendants("point"))
                    {
                        var x = point.Attribute("x").Value.ParseDouble();
                        var y = point.Attribute("y").Value.ParseDouble();
                        pnts.Add(new SvgPoint(x, y));
                    }
                    nfps.Add(new NFP() { Points = pnts.Select(y => new SvgPoint(y.X, y.Y)).ToArray() });
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

                foreach (var item in nfps)
                {
                    if (item.Parent != null) continue;
                    PolygonHelper phh = new PolygonHelper();
                    ret.Add(phh);
                    phh.Polygon = item;
                }
            }
            return ret.ToArray();
        }

        void deleteItems()
        {
            if (treeListView1.SelectedObjects.Count == 0) return;
            if (dataModel.ParentForm.ShowQuestion($"Are you to sure to delete {treeListView1.SelectedObjects.Count} items?") == DialogResult.No) return;

            List<HelperItem> toDel = new List<HelperItem>();
            for (int i = 0; i < treeListView1.SelectedObjects.Count; i++)
            {
                toDel.Add(treeListView1.SelectedObjects[i] as HelperItem);
            }
            dataModel.RemoveItems(toDel.ToArray());

            UpdateList();
        }

        int lastSelectedIndex = -1;
        private void treeListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataModel.ClearSelection();

            if (treeListView1.SelectedObjects.Count == 0)            
                return;

            var sel = treeListView1.SelectedObjects.OfType<HelperItem>().ToArray();
            if (treeListView1.SelectedIndices.Count == 1)
            {
                lastSelectedIndex = treeListView1.SelectedIndices[0];
            }
            else if (treeListView1.SelectedIndices.Count == 2)
            {
                if (lastSelectedIndex == treeListView1.SelectedIndices[1])
                {
                    sel = sel.Reverse().ToArray();
                }
            }
            
            dataModel.ChangeSelectedItems(sel.ToArray());
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteItems();
        }

        private void segmentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new SegmentHelper() { Point2 = new Vector2d(10, 10) });
        }

        private void treeListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteItems();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataModel.Clear();
            UpdateList();
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

        private void extractChildsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            var hh = dataModel.SelectedItem;
            bool was = false;
            if ((hh is PolygonHelper ph))
            {

                List<PolygonHelper> pp = new List<PolygonHelper>();
                foreach (var item in ph.Polygon.Childrens)
                {
                    PolygonHelper ph2 = new PolygonHelper();
                    ph2.Polygon = DeepNest.clone(item);
                    pp.Add(ph2);
                }
                dataModel.AddItems(pp.ToArray());
                was = true;
            }
            else
            if ((hh is LinesSetHelper lsh))
            {
                List<SegmentHelper> pp = new List<SegmentHelper>();
                foreach (var item in lsh.Lines)
                {
                    SegmentHelper ph2 = new SegmentHelper();
                    ph2.Point = item.Start;
                    ph2.Point2 = item.End;
                    ph2.DrawArrowCap = lsh.DrawArrows;
                    ph2.ArrowZoomRelative = lsh.ArrowZoomRelative;
                    ph2.ArrowLen = lsh.ArrowLen;
                    ph2.ArrowAng = lsh.ArrowAng;

                    pp.Add(ph2);
                }
                dataModel.AddItems(pp.ToArray());
                was = true;
            }
            else
            if ((hh is MeshHelper mh))
            {
                List<PolygonHelper> pp = new List<PolygonHelper>();
                foreach (var item in mh.Mesh)
                {
                    PolygonHelper ph2 = new PolygonHelper();
                    ph2.Polygon.Points = item.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                    ph2.RecalcArea();
                    pp.Add(ph2);
                }
                dataModel.AddItems(pp.ToArray());
                was = true;
            }
            if (was && GuiHelpers.Question($"Remove parent item: {hh.Name}?", ParentForm.Text))
            {
                dataModel.RemoveItem(hh);
            }
        }

        void ImportDxf(string file)
        {
            var r = DxfParser.LoadDxf(file);

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
                phh.Name = new FileInfo(file).Name;
                ret.Add(phh);
                phh.Polygon = item;
            }

            dataModel.AddItems(ret.ToArray());
        }

        private void dxfFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "dxf files (*.dxf)|*.dxf";
            if (ofd.ShowDialog() != DialogResult.OK) 
                return;

            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                ImportDxf(ofd.FileNames[i]);
            }           
        }

        private void polygonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new PolygonHelper());
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new CircleGenerator();
            dataModel.AddItem(ph);
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new RectangleGenerator() { Changed = changed };
            dataModel.AddItem(ph);
            UpdateList();
        }

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new Group() { Changed = changed };
            dataModel.AddItem(ph);
            UpdateList();
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

        private void polylineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataModel.AddItem(new PolylineHelper());
        }

        private void linesSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lsh = new LinesSetHelper() { };
            dataModel.AddItem(lsh);
            lsh.Lines.Add(new Line2D() { Start = new Vector2d(0, 0), End = new Vector2d(100, 100) });
            lsh.Lines.Add(new Line2D() { Start = new Vector2d(100, 100), End = new Vector2d(100, 120) });
        }

        private void meshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Vector2d[]> tr = new List<Vector2d[]>
            {
                new Vector2d[] {
                new Vector2d(5,5),
                new Vector2d(15,15),
                new Vector2d(20,45),
            },
                new Vector2d[] {
                new Vector2d(55,35),
                new Vector2d(15,15),
                new Vector2d(20,45),
            }
            };
            var msh = new MeshHelper(tr.ToArray()) { };
            dataModel.AddItem(msh);
        }

        private void commandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var curp = treeListView1.PointToClient(contextMenuStrip1.Bounds.Location);
            var tt = treeListView1.GetItemAt(curp.X, curp.Y) as OLVListItem;
            if (tt == null)
                return;

            dataModel.SelectedItem = tt.RowObject as IHelperItem;
            commandsToolStripMenuItem.DropDownItems.Clear();

            if (dataModel.SelectedItem == null) return;
            ////var focusedItem = treeListView1.FocusedItem;
            var cc = dataModel.SelectedItem as ICommandsContainer;
            if (cc == null)
                return;

            List<HelperItem> all = new List<HelperItem>();
            for (int i = 0; i < treeListView1.SelectedObjects.Count; i++)
            {
                all.Add(treeListView1.SelectedObjects[i] as HelperItem);
            }

            foreach (var item in cc.Commands)
            {
                var ccc = new ToolStripMenuItem(item.Name);
                commandsToolStripMenuItem.DropDownItems.Add(ccc);
                ccc.Click += (s, ee) =>
                {
                    item.Process(new CommandContext(cc as HelperItem, all.Except(new[] { cc as HelperItem }).ToArray(), dataModel.ParentForm));
                };
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
            else if (hh is PolylineHelper plh)
            {
                dataModel.AddItem(plh.Clone());
            }
            else if (hh is PolygonHelper pl)
            {
                dataModel.AddItem(pl.Clone());
            }
        }

        private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                var maxz = dataModel.Items.Max(z => z.ZIndex);
                dataModel.SelectedItems[i].ZIndex = maxz + 1;
            }

            dataModel.UpdateList();
        }

        private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataModel.SelectedItems.Length == 0) return;
            for (int i = 0; i < dataModel.SelectedItems.Length; i++)
            {
                var minz = dataModel.Items.Min(z => z.ZIndex);
                dataModel.SelectedItems[i].ZIndex = minz - 1;
            }
        }
    }
}
