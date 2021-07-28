using System.Text;

namespace GeomPad
{
    public abstract class AbstractHelperItem
    {
        public abstract void Draw(IDrawingContext gr); 
        public bool Selected;
        public string Name { get; set; }
        public bool Visible { get; set; } = true;

        public virtual void AppendToXml(StringBuilder sb) { }
        
    }
}
