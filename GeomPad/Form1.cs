using ClipperLib;
using GeomPad.Helpers;
using OpenTK;
using PolyBoolCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace GeomPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //hack
            toolStripButton2.BackgroundImageLayout = ImageLayout.None;
            toolStripButton2.BackgroundImage = new Bitmap(1, 1);
            toolStripButton2.BackColor = Color.Transparent;

            Recreate();
            SizeChanged += Form1_SizeChanged;
            var vls = Enum.GetValues(typeof(JoinType));
            foreach (var item in vls)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.ToString() });
            }
            comboBox1.SelectedIndex = Array.IndexOf(vls, JoinType.jtRound);
            dc.Init(pictureBox1);
            Load += Form1_Load;


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Recreate();
        }

        void Recreate()
        {
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            dc.gr = gr;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }

        Bitmap bmp;
        Graphics gr;
        List<HelperItem> Items = new List<HelperItem>();



        DrawingContext dc = new DrawingContext();
        private void timer1_Tick(object sender, EventArgs e)
        {
            dc.UpdateDrag();
            gr.Clear(Color.White);

            gr.DrawLine(Pens.Red, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(500, 0)));
            gr.DrawLine(Pens.Green, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(0, 500)));

            if (dc.MiddleDrag)//measure tool
            {
                Pen pen = new Pen(Color.Blue, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

                var curp = dc.Transform(dc.GetCursor());
                var t = dc.Transform(new PointF(dc.startx, dc.starty));
                gr.DrawLine(pen, dc.startx, dc.starty, curp.X, curp.Y);
                var pp = dc.BackTransform(new PointF(dc.startx, dc.starty));
                Vector2 v1 = new Vector2(pp.X, pp.Y);
                Vector2 v2 = new Vector2(dc.GetCursor().X, dc.GetCursor().Y);
                var dist = (v2 - v1).Length;
                gr.DrawString(dist.ToString("N2"), SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);
            }

            //gr.ResetTransform();

            //gr.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            //gr.ScaleTransform(dc.scale, dc.scale);

            foreach (var item in Items.OrderBy(z => z.Selected))
            {
                item.Draw(dc);
            }
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), 0, 0, 120, 45);
            gr.DrawString(dc.scale + "", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            gr.DrawString(back.X + "; " + back.Y, SystemFonts.DefaultFont, Brushes.Black, 0, 15);
            if (selected is PolygonHelper ph)
            {
                gr.DrawString($"{ph.Polygon.Points.Length} points", SystemFonts.DefaultFont, Brushes.Black, 0, 30);
            }
            if (selected is MeshHelper mh)
            {
                gr.DrawString($"{mh.TianglesCount} triangles", SystemFonts.DefaultFont, Brushes.Black, 0, 30);
            }
            pictureBox1.Image = bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Items.Add(new PointHelper());
            UpdateList();
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in Items)
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
            UpdateList2();


        }
        public void UpdateList2()
        {
            listView2.Items.Clear();
            if (!(selected is PolygonHelper ph)) return;
            for (int i = 0; i < ph.Polygon.Points.Length; i++)
            {
                listView2.Items.Add(new ListViewItem(new string[] { ph.Polygon.Points[i].X + "", ph.Polygon.Points[i].Y + "" })
                {
                    Tag = new PolygonPointEditorWrapper(ph.Polygon, i)
                });
            }
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Items.ForEach(z => z.Selected = false);

            if (listView1.SelectedItems.Count == 0) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                (listView1.SelectedItems[i].Tag as HelperItem).Selected = true;
            }

            propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
            selected = listView1.SelectedItems[0].Tag as HelperItem;
            UpdateList2();
        }

        HelperItem selected;

        private void button3_Click(object sender, EventArgs e)
        {
            dc.scale = float.Parse(textBox1.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                Items.Remove(listView1.SelectedItems[i].Tag as HelperItem);
            }

            UpdateList();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList();
        }


        public float zoom = 1;

        public Bitmap Bmp;
        

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var hh = listView1.SelectedItems[0].Tag as HelperItem;
            if (hh is SegmentHelper sh)
            {
                Items.Add(new SegmentHelper() { X = sh.X, Y = sh.Y, X2 = sh.X2, Y2 = sh.Y2 });
            }

            UpdateList();

        }

        
        private void button5_Click(object sender, EventArgs e)
        {
            if (selected is PolygonHelper ph)
            {
                var list = ph.Polygon.Points.ToList();
                list.Add(new SvgPoint(0, 0));
                ph.Polygon.Points = list.ToArray();
            }
            UpdateList();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            if (!(selected is PolygonHelper)) return;
            var hh = listView2.SelectedItems[0].Tag;
            propertyGrid1.SelectedObject = hh;
            //(selected as PolygonHelper).Polygon

        }

        public static PointList GetPointList(PointF[] pnts)
        {
            PointList plist = new PointList();

            foreach (var blankPoint in pnts)
            {
                plist.Add(new PolyBoolCS.Point(blankPoint.X, blankPoint.Y));
            }

            return plist;
        }
        public static PointList GetPointList(SvgPoint[] pnts)
        {
            PointList plist = new PointList();
            foreach (var blankPoint in pnts)
            {
                plist.Add(new PolyBoolCS.Point(blankPoint.X, blankPoint.Y));
            }

            return plist;
        }

        public static PolyBoolCS.Polygon GetPolygon(PointF[] pnts)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }

        public static PolyBoolCS.Polygon GetPolygon(SvgPoint[] pnts)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> ar1 = new List<PolygonHelper>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                if (listView1.SelectedItems[i].Tag is PolygonHelper ph1)
                {
                    ar1.Add(ph1);
                }
            }
            if (ar1.Count != 2) { StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

            PolyBool pb = new PolyBool();

            var poly1 = GetPolygon(ar1[0].TransformedPoints().ToArray());
            var poly2 = GetPolygon(ar1[1].TransformedPoints().ToArray());
            var r = pb.intersect(poly1, poly2);
            if (r.regions.Count == 0)
            {
                StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }
            var pnts = r.regions.ToArray()[0].ToArray();
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon.Points = pnts.Select(z => new SvgPoint(z.x, z.y)).ToArray();
            Items.Add(ph);
            UpdateList();
        }

        public enum StatusMessageType
        {
            Message, Warning, Error, Info
        }
        private void StatusMessage(string v, StatusMessageType type)
        {
            toolStripStatusLabel1.Text = v;
            switch (type)
            {
                case StatusMessageType.Warning:
                    toolStripStatusLabel1.BackColor = Color.Yellow;
                    toolStripStatusLabel1.ForeColor = Color.Blue;
                    break;
                case StatusMessageType.Error:
                    toolStripStatusLabel1.BackColor = Color.Red;
                    toolStripStatusLabel1.ForeColor = Color.White;
                    break;
                case StatusMessageType.Info:
                    toolStripStatusLabel1.BackColor = Color.Blue;
                    toolStripStatusLabel1.ForeColor = Color.White;
                    break;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<PointF> pp = new List<PointF>();
            foreach (var item in Items)
            {
                var rect = item.BoundingBox();
                if (rect == null) continue;
                pp.Add(rect.Value.Location);
                pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));
            }
            if (pp.Count == 0) return;
            dc.FitToPoints(pp.ToArray(), 5);
        }

        Random rand = new Random();
        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            var ppw = listView2.SelectedItems[0].Tag as PolygonPointEditorWrapper;
            ppw.X = rand.Next(-100, 300);
            ppw.Y = rand.Next(-100, 300);
        }

        private void randomToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var hh = listView1.SelectedItems[0].Tag as HelperItem;
            if (hh is PolygonHelper ph)
            {
                for (int i = 0; i < ph.Polygon.Points.Length; i++)
                {
                    ph.Polygon.Points[i] = new SvgPoint(rand.Next(-100, 100), rand.Next(-100, 100));
                }
            }
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
        private void loadXmlFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {


        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Items.Add(new PointHelper());
            UpdateList();
        }

        private void segmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Items.Add(new SegmentHelper() { Point2 = new PointF(10, 10) });
            UpdateList();
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Items.Add(new PolygonHelper());
            UpdateList();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            NFP p = new NFP();
            if (!(selected is PolygonHelper ph2)) { return; }

            p.Points = ph2.Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            var jType = (JoinType)comboBox1.SelectedIndex;
            double offset = double.Parse(textBox2.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double miterLimit = double.Parse(textBox3.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double curveTolerance = double.Parse(textBox4.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            var offs = ClipperHelper.offset(p, offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
            //if (offs.Count() > 1) throw new NotImplementedException();
            PolygonHelper ph = new PolygonHelper();
            foreach (var item in ph2.Polygon.Childrens)
            {
                var offs2 = ClipperHelper.offset(item, -offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                var nfp1 = new NFP();
                if (offs2.Any())
                {
                    if (offs2.Count() > 1) throw new NotImplementedException();
                    nfp1.Points = offs2.First().Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                    ph.Polygon.Childrens.Add(nfp1);
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
            Items.Add(ph);
            UpdateList();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Items.Clear();
            UpdateList();
        }

        private void polygonCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new CircleGenerator();
            Items.Add(ph);
            /*            
                        int rad = 100;
                        List<SvgPoint> pnts = new List<SvgPoint>();
                        for (int i = 0; i < 360; i+=15)
                        {
                            var ang = i / 180f * Math.PI;
                            var x = rad * Math.Cos(ang);
                            var y = rad * Math.Sin(ang);
                            pnts.Add(new SvgPoint(x, y));
                        }
                        ph.Polygon.Points = pnts.ToArray();*/
            UpdateList();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            if (!checkBox1.Checked)
            {
                if (listView1.SelectedItems.Count < 2) { StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

                foreach (var item in listView1.SelectedItems)
                {
                    phhs.Add((item as ListViewItem).Tag as PolygonHelper);
                }
            }
            else
            {
                phhs.Add((comboBox2.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
                phhs.Add((comboBox3.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
            }

            var ar1 = phhs.ToArray();

            PolyBool pb = new PolyBool();

            var poly1 = GetPolygon(ar1[0].Polygon.Points.ToArray());
            foreach (var item in ar1.Skip(1))
            {
                poly1 = pb.difference(poly1, GetPolygon(item.Polygon.Points.ToArray()));
            }

            if (poly1.regions.Count == 0)
            {
                StatusMessage("no intersections", StatusMessageType.Warning);
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
                Items.Add(phh);
                phh.Polygon = item;
            }
            UpdateList();
        }


        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!(selected is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(new NFP() { Points = ph2.TransformedPoints() });
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;
            ph.Name = "hull";
            Items.Add(ph);
            UpdateList();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!(selected is PolygonHelper ph2)) { return; }

            double clipperScale = 10000000;
            var hull = DeepNest.simplifyFunction(new NFP() { Points = ph2.TransformedPoints() }, false, clipperScale);
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;
            ph.Name = "simplify";

            Items.Add(ph);
            UpdateList();
        }

        private void pointToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Items.Add(new PointHelper());
            UpdateList();
        }

        private void polygonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Items.Add(new PolygonHelper() { Changed = changed });
            UpdateList();
        }

        private void segmentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Items.Add(new SegmentHelper() { Point2 = new PointF(10, 10) });
            UpdateList();
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new CircleGenerator() { Changed = changed };
            Items.Add(ph);
            UpdateList();
        }

        private void addPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected is PolygonHelper ph)
            {
                var list = ph.Polygon.Points.ToList();
                list.Add(new SvgPoint(0, 0));
                ph.Polygon.Points = list.ToArray();
            }
            UpdateList();
        }


        private void button5_Click_1(object sender, EventArgs e)
        {

            var poly2 = new TriangleNet.Geometry.Polygon();
            var plh = selected as PolygonHelper;

            //foreach (var item in plh.Polygon.Points)
            {
                var pn = plh.TransformedPoints();
                if (StaticHelpers.signed_area(pn) < 0) { pn = pn.Reverse().ToArray(); }
                var a = pn.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();

                poly2.Add(new Contour(a));
            }

            var rev = plh.Polygon.Childrens.ToArray();
            rev = rev.Reverse().ToArray();
            foreach (var item in rev)
            {
                var pn = item.Points.Select(z => plh.Transform(z)).ToArray();
                var ar = StaticHelpers.signed_area(pn);
                /*if (StaticHelpers.signed_area(pn) > 0) 
                {
                    pn = pn.Reverse().ToArray(); 
                }*/
                var a = pn.Select(z => new Vertex(z.X, z.Y, 0)).ToArray();
                var p0 = plh.Transform(item.Points[0]);
                PointF test = new PointF((float)p0.X, (float)p0.Y);
                var maxx = pn.Max(z => z.X);
                var minx = pn.Min(z => z.X);
                var maxy = pn.Max(z => z.Y);
                var miny = pn.Min(z => z.Y);

                var tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
                var ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
                while (true)
                {
                    if (StaticHelpers.pnpoly(pn, test.X, test.Y))
                    {
                        break;
                    }

                    tx = rand.Next((int)(minx * 100),
                       (int)(maxx * 100)) / 100f;
                    ty = rand.Next((int)(miny * 100),
                       (int)(maxy * 100)) / 100f;
                    test = new PointF(tx, ty);
                }

                poly2.Add(new Contour(a), new TriangleNet.Geometry.Point(test.X, test.Y));
                //poly2.Add(new Contour(a), true);
            }

            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions(), new QualityOptions());

            var tr = trng.Triangles.Select(z => new PointF[] {
                  new PointF((float)z.GetVertex(0).X, (float)z.GetVertex(0).Y),
                  new PointF((float)z.GetVertex(1).X, (float)z.GetVertex(1).Y),
                  new PointF((float)z.GetVertex(2).X, (float)z.GetVertex(2).Y)
            }).ToArray();
            Items.Add(new MeshHelper(tr) { Name = "triangulate", Changed = () => { UpdateList(); } });
            UpdateList();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var em = e as MouseEventArgs;
            if (em.Button != MouseButtons.Right)
            {
                Items.ForEach(z => z.Selected = false);
                selected = null;
            }
        }

        void changed()
        {
            UpdateList();
        }
        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new RectangleGenerator() { Changed = changed };
            Items.Add(ph);
            UpdateList();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            foreach (var item in Items)
            {
                comboBox2.Items.Add(new ComboBoxItem() { Name = $"{item.Name ?? string.Empty} ({item.GetType().Name})", Tag = item });
            }
        }

        private void comboBox3_DropDown(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();
            foreach (var item in Items)
            {
                comboBox3.Items.Add(new ComboBoxItem() { Name = $"{item.Name ?? string.Empty} ({item.GetType().Name})", Tag = item });
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = checkBox1.Checked;
            comboBox3.Enabled = checkBox1.Checked;
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            var ppw = listView2.SelectedItems[0].Tag as PolygonPointEditorWrapper;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Xml files (*.xml)|*.xml";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            foreach (var item in Items)
            {
                item.AppendToXml(sb);
            }
            sb.AppendLine("</root>");
            File.WriteAllText(sfd.FileName, sb.ToString());
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml files|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var ret = loadXml(File.ReadAllText(ofd.FileName));
                Items.AddRange(ret);
                var fin = new FileInfo(ofd.FileName);
                foreach (var item in ret)
                {
                    item.Name = fin.Name;
                }
                UpdateList();
                StatusMessage("succesfully loaded.", StatusMessageType.Info);

            }
            catch (Exception ex)
            {
                StatusMessage(ex.Message, StatusMessageType.Error);
            }
        }

        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var hh = loadXml(Clipboard.GetText());
                Items.AddRange(hh);
                UpdateList();
                StatusMessage("succesfully loaded.", StatusMessageType.Info);

            }
            catch (Exception ex)
            {
                StatusMessage(ex.Message, StatusMessageType.Error);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Xml files (*.xml)|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var doc = XDocument.Load(ofd.FileName);
            var root = doc.Descendants("root").First();
            Items.Clear();
            foreach (var item in root.Elements())
            {
                if (item.Name == "polygonHelper")
                {
                    var ph = new PolygonHelper();
                    ph.ParseXml(item);
                    Items.Add(ph);
                }
            }
            UpdateList();
        }
        bool snapEnable = false;
        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            snapEnable = toolStripButton2.Checked;
            toolStripButton2.BackColor = snapEnable ? Color.LightGreen : Color.Transparent;
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

            Items.AddRange(ret);
            foreach (var item in ret)
            {
                item.Changed = () => { UpdateList(); };
            }
            UpdateList();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            if (!checkBox1.Checked)
            {
                if (listView1.SelectedItems.Count < 2) { StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }

                foreach (var item in listView1.SelectedItems)
                {
                    phhs.Add((item as ListViewItem).Tag as PolygonHelper);
                }
            }
            else
            {
                phhs.Add((comboBox2.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
                phhs.Add((comboBox3.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
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


            Items.Add(ph);
            UpdateList();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (!(selected is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(ph2.TransformedNfp());
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;
            var box = ph.BoundingBox().Value;
            PolygonHelper ph3 = new PolygonHelper();
            ph3.Polygon = new NFP()
            {
                Points = new SvgPoint[] {
                    new SvgPoint (box.Left,box.Top),
                    new SvgPoint (box.Left,box.Bottom),
                    new SvgPoint (box.Right,box.Bottom),
                    new SvgPoint (box.Right,box.Top),
            }
            };

            ph3.Name = "AABB";
            Items.Add(ph3);
            UpdateList();
        }



        private void button10_Click(object sender, EventArgs e)
        {
            if (!(selected is PolygonHelper ph2)) { return; }

            var hull = DeepNest.getHull(new NFP() { Points = ph2.TransformedPoints().ToArray() });
            PolygonHelper ph = new PolygonHelper();
            ph.Polygon = hull;

            var mar = GeometryUtil.GetMinimumBox(hull.Points.Select(z => new Vector2d(z.X, z.Y)).ToArray());
            PolygonHelper ph3 = new PolygonHelper();

            ph3.Polygon = new NFP()
            {
                Points = mar.Select(z => new SvgPoint(z.X, z.Y)).ToArray()
            };

            ph3.Name = "minRect";
            Items.Add(ph3);
            UpdateList();
        }

        private void button11_Click(object sender, EventArgs e)
        {

            if (!(selected is PolygonHelper ph2)) { return; }
            var c = ph2.CenterOfMass();
            //ph2.OffsetX = -c.X;
            //ph2.OffsetY = -c.Y;
            ph2.Translate(-c);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}