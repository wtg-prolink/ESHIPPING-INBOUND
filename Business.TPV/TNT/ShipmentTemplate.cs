using Business.EDI;
using Business.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Business.Attribute;

namespace Business.TPV.TNT
{
    public class ShipmentTemplate : Template
    {
        #region edi
        public override XmlEDINode CreateXmlEDINode()
        {
            XmlEDINode root = new XmlEDINode("ESHIPPER");
            SetLogin(root);
            SetConsignMentBatch(root);
            SetActivity(root);
            return root;
        }
        void SetLogin(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("LOGIN");
            TNTCertification c = Manager.GetRequstCertificationInfo();
            XmlEDINode node = new XmlEDINode("COMPANY", c.User);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("PASSWORD", c.Password);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("APPID", "EC");
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("APPVERSION", "2.2");
            mNode.ChildNodes.Add(node);
            root.ChildNodes.Add(mNode);
        }
        void SetConsignMentBatch(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("CONSIGNMENTBATCH");
            XmlEDINode node = new XmlEDINode("GROUPCODE", GroupCode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("SENDER");
            SetParty(node, Sender);
            SetCollection(node);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONSIGNMENT");
            SetConsignMent(node);
            mNode.ChildNodes.Add(node);
            root.ChildNodes.Add(mNode);
        }
        void SetConsignMent(XmlEDINode mNode)
        {
            XmlEDINode conRefNode = new XmlEDINode("CONREF", UniqueNumber);
            mNode.ChildNodes.Add(conRefNode);
            XmlEDINode dNode = new XmlEDINode("DETAILS");
            XmlEDINode pNode = new XmlEDINode("RECEIVER");
            SetParty(pNode, Receiver);
            dNode.ChildNodes.Add(pNode);
            if (Delivery != null)
            {
                pNode = new XmlEDINode("DELIVERY");
                SetParty(pNode, Delivery);
                dNode.ChildNodes.Add(pNode);
            }
            XmlEDINode node = new XmlEDINode("CUSTOMERREF", CustomerReference);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTYPE", ConType.ToString());
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("PAYMENTIND", GetPayCode());
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("ITEMS", TotalItems);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("TOTALWEIGHT", TotalWeight);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("TOTALVOLUME", TotalVolume);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("CURRENCY", Currency);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("GOODSVALUE", GoodsValue);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("INSURANCEVALUE", InsuranceValue);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("INSURANCECURRENCY", InsuranceCurrency);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("SERVICE", Service);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("OPTION", Option1);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("DESCRIPTION", Description);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("DELIVERYINST", DeliveryInst);
            dNode.ChildNodes.Add(node);
            node = new XmlEDINode("PACKAGE");
            SetPackage(node);
            dNode.ChildNodes.Add(node);
            mNode.ChildNodes.Add(dNode);
        }
        void SetPackage(XmlEDINode mNode)
        {
            XmlEDINode node = new XmlEDINode("ITEMS", PackageInfo.Count.ToString());
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("DESCRIPTION", PackageInfo.Description);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("LENGTH", PackageInfo.Length);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("HEIGHT", PackageInfo.Height);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("WIDTH", PackageInfo.Width);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("WEIGHT", PackageInfo.Weight);
            mNode.ChildNodes.Add(node);
            foreach (var item in PackageInfo.ArticleInfos)
            {
                node = new XmlEDINode("ARTICLE");
                XmlEDINode aNode = new XmlEDINode("ITEMS", item.Count);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("DESCRIPTION", item.Description);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("WEIGHT", item.Weight);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("INVOICEVALUE", item.InvoiceValue);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("INVOICEDESC", item.InvoiceDesc);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("HTS", item.HTSCode);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("COUNTRY", item.Country);
                node.ChildNodes.Add(aNode);
                aNode = new XmlEDINode("EMRN", item.EMRN);
                node.ChildNodes.Add(aNode);
                mNode.ChildNodes.Add(node);
            }
        }
        void SetCollection(XmlEDINode mNode)
        {
            XmlEDINode cNode = new XmlEDINode("COLLECTION");
            XmlEDINode node = null;
            if (Collection != null)
            {
                node = new XmlEDINode("COLLECTIONADDRESS");
                SetParty(node, Collection);
                cNode.ChildNodes.Add(node);
            }
            node = new XmlEDINode("SHIPDATE", ShipDate.Value.ToString("dd/MM/yyyy"));
            cNode.ChildNodes.Add(node);
            node = new XmlEDINode("PREFCOLLECTTIME");
            SetCollectionTime(node, PrefCollectTime);
            cNode.ChildNodes.Add(node);
            node = new XmlEDINode("ALTCOLLECTTIME");
            SetCollectionTime(node, AltCollectTime);
            cNode.ChildNodes.Add(node);
            node = new XmlEDINode("COLLINSTRUCTIONS", CollinsTructions);
            cNode.ChildNodes.Add(node);
            mNode.ChildNodes.Add(cNode);
        }
        void SetActivity(XmlEDINode root)
        {
            XmlEDINode mNode = new XmlEDINode("ACTIVITY");
            XmlEDINode cNode = new XmlEDINode("CREATE");
            cNode.ChildNodes.Add(GetUniqueNumberNode());
            mNode.ChildNodes.Add(cNode);
            cNode = new XmlEDINode("RATE");
            cNode.ChildNodes.Add(GetUniqueNumberNode());
            mNode.ChildNodes.Add(cNode);
            cNode = new XmlEDINode("PRINT");
            XmlEDINode nNode = new XmlEDINode("CONNOTE");
            nNode.ChildNodes.Add(GetUniqueNumberNode());
            cNode.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("LABEL");
            nNode.ChildNodes.Add(GetUniqueNumberNode());
            cNode.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("MANIFEST");
            nNode.ChildNodes.Add(GetUniqueNumberNode());
            cNode.ChildNodes.Add(nNode);
            nNode = new XmlEDINode("INVOICE");
            nNode.ChildNodes.Add(GetUniqueNumberNode());
            cNode.ChildNodes.Add(nNode);
            mNode.ChildNodes.Add(cNode);
            root.ChildNodes.Add(mNode);
        }
        XmlEDINode GetUniqueNumberNode()
        {
            return new XmlEDINode("CONREF", UniqueNumber);
        }

        void SetCollectionTime(XmlEDINode mNode, BetweenTime time)
        {
            if (time == null) return;
            XmlEDINode node = new XmlEDINode("FROM", time.From.ToShortTimeString());
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("TO", time.From.ToShortTimeString());
            mNode.ChildNodes.Add(node);
        }
        void SetParty(XmlEDINode mNode, PartyInfo info)
        {
            if (info == null) return;
            XmlEDINode node = new XmlEDINode("COMPANYNAME", info.CompanyName);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("STREETADDRESS1", info.Address1);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("STREETADDRESS2", info.Address2);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("STREETADDRESS3", info.Address3);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CITY", info.City);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("PROVINCE", info.Province);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("POSTCODE", info.PostCode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("COUNTRY", info.Country);
            mNode.ChildNodes.Add(node);
            if (!string.IsNullOrEmpty(info.Account))
            {
                node = new XmlEDINode("ACCOUNT", info.Account);
                mNode.ChildNodes.Add(node);
            }
            if (!string.IsNullOrEmpty(info.AccountCountry))
            {
                node = new XmlEDINode("ACCOUNTCOUNTRY ", info.AccountCountry);
                mNode.ChildNodes.Add(node);
            }
            node = new XmlEDINode("VAT", info.VAT);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTACTNAME", info.ContactName);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTACTDIALCODE", info.ContactDialCode);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTACTTELEPHONE", info.ContactTelephone);
            mNode.ChildNodes.Add(node);
            node = new XmlEDINode("CONTACTEMAIL", info.ContactMail);
            mNode.ChildNodes.Add(node);
        }
        #endregion

