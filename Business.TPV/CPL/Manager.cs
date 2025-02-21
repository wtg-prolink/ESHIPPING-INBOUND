using Business.EDI;
using Business.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Business.Utils;
using System.Data;
using Models.EDI;
using Models.EDI.CPL;
using Prolink.DataOperation;
using Prolink.Data;
using System.ComponentModel.DataAnnotations;
using TrackingEDI.Business;
using Business.Service;
using Business.TPV.Base;

namespace Business.TPV.CPL
{
    public class ImprotManager : Business.Import.ImportBase
    {
        public ResultInfo ImportBookingResponse(IEnumerable<BookingResponse> list)
        {
            ResultInfo result = null;
            foreach (var item in list)
            {
                if (!Check(item, out result))
                    return result;
            }
            DataTable smDT = QuerySM(list);
            if (!CheckOther(list, smDT, out result)) return result;
            EditInstructList eiList = new EditInstructList();
            foreach (var item in list)
            {
                BackupData(item, string.Format("{0}_{1}", item.CustOrderID, GetCurrentTimeString()));
                bool isBookingConfirm = false;
                EditInstruct ei = ToEi(item, out isBookingConfirm);
                eiList.Add(ei);
            }
            result = Execute(eiList);
            foreach (var item in list)
            {
                DataRow row = GetSMRow(item, smDT);
                CPLBookingResponed log = new CPLBookingResponed(item, row);
                if (result.IsSucceed)
                {
                    Logger.WriteLog(log.CreateSucceed());
                }
                else
                    Logger.WriteLog(log.CreateEx(result.Description));
            }
            ConfirmBooking(list);
            ToTrackingStatus(list,smDT);
            return result;
        }
        void ToTrackingStatus(IEnumerable<BookingResponse> list, DataTable smDT)
        {
            foreach (var item in list)
            {
                DataRow row = GetSMRow(item, smDT);
                CPLTrackingEDILog log = new CPLTrackingEDILog(item, row);
                try
                {
                    SaveToTracking(item);
                    Logger.WriteLog(log.CreateSucceed());
                }
                catch (Exception ex)
                {
                    Logger.WriteLog(log.CreateEx(ex));
                }
            }
        }
        void ConfirmBooking(IEnumerable<BookingResponse> list)
        {
            foreach (var item in list)
            {
                try
                {
                    Helper.ConfirmBooking(item.CustOrderID, item.Company);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog(string.Format("订舱确认异常，Shipment ID：{0}", item.CustOrderID), ex);
                }
            }
        }

