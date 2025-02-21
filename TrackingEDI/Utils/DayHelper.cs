using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Utils
{
    public class DayHelper
    {
        /// <summary>
        /// Calendar Day
        /// </summary>
        public static string CalendarDay = "C";

        /// <summary>
        /// Working Day
        /// </summary>
        public static string WorkingDay = "W";

        public static int GetWorkAndHolidays(DateTime startDate, DateTime endDate, DataTable bsDateDt)
        {
            return GetWorkdaysBetweenDates(startDate, endDate) - Holiday(startDate, endDate, bsDateDt);
        }

        public static DateTime AddWorkHolidays(DateTime startDate, int day, DataTable bsDateDt, string chgDayType)
        {
            DateTime date;
            if (chgDayType != null && chgDayType.Equals(WorkingDay))
            {
                date = AddWorkDayNoHolidays(startDate, day, bsDateDt);
            }
            else
            {
                date = startDate.AddDays(day);
            }
            return date;
        }



        public static int GetWorkdaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            int workdays = 0;
            DateTime currentstartDate;
            DateTime currentEndDate;
            DateTime currentDate;
            int option = 0;
            if (startDate < endDate)
            {
                currentstartDate = startDate;
                currentEndDate = endDate;
                option = 1;
            }
            else
            {
                currentstartDate = endDate;
                currentEndDate = startDate;
                option = -1;
            }
            currentDate = currentstartDate;
            while (currentDate < currentEndDate)
            {
                if (IsWorkingDay(currentDate))
                {
                    workdays += option;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workdays;
        }

        public static DateTime AddWorkDayNoHolidays(DateTime startDate, int days,DataTable bsDateDt)
        {
            DateTime newDate = startDate;
            for (int i = 0; i < Math.Abs(days); i++)
            {
                newDate = days > 0 ? newDate.AddDays(1) : newDate.AddDays(-1);
                while (IsHoliday(newDate, bsDateDt)|| !IsWorkingDay(newDate))
                {
                    newDate = days > 0 ? newDate.AddDays(1) : newDate.AddDays(-1);
                }
            }
            return newDate;
        } 
         
        private static bool IsWorkingDay(DateTime date)
        {
            DayOfWeek dayOfWeek = date.DayOfWeek;
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }
        public static DataTable GetBsdate(string location)
        {
            string sql = $"SELECT DISTINCT D_DAY FROM  BSDATE WHERE CMP={SQLUtils.QuotedStr(location)} AND DATEPART(WEEKDAY, D_DAY) NOT IN(1,7)";
            DataTable bsDateDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return bsDateDt;
        } 

        public static int Holiday(DateTime startDate, DateTime endDate, DataTable dt)
        {
            DateTime currentstartDate;
            DateTime currentEndDate;
            int day = 0;
            if (startDate < endDate)
            {
                currentstartDate = startDate;
                currentEndDate = endDate; 
            }
            else
            {
                currentstartDate = endDate;
                currentEndDate = startDate; 
            }


            DataRow[] rows = dt.Select($"D_DAY>={SQLUtils.QuotedStr(currentstartDate.ToString("yyyy-MM-dd"))} AND D_DAY<={SQLUtils.QuotedStr(currentEndDate.ToString("yyyy-MM-dd"))}");
            if (rows.Length > 0)
            {
                day= startDate < endDate ? rows.Length : -rows.Length;
            }
            return day;
        }

        public static bool IsHoliday(DateTime time, DataTable dt)
        {
            DataRow[] rows = dt.Select($"D_DAY={SQLUtils.QuotedStr(time.ToString("yyyy-MM-dd"))}");
            if (rows.Length > 0)
                return true;
            else
                return false;
        }
    }
}
