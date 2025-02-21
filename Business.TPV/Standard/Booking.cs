using Business.EDI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    public class Booking : EntityEDITemplate
    {
        public override XmlEDINode CreateXmlEDINode()
        {
            return null;
        }
        [XmlElement]
        public string UniqueNumber { get; set; }
        [XmlElement]
        public string Mode { get; set; }
        [XmlElement]
        public string EdiType { get; set; }
        [XmlElement]
        public string SenderCode { get; set; }
        [XmlElement]
        public string RecieveCode { get; set; }
        [XmlElement]
        public string MsgCode { get; set; }
        [XmlElement]
        public string ShipType { get; set; }
        [XmlElement]
        public string ShipmentID { get; set; }
        [XmlArray]
        public DNInfo[] DNInfos { get; set; }
        [XmlArray]
        public Commodity[] Commoditys { get; set; }
        [Required]
        [XmlElement]
        public PartyInfo Shipper { get; set; }
        [Required]
        [XmlElement]
        public PartyInfo Consignee { get; set; }
        [XmlElement]
        public PartyInfo Notify { get; set; }
        [XmlElement]
        public PartyInfo Notify2 { get; set; }
        [XmlElement]
        public PartyInfo Notify3 { get; set; }
        [XmlElement]
        public double? GW { get; set; }
        [XmlElement]
        public string GWUnit { get; set; }
        [XmlElement]
        public double? CBM { get; set; }
        [XmlElement]
        public int QTY { get; set; }
        [XmlElement]
        public int CTN { get; set; }
        [XmlElement]
        public string PKGUnit { get; set; }
        [XmlElement]
        public string PKGUnitDesc { get; set; }
        [XmlElement]
        public double? DutiableValue { get; set; }
        [XmlElement]
        public string Currency { get; set; }
        [XmlElement]
        public string OPUser { get; set; }
        [XmlElement]
        public string Remark { get; set; }
        [XmlElement]
        public string ShippingMark { get; set; }
        [XmlElement]
        public PaymentModes PaymentMode { get; set; }
        [XmlElement]
        public string DeliveryTerm { get; set; }
        [XmlElement]
        public string DeliveryTermDesc { get; set; }
    }

    public class Commodity
    {
        [XmlElement]
        public string HTSCode { get; set; }
        [XmlElement]
        public string ProductName { get; set; }
        [XmlElement]
        public string ProductNameCN { get; set; }
        [XmlElement]
        public string QTY { get; set; }
        [XmlElement]
        public string QTYUnit { get; set; }
        [XmlElement]
        public string GW { get; set; }
        [XmlElement]
        public string CBM { get; set; }
    }
}
