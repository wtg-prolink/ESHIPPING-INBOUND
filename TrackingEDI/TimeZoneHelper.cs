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

namespace TrackingEDI
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
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='LCTZ' AND CD={0} AND CMP='*'", SQLUtils.QuotedStr(CompanyId));
            string zome = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(zome))
                ndt = tz.GetTimeZone(odt, zome);
            return ndt;
        }

        public static DateTime GetTimeZoneDate(string CompanyId, DateTime? odt = null)
        {
            DateTime odt1 = DateTime.Now;
            if (odt != null)
                odt1 = odt.Value;
            return GetTimeZoneDate(odt1, CompanyId);
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
            TimeZoneInfo MX_TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time (Mexico)");
            DateTime MX_DateTime = TimeZoneInfo.ConvertTime(CN_DateTime, MX_TimeZoneInfo);
            return MX_DateTime;
        }
        #endregion

        public DateTime GetTimeZone(DateTime ldateTime, string zome)
        {
            DateTime dateTime = ldateTime;
            try
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zome);
                dateTime = TimeZoneInfo.ConvertTime(ldateTime, timeZoneInfo);
            }
            catch (Exception)
            {

            }
            return dateTime;
        }
    }
}