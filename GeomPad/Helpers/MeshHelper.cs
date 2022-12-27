using GeomPad.Dialogs;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace GeomPad.Helpers
{
    public class MeshHelper : HelperItem, ICommandsContainer
    {
        public MeshHelper()
        {

        }

        public MeshHelper(Vector2d[][] triangles)
        {
            _mesh = triangles;
        }

        public override RectangleF? BoundingBox()
        {
            var _points = _mesh.SelectMany(zz => zz).ToArray();
            var minx = (float)_points.Min(z => z.X);
            var maxx = (float)_points.Max(z => z.X);
            var miny = (float)_points.Min(z => z.Y);
            var maxy = (float)_points.Max(z => z.Y);

            return new RectangleF(minx, miny, maxx - minx, maxy - miny);
        }

        Vector2d[][] _mesh;
        public Vector2d[][] Mesh { get => _mesh; set => _mesh = value; }
        bool _fill = true;
        public bool Fill { get => _fill; set { _fill = value; Changed?.Invoke(); } }

        public int TianglesCount
        {
            get
            {
                if (_mesh == null) return 0;
                return _mesh.Length;
            }
        }

        public Color FillColor
        {
            get
            {
                return (FillBrush as SolidBrush).Color;
            }
            set
            {
                FillBrush = new SolidBrush(value);
                Changed?.Invoke();
            }
        }
        public bool DrawPoints { get; set; } = false;
        public bool DrawWireframe { get; set; } = true;

        public ICommand[] Commands => new ICommand[] { new FilterOutsideTriangles(), new ExtractOutsideContour() };

        public class ExtractOutsideContour : ICommand
        {
            private static PointF[] ExtractContour(List<Vector2d[]> tr)
            {
                List<Edge> edges = new List<Edge>();
                List<PointF> ret = new List<PointF>();

                Dictionary<string, List<Vector2d>> dd = new Dictionary<string, List<Vector2d>>();
                List<Node> nodes = new List<Node>();

                //collect all edges and outter edges check

                foreach (var item in tr)
                {
                    List<Node> nn = new List<Node>();
                    foreach (var t1 in item)
                    {
                        Node nd = null;
                        var fr = nodes.FirstOrDefault(zz => (zz.Point - t1).Length < 1e-3f);
                        if (fr == null)
                        {
                            nd = new Node() { Point = t1 };
                            nodes.Add(nd);
                        }
                        else
                        {
                            nd = fr;
                        }
                        nn.Add(nd);
                    }

                    var e1 = GetEdge(nn[0], nn[1], edges);
                    if (e1 == null)
                    {
                        e1 = new Edge(nn[0], nn[1]);
                        edges.Add(e1);
                    }
                    e1.Qty++;
                    nn[0].AddEdge(e1);
                    nn[1].AddEdge(e1);

                    nn[0].AddEdge(nn[1]);
                    nn[1].AddEdge(nn[0]);


                    var e2 = GetEdge(nn[1], nn[2], edges);
                    if (e2 == null)
                    {
                        e2 = new Edge(nn[1], nn[2]);
                        edges.Add(e2);
                    }
                    e2.Qty++;
                    nn[1].AddEdge(e2);
                    nn[2].AddEdge(e2);

                    nn[1].AddEdge(nn[2]);
                    nn[2].AddEdge(nn[1]);


                    var e3 = GetEdge(nn[2], nn[0], edges);
                    if (e3 == null)
                    {
                        e3 = new Edge(nn[2], nn[0]);
                        edges.Add(e3);
                    }
                    e3.Qty++;
                    nn[2].AddEdge(e3);
                    nn[0].AddEdge(e3);

                    nn[2].AddEdge(nn[0]);
                    nn[0].AddEdge(nn[2]);
                }

                var fr1 = nodes.First(z => z.Childs.Count == 2);
                List<Node> remains = new List<Node>();

                Queue<Node> q = new Queue<Node>();
                q.Enqueue(fr1);
                List<Node> contour = new List<Node>();
                while (q.Count > 0)
                {
                    //check edge is outter. 
                    var deq = q.Dequeue();
                    if (deq.Visited) continue;
                    deq.Visited = true;
                    contour.Add(deq);

                    foreach (var item in deq.Edges.Where(zz => zz.Qty == 1))
                    {
                        
                        if (item.End == deq)
                        {
                            if (!item.Start.Visited)
                            {
                                q.Enqueue(item.Start);
                                break;
                            }
                            
                        }
                        else
                        {
                            if (!item.End.Visited)
                            {
                                q.Enqueue(item.End);
                                break;
                            }
                        }
                    }
                }

                var list = contour.Select(z => z.Point.ToPointF()).ToList();
                list.Add(list[0]);
                var ret2 = list.ToArray();
                return ret2;
            }

            private static Edge GetEdge(Node point1, Node point2, List<Edge> edges)
            {
                var ee = new Edge(point1, point2);
                var fr = edges.FirstOrDefault(zz => zz.IsEqualTo(ee));
                if (fr != null)
                {
                    return fr;
                }
                return null;
            }

            public string Name => "extract outside contour";

            public System.Action<ICommandContext> Process => (cc) =>
            {
                var ee = cc.Source as MeshHelper;
                var tr = ee.Mesh.ToList();
                //mesh->contour
                var plh = new PolylineHelper();
                plh.Name = "extracted contour";
                plh.Points = ExtractContour(tr).Select(z => z.ToVector2d()).ToList();
                cc.Parent.AddHelper(plh);
            };
        }

        public class FilterOutsideTriangles : ICommand
        {
            public string Name => "filter outside triangles";

            public System.Action<ICommandContext> Process => (cc) =>
            {

                var ee = cc.Source as MeshHelper;
                var tr = ee.Mesh.ToList();

                DoubleInputDialog did = new DoubleInputDialog();
                double EarCutKoef = 15;
                did.Init(EarCutKoef);
                did.Caption = "Area koef";
                if (did.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                EarCutKoef = did.Value;

                while (true)
                {
                    List<Vector2d[]> toDel = new List<Vector2d[]>();
                    List<Vector2d[]> outsides = new List<Vector2d[]>();

                    float eps = 1e-3f;
                    foreach (var item in ee.Mesh)
                    {
                        int cnt = 0;
                        //check inside or outside?
                        bool outside = false;
                        int[] inters = new int[item.Length];

                        for (int i = 0; i < item.Length; i++)
                        {
                            Vector2d tt = item[i];

                            foreach (var tt2 in ee.Mesh)
                            {
                                foreach (var tt3 in tt2)
                                {
                                    var d = (tt3 - tt).Length;
                                    if (d < eps)
                                    {
                                        inters[i]++;
                                    }
                                }
                            }

                            /*if (OnTheEdge(draft, tt))
                            {
                                cnt++;
                            }*/
                        }

                        outside = inters.Any(z => z == 1);
                        if (outside)
                        {
                            outsides.Add(item);
                        }
                    }

                    List<Vector2d[]> ret = new List<Vector2d[]>();

                    foreach (var item in outsides)
                    {
                        if (Math.Abs(StaticHelpers.signed_area(item)) > EarCutKoef) continue;
                        ret.Add(item);
                        //ret.FillPoly(new[] { item.Select(z => new OpenCvSharp.Point(z.X, z.Y)) }, new Scalar(255));
                    }
                    if (ret.Count == 0) break;
                    foreach (var item in ret)
                    {
                        tr.Remove(item);
                    }
                    ee.Mesh = tr.ToArray();
                }


                //mesh->contour
                //draft.Points = ExtractContour(tr).ToList();
            };
        }


        public Brush FillBrush = SystemBrushes.Highlight;
        public override void Draw(IDrawingContext idc)
        {
            var dc = idc as DrawingContext;
            if (!Visible) return;

            float r = 3 / dc.scale;
            Brush br = Brushes.Black;
            Pen pen = Pens.Black;
            if (Selected)
            {
                br = Brushes.Red;
                pen = Pens.Red;
            }

            foreach (var item in _mesh)
            {
                GraphicsPath gp = new GraphicsPath();
                if (Fill && item.Length >= 3)
                {
                    gp.AddPolygon(item.Select(z => dc.Transform(z)).ToArray());

                    dc.gr.FillPath(FillBrush, gp);

                }
                if (DrawPoints)
                    foreach (var item2 in item)
                    {
                        var tr1 = dc.Transform(item2);
                        dc.gr.FillEllipse(br, tr1.X - r, tr1.Y - r, 2 * r, 2 * r);
                    }

                if (DrawWireframe)
                    for (int i = 0; i < item.Length; i++)
                    {
                        var j = (i + 1) % item.Length;
                        var tr1 = dc.Transform(item[i]);
                        var tr2 = dc.Transform(item[j]);
                        dc.gr.DrawLine(pen, tr1, tr2);
                    }
            }
        }
    }

    public class Node
    {
        public Vector2d Point;
        public List<Node> Childs = new List<Node>();
        public List<Edge> Edges = new List<Edge>();
        public bool Visited = false;

        public void AddEdge(Node node)
        {
            if (!Childs.Contains(node))
                Childs.Add(node);
        }
        public void AddEdge(Edge edge)
        {
            if (!Edges.Contains(edge))
                Edges.Add(edge);
        }
    }

    public class Edge
    {
        public Edge(Node s, Node e)
        {
            Start = s;
            End = e;
        }
        public Node Start;
        public Node End;
        public int Qty = 0;
        public double Len => (End.Point - Start.Point).Length;

        public bool IsEqualTo(Edge ee)
        {
            if (ee.Start == Start && ee.End == End)
            {
                return true;
            }
            if (ee.Start == End && ee.End == Start)
            {
                return true;
            }

            return false;
        }
    }

}