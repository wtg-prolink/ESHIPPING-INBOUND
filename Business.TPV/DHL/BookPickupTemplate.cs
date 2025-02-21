using Business.Attribute;
using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.DHL
{
    public class BookPickupTemplate : Template
    {
        #region edi
        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = new XmlEDINode("req:BookPickupRequestAP");
            root.Attributes.Add(new XmlAttr("xmlns:req", "http://www.dhl.com"));
            root.Attributes.Add(new XmlAttr("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"));
            root.Attributes.Add(new XmlAttr("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.dhl.com book-pickup-req.xsd"));
            //root.Attributes.Add(new XmlAttr("schemaVersion", "1.0"));
            SetRequst(root);
            if (RegionCode.HasValue)
            {
                XmlEDINode node = new XmlEDINode("RegionCode", RegionCode.Value.ToString());
                root.ChildNodes.Add(node);
            }
            SetRequestor(root);
            SetPlace(root);
            SetPickup(root);
            SetPickupContact(root);
            SetShipmentDetailsInfo(root);
            return root;
        }
        void SetRequestor(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("Requestor");
            XmlEDINode node = new XmlEDINode("AccountType", "D");
            mNode.ChildNodes.Add(node);
            string account = string.IsNullOrEmpty(RequestorInfo.AccountNumber) ? Certification.DefaultAccountNumber : RequestorInfo.AccountNumber;
            node = new XmlEDINode("AccountNumber", account);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("RequestorContact");
            SetContaceInfo(node, RequestorInfo);
            mNode.ChildNodes.Add(node);
            root.ChildNodes.Add(mNode);
        }
        void SetPlace(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("Place");
            XmlEDINode node = new XmlEDINode("LocationType", PickupInfo.GetLocationTypeCode());
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CompanyName", PickupInfo.CompanyName);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("Address1", PickupInfo.Address1);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("Address2", PickupInfo.Address2);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("PackageLocation", PickupInfo.PackageLocation);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("City", PickupInfo.City);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("StateCode", PickupInfo.StateCode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("DivisionName", PickupInfo.DivisionName);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CountryCode", PickupInfo.CountryCode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("PostalCode", PickupInfo.PostalCode);
            mNode.ChildNodes.Add(node);
            root.ChildNodes.Add(mNode);
        }
        void SetPickup(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("Pickup");
            XmlEDINode node = new XmlEDINode("PickupDate", PickupInfo.PickupDate.Value.ToString("yyyy-MM-dd"));
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("ReadyByTime", PickupInfo.PickupDate.Value.ToString("HH:MM"));
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CloseTime", PickupInfo.CloseTime.Value.ToString("HH:MM"));
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("Pieces", PickupInfo.Pieces.ToString());
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("weight");
            XmlEDINode nNode = new XmlEDINode("Weight", PickupInfo.Weight);
            node.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("WeightUnit", PickupInfo.WeightUnit.ToString());
            node.ChildNodes.Add(nNode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("SpecialInstructions", PickupInfo.SpecialInstructions);
            mNode.ChildNodes.Add(node);
            root.ChildNodes.Add(mNode);
        }
        void SetPickupContact(XmlEDINode root)
        {
            XmlEDINode node = new XmlEDINode("PickupContact");
            SetContaceInfo(node, PickupContactInfo);
            root.ChildNodes.Add(node);
        }
        void SetShipmentDetailsInfo(XmlEDINode root)
        {
            Business.EDI.XmlEDINode shipmentDetailsNode = new Business.EDI.XmlEDINode("ShipmentDetails");
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("AccountType", "D");
            shipmentDetailsNode.ChildNodes.Add(node);
            string account = string.IsNullOrEmpty(PickupShipmentDetailsInfo.AccountNumber) ? Certification.DefaultAccountNumber : PickupShipmentDetailsInfo.AccountNumber;
            node = new XmlEDINode("AccountNumber", account);
            shipmentDetailsNode.ChildNodes.Add(node);
            string billAccount = string.IsNullOrEmpty(PickupShipmentDetailsInfo.BillAccountNumber) ? Certification.DefaultAccountNumber : PickupShipmentDetailsInfo.BillAccountNumber;
            node = new XmlEDINode("BillToAccountNumber", billAccount);
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("AWBNumber", PickupShipmentDetailsInfo.AWBNumber);
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("NumberOfPieces", PickupShipmentDetailsInfo.NumberOfPieces.ToString());
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("Weight", PickupShipmentDetailsInfo.Weight);
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("WeightUnit", PickupShipmentDetailsInfo.WeightUnit.ToString());
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("GlobalProductCode", ShipmentDetailsInfo.GetGlobalProductCode(PickupShipmentDetailsInfo.GlobalProductMode));
            shipmentDetailsNode.ChildNodes.Add(node);
            node = new XmlEDINode("DoorTo", "DD");
            shipmentDetailsNode.ChildNodes.Add(node);
            if (PickupShipmentDetailsInfo.DimensionUnit.HasValue)
            {
                node = new XmlEDINode("DimensionUnit", ShipmentDetailsInfo.GetDimensionUnitCode(PickupShipmentDetailsInfo.DimensionUnit.Value));
                shipmentDetailsNode.ChildNodes.Add(node);
            }
            if (PickupShipmentDetailsInfo.InsuredAmount > 0)
            {
                node = new XmlEDINode("InsuredAmount", PickupShipmentDetailsInfo.InsuredAmount.ToString());
                shipmentDetailsNode.ChildNodes.Add(node);
                node = new XmlEDINode("InsuredCurrencyCode", PickupShipmentDetailsInfo.InsuredCurrencyCode);
                shipmentDetailsNode.ChildNodes.Add(node);
            }
            root.ChildNodes.Add(shipmentDetailsNode);
        }

        void SetContaceInfo(XmlEDINode pNode, PickupContactInfo info)
        {
            XmlEDINode nNode = new XmlEDINode("PersonName", RequestorInfo.Name);
            pNode.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("Phone", RequestorInfo.Phone);
            pNode.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("PhoneExtension", RequestorInfo.PhoneExtension);
            pNode.ChildNodes.Add(nNode);
        }
        #endregion
        /// <summary>
        /// 取件明细信息
        /// </summary>
        [Required]
        public PickupShipmentDetailsInfo PickupShipmentDetailsInfo { get; set; }
        /// <summary>
        /// 取件信息
        /// </summary>  
        [Required]
        public PickupInfo PickupInfo { get; set; }
        /// <summary>
        /// 发件区域
        /// </summary>
        public RegionCodes? RegionCode { get; set; }
        /// <summary>
        /// 发送请求人信息
        /// </summary>
        public RequestorInfo RequestorInfo { get; set; }
        /// <summary>
        /// 取件联系人信息
        /// </summary>
        public PickupContactInfo PickupContactInfo { get; set; }

        public BookPickupTemplate()
        {

        }

        public override bool Check(out EntityValidationResult result)
        {
            base.Check(out result);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.RequestorInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.PickupShipmentDetailsInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.PickupContactInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.PickupInfo);
            if (result.HasError) return false;
            return true;
        }
    }
    /// <summary>
    /// 地点类型
    /// </summary>
    public enum LocationTypes { Business, Residence, BusinessOrResidence }
    /// <summary>
    /// 取件信息
    /// </summary>
    public class PickupInfo
    {
        /// <summary>
        /// 地点类型
        /// </summary>
        public LocationTypes LocationType { get; set; }
        /// <summary>
        /// 公司名称(如果发件人是个人，写联系人名称)
        /// </summary>
        [Required]
        public string CompanyName { get; set; }
        /// <summary>
        /// 取件地址1
        /// </summary>
        [Required]
        public string Address1 { get; set; }
        /// <summary>
        /// 取件地址2
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 具体取件地点(例如：office, frontdesk)
        /// </summary>
        public string PackageLocation { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        public string City { get; set; }
        /// <summary>
        /// 州
        /// </summary>
        [Required]
        public string StateCode { get; set; }
        /// <summary>
        /// 州名
        /// </summary>
        [RequiredIf("CountryCode", "US")]
        public string DivisionName { get; set; }
        /// <summary>
        /// 国家代码
        /// </summary>
        [Required]
        [StringLength(2)]
        public string CountryCode { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// 取件时间
        /// </summary>
        [Required]
        public DateTime? PickupDate { get; set; }
        /// <summary>
        /// 最晚取件时间 (与PickupDate至少间隔2小时)
        /// </summary>
        [Required]
        public DateTime? CloseTime { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public decimal Pieces { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        [Required]
        public string Weight { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public WeightUnitCodes WeightUnit { get; set; }
        /// <summary>
        /// 特殊说明
        /// </summary>
        public string SpecialInstructions { get; set; }

        internal string GetLocationTypeCode()
        {
            switch (LocationType)
            {
                case LocationTypes.Business: return "B";
                case LocationTypes.BusinessOrResidence: return "C";
                case LocationTypes.Residence: return "R";
            }
            return null;
        }
    }
    /// <summary>
    /// 发件人请求信息
    /// </summary>
    public class RequestorInfo : PickupContactInfo
    {
        /// <summary>
        /// 发件人账号（DHL的账号,为空会自动带入默认)
        /// </summary>        
        public string AccountNumber { get; set; }
    }
    /// <summary>
    /// 取件联系人信息
    /// </summary>
    public class PickupContactInfo
    {
        /// <summary>
        /// 联系人
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [Required]
        public string Phone { get; set; }
        /// <summary>
        /// 分机
        /// </summary>
        public string PhoneExtension { get; set; }
    }
    /// <summary>
    /// 取件明细信息
    /// </summary>
    public class PickupShipmentDetailsInfo
    {
        /// <summary>
        /// 发件账号（为空会自动带入默认)
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// 付款账号（为空会自动带入默认)
        /// </summary>
        public string BillAccountNumber { get; set; }

        /// <summary>
        /// 运单号码
        /// </summary>
        [StringLength(10)]
        public string AWBNumber { get; set; }
        /// <summary>
        /// 件数
        /// </summary>
        public decimal NumberOfPieces { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        [Required]
        public string Weight { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public WeightUnitCodes WeightUnit { get; set; }
        /// <summary>
        /// DHL产品类型
        /// </summary>
        public GlobalProductModes GlobalProductMode { get; set; }
        /// <summary>
        /// 尺寸单位
        /// </summary>
        public DimensionUnitModes? DimensionUnit { get; set; }
        /// <summary>
        /// 保险价值
        /// </summary>
        public double InsuredAmount { get; set; }
        /// <summary>
        /// 保险价值货币
        /// </summary>
        [StringLength(3)]
        public string InsuredCurrencyCode { get; set; }
        /// <summary>
        /// 特殊服务类型
        /// </summary>
        [StringLength(1)]
        public string SpecialService { get; set; }
    }

    public enum WeightUnitCodes
    {
        /// <summary>
        /// 千克
        /// </summary>
        K,
        /// <summary>
        /// 磅
        /// </summary>
        L
    }
}