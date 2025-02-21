using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Xml;

namespace Business.TPV.Import
{
    public class DNManager : ManagerBase
    {
        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "SMDN", "SMDNP", "SMDNPT", "SMINP", "SMINM", "SMIND", "SMSIM" };
        }
        public ResultInfo ImportDN(DNInfo info, bool forced = false)
        {
            return ImportDNList(new DNInfo[] { info }, forced);
        }
        public ResultInfo ImportDNList(DNInfo[] infos, bool forced = false)
        {
            return ImportDNList(infos, DNImportModes.ReloadDefault, forced);
        }

        public EditInstructList ImportAsusForSAP(string SoNo, string location, string dnNo)
        {
            AsusEDI asusEDI = new AsusEDI();
            List<AsusInfo> info = null;
            try
            {
                info = asusEDI.GetAsusInfo(SoNo, location, dnNo).ToList();
            }
            catch (Exception ex) { ex.ToString(); }

            return RemarkSave(info, dnNo);
        }

        EditInstructList RemarkSave(List<AsusInfo> asusInfos, string dnNo)
        {
            if (asusInfos == null || asusInfos.Count <= 0)
                return null;
            AsusInfo asusInfo = asusInfos[0];
            int count = int.Parse(asusInfo.VERSION);
            if (asusInfos.Count > 0)
            {
                foreach (var s in asusInfos)
                {
                    if (int.Parse(s.VERSION) > count)
                        asusInfo = s;
                }
            }
            string DnRmark = string.Empty, AcRemark = string.Empty;
            EditInstructList eiList = new EditInstructList();
            eiList.Add(ParseAsusExport(asusInfo));

            AcRemark += "PO:" + asusInfo.PO + "-" + asusInfo.POLINE + "\n";
            AcRemark += "DN:" + asusInfo.DN + "-" + asusInfo.DNLINE + "\n";
            AcRemark += "SO:" + asusInfo.SO + "-" + asusInfo.SOLINE + "\n";
            DnRmark += "DN:" + asusInfo.DN + "\n";
            DnRmark += "PO:" + asusInfo.PO + "\n";
            DnRmark += "SO:" + asusInfo.SO + "/" + asusInfo.SOLINE + "\n";
            DnRmark += "CO:" + asusInfo.POCUS + "\n";
            DnRmark += "Destination:" + asusInfo.DEST + "\n";
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("DN_NO={0}", SQLUtils.QuotedStr(dnNo));
            ei.Put("AC_REMARK", AcRemark);
            ei.Put("DN_RMARK", DnRmark);
            eiList.Add(ei);
            return eiList;
        }
        EditInstruct ParseAsusExport(AsusInfo asusInfos)
        {
            string uId = System.Guid.NewGuid().ToString();
            string sql = string.Format("DELETE FROM ASUS_EDI_EXPORT WHERE DN_NO={0}", SQLUtils.QuotedStr(asusInfos.DN_NO));
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct asusei = new EditInstruct("ASUS_EDI_EXPORT", EditInstruct.INSERT_OPERATION);
            asusei.Put("U_ID", uId);
            asusei.Put("SOURCE_NO", asusInfos.SOURCE_NO);
            asusei.Put("ITEM_NUM", asusInfos.ITEM_NUM);
            asusei.Put("VERSION", asusInfos.VERSION);
            asusei.Put("PO", asusInfos.PO);
            asusei.Put("PO_LINE", asusInfos.POLINE);
            asusei.Put("SO", asusInfos.SO);
            asusei.Put("SO_LINE", asusInfos.SOLINE);
            asusei.Put("DN", asusInfos.DN);
            asusei.Put("DN_LINE", asusInfos.DNLINE);
            asusei.Put("PO_CUS", asusInfos.POCUS);
            asusei.Put("DESTINATION", asusInfos.DEST);
            asusei.Put("DN_NO", asusInfos.DN_NO);
            return asusei;
        }
        static object _lockObj = new object();
        public ResultInfo ImportDNList(DNInfo[] infos, DNImportModes mode, bool forced = false)
        {
            try
            {
                lock (_lockObj)
                {
                    ResultInfo result = null;
                    foreach (var item in infos)
                    {
                        if (!CheckDNInfo(item, out result))
                        {
                            WriteLogEx(result, infos, "转入规格验证不通过");
                            return result;
                        }
                        if (!FilterDNInfo(item, out result))
                        {
                            WriteLogEx(result, infos, "DN转入规格验证不通过");
                            return SucceedResult();
                        }
                    }

                    List<Tuple<DNInfo, ResultInfo>> workResult = new List<Tuple<DNInfo, ResultInfo>>();
                    foreach (var item in infos)
                    {
                        EditInstructList eiList = new EditInstructList();
                        string fileName = BackupData(item, string.Join("_", item.HeaderInfo.DNNOWithCompanyCode, GetCurrentTimeString()));
                        ParseContext context = new ParseContext();
                        context.ELog = new ImportDNEDILog(item);
                        context.DNInfo = item;
                        EditInstructList itemEiList = ToEiList(context, mode, forced);
                        for (int i = 0; i < itemEiList.Count; i++)
                            eiList.Add(itemEiList[i]);
                        var v = Execute(eiList);
                        if (!v.IsSucceed)
                        {
                            WriteLogEx(v, infos);
                        }
                        else
                        {
                            Logger.WriteLog(context.ELog.CreateSucceed("", fileName));
                        }
                        ToBooking(context);
                        workResult.Add(new Tuple<DNInfo, ResultInfo>(item, v));
                    }
                    var items = workResult.Where(x => infos.Contains(x.Item1)).ToList();
                    workResult.RemoveAll(x => infos.Contains(x.Item1));
                    if (items == null || items.Count <= 0) return SucceedResult();
                    var errorResult = items.Where(n => (n.Item2 != null && !n.Item2.IsSucceed)).ToList();
                    if (errorResult == null || errorResult.Count <= 0) return SucceedResult();
                    return new ResultInfo
                    {
                        ResultCode = "Error",
                        Description = string.Join(Environment.NewLine,
                            errorResult.Select(n => string.Format("{0}:{1},{2}", n.Item1.HeaderInfo.DNNOWithCompanyCode, n.Item2.ResultCode, n.Item2.Description)))
                    };
                }
            }
            catch (Exception ex)
            {
                var result = new ResultInfo { IsSucceed = false, ResultCode = ResultCode.UnKnow, Description = ex.Message };
                WriteLogEx(result, infos, ex);
                return result;
            }
        }


        void WriteLogEx(ResultInfo r, DNInfo[] infos, Exception ex = null)
        {
            WriteLogEx(r, infos, ex == null ? string.Empty : ex.ToString());
        }

        void WriteLogEx(ResultInfo r, DNInfo[] infos, string msg)
        {
            try
            {
                DNInfo info = null;
                if (infos != null && infos.Length > 0)
                {
                    info = infos.Where(n => n.HeaderInfo != null && !string.IsNullOrEmpty(n.HeaderInfo.DNNOWithCompanyCode)).FirstOrDefault();
                }
                Logger.WriteLog(new ImportDNEDILog(info).CreateEx(string.Join(",", msg, r.Description)));
            }
            catch
            {

            }
        }

        ResultInfo ToBooking(ParseContext info)
        {
            Func<ResultInfo> noBookingResult = () => new ResultInfo { IsSucceed = true, ResultCode = "NoBooking", Description = "该笔不直接订舱!" };
            if (string.IsNullOrEmpty(info.DNInfo.HeaderInfo.RefDNNOWithCompanyCode)) return noBookingResult();
            if (!string.IsNullOrEmpty(info.DNInfo.HeaderInfo.RefDNNO)) return noBookingResult();
            if (!string.IsNullOrEmpty(info.DNInfo.HeaderInfo.RefCompanyCode)) return noBookingResult();
            if (info.DNProfileInfo == null || string.IsNullOrEmpty(info.DNProfileInfo.Vendor)) return noBookingResult();
            TrackingEDI.Business.TransferBooking b = new TrackingEDI.Business.TransferBooking();
            string userId = info.HeaderEI.Get("CREATE_BY");
            try
            {
                b.SaveToBooking(info.Uid, userId);
            }
            catch (Exception ex)
            {
                var l = Logger.CreateLog("DN直接转入Booking发生异常", this.GetType().Name, info.DNInfo.HeaderInfo.DNNOWithCompanyCode, "", ex.ToString());
                Logger.WriteLog(l);
            }
            return SucceedResult();
        }
        bool FilterDNInfo(DNInfo info, out ResultInfo result)
        {
            result = UnknowResult();
            if (info.HeaderInfo.SalesDocumentType == "ZBV")
            {
                result.ResultCode = "DN_FILTER";
                result.Description = "Sales document type equal ZBV,not into system!";
                return false;
            }
            if (info.HeaderInfo.MeansOfTransportType == "T009")
            {
                result.ResultCode = "DN_FILTER";
                result.Description = "Means of transport type equal T009,not into system!";
                return false;
            }
            if (info.HeaderInfo.MeansOfTransportType == "T005")
            {
                result.ResultCode = "DN_FILTER";
                result.Description = "Means of transport type equal T005,not into system!";
                return false;
            }
            if (info.HeaderInfo.MeansOfTransportType == "T007")
            {
                result.ResultCode = "DN_FILTER";
                result.Description = "Means of transport type equal T007,not into system!";
                return false;
            }
            if (info.HeaderInfo.SourceSAP == "SPMS")
            {
                result.ResultCode = "DN_FILTER";
                result.Description = "SPMS not into system!";
                return false;
            }
            string SapId = WebConfigurationManager.AppSettings["FILTER_SAPID"];
            string sapId = info.HeaderInfo.SourceSAP.Substring(0, 3);
            string[] saps = SapId.Split(';');
            foreach (var sap in saps)
            {
                if (sapId.Equals(sap))
                    return true;
            }
            result.ResultCode = "DN_FILTER";
            result.Description = info.HeaderInfo.SourceSAP + " not into system!";
            return false;
        }

        bool CheckDNInfo(DNInfo info, out ResultInfo result)
        {
            result = new ResultInfo() { IsSucceed = false };
            if (info == null)
            {
                result.ResultCode = ResultCode.DataIsNull;
                result.Description = ResultCode.GetDescription(ResultCode.DataIsNull);
                return false;
            }
            if (info.HeaderInfo == null)
            {
                result.ResultCode = ResultCode.ColumnValueIsNull;
                result.Description = "HeaderInfo conn't be null.";
                return false;
            }
            if (string.IsNullOrEmpty(info.HeaderInfo.DNNOWithCompanyCode))
            {
                result.ResultCode = ResultCode.ColumnValueIsNull;
                result.Description = "DN NO is Required!";
                return false;
            }
            return true;
        }

        Dictionary<string, List<string>> GetDNCanoverrideColumns(DNImportModes mode)
        {
            XmlDocument doc = Context.GetSysColumnsDoc();
            if (doc == null) return null;
            string nodeName = string.Empty;
            switch (mode)
            {
                case DNImportModes.ReloadQuantity: nodeName = "ReloadQuantity"; break;
                default: nodeName = "Default"; break;
            }
            XmlNodeList nodes = doc.SelectNodes(string.Format("//DNCanOverride/{0}", nodeName));
            if (nodes == null || nodes.Count <= 0) return null;
            Dictionary<string, List<string>> columns = new Dictionary<string, List<string>>();
            foreach (XmlNode n in nodes)
            {
                foreach (XmlNode item in n.ChildNodes)
                {
                    if (string.IsNullOrEmpty(item.InnerText)) continue;
                    var att = item.Attributes["table"];
                    if (att == null) continue;
                    string attV = att.Value;
                    if (string.IsNullOrEmpty(attV)) continue;
                    if (!columns.ContainsKey(attV))
                        columns.Add(attV, new List<string>());
                    var list = columns[attV];
                    if (!list.Contains(item.InnerText))
                        list.Add(item.InnerText);
                }
            }
            return columns;
        }

        EditInstructList ToEiList(ParseContext context, DNImportModes mode, bool forced = false)
        {
            DNInfo dnInfo = context.DNInfo;
            EditInstructList eiList = new EditInstructList();
            string uid = string.Empty;
            context.SIApply = new SI.ApplyBase();
            string dnLevel = "1st";
            string sql = string.Format(@" ;with COM_REF_DATA AS(
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN where DN_NO = {0}
union all
select SMDN.DN_NO,SMDN.REF_NO,SMDN.COMBINE_INFO,SMDN.WE_NM,SMDN.PLANT,SMDN.ETD,SMDN.DN_NO_CMP_REF from SMDN inner join COM_REF_DATA r on  r.DN_NO_CMP_REF = SMDN.DN_NO and SMDN.DN_NO <> r.DN_NO
)select  * from COM_REF_DATA   where DN_NO !={0}", SQLUtils.QuotedStr(dnInfo.HeaderInfo.DNNOWithCompanyCode));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count == 1)
                dnLevel = "2st";
            if (dt.Rows.Count == 2)
                dnLevel = "3rd";
            if (dt.Rows.Count == 3)
                dnLevel = "4th";

            context.DNProfileInfo = Helper.QuerySIProfileInfo(dnInfo.HeaderInfo.ProfileCode, null, dnLevel);
            context.PartyDT = QueryParty(dnInfo, context);
            DataRow row = null;
            context.Operation = GetOperation(dnInfo, out row);
            EditInstruct hedaderEI = ParseHeader(context);
            if (row != null)
                context.Uid = Prolink.Math.GetValueAsString(row["U_ID"]);
            context.HeaderEI = hedaderEI;
            //string sapId = context.DNInfo.HeaderInfo.SourceSAP.Substring(0,3);
            //if (sapId.Equals("TPV"))
            //    return eiList;
            eiList.Add(hedaderEI);
            eiList.MergeEditInstructList(ParsePartner(context));
            eiList.MergeEditInstructList(ParseDetail(context));
            eiList.MergeEditInstructList(ParsePallets(eiList, context));
            eiList.MergeEditInstructList(CreateUpSM(context, row));
            eiList.MergeEditInstruct(CreateUpSI(context));
            eiList.MergeEditInstruct(CreateUpFirstDN(dnInfo));
            if ("0006000017".Equals(context.HeaderEI.Get("FC_CD").ToString()))
                eiList.MergeEditInstructList(UpDn(context));

            if (!forced)
            {
                if (context.Operation == EditInstruct.UPDATE_OPERATION)
                {
                    return FileterUpEiList(eiList, mode, row);
                }
            }
            return eiList;
        }

        class ParseContext
        {
            public DNInfo DNInfo { get; set; }
            public int Operation { get; set; }
            public DataTable PartyDT { get; set; }
            public string Uid { get; set; }
            public SIProfileInfo DNProfileInfo { get; set; }
            public EditInstruct HeaderEI { get; set; }
            public SI.ApplyBase SIApply { get; set; }
            public ImportDNEDILog ELog { get; set; }
        }

        EditInstruct CreateUpSI(ParseContext context)
        {
            if (string.IsNullOrEmpty(context.DNInfo.HeaderInfo.OriginalCompany)) return null;
            if (context.DNProfileInfo == null) return null;
            if (!string.IsNullOrEmpty(context.DNProfileInfo.ISFSellerCode)) return null;
            DataRow row = GetISFSellerRow(context);
            if (row == null) return null;
            EditInstruct ei = new EditInstruct("SMSIM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("PROFILE={0}", SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.ProfileCode));
            ei.Put("IM_RECORD", row["IM_RECORD"]);
            ei.Put("ISF_SELLER", row["PARTY_NO"]);
            ei.Put("ISF_SELLERNM", row["PARTY_NAME"]);
            ei.Put("ISF_SELLERADDR1", row["PART_ADDR1"]);
            ei.Put("ISF_SELLERADDR2", row["PART_ADDR2"]);
            ei.Put("ISF_SELLERADDR3", row["PART_ADDR3"]);
            ei.Put("ISF_SELLERCITY", row["CITY"]);
            ei.Put("ISF_SELLERCITY_NM", row["CITY_NM"]);
            ei.Put("ISF_SELLERSTATE", row["STATE"]);
            ei.Put("ISF_SELLERCNTY", row["CNTY"]);
            ei.Put("ISF_SELLERZIP", row["ZIP"]);
            ei.Put("ISF_SELLERCNTY_NM", row["CNTY_NM"]);
            return ei;
        }
        DataRow GetISFSellerRow(ParseContext context)
        {
            DataRow[] rows = GetPartyRows(context, context.DNInfo.HeaderInfo.OriginalCompany);
            if (rows == null || rows.Length <= 0) return null;
            var v = rows.Where(row => Prolink.Math.GetValueAsString(row["PARTY_TYPE"]).Contains("SL")).FirstOrDefault();
            if (v != null) return v;
            return rows[0];
        }

        EditInstructList CreateUpSM(ParseContext context, DataRow dr)
        {
            if (dr == null) return null;
            string shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            if (string.IsNullOrEmpty(shipmentid))
                return null;
            EditInstructList eiList = new EditInstructList();
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO={0})",
               SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.DNNOWithCompanyCode));
            ei.Put("APPROVE_NO", context.DNInfo.HeaderInfo.ApprovalNO);
            ei.Put("EXPORT_NO", context.DNInfo.HeaderInfo.ExportNO);
            var declDate = ParseDateTime(context.DNInfo.HeaderInfo.DeclarationDate);
            if (declDate.HasValue)
                ei.PutDate("DECL_DATE", declDate.Value);
            string edeclno = context.DNInfo.HeaderInfo.DeclarationNO;
            if (!string.IsNullOrEmpty(edeclno))
                ei.Put("EDECL_NO", edeclno);
            var postflagdate = ParseDateTime(context.DNInfo.HeaderInfo.ActualGoodsMovementDate);
            if (postflagdate.HasValue)
                ei.PutDate("POST_FLAG_DATE", postflagdate.Value);
            else
            {
                ei.PutDate("POST_FLAG_DATE", null);
            }
            eiList.Add(ei);
            if (postflagdate.HasValue)
            {
                EditInstruct cbei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                cbei.Condition = string.Format("SHIPMENT_ID IN (SELECT COMBIN_SHIPMENT FROM SMSM WHERE SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO={0}))",
                     SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.DNNOWithCompanyCode));
                cbei.PutDate("POST_FLAG_DATE", postflagdate.Value);
                eiList.Add(cbei);
            }
            return eiList;
        }

        EditInstruct CreateUpFirstDN(DNInfo dnInfo)
        {
            if (string.IsNullOrEmpty(dnInfo.HeaderInfo.RefDNNOWithCompanyCode)) return null;
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("DN_NO={0}", SQLUtils.QuotedStr(dnInfo.HeaderInfo.RefDNNOWithCompanyCode));
            ei.Put("REF_NO", dnInfo.HeaderInfo.DNNOWithCompanyCode);
            return ei;
        }

        EditInstructList FileterUpEiList(EditInstructList eiList, DNImportModes mode, DataRow row)
        {
            Func<Func<EditInstructList, EditInstruct, bool>, EditInstructList> filter = cusHandler =>
            {
                Dictionary<string, List<string>> columns = GetDNCanoverrideColumns(mode);
                EditInstructList upEiList = new EditInstructList();
                if (eiList == null || eiList.Count <= 0) return eiList;
                for (int i = 0; i < eiList.Count; i++)
                {
                    EditInstruct ei = eiList[i];
                    if (cusHandler != null)
                    {
                        if (cusHandler(upEiList, ei)) continue;
                    }
                    if (columns == null || columns.Count <= 0) continue;
                    if (!columns.ContainsKey(ei.ID)) continue;
                    string[] names = ei.getNameSet();
                    List<string> list = columns[ei.ID];
                    foreach (var name in names)
                    {
                        if (list.Contains(name)) continue;
                        ei.Remove(name);
                    }
                    string[] resultNames = ei.getNameSet();
                    if (resultNames != null && resultNames.Length > 0)
                        upEiList.Add(ei);
                }
                return upEiList;
            };
            switch (mode)
            {
                case DNImportModes.ReloadQuantity:
                    //return filter(null);
                    return filter((upEiList, ei) =>
                    {
                        switch (ei.ID)
                        {
                            case DNTableD:
                            case "SMIND":
                            case "SMINP":
                                upEiList.Add(ei);
                                return true;
                            default: return false;
                        }
                    });
                default:
                    string appTo = Prolink.Math.GetValueAsString(row["APPROVE_TO"]);
                    string status = Prolink.Math.GetValueAsString(row["STATUS"]);
                    if (appTo != "A" || (status != "T" && status != "D" && !string.IsNullOrEmpty(status)))
                        //return filter(null);
                        return filter((upEiList, ei) =>
                        {
                            switch (ei.ID)
                            {
                                case "ASUS_EDI_EXPORT":
                                    upEiList.Add(ei);
                                    return true;
                                //case DNTableD:
                                //case "SMIND":
                                //case "SMINP":
                                //    upEiList.Add(ei);
                                //    return true;
                                default: return false;
                            }
                        });
                    else return eiList;
            }
        }

        EditInstructList UpDn(ParseContext context)
        {
            string dnNo = context.DNInfo.HeaderInfo.DNNOWithCompanyCode;
            string SoNo = context.DNInfo.ItemInfos[0].CustomerPurchaseOrderNumber;
            string cmp = context.DNInfo.HeaderInfo.CompanyCode;
            //string sapId = context.DNInfo.HeaderInfo.SourceSAP;
            return ImportAsusForSAP(SoNo, cmp, dnNo);
        }


        int GetOperation(DNInfo info, out DataRow row)
        {
            row = null;
            string sql = string.Format("SELECT * FROM SMDN WHERE SAP_ID={0} AND DN_NO={1}", SQLUtils.QuotedStr(info.HeaderInfo.SourceSAP),
                SQLUtils.QuotedStr(info.HeaderInfo.DNNOWithCompanyCode));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return EditInstruct.INSERT_OPERATION;
            row = dt.Rows[0];
            return EditInstruct.UPDATE_OPERATION;
        }
        DataTable QueryParty(DNInfo info, ParseContext context)
        {
            if (info.PartnerInfos == null || info.PartnerInfos.Length <= 0) return null;
            List<string> partyCodes = info.PartnerInfos.Select(item => item.PartnerNumber).ToList();
            Action<string> add = partyNO =>
            {
                if (string.IsNullOrEmpty(partyNO)) return;
                if (partyCodes.Contains(partyNO)) return;
                partyCodes.Add(partyNO);
            };
            string plant = GetPlant(info);
            add(plant);
            string shipperPartyNO = GetShipperCode(info);
            add(shipperPartyNO);
            add(info.HeaderInfo.OriginalCompany);
            return Helper.QueryPartyDT(partyCodes);
        }

        DataTable QueryPartyType(DNInfo info)
        {
            if (info.PartnerInfos == null || info.PartnerInfos.Length <= 0) return null;
            return Helper.QueryPartyType();
        }
        string GetCompany(DNInfo dnInfo, string refno)
        {
            return Helper.GetCompany(dnInfo.HeaderInfo.SourceSAP, GetPlant(dnInfo), refno);
            //string sql = string.Format("SELECT HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(dnInfo.HeaderInfo.CompanyCode));
            //string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TSAP' AND CD={0}", SQLUtils.QuotedStr(dnInfo.HeaderInfo.SourceSAP));
            //return DB.GetValueAsString(sql);
        }
        string GetPlant(DNInfo dnInfo)
        {
            if (dnInfo.HeaderInfo.SourceSAP == SPMS) return dnInfo.HeaderInfo.ReceivingPlant;
            if (dnInfo.ItemInfos == null || dnInfo.ItemInfos.Length <= 0) return null;
            return dnInfo.ItemInfos[0].Plant;
        }
        class DNUserInfo
        {
            public string Dep { get; set; }
            public string Ext { get; set; }
            public string UserID { get; set; }
        }
        DNUserInfo GetDNUserInfo(DNInfo dnInfo)
        {
            DataRow row = GetFirstDNRow(dnInfo);
            if (row != null)
            {
                return new DNUserInfo
                {
                    Dep = Prolink.Math.GetValueAsString(row["DEP"]),
                    Ext = Prolink.Math.GetValueAsString(row["CREATE_EXT"]),
                    UserID = Prolink.Math.GetValueAsString(row["CREATE_BY"])
                };
            }
            string sql = string.Format("SELECT DEP,U_EXT,U_ID FROM SYS_ACCT WHERE SAP_ID LIKE '%{0}%'", dnInfo.HeaderInfo.CreateBy);
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            row = dt.Rows[0];
            return new DNUserInfo
            {
                Dep = Prolink.Math.GetValueAsString(row["DEP"]),
                Ext = Prolink.Math.GetValueAsString(row["U_EXT"]),
                UserID = Prolink.Math.GetValueAsString(row["U_ID"])
            };
        }
        DataRow GetFirstDNRow(DNInfo dnInfo)
        {
            if (string.IsNullOrEmpty(dnInfo.HeaderInfo.RefDNNOWithCompanyCode)) return null;
            DataTable dt = Helper.QueryMultiSegmentsDNInfo(dnInfo.HeaderInfo.RefDNNOWithCompanyCode, new List<string> { " CREATE_EXT", "DEP", "CREATE_BY" });
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow[] rows = dt.Select("DN_NO_CMP_REF IS NULL");
            if (rows == null || rows.Length <= 0) return null;
            return rows[0];
        }

        string GetFirstSMINDRow(DNInfo dnInfo, EditInstruct smindei, string topcolumn)
        {
            if (string.IsNullOrEmpty(dnInfo.HeaderInfo.RefDNNOWithCompanyCode)) return null;
            string sql = string.Format("SELECT TOP 1 " + topcolumn + " FROM SMIND WHERE DN_NO = {0} AND IPART_NO={1} AND OHS_CODE={2}",
                SQLUtils.QuotedStr(dnInfo.HeaderInfo.RefDNNOWithCompanyCode),
                SQLUtils.QuotedStr(smindei.Get("IPART_NO")), SQLUtils.QuotedStr(smindei.Get("OHS_CODE")));
            string unitprice1 = DB.GetValueAsString(sql);
            return unitprice1;
        }

        string GetShipMark(DNInfo info)
        {
            List<string> items = new List<string>();
            Action<string> add = mark =>
            {
                if (string.IsNullOrEmpty(mark)) return;
                if (!items.Contains(mark))
                    items.Add(mark);
            };
            add(info.HeaderInfo.Mark1);
            add(info.HeaderInfo.Mark2);
            add(info.HeaderInfo.Mark3);
            add(info.HeaderInfo.Mark4);
            add(info.HeaderInfo.Mark5);
            add(info.HeaderInfo.Mark6);
            return string.Join(Environment.NewLine, items);
        }

        Tuple<string, string> GetGoods(DNInfo info, SIProfileInfo pInfo, string dep)
        {
            //if (info.HeaderInfo.Division == "30") return new Tuple<string, string>("Spare parts/维修零件", "Spare parts/维修零件");
            switch (info.HeaderInfo.MaterialType)
            {
                case "HALB":
                    if (dep == "CS")
                        return new Tuple<string, string>("Spare parts/维修零件", "Spare parts/维修零件");
                    else
                        return GoodsFolowItems(info);
                case "ROH":
                    return new Tuple<string, string>("Spare parts/维修零件", "Spare parts/维修零件");
                default:
                    return GoodsFolowItems(info);
            }
        }

        private static Tuple<string, string> GoodsFolowItems(DNInfo info)
        {
            if (info.ItemInfos == null || info.ItemInfos.Length <= 0) return null;
            var items = info.ItemInfos.Where(item => item.DeliveryItemCategory != SkipItemType);
            var v = items.Select(item => new Tuple<string, string>(item.ShortText, item.CommodityDescription)).ToList();
            if (v == null || v.Count <= 0) return null;
            var eGoods = v.Select(item => item.Item1).Distinct();
            var cGoods = v.Select(item => item.Item2).Distinct();
            return new Tuple<string, string>(string.Join(Environment.NewLine, eGoods),
                string.Join(Environment.NewLine, cGoods));
        }
        string GetBanksMemo(DNInfo info)
        {
            HeaderInfo h = info.HeaderInfo;
            List<string> items = new List<string>();
            Action<string, string> addMemo = (display, memo) =>
            {
                items.Add(string.Format("{0}:{1}", display, memo));
            };
            addMemo("Bank name", h.BankName);
            //addMemo(h.BankRegion);
            addMemo("Bank adress", h.BankStreet);
            //addMemo(h.BankCity);
            //addMemo(h.BankBranch);
            items.Add(string.Format("{0}{1}", string.Format("Swift:{0}", h.BankSwift), string.Format("     A/C NO:{0}", h.BankAccountNumber)));
            return string.Join(Environment.NewLine, items);
        }
        string QueryVATNO(DNInfo dnInfo, DataTable partyDT)
        {
            if (partyDT == null || dnInfo.PartnerInfos == null) return null;
            PartnerInfo pInfo = dnInfo.PartnerInfos.Where(item => item.PartnerFunction == "AG").FirstOrDefault();
            if (pInfo == null) return null;
            DataRow[] rows = partyDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(pInfo.PartnerNumber)));
            if (rows == null || rows.Length <= 0) return null;
            return Prolink.Math.GetValueAsString(rows[0]["TAX_NO"]);
        }

        string QueryPartyName(string partyNO, DataTable partyDT)
        {
            DataRow row = Helper.QueryPartyRow(partyNO, partyDT);
            if (row == null) return null;
            return Prolink.Math.GetValueAsString(row["PARTY_NAME"]);
        }
        DataRow QueryCityDT(string prolinkCD, bool isDomesticTrade)
        {
            if (string.IsNullOrEmpty(prolinkCD)) return null;
            string sql = string.Empty;
            if (isDomesticTrade)
            {
                sql = string.Format("SELECT STATE,PORT_NM,REGION,(SELECT TOP 1 CD_DESCP FROM BSCODE WHERE CD_TYPE='TRGN' AND CD=B.REGION) AS REGION_NM " +
                "FROM BSTPORT B WHERE PORT_CD={0}", SQLUtils.QuotedStr(prolinkCD));
            }
            else
            {
                if (prolinkCD.Length < 5) return null;
                Tuple<string, string> port = Helper.GetPortCode(prolinkCD);
                sql = string.Format("SELECT STATE,PORT_NM,REGION,(SELECT TOP 1 CD_DESCP FROM BSCODE WHERE CD_TYPE='TRGN' AND CD=B.REGION) AS REGION_NM FROM BSCITY B" +
                 " WHERE B.PORT_CD={0} AND B.CNTRY_CD={1}", SQLUtils.QuotedStr(port.Item2), SQLUtils.QuotedStr(port.Item1));
            }
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }

        DataRow QueryPortCode(string sapPort, bool isName)
        {
            if (string.IsNullOrEmpty(sapPort)) return null;
            string sql = string.Empty;
            if (isName)
                sql = string.Format("SELECT TRUCK_PORT,PORT,PROLINK_CD FROM TPVPORT P WHERE PORT_NM={0}", SQLUtils.QuotedStr(sapPort));
            else
                sql = string.Format("SELECT TRUCK_PORT,PORT,PROLINK_CD FROM TPVPORT P WHERE PORT={0}", SQLUtils.QuotedStr(sapPort));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt != null && dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }
        string QueryTransMode(DNInfo info)
        {
            string code = info.HeaderInfo.MeansOfTransportType;
            if (string.IsNullOrEmpty(code)) return null;
            switch (code)
            {
                case "T001":
                    List<PalletInfo> pList = SelectPallets(info, true);
                    if (pList == null || pList.Count <= 0) return "F";
                    var pk600 = pList.Where(p => p.PackagingMaterials == "PK-600").FirstOrDefault();
                    if (pk600 != null) return "L";
                    return "F";
                default:
                    string sql = string.Format("SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TTRN' AND CD={0}", SQLUtils.QuotedStr(code));
                    return DB.GetValueAsString(sql);
            }
        }
        string QueryTransDesc(string code)
        {
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='TNT' AND CD={0}", SQLUtils.QuotedStr(code));
            return DB.GetValueAsString(sql);
        }
        string QueryFITransportType(DNInfo info, string location)
        {
            string type = "A";
            if (info.HeaderInfo.SourceSAP == SPMS)
                type = "P";
            string sql = string.Format("SELECT CMP,BU,SOLD_TO,ATRAN_TYPE FROM SMACTN WHERE CMP={0} AND SA_SP={1}", SQLUtils.QuotedStr(location),
                SQLUtils.QuotedStr(type));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            Func<string, string, string> getCondition = (org, soldTo) =>
            {
                List<ConditionItem> items = new List<ConditionItem>();
                items.Add(new ConditionItem("BU", org));
                if (!string.IsNullOrEmpty(soldTo))
                    items.Add(new ConditionItem("SOLD_TO", soldTo));
                return DBManager.CreateCondition(items, false);
            };
            string condition = getCondition(info.HeaderInfo.OriginalSalesOrg, info.HeaderInfo.OriginalSoldTo);
            DataRow[] rows = dt.Select(condition);
            if (rows != null && rows.Length > 0) return Prolink.Math.GetValueAsString(rows[0]["ATRAN_TYPE"]);
            condition = getCondition(info.HeaderInfo.OriginalSalesOrg, null);
            rows = dt.Select(condition);
            if (rows != null && rows.Length > 0) return Prolink.Math.GetValueAsString(rows[0]["ATRAN_TYPE"]);
            condition = getCondition(null, null);
            rows = dt.Select(condition);
            if (rows != null && rows.Length > 0) return Prolink.Math.GetValueAsString(rows[0]["ATRAN_TYPE"]);
            return null;
        }
        string GetFreightTerm(DNInfo info)
        {
            if (string.IsNullOrEmpty(info.HeaderInfo.InsuranceType)) return null;
            switch (info.HeaderInfo.InsuranceType)
            {
                case "01":
                case "03":
                    return "P";
                case "02": return "C";
                default: return null;
            }
        }
        string GetTransacteMode(DNInfo info, string location)
        {
            string materialtype = info.HeaderInfo.MaterialType;
            string sql = string.Format("SELECT TOP 1 TRANSACTE_MODE FROM SMMATERIAL WHERE MATERIAL_TYPE={0} AND CMP={1}", SQLUtils.QuotedStr(materialtype), SQLUtils.QuotedStr(location));
            return DB.GetValueAsString(sql);
        }
        class TotalPackageInfo
        {
            public int Count { get; set; }
            public string Unit { get; set; }
            public string Unit_Desc { get; set; }
            public int CartonsNum { get; set; }
            public int PalletNum { get; set; }
            public int CartonsSum { get; set; }
        }

        DataTable QueryUnitDT(List<string> items, string cmp = "")
        {

            if (items == null || items.Count <= 0) return null;
            string sql = string.Empty;
            if (string.IsNullOrEmpty(cmp))
                sql = string.Format("SELECT CD,CD_DESCP,AP_CD FROM BSCODE WHERE CD_TYPE='TCNT' AND CD IN({0})", string.Join(",", items.Select(s => SQLUtils.QuotedStr(s))));
            else
                sql = string.Format("SELECT CD,CD_DESCP,AP_CD FROM BSCODE WHERE CD_TYPE='TCNT' AND CD IN({0}) AND (CMP={1} OR CMP='*')", string.Join(",", items.Select(s => SQLUtils.QuotedStr(s))), SQLUtils.QuotedStr(cmp));
            return DB.GetDataTable(sql, new string[] { });
        }

        double? GetSmcuftCBM(string dnno)
        {
            string sql = string.Format("SELECT SUM(VW) FROM SMCUFT WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            return DB.GetValueAsFloat(sql);
        }

        TotalPackageInfo GetTotalPackageInfo(DNInfo info, string dep, string cmp)
        {
            if ("CS".Equals(dep))
                return GetCSTotalPackageInfo(info, cmp);
            return GetTotalPackageInfo(info);
        }

        TotalPackageInfo GetCSTotalPackageInfo(DNInfo info, string cmp)
        {
            List<PalletInfo> list = SelectPallets(info, false);
            if (list == null || list.Count <= 0) return null;
            list = GetListDeliItemNotExcludeTANN(info, list);
            List<PalletInfo> contFirstLevelItems = new List<PalletInfo>();
            List<PalletInfo> contOtherLevelItems = new List<PalletInfo>();
            List<PalletInfo> contFirstLevelPltItems = new List<PalletInfo>();
            List<PalletInfo> contFirstLevelCtnItems = new List<PalletInfo>();
            List<PalletInfo> containerList = SelectPallets(info, true); //找到conainter 中的信息
            string containerIhnumber = string.Empty;
            foreach (PalletInfo cp in containerList)
            {
                containerIhnumber = cp.InternalHandlingUnitNumber;
            }

            #region Func checkInContainer//查找PalletInfo中是否属于container第一级的，将这些不同的Item 都放入到contFirstLevelItems，并且返回bool 类型判断contFirstLevelItems是否有包含此票
            Func<PalletInfo, bool> checkInContainer = p =>
            {
                if (p.HigherLevelHandlingUnit == containerIhnumber)
                {
                    contFirstLevelItems.Add(p); return true;
                }
                else
                {
                    if (!contOtherLevelItems.Contains(p))
                        contOtherLevelItems.Add(p);
                }
                return false;
            };
            #endregion

            #region Func checkInOnePlace//查找PalletInfo中是否属于同一个Place下，将不同Place的Item 都放入到contFirstLevelPltItems，并且返回bool 类型判断contFirstLevelPltItems是否有包含此票
            Func<PalletInfo, bool> checkInOnePlace = p =>
            {
                if (contFirstLevelPltItems.Count <= 0) { contFirstLevelPltItems.Add(p); return false; }
                if (contFirstLevelPltItems.Contains(p)) return false;
                var r = contFirstLevelPltItems.Select(item => item.ExternalHandlingUnitIdentification).Contains(p.ExternalHandlingUnitIdentification);
                contFirstLevelPltItems.Add(p);
                return r;
            };
            #endregion

            #region Func getUnitDesc  根据SAP的单位获取ES中对应的单位以及单位描述
            DataTable unitDT = QueryUnitDT(list.Select(l => l.PackagingMaterials).Distinct().ToList());
            Func<PalletInfo, string> getUnitDesc = p =>
            {
                string desc = p.PackagingMaterialsDescription;
                if (unitDT == null || unitDT.Rows.Count <= 0) return desc;
                DataRow[] rows = unitDT.Select(string.Format("CD={0}", SQLUtils.QuotedStr(p.PackagingMaterials)));
                if (rows == null || rows.Length <= 0) return desc;
                return Prolink.Math.GetValueAsString(rows[0]["CD_DESCP"]);
            };

            #endregion
            //查找包含在container下的所有pallet和carton
            foreach (PalletInfo p in list)
            {
                checkInContainer(p);
            }
            foreach (PalletInfo p in contFirstLevelItems)
            {
                if (CheckIsPallet(p, unitDT))
                {
                    //PLT的做法
                    checkInOnePlace(p);   //判断是否同属于一个Place
                }
                else
                {
                    if (!contFirstLevelCtnItems.Contains(p))
                        contFirstLevelCtnItems.Add(p);
                }
            }

            TotalPackageInfo plt = new TotalPackageInfo
            {
                Unit = "PLT",
                Unit_Desc = "Pallet"
            };
            TotalPackageInfo pltctn = new TotalPackageInfo
            {
                Unit = "CTN",
                Unit_Desc = "CARTON"
            };

            #region Func checkInOnePallet//查找PalletInfo中是否属于同一个Place下，将不同Place的Item 都放入到contFirstLevelPltItems，并且返回bool 类型判断contFirstLevelPltItems是否有包含此票
            Func<PalletInfo, bool> checkInOnePallet = p =>
            {
                foreach (PalletInfo fp in contOtherLevelItems)
                {
                    if (fp.HigherLevelHandlingUnit == p.InternalHandlingUnitNumber)
                        pltctn.Count++;
                }
                return true;
            };
            #endregion

            //计算栈板数下的所有ctn
            List<string> pltList = new List<string>();
            foreach (PalletInfo p in contFirstLevelPltItems)
            {
                if (!string.IsNullOrEmpty(p.ExternalHandlingUnitIdentification))
                {
                    if (pltList.Contains(p.ExternalHandlingUnitIdentification))
                        continue;
                    pltList.Add(p.ExternalHandlingUnitIdentification);
                }
                plt.Count++;
                checkInOnePallet(p);
            }
            //(list.Select(l => l.PackagingMaterials).Distinct().ToList()
            int ctncounts = contFirstLevelCtnItems.Select(p => p.InternalHandlingUnitNumber).Distinct().ToList().Count();
            pltctn.Count += ctncounts;
            //for (int i=0;i<ctncounts;i++)
            //{
            //    pltctn.Count++;
            //}

            if (contFirstLevelPltItems.Count > 0 && contFirstLevelCtnItems.Count > 0)
            {
                plt.Count = contFirstLevelPltItems.Count + ctncounts;
                plt.Unit = "PKG";
                //plt.Unit_Desc = string.Format("{0}Package={1}Pallet+{2}CARTON={3}CARTON", plt.Count, contFirstLevelPltItems.Count, ctncounts, pltctn.Count);
                plt.Unit_Desc = string.Format("{0}Package={1}{2}+{3}CARTON={4}CARTON", plt.Count, contFirstLevelPltItems.Count, getUnitDesc(contFirstLevelPltItems.First()), ctncounts, pltctn.Count);
                plt.CartonsNum = ctncounts;
                plt.PalletNum = contFirstLevelPltItems.Count;
                plt.CartonsSum = pltctn.Count;
                return plt;
            }
            else if (contFirstLevelPltItems.Count > 0)
            {
                plt.Unit = "PLT";
                if (pltctn.Count <= 0)
                {
                    pltctn.Count = Prolink.Math.GetValueAsInt(contFirstLevelPltItems.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum());
                }
                //plt.Unit_Desc = string.Format("{0}Pallet={1}CARTON", plt.Count, pltctn.Count);
                plt.Unit_Desc = string.Format("{0}{1}={2}CARTON", plt.Count, getUnitDesc(contFirstLevelPltItems.First()), pltctn.Count);
                plt.CartonsNum = 0;
                plt.PalletNum = plt.Count;
                plt.CartonsSum = pltctn.Count;
                return plt;
            }
            pltctn.Unit = "CTN";
            pltctn.Unit_Desc = pltctn.Count + "CARTON";
            pltctn.CartonsNum = pltctn.Count;
            pltctn.CartonsSum = pltctn.Count;
            pltctn.PalletNum = 0;
            return pltctn;

            /*TotalPackageInfo plt = new TotalPackageInfo
            {
                Unit = "PLT",
                Unit_Desc = "Pallet"
            };
            List<PalletInfo> palletItems = new List<PalletInfo>();
            List<PalletInfo> checkedItems = new List<PalletInfo>(); //存放所有不同的Place的PalletInfo信息
            #region Func checkInOnePlace//查找PalletInfo中是否属于同一个Place下，将不同Place的Item 都放入到checkedItems，并且返回bool 类型判断checkedItems是否有包含此票
            Func<PalletInfo, bool> checkInOnePlace = p =>
            {
                if (checkedItems.Count <= 0) { checkedItems.Add(p); return false; }
                if (checkedItems.Contains(p)) return false;
                var r = checkedItems.Select(item => item.ExternalHandlingUnitIdentification).Contains(p.ExternalHandlingUnitIdentification);
                checkedItems.Add(p);
                return r;
            };
            #endregion

            List<TotalPackageInfo> other = new List<TotalPackageInfo>();//单位描述
            #region Func getP 查找other TotalPackage 中是否有包含同种单位的，如果有就取默认或者第一个单位类型，如果没有返回Null
            Func<PalletInfo, TotalPackageInfo> getP = p =>
            {
                if (other == null || other.Count <= 0) return null;
                return other.Where(o => o.Unit == p.PackagingMaterials).FirstOrDefault();
            };
            #endregion

            #region Func getUnitDesc  根据SAP的单位获取ES中对应的单位以及单位描述
            DataTable unitDT = QueryUnitDT(list.Select(l => l.PackagingMaterials).Distinct().ToList(),cmp);
            Func<PalletInfo, string> getUnitDesc = p =>
            {
                string desc = p.PackagingMaterialsDescription;
                if (unitDT == null || unitDT.Rows.Count <= 0) return desc;
                DataRow[] rows = unitDT.Select(string.Format("CD={0}", SQLUtils.QuotedStr(p.PackagingMaterials)));
                if (rows == null || rows.Length <= 0) return desc;
                return Prolink.Math.GetValueAsString(rows[0]["AP_CD"]);
            };
            #endregion

            //做法：每一笔的palletsinfo信息通过区分出CTN或者是PLT来进行分开处理
            foreach (var p in list)
            {
                //这里主要用来区分是否是ＰＬＴ还是ＣＴＮ，
                if (CheckIsPallet(p))//判断是否包含Pallet 判断标准为（是否包含PALLET）
                {
                    //PLT的做法
                    if (!checkInOnePlace(p))    //判断是否同属于一个Place
                        plt.Count++;            //若CheckedItems中没有包含，则PLT增加1说明PLT增加数量
                    palletItems.Add(p);         //记录PalletItems的细项
                }
                else
                {
                    //CTN做法
                    var t = getP(p);  //首先去存储了Other的List中查找若找不到返回Null
                    if (t == null)
                    {   
                        //t为null的处理方式，构建一个TotalPackageInfo信息，附上初始单位，描述已经数量
                        t = new TotalPackageInfo
                        {
                            Unit = p.PackagingMaterials,
                            Unit_Desc = getUnitDesc(p),
                            Count = checkInOnePlace(p) ? 0 : 1,
                            HigherLevelHandlingUnit = p.HigherLevelHandlingUnit
                        };
                        other.Add(t);   //初始化完毕后，Other中也保存一份资料
                    }
                    else
                    {
                        //other的List中包含了此种信息，则将数量执行加1的动作
                        if (!checkInOnePlace(p))
                            t.Count++;
                    }
                }
            }
            if (plt.Count > 0)
            {
                plt.Unit_Desc=string.Format("{0}Pallet{1}",plt.Count,palletItems.Count <= 0 ? string.Empty : string.Format("={0}{1}",
                    palletItems.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum().ToString(), palletItems.First().QTYUnit));
            }
            if (other.Count <= 0) return plt;
            string otherDesc = string.Empty;
            var dist = other.Select(u => u.Unit_Desc).Distinct().ToList();
            int totalcount = palletItems.Count + other.Count;
            if (dist.Count > 1)
            {
                List<TotalPackageInfo> pltitems = other.Where(item => item.Unit_Desc == "PLT").ToList();
                List<TotalPackageInfo> ctnitems = other.Where(item => item.Unit_Desc == "CTN").ToList();
                otherDesc = ctnitems.Select(t => t.Count).Sum() + "CARTON";//PALLET other.First().Unit_Desc
                var sum = 0;
                foreach (TotalPackageInfo pltindex in pltitems)
                {
                    int ctnnum = GetTotalICartons(pltindex.HigherLevelHandlingUnit, list);
                    sum += ctnnum * pltindex.Count;
                }
                sum += ctnitems.Select(t => t.Count).Sum();
                otherDesc += "+" + pltitems.Select(t => t.Count).Sum() + "PALLET";//PALLET other.First().Unit_Desc
                otherDesc += "=" + sum + "CARTON";

                return new TotalPackageInfo
                {
                    Unit = "PKG",
                    Count = pltitems.Select(t => t.Count).Sum() + ctnitems.Select(t => t.Count).Sum(),
                    Unit_Desc = otherDesc
                };
            }
            else
            {
                if ("CTN".Equals(other.First().Unit_Desc))
                {
                    otherDesc = other.Select(t => t.Count).Sum() + "CARTON";
                }
                else
                {
                    var sum = 0;
                    foreach (TotalPackageInfo pltindex in other)
                    {
                        int ctnnum = GetTotalICartons(pltindex.HigherLevelHandlingUnit, list);
                        sum += ctnnum * pltindex.Count;
                    }
                    otherDesc = other.Select(t => t.Count).Sum() + "PALLET=" + sum + "CARTON";
                }
            }
            if (palletItems.Count <= 0)
            {
                return new TotalPackageInfo
                {
                    Unit = other.First().Unit_Desc,
                    Count = other.Select(t => t.Count).Sum(),//other.Select(t => t.Count).Sum(),
                    Unit_Desc = otherDesc
                };
            }
            //string otherDesc = string.Join("+", other.Select(u => string.Format("{0}{1}", u.Count.ToString(), u.Unit_Desc)));
            //if (palletItems.Count <= 0)
            //{
            //    return new TotalPackageInfo
            //    {
            //        Unit = other.First().Unit,
            //        Count = other.Select(t => t.Count).Sum(),
            //        Unit_Desc =string.Format("{0}CTN={0}",other.Count.ToString(),other.Unit_Desc)
            //    };
            //}
            return new TotalPackageInfo
            {
                Unit = "PKG",
                Count = plt.Count + other.Select(t => t.Count).Sum(),
                Unit_Desc = string.Format("{0}+{1}", plt.Unit_Desc, otherDesc)
            };
             * */
        }

        private static int GetTotalICartons(string Unit, List<PalletInfo> pilists)
        {
            int count = 0;
            foreach (var p in pilists)
            {
                if (p.HigherLevelHandlingUnit.Equals(Unit))
                    count++;
            }
            return count;
        }




        TotalPackageInfo GetTotalPackageInfo(DNInfo info)
        {
            List<PalletInfo> list = SelectPallets(info, false);
            if (list == null || list.Count <= 0) return null;
            list = GetListDeliItemNotExcludeTANN(info, list);
            TotalPackageInfo plt = new TotalPackageInfo
            {
                Unit = "PLT",
                Unit_Desc = "Pallet"
            };
            List<PalletInfo> palletItems = new List<PalletInfo>();
            List<PalletInfo> checkedItems = new List<PalletInfo>(); //存放所有不同的Place的PalletInfo信息
            #region Func checkInOnePlace//查找PalletInfo中是否属于同一个Place下，将不同Place的Item 都放入到checkedItems，并且返回bool 类型判断checkedItems是否有包含此票
            Func<PalletInfo, bool> checkInOnePlace = p =>
            {
                if (checkedItems.Count <= 0) { checkedItems.Add(p); return false; }
                if (checkedItems.Contains(p)) return false;
                var r = checkedItems.Select(item => item.ExternalHandlingUnitIdentification).Contains(p.ExternalHandlingUnitIdentification);
                checkedItems.Add(p);
                return r;
            };
            #endregion

            List<TotalPackageInfo> other = new List<TotalPackageInfo>();//单位描述
            #region Func getP 查找other TotalPackage 中是否有包含同种单位的，如果有就取默认或者第一个单位类型，如果没有返回Null
            Func<PalletInfo, TotalPackageInfo> getP = p =>
            {
                if (other == null || other.Count <= 0) return null;
                return other.Where(o => o.Unit == p.PackagingMaterials).FirstOrDefault();
            };
            #endregion

            //Dictionary<TotalPackageInfo, List<PalletInfo>> otherItems = new Dictionary<TotalPackageInfo, List<PalletInfo>>();

            #region Func getUnitDesc  根据SAP的单位获取ES中对应的单位以及单位描述
            DataTable unitDT = QueryUnitDT(list.Select(l => l.PackagingMaterials).Distinct().ToList());
            Func<PalletInfo, string> getUnitDesc = p =>
            {
                string desc = p.PackagingMaterialsDescription;
                if (unitDT == null || unitDT.Rows.Count <= 0) return desc;
                DataRow[] rows = unitDT.Select(string.Format("CD={0}", SQLUtils.QuotedStr(p.PackagingMaterials)));
                if (rows == null || rows.Length <= 0) return desc;
                return Prolink.Math.GetValueAsString(rows[0]["CD_DESCP"]);
            };

            Func<string, string> getUnitByCd = cd =>
            {
                DataRow[] rows = unitDT.Select(string.Format("CD={0}", SQLUtils.QuotedStr(cd)));
                if (rows == null || rows.Length <= 0) return cd;
                return Prolink.Math.GetValueAsString(rows[0]["AP_CD"]);
            };


            #endregion

            foreach (var p in list)
            {
                if (CheckIsPallet(p, unitDT))//判断是否包含Pallet 判断标准为（是否包含PALLET）
                {
                    if (!checkInOnePlace(p))    //判断是否同属于一个Place
                        plt.Count++;
                    palletItems.Add(p);
                }
                else
                {
                    var t = getP(p);
                    if (t == null)
                    {
                        t = new TotalPackageInfo
                        {
                            Unit = p.PackagingMaterials,
                            Unit_Desc = getUnitDesc(p),
                            Count = checkInOnePlace(p) ? 0 : 1
                        };
                        other.Add(t);
                    }
                    else
                    {
                        if (!checkInOnePlace(p))
                            t.Count++;
                    }
                }
            }
            plt.Unit_Desc = string.Format("{0}Pallet{1}", plt.Count, palletItems.Count <= 0 ? string.Empty : string.Format("({0}{1})",
                palletItems.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum().ToString(), palletItems.First().QTYUnit));
            string unit = string.Empty;
            if (palletItems.Count > 0)
            {
                unit = getUnitDesc(palletItems.First());
            }
            if (string.IsNullOrEmpty(unit))
            {
                unit = "Pallet";
            }
            plt.Unit_Desc = string.Format("{0}{1}{2}", plt.Count, unit, palletItems.Count <= 0 ? string.Empty : string.Format("({0}{1})",
                palletItems.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum().ToString(), palletItems.First().QTYUnit));

            //plt.CartonsNum = palletItems.Select(p => Prolink.Math.GetValueAsInt(p.QTY)).Sum();
            plt.PalletNum = plt.Count;
            plt.CartonsSum = palletItems.Select(p => Prolink.Math.GetValueAsInt(p.QTY)).Sum();
            plt.CartonsSum = palletItems.Select(p => Prolink.Math.GetValueAsInt(p.QTY)).Sum();
            //plt.Unit_Desc = string.Format("{0}Pallet{1}", plt.Count, palletItems.Count <= 0 ? string.Empty : string.Format("={0}CARTON",
            //    palletItems.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum().ToString()));

            //plt.Unit_Desc = string.Format("{0}Pallet={1}CARTON", plt.Count, pltctn.Count);
            if (other.Count <= 0) return plt;

            string otherDesc = string.Empty;

            var dist = other.Select(u => u.Unit_Desc).Distinct().ToList();
            if (dist.Count == 1)
            {
                otherDesc = string.Join("+", string.Format("{0}{1}", other.Select(t => t.Count).Sum(), other.First().Unit_Desc));
            }
            else if (dist.Count > 1)
            {
                foreach (string distitem in dist)
                {
                    List<TotalPackageInfo> otherTp = other.Where(t => t.Unit_Desc == distitem).ToList();
                    otherDesc = string.Join("+", string.Format("{0}{1}", otherTp.Select(p => p.Count).Sum(), otherTp.First().Unit_Desc));
                }
            }
            else
            {
                otherDesc = string.Join("+", other.Select(u => string.Format("{0}{1}", u.Count.ToString(), u.Unit_Desc)));
            }


            if (palletItems.Count <= 0)
            {
                unit = getUnitByCd(other.First().Unit);
                int palletnum = 0;
                int cartonnum = 0;
                int cartonsum = 0;
                if ("PLT".Equals(unit))
                {
                    palletnum = other.Select(t => t.Count).Sum();
                }
                else
                {
                    cartonnum = other.Select(t => t.Count).Sum();
                    cartonsum = other.Select(t => t.Count).Sum();
                }
                return new TotalPackageInfo
                {
                    Unit = getUnitByCd(other.First().Unit),
                    Count = other.Select(t => t.Count).Sum(),
                    Unit_Desc = otherDesc,
                    CartonsNum = cartonnum,
                    CartonsSum = cartonsum,
                    PalletNum = palletnum
                };
            }
            return new TotalPackageInfo
            {
                Unit = "PKG",
                Count = plt.Count + other.Select(t => t.Count).Sum(),
                Unit_Desc = string.Format("{0}+{1}", plt.Unit_Desc, otherDesc),
                PalletNum = plt.Count
            };
        }

        /// <summary>
        /// 过滤List中 包含DNInfo的PalletInfo信息中为TANN的Pallet信息
        /// </summary>
        /// <param name="info">DNInfo信息</param>
        /// <param name="list">需要过滤的PalletInfo列表信息</param>
        /// <returns></returns>
        private static List<PalletInfo> GetListDeliItemNotExcludeTANN(DNInfo info, List<PalletInfo> list)
        {
            if (info.ItemInfos != null && info.ItemInfos.Length > 0)
            {
                List<ItemInfo> items = info.ItemInfos.Where(item => item.DeliveryItemCategory == SkipItemType).ToList();
                if (items != null && items.Count > 0)
                    list = list.Where(p => { return !items.Select(item => item.DeliveryItem).Contains(p.ItemNumber); }).ToList();
            }
            return list;
        }
        string GetApproveTyp(DNInfo info, string dep)
        {
            if (info.HeaderInfo.SourceSAP == SPMS)
            {
                if (info.HeaderInfo.SalesDocumentType == "2")
                    return "SPMS2";
                else return null;
            }
            else
            {
                string dnType = info.HeaderInfo.OriginalSalesDocumentType;
                if (string.IsNullOrEmpty(dnType))
                    dnType = info.HeaderInfo.SalesDocumentType;
                switch (dnType)
                {
                    case "ZSL": return "FOC_DN_INV";
                    case "ZSP": return "FOC_CS";
                    case "ZBV": return "Staff_Buy";
                    case "ZRE":
                        if (dep == "BLUQT")
                            return "IB_RT_BLU";
                        else
                            return "IB_RETURN";
                    default: return "STD_DN_INV";
                }
            }
        }

        static XmlDocument _incotermMappingDoc;
        XmlDocument GetIncotermMappingDoc()
        {
            if (_incotermMappingDoc != null) return _incotermMappingDoc;
            string filePath = "edi/IncotermMapping.xml";
            filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, filePath);
            _incotermMappingDoc = new XmlDocument();
            _incotermMappingDoc.Load(filePath);
            return _incotermMappingDoc;
        }
        Tuple<string, string> GetServiceMode(DNInfo info, string transType)
        {
            if (string.IsNullOrEmpty(info.HeaderInfo.Incoterms1)) return null;
            if (string.IsNullOrEmpty(transType)) return null;
            try
            {
                XmlDocument doc = GetIncotermMappingDoc();
                if (doc == null) return null;
                XmlNode tNode = doc.SelectSingleNode(string.Format("//Item[@code='{0}']/TransType[@code='{1}']", info.HeaderInfo.Incoterms1, transType));
                if (tNode == null) return null;
                string from = null;
                string to = null;
                foreach (XmlNode node in tNode)
                {
                    switch (node.Name)
                    {
                        case "LoadingFrom": from = node.InnerText; break;
                        case "LoadingTo": to = node.InnerText; break;
                    }
                }
                if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to))
                    return null;
                return new Tuple<string, string>(from, to);
            }
            catch (Exception e)
            {
                Logger.WriteLog("GetServiceMode Exception!", e);
                return null;
            }
        }

        private const string SPMS = "SPMS";
        EditInstruct ParseHeader(ParseContext context)
        {
            SIProfileInfo pInfo = context.DNProfileInfo;
            DNInfo dnInfo = context.DNInfo;
            context.Uid = System.Guid.NewGuid().ToString();
            HeaderInfo hInfo = context.DNInfo.HeaderInfo;
            EditInstruct ei = new EditInstruct("SMDN", context.Operation);
            if (context.Operation == EditInstruct.INSERT_OPERATION)
            {
                ei.Put("U_ID", context.Uid);
                ei.Put("APPROVE_TO", "A");
            }
            else
            {
                ei.Condition = string.Format("SAP_ID={0} AND DN_NO={1}", SQLUtils.QuotedStr(hInfo.SourceSAP), SQLUtils.QuotedStr(hInfo.DNNOWithCompanyCode));
            }
            Action<string, string, string> bindValue = (column, proValue, dnValue) =>
            {
                if (!string.IsNullOrEmpty(proValue))
                    ei.Put(column, proValue);
                else
                    ei.Put(column, dnValue);
            };
            ei.Put("STATUS", "D");
            ei.Put("PROFILE_CD", hInfo.ProfileCode);
            ei.Put("OBU", hInfo.OriginalSalesOrg);
            ei.Put("OSOLD_TO", hInfo.OriginalSoldTo);
            ei.Put("OCMP", hInfo.OriginalCompany);
            ei.Put("CUR", hInfo.SDCurrency);
            ei.Put("GROUP_ID", Context.GroupId);
            ei.Put("SAP_ID", hInfo.SourceSAP);
            ei.Put("ORIGIN_NO", hInfo.DNNO);
            ei.PutExpress("SEAL_QTY", string.Format("(SELECT COUNT(*) FROM SMDNS WHERE ODN_NO={0})", SQLUtils.QuotedStr(hInfo.DNNO)));
            string cmp = GetCompany(dnInfo, hInfo.RefDNNO);
            ei.Put("CMP", string.IsNullOrEmpty(cmp) ? hInfo.CompanyCode : cmp);
            ei.Put("DN_NO", hInfo.DNNOWithCompanyCode);
            ei.Put("SAP_NO", hInfo.RefSourceSAP);
            ei.Put("DN_NO_CMP_REF", hInfo.RefDNNOWithCompanyCode);
            ei.Put("DN_NO_REF", hInfo.RefDNNO);
            ParseHeaderForDate(ei, context);
            ei.Put("STN", hInfo.CompanyCode);
            context.ELog = new ImportDNEDILog(context.DNInfo, cmp, hInfo.CompanyCode);
            DNUserInfo dnUserInfo = GetDNUserInfo(dnInfo);
            string dep = string.Empty;
            if (dnUserInfo != null)
            {
                dep = dnUserInfo.Dep;
                ei.Put("DEP", dep);
                ei.Put("CREATE_DEP", dnUserInfo.Dep);
                ei.Put("CREATE_EXT", dnUserInfo.Ext);
                ei.Put("CREATE_BY", dnUserInfo.UserID);
            }
            else
                ei.Put("CREATE_BY", hInfo.CreateBy);
            ei.Put("APPROVE_TYPE", GetApproveTyp(dnInfo, dep));
            string plantCode = GetPlant(dnInfo);
            ei.Put("PLANT", plantCode);
            ei.Put("PLANT_NM", QueryPartyName(plantCode, context.PartyDT));
            ei.Put("VAT_NO", QueryVATNO(dnInfo, context.PartyDT));
            ei.Put("PORTE_CD", hInfo.ExportCode);
            ei.Put("PORTE_DESCP", hInfo.ExportDescp);
            ei.Put("MODIFY_BY", hInfo.ChangedBy);
            if (!string.IsNullOrEmpty(hInfo.OriginalSalesDocumentType))
                ei.Put("DN_TYPE", hInfo.OriginalSalesDocumentType);
            else
                ei.Put("DN_TYPE", hInfo.SalesDocumentType);
            ei.Put("BU", hInfo.SalesOrganization);
            ei.Put("CHANNEL_CD", hInfo.DistributionChannel);
            ei.Put("CHANNEL", hInfo.DistributionChannelDescription);
            ei.Put("DIVISION_CD", hInfo.Division);
            ei.Put("DIVISION_DESCP", hInfo.DivisionDescription);
            ei.Put("TRAN_MODE", hInfo.MeansOfTransportType);
            ei.Put("TRAN_DESCP", hInfo.MeansOfTransportTypeDescription);
            string transType = QueryTransMode(dnInfo);
            ei.Put("TRAN_TYPE", transType);
            ei.Put("TRAN_TYPE_DESCP", QueryTransDesc(transType));
            ParseHeaderForPort(ei, context, transType);
            Tuple<string, string> serviceMode = GetServiceMode(dnInfo, transType);
            if (serviceMode != null)
            {
                ei.Put("LOADING_FROM", serviceMode.Item1);
                ei.Put("LOADING_TO", serviceMode.Item2);
            }
            ei.Put("CARGO_TYPE", GetCargoType(dnInfo, dep, cmp));
            ei.Put("PORT_POL", hInfo.ShippingPoint);
            ei.Put("PORT_POLNM", hInfo.ShippingPointDescription);
            ei.Put("PAY_TERM_CD", hInfo.TermsOfPaymentKey);
            ei.Put("PAY_DESCP", hInfo.TermsOfPaymentKeyDescription);
            ei.Put("INCOTERM", hInfo.Incoterms1);
            ei.Put("INCOTERM_DESCP", hInfo.Incoterms2);
            ei.Put("TRADE_TERM", hInfo.Incoterms1);
            ei.Put("TRADETERM_DESCP", hInfo.Incoterms2);
            double? amount = CalcTotalAmount(dnInfo);
            ei.Put("AMOUNT1", amount.HasValue ? amount.Value : 0);
            double? gw = CalcTotalGW(dnInfo);
            ei.Put("GW", gw.HasValue ? gw.Value : Prolink.Math.GetValueAsDouble(hInfo.TotalWeight));
            ei.Put("GWU", hInfo.WeightUnit);
            double? cbm = GetSmcuftCBM(hInfo.DNNOWithCompanyCode);
            if (cbm.HasValue && cbm.Value > 0)
            {
                ei.Put("CBM", cbm.Value);
            }
            else
            {
                cbm = CalcTotalCBM(dnInfo);
                ei.Put("CBM", cbm.HasValue ? cbm.Value : Prolink.Math.GetValueAsDouble(hInfo.Volume));
            }
            double? nw = CalcTotalNW(dnInfo);
            ei.Put("NW", nw.HasValue ? nw.Value : Prolink.Math.GetValueAsDouble(hInfo.NetWeight));
            string qtyUnit = null;
            ei.Put("QTY", CalcTotalNumber(dnInfo, out qtyUnit));
            if (!string.IsNullOrEmpty(qtyUnit))
                ei.Put("QTYU", qtyUnit);
            //bindValue("AC_REMARK", pInfo == null ? null : pInfo.ACRemark, null);
            if (pInfo != null)
            {
                if (!string.IsNullOrEmpty(pInfo.ACRemark))
                    ei.Put("AC_REMARK", pInfo.ACRemark);
            }
            Tuple<string, string> goods = GetGoods(dnInfo, pInfo, dep);
            ei.Put("GOODS", goods == null ? null : goods.Item1);
            ei.Put("LGOODS", goods == null ? null : goods.Item2);
            bindValue("SHIP_MARK", GetShipMark(dnInfo), pInfo == null ? null : pInfo.ShippingMark);
            ei.Put("MEMO", GetBanksMemo(dnInfo));
            ei.Put("SC_CODE", hInfo.InsuranceType);
            ei.Put("SC_DESCP", hInfo.InsuranceTypeDescription);
            ei.Put("EXPORT_NO", hInfo.ExportNO);
            string edeclno = hInfo.DeclarationNO;
            if (!string.IsNullOrEmpty(edeclno))
                ei.Put("EDECL_NO", edeclno);
            //ei.Put("EDECL_NO", hInfo.DeclarationNO);
            ei.Put("APPROVE_NO", hInfo.ApprovalNO);
            var declDate = ParseDateTime(hInfo.DeclarationDate);
            if (declDate.HasValue)
                ei.PutDate("DECL_DATE", declDate.Value);
            ei.Put("SPEC_PROCID", hInfo.SpecialProcID);
            ei.Put("SPEC_DESCP", hInfo.SpecialDescp);
            ei.Put("ORDER_REASON", hInfo.OrderReason);
            ei.Put("UNICODE", hInfo.UniCode);
            ParseHeaderForCNTR(ei, context);
            ei.Put("ATRAN_TYPE", QueryFITransportType(dnInfo, cmp));
            TotalPackageInfo tpInfo = GetTotalPackageInfo(dnInfo, dep, cmp);
            if (tpInfo != null)
            {
                ei.Put("PKG_NUM", tpInfo.Count);
                ei.Put("PKG_UNIT", tpInfo.Unit);
                ei.Put("PKG_UNIT_DESC", tpInfo.Unit_Desc);
                ei.Put("CATON_NUM", tpInfo.CartonsNum);
                ei.Put("PALLET_NUM", tpInfo.PalletNum);
                ei.Put("CATON_SUM", tpInfo.CartonsSum);
            }
            ei.Put("FREIGHT_TERM", GetFreightTerm(dnInfo));
            ei.Put("TRANSACTE_MODE", GetTransacteMode(dnInfo, cmp));
            ei.Put("CUSTOMER_INNUMBER", hInfo.CustomerIncomingNumber);
            var postflagdate = ParseDateTime(hInfo.ActualGoodsMovementDate);
            if (postflagdate.HasValue)
            {
                ei.PutDate("POST_FLAG_DATE", postflagdate.Value);
            }
            else
            {
                ei.PutDate("POST_FLAG_DATE", null);
            }
            return ei;
        }
        void ParseHeaderForDate(EditInstruct ei, ParseContext context)
        {
            HeaderInfo hInfo = context.DNInfo.HeaderInfo;
            if (context.Operation == EditInstruct.INSERT_OPERATION)
                ei.PutDate("CREATE_DATE", DateTime.Now);
            else
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            if (!string.IsNullOrEmpty(hInfo.PlannedGoodsMovementDate))
            {
                DateTime? etd = ParseDateTime(hInfo.PlannedGoodsMovementDate);
                if (etd.HasValue)
                {
                    ei.PutDate("ETD", etd.Value);
                    ei.Put("WEEKLY", DateTimeUtils.WeekOfYear(etd.Value));
                    ei.Put("MONTH", DateTimeUtils.MonthOfYear(etd.Value));
                    ei.Put("YEAR", DateTimeUtils.YearOfYear(etd.Value));
                }
            }
        }
        DateTime? ParseDateTime(string dateTimeStr)
        {
            DateTime dt;
            if (DateTime.TryParse(dateTimeStr, out dt))
                return dt;
            if (DateTime.TryParseExact(dateTimeStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture,
                     DateTimeStyles.None, out dt)) return dt;
            if (DateTime.TryParseExact(dateTimeStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture,
                     DateTimeStyles.None, out dt)) return dt;
            return null;
        }

        void ParseHeaderForPort(EditInstruct ei, ParseContext context, string transType)
        {
            Func<bool> checkIsDomesticTrade = () =>
            {
                switch (transType)
                {
                    case "D":
                    case "T": return true;
                    default: return false;
                }
            };
            bool isDomesticTrade = checkIsDomesticTrade();
            HeaderInfo hInfo = context.DNInfo.HeaderInfo;
            if (hInfo.SourceSAP == SPMS)
            {
                ei.Put("PORT_DEST", hInfo.UnloadingPoint);
                ei.Put("PORT_DESCP", hInfo.UnloadingPointName);
            }
            else
            {
                ei.Put("PORT_DESCP", hInfo.UnloadingPoint);
                DataRow podRow = QueryPortCode(hInfo.UnloadingPoint, true);
                if (podRow != null)
                {
                    string cdColumn = isDomesticTrade ? "TRUCK_PORT" : "PROLINK_CD";
                    string pod = Prolink.Math.GetValueAsString(podRow[cdColumn]);
                    ei.Put("POD", pod);
                    DataRow cityRow = QueryCityDT(pod, isDomesticTrade);
                    if (cityRow != null)
                    {
                        string region = Prolink.Math.GetValueAsString(cityRow["REGION"]);
                        ei.Put("POD_NM", cityRow["PORT_NM"]);
                        ei.Put("REGION", region);
                        ei.Put("REGION_NM", cityRow["REGION_NM"]);
                        ei.Put("STATE", cityRow["STATE"]);
                        List<string> mlblist = new List<string>() { "USACV","USAST","USBNC","USCNO","USCRN","USCXL",
                            "USEAR","USFOB","USFUL","USGDR","USGHR","USIAD","USJMC","USLAX","USLGB","USLGV"
                            ,"USLIT","USLMT","USMFR","USMLG","USMRY","USNGZ","USNPO","USOAK","USOMA","USONT","USOTH","USPBG"
                            ,"USPDT","USPDX","USPSP","USRWC","USSAC","USSAN","USSBP","USSCA","USSCK","USSCX","USSFO","USSVR",
                            "USTEC","USVLO","USWES","USWIR","USWOA","USWTO","USSJC","USSMO" };
                        if ("F".Equals(transType) && "NA".Equals(region))
                        {
                            if (mlblist.Contains(pod))
                                ei.Put("BRG_TYPE", "");
                            else ei.Put("BRG_TYPE", "AWR");
                        }
                    }
                    else
                        ei.Put("POD_NM", hInfo.UnloadingPoint);
                    ei.Put("PORT_DEST", podRow["PORT"]);
                }
            }
            DataRow polRow = QueryPortCode(hInfo.ShippingPoint, false);
            if (polRow != null)
            {
                string cdColumn = isDomesticTrade ? "TRUCK_PORT" : "PROLINK_CD";
                string pol = Prolink.Math.GetValueAsString(polRow[cdColumn]);
                ei.Put("POL", pol);
                DataRow cityRow = QueryCityDT(pol, isDomesticTrade);
                if (cityRow != null)
                {
                    ei.Put("POL_NM", cityRow["PORT_NM"]);
                }
                else
                {
                    ei.Put("POL_NM", hInfo.ShippingPointDescription);
                }
            }
        }
        void ParseHeaderForCNTR(EditInstruct ei, ParseContext context)
        {
            CNInfo cnInfo = GetCNInfo(context.DNInfo);
            ei.Put("CNT20", null);
            ei.Put("CNT40", null);
            ei.Put("CNT40HQ", null);
            ei.Put("CNT_TYPE", null);
            ei.Put("CNT_NUMBER", null);
            if (cnInfo != null)
            {
                if (cnInfo.CNT20.HasValue)
                    ei.Put("CNT20", cnInfo.CNT20.Value);
                if (cnInfo.CNT40.HasValue)
                    ei.Put("CNT40", cnInfo.CNT40.Value);
                if (cnInfo.CNT40HQ.HasValue)
                    ei.Put("CNT40HQ", cnInfo.CNT40HQ.Value);
                if (cnInfo.CNTOther.HasValue)
                {
                    ei.Put("CNT_TYPE", cnInfo.CNTOtherType);
                    ei.Put("CNT_NUMBER", cnInfo.CNTOther.Value);
                }
            }
            double? feu = CalcFEU(cnInfo);
            if (feu.HasValue)
                ei.Put("FEU", feu.Value);
        }
        static Func<ParseContext, string, DataRow> GetPartyRow = (context, code) =>
        {
            DataRow[] rows = GetPartyRows(context, code);
            if (rows != null && rows.Length > 0) return rows[0];
            return null;
        };
        static Func<ParseContext, string, DataRow[]> GetPartyRows = (context, code) =>
        {
            if (context.PartyDT == null || context.PartyDT.Rows.Count <= 0) return null;
            return context.PartyDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(code)));
        };
        EditInstructList ParsePartner(ParseContext context)
        {
            DNInfo info = context.DNInfo;
            if (info.PartnerInfos == null || info.PartnerInfos.Length <= 0) return null;
            EditInstructList eiList = new EditInstructList();
            EditInstruct delEi = new EditInstruct("SMDNPT", EditInstruct.DELETE_OPERATION);
            delEi.Condition = string.Format("DN_NO={0}", SQLUtils.QuotedStr(info.HeaderInfo.DNNOWithCompanyCode));
            eiList.Add(delEi);
            DataTable partyTypeDT = QueryPartyType(info);
            Func<string, DataRow> getPartyRow = code => { return GetPartyRow(context, code); };
            Func<string, DataRow> getPartyTypeDesc = code =>
            {
                DataRow[] rows = partyTypeDT.Select(string.Format("CD_TYPE='PT' AND CD={0}", SQLUtils.QuotedStr(code)));
                if (rows != null && rows.Length > 0) return rows[0];
                return null;
            };
            Func<string, string, string> getResultV = (baseValue, dnValue) =>
            {
                if (string.IsNullOrEmpty(dnValue) || string.IsNullOrEmpty(dnValue.Trim())) return baseValue;
                return dnValue;
            };
            Action<EditInstruct, string, string, string> bindValue = (ei, column, baseValue, dnValue) =>
            {
                ei.Put(column, getResultV(baseValue, dnValue));
            };
            Func<DataRow, string, string> getValue = (row, column) =>
            {
                if (row == null) return null;
                return Prolink.Math.GetValueAsString(row[column]);
            };

            List<string> hederPTCodes = new List<string> { "AG", "WE", "ZE", "FC", "RE", "RO", "SH", "CS", "NT", "FS", "SP" };
            Action<string, string, string> fillHeader = (partyType, partyNo, partyName) =>
            {
                if (!hederPTCodes.Contains(partyType)) return;
                context.HeaderEI.Put(string.Format("{0}_CD", partyType), partyNo);
                context.HeaderEI.Put(string.Format("{0}_NM", partyType), partyName);
            };
            List<string> partyCodes = new List<string>();
            Action<PartnerInfo, string, string> addParty = (pi, partyType, partyNo) =>
            {
                if (partyCodes.Contains(partyType)) return;
                partyCodes.Add(partyType);
                EditInstruct ei = new EditInstruct("SMDNPT", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", context.Uid);
                ei.Put("DN_NO", info.HeaderInfo.DNNOWithCompanyCode);
                ei.Put("PARTY_TYPE", partyType);
                ei.Put("PARTY_NO", partyNo);
                DataRow row = getPartyRow(partyNo);
                bindValue(ei, "PARTY_NAME", getValue(row, "PARTY_NAME"), pi == null ? null : pi.Name1);
                bindValue(ei, "PARTY_NAME2", getValue(row, "PARTY_NAME2"), pi == null ? null : pi.Name2);
                bindValue(ei, "PARTY_NAME3", getValue(row, "PARTY_NAME3"), pi == null ? null : pi.Name3);
                bindValue(ei, "PARTY_NAME4", getValue(row, "PARTY_NAME4"), pi == null ? null : pi.Name4);
                bindValue(ei, "PART_ADDR", getValue(row, "PART_ADDR1"), pi == null ? null : pi.Street);
                bindValue(ei, "PART_ADDR2", getValue(row, "PART_ADDR2"), pi == null ? null : pi.Street2);
                bindValue(ei, "PART_ADDR3", getValue(row, "PART_ADDR3"), pi == null ? null : pi.Street3);
                bindValue(ei, "PART_ADDR4", getValue(row, "PART_ADDR4"), pi == null ? null : pi.Street4);
                bindValue(ei, "PART_ADDR5", getValue(row, "PART_ADDR5"), pi == null ? null : pi.Street5);
                string fax = string.Empty;
                if (pi != null)
                {
                    fax = pi.Fax;
                    if (!string.IsNullOrEmpty(pi.FaxExtension))
                        fax = string.Join("#", pi.Fax, pi.FaxExtension);
                }
                bindValue(ei, "FAX_NO", getValue(row, "PARTY_FAX"), fax);
                bindValue(ei, "CNTY", getValue(row, "CNTY"), pi == null ? null : pi.CountryKey);
                bindValue(ei, "CNTY_NM", getValue(row, "CNTY_NM"), pi == null ? null : pi.CountryName);
                bindValue(ei, "CITY", getValue(row, "CITY"), pi == null ? null : pi.CityCode);
                bindValue(ei, "CITY_NM", getValue(row, "CITY_NM"), pi == null ? null : pi.City);
                bindValue(ei, "STATE", getValue(row, "STATE"), pi == null ? null : pi.Region);
                bindValue(ei, "ZIP", getValue(row, "ZIP"), pi == null ? null : pi.PostalCode);
                string attn = string.Empty;
                if (pi != null)
                {
                    if (!string.IsNullOrEmpty(pi.FirstName) || !string.IsNullOrEmpty(pi.LastName))
                        attn = string.Format("{0} {1}", pi.FirstName, pi.LastName);
                    else
                        attn = pi.NameCO;
                }
                bindValue(ei, "CONTACT", getValue(row, "PARTY_ATTN"), pi == null ? null : attn);
                bindValue(ei, "MAIL", getValue(row, "PARTY_MAIL"), pi == null ? null : pi.EMail);
                bindValue(ei, "TEL", getValue(row, "PARTY_TEL"), pi == null ? null : pi.Telephone1);
                DataRow partyTypeRow = getPartyTypeDesc(partyType);
                if (partyTypeRow != null)
                {
                    ei.Put("TYPE_DESCP", partyTypeRow["CD_DESCP"]);
                    ei.Put("ORDER_BY", partyTypeRow["ORDER_BY"]);
                }
                string partyName = getResultV(getValue(row, "PARTY_NAME"), pi == null ? null : pi.Name1);
                fillHeader(partyType, partyNo, partyName);
                eiList.Add(ei);
            };
            Func<string, PartnerInfo> getProfileInfo = pType =>
            {
                if (context.DNProfileInfo == null || context.DNProfileInfo.Parties == null) return null;
                if (!context.DNProfileInfo.Parties.ContainsKey(pType)) return null;
                return context.DNProfileInfo.Parties[pType];
            };
            Func<PartnerInfo, PartnerInfo> getProfileInfoForPI = pi =>
            {
                return getProfileInfo(pi.PartnerFunction);
            };
            if (context.DNProfileInfo != null && context.DNProfileInfo.Parties != null && context.DNProfileInfo.Parties.Count > 0)
            {
                foreach (var item in context.DNProfileInfo.Parties)
                {
                    if (item.Value == null) continue;
                    addParty(item.Value, item.Key, item.Value.PartnerNumber);
                }
            }
            string shipperCode = "SH";
            if (getProfileInfo(shipperCode) == null)
            {
                string shipperPartyNO = GetShipperCode(info);
                addParty(null, shipperCode, shipperPartyNO);
            }
            foreach (var item in info.PartnerInfos)
            {
                if (getProfileInfoForPI(item) != null) continue;
                addParty(item, item.PartnerFunction, item.PartnerNumber);
            }
            return eiList;
        }
        string GetShipperCode(DNInfo info)
        {
            switch (info.HeaderInfo.SourceSAP)
            {
                case SPMS: switch (info.HeaderInfo.SalesDocumentType)
                    {
                        case "1":
                        case "2":
                            return "0009000001";
                        default: return info.HeaderInfo.CompanyCode;
                    }
                default: return info.HeaderInfo.CompanyCode;
            }
        }

        EditInstructList ParseDetail(ParseContext context)
        {
            EditInstructList eiList = new EditInstructList();
            EditInstruct delEi = new EditInstruct("SMDNP", EditInstruct.DELETE_OPERATION);
            delEi.Condition = string.Format("DN_NO={0}", SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.DNNOWithCompanyCode));
            eiList.Add(delEi);
            if (context.DNInfo.ItemInfos == null || context.DNInfo.ItemInfos.Length <= 0) return eiList;
            foreach (var item in context.DNInfo.ItemInfos)
            {
                EditInstruct ei = new EditInstruct("SMDNP", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("U_FID", context.Uid);
                ei.Put("SEND_SFIS_FLAG", "N");
                if (item.ContainerQTY != "NA")
                {
                    double cntrstdQty = Prolink.Math.GetValueAsDouble(item.ContainerQTY);
                    ei.Put("CNTR_STD_QTY", cntrstdQty);
                }
                ei.Put("CUR1", item.SDDocumentCurrency);
                double qty = Prolink.Math.GetValueAsDouble(item.ConditionPricingUnit);
                double price = 0;
                if (qty > 0)
                {
                    price = System.Math.Round(Prolink.Math.GetValueAsDouble(item.NetPrice) / qty, 10);
                    decimal dprice = (decimal)price;
                    ei.Put("UNIT_PRICE1", dprice);
                }
                ei.Put("QTY", item.ActualQuantityDelivered);
                ei.Put("QTYU", item.SalesUnit);
                ei.Put("VALUE1", price * Prolink.Math.GetValueAsDouble(item.ActualQuantityDelivered));
                ei.Put("ODN_NO", item.DNNO);
                ei.Put("SO_NO", item.SalesDocument);
                ei.Put("DN_NO", item.DNNOWithCompanyCode);
                ei.Put("DELIVERY_ITEM", item.DeliveryItem);
                ei.Put("SO_ITEM", item.SalesDocumentItem);
                ei.Put("CATEGORY", item.DeliveryItemCategory);
                ei.Put("IPART_NO", item.MaterialNumber);
                ei.Put("OPART_NO", item.CustomerModelName);
                ei.Put("PART_NO", GetPartNO(item));
                ei.Put("SAFETY_MODEL", item.OldMaterialNumber);
                ei.Put("PLANT", item.Plant);
                Tuple<string, string> pkgUnit = null;
                ei.Put("PKG_NUM", GetItemTotalPallets(context.DNInfo, item, out pkgUnit));
                if (pkgUnit != null)
                    ei.Put("PKG_UNIT", pkgUnit.Item1);
                double gw = CalcItemGW(context.DNInfo, item);
                if (gw > 0)
                    ei.Put("GW", gw);
                else
                    ei.Put("GW", item.GrossWeight);
                double nw = CalcItemNw(context.DNInfo, item);
                if (nw > 0)
                    ei.Put("NW", nw);
                else
                    ei.Put("NW", item.NetWeight);
                double cbm = CalcItemCBM(context.DNInfo, item);
                if (cbm > 0)
                    ei.Put("CBM", cbm);
                else
                    ei.Put("CBM", item.Volume);
                ei.Put("CBMU", item.VolumeUnit);
                ei.Put("PO_NO", item.CustomerPurchaseOrderNumber);
                ei.Put("BSTKD_E", item.ShipToPurchaseOrderNumber);
                ei.Put("IHREZ", item.YourReference);
                ei.Put("IHREZ_E", item.ShipToCharacter);
                ei.Put("PMATN", item.PricingReferenceMaterial);
                ei.Put("REPLACE_PART", item.AlternativeMaterials);
                ei.Put("PREPARE_RMK", item.PreparationConditions);
                ei.Put("PROD_DESCP", item.CommodityDescription);
                ei.Put("PROD_SPEC", item.CommodityCodeOfTheGoods);
                ei.Put("OHS_CODE", item.ExporterCommodityCode);
                ei.Put("IHS_CODE", item.DestinationCountriesCommodityCode);
                ei.Put("GOODS_DESCP", item.ShortText);
                ei.Put("MATERIAL_TYPE", item.MaterialType);
                ei.Put("SLOC", item.StorageLocation);
                ei.Put("INTERFACE_CD", item.UL);
                //FillPriceInfo(dnInfo, ei, item.SourceSAP, item.DNNOWithCompanyCode, item.DeliveryItem);
                FillClassficationInfo(context.DNInfo, ei, item.SourceSAP, item.DNNOWithCompanyCode, item.DeliveryItem);
                eiList.Add(ei);
            }
            return eiList;
        }
        string GetPartNO(ItemInfo item)
        {
            if (item == null) return null;
            if (string.IsNullOrEmpty(item.MaterialBelonging)) return item.MaterialBelonging;
            int index = item.MaterialBelonging.IndexOf("~");
            if (index <= 0) return item.MaterialBelonging;
            string v = item.MaterialBelonging.Substring(index);
            if (string.IsNullOrEmpty(v) || v.Length > 3) return item.MaterialBelonging;
            return item.MaterialBelonging.Substring(0, index);
        }
        EditInstructList ParsePallets(EditInstructList dnEiList, ParseContext context)
        {
            DNInfo info = context.DNInfo;
            EditInstructList eiList = new EditInstructList();
            string invId = null;
            EditInstruct invEi = CreateInvM(dnEiList, context, out invId);
            if (invEi != null)
                eiList.Add(invEi);
            eiList.MergeEditInstructList(CreateInvD(dnEiList, info, context.Operation, invId));
            string sql = string.Format("SELECT TOP 1 UPLOAD_BY FROM SMINM WHERE DN_NO={0} AND INVOICE_TYPE='I'", SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.DNNOWithCompanyCode));
            string upload = DB.GetValueAsString(sql);
            if (!string.IsNullOrEmpty(upload))
                return eiList;
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return eiList;
            EditInstruct delEi = new EditInstruct("SMINP", EditInstruct.DELETE_OPERATION);
            delEi.Condition = string.Format("INVOICE_TYPE='O' AND DN_NO={0}", SQLUtils.QuotedStr(info.HeaderInfo.DNNOWithCompanyCode));
            eiList.Add(delEi);
            var packings = CreatePackings(info);
            List<string> items = new List<string>();
            Action<string> onAdd = str =>
            {
                if (string.IsNullOrEmpty(str)) return;
                if (items.Contains(str)) return;
                items.Add(str);
            };
            int i = 0;
            double totalGW = 0;
            double totalNW = 0;
            double totalCBM = 0;
            string gwUnit = string.Empty;
            DataTable unitDT = QueryUnitDT(null);
            if (packings.Count > 0)
            {
                unitDT = QueryUnitDT(packings.First().Pallets.Select(l => l.PackagingMaterials).Distinct().ToList());
            }
            foreach (var item in packings)
            {
                i++;
                PalletInfo onePl = item.Pallets[0];
                EditInstruct ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("U_FID", invId);
                ei.Put("INVOICE_TYPE", "O");
                ei.Put("DN_NO", info.HeaderInfo.DNNOWithCompanyCode);
                string caseNo = CalcPackingCaseNO(item);
                if (CheckIsPallet(item, unitDT))
                    ei.Put("PLA_NO", caseNo);
                else
                    ei.Put("CASE_NO", caseNo);
                ei.Put("CASE_NUM", item.Pallets.Count);
                ei.Put("QTY", onePl.QTY);
                ei.Put("SEQ_NO", i);
                ei.Put("QTYU", onePl.QTYUnit);
                ei.Put("TTL_QTY", item.Pallets.Select(p => Prolink.Math.GetValueAsDouble(p.QTY)).Sum());
                ItemInfo itemInfo = GetItemInfo(info, onePl);
                if (itemInfo != null)
                {
                    ei.Put("IHS_CODE", itemInfo.DestinationCountriesCommodityCode);
                    ei.Put("PROD_DESCP", itemInfo.CommodityDescription);
                    ei.Put("PART_NO", GetPartNO(itemInfo));
                    ei.Put("GOODS_DESCP", itemInfo.ShortText);
                    ei.Put("OPART_NO", itemInfo.CustomerModelName);
                    ei.Put("IPART_NO", itemInfo.MaterialNumber);
                    ei.Put("SAFETY_MODEL", itemInfo.OldMaterialNumber);
                }
                else
                {
                    continue;
                }
                ei.Put("NW", onePl.NetWeight);
                ei.Put("NWU", onePl.NetWeightUnit);
                double tnw = CalcPackingTotalNW(item);
                totalNW += tnw;
                ei.Put("TTL_NW", tnw);
                ei.Put("GW", CalcPackingGW(item, items));
                if (string.IsNullOrEmpty(gwUnit) && !string.IsNullOrEmpty(onePl.WeightUnit))
                    gwUnit = onePl.WeightUnit;
                double tgw = CalcPackingTotalGW(item, items);
                totalGW += tgw;
                ei.Put("TTL_GW", tgw);
                ei.Put("CBM", CalcPackingCBM(item));
                double tcbm = CalcPackingTotalCBM(item);
                totalCBM += tcbm;
                ei.Put("TTL_CBM", tcbm);
                //ei.Put("DIMENSION",item.);
                //ei.Put("REMARK",itemInfo.);
                eiList.Add(ei);
                item.Pallets.ForEach(p => onAdd(p.ExternalHandlingUnitIdentification));
            }
            context.HeaderEI.Put("GW", totalGW);
            context.HeaderEI.Put("GWU", gwUnit);
            context.HeaderEI.Put("CBM", totalCBM);
            context.HeaderEI.Put("NW", totalNW);
            return eiList;
        }
        bool CheckIsPallet(PackingItem item, DataTable unitDt)
        {
            if (item.Pallets == null || item.Pallets.Count <= 0) return false;
            PalletInfo p = item.Pallets[0];
            return CheckIsPallet(p, unitDt);
        }
        bool CheckIsPallet(PalletInfo p, DataTable unitDt)
        {
            if (string.IsNullOrEmpty(p.PackagingMaterials)) return false;
            if (getUnitByCd(p.PackagingMaterials, unitDt).Trim().ToUpper().Equals("PLT"))
                return true;
            return p.PackagingMaterialsDescription.ToUpper().Contains(PalletDescription);
        }

        string getUnitByCd(string cd, DataTable unitDt)
        {
            if (unitDt == null || unitDt.Rows.Count <= 0) return cd;
            DataRow[] rows = unitDt.Select(string.Format("CD={0}", SQLUtils.QuotedStr(cd)));
            if (rows == null || rows.Length <= 0) return cd;
            return Prolink.Math.GetValueAsString(rows[0]["AP_CD"]);
        }
        const string PalletDescription = "PALLET";

        ItemInfo GetItemInfo(DNInfo dnInfo, PalletInfo pl)
        {
            if (dnInfo.ItemInfos == null || dnInfo.ItemInfos.Length <= 0) return null;
            return dnInfo.ItemInfos.Where(item => item.DeliveryItem == pl.ItemNumber).FirstOrDefault();
        }

        const string SkipItemType = "TANN";
        int? CalcTotalNumber(DNInfo info, out string qtyUnit)
        {
            qtyUnit = null;
            if (info.ItemInfos == null || info.ItemInfos.Length <= 0) return null;
            List<ItemInfo> items = info.ItemInfos.Where(item => !string.IsNullOrEmpty(item.ActualQuantityDelivered) && item.DeliveryItemCategory != SkipItemType).ToList();
            if (items != null && items.Count > 0)
            {
                qtyUnit = items[0].SalesUnit;
                return items.Select(p => Prolink.Math.GetValueAsInt(p.ActualQuantityDelivered)).Sum();
            }
            return null;
        }
        double CalcItemCBM(DNInfo info, ItemInfo item)
        {
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return 0;
            return info.PalletInfos.Where(p => p.ItemNumber == item.DeliveryItem).Select(x =>
                Prolink.Math.GetValueAsDouble(x.Volume)).Sum();
        }
        double CalcItemGW(DNInfo info, ItemInfo item)
        {
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return 0;
            return info.PalletInfos.Where(p => p.ItemNumber == item.DeliveryItem).Select(x =>
                Prolink.Math.GetValueAsDouble(x.GrossWeight)).Sum();
        }
        double CalcItemNw(DNInfo info, ItemInfo item)
        {
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return 0;
            return info.PalletInfos.Where(p => p.ItemNumber == item.DeliveryItem).Select(x =>
                Prolink.Math.GetValueAsDouble(x.NetWeight)).Sum();
        }
        int GetItemTotalPallets(DNInfo info, ItemInfo item, out Tuple<string, string> unit)
        {
            unit = null;
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return 0;
            var items = info.PalletInfos.Where(p => p.ItemNumber == item.DeliveryItem).ToList();
            if (items.Count > 0)
                unit = new Tuple<string, string>(items[0].PackagingMaterials, items[0].PackagingMaterialsDescription);
            return items.Count;
        }
        string GetCargoType(DNInfo info, string dep, string location)
        {
            if (info.HeaderInfo.SourceSAP == SPMS) return "C";
            //A 成品， B料件 C 售后料件  D 半成品  E 售后半成品
            //switch (info.HeaderInfo.Division)
            //{
            //    case "30":
            //        if (dep == "CS")
            //            return "C";
            //        return "B";
            //}
            switch (info.HeaderInfo.MaterialType)
            {
                case "FERT":
                    return "A";
                case "HALB":
                    if (dep == "CS")
                        return "E";
                    return "D";
                case "ROH":
                    if (dep == "CS")
                        return "C";
                    return "B";
            }
            return "A";
        }
        double? CalcFEU(CNInfo cnInfo)
        {
            if (cnInfo == null) return null;
            double feu = 0;
            if (cnInfo.CNT20.HasValue)
                feu += cnInfo.CNT20.Value / 2;
            if (cnInfo.CNT40.HasValue)
                feu += cnInfo.CNT40.Value;
            if (cnInfo.CNT40HQ.HasValue)
                feu += cnInfo.CNT40HQ.Value;
            if (cnInfo.CNTOther.HasValue)
                feu += cnInfo.CNTOther.Value;
            return feu;
        }
        class CNInfo
        {
            public double? CNT20 { get; set; }
            public double? CNT40 { get; set; }
            public double? CNT40HQ { get; set; }
            public double? CNTOther { get; set; }
            public string CNTOtherType { get; set; }
        }
        CNInfo GetCNInfo(DNInfo info)
        {
            List<PalletInfo> items = SelectPallets(info, true);
            if (items == null || items.Count <= 0) return null;
            double cn20 = 0;
            double cn40 = 0;
            double cn40HQ = 0;
            double other = 0;
            string otherType = null;
            Action<PalletInfo> check = p =>
            {
                switch (p.PackagingMaterials)
                {
                    case "PK-100": cn20++; break;
                    case "PK-200": cn40++; break;
                    case "PK-400": cn40HQ++; break;
                    default: otherType = p.PackagingMaterialsDescription;
                        other++;
                        break;
                }
            };
            items.ForEach(item => check(item));
            CNInfo cnInfo = new CNInfo();
            if (cn20 > 0)
                cnInfo.CNT20 = cn20;
            if (cn40 > 0)
                cnInfo.CNT40 = cn40;
            if (cn40HQ > 0)
                cnInfo.CNT40HQ = cn40HQ;
            if (other > 0)
            {
                cnInfo.CNTOther = other;
                cnInfo.CNTOtherType = otherType;
            }
            return cnInfo;
        }
        double? CalcTotalGW(DNInfo info)
        {
            double totalGW = Prolink.Math.GetValueAsDouble(info.HeaderInfo.TotalWeight);
            var items = SelectPalletsForPacking(info);
            if (items == null || items.Count <= 0) return totalGW;
            Action<PalletInfo> add = p =>
            {
                double tareGW = Prolink.Math.GetValueAsDouble(p.TareWeight);
                totalGW += tareGW;
            };
            foreach (var item in items)
            {
                add(item);
            }
            double? tannV = GetTANNValue(info, p => Prolink.Math.GetValueAsDouble(p.GrossWeight));
            if (tannV.HasValue)
                totalGW = totalGW - tannV.Value;
            return totalGW;
        }
        double? CalcTotalAmount(DNInfo dnInfo)
        {
            if (dnInfo.ItemInfos == null || dnInfo.ItemInfos.Length <= 0) return null;
            double val = 0;
            foreach (var item in dnInfo.ItemInfos)
            {
                double v = Prolink.Math.GetValueAsDouble(item.NetValue);
                val += v;
            }
            return val;
        }
        double? CalcTotalCBM(DNInfo dnInfo)
        {
            if (string.IsNullOrEmpty(dnInfo.HeaderInfo.Volume))
                return GetTotalValue(dnInfo, n => Prolink.Math.GetValueAsDouble(n.TotalVolume));
            double? tannV = GetTANNValue(dnInfo, p => Prolink.Math.GetValueAsDouble(p.Volume));
            double cbm = Prolink.Math.GetValueAsDouble(dnInfo.HeaderInfo.Volume);
            if (tannV.HasValue)
                cbm = cbm - tannV.Value;
            return cbm;
        }
        double? CalcTotalNW(DNInfo dnInfo)
        {
            if (string.IsNullOrEmpty(dnInfo.HeaderInfo.NetWeight))
                return GetTotalValue(dnInfo, n => Prolink.Math.GetValueAsDouble(n.NetWeight));
            double? tannV = GetTANNValue(dnInfo, p => Prolink.Math.GetValueAsDouble(p.NetWeight));
            double nw = Prolink.Math.GetValueAsDouble(dnInfo.HeaderInfo.NetWeight);
            if (tannV.HasValue)
                nw = nw - tannV.Value;
            return nw;
        }
        double? GetTotalValue(DNInfo info, Func<PalletInfo, double> f)
        {
            List<PalletInfo> items = SelectPallets(info, false);
            if (items == null || items.Count <= 0) return null;
            return items.Select(n => f(n)).Sum();
        }
        double? GetTANNValue(DNInfo info, Func<ItemInfo, double> f)
        {
            if (info.ItemInfos == null || info.ItemInfos.Length <= 0) return null;
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return null;
            var items = info.ItemInfos.Where(item => item.DeliveryItemCategory == SkipItemType);
            return items.Select(p => f(p)).Sum();
        }

        double CalcPackingTotalCBM(PackingItem item)
        {
            double totalCBM = 0;
            Action<PalletInfo> add = p =>
            {
                double cbm = Prolink.Math.GetValueAsDouble(p.Volume);
                totalCBM += cbm;
            };
            if (item.Pallets == null || item.Pallets.Count <= 0) return totalCBM;
            item.Pallets.ForEach(p => add(p));
            return totalCBM;
        }
        double CalcPackingCBM(PackingItem item)
        {
            if (item.Pallets == null || item.Pallets.Count <= 0) return 0;
            PalletInfo onePl = item.Pallets[0];
            double cbm = Prolink.Math.GetValueAsDouble(onePl.Volume);
            return System.Math.Round(cbm, 3);
        }
        double CalcPackingTotalNW(PackingItem item)
        {
            double totalNW = 0;
            Action<PalletInfo> add = p =>
            {
                double nw = Prolink.Math.GetValueAsDouble(p.NetWeight);

                totalNW += nw;
            };
            item.Pallets.ForEach(p => add(p));
            return totalNW;
        }
        double CalcPackingTotalGW(PackingItem item, List<string> items)
        {
            double totalGW = 0;
            List<PalletInfo> list = new List<PalletInfo>();
            Action<PalletInfo> add = p =>
            {
                double gw = Prolink.Math.GetValueAsDouble(p.GrossWeight);
                totalGW += gw;
            };
            if (item.Pallets == null || item.Pallets.Count <= 0) return 0;
            item.Pallets.ForEach(p => add(p));
            double tareW = item.Pallets.Where(p => !items.Contains(p.ExternalHandlingUnitIdentification)).
                GroupBy(p => p.ExternalHandlingUnitIdentification).Select(n => n.First()).Select(
                pl => Prolink.Math.GetValueAsDouble(pl.TareWeight)).Sum();
            return totalGW + tareW;
        }
        double CalcPackingGW(PackingItem item, List<string> items)
        {
            if (item.Pallets == null || item.Pallets.Count <= 0) return 0;
            PalletInfo onePl = item.Pallets[0];
            double gw = Prolink.Math.GetValueAsDouble(onePl.GrossWeight);
            double tareW = 0;
            if (!items.Contains(onePl.ExternalHandlingUnitIdentification))
                tareW = Prolink.Math.GetValueAsDouble(onePl.TareWeight);
            return gw + tareW;
        }
        string CalcPackingCaseNO(PackingItem item)
        {
            if (item.Pallets != null && item.Pallets.Count > 1)
                return string.Format("{0}-{1}", item.Index, item.Index + item.Pallets.Count - 1);
            return item.Index.ToString();
        }

        const string DNTableM = "SMDN";
        const string DNTableD = "SMDNP";
        IEnumerable<EditInstruct> GetDNEi(EditInstructList dnEiList, string column, string table)
        {
            for (int i = 0; i < dnEiList.Count; i++)
            {
                EditInstruct ei = dnEiList[i];
                if (ei.ID != table) continue;
                yield return ei;
            }
        }
        string GetDNValueM(EditInstructList dnEiList, string column)
        {
            EditInstruct ei = GetDNEi(dnEiList, column, DNTableM).FirstOrDefault();
            if (ei == null) return null;
            string[] names = ei.getNameSet();
            if (!names.Contains(column)) return null;
            return ei.Get(column);
        }
        EditInstruct CreateInvM(EditInstructList dnEiList, ParseContext context, out string invId)
        {
            invId = null;
            int op = context.Operation;
            EditInstruct ei = null;
            string sql = string.Format("SELECT U_ID FROM SMINM WHERE DN_NO={0}", SQLUtils.QuotedStr(context.DNInfo.HeaderInfo.DNNOWithCompanyCode));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0)
            {
                ei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
                invId = System.Guid.NewGuid().ToString();
                ei.Put("U_ID", invId);
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            else
            {
                ei = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                invId = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(invId));
            }
            ei.Put("INCOTERM", context.DNInfo.HeaderInfo.Incoterms1);
            ei.Put("INCOTERM_DESCP", context.DNInfo.HeaderInfo.Incoterms2);
            ei.Put("TRADE_TERM", context.DNInfo.HeaderInfo.Incoterms1);
            ei.Put("TRADETERM_DESCP", context.DNInfo.HeaderInfo.Incoterms2);
            ei.Put("GROUP_ID", Context.GroupId);
            ei.Put("CMP", GetDNValueM(dnEiList, "CMP"));
            ei.Put("INVOICE_TYPE", GetInvoiceType(context.DNInfo));
            ei.Put("DN_NO", context.DNInfo.HeaderInfo.DNNOWithCompanyCode);
            if (context.DNProfileInfo != null)
            {
                context.SIApply.FillInvoiceEi(context.DNProfileInfo, ei);
            }
            return ei;
        }
        string GetInvoiceType(DNInfo info)
        {
            if (!string.IsNullOrEmpty(info.HeaderInfo.RefDNNOWithCompanyCode) && !string.IsNullOrEmpty(info.HeaderInfo.RefCompanyCode))
                return "I";
            else return "O";
        }

        EditInstructList CreateInvD(EditInstructList dnEiList, DNInfo info, int operation, string invId)
        {
            if (string.IsNullOrEmpty(invId)) return null;
            EditInstructList eiList = new EditInstructList();
            EditInstruct delEi = new EditInstruct("SMIND", EditInstruct.DELETE_OPERATION);
            delEi.Condition = string.Format("U_FID={0}", SQLUtils.QuotedStr(invId));
            eiList.Add(delEi);
            for (int i = 0; i < dnEiList.Count; i++)
            {
                EditInstruct dnEi = dnEiList[i];
                if (dnEi.ID != DNTableD) continue;
                if (dnEi.OperationType == EditInstruct.DELETE_OPERATION) continue;
                EditInstruct ei = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("U_FID", invId);
                ei.Put("DN_NO", dnEi.Get("DN_NO"));
                ei.Put("CATEGORY", dnEi.Get("CATEGORY"));
                ei.Put("SEQ_NO", i);
                ei.Put("SAFETY_MODEL", dnEi.Get("SAFETY_MODEL"));
                ei.Put("DELIVERY_ITEM", dnEi.Get("DELIVERY_ITEM"));
                ei.Put("PO_NO", dnEi.Get("PO_NO"));
                ei.Put("SO_NO", dnEi.Get("SO_NO"));
                ei.Put("OPART_NO", dnEi.Get("OPART_NO"));
                ei.Put("PART_NO", dnEi.Get("PART_NO"));
                ei.Put("IPART_NO", dnEi.Get("IPART_NO"));
                ei.Put("OHS_CODE", dnEi.Get("OHS_CODE"));
                ei.Put("IHS_CODE", dnEi.Get("IHS_CODE"));
                ei.Put("PROD_DESCP", dnEi.Get("PROD_DESCP"));
                ei.Put("GOODS_DESCP", dnEi.Get("GOODS_DESCP"));
                ei.Put("BRAND", dnEi.Get("BRAND"));
                ei.Put("QTY", dnEi.Get("QTY"));
                ei.Put("QTYU", dnEi.Get("QTYU"));
                ei.Put("CUR1", dnEi.Get("CUR1"));
                decimal price1 = Prolink.Math.GetValueAsDecimal(dnEi.Get("UNIT_PRICE1"));
                if (price1 <= 0) price1 = Prolink.Math.GetValueAsDecimal(GetFirstSMINDRow(info, dnEi, "UNIT_PRICE1"));
                ei.Put("UNIT_PRICE1", price1);
                decimal amt = Prolink.Math.GetValueAsDecimal(dnEi.Get("VALUE1"));
                if (amt <= 0) amt = Prolink.Math.GetValueAsDecimal(GetFirstSMINDRow(info, dnEi, "AMT"));
                ei.Put("AMT", amt);
                ei.Put("INVOICE_TYPE", "O");
                eiList.Add(ei);
            }
            return eiList;
        }
        List<PackingItem> CreatePackings(DNInfo info)
        {
            List<PalletInfo> list = SelectPallets(info, false);
            int index = 0;
            List<PackingItem> packings = new List<PackingItem>();
            Func<PalletInfo, PackingItem> checkHas = pl =>
            {
                if (packings.Count <= 0) return null;
                foreach (var p in packings)
                {
                    var x = p.Pallets.Where(item => item.ExternalHandlingUnitIdentification == pl.ExternalHandlingUnitIdentification).FirstOrDefault();
                    if (x != null) return p;
                }
                return null;
            };
            Action<PalletInfo> addPacking = pl =>
            {
                PackingItem p = new PackingItem() { Index = index };
                p.Pallets.Add(pl);
                packings.Add(p);
            };
            Func<PalletInfo, PalletInfo, bool> checkIsCombin = (p1, p2) =>
            {
                return p1.PackagingMaterials == p2.PackagingMaterials && p1.QTY == p2.QTY && p1.MaterialNumber == p2.MaterialNumber;
            };
            Action<PalletInfo> onOther = pl =>
            {
                if (packings.Count <= 0)
                {
                    addPacking(pl);
                    return;
                }
                PackingItem p = packings[packings.Count - 1];
                PalletInfo lastPl = p.Pallets[p.Pallets.Count - 1];
                if (checkIsCombin(lastPl, pl))
                    p.Pallets.Add(pl);
                else
                {
                    addPacking(pl);
                }
            };
            list.ForEach(item =>
            {
                var p = checkHas(item);
                if (p == null)
                {
                    index++;
                    onOther(item);
                }
                else
                    addPacking(item);
            });
            return packings;
        }
        class PackingItem
        {
            public PackingItem()
            {
                Pallets = new List<PalletInfo>();
            }
            public int Index { get; set; }
            public List<PalletInfo> Pallets { get; private set; }
        }
        List<PalletInfo> SelectPalletsForPacking(DNInfo info)
        {
            List<PalletInfo> list = SelectPallets(info, false);
            if (list == null || list.Count <= 0) return list;
            return list.GroupBy(p => p.ExternalHandlingUnitIdentification).Select(g => g.FirstOrDefault()).Where(p => p != null).ToList();
        }
        List<PalletInfo> SelectPallets(DNInfo info, bool isContainer)
        {
            if (info.PalletInfos == null || info.PalletInfos.Length <= 0) return null;
            if (isContainer)
                return info.PalletInfos.Where(p => p.PackagingMaterialType == "0011").ToList();
            else
            {
                Func<PalletInfo, bool> check = p =>
                {
                    if (info.ItemInfos == null || info.ItemInfos.Length <= 0) return true;
                    return info.ItemInfos.Where(item => p.ItemNumber == item.DeliveryItem && item.DeliveryItemCategory == SkipItemType).Count() <= 0;
                };

                Func<PalletInfo, bool> check1 = p =>
                {
                    return info.ItemInfos.Where(item => !(p.ItemNumber == item.DeliveryItem && item.DeliveryItemCategory == SkipItemType)).Count() <= 0;
                };
                decimal qty = 0;
                List<string> units = new List<string>();
                foreach (var item in info.PalletInfos)
                {
                    decimal q0 = 0;
                    decimal.TryParse(item.QTY, out q0);
                    qty += q0;
                    if (!units.Contains(item.QTYUnit))
                        units.Add(item.QTYUnit);
                }

                var xx = info.PalletInfos.Where(p => check1(p) || p.PackagingMaterialType == "0011").ToList();

                return info.PalletInfos.Where(p => check(p) && p.PackagingMaterialType != "0011").ToList();
            }
        }

        //void FillPriceInfo(DNInfo dnInfo, EditInstruct ei, string sapId, string dnno, string deiveryItem)
        //{
        //    if (dnInfo.PriceInfos == null || dnInfo.PriceInfos.Length <= 0) return;
        //    int n = 0;
        //    foreach (var item in dnInfo.PriceInfos.Where(
        //        item => item.DNNOWithCompanyCode == dnno && item.SourceSAP == sapId && item.DeliveryItem == deiveryItem))
        //    {
        //        n++;
        //        if (n > 3) return;
        //        string conColumn = string.Format("CON{0}", n.ToString());
        //        ei.Put(conColumn, item.ConditionType);
        //        string unitColumn = string.Format("UNIT_PRICE{0}", n.ToString());
        //        ei.Put(unitColumn, item.Rate);
        //        string curColumn = string.Format("CUR{0}", n.ToString());
        //        ei.Put(curColumn, item.CurrencyKey);
        //        string valueColumn = string.Format("VALUE{0}", n.ToString());
        //        ei.Put(valueColumn, item.Value);
        //    }
        //}
        void FillClassficationInfo(DNInfo dnInfo, EditInstruct ei, string sapId, string dnno, string deiveryItem)
        {
            if (dnInfo.ClassificationInfos == null || dnInfo.ClassificationInfos.Length <= 0) return;
            foreach (var cItem in dnInfo.ClassificationInfos.Where(
                item => item.DNNOWithCompanyCode == dnno && item.SourceSAP == sapId && item.DeliveryItem == deiveryItem))
            {
                switch (cItem.CharacteristicName)
                {
                    case "Z_BRDID": ei.Put("BRAND", cItem.CharacteristicValue); break;
                    case "Z_ASSM": ei.Put("ASSEMBLY_ID", cItem.CharacteristicValue); break;
                    case "Z_SIZE": ei.Put("SIZE", cItem.CharacteristicValue); break;
                    case "Z_PRDLINE": ei.Put("PRODUCT_LINE", cItem.CharacteristicValue); break;
                    case "Z_PARSE": ei.Put("RESOLUTION", cItem.CharacteristicValue); break;
                    case "Z_CMNFTYPE": ei.Put("PROD_TYPE", cItem.CharacteristicValue); break;  //未确认
                    case "Z_BRDTYPE": ei.Put("BRAND_TYPE", cItem.CharacteristicValue); break;
                    case "Z_DOBLY":
                        if (string.IsNullOrEmpty(cItem.CharacteristicValue)) continue;
                        bool isDU = false;
                        if (cItem.CharacteristicValue.ToUpper() == "YES")
                            isDU = true;
                        else
                            isDU = Prolink.Math.GetValueAsBool(cItem.CharacteristicValue, false);
                        ei.Put("DU", isDU ? "Y" : "N");
                        break;
                    //case "Z_PNLLIGHT": ei.Put("", cItem.CharacteristicValue); break;
                    //case "Z_PNLSUPTYPE": ei.Put("", cItem.CharacteristicValue); break;
                    //case "Z_REGION": ei.Put("", cItem.CharacteristicValue); break;
                    //case "Z_SGNTYPE": ei.Put("", cItem.CharacteristicValue); break;
                    //case "Z_SRCTYPE": ei.Put("", cItem.CharacteristicValue); break;
                }
            }
        }

        public ResultInfo ImportDNForSAP(string sapId, string dnno, string location, bool forced = false)
        {
            return ImportDNForSAP(sapId, dnno, location, DNImportModes.ReloadDefault, forced);
        }
        public ResultInfo ImportDNForSAP(string sapId, string dnno, string location, DNImportModes mode, bool forced = false)
        {
            DNEDI dnEDI = new DNEDI();
            DNInfo info = null;
            try
            {
                info = dnEDI.GetDN(sapId, dnno, location);
                WriteEdiLog(sapId, dnno, info);
                return ImportDNList(new DNInfo[] { info }, mode, forced);
            }
            catch (Exception ex)
            {
                return new ResultInfo { ResultCode = "Error", Description = ex.ToString() };
            }
        }

        public void WriteEdiLog(string sapId, string dnno, DNInfo info)
        {
            string str = ToJsonString(info);
            string uid = System.Guid.NewGuid().ToString();
            Business.TPV.Utils.EdiInfo ediInfo = new Business.TPV.Utils.EdiInfo();
            ediInfo.ID = dnno;
            ediInfo.EdiId = "ReloadDN";
            ediInfo.Remark = "";
            ediInfo.CreateBy = sapId;
            ediInfo.Rs = "Receive";
            ediInfo.Status = "Succeed";
            ediInfo.FromCd = "SAP";
            ediInfo.ToCd = "eShipping";
            ediInfo.DataFolder = uid; ;
            ediInfo.RefNO = dnno;
            ediInfo.GroupId = Context.GroupId;
            ediInfo.Cmp = "";
            ediInfo.Stn = "";

            MixedList ml = new MixedList();
            ml.Add(Helper.CreateEDIEi(ediInfo));
            if (!string.IsNullOrEmpty(str))
            {
                EditInstruct ei = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", uid);
                ei.Put("EDI_DATE", str);
                ei.PutExpress("CREATE_DATE", "getdate()");
                ml.Add(ei);
            }
            try
            {
                DB.ExecuteUpdate(ml);
            }
            catch
            {
            }
        }

    }

    public enum DNImportModes { ReloadQuantity, ReloadDefault }
}