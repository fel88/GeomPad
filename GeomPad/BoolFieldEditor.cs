using System.Reflection;

namespace GeomPad
{
    public class BoolFieldEditor : IName
    {
        public BoolFieldEditor(PropertyInfo  f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public PropertyInfo Field;
        public bool Value
        {
            get
            {
                return ((bool)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
}