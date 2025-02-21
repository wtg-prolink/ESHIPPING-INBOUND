using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Prolink.DataOperation;
using Prolink.V3;
using WebGui.App_Start;
using System;
using Prolink.Data;
using Prolink.Web;
using System.Collections.Specialized;
using System.Collections;
using Prolink.Model;
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Web;
using Prolink;
using Prolink.Tools;
using System.Linq;
using System.Web.Configuration;
using System.Globalization;
using Business;

namespace WebGui.Controllers
{
    public class CommonController : BaseController
    {

        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        private static System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Resources.Locale", global::System.Reflection.Assembly.Load("App_GlobalResources"));
        #region V3 CORE
        public ActionResult Message(string id = null, string msgid = null)
        {
            if (msgid == null)
            {
                msgid = id;
            }

            string sql = "UPDATE GFMESSAGE SET STATUS='3' WHERE MSG_ID='" + msgid + "'";
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dt = OperationUtils.GetDataTable("SELECT CONTENT FROM GFMESSAGE WITH (NOLOCK) WHERE MSG_ID='" + msgid + "'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string content = Prolink.Math.GetValueAsString(dr["CONTENT"]);
                MessageData md = (MessageData)HierarchicalMap.CreateFromXml(content, typeof(MessageData));
                ViewBag.titletmp = md.Title;
                ViewBag.contenttmp = md.Content;
                ViewBag.msgidtmp = msgid;
            }
            ViewBag.pmsList = GetBtnPms("MESSAGE");
            return View();
        }

        /// <summary>
        /// 单笔无条件查询  目前还不支持分页
        /// </summary>
        /// <returns></returns>
        public ActionResult InquiryData()
        {
            string model = Request["model"];
            //int pageIndex = Prolink.Math.GetValueAsInt(Request.Params["page"]);//第几页
            //int pageSize = Prolink.Math.GetValueAsInt(Request.Params["rows"]);//每页大小
            int pageIndex = 0, pageSize = 0;
            string hasm = Request["hasm"];//是否包含实体定义
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> colKVS = null;
            if (!string.IsNullOrEmpty(Request["cols"]))
            {
                colKVS = js.DeserializeObject(Request["cols"]) as Dictionary<string, object>;
            }
            int total = 0;
            DataTable dt = ModelFactory.InquiryData(model, Request.Params, ref total, ref pageIndex, ref pageSize);
            BootstrapResult result = null;

            if (!"Y".Equals(hasm))
                result = BootstrapResult.GetBootstrapResult(model, dt, colKVS, pageIndex, total);
            else
            {
                result = new BootstrapResult() { rows = ModelFactory.ToTableJson(dt, model), total = dt.Rows.Count };
            }
            return result.ToContent();
        }

