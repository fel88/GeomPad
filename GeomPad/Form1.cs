using ClipperLib;
using PolyBoolCS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Recreate();
            SizeChanged += Form1_SizeChanged;
            var vls = Enum.GetValues(typeof(JoinType));
            foreach (var item in vls)
            {
                comboBox1.Items.Add(new ComboBoxItem() { Tag = item, Name = item.ToString() });
            }
            dc.Init(pictureBox1);
        }
        public float ZoomFactor = 1.5f;
        public class ComboBoxItem
        {
            public string Name;
            public object Tag;
            public override string ToString()
            {
                return Name;
            }
        }


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

            //gr.ResetTransform();
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);
            gr.DrawString(dc.scale + "", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            gr.DrawString(back.X + "; " + back.Y, SystemFonts.DefaultFont, Brushes.Black, 0, 15);
            //gr.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            //gr.ScaleTransform(dc.scale, dc.scale);

            foreach (var item in Items.OrderBy(z => z.Selected))
            {
                item.Draw(dc);
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
                listView1.Items.Add(new ListViewItem(new string[] { item.GetType().Name, item.Name }) { Tag = item });
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
            propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
            (propertyGrid1.SelectedObject as HelperItem).Selected = true;
            selected = listView1.SelectedItems[0].Tag as HelperItem;

            UpdateList2();
        }

        HelperItem selected;

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
        }

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


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        public float zoom = 1;

        public Bitmap Bmp;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

        }

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

        private void button4_Click(object sender, EventArgs e)
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
        public static Polygon GetPolygon(PointF[] pnts)
        {
            var p = new Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }
        public static Polygon GetPolygon(SvgPoint[] pnts)
        {
            var p = new Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            var ar0 = Items.Where(z => z is PolygonHelper).Select(z => z as PolygonHelper).ToArray();
            if (ar0.Length < 2) { StatusMessage("there are no 2 polygon detected", StatusMessageType.Warning); return; }
            var ar1 = ar0.Take(2).ToArray();

            PolyBool pb = new PolyBool();

            var poly1 = GetPolygon(ar1[0].Polygon.Points.ToArray());
            var poly2 = GetPolygon(ar1[1].Polygon.Points.ToArray());
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

        private void loadXmlFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doc = XDocument.Parse(Clipboard.GetText());

            foreach (var item in doc.Descendants("region"))
            {
                PolygonHelper ph = new PolygonHelper();
                Items.Add(ph);
                List<SvgPoint> pnts = new List<SvgPoint>();
                foreach (var point in item.Descendants("point"))
                {
                    var x = float.Parse(point.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    var y = float.Parse(point.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    pnts.Add(new SvgPoint(x, y));
                }
                ph.Polygon.Points = pnts.ToArray();

            }
            UpdateList();
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
            PolygonHelper ph = new PolygonHelper();
            foreach (var item in ph2.Polygon.Childrens)
            {
                var offs2 = ClipperHelper.offset(item, -offset, jType, curveTolerance: curveTolerance, miterLimit: miterLimit);
                var nfp1 = new NFP();
                nfp1.Points = offs2.First().Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
                ph.Polygon.Childrens.Add(nfp1);
            }

          
            if (offs.Any())
            {
                ph.Polygon.Points = offs.First().Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            }

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
            if (listView1.SelectedItems.Count < 2) { StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return; }
            List<PolygonHelper> phhs = new List<PolygonHelper>();
            foreach (var item in listView1.SelectedItems)
            {
                phhs.Add((item as ListViewItem).Tag as PolygonHelper);
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
                        if (Helpers.pnpoly(d2.Points.ToArray(), f0.X, f0.Y))
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
    }
}