using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Prolink.DataOperation;

namespace TrackingEDI.Business
{
    public class BaseParser
    {
        public static readonly string ERROR = "ERROR";
        public static readonly string END = "END";
        StringBuilder warningMessager = new StringBuilder();
        public List<string> ErrorMessager = new List<string>();
        static Dictionary<string, Func<DataRow, EditInstruct, Dictionary<string, object>, string>> _eiFuncs = new Dictionary<string, Func<DataRow, EditInstruct, Dictionary<string, object>, string>>();

        public static void RegisterEditInstructFunc(string type, Func<DataRow, EditInstruct, Dictionary<string, object>, string> func)
        {
            if (_eiFuncs.ContainsKey(type))
                return;
            _eiFuncs[type] = func;
        }
        /// <summary>
        /// 将DataTable转换成EditInstruct
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public MixedList ParseEditInstruct(DataTable dt, string mappingName,MixedList list = null,Dictionary<string,object> parm=null,bool isXls=false)
        {
            if (list == null)
                list = new MixedList();
            Func<DataRow, EditInstruct, Dictionary<string, object>, string> func = null;
            if (_eiFuncs.ContainsKey(mappingName))
                func = _eiFuncs[mappingName];

            Dictionary<string, object> mapping = XmlParser.GetMapping(mappingName, isXls);
            StringBuilder KeyWhereSb = new StringBuilder();
            //获取表名
            string tableName = XmlParser.GetTableName(mapping);
            EditInstruct ei = null;
            Dictionary<string, object> fields = XmlParser.GetFields(mapping);
            Dictionary<string, object> field = null;
            List<string> keys = XmlParser.GetKeys(mapping);

            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty, dataType = string.Empty; //字段值
            bool isKey = false;

            List<string> msgs = new List<string>();

            #region 验证上传匹配个数
            List<string> filedList = new List<string>();
            foreach (var kv in fields)
            {
                field = kv.Value as Dictionary<string, object>;
                fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                //string caption = Prolink.Math.GetValueAsString(field["name"]);
                if (mappingName.StartsWith("Quot"))//报价相关验证
                {
                    switch (fieldname)
                    {
                        case "REMARK":
                        case "VIA_NM":
                        case "SAILING_DAY":
                        case "FREE_ODT":
                        case "FREE_ODM":
                        case "FREE_DDT":
                        case "FREE_DDM":
                        case "NOTE":
                        case "CUT_OFF":
                        case "ETD":
                        case "POD_NM":
                        case "POL_NM":
                        case "REGION_EN":
                        case "ALL_IN":
                        case "STATE_NM":
                        case "REGION_NM":   
                            continue;
                    }
                    if (fieldname.Length <= 3 && fieldname.StartsWith("F"))
                        continue;
                }
                if (!filedList.Contains(fieldname))
                {
                    filedList.Add(fieldname);

                    List<string> clist = GetNameList(fields, fieldname);

                    bool check = false;
                    foreach (DataColumn col in dt.Columns)
                    {
                        string name = col.ColumnName.ToUpper().Trim().Replace(System.Environment.NewLine, "").Replace("\n", "");
                        if (clist.Contains(name))
                        {
                            check = true;
                            break;
                        }
                    }
                    if (!check)
                    {
                        string msg = string.Format("缺少{0};", kv.Key);
                        if (!ErrorMessager.Contains(msg))
                            ErrorMessager.Add(msg);
                    }
                }
            }
            #endregion

            foreach (DataRow dr in dt.Rows)
            {
                ei = new EditInstruct(tableName, EditInstruct.INSERT_OPERATION);
                foreach (DataColumn col in dt.Columns)
                {
                    string name = col.ColumnName.ToUpper().Trim().Replace(System.Environment.NewLine, "").Replace("\n", "");
                    if (!fields.ContainsKey(name))
                    {
                        //string msg = string.Format("{0}不是有效表头;", name);
                        //if (!ErrorMessager.Contains(msg) && !name.ToUpper().StartsWith("COLUMN"))
                        //    ErrorMessager.Add(msg);
                        continue;
                    }
                    field = fields[name] as Dictionary<string, object>;
                    isKey = keys.Contains(name);
                    fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                    defalutValue = Prolink.Math.GetValueAsString(field["defalutValue"]);
                    dataType = Prolink.Math.GetValueAsString(field["dataType"]);
                    fieldValue = Prolink.Math.GetValueAsString(dr[col.ColumnName]);
                    if (!string.IsNullOrEmpty(fieldValue))
                        fieldValue = fieldValue.Trim();

                    string isnotnull = string.Empty;
                    if (field.ContainsKey("isnotnull"))
                        isnotnull = Prolink.Math.GetValueAsString(field["isnotnull"]);
                    if ("Y".Equals(isnotnull))
                    {
                        switch (dataType)
                        {
                            case "number":
                                if (Prolink.Math.GetValueAsDecimal(fieldValue) == 0)
                                    msgs.Add(name);
                                break;
                            default:
                                if (string.IsNullOrEmpty(fieldValue))
                                    msgs.Add(name);
                                break;
                        }
                    }

                    if (isKey && string.IsNullOrEmpty(fieldValue))
                    {
                        throw new Exception(string.Format("Field:{0} is key,key must is not null", name));
                    }
                    XmlParser.PutToEditInstruct(ei, fieldname, fieldValue, dataType, isKey, warningMessager);
                }
                if (func != null)
                {
                    string result = func(dr, ei, parm);
                    if (ERROR.Equals(result))
                        continue;
                    else if (END.Equals(result))
                        return list;
                }
                list.Add(ei);
            }
            if (msgs.Count > 0)
            {
                throw new Exception(string.Format("Import failed! {0} is key item, cannot be kept blank! Please check again.", string.Join(",", msgs)));
            }
            return list;
        }

