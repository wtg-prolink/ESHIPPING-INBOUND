using Business.Service;
using Business.TPV.Import;
using Business.TPV.Utils;
using Models.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

namespace WebGui.Interface.WebService
{
    /// <summary>
    /// TraceWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class TraceWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入空运货况信息", EnableSession = true)]
        public ResultInfo PostAirTraceInfo(AirTraceInfo info)
        {
            AirTraceManager manager = new AirTraceManager();
            return manager.ImportTraceInfo(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入空运货况信息", EnableSession = true)]
        public ResultInfo PostAirTraceInfoList(AirTraceInfo[] infos)
        {
            AirTraceManager manager = new AirTraceManager();
            return manager.ImportTraceInfoList(infos);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入空运货况信息", EnableSession = true)]
        public ResultInfo PostAirTraceInfoXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            AirTraceManager manager = new AirTraceManager();
            return manager.ImportTraceInfo(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入海运货况信息", EnableSession = true)]
        public ResultInfo PostOceanTraceInfo(OceanTraceInfo info)
        {
            OceanTraceManager manager = new OceanTraceManager();
            return manager.ImportTraceInfo(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入海运货况信息", EnableSession = true)]
        public ResultInfo PostOceanTraceInfoXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            OceanTraceManager manager = new OceanTraceManager();
            return manager.ImportTraceInfo(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入海运货况信息", EnableSession = true)]
        public ResultInfo PostOceanTraceInfoList(OceanTraceInfo[] infos)
        {
            OceanTraceManager manager = new OceanTraceManager();
            return manager.ImportTraceInfoList(infos);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.Trace; }
        }
    }
}
