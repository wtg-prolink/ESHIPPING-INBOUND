using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Prolink.Log;
using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Net;

namespace Business.Mail
{
    public class MailServices
    {
        private static DefaultLogger logger;
        private MailServices() { }
        public static MailServices GetInstance()
        {
            return new MailServices();
        }

        public void SetLogger(DefaultLogger logger)
        {
            MailServices.logger = logger;
        }

        private static MailServerInfo _serverInfo;
        private static string DefaultFrom;

        void BuildServer(Prolink.Context.Context ctx)
        {
            if (_serverInfo != null) return;
            string mailServerXmlPath = ctx.GetProperty("mail-server");
            XmlDocument doc = new XmlDocument();
            doc.Load(mailServerXmlPath);
            _serverInfo = new MailServerInfo();

            XmlNodeList xnl = doc.GetElementsByTagName("mail-from");
            if (xnl.Count > 0) DefaultFrom = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_name");
            if (xnl.Count > 0) _serverInfo.Server = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_id");
            if (xnl.Count > 0) _serverInfo.User = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_pwd");
            if (xnl.Count > 0) _serverInfo.Password = xnl[0].InnerText;
        }

        bool Check(MailInfo info)
        {
            if (string.IsNullOrEmpty(info.From))
            {
                string errorMsg = "Send mail fail,From is null!";
                if (logger != null)
                    logger.WriteLog(errorMsg, Logger.ERROR);
                throw new Exception(errorMsg);
            }

            if (string.IsNullOrEmpty(info.To))
            {
                string errMsg = "Send mail fail,To is null!";
                if (logger != null) logger.WriteLog(errMsg, Logger.ERROR);
                throw new Exception(errMsg);
            }
            return true;
        }

        public bool SendMail(MailInfo info)
        {
            if (_serverInfo == null)
                BuildServer(Prolink.Web.WebContext.GetInstance());
            MailMessage msg = new MailMessage();
            if (string.IsNullOrEmpty(info.From))
                info.From = DefaultFrom;
            Check(info);
            msg.From = new MailAddress(info.From);
            if (!string.IsNullOrEmpty(info.To))
            {
                string[] toList = info.To.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string to in toList)
                    msg.To.Add(to);
            }
            if (!string.IsNullOrEmpty(info.CC))
            {
                string[] toList = info.CC.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cc in toList)
                    msg.CC.Add(cc);
            }
            msg.Subject = info.Subject;
            msg.Body = info.Body;
            msg.BodyEncoding = info.BodyEncoding;
            msg.IsBodyHtml = !string.IsNullOrEmpty(info.BodyFormat) && info.BodyFormat.ToUpper() == "HTML";
            msg.Priority = info.Priority.HasValue ? info.Priority.Value : MailPriority.High;
            foreach (var item in info.Attachments)
                msg.Attachments.Add(item);
            return SendMail(msg);
        }

        public bool SendMail(MailMessage msg)
        {
            try
            {
                if (_serverInfo == null)
                    BuildServer(Prolink.Web.WebContext.GetInstance());
                SmtpClient smtpMail = new SmtpClient();
                smtpMail.Host = _serverInfo.Server;
                smtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpMail.Credentials = new NetworkCredential(_serverInfo.User, _serverInfo.Password);
                smtpMail.Send(msg);
                return true;
            }
            catch (Exception e)
            {
                logger.WriteLog(e);
                return false;
            }
        }
    }

    class MailServerInfo
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class MailInfo
    {
        public MailInfo()
        {
            Attachments = new List<Attachment>();
            BodyEncoding = Encoding.UTF8;
        }
        public string To { get; set; }
        public string CC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Format { get; set; }
        public string From { get; set; }
        public Encoding BodyEncoding { get; set; }
        public MailPriority? Priority { get; set; }
        public string BodyFormat { get; set; }
        public List<Attachment> Attachments
        {
            get;
            private set;
        }
    }
}
