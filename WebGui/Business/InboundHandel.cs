using Business.TPV;
using Business.TPV.Financial;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Mail;

namespace Business
{
    public class InboundHandel
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

        public static string SetConfirm(UserInfo userinfo, string suid, DataTable smdt = null)
        { 
            if (smdt == null) 
                smdt = GetSMByUid(suid); 
            string msg = TrackingEDI.InboundBusiness.BookingConfirm.SetConfirm(smdt, userinfo.UserId, userinfo.CompanyId);
            if (string.IsNullOrEmpty(msg))
            {
                smdt = GetSMByUid(suid);
                msg = AfterLSPConfirmDoDecl(smdt, userinfo);
                DoDeclaration(smdt, userinfo);
            }
            return msg;
        } 

        private static void DoDeclaration(DataTable smdt, UserInfo userinfo)
        {
            if (smdt == null || smdt.Rows.Count <= 0) return;
            DataRow dr = smdt.Rows[0];
            string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
            string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
            string masterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
            string houseNo = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
            string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string delist = string.Empty;
            switch (tranType)
            {
                case "F":
                case "L":
                case "A":
                case "R":
                    delist = string.IsNullOrEmpty(houseNo) ? masterNo : houseNo;
                    break;
                case "E":
                    delist = houseNo;
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(delist))
            {
                CreateDeclaration(uid, shipmentId, userinfo, delist);
            }
        }

