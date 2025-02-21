using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Prolink.Web.Services;
using System.Data;
using Business;
using Prolink.Web.Utils;
using Prolink.Model;
using Prolink.V6.Model;

namespace WebGui.Webservice
{
    /// <summary>
    /// Summary description for POServices
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class POServices : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetPOList(string buyerCd, string supplierCd,string groupId, string cmp,string stn,string fromCd , string toCd)
        {
            POProcess po = new POProcess();

            RootModel poDt = po.GetPOList(buyerCd, supplierCd, groupId, cmp, stn, fromCd, toCd);
            WSResult Res = new WSResult(true);
            Res.AddMsgNode(poDt);
            return Res.ToXML();
        }
        [WebMethod]  
        public string GetPODetail(string poNo)
        {
            POProcess po = new POProcess();
            RootModel poDt = po.GetPODetail(poNo);
            WSResult Res = new WSResult(true);
            Res.AddMsgNode(poDt);

            return Res.ToXML();
        }
    }
}
