using Business.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Business.Utils;

namespace Business.TPV.LSP
{
    class BookingTemplate : Template
    {
        /// <summary>
        /// 拖车公司代码
        /// </summary>
        public string TruckCode { get; set; }
        /// <summary>
        /// 拖车公司名称
        /// </summary>
        public string TruckName { get; set; }
        [Required]
        public string Contract { get; set; }
        public override EdiTypes EdiType
        {
            get { return EdiTypes.Booking; }
        }

        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = base.CreateXmlEDINode();
            XmlEDINode node = new XmlEDINode("TRUCK_CODE", TruckCode);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("TRUCK_NAME", TruckName);
            root.ChildNodes.Add(node);
            node = new XmlEDINode("CONTRACT", Contract);
            root.ChildNodes.Add(node);
            return root;
        }

        protected override void SetDNInfo(XmlEDINode dnDetail, DNInfo info)
        {
            XmlEDINode node = new XmlEDINode("DN_NO", info.DNNO);
            dnDetail.ChildNodes.Add(node);
        }
    }
}
