using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers
{
    public abstract class HelperItem : AbstractHelperItem
    {
        public override RectangleF? BoundingBox()
        {
            return null;
        }
     
        public virtual void ParseXml(XElement item)
        {

        }        
    }
}
