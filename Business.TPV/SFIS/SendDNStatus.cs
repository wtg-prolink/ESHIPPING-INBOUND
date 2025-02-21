using Business.TPV.Base;
using Prolink.Data;
using Prolink.Task;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.SFIS
{
    class SendDNStatus : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            try
            {
                _messenger = messenger;
                string sql = @"SELECT DN_NO,ORIGIN_NO,CMP FROM SMDN WHERE DN_NO_CMP_REF IS NULL AND (SEND_SFIS_FLAG IS NULL OR SEND_SFIS_FLAG='N') AND 
 CARGO_TYPE='A' AND
(SELECT COUNT(*) FROM SMDNP P WHERE SMDN.U_ID=P.U_FID AND UPPER(P.JOB_NO)='X')<=0";
                DataTable dt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
                if (dt == null || dt.Rows.Count <= 0) return;

                DataView dataView = dt.DefaultView;
                DataTable dataTableDistinct = dataView.ToTable(true, "CMP");

                if (dataTableDistinct.Rows.Count > 0)
                {
                    foreach(DataRow ditrow in dataTableDistinct.Rows)
                    {
                        string cmp=ditrow["CMP"].ToString();
                        DataRow[] drs = dt.Select("CMP='" + cmp + "'");
                        var dritems = GetDNStatusFinishInfo(drs,false).Distinct().ToList();
                        if (dritems == null || dritems.Count <= 0) return;
                        string filePath = Manager.SendDNStatus(dritems,cmp);
                        UpdateFlag(dritems);
                        SendDNStatusEDILog log = new SendDNStatusEDILog(dritems, filePath);
                        Business.Utils.Context.Logger.WriteLog(log.CreateSucceed());
                    }
                }

                sql = @"SELECT TOP 300 DN_NO,ORIGIN_NO,CMP,SEAL_QTY,ETD,QTY,SM_STATUS FROM V_SMDN WHERE  SEAL_QTY!=QTY
                AND SM_STATUS IN('O','H') AND  CARGO_TYPE='A' AND (CNEE3_CD IS NULL OR CNEE3_CD='N')";
                dt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
                dataView = dt.DefaultView;
                dataTableDistinct = dataView.ToTable(true, "CMP");

                if (dataTableDistinct.Rows.Count > 0)
                {
                    foreach (DataRow ditrow in dataTableDistinct.Rows)
                    {
                        string cmp = ditrow["CMP"].ToString();
                        DataRow[] drs = dt.Select("CMP='" + cmp + "'");
                        var dritems = GetDNStatusFinishInfo(drs, true).Distinct().ToList();
                        if (dritems == null || dritems.Count <= 0) return;
                        try
                        {
                            string filePath = Manager.SendDNStatus(dritems, cmp);
                            UpdateExcd(dritems);
                            SendDNStatusEDILog log = new SendDNStatusEDILog(dritems, filePath);
                            Business.Utils.Context.Logger.WriteLog(log.CreateSucceed());
                        }
                        catch (Exception e)
                        {
                            string msg = string.Format("Send DN Status Error! {0}{1}", Environment.NewLine, e.ToString());
                            string subject = "Send DN Status Error!";
                            SendMail(subject, msg);
                            Prolink.Log.Logger logger = _messenger.GetLogger();
                            if (logger != null)
                                logger.WriteLog(msg);
                            SendDNStatusEDILog log = new SendDNStatusEDILog(null, null);
                            Business.Utils.Context.Logger.WriteLog(log.CreateEx(e));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("Send DN Status Error! {0}{1}", Environment.NewLine, e.ToString());
                string subject = "Send DN Status Error!";
                SendMail(subject, msg);
                Prolink.Log.Logger logger = _messenger.GetLogger();
                if (logger != null)
                    logger.WriteLog(msg);
                SendDNStatusEDILog log = new SendDNStatusEDILog(null, null);
                Business.Utils.Context.Logger.WriteLog(log.CreateEx(e));
            }
        }

        void SendMail(string subject, string msg)
        {
            string to = _messenger.GetRuntime().GetTaskDefinition().MailToOnError;
            string cc = _messenger.GetRuntime().GetTaskDefinition().MailCcOnError;
            Mail.MailServices mail = Mail.MailServices.GetInstance();
            mail.SendMail(new Mail.MailInfo
            {
                To = to,
                CC = cc,
                Body = msg,
                Subject = subject
            });
        }

        void UpdateFlag(List<DNStatusInfo> infos)
        {
            if (infos == null || infos.Count <= 0) return;
            string sql = string.Format("UPDATE SMDN SET SEND_SFIS_FLAG='Y' WHERE DN_NO IN({0})",
                string.Join(",", infos.Select(item => SQLUtils.QuotedStr(item.CmpDNNO))));
            DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(sql);
        }

        void UpdateExcd(List<DNStatusInfo> infos)
        {
            if (infos == null || infos.Count <= 0) return;
            string sql = string.Format("UPDATE SMDN SET CNEE3_CD='Y' WHERE DN_NO IN({0})",
                string.Join(",", infos.Select(item => SQLUtils.QuotedStr(item.CmpDNNO))));
            DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(sql);
        }

        IEnumerable<DNStatusInfo> GetDNStatusInfo(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                DNStatusInfo info = new DNStatusInfo();
                info.DNNO = Prolink.Math.GetValueAsString(row["ORIGIN_NO"]);
                info.Status = DNStatus.Unfinished;
                info.CmpDNNO = Prolink.Math.GetValueAsString(row["DN_NO"]);
                yield return info;
            }
        }

        IEnumerable<DNStatusInfo> GetDNStatusFinishInfo(DataRow[] drs,bool isFinish )
        {
            
            foreach (DataRow row in drs)
            {
                DNStatusInfo info = new DNStatusInfo();
                info.DNNO = Prolink.Math.GetValueAsString(row["ORIGIN_NO"]);
                if (isFinish)
                {
                    info.Status = DNStatus.Finished;
                }
                else
                {
                    info.Status = DNStatus.Unfinished;
                }
                info.CmpDNNO = Prolink.Math.GetValueAsString(row["DN_NO"]);
                yield return info;
            }
        }
    }
}