        /// <summary>
        /// DECLARATION Task
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="delist"></param>
        public static void CreateDeclaration(string uId, string shipmentId, UserInfo userinfo, string delist,string edoctype= "DO", MixedList ml=null)
        {
            if (string.IsNullOrEmpty(delist)) return;
            EditInstruct decEi = new EditInstruct("DECLARATION_TASK", EditInstruct.INSERT_OPERATION);
            decEi.Put("NEW_ID", Guid.NewGuid().ToString());
            decEi.Put("U_ID", uId);
            decEi.Put("GROUP_ID", userinfo.GroupId);
            decEi.Put("SHIPMENT_ID", shipmentId);
            decEi.Put("DEC_INFO", delist);
            decEi.Put("CMP", userinfo.CompanyId);
            decEi.Put("SUCCESS", "N");
            decEi.Put("CREATE_BY", userinfo.UserId);
            decEi.PutDate("CREATE_DATE", DateTime.Now);
            decEi.Put("IO_FLAG", "I");
            decEi.Put("EDOC_TYPE", edoctype);
            try
			{
                if (ml != null)
                {
                    ml.Add(decEi);
                }
                else
                {
                    OperationUtils.ExecuteUpdate(decEi, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        public static DataTable GetSMByUid(string uid)
        {
            string sql = "SELECT DATEDIFF(DAY,ETD,ETA) AS INTERVAL_DAY,* FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        private static string AfterLSPConfirmDoDecl(DataTable maindt, UserInfo userinfo)
        {
            if (maindt.Rows.Count <= 0)
                return "No valid data";
            string returnMsg = "";
            string type = "IBBR";
            string uid = Prolink.Math.GetValueAsString(maindt.Rows[0]["U_ID"]);
            int ibtccoun = OperationUtils.GetValueAsInt(string.Format("SELECT COUNT(1) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE='IBTC'", SQLUtils.QuotedStr(uid)),
                 Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ibtccoun > 0)
                type = "IBTC";

            returnMsg = InboundHandel.SendDeclaration(maindt, type, userinfo, "", false);

            string sql = string.Format("SELECT BSTATUS,STATUS,IB_WINDOW,CMP,GROUP_ID,U_ID,SHIPMENT_ID,IS_TRANSIT_BROKER FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable smsmidt = getDataTableFromSql(sql);
            string bstatus = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["BSTATUS"]);
            if (InboundHandel.BROKER_STATUS_Y_NotifyBroker != bstatus || InboundHandel.BROKER_STATUS_H_NotifyTransitBroker != bstatus)
            {
                if (!returnMsg.Contains(@Resources.Locale.L_BookingActionController_Controllers_64))
                {
                    string cmp = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["CMP"]);
                    string ibwindows = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["IB_WINDOW"]);
                    string shipmentid = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["SHIPMENT_ID"]);
                    string istransitbroker = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["IS_TRANSIT_BROKER"]);
                    sql = string.Format("SELECT U_EMAIL FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(ibwindows));
                    string email = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (!string.IsNullOrEmpty(email))
                    {
                        string subtitle = "Auto Send Broker Error!Shipment ID : " + shipmentid;
                        if ("Y".Equals(istransitbroker))
                        {
                            subtitle = "Auto Send Transit Broker Error!Shipment ID : " + shipmentid;
                        }
                        subtitle += " Please correct the data and send it again！";
                        returnMsg += " Please correct the data and send it again！";
                        EvenFactory.AddEven(uid + "#" + shipmentid + "#" + Guid.NewGuid().ToString(), shipmentid, MailManager.InboundNotifyBrokerError, null, 1, 0, email, subtitle, returnMsg);
                    }
                    return returnMsg;
                }
            }
            return returnMsg;
        }

        public static void SetCntrType(string ordno, string CntrType, MixedList ml) {
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
            if (string.IsNullOrEmpty(cntrtype)) return;
            EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("ORD_NO", ordno);
            ei.Put(cntrtype, 1);
            ml.Add(ei);
        }

        public static void SetScmReqETAmsl(DataTable smorddt, string ordno, string newcntr, MixedList ml, DataRow idnicntrdr, string plant = "", string cuft = "", string cntrtype = "")
        {
            TrackingEDI.InboundBusiness.BookingConfirm.SetScmReqETAmsl(smorddt, ordno, newcntr, ml, idnicntrdr, plant, cuft, cntrtype);
        }




        #region 產生運輸單

        public static string CreateOrdNew(string ShipmentId, string suid, MixedList mixList = null)
        {
            return TrackingEDI.InboundBusiness.BookingConfirm.CreateOrdNew(ShipmentId, suid, mixList);
        }


        public static void UpdatePlant(string ShipmentId, MixedList ml)
        {
            string sql = string.Format("SELECT ADDR_CODE,ORD_NO FROM SMRCNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable smrcntr = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow smrcnt in smrcntr.Rows)
            {
                string addrCode = Prolink.Math.GetValueAsString(smrcnt["ADDR_CODE"]);
                string ordNo = Prolink.Math.GetValueAsString(smrcnt["ORD_NO"]);
                if (!string.IsNullOrEmpty(addrCode))
                {
                    sql = string.Format("UPDATE SMORD SET PLANT=(SELECT TOP 1 PLANT FROM BSADDR WHERE FINAL_WH='TEMP' AND ADDR_CODE={0}) WHERE ORD_NO={1}", SQLUtils.QuotedStr(addrCode), SQLUtils.QuotedStr(ordNo));
                    ml.Add(sql);
                }
            }
        }
        #endregion

        public static string SendDeclaration(DataTable maindt, string partytype, UserInfo userinfo, string isdirectlynb = "", bool isBath = false, bool checkIBCR = false)
        {
            string UserId = userinfo.UserId;
            if (maindt.Rows.Count <= 0)
                return "No valid data";
            string returnMsg = string.Empty;
            string nullmsg = string.Empty;
            int i = 0;
            List<string> idList = new List<string>();
            foreach (DataRow dr in maindt.Rows)
            {
                string shipmentid = dr["SHIPMENT_ID"].ToString();
                string brokerstatus = dr["BSTATUS"].ToString();
                string uid = dr["U_ID"].ToString();
                string status = dr["STATUS"].ToString();
                nullmsg = shipmentid + ":";
                if (i > 0)
                {
                    nullmsg = "；" + shipmentid + ":";
                }
                i++;
                string oexporter = dr["OEXPORTER"].ToString();
                string oimporter = dr["OIMPORTER"].ToString();
                if (string.IsNullOrEmpty(oexporter) || string.IsNullOrEmpty(oimporter))
                {
                    returnMsg += nullmsg + @Resources.Locale.L_BookingAction_Controllers_165;
                    continue;
                }
                if (status != "C" && status != "Z" && "Y" != isdirectlynb && "T" != isdirectlynb)
                {
                    if (BROKER_STATUS_I_TransitConfirm != brokerstatus)
                    {
                        returnMsg += nullmsg + @Resources.Locale.M_SMSMI_ERRORMSG_01;
                        continue;
                    }
                }
                if (brokerstatus.Equals("C"))
                {
                    returnMsg += nullmsg + @Resources.Locale.L_DNInfoCheck_Business_68;
                    continue;
                }
                string mailType = string.Empty;
                string groupType = string.Empty;
                if (partytype == "IBBR")
                {
                    if (BROKER_STATUS_C_BrokerConfirm.Equals(brokerstatus) || BROKER_STATUS_H_NotifyTransitBroker.Equals(brokerstatus))
                    {
                        returnMsg += nullmsg + @Resources.Locale.L_DNInfoCheck_Business_68;
                        continue;
                    }
                    if ("F".Equals(status))
                    {
                        returnMsg += nullmsg + @Resources.Locale.L_DNInfoCheck_Business_69;
                        continue;
                    }
                    mailType = "IBR";
                    groupType = "IB";
                }
                else
                {
                    mailType = "TBR";
                    groupType = "ITC";
                }
                DataTable partytable = GetPTByShipmentid(shipmentid);
                partytable = GetPTByPartyType(shipmentid, "IBBR;IBCR;IBTC");
                DataRow[] ibbrs = partytable.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(partytype)));
                if (ibbrs.Length <= 0)
                {
                    returnMsg += nullmsg + @Resources.Locale.L_BookingAction_Controllers_144;
                    continue;
                }
                if (checkIBCR)
                {
                    DataRow[] ibcars = partytable.Select("PARTY_TYPE='IBCR'");
                    if (ibcars.Length <= 0)
                    {
                        returnMsg += nullmsg + @Resources.Locale.L_BookingAction_Controllers_144;
                        continue;
                    }
                }
                string partyno = Prolink.Math.GetValueAsString(ibbrs[0]["PARTY_NO"]);
                string partype = Prolink.Math.GetValueAsString(ibbrs[0]["PARTY_TYPE"]);
                if (!idList.Contains(uid))
                    idList.Add(uid);
                string message = string.Empty;
                if (!isBath)
                    message = SendBrokerMail(partyno, mailType, userinfo.GroupId, partype, dr, groupType);
                if (!string.IsNullOrEmpty(message))
                {
                    returnMsg += nullmsg + message;
                    continue;
                }
                UpdateBorder(uid, partytype, dr, userinfo);
                nullmsg += @Resources.Locale.L_BookingActionController_Controllers_64;
                returnMsg += nullmsg;
            }

            if (isBath && idList.Count > 0)
            {
                WebGui.Controllers.SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "INS", partytype, true);
            }
            return returnMsg;
        }

