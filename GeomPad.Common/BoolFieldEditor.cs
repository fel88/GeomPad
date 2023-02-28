using System.Reflection;

namespace GeomPad
{
    public class BoolFieldEditor : AbstractFieldEditor
    {
        public BoolFieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }
        public BoolFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }

        public bool Value
        {
            get
            {
                if (Field != null)
                {
                    return ((bool)Field.GetValue(Object));
                }
                return ((bool)Property.GetValue(Object));                
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