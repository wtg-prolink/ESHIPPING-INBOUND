using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.CPL
{
    class BillingInfo
    {

    }

    /// <summary>
    /// 料件请款信息
    /// </summary>
    public class MaterialsBillingInfo
    {

    }

    /// <summary>
    /// 成品请款信息
    /// </summary>
    public class FinishedBillingInfo 
    {
        /// <summary>
        /// 关链单号
        /// </summary>
        public string RefNO { get; set; }
        /// <summary>
        /// 出货日期
        /// </summary>
        public string ShippingDate { get; set; }
        /// <summary>
        /// 目的地城市
        /// </summary>
        public string DestinationCity { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string ShippingAddress { get; set; }
        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }
        /// <summary>
        /// 货物名称
        /// </summary>
        public string CommodityName { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public string QTY { get; set; }
        /// <summary>
        /// 装柜量
        /// </summary>
        public string ContainerQTY { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public string UnitPrice { get; set; }
        /// <summary>
        /// 总额
        /// </summary>
        public string TotalAmount { get; set; }
        /// <summary>
        /// 币别
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 费用明细
        /// </summary>
        public ChargeItem[] ChargeItems { get; set; }
    }

    /// <summary>
    /// 费用项目
    /// </summary>
    public class ChargeItem
    {
        /// <summary>
        /// 费用代码
        /// </summary>
        public string ChargeCode { get; set; }
        /// <summary>
        /// 费用名称
        /// </summary>
        public string ChargeName { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}