        public static string UpdateBorder(string uid, string Type, DataRow smdr, UserInfo userinfo)
        {
            string status = smdr["STATUS"].ToString();
            string returnMsg = string.Empty;
            string shipmentid = smdr["SHIPMENT_ID"].ToString();
            string cmp = smdr["CMP"].ToString();
            string pod = smdr["POD_CD"].ToString();
            string csmNo = smdr["CSM_NO"].ToString();
            string IBwindow = Prolink.Math.GetValueAsString(smdr["IB_WINDOW"]);
            string UId = userinfo.UserId;
            if ("O".Equals(userinfo.IoFlag))
                UId = IBwindow;
            MixedList mixList = new MixedList();
            bool isTransload = CheckIsTransload(smdr);
            EditInstruct apei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);
            string newStatus = "";
            if (isTransload && string.IsNullOrEmpty(csmNo))
            {
                apei.Put("TRANSLOAD", Type == "IBTC" ? "T" : "Y");
                if (Type == "IBTC")
                {
                    SplitSMINPByShipment(shipmentid, userinfo.UserId, mixList);
                }
            }
            if (Type == "IBBR")
            {
                if ("A".Equals(status) || "B".Equals(status) || "C".Equals(status) || "I".Equals(status))
                {
                    apei.Put("STATUS", 'C');
                }
                apei.Put("BSTATUS", BROKER_STATUS_Y_NotifyBroker);
                newStatus = "C";
            }
            else if (Type == "IBTC")
            {
                if ("A".Equals(status) || "B".Equals(status) || "C".Equals(status))
                {
                    apei.Put("STATUS", 'H');
                }
                apei.Put("BSTATUS", BROKER_STATUS_H_NotifyTransitBroker);
                newStatus = "H";
            }
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(DateTime.Now, userinfo.CompanyId);
            apei.PutDate("LSP_CONFIRM_DATE", ndt);
            apei.Put("LSP_CONFIRM_BY", userinfo.UserId);

