using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Models.EDI.CPL
{
    public class BookingResponse
    {
        /// <summary>
        /// 物流公司代码
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Company { get; set; }
        /// <summary>
        /// 物流详情单号 Tracking NO
        /// </summary>
        [Required]
        [StringLength(64)]
        public string BillNumber { get; set; }
        /// <summary>
        /// 客户唯一单号(TPV shipment id)
        /// </summary>
        [Required]
        [StringLength(64)]
        public string CustOrderID { get; set; }
        /// <summary>
        /// 司機(中郵倉庫出去)專車為必輸
        /// </summary>
        public string Driver { get; set; }
        /// <summary>
        /// 車號(中郵倉庫出去)專車為必輸
        /// </summary>
        public string CarNubmer { get; set; }
        /// <summary>
        /// 司機連絡電話(中郵倉庫出去)專車為必輸
        /// </summary>
        public string DriverPhone { get; set; }
        /// <summary>
        /// 物流状态
        /// </summary>
        [Required]
        public int Status { get; set; }
        /// <summary>
        /// ETD預計離開(起點)
        /// </summary>
        [Required]
        public DateTime ETD { get; set; }
        /// <summary>
        /// ETA預計到達(最終點)
        /// </summary>
        [Required]
        public DateTime ETA { get; set; }
        /// <summary>
        /// 處理地點
        /// </summary>
        [StringLength(10)]
        public string Location { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        [Required]
        public DateTime ProcTime { get; set; }
        /// <summary>
        /// 异常代码
        /// </summary>
        [StringLength(64)]
        public string ExceptionCode { get; set; }
        /// <summary>
        /// 异常件数
        /// </summary>
        [StringLength(64)]
        public string ExceptionQTY { get; set; }
    }
}

namespace Models.EDI
{
    /// <summary>
    /// 货况信息
    /// </summary>
    public class TraceInfo
    {
        /// <summary>
        /// 货况单号
        /// </summary>
        public string RefNO { get; set; }
        /// <summary>
        /// 货况代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 货况描述
        /// </summary>
        public string Descp { get; set; }
        /// <summary>
        /// 货况时间
        /// </summary>
        public DateTime EventDate { get; set; }
        /// <summary>
        /// 港口代码
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 港口名称
        /// </summary>
        public string LocationName { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }

    public class OceanTraceInfo : TraceInfoBase
    {
        public OceanRoutingInfo RoutingInfo { get; set; }
    }

    public class AirTraceInfo : TraceInfoBase
    {
        public AirRoutingInfo RoutingInfo { get; set; }
    }

    public class TraceInfoBase
    {
        public string Sender { get; set; }
        public string GroupID { get; set; }
        public string Cmp { get; set; }
        public string STN { get; set; }
        public string MsgCode { get; set; }
        public string ShipType { get; set; }
        public string MasterNO { get; set; }
        public string HouseNO { get; set; }
        public string ShipmentID { get; set; }
    }

    public class AirRoutingInfo : RoutingInfoBase
    {
        public string FlightNO { get; set; }
        public string FlightDate { get; set; }
        public AirEventInfo[] EventInfos { get; set; }
    }

    public class RoutingInfoBase
    {
        public string OriginCode { get; set; }
        public string Origin { get; set; }
        public string DestinationCode { get; set; }
        public string Destination { get; set; }
        public string ETD { get; set; }
        public string ETA { get; set; }
        public string ATD { get; set; }
        public string ATA { get; set; }
    }

    public class OceanRoutingInfo : RoutingInfoBase
    {
        public string ContainerNO { get; set; }
        public string VesselNO { get; set; }
        public string Voyage { get; set; }
        public OceanEventInfo[] EventInfos { get; set; }
    }
    public class AirEventInfo : EventInfoBase
    {
        public string FlightNO { get; set; }
        public string FlightDate { get; set; }
    }
    public class OceanEventInfo : EventInfoBase
    {
        public string ConveyanceName { get; set; }
        public string VoyageTripNO { get; set; }
        public string CarrierSCAC { get; set; }
    }

    public class EventInfoBase
    {
        public string EventCode { get; set; }
        public string EventDescription { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string EventTime { get; set; }
    }
}
