using Business.Service;
using Business.TPV.Standard;
using Business.TPV.Utils;
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
    /// TruckWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class TruckWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Destination PODImage Response", EnableSession = true)]
        public ResultInfo PostDestPODImagebyXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            TruckerUploadFinalImg t = new TruckerUploadFinalImg();
            return t.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Temp PODImage Response", EnableSession = true)]
        public ResultInfo PostTempPODImagebyXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            TruckerUploadTempImg t = new TruckerUploadTempImg();
            return t.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Transloading Response", EnableSession = true)]
        public ResultInfo PostTranUSTSCinfoXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            TransloadingResponse t = new TransloadingResponse();
            return t.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Delivery Truck Response", EnableSession = true)]
        public ResultInfo PostTranIBCRCinfoXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            DeliveryTruckResponse tr = new DeliveryTruckResponse();
            return tr.ImportXml(doc);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Delivery WareHouseResponse", EnableSession = true)]
        public ResultInfo PostTranIBTWCinfoXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            DeliveryWareHouseResponse tw = new DeliveryWareHouseResponse();
            return tw.ImportXml(doc);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.Truck; }
        }
    }
}
