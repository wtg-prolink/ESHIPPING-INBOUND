using Business.Service;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Models.EDI.CPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace WebGui.Interface.WebService
{
    /// <summary>
    /// CPLWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class CPLWebService : Business.TPV.Service.BaseWebService
    {
        [SoapHeader("soapHeader")]
        [WebMethod(Description = "传入订舱反馈信息", EnableSession = true)]
        public ResultInfo PostBookingResponse(BookingResponse bookingResponse)
        {
            return PostBookingResponseList(new BookingResponse[] { bookingResponse });
        }

        [SoapHeader("soapHeader")]
        [WebMethod(Description = "批量传入订舱反馈信息", EnableSession = true)]
        public ResultInfo PostBookingResponseList(BookingResponse[] bookingResponse)
        {
            ResultInfo result = null;
            if (!CheckLogin(out result))
                return result;
            Business.TPV.CPL.ImprotManager manager = new Business.TPV.CPL.ImprotManager();
            return manager.ImportBookingResponse(bookingResponse);
        }

        protected override SecurityModes Mode
        {
            get { return SecurityModes.CPL; }
        }
    }
}
