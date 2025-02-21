using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Service
{
    public interface ISecurity
    {
        bool Check(CertificationInfo certificationInfo, out ResultInfo result);
    }

    public class CertificationInfo
    {
        public CertificationInfo() { }
        public CertificationInfo(string user, string psw)
        {
            User = user;
            Password = psw;
        }

        public string User { get; set; }
        public string Password { get; set; }
    }

    public class ResultInfo
    {
        public bool IsSucceed { get; set; }
        public string ResultCode { get; set; }
        public string Description { get; set; }
        public string RefNo { get; set; }

        public static Func<ResultInfo> SucceedResult = () => { return new ResultInfo { IsSucceed = true, ResultCode = "Succeed", Description = "Successfully!" }; };
        public static Func<ResultInfo> NullDataResult = () => { return new ResultInfo { ResultCode = "NullData", Description = "Not any data processing!" }; };

        public static ResultInfo UnknowResult()
        {
            return UnknowResult("Unknow");
        }
        public static ResultInfo UnknowResult(Exception ex)
        {
            return UnknowResult(ex.Message);
        }
        public static ResultInfo UnknowResult(string msg)
        {
            return new ResultInfo { ResultCode = "Unknow", Description = msg };
        }
    }
}
