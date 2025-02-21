using Business.Service;
using Business.TPV.Standard;
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
    /// BookingWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class BookingWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入空运订舱信息", EnableSession = true)]
        public ResultInfo PostAirBookingResponse(AirBookingResponse info)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            AirBookingManager manager = new AirBookingManager();
            return manager.ImportInstance(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入空运订舱信息", EnableSession = true)]
        public ResultInfo PostAirBookingResponseList(AirBookingResponse[] infos)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            AirBookingManager manager = new AirBookingManager();
            return manager.ImportInstanceList(infos);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入空运订舱信息(XML格式)", EnableSession = true)]
        public ResultInfo PostAirBookingResponseXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            AirBookingManager manager = new AirBookingManager();
            return manager.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入海运订舱信息", EnableSession = true)]
        public ResultInfo PostOceanBookingResponse(OceanBookingResponse info)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            OceanBookingManager manager = new OceanBookingManager();
            return manager.ImportInstance(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入海运订舱信息(XML格式)", EnableSession = true)]
        public ResultInfo PostOceanBookingResponseXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            OceanBookingManager manager = new OceanBookingManager();
            return manager.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入海运订舱信息", EnableSession = true)]
        public ResultInfo PostOceanBookingResponseList(OceanBookingResponse[] infos)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            OceanBookingManager manager = new OceanBookingManager();
            return manager.ImportInstanceList(infos);
        }


        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入快递订舱信息", EnableSession = true)]
        public ResultInfo PostExpressBookingResponse(ExpressBookingResponse info)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            ExpressBookingManager manager = new ExpressBookingManager();
            return manager.ImportInstance(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入快递订舱信息(XML格式)", EnableSession = true)]
        public ResultInfo PostExpressBookingResponseXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            ExpressBookingManager manager = new ExpressBookingManager();
            return manager.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入快递订舱信息", EnableSession = true)]
        public ResultInfo PostExpressBookingResponseList(ExpressBookingResponse[] infos)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            ExpressBookingManager manager = new ExpressBookingManager();
            return manager.ImportInstanceList(infos);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.Booking; }
        }
    }
}
