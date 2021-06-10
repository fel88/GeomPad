using OpenTK;
using System.Reflection;

namespace GeomPad
{
    public class VectorEditor : IName
    {
        public VectorEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public double X
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).X;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.X = value;
                Field.SetValue(Object, v);
            }
        }

        public double Y
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Y;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Y = value;
                Field.SetValue(Object, v);
            }
        }

        public double Z
        {
            get
            {
                return ((Vector3d)Field.GetValue(Object)).Z;
            }
            set
            {
                var v = ((Vector3d)Field.GetValue(Object));
                v.Z = value;
                Field.SetValue(Object, v);
            }
        }
    }
    public class StringFieldEditor : IName
    {
        public StringFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public string Value
        {
            get
            {
                return ((string)Field.GetValue(Object));
            }
            set        
            {
                Field.SetValue(Object, value);
            }
        }

    }
    public class IntFieldEditor : IName
    {
        public IntFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }
        public string Name { get; set; }
        public object Object;
        public FieldInfo Field;
        public int Value
        {
            get
            {
                return ((int)Field.GetValue(Object));
            }
            set
            {
                Field.SetValue(Object, value);
            }
        }

    }
}