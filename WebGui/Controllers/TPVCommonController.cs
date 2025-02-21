using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class TPVCommonController : BaseController
    {
        //
        // GET: /TPVCommon/

        public ActionResult GetGroupData()
        {
            return GetSmptyData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "'");
        }

        public ActionResult GetSiteGroupData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("GROUP_ID AS CD, NAME AS CD_DESCP", "SYS_SITE", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetSiteCmpData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("CMP AS CD, NAME AS CD_DESCP", "SYS_SITE", " TYPE='1'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetSiteCmpData1()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("CMP, NAME", "SYS_SITE", " TYPE='1'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetSmptyData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            condition += " AND STATUS=" + SQLUtils.QuotedStr("U");
            DataTable dt = ModelFactory.InquiryData("*", "SMPTY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetAppDData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "APPROVE_ATTR_D", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public JsonResult GetGroupSelect()
        {
            string gType = Prolink.Math.GetValueAsString(Request.Params["gType"]);
            string returnMessage = "success";
            List<Dictionary<string, object>> returnData = new List<Dictionary<string, object>>();
            string sql = "SELECT GROUP_ID, CMP, STN, DEP, NAME FROM SYS_SITE WHERE TYPE=" + SQLUtils.QuotedStr(gType);

            try
            {
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                returnData = ModelFactory.ToTableJson(dt, "SysSiteModel");
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }

            return Json(new { message = returnMessage, data = returnData });
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

        public ActionResult GetIpartNoForLookup()
        {
            return GetBaseData("SMDNP", "", "*", "", "1");
        }

        public ActionResult GetSmptyDataForLookup()
        {
            return GetBaseData("SMPTY", "", "*", "", "1");
        }

        public ActionResult GetPortData()
        {
            return GetBaseData("BSTPORT", "", "*", "", "1");
        }

        public ActionResult GetAddrData()
        {
            return GetBaseData("BSADDR", "", "*", "", "1");
        }

        public ActionResult GetBscodeDataForLookup()
        {
            return GetBaseData("BSCODE", "", "*", "", "1");
        }

        public ActionResult GetCntyDataForLookup()
        {
            string con = "CD_TYPE = 'CTNY'";
            return GetBaseData("BSCODE", con, "*", "", "1");
        }

        public ActionResult GetItsdArSiteLookup()
        {
            string con = "CD_TYPE='ITSD_SITE'";
            return GetBaseData("BSCODE", con, "*", "", "1");
        }

        public ActionResult GetTCARDataForLookup()
        {
            string con = "CD_TYPE = 'TCAR'";
            return GetBaseData("BSCODE", con, "*", "", "1");
        }

        public ActionResult GetChgDataForLookup()
        {
            return GetBaseData("SMCHG", GetDataPmsCondition("C"), "*", "", "1");
        }

        public ActionResult GetMaterialForLookup()
        {
            //string con = "CD_TYPE = 'TSBM' GROUP BY AP_CD";
            //return GetBaseData("BSCODE", con, "*", "", "1");
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string sql = "SELECT AP_CD FROM BSCODE WHERE CD_TYPE='TSPM' GROUP BY AP_CD";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable dt = ModelFactory.InquiryData("*", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            BootstrapResult result = null;
            result = new BootstrapResult() { rows = ModelFactory.ToTableJson(dt, "BsCodeModel"), total = dt.Rows.Count };

            return result.ToContent();
        }

        public ActionResult GetBsdistforLookup()
        {
            return GetBaseData("BSDIST", GetDataPmsCondition("C"), "*", "", "1");
        }

        public ActionResult GetDepDataForLookup()
        {
            string con = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND (CMP=" + SQLUtils.QuotedStr(CompanyId) + "  OR CMP='*') AND CD_TYPE='DE'";
            return GetBaseData("BSCODE", con, "*", "", "1");
        }
        public ActionResult GetUserData()
        {
            string con = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND IO_FLAG='I'";
            return GetBaseData("SYS_ACCT", con, "*", "", "1");
        }
        public ActionResult GetCountryDataForLookup()
        {
            return GetBaseData("BSCNTY", "", "*", "", "1");
        }
        public ActionResult GetBsCurDataForLookup()
        {
            return GetBaseData("BSCUR", "", "*", "", "1");
        }
        public ActionResult GetBsCityDataForLookup()
        {
            return GetBaseData("BSCITY", "", "*", "", "1");
        }

        public ActionResult GetBstruckcForLookup()
        {
            return GetBaseData("BSTRUCKC", "", "*", "", "1");
        }

        public ActionResult GetBstruckdForLookup()
        {
            return GetBaseData("BSTRUCKD", "", "*", "", "1");
        }

        public ActionResult GetDnForLookup()
        {
            return GetBaseData("SMDN", "", "*", "", "1");
        }

        public ActionResult GetStateDataForLookup()
        {
            return GetBaseData("BSSTATE", "", "*", "", "1");
        }

        public ActionResult GetSmwhForLookup()
        {
            return GetBaseData("SMWH", "", "*", "", "1");
        }

        public ActionResult GetSmwhgtForLookup()
        {
            return GetBaseData("SMWHGT", "", "*", "", "1");
        }

        public ActionResult GetBsaddrForLookup()
        {
            string table = "(SELECT BSADDR.*,SMWH.WS_CD,SMWH.WS_NM FROM BSADDR LEFT JOIN SMWH ON BSADDR.ADDR_CODE=SMWH.DLV_ADDR AND SMWH.CMP=BSADDR.CMP)T";
            return GetBaseData(table, "", "*", "", "1");
        }

        public ActionResult GetBstportDataForLookup()
        {
            return GetBaseData("BSTPORT", GetDataPmsCondition("G"), "*", "", "1");
        }

        public ActionResult GetBsDestDataForLookup()
        {
            return GetBaseData("BSDEST", GetDataPmsCondition("G"), "*", "", "1");
        }
        public ActionResult GetDestAdddrDataForLookup()
        {
            string table = "(SELECT DEST_ADDR.*,WH_CODE AS WS_CD,WH_NAME AS WS_NM FROM DEST_ADDR)T";
            return GetBaseData(table, GetDataPmsCondition("G"), "*", "", "1");
        }

        public ActionResult GetSmqtmForLookup()
        {
            return GetBaseData("SMQTM", GetDataPmsCondition("C") + " AND TRAN_MODE='C' AND QUOT_TYPE='A'", "*", "", "1");
        }

        public ActionResult GetSmsmiForLookup()
        {
            string con = "";
            if (IOFlag == "O")
            {
                con = " SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMSMIPT WHERE PARTY_NO={0})";
                con = string.Format(con, SQLUtils.QuotedStr(CompanyId));
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId);
            }

            return GetBaseData("SMSMI", con, "*", "", "1");
        }

        public ActionResult GetSmsmForLookup()
        {
            string con = "";
            if (IOFlag == "O")
            {
                con = " SHIPMENT_ID IN (SELECT DISTINCT SHIPMENT_ID FROM SMSMPT WHERE PARTY_NO={0})";
                con = string.Format(con, SQLUtils.QuotedStr(CompanyId));
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId);
            }

            return GetBaseData("SMSM", con, "*", "", "1");
        }

        #region  开立账单时费用筛选
        public ActionResult SmbidQueryData()
        {
            string con = "";
            string conditions = Request.Params["conditions"];
            string baseCondition = Request.Params["baseCondition"];
            string page_str = Request.Params["page"];//第几页  从1开始
            string limit_str = Request.Params["rows"];//每页大小
            int page = 1, limit = 20;
            if (!int.TryParse(page_str, out page)) page = 0;
            if (!int.TryParse(limit_str, out limit)) limit = 20;
            string condition = "";
            string subCondition = "";
            if (!string.IsNullOrEmpty(conditions))
            {
                conditions = HttpUtility.UrlDecode(conditions);
                string[] cs = conditions.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string c in cs)
                {
                    if (c.Contains("CntrInfo"))
                    {
                        if (string.IsNullOrEmpty(subCondition))
                        {
                            subCondition = c;
                            continue;
                        }
                        subCondition += "&" + c;
                        continue;
                    }
                    if (string.IsNullOrEmpty(condition))
                    {
                        condition = c;
                        continue;
                    }
                    condition += "&" + c;
                }
                subCondition = subCondition.Replace("CntrInfo", "CntrNo");
                condition = ModelFactory.ConvParam2Condition(condition, "SMBID");
                subCondition = ModelFactory.ConvParam2Condition(subCondition, "SMBID");
            }
            baseCondition = HttpUtility.UrlDecode(baseCondition);
            baseCondition = Prolink.Utils.SerializeUtils.DecodeBase64ToString(baseCondition);
            if (!string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(baseCondition))
            {
                condition += " AND " + baseCondition;
            }
            else if (string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(baseCondition))
            {
                condition = baseCondition;
            }
            if (IOFlag == "O")
            {
                con = " LSP_NO=" + SQLUtils.QuotedStr(CompanyId) + " AND U_FID IS NULL AND BAMT!=0 AND APPROVE_STATUS='Y'";
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND U_FID IS NULL AND BAMT!=0 AND APPROVE_STATUS='Y'";
            }
            string subSql = "";
            if (subCondition != "")
            {
                subSql = " AND SHIPMENT_ID IN ( SELECT SHIPMENT_ID FROM SMICNTR WHERE 1=1 AND " + subCondition + " )";
            }
            string allCondition = con;
            if (!string.IsNullOrEmpty(condition))
                allCondition += " AND " + condition;
            allCondition += subSql;
            return GetBaseData("SMBID", allCondition, "*", getOrderByRequest(), "2", page, limit);
        }

        public string getOrderByRequest(string orderBy = "")
        {
            string sidx = Request.Params["sidx"];
            string sord = Request.Params["sord"];
            if (!string.IsNullOrEmpty(sidx))
            {
                //orderBy = sidx;
                string[] fs = sidx.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string f in fs)
                {
                    if (orderBy.Length > 0) orderBy += ",";
                    orderBy += ModelFactory.ReplaceFiledToDBName(f);
                }
                orderBy += string.IsNullOrEmpty(sord) ? " ASC" : " " + sord.ToUpper();
            }
            return orderBy;
        }

        public ActionResult SmbidQueryData1()
        {
            string con = "";
            if (IOFlag == "O")
            {
                con = " LSP_NO=" + SQLUtils.QuotedStr(CompanyId) + " AND U_FID IS NULL AND BAMT!=0";
            }
            else
            {
                con = " CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND U_FID IS NULL AND BAMT!=0";
            }

            return GetBaseData("SMBID", con, "*", "", "1");
        }

        public ActionResult GetBslcpolForLookup()
        {
            return GetBaseData("BSLCPOL", " CMP=" + SQLUtils.QuotedStr(CompanyId), "*", "", "1");
        }

        public ActionResult GetSmbidByid()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string LspNo = Prolink.Math.GetValueAsString(Request.Params["LspNo"]);
            string Cur = Prolink.Math.GetValueAsString(Request.Params["Cur"]);
            //string sql = string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND CUR={2} AND U_FID IS NULL AND BAMT!=0 AND APPROVE_STATUS='Y'", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Cur));
            string sql = string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND U_FID IS NULL AND BAMT!=0 AND APPROVE_STATUS='Y'", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));

            string DebitTo = Prolink.Math.GetValueAsString(Request.Params["DebitTo"]);
            if (!string.IsNullOrEmpty(DebitTo))
                sql += string.Format(" AND DEBIT_TO={0}", SQLUtils.QuotedStr(DebitTo));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
            return ToContent(data);
        }

        public ActionResult GetSmbidByid1()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string LspNo = Prolink.Math.GetValueAsString(Request.Params["LspNo"]);
            string Cur = Prolink.Math.GetValueAsString(Request.Params["Cur"]);
            //string sql = string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND CUR={2} AND U_FID IS NULL AND BAMT!=0 AND APPROVE_STATUS='Y'", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Cur));
            string sql = string.Format("SELECT * FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND CUR={2} AND U_FID IS NULL AND BAMT!=0", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Cur));

            string DebitTo = Prolink.Math.GetValueAsString(Request.Params["DebitTo"]);
            if (!string.IsNullOrEmpty(DebitTo))
                sql += string.Format(" AND DEBIT_TO={0}", SQLUtils.QuotedStr(DebitTo));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmbidModel");
            return ToContent(data);
        }

        #endregion

        public ActionResult GetSmsmByid()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);

            return GetBaseData("SMSM", " SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId), "*", "", "1");
        }

        public ActionResult GetSmsmiByid()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);

            return GetBaseData("SMSMI", " SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId), "*", "", "1");
        }


        public ActionResult GetSmrvDataForLookup()
        {
            string sql = "SELECT TOP 1 IO_FLAG FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId);
            string IoFlag = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string condition = "";

            if (IoFlag == "O")
            {
                condition = " TRUCKER=" + SQLUtils.QuotedStr(CompanyId);
            }
            return GetBaseData("SMRV", condition, "*", "", "1");
        }

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "", int pageIndex = 1, int pageSize = 20)
        {
            int recordsCount = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref recordsCount);
                    break;
                case "3":
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                default:
                    dt = ModelFactory.InquiryData("*", table, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
            }

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

        public ActionResult GetBscodeDataForLookupPri()
        {

            int recordsCount = 0, pageIndex = 1, pageSize = 20;
            string[] edocViewPri = GetBtnPms("EDOC").Split('|');
            string condition = "";
            for (int i = 0; i < edocViewPri.Length; i++)
            {
                if (edocViewPri[i].IndexOf("EDOC_EDT_V_") > -1)
                {
                    condition += SQLUtils.QuotedStr(edocViewPri[i].Replace("EDOC_EDT_V_", "")) + ',';
                }
            }

            if (condition != "")
            {
                condition = " CD IN(" + condition + "'0')";
            }
            string resultType = Request.Params["resultType"];
            DataTable dt = ModelFactory.InquiryData("*", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);


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
        public ActionResult ToMyContent(object obj)
        {
            JavaScriptSerializer jsSeri = new JavaScriptSerializer();
            jsSeri.MaxJsonLength = 1024 * 1024 * 50;
            string str = jsSeri.Serialize(obj);
            ContentResult result = new ContentResult();
            result.Content = str;
            return result;
        }

        public ActionResult GetWarehouseTypeDataByCmp()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = ModelFactory.InquiryData("WS_CD,WS_NM,CMP", "SMWH", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetWarehouseTypeData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = string.Format("GROUP_ID={0}", SQLUtils.QuotedStr(GroupId));
            DataTable dt = ModelFactory.InquiryData("WS_CD,WS_NM,CMP", "SMWH", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
    }
}

public class SiteOptions
{
    public List<SiteOptionsItem> rows = new List<SiteOptionsItem>();
}

public class SiteOptionsItem
{
    public string Cd { get; set; }
    public string CdDescp { get; set; }
}
