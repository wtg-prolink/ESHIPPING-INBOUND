using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TrackingEDI.Business
{
    public static class DateTimeUtils
    {
        public static int YearOfYear(string date)
        {
            DateTime curDay = SetDateStrToDate(date);
            return YearOfYear(curDay);
        }
        public static int YearOfYear(DateTime date)
        {
            return date.Year;
        }
        public static int MonthOfYear(string date)
        {
            DateTime curDay = SetDateStrToDate(date);
            return MonthOfYear(curDay);
        }
        public static int MonthOfYear(DateTime date)
        {
            return date.Month;
        }
        public static int WeekOfYear(string date)
        {
            DateTime curDay = SetDateStrToDate(date);
            return WeekOfYear(curDay);
        }
        public static int WeekOfYear(DateTime date)
        {
            int firstdayofweek = Convert.ToInt32(Convert.ToDateTime(date.Year.ToString() + "- " + "1-1 ").DayOfWeek);
            int days = date.DayOfYear;
            int daysOutOneWeek = days - (7 - firstdayofweek);
            if (daysOutOneWeek <= 0)
            {
                return 1;
            }
            else
            {
                int weeks = daysOutOneWeek / 7;
                if (daysOutOneWeek % 7 != 0)
                    weeks++;
                return weeks + 1;
            }
        }

        public static bool IsDate(string strDate)
        {
            try
            {
                SetDateStrToDate(strDate);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static DateTime SetDateStrToDate(string strDate)
        {
            string strDateFormat = "yyyyMMdd";
            strDate = strDate.Substring(0, 8);
            DateTime datetime = DateTime.ParseExact(strDate, strDateFormat, new CultureInfo("zh-CN"), DateTimeStyles.AllowWhiteSpaces);
            return datetime;
        }

        /// <summary>
        /// 获取某年某周的第一天 
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="week">第几周</param>
        /// <returns></returns>
        public static DateTime WeekToDate(int year, int week)
        {
            DateTime day = new DateTime(year, 1, 1, 0, 0, 0);
            int w = Week(day);
            int offset = 0;
            if (w != 1)
            {
                day = day.AddDays(8 - w);
            }
            offset = (week - 1) * 7;
            return day.AddDays(offset);
        }

        /// <summary>
        /// 获取时间是星期几    1-7
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static int Week(DateTime day)
        {
            int[] weekdays = { 7, 1, 2, 3, 4, 5, 6 };
            int week = weekdays[Convert.ToInt32(day.DayOfWeek)];
            return week;
        }

        /// <summary>
        /// 日期转成tpv的万年历的年月周
        /// 0：年;
        /// 1：月;
        /// 2：周;
        /// 3：实际的年
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int[] DateToYMW(string date)
        {
            int[] ymw = new int[4];
            date = date.Replace("-", "");
            if (string.IsNullOrEmpty(date) || date.Length < 8)
                return ymw;
            date = date.Substring(0, 8) + "000000";
            DateTime curDay = Prolink.Utils.FormatUtils.ParseDateTime(date, "yyyyMMddHHmmss");
            //DateTime curDay = SetDateStrToDate(date);
            ymw[0] = curDay.Year;
            ymw[1] = curDay.Month;
            ymw[3] = curDay.Year;
            DateTime day = WeekToDate(curDay.Year, 1);
            int offset = 0;
            if (curDay.CompareTo(day) < 0)
            {
                ymw[0] = curDay.Year - 1;
                day = WeekToDate(curDay.Year - 1, 1);
            }
            offset = curDay.Subtract(day).Days;
            ymw[2] = offset / 7 + 1;
            return ymw;
        }

        public static int GetWeekOfYear(DateTime dt, DayOfWeek beginDay)
        {
            GregorianCalendar gc = new GregorianCalendar();
            return gc.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, beginDay);
        }
    }
}
