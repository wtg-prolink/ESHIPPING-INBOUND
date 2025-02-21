using Business.TPV.Base;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public class OceanBookingManager : ShippingBookingManager<OceanBookingResponse>
    {
        const string BaseCode_Port = "TVST";
        public override Business.Service.ResultInfo ImportInstanceList(IEnumerable<OceanBookingResponse> infos)
        {           
            var result= base.ImportInstanceList(infos);
            foreach (var item in infos)
            {
                OceanBookingResponedEDILog log = new OceanBookingResponedEDILog(item);
                if (result.IsSucceed)
                {
                    Logger.WriteLog(log.CreateSucceed());
                }
                else
                {
                    Logger.WriteLog(log.CreateEx(result.Description));
                }
            }
            return result;
        }

        protected override DataTable QueryBaseCodeDT(List<OceanBookingResponse> infos)
        {
            List<string> types = new List<string>() { BaseCode_Port, BaseCode_Carrier };
            string sql = string.Format("SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE {0}",
                string.Join(" OR ", types.Select(t => string.Format("CD_TYPE={0}", SQLUtils.QuotedStr(t)))));
            return DB.GetDataTable(sql, new string[] { });
        }

        protected override EditInstruct CreateEi(ShippingBookingRuntime<OceanBookingResponse> runtime)
        {
            EditInstruct ei = base.CreateEi(runtime);
            ei.Put("SCAC_CD", runtime.Info.SCAC);
            ei.Put("POR_CD", runtime.Info.POR);
            ei.Put("POR_NAME", runtime.GetPortNM(runtime.Info.POR));
            ei.Put("POL_CD", runtime.Info.POL);
            ei.Put("POL_NAME", runtime.GetPortNM(runtime.Info.POL));
            ei.Put("POD_CD", runtime.Info.POD);
            ei.Put("POD_NAME", runtime.GetPortNM(runtime.Info.POD));
            ei.Put("DEST_CD", runtime.Info.DEST);
            ei.Put("DEST_NAME", runtime.GetPortNM(runtime.Info.DEST));
            //ei.Put("PORT_CD", runtime.Info.Port);
            //ei.Put("PORT_NM", runtime.GetBaseCodeNM(BaseCode_Port,runtime.Info.Port));
            ei.Put("PORT_NM",  runtime.Info.Port);
            ei.PutDate("CUT_BL_DATE", runtime.Info.CutBL);
            ei.PutDate("PORT_DATE", runtime.Info.PortClose1);  //CUT_PORT_DATE  
            ei.PutDate("CUT_PORT_DATE", runtime.Info.PortClose2);  //PORT_DATE  
            ei.PutDate("CUSTOMS_DATE", runtime.Info.DeclClose);
            ei.PutDate("PORT_RLS_DATE", runtime.Info.ReleaseClose);
            //ei.PutDate("RCV_DATE", runtime.Info.AMSClose);//   runtime.Info.CutBL
            string sender = runtime.Info.Sender;
            switch (sender)
            {
                case "0008914000":
                case "0008914001":
                    ei.PutDate("RCV_DATE", runtime.Info.CutBL);//   runtime.Info.AMSClose
                    break;
                default:
                    ei.PutDate("RCV_DATE", runtime.Info.AMSClose);//   runtime.Info.AMSClose
                    break;
            }
            ei.PutDate("RLS_CNTR_DATE", runtime.Info.CntrRelease);
            ei.PutDate("RCV_DOC_DATE", runtime.Info.DeliveryNote);
            ei.PutDate("ETD1", runtime.Info.VesselETD1);
            ei.PutDate("ETD2", runtime.Info.VesselETD2);
            ei.PutDate("ETD3", runtime.Info.VesselETD3);
            ei.PutDate("ETD4", runtime.Info.VesselETD4);
            ei.PutDate("ETA1", runtime.Info.VesselETA1);
            ei.PutDate("ETA2", runtime.Info.VesselETA2);
            ei.PutDate("ETA3", runtime.Info.VesselETA3);
            ei.PutDate("ETA4", runtime.Info.VesselETA4);
            if (runtime.Info.ETD.HasValue)
                ei.PutDate("ETD", runtime.Info.ETD.Value);
            if (runtime.Info.ETA.HasValue)
                ei.PutDate("ETA", runtime.Info.ETA.Value);
            ei.Put("VESSEL1", runtime.Info.Vessel1);
            ei.Put("VESSEL2", runtime.Info.Vessel2);
            ei.Put("VESSEL3", runtime.Info.Vessel3);
            ei.Put("VESSEL4", runtime.Info.Vessel4);
            ei.Put("VOYAGE1", runtime.Info.Voyage1);
            ei.Put("VOYAGE2", runtime.Info.Voyage2);
            ei.Put("VOYAGE3", runtime.Info.Voyage3);
            ei.Put("VOYAGE4", runtime.Info.Voyage4);
            return ei;
        }

        protected override void FillPort(IEnumerable<OceanBookingResponse> infos, Action<string> addPort)
        {
            foreach (var v in infos)
            {
                addPort(v.POR);
                addPort(v.POL);
                addPort(v.POD);
                addPort(v.DEST);
            }
        }
    }
}
