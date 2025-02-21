using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Business.TPV.Standard
{
    public class ExpressBookingResponse : BookingResponse
    {
        /// <summary>
        /// 快递追踪单号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TackingNumber { get; set; }
        /// <summary>
        /// ETD预计出发日期
        /// </summary>
        [Required]
        [XmlIgnore]
        public DateTime? ETD { get; set; }
        /// <summary>
        /// ETA预计到达日期
        /// </summary>
        [Required]
        [XmlIgnore]
        public DateTime? ETA { get; set; }
        /// <summary>
        /// 司机
        /// </summary>
        [XmlElement]
        public string Driver { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        [XmlElement]
        public string CarNubmer { get; set; }
        /// <summary>
        /// 司机联系电话
        /// </summary>
        [XmlElement]
        public string DriverPhone { get; set; }

        //替代节点
        [XmlElement("ETD")]
        public string XETD
        {
            get
            {
                if (ETD.HasValue) return ETD.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                ETD = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }

        //替代节点
        [XmlElement("ETA")]
        public string XETA
        {
            get
            {
                if (ETA.HasValue)
                    return ETA.Value.ToString("yyyyMMddHHmm");
                return null;
            }
            set
            {
                ETA = DateTimeUtils.ParseToDateTimeForNullValue(value);
            }
        }
    }
}
