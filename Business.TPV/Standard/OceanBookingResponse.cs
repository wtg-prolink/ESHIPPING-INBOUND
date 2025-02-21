using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    [XmlRoot]
    public class OceanBookingResponse : ShippingBookingResponse
    {
        /// <summary>
        /// SCAC CODE
        /// </summary>
        [StringLength(4)]
        [Required]
        public string SCAC { get; set; }
        /// <summary>
        /// 收货港
        /// </summary>
        [Required]
        [StringLength(5)]
        public string POR { get; set; }
        /// <summary>
        /// 装货港
        /// </summary>
        [Required]
        [StringLength(5)]
        public string POL { get; set; }
        /// <summary>
        /// 卸货港
        /// </summary>
        [Required]
        [StringLength(5)]
        public string POD { get; set; }
        /// <summary>
        /// 目的地港口
        /// </summary>
        [Required]
        [StringLength(5)]
        public string DEST { get; set; }
        /// <summary>
        /// 裝貨碼頭
        /// </summary>
        [Required]
        public string Port { get; set; }
        /// <summary>
        /// 截进港日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? PortClose1 { get; set; }
        /// <summary>
        /// 码头截进港日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? PortClose2 { get; set; }
        /// <summary>
        /// 海关截投单日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? DeclClose { get; set; }
        /// <summary>
        /// 码头截放行日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? ReleaseClose { get; set; }
        /// <summary>
        /// 截海外申報日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? AMSClose { get; set; }
        /// <summary>
        /// 放箱日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? CntrRelease { get; set; }
        /// <summary>
        /// 截提单日期
        /// </summary>
        [XmlIgnore]
        public DateTime? CutBL { get; set; }
        /// <summary>
        /// 簽單日期時間
        /// </summary>
        [XmlIgnore]
        public DateTime? DeliveryNote { get; set; }
        /// <summary>
        /// 船舶名稱(第一段)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string Vessel1 { get; set; }
        /// <summary>
        /// 船舶航次(第一段)
        /// </summary>
        [Required]
        [StringLength(12)]
        public string Voyage1 { get; set; }
        /// <summary>
        /// 預計出發(第一段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETD1 { get; set; }
        /// <summary>
        /// 預計抵達(第一段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETA1 { get; set; }

        /// <summary>
        /// 船舶名稱(第二段)
        /// </summary>
        public string Vessel2 { get; set; }
        /// <summary>
        /// 船舶航次(第二段)
        /// </summary>
        public string Voyage2 { get; set; }
        /// <summary>
        /// 預計出發(第二段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETD2 { get; set; }
        /// <summary>
        /// 預計抵達(第二段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETA2 { get; set; }

        /// <summary>
        /// 船舶名稱(第三段)
        /// </summary>
        public string Vessel3 { get; set; }
        /// <summary>
        /// 船舶航次(第三段)
        /// </summary>
        public string Voyage3 { get; set; }
        /// <summary>
        /// 預計出發(第三段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETD3 { get; set; }
        /// <summary>
        /// 預計抵達(第三段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETA3 { get; set; }
        /// <summary>
        /// 船舶名稱(第四段)
        /// </summary>
        public string Vessel4 { get; set; }
        /// <summary>
        /// 船舶航次(第四段)
        /// </summary>
        public string Voyage4 { get; set; }
        /// <summary>
        /// 預計出發(第四段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETD4 { get; set; }
        /// <summary>
        /// 預計抵達(第四段)
        /// </summary>
        [XmlIgnore]
        public DateTime? VesselETA4 { get; set; }

        [XmlElement("VesselETD1")]
        public string XVesselETD1
        {
            get
            {
                if (VesselETD1.HasValue)
                    return VesselETD1.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETD1 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETA1")]
        public string XVesselETA1
        {
            get
            {
                if (VesselETA1.HasValue)
                    return VesselETA1.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETA1 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETD2")]
        public string XVesselETD2
        {
            get
            {
                if (VesselETD2.HasValue)
                    return VesselETD2.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETD2 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETA2")]
        public string XVesselETA2
        {
            get
            {
                if (VesselETA2.HasValue)
                    return VesselETA2.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETA2 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETD3")]
        public string XVesselETD3
        {
            get
            {
                if (VesselETD3.HasValue)
                    return VesselETD3.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETD3 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETA3")]
        public string XVesselETA3
        {
            get
            {
                if (VesselETA3.HasValue)
                    return VesselETA3.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETA3 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETD4")]
        public string XVesselETD4
        {
            get
            {
                if (VesselETD4.HasValue)
                    return VesselETD4.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETD4 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("VesselETA4")]
        public string XVesselETA4
        {
            get
            {
                if (VesselETA4.HasValue)
                    return VesselETA4.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                VesselETA4 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("Port1Close")]
        public string XPortClose1
        {
            get
            {
                if (PortClose1.HasValue)
                    return PortClose1.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                PortClose1 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("Port2Close")]
        public string XPortClose2
        {
            get
            {
                if (PortClose2.HasValue)
                    return PortClose2.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                PortClose2 = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("DeclClose")]
        public string XDeclClose
        {
            get
            {
                if (DeclClose.HasValue)
                    return DeclClose.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                DeclClose = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("ReleaseClose")]
        public string XReleaseClose
        {
            get
            {
                if (ReleaseClose.HasValue)
                    return ReleaseClose.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                ReleaseClose = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("AMSClose")]
        public string XAMSClose
        {
            get
            {
                if (AMSClose.HasValue)
                    return AMSClose.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                AMSClose = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("CntrRelease")]
        public string XCntrRelease
        {
            get
            {
                if (CntrRelease.HasValue)
                    return CntrRelease.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                CntrRelease = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("CutBL")]
        public string XCutBL
        {
            get
            {
                if (CutBL.HasValue)
                    return CutBL.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                CutBL = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        [XmlElement("DeliveryNote")]
        public string XDeliveryNote
        {
            get
            {
                if (DeliveryNote.HasValue)
                    return DeliveryNote.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                DeliveryNote = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
    }
}