        public ShipmentTemplate()
        {

        }

        public string DeliveryInst { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Groupcode to use for the subsequent consignments, so that they can be treated as a batch.
        /// </summary>
        [Required]
        public string GroupCode { get; set; }

        /// <summary>
        /// 装货日期
        /// </summary>
        [Required]
        public DateTime? ShipDate { get; set; }

        /// <summary>
        /// 发送者信息
        /// </summary>
        [Required]
        public PartyInfo Sender { get; set; }
        /// <summary>
        /// 收件地址信息
        /// </summary>
        public PartyInfo Collection { get; set; }
        /// <summary>
        /// 接收者信息
        /// </summary>
        [Required]
        public PartyInfo Receiver { get; set; }

        /// <summary>
        /// Holds Delivery Address elements – can only be used if details, not consignmentnumbers, being passed in consignmentbatch
        /// </summary>
        public PartyInfo Delivery { get; set; }
        /// <summary>
        /// 包装信息
        /// </summary>
        [Required]
        public PackageInfo PackageInfo { get; set; }
        public PaymentType PaymentType { get; set; }
        /// <summary>
        /// Total number of packages in consignment
        /// </summary>
        [Required]
        public string TotalItems { get; set; }
        /// <summary>
        /// Total Volume of all packages
        /// </summary>
        [Required]
        public string TotalVolume { get; set; }
        /// <summary>
        /// Total weight of all packages
        /// </summary>
        [Required]
        public string TotalWeight { get; set; }
        /// <summary>
        /// Customer reference
        /// </summary>
        public string CustomerReference { get; set; }
        /// <summary>
        /// Consignment Type
        /// </summary>
        public ConTypes ConType { get; set; }
        /// <summary>
        /// A consignment number previously generated by ExpressConnect Shipping.
        /// </summary>
        [Required]
        public string ConNumber { get; set; }
        /// <summary>
        /// The TNT service required
        /// </summary>
        [Required]
        public string Service { get; set; }
        /// <summary>
        /// Insurance value of consignment in specified currency
        /// </summary>
        public string InsuranceValue { get; set; }
        /// <summary>
        /// First 30 characters of goods description
        /// </summary>
        public string GoodsDesc1 { get; set; }
        /// <summary>
        /// Middle 30 characters of goods description
        /// </summary>
        public string GoodsDesc2 { get; set; }
        /// <summary>
        /// Last 30 characters of goods description
        /// </summary>
        public string GoodsDesc3 { get; set; }
        /// <summary>
        /// Value of consignment in specified currency
        /// </summary>
        public string GoodsValue { get; set; }
        /// <summary>
        /// Delivery Instructions
        /// </summary>
        public string DeliveryInstructions { get; set; }
        /// <summary>
        /// Option required on this service
        /// </summary>
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Option5 { get; set; }
        /// <summary>
        /// Currency to be used
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Always the same as currency
        /// </summary>
        public string InsuranceCurrency { get; set; }

        /// <summary>
        /// Preferred period of time for collection – holds from and to elements
        /// </summary>
        [Required]
        public BetweenTime PrefCollectTime { get; set; }
        public BetweenTime AltCollectTime { get; set; }
        /// <summary>
        /// Collection Instructions
        /// </summary>
        [Required]
        public string CollinsTructions { get; set; }

        string GetPayCode()
        {
            switch (PaymentType)
            {
                case TNT.PaymentType.Receiver: return "R";
                case TNT.PaymentType.Sender: return "S";
            }
            return null;
        }

        public override bool Check(out EntityValidationResult result)
        {
            base.Check(out result);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Sender);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.Receiver);
            if (result.HasError) return false;
            result = ValidationHelper.ValidateEntity(this.PackageInfo);
            if (result.HasError) return false;
            result = CheckPayment();
            return result == null;
        }

