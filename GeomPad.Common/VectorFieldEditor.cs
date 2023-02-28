using OpenTK;
using System.Reflection;

namespace GeomPad
{
    public class VectorFieldEditor : AbstractFieldEditor
    {
        public VectorFieldEditor(PropertyInfo f)
        {
            Property = f;
            Name = f.Name;
        }
        public VectorFieldEditor(FieldInfo f)
        {
            Field = f;
            Name = f.Name;
        }

        public void SetVector(Vector3d v)
        {
            if (Field != null)
                Field.SetValue(Object, v);
            else
                Property.SetValue(Object, v);
        }

        public Vector3d Vector
        {
            get
            {
                if (Field != null)
                {
                    return ((Vector3d)Field.GetValue(Object));
                }
                return ((Vector3d)Property.GetValue(Object));
            }
        }
        public double X
        {
            get
            {
                if (Field != null)
                {
                    return ((Vector3d)Field.GetValue(Object)).X;
                }
                return ((Vector3d)Property.GetValue(Object)).X;
            }
            set
            {
                if (Field != null)
                {
                    var v = ((Vector3d)Field.GetValue(Object));
                    v.X = value;
                    Field.SetValue(Object, v);
                }
                else
                {
                    var v = ((Vector3d)Property.GetValue(Object));
                    v.X = value;
                    Property.SetValue(Object, v);
                }
            }
        }

        public double Y
        {
            get
            {
                if (Field != null)
                {
                    return ((Vector3d)Field.GetValue(Object)).Y;
                }
                return ((Vector3d)Property.GetValue(Object)).Y;
            }
            set
            {
                if (Field != null)
                {
                    var v = ((Vector3d)Field.GetValue(Object));
                    v.Y = value;
                    Field.SetValue(Object, v);
                }
                else
                {
                    var v = ((Vector3d)Property.GetValue(Object));
                    v.Y = value;
                    Property.SetValue(Object, v);
                }
            }
        }

        public double Z
        {
            get
            {
                if (Field != null)
                {
                    return ((Vector3d)Field.GetValue(Object)).Z;
                }
                return ((Vector3d)Property.GetValue(Object)).Z;
            }
            set
            {
                if (Field != null)
                {
                    var v = ((Vector3d)Field.GetValue(Object));
                    v.Z = value;
                    Field.SetValue(Object, v);
                }
                else
                {
                    var v = ((Vector3d)Property.GetValue(Object));
                    v.Z = value;
                    Property.SetValue(Object, v);
                }
            }
        }
    }
}