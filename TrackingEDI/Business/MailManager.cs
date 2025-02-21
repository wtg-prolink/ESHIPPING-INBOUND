using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using TrackingEDI.Mail;


namespace TrackingEDI.Business
{
    public class MailManager
    {
        #region 注册类型
        /// <summary>
        /// 发送booking mail的注册类型：代码SB
        /// </summary>
        public static string SendBooking = "SB";

        /// <summary>
        /// 发送air booking mail的注册类型：代码SAB
        /// </summary>
        public static string SendAirBook = "SAB";

        /// <summary>
        /// 发送FLC booking mail的注册类型：代码SFB
        /// </summary>
        public static string SendFCLBook = "SFB";

        /// <summary>
        /// 发送LCL booking mail的注册类型：代码SLB
        /// </summary>
        public static string SendLCLBook = "SLB";

        /// <summary>
        /// 发送快递 booking mail的注册类型：代码SEB
        /// </summary>
        public static string SendExpBook = "SEB";

        /// <summary>
        /// 发送内贸 booking mail的注册类型：代码SDB
        /// </summary>
        public static string SendSdbBook = "SDB";

        /// <summary>
        /// 路线规划 发送mail的类型：代码RN
        /// </summary>
        public static string RouteNotify = "RN";

        /// <summary>
        /// 文件夹档
        /// </summary>
        public static string DocSend = "DOC";

        /// <summary>
        /// 询价通知
        /// </summary>
        public static string QuotNotify = "QN";

        /// <summary>
        /// 帐单审核
        /// </summary>
        public static string BILLAp = "BILL_AP";

        /// <summary>
        /// DN签核与退回
        /// </summary>
        public static string DNNotify = "DN";

        /// <summary>
        /// LST退回
        /// </summary>
        public static string SMVoid = "SMV";
        /// <summary>
        /// 自动计价
        /// </summary>
        public static string AutoCalcul = "AC";
        /// <summary>
        /// 过账失败
        /// </summary>
        public static string PBNotify = "PB";

        /// <summary>
        /// 預約叫車
        /// </summary>
        public static string RVNotify = "RV";

        /// <summary>
        /// 发送报关
        /// </summary>
        public static string BrokerNotify = "BR";

        /// <summary>
        /// 发送取消订舱
        /// </summary>
        public static string CancelBooking = "CABK";

        /// <summary>
        /// 发送取消订舱
        /// </summary>
        public static string CancelBroker = "CABR";

        /// <summary>
        /// 发送提单通知
        /// </summary>
        public static string NoticeBill = "NoticeBL";

        /// <summary>
        /// 合併提單通知
        /// </summary>
        public static string BLC = "BLC";

        /// <summary>
        /// 帐单比对结果
        /// </summary>
        public static string BILLRejectNotify = "BILL_RJ";

        /// <summary>
        /// 发送使用者註冊資訊
        /// </summary>
        public static string UserInfo = "UInfo";

        /// <summary>
        /// 发送edoc資訊
        /// </summary>
        public static string EdocList = "EdocList";

        // <summary>
        /// 帐单比对结果
        /// </summary>
        public static string InboundBILLPassNotify = "IB_BILL";

        // <summary>
        /// 帐单比对结果
        /// </summary>
        public static string InboundBILLRejectNotify = "IB_BILL_RJ";
        // <summary>
        /// 报关报价拒绝
        /// </summary>
        public static string QuotRefuse_B = "QRB";
        // <summary>
        /// 拖卡车报价拒绝
        /// </summary>
        public static string QuotRefuse_C = "QRC";
        // <summary>
        /// local报价拒绝
        /// </summary>
        public static string QuotRefuse_X = "QRX";
        // <summary>
        /// 海运报价拒绝
        /// </summary>
        public static string QuotRefuse_O = "QRO";
        // <summary>
        /// local报价通知
        /// </summary>
        public static string QuotNotify_X = "QNX";
        // <summary>
        /// 海运报价通知
        /// </summary>
        public static string QuotNotify_O = "QNO";
        // <summary>
        /// 报关报价通知
        /// </summary>
        public static string QuotNotify_B = "QNB";
        // <summary>
        /// 拖卡车报价通知
        /// </summary>
        public static string QuotNotify_C = "QNC";


        public static string IB_QuotRefuse_B = "IBQRB";
        public static string IB_QuotRefuse_C = "IBQRC";
        public static string IB_QuotRefuse_X = "IBQRX";
        public static string IB_QuotRefuse_O = "IBQRO";
        public static string IB_QuotNotify_X = "IBQNX";
        public static string IB_QuotNotify_O = "IBQNO";
        public static string IB_QuotNotify_B = "IBQNB";
        public static string IB_QuotNotify_C = "IBQNC";
        // <summary>
        /// 帐单审核
        /// </summary>
        public static string InboundBILLAp = "IB_BILL_AP";

        // <summary>
        /// 帐单审核
        /// </summary>
        public static string ContractAp = "CONT_AP";

        // <summary>
        /// 取消叫車
        /// </summary>
        public static string RVC = "RVC";

        // <summary>
        /// 進口报价通知
        /// </summary>
        public static string Inquery_Quotation = "IQQT";

        public static string CHANGE_POD = "COD_POD";

        /// <summary>
        /// Inbound Intermodal邮件类型
        /// </summary>
        public static string INTERMODAL_CALLCAR = "IICS";

        /// <summary>
        /// Inbound LSP Confirm Auto Send Broker Error 邮件类型
        /// </summary>
        public static string InboundNotifyBrokerError = "IBR_EROR";
        #endregion

        private string _folder = "DownFiles/MailTemp";

        /// <summary>
        /// 发送订舱有关的Mail  Mail代号MM
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendBookingMail(MailData data, MixedList ml)
        {
            try
            {
                string partytype=string.Empty;
                string mailType = data.Keys[1];
                string uid = data.Keys[2];
                string partyno = data.Keys[3];
                string groupid = data.Keys[4];
                string cmp= data.Keys[5];
                partytype = data.Keys[6];
                string mailcc = string.Empty;
                if (data.Keys.Length >= 9)
                {
                    mailcc = data.Keys[8];
                }
                IMailTemplateParse parse = new DefaultMailParse();

                string sql = @"SELECT SMSM.*,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FC')CUSTOMER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='SH')SHIPPER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE IN('SP','BO'))FORWARDER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FS')CARRIER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='BR')BROKER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CR')TRUCKER
                 FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string shipmentid = Prolink.Math.GetValueAsString(maindt.Rows[0]["SHIPMENT_ID"]);
                Dictionary<string, string> map = new Dictionary<string, string>();
                GetSMDNPInfoToMap(ref map, uid);

                map["SMCUFT_TD"] = GetSMCUFTDt(maindt.Rows[0]);
                string trantype=Prolink.Math.GetValueAsString(maindt.Rows[0]["TRAN_TYPE"]);
                string lspno=Prolink.Math.GetValueAsString(maindt.Rows[0]["LSP_NO"]);
                map["LSP_MAIL"] = GetMailByPartyNo(lspno, groupid, trantype);
                string transmode=Prolink.Math.GetValueAsString(maindt.Rows[0]["TRANSACTE_MODE"]);
                maindt.Columns["TRANSACTE_MODE"].MaxLength = 50;
                switch(transmode){
                    case "A":
                        transmode="一般贸易";
                        break;
                        case "B":
                        transmode="进料对口";
                        break;
                        case "D":
                        transmode="进料复出";
                        break;
                         case "O":
                        transmode="其它";
                        break;
                }
                maindt.Rows[0]["TRANSACTE_MODE"]=transmode;
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                if (mailType.Equals(MailManager.BrokerNotify))
                {
                    sql = string.Format("SELECT SMRV.CNTR_NO FROM SMRV WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentid));
                    DataTable smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string cntrnos = string.Empty;
                    foreach (DataRow dr in smrvdt.Rows)
                    {
                        cntrnos += GetSemicolonVal(dr["CNTR_NO"]);
                    }
                    cntrnos = cntrnos.Trim(';');
                    map["CNTR_NO"] = cntrnos;
                    list = GetEdocSend(maindt, partytype, partyno,true);
                }
                else
                {
                    list = GetEdocSend(maindt, partytype, partyno);
                }
                try
                {
                    if (list.Count > 0)
                    {
                        SetDocUrl(ref map, ref list);
                    }
                    else
                    {
                        if (mailType.Equals("AB") || mailType.Equals("DB") || mailType.Equals("EB") || mailType.Equals("FB") ||
                            mailType.Equals("LB") || mailType.Equals("RB") || mailType.Equals("TB"))
                        {
                            //如果没有档案就新增一个对于FCL/LCL  内贸的用中文bookingfrom 要实现自动归档
                            EdocHelper edochelper = new EdocHelper();
                            string filetype = "BF";
                            Result result = edochelper.CreateBFReport(shipmentid, filetype);
                            if (!result.Success)    //发送之前先归档一次，如果归档失败，再用排程去归档
                            {
                                //加入排程的Table中
                                DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT 1 FROM SM_AUTOEDOC WHERE JOB_NO={0} AND FILE_TYPE={1} AND IS_OK!='Y'",
                        SQLUtils.QuotedStr(maindt.Rows[0]["U_ID"].ToString()),
                        SQLUtils.QuotedStr(filetype)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (dt.Rows.Count == 0)
                                {
                                    Prolink.DataOperation.OperationUtils.Logger.WriteLog(result.Message);
                                    EditInstruct ei = new EditInstruct("SM_AUTOEDOC", EditInstruct.INSERT_OPERATION);
                                    ei.Put("U_ID", Guid.NewGuid().ToString());
                                    ei.Put("SHIPMENT_ID", shipmentid);
                                    ei.Put("FILE_TYPE", "BF");
                                    ei.Put("GROUP_ID", groupid);
                                    ei.Put("CMP", cmp);
                                    ei.Put("JOB_NO", uid);
                                    if ("T".Equals(trantype))
                                    {
                                        ei.Put("REPORT_NAME", "Booking form 中文");
                                        ei.Put("REPORT_ID", "FCL03");
                                    }
                                    else
                                    {
                                        ei.Put("REPORT_NAME", "Booking Form ");
                                        ei.Put("REPORT_ID", "FCL01");
                                    }
                                    ei.Put("EXPORT_FILE_TYPE", "pdf");
                                    ei.Put("ADD_RESON", result.Message);
                                    ei.Put("CREATE_BY", "Booking_Mail_Send");
                                    ei.PutDate("CREATE_DATE", DateTime.Now);
                                    ml.Add(ei);
                                }
                            }
                            else
                            {
                                list = GetEdocSend(maindt, partytype, partyno);
                                SetDocUrl(ref map, ref list);
                            }
                        }
                    }
                }
                catch { }
                string body = MailTemplate.GetMailBody(mailType, groupid,cmp);
                MailTemplate.ChageDateToString(maindt, new string[] { "DN_ETD", "PRODUCT_DATE" });
                DataTable baseDt = MailTemplate.GetBaseData("'TRGN','TNT'", groupid,cmp);
                maindt.Columns["TRAN_TYPE"].MaxLength = 50;
                maindt.Columns["REGION"].MaxLength = 50;
                maindt.Columns["STN"].MaxLength = 70;
                foreach (DataRow dr in maindt.Rows)
                {
                    string ptsql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["STN"])));
                    dr["STN"]=OperationUtils.GetValueAsString(ptsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    dr["TRAN_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]));
                    dr["REGION"] = MailTemplate.GetBaseCodeValue(baseDt, "TRGN", Prolink.Math.GetValueAsString(dr["REGION"]));
                    Func<string, string,string> ReplacePort = (fieldname, replacename) =>
                    {
                        string value = Prolink.Math.GetValueAsString(dr[fieldname]);
                        if (string.IsNullOrEmpty(value))
                            dr[fieldname] = dr[replacename];
                        return string.Empty;
                    };
                    ReplacePort("POL_CD", "PPOL_CD");
                    ReplacePort("POL_NAME", "PPOL_NAME");
                    ReplacePort("POD_CD", "PPOD_CD");
                    ReplacePort("POD_NAME", "PPOD_NAME");
                    ReplacePort("DEST_CD", "PDEST_CD");
                    ReplacePort("DEST_NAME", "PDEST_NAME");
                }
                data.Body = parse.Parse(maindt, null, body, map);
                if (data.ProcessTimes > 0)
                {
                    data.Subject = "Revised " + data.Subject;
                }
                data.MailClient.Send(data.Subject, data.To, mailcc, data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "=>" + e.InnerException.Message;
                EvenFactory.SaveMailLog(data, msg, EvenFactory.Fail, ml);
                return msg;
            }
            return string.Empty;
        }

        private static string SetDocUrl(ref Dictionary<string, string> map,ref List<Dictionary<string, string>> list)
        {
            string urls = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                string url = list[i]["FileUrl"];
                if (!string.IsNullOrEmpty(url))
                {
                    urls += string.Format("<a href=\"{0}\">{1}</a> ", url, list[i]["FileName"]);
                }
            }
            map["DOC_TEXT"] = "夹档:";
            map["DOC_HTML"] = urls;
            return urls;
        }

