using System.Collections.Generic;
using System.Drawing;

namespace GeomPad.Helpers
{
    public class Group : HelperItem
    {
        public Group()
        {
            SelectedChanged += Group_SelectedChanged;
        }

        private void Group_SelectedChanged()
        {
            foreach (var item in Items)
            {
                item.Selected = Selected;
            }
        }

        public override RectangleF? BoundingBox()
        {
            RectangleF? ret = null;
            foreach (var item in Items)
            {
                var t = item.BoundingBox();
                if (t == null) 
                    continue;

                if (ret == null) 
                    ret = t;
                else
                    RectangleF.Union(ret.Value, t.Value);
            }
            return ret;
        }

        public List<HelperItem> Items = new List<HelperItem>();
        public override void Draw(IDrawingContext gr)
        {
            if (!Visible) return;
            foreach (var item in Items)
            {
                item.Draw(gr);
            }
        }

        public override void ClearSelection()
        {
            Selected = false;
            foreach (var item in Items)
            {
                item.ClearSelection();
            }
        }
    }
}
