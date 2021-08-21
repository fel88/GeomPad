using System.Reflection;

namespace GeomPad
{
    public class FieldEditor<T> : AbstractFieldEditor
    {
        public FieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public FieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }
        
        public T Value
        {
            get
            {
                if (Field != null)
                {
                    return ((T)Field.GetValue(Object));
                }
                return ((T)Property.GetValue(Object));
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