using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot("BookingInformation")]
    public class OceanBooking : Booking
    {
        [XmlElement]
        public PartyInfo Carrier { get; set; }
        [XmlElement]
        public PartyInfo Trucker { get; set; }
        [XmlIgnore]
        [Required]
        public DateTime? ETD { get; set; }
        [XmlElement]
        public string POR { get; set; }
        [XmlElement]
        public string PORName { get; set; }
        [XmlElement]
        public string POL { get; set; }
        [XmlElement]
        public string POL_Name { get; set; }
        [XmlElement]
        public string POD { get; set; }
        [XmlElement]
        public string POD_Name { get; set; }
        [XmlElement]
        public string DEST { get; set; }
        [XmlElement]
        public string DEST_Name { get; set; }
        [Required]
        [XmlElement]
        public string ServiceMode { get; set; }
        [XmlElement]
        public string ShipMark { get; set; }
        [XmlElement]
        public string CargoType { get; set; }
        [XmlElement]
        public string GP20 { get; set; }
        [XmlElement]
        public string GP40 { get; set; }
        [XmlElement]
        public string HQ40 { get; set; }
        [XmlElement]
        public string OtherCNTType { get; set; }
        [XmlElement]
        public string OtherCNTCount { get; set; }
        public string Contract { get; set; }
        [XmlElement("ETD")]
        public string XETD
        {
            get
            {
                if (ETD.HasValue)
                    return ETD.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                ETD = ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement]
        public string Goods { get; set; }
        [XmlElement]
        public string GoodsCN { get; set; }
    }
}
