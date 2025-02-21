using Business.Service;
using Business.TPV.Standard;
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
    /// DeclarationWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class DeclarationWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入报送反馈信息", EnableSession = true)]
        public ResultInfo PostDeclarationInfo(DeclarationInfo info)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            DeclarationManager manager = new DeclarationManager();
            return manager.ImportInstance(info);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入报送反馈信息", EnableSession = true)]
        public ResultInfo PostDeclarationInfoList(DeclarationInfo[] infos)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            DeclarationManager manager = new DeclarationManager();
            return manager.ImportInstanceList(infos);
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入报送反馈信息(XML格式)", EnableSession = true)]
        public ResultInfo PostDeclarationInfoXml(string xml)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            DeclarationManager manager = new DeclarationManager();
            return manager.ImportXml(doc);
        }

        protected override Business.TPV.Utils.SecurityModes Mode
        {
            get { return Business.TPV.Utils.SecurityModes.Declaration; }
        }
    }
}
