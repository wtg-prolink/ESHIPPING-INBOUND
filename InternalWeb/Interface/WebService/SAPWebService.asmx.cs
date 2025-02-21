using Business.Service;
using Business.TPV.RFC;
using Business.TPV.Service;
using Business.TPV.Utils;
using Models.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace InternalWeb.Interface.WebService
{
    /// <summary>
    /// SAPWebService 的摘要说明
    /// </summary>
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class SAPWebService : Business.TPV.Service.BaseWebService
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

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入单笔Profile资料", EnableSession = true)]
        public ResultInfo PostProfile(ProfileInfo profile)
        {
            return PostProfileList(new Business.TPV.RFC.ProfileInfo[] { profile });
        }


        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入Profile资料", EnableSession = true)]
        public ResultInfo PostProfileList(ProfileInfo[] profileList)
        {
            if (profileList == null || profileList.Length <= 0)
                return new ResultInfo { ResultCode = ResultCode.DataIsNull, Description = ResultCode.GetDescription(ResultCode.DataIsNull) };
            ProfileInfo p = profileList[0];
            ResultInfo result = null;
            if (!CheckLogin(out result))
            {
                if (!CheckLogin(p.User, p.Password, out result))
                    return result;
            }
            Business.TPV.Import.ProfileManager manager = new Business.TPV.Import.ProfileManager();
            return manager.Import(profileList);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.SAP; }
        }
    }
}