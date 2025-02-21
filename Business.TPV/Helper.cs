using Business.Service;
using Business.TPV.Export;
using Business.TPV.RFC;
using Business.TPV.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;

namespace Business.TPV
{
    public static class Helper
    {
        const string SPMS = "SPMS";
        public static string GetCompany(string sapId, string planCode = "",string refno="")
        {
            string sql = string.Empty;
            if (sapId == SPMS)
                return GetCompanyForSPMS(sapId, planCode);
            else
                return GetCompanyForSAP(sapId, planCode, refno);
        }
        static string GetCompanyForSPMS(string sapId, string planCode)
        {
            string sql = string.Format("SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TSAP' AND CD LIKE '%{0}%' AND CD_DESCP LIKE '%{1}%'", sapId, planCode);
            return Business.Utils.DBManager.DefaultDB.GetValueAsString(sql);
        }
        static string GetCompanyForSAP(string sapId, string planCode,string refno)
        {
            string sql="SELECT CMP FROM SMDN WHERE ORIGIN_NO='"+refno+"' AND DN_NO_CMP_REF IS NULL";
            string cmp=Business.Utils.DBManager.DefaultDB.GetValueAsString(sql);
            if (!string.IsNullOrEmpty(cmp)) return cmp;
            sql = string.Format("SELECT AP_CD FROM BSCODE WHERE CD_TYPE='PLT' AND CD={0}", SQLUtils.QuotedStr(planCode));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return Prolink.Math.GetValueAsString(dt.Rows[0]["AP_CD"]);
        }

        public static void SendICACargoInfo(DataRow smRow)
        {
            string shipmentId = "";
            string cmp = "";
            try
            {
                shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
                cmp = Prolink.Math.GetValueAsString(smRow["CMP"]);
                string sql = string.Format(@"SELECT  TOP 1 IB_CMP FROM (SELECT (SELECT TOP 1 AP_CD FROM BSCODE WHERE CD_TYPE='CULO' 
            AND CD=SMSMPT.PARTY_NO AND (CMP='*' OR CMP={0})) AS IB_CMP FROM SMSMPT WHERE SHIPMENT_ID={1}
                AND PARTY_TYPE IN ('CS','NT','WE'))T WHERE IB_CMP IS NOT NULL", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(shipmentId));
                string ibcmp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (ISSendICAInfo(ibcmp))
                {
                    Business.TPV.Export.ICAInfoManager m = new Business.TPV.Export.ICAInfoManager();
                    string combininfo = Prolink.Math.GetValueAsString(smRow["COMBINE_INFO"]);
                    combininfo = combininfo.Trim(',');
                    string[] dnitems = combininfo.Split(',');
                    string isok= Prolink.Math.GetValueAsString(smRow["IS_OK"]);
                    if ("T".Equals(isok))
                    {
                        isok = "N";
                    }
                    List<Business.TPV.RFC.ICAInfo> items = m.GetICAInfo(dnitems, isok);

                    #region 比对传前与发送时DN NO是否一致
                    bool sendFlag = true;
                    List<string> dnList = new List<string>();//查询后的DN NO
                    foreach (var item in items)
                    {
                        dnList.Add(item.DNNO);
                    }
                    foreach (var dnNo in dnitems)
                    {
                        if (!dnList.Contains(dnNo))
                        {
                            sendFlag = false;
                            continue;
                        }
                    }
                    //Helper.CreateICALog(shipmentId, cmp, "DN No Before:" + string.Join(",", dnitems)+ ";DN No After:" + string.Join(",", dnList), sendFlag ? "Succeed" : "Exception");
                    if (!sendFlag)
                        return;
                    #endregion

                    Business.Service.ResultInfo result1 = m.TryPostCargoInfo(null, items, "", ibcmp, shipmentId, cmp);
                }
                else {
                    string combininfo = Prolink.Math.GetValueAsString(smRow["COMBINE_INFO"]);
                    combininfo = combininfo.Trim(',');
                    string[] dnitems = combininfo.Split(',');
                    CreateNICATask(dnitems);
                }
            }
            catch (Exception ex) {
                CreateICALog(shipmentId, cmp, " Ex1:" + ex.Message);
            }
        }

