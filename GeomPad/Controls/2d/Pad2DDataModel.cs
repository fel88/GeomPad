using GeomPad.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GeomPad.Controls._2d
{
    public class Pad2DDataModel
    {
        public List<HelperItem> Items = new List<HelperItem>();
        public HelperItem SelectedItem;
        public HelperItem[] SelectedItems;

        public Form1 ParentForm;
        public event Action OnListUpdated;
        public event Action<HelperItem> OnSelectedChanged;

        public void Clear()
        {
            Items.Clear();
            OnListUpdated?.Invoke();
        }
        public bool drawAxis = true;
        public DrawingContext dc = new DrawingContext();



        public bool bubbleUpSelected = false;
        public NFP[] GetPairOfSelectedNfps()
        {
            List<PolygonHelper> phhs = new List<PolygonHelper>();

            //if (!checkBox1.Checked)
            {
                if (SelectedItems.Length < 2) { ParentForm.StatusMessage("there are no 2 polygon selected", StatusMessageType.Warning); return null; }

                foreach (var item in SelectedItems)
                {
                    phhs.Add(item as PolygonHelper);
                }
            }
            //  else
            {
                //   phhs.Add((comboBox2.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
                //  phhs.Add((comboBox3.SelectedItem as ComboBoxItem).Tag as PolygonHelper);
            }

            var ar1 = phhs.ToArray();


            NFP p = new NFP();
            NFP p2 = new NFP();


            p.Points = ar1[0].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            foreach (var item in ar1[0].Polygon.Childrens)
            {
                if (p.Childrens == null)
                    p.Childrens = new List<NFP>();
                p.Childrens.Add(new NFP() { Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            }
            p2.Points = ar1[1].Polygon.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray();
            foreach (var item in ar1[1].Polygon.Childrens)
            {
                if (p2.Childrens == null)
                    p2.Childrens = new List<NFP>();
                p2.Childrens.Add(new NFP() { Points = item.Points.Select(z => new SvgPoint(z.X, z.Y)).ToArray() });
            }
            return new[] { p, p2 };
        }
        public void ClearSelection()
        {
            Items.ForEach(z => z.ClearSelection());
            
        }

        internal void AddItem(HelperItem pointHelper)
        {
            Items.Add(pointHelper);
            pointHelper.Changed = () => { OnListUpdated?.Invoke(); };
            OnListUpdated?.Invoke();
        }
        
        internal void AddItems(HelperItem[] pointHelper)
        {
            Items.AddRange(pointHelper);
            foreach (var item in pointHelper)
            {
                item.Changed = () => { OnListUpdated?.Invoke(); };
            }
            
            OnListUpdated?.Invoke();
        }

        internal void RemoveItems(HelperItem[] helperItems)
        {
            foreach (var item in helperItems)
            {
                Items.Remove(item);
            }            
            OnListUpdated?.Invoke();
        }

        internal void RemoveItem(HelperItem helperItem)
        {
            Items.Remove(helperItem);
            OnListUpdated?.Invoke();
        }

        internal void ChangeSelectedItems(HelperItem[] helperItem)
        {
            
            for (int i = 0; i < helperItem.Length; i++)
            {
                helperItem[i].Selected = true;
            }

            OnSelectedChanged?.Invoke(helperItem[0]);            
            SelectedItem = helperItem[0];            
            SelectedItems = helperItem;            
        }

        internal void UpdateList()
        {
            OnListUpdated?.Invoke();
        }
    }
}