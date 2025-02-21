using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace TrackingEDI.InboundBusiness
{
    public class BookingConfirm
    {
        public const string BOOKING_STATUS_S_ISFSending = "S";
        public const string BOOKING_STATUS_A_Unreach = "A";
        public const string BOOKING_STATUS_B_NotifyLSP = "B";
        public const string BOOKING_STATUS_C_NotifyBroker = "C";
        public const string BOOKING_STATUS_D_BrokerConfirm = "D";
        public const string BOOKING_STATUS_H_NotifyTransitBroker = "H";
        public const string BOOKING_STATUS_I_TransitConfirm = "I";
        public const string BOOKING_STATUS_G_GateIn = "G";
        public const string BOOKING_STATUS_P_POD = "P";
        public const string BOOKING_STATUS_O_GateOut = "O";
        public const string BOOKING_STATUS_Z_Finish = "Z";
        public const string BOOKING_STATUS_V_Void = "V";


        public const string BROKER_STATUS_B_DeclarationWithoutNotice = "B";
        public const string BROKER_STATUS_H_NotifyTransitBroker = "H";
        public const string BROKER_STATUS_I_TransitConfirm = "I";
        public const string BROKER_STATUS_Y_NotifyBroker = "Y";
        public const string BROKER_STATUS_C_BrokerConfirm = "C";
        public const string BROKER_STATUS_F_Release = "F";
        public static string SetConfirm(DataTable smdt, string UserId, string CompanyId)
        {
            MixedList ml = new MixedList();
            if (smdt.Rows.Count <= 0) return "No data!";
            string suid= Prolink.Math.GetValueAsString(smdt.Rows[0]["U_ID"]);
            string status = Prolink.Math.GetValueAsString(smdt.Rows[0]["STATUS"]);
            string bstatus = Prolink.Math.GetValueAsString(smdt.Rows[0]["BSTATUS"]);
            string ShipmentId = Prolink.Math.GetValueAsString(smdt.Rows[0]["SHIPMENT_ID"]);
            string intervalDays = Prolink.Math.GetValueAsString(smdt.Rows[0]["INTERVAL_DAY"]);
            if (!string.IsNullOrEmpty(intervalDays))
            {
                decimal intervalDay = decimal.Parse(intervalDays);
                if (intervalDay < 0)
                    return string.Format("Shipment ID:{0} ETA is not less than ETD, please check!", SQLUtils.QuotedStr(ShipmentId));
            }

            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", suid);
            string confirmStatus = "CDEFGPOXIJZ";
            if (!BOOKING_STATUS_D_BrokerConfirm.Equals(status) && !BOOKING_STATUS_H_NotifyTransitBroker.Equals(status)
                && !BOOKING_STATUS_I_TransitConfirm.Equals(status) && !BOOKING_STATUS_C_NotifyBroker.Equals(status) && confirmStatus.IndexOfAny(bstatus.ToCharArray()) < 0)
            {
                ei.Put("STATUS", BOOKING_STATUS_C_NotifyBroker);
                status = BOOKING_STATUS_C_NotifyBroker;
            }
            ei.Put("CSTATUS", SMSMIHelper.CALL_STATUS_Y_CallTruck);
            ei.Put("BL_WIN", UserId);
            DateTime ndt = TrackingEDI.Business.TimeZoneHelperCG.GetTimeZoneDate(DateTime.Now, CompanyId);
            ei.PutDate("LSP_CONFIRM_DATE", ndt);
            ml.Add(ei);

            try
            {
                string CMP = smdt.Rows[0]["CMP"].ToString();
                string podcd = smdt.Rows[0]["POD_CD"].ToString();

                string sql = string.Format("SELECT TOP 1 U_ID FROM TKBL WHERE SHIPMENT_ID={0} AND CMP={1} ORDER BY CREATE_DATE DESC", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(CMP));
                string tkuid = getOneValueAsStringFromSql(sql);
                if (!string.IsNullOrEmpty(tkuid))
                {
                    sql = "DELETE FROM TKBLPT WHERE U_ID=" + SQLUtils.QuotedStr(tkuid);
                    exeSql(sql);
                    #region 將shipment party轉到貨況party
                    sql = "SELECT M.GROUP_ID, M.CMP, M.STN, M.DEP, D.* FROM SMSMI M, SMSMIPT D WHERE D.U_FID=M.U_ID AND PARTY_TYPE IN('IBBR','IBCR','IBCB','IBCY','IBSP','IBTC','IBTW') AND M.U_ID=" + SQLUtils.QuotedStr(suid);
                    DataTable dt = getDataTableFromSql(sql);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            string GROUP_ID = Prolink.Math.GetValueAsString(item["GROUP_ID"]);
                            string STN = Prolink.Math.GetValueAsString(item["STN"]);
                            string DEP = Prolink.Math.GetValueAsString(item["DEP"]);
                            string PARTY_TYPE = Prolink.Math.GetValueAsString(item["PARTY_TYPE"]);
                            string SHIPMENT_ID = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                            string TYPE_DESCP = Prolink.Math.GetValueAsString(item["TYPE_DESCP"]);
                            string ORDER_BY = Prolink.Math.GetValueAsString(item["ORDER_BY"]);
                            string PARTY_NO = Prolink.Math.GetValueAsString(item["PARTY_NO"]);
                            string PARTY_NAME = Prolink.Math.GetValueAsString(item["PARTY_NAME"]);
                            string PARTY_ADDR1 = Prolink.Math.GetValueAsString(item["PARTY_ADDR1"]);
                            string PARTY_ATTN = Prolink.Math.GetValueAsString(item["PARTY_ATTN"]);
                            string PARTY_TEL = Prolink.Math.GetValueAsString(item["PARTY_TEL"]);
                            string PARTY_MAIL = Prolink.Math.GetValueAsString(item["PARTY_MAIL"]);
                            string duid = System.Guid.NewGuid().ToString();


                            string str = SQLUtils.QuotedStr(tkuid) + "," + SQLUtils.QuotedStr(PARTY_TYPE) + "," + SQLUtils.QuotedStr(PARTY_NO) + "," + SQLUtils.QuotedStr(SHIPMENT_ID) + "," + SQLUtils.QuotedStr(ORDER_BY) + "," + SQLUtils.QuotedStr(PARTY_NAME) + "," + SQLUtils.QuotedStr(PARTY_ADDR1) + "," + SQLUtils.QuotedStr(PARTY_ATTN) + "," + SQLUtils.QuotedStr(PARTY_TEL) + "," + SQLUtils.QuotedStr(PARTY_MAIL) + "," + SQLUtils.QuotedStr(GROUP_ID) + "," + SQLUtils.QuotedStr(CMP) + "," + SQLUtils.QuotedStr(STN) + "," + SQLUtils.QuotedStr(DEP) + "," + SQLUtils.QuotedStr(TYPE_DESCP);
                            sql = @"INSERT INTO TKBLPT( 
                                    U_ID,
                                    PARTY_TYPE,
                                    PARTY_NO,
                                    SHIPMENT_ID,
                                    ORDER_BY,
                                    PARTY_NAME,
                                    PARTY_ADDR,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    GROUP_ID,
                                    CMP,
                                    STN,
                                    DEP,
                                    TYPE_DESCP  
                                    ) VALUES ({0})";
                            sql = string.Format(sql, str);
                            ml.Add(sql);
                        }
                    }
                    #endregion
                }
                CreateOrdNew(ShipmentId, suid, ml);//CreateOrd(ShipmentId, suid)
                EditInstruct ordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ordEi.PutKey("SHIPMENT_ID", ShipmentId);
                ordEi.Put("STATUS", status);
                ordEi.Put("CSTATUS", SMSMIHelper.CALL_STATUS_Y_CallTruck);
                ordEi.Put("BL_WIN", UserId);
                ml.Add(ordEi);
                EditInstruct AutoCalculTaskEi = new EditInstruct("AUTO_IBCALCUL_TASK", EditInstruct.INSERT_OPERATION);
                AutoCalculTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
                AutoCalculTaskEi.Put("SHIPMENT_ID", ShipmentId);
                AutoCalculTaskEi.Put("DONE", "N");
                AutoCalculTaskEi.Put("CREATE_BY", UserId);
                AutoCalculTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                ml.Add(AutoCalculTaskEi);
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());



                TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status
                {
                    ShipmentId = ShipmentId,
                    StsCd = "020",
                    Sender = UserId,
                    Cmp = CMP,
                    Location = podcd,
                    StsDescp = "Lsp Confirm"
                });

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return string.Empty;
        }
         
        public static string CreateOrdNew(string ShipmentId, string suid, MixedList mixList = null)
        {
            string msg = "";
            bool excute = false;
            if (mixList == null)
            {
                mixList = new MixedList();
                excute = true;
            }

            Action<List<string>, List<string>, DataTable> onAdd = (items, items2, dt) =>
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string dnNo = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    string addrCode = Prolink.Math.GetValueAsString(dr["ADDR_CODE"]);
                    if (!string.IsNullOrEmpty(addrCode) && !items2.Contains(addrCode))
                        items2.Add(addrCode);
                    if (string.IsNullOrEmpty(dnNo)) continue;
                    string[] temps = dnNo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string dn in temps)
                    {
                        if (items.Contains(dn))
                            continue;
                        items.Add(dn);
                    }
                }
            };

            string smordsql = string.Format("SELECT CNTR_NO,SHIPMENT_ID,ARRIVAL_DATE,ETA_MSL,ETA_MSL_TIME FROM SMORD WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable smorddt = getDataTableFromSql(smordsql);

            string sql = "DELETE FROM SMORD WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            mixList.Add(sql);
            sql = "DELETE FROM SMRDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            mixList.Add(sql);
            sql = "DELETE FROM SMRCNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            mixList.Add(sql);

            sql = "SELECT * FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(suid);
            DataTable sdt = getDataTableFromSql(sql);
            if (sdt.Rows.Count <= 0) return "Cann't find Shipment:" + ShipmentId;
            List<string> dnList = new List<string>();
            List<string> aCdList = new List<string>();
            sql = "SELECT * FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dnDt = getDataTableFromSql(sql);
            onAdd(dnList, aCdList, dnDt);
            sql = "SELECT * FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable cntrDt = getDataTableFromSql(sql);
            onAdd(dnList, aCdList, cntrDt);
            string TranType = sdt.Rows[0]["TRAN_TYPE"].ToString();
            string csCd = "", csNm = "", bu = "", csName = "";
            string Cmp = Prolink.Math.GetValueAsString(sdt.Rows[0]["CMP"]);
            string Gid = Prolink.Math.GetValueAsString(sdt.Rows[0]["GROUP_ID"]);
            sql = string.Format("SELECT Y.ABBR,Y.DEP,Y.PARTY_NAME,T.PARTY_NO,T.PARTY_TYPE FROM SMPTY Y LEFT JOIN SMSMIPT T ON T.PARTY_NO=Y.PARTY_NO WHERE T.SHIPMENT_ID={0} AND T.PARTY_TYPE IN ('CS','ZT')", SQLUtils.QuotedStr(ShipmentId));
            DataTable ptDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(dnList.ToArray()));
            DataTable smicuftDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT ADDR_CODE,PLANT FROM BSADDR WHERE FINAL_WH='TEMP' AND ADDR_CODE IN {0}", SQLUtils.Quoted(aCdList.ToArray()));
            DataTable addrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ptDt.Rows.Count > 0)
            {
                foreach (DataRow dr in ptDt.Rows)
                {
                    string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    switch (partyType)
                    {
                        case "CS":
                            csNm = Prolink.Math.GetValueAsString(dr["PARTY_NAME"]);
                            csCd = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                            csName = Prolink.Math.GetValueAsString(dr["ABBR"]);
                            break;
                        case "ZT":
                            bu = Prolink.Math.GetValueAsString(dr["DEP"]);
                            break;
                    }
                }
            }
            string LCLCntrNo = "";
            if (TranType == "L" && cntrDt.Rows.Count > 0)
            {
                LCLCntrNo = Prolink.Math.GetValueAsString(cntrDt.Rows[0]["CNTR_NO"]);
            }
            InboundTransfer ib = new InboundTransfer(csCd, csNm, csName, bu, ShipmentId, LCLCntrNo);
            List<string> valList = new List<string>() { "one", "two", "three" };
            switch (TranType)
            {
                case "F":
                case "R":
                    sql = string.Format("SELECT * FROM SMIDNP WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                    DataTable pdt = getDataTableFromSql(sql);
                    foreach (DataRow cntrDr in cntrDt.Rows)
                    {
                        string cntrUid = Prolink.Math.GetValueAsString(cntrDr["U_ID"]);
                        List<string> partList = new List<string>();
                        List<string> asnList = new List<string>();
                        List<int> qtyList = new List<int>();
                        string CntrNo = cntrDr["CNTR_NO"].ToString();
                        DataRow[] pdrs = pdt.Select(string.Format("CNTR_NO={0}", SQLUtils.QuotedStr(CntrNo)));
                        foreach (DataRow pdr in pdrs)
                        {
                            string asnNo = Prolink.Math.GetValueAsString(pdr["ASN_NO"]);
                            string partNo = Prolink.Math.GetValueAsString(pdr["PART_NO"]);
                            if (string.IsNullOrEmpty(partNo))
                            {
                                partNo = Prolink.Math.GetValueAsString(pdr["IPART_NO"]);
                            }
                            int qty = Prolink.Math.GetValueAsInt(pdr["QTY"]);
                            if (!string.IsNullOrEmpty(partNo))
                            {
                                partList.Add(partNo);
                                qtyList.Add(qty);
                                asnList.Add(asnNo);
                            }
                        }
                        ib.AsnInfo = string.Join(",", asnList);
                        if (ib.AsnInfo.Length > 500)
                            ib.AsnInfo = ib.AsnInfo.Substring(0, 500);
                        ib.PartInfo = string.Join(",", partList);
                        if (ib.PartInfo.Length > 500)
                            ib.PartInfo = ib.PartInfo.Substring(0, 500);
                        ib.PartQty = string.Join(",", qtyList);
                        if (ib.PartQty.Length > 500)
                            ib.PartQty = ib.PartQty.Substring(0, 500);
                        string CntrType = cntrDr["CNTR_TYPE"].ToString();
                        string cntrtype = string.Empty;
                        switch (CntrType)
                        {
                            case "20GP":
                                cntrtype = "CNT20";
                                break;
                            case "40GP":
                                cntrtype = "CNT40";
                                break;
                            case "40HQ":
                                cntrtype = "CNT40HQ";
                                break;
                        }
                        string addrCode = Prolink.Math.GetValueAsString(cntrDr["ADDR_CODE"]);
                        string plant = string.Empty;
                        if (!string.IsNullOrEmpty(addrCode))
                        {
                            DataRow[] drs = addrDt.Select(string.Format("ADDR_CODE={0}", SQLUtils.QuotedStr(addrCode)));
                            if (drs.Length > 0)
                                plant = Prolink.Math.GetValueAsString(drs[0]["PLANT"]);
                        }
                        for (int i = 1; i < 4; i++)
                        {
                            string tranType = Prolink.Math.GetValueAsString(cntrDr["TRAN_TYPE" + i]);
                            if (string.IsNullOrEmpty(tranType))
                                continue;
                            ib.Pol = Prolink.Math.GetValueAsString(cntrDr["POL" + i]);
                            ib.PolName = Prolink.Math.GetValueAsString(cntrDr["POL_NM" + i]);
                            ib.OrdNo = ReserveHelper.getAutoNo("ORD_NO", Gid, Cmp);
                            msg = ib.SaveToTruckCalling(sdt, valList[i - 1], mixList, cntrDr);
                            if (!string.IsNullOrEmpty(msg)) return msg;
                            string cuft = string.Empty;
                            if (tranType == "T" || tranType == "R" || (i == 1 && tranType == "I"))
                            {
                                DataTable clDt = cntrDt.Clone();
                                clDt.ImportRow(cntrDr);
                                msg = ib.saveToSmrcntr(sdt, clDt, mixList);
                                if (!string.IsNullOrEmpty(msg)) return msg;
                                string dnNo = Prolink.Math.GetValueAsString(cntrDr["DN_NO"]);
                                string[] dns = dnNo.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                                if (dns.Length > 0)
                                    cuft = SMSMIHelper.GetCuft(smicuftDt.Select(string.Format("DN_NO IN {0}", SQLUtils.Quoted(dns))));
                            }
                            string conditions = string.Format(" CNTR_NO={0}", SQLUtils.QuotedStr(CntrNo));
                            mixList.Add(SetSMORNArrival(cntrDt.Select(conditions), ib.ShipmentId, ib.OrdNo, TranType, "AND" + conditions));
                            SetScmReqETAmsl(smorddt, ib.OrdNo, CntrNo, mixList, cntrDr, plant, cuft, cntrtype);
                        }

                        EditInstruct smicntr = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                        smicntr.PutKey("U_ID", cntrUid);
                        smicntr.PutExpress("DEMURRAGE_DUE_DATE", "DEMURRAGE_DUE_DATE");
                        smicntr.PutExpress("DETENTION_DUE_DATE", "DETENTION_DUE_DATE");
                        smicntr.PutExpress("STORAGE_DUE_DATE", "STORAGE_DUE_DATE");
                        smicntr.PutExpress("PORT_CHG_TYPE", "PORT_CHG_TYPE");
                        smicntr.PutExpress("FACT_CHG_TYPE", "FACT_CHG_TYPE");
                        smicntr.PutExpress("CON_CHG_TYPE", "CON_CHG_TYPE");
                        mixList.Add(smicntr);
                    }
                    break;
                default:
                    DataRow dr = sdt.Rows[0];
                    ib.AsnInfo = Prolink.Math.GetValueAsString(dr["ASNNO_INFO"]);
                    ib.PartInfo = Prolink.Math.GetValueAsString(dr["PARTNO_INFO"]);
                    ib.PartQty = Prolink.Math.GetValueAsString(dr["PART_QTY"]);
                    for (int i = 1; i < 4; i++)
                    {
                        string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE" + i]);
                        if (string.IsNullOrEmpty(tranType))
                            continue;
                        ib.Pol = Prolink.Math.GetValueAsString(dr["POL" + i]);
                        ib.PolName = Prolink.Math.GetValueAsString(dr["POL_NM" + i]);
                        ib.OrdNo = ReserveHelper.getAutoNo("ORD_NO", Gid, Cmp);
                        msg=ib.SaveToTruckCalling(sdt, valList[i - 1], mixList);
                        if (!string.IsNullOrEmpty(msg)) return msg;
                        string cuft = string.Empty, plant = string.Empty;
                        if ("T".Equals(TranType)) continue;
                        if ("T".Equals(tranType) || 1 == i)
                        {
                            ib.saveToSmrdn(sdt, dnDt, i == 1 ? true : false, mixList);
                            if (cntrDt.Rows.Count > 0)
                            {
                                msg=ib.saveToSmrcntr(sdt, cntrDt, mixList);
                                if (!string.IsNullOrEmpty(msg)) return msg;
                                foreach (DataRow cntrDr in cntrDt.Rows)
                                {
                                    string addrCode = Prolink.Math.GetValueAsString(cntrDr["ADDR_CODE"]);
                                    if (string.IsNullOrEmpty(addrCode))
                                        continue;
                                    DataRow[] drs = addrDt.Select(string.Format("ADDR_CODE={0}", SQLUtils.QuotedStr(addrCode)));
                                    if (drs.Length > 0)
                                        plant = Prolink.Math.GetValueAsString(drs[0]["PLANT"]);
                                    break;
                                }
                            }
                            cuft = SMSMIHelper.GetCuft(smicuftDt.Select());
                        }
                        mixList.Add(SetSMORNArrival(cntrDt.Select(), ib.ShipmentId, ib.OrdNo, TranType));
                        SetScmReqETAmsl(smorddt, ib.OrdNo, "", mixList, dnDt.Rows[0], plant, cuft);
                    }
                    break;
            }

            if (excute && mixList.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }
            return msg;
        }

        public static string SetSMORNArrival(string shipmentid, string ornno, string cntrno = "")
        {
            string conditions = string.Empty;
            if (!string.IsNullOrEmpty(cntrno))
            {
                conditions = string.Format(" AND CNTR_NO={0}", SQLUtils.QuotedStr(cntrno));
            }

            string sql = string.Format("SELECT count(1) FROM SMICNTR WHERE SHIPMENT_ID={0} {1}",
                SQLUtils.QuotedStr(shipmentid), conditions);
            int cntrcount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentid));
            string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (cntrcount > 0 && !"A".Equals(trantype))
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID {0}),
                PRIORITY=(SELECT MIN(PRIORITY) FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID {0}),
                SLIP_SHEET_INFO=(SELECT DISTINCT PKG_UNIT_DESC+';' FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMIDN.PKG_UNIT_DESC like '%PLASTIC%' FOR XML PATH('')) 
                WHERE ORD_NO={1} AND SHIPMENT_ID={2}",
                 conditions, SQLUtils.QuotedStr(ornno), SQLUtils.QuotedStr(shipmentid));
            }
            else
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),
                PRIORITY=(SELECT MIN(PRIORITY) FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),
                SLIP_SHEET_INFO=(SELECT DISTINCT PKG_UNIT_DESC+';' FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMIDN.PKG_UNIT_DESC like '%PLASTIC%' FOR XML PATH('')) 
                WHERE ORD_NO={0} AND SHIPMENT_ID={1}",
                SQLUtils.QuotedStr(ornno), SQLUtils.QuotedStr(shipmentid));
            }
            return sql;
        }

        public static string SetSMORNArrival(DataRow[] cntrDrs, string shipmentid, string ornno, string trantype, string conditions = "")
        {
            int cntrcount = cntrDrs.Count();
            string sql = string.Empty;
            if (cntrcount > 0 && !"A".Equals(trantype))
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID {0}),
                PRIORITY=(SELECT MIN(PRIORITY) FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID {0}),
                SLIP_SHEET_INFO=(SELECT DISTINCT PKG_UNIT_DESC+';' FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMIDN.PKG_UNIT_DESC like '%PLASTIC%' FOR XML PATH('')) 
                WHERE ORD_NO={1} AND SHIPMENT_ID={2}",
                 conditions, SQLUtils.QuotedStr(ornno), SQLUtils.QuotedStr(shipmentid));
            }
            else
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),
                PRIORITY=(SELECT MIN(PRIORITY) FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),
                SLIP_SHEET_INFO=(SELECT DISTINCT PKG_UNIT_DESC+';' FROM SMIDN WHERE SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMIDN.PKG_UNIT_DESC like '%PLASTIC%' FOR XML PATH('')) 
                WHERE ORD_NO={0} AND SHIPMENT_ID={1}",
                SQLUtils.QuotedStr(ornno), SQLUtils.QuotedStr(shipmentid));
            }
            return sql;
        }

        public static void SetScmReqETAmsl(DataTable smorddt, string ordno, string newcntr, MixedList ml, DataRow idnicntrdr, string plant = "", string cuft = "", string cntrtype = "")
        {
            EditInstruct smordedi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            smordedi.PutKey("ORD_NO", ordno);
            string addrcode = Prolink.Math.GetValueAsString(idnicntrdr["ADDR_CODE"]);
            string wscd = Prolink.Math.GetValueAsString(idnicntrdr["WS_CD"]);
            string wsnm = Prolink.Math.GetValueAsString(idnicntrdr["WS_NM"]);
            string finalwh = Prolink.Math.GetValueAsString(idnicntrdr["FINAL_WH"]);
            smordedi.Put("ADDR_CODE", addrcode);
            smordedi.Put("WS_CD", wscd);
            smordedi.Put("WS_NM", wsnm);
            smordedi.Put("FINAL_WH", finalwh);
            if (!string.IsNullOrEmpty(cuft))
                smordedi.Put("DIMENSIONS_INFO", cuft);
            if (!string.IsNullOrEmpty(cntrtype))
                smordedi.Put(cntrtype, 1);
            smordedi.Put("PLANT", DBNull.Value);
            if (!string.IsNullOrEmpty(plant))
                smordedi.Put("PLANT", plant);
            ml.Add(smordedi);

            if (smorddt.Rows.Count <= 0)
                return;
            try
            {
                DateTime arrivalDate = Prolink.Math.GetValueAsDateTime(smorddt.Rows[0]["ARRIVAL_DATE"]);
                DateTime etaMsl = Prolink.Math.GetValueAsDateTime(smorddt.Rows[0]["ETA_MSL"]);
                string etamsltime = Prolink.Math.GetValueAsString(smorddt.Rows[0]["ETA_MSL_TIME"]);
                EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                DataRow[] dr = smorddt.Select(string.Format("CNTR_NO={0}", SQLUtils.QuotedStr(newcntr)));
                if (dr.Length > 0)
                {
                    arrivalDate = Prolink.Math.GetValueAsDateTime(dr[0]["ARRIVAL_DATE"]);
                    etaMsl = Prolink.Math.GetValueAsDateTime(dr[0]["ETA_MSL"]);
                    etamsltime = dr[0]["ETA_MSL_TIME"].ToString();
                }
                string arrivaldate = getDate(arrivalDate, "yyyyMMddHHmmss");
                string etamsl = getDate(etaMsl, "yyyMMdd");

                ei.PutKey("ORD_NO", ordno);
                if (!string.IsNullOrEmpty(arrivaldate))
                    ei.PutDate("ARRIVAL_DATE", arrivaldate);
                if (!string.IsNullOrEmpty(etamsl))
                    ei.PutDate("ETA_MSL", etamsl);
                ei.Put("ETA_MSL_TIME", etamsltime);
                ml.Add(ei);
            }
            catch (Exception ex) { }
        }

        private static string getDate(DateTime dateTime, string format)
        {
            DateTime tdt = new DateTime(2000, 1, 1);
            string date = "";
            if (dateTime.CompareTo(tdt) > 0)
            {
                date = dateTime.ToString(format, CultureInfo.InvariantCulture);
            }
            return date;
        }

        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
        public static string getOneValueAsStringFromSql(string sql)
        {
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void exeSql(string sql)
        {
            if (sql != "")
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
         
    }
}
