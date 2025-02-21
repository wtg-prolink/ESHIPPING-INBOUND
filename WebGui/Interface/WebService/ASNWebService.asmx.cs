using Business.Service;
using Business.TPV.Import;
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
    /// BillingWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class ASNWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入ASN信息", EnableSession = true)]
        public ResultInfo PostAsnInfo(string json, string User, string Password)
        {
            ASNDataManager m = new ASNDataManager();

            ResultInfo result = null;
            if (!CheckLogin(out result))
            {
                if (!CheckLogin(User, Password, out result))
                    return result;
            }

            return m.Import(json);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.SAP; }
        }
    }
}
