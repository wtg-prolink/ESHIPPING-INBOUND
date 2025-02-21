using Models.EDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{
    public class ApiHelper
    {
        public static bool CheckEffectiveness(ref ApiResult apiresult, string TimeStamp, decimal atdiff, List<string> keylist, string keyno, bool isTicks = false)
        {
            if (!CheckTime(TimeStamp, atdiff, isTicks))
            {
                apiresult = ApiResult.TimeOutResult();
                return false;
            }
            string secret_key = Getsecretkey();
            if (isTicks)
                secret_key = GetCnOBsecretkey();
            keylist.Insert(0, secret_key);
            if (!CheckKeyNo(keyno, keylist))
            {
                apiresult = ApiResult.KeyDiffResult();
                return false;
            }
            return true;
        }

        private static bool CheckTime(string TimeStamp, decimal atdiff, bool isTicks = false)
        {
            DateTime stime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (isTicks)
            {
                stime = stime.AddMilliseconds(Convert.ToInt64(TimeStamp));
            }
            else
            {
                stime = Prolink.Math.GetValueAsDateTime(TimeStamp);
            }
            DateTime ntime = DateTime.UtcNow;
            DateTime maxTime = ntime.AddMinutes(System.Math.Abs((double)atdiff));
            DateTime minTime = ntime.AddMinutes(-System.Math.Abs((double)atdiff));
            if (stime.CompareTo(maxTime) <= 0 && stime.CompareTo(minTime) >= 0)
            {
                return true;
            }
            return false;
        }
        private static string GetCnOBsecretkey()
        {
            return "481d35d7eef280ed76ada0559742d509";
        }

        private static string Getsecretkey()
        {
            return "123456";
        }
        private static bool CheckKeyNo(string keyno, List<string> keylist)
        {
            string tempkeystr = string.Join(":", keylist);
            string keymd5 = GenMD5(tempkeystr).ToUpper();
            keyno = keyno.ToUpper();
            if (!keymd5.Equals(keyno)) return false;
            return true;
        }
        private static string GenMD5(string str)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash((new System.Text.ASCIIEncoding()).GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}