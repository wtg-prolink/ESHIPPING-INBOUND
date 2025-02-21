using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace TrackingEDI.InboundBusiness
{
    public class InboundTransfer : BaseParser
    {
        static string SMordEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {

            string TranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string pol1 = Prolink.Math.GetValueAsString(dr["POL1"]);
            string pol_nm1 = Prolink.Math.GetValueAsString(dr["POL_NM1"]);
            string pol2 = Prolink.Math.GetValueAsString(dr["POD1"]);
            string pol_nm2 = Prolink.Math.GetValueAsString(dr["POD_NM1"]);
            string tran_type1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE1"]);
            string dep_addr1 = Prolink.Math.GetValueAsString(dr["DEP_ADDR1"]);
            string trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
            string trucker_nm1 = Prolink.Math.GetValueAsString(dr["TRUCKER_NM1"]);
            string productType = Prolink.Math.GetValueAsString(dr["PRODUCT_INFO"]);
            string ordno = Prolink.Math.GetValueAsString(parm["ORD_NO"]);
            string trantimes = Prolink.Math.GetValueAsString(parm["TRAN_TIMES"]);
            string asnInfo = Prolink.Math.GetValueAsString(parm["ASNNO_INFO"]);
            string partInfo = Prolink.Math.GetValueAsString(parm["PARTNO_INFO"]);
            string csCd = Prolink.Math.GetValueAsString(parm["CS_CD"]);
            string csName = Prolink.Math.GetValueAsString(parm["CS_NAME"]);
            string csNm = Prolink.Math.GetValueAsString(parm["CS_NM"]);
            string bu = Prolink.Math.GetValueAsString(parm["BU"]);
            string partQty = Prolink.Math.GetValueAsString(parm["PART_QTY"]);
            string tranno = "1";
            string invoiceInfo = string.Empty;
            if (TranType != "F" && TranType != "R")
            {
                switch (trantimes)
                {
                    case "two":
                        pol1 = Prolink.Math.GetValueAsString(dr["POL2"]);
                        pol_nm1 = Prolink.Math.GetValueAsString(dr["POL_NM2"]);
                        pol2 = Prolink.Math.GetValueAsString(dr["POL3"]);
                        pol_nm2 = Prolink.Math.GetValueAsString(dr["POL_NM3"]);
                        tran_type1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE2"]);
                        dep_addr1 = Prolink.Math.GetValueAsString(dr["DEP_ADDR2"]);
                        trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER2"]);
                        trucker_nm1 = Prolink.Math.GetValueAsString(dr["TRUCKER_NM2"]);
                        tranno = "2";
                        break;
                    case "three":
                        pol1 = Prolink.Math.GetValueAsString(dr["POL3"]);
                        pol_nm1 = Prolink.Math.GetValueAsString(dr["POL_NM3"]);
                        pol2 = null;
                        pol_nm2 = null;
                        tran_type1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE3"]);
                        dep_addr1 = Prolink.Math.GetValueAsString(dr["DEP_ADDR3"]);
                        trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER3"]);
                        trucker_nm1 = Prolink.Math.GetValueAsString(dr["TRUCKER3"]);
                        tranno = "3";
                        break;
                }
                invoiceInfo = Prolink.Math.GetValueAsString(dr["INVOICE_INFO"]);
                if (TranType == "L") 
                    ei.Put("CNTR_NO", Prolink.Math.GetValueAsString(parm["LCLCntrNo"])); 
            }
            else
            {
                DataRow smicntrdr = (DataRow)parm["SMICNTR_ROW"];
                pol1 = Prolink.Math.GetValueAsString(smicntrdr["POL1"]);
                pol_nm1 = Prolink.Math.GetValueAsString(smicntrdr["POL_NM1"]);
                pol2 = Prolink.Math.GetValueAsString(smicntrdr["POD1"]);
                pol_nm2 = Prolink.Math.GetValueAsString(smicntrdr["POD_NM1"]);
                tran_type1 = Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE1"]);
                dep_addr1 = Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR1"]);
                trucker1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER1"]);
                trucker_nm1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER_NM1"]);
                string CntrNo = smicntrdr["CNTR_NO"].ToString();
                string CntrType = smicntrdr["CNTR_TYPE"].ToString();
                string containerdn = Prolink.Math.GetValueAsString(smicntrdr["DN_NO"]);
                DateTime stoDueDate = Prolink.Math.GetValueAsDateTime(smicntrdr["STORAGE_DUE_DATE"]);
                DateTime demDueDate = Prolink.Math.GetValueAsDateTime(smicntrdr["DEMURRAGE_DUE_DATE"]);
                DateTime detDueDate = Prolink.Math.GetValueAsDateTime(smicntrdr["DETENTION_DUE_DATE"]);
                productType = Prolink.Math.GetValueAsString(smicntrdr["DIVISION_DESCP"]);
                switch (trantimes)
                {
                    case "two":
                        pol1 = Prolink.Math.GetValueAsString(smicntrdr["POL2"]);
                        pol_nm1 = Prolink.Math.GetValueAsString(smicntrdr["POL_NM2"]);
                        pol2 = Prolink.Math.GetValueAsString(smicntrdr["POL3"]);
                        pol_nm2 = Prolink.Math.GetValueAsString(smicntrdr["POL_NM3"]);
                        tran_type1 = Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE2"]);
                        dep_addr1 = Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR2"]);
                        trucker1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER2"]);
                        trucker_nm1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER_NM2"]);
                        tranno = "2";
                        break;
                    case "three":
                        pol1 = Prolink.Math.GetValueAsString(smicntrdr["POL3"]);
                        pol_nm1 = Prolink.Math.GetValueAsString(smicntrdr["POL_NM3"]);
                        pol2 = null;
                        pol_nm2 = null;
                        tran_type1 = Prolink.Math.GetValueAsString(smicntrdr["TRAN_TYPE3"]);
                        dep_addr1 = Prolink.Math.GetValueAsString(smicntrdr["DEP_ADDR3"]);
                        trucker1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER3"]);
                        trucker_nm1 = Prolink.Math.GetValueAsString(smicntrdr["TRUCKER3"]);
                        tranno = "3";
                        break;
                }
                ei.Put("CNTR_NO", CntrNo);
                ei.Put("CNTR_TYPE", CntrType);
                ei.Put("DN_NO", containerdn);
                ei.PutDate("STORAGE_DUE_DATE", stoDueDate);
                ei.PutDate("DEMURRAGE_DUE_DATE", demDueDate);
                ei.PutDate("DETENTION_DUE_DATE", detDueDate);
                ei.PutDate("DISCHARGE_DATE", Prolink.Math.GetValueAsDateTime(smicntrdr["DISCHARGE_DATE"]));
                ei.PutDate("EMP_PICK_DATE", Prolink.Math.GetValueAsDateTime(smicntrdr["EMP_PICK_DATE"]));
                ei.PutDate("PICKUP_CDATE", Prolink.Math.GetValueAsDateTime(smicntrdr["PICKUP_CDATE"]));
                ei.PutDate("EMPTY_TIME", Prolink.Math.GetValueAsDateTime(smicntrdr["EMPTY_TIME"]));
                string shipmentId = Prolink.Math.GetValueAsString(smicntrdr["SHIPMENT_ID"]);
                invoiceInfo = Prolink.Math.GetValueAsString(smicntrdr["INV_NO"]);
                if (!string.IsNullOrEmpty(CntrNo))
                    UpdateBillInfoToSMORD(shipmentId, CntrNo, ei);
            }
            ei.Put("PRODUCT_TYPE", productType);
            ei.Put("POL1", pol1);
            ei.Put("POL_NM1", pol_nm1);
            ei.Put("POD1", pol2);
            ei.Put("POD_NM1", pol_nm2);
            ei.Put("TRAN_TYPE1", tran_type1);
            ei.Put("DEP_ADDR", dep_addr1);
            ei.Put("TRUCKER1", trucker1);
            ei.Put("TRUCKER_NM1", trucker_nm1);
            ei.Put("TRAN_NO", tranno);
            ei.Put("ORD_NO", ordno);
            ei.Put("ASNNO_INFO", asnInfo);
            ei.Put("PARTNO_INFO", partInfo);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("CS_CD", csCd);
            ei.Put("PART_QTY", partQty);
            ei.Put("CS_NAME", csName);
            ei.Put("CS_NM", csNm);
            ei.Put("BU", bu);
            ei.Put("INVOICE_INFO", invoiceInfo);
            return string.Empty;
        }

        public string SaveToTruckCalling(DataTable smsmidt,string trantimes, MixedList ml,DataRow smincntrDr = null)
        {
            try
            {
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["ORD_NO"] = this.OrdNo;
                parm["TRAN_TIMES"] = trantimes;
                parm["SMICNTR_ROW"] = smincntrDr;
                parm["ASNNO_INFO"] = this.AsnInfo;
                parm["PARTNO_INFO"] = this.PartInfo;
                parm["PART_QTY"] = this.PartQty;
                parm["CS_CD"] = this.CsCd;
                parm["CS_NAME"] = this.CsName;
                parm["CS_NM"] = this.CsNm;
                parm["BU"] = this.Bu;
                parm["LCLCntrNo"] = this.LCLCntrNo;
                RegisterEditInstructFunc("SMSMIToSMORDMapping", SMordEditInstruct);
                ParseEditInstruct(smsmidt, "SMSMIToSMORDMapping", ml, parm);
            }
            catch (Exception ex)
            {                
                return "Error:Create trackcalling fail! Errorinfo:"+ex.ToString();
            }
            return string.Empty;
        }

        static string SmrdnEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string pol1 = Prolink.Math.GetValueAsString(dr["POL1"]);
            string pol_nm1 = Prolink.Math.GetValueAsString(dr["POL_NM1"]);
            string pol2 = Prolink.Math.GetValueAsString(dr["POD1"]);
            string pol_nm2 = Prolink.Math.GetValueAsString(dr["POD_NM1"]);
            string tran_type1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE1"]);
            string dep_addr1 = Prolink.Math.GetValueAsString(dr["DEP_ADDR"]);
            string trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
            string trucker_nm1 = Prolink.Math.GetValueAsString(dr["TRUCKER_NM1"]);
            string ordno = Prolink.Math.GetValueAsString(parm["ORD_NO"]);

            ei.Put("POL1", pol1);
            ei.Put("POL_NM1", pol_nm1);
            ei.Put("POL2", pol2);
            ei.Put("POL_NM2", pol_nm2);
            ei.Put("TRAN_TYPE1", tran_type1);
            ei.Put("DEP_ADDR1", dep_addr1);
            ei.Put("TRUCKER1", trucker1);
            ei.Put("TRUCKER_NM1", trucker_nm1);
            ei.Put("TRAN_NO", '1');
            ei.Put("ORD_NO", ordno);
            ei.Put("TORDER", 'N');
            ei.PutDate("CREATE_DATE", DateTime.Now);
            return string.Empty;
        }

        public string SaveSmidnToSmrdn(string ordno, string shipmentid, MixedList ml)
        {
            try
            {
                string sql = string.Format("SELECT * FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                DataTable smidnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["ORD_NO"] = ordno;
                RegisterEditInstructFunc("InboundSMRDNMapping", SmrdnEditInstruct);
                ParseEditInstruct(smidnDt, "InboundSMRDNMapping", ml, parm);
            }
            catch (Exception ex)
            {
                return "";
            }
            return string.Empty;
        }


        static string SmrcntrEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string addrCode = Prolink.Math.GetValueAsString(dr["ADDR_CODE"]);
            //parm
            string ordno = Prolink.Math.GetValueAsString(parm["ORD_NO"]);
            string pick_area = Prolink.Math.GetValueAsString(parm["PICK_AREA"]);
            string pick_area_nm = Prolink.Math.GetValueAsString(parm["PICK_AREA_NM"]);

            string wscd = Prolink.Math.GetValueAsString(dr["WS_CD"]);
            if (string.IsNullOrEmpty(wscd))
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string sql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE SMWH.DLV_ADDR={0} AND SMWH.CMP={1}", SQLUtils.QuotedStr(addrCode), SQLUtils.QuotedStr(cmp));
                wscd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            string sql1 = string.Format("SELECT TOP 1 PRODUCTION_DATE FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smsmidt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DateTime productiondate = Prolink.Math.GetValueAsDateTime(smsmidt.Rows[0]["PRODUCTION_DATE"]);
            ei.Put("PICK_AREA", pick_area);
            ei.Put("PICK_AREA_NM", pick_area_nm);
            ei.Put("WS_CD", wscd);
            ei.Put("ORD_NO", ordno);
            ei.PutDate("ARRIVAL_DATE", productiondate);
            return string.Empty;
        }

        public string SaveSmicntrToSmrcntr(string ordno, string shipmentid, string pickarea, string pickname, MixedList ml)
        {
            try
            {
                string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                DataTable smicntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["ORD_NO"] = ordno;
                parm["PICK_AREA"] = pickarea;
                parm["PICK_AREA_NM"] = pickname;
                RegisterEditInstructFunc("SmicntrToSmrcntrMapping", SmrcntrEditInstruct);
                ParseEditInstruct(smicntrDt, "SmicntrToSmrcntrMapping", ml, parm);
            }
            catch (Exception ex)
            {
                return "";
            }
            return string.Empty;
        }

        public string SaveSmicntrToSmrcntrByCntrNo(string ordno, string shipmentid, string pickarea, string pickname, MixedList ml, string cntrno)
        {
            try
            {
                string sql = string.Format("SELECT * FROM SMICNTR WHERE SHIPMENT_ID={0} AND CNTR_NO={1} ", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(cntrno));
                DataTable smicntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> parm = new Dictionary<string, object>();
                parm["ORD_NO"] = ordno;
                parm["PICK_AREA"] = pickarea;
                parm["PICK_AREA_NM"] = pickname;
                RegisterEditInstructFunc("SmicntrToSmrcntrMapping", SmrcntrEditInstruct);
                ParseEditInstruct(smicntrDt, "SmicntrToSmrcntrMapping", ml, parm);
            }
            catch (Exception ex)
            {
                return "";
            }
            return string.Empty;
        }

        public string saveToSmrcntr(DataTable smDt,DataTable smicntrDt, MixedList ml)
        {
            try
            {
                Dictionary<string, object> parm = new Dictionary<string, object>();
                string arrivalDate = Prolink.Math.GetValueAsString(smDt.Rows[0]["PRODUCTION_DATE"]);
                parm["ORD_NO"] = this.OrdNo;
                parm["PICK_AREA"] = this.Pol;
                parm["PICK_AREA_NM"] = this.PolName;
                parm["ARRIVAL_DATE"] = arrivalDate;
                RegisterEditInstructFunc("SmicntrToSmrcntrMapping", RcntrEditInstruct);
                ParseEditInstruct(smicntrDt, "SmicntrToSmrcntrMapping", ml, parm);
            }
            catch (Exception ex)
            {
                return "Error: Save to Smrcntr,Error Info: "+ex.ToString();
            }
            return string.Empty;
        }

        static string RcntrEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string addrCode = Prolink.Math.GetValueAsString(dr["ADDR_CODE"]);
            string ordno = Prolink.Math.GetValueAsString(parm["ORD_NO"]);
            string pick_area = Prolink.Math.GetValueAsString(parm["PICK_AREA"]);
            string pick_area_nm = Prolink.Math.GetValueAsString(parm["PICK_AREA_NM"]);

            string wscd = Prolink.Math.GetValueAsString(dr["WS_CD"]);
            if (string.IsNullOrEmpty(wscd))
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string sql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE SMWH.DLV_ADDR={0} AND SMWH.CMP={1}", SQLUtils.QuotedStr(addrCode), SQLUtils.QuotedStr(cmp));
                wscd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            ei.Put("PICK_AREA", pick_area);
            ei.Put("PICK_AREA_NM", pick_area_nm);
            ei.Put("WS_CD", wscd);
            ei.Put("ORD_NO", ordno);
            ei.PutDate("ARRIVAL_DATE", parm["ARRIVAL_DATE"]);
            return string.Empty;
        }

        public void saveToSmrdn(DataTable smDt,DataTable idnDt, bool isFirst, MixedList ml)
        {
            if (idnDt == null || idnDt.Rows.Count <= 0) return;
            if (smDt.Rows.Count <= 0 || smDt == null) return;
            string tranType1 = Prolink.Math.GetValueAsString(smDt.Rows[0]["TRAN_TYPE1"]);
            string PolNm2 = Prolink.Math.GetValueAsString(smDt.Rows[0]["POL_NM2"]);
            string DepAddr2 = Prolink.Math.GetValueAsString(smDt.Rows[0]["DEP_ADDR2"]);
            string arrivalDate = Prolink.Math.GetValueAsString(smDt.Rows[0]["PRODUCTION_DATE"]);
            string cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
            foreach (DataRow row in idnDt.Rows)
            {
                DataTable clDT = idnDt.Clone();
                clDT.ImportRow(row);
                Dictionary<string, object> parm = new Dictionary<string, object>();
                if (!"T".Equals(tranType1) && isFirst)
                {
                    parm["WS_CD"] = "";
                    parm["DLV_AREA_NM"] = PolNm2;
                    parm["DLV_ADDR"] = DepAddr2;
                }
                else
                {
                    string WsCd = Prolink.Math.GetValueAsString(clDT.Rows[0]["WS_CD"]);
                    string AddrCode = Prolink.Math.GetValueAsString(clDT.Rows[0]["ADDR_CODE"]);
                    if (string.IsNullOrEmpty(WsCd) && !string.IsNullOrEmpty(AddrCode))
                    {
                        string sql = string.Format("SELECT TOP 1 WS_CD FROM SMWH WHERE DLV_ADDR={0} AND CMP={1}", SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(cmp));
                        WsCd = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    parm["WS_CD"] = WsCd;
                    parm["DLV_AREA_NM"] = Prolink.Math.GetValueAsString(clDT.Rows[0]["DLV_AREA_NM"]);
                    parm["DLV_ADDR"] = Prolink.Math.GetValueAsString(clDT.Rows[0]["DLV_ADDR"]);
                }
                parm["PICK_AREA"] = this.Pol;
                parm["PICK_AREA_NM"] = this.PolName;
                parm["ORD_NO"] = this.OrdNo;
                parm["ARRIVAL_DATE"] = arrivalDate;
                RegisterEditInstructFunc("InboundSMRDNMapping", RdnEditInstruct);
                ParseEditInstruct(clDT, "InboundSMRDNMapping", ml, parm);
            }
        }
        static string RdnEditInstruct(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            ei.Put("ORD_NO", parm["ORD_NO"]);
            ei.Put("PICK_AREA_NM", parm["PICK_AREA_NM"]);
            ei.Put("PICK_AREA", parm["PICK_AREA"]);
            ei.Put("DLV_ADDR", parm["DLV_ADDR"]);
            ei.Put("PICK_AREA", parm["PICK_AREA"]);
            ei.Put("DLV_ADDR", parm["DLV_ADDR"]);
            ei.Put("DLV_AREA_NM", parm["DLV_AREA_NM"]);
            ei.Put("WS_CD", parm["WS_CD"]);
            ei.PutDate("ARRIVAL_DATE", parm["ARRIVAL_DATE"]);
            return string.Empty;
        }

        public static void UpdateBillInfoToSMORD(string shipment, string cntrNo, EditInstruct ei)
        {
            string condition = "";
            if(!string.IsNullOrEmpty(cntrNo))
                condition += string.Format(" AND CNTR_NO={0} ", SQLUtils.QuotedStr(cntrNo));
            string sql = string.Format("SELECT CMP,CNTR_NO FROM SMORD WHERE SHIPMENT_ID={0} AND CNTR_NO IS NOT NULL {1}",
                SQLUtils.QuotedStr(shipment), condition);
            DataTable ordDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ordDt.Rows.Count <= 0)
                return;
            Dictionary<string, BillOrdInfo> billOrdDic = new Dictionary<string, BillOrdInfo>();
            string cmp = Prolink.Math.GetValueAsString(ordDt.Rows[0]["CMP"]);
            foreach (DataRow dr in ordDt.Rows)
            {
                string ordCntr = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                string key = shipment + "_" + ordCntr;
                BillOrdInfo billOrdInfo = new BillOrdInfo(shipment, ordCntr);
                if (!billOrdDic.ContainsKey(key))
                    billOrdDic.Add(key, billOrdInfo);
            }
            if (!string.IsNullOrEmpty(cntrNo))
                condition += string.Format(" AND D.DEC_NO={0} ", SQLUtils.QuotedStr(cntrNo));
            sql = string.Format(@"SELECT D.QUOT_NO,D.DEBIT_NO,D.SHIPMENT_ID,D.DEC_NO,M.APPROVE_TO,M.VERIFY_DATE,D.QAMT,D.QCUR,D.BAMT,D.CUR,
D.CHG_CD FROM SMBID D LEFT JOIN SMBIM M ON D.U_FID=M.U_ID WHERE D.SHIPMENT_ID={0} AND D.CMP={1} AND D.CHG_CD IN ('DSTF','DDET','DDEM') {2}",
SQLUtils.QuotedStr(shipment), SQLUtils.QuotedStr(cmp), condition);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            UpdateBillInfoToSMORDByBID(dt, ei, true);
        }

        public static void UpdateBillInfoToSMORDByBID(DataTable dt, EditInstruct ei=null,bool UpdateAll=false)
        {
            bool update = false;
            MixedList ml = new MixedList();
            if (ei == null)
                update = true;
            Dictionary<string, BillOrdInfo> billOrdDic = new Dictionary<string, BillOrdInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                string shipment = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string decNo = Prolink.Math.GetValueAsString(dr["DEC_NO"]);
                string quotNo = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
                string debitNo = Prolink.Math.GetValueAsString(dr["DEBIT_NO"]);
                string approveTo = Prolink.Math.GetValueAsString(dr["APPROVE_TO"]);
                string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                DateTime? verifyDate = null;
                if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["VERIFY_DATE"])))
                    verifyDate = Prolink.Math.GetValueAsDateTime(dr["VERIFY_DATE"]);
                decimal qamt = Prolink.Math.GetValueAsDecimal(dr["QAMT"]);
                decimal bamt = Prolink.Math.GetValueAsDecimal(dr["BAMT"]);
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                string qcur = Prolink.Math.GetValueAsString(dr["QCUR"]);
                if (string.IsNullOrEmpty(decNo))
                    continue;
                string key = shipment + "_" + decNo;
                BillOrdInfo billOrdInfo = new BillOrdInfo(shipment, decNo);
                if (!billOrdDic.ContainsKey(key))
                    billOrdDic.Add(key, billOrdInfo);
                billOrdInfo = billOrdDic[key];
                if (!string.IsNullOrEmpty(quotNo))
                    billOrdInfo.QuotNo = quotNo;

                switch (chgCd)
                {
                    case "DSTF":
                        billOrdInfo.EStoCur = qcur;
                        billOrdInfo.AStoCur = cur;
                        billOrdInfo.EStoAmt = qamt;
                        billOrdInfo.AStoAmt = bamt;
                        billOrdInfo.StoDebitNo = debitNo;
                        billOrdInfo.StoApproveStatus = approveTo;
                        billOrdInfo.StoApproveTime = verifyDate;
                        break;
                    case "DDET":
                        billOrdInfo.EDetCur = qcur;
                        billOrdInfo.ADetCur = cur;
                        billOrdInfo.EDetAmt = qamt;
                        billOrdInfo.ADetAmt = bamt;
                        billOrdInfo.DetDebitNo = debitNo;
                        billOrdInfo.DetApproveStatus = approveTo;
                        billOrdInfo.DetApproveTime = verifyDate;
                        break;
                    case "DDEM":
                        billOrdInfo.EDemCur = qcur;
                        billOrdInfo.ADemCur = cur;
                        billOrdInfo.EDemAmt = qamt;
                        billOrdInfo.ADemAmt = bamt;
                        billOrdInfo.DemDebitNo = debitNo;
                        billOrdInfo.DemApproveStatus = approveTo;
                        billOrdInfo.DemApproveTime = verifyDate;
                        break;
                }
            }
            foreach (string key in billOrdDic.Keys)
            {
                if (update)
                    ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", billOrdDic[key].ShipmentId);
                ei.PutKey("CNTR_NO", billOrdDic[key].DecNo);
                ei.Put("QUOT_NO", DBNull.Value);
                if (!string.IsNullOrEmpty(billOrdDic[key].QuotNo))
                    ei.Put("QUOT_NO", billOrdDic[key].QuotNo);
                if (UpdateAll)
                {
                    SetSmordFee(billOrdDic[key], "STO", ei);
                    SetSmordFee(billOrdDic[key], "DET", ei);
                    SetSmordFee(billOrdDic[key], "DEM", ei);
                    SetSmordFee(billOrdDic[key], "STO", ei, "ACT");
                    SetSmordFee(billOrdDic[key], "DET", ei, "ACT");
                    SetSmordFee(billOrdDic[key], "DEM", ei, "ACT");
                }
                else
                {
                    if (!string.IsNullOrEmpty(billOrdDic[key].DemDebitNo))
                    {
                        SetSmordFee(billOrdDic[key], "DEM", ei);
                        SetSmordFee(billOrdDic[key], "DEM", ei, "ACT");
                    }
                    if (!string.IsNullOrEmpty(billOrdDic[key].DetDebitNo))
                    {
                        SetSmordFee(billOrdDic[key], "DET", ei);
                        SetSmordFee(billOrdDic[key], "DET", ei, "ACT");
                    }
                    if (!string.IsNullOrEmpty(billOrdDic[key].StoDebitNo))
                    {
                        SetSmordFee(billOrdDic[key], "STO", ei);
                        SetSmordFee(billOrdDic[key], "STO", ei, "ACT");
                    }
                }
                ml.Add(ei);
            }
            if (update && ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void SetSmordFee(BillOrdInfo billOrdInfo,string feeType, EditInstruct ei, string type = "EST")
        {
            decimal fee = 0;
            string cur = "", debitNo = "", approveTo="";
            DateTime? verifyDate = null;
            switch (feeType)
            {
                case "STO":
                    fee = type == "EST" ? billOrdInfo.EStoAmt : billOrdInfo.AStoAmt;
                    cur = type == "EST" ? billOrdInfo.EStoCur : billOrdInfo.AStoCur;
                    debitNo = billOrdInfo.StoDebitNo;
                    approveTo = billOrdInfo.StoApproveStatus;
                    verifyDate = billOrdInfo.StoApproveTime;
                    break;
                case "DET":
                    fee = type == "EST" ? billOrdInfo.EDetAmt : billOrdInfo.ADetAmt;
                    cur = type == "EST" ? billOrdInfo.EDetCur : billOrdInfo.ADetCur;
                    debitNo = billOrdInfo.DetDebitNo;
                    approveTo = billOrdInfo.DetApproveStatus;
                    verifyDate = billOrdInfo.DetApproveTime;
                    break;
                case "DEM":
                    fee = type == "EST" ? billOrdInfo.EDemAmt : billOrdInfo.ADemAmt;
                    cur = type == "EST" ? billOrdInfo.EDemCur : billOrdInfo.ADemCur;
                    debitNo = billOrdInfo.DemDebitNo;
                    approveTo = billOrdInfo.DemApproveStatus;
                    verifyDate = billOrdInfo.DemApproveTime;
                    break;
            }
            ei.Put(feeType + "_" + type + "_AMT", DBNull.Value);
            ei.Put(feeType + "_" + type + "_CUR", DBNull.Value);
            if (!string.IsNullOrEmpty(cur) || fee > 0)
            {
                ei.Put(feeType + "_" + type + "_AMT", fee);
                ei.Put(feeType + "_" + type + "_CUR", cur);
            }
            ei.Put(feeType + "_DEBIT_NO", debitNo);
            ei.Put(feeType + "_APPROVE_TO", approveTo);
            ei.PutDate(feeType + "_APPROVE_TIME", verifyDate);
        }

        public class BillOrdInfo
        {
            public string ShipmentId { get; set; }
            public string DecNo { get; set; }
            public string QuotNo { get; set; }

            public string EStoCur { get; set; }
            public decimal EStoAmt { get; set; }
            public string EDemCur { get; set; }
            public decimal EDemAmt { get; set; }
            public string EDetCur { get; set; }
            public decimal EDetAmt { get; set; }
            public string AStoCur { get; set; }
            public decimal AStoAmt { get; set; }
            public string ADemCur { get; set; }
            public decimal ADemAmt { get; set; }
            public string ADetCur { get; set; }
            public decimal ADetAmt { get; set; }
            public string StoDebitNo { get; set; }
            public string StoApproveStatus { get; set; }
            public DateTime? StoApproveTime { get; set; }
            public string DetDebitNo { get; set; }
            public string DetApproveStatus { get; set; }
            public DateTime? DetApproveTime { get; set; }
            public string DemDebitNo { get; set; }
            public string DemApproveStatus { get; set; }
            public DateTime? DemApproveTime { get; set; }

            public BillOrdInfo(string shipmentId, string decNo)
            {
                this.ShipmentId = shipmentId;
                this.DecNo = decNo;
            }
        }


        public InboundTransfer(string CsCd, string CsNm, string CsName, string Bu, string ShipmentId, string LCLCntrNo)
        {
            this.CsCd = CsCd;
            this.CsNm = CsNm;
            this.CsName = CsName;
            this.Bu = Bu;
            this.ShipmentId = ShipmentId;
            this.LCLCntrNo = LCLCntrNo;
        }

        public InboundTransfer() { }

        public string OrdNo { get; set; }
        public string CsCd { get; set; }
        public string CsNm { get; set; }
        public string CsName { get; set; }
        public string Bu { get; set; }
        public string PartQty { get; set; }
        public string AsnInfo { get; set; }
        public string PartInfo { get; set; }
        public string Pol { get; set; }
        public string PolName { get; set; }
        public string ShipmentId { get; set; }
        public string LCLCntrNo { get; set; }
    }
}
