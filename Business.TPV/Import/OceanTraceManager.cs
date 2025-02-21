using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Business.Utils;
using TrackingEDI.Business;

namespace Business.TPV.Import
{
    public class OceanTraceManager : ShipTraceManager<OceanTraceInfo>
    {
        protected override OceanTraceInfo OperateData(XmlDocument doc)
        {
            OceanTraceInfo info = new OceanTraceInfo();
            XmlNode root = doc.SelectSingleNode("Trace");
            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "SeaRoutingInfo": HandleRoutingInfo(node, info); break;
                    default: HandleHeaderInfo(node, info); break;
                }
            }
            return info;
        }

        void HandleRoutingInfo(XmlNode mNode, OceanTraceInfo info)
        {
            foreach (XmlNode node in mNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "ContainerNumber": info.RoutingInfo.ContainerNO = node.InnerText; break;
                    case "VesselNo": info.RoutingInfo.VesselNO = node.InnerText; break;
                    case "Voyage": info.RoutingInfo.Voyage = node.InnerText; break;
                    case "EventList": HandleEventList(node, info.RoutingInfo); break;
                    default: HandleRoutingInfo(node, info.RoutingInfo); break;
                }
            }
        }

        void HandleEventList(XmlNode mNode, OceanRoutingInfo info)
        {
            List<OceanEventInfo> list = new List<OceanEventInfo>();
            foreach (XmlNode eNode in mNode.ChildNodes)
            {
                OceanEventInfo eInfo = new OceanEventInfo();
                foreach (XmlNode node in eNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "ConveyanceName": eInfo.ConveyanceName = node.InnerText; break;
                        case "VoyageTripNumber": eInfo.VoyageTripNO = node.InnerText; break;
                        case "CarrierSCAC": eInfo.CarrierSCAC = node.InnerText; break;
                        default: HandleEventList(node, eInfo); break;
                    }
                }
                list.Add(eInfo);
            }
            info.EventInfos = list.ToArray();
        }

        protected override EditInstructList ToEiList(OceanTraceInfo info, IEnumerable<BLInfo> blInfos)
        {
            EditInstructList eiList = new EditInstructList();
            foreach (var blInfo in blInfos)
            {
                foreach (var item in info.RoutingInfo.EventInfos)
                {
                    EditInstruct ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
                    ei.Put("SEQ_NO", System.Guid.NewGuid().ToString());
                    ei.Put("CNTR_NO", info.RoutingInfo.ContainerNO);
                    FillEiForBLInfo(ei, blInfo);
                    FillEiForEventInfo(ei, item);
                    eiList.Add(ei);
                }
            }
            return eiList;
        }
    }
}