        private static List<string> GetNameList(Dictionary<string, object> fields, string name)
        {
            List<string> list=new List<string>();
            foreach (var kv in fields)
            {
                Dictionary<string, object> field = kv.Value as Dictionary<string, object>;
                string fieldname = Prolink.Math.GetValueAsString(field["fieldname"]);
                //string caption = Prolink.Math.GetValueAsString(field["name"]);
                if (name.Equals(fieldname))
                    list.Add(kv.Key);
            }
            return list;
        }

        static string PartyEditInstruct(DataRow dr, EditInstruct ei)
        {
            return string.Empty;
        }

        #region xml 节点操作
        public System.Xml.XmlNode GetXmlNode(XmlDocument xmldoc, string RootName, string NodeName)
        {
            try
            {
                System.Xml.XmlNode XnNode = xmldoc.SelectSingleNode(string.Format("//{0}/node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", new object[] { RootName, NodeName.ToLower() }));
                if (XnNode != null)
                {
                    return XnNode;
                }
                else
                    return null;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return null;
            }
        }

        public string GetXmlNodeValue(XmlDocument receiverXML, string RootName, string NodeName)
        {
            try
            {
                string value = string.Empty;
                System.Xml.XmlNode XnNode = receiverXML.SelectSingleNode(string.Format("//{0}/node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", new object[] { RootName, NodeName.ToLower() }));
                if (XnNode != null)
                {
                    value = XnNode.InnerText;
                    if (!string.IsNullOrEmpty(value))
                        value = value.Trim();
                    return value;
                }
                else
                    return value;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return string.Empty;
            }
        }

        public static string GetXmlNodeValue(XmlNode xn, string NodeName)
        {
            try
            {
                string value = string.Empty;
                System.Xml.XmlNode XnNode = xn.SelectSingleNode(string.Format("node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{0}']", NodeName.ToLower()));
                if (XnNode != null)
                {
                    value = XnNode.InnerText;
                    if (!string.IsNullOrEmpty(value))
                        value = value.Trim();
                    return value;
                }
                else
                    return value;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
