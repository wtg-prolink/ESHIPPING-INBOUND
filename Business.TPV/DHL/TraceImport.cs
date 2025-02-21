using Business.Import;
using Business.TPV.Base;
using Business.TPV.Import;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using TrackingEDI.Business;

namespace Business.TPV.DHL
{
    class TraceImport : TpvFtpImportForLineText
    {
        public TraceImport()
        {
            this.AfterBackup += TraceImport_AfterBackup;
            this.OccuredError += TraceImport_OccuredError;
        }

        void TraceImport_OccuredError(object sender, FtpImportEvertArgs e)
        {
            DHLTrackingEDILog log = new DHLTrackingEDILog(e);
            Logger.WriteLog(log.CreateEx(e.EX));
        }

        void TraceImport_AfterBackup(object sender, FtpImportEvertArgs e)
        {
            DHLTrackingEDILog log = new DHLTrackingEDILog(e);
            Logger.WriteLog(log.CreateSucceed());
        }

        protected override bool OperateFile(FtpImportEvertArgs args)
        {
            List<TrackingEDI.Model.Status> status = CreateTraceStatus(args.LocalFileName);
            args.Data = status;
            if (status != null && status.Count > 0)
            {
                TraceStatus ts = new TraceStatus();
                status.ForEach(s => ts.AddStatus(s));
                ts.SaveModel(null, TraceModes.DHL);
            }
            return true;
        }

        List<TrackingEDI.Model.Status> CreateTraceStatus(string filePath)
        {
            TraceStatus ts = new TraceStatus();
            TraceHeader header = null;
            List<TraceDetail> details = new List<TraceDetail>();
            foreach (var line in CreateLines(filePath))
            {
                Tuple<LineType, string[]> val = ParseToArray(line);
                if (val == null) continue;
                switch (val.Item1)
                {
                    case LineType.H: header = CreateTraceHeader(val.Item2); continue;
                    case LineType.D:
                        details.Add(CreateDetailInfo(val.Item2));
                        break;
                }
            }
            if (details.Count <= 0) return null;
            string sql = string.Format("SELECT SHIPMENT_ID,HOUSE_NO FROM SMSM WHERE HOUSE_NO IN({0})", string.Join(",",
                details.Select(t => t.HouseNO).Distinct().Select(c => SQLUtils.QuotedStr(c))));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            Func<TraceDetail, string> checkSM = td =>
            {
                if (string.IsNullOrEmpty(td.HouseNO)) return null;
                DataRow[] rows = dt.Select(string.Format("HOUSE_NO={0}", SQLUtils.QuotedStr(td.HouseNO)));
                if (rows == null || rows.Length <= 0) return null;
                return Prolink.Math.GetValueAsString(rows[0]["SHIPMENT_ID"]);
            };
            List<TrackingEDI.Model.Status> list = new List<TrackingEDI.Model.Status>();
            details.ForEach(td =>
                {
                    string shipmentId = checkSM(td);
                    if (!string.IsNullOrEmpty(shipmentId))
                        list.Add(CreateStatus(header, td, shipmentId));
                });
            return list;
        }

        TrackingEDI.Model.Status CreateStatus(TraceHeader header, TraceDetail td,string shipmentId)
        {
            return new TrackingEDI.Model.Status
            {                
                EventTime = td.EventDate.ToString("yyyyMMddHHmmss"),
                ShipmentId=shipmentId,
                HouseNo = td.HouseNO,
                LocationName = td.Destination,
                StsCd = td.StatusCode,
                StsDescp = td.StatusDescription
            };
        }

        TraceHeader CreateTraceHeader(string[] strs)
        {
            return new TraceHeader
            {
                Message = GetValue(strs, 1),
                Version = GetValue(strs, 2),
                VersionShort = GetValue(strs, 3),
                SenderCode = GetValue(strs, 4),
                SenderAddress = GetValue(strs, 5),
                SenderReverseAddress = GetValue(strs, 6),
                RecipientCode = GetValue(strs, 7),
                RecipientAddress = GetValue(strs, 8),
                RecipientAdditional = GetValue(strs, 9),
                CustomerVANAddress = GetValue(strs, 10),
                VendorRef = GetValue(strs, 11),
                EMail = GetValue(strs, 12),
                DocDateTime = GetValue(strs, 13),
                RefNO = GetValue(strs, 14),
                IsTest = Prolink.Math.GetValueAsBool(GetValue(strs, 15), false)
            };
        }

