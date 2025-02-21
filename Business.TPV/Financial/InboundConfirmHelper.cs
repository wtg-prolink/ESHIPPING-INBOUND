using Business.TPV.Financial;
using EDOCApi;
using Models.InboundProcess;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Mail;
using TrackingEDI.Model;

namespace Business.TPV.Financial
{
    public class InboundConfirmHelper
    {
        public const string _UserId = "System AUTO";
        public const string CallType_ByDN = "D";
        public const string CallType_ByContainer = "C";

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

        public const string L_BookingActionController_Controllers_64 = "Export CC notification has been sent to broker!";
        public const string L_BookingAction_Controllers_144 = "The trucker/trailer(CR) or broker/BR was required, please check!";
        public const string L_BookingAction_Controllers_165 = "Please verify the Exporter &amp; Importer for Exp. C/C Notification has been well maintained.";
        public const string M_SMSMI_ERRORMSG_01 = "Can not notify broker before Shipment Confirm";
        public const string L_DNInfoCheck_Business_68 = "The shipment has finished Exp. Customs Declaration!";
        public const string L_DNInfoCheck_Business_69 = "The shipment has finished Exp. CC Release!";
        public const string L_BookingActionController_Controllers_47 = "email sending was terminated, please check the settings.";
        public const string L_BookingActionController_Controllers_48 = "the email group setting.";
        public const string L_BookingAction_Controllers_167 = "Send Exp. C/C Notification";
        public const string L_BookingActionController_Controllers_68 = "The Shipment:";
        public const string L_BookingAction_Controllers_170 = "Exp. C/C Successful";
        public const string L_BookingActionController_Controllers_66 = "The Shipment:";
        public const string L_BookingAction_Controllers_168 = "Exp. C/C Confirmed";
        public const string L_BookingActionController_Controllers_67 = "Export C/C notification hasn\'t been sent to broker yet!";
        public const string L_GateManageController_Controllers_137 = "Double calling on truck";
        public const string L_GateManageController_Controllers_146 = "Corrosponding truck company not found; can\'t call truck";

        public InboundConfirmHelper()
        {

        }