        EntityValidationResult CheckPayment()
        {
            switch (PaymentType)
            {
                case TNT.PaymentType.Sender:
                    if (Sender == null || string.IsNullOrEmpty(Sender.Account))
                        return new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("when PaymentType is Sender,Sender's Account conn't be null!") });break;
                case TNT.PaymentType.Receiver:
                    if (Receiver == null || string.IsNullOrEmpty(Receiver.Account))
                        return new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("when PaymentType is Receiver,Receiver's Account conn't be null!") });break;
            }
            return null;
        }
    }

    public enum ConTypes
    {
        /// <summary>
        /// NonDoc
        /// </summary>
        N,
        /// <summary>
        /// Doc
        /// </summary>
        D
    }

    public class PartyInfo
    {
        /// <summary>
        /// 公司名称
        /// </summary>
        [Required]
        public string CompanyName { get; set; }
        /// <summary>
        /// First line of address
        /// </summary>
        [Required]
        public string Address1 { get; set; }
        /// <summary>
        /// Second line of address
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// Third line of address
        /// </summary>
        public string Address3 { get; set; }
        /// <summary>
        /// Town name
        /// </summary>
        [Required]
        public string City { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        [Required]
        public string Country { get; set; }
        /// <summary>
        /// Company account upon which collection is to be booked/rated (Must be numeric and be registered against the);
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// Country of account
        /// </summary>
        public string AccountCountry { get; set; }
        /// <summary>
        /// Name of person to be contacted if needed
        /// </summary>
        [Required]
        public string ContactName { get; set; }
        /// <summary>
        /// This person’s dial code
        /// </summary>
        [Required]
        public string ContactDialCode { get; set; }
        /// <summary>
        /// This person’s telephone number
        /// </summary>
        [Required]
        public string ContactTelephone { get; set; }
        /// <summary>
        /// This person’s email address.
        /// </summary>
        public string ContactMail { get; set; }
        /// <summary>
        /// Company VAT number
        /// </summary>
        public string VAT { get; set; }
    }

    public class BetweenTime
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class PackageInfo
    {
        public PackageInfo()
        {
            ArticleInfos = new List<ArticleInfo>();
        }

        /// <summary>
        /// Number of Packages of package type
        /// </summary>
        public double Count { get; set; }
        /// <summary>
        /// Package description
        /// </summary>
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// Length of package
        /// </summary>
        [Required]
        public string Length { get; set; }
        /// <summary>
        /// Height of package
        /// </summary>
        [Required]
        public string Height { get; set; }
        /// <summary>
        /// Weight of package 
        /// </summary>
        [Required]
        public string Weight { get; set; }
        /// <summary>
        /// Width of package
        /// </summary>
        public string Width { get; set; }
        public List<ArticleInfo> ArticleInfos { get; private set; }
    }

    public enum PaymentType { Sender, Receiver }

    public class ArticleInfo
    {
        /// <summary>
        /// Number of items in article
        /// </summary>
        [Required]
        public string Count { get; set; }
        /// <summary>
        /// Article description
        /// </summary>
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// Weight of article
        /// </summary>
        [Required]
        public string Weight { get; set; }
        /// <summary>
        /// Invoice Value of article
        /// </summary>
        [Required]
        public string InvoiceValue { get; set; }
        /// <summary>
        /// Invoice Description of article
        /// </summary>
        [Required]
        public string InvoiceDesc { get; set; }
        /// <summary>
        /// HTS number
        /// </summary>
        public string HTSCode { get; set; }
        /// <summary>
        /// Country of article’s origin
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Export Management Reference Number
        /// </summary>
        public string EMRN { get; set; }
    }
}