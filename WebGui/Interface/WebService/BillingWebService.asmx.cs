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
    /// BillingWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class BillingWebService : Business.TPV.Service.BaseWebService
    {
        public Dictionary<string, string> GetLang()
        {
            Dictionary<string, string> lang = new Dictionary<string, string>();
            lang.Add("L_ActCheck_Views_0", Resources.Locale.L_ActCheck_Views_0);
            lang.Add("L_ActManage_Controllers_70", Resources.Locale.L_ActManage_Controllers_70);

            return lang;
        }
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入请款信息", EnableSession = true)]
        public ResultInfo PostBillingInfo(BillingInfo info)
        {
            BillingManager m = new BillingManager(GetLang());
            return m.ImportInstance(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入请款信息", EnableSession = true)]
        public ResultInfo PostBillingInfoList(BillingInfo[] infos)
        {
            BillingManager m = new BillingManager(GetLang());
            return m.ImportInstanceList(infos);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入请款信息（xml结构）", EnableSession = true)]
        public ResultInfo PostBillingInfoXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            BillingManager m = new BillingManager(GetLang());
            return m.ImportXml(doc);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.Billing; }
        }
    }
}
