using GeomPad.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace GeomPad.Controls._2d
{

    public class Viewer2DDockPanel : DockContent
    {
        public PictureBox PictureBox;
        public Viewer2DDockPanel()
        {
            Text = "Viewer";
            CloseButton = false;
            //DockAreas &= ~(DockAreas.Float|DockAreas.DockLeft|DockAreas.do);
            DockAreas = DockAreas.Document;
            PictureBox = new PictureBox() { };
            Controls.Add(PictureBox);
            PictureBox.Dock = DockStyle.Fill;
        }
    }


    public class PropertiesListDockPanel : DockContent, IDataModel2DConsumer
    {
        public PropertyGrid Control;

        public PropertiesListDockPanel()
        {
            Text = "Properties";
            Control = new PropertyGrid();
            Control.HelpVisible = false;
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }

        public void Init(Pad2DDataModel dataModel)
        {
            dataModel.OnSelectedChanged += DataModel_OnSelectedChanged;
        }

        private void DataModel_OnSelectedChanged(HelperItem obj)
        {
            Control.SelectedObject = obj;
        }
    }


    public class PolyBoolListDockPanel : DockContent, IDataModel2DConsumer
    {
        public PolyBoolOperationsControl Control;
        public PolyBoolListDockPanel()
        {
            Text = "PolyBool operations";
            Control = new PolyBoolOperationsControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }


        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }
    public class ClipperOperationsDockPanel : DockContent, IDataModel2DConsumer
    {
        public ClipperOperationsControl Control;
        public ClipperOperationsDockPanel()
        {
            Text = "Clipper operations";
            Control = new ClipperOperationsControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }


        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }
    public class NFPOperationsDockPanel : DockContent, IDataModel2DConsumer
    {
        public NFPOperationsControl Control;
        public NFPOperationsDockPanel()
        {
            Text = "NFP operations";
            Control = new NFPOperationsControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }


        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }
    public class OtherOperationsDockPanel : DockContent, IDataModel2DConsumer
    {
        public OtherOperationsControl Control;
        public OtherOperationsDockPanel()
        {
            Text = "Other operations";
            Control = new OtherOperationsControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }


        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }
    public class DebugDockPanel : DockContent, IDataModel2DConsumer
    {
        public DebugControl Control;
        public DebugDockPanel()
        {
            Text = "Debug";
            Control = new DebugControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }

        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }


    public class HelpersListDockPanel : DockContent, IDataModel2DConsumer
    {
        public HelpersTreeControl Control;
        public HelpersListDockPanel()
        {
            Text = "Objects";
            Control = new HelpersTreeControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }

        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }

    public class PointsListDockPanel : DockContent, IDataModel2DConsumer
    {
        public PointsListControl Control;
        public PointsListDockPanel()
        {
            Text = "Points";
            Control = new PointsListControl();
            Controls.Add(Control);
            Control.Dock = DockStyle.Fill;
        }


        public void Init(Pad2DDataModel dataModel)
        {
            Control.Init(dataModel);
        }
    }
}
