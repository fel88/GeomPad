using System.Reflection;

namespace GeomPad
{
    public class FloatFieldEditor : AbstractFieldEditor
    {
        public FloatFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public FloatFieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }
        
        public float Value
        {
            get
            {
                if (Field != null)
                {
                    return ((float)Field.GetValue(Object));
                }
                return ((float)Property.GetValue(Object));
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