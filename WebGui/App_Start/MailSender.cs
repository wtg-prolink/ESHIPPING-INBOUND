using System;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace WebGui
{
    public class MailSender:IMailSender
    {
        private string _sender;                   //源地址
        private string _server;                  //SMTP服务
        private string _id;                //SMTP服务验证ID
        private string _password;               //SMTP服务验证密码


        public MailSender(string server, string id, string password, string sender)
        {
            this._server = server;
            this._id = id;
            this._password = password;
            this._sender = sender;
            this.Priority = MailPriority.Normal;
        }

        public MailSender(string mailServerXmlPath)
        {
            BuildServer(mailServerXmlPath);
            this.Priority = MailPriority.Normal;
        }

        /// <summary>
        /// 构建mailserver
        /// </summary>
        /// <param name="mailServerXmlPath"></param>
        public void BuildServer(string mailServerXmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(mailServerXmlPath);
            //doc.LoadXml(mailServer);
            XmlNodeList xnl = doc.GetElementsByTagName("mail-from");
            if (xnl.Count > 0) _sender = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_name");
            if (xnl.Count > 0) _server = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_id");
            if (xnl.Count > 0) _id = xnl[0].InnerText;

            xnl = doc.GetElementsByTagName("mail-server_pwd");
            if (xnl.Count > 0) _password = xnl[0].InnerText;

            ////add by gavin on 2008-09-08
            //xnl = doc.GetElementsByTagName("mail-bcc");
            //if (xnl.Count > 0) mailBcc = xnl[0].InnerText;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="receiver">收件人</param>
        /// <param name="body">邮件内容</param>
        public bool Send(string subject, string receiver, string body)
        {
          return  Send(subject, receiver, "", body, null);
        }

        /// <summary>
        /// 返回出错消息
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="receiver"></param>
        /// <param name="copyReceiver"></param>
        /// <param name="body"></param>
        /// <param name="attachs"></param>
        /// <returns></returns>
        public string Send(string subject, string receiver, string copyReceiver, string body, object attachs)
        {
            try
            {
                Send(subject, receiver, copyReceiver, body, attachs as Attachment[]);
            }
            catch (Exception e)
            {
                string msg = "Exception:" + e.Message;
                if(e.StackTrace!=null)
                    msg+="->"+e.StackTrace;
                if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                    msg += "(" + e.InnerException.Message + ")";
                return msg;
            }
            return "";
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="receiver">收件人</param>
        /// <param name="copyReceiver">抄送人</param>
        /// <param name="body">邮件内容</param>
        /// <param name="attachs">htm显示时所需的图片资源或附件</param>
        public bool Send(string subject, string receiver, string copyReceiver, string body, Attachment[] attachs)
        {
            using (MailMessage msg = new MailMessage())
            {
                if (string.IsNullOrEmpty(_sender))
                {
                    throw new Exception("Sender can't be null");
                }

                if (string.IsNullOrEmpty(receiver))
                {
                    throw new Exception("Receiver can't be null");
                }

                if (attachs != null)//添加附件
                {
                    AlternateView htmlBody = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    //LinkedResource lrImage = new LinkedResource(@"d:\1.jpg", "image/gif");//src=\"cid:weblogo\
                    //lrImage.ContentId = "weblogo"; //此处的ContentId 对应 htmlBodyContent 内容中的 cid: ，如果设置不正确，请不会显示图片   
                    for (int i = 0; i < attachs.Length; i++)
                        msg.Attachments.Add(attachs[i]);
                        //htmlBody.LinkedResources.Add(attachs[i]);
                    msg.AlternateViews.Add(htmlBody);
                    
                }

                msg.Body = body;//设置内容

                msg.From = new MailAddress(_sender);//发信人
                if (msg.To != null)//收件人
                    msg.To.Add(receiver.Replace(";", ","));

                if (copyReceiver != null && msg.CC != null && !string.IsNullOrEmpty(copyReceiver.Trim())) msg.CC.Add(copyReceiver.Replace(";", ","));
                msg.Subject = subject;

                msg.BodyEncoding = Encoding.UTF8;
                msg.IsBodyHtml = true;//msg.BodyFormat   = MailFormat.Html;

                //mail发送优先级别
                msg.Priority = this.Priority;

                SmtpClient smtp = new SmtpClient(_server);
                smtp.EnableSsl = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new System.Net.NetworkCredential(_id, _password);
                smtp.Send(msg);

                return true;

                #region 夹带附件的另一种方式
                //if (attachs != null)//添加附件
                //{
                //    for (int i = 0; i < attachs.Length; i++)
                //        msg.Attachments.Add(attachs[i]);
                //}
                //if (lrImages != null)
                //{
                //    AlternateView htmlBody = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                //    //LinkedResource lrImage = new LinkedResource(@"d:\1.jpg", "image/gif");//src=\"cid:weblogo\
                //    //lrImage.ContentId = "weblogo"; //此处的ContentId 对应 htmlBodyContent 内容中的 cid: ，如果设置不正确，请不会显示图片   
                //    for (int i = 0; i < lrImages.Length; i++)
                //        htmlBody.LinkedResources.Add(lrImages[i]);
                //    msg.AlternateViews.Add(htmlBody);
                //}
                #endregion

                #region 其他功能
                ////要求回执的标志   
                //msg.Headers.Add("Disposition-Notification-To", "test@163.com");   
                ////自定义邮件头   
                //msg.Headers.Add("X-Website", "http://www.fenbi360.net");   
                ////针对 LOTUS DOMINO SERVER，插入回执头   
                //msg.Headers.Add("ReturnReceipt", "1");
                #endregion

            }
        }

        /// <summary>
        /// mail的优先级
        /// </summary>
        public MailPriority Priority
        {
            get;
            set;
        }
    }
}
