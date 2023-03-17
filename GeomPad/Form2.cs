using GeomPad.Common;
using GeomPad.Helpers3D;
using GeomPad.Helpers3D.BRep;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GeomPad
{
    public partial class Form2 : Form, IPadContainer
    {
        public Form2()
        {
            InitializeComponent();
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));

            DebugHelper.Error = (x) => { infoPanel.AddError(x); };


            if (glControl.Context.GraphicsMode.Samples == 0)
            {
                glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            }
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);

            panel1.Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;
            infoPanel.Dock = DockStyle.Bottom;
            panel1.Controls.Add(infoPanel);

            glControl.MouseDoubleClick += GlControl_MouseDoubleClick;

            updatePickColorButton();
        }

        private void GlControl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (pickedPoint != null)
            {
                camera1.CamTo = pickedPoint.Value;
            }
        }

        InfoPanel infoPanel = new InfoPanel();
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

        Vector3d? pickedPoint = null;
        void PickUpdate()
        {
            var pos = glControl.PointToClient(Cursor.Position);
            camera1.UpdateMatricies(glControl);
            MouseRay.UpdateMatrices();

            var ray = new MouseRay(pos.X, pos.Y);
            double? minDist = null;
            Vector3d? p = null;

            foreach (var item in Helpers.OfType<IPointsProvider>())
            {
                if (!item.Visible)
                    continue;

                foreach (var pitem in item.GetPoints())
                {
                    var d = dist(pitem, ray.Start, ray.End);
                    if (d > 20)
                        continue;

                    if (minDist == null || d < minDist.Value)
                    {
                        minDist = d;
                        p = pitem;
                    }
                }
            }
            pickedPoint = p;
            if (minDist != null)
            {
                toolStripStatusLabel3.Text = $"picked point: {p.Value.X} {p.Value.Y} {p.Value.Z}";

                GL.Color3(Color.Blue);
                GL.Disable(EnableCap.DepthTest);
                GL.PointSize(10);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(p.Value);
                GL.End();
                GL.Enable(EnableCap.DepthTest);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Decimal)
            {
                if (pickedPoint != null)
                    camera1.CamTo = pickedPoint.Value;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private double dist(Vector3d pitem, Vector3d start, Vector3d end)
        {
            return (GeometryUtils.point_on_line(start, end, pitem) - pitem).Length;
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
            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4d.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
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
            GL.PopMatrix();
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

            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);
            foreach (var item in Helpers)
            {
                item.Draw(null);
            }

            if (pickEnabled)
                PickUpdate();

            glControl.SwapBuffers();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        public List<IHelperItem> Helpers = new List<IHelperItem>();

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new LineHelper() { Start = new Vector3d(), End = new Vector3d(10, 10, 10) });
            updateHelpersList();

        }

        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new TriangleHelper() { Vertices = new[] { new Vector3d(), new Vector3d(10, 0, 0), new Vector3d(0, 10, 0) } });
            updateHelpersList();
        }

        void updateHelpersList()
        {
            listView1.Items.Clear();
            foreach (var item in Helpers)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.GetType().Name }) { Tag = item });
            }
        }

        void deleteItems()
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (!StaticHelpers.ShowQuestion("Are you sure to delete selected item?", Text)) return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var h = listView1.SelectedItems[i].Tag as HelperItem;
                Helpers.Remove(h);
            }

            updateHelpersList();
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteItems();
        }

        private void intersectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 2) return;
            var i0 = listView1.SelectedItems[0].Tag as HelperItem;
            var i1 = listView1.SelectedItems[1].Tag as HelperItem;

            var objs = new[] { i0, i1 };
            if (i0 is LineHelper lh0 && i1 is LineHelper lh1)
            {
                var inter = Geometry.Intersect3dCrossedLines(new Line3D() { Start = lh0.Start, End = lh0.End }, new Line3D() { Start = lh1.Start, End = lh1.End });
                if (inter != null && !double.IsNaN(inter.Value.X) && !double.IsInfinity(inter.Value.X))
                {
                    Helpers.Add(new PointHelper() { Position = inter.Value });
                    updateHelpersList();
                }
                else
                {
                    SetStatus("no intersection", StatusMessageType.Warning);
                }
            }
            if (objs.Any(z => z is LineHelper) && objs.Any(z => z is PointHelper))
            {
                var pl = objs.First(z => z is PointHelper) as PointHelper;
                var th = objs.First(z => z is LineHelper) as LineHelper;

                var l = new Line3D() { Start = th.Start, End = th.End };
                if (l.IsPointOnLine(pl.Position))
                {
                    SetStatus($"point is on line ({(l.IsPointInsideSegment(pl.Position) ? "inside" : "not inside")})", StatusMessageType.Info);
                }
                else
                {
                    SetStatus("point is not on line", StatusMessageType.Warning);
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
                    Helpers.Add(new LineHelper() { Start = ln.Start, End = ln.End });
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
                    Helpers.Add(new LineHelper() { Start = ln.Start, End = ln.End });
                    updateHelpersList();
                }
            }
        }

        public void SetStatus(string v, StatusMessageType type)
        {
            switch (type)
            {
                case StatusMessageType.Info:
                    toolStripStatusLabel1.BackColor = Color.LightGreen;
                    toolStripStatusLabel1.ForeColor = Color.Black;
                    break;
                case StatusMessageType.Warning:
                    toolStripStatusLabel1.BackColor = Color.Yellow;
                    toolStripStatusLabel1.ForeColor = Color.Blue;
                    break;
                case StatusMessageType.Error:
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
            var tag = listView1.SelectedItems[0].Tag as HelperItem;
            propertyGrid1.SelectedObject = tag;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var tag2 = listView1.SelectedItems[i].Tag as HelperItem;
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
            Helpers.Add(new PointHelper() { Position = new Vector3d() });
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
            var obj = listView2.SelectedItems[0].Tag;
            propertyGrid1.SelectedObject = obj;
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
                item.AppendToXml(sb);
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
                        Helpers.Add(new PointHelper(item));
                        break;
                    case "plane":
                        Helpers.Add(new PlaneHelper(item));
                        break;
                    case "line":
                        Helpers.Add(new LineHelper(item));
                        break;
                    case "triangle":
                        Helpers.Add(new TriangleHelper(item));
                        break;
                    case "polyline":
                        Helpers.Add(new PolylineHelper(item));
                        break;
                    case "ellipse":
                        Helpers.Add(new EllipseHelper(item));
                        break;
                    case "cloud":
                        Helpers.Add(new PointCloudHelper(item));
                        break;
                }
            }
            updateHelpersList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3d.Zero;
            camera1.CamFrom = new Vector3d(0, -10, 0);
            camera1.CamUp = Vector3d.UnitZ;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3d.Zero;
            camera1.CamFrom = new Vector3d(-10, 0, 0);
            camera1.CamUp = Vector3d.UnitZ;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            camera1.CamTo = Vector3d.Zero;
            camera1.CamFrom = new Vector3d(0, 0, -10);
            camera1.CamUp = Vector3d.UnitY;
        }

        private void commandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            commandsToolStripMenuItem.DropDownItems.Clear();
            if (listView1.SelectedItems.Count == 0) return;
            var focusedItem = listView1.FocusedItem;
            var cc = focusedItem.Tag as ICommandsContainer;
            if (cc == null) return;
            List<HelperItem> all = new List<HelperItem>();
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                all.Add(listView1.SelectedItems[i].Tag as HelperItem);
            }

            foreach (var item in cc.Commands)
            {
                var ccc = new ToolStripMenuItem(item.Name);
                commandsToolStripMenuItem.DropDownItems.Add(ccc);
                ccc.Click += (s, ee) => { item.Process(new CommandContext(cc as HelperItem, all.Except(new[] { cc as HelperItem }).ToArray(), this)); };
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
        void camToSelected(Vector3d[] vv)
        {
            if (vv == null || vv.Length == 0) return;
            Vector3d cnt = Vector3d.Zero;
            foreach (var item in vv)
            {
                cnt += item;
            }
            cnt /= vv.Length;
            var len = camera1.DirLen;
            var dir = camera1.Dir;
            camera1.CamTo = cnt;
            camera1.CamFrom = camera1.CamTo + dir * len;
        }
        private void setCameraToPlaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var h = listView1.SelectedItems[0].Tag as HelperItem;
            if (h is LineHelper lh)
            {
                camera1.CamTo = lh.Start;
            }
            if (h is PointHelper pnh)
            {
                camera1.CamTo = pnh.Position;
            }
            if (h is PlaneHelper ph)
            {
                camera1.CamTo = ph.Position;
            }
            if (h is SplineHelper sph)
            {
                camToSelected(sph.Poles.ToArray());
            }
        }

        private void toLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 2) return;
            var i0 = listView1.SelectedItems[0].Tag as HelperItem;
            var i1 = listView1.SelectedItems[1].Tag as HelperItem;

            if (i0 is PointHelper lh0 && i1 is PointHelper lh1)
            {
                Helpers.Add(new LineHelper() { Start = lh0.Position, End = lh1.Position });
                updateHelpersList();
            }
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new PolylineHelper() { };
            Helpers.Add(ph);
            ph.Verticies.Add(new Vector3d(0, 0, 0));
            ph.Verticies.Add(new Vector3d(6, 2, 0));
            ph.Verticies.Add(new Vector3d(6, 6, 0));
            ph.Verticies.Add(new Vector3d(0, 8, 0));
            updateHelpersList();
        }

        public void AddHelper(IHelperItem h)
        {
            Helpers.Add(h);
            updateHelpersList();
        }

        public void AddHelpers(IHelperItem[] h)
        {
            Helpers.AddRange(h);
            updateHelpersList();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in Helpers)
            {
                if (!item.Visible)
                    continue;

                if (item is IPointsProvider f)
                {
                    pp.AddRange(f.GetPoints());
                }
                else
                if (item is TriangleHelper l3)
                {
                    pp.Add(l3.V0);
                    pp.Add(l3.V1);
                    pp.Add(l3.V2);
                }
                else
                if (item is ArcDividerHelper arc)
                {
                    pp.AddRange(arc.GetPointsD());
                }
                else
                if (item is EllipseHelper elp)
                {
                    pp.AddRange(elp.GetPointsD());
                }
                else
                if (item is HingeHelper hinge)
                {
                    pp.Add(hinge.EdgePoint0);
                    pp.Add(hinge.EdgePoint1);
                    pp.Add(hinge.AuxPoint0);
                    pp.Add(hinge.AuxPoint1);
                }
                else
                if (item is PointCloudHelper phc)
                {
                    pp.AddRange(phc.Cloud.Points);
                }
                else
                if (item is SplineHelper sph)
                {
                    pp.AddRange(sph.Poles);
                }
                else
                if (item is AbstractBRepFaceHelper bh)
                {
                    if (bh.Mesh != null)
                        pp.AddRange(bh.Mesh.Triangles.SelectMany(z => z.Vertices.Select(u => u.Position)));
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

        private void hingeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new HingeHelper() { };
            Helpers.Add(ph);
            ph.EdgePoint0 = new Vector3d(0, 0, 0);
            ph.EdgePoint1 = new Vector3d(0, 10, 0);
            ph.AuxPoint0 = new Vector3d(10, 10, 0);
            ph.AuxPoint1 = new Vector3d(0, 5, 5);

            updateHelpersList();
        }

        private void ellipseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new EllipseHelper()
            {
                Normal = new Vector3d(0, 1, 0),
                AuxPoint = new Vector3d(10, 0, 0)
            };
            Helpers.Add(ph);
            updateHelpersList();
        }

        private void setDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0) return;
            if (!(listView2.SelectedItems[0].Tag is VectorFieldEditor tag)) return;
            VectorSetValuesDialog d = new VectorSetValuesDialog();
            d.Init(tag.Vector);
            d.ShowDialog();
            tag.SetVector(d.Vector);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var hi = propertyGrid1.SelectedObject as HelperItem;
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Tag == hi)
                {
                    listView1.Items[i].Text = hi.Name;
                }
            }
        }

        private void moveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            var h = listView1.SelectedItems[0].Tag as HelperItem;
            VectorSetValuesDialog d = new VectorSetValuesDialog();
            d.ShowDialog();
            h.MoveTo(d.Vector);
        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PLY files (*.ply)|*.ply";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var res = PlyLoader.LoadPly(ofd.FileName);

            Helpers.Add(new PointCloudHelper() { Cloud = res, Name = Path.GetFileName(ofd.FileName) });
            updateHelpersList();
        }

        private void cylinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new CylinderBRepFaceHelper() { });
            updateHelpersList();
        }

        public void OpenChildWindow(Form f)
        {
            f.MdiParent = MdiParent;
            f.Show();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteItems();
            }
        }

        private void splineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ph = new SplineHelper()
            {
                Degree = 2
            };
            ph.IsPeriodic = false;
            ph.IsNonPeriodic = true;
            ph.IsBSpline = true;
            ph.IsPolynomial = false;
            ph.SetKnots(new double[] { 0, 1 });
            ph.SetMultiplicities(new[] { 3, 3 });
            ph.SetWeights(new double[] { 1, 1, 1 });
            ph.SetPoles(new Vector3d[] {
                    new Vector3d (-3, -2.858306, 5.2154),
                    new Vector3d (-3,-5,5.2154),
                    new Vector3d (-3,-5,2.7154)
            });
            Helpers.Add(ph);
            updateHelpersList();
        }

        HelperItem[] loadXml(string content)
        {
            List<HelperItem> ret = new List<HelperItem>();

            var doc = XDocument.Parse(content);
            var root = doc.Descendants("root");
            foreach (var pitem in root.Elements("line"))
            {
                LineHelper lh = new LineHelper();
                if (pitem.Attribute("showCrosses") != null)
                    lh.ShowCrosses = bool.Parse(pitem.Attribute("showCrosses").Value);

                List<Vector3d> pnts = new List<Vector3d>();
                foreach (var point in pitem.Descendants("point"))
                {
                    var x = point.Attribute("x").Value.ParseDouble();
                    var y = point.Attribute("y").Value.ParseDouble();
                    var z = point.Attribute("z").Value.ParseDouble();
                    pnts.Add(new Vector3d(x, y, z));
                }
                lh.Start = pnts[0];
                lh.End = pnts[1];

                ret.Add(lh);
            }
            foreach (var pitem in doc.Descendants("mesh"))
            {
                MeshHelper mh = new MeshHelper();
                foreach (var item in pitem.Elements("triangle"))
                {
                    List<Vector3d> pnts = new List<Vector3d>();
                    foreach (var point in item.Descendants("vertex"))
                    {
                        var x = point.Attribute("x").Value.ParseDouble();
                        var y = point.Attribute("y").Value.ParseDouble();
                        var z = point.Attribute("z").Value.ParseDouble();
                        pnts.Add(new Vector3d(x, y, z));
                    }
                    mh.Mesh.Triangles.Add(new TriangleInfo() { Vertices = pnts.Select(z => new VertexInfo() { Position = z }).ToArray() });
                }
                ret.Add(mh);
            }
            foreach (var pitem in doc.Descendants("cloud"))
            {
                PointCloudHelper mh = new PointCloudHelper(pitem);
                ret.Add(mh);
            }
            foreach (var pitem in doc.Descendants("plane"))
            {
                PlaneHelper mh = new PlaneHelper(pitem);
                ret.Add(mh);
            }
            foreach (var pitem in doc.Descendants("spline"))
            {
                SplineHelper mh = new SplineHelper(pitem);
                ret.Add(mh);
            }

            return ret.ToArray();
        }

        private void fromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var hh = loadXml(Clipboard.GetText());
                Helpers.AddRange(hh);
                updateHelpersList();
                SetStatus("succesfully loaded.", StatusMessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, StatusMessageType.Error);
            }
        }

        private void fromFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void impoertMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All mesh formats|*.obj;*.off;*.stl";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var ext = Path.GetExtension(ofd.FileName).ToLower();
            IMeshLoader loader = null;
            if (ext == ".off")
            {
                loader = new OffLoader();
            }
            else if (ext == ".stl")
            {
                loader = new StlLoader();
            }
            else if (ext == ".obj")
            {
                loader = new ObjLoader();
            }

            AddHelper(loader.Load(ofd.FileName));
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var h = listView1.SelectedItems[0].Tag as HelperItem;
            var hh = h.Clone();
            hh.Name = h.Name + "_cloned";
            AddHelper(hh);
        }

        private void vectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helpers.Add(new VectorHelper() { Start = new Vector3d(), Dir = new Vector3d(1, 0, 0) });
            updateHelpersList();
        }

        bool pickEnabled = true;
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pickEnabled = !pickEnabled;
            updatePickColorButton();
        }

        void updatePickColorButton()
        {
            toolStripButton4.BackColor = pickEnabled ? Color.LightGreen : SystemColors.Control;
            toolStripButton4.ForeColor = pickEnabled ? Color.White : Color.Black;
        }

        private void editorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptEditor2D ss = new ScriptEditor2D();
            //ss.Init(dataModel);
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
            r.Script.Run(null, this);
        }
    }
}
