using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EDI
{
    public class ApiResult
    {
        public string RETURN_FLAG { get; set; }
        public string ERROR_TYPE { get; set; }
        public string ERROR_INFO { get; set; }

        public static Func<ApiResult> SucceedResult = () => { return new ApiResult { RETURN_FLAG = "1", ERROR_TYPE = "DUS01", ERROR_INFO = "Successfully!" }; };
        public static Func<ApiResult> NullDataResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "SND01", ERROR_INFO = "Not any data processing!" }; };
        public static Func<ApiResult> IncompletedataResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "RDI01", ERROR_INFO = "Incomplete data" }; };
        public static Func<ApiResult> FailResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "DUF01", ERROR_INFO = "Data update failed" }; };
        public static Func<ApiResult> TimeOutResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "DRT01", ERROR_INFO = "Request timeout!" }; };
        public static Func<ApiResult> KeyDiffResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "KVF01", ERROR_INFO = "Key verification failed!" }; };
        public static Func<ApiResult> VoidedResult = () => { return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "DBV01", ERROR_INFO = "Data Voided" }; };

        public static ApiResult UnknowResult()
        {
            return UnknowResult("Unknow");
        }
        public static ApiResult UnknowResult(Exception ex)
        {
            return UnknowResult(ex.Message);
        }
        public static ApiResult UnknowResult(string msg)
        {
            return new ApiResult { RETURN_FLAG = "2", ERROR_TYPE = "UNK01", ERROR_INFO = msg };
        }

        public static ApiResult UrlSucResult(string url)
        {
            return new ApiResult { RETURN_FLAG = "1", ERROR_TYPE = "URL01", ERROR_INFO = url };
        }
    }
}