        private static List<Dictionary<string, object>> GetPermission(Permission p)
        {
            Dictionary<string, object> item = null;
            Dictionary<string, object> node = null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> nodes = null;
            foreach (var kv in p.Children)
            {
                nodes = new List<Dictionary<string, object>>();
                item = new Dictionary<string, object>();
                item["text"] = kv.Value.Text;
                item["href"] = "#";
                item["icon"] = "glyphicon glyphicon-folder-open";

                item["nodes"] = nodes;
                foreach (var menu in kv.Value.Children)
                {
                    node = new Dictionary<string, object>();
                    node["text"] = menu.Value.Text;
                    node["href"] = menu.Value.PmsId;
                    nodes.Add(node);
                }
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 根据角色获取菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllPermissionByRole()
        {
            string roleId = Request["roleId"] + "|" + GroupId + "|" + CompanyId + "|" + Station;
            Permission p = PermissionManager.GetAllPermissionByRole(roleId);
            List<Dictionary<string, object>> list = GetPermission(p);
            return ToContent(list);
        }

        /// <summary>
        /// 根据角色获得单个菜单的menuItem
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMenuItemPermissionByRole()
        {
            string roleId = Request["roleId"];
            string pmsId = Request["pmsId"];
            string GroupId = Request["GroupId"];
            string CompanyId = Request["CompanyId"];
            string Station = Request["Station"];
            Permission p = PermissionManager.GetMenuItemPermissionByRole(roleId + "|" + GroupId + "|" + CompanyId + "|" + Station, pmsId);
            string lang = "zh-CN";
            switch (SiteLang)
            {
                case "zh-TW":
                    lang = "zh-TW";
                    break;
                case "en-US":
                    lang = "en-US";
                    break;
                case "ru-RU":
                    lang = "ru-RU";
                    break;
            }
            CultureInfo cul = CultureInfo.CreateSpecificCulture(lang);

            //resources.re
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> item = null;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            result["roleId"] = roleId;
            result["permision"] = list;

            //DataTable edocDt = null;
            if (p != null)
            {
                Dictionary<string, Permission> sortedResult = p.Children;
                if ("EDOC".Equals(pmsId))
                    sortedResult = p.Children.OrderBy(o => o.Value.Text).ToDictionary(o => o.Key, o => o.Value);
                foreach (var kv in sortedResult)
                {
                    item = new Dictionary<string, object>();
                    item["checked"] = "Y".Equals(kv.Value.Allowed) ? 1 : 0;
                    if (kv.Value.Text.IndexOf("TLB_") > -1)
                    {
                        item["caption"] = resources.GetString(kv.Value.Text, cul);
                    }
                    else
                        item["caption"] = kv.Value.Text;
                    item["pmsId"] = kv.Value.PmsId;
                    if (kv.Key.Equals("EDOC_UP") || kv.Key.Equals("EDOC_DEL") || kv.Key.Equals("EDOC_DOWN") || kv.Key.Equals("EDOC_ALLDEL"))
                    {
                        list.Insert(0, item);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            return ToContent(result);
        }
        /// <summary>
        /// 获取权限菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult Menu()
        {
            //TODO 判斷cookie是否有MENU，並驗證COOKIE是否有被修改過，若任一為false就重新登入
            string menuStr = GetMenu();

            if (menuStr.Equals("") || menuStr == null)
            {
                return RedirectToAction("LoginOut", "Home");
                //return Content("{success: fasle}");
            }
            else
            {
                ContentResult res = new ContentResult();
                // res.Content = menuStr.Replace(@"\", string.Empty).TrimStart('"').TrimEnd('"');
                res.Content = menuStr;
                return res;
            }
        }
        public ActionResult VerifyToken()
        {
            Boolean isSameCookie = CheckToken();
            Dictionary<string, object> result = new Dictionary<string, object>();

            result["isSameCookie"] = isSameCookie;
            return ToContent(result);
        }

        /// <summary>
        /// 获取角色id为roleId的所有用户
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleUsers()
        {
            //string condtion = " AND (CMP=" + SQLUtils.QuotedStr(CompanyId) + " OR CMP = '*') AND (STN=" + SQLUtils.QuotedStr(Station) + " OR STN = '*') AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            
            
            string roleId = Request["roleId"];
            string cmp = Request["cmp"];
            string stn = Request["stn"];
            string condtion = "  AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + "  AND RCMP=" + SQLUtils.QuotedStr(cmp) + " AND RSTN=" + SQLUtils.QuotedStr(stn);
            DataTable dt = OperationUtils.GetDataTable("SELECT U_ID,U_NAME FROM SYS_ACCT WHERE U_ID IN (SELECT FACCT_ID FROM SYS_ACCT_ROLE WHERE FROLE_ID=" + SQLUtils.QuotedStr(roleId) + condtion + ")" + "AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            foreach (DataRow dr in dt.Rows)
            {
                item = new Dictionary<string, object>();
                item["UserID"] = Prolink.Math.GetValueAsString(dr["U_ID"]);
                item["UserName"] = Prolink.Math.GetValueAsString(dr["U_NAME"]);
                list.Add(item);
            }
            return ToContent(list);
        }

        /// <summary>
        /// 根据角色获得单个菜单的menuItem
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePermission()
        {
            MixedList ml = new MixedList();
            List<Role> roleList = new List<Role>();
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                string changeData = Request.Params["permision"];
                if (changeData != null)
                    //changeData = System.Web.HttpUtility.UrlDecode(changeData);
                    changeData= GetDecodeBase64ToString(changeData);
                JavaScriptSerializer js = new JavaScriptSerializer();
                object[] jsmodels = js.DeserializeObject(changeData) as object[];
                string roleId = string.Empty;
                string[] ugroupId = null;
                string[] ucmp = null;
                string[] ustn = null;
                string groupId = null;
                string cmp = null;
                string stn = null;


                EditInstruct ei = null;
                EditInstruct logEi = null;
                object[] list = null;
                object[] psms = null;
                object[] users = null;
                List<string> permissions = new List<string>();
                List<string> addPermission = new List<string>();
                List<string> delPermission = new List<string>();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                string fobjId = "", fpmlist = "";              
                foreach (Dictionary<string, object> item in jsmodels)
                {
                    roleId = Prolink.Math.GetValueAsString(item["roleID"]);
                    if (item.ContainsKey("permission"))
                    {
                        list = item["permission"] as object[];
                        foreach (Dictionary<string, object> permision in list)
                        {
                            if (permision.ContainsKey("psm"))
                            {
                                string pmsId = Prolink.Math.GetValueAsString(permision["pms-id"]);
                                psms = permision["psm"] as object[];
                                if (psms.Length > 0)
                                {
                                    groupId = Prolink.Math.GetValueAsString(item["groupID"]);
                                    cmp = Prolink.Math.GetValueAsString(item["cmp"]);
                                    stn = Prolink.Math.GetValueAsString(item["stn"]);

                                    ei = new EditInstruct("SYS_ROLE_OBJ_PMS", EditInstruct.DELETE_OPERATION);
                                    ei.PutKey("GROUP_ID", groupId);
                                    ei.PutKey("CMP", cmp);
                                    ei.PutKey("STN", stn);
                                    ei.PutKey("FROLE_ID", roleId);
                                    ei.PutKey("FOBJ_ID", pmsId);
                                    ml.Add(ei);

                                    ei = new EditInstruct("SYS_ROLE_OBJ_PMS", EditInstruct.INSERT_OPERATION);
                                    ei.PutKey("GROUP_ID", groupId);
                                    ei.PutKey("CMP", cmp);
                                    ei.PutKey("STN", stn);
                                    ei.PutKey("FROLE_ID", roleId);
                                    ei.PutKey("FOBJ_ID", pmsId);
                                    ei.Put("FPMLIST", "Y");     //对应是Permission的主menu
                                    ml.Add(ei);
                                }
                                permissions = new List<string>();
                                string sql = string.Format("SELECT FOBJ_ID,FPMLIST FROM SYS_ROLE_OBJ_PMS WHERE FOBJ_ID LIKE '{0}%' AND CMP={1} AND GROUP_ID={2} AND STN={3} AND FROLE_ID={4}", pmsId,
                                    SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(stn), SQLUtils.QuotedStr(roleId));
                                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                foreach (DataRow dr in dt.Rows)
                                {
                                    fobjId = Prolink.Math.GetValueAsString(dr["FOBJ_ID"]);
                                    fpmlist = Prolink.Math.GetValueAsString(dr["FPMLIST"]);
                                    if (!dic.ContainsKey(fobjId))
                                        dic.Add(fobjId, fpmlist);
                                    if (!permissions.Contains(fobjId))
                                        permissions.Add(fobjId);
                                }
                                foreach (Dictionary<string, object> psm in psms)
                                {
                                    fobjId = Prolink.Math.GetValueAsString(psm["pms-id"]);
                                    fpmlist = Prolink.Math.GetValueAsInt(psm["checked"]) == 1 ? "Y" : "N";
                                    ei = new EditInstruct("SYS_ROLE_OBJ_PMS", EditInstruct.DELETE_OPERATION);
                                    ei.PutKey("GROUP_ID", groupId);
                                    ei.PutKey("CMP", cmp);
                                    ei.PutKey("STN", stn);
                                    ei.PutKey("FROLE_ID", roleId);
                                    ei.PutKey("FOBJ_ID", fobjId);
                                    ml.Add(ei);

                                    ei = new EditInstruct("SYS_ROLE_OBJ_PMS", EditInstruct.INSERT_OPERATION);
                                    ei.PutKey("GROUP_ID", groupId);
                                    ei.PutKey("CMP", cmp);
                                    ei.PutKey("STN", stn);
                                    ei.PutKey("FROLE_ID", roleId);
                                    ei.PutKey("FOBJ_ID", fobjId);
                                    ei.Put("FPMLIST", fpmlist);//对应的增删改查权限
                                    ml.Add(ei);
                                    if (permissions.Contains(fobjId))
                                    {
                                        if (fpmlist == "N" && dic[fobjId] == "Y")
                                            delPermission.Add(fobjId);
                                        else if (fpmlist == "Y" && dic[fobjId] == "N")
                                            addPermission.Add(fobjId);
                                    }
                                    else if (fpmlist == "Y")
                                        addPermission.Add(fobjId);

                                    Business.CommonHelp.AddRoles(roleId, groupId, cmp, stn, roleList);
                                }
                            }
                        }
                        if (delPermission.Count() > 0 || addPermission.Count > 0)
                        {
                            logEi = new EditInstruct("SYS_ROLE_LOG", EditInstruct.INSERT_OPERATION);
                            logEi.Put("GROUP_ID", groupId);
                            logEi.Put("CMP", cmp);
                            logEi.Put("STN", stn);
                            logEi.Put("FROLE_ID", roleId);
                            logEi.Put("ROLE_TYPE", "Permission");
                            logEi.PutDate("CREATE_DATE", DateTime.Now);
                            logEi.Put("CREATE_BY", UserId);
                            logEi.Put("ROLE_ADD", string.Join(",", addPermission.ToArray()));
                            logEi.Put("ROLE_DEL", string.Join(",", delPermission.ToArray()));
                            ml.Add(logEi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result["result"] = false;
                result["resultDecp"] = ex.ToString().Substring(0, 200);           
                string errInfo = ex.ToString().Length > 2000 ? ex.ToString().Substring(0, 2000) : ex.ToString();
                EditInstruct ei = new EditInstruct("SYS_LOG", EditInstruct.INSERT_OPERATION);
                ei.Put("ID", System.Guid.NewGuid().ToString());
                ei.Put("MsgType", "UserPermission");            
                ei.Put("Remark", "UserPermission before save");
                ei.Put("MsgInfo", errInfo);
                ei.Put("CreateBy", UserId);
                ei.PutExpress("EventTime", "getdate()");
                ei.Put("Data", ex.Message);                
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                return ToContent(result);
            }            
           
            if (ml.Count > 0)
            {
                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                    result["result"] = true;
                    //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                    Prolink.V3.PermissionManager.SetRolePermission(roleList);
                    Business.CommonHelp.NotifyRebuidPermission(roleList);
                }
                catch (Exception ex)
                {
                    result["result"] = false;
                    result["resultDecp"] = "DB SQL Run Error!";
                    string errInfo = ex.ToString().Length > 2000 ? ex.ToString().Substring(0, 2000) : ex.ToString();
                    EditInstruct ei = new EditInstruct("SYS_LOG", EditInstruct.INSERT_OPERATION);
                    ei.Put("ID", System.Guid.NewGuid().ToString());
                    ei.Put("MsgType", "UserPermission");
                    ei.Put("Remark", "UserPermission save");
                    ei.Put("MsgInfo", errInfo);
                    ei.Put("CreateBy", UserId);
                    ei.PutExpress("EventTime", "getdate()");
                    ei.Put("Data", ex.Message);
                    OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
                


            return ToContent(result);
        }

        /// <summary>
        /// 抓取Message Data status = 1
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMessageKeep()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("CREATE_BY,CREATE_DATE,MSG_TYPE,CONTENT,GROUP_ID,MSG_ID", "GFMESSAGE", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = OperationUtils.GetDataTable("SELECT TOP 1 MSG_ID,CREATE_BY,CREATE_DATE,MSG_TYPE,CONTENT,GROUP_ID,MSG_ID FROM GFMESSAGE WITH (NOLOCK) WHERE STATUS ='1' AND RCV_CD = " + SQLUtils.QuotedStr(UserId) + " ORDER BY CREATE_DATE", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataColumn col = dt.Columns.Add("INDEX", System.Type.GetType("System.Int32"));
            foreach (DataRow dr in dt.Rows)
            {
                String msgid = Prolink.Math.GetValueAsString(dr["MSG_ID"]);
                string sql = "UPDATE GFMESSAGE SET STATUS='2' WHERE MSG_ID='" + msgid + "'";
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            //col.ColumnName = "INDEX";
            //col.DataType = System.Type.GetType("System.Int32");
            //col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "WORK_JOB";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "JOB_TYPE";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "TYPE";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "TITLE";
            col.MaxLength = 10000;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string content = Prolink.Math.GetValueAsString(dt.Rows[i]["CONTENT"]);
                MessageData md = (MessageData)HierarchicalMap.CreateFromXml(content, typeof(MessageData));
                dt.Rows[i]["WORK_JOB"] = md.JobNo;
                dt.Rows[i]["JOB_TYPE"] = md.JobType;
                dt.Rows[i]["TITLE"] = md.Title;
                dt.Rows[i]["CONTENT"] = md.Content;
                dt.Rows[i]["INDEX"] = (pageIndex - 1) * pageSize + 1 + i;
            }
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            return result.ToContent();
        }


        public ActionResult GetRoleOption()
        {
            return Json(GetRoleSelects());
        }

        public PriDataOptions GetRoleSelects()
        {
            string sql = "SELECT FID,FDESCP FROM SYS_ROLE WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(BaseCompanyId) + " AND STN=" + SQLUtils.QuotedStr(BaseStation);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            PriDataOptions iOptions = new PriDataOptions();
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["FID"]);
                cdDescp = Prolink.Math.GetValueAsString(dr["FDESCP"]);

                iOptions.Role.Add(new OptionsItem
                {
                    cd = cd,
                    cdDescp = cdDescp
                });


            }

            return iOptions;
        }

        public class PriDataOptions
        {
            public List<OptionsItem> Role = new List<OptionsItem>();

        }


        /// <summary>
        /// 抓取Message Data
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMessage()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string status = Request.Params["status"];

            DataTable dt = null;
            if (status == "4")
            {
                dt = ModelFactory.InquiryData("CREATE_BY,CREATE_DATE,'MSG' AS MSG_TYPE,NOTICE_CONTENT AS CONTENT,GROUP_ID,'M01' AS MSG_ID, NOTICE_SUBJECT", "SYS_NOTICE_RECORD WITH (NOLOCK) ", " CREATE_BY = " + SQLUtils.QuotedStr(UserId), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            }
            else
            {
                dt = ModelFactory.InquiryData("CREATE_BY,CREATE_DATE,MSG_TYPE,CONTENT,GROUP_ID,MSG_ID", "GFMESSAGE WITH (NOLOCK) ", " RCV_CD = " + SQLUtils.QuotedStr(UserId), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            }



            DataColumn col = dt.Columns.Add("INDEX", System.Type.GetType("System.Int32"));
            //col.ColumnName = "INDEX";
            //col.DataType = System.Type.GetType("System.Int32");
            //col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "WORK_JOB";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "JOB_TYPE";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "TYPE";
            col.MaxLength = 10000;

            col = dt.Columns.Add();
            col.ColumnName = "TITLE";
            col.MaxLength = 10000;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (status == "4")
                {
                    dt.Rows[i]["WORK_JOB"] = "";
                    dt.Rows[i]["JOB_TYPE"] = "";
                    dt.Rows[i]["TITLE"] = dt.Rows[i]["NOTICE_SUBJECT"];
                    dt.Rows[i]["CONTENT"] = dt.Rows[i]["CONTENT"];
                    dt.Rows[i]["INDEX"] = (pageIndex - 1) * pageSize + 1 + i;
                }
                else
                {
                    string content = Prolink.Math.GetValueAsString(dt.Rows[i]["CONTENT"]);
                    MessageData md = (MessageData)HierarchicalMap.CreateFromXml(content, typeof(MessageData));
                    dt.Rows[i]["WORK_JOB"] = md.JobNo;
                    dt.Rows[i]["JOB_TYPE"] = md.JobType;
                    dt.Rows[i]["TITLE"] = md.Title;
                    dt.Rows[i]["CONTENT"] = md.Content;
                    dt.Rows[i]["INDEX"] = (pageIndex - 1) * pageSize + 1 + i;
                }


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

        /// <summary>
        /// 新增消息
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="type">消息类型</param>
        /// <param name="jobType">工作类型</param>
        /// <param name="jobNo">工作号码</param>
        /// <param name="receiveUser">接收者</param>
        /// <param name="content">消息内容</param>
        /// <param name="user">操作用户</param>
        /// <returns>消息的流水号ID</returns>
        public string AddMessage(string title, string type, string jobType, string jobNo, string receiveUser, string content)
        {
            MessageData md = new MessageData();
            md.Type = type;
            md.Title = title;
            md.JobType = jobType;
            md.JobNo = jobNo;
            md.Content = content;
            EditInstruct ei = new EditInstruct("GFMESSAGE", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", GroupId);
            string msg_id = AutoNo.GetNo("MESSAGE_JOB_NO", new Hashtable(), GroupId, CompanyId, Station);
            ei.Put("MSG_ID", msg_id);
            ei.Put("RCV_CD", receiveUser);
            ei.Put("STATUS", MessageData.NOT_RECEVIE);
            ei.Put("CREATE_BY", UserId);
            ei.Put("MSG_TYPE", type);
            ei.Put("JOB_NO", jobNo);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutClob("CONTENT", md.ToXml());
            OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            return msg_id;
        }

        [ValidateInput(false)]
        public ActionResult ResetLayout()
        {
            string layoutId = Request.Params["layoutid"];
            string layoutType = Request.Params["layouttype"];
            string captionname = Prolink.Math.GetValueAsString(Request.Params["layoutName"]);

            if (string.IsNullOrEmpty(captionname))
                captionname = "default";

            string localcondition = " AND IO_TYPE='I' AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            string condtion = localcondition + " AND U_ID=" + SQLUtils.QuotedStr(UserId);
            //string layoutId = Request.Params["layoutId"];
            //string layoutType = Request.Params["layoutType"];
            DataTable dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                 " AND ID_NAME =" + SQLUtils.QuotedStr(captionname) + " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count == 0)
            {
                dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                 " AND ID_NAME ='default' AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            if (dt.Rows.Count == 0)
            {
                dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) + " AND ID_NAME ='default' AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) +
                    localcondition + " AND U_ID=" + SQLUtils.QuotedStr("ADMIN"), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }


            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["LAYOUT"] = null;
            foreach (DataRow dr in dt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    JArray jLayout = JArray.Parse(Prolink.Math.GetValueAsString(dr["LAYOUT"]));
                    item["LAYOUT"] = jLayout.ToString();
                }
                else
                {
                    string jLayout = Prolink.Math.GetValueAsString(dr["LAYOUT"]);
                    item["LAYOUT"] = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jLayout));
                }
            }

            string sql = "UPDATE SYS_LAYOUT SET IS_SELECT=Null WHERE ID=" + SQLUtils.QuotedStr(layoutId) +
               " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion;
            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mlist = new MixedList();
            string updatecondition = "ID = " + SQLUtils.QuotedStr(layoutId) +
                " AND ID_NAME =" + SQLUtils.QuotedStr(captionname) + " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion;
            EditInstruct ei = new EditInstruct("SYS_LAYOUT", EditInstruct.UPDATE_OPERATION);
            ei.Condition = updatecondition;
            ei.Put("IS_SELECT", "Y");
            mlist.Add(ei);
            OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());

            list.Add(item);

            sql = @"SELECT ID,ID_NAME,LAYOUT_TYPE,IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
            " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + condtion;

            if (!"ADMIN".Equals(UserId.ToUpper()))
            {
                sql += @" union
                SELECT ID,ID_NAME,LAYOUT_TYPE,NULL as IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + localcondition + " AND U_ID = 'admin' AND ID_NAME = 'default'";
            }

            DataTable layoutdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> griditem = new Dictionary<string, object>();
            List<string> namelist = new List<string>();
            foreach (DataRow dr in layoutdt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    namelist.Add(Prolink.Math.GetValueAsString(dr["ID_NAME"]));
                }
            }
            griditem["LAYOUT_ID_NAME"] = string.Join(";", namelist);
            list.Add(griditem);
            return ToContent(list);
        }

        public ActionResult DeleteLayout()
        {
            string layoutId = Request.Params["layoutid"];
            string layoutType = Request.Params["layouttype"];
            string captionname = Prolink.Math.GetValueAsString(Request.Params["layoutName"]);

            string localcondition = " AND IO_TYPE='I' AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            string condtion = localcondition + " AND U_ID=" + SQLUtils.QuotedStr(UserId);
            string updatecondition = "ID = " + SQLUtils.QuotedStr(layoutId) +
               " AND ID_NAME =" + SQLUtils.QuotedStr(captionname) + " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion;

            string sql = "select IS_SELECT from SYS_LAYOUT WHERE " + updatecondition;
            string isselect = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["LAYOUT"] = null;
            if ("Y".Equals(isselect))
            {
                sql = "SELECT LAYOUT,LAYOUT_TYPE FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND ID_NAME ='default' AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + localcondition + " AND U_ID=" + SQLUtils.QuotedStr("ADMIN");
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                foreach (DataRow dr in dt.Rows)
                {
                    if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                    {
                        JArray jLayout = JArray.Parse(Prolink.Math.GetValueAsString(dr["LAYOUT"]));
                        item["LAYOUT"] = jLayout.ToString();
                    }
                    else
                    {
                        string jLayout = Prolink.Math.GetValueAsString(dr["LAYOUT"]);
                        item["LAYOUT"] = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jLayout));
                    }
                }
            }
            list.Add(item);
            MixedList mlist = new MixedList();
            EditInstruct ei = new EditInstruct("SYS_LAYOUT", EditInstruct.DELETE_OPERATION);
            ei.Condition = updatecondition;
            mlist.Add(ei);
            OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = @"SELECT ID,ID_NAME,LAYOUT_TYPE,IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
            " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + condtion;

            if (!"ADMIN".Equals(UserId.ToUpper()))
            {
                sql += @" union
                SELECT ID,ID_NAME,LAYOUT_TYPE,NULL as IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + localcondition + " AND U_ID = 'admin' AND ID_NAME = 'default'";
            }

            DataTable layoutdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> griditem = new Dictionary<string, object>();
            List<string> namelist = new List<string>();
            foreach (DataRow dr in layoutdt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    namelist.Add(Prolink.Math.GetValueAsString(dr["ID_NAME"]));
                }
            }
            griditem["LAYOUT_ID_NAME"] = string.Join(";", namelist);
            list.Add(griditem);
            return ToContent(list);

        }

        [ValidateInput(false)]
        public ActionResult SetLayout()
        {
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            string layout = Request.Params["layout"];
            string layoutId = Request.Params["layoutId"];
            string layoutName = Request.Params["layoutName"];
            string layoutType = Request.Params["layoutType"];

            ei = new EditInstruct("SYS_LAYOUT", EditInstruct.DELETE_OPERATION);
            ei.PutKey("GROUP_ID", GroupId);
            ei.PutKey("CMP", CompanyId);
            ei.PutKey("STN", Station);
            ei.PutKey("ID", layoutId);
            ei.PutKey("ID_NAME", layoutName);
            ei.PutKey("U_ID", UserId);
            ei.PutKey("LAYOUT_TYPE", layoutType);
            ei.PutKey("IO_TYPE", "I");
            if (!layoutType.Equals("GRID"))
            {
                ei.PutKey("ID_NAME", layoutType);
            }
            ml.Add(ei);

            ei = new EditInstruct("SYS_LAYOUT", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("GROUP_ID", GroupId);
            ei.PutKey("CMP", CompanyId);
            ei.PutKey("STN", Station);
            ei.PutKey("ID", layoutId);
            ei.PutKey("U_ID", UserId);
            ei.PutKey("LAYOUT_TYPE", layoutType);
            ei.PutKey("IO_TYPE", "I");
            ei.Put("IS_SELECT", null);
            ml.Add(ei);

            ei = new EditInstruct("SYS_LAYOUT", EditInstruct.INSERT_OPERATION);
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            ei.Put("STN", Station);
            ei.Put("ID", layoutId);
            ei.Put("ID_NAME", layoutName);
            ei.Put("U_ID", UserId);
            ei.Put("LAYOUT_TYPE", layoutType);
            ei.Put("IS_SELECT", "Y");
            ei.Put("IO_TYPE", "I");

            if (layoutType.Equals("GRID"))
            {
                ei.Put("LAYOUT", layout);
            }
            else
            {
                ei.Put("ID_NAME", layoutType);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(layout);
                //編成 Base64 字串
                string b = Convert.ToBase64String(bytes);
                ei.Put("LAYOUT", b);
            }
            ml.Add(ei);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["result"] = true;

            return ToContent(result);
        }
        public ActionResult GetLayout()
        {
            string captionname = Prolink.Math.GetValueAsString(Request.Params["captionname"]);
            if (string.IsNullOrEmpty(captionname))
                captionname = "default";

            string loadconditon = " AND IO_TYPE='I' AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            string condtion = loadconditon + " AND U_ID =" + SQLUtils.QuotedStr(UserId);
            string layoutId = Request.Params["layoutId"];
            string layoutType = Request.Params["layoutType"];


            DataTable dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE,ID_NAME FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND IS_SELECT ='Y' AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count == 0)
            {
                dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE,ID_NAME FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                    " AND ID_NAME =" + SQLUtils.QuotedStr(captionname) + " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            if (dt.Rows.Count == 0)
            {
                dt = OperationUtils.GetDataTable("SELECT LAYOUT,LAYOUT_TYPE,ID_NAME FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) + " AND ID_NAME ='default' AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + " AND U_ID=" + SQLUtils.QuotedStr("ADMIN")
                    + loadconditon, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            item = new Dictionary<string, object>();
            item["LAYOUT"] = null;
            foreach (DataRow dr in dt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    JArray jLayout = JArray.Parse(Prolink.Math.GetValueAsString(dr["LAYOUT"]));
                    item["LAYOUT"] = jLayout.ToString();
                }
                else
                {
                    string jLayout = Prolink.Math.GetValueAsString(dr["LAYOUT"]);
                    item["LAYOUT"] = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jLayout));
                }
                string idName = Prolink.Math.GetValueAsString(dr["ID_NAME"]);
                item["SELECT_ID_NAME"] = !string.IsNullOrEmpty(idName) ? idName : "ADMIN".Equals(UserId.ToUpper()) ? "default" : "layout1";
            }
            list.Add(item);

            DataTable layoutdt = OperationUtils.GetDataTable("SELECT ID,ID_NAME,LAYOUT_TYPE FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND LAYOUT_TYPE =" + SQLUtils.QuotedStr(layoutType) + condtion, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> griditem = new Dictionary<string, object>();
            List<string> namelist = new List<string>();
            foreach (DataRow dr in layoutdt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    namelist.Add(Prolink.Math.GetValueAsString(dr["ID_NAME"]));
                }
            }
            griditem["LAYOUT_ID_NAME"] = string.Join(";", namelist);
            list.Add(griditem);
            //ResetAdmimLayout(ref list);
            return ToContent(list);
        }

        public ActionResult GetLayoutName()
        {
            string captionname = Prolink.Math.GetValueAsString(Request.Params["captionname"]);
            if (string.IsNullOrEmpty(captionname))
                captionname = "default";

            string localcondition = " AND IO_TYPE='I' AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            string condtion = localcondition + " AND U_ID=" + SQLUtils.QuotedStr(UserId);
            string layoutId = Request.Params["layoutId"];
            string layoutType = Request.Params["layoutType"];

            string sql = @"SELECT ID,ID_NAME,LAYOUT_TYPE,IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
             " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + condtion;

            if (!"ADMIN".Equals(UserId.ToUpper()))
            {
                sql += @" union
                SELECT ID,ID_NAME,LAYOUT_TYPE,NULL as IS_SELECT FROM SYS_LAYOUT WHERE ID =" + SQLUtils.QuotedStr(layoutId) +
                " AND LAYOUT_TYPE = " + SQLUtils.QuotedStr(layoutType) + localcondition + " AND U_ID = 'admin' AND ID_NAME = 'default'";
            }
            DataTable layoutdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> griditem = new Dictionary<string, object>();
            List<string> namelist = new List<string>();
            string selectlayout = "";
            foreach (DataRow dr in layoutdt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["LAYOUT_TYPE"]).Equals("GRID"))
                {
                    namelist.Add(Prolink.Math.GetValueAsString(dr["ID_NAME"]));
                    if ("Y".Equals(Prolink.Math.GetValueAsString(dr["IS_SELECT"])))
                    {
                        selectlayout = Prolink.Math.GetValueAsString(dr["ID_NAME"]);
                    }
                }
            }
            if (string.IsNullOrEmpty(selectlayout) && namelist.Count > 0)
                selectlayout = namelist[0];
            griditem["LAYOUT_ID_NAME"] = string.Join(";", namelist);
            griditem["LAYOUT_DEFAULT_NAME"] = !string.IsNullOrEmpty(selectlayout) ? selectlayout : "ADMIN".Equals(UserId.ToUpper()) ? "default" : "layout1";
            //ResetAdmimLayout(ref list);
            return ToContent(griditem);
        }

        public void ResetAdmimLayout(ref List<Dictionary<string, object>> lists)
        {
            List<Dictionary<string, object>> newlists = new List<Dictionary<string, object>>();
            if (lists.Count > 0)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                for (int i = 0; i < lists.Count; i++)
                {
                    foreach (var list in lists[i])
                    {
                        dic.Add(list.Key, list.Value);
                        if (list.Key.Equals("LAYOUT"))
                            dic["LAYOUT"] = "";
                    }
                    newlists.Add(dic);
                }
            }
            lists = null;
            lists = newlists;
        }
        /// <summary>
        /// 取得流水號
        /// </summary>
        /// <param>params(rulecode=XX&groupid=XX&cmp=XX&stn=XX...)</param>
        /// <returns></returns>
        public ActionResult GetAutoNo()
        {
            string[] paramArr = Request.Params["params"].ToString().Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string ruleCode = "";
            string group = GroupId;
            string cmp = CompanyId;
            string stn = Station;
            System.Collections.Hashtable hash = new System.Collections.Hashtable();

            for (int i = 0; i < paramArr.Length; i++)
            {
                string[] param = paramArr[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                if (param[0].Equals("rulecode"))
                {
                    ruleCode = param[1];
                }
                else if (param[0].Equals("groupid"))
                {
                    group = param[1];
                    hash.Add(param[0].ToUpper(), param[1]);
                }
                else if (param[0].Equals("cmp"))
                {
                    cmp = param[1];
                    hash.Add(param[0].ToUpper(), param[1]);
                }
                else if (param[0].Equals("stn"))
                {
                    stn = param[1];
                    hash.Add(param[0].ToUpper(), param[1]);
                }
                else
                {
                    hash.Add(param[0].ToUpper(), param[1]);
                }
            }


            string autoNo = AutoNo.GetNo(ruleCode, hash, group, cmp, "*");

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["autoNo"] = autoNo;
            list.Add(item);
            return ToContent(list);
        }

        /// <summary>
        /// 回收流水號
        /// </summary>
        /// <param>params(rulecode=XX&recoverno=XX&groupid=XX&cmp=XX&stn=XX...)</param>
        /// <returns></returns>
        public ActionResult RecoverAutoNo()
        {

            string[] paramArr = Request.Params["params"].ToString().Sanitize().Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string ruleCode = "";
            string group = GroupId;
            string cmp = CompanyId;
            string stn = Station;
            string recoverNo = "";
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            System.Collections.Hashtable hash = new System.Collections.Hashtable();

            for (int i = 0; i < paramArr.Length; i++)
            {
                string[] param = paramArr[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                if (param.Length < 2)
                {
                    item["status"] = "0";
                    list.Add(item);
                    return ToContent(list);
                }

                if (param[0].Equals("rulecode"))
                {
                    ruleCode = param[1];
                }
                else if (param[0].Equals("groupid"))
                {
                    group = param[1];
                }
                else if (param[0].Equals("cmp"))
                {
                    cmp = param[1];
                }
                else if (param[0].Equals("stn"))
                {
                    stn = param[1];
                }
                else if (param[0].Equals("recoverno"))
                {
                    recoverNo = param[1];
                }
            }

            AutoNo.RecoverNo(ruleCode, recoverNo, group, cmp, stn);
            item["status"] = "1";
            list.Add(item);
            return ToContent(list);
        }

        [HttpPost]
        public ActionResult SetMessageRead(string msgList, int status)
        {
            msgList = HttpUtility.UrlDecode(msgList);
            JavaScriptSerializer js = new JavaScriptSerializer();
            object[] items = js.DeserializeObject(msgList) as object[];
            MixedList ml = new MixedList();
            EditInstruct ei;
            foreach (Dictionary<string, object> item in items)
            {
                ei = new EditInstruct("GFMESSAGE", EditInstruct.UPDATE_OPERATION);
                ei.Put("STATUS", status);
                ei.PutKey("GROUP_ID", item["groupId"]);
                ei.PutKey("MSG_ID", item["msgId"]);
                ml.Add(ei);
            }
            int[] result = Prolink.DataOperation.OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (result == null)
                return Json(false);
            return Json(true);
        }

        [HttpPost]
        public ActionResult SendMessage(string data)
        {
            data = HttpUtility.UrlDecode(data);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var items = js.Deserialize<Dictionary<string, object>>(data);
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SYS_NOTICE_RECORD", EditInstruct.INSERT_OPERATION);
            ei.PutKey("ID", "M01");
            ei.PutKey("SYS_CODE", "MSG");
            ei.PutKey("PROG_CODE", "MSG");
            ei.PutKey("GROUP_ID", this.GroupId);
            ei.PutKey("CMP", this.CompanyId);
            ei.PutKey("STN", this.Station);
            ei.PutKey("U_ID", Guid.NewGuid().ToString());
            ei.Put("IS_SEND", "0");
            ei.Put("NOTICE_TYPE", items["NoticeType"]);
            ei.Put("NOTICE_SUBJECT", items["NoticeSubject"]);
            ei.Put("NOTICE_CONTENT", items["BullContent"]);
            string groupStr = "";
            if (items.ContainsKey("Group"))
            {
                groupStr = items["Group"].ToString().Replace(',', ';');
            }

    

            string Role = groupStr + ";[" + items["Account"].ToString() + "]";
            ei.Put("ROLE", Role);
            ei.Put("CREATE_BY", UserId);
            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutDate("MODIFY_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
            ei.PutDate("NOTICE_DATE", items["NoticeDate"].ToString());
            ei.Put("ROLE", Role);

            ml.Add(ei);


            try
            {
                int[] result = Prolink.DataOperation.OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(false);
            }     
            return Json(true);
        }

        /// <summary>
        /// 获取BSCODE资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetBsCodeData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += GetBaseCodeCondition();
            condition += " AND " + GetBaseGroup();
            DataTable dt = ModelFactory.InquiryData("CD,CD_DESCP", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取BSCODE资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetDisBsCodeData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += GetBaseCodeCondition();
            condition += " AND " + GetBaseGroup();
            DataTable dt = ModelFactory.InquiryData("DISTINCT CD,CD_DESCP", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取国家代码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCntryCdData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable dt = ModelFactory.InquiryData("*", "BSCNTY", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetStnData()
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("STN, NAME", "SYS_SITE", "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND DEP='*'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        //获取集团资料
        public ActionResult GetGroupData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SYS_SITE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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


        public string GetBaseCodeCondition()
        {
            return " AND CMP = " + SQLUtils.QuotedStr(BaseCompanyId) + " AND STN = " + SQLUtils.QuotedStr(BaseStation);
        }

        public ActionResult GetAutoCompData()
        {
            Dictionary<string, string> tableMap = new Dictionary<string, string>();
            tableMap["bsc"] = "BSCODE";
            tableMap["cus"] = "BSCS";
            tableMap["igd"] = "IPGOODS";
            tableMap["crn"] = "BSCUR";
            tableMap["country"] = "BSCNTY";
            tableMap["port"] = "BSCITY";
            tableMap["port1"] = "BSCITY";//要求5码
            tableMap["user"] = "SYS_ACCT";
            tableMap["bgn"] = "BSCHGCD";
            tableMap["ipo"] = "IPPOM";
            tableMap["cnt"] = "BSCNT";
            tableMap["ctm"] = "IPCTM";
            tableMap["btm"] = "IPBTM";
            tableMap["role"] = "SYS_ROLE";
            tableMap["elmt"] = "IPELMT";
            tableMap["part"] = "IPPART";
            tableMap["stn"] = "SYS_SITE";
            tableMap["ipt"] = "IPPART";
            tableMap["ptm"] = "IPPTM";
            tableMap["bank"] = "BSBANK";
            tableMap["bscsC"] = "V_BSCS_CUSTOM";
            tableMap["lcb"] = "IPLCB";
            tableMap["wcm"] = "IPWCM";
            tableMap["emp"] = "STEMP";
            tableMap["blcnt"] = "GFBLCNT";
            tableMap["cntab"] = "V_CNTAB";
            tableMap["ocntab"] = "V_OCNTAB";
            tableMap["som"] = "IPSOM";
            tableMap["status"] = "TKSTSCD";
            tableMap["bstport"] = "BSTPORT";
            tableMap["smpty"] = "SMPTY";
            tableMap["apprad"] = "APPROVE_ATTR_D";
            tableMap["smdn"] = "SMDN";
            tableMap["state"] = "BSSTATE";
            tableMap["chg"] = "SMCHG";
            tableMap["smwh"] = "SMWH";
            tableMap["smwhgt"] = "SMWHGT";
            tableMap["bsstate"] = "BSSTATE";
            tableMap["tpvport"] = "TPVPORT";
            tableMap["bstruckc"] = "BSTRUCKC";
            tableMap["bstruckd"] = "BSTRUCKD";
            tableMap["exp"] = "EXPRELA";
            tableMap["smcc"] = "SMCC";
            tableMap["smdnp"] = "SMDNP";
            tableMap["smsm"] = "SMSM";
            tableMap["bsdist"] = "BSDIST";
            tableMap["smdnsmbd"] = @"(SELECT SMDN.*,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='SH')SHIPPER,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='NT')NOTIFY,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='CS')CONSIGNEE,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='WE')SHIPTO,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='RE')BILLTO
FROM SMDN) AS SMDN";
            tableMap["smipc"] = "SMIPC";
            tableMap["smipm"] = "SMIPM";
            tableMap["smipr"] = "SMIPR";
            tableMap["vsmsm"] = "V_SMSM";
            tableMap["bslcpol"] = "BSLCPOL";
            tableMap["rqm"] = "SMRQM";
            tableMap["qtm"] = "SMQTM";
            tableMap["bsaddr"] = "BSADDR";
            tableMap["smsmi"] = "SMSMI";
            tableMap["cityport"]= string.Format("(SELECT CNTRY_CD,PORT_CD,PORT_NM,(CNTRY_CD+PORT_CD) AS POD,GROUP_ID FROM BSCITY UNION SELECT CNTRY_CD,PORT_CD,PORT_NM,PORT_CD AS POD,GROUP_ID FROM BSTPORT) T");
            string returnValue = Request.Params["returnValue"].ToString().Sanitize();
            string[] returnValueArr = returnValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            returnValueArr = returnValueArr.Where(val => !val.Contains("showValue")).ToArray();

            string[] paramArr = System.Web.HttpUtility.UrlDecode(Request.Params["params"]).ToString().Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string condition = "";
            string orderBy = "";
            string table = "";
            string selectNum = "10";
            Boolean clearData = true;

            string dt1 = string.Empty;
            for (int i = 0; i < paramArr.Length; i++)
            {
                string[] kv = paramArr[i].Split('=');

                if (kv[0].Equals("dt"))
                {
                    table = tableMap[kv[1]];
                    dt1 = kv[1];
                }
                else if (kv[0].Equals("num"))
                {
                    selectNum = kv[1];
                }
                else if (kv[0].Equals("desc"))
                {
                    orderBy += kv[1] + " DESC,";
                }
                else if (kv[0].Equals("asc"))
                {
                    orderBy += kv[1] + " ASC,";
                }
                else if (kv[0].Equals("clearevent"))
                {
                    if (kv[1] == "false")
                    {
                        clearData = false;
                    }
                }
                else
                {
                    if (kv.Length > 1)
                    {
                        condition += " AND " + kv[0] + " = " + SQLUtils.QuotedStr(kv[1]);
                    }
                    else
                    {
                        kv = paramArr[i].Split('~');
                        if (kv.Length > 1)
                        {
                            condition += " AND " + kv[0] + " LIKE " + SQLUtils.QuotedStr("%" + kv[1] + "%");
                        }
                        kv = paramArr[i].Split('^');
                        if (kv.Length > 1)
                        {
                            string fieldName = kv[0];
                            string[] liValues = kv[1].Split(';');
                            string liFilterStr = "";
                            for (int m = 0; m < liValues.Length;m++)
                            {
                                if (liValues[m] == "")
                                {
                                    continue;
                                }

                                if (liFilterStr == "")
                                {
                                    liFilterStr += " AND (" + fieldName + " LIKE " + SQLUtils.QuotedStr("%" + liValues[m] + ";%");
                                }
                                else
                                {
                                    liFilterStr += " OR " + fieldName + " LIKE " + SQLUtils.QuotedStr("%" + liValues[m] + ";%");
                                }
                            }
                            condition = liFilterStr + ")";
                        }
                        kv = paramArr[i].Split('%');
                        if (kv.Length > 1)
                        {
                            if ("port1".Equals(dt1))//5码的city
                            {
                                if (kv[1] != null && kv[1].Length == 5 && "PORT_CD".Equals(kv[0]))
                                    condition += string.Format(" AND CNTRY_CD={0} AND PORT_CD={1}", SQLUtils.QuotedStr(kv[1].Substring(0, 2)), SQLUtils.QuotedStr(kv[1].Substring(2, 3)));
                                else
                                    condition += " AND " + kv[0] + " LIKE " + SQLUtils.QuotedStr(kv[1] + "%");
                            }
                            else if ("chg".Equals(dt1) && "CHG_CD".Equals(kv[0]))//5码的city
                            {
                                condition += " AND " + kv[0] + " = " + SQLUtils.QuotedStr(kv[1]);
                            }
                             else if ("chg".Equals(dt1) && "CHG_CD".Equals(kv[0]))//5码的city
                            {
                                condition += " AND " + kv[0] + " = " + SQLUtils.QuotedStr(kv[1]);
                            }
                                //bsc
                            else
                                condition += " AND " + kv[0] + " LIKE " + SQLUtils.QuotedStr(kv[1] + "%");
                        }

                        kv = paramArr[i].Split('@');
                        if (kv.Length > 1)
                        {
                            kv[1] = kv[1].Replace(";", "','");
                            condition += " AND " + kv[0] + " in ('" + kv[1] + "')";
                        }
                    }

                }

            }

            if (table.Equals("BSCUR"))
            {
                //condition += " AND CMP = " + SQLUtils.QuotedStr(CompanyId) + " AND STN = " + SQLUtils.QuotedStr(Station);
            }

            /*if (table.Equals("BSCNTY") || table.Equals("BSCITY"))
            {
                condition += " AND CMP = " + SQLUtils.QuotedStr(BaseCompanyId) + " AND STN = " + SQLUtils.QuotedStr(BaseStation);
            }*/

            if (table.Equals("BSCODE"))
            {
                //condition += " AND " + GetBaseGroup();
                condition += string.Format(" AND GROUP_ID={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
                //condition += GetBaseCodeCondition();
            }
            if (table.Equals("BSCS") || table.Equals("V_BSCS_CUSTOM"))
            {
                condition += " AND STATUS=" + SQLUtils.QuotedStr("M");
            }

            if (orderBy != "")
            {
                orderBy = " ORDER BY " + orderBy + " 1 ";
            }
            if(string.IsNullOrEmpty(orderBy)&&("BSCODE").Equals(table)){
                orderBy = " ORDER BY CD ASC ";
            }
            string sql = " SELECT TOP " + selectNum + " " + returnValue.Replace("&", "+':'+").Replace("=", " AS ") + " FROM " + table + " WHERE 1=1 " + condition + orderBy;
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            try
            {
                DataTable dt = OperationUtils.GetDataTable(sql, new string[0], Prolink.Web.WebContext.GetInstance().GetConnection());
                ;

                foreach (DataRow dr in dt.Rows)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["label"] = Prolink.Math.GetValueAsString(dr["showValue"]);
                    Dictionary<string, object> subItem = new Dictionary<string, object>();
                    for (int j = 0; j < returnValueArr.Length; j++)
                    {
                        if (returnValueArr[j].IndexOf("=") > -1)
                        {
                            returnValueArr[j] = "CD";
                        }
                        subItem[returnValueArr[j]] = Prolink.Math.GetValueAsString(dr[returnValueArr[j]]);
                    }
                    item["returnValue"] = subItem;
                    list.Add(item);

                }

            }
            catch (Exception ex)
            {

            }
            return ToContent(list);
        }

        #endregion

        #region SOUND

        /// <summary>
        /// 获取部门
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDepData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='DE'");
        }

        /// <summary>
        /// 获取部门
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDisDepData()
        {
            return GetDisBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='DE'");
        }

        /// <summary>
        /// 获取业别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBuData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='BU'");
        }

        /// <summary>
        /// 获取运别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLuData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='LT'");
        }

        /// <summary>
        /// 获取贸易条件
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTermData()
        {
            return GetBsGroupCodeData("CD_TYPE='TINC'");
        }

        /// <summary>
        /// DLV Term
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDlvTermUrlData()
        {
            return GetBsGroupCodeData("CD_TYPE='TD'");
        }

        public ActionResult GetCntTypeData()
        {
            string sql = "SELECT DISTINCT CD+';' FROM BSCODE WHERE CMP='*' AND CD_TYPE='VERP' FOR XML PATH('') ";
            string cnttypes = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Json(new { message = cnttypes.Trim(';') });
        }

        /// <summary>
        /// 获取carriercode
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCarrier()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='AO'");
        }

        /// <summary>
        /// 获取工厂代码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPltCode()
        {
            return GetBsCodeData("AP_CD='" + CompanyId + "' AND GROUP_ID='" + GroupId + "' AND CD_TYPE='PLT'");
        }

        /// <summary>
        /// 获取DN权限公司
        /// </summary>
        public ActionResult GetTcmpCode()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND (CMP='*' OR CMP='" + CompanyId + "') AND CD_TYPE='TCMP'");
        }

        /// <summary>
        /// 获取口岸
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPortData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='PORT'");
        }

        /// <summary>
        /// 数量单位
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUnitData()
        {
            return GetDisBsCodeData(GetBaseCmp() + " AND CD_TYPE='UB'");
        }

        /// <summary>
        /// 重量单位
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNwuData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='UT'");
        }

