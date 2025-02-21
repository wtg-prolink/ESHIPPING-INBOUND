using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using Prolink.Model;
using System.Text;
using System.Collections.Specialized;
using Business;
using Business.Mail;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using EDOCApi;

namespace WebGui.Controllers
{
    public class IRQManageController : BaseController
    {
        //
        // GET: /RQManage/

        #region View
        public ActionResult RQQuery()
        {
            ChangeQuotType();
            SetTranModeSelect();
            ViewBag.MenuBar = false;
            return View();
        }

        public static void ChangeQuotType()
        {
            string id = string.Empty;
            //SELECT * FROM SMRQM WHERE STATUS='B' AND RFQ_TO>'2016-1-20'
            string nowstr = DateTime.Now.ToString("yyyy-MM-dd");
            string sql = string.Format("SELECT * FROM SMRQM WHERE STATUS='B' AND RFQ_TO<CONVERT(date,{0},120)", SQLUtils.QuotedStr(nowstr));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei = null;
            MixedList ml = new MixedList();

            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["RFQ_TO"] == null || dr["RFQ_TO"] == DBNull.Value)
                    continue;
                DateTime rfq_to = (DateTime)dr["RFQ_TO"];
                rfq_to = new DateTime(rfq_to.Year, rfq_to.Month, rfq_to.Day, 0, 0, 0);

                id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                ei = new EditInstruct("SMRQM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", id);
                ei.PutKey("STATUS", "B");
                ei.Put("STATUS", "C");

                if (now.CompareTo(rfq_to) > 0)
                    ml.Add(ei);
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public ActionResult RQSetup(string id = null, string uid = null)
        {
            ViewBag.pmsList = GetBtnPms("IRQ010");
            SetSchema("RQSetup");
            if (uid == null)
            {
                uid = id;
            }
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult SFCLQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetLspCd();
            return View();
        }

        public ActionResult SLCLQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetLspCd();
            return View();
        }

