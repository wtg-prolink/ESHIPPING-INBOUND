using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.Utils
{
    public class XmlUtil
    {
        public static T Deserialize<T>(string xml) where T : class
        {
            using (StringReader sr = new StringReader(xml))
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                return xmldes.Deserialize(sr) as T;
            }
        }

        public static string Serializer<T>(T obj) where T : class
        {
            using (MemoryStream Stream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(Stream, obj);
                Stream.Position = 0;
                using (StreamReader sr = new StreamReader(Stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static string SerializerUTF8<T>(T obj) where T : class
        {
            using (MemoryStream Stream = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(Stream, obj);
                Stream.Position = 0;
                //Encoding.GetEncoding("UTF-8"));
                using (StreamReader sr = new StreamReader(Stream, Encoding.GetEncoding("UTF-8")))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
