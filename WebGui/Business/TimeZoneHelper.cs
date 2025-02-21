using Prolink.Data;
using Prolink.DataOperation;
using System;

namespace Business
{
    public class TimeZoneHelper
    {

        /// <summary>
        /// 根據公司別獲取相應時區的時間
        /// </summary>
        /// <param name="odt"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public static DateTime GetTimeZoneDate(string CompanyId, DateTime? odt = null)
        {
            if (odt == null)
                odt = DateTime.Now;
            TimeZoneHelper tz = new TimeZoneHelper();
            DateTime ndt = Prolink.Math.GetValueAsDateTime(odt);
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='LCTZ' AND CD={0} AND CMP='*'", SQLUtils.QuotedStr(CompanyId));
            string zome = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(zome))
                ndt = tz.GetTimeZone(ndt, zome);
            return ndt;
        }
        /// <summary>
        /// 根據公司別獲取相應時區的時間
        /// </summary>
        /// <param name="odt"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public static DateTime GetTimeZoneDate(DateTime odt, string CompanyId,string lcmp="")
        {
            TimeZoneHelper tz = new TimeZoneHelper();
            DateTime ndt = odt;
            switch (CompanyId)
            {
                case "PL":
                    ndt = tz.PolandTimeZone(odt);
                    break;
                case "BR":
                    ndt = tz.BrazilTimeZone(odt);
                    break;
                case "MX":
                case "EN":
                case "TA":
                case "TS":
                    ndt = tz.MexicoTimeZone(odt);
                    break;
                case "RU":
                    ndt = tz.RussianTimeZone(odt);
                    break;
                case "IN":
                    ndt = tz.GetTimeZone(odt, "India Standard Time");
                    break;
                case "CN":
                    ndt = tz.GetCNTimeZone(odt, lcmp);
                    break;
            }
            return ndt;
        }

        #region 轉換成波蘭時區
        public DateTime PolandTimeZone(DateTime CN_DateTime)
        {
            TimeZoneInfo PL_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime PL_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, PL_TimeZoneInfo);
            
            return PL_DateTime;
        }
        #endregion

        #region 轉換成俄羅斯(聖彼得堡)時區
        public DateTime RussianTimeZone(DateTime CN_DateTime)
        {
            TimeZoneInfo RU_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            DateTime RU_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, RU_TimeZoneInfo);
            return RU_DateTime;
        }
        #endregion

        #region 轉換成巴西(瑪瑙斯)時區
        public DateTime BrazilTimeZone(DateTime CN_DateTime)
        {
            TimeZoneInfo BZ_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime BZ_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, BZ_TimeZoneInfo);
            return BZ_DateTime;
        }
        #endregion

        #region 轉換成墨西哥時區
        public DateTime MexicoTimeZone(DateTime CN_DateTime)
        {
            //TimeZoneInfo MX_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            TimeZoneInfo MX_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time (Mexico)");
            DateTime MX_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, MX_TimeZoneInfo);
            return MX_DateTime;
        }

        public DateTime GetTimeZone(DateTime ldateTime,string zome)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zome);
            DateTime dateTime = TimeZoneInfo.ConvertTime(ldateTime, timeZoneInfo);
            return dateTime;
        }
        #endregion

        #region 时区转换
        public DateTime GetCNTimeZone(DateTime ldateTime,string lcmp)
        {
            string zome="China Standard Time";
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            switch (lcmp)
            {
                case "PL":
                    zome="Central European Standard Time";
                    break;
                case "BR":
                    zome="SA Western Standard Time";
                    break;
                case "MX":
                case "EN":
                case "TA":
                case "TS":
                    zome="Pacific Standard Time (Mexico)";
                    break;
                case "RU":
                    zome="Russian Standard Time";
                    break;
                case "IN":
                    zome="India Standard Time";
                    break;
            }
            TimeZoneInfo localZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zome);
            DateTime dateTime = TimeZoneInfo.ConvertTime(ldateTime, localZoneInfo, timeZoneInfo);
            return dateTime;
        }
        #endregion
    }
}