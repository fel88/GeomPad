using OpenTK;
using System.Collections.Generic;
using System.Text;

namespace GeomPad.Helpers3D
{
    public abstract class HelperItem : AbstractHelperItem, IEditFieldsContainer
    {        

        public virtual IName[] GetObjects()
        {
            List<IName> ret = new List<IName>();
            var fld = GetType().GetFields();
            for (int i = 0; i < fld.Length; i++)
            {
                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    ret.Add(new VectorEditor(fld[i]) { Object = this });
                }
            }
            var props = GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var at = props[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    if (props[i].PropertyType == typeof(Vector3d))
                    {
                        ret.Add(new VectorEditor(props[i]) { Object = this });
                    }
                    if (props[i].PropertyType == typeof(bool))
                    {
                        ret.Add(new BoolFieldEditor(props[i]) { Object = this });
                    }
                    if (props[i].PropertyType == typeof(int))
                    {
                        ret.Add(new IntFieldEditor(props[i]) { Object = this });
                    }
                    if (props[i].PropertyType == typeof(float))
                    {
                        ret.Add(new FieldEditor<float>(props[i]) { Object = this });
                    }
                    if (props[i].PropertyType == typeof(double))
                    {
                        ret.Add(new FieldEditor<double>(props[i]) { Object = this });
                    }
                }
            }
            return ret.ToArray();
        }
    }
}