        public string NotifyLspCheck(DataTable dt)
        {
            string message = "";
            string str = "\r\n";
            DataRow dr = dt.Rows[0];
            string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string sql = string.Format(@"SELECT * FROM SMSMIPT WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable subdt = getDataTableFromSql(sql);
             
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                sql = string.Format("SELECT COUNT(*) FROM SMICNTR WHERE (WS_CD IS NULL OR WS_CD ='') AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            }
            else
            {
                sql = string.Format("SELECT COUNT(*) FROM SMIDN WHERE (WS_CD IS NULL OR WS_CD ='') AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            }

            if (OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection()) > 0)
            {
                message += ShipmentId + " Warehouse is Empty,Please Check it !" + str;
            }

            if (trantype == "F" || trantype == "R")
            {
                sql = @"SELECT * FROM SMICNTR WHERE (POL1 IS NULL OR TRAN_TYPE1 IS NULL OR TRUCKER1 IS NULL 
                        OR DLV_AREA IS NULL OR DLV_AREA_NM IS NULL OR ADDR_CODE IS NULL OR DLV_ADDR IS NULL) AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                DataTable cntr = getDataTableFromSql(sql);

                if (cntr.Rows.Count > 0)
                {
                    message += ShipmentId + " Delivery Area Name,Delivery Address,Address Code,1st POL, 1st Tran Type, 1st Trucker, is required in Container Information!" + str;
                }
            }
            else
            {
                string Pol1 = Prolink.Math.GetValueAsString(dr["POL1"]);
                string TranType1 = Prolink.Math.GetValueAsString(dr["TRAN_TYPE1"]);
                string Trucker1 = Prolink.Math.GetValueAsString(dr["TRUCKER1"]);
                if (Pol1 == "" || TranType1 == "" || Trucker1 == "")
                {
                    message += ShipmentId + " 1st POL, 1st Tran Type, 1st Trucker is required." + str;
                }

                sql = @"SELECT * FROM SMIDN WHERE ( DLV_AREA IS NULL OR DLV_AREA_NM IS NULL OR ADDR_CODE IS NULL OR DLV_ADDR IS NULL) AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                DataTable dninfo = getDataTableFromSql(sql);

                if (dninfo.Rows.Count > 0)
                {
                    message += ShipmentId + " Delivery Area, Delivery Address is required in Import DN/Invoice." + str;
                }
            }

            DataRow[] brRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBBR'", SQLUtils.QuotedStr(ShipmentId)));
            DataRow[] crRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBCR'", SQLUtils.QuotedStr(ShipmentId)));
            DataRow[] spRows = subdt.Select(string.Format(@"SHIPMENT_ID={0} AND PARTY_TYPE='IBSP'", SQLUtils.QuotedStr(ShipmentId)));
            if (brRows.Length <= 0 || crRows.Length <= 0 || spRows.Length <= 0)
            {
                message += "This shipment " + ShipmentId + " Please check Party type has [IBBR], [IBCR], [IBSP]." + str;
            }
             
            return message;
        }


        #region Booking Agent Confirm 

        public string BookingAgentConfirm(SMSMIModel smsmiModel, BookingAgent agent, UserInfo userinfo)
        {
            try
            {
                List<string> cntrList = new List<string>();
                MixedList ml = new MixedList();

                EditInstruct ei;
                ml = new MixedList();
                string IbpaPartyno = string.Empty;
                string ShipmentID = smsmiModel.SHIPMENT_ID;
                string tyanType = smsmiModel.TRAN_TYPE;
                if (string.IsNullOrEmpty(ShipmentID))
                    return "Booking Agent Confirm Error:No ShipmentID";
                DateTime Ata = Prolink.Math.GetValueAsDateTime(agent.ATA);
                string ContainerNo = smsmiModel.CNTR_NO;
                string ERLocation = agent.BACK_LOCATION;
                string PinNo = agent.PIN_NO;
                DateTime DischargeTime = Prolink.Math.GetValueAsDateTime(agent.DISCHARGE_DATE);
                IbpaPartyno = agent.InboundTerminalAgent;

                bool IsCntr = false;

                switch (tyanType)
                {
                    case "F":
                    case "R":
                        IsCntr = true;
                        break;
                }

                if (IsCntr)
                {
                    if (!cntrList.Contains(ShipmentID))
                        cntrList.Add(ShipmentID);
                    ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    ei.PutKey("CNTR_NO", ContainerNo);
                    ei.Put("BACK_LOCATION", ERLocation);
                    ei.Put("PIN_NO", PinNo);
                    ei.PutDate("DISCHARGE_DATE", DischargeTime);
                    ml.Add(ei);
                    ei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    ei.PutKey("CNTR_NO", ContainerNo);
                    ei.PutDate("DISCHARGE_DATE", DischargeTime);
                    ml.Add(ei);
                }
                else
                {
                    //string invno = Prolink.Math.GetValueAsString(dr[4]);  
                    ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    //ei.PutKey("INV_NO", invno);
                    ei.PutDate("DISCHARGE_DATE", DischargeTime);
                    ml.Add(ei);
                    ei = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    //ei.PutKey("INV_NO", invno);
                    ei.PutDate("DISCHARGE_DATE", DischargeTime);
                    ml.Add(ei);
                }

                if (Ata.Year != 1)
                {
                    ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    ei.PutDate("ATA", Ata);
                    ml.Add(ei);
                    ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("SHIPMENT_ID", ShipmentID);
                    ei.PutDate("ATA", Ata);
                    ml.Add(ei);
                }

                if (ml.Count > 0)
                {
                    int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }

                if (!string.IsNullOrEmpty(IbpaPartyno))
                {
                    string sql1 = string.Format("SELECT * FROM SMPTY WHERE STATUS='U' AND PARTY_NO Like '%{0}%'", IbpaPartyno);
                    DataTable smpty = getDataTableFromSql(sql1);
                    if (smpty.Rows.Count > 0)
                    {
                        string UId = smsmiModel.U_ID;
                        IbpaPartyno = smpty.Rows[0]["PARTY_NO"].ToString();
                        UploadSmsmipt(UId, ShipmentID, "IBTA", IbpaPartyno, smsmiModel.CMP);
                    }
                }
                ml = new MixedList();

                string updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentID));
                if (!cntrList.Contains(ShipmentID))
                    updatesql = string.Format("UPDATE SMSMI SET DISCHARGE_DATE=(SELECT TOP 1 DISCHARGE_DATE FROM SMIDN WHERE SMIDN.SHIPMENT_ID={0} ORDER BY DISCHARGE_DATE DESC ) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentID));
                ml.Add(updatesql);
                if (ml.Count > 0)
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                //订舱确认
                string condition = " STATUS NOT IN ('I','C','D','H')";
                DataTable maindt = GetSMByShipmentId(ShipmentID, condition);
                if (maindt.Rows.Count <= 0)
                    return "Booking Agent Confirm:SMSMI No Data";
                string sql = "SELECT * FROM SMIRV WHERE SHIPMENT_INFO LIKE '%" + ShipmentID + "%' AND RV_TYPE='I' AND STATUS !='V'";
                DataTable Smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (Smrvdt.Rows.Count > 0)
                    return "Booking Agent Confirm:SMIRV No Data";
                string uid = Prolink.Math.GetValueAsString(maindt.Rows[0]["U_ID"]);

                string msg = BookingConfirm.SetConfirm(maindt, userinfo.UserId, userinfo.CompanyId);
                if (string.IsNullOrEmpty(msg))
                {
                    maindt = GetSMByShipmentId(ShipmentID);
                    msg = AfterLSPConfirmDoDecl(maindt, userinfo);
                    DoDeclaration(maindt, userinfo);

                    return "Booking Agent Confirm Success!" + msg;
                } 
                return msg; 
            }
            catch (Exception ex)
            {
                return "Booking Agent Confirm Error:" + ex.ToString();
            }
        }

        public DataTable GetSMByShipmentId(string shipmentid, string condition = "1=1")
        {
            string sql = string.Format("SELECT DATEDIFF(DAY,ETD,ETA) AS INTERVAL_DAY,* FROM SMSMI WHERE SHIPMENT_ID={0} AND " + condition, SQLUtils.QuotedStr(shipmentid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public string UploadSmsmipt(string U_ID, string Shipment_Id, string PARTY_TYPE, string PARTY_NO, string cmp)
        {
            string returnMessage = "success";
            string errorMsg = "";
            string Sql = "";
            EditInstruct ei;
            MixedList ml = new MixedList();
            try
            {
                if (PARTY_NO == "")
                {
                    return returnMessage;
                }
                Sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(PARTY_NO));
                DataTable dt = getDataTableFromSql(Sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Sql = string.Format("DELETE FROM SMSMIPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE={1}", SQLUtils.QuotedStr(Shipment_Id), SQLUtils.QuotedStr(PARTY_TYPE));
                        ml.Add(Sql);
                        Sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(PARTY_TYPE), SQLUtils.QuotedStr(cmp));
                        string TypeDescp = getOneValueAsStringFromSql(Sql);
                        Sql = string.Format("SELECT ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(PARTY_TYPE), SQLUtils.QuotedStr(cmp));
                        string OrderBy = getOneValueAsStringFromSql(Sql);
                        if (PARTY_TYPE == "IBBK")
                        {
                            TypeDescp = "Outsite Vender";
                        }
                        string UFid = System.Guid.NewGuid().ToString();
                        ei = new EditInstruct("SMSMIPT", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", UFid);
                        ei.Put("U_FID", U_ID);
                        ei.Put("SHIPMENT_ID", Shipment_Id);
                        ei.Put("PARTY_TYPE", PARTY_TYPE);
                        ei.Put("TYPE_DESCP", TypeDescp);
                        ei.Put("ORDER_BY", OrderBy);
                        ei.Put("PARTY_NO", PARTY_NO);
                        ei.Put("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                        ei.Put("PARTY_NAME2", Prolink.Math.GetValueAsString(dr["PARTY_NAME2"]));
                        ei.Put("PARTY_NAME3", Prolink.Math.GetValueAsString(dr["PARTY_NAME3"]));
                        ei.Put("PARTY_NAME4", Prolink.Math.GetValueAsString(dr["PARTY_NAME4"]));
                        ei.Put("PARTY_ADDR1", Prolink.Math.GetValueAsString(dr["PART_ADDR1"]));
                        ei.Put("PARTY_ADDR2", Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
                        ei.Put("PARTY_ADDR3", Prolink.Math.GetValueAsString(dr["PART_ADDR3"]));
                        ei.Put("PARTY_ADDR4", Prolink.Math.GetValueAsString(dr["PART_ADDR4"]));
                        ei.Put("PARTY_ADDR5", Prolink.Math.GetValueAsString(dr["PART_ADDR5"]));
                        ei.Put("CNTY", Prolink.Math.GetValueAsString(dr["CNTY"]));
                        ei.Put("CNTY_NM", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
                        ei.Put("CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
                        ei.Put("CITY_NM", Prolink.Math.GetValueAsString(dr["CITY_NM"]));
                        ei.Put("STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
                        ei.Put("ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
                        ei.Put("PARTY_ATTN", Prolink.Math.GetValueAsString(dr["PARTY_ATTN"]));
                        ei.Put("PARTY_TEL", Prolink.Math.GetValueAsString(dr["PARTY_TEL"]));
                        ei.Put("PARTY_MAIL", Prolink.Math.GetValueAsString(dr["PARTY_MAIL"]));
                        //ei.Put("DEBIT_TO",Prolink.Math.GetValueAsString(dr["STATE"]));
                        ei.Put("FAX_NO", Prolink.Math.GetValueAsString(dr["PARTY_FAX"]));
                        ei.Put("TAX_NO", Prolink.Math.GetValueAsString(dr["TAX_NO"]));
                        ml.Add(ei);
                    }
                }
                if (ml.Count > 0)
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("Import PartyType Error :" + ex.ToString() + " : PartyType = " + PARTY_TYPE + " : PartyNo = " + PARTY_NO);
                returnMessage = "Import PartyType Error";
            }
            return returnMessage;
        }

        #endregion


        #region C.C. Confirm

        public string CCConfirm(SMSMIModel smsmiModel, InboundBroker broker, UserInfo userInfo)
        {
            string returnMessage = "C.C. Confirm Success!";
            string ermsg = "";
            MixedList ml = new MixedList();
            string ShipmentId = smsmiModel.SHIPMENT_ID;
            try
            {

                MixedList ml2 = new MixedList();
                EditInstruct ei;
                EditInstruct ei2;
                DataTable cnDt = new DataTable();

                string TransType = smsmiModel.TRAN_TYPE;


                //string MasterNo = Prolink.Math.GetValueAsString(dr[2]);
                string CntrNo = smsmiModel.CNTR_NO;
                //string DnNo = Prolink.Math.GetValueAsString(dr[4]); 
                string DecNo = broker.DEC_NO;
                string ImportNo = broker.IMPORT_NO;
                DateTime DecDate = Prolink.Math.GetValueAsDateTime(broker.DEC_DATE);
                DateTime RelDate = Prolink.Math.GetValueAsDateTime(broker.REL_DATE);
                string Inspection = broker.INSPECTION;
                string CerNo = broker.CER_NO;
                string DecReply = broker.DEC_REPLY;
                string ICDF = broker.ICDF;
                string CcChannel = broker.CC_CHANNEL;
                string HsQty = broker.HS_QTY;
                string Country = broker.CNTRY_CD;
                string pli = broker.PLI;
                string li = broker.LI;
                string sufCost = broker.SUF_COST;
                string ccRate = broker.CC_RATE;
                string addQty = broker.ADD_QTY;
                string sisFee = broker.SIS_FEE;


                //string[] Dnlist = DnNo.Split(',');
                if (Inspection == "Yes" || Inspection == "YES")
                {
                    Inspection = "Y";
                }
                else if (Inspection == "No" || Inspection == "NO")
                {
                    Inspection = "N";
                }

                Func<string[], string[], string> checkIsEmpty = (column, descp) =>
                {
                    string msg = "";
                    for (int j = 0; j < column.Length; j++)
                    {
                        if (string.IsNullOrEmpty(column[j]))
                        {
                            msg += "," + descp[j];
                        }
                    }
                    if (!string.IsNullOrEmpty(msg))
                    {
                        return "ShipmentId :" + ShipmentId + msg + " is empty" + "\n";
                    }
                    return string.Empty;
                };

                #region 判断此笔ShipmentId的报关费用是否已经上传了
                string sql = string.Format("SELECT CMP FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
                string cmp = getOneValueAsStringFromSql(sql);
                sql = string.Format("SELECT COUNT(1) FROM SMBID WHERE SHIPMENT_ID={0} AND CMP={1} AND FSTATUS IN ('C','D')AND CHG_CD IN ('ICD','ICDF')", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp));
                int billcount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (billcount > 0)
                {
                    return string.Format("The Shipment {0} corresponding bill has been uploaded!", ShipmentId);
                }
                #endregion
                Func<EditInstruct, string> UpdateEi = (eii) =>
                {
                    eii.Put("DEC_NO", DecNo);
                    eii.Put("IMPORT_NO", ImportNo);
                    eii.PutDate("DEC_DATE", DecDate);
                    eii.PutDate("REL_DATE", RelDate);
                    eii.Put("INSPECTION", Inspection);
                    eii.Put("CER_NO", CerNo);
                    eii.Put("DEC_REPLY", DecReply);
                    eii.Put("ICDF", ICDF);
                    eii.Put("CC_CHANNEL", CcChannel);
                    eii.Put("HS_QTY", HsQty);
                    eii.Put("CNTRY_CD", Country);
                    if ("BR" == cmp)
                    {
                        eii.Put("PLI", pli);
                        eii.Put("LI", li);
                        eii.Put("SUF_COST", sufCost);
                        eii.Put("CC_RATE", ccRate);
                        eii.Put("ADD_QTY", addQty);
                        eii.Put("SIS_FEE", sisFee);
                    }
                    return string.Empty;
                };
                #region 判斷必填欄位
                switch (TransType)
                {
                    case "F":
                    case "R":
                        string[] columnList = new string[] { TransType, CntrNo, DecNo, DecDate.ToString(), RelDate.ToString() };
                        string[] descpList = new string[] { "Tran Type", "Container no.", "DECL NO", "CLEARANCE DATE", "RELEASE DATE" };
                        string msg = checkIsEmpty(columnList, descpList);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            ermsg += msg;
                            break;
                        }


                        ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentId);
                        ei.PutKey("CNTR_NO", CntrNo);
                        UpdateEi(ei);
                        ml.Add(ei);

                        ei2 = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        ei2.PutKey("SHIPMENT_ID", ShipmentId);
                        //ei2.PutKey("INV_NO", Dnlist[n].ToString());
                        UpdateEi(ei2);
                        ml.Add(ei2);

                        //if (Dnlist.Length > 0)
                        //{
                        //    for (int n = 0; n < Dnlist.Length; n++)
                        //    {
                        //        ei2 = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        //        ei2.PutKey("SHIPMENT_ID", ShipmentId);
                        //        ei2.PutKey("INV_NO", Dnlist[n].ToString());
                        //        UpdateEi(ei2);
                        //        ml.Add(ei2);
                        //    }
                        //}

                        break;
                    case "L":
                    case "A":
                    case "E":
                        string[] columnList2 = new string[] { TransType, DecNo, DecDate.ToString(), RelDate.ToString() };
                        string[] descpList2 = new string[] { "Tran Type", "DECL NO", "CLEARANCE DATE", "RELEASE DATE" };
                        string msg2 = checkIsEmpty(columnList2, descpList2);
                        if (!string.IsNullOrEmpty(msg2))
                        {
                            ermsg += msg2;
                            break;
                        }

                        ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", ShipmentId);
                        //ei.PutKey("INV_NO", Dnlist[a].ToString());
                        UpdateEi(ei);
                        ml.Add(ei);
                        //if (Dnlist.Length > 0)
                        //{
                        //    for (int a = 0; a < Dnlist.Length; a++)
                        //    {
                        //        ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                        //        ei.PutKey("SHIPMENT_ID", ShipmentId);
                        //        ei.PutKey("INV_NO", Dnlist[a].ToString());
                        //        UpdateEi(ei);
                        //        ml.Add(ei);
                        //    }
                        //}
                        break;
                }
                #endregion
                if (!string.IsNullOrEmpty(ermsg))
                    return ermsg;
                if (ml.Count > 0)
                {
                    int[] results = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                    SetDecInfoToSmsmi(TransType, ShipmentId);
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("ImportBrokerExcel Error :" + ex.ToString());
                return returnMessage;
            }

            Result result = DECLBookConfirmFunc(ShipmentId, broker.REL_DATE, userInfo);
            if (!result.Success)
            {
                returnMessage += ShipmentId + " " + result.Message;
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("ImportBrokerExcel Error :" + ShipmentId + "报关确认失败:" + result.Message);
            }
            return returnMessage;
        }

        private void SetDecInfoToSmsmi(string trantype, string shipmentid)
        {
            string sql = "";
            string delist = "";
            string decDateList = "";
            string relesDateList = "";

            if (trantype == "F" || trantype == "R")
            {
                sql = string.Format("SELECT DISTINCT ','+DEC_NO FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                delist = getOneValueAsStringFromSql(sql);

                sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), DEC_DATE, 23) FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                decDateList = getOneValueAsStringFromSql(sql);

                sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), REL_DATE, 23) FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                relesDateList = getOneValueAsStringFromSql(sql);
            }
            else
            {
                sql = string.Format("SELECT DISTINCT ','+DEC_NO FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                delist = getOneValueAsStringFromSql(sql);

                sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), DEC_DATE, 23) FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                decDateList = getOneValueAsStringFromSql(sql);

                sql = string.Format("SELECT DISTINCT ','+CONVERT(VARCHAR(100), REL_DATE, 23) FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_DATE IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(shipmentid));
                relesDateList = getOneValueAsStringFromSql(sql);
            }
            if (delist.Length > 500)
            {
                delist = delist.Substring(0, 500);
            }
            EditInstruct smei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            smei.PutKey("SHIPMENT_ID", shipmentid);
            smei.Put("DEC_INFO", delist.Length > 0 ? delist.Substring(1) : delist);
            smei.Put("DEC_DATE_INFO", decDateList.Length > 0 ? decDateList.Substring(1) : decDateList);
            smei.Put("TC_REL_DATE", relesDateList.Length > 0 ? relesDateList.Substring(1) : relesDateList);
            OperationUtils.ExecuteUpdate(smei, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetDecInfoToSmord(trantype, shipmentid);
        }


        private void SetDecInfoToSmord(string trantype, string shipmentid)
        {
            string sql = "";
            DataTable dt = new DataTable();
            MixedList mixList = new MixedList();
            if (trantype == "F" || trantype == "R")
            {
                sql = string.Format("SELECT DEC_NO,DEC_DATE,TC_DEC_NO,TC_DEC_DATE,CNTR_NO,REL_DATE FROM SMICNTR WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string cntrNo = Prolink.Math.GetValueAsString(dt.Rows[i]["CNTR_NO"]);
                    if (!string.IsNullOrEmpty(cntrNo))
                    {
                        EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        smordEi.PutKey("SHIPMENT_ID", shipmentid);
                        smordEi.PutKey("CNTR_NO", cntrNo);
                        smordEi.Put("DEC_NO", dt.Rows[i]["DEC_NO"]);
                        smordEi.PutDate("DEC_DATE", dt.Rows[i]["DEC_DATE"]);
                        smordEi.Put("TC_DEC_NO", dt.Rows[i]["TC_DEC_NO"]);
                        smordEi.PutDate("TC_DEC_DATE", dt.Rows[i]["TC_DEC_DATE"]);
                        smordEi.PutDate("REL_DATE", dt.Rows[i]["REL_DATE"]);
                        mixList.Add(smordEi);
                    }
                }
            }
            else
            {
                sql = string.Format("SELECT TOP 1 DEC_NO,DEC_DATE,TC_DEC_NO,TC_DEC_DATE,REL_DATE FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    EditInstruct smordEi = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    smordEi.PutKey("SHIPMENT_ID", shipmentid);
                    smordEi.Put("DEC_NO", dt.Rows[0]["DEC_NO"]);
                    smordEi.PutDate("DEC_DATE", dt.Rows[0]["DEC_DATE"]);
                    smordEi.Put("TC_DEC_NO", dt.Rows[0]["TC_DEC_NO"]);
                    smordEi.PutDate("TC_DEC_DATE", dt.Rows[0]["TC_DEC_DATE"]);
                    smordEi.PutDate("REL_DATE", dt.Rows[0]["REL_DATE"]);
                    mixList.Add(smordEi);
                }
            }
            if (mixList.Count > 0)
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }
        #endregion


        #region SlotTime Booked
        public void SoltTimeBooked(SMSMIModel smsmiModel, TruckCompany truck, DataRow dr, UserInfo userInfo, MixedList list)
        {
            bool isTsalog = false;
            bool isTsa = TrackingEDI.InboundBusiness.SMSMIHelper.IsTSALogistic(Prolink.Math.GetValueAsString(dr["CMP"]));
            if (isTsa)
            {
                isTsalog = "T".Equals(dr["STATUS"]) || "W".Equals(dr["STATUS"]);
            }
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_INFO", smsmiModel.SHIPMENT_ID);
            if (smsmiModel.TRAN_TYPE.Equals("F"))
            {
                ei.PutKey("CNTR_NO", smsmiModel.CNTR_NO);
            }
            
            ei.PutDate("USE_DATE", truck.USE_DATE);
            if ("D".Equals(dr["STATUS"]) || "R".Equals(dr["STATUS"]) || isTsalog)
            {
                ei.Put("ORDER_BY", userInfo.UserId);
                ei.PutDate("ORDER_DATE_L", truck.ARRIVAL_DATE);
                ei.Put("STATUS", "R");
            }
            list.Add(ei);
        }
        #endregion




        #region DnOrderTrucker
        public string InboundDnOrderTrucker(TruckCompany truck, DataRow smordRow, DataRow smrcntrRow, string CntType, List<string> EtaMsl, UserInfo userinfo, Dictionary<string, bool> shipmentDic, List<string> idList = null)
        {
            
            DateTime PickupDate = Prolink.Math.GetValueAsDateTime(truck.USE_DATE);
            string UserId = userinfo.UserId;
            //string Ext = userinfo.Ext;
            string GroupId = userinfo.GroupId;
            string CompanyId = userinfo.CompanyId;
            string Dep = userinfo.Dep;
            string LotNo = "S" + ReserveHelper.getAutoNo("SHIB_NO", GroupId, CompanyId);
            string returnMessage = "";
            string sql = "";

            string Trucker = string.Empty;
            string TruckerNm = string.Empty;
            string TranType = string.Empty;
            string WsCd = smrcntrRow["WS_CD"].ToString();
            string WsNm = string.Empty;
            string PickArea = string.Empty;
            string PickAreaNm = string.Empty;
            string ShipmentId = smrcntrRow["SHIPMENT_ID"].ToString();
            string location = string.Empty;
            string DnNo = smrcntrRow["DN_NO"].ToString();
            string InvNo = smrcntrRow["INV_NO"].ToString();
            string DivDescp = string.Empty;
            string PoNo = string.Empty;
            string podcd = string.Empty;
            string Wo = string.Empty;
            string SmCreateBy = string.Empty;
            string DepAddr = smordRow["DEP_ADDR"].ToString();
            string QuotNo = smordRow["QUOT_NO"].ToString();
            List<string> DnList = new List<string>();
            List<string> DivList = new List<string>();
            List<string> SmList = new List<string>();
            List<string> OrdList = new List<string>();
            List<string> DecList = new List<string>();
            List<string> MasterList = new List<string>();
            List<string> WSList = new List<string>();
            List<string> WNList = new List<string>();
            List<string> ShipperList = new List<string>();
            List<string> LspList = new List<string>();
            List<string> CarrierList = new List<string>();

            decimal SumGw = 0, SumCbm = 0;
            MixedList mixList = new MixedList();
            string isdirectlynb = string.Empty;
            string remark = smrcntrRow["INV_NO"].ToString();
            string addrcode = smrcntrRow["ADDR_CODE"].ToString();
            string dlvArea = smrcntrRow["DLV_AREA"].ToString();
            string dlvAreaNm = smrcntrRow["DLV_AREA_NM"].ToString();
            string OrdNo = smrcntrRow["ORD_NO"].ToString();
            //string dUId = smrcntrRow json["UId"].ToString();     
            string reserveNo = smrcntrRow["RESERVE_NO"].ToString();
            if (!string.IsNullOrEmpty(reserveNo))
            {
                returnMessage += "Shipment ID: " + ShipmentId + ";DN NO" + DnNo + L_GateManageController_Controllers_137 + "\n";
                return returnMessage;
            }

            //WsNm = smrcntrRow["WS_NM"].ToString();
            if (!WSList.Contains(WsCd))
            {
                WSList.Add(WsCd);
                //WNList.Add(WsNm);
            }

            string wsname = "";
            string wsnmsql = string.Format("SELECT WS_NM,DLV_ADDR,DLV_AREA,DLV_AREA_NM FROM SMWH WHERE CMP={0} AND WS_CD = {1}", SQLUtils.QuotedStr(CompanyId),
                SQLUtils.QuotedStr(WsCd));
            DataTable wsdt = OperationUtils.GetDataTable(wsnmsql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (wsdt.Rows.Count <= 0)
            {
                return " ShipmentId:" + ShipmentId + " warehouse is empty";
            }
            for (int i = 0; i < wsdt.Rows.Count; i++)
            {
                wsname += wsdt.Rows[i]["WS_NM"].ToString() + ",";
            }
            wsname = wsname.Trim(',');
            addrcode = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_ADDR"]);
            dlvArea = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_AREA"]);
            dlvAreaNm = Prolink.Math.GetValueAsString(wsdt.Rows[0]["DLV_AREA_NM"]);

            sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
            DataTable dt = CommonHelp.getDataTableFromSql(sql);

            if (dt.Rows.Count > 0)
            {
                Trucker = dt.Rows[0]["TRUCKER1"].ToString();
                TruckerNm = dt.Rows[0]["TRUCKER_NM1"].ToString();
                TranType = dt.Rows[0]["TRAN_TYPE1"].ToString();
                PickArea = dt.Rows[0]["POL1"].ToString();
                PickAreaNm = dt.Rows[0]["POL_NM1"].ToString();
                isdirectlynb = dt.Rows[0]["IS_DIRECTLYNB"].ToString();
                SmCreateBy = Prolink.Math.GetValueAsString(dt.Rows[0]["IB_WINDOW"]);
            }



            bool hIBCR = checkIBCR(shipmentDic, ShipmentId);

            if (!hIBCR)
            {
                returnMessage += "Shipment ID: " + ShipmentId + L_GateManageController_Controllers_146 + "\n";

                return returnMessage;
            }

            sql = "SELECT DIVISION_DESCP FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO={1}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(DnNo));
            DivDescp = CommonHelp.getOneValueAsStringFromSql(sql);
            string DecNo = smrcntrRow["DEC_NO"].ToString();

            if (!DnList.Contains(InvNo))
            {
                DnList.Add(InvNo);
            }

            if (!DivList.Contains(DivDescp))
            {
                DivList.Add(DivDescp);
            }

            if (!SmList.Contains(ShipmentId))
            {
                SmList.Add(ShipmentId);

                sql = "SELECT MASTER_NO,SH_NO+SH_NM AS SH_NO,LSP_NO+LSP_NM AS LSP_NO,CARRIER,POD_CD,CMP FROM SMSMI WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                DataTable smsmidt = CommonHelp.getDataTableFromSql(sql);
                if (smsmidt.Rows.Count > 0)
                {
                    podcd = smsmidt.Rows[0]["POD_CD"].ToString();
                    string MasterNo = smsmidt.Rows[0]["MASTER_NO"].ToString();
                    if (!MasterList.Contains(MasterNo))
                    {
                        MasterList.Add(MasterNo);
                    }
                    string shno = smsmidt.Rows[0]["SH_NO"].ToString();
                    if (!ShipperList.Contains(shno))
                    {
                        ShipperList.Add(shno);
                    }
                    string lspno = smsmidt.Rows[0]["LSP_NO"].ToString();
                    if (!LspList.Contains(lspno))
                    {
                        LspList.Add(lspno);
                    }
                    string carrier = smsmidt.Rows[0]["CARRIER"].ToString();
                    if (!CarrierList.Contains(carrier))
                    {
                        CarrierList.Add(carrier);
                    }

                    location = smsmidt.Rows[0]["CMP"].ToString();
                }
            }

            if (!OrdList.Contains(OrdNo))
            {
                OrdList.Add(OrdNo);
            }
            if (!DecList.Contains(DecNo))
            {
                DecList.Add(DecNo);
            }
            decimal Gw = Convert.ToDecimal(smrcntrRow["GW"]);
            string Gwu = smrcntrRow["GWU"].ToString();
            decimal gu = Convert.ToDecimal(Prolink.Math.unitConvert(Gwu, "KG"));
            Gw = Gw * gu;
            SumGw += Gw;

            decimal Cbm = Convert.ToDecimal(smrcntrRow["CBM"]);
            SumCbm += Cbm;



            EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            smordei.PutKey("ORD_NO", OrdNo);


            EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
            smidnei.PutKey("SHIPMENT_ID", ShipmentId);
            //smidnei.PutKey("DN_NO", DnNo);

            smidnei.PutDate("PICKUP_CDATE", PickupDate);
            smordei.PutDate("PICKUP_CDATE", PickupDate);

            if (EtaMsl != null)
            {
                if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                {
                    smidnei.Put("CALL_TRUCK_STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
                    smordei.Put("CSTATUS", 'R');

                }
                else
                {
                    smidnei.Put("CALL_TRUCK_STATUS", 'D');
                    //smordei1.Put("CSTATUS", 'D');
                }
                if (EtaMsl.Count > 0 && !string.IsNullOrEmpty(EtaMsl[0]))
                {
                    smordei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));
                    smidnei.PutDate("ETA_MSL", Business.DateTimeUtils.ParseToDateTime(EtaMsl[0]));

                }
                if (EtaMsl.Count > 1 && !string.IsNullOrEmpty(EtaMsl[1]))
                {
                    smordei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));
                    smidnei.Put("ETA_MSL_TIME", Convert.ToString(EtaMsl[1]));

                }
            }
            mixList.Add(smordei);
            mixList.Add(smidnei);

            //mixList.Add(smordei1);

            //將PO NO從SMIDN寫入SMRDN
            sql = "SELECT PO_NO,WO FROM SMIDN WHERE SHIPMENT_ID={0} AND DN_NO={1} ORDER BY SCMREQUEST_DATE ASC";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(smrcntrRow["DN_NO"].ToString()));
            DataTable dt1 = CommonHelp.getDataTableFromSql(sql);
            if (dt1.Rows.Count > 0)
            {
                foreach (DataRow dr in dt1.Rows)
                {
                    string PN = Prolink.Math.GetValueAsString(dr["PO_NO"]);
                    PoNo += PN + ",";
                    string wo = Prolink.Math.GetValueAsString(dr["WO"]);
                    Wo += wo + ",";
                }
            }
            if (!string.IsNullOrEmpty(PoNo))
            {

                PoNo = PoNo.Remove(PoNo.Length - 1);
            }
            if (!string.IsNullOrEmpty(Wo))
            {

                Wo = Wo.Remove(Wo.Length - 1);
            }
            string CarType = string.Empty;
            if (CntType == "20GP")
            {
                CarType = "F4";
            }
            else if (CntType == "40GP")
            {
                CarType = "F5";
            }
            else if (CntType == "40HQ")
            {
                CarType = "F6";
            }
            if (string.IsNullOrEmpty(CarType))
            {
                string rnfsql = string.Format("SELECT CHG_CD FROM ECREFFEE WHERE (CMP ={0} OR CMP = '*') AND FEE_TYPE = 'O' AND CHG_CD NOT IN('F1','F2','F3') AND CHG_DESCP={1}",
                     SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(CntType));
                CarType = OperationUtils.GetValueAsString(rnfsql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }


            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.INSERT_OPERATION);
            string UId = System.Guid.NewGuid().ToString();
            string ReserveNo = ReserveHelper.getAutoNo("RV_NO", GroupId, CompanyId);
            ei.Put("U_ID", UId);
            ei.Put("RESERVE_NO", ReserveNo);

            if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
            {
                ei.Put("STATUS", 'R');      //直接由Directly Notify Broker 执行的，叫车的时候直接转愉悦
            }
            else
            {
                ei.Put("STATUS", 'D');
            }
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", location);
            ei.Put("DN_NO", String.Join(",", DnList.ToArray()));
            ei.Put("INVOICE_INFO", String.Join(",", DnList.ToArray()));
            ei.Put("PRODUCT_TYPE", String.Join(",", DivList.ToArray()));
            ei.Put("SHIPMENT_INFO", String.Join(",", SmList.ToArray()));
            ei.Put("SHIPPER", String.Join(",", ShipperList.ToArray()));
            ei.Put("FOWARDER", String.Join(",", LspList.ToArray()));
            ei.Put("CARRIER", String.Join(",", CarrierList.ToArray()));
            ei.Put("DEP", Dep);
            ei.Put("CREATE_BY", UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

            ei.PutDate("CREATE_DATE", odt.ToString("yyyyMMddHHmmss"));
            ei.PutDate("CREATE_DATE_L", ndt.ToString("yyyyMMddHHmmss"));
            ei.Put("CREATE_CMP", CompanyId);
            ei.Put("CREATE_DEP", Dep);
            // ei.Put("CREATE_EXT", Ext);
            ei.PutDate("CALL_DATE", odt.ToString("yyyyMMddHHmmss"));
            ei.PutDate("CALL_DATE_L", ndt.ToString("yyyyMMddHHmmss"));

            ei.PutDate("USE_DATE", PickupDate);
            ei.Put("DEP_ADDR", DepAddr);
            ei.Put("TRUCKER", Trucker);
            ei.Put("TRUCKER_NM", TruckerNm);
            ei.Put("TRAN_TYPE", TranType);
            ei.Put("GW", SumGw);
            ei.Put("GWU", "KGS");
            ei.Put("CBM", SumCbm);
            ei.Put("LOT_NO", LotNo);
            ei.Put("BAT_NO", ReserveNo);
            ei.Put("CAR_TYPE", CarType);
            ei.Put("TRS_MODE", "Y");
            ei.Put("RV_TYPE", "I");
            ei.Put("CALL_TYPE", "D");
            ei.Put("ORD_INFO", String.Join(",", OrdList.ToArray()));
            ei.Put("WS_CD", string.Join(",", WSList));
            ei.Put("WS_NM", string.Join(",", WNList));

            ei.Put("ADDR_CODE", addrcode);
            ei.Put("DLV_AREA", dlvArea);
            ei.Put("DLV_AREA_NM", dlvAreaNm);
            ei.Put("PICK_AREA", PickArea);
            ei.Put("PICK_AREA_NM", PickAreaNm);
            ei.Put("QUOT_NO", QuotNo);
            ei.Put("DEC_INFO", String.Join(",", DecList.ToArray()));
            ei.Put("CNTR_NO", String.Join(",", MasterList.ToArray()));
            ei.Put("PRIORITY", GetPriority(SmList.ToArray()));
            ei.Put("SMCREATE_BY", SmCreateBy);

            DateTime arrivaltime = Prolink.Math.GetValueAsDateTime(truck.ARRIVAL_DATE);
            //string dUId = smrcntrRow["U_ID"].ToString();
            string AddPoint = smrcntrRow["ADD_POINT"].ToString();

            if (!OrdList.Contains(OrdNo))
            {
                OrdList.Add(OrdNo);
            }
            sql = "UPDATE SMRDN SET PICKUP_DATE={0}, ARRIVAL_DATE={1}, WS_CD={2}, RESERVE_NO={3}, ADD_POINT={4}, LOT_NO={5} WHERE ORD_NO={6}";
            sql = string.Format(sql, SQLUtils.QuotedStr(PickupDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(arrivaltime.ToString("yyyy-MM-dd")),
                SQLUtils.QuotedStr(WsCd), SQLUtils.QuotedStr(ReserveNo), SQLUtils.QuotedStr(AddPoint), SQLUtils.QuotedStr(LotNo),
                SQLUtils.QuotedStr(OrdNo));
            mixList.Add(sql);
            if (DateTime.Compare(arrivaltime, DateTime.MinValue) > 0)
            {
                ei.PutDate("RESERVE_DATE", arrivaltime);
                ei.Put("RESERVE_FROM", arrivaltime.Hour);
            }
            ei.Put("PO_NO", PoNo);
            ei.Put("WO", Wo);
            List<string> asnList = new List<string>();
            List<string> partList = new List<string>();
            List<int> qtyList = new List<int>();
            string psql = string.Format("SELECT ASN_NO,PART_NO,IPART_NO,QTY FROM SMIDNP WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(SmList.ToArray()));
            DataTable inpDt = OperationUtils.GetDataTable(psql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in inpDt.Rows)
            {
                string asnNo = Prolink.Math.GetValueAsString(dr["ASN_NO"]);
                string partNo = Prolink.Math.GetValueAsString(dr["PART_NO"]);
                int qty = Prolink.Math.GetValueAsInt(dr["QTY"]);
                if (string.IsNullOrEmpty(partNo))
                {
                    partNo = Prolink.Math.GetValueAsString(dr["IPART_NO"]);
                }
                if (!string.IsNullOrEmpty(partNo))
                {
                    partList.Add(partNo);
                    qtyList.Add(qty);
                    asnList.Add(asnNo);
                }
            }
            ei.Put("ASNNO_INFO", getValueByCol(asnList.ToArray()));
            ei.Put("PARTNO_INFO", getValueByCol(partList.ToArray()));
            ei.Put("PART_QTY", getValueByCol(qtyList.ToArray()));
            string csCd = "", csNm = "", csName = "", bu = "";
            List<string> csCdList = new List<string>();
            List<string> csNmList = new List<string>();
            List<string> csNameList = new List<string>();
            List<string> buList = new List<string>();
            string ptSql = string.Format("SELECT Y.ABBR,Y.DEP,Y.PARTY_NAME,T.PARTY_NO,T.PARTY_TYPE FROM SMPTY Y LEFT JOIN SMSMIPT T ON T.PARTY_NO=Y.PARTY_NO WHERE T.SHIPMENT_ID IN {0} AND T.PARTY_TYPE IN ('CS','ZT')", SQLUtils.Quoted(SmList.ToArray()));
            DataTable ptyDt = OperationUtils.GetDataTable(ptSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (ptyDt.Rows.Count > 0)
            {
                foreach (DataRow dr in ptyDt.Rows)
                {
                    string partyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    switch (partyType)
                    {
                        case "CS":
                            csCd = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                            csName = Prolink.Math.GetValueAsString(dr["ABBR"]);
                            csNm = Prolink.Math.GetValueAsString(dr["PARTY_NAME"]);
                            if (!csCdList.Contains(csCd) && !string.IsNullOrEmpty(csCd))
                            {
                                csCdList.Add(csCd);
                                csNameList.Add(csName);
                                csNmList.Add(csNm);
                            }
                            break;
                        case "ZT":
                            bu = Prolink.Math.GetValueAsString(dr["DEP"]);
                            if (!buList.Contains(bu) && !string.IsNullOrEmpty(bu))
                            {
                                buList.Add(bu);
                            }
                            break;
                    }
                }
            }
            csCd = string.Join(",", csCdList);
            if (csCd.Length > 350)
                csCd.Substring(0, 350);
            csName = string.Join(",", csNameList);
            if (csName.Length > 350)
                csName.Substring(0, 350);
            csNm = string.Join(",", csNmList);
            if (csNm.Length > 500)
                csNm.Substring(0, 500);
            bu = string.Join(",", buList);
            if (bu.Length > 100)
                bu.Substring(0, 100);
            ei.Put("CS_CD", csCd);
            ei.Put("CS_NAME", csName);
            ei.Put("CS_NM", csNm);
            ei.Put("BU", bu);
            TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(ShipmentId, ei);
            mixList.Add(ei);

            try
            {
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idList != null && !idList.Contains(UId))
                    idList.Add(UId);
                TrackingEDI.InboundBusiness.SMSMIHelper.GetDivisonBySMR(ReserveNo, TranType);
                TrackingEDI.InboundBusiness.SMSMIHelper.UpdateSMICUFT(ReserveNo, true);
                foreach (string shipment in SmList)
                {
                    TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = ShipmentId, StsCd = "018", Cmp = CompanyId, Remark = remark, Sender = UserId, Location = podcd, LocationName = "", StsDescp = "Order Truck By DN" });
                }
                for (int i = 0; i < OrdList.Count; i++)
                {
                    int r = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE (RESERVE_NO IS NOT NULL OR RESERVE_NO <> '') AND ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));
                    int c = CommonHelp.getOneValueAsIntFromSql("SELECT COUNT(*) FROM SMRDN WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdList[i]));

                    if (r == c)
                    {
                        //更新運輸單的Cstatus
                        sql = "UPDATE SMORD SET CSTATUS='D' WHERE ORD_NO={0}";
                        if ("Y".Equals(isdirectlynb) || "T".Equals(isdirectlynb))
                        {
                            sql = "UPDATE SMORD SET CSTATUS='R' WHERE ORD_NO={0}";
                        }
                        sql = string.Format(sql, SQLUtils.QuotedStr(OrdList[i]));
                        CommonHelp.exeSql(sql);
                    }
                }


                for (int i = 0; i < SmList.Count; i++)
                {
                    string updateArrivateDatetoSmord = SetArrivalDateToSMORD(SmList[i]);
                    CommonHelp.exeSql(updateArrivateDatetoSmord);
                    CalculateFee cs = new CalculateFee(ShipmentId);
                    Bill.WriteLogTagStart("dn叫车自动计价", SmList[i]);
                    List<string> emptyMessage = new List<string>();
                    cs.FindTrailerQuote(ReserveNo, SmList[i], emptyMessage);
                    InboundTransfer.UpdateBillInfoToSMORD(SmList[i], "", null);
                    Bill.WriteLogTagStart("结束计算", SmList[i]);
                }


            }
            catch (Exception ex)
            {
                returnMessage += ex.ToString();
            }
            return returnMessage;
        }

        public static bool checkIBCR(Dictionary<string, bool> shipmentDic, string ShipmentId)
        {
            bool hIBCR = false;
            if (!shipmentDic.ContainsKey(ShipmentId))
            {
                DataTable ptdt = GetPTByPartyType(ShipmentId, "IBCR");
                foreach (DataRow dr in ptdt.Rows)
                {
                    string partyNo = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    if (!string.IsNullOrEmpty(partyNo))
                    {
                        hIBCR = true;
                    }
                }
                shipmentDic.Add(ShipmentId, hIBCR);
            }
            else
            {
                hIBCR = shipmentDic[ShipmentId];
            }
            return hIBCR;
        }

        public static string getValueByCol(string[] colList)
        {
            string col = string.Join(",", colList.ToArray());
            if (col.Length > 500)
                col = col.Substring(0, 500);
            return col;
        }

        public static string getValueByCol(int[] colList)
        {
            string col = string.Join(",", colList.ToArray());
            if (col.Length > 500)
                col = col.Substring(0, 500);
            return col;
        }


        public static string SetArrivalDateToSMORD(string shipmentid, MixedList ml = null)
        {
            string sql = string.Format("SELECT TRAN_TYPE FROM SMSMI WHERE SHIPMENT_ID={0} ",
                SQLUtils.QuotedStr(shipmentid));
            string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string scmSql = string.Empty;
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE
                 SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMICNTR.CNTR_NO=SMORD.CNTR_NO),PRIORITY=(SELECT MIN(PRIORITY) FROM SMICNTR WHERE
                 SMICNTR.SHIPMENT_ID=SMORD.SHIPMENT_ID AND SMICNTR.CNTR_NO=SMORD.CNTR_NO) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));

                scmSql = string.Format(@"UPDATE SMSMI SET PRODUCTION_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMICNTR WHERE  
                SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            else
            {
                sql = string.Format(@"UPDATE SMORD SET ARRIVAL_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE
                 SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID),PRIORITY=(SELECT MIN(PRIORITY) FROM SMIDN WHERE
                 SMIDN.SHIPMENT_ID=SMORD.SHIPMENT_ID) WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentid));

                scmSql = string.Format(@"UPDATE SMSMI SET PRODUCTION_DATE=(SELECT MIN(SCMREQUEST_DATE) FROM SMIDN WHERE  
                SMIDN.SHIPMENT_ID=SMSMI.SHIPMENT_ID) WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            }
            if (ml == null)
            {
                OperationUtils.ExecuteUpdate(scmSql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else
                ml.Add(scmSql);
            return sql;
        }
        #endregion



        public string AfterLSPConfirmDoDecl(DataTable maindt, UserInfo userinfo)
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

            returnMsg = SendDeclaration(maindt, type, userinfo, "", false);

            string sql = string.Format("SELECT BSTATUS,STATUS,IB_WINDOW,CMP,GROUP_ID,U_ID,SHIPMENT_ID,IS_TRANSIT_BROKER FROM SMSMI WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable smsmidt = getDataTableFromSql(sql);
            string bstatus = Prolink.Math.GetValueAsString(smsmidt.Rows[0]["BSTATUS"]);
            if (BROKER_STATUS_Y_NotifyBroker != bstatus || BROKER_STATUS_H_NotifyTransitBroker != bstatus)
            {
                if (!returnMsg.Contains(L_BookingActionController_Controllers_64))
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

        public string SendDeclaration(DataTable maindt, string partytype, UserInfo userinfo, string isdirectlynb = "", bool isBath = false, bool checkIBCR = false)
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
                    returnMsg += nullmsg + L_BookingAction_Controllers_165;
                    continue;
                }
                if (status != "C" && status != "Z" && "Y" != isdirectlynb && "T" != isdirectlynb)
                {
                    if (BROKER_STATUS_I_TransitConfirm != brokerstatus)
                    {
                        returnMsg += nullmsg + M_SMSMI_ERRORMSG_01;
                        continue;
                    }
                }
                if (brokerstatus.Equals("C"))
                {
                    returnMsg += nullmsg + L_DNInfoCheck_Business_68;
                    continue;
                }
                string mailType = string.Empty;
                string groupType = string.Empty;
                if (partytype == "IBBR")
                {
                    if (BROKER_STATUS_C_BrokerConfirm.Equals(brokerstatus) || BROKER_STATUS_H_NotifyTransitBroker.Equals(brokerstatus))
                    {
                        returnMsg += nullmsg + L_DNInfoCheck_Business_68;
                        continue;
                    }
                    if ("F".Equals(status))
                    {
                        returnMsg += nullmsg + L_DNInfoCheck_Business_69;
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
                    returnMsg += nullmsg + L_BookingAction_Controllers_144;
                    continue;
                }
                if (checkIBCR)
                {
                    DataRow[] ibcars = partytable.Select("PARTY_TYPE='IBCR'");
                    if (ibcars.Length <= 0)
                    {
                        returnMsg += nullmsg + L_BookingAction_Controllers_144;
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
                nullmsg += L_BookingActionController_Controllers_64;
                returnMsg += nullmsg;
            }

            if (isBath && idList.Count > 0)
            {
                SendBookingOrCallMailList(idList.ToArray(), "INS", partytype, true);
            }
            return returnMsg;
        }

        public void DoDeclaration(DataTable smdt, UserInfo userinfo)
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
        public void CreateDeclaration(string uId, string shipmentId, UserInfo userinfo, string delist)
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
            decEi.Put("EDOC_TYPE", "DO");
            try
            {
                OperationUtils.ExecuteUpdate(decEi, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception)
            {
                throw;
            }
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

        public string SendBrokerMail(string partyno, string mailType, string GroupId, string partytype, DataRow smRow, string groupType)
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
                return L_BookingActionController_Controllers_47 + partyno + L_BookingActionController_Controllers_48;
            }
            subtital = shipmentid + L_BookingAction_Controllers_167;
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

        public static string UpdateBorder(string uid, string Type, DataRow smdr, UserInfo userinfo)
        {
            string status = smdr["STATUS"].ToString();
            string returnMsg = string.Empty;
            string shipmentid = smdr["SHIPMENT_ID"].ToString();
            string cmp = smdr["CMP"].ToString();
            string pod = smdr["POD_CD"].ToString();
            string IBwindow = Prolink.Math.GetValueAsString(smdr["IB_WINDOW"]);
            string UId = userinfo.UserId;
            if ("O".Equals(userinfo.IoFlag))
                UId = IBwindow;
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);

            if (Type == "IBBR")
            {
                if ("A".Equals(status) || "B".Equals(status) || "C".Equals(status) || "I".Equals(status))
                {
                    apei.Put("STATUS", 'C');
                }
                apei.Put("BSTATUS", BROKER_STATUS_Y_NotifyBroker);
            }
            else if (Type == "IBTC")
            {
                if ("A".Equals(status) || "B".Equals(status) || "C".Equals(status))
                {
                    apei.Put("STATUS", 'H');
                }
                apei.Put("BSTATUS", BROKER_STATUS_H_NotifyTransitBroker);
            }
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, userinfo.CompanyId);
            apei.PutDate("LSP_CONFIRM_DATE", ndt);
            apei.Put("LSP_CONFIRM_BY", userinfo.UserId);

            mixList.Add(apei);

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

        public void SendBookingOrCallMailList(string[] uids, string even_type = "INS", string partyType = "", bool isNotifyBroker = false)
        {


            DataTable smsmipt = null;
            DataTable smrv = null;
            switch (even_type)
            {
                case "ICS":
                case "IICS":
                    smrv = OperationUtils.GetDataTable(string.Format("SELECT U_ID,SHIPMENT_ID,SHIPMENT_INFO FROM SMIRV WITH(NOLOCK) WHERE U_ID IN {0}", SQLUtils.Quoted(uids)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    List<string> smList = new List<string>();
                    foreach (DataRow dr in smrv.Rows)
                    {
                        string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        if (!string.IsNullOrEmpty(shipment_id) && !smList.Contains(shipment_id))
                            smList.Add(shipment_id);
                        shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                        if (string.IsNullOrEmpty(shipment_id))
                            continue;
                        string[] temps = shipment_id.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var temp in temps)
                        {
                            if (!smList.Contains(temp))
                                smList.Add(temp);
                        }
                    }

                    smsmipt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID IN (SELECT U_ID FROM SMSMI WHERE SHIPMENT_ID IN {0}))A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP,SHIPMENT_ID FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(smList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                case "INS":
                    string insSql = string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE IN('IBBR','IBCR','IBSP') AND U_FID IN {0})A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(uids));
                    if (!string.IsNullOrEmpty(partyType))
                    {
                        insSql = string.Format("SELECT * FROM (SELECT PARTY_TYPE,PARTY_NO,PARTY_NAME,U_FID FROM SMSMIPT WITH(NOLOCK) WHERE PARTY_TYPE={1} AND U_FID IN {0})A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,GROUP_ID,CMP FROM SMSMI M WITH (NOLOCK) WHERE M.U_ID=A.U_FID)B", SQLUtils.Quoted(uids), SQLUtils.QuotedStr(partyType));
                    }
                    smsmipt = OperationUtils.GetDataTable(insSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
            }

            List<string> tranTypes = new List<string>();
            List<string> lspLsit = new List<string>();
            foreach (DataRow dr in smsmipt.Rows)
            {
                string party_no = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                if (!string.IsNullOrEmpty(party_no) && !lspLsit.Contains(party_no))
                    lspLsit.Add(party_no);
                string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                if (!string.IsNullOrEmpty(tranType) && !tranTypes.Contains(tranType))
                    tranTypes.Add(tranType);
            }

            MixedList ml = new MixedList();
            foreach (var tranType in tranTypes)
            {
                foreach (var lspNo in lspLsit)
                {
                    DataRow[] drs = smsmipt.Select(string.Format("PARTY_NO={0} AND TRAN_TYPE={1}", SQLUtils.QuotedStr(lspNo), SQLUtils.QuotedStr(tranType)));
                    if (drs.Length <= 0)
                        continue;
                    List<string> idList = new List<string>();
                    if (smrv != null)
                    {
                        foreach (var dr in drs)
                        {
                            string smid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                            DataRow[] rvs = smrv.Select(string.Format("SHIPMENT_ID={0} OR SHIPMENT_INFO LIKE {1}", SQLUtils.QuotedStr(smid), SQLUtils.QuotedStr("%" + smid + "%")));
                            foreach (var rv in rvs)
                            {
                                string fid = Prolink.Math.GetValueAsString(rv["U_ID"]);
                                if (idList.Contains(fid))
                                    continue;
                                idList.Add(fid);
                            }
                        }
                    }
                    else
                    {

                        foreach (var dr in drs)
                        {
                            string fid = Prolink.Math.GetValueAsString(dr["U_FID"]);
                            if (idList.Contains(fid))
                                continue;
                            idList.Add(fid);
                        }
                    }

                    string groupId = Prolink.Math.GetValueAsString(drs[0]["GROUP_ID"]);
                    string cmp = Prolink.Math.GetValueAsString(drs[0]["CMP"]);
                    string party_name = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                    string guid = Guid.NewGuid().ToString();
                    string subject = isNotifyBroker ? L_BookingAction_Controllers_167 : GetSubJectByTranType(tranType, tranType, "");
                    EvenFactory.AddEven(string.Format("{0}#{1}#{2}#{3}#{4}#{5}", tranType, lspNo, groupId, cmp, guid, partyType), guid, even_type, ml, 1, 0, party_name, subject, string.Join(";", idList));//mailcc
                }
            }
            if (ml.Count > 0)
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

        }

        private static string GetSubJectByTranType(string trantype, string term, string cancel = "", int sendtimes = 0)
        {
            string subject = string.Empty;
            string notice = " Booking Notice ";
            if (sendtimes > 0)
                notice = " Revise(" + sendtimes + ") Booking Notice ";
            switch (trantype)
            {
                case "A":
                    subject = "Air (" + term + ")" + cancel + notice;
                    break;
                case "L":
                    subject = "LCL (" + term + ")" + cancel + notice;
                    break;
                case "R":
                    subject = "RailWay (" + term + ")" + cancel + notice;
                    break;
                case "F":
                    subject = "FCL (" + term + ")" + cancel + notice;
                    break;
                case "E":
                    subject = "Express (" + term + ")" + cancel + notice;
                    break;
                case "D":
                    subject = "Express (" + term + ")" + cancel + notice;
                    break;
                case "T":
                    subject = "Domestic (" + term + ")" + cancel + notice;
                    break;
                default:
                    subject = "(" + term + ")" + cancel + notice;
                    break;
            }
            return subject;
        }

        public Result DECLBookConfirmFunc(string ShipmentId, DateTime? declrlsdate, UserInfo info)
        {
            MixedList mixedlist = new MixedList();
            Result result = new Result();
            string smsmisql = string.Format(@"SELECT TRAN_TYPE,BSTATUS,U_ID,CMP,POD_CD,SHIPMENT_ID,STATUS FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId));
            DataTable dt = getDataTableFromSql(smsmisql);
            if (dt.Rows.Count <= 0)
            {
                result.Success = false;
                result.Message = "No Data ,So you cann't do C.C Confirm！";
                return result;
            }
            string u_id = dt.Rows[0]["U_ID"].ToString();
            string TranType = dt.Rows[0]["TRAN_TYPE"].ToString();
            string status = dt.Rows[0]["STATUS"].ToString();
            string smsmiBstatus = dt.Rows[0]["BSTATUS"].ToString();
            string pod = dt.Rows[0]["POD_CD"].ToString();
            string cmp = dt.Rows[0]["CMP"].ToString();
            string sql = string.Empty;

            result = CheckBrokerConfirm(dt.Rows[0], info);
            if (!result.Success)
            {
                return result;
            }
            string bsctatus = BROKER_STATUS_C_BrokerConfirm;
            string smsmistatus = "D";
            if (declrlsdate != null)
            { //有放行时间，将状态更新为放行
                bsctatus = BROKER_STATUS_F_Release; 
            }

            EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", u_id);
            ei.Put("MODIFY_BY", info.UserId);
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, info.CompanyId);

            ei.PutDate("MODIFY_DATE", odt);
            ei.PutDate("MODIFY_DATE_L", ndt);
            ei.Put("BROKER_CONFIRM_BY", info.UserId);
            ei.PutDate("BROKER_CONFIRM_DATE", ndt);

            if ("Y".Equals(smsmiBstatus))
            {
                ei.Put("BSTATUS", bsctatus);
            }

            if ("C".Equals(status))     //只有notify broker的时候才能将状态更新成报关确认
            {
                ei.Put("STATUS", smsmistatus);
            }
            mixedlist.Add(ei);

            try
            {
                int[] results = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                CalculateFee cf = new CalculateFee(ShipmentId);
                List<string> emptyMessage = new List<string>();
                Bill.WriteLogTagStart("报关确认计算报关费用", ShipmentId);
                cf.CalBrokerFee(ShipmentId, emptyMessage);
                InboundTransfer.UpdateBillInfoToSMORD(ShipmentId, "", null);
                Bill.WriteLogTagStart(string.Join(";", emptyMessage), ShipmentId);
                Bill.WriteLogTagStart("结束计算", ShipmentId);

                TrackingEDI.Manager.IBSaveStatus(new Status() { ShipmentId = ShipmentId, StsCd = "046", Cmp = info.CompanyId, Sender = info.UserId, Location = pod, LocationName = "", StsDescp = "Broker Confirm" });

                #region 報關確認時更新dec no 到SMORD
                List<string> DecList = new List<string>();
                string delist = string.Empty;
                if (TranType == "F" || TranType == "R")
                {
                    sql = string.Format("SELECT DISTINCT DEC_NO+',' FROM SMICNTR WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
                    delist = getOneValueAsStringFromSql(sql);
                }
                else
                {
                    sql = string.Format("SELECT DISTINCT DEC_NO+',' FROM SMIDN WHERE SHIPMENT_ID={0} AND DEC_NO IS NOT NULL FOR XML PATH('')", SQLUtils.QuotedStr(ShipmentId));
                    delist = getOneValueAsStringFromSql(sql);
                }

                sql = "UPDATE SMORD SET DEC_INFO={0},BSTATUS={1},STATUS={2} WHERE SHIPMENT_ID={3}";
                sql = string.Format(sql, SQLUtils.QuotedStr(delist),
                    SQLUtils.QuotedStr(bsctatus), SQLUtils.QuotedStr(smsmistatus),
                    SQLUtils.QuotedStr(ShipmentId));
                exeSql(sql);

                sql = "SELECT RESERVE_NO, SHIPMENT_INFO FROM SMIRV WHERE STATUS <> 'V' AND SHIPMENT_INFO LIKE '%{0}%'";
                sql = string.Format(sql, ShipmentId);
                DataTable smdt = getDataTableFromSql(sql);
                if (smdt.Rows.Count > 0)
                {
                    foreach (DataRow smdr in smdt.Rows)
                    {
                        string ShipmentInfo = Prolink.Math.GetValueAsString(smdr["SHIPMENT_INFO"]);
                        string ReserveNo = Prolink.Math.GetValueAsString(smdr["RESERVE_NO"]);
                        sql = "SELECT DEC_NO FROM SMIDN WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentInfo.Split(','));
                        DataTable dndt = getDataTableFromSql(sql);

                        if (dndt.Rows.Count > 0)
                        {
                            foreach (DataRow dndr in dndt.Rows)
                            {
                                string DecNo = Prolink.Math.GetValueAsString(dndr["DEC_NO"]);

                                if (!DecList.Contains(DecNo))
                                {
                                    DecList.Add(DecNo);
                                }
                            }
                        }

                        sql = "SELECT DEC_NO,NEW_SEAL,SHIPMENT_ID FROM SMICNTR WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(ShipmentInfo.Split(','));
                        DataTable cntrdt = getDataTableFromSql(sql);
                        string NewSeal = string.Empty;
                        if (cntrdt.Rows.Count > 0)
                        {
                            foreach (DataRow cntrdr in cntrdt.Rows)
                            {
                                string DecNo = Prolink.Math.GetValueAsString(cntrdr["DEC_NO"]);
                                string shipment_id = Prolink.Math.GetValueAsString(cntrdr["SHIPMENT_ID"]);
                                if (ShipmentId == shipment_id)
                                {
                                    NewSeal = Prolink.Math.GetValueAsString(cntrdr["NEW_SEAL"]);
                                }
                                if (!DecList.Contains(DecNo))
                                {
                                    DecList.Add(DecNo);
                                }
                            }
                        }

                        sql = "UPDATE SMIRV SET DEC_INFO={0},NEW_SEAL={1} WHERE RESERVE_NO={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(String.Join(",", DecList.ToArray())), SQLUtils.QuotedStr(NewSeal), SQLUtils.QuotedStr(ReserveNo));
                        exeSql(sql);

                    }
                }
                #endregion

                #region 報關確認時更dec no 到 SMRDN, SMRCNTR
                sql = "UPDATE SMRDN SET SMRDN.DEC_NO=SMIDN.DEC_NO, SMRDN.REL_DATE=SMIDN.REL_DATE FROM SMIDN WHERE SMIDN.DN_NO=SMRDN.DN_NO AND SMIDN.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                exeSql(sql);

                sql = "UPDATE SMRCNTR SET SMRCNTR.DEC_NO=SMICNTR.DEC_NO,SMRCNTR.REL_DATE=SMICNTR.REL_DATE FROM SMICNTR WHERE SMICNTR.CNTR_NO=SMRCNTR.CNTR_NO AND SMICNTR.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                exeSql(sql);
                #endregion

                if ("BR" == cmp)
                {
                    UserInfo userInfo = new UserInfo { UserId = info.UserId, CompanyId = info.CompanyId, GroupId = info.GroupId };
                    CreateDeclaration(u_id, ShipmentId, userInfo, delist);
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
                result.Success = false;
                return result;
            }
            result.Message = L_BookingActionController_Controllers_68 + ShipmentId + L_BookingAction_Controllers_170;
            result.Success = true;
            return result;
        }

        private Result CheckBrokerConfirm(DataRow smsmidr, UserInfo info)
        {


            Result result = new Result();
            string TranType = Prolink.Math.GetValueAsString(smsmidr["TRAN_TYPE"]);
            string bstatus = Prolink.Math.GetValueAsString(smsmidr["BSTATUS"]);
            string pod = Prolink.Math.GetValueAsString(smsmidr["POD_CD"]);
            string shipmentid = Prolink.Math.GetValueAsString(smsmidr["SHIPMENT_ID"]);
            string sql = string.Empty;
            if ("O".Equals(info.IoFlag))
            {
                sql = string.Format("SELECT COUNT(1) FROM SMSMIPT WHERE SHIPMENT_ID={0}  AND PARTY_TYPE='IBBR'",
                        SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(info.CompanyId));
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (count <= 0)
                {
                    result.Success = false;
                    result.Message = "No permission to operated!";
                    return result;
                }
            }

            string countmessage = " DN";
            string table = "SMIDN";
            if (TranType == "F" || TranType == "R")
            {
                table = "SMICNTR";
                countmessage = " Container";
            }
            sql = string.Format(@"SELECT COUNT(1) FROM {0} WHERE SHIPMENT_ID={1}
            AND (DEC_DATE IS NULL OR  DEC_DATE ='' OR REL_DATE='' OR IMPORT_NO='' OR INSPECTION='' OR
            REL_DATE IS NULL OR IMPORT_NO IS NULL OR INSPECTION IS NULL )", table, SQLUtils.QuotedStr(shipmentid));
            int nodatacount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            int hasdatacount = 0;
            if (nodatacount > 0)
            {
                sql = string.Format(@"SELECT COUNT(1) FROM {0} WHERE SHIPMENT_ID={1}
                AND DEC_DATE IS NOT NULL AND REL_DATE IS NOT NULL AND IMPORT_NO IS NOT NULL AND INSPECTION IS NOT NULL",
                 table, SQLUtils.QuotedStr(shipmentid));
                hasdatacount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (hasdatacount >= 1)
                {
                    result.Message = L_BookingActionController_Controllers_66 + shipmentid +
                        " Has " + hasdatacount.ToString() + countmessage + " Exp. C/C Successful And Also Has " + nodatacount.ToString() + countmessage + " Exp. C/C Failure!";
                    result.Success = false;
                    return result;
                }
            }

            sql = string.Format("SELECT * FROM " + table + " WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (mainDt.Rows.Count > 0)
            {
                foreach (DataRow dr in mainDt.Rows)
                {
                    string DecDate = Prolink.Math.GetValueAsString(dr["DEC_DATE"]);
                    string RelDate = Prolink.Math.GetValueAsString(dr["REL_DATE"]);
                    string ImportNo = Prolink.Math.GetValueAsString(dr["IMPORT_NO"]);
                    string Inspection = Prolink.Math.GetValueAsString(dr["INSPECTION"]);

                    if (DecDate == "")
                    {
                        result.Message = L_BookingActionController_Controllers_66 + shipmentid + "[Declaration Date] not fill";
                        result.Success = false;
                        return result;
                    }

                    if (RelDate == "")
                    {
                        result.Message = L_BookingActionController_Controllers_66 + shipmentid + "[Release Date] not fill";
                        result.Success = false;
                        return result;
                    }

                    if (ImportNo == "")
                    {
                        result.Message = L_BookingActionController_Controllers_66 + shipmentid + "[Import No.] not fill";
                        result.Success = false;
                        return result;
                    }

                    //if (Inspection == "")
                    //{
                    //    result.Message = L_BookingActionController_Controllers_66 + shipmentid + "[Inspection] not fill";
                    //    result.Success = false;
                    //    return result;
                    //}
                }

            }

            if (bstatus.Equals("C"))
            {
                result.Message = L_BookingActionController_Controllers_66 + shipmentid + L_BookingAction_Controllers_168;
                result.Success = false;
                return result;
            }
            if ("N".Equals(bstatus))
            {
                result.Message = L_BookingActionController_Controllers_66 + shipmentid + L_BookingActionController_Controllers_67;
                result.Success = false;
                return result;
            }
            result.Success = true;
            return result;
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

        public string GateInConfirm(DataTable smirvDt, TruckCompany truck)
        {
            string returnMessage = "";
            MixedList ml = new MixedList();
            //List<string> thirdPartyReserveno = new List<string>();
            //List<string> normallist = new List<string>();
            List<string> AllInsertTrackingSM = new List<string>();
            string shippment_info = string.Empty;
            string reserve_no = string.Empty;
            string call_type = string.Empty;
            string ord_no = string.Empty;
            string ord_info = string.Empty;
            string uid = string.Empty;
            string cmp = string.Empty;
            string podcd = string.Empty;
            if (smirvDt != null && smirvDt.Rows.Count > 0)
            {
                call_type = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["CALL_TYPE"]);
                reserve_no = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["RESERVE_NO"]);
                shippment_info = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["SHIPMENT_INFO"]);
                ord_no = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["ORD_NO"]);
                ord_info = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["ORD_INFO"]);
                uid = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["U_ID"]);
                cmp = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["CMP"]);
            }
            string[] shipmentids = shippment_info.Split(',');
            string[] ordnos = ord_info.Split(',');


            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("STATUS", "I");
            DateTime odt = Prolink.Math.GetValueAsDateTime(truck.IN_DATE_L);
            DateTime ndt = Prolink.Math.GetValueAsDateTime(truck.IN_DATE_L);

            ei.PutDate("IN_DATE", odt);
            ei.PutDate("IN_DATE_L", ndt);
            ei.Put("TRUCK_NO", truck.TRUCK_NO);
            ei.Put("DRIVER", truck.DRIVER);
            ei.Put("DRIVER_ID", truck.DRIVER_ID);
            ei.Put("TEL", truck.TEL);
            ei.Put("LTRUCK_NO", truck.LTRUCK_NO);
            ei.Put("LDRIVER", truck.LDRIVER);
            ei.Put("LDRIVER_ID", truck.LDRIVER_ID);
            ei.Put("LTEL", truck.LTEL);
            ml.Add(ei);

            EditInstruct ei2 = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
            if ("D".Equals(call_type))
                ei2 = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
            ei2.PutKey("RESERVE_NO", reserve_no);
            ei2.PutDate("IDATE", odt);
            ei2.PutDate("IDATE_L", ndt);
            ml.Add(ei2);

            string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
            string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
            string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.IDATE={0}, SMICNTR.IDATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
            SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            if ("D".Equals(call_type))
            {
                updatesql = string.Format("UPDATE SMIDN SET SMIDN.IDATE={0}, SMIDN.IDATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
                SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            }
            ml.Add(updatesql);

            for (int j = 0; j < shipmentids.Length; j++)
            {
                string shippment_id = shipmentids[j];
                if (!AllInsertTrackingSM.Contains(shippment_id))
                {
                    AllInsertTrackingSM.Add(shippment_id);
                }
                if ("D".Equals(call_type))
                {
                    string smrdnsql = string.Format(@"SELECT COUNT(1) FROM (
                SELECT DISTINCT DN_NO FROM SMRDN WHERE SHIPMENT_ID LIKE '%{0}%' AND LOT_no IS NOT NULL) SMRDN", shippment_id);
                    int rdncount = OperationUtils.GetValueAsInt(smrdnsql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    string smidnsql = string.Format(@"SELECT COUNT(1) FROM (
                SELECT DISTINCT DN_NO FROM SMIDN WHERE SHIPMENT_ID LIKE '%{0}%') SMRDN", shippment_id);
                    int idncount = OperationUtils.GetValueAsInt(smidnsql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (rdncount == idncount)
                    {
                        UpdateCstatus(ml, ord_no, ordnos);
                    }
                }
                else if ("D" != call_type)
                {
                    UpdateCstatus(ml, ord_no, ordnos);
                }

                string gateSMRVSQL = "SELECT STATUS FROM SMIRV WHERE SHIPMENT_INFO like '%" + shippment_id + "%' and STATUS != 'V' and RESERVE_NO !='" + reserve_no + "'";
                string gateSMRV2SQL = "SELECT ARRIVAL_FACT_DATE FROM SMIRV WHERE RESERVE_NO ='" + reserve_no + "'";
                DataTable SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable SMRV2Dt = OperationUtils.GetDataTable(gateSMRV2SQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string IsGateIn = "Y";
                EditInstruct ei3 = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                if (SMRVDt.Rows.Count > 0)
                {
                    foreach (DataRow dr2 in SMRV2Dt.Rows)
                    {
                        DateTime Arrival_Fact_Date = Prolink.Math.GetValueAsDateTime(dr2["ARRIVAL_FACT_DATE"]);
                        if (Arrival_Fact_Date.Year == 1)
                        {
                            ei3.PutKey("RESERVE_NO", reserve_no);
                            ei3.PutDate("ARRIVAL_FACT_DATE", odt);
                            ei3.PutDate("ARRIVAL_FACT_DATE_L", ndt);
                            ml.Add(ei3);
                        }
                    }
                    foreach (DataRow dr in SMRVDt.Rows)
                    {
                        string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                        if (status == "A" || status == "D" || status == "R" || status == "C")
                        {
                            IsGateIn = "N";
                        }
                    }
                    if (IsGateIn == "Y")
                    {
                        string gateSMSMISQL = string.Format("SELECT STATUS,POD_CD FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
                        DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (SMSMIDt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in SMSMIDt.Rows)
                            {
                                string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                                if (string.IsNullOrEmpty(podcd))
                                {
                                    podcd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                                }
                                if (status != "X")
                                {
                                    ei4.PutKey("SHIPMENT_ID", shippment_id);
                                    ei4.Put("STATUS", "G");
                                    ml.Add(ei4);
                                }
                            }
                        }
                    }
                }
                else
                {
                    gateSMRVSQL = "SELECT ARRIVAL_FACT_DATE FROM SMIRV WHERE RESERVE_NO ='" + reserve_no + "'";
                    SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (SMRVDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in SMRVDt.Rows)
                        {
                            DateTime Arrival_Fact_Date = Prolink.Math.GetValueAsDateTime(dr["ARRIVAL_FACT_DATE"]);
                            if (Arrival_Fact_Date.Year == 1)
                            {
                                ei3.PutKey("RESERVE_NO", reserve_no);
                                ei3.PutDate("ARRIVAL_FACT_DATE", odt);
                                ei3.PutDate("ARRIVAL_FACT_DATE_L", ndt);
                                ml.Add(ei3);
                            }
                        }
                    }
                    string gateSMSMISQL = string.Format("SELECT STATUS FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
                    DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (SMSMIDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in SMSMIDt.Rows)
                        {
                            string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                            if (status != "X")
                            {
                                ei4.PutKey("SHIPMENT_ID", shippment_id);
                                ei4.Put("STATUS", "G");
                                ml.Add(ei4);
                            }
                        }
                    }
                }
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (string shipment in AllInsertTrackingSM)
                    {
                        TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = shipment, StsCd = "035", Cmp = cmp, Sender = _UserId, Location = podcd, LocationName = "", StsDescp = "In Factory" });
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }

            return returnMessage;
        }

        private static void UpdateCstatus(MixedList ml, string ord_no, string[] ordnos, string status = "I")
        {
            EditInstruct ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            if (!string.IsNullOrEmpty(ord_no))
            {
                ei.PutKey("ORD_NO", ord_no);
                if ("O".Equals(status) && IsSMORDChangeToFinish(ord_no))
                {
                    ei.Put("CSTATUS", "Z");
                }
                else
                {
                    ei.Put("CSTATUS", status);
                }
                ml.Add(ei);
            }
            else
            {
                for (int k = 0; k < ordnos.Length; k++)
                {
                    if (k > 0)
                        ei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("ORD_NO", ordnos[k]);
                    if ("O".Equals(status) && IsSMORDChangeToFinish(ordnos[k]))
                    {
                        ei.Put("CSTATUS", "Z");
                    }
                    else
                    {
                        ei.Put("CSTATUS", status);
                    }
                    ml.Add(ei);
                }
            }
        }

        private static bool IsSMORDChangeToFinish(string ord_no)
        {
            string sql = string.Format(@"SELECT * FROM (SELECT ORD_NO,TRAN_TYPE,(SELECT COUNT(1) FROM SMRCNTR WHERE EMPTY_TIME IS NULL AND SMRCNTR.ORD_NO=SMORD.ORD_NO)AS EMPTYNULL FROM SMORD )T
                        WHERE EMPTYNULL>0 AND TRAN_TYPE IN ('F','R')  AND ORD_NO={0}", SQLUtils.QuotedStr(ord_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                int isnullcount = Prolink.Math.GetValueAsInt(dt.Rows[0]["EMPTYNULL"]);
                if (isnullcount > 0)
                    return false;
            }
            return true;
        }

        private static string GetOuterFlagInfo(string reserve_no)
        {
            string outterflagSql = string.Format(@"SELECT TOP 10 OUTER_FLAG
						FROM BSADDR
						WHERE BSADDR.ADDR_CODE in(SELECT top 1 ADDR_CODE
						FROM SMRCNTR
						WHERE SMRCNTR.RESERVE_NO ={0}
						UNION
						SELECT top 1 ADDR_CODE
						FROM SMRDN
						WHERE SMRDN.RESERVE_NO={0}) order by OUTER_FLAG desc", SQLUtils.QuotedStr(reserve_no));
            string outterflag = OperationUtils.GetValueAsString(outterflagSql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return outterflag;
        }

        public string GateOutConfirm(DataTable smrvDt, TruckCompany truck)
        {
            string shippment_info = string.Empty;
            string reserve_no = string.Empty;
            string call_type = string.Empty;
            string ord_no = string.Empty;
            string ord_info = string.Empty;
            string uid = string.Empty;
            //List<string> thirdPartyReserveno = new List<string>(); 
            //List<string> normallist = new List<string>();
            List<string> AllInsertTrackingSM = new List<string>();
            MixedList ml = new MixedList();
            string returnMessage = "";
            if (smrvDt != null && smrvDt.Rows.Count > 0)
            {
                call_type = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["CALL_TYPE"]);
                reserve_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["RESERVE_NO"]);
                shippment_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["SHIPMENT_INFO"]);
                ord_no = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_NO"]);
                ord_info = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["ORD_INFO"]);
                uid = Prolink.Math.GetValueAsString(smrvDt.Rows[0]["U_ID"]);
            }
            string[] s = shippment_info.Split(',');
            string[] ordnos = ord_info.Split(',');

            //string outterflag = GetOuterFlagInfo(reserve_no);
            //if ("Y".Equals(outterflag))
            //{
            //    thirdPartyReserveno.Add(reserve_no); 
            //}
            //normallist.Add(reserve_no);


            string gateOutSQL = string.Format("SELECT POD_MDATE,CNTR_NO,DN_NO FROM SMRCNTR WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            if ("D".Equals(call_type))
                gateOutSQL = string.Format("SELECT POD_MDATE,'' AS CNTR_NO,DN_NO FROM SMRDN WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));

            DataTable smDt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count > 0)
            {
                if (smDt.Select(string.Format("POD_MDATE IS NULL")).Length == smDt.Rows.Count)
                    return "No POD{edoc Type: POD}, no Abnormal {edoc type: Abnormal pic} for container/DN{" + Prolink.Math.GetValueAsString(smDt.Rows[0]["CNTR_NO"]) + "/" + Prolink.Math.GetValueAsString(smDt.Rows[0]["DN_NO"]) + "} , cannot Gate Out factory. Pls check with WH/Warehouse for help.";
            }

            string gateFWHSQL = string.Format("SELECT b.FINAL_WH FROM SMRCNTR as a, BSADDR as b WHERE a.ADDR_CODE=b.ADDR_CODE AND RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            if ("D".Equals(call_type))
                gateFWHSQL = string.Format("SELECT b.FINAL_WH FROM SMRDN as a, BSADDR as b WHERE a.ADDR_CODE=b.ADDR_CODE AND RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            DataTable FWHDt = OperationUtils.GetDataTable(gateFWHSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (FWHDt.Rows.Count > 0)
            {
                string IsFinal = "Y";
                for (int j = 0; j < s.Length; j++)
                {
                    string shippment_id = s[j];
                    if (!AllInsertTrackingSM.Contains(shippment_id))
                    {
                        AllInsertTrackingSM.Add(shippment_id);
                    }

                    foreach (DataRow dr in FWHDt.Rows)
                    {
                        string FINAL_WH = Prolink.Math.GetValueAsString(dr["FINAL_WH"]);
                        if (FINAL_WH == "Temp")
                        {
                            UpdateCstatus(ml, ord_no, ordnos, "T");
                            IsFinal = "N";
                        }
                    }
                    if (IsFinal == "Y")
                    {
                        UpdateCstatus(ml, ord_no, ordnos, "O");
                    }
                }
            }

            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            if (IsSMRVChangeToFinish(uid))
            {
                ei.Put("STATUS", "Z");
            }
            else
            {
                ei.Put("STATUS", "O");
            }
            DateTime odt = Prolink.Math.GetValueAsDateTime(truck.OUT_DATE_L);
            DateTime ndt = Prolink.Math.GetValueAsDateTime(truck.OUT_DATE_L);

            ei.PutDate("OUT_DATE", odt);
            ei.PutDate("OUT_DATE_L", ndt);
            ml.Add(ei);

            EditInstruct ei2 = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
            if ("D".Equals(call_type))
                ei2 = new EditInstruct("SMRDN", EditInstruct.UPDATE_OPERATION);
            ei2.PutKey("RESERVE_NO", reserve_no);
            ei2.PutDate("LDATE", odt);
            ei2.PutDate("LDATE_L", ndt);
            ml.Add(ei2);

            string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
            string newdate = ndt.ToString("yyyy-MM-dd hh:mm:ss");
            string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.ODATE={0}, SMICNTR.ODATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            if ("D".Equals(call_type))
            {
                updatesql = string.Format("UPDATE SMIDN SET SMIDN.ODATE={0}, SMIDN.ODATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
               SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(newdate), SQLUtils.QuotedStr(reserve_no));
            }
            ml.Add(updatesql);


            for (int j = 0; j < s.Length; j++)
            {
                string shippment_id = s[j];
                string gateSMRVSQL = "SELECT STATUS FROM SMIRV WHERE SHIPMENT_INFO like '%" + shippment_id + "%' and STATUS != 'V' and RESERVE_NO !='" + reserve_no + "'";
                DataTable SMRVDt = OperationUtils.GetDataTable(gateSMRVSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string IsGateOut = "Y";
                EditInstruct ei3 = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                if (SMRVDt.Rows.Count > 0)
                {
                    foreach (DataRow dr in SMRVDt.Rows)
                    {
                        string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                        if (status != "O" && status != "Z")
                        {
                            IsGateOut = "N";
                        }
                    }
                    if (IsGateOut == "Y")
                    {
                        UpdateSMSMIStatus(ml, shippment_id);
                    }
                }
                else
                {
                    UpdateSMSMIStatus(ml, shippment_id);
                }
            }
            if (smrvDt != null && smrvDt.Rows.Count > 0)
            {
                DataRow item = smrvDt.Rows[0];
                string CntrNo = Prolink.Math.GetValueAsString(item["CNTR_NO"]);
                string TruckCntrno = Prolink.Math.GetValueAsString(item["TRUCK_CNTRNO"]);
                string TruckNo = Prolink.Math.GetValueAsString(item["TRUCK_NO"]);
                string GateNo = Prolink.Math.GetValueAsString(item["GATE_NO"]);
                string Cmp = Prolink.Math.GetValueAsString(item["CMP"]);
                string WsCd = Prolink.Math.GetValueAsString(item["WS_CD"]);
                string BatNo = Prolink.Math.GetValueAsString(item["BAT_NO"]);

                if (CntrNo == "" && TruckCntrno == "")
                {
                    CntrNo = TruckNo;
                }
                else if (CntrNo == "")
                {
                    CntrNo = TruckCntrno;
                }
                string sql = "UPDATE SMWHGT SET CNTR_NO=NULL,RESERVE_NO=NULL WHERE CMP=" + SQLUtils.QuotedStr(Cmp) + " AND WS_CD=" + SQLUtils.QuotedStr(WsCd);
                ml.Add(sql);
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql1 = string.Format("SELECT CMP,POD_CD,SHIPMENT_ID FROM SMSMI WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(AllInsertTrackingSM.ToArray()));
                    DataTable dt = getDataTableFromSql(sql1);
                    foreach (DataRow dr in dt.Rows)
                    {
                        TrackingEDI.Manager.IBSaveStatus(new TrackingEDI.Model.Status() { ShipmentId = dr["SHIPMENT_ID"].ToString(), StsCd = "100", Cmp = dr["CMP"].ToString(), Sender = _UserId, Location = dr["POD_CD"].ToString(), LocationName = "", StsDescp = "Leave Factory" });
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            //if (normallist.Count > 0)
            //{
            //    returnMessage += " Appointment No.:" + string.Join(";", normallist) + " successed!";
            //}


            //if (thirdPartyReserveno.Count > 0)
            //{
            //    returnMessage += " Appointment No.:" + string.Join(";", thirdPartyReserveno) + " is in third party WareHouse!";
            //}
            return returnMessage;
        }

        private static bool IsSMRVChangeToFinish(string smrvuid)
        {
            string sql = string.Format(@"SELECT * FROM (SELECT (SELECT TOP 1 TRAN_TYPE FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS TRAN_TYPE
            ,(SELECT COUNT(1) FROM SMRCNTR WHERE EMPTY_TIME IS NULL AND SMRCNTR.ORD_NO=SMIRV.ORD_NO)AS EMPTYNULL
            FROM SMIRV WHERE U_ID={0})T  WHERE EMPTYNULL>0 AND TRAN_TYPE IN ('F','R') ", SQLUtils.QuotedStr(smrvuid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                int isnullcount = Prolink.Math.GetValueAsInt(dt.Rows[0]["EMPTYNULL"]);
                if (isnullcount > 0)
                    return false;
            }
            return true;
        }

        private static void UpdateSMSMIStatus(MixedList ml, string shippment_id)
        {
            string gateSMSMISQL = string.Format(@"SELECT STATUS,SHIPMENT_ID ,TRAN_TYPE,
                (SELECT COUNT(1) FROM SMICNTR WHERE EMPTY_TIME IS NULL AND SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID)AS EMPTYNULL
                FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shippment_id));
            DataTable SMSMIDt = OperationUtils.GetDataTable(gateSMSMISQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (SMSMIDt.Rows.Count > 0)
            {
                foreach (DataRow dr in SMSMIDt.Rows)
                {
                    string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                    if (status != "X")
                    {
                        EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                        ei4.PutKey("SHIPMENT_ID", shippment_id);
                        int count = Prolink.Math.GetValueAsInt(dr["EMPTYNULL"]);
                        string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                        if ("F".Equals(trantype) || "R".Equals(trantype))
                        {
                            if (count == 0)
                                ei4.Put("STATUS", "Z");
                            else
                                ei4.Put("STATUS", "O");
                        }
                        else
                        {
                            ei4.Put("STATUS", "Z");
                        }
                        ml.Add(ei4);
                    }
                }
            }
        }


        public string EmptyReturnConfirm(SMSMIModel smsmiModel, TruckCompany truck)
        {
            DataTable portDt = null;
            MixedList list = new MixedList();
            List<string> msg = new List<string>();
            List<string> msg1 = new List<string>();

            List<string> names = new List<string>() { "RESERVE_NO", "CNTR_NO" };

            EditInstruct ei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
            string reserve_no = truck.RESERVE_NO;
            string cntrNo = smsmiModel.CNTR_NO;
            if (string.IsNullOrEmpty(smsmiModel.CNTR_NO) || string.IsNullOrEmpty(reserve_no))
                return "";
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("CNTR_NO", cntrNo);
            string back_location = truck.BACK_LOCATION;
            if (!string.IsNullOrEmpty(back_location))
            {
                back_location = back_location.ToUpper();
                if (portDt == null)
                    portDt = OperationUtils.GetDataTable("SELECT PORT_CD FROM BSTPORT", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (portDt.Select(string.Format("PORT_CD={0}", SQLUtils.QuotedStr(back_location))).Length > 0)
                {
                    list.Add(string.Format("UPDATE SMIRV SET BACK_LOCATION={0} WHERE RESERVE_NO={1}", SQLUtils.QuotedStr(back_location), SQLUtils.QuotedStr(reserve_no)));
                    list.Add(string.Format("UPDATE SMICNTR SET SMICNTR.BACK_LOCATION={1} FROM SMRCNTR WHERE SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no), SQLUtils.QuotedStr(back_location)));
                }
                else
                {
                    msg1.Add(string.Format("【{0}】 is not in truck port,update fail", back_location));
                }

                if (msg1.Count == 0 || msg1 == null)
                {
                    ei.PutDate("EMPTY_TIME", truck.EMPTY_TIME);
                    list.Add(ei);
                    UpdateEmptyDate(reserve_no, _UserId, null, list);
                }
            }
            else
                msg1.Add("Empty Return Location or Empty Return Date is null");

            if (msg1.Count > 0)
                msg.Add(string.Join(";", msg1));
            try
            {
                if (list.Count > 0 && (msg.Count == 0 || msg == null))
                {
                    OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //msg.Add("Empty Return Date update success");
                }
            }
            catch (Exception e)
            {
                msg.Add("update fail:" + e.Message);
            }
            return string.Join("\r\n", msg);
        }

        public static void UpdateEmptyDate(string reserve_no, string userId, string back_location = "", MixedList mixList = null)
        {
            if (string.IsNullOrEmpty(reserve_no))
                return;
            bool update = false;
            if (mixList == null)
            {
                update = true;
                mixList = new MixedList();
            }
            if (back_location == null)
                back_location = string.Empty;
            else if (!string.IsNullOrEmpty(back_location))
                back_location = string.Format(",BACK_LOCATION={0}", SQLUtils.QuotedStr(back_location));
            else
                back_location = string.Format(",BACK_LOCATION=(SELECT TOP 1 BACK_LOCATION FROM SMIRV WHERE RESERVE_NO={0})", SQLUtils.QuotedStr(reserve_no));

            string sql = string.Format("SELECT STATUS,SHIPMENT_INFO FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string shipmentinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);//XMB1706036451,XMB1706035598

            string[] shipments = shipmentinfo.Split(',');
            foreach (string shipment in shipments)
            {
                sql = string.Format(@"SELECT STATUS,SHIPMENT_ID ,TRAN_TYPE,
                (SELECT COUNT(1) FROM SMICNTR WHERE EMPTY_TIME IS NULL AND SMICNTR.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMICNTR.CNTR_NO not in (
				SELECT CNTR_NO FROM SMRCNTR WHERE SMRCNTR.RESERVE_NO={0}))AS EMPTYNULL
                FROM SMSMI WHERE SHIPMENT_ID={1} AND TRAN_TYPE IN ('F','R')", SQLUtils.QuotedStr(reserve_no), SQLUtils.QuotedStr(shipment));
                DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                bool change = true;
                if (smdt.Rows.Count > 0)
                {
                    foreach (DataRow dr in smdt.Rows)
                    {
                        string status1 = Prolink.Math.GetValueAsString(dr["STATUS"]);
                        if (status1 != "X")
                        {
                            EditInstruct ei4 = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                            ei4.PutKey("SHIPMENT_ID", shipment);
                            int count = Prolink.Math.GetValueAsInt(dr["EMPTYNULL"]);
                            string trantype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                            if ("F".Equals(trantype) || "R".Equals(trantype))
                            {
                                if (count > 0)
                                    change = false;
                            }
                            ei4.Put("STATUS", "Z");
                            if (change)
                            {
                                mixList.Add(ei4);
                            }
                        }
                    }
                }
                EditInstruct AutoCalculTaskEi = new EditInstruct("AUTO_IBCALCUL_TASK", EditInstruct.INSERT_OPERATION);
                AutoCalculTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
                AutoCalculTaskEi.Put("SHIPMENT_ID", shipment);
                AutoCalculTaskEi.Put("RESERVE_NO", reserve_no);
                AutoCalculTaskEi.Put("DONE", "N");
                AutoCalculTaskEi.Put("CREATE_BY", userId);
                AutoCalculTaskEi.PutDate("CREATE_DATE", DateTime.Now);
                mixList.Add(AutoCalculTaskEi);
            }

            if ("O".Equals(status) || "Z".Equals(status))
            {
                mixList.Add(string.Format("UPDATE SMIRV SET STATUS='Z',EMPTY_TIME=(SELECT TOP 1 EMPTY_TIME FROM SMRCNTR WHERE RESERVE_NO={0}) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
                mixList.Add(string.Format("UPDATE SMORD SET CSTATUS='Z' WHERE SMORD.ORD_NO IN (SELECT ORD_NO FROM SMIRV WHERE RESERVE_NO={0})", SQLUtils.QuotedStr(reserve_no)));
            }
            else
            {
                mixList.Add(string.Format("UPDATE SMIRV SET EMPTY_TIME=(SELECT TOP 1 EMPTY_TIME FROM SMRCNTR WHERE RESERVE_NO={0}) WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
            }
            mixList.Add(string.Format("UPDATE SMICNTR SET SMICNTR.DISCHARGE_DATE=SMRCNTR.DISCHARGE_DATE,SMICNTR.EMPTY_TIME=SMRCNTR.EMPTY_TIME{1} FROM SMRCNTR WHERE SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no), back_location));
            mixList.Add(string.Format("UPDATE SMIDN SET SMIDN.DISCHARGE_DATE=SMRDN.DISCHARGE_DATE FROM SMRDN WHERE SMRDN.INV_NO=SMIDN.INV_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no)));
            if (update)
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
        }


        public string BatchUploadPOD(TruckCompany truck, string cmp, DataTable smirvDt)
        {
            string reserve_no = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["RESERVE_NO"]);
            string jobNo = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["U_ID"]);
            string wh = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["WS_CD"]);
            string call_type = Prolink.Math.GetValueAsString(smirvDt.Rows[0]["CALL_TYPE"]);

            string msg = string.Empty;
            try
            {
                string sql = string.Format("SELECT SHIPMENT_ID,SHIPMENT_INFO,IBAT_NO FROM SMIRV WHERE RESERVE_NO={0}", SQLUtils.QuotedStr(reserve_no));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    UploadPODHandle(truck, jobNo, reserve_no, wh, call_type);
                    string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                    if (string.IsNullOrEmpty(shipmentid))
                    {
                        shipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);
                        string[] shipments = shipmentid.Split(',');
                        if (shipments.Length > 0)
                            shipmentid = shipments[0];
                    }
                    sql = string.Format("select tran_type from smsmi where smsmi.shipment_id ={0}", SQLUtils.QuotedStr(shipmentid));
                    string trantype = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if ("T".Equals(trantype))
                    {
                        string ibatno = Prolink.Math.GetValueAsString(dt.Rows[0]["IBAT_NO"]);
                        if (!string.IsNullOrEmpty(ibatno))
                        {
                            string smirvsql = string.Format("SELECT U_ID,RESERVE_NO,CALL_TYPE FROM SMIRV WHERE IBAT_NO={0} AND RESERVE_NO!={1}",
                           SQLUtils.QuotedStr(ibatno), SQLUtils.QuotedStr(reserve_no));
                            DataTable smirvdt = OperationUtils.GetDataTable(smirvsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            foreach (DataRow dr in smirvdt.Rows)
                            {
                                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                                string reserveno = Prolink.Math.GetValueAsString(dr["RESERVE_NO"]);
                                string calltype = Prolink.Math.GetValueAsString(dr["CALL_TYPE"]);
                                UploadPODHandle(truck, uid, reserveno, wh, calltype);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }
            return msg;
        }

        public static void UploadPODHandle(TruckCompany truck, string EdocjobNo, string reserve_no, string wh, string call_type)
        {
            MixedList ml = new MixedList();
            DateTime odt = Prolink.Math.GetValueAsDateTime(truck.POD_UPDATE_DATE);

            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("RV_TYPE", "I");
            ei.Put("POD_CHECK", "Y");
            ei.PutDate("POD_UPDATE_DATE", odt);
            //if (checkPODstatus(reserve_no, EdocjobNo))
                ei.Put("STATUS", "U");
            ml.Add(ei);

            UpdateSMORDStatusAtPod(ml, ei, reserve_no, wh, call_type);

            string table = CallTypeIsByDn(call_type) ? "SMRDN" : "SMRCNTR";
            ei = new EditInstruct(table, EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RESERVE_NO", reserve_no);
            ei.PutKey("WS_CD", wh);
            ei.PutDate("POD_MDATE", odt);
            ei.PutDate("POD_MDATE_L", odt);
            ei.Put("POD_CHECK", "Y");
            ml.Add(ei);
            string nowdate = odt.ToString("yyyy-MM-dd hh:mm:ss");
            string updatesql = string.Format("UPDATE SMICNTR SET SMICNTR.POD_DATE={0}, SMICNTR.POD_DATE_L={1} FROM SMRCNTR WHERE SMRCNTR.CNTR_NO=SMICNTR.CNTR_NO AND SMRCNTR.SHIPMENT_ID=SMICNTR.SHIPMENT_ID AND SMRCNTR.RESERVE_NO={2}",
                SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(reserve_no));
            if (CallTypeIsByDn(call_type))
            {
                updatesql = string.Format("UPDATE SMIDN SET SMIDN.POD_DATE={0}, SMIDN.POD_DATE_L={1} FROM SMRDN WHERE SMRDN.DN_NO=SMIDN.DN_NO AND SMRDN.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND SMRDN.RESERVE_NO={2}",
               SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(nowdate), SQLUtils.QuotedStr(reserve_no));
            }
            ml.Add(updatesql);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            UpdateInboundSMSMStatus(reserve_no, "I", "RV", "U");
        }

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

        public static void UpdateSMORDStatusAtPod(MixedList ml, EditInstruct rvei, string reserve_no, string warehouse, string calltype)
        {
            DataTable smrvDetail = GetSMRVDetail(warehouse, reserve_no, calltype);
            if (smrvDetail == null || smrvDetail.Rows.Count <= 0)
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
                        if ("F" != trantype && "R" != trantype)
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
                    filter = string.Format(" SHIPMENT_ID IN {0}", SQLUtils.Quoted(smlist.ToArray()));
                }

                DataTable smsmiDt = OperationUtils.GetDataTable("SELECT U_ID,O_UID FROM SMSMI WITH(NOLOCK) WHERE " + filter, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in smsmiDt.Rows)
                {
                    TrackingEDI.InboundBusiness.SMSMIHelper.InboundsetLight(Prolink.Math.GetValueAsString(dr["U_ID"]), Prolink.Math.GetValueAsString(dr["O_UID"]), "I");
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

        public static List<string> GetSmrvShipmentList(string id, string type)
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

        public static string GetPriority(string[] shipmentids)
        {
            if (shipmentids.Length <= 0) return string.Empty;
            string sql = string.Format(@"SELECT MIN(PRIORITY) FROM (SELECT PRIORITY FROM SMICNTR WHERE SHIPMENT_ID IN {0} 
                            UNION  SELECT PRIORITY FROM SMIDN WHERE SHIPMENT_ID IN {0}) T", SQLUtils.Quoted(shipmentids)); ;
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
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

        public static bool CallTypeIsByDn(string calltype)
        {
            if (calltype == CallType_ByDN)
                return true;
            return false;
        }


        public class UserInfo : BaseUserInfo
        {
            public string Upri { get; set; }
            public string Dep { get; set; }
            public string DataCmp { get; set; }
            public string basecondtions { get; set; }

            public string IoFlag { get; set; }
        }
    }
}
