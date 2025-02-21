using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot(ElementName = "BillingInformation")]
    public class BillingInfo : InfoBase
    {
        [XmlArray(ElementName = "Details")]
        public BillingDetail[] BillingDetails { get; set; }

        public override string RecieveCode
        {
            get;
            set;
        }
    }

    [XmlType("Detail")]
    public class BillingDetail
    {
        [Required]
        [StringLength(20)]
        [XmlElement]
        public string ShipmentID { get; set; }
        [XmlElement("BL_NO")]
        public string BLNO { get; set; }
        [XmlElement("DEBIT_NO")]
        public string DebitNO { get; set; }
        [XmlIgnore]
        public DateTime? DebitDate { get; set; }
        [Required]
        [XmlElement("CHG_CODE")]
        //[XmlElement("CHG_CD")]
        public string ChargeCode { get; set; }
        [XmlElement("CHG_DESCP")]
        public string ChargeDescp { get; set; }
        [XmlElement("CUR")]
        public string Currency { get; set; }
        [XmlElement("Unit_Price")]
        public string UnitPrice { get; set; }
        [XmlElement("CHG_UNIT")]
        public string ChargeUnit { get; set; }
        [XmlElement]
        public string QTY { get; set; }
        [XmlElement("QTYU")]
        public string QTYUnit { get; set; }
        [XmlElement("BAMT")]
        public string Amount { get; set; }

        [XmlElement("VAT_NO")]
        public string VatNO { get; set; }
        [XmlElement]
        public string Remark { get; set; }
        [XmlElement]
        public string Tax { get; set; }

        [XmlElement("DEBIT_DATE")]
        public string XDebitDate
        {
            get
            {
                if (DebitDate.HasValue)
                    return DebitDate.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                DebitDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
    }
}