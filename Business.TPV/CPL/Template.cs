using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.CPL
{
    class LogisticsWaybillAddTemplate : Business.EDI.XmlEDITemplateBase
    {
        #region edi
        public override Business.EDI.XmlEDINode CreateXmlEDINode()
        {
            Business.EDI.XmlEDINode root = new Business.EDI.XmlEDINode("logistics_waybill_add_request");
            root.Attributes.Add(new Business.EDI.XmlAttr("xmlns", "http://www.fjcpl.com"));
            Business.EDI.XmlEDINode waybill = new Business.EDI.XmlEDINode("waybill");
            root.ChildNodes.Add(waybill);
            SetWaybillInfos(waybill);
            SetPartyInfo(waybill);
            SetProperties(waybill);
            return root;
        }
        void SetWaybillInfos(Business.EDI.XmlEDINode waybill)
        {
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("company");
            node.Value = Company;
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("bill_no", BLNO);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("cust_order_id", CustomerOrderID);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("cust_name", CustomerName);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("business_type", "0");
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("product_info", ProductInfo);
            waybill.ChildNodes.Add(node);            
            node = new Business.EDI.XmlEDINode("product_type", ProductType);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("product_code", ProductCode);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("qty", QTY.ToString());
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("volume", Volume.ToString());
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("weight", Weight.ToString());
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("load_qty", LoadQTY.ToString());
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("transport_mode", TransportMode);
            waybill.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("delivery_claim", DeliveryClaim);
            waybill.ChildNodes.Add(node);
        }
        void SetProperties(Business.EDI.XmlEDINode waybillNode)
        {
            if (Properties == null) return;
            foreach (Properties pp in Properties)
            {
                Business.EDI.XmlEDINode propertiesNode = new Business.EDI.XmlEDINode("properties");
                Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("property", pp.ProductInfoEn);
                node.Attributes.Add(new Business.EDI.XmlAttr("key", "product_info_en"));
                propertiesNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("property", pp.ProductTypeEx);
                node.Attributes.Add(new Business.EDI.XmlAttr("key", "product_type_ex"));
                propertiesNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("property", pp.DnNo);
                node.Attributes.Add(new Business.EDI.XmlAttr("key", "dn_no"));
                propertiesNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("property", pp.BandType);
                node.Attributes.Add(new Business.EDI.XmlAttr("key", "band_type"));
                propertiesNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("property", pp.TotalPackage);
                node.Attributes.Add(new Business.EDI.XmlAttr("key", "total_package"));
                propertiesNode.ChildNodes.Add(node);
                waybillNode.ChildNodes.Add(propertiesNode);
            }
        }
        void SetPartyInfo(Business.EDI.XmlEDINode waybillNode)
        {
            SetPartyInfo(waybillNode, PartyModes.sender);
            SetPartyInfo(waybillNode, PartyModes.receiver);
            SetPartyInfo(waybillNode, PartyModes.collect);
        }
        enum PartyModes { sender, receiver, collect }
        void SetPartyInfo(Business.EDI.XmlEDINode waybillNode, PartyModes mode)
        {
            PartyInfo partyInfo = null;
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode(mode.ToString());
            switch (mode)
            {
                case PartyModes.sender: partyInfo = Sender; break;
                case PartyModes.receiver: partyInfo = Receiver; break;
                case PartyModes.collect: partyInfo = Collect; break;
            }
            if (partyInfo == null) return;
            Business.EDI.XmlEDINode childnode = new Business.EDI.XmlEDINode("name", partyInfo.Name);
            node.ChildNodes.Add(childnode);
            childnode = new Business.EDI.XmlEDINode("company", partyInfo.Company);
            node.ChildNodes.Add(childnode);
            childnode = new Business.EDI.XmlEDINode("phone", partyInfo.Phone);
            node.ChildNodes.Add(childnode);
            childnode = new Business.EDI.XmlEDINode("mobile", partyInfo.Mobile);
            node.ChildNodes.Add(childnode);
            if (partyInfo.Address != null)
            {
                childnode = new Business.EDI.XmlEDINode("address");
                childnode.Value = partyInfo.Address.Value;
                childnode.Attributes.Add(new Business.EDI.XmlAttr("province", partyInfo.Address.Province));
                childnode.Attributes.Add(new Business.EDI.XmlAttr("city", partyInfo.Address.City));
                childnode.Attributes.Add(new Business.EDI.XmlAttr("county", null));
                childnode.Attributes.Add(new Business.EDI.XmlAttr("country", partyInfo.Address.Country));
                childnode.Attributes.Add(new Business.EDI.XmlAttr("zip", partyInfo.Address.Zip));
                node.ChildNodes.Add(childnode);
            }
            waybillNode.ChildNodes.Add(node);
        }
        #endregion

        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 物流详情单号
        /// </summary>
        public string BLNO { get; set; }
        /// <summary>
        /// 客户唯一单号
        /// </summary>
        public string CustomerOrderID { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 产品信息品名
        /// </summary>
        public string ProductInfo { get; set; }
        /// <summary>
        /// 产品类型、机种名
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 产品代码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public int QTY { get; set; }
        /// <summary>
        /// 体积
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// 装柜量
        /// </summary>
        public int LoadQTY { get; set; }
        /// <summary>
        /// 运输模式
        /// </summary>
        public string TransportMode { get; set; }
        /// <summary>
        /// 車型
        /// </summary>
        public string CarType { get; set; }
        /// <summary>
        /// 配送、投递要求
        /// </summary>
        public string DeliveryClaim { get; set; }
        /// <summary>
        /// 始发地
        /// </summary>
        public PartyInfo Sender { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public PartyInfo Receiver { get; set; }
        /// <summary>
        /// 揽收地信息
        /// </summary>
        public PartyInfo Collect { get; set; }
        /// <summary>
        /// dn信息
        /// </summary>
        public List<Properties> Properties { get; set; }
    }

    public class PartyInfo
    {
        public string Company { get; set; }
        /// <summary>
        /// 各称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public Address Address { get; set; }
    }

    public class Properties
    {
        /// <summary>
        /// 英文品名
        /// </summary>
        public string ProductInfoEn { get; set; }
        /// <summary>
        /// 对内机种名（扩展）
        /// </summary>
        public string ProductTypeEx { get; set; }
        /// <summary>
        /// DN NO
        /// </summary>
        public string DnNo { get; set; }
        /// <summary>
        /// 是否饶物流园，Y:是;N:否;Null:否
        /// </summary>
        public String BandType { get; set; }

        public string TotalPackage { get; set; }
    }

    public class Address
    {
        public string Value { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
    }

    public class ItemInventoryQueryTemplate : Business.EDI.XmlEDITemplateBase
    {
        public override EDI.XmlEDINode CreateXmlEDINode()
        {
            Business.EDI.XmlEDINode root = new Business.EDI.XmlEDINode("item_inventory_query_request");
            root.Attributes.Add(new Business.EDI.XmlAttr("xmlns", "http://www.fjcpl.com"));

            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("warehouse_code");
            node.Value = WarehouseCode;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("item_spec");
            node.Value = ItemSpec;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("item_spec_ex");
            node.Value = ItemSpecEx;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("item_status");
            node.Value = ItemStatus;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("page_index");
            node.Value = PageIndex;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("page_size");
            node.Value = PageSize;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("product_code");
            node.Value = ProductCode;
            root.ChildNodes.Add(node);
            return root;
        }
        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WarehouseCode { get; set; }
        /// <summary>
        /// 货品规格
        /// </summary>
        public string ItemSpec { get; set; } 
        /// <summary>
        /// 货品规格(附加)
        /// </summary>
        public string ItemSpecEx { get; set; }
        /// <summary>
        /// 货品状态
        /// </summary>
        public string ItemStatus { get; set; }
        /// <summary>
        /// 货品条码
        /// </summary>        
        public string ItemBarcode { get; set; }
        /// <summary>
        /// 数据页索引
        /// </summary>
        public string PageIndex { get; set; }
        /// <summary>
        /// 数据页大小
        /// </summary>
        public string PageSize { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductCode { get; set; }
    }

    public class LogisticsTracePush : Business.EDI.XmlEDITemplateBase
    {
        public override EDI.XmlEDINode CreateXmlEDINode()
        {
            Business.EDI.XmlEDINode root = new Business.EDI.XmlEDINode("logistics_trace_push_request");
            root.Attributes.Add(new Business.EDI.XmlAttr("xmlns", "http://www.fjcpl.com"));

            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("company");
            node.Value = Company;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("bill_no");
            node.Value = BLNO;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("status");
            node.Value = GetStatus().ToString();
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("proc_time");
            node.Value = ProcTime.ToString("YYYY-MM-DD HH:MM:SS");
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("content");
            node.Value = Content;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("exception_code");
            node.Value = ExceptionCode;
            root.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("exception_qty");
            node.Value = ExceptionQTY;
            root.ChildNodes.Add(node);
            return root;
        }

        int GetStatus()
        {
            switch (Status)
            {
                case LogisticStatus.TookAPosting: return 1;
                case LogisticStatus.BeingArranged: return 2;
                case LogisticStatus.SignIn: return 3;
                case LogisticStatus.BackToASingle: return 6;
                default: return 0;
            }
        }

        /// <summary>
        /// 物流公司代码
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 物流详情单号
        /// </summary>
        public string BLNO { get; set; }
        /// <summary>
        /// 物流详情单号
        /// </summary>
        public LogisticStatus Status { get; set; }
        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime ProcTime { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 异常代码
        /// </summary>
        public string ExceptionCode { get; set; }
        /// <summary>
        /// 异常件数
        /// </summary>
        public string ExceptionQTY { get; set; }
    }
    /// <summary>
    /// 物流状态
    /// </summary>
    public enum LogisticStatus
    {
        /// <summary>
        /// 未定义
        /// </summary>
        None,
        /// <summary>
        /// 揽件收寄
        /// </summary>
        TookAPosting,
        /// <summary>
        /// 派送中
        /// </summary>
        BeingArranged,
        /// <summary>
        /// 签收
        /// </summary>
        SignIn,
        /// <summary>
        /// 返单
        /// </summary>
        BackToASingle
    }

}