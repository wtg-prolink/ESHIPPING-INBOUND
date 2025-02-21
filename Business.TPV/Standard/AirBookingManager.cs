using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public class AirBookingManager : ShippingBookingManager<AirBookingResponse>
    {
        //protected override AirBookingResponse OperateData(XmlDocument doc)
        //{
        //    AirBookingResponse info = new AirBookingResponse();
        //    XmlNode root = doc.SelectSingleNode("BOOKINGRESPONSE");
        //    foreach (XmlNode node in root.ChildNodes)
        //    {
        //        switch (node.Name)
        //        {
        //            case "ORIGINAL": info.Original = node.InnerText; break;
        //            case "AIR_DEST": info.DEST = node.InnerText; break;
        //            case "F_AIR_DEST": info.LastDEST = node.InnerText; break;
        //            case "FLIGHT_ETD": info.ETD = ParseToDateTimeForNullValue(node.InnerText); break;
        //            case "FLIGHT_ETA": info.ETA = ParseToDateTimeForNullValue(node.InnerText); break;
        //            case "FLIGHT1": info.Flight1 = node.InnerText; break;
        //            case "FLIGHT2": info.Flight2 = node.InnerText; break;
        //            case "FLIGHT3": info.Flight3 = node.InnerText; break;
        //            default: HandleHeaderInfo(node, info); break;
        //        }
        //    }
        //    return info;
        //}

        protected override EditInstruct CreateEi(ShippingBookingRuntime<AirBookingResponse> runtime)
        {
            EditInstruct ei = base.CreateEi(runtime);
            AirBookingResponse info = runtime.Info;
            ei.Put("DEST_CD", info.DEST);
            ei.Put("DEST_NAME", runtime.GetPortNM(info.DEST));
            if (info.ETA.HasValue)
                ei.PutDate("ETA", info.ETA.Value);
            if (info.ETD.HasValue)
                ei.PutDate("ETD", info.ETD.Value);
            ei.Put("VOYAGE1", info.Flight1);
            ei.Put("VOYAGE2", info.Flight2);
            ei.Put("VOYAGE3", info.Flight3);
            ei.Put("POD_CD", info.LastDEST);
            ei.Put("POD_NAME", runtime.GetPortNM(info.LastDEST));
            ei.Put("POL_CD", info.Original);
            ei.Put("POL_NAME", runtime.GetPortNM(info.Original));
            return ei;
        }

        protected override void FillPort(IEnumerable<AirBookingResponse> infos, Action<string> addPort)
        {
            foreach (var v in infos)
            {
                addPort(v.Original);
                addPort(v.DEST);
                addPort(v.LastDEST);
            }
        }
    }
}
