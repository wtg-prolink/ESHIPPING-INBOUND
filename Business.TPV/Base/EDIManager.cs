using Business.Import;
using Business.Log;
using Business.TPV.Export;
using Business.TPV.RFC;
using Business.TPV.SFIS;
using Business.TPV.Standard;
using Business.TPV.Utils;
using Models.EDI;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.TPV.Base
{
    class EDIManager
    {
        public static string GetEDIID(EDIModes mode)
        {
            switch (mode)
            {
                case EDIModes.ImportDN:
                    return "ImportDN";
                default: return mode.ToString();
            }
        }
    }

    class EDIRuntime
    {
        public string OPUser { get; set; }
        public string GroupID { get; set; }
        public string CMP { get; set; }
        public string STN { get; set; }
        public string RefNO { get; set; }
    }

    abstract class EDILog
    {
        protected abstract EDIModes Mode { get; }
        protected abstract EDIParty From { get; }
        protected abstract EDIParty To { get; }
        protected abstract EDIRS RS { get; }
        protected EDIRuntime Runtime { get; set; }

        public EdiInfo CreateEx(Exception ex, string refNO = null)
        {
            return CreateEx(ex.ToString(), refNO);
        }
        public EdiInfo CreateEx(string msg, string refNO = null)
        {
            return CreateAction("Exception", msg, refNO);
        }
        public EdiInfo CreateSucceed(string refNO = null)
        {
            return CreateAction("Succeed", "Successfully", refNO);
        }
        public EdiInfo CreateSucceed(string refNO, object obj)
        {
            return CreateAction("Succeed", "Successfully", refNO, stringtoJson(obj));
        }

        public EdiInfo CreateSucceed(string refNO, string ediData, string successremark)
        {
            return CreateAction("Succeed", successremark, refNO, ediData);
        }

        public EdiInfo CreateAction(string status, string caption, string refNO, string EdiData)
        {
            EdiInfo info = CreateEdiInfo(EdiData);
            info.Remark = string.IsNullOrEmpty(info.Remark) ? caption : string.Join(" ", info.Remark, caption);
            info.Status = status;
            if (!string.IsNullOrEmpty(refNO))
                info.RefNO = refNO;
            return info;
        }
        public EdiInfo CreateEx(string msg, string refNO, object obj)
        {
            string txt = stringtoJson(obj);
            return CreateAction("Exception", msg, refNO, txt);
        }

        public string stringtoJson(object obj)
        {
            try
            {
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                Serializer.MaxJsonLength = 40 * 1024 * 1024;
                return Serializer.Serialize(obj);
            }
            catch (Exception ex)
            {
            }
            return string.Empty;
        }

        public EdiInfo CreateAction(string status, string caption, string refNO = null)
        {
            EdiInfo info = CreateEdiInfo();
            info.Remark = string.IsNullOrEmpty(info.Remark) ? caption : string.Join(" ", info.Remark, caption);
            info.Status = status;
            if (!string.IsNullOrEmpty(refNO))
                info.RefNO = refNO;
            return info;
        }
        public EdiInfo CreateAction(ActionModes mode, string actionData = null)
        {
            var v = CreateAction(mode.ToString(), GetActionCaption(mode));
            if (!string.IsNullOrEmpty(actionData))
                v.DataFolder = actionData;
            return v;
        }
        string GetActionCaption(ActionModes mode)
        {
            switch (mode)
            {
                case ActionModes.Validating: return "正在验证数据";
                case ActionModes.Validated: return "数据验证通过";
                case ActionModes.Backup: return "已备份数据";
                case ActionModes.Parsed: return "已解析数据";
                case ActionModes.Processed: return "已处理数据";
                case ActionModes.ReturnResult: return "正在返回结果";
            }
            return mode.ToString();
        }

        public virtual EdiInfo CreateEdiInfo(string EdiData)
        {
            EdiInfo info = new EdiInfo();
            info.ID = System.Guid.NewGuid().ToString();
            info.EdiId = EDIManager.GetEDIID(Mode);
            info.FromCd = From.ToString();
            info.ToCd = To.ToString();
            info.Rs = RS.ToString();
            info.Data = EdiData;
            if (Runtime != null)
            {
                info.CreateBy = Runtime.OPUser;
                info.GroupId = Runtime.GroupID;
                info.Cmp = Runtime.CMP;
                info.Stn = Runtime.STN;
                info.RefNO = Runtime.RefNO;
            }
            return info;
        }

        public virtual EdiInfo CreateEdiInfo()
        {
            EdiInfo info = new EdiInfo();
            info.ID = System.Guid.NewGuid().ToString();
            info.EdiId = EDIManager.GetEDIID(Mode);
            info.FromCd = From.ToString();
            info.ToCd = To.ToString();
            info.Rs = RS.ToString();
            if (Runtime != null)
            {
                info.CreateBy = Runtime.OPUser;
                info.GroupId = Runtime.GroupID;
                info.Cmp = Runtime.CMP;
                info.Stn = Runtime.STN;
                info.RefNO = Runtime.RefNO;
            }
            return info;
        }

        protected DataRow QuerySM(Runtime runtime)
        {
            string sql = string.Format("SELECT GROUP_ID,CMP,STN FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows[0];
        }

        protected EDIRuntime CreateRuntime(Runtime r)
        {
            if (r == null) return null;
            var er = new EDIRuntime
              {
                  RefNO = r.ShipmentID,
                  OPUser = r.OPUser
              };
            DataRow row = QuerySM(r);
            if (row == null) return er;
            er.GroupID = Prolink.Math.GetValueAsString(row["GROUP_ID"]);
            er.CMP = Prolink.Math.GetValueAsString(row["CMP"]);
            er.STN = Prolink.Math.GetValueAsString(row["STN"]);
            return er;
        }
        protected EDIRuntime CreateRuntime(string refNO, string opUser, string groupId, string cmp, string stn)
        {
            return new EDIRuntime
            {
                CMP = cmp,
                GroupID = groupId,
                OPUser = opUser,
                RefNO = refNO,
                STN = stn
            };
        }
    }

    class ImportDNEDILog : ReceiveSAPEDILog
    {
        public ImportDNEDILog(Models.EDI.DNInfo dnInfo, string cmp = null, string stn = null)
        {
            Runtime = CreateRuntime(dnInfo, cmp, stn);
        }

        EDIRuntime CreateRuntime(Models.EDI.DNInfo dnInfo, string cmp, string stn)
        {
            if (dnInfo == null) return null;
            if (dnInfo.HeaderInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = dnInfo.HeaderInfo.DNNOWithCompanyCode,
                GroupID = Context.GroupId,
                OPUser = dnInfo.HeaderInfo.CreateBy,
                CMP = cmp,
                STN = stn
            };
            return r;
        }

        public EdiInfo CreateSucceed(string refNO, string fileName)
        {
            EdiInfo info = CreateSucceed(refNO);
            info.DataFolder = fileName;
            return info;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportDN; }
        }
    }

    abstract class ReceiveSAPEDILog : EDILog
    {
        protected override EDIParty From
        {
            get { return EDIParty.SAP; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class ImportProfileEDILog : ReceiveSAPEDILog
    {
        public ImportProfileEDILog(ProfileInfo info,string user)
        {
            Runtime = CreateRuntime(info, user);
        }

        EDIRuntime CreateRuntime(ProfileInfo info,string user)
        {
            if (info == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO =info.ProfileCode,
                OPUser = user
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportProfile; }
        }
    }

    abstract class SendSAPEDILog:EDILog
    {
        protected override EDIParty From
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.SAP; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Send; }
        }
    }

    class FeeEDILog : SendSAPEDILog
    {
        public FeeEDILog(FeeInfo feeInfo, string sendUser)
        {
            Runtime = CreateRuntime(feeInfo, sendUser);
        }

        EDIRuntime CreateRuntime(FeeInfo feeInfo, string sendUser)
        {
            if (feeInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = feeInfo.DNNO,
                OPUser = sendUser,
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.PostFeeInfoToSAP; }
        }
    }

    class PostCargoEDILog : SendSAPEDILog
    {
        public PostCargoEDILog(CargoInfo cargoInfo, string sendUser)
        {
            Runtime = CreateRuntime(cargoInfo, sendUser);
        }

        EDIRuntime CreateRuntime(CargoInfo cargoInfo, string sendUser)
        {
            if (cargoInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = cargoInfo.DNNO,
                GroupID = cargoInfo.GroupId,
                OPUser = sendUser,
                CMP = cargoInfo.CMP,
                STN = cargoInfo.STN
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.PostCargoInfoToSAP; }
        }
    }

    class PackingReceiveEDILog : SendSAPEDILog
    {
        public PackingReceiveEDILog(PackingReceiveInfo packingInfo, string sendUser)
        {
            Runtime = CreateRuntime(packingInfo, sendUser);
        }

        EDIRuntime CreateRuntime(PackingReceiveInfo packingInfo, string sendUser)
        {
            if (packingInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = packingInfo.DNNO,
                OPUser = sendUser,
                GroupID = "TPV",
                CMP = packingInfo.CMP,
                STN = packingInfo.STN
            };
            return r;
        }

        public PackingReceiveEDILog(string dnno, string sendUser)
        {
            Runtime = CreateRuntime(dnno, sendUser);
        }

        EDIRuntime CreateRuntime(string dnno, string sendUser)
        {
            if (dnno == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = dnno,
                OPUser = sendUser,
                GroupID = "TPV",
                CMP = "XM",
                STN = "*"
            };
            return r;
        }


        protected override EDIModes Mode
        {
            get { return EDIModes.PackingReceiveEDI; }
        }
    }


    abstract class TaskEDILog : EDILog
    {
        protected TaskEDILog(FtpImportEvertArgs args)
        {
            Args = args;
            Runtime = CreateRuntime(args);
        }
        protected FtpImportEvertArgs Args { get; private set; }

        EDIRuntime CreateRuntime(FtpImportEvertArgs args)
        {
            if (args == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = args.File == null ? null : args.File.Name,
                GroupID = Context.GroupId,
                OPUser = "TaskSystem",
                CMP = null,
                STN = null
            };
            return r;
        }

        public override EdiInfo CreateEdiInfo()
        {
            var info = base.CreateEdiInfo();
            if (Args != null)
            {
                info.DataFolder = Args.LocalFileName;
            }
            return info;
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class SFISerNOEDILog : TaskEDILog
    {
        public SFISerNOEDILog(FtpImportEvertArgs args) : base(args) { }

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (Args != null)
            {
                List<SerialNumberInfo> items = Args.Data as List<SerialNumberInfo>;
                if (items != null && items.Count > 0)
                {
                    v.Remark = string.Join(",", items.GroupBy(item => item.DNNO).Select(gItem =>
                        string.Format("{0}:{1}", gItem.Key, gItem.Count().ToString())));
                }
            }
            return v;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.SFISeriaNumber; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.SFIS; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }
    }

    class PackingEDILog : SendSAPEDILog
    {
        public PackingEDILog(PackingInfo packingInfo, string sendUser)
        {
            Runtime = CreateRuntime(packingInfo, sendUser);
        }

        EDIRuntime CreateRuntime(PackingInfo packingInfo, string sendUser)
        {
            if (packingInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = packingInfo.DNNO,
                OPUser = sendUser,
                GroupID = "TPV",
                CMP = packingInfo.CMP,
                STN = packingInfo.STN
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.PostPackingEDI; }
        }
    }

    class PostICAInfoEDILog : SendSAPEDILog
    {
        public PostICAInfoEDILog(ICAInfo icainfo, string sendUser)
        {
            Runtime = CreateRuntime(icainfo, sendUser);
        }

        EDIRuntime CreateRuntime(ICAInfo icainfo, string sendUser)
        {
            if (icainfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = icainfo.DNNO,
                GroupID = icainfo.GroupId,
                OPUser = sendUser,
                CMP = icainfo.CMP,
                STN = icainfo.STN
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.PostICAInfoToSAP; }
        }
    }

    class PostASNDateInfoEDILog : SendSAPEDILog
    {
        public PostASNDateInfoEDILog(ASNDateInfo asninfo, string sendUser)
        {
            Runtime = CreateRuntime(asninfo, sendUser);
        }

        EDIRuntime CreateRuntime(ASNDateInfo asninfo, string sendUser)
        {
            if (asninfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = asninfo.InvNo,
                GroupID = asninfo.GroupId,
                OPUser = sendUser,
                CMP = asninfo.CMP
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.SEND_ASN_LOG; }
        }
    }

    class PostBillInfoEDILog : SendSAPEDILog
    {
        public PostBillInfoEDILog(Business.TPV.Import.DeliveryPostingInfo DeliveryPostingInfo, string sendUser)
        {
            Runtime = CreateRuntime(DeliveryPostingInfo, sendUser);
        }

        EDIRuntime CreateRuntime(Business.TPV.Import.DeliveryPostingInfo DeliveryPostingInfo, string sendUser)
        {
            if (DeliveryPostingInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = DeliveryPostingInfo.DNNO,
                GroupID = "TPV",
                OPUser = sendUser,
                CMP = DeliveryPostingInfo.CMP,
                STN = "*"
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.PostBillInfoEDI; }
        }
    }


    class SFISProdLineEDILog : TaskEDILog
    {
        public SFISProdLineEDILog(FtpImportEvertArgs args)
            : base(args)
        {
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.SFISProductionLine; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.SFIS; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }
    }

    class SerNOToDBEDILog : EDILog
    {
        public SerNOToDBEDILog(List<SerialNumberInfo> infos, string fileName)
        {
            _fileName = fileName;
            _infos = infos;
            CreateRuntime(infos, fileName);
        }
        List<SerialNumberInfo> _infos;
        string _fileName;

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (!string.IsNullOrEmpty(_fileName))
                v.DataFolder = _fileName;

            if (_infos != null && _infos.Count > 0)
            {
                v.Remark = string.Join(",", _infos.GroupBy(item => item.DNNO).Select(gItem =>
                        string.Format("{0}:{1}", gItem.Key, gItem.Count().ToString())));
            }
            return v;
        }

        EDIRuntime CreateRuntime(List<SerialNumberInfo> infos, string fileName)
        {
            if (infos == null || infos.Count <= 0) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = string.IsNullOrEmpty(fileName) ? null : Path.GetFileNameWithoutExtension(fileName),
                GroupID = Context.GroupId,
                OPUser = "TaskSystem",
                CMP = null,
                STN = null
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.SerialNumberToDB; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.Local; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class SendOrderNOEDILog : EDILog
    {
        public SendOrderNOEDILog(List<OrderInfo> infos, string fileName)
        {
            _fileName = fileName;
            _infos = infos;
            CreateRuntime(infos, fileName);
        }
        List<OrderInfo> _infos;
        string _fileName;

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (!string.IsNullOrEmpty(_fileName))
                v.DataFolder = _fileName;
            return v;
        }

        EDIRuntime CreateRuntime(List<OrderInfo> infos, string fileName)
        {
            if (infos == null || infos.Count <= 0) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = string.IsNullOrEmpty(fileName) ? null : Path.GetFileNameWithoutExtension(fileName),
                GroupID = Context.GroupId,
                OPUser = "TaskSystem",
                CMP = null,
                STN = null
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.SendOrderNO; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.SFIS; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Send; }
        }
    }

    class SendDNStatusEDILog : EDILog
    {
        public SendDNStatusEDILog(List<DNStatusInfo> infos, string fileName)
        {
            _fileName = fileName;
            _infos = infos;
            CreateRuntime(infos, fileName);
        }
        List<DNStatusInfo> _infos;
        string _fileName;

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (!string.IsNullOrEmpty(_fileName))
                v.DataFolder = _fileName;
            if (_infos != null && _infos.Count > 0)
            {
                v.Remark = string.Join(",", _infos.Select(item => string.Join("{0}:{1}", item.CmpDNNO, item.Status == DNStatus.Finished ? "1" : "0")));
            }
            return v;
        }

        EDIRuntime CreateRuntime(List<DNStatusInfo> infos, string fileName)
        {
            if (infos == null || infos.Count <= 0) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = string.IsNullOrEmpty(fileName) ? null : Path.GetFileNameWithoutExtension(fileName),
                GroupID = Context.GroupId,
                OPUser = "TaskSystem",
                CMP = null,
                STN = null
            };
            return r;
        }
        protected override EDIModes Mode
        {
            get { return EDIModes.SendDNStatus; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.SFIS; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Send; }
        }
    }

    abstract class BookingEDILog : EDILog
    {
        protected BookingEDILog(Runtime runtime, DataRow smRow)
        {
            CurrentRuntime = runtime;
            SMRow = smRow;
            Runtime= CreateRuntime(runtime, smRow);
        }

        protected Runtime CurrentRuntime{get;private set;}      
        protected DataRow SMRow { get;private set; }

        EDIRuntime CreateRuntime(Runtime runtime, DataRow smRow)
        {
            if (runtime == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = runtime.ShipmentID,
                OPUser = runtime.OPUser
            };
            if (smRow == null) return r;
            r.GroupID = Prolink.Math.GetValueAsString(smRow["GROUP_ID"]);
            r.CMP = Prolink.Math.GetValueAsString(smRow["CMP"]);
            r.STN = Prolink.Math.GetValueAsString(smRow["STN"]);
            return r;
        }

        public override EdiInfo CreateEdiInfo()
        {
            var v= base.CreateEdiInfo();
            if (CurrentRuntime != null)
            {
                if (CurrentRuntime.Data != null)
                    v.DataFolder = CurrentRuntime.Data.ToString();
            }
            return v;
        }

        protected override EDIParty From
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Send; }
        }
    }

    class CPLBookingEDILog : BookingEDILog
    {
        public CPLBookingEDILog(Runtime runtime, DataRow smRow)
            : base(runtime, smRow)
        {

        }

        protected override EDIModes Mode
        {
            get { return EDIModes.CPL_Booking; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.CPL; }
        }   
    }

    class CPLTrackingEDILog : CPLBookingResponed
    {
        public CPLTrackingEDILog(Models.EDI.CPL.BookingResponse rInfo, DataRow smRow)
            : base(rInfo, smRow)
        {

        }
        protected override EDIModes Mode
        {
            get { return EDIModes.CPL_Tracking; }
        }
    }

    class CPLBookingResponed : EDILog
    {
        public CPLBookingResponed(Models.EDI.CPL.BookingResponse rInfo, DataRow smRow)
        {
            CreateRuntime(rInfo, smRow);
        }

        EDIRuntime CreateRuntime(Models.EDI.CPL.BookingResponse rInfo, DataRow smRow)
        {
            if (rInfo == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = rInfo.CustOrderID,
                OPUser = "System"
            };
            if (smRow == null) return r;
            r.GroupID = Prolink.Math.GetValueAsString(smRow["GROUP_ID"]);
            r.CMP = Prolink.Math.GetValueAsString(smRow["CMP"]);
            r.STN = Prolink.Math.GetValueAsString(smRow["STN"]);
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.CPL_BookingResponed; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.CPL; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class DHLBookingEDILog : BookingEDILog
    {
        public DHLBookingEDILog(Runtime runtime, DataRow smRow)
            : base(runtime, smRow)
        {

        }

        protected override EDIModes Mode
        {
            get { return EDIModes.DHL_Booking; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.DHL; }
        }
    }

    class DHLTrackingEDILog : TaskEDILog
    {
        public DHLTrackingEDILog(FtpImportEvertArgs args)
            : base(args)
        {

        }

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (Args != null)
            {
                List<TrackingEDI.Model.Status> status = Args.Data as List<TrackingEDI.Model.Status>;
                if (status != null && status.Count > 0)
                {
                    v.Remark = string.Join(",", status.Select(item => item.ShipmentId).Distinct());
                }
            }
            return v;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.DHL_Tracking; }
        }

        protected override EDIParty From
        {
            get { return EDIParty.DHL; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class XPIBookingEDILog : BookingEDILog
    {
        public XPIBookingEDILog(Runtime runtime, DataRow smRow) : base(runtime, smRow) { }
        protected override EDIModes Mode
        {
            get { return EDIModes.XPI_Booking; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.XPI; }
        } 
    }

    class TNTBookingEDILog : BookingEDILog
    {
        public TNTBookingEDILog(Runtime runtime, DataRow smRow) : base(runtime, smRow) { }

        protected override EDIModes Mode
        {
            get { return EDIModes.TNT_Booking; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.TNT; }
        }
    }

    class CoscoBookingEDILog : BookingEDILog
    {
        public CoscoBookingEDILog(Runtime runtime, DataRow smRow) : base(runtime, smRow) { }

        protected override EDIModes Mode
        {
            get { return EDIModes.COSCO_Booking; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.COSCO; }
        }
    }

    enum FunctionMode{UnKnow,TNT,XPI,COSCO};
    abstract class StandardBookingResponedEDILog<T> : EDILog where T:InfoBase
    {
        protected StandardBookingResponedEDILog(T info)
        {
            Info = info;
            Runtime=CreateRuntime(info);
        }

        protected T Info { get; private set; }

        EDIConfig _config;
        protected EDIConfig GetConfig()
        {
            if(_config==null)
            _config= Context.GetEDIConfig(Info.Sender);
            return _config;
        }

        public override EdiInfo CreateEdiInfo()
        {
            var v= base.CreateEdiInfo();
            if (Info != null)
            {
                if (Info.Data != null)
                    v.DataFolder = Info.Data.ToString();
            }
            return v;
        }

        protected FunctionMode GetFuncMode()
        {
            var c=GetConfig();
            if(c==null) return FunctionMode.UnKnow;
            switch (c.FunctionCode)
            {
                case "XPI": return FunctionMode.XPI;
                case "TNT": return FunctionMode.TNT;
                case "COSCO": return FunctionMode.COSCO;
            }
            return FunctionMode.UnKnow;
        }

        EDIRuntime CreateRuntime(T info)
        {
            if (info == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = info.ShipmentID,
                OPUser = info.Sender
            };
            FillLocation(r);
            return r;
        }

        protected void FillLocation(EDIRuntime r)
        {
            if (Info == null || r == null) return;
            if (Info.SMRows == null || Info.SMRows.Count <= 0) return;
            var br = Info.SMRows.Where(row => row != null && Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]) == r.RefNO).FirstOrDefault();
            if (br == null) return;
            r.GroupID = Prolink.Math.GetValueAsString(br["GROUP_ID"]);
            r.CMP = Prolink.Math.GetValueAsString(br["CMP"]);
            r.STN = Prolink.Math.GetValueAsString(br["STN"]);
        }

        protected override EDIParty To
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Receive; }
        }
    }

    class ExpressBookingResponedEDILog : StandardBookingResponedEDILog<ExpressBookingResponse>
    {
        public ExpressBookingResponedEDILog(ExpressBookingResponse info) : base(info) { }

        protected override EDIModes Mode
        {
            get
            {
                switch (GetFuncMode())
                {
                    case FunctionMode.TNT: return EDIModes.TNT_BookingResponed;
                }
                return EDIModes.UnKnow;
            }
        }

        protected override EDIParty From
        {
            get
            {
                switch (Mode)
                {
                    case EDIModes.TNT_BookingResponed: return EDIParty.TNT;
                }
                return EDIParty.UnKnow;
            }
        }
    }

    class OceanBookingResponedEDILog : StandardBookingResponedEDILog<OceanBookingResponse>
    {
        public OceanBookingResponedEDILog(OceanBookingResponse info):base(info)
        {

        }
        protected override EDIModes Mode
        {
            get
            {
                switch (GetFuncMode())
                {
                    case FunctionMode.XPI: return EDIModes.XPI_BookingResponed;
                    case FunctionMode.COSCO: return EDIModes.COSCO_BookingResponed;
                }
                return EDIModes.UnKnow;
            }
        }

        protected override EDIParty From
        {
            get
            {
                switch (Mode)
                {
                    case EDIModes.XPI_BookingResponed: return EDIParty.XPI;
                    case EDIModes.COSCO_BookingResponed: return EDIParty.COSCO;
                }
                return EDIParty.UnKnow;
            }
        }
    }

    class OceanDeclarationResponedEDILog : StandardBookingResponedEDILog<DeclarationInfo>
    {
        public OceanDeclarationResponedEDILog(DeclarationInfo info) : base(info) { }

        protected override EDIModes Mode
        {
            get
            {
                switch (GetFuncMode())
                {
                    case FunctionMode.XPI: return EDIModes.XPI_DeclarationResponed;
                }
                return EDIModes.UnKnow;
            }
        }

        protected override EDIParty From
        {
            get
            {
                switch (Mode)
                {
                    case EDIModes.XPI_DeclarationResponed: return EDIParty.XPI;
                }
                return EDIParty.UnKnow;
            }
        }
    }

    class BillingEDILog : StandardBookingResponedEDILog<BillingInfo>
    {
        public BillingEDILog(BillingInfo info, BillingDetail detail)
            : base(info)
        {
            if (Runtime != null && detail != null)
                Runtime.RefNO = detail.ShipmentID;
            FillLocation(Runtime);
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.Billing; }
        }

        protected override EDIParty From
        {
            get
            {
                switch (GetFuncMode())
                {
                    case FunctionMode.XPI: return EDIParty.XPI;
                    case FunctionMode.COSCO: return EDIParty.COSCO;
                    case FunctionMode.TNT: return EDIParty.TNT;
                }
                return EDIParty.UnKnow;
            }
        }
    }

    class XPIDeclarationEDILog : BookingEDILog
    {
        public XPIDeclarationEDILog(Runtime runtime, DataRow smRow) : base(runtime, smRow) { }

        protected override EDIModes Mode
        {
            get
            {
                return EDIModes.XPI_Declaration;
            }
        }

        protected override EDIParty To
        {
            get { return EDIParty.XPI; }
        } 
    }

    class BaseCodeEDILog : ReceiveSAPEDILog
    {
        public BaseCodeEDILog(List<BaseCodeInfo> infos,string userName)
        {
            Runtime = CreateRuntime(infos, userName);
        }

        EDIRuntime CreateRuntime(List<BaseCodeInfo> infos, string userName)
        {
            if (infos == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                OPUser = userName,
            };
            if (infos == null || infos.Count <= 0) return r;
            r.RefNO = infos[0].Type;
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportBaseCode; }
        }
    }
    class PartnerEDILog : ReceiveSAPEDILog
    {
        public PartnerEDILog(List<CompanyInfo> infos,string refNO, string userName)
        {
            Infos = infos;
            Runtime = CreateRuntime(infos,refNO, userName);
        }

        List<CompanyInfo> Infos { get; set; }

        public override EdiInfo CreateEdiInfo()
        {
            var v=base.CreateEdiInfo();
            if (Infos == null || Infos.Count <= 0) return null;
            v.DataFolder = string.Format("Count:{0}", Infos.Count.ToString());
            return v;
        }

        EDIRuntime CreateRuntime(List<CompanyInfo> infos, string refNO, string userName)
        {
            if (infos == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                OPUser = userName,
                RefNO = refNO
            };
            if (infos == null || infos.Count <= 0) return r;
            if (string.IsNullOrEmpty(r.RefNO))
                r.RefNO = infos[0].PartnerFunction;
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportPartner; }
        }
    }

    class ExchangeRateEDILog:  ReceiveSAPEDILog
    {
        public ExchangeRateEDILog(List<ExchangeRateInfo> infos, string refNO, string userName)
        {
            Infos = infos;
            Runtime = CreateRuntime(infos,refNO, userName);
        }

        List<ExchangeRateInfo> Infos { get; set; }

        public override EdiInfo CreateEdiInfo()
        {
            var v=base.CreateEdiInfo();
            if (Infos == null || Infos.Count <= 0) return null;
            v.DataFolder = string.Format("Count:{0}", Infos.Count.ToString());
            return v;
        }

        EDIRuntime CreateRuntime(List<ExchangeRateInfo> infos, string refNO, string userName)
        {
            if (infos == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                OPUser = userName,
                RefNO = refNO
            };
            if (infos == null || infos.Count <= 0) return r;
            if (string.IsNullOrEmpty(r.RefNO))
                r.RefNO = infos[0].ExchangeRateType;
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportExchangeRate; }
        }
    }

    class UnLoadingPortEDILog : ReceiveSAPEDILog
    {
        public UnLoadingPortEDILog(List<UnloadingPortInfo> infos, string refNO, string userName)
        {
            Infos = infos;
            Runtime = CreateRuntime(infos, refNO, userName);
        }

        List<UnloadingPortInfo> Infos { get; set; }

        public override EdiInfo CreateEdiInfo()
        {
            var v = base.CreateEdiInfo();
            if (Infos == null || Infos.Count <= 0) return null;
            v.DataFolder = string.Format("Count:{0}", Infos.Count.ToString());
            return v;
        }

        EDIRuntime CreateRuntime(List<UnloadingPortInfo> infos, string refNO, string userName)
        {
            if (infos == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                OPUser = userName,
                RefNO = refNO
            };
            if (infos == null || infos.Count <= 0) return r;
            if (string.IsNullOrEmpty(r.RefNO))
                r.RefNO = infos.Count.ToString();
            return r;
        }

        protected override EDIModes Mode
        {
            get { return EDIModes.ImportUnLoadingPort; }
        }
    }

    class FSSPEDILog : EDILog
    {
        string _mode;

        public FSSPEDILog(Runtime runtime, string mode)
        {
            Runtime = CreateRuntime(runtime);
            _mode = mode;
        }

        new EDIRuntime CreateRuntime(Runtime runtime)
        {
            if (runtime == null) return null;
            EDIRuntime r = new EDIRuntime
            {
                RefNO = runtime.RefNo,
                GroupID = Context.GroupId,
                OPUser = runtime.OPUser,
                CMP = runtime.Location,
                STN = null
            };
            return r;
        }

        protected override EDIModes Mode
        {
            get
            {
                switch (_mode)
                {
                    case "INV_C": return EDIModes.SEND_INV_PAY_CHECK;
                    case "EST_I": return EDIModes.SendEstimateFSSP;
                    case "BID_D": return EDIModes.Send_Pay_Item;
                    case "INV_R": return EDIModes.VOID_INM_FSSP;
                    case "INV_V": return EDIModes.VOID_INV_IM;
                }
                return EDIModes.UnKnow;
            }
        }

        protected override EDIParty From
        {
            get { return EDIParty.eShipping; }
        }

        protected override EDIParty To
        {
            get { return EDIParty.FSSP; }
        }

        protected override EDIRS RS
        {
            get { return EDIRS.Send; }
        }
    }

    enum ActionModes { Validating, Backup, Validated, Parsed, Processed, ReturnResult }
    enum EDIParty { UnKnow,SAP, eShipping, SFIS, CPL, DHL, XPI, COSCO, TNT, Local, FSSP };
    enum EDIRS { Receive, Send }
    enum EDIModes
    {
        UnKnow, ImportDN,ImportProfile,ImportBaseCode,ImportPartner,ImportExchangeRate,ImportUnLoadingPort,
        PostFeeInfoToSAP,PostCargoInfoToSAP, SFISProductionLine, SFISeriaNumber, SerialNumberToDB, SendOrderNO, SendDNStatus, CPL_Booking, CPL_Tracking, CPL_BookingResponed,
        DHL_Booking, DHL_Tracking, XPI_Booking, XPI_Declaration, XPI_BookingResponed,XPI_DeclarationResponed,COSCO_Booking, COSCO_BookingResponed, TNT_Booking, TNT_BookingResponed,
        Billing, PostICAInfoToSAP, PostPackingEDI, PackingReceiveEDI, PackingEdocReceive,PostBillInfoEDI, SEND_ASN_LOG, SEND_INV_PAY_CHECK, SendEstimateFSSP, Send_Pay_Item,
        VOID_INM_FSSP, VOID_INV_IM
    }
}
