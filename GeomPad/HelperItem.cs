using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    public abstract class HelperItem: AbstractHelperItem
    {
        public int Z { get; set; }
        
        
        public Action Changed;
       

        public virtual RectangleF? BoundingBox()
        {
            return null;
        }

        public virtual void Shift(Vector2d vector)
        {
            
        }

        public virtual void ParseXml(XElement item)
        {
            
        }
    }      
}
