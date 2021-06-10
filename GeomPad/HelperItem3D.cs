using System.Text;

namespace GeomPad
{
    public abstract class HelperItem3D
    {
        
        public bool Visible { get; set; } = true;
        public string Name { get; set; }
        public bool Selected;
        public abstract void Draw();

        public abstract void AppendXml(StringBuilder sb);
        
    }


}