        public static void CreateNICATask(string[] dnitems)
        {
            try
            {
                MixedList ml = new MixedList();
                EditInstruct ei = new EditInstruct("NICA_EDI_TASK", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("DN_NO", string.Join(",", dnitems));
                ei.Put("RESULT_STATUS", "N");
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ml.Add(ei);
                if (ml != null && ml.Count > 0)
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void CreateICALog(string shipmentId, string cmp, string edidata, string status = "Exception", string remark = "")
        {
            try
            {
                MixedList ml = new MixedList();
                EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
                string uId = System.Guid.NewGuid().ToString();
                ei.Put("U_ID", uId);
                ei.Put("EDI_ID", Base.EDIModes.PostICAInfoToSAP);
                ei.Put("SENDER", "System");
                ei.Put("RS", "Send");
                ei.Put("FROM_CD", "eShipping");
                ei.Put("TO_CD", "SAP");
                ei.Put("DATA_FOLDER", "");
                ei.Put("REF_NO", shipmentId);
                ei.Put("GROUP_ID", "TPV");
                ei.Put("CMP", cmp);
                ei.Put("STN", "*");
                if (!string.IsNullOrEmpty(edidata))
                {
                    EditInstruct edidataei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                    edidataei.Put("U_ID", uId);
                    edidataei.Put("EDI_DATE", edidata);
                    edidataei.PutExpress("CREATE_DATE", "getdate()");
                    ml.Add(edidataei);
                }
                if (!string.IsNullOrEmpty(remark) && remark.Length > 500)
                    remark = remark.Substring(0, 500);
                ei.Put("REMARK", remark);
                ei.PutExpress("EVENT_DATE", "getdate()");
                ei.Put("STATUS", status);
                ml.Add(ei);
                if (ml != null && ml.Count > 0)
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static bool ISSendICAInfo(string ibcmp)
        {
            bool send = false;
            string sql = "SELECT CD FROM BSCODE WHERE CD_TYPE='ICA' AND CMP='*'";
            DataTable bscodedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (bscodedt.Rows.Count > 0)
            {
                sql = string.Format("SELECT COUNT(1) FROM BSCODE WHERE CD_TYPE='ICA' AND CMP='*' AND CD={0}",
                    SQLUtils.QuotedStr(ibcmp));
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (count > 0) send= true;
            }
            else if (ibcmp == "PL")
            {
                send= true;
            }
            return send;
        }


        public static void AddEDIInfoToDB(EdiInfo ediInfo, MixedList mixedList = null)
        {
            try
            {
                Prolink.DataOperation.EditInstruct ei = new Prolink.DataOperation.EditInstruct("EDI_LOG", Prolink.DataOperation.EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", ediInfo.ID);
                ei.Put("EDI_ID", ediInfo.EdiId);
                ei.PutExpress("EVENT_DATE", "getdate()");
                if (!string.IsNullOrEmpty(ediInfo.Remark) && ediInfo.Remark.Length > 500)
                    ediInfo.Remark = ediInfo.Remark.Substring(0, 500);
                ei.Put("REMARK", ediInfo.Remark);
                ei.Put("SENDER", ediInfo.CreateBy);
                ei.Put("RS", ediInfo.Rs);
                ei.Put("STATUS", ediInfo.Status);
                ei.Put("FROM_CD", ediInfo.FromCd);
                ei.Put("TO_CD", ediInfo.ToCd);
                ei.Put("DATA_FOLDER", ediInfo.DataFolder);
                ei.Put("REF_NO", ediInfo.RefNO);
                ei.Put("GROUP_ID", ediInfo.GroupId);
                ei.Put("CMP", ediInfo.Cmp);
                ei.Put("STN", ediInfo.Stn);
                MixedList el = new MixedList();
                string edidata = Prolink.Math.GetValueAsString(ediInfo.Data);
                if (!string.IsNullOrEmpty(edidata) && el != null)
                {
                    EditInstruct edidataei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                    edidataei.Put("U_ID", ediInfo.ID);
                    edidataei.Put("EDI_DATE", edidata);
                    edidataei.PutExpress("CREATE_DATE", "getdate()");
                    el.Add(edidataei);
                }
                el.Add(ei);
                if (mixedList == null)
                {
                    int[] result = OperationUtils.ExecuteUpdate(el, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                else
                {
                    mixedList.Add(ei);
                }
            }
            catch (Exception e) { }
        }

        public static void SendICACargoInfoByInbound(DataRow smRow)
        {
            string shipmentId = "";
            string cmp = "";
            try
            {
                shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
                cmp = Prolink.Math.GetValueAsString(smRow["CMP"]);
                if (!"PL".Equals(cmp))
                    return;
                Business.TPV.Export.ICAInfoManager m = new Business.TPV.Export.ICAInfoManager();
                string combininfo = Prolink.Math.GetValueAsString(smRow["COMBINE_INFO"]);
                combininfo = combininfo.Trim(',');
                string[] dnitems = combininfo.Split(',');
                List<Business.TPV.RFC.ICAInfo> items = m.GetICAInfoInBound(dnitems);


                #region 比对传前与发送时DN NO是否一致
                bool sendFlag = true;
                List<string> dnList = new List<string>();//查询后的DN NO
                foreach (var item in items)
                {
                    dnList.Add(item.DNNO);
                }
                foreach (var dnNo in dnitems)
                {
                    if (!dnList.Contains(dnNo))
                    {
                        sendFlag = false;
                        continue;
                    }
                }
                //Helper.CreateICALog(shipmentId, cmp, "DN No Before:" + string.Join(",", dnitems)+";DN No After:" + string.Join(",", dnList), sendFlag ? "Succeed" : "Exception");
                if (!sendFlag)
                    return;
                #endregion

                Business.Service.ResultInfo result1 = m.TryPostCargoInfo(null, items, "", "", shipmentId, cmp);
            }
            catch (Exception ex) {
                CreateICALog(shipmentId, cmp, " Ex3:" + ex.Message);
            }
        }

        public static string GetSapId(string companyCode)
        {
            string sql = string.Format("SELECT CD FROM BSCODE WHERE CD_TYPE='TSAP' AND CD_DESCP={0}", SQLUtils.QuotedStr(companyCode));
            return Business.Utils.DBManager.DefaultDB.GetValueAsString(sql);
        }
        public static string GetSapUserId(string eShippingUserId)
        {
            string sql = string.Format("SELECT SAP_ID FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(eShippingUserId));
            return Business.Utils.DBManager.DefaultDB.GetValueAsString(sql);
        }

        public static Tuple<string, string> GetPortCode(string prolinkCD)
        {
            if (string.IsNullOrEmpty(prolinkCD)) return null;
            if (prolinkCD.Length < 5) return null;
            string country = prolinkCD.Substring(0, 2);
            string portCode = prolinkCD.Substring(2, 3);
            return new Tuple<string, string>(country, portCode);
        }

        public static EditInstruct ChangeTorder(string dnno, string shipmentid, string combineinfo)
        {
            string torder = string.Empty;
            string border = string.Empty;
            if (!CheckTorder(dnno, shipmentid, combineinfo,ref torder,ref border)) return null;
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            if (!"C".Equals(torder))
                ei.Put("TORDER", "S");

            if ("N".Equals(border))
                ei.Put("BORDER", "M");
            return ei;
        }

        public static IEnumerable<DataRow> GetBookingAgent(DataRow smRow)
        {
            if (smRow == null) return null;
            string shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
            string transType = Prolink.Math.GetValueAsString(smRow["TRAN_TYPE"]);
            var codes = GetTypeByTranType(transType);
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE in({1})",
              SQLUtils.QuotedStr(shipmentId), string.Join(",", codes.Select(c => SQLUtils.QuotedStr(c))));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows.Cast<DataRow>();
        }

        public static IEnumerable<DataRow> GetBookingAgent(string shipmentId)
        {
            if (string.IsNullOrEmpty(shipmentId)) return null;
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return GetBookingAgent(dt.Rows[0]);
        }

        public static string GetBroker(string shipmentId)
        {
            if (string.IsNullOrEmpty(shipmentId)) return null;
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE='BR'", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_NO"]);
        }

        public static ResultInfo TryGetBookingAgentOnlyOne(DataRow smRow, out string partyNO)
        {
            partyNO = null;
            var rows = GetBookingAgent(smRow).ToList();
            if (rows == null || rows.Count <= 0)
                return new ResultInfo
                {
                    ResultCode = "NoExists",
                    Description = "请确认该笔订舱代理,货代或Express细档资料是否齐全"
                };
            if (rows.Count > 1)
            {
                int ind = 0;
                var items = rows.Select(row => new
                {
                    index = ind++,
                    t_name = Prolink.Math.GetValueAsString(row["TYPE_DESCP"]),
                    no = Prolink.Math.GetValueAsString(row["PARTY_NO"])
                });
                return new ResultInfo
                {
                    ResultCode = "MaxCountError",
                    Description = string.Format("该笔的 {0} 订舱代理重复，如下资料请删除后保留一笔：{1}{2}", Environment.NewLine,
                    string.Join(Environment.NewLine, items.Select(x => string.Format("{0}.{1}:{2}", x.index, x.t_name, x.no))))
                };
            }
            partyNO = Prolink.Math.GetValueAsString(rows[0]["PARTY_NO"]);
            return ResultInfo.SucceedResult();
        }
        private static IEnumerable<string> GetTypeByTranType(string trantype)
        {
            switch (trantype)
            {
                case "A":
                case "L":
                case "R":
                    yield return "SP"; break;                   
                case "F":
                    yield return "BO";
                    yield return "SP";break;
                case "E":
                    yield return "SP";break;
                case "D":
                case "T":
                    yield return "CR";break;
                default:
                    yield break;
            }
        }

        public static void CancelBooking(string shipmenId, string cancelReason = "",bool IsChangStatus=true)
        {
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmenId));
            List<string> list = new List<string>()
            { "CARRIER", "CARRIER_NM","HOUSE_NO", "MASTER_NO","DEST_CD","DEST_NAME", "ETA", "ETD", "VOYAGE1", "VOYAGE2", "VOYAGE3", "VOYAGE4", "POD_CD", "POD_NAME", "CUT_BL_DATE",
               "POL_CD", "POL_NAME", "POR_CD", "POR_NAME", "DEST_CD", "DEST_NAME", "PORT_CD", "PORT_NM", "CUT_PORT_DATE", "PORT_DATE", "CUSTOMS_DATE", "PORT_RLS_DATE", 
                "RCV_DATE", "RLS_CNTR_DATE", "RCV_DOC_DATE", "ETD1", "ETD2", "ETD3", "ETD4", "ETA1", "ETA2","ETA3","ETA4","VESSEL1","VESSEL2","VESSEL3","VOYAGE4","SIGN_BACK","VESSEL4",
                "ATD","ATP","ATD1","ATD2","ATD3","ATD4","ATA1","ATA2","ATA3","ATA4"
            };
            list.ForEach(column => ei.Put(column, null));
            string sql = string.Format("SELECT STATUS,TORDER,U_ID FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmenId));

            DataTable smdt = OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string status=string.Empty;
            string torder=string.Empty;
            string smuid = string.Empty;
            if (smdt.Rows.Count > 0)
            {
                status = Prolink.Math.GetValueAsString(smdt.Rows[0]["STATUS"]);
                torder = Prolink.Math.GetValueAsString(smdt.Rows[0]["TORDER"]);
                smuid = Prolink.Math.GetValueAsString(smdt.Rows[0]["U_ID"]);
            }
            if (!string.IsNullOrEmpty(cancelReason) && !string.IsNullOrEmpty(status) && status == "C")
            {
                ei.Put("STATUS", "B");
                ei.Put("CORDER", "S");
            }
            else
            {
                if (IsChangStatus)
                {
                    ei.Put("CORDER", "N");
                    ei.Put("STATUS", "A");
                }
                else
                {
                    ei.Put("CORDER", "S");
                }
            }
            if (torder.Equals("S"))
                ei.Put("TORDER", "N");
            ei.PutExpress("CNT20", "PCNT20");
            ei.PutExpress("CNT40", "PCNT40");
            ei.PutExpress("CNT40HQ", "PCNT40HQ");
            ei.PutExpress("CNT_TYPE", "PCNT_TYPE");
            ei.PutExpress("CNT_NUMBER", "PCNT_NUMBER");

            ei.PutExpress("POL_CD", "PPOL_CD");
            ei.PutExpress("POR_CD", "PPOR_CD");
            ei.PutExpress("POD_CD", "PPOD_CD");
            ei.PutExpress("DEST_CD", "PDEST_CD");
            ei.PutExpress("POL_NAME", "PPOL_NAME");
            ei.PutExpress("POR_NAME", "PPOR_NAME");
            ei.PutExpress("POD_NAME", "PPOD_NAME");
            ei.PutExpress("DEST_NAME", "PDEST_NAME");
            ei.PutExpress("CBM", "PCBM");
            ei.PutExpress("GW", "PGW");
            ei.Put("SCAC_CD", "");
            if (!string.IsNullOrEmpty(cancelReason))
            {
                cancelReason = DateTime.Now.ToString("MM/dd") + ":" + cancelReason;
                sql = string.Format("SELECT INSTRUCTION FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmenId));
                string ccr = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!string.IsNullOrEmpty(ccr))
                {
                    cancelReason = ccr + "\r\n" + cancelReason;
                }
                ei.Put("INSTRUCTION", cancelReason);
            }
            ei.Put("FREIGHT_AMT", "");
            //if (!torder.Equals("N"))
            //{
            //    ei.Put("TORDER", "");
            //}
            Business.Utils.DBManager.DefaultDB.ExecuteUpdate(ei);
            if ("A".Equals(Prolink.Math.GetValueAsString(ei.Get("STATUS"))))
            {
                Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                bill.Share(smuid);
            }
        }

        public static ConfirmBookingResultInfo ConfirmBooking(VolumeBookingInfo info)
        {
            return null;
        }

        public static ConfirmBookingResultInfo ConfirmBooking(string shipmentId, string confirmUser)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            var dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return new ConfirmBookingResultInfo(ConfirmBookingStatus.DataIsNull)
            {
                Description = string.Format("Shipment ID:{0} 无此数据存在！", shipmentId)
            };
            return ConfirmBooking(dt.Rows[0], confirmUser);
        }
        public static ConfirmBookingResultInfo ConfirmBooking(DataRow smRow, string confirmUser)
        {
            ConfirmBookingResultInfo result = CheckShipmentForConfirmBooking(smRow);
            if (!result.IsSucceed) return result;
            string shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
            string uid = Prolink.Math.GetValueAsString(smRow["U_ID"]);
            string iscombine_bl = Prolink.Math.GetValueAsString(smRow["ISCOMBINE_BL"]);
            string dnno = Prolink.Math.GetValueAsString(smRow["DN_NO"]);
            string corder= Prolink.Math.GetValueAsString(smRow["CORDER"]);
            try
            {
                EditInstruct ei = CreateConfirmBookingEI(shipmentId, confirmUser, iscombine_bl, corder);
                string dnetd = Prolink.Math.GetValueAsString(smRow["ETD"]);
                if (TrackingEDI.Business.DateTimeUtils.IsDate(dnetd))
                {
                    int[] ymw = TrackingEDI.Business.DateTimeUtils.DateToYMW(dnetd);
                    if (ymw.Length >= 3)
                    {
                        ei.Put("YEAR", ymw[0]);
                        ei.Put("MONTH", ymw[1]);
                        ei.Put("WEEKLY", ymw[2]);
                    }
                }
                if (dnno.StartsWith("1090"))
                {
                    ei.Put("STATUS", "O");
                    ei.Put("BORDER", "N");
                    ei.Put("TORDER", "N");
                }
                Business.Utils.DBManager.DefaultDB.ExecuteUpdate(ei);
                result.Description = string.Format("该笔Shipment ID : {0}订舱确认成功", shipmentId);
                try
                {
                    BookingParser bp = new BookingParser();
                    bp.SaveToTrackingByShimentID(new List<string>() { shipmentId });
                }
                catch (Exception ex)
                {
                }
                if (!"C".Equals(iscombine_bl) && !"Y".Equals(iscombine_bl))
                {
                    try
                    {
                        EditInstruct AutoValuationTaskEi = new EditInstruct("AUTO_VALUATION_TASK", EditInstruct.INSERT_OPERATION);
                        AutoValuationTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
                        AutoValuationTaskEi.Put("SMU_ID", uid);
                        AutoValuationTaskEi.Put("DONE", "N");
                        AutoValuationTaskEi.Put("CREATE_BY", confirmUser);
                        AutoValuationTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                        OperationUtils.ExecuteUpdate(AutoValuationTaskEi, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                    }

                    TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status
                    {
                        ShipmentId = shipmentId,
                        StsCd = "020",
                        Sender = confirmUser,
                        Location = Prolink.Math.GetValueAsString(smRow["POL_CD"]),
                        StsDescp = "Booking Confirm"
                    });
                }
                else
                {
                    UpdateCMasterToCombineBL(smRow);
                }
                
            }
            catch (Exception ex)
            {
                return new ConfirmBookingResultInfo(ConfirmBookingStatus.Exception) { Description = ex.Message };
            }
            return result;
        }

        private static void UpdateCMasterToCombineBL(DataRow smrow){
            string master_no = Prolink.Math.GetValueAsString(smrow["MASTER_NO"]);
            string house_no = Prolink.Math.GetValueAsString(smrow["HOUSE_NO"]);
            string combin_shipment = Prolink.Math.GetValueAsString(smrow["COMBIN_SHIPMENT"]);
            string sql = string.Format("SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT={0}", SQLUtils.QuotedStr(combin_shipment));
            DataTable dt= Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            MixedList ml = new MixedList();
            foreach (DataRow dr in dt.Rows)
            {
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                ei.Put("C_MASTER_NO", master_no);
                ei.Put("C_HOUSE_NO", house_no);
                ml.Add(ei);
                ei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                ei.Put("MASTER_NO", master_no);  //因为货况追踪刚好和订舱的相反
                ei.Put("HOUSE_NO", house_no);
                ml.Add(ei);
            }
            if(ml.Count>0)
                try
                {
                    Business.Utils.DBManager.DefaultDB.ExecuteUpdate(ml);
                }catch(Exception ex){
                }
        }

        const string Status_Vaild = "V";
        const string Status_Back = "Z";
        static ConfirmBookingResultInfo CheckShipmentForConfirmBooking(DataRow smRow)
        {
            string shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
            string corder = Prolink.Math.GetValueAsString(smRow["CORDER"]);
            string status = Prolink.Math.GetValueAsString(smRow["STATUS"]);
            switch (status)
            {
                case Status_Vaild:
                    return new ConfirmBookingResultInfo(ConfirmBookingStatus.UnBooking)
                    {
                        Description = string.Format("该笔Shipment ID:{0} 已经作废！", shipmentId)
                    };
                case Status_Back: return new ConfirmBookingResultInfo(ConfirmBookingStatus.UnBooking)
                {
                    Description = string.Format("该笔Shipment ID:{0} 已经退运！", shipmentId)
                };
            }
            string tranType = Prolink.Math.GetValueAsString(smRow["TRAN_TYPE"]);
            string region = Prolink.Math.GetValueAsString(smRow["REGION"]);
            if ("F".Equals(tranType))
            {
                DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", smRow["GROUP_ID"].ToString(), smRow["CMP"].ToString());
                string crcode = Prolink.Math.GetValueAsString(smRow["CARRIER"]);
                string crname = TrackingEDI.Mail.MailTemplate.GetBaseCodeValueEmpty(baseDt, "TCAR", crcode);
                if (string.IsNullOrEmpty(crname)) return new ConfirmBookingResultInfo(ConfirmBookingStatus.Confirmed)
                {
                    Description = string.Format("该笔Shipment ID:{0} 的Carrier:{1}和系统的船公司建档不匹配！", shipmentId, crcode)
                };
            }
            //if (corder == "C") return new ConfirmBookingResultInfo(ConfirmBookingStatus.Confirmed)
            //{
            //    Description = string.Format("该笔Shipment ID:{0} 已经订舱确认！", shipmentId)
            //};
            Func<string, ConfirmBookingResultInfo> getValidateResult = display =>
                {
                    return new ConfirmBookingResultInfo(ConfirmBookingStatus.ValidateNotMatch)
                    {
                        Description = string.Format("订舱确认失败! 请输入:{0}", display)
                    };
                };
            Func<string, string, ConfirmBookingResultInfo> checkColumnResult = (column, display) =>
                {
                    string v = Prolink.Math.GetValueAsString(smRow[column]);
                    if (string.IsNullOrEmpty(v)) return getValidateResult(display);
                    return null;
                };

            foreach (var item in GetConfirmBookingCheckColumns(tranType, region))
            {
                ConfirmBookingResultInfo result = checkColumnResult(item.Item1, item.Item2);
                if (result != null) return result;
            }
            switch (tranType)
            {
                case "L":
                case "F":
                    try
                    {
                        BookingStatusManager.CheckShipment(shipmentId);
                    }
                    catch (Exception ex)
                    {
                        var l = Business.Utils.Context.Logger.CreateLog("F/L订舱确认验证异常", "CheckShipmentForConfirmBooking", shipmentId, "", ex.ToString());
                        Business.Utils.Context.Logger.WriteLog(l);
                    }
                    break;
            }
            return new ConfirmBookingResultInfo(ConfirmBookingStatus.Succeed) { IsSucceed = true };
        }
        static IEnumerable<Tuple<string, string>> GetConfirmBookingCheckColumns(string tranType,string region)
        {
            yield return new Tuple<string, string>("ETD", "ETD");
            yield return new Tuple<string, string>("ETA", "ETA");
            switch (tranType)
            {
                case "L":
                    yield return new Tuple<string, string>("PORT_DATE", "截进仓时间");
                    yield return new Tuple<string, string>("VESSEL1", "船名1");
                    yield return new Tuple<string, string>("VOYAGE1", "船期1");
                    yield return new Tuple<string, string>("MASTER_NO", "Master NO");
                    if ("NA".Equals(region) || "SA".Equals(region) || "EU".Equals(region))
                    {
                        yield return new Tuple<string, string>("SCAC_CD", "SCAC Code（因为目的地区域为:EU\\NA\\SA）");
                    }
                    yield break;
                case "F":
                    yield return new Tuple<string, string>("CUT_PORT_DATE", "截进港时间");
                    yield return new Tuple<string, string>("RCV_DATE", "截海外申报时间");
                    yield return new Tuple<string, string>("CUSTOMS_DATE", "海关截单日期");
                    yield return new Tuple<string, string>("PORT_RLS_DATE", "码头截放行时间");
                    yield return new Tuple<string, string>("VESSEL1", "船名1");
                    yield return new Tuple<string, string>("VOYAGE1", "船期1");
                    yield return new Tuple<string, string>("MASTER_NO", "Master NO");
                    if ("NA".Equals(region) || "SA".Equals(region) || "EU".Equals(region))
                    {
                        yield return new Tuple<string, string>("SCAC_CD", "SCAC Code（因为目的地区域为:EU\\NA\\SA）");
                    }
                    yield break;
                case "A":
                    yield return new Tuple<string, string>("MASTER_NO", "Master NO");
                    yield return new Tuple<string, string>("HOUSE_NO", "House NO");
                    yield return new Tuple<string, string>("VESSEL1", "航班");
                    yield break;
                case "D":
                case "E":
                    yield return new Tuple<string, string>("HOUSE_NO", "快递单号");
                    yield break;
                case "T":
                    yield return new Tuple<string, string>("HOUSE_NO", "运输号码");
                    yield break;
                case "R":
                    yield return new Tuple<string, string>("HOUSE_NO", "House B/L");
                    yield return new Tuple<string, string>("VESSEL1", "车次 ");
                    yield break;
            }
        }
        static EditInstruct CreateConfirmBookingEI(string shipmentId, string confirmUser, string iscombine_bl,string corder)
        {
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            ei.Put("MODIFY_BY", confirmUser);
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            ei.PutDate("RLS_CNTR_DATE", DateTime.Now);
            //ei.Put("STATUS", "C");
            ei.Put("CORDER", "C");
            string status = "A";
            string torder="N", border="N";
            bool isfimok = BookingStatusManager.IsApproveByFIM(shipmentId, ref border, ref torder, ref status);
            if ("A".Equals(status)||"B".Equals(status) || string.IsNullOrEmpty(status))
                ei.Put("STATUS", "C");
            if (isfimok)
            {
                if (torder == "N" || string.IsNullOrEmpty(torder))
                    ei.Put("TORDER", "S");
                if (border == "N" || string.IsNullOrEmpty(border))
                    ei.Put("BORDER", "M");
            }
            if ("C".Equals(iscombine_bl) || "Y".Equals(iscombine_bl))
            {
                ei.Put("SORDER", "C");  //更改合并提单通知状态为C表示已经回写
                ei.Put("CORDER", "Y");  //更改合并提单通知状态为C表示已经回写
                ei.Put("STATUS", "Y");  //将订舱主状态回写成合并提单确认
            }
            if ("P".Equals(corder) || "Q".Equals(corder))
            {
                ei.Put("CORDER", "Q"); 
            }
            return ei;
        }

        public static bool CheckTorder(string shipmentId)
        {
            string sql = string.Format("SELECT DN_NO,COMBINE_INFO FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return false;
            List<string> dnList = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                string dnno = Prolink.Math.GetValueAsString(row["DN_NO"]);
                if (string.IsNullOrEmpty(dnno)) continue;
                if (dnList.Contains(dnno)) continue;
                dnList.Add(dnno);
            }
            return CheckTorder(dnList);
        }

        static bool CheckTorder(IEnumerable<string> dnList)
        {
            if (dnList.Count() <= 0)
                return false;

            string sql = string.Format(@"SELECT AR.STATUS FROM APPROVE_RECORD AR,SMDN SN 
                            WHERE AR.REF_NO=SN.DN_NO AND AR.APPROVE_CODE=SN.APPROVE_TYPE AND AR.ROLE='FIM'
                            AND SN.DN_NO IN ({0})", string.Join(",", dnList.Select(s => SQLUtils.QuotedStr(s))));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count != dnList.Count()) return false;
            bool isOK = false;
            foreach (DataRow aprow in dt.Rows)
            {
                isOK = true;
                string status = Prolink.Math.GetValueAsString(aprow["STATUS"]);
                if (status != "1")
                {
                    isOK = false;
                    break;
                }
            }
            if (isOK)
            {
                return true;
            }
            return false;
        }

        public static bool CheckTorder(string dnno, string shipmentid, string combineinfo, ref string torder,ref string border)
        {
            if (string.IsNullOrEmpty(shipmentid)) return false;
            string sql = string.Format("SELECT CORDER,TORDER,BORDER FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return false;
            DataRow dr = dt.Rows[0];
            string corder = Prolink.Math.GetValueAsString(dr["CORDER"]);
            torder = Prolink.Math.GetValueAsString(dr["TORDER"]);
            border = Prolink.Math.GetValueAsString(dr["BORDER"]);
            if (corder != "C" ) return false;
            if (string.IsNullOrEmpty(combineinfo)) return true;
            string[] dnnos = combineinfo.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> dnlist = dnnos.ToList();
            dnlist.Remove(dnno);
            dnnos = dnlist.ToArray();
            if (dnnos.Length <= 0) return true;
            return CheckTorder(dnnos);
        }

        public static DataTable QueryPartyDT(IEnumerable<string> partyCodes)
        {
            List<string> list = partyCodes.ToList();
            string sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO IN({0})", list.Count <= 0 ? "''" : string.Join(",",
               list.Select(p => SQLUtils.QuotedStr(p))));
            return Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
        }
        public static DataRow QueryPartyRow(string partyNO, DataTable partyDT)
        {
            if (string.IsNullOrEmpty(partyNO)) return null;
            if (partyDT == null) return null;
            DataRow[] rows = partyDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(partyNO)));
            if (rows == null || rows.Length <= 0) return null;
            return rows[0];
        }

        public static DataTable QueryPartyType()
        {
            string sql = "SELECT CD_TYPE,CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT'";
            return Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
        }

        public static DataRow QuerySM(string shipmentId)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }
        public static DataTable QuerySM(IEnumerable<string> shipmentIdList)
        {
            if(shipmentIdList==null) return null;
            List<string> list = shipmentIdList.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            if (list == null || list.Count <= 0) return null;
            string sql = string.Format("SELECT * FROM SMSM WHERE {0}", string.Join(" OR ", list.Select(s => string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(s)))));
            return Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });       
        }

        public static EditInstruct CreateDeclarationUpEi(Business.TPV.Standard.DeclarationInfo info)
        {
            string status = string.Empty;
            if (!CheckCanUpSM(info.ShipmentID,ref status)) return null;
            if (info.Items == null || info.Items.Length <= 0) return null;
            
            List<string> decllist = new List<string>();
            int nextnum=0;
            foreach (Business.TPV.Standard.DeclarationItem declitem in info.Items)
            {
                if (string.IsNullOrEmpty(declitem.DeclarationNumber)) continue;
                if (!decllist.Contains(declitem.DeclarationNumber))
                    decllist.Add(declitem.DeclarationNumber);
                nextnum+=Prolink.Math.GetValueAsInt(declitem.NextNum);
            }
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            var item = info.Items.First();
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(info.ShipmentID));
            ei.Put("APPROVE_NO", item.ApprovalNumber);
            if (item.DeclarationDate.HasValue)
                ei.PutDate("DECL_DATE", item.DeclarationDate.Value);
            if (item.DeclarationReleaseDate.HasValue)
                ei.PutDate("DECL_RLS_DATE", item.DeclarationReleaseDate.Value);
            ei.Put("EDECL_NO", item.DeclarationNumber);
             if (decllist.Count > 0)
            {
                ei.Put("DECL_NUM", decllist.Count());   //报单数
                ei.Put("CONT_DECL_NUM", (decllist.Count() - 1).ToString());   //续单数
            }
            ei.Put("NEXT_NUM", nextnum.ToString());
            if (item.DeclarationReleaseDate.HasValue)
            {
                ei.Put("BORDER", "H");
                if ("O".Equals(status))
                    ei.Put("STATUS", "H");
            }
            else
                ei.Put("BORDER", "C");
            return ei;
        }

        static bool CheckCanUpSM(string shipmentId,ref string status)
        {
            DataRow row = QuerySM(shipmentId);
            if (row == null) return false;
            string border = Prolink.Math.GetValueAsString(row["BORDER"]);
            status = Prolink.Math.GetValueAsString(row["STATUS"]);
            switch (border)
            {
                case "H": return false;
                default: return true;
            }
        }

        public static EditInstruct CreateCancelDeclaration(Business.TPV.Standard.DeclarationInfo info)
        {
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(info.ShipmentID));
            ei.Put("APPROVE_NO", null);
            ei.Put("DECL_DATE", null);
            ei.Put("DECL_RLS_DATE", null);
            ei.Put("EDECL_NO", null);
            ei.Put("BORDER", "N");
            ei.Put("STATUS", "O");
            return ei;
        }

        public static SIProfileInfo QuerySIProfileInfo(string profileCode, SI.DataGetter dataGetter = null, string dnLevel = "")
        {
            if (dataGetter == null)
                dataGetter = new SI.DataGetter();
            return dataGetter.QuerySIProfileInfo(profileCode, dnLevel);
        }

        public static PartnerInfo CreatePartnerInfo(string partyType, DataRow row)
        {
            if (row == null) return null;
            return new PartnerInfo
            {
                PartnerNumber = Prolink.Math.GetValueAsString(row["PARTY_NO"]),
                Name1 = Prolink.Math.GetValueAsString(row["PARTY_NAME"]),
                Name2 = Prolink.Math.GetValueAsString(row["PARTY_NAME2"]),
                Name3 = Prolink.Math.GetValueAsString(row["PARTY_NAME3"]),
                Name4 = Prolink.Math.GetValueAsString(row["PARTY_NAME4"]),
                Street = Prolink.Math.GetValueAsString(row["PART_ADDR1"]),
                Street2 = Prolink.Math.GetValueAsString(row["PART_ADDR2"]),
                Street3 = Prolink.Math.GetValueAsString(row["PART_ADDR3"]),
                Street4 = Prolink.Math.GetValueAsString(row["PART_ADDR4"]),
                Street5 = Prolink.Math.GetValueAsString(row["PART_ADDR5"]),
                Fax = Prolink.Math.GetValueAsString(row["PARTY_FAX"]),
                CountryKey = Prolink.Math.GetValueAsString(row["CNTY"]),
                CountryName = Prolink.Math.GetValueAsString(row["CNTY_NM"]),
                CityCode = Prolink.Math.GetValueAsString(row["CITY"]),
                City = Prolink.Math.GetValueAsString(row["CITY_NM"]),
                Region = Prolink.Math.GetValueAsString(row["STATE"]),
                PostalCode = Prolink.Math.GetValueAsString(row["ZIP"]),
                FirstName = Prolink.Math.GetValueAsString(row["PARTY_ATTN"]),
                EMail = Prolink.Math.GetValueAsString(row["PARTY_MAIL"]),
                Telephone1 = Prolink.Math.GetValueAsString(row["PARTY_TEL"]),
                VATNumber = Prolink.Math.GetValueAsString(row["TAX_NO"]),
                PartnerFunction = partyType
            };
        }

        public static DataTable QueryMultiSegmentsDNInfo(string dnNo, List<string> columns = null)
        {
            List<string> list = new List<string> { "DN_NO_CMP_REF" };
            list.AddRange(columns);
            string sql = string.Format(@"WITH DN AS (SELECT {0} FROM SMDN WHERE DN_NO ={1} UNION ALL SELECT {2} FROM SMDN INNER JOIN DN ON DN.DN_NO_CMP_REF = SMDN.DN_NO) 
                    SELECT * FROM DN", string.Join(",", list), SQLUtils.QuotedStr(dnNo), string.Join(",", list.Select(c => string.Format("SMDN.{0}", c))));
            return Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
        }

        public static EDocInfo CreateShipmentEDocInfo(DataRow row)
        {
            return new EDocInfo
            {
                JobNo = Prolink.Math.GetValueAsString(row["U_ID"]),
                GroupId = Prolink.Math.GetValueAsString(row["GROUP_ID"]),
                Cmp = Prolink.Math.GetValueAsString(row["CMP"]),
                Stn = "*",
                UserId = Prolink.Math.GetValueAsString(row["CREATE_BY"]),
                Dep = null
            };
        }

        public static void SaveIPLog(HttpRequestBase request, string user,string cmp, string data_path = "",string pass="")
        {
            if (request == null)
                return;
            try
            {
                if (string.IsNullOrEmpty(data_path))
                    data_path = request.Url.ToString();

                EditInstruct ei = new EditInstruct("IP_LOG", EditInstruct.INSERT_OPERATION);
                ei.Put("DATA_PATH", data_path);
                ei.Put("IP", request.UserHostAddress);
                ei.Put("KEY_WORD", pass);
                ei.Put("CMP", cmp);
                ei.Put("PC_NAME", request.UserHostName);
                ei.Put("CREATE_BY", user);
                ei.PutDate("CREATE_DATE", DateTime.Now);

                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
        }

        public static EditInstruct CreateEDIEi(EdiInfo ediInfo, EditInstructList el = null)
        {
            EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", ediInfo.ID);
            ei.Put("EDI_ID", ediInfo.EdiId);
            ei.Put("SENDER", ediInfo.CreateBy);
            ei.Put("RS", ediInfo.Rs);
            ei.Put("FROM_CD", ediInfo.FromCd);
            ei.Put("TO_CD", ediInfo.ToCd);
            ei.Put("DATA_FOLDER", ediInfo.DataFolder);
            ei.Put("REF_NO", ediInfo.RefNO);
            ei.Put("GROUP_ID", ediInfo.GroupId);
            ei.Put("CMP", ediInfo.Cmp);
            ei.Put("STN", ediInfo.Stn);
            string edidata = Prolink.Math.GetValueAsString(ediInfo.Data);
            if (!string.IsNullOrEmpty(edidata) && el != null)
            {
                EditInstruct edidataei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                edidataei.Put("U_ID", ediInfo.ID);
                edidataei.Put("EDI_DATE", edidata);
                edidataei.PutExpress("CREATE_DATE", "getdate()");
                el.Add(edidataei);
            }

            if (!string.IsNullOrEmpty(ediInfo.Remark) && ediInfo.Remark.Length > 500)
                ediInfo.Remark = ediInfo.Remark.Substring(0, 500);
            ei.Put("REMARK", ediInfo.Remark);
            ei.PutExpress("EVENT_DATE", "getdate()");
            ei.Put("STATUS", ediInfo.Status);
            return ei;
        }

        public static ResultInfo SendASNDateInfo(DataRow dnrow, bool sameConnection = true, bool MmdFlag = false)
        {
            string shipmentid = Prolink.Math.GetValueAsString(dnrow["SHIPMENT_ID"]);
            string invNo = Prolink.Math.GetValueAsString(dnrow["INV_NO"]);
            string asnNo = Prolink.Math.GetValueAsString(dnrow["ASN_NO"]);
            string cmp = Prolink.Math.GetValueAsString(dnrow["CMP"]);

            ASNDateInfo asninfo = new ASNDateInfo();
            asninfo.ShipmentId = shipmentid;
            asninfo.InvNo = invNo;
            asninfo.AsnNo = asnNo;
            asninfo.GroupId = "TPV";
            asninfo.CMP = cmp;
            asninfo.ASN_Date = Prolink.Math.GetValueAsDateTime(dnrow["ASN_DATE"]);
            if (MmdFlag)
            {
                asninfo.OriginalAsnDate = Prolink.Math.GetValueAsDateTime(dnrow["OLD_ASN_DATE"]);
                asninfo.WorkDate = getWorkDays(Prolink.Math.GetValueAsDateTime(dnrow["ASN_DATE"]), Prolink.Math.GetValueAsDateTime(dnrow["OLD_ASN_DATE"]));
                asninfo.CntrNo = Prolink.Math.GetValueAsString(dnrow["CNTR_NO"]);
                asninfo.Bu = Prolink.Math.GetValueAsString(dnrow["BU"]);
                asninfo.TranType = Prolink.Math.GetValueAsString(dnrow["TRAN_TYPE"]);
            }
            ASNDateManager aSNDateManager = new ASNDateManager();
            if (!sameConnection)
            {
                aSNDateManager.DsiposeDestination();
            }
            return aSNDateManager.TryPostASNDateInfo(asninfo,cmp);
        }
        /// <summary>
        /// 工作差天数
        /// </summary>
        /// <param name="dt1">更新日期</param>
        /// <param name="dt2">原始日期</param>
        /// <returns></returns>
        public static int getWorkDays(DateTime dt1, DateTime dt2)
        {
            int countday;
            int weekday = 0;
            bool addFlag = true;

            dt1 = new DateTime(dt1.Year, dt1.Month, dt1.Day);
            dt2 = new DateTime(dt2.Year, dt2.Month, dt2.Day);

            if (dt1 == DateTime.MinValue || dt2 == DateTime.MinValue)
            {
                return weekday;
            }
            if (dt1.Subtract(dt2).Days >= 0)
            {
                countday = dt1.Subtract(dt2).Days;
            }
            else
            {
                countday = dt2.Subtract(dt1).Days;
                addFlag = false;
            }

            for (int i = 1; i <= countday; i++)
            {
                DateTime tempdt = addFlag ? dt2.Date.AddDays(i) : dt2.Date.AddDays(-i);
                if (tempdt.DayOfWeek != System.DayOfWeek.Saturday && tempdt.DayOfWeek != System.DayOfWeek.Sunday)
                {
                    weekday++;
                }
            }
            return addFlag ? weekday : -weekday;
        }

        public static void WriteEdiLog(EdiInfo ediInfo, MixedList ml)
        {
            if (ml == null)
                ml = new MixedList();
            EditInstructList el = new EditInstructList();
            ml.Add(CreateEDIEi(ediInfo, el));
            for (int i = 0; i < el.Count; i++)
            {
                ml.Add(el[i]);
            }

            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {

            }
        }
    }

    public class ConfirmBookingResultInfo : ResultInfo
    {
        public ConfirmBookingResultInfo()
        {

        }
        public ConfirmBookingResultInfo(ConfirmBookingStatus status)
        {
            Status = status;
            ResultCode = status.ToString();
        }
        public ConfirmBookingStatus Status { get; private set; }
    }

    public class VolumeBookingInfo
    {
        public IEnumerable<string> ShipmentIDList { get; set; }
        public string ConfirmUser { get; set; }
    }

    public enum ConfirmBookingStatus { None, Succeed,UnBooking, Confirmed, DataIsNull, ValidateNotMatch, Exception }

    public class SIProfileInfo
    {
        public Dictionary<string, PartnerInfo> Parties { get; set; }
        public string ACRemark { get; set; }
        public string Goods { get; set; }
        public string ShippingMark { get; set; }
        public string InvoiceIntruction { get; set; }
        public string PackingIntruction { get; set; }
        public string BLIntruction { get; set; }
        public string InvoiceRemark { get; set; }
        public string PackingRemark { get; set; }
        public string BLRemark { get; set; }
        public string Vendor { get; set; }
        public string ISFSellerCode { get; set; }
    }
}