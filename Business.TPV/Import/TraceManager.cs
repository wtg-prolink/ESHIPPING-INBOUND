using Business.Service;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class TraceManager:TraceManagerBase
    {
        public ResultInfo ImportTraceInfo(TraceInfo traceInfo)
        {
            return ImportTraceInfoList(new List<TraceInfo>() { traceInfo });
        }

        public ResultInfo ImportTraceInfoList(IEnumerable<TraceInfo> infos)
        {
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.RefNO, GetCurrentTimeString()));
                eiList.Add(ToEiList(item));
            }
            return Execute(eiList);
        }


        BLInfo GetBLInfo(string refNo)
        {
            return QueryBLInfo(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(refNo))).FirstOrDefault();           
        }

        EditInstruct ToEiList(TraceInfo info)
        {
            BLInfo blInfo = GetBLInfo(info.RefNO);
            EditInstruct ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
            ei.Put("SEQ_NO", System.Guid.NewGuid().ToString());
            ei.Put("SHIPMENT_ID", info.RefNO);
            ei.Put("STS_CD", info.Code);
            ei.Put("STS_DESCP", info.Descp);
            ei.PutDate("EVEN_DATE", info.EventDate);
            ei.Put("LOCATION", info.Location);
            ei.Put("REMARK", info.Remark);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("LOCATION_DESCP", info.LocationName);            
            if (blInfo != null)
            {
                FillEiForBLInfo(ei, blInfo);
            }
            return ei;
        }
    }
}
