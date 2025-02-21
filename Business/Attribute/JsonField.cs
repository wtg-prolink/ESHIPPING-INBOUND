using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Attribute
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class JsonFieldAttribute : System.Attribute
    {
        public JsonFieldAttribute()
        {

        }
        public JsonFieldAttribute(string name)
        {
            Name = name;
        }

        public string Name
        {
            get;
            set;
        }
    }
}
