using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeomPad.Helpers3D.BRep
{
    public partial class ProjectMapEditor : Form
    {
        public ProjectMapEditor()
        {
            InitializeComponent();
            Recreate();
            SizeChanged += Form1_SizeChanged;
            dc.Init(pictureBox1);
            dc.MouseUp += Dc_MouseUp;
            Load += Form1_Load;
            FormClosing += ProjectMapEditor_FormClosing;
        }

        private void ProjectMapEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<ProjectPolygon> pp = new List<ProjectPolygon>();
            foreach (var item in Polygons)
            {
                var poly = new ProjectPolygon();
                pp.Add(poly);
                poly.Points = item.Points.Select(z => z).ToList();
                for (int i = 0; i < poly.Points.Count; i++)
                {
                    var t1 = poly.Points[i];
                    poly.Points[i] = new Vector2d(t1.X / xxScale, t1.Y / 100);
                }
            }
            face.UpdateMesh(pp.ToArray());
        }

        List<ProjectPolygon> polygons = new List<ProjectPolygon>();
        public ProjectPolygon[] Polygons
        {
            get
            {
                return polygons.ToArray();
            }
        }
        float boundY(float y)
        {
            return Math.Max(Math.Min(y, 100), 0);
        }
        private void Dc_MouseUp(float arg1, float arg2, MouseButtons b)
        {
            if (addPolygonMode && b == MouseButtons.Left)
                newPolygonPoints.Add(new Vector2(arg1, boundY(arg2)));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;
        DrawingContext dc = new DrawingContext();

        AbstractBRepFaceHelper face;
        internal void Init(AbstractBRepFaceHelper tr)
        {
            face = tr;
            if (face.ProjectPolygons != null)
            {
                polygons = face.ProjectPolygons.Select(z => z.Clone()).ToList();
                foreach (var item in polygons)
                {
                    item.Scale(xxScale, yyScale);
                }
            }

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            Recreate();
        }
        Bitmap bmp;
        Graphics gr;
        void Recreate()
        {
            bmp = new Bitmap(Width, Height);
            gr = Graphics.FromImage(bmp);
            dc.gr = gr;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }
        double xxScale = 20;
        double yyScale = 100;
        private void timer1_Tick(object sender, EventArgs e)
        {
            dc.UpdateDrag();
            gr.Clear(Color.White);

            gr.DrawLine(Pens.Red, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(500, 0)));
            gr.DrawLine(Pens.Green, dc.Transform(new PointF(0, 0)), dc.Transform(new PointF(0, 500)));

            Pen p = new Pen(Color.Gray);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            p.DashPattern = new float[] { 4, 2, 2, 4 };
            gr.DrawLine(p, dc.Transform(-2000, 100), dc.Transform(2000, 100));
            gr.DrawLine(p, dc.Transform(-2000, 0), dc.Transform(2000, 0));
            for (int i = -10; i < 10; i++)
            {
                var x1 = (float)(i * Math.PI * 2 * xxScale);
                gr.DrawLine(p, dc.Transform(x1, 100), dc.Transform(x1, 0));
            }

            int w1 = 4;
            int w2 = 2;
            var temp1 = new List<ProjectPolygon>();
            temp1.AddRange(polygons);
            if (addPolygonMode)
            {
                temp1.Add(new ProjectPolygon() { Points = newPolygonPoints.Select(z => new Vector2d(z.X, z.Y)).ToList() });
            }


            foreach (var polygon in temp1)
            {
                foreach (var item in polygon.Points)
                {
                    var tr = dc.Transform(item.X, item.Y);
                    gr.FillRectangle(Brushes.Blue, tr.X - w1, tr.Y - w1, w1 * 2, w1 * 2);
                    gr.FillRectangle(Brushes.LightBlue, tr.X - w2, tr.Y - w2, w2 * 2, w2 * 2);
                }
                if (polygon.Points.Count > 2)
                {
                    var pp = polygon.Points.Select(z =>
                     {
                         var t = dc.Transform(z.X, z.Y);
                         return new PointF(t.X, t.Y);
                     }).ToArray();
                    gr.FillPolygon(new SolidBrush(Color.FromArgb(64, Color.LightGreen)), pp);
                    Pen p2 = new Pen(Color.Blue, 1);
                    p2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    p2.DashPattern = new float[] { 10, 5 };
                    Pen p3 = new Pen(Color.Black);
                    p3.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    p3.DashPattern = new float[] { 5, 10 };
                    gr.DrawPolygon(p2, pp);
                    //gr.DrawPolygon(p3, pp);
                }
            }


            var pos = pictureBox1.PointToClient(Cursor.Position);
            var back = dc.BackTransform(pos);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(64, Color.LightBlue)), 0, 0, 100, 35);
            gr.DrawString(dc.scale + "", SystemFonts.DefaultFont, Brushes.Black, 0, 0);
            gr.DrawString(back.X + "; " + back.Y, SystemFonts.DefaultFont, Brushes.Black, 0, 15);

            pictureBox1.Image = bmp;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            dc.ResetView();
        }


        bool addPolygonMode = false;
        List<Vector2> newPolygonPoints = new List<Vector2>();
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            addPolygonMode = true;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (addPolygonMode && newPolygonPoints.Any())
            {
                addPolygonMode = false;
                polygons.Add(new ProjectPolygon() { Points = newPolygonPoints.Select(z => new Vector2d(z.X, z.Y)).ToList() });
                newPolygonPoints.Clear();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            List<PointF> pp = new List<PointF>();
            foreach (var item in polygons)
            {
                var rect = item.BoundingBox();
                if (rect == null) continue;
                pp.Add(rect.Value.Location);
                pp.Add(new PointF(rect.Value.Right, rect.Value.Bottom));
            }
            if (pp.Count == 0) return;
            dc.FitToPoints(pp.ToArray(), 5);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            polygons.Clear();
        }
    }
}
