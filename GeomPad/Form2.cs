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
    public partial class Form2 : Form
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
            var h = listView1.SelectedItems[0].Tag as HelperItem3D;
            Helpers.Remove(h);
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
                if (inter != null)
                {
                    Helpers.Add(new Point3DHelper() { Position = inter.Value });
                    updateHelpersList();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var h in Helpers)
            {
                h.Selected = false;
            }
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as HelperItem3D;
            propertyGrid1.SelectedObject = tag;

            tag.Selected = true;
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
                var n0 = th.V2 - th.V0;
                var n1 = th.V1 - th.V0;
                var normal = Vector3d.Cross(n0, n1);
                Helpers.Add(new PlaneHelper() { Position = th.V0, Normal = normal });
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
                    case "plane":
                        Helpers.Add(new PlaneHelper(item));
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
            var cc = listView1.SelectedItems[0].Tag as ICommandsContainer;
            if (cc == null) return;
            foreach (var item in cc.Commands)
            {
                var ccc = new ToolStripMenuItem(item.Name);
                commandsToolStripMenuItem.DropDownItems.Add(ccc);
                ccc.Click += (s, ee) => { item.Process(cc as HelperItem3D); };
            }
        }
    }

    public interface ICommandsContainer
    {
        ICommand[] Commands { get; }
    }
    public interface ICommand
    {
        string Name { get; }
        Action<HelperItem3D> Process { get; }
    }

}
