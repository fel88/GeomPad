using System.Reflection;

namespace GeomPad
{
    public class StringFieldEditor : AbstractFieldEditor
    {
        public StringFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public StringFieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }

        public string Value
        {
            get
            {
                if (Field != null)
                {
                    return ((string)Field.GetValue(Object));
                }
                return ((string)Property.GetValue(Object));
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