            mixList.Add(apei);
            if (!string.IsNullOrEmpty(csmNo) && !string.IsNullOrEmpty(newStatus))
                UpdateShipmentStatusByCsmUId(uid, newStatus, mixList);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = shipmentid, StsCd = "045", Cmp = cmp, Sender = UId, Location = pod, LocationName = "", StsDescp = "Notify To Broker" });
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }

        public static void UpdateShipmentStatusByCsmUId(string uid,string status,MixedList ml=null)
        {
            bool isUpdate = false;
            if (ml == null)
            {
                isUpdate = true;
                ml = new MixedList();
            }
            Dictionary<string, List<ShipmentInfo>> ShipmentCsmDic = new Dictionary<string, List<ShipmentInfo>>();
            Dictionary<string, string> ShipmentStatusDic = new Dictionary<string, string>();
            List<string> shipmentList = new List<string>();
            string sql = string.Format(@"SELECT T.U_FID,T.SHIPMENT_ID,T.SHIPMENT_STATUS,SMSMI.SHIPMENT_ID AS CSM_NO,
SMSMI.STATUS AS CSM_STATUS FROM 
(SELECT U_FID,SHIPMENT_ID,(SELECT TOP 1 STATUS FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDNPL.SHIPMENT_ID) AS SHIPMENT_STATUS FROM SMIDNPL WHERE 
SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMIDNPL WHERE U_FID={0})) T LEFT JOIN SMSMI ON T.U_FID=SMSMI.U_ID",
SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string csmUid = Prolink.Math.GetValueAsString(dr["U_FID"]);
                string shipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (string.IsNullOrEmpty(csmUid) || shipmentList.Contains(shipmentId))
                {
                    if (!shipmentList.Contains(shipmentId))
                        shipmentList.Add(shipmentId);
                    continue;
                }
                string shipmentStatus = Prolink.Math.GetValueAsString(dr["SHIPMENT_STATUS"]);
                string csmNo = Prolink.Math.GetValueAsString(dr["CSM_NO"]);
                string csmStatus = Prolink.Math.GetValueAsString(dr["CSM_STATUS"]);
                ShipmentInfo csmInfo = new ShipmentInfo();
                csmInfo.ShipmentId = csmNo;
                csmInfo.Status = csmStatus;
                if (csmUid == uid)
                    csmInfo.Status = status;
                if (!ShipmentCsmDic.ContainsKey(shipmentId))
                {
                    ShipmentCsmDic.Add(shipmentId, new List<ShipmentInfo>() { });
                    ShipmentStatusDic.Add(shipmentId, shipmentStatus);
                }

                List<ShipmentInfo> shipmentInfos = ShipmentCsmDic[shipmentId];
                shipmentInfos.Add(csmInfo);
            }

            foreach (string key in ShipmentCsmDic.Keys)
            {
                if (shipmentList.Contains(key))
                    continue;
                List<ShipmentInfo> shipmentInfos = ShipmentCsmDic[key];
                bool updateStatus = true;
                string bstatus = "";
                foreach (ShipmentInfo info in shipmentInfos)
                {
                    switch (status)
                    {
                        case "H"://Notify TransitBroker
                            if (info.Status == "A" || info.Status == "B" || info.Status == "X")
                                updateStatus = false;
                            bstatus = BROKER_STATUS_H_NotifyTransitBroker;
                            break;
                        case "I"://TransitBroker Confirm
                            if (info.Status == "A" || info.Status == "B" || info.Status=="H" || info.Status == "X")
                                updateStatus = false;
                            bstatus = BROKER_STATUS_I_TransitConfirm;
                            break;
                        case "C"://Notify Broker
                            if(info.Status == "A" || info.Status == "B" || info.Status == "H"|| info.Status == "I" || info.Status == "X")
                                updateStatus = false;
                            bstatus = BROKER_STATUS_Y_NotifyBroker;
                            break;
                        case "D"://Broker Confirm
                            if (info.Status == "A" || info.Status == "B" || info.Status == "H" || info.Status == "I" || info.Status == "C" || info.Status == "X")
                                updateStatus = false;
                            bstatus = BROKER_STATUS_C_BrokerConfirm;
                            break;
                        case "G"://Gate In
                            if(info.Status != "G" && info.Status != "O")
                                updateStatus = false;
                            break;
                        case "O"://Gate Out
                            if (info.Status != "O")
                                updateStatus = false;
                            break;

                    }
                }
                if (updateStatus)
                {
                    EditInstruct apei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    apei.PutKey("SHIPMENT_ID", key);
                    apei.Put("STATUS", status);
                    if(!string.IsNullOrEmpty(bstatus))
                        apei.Put("BSTATUS", bstatus);
                    ml.Add(apei);
                }
            }

            if (isUpdate&& ml.Count>0)
            {
                try {
                    int[] result1 = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                { 
                    
                }
            }
        }

        public static void UpdateShipmentStatusByCsmNo(string shipmentId,string status,MixedList ml)
        {
            string sql =string.Format("SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            string uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(uid))
                UpdateShipmentStatusByCsmUId(uid, status, ml);
        }

        public static string SendBrokerMail(string partyno, string mailType, string GroupId, string partytype, DataRow smRow, string groupType)
        {
            string shipmentid = smRow["SHIPMENT_ID"].ToString();
            string uid = smRow["U_ID"].ToString();
            string CompanyId = smRow["CMP"].ToString();
            string returnMsg = string.Empty;
            string mail_to = string.Empty;
            string subtital = string.Empty;
            DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, groupType);
            if (mailGroupDt.Rows.Count <= 0)
            {
                return @Resources.Locale.L_BookingActionController_Controllers_47 + partyno + @Resources.Locale.L_BookingActionController_Controllers_48;
            }
            subtital = shipmentid + @Resources.Locale.L_BookingAction_Controllers_167;
            if ("TBR".Equals(mailType))
                subtital = shipmentid + " Send to Transit Broker Information";
            foreach (DataRow mailGroup in mailGroupDt.Rows)
            {
                mail_to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                if (string.IsNullOrEmpty(mail_to))
                {
                    continue;
                }
                EvenFactory.AddEven(mailType + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString(), uid, "IN", null, 1, 0, mail_to, subtital, "");
            }
            return returnMsg;
        }


        public static DataTable GetPTByShipmentid(string shipmentid)
        {
            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static DataTable GetPTByPartyType(string shipmentid, string partytype)
        {
            string[] partytypes = partytype.Split(';');
            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN {1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.Quoted(partytypes));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }



        public static DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
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
            TrackingEDI.InboundBusiness.ReserveHelper.ToEi(dt, tableName, ml, uid, parm, filesname);
        }

        /// <summary>
        /// 将对应的party Type以及Party No生成EditInstruct 加入到MixedList中
        /// </summary>
        /// <param name="shipmentuid">shipmentuid</param>
        /// <param name="ShipmentId">ShipmentId</param>
        /// <param name="ml">MixedList</param>
        /// <param name="PartyNo">PartyNo</param>
        /// <param name="PartyType">PartyType</param>
        public static void SetPartyToSMSMIPT(string shipmentuid, string ShipmentId, MixedList ml, string PartyNo, string PartyType)
        {
            TrackingEDI.InboundBusiness.SMSMIHelper.SetPartyToSMSMIPT(shipmentuid, ShipmentId, ml, PartyNo, PartyType);
        }

        /// <summary>
        /// 更新订舱相关的 SMIRV 和 SMIRV的CUFT栏位
        /// </summary>
        /// <param name="no"></param>
        /// <param name="is_reserve">是否是预约单</param>
        public static void UpdateSMICUFT(string no, bool is_reserve = false)
        {
            TrackingEDI.InboundBusiness.SMSMIHelper.UpdateSMICUFT(no, is_reserve);
        }

        public static Result TCBookConfirmFunc(DataRow maindr, UserInfo userinfo)
        {
            string shipmentid = maindr["SHIPMENT_ID"].ToString();
            string TranType = maindr["TRAN_TYPE"].ToString();
            string bstatus = maindr["BSTATUS"].ToString();
            string status = maindr["STATUS"].ToString();
            string u_id = maindr["U_ID"].ToString();
            string sql = string.Empty;
            string returnMessage = string.Empty;
            string CompanyId = userinfo.CompanyId;
            Result result = new Result();
            result.Success = false;
            MixedList mixedlist = new MixedList();

            if ("O".Equals(userinfo.IoFlag))
            {
                sql = string.Format("SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_NO={1} AND PARTY_TYPE='IBTC'",
                        SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(CompanyId));
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (count <= 0)
                {
                    result.Message = "No permission to operated!";
                    return result;
                }
            }
            string table = "SMIDN";
            if (TranType == "F" || TranType == "R")
            {
                table = "SMICNTR";
            }

            sql = string.Format("SELECT COUNT(1) FROM SMBID WHERE SHIPMENT_ID={0} AND CMP={1} AND FSTATUS IN ('C','D')AND CHG_CD IN ('ICD','ICDF')", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(CompanyId));
            int billcount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (billcount > 0)
            {
                result.Message = string.Format("The Shipment {0} corresponding bill has been uploaded!", shipmentid);
                result.Success = false;
                return result;
            }

            sql = string.Format("SELECT * FROM " + table + " WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (mainDt.Rows.Count > 0)
            {
                foreach (DataRow dr in mainDt.Rows)
                {
                    string DecDate = Prolink.Math.GetValueAsString(dr["TC_DEC_DATE"]);
                    string RelDate = Prolink.Math.GetValueAsString(dr["TC_REL_DATE"]);
                    string ImportNo = Prolink.Math.GetValueAsString(dr["TC_IMPORT_NO"]);
                    string Inspection = Prolink.Math.GetValueAsString(dr["TC_INSPECTION"]);

                    if (DecDate == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "Declaration Date is required";
                        return result;
                    }

                    if (RelDate == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "Release Date is required";
                        return result;
                    }

                    if (ImportNo == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "Import No. is required";
                        return result;
                    }

                    if (Inspection == "")
                    {
                        result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + "Inspection is required";
                        return result;
                    }
                }

            }

            if (bstatus.Equals("C"))
            {
                result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingAction_Controllers_168;
                return result;
            }
            if ("N".Equals(bstatus))
            {
                result.Message = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_67;
                return result;
            }

            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", u_id);
            ei.Put("MODIFY_BY", userinfo.UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            ei.PutDate("MODIFY_DATE", odt);
            ei.PutDate("MODIFY_DATE_L", ndt);
            ei.Put("BSTATUS", BROKER_STATUS_I_TransitConfirm);
            if (BOOKING_STATUS_H_NotifyTransitBroker.Equals(status))     //只有在Notify transit broker状态下才转变成transit broker confirm
            {
                ei.Put("STATUS", BOOKING_STATUS_I_TransitConfirm);
            }
            mixedlist.Add(ei);

            if (table == "SMICNTR")
            {
                foreach (DataRow cdr in mainDt.Rows)
                {
                    string cuid = Prolink.Math.GetValueAsString(cdr["U_ID"]);
                    string TcNewSeal = Prolink.Math.GetValueAsString(cdr["TC_NEW_SEAL"]);
                    sql = "UPDATE SMICNTR SET NEW_SEAL=" + SQLUtils.QuotedStr(TcNewSeal) + " WHERE U_ID=" + SQLUtils.QuotedStr(cuid);
                    mixedlist.Add(sql);
                }
            }

            sql = "UPDATE SMRDN SET SMRDN.TC_DEC_NO=SMIDN.TC_DEC_NO FROM SMIDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            mixedlist.Add(sql);

            sql = "UPDATE SMRCNTR SET SMRCNTR.TC_DEC_NO=SMICNTR.TC_DEC_NO FROM SMICNTR WHERE SMICNTR.CNTR_NO=SMRCNTR.CNTR_NO AND SMICNTR.SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            mixedlist.Add(sql);

            sql = string.Format("UPDATE SMORD SET BSTATUS='I' FROM (select i.BSTATUS from SMSMI as i inner join SMORD as d on i.SHIPMENT_ID=d.SHIPMENT_ID where i.SHIPMENT_ID={0}) as a where SHIPMENT_ID={0}",
                    SQLUtils.QuotedStr(shipmentid));
            mixedlist.Add(sql);
            try
            {
                int[] result1 = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                //执行发送notify broker
                sql = "SELECT * FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string notifybrMsg = InboundHandel.SendDeclaration(maindt, "IBBR", userinfo, "", false);
                result.Message = @Resources.Locale.L_BookingActionController_Controllers_68 + shipmentid + " Exp. Transit Confirm Successful!";
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
                return result;
            }
        }

        /// <summary>
        /// Check POD fils is complete
        /// </summary>
        /// <param name="ReserveNo">预约号码</param>
        /// <param name="Edocjobno">Edoc Job No</param>
        /// <param name="sum"></param>
        /// <returns></returns>
        public static bool checkPODstatus(string ReserveNo, string Edocjobno, int existcounts = 0)
        {
            string sql = string.Format("SELECT COUNT(*) FROM FILES WHERE FID IN (SELECT FID FROM FOLDERS WHERE GUID IN (SELECT FOLDER_GUID FROM EDOC2_FOLDER WHERE JOB_NO ={0} )) AND EdocType='POD'", SQLUtils.QuotedStr(Edocjobno));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT TRAN_TYPE,CASE WHEN TRAN_TYPE='F' OR TRAN_TYPE='R' "
                                + " THEN (SELECT COUNT(1) FROM (SELECT DISTINCT WS_CD FROM SMRCNTR WHERE RESERVE_NO=SMIRV.RESERVE_NO)T) "
                                + " ELSE (SELECT COUNT(1) FROM (SELECT DISTINCT WS_CD FROM SMRDN WHERE RESERVE_NO=SMIRV.RESERVE_NO)T1) END AS COUNT " +
                                    " FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(ReserveNo));
            DataTable smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int countdn = 0;
            if (smrvdt.Rows.Count > 0)
                countdn = Prolink.Math.GetValueAsInt(smrvdt.Rows[0]["COUNT"]);
            if (count >= countdn - existcounts)
                return true;
            return false;
        }

        public static string ApartFromSATU(DateTime dt)
        {
            int saturint = 0;
            string weekstr = dt.DayOfWeek.ToString();
            switch (weekstr)
            {
                case "Monday": saturint = 5; break;
                case "Tuesday": saturint = 4; break;
                case "Wednesday": saturint = 3; break;
                case "Thursday": saturint = 2; break;
                case "Friday": saturint = 1; break;
                case "Saturday": saturint = 0; break;
                case "Sunday": saturint = -1; break;
            }
            return dt.AddDays(saturint).ToString("yyyyMMdd");
        }


        public static string CreateShipment(List<string> shipmentlist, string groupid, string cmp, string userid, ref string _newshipmentid)
        {
            string Msg = string.Empty;
            Dictionary<string, object> parm = new Dictionary<string, object>();

            string firstshipment = shipmentlist[0];
            if (!string.IsNullOrEmpty(_newshipmentid))
            {
                firstshipment = _newshipmentid;
            }
            string sql = string.Format("SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            string newguid = System.Guid.NewGuid().ToString();
            string polcd = smsmdt.Rows[0]["POL_CD"].ToString();

            if (string.IsNullOrEmpty(_newshipmentid))
            {
                _newshipmentid = "B" + TransferBooking.GetSMAutoNo(groupid, cmp, "*");
            }
            else
            {
                sql = string.Format("SELECT MASTER_NO,HOUSE_NO,POL_CD FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(_newshipmentid));
                DataTable cbsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (cbsmdt.Rows.Count > 0)
                    polcd = cbsmdt.Rows[0]["POL_CD"].ToString();
            }
            parm.Add("SHIPMENT_ID", _newshipmentid);
            ToEi(smsmdt, "SMSMI", ml, newguid, parm);
            ToEi(smsmptdt, "SMSMIPT", ml, newguid, parm);
            string shipmentinfo = string.Join(",", shipmentlist);
            UpdateComineBill(ml, _newshipmentid, shipmentinfo);
            foreach (string shipmentid in shipmentlist)
            {
                UpdateCombinSMInfo(_newshipmentid, ml, shipmentinfo, shipmentid);
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //try
                    //{
                    //    BookingParser bp = new BookingParser();
                    //    bp.SaveToTrackingByShimentID(new string[] { _newshipmentid });
                    //    TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ShipmentId = _newshipmentid, StsCd = "014", Sender = userid, Location = polcd, LocationName = "", StsDescp = "Combine BL" });
                    //}
                    //catch (Exception ex)
                    //{
                    //}
                }
                catch (Exception ex)
                {
                    Msg = ex.ToString();
                }
            }
            return Msg;
        }

        public static void UpdateComineBill(MixedList ml, string newshipment, string shipmentinfo)
        {
            string sql = string.Format("SELECT CNTR_INFO,MASTER_NO,HOUSE_NO,INVOICE_INFO FROM SMSMI WHERE  SHIPMENT_ID  IN{0}", SQLUtils.Quoted(shipmentinfo.Split(',')));
            DataTable smdt = getDataTableFromSql(sql);
            List<string> cntrlist = new List<string>();
            List<string> masternolist = new List<string>();
            List<string> housenolist = new List<string>();
            List<string> invoicelist = new List<string>();
            foreach (DataRow dr in smdt.Rows)
            {
                string cntrinfo = Prolink.Math.GetValueAsString(dr["CNTR_INFO"]);
                string master = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                string house = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
                string invoiceinfo = Prolink.Math.GetValueAsString(dr["INVOICE_INFO"]);
                cntrinfo = cntrinfo.Trim(',');
                if (!cntrlist.Contains(cntrinfo))
                {
                    cntrlist.Add(cntrinfo);
                }
                if (!masternolist.Contains(master))
                {
                    masternolist.Add(master);
                }
                if (!housenolist.Contains(house))
                {
                    housenolist.Add(house);
                }
                invoiceinfo = invoiceinfo.Trim(',');
                if (!invoicelist.Contains(invoiceinfo))
                {
                    invoicelist.Add(invoiceinfo);
                }
            }

            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", newshipment);
            ei.Put("STATUS", "V");
            ei.Put("BSTATUS", "F");
            ei.Put("SHIPMENT_INFO", shipmentinfo);
            ei.Put("COMBIN_SHIPMENT", newshipment);
            ei.Put("CNTR_INFO", string.Join(",", cntrlist));
            ei.Put("HOUSE_NO", string.Join(",", housenolist));
            ei.Put("MASTER_NO", string.Join(",", masternolist));
            ei.Put("INVOICE_INFO", string.Join(",", invoicelist));
            ei.Put("ISCOMBINE_CALLTRUCK", "C");    //合并的提单，设置为Y
            ml.Add(ei);
        }

        private static void UpdateCombinSMInfo(string combineShipment, MixedList ml, string shipmentinfo, string keyshipmentid)
        {
            EditInstruct cbei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            cbei.PutKey("SHIPMENT_ID", keyshipmentid);
            cbei.Put("SHIPMENT_INFO", shipmentinfo);
            cbei.Put("COMBIN_SHIPMENT", combineShipment);
            //ei.PutDate("COMBINE_DATE", DateTime.Now);
            cbei.Put("ISCOMBINE_CALLTRUCK", "S");    //合并的提单，设置为Y
            //cbei.Put("BL_TYPE", "S");
            ml.Add(cbei);
        }

        public static bool CheckIsTransload(DataRow smDr)
        {
            string shipmentId = Prolink.Math.GetValueAsString(smDr["SHIPMENT_ID"]);
            string cmp = Prolink.Math.GetValueAsString(smDr["CMP"]);
            string tranType = Prolink.Math.GetValueAsString(smDr["TRAN_TYPE"]);
            string pod = Prolink.Math.GetValueAsString(smDr["POD_CD"]);
            string table = "F".Equals(tranType) || "R".Equals(tranType) ? "SMICNTR" : "SMIDN";
            string sql = string.Format("SELECT DLV_AREA FROM {1} WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentId), table);
            string dlvArea = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM BSTPS WHERE CMP={0} AND PORT_CD={1} AND TRAN_TYPE={2} AND DLV_AREA={3}",
                SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(tranType), SQLUtils.QuotedStr(dlvArea));
            DataTable tpsDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return tpsDt.Rows.Count > 0;

        }

        public static void SplitSMINPByShipment(string shipmentId,string userid,MixedList ml = null)
        {
            bool isUpdate = ml == null;
            if (ml == null)
                ml = new MixedList();
            ml.Add(string.Format("DELETE FROM SMIDNPL WHERE DN_NO IN (SELECT DISTINCT DN_NO FROM SMIDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentId)));
            ml.Add(string.Format("DELETE FROM SMORD WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId)));
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT *,(SELECT TOP 1 INV_NO FROM SMIDN WHERE SMIDN.DN_NO=SMINP.DN_NO) AS IINV_NO FROM SMINP WHERE DN_NO IN (SELECT DISTINCT DN_NO FROM SMIDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //dt.Columns.Add("U_ID", typeof(string));
            dt.Columns.Add("MASTER_NO", typeof(string));
            dt.Columns.Add("HOUSE_NO", typeof(string));
            dt.Columns.Add("ETA", typeof(DateTime));
            dt.Columns.Add("ATA", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_BY", typeof(string));
            dt.Columns.Add("DISCHARGE_DATE", typeof(DateTime));
            dt.Columns.Add("POD_CD", typeof(string));
            dt.Columns.Add("POD_NAME", typeof(string));
            dt.Columns.Add("CARRIER", typeof(string));
            dt.Columns.Add("CARRIER_NM", typeof(string));
            dt.Columns.Add("AVG_TTL_QTY", typeof(decimal));
            dt.Columns.Add("AVG_CASE_NUM", typeof(decimal));
            dt.Columns.Add("AVG_TTL_NW", typeof(decimal));
            dt.Columns.Add("AVG_TTL_GW", typeof(decimal));
            dt.Columns.Add("AVG_TTL_CBM", typeof(decimal));
            dt.Columns.Add("AVG_GW_BY_PN", typeof(decimal));
            string masterNo = Prolink.Math.GetValueAsString(smDt.Rows[0]["MASTER_NO"]);
            string houseNo = Prolink.Math.GetValueAsString(smDt.Rows[0]["HOUSE_NO"]);
            DateTime eta = Prolink.Math.GetValueAsDateTime(smDt.Rows[0]["ETA"]);
            DateTime ata = Prolink.Math.GetValueAsDateTime(smDt.Rows[0]["ATA"]);
            DateTime dischargeDate = Prolink.Math.GetValueAsDateTime(smDt.Rows[0]["DISCHARGE_DATE"]);
            string pod = Prolink.Math.GetValueAsString(smDt.Rows[0]["POD_CD"]);
            string podNm = Prolink.Math.GetValueAsString(smDt.Rows[0]["POD_NAME"]);
            string carrier = Prolink.Math.GetValueAsString(smDt.Rows[0]["CARRIER"]);
            string carrierNm = Prolink.Math.GetValueAsString(smDt.Rows[0]["CARRIER_NM"]);
            DataTable newDt = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                string plaNo = Prolink.Math.GetValueAsString(dr["PLA_NO"]);
                string caseNo = Prolink.Math.GetValueAsString(dr["CASE_NO"]);
                
                dr["MASTER_NO"] = masterNo;
                dr["HOUSE_NO"] = houseNo;
                dr["ETA"] = eta;
                dr["ATA"] = ata;
                dr["DISCHARGE_DATE"] = dischargeDate;
                dr["POD_CD"] = pod;
                dr["POD_NAME"] = podNm;
                dr["CARRIER"] = carrier;
                dr["CARRIER_NM"] = carrierNm;
                dr["SHIPMENT_ID"] = shipmentId;
                dr["CREATE_DATE"] = DateTime.Now;
                dr["CREATE_BY"] = userid;
                List<string> plaList = ProcessRangeString(plaNo);
                List<string> caseList = ProcessRangeString(caseNo);
                SetAVGValue(dr, plaList, caseList);
                if (!string.IsNullOrEmpty(plaNo))
                {
                    for (int i = 0; i < plaList.Count; i++)
                    {
                        dr["U_ID"] = System.Guid.NewGuid().ToString();
                        dr["PLA_NO"] = plaList[i];
                        if (caseList.Count > i + 1 && plaList.Count > 1)
                            dr["CASE_NO"] = caseList[i];
                        if (plaList.Count - 1 == i)
                            SetAVGValue(dr, plaList, caseList, true);
                        newDt.ImportRow(dr);
                       
                    }
                }
                else {
                    for (int i = 0; i < caseList.Count; i++)
                    {
                        dr["U_ID"] = System.Guid.NewGuid().ToString();
                        dr["PLA_NO"] = "";
                        dr["CASE_NO"] = caseList[i];
                        if (caseList.Count - 1 == i && caseList.Count>1)
                            SetAVGValue(dr, plaList, caseList, true);
                        newDt.ImportRow(dr);
                    }
                }
            }
            BaseParser baseparser = new BaseParser();
            baseparser.ParseEditInstruct(newDt, "IDNPLMapping", ml);
            if (isUpdate)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                { 
                
                }
            }
        }

        public static void SetAVGValue(DataRow dr, List<string> plaList,List<string> caseList,bool isLast=false)
        {
            int length = plaList.Count > 0 ? plaList.Count : caseList.Count;
            if (length <= 0)
                length = 1;
            //decimal Qty = Prolink.Math.GetValueAsDecimal(dr["QTY"]);
            decimal TtlQty = Prolink.Math.GetValueAsDecimal(dr["TTL_QTY"]);
            //decimal Nw = Prolink.Math.GetValueAsDecimal(dr["NW"]);
            decimal TtlNw = Prolink.Math.GetValueAsDecimal(dr["TTL_NW"]);
            decimal caseNum = Prolink.Math.GetValueAsDecimal(dr["CASE_NUM"]);
            //decimal Gw = Prolink.Math.GetValueAsDecimal(dr["GW"]);
            decimal TtlGw = Prolink.Math.GetValueAsDecimal(dr["TTL_GW"]);
            //decimal cbm = Prolink.Math.GetValueAsDecimal(dr["CBM"]);
            decimal ttlCbm = Prolink.Math.GetValueAsDecimal(dr["TTL_CBM"]);
            decimal gwByPn = Prolink.Math.GetValueAsDecimal(dr["GW_BY_PN"]);
            decimal avgTtlQty = isLast ? TtlQty - Math.Round(TtlQty / length, 0) * (length - 1) : Math.Round(TtlQty / length, 0);
            decimal avgTtlNw = isLast ? TtlNw - Math.Round(TtlNw / length, 6) * (length - 1) : Math.Round(TtlNw / length, 6);
            decimal avgTtlGw = isLast ? TtlGw - Math.Round(TtlGw / length, 6) * (length - 1) : Math.Round(TtlGw / length, 6);
            decimal avgTtlCbm = isLast ? ttlCbm - Math.Round(ttlCbm / length, 6) * (length - 1) : Math.Round(ttlCbm / length, 6);
            decimal avgGwByPn = isLast ? gwByPn - Math.Round(gwByPn / length, 6) * (length - 1) : Math.Round(gwByPn / length, 6);
            decimal avgCaseNum = isLast ? caseNum - Math.Round(caseNum / length, 0) * (length - 1) : Math.Round(caseNum / length, 0);
            if (avgTtlQty > 0)
                dr["AVG_TTL_QTY"] = avgTtlQty;
            if (avgTtlNw > 0)
                dr["AVG_TTL_NW"] = avgTtlNw;
            if (avgTtlGw > 0)
                dr["AVG_TTL_GW"] = avgTtlGw;
            if (avgTtlCbm > 0)
                dr["AVG_TTL_CBM"] = avgTtlCbm;
            if (avgGwByPn > 0)
                dr["AVG_GW_BY_PN"] = avgGwByPn;
            if (avgCaseNum > 0)
                dr["AVG_CASE_NUM"] = avgCaseNum;
        }

        public static List<string> ProcessRangeString(string input)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(input))
                return list;
            // 检查输入字符串是否包含"-"
            if (!input.Contains("-"))
            {
                // 如果不包含"-"，则直接返回原始字符串
                return new List<string>() { input };
            }

            // 使用正则表达式匹配可能的前缀、起始值和结束值
            Match match = Regex.Match(input, @"^(?<prefix>.*?)(?<start>\d+)-(?<end>\d+)(?<suffix>.*)$");
            if (!match.Success)
            {
                // 如果正则表达式不匹配，返回原始字符串或抛出异常（取决于具体需求）
                // 这里我们选择返回原始字符串
                return new List<string>() { input };
            }

            string prefix = match.Groups["prefix"].Value;
            string suffix = match.Groups["suffix"].Value;
            int start = int.Parse(match.Groups["start"].Value);
            int end = int.Parse(match.Groups["end"].Value);

            // 构建结果字符串
            for (int i = start; i <= end; i++)
            {
                // 根据前缀、当前值和后缀构建字符串
                list.Add($"{prefix}{i.ToString().PadLeft(match.Groups["start"].Value.Length, '0')}{suffix}");
            }

            return list;
        }
    }
}