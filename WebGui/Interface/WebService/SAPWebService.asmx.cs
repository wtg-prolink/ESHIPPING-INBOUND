using Models.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace WebGui.Interface.WebService
{
    /// <summary>
    /// SAPWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class SAPWebService : BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入单笔DN资料", EnableSession = true)]
        public ResultInfo PostDN(DNInfo dnInfo)
        {
            return PostDNList(new DNInfo[] { dnInfo });
        }


        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入DN资料", EnableSession = true)]
        public ResultInfo PostDNList(DNInfo[] dnInfoList)
        {
            if (dnInfoList == null || dnInfoList.Length <= 0)
                return new ResultInfo { ResultCode = ResultCode.DataIsNull, Description = ResultCode.GetDescription(ResultCode.DataIsNull) };
            DNInfo dn = dnInfoList[0];
            ResultInfo result = null;
            if (!CheckLogin(out result))
            {
                if (!CheckLogin(dn.User, dn.Password, out result))
                    return result;
            }
            Business.TPV.Import.DNManager manager = new Business.TPV.Import.DNManager();
            return manager.ImportDNList(dnInfoList);
        }

        protected override Business.Utils.SecurityModes Mode
        {
            get { return Business.Utils.SecurityModes.SAP; }
        }
    }
}