        /// <summary>
        /// 流向
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTranNo()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='STA'");
        }

        /// <summary>
        /// 获取批号品号
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCoData()
        {
            return GetGoodsData();
        }

        /// <summary>
        /// 供应商查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSupplierData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%V%'");
        }

        public ActionResult GetBillCd()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND (CUST_TYPE LIKE '%M%' OR CUST_TYPE LIKE '%P%')");
        }


        /// <summary>
        /// 外檢查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetArbData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%L%'");
        }

        /// <summary>
        /// 落箱查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetYardData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%I%'");
        }

        /// <summary>
        /// 費用查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFeeData()
        {
            return GetBschgcdData("GROUP_ID='" + GroupId + "' AND CMP='" + BaseCompanyId + "' AND STN='" + BaseStation + "' AND DEP='AC' ");
        }

        /// <summary>
        /// 結單費用查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCloseFeeData()
        {
            return GetBschgcdData("GROUP_ID='" + GroupId + "' AND CMP='" + BaseCompanyId + "' AND STN='" + BaseStation + "'");
        }

        /// <summary>
        /// 委托人查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetConsignerData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%M%'");
        }

        /// <summary>
        ///检验机构查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPayData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%L%'");
        }

        /// <summary>
        /// 銀行查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBankData()
        {
            return GetBsBankData("1=1");
        }
        /// <summary>
        /// 銀行查询 FROM IPLCB
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLCBBankData()
        {
            return GetLCBBank("1=1");
        }


        /// <summary>
        /// 付款對象查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPayCustData()
        {
            return GetBscsccData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%V%'");
        }

        /// <summary>
        /// 销售對象查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSaleCustData()
        {
            return GetBscsccData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%M%'");
        }

        public ActionResult GetCarrierData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND SCAC_CODE IS NOT NULL AND CUST_TYPE LIKE '%F%'");
        }


        /// <summary>
        /// 付款對象查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetArbDetailData()
        {
            return GetBscsccData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%L%'");
        }

        /// <summary>
        /// 获取金屬采购点价基准/销售点价基准
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPriceBaseData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='TW'");
        }


        /// <summary>
        /// 获取EDM樣板類別
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTpltData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='TPLT'");
        }


        /// <summary>
        /// 获取BSBANK资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetBsBankData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            condition += GetBaseCodeCondition();
            DataTable dt = ModelFactory.InquiryData("BANK_CD,BANK_NM", "BSBANK", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取IPLCB资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetLCBBank(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            condition += GetBaseCodeCondition();
            DataTable dt = ModelFactory.InquiryData("*", "IPLCB", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取科目(費用代碼)资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetBschgcdData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCHGCD", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBscsData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            condition += " AND STATUS=" + SQLUtils.QuotedStr("M") + " AND VOID_BY IS NULL ";
            DataTable dt = ModelFactory.InquiryData("*", "BSCS", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBscsccData(string condition)
        {
            string stempCondition = "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'" + " AND STATUS=" + SQLUtils.QuotedStr("M");
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("V_BSCS_CUSTOM.*", "V_BSCS_CUSTOM", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBsBank()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSBANK", "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取币别资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetBscurData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("*", "BSCUR", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //string baseCondition = " CMP='" + CompanyId + "' AND STN='" + Station + "'";
            string baseCondition = " GROUP_ID=" + SQLUtils.QuotedStr(this.GroupId);
            //if(!string.IsNullOrEmpty(this.BaseCompanyId))
            //    baseCondition += " AND CMP=" + SQLUtils.QuotedStr(this.BaseCompanyId);
            DataTable dt = ModelFactory.InquiryData("*", "BSCUR", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetCrncyData()
        {
            return GetBscurData();
        }

        public ActionResult GetShipmentInfo()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable smsmiDt = ModelFactory.InquiryData("COMBINE_INFO", "SMSMI", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = new DataTable();
            if (smsmiDt.Rows.Count > 0)
            {
                string combineInfo = Prolink.Math.GetValueAsString(smsmiDt.Rows[0]["COMBINE_INFO"]);
              
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

        /// <summary>
        /// 获取货名资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetGoodsData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable dt = ModelFactory.InquiryData("*", "IPGOODS", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取大品名明细表
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetGoodscData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            string groupId = Prolink.Math.GetValueAsString(Request.Params["GROUP_ID"]),
                   goods = Prolink.Math.GetValueAsString(Request.Params["GOODS"]),
                   country = Prolink.Math.GetValueAsString(Request.Params["CNTRY_CD"]);
            string conditions = "GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND GOODS=" + SQLUtils.QuotedStr(goods) + " AND CNTRY_CD=" + SQLUtils.QuotedStr(country);

            //DataTable dt = ModelFactory.InquiryData("*", "IPGOODSC", "", "", ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = ModelFactory.InquiryData("t1.*,(SELECT t2.HS_CODE FROM IPGOODS t2 WHERE t1.GROUP_ID=t2.GROUP_ID AND t1.GOODS=t2.GOODS) HS_CODE", "IPGOODSC t1", conditions, "t1.GOODS DESC", pageIndex, pageSize, ref recordsCount);

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
        /// 获取箱型资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetCntData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCNT", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBlNoData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("IPSOD.*,IPWCM.BL_NO", "IPSOD,IPWCM", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetCntabData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "V_CNTAB", "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetOCntabData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "V_OCNTAB", "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        //获取员工档
        public ActionResult GetUsersData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        //获取员工档
        public ActionResult GetStempData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = "";
            DataTable dt = ModelFactory.InquiryData("*", "STEMP", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 仓库查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWmsData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE='W'");
        }

        /// <summary>
        /// 有色仓库查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWmsDataForA()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE='I'");
        }

        /// <summary>
        /// 物流反馈查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLbLookup()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='LB'");
        }

        /// <summary>
        /// 客户反馈查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSbLookup()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='SB'");
        }

        /// <summary>
        /// 箱型查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCntTypeLookup()
        {
            return GetCntData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'"); //目前CD_TYPE类型待定
        }

        /// <summary>
        /// 签核角色
        /// </summary>
        /// <returns></returns>
        public ActionResult GetaAprLookup()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='APR'");
        }

        /// <summary>
        /// 交單單據查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDocRmkData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='DOC'");
        }

        /// <summary>
        /// 合同
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIPCTMData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "IPCTM", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 信用證
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIPPOLData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "IPPOL", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 目标客户
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTargetCustomerData()
        {
            return GetBscsData("GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "' AND CUST_TYPE LIKE '%M%'");
        }

        /// <summary>
        /// 查询金属元素
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIpElemData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "IPELMT", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        /// 料号类别查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPartTypeData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='S01'");
        }

        //TPV仓库查询
        public ActionResult GetWHData()
        {
            return GetBsCodeData(GetBaseCmp() + " AND CD_TYPE='WH'");
        }

        /// <summary>
        /// 料号杂别查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPartOthData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='S02'");
        }

        /// <summary>
        /// 料号编号查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPartCdData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='S03'");
        }

        /// <summary>
        /// 料号包装查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPartPkgData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='S04'");
        }

        // <summary>
        /// 工厂查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMafNoData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='S05'");
        }

        // <summary>
        /// 交易所查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTreWebData()
        {
            return GetBsCodeData("GROUP_ID='" + GroupId + "' AND CD_TYPE='TW'");
        }

        //查询入库资料
        public ActionResult GetWcmData()
        {
            string condition = "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "IPWCM", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        //查询ifreight用户表
        public ActionResult GetIfUsers()
        {
            string condition = "GROUP_ID='" + GroupId + "' AND CMP='" + CompanyId + "' AND STN='" + Station + "'";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "STEMP", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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


        public class OptionsItem
        {
            public string cd { get; set; }
            public string cdDescp { get; set; }
        }


        public class IppomOptions
        {
            public List<OptionsItem> De = new List<OptionsItem>();
            public List<OptionsItem> Bu = new List<OptionsItem>();
            public List<OptionsItem> Lu = new List<OptionsItem>();
            public List<OptionsItem> Tt = new List<OptionsItem>();
            public List<OptionsItem> Td = new List<OptionsItem>();
            public List<OptionsItem> S006 = new List<OptionsItem>();
            public List<OptionsItem> S10 = new List<OptionsItem>();
            public List<OptionsItem> Ca = new List<OptionsItem>();
        }


        /// <summary>
        /// 取得冷链采购画面相关的选择选项
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSelectOptions()
        {
            return Json(GetSelects());
        }

        public IppomOptions GetSelects()
        {
            string sql = "SELECT CD,CD_DESCP,CD_TYPE FROM BSCODE WHERE GROUP_ID='" + GroupId + "' AND (CMP='" + BaseCompanyId + "' OR CMP='*') AND STN='" + BaseStation + "' AND CD_TYPE IN('BU','LU','DE','TT','TD','S006','S10','CA')";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            IppomOptions iOptions = new IppomOptions();
            string cd, cdDescp, cdType;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]);
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]);
                switch (cdType)
                {
                    case "DE":
                        {
                            iOptions.De.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "BU":
                        {
                            iOptions.Bu.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "LU":
                        {
                            iOptions.Lu.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "TT":
                        {
                            iOptions.Tt.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "TD":
                        {
                            iOptions.Td.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "S006":
                        {
                            iOptions.S006.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "S10":
                        {
                            iOptions.S10.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    case "CA":
                        {
                            iOptions.Ca.Add(new OptionsItem
                            {
                                cd = cd,
                                cdDescp = cdDescp
                            });
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }

            return iOptions;
        }

        #endregion

        #region TPV
        public ActionResult GetLookUpData()
        {
            //string model = Request["model"];
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BLPORT", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        /// 获取国家代码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCnData()
        {
            return GetCntryCdData();
        }

        public ActionResult GetPartyNoData(string condition="")
        {
            //string condition = "PARTY_TYPE LIKE '%IBCR%'";
            condition += GetBaseGroup();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMPTY",condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        public ActionResult GetPartyNoDataN()
        {
            string table = "SMPTY";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = InquiryData("*", table, GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize,"");

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
        public static string GetCondition(string condition)
        {
            if (!string.IsNullOrEmpty(condition))
            {
                string[] con = condition.Split('&');
                
                if (con.Length==2&&!string.IsNullOrEmpty(con[1]))
                {
                    condition = con[0];
                    string profile = con[1].Replace("PROFILE=","");
                    string partyno = string.Empty;
                    string sql = "SELECT * FROM SMSIM WHERE PROFILE=" + SQLUtils.QuotedStr(profile);
                    DataTable SMSIdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (condition.Contains("FS"))
                    {
                        foreach (DataRow dr in SMSIdt.Rows)
                        {
                            string Carrier1 = Prolink.Math.GetValueAsString(dr["CARRIER1"]);
                            string Carrier2 = Prolink.Math.GetValueAsString(dr["CARRIER2"]);
                            string Carrier3 = Prolink.Math.GetValueAsString(dr["CARRIER3"]);
                            if (!string.IsNullOrEmpty(Carrier1) && !partyno.Contains(Carrier1))
                            {
                                partyno += SQLUtils.QuotedStr(Carrier1) + ",";
                            }
                            if (!string.IsNullOrEmpty(Carrier2) && !partyno.Contains(Carrier2))
                            {
                                partyno += SQLUtils.QuotedStr(Carrier2) + ",";
                            }
                            if (!string.IsNullOrEmpty(Carrier3) && !partyno.Contains(Carrier3))
                            {
                                partyno += SQLUtils.QuotedStr(Carrier3) + ",";
                            }
                        }
                    }
                    if (condition.Contains("SP"))
                    {
                        foreach (DataRow dr in SMSIdt.Rows)
                        {
                            string LSP_NO1 = Prolink.Math.GetValueAsString(dr["LSP_NO1"]);
                            string LSP_NO2 = Prolink.Math.GetValueAsString(dr["LSP_NO2"]);
                            string LSP_NO3 = Prolink.Math.GetValueAsString(dr["LSP_NO3"]);
                            if (!string.IsNullOrEmpty(LSP_NO1) && !partyno.Contains(LSP_NO1))
                            {
                                partyno += SQLUtils.QuotedStr(LSP_NO1) + ",";
                            }
                            if (!string.IsNullOrEmpty(LSP_NO2) && !partyno.Contains(LSP_NO2))
                            {
                                partyno += SQLUtils.QuotedStr(LSP_NO2) + ",";
                            }
                            if (!string.IsNullOrEmpty(LSP_NO3) && !partyno.Contains(LSP_NO3))
                            {
                                partyno += SQLUtils.QuotedStr(LSP_NO3) + ",";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(partyno))
                    {
                        partyno = partyno.Substring(0, partyno.Length - 1);
                        partyno = string.Format("PARTY_NO IN ({0})", partyno);
                        condition += " AND " + partyno;
                    }
                }
            }
            return condition;
        }
        public static DataTable InquiryData(string colName, string defaultTable, string baseCondition, NameValueCollection nameValues, ref int total, ref int pageIndex, ref int pageSize,string orderBy)
        {
            int beginRow = 0, endRow = 0, page = 1, limit = 20;
            string page_str = nameValues["page"];//第几页  从1开始
            string limit_str = nameValues["rows"];//每页大小
            if (!int.TryParse(page_str, out page)) page = 0;
            if (!int.TryParse(limit_str, out limit)) limit = 20;
            beginRow = (page - 1) * limit;
            endRow = (page) * limit;
            string columns = nameValues["columns"], table = nameValues["table"],
                conditions = nameValues["conditions"];
            string _basecondition = BaseController.GetDecodeBase64ToString(Prolink.Math.GetValueAsString(nameValues["basecondition"]));
            _basecondition = GetCondition(_basecondition);
            string condition = string.Empty;
            //string orderBy = "";
            string sidx = nameValues["sidx"];
            string sord = nameValues["sord"];

            if (!string.IsNullOrEmpty(sidx))
            {
                //orderBy = sidx;
                string[] fs = sidx.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string f in fs)
                {
                    if (orderBy.Length > 0) orderBy += ",";
                    orderBy += ReplaceFiledToDBName(f);
                }
                orderBy += string.IsNullOrEmpty(sord) ? " ASC" : " " + sord.ToUpper();
            }

            if (conditions != null)
                conditions = HttpUtility.UrlDecode(conditions);
            if (string.IsNullOrEmpty(baseCondition))
            {
                baseCondition = _basecondition;
            }
            else
            {
                if (!string.IsNullOrEmpty(_basecondition))
                {
                    baseCondition += " AND " + _basecondition;
                }
                
            }

            //条件参数
            if (!string.IsNullOrEmpty(conditions))
            {
                condition = ConvParam2Condition(conditions, table);
            }

            if (!string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(baseCondition))
            {
                condition += " AND " + baseCondition;
            }
            else if (string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(baseCondition))
            {
                condition = baseCondition;
            }

            if (string.IsNullOrEmpty(table))
            {
                table = defaultTable;
            }
            if (string.IsNullOrEmpty(columns))
            {
                columns = colName;
            }
            else
            {
                string repalceColumns = "";
                string[] columnsList = columns.Split(',');
                foreach (string col in columnsList)
                {
                    repalceColumns += ',' + ReplaceFiledToDBName(col);
                }
                repalceColumns = repalceColumns.Trim(',');
                columns = repalceColumns;
            }

            InquiryInstruct ii = new InquiryInstruct(columns, table, orderBy);
            if (!string.IsNullOrEmpty(condition))
                ii.AddFilterBlock(new FilterBlock(condition));
            
            DataTable dt = null;
            if (endRow > 0)
            {
                dt = OperationUtils.GetDataTable(ii, Prolink.Web.WebContext.GetInstance().GetConnection(), beginRow, endRow);
                ii = new InquiryInstruct("1", table, orderBy);
                if (!string.IsNullOrEmpty(condition))
                    ii.AddFilterBlock(new FilterBlock(condition));
                DataTable dt1 = Prolink.DataOperation.OperationUtils.GetDataTable(ii, Prolink.Web.WebContext.GetInstance().GetConnection());
                total = Prolink.Math.GetValueAsInt(dt1.Rows.Count);
            }
            else
            {
                dt = OperationUtils.GetDataTable(ii, Prolink.Web.WebContext.GetInstance().GetConnection());
                total = dt.Rows.Count;
            }
            pageIndex = page;
            pageSize = limit;
            return dt;
        }
        public static string ConvParam2Condition(string conditions, string defaultTable)
        {
            //sopt_id=eq&id=sadf&sopt_invdate=ne&invdate=sdfasdfafd&_search=false&nd=1422945681209
            string[] cs = conditions.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            string[] cs1 = null;
            Dictionary<string, string> eq = new Dictionary<string, string>();
            Dictionary<string, string> pdt = new Dictionary<string, string>();
            Dictionary<string, string> data = new Dictionary<string, string>();
            #region 构建条件参数
            foreach (string c in cs)
            {
                if (string.IsNullOrEmpty(c))
                    continue;
                cs1 = c.Split(new string[] { "=" }, StringSplitOptions.None);
                if (cs1.Length < 2)
                    continue;
                if (cs1[0].StartsWith("sopt_"))
                {
                    eq[cs1[0]] = cs1[1];
                }
                else if (cs1[0].StartsWith("dt_"))
                {
                    pdt[cs1[0]] = cs1[1];
                }
                else
                {
                    data[cs1[0]] = cs1[1];
                }
            }
            #endregion
            #region 生成条件
            string fieldName = string.Empty;
            string[] fkv;
            string btType;
            string condition = string.Empty;
            string fieldVal = string.Empty;
            foreach (var kv in data)
            {
                fkv = kv.Key.Split(new string[] { "$" }, StringSplitOptions.RemoveEmptyEntries);

                if (eq.ContainsKey("sopt_" + fkv[0]))
                {
                    if (kv.Value == string.Empty && eq["sopt_" + fkv[0]] != "nu" && eq["sopt_" + fkv[0]] != "nn")
                        continue;

                    fieldVal = kv.Value.Replace("(@nd)", "&");


                    fieldName = ReplaceFiledToDBName(fkv[0]);
                    if (pdt.ContainsKey("dt_" + fkv[0]) && pdt["dt_" + fkv[0]] != "")
                    {
                        fieldName = ReplaceFiledToDBName(pdt["dt_" + fkv[0]]) + "." + fieldName;
                    }


                    if (defaultTable == "BSCODE")
                    {
                        if (fieldName == "CMP" || fieldName == "STN")
                            continue;
                    }
                    if (!string.IsNullOrEmpty(condition))
                        condition += " AND ";
                    switch (eq["sopt_" + fkv[0]])
                    {
                        case "eq": //eq 等于( = )
                            condition += fieldName + " = " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "ne": //ne 不等于( <> )
                            condition += fieldName + " <> " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "lt": //lt 小于( < )
                            condition += fieldName + " < " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "le": //le 小于等于( <= )
                            condition += fieldName + " <= " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "gt": //gt 大于( > )
                            condition += fieldName + " > " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "ge":  //ge 大于等于( >= )
                            condition += fieldName + " >= " + SQLUtils.QuotedStr(fieldVal);
                            break;
                        case "bw":  //bw 开始于 ( LIKE val% )
                            condition += fieldName + " LIKE '" + fieldVal + "%'";
                            break;
                        case "bn":  //bn 不开始于 ( not like val%)
                            condition += fieldName + " NOT LIKE '" + fieldVal + "%'";
                            break;
                        case "in":  //in 在内 ( in ())
                            string[] values = fieldVal.Split(';');
                            string filterStr = "";
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (filterStr == "")
                                {
                                    filterStr += SQLUtils.QuotedStr(values[i]);
                                }
                                else
                                {
                                    filterStr += "," + SQLUtils.QuotedStr(values[i]);
                                }
                            }
                            condition += fieldName + " IN (" + filterStr + ")";
                            break;
                        case "ni":  //ni 不在内( not in ())
                            string[] notvalues = fieldVal.Split(';');
                            string notFilterStr = "";
                            for (int i = 0; i < notvalues.Length; i++)
                            {
                                if (notFilterStr == "")
                                {
                                    notFilterStr += SQLUtils.QuotedStr(notvalues[i]);
                                }
                                else
                                {
                                    notFilterStr += "," + SQLUtils.QuotedStr(notvalues[i]);
                                }
                            }
                            condition += fieldName + "NOT IN (" + notFilterStr + ")";
                            break;
                        case "ew":    //ew 结束于 (LIKE %val )
                            condition += fieldName + " LIKE '%" + fieldVal + "'";
                            break;
                        case "en":  //en 不结束于
                            condition += fieldName + " NOT LIKE '%" + fieldVal + "'";
                            break;
                        case "cn":  //cn 包含 (LIKE %val% )
                            condition += fieldName + " LIKE " + SQLUtils.QuotedStr("%" + fieldVal + "%");
                            break;
                        case "nc":  //nc 不包含
                            condition += fieldName + " NOT LIKE " + SQLUtils.QuotedStr("%" + fieldVal + "%");
                            break;
                        case "nu":  // is null
                            condition += fieldName + " IS NULL";
                            break;
                        case "nn":  //is not null
                            condition += fieldName + " IS NOT NULL";
                            break;
                        case "bt":  //between

                            if (fkv.Length > 1 && fkv[1].Equals("S"))
                            {
                                condition += fieldName + " >= " + SQLUtils.QuotedStr(fieldVal);
                            }
                            if (fkv.Length > 1 && fkv[1].Equals("E"))
                            {
                                condition += fieldName + " <= " + SQLUtils.QuotedStr(fieldVal);
                            }
                            break;
                        case "li":  //lookup in
                            string[] liValues = fieldVal.Split(';');
                            string liFilterStr = "";
                            for (int i = 0; i < liValues.Length; i++)
                            {
                                if (liValues[i] == "")
                                {
                                    continue;
                                }

                                if (liFilterStr == "")
                                {
                                    liFilterStr += "(" + fieldName + " LIKE " + SQLUtils.QuotedStr("%" + liValues[i] + ";%");
                                }
                                else
                                {
                                    liFilterStr += " OR " + fieldName + " LIKE " + SQLUtils.QuotedStr("%" + liValues[i] + ";%");
                                }
                            }
                            condition = liFilterStr + ")";
                            break;
                        default:
                            condition += fieldName + " = " + SQLUtils.QuotedStr(fieldVal);
                            break;
                    }
                }
            }
            #endregion


            return condition;
        }
        public static string ReplaceFiledToDBName(string fieldName)
        {
            string replace = "";
            bool a = false;
            for (int i = 0; i < fieldName.Length; i++)
            {
                char code = Convert.ToChar(fieldName[i]);
                if (code <= 90 && code >= 65)
                {
                    if (a)
                    {
                        replace += "_" + fieldName[i].ToString().ToUpper();
                    }
                    else
                    {
                        replace += fieldName[i].ToString().ToUpper();
                    }
                    a = true;
                }
                else if (code <= 122 && code >= 97)
                {
                    replace += fieldName[i].ToString().ToUpper();
                }
                else
                    replace += fieldName[i].ToString();
            }
            return replace;
        }
        public ActionResult GetCountryData()
        {
            return GetCntyData("CMP='" + BaseCompanyId + "' AND STN='" + BaseStation + "'");
        }

        /// <summary>
        /// 获取国家资料
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetCntyData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCNTY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 起运港 目的港
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPolOrPodData()
        {
            return GetBscityData();
        }

        /// <summary>
        /// 获取港口资料 起运港 目的港
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public ActionResult GetBscityData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCITY", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        //city
        public ActionResult GetCityCdData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable dt = ModelFactory.InquiryData("*", "BSCITY", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetTruckPortCdData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable dt = ModelFactory.InquiryData("*", "BSTPORT", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetTruckPortAddrData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            DataTable dt = ModelFactory.InquiryData("*", "BSADDR", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetCompanyByPartyType()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string partytype = Prolink.Math.GetValueAsString(Request.Params["partyType"]);
            string conditions = "GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            //conditions += " AND TYPE='1'";
            //conditions += " AND PARTY_TYPE=" + SQLUtils.QuotedStr(partytype);
            DataTable dt = ModelFactory.InquiryData("*", "SYS_CMP", conditions, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        [HttpPost]
        public JsonResult CheckUserPmsForTPV()
        {
            string account = Prolink.Math.GetValueAsString(Request.Params["account"]);
            string password = Prolink.Math.GetValueAsString(Request.Params["password"]);
            string isDetail = Prolink.Math.GetValueAsString(Request.Params["isDetail"]);
            bool isdetail = false;
            if (isDetail == "checky")
            {
                isdetail = true;
            }
            return Json(new { message = OACheck.CheckUserPmsForTPV(account, password, isdetail) });
        }


        #endregion

        #region TPV 放大镜数据查询
        /// <summary>
        /// 公司别
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCompanyData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SYS_SITE", string.Format("GROUP_ID={0} AND TYPE='1'", SQLUtils.QuotedStr(GroupId)), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 获取運輸類別
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTranModeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TTRN'");
        }

        public ActionResult GetTrackingTranModeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TNT'");
        }

        public ActionResult GetOrderTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TVAK'");
        }

        public ActionResult GetQAType()
        {
            return GetBsGroupCodeData("CD_TYPE='IQAT'");
        }

        public ActionResult GetCntrType()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //因为代码建档完成就没根据集团公司站点去，所以注释
            string condition = GetDataPmsCondition("C");
            DataTable dt = ModelFactory.InquiryData("CHG_CD, CHG_DESCP", "ECREFFEE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBsGroupCodeData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //因为代码建档完成就没根据集团公司站点去，所以注释
            condition += " AND " + string.Format("GROUP_ID={0} AND (CMP='*' OR CMP={1})", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(this.BaseCompanyId));
            DataTable dt = ModelFactory.InquiryData("CD,CD_DESCP, ORDER_BY", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetPartyTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='PT'");
        }

        public ActionResult GetMailTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='MT'");
        }

        public ActionResult GetTrackWayData()
        {
            return GetBsGroupCodeData("CD_TYPE='TDTK'");
        }

        public ActionResult GetCarTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TDT'");
        }

        public ActionResult GetCargoTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TCGT'");
        }

        public ActionResult GetChannelData()
        {
            return GetBsGroupCodeData("CD_TYPE='TVTW'");
        }

        public ActionResult GetDivisionData()
        {
            return GetBsGroupCodeData("CD_TYPE='TSPA'");
        }

        public ActionResult GetPorteData()
        {
            return GetBsGroupCodeData("CD_TYPE='TVST'");
        }

        public ActionResult GetTVKOData()
        {
            return GetBsGroupCodeData("CD_TYPE='TVKO'");
        }

        public ActionResult GetTCARData()
        {
            return GetBsGroupCodeData("CD_TYPE='TCAR'");
        }

        public ActionResult GetRCARData()
        {
            return GetBsGroupCodeData("1=1");
        }

        /// <summary>
        /// REGION 获取地区code和描述
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRegionData()
        {
            return GetBsCodeData(GetBaseCmp() + " AND CD_TYPE='TRGN'");
        }


        /// <summary>
        /// 货况类型查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStatusData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "TKSTSCD", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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


        public ActionResult GetBscsBsCodeData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += GetBaseCodeCondition();
            DataTable dt = ModelFactory.InquiryData("CD,CD_DESCP", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        public ActionResult GetSmptyCmpData()
        {
            return GetSmptyData("GROUP_ID='" + GroupId + "' AND PARTY_TYPE LIKE '%SP%'");
        }
        public ActionResult GetSmptyExpressData()
        {
            return GetSmptyData("GROUP_ID='" + GroupId + "' AND PARTY_TYPE LIKE '%EX%'");
        }

        public ActionResult GetSmptyData(string condition)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += " AND STATUS=" + SQLUtils.QuotedStr("M") + " AND VOID_BY IS NULL ";
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

        public ActionResult GetBscsLocationData(string condition)
        {
            //NameValueCollection nv = new NameValueCollection();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            //condition += GetBaseCodeCondition();
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

        /// <summary>
        /// 城市港口
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCityPortData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCITY", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetCityAndTruckPortData()
        {
            string table = string.Format("(SELECT CNTRY_CD,PORT_CD,PORT_NM,(CNTRY_CD+PORT_CD) AS POD,GROUP_ID FROM BSCITY UNION SELECT CNTRY_CD,PORT_CD,PORT_NM,PORT_CD AS POD,GROUP_ID FROM BSTPORT) T");
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", table, GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        /// 客户建档查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCustomerData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "TK_CMP", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetTpvPortLData()
        {
            return GetTpvPortData("L");
        }

        public ActionResult GetTpvPortDData()
        {
            return GetTpvPortData("D");
        }

        public ActionResult GetTpvPortData(string type)
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string conditons = "FLAG=" + SQLUtils.QuotedStr(type);
            DataTable dt = ModelFactory.InquiryData("*", "TPVPORT", conditons, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 目的地
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDeliveryData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSTPort", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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
        /// 裝運類型
        /// </summary>
        /// <returns></returns>
        public ActionResult GetServiceModeData()
        {
            return GetBsCodeData(GetBaseGroup() + " AND CD_TYPE='PK'");
        }

        /// <summary>
        /// 文件类型
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFileTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='EDT'");
        }

        public ActionResult GetDnQueryData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string table = @"(SELECT SMDN.*,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='SH')SHIPPER,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='NT')NOTIFY,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='CS')CONSIGNEE,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='WE')SHIPTO,
(SELECT TOP 1 PARTY_NO+'|'+ PARTY_NAME FROM SMDNPT WHERE SMDNPT.DN_NO=SMDN.DN_NO AND SMDNPT.PARTY_TYPE='RE')BILLTO
FROM SMDN) AS SMDN";

            DataTable dt = ModelFactory.InquiryData("*", table, GetUpriCondition()+" AND TRAN_TYPE='T' AND STATUS='D' ", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult GetBankInfo()
        {
            string cmp = Request.Params["cmp"].ToString();

            string sql = string.Format("SELECT * FROM SMBKINFO WHERE CMP={0}", SQLUtils.QuotedStr(cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            if ("O".Equals(IOFlag) && dt.Rows.Count == 1)
            {
                item["success"] = true;
                item["CollectBank"] = dt.Rows[0]["COLLECT_BANK"];
                item["AccountName"] = dt.Rows[0]["ACCOUNT_NAME"];
                item["BankInfo"] = dt.Rows[0]["BANK_INFO"];
                item["SwiftCode"] = dt.Rows[0]["SWIFT_CODE"];
                item["BankType"] = dt.Rows[0]["BANK_TYPE"];
                item["Crncy"] = dt.Rows[0]["CRNCY"];
                item["DebitType"] = "USD".Equals(Prolink.Math.GetValueAsString(dt.Rows[0]["CRNCY"])) ? "V" : "";
            }
            else
            {
                item["success"] = false;
            }
            list.Add(item);
            return ToContent(list);
        }

        public ActionResult GetBankInfoData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetBaseGroup();
            DataTable dt = ModelFactory.InquiryData("*", "SMBKINFO", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        /// 货况位置
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLocationTypeData()
        {
            return GetBsGroupCodeData("CD_TYPE='TKLC'");
        }

        public ActionResult GetChgData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetBaseGroup();
            DataTable dt = ModelFactory.InquiryData("*", "SMCHG", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        /// 省份
        /// </summary>
        /// <returns></returns>
        public ActionResult GetProvince()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetBaseCmp();
            DataTable dt = ModelFactory.InquiryData("*", "BSSTATE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public ActionResult GetTALNData()
        {
            return GetBsGroupCodeData("CD_TYPE='TALN'");
        }

        public ActionResult GetCostCenter()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetBaseCmp();
            DataTable dt = ModelFactory.InquiryData("*", "SMCC", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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
        #endregion.

        /*產生colmodel*/
        public ActionResult genColModel()
        {

            string table = Request.Params["table"];
            string gridIndKey = Request.Params["gridIndKey"];
            string name = Request.Params["name"];
            string prefix = Request.Params["prefix"];
            string keyValue = Request.Params["keys"];

            if (table != null)
                table = HttpUtility.UrlDecode(table);
            if (name != null)
                name = HttpUtility.UrlDecode(name);

            table = GetDecodeBase64ToString(table);
            name = GetDecodeBase64ToString(name);

            List<string> keys = null;
            keys = new List<string> { };
            string[] keyValues = keyValue.Split(',');
            foreach(string val in keyValues){
                keys.Add(val);
            }

            string sql = "";
            if (String.IsNullOrEmpty(gridIndKey))
            {
                sql = "SELECT * FROM " + table + " WHERE 1=0";
            }
            else
            {
                sql = "SELECT * FROM " + table + " WHERE 1=0";
            }


            HttpCookie MyLang = Request.Cookies["plv3.passport.lang"];
            CultureInfo cul = CultureInfo.CreateSpecificCulture(MyLang.Value.ToString());
            System.Resources.ResourceSet myResourceSet = Resources.Locale.ResourceManager.GetResourceSet(cul, true, true);
            SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, keys, prefix, myResourceSet));

            return ToContent(SchemasCache[name]);
        }
        public JsonResult ModifyExprid()
        {
            string Oripwd = Prolink.Math.GetValueAsString(Request.Params["Oripwd"]);
            string npwd = Prolink.Math.GetValueAsString(Request.Params["npwd"]);
            string cpwd = Prolink.Math.GetValueAsString(Request.Params["cpwd"]);
            string userid = Prolink.Math.GetValueAsString(Request.Params["userid"]);
            string cmp = Prolink.Math.GetValueAsString(Request.Params["cmp"]);
            string msg = string.Empty;
            if (Oripwd == "")
            {
                msg = @Resources.Locale.L_ModifyExpridPwd_Opwd;
            }
            if (npwd == "")
            {
                msg = @Resources.Locale.L_ModifyExpridPwd_Npwd;
            }
            if (cpwd == "")
            {
                msg = @Resources.Locale.L_ModifyExpridPwd_Cpwd;
            }
            if (npwd != cpwd)
            {
                msg = @Resources.Locale.L_ModifyExpridPwd_Npwdcompare;
            }
            else if (Oripwd == npwd)
            {
                msg = @Resources.Locale.L_ModifyExpridPwd_Opwdcompare;
            }

            if (!string.IsNullOrEmpty(msg))
            {
                return Json(new { message = msg });
            }

            string sql = "SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND Upper(U_ID)=Upper(" + SQLUtils.QuotedStr(userid) + ")" +
                " AND U_PASSWORD=" + SQLUtils.QuotedStr(genMD5(Oripwd)) + " AND CMP=" + SQLUtils.QuotedStr(cmp);
           DataTable result = Prolink.DataOperation.OperationUtils.GetDataTable(sql, new string[0], Prolink.Web.WebContext.GetInstance().GetConnection());
           if (result != null&& result.Rows.Count > 0)
           {
               MixedList ml = new MixedList();
               EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
               ei.PutKey("U_ID", userid);
               ei.Put("U_PASSWORD", genMD5(npwd));
               ei.Put("MODIFY_BY", userid);
               ei.PutDate("MODIFY_DATE", DateTime.Now);
               string IOFLAG = Prolink.Math.GetValueAsString(result.Rows[0]["IO_FLAG"]);
               string I_E = Prolink.Math.GetValueAsString(result.Rows[0]["I_E"]);
               if (IOFLAG == "O" || (IOFLAG == "I" && I_E == "Y"))
               {
                   //int date = Prolink.Math.GetValueAsInt(dt.Rows[0]["MODI_PW_DATE"]);
                   //date = date <= 0 ? 90 : date;
                   int date = 90;
                   DateTime d = DateTime.Now.AddDays(date);
                   ei.PutDate("UPDATE_PRI_DATE", d);
               }
               ml.Add(ei);
               try
               {
                   OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                   msg = "Y";
               }
               catch (Exception ex)
               {
                   msg = ex.Message;
               }
           }
           else
           {
               msg = @Resources.Locale.L_ModifyExpridPwd_Opwdwrong;
           }

            return Json(new { message = msg });
        }

        public ActionResult GetPartyNo1Data() {
            string condition = "PARTY_TYPE LIKE '%IBCR%' AND ";
            return GetPartyNoData(condition);
        }



        public ActionResult GetGridModel()
        {
            bool isok = false;
            string captionname = Prolink.Math.GetValueAsString(Request.Params["layoutid"]);
            ArrayList arraylist = new ArrayList();
            if (gridmodel.ContainsKey(captionname))
            {
                string tablename = gridmodel[captionname];
                string sql1 = string.Format(@"SELECT COLUMN_NAME,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,DATA_TYPE 
                FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME={0}", SQLUtils.QuotedStr(tablename));
                DataTable coldt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string columnListStr = Request["columnList"];
                if (!string.IsNullOrEmpty(columnListStr))
                    columnListStr = System.Web.HttpUtility.UrlDecode(columnListStr).Replace("null,", "");
                JavaScriptSerializer js = new JavaScriptSerializer();
                object[] items = js.DeserializeObject(columnListStr) as object[];
                foreach (Dictionary<string, object> item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    if (item.ContainsKey("formatter"))
                    {
                        string formatter = Prolink.Math.GetValueAsString(item["formatter"]);
                        if (formatter == "date" || formatter == "select")
                        {
                            continue;
                        }
                    }
                    if (item.ContainsKey("edittype") && "custom".Equals(Prolink.Math.GetValueAsString(item["edittype"])))
                    {
                        continue;
                    }
                    AddGridModelItem(arraylist, item, coldt);
                }
                if (arraylist.Count > 0)
                    isok = true;
            }
            return Json(new { isok = isok, collist = ObjectToJson(arraylist) });
        }

        static Dictionary<string, string> gridmodel = new Dictionary<string, string>();
        public static void SetGridModel()
        {
            gridmodel = new Dictionary<string, string>();
            Func<string, string, string> SetModel = (gridtitle, table) =>
            {
                if (!gridmodel.ContainsKey(gridtitle))
                {
                    gridmodel.Add(gridtitle, table);
                }
                return "";
            };

            SetModel("BSCODE Kind", "BSCODE_KIND");
            SetModel("BsCodeGrid", "BSCODE"); 
            SetModel(@Resources.Locale.L_BSCSSetup_DomAdressSetup, "SMPTYD");
            SetModel(@Resources.Locale.L_BSCSSetup_StsSet, "TKCSTS");
            SetModel(@Resources.Locale.L_BSCSSetup_SuspendSetup, "SMPTYS");
            SetModel(@Resources.Locale.L_BSCSSetup_VenderContract, "SMPTYC");
            SetModel(@Resources.Locale.L_BSTRUCKSetup_DriverInfo, "BSTRUCKD");
            SetModel(@Resources.Locale.L_BSTRUCKSetup_TruckInfo, "BSTRUCKC");
            SetModel(@Resources.Locale.L_CurrencySetup_CurSetup, "BSCUR"); 
            SetModel(@Resources.Locale.L_TruckPort_CommonAddress, "BSADDR"); 
            SetModel(@Resources.Locale.L_DNManage_InvoiceDetail, "SMIND");
            SetModel(@Resources.Locale.L_DNManage_PackingDetail, "SMINP");
            SetModel(@Resources.Locale.L_ApproveGroupSetup_ApproveMem, "APPROVE_ATTR_DP");
            SetModel(@Resources.Locale.L_ApproveGroupSetup_ApproveDep, "APPROVE_ATTR_D");
            SetModel(@Resources.Locale.L_ApproveGroupSetup_ApproveAttr, "APPROVE_ATTRIBUTE");
            SetModel("Approve Group", "APPROVE_FLOW_M");
            SetModel("Approve Flow", "APPROVE_FLOW_D"); 
            SetModel(@Resources.Locale.L_Sign_Type_SETUP, "APPROVE_SIGN");
            SetModel(@Resources.Locale.L_ExchangeRate_ExRateSetup, "BSERATE"); 
            SetModel("FCL", "SMQTD"); 
            SetModel(@Resources.Locale.L_DNManage_Move, "SMRVM"); 
            SetModel(@Resources.Locale.L_DNManage_GateList, "SMWHGT");
            SetModel(@Resources.Locale.L_ErrRelationSetup_ExpType, "BSCODE");   
            SetModel(@Resources.Locale.L_ErrRelationSetup_ExpReason, "EXPRELA"); 
            SetModel(@Resources.Locale.L_ExpressSetup_ExpressQuery, "SMEXM"); 
            SetModel(@Resources.Locale.L_IpPart_PartdGrid, "IPPARTD"); 
            SetModel("Party No Filter", "SMPTY_FILTER");
            SetModel("SCM Requested", "SCMREF");
            SetModel(@Resources.Locale.L_TKSetup_Scripts_369, "TKSTSCD"); 
            SetModel(@Resources.Locale.L_SMDNDSetup_Views_391, "SMDND");
              
        }

        private static void AddGridModelItem(ArrayList arraylist, Dictionary<string, object> itemmodel, DataTable dt)
        {
            string fieldName = Prolink.Math.GetValueAsString(itemmodel["name"]);
            string colName = ModelFactory.ReplaceFiledToDBName(fieldName);
            DataRow[] rows = dt.Select(string.Format("COLUMN_NAME={0}", SQLUtils.QuotedStr(colName)));
            if (rows == null || rows.Length <= 0)
                return;
            string datatype = Prolink.Math.GetValueAsString(rows[0]["DATA_TYPE"]);
            int strlen = Prolink.Math.GetValueAsInt(rows[0]["CHARACTER_MAXIMUM_LENGTH"]);
            int precisionlen = Prolink.Math.GetValueAsInt(rows[0]["NUMERIC_PRECISION"]);
            int scalelen = Prolink.Math.GetValueAsInt(rows[0]["NUMERIC_SCALE"]);
            Dictionary<string, object> item = new Dictionary<string, object>();
            switch (datatype)
            {
                case "nvarchar":
                case "bit":
                case "varchar":
                case "nchar":
                    item["name"] = fieldName;
                    item["editoptions"] = new Dictionary<string, object> { { "size", strlen }, { "maxlength", strlen } };
                    break;
                case "bigint":
                case "int":
                case "smallint":
                case "numeric":
                case "decimal":
                    item["name"] = fieldName;
                    Dictionary<string, object> formatoptions = new Dictionary<string, object>();
                    if (scalelen > 0)
                    {
                        formatoptions.Add("decimalSeparator", ".");
                        formatoptions.Add("decimalPlaces", scalelen);
                        precisionlen = precisionlen + scalelen + 1;
                    }
                    item["formatoptions"] = formatoptions;
                    item["editoptions"] = new Dictionary<string, object> { { "size", precisionlen }, { "maxlength", precisionlen } };
                    break;
                case "date":
                case "datetime":
                case "datetime2":
                case "datetimeoffset":
                case "ntext":
                case "smalldatetime":
                case "text":
                case "uniqueidentifier": break;
            }
            if (item.Count > 0)
                arraylist.Add(item);
        }

    }
}