        public static string GetSMCUFTDt(DataRow smrow){
            if(smrow==null)return string.Empty;
            string combineinfo=Prolink.Math.GetValueAsString(smrow["COMBINE_INFO"]);
            string[] combines=combineinfo.Split(new char[]{',',';'});
            string sql = string.Format("SELECT * FROM SMCUFT WHERE DN_NO IN {0}", SQLUtils.Quoted(combines));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            StringBuilder columpars = new StringBuilder();

            columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
			                <td class='title'>长(cm)</td>
			                <td class='title'>宽(cm)</td>
			                <td class='title'>高(cm)</td>
			                <td class='title'>件数</td>
                            <td class='title'>单位</td>
                            <td class='title'>体积(m3)</td>
		                </tr>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                columpars.Append("<tr><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["L"]));
                columpars.Append("</td><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["W"]));
                columpars.Append("</td><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["H"]));
                columpars.Append("</td><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["PKG"]));
                columpars.Append("</td><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["PKG_UNIT"]));
                columpars.Append("</td><td>");
                columpars.Append(Prolink.Math.GetValueAsString(dr["VW"]));
                columpars.Append("</td></tr>");
            }
            columpars.Append("</tbody></table>");
            return columpars.ToString();
        }

        public static string GetSemicolonVal(object obj)
        {
            string val = Prolink.Math.GetValueAsString(obj);
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }
            return val + ";";
        }

        public static string SendCancelMail(MailData data, MixedList ml)
        {
            try
            {
                string partytype = string.Empty;
                string mailType = data.Keys[1];
                string uid = data.Keys[2];
                string partyno = data.Keys[3];
                string groupid = data.Keys[4];
                string cmp = data.Keys[5];
                partytype = data.Keys[6];
                IMailTemplateParse parse = new DefaultMailParse();

                string sql = @"SELECT SMSM.*,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FC')CUSTOMER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='SH')SHIPPER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE IN('SP','BO'))FORWARDER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FS')CARRIER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='BR')BROKER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CR')TRUCKER
                 FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, string> map = new Dictionary<string, string>();
                string body = MailTemplate.GetMailBody(mailType, groupid, cmp);
                data.Body = parse.Parse(maindt, null, body, map);
                if (data.ProcessTimes > 0)
                {
                    data.Subject = "Revised " + data.Subject;
                }
                data.MailClient.Send(data.Subject, data.To, "", data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 报单发送
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendDraftBLMail(MailData data, MixedList ml)
        {
            string mailtype = MailManager.NoticeBill;
            try
            {
                string mailType = data.Keys[1];
                string uid = data.Keys[2];
                string partyno = data.Keys[3];
                string groupid = data.Keys[4];
                string cmp = data.Keys[5];
                string partytype = data.Keys[6];
                string mailcc = string.Empty;
                if (data.Keys.Length >= 9)
                {
                    mailcc = data.Keys[8];
                }
                string type = "BL";
                IMailTemplateParse parse = new DefaultMailParse();

                string sql = @"SELECT SMSM.*,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FC')CUSTOMER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='SH')SHIPPER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE IN('SP','BO'))FORWARDER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FS')CARRIER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='BR')BROKER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CR')TRUCKER,
                (SELECT TOP 1 BL_TYPE FROM SMSIM WHERE PROFILE=SMSM.PROFILE_CD)DRAFTBL_TYPE 
                 FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string shipmentid = Prolink.Math.GetValueAsString(maindt.Rows[0]["SHIPMENT_ID"]);
                Dictionary<string, string> map = new Dictionary<string, string>();
                GetSMDNPInfoToMap(ref map,uid);

                List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(uid, groupid, cmp, "*", "", type);
                try
                {
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog("获取DraftBL文档文档数："+list.Count+"  "+uid+","+ groupid+", "+cmp+", *, "+ type+")");
                }catch(Exception ex){
                }
                try
                {
                    string urls = string.Empty;
                    if (list.Count > 0)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string url = list[i]["FileUrl"];
                            if (!string.IsNullOrEmpty(url))
                            {
                                urls += string.Format("<a href=\"{0}\">{1}</a> ", url, list[i]["FileName"]);
                            }
                        }
                        map["DOC_TEXT"] = "夹档:";
                        map["DOC_HTML"] = urls;
                    }
                }
                catch { }
                string body = MailTemplate.GetMailBody(mailType, groupid, cmp);
                MailTemplate.ChageDateToString(maindt, new string[] { "DN_ETD" });
                DataTable baseDt = MailTemplate.GetBaseData("'TRGN','TNT'", groupid, cmp);
                maindt.Columns["TRAN_TYPE"].MaxLength = 50;
                maindt.Columns["REGION"].MaxLength = 50;
                foreach (DataRow dr in maindt.Rows)
                {
                    string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);

                    dr["TRAN_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]));
                    dr["REGION"] = MailTemplate.GetBaseCodeValue(baseDt, "TRGN", Prolink.Math.GetValueAsString(dr["REGION"]));
                }
                data.Body = parse.Parse(maindt, null, body, map);
                string bookingno= Prolink.Math.GetValueAsString(maindt.Rows[0]["SO_NO"]);
                data.Subject = shipmentid + "(" + bookingno + ") 提单核对通知";
                if (data.ProcessTimes > 0)
                {
                    data.Subject = "Revised " + data.Subject;
                }
                data.MailClient.Send(data.Subject, data.To, mailcc, data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        public static string GetMailByPartyNo(string partyno,string groupid,string trantype)
        {
            DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, groupid, trantype);
            if (mailGroupDt.Rows.Count <= 0)
            {
                return string.Empty;
            }
            List<string> maillist = new List<string>();
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };

            foreach (DataRow mailGroup in mailGroupDt.Rows)
            {
                onAdd(maillist,Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]));
            }
            return string.Join(";", maillist);
        }

        public static void GetSMDNPInfoToMap(ref  Dictionary<string, string> map,string uid)
        {
            string sql = string.Format(@"SELECT DN_NO,UL,DU,RESOLUTION,OHS_CODE FROM SMDNP WHERE DN_NO IN(
                SELECT SMDN.DN_NO FROM SMDN,SMSM WHERE SMDN.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSM.U_ID={0})", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string dnno = string.Empty;
            List<string> resolist = new List<string>();
            List<string> dnnolist = new List<string>();
            List<string> ullist = new List<string>();
            List<string> dulist = new List<string>();
            List<string> ohslist = new List<string>();
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            foreach (DataRow dr in dt.Rows)
            {
                onAdd(resolist, dr["RESOLUTION"].ToString());
                onAdd(dnnolist, dr["DN_NO"].ToString());
                onAdd(ullist, dr["UL"].ToString());
                onAdd(dulist, dr["DU"].ToString());
                onAdd(ohslist, dr["OHS_CODE"].ToString());
            }
            map["DN_NO"] = string.Join(";", dnnolist); ;
            map["RESOLUTION"] = string.Join(";", resolist);
            map["UL"] = string.Join(";", ullist);
            map["DU"] = string.Join(";", dulist);
            map["OHS_CODE"] = string.Join(";", ohslist);
        }

        public static List<Dictionary<string, string>> GetEdocSend(DataTable dt, string partytype, string partyno,bool iscombine=false)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            string uid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            string groupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            string cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            string combineinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);
            string sql = string.Format("SELECT * FROM TKPDM WHERE GROUP_ID={0} AND CMP={1} AND PARTY_TYPE={2} AND TRAN_MODE={3}", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(partytype), SQLUtils.QuotedStr(trantype));
            string unionsql = sql + " AND PARTY_NO=" + SQLUtils.QuotedStr(partyno) + " UNION " + sql;
            DataTable pdmDdt = OperationUtils.GetDataTable(unionsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string stscd = string.Empty;

            if (pdmDdt.Rows.Count <= 0)
            {
                return list;
            }
            DataRow dr = pdmDdt.Rows[0];
            string docId = Prolink.Math.GetValueAsString(dr["U_ID"]);
            sql = string.Format("SELECT DOC_TYPE FROM TKPDD WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(docId), SQLUtils.QuotedStr(cmp));
            string type = "";
            DataTable docTypeDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow docType in docTypeDt.Rows)
            {
                string doc_type = Prolink.Math.GetValueAsString(docType["DOC_TYPE"]);
                if (string.IsNullOrEmpty(doc_type))
                    continue;
                doc_type = doc_type.Trim();
                if (!string.IsNullOrEmpty(type))
                    type += ";";
                type += doc_type;
            }
            string[] types = type.Split(';');
            int typescount = types.Length;
            List<Dictionary<string, string>> temp = MailTemplate.FileQueryDownlodInfo(uid, groupId, cmp, "*", "", type);
            list.AddRange(temp);
            string[] dns=combineinfo.Split(',');
            sql = string.Format("SELECT U_ID,GROUP_ID,CMP,STN,DEP FROM SMDN WHERE DN_NO IN {0}", SQLUtils.Quoted(dns));
            DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (iscombine)
            {
                foreach (DataRow dnrow in dndt.Rows)
                {
                    string dnuid = dnrow["U_ID"].ToString();
                    string dngroupid = dnrow["GROUP_ID"].ToString();
                    string dncmp = dnrow["CMP"].ToString();
                    temp = MailTemplate.FileQueryDownlodInfo(uid, groupId, cmp, "*", "", type);
                    list.AddRange(temp);
                }
            }
            return list;
        }


        public static string SendDnRelativeMail(MailData data, MixedList ml)
        {
            try
            {
                //DN#MailType#0b224906-40ee-4da4-883d-8ccf96f9d1fe#groupid#cmp
                string mailType = data.Keys[1];
                string uid = data.Keys[2];
                string groupid = data.Keys[3];
                string cmp = data.Keys[4];
                IMailTemplateParse parse = new DefaultMailParse();
                string sql = "SELECT * FROM SMDN WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Dictionary<string, string> map = new Dictionary<string, string>();

                string dnno = Prolink.Math.GetValueAsString(maindt.Rows[0]["DN_NO"]);
                sql = string.Format("SELECT * FROM SMDNPT WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
                DataTable partydt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string Column="PARTYROWS_TD";           //对应maitemp中的patyrows
                StringBuilder columpars = new StringBuilder();

                columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
			                <td class='title'>TYPE</td>
			                <td class='title'>NO</td>
			                <td class='title'>NAME</td>
			                <td class='title'>ADDRESS</td>
		                </tr>");

                for (int i = 0; i < partydt.Rows.Count;i++ )
                {
                    DataRow dr = partydt.Rows[i];
                    //<tr>
	                //    <td class="title">TYPE</td>
                    //    <td class="title">NO</td>
	                //    <td class="title">NAME</td>
	                 //   <td class="title">ADDRESS</td>
                    //</tr>
                    columpars.Append("<tr><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["TYPE_DESCP"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["PARTY_NO"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["PART_ADDR"]));
                    columpars.Append("-");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
                    columpars.Append("</td></tr>");
                }
                columpars.Append("</tbody></table>");
                map.Add(Column, columpars.ToString());
                string body = MailTemplate.GetMailBody(mailType, groupid, cmp);
                DataTable baseDt = MailTemplate.GetBaseData("'TDTK','TCGT'", groupid,cmp);
                maindt.Columns["TRACK_WAY"].MaxLength = 50;
                maindt.Columns["CARGO_TYPE"].MaxLength = 50;
                maindt.Columns["CNT_TYPE"].MaxLength = 50;
                foreach (DataRow dr in maindt.Rows)
                {
                    dr["TRACK_WAY"] = MailTemplate.GetBaseCodeValue(baseDt, "TDTK", Prolink.Math.GetValueAsString(dr["TRACK_WAY"]));
                    dr["CARGO_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TRGN", Prolink.Math.GetValueAsString(dr["CARGO_TYPE"]));
                    Func<string, string, string> setCnt = (fieldname, filedescp) =>
                    {
                        int value = Prolink.Math.GetValueAsInt(dr[fieldname]);
                        if (value > 0)
                            return filedescp + value + ";";
                        return string.Empty;
                    };
                    dr["CNT_TYPE"] = setCnt("CNT_NUMBER", Prolink.Math.GetValueAsString(dr["CNT_TYPE"])) + setCnt("CNT20", "20' X ") + setCnt("CNT40", "40' X ") + setCnt("CNT40HQ", "40HQ X ");
                }
                data.Body = parse.Parse(maindt, null, body, map);

                if (data.ProcessTimes > 0)
                {
                    data.Subject = "Revised " + data.Subject;
                }
                data.MailClient.Send(data.Subject, data.To, "", data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送询价通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendQuotNotifyMail(MailData data, MixedList ml)
        {
            try
            {
                //data.EvenNo = "";
                string u_id = data.Keys[1];
                string u_id1 = data.Keys[2];
                string notify_group = data.Keys[3];
                string lsp_cd = data.Keys[4];
                string groupId = data.Keys[5];

                string sql = string.Format("SELECT * FROM SMRQM WHERE U_ID={0} ", SQLUtils.QuotedStr(u_id));
                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                sql = string.Format("SELECT * FROM SMRQD WHERE U_ID={0} ", SQLUtils.QuotedStr(u_id1));
                DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
                string period = Prolink.Math.GetValueAsString(MainDt.Rows[0]["PERIOD"]);
                string mailType = "RQ"; ;
                
                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
              
                DataRow rfq = MainDt.Rows[0];
                Func<string, string, string> setCnt = (fieldname, filedescp) =>
                {
                    int value = Prolink.Math.GetValueAsInt(rfq[fieldname]);
                    if (value>0)
                        return filedescp + value + ";";
                    return string.Empty;
                };
                map["CNT_ALL"] = setCnt("CNT20", "20' X ") + setCnt("CNT40", "40' X ") + setCnt("CNT40HQ", "40HQ X ");
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                MailTemplate.CreateQuotData(mailDt, group_id, cmp);
                #endregion

                #region 获取加档内容
                if ("B".Equals(period))//判断是否要加档
                {
                    mailType = "BID"; ;
                    try
                    {
                        List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(u_id, group_id, cmp, "*", "", "RFQ");
                        string urls = string.Empty;
                        if (list.Count > 0)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                string url = list[i]["FileUrl"];
                                if (!string.IsNullOrEmpty(url))
                                {
                                    urls += string.Format("<a href=\"{0}\">{1}</a> ", url, list[i]["FileName"]);
                                }
                            }
                            map["DOC_TEXT"] = "要求报价的参考excel:";
                            map["DOC_HTML"] = urls;
                        }
                    }
                    catch { }
                }
                #endregion

                string htmltemp = MailTemplate.GetMailBody(mailType, groupId, cmp);//MT_NAME,MT_CONTENT
                DataTable mailGroupDt = MailTemplate.GetMailGroup(notify_group, groupId,"Q");
                EditInstruct ei = null;
                for (int i = 0; i < subDt.Rows.Count; i++)
                {
                    DataRow dr = subDt.Rows[i];
                    ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", dr["U_ID"]);
                    ei.Put("STATUS", "Y");
                    DateTime odt = DateTime.Now;                    
                    string CompanyId = cmp;
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("NOTIFY_DATE", odt);
                    ei.PutDate("NOTIFY_DATE_L", ndt);
                    ml.Add(ei);

                    foreach (DataRow mrow in mailDt.Rows)
                    {
                        mrow["LSP_CD"] = dr["LSP_CD"];
                        mrow["LSP_NM"] = dr["LSP_NM"];
                    }
                      
                    foreach (DataRow mailGroup in mailGroupDt.Rows)
                    {
                        string to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                        data.To = to;
                        if (string.IsNullOrEmpty(to))
                            continue;
                        data.MailClient.Send(data.Subject, data.To, parse.Parse(mailDt, null, htmltemp, map));

                        EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
                    }
                   
                }
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.Message, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送叫車通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendRvNotifyMail(MailData data, MixedList ml)
        {
            try
            {
                //data.EvenNo = "";
                string UId = data.Keys[1];
                //string groupId = data.Keys[2];
                //string companyId = data.Keys[3];
                string u_id = "";
                string txt = @"SELECT TOP 1 SMSM.SHIPMENT_ID
                                        ,SMSM.U_ID
                                        ,SMRV.DN_NO 
                                        ,SMRV.RESERVE_NO
                                        ,SMRV.BAT_NO
	                                    ,SMRV.TRUCKER
	                                    ,SMRV.TRUCKER_NM
	                                    ,SMRV.USE_DATE
                                        ,SMSM.U_ID
	                                    ,SMSM.SHIPMENT_INFO
                                        ,SMSM.COMBINE_INFO
	                                    ,(SELECT TOP 1 CD_DESCP FROM BSCODE WHERE CD=SMSM.TRAN_TYPE AND CD_TYPE='TNT') AS TRAN_DESCP
	                                    ,SMSM.PPOR_CD AS PPOR
	                                    ,SMSM.PPOL_CD AS PPOL
	                                    ,SMSM.POL_CD AS POL
	                                    ,SMSM.DEST_NAME
	                                    ,SMSM.CMP AS LOCATION
                                        ,SMSM.CMP
	                                    ,SMSM.GROUP_ID
	                                    ,SMRV.WS_CD
	                                    ,SMSM.GOODS AS COMMODITY
	                                    ,SMSM.REGION
	                                    ,SMSM.PPOD_CD AS PPOD
	                                    ,SMSM.GW
	                                    ,SMSM.QTY
	                                    ,SMSM.CBM
                                        ,SMSM.PCBM
	                                    ,SMSM.CARRIER_NM
	                                    ,SMSM.OEXPORTER_NM
	                                    ,SMSM.PCNT20
	                                    ,SMSM.PCNT40
	                                    ,SMSM.PCNT40HQ
                                        ,SMSM.CNT20
	                                    ,SMSM.CNT40
	                                    ,SMSM.CNT40HQ
	                                    ,SMSM.CNT_TYPE
	                                    ,SMSM.INSTRUCTION
                                        ,SMSM.PKG_UNIT_DESC
                                        ,SMSM.GW
                                        ,SMSM.TRAN_TYPE
                                        ,SMSM.LSP_NO
                                        ,SMSM.CUSTOMS_DATE
                                        ,SMSM.CUT_PORT_DATE
                                        ,(SELECT TOP 1 ' 长(cm):' + convert(varchar(10),L) + ' 宽(cm): ' + convert(varchar(10),W) + ' 高(cm):' + convert(varchar(10),H) + ' 体积(m3):' + convert(varchar(10),VW) FROM SMCUFT WHERE SMCUFT.DN_NO=SMSM.DN_NO ) AS LWH
                                        ,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSM.SHIPMENT_ID=SMSMPT.SHIPMENT_ID AND PARTY_TYPE='BR') AS BROKER_NM
                                        ,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSM.SHIPMENT_ID=SMSMPT.SHIPMENT_ID AND PARTY_TYPE='SP') AS SP_NM
                                        ,(SELECT TOP 1 PARTY_NAME FROM SMSMPT WHERE SMSM.SHIPMENT_ID=SMSMPT.SHIPMENT_ID AND PARTY_TYPE='FC') AS FC_NM
                                    FROM SMRV
                                    INNER JOIN SMSM ON SMRV.SHIPMENT_ID = SMSM.SHIPMENT_ID
                                    WHERE SMRV.U_ID = {0}";

                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));
                string PartyNo = "";
                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                foreach (DataRow dr in MainDt.Rows)
                {
                    PartyNo = Prolink.Math.GetValueAsString(dr["TRUCKER"]);
                    u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                }

                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
                string mailType = "RVTK"; ;

                //MailTemplate.ChageDateToString(MainDt, new string[] { "USE_DATE", "CUSTOMS_DATE","CUT_PORT_DATE"});

                Dictionary<string, string> map = new Dictionary<string, string>();

                string trantype = Prolink.Math.GetValueAsString(MainDt.Rows[0]["TRAN_TYPE"]);
                string lspno = Prolink.Math.GetValueAsString(MainDt.Rows[0]["LSP_NO"]);
                map["LSP_MAIL"] = GetMailByPartyNo(lspno, group_id, trantype);

                IMailTemplateParse parse = new DefaultMailParse();
                #region 获取加档内容
                try
                {
                    List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(u_id, group_id, cmp, "*", "", "WMSLOC");
                    string urls = string.Empty;
                    if (list.Count > 0)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            string url = list[i]["FileUrl"];
                            if (!string.IsNullOrEmpty(url))
                            {
                                urls += string.Format("<a href=\"{0}\">{1}</a> ", url, list[i]["FileName"]);
                            }
                        }
                        map["DOC_TEXT"] = "进仓图:";
                        map["DOC_HTML"] = urls;
                    }
                }
                catch { }
                #endregion
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                 EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送進口報價通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendIQQTvoid(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];
                string groupId = data.Keys[2];
                string companyId = data.Keys[3];
                string mailType = "IQQT";

                //各Inquery要套的郵件格式：報關費：IQNB，托卡車費：IQNC，Local報價：IQNX
                string txt = @"SELECT TOP 1 * FROM SMQTM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));
                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string tranmode = MainDt.Rows[0]["TRAN_MODE"].ToString();
                switch (tranmode)
                {
                    case "C":   //truck
                        mailType = "IQNC";
                        break;
                    case "X":   //local
                        mailType = "IQNX";
                        break;
                    case "B":   //broker
                        mailType = "IQNB";
                        break;
                }

                //MailTemplate.ChageDateToString(MainDt, new string[] { "USE_DATE", "CUSTOMS_DATE","CUT_PORT_DATE"});

                Dictionary<string, string> map = new Dictionary<string, string>();
                string htmltemp = MailTemplate.GetMailBody(mailType, groupId, companyId);

                IMailTemplateParse parse = new DefaultMailParse();
                string body = MailTemplate.GetMailBody(mailType, groupId, companyId);
                data.Body = parse.Parse(MainDt, null, body, map);
                data.Subject = "Inbound Quotation Enquiry";
                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        private static Attachment[] GetAttach(string uid,string partyno)
        {
            Attachment[] attachment=new Attachment[]{};
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            //根据partyno带出需要上传的档案类型
            string sql = "SELECT DOC_TYPE FROM TKPDM,TKPDD WHERE TKPDM.U_ID=TKPDD.U_ID AND TKPDM.CMP=" + SQLUtils.QuotedStr(partyno);
            DataTable doctypedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string doctypes=";";
            for(int i=0;i<doctypedt.Rows.Count;i++)
            {
                doctypes+=Prolink.Math.GetValueAsString(doctypedt.Rows[i]["DOC_TYPE"])+";";
            }
            string jobNo = "123";//dnno
            string GroupId = "";
            string CompanyId = "";
            string Station = "";
            string dep = "";// Request.Params["DEP"];
            _api.Login();
            int serverNum = 0;
            string guid = _api.GetFolderGUID(jobNo, GroupId, CompanyId, Station, dep, ref serverNum, "FOLDER_GUID,SERVER_NUM");
            List<EDOCFileItem> edocList = _api.Inquery(guid,"", serverNum);
            int j=0;
            for(int i=0;i<edocList.Count;i++)
            {
                EDOCFileItem Efi=edocList[i];
                string edoctype=Prolink.Math.GetValueAsString(Efi.EdocType);
                string fileid = Prolink.Math.GetValueAsString(Efi.FileID);
                string filepath = "";//Server.MapPath(_folder)+"\\"+Prolink.Math.GetValueAsString(Efi.Name+"."+Efi.Ext);
                if (doctypes.Contains(";" + edoctype + ";"))
                {
                    _api.DownloadFile(filepath, fileid, "");
                    attachment[j]=new Attachment(filepath);
                    j++;
                }
            }
            return attachment;
        }

        /// <summary>
        /// 发送叫車通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendUserInfoMail(MailData data, MixedList ml)
        {
            try
            {
                //data.EvenNo = "";
                string UId = data.Keys[1];
                string PartyName = data.Keys[2];
                string UName = data.Keys[3];
                string Upassword = data.Keys[4];
                string group_id = data.Keys[5];
                string cmp = data.Keys[6];

                string sql = "SELECT CMP FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                string u_cmp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                //DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable MainDt = new DataTable();
                DataRow workRow = MainDt.NewRow();
                MainDt.Columns.Add("U_ID", typeof(String));
                MainDt.Columns.Add("CMP", typeof(String));
                MainDt.Columns.Add("U_NAME", typeof(String));
                MainDt.Columns.Add("PARTY_NAME", typeof(String));
                MainDt.Columns.Add("U_PASSWORD", typeof(String));

                workRow["U_ID"] = UId;
                workRow["U_NAME"] = UName;
                workRow["PARTY_NAME"] = PartyName;
                workRow["U_PASSWORD"] = Upassword;
                workRow["CMP"] = u_cmp;
                MainDt.Rows.Add(workRow);

                string mailType = "USERINFO"; ;

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

         /// <summary>
        /// 发送edoc通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendEdocList(MailData data, MixedList ml)
        {
            try
            {
                //data.EvenNo = "";
                string UId = data.Keys[1];
                string group_id = data.Keys[2];
                string cmp = data.Keys[3];
                string mailContent = data.Body;


                //DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                DataTable MainDt = new DataTable();
                DataRow workRow = MainDt.NewRow();
                MainDt.Columns.Add("U_ID", typeof(String));
                MainDt.Columns.Add("MAIL_CONTENT", typeof(String));

                workRow["U_ID"] = UId;
                workRow["MAIL_CONTENT"] = mailContent;
                MainDt.Rows.Add(workRow);

                string mailType = "EDOCINFO"; ;

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送edoc通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendInboundBillResult(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];
                string mailType = "IB_BILL";
                if (data.Keys.Length >= 6)
                {
                    mailType = data.Keys[5];
                }
                string txt = @"SELECT TOP 1 * FROM SMBIM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));

                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                sql = "SELECT * FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                DataTable SubDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string Column = "SMBID";           //对应maitemp中的patyrows
                StringBuilder columpars = new StringBuilder();

                columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
                            <td class='title'>Bill NO</td>
			                <td class='title'>Charge Code</td>
			                <td class='title'>Charge Descp</td>
			                <td class='title'>Currency</td>
                            <td class='title'>Chargeable Unit</td>
                            <td class='title'>QTY</td>
                            <td class='title'>Amount</td>
                            <td class='title'>Rate</td>
                            <td class='title'>Remark</td>
                            <td class='title'>Rejected reason：</td>
		                </tr>");

                for (int i = 0; i < SubDt.Rows.Count; i++)
                {
                    DataRow dr = SubDt.Rows[i];
                    columpars.Append("<tr><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["BL_NO"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_CD"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CUR"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_UNIT"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["QTY"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["BAMT"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["TAX"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["REMARK"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHECK_DESCP"]));
                    columpars.Append("</td></tr>");
                }
                columpars.Append("</tbody></table>");
                map.Add(Column, columpars.ToString());
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                //问题单：111130
                MailTemplate.ChageNumToFormat(mailDt, new string[] { "QTY" });
                MailTemplate.ChageDateToString(mailDt, new string[] { "DEBIT_DATE" });

                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }
        /// <summary>
        /// 发送报价mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendQuotResult(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];
                string mailType = "", name = "", reson = "";
                
                if (data.Keys.Length >= 8)
                {
                    mailType = data.Keys[5];
                    name = data.Keys[6];
                    reson = data.Keys[7];
                }
                else if (data.Keys.Length >= 7)
                {
                    mailType = data.Keys[5];
                    name = data.Keys[6];
                }
                else if (data.Keys.Length >= 6)
                {
                    mailType = data.Keys[5];
                }
                string txt = @"SELECT TOP 1 * FROM SMQTM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));

                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["Rlocation"]);
                cmp = string.IsNullOrEmpty(cmp) ? "FQ" : cmp;
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
                string tranmode = Prolink.Math.GetValueAsString(MainDt.Rows[0]["TRAN_MODE"]);

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                sql = "SELECT * FROM SMQTD WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                DataTable SubDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string Column = "SMQTD";//对应maitemp中的patyrows
                string approveuser = "APPROVE_USER";
                StringBuilder columpars = new StringBuilder();

                switch (tranmode)
                {
                    case "B": columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
                            <td class='title'>Charge CD</td>
                            <td class='title'>Description</td>
                            <td class='title'>Charge Type</td>
                            <td class='title'>Cost Classify</td>
                            <td class='title'>Contracted Warehouse</td>
                            <td class='title'>Chargable Unit</td>
                            <td class='title'>Cost</td>
                            <td class='title'>Currency</td>
                            <td class='title'>Remark</td>
		                </tr>");
                        for (int i = 0; i < SubDt.Rows.Count; i++)
                        {
                            DataRow dr = SubDt.Rows[i];
                            columpars.Append("<tr><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_TYPE"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REPAY"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CARRIER"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["PUNIT"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CUR"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F3"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REMARK"]));
                            columpars.Append("</td></tr>");
                        }
                        columpars.Append("</tbody></table>");
                        break;
                    case "C": columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
			                <td class='title'>POL CD</td>
			                <td class='title'>POL Name</td>
			                <td class='title'>DEST CD</td>
			                <td class='title'>DEST Name</td>
                            <td class='title'>Charge CD</td>
                            <td class='title'>Description</td>
                            <td class='title'>Chargable Unit</td>
                            <td class='title'>Cost</td>
                            <td class='title'>20'</td>
                            <td class='title'>40'</td>
                            <td class='title'>Charge Type</td>
                            <td class='title'>Cost Classify</td>
                            <td class='title'>Went Dutch?</td>
                            <td class='title'>T/T</td>
                            <td class='title'>Remark</td>
		                </tr>");
                        for (int i = 0; i < SubDt.Rows.Count; i++)
                        {
                            DataRow dr = SubDt.Rows[i];
                            columpars.Append("<tr><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["POL_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["POL_NM"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["POD_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["POD_NM"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["PUNIT"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F3"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F1"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F2"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_TYPE"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REPAY"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["IS_SHARE"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["TT"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REMARK"]));
                            columpars.Append("</td></tr>");
                        }
                        columpars.Append("</tbody></table>"); break;
                    case "X": columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
			                <td class='title'>POD</td>
                            <td class='title'>Charge CD</td>
                            <td class='title'>Description</td>
                            <td class='title'>Charge Type</td>
                            <td class='title'>Cost Classify</td>
                            <td class='title'>Chargable Unit</td>
                            <td class='title'>Currency</td>
                            <td class='title'>Cost</td>
                            <td class='title'>20GP</td>
                            <td class='title'>40GP</td>
                            <td class='title'>40HQ</td>
                            <td class='title'>Carrier</td>
                            <td class='title'>Went Dutch?</td>
                            <td class='title'>Remark</td>
		                </tr>");
                        for (int i = 0; i < SubDt.Rows.Count; i++)
                        {
                            DataRow dr = SubDt.Rows[i];
                            columpars.Append("<tr><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["POD_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_CD"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_TYPE"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REPAY"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["PUNIT"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CUR"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F3"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F4"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F5"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["F6"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["CARRIER"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["IS_SHARE"]));
                            columpars.Append("</td><td>");
                            columpars.Append(Prolink.Math.GetValueAsString(dr["REMARK"]));
                            columpars.Append("</td></tr>");
                        }
                        columpars.Append("</tbody></table>"); break;
                }
                map.Add(Column, columpars.ToString());
                map.Add(approveuser, name);
                string BACK_REMARK = "BACK_REMARK";

                map.Add(BACK_REMARK, reson);
                MailTemplate.ChageDateToString(mailDt, new string[] { "EFFECT_TO" });
                MailTemplate.ChageDateToString(mailDt, new string[] { "QUOT_DATE" });
                MailTemplate.ChageDateToString(mailDt, new string[] { "EFFECT_FROM" });

                Dictionary<string, object> OUTIN = new Dictionary<string, object>();
                OUTIN.Add("O", "OutBound");
                OUTIN.Add("I", "InBound");

                Dictionary<string, object> TRANTYPE = new Dictionary<string, object>();
                TRANTYPE.Add("F", "FCL");
                TRANTYPE.Add("L", "LCL");
                TRANTYPE.Add("A", "AIR");
                TRANTYPE.Add("D", "INLAND EXPRESS");
                TRANTYPE.Add("E", "EXPRESS");
                TRANTYPE.Add("R", "Railroad");
                TRANTYPE.Add("T", "TRUCK");

                Dictionary<string, object> FrtTerm = new Dictionary<string, object>();
                FrtTerm.Add("P", "Prepaid");
                FrtTerm.Add("C", "Collect");
                FrtTerm.Add("O", "Other");
                ResetMark(mailDt, new string[] { "OUT_IN" }, OUTIN);
                ResetMark(mailDt, new string[] { "TRAN_TYPE" }, TRANTYPE);
                ResetMark(mailDt, new string[] { "FREIGHT_TERM" }, FrtTerm);
                //remark(mailDt, new string[] { "BACK_REMARK" });
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                //问题单：111130
                //MailTemplate.ChageNumToFormat(mailDt, new string[] { "QTY" });
                //MailTemplate.ChageDateToString(mailDt, new string[] { "DEBIT_DATE" });

                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion

                List<string> testMailList = new List<string>() { "820839045@qq.com", "664437899@qq.com", "827737035@qq.com" };
                string prdortest = OperationUtils.GetValueAsString("select AP_CD from bscode where cd_type='SYS' and CD='PRD_OR_TEST' AND CMP='*' ", Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(prdortest) || "TEST".Equals(prdortest))
                {
                    if (!testMailList.Contains(data.To.ToLower()))
                        data.To = "820839045@qq.com";
                }
                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }
        public static void remark(DataTable MainDt, string[] dateFields)
        {
            string name = string.Empty;
            for (int i = 0; i < dateFields.Length; i++)
            {
                MainDt.Columns.Add(dateFields[i] + "_TEMP", typeof(string));
                //MainDt.Columns[dateFields[i] + "_TEMP"].MaxLength = 200;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    if (dr[name] == null || dr[name] == DBNull.Value)
                        break;
                    string val=Prolink.Math.GetValueAsString( dr[name]);
                    string [] vals =val.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    if (vals.Length >= 2)
                    {
                        dr[name + "_TEMP"] = vals.Length > 0 ? vals[vals.Length - 2] : string.Empty;
                    }
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name);
                MainDt.Columns.Add(name, typeof(string));
                //MainDt.Columns[name].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    dr[name] = dr[name + "_TEMP"];
                }
            }
        }
        public static void ResetMark(DataTable MainDt, string[] dateFields,Dictionary<string,object> list)
        {
            string name = string.Empty;
            for (int i = 0; i < dateFields.Length; i++)
            {
                MainDt.Columns.Add(dateFields[i] + "_TEMP", typeof(string));
                MainDt.Columns[dateFields[i] + "_TEMP"].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    if (dr[name] == null || dr[name] == DBNull.Value)
                        break;
                    string val = Prolink.Math.GetValueAsString(dr[name]);

                    foreach (var n in list)
                    {
                        if (n.Key.Equals(val))
                        {
                            dr[name + "_TEMP"] = n.Value;
                        }
                    }
                }
            }
            for (int i = 0; i < dateFields.Length; i++)
            {
                name = dateFields[i];
                MainDt.Columns.Remove(name);
                MainDt.Columns.Add(name, typeof(string));
                MainDt.Columns[name].MaxLength = 50;
            }
            foreach (DataRow dr in MainDt.Rows)
            {
                for (int i = 0; i < dateFields.Length; i++)
                {
                    name = dateFields[i];
                    dr[name] = dr[name + "_TEMP"];
                }
            }
        }
        /// <summary>
        /// 发送帳單簽核通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendInboundBillApNotiyMail(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];


                string txt = @"SELECT TOP 1 * FROM SMBIM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));

                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
                string mailType = "IB_BILL_AP";

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                sql = "SELECT * FROM SMBID WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                DataTable SubDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string Column = "SMBID";           //对应maitemp中的patyrows
                StringBuilder columpars = new StringBuilder();

                columpars.Append(@"<table class='GeneratedTable'>
	                <tbody>
		                <tr>
			                <td class='title'>Bill NO</td>
			                <td class='title'>Charge Code</td>
			                <td class='title'>Charge Descp</td>
			                <td class='title'>Currency</td>
                            <td class='title'>Chargeable Unit</td>
                            <td class='title'>QTY</td>
                            <td class='title'>Amount</td>
                            <td class='title'>Rate</td>
                            <td class='title'>Remark</td>
                            <td class='title'>Rejected reason：</td>
		                </tr>");

                for (int i = 0; i < SubDt.Rows.Count; i++)
                {
                    DataRow dr = SubDt.Rows[i];
                    columpars.Append("<tr><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["BL_NO"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_CD"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_DESCP"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CUR"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHG_UNIT"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["QTY"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["BAMT"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["TAX"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["REMARK"]));
                    columpars.Append("</td><td>");
                    columpars.Append(Prolink.Math.GetValueAsString(dr["CHECK_DESCP"]));
                    columpars.Append("</td></tr>");
                }
                columpars.Append("</tbody></table>");
                map.Add(Column, columpars.ToString());
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                MailTemplate.ChageDateToString(mailDt, new string[] { "DEBIT_DATE" });
                MailTemplate.ChageNumToFormat(mailDt, new string[] { "QTY" });
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                list.Add(new Dictionary<string, object>(){ 
                {"F","FCL"},{"L","LCL"}, {"A","AIR"}, {"T","Domestic"}, {"B","Broker"}, 
                {"E","Express"}, {"R","RailWay"}, {"C","Trailer"}, {"P","Truck"}});
                MailTemplate.ChageExpression(mailDt, new string[] { "TRAN_TYPE" }, list);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送叫車通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendInboundCancelRvNotifyMail(MailData data, MixedList ml)
        {
            try
            {
                //data.EvenNo = "";
                string reserveNo = data.Keys[1];
                string shipmentId = data.Keys[2];
                string dnNo = data.Keys[3];
                string truckerNm = data.Keys[4];
                string groupId = data.Keys[5];
                string companyId = data.Keys[6];
                
                string mailType = "IRVC"; ;

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();

                DataTable MainDt = new DataTable();
                DataRow workRow = MainDt.NewRow();
                MainDt.Columns.Add("RESERVE_NO", typeof(String));
                MainDt.Columns.Add("SHIPMENT_ID", typeof(String));
                MainDt.Columns.Add("DN_NO", typeof(String));
                MainDt.Columns.Add("TRUCKER_NM", typeof(String));

                workRow["RESERVE_NO"] = reserveNo;
                workRow["SHIPMENT_ID"] = shipmentId;
                workRow["DN_NO"] = dnNo;
                workRow["TRUCKER_NM"] = truckerNm;
                MainDt.Rows.Add(workRow);

                string htmltemp = MailTemplate.GetMailBody(mailType, groupId, companyId);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                string body = MailTemplate.GetMailBody(mailType, groupId, companyId);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion


                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送合约簽核通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendContractApNotiyMail(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];

                string txt = @"SELECT TOP 1 * FROM SMCTM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));

                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
                string mailType = "CONT_AP";

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                #region mail 数据源格式
                DataTable mailDt = MainDt.Copy();
                //MailTemplate.CreateQuotData(mailDt, group_id);
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                MailTemplate.ChageDateToString(mailDt, new string[] { "CT_DATE", "EFFECT_TO" });
                //MailTemplate.ChageNumToFormat(mailDt, new string[] { "QTY" });
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                list.Add(new Dictionary<string, object>(){ 
                {"N","未发起"},{"Y","发起"}, {"A","审核通过"}, {"V","作废"}, {"E","到期"}});
                MailTemplate.ChageExpression(mailDt, new string[] { "STATUS" }, list);
                data.Body = parse.Parse(mailDt, null, body, map);
                #endregion
                string subject = data.Subject;
                subject = subject.Replace("\r", " ");
                subject = subject.Replace("\n", " ");
                data.MailClient.Send(subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送订舱有关的Mail  Mail代号IN
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendInboundBookingMail(MailData data, MixedList ml)
        {
            try
            {
                string partytype = string.Empty;
                string mailType = data.Keys[1];
                string uid = data.Keys[2];
                string partyno = data.Keys[3];
                string groupid = data.Keys[4];
                string cmp = data.Keys[5];
                partytype = data.Keys[6];
                string mailcc = string.Empty;
                if (data.Keys.Length >= 9)
                {
                    mailcc = data.Keys[8];
                }
                IMailTemplateParse parse = new DefaultMailParse();

                string sql = @"SELECT SMSMI.*,(SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='FC')CUSTOMER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='SH')SHIPPER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='IBSP')FORWARDER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='FS')CARRIER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='IBBR')BROKER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='IBCR')TRUCKER,
                (SELECT TOP 1 PARTY_NAME FROM SMSMIPT WHERE SMSMIPT.SHIPMENT_ID=SMSMI.SHIPMENT_ID AND SMSMIPT.PARTY_TYPE='IBTC')TRANSIT
                 FROM SMSMI WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string shipmentid = Prolink.Math.GetValueAsString(maindt.Rows[0]["SHIPMENT_ID"]);
                Dictionary<string, string> map = new Dictionary<string, string>();
                GetSMDNPInfoToMap(ref map, uid);

                map["SMCUFT_TD"] = GetSMCUFTDt(maindt.Rows[0]);
                string trantype = Prolink.Math.GetValueAsString(maindt.Rows[0]["TRAN_TYPE"]);
                //string lspno = Prolink.Math.GetValueAsString(maindt.Rows[0]["LSP_NO"]);
                //map["LSP_MAIL"] = GetMailByPartyNo(lspno, groupid, trantype);
                /*
                string transmode = Prolink.Math.GetValueAsString(maindt.Rows[0]["TRANSACTE_MODE"]);
                maindt.Columns["TRANSACTE_MODE"].MaxLength = 50;
                switch (transmode)
                {
                    case "A":
                        transmode = "一般贸易";
                        break;
                    case "B":
                        transmode = "进料对口";
                        break;
                    case "D":
                        transmode = "进料复出";
                        break;
                    case "O":
                        transmode = "其它";
                        break;
                }
                maindt.Rows[0]["TRANSACTE_MODE"] = transmode;
                 * */
                List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
                if (mailType.Equals(MailManager.BrokerNotify))
                {
                    sql = string.Format("SELECT SMIRV.CNTR_NO FROM SMIRV WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentid));
                    DataTable smrvdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string cntrnos = string.Empty;
                    foreach (DataRow dr in smrvdt.Rows)
                    {
                        cntrnos += GetSemicolonVal(dr["CNTR_NO"]);
                    }
                    cntrnos = cntrnos.Trim(';');
                    map["CNTR_NO"] = cntrnos;
                    list = GetEdocSend(maindt, partytype, partyno, true);
                }
                else
                {
                    list = GetEdocSend(maindt, partytype, partyno);
                }
                List <string> dlvaddrs=new List<string>();
                sql = string.Format("SELECT DLV_ADDR FROM SMIDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
                if ("F".Equals(trantype) || "R".Equals(trantype))
                    sql = string.Format("SELECT DLV_ADDR FROM SMICNTR WHERE SHIPMENT_ID={0}",SQLUtils.QuotedStr(shipmentid));
                DataTable idt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (idt.Rows.Count > 0)
                {
                    foreach (DataRow dr in idt.Rows)
                    {
                        string DlvAddr = Prolink.Math.GetValueAsString(dr["DLV_ADDR"]);
                        if (!string.IsNullOrEmpty(DlvAddr) && !dlvaddrs.Contains(DlvAddr))
                            dlvaddrs.Add(DlvAddr);
                    }
                    if (dlvaddrs.Count > 0)
                        map["DLV_ADDR"] = string.Join(",", dlvaddrs);
                }

                try
                {
                    if (list.Count > 0)
                    {
                        SetDocUrl(ref map, ref list);
                    }
                    else
                    {
                        if (mailType.Equals("IAB") || mailType.Equals("IDB") || mailType.Equals("IEB") || mailType.Equals("IFB") ||
                            mailType.Equals("ILB") || mailType.Equals("IBR") || mailType.Equals("TBR"))
                        {
                            //如果没有档案就新增一个对于FCL/LCL  内贸的用中文bookingfrom 要实现自动归档
                            EdocHelper edochelper = new EdocHelper();
                            string filetype = "BF";
                            Result result = edochelper.CreateBFReport(shipmentid, filetype);
                            if (!result.Success)    //发送之前先归档一次，如果归档失败，再用排程去归档
                            {
                                //加入排程的Table中
                                DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT 1 FROM SM_AUTOEDOC WHERE JOB_NO={0} AND FILE_TYPE={1} AND IS_OK!='Y'",
                                SQLUtils.QuotedStr(maindt.Rows[0]["U_ID"].ToString()),
                                SQLUtils.QuotedStr(filetype)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (dt.Rows.Count == 0)
                                {
                                    Prolink.DataOperation.OperationUtils.Logger.WriteLog(result.Message);
                                    EditInstruct ei = new EditInstruct("SM_AUTOEDOC", EditInstruct.INSERT_OPERATION);
                                    ei.Put("U_ID", Guid.NewGuid().ToString());
                                    ei.Put("SHIPMENT_ID", shipmentid);
                                    ei.Put("FILE_TYPE", "BF");
                                    ei.Put("GROUP_ID", groupid);
                                    ei.Put("CMP", cmp);
                                    ei.Put("JOB_NO", uid);
                                    if ("T".Equals(trantype))
                                    {
                                        ei.Put("REPORT_NAME", "Booking form 中文");
                                        ei.Put("REPORT_ID", "FCL03");
                                    }
                                    else
                                    {
                                        ei.Put("REPORT_NAME", "Booking Form ");
                                        ei.Put("REPORT_ID", "FCL01");
                                    }
                                    ei.Put("EXPORT_FILE_TYPE", "pdf");
                                    ei.Put("ADD_RESON", result.Message);
                                    ei.Put("CREATE_BY", "Booking_Mail_Send");
                                    ei.PutDate("CREATE_DATE", DateTime.Now);
                                    ml.Add(ei);
                                }
                            }
                            else
                            {
                                list = GetEdocSend(maindt, partytype, partyno);
                                SetDocUrl(ref map, ref list);
                            }
                        }
                    }
                }
                catch { }
                string body = MailTemplate.GetMailBody(mailType, groupid, cmp);
                //MailTemplate.ChageDateToString(maindt, new string[] { "DN_ETD", "PRODUCT_DATE" });
                DataTable baseDt = MailTemplate.GetBaseData("'TRGN','TNT'", groupid, cmp);
                maindt.Columns["TRAN_TYPE"].MaxLength = 50;
                //maindt.Columns["REGION"].MaxLength = 50;
                maindt.Columns["STN"].MaxLength = 70;
                foreach (DataRow dr in maindt.Rows)
                {
                    string ptsql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["STN"])));
                    dr["STN"] = OperationUtils.GetValueAsString(ptsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    dr["TRAN_TYPE"] = MailTemplate.GetBaseCodeValue(baseDt, "TNT", Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]));
                    //dr["REGION"] = MailTemplate.GetBaseCodeValue(baseDt, "TRGN", Prolink.Math.GetValueAsString(dr["REGION"]));
                    Func<string, string, string> ReplacePort = (fieldname, replacename) =>
                    {
                        string value = Prolink.Math.GetValueAsString(dr[fieldname]);
                        if (string.IsNullOrEmpty(value))
                            dr[fieldname] = dr[replacename];
                        return string.Empty;
                    };
                    ReplacePort("POL_CD", "PPOL_CD");
                    ReplacePort("POL_NAME", "PPOL_NAME");
                    ReplacePort("POD_CD", "PPOD_CD");
                    ReplacePort("POD_NAME", "PPOD_NAME");
                    ReplacePort("DEST_CD", "PDEST_CD");
                    ReplacePort("DEST_NAME", "PDEST_NAME");
                }
                data.Body = parse.Parse(maindt, null, body, map);
                if (data.ProcessTimes > 0)
                {
                    data.Subject = "Revised " + data.Subject;
                }
                data.MailClient.Send(data.Subject, data.To, mailcc, data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "=>" + e.InnerException.Message;
                EvenFactory.SaveMailLog(data, msg, EvenFactory.Fail, ml);
                return msg;
            }
            return string.Empty;
        }
        /// <summary>
        /// Lst 退回通知
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendSMvoid(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.EvenNo;
                string mailType = data.Keys[0];
                string txt = @"SELECT TOP 1 * FROM SMSM WHERE U_ID = {0}";
                string sql = string.Format(txt, SQLUtils.QuotedStr(UId));

                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (MainDt != null && MainDt.Rows.Count > 0)
                {
                    string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                    string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);

                    string combineinfo = Prolink.Math.GetValueAsString(MainDt.Rows[0]["COMBINE_INFO"]);
                    string[] dnnos = combineinfo.Split(',');

                    //string TMsql = string.Format("SELECT top 1 EXP_TEXT FROM TMEXP WHERE JOB_NO={0}", SQLUtils.Quoted(dnnos));
                    //string reason = OperationUtils.GetValueAsString(TMsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    IMailTemplateParse parse = new DefaultMailParse();
                    string htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);//MT_NAME,MT_CONTENT
                    #region mail 数据源格式
                    DataTable mailDt = MainDt.Copy();
                    string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                    //问题单：111130
                    //MailTemplate.ChageNumToFormat(mailDt, new string[] { "QTY" });
                    //MailTemplate.ChageDateToString(mailDt, new string[] { "DEBIT_DATE" });
                    string reason = data.Body;
                    string sales = Prolink.Math.GetValueAsString(MainDt.Rows[0]["SALES_WIN"]);
                    string id = string.Empty;
                    Dictionary<string, object> TRANTYPE = new Dictionary<string, object>();
                    TRANTYPE.Add("F", "FCL");
                    TRANTYPE.Add("L", "LCL");
                    TRANTYPE.Add("A", "AIR");
                    TRANTYPE.Add("D", "INLAND EXPRESS");
                    TRANTYPE.Add("E", "EXPRESS");
                    TRANTYPE.Add("R", "Railroad");
                    TRANTYPE.Add("T", "TRUCK");
                    ResetMark(mailDt, new string[] { "TRAN_TYPE" }, TRANTYPE);
                    if (!string.IsNullOrEmpty(sales))
                    {
                        id = sales.Replace(" ", ";").Split(';')[0];
                    }
                    body = body.Replace("{REASON}", reason);
                    body = body.Replace("{SALES_WIN}", id);
                    data.Body = parse.Parse(mailDt, null, body, map);
                    #endregion


                    data.MailClient.Send(data.Subject, data.To, data.Body);
                    EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

                }
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        public static string SendInboundBookingMail1(MailData data, MixedList ml)
        {
            try
            {
                string tranType = data.Keys[1];
                string partyno = data.Keys[2];
                string groupid = data.Keys[3];
                string cmp = data.Keys[4];
                string partyType = data.Keys.Length > 6 ? data.Keys[6] : string.Empty;
                string mailType = string.Empty;
                string party_name = data.To;
               
                if(string.IsNullOrEmpty(data.Body))
                    return string.Empty;
                string[] ids = data.Body.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if(ids.Length<=0)
                    return string.Empty;
                
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = SQLUtils.QuotedStr(ids[i]);
                }
                string filter = string.Empty;
                string keyName = "SHIPMENT_ID";
                mailType = string.Format("I{0}B1", tranType);
                switch (tranType)
                {
                    default:
                        filter = string.Format("{0}.{1} IN ({2})", "SMSMI", "U_ID", string.Join(",", ids));
                        break;
                }

                string groupType = "I" + tranType;
                if (!string.IsNullOrEmpty(partyType) && partyType.Length<7)//broker NOTIFY
                {
                    switch (partyType)
                    {
                        case "IBBR":
                            groupType = "IB";
                            break;
                        default:
                            groupType = "ITC";
                            break;
                    }
                }

                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, groupid, groupType);
                //if (mailGroupDt.Rows.Count <= 0) 
                //    return string.Empty;
                string mailindex = string.Empty;
                List<string> temps = new List<string>();
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailindex) && !temps.Contains(mailindex))
                    {
                        temps.Add(mailindex);
                    }
                }
                data.To = string.Join(";", temps);
                if (string.IsNullOrEmpty(data.To))
                {
                    EvenFactory.SaveMailLog(data, string.Format("{0}无{1}类的mail建档", partyno, groupType), EvenFactory.Fail, ml);
                    return string.Format("{0}无{1}类的mail建档", partyno, groupType);
                }
                Dictionary<string, string> map = new Dictionary<string, string>();
                map["FORWARDER"] = party_name;
                string body = GetBookingMailBody(mailType, groupid, cmp, filter, keyName, map); 
                data.Body = body;
                data.Subject = GetSubJectByTranType(tranType, "INS"); 
                data.MailClient.Send(data.Subject, data.To, "", data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "=>" + e.InnerException.Message;
                EvenFactory.SaveMailLog(data, msg, EvenFactory.Fail, ml);
                return msg;
            }
            return string.Empty;
        }

        public static string SendInboundInterModalCallCarMail1(MailData data, MixedList ml)
        {
            try
            {
                string tranType = data.Keys[1];
                string partyno = data.Keys[2];
                string groupid = data.Keys[3];
                string cmp = data.Keys[4];
                string mailType = string.Empty;
                string party_name = data.To;

                if (string.IsNullOrEmpty(data.Body))
                    return string.Empty;
                string[] ids = data.Body.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length <= 0)
                    return string.Empty;

                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = SQLUtils.QuotedStr(ids[i]);
                }
                string filter = string.Empty;
                string keyName = "RESERVE_NO";
                mailType = string.Format("I{0}C1", tranType);
                switch (tranType)
                {
                    default:
                        filter = string.Format("{0}.{1} IN ({2})", "SMIRV", "U_ID", string.Join(",", ids));
                        break;
                }

                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, groupid, "IIC"); //interModal 的邮件群组为IIC
                string mailindex = string.Empty;
                List<string> temps = new List<string>();
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailindex) && !temps.Contains(mailindex))
                    {
                        temps.Add(mailindex);
                    }
                }
                data.To = string.Join(";", temps);
                if (string.IsNullOrEmpty(data.To))
                {
                    EvenFactory.SaveMailLog(data, string.Format("{0}无{1}类的mail建档", partyno, "IIC"), EvenFactory.Fail, ml);
                    return string.Format("{0}无{1}类的mail建档", partyno, "IIC");
                }
                Dictionary<string, string> map = new Dictionary<string, string>();
                map["FORWARDER"] = party_name;
                string body = GetBookingMailBody(mailType, groupid, cmp, filter, keyName, map);
                data.Body = body;
                data.Subject = GetSubJectByTranType(tranType, "IICS"); 
                data.MailClient.Send(data.Subject, data.To, "", data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "=>" + e.InnerException.Message;
                EvenFactory.SaveMailLog(data, msg, EvenFactory.Fail, ml);
                return msg;
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送edoc通知mail
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ml"></param>
        /// <returns></returns>
        public static string SendMailByAutoNotifyBrokerError(MailData data, MixedList ml)
        {
            try
            {
                string UId = data.Keys[1];
                string mailType = "IBR_EROR";
                string sql = string.Format(@"SELECT * FROM SMSMI WHERE U_ID = {0}", SQLUtils.QuotedStr(UId));
                DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
                string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);

                Dictionary<string, string> map = new Dictionary<string, string>();
                IMailTemplateParse parse = new DefaultMailParse();
                map.Add("MAIL_BODY_SHOW", data.Body);
                DataTable mailDt = MainDt.Copy();
                string body = MailTemplate.GetMailBody(mailType, group_id, cmp);
                if (!string.IsNullOrEmpty(body))
                {
                    data.Body = parse.Parse(mailDt, null, body, map);
                }
                data.MailClient.Send(data.Subject, data.To, data.Body);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);
            }
            catch (Exception e)
            {
                EvenFactory.SaveMailLog(data, e.StackTrace, EvenFactory.Fail, ml);
                return e.Message;
            }
            return string.Empty;
        }

        public static string SendInboundCallCarMail1(MailData data, MixedList ml)
        {
            try
            {
                string tranType = data.Keys[1];
                string partyno = data.Keys[2];
                string groupid = data.Keys[3];
                string cmp = data.Keys[4];
                string mailType = string.Empty;
                string party_name = data.To;

                if (string.IsNullOrEmpty(data.Body))
                    return string.Empty;
                string[] ids = data.Body.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (ids.Length <= 0)
                    return string.Empty;

                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = SQLUtils.QuotedStr(ids[i]);
                }
                string filter = string.Empty;
                string keyName = "RESERVE_NO";
                mailType = string.Format("I{0}C1", tranType);
                switch (tranType)
                {
                    default:
                        filter = string.Format("{0}.{1} IN ({2})", "SMIRV", "U_ID", string.Join(",", ids));
                        break;
                }

                DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, groupid, "IC");
                string mailindex = string.Empty;
                List<string> temps = new List<string>();
                foreach (DataRow mailGroup in mailGroupDt.Rows)
                {
                    mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                    if (!string.IsNullOrEmpty(mailindex) && !temps.Contains(mailindex))
                    {
                        temps.Add(mailindex);
                    }
                }
                data.To = string.Join(";", temps);
                if (string.IsNullOrEmpty(data.To))
                {
                    EvenFactory.SaveMailLog(data, string.Format("{0}无{1}类的mail建档", partyno, "IC"), EvenFactory.Fail, ml);
                    return string.Format("{0}无{1}类的mail建档", partyno, "C");
                }
                Dictionary<string, string> map = new Dictionary<string, string>();
                map["FORWARDER"] = party_name;
                string body = GetBookingMailBody(mailType, groupid, cmp, filter, keyName, map);
                data.Body = body;
                data.Subject = GetSubJectByTranType(tranType, "ICS"); 
                data.MailClient.Send(data.Subject, data.To, "", data.Body, null);
                EvenFactory.SaveMailLog(data, string.Format("发送给{0}成功", data.To), EvenFactory.Success, ml);

            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "=>" + e.InnerException.Message;
                EvenFactory.SaveMailLog(data, msg, EvenFactory.Fail, ml);
                return msg;
            }
            return string.Empty;
        }
        #region 批次发送mail
        static DataTable SMIDN = null;
        static DataTable SMICNTR = null;
        static DataTable SMRDN = null;
        static DataTable SMRCNTR = null;
        private static string GetBookingMailBody(string mailType, string groupid, string cmp, string filter = "0=1", string keyName = "SHIPMENT_ID",Dictionary<string,string> map=null)
        {
            #region 初始化table
            //if (SMIDN == null)
            //{
            //    SMIDN = OperationUtils.GetDataTable("SELECT * FROM SMIDN WITH(NOLOCK) WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //    SMIDN.TableName = "SMIDN";
            //}
            //if (SMICNTR == null)
            //{
            //    SMICNTR = OperationUtils.GetDataTable("SELECT * FROM SMICNTR WITH(NOLOCK) WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //    SMICNTR.TableName = "SMICNTR";
            //}
            if (SMRDN == null)
            {
                SMRDN = OperationUtils.GetDataTable("SELECT * FROM SMRDN WITH(NOLOCK) WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                SMRDN.TableName = "SMRDN";
            }
            if (SMRCNTR == null)
            {
                SMRCNTR = OperationUtils.GetDataTable("SELECT * FROM SMRCNTR WITH(NOLOCK) WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());
                SMRCNTR.TableName = "SMRCNTR";
            }
            #endregion

            if (string.IsNullOrEmpty(filter))
                filter = "0=1";
            List<string> dList = new List<string>();//明细栏位
            IMailTemplateParse parse = new DefaultMailParse();
            DataTable dt = null;
            string sql = string.Empty;
            string rightCols = string.Empty;
            string rightCols1 = string.Empty;
            string dimCol = string.Empty;
            #region 构建数据源
            switch (mailType)
            {
                case "ILB":
                case "IEB":
                case "IDB":
                case "ITB":
                case "IAB":
                case "ILB1":
                case "IEB1":
                case "IDB1":
                case "ITB1":
                case "IAB1":
                    AddDetailColumn(dList, SMIDN);

                    dimCol = "(SELECT (convert(varchar(10),L) + '*' + convert(varchar(10),W) + '*' +convert(varchar(10),H) +';') FROM SMICUFT WITH(NOLOCK) WHERE SMICUFT.DN_NO=SMIDN.DN_NO FOR XML PATH('')) AS DN_DIM";
                    //SMSMI.SHIPMENT_ID,MASTER_NO,HOUSE_NO,POL_CD,ATD,VESSEL1,VOYAGE1,POD_CD,ATA,TRADE_TERM,SMIDN.DN_NO,DISCHARGE_DATE
                    sql = string.Format("SELECT SMSMI.*,SMIDN.*," + dimCol + " FROM SMSMI WITH(NOLOCK) LEFT JOIN SMIDN WITH(NOLOCK) ON SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID WHERE {0} ORDER BY SMSMI.SHIPMENT_ID DESC", filter);
                    break;
                case "IFB":
                case "IRB":
                case "IFB1":
                case "IRB1":
                    AddDetailColumn(dList, SMICNTR);
                    dimCol = "(SELECT (convert(varchar(10),L) + '*' + convert(varchar(10),W) + '*' +convert(varchar(10),H) +';') FROM SMICUFT WITH(NOLOCK) WHERE SMICUFT.DN_NO=SMICNTR.DN_NO FOR XML PATH('')) AS DN_DIM";
                    //rightCols = GetDetailColumn(SMICNTR);
                    //SMSMI.SHIPMENT_ID,MASTER_NO,HOUSE_NO,POL_CD,ATD,VESSEL1,VOYAGE1,POD_CD,ATA,TRADE_TERM,CNTR_NO,CNTR_TYPE,DISCHARGE_DATE
                    sql = string.Format("SELECT SMSMI.*,SMICNTR.*," + dimCol + " FROM SMSMI WITH(NOLOCK) LEFT JOIN SMICNTR WITH(NOLOCK) ON SMSMI.SHIPMENT_ID=SMICNTR.SHIPMENT_ID WHERE {0} ORDER BY SMSMI.SHIPMENT_ID DESC", filter);
                    break;
                case "ILC":
                case "IEC":
                case "IAC":
                case "ITC":
                case "IDC":
                case "IFC":
                case "IRC":
                case "ILC1":
                case "IEC1":
                case "IAC1":
                case "ITC1":
                case "IDC1":
                case "IFC1":
                case "IRC1":
                default:
                    AddDetailColumn(dList, SMRDN, SMRCNTR);
                    rightCols = GetDetailColumn(SMRDN, SMRCNTR);
                    rightCols1 = GetDetailColumn(SMRDN, SMRCNTR, false);

                    dimCol = "(SELECT (convert(varchar(10),L) + '*' + convert(varchar(10),W) + '*' +convert(varchar(10),H)+';') FROM SMICUFT WITH(NOLOCK) WHERE SMICUFT.DN_NO=SMRDN.DN_NO FOR XML PATH('')) AS DN_DIM";
                    string dimCol1 = "(SELECT (convert(varchar(10),L) + '*' + convert(varchar(10),W) + '*' +convert(varchar(10),H) +';') FROM SMICUFT WITH(NOLOCK) WHERE SMICUFT.DN_NO=SMRCNTR.DN_NO FOR XML PATH('')) AS DN_DIM";
                    //SMSMI.SHIPMENT_ID,MASTER_NO,HOUSE_NO,POL_CD,ATD,VESSEL1,VOYAGE1,POD_CD,ATA,TRADE_TERM,SMIDN.DN_NO,DISCHARGE_DATE
                    sql = string.Format("SELECT SMIRV.*,{0}," + dimCol + " FROM SMIRV WITH(NOLOCK) LEFT JOIN SMRDN WITH(NOLOCK) ON SMIRV.RESERVE_NO=SMRDN.RESERVE_NO WHERE {2} AND RV_TYPE='I' AND CALL_TYPE='D' UNION SELECT SMIRV.*,{1}," + dimCol1 + " FROM SMIRV WITH(NOLOCK) LEFT JOIN SMRCNTR WITH(NOLOCK) ON SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO WHERE {2} AND RV_TYPE='I' AND CALL_TYPE<>'D' ORDER BY SMIRV.RESERVE_NO,SMIRV.SHIPMENT_ID DESC", rightCols, rightCols1, filter);
                    break;
            }


            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            dt.TableName = "MAIN";
            #endregion

            string shipment_id = string.Empty;
            #region 新增缺失栏位
            DataTable copyDt = dt.Clone();
            dt.Columns.Add("COLUMN_DISPLAY", typeof(string));
            dt.Columns["COLUMN_DISPLAY"].MaxLength = 30;

            dt.Columns.Add("TD_ROWSPAN", typeof(string));
            dt.Columns["TD_ROWSPAN"].MaxLength = 30;

            dt.Columns.Add("TD_STYLE", typeof(string));
            dt.Columns["TD_STYLE"].MaxLength = 30;

            if (!dt.Columns.Contains("TERMINAL_NM"))
            {
                List<string> smList = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (!smList.Contains(shipment_id))
                        smList.Add(shipment_id);
                    if (dr.Table.Columns.Contains("SHIPMENT_INFO"))
                    {
                        string[] temps = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]).Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string temp in temps)
                        {
                            if (!smList.Contains(temp))
                                smList.Add(temp);
                        }
                    }
                }
                DataTable smsm = OperationUtils.GetDataTable("SELECT SHIPMENT_ID,TERMINAL_NM,MASTER_NO,HOUSE_NO FROM SMSMI WITH(NOLOCK) WHERE SHIPMENT_ID IN " + SQLUtils.Quoted(smList.ToArray()), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                List<string> othList = new List<string>() { "TERMINAL_NM", "MASTER_NO", "HOUSE_NO" };
                foreach (string name in othList)
                {
                    if (dt.Columns.Contains(name))
                        continue;
                    dt.Columns.Add(name, typeof(string));
                    dt.Columns[name].MaxLength = 9999;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    #region 获取所有相关联的SHIPMENT_iD
                    smList.Clear();
                    shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (!smList.Contains(shipment_id))
                        smList.Add(shipment_id);
                    if (dr.Table.Columns.Contains("SHIPMENT_INFO"))
                    {
                        string[] temps = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]).Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string temp in temps)
                        {
                            if (!smList.Contains(temp))
                                smList.Add(temp);
                        }
                    }
                    #endregion

                    if (smList.Count <= 0)
                        continue;
                    DataRow[] drs = smsm.Select(string.Format("SHIPMENT_ID IN {0}", SQLUtils.Quoted(smList.ToArray())));
                    if (drs.Length <= 0)
                        continue;

                    foreach (string name in othList)
                    {
                        List<string> datas = new List<string>();
                        foreach (DataRow smsmrd in drs)
                        {
                            string temp = Prolink.Math.GetValueAsString(smsmrd[name]);
                            if (!string.IsNullOrEmpty(temp) && !datas.Contains(temp))
                                datas.Add(temp);
                        }
                        if (datas.Count > 0)
                            dr[name] = string.Join(";", datas);
                    }
                }
            }
            #endregion

            #region 设置合并行
            List<string> keyList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                shipment_id = Prolink.Math.GetValueAsString(dr[keyName]);
                string name = "COLUMN_DISPLAY";
                if (keyList.Contains(shipment_id))
                {
                    dr[name] = "style=\"display: none;\"";
                    dr["TD_STYLE"] = "display: none;";
                    continue;
                }
                int count = dt.Select(string.Format("{0}={1}", keyName, SQLUtils.QuotedStr(shipment_id))).Length;
                dr[name] = string.Format("rowspan=\"{0}\"", count);
                dr["TD_ROWSPAN"] = count.ToString();
                if (!keyList.Contains(shipment_id))
                    keyList.Add(shipment_id);
            }
            #endregion

            string body = MailTemplate.GetMailBody(mailType, groupid, cmp);
            #region 测试模板
            //body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\TESMP.HTML", System.Text.Encoding.UTF8);
            //switch (mailType)
            //{
            //    case "ILB":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\LCL訂艙通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //    case "IEB":
            //    case "IAB":
            //    case "ITB":
            //    case "IDB":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\AIR_EXPRESS訂艙通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //    case "IFB":
            //    case "IRB":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\FCL_鐵路訂艙通知.HTML", System.Text.Encoding.UTF8);
            //        break;

            //    case "ILC":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\LCL叫車通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //    case "IEC":
            //    case "IAC":
            //    case "ITC":
            //    case "IDC":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\AIR_EXPRESS叫車通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //    case "IFC":
            //    case "IRC":
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\FCL_鐵路叫車通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //    default:  
            //        body = File.ReadAllText(@"C:\Users\acer\Desktop\更版\AIR_EXPRESS叫車通知.HTML", System.Text.Encoding.UTF8);
            //        break;
            //}
            #endregion
            body = parse.Parse(new DataTable(), dt, body, map);
            body = Regex.Replace(body, "<td((?!<td)[\\S\\s])*none((?!>)[\\S\\s])*>[\\S\\s]*?</td>", "");
            return body;
        }

        private static void AddDetailColumn(List<string> dList, DataTable dt, DataTable dt1 = null)
        {
            if (dt1 != null)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    dList.Add(col.ColumnName);
                }
            }
            if (dt1 != null)
            {
                foreach (DataColumn col in dt1.Columns)
                {
                    if (dList.Contains(col.ColumnName))
                        continue;
                    dList.Add(col.ColumnName);
                }
            }
        }

