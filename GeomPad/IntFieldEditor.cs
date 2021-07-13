using System.Reflection;

namespace GeomPad
{
    public class IntFieldEditor : IName
    {
        public IntFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public IntFieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public PropertyInfo Property;
        public int Value
        {
            get
            {
                if (Field != null)
                {
                    return ((int)Field.GetValue(Object));
                }
                return ((int)Property.GetValue(Object));
            }
            set
            {
                if (Field != null)
                {
                    Field.SetValue(Object, value);
                    return;
                }
                Property.SetValue(Object, value);
            }
        }

    }
}