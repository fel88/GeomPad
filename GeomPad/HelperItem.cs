using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GeomPad
{
    public abstract class HelperItem
    {
        public int Z { get; set; }
        public bool Visible { get; set; } = true;
        public string Name { get; set; }
        public bool Selected;
        public abstract void Draw(DrawingContext gr);

        public virtual void AppendToXml(StringBuilder sb)
        {

        }

        public virtual RectangleF? BoundingBox()
        {
            return null;
        }
    }    
}
