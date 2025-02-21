using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
	public class UpdateSmsmiEta
	{
		public static string SetEta(List<string> shipmentids, string Cmp)
		{
			GetDueDate.InitBsDate(Cmp);
			string msg = string.Empty;
			string sql = string.Format(@"SELECT C.U_ID,I.SHIPMENT_ID,CNTR_NO,TRAN_TYPE,I.CMP,POD_CD,CARRIER,ETA,ATA,C.DISCHARGE_DATE,C.EMP_PICK_DATE,C.PICKUP_CDATE,
		CASE WHEN ATA IS NULL THEN ETA ELSE ATA END ARV_DATE,CASE WHEN ATD IS NULL THEN ETD ELSE ATD END DEP_DATE,
		(SELECT TOP 1 PARTY_NO FROM SMSMIPT PT WHERE PT.SHIPMENT_ID=I.SHIPMENT_ID AND PT.PARTY_TYPE='SP') AS LSP_NO
		FROM SMSMI I LEFT JOIN SMICNTR C ON I.SHIPMENT_ID=C.SHIPMENT_ID WHERE I.SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentids.ToArray()));
			DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
			foreach (DataRow dr in dt.Rows)
			{
				MixedList ml = new MixedList();
				string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
				DateTime dep_date = Prolink.Math.GetValueAsDateTime(dr["DEP_DATE"]);
				DateTime arv_date = Prolink.Math.GetValueAsDateTime(dr["ARV_DATE"]);
				string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
				string pod_cd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
				string carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
				string lspno = Prolink.Math.GetValueAsString(dr["LSP_NO"]);
				string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
				string cntr_no = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
				string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
				DateTime eta = Prolink.Math.GetValueAsDateTime(dr["ETA"]);
				List<string> ty = new List<string>() { "F", "R" };
				if (ty.Contains(tran_type))
				{
					Tuple<string, int, int, int, string, Tuple<string, string, string>> item = GetFreeTime.Get(dep_date, arv_date, cmp, pod_cd, carrier, tran_type, lspno);
					int fact_free_time = item.Item3;
					int port_free_time = item.Item2;
					int con_free_time = item.Item4;
					DateTime discharge_date = Prolink.Math.GetValueAsDateTime(dr["DISCHARGE_DATE"]);
					DateTime emp_pick_date = Prolink.Math.GetValueAsDateTime(dr["EMP_PICK_DATE"]);
					DateTime pickup_cdate = Prolink.Math.GetValueAsDateTime(dr["PICKUP_CDATE"]);
					string combine_det = item.Item1;
					string fact_chg_type = item.Item6.Item2;
					string port_chg_type = item.Item6.Item1;
					string con_chg_type = item.Item6.Item3;
					string showcombine_det = item.Item5;
					Tuple<DateTime, DateTime, DateTime> item2 = GetDueDate.Get(fact_free_time, port_free_time, con_free_time, eta, discharge_date, emp_pick_date, pickup_cdate, combine_det, fact_chg_type, port_chg_type, con_chg_type, cmp);

					DateTime dem_due_date = item2.Item1;
					DateTime det_due_date = item2.Item2;
					DateTime sto_due_date = item2.Item3;

					string sql1 = string.Format(@"UPDATE SMORD SET COMBINE_DET={0},SHOW_COMBINE_DET={1},PORT_FREE_TIME={2},FACT_FREE_TIME={3},CON_FREE_TIME={4},
				DEMURRAGE_DUE_DATE={5},DETENTION_DUE_DATE={6},STORAGE_DUE_DATE={7} WHERE SHIPMENT_ID={8} AND CNTR_NO={9}",
				SQLUtils.QuotedStr(combine_det), SQLUtils.QuotedStr(showcombine_det), port_free_time, fact_free_time,
				con_free_time, dem_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(dem_due_date.ToString("yyyy-MM-dd")) : "null",
				 det_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(det_due_date.ToString("yyyy-MM-dd")) : "null",
				 sto_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(sto_due_date.ToString("yyyy-MM-dd")) : "null", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cntr_no));
					ml.Add(sql1);

					string sql2 = string.Format(@"UPDATE SMSMI SET COMBINE_DET={0},SHOW_COMBINE_DET={1},PORT_FREE_TIME={2},
				FACT_FREE_TIME={3},CON_FREE_TIME={4} WHERE SHIPMENT_ID={5}",
				SQLUtils.QuotedStr(combine_det), SQLUtils.QuotedStr(showcombine_det), port_free_time, fact_free_time,
				con_free_time, SQLUtils.QuotedStr(ShipmentId));
					ml.Add(sql2);

					string sql3 = string.Format(@"UPDATE SMICNTR SET DEMURRAGE_DUE_DATE={0},DETENTION_DUE_DATE={1},STORAGE_DUE_DATE={2},
				PORT_CHG_TYPE={3},FACT_CHG_TYPE={4},CON_CHG_TYPE={5} WHERE U_ID={6}",
				dem_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(dem_due_date.ToString("yyyy-MM-dd")) : "null",
				det_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(det_due_date.ToString("yyyy-MM-dd")) : "null",
				sto_due_date > DateTime.MinValue ? SQLUtils.QuotedStr(sto_due_date.ToString("yyyy-MM-dd")) : "null",
				SQLUtils.QuotedStr(port_chg_type), SQLUtils.QuotedStr(fact_chg_type), SQLUtils.QuotedStr(con_chg_type), SQLUtils.QuotedStr(uid));
					ml.Add(sql3);
				}
				if (DBNull.Value == dr["ATA"] && DBNull.Value != dr["ETA"])
				{
					string sql4 = string.Format(@"UPDATE SMCSI SET CS_YEAR=DATEPART(YEAR,{0}),CS_MONTH=DATEPART(MONTH,{0}),
			 CS_QUARTER=DATEPART(QUARTER,{0}),CS_WEEK=DATEPART(WEEK, {0}) WHERE SHIPMENT_ID ={1}",
				eta > DateTime.MinValue ? SQLUtils.QuotedStr(eta.ToString("yyyy-MM-dd")) : "null", SQLUtils.QuotedStr(ShipmentId));
					ml.Add(sql4);
				}
				if (ml.Count > 0)
					try
					{
						OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
					}
					catch (Exception e)
					{
						msg = e.Message;
					}
			}
			return msg;
		}
	}
}
