using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad
{
    public partial class Form2 : Form, I3DPadContainer
    {
        public Form2()
        {
            InitializeComponent();
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));


            if (glControl.Context.GraphicsMode.Samples == 0)
            {
                glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            }
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);

            tableLayoutPanel1.Controls.Add(glControl, 0, 0);
            glControl.Dock = DockStyle.Fill;
        }
        public CameraViewManager ViewManager;
        Camera camera1 = new Camera() { IsOrtho = true };
        private EventWrapperGlControl evwrapper;
        GLControl glControl;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }


            Redraw();

        }
        void Redraw()
        {
            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);



            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);



            GL.Enable(EnableCap.DepthTest);

            float zz = -500;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();

            camera1.Setup(glControl);

            if (drawAxis)
            {
                GL.LineWidth(2);
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(100, 0, 0);
                GL.End();

                GL.Color3(Color.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 100, 0);
                GL.End();

                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 100);
                GL.End();
            }


            foreach (var item in Helpers)
            {
                item.Draw();
            }

            glControl.SwapBuffers();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        List<HelperItem3D> Helpers = new List<HelperItem3D>();


        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new Line3DHelper() { Start = new Vector3d(), End = new Vector3d(10, 10, 10) });
            updateHelpersList();

        }

        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new TriangleHelper() { Verticies = new[] { new Vector3d(), new Vector3d(10, 0, 0), new Vector3d(0, 10, 0) } });
            updateHelpersList();
        }

        void updateHelpersList()
        {
            listView1.Items.Clear();
            foreach (var item in Helpers)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.GetType().Name }) { Tag = item });
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var h = listView1.SelectedItems[i].Tag as HelperItem3D;
                Helpers.Remove(h);
            }

            updateHelpersList();
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 2) return;
            var i0 = listView1.SelectedItems[0].Tag as HelperItem3D;
            var i1 = listView1.SelectedItems[1].Tag as HelperItem3D;

            var objs = new[] { i0, i1 };
            if (i0 is Line3DHelper lh0 && i1 is Line3DHelper lh1)
            {
                var inter = Geometry.Intersect3dCrossedLines(new Line3D() { Start = lh0.Start, End = lh0.End }, new Line3D() { Start = lh1.Start, End = lh1.End });
                if (inter != null && !double.IsNaN(inter.Value.X) && !double.IsInfinity(inter.Value.X))
                {
                    Helpers.Add(new Point3DHelper() { Position = inter.Value });
                    updateHelpersList();
                }
                else
                {
                    SetStatus("no intersection", StatusTypeEnum.Warning);
                }
            }
            if (objs.Any(z => z is Line3DHelper) && objs.Any(z => z is Point3DHelper))
            {
                var pl = objs.First(z => z is Point3DHelper) as Point3DHelper;
                var th = objs.First(z => z is Line3DHelper) as Line3DHelper;

                var l = new Line3D() { Start = th.Start, End = th.End };
                if (l.IsPointOnLine(pl.Position))
                {
                    SetStatus($"point is on line ({(l.IsPointInsideSegment(pl.Position) ? "inside" : "not inside")})", StatusTypeEnum.Information);
                }
                else
                {
                    SetStatus("point is not on line", StatusTypeEnum.Warning);
                }

            }
            if (objs.Any(z => z is TriangleHelper) && objs.Any(z => z is PlaneHelper))
            {
                var pl = objs.First(z => z is PlaneHelper) as PlaneHelper;
                var th = objs.First(z => z is TriangleHelper) as TriangleHelper;

                var n0 = th.V2 - th.V0;
                var n1 = th.V1 - th.V0;
                var normal = Vector3d.Cross(n0, n1);
                var pln = new PlaneHelper() { Position = th.V0, Normal = normal };
                var ln = pln.Intersect(pl);
                if (ln != null)
                {
                    Helpers.Add(new Line3DHelper() { Start = ln.Start, End = ln.End });
                    updateHelpersList();
                }
            }
            if (objs.All(z => z is PlaneHelper))
            {
                var pl = objs[0] as PlaneHelper;
                var pl2 = objs[1] as PlaneHelper;

                var ln = pl2.Intersect(pl);
                if (ln != null)
                {
                    Helpers.Add(new Line3DHelper() { Start = ln.Start, End = ln.End });
                    updateHelpersList();
                }
            }
        }

        public enum StatusTypeEnum
        {
            Information, Warning, Error
        }
        private void SetStatus(string v, StatusTypeEnum type)
        {
            switch (type)
            {
                case StatusTypeEnum.Information:
                    toolStripStatusLabel1.BackColor = Color.LightGreen;
                    toolStripStatusLabel1.ForeColor = Color.Black;
                    break;
                case StatusTypeEnum.Warning:
                    toolStripStatusLabel1.BackColor = Color.Yellow;
                    toolStripStatusLabel1.ForeColor = Color.Blue;
                    break;
                case StatusTypeEnum.Error:
                    toolStripStatusLabel1.BackColor = Color.Red;
                    toolStripStatusLabel1.ForeColor = Color.White;
                    break;
                default:
                    break;
            }
            toolStripStatusLabel1.Text = v;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var h in Helpers)
            {
                h.Selected = false;
            }
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as HelperItem3D;
            propertyGrid1.SelectedObject = tag;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var tag2 = listView1.SelectedItems[i].Tag as HelperItem3D;
                tag2.Selected = true;
            }

            if (tag is IEditFieldsContainer c)
            {
                var objs = c.GetObjects();
                listView2.Items.Clear();
                foreach (var item in objs)
                {
                    listView2.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
                }
            }
        }

        private void pointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new Point3DHelper() { Position = new Vector3d() });
            updateHelpersList();
        }

        private void planeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new PlaneHelper() { Position = new Vector3d(), Normal = Vector3d.UnitZ });
            updateHelpersList();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            propertyGrid1.SelectedObject = listView2.SelectedItems[0].Tag;
        }

        private void planeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is TriangleHelper th)
            {
                var pl = th.GetPlane();
                Helpers.Add(pl);
                updateHelpersList();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "scenes (*.xml)|*.xml";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            foreach (var item in Helpers)
            {
                item.AppendXml(sb);
            }
            sb.AppendLine("</root>");
            File.WriteAllText(sfd.FileName, sb.ToString());


        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var doc = XDocument.Load(ofd.FileName);
            var root = doc.Descendants("root").First();
            Helpers.Clear();
            foreach (var item in root.Elements())
            {
                switch (item.Name.LocalName)
                {
                    case "point":
                        Helpers.Add(new Point3DHelper(item));
                        break;
                    case "plane":
                        Helpers.Add(new PlaneHelper(item));
                        break;
                    case "line":
                        Helpers.Add(new Line3DHelper(item));
                        break;
                    case "triangle":
                        Helpers.Add(new TriangleHelper(item));
                        break;
                }
            }
            updateHelpersList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(0, -10, 0);
            camera1.CamUp = Vector3.UnitZ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(-10, 0, 0);
            camera1.CamUp = Vector3.UnitZ;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3.Zero;
            camera1.CamFrom = new Vector3(0, 0, -10);
            camera1.CamUp = Vector3.UnitY;
        }

        private void commandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            commandsToolStripMenuItem.DropDownItems.Clear();
            if (listView1.SelectedItems.Count == 0) return;
            var focusedItem = listView1.FocusedItem;
            var cc = focusedItem.Tag as ICommandsContainer;
            if (cc == null) return;
            List<HelperItem3D> all = new List<HelperItem3D>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                all.Add(listView1.SelectedItems[i].Tag as HelperItem3D);
            }

            foreach (var item in cc.Commands)
            {
                var ccc = new ToolStripMenuItem(item.Name);
                commandsToolStripMenuItem.DropDownItems.Add(ccc);
                ccc.Click += (s, ee) => { item.Process(cc as HelperItem3D, all.Except(new[] { cc as HelperItem3D }).ToArray(), this); };
            }
        }

        private void linesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is TriangleHelper th)
            {
                var lns = th.GetLines();
                Helpers.AddRange(lns);
                updateHelpersList();
            }
        }

        bool drawAxis = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            drawAxis = checkBox1.Checked;
        }

        private void setCameraToPlaneToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 2) return;
            var i0 = listView1.SelectedItems[0].Tag as HelperItem3D;
            var i1 = listView1.SelectedItems[1].Tag as HelperItem3D;

            if (i0 is Point3DHelper lh0 && i1 is Point3DHelper lh1)
            {
                Helpers.Add(new Line3DHelper() { Start = lh0.Position, End = lh1.Position });
                updateHelpersList();
            }
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new PolygonleHelper() { };
            Helpers.Add(ph);
            ph.Verticies.Add(new Vector3d(0, 0, 0));
            ph.Verticies.Add(new Vector3d(6, 2, 0));
            ph.Verticies.Add(new Vector3d(6, 6, 0));
            ph.Verticies.Add(new Vector3d(0, 8, 0));
            updateHelpersList();
        }

        public void AddHelper(HelperItem3D h)
        {
            Helpers.Add(h);
            updateHelpersList();
        }

        public void AddHelpers(HelperItem3D[] h)
        {
            Helpers.AddRange(h);
            updateHelpersList();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in Helpers)
            {
                if (item is Line3DHelper l1)
                {
                    pp.Add(l1.Start);
                    pp.Add(l1.End);
                }
                if (item is Point3DHelper l2)
                {
                    pp.Add(l2.Position);
                }
                if (item is TriangleHelper l3)
                {
                    pp.Add(l3.V0);
                    pp.Add(l3.V1);
                    pp.Add(l3.V2);
                }
                if (item is ArcDividerHelper arc)
                {
                    pp.AddRange(arc.GetPointsD());                    
                }
            }
            if (pp.Count == 0) return;
            camera1.FitToPoints(pp.ToArray(), glControl.Width, glControl.Height);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            camera1 = new Camera() { IsOrtho = true };
            ViewManager.Attach(evwrapper, camera1);

        }

        private void arcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new ArcDividerHelper()
            {
                Center = new Vector3d(),
                Start = new Vector3d(5, 5, 5),
                Aux = new Vector3d(-1, -1, 1),
                SweepAng = 130
            };
            Helpers.Add(ph);
            updateHelpersList();
        }
    }

}
