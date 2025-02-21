
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace TrackingEDI.InboundBusiness
{
    public class ASNManager
    {
        public static string GetAsnConditions(string asnNo, string modelNo)
        {
            return string.Format("ASN_NO = {0} AND (PART_NO ={1} OR IPART_NO = {1})",
                SQLUtils.QuotedStr(asnNo), SQLUtils.QuotedStr(modelNo));
        }
        public static void SetDateTimeToEiGR(EditInstruct ei, DateTime datetime)
        {
            if (datetime.CompareTo(new DateTime(2000, 1, 1)) > 0)
            {
                ei.Put("GR_DATE", datetime.ToString("yyyy-MM-dd"));
            }
        }

        public static bool SetGRToSMINDNP(string asnNo, string modelNo, int grQty, DateTime grDate, MixedList ml)
        { 
            string sql = "SELECT U_ID,ASN_NO,PART_NO,QTY FROM SMIDNP WHERE (NEW_CATEGORY != 'TANN' OR NEW_CATEGORY IS NULL) AND " + TrackingEDI.InboundBusiness.ASNManager.GetAsnConditions(asnNo, modelNo);
            DataTable inpDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in inpDt.Rows)
            {
                int modelqty = Prolink.Math.GetValueAsInt(dr["QTY"]);
                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                EditInstruct smidnpEi = new EditInstruct("SMIDNP", EditInstruct.UPDATE_OPERATION);
                smidnpEi.PutKey("U_ID", uid);
                smidnpEi.Put("ASN_NO", asnNo);
                smidnpEi.Put("GR_STATUS", grQty >= modelqty ? "Y" : "P");
                smidnpEi.Put("GR_QTY", grQty >= modelqty ? modelqty : grQty);
                grQty = grQty - modelqty;
                if (grQty <= 0)
                    grQty = 0;
                SetDateTimeToEiGR(smidnpEi, grDate);
                ml.Add(smidnpEi);
            }
            return inpDt.Rows.Count > 0;
        }

        public static void SetAsnMapToSMIDNP(string shipmentid, MixedList ml = null)
        {
            bool executeNow = false;
            if (ml == null)
            {
                executeNow = true;
                ml = new MixedList();
            }
            DataTable inpDt = OperationUtils.GetDataTable(string.Format(@"SELECT U_ID,ASN_DATE,
QTY,ASN_NO,PART_NO,IPART_NO,CATEGORY,ISNULL(PART_NO,IPART_NO) AS SPART_NO,INV_NO,SHIPMENT_ID,
(SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS ETA,
(SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS TRAN_TYPE,
(SELECT TOP 1 CMP FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNP.SHIPMENT_ID) AS CMP FROM SMIDNP WHERE (NEW_CATEGORY != 'TANN' OR NEW_CATEGORY IS NULL) AND SHIPMENT_ID ={0} ORDER BY ASN_NO DESC",
                SQLUtils.QuotedStr(shipmentid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable smDt = GetSmsmi(shipmentid);
            Dictionary<string, int> grQtyDic = new Dictionary<string, int>();
            Dictionary<string, DataTable> asnMapDic = new Dictionary<string, DataTable>();
            
            foreach (DataRow dr in inpDt.Rows)
            {
                string partno = Prolink.Math.GetValueAsString(dr["SPART_NO"]);
                string invno = Prolink.Math.GetValueAsString(dr["INV_NO"]);
                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                int modelQty = Prolink.Math.GetValueAsInt(dr["QTY"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]); 
                if (!asnMapDic.ContainsKey(invno))
                    asnMapDic.Add(invno, GetAsnDtByInv(invno));
                DataTable asnMap = asnMapDic[invno];
                if (asnMap.Rows.Count <= 0)
                    continue;
                EditInstruct smidnpEi = new EditInstruct("SMIDNP", EditInstruct.UPDATE_OPERATION);
                smidnpEi.PutKey("U_ID", uid);
                string asnNo = Prolink.Math.GetValueAsString(asnMap.Rows[0]["ASN_NO"]);
                DateTime asnDate = Prolink.Math.GetValueAsDateTime(asnMap.Rows[0]["ASN_DATE"]);
                DateTime directDate = Prolink.Math.GetValueAsDateTime(asnMap.Rows[0]["DIRECT_DATE"]);
                Datetype datetype = TrackingEDI.InboundBusiness.ASNManager.Getasndate(shipmentid, invno, dr, ref asnDate);
                smidnpEi.PutDate("ASN_DATE", asnDate);
                smidnpEi.Put("ASN_NO", asnNo);
                ml.Add(smidnpEi);
                UpdateAsnToSmidn(datetype, ml, asnDate, dr);

                DataRow[] grDrs = asnMap.Select("MODEL_NO=" + SQLUtils.QuotedStr(partno), "A_MODIFY DESC,A_CREATE DESC,G_MODIFY DESC,G_CREATE DESC");
                int grQty = modelQty;
                DateTime grDate = directDate;
                if (grDrs.Length > 0 && directDate <= DateTime.MinValue)
                {
                    grDate = Prolink.Math.GetValueAsDateTime(grDrs[0]["GR_DATE"]);
                    int qty = Prolink.Math.GetValueAsInt(grDrs[0]["GR_QTY"]);
                    string keyValue = invno + ";_" + partno;
                    if (!grQtyDic.ContainsKey(keyValue))
                        grQtyDic.Add(keyValue, qty);
                    grQtyDic[keyValue] -= modelQty;
                    if (grQtyDic[keyValue] <= 0)
                    {
                        grQty = grQtyDic[keyValue] + modelQty;
                        grQtyDic[keyValue] = 0;
                    }
                }

                string grStatus = grQty == modelQty ? "Y" : "P";
                if (grDate <= DateTime.MinValue)
                    continue;
                smidnpEi.Put("GR_QTY", grQty);
                smidnpEi.Put("GR_STATUS", grStatus);
                smidnpEi.Put("GR_DATE", grDate.ToString("yyyy-MM-dd"));

            }
            if (executeNow)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static DataTable GetAsnDtByInv(string invNo)
        {
            string sql = string.Format(@"SELECT A.ASN_NO,A.ASN_DATE,A.GR_DATE AS DIRECT_DATE,G.GR_QTY,G.MODEL_NO,G.GR_DATE,A.CREATE_DATE AS A_CREATE,G.CREATE_DATE AS G_CREATE,
A.MODIFY_DATE AS A_MODIFY,G.MODIFY_DATE AS G_MODIFY FROM ASN_MAP A LEFT JOIN GR_MAP G ON A.ASN_NO=G.ASN_NO WHERE A.CNTR_NO={0} ORDER BY A.MODIFY_DATE DESC,A.CREATE_DATE DESC",
SQLUtils.QuotedStr(invNo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }


        public static void SetAsnByAsnNo(string asnno)
        {
            string sql = string.Format("SELECT TOP 1 SHIPMENT_ID FROM SMIDNP WHERE ASN_NO = {0}",
                SQLUtils.QuotedStr(asnno));
            string shipmentid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetAsnByShipmentid(shipmentid);
        }

        public static void SetAsnByShipmentid(string shipmentid)
        {
            string sql = string.Format("SELECT SHIPMENT_ID,TRAN_TYPE,SHIPMENT_INFO,INVOICE_INFO FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smsmiDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetAsn(smsmiDt);
        }


        public static void SetAsn(DataTable smidt)
        {
            string sql = string.Empty;
            foreach (DataRow dr in smidt.Rows)
            {
                MixedList ml = new MixedList();
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string shipmentInfo = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                string OldInvoiceInfo = Prolink.Math.GetValueAsString(dr["INVOICE_INFO"]);
                List<string> OldInvoiceInfolist = OldInvoiceInfo.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                DataTable inpDt = OperationUtils.GetDataTable(string.Format(@"SELECT ASN_NO,GR_DATE,PART_NO,IPART_NO,GR_QTY,QTY,INV_NO,
ASN_DATE,CNTR_NO FROM SMIDNP WHERE SHIPMENT_ID={0} AND (NEW_CATEGORY!='TANN' OR NEW_CATEGORY IS NULL) ORDER BY DN_NO", SQLUtils.QuotedStr(shipmentId)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string grStatus = "N";
                List<string> invList = new List<string>();
                List<string> asnList = new List<string>();
                List<string> grInfo = new List<string>();
                List<string> grQtyInfo = new List<string>();
                List<string> partInfo = new List<string>();
                List<string> qtyInfo = new List<string>();
                List<string> asnDateInfo = new List<string>();
                Action<List<string>, string> onAdd = (items, txt) =>
                {
                    if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                    items.Add(txt);
                };
                bool isY = true;
                DateTime asnDate = DateTime.MinValue;
                foreach (DataRow inpDr in inpDt.Rows)
                {
                    string asnno = Prolink.Math.GetValueAsString(inpDr["ASN_NO"]);
                    string grDate = Prolink.Math.GetValueAsString(inpDr["GR_DATE"]);
                    string partNo = Prolink.Math.GetValueAsString(inpDr["PART_NO"]);
                    string invNo = Prolink.Math.GetValueAsString(inpDr["INV_NO"]);
                    if (string.IsNullOrEmpty(partNo))
                        partNo = Prolink.Math.GetValueAsString(inpDr["IPART_NO"]);
                    int grQty = Prolink.Math.GetValueAsInt(inpDr["GR_QTY"]);
                    int pQty = Prolink.Math.GetValueAsInt(inpDr["QTY"]);
                    if (grQty != pQty || string.IsNullOrEmpty(grDate))
                        isY = false;
                    asnDate = Prolink.Math.GetValueAsDateTime(inpDr["ASN_DATE"]);
                    if (asnDate <= DateTime.MinValue)
                    {
                        string asndatestr = OperationUtils.GetValueAsString(string.Format("SELECT ASN_DATE FROM ASN_MAP WHERE ASN_NO = {0}",
                        SQLUtils.QuotedStr(asnno)), Prolink.Web.WebContext.GetInstance().GetConnection());
                        asnDate = Prolink.Math.GetValueAsDateTime(asndatestr);
                    }

                    if (!string.IsNullOrEmpty(invNo) && !invList.Contains(invNo))
                    {
                        invList.Add(invNo);
                        asnList.Add(asnno);
                        asnDateInfo.Add(asnDate > DateTime.MinValue ? asnDate.ToString("yyyy-MM-dd") : "");
                    }

                    if (!string.IsNullOrEmpty(partNo))
                    {
                        partInfo.Add(partNo);
                        qtyInfo.Add(pQty.ToString());
                        grInfo.Add(grDate);
                        grQtyInfo.Add(grQty.ToString());
                    }
                    if (!string.IsNullOrEmpty(grDate) || grQty > 0)
                        grStatus = "P";
                }

                DataRow[] Prows = inpDt.Select("ASN_NO IS NULL OR ASN_NO=''");
                DataRow[] Yrows = inpDt.Select("ASN_NO IS NOT NULL AND ASN_NO <>''");

                string asnnoinfo = string.Join(",", asnList);
                string partnoinfo = string.Join(",", partInfo);
                string qtynoinfo = string.Join(",", qtyInfo);
                EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                smsmiei.PutKey("SHIPMENT_ID", shipmentId);
                smsmiei.Put("ASNNO_INFO", asnnoinfo);
                if (Prows.Length <= 0 && Yrows.Length > 0)
                {
                    smsmiei.Put("ASN_STATUS", "Y");
                }
                else if (Prows.Length > 0 && Yrows.Length > 0)
                {
                    smsmiei.Put("ASN_STATUS", "P");
                }
                else
                {
                    smsmiei.Put("ASN_STATUS", null);
                }
                foreach (string inv in OldInvoiceInfolist)
                {
                    int currentIndex = OldInvoiceInfolist.IndexOf(inv);
                    if (invList.Contains(inv))
                    {
                        invList.Remove(inv);
                        invList.Insert(currentIndex, inv);
                    }
                }
                //smsmiei.PutDate("ASN_DATE", asnDate);
                smsmiei.Put("INVOICE_INFO", string.Join(",", invList));
                smsmiei.Put("ASN_DATE", string.Join(",", asnDateInfo));
                smsmiei.Put("GR_DATE", string.Join(",", grInfo));
                smsmiei.Put("GR_STATUS", isY ? "Y" : grStatus);
                smsmiei.Put("GR_QTY", string.Join(",", grQtyInfo));
                smsmiei.Put("PARTNO_INFO", partnoinfo);
                smsmiei.Put("PART_QTY", qtynoinfo);
                ml.Add(smsmiei);

                string ordSql = string.Format("UPDATE SMORD SET ASNNO_INFO={0},PARTNO_INFO={2},PART_QTY={3} WHERE SHIPMENT_ID={1}", SQLUtils.QuotedStr(string.Join(",", asnList)),
                    SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(string.Join(",", partInfo)), SQLUtils.QuotedStr(string.Join(",", qtyInfo)));
                string irvSql = string.Format("UPDATE SMIRV SET ASNNO_INFO={0},PARTNO_INFO={2},PART_QTY={3} WHERE SHIPMENT_INFO={1}", SQLUtils.QuotedStr(string.Join(",", asnList)),
                    SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(string.Join(",", partInfo)), SQLUtils.QuotedStr(string.Join(",", qtyInfo)));
                if (tranType.Equals("F") || tranType.Equals("R"))
                {
                    sql = string.Format("SELECT * FROM SMORD WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
                    DataTable ordDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow ordDr in ordDt.Rows)
                    {
                        string cntrNo = Prolink.Math.GetValueAsString(ordDr["CNTR_NO"]);
                        string ordNo = Prolink.Math.GetValueAsString(ordDr["ORD_NO"]);
                        DataRow[] inpRows = inpDt.Select(string.Format("CNTR_NO = {0}", SQLUtils.QuotedStr(cntrNo)));
                        asnList = new List<string>();
                        partInfo = new List<string>();
                        qtyInfo = new List<string>();
                        foreach (DataRow inpDr in inpRows)
                        {
                            string masnNo = Prolink.Math.GetValueAsString(inpDr["ASN_NO"]);
                            string partNo = Prolink.Math.GetValueAsString(inpDr["PART_NO"]);
                            if (string.IsNullOrEmpty(partNo))
                                partNo = Prolink.Math.GetValueAsString(inpDr["IPART_NO"]);
                            int pQty = Prolink.Math.GetValueAsInt(inpDr["QTY"]);
                            if (!string.IsNullOrEmpty(partNo))
                            {
                                partInfo.Add(partNo);
                                asnList.Add(masnNo);
                                qtyInfo.Add(pQty.ToString());
                            }
                        }
                        if (partInfo.Count > 0)
                        {
                            ordSql = string.Format("UPDATE SMORD SET ASNNO_INFO={0},PARTNO_INFO={2},PART_QTY={3} WHERE ORD_NO={1}", SQLUtils.QuotedStr(string.Join(",", asnList)),
                                SQLUtils.QuotedStr(ordNo), SQLUtils.QuotedStr(string.Join(",", partInfo)), SQLUtils.QuotedStr(string.Join(",", qtyInfo)));
                            string reserveNo = OperationUtils.GetValueAsString(string.Format("SELECT RESERVE_NO FROM SMRCNTR WHERE ORD_NO={0}", SQLUtils.QuotedStr(ordNo)),
                                Prolink.Web.WebContext.GetInstance().GetConnection());
                            irvSql = string.Format("UPDATE SMIRV SET ASNNO_INFO={0},PARTNO_INFO={2},PART_QTY={3} WHERE RESERVE_NO={1}", SQLUtils.QuotedStr(string.Join(",", asnList)),
                                SQLUtils.QuotedStr(reserveNo), SQLUtils.QuotedStr(string.Join(",", partInfo)), SQLUtils.QuotedStr(string.Join(",", qtyInfo)));
                            ml.Add(ordSql);
                            ml.Add(irvSql);
                        }
                    }
                }
                else
                {
                    string[] shipments = shipmentInfo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (shipments.Length > 1)
                    {
                        sql = string.Format("SELECT ASN_NO,PART_NO,QTY FROM SMIDNP WHERE SHIPMENT_ID IN {0} AND (NEW_CATEGORY!='TANN' OR NEW_CATEGORY IS NULL)", SQLUtils.Quoted(shipments));
                        inpDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        asnList = new List<string>();
                        partInfo = new List<string>();
                        qtyInfo = new List<string>();
                        foreach (DataRow inpDr in inpDt.Rows)
                        {
                            string masnNo = Prolink.Math.GetValueAsString(inpDr["ASN_NO"]);
                            string partNo = Prolink.Math.GetValueAsString(inpDr["PART_NO"]);
                            int pQty = Prolink.Math.GetValueAsInt(inpDr["QTY"]);
                            if (!string.IsNullOrEmpty(partNo))
                            {
                                partInfo.Add(partNo);
                                asnList.Add(masnNo);
                                qtyInfo.Add(pQty.ToString());
                            }
                        }
                        irvSql = string.Format("UPDATE SMIRV SET ASNNO_INFO={0},PARTNO_INFO={2},PART_QTY={3} WHERE SHIPMENT_INFO={1}", SQLUtils.QuotedStr(string.Join(",", asnList)),
                            SQLUtils.QuotedStr(shipmentInfo), SQLUtils.QuotedStr(string.Join(",", partInfo)), SQLUtils.QuotedStr(string.Join(",", qtyInfo)));
                    }
                    ml.Add(ordSql);
                    if (!string.IsNullOrEmpty(irvSql))
                        ml.Add(irvSql);
                }

                if (ml.Count > 0)
                {
                    try
                    {
                        OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        public static void UpdateAsnToSmidn(Datetype datetype, MixedList ml, DateTime asndate, DataRow dr)
        {
            string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
            string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            string invno = Prolink.Math.GetValueAsString(dr["INV_NO"]);
            EditInstruct smidnEi = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
            switch (datetype)
            {
                case Datetype.DeliveryDate:
                    smidnEi.PutKey("SHIPMENT_ID", shipmentId);
                    smidnEi.PutKey("INV_NO", invno);
                    smidnEi.PutDate("ASN_DATE", asndate);
                    smidnEi.Put("SEND_ASN_STATUS", "N");
                    ml.Add(smidnEi);
                    break;
                case Datetype.ETA:
                    smidnEi.PutKey("SHIPMENT_ID", shipmentId);
                    smidnEi.PutDate("ASN_DATE", asndate);
                    smidnEi.Put("SEND_ASN_STATUS", "N");
                    ml.Add(smidnEi);
                    break;
                case Datetype.MMNUpdate:
                    smidnEi.PutKey("SHIPMENT_ID", shipmentId);
                    smidnEi.PutDate("ASN_DATE", asndate);
                    smidnEi.Put("SEND_ASN_STATUS", "F");
                    ml.Add(smidnEi);
                    break;
            }
        }

        public static DataTable GetSmsmi(string shipmentId)
        {
            string sql = string.Format("SELECT ETA,CMP,ASN_DATE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable smidt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection()); 
            return smidt;
        }


        public static Datetype Getasndate(string shipmentId, string invno,DataRow smDr, ref DateTime asndate)
        {
            Datetype datetype = Datetype.ASNdate; 
            string cmp = Prolink.Math.GetValueAsString(smDr["CMP"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(smDr["ETA"]); 
            if ("MN".Equals(cmp))
                return Datetype.MMNUpdate;

            string sql = string.Format(@"SELECT TOP 1 DELIVERY_DATE FROM SMORD WHERE SHIPMENT_ID={0} AND DELIVERY_DATE IS NOT NULL
             AND DELIVERY_DATE !='' AND CNTR_NO IN (
            SELECT CNTR_NO FROM SMIDNP WHERE SHIPMENT_ID = {0} AND(NEW_CATEGORY != 'TANN' OR NEW_CATEGORY IS NULL) AND INV_NO={1})"
                        , SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(invno));
            string deliverydatestr = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());


            Func<DateTime, bool> CheckTime = (date) =>
            {
                if (date != null && date > DateTime.MinValue)
                {
                    return true;
                }
                return false;
            };
            Func<DateTime, int, DateTime> AddWorkDay = (date, day) =>
            {
                int i = 0;
                while (day > 0)
                {
                    date = date.AddDays(1);
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) day -= 1;
                    i++;
                    if (i > 10000) break;
                }
                return date;
            };
            Func<DateTime, DateTime, int, bool> CheckWorkDate = (oldDate, newDate, workday) =>
            {
                DateTime date1 = AddWorkDay(newDate, workday);
                DateTime date2 = AddWorkDay(oldDate, workday);

                if (oldDate.CompareTo(date1) >= 0 || date2.CompareTo(newDate) <= 0)
                    return true;
                else
                    return false;
            };
            DateTime DeliveryDate = Prolink.Math.GetValueAsDateTime(deliverydatestr);
            if (CheckTime(DeliveryDate))
            {
                asndate = DeliveryDate;
                datetype = Datetype.DeliveryDate;
            }
            
            if (datetype == Datetype.ASNdate)
            {
                if (CheckTime(eta))
                {
                    asndate = eta.AddDays(6);
                    if ("AVAIB".Equals(cmp))
                    {
                        string tranType = Prolink.Math.GetValueAsString(smDr["TRAN_TYPE"]);
                        if (tranType == "R") asndate = eta.AddDays(3);
                        if (tranType == "A" || tranType == "E") asndate = eta.AddDays(1);
                    }                 
                    datetype = Datetype.ETA;
                }
            }
            return datetype;
        }

        public enum Datetype
        {
            DeliveryDate,
            ETA,
            ASNdate,
            NotUpdate,
            MMNUpdate
        }
    }
}
