using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Business.EDI
{
    public abstract class XmlEDITemplateBase
    {
        public abstract XmlEDINode CreateXmlEDINode();
        public XmlDocument ToXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xd = CreateXmlDeclaration(doc);
            doc.AppendChild(xd);

            XmlEDINode rootNode = CreateXmlEDINode();

            XmlElement root = CreateElement(doc, rootNode);
            doc.AppendChild(root);

            foreach (XmlEDINode xmlItem in rootNode.ChildNodes)
            {
                AppendElement(xmlItem, root);
            }
            return doc;
        }

        protected virtual XmlDeclaration CreateXmlDeclaration(XmlDocument doc)
        {
            return doc.CreateXmlDeclaration("1.0", "utf-8", null);
        }

        void AppendElement(XmlEDINode xmlItem, XmlElement parentNode)
        {
            XmlElement node = CreateElement(parentNode.OwnerDocument, xmlItem);
            parentNode.AppendChild(node);
            foreach (XmlEDINode item in xmlItem.ChildNodes)
            {
                if (IsFilterNullValue && item.ChildNodes.Count <= 0 && item.Value == null) continue;
                AppendElement(item, node);
            }
        }
        [XmlIgnore]
        public bool IsFilterNullValue
        {
            get;
            set;
        }

        protected DateTime? ParseToDateTimeForNullValue(string dateStr, string formart = "yyyyMMddHHmm")
        {
            DateTime dt = ParseToDateTime(dateStr, formart);
            if (dt == DateTime.MinValue) return null;
            return dt;
        }

        protected DateTime ParseToDateTime(string dateStr, string formart = "yyyyMMddHHmm")
        {
            if (string.IsNullOrEmpty(dateStr)) return DateTime.MinValue;
            DateTime time;
            if (DateTime.TryParseExact(dateStr, formart, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat,
                System.Globalization.DateTimeStyles.None, out time))
            {
                return time;
            }
            else
            {
                if (DateTime.TryParse(dateStr, out time))
                    return time;
            }
            return DateTime.MinValue;
        }

        XmlElement CreateElement(XmlDocument doc, XmlEDINode node)
        {
            XmlElement element = null;
            if (node.Name.IndexOf(':') > 0)
            {
                string[] strs = node.Name.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                var att = node.Attributes.Where(item => item.Name.IndexOf(strs[0]) > 0).FirstOrDefault();
                if (att != null)
                {
                    element = doc.CreateElement(strs[0], strs[1], att.Value);
                }
                else
                    element = doc.CreateElement(strs[0], strs[1], "");
            }
            else
                element = doc.CreateElement(node.Name);
            element.InnerText = node.Value;
            foreach (XmlAttr item in node.Attributes)
            {
                if (!string.IsNullOrEmpty(item.NameSpace))
                {
                    string[] strs = item.Name.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    element.SetAttribute(strs[1], item.NameSpace, item.Value);
                }
                else
                    element.SetAttribute(item.Name, item.Value);
            }
            return element;
        } 
    }

    public class XmlEDINode
    {
        public XmlEDINode(string name)
            : this(name, null)
        {

        }

        public XmlEDINode(string name, string value)
        {
            Name = name;
            Value = value;
            ChildNodes = new List<XmlEDINode>();
            Attributes = new List<XmlAttr>();
        }

        public string Value
        {
            get;
            set;
        }

        public string Name
        {
            get;
            private set;
        }

        public List<XmlAttr> Attributes
        {
            get;
            private set;
        }

        public List<XmlEDINode> ChildNodes
        {
            get;
            private set;
        }
    }

    public class XmlAttr
    {
        public XmlAttr(string name)
            : this(name, null)
        {

        }

        public XmlAttr(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public XmlAttr(string name, string nameSpace, string value)
        {
            Name = name;
            NameSpace = nameSpace;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public string NameSpace { get; set; }
    }
}


