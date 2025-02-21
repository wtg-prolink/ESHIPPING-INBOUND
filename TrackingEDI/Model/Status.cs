using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackingEDI.Model
{
    public class Status
    {
        /// <summary>
        /// 主单号
        /// </summary>
        public string MasterNo
        {
            get;
            set;
        }

        /// <summary>
        /// 提单号
        /// </summary>
        public string HouseNo
        {
            get;
            set;
        }

        /// <summary>
        /// Shipment Id
        /// </summary>
        public string ShipmentId
        {
            get;
            set;
        }

        /// <summary>
        /// 提单主键
        /// </summary>
        public string JobNo
        {
            get;
            set;
        }

        /// <summary>
        /// 货况代码
        /// </summary>
        public string StsCd
        {
            get;
            set;
        }

        /// <summary>
        /// 货况描述   可为空
        /// </summary>
        public string StsDescp
        {
            get;
            set;
        }

        /// <summary>
        /// location 发生位置
        /// </summary>
        public string Location
        {
            get;
            set;
        }

        public string LocationName
        {
            get;
            set;
        }

        /// <summary>
        /// 货况产生时间 格式 yyyyMMddHHmmss
        /// </summary>
        public string EventTime
        {
            get;
            set;
        }

        /// <summary>
        /// 货柜号码
        /// </summary>
        public string ContainerNo
        {
            get;
            set;
        }

        /// <summary>
        /// 创建者
        /// </summary>
        public string Sender
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;
            set;
        }

        /// <summary>
        /// Company
        /// </summary>
        public string Cmp
        {
            get;
            set;
        }
    }
}
