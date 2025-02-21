using Business.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    public class ShippingBookingResponse : BookingResponse
    {
        /// <summary>
        /// 運輸別
        /// </summary>
        [StringLength(1)]
        public string ShipType { get; set; }
        /// <summary>
        /// 运输公司代碼
        /// </summary>
        [StringLength(10)]
        [Required]
        public string CarrierID { get; set; }
        /// <summary>
        /// 主提單號
        /// </summary>
        [Required]
        [StringLength(30)]
        public string MasterNO { get; set; }
        /// <summary>
        /// 分提單號
        /// </summary>
        [StringLength(30)]
        public string HouserNO { get; set; }
        /// <summary>
        /// 預計出發
        /// </summary>
        [Required]
        [XmlIgnore]
        public DateTime? ETD { get; set; }
        /// <summary>
        /// 預計抵達
        /// </summary>
        [Required]
        [XmlIgnore]
        public DateTime? ETA { get; set; }

        [StringLength(20)]
        public string BookingNO { get; set; }

        [XmlElement("ETD")]
        public string XETD
        {
            get
            {
                if (ETD.HasValue)
                    return ETD.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                ETD = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
        
        [XmlElement("ETA")]
        public string XETA
        {
            get
            {
                if (ETA.HasValue)
                    return ETA.Value.ToString("yyyyMMdd");
                return null;
            }
            set
            {
                ETA = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
    }
}
