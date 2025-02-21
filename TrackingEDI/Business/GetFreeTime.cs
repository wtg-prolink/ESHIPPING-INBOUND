using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackingEDI.Business
{
	public class GetFreeTime
	{
		public static Tuple<string, int, int, int, string, Tuple<string, string, string>> Get(DateTime dep_date, DateTime arv_date, string cmp, string pod_cd, string carrier, string tran_type, string lspno)
		{
			bool isf = false;
			string sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
			CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND TRAN_TYPE={3} AND FEE_PER_DAY=0 AND (LSP_NO IS NULL OR LSP_NO='') "
			, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tran_type));
			DataTable tmp_table = null;
			if (!string.IsNullOrEmpty(lspno))
			{
				if (tran_type == "F")
				{
					sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
			CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND TRAN_TYPE={3} AND FEE_PER_DAY=0 AND LSP_NO=(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={4} AND STATUS='U') "
				, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
					isf = true;
				}
				else
				{
					sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
			CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND TRAN_TYPE={3} AND FEE_PER_DAY=0 AND LSP_NO={4} "
					, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
				}
				tmp_table = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
				if (isf && (tmp_table == null || tmp_table.Rows.Count <= 0))
				{
					sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
			CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND TRAN_TYPE={3} AND FEE_PER_DAY=0 AND LSP_NO={4} "
					, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
					tmp_table = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
				}
			}
			tmp_table = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
			if (tmp_table == null || tmp_table.Rows.Count <= 0)
			{
				isf = false;
				sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
				CMP={0} AND POD_CD={1} AND CARRIER_CD IS NULL AND TRAN_TYPE={2} AND FEE_PER_DAY=0 AND (LSP_NO IS NULL OR LSP_NO='')"
						, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(tran_type));
				if (!string.IsNullOrEmpty(lspno))
				{
					if (tran_type == "F")
					{
						sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
						CMP={0} AND POD_CD={1} AND CARRIER_CD IS NULL AND TRAN_TYPE={2} AND FEE_PER_DAY=0 AND LSP_NO=(SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={3} AND STATUS='U')"
						, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
						isf = true;
					}
					else
					{
						sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
						CMP={0} AND POD_CD={1} AND CARRIER_CD IS NULL AND TRAN_TYPE={2} AND FEE_PER_DAY=0 AND LSP_NO={3}"
						, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
					}
				}
				tmp_table = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
				if (isf && (tmp_table == null || tmp_table.Rows.Count <= 0))
				{
					sql = string.Format(@"SELECT E_DAY,I_TYPE,CAL_DATE,EFFECT_DATE,EXPIRAT_DATE,CHG_DAY_TYPE FROM SMQTI WHERE 
			CMP={0} AND POD_CD={1} AND CARRIER_CD={2} AND TRAN_TYPE={3} AND FEE_PER_DAY=0 AND LSP_NO={4} "
					, SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(tran_type), SQLUtils.QuotedStr(lspno));
					tmp_table = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
				}
			}
			string COMBINE_DET = string.Empty, iscombine_det = string.Empty, showcombine_det = string.Empty, PORT_CHG_DAY_TYPE = string.Empty, FACT_CHG_DAY_TYPE = string.Empty, CON_CHG_DAY_TYPE = string.Empty;
			int PORT_FREE_TIME = 0, FACT_FREE_TIME = 0, CON_FREE_TIME = 0;
			if (tmp_table != null && tmp_table.Rows.Count > 0)
			{
				COMBINE_DET = "N";
				DataRow[] drs = tmp_table.Select(string.Format(@"I_TYPE IN ('DDEM','BOTH','USAGE') AND CAL_DATE='A' AND 
				EFFECT_DATE<={0} and EXPIRAT_DATE>={0} ", SQLUtils.QuotedStr(arv_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				if (drs == null || drs.Length <= 0)
				{
					drs = tmp_table.Select(string.Format(@"I_TYPE IN ('DDEM','BOTH','USAGE') AND CAL_DATE='D' AND EFFECT_DATE<={0} and 
					EXPIRAT_DATE>={0} ", SQLUtils.QuotedStr(dep_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				}
				if (drs != null && drs.Length > 0)
				{
					foreach (DataRow dr in drs)
					{
						PORT_FREE_TIME = Prolink.Math.GetValueAsInt(dr["E_DAY"]);
						COMBINE_DET = Prolink.Math.GetValueAsString(dr["I_TYPE"]);
						switch (COMBINE_DET)
						{
							case "BOTH": COMBINE_DET = "Y"; break;
							case "USAGE": COMBINE_DET = "U"; break;
							default: COMBINE_DET = "N"; break;
						}
						PORT_CHG_DAY_TYPE = Prolink.Math.GetValueAsString(dr["CHG_DAY_TYPE"]);
					}
				}
				iscombine_det = COMBINE_DET;
				COMBINE_DET = "N";

				DataRow[] drs2 = tmp_table.Select(string.Format(@"I_TYPE IN ('DDET','BOTH') AND CAL_DATE='A' AND 
				EFFECT_DATE<={0} and EXPIRAT_DATE>={0}", SQLUtils.QuotedStr(arv_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				if (drs2 == null || drs2.Length <= 0)
				{
					drs2 = tmp_table.Select(string.Format(@"I_TYPE IN ('DDET','BOTH') AND CAL_DATE='D' AND EFFECT_DATE<={0} 
					and EXPIRAT_DATE>={0}", SQLUtils.QuotedStr(dep_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				}
				if (drs2 != null && drs2.Length > 0)
				{
					foreach (DataRow dr in drs2)
					{
						CON_FREE_TIME = Prolink.Math.GetValueAsInt(dr["E_DAY"]);
						COMBINE_DET = Prolink.Math.GetValueAsString(dr["I_TYPE"]);
						switch (COMBINE_DET)
						{
							case "BOTH": COMBINE_DET = "Y"; break;
							case "USAGE": COMBINE_DET = "U"; break;
							default: COMBINE_DET = "N"; break;
						}
						CON_CHG_DAY_TYPE = Prolink.Math.GetValueAsString(dr["CHG_DAY_TYPE"]);
					}
				}
				if (CON_FREE_TIME > 0)
				{
					showcombine_det = COMBINE_DET;
				}
				iscombine_det = iscombine_det + COMBINE_DET;
				DataRow[] drs3 = tmp_table.Select(string.Format(@"I_TYPE IN ('DSTF') AND CAL_DATE='A' AND EFFECT_DATE<={0} 
				and EXPIRAT_DATE>={0}", SQLUtils.QuotedStr(arv_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				if (drs3 == null || drs3.Length <= 0)
				{
					drs3 = tmp_table.Select(string.Format(@"I_TYPE IN ('DSTF') AND CAL_DATE='D' AND EFFECT_DATE<={0} 
					and EXPIRAT_DATE>={0}", SQLUtils.QuotedStr(dep_date.ToString("yyyy-MM-dd"))), "EFFECT_DATE ASC,EXPIRAT_DATE ASC,I_TYPE DESC");
				}
				if (drs3 != null && drs3.Length > 0)
				{
					foreach (DataRow dr in drs3)
					{
						FACT_FREE_TIME = Prolink.Math.GetValueAsInt(dr["E_DAY"]);
						FACT_CHG_DAY_TYPE = Prolink.Math.GetValueAsString(dr["CHG_DAY_TYPE"]);
					}
				}
			}
			if (showcombine_det == "Y")
				PORT_FREE_TIME = 0;

			return new Tuple<string, int, int, int, string, Tuple<string, string, string>>(iscombine_det, PORT_FREE_TIME, FACT_FREE_TIME, CON_FREE_TIME, showcombine_det, new Tuple<string, string, string>(PORT_CHG_DAY_TYPE, FACT_CHG_DAY_TYPE, CON_CHG_DAY_TYPE));
		}
	}
}
