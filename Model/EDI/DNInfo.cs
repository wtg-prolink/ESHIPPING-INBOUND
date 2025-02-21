using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EDI
{
    public class DNInfo
    {
        /// <summary>
        /// DN主信息
        /// </summary>
        public HeaderInfo HeaderInfo { get; set; }

        public ItemInfo[] ItemInfos { get; set; }
        /// <summary>
        /// 伙伴信息
        /// </summary>
        public PartnerInfo[] PartnerInfos { get; set; }
        /// <summary>
        /// 价格信息
        /// </summary>
        public PriceInfo[] PriceInfos { get; set; }
        /// <summary>
        /// 分类信息
        /// </summary>
        public ClassificationInfo[] ClassificationInfos { get; set; }
        /// <summary>
        /// 包装信息（栈板信息）
        /// </summary>
        public PalletInfo[] PalletInfos { get; set; }

        public string User { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// DN主信息
    /// </summary>
    public class HeaderInfo
    {
        /// <summary>
        /// Material Type
        /// </summary>
        public string MaterialType { get; set; }
        /// <summary>
        /// Company for Original Sales Org
        /// </summary>
        public string OriginalCompany { get; set; }
        public string DeclarationDate { get; set; }
        /// <summary>
        /// Source SAP
        /// </summary>
        public string SourceSAP { get; set; }
        /// <summary>
        /// Outbound delivery no.
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// Outbound delivery no. with company code
        /// </summary>
        public string DNNOWithCompanyCode { get; set; }
        /// <summary>
        /// company code for DN
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// Source SAP for reference DN
        /// </summary>
        public string RefSourceSAP { get; set; }
        /// <summary>
        /// company code for Reference DN
        /// </summary>
        public string RefCompanyCode { get; set; }
        /// <summary>
        /// Reference Outbound delivery no.
        /// </summary>
        public string RefDNNO { get; set; }
        /// <summary>
        /// Reference DN with company code
        /// </summary>
        public string RefDNNOWithCompanyCode { get; set; }
        /// <summary>
        /// Sales Document Type
        /// </summary>
        public string SalesDocumentType { get; set; }
        /// <summary>
        /// Order reason (reason for the business transaction)
        /// </summary>
        public string OrderReason { get; set; }
        /// <summary>
        ///  Net Value of the Sales Order in Document Currency
        /// </summary>
        public string SalesOrderNetValue { get; set; }
        /// <summary>
        /// SD Document Currency
        /// </summary>
        public string SDCurrency { get; set; }
        /// <summary>
        /// Sales Organization
        /// </summary>
        public string SalesOrganization { get; set; }
        /// <summary>
        /// Distribution Channel Description
        /// </summary>
        public string DistributionChannelDescription { get; set; }
        /// <summary>
        /// Business area from cost center
        /// </summary>
        public string BusinessAreaFromCostCenter { get; set; }
        /// <summary>
        /// Valid-from date (outline agreements, product proposals)
        /// </summary>
        public string ValidFromDate { get; set; }
        /// <summary>
        /// Valid-to date (outline agreements, product proposals)
        /// </summary>
        public string ValidToDate { get; set; }
        /// <summary>
        /// Number of the document condition
        /// </summary>
        public string NumberOfTheDocumentCondition { get; set; }
        /// <summary>
        /// Requested delivery date
        /// </summary>
        public string RequestedDeliveryDate { get; set; }
        /// <summary>
        /// Shipping Conditions
        /// </summary>
        public string ShippingConditions { get; set; }
        /// <summary>
        /// Shipping Point/Receiving Point
        /// </summary>
        public string ShippingPoint { get; set; }
        /// <summary>
        /// Shipping Point/Receiving Point Description
        /// </summary>
        public string ShippingPointDescription { get; set; }
        /// <summary>
        /// Delivery Type
        /// </summary>
        public string DeliveryType { get; set; }
        /// <summary>
        /// Planned goods movement date
        /// </summary>
        public string PlannedGoodsMovementDate { get; set; }
        /// <summary>
        /// Loading Date
        /// </summary>
        public string LoadingDate { get; set; }
        /// <summary>
        /// Transportation Planning Date
        /// </summary>
        public string TransportationPlanningDate { get; set; }
        /// <summary>
        /// Delivery Date
        /// </summary>
        public string DeliveryDate { get; set; }
        /// <summary>
        /// Picking Date
        /// </summary>
        public string PickingDate { get; set; }
        /// <summary>
        /// Unloading Point
        /// </summary>
        public string UnloadingPoint { get; set; }
        /// <summary>
        /// 卸货港名称
        /// </summary>
        public string UnloadingPointName { get; set; }
        /// <summary>
        /// Terms of Payment Key
        /// </summary>
        public string TermsOfPaymentKey { get; set; }
        /// <summary>
        /// Terms of Payment Key Description
        /// </summary>
        public string TermsOfPaymentKeyDescription { get; set; }
        /// <summary>
        /// Incoterms (Part 1)
        /// </summary>
        public string Incoterms1 { get; set; }
        /// <summary>
        /// Incoterms (Part 2)
        /// </summary>
        public string Incoterms2 { get; set; }
        /// <summary>
        /// Total Weight
        /// </summary>
        public string TotalWeight { get; set; }
        /// <summary>
        /// Total number of packages in delivery
        /// </summary>
        public string TotalNumber { get; set; }
        /// <summary>
        /// Time of delivery
        /// </summary>
        public string TimeOfDelivery { get; set; }
        /// <summary>
        /// Loading Point
        /// </summary>
        public string LoadingPoint { get; set; }
        /// <summary>
        /// Billing date for billing index and printout
        /// </summary>
        public string BillingDate { get; set; }
        /// <summary>
        /// Bill of lading
        /// </summary>
        public string BillOfLading { get; set; }
        /// <summary>
        /// Means-of-Transport Type
        /// </summary>
        public string MeansOfTransportType { get; set; }
        /// <summary>
        /// Means-of-Transport Type Description
        /// </summary>
        public string MeansOfTransportTypeDescription { get; set; }
        /// <summary>
        /// Means of Transport ID
        /// </summary>
        public string MeansOfTransportID { get; set; }
        /// <summary>
        /// Insurance type
        /// </summary>
        public string InsuranceType { get; set; }
        /// <summary>
        /// Insurance type Description of the Shipping Type
        /// </summary>
        public string InsuranceTypeDescription { get; set; }
        /// <summary>
        /// Document Date in Document
        /// </summary>
        public string DocumentDate { get; set; }
        /// <summary>
        /// Actual Goods Movement Date
        /// </summary>
        public string ActualGoodsMovementDate { get; set; }
        /// <summary>
        /// Receiving plant for deliveries
        /// </summary>
        public string ReceivingPlant { get; set; }
        /// <summary>
        /// Receiving plant name for deliveries
        /// </summary>
        public string ReceivingPlantName { get; set; }
        /// <summary>
        /// Time zone of delivering location
        /// </summary>
        public string TimeZone { get; set; }
        /// <summary>
        /// Time zone of recipient location
        /// </summary>
        public string TimeZoneOfRecipient { get; set; }
        /// <summary>
        /// Transaction Code
        /// </summary>
        public string TransactionCode { get; set; }
        /// <summary>
        /// DN created date
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// DN created time
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// DN creator
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// DN changed date
        /// </summary>
        public string ChangedDate { get; set; }
        /// <summary>
        /// DN changed time
        /// </summary>
        public string ChangedTime { get; set; }
        /// <summary>
        /// DN changed by
        /// </summary>
        public string ChangedBy { get; set; }
        /// <summary>
        /// Worldwide unique key for Delivery header
        /// </summary>
        public string Uniqueidentifier { get; set; }
        /// <summary>
        /// Net weight
        /// </summary>
        public string NetWeight { get; set; }
        /// <summary>
        /// Weight Unit
        /// </summary>
        public string WeightUnit { get; set; }
        /// <summary>
        /// Volume
        /// </summary>
        public string Volume { get; set; }
        /// <summary>
        /// Volume unit
        /// </summary>
        public string VolumeUnit { get; set; }
        /// <summary>
        /// Document number of the reference document
        /// </summary>
        public string DocumentNumber { get; set; }
        /// <summary>
        /// Division
        /// </summary>
        public string Division { get; set; }
        /// <summary>
        /// Division Description
        /// </summary>
        public string DivisionDescription { get; set; }
        /// <summary>
        /// Sales Group
        /// </summary>
        public string SalesGroup { get; set; }
        /// <summary>
        /// Sales Office
        /// </summary>
        public string SalesOffice { get; set; }
        /// <summary>
        /// Business Area
        /// </summary>
        public string BusinessArea { get; set; }
        /// <summary>
        /// Distribution Channel
        /// </summary>
        public string DistributionChannel { get; set; }
        /// <summary>
        /// Cost Center
        /// </summary>
        public string CostCenter { get; set; }
        /// <summary>
        /// Controlling Area
        /// </summary>
        public string ControllingArea { get; set; }

        /// <summary>
        /// 出口号码
        /// </summary>
        public string ExportNO { get; set; }
        /// <summary>
        /// 报关单号
        /// </summary>
        public string DeclarationNO { get; set; }
        /// <summary>
        /// 批准文号
        /// </summary>
        public string ApprovalNO { get; set; }
        /// <summary>
        /// 出口口岸代码
        /// </summary>
        public string ExportCode { get; set; }
        /// <summary>
        ///  出口口岸
        /// </summary>
        public string ExportDescp { get; set; }
        /// <summary>
        /// 出口报关单统一编号
        /// </summary>
        public string UniCode { get; set; }
        /// <summary>
        /// 特殊处理标记
        /// </summary>
        public string SpecialProcID { get; set; }
        /// <summary>
        /// 特殊处理说明
        /// </summary>
        public string SpecialDescp { get; set; }
        /// <summary>
        /// 賣頭1
        /// </summary>
        public string Mark1 { get; set; }
        /// <summary>
        /// 賣頭2
        /// </summary>
        public string Mark2 { get; set; }
        /// <summary>
        /// 賣頭3
        /// </summary>
        public string Mark3 { get; set; }
        /// <summary>
        /// 賣頭4
        /// </summary>
        public string Mark4 { get; set; }
        /// <summary>
        /// 賣頭5
        /// </summary>
        public string Mark5 { get; set; }
        /// <summary>
        /// 賣頭6
        /// </summary>
        public string Mark6 { get; set; }
        /// <summary>
        /// Original Sales Org
        /// </summary>
        public string OriginalSalesOrg { get; set; }
        /// <summary>
        /// Original Sold to
        /// </summary>
        public string OriginalSoldTo { get; set; }
        /// <summary>
        /// Customer Profile Code
        /// </summary>
        public string ProfileCode { get; set; }
        /// <summary>
        /// 开户银行的简要键
        /// </summary>
        public string BankShortKey{get;set;}
        /// <summary>
        /// 银行国家代码
        /// </summary>
        public string BankCountryKey{get;set;}
        /// <summary>
        /// 银行代码
        /// </summary>
        public string BankKeys{get;set;}
        /// <summary>
        /// 帐户细目的代码 
        /// </summary>
        public string BankAccountDetails{get;set;}
        /// <summary>
        /// 银行帐户号码
        /// </summary>
        public string BankAccountNumber{get;set;}
        /// <summary>
        /// 备选银行帐号 (用于不明确帐号）
        /// </summary>
        public string BankAlternativeNumber{get;set;}
        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName{get;set;}
        /// <summary>
        /// 银行地区（省/自治区/直辖市、市、县）
        /// </summary>
        public string BankRegion{get;set;}
        /// <summary>
        /// 银行住宅号及街道
        /// </summary>
        public string BankStreet{get;set;}
        /// <summary>
        /// 银行城市
        /// </summary>
        public string BankCity{get;set;}
        /// <summary>
        /// 国际付款的 SWIFT/BIC
        /// </summary>
        public string BankSwift{get;set;}
        /// <summary>
        /// 银行编号
        /// </summary>
        public string BankNumber{get;set;}
        /// <summary>
        /// 邮政银行的往来帐户号
        /// </summary>
        public string BankCurrentAccountNumber{get;set;}
        /// <summary>
        /// 分行
        /// </summary>
        public string BankBranch{get;set;}

        /// <summary>
        /// 客户入库单号，出货备注
        /// </summary>
        public string CustomerIncomingNumber { get; set; }
        public string OriginalSalesDocumentType { get; set; }
        public string CustomerOrderNO { get; set; }
    }

    /// <summary>
    /// Item Info
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// Source SAP
        /// </summary>
        public string SourceSAP { get; set; }
        /// <summary>
        /// Outbound delivery no.
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// Outbound delivery no. with company code
        /// </summary>
        public string DNNOWithCompanyCode { get; set; }
        /// <summary>
        /// company code for DN
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// Delivery Item
        /// </summary>
        public string DeliveryItem { get; set; }
        /// <summary>
        /// Sales Document
        /// </summary>
        public string SalesDocument { get; set; }
        /// <summary>
        /// Sales Document Item
        /// </summary>
        public string SalesDocumentItem { get; set; }
        /// <summary>
        /// SO created date
        /// </summary>
        public string SOCreateDate { get; set; }
        /// <summary>
        /// SO created time
        /// </summary>
        public string SOCreateTime { get; set; }
        /// <summary>
        /// SO creator
        /// </summary>
        public string SOCreateBy { get; set; }
        /// <summary>
        /// SO changed date
        /// </summary>
        public string SOChangedDate { get; set; }
        /// <summary>
        /// SO changed time
        /// </summary>
        public string SOChangedTime { get; set; }
        /// <summary>
        /// SO changed by
        /// </summary>
        public string SOChangedBy { get; set; }
        /// <summary>
        /// Delivery item category
        /// </summary>
        public string DeliveryItemCategory { get; set; }
        /// <summary>
        /// Material Number
        /// </summary>
        public string MaterialNumber { get; set; }
        /// <summary>
        /// Customer Model Name
        /// </summary>
        public string CustomerModelName { get; set; }
        /// <summary>
        /// Material belonging to the customer
        /// </summary>
        public string MaterialBelonging { get; set; }
        /// <summary>
        /// Old material number
        /// </summary>
        public string OldMaterialNumber { get; set; }
        /// <summary>
        /// Material Group
        /// </summary>
        public string MaterialGroup { get; set; }
        /// <summary>
        /// Plant
        /// </summary>
        public string Plant { get; set; }
        /// <summary>
        /// Storage Location
        /// </summary>
        public string StorageLocation { get; set; }
        /// <summary>
        /// Actual quantity delivered (in sales units)
        /// </summary>
        public string ActualQuantityDelivered { get; set; }
        /// <summary>
        /// Base Unit of Measure
        /// </summary>
        public string BaseUnitOfMeasure { get; set; }
        /// <summary>
        /// Sales unit
        /// </summary>
        public string SalesUnit { get; set; }
        /// <summary>
        /// Gross Weight
        /// </summary>
        public string GrossWeight { get; set; }
        /// <summary>
        /// Actual quantity delivered in stockkeeping units
        /// </summary>
        public string ActualQuantityDeliveredInStockkeepingUnits { get; set; }
        /// <summary>
        /// Customer purchase order number
        /// </summary>
        public string CustomerPurchaseOrderNumber { get; set; }
        /// <summary>
        /// Ship-to Party's Purchase Order Number
        /// </summary>
        public string ShipToPurchaseOrderNumber { get; set; }
        /// <summary>
        /// Your Reference
        /// </summary>
        public string YourReference { get; set; }
        /// <summary>
        /// Ship-to party character
        /// </summary>
        public string ShipToCharacter { get; set; }
        /// <summary>
        /// Pricing Reference Material
        /// </summary>
        public string PricingReferenceMaterial { get; set; }
        /// <summary>
        /// Alternative materials
        /// </summary>
        public string AlternativeMaterials { get; set; }
        /// <summary>
        /// Preparation conditions
        /// </summary>
        public string PreparationConditions { get; set; }
        /// <summary>
        /// Commodity description
        /// </summary>
        public string CommodityDescription { get; set; }
        /// <summary>
        /// Commodity code of the goods
        /// </summary>
        public string CommodityCodeOfTheGoods { get; set; }
        /// <summary>
        /// Exporter commodity code
        /// </summary>
        public string ExporterCommodityCode { get; set; }
        /// <summary>
        /// Destination countries commodity code
        /// </summary>
        public string DestinationCountriesCommodityCode { get; set; }
        /// <summary>
        /// Short text for sales order item
        /// </summary>
        public string ShortText { get; set; }
        /// <summary>
        /// Storage Bin
        /// </summary>
        public string StorageBin { get; set; }
        /// <summary>
        /// Originating document
        /// </summary>
        public string OriginatingDocument { get; set; }
        /// <summary>
        /// Originating item
        /// </summary>
        public string OriginatingItem { get; set; }
        /// <summary>
        /// SD document category
        /// </summary>
        public string SDDocumentCategory { get; set; }
        /// <summary>
        /// Item number of the reference item
        /// </summary>
        public string ItemNumber { get; set; }
        /// <summary>
        /// Movement Type (Inventory Management)
        /// </summary>
        public string MovementType { get; set; }
        /// <summary>
        /// Requirement type
        /// </summary>
        public string RequirementType { get; set; }
        /// <summary>
        /// Planning type
        /// </summary>
        public string PlanningType { get; set; }
        /// <summary>
        /// Material Type
        /// </summary>
        public string MaterialType { get; set; }
        /// <summary>
        /// Valuation Type
        /// </summary>
        public string ValuationType { get; set; }
        /// <summary>
        /// Delivery group (items are delivered together)
        /// </summary>
        public string DeliveryGroup { get; set; }
        /// <summary>
        /// Quantity is Fixed
        /// </summary>
        public string QuantityIsFixed { get; set; }
        /// <summary>
        /// International Article Number (EAN/UPC)
        /// </summary>
        public string InternationalArticleNumber { get; set; }
        /// <summary>
        /// Profit Center
        /// </summary>
        public string ProfitCenter { get; set; }
        /// <summary>
        /// Order Number
        /// </summary>
        public string OrderNumber { get; set; }
        /// <summary>
        /// Order Item Number
        /// </summary>
        public string OrderItemNumber { get; set; }
        /// <summary>
        /// Sales Order Number
        /// </summary>
        public string SalesOrderNumber { get; set; }
        /// <summary>
        /// Item Number in Sales Order
        /// </summary>
        public string ItemNumberInSalesOder { get; set; }
        /// <summary>
        /// Receiving point
        /// </summary>
        public string ReceivingPoint { get; set; }
        /// <summary>
        /// Condition pricing unit
        /// </summary>
        public string ConditionPricingUnit { get; set; }
        /// <summary>
        /// Condition unit
        /// </summary>
        public string ConditionUnit { get; set; }
        /// <summary>
        /// Net price
        /// </summary>
        public string NetPrice { get; set; }
        /// <summary>
        /// Net Value in Document Currency
        /// </summary>
        public string NetValue { get; set; }
        /// <summary>
        /// Statistical values
        /// </summary>
        public string StatisticalValues { get; set; }
        /// <summary>
        /// Movement Indicator
        /// </summary>
        public string MovementIndicator { get; set; }
        /// <summary>
        /// POD indicator (relevance, verification, confirmation)
        /// </summary>
        public string PODIndicator { get; set; }
        /// <summary>
        /// Original Quantity of Delivery Item
        /// </summary>
        public string OriginalQuantityOfDeliveryItem { get; set; }
        /// <summary>
        /// DN created date
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// DN created time
        /// </summary>
        public string CreateTime { get; set; }
        /// <summary>
        /// DN creator
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// DN changed date
        /// </summary>
        public string ChangedDate { get; set; }
        /// <summary>
        /// DN changed time
        /// </summary>
        public string ChangedTime { get; set; }
        /// <summary>
        /// DN changed by
        /// </summary>
        public string ChangedBy { get; set; }
        /// <summary>
        /// Worldwide unique key for Delivery header
        /// </summary>
        public string Uniqueidentifier { get; set; }
        /// <summary>
        /// Net weight
        /// </summary>
        public string NetWeight { get; set; }
        /// <summary>
        /// Weight Unit
        /// </summary>
        public string WeightUnit { get; set; }
        /// <summary>
        /// Volume
        /// </summary>
        public string Volume { get; set; }
        /// <summary>
        /// Volume unit
        /// </summary>
        public string VolumeUnit { get; set; }
        /// <summary>
        /// Document number of the reference document
        /// </summary>
        public string DocumentNumber { get; set; }
        /// <summary>
        /// Division
        /// </summary>
        public string Division { get; set; }
        /// <summary>
        /// Division Description
        /// </summary>
        public string DivisionDescription { get; set; }
        /// <summary>
        /// Sales Group
        /// </summary>
        public string SalesGroup { get; set; }
        /// <summary>
        /// Sales Office
        /// </summary>
        public string SalesOffice { get; set; }
        /// <summary>
        /// Business Area
        /// </summary>
        public string BusinessArea { get; set; }
        /// <summary>
        /// Distribution Channel
        /// </summary>
        public string DistributionChannel { get; set; }
        /// <summary>
        /// Cost Center
        /// </summary>
        public string CostCenter { get; set; }
        /// <summary>
        /// Controlling Area
        /// </summary>
        public string ControllingArea { get; set; }
        /// <summary>
        /// SD 凭证货币
        /// </summary>
        public string SDDocumentCurrency { get; set; }
        /// <summary>
        /// 标准装柜量
        /// </summary>
        public string ContainerQTY { get; set; }
        /// <summary>
        /// 计费重
        /// </summary>
        public string ChargeableWeight { get; set; }
        /// <summary>
        /// UL标识
        /// </summary>
        public string UL { get; set; }

        public string WBS{get;set;}
        public string MaterialText { get; set; }


    }

    /// <summary>
    /// 伙伴信息
    /// </summary>
    public class PartnerInfo
    {
        /// <summary>
        /// Source SAP
        /// </summary>
        public string SourceSAP { get; set; }
        /// <summary>
        /// Outbound delivery no.
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// Outbound delivery no. with company code
        /// </summary>
        public string DNNOWithCompanyCode { get; set; }
        /// <summary>
        /// company code for DN
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// Partner function
        /// </summary>
        public string PartnerFunction { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CustomerNumber { get; set; }
        /// <summary>
        /// 供应商或债权人的帐号
        /// </summary>
        public string VendorAccountNumber { get; set; }
        /// <summary>
        /// Partner number	合作伙伴代码
        /// </summary>
        public string PartnerNumber { get; set; }
        /// <summary>
        /// Indicator: Is the account a one-time account?	指示符:科目是一次性科目吗?
        /// </summary>
        public string Indicator { get; set; }
        /// <summary>
        /// Date on Which Record Was Created	记录的创建日期
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// Entry time	输入时间
        /// </summary>
        public string EntryTime { get; set; }
        /// <summary>
        /// Name of Person who Created the Object	创建对象的人员名称
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// Changed On	更改日期
        /// </summary>
        public string ChangedDate { get; set; }
        /// <summary>
        /// Time last change was made	上一次作修改的时间
        /// </summary>
        public string LastChangedTime { get; set; }
        /// <summary>
        ///  Name of Person Who Changed Object	对象更改人员的名称
        /// </summary>
        public string ChangedBy { get; set; }
        /// <summary>
        /// Form-of-Address Key	地址关键字的表格 
        /// </summary>
        public string AddressKey { get; set; }
        /// <summary>
        /// Form-of-Address Key	地址关键字的表格 
        /// </summary>
        public string AddressKey_P { get; set; }
        /// <summary>
        /// Name 1	名称 1
        /// </summary>
        public string Name1 { get; set; }
        /// <summary>
        /// Name 2	名称 2
        /// </summary>
        public string Name2 { get; set; }
        /// <summary>
        /// Name 3	名称 3
        /// </summary>
        public string Name3 { get; set; }
        /// <summary>
        /// Name 4	名称 4
        /// </summary>
        public string Name4 { get; set; }
        /// <summary>
        /// Converted name field (with form of address)	转换的名称字段(带有地址表单)
        /// </summary>
        public string ConvertedName { get; set; }
        /// <summary>
        /// c/o name	代收人姓名
        /// </summary>
        public string NameCO { get; set; }
        /// <summary>
        /// City	城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// District	区域 
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// City code for city/street file	城市/街道 文件的城市号码
        /// </summary>
        public string CityCode { get; set; }
        /// <summary>
        /// District code for City and Street file	城市和街道文件的区域代码
        /// </summary>
        public string DistrictCode { get; set; }
        /// <summary>
        /// City (different from postal city)	城市（与邮政城市不同） 
        /// </summary>
        public string PostalCity { get; set; }
        /// <summary>
        /// Different city for city/street file	与城市/街道文件不同的城市 
        /// </summary>
        public string StreetD { get; set; }
        /// <summary>
        /// City file test status	城市文件测试状态
        /// </summary>
        public string CityStatus { get; set; }
        /// <summary>
        /// Regional structure grouping	地区结构分组
        /// </summary>
        public string RegionalStructureGrouping { get; set; }
        /// <summary>
        /// City postal code	城市邮政编码
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// PO Box Postal Code邮政信箱的邮政编码
        /// </summary>
        public string POBoxPostalCode { get; set; }
        /// <summary>
        /// PO Box Postal Code邮政信箱的邮政编码
        /// </summary>
        public string POBoxPostalCode2 { get; set; } 
        /// <summary>
        /// PO Box Postal Code邮政信箱的邮政编码
        /// </summary>
        public string POBoxPostalCode2C { get; set; }
        /// <summary>
        /// Company Postal Code (for Large Customers)	公司的邮政编码（针对大客户）
        /// </summary>
        public string CompanyPostalCode { get; set; }
        /// <summary>
        /// (Not Supported)City Postal Code Extension, e.g. ZIP+4+2 Code	（不受支持）城市邮政编码扩展，例如 ZIP+4+2 编码
        /// </summary>
        public string PostalCodeExtension { get; set; }
        /// <summary>
        /// (Not Supported) PO Box Postal Code Extension	（不受支持）邮箱邮政编码扩展
        /// </summary>
        public string POBoxPostalCodeExtension { get; set; }
        /// <summary>
        /// (Not Supported) Major Customer Postal Code Extension	（不受支持）主要客户邮政编码扩展
        /// </summary>
        public string MajorCustomerPostalCodeExtension { get; set; }
        /// <summary>
        /// PO Box	邮政信箱 
        /// </summary>
        public string POBox { get; set; }
        /// <summary>
        /// PO Box Address Undeliverable Flag	邮局信箱地址不可交付标志 
        /// </summary>
        public string POBoxAddressUndeliverableFlag { get; set; }
        /// <summary>
        /// Flag: PO Box Without Number	标志：没有编号的邮箱
        /// </summary>
        public string POBoxWithoutNumber { get; set; }
        /// <summary>
        /// PO Box city	邮政信箱城市 
        /// </summary>
        public string POBoxCity { get; set; }
        /// <summary>
        /// City PO box code (City file)	城市邮箱代码(城市文件)
        /// </summary>
        public string POBoxCityCode { get; set; }
        /// <summary>
        /// Region for PO Box (Country, State, Province, ...)	邮政信箱的区域（州、省、国家...）
        /// </summary>
        public string POBoxRegion { get; set; }
        /// <summary>
        /// PO box country	邮箱国家
        /// </summary>
        public string POBoxCountry { get; set; }
        /// <summary>
        /// (Not Supported) Post Delivery District	（不受支持）过帐交货地区
        /// </summary>
        public string PostDeliveryDistrict { get; set; }
        /// <summary>
        /// Transportation zone to or from which the goods are delivered	发送货物的目的地运输区域
        /// </summary>
        public string TransportationZone { get; set; }
        /// <summary>
        /// Street 街道
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Street Address Undeliverable Flag
        /// </summary>
        public string StreetAddressUndeliverableFlag { get; set; }
        /// <summary>
        /// Street Number for City/Street File	城市/街道文件的街道编号
        /// </summary>
        public string StreetNumber { get; set; }
        /// <summary>
        /// (Not Supported) Abbreviation of Street Name	（不受支持）街区名称缩写
        /// </summary>
        public string AbbreviationOfStreetName { get; set; }
        /// <summary>
        /// House Number	门牌号 
        /// </summary>
        public string HouseNumber { get; set; }
        /// <summary>
        /// House number supplement 开户号补充
        /// </summary>
        public string HouseNumberSupplement { get; set; }
        /// <summary>
        /// (Not supported) House Number Range	（不受支持）房屋号范围
        /// </summary>
        public string HouseNumberRange { get; set; }
        /// <summary>
        /// Street 2	街道2
        /// </summary>
        public string Street2 { get; set; }
        /// <summary>
        /// Street 3	街道 3
        /// </summary>
        public string Street3 { get; set; }
        /// <summary>
        /// Street 4	街道 4 
        /// </summary>
        public string Street4 { get; set; }
        /// <summary>
        /// Street 5	街道 5
        /// </summary>
        public string Street5 { get; set; }
        /// <summary>
        /// Building (Number or Code)	建筑物（编号或代码） 
        /// </summary>
        public string Building { get; set; }
        /// <summary>
        /// Building (number or code) 建筑物（编号或代码）
        /// </summary>
        public string Building_C { get; set; }
        /// <summary>
        /// Floor in building	建筑物的层
        /// </summary>
        public string Floor { get; set; }
        /// <summary>
        /// Floor in building	建筑物的层
        /// </summary>
        public string Floor_C { get; set; }
        /// <summary>
        /// Room or Appartment Number	房间或公寓号 
        /// </summary>
        public string Room { get; set; }
        /// <summary>
        /// Room or Appartment Number	房间或公寓号 
        /// </summary>
        public string Room_C { get; set; }
        /// <summary>
        /// Country Key	国家键值
        /// </summary>
        public string CountryKey { get; set; }
        public string CountryName { get; set; }
        /// <summary>
        /// Language Key	语言代码
        /// </summary>
        public string LanguageKey { get; set; }
        /// <summary>
        /// Language Key	语言代码
        /// </summary>
        public string LanguageKey_P { get; set; }
        /// <summary>
        /// Region (State, Province, County)	地区（省/自治区/直辖市、市、县）
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// Search Term 1	检索项1 
        /// </summary>
        public string SearchTerm1 { get; set; }
        /// <summary>
        /// Search Term 2	检索项2
        /// </summary>
        public string SearchTerm2 { get; set; }
        /// <summary>
        /// (Not Supported) Search Term	（不受支持）检索项	
        /// </summary>
        public string SearchTerm1_C { get; set; }
        /// <summary>
        /// (Not Supported) Search Term	（不受支持）检索项	
        /// </summary>
        public string SearchTerm2_C { get; set; }
        /// <summary>
        /// (Not Supported) Search Term	（不受支持）检索项	
        /// </summary>
        public string SearchTerm1_P { get; set; }
        /// <summary>
        /// (Not Supported) Search Term	（不受支持）检索项	
        /// </summary>
        public string SearchTerm2_P { get; set; }
        /// <summary>
        /// (Not Supported) Phonetic Search Sort Field	（不受支持）语音搜索排序字段
        /// </summary>
        public string PhoneticSearchSortField { get; set; }
        /// <summary>
        /// (Not Supported) Phonetic Search Sort Field	（不受支持）语音搜索排序字段
        /// </summary>
        public string PhoneticSearchSortField_C { get; set; }
        /// <summary>
        /// (Not Supported) Phonetic Search Sort Field	（不受支持）语音搜索排序字段
        /// </summary>
        public string PhoneticSearchSortField_P { get; set; }
        /// <summary>
        /// Not Supported) Address Data Source (Key)	（不受支持）地址数据源（键值）
        /// </summary>
        public string AddressDataSourceKey { get; set; }
        /// <summary>
        /// Extension (only for data conversion) (e.g. data line)	扩展名（仅用于数据转换）（例如，数据线）
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        ///  (only for data conversion) (e.g. telebox)	扩展名（仅用于数据转换）（例如语音信箱）
        /// </summary>
        public string Extension2 { get; set; }
        /// <summary>
        /// Address time zone	地址时区
        /// </summary>
        public string TimeZone { get; set; }
        /// <summary>
        /// Tax Jurisdiction	税务管辖权
        /// </summary>
        public string TaxJurisdiction { get; set; }
        /// <summary>
        /// (Not supported) Physical address ID	（不受支持）物理地址标识
        /// </summary>
        public string PhysicalAdressID { get; set; }
        /// <summary>
        /// Address notes	地址注释
        /// </summary>
        public string AddressNotes { get; set; }
        /// <summary>
        /// Address record creation original language	地址记录创建原始语言
        /// </summary>
        public string OriginalLanguage { get; set; }
        /// <summary>
        /// Address record creation original language	地址记录创建原始语言
        /// </summary>
        public string OriginalLanguage_P { get; set; }
        /// <summary>
        /// PO Box Lobby	前厅邮政信箱
        /// </summary>
        public string POBoxLobby { get; set; }
        /// <summary>
        /// Type of Delivery Service	交付服务的类型
        /// </summary>
        public string DeliveryServiceType { get; set; }
        /// <summary>
        /// Number of Delivery Service	交付服务的编号
        /// </summary>
        public string DeliveryServiceNumber { get; set; }
        /// <summary>
        /// Prefecture code for county	县的县代码
        /// </summary>
        public string PrefectureCode { get; set; }
        /// <summary>
        /// Prefecture	县
        /// </summary>
        public string Prefecture { get; set; }
        /// <summary>
        /// Township code for Township	乡镇的乡镇代码
        /// </summary>
        public string TownshipCode { get; set; }
        /// <summary>
        /// Township	乡镇
        /// </summary>
        public string Township { get; set; }
        /// <summary>
        /// Title text	称谓文本 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// First telephone no.: dialling code+number	第一个电话号码：区号 + 号码
        /// </summary>
        public string Telephone1 { get; set; }
        /// <summary>
        /// First Telephone No.: Extension	第一个电话号码:分机号
        /// </summary>
        public string Telephone1Extension { get; set; }
        /// <summary>
        /// First fax no.: dialling code+number	第一个传真号: 拨号 + 编号
        /// </summary>
        public string Fax { get; set; }
        /// <summary>
        /// First fax no.: extension	第一个传真号: 分机号
        /// </summary>
        public string FaxExtension { get; set; }
        /// <summary>
        /// E-Mail Address	电子邮件地址
        /// </summary>
        public string EMail { get; set; }
        /// <summary>
        /// First Mobile Telephone No.: Dialing Code + Number	第一个移动电话号码：区号 + 电话号码
        /// </summary>
        public string MobileTelephone { get; set; }
        /// <summary>
        /// First Mobile Telephone No.: Extension	第一个移动电话号码：分机
        /// </summary>
        public string MobileTelephoneExtension { get; set; }
        /// <summary>
        /// SAP-Office key	SAP-Office 键 
        /// </summary>
        public string SAPOfficeKey { get; set; }
        /// <summary>
        /// Department	部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// Function	功能
        /// </summary>
        public string Function { get; set; }
        /// <summary>
        /// Short name for correspondence	信函的短名称 
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// First name	名  
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last name	姓
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Name of person at birth   人员出生时的名字
        /// </summary>
        public string NameOfPersonAtBirth { get; set; }
        /// <summary>
        /// Middle name or second forename of a person	一个人的中间名字或第二个名字 
        /// </summary>
        public string SecondName { get; set; }
        /// <summary>
        /// Second surname of a person	人的第二个姓
        /// </summary>
        public string SecondLastName { get; set; }
        /// <summary>
        /// Full Name of Person	完整的人员名称
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Status of Field 'Full Name' NAME_TEXT	字段'命名' NAME_TEXT 的状态 
        /// </summary>
        public string FullNameStatus { get; set; }
        /// <summary>
        /// Academic Title: Key	学术头衔:关键字 
        /// </summary>
        public string AcademicTitle { get; set; }
        /// <summary>
        /// Second academic title (key)	第二部分学术头衔(代码)
        /// </summary>
        public string AcademicTitle2 { get; set; }
        /// <summary>
        /// Name Prefix (Key)	名称前缀(关键字) 
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// 2nd name prefix (key)	第二个名字前缀(代码)
        /// </summary>
        public string Prefix2 { get; set; }
        /// <summary>
        /// Name supplement, e.g. noble title (key)	名称的补充部分,例如贵族的头衔(代码) 
        /// </summary>
        public string NameSupplement { get; set; }
        /// <summary>
        /// Nickname or name used	所使用的昵称或名字
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// Middle Initial or personal initials	中间初始 或个人初始
        /// </summary>
        public string Initial { get; set; }
        /// <summary>
        /// Name format	名称格式
        /// </summary>
        public string NameFormat { get; set; }
        /// <summary>
        /// Country for name format rule	国家名称的格式规则
        /// </summary>
        public string CountryFormat { get; set; }
        /// <summary>
        /// Profession	职业
        /// </summary>
        public string Profession { get; set; }
        /// <summary>
        /// Gender key	性别码
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Personal data source (key)	个人数据源（关键字)
        /// </summary>
        public string PersonalDataSource { get; set; }
        /// <summary>
        /// Group key	组代码
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Vendor account group	供应商帐户组
        /// </summary>
        public string VendorAccountGroup { get; set; }
        /// <summary>
        /// Company ID of Trading Partner	贸易伙伴的公司标识
        /// </summary>
        public string CompanyIDOfTradingPartner { get; set; }
        /// <summary>
        /// VAT Registration Number
        /// </summary>
        public string VATNumber { get; set; }

        /// <summary>
        /// Int. mail postal code	国内邮政编码
        /// </summary>
        public string IntMail { get; set; }

        public string InternationalNO1{get;set;}
        public string InternationalNO2 { get; set; }
    }

    /// <summary>
    /// 价格信息
    /// </summary>
    public class PriceInfo
    {
        /// <summary>
        /// Source SAP
        /// </summary>
        public string SourceSAP { get; set; }
        /// <summary>
        /// Outbound delivery no.
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// Outbound delivery no. with company code
        /// </summary>
        public string DNNOWithCompanyCode { get; set; }
        /// <summary>
        /// company code for DN
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// Delivery Item
        /// </summary>
        public string DeliveryItem { get; set; }
        /// <summary>
        /// Condition type
        /// </summary>
        public string ConditionType { get; set; }
        /// <summary>
        /// Number of the document condition
        /// </summary>
        public string NumberOfTheDocumentCondition { get; set; }
        /// <summary>
        /// Condition item number
        /// </summary>
        public string ConditionItemNumber { get; set; }
        /// <summary>
        /// Step number
        /// </summary>
        public string StepNumber { get; set; }
        /// <summary>
        /// Condition counter
        /// </summary>
        public string ConditionCounter { get; set; }
        /// <summary>
        /// Application
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Condition base value
        /// </summary>
        public string BaseValue { get; set; }
        /// <summary>
        /// Rate (condition amount or percentage)
        /// </summary>
        public string Rate { get; set; }
        /// <summary>
        /// Currency Key
        /// </summary>
        public string CurrencyKey { get; set; }
        /// <summary>
        /// Condition exchange rate for conversion to local currency
        /// </summary>
        public string LocalCurrency { get; set; }
        /// <summary>
        /// Condition pricing unit
        /// </summary>
        public string PricingUnit { get; set; }
        /// <summary>
        /// Condition category (examples: tax, freight, price, cost)
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Condition is used for statistics
        /// </summary>
        public string Statistics { get; set; }
        /// <summary>
        /// Condition is Relevant for Accrual  (e.g. Freight)
        /// </summary>
        public string Accrual { get; set; }
        /// <summary>
        /// Condition record number
        /// </summary>
        public string RecordNumber { get; set; }
        /// <summary>
        /// Sequential number of the condition
        /// </summary>
        public string SEQNumber { get; set; }
        /// <summary>
        /// Account key
        /// </summary>
        public string AccountKey { get; set; }
        /// <summary>
        /// G/L Account Number
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Tax on sales/purchases code
        /// </summary>
        public string PurchasesCode { get; set; }
        /// <summary>
        /// Withholding tax code
        /// </summary>
        public string WithholdingTaxCode { get; set; }
        /// <summary>
        /// Condition value
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Condition is inactive
        /// </summary>
        public string Inactive { get; set; }
        /// <summary>
        /// Condition class
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// Condition base value
        /// </summary>
        public string BaseValue2 { get; set; }
        /// <summary>
        /// Condition currency (for cumulation fields)
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Condition value
        /// </summary>
        public string Value2 { get; set; }

        public string CalculationType { get; set; }
        public string ConditionUnit { get; set; }
    }

    /// <summary>
    /// 分类信息
    /// </summary>
    public class ClassificationInfo
    {
        /// <summary>
        /// Source SAP
        /// </summary>
        public string SourceSAP { get; set; }
        /// <summary>
        /// Outbound delivery no.
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// Outbound delivery no. with company code
        /// </summary>
        public string DNNOWithCompanyCode { get; set; }
        /// <summary>
        /// company code for DN
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// DeliveryItem
        /// </summary>
        public string DeliveryItem { get; set; }
        /// <summary>
        /// Material Number
        /// </summary>
        public string MaterialNumber { get; set; }
        /// <summary>
        /// Characteristic Name
        /// </summary>
        public string CharacteristicName { get; set; }
        /// <summary>
        /// Characteristic Value
        /// </summary>
        public string CharacteristicValue { get; set; }
        /// <summary>
        /// Indicator: characteristic is inherited
        /// </summary>
        public string Indicator { get; set; }
        /// <summary>
        /// Instance counter
        /// </summary>
        public string InstanceCounter { get; set; }
        /// <summary>
        /// Characteristic Value
        /// </summary>
        public string CharacteristicValue2 { get; set; }
        /// <summary>
        /// Characteristic description
        /// </summary>
        public string CharacteristicDescription { get; set; }
    }

    public class PalletInfo
    {
        public string SourceSAP { get; set; }
        public string DNNOWithCompanyCode { get; set; }
        public string CompanyCode { get; set; }
        public string DNNO { get; set; }
        public string InternalHandlingUnitNumber { get; set; }
        public string HandlingUnitItem { get; set; }
        public string ItemNumber { get; set; }
        public string CreateDate { get; set; }
        public string EntryTime { get; set; }
        public string CreatedBy { get; set; }
        public string ChangedDate { get; set; }
        public string ChangedTime { get; set; }
        public string ChangedBy { get; set; }
        public string ExternalHandlingUnitIdentification { get; set; }
        public string PackagingMaterials { get; set; }
        public string PackagingMaterialsDescription { get; set; }
        public string HandlingUnitContent { get; set; }
        public string PackagingMaterialType { get; set; }
        public string Description { get; set; }
        public string HigherLevelHandlingUnit { get; set; }
        public string MaterialNumber { get; set; }
        public string QTY { get; set; }
        public string QTYUnit { get; set; }
        public string GrossWeight { get; set; }
        public string NetWeight { get; set; }
        public string NetWeightUnit { get; set; }
        public string AllowedLoadingWeight { get; set; }
        public string TareWeight { get; set; }
        public string WeightUnit { get; set; }
        public string TotalVolume { get; set; }
        public string LoadingVolume { get; set; }
        public string LoadingVolumeUnit { get; set; }
        public string TareVolume { get; set; }
        public string Volume { get; set; }
        public string VolumeUnit { get; set; }
        public string ExternalHandlingUnitIdentification2 { get; set; }
        public string SortField { get; set; }
        public string HandlingUnitGroup1 { get; set; }
        public string HandlingUnitGroup2 { get; set; }
        public string HandlingUnitGroup3 { get; set; }
        public string HandlingUnitGroup4 { get; set; }
        public string HandlingUnitGroup5 { get; set; }
    }
}