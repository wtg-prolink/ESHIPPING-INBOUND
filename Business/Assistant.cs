using Business.Attribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace Business
{
    public static class Assistant
    {
        public static T ToJsonObj<T>(string jsonStr) where T : class
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Deserialize(jsonStr, typeof(T)) as T;
        }

        public static dynamic ToJsonDynamicObj(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
            return jss.Deserialize(json, typeof(object)) as dynamic;
        }

        public static string ToJsonString<T>(IList<T> list, Func<T, string> fun)
        {
            return string.Join(",", list.Select(t => fun(t)));
        }

        public static string ToJsonString(object o)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            JavaScriptSerializer json = new JavaScriptSerializer();
            json.Serialize(o, sb);
            return sb.ToString();
        }

        public static string ToJsonString<T>(T t, Func<T, string> fun)
        {
            return fun(t);
        }
    }

    public class DynamicJsonConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(object))
            {
                return new DynamicJsonObject(dictionary);
            }

            return null;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(object) })); }
        }
    }
    public class DynamicJsonObject : DynamicObject
    {
        private IDictionary<string, object> Dictionary { get; set; }

        public DynamicJsonObject(IDictionary<string, object> dictionary)
        {
            this.Dictionary = dictionary;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.Dictionary[binder.Name];

            if (result is IDictionary<string, object>)
            {
                result = new DynamicJsonObject(result as IDictionary<string, object>);
            }
            else if (result is ArrayList && (result as ArrayList) is IDictionary<string, object>)
            {
                result = new List<DynamicJsonObject>((result as ArrayList).ToArray().Select(x => new DynamicJsonObject(x as IDictionary<string, object>)));
            }
            else if (result is ArrayList)
            {
                result = new List<object>((result as ArrayList).ToArray());
            }

            return this.Dictionary.ContainsKey(binder.Name);
        }
    }
}