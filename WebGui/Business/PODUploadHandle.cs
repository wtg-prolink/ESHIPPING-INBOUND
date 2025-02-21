using Business;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;

namespace Business
{
    public class PODUploadHandle
    {
        public const string CallType_ByDN = "D";
        public const string CallType_ByContainer = "C";

        public static bool CallTypeIsByDn(string calltype)
        {
            if (calltype == CallType_ByDN)
                return true;
            return false;
        }
        public static void UploadPODHandle(string EdocjobNo, string reserve_no, string wh, string call_type, string companyid)
        {
            DateTime podDate = DateTime.Now;
            MixedList ml = new MixedList();
            DateTime odt = podDate;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, companyid);
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("RV_TYPE", "I");
            ei.Put("POD_CHECK", "Y");
            ei.PutDate("POD_UPDATE_DATE", ndt);
            if (InboundHandel.checkPODstatus(reserve_no, EdocjobNo))
                ei.Put("STATUS", "U");
            ml.Add(ei);

            UpdateSMORDStatusAtPod( ml, ei, reserve_no, wh, call_type);

            string table = CallTypeIsByDn(call_type) ? "SMRDN" : "SMRCNTR";
            ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("WS_CD", wh);
            ei.PutDate("POD_MDATE", podDate);
            ei.PutDate("POD_MDATE_L", ndt);
            ei.Put("POD_CHECK", "Y");
            ml.Add(ei);
            string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
            string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
            string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.POD_DATE={0}, SMICNTR.POD_DATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            if (CallTypeIsByDn(call_type))
            {
                updatesql = string.Format("UPDATE SMIDN SET SMIDN.POD_DATE={0}, SMIDN.POD_DATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
               SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            }
            ml.Add(updatesql);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            UpdateInboundSMSMStatus(reserve_no, "I", "RV", "U");
        }

        /// <summary>
        /// 當一個Shipment中所有預約單有1或多個在進廠且其他都在靠月台orPODor出廠時，Shipment的狀態應該要更新成進廠
        /// 當一個Shipment中所有預約單有1個或多個是POD且其他都是出廠時，Shipment的狀態更新成POD
        /// 當一個Shipment中所有預約單所有都出廠，Shpment的狀態會更新成Finish
        /// 更新inbound状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rv_type"></param>
        public static void UpdateInboundSMSMStatus(string id, string rv_type, string type = "", string newStatus = "")
        {
            MixedList ml = new MixedList();
            if (!"I".Equals(rv_type))
                return;

            List<string> smlist = GetSmrvShipmentList(id, type);
            DataTable dt = GetInboundSmrvData(smlist);
            string trantype = string.Empty;
            if (dt.Rows.Count > 0)
            {
                trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            }
                
            foreach (string sm_id in smlist)
            {
                if (string.IsNullOrEmpty(sm_id))
                    continue;
                string status = SetStatus(newStatus, dt, sm_id);
                if (dt.Rows.Count > 0)
                {
                    if ("T".Equals(trantype))
                    {
                        string reserve_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RESERVE_NO"]);
                        status = UpdateSMRIVAndSMORDStatus(ml, sm_id, reserve_no);
                    }
                }

                if (!string.IsNullOrEmpty(status))
                {
                    UpdateSMSMIStatus(ml, sm_id, status);
                }
            }

            #region 更新运输单
            if (!string.IsNullOrEmpty(newStatus))
            {
                List<string> reservenolist = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    string reserve_no = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                    if (!string.IsNullOrEmpty(reserve_no) && !reservenolist.Contains(reserve_no))
                        reservenolist.Add(reserve_no);
                }

                DataTable smrvDetail = GetSMRVDetail(reservenolist);
                List<string> ordList = new List<string>();
                foreach (DataRow smrvD in smrvDetail.Rows)
                {
                    string ord_no = Prolink.Math.GetValueAsString(smrvD["ORD_NO"]);
                    string ReserveNo = Prolink.Math.GetValueAsString(smrvD["RESERVE_NO"]);
                    string Status = Prolink.Math.GetValueAsString(smrvD["SMRVSTATUS"]);
                    if (string.IsNullOrEmpty(ord_no) || ordList.Contains(ord_no))
                        continue;

                    EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("ORD_NO", ord_no);

                    string finalWh = Prolink.Math.GetValueAsString(smrvD["FINAL_WH"]);
                    string outerFalg = Prolink.Math.GetValueAsString(smrvD["OUTER_FLAG"]);

                    if ("Y".Equals(outerFalg) && "Final".Equals(finalWh))
                    {
                        if("F"!=trantype && "R" != trantype)
                        {
                            foreach (string shipmentid in smlist)
                            {
                                string returnstatus = UpdateSMRIVAndSMORDStatus(ml, shipmentid, ReserveNo);
                                UpdateSMSMIStatus(ml, shipmentid, returnstatus);
                            }
                        }
                        continue;
                    }

                    if ("Final".Equals(finalWh))
                    {
                        switch (newStatus)
                        {
                            case "I":
                            case "O":
                            case "G":
                            case "Z":
                                ei.Put("CSTATUS", newStatus);
                                break;
                            case "U":
                                if ("U".Equals(Status))
                                    ei.Put("CSTATUS", "U");
                                break;
                            default:
                                continue;
                        }
                    }
                    else if ("Final".Equals(finalWh))
                    {
                        switch (newStatus)
                        {
                            case "I":
                                ei.Put("CSTATUS", newStatus);
                                break;
                            case "U":
                                if ("U".Equals(Status))
                                    ei.Put("CSTATUS", "U");
                                break;
                            case "O":
                            case "Z":
                                ei.Put("CSTATUS", "T");
                                break;
                            default:
                                continue;
                        }
                    }
                    ordList.Add(ord_no);
                    ml.Add(ei);
                }
            }
            #endregion

            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (!"BL".Equals(type))
            {
                string filter = "1=0";
                if (smlist.Count > 0)
                {
                    filter = string.Format(" SHIPMENT_ID IN {0}",SQLUtils.Quoted(smlist.ToArray()));
                }

                DataTable smsmiDt = OperationUtils.GetDataTable("SELECT U_ID,O_UID FROM SMSMI WITH(NOLOCK) WHERE " + filter, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in smsmiDt.Rows)
                {
                    TrackingEDI.InboundBusiness.SMSMIHelper.InboundsetLight(Prolink.Math.GetValueAsString(dr["U_ID"]), Prolink.Math.GetValueAsString(dr["O_UID"]), "I");
                }
            }
        }

