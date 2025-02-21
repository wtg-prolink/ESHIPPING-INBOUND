using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Business.TPV.DHL
{
    public class ShipmentTemplate : Template
    {
        #region edi
        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = new XmlEDINode("req:ShipmentRequest");
            //root.Attributes.Add(new XmlAttr("xmlns:req", "http://www.dhl.com"));
            //root.Attributes.Add(new XmlAttr("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"));

            root.Attributes.Add(new XmlAttr("xmlns:req", "http://www.dhl.com"));
            root.Attributes.Add(new XmlAttr("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"));
            root.Attributes.Add(new XmlAttr("xsi:schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.dhl.com ship-val-global-req-4.0.xsd"));
            root.Attributes.Add(new XmlAttr("schemaVersion", "4.0"));

            SetRequst(root);
            XmlEDINode node = new XmlEDINode("RegionCode", RegionCode.ToString());
            root.ChildNodes.Add(node);
            node = new XmlEDINode("LanguageCode", "EN");
            root.ChildNodes.Add(node);
            node = new XmlEDINode("PiecesEnabled", "Y");
            root.ChildNodes.Add(node);
            SetBilling(root);
            SetConsignee(root);
            SetCommodity(root);
            SetDutiable(root);
            SetExportDeclaration(root);
            SetReference(root);
            SetShipmentDetailsInfo(root);
            SetShipper(root);
            SetSpecialService(root);
            SetLabelImage(root);
            return root;
        }
        void SetBilling(XmlEDINode root)
        {
            Business.EDI.XmlEDINode billingNode = new Business.EDI.XmlEDINode("Billing");
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("ShipperAccountNumber");
            node.Value = BillingInfo.ShipperAccountNumber;
            billingNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("ShippingPaymentType");
            node.Value = BillingInfo.GetPaymentCode(BillingInfo.PaymentType);
            billingNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("BillingAccountNumber");
            node.Value = BillingInfo.BillNumber;
            billingNode.ChildNodes.Add(node);
            if (BillingInfo.DutyPaymentType.HasValue)
            {
                if (string.IsNullOrEmpty(BillingInfo.DutyAccountNumber))    //如果目的国税金有值，那么就是第三方付款
                {
                    node = new Business.EDI.XmlEDINode("DutyPaymentType", "T");
                    billingNode.ChildNodes.Add(node);
                    node = new Business.EDI.XmlEDINode("DutyAccountNumber");
                    node.Value = BillingInfo.DutyAccountNumber;
                    billingNode.ChildNodes.Add(node);
                }
                else
                {
                    node = new Business.EDI.XmlEDINode("DutyPaymentType", BillingInfo.GetPaymentCode(BillingInfo.DutyPaymentType.Value));
                    billingNode.ChildNodes.Add(node);
                }
                //node = new Business.EDI.XmlEDINode("DutyAccountNumber");
                //node.Value = BillingInfo.DutyAccountNumber;
                //billingNode.ChildNodes.Add(node);
            }
            root.ChildNodes.Add(billingNode);
        }
        void SetConsignee(XmlEDINode root)
        {
            Business.EDI.XmlEDINode cneeNode = new Business.EDI.XmlEDINode("Consignee");
            SetParty(cneeNode, Consignee);
            root.ChildNodes.Add(cneeNode);
        }
        void SetCommodity(XmlEDINode root)
        {
            if (CommodityInfos == null || CommodityInfos.Count <= 0) return;
            foreach (var item in CommodityInfos)
            {
                Business.EDI.XmlEDINode commodityNode = new Business.EDI.XmlEDINode("Commodity");
                Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("CommodityCode", item.Code);
                commodityNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("CommodityName", item.Description);
                commodityNode.ChildNodes.Add(node);
                root.ChildNodes.Add(commodityNode);
            }
        }
        void SetDutiable(XmlEDINode root)
        {
            if (DutiableInfo == null) return;
            Business.EDI.XmlEDINode dutiableNode = new Business.EDI.XmlEDINode("Dutiable");
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("DeclaredValue", DutiableInfo.Value.ToString("0.00"));
            dutiableNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("DeclaredCurrency", DutiableInfo.Currency);
            dutiableNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("ShipperEIN", DutiableInfo.ShipperEIN);
            dutiableNode.ChildNodes.Add(node);
            root.ChildNodes.Add(dutiableNode);
        }
        void SetExportDeclaration(XmlEDINode root)
        {
            if (ExportDeclarationInfo == null) return;
            Business.EDI.XmlEDINode exportDeclarationNode = new Business.EDI.XmlEDINode("ExportDeclaration");
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("InterConsignee", ExportDeclarationInfo.InterConsignee);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("IsPartiesRelation", ExportDeclarationInfo.IsPartiesRelation);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("ECCN", ExportDeclarationInfo.ECCN);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new XmlEDINode("SignatureName", ExportDeclarationInfo.SignatureName);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("SignatureTitle", ExportDeclarationInfo.SignatureTitle);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("ExportReason", ExportDeclarationInfo.ExportReason);
            exportDeclarationNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("ExportReasonCode", ExportDeclarationInfo.ExportReasonCode);
            exportDeclarationNode.ChildNodes.Add(node);
            foreach (var item in ExportDeclarationInfo.ExportLineItems)
            {
                XmlEDINode lineItemNode = new Business.EDI.XmlEDINode("ExportLineItem");
                node = new Business.EDI.XmlEDINode("Quantity", item.Quantity);
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("QuantityUnit", item.QuantityUnit);
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("Description", item.Description);
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("Value", item.Value.ToString());
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("IsDomestic", item.IsDomestic);
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("CommodityCode", item.CommodityCode);
                lineItemNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("ECCN", item.ECCN);
                XmlEDINode weightNode = new Business.EDI.XmlEDINode("Weight");
                node = new Business.EDI.XmlEDINode("Weight", item.Weight);
                weightNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("WeightUnit", item.WeightUnit);
                weightNode.ChildNodes.Add(node);
                lineItemNode.ChildNodes.Add(weightNode);
                exportDeclarationNode.ChildNodes.Add(lineItemNode);
            }
            root.ChildNodes.Add(exportDeclarationNode);
        }
        void SetReference(XmlEDINode root)
        {
            foreach (var item in ReferenceInfos)
            {
                Business.EDI.XmlEDINode refNode = new Business.EDI.XmlEDINode("Reference");
                Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("ReferenceID", item.ReferenceNO);
                refNode.ChildNodes.Add(node);
                if (!string.IsNullOrEmpty(item.ReferenceType))
                {
                    node = new Business.EDI.XmlEDINode("ReferenceType", item.ReferenceType);
                    refNode.ChildNodes.Add(node);
                }
                root.ChildNodes.Add(refNode);
            }
        }
        void SetShipmentDetailsInfo(XmlEDINode root)
        {
            Business.EDI.XmlEDINode shipmentDetailsNode = new Business.EDI.XmlEDINode("ShipmentDetails");
            Business.EDI.XmlEDINode numberNode = new Business.EDI.XmlEDINode("NumberOfPieces", ShipmentDetailsInfo.TotalQTY.ToString());
            shipmentDetailsNode.ChildNodes.Add(numberNode);
            Business.EDI.XmlEDINode piecesNode = new Business.EDI.XmlEDINode("Pieces");
            foreach (var item in ShipmentDetailsInfo.Pieces)
            {
                XmlEDINode pieceNode = new Business.EDI.XmlEDINode("Piece");
                XmlEDINode node = new Business.EDI.XmlEDINode("PieceID", item.PieceID);
                pieceNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("PackageType", ShipmentDetailsInfo.GetPackageTypeCode());
                pieceNode.ChildNodes.Add(node);
                if (!string.IsNullOrEmpty(item.Weight))
                {
                    node = new Business.EDI.XmlEDINode("Weight", item.Weight);
                    pieceNode.ChildNodes.Add(node);
                }
                if (!string.IsNullOrEmpty(item.DimWeight))
                {
                    node = new Business.EDI.XmlEDINode("DimWeight", item.DimWeight);
                    pieceNode.ChildNodes.Add(node);
                }
                if (!string.IsNullOrEmpty(item.Width))
                {
                    node = new Business.EDI.XmlEDINode("Width", item.Width);
                    pieceNode.ChildNodes.Add(node);
                }
                if (!string.IsNullOrEmpty(item.Height))
                {
                    node = new Business.EDI.XmlEDINode("Height", item.Height);
                    pieceNode.ChildNodes.Add(node);
                }
                if (!string.IsNullOrEmpty(item.Depth))
                {
                    node = new Business.EDI.XmlEDINode("Depth", item.Depth);
                    pieceNode.ChildNodes.Add(node);
                }
                if (item.ReferenceID != null && item.ReferenceID.Count > 0)
                {
                    node = new Business.EDI.XmlEDINode("PieceReference");
                    foreach (var v in item.ReferenceID)
                    {
                        XmlEDINode refNde = new Business.EDI.XmlEDINode("ReferenceID");
                        node.ChildNodes.Add(refNde);
                    }
                    pieceNode.ChildNodes.Add(node);
                }
                piecesNode.ChildNodes.Add(pieceNode);
            }
            shipmentDetailsNode.ChildNodes.Add(piecesNode);
            XmlEDINode sNode = new Business.EDI.XmlEDINode("Weight", ShipmentDetailsInfo.Weight);
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new XmlEDINode("WeightUnit", ShipmentDetailsInfo.WeightUnit.ToString());
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("GlobalProductCode", ShipmentDetailsInfo.GetGlobalProductCode(ShipmentDetailsInfo.GlobalProductMode));
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("LocalProductCode", ShipmentDetailsInfo.GetGlobalProductCode(ShipmentDetailsInfo.LocalProductMode));
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("Date", ShipmentDetailsInfo.Date.ToString("yyyy-MM-dd"));
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("Contents", ShipmentDetailsInfo.Contents);
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("DoorTo", "DD");
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("DimensionUnit", ShipmentDetailsInfo.GetDimensionUnitCode(ShipmentDetailsInfo.DimensionUnit));
            shipmentDetailsNode.ChildNodes.Add(sNode);
            if (!string.IsNullOrEmpty(ShipmentDetailsInfo.InsuredAmount))
            {
                sNode = new Business.EDI.XmlEDINode("InsuredAmount", ShipmentDetailsInfo.InsuredAmount);
                shipmentDetailsNode.ChildNodes.Add(sNode);
            }
            sNode = new Business.EDI.XmlEDINode("PackageType", ShipmentDetailsInfo.GetPackageTypeCode());
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("IsDutiable", ShipmentDetailsInfo.IsDutiable ? "Y" : "N");
            shipmentDetailsNode.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("CurrencyCode", ShipmentDetailsInfo.CurrencyCode);
            shipmentDetailsNode.ChildNodes.Add(sNode);
            root.ChildNodes.Add(shipmentDetailsNode);
        }
        void SetShipper(XmlEDINode root)
        {
            Business.EDI.XmlEDINode shipperNode = new Business.EDI.XmlEDINode("Shipper");
            XmlEDINode node = new XmlEDINode("ShipperID", Certification.DefaultAccountNumber);
            shipperNode.ChildNodes.Add(node);
            SetParty(shipperNode, Shipper);
            root.ChildNodes.Add(shipperNode);
        }
        void SetSpecialService(XmlEDINode root)
        {
            foreach (var item in SpecialServiceInfos)
            {
                Business.EDI.XmlEDINode sNode = new Business.EDI.XmlEDINode("SpecialServiceType");
                Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("SpecialServiceType", item.SpecialServiceType);
                sNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("ChargeValue", item.ChargeValue);
                sNode.ChildNodes.Add(node);
                node = new Business.EDI.XmlEDINode("CurrencyCode", item.CurrencyCode);
                sNode.ChildNodes.Add(node);
                root.ChildNodes.Add(sNode);
            }
        }
        void SetLabelImage(XmlEDINode root)
        {
            if (!IsUseDHLStandardWaybill) return;
            Business.EDI.XmlEDINode sNode = new Business.EDI.XmlEDINode("LabelImageFormat", LabelImageFormat.ToString());
            root.ChildNodes.Add(sNode);
            sNode = new Business.EDI.XmlEDINode("Label");
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("LabelTemplate", LabelTemplateInfo.GetLabelTemplateCode());
            sNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("Logo", LabelTemplateInfo.Logo ? "Y" : "N");
            sNode.ChildNodes.Add(node);
            if (LabelTemplateInfo.CustomerLogo != null)
            {
                node = new Business.EDI.XmlEDINode("CustomerLogo");
                XmlEDINode cNode = new Business.EDI.XmlEDINode("CurrencyCode", LabelTemplateInfo.CustomerLogo.Bytes);
                node.ChildNodes.Add(cNode);
                cNode = new Business.EDI.XmlEDINode("LogoImageFormat", LabelTemplateInfo.CustomerLogo.LogoImageFormat.ToString());
                node.ChildNodes.Add(cNode);
                sNode.ChildNodes.Add(node);
            }
            if (!string.IsNullOrEmpty(LabelTemplateInfo.Resolution))
            {
                node = new Business.EDI.XmlEDINode("Resolution", LabelTemplateInfo.Resolution);
                sNode.ChildNodes.Add(node);
            }
            root.ChildNodes.Add(sNode);
        }

        void SetParty(XmlEDINode partyNode, PartyInfo info)
        {
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("CompanyName", info.CompanyName);
            partyNode.ChildNodes.Add(node);
            if (info.Address != null && info.Address.Count > 0)
            {
                foreach (var address in info.Address)
                {
                    node = new Business.EDI.XmlEDINode("AddressLine", address);
                    partyNode.ChildNodes.Add(node);
                }
            }
            node = new Business.EDI.XmlEDINode("City", info.City);
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("DivisionCode", info.DivisionCode);
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("PostalCode", info.PostalCode);
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("CountryCode", info.CountryCode);
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("CountryName", info.CountryName);
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("Contact");
            partyNode.ChildNodes.Add(node);
            SetCotact(node, info.ContactInfo);
        }
        void SetCotact(XmlEDINode partyNode, ContactInfo info)
        {
            Business.EDI.XmlEDINode node = new Business.EDI.XmlEDINode("PersonName");
            node.Value = info.Name;
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("PhoneNumber");
            node.Value = info.PhoneNumber;
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("PhoneExtension");
            node.Value = info.PhoneExtension;
            partyNode.ChildNodes.Add(node);
            node = new Business.EDI.XmlEDINode("FaxNumber");
            node.Value = info.FaxNumber;
            partyNode.ChildNodes.Add(node);
            if (!string.IsNullOrEmpty(info.Telex))
            {
                node = new Business.EDI.XmlEDINode("Telex", info.Telex);
                partyNode.ChildNodes.Add(node);
            }
            if (!string.IsNullOrEmpty(info.Email))
            {
                node = new Business.EDI.XmlEDINode("Email", info.Email);
                partyNode.ChildNodes.Add(node);
            }
            if (!string.IsNullOrEmpty(info.MobilePhoneNumber))
            {
                node = new Business.EDI.XmlEDINode("MobilePhoneNumber", info.MobilePhoneNumber);
                partyNode.ChildNodes.Add(node);
            }
        }
        #endregion

        public ShipmentTemplate()
        {
            ReferenceInfos = new List<ReferenceInfo>();
            SpecialServiceInfos = new List<SpecialServiceInfo>();
        }

        /// <summary>
        /// 发件区域
        /// </summary>
        public RegionCodes RegionCode { get; set; }

        /// <summary>
        /// 账单信息
        /// </summary>
        [Required]
        public BillingInfo BillingInfo { get; set; }
        /// <summary>
        /// 货品信息
        /// </summary>
        public List<CommodityInfo> CommodityInfos { get; set; }
        [Required(ErrorMessage = "Ship to的Party是必须的")]
        public PartyInfo Consignee { get; set; }
        [Required]
        public PartyInfo Shipper { get; set; }
        /// <summary>
        /// 纳綐信息
        /// </summary>
        [Required]
        public DutiableInfo DutiableInfo { get; set; }
        /// <summary>
        /// 出口申报单信息
        /// </summary>
        public ExportDeclarationInfo ExportDeclarationInfo { get; set; }
        /// <summary>
        /// 订单信息
        /// </summary>
        public List<ReferenceInfo> ReferenceInfos { get; private set; }

        /// <summary>
        /// 运单明细信息
        /// </summary>
        [Required]
        public ShipmentDetailsInfo ShipmentDetailsInfo { get; set; }
        /// <summary>
        /// 特别服务信息 （比如保险等）
        /// </summary>
        public List<SpecialServiceInfo> SpecialServiceInfos { get; private set; }

        /// <summary>
        /// 是否使用DHL标准运单
        /// </summary>
        public bool IsUseDHLStandardWaybill { get; set; }
        /// <summary>
        /// 服务器返回的运单文件格式
        /// </summary>
        public LabelImageFormats LabelImageFormat { get; set; }
        /// <summary>
        /// 服务器返回的运单文件格式明细
        /// </summary>
        public LabelTemplateInfo LabelTemplateInfo { get; set; }

        public override bool Check(out EntityValidationResult result)
        {
            base.Check(out result);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Consignee);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Consignee.ContactInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Shipper);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Shipper.ContactInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.DutiableInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.ShipmentDetailsInfo);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.BillingInfo);
            if (result.HasError) return false;
            if (CommodityInfos != null && CommodityInfos.Count > 0)
            {
                foreach (var item in CommodityInfos)
                {
                    if (string.IsNullOrEmpty(item.Code))
                    {
                        result = new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("HTS Code不可为空，请检查对应DN料号档中的HTS Code!") });
                        return false;
                    }
                }
            }
            return true;
        }
    }
    class PartyRequiredAttribute : RequiredAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult result = base.IsValid(value, validationContext);
            if (result != null && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                PartyInfo p = validationContext.ObjectInstance as PartyInfo;
                if (p != null)
                    result.ErrorMessage = string.Format("{0} {1}", p.PartyDisplay, result.ErrorMessage);
                else
                {
                    ContactInfo c = validationContext.ObjectInstance as ContactInfo;
                    if (c != null)
                        result.ErrorMessage = string.Format("{0} {1}", c.PartyDisplay, result.ErrorMessage);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 发件区域
    /// </summary>
    public enum RegionCodes { AP, EU, AM }
    public enum LabelImageFormats { PDF, ZPL2, EPL2 }
    public class LabelTemplateInfo
    {
        public LabelTemplates LabelTemplate { get; set; }
        /// <summary>
        /// 仅限热敏标签
        /// </summary>
        public bool Logo { get; set; }
        public CustomerLogo CustomerLogo { get; set; }
        public string Resolution { get; set; }

        internal string GetLabelTemplateCode()
        {
            switch (LabelTemplate)
            {
                case LabelTemplates.PDF_6X4_A4: return "6X4_A4_PDF";
                case LabelTemplates.PDF_8X4_A4: return "8X4_A4_PDF";
                case LabelTemplates.Thermal_6X4: return "6X4_thermal";
                case LabelTemplates.Thermal_8X4: return "8X4_thermal";
            }
            return null;
        }
    }
    /// <summary>
    /// 自定义Logo
    /// </summary>
    public class CustomerLogo
    {
        /// <summary>
        /// base64数据流
        /// </summary>
        public string Bytes { get; set; }
        /// <summary>
        /// 图片格式
        /// </summary>
        public LogoImageFormats LogoImageFormat { get; set; }
    }
    public enum LabelTemplates { PDF_8X4_A4, PDF_6X4_A4, Thermal_8X4, Thermal_6X4 }
    public enum LogoImageFormats { PNG, GIF, JPEG, JPG }


    /// <summary>
    /// 特别服务信息 （比如保险等）
    /// </summary>
    public class SpecialServiceInfo
    {
        /// <summary>
        /// 服务类别
        /// </summary>
        public string SpecialServiceType { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string ChargeValue { get; set; }
        /// <summary>
        /// 币别
        /// </summary>
        public string CurrencyCode { get; set; }
    }

    /// <summary>
    /// 运单明细信息
    /// </summary>
    public class ShipmentDetailsInfo
    {
        public ShipmentDetailsInfo()
        {
            Pieces = new List<PiecesInfo>();
        }

        public int TotalQTY { get; set; }

        public List<PiecesInfo> Pieces { get; private set; }

        /// <summary>
        /// 总毛重
        /// </summary>
        [Required]
        public string Weight { get; set; }
        /// <summary>
        /// 重量单位
        /// </summary>
        public WeightUnitCodes WeightUnit { get; set; }
        /// <summary>
        /// DHL产品代码
        /// </summary>
        public GlobalProductModes GlobalProductMode { get; set; }
        /// <summary>
        /// DHL产品代码
        /// </summary>
        public GlobalProductModes LocalProductMode { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 主要品名
        /// </summary>
        [Required(ErrorMessage = "主要品名(Commodity)不可为空")]
        public string Contents { get; set; }
        /// <summary>
        /// 尺寸单位
        /// </summary>
        public DimensionUnitModes DimensionUnit { get; set; }
        /// <summary>
        /// 保险价值(有保险则必填，等于申报价值)
        /// </summary>
        public string InsuredAmount { get; set; }
        /// <summary>
        /// 包装类型
        /// </summary>
        public PackageTypes PackageType { get; set; }
        /// <summary>
        /// 是否完税(包裹时为true，文件为flase)
        /// </summary>
        public bool IsDutiable { get; set; }
        /// <summary>
        /// 运费结算币种
        /// </summary>
        public string CurrencyCode { get; set; }

        internal static string GetGlobalProductCode(GlobalProductModes mode)
        {
            switch (mode)
            {
                case GlobalProductModes.Parcel: return "P";
                case GlobalProductModes.Document: return "D";
                case GlobalProductModes.Native: return "N";
            }
            return null;
        }
        internal static string GetDimensionUnitCode(DimensionUnitModes mode)
        {
            switch (mode)
            {
                case DimensionUnitModes.CM: return "C";
                case DimensionUnitModes.Inches: return "I";
            }
            return null;
        }
        internal string GetPackageTypeCode()
        {
            switch (PackageType)
            {
                case PackageTypes.DHLExpressEnvelope: return "EE";
                case PackageTypes.OtherDHLPackaging: return "OD";
                case PackageTypes.CustomerProvided: return "CP";
                case PackageTypes.JumboBox: return "JB";
                case PackageTypes.JuniorJumboBox: return "JJ";
                case PackageTypes.DHLFlyer: return "DF";
                case PackageTypes.YourPackaging: return "YP";
            }
            return null;
        }
    }

    public enum PackageTypes { DHLExpressEnvelope, OtherDHLPackaging, CustomerProvided, JumboBox, JuniorJumboBox, DHLFlyer, YourPackaging }

    public enum DimensionUnitModes
    {
        /// <summary>
        /// 厘米
        /// </summary>
        CM,
        /// <summary>
        /// 英寸
        /// </summary>
        Inches
    }
    /// <summary>
    /// DHL产品代码
    /// </summary>
    public enum GlobalProductModes
    {
        /// <summary>
        /// 包裹
        /// </summary>
        Parcel,
        /// <summary>
        /// 文件
        /// </summary>
        Document,
        /// <summary>
        /// 本地件
        /// </summary>
        Native
    }

    public class PiecesInfo
    {
        public PiecesInfo()
        {
            ReferenceID = new List<string>();
        }
        public string PieceID { get; set; }
        public string PackageType { get; set; }
        public string Weight { get; set; }
        public string DimWeight { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
        public List<string> ReferenceID { get; private set; }
    }

    /// <summary>
    /// 订单信息
    /// </summary>
    public class ReferenceInfo
    {
        /// <summary>
        /// 订单号码
        /// </summary>
        public string ReferenceNO { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public string ReferenceType { get; set; }
    }

    /// <summary>
    /// 账单信息
    /// </summary>
    public class BillingInfo
    {
        public BillingInfo()
        {
            DutyPaymentType = PaymentTypes.Receiver;
        }
        /// <summary>
        /// 发件人账号
        /// </summary>
        [Required(ErrorMessage = "快递账号不可为空!")]
        public string ShipperAccountNumber { get; set; }
        /// <summary>
        /// 运费付款方
        /// </summary>
        public PaymentTypes PaymentType { get; set; }
        /// <summary>
        /// 付款账号（PaymentType为Receiver与ThirdParty时必输）
        /// </summary>
        public string BillNumber { get; set; }
        /// <summary>
        /// 税金付款方
        /// </summary>
        public PaymentTypes? DutyPaymentType { get; set; }
        /// <summary>
        /// 税金付款账号（DutyPaymentType为ThirdParty时必输）
        /// </summary>
        public string DutyAccountNumber
        {
            get;
            set;
        }

        internal string GetPaymentCode(PaymentTypes type)
        {
            switch (type)
            {
                case PaymentTypes.Shipper: return "S";
                case PaymentTypes.Receiver: return "R";
                case PaymentTypes.ThirdParty: return "T";
            }
            return null;
        }
    }
    public enum PaymentTypes
    {
        Shipper,
        Receiver,
        ThirdParty
    }
    /// <summary>
    /// 货品信息
    /// </summary>
    public class CommodityInfo
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    /// <summary>
    /// 纳綐信息
    /// </summary>
    public class DutiableInfo
    {
        /// <summary>
        /// 海关申报价值
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 币别
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 发件人增值税号
        /// </summary>
        public string ShipperEIN { get; set; }
    }
    /// <summary>
    /// 出口申报单信息
    /// </summary>
    public class ExportDeclarationInfo
    {
        public ExportDeclarationInfo()
        {
            ExportLineItems = new List<ExportLineInfo>();
        }
        public string InterConsignee { get; set; }
        public string IsPartiesRelation { get; set; }
        public string ECCN { get; set; }
        public string SignatureName { get; set; }
        public string SignatureTitle { get; set; }
        public string ExportReason { get; set; }
        public string ExportReasonCode { get; set; }
        public List<ExportLineInfo> ExportLineItems { get; private set; }
    }
    public class ExportLineInfo
    {
        public string LineNumber { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string Description { get; set; }
        public double? Value { get; set; }
        public string IsDomestic { get; set; }
        public string CommodityCode { get; set; }
        public string ECCN { get; set; }
        public string Weight { get; set; }
        public string WeightUnit { get; set; }
    }

    public class PartyInfo
    {
        [StringLength(35)]
        [PartyRequired]
        public string CompanyName { get; set; }
        public List<string> Address { get; set; }
        [PartyRequired]
        public string City { get; set; }
        public string DivisionCode { get; set; }
        public string PostalCode { get; set; }
        [PartyRequired(ErrorMessage = "国家代码不可为空!")]
        public string CountryCode { get; set; }
        [PartyRequired(ErrorMessage = "国家名称不可为空!")]
        public string CountryName { get; set; }
        public string FederalTaxId { get; set; }
        [PartyRequired(ErrorMessage = "联系人不可为空!")]
        public ContactInfo ContactInfo { get; set; }
        public string PartyDisplay { get; set; }
    }
    public class ContactInfo
    {        
        [PartyRequired(ErrorMessage = "联系人名称不可为空!")]
        public string Name { get; set; }
        [PartyRequired(ErrorMessage = "联系人电话不可为空!")]
        public string PhoneNumber { get; set; }
        public string PhoneExtension { get; set; }
        public string FaxNumber { get; set; }
        public string Telex { get; set; }
        public string Email { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string PartyDisplay { get; set; }
    }
}