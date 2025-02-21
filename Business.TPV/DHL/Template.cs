using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.DHL
{
    public abstract class Template : EntityEDITemplate
    {
        protected void SetRequst(XmlEDINode root)
        {
            Business.EDI.XmlEDINode requst = new Business.EDI.XmlEDINode("Request");
            Business.EDI.XmlEDINode header = new Business.EDI.XmlEDINode("ServiceHeader");
            requst.ChildNodes.Add(header);
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("MessageTime");
            node.Value = DateTime.Now.ToString("yyyy-MM-ddTHH:MM:sszzz");
            header.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("MessageReference",UniqueNumber);
            header.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("SiteID", Certification.User);
            header.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("Password", Certification.Password);
            header.ChildNodes.Add(node);
            root.ChildNodes.Add(requst);
        }

        public DHLCertification Certification { get; set; }


        /// <summary>
        /// 报文唯一标识
        /// </summary>
        [Required]
        [StringLength(32, MinimumLength = 28)]
        public string UniqueNumber { get; set; }
    }
}
