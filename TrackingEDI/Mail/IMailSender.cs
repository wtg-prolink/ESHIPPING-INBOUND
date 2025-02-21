using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TrackingEDI.Mail
{
    public interface IMailSender
    {
        void BuildServer(string mailServerXmlPath);
        bool Send(string subject, string receiver, string body);
        string Send(string subject, string receiver, string copyReceiver, string body, object attachs);
        /// <summary>
        /// mail的优先级
        /// </summary>
        MailPriority Priority
        {
            get;
            set;
        }
    }
}
