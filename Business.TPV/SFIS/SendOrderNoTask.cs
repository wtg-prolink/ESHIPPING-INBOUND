using Business.TPV.Base;
using Business.Utils;
using Prolink.Data;
using Prolink.Persistence;
using Prolink.Task;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.SFIS
{
    class SendOrderNoTask : IPlanTask
    {
        IPlanTaskMessenger _messenger;

        protected string Location { get; private set; }
        public SendOrderNoTask() { }
        public SendOrderNoTask(string location)
        {
            Location = location;
        }

        public void Run(IPlanTaskMessenger messenger)
        {
            try
            {
                _messenger = messenger;
                //string sql = "SELECT JOB_NO,DN_NO,ODN_NO,(SELECT TOP 1 STATUS FROM SMSM WHERE DN_NO=SMDNP.DN_NO) AS STATUS FROM SMDNP WHERE JOB_NO IS NOT NULL AND SEND_SFIS_FLAG='N'";
                string sql = string.Format(@"SELECT SMDNP.JOB_NO,SMDNP.DN_NO,SMDNP.ODN_NO,(SELECT TOP 1 V_SMDN.SM_STATUS FROM V_SMDN WHERE V_SMDN.DN_NO=SMDNP.DN_NO) AS STATUS
                FROM SMDNP,SMDN WHERE SMDNP.DN_NO=SMDN.DN_NO AND SMDNP.JOB_NO IS NOT NULL AND SMDN.CARGO_TYPE='A' AND SMDNP.SEND_SFIS_FLAG='N'AND SMDN.CMP={0}", SQLUtils.QuotedStr(Location));

                DataTable dt = DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
                if (dt == null || dt.Rows.Count <= 0) return;
                List<OrderInfo> infos = GetOrderInfo(dt).ToList();
                string filePath = Manager.SendOrderNO(infos, Location);
                UpdateFlag(infos);
                SendOrderNOEDILog log = new SendOrderNOEDILog(infos, filePath);
                Business.Utils.Context.Logger.WriteLog(log.CreateSucceed());
            }
            catch (Exception e)
            {
                string msg = string.Format("Send Order Number Error! {0}{1}", Environment.NewLine, e.Message);
                string subject = "Send Order Number Error!";
                SendMail(subject, msg);
                SendOrderNOEDILog log = new SendOrderNOEDILog(null, null);
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

        void UpdateFlag(List<OrderInfo> infos)
        {
            string sql = string.Format("UPDATE SMDNP SET SEND_SFIS_FLAG='Y' WHERE JOB_NO IN({0})",
                string.Join(",", infos.Select(item => SQLUtils.QuotedStr(item.Number))));
            DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(sql);
        }

        IEnumerable<OrderInfo> GetOrderInfo(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                OrderInfo info = new OrderInfo();
                info.Number = Prolink.Math.GetValueAsString(row["JOB_NO"]);
                string status = Prolink.Math.GetValueAsString(row["STATUS"]);
                switch (status)
                {
                    case "O": info.Status = "1"; break;
                    default: info.Status = "0"; break;
                }
                yield return info;
            }
        }
    }
}
