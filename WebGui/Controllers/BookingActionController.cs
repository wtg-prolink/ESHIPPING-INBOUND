using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using WebGui.App_Start;
using System.Collections.Specialized;
using Business.Mail;
using EDOCApi;
using System.Net.Mail;
using Business;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using Models.EDI;
using Business.Utils;
using Business.Service;
using Business.TPV;
using TrackingEDI;
using TrackingEDI.Model;
using System.IO;
using System.Web;

namespace WebGui.Controllers
{
    public class BookingActionController : BaseController
    {
        private Prolink.EDOC_API _api = new Prolink.EDOC_API();
        private string _folder = "DownFiles/MailTemp";
        private static string cloumns = "FOLDER_GUID,SERVER_NUM";
        private string[] BROKER_CD = { "SL", "SH", "CS" };
        // GET: /BookingAction/

        #region 基础方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        /// <summary>
        /// 设置Schema
        /// </summary>
        /// <param name="name"></param>
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "DNDetailView":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMDN WHERE 1=0";
                        break;
                    case "FCLBooking":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMDN WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                dt = GetStatusCountData(statusField, table, condition, Request.Params);
                pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
            }
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
        #endregion

        #region 海运FCL订舱操作
        public ActionResult FCLBookAction()
        {
            string returnMsg = "";
            string isok = "N";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string[] uids = uid.Split(',');

            for (int i = 0; i < uids.Length; i++)
            {
                if (!string.IsNullOrEmpty(uids[i]))
                {
                    string valueitme = FCLBookItem(uids[i]);
                    returnMsg += valueitme;
                    if (!string.IsNullOrEmpty(valueitme))
                    {
                        isok = "N";
                    }
                    else
                    {
                        isok = "Y";
                    }
                }
            }
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_131;
            }
            return Json(new { message = returnMsg, IsOk = isok });
        }

        private string FCLBookItem(string uid)
        {
            string returnMsg = string.Empty;
            string shipmentid = "";
            string mail_to = "";
            string partyno = "";
            string partytype = "";
            int booking_times = 0;

            DataTable maindt = GetSMByUid(uid);
            string tran_type = Prolink.Math.GetValueAsString(maindt.Rows[0]["TRAN_TYPE"]);

            shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
            string trantype = maindt.Rows[0]["TRAN_TYPE"].ToString();
            string term = maindt.Rows[0]["INCOTERM_CD"].ToString();
            booking_times = Prolink.Math.GetValueAsInt(maindt.Rows[0]["BOOKING_TIMES"]);
            string pol = maindt.Rows[0]["POL_CD"].ToString();
            string status = maindt.Rows[0]["STATUS"].ToString();
            string bandtype = maindt.Rows[0]["BAND_TYPE"].ToString();   //饶物流园
            string iscombine_bl = maindt.Rows[0]["ISCOMBINE_BL"].ToString();
            if ("Y".Equals(iscombine_bl) || "C".Equals(iscombine_bl)) return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_41;
            returnMsg = SMHandle.QAHoldBlMessage(shipmentid);
            if (!string.IsNullOrEmpty(returnMsg))
                return returnMsg;
            if ("T".Equals(trantype))
            {
                if (!CheckTruckType(maindt.Rows[0]))
                {
                    return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_42;
                }
            }
            if ("F".Equals(trantype))
            {
                if (CheckFCLType(maindt.Rows[0]))
                {
                    return @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_43;
                }
            }
            EdocHelper edochelper = new EdocHelper();
            partytype = edochelper.GetTypeByTranType(trantype);
            string[] partytypes = partytype.Split(';');

            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            DataTable smsmptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataRow[] smptrows = smsmptdt.Select(string.Format("PARTY_TYPE IN {0}", SQLUtils.Quoted(partytypes)));

            if (smptrows.Length != 1)
            {
                return @Resources.Locale.L_BookingAction_Controllers_138 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_44;
            }

            string msg = CheckBrCr(smsmptdt, trantype, bandtype);
            if (!string.IsNullOrEmpty(msg))
                return msg;


            string brcrpary = "BR;CR";
            if ("CR".Equals(partytype)) brcrpary = "BR;SP";
            string[] brcrs = brcrpary.Split(';');
            DataRow[] brcrrows = smsmptdt.Select(string.Format("PARTY_TYPE IN {0}", SQLUtils.Quoted(brcrs)));

            string mailType = string.Empty;
            mailType = GetMailTypeByTran(trantype);
            bool IsEdi = false;
            if (tran_type.Equals("E"))
            {
                IsEdi = true;
            }
            string dhlset = string.Empty;
            foreach (DataRow dr in smptrows)
            {
                partyno = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                //if (!SMHandle.CheckEDoc(shipmentid, partyno,partytype, "*", ""))
                //{
                //    return "请确认该笔 " + shipmentid + "发给货代的夹档是否完整！";
                //}

                try
                {
                    ResultInfo resultinfo = SendBookingEdi(partyno, shipmentid);
                    EDIConfig config = Business.TPV.Context.GetEDIConfig(partyno, CompanyId);
                    if (config != null)
                    {
                        dhlset = config.FunctionCode;
                    }
                    if (resultinfo != null)
                    {
                        if (resultinfo.IsSucceed)
                        {
                            returnMsg = string.Format("{0}{1}{2}:" + @Resources.Locale.L_BookingActionController_Controllers_45, returnMsg,
                                string.IsNullOrEmpty(returnMsg) ? string.Empty : Environment.NewLine,
                                shipmentid);
                            IsEdi = true;
                        }
                        else
                        {
                            returnMsg = resultinfo.Description;
                            return returnMsg;
                        }
                    }
                }
                catch (Exception ex)
                {
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_46 + ex.Message.ToString();
                    return returnMsg;
                }
                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, tran_type);
                if (!IsEdi && mailGroupDt.Rows.Count <= 0)
                {
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_47 + partyno + @Resources.Locale.L_BookingActionController_Controllers_48;
                    return returnMsg;
                }
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    string mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailindex))
                    {
                        mail_to += mailindex + ";";
                    }
                }

                string mailcc = string.Empty;
                foreach (DataRow brcrdr in brcrrows)
                {
                    partyno = Prolink.Math.GetValueAsString(brcrdr["PARTY_NO"]);
                    mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, tran_type);
                    if (!IsEdi && mailGroupDt.Rows.Count <= 0) continue;
                    foreach (DataRow mailGroup in mailGroupDt.Rows)
                    {
                        string mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                        if (!string.IsNullOrEmpty(mailindex) && mailcc.IndexOf(mailindex) < 0)
                        {
                            mailcc += mailindex + ";";
                        }
                    }
                }
                EvenFactory.AddEven(mailType + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString() + "#" + mailcc, uid, "MM", null, 1, 0, mail_to, GetSubJectByTranType(trantype, term, "", booking_times) + shipmentid, "");
            }
            //update 资料信息
            string uext = BookingStatusManager.GetUserFxt(UserId);
            if (string.IsNullOrEmpty(uext)) uext = UserId;
            MixedList mixList = new MixedList();
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("BOOKING_USER", UserId);
            ei.Put("MODIFY_BY", UserId);
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            ei.Put("BL_WIN", uext);
            ei.PutDate("SEND_BOOK_DATE", DateTime.Now);
            mixList.Add(ei);

            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);
            if ("A".Equals(status) || booking_times <= 0)
            {
                apei.Put("STATUS", 'B');
                apei.Put("CORDER", 'S');    //表示已经发送订舱通知为S
            }
            apei.Put("BOOKING_USER", UserId);
            apei.Put("BOOKING_TIMES", booking_times + 1);
            apei.Put("BL_WIN", uext);

            if (dhlset != "DHL")
            {
                mixList.Add(apei);
            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "010", Sender = UserId, Location = pol, LocationName = "", StsDescp = "Booking" });
            }
            catch (Exception ex)
            {
                returnMsg += ex.ToString();
            }
            return returnMsg;
        }

        private bool CheckTruckType(DataRow smrow)
        {
            string trackway = Prolink.Math.GetValueAsString(smrow["Track_Way"]);
            if (!"F".Equals(trackway)) return true;

            Func<string, string, bool> checkEmpty = (column, field) =>
            {
                string type = Prolink.Math.GetValueAsString(smrow[column]);
                int qty = Prolink.Math.GetValueAsInt(smrow[field]);
                if (!string.IsNullOrEmpty(type) && qty > 0) return true;
                return false;
            };
            bool b1 = checkEmpty("CAR_TYPE", "CAR_QTY");
            bool b2 = checkEmpty("CAR_TYPE1", "CAR_QTY1");
            bool b3 = checkEmpty("CAR_TYPE2", "CAR_QTY2");
            return b1 || b2 || b3;
        }

        private bool CheckFCLType(DataRow smrow)
        {
            string co = Prolink.Math.GetValueAsString(smrow["COMBINE_OTHER"]);
            int tcbm = Prolink.Math.GetValueAsInt(smrow["TCBM"]);
            if (!"Y".Equals(co)) return false;
            if (tcbm <= 0) return true;
            return false;
        }

        private string CheckBrCr(DataTable smsmptdt, string trantype, string bandtype)
        {
            string msg = string.Empty;
            string partype = "BR;CR";
            switch (trantype)
            {
                case "D":
                case "T":
                    partype = "CR";
                    break;
            }
            if ("Y".Equals(bandtype))
            {
                partype = "BR;CR";
            }
            string[] partypes = partype.Split(';');
            DataRow[] drs = smsmptdt.Select(string.Format("PARTY_TYPE IN {0}", SQLUtils.Quoted(partypes)));
            if (drs.Length == partypes.Length)
                return string.Empty;
            return @Resources.Locale.L_BookingActionController_Controllers_49;
        }

        private string GetSubJectByTranType(string trantype, string term, string cancel = "", int sendtimes = 0)
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

        private ResultInfo SendBookingEdi(string partyno, string shipmentid, OperationModes mode = OperationModes.Add)
        {
            EDIConfig config = Business.TPV.Context.GetEDIConfig(partyno, CompanyId);
            if (config == null)
            {
                return null;
            }
            ResultInfo resultinfo = new ResultInfo();
            Business.TPV.Runtime runtime = new Business.TPV.Runtime { PartyNo = partyno, ShipmentID = shipmentid, OPUser = UserId, OperationMode = mode };
            switch (config.FunctionCode)
            {
                case "CPL":
                    Business.TPV.CPL.Manager m = new Business.TPV.CPL.Manager();
                    resultinfo = m.SendBooking(runtime);
                    break;
                case "DHL":
                    Business.TPV.DHL.Manager dhlm = new Business.TPV.DHL.Manager();
                    resultinfo = dhlm.SendBookingInfo(runtime);
                    break;
                case "XPI":
                    Business.TPV.LSP.ExportManager lspbm = new Business.TPV.LSP.ExportManager();
                    resultinfo = lspbm.SendBooking(runtime);
                    break;
                case "TNT":
                    Business.TPV.TNT.Manager tntm = new Business.TPV.TNT.Manager();
                    resultinfo = tntm.SendBooking(runtime);
                    break;
                case "COSCO":
                    //if (OperationModes.Cancel.Equals(mode)) return null;  //cosco 无取消订舱的EDI
                    Business.TPV.Cosco.Manager coscom = new Business.TPV.Cosco.Manager();
                    resultinfo = coscom.SendBooking(runtime);
                    break;
            }
            return resultinfo;
        }

        private ResultInfo SendBrokerEdi(string partyno, string shipmentid)
        {
            EDIConfig config = Business.TPV.Context.GetEDIConfig(partyno, CompanyId);
            if (config == null)
            {
                return null;
            }
            ResultInfo resultinfo = new ResultInfo();
            switch (config.FunctionCode)
            {
                case "XPI":
                    Business.TPV.LSP.ExportManager lspbm = new Business.TPV.LSP.ExportManager();
                    resultinfo = lspbm.SendDeclaration(new Business.TPV.Runtime { PartyNo = partyno, ShipmentID = shipmentid, OPUser = UserId });
                    break;
                default:
                    return null;
            }
            return resultinfo;
        }

        private string GetMailTypeByTran(string trantype)
        {
            string mailType = string.Empty;
            switch (trantype)
            {
                case "A":
                    mailType = "AB";
                    break;
                case "D":
                    mailType = "DB";
                    break;
                case "E":
                    mailType = "EB";
                    break;
                case "F":
                    mailType = "FB";
                    break;
                case "L":
                    mailType = "LB";
                    break;
                case "R":
                    mailType = "RB";
                    break;
                case "T":
                    mailType = "TB";
                    break;
                default:
                    mailType = "A";
                    break;
            }
            return mailType;
        }

        #endregion

        #region 货况
        public ActionResult GetStatus()     //和TKBL的GetStatus一致，只是查询条件不一样
        {
            string shipment_id = Request["ShipmentId"];
            string sql = string.Format("SELECT * FROM TKBLST WHERE SHIPMENT_ID={0} ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(shipment_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 数据改造
            mainDt.Columns.Add("EVEN_TMG1", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                if (dr["EVEN_TMG"] == null || dr["EVEN_TMG"] == DBNull.Value)
                    continue;
                dr["EVEN_TMG1"] = ((DateTimeOffset)dr["EVEN_TMG"]).ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
            mainDt.Columns.Remove("EVEN_TMG");
            mainDt.Columns.Add("EVEN_TMG", typeof(string));
            foreach (DataRow dr in mainDt.Rows)
            {
                dr["EVEN_TMG"] = dr["EVEN_TMG1"];
            }
            #endregion

            return ToContent(ModelFactory.ToTableJson(mainDt, "StatusModel"));
        }
        #endregion


        public string GetMailBody(string group_id, string notify_format = "STT")
        {
            string sql = string.Format("SELECT MT_NAME,MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND MT_TYPE={1}", SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(notify_format));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return "";
            }
            return Prolink.Math.GetValueAsString(dt.Rows[0]["MT_CONTENT"].ToString());
        }

        #region 取消订舱
        public ActionResult FCLBookCancel()
        {
            string returnMsg = string.Empty;
            string showmsg = string.Empty;
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string cancelrmk = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);
            string[] uids = uid.Split(',');
            string shipmentid = "";
            string partyno = "";
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            uid = uids[0];

            DataTable maindt = GetSMByUid(uid);
            if (maindt.Rows.Count <= 0)
                return Json(new { message = "No valid data" });
            shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
            string atd = maindt.Rows[0]["ATD"].ToString();
            string status = maindt.Rows[0]["STATUS"].ToString();
            string trantype = maindt.Rows[0]["TRAN_TYPE"].ToString();
            string term = maindt.Rows[0]["INCOTERM_CD"].ToString();
            string pol = maindt.Rows[0]["POL_CD"].ToString();
            string sql = string.Format("SELECT  COUNT(1)  FROM SMRV WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            int callcarnum = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (callcarnum > 0)
            {
                showmsg =  @Resources.Locale.L_BookingActionController_Controllers_50;
            }

            switch (status)
            {
                case "A":
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_51+ shipmentid +  @Resources.Locale.L_BookingActionController_Controllers_52;
                    break;
                case "V":
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_51 + shipmentid +  @Resources.Locale.L_BookingActionController_Controllers_53;
                    break;
            }
            if (!string.IsNullOrEmpty(atd))
            {
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_54;
            }
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            EdocHelper edochelper = new EdocHelper();
            string partytype = edochelper.GetTypeByTranType(trantype);
            string[] partytypes = partytype.Split(';');

            sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE in {1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.Quoted(partytypes));

            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string mailType = MailManager.CancelBooking;
            IMailTemplateParse parse = new DefaultMailParse();
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_TYPE"]);
                bool IsEdi = false;
                try
                {
                    ResultInfo resultinfo = SendBookingEdi(partyno, shipmentid, OperationModes.Cancel);
                    if (resultinfo != null)
                    {
                        if (resultinfo.IsSucceed)
                        {
                            returnMsg = string.Format("{0}{1}{2}:"+@Resources.Locale.L_BookingActionController_Controllers_45, returnMsg,
                                string.IsNullOrEmpty(returnMsg) ? string.Empty : Environment.NewLine,
                                shipmentid);
                            HandleCancelBooking(shipmentid);
                            return Json(new { message = returnMsg });
                        }
                        else
                        {
                            returnMsg = resultinfo.Description;
                            return Json(new { message = returnMsg });
                        }
                    }
                }
                catch (Exception ex)
                {
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_46 + ex.Message.ToString();
                    return Json(new { message = returnMsg });
                }

                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, trantype);
                if (mailGroupDt.Rows.Count <= 0)
                {
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_47 + partyno +  @Resources.Locale.L_BookingActionController_Controllers_48;
                    return Json(new { message = returnMsg });
                }
                string mail_to = string.Empty;
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    string mailid = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailid))
                    {
                        mail_to += mailid + ";";
                    }
                }
                EvenFactory.AddEven(mailType + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString(), uid, MailManager.CancelBooking, null, 1, 0, mail_to, GetSubJectByTranType(trantype, term, " Cancel") + shipmentid +  @Resources.Locale.L_DNManage_Descp + cancelrmk, "");
            }

            //update 资料信息
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);
            apei.Put("STATUS", "A");
            apei.Put("CORDER", "N");
            apei.Put("BOOKING_TIMES", "0");
            //apei.Put("SIGN_BACK", cancelrmk);   //遲簽退原因
            mixList.Add(apei);
            HandleCancelBooking(shipmentid);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_55 + shipmentid +  @Resources.Locale.L_BookingActionController_Controllers_56 + showmsg;
                Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "011", Sender = UserId, Location = pol, LocationName = "", StsDescp = "Cancel Booking" });
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message, IsOk = "N" });
            }

            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public void HandleCancelBooking(string shipmentid){
            Helper.CancelBooking(shipmentid);
            TmexpHandler th = new TmexpHandler();
            TmexpInfo tpi = new TmexpInfo();
            tpi.UFid = Guid.NewGuid().ToString();
            tpi.WrId = UserId;
            tpi.WrDate = DateTime.Now;
            tpi.Cmp = CompanyId;
            tpi.GroupId = GroupId;
            tpi.JobNo = shipmentid;
            tpi.ExpType = "SM";
            tpi.ExpReason = "BK_CANCEL";
            tpi.ExpText = @Resources.Locale.L_BSTSetup_Book+ @Resources.Locale.L_BookingActionController_Controllers_57 + UserId +  @Resources.Locale.L_BookingActionController_Controllers_58;
            tpi.ExpObj = UserId;
            EditInstruct ei = th.SetTmexpEi(tpi);
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        #endregion

        #region 报关订舱
        public ActionResult DeclareBooking()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string cancelrmk = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);
            string[] uids = uid.Split(',');
            string dnno = "";
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            uid = uids[0];

            string sql = "SELECT * FROM SMSMPT WHERE U_FID=" + SQLUtils.QuotedStr(uid) + " AND PARTY_TYPE ='BA'";
            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string mailto = "";
            string partyno = "";

            DataTable maindt = GetSMByUid(uid);
            if (maindt.Rows.Count <= 0)
                return Json(new { message = "No valid data" });
            dnno = maindt.Rows[0]["DN_NO"].ToString();
            IMailTemplateParse parse = new DefaultMailParse();
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                mailto = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_MAIL"]);
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                //check 夹带文档
                //根据partyno带出需要上传的档案类型
                sql = "SELECT DOC_TYPE FROM TKPDM,TKPDD WHERE TKPDM.U_ID=TKPDD.U_ID AND TKPDM.CMP=" + SQLUtils.QuotedStr(partyno);
                DataTable doctypedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string doctypes = ";";
                for (int j = 0; j < doctypedt.Rows.Count; j++)
                {
                    doctypes += Prolink.Math.GetValueAsString(doctypedt.Rows[j]["DOC_TYPE"]) + ";";
                }

                string jobNo = "123";//dnno
                string dep = "";// Request.Params["DEP"];
                _api.Login();
                int serverNum = 0;
                string guid = _api.GetFolderGUID(jobNo, GroupId, CompanyId, Station, dep, ref serverNum, cloumns);
                List<EDOCFileItem> edocList = _api.Inquery(guid, "", serverNum);
                MailInfo mi = new MailInfo();
                foreach (EDOCFileItem Efi in edocList)
                {
                    string edoctype = Prolink.Math.GetValueAsString(Efi.EdocType);
                    string fileid = Prolink.Math.GetValueAsString(Efi.FileID);
                    string filepath = Server.MapPath(_folder) + "\\" + Prolink.Math.GetValueAsString(Efi.Name + "." + Efi.Ext);
                    if (doctypes.Contains(";" + edoctype + ";"))
                    {
                        _api.DownloadFile(filepath, fileid, "", serverNum);
                        mi.Attachments.Add(new Attachment(filepath));
                    }
                }

                if (string.IsNullOrEmpty(mailto))
                {
                    continue;
                }

                string mailType = "CancelBooking";
                Dictionary<string, string> map = new Dictionary<string, string>();
                string body = GetMailBody(GroupId, mailType);
                EvenFactory.AddEven(dnno + "#" + mailType, dnno, "MM", null, 1, 0, mailto, dnno + @Resources.Locale.L_DNManage_DeclaNiti, parse.Parse(maindt, null, body, map));
            }

            //update 资料信息
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.INSERT_OPERATION);
            apei.PutKey("U_ID", uid);
            apei.PutDate("MODIFY_DATE", DateTime.Now);
            mixList.Add(apei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = "";
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message, message1 = returnMsg });
            }

            return Json(new { message = returnMsg });

        }
        #endregion

        #region BL Info
        public ActionResult BLInfoSend()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string[] uids = uid.Split(',');
            string dnno = "";
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            uid = uids[0];

            string sql = "SELECT * FROM SMSMPT WHERE U_FID=" + SQLUtils.QuotedStr(uid) + " AND PARTY_TYPE ='BA'";
            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string mailto = "";
            string partyno = "";
            DataTable maindt = GetSMByUid(uid);
            if (maindt.Rows.Count <= 0)
                return Json(new { message = "No valid data" });
            dnno = maindt.Rows[0]["DN_NO"].ToString();
            IMailTemplateParse parse = new DefaultMailParse();
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                mailto = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_MAIL"]);
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                //check 夹带文档
                //根据partyno带出需要上传的档案类型
                sql = "SELECT DOC_TYPE FROM TKPDM,TKPDD WHERE TKPDM.U_ID=TKPDD.U_ID AND TKPDM.CMP=" + SQLUtils.QuotedStr(partyno);
                DataTable doctypedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string doctypes = ";";
                for (int j = 0; j < doctypedt.Rows.Count; j++)
                {
                    doctypes += Prolink.Math.GetValueAsString(doctypedt.Rows[j]["DOC_TYPE"]) + ";";
                }

                string jobNo = "123";//dnno
                string dep = "";// Request.Params["DEP"];
                _api.Login();
                int serverNum = 0;
                string guid = _api.GetFolderGUID(jobNo, GroupId, CompanyId, Station, dep,ref serverNum, cloumns);
                List<EDOCFileItem> edocList = _api.Inquery(guid, "", serverNum);
                MailInfo mi = new MailInfo();
                foreach (EDOCFileItem Efi in edocList)
                {
                    string edoctype = Prolink.Math.GetValueAsString(Efi.EdocType);
                    string fileid = Prolink.Math.GetValueAsString(Efi.FileID);
                    string filepath = Server.MapPath(_folder) + "\\" + Prolink.Math.GetValueAsString(Efi.Name + "." + Efi.Ext);
                    if (doctypes.Contains(";" + edoctype + ";"))
                    {
                        _api.DownloadFile(filepath, fileid, "", serverNum);
                        mi.Attachments.Add(new Attachment(filepath));
                    }
                }

                if (string.IsNullOrEmpty(mailto))
                {
                    continue;
                }

                string mailType = "BLINFO"; //BLInfo
                Dictionary<string, string> map = new Dictionary<string, string>();
                string body = GetMailBody(GroupId, mailType);
                EvenFactory.AddEven(dnno + "#" + mailType, dnno, "MM", null, 1, 0, mailto, dnno + @Resources.Locale.L_DNManage_DeclaNiti, parse.Parse(maindt, null, body, map));
            }

            //update 资料信息
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.INSERT_OPERATION);
            apei.PutKey("U_ID", uid);
            apei.PutDate("MODIFY_DATE", DateTime.Now);
            mixList.Add(apei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = "";
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message, message1 = returnMsg });
            }

            return Json(new { message = returnMsg });
        }
        #endregion

        #region 退運
        public ActionResult BackTransport()
        {
            string shipment = Request.Params["Shipmentid"];
            shipment = shipment.Trim(',');
            string[] shipments = shipment.Split(',');
            string returnMsg = "";
            string IsOk = string.Empty;
            string sql = string.Empty;
            foreach (string shipmentitem in shipments)
            {
                DataTable smdt = GetSMByShipmentId(shipmentitem);
                if (smdt.Rows.Count < 0)
                {
                    returnMsg += @Resources.Locale.L_ForecastQueryData_Views_243;
                    continue;
                }
                string border = Prolink.Math.GetValueAsString(smdt.Rows[0]["BORDER"]);
                if ("S".Equals(border) || "C".Equals(border) || "H".Equals(border))
                    return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_59, IsOk = "N" });

                sql = string.Format("SELECT APPROVE_TYPE,DN_NO FROM SMDN WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentitem));
                DataTable smdndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //update 资料信息
                MixedList mixList = new MixedList();
                for (int i = 0; i < smdndt.Rows.Count; i++)
                {
                    //清除该shipment下所有dn的审核状态 shipment不能再使用
                    string dnno = Prolink.Math.GetValueAsString(smdndt.Rows[i]["DN_NO"]);
                    string approvetype = Prolink.Math.GetValueAsString(smdndt.Rows[i]["APPROVE_TYPE"]);

                    EditInstruct apei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                    apei.PutKey("DN_NO", dnno);
                    apei.PutKey("GROUP_ID", GroupId);
                    apei.PutKey("CMP", CompanyId);
                    apei.Put("APPROVE_TO", "A");
                    apei.Put("SHIPMENT_ID", "");
                    apei.Put("STATUS", "D");
                    mixList.Add(apei);

                    mixList.Add(DNInfoCheck.GetApproveRdVoidEI(dnno, approvetype));
                }
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipment);
                ei.PutKey("GROUP_ID", GroupId);
                ei.Put("STATUS", 'Z');  //状态变为退运
                mixList.Add(ei);

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMsg += shipmentitem + ex.ToString();
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg += @Resources.Locale.L_BookingActionController_Controllers_60;
                IsOk = "Y";
            }
            else
            {
                IsOk = "N";
            }
            return Json(new { message = returnMsg, IsOk = IsOk });
        }
        #endregion

        #region 通知报关操作
        public ActionResult DECLBookAction()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            DataTable maindt = GetSMByUids(uids);
            //DataTable partydt = GetSMPTByUids(uids);
            returnMsg = SendDeclaration(maindt);
            if (returnMsg.Contains(@Resources.Locale.L_BookingAction_Controllers_160))
                return Json(new { message = returnMsg, IsOk = "Y" });
            return Json(new { message = returnMsg, IsOk = "N" });
        }

        public ActionResult DECLBookActionInvPkg()
        {
            string returnMsg = "";
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            if (shipmentid.Length <= 0)
                return Json(new { message = "No valid data" });
            shipmentid = shipmentid.Trim(',');
            string[] shipmentids = shipmentid.Split(',');
            foreach (string shipmentindex in shipmentids)
            {
                string border = string.Empty;
                string torder = string.Empty;
                string corder = string.Empty;
                string status = string.Empty;
                List<string> dnlist = new List<string>();

                Result result = CheckBConfirmAndApproveFIM(returnMsg, shipmentindex, ref border, ref torder, ref corder, ref status, ref dnlist);
                if (!result.Success)
                {
                    return Json(new { message = result.Message, IsOk = "N" });
                }
            }
            DataTable maindt = GetSMByShipmentId(shipmentids);

            returnMsg = SendDeclaration(maindt);
            if (returnMsg.Contains(@Resources.Locale.L_BookingAction_Controllers_160))
                return Json(new { message = returnMsg, IsOk = "Y" });
            return Json(new { message = returnMsg, IsOk = "N" });
        }

        private Result CheckBConfirmAndApproveFIM(string returnMsg, string shipmentid, ref string border, ref string torder, ref string corder, ref string status, ref List<string> dnlist)
        {
            Result result=new Result();
            bool isfimok = BookingStatusManager.IsApproveByFIM(shipmentid, ref border, ref torder, ref status, ref corder, ref dnlist);
            if (!isfimok)
            {
                result.Success = false;
                if ("C" != corder &&"Q" != corder){
                    result.Message = string.Format(@Resources.Locale.L_BookingActionController_Controllers_61, shipmentid, string.Join(";", dnlist));
                    return result;
                }
                result.Message = string.Format(@Resources.Locale.L_BookingActionController_Controllers_62, shipmentid, string.Join(";", dnlist));
                return result;
            }
            if ("C" != corder && "Q" != corder)
            {
                result.Success = false;
                result.Message = string.Format(@Resources.Locale.L_BookingActionController_Controllers_63 + @Resources.Locale.L_BookingAction_Controllers_162, shipmentid);
                return result;
            }
            result.Success = true;
            return result;
        }

        private string SendDeclaration(DataTable maindt)
        {
            if (maindt.Rows.Count <= 0)
                return "No valid data";
            string returnMsg = string.Empty;
            string nullmsg = string.Empty;
            int i = 0;
            foreach (DataRow dr in maindt.Rows)
            {
                string shipmentid = dr["SHIPMENT_ID"].ToString();
                string border= dr["BORDER"].ToString();
                string uid = dr["U_ID"].ToString();
                if (i == 0)
                {
                    nullmsg = shipmentid + ":";
                }
                else
                {
                    nullmsg = "；" + shipmentid + ":";
                }
                i++;
                string partyno = string.Empty;

                if ("G".Equals(border))
                {
                    returnMsg += nullmsg + @Resources.Locale.L_DNInfoCheck_Business_68;
                    continue;
                }
                if ("H".Equals(border) || "H".Equals(dr["STATUS"].ToString()))
                {
                    returnMsg += nullmsg + @Resources.Locale.L_DNInfoCheck_Business_69;
                    continue;
                }

                string oexporter = dr["OEXPORTER"].ToString();
                string oimporter = dr["OIMPORTER"].ToString();
                DataTable partytabe = GetPTByShipmentid(shipmentid);

                if (string.IsNullOrEmpty(oexporter))
                {
                    partytabe = GetPTByPartyType(shipmentid, "SL;SH");
                    if (partytabe.Rows.Count > 0)
                    {
                        oexporter = Prolink.Math.GetValueAsString(partytabe.Rows[0]["PARTY_NO"]);
                    }
                }
                if (string.IsNullOrEmpty(oimporter))
                {
                    partytabe = GetPTByPartyType(shipmentid, "CS");
                    if (partytabe.Rows.Count > 0)
                    {
                        oimporter = Prolink.Math.GetValueAsString(partytabe.Rows[0]["PARTY_NO"]);
                    }
                }
                partytabe = GetPTByPartyType(shipmentid, "BR");
                string partype = string.Empty;
                if (partytabe.Rows.Count > 0)
                {
                    partyno = Prolink.Math.GetValueAsString(partytabe.Rows[0]["PARTY_NO"]);
                    partype = Prolink.Math.GetValueAsString(partytabe.Rows[0]["PARTY_TYPE"]);
                }
                if (string.IsNullOrEmpty(partyno))
                {
                    returnMsg += nullmsg +  @Resources.Locale.L_BookingAction_Controllers_164;
                    continue;
                }
                if (string.IsNullOrEmpty(oexporter) || string.IsNullOrEmpty(oimporter))
                {
                    returnMsg += nullmsg + @Resources.Locale.L_BookingAction_Controllers_165;
                    continue;
                }
                else
                {
                    try
                    {
                        ResultInfo resultinfo = SendBrokerEdi(partyno, shipmentid);
                        if (resultinfo != null)
                        {
                            if (resultinfo.IsSucceed)
                            {
                                returnMsg += string.Format("{0}{1}{2}:"+@Resources.Locale.L_BookingActionController_Controllers_45, returnMsg,
                                    string.IsNullOrEmpty(returnMsg) ? string.Empty : Environment.NewLine,
                                    shipmentid);
                                UpdateBorder(uid);
                            }
                            else
                            {
                                returnMsg += nullmsg + resultinfo.Description;
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        returnMsg += nullmsg +  @Resources.Locale.L_BookingActionController_Controllers_46 + ex.Message.ToString();
                        continue;
                    }
                    NotifyBookingAgentWhenDECL(maindt.Rows[0]);
                    string message = SendBrokerMail(uid, shipmentid, partyno, MailManager.BrokerNotify, GroupId, CompanyId, partype, maindt.Rows[0]);
                    if (!string.IsNullOrEmpty(message))
                    {
                        returnMsg += nullmsg + message;
                        continue;
                    }
                }
                nullmsg += UpdateBorder(uid);
                nullmsg +=  @Resources.Locale.L_BookingActionController_Controllers_64;
                returnMsg += nullmsg;
            }
            return returnMsg;
        }

        void NotifyBookingAgentWhenDECL(DataRow smRow)
        {
            try
            {
                var rows = Helper.GetBookingAgent(smRow);
                if (rows == null) return;
                foreach (var row in rows)
                {
                    string partyNo = Prolink.Math.GetValueAsString(row["PARTY_NO"]);
                    switch (partyNo)
                    {
                        case "0008914001":
                        case "0008914000":
                            Business.TPV.LSP.ExportManager m = new Business.TPV.LSP.ExportManager();
                            m.SendDeclaration(new Business.TPV.Runtime
                            {
                                PartyNo = partyNo,
                                ShipmentID = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]),
                                OPUser = UserId
                            }, true);
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        public string UpdateBorder(string uid)
        {
            string returnMsg = string.Empty;
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);
            //apei.Put("STATUS", 'G');
            apei.Put("BORDER", 'S');
            apei.PutDate("SEND_BROKER_DATE", DateTime.Now);
            mixList.Add(apei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = "";
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
            }
            return returnMsg;
        }

        private string SendBrokerMail(string uid, string shipmentid, string partyno, string mailType, string GroupId, string CompanyId, string partytype, DataRow smRow)
        {
            string returnMsg = string.Empty;
            string mail_to = string.Empty;
            if (!SMHandle.CheckEDoc(shipmentid, partyno, partytype, "*", "", true))
            {
                return @Resources.Locale.L_BookingActionController_Controllers_65;
            }
            DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, "B");
            try
            {
                var rows = Helper.GetBookingAgent(smRow);
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        string partyNo = Prolink.Math.GetValueAsString(row["PARTY_NO"]);
                        switch (partyNo)
                        {
                            case "0008914001":
                            case "0008914000":
                                string tran_type = Prolink.Math.GetValueAsString(smRow["TRAN_TYPE"]);
                                DataTable bkAgentMailGroupDT = MailTemplate.GetMailGroup(partyNo, GroupId, tran_type);
                                if (bkAgentMailGroupDT != null && bkAgentMailGroupDT.Rows.Count > 0)
                                {
                                    foreach (DataRow dr in bkAgentMailGroupDT.Rows)
                                        mailGroupDt.ImportRow(dr);
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {

            }
            if (mailGroupDt.Rows.Count <= 0)
            {
                return @Resources.Locale.L_BookingActionController_Controllers_47 + partyno +  @Resources.Locale.L_BookingActionController_Controllers_48;
            }
            foreach (DataRow mailGroup in mailGroupDt.Rows)
            {
                mail_to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                if (string.IsNullOrEmpty(mail_to))
                {
                    continue;
                }
                EvenFactory.AddEven(mailType + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString(), uid, "MM", null, 1, 0, mail_to, shipmentid + @Resources.Locale.L_BookingAction_Controllers_167, "");
            }
            return returnMsg;
        }
        #endregion

        #region 报关确认
        public ActionResult DECLBookConfirm()
        {
            string u_id = Request.Params["UId"];
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string declrlsdate = Request.Params["DeclRlsDate"];
            string returnMessage = "success";
            MixedList mixedlist = new MixedList();
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string pol = string.Empty;
            if (mainDt.Rows.Count > 0)
            {
                pol = Prolink.Math.GetValueAsString(mainDt.Rows[0]["POL_CD"]);
                string corder = Prolink.Math.GetValueAsString(mainDt.Rows[0]["BORDER"]);
                if (corder.Equals("C"))
                {
                    returnMessage = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingAction_Controllers_168;
                    return Json(new { message = returnMessage, IsOk = "N" });
                }
                if ("M".Equals(corder))
                {
                    returnMessage = @Resources.Locale.L_BookingActionController_Controllers_66 + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_67;
                    return Json(new { message = returnMessage, IsOk = "N" });
                }
            }
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", u_id);
            ei.Put("MODIFY_BY", UserId);
            ei.PutDate("MODIFY_DATE", DateTime.Now);
            ei.Put("BORDER", "C");
            if (!string.IsNullOrEmpty(declrlsdate))
            {
                ei.Put("BORDER", "H");  //有放行时间，将状态更新为放行
                ei.Put("STATUS", "H");
            }
            mixedlist.Add(ei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!string.IsNullOrEmpty(declrlsdate))
                    Manager.SaveStatus(new Status() { ShipmentId = shipmentid, StsCd = "200", Sender = UserId, Location = pol, LocationName = "", StsDescp = "Release Date" });
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                return Json(new { message = ex.Message, IsOk = "N" });
            }
            returnMessage = @Resources.Locale.L_BookingActionController_Controllers_68 + shipmentid + @Resources.Locale.L_BookingAction_Controllers_170;
            return Json(new { message = returnMessage, IsOk = "Y" });
        }
        #endregion

        #region 取消报关操作
        public ActionResult DECLCancelAction()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            string backremark = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);

            string[] uids = uid.Split(',');
            string partyno = "";
            if (uids.Length <= 0)
                return Json(new { message = "No valid data" });
            uid = uids[0];
            DataTable maindt = GetSMByUid(uid);
            if (maindt.Rows.Count <= 0)
                return Json(new { message = "No valid data" });
            string shipmentid = Prolink.Math.GetValueAsString(maindt.Rows[0]["SHIPMENT_ID"]);
            string status = Prolink.Math.GetValueAsString(maindt.Rows[0]["STATUS"]);
            //if()

            string sql = "SELECT * FROM SMSMPT WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid) + " AND PARTY_TYPE ='BR'";
            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            //check 夹带文档
            IMailTemplateParse parse = new DefaultMailParse();
            string partytype = string.Empty;
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_TYPE"]);
                string mailType = MailManager.CancelBroker;

                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, "B");
                if (mailGroupDt.Rows.Count <= 0)
                {
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_47 + partyno +  @Resources.Locale.L_BookingActionController_Controllers_48;
                    return Json(new { message = returnMsg });
                }
                string mail_to = string.Empty;
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    mail_to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (string.IsNullOrEmpty(mail_to))
                    {
                        continue;
                    }
                    EvenFactory.AddEven(mailType + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString(), uid, MailManager.CancelBroker, null, 1, 0, mail_to, shipmentid + " Cancel Declare ", "");
                }
            }

            //update 资料信息
            MixedList mixList = new MixedList();
            EditInstruct apei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            apei.PutKey("U_ID", uid);
            apei.PutExpress("BROKER_INSTR", "BROKER_INSTR+'\n "+@Resources.Locale.L_BookingActionController_Controllers_69 + backremark + "'");
            //apei.Put("STATUS", 'O');
            apei.Put("BORDER", "M");
            mixList.Add(apei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnMsg = "";
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message });
            }

            return Json(new { message = returnMsg, IsOk = "Y" });
        }
        #endregion

        #region  作废Shipment
        public ActionResult VoidSM()
        {
            string returnMsg = "";
            string uid = Request.Params["UId"];
            string cancelreson = Prolink.Math.GetValueAsString(Request.Params["CancelReson"]);
            MixedList mlist = new MixedList();
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smdt.Rows.Count <= 0)
            {
                return Json(new { message = @Resources.Locale.L_ForecastQueryData_Views_243, IsOk = "N" });
            }
            DataRow dr = smdt.Rows[0];
            string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            if (Prolink.Math.GetValueAsString(dr["CORDER"]).Equals("S") || Prolink.Math.GetValueAsString(dr["CORDER"]).Equals("C"))
            {
                return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_70, IsOk = "N" });
            }
            if (Prolink.Math.GetValueAsString(dr["TORDER"]).Equals("C"))
            {
                return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_71, IsOk = "N" });
            }
            if (Prolink.Math.GetValueAsString(dr["BORDER"]).Equals("S") || Prolink.Math.GetValueAsString(dr["BORDER"]).Equals("C"))
            {
                return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_72, IsOk = "N" });
            }
            string bltype = Prolink.Math.GetValueAsString(dr["BL_TYPE"]);
            if (!string.IsNullOrEmpty(uid))
            {
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("STATUS", "V");
                ei.Put("MODIFY_BY", UserId);
                ei.PutDate("MODIFY_DATE", DateTime.Now);

                string smbdsql = string.Format("UPDATE SMBD SET STATUS='D' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                sql = string.Format("UPDATE SMDN SET SHIPMENT_ID='',STATUS='T' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                string sminmsql = string.Format("UPDATE SMINM SET SHIPMENT_ID='' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                string smindsql = string.Format("UPDATE SMIND SET SHIPMENT_ID='' WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                mlist.Add(smbdsql);
                mlist.Add(sql);
                mlist.Add(sminmsql);
                mlist.Add(smindsql);
                mlist.Add(ei);

                string transtype = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                if (transtype.Equals("T"))
                {
                    string sales = Prolink.Math.GetValueAsString(dr["SALES_WIN"]);
                    string id = string.Empty;
                    if (!string.IsNullOrEmpty(sales))
                    {
                        id = sales.Replace(" ", ";").Split(';')[0];
                    }
                    string mailto = OperationUtils.GetValueAsString(string.Format("SELECT U_EMAIL  FROM SYS_ACCT WHERE U_ID='{0}'", id), Prolink.Web.WebContext.GetInstance().GetConnection());

                    EvenFactory.AddEven(Guid.NewGuid().ToString(), uid, TrackingEDI.Business.MailManager.SMVoid, null, 1, 0, mailto, "Lst 退回通知", cancelreson);
                }
                TmexpHandler th1 = new TmexpHandler();
                TmexpInfo tpi1 = new TmexpInfo();
                tpi1.UId = Guid.NewGuid().ToString();
                tpi1.UFid = Guid.NewGuid().ToString();
                tpi1.WrId = UserId;
                tpi1.WrDate = DateTime.Now;
                tpi1.Cmp = CompanyId;
                tpi1.GroupId = GroupId;
                tpi1.JobNo = shipmentid;
                tpi1.ExpType = "SM";
                tpi1.ExpReason = "SMV";
                tpi1.ExpText = @Resources.Locale.L_BSTSetup_Book + @Resources.Locale.L_BookingActionController_Controllers_73 + UserId + @Resources.Locale.L_BookingActionController_Controllers_74 +
                    @Resources.Locale.L_BillApproveHelper_Business_30 + cancelreson;
                tpi1.ExpObj = UserId;
                mlist.Add(th1.SetTmexpEi(tpi1));
            }
            TmexpHandler th = new TmexpHandler();
            TmexpInfo tpi = new TmexpInfo();
            string combineinfo=Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
            string[] dnnos=combineinfo.Split(',');
            foreach (string dnno in dnnos)
            {
                tpi.UId = Guid.NewGuid().ToString();
                tpi.UFid = Guid.NewGuid().ToString();
                tpi.WrId = UserId;
                tpi.WrDate = DateTime.Now;
                tpi.Cmp = CompanyId;
                tpi.GroupId = GroupId;
                tpi.JobNo = dnno;
                tpi.ExpType = "SM";
                tpi.ExpReason = "SMV";
                tpi.ExpText = @Resources.Locale.L_BSTSetup_Book + @Resources.Locale.L_BookingActionController_Controllers_73 + UserId + @Resources.Locale.L_BookingActionController_Controllers_74 +
                    @Resources.Locale.L_BillApproveHelper_Business_30 + cancelreson;
                tpi.ExpObj = UserId;
                mlist.Add(th.SetTmexpEi(tpi));

            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_BookingActionController_Controllers_75;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }
        #endregion

        #region  Allocation
        public ActionResult Allocation()
        {
            string returnMsg = "";
            string uid = Request.Params["UId"];
            DataTable dt = GetSMByUid(uid);
            AllocationHelper ah = new AllocationHelper(uid);
            returnMsg = ah.GetAllocation();
            if (string.IsNullOrEmpty(returnMsg))
            {
                CommonManager.UpdateSMSMPartys(ah._shipmentid);
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_76;
                return Json(new { message = returnMsg, IsOk = "Y" });
            }
            else
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
        }
        #endregion

        #region  DTAllocation
        public ActionResult DTAllocation()
        {
            string returnMsg = "";
            string uid = Request.Params["UId"];
            AllocationHelper ah = new AllocationHelper(uid);
            returnMsg = ah.GetDTAllocation();
            if (string.IsNullOrEmpty(returnMsg))
            {
                CommonManager.UpdateSMSMPartys(ah._shipmentid);
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_76;
                return Json(new { message = returnMsg, IsOk = "Y" });
            }
            else
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
        }

        private string InsetIntoSMPTY(string partytype, string partyno, string shipmentid, string uid)
        {
            string msg = string.Empty;
            EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("U_FID", uid);
            ei.Put("SHIPMENT_ID", shipmentid);
            string sql = "SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND GROUP_ID='TPV' AND CD=" + SQLUtils.QuotedStr(partytype);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_TYPE", dr["CD"]);
                ei.Put("TYPE_DESCP", dr["CD_DESCP"]);
                ei.Put("ORDER_BY", dr["ORDER_BY"]);
            }
            else
            {
                return partytype + "：@Resources.Locale.L_BookingAction_Controllers_180";
            }
            sql = "SELECT * FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(partyno);
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                ei.Put("PARTY_NAME", dr["PARTY_NAME"]);
                ei.Put("PARTY_MAIL", dr["PARTY_MAIL"]);
                ei.Put("PART_ADDR1", dr["PART_ADDR1"]);
                ei.Put("PART_ADDR2", dr["PART_ADDR2"]);
                ei.Put("PART_ADDR3", dr["PART_ADDR3"]);
                ei.Put("PARTY_ATTN", dr["PARTY_ATTN"]);
                ei.Put("PARTY_TEL", dr["PARTY_TEL"]);
                ei.Put("PARTY_NO", dr["PARTY_NO"]);
            }
            else
            {
                return partyno + "：@Resources.Locale.L_BookingAction_Controllers_181";
            }
            MixedList ml = new MixedList();
            ml.Add(ei);
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }
            return msg;
        }
        #endregion

        public ActionResult DefaultAllocation()
        {
            string returnMsg = string.Empty;
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            AllocationHelper ah = new AllocationHelper(uid);
            returnMsg = ah.GetAllocation();
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            returnMsg = "Allocation"+@Resources.Locale.L_SYS_SUCCESS;
            return Json(new { message = returnMsg, IsOk = "Y" });

        }

        #region 合并提单 &取消合并提单
        public ActionResult CombineBill()
        {
            string returnMsg = "";
            string shipmentids = Request.Params["Shipmentid"];
            List<string> list = new List<string>();
            shipmentids = shipmentids.Trim(';');
            string[] shipmentitems = shipmentids.Split(';');
            string newShipmentId = string.Empty;
            string sql = string.Format("SELECT SHIPMENT_INFO,COMBIN_SHIPMENT,SHIPMENT_ID,ISCOMBINE_BL FROM SMSM WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentitems));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string combinshipment = string.Empty;
            List<string> cbsmlist = new List<string>();

            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            string smitem = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                newShipmentId = Prolink.Math.GetValueAsString(dr["COMBIN_SHIPMENT"]);
                smitem = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                onAdd(cbsmlist, newShipmentId);
                onAdd(list, smitem);
                combinshipment = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                if ("Y".Equals(Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"])) || "C".Equals(Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"])))
                    list.Remove(smitem);
                if (string.IsNullOrEmpty(combinshipment)) continue;
                string[] combinitems = combinshipment.Split(',');
                foreach (string item in combinitems)
                {
                    onAdd(list, item);
                }
            }
            if (cbsmlist.Count > 1)
                return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_77, IsOk = "N" });
            shipmentitems = list.ToArray();// string.Join(",", list);
            if (newShipmentId.Length <= 13)
                newShipmentId = string.Empty;
            returnMsg = SMHandle.CombineBill(shipmentitems, GroupId, CompanyId, UserId, ref newShipmentId);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            string exceptionmsg = string.Empty;
            try
            {
                sql = "SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(newShipmentId);
                string sm_uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                bill.Share(sm_uid);
                exceptionmsg = @Resources.Locale.L_BookingActionController_Controllers_78;
            }
            catch (Exception ex)
            {
                exceptionmsg = @Resources.Locale.L_BookingActionController_Controllers_79 + ex.Message.ToString();
            }
            returnMsg = @Resources.Locale.L_BookingActionController_Controllers_80 + exceptionmsg;
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult SPellCombineBill()//SPellCombineBill
        {
            string returnMsg = "";
            string shipmentids = Request.Params["ShipmentId"];
            string removesm = Request.Params["RemoveSm"];
            string exceptionmsg = string.Empty;
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentids));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_BookingAction_Controllers_187  + shipmentids + @Resources.Locale.L_BookingActionController_Controllers_81, IsOk = "N" });
            string _newshipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBIN_SHIPMENT"]);
            returnMsg = SMHandle.SpellCombineBill(shipmentids, ref removesm, dt,UserId);
            string[] removednitems = removesm.Split(',');
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_189;
                try
                {
                    sql = "SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(_newshipmentid);
                    string sm_uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (string.IsNullOrEmpty(sm_uid))
                    {
                        MixedList ml = new MixedList();
                        EditInstruct ei = new EditInstruct("SMBID", EditInstruct.DELETE_OPERATION);
                        ei.PutKey("SHIPMENT_ID", _newshipmentid);
                        ml.Add(ei);
                        try
                        {
                            int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            exceptionmsg = ex.Message.ToString();
                        }
                    }
                    else
                    {
                        Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                        bill.Share(sm_uid);
                    }

                    sql = string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(removednitems));
                    DataTable rmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in rmdt.Rows)
                    {
                        sm_uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                        bill.Share(sm_uid);
                    }
                    exceptionmsg = @Resources.Locale.L_BookingActionController_Controllers_78;
                }
                catch (Exception ex)
                {
                    exceptionmsg = @Resources.Locale.L_BookingActionController_Controllers_79 + ex.Message.ToString();
                }
                returnMsg += exceptionmsg;
                return Json(new { message = returnMsg, IsOk = "Y" });
            }
            return Json(new { message = returnMsg, IsOk = "N" });
        }
        #endregion

        #region 合并Shipment & 取消合并Shipment
        public ActionResult CombineShipment()
        {
            string returnMsg = "";
            string shipmentids = Request.Params["Shipmentid"];
            shipmentids = shipmentids.Trim(';');
            string[] shipmentitems = shipmentids.Split(';');
            string newShipmentId = string.Empty;
            returnMsg = SMHandle.CombineShipment(shipmentitems, GroupId, CompanyId, Station, ref newShipmentId, UserId);
            if (!string.IsNullOrEmpty(returnMsg))
            {
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            returnMsg = @Resources.Locale.L_BaseBooking_Script_94 + @Resources.Locale.L_DNManage_Finish;
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult SPellCombineShipment()
        {
            string returnMsg = "";
            string shipmentids = Request.Params["ShipmentId"];
            string removedn = Request.Params["RemoveDn"];

            returnMsg = SMHandle.SpellCombineSM(shipmentids, removedn);
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_190;
                return Json(new { message = returnMsg, IsOk = "Y" });
            }
            return Json(new { message = returnMsg, IsOk = "N" });
        }
        # endregion
        #region
        public ActionResult HandIn()
        {
            string returnMsg = "";
            string shipmentids = Request.Params["Shipmentid"];
            shipmentids = shipmentids.Trim(';');
            string[] shipmentitems = shipmentids.Split(';');
            MixedList mlist = new MixedList();
            foreach (string shipmentid in shipmentitems)
            {
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("STATUS", "O");
                mlist.Add(ei);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    returnMsg = @Resources.Locale.L_EventSetup_Complete;
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                    return Json(new { message = returnMsg });
                }
            }
            return Json(new { message = returnMsg });
        }
        #endregion

        #region
        public ActionResult ShipmentDNQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string shipmentid = Request.Params["shipmentid"];

            string conditions = string.Format(" SHIPMENT_ID={0} AND ", SQLUtils.QuotedStr(shipmentid));
            conditions += GetBaseGroup();

            DataTable dt = ModelFactory.InquiryData("*", "SMDN", conditions, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmdnModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        #endregion

        #region 叫车
        public ActionResult CallCar()
        {
            string returnMessage = "";
            string shipmentid = Request.Params["ShipmentId"];
            string result = string.Empty;
            string exsit = Business.ReserveManage.ChkSmrv(shipmentid);
            if (exsit != "success")
            {
                //問user要不要重新叫車，如果重新叫車，會把原來的預約記錄刪掉再新增
            }
            List<string> idList = new List<string>();
            result = Business.ReserveManage.OrderTrucker(shipmentid, GroupId, CompanyId, Dep, Ext, UserId, idList);

            if (result != "success")
            {
                returnMessage = "Shipment ID: " + shipmentid + "," + result;
            }

            if (idList.Count > 0)
                SMSMIController.SendBookingOrCallMailList(idList.ToArray(), "ICS");
            return Json(new { message = returnMessage });
        }
        #endregion

        public ActionResult GetContainerInfo()
        {
            string shipmentid = Request["ShipmentId"];
            string sql = string.Format(@"SELECT * FROM SMRV WHERE SHIPMENT_ID IN(
                SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT={0})
                UNION SELECT * FROM SMRV WHERE SHIPMENT_ID={0} ORDER BY RESERVE_NO DESC", SQLUtils.QuotedStr(shipmentid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "SmrvModel"));
        }

        #region 提单核对
        public ActionResult BlCheckConfirm()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');

            string sql = string.Format("UPDATE SMSM SET BL_CHECK='Y' WHERE U_ID IN {0}", SQLUtils.Quoted(uids));

            try
            {
                int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg += ex.ToString();
                return Json(new { message = returnMsg, IsOk = "N" });
            }
            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingAction_Controllers_192;
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }
        #endregion

        #region 提单通知
        public ActionResult NoticeBillFreight()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');

            for (int i = 0; i < uids.Length; i++)
            {
                returnMsg = NoticeSPMail(uids[i]);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }

            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_82;
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public string NoticeSPMail(string uid)
        {
            string partyno = string.Empty;
            string partytype = string.Empty;
            string returnMsg = string.Empty;
            string mail_to = string.Empty;
            string shipmentid = string.Empty;
            string groupId = string.Empty;
            string cmp = string.Empty;
            string stn = string.Empty;
            string dep = string.Empty;
            string tran_type = string.Empty;
            DataTable maindt = GetSMByUid(uid);
            string iscombine_bl = string.Empty;
            if (maindt.Rows.Count > 0)
            {
                shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
                groupId = maindt.Rows[0]["GROUP_ID"].ToString();
                cmp = maindt.Rows[0]["CMP"].ToString();
                tran_type = maindt.Rows[0]["TRAN_TYPE"].ToString();
                iscombine_bl = maindt.Rows[0]["ISCOMBINE_BL"].ToString();
            }
            if ("S".Equals(iscombine_bl))
            {
                return string.Format(@Resources.Locale.L_BookingActionController_Controllers_841);
            }

            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN ('SP','BO')", SQLUtils.QuotedStr(shipmentid));
            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (maildt.Rows.Count <= 0) return string.Format(@Resources.Locale.L_BookingActionController_Controllers_85, shipmentid);

            string syssql = string.Format("SELECT U_EXT,U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(UserId));
            DataTable dt = OperationUtils.GetDataTable(syssql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return @Resources.Locale.L_BookingActionController_Controllers_86;
            string uext = dt.Rows[0]["U_EXT"].ToString();
            string mailcc = dt.Rows[0]["U_EMAIL"].ToString();
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                //1.多一個提單通知按鈕，可以mail通知forwarder及夾憱draft B/L 
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_TYPE"]);
                int serverNum = 0;
                List<EDOCFileItem> edocList = MailTemplate.GetEdoList(uid, groupId, cmp, "*", "", "BL",ref serverNum);//"*", ""
                if (edocList == null) return @Resources.Locale.L_BookingAction_Controllers_138  + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_891;
                if (edocList.Count <= 0)
                {
                    return @Resources.Locale.L_BookingAction_Controllers_138  + shipmentid + @Resources.Locale.L_BookingActionController_Controllers_891;
                }
                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, "Z");//Z 提单核对群组
                if (mailGroupDt.Rows.Count <= 0)
                {
                    return @Resources.Locale.L_BookingActionController_Controllers_47 + partyno +  @Resources.Locale.L_BookingActionController_Controllers_48;
                }
                mail_to = string.Empty;
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    string mailtoindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailtoindex))
                    {
                        mail_to += mailtoindex + ";";
                    }
                }
                EvenFactory.AddEven(MailManager.NoticeBill + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString() + "#" + mailcc, uid, MailManager.NoticeBill, null, 1, 0, mail_to, "", "");
            }
            MixedList mlist = new MixedList();
            if (!string.IsNullOrEmpty(uext))
            {
                string[] uexts = uext.Split('-');
                if (uexts.Length > 1)
                {
                    uext = UserId + " " + uexts[1];
                }
                else
                {
                    uext = UserId;
                }
            }

            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("DRAFT_BL_WIN", uext);
            ei.PutDate("BLNOTIFY_DATE", DateTime.Now);
            mlist.Add(ei);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return @Resources.Locale.L_BookingActionController_Controllers_90;
                }
            }
            return string.Empty;
        }
        #endregion

        #region 基础查询方法
        public DataTable GetSMByUid(string uid)
        {
            string sql = "SELECT * FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetSMByUids(string[] uids)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE U_ID IN {0} ", SQLUtils.Quoted(uids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetSMByShipmentId(string shipmentid)
        {
            string sql = "SELECT * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetSMByShipmentId(string []shipmentids)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetPTBySMUid(string[] uids)
        {
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID=(SELECT SHIPMENT_ID FROM SMSM WHERE U_ID IN {0}) ORDER BY ORDER_BY ASC", SQLUtils.Quoted(uids));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetPTByPartyType(string shipmentid, string partytype)
        {
            string[] partytypes = partytype.Split(';');
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN {1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.Quoted(partytypes));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public DataTable GetPTByShipmentid(string shipmentid)
        {
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} ", SQLUtils.QuotedStr(shipmentid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        #endregion

        [HttpPost]
        public ActionResult LNTComfirmUpload(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0) return Json(new { message = "error" });
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0) return Json(new { message = "error" });
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                mapping = BookingStatusManager.LNTConfirmMapping;
                MixedList ml = new MixedList();
                Dictionary<string, object> parm = new Dictionary<string, object>();
                ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleLNTBookingStatus);//处理LNTbooking
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml, parm);
                if (ml.Count <= 0)
                {
                    returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                }
                if (ml.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                        return Json(new { message = ex.Message, message1 = returnMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

        [HttpPost]
        public ActionResult LogisticUpload(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0) return Json(new { message = "error" });
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0) return Json(new { message = "error" });
                //string strExt = file.FileName.Split('.')[1].ToUpper();
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/Logistic/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                mapping = BookingStatusManager.LogisticsMapping;
                MixedList ml = new MixedList();
                MixedList partyml = new MixedList();
                Dictionary<string, object> parm = new Dictionary<string, object>();
                DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", GroupId, CompanyId);
                parm["bacodeDt"] = baseDt;
                parm["mixedlist"] = partyml;
                ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleLogisctImport);
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml, parm);
                if (ml.Count <= 0)
                {
                    returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                }
                partyml = (MixedList)parm["mixedlist"];
                for (int i = 0; i < partyml.Count; i++)
                {
                    ml.Add((EditInstruct)partyml[i]);
                }
                //string msgfocheck = string.Empty;
                //for (int i = 0; i < ml.Count; i++)
                //{
                //    if (ml[i] is EditInstruct)
                //    {
                //        EditInstruct ei = (EditInstruct)ml[i];
                //        string shipment_id = ei.Get("SHIPMENT_ID");
                //        string BO_CD = ei.Get("BO_CD");
                //        string SP_CD = ei.Get("SP_CD");
                //        string FS_CD = ei.Get("FS_CD");
                //        string BR_CD = ei.Get("BR_CD");
                //        string CR_CD = ei.Get("CR_CD");
                //        string BM_CD = ei.Get("BM_CD");
                //        string PPOD_CD = ei.Get("PPOD_CD");
                //        string PDEST_CD = ei.Get("PDEST_CD");

                //        string sql = string.Format("select Torder,BORDER from smsm where  SHIPMENT_ID={0}",SQLUtils.QuotedStr( shipment_id));
                //        DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //        if (dt != null && dt.Rows.Count > 0)
                //        {
                //          string   Torder=Prolink.Math.GetValueAsString( dt.Rows[0]["Torder"]);
                //          string BORDER = Prolink.Math.GetValueAsString(dt.Rows[0]["BORDER"]);
                //          if (!string.IsNullOrEmpty(CR_CD)&&!string.IsNullOrEmpty(Torder)&&!"N".Equals(Torder))
                //          {
                //              msgfocheck += shipment_id + ":" + "已叫车，无法修改CR信息"+"\r\n";
                //          }
                //          if (!string.IsNullOrEmpty(BR_CD) && !string.IsNullOrEmpty(BORDER) && !"M".Equals(BORDER))
                //          {
                //              msgfocheck += shipment_id + ":" + "已通知报关，无法修改BR信息" + "\r\n";
                //          }
                //        }
                        
                //        if (!string.IsNullOrEmpty(SP_CD) || !string.IsNullOrEmpty(BO_CD) || !string.IsNullOrEmpty(FS_CD))
                //        {
                //            string rvstatusSQL = string.Format("SELECT STATUS FROM SMRV WHERE SHIPMENT_ID =", SQLUtils.QuotedStr(shipment_id));
                //            string rvstatus = OperationUtils.GetValueAsString(rvstatusSQL, Prolink.Web.WebContext.GetInstance().GetConnection());
                //            bool statusb = false;
                //            switch (rvstatus)
                //            {
                //                case "D":
                //                case "R":
                //                case "C":
                //                case "I":
                //                case "G":
                //                case "V":
                //                case "":
                //                    statusb = true; break;
                //            }
                //            if (!statusb)
                //            {
                //                msgfocheck += shipment_id + ":" + "已封柜，无法修改FS，BO,SP信息" + "\r\n";
                //            }
                //        }
                //    }
                //}
                //if (!string.IsNullOrEmpty(msgfocheck))
                //    Json(new { message = msgfocheck });
                if (ml.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                        return Json(new { message = ex.Message, message1 = returnMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }
        //public void Save(string mapping, string filename, MixedList ml, Dictionary<string, object> parm = null, int StartRow = 0)
        //{
        //    DataTable dt = TrackingEDI.Business.ExcelHelper.ImportExcelToDataTable(filename, StartRow);
        //    //ParseEditInstruct(dt, mapping, ml, parm);
        //    foreach (DataRow dr in dt.Rows)
        //    {
 
        //    };

        //}
        public ActionResult CheckEtdDate()
        {
            string returnMsg = string.Empty;
            string trantype = string.Empty;
            string shipmentid = Request.Params["shipmentid"];
            string checktype = Prolink.Math.GetValueAsString(Request.Params["checktype"]);
            ResultData rd = TrackingEDI.Business.BookingStatusManager.CheckSMEtd(shipmentid, null, checktype);
            if (rd.IsSucceed)
            {
                return Json(new { IsOk = "Y" });
            }
            return Json(new { IsOk = "N" });
        }

        public ActionResult ChangeShipDate()
        {
            string returnMsg = @Resources.Locale.L_BookingAction_Controllers_204;
            string trantype = string.Empty;
            string shipmentid = Request.Params["shipmentid"];
            string cancelreson = Request.Params["cancelreson"];
            DataTable maindt = GetSMByShipmentId(shipmentid);
            string status = string.Empty;
            string iscombine_bl = string.Empty;
            if (maindt.Rows.Count > 0)
            {
                status = maindt.Rows[0]["STATUS"].ToString();
                iscombine_bl = maindt.Rows[0]["ISCOMBINE_BL"].ToString();
            }
            if ("Y".Equals(iscombine_bl) || "C".Equals(iscombine_bl))
                return Json(new { message = @Resources.Locale.L_BookingAction_Controllers_132  + shipmentid +  @Resources.Locale.L_BookingActionController_Controllers_91, IsOk = "N" });
            switch (status)
            {
                case "V":
                    return Json(new { message = @Resources.Locale.L_BookingAction_Controllers_132  + shipmentid +  @Resources.Locale.L_BookingActionController_Controllers_92, IsOk = "N" });
                case "A":
                case "B":
                    return Json(new { message = @Resources.Locale.L_BookingAction_Controllers_132 + shipmentid + @Resources.Locale.L_BookingAction_Controllers_162 + @Resources.Locale.L_BookingAction_Controllers_205, IsOk = "N" });
            }
            try
            {
                Helper.CancelBooking(shipmentid, cancelreson, false);
                string sql = string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                string ufid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, object> list = new Dictionary<string, object>();
                list.Add("UFid", ufid);
                list.Add("JobNo", shipmentid);
                list.Add("ExpObj", UserId);
                list.Add("ExpCd", "");
                list.Add("ExpType", "SM");
                list.Add("ExpReason", "BKC");
                list.Add("ExpText", @Resources.Locale.L_BookingActionController_Controllers_93);
                list.Add("Dep", Dep);
                list.Add("UserId", UserId);
                list.Add("GROUP_ID", GroupId);
                list.Add("CMP", CompanyId);
                ErrMsgController err = new ErrMsgController();
                err.InsrtErrMsgByDic(list);

            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message, IsOk = "N" });
            }
            //清除物流费用
            try
            {
                string sql = string.Format(@"DELETE  FROM SMBID WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMsg = ex.ToString();
                return Json(new { message = ex.Message, IsOk = "N" });
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult NoticeCombineBill()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');

            for (int i = 0; i < uids.Length; i++)
            {
                returnMsg = NoticeCombinMail(uids[i]);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }

            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_94;
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public string NoticeCombinMail(string uid)
        {
            string partyno = string.Empty;
            string partytype = string.Empty;
            string returnMsg = string.Empty;
            string mail_to = string.Empty;
            string shipmentid = string.Empty;
            string groupId = string.Empty;
            string cmp = string.Empty;
            string stn = string.Empty;
            string dep = string.Empty;
            string tran_type = string.Empty;
            string iscombine_bl = string.Empty;
            DataTable maindt = GetSMByUid(uid);
            if (maindt.Rows.Count > 0)
            {
                shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
                groupId = maindt.Rows[0]["GROUP_ID"].ToString();
                cmp = maindt.Rows[0]["CMP"].ToString();
                tran_type = maindt.Rows[0]["TRAN_TYPE"].ToString();
                iscombine_bl = maindt.Rows[0]["ISCOMBINE_BL"].ToString();
            }

            if (!"C".Equals(iscombine_bl) && !"Y".Equals(iscombine_bl))
            {
                return string.Format(@Resources.Locale.L_BookingActionController_Controllers_951, shipmentid);
            }

            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN ('SP','BO')", SQLUtils.QuotedStr(shipmentid));
            DataTable maildt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (maildt.Rows.Count <= 0) return string.Format(@Resources.Locale.L_BookingActionController_Controllers_961, shipmentid);

            string syssql = string.Format("SELECT U_EXT,U_EMAIL FROM SYS_ACCT WHERE U_ID ={0}", SQLUtils.QuotedStr(UserId));
            DataTable dt = OperationUtils.GetDataTable(syssql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return @Resources.Locale.L_BookingActionController_Controllers_86;
            string uext = dt.Rows[0]["U_EXT"].ToString();
            string mailcc = dt.Rows[0]["U_EMAIL"].ToString();
            for (int i = 0; i < maildt.Rows.Count; i++)
            {
                partyno = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_NO"]);
                partytype = Prolink.Math.GetValueAsString(maildt.Rows[i]["PARTY_TYPE"]);
                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, GroupId, tran_type);
                if (mailGroupDt.Rows.Count <= 0)
                {
                    return @Resources.Locale.L_BookingActionController_Controllers_47 + partyno +  @Resources.Locale.L_BookingActionController_Controllers_48;
                }
                mail_to = string.Empty;
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    string mailtoindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailtoindex))
                    {
                        mail_to += mailtoindex + ";";
                    }
                }
                EvenFactory.AddEven(MailManager.BLC + "#" + uid + "#" + partyno + "#" + GroupId + "#" + CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString() + "#" + mailcc, uid, "MM", null, 1, 0, mail_to, shipmentid + "：@Resources.Locale.L_DNManage_CombBLNoti", "");
            }
            MixedList mlist = new MixedList();
            if (!string.IsNullOrEmpty(uext))
            {
                string[] uexts = uext.Split('-');
                if (uexts.Length > 1)
                {
                    uext = UserId + " " + uexts[1];
                }
                else
                {
                    uext = UserId;
                }
            }

            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.Put("SORDER", "S");  //记录是否发送合并提单通知给货代
            ei.Put("CORDER", "U");  //记录是否发送合并提单通知给货代
            ei.Put("STATUS", "U");  //状态更新到订舱的主状态
            ei.PutDate("SDATE", DateTime.Now);
            ei.Put("SORDER_BY", uext);
            mlist.Add(ei);
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return @Resources.Locale.L_BookingActionController_Controllers_97;
                }
            }
            return string.Empty;
        }

        public ActionResult ASDReportHandler()
        {
            int year = Prolink.Math.GetValueAsInt(Request.Params["Year"]);
            int month = Prolink.Math.GetValueAsInt(Request.Params["Month"]);

            string asdtype = Request.Params["AsdType"];
            int nowyear = DateTime.Now.Year;
            int nowmonth = DateTime.Now.Month;
            if (year > nowyear) return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_98, IsOk = "N" });
            if (year == nowyear)
            {
                if (month > nowmonth) return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_98, IsOk = "N" });
            }
            string stryear = year.ToString();
            string strmonth = month.ToString();
            if (strmonth.Length == 1)
                strmonth = "0" + strmonth;
            string condition = string.Empty;
            MixedList mlist = new MixedList();
            string sql = string.Format("SELECT COUNT(1) FROM ASD_TASK WHERE CMP={0} AND YEAR={1} AND MONTH={2} AND ASD_TYPE={3}",
                SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(stryear), SQLUtils.QuotedStr(strmonth), SQLUtils.QuotedStr(asdtype));
            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            int lastmonth = month + 1;
            int lastyear = year;
            if (lastmonth > 12)
            {
                lastyear = year + 1;
                lastmonth = 1;
            }
            string strlsthmonth = lastmonth.ToString();
            if (strlsthmonth.Length == 1)
                strlsthmonth = "0" + strlsthmonth;
            DateTime etdfrom = Prolink.Math.GetValueAsDateTime(year + strmonth + "01");
            DateTime etdto = Prolink.Math.GetValueAsDateTime(lastyear + strlsthmonth + "01");
            EditInstruct ei = null;
            if (count > 0)
            {
                ei = new EditInstruct("ASD_TASK", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("YEAR", stryear);
                ei.PutKey("MONTH", month);
                ei.PutKey("CMP", CompanyId);
                ei.Put("RESULT_STATUS", string.Empty);
                ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmmss"));
                ei.Put("REMARK", string.Empty);
                ei.Put("FILEPATH", string.Empty);
                ei.Put("TEMP_OK", string.Empty);
            }
            else
            {
                ei = new EditInstruct("ASD_TASK", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("GROUP_ID", GroupId);
                ei.Put("CMP", CompanyId);
                ei.Put("USERID", UserId);
                ei.Put("BASE_CONDITION", condition);
                ei.Put("ASD_TYPE", asdtype);
                ei.Put("CREATE_BY", UserId);
                ei.Put("IO_TYPE", "I");
                string fileNames = "INBOUND_ASDReport_" + stryear + strmonth;
                if ("Q".Equals(asdtype))
                    fileNames = "INBOUND_ASD Accrued Report_" + stryear + strmonth;
                else if ("B".Equals(asdtype))
                    fileNames = "INBOUND_ASD Payment Report_" + stryear + strmonth;
                ei.Put("FILE_NAMES", fileNames);
                ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmmss"));
                ei.Put("MONTH", month);
                ei.Put("YEAR", stryear);
            }
            ei.Put("STR_MONTH", strmonth);
            ei.PutDate("ETD_FROM", etdfrom);
            ei.PutDate("ETD_TO", etdto);
            mlist.Add(ei);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_99 + ex.Message, IsOk = "N" });
            }
            return Json(new { message = @Resources.Locale.L_BookingActionController_Controllers_100, IsOk = "Y" });
        }

        public void ASDReportGetFile()
        {
            string uid = Request.Params["UId"];
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT FILEPATH,FILE_NAMES FROM ASD_TASK WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string xlsFile = dt.Rows[0]["FILEPATH"].ToString();
            string strName = dt.Rows[0]["FILE_NAMES"].ToString();
            strName = strName + ".xls";
            using (FileStream fs = new FileStream(xlsFile, FileMode.Open))
            {
                byte[] bytes = new byte[(int)fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(strName, System.Text.Encoding.UTF8));
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
            }
        }

        public ActionResult GetASDFiles()
        {
            string asdtype = Request.Params["Type"];
            string sql = string.Format("SELECT * FROM ASD_TASK WHERE CMP={0} AND ASD_TYPE={1} ORDER BY YEAR,MONTH DESC", SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(asdtype));
            if (string.IsNullOrEmpty(asdtype))
            {
                sql = string.Format("SELECT * FROM ASD_TASK WHERE CMP={0} AND (ASD_TYPE IS NULL OR ASD_TYPE='') ORDER BY YEAR,MONTH DESC", SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(asdtype));
            }
            DataTable asdDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(asdDt, "AsdTaskModel"));
        }

        public ActionResult CBMAndGwUpload(FormCollection form)
        {
            string returnMessage = "success";
            string type=Prolink.Math.GetValueAsString(Request["type"]);
            if (Request.Files.Count == 0) return Json(new { message = "error" });
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0) return Json(new { message = "error" });
                //string strExt = file.FileName.Split('.')[1].ToUpper();
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string path = Server.MapPath("~/FileUploads/GwAndCbm/");
                bool exists = System.IO.Directory.Exists(path);
                if (!exists)
                    System.IO.Directory.CreateDirectory(path);
                string excelFileName = string.Format("{0}.{1}", path + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                mapping = BookingStatusManager.GwAndCbmMapping;
                MixedList ml = new MixedList();
                MixedList partyml = new MixedList();
                Dictionary<string, object> parm = new Dictionary<string, object>();
                DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", GroupId, CompanyId);
                parm["bacodeDt"] = baseDt;
                parm["mixedlist"] = partyml;
                parm["UserId"] = UserId;
                ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleGwAndCbmImport);
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml, parm);
                if (ml.Count <= 0)
                {
                    returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                }
                partyml = (MixedList)parm["mixedlist"];
                for (int i = 0; i < partyml.Count; i++)
                {
                    ml.Add((EditInstruct)partyml[i]);
                }
                if (type == "A" || type == "E")
                {
                    MixedList _ml = new MixedList();
                    for (int i = 0; i < ml.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)ml[i];
                        string[] name = ei.getNameSet();
                        string[] needcalculate = { "GW", "CW", "CBM" };
                        foreach (string ne in needcalculate)
                        {
                            double Oval = 0;
                            if (name.Contains(ne))
                            {
                                Oval = Prolink.Math.GetValueAsDouble(ei.Get(ne));
                                ei.Put(ne, ResetCalculation(Oval, type));
                            }
                        }
                        _ml.Add(ei);
                    }
                    ml = _ml;
                }
                if (ml.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                        return Json(new { message = ex.Message, message1 = returnMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }
        public double ResetCalculation(double val, string type)
        {
            var newval = Math.Floor(val);
            if (type == "A" || (type == "E" & val <= 20))
            {
                if (val <= newval + 0.5 && val > newval)
                {
                    return newval + 0.5;
                }
                else if (val > newval + 0.5 && val < newval + 1)
                {
                    return newval + 1;
                }
            }
            if (type == "E" && val > 20)
            {
                if (val > newval)
                {
                    return newval + 1;
                }
            }
            return val;
        }

        public ActionResult O2IAction()
        {
            string shipmentid = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string []shipmenids = shipmentid.Split(',');
            string message = string.Empty;
            string isok = "N";
            MixedList ml = new MixedList();
            foreach (string si in shipmenids)
            {
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", si);
                isok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc(si,"", ei);
                if (isok == "Y")
                {
                    ei.Put("IS_OK", "Y");
                }
                else if (isok == "S")
                {
                    ei.Put("IS_OK_ISF", "Y");
                }
                else if (isok == "N")
                {
                    ei.Put("IS_OK", "N");
                    ei.PutDate("ICREATE_DATE", DateTime.Now);
                    ei.Put("ICREATE_BY", UserId);
                }
                else
                {
                    message += string.Format("{0} Send InBound Fail! The reason is {1} /r/n", si, isok);
                }
            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex });
            }
            if(string.IsNullOrEmpty(message))
                return Json(new { msg = "success" });
            return Json(new { msg = message });
        }

        /// <summary>
        /// 改港
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeToPOD()
        {
            string returnMsg = "";
            string uid = Prolink.Math.GetValueAsString(Request.Params["shipmentid"]);
            string changeRmk = Prolink.Math.GetValueAsString(Request.Params["cancelreson"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');
            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Dep = Dep
            };
            for (int i = 0; i < uids.Length; i++)
            {
                returnMsg = SMHandle.ChangePodAction(uids[i], changeRmk, userinfo);
                if (!string.IsNullOrEmpty(returnMsg))
                {
                    return Json(new { message = returnMsg, IsOk = "N" });
                }
            }

            if (string.IsNullOrEmpty(returnMsg))
            {
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_82;
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        [HttpPost]
        public ActionResult BatchChangePOD(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0) return Json(new { message = "error" });
            try
            {
                string mapping = string.Empty;
                var file = Request.Files[0];
                if (file.ContentLength == 0) return Json(new { message = "error" });
                //string strExt = file.FileName.Split('.')[1].ToUpper();
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                if (Directory.Exists(Server.MapPath("~/FileUploads/changePod")) == false)//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(Server.MapPath("~/FileUploads/changePod"));
                }
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/changePod/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);
                mapping = BookingStatusManager.ChangePodMapping;
                MixedList ml = new MixedList();
                MixedList partyml = new MixedList();
                Dictionary<string, object> parm = new Dictionary<string, object>();
                DataTable baseDt = TrackingEDI.Mail.MailTemplate.GetBaseData("'TCAR'", GroupId, CompanyId);
                parm["bacodeDt"] = baseDt;
                parm["mixedlist"] = partyml;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                parm["reasonDic"] = dictionary;
                ExcelParser.RegisterEditInstructFunc(mapping, BookingStatusManager.HandleChangePod);
                ExcelParser ep = new ExcelParser();
                ep.Save(mapping, excelFileName, ml, parm);
                if (ml.Count <= 0)
                {
                    returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                }
                partyml = (MixedList)parm["mixedlist"];
                for (int i = 0; i < partyml.Count; i++)
                {
                    ml.Add((EditInstruct)partyml[i]);
                }
                if (ml.Count > 0)
                {
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        UserInfo userinfo = new UserInfo
                        {
                            UserId = UserId,
                            CompanyId = CompanyId,
                            GroupId = GroupId,
                            Dep = Dep
                        };
                        dictionary = (Dictionary<string, string>)parm["reasonDic"];
                        foreach (var reson in dictionary)
                        {
                            SMHandle.ChangePodAction(reson.Key, reson.Value, userinfo);
                        } 
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                        return Json(new { message = ex.Message, message1 = returnMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

    }
}
