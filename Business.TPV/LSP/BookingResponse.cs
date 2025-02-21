using Business.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.LSP
{
    public class BookingResponse : ImprotInfo
    {
        /// <summary>
        /// 運輸別
        /// </summary>
        [StringLength(1)]
        [Required]
        public string ShipType { get; set; }
        /// <summary>
        /// 船公司代碼
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

        [RequiredIf("Mode", "C")]
        [StringLength(300)]
        public string Remark { get; set; }

        /// <summary>
        /// 預計出發
        /// </summary>
        [Required]
        public DateTime? ETD { get; set; }
        /// <summary>
        /// 預計抵達
        /// </summary>
        [Required]
        public DateTime? ETA { get; set; }
    }
    enum BookingResponseModes { Add, Modify, Cancel }

    public class AirBookingResponse : BookingResponse
    {
        /// <summary>
        /// 飛機啟運點
        /// </summary>
        [Required]
        [StringLength(5)]
        public string Original { get; set; }
        /// <summary>
        /// 飛機目的地
        /// </summary>
        [Required]
        [StringLength(5)]
        public string DEST { get; set; }
        /// <summary>
        /// 最終目的地
        /// </summary>
        [Required]
        [StringLength(5)]
        public string LastDEST { get; set; }
        /// <summary>
        /// 飛機班次(第一段)
        /// </summary>
        [Required]
        [StringLength(30)]
        public string Flight1 { get; set; }
        /// <summary>
        /// 飛機班次(第二段)
        /// </summary>
        public string Flight2 { get; set; }
        /// <summary>
        /// 飛機班次(第三段)
        /// </summary>
        public string Flight3 { get; set; }
    }

    public class OceanBookingResponse : BookingResponse
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
        [StringLength(5)]
        public string Port { get; set; }
        /// <summary>
        /// 截进港日期時間
        /// </summary>
        public DateTime PortClose1 { get; set; }
        /// <summary>
        /// 码头截进港日期時間
        /// </summary>
        public DateTime PortClose2 { get; set; }
        /// <summary>
        /// 海关截投单日期時間
        /// </summary>
        public DateTime DeclClose { get; set; }
        /// <summary>
        /// 码头截放行日期時間
        /// </summary>
        public DateTime ReleaseClose { get; set; }
        /// <summary>
        /// 截海外申報日期時間
        /// </summary>
        public DateTime AMSClose { get; set; }
        /// <summary>
        /// 放箱日期時間
        /// </summary>
        public DateTime CntrRelease { get; set; }
        /// <summary>
        /// 簽單日期時間
        /// </summary>
        public DateTime DeliveryNote { get; set; }
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
        public DateTime VesselETD1 { get; set; }
        /// <summary>
        /// 預計抵達(第一段)
        /// </summary>
        public DateTime VesselETA1 { get; set; }

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
        public DateTime VesselETD2 { get; set; }
        /// <summary>
        /// 預計抵達(第二段)
        /// </summary>
        public DateTime VesselETA2 { get; set; }

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
        public DateTime VesselETD3 { get; set; }
        /// <summary>
        /// 預計抵達(第三段)
        /// </summary>
        public DateTime VesselETA3 { get; set; }

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
        public DateTime VesselETD4 { get; set; }
        /// <summary>
        /// 預計抵達(第四段)
        /// </summary>
        public DateTime VesselETA4 { get; set; }
    }
}