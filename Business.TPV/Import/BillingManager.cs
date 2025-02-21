using Business.Service;
using Business.Utils;
using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Import
{
    public class BillingManager : ManagerBase
    {
        public ResultInfo ImportBillingInfo(XmlDocument doc)
        {
            BillingInfo info = OperateData(doc);
            return ImportBillingInfo(info);
        }
        BillingInfo OperateData(XmlDocument doc)
        {
            BillingInfo info = new BillingInfo();
            XmlNode root = doc.SelectSingleNode("BillingInformation");
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "SENDER": info.HeaderInfo.Sender = node.InnerText; break;
                    case "GROUP_ID": info.HeaderInfo.GroupId = node.InnerText; break;
                    case "CMP_ID": info.HeaderInfo.Cmp = node.InnerText; break;
                    case "STN_ID": info.HeaderInfo.STN = node.InnerText; break;
                    case "MSGCODE": info.HeaderInfo.MsgCode = node.InnerText; break;
                    case "BILLING_M": HandleBilling(node, info); break;
                }
            }
            return info;
        }
        void HandleBilling(XmlNode mNode, BillingInfo info)
        {
            foreach (XmlNode node in mNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "SHIP_TYPE": info.ShipType = node.InnerText; break;
                    case "DN_NO": info.DNNO = node.InnerText; break;
                    case "SHIPMENT_ID": info.ShipmentID = node.InnerText; break;
                    case "BL_NO": info.BLNO = node.InnerText; break;
                    case "CARRIER": info.Carrier = node.InnerText; break;
                    case "VESSEL": info.Vessel = node.InnerText; break;
                    case "VOYAGE": info.Voyage = node.InnerText; break;
                    case "ATD_DATE":
                        string str = node.InnerText;
                        if (!string.IsNullOrEmpty(str))
                        {
                            DateTime dt;
                            if (DateTime.TryParse(str, out dt))
                                info.ATD = dt;
                        }
                        break;
                    case "DELIVERY": info.Delivery = node.InnerText; break;
                    case "BILLING_D":
                        DetailInfo dInfo = new DetailInfo();
                        HandleDetail(node, dInfo);
                        info.Details.Add(dInfo);
                        break;
                }
            }
        }
        void HandleDetail(XmlNode mNode, DetailInfo info)
        {
            foreach (XmlNode node in mNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "CTN_TYPE": info.ContainerType = node.InnerText; break;
                    case "QTY": info.QTY = node.InnerText; break;
                    case "UNIT": info.Unit = node.InnerText; break;
                    case "CHG_ITEM": info.ChargeName = node.InnerText; break;
                    case "CHG_CODE": info.ChargeCode = node.InnerText; break;
                    case "CURRENCY": info.Currency = node.InnerText; break;
                    case "AMOUNT": info.Amount = node.InnerText; break;
                }
            }
        }

        public ResultInfo ImportBillingInfo(BillingInfo info)
        {
            return ImportBillingInfoList(new List<BillingInfo>() { info });
        }

        public ResultInfo ImportBillingInfoList(IEnumerable<BillingInfo> infos)
        {
            EntityValidationResult result = null;
            if (!Check<BillingInfo>(infos, ref result))
            {
                return new ResultInfo()
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = string.Join(Environment.NewLine, result.Errors.Select(item => string.Format("{0}:{1}", item.MemberNames, item.ErrorMessage)))
                };
            }
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                eiList.MergeEditInstructList(ToEiList(item));
            }
            return Execute(eiList);
        }

        EditInstructList ToEiList(BillingInfo info)
        {
            return null;
        }
    }


    public class BillingHeaderInfo
    {
        /// <summary>
        /// 發送者
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// 集團代碼
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// 公司代碼
        /// </summary>
        public string Cmp { get; set; }
        /// <summary>
        /// 站別代碼
        /// </summary>
        public string STN { get; set; }
        /// <summary>
        /// 訊息序號
        /// </summary>
        public string MsgCode { get; set; }
    }
    /// <summary>
    /// 请款信息
    /// </summary>
    public class BillingInfo
    {
        public BillingInfo()
        {
            Details = new List<DetailInfo>();
            HeaderInfo = new BillingHeaderInfo();
        }
        /// <summary>
        ///  基本头信息
        /// </summary>
        public BillingHeaderInfo HeaderInfo { get; private set; }
        /// <summary>
        /// 運輸別
        /// </summary>
        public string ShipType { get; set; }
        /// <summary>
        /// DN NO
        /// </summary>
        public string DNNO { get; set; }
        /// <summary>
        /// 運單號
        /// </summary>
        public string ShipmentID { get; set; }
        /// <summary>
        /// 提單號
        /// </summary>
        public string BLNO { get; set; }
        /// <summary>
        /// 委託號
        /// </summary>
        public string EntrustNO { get; set; }
        /// <summary>
        /// 船公司
        /// </summary>
        public string Carrier { get; set; }
        /// <summary>
        /// 船名
        /// </summary>
        public string Vessel { get; set; }
        /// <summary>
        /// 航次
        /// </summary>
        public string Voyage { get; set; }
        /// <summary>
        /// 開航日
        /// </summary>
        public DateTime ATD { get; set; }
        /// <summary>
        /// 交貨地
        /// </summary>
        public string Delivery { get; set; }

        /// <summary>
        /// 请款明细
        /// </summary>
        public List<DetailInfo> Details
        {
            get;
            private set;
        }
    }
    /// <summary>
    /// 请款明细
    /// </summary>
    public class DetailInfo
    {
        /// <summary>
        /// 柜型
        /// </summary> 
        public string ContainerType { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string QTY { get; set; }
        /// <summary>
        /// 計價單位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 费用代码
        /// </summary>
        public string ChargeCode { get; set; }
        /// <summary>
        /// 費用項目名稱
        /// </summary>
        public string ChargeName { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 金額
        /// </summary>
        public string Amount { get; set; }
    }
}
