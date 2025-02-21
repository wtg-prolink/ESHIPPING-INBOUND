using Business.Service;
using Business.TPV.Utils;
using Models.EDI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;

namespace Business.TPV.Service
{
    /// <summary>
    /// BaseWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://218.66.59.12:8020/TPV/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public abstract class BaseWebService :Business.Service.BaseWebService
    {
        protected override CertificationInfo CreateCertificationInfo(string user, string psw)
        {
            return new TPVCertificationInfo(Mode, user, psw);
        }

        protected abstract SecurityModes Mode { get; }
    }
}