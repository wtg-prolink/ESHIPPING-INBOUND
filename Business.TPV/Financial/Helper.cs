using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Business.TPV.Financial
{
    public class Helper
    {
        #region 工具方法
        public static decimal GetValueAsDecimal(DataRow row, string[] names)
        {
            if (row == null)
                return 0m;
            foreach (string name in names)
            {
                if (!row.Table.Columns.Contains(name))
                    continue;
                decimal val = Prolink.Math.GetValueAsDecimal(row[name]);
                if (val > 0)
                    return val;
            }
            return 0m;
        }

        public static string GetValueAsString(DataRow row, string[] names)
        {
            if (row == null)
                return string.Empty; ;
            foreach (string name in names)
            {
                if (!row.Table.Columns.Contains(name))
                    continue;
                string val = Prolink.Math.GetValueAsString(row[name]);
                if (!string.IsNullOrEmpty(val))
                    return val;
            }
            return string.Empty; ;
        }

        public static EditInstruct CopyEditInstruct(EditInstruct ei)
        {
            EditInstruct cei = new EditInstruct(ei.ID, ei.OperationType);
            string[] keys = ei.GetKeySet();
            string[] dates = ei.GetDateSet();
            string[] fields = ei.getNameSet();
            foreach (string name in fields)
            {
                cei.Put(name, ei.Get(name));
            }
            foreach (string name in keys)
            {
                cei.AddKey(name);
            }
            foreach (string date in dates)
            {
                cei.AddDate(date);
            }
            cei.Condition = ei.Condition;
            return cei;
        }

        public static decimal GetEexpressGw(decimal gw, decimal baseVal = 1m)
        {
            int gw1 = (int)gw;
            if (gw > gw1)
            {
                gw = gw1 + baseVal;
            }
            return gw;
        }

        public static decimal GetCW(decimal gw, decimal cbm, decimal baseVal = 5000m, string tranType = "",decimal bw=0)
        {
            //cbm*167
            //0.5 以下便0.5 ,  0.5 以上進位
            if (baseVal == 0) baseVal = 1M;
            decimal cw = gw;
            switch (tranType)
            {
                case "A":
                    cbm = cbm * 167;
                    if (cbm > cw)
                        cw = cbm;
                    break;
                case "E":
                    cbm = Get45AmtValue(cbm * 200, 3);
                    if (cbm > cw)
                        cw = cbm;

                    int cw1 = (int)cw;
                    if (bw > 0) {                  
                        if (cw > bw)
                        {
                            if ((cw - cw1) > 0)
                                cw = cw1 + 1;
                        }
                        else 
                        {
                            if ((cw - cw1) > 0.5m)
                                cw = cw1 + 1;
                            else
                                cw = cw1 + 0.5m;
                        }
                    }
                    else
                    {
                        if (cw > cw1 && cw <= 20)
                        {
                            if ((cw - cw1) <= 0.5M)
                                cw = cw1 + 0.5M;
                            else
                                cw = cw1 + 1M;
                        }
                    }
                   
                    break;
                default:
                    cbm = Get45AmtValue(cbm * 1000000m / baseVal, 3);
                    if (cbm > cw)
                        cw = cbm;
                    break;
            }
            return cw;
        }

        public static decimal GetQty(string punit, Dictionary<string, object> parm, List<string> msg = null)
        {
            decimal qty = 1;
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            decimal gw = 0M;
            decimal cmb = 0M;
            string trackWay = string.Empty;
            if (parm.ContainsKey("TrackWay"))
                trackWay = Prolink.Math.GetValueAsString(parm["TrackWay"]);

            switch (punit)
            {
                case "20GP":
                    qty = Helper.GetDecimalValue(parm["Cnt20"]);
                    break;
                case "40GP":
                    qty = Helper.GetDecimalValue(parm["Cnt40"]);
                    break;
                case "40HQ":
                    qty = Helper.GetDecimalValue(parm["Cnt40hq"]);
                    break;
                case "CNT":
                case "CTR":
                    qty = Helper.GetDecimalValue(parm["CntNum"]);
                    break;
                case "DN":
                    qty = Helper.GetDecimalValue(parm["DnNum"]);
                    break;
                case "BL":
                case "SHT":
                    qty = 1;
                    break;
                case "M3":
                case "CBM":
                    qty = Helper.GetDecimalValue(parm["cbm"]);
                    switch (tranMode)
                    {
                        case "L":
                        case "F":
                            if (qty < 1M)
                                qty = 1M;
                            break;
                    }
                    break;
                case "CTN":
                case "PLT":
                    qty = Helper.GetDecimalValue(parm["cnt"]);
                    break;
                case "CW":
                    decimal cw = 0;
                    if (parm.ContainsKey("nw"))
                    {
                        cw = Helper.GetDecimalValue(parm["nw"]);
                        return cw;
                    }

                    gw = GetQty("KG", parm, msg);
                    cmb = Helper.GetDecimalValue(parm["cbm"]);
                    switch (tranMode)
                    {
                        case "T":
                            if ("A".Equals(trackWay))
                                qty = GetCW(gw, cmb, 6000m, trackWay);
                            else
                                qty = GetCW(gw, cmb, 6000m);
                            break;
                        default:
                            qty = GetCW(gw, cmb, 6000m, tranMode);
                            break;
                    }
                    break;
                case "L":
                case "G":
                case "LB":
                case "K":
                case "KG":
                case "KGM":
                case "KGS":
                    string gwu = Prolink.Math.GetValueAsString(parm["gwu"]).ToUpper();
                    qty = Helper.GetDecimalValue(parm["gw"]);
                    qty = GetKGWeight(punit,qty, ref gwu,msg);
                    break;
                case "%":
                    qty = parm.ContainsKey("Gvalue") ? Prolink.Math.GetValueAsDecimal(parm["Gvalue"]) : 0;
                    break;
                default:
                    qty = 1;
                    break;
            }
            return qty;
        }


        public static decimal GetKGWeight(string punit, decimal qty, ref string gwu, List<string> msg=null)
        {
            if (gwu.StartsWith("T"))
            {
                qty = qty * 1000M;
                gwu = "KG";
            }
            else if (gwu.StartsWith("T"))
            {
                qty = qty * 0.001m;
                gwu = "KG";
            }
             
            if (punit.StartsWith("L") && (gwu.StartsWith("K") || gwu.Contains("公斤") || gwu.Contains("千克")))
            {
                if (msg != null && !msg.Contains("1千克(kg)=2.2046226磅(lb)"))
                    msg.Add("1千克(kg)=2.2046226磅(lb)");
                qty = qty * 2.2046226M;
            }
            else if ((punit.StartsWith("K") || punit.Contains("公斤") || punit.Contains("千克")) && gwu.StartsWith("L"))
            {
                if (msg != null && !msg.Contains("1磅(lb)=0.4535924千克(kg)"))
                    msg.Add("1磅(lb)=0.4535924千克(kg)");
                qty = qty * 0.4535924M;
            }
            return qty;
        }

        /// <summary>
        /// 新增报价相关附加栏位
        /// </summary>
        /// <param name="dt"></param>
        public static void AddOthColumns(DataTable dt)
        {
            if (dt == null)
                return;

            if (!dt.Columns.Contains("EX_REMARK"))
            {
                dt.Columns.Add("EX_REMARK", typeof(string));
                dt.Columns["EX_REMARK"].MaxLength = 9999;
            }

            //QUNIT_PRICE   QCHG_UNIT    QQTY   QAMT
            if (!dt.Columns.Contains("CHG_REMARK"))
            {
                dt.Columns.Add("CHG_REMARK", typeof(string));
                dt.Columns["CHG_REMARK"].MaxLength = 9999;
            }
            if (!dt.Columns.Contains("QCHG_UNIT"))//
            {
                dt.Columns.Add("QCHG_UNIT", typeof(string));
                dt.Columns["QCHG_UNIT"].MaxLength = 20;
            }

            if (!dt.Columns.Contains("LOCALE_AMT"))
                dt.Columns.Add("LOCALE_AMT", typeof(decimal));
            if (!dt.Columns.Contains("QUNIT_PRICE"))
                dt.Columns.Add("QUNIT_PRICE", typeof(decimal));
            if (!dt.Columns.Contains("QQTY"))
                dt.Columns.Add("QQTY", typeof(decimal));
            if (!dt.Columns.Contains("QAMT"))
                dt.Columns.Add("QAMT", typeof(decimal));
            if (!dt.Columns.Contains("QEX_RATE"))
                dt.Columns.Add("QEX_RATE", typeof(decimal));
            if (!dt.Columns.Contains("QTAX"))
                dt.Columns.Add("QTAX", typeof(decimal));

            if (!dt.Columns.Contains("C_FLAG"))
                dt.Columns.Add("C_FLAG", typeof(string));

            if (!dt.Columns.Contains("QCHG_CD"))
                dt.Columns.Add("QCHG_CD", typeof(string));
            if (!dt.Columns.Contains("QCHG_DESCP"))
                dt.Columns.Add("QCHG_DESCP", typeof(string));
            if (!dt.Columns.Contains("QREPAY"))
                dt.Columns.Add("QREPAY", typeof(string));

            if (!dt.Columns.Contains("QCHG_TYPE"))
                dt.Columns.Add("QCHG_TYPE", typeof(string));

            if (!dt.Columns.Contains("IPART_NO"))
                dt.Columns.Add("IPART_NO", typeof(string));

            if (!dt.Columns.Contains("DN_NO"))
                dt.Columns.Add("DN_NO", typeof(string));

            if (!dt.Columns.Contains("CNTR_STD_QTY"))
                dt.Columns.Add("CNTR_STD_QTY", typeof(decimal));
        }

        /// <summary>
        /// 获取卡车类型对照的栏位
        /// </summary>
        /// <param name="carType"></param>
        /// <returns></returns>
        public static string GetCarTypeField(string carType,string cmp)
        {
            string sql = string.Format("SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE = 'TDT' AND(CMP = '*' OR CMP = {0}) AND CD = {1}",
                SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(carType));
            string apcd= OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (string.IsNullOrEmpty(apcd))
                apcd = carType;
            return apcd;
        }

        /// <summary>
        /// 获取栏位对应的卡车名称
        /// </summary>
        /// <param name="carField"></param>
        /// <returns></returns>
        public static string GetCarName(string carField)
        {
            switch (carField)
            {
                case "F7"://40GP
                    return "40GP";
                case "F8"://
                    return "16米";
                case "F4"://
                    return "20GP";
                case "F3"://
                    return "4.2米";
                case "F5"://
                    return "7.2米";
                case "F6"://
                    return "9.6米";
            }
            return carField;
        }

        /// <summary>
        /// 按物流业者复制报价
        /// </summary>
        /// <param name="qtDt"></param>
        /// <param name="party_no"></param>
        /// <returns></returns>
        public static DataTable CloneQTTable(DataTable qtDt, string party_no, string tranMode = "", string term = "", bool byterm = false, string loadingFrom = "", string loadingTo = "")
        {
            DataTable pt_qtDt = qtDt.Clone();
            DataRow[] drs = null;
            string filter = string.Format("LSP_CD={0}", SQLUtils.QuotedStr(party_no));
            string termFilter = string.Empty;
            if (!string.IsNullOrEmpty(term))
                termFilter += string.Format(" AND INCOTERM LIKE'%{0}%'", term); 
            switch (tranMode)
            {
                case "F":
                    if (!string.IsNullOrEmpty(loadingFrom) || !string.IsNullOrEmpty(loadingTo))
                    {
                        string filter1 = string.Empty;
                        string filter2 = " AND LOADING_FROM='CY' AND LOADING_TO='CY'";
                        if (!string.IsNullOrEmpty(loadingFrom))
                            filter1 += " AND LOADING_FROM=" + SQLUtils.QuotedStr(loadingFrom);
                        if (!string.IsNullOrEmpty(loadingTo))
                            filter1 += " AND LOADING_TO=" + SQLUtils.QuotedStr(loadingTo);
                        if (!string.IsNullOrEmpty(filter1))
                        {
                            drs = qtDt.Select(filter + filter1 + termFilter, "QT_EFFECT_FROM DESC");
                            if (drs.Length <= 0 && !filter2.Equals(filter1))
                                drs = qtDt.Select(filter + filter2 + termFilter, "QT_EFFECT_FROM DESC");
                        }
                        else
                        {
                            drs = qtDt.Select(filter + filter2 + termFilter, "QT_EFFECT_FROM DESC");
                        }
                        if (drs.Length <= 0)
                            drs = qtDt.Select(filter + termFilter, "QT_EFFECT_FROM DESC");
                    }
                    else
                        drs = qtDt.Select(filter + termFilter);
                    break;
                default:
                    drs = qtDt.Select(filter + termFilter);
                    break;
            }

            if (drs.Length <= 0 && !string.IsNullOrEmpty(termFilter) && !byterm)
                drs = qtDt.Select(filter);

            foreach (DataRow dr in drs)
            {
                pt_qtDt.ImportRow(dr);
            }
            return pt_qtDt;
        }

        public static DataTable CloneQTTable1(DataTable qtDt, string party_no, string tranMode = "", string term = "", bool byterm = false, string loadingFrom = "", string loadingTo = "")
        {
            DataTable pt_qtDt = qtDt.Clone();
            DataRow[] drs = null;
            string filter = string.Format("LSP_CD={0}", SQLUtils.QuotedStr(party_no));
            string termFilter = string.Empty;
            if (!string.IsNullOrEmpty(term))
            {
                if ("P".Equals(term))
                    termFilter += string.Format(" AND FREIGHT_TERM={0}", SQLUtils.QuotedStr(term));
                else
                    termFilter += " AND FREIGHT_TERM<>'P'";
            }
            switch (tranMode)
            {
                case "F":
                    if (!string.IsNullOrEmpty(loadingFrom) || !string.IsNullOrEmpty(loadingTo))
                    {
                        string filter1 = string.Empty;
                        string filter2 = " AND LOADING_FROM='CY' AND LOADING_TO='CY'";
                        if (!string.IsNullOrEmpty(loadingFrom))
                            filter1 += " AND LOADING_FROM=" + SQLUtils.QuotedStr(loadingFrom);
                        if (!string.IsNullOrEmpty(loadingTo))
                            filter1 += " AND LOADING_TO=" + SQLUtils.QuotedStr(loadingTo);
                        if (!string.IsNullOrEmpty(filter1))
                        {
                            drs = qtDt.Select(filter + filter1 + termFilter, "QT_EFFECT_FROM DESC");
                            if (drs.Length <= 0 && !filter2.Equals(filter1))
                                drs = qtDt.Select(filter + filter2 + termFilter, "QT_EFFECT_FROM DESC");
                        }
                        else
                        {
                            drs = qtDt.Select(filter + filter2 + termFilter, "QT_EFFECT_FROM DESC");
                        }
                        if (drs.Length <= 0)
                            drs = qtDt.Select(filter + termFilter, "QT_EFFECT_FROM DESC");
                    }
                    else
                        drs = qtDt.Select(filter + termFilter);
                    break;
                default:
                    drs = qtDt.Select(filter + termFilter);
                    break;
            }

            if (drs.Length <= 0 && !string.IsNullOrEmpty(termFilter) && !byterm)
                drs = qtDt.Select(filter);

            foreach (DataRow dr in drs)
            {
                pt_qtDt.ImportRow(dr);
            }
            return pt_qtDt;
        }

        /// <summary>
        /// 根据party type获取报价
        /// </summary>
        /// <param name="shipment_id"></param>
        /// <param name="partyDt"></param>
        /// <param name="types"></param>
        /// <param name="qtDt"></param>
        /// <param name="tranMode"></param>
        /// <param name="term"></param>
        /// <param name="loadingFrom"></param>
        /// <param name="loadingTo"></param>
        /// <returns></returns>
        public static DataTable CloneQTTableByType(string shipment_id, DataTable partyDt, string[] types, DataTable qtDt, string tranMode = "", string term = "", string loadingFrom = "", string loadingTo = "",bool byTerm=false)
        {
            DataTable pt_qtDt = null;
            foreach (string type in types)
            {
                string[] partyNos = GetPartyNo(shipment_id, partyDt, new string[] { type });
                if (string.IsNullOrEmpty(partyNos[0]))
                    continue;
                pt_qtDt = Helper.CloneQTTable(qtDt, partyNos[0], tranMode, term, byTerm, loadingFrom, loadingTo);//运费
                if (pt_qtDt.Rows.Count <= 0 && !string.IsNullOrEmpty(partyNos[2]) && !partyNos[2].Equals(partyNos[0]))
                {
                    pt_qtDt = Helper.CloneQTTable(qtDt, partyNos[2], tranMode, term, byTerm, loadingFrom, loadingTo);//运费
                }
                if (pt_qtDt.Rows.Count > 0)
                    break;
            }

            if (pt_qtDt == null) pt_qtDt = qtDt.Clone();
            return pt_qtDt;
        }

        public static DataTable CloneQTTableByPartyNos(string shipment_id, DataTable partyDt, string[] partyNos, DataTable qtDt, string tranMode = "", string term = "", string loadingFrom = "", string loadingTo = "", bool byTerm = false)
        {
            DataTable pt_qtDt = null;
            foreach (string partyNo in partyNos)
            {
                if (string.IsNullOrEmpty(partyNo))
                    continue;
                pt_qtDt = Helper.CloneQTTable(qtDt, partyNo, tranMode, term, byTerm, loadingFrom, loadingTo);//运费
                if (pt_qtDt.Rows.Count > 0)
                    break;
            }

            if (pt_qtDt == null) pt_qtDt = qtDt.Clone();
            return pt_qtDt;
        }

        public static DataTable CloneQTTableByTypeOnlyowner(string shipment_id, DataTable partyDt, string[] types, DataTable qtDt, string tranMode = "", string term = "", string loadingFrom = "", string loadingTo = "", bool byTerm = false)
        {
            DataTable pt_qtDt = null;
            foreach (string type in types)
            {
                string[] partyNos = GetPartyNo(shipment_id, partyDt, new string[] { type });
                if (string.IsNullOrEmpty(partyNos[0]))
                    continue;
                pt_qtDt = Helper.CloneQTTable1(qtDt, partyNos[0], tranMode, term, byTerm, loadingFrom, loadingTo);//运费
                if (pt_qtDt.Rows.Count > 0)
                    break;
            }

            if (pt_qtDt == null) pt_qtDt = qtDt.Clone();
            return pt_qtDt;
        }

        public static string[] GetPartyNo(string shipment_id, DataTable partyDt, string[] types)
        {
            DataRow[] drs = GetParty(shipment_id, partyDt, types);
            string[] partyNos = new string[] { "", "","","" };
            if (drs.Length > 0)
            {
                partyNos[0] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                partyNos[1] = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                partyNos[2] = Prolink.Math.GetValueAsString(drs[0]["HEAD_OFFICE"]);
                partyNos[3] = Prolink.Math.GetValueAsString(drs[0]["HEAD_NAME"]);
            }
            return partyNos;
        }

        public static DataRow[] GetParty(string shipment_id, DataTable partyDt, string[] pts1)
        {
            DataRow[] drs = null;
            foreach (string pt1 in pts1)
            {
                drs = partyDt.Select(string.Format("PARTY_TYPE={0} AND SHIPMENT_ID={1}", SQLUtils.QuotedStr(pt1), SQLUtils.QuotedStr(shipment_id)));
                if (drs.Length > 0)
                    break;
            }
            if (drs == null) drs = new DataRow[0];
            return drs;
        }

        /// <summary>
        /// 获取数据明细list
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<string> GetValueList(DataTable dt, string name = "U_ID")
        {
            string temp = string.Empty;
            List<string> valueList = new List<string>();
            foreach (DataRow qt in dt.Rows)
            {
                temp = Prolink.Math.GetValueAsString(qt[name]);
                if (!valueList.Contains(temp))
                    valueList.Add(temp);
            }
            return valueList;
        }

        /// <summary>
        /// 获取可查询的港口
        /// </summary>
        /// <param name="pols"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static string[] GetPols(string[] pols, string cmp)
        {
            DataTable polCtity = OperationUtils.GetDataTable(string.Format("SELECT POL,POL_DESCP FROM BSLCPOL WHERE CMP={0}", SQLUtils.QuotedStr(cmp)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (polCtity != null && polCtity.Rows.Count > 0)
            {
                List<string> list = new List<string>();
                List<string> list1 = new List<string>();
                foreach (DataRow city in polCtity.Rows)
                {
                    string name = Prolink.Math.GetValueAsString(city["POL"]);
                    if (string.IsNullOrEmpty(name))
                        continue;
                    if (!list.Contains(name))
                        list.Add(name);
                }
                if (pols.Length <= 0)
                {
                    pols = list.ToArray();
                }
                else
                {
                    foreach (string name in pols)
                    {
                        if (string.IsNullOrEmpty(name))
                            continue;
                        if (list.Contains(name))
                            list1.Add(name);
                    }
                    pols = list1.ToArray();
                }
            }
            return pols;
        }

        /// <summary>
        /// 获取金额的四舍五入的值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal Get45AmtValue(decimal val, int decimals=2)
        {
            return System.Math.Round(val, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 获取本币名称
        /// </summary>
        /// <param name="cur"></param>
        /// <returns></returns>
        public static string GetLoalCurName(string cur)
        {
            if ("RMB".Equals(cur) || "CNY".Equals(cur))
                return "CNY";
            return cur;
        }

        public static decimal GetRateAmt(DataTable rateDt, decimal amt, string fcur, string tcur, ref bool error, List<string> msg = null)
        {
            decimal result = 0M;
            Helper.GetTotal(rateDt, msg, amt, fcur, ref result, ref error, tcur);
            return result;
        }

        /// <summary>
        /// 获取本币金额，返回汇率
        /// </summary>
        /// <param name="rateDt"></param>
        /// <param name="msg"></param>
        /// <param name="val"></param>
        /// <param name="cur"></param>
        /// <param name="total"></param>
        /// <param name="error"></param>
        public static decimal GetTotal(DataTable rateDt, List<string> msg, decimal val, string cur, ref decimal total, ref bool error, string to_cur = "")
        {
            decimal rate = 0M;
            if (string.IsNullOrEmpty(to_cur))
                to_cur = "CNY";

            if ((("RMB".Equals(cur) || "CNY".Equals(cur)) && "CNY".Equals(to_cur)) 
                || to_cur.Equals(cur))
            {
                rate = 1;
                total += val;
                return rate;
            }
            int type = 1;
            string msgStr = string.Empty;
            DataRow[] drs = null;
            if ("CNY".Equals(to_cur))
                drs = rateDt.Select(string.Format("FCUR={0} AND (TCUR='RMB' OR TCUR='CNY')", SQLUtils.QuotedStr(cur)), "EDATE DESC");
            else
                drs = rateDt.Select(string.Format("FCUR={0} AND TCUR={1}", SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(to_cur)), "EDATE DESC");

            if (drs.Length <= 0)
            {
                type = 2;
                if ("CNY".Equals(to_cur))
                    drs = rateDt.Select(string.Format("TCUR={0} AND (FCUR='RMB' OR FCUR='CNY')", SQLUtils.QuotedStr(cur)), "EDATE DESC");
                else
                    drs = rateDt.Select(string.Format("TCUR={0} AND FCUR={1}", SQLUtils.QuotedStr(cur), SQLUtils.QuotedStr(to_cur)), "EDATE DESC");
            }

            if (drs.Length <= 0)
            {
                error = true;
                msgStr = string.Format("无{0}对应{1}的费率", cur, to_cur);
                if (msg!=null&&!msg.Contains(msgStr))
                    msg.Add(msgStr);
            }
            else
            {
                rate = Prolink.Math.GetValueAsDecimal(drs[0]["EX_RATE"]);
                msgStr = string.Format("{0}对应{2}的费率{1}", cur, Get45AmtValue(rate), to_cur);
                if (msg != null && !msg.Contains(msgStr))
                    msg.Add(msgStr);
                if (type != 1)
                    rate = 1M / rate;
                total += rate * val;
            }
            if (rate == 0)
            {
                rate = 1M;
                total += rate * val;
            }
            return rate;
        }

        /// <summary>
        /// 获取小数数据
        /// </summary>
        /// <param name="val1"></param>
        /// <returns></returns>
        public static decimal GetDecimalValue(object val1)
        {
            string val = Prolink.Math.GetValueAsString(val1);
            if (string.IsNullOrEmpty(val))
                return 0M;
            decimal dvalue = 0m;
            if (decimal.TryParse(val, out dvalue))
                return dvalue;
            return 0M;
        }

        /// <summary>
        /// 合并查询条件
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static string JoinString(string[] vals)
        {
            string result = string.Empty;
            foreach (string val in vals)
            {
                if (result.Length > 0)
                    result += ",";
                result += SQLUtils.QuotedStr(val);
            }
            return "(" + result + ")";
        }

        /// <summary>
        /// 获取url decode数据
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string GetUrlDecodeValue(string val)
        {
            if (!string.IsNullOrEmpty(val))
                return HttpUtility.UrlDecode(val);
            else
                return string.Empty;
        }

        /// <summary>
        /// 拆分数据为list
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static List<string> SplitToList(string val)
        {
            string[] vals = val.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            list.AddRange(vals);
            return list;
        }

        public static Dictionary<string, decimal> GetEcreffee(string type, string VenderCd, Dictionary<string, decimal> baseMap)
        {
            Dictionary<string, decimal> my_map = new Dictionary<string, decimal>();

            string sql = string.Format("SELECT CHG_CD,CHG_DESCP,GW,CBM,REMARK,FEE_WEIGHT,FEE_OP,CAL_TYPE FROM ECREFFEEO WHERE VENDER_CD={0} AND TRAN_TYPE={1} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(VenderCd), SQLUtils.QuotedStr(type));
            DataTable dtAll = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection()); ;
            if (dtAll.Rows.Count <= 0)
                return baseMap;

            foreach (DataRow dr in dtAll.Rows)
            {
                decimal val = Prolink.Math.GetValueAsDecimal(dr["FEE_WEIGHT"]);
                string fee_op = Prolink.Math.GetValueAsString(dr["FEE_OP"]);
                string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                #region 获取费用代码
                switch (fee_op)
                {
                    case "1":
                    case "2":
                        val = val * -1;
                        break;
                }
                #endregion
                my_map[chg_cd] = val;
            }

            #region 排序
            List<KeyValuePair<string, decimal>> myList = new List<KeyValuePair<string, decimal>>(my_map);
            myList.Sort(delegate (KeyValuePair<string, decimal> s1, KeyValuePair<string, decimal> s2)
            {
                int n1 = (int)(s1.Value * 10);
                int n2 = (int)(s2.Value * 10);
                if (n2 < 0 && n1 > 0)
                    return n1 - n2;
                else if (n2 > 0 && n1 < 0)
                    return n1 - n2;
                return n2.CompareTo(n1);
            });
            my_map.Clear();
            foreach (KeyValuePair<string, decimal> pair in myList)
            {
                my_map.Add(pair.Key, pair.Value);
            }
            #endregion

            return my_map;
        }

        public static decimal GetFsc(string lspCd, string date)
        {
            string sql = string.Format(@"SELECT FSC FROM SMBFA WHERE IO='I' AND LSP_CD={0} AND EFFECT_DATE<={1} AND EXPIRAT_DATE>={1} ORDER BY EFFECT_DATE DESC",
                SQLUtils.QuotedStr(lspCd), SQLUtils.QuotedStr(date));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return 0;
            return Prolink.Math.GetValueAsDecimal(dt.Rows[0]["FSC"]) / 100;
        }
         
        #endregion
    }
}