        private static string UpdateSMRIVAndSMORDStatus(MixedList ml, string sm_id, string reserve_no)
        {
            string status = "Z";
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.Put("STATUS", status);
            ml.Add(ei);

            EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            smordei.PutKey("SHIPMENT_ID", sm_id);
            smordei.Put("CSTATUS", status);
            ml.Add(smordei);
            return status;
        }
        private static void UpdateSMSMIStatus(MixedList ml, string sm_id, string status)
        {
            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", sm_id);
            ei.Put("STATUS", status);
            ml.Add(ei);
        }


        private static string SetStatus(string newStatus, DataTable dt, string sm_id)
        {
            string status = string.Empty;

            string baseconditions = string.Format("SHIPMENT_INFO LIKE {0}", SQLUtils.QuotedStr("%" + sm_id + "%"));

            if (dt.Select(baseconditions).Length == dt.Select(string.Format("{0} AND STATUS IN ('O','Z')", baseconditions)).Length)
            {
                status = "O";
                if (newStatus == "Z")
                    status = "Z";
            } 

            if (string.IsNullOrEmpty(status) && dt.Select(string.Format("{0} AND STATUS='U'", baseconditions)).Length > 0)
            {
                if (dt.Select(string.Format("{0} AND (STATUS NOT IN ('U','O') OR STATUS IS NULL)", baseconditions)).Length <= 0)
                    status = "P";
            }

            if (string.IsNullOrEmpty(status) && dt.Select(string.Format("{0} AND STATUS='I'", baseconditions)).Length > 0)
            {
                if (dt.Select(string.Format("{0} AND (STATUS NOT IN ('I','G','O','U','Z',) OR STATUS IS NULL)", baseconditions)).Length <= 0)
                    status = "G";
            }

            return status;
        }

