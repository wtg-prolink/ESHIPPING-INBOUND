using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class StatisticManageController : BaseController
    {
        public ActionResult CostStatistics()
        {
            ViewBag.pmsList = GetBtnPms("IST001");
            ViewBag.MenuBar = false;

            ViewBag.ContractSelect = GetBscodeByMode("CART");
            SetOpitonForSelect(new string[] { "CTYP" }, "  ORDER BY Min(ORDER_BY) DESC");
            return View();
        }

        public ActionResult CostQueryData()
        {
            string condition = string.Empty;
            return GetBootstrapData("SMCSI", GetCreateDateCondition("SMCSI", condition));
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            if (resultType == "excel")
                return ExportExcelFile(dt);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        public ActionResult CostSortQueryData()
        {
            string InOut = Prolink.Math.GetValueAsString(Request.Params["InOut"]);
            string Location = Prolink.Math.GetValueAsString(Request.Params["Location"]);
            string Bg = Prolink.Math.GetValueAsString(Request.Params["Bg"]);
            string Payee = Prolink.Math.GetValueAsString(Request.Params["Payee"]);
            string PayeeName = Prolink.Math.GetValueAsString(Request.Params["Payee"]);
            string Year = Prolink.Math.GetValueAsString(Request.Params["Year"]);
            string Month = Prolink.Math.GetValueAsString(Request.Params["Month"]);
            string Sort = Prolink.Math.GetValueAsString(Request.Params["Sort"]);
            string Analysis1 = Prolink.Math.GetValueAsString(Request.Params["Analysis1"]);
            string Analysis2 = Prolink.Math.GetValueAsString(Request.Params["Analysis2"]);
            string groupby = "";
            string dnpgroup = "";
            SetGroup(InOut, "IN_OUT", ref groupby, ref dnpgroup);
            SetGroup(Location, "LOCATION", ref groupby, ref dnpgroup);
            SetGroup(Bg, "BG", ref groupby, ref dnpgroup);
            SetGroup(Payee, "LSP_NO", ref groupby, ref dnpgroup);
            SetGroup(PayeeName, "LSP_NM", ref groupby, ref dnpgroup);
            SetGroup(Year, "CS_YEAR", ref groupby, ref dnpgroup);
            SetGroup(Month, "CS_MONTH", ref groupby, ref dnpgroup);
            SetGroup(Sort, "CHG_TYPE", ref groupby, ref dnpgroup);

            string gapcol = "";
            if (!string.IsNullOrEmpty(Analysis1) && !string.IsNullOrEmpty(Analysis2))
            {
                switch (Analysis1)
                {
                    case "Q": gapcol += ",SUM(QCOST)"; break;
                    case "E": gapcol += ",SUM(ECOST)"; break;
                    case "A": gapcol += ",SUM(ACOST)"; break;
                }
                gapcol += "-";
                switch (Analysis2)
                {
                    case "Q": gapcol += "SUM(QCOST) as GAP"; break;
                    case "E": gapcol += "SUM(ECOST) as GAP"; break;
                    case "A": gapcol += "SUM(ACOST) as GAP"; break;
                }
            }

            string table = "SMCSI";
            string conditon = ModelFactory.GetInquiryCondition(table, "", Request.Params);
            if (string.IsNullOrEmpty(conditon)) conditon = " 1=1";
            string sqlcode = "SELECT * FROM BSCODE WHERE CD_TYPE='CTYP' ORDER BY ORDER_BY DESC";
            DataTable ctypedt = OperationUtils.GetDataTable(sqlcode, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable newdt = new DataTable();
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("CONDITION1", "IN_OUT");
            cols.Add("CONDITION2", "LOCATION");
            cols.Add("CONDITION3", "BG");
            cols.Add("CONDITION4", "LSP_NO");
            cols.Add("CONDITION5", "CS_YEAR");
            cols.Add("CONDITION6", "CS_MONTH");
            cols.Add("CONDITION7", "LSP_NM");

            foreach (var coldic in cols)
            {
                DataColumn col = new DataColumn();
                col.ColumnName = coldic.Key;
                switch (coldic.Key)
                {
                    case "CONDITION1":
                        col.DefaultValue = string.IsNullOrEmpty(InOut) ? "" : InOut;
                        break;
                    case "CONDITION2":
                        col.DefaultValue = string.IsNullOrEmpty(Location) ? "" : Location;
                        break;
                    case "CONDITION3":
                        col.DefaultValue = string.IsNullOrEmpty(Bg) ? "" : Bg;
                        break;
                    case "CONDITION4":
                        col.DefaultValue = string.IsNullOrEmpty(Payee) ? "" : Payee;
                        break;
                    case "CONDITION5":
                        col.DefaultValue = string.IsNullOrEmpty(Year) ? "" : Year;
                        break;
                    case "CONDITION6":
                        col.DefaultValue = string.IsNullOrEmpty(Month) ? "" : Month;
                        break;
                    case "CONDITION7":
                        col.DefaultValue = string.IsNullOrEmpty(PayeeName) ? "" : PayeeName;
                        break;
                }
                newdt.Columns.Add(col);
            }
            DataColumn feucol = new DataColumn();
            feucol.ColumnName = "FEU";
            feucol.DefaultValue = 0;
            newdt.Columns.Add(feucol);
            for (int i = 1; i < 11; i++)
            {
                DataColumn QCostcol = new DataColumn();
                QCostcol.ColumnName = "QCOST" + i.ToString();
                newdt.Columns.Add(QCostcol);
                DataColumn ECostcol = new DataColumn();
                ECostcol.ColumnName = "ECOST" + i.ToString();
                newdt.Columns.Add(ECostcol);
                DataColumn ACostcol = new DataColumn();
                ACostcol.ColumnName = "ACOST" + i.ToString();
                newdt.Columns.Add(ACostcol);
                DataColumn Gapcol = new DataColumn();
                Gapcol.ColumnName = "GAP" + i.ToString();
                newdt.Columns.Add(Gapcol);
            }
            string groupbycondition = "";
            if (!string.IsNullOrEmpty(groupby))
                groupbycondition = "GROUP BY " + groupby;
            if (!string.IsNullOrEmpty(conditon))
            {
                if (!string.IsNullOrEmpty(dnpgroup)) dnpgroup += " AND ";
                dnpgroup += conditon;
            }
            string orderbycondition = "";
            if (!string.IsNullOrEmpty(groupby))
                orderbycondition = "ORDER BY " + groupby;
            string sql = string.Format(@"SELECT {1}SUM(QCOST) as QCOST,SUM(ECOST) as ECOST,SUM(ACOST) as ACOST,SUM(FEU) AS FEU {0},STUFF(
　　　　　　　(SELECT distinct ',' + T.DNP_ID FROM SMCSI T
　　　　　　　　　WHERE {4}
　　　　　　　　　FOR XML PATH('')), 1, 1, '') as DNP_IDS
                FROM SMCSI WHERE {3} {2} {5}", gapcol, !string.IsNullOrEmpty(groupby) ? groupby + "," : groupby,
                groupbycondition, conditon, dnpgroup, orderbycondition);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (Sort == "Y" && !string.IsNullOrEmpty(groupby))
            {
                string[] groupbys = groupby.Split(',');
                DataTable groupdrs = dt.DefaultView?.ToTable(true, groupby.Split(','));
                if (groupdrs != null && groupdrs.Rows.Count > 0)
                {
                    foreach (DataRow groupdr in groupdrs.Rows)
                    {
                        string groupconditon = "";
                        foreach (string group in groupbys)
                        {
                            if ("CHG_TYPE".Equals(group)) continue;
                            if (!string.IsNullOrEmpty(groupconditon)) groupconditon += " AND ";
                            groupconditon += group + "=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(groupdr[group]));
                        }
                        DataRow newdr = newdt.NewRow();
                        foreach (var coldic in cols)
                        {
                            string colkey = coldic.Key;
                            string colValue = coldic.Value;
                            if (dt.Columns.Contains(colValue))
                                newdr[colkey] = groupdr[colValue];
                        }

                        //DataTable chgtypedrs = dt.Select(groupconditon).CopyToDataTable().DefaultView.ToTable(true, "CHG_TYPE");
                        int i = 0;
                        foreach (DataRow chgtypedr in ctypedt.Rows)
                        {
                            i++;
                            if (i > 10) break;
                            string chgtype = Prolink.Math.GetValueAsString(chgtypedr["CD"]);
                            string cddescp = Prolink.Math.GetValueAsString(chgtypedr["CD_DESCP"]);
                            //string[] cddescps = cddescp.Split(';');
                            string tempgroupconditon = groupconditon;
                            if (!string.IsNullOrEmpty(tempgroupconditon)) tempgroupconditon += " AND ";
                            tempgroupconditon += " CHG_TYPE = " + SQLUtils.QuotedStr(chgtype);
                            decimal qcost = GetCsAmt(dt, "QCOST", tempgroupconditon);
                            decimal ecost = GetCsAmt(dt, "ECOST", tempgroupconditon);
                            decimal acost = GetCsAmt(dt, "ACOST", tempgroupconditon);
                            decimal gap = GetCsAmt(dt, "GAP", tempgroupconditon);

                            //DataTable chgtypedrs = dt.Select(groupconditon).CopyToDataTable().DefaultView.ToTable(true, "CHG_TYPE");
                            newdr["QCOST" + i] = qcost;
                            newdr["ECOST" + i] = ecost;
                            newdr["ACOST" + i] = acost;
                            newdr["GAP" + i] = gap;
                        }
                        decimal feu = CalculateFeu(dt, groupconditon);
                        newdr["FEU"] = feu;
                        newdt.Rows.Add(newdr);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(groupby))
            {
                int i = 0;
                //DataRow newdr = newdt.NewRow();

                string[] groupbys = groupby.Split(',');
                DataTable groupdrs = dt.DefaultView?.ToTable(true, groupby.Split(','));
                if (groupdrs != null && groupdrs.Rows.Count > 0)
                {
                    foreach (DataRow groupdr in groupdrs.Rows)
                    {
                        if (i > 10) break;
                        string groupconditon = "";
                        foreach (string group in groupbys)
                        {
                            if (!string.IsNullOrEmpty(groupconditon)) groupconditon += " AND ";
                            groupconditon += "ISNULL(" + group + ",'')=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(groupdr[group]));
                        }
                        DataRow newdr = newdt.NewRow();
                        foreach (var coldic in cols)
                        {
                            string colkey = coldic.Key;
                            string colValue = coldic.Value;
                            if (dt.Columns.Contains(colValue))
                                newdr[colkey] = groupdr[colValue];
                        }
                        decimal qcost = GetCsAmt(dt, "QCOST", groupconditon);
                        decimal ecost = GetCsAmt(dt, "ECOST", groupconditon);
                        decimal acost = GetCsAmt(dt, "ACOST", groupconditon);
                        decimal gap = GetCsAmt(dt, "GAP", groupconditon);

                        decimal feu = CalculateFeu(dt, groupconditon);
                        newdr["FEU"] = feu;
                        newdr["QCOST1"] = qcost;
                        newdr["ECOST1"] = ecost;
                        newdr["ACOST1"] = acost;
                        newdr["GAP1"] = gap;
                        newdt.Rows.Add(newdr);
                    }
                }
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    DataRow newdr = newdt.NewRow();
                    newdr["QCOST1"] = dr["QCOST"];
                    newdr["ECOST1"] = dr["ECOST"];
                    newdr["ACOST1"] = dr["ACOST"];
                    foreach (var coldic in cols)
                    {
                        string colkey = coldic.Key;
                        string colValue = coldic.Value;
                        if (dt.Columns.Contains(colValue))
                            newdr[colkey] = dr[colValue];
                    }
                    decimal feu = CalculateFeu(dt);
                    newdr["FEU"] = feu;
                    newdt.Rows.Add(newdr);
                }
            }
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(newdt);
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = pageSize == 0 ? 0 : recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(newdt)
            };
            return result.ToContent();
        }

        public void SetGroup(string val, string col, ref string groupby, ref string dnpgroup)
        {
            if ("Y".Equals(val))
            {
                if (!string.IsNullOrEmpty(groupby)) groupby += ",";
                groupby += col;
                if (!string.IsNullOrEmpty(dnpgroup)) dnpgroup += " AND ";
                dnpgroup += string.Format(" SMCSI.{0} = T.{0} ", col);
            }
        }

        public decimal GetCsAmt(DataTable dt, string fieldname, string condition)
        {
            return Prolink.Math.GetValueAsDecimal(dt.Compute("Sum(" + fieldname + ")", condition));
        }


        public decimal CalculateFeu(DataTable dt, string groupconditon = "")
        {
            if (!string.IsNullOrEmpty(groupconditon))
            {
                DataRow[] groupdrs = dt.Select(groupconditon);
                if (groupdrs != null && groupdrs.Length > 0)
                {
                    dt = groupdrs.CopyToDataTable();
                }
            }
            if (dt == null && dt.Rows.Count <= 0) return 0;

            string dnid = "";
            foreach (DataRow dr in dt.Rows)
            {
                string dnpid = Prolink.Math.GetValueAsString(dr["DNP_IDS"]);
                if (!string.IsNullOrEmpty(dnid)) dnid += ",";
                dnid += dnpid;
            }
            decimal feu = 0M;
            string[] dnpids = dnid.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (dnpids.Length <= 0) return feu;
            string Sql = string.Format(@"select SMDNP.DN_NO,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT CNT20 FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT20 END) AS CNT20,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT CNT40 FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT40 END) AS CNT40,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT CNT40HQ FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT40 END) AS CNT40HQ,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT CNT_TYPE FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT_TYPE END) AS CNT_TYPE,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT OCNT_TYPE FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT40 END) AS OCNT_TYPE,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT CNT_NUMBER FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT40 END) AS CNT_NUMBER,
                    (CASE WHEN ISCOMBINE_BL='S' THEN (SELECT OCNT_NUMBER FROM SMSM WHERE SHIPMENT_ID=M.Combin_Shipment) ELSE M.CNT40 END) AS OCNT_NUMBER,
                    M.TCBM,
                   (CASE WHEN ISCOMBINE_BL='S' THEN 
                    (SELECT SUM(P.CBM) FROM SMDNP P WHERE (P.NEW_CATEGORY !='TANN' OR P.NEW_CATEGORY IS NULL) and P.DN_NO IN (SELECT DN_NO FROM SMDN A WHERE A.SHIPMENT_ID IN (
                    SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT=M.COMBIN_SHIPMENT)))
                    ELSE (SELECT SUM(P.CBM) FROM SMDNP P WHERE (P.NEW_CATEGORY !='TANN' OR P.NEW_CATEGORY IS NULL) and P.DN_NO IN (SELECT DN_NO FROM SMDN A WHERE A.SHIPMENT_ID IN
                    (select SHIPMENT_ID from SMDN B where B.U_ID=SMDNP.U_FID))) END) AS SM_CBM, 
                    SMDN.COMBINE_OTHER,(SELECT MIN(CBM) FROM SMDNP T WHERE T.DN_NO =SMDNP.DN_NO) MINCBM,SMDNP.CBM,SMDN.AMOUNT1,
                    (CASE WHEN ISCOMBINE_BL='S' THEN 
                    (SELECT SUM(P.VALUE1) FROM SMDNP P WHERE (P.NEW_CATEGORY !='TANN' OR P.NEW_CATEGORY IS NULL) and P.DN_NO IN (SELECT DN_NO FROM SMDN A WHERE A.SHIPMENT_ID IN (
                    SELECT SHIPMENT_ID FROM SMSM WHERE COMBIN_SHIPMENT=M.COMBIN_SHIPMENT)))
                    ELSE (SELECT SUM(P.VALUE1) FROM SMDNP P WHERE (P.NEW_CATEGORY !='TANN' OR P.NEW_CATEGORY IS NULL) 
                    and P.DN_NO IN (SELECT DN_NO FROM SMDN A WHERE A.SHIPMENT_ID IN
                    (select SHIPMENT_ID from SMDN B where B.U_ID=SMDNP.U_FID))) END) AS GVALUE,VALUE1
                    from SMDNP INNER JOIN SMDN ON SMDN.U_ID=SMDNP.U_FID INNER JOIN SMSM M ON SMDN.SHIPMENT_ID=M.SHIPMENT_ID
                    WHERE SMDNP.U_ID IN {0}", SQLUtils.Quoted(dnpids));
            DataTable masterDt = OperationUtils.GetDataTable(Sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < masterDt.Rows.Count; i++)
            {  
                feu += TrackingEDI.Business.CostStatistics.GetHtmlFeu(masterDt.Rows[i], 3, GroupId, CompanyId);
            }

            return feu;
        }

         
    }
}
