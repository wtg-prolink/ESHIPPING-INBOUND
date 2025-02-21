using Prolink;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.InboundBusiness
{
    public class ReserveHelper
    {
        public static string getAutoNo(string ruleCode, string GroupId, string CompanyId)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            hash.Add(CompanyId, ruleCode);
            string ReserveNo = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");
            ReserveNo = CompanyId + ReserveNo;
            return ReserveNo;
        }

        public static string getAutoNoByHash(string ruleCode, string GroupId, string CompanyId, System.Collections.Hashtable hash)
        {
            string ReserveNo = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");
            ReserveNo = CompanyId + ReserveNo;
            return ReserveNo;
        }

        /// <summary>
        /// 将传入的DataTable的内容,转换成对应的传入的Table Name的 EditInstruct 并加入到MixedList中
        /// </summary>
        /// <param name="dt">需要转换的Datatable</param>
        /// <param name="tableName">转换后的EditInstruct Key</param>
        /// <param name="ml">MixedList</param>
        /// <param name="uid">此笔资料的U_ID</param>
        /// <param name="parm">传入的需要替换的dictionary 键值对</param>
        public static void ToEi(DataTable dt, string tableName, MixedList ml, string uid, Dictionary<string, object> parm, Dictionary<string, string> filesname = null)
        {
            EditInstruct ei = null;
            string name = string.Empty;
            if (dt == null || dt.Rows.Count <= 0) return;
            foreach (DataRow dr in dt.Rows)
            {
                ei = new EditInstruct(tableName, EditInstruct.INSERT_OPERATION);
                foreach (DataColumn col in dt.Columns)
                {
                    name = col.ColumnName;
                    switch (name.ToUpper())
                    {
                        case "U_ID":
                            if (tableName.Equals("SMSMI"))
                            {
                                ei.Put("U_ID", uid);
                            }
                            else
                            {
                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            }
                            continue;
                        case "U_FID":
                            ei.Put("U_FID", uid);
                            continue;
                    }
                    if (dr[name] is DateTime)
                    {
                        if (dr[name] != null && dr[name] != DBNull.Value)
                            ei.PutDate(name, (DateTime)dr[name]);
                        if (parm != null)
                        {
                            if (parm.ContainsKey(name))
                            {
                                ei.PutDate(name, parm[name]);
                            }
                        }
                    }
                    else
                        ei.Put(name, dr[name]);

                    if (parm != null)
                    {
                        if (parm.ContainsKey(name))
                        {
                            ei.Put(name, parm[name]);
                        }
                    }

                    if (filesname != null)
                    {
                        if (filesname.ContainsKey(name))
                        {
                            ei.Put(name, dr[filesname[name]]);
                        }
                    }
                }
                ml.Add(ei);
            }
        }

    }
}
