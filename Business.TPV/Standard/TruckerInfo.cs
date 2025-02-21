using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot(ElementName = "TruckerInfomation")]
    public class TruckerInfo:Truck
    {
        [StringLength(40)]
        [Required]
        [XmlElement]
        public string ShipmentId { get; set; }

        [StringLength(1)]
        [Required]
        [XmlElement]
        public string Mode { get; set; }

        [StringLength(200)]
        [Required]
        [XmlElement]
        public string ContainerNo { get; set; }

        [StringLength(200)]
        [Required]
        [XmlElement]
        public string EachCntrQty { get; set; }

        [XmlIgnore]
        public DateTime? ReceivedDate { get; set; }
        [XmlElement("ReceivedDate")]
        public string XReceivedDate
        {
            get
            {
                if (ReceivedDate.HasValue)
                    return ReceivedDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                ReceivedDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        [StringLength(20)]
        [Required]
        [XmlElement]
        public string NewTrailerNo { get; set; }
        
        [XmlIgnore]
        public DateTime? ShipDate { get; set; }

        [XmlElement("ShipDate")]
        public string XShipDate
        {
            get
            {
                if (ShipDate.HasValue)
                    return ShipDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                ShipDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        [XmlIgnore]
        public DateTime? NewEtaHub { get; set; }

        [XmlElement("NewEtaHub")]
        public string XNewEtaHub
        {
            get
            {
                if (NewEtaHub.HasValue)
                    return NewEtaHub.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                NewEtaHub = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        [XmlIgnore]
        public DateTime? EtaRailRampDate { get; set; }

        [XmlElement("EtaRailRampDate")]
        public string XEtaRailRampDate
        {
            get
            {
                if (EtaRailRampDate.HasValue)
                    return EtaRailRampDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                EtaRailRampDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        [StringLength(20)]
        [XmlElement]
        public string Driver { get; set; }

        [StringLength(30)]
        [XmlElement]
        public string CardId { get; set; }

        [StringLength(20)]
        [XmlElement]
        public string Mobile { get; set; }
    }

    [XmlRoot(ElementName = "TruckerInfomation")]
    public class TruckerInfoR:Truck
    {
        [Required]
        [XmlElement]
        public string ShipmentId { get; set; }
        [Required]
        public string ContainerNo { get; set; }
        [XmlIgnore]
        public DateTime? HeavyPickupDate { get; set; }
        [Required]
        [XmlIgnore]
        public DateTime? EmptyReturnDate { get; set; }
        [XmlIgnore]
        public DateTime? AtYardDate { get; set; }
        [XmlIgnore]
        public DateTime? LeaveYardDate { get; set; }
        [XmlIgnore]
        public DateTime? PortDate { get; set; }
        [XmlIgnore]
        public string PortTime { get; set; }
        [XmlIgnore]
        public DateTime? TerminDate { get; set; }
        [StringLength(200)]
        [XmlElement]
        public string Remark { get; set; }
        [XmlElement("TerminDate")]
        public string XTerminDate
        {
            get
            {
                if (TerminDate.HasValue)
                    return TerminDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                TerminDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("PortDate")]
        public string XPortDate
        {
            get
            {
                if (PortDate.HasValue)
                    return PortDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                PortDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("AtYardDate")]
        public string XAtYardDate
        {
            get
            {
                if (AtYardDate.HasValue)
                    return AtYardDate.Value.ToString("yyyyMMddHHmmss");
                return null;
            }
            set
            {
                AtYardDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("LeaveYardDate")]
        public string XLeaveYardDate
        {
            get
            {
                if (LeaveYardDate.HasValue)
                    return LeaveYardDate.Value.ToString("yyyyMMddHHmmss");
                return null;
            }
            set
            {
                LeaveYardDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("EmptyReturnDate")]
        public string XEmptyReturnDate
        {
            get
            {
                if (EmptyReturnDate.HasValue)
                    return EmptyReturnDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                EmptyReturnDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("HeavyPickupDate")]
        public string XHeavyPickupDate
        {
            get
            {
                if (HeavyPickupDate.HasValue)
                    return HeavyPickupDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                HeavyPickupDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        [StringLength(20)]
        [XmlElement]
        public string Driver { get; set; }

        [StringLength(30)]
        [XmlElement]
        public string CardId { get; set; }

        [StringLength(20)]
        [XmlElement]
        public string Mobile { get; set; }
    }

    [XmlRoot(ElementName = "TruckerInfomation")]
    public class TruckerInfoW:Truck
    {
        [Required]
        [XmlElement]
        public string ShipmentId { get; set; }
        [Required]
        [XmlElement]
        public string NewTrailerNo { get; set; }
        [XmlIgnore]
        public DateTime? GateInDate { get; set; }
        [XmlIgnore]
        public DateTime? PodDate { get; set; }
        [XmlIgnore]
        public DateTime? GateOutDate { get; set; }
        [XmlElement("GateInDate")]
        public string XGateInDate
        {
            get
            {
                if (GateInDate.HasValue)
                    return GateInDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                GateInDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("PodDate")]
        public string XPodDate
        {
            get
            {
                if (PodDate.HasValue)
                    return PodDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                PodDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("GateOutDate")]
        public string XGateOutDate
        {
            get
            {
                if (GateOutDate.HasValue)
                    return GateOutDate.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                GateOutDate = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
    }

    [XmlRoot(ElementName = "TruckerInfomation")]
    public class TruckerInfoF : Truck
    {
        [Required]
        [XmlElement]
        public string ShipmentId { get; set; }
        [Required]
        public string ContainerNo { get; set; }
        [XmlIgnore]
        public DateTime? PodDate { get; set; }
        [Required]
        public string OutputFormat{ get; set; }
        [Required]
        public string OutputImage { get; set; }
    }

    public class Truck
    {
        [StringLength(20)]
        [Required]
        [XmlElement]
        public string Sender { get; set; }
    }
}