        DataRow GetSMRow(BookingResponse info, DataTable smDT)
        {
            if (smDT == null) return null;
            DataRow[] rows = smDT.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(info.CustOrderID)));
            if (rows == null || rows.Length <= 0) return null;
            return rows[0];
        }

        DataTable QuerySM(IEnumerable<BookingResponse> infos)
        {
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID IN({0})",
                string.Join(",", infos.Select(item => SQLUtils.QuotedStr(item.CustOrderID))));
            return DB.GetDataTable(sql, new string[] { });
        }

        EditInstruct ToEi(BookingResponse info, out bool isBookingConfirm)
        {
            isBookingConfirm = false;
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(info.CustOrderID));
            ei.Put("HOUSE_NO", info.BillNumber);
            ei.Put("MASTER_NO", info.BillNumber);
            ei.Put("DRIVER", info.Driver);
            ei.Put("DRIVER_TEL", info.DriverPhone);
            ei.Put("TRUCK_NO", info.CarNubmer);
            switch (info.Status)
            {
                case 1: ei.PutDate("ATD", info.ProcTime); break;
                case 3: ei.PutDate("ATA", info.ProcTime); break;
            }
            ei.PutDate("ETD", info.ETD);
            ei.PutDate("ETA", info.ETA);
            return ei;
        }

        void SaveToTracking(BookingResponse info)
        {
            TraceStatus ts = new TraceStatus();
            Action<DateTime, string, string> addStatus = (eventTime, strsCode, strsDesc) =>
                {
                    ts.AddStatus(new TrackingEDI.Model.Status
                    {
                        EventTime = eventTime.ToString("yyyyMMddHHmmss"),
                        HouseNo = info.BillNumber,
                        ShipmentId = info.CustOrderID,
                        LocationName = info.Location,
                        StsCd = strsCode,
                        StsDescp = strsDesc
                    });
                };
            if (info.ETD != DateTime.MinValue)
                addStatus(info.ETD, "S12", "预计离开");
            if (info.ETA != DateTime.MinValue)
                addStatus(info.ETD, "S43", "预计到达");
            if (info.ProcTime != DateTime.MinValue)
            {
                switch (info.Status)
                {
                    case 1: addStatus(info.ProcTime, "400", "揽件收寄,实际离开（ATD)"); break;
                    case 2: addStatus(info.ProcTime, "466", "派送中"); break;
                    case 3: addStatus(info.ProcTime, "500", "签收，实际到达（ATA）"); break;
                    case 6: addStatus(info.ProcTime, "800", "返单"); break;
                }
            }
            ts.SaveModel(null, TraceModes.EMS);
        }

        bool CheckOther(IEnumerable<BookingResponse> list, DataTable smDT, out ResultInfo result)
        {
            result = null;
            foreach (var item in list)
            {
                DataRow row = GetSMRow(item, smDT);
                if (row == null)
                {
                    result = ToResult(new EntityValidationResult(new List<ValidationResult>() { 
                        new ValidationResult(string.Format("单号：{0} 不存在系统，请确认!",item.CustOrderID)) }));
                    return false;
                }
                string trackWay = Prolink.Math.GetValueAsString(row["TRACK_WAY"]);
                if (trackWay != "F") continue;
                if (string.IsNullOrEmpty(item.Driver))
                {
                    result = ToResult(new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("专车运输，司机名称必输!") }));
                    return false;
                }
                if (string.IsNullOrEmpty(item.CarNubmer))
                {
                    result = ToResult(new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("专车运输，车牌号必输!") }));
                    return false;
                }
                if (string.IsNullOrEmpty(item.DriverPhone))
                {
                    result = ToResult(new EntityValidationResult(new List<ValidationResult>() { new ValidationResult("专车运输，司机电话必输!") }));
                    return false;
                }
            }
            return true;
        }

        bool Check(BookingResponse bookingResponse, out ResultInfo result)
        {
            result = null;
            EntityValidationResult vResult = ValidationHelper.ValidateEntity(bookingResponse);
            if (vResult.HasError)
            {
                result = ToResult(vResult);
                return false;
            }
            return true;
        }

        ResultInfo ToResult(EntityValidationResult result)
        {
            if (result.HasError)
            {
                return new ResultInfo
                {
                    IsSucceed = false,
                    ResultCode = ResultCode.ValidateException,
                    Description = new EntityValidationResultException(result).Message
                };
            }
            return null;
        }
    }

    public class Manager : Export.ShipmentManager
    {
        public ResultInfo SendBooking(Runtime runtime)
        {
            Runtime = runtime;
            if (runtime != null && runtime.OperationMode == OperationModes.Cancel) return null;
            DataRow smRow = QuerySM(runtime);
            if (smRow == null)
                throw new Exception("未找到这笔数据!");
            EDIConfig config = Context.GetEDIConfig(runtime.PartyNo, runtime.Location);
            var result = LogisticsWaybillAdd(runtime, CreateBookingTemplate(runtime, smRow), config);
            WriteEDILog(new CPLBookingEDILog(runtime, smRow), result);
            return result;
        }
        string QueryCustomerName(string shipmentId)
        {
            DataTable dt=QuerySmdnDT(shipmentId);
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow row = dt.Rows[0];
            string source = Prolink.Math.GetValueAsString(row["SAP_ID"]);
            switch (source)
            {
                case "SPMS": return string.Join(string.Empty, "SPMS", Prolink.Math.GetValueAsString(row["DN_TYPE"]));
                default: return Prolink.Math.GetValueAsString(row["CMP"]);
            }
        }

        DataTable QuerySmdnDT(string shipmentId)
        {
            string sql = string.Format("SELECT DN_TYPE,SAP_ID,CMP,DN_NO FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            return dt;
        }
        string GetTransportMode(DataRow smRow)
        {
            string mode = Prolink.Math.GetValueAsString(smRow["TRACK_WAY"]);
            switch (mode)
            {
                case "S": return "22";//班車快運
                case "F": return "20";//專車
                case "T": return "21";//零擔
                case "R": return "1";//鐵路
                case "w": return "8";//江運水路
                case "O": //海運
                case "E": //快遞
                default: return "0";
            }
        }
        PartyInfo GetPartyInfo(string partyCode, DataTable partyDT)
        {
            DataRow[] rows = partyDT.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(partyCode)));
            if (rows == null || rows.Length <= 0) return null;
            DataRow row = rows[0];
            string address = Prolink.Math.GetValueAsString(row["PART_ADDR1"]);
            if (string.IsNullOrEmpty(address))
                address = Prolink.Math.GetValueAsString(row["PART_ADDR2"]);
            if (string.IsNullOrEmpty(address))
                address = Prolink.Math.GetValueAsString(row["PART_ADDR3"]);
            return new PartyInfo
            {
                Company = Prolink.Math.GetValueAsString(row["PARTY_NAME"]),
                Name = Prolink.Math.GetValueAsString(row["PARTY_ATTN"]),
                Mobile = Prolink.Math.GetValueAsString(row["PARTY_TEL"]),
                Address = new Address
                {
                    City = Prolink.Math.GetValueAsString(row["CITY_NM"]),
                    County = Prolink.Math.GetValueAsString(row["CNTY_NM"]),  //区县放在county （没有区县）
                    Country = Prolink.Math.GetValueAsString(row["CNTY_NM"]),
                    Province = Prolink.Math.GetValueAsString(row["STATE"]),
                    Zip = Prolink.Math.GetValueAsString(row["ZIP"]),
                    Value = address
                }
            };
        }

        List<Properties> GetPropertiesInfo(string shipmentId, ref int cntrstdqty,string bandtype,string totalpackage)
        {
            string sql = string.Format(@"SELECT DISTINCT IPART_NO,  GOODS_DESCP ,CNTR_STD_QTY, DN_NO FROM SMDNP WHERE DN_NO IN(
                SELECT DN_NO FROM SMSM WHERE SHIPMENT_ID={0} )", SQLUtils.QuotedStr(shipmentId));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            List<Properties> listpro =new List<Properties>();

            List<string> ipartnos = new List<string>();
            List<string> goodsdescps = new List<string>();
            List<string> dnnos = new List<string>();

            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            foreach (DataRow dr in dt.Rows)
            {
                if (Prolink.Math.GetValueAsInt(dr["CNTR_STD_QTY"]) > 0)
                    cntrstdqty = Prolink.Math.GetValueAsInt(dr["CNTR_STD_QTY"]);
                onAdd(ipartnos,Prolink.Math.GetValueAsString(dr["IPART_NO"]));
                onAdd(goodsdescps,Prolink.Math.GetValueAsString(dr["GOODS_DESCP"]));
                onAdd(dnnos,Prolink.Math.GetValueAsString(dr["DN_NO"]));
            }
            string productinfoen = string.Join(",", goodsdescps);
            if (productinfoen.Length > 500)
                productinfoen = productinfoen.Substring(0, 499);
            string producttypeex = string.Join(",", ipartnos);
            if (producttypeex.Length > 500)
                producttypeex = producttypeex.Substring(0, 499);
            listpro.Add(
                new Properties
                {
                    DnNo = string.Join(",", dnnos),
                    ProductInfoEn = productinfoen,
                    ProductTypeEx = producttypeex,
                    BandType = bandtype,
                    TotalPackage = totalpackage
                });
            return listpro;
        }

        LogisticsWaybillAddTemplate CreateBookingTemplate(Runtime runtime, DataRow smRow)
        {
            LogisticsWaybillAddTemplate template = new LogisticsWaybillAddTemplate();
            template.Company = "";
            template.BLNO = Prolink.Math.GetValueAsString(smRow["HOUSE_NO"]);
            template.CustomerOrderID = runtime.ShipmentID;

            template.CustomerName = QueryCustomerName(runtime.ShipmentID);
            if (string.IsNullOrEmpty(template.CustomerName))
                template.CustomerName = Prolink.Math.GetValueAsString(smRow["CMP"]);
            template.ProductInfo = Prolink.Math.GetValueAsString(smRow["GOODS"]);
            //template.ProductType=;
            //template.ProductCode=;
            template.QTY = Prolink.Math.GetValueAsInt(smRow["QTY"]);
            template.Volume = Prolink.Math.GetValueAsDouble(smRow["CBM"]);
            template.Weight = Prolink.Math.GetValueAsDouble(smRow["GW"]);
            //增加传递BandType 是否饶物流园
            string bandtype = Prolink.Math.GetValueAsString(smRow["BAND_TYPE"]);
            
            template.TransportMode = GetTransportMode(smRow);
            //template.CarType=;
            template.DeliveryClaim = Prolink.Math.GetValueAsString(smRow["INSTRUCTION"]);
            DataTable partyDT = QueryPartyDT(runtime.ShipmentID, new List<string> { PartyCode_ShipTo, PartyCode_Shipper });
            template.Receiver = GetPartyInfo(PartyCode_ShipTo, partyDT);
            PartyInfo shipper = GetPartyInfo(PartyCode_Shipper, partyDT);
            template.Sender = shipper;
            template.Collect = shipper;
            int loadQty = 0 ;
            string totalpackage = Prolink.Math.GetValueAsString(smRow["PKG_NUM"]) + Prolink.Math.GetValueAsString(smRow["PKG_UNIT"]);
            List<Properties> dnpinfo = GetPropertiesInfo(runtime.ShipmentID, ref loadQty, bandtype, totalpackage);
            //loadQty 对于一单有多个装柜量的可以只取其中一个的值
             template.LoadQTY = loadQty;
            template.Properties = dnpinfo;
            return template;
        
        }


        /// <summary>
        /// 物流运单添加
        /// </summary>
        /// <param name="edi"></param>
        /// <returns></returns>
        ResultInfo LogisticsWaybillAdd(Runtime runtime, LogisticsWaybillAddTemplate edi, EDIConfig config)
        {
            XmlDocument doc = edi.ToXml();
            RequstResult result = OnRequst(CPLRequstModes.LogisticsWaybillAdd, doc, config);
            runtime.Data = result.RequstBackupFile;
            string code = string.Empty;
            result.ResultDoc.TryGetSingleNodeValue("rsp_code", out code);
            string msg = string.Empty;
            result.ResultDoc.TryGetSingleNodeValue("rsp_msg", out msg);
            return new ResultInfo { ResultCode = code, Description = string.Format("中邮返回提示:{0},{1}", code, msg), IsSucceed = code == "100" };
        }

        /// <summary>
        /// 商品库存查询
        /// </summary>
        /// <param name="edi"></param>
        /// <returns></returns>
        RequstResult ItemInventoryQuery(LogisticsWaybillAddTemplate edi, EDIConfig config)
        {
            XmlDocument doc = edi.ToXml();
            return OnRequst(CPLRequstModes.ItemInventoryQuery, doc,config);
        }
        /// <summary>
        /// 物流状态推送
        /// </summary>
        /// <param name="edi"></param>
        /// <returns></returns>
        RequstResult LogisticsTracePush(LogisticsWaybillAddTemplate edi, EDIConfig config)
        {
            XmlDocument doc = edi.ToXml();
            return OnRequst(CPLRequstModes.LogisticsTracePush, doc,config);
        }

        RequstResult OnRequst(CPLRequstModes mode, XmlDocument doc, EDIConfig config)
        {
            string url = CreateUrl(mode, config);
            return OnRequst(doc, url);
        }

        static long GetCurrentDateTimeInt()
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTime.Now.Kind);
            return Convert.ToInt64((DateTime.Now - start).TotalSeconds);
        }
        enum CPLRequstModes { ItemInventoryQuery, LogisticsWaybillAdd, LogisticsTracePush }
        static string CreateUrl(CPLRequstModes mode, EDIConfig config)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            //list.Add("api_key", "0123456789");
            list.Add("api_key", config.User);
            list.Add("sign_method", "md5");
            list.Add("timestamp", GetCurrentDateTimeInt().ToString());
            list.Add("v", "1.0");
            //string apiSecret = "123456";
            string apiSecret = config.Psw;
            string md5 = GetMD5(string.Format("{0}{1}{0}", apiSecret, string.Join("", list.Select(item => string.Format("{0}{1}", item.Key, item.Value)))));
            list.Add("sign", md5);
            //string api = "http://218.85.121.126:59081/";
            string api = config.Server;
            string path = string.Empty;
            switch (mode)
            {
                case CPLRequstModes.ItemInventoryQuery: path = "item/inventory/query"; break;
                case CPLRequstModes.LogisticsWaybillAdd: path = "logistics/waybill/add"; break;
                case CPLRequstModes.LogisticsTracePush: path = "logistics/trace/push"; break;
            }
            return string.Format("{0}{1}?{2}", api, path,
                string.Join("&", list.Select(item => string.Format("{0}={1}", item.Key, item.Value))));
        }

        static string GetMD5(string str)
        {
            byte[] result = Encoding.Default.GetBytes(str);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");
        }

        protected override RequstModes Mode
        {
            get { return RequstModes.CPL; }
        }
    }

    /// <summary>
    /// 调用成功时返回数据
    /// </summary>
    class QueryRequestInfo
    {
        public string WarehouseCode { get; set; }
        public string ItemSpec { get; set; }
        public string ItemBarcode { get; set; }
        public string UOM { get; set; }
        public string PageIndex { get; set; }
        public string PageSize { get; set; }
    }

    /// <summary>
    /// 调用失败时返回数据
    /// </summary>
    class ItemInventoryResponse
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public string Inventories { get; set; }
        public string TotalRecord { get; set; }
    }
}
