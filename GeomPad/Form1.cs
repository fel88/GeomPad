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
using System.Reflection;
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

            toolStripButton3.BackgroundImageLayout = ImageLayout.None;
            toolStripButton3.BackgroundImage = new Bitmap(1, 1);
            toolStripButton3.BackColor = Color.Transparent;

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

        bool drawAxis = true;

        DrawingContext dc = new DrawingContext();

        bool bubbleUpSelected = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            dc.UpdateDrag();
            gr.Clear(Color.White);

            if (drawAxis)
            {
                gr.DrawLine(Pens.Red, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(500, 0)));
                gr.DrawLine(Pens.Green, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(0, 500)));
            }



            //gr.ResetTransform();

            //gr.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            //gr.ScaleTransform(dc.scale, dc.scale);

            var ord = Items.OrderBy(z => z.Z);
            if (bubbleUpSelected)
            {
                ord = ord.ThenBy(z => z.Selected);
            }
            foreach (var item in ord)
            {
                item.Draw(dc);
            }
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
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);
            var str = $"X: {back.X,8:N2}";
            var str3 = $"Y: {back.Y,8:N2}";
            var str2 = $"X: 99999.999 ; Y: 99999.999";
            var msr = gr.MeasureString(str2, SystemFonts.DefaultFont);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), 3, 3, msr.Width, 35);
            gr.DrawString(dc.scale + "", SystemFonts.DefaultFont, Brushes.Black, 5, 5);


            gr.DrawString(str, SystemFonts.DefaultFont, Brushes.Black, 5, 20);
            gr.DrawString(str3, SystemFonts.DefaultFont, Brushes.Black, 70, 20);
            if (selected is PolygonHelper ph)
            {
                gr.DrawString($"{ph.Polygon.Points.Length} points", SystemFonts.DefaultFont, Brushes.Black, 5, 40);
            }
            if (selected is MeshHelper mh)
            {
                gr.DrawString($"{mh.TianglesCount} triangles", SystemFonts.DefaultFont, Brushes.Black, 5, 40);
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
        public void ShowMessage(string text, MessageBoxIcon type)
        {
            MessageBox.Show(text, Text, MessageBoxButtons.OK, type);
        }

        public DialogResult ShowQuestion(string text)
        {
            return MessageBox.Show(text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        void deleteItems()
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (ShowQuestion($"Are you to sure to delete {listView1.SelectedItems.Count} items?") == DialogResult.No) return;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                Items.Remove(listView1.SelectedItems[i].Tag as HelperItem);
            }

            UpdateList();
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteItems();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateList();
        }


        public float zoom = 1;

        public Bitmap Bmp;


        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {


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
        public static PolyBoolCS.Polygon GetPolygon(PolygonHelper ph)
        {
            var p = new PolyBoolCS.Polygon();
            p.regions = new List<PointList>();
            var pnts = ph.Polygon.Points.Select(z => ph.Transform(z)).ToArray();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            foreach (var item in ph.Polygon.Childrens)
            {

                pnts = item.Points.Select(z => ph.Transform(z)).ToArray();
                plist = GetPointList(pnts);
                p.regions.Add(plist);
            }
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

            //var poly1 = GetPolygon(ar1[0].TransformedPoints().ToArray());
            //var poly2 = GetPolygon(ar1[1].TransformedPoints().ToArray());
            var poly1 = GetPolygon(ar1[0]);
            var poly2 = GetPolygon(ar1[1]);
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
            Items.Add(new SegmentHelper() { Point2 = new Vector2d(10, 10) });
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

            var poly1 = GetPolygon(ar1[0]);
            foreach (var item in ar1.Skip(1))
            {
                poly1 = pb.difference(poly1, GetPolygon(item));
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
            Items.Add(new SegmentHelper() { Point2 = new Vector2d(10, 10) });
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
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlParseAttribute)) != null).ToArray();
            foreach (var item in root.Elements())
            {
                var fr = types.FirstOrDefault(z => (z.GetCustomAttribute(typeof(XmlParseAttribute)) as XmlParseAttribute).XmlKey == item.Name);
                if (fr != null)
                {
                    var ph = Activator.CreateInstance(fr) as HelperItem;
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            var res = getPairOfSelectedNfps();
            if (res == null) return;
            NFP offs = null;
            offs = ClipperHelper.MinkowskiSum(res[0], res[1], checkBox2.Checked, checkBox3.Checked);
            if (offs != null)
            {
                PolygonHelper ph = new PolygonHelper();
                //ph.Polygon.Points = offs.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon = DeepNest.clone2(offs);

                Items.Add(ph);
                UpdateList();
            }
        }

        private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var maxz = Items.Max(z => z.Z);
                (listView1.SelectedItems[i].Tag as HelperItem).Z = maxz + 1;
            }

            UpdateList();
        }

        private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var minz = Items.Min(z => z.Z);
                (listView1.SelectedItems[i].Tag as HelperItem).Z = minz - 1;
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


        NFP[] getPairOfSelectedNfps()
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            if (!checkBox1.Checked)
            {
                if (listView1.SelectedItems.Count < 2) { StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return null; }

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


            p.Points = ar1[0].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            foreach (var item in ar1[0].Polygon.Childrens)
            {
                if (p.Childrens == null)
                    p.Childrens = new List<NFP>();
                p.Childrens.Add(new NFP() { Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            }
            p2.Points = ar1[1].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            foreach (var item in ar1[1].Polygon.Childrens)
            {
                if (p2.Childrens == null)
                    p2.Childrens = new List<NFP>();
                p2.Childrens.Add(new NFP() { Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            }
            return new[] { p, p2 };
        }
        private void button14_Click(object sender, EventArgs e)
        {
            var res = getPairOfSelectedNfps();
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
                    Items.Add(ph);
                }

                UpdateList();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var res = getPairOfSelectedNfps();
            if (res == null) return;
            var p = res[0];
            var p2 = res[1];
            var offs = DeepNest.getOuterNfp(p, p2);

            if (offs != null)
            {

                PolygonHelper ph = new PolygonHelper();
                //ph.Polygon.Points = offs.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon = DeepNest.clone2(offs);

                Items.Add(ph);


                UpdateList();
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var res = getPairOfSelectedNfps();
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
                    Items.Add(ph);
                }

                UpdateList();
            }
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var hi = propertyGrid1.SelectedObject as HelperItem;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Tag == hi)
                {
                    listView1.Items[i].SubItems[1].Text = hi.Name;
                }
            }
        }



        private void button17_Click(object sender, EventArgs e)
        {
            List<SegmentHelper> ar1 = new List<SegmentHelper>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                if (listView1.SelectedItems[i].Tag is SegmentHelper ph1)
                {
                    ar1.Add(ph1);
                }
            }
            if (ar1.Count != 2) { StatusMessage("there are no 2 line segments selected", StatusMessageType.Warning); return; }

            Vector2d ret = Vector2d.Zero;
            if (!GeometryUtils.IntersectSegments(ar1[0].Point, ar1[0].Point2, ar1[1].Point, ar1[1].Point2, ref ret))
            {
                StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }

            PointHelper ph = new PointHelper();
            ph.Point = ret;
            Items.Add(ph);
            UpdateList();
        }
        bool editEnabled = false;
        private void toolStripButton3_Click_1(object sender, EventArgs e)
        {
            editEnabled = toolStripButton3.Checked;
            toolStripButton3.BackColor = editEnabled ? Color.LightGreen : Color.Transparent;
        }

        private void cloneToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var hh = listView1.SelectedItems[0].Tag as HelperItem;
            if (hh is SegmentHelper sh)
            {
                Items.Add(new SegmentHelper() { X = sh.X, Y = sh.Y, X2 = sh.X2, Y2 = sh.Y2 });
            }

            UpdateList();
        }

        private void randomizePointsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void fitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var hh = listView1.SelectedItems[0].Tag as HelperItem;
            List<PointF> pp = new List<PointF>();


            var rect = hh.BoundingBox();
            if (rect == null) return;
            pp.Add(rect.Value.Location);
            pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));

            if (pp.Count == 0) return;
            dc.FitToPoints(pp.ToArray(), 5);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            drawAxis = checkBox4.Checked;
        }

        private void extractChildsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var hh = listView1.SelectedItems[0].Tag as HelperItem;
            if (!(hh is PolygonHelper ph)) return;

            foreach (var item in ph.Polygon.Childrens)
            {
                PolygonHelper ph2 = new PolygonHelper();
                ph2.Polygon = DeepNest.clone(item);
                Items.Add(ph2);
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            bubbleUpSelected = true;
        }

        private void moveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var h = listView1.SelectedItems[0].Tag as HelperItem;
            VectorSetValuesDialog d = new VectorSetValuesDialog();
            d.ShowDialog();
            h.Shift(d.Vector.Xy);
        }

        public HelperItem[] SelectedHelpers
        {
            get
            {
                List<HelperItem> ret = new List<HelperItem>();
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    var hh = listView1.SelectedItems[i].Tag as HelperItem;
                    ret.Add(hh);
                }
                return ret.ToArray();
            }
        }
        public void calcOrthogonalFor2Polygons(bool xAxis = true)
        {
            var hh = SelectedHelpers.OfType<PolygonHelper>().ToArray();
            var p1 = hh[0] as PolygonHelper;
            var p2 = hh[1] as PolygonHelper;

            List<SegmentHelper> s1 = new List<SegmentHelper>();
            List<SegmentHelper> s2 = new List<SegmentHelper>();

            //skip all segments outside boundingBox

            var tr1 = p1.GetTrasformed(p1.Polygon);
            var tr2 = p2.GetTrasformed(p2.Polygon);
            for (int i = 1; i < tr1.Points.Length; i++)
            {
                var p00 = tr1.Points[i - 1];
                var p11 = tr1.Points[i];
                s1.Add(new SegmentHelper() { Point = new Vector2d(p00.X, p00.Y), Point2 = new Vector2d(p11.X, p11.Y) });
            }
            for (int i = 1; i < tr2.Points.Length; i++)
            {
                var p00 = tr2.Points[i - 1];
                var p11 = tr2.Points[i];
                s2.Add(new SegmentHelper() { Point = new Vector2d(p00.X, p00.Y), Point2 = new Vector2d(p11.X, p11.Y) });
            }

            double mindist = double.MaxValue;
            SegmentHelper mins1 = null;
            SegmentHelper minss1 = null;
            SegmentHelper minss2 = null;
            foreach (var seg1 in s1)
            {
                foreach (var seg2 in s2)
                {

                    SegmentHelper dist = null;
                    if (xAxis)
                        dist = OrthogonalDist2Segments.SegmentsXDist(seg1, seg2);
                    else
                        dist = OrthogonalDist2Segments.SegmentsYDist(seg1, seg2);

                    if (dist != null)
                    {
                        if (mindist > dist.Length)
                        {
                            minss1 = seg1;
                            minss2 = seg2;
                            mindist = dist.Length;
                            mins1 = dist;
                        }
                    }
                }
            }
            if (mins1 != null)
            {
                mins1.Reverse();
                double reqDist = (mins1.Point.X > mins1.Point2.X ? -1 : 1) * mins1.Length;
                if (!xAxis)
                {
                    reqDist = (mins1.Point.Y > mins1.Point2.Y ? -1 : 1) * mins1.Length;
                }
                Items.Add(new OrthogonalDist2Segments()
                {
                    Name = $"{p1.Name} --> {p2.Name}",
                    Segment1 = minss1.Clone(),
                    Segment2 = minss2.Clone(),
                    DistSegment = mins1
                ,
                    RequiredOffset = reqDist
                });
                //p1.OffsetX += (mins1.Point.X > mins1.Point2.X ? 1 : -1) * mins1.Length;

                UpdateList();
            }
            else
            {

                StatusMessage("no orthogonal intersection", StatusMessageType.Warning);
            }
        }
        private void button16_Click(object sender, EventArgs e)
        {
            calcOrthogonalFor2Polygons();

        }

        private void button18_Click(object sender, EventArgs e)
        {
            calcOrthogonalFor2Polygons(false);
        }
    }
}