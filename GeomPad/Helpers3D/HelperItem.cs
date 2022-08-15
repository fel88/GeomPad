using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomPad.Helpers3D
{
    public abstract class HelperItem : AbstractHelperItem, IEditFieldsContainer
    {
        public HelperItem() { }
        public HelperItem(XElement item)
        {
            if (item.Attribute("name") != null)
                Name = item.Attribute("name").Value;
        }

        public override RectangleF? BoundingBox()
        {
            throw new NotImplementedException();
        }

        protected Vector3d parseVector(Vector3d defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Element(key);
            if (nrm == null)
            {
                if (required)
                    throw new GeomPadException();
                return defValue;
            }
            var pos = nrm.Attribute("pos").Value.Split(new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries).Select(z => double.Parse(z.Replace(",", "."),
                CultureInfo.InvariantCulture)).ToArray();
            return new Vector3d(pos[0], pos[1], pos[2]);
        }
        protected bool parseBool(bool defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Attribute(key);
            if (nrm == null)
            {
                if (required)
                    throw new GeomPadException();
                return defValue;
            }
            return bool.Parse(nrm.Value);
        }
        protected double parseDouble(double defValue, XElement parent, string key, bool required = false)
        {
            var nrm = parent.Attribute(key);
            if (nrm == null)
            {
                if (required)
                    throw new GeomPadException();
                return defValue;
            }
            return StaticHelpers.ParseDouble(nrm.Value);
        }

        public virtual IName[] GetObjects()
        {
            List<IName> ret = new List<IName>();
            var fld = GetType().GetFields();
            for (int i = 0; i < fld.Length; i++)
            {
                var at = fld[i].GetCustomAttributes(typeof(EditFieldAttribute), true);
                if (at != null && at.Length > 0)
                {
                    if (fld[i].FieldType == typeof(Vector3d))
                    {
                        ret.Add(new VectorFieldEditor(fld[i]) { Object = this });
                    }
                    else
                    if (fld[i].FieldType == typeof(double))
                    {
                        ret.Add(new FieldEditor<double>(fld[i]) { Object = this });
                    }
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
                        ret.Add(new VectorFieldEditor(props[i]) { Object = this });
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
                    if (props[i].PropertyType == typeof(string))
                    {
                        ret.Add(new StringFieldEditor(props[i]) { Object = this });
                    }
                }
            }
            return ret.ToArray();
        }

        public virtual void MoveTo(Vector3d vector)
        {

        }
    }


}
