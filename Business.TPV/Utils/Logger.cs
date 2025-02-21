using Business.Log;
using Business.Utils;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Utils
{
    class Logger : DLogger
    {
        protected override List<string> GetCheckTables()
        {
            return new List<string> { "SYS_LOG", "EDI_LOG" };
        }

        static List<ColumnInfo> _items;

        protected override List<ColumnInfo> GetColumnsInfos()
        {
            if (_items == null)
                _items = base.GetColumnsInfos();
            return _items;
        }

        EditInstruct CreateEDIEi(EdiInfo ediInfo)
        {
            EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", ediInfo.ID);
            ei.Put("EDI_ID", ediInfo.EdiId);
            ei.PutExpress("EVENT_DATE", "getdate()");
            ei.Put("REMARK", ediInfo.Remark);
            ei.Put("SENDER", ediInfo.CreateBy);
            ei.Put("RS", ediInfo.Rs);
            ei.Put("STATUS", ediInfo.Status);
            ei.Put("FROM_CD", ediInfo.FromCd);
            ei.Put("TO_CD", ediInfo.ToCd);
            ei.Put("DATA_FOLDER", ediInfo.DataFolder);
            ei.Put("REF_NO", ediInfo.RefNO);
            ei.Put("GROUP_ID", ediInfo.GroupId);
            ei.Put("CMP", ediInfo.Cmp);
            ei.Put("STN", ediInfo.Stn);
            return ei;
        }

        EditInstruct CreateLogEi(LogInfo info)
        {
            EditInstruct ei = new EditInstruct("SYS_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("ID", info.ID);
            ei.Put("MsgType", info.MsgType);
            ei.Put("RefNO", info.RefNO);
            ei.Put("Remark", info.Remark);
            ei.Put("TCode", info.TCode);
            ei.Put("IP", info.IP);
            ei.Put("MsgInfo", info.MsgInfo);
            ei.Put("MsgInfo2", info.MsgInfo2);
            ei.Put("MsgLevel", info.MsgLevel);
            ei.Put("CreateBy", info.CreateBy);
            ei.PutExpress("EventTime", "getdate()");
            if (info.Data != null)
                ei.Put("Data", ToJsonString(info.Data));
            return ei;
        }

        public override void WriteLog(LogInfo info)
        {
            if (info == null) return;
            EditInstruct ei = null;
            EdiInfo ediInfo = info as EdiInfo;
            if (ediInfo != null) ei = CreateEDIEi(ediInfo);
            else
                ei = CreateLogEi(info);
            if (ei == null) return;
            try
            {
                CheckValue(ei);
                DB.ExecuteUpdate(ei);
            }
            catch
            {

            }
        }       
    }

    public class EdiInfo : LogInfo
    {
        public string EdiId { get; set; }
        public string Rs { get; set; }
        public string Status { get; set; }
        public string FromCd { get; set; }
        public string ToCd { get; set; }
        public string DataFolder { get; set; }
        public string GroupId { get; set; }
        public object Cmp { get; set; }
        public object Stn { get; set; }
    }
}