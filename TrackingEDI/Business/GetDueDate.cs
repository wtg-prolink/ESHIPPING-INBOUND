using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
	public class GetDueDate
	{
		private static DataTable bsDateDt { get; set; }

		public static void InitBsDate(string Cmp)
		{
			bsDateDt = Utils.DayHelper.GetBsdate(Cmp);
		}

		public static Tuple<DateTime, DateTime, DateTime> Get(int fact_free_time, int port_free_time, int con_free_time, DateTime eta, DateTime discharge_date, DateTime emp_pick_date, DateTime pickup_cdate, string combine_det, string fact_chg_type, string port_chg_type, string con_chg_type, string cmp)
		{
			if (bsDateDt == null || bsDateDt.Rows.Count <= 0)
			{
				InitBsDate(cmp);
			}
			DateTime sto_due_date = DateTime.MinValue, dem_due_date = DateTime.MinValue, det_due_date = DateTime.MinValue;
			if (fact_free_time > 0)
			{
				if (eta > DateTime.MinValue)
				{
					sto_due_date = CountWorkdays(fact_free_time, eta, fact_chg_type);
				}
				if (discharge_date > DateTime.MinValue)
				{
					sto_due_date = CountWorkdays(fact_free_time, discharge_date, fact_chg_type);
				}
			}

			if (port_free_time > 0)
			{
				if (eta > DateTime.MinValue)
				{
					dem_due_date = CountWorkdays(port_free_time, eta, port_chg_type);
				}
				List<string> combinedet = new List<string>() { "UY", "UN" };
				if (emp_pick_date > DateTime.MinValue && combinedet.Contains(combine_det))
				{
					dem_due_date = CountWorkdays(port_free_time, emp_pick_date, port_chg_type);
				}
				if (discharge_date > DateTime.MinValue && (!combinedet.Contains(combine_det) || string.IsNullOrEmpty(combine_det)))
				{
					dem_due_date = CountWorkdays(port_free_time, discharge_date, port_chg_type);
				}
			}
			if (con_free_time > 0)
			{
				List<string> combinedet = new List<string>() { "YY", "UY" };
				if (combinedet.Contains(combine_det) && eta > DateTime.MinValue)
				{
					det_due_date = CountWorkdays(con_free_time, eta, con_chg_type);
				}
				if (combinedet.Contains(combine_det) && discharge_date > DateTime.MinValue)
				{
					det_due_date = CountWorkdays(con_free_time, discharge_date, con_chg_type);
				}
				if ((!combinedet.Contains(combine_det) || string.IsNullOrEmpty(combine_det)) && pickup_cdate > DateTime.MinValue)
				{
					det_due_date = CountWorkdays(con_free_time, pickup_cdate, con_chg_type);
				}
			}

			return new Tuple<DateTime, DateTime, DateTime>(dem_due_date, det_due_date, sto_due_date);
		}

		public static DateTime CountWorkdays(int Days, DateTime StartDate, string ChgDayType)
		{
			DateTime ResultDate = StartDate;
			if (ChgDayType == "W")
			{
				ResultDate = Utils.DayHelper.AddWorkDayNoHolidays(ResultDate, Days, bsDateDt);
			}

			if (ChgDayType == "C" || string.IsNullOrEmpty(ChgDayType))
			{
				ResultDate = StartDate.AddDays(Days);
			}

			return ResultDate;
		}
	}
}