        public ActionResult AirQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetLspCd();
            return View();
        }

        public ActionResult IEQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetLspCd();
            return View();        
        }

        public ActionResult DEQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetLspCd();
            return View();
        }

        public ActionResult DTQuery(string id = null)
        {
            ViewBag.MenuBar = false;
            if (string.IsNullOrEmpty(id))
                id = Request["RfqNo"];
            ViewBag.Uid = id;
            SetTranTypeSelect();
            SetLspCd();
            return View();
        }

        private void SetLspCd()
        {
            try
            {
                string lc = Request["lc"];//通知人数组
                ViewBag.LSP = "";
                if (!string.IsNullOrEmpty(lc))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    List<string> lcs = js.Deserialize<List<string>>(lc);
                    ViewBag.LSP = string.Join(";", lcs);
                }
            }
            catch { }
        }
        #endregion

        #region 基础方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        /// <summary>
        /// 设置Schema
        /// </summary>
        /// <param name="name"></param>
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "RQSetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM SMRQM WHERE 1=0";
                        break;

                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        private ActionResult GetBootstrapData(string table, string condition, string orderBy = "", string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                string statusField = Request.Params["statusField"];
                dt = GetStatusCountData(statusField, table, condition, Request.Params);
                pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, orderBy);
                //dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                if (resultType == "excel")
                    return ExportExcelFile(dt);
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

        public DataTable GetStatusCountData(string statusField, string defaultTable, string baseCondition, NameValueCollection nameValues)
        {
            string col = ModelFactory.ReplaceFiledToDBName(statusField);
            string condition = ModelFactory.GetInquiryCondition(defaultTable, baseCondition, nameValues);

            string sql = " SELECT " + col + " AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition + " GROUP BY " + col + " ORDER BY " + col + " ASC ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "  SELECT '' AS PO_STATUS,COUNT(1) AS COUNT FROM " + defaultTable + " WHERE " + condition;
            DataTable dtsum = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dtAll = dt.Copy();
            foreach (DataRow dr in dtsum.Rows)
            {
                dtAll.ImportRow(dr);
            }

            return dtAll;
        }

        private void SetTranTypeSelect()
        {
            ViewBag.SelectTranType = "";
            ViewBag.DefaultTranType = "";

            #region Tran Type
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TDTK' AND GROUP_ID={0} AND (CMP={1} OR CMP ='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            //string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TTRN'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            //A:有色采购;B:有色销售;C:冷链;D:日化;E:空白
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultTranType = Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]).Trim();
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"]).Trim() + "." + Prolink.Math.GetValueAsString(dr["CD_DESCP"]).Trim();
            }
            ViewBag.SelectTranType = select;
            #endregion
        }
        #endregion

        #region 询价操作
        /// <summary>
        /// 询价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult RQManageQueryData()
        {
            return GetBootstrapData("SMRQM", GetRQCondition());
        }

        public string GetRQCondition()
        {
            switch (UPri)
            {
                case "G":
                    return GetBaseGroup();
                case "C":
                    return GetBaseCmp();
                //case "D":
                //return string.Format("GROUP_ID={0} AND CMP={1} AND CREATE_BY={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UserId));
                //case "U":
                default:
                    return string.Format("GROUP_ID={0} AND CMP={1} AND CREATE_BY={2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UserId));
            }
        }

        /// <summary>
        /// 询价保存
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveRQSetupData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            string rfq_no = Request["rfq_no"];
            string dn_no = string.Empty;
            string u_id = Request["u_id"];
            if (!string.IsNullOrEmpty(u_id))
                u_id = HttpUtility.UrlDecode(u_id);
            if (!string.IsNullOrEmpty(rfq_no))
                rfq_no = HttpUtility.UrlDecode(rfq_no);

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmrqmModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        rfq_no = ei.Get("RFQ_NO");
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id= System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.Put("CREATE_DEP", Dep);
                            ei.Put("CREATE_EXT", Ext);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                            //数据验证
                            sql = string.Format("SELECT * FROM SMRQM WHERE RFQ_NO={0} "
                              , SQLUtils.QuotedStr(ei.Get("RFQ_NO")));

                            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt.Rows.Count > 0)
                                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_391 });
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);

                            #region 同步数据到报价
                            if (!string.IsNullOrEmpty(rfq_no))
                            {
                                string[] fileds = ei.getNameSet();
                                List<string> ufileds = new List<string> { "TRAN_MODE", "RLOCATION", "INCOTERM", "LOADING_FROM", "LOADING_TO", "CUR", "INCOTERM_DESCP" };
                                List<string> dfileds = new List<string> { "RFQ_FROM", "RFQ_TO", "EFFECT_FROM", "EFFECT_TO", "RFQ_DATE" };
                                EditInstruct uei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
                                uei.PutKey("RFQ_NO", rfq_no);
                                uei.PutKey("GROUP_ID", GroupId);
                                uei.PutKey("CMP", CompanyId);
                                bool up = false;
                                foreach (var filed in fileds)
                                {
                                    if (ufileds.Contains(filed))
                                    {
                                        up = true;
                                        uei.Put(filed, ei.Get(filed));
                                    }
                                    if (dfileds.Contains(filed))
                                    {
                                        up = true;
                                        uei.PutDate(filed, ei.Get(filed));
                                    }
                                }
                                if(up)
                                    mixList.Add(uei);
                            }
                            #endregion
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei = new EditInstruct("SMRQD", EditInstruct.DELETE_OPERATION);
                            //ei.PutKey("RFQ_NO", rfq_no);
                            ei.PutKey("U_FID", u_id);
                            mixList.Add(ei);

                            ei = new EditInstruct("SMQTD", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("RFQ_NO", rfq_no);
                            mixList.Add(ei);

                            ei = new EditInstruct("SMQTM", EditInstruct.DELETE_OPERATION);
                            ei.PutKey("RFQ_NO", rfq_no);
                            mixList.Add(ei);
                        }
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmrqdModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("RFQ_NO", rfq_no);
                            ei.Put("U_FID", u_id);
                        }
                        else
                            ei.AddKey("U_ID");
                        //string test_id = ei.Get("U_ID");
                        System.Guid test_id = System.Guid.NewGuid();
                        if (!System.Guid.TryParse(ei.Get("U_ID"), out test_id))
                            continue;
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }

            sql = string.Format("SELECT * FROM SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetTableEdit(mainDt);
            sql = string.Format("SELECT * FROM SMRQD WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmrqmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmrqdModel");
            return ToContent(data);
        }

        public ActionResult GetRQSetupData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMRQM", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        /// <summary>
        /// 获取单笔询价数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRQSetupDataItem()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetTableEdit(mainDt);
            sql = string.Format("SELECT * FROM SMRQD WHERE U_FID={0}", SQLUtils.QuotedStr(u_id));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "SmrqmModel");
            data["sub"] = ModelFactory.ToTableJson(subDt, "SmrqdModel");
            return ToContent(data);
        }
        #endregion

        #region 比价数据查询
        /// <summary>
        /// 海运整柜比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult SFCLQueryData()
        {
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "REGION,POD_CD,POL_CD,CUR,LOADING_FROM,LOADING_TO,L3");
        }

        /// <summary>
        /// 海运散货比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult SLCLQueryData()
        {
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "POD_CD,POL_CD,CHG_CD,LOADING_FROM,LOADING_TO,L1");
        }

        /// <summary>
        /// 空运比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult AirQueryData()
        {
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "POD_CD,POL_CD,CHG_CD,LOADING_FROM,LOADING_TO,L1");
        }

        /// <summary>
        /// 国际快递比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult IEQueryData()
        {
            //return GetBootstrapData("SMQTM", "1=1", "CMP,REGION,CHG_CD,L11");
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "REGION,LOADING_FROM,LOADING_TO,L1");
        }

        /// <summary>
        /// 国内快递比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult DEQueryData()
        {
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "POD_CD,POL_CD,CHG_CD,LOADING_FROM,LOADING_TO,L1");
            //return GetBootstrapData("SMQTD", "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL)", "POD_CD,POL_CD,CHG_CD,LOADING_FROM,LOADING_TO,L1");
        }

        /// <summary>
        /// 国内运输比价查询
        /// </summary>
        /// <returns></returns>
        public ActionResult DTQueryData()
        {
            string table = "(SELECT * FROM SMQTD S WITH (NOLOCK) OUTER APPLY (SELECT TOP 1 M.LSP_NM  FROM SMQTM M WITH (NOLOCK) WHERE M.RFQ_NO = S.RFQ_NO AND M.LSP_CD=S.LSP_CD) B) MD";
            return GetBootstrapData(table, "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL) AND CHG_CD='FRT'", "POD_CD,POL_CD,STATE,CHG_CD,LOADING_FROM,LOADING_TO,L1");
            //return GetBootstrapData("SMQTD", "1=1 AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL) AND CHG_CD='FRT'", "POD_CD,POL_CD,STATE,CHG_CD,LOADING_FROM,LOADING_TO,L1");
        }
        #endregion

        #region 业务流程
        /// <summary>
        /// 验证是否一样上传电子文档
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckNoEdoc()
        {
            string jobno = Request["JobNo"];
            //string sql = string.Format("SELECT COUNT(1) FROM  EDOC2_FOLDER WHERE JOB_NO={0} ", SQLUtils.QuotedStr(jobno));
            //int rows = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            //if (rows <= 0)
            //{
            //    return Json(new { message = true });
            //}
            List<Dictionary<string, string>> list = MailTemplate.FileQueryDownlodInfo(jobno, GroupId, CompanyId, "*", "", "RFQ");
            if (list.Count <= 0)
            {
                return Json(new { message = true });
            }
            return Json(new { message = false });
        }

        /// <summary>
        /// 发送询价通知
        /// </summary>
        /// <returns></returns>
        public ActionResult SendNotifi()
        {
            string lc = Request["LC"];//通知人数组
            string lang = Request["lang"];//多语言判断
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<string> lcs = js.Deserialize<List<string>>(lc);

            string rfqno = Prolink.Math.GetValueAsString(Request.Params["rfqno"]);
            bool flag = false;
            string sql = string.Format("SELECT * FROM  SMRQD WHERE RFQ_NO={0} ", SQLUtils.QuotedStr(rfqno));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM  SMRQM WHERE RFQ_NO={0} ", SQLUtils.QuotedStr(rfqno));
            DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
          
            string jobno = Prolink.Math.GetValueAsString(MainDt.Rows[0]["U_ID"]);
            string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);

            DateTime now_date = DateTime.Now;
            now_date = new DateTime(now_date.Year, now_date.Month, now_date.Day, 0, 0, 1);
            MixedList mixedlist = new MixedList();
            EditInstruct ei = null;

            List<string> lspcd_list1 = new List<string>();
            for (int i = 0; i < subDt.Rows.Count; i++)
            {
                DataRow dr = subDt.Rows[i];
                string u_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);
                string notify_group = Prolink.Math.GetValueAsString(dr["NOTIFY_GROUP"]);
                string lsp_cd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                if (lspcd_list1.Contains(lsp_cd))
                    continue;
                lspcd_list1.Add(lsp_cd);

                if (!lcs.Contains(lsp_cd))
                {
                    continue;
                }


                EvenFactory.AddEven(u_fid + "#" + u_id + "#" + notify_group + "#" + lsp_cd + "#" + group_id + "#" + DateTime.Now.Ticks, u_fid, MailManager.QuotNotify, mixedlist, 1, 0, "", rfqno + @Resources.Locale.L_RQManage_Controllers_438, string.Empty);
            }

            sql = string.Format("SELECT U_ID,LSP_CD FROM SMQTM WHERE RFQ_NO={0}", SQLUtils.QuotedStr(rfqno));
            DataTable checkDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            for (int i = 0; i < MainDt.Rows.Count; i++)
            {
                DataRow dr = MainDt.Rows[i];
                #region 更新询价状态
                ei = new EditInstruct("SMRQM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("RFQ_NO", Prolink.Math.GetValueAsString(dr["RFQ_NO"]));
                ei.PutKey("U_ID", Prolink.Math.GetValueAsString(dr["U_ID"]));
                ei.Put("STATUS", "B");
                mixedlist.Add(ei);
                #endregion

                List<string> lspcd_list = new List<string>();
                foreach(DataRow smqtd in subDt.Rows)
                {
                    string smqtm_id = string.Empty;
                    #region 过滤相同物流业者
                    string lsp_cd = Prolink.Math.GetValueAsString(smqtd["LSP_CD"]);
                    if (lspcd_list.Contains(lsp_cd))
                        continue;
                    lspcd_list.Add(lsp_cd);
                    #endregion

                    ei = new EditInstruct("SMQTM", EditInstruct.INSERT_OPERATION);
                    DataRow[] drs = checkDt.Select(string.Format("LSP_CD={0}", SQLUtils.QuotedStr(lsp_cd)));
                    if (drs.Length > 0)
                    {
                        smqtm_id = Prolink.Math.GetValueAsString(drs[0]["U_ID"]);
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        ei.PutKey("U_ID", smqtm_id);
                    }
                    else
                    {
                        smqtm_id = Guid.NewGuid().ToString();
                        ei.Put("U_ID", smqtm_id);
                    }
                    ei.Put("PERIOD",  Prolink.Math.GetValueAsString(dr["PERIOD"]));
                    ei.Put("LSP_CD", lsp_cd);
                    ei.Put("LSP_NM", Prolink.Math.GetValueAsString(smqtd["LSP_NM"]));
                    ei.Put("RFQ_NO", Prolink.Math.GetValueAsString(dr["RFQ_NO"]));
                    ei.Put("BUSINESS_GROUP", Prolink.Math.GetValueAsString(dr["BUSINESS_GROUP"]));
                    try
                    {
                        ei.Put("QUOT_NO", AutoNo.GetNo("QUOT_NO", new Hashtable(), GroupId, "*", "*"));//LSPQYYMM9999
                    }
                    catch (Exception ex1)
                    {
                        return Json(new { message = @Resources.Locale.L_RQManageController_Controllers_204 + ex1.Message });
                    }

                    //ei.PutDate("QUOT_DATE", DateTime.Now);
                    ei.PutDate("RFQ_FROM", Prolink.Math.GetValueAsString(dr["RFQ_FROM"]));
                    ei.PutDate("RFQ_TO", Prolink.Math.GetValueAsString(dr["RFQ_TO"]));
                    ei.PutDate("RFQ_DATE", Prolink.Math.GetValueAsString(dr["RFQ_DATE"]));
                    ei.PutDate("EFFECT_FROM", Prolink.Math.GetValueAsString(dr["EFFECT_FROM"]));
                    ei.PutDate("EFFECT_TO", Prolink.Math.GetValueAsString(dr["EFFECT_TO"]));
                    ei.Put("TRAN_MODE", Prolink.Math.GetValueAsString(dr["TRAN_MODE"]));
                    string outin = Prolink.Math.GetValueAsString(dr["OUT_IN"]);
                    if (string.IsNullOrEmpty(outin))
                    {
                        outin = "O";
                    }
                    ei.Put("OUT_IN", outin);
                    ei.Put("RLOCATION", Prolink.Math.GetValueAsString(dr["RLOCATION"]));
                    ei.Put("QUOT_TYPE", "Q");
                    ei.Put("INCOTERM", Prolink.Math.GetValueAsString(dr["INCOTERM"]));
                    ei.Put("FREIGHT_TERM", Prolink.Math.GetValueAsString(dr["FREIGHT_TERM"]));
                    ei.Put("SERVICE_MODE", Prolink.Math.GetValueAsString(dr["SERVICE_MODE"]));
                    ei.Put("LOADING_FROM", Prolink.Math.GetValueAsString(dr["LOADING_FROM"]));
                    ei.Put("LOADING_TO", Prolink.Math.GetValueAsString(dr["LOADING_TO"]));
                    ei.Put("CUR", Prolink.Math.GetValueAsString(dr["CUR"]));
                    ei.Put("POL_CD", Prolink.Math.GetValueAsString(dr["POL_CD"]));
                    ei.Put("POL_NM", Prolink.Math.GetValueAsString(dr["POL_NM"]));
                    ei.Put("POD_CD", Prolink.Math.GetValueAsString(dr["POD_CD"]));
                    ei.Put("POD_NM", Prolink.Math.GetValueAsString(dr["POD_NM"]));
                    ei.Put("REMARK", Prolink.Math.GetValueAsString(dr["REMARK"]));
                    ei.Put("CREATE_BY", UserId);
                    ei.Put("CREATE_DEP", Dep);
                    DateTime odt = DateTime.Now;                  
                    DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                    
                    ei.PutDate("CREATE_DATE", odt);
                    ei.PutDate("CREATE_DATE_L", ndt);
                    ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(dr["GROUP_ID"]));
                    ei.Put("CMP", Prolink.Math.GetValueAsString(dr["CMP"]));
                    //ei.Put("MODIFY_BY", UserId);
                    //ei.PutDate("MODIFY_DATE", DateTime.Now);
                    mixedlist.Add(ei);
                }
            }

            if (mixedlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    flag = true;
                }
                catch (Exception ex)
                {
                    return Json(new { message = ex.Message });
                }
            }

            //for (int i = 0; i < subDt.Rows.Count; i++)
            //{
            //    DataRow dr = subDt.Rows[i];
            //    foreach (DataRow mrow in mailDt.Rows)
            //    {
            //        mrow["LSP_CD"] = dr["LSP_CD"];
            //        mrow["LSP_NM"] = dr["LSP_NM"];
            //    }
            //    string u_id = Prolink.Math.GetValueAsString(dr["U_FID"]);
            //    string notify_group = Prolink.Math.GetValueAsString(dr["NOTIFY_GROUP"]);

            //    MailInfo mi = new MailInfo();
            //    mi.CC = "";
            //    mi.Subject = rfqno + "询价通知";
            //    mi.Body = parse.Parse(mailDt, null, htmltemp, map); ;
            //    mi.BodyFormat = "HTML";
            //    DataTable mailGroupDt = TrackingEDI.Mail.MailTemplate.GetMailGroup(notify_group, group_id);
            //    foreach (DataRow mailGroup in mailGroupDt.Rows)
            //    {
            //        string to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
            //        if (string.IsNullOrEmpty(to))
            //            continue;
            //        mi.To = to;
            //        MailServices.GetInstance().SendMail(mi);
            //    }
            //}
            return Json(new { message = @Resources.Locale.L_RQManageController_Controllers_205, flag = flag });
        }

      

        /// <summary>
        /// 中标通知
        /// </summary>
        /// <returns></returns>
        public ActionResult SendWinNotifiy()
        {
            return SendQuotWinNotify();
        }

        /// <summary>
        /// 得标通知
        /// </summary>
        /// <returns></returns>
        public ActionResult SendFinalNotifiy()
        {
            Insert2Smctm();
            return SendQuotWinNotify("F");
        }

        public ActionResult SendLostNotifiy()
        {
            return SendQuotWinNotify("N");
        }

        public void Insert2Smctm()
        {
            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string lc = Request["LC"];//通知人数组
            string sql = string.Empty;
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<string> lcs = js.Deserialize<List<string>>(lc);
            EditInstruct ei = null;
            MixedList ml = new MixedList();

            //sql = "SELECT D.LSP_CD,D.LSP_NM,M.* FROM SMRQM M, SMRQD D WHERE M.RFQ_NO=D.RFQ_NO AND D.LSP_CD={0} AND M.U_ID={1}";
            for (int i = 0; i < lcs.Count; i++ )
            {
                sql = "SELECT D.LSP_CD,D.LSP_NM,M.* FROM SMRQM M, SMRQD D WHERE M.RFQ_NO=D.RFQ_NO AND D.LSP_CD={0} AND M.U_ID={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(lcs[i]), SQLUtils.QuotedStr(uid));
                DataTable dt = getDataTableFromSql(sql);

                if(dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        ei = new EditInstruct("SMCTM", EditInstruct.INSERT_OPERATION);
                        string cuid = System.Guid.NewGuid().ToString();
                        string JobNo = Business.ReserveManage.getAutoNo("CT_NO", GroupId, CompanyId);
                        string RfqNo = Prolink.Math.GetValueAsString(item["RFQ_NO"]);
                        string LspCd = Prolink.Math.GetValueAsString(item["LSP_CD"]);
                        ei.Put("U_ID", cuid);
                        ei.Put("JOB_NO", JobNo);
                        ei.Put("RFQ_NO", RfqNo);
                        sql = "SELECT QUOT_NO FROM SMQTM WHERE RFQ_NO={0} AND LSP_CD={1}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(RfqNo), SQLUtils.QuotedStr(LspCd));
                        string QuotNo = getOneValueAsStringFromSql(sql);
                        ei.Put("QUOT_NO", QuotNo);
                        ei.Put("REGION", Prolink.Math.GetValueAsString(item["REGION"]));
                        ei.Put("LSP_NO", LspCd);
                        ei.Put("LSP_NM", Prolink.Math.GetValueAsString(item["LSP_NM"]));
                        ei.Put("LOCATION", Prolink.Math.GetValueAsString(item["RLOCATION"]));
                        ei.Put("LOC_NAME", Prolink.Math.GetValueAsString(item["RLOCATION_NM"]));
                        ei.Put("STATUS", "N");
                        ei.Put("CREATE_DEP", Dep);
                        ei.Put("CREATE_EXT", Ext);
                        ei.Put("CREATE_BY", UserId);
                        ei.PutDate("CREATE_DATE", DateTime.Now);
                        ei.Put("GROUP_ID", GroupId);
                        ei.Put("CMP", CompanyId);
                        ei.Put("STN", Station);
                        ei.Put("DEP", Dep);

                        ml.Add(ei);
                    }
                }
            }

            if(ml.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch(Exception ex)
                {

                }
            }
            return;
        }

        /// <summary>
        /// 报价得标中标通知
        /// </summary>
        /// <param name="defaultBidder"></param>
        /// <returns></returns>
        private ActionResult SendQuotWinNotify(string defaultBidder = "B")
        {
            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string lc = Request["LC"];//通知人数组
            string lang = Request["lang"];//多语言判断
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<string> lcs = js.Deserialize<List<string>>(lc);
            bool flag = true;
            string sql = string.Format("SELECT * FROM  SMRQD WHERE U_FID={0} ", SQLUtils.QuotedStr(uid));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string mailType = "FQT";//mail模板类型
            sql = string.Format("SELECT * FROM  SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable MainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string rfq_no = Prolink.Math.GetValueAsString(MainDt.Rows[0]["RFQ_NO"]);
            string group_id = Prolink.Math.GetValueAsString(MainDt.Rows[0]["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(MainDt.Rows[0]["CMP"]);
            string status = Prolink.Math.GetValueAsString(MainDt.Rows[0]["STATUS"]);
            //测试需要暂时不开启
            //if ("A".Equals(status) || "B".Equals(status))
            //    return Json(new { message = "询价尚未结束", flag = false });
            if ("D".Equals(status))
                return Json(new { message = @Resources.Locale.L_QTSetup_HasClosed, flag = false });

            DataTable mailDt = MainDt.Copy();
            MailTemplate.CreateQuotData(mailDt, group_id,cmp);

            IMailTemplateParse parse = new DefaultMailParse();
            EditInstruct ei = null;
            MixedList ml = new MixedList();
            Dictionary<string, string> map = new Dictionary<string, string>();
            Dictionary<string, string> mailTemp = new Dictionary<string, string>();

            EditInstruct m_ei = null;

            DataTable smqtm = OperationUtils.GetDataTable(string.Format("SELECT * FROM  SMQTM WHERE RFQ_NO={0}", SQLUtils.QuotedStr(rfq_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            StringBuilder msg = new StringBuilder();
            List<string> voidList = new List<string>();
            foreach (DataRow lspDr in subDt.Rows)
            {
                ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
                string lsp_cd = Prolink.Math.GetValueAsString(lspDr["LSP_CD"]);
                string old_bidder = Prolink.Math.GetValueAsString(lspDr["BIDDER"]);
                string notify_group = Prolink.Math.GetValueAsString(lspDr["NOTIFY_GROUP"]);
                ei.PutKey("U_ID", lspDr["U_ID"]);
                string bidder = defaultBidder;
                if (!lcs.Contains(lsp_cd))
                {
                    continue;
                }

                if ("F".Equals(old_bidder))
                    continue;

                ei.Put("BIDDER", bidder);
                DataRow[] drs = smqtm.Select(string.Format("RFQ_NO={0} AND LSP_CD={1}", SQLUtils.QuotedStr(rfq_no), SQLUtils.QuotedStr(lsp_cd)));
                if (drs.Length > 0)
                {
                    //已作废
                    if ("V".Equals(Prolink.Math.GetValueAsString(drs[0]["QUOT_TYPE"])))
                    {
                        msg.Append(string.Format(@Resources.Locale.L_RQManageController_Controllers_206, lsp_cd));
                        voidList.Add(lsp_cd);
                        continue;
                    }
                    ml.Add(ei);//添加更询价细档状态的部分

                    if (m_ei == null && "F".Equals(defaultBidder))
                    {
                        m_ei = new EditInstruct("SMRQM", EditInstruct.UPDATE_OPERATION);
                        m_ei.PutKey("U_ID", uid);
                        m_ei.Put("STATUS", "D");
                        ml.Add(m_ei);
                    }
                    string quot_type = bidder;
                    if ("N".Equals(bidder))
                    {
                        if (drs[0]["QUOT_DATE"] == null || drs[0]["QUOT_DATE"] == DBNull.Value)
                            quot_type = "N";
                        else
                            quot_type = "Q";
                    }
                    ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", drs[0]["U_ID"]);
                    ei.Put("QUOT_TYPE", quot_type);//Q:報價/N:投標/B:中標/F:得標/V:作廢
                    ml.Add(ei);

                    ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_FID", drs[0]["U_ID"]);
                    ei.Put("QUOT_TYPE", quot_type);
                    ml.Add(ei);
                }
                else
                {
                    msg.Append(string.Format(@Resources.Locale.L_RQManageController_Controllers_207, lsp_cd));
                    continue;
                }

                #region 发送mail
                map["RQ_REMARK"] = Prolink.Math.GetValueAsString(lspDr["REMARK"]);
                foreach (DataRow mrow in mailDt.Rows)
                {
                    mrow["LSP_CD"] = lsp_cd;
                    mrow["LSP_NM"] = lspDr["LSP_NM"];
                }

                try
                {
                    MailInfo mi = new MailInfo();
                    switch (bidder)
                    {
                        case "N":
                            map["BIDDER"] = @Resources.Locale.L_RQSetup_LostBid;
                            mi.Subject = rfq_no + @Resources.Locale.L_RQQuery_UnBidNotify1;
                            mailType = "RFQN";
                            break;
                        case "B":
                            map["BIDDER"] = @Resources.Locale.L_RQSetup_Bidder;
                            mi.Subject = rfq_no + @Resources.Locale.L_RQQuery_UnBidNotify1;
                            mailType = "RFQB";
                            break;
                        case "F":
                            map["BIDDER"] = @Resources.Locale.L_RQSetup_FinalBid;
                            mi.Subject = rfq_no + @Resources.Locale.L_RQManage_Controllers_446;
                            mailType = "RFQF";
                            break;
                    }

                    string htmltemp = string.Empty;
                    string key = mailType + "#" + group_id + "#" + cmp;
                    if (mailTemp.ContainsKey(key))
                    {
                        htmltemp = mailTemp[key];
                    }
                    else
                    {
                        htmltemp = MailTemplate.GetMailBody(mailType, group_id, cmp);
                        mailTemp[key] = htmltemp;
                    }

                    mi.Body = parse.Parse(mailDt, null, htmltemp, map);
                    mi.BodyFormat = "HTML";
                    mi.CC = "";
                    DataTable mailGroupDt = TrackingEDI.Mail.MailTemplate.GetMailGroup(notify_group, group_id, "Q");
                    foreach (DataRow mailGroup in mailGroupDt.Rows)
                    {
                        string to = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                        if (string.IsNullOrEmpty(to))
                            continue;
                        mi.To = to;
                        MailServices.GetInstance().SendMail(mi);
                    }
                }
                catch { }
                #endregion
            }
            if (m_ei != null)
                ml.Add(m_ei);
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = @Resources.Locale.L_RQManageController_Controllers_208 + (msg.Length>0?"("+msg.ToString()+")":""), flag = flag });
        }

        /// <summary>
        /// 比价计算
        /// </summary>
        /// <returns></returns>
        public ActionResult ComparePrice()
        {
            List<string> lcs = new List<string>();

            string lc = Request["LC"];//通知人数组
            string lsp_filter = string.Empty;
            if (!string.IsNullOrEmpty(lc))
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                lcs = js.Deserialize<List<string>>(lc);

                for (int i = 0; i < lcs.Count; i++)
                {
                    lcs[i] = SQLUtils.QuotedStr(lcs[i]);
                }
                if (lcs.Count > 0)
                {
                    lsp_filter = string.Format(" AND LSP_CD IN ({0})", string.Join(",", lcs));
                }
            }

            string u_id = Request["U_ID"];
            string sql = string.Format("SELECT * FROM SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable smrqmDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smrqmDt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_RQManage_Controllers_448, flag = true });

            string rfq_no = Prolink.Math.GetValueAsString(smrqmDt.Rows[0]["RFQ_NO"]);
            string period = Prolink.Math.GetValueAsString(smrqmDt.Rows[0]["PERIOD"]);
            string tran_type = Prolink.Math.GetValueAsString(smrqmDt.Rows[0]["TRAN_MODE"]);

            sql = string.Format("SELECT * FROM SMQTD WHERE RFQ_NO={0} AND (QUOT_TYPE<>'V' OR QUOT_TYPE IS NULL) {1} ORDER BY LSP_CD", SQLUtils.QuotedStr(rfq_no), lsp_filter);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, decimal> prices = new Dictionary<string, decimal>();
            List<string> keys = new List<string>();
            string filter = string.Empty;

            List<int> indexs = GetIndexs(tran_type);
            foreach (DataRow dr in dt.Rows)
            {
                //filter = string.Format("{0}#{1}#{2}", Prolink.Math.GetValueAsString(dr["POL_CD"]), Prolink.Math.GetValueAsString(dr["POD_CD"]), Prolink.Math.GetValueAsString(dr["TRAN_MODE"]));
                string region = Prolink.Math.GetValueAsString(dr["REGION"]);
                string state = Prolink.Math.GetValueAsString(dr["STATE"]);

                string loading = string.Empty;
                string loading_from = Prolink.Math.GetValueAsString(dr["LOADING_FROM"]);
                string loading_to = Prolink.Math.GetValueAsString(dr["LOADING_TO"]);
                if (!string.IsNullOrEmpty(loading_from))
                    loading += " AND LOADING_FROM=" + SQLUtils.QuotedStr(loading_from);

                if (!string.IsNullOrEmpty(loading_to))
                    loading += " AND LOADING_TO=" + SQLUtils.QuotedStr(loading_to);

                switch (tran_type)
                {
                    case "F"://海运整柜
                        if ("B".Equals(period))
                        {
                            filter = string.Format("POD_CD={0} AND POL_CD={1} AND CHG_CD={2}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CHG_CD"])));

                            if (!string.IsNullOrEmpty(region))
                                filter += " AND REGION=" + SQLUtils.QuotedStr(region);
                            else
                                filter += " AND (REGION='' OR REGION IS NULL)";
                        }
                        else//FCL 費用
                            filter = string.Format("POL_CD={0} AND CHG_CD={1}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CHG_CD"])));
                        break;
                    case "D": //国内快递
                        //filter = string.Format("CARRIER ={0} AND POD_CD={1}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CMP"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])));
                        filter = string.Format("POD_CD={0} AND POL_CD={1} AND CHG_CD={2}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CHG_CD"])));
                        break;
                    case "L"://海运散货
                        filter = string.Format("POD_CD={0} AND POL_CD={1} AND CHG_CD={2}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CHG_CD"])));
                        break;
                    case "A"://空运
                        filter = string.Format("POD_CD={0} AND POL_CD={1} AND CHG_CD={2}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["CHG_CD"])));
                        break;
                    case "T"://国内运输
                        if (!"FRT".Equals(Prolink.Math.GetValueAsString(dr["CHG_CD"])))
                            continue;

                        filter = string.Format("POD_CD={0} AND POL_CD={1}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])));

                        if (!string.IsNullOrEmpty(region))
                            filter += " AND REGION=" + SQLUtils.QuotedStr(region);
                        else
                            filter += " AND (REGION='' OR REGION IS NULL)";
                        if (!string.IsNullOrEmpty(state))
                            filter += " AND STATE=" + SQLUtils.QuotedStr(state);
                        else
                            filter += " AND (STATE='' OR STATE IS NULL)";
                        break;
                    case "E": //国际快递
                        filter = string.Format("POL_CD={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])));
                        if (!string.IsNullOrEmpty(region))
                            filter += " AND REGION=" + SQLUtils.QuotedStr(region);
                        else
                            filter += " AND (REGION='' OR REGION IS NULL)";
                        //filter = string.Format("TRAN_MODE={0} AND POD_CD={1} AND POL_CD={2} AND STATE={3}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["TRAN_MODE"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["STATE"])));
                        break;
                }
                switch (tran_type)
                {
                    case "F"://海运整柜
                        if (!string.IsNullOrEmpty(loading))
                            filter += loading;
                        break;
                }
                if (keys.Contains(filter))
                    continue;
                keys.Add(filter);

                //for (int i = 1; i <= 50; i++)
                foreach (int i in indexs)
                {
                    DataRow[] orderPrices = dt.Select(filter, "F" + i);
                    //bool isOrder = false;
                    //foreach (DataRow orderPrice in orderPrices)
                    //{
                    //    decimal test = Prolink.Math.GetValueAsDecimal(orderPrice["F" + i]);
                    //    if (test != 0)
                    //    {
                    //        isOrder = true;
                    //        break;
                    //    }
                    //}

                    int order = 1;
                    DataRow pre_orderPrice = null;
                    //for (int j = 0; j < orderPrices.Length; j++)
                    foreach (DataRow orderPrice in orderPrices)
                    {
                        decimal val=Prolink.Math.GetValueAsDecimal(orderPrice["F" + i]);
                        if (val == 0)
                        {
                            orderPrice["L" + i] = 0;
                            continue;
                        }
                        if (pre_orderPrice != null && val == Prolink.Math.GetValueAsDecimal(pre_orderPrice["F" + i]))
                        {
                            orderPrice["L" + i] = pre_orderPrice["L" + i];
                        }
                        else
                        {
                            orderPrice["L" + i] = order;
                            order++;
                        }
                        pre_orderPrice = orderPrice;
                    }
                }
            }
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            #region 更新排名
          
            foreach (DataRow dr in dt.Rows)
            {
                //ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
                //ei.PutKey("U_ID", dr["U_ID"]);
                string up_id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                bool up = false;
                string updateData = string.Empty;
                //for (int i = 1; i <= 50; i++)
                foreach(var i in indexs)
                {
                    if (dr["L" + i] != null && dr["L" + i] != DBNull.Value)
                    {
                        if (updateData.Length > 0)
                            updateData += ",";
                        updateData += string.Format("L{0}={1}", i, dr["L" + i]);
                        //ei.Put("L" + i, dr["L" + i]);
                        up = true;
                    }
                }
                if (up)
                    ml.Add(string.Format("UPDATE SMQTD SET {0} WHERE U_ID={1}", updateData, SQLUtils.QuotedStr(up_id)));
                //if (up)
                //    ml.Add(ei);
                ml=ExecuteLimitUpdate(ml, 100);
            }
            ml=ExecuteLimitUpdate(ml);
            #endregion

            #region 计算最低价次数
            if (string.IsNullOrEmpty(lsp_filter))
            {
                Dictionary<string, int> lspCdMap = new Dictionary<string, int>();
                foreach (DataRow dr in dt.Rows)
                {
                    string lsp_cd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                    if (string.IsNullOrEmpty(lsp_cd))
                        continue;
                    if (!lspCdMap.ContainsKey(lsp_cd))
                        lspCdMap[lsp_cd] = 0;
                    if ("F".Equals(tran_type))
                    {
                        if (Prolink.Math.GetValueAsInt(dr["L3"]) == 1)
                            lspCdMap[lsp_cd]++;
                    }
                    else
                    {
                        //for (int i = 1; i <= 50; i++)
                        foreach (int i in indexs)
                        {
                            if (Prolink.Math.GetValueAsInt(dr["L" + i]) == 1)
                                lspCdMap[lsp_cd]++;
                        }
                    }
                }

                foreach (var kv in lspCdMap)
                {
                    ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_FID", u_id);
                    ei.PutKey("LSP_CD", kv.Key);
                    ei.Put("BEST_PRICE", kv.Value);
                    ml.Add(ei);
                }
            }
            #endregion
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            return Json(new { message = @Resources.Locale.L_RQManage_Controllers_451, flag = true });
        }

        private static MixedList ExecuteLimitUpdate(MixedList ml, int limit = 0)
        {
            if (ml.Count > limit)
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                ml = new MixedList();
            }
            return ml;
        }

        private static List<int> GetIndexs(string tran_type)
        {
            List<int> indexs = new List<int>();
            for (int i = 1; i <= 50; i++)
                indexs.Add(i);

            switch (tran_type)
            {
                case "F"://海运整柜
                    //indexs = new List<int>() { 2, 3, 4 };
                    indexs = new List<int>() { 3 };
                    //indexs = new List<int>() { 2, 3, 4, 12, 13, 14 };
                    break;
                case "D": //国内快递
                    indexs = new List<int>() { 1, 2, 3, 4 };
                    break;
                case "L"://海运散货
                    indexs = new List<int>() { 1 };
                    break;
                case "A"://空运
                    indexs = new List<int>() { 1, 2, 3, 4, 5, 6,7 };
                    break;
                case "T"://国内运输   
                    indexs = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                    break;
                //case "E": //国际快递
                //    indexs = new List<int>() { 2, 3, 4, 12, 13, 14 };
                //    break;
            }
            return indexs;
        }

        private static void SetTableEdit(DataTable mainDt)
        {
            mainDt.Columns.Add("SYS_EDIT", typeof(bool));//是否允许编辑
            DateTime now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            foreach (DataRow dr in mainDt.Rows)
            {
                dr["SYS_EDIT"] = true;
                if (dr["RFQ_TO"] != null && dr["RFQ_TO"] != DBNull.Value)
                {
                    DateTime rfq_to = (DateTime)dr["RFQ_TO"];
                    rfq_to = new DateTime(rfq_to.Year, rfq_to.Month, rfq_to.Day, 0, 0, 0);
                    if (now.CompareTo(rfq_to) > 0)
                        dr["SYS_EDIT"] = false;
                }
            }
        }
        #endregion

        /// <summary>
        /// 作废报价
        /// </summary>
        /// <returns></returns>
        public ActionResult VoidQT()
        {
            string uid = Request["UId"];
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT TRAN_MODE,QUOT_TYPE,RFQ_NO FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_395, flag = false });
            string quot_type = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_TYPE"]);
            string rfq_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RFQ_NO"]);
            string tran_mode = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_MODE"]);
            //if (!"Q".Equals(quot_type))
            //    return Json(new { message = "已投标", flag = false });
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            switch (tran_mode)
            {
                case "X":
                case "C":
                case "B":
                case "O":
                    if (!"I".Equals(ioflag))//非内部用户
                    {
                        if ("Q".Equals(quot_type))
                            return Json(new { message = @Resources.Locale.L_RQManage_Controllers_452, flag = false });
                        if ("A".Equals(quot_type))
                            return Json(new { message = @Resources.Locale.L_RQManage_Controllers_453, flag = false });
                    }
                    break;
            }

            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_ID", uid);
            ei.Put("QUOT_TYPE", "V");
            ei.Put("MODIFY_BY", this.UserId);
            DateTime odt = DateTime.Now;          
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("MODIFY_DATE", odt);
            ei.PutDate("MODIFY_DATE_L", ndt);
            ml.Add(ei);

            ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_FID", uid);
            ei.Put("QUOT_TYPE", "V");
            ml.Add(ei);

            ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RFQ_NO", rfq_no);
            ei.PutKey("LSP_CD", CompanyId);
            ei.Put("STATUS", "V");
            ml.Add(ei);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = @Resources.Locale.L_ActSetup_Scripts_76, flag = true });
        }

        /// <summary>
        /// 签核
        /// </summary>
        /// <returns></returns>
        public ActionResult ApproveQT()
        {
            string uid = Request["UId"];
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT QUOT_TYPE,RFQ_NO FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_395, flag = false });
            string quot_type = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_TYPE"]);
            string rfq_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RFQ_NO"]);
            if ("A".Equals(quot_type))
                return Json(new { message = @Resources.Locale.L_RQManage_Controllers_454, flag = false });
            if ("V".Equals(quot_type))
                return Json(new { message = @Resources.Locale.L_QTSetup_HasDiscd, flag = false });
            if (!"Q".Equals(quot_type))
                return Json(new { message = @Resources.Locale.L_RQManage_Controllers_455, flag = false });

            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_ID", uid);
            ei.Put("QUOT_TYPE", "A");
            ei.Put("APPROVE_BY", this.UserId);
            DateTime odt = DateTime.Now;            
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("APPROVE_DATE", odt);
            ei.PutDate("APPROVE_DATE_L", ndt);
            ml.Add(ei);

            ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("U_FID", uid);
            ei.Put("QUOT_TYPE", "F");
            ml.Add(ei);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = @Resources.Locale.L_RQManage_Controllers_456, flag = true });
        }

        /// <summary>
        /// 提交报价
        /// </summary>
        /// <returns></returns>
        public ActionResult SubmitQT()
        {
            string uid = Request["UId"];
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT TRAN_MODE,QUOT_TYPE,RFQ_NO FROM SMQTM WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_395, flag = false });
            string quot_type = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_TYPE"]);
            string tran_mode = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_MODE"]);
            if ("V".Equals(quot_type))
                return Json(new { message = @Resources.Locale.L_QTSetup_HasDiscd, flag = false });

            string submit_quot_type = "N";
            string key_quot_type= "Q";
            switch (tran_mode)
            {
                case "C":
                case "B":
                case "O":
                case "X":
                    if ("A".Equals(quot_type))
                        return Json(new { message = @Resources.Locale.L_RQManage_Controllers_454, flag = false });
                    submit_quot_type = "Q";
                    key_quot_type = "P";
                    if (submit_quot_type.Equals(quot_type))
                        return Json(new { message = @Resources.Locale.L_QTSetup_HasBid, flag = false });
                    break;
                default:
                    if (!key_quot_type.Equals(quot_type))
                        return Json(new { message = @Resources.Locale.L_QTSetup_HasBid, flag = false });
                    break;
            }
            
            string rfq_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RFQ_NO"]);
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.PutKey("QUOT_TYPE", quot_type);//key_quot_type
            ei.Put("QUOT_TYPE", submit_quot_type);
            DateTime odt = DateTime.Now;           
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            if (tran_mode.Equals("C") || tran_mode.Equals("B") || tran_mode.Equals("X"))
                ei.Put("APPROVE_TO", "A");
            //ei.PutDate("QUOT_DATE", odt);
            //ei.PutDate("QUOT_DATE_L", ndt);
            ml.Add(ei);

            ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RFQ_NO", rfq_no);
            ei.PutKey("LSP_CD", CompanyId);
            ei.Put("STATUS", "Q");
            ml.Add(ei);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = @Resources.Locale.L_RQManage_Controllers_457, flag = true });
        }

        public ActionResult FileUpload()
        {
            string groupId = string.Empty;
            string cmp = string.Empty;
            string stn = "*";

            string returnMessage = "Y";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                string jobNo = Request.Params["jobNo"];
                MixedList ml = new MixedList();
                string excelFileName = string.Empty;
                try
                {
                    string name=System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                    string strExt = System.IO.Path.GetExtension(file.FileName).ToLower();
                    string path = Server.MapPath("~/FileUploads/");
                    if (!strExt.EndsWith(".xls") && !strExt.EndsWith(".xlsx"))
                        throw new Exception(@Resources.Locale.L_QTManageController_Controllers_167);

                    excelFileName = System.IO.Path.Combine(path, name + DateTime.Now.ToString("yyyyMMddHHmmfff") + strExt);
                    file.SaveAs(excelFileName);
                    DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT GROUP_ID,CMP FROM SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(jobNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count <= 0)
                        throw new Exception(@Resources.Locale.L_RQManage_Controllers_458);

                    groupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
                    cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                    EDOCFileItem data = EDOCController.UploadFile2EDOC(jobNo, excelFileName, groupId, cmp, stn, UserId, "RFQ");
                    if (data == null)
                    {
                        returnMessage = @Resources.Locale.L_RQManage_Controllers_459;
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                }
                try
                {
                    if (System.IO.File.Exists(excelFileName))
                        System.IO.File.Delete(excelFileName);
                }
                catch { }
                Response.Write("<script type=\"text/javascript\">parent.CallBack(\"" + returnMessage + "\")</script>");
            }

            ViewBag.MenuBar = false;
            return View();
        }

        /// <summary>
        /// 作废报价
        /// </summary>
        /// <returns></returns>
        public ActionResult VoidRQ()
        {
            string uid = Request["UId"];
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT TRAN_MODE,STATUS,RFQ_NO,CREATE_BY FROM SMRQM WHERE U_ID={0}", SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return Json(new { message = @Resources.Locale.L_QTManage_Controllers_395, flag = false });
            string status = Prolink.Math.GetValueAsString(dt.Rows[0]["STATUS"]);
            string rfq_no = Prolink.Math.GetValueAsString(dt.Rows[0]["RFQ_NO"]);
            string create_by = Prolink.Math.GetValueAsString(dt.Rows[0]["CREATE_BY"]);
            //if (!"Q".Equals(quot_type))
            //    return Json(new { message = "已投标", flag = false });
            string ioflag = this.IOFlag;
            if (!string.IsNullOrEmpty(ioflag))
                ioflag = ioflag.ToUpper();
            if ("D".Equals(status))
            {
                return Json(new { message = @Resources.Locale.L_QTSetup_HasClosed, flag = false });
            }
            if ("V".Equals(status))
            {
                return Json(new { message = @Resources.Locale.L_QTSetup_HasDiscd, flag = false });
            }
            if (!create_by.Equals(this.UserId))
            {
                return Json(new { message = @Resources.Locale.L_RQManageController_Controllers_209, flag = false });
            }

            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMQTM", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("RFQ_NO", rfq_no);
            ei.Put("QUOT_TYPE", "V");
            ei.Put("MODIFY_BY", this.UserId);
            DateTime odt = DateTime.Now;          
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("MODIFY_DATE", odt);
            ei.PutDate("MODIFY_DATE_L", ndt);
            ml.Add(ei);

            ei = new EditInstruct("SMQTD", EditInstruct.UPDATE_OPERATION);
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            ei.PutKey("RFQ_NO", rfq_no);
            ei.Put("QUOT_TYPE", "V");
            ml.Add(ei);

            ei = new EditInstruct("SMRQD", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("RFQ_NO", rfq_no);
            ei.Put("STATUS", "V");

            ei = new EditInstruct("SMRQM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("STATUS", "V");
            ml.Add(ei);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = @Resources.Locale.L_ActSetup_Scripts_76, flag = true });
        }


        public ActionResult InitiatedCheck(string uid = "")
        {
            string returnMsg = "";
            if (string.IsNullOrEmpty(uid))
                uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            if (string.IsNullOrEmpty(uid))
            {
                returnMsg = @Resources.Locale.L_DNManageController_Controllers_105;
            }
            else
            {
                string[] uids = uid.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < uids.Length; i++)
                {
                    MixedList mixList = new MixedList();
                    uid = uids[i].ToString();
                    UserInfo userinfo = new UserInfo
                    {
                        UserId = UserId,
                        CompanyId = CompanyId,
                        GroupId = GroupId,
                        Upri = UPri,
                        Dep = Dep,
                        basecondtions = GetBaseCmp()
                    };
                    IQTApproveHelper iqtHelper = new IQTApproveHelper(userinfo, uid,"A"); 
                    returnMsg += iqtHelper.ApprovePass() + "\r\n";
                }
            }
            return Json(new { message = returnMsg });
        }

        public ActionResult ApproveBackQuot()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string backremark = Prolink.Math.GetValueAsString(Request.Params["BackRemark"]);
            UserInfo userinfo = new UserInfo
            {
                UserId = UserId,
                CompanyId = CompanyId,
                GroupId = GroupId,
                Upri = UPri,
                Dep = Dep,
                basecondtions = GetBaseCmp()
            };
            IQTApproveHelper iqtHelper = new IQTApproveHelper(userinfo, uid,"B");
            string returnMsg = iqtHelper.ApproveBack(backremark);
            return Json(new { message = returnMsg });
        }
    }
}
