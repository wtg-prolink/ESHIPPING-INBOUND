using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Prolink.DataOperation;
using System.Collections.Specialized;
using Prolink.Data;
using Prolink.V3;


namespace WebGui.Reporter
{
    public class FCL02DataGetter : Prolink.WebReport.Business.IDataGetter
    {

        public DataSet GetDataSet(NameValueCollection nameValues)
        {
            nameValues.Add("RPT_DESCP_PARAMTER", "Picking List");
            //string uId = Prolink.Math.GetValueAsString(nameValues["uid"]);
            /* DataSet ds = new DataSet();
             string sql = "SELECT * FROM V_CHM02";
             DataTable dt = GetDataTable("V_CHM02", sql);
             */
            string lcno = Prolink.Math.GetValueAsString(nameValues["UID"]);
            DataSet ds = new DataSet();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = "";
            string conditions = Prolink.Math.GetValueAsString(nameValues["conditions"]);
            string uid = nameValues["uid"];
            if (!string.IsNullOrEmpty(uid))
            {
                condition = string.Format(" U_ID={0}", SQLUtils.QuotedStr(uid));
                nameValues["conditions"] = "";
            }
            //Condition(ref con, ref condition);
            DataTable dt = ModelFactory.InquiryData("V_FCL02.*", "V_FCL02", condition, nameValues, ref recordsCount, ref pageIndex, ref pageSize);
            condition = string.Empty;
            if (dt.Rows.Count > 0)
            {
                string combinshipment = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBIN_SHIPMENT"]);
               
                if (!string.IsNullOrEmpty(combinshipment))
                {
                    string sql = string.Format("SELECT U_ID,SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT={0}", SQLUtils.QuotedStr(combinshipment));
                    DataTable dt2 = OperationUtils.GetDataTable(sql, new string[] { }, ReporterRegister.DelegateConnection);
                    
                    foreach (DataRow dr in dt2.Rows)
                    {
                        string shipment = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        string id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        if (shipment != combinshipment)
                        {
                            condition += SQLUtils.QuotedStr(id) + ",";
                        }
                    }
                    if (!string.IsNullOrEmpty(condition))
                    {
                        condition = condition.Substring(0, condition.Length - 1);
                        condition = string.Format(" A.U_ID IN ({0})",condition);
                    }
                }
            }
            if (string.IsNullOrEmpty(condition))
                condition = " 1=2";
            string sql1 = string.Format(@"SELECT A.U_ID,A.SHIPMENT_ID,A.MARKS,A.GOODS,(SELECT TOP 1 COMMODITY FROM SMSIM
         WHERE SMSIM. [PROFILE] = A.PROFILE_CD) AS SI_COMMODITY,A.BL_RMK,
         A.GWU,A.GW,A.CBM,(SELECT TOP 1 ISNULL(CD_DESCP, A.PKG_UNIT)  FROM BSCODE WHERE BSCODE.CD_TYPE = 'UB'
           AND BSCODE.CD = A.PKG_UNIT AND BSCODE.GROUP_ID = A.GROUP_ID) PKG_UNIT_UB,A.PKG_NUM,A.QTY,
         A.LOADING_FROM,A.LOADING_TO,A.FRT_TERM,D.CNTR_NO,D.SEAL_NO1,D.CNTY_TYPE,D.GW AS V_GW,D.GWU AS V_GWU,D.CBM AS V_CBM,
         A.PKG_NUM AS N_TTL_PLT,A.PKG_UNIT AS N_PLTU,A.COMBIN_SHIPMENT,
         (ISNULL('20''*'+convert(varchar(10),A.CNT20)+' ,','')+ISNULL('40''GP*'+convert(varchar(10),A.CNT40)+' ,','')+ISNULL('40''HQ*'+convert(varchar(10),A.CNT40HQ)+' ,','')) AS CntType
  FROM SMSM A  LEFT JOIN SMRV D  ON A.SHIPMENT_ID = D.SHIPMENT_ID WHERE {0}", condition);
            //            if (string.IsNullOrEmpty(condition))
            //            {
            //                Condition(ref conditions, ref condition);
            //                nameValues.Set("conditions", "");
            //            }
            //            else
            //            {
            //                if (!string.IsNullOrEmpty(condition))
            //                {
            //                    condition = @" EXISTS (select A.*  from SMSM A
            //                 WHERE " + condition + "AND V_FCL02.COMBIN_SHIPMENT = A.SHIPMENT_ID ) AND V_FCL02.COMBIN_SHIPMENT != V_FCL02.SHIPMENT_ID";
            //                    conditions = "";
            //                }
            //            }
            //DataTable dt1 = ModelFactory.InquiryData("V_FCL02.*", "V_FCL02", condition, nameValues, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt1 = OperationUtils.GetDataTable(sql1, new string[] { }, ReporterRegister.DelegateConnection);
            try
            {
                ds.Tables.Add(dt);
                ds.Tables.Add(dt1);
            }
            catch (Exception ex)
            {
            }
            return ds;
        }
        //protected void Condition(ref string con, ref string condition)
        //{
        //    string[] cs = con.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (string c in cs)
        //    {
        //        if (c.Contains("UId") && c.IndexOf("UId") < 2)
        //        {
        //            string[] uid = c.Split('=');
        //            if (uid.Length == 2)
        //            {
        //                condition = "U_ID= " + SQLUtils.QuotedStr(uid[1]) + " OR ( EXISTS (select  A.* from SMSM A WHERE A.U_ID= " + SQLUtils.QuotedStr(uid[1]) + " AND V_FCL01.COMBIN_SHIPMENT=A.SHIPMENT_ID)) ";
        //                con = con.Replace(c + "&", "");
        //            }
        //        }
        //        //if()
        //    }
        //}
        protected void Condition(ref string conditions, ref string condition)
        {
            if (conditions != null)
                conditions = HttpUtility.UrlDecode(conditions);
            //条件参数
            if (!string.IsNullOrEmpty(conditions))
            {
                //sopt_id=eq&id=sadf&sopt_invdate=ne&invdate=sdfasdfafd&_search=false&nd=1422945681209
                string[] cs = conditions.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                string[] cs1 = null;
                Dictionary<string, string> eq = new Dictionary<string, string>();
                Dictionary<string, string> pdt = new Dictionary<string, string>();
                Dictionary<string, string> data = new Dictionary<string, string>();
                #region 构建条件参数
                foreach (string c in cs)
                {
                    if (string.IsNullOrEmpty(c))
                        continue;
                    cs1 = c.Split(new string[] { "=" }, StringSplitOptions.None);
                    if (cs1.Length < 2)
                        continue;
                    if (cs1[0].StartsWith("sopt_"))
                    {
                        eq[cs1[0]] = cs1[1];
                    }
                    else if (cs1[0].StartsWith("dt_"))
                    {
                        pdt[cs1[0]] = cs1[1];
                    }
                    else
                    {
                        data[cs1[0]] = cs1[1];
                    }
                }
                #endregion
                #region 生成条件
                string fieldName = string.Empty;
                string[] fkv;
                foreach (var kv in data)
                {
                    fkv = kv.Key.Split(new string[] { "$" }, StringSplitOptions.RemoveEmptyEntries);

                    if (eq.ContainsKey("sopt_" + fkv[0]))
                    {
                        if (kv.Value == string.Empty && eq["sopt_" + fkv[0]] != "nu" && eq["sopt_" + fkv[0]] != "nn")
                            continue;
                        fieldName = ReplaceFiledToDBName(fkv[0]);
                        if (pdt.ContainsKey("dt_" + fkv[0]) && pdt["dt_" + fkv[0]] != "")
                        {
                            fieldName = ReplaceFiledToDBName(pdt["dt_" + fkv[0]]) + "." + fieldName;
                        }
                        if (!string.IsNullOrEmpty(condition))
                            condition += " AND ";
                        switch (eq["sopt_" + fkv[0]])
                        {
                            case "eq": //eq 等于( = )
                                condition += fieldName + " = " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "ne": //ne 不等于( <> )
                                condition += fieldName + " <> " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "lt": //lt 小于( < )
                                condition += fieldName + " < " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "le": //le 小于等于( <= )
                                condition += fieldName + " <= " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "gt": //gt 大于( > )
                                condition += fieldName + " > " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "ge":  //ge 大于等于( >= )
                                condition += fieldName + " >= " + SQLUtils.QuotedStr(kv.Value);
                                break;
                            case "bw":  //bw 开始于 ( LIKE val% )
                                condition += fieldName + " LIKE '" + kv.Value + "%'";
                                break;
                            case "bn":  //bn 不开始于 ( not like val%)
                                condition += fieldName + " NOT LIKE '" + kv.Value + "%'";
                                break;
                            case "in":  //in 在内 ( in ())
                                condition += fieldName + " IN (" + SQLUtils.QuotedStr(kv.Value) + ")";
                                break;
                            case "ni":  //ni 不在内( not in ())
                                condition += fieldName + " NOT IN (" + SQLUtils.QuotedStr(kv.Value) + ")";
                                break;
                            case "ew":    //ew 结束于 (LIKE %val )
                                condition += fieldName + " LIKE '%" + kv.Value + "'";
                                break;
                            case "en":  //en 不结束于
                                condition += fieldName + " NOT LIKE '%" + kv.Value + "'";
                                break;
                            case "cn":  //cn 包含 (LIKE %val% )
                                condition += fieldName + " LIKE " + SQLUtils.QuotedStr("%" + kv.Value + "%");
                                break;
                            case "nc":  //nc 不包含
                                condition += fieldName + " NOT LIKE " + SQLUtils.QuotedStr("%" + kv.Value + "%");
                                break;
                            case "nu":  // is null
                                condition += fieldName + " IS NULL";
                                break;
                            case "nn":  //is not null
                                condition += fieldName + " IS NOT NULL";
                                break;
                            case "bt":  //between

                                if (fkv.Length > 1 && fkv[1].Equals("S"))
                                {
                                    condition += fieldName + " >= " + SQLUtils.QuotedStr(kv.Value);
                                }
                                if (fkv.Length > 1 && fkv[1].Equals("E"))
                                {
                                    condition += fieldName + " <= " + SQLUtils.QuotedStr(kv.Value);
                                }
                                break;
                            default:
                                condition += fieldName + " = " + SQLUtils.QuotedStr(kv.Value);
                                break;
                        }
                    }
                }
                #endregion
            }
            if (!string.IsNullOrEmpty(condition))
            {
                condition = @" EXISTS (select A.*  from SMSM A
                 WHERE " + condition + "AND V_FCL02.COMBIN_SHIPMENT = A.SHIPMENT_ID ) AND V_FCL02.COMBIN_SHIPMENT != V_FCL02.SHIPMENT_ID";
                conditions = "";
            }
        }
        public static string ReplaceFiledToDBName(string fieldName)
        {
            string replace = "";
            bool a = false;
            for (int i = 0; i < fieldName.Length; i++)
            {
                char code = Convert.ToChar(fieldName[i]);
                if (code <= 90 && code >= 65)
                {
                    if (a)
                    {
                        replace += "_" + fieldName[i].ToString().ToUpper();
                    }
                    else
                    {
                        replace += fieldName[i].ToString().ToUpper();
                    }
                    a = true;
                }
                else if (code <= 122 && code >= 97)
                {
                    replace += fieldName[i].ToString().ToUpper();
                }
                else
                    replace += fieldName[i].ToString();
            }
            return replace;
        }
        public DataTable GetDataTable(string name, string sql)
        {
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, ReporterRegister.DelegateConnection);
            dt.TableName = name;
            return dt;
        }
    }
}