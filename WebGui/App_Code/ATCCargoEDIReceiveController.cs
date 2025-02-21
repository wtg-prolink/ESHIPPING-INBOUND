
using Models.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Business;
using Business.TPV.Import;
using System.Web.Script.Serialization;

namespace WebGui.App_Code
{
    public class ATCCargoEDIReceiveController : ApiController
    {
        public ApiResult Post()
        {
            HttpRequest request = HttpContext.Current.Request;
            byte[] requestData = new byte[request.InputStream.Length];
            request.InputStream.Read(requestData, 0, (int)request.InputStream.Length);
            string value = Encoding.UTF8.GetString(requestData);

            decimal atdiff = 10;
            string ts = request.Headers["ts"];
            string toCode = request.Headers["toCode"];
            string sign = request.Headers["sign"];

            List<string> keylist = new List<string>();
            keylist.Add(ts);
            keylist.Add(toCode);
            
            ApiResult apiresult = new ApiResult();
            
            //if (!ApiHelper.CheckEffectiveness(ref apiresult, ts, atdiff, keylist, sign, true))
                return apiresult;
            
            /*string ErrorMessage = "";
            ATCCargoReceiveInfo ATCCargoInfo = new ATCCargoReceiveInfo();
            ATCCargoReceiveManager AtccargoManager = new ATCCargoReceiveManager();
            ErrorMessage = AtccargoManager.Import(ATCCargoInfo, value);

            if(!string.IsNullOrEmpty(ErrorMessage))
                return ApiResult.UnknowResult(ErrorMessage);
            return ApiResult.SucceedResult();
            */
        }
    }
}
