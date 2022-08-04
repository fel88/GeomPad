using System;
using System.Text;

namespace GeomPad
{
    public abstract class AbstractHelperItem
    {
        public abstract void Draw(IDrawingContext gr);
        bool _selected;
        public bool Selected
        {
            get => _selected; set
            {
                if (_selected != value)
                {
                    _selected = value;
                    SelectedChanged?.Invoke();
                }
            }
        }

        public event Action SelectedChanged;
        public string Name { get; set; }
        public string TypeName => GetType().Name;
        public bool Visible { get; set; } = true;

        public virtual void AppendToXml(StringBuilder sb) { }

    }
}
