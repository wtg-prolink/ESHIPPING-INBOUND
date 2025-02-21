using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using Business.Service;
using Business.TPV.Export;
using Business.TPV.Utils;

namespace WebGui.Interface.WebService
{
    /// <summary>
    /// Summary description for WareHouseWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WareHouseWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Post Container Info. By DN1No Response", EnableSession = true)]
        public string PostContainerInfoByDN1No(string dnno)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result.Description;
            WHManager wHManager = new WHManager(dnno);
            return wHManager.GetWareHouseInfoByDn1();
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Post Container Info. By DN2No Response", EnableSession = true)]
        public string PostContainerInfoByDN2No(string dnno)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result.Description;
            WHManager wHManager = new WHManager(dnno);
            return wHManager.GetWareHouseInfoByDn2();
        }
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "Post Container Info. By DN3No Response", EnableSession = true)]
        public string PostContainerInfoByDN3No(string dnno)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result.Description;
            WHManager wHManager = new WHManager(dnno);
            return wHManager.GetWareHouseInfoByDn3();
        }
        protected override SecurityModes Mode
        {
            get { return SecurityModes.WareHouse; }
        }
    }
}
