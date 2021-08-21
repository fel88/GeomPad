using System.Reflection;

namespace GeomPad
{
    public abstract class AbstractFieldEditor : IName
    {
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public PropertyInfo Property;
    }
}