        private static DataTable GetInboundSmrvData(List<string> smlist)
        {
            string sql = string.Empty;
            foreach (string sm_id in smlist)
            {
                if (sql.Length > 0)
                    sql += " UNION ";
                sql += string.Format("SELECT RESERVE_NO,SHIPMENT_INFO,SHIPMENT_ID,STATUS,(select tran_type from smsmi where smsmi.shipment_id={0}) AS TRAN_TYPE FROM SMIRV WITH(NOLOCK) WHERE SHIPMENT_INFO LIKE {1} AND RV_TYPE='I' AND STATUS<>'V'", SQLUtils.QuotedStr(sm_id), SQLUtils.QuotedStr("%" + sm_id + "%"));
            }

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static List<string> GetSmrvShipmentList(string id, string type )
        {
            List<string> smlist = new List<string>();
            if ("BL".Equals(type))
            {
                DataTable smrv = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID FROM TKBL WITH(NOLOCK) WHERE U_ID={0}", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in smrv.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (!string.IsNullOrEmpty(sm_id) && !smlist.Contains(sm_id))
                        smlist.Add(sm_id);
                }
            }
            else
            {
                DataTable smrv = null;
                if ("RV".Equals(type))
                    smrv = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,SHIPMENT_INFO FROM SMIRV WITH(NOLOCK) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                else
                    smrv = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,SHIPMENT_INFO FROM SMIRV WITH(NOLOCK) WHERE U_ID={0}", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in smrv.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    string sm_info = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                    string[] sms = sm_info.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!string.IsNullOrEmpty(sm_id) && !smlist.Contains(sm_id))
                        smlist.Add(sm_id);
                    foreach (string str in sms)
                    {
                        if (!string.IsNullOrEmpty(str) && !smlist.Contains(str))
                            smlist.Add(str);
                    }
                }
            }
            return smlist;
        }



        /// <summary>
        ///  上传POD时更新运输单状态
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="rvei"></param>
        /// <param name="reserve_no"></param>
        /// <param name="warehouse"></param>
        /// <param name="calltype"></param>
        public static void UpdateSMORDStatusAtPod(MixedList ml, EditInstruct rvei, string reserve_no,string warehouse,string calltype)
        {
            DataTable smrvDetail = GetSMRVDetail(warehouse, reserve_no, calltype);
            if (smrvDetail == null|| smrvDetail.Rows.Count<=0)
                return;
            List<string> ordList = new List<string>();
            DataRow[] drs = string.IsNullOrEmpty(reserve_no) ? smrvDetail.Select() : smrvDetail.Select(string.Format("RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
            foreach (DataRow ord in drs)
            {
                string ord_no = Prolink.Math.GetValueAsString(ord["ORD_NO"]);
                if (ordList.Contains(ord_no))
                    continue;
                EditInstruct ordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                ordei.PutKey("ORD_NO", ord_no);
                if ("Final".Equals(Prolink.Math.GetValueAsString(ord["FINAL_WH"])) && "Y".Equals(Prolink.Math.GetValueAsString(ord["OUTER_FLAG"])))
                {
                    //2-1-2. 當預約單明細的Delivery Address是外倉(檢查卡車送貨點建檔中的常用地址中的Third Party WH=Y)，可以不用Gate In(不用判斷IGATE是否有料)，上傳POD時預約單狀態狀態直接切換到出廠(O)，且運輸訂單狀態寫成Finished(O)
                    rvei.Put("STATUS", "O");
                    ordei.Put("CSTATUS", "O");
                    ordList.Add(ord_no);
                    ml.Add(ordei);
                }
                else if ("Temp".Equals(Prolink.Math.GetValueAsString(ord["FINAL_WH"])) && "Y".Equals(Prolink.Math.GetValueAsString(ord["OUTER_FLAG"])))
                {
                    //2-2-2. 當預約單明細的Delivery Address是外倉時(在常用地址中的Thrid Party WH=Y判斷)，預約單上傳POD時，預約單狀態寫成出廠(O)，要把運輸訂單狀態寫On the way(SMORD.CSTATUS=T)，Shipment的狀態不用管"
                    rvei.Put("STATUS", "O");
                    ordei.Put("CSTATUS", "T");
                    ordList.Add(ord_no);
                    ml.Add(ordei);
                }
            }
        }


        private static DataTable GetSMRVDetail(List<string> reservenolist)
        {
            string filter = "'@@@@@@@@@@@@@@@'";
            if (reservenolist.Count > 0)
            {
                filter = SQLUtils.Quoted(reservenolist.ToArray());
            }

            string gateOutSQL = string.Format("SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD,SMRVSTATUS FROM (SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP,(SELECT TOP 1 STATUS FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO) AS SMRVSTATUS FROM SMRCNTR WITH (NOLOCK) WHERE SMRCNTR.RESERVE_NO IN {0})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", filter);
            gateOutSQL += string.Format(" UNION SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD,SMRVSTATUS FROM (SELECT ORD_NO,IDATE,'' AS CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP,(SELECT TOP 1 STATUS FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRDN.RESERVE_NO) AS SMRVSTATUS FROM SMRDN WITH (NOLOCK) WHERE SMRDN.RESERVE_NO IN {0})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", filter);

            DataTable dt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static DataTable GetSMRVDetail(string wh, string reserveNo, string callType)
        {
            string gateOutSQL = string.Format("SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD FROM (SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP FROM SMRCNTR WITH (NOLOCK) WHERE WS_CD={0} AND SMRCNTR.RESERVE_NO={1})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", SQLUtils.QuotedStr(wh), SQLUtils.QuotedStr(reserveNo));
            if (CallTypeIsByDn(callType))
                gateOutSQL = string.Format("SELECT ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD FROM (SELECT ORD_NO,IDATE,'' AS CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,CMP FROM SMRDN WITH (NOLOCK) WHERE WS_CD={0} AND SMRDN.RESERVE_NO={1})A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", SQLUtils.QuotedStr(wh), SQLUtils.QuotedStr(reserveNo));

            DataTable dt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
    }
}