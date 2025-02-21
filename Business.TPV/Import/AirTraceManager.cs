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
    public class AirTraceManager : ShipTraceManager<AirTraceInfo>
    {
        protected override AirTraceInfo OperateData(XmlDocument doc)
        {
            AirTraceInfo info = new AirTraceInfo();
            XmlNode root = doc.SelectSingleNode("Trace");
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "AIRRoutingInfo": HandleRoutingInfo(node, info); break;
                    default: HandleHeaderInfo(node, info); break;
                }
            }
            return info;
        }
        void HandleRoutingInfo(XmlNode mNode, AirTraceInfo info)
        {
            foreach (XmlNode node in mNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "FlightNo": info.RoutingInfo.FlightNO = node.InnerText; break;
                    case "FlightDt": info.RoutingInfo.FlightDate = node.InnerText; break;
                    case "EventList": HandleEventList(node, info.RoutingInfo); break;
                    default: HandleRoutingInfo(node, info.RoutingInfo); break;
                }
            }
        }
        void HandleEventList(XmlNode mNode, AirRoutingInfo info)
        {
            List<AirEventInfo> list = new List<AirEventInfo>();
            foreach (XmlNode eNode in mNode.ChildNodes)
            {
                AirEventInfo eInfo = new AirEventInfo();
                foreach (XmlNode node in eNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "FlightNo": eInfo.FlightNO = node.InnerText; break;
                        case "FlightDt": eInfo.FlightDate = node.InnerText; break;
                        default: HandleEventList(node, eInfo); break;
                    }
                }
                list.Add(eInfo);
            }
            info.EventInfos = list.ToArray();
        }
        protected override EditInstructList ToEiList(AirTraceInfo info, IEnumerable<BLInfo> blInfos)
        {
            EditInstructList eiList = new EditInstructList();
            foreach (var blInfo in blInfos)
            {
                EditInstruct foEi = new EditInstruct("TKBLFLNO", EditInstruct.INSERT_OPERATION);
                foEi.Put("JOB_NO", blInfo.BLNO);
                foEi.Put("FL_NO", info.RoutingInfo.FlightNO);
                foEi.Put("FTO", info.RoutingInfo.DestinationCode);
                if (!string.IsNullOrEmpty(info.RoutingInfo.ETD))
                    foEi.PutDate("ETD", info.RoutingInfo.ETD);
                if (!string.IsNullOrEmpty(info.RoutingInfo.ETA))
                    foEi.PutDate("ETA", info.RoutingInfo.ETA);
                if (!string.IsNullOrEmpty(info.RoutingInfo.ATD))
                    foEi.PutDate("ATD", info.RoutingInfo.ATD);
                if (!string.IsNullOrEmpty(info.RoutingInfo.ATA))
                    foEi.PutDate("ATA", info.RoutingInfo.ATA);
                eiList.Add(foEi);
                foreach (var item in info.RoutingInfo.EventInfos)
                {
                    EditInstruct ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
                    ei.Put("SEQ_NO", System.Guid.NewGuid().ToString());
                    ei.Put("CNTR_NO", info.RoutingInfo.FlightNO);
                    FillEiForBLInfo(ei, blInfo);
                    FillEiForEventInfo(ei, item);
                    eiList.Add(ei);
                }
            }
            return eiList;
        }
    }
}