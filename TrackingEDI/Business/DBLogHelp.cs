using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
    public class DBLogHelp
    {
        public static List<string> ErrorMessager = new List<string>();
        static Dictionary<string, Func<DataRow, EditInstruct, Dictionary<string, object>, Tuple<string, EditInstruct>>> _eiFuncs = new Dictionary<string, Func<DataRow, EditInstruct, Dictionary<string, object>, Tuple<string, EditInstruct>>>();
        public static void RegisterEditInstructFunc(string type, Func<DataRow, EditInstruct, Dictionary<string, object>, Tuple<string, EditInstruct>> func)
        {
            if (_eiFuncs.ContainsKey(type))
                return;
            _eiFuncs[type] = func;
        }

        public static void WriteDBLog(MixedList ml, EditInstruct ei, string mappingName, Dictionary<string, object> parm = null)
        {
            string table = ei.ID;
            string[] key = ei.GetKeySet();

            Func<string> GetCondition = () =>
            {
                string condition = "";
                foreach (string col in key)
                {
                    condition += string.IsNullOrEmpty(condition) ? "" : " AND ";
                    condition += string.Format(" {0}={1}", col, SQLUtils.QuotedStr(ei.Get(col)));
                }
                return string.IsNullOrEmpty(condition) ? " 1=2 " : condition;
            };
            string sql = string.Format("SELECT * FROM {0} WHERE {1}", table, GetCondition());
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return;
            Func<DataRow, EditInstruct, Dictionary<string, object>, Tuple<string, EditInstruct>> func = null;
            if (_eiFuncs.ContainsKey(mappingName))
                func = _eiFuncs[mappingName];

            if (func != null)
            {
                if (parm.ContainsKey("Col"))
                {
                    if (!parm.ContainsKey("ml"))
                    {
                        parm["ml"] = ml;
                    }
                    Tuple<string, EditInstruct> result = func(dt.Rows[0], ei, parm);
                    string resultstr = result.Item1;
                    if (!string.IsNullOrEmpty(resultstr))
                    {
                        ErrorMessager.Add(resultstr);
                        return;
                    }
                    ml = parm["ml"] as MixedList;
                }
                else
                {
                    Tuple<string, EditInstruct> result = func(dt.Rows[0], ei, parm);
                    string resultstr = result.Item1;
                    EditInstruct ei2 = result.Item2;
                    if (!string.IsNullOrEmpty(resultstr))
                    {
                        ErrorMessager.Add(resultstr);
                        return;
                    }
                    else if (ei2 != null)
                    {
                        ml.Add(ei2);
                    }
                }
            }
        }

        public static Tuple<string, string> GetChageValue(DataRow dr, EditInstruct ei, Dictionary<string, object> parm = null)
        {
            string newvalue = string.Empty;
            string oldvalue = string.Empty;
            List<string> ignorecol = new List<string>();
            if (parm.ContainsKey("ignorecol"))
                ignorecol = parm["ignorecol"] as List<string>;

            foreach (string col in ei.getNameSet())
            {
                if (ignorecol.Contains(col)) continue;
                string newval = ei.Get(col);
                if (!dr.Table.Columns.Contains(col)) continue;
                string oldval = Prolink.Math.GetValueAsString(dr[col]);
                if (newval != oldval)
                {
                    string newcol = Prolink.V3.ModelFactory.GetModelFiledName(col);
                    newval += string.Format("{0}:{1}", newcol, newval);
                    oldvalue += string.Format("{0}:{1}", newcol, oldval);
                }
            }
            return new Tuple<string, string>(newvalue, oldvalue);
        }

        public static Tuple<bool, string, string> GetChageValue(DataRow dr, EditInstruct ei, string col, Dictionary<string, object> parm = null)
        {
            string newvalue = string.Empty;
            string oldvalue = string.Empty;
            List<string> ignorecol = new List<string>();
            if (parm.ContainsKey("ignorecol"))
                ignorecol = parm["ignorecol"] as List<string>;
            bool isok = true;
            if (!ignorecol.Contains(col) && dr.Table.Columns.Contains(col) && ei.getNameSet().Contains(col))
            {
                if (ei.IsDate(col))
                {
                    newvalue = Prolink.Math.GetValueAsDateTime(ei.Get(col)).ToString("yyyyMMddHHmm");
                    oldvalue = Prolink.Math.GetValueAsDateTime(dr[col]).ToString("yyyyMMddHHmm");
                }
                else
                {
                    newvalue = ei.Get(col);
                    oldvalue = Prolink.Math.GetValueAsString(dr[col]);
                }
                if (newvalue != oldvalue)
                {
                    isok = false;
                }
            }

            return new Tuple<bool, string, string>(isok, newvalue, oldvalue);
        }


        public static Tuple<string, EditInstruct> Handlesysacct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            Dictionary<string, string> coldic = parm["Col"] as Dictionary<string, string>;
            Dictionary<string, Dictionary<string, string>> ColValue = parm["ColValue"] as Dictionary<string, Dictionary<string, string>>;
            MixedList ml = parm["ml"] as MixedList;

            Func<string, string, string> GetColValue = (col, value) =>
            {
                if (!string.IsNullOrEmpty(value))
                    switch (col)
                    {
                        case "CMP_PRI":
                        case "TRAN_TYPE":
                            string[] values = value.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                            List<string> valuelist = new List<string>();
                            foreach (string val in values)
                            {
                                if (ColValue[col].ContainsKey(val))
                                {
                                    valuelist.Add(ColValue[col][val]);
                                }
                            }
                            if (valuelist.Count > 0)
                                value = string.Join(";", valuelist);
                            break;
                        default:
                            if (ColValue[col].ContainsKey(value))
                            {
                                value = ColValue[col][value];
                            }
                            break;
                    }
                return value;
            };

            foreach (var key in coldic)
            {
                Tuple<bool, string, string> result = GetChageValue(dr, ei, key.Key, parm);
                if (!result.Item1)
                {
                    string oldvalue = result.Item3;
                    string newvalue = result.Item2;
                    if (ColValue.ContainsKey(key.Key))
                    {
                        oldvalue = GetColValue(key.Key, oldvalue);
                        newvalue = GetColValue(key.Key, newvalue);
                    }
                    EditInstruct ei1 = new EditInstruct("SYS_ACCT_LOG", EditInstruct.INSERT_OPERATION);
                    ei1.Put("GROUP_ID", dr["GROUP_ID"]);
                    ei1.Put("CMP", dr["CMP"]);
                    ei1.Put("USER_ID", dr["U_ID"]);
                    ei1.Put("FIELD_CODE", key.Key);
                    ei1.Put("FIELD_NAME", key.Value);
                    ei1.Put("OLD_VALUE", oldvalue);
                    ei1.Put("UPDATE_VALUE", newvalue);
                    ei1.Put("IT_SD", parm["ItSd"]);
                    ei1.Put("MODIFY_BY", parm["user"]);
                    ei1.PutDate("MODIFY_DATE", DateTime.Now);
                    ml.Add(ei1);
                }
            }
            return new Tuple<string, EditInstruct>("", null);
        }
    }
}