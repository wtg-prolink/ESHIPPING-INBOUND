using Business.Mail;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using Prolink.V3;
using Prolink;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Business.TPV
{
    public class TimeZoneHelper
    {
        /// <summary>
        /// 根據公司別獲取相應時區的時間
        /// </summary>
        /// <param name="odt"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public static DateTime GetTimeZoneDate(DateTime odt, string CompanyId)
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
                    ndt = tz.MexicoTimeZone(odt);
                    break;
                case "RU":
                    ndt = tz.RussianTimeZone(odt);
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
            TimeZoneInfo MX_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            DateTime MX_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, MX_TimeZoneInfo);
            return MX_DateTime;
        }
        #endregion
    }
}