        private static string GetDetailColumn(DataTable dt, DataTable dt1 = null, bool left = true)
        {
            List<string> igs = new List<string>() { "DEC_CUST", "DEC_REPLY", "TC_DEC_CUST", "TC_DEC_REPLY" };
            List<string> dList = new List<string>();
            List<string> list = new List<string>();
            foreach (DataColumn col in dt.Columns)
            {
                if (col.MaxLength > 1000 || igs.Contains(col.ColumnName))
                {
                    continue;
                }
                dList.Add(col.ColumnName);
                if (left)
                    list.Add(dt.TableName + "." + col.ColumnName + " AS " + col.ColumnName);
                else
                {
                    if (dt1.Columns.Contains(col.ColumnName))
                        list.Add(dt1.TableName + "." + col.ColumnName + " AS " + col.ColumnName);
                    else
                        list.Add("NULL AS " + col.ColumnName);
                }
            }
            if (dt1 != null)
            {
                foreach (DataColumn col in dt1.Columns)
                {
                    if (col.MaxLength > 1000 || igs.Contains(col.ColumnName))
                    {
                        continue;
                    }
                    if (dList.Contains(col.ColumnName))
                        continue;
                    if (left)
                        list.Add("NULL AS " + col.ColumnName);
                    else
                    {
                        list.Add(dt1.TableName + "." + col.ColumnName + " AS " + col.ColumnName);
                    }
                }
            }
            return string.Join(",", list);
        }

        private static string GetSubJectByTranType(string trantype, string type)
        {
            string subject = string.Empty;
            string notice = " Booking Notice ";
            if ("ICS".Equals(type))
            {
                notice = " Truck Calling Notice";
            }
            if ("IICS".Equals(type))
            {
                notice = " Intermodal Call Truck Notice";
            }
            switch (trantype)
            {
                case "A":
                    subject = "Air";
                    break;
                case "L":
                    subject = "LCL";
                    break;
                case "R":
                    subject = "RailWay";
                    break;
                case "F":
                    subject = "FCL";
                    break;
                case "E":
                    subject = "Express";
                    break;
                case "D":
                    subject = "Express";
                    break;
                case "T":
                    subject = "Domestic";
                    break;
                default:
                    subject = "(" + trantype + ")";
                    break;
            }
            return subject + notice;
        }
        #endregion
    }
}