        TraceDetail CreateDetailInfo(string[] strs)
        {
            Func<int, DHLPartyInfo> getPtInfo = index =>
            {
                return new DHLPartyInfo
                {
                    Name = GetValue(strs, index),
                    Contact = GetValue(strs, index + 1),
                    Address = GetValue(strs, index + 2),
                    Address2 = GetValue(strs, index + 3),
                    Address3 = GetValue(strs, index + 4),
                    City = GetValue(strs, index + 5),
                    ZIP = GetValue(strs, index + 6),
                    Country = GetValue(strs, index + 7),
                    Tel = GetValue(strs, index + 8)
                };
            };
            DateTime eventDT = DateTime.MinValue;
            DateTime pickupDT = DateTime.MinValue;
            try
            {
                string eventStr = string.Format("{0} {1}", GetValue(strs, 9), GetValue(strs, 10));
                DateTime.TryParse(eventStr, out eventDT);
                string pickupStr = GetValue(strs, 11);
                if (!string.IsNullOrEmpty(pickupStr))
                    DateTime.TryParse(pickupStr, out pickupDT);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("DHL Trace Error!", ex);
            }
            return new TraceDetail
            {
                UniqueID = GetValue(strs, 1),
                StatusCode = GetValue(strs, 2),
                StatusDescription = GetValue(strs, 3),
                Weight = GetValue(strs, 4),
                WeightUnit = GetValue(strs, 5),
                QTY = GetValue(strs, 6),
                ShipperRef = GetValue(strs, 7),
                HouseNO = GetValue(strs, 8),
                EventDate = eventDT,
                PickupDate = pickupDT,
                Destination = GetValue(strs, 12),
                Origin = GetValue(strs, 13),
                TransshipmentPoint = GetValue(strs, 14),
                ProductCode = GetValue(strs, 15),
                ShipperAccount = GetValue(strs, 16),
                PayerAccount = GetValue(strs, 17),
                Shipper = getPtInfo(18),
                Consignee = getPtInfo(27),
                Signatory = GetValue(strs, 37)
            };
        }

        enum LineType { H, D }
        Tuple<LineType, string[]> ParseToArray(string line)
        {
            string[] strs = line.Split(new string[] { "|" }, StringSplitOptions.None);
            switch (strs[0])
            {
                case "H": return new Tuple<LineType, string[]>(LineType.H, strs);
                case "D": return new Tuple<LineType, string[]>(LineType.D, strs);
            }
            return null;
        }

        protected override string FileName
        {
            get { return "DHL"; }
        }

        protected override string ConfigNodeName
        {
            get
            {
                return "trace";
            }
        }
    }

    class TraceHeader
    {
        public string Message { get; set; }
        public string Version { get; set; }
        public string VersionShort { get; set; }
        public string SenderCode { get; set; }
        public string SenderAddress { get; set; }
        public string SenderReverseAddress { get; set; }
        public string RecipientCode { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientAdditional { get; set; }
        public string CustomerVANAddress { get; set; }
        public string VendorRef { get; set; }
        public string EMail { get; set; }
        public string DocDateTime { get; set; }
        public string RefNO { get; set; }
        public bool IsTest { get; set; }
    }
    class TraceDetail
    {
        public string UniqueID { get; set; }
        public string StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Weight { get; set; }
        public string WeightUnit { get; set; }
        public string QTY { get; set; }
        public string ShipperRef { get; set; }
        public string HouseNO { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime PickupDate { get; set; }
        public string Destination { get; set; }        
        public string Origin { get; set; }
        public string TransshipmentPoint { get; set; }
        public string ProductCode { get; set; }
        public string ShipperAccount { get; set; }
        public string PayerAccount { get; set; }
        public DHLPartyInfo Shipper { get; set; }
        public DHLPartyInfo Consignee { get; set; }
        public string Signatory { get; set; }
    }

    class DHLPartyInfo
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string Country { get; set; }
        public string Tel { get; set; }
    }
}