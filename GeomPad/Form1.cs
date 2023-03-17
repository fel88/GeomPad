using GeomPad.Common;
using GeomPad.Controls._2d;
using GeomPad.Helpers;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad
{
    public partial class Form1 : Form, IPadContainer
    {
        Pad2DDataModel dataModel;

        Pad2DMainPanel dpanel;
        List<IHelperItem> Items => dataModel.Items;

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;
            dataModel = new Pad2DDataModel() { ParentForm = this };
            //dataModel.OnListUpdated += () => { UpdateList(); };

            dpanel = new Pad2DMainPanel();

            tableLayoutPanel1.Controls.Add(dpanel, 0, 0);
            dpanel.Dock = DockStyle.Fill;

            dpanel.view.PictureBox.Click += pictureBox1_Click;


            dpanel.Init(dataModel);

            //hack
            toolStripButton2.BackgroundImageLayout = ImageLayout.None;
            toolStripButton2.BackgroundImage = new Bitmap(1, 1);
            toolStripButton2.BackColor = Color.Transparent;

            toolStripButton3.BackgroundImageLayout = ImageLayout.None;
            toolStripButton3.BackgroundImage = new Bitmap(1, 1);
            toolStripButton3.BackColor = Color.Transparent;

            dc.Init(dpanel.view.PictureBox);
            dpanel.view.PictureBox.Paint += PictureBox_Paint;
            Load += Form1_Load;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            dc.gr = e.Graphics;
            Render();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dpanel.SaveAsXml("layout2d.xml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;

        bool drawAxis => dataModel.drawAxis;

        DrawingContext dc => dataModel.dc;

        bool bubbleUpSelected => dataModel.bubbleUpSelected;
        private void timer1_Tick(object sender, EventArgs e)
        {
            dpanel.view.PictureBox.Invalidate();
        }

        public void Render()
        {
            var gr = dc.gr;
            dc.UpdateDrag();
            gr.Clear(Color.White);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var pos = dpanel.view.PictureBox.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);

            if (drawAxis)
            {
                gr.DrawLine(Pens.Red, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(500, 0)));
                gr.DrawLine(Pens.Green, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(0, 500)));
            }

            if (dc.SnapEnable)
            {
                double mindist = double.MaxValue;
                Vector2d? minp = null;
                //get all items
                var items = GetAllItems();
                foreach (var hitem in items)
                {
                    if (!hitem.Visible) continue;

                    if (hitem is PolygonHelper ph2)
                    {
                        var trans = ph2.GetTrasformed(ph2.Polygon);
                        foreach (var pp in trans)
                        {
                            foreach (var item in pp.Points)
                            {
                                double dist = ((new Vector2d(item.X, item.Y)) - new Vector2d(back.X, back.Y)).Length;
                                if (dist < mindist)
                                {
                                    mindist = dist;
                                    minp = new Vector2d(item.X, item.Y);
                                }
                            }
                        }
                    }
                    if (hitem is LinesSetHelper lsh)
                    {
                        foreach (var pp in lsh.Lines)
                        {
                            foreach (var item in new[] { pp.Start, pp.End })
                            {
                                double dist = ((new Vector2d(item.X, item.Y)) - new Vector2d(back.X, back.Y)).Length;
                                if (dist < mindist)
                                {
                                    mindist = dist;
                                    minp = item;
                                }
                            }
                        }
                    }
                    if (hitem is PolylineHelper plh)
                    {
                        foreach (var item in plh.Points)
                        {
                            double dist = ((new Vector2d(item.X, item.Y)) - new Vector2d(back.X, back.Y)).Length;
                            if (dist < mindist)
                            {
                                mindist = dist;
                                minp = item;
                            }
                        }
                    }
                    if (hitem is PointHelper pnh)
                    {
                        double dist = ((new Vector2d(pnh.Point.X, pnh.Point.Y)) - new Vector2d(back.X, back.Y)).Length;
                        if (dist < mindist)
                        {
                            mindist = dist;
                            minp = pnh.Point;
                        }
                    }
                }
                if (minp != null)
                {
                    dc.SnapPoint = minp;

                    int snapW = 10;
                    var trrrr = dc.Transform(minp.Value);
                    var dist1 = (new Vector2d(minp.Value.X, minp.Value.Y) - new Vector2d(back.X, back.Y)).Length;
                    if (dist1 > 3)
                    {
                        dc.SnapPoint = null;
                    }
                    else
                    {
                        toolStripStatusLabel2.Text = "snapped point: " + minp.Value.X + "; " + minp.Value.Y;
                        gr.DrawRectangle(Pens.Blue, trrrr.X - snapW / 2, trrrr.Y - snapW / 2, snapW, snapW);
                    }

                }
            }
            else
            {
                toolStripStatusLabel2.Text = string.Empty;
            }

            //gr.ResetTransform();

            //gr.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            //gr.ScaleTransform(dc.scale, dc.scale);

            var ord = Items.OrderBy(z => z.ZIndex);
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

                if (dc.SnapEnable && dc.SnapPoint != null)
                {
                    var trsp = dc.Transform(dc.SnapPoint.Value);
                    curp = new PointF(trsp.X, trsp.Y);
                }
                gr.DrawLine(pen, dc.startx, dc.starty, curp.X, curp.Y);
                //var pp = dc.BackTransform(new PointF(dc.startx, dc.starty));
                //Vector2 v1 = new Vector2(pp.X, pp.Y);
                var v1 = dc.startReal;

                Vector2d v2 = new Vector2d(curp.X, curp.Y);
                var v22 = dc.BackTransform(v2.X, v2.Y);
                double dist = 0;
                if (dc.SnapPoint != null)
                {
                    dist = (v1 - dc.SnapPoint.Value).Length;
                }
                else
                {
                    var pp = dc.BackTransform(new PointF(dc.startx, dc.starty));
                    v1 = new Vector2d(pp.X, pp.Y);
                    v2 = new Vector2d(dc.GetCursor().X, dc.GetCursor().Y);
                    dist = (v2 - v1).Length;
                }
                gr.DrawString(dist.ToString("N2"), SystemFonts.DefaultFont, Brushes.Black, curp.X + 10, curp.Y);
            }

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

            //dpanel.view.PictureBox.Image = bmp;
        }
        private IHelperItem[] GetAllItems(IHelperItem h = null, List<IHelperItem> list = null)
        {
            if (list == null)
            {
                list = new List<IHelperItem>();
            }

            if (h != null)
                list.Add(h);

            if (h == null)
            {
                foreach (var item in Items)
                {
                    GetAllItems(item, list);
                }
            }
            else if (h is Group g)
            {
                foreach (var item in g.Items)
                {
                    GetAllItems(item, list);
                }
            }

            return list.ToArray();
        }

        IHelperItem selected => dataModel.SelectedItem;

        public float zoom = 1;

        public Bitmap Bmp;

        public void StatusMessage(string v, StatusMessageType type)
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


        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (listView2.SelectedItems.Count == 0) return;
            var ppw = listView2.SelectedItems[0].Tag as PolygonPointEditorWrapper;
            ppw.X = rand.Next(-100, 300);
            ppw.Y = rand.Next(-100, 300);*/
        }



        private void addPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected is PolygonHelper ph)
            {
                var list = ph.Polygon.Points.ToList();
                list.Add(new SvgPoint(0, 0));
                ph.Polygon.Points = list.ToArray();
            }
            //UpdateList();
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var em = e as MouseEventArgs;
            if (em.Button != MouseButtons.Right)
            {
                Items.ForEach(z => z.Selected = false);
                dataModel.ClearSelection();
            }
        }



        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //if (listView2.SelectedItems.Count == 0) return;
            // var ppw = listView2.SelectedItems[0].Tag as PolygonPointEditorWrapper;

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



        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Xml files (*.xml)|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var doc = XDocument.Load(ofd.FileName);
            var root = doc.Descendants("root").First();
            Items.Clear();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlParseAttribute), false) != null).ToArray();
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
            dataModel.UpdateList();
        }

        public void UpdateHelpersList()
        {
            dataModel.UpdateList();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            dc.SnapEnable = toolStripButton2.Checked;
            toolStripButton2.BackColor = dc.SnapEnable ? Color.LightGreen : Color.Transparent;
        }


        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            /*var hi = propertyGrid1.SelectedObject as HelperItem;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Tag == hi)
                {
                    listView1.Items[i].SubItems[1].Text = hi.Name;
                }
            }*/
        }


        bool editEnabled = false;
        private void toolStripButton3_Click_1(object sender, EventArgs e)
        {
            editEnabled = toolStripButton3.Checked;
            toolStripButton3.BackColor = editEnabled ? Color.LightGreen : Color.Transparent;
        }



        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dpanel.ShowDebug();
        }

        private void nfpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dpanel.ShowNfp();

        }

        public void OpenChildWindow(Form f)
        {
            f.MdiParent = MdiParent;
            f.Show();
        }

        public void AddHelper(IHelperItem h)
        {
            dataModel.AddItem(h);
        }

        public void AddHelpers(IEnumerable<IHelperItem> h)
        {
            throw new NotImplementedException();
        }

        public void SetStatus(string v, StatusMessageType type)
        {
            StatusMessage(v, type);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
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

        private void fitSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<PointF> pp = new List<PointF>();
            foreach (var item in dataModel.SelectedItems)
            {
                var rect = item.BoundingBox();
                if (rect == null) continue;
                pp.Add(rect.Value.Location);
                pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));
            }
            if (pp.Count == 0) return;
            dc.FitToPoints(pp.ToArray(), 5);
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {

        }

        private void editorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor2D ss = new ScriptEditor2D();
            ss.Init(dataModel);
            ss.MdiParent = MdiParent;
            ss.Show();
        }

        private void runToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            runToolStripMenuItem.DropDownItems.Clear();
            foreach (var item in Stuff.Scripts)
            {
                var v = new ToolStripMenuItem(item.Name) { Tag = item };
                v.Click += V_Click;
                runToolStripMenuItem.DropDownItems.Add(v);
            }

        }

        private void V_Click(object sender, EventArgs e)
        {
            var r = (sender as ToolStripMenuItem).Tag as ScriptRunInfo;
            r.Script.Run(dataModel, this);
        }
    }
}