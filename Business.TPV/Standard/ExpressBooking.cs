using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot("BookingInformation")]
    public class ExpressBooking : Booking
    {
        [XmlElement]
        public string Commodity { get; set; }
        [XmlElement]
        public string PaymentAccountNumber { get; set; }
        [XmlElement]
        public PartyInfo ShipTo { get; set; }

        public string Option { get; set; }
        public string ServiceMode { get; set; }
        public string PickupDate { get; set; }
    }
    public enum PaymentModes { P, C, O }

    public class DNInfo
    {
        [XmlElement]
        public string DNNO { get; set; }
        public string UniCode { get; set; }
        public string ExportNO { get; set; }
        public string PONO { get; set; }
    }

    public class PartyInfo
    {
        [XmlElement]
        public string ID { get; set; }
        [XmlElement]
        public string Code { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Address { get; set; }
        [XmlElement]
        public string Address2 { get; set; }
        [XmlElement]
        public string Address3 { get; set; }
        [XmlElement]
        public string CountryCode { get; set; }
        [XmlElement]
        public string CountryName { get; set; }
        [XmlElement]
        public string CityName { get; set; }
        [XmlElement]
        public string Tel { get; set; }
        [XmlElement]
        public string PostalCode { get; set; }
        [XmlElement]
        public string Email { get; set; }
        [XmlElement]
        public string Contact { get; set; }
        [XmlIgnore]
        public string PartyDisplay { get; set; }       
    }
}







































