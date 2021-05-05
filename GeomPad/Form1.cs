using PolyBoolCS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            dc.Init(pictureBox1);
        }
        public float ZoomFactor = 1.5f;



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
            for (int i = 0; i < ph.Points.Count; i++)
            {
                listView2.Items.Add(new ListViewItem(new string[] { ph.Points[i].X + "", ph.Points[i].Y + "" }) { Tag = new PolygonPointEditorWrapper(ph, i) });
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
            Items.Remove(listView1.SelectedItems[0].Tag as HelperItem);
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
                ph.Points.Add(new PointF());
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
        public static Polygon GetPolygon(PointF[] pnts)
        {
            var p = new Polygon();
            p.regions = new List<PointList>();
            PointList plist = GetPointList(pnts);
            p.regions.Add(plist);
            return p;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            var ar1 = Items.Where(z => z is PolygonHelper).Select(z => z as PolygonHelper).Take(2).ToArray();

            PolyBool pb = new PolyBool();

            var poly1 = GetPolygon(ar1[0].Points.ToArray());
            var poly2 = GetPolygon(ar1[1].Points.ToArray());
            var r = pb.intersect(poly1, poly2);
            if (r.regions.Count == 0)
            {
                StatusMessage("no intersections", StatusMessageType.Warning);
                return;
            }
            var pnts = r.regions.ToArray()[0].ToArray();
            PolygonHelper ph = new PolygonHelper();
            ph.Points = pnts.Select(z => new PointF((float)z.x, (float)z.y)).ToList();
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
                for (int i = 0; i < ph.Points.Count; i++)
                {
                    ph.Points[i] = new PointF(rand.Next(-100, 100), rand.Next(-100, 100));
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
                foreach (var point in item.Descendants("point"))
                {
                    var x = float.Parse(point.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    var y = float.Parse(point.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    ph.Points.Add(new PointF(x, y));
                }
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
    }

    public class PolygonPointEditorWrapper
    {
        public PolygonPointEditorWrapper(PolygonHelper ph, int index)
        {
            this._index = index;
            _polygon = ph;
        }
        public PointF Point;
        public float X { get => _polygon.Points[_index].X; set => _polygon.Points[_index] = new PointF(value, _polygon.Points[_index].Y); }
        public float Y { get => _polygon.Points[_index].Y; set => _polygon.Points[_index] = new PointF(_polygon.Points[_index].X, value); }
        int _index;
        PolygonHelper _polygon;
    }

    public abstract class HelperItem
    {
        public int Z { get; set; }
        public bool Visible { get; set; } = true;
        public string Name { get; set; }
        public bool Selected;
        public abstract void Draw(DrawingContext gr);
    }

    public static class Helper
    {
        public static float ParseFloat(this string f)
        {
            return float.Parse(f.Replace(",", "."), CultureInfo.InvariantCulture);
        }
    }
}
