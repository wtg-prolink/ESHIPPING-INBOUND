using Business.Service;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using TrackingEDI.Business;

namespace Business.TPV.Import
{
    public class TraceManagerBase : ManagerBase
    {
        protected IEnumerable<BLInfo> QueryBLInfo(string condition)
        {
            string sql = string.Format("SELECT CMP,STN,GROUP_ID,CREATE_BY,U_ID,SHIPMENT_ID FROM TKBL WHERE {0}", condition);
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                yield return new BLInfo
                {
                    BLNO = Prolink.Math.GetValueAsString(row["U_ID"]),
                    CMP = Prolink.Math.GetValueAsString(row["CMP"]),
                    STN = Prolink.Math.GetValueAsString(row["STN"]),
                    CreateBy = Prolink.Math.GetValueAsString(row["CREATE_BY"]),
                    GroupID = Prolink.Math.GetValueAsString(row["GROUP_ID"]),
                    ShimentID = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"])
                };
            }
        }

        protected void FillEiForBLInfo(EditInstruct ei, BLInfo blInfo)
        {
            ei.Put("SHIPMENT_ID", blInfo.ShimentID);
            ei.Put("U_ID", blInfo.BLNO);
            ei.Put("CMP", blInfo.CMP);
            ei.Put("STN", blInfo.STN);
            ei.Put("DEP", blInfo.DEP);
            ei.Put("CREATE_BY", blInfo.CreateBy);
            ei.Put("GROUP_ID", blInfo.CreateBy);
        }
    }

    public class BLInfo
    {
        public string CreateBy { get; set; }
        public string BLNO { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
        public string DEP { get; set; }
        public string GroupID { get; set; }
        public string ShimentID { get; set; }
    }

    public abstract class ShipTraceManager<T> : TraceManagerBase where T:TraceInfoBase
    {
        public ResultInfo ImportTraceInfo(XmlDocument doc)
        {
            T info = OperateData(doc);
            return ImportTraceInfo(info);
        }
        protected abstract T OperateData(XmlDocument doc);
        protected abstract EditInstructList ToEiList(T info, IEnumerable<BLInfo> blInfos);

        public ResultInfo ImportTraceInfo(T info)
        {
            return ImportTraceInfoList(new List<T>() { info });
        }
        public ResultInfo ImportTraceInfoList(IEnumerable<T> infos)
        {
            EntityValidationResult result = null;
            if (!Check<T>(infos, ref result))
            {
                return new ResultInfo()
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = string.Join(Environment.NewLine, result.Errors.Select(item => item.ErrorMessage))
                };
            }
            EditInstructList eiList = new EditInstructList();
            List<BLInfo> blInfoList = new List<BLInfo>();
            MixedList ml = new MixedList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                List<BLInfo> itemBLInfoList = GetBLInfo(item).ToList();
                blInfoList.AddRange(itemBLInfoList);
                eiList.MergeEditInstructList(ToEiList(item, itemBLInfoList));
                foreach (var v in itemBLInfoList)
                    EvenFactory.AddOnceEven(v.BLNO, v.BLNO, EvenManager.StatusEven1, ml);
            }
            ResultInfo resultInfo = Execute(eiList);
            if (!result.HasError)
            {
                if (ml.Count > 0)
                {
                    try
                    {
                        Execute(ml);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog("启动货况通知Task异常!", ex);
                    }
                }
            }
            return resultInfo;
        }

        protected IEnumerable<BLInfo> GetBLInfo(TraceInfoBase info)
        {
            return QueryBLInfo(string.Format("MASTER_NO={0} AND HOUSE_NO={1}", SQLUtils.QuotedStr(info.MasterNO), SQLUtils.QuotedStr(info.HouseNO)));
        }
        protected void FillEiForEventInfo(EditInstruct ei, EventInfoBase info)
        {
            ei.Put("STS_CD", info.EventCode);
            ei.Put("STS_DESCP", GetEventDescription(info));
            if (!string.IsNullOrEmpty(info.EventTime))
            {
                DateTime dt;
                if (DateTime.TryParseExact(info.EventTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat,
                    System.Globalization.DateTimeStyles.None, out dt))
                    ei.PutDate("EVEN_DATE", dt);
            }
            ei.Put("LOCATION", info.LocationCode);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("LOCATION_DESCP", info.LocationName);
        }

        string GetEventDescription(EventInfoBase info)
        {
            if (!string.IsNullOrEmpty(info.EventDescription)) return info.EventDescription;
            string sql = string.Format("SELECT EDESCP FROM TKSTSCD WHERE STS_CD={0}", SQLUtils.QuotedStr(info.EventCode));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return string.Empty;
            return Prolink.Math.GetValueAsString(dt.Rows[0]["EDESCP"]);
        }

        protected void HandleEventList(XmlNode node, EventInfoBase info)
        {
            switch (node.Name)
            {
                case "EventCode": info.EventCode = node.InnerText; break;
                case "EventDescription": info.EventDescription = node.InnerText; break;
                case "LocationCode": info.LocationCode = node.InnerText; break;
                case "LocationName": info.LocationName = node.InnerText; break;
                case "EventTime": info.EventTime = node.InnerText; break;
            }
        }

        protected void HandleRoutingInfo(XmlNode node, RoutingInfoBase info)
        {
            switch (node.Name)
            {
                case "OriginCode": info.Origin = node.InnerText; break;
                case "Origin": info.OriginCode = node.InnerText; break;
                case "DestinationCode": info.DestinationCode = node.InnerText; break;
                case "Destination": info.Destination = node.InnerText; break;
                case "ETD": info.ETD = node.InnerText; break;
                case "ETA": info.ETA = node.InnerText; break;
                case "ATD": info.ATD = node.InnerText; break;
                case "ATA": info.ATA = node.InnerText; break;
            }
        }

        protected void HandleHeaderInfo(XmlNode node, TraceInfoBase info)
        {
            switch (node.Name)
            {
                case "SENDER": info.Sender = node.InnerText; break;
                case "GROUP_ID": info.GroupID = node.InnerText; break;
                case "CMP_ID": info.Cmp = node.InnerText; break;
                case "STN_ID": info.STN = node.InnerText; break;
                case "MSGCODE": info.MsgCode = node.InnerText; break;
                case "SHIP_TYPE": info.ShipType = node.InnerText; break;
                case "MASTER_NO": info.MasterNO = node.InnerText; break;
                case "HOUSE_NO": info.HouseNO = node.InnerText; break;
            }
        }
    }
}