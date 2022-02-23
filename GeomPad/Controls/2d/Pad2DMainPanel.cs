using System;
using System.IO;
using System.Linq;
using System.Reflection;
using WeifenLuo.WinFormsUI.Docking;

namespace GeomPad.Controls._2d
{
    public class Pad2DMainPanel : DockPanel
    {
        PropertiesListDockPanel prop;
        public Viewer2DDockPanel view;
        HelpersListDockPanel helpersListPanel;
        PolyBoolListDockPanel pbPanel;
        ClipperOperationsDockPanel clipper;
        NFPOperationsDockPanel nfp;
        DebugDockPanel debug;
        PointsListDockPanel points;
        OtherOperationsDockPanel other;
        public Pad2DMainPanel()
        {
            Theme = new VS2015LightTheme();
            bool loaded = false;
            if (File.Exists("layout2d.xml"))
            {
                try
                {
                    LoadFromXml("layout2d.xml", (x) =>
                    {
                        var tps = Assembly.GetExecutingAssembly().GetTypes();
                        var fr = tps.First(z => z.FullName.Contains(x));
                        var ret = Activator.CreateInstance(fr) as IDockContent;
                        if (ret is Viewer2DDockPanel vv)
                        {
                            view = vv;
                        }
                        if (ret is OtherOperationsDockPanel oo)
                        {
                            other = oo;
                        }
                        if (ret is PropertiesListDockPanel p1)
                        {
                            prop = p1;
                        }
                        if (ret is NFPOperationsDockPanel _nfp)
                        {
                            nfp = _nfp;
                        }
                        if (ret is PointsListDockPanel pll)
                        {
                            points = pll;
                        }
                        if (ret is PolyBoolListDockPanel pbb)
                        {
                            pbPanel = pbb;
                        }
                        if (ret is ClipperOperationsDockPanel clp)
                        {
                            clipper = clp;
                        }
                        if (ret is DebugDockPanel dbg)
                        {
                            debug = dbg;
                        }
                        if (ret is HelpersListDockPanel hlp)
                        {
                            helpersListPanel = hlp;
                        }
                        return ret;
                    });
                    loaded = true;
                }
                catch (Exception ex)
                {

                }
            }
            if (!loaded)
            {
                pbPanel = new PolyBoolListDockPanel();
                other = new OtherOperationsDockPanel();
                clipper = new ClipperOperationsDockPanel();
                nfp = new NFPOperationsDockPanel();
                debug = new DebugDockPanel();
                points = new PointsListDockPanel();

                pbPanel.Show(this, DockState.DockBottom);
                clipper.Show(this, DockState.DockBottom);
                nfp.Show(this, DockState.DockBottom);
                debug.Show(this, DockState.DockBottom);
                points.Show(this, DockState.DockBottom);

                helpersListPanel = new HelpersListDockPanel();

                view = new Viewer2DDockPanel();
                view.Show(this, DockState.Document);
                helpersListPanel.Show(this, DockState.DockRight);

                prop = new PropertiesListDockPanel();

                prop.Show(this, DockState.DockBottom);
                other.Show(this, DockState.DockBottom);
            }

        }
        public void Init(Pad2DDataModel dm)
        {
            IDataModel2DConsumer[] items = new IDataModel2DConsumer[] { pbPanel, clipper, nfp, debug, points, prop, other, helpersListPanel };
            foreach (var item in items)
            {
                if (item == null) continue;
                item.Init(dm);
            }
        }

        internal void ShowNfp()
        {
            if (nfp == null)
            {
                nfp = new NFPOperationsDockPanel();
                nfp.Show(this, DockState.DockBottom);
            }
        }
        internal void ShowDebug()
        {
            if (debug == null)
            {
                debug = new DebugDockPanel();
                debug.Show(this, DockState.DockBottom);
            }
        }
    }

    public interface IDataModel2DConsumer
    {
        void Init(Pad2DDataModel dm);
    }
}