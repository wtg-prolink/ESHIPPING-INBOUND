using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EDI
{
    public class ResultCode
    {
        public const string UnKnow = "Unknow";
        public const string Succeed = "Succeed";
        public const string NoneLogined = "NoneLogined";
        public const string DBConnectionException = "DBConnectionException";
        public const string MaxDBPool = "MaxDBPool";
        public const string DataIsNull = "DataIsNull";
        public const string ColumnValueIsNull = "ColumnValueIsNull";
        public const string ValidateException = "ValidateException";

        public static string GetDescription(string code, params string[] values)
        {
            ResultCode rsult = new ResultCode();
            return rsult.GetDesc(code, values);
        }

        protected virtual string GetDesc(string code, params string[] values)
        {
            switch (code)
            {
                case UnKnow: return "An unknown error!";
                case Succeed: return "Successfully!";
                case NoneLogined: return "None Logined!";
                case MaxDBPool: return "Max database pool connections!";
                case DBConnectionException: return "Database connection exception!";
            }
            return string.Empty;
        }
    }

    public class SOAPResultCode : ResultCode
    {
        public const string ValidationNotPass = "ValidationNotPass";
        public const string ValidationException = "ValidationException";
        protected override string GetDesc(string code, params string[] values)
        {
            switch (code)
            {
                case ValidationNotPass: return "Validation error, please make sure your user name and password!";
                case ValidationException: return "Verify an exception, please try again later!";
                default: return base.GetDesc(code, values);
            }
        }
    }
}
