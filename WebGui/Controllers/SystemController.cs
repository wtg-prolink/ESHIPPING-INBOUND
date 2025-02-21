using Business.Service;
using Business.TPV.Import;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Model;
using Prolink.V3;
using Prolink.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using WebGui.App_Start;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using System.Text;
using System.IO;
using System.Web.Configuration;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Business;
using Models.EDI;

namespace WebGui.Controllers
{
    public class SystemController : BaseController
    {
        //
        // GET: /System/
        private static System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Resources.Locale", global::System.Reflection.Assembly.Load("App_GlobalResources"));
        private static Dictionary<string, Dictionary<string, string>> _pmsList = null;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IPLog()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult UserQuery()
        {
            ViewBag.MenuBar = false;
           // ViewBag.aaa = sayHelloMethod.Invoke(myObject, null);
            ViewBag.pmsList = GetBtnPms("USERSETUP");
            return View();
        }

        public ActionResult UserQueryO()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("OUSERSETUP");
            return View();
        }

        public ActionResult ModifyPwd()
        {
            ViewBag.pmsList = GetBtnPms("MODIFYPWD");
            return View();
        }

        public ActionResult UserSetUp(string UId = null, string GroupId = null, string Cmp = null, string Stn = null)
        {
            string sql = "SELECT * FROM SYS_ACCT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID", "GROUP_ID", "CMP", "STN" });
            sql = "SELECT CMP, NAME FROM SYS_SITE WHERE GROUP_ID='TPV' AND TYPE='1'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT CD,CD_DESCP from BSCODE where CD_TYPE='TCT' AND " + GetBaseCmp();
            DataTable trantypedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT CD,CD_DESCP from BSCODE where CD_TYPE='PLT' AND " + GetBaseCmp();
            DataTable plDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            ViewBag.TranTypeData = Json(new { result = ModelFactory.ToTableJson(trantypedt) });
            ViewBag.CmpPriData = Json(new { result = ModelFactory.ToTableJson(dt) });
            ViewBag.PlantPriData = Json(new { result = ModelFactory.ToTableJson(plDt) });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = UId;
            ViewBag.GroupId = GroupId;
            ViewBag.Cmp = Cmp;
            ViewBag.Stn = Stn;
            ViewBag.pmsList = GetBtnPms("USERSETUP");
            ViewBag.useRole = getRolesByCmp();
            ViewBag.userRole = getUserRole(UId, GroupId, Cmp);
            return View();
        }
        public ActionResult UserSetUpO(string UId = null, string GroupId = null, string Cmp = null, string Stn = null)
        {
            string sql = "SELECT * FROM SYS_ACCT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID", "GROUP_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = UId;
            ViewBag.GroupId = GroupId;
            ViewBag.Cmp = Cmp;
            ViewBag.Stn = Stn;
            ViewBag.pmsList = GetBtnPms("OUSERSETUP");
            ViewBag.useRole = getRolesByCmp();
            ViewBag.userRole = getUserRole(UId, GroupId, Cmp);
            return View();
        }
        public ActionResult UserOQueryData()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string resultType = Request.Params["resultType"];
            string condition = GetCreateDateCondition("SYS_ACCT", "");
            string table = @"(SELECT SYS_ACCT.*,STUFF((  SELECT ','+FDESCP  FROM SYS_ROLE WHERE GROUP_ID=SYS_ACCT.GROUP_ID AND CMP=SYS_ACCT.BASE_CMP AND FID 
                IN(SELECT FROLE_ID FROM SYS_ACCT_ROLE WHERE FACCT_ID=SYS_ACCT.U_ID AND GROUP_ID=SYS_ACCT.GROUP_ID AND CMP=SYS_ACCT.CMP)   FOR XML PATH('')),1,1,'') AS ROLE_ASSIGN
                FROM SYS_ACCT)SYS_ACCT";
            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult UserQueryData()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string resultType = Request.Params["resultType"];
            string condition = GetUpriCondition();
            condition = GetCreateDateCondition("SYS_ACCT", condition);

            string table = string.Format(@"(SELECT SYS_ACCT.*,STUFF((  SELECT ','+FDESCP  FROM SYS_ROLE WHERE GROUP_ID=SYS_ACCT.GROUP_ID AND CMP=SYS_ACCT.CMP AND FID IN(SELECT FROLE_ID FROM SYS_ACCT_ROLE WHERE FACCT_ID=SYS_ACCT.U_ID AND GROUP_ID=SYS_ACCT.GROUP_ID AND CMP=SYS_ACCT.CMP)   FOR XML PATH('')),1,1,'') AS ROLE_ASSIGN FROM SYS_ACCT)SYS_ACCT");

            DataTable dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult UserDetailQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string conditions = "";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string GroupId = Prolink.Math.GetValueAsString(Request.Params["GroupId"]);
            string Cmp = Prolink.Math.GetValueAsString(Request.Params["Cmp"]);
            string Stn = Prolink.Math.GetValueAsString(Request.Params["Stn"]);
            if (UId != "")
            {
                conditions = " WHERE U_ID=" + SQLUtils.QuotedStr(UId) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId)
                    + "AND CMP=" + SQLUtils.QuotedStr(Cmp);
            }
            DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", conditions, "", pageIndex, pageSize, ref recordsCount);

            string sql1 = string.Format("SELECT * FROM SYS_ACCT_LOG WHERE USER_ID ={0} AND GROUP_ID={1} AND CMP={2} ORDER BY MODIFY_DATE DESC", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp));
            DataTable subDt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow dr in dt.Rows)
            {
                string ioflag = Prolink.Math.GetValueAsString(dr["IO_FLAG"]);
                if ("O".Equals(ioflag))
                { 
                    Cmp = Prolink.Math.GetValueAsString(dr["BASE_CMP"]);
                }
                if (!Cmp.Equals(CompanyId))
                {
                    dr["MD5_INFO"] = "N";
                }
                else
                {
                    dr["MD5_INFO"] = "Y";
                }
            }
            
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SysAcctModel"),
                records = recordsCount,
                page = pageIndex,
                total = dt.Rows.Count
            };
            if (UId != "")
            {
                conditions = " WHERE USER_ID=" + SQLUtils.QuotedStr(UId) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId)
                    + "AND U_CMP=" + SQLUtils.QuotedStr(Cmp);
            }
            DataTable whDt = ModelFactory.InquiryData("*", "SYS_ACCT_WH", conditions, "", pageIndex, pageSize, ref recordsCount);
            BootstrapResult whResult = null;
            whResult = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(whDt, "SysAcctWhModel"),
                records = recordsCount,
                page = pageIndex,
                total = dt.Rows.Count
            };
             
            BootstrapResult LOGResult = null;
            LOGResult = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(subDt, "SysAcctLogModel"),
                records = recordsCount,
                page = pageIndex,
                total = dt.Rows.Count
            };
            return Json(new { mainTable = result.ToContent(), whData = whResult.ToContent(), LogData = LOGResult.ToContent() });
        }
        public string getRolesByCmp()
        {
            string value = "";
            string sql = string.Format("SELECT * FROM SYS_ROLE WHERE GROUP_ID={0} AND CMP={1}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string fid = Prolink.Math.GetValueAsString(dr["FID"]);
                string stn = Prolink.Math.GetValueAsString(dr["STN"]);
                string fdescp = Prolink.Math.GetValueAsString(dr["FDESCP"]);
                if (!string.IsNullOrEmpty(value))
                    value += ";";
                value += GroupId + "_" + CompanyId + "_" + stn + "_" + fid + ":" + fdescp;
            }
            return value;
        }

        public string getUserRole(string UId = "", string GroupId = "", string Cmp = "")
        {
            string value = "";
            string sql = string.Format("SELECT FROLE_ID,GROUP_ID,RCMP,RSTN FROM SYS_ACCT_ROLE WHERE FACCT_ID={0} AND GROUP_ID={1} AND CMP={2}",
                SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string fid = Prolink.Math.GetValueAsString(dr["FROLE_ID"]);
                string rGroupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                string rCmp = Prolink.Math.GetValueAsString(dr["RCMP"]);
                string rStn = Prolink.Math.GetValueAsString(dr["RSTN"]);
                if (!string.IsNullOrEmpty(value))
                    value += ";";
                value += rGroupId + "_" + rCmp + "_" + rStn + "_" + fid;
            }
            return value;
        }

        #region 客户建档
        public ActionResult BSCSDataQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BSCSSETUP");
            return View();
        }

        public ActionResult BSCSSetup(string UId = null)
        {
            string sql = "SELECT * FROM SMPTY WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.UId = UId;
            ViewBag.pmsList = GetBtnPms("BSCSSETUP");
            return View();
        }
        public ActionResult BSCSQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string resultType = Request.Params["resultType"];
            string condition = GetBaseGroup();
            condition = GetCreateDateCondition("SMPTY", condition);
            DataTable dt = ModelFactory.InquiryData("*", "SMPTY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptyModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult BSCSDetailQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMPTY", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptyModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            string baseCondition = "";
            string conditions = "";
            string ufid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            if (ufid != "")
            {
                baseCondition = string.Format("U_FID={0}", SQLUtils.QuotedStr(ufid));
                conditions = " WHERE U_FID=" + SQLUtils.QuotedStr(ufid);
            }


            //DataTable dtSmptyc = ModelFactory.InquiryData("*", "SMPTYC", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dtSmptyc = ModelFactory.InquiryData("*", "SMPTYC", conditions, "U_FID", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultSmptyc = null;
            resultSmptyc = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dtSmptyc),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            DataTable dtSmptys = ModelFactory.InquiryData("*", "SMPTYS", conditions, "U_FID", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultSmptys = null;
            resultSmptys = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dtSmptys),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            DataTable dtSmptyd = ModelFactory.InquiryData("*", "SMPTYD", conditions, "U_FID", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultSmptyd = null;
            resultSmptyd = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dtSmptyd),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            DataTable dtTkcsts = ModelFactory.InquiryData("*", "TKCSTS", conditions, "U_FID", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultTkcsts = null;
            resultTkcsts = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dtTkcsts),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            DataTable dtSmbkInfo = ModelFactory.InquiryData("*", "SMBKINFO", conditions, "U_FID", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultSmbkInfo = null;
            resultSmbkInfo = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dtSmbkInfo),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };

            return Json(new {
                mainTable = result.ToContent(),
                resultSmptyc = resultSmptyc.ToContent(),
                resultSmptys = resultSmptys.ToContent(),
                resultSmptyd = resultSmptyd.ToContent(), 
                resultTkcsts = resultTkcsts.ToContent(),
                resultSmbkInfo = resultSmbkInfo.ToContent()
            });
        }

        public ActionResult SmptycQuery(string UFid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string baseCondition = "U_FID='" + UFid + "'";
            DataTable dt = ModelFactory.InquiryData("*", "SMPTYC", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptycModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult SmptysQuery(string UFid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string baseCondition = "U_FID='" + UFid + "'";
            DataTable dt = ModelFactory.InquiryData("*", "SMPTYS", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptysModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult SmptydQuery(string UFid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string baseCondition = "U_FID='" + UFid + "'";
            DataTable dt = ModelFactory.InquiryData("*", "SMPTYD", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptydModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult TkcstsQuery(string UFid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string baseCondition = "U_FID='" + UFid + "'";
            DataTable dt = ModelFactory.InquiryData("*", "TKCSTS", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "TkcstsModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult SmbkinfoQuery(string UFid = null)
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string baseCondition = "U_FID='" + UFid + "'";
            DataTable dt = ModelFactory.InquiryData("*", "SMBKINFO", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmbkinfoModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        [ValidateInput(false)]
        public ActionResult BSCSSUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string returnMessage = "success";
            string u_id = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string partyNo = Prolink.Math.GetValueAsString(Request.Params["PartyNo"]);
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            List<Dictionary<string, object>> smptyData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> smptycData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> smptydData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> smptysData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> tkcstsData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> smbinfoData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> PartyNo = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            string sql = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmptyModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_id = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_id = System.Guid.NewGuid().ToString("N");
                            ei.Put("U_ID", u_id);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            int recordsCount1 = 0, pageIndex1 = 1, pageSize1 = 20;
                            string condtions = GetBaseGroup() + " AND PARTY_NO=" + SQLUtils.QuotedStr(partyNo);
                            DataTable partyNodt = ModelFactory.InquiryData("PARTY_NO", "SMPTY", condtions, "", pageIndex1, pageSize1, ref recordsCount1);
                            PartyNo = ModelFactory.ToTableJson(partyNodt, "SmptyModel");
                            if (PartyNo.Count > 0)
                            {
                                return Json(new { message = @Resources.Locale.L_SystemController_Controllers_211, PartyNo = PartyNo });
                            }

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            EditInstruct ei2 = new EditInstruct("SMPTYC", EditInstruct.DELETE_OPERATION);
                            EditInstruct ei3 = new EditInstruct("SMPTYS", EditInstruct.DELETE_OPERATION);
                            EditInstruct ei4 = new EditInstruct("SMPTYD", EditInstruct.DELETE_OPERATION);
                            EditInstruct ei5 = new EditInstruct("TKCSTS", EditInstruct.DELETE_OPERATION);
                            if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }
                            string UId = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            ei2.PutKey("U_FID", UId);
                            ei3.PutKey("U_FID", UId);
                            ei4.PutKey("U_FID", UId);
                            ei5.PutKey("U_FID", UId);
                            mixList.Add(ei2);
                            mixList.Add(ei3);
                            mixList.Add(ei4);
                            mixList.Add(ei5);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "ct")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmptycModel");
                    var u_fcid = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_fcid = ei.Get("U_FID");
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (u_id == null || u_fcid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fcid);
                            //ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_fcid = System.Guid.NewGuid().ToString("N");
                            if (u_id == null || u_fcid == null)
                            {
                                continue;
                            }
                            ei.PutKey("U_ID", u_fcid);
                            ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (u_id == null || u_fcid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fcid);
                            //ei.PutKey("U_FID", u_id);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmptysModel");
                    var u_fsid = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_fsid = ei.Get("U_FID");
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (u_id == null || u_fsid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fsid);
                            //ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_fsid = System.Guid.NewGuid().ToString("N");
                            if (u_id == null || u_fsid == null)
                            {
                                continue;
                            }
                            ei.PutKey("U_ID", u_fsid);
                            ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (u_id == null || u_fsid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fsid);
                            //ei.PutKey("U_FID", u_id);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "dt")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmptydModel");
                    var u_fdid = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_fdid = ei.Get("U_FID");
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fdid);
                            //ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_fdid = System.Guid.NewGuid().ToString("N");
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            ei.PutKey("U_ID", u_fdid);
                            ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fdid);
                            //ei.PutKey("U_FID", u_id);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "tk")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TkcstsModel");
                    var u_fdid = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_fdid = ei.Get("U_FID");
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            //ei.PutKey("U_ID", u_fdid);
                            //ei.PutKey("U_FID", u_id);
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_fdid = System.Guid.NewGuid().ToString("N");
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            ei.PutKey("U_ID", u_fdid);
                            ei.PutKey("U_FID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("U_ID", u_fdid);
                            //ei.PutKey("U_FID", u_id);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "bk")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmbkinfoModel");
                    var u_fdid = string.Empty;
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        u_fdid = ei.Get("U_FID");
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            u_fdid = System.Guid.NewGuid().ToString("N");
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            ei.PutKey("U_ID", u_fdid);
                            ei.PutKey("U_FID", u_id);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (u_id == null || u_fdid == null)
                            {
                                continue;
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", TimeZoneHelper.GetTimeZoneDate(CompanyId));
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    DataTable dt = ModelFactory.InquiryData("*", "SMPTY", "U_ID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    smptyData = ModelFactory.ToTableJson(dt, "SmptyModel");
                    dt = ModelFactory.InquiryData("*", "SMPTYC", "U_FID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    smptycData = ModelFactory.ToTableJson(dt, "SmptycModel");
                    dt = ModelFactory.InquiryData("*", "SMPTYS", "U_FID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    smptysData = ModelFactory.ToTableJson(dt, "SmptysModel");
                    dt = ModelFactory.InquiryData("*", "SMPTYD", "U_FID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    smptydData = ModelFactory.ToTableJson(dt, "SmptydModel");
                    dt = ModelFactory.InquiryData("*", "TKCSTS", "U_FID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    tkcstsData = ModelFactory.ToTableJson(dt, "TkcstsModel");
                    dt = ModelFactory.InquiryData("*", "SMBKINFO", "U_FID='" + u_id + "'", "", pageIndex, pageSize, ref recordsCount);
                    smbinfoData = ModelFactory.ToTableJson(dt, "SmbkinfoModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, smptyData = smptyData, smptycData = smptycData, smptysData = smptysData, smptydData = smptydData, tkcstsData = tkcstsData, smbinfoData = smbinfoData, UId = u_id });
        }

        #endregion

        #region 同步SAP时过滤筛选条件
        public ActionResult PartyFilterSetup()
        {
            ViewBag.pmsList = GetBtnPms("BSCSSETUP");
            return View();
        }

        public ActionResult SmptyFilterQueryData()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMPTY_FILTER", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SmptyFilterModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult SmptyFilterUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmptyFilterModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
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
                }
            }
            return Json(new { message = returnMessage });
        }

        #endregion

        public ActionResult BSTQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BSTSETUP");
            return View();
        }

        public ActionResult BSTSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM SMSIM WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> mtSchemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.mtSchemas = ToContent(mtSchemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BSTSETUP");
            return View();
        }

        public ActionResult BSTDataSetup(string id = null, string uid = null)
        {
            //string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            //string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);

            if (id != null)
            {
                uid = id;
            }
            string profile = Prolink.Math.GetValueAsString(Request.Params["Profile"]);
            if (!string.IsNullOrEmpty(profile))
            {
                uid = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 U_ID FROM SMSIM WHERE PROFILE={0}", SQLUtils.QuotedStr(profile)), Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            string sql = "SELECT * FROM SMSIM WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> stSchemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.stSchemas = ToContent(stSchemas);
            //ViewBag.UFid = UFid;
            ViewBag.UId = uid;
            ViewBag.pmsList = GetBtnPms("BSTSETUP");
            return View();
        }


        public ActionResult RoleSetUp()
        {
            ViewBag.pmsList = GetBtnPms("ROLE");
            return View();
        }

        public ActionResult GroupRelation()
        {
            //获取集团
            ViewBag.groupData = GetSysSiteInfo("0");
            return View();
        }

        public ActionResult UserPermission()
        {
            ViewBag.Role = Roles();
            ViewBag.Users = Users();
            ViewBag.Menu = GetAllPermission();
            ViewBag.pmsList = GetBtnPms("SYSPERMIS");
            return View();
        }

        private ActionResult Users()
        {
            //if login user was * cmp or * stn , can get cross cmp or stn users
            string sql = "SELECT U_ID,U_NAME,GROUP_ID,CMP,STN,U_STATUS FROM SYS_ACCT WHERE IO_FLAG = 'I'  AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);

            if (CompanyId != "*")
            {
                sql += "AND (CMP=" + SQLUtils.QuotedStr(CompanyId) + " OR CMP = '*') ";
            }
            if (Station != "*")
            {
                sql += "AND (STN=" + SQLUtils.QuotedStr(Station) + " OR STN = '*')";
            }

            sql += "UNION ALL SELECT U_ID,U_NAME,GROUP_ID,CMP,STN,U_STATUS FROM SYS_ACCT WHERE IO_FLAG = 'O' AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            foreach (DataRow dr in dt.Rows)
            {
                item = new Dictionary<string, object>();
                item["UserID"] = Prolink.Math.GetValueAsString(dr["U_ID"]);
                item["UserName"] = Prolink.Math.GetValueAsString(dr["U_NAME"]);
                item["GroupId"] = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                item["Cmp"] = Prolink.Math.GetValueAsString(dr["CMP"]);
                item["Stn"] = Prolink.Math.GetValueAsString(dr["STN"]);
                item["UStatus"] = Prolink.Math.GetValueAsString(dr["U_STATUS"]);
                list.Add(item);
            }
            return ToContent(list);
        }

        public ActionResult IpGoods()
        {
            return View();
        }

        public ActionResult TKSetup()
        {
            return View();
        }

        public ActionResult CurrencySetup()
        {
            ViewBag.pmsList = GetBtnPms("BSB010SETUP");
            return View();
        }

        #region 国家代码建档
        public ActionResult CntySetup(string id = null)
        {
            ViewBag.pmsList = GetBtnPms("CntySetup");
            SetSchema("CntySetup");
            ViewBag.pmsList = GetBtnPms("CntySetup");
            ViewBag.CntryCd = Request["CntryCd"];
            ViewBag.GroupId = Request["gid"];
            ViewBag.Cmp = Request["cid"];
            ViewBag.Stn = Request["sid"];
            return View();
        }

        public ActionResult CntyQueryView()
        {
            ViewBag.pmsList = GetBtnPms("CntySetup");
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult CntyQueryData()
        {
            string condition = GetBaseCmp();
            condition = GetCreateDateCondition("BSCNTY", condition);
            return GetBootstrapData("BSCNTY", condition);
        }

        public ActionResult CntyGetDetail()
        {
            string gid = Request["gid"];
            string cid = Request["cid"];
            string sid = Request["sid"];
            string CntryCd = Request["uId"];
            string sql = string.Format("SELECT * FROM BSCNTY WHERE CNTRY_CD={0} AND CMP={1}", SQLUtils.QuotedStr(CntryCd), SQLUtils.QuotedStr(cid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "CntyModel");
            return ToContent(data);
        }
        #endregion



        #region 異常關係

        public ActionResult ErrRelationSetup()
        {
            
            ViewBag.PmsList = GetBtnPms("EXPRELA");
            return View();
        }

        public ActionResult GetData()
        {
            string u_id = Request["UId"];
            
            string sql = "SELECT CD_TYPE,CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='ASR'";
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            Dictionary<string, object> data = new Dictionary<string, object>();

            return ToContent(ModelFactory.ToTableJson(mainDt, "ExprelaModel"));
        }
        

        public ActionResult GetErrTypeData()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='AST' AND (CMP={0}  OR CMP='*')", SQLUtils.QuotedStr(CompanyId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


            Dictionary<string, object> data = new Dictionary<string, object>();
           
            return ToContent(ModelFactory.ToTableJson(mainDt, "BsCodeModel"));
        }


        public JsonResult ErrReasonRequiry()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            string cd = Prolink.Math.GetValueAsString(Request.Params["Cd"]);
            //string condtions = "LEFT JOIN EXPRELA ON EXPRELA.EXP_TYPE = BSCODE.CD WHERE BSCODE.CD_TYPE='AST' AND BSCODE.CD=" + SQLUtils.QuotedStr(cd);
            //condtions += " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            //DataTable detailDt = ModelFactory.InquiryData("*", "BSCODE", condtions, " CD ASC", pageIndex, pageSize, ref recordsCount);
            string sql = "SELECT * FROM BSCODE LEFT JOIN EXPRELA ON EXPRELA.EXP_TYPE = BSCODE.CD WHERE BSCODE.CD_TYPE='AST' AND BSCODE.CD=" + SQLUtils.QuotedStr(cd) + " AND ( BSCODE.CMP=" + SQLUtils.QuotedStr(CompanyId) + " OR CMP='*')";
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(mainDt)
            };
            return Json(new { mainTable = resultDetail.ToContent() });
        }


        public JsonResult ErrRelaUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            bool rebuildPms = false;
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                
                if (item.Key == "Errinfo")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ExprelaModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("EXP_TYPE", ei.Get("EXP_TYPE"));
                            ei.Put("EXP_REASON", ei.Get("EXP_REASON"));
                            ei.Put("EXP_DESCP", ei.Get("EXP_DESCP"));
                            ei.Put("IS_RELIEVE", ei.Get("IS_RELIEVE"));
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("U_ID", ei.Get("U_ID"));
                            //ei.PutKey("EXP_TYPE", ei.Get("EXP_TYPE"));
                            //ei.PutKey("EXP_REASON", ei.Get("EXP_REASON"));
                            //ei.PutKey("EXP_DESCP", ei.Get("EXP_DESCP"));
                            
                        }
                        //if (ei.Get("EXP_TYPE") != "")
                        //{
                            
                        //}
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (rebuildPms)
                    {
                        //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                        //Prolink.V3.PermissionManager.SetRolePermission(roleList);
                        //Business.CommonHelp.NotifyRebuidPermission(roleList);
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }


        

        #endregion

        #region 汇率建档
        public ActionResult ExchangeRate()
        {
            string sql = "SELECT * FROM BSERATE WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "ETYPE", "EDATE", "FCUR", "TCUR" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.pmsList = GetBtnPms("ExchangeRateSetup");
            return View();
        }

        public ActionResult syncExchageRate()
        {
            string sap_id = Business.TPV.Helper.GetSapId(CompanyId);
            Business.TPV.Import.ExchangeRateManager m = new Business.TPV.Import.ExchangeRateManager();
            var v = m.Import(sap_id, CompanyId, Business.TPV.RFC.ExchangeRateTypes.Standard);
            v = m.Import(sap_id, CompanyId, Business.TPV.RFC.ExchangeRateTypes.EndOfTheMonth);
            string sql = "SELECT * FROM BSERATE WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return ToContent(ModelFactory.ToTableJson(mainDt, "ExchangeRateModel"));
        }
        #endregion

        #region City
        public ActionResult CityDataQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("CitySetup");
            return View();
        }
        public ActionResult CitySetup(string PortCd = null, string CntryCd = null)
        {
            string sql = "SELECT * FROM BSCITY WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "CNTRY_CD", "PORT_CD" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.PortCd = PortCd;
            ViewBag.CntryCd = CntryCd;
            ViewBag.pmsList = GetBtnPms("CitySetup");
            return View();
        }
        #endregion

        #region 郵件格式
        public ActionResult MailFormatDataQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("TKPMT");
            return View();
        }

        public ActionResult MailFormatSetup()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT * FROM TKPMT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.pmsList = GetBtnPms("TKPMT");
            ViewBag.UId = UId;

            if (UId != "")
            {
                sql = "SELECT * FROM TKPMT WHERE U_ID = " + SQLUtils.QuotedStr(UId);
                DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        string MtContent = Prolink.Math.GetValueAsString(dt.Rows[0]["MT_CONTENT"]);
                        ViewBag.MtType = Prolink.Math.GetValueAsString(dt.Rows[0]["MT_TYPE"]);
                        ViewBag.MtName = Prolink.Math.GetValueAsString(dt.Rows[0]["MT_NAME"]);
                        ViewBag.Cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
                        ViewBag.MtContent = Prolink.Math.GetValueAsString(HttpUtility.HtmlDecode(MtContent));
                        ViewBag.CreateBy = Prolink.Math.GetValueAsString(dt.Rows[0]["CREATE_BY"]);
                        ViewBag.CreateDate = dt.Rows[0]["CREATE_DATE"] == System.DBNull.Value ? "" : Prolink.Math.GetValueAsDateTime(dt.Rows[0]["CREATE_DATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                        ViewBag.ModifyBy = Prolink.Math.GetValueAsString(dt.Rows[0]["MODIFY_BY"]);
                        ViewBag.ModifyDate = dt.Rows[0]["MODIFY_DATE"] == System.DBNull.Value ? "" : Prolink.Math.GetValueAsDateTime(dt.Rows[0]["MODIFY_DATE"]).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            }
            return View();
        }

        public ActionResult MailFormatData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            string condition = GetBaseCmp();
            condition = GetCreateDateCondition("TKPMT", condition);
            DataTable dt = ModelFactory.InquiryData("*", "TKPMT", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        public ActionResult MailFormatDetailData()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "";
            List<Dictionary<string, object>> MailData = new List<Dictionary<string, object>>();
            sql = "SELECT * FROM TKPMT WHERE U_ID = " + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string MtContent = Prolink.Math.GetValueAsString(dt.Rows[0]["MT_CONTENT"]);
                    dt.Rows[0]["MT_CONTENT"] = HttpUtility.HtmlDecode(MtContent);
                }
            }

            MailData = ModelFactory.ToTableJson(dt, "TKPMTModel");
            return Json(new { mainTable = MailData });
        }

        [ValidateInput(false)]
        public JsonResult MailFormatUpdateData()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string MtType = Prolink.Math.GetValueAsString(Request.Params["MtType"]);
            string MtName = Prolink.Math.GetValueAsString(Request.Params["MtName"]);
            string MtContent = Prolink.Math.GetValueAsString(Request.Params["MtContent"]);
            string returnMessage = "success";
            string editMode = Prolink.Math.GetValueAsString(Request.Params["editMode"]);
            string sql = "";

            MtContent = HttpUtility.HtmlEncode(MtContent);
            if (editMode == "A")
            {
                uid = Guid.NewGuid().ToString();
                sql = "INSERT INTO TKPMT (U_ID, GROUP_ID, CMP, STN, DEP, MT_TYPE, MT_NAME,MT_CONTENT,REMARK,CREATE_BY,CREATE_DATE) VALUES (" + SQLUtils.QuotedStr(uid) + ", " + SQLUtils.QuotedStr(GroupId) + ", " + SQLUtils.QuotedStr(CompanyId) + ", " + SQLUtils.QuotedStr(Station) + "," + SQLUtils.QuotedStr(Dep) + ", " + SQLUtils.QuotedStr(MtType) + ", " + SQLUtils.QuotedStr(MtName) + ", " + SQLUtils.QuotedStr(MtContent) + ", '',"+SQLUtils.QuotedStr(UserId)+","+SQLUtils.QuotedStr(DateTime.Now.ToString())+")";
            }
            else if (editMode == "E")
            {
                sql = "UPDATE TKPMT SET MT_TYPE=" + SQLUtils.QuotedStr(MtType) + ", MT_NAME=" + SQLUtils.QuotedStr(MtName) + ", MT_CONTENT=" + SQLUtils.QuotedStr(MtContent) + ", MODIFY_BY=" + SQLUtils.QuotedStr(UserId) + ", MODIFY_DATE=" + SQLUtils.QuotedStr(DateTime.Now.ToString()) + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            }
            else if (editMode == "D")
            {
                sql = "DELETE FROM TKPMT WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            }


            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }

            return Json(new { message = returnMessage, UId = uid });
        }
        #endregion

        #region 卡車建檔
        /// <summary>
        /// 卡車建檔查询
        /// </summary>
        /// <returns></returns>
        public ActionResult TruckPortQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("TruckPortQuery");
            return View();
        }
        public ActionResult TruckPortSetup(string PortCd = null, string CntryCd = null,string Cmp=null)
        {
            string sql = "SELECT * FROM BSTPORT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "PORT_CD", "CNTRY_CD" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.PortCd = PortCd;
            ViewBag.CntryCd = CntryCd;
            ViewBag.Cmp = Cmp;
            ViewBag.pmsList = GetBtnPms("TruckPortQuery");
            return View();
        }

        /// <summary>
        /// 货况代码查询
        /// </summary>
        /// <returns></returns>
        public ActionResult TKSetupQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string resultType = Request.Params["resultType"];
            string condition = GetCreateDateCondition("TKSTSCD", "");
            DataTable dt = ModelFactory.InquiryData("*","TKSTSCD", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "TkStscdModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult TKSetupUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string StsCd = Prolink.Math.GetValueAsString(Request.Params["StsCd"]);
            List<Dictionary<string, object>> ipstsData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TkStscdModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("STS_CD", ei.Get("STS_CD"));
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql = string.Format("SELECT * FROM TKSTSCD WHERE STS_CD={0}", SQLUtils.QuotedStr(StsCd));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ipstsData = ModelFactory.ToTableJson(mainDt, "TkStscdModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = ipstsData });
        }
        #endregion

        #region 目的地库位建档
        public ActionResult DestinationQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("DestinationQuery");
            return View();
        }

        public ActionResult DestinationSetup(string PortCd = null, string CntryCd = null, string Factory = null, string ShipTo = null, string Cmp = null)
        {
            string sql = "SELECT * FROM BSDEST WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "PORT_CD", "CNTRY_CD","FACTORY","SHIP_TO" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.PortCd = PortCd;
            ViewBag.CntryCd = CntryCd;
            ViewBag.Factory = Factory;
            ViewBag.ShipTo = ShipTo;
            ViewBag.Cmp = Cmp;            
            ViewBag.pmsList = GetBtnPms("DestinationQuery");
            return View();
        }

        public ActionResult DestinationSetupInquiryData()
        {

            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string condition = GetBookingCondition();
            condition = GetCreateDateCondition("BSDEST", condition);
            DataTable dt = ModelFactory.InquiryData("*", "BSDEST", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "BsDestModel")
            };

            return result.ToContent();
        }

        public ActionResult DestinationSetupQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("BsDestModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BsDestModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult DestAddrQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("DestAddrModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "DestAddrModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return Json(new { subTable = result.ToContent() });
        }

        public ActionResult DestinationSetupUpdate()
        {
            string changeData = Request.Params["changedData"];
            string portCd = Request.Params["portCd"];
            string cntryCd = Request.Params["cntryCd"];
            string factory = Request.Params["factory"];
            string shipTo = Request.Params["shipTo"];
            string Cmp = Request.Params["cmp"];
            string uid = string.Empty;
            string returnMessage = "success";
            string sql = string.Empty;
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Dictionary<string, object>> mainTable = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> subTable = new List<Dictionary<string, object>>();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            MixedList mixList2 = new MixedList();

            
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsDestModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];


                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            sql = string.Format("SELECT * FROM BSDEST WHERE CNTRY_CD={0} AND PORT_CD={1} AND FACTORY={2} AND SHIP_TO={3} AND CMP={4}",
                                SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo), SQLUtils.QuotedStr(Cmp));
                            DataTable dt = getDataTableFromSql(sql);
                            if (dt.Rows.Count > 0)
                            {
                                return Json(new { message = @Resources.Locale.L_SMFSC_Controllers_461 });
                            }
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("FACTORY", ei.Get("FACTORY"));
                            ei.PutKey("SHIP_TO", ei.Get("SHIP_TO"));
                            ei.PutKey("CMP", Cmp);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("FACTORY", ei.Get("FACTORY"));
                            ei.PutKey("SHIP_TO", ei.Get("SHIP_TO"));
                            ei.PutKey("CMP", Cmp);
                            string Sql = string.Format("DELETE FROM DEST_ADDR WHERE CNTRY_CD = {0} AND PORT_CD = {1} AND FACTORY={2} AND SHIP_TO={3} AND CMP={4}",
                                SQLUtils.QuotedStr(ei.Get("CNTRY_CD")), SQLUtils.QuotedStr(ei.Get("PORT_CD")), SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo), SQLUtils.QuotedStr(Cmp));
                            mixList.Add(Sql);
                        }
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("PORT_CD") != "" && ei.Get("FACTORY") != "" && ei.Get("SHIP_TO") != "")
                        {
                            mixList.Add(ei);
                        }

                    }
                }
                if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "DestAddrModel");

                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string AddrCode = ei.Get("ADDR_CODE");
                            sql = string.Format("SELECT * FROM DEST_ADDR WHERE CNTRY_CD={0} AND PORT_CD={1}  AND ADDR_CODE={2} AND FACTORY={3} AND SHIP_TO={4} AND CMP={5}",
                                SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo), SQLUtils.QuotedStr(Cmp));
                            DataTable dt = getDataTableFromSql(sql);
                            if (dt.Rows.Count > 0)
                            {
                                return Json(new { message = @Resources.Locale.L_SMFSC_Controllers_461 });
                            }
                            ei.Put("PORT_CD", portCd);
                            ei.Put("CNTRY_CD", cntryCd);
                            ei.Put("FACTORY", factory);
                            ei.Put("SHIP_TO", shipTo);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Cmp);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("ADDR_CODE", ei.Get("ADDR_CODE"));
                            ei.PutKey("FACTORY", ei.Get("FACTORY"));
                            ei.PutKey("SHIP_TO", ei.Get("SHIP_TO"));
                            ei.PutKey("CMP", Cmp);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("ADDR_CODE", ei.Get("ADDR_CODE"));
                            ei.PutKey("FACTORY", ei.Get("FACTORY"));
                            ei.PutKey("SHIP_TO", ei.Get("SHIP_TO"));
                            ei.PutKey("CMP", Cmp);
                        }
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("PORT_CD") != "" && ei.Get("FACTORY") != "" && ei.Get("SHIP_TO") != "")
                        {
                            mixList.Add(ei);
                        }
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
                }
            }
            sql = string.Format("SELECT * FROM BSDEST WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2} AND FACTORY={3} AND SHIP_TO={4}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            mainTable = ModelFactory.ToTableJson(mainDt, "BsDestModel");

            sql = string.Format("SELECT * FROM DEST_ADDR WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2} AND FACTORY={3} AND SHIP_TO={4}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(factory), SQLUtils.QuotedStr(shipTo));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            subTable = ModelFactory.ToTableJson(subDt, "DestAddrModel");
            return Json(new { message = returnMessage, mainData = mainTable, subData = subTable });
        }

        #endregion

        public ActionResult ExpressSetup()
        {
            //ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("ExpressSetup");
            return View();
        }


        public ActionResult IpPart()
        {
            string sql = "SELECT * FROM IPPART WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "SUPPLIER_CD", "PART_NO", "GROUP_ID" });
            ViewBag.schemas = ToContent(schemas);
            return View();
        }

        //料号明细档
        public ActionResult PartdInquiryData()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "IPPARTD", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            string resultType = Request.Params["resultType"];
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

        //币别建档
        public ActionResult CurrencyQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "BSCUR", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "CurrencyModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        //币别建档
        public ActionResult CurrencyUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "CurrencyModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("CUR", ei.Get("CUR"));
                        }
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
                }
            }
            return Json(new { message = returnMessage });
        }

        //汇率建档
        public ActionResult ExchangeRateQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("ExchangeRateModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            string baseCondition = "GROUP_ID='" + GroupId + "'";// +"AND CMP='" + CompanyId + "'";
            string FSearchRDate = Prolink.Math.GetValueAsString(Request["FSearchRDate"]);
            string Fcur = Prolink.Math.GetValueAsString(Request["Fcur"]);
            string Tcur = Prolink.Math.GetValueAsString(Request["Tcur"]);
            if (!string.IsNullOrEmpty(FSearchRDate)) baseCondition += string.Format(" AND EDATE LIKE '{0}%' ",FSearchRDate);
            if (!string.IsNullOrEmpty(Fcur)) baseCondition += string.Format(" AND Fcur LIKE '%{0}%' ", Fcur);
            if (!string.IsNullOrEmpty(Tcur)) baseCondition += string.Format(" AND Tcur LIKE '%{0}%' ", Tcur);
            //DataTable dt = ModelFactory.InquiryData("*", "BSERATE", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = ModelFactory.InquiryData("*", "BSERATE", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ExchangeRateModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        //汇率建档
        public ActionResult ExchangeRateInquiryData()
        {

            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            //NameValueCollection nameValues = Request.Params;
            //nameValues["conditions"] += "sopt_GroupId=eq&sopt_GroupId=" + GroupId;
            DataTable dt = ModelFactory.InquiryData("ExchangeRateModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "ExchangeRateModel")
            };

            return result.ToContent();
        }

        //汇率建档
        public ActionResult ExchangeRateUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string date = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ExchangeRateModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        string dateTime = ei.Get("EDATE");
                        if (dateTime.Length > 7)
                        {
                            date = dateTime.Substring(0, 8);
                        }
                        ei.PutDate("EDATE", date);
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("ETYPE", ei.Get("ETYPE"));
                            ei.PutDate("EDATE", date);
                            ei.PutKey("FCUR", ei.Get("FCUR"));
                            ei.PutKey("TCUR", ei.Get("TCUR"));
                        }
                        //if (ei.Get("ETYPE") != "" && ei.Get("EDATE") != "" && ei.Get("FCUR") != "" && ei.Get("TCUR") != "")
                        //{
                        mixList.Add(ei);
                        //}
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
                }
            }
            return Json(new { message = returnMessage });
        }

        //城市港口建档
        public ActionResult CitySetupQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("CitySetupModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "CitySetupModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        //城市港口建档
        public ActionResult CityPortInquiryData()
        {

            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            //NameValueCollection nameValues = Request.Params;
            //nameValues["conditions"] += "sopt_GroupId=eq&sopt_GroupId=" + GroupId;
            string condition = GetBaseGroup();
            condition = GetCreateDateCondition("BSCITY", condition);
            DataTable dt = ModelFactory.InquiryData("*", "BSCITY", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            //DataTable dt = ModelFactory.InquiryData("CitySetupModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "CitySetupModel")
            };

            return result.ToContent();
        }

        //城市港口建档
        public ActionResult CitySetupUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string portCd = Request.Params["portCd"];
            string cntryCd = Request.Params["cntryCd"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Dictionary<string, object>> mainTable = new List<Dictionary<string, object>>();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "CitySetupModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string PortCd = Prolink.Math.GetValueAsString(ei.Get("PORT_CD"));
                            string CntryCd = Prolink.Math.GetValueAsString(ei.Get("CNTRY_CD"));
                            //string sql = "SELECT COUNT(*) FROM BSCITY WHERE PORT_CD="+SQLUtils.QuotedStr(PortCd) + " AND CNTRY_CD="+SQLUtils.QuotedStr(CntryCd) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP="+SQLUtils.QuotedStr(CompanyId) + " AND STN="+ SQLUtils.QuotedStr(Station);
                            string sql = "SELECT COUNT(*) FROM BSCITY WHERE PORT_CD=" + SQLUtils.QuotedStr(PortCd) + " AND CNTRY_CD=" + SQLUtils.QuotedStr(CntryCd) + " AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
                            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (num > 0)
                            {
                                returnMessage = @Resources.Locale.L_System_Controllers_474;
                                break;
                            }
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                        }
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("PORT_CD") != "")
                        {
                            mixList.Add(ei);
                        }
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql = string.Format("SELECT * FROM BSCITY WHERE PORT_CD={0} AND CNTRY_CD={1}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    mainTable = ModelFactory.ToTableJson(mainDt, "CitySetupModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = mainTable });
        }

        //卡车送货点建档
        public ActionResult TruckPortSetupQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("*", "BSTPORT", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = ModelFactory.InquiryData("TruckPortSetupModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "TruckPortSetupModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        //常用地址建档
        public ActionResult BSADDRQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("*", "BSTPORT", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = ModelFactory.InquiryData("BSADDRModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BSADDRModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return Json(new { subTable = result.ToContent() });
        }

        //卡车送货点建档
        public ActionResult TruckPortSetupInquiryData()
        {

            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            //NameValueCollection nameValues = Request.Params;
            //nameValues["conditions"] += "sopt_GroupId=eq&sopt_GroupId=" + GroupId;
            string condition = GetBookingCondition();
            condition = GetCreateDateCondition("BSTPORT", condition);
            //DataTable dt = ModelFactory.InquiryData("TruckPortSetupModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            DataTable dt = ModelFactory.InquiryData("*", "BSTPORT", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "TruckPortSetupModel")
            };

            return result.ToContent();
        }

        //卡车送货点建档
        public ActionResult TruckPortSetupUpdate()
        {
            string changeData = Request.Params["changedData"];
            string portCd = Request.Params["portCd"];
            string cntryCd = Request.Params["cntryCd"];
            string Cmp = Request.Params["cmp"];
            string shareTo = Prolink.Math.GetValueAsString(Request.Params["ShareTo"]);
            string uid = string.Empty;
            string returnMessage = "success";
            string sql = string.Empty;
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }

            if (!string.IsNullOrEmpty(shareTo))
                shareTo = HttpUtility.UrlDecode(shareTo);
            string[] shares = shareTo.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<Dictionary<string, object>> mainTable = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> subTable = new List<Dictionary<string, object>>();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            MixedList mixList2 = new MixedList();

            List<string> shareList = new List<string>(shares);
            List<string> oldersharelist= new List<string>();
            sql = string.Format("SELECT * FROM BSTPORT WHERE CNTRY_CD={0} AND PORT_CD={1} AND CMP={2}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(Cmp));
            DataTable bsdt = getDataTableFromSql(sql);
            if (bsdt.Rows.Count > 0)
            {
                string oldshare = Prolink.Math.GetValueAsString(bsdt.Rows[0]["SHARE_TO"]);
                if (!string.IsNullOrEmpty(oldshare))
                {
                    string[] oldshares = oldshare.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    oldersharelist = new List<string>(oldshares);
                }
            }

            if (oldersharelist.Count > 0)
            {
                foreach (string ocmp in oldersharelist)
                {
                    RemoveFromOlder(portCd, cntryCd, Cmp, mixList, ocmp);
                }
            }
            bool isdelete = false;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TruckPortSetupModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            sql = string.Format("SELECT * FROM BSTPORT WHERE CNTRY_CD={0} AND PORT_CD={1} AND CMP={2}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(Cmp));
                            DataTable dt = getDataTableFromSql(sql);
                            if (dt.Rows.Count > 0)
                            {
                                return Json(new { message = @Resources.Locale.L_SMFSC_Controllers_461 });
                            }
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("CMP", Cmp);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION) 
                        {
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("CMP", Cmp);
                            string Sql = string.Format("DELETE FROM BSADDR WHERE CNTRY_CD = {0} AND PORT_CD = {1} AND CMP={2}", SQLUtils.QuotedStr(ei.Get("CNTRY_CD")), SQLUtils.QuotedStr(ei.Get("PORT_CD")),SQLUtils.QuotedStr(Cmp));
                            mixList.Add(Sql);
                            //RemoveFromOlder(ei.Get("PORT_CD"), ei.Get("CNTRY_CD"), Cmp, mixList, ocmp);
                            isdelete = true;
                        }
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("PORT_CD") != "")
                        {
                            mixList.Add(ei);
                        }

                    }
                }
                if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BSADDRModel");
                    //string sql = string.Format("DELETE FROM BSADDR WHERE CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd));
                    //mixList2.Add(sql);
                    //int[] result = OperationUtils.ExecuteUpdate(mixList2, Prolink.Web.WebContext.GetInstance().GetConnection());
                    
                    for (int i = 0; i < list.Count; i++) 
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string AddrCode = ei.Get("ADDR_CODE");
                            sql = string.Format("SELECT * FROM BSADDR WHERE CNTRY_CD={0} AND PORT_CD={1} AND ADDR_CODE={2} AND CMP={3}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(Cmp));
                            DataTable dt = getDataTableFromSql(sql);
                            if (dt.Rows.Count > 0) 
                            {
                                return Json(new { message = @Resources.Locale.L_SMFSC_Controllers_461 });
                            }
                            ei.Put("PORT_CD", portCd);
                            ei.Put("CNTRY_CD", cntryCd);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            //ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", Cmp);
                            //ei.Put("STN", BaseStation);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("ADDR_CODE", ei.Get("ADDR_CODE"));
                            //ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", Cmp);
                            ei.PutKey("SHARE_FROM", ei.Get("SHARE_FROM"));
                        }
                        if(ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                            ei.PutKey("PORT_CD", ei.Get("PORT_CD"));
                            ei.PutKey("ADDR_CODE", ei.Get("ADDR_CODE"));
                            ei.PutKey("CMP", Cmp);
                            ei.PutKey("SHARE_FROM", ei.Get("SHARE_FROM"));
                        }
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("PORT_CD") != "")
                        {
                            mixList.Add(ei);
                        }
                    }
                }

            }
            if (mixList.Count > 0)
            {

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    mixList = new MixedList();
                    if (!isdelete) {
                        foreach (string share in shareList)
                        {
                            string portsql = string.Format("SELECT COUNT(1) FROM BSTPORT WHERE CNTRY_CD={0} AND PORT_CD={1} AND CMP={2}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(share));
                            int count = getOneValueAsIntFromSql(portsql);
                            string insertsql = string.Empty;
                            if (count <= 0)
                            {
                                insertsql = string.Format(@"INSERT INTO BSTPORT(CNTRY_CD,PORT_CD,GROUP_ID,CMP,PORT_NM,CNTRY_NM,STATE,REGION,NS,EW,REMARK,STN,DEP,CREATE_BY,CREATE_DATE,SHARE_FROM)
                        SELECT CNTRY_CD,PORT_CD,GROUP_ID,{3},PORT_NM,CNTRY_NM,STATE,REGION,NS,EW,REMARK,STN,DEP,{4},GETDATE(),{2} 
                        FROM BSTPORT WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(share), SQLUtils.QuotedStr(UserId));
                                mixList.Add(insertsql);
                            }
                            insertsql = string.Format(@"INSERT INTO BSADDR(GROUP_ID,CMP,STN,DEP,CNTRY_CD,PORT_CD,SEQ_NO,ADDR_CODE,ADDR,CREATE_BY,CREATE_DATE,OUTER_FLAG,FINAL_WH,CUSTOMER_CODE,PLANT,SHARE_FROM)
                        SELECT GROUP_ID,{3},STN,DEP,CNTRY_CD,PORT_CD,SEQ_NO,ADDR_CODE,ADDR,{4},GETDATE(),OUTER_FLAG,FINAL_WH,CUSTOMER_CODE,PLANT,{2} 
                        from BSADDR  WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp), SQLUtils.QuotedStr(share), SQLUtils.QuotedStr(UserId));
                            mixList.Add(insertsql);
                        }
                    }
                    if (mixList.Count > 0)
                        result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            sql = string.Format("SELECT * FROM BSTPORT WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            mainTable = ModelFactory.ToTableJson(mainDt, "TruckPortSetupModel");

            sql = string.Format("SELECT * FROM BSADDR WHERE PORT_CD={0} AND CNTRY_CD={1} AND CMP={2}", SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(Cmp));
            DataTable subDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            subTable = ModelFactory.ToTableJson(subDt, "BSADDRModel");
            return Json(new { message = returnMessage, mainData = mainTable , subData = subTable });
        }

        private void RemoveFromOlder(string portCd, string cntryCd, string Cmp, MixedList mixList, string ocmp)
        {
            string addrsql = string.Format("SELECT COUNT(1) FROM BSADDR WHERE CNTRY_CD = {0} AND PORT_CD = {1} AND CMP={2} AND (SHARE_FROM IS NULL OR SHARE_FROM !={3})", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(ocmp), SQLUtils.QuotedStr(Cmp));
            int subcount = getOneValueAsIntFromSql(addrsql);
            string delsql = string.Empty;
            if (subcount <= 0)
            {
                delsql = string.Format("DELETE FROM BSTPORT WHERE CNTRY_CD={0} AND PORT_CD={1} AND CMP={2}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(ocmp));
                mixList.Add(delsql);
            }
            delsql = string.Format("DELETE FROM BSADDR WHERE CNTRY_CD = {0} AND PORT_CD = {1} AND CMP={2} AND SHARE_FROM={3}", SQLUtils.QuotedStr(cntryCd), SQLUtils.QuotedStr(portCd), SQLUtils.QuotedStr(ocmp), SQLUtils.QuotedStr(Cmp));
            mixList.Add(delsql);
        }

        //国家代码建档
        public ActionResult CntyQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("CntyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            string baseCondition = "GROUP_ID='" + GroupId + "'";
            DataTable dt = ModelFactory.InquiryData("*", "BSCNTY", baseCondition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        //国家代码建档
        public ActionResult CntyUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string cid = Prolink.Math.GetValueAsString(Request.Params["cid"]);
            string gid = Prolink.Math.GetValueAsString(Request.Params["gid"]);
            string CntryCd = Prolink.Math.GetValueAsString(Request.Params["CntryCd"]);
            if (!string.IsNullOrEmpty(cid))
                cid = HttpUtility.UrlDecode(cid);
            if (!string.IsNullOrEmpty(gid))
                gid = HttpUtility.UrlDecode(gid);
            if (!string.IsNullOrEmpty(CntryCd))
                CntryCd = HttpUtility.UrlDecode(CntryCd);
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "CntyModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                            ei.PutKey("STN", Station);
                            ei.Put("DEP", Dep);
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("GROUP_ID", gid);
                            ei.PutKey("CMP", cid);
                            ei.PutKey("CNTRY_CD", CntryCd);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("GROUP_ID", ei.Get("GROUP_ID"));
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("CNTRY_CD", ei.Get("CNTRY_CD"));
                        }
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
                }
            }
            string sql = string.Format("SELECT * FROM BSCNTY WHERE CNTRY_CD={0} AND CMP={1}", SQLUtils.QuotedStr(CntryCd), SQLUtils.QuotedStr(cid));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "CntyModel");
            return ToContent(data);
        }

        /// <summary>
        /// 获取全部的权限菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllPermission()
        {
            //Permission p = PermissionManager.GetFirstPermission();
            //List<Dictionary<string, object>> list = GetPermission(p);
            //return ToContent(list);
            Dictionary<string, object> level = null;
            List<Dictionary<string, object>> firstLevel = null;
            List<Dictionary<string, object>> secondLevel = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = null;
            string path = MenuManager.GetMenuPath(Prolink.Web.WebContext.GetInstance());
            string menuName = "BIG5_MENU.xml";
            switch (SiteLang)
            {
                case "zh-TW":
                    menuName = "BIG5_MENU.xml";
                    break;
                case "en-US":
                    menuName = "ENG_MENU.xml";
                    break;
                case "zh-CN":
                    menuName = "GB_MENU.xml";
                    break;
                case "ru-RU":
                    menuName = "RU_MENU.xml";
                    break;
                default:
                    menuName = "GB_MENU.xml";
                    break;
            }
            string menuPath = System.IO.Path.Combine(path, menuName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(menuPath);
            XmlNodeList xnl = xmlDoc.SelectNodes("/menu/menu");
            firstLevel = new List<Dictionary<string, object>>();
            foreach (XmlNode xn in xnl)
            {
                level = new Dictionary<string, object>();
                level["text"] = "";
                if (xn.FirstChild.Value.Split('@').Length > 0)
                    level["text"] = xn.FirstChild.Value.Split('@')[0].ToString().Replace("\r\n", "");
                level["href"] = "#";
                level["icon"] = "glyphicon glyphicon-folder-open";
                secondLevel = new List<Dictionary<string, object>>();
                XmlNodeList menus = xn.SelectNodes("menu");
                foreach (XmlNode menu in menus)
                {
                    item = new Dictionary<string, object>();
                    item["text"] = menu.FirstChild.Value;
                    item["href"] = GetObjectValue(menu.Attributes, "pmsId");
                    secondLevel.Add(item);
                }
                level["nodes"] = secondLevel;
                firstLevel.Add(level);
            }

            return ToContent(firstLevel);
        }

        public ActionResult UserSetInquiryData()
        {
            string model = Request["model"];
            string hasm = Request["hasm"];//是否包含实体定义
            JavaScriptSerializer js = new JavaScriptSerializer();
            string sql = ModelFactory.GetHeadSql("SysAcctModel");

            sql = sql + " ORDER BY UPDATE_PRI_DATE DESC";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("SysAcctModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "SysAcctModel")
            };


            return result.ToContent();
        }

        public string GetObjectValue(XmlAttributeCollection nObject, string nKey)
        {
            if (nObject[nKey] == null)
            {
                return string.Empty;
            }
            else
            {
                return nObject[nKey].Value;
            }
        }
        //客户建档
        public ActionResult BSCSSetupQuery()
        {
            string resultType = Request.Params["resultType"];
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SYS_CMP", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        public JsonResult BSCSSetUpdateData()
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

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            string uid = string.Empty;
            List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysCmpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        uid = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                    }
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //int recordsCount = 0, pageIndex = 1, pageSize = 20;
                        //DataTable dt = ModelFactory.InquiryData("*", "SYS_CMP", "U_ID='" + uid + "'", "", pageIndex, pageSize, ref recordsCount);
                        //userData = ModelFactory.ToTableJson(dt, "SysCmpModel");
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                    }

                }
            }
            return Json(new { message = returnMessage, userData = userData });
        }


        public JsonResult UserSetUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string groupId = Request.Params["groupId"];
            string uicmp = Request.Params["cmp"];
            string ItSd = Request.Params["ItSd"];

            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();



            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            string uid = string.Empty;
            List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> userWhData = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            string cmp = "";
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysAcctModel");

                    string sapid = "";
                    string uStatus = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.Get("CMP") != null)
                        {
                            cmp = ei.Get("CMP");
                        }
                        else
                        {
                            cmp = "*";
                        }

                        if (ei.Get("SAP_ID") != null)
                        {
                            sapid = ei.Get("SAP_ID");
                        }

                        if (ei.Get("U_STATUS") != null)
                        {
                            uStatus = ei.Get("U_STATUS");
                        }

                        string defaultSite = Prolink.Math.GetValueAsString(ei.Get("DEFAULT_SITE"));

                        uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                        if ("Y".Equals(defaultSite))
                        {
                            string sql = string.Format(@"UPDATE SYS_ACCT SET DEFAULT_SITE=NULL WHERE U_ID={0} AND CMP<>{1} AND 
EXISTS( SELECT 1 FROM SMPTY WHERE PARTY_NO=SYS_ACCT.CMP AND STATUS='U' AND IS_OUTBOUND IN('I','A'))",
                                SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(cmp));
                            mixList.Add(sql);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //ei.PutDate("MODI_PW_DATE", DateTime.Now.AddMonths(6).ToString("yyyyMMddHHmm"));
                            uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            WebGui.Models.User.RemoveIPyCache(uid, uicmp);
                            bool chk = chkSapIdExist(sapid, uid);
                            if (chk == true)
                            {
                                return Json(new { message = @Resources.Locale.L_SystemController_Controllers_212 });
                            }

                            if (Prolink.Math.GetValueAsInt(ei.Get("U_STATUS")).Equals(0) && checkStatus(uid, groupId, uicmp))
                            {
                                ei.PutDate("LOGIN_DATE", DateTime.Now);
                            }

                            if (ei.Get("U_PASSWORD") != null)
                            {
                                string pwd = genMD5(Prolink.Math.GetValueAsString(ei.Get("U_PASSWORD")));
                                ei.Put("U_PASSWORD", pwd);
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutDate("UPDATE_PRI_DATE", DateTime.Now.AddMonths(6).ToString("yyyyMMddHHmm"));
                            if ("2".Equals(uStatus))
                            {
                                ei.PutDate("LEAVE_DATE", Business.TimeZoneHelper.GetTimeZoneDate(CompanyId));
                            } 
                        }
                        else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            //ei.PutDate("UPDATE_PRI_DATE", DateTime.Now.AddMonths(6).ToString("yyyyMMddHHmm"));
                            // ei.PutDate("MODI_PW_DATE", DateTime.Now.AddMonths(6).ToString("yyyyMMddHHmm"));
                            bool chk = chkSapIdExist(sapid);
                            if (chk == true)
                            {
                                return Json(new { message = @Resources.Locale.L_SystemController_Controllers_212 });
                            }
                            uid = Prolink.Math.GetValueAsString(ei.Get("U_ID")).Trim();
                            chk = chkUserExsit(uid, ei.Get("CMP"));
                            if (chk == true)
                            {
                                returnMessage = @Resources.Locale.L_ModifyPwd_Dup;
                                return Json(new { message = returnMessage });
                            }
                            string pwd = genMD5(Prolink.Math.GetValueAsString(ei.Get("U_PASSWORD")));
                            ei.Put("U_PASSWORD", pwd);
                            //king fix stn equal cmp error
                            ei.Put("U_ID", uid);
                            ei.Put("STN", "*");
                            ei.Put("CMP", cmp);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.PutDate("LOGIN_DATE", DateTime.Now);  
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            EditInstruct ei2 = new EditInstruct("SYS_ACCT_LOG", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("USER_ID", ei.Get("U_ID"));
                            ei2.PutKey("CMP", ei.Get("CMP"));
                            ei2.PutKey("GROUP_ID", GroupId);
                            mixList.Add(ei2);

                            EditInstruct ei3 = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.DELETE_OPERATION);
                            ei3.PutKey("FACCT_ID", ei.Get("U_ID"));
                            ei3.PutKey("GROUP_ID", GroupId);
                            ei3.PutKey("CMP", ei.Get("CMP"));
                            mixList.Add(ei3);
                        }
                        if ("1".Equals(uStatus))
                        {
                            ei.Put("STOP_BY", UserId);
                            ei.PutDate("STOP_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            WriteDBLog(mixList, ei, ItSd);
                        }
                    }
                }
                else if (item.Key == "wh")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysAcctWhModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        ei.Remove("WsUid");
                        if (ei.Get("U_CMP") != null&&string.IsNullOrEmpty(cmp))
                        {
                            cmp = ei.Get("CMP");
                        }
                        else if(string.IsNullOrEmpty(cmp))
                        {
                            cmp = "*";
                        }
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            //ei.Put("U_FID", UId);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);
                    }
                }

            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                int recordsCount = 0, pageIndex = 1, pageSize = 20;
                if (string.IsNullOrEmpty(uicmp)) uicmp = cmp;
                DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", string.Format("U_ID={0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(uicmp)), "", pageIndex, pageSize, ref recordsCount);
                userData = ModelFactory.ToTableJson(dt, "SysAcctModel");
                DataTable whDt = ModelFactory.InquiryData("*", "SYS_ACCT_WH", string.Format("USER_ID={0} AND GROUP_ID={1} AND U_CMP={2}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(uicmp)), "", pageIndex, pageSize, ref recordsCount);
                userWhData = ModelFactory.ToTableJson(whDt, "SysAcctWhModel");
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }
            return Json(new { message = returnMessage, userData = userData, userWhData = userWhData });
        }

        /// <summary>
        /// 停用状态返回true, 启用状态返回false
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="groupId"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        private bool checkStatus(string uid,string groupId,string cmp)
        {
            bool flag = false;
            string sql = string.Format("SELECT U_STATUS FROM SYS_ACCT WHERE U_ID={0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                if (Prolink.Math.GetValueAsInt(dt.Rows[0]["U_STATUS"]).Equals(1))
                    flag = true;
                else
                    flag = false;
            }
            return flag;
        }

        public JsonResult UserSetUpdatePwd()
        {
            string changeData = Request.Params["changedData"];
            string Opwd = Request.Params["Opwd"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }

            string sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_STATUS=0 AND Upper(U_ID)=Upper('{0}') AND U_PASSWORD='{1}' AND CMP='{2}'",UserId,genMD5(Opwd),CompanyId);
            DataTable dt1 = OperationUtils.GetDataTable(sql,null,Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt1 == null || dt1.Rows.Count < 1)
            {
                return Json(new { message = @Resources.Locale.L_ModifyExpridPwd_Opwdwrong });
            }

            JavaScriptSerializer js = new JavaScriptSerializer();



            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            string uid = string.Empty;
            List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysAcctModel");
                    string cmp = "";
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            //ei.PutDate("MODI_PW_DATE", DateTime.Now.AddMonths(6).ToString("yyyyMMddHHmm"));
                            if (ei.Get("U_PASSWORD") != null)
                            {
                                string pwd = genMD5(Prolink.Math.GetValueAsString(ei.Get("U_PASSWORD")));
                                ei.Put("U_PASSWORD", pwd);
                            }

                            uid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                            ei.PutKey("U_ID", UserId);

                            string sql1 = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0} AND CMP={1} AND GROUP_ID={2}", SQLUtils.QuotedStr(UserId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(GroupId));
                            DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                string IOFLAG= Prolink.Math.GetValueAsString(dt.Rows[0]["IO_FLAG"]);
                                string I_E = Prolink.Math.GetValueAsString(dt.Rows[0]["I_E"]);
                                if (IOFLAG == "O" || (IOFLAG == "I"))
                                {
                                    int date = Prolink.Math.GetValueAsInt(dt.Rows[0]["MODI_PW_DATE"]);
                                    date = date <= 0 ? 90 : date;
                                    //int date = 90;
                                    DateTime d = DateTime.Now.AddDays(date);
                                    ei.PutDate("UPDATE_PRI_DATE", d);
                                }
                            }
                        }
                        mixList.Add(ei);
                    }
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        int recordsCount = 0, pageIndex = 1, pageSize = 20;
                        DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", "U_ID='" + uid + "'", "", pageIndex, pageSize, ref recordsCount);
                        userData = ModelFactory.ToTableJson(dt, "SysAcctModel");
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                    }

                }
            }
            return Json(new { message = returnMessage, userData = userData });
        }


        public JsonResult UserUpdatePwd()
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

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            string uid = UserId;
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysAcctModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            string pwd = Prolink.Math.GetValueAsString(ei.Get("U_PASSWORD"));
                            if (pwd == "")
                            {
                                returnMessage = @Resources.Locale.L_System_Controllers_476;
                                continue;
                            }
                            ei.Put("U_PASSWORD", genMD5(pwd));
                            ei.PutKey("U_ID", uid);
                            ei.PutKey("CMP", CompanyId);
                            ei.PutKey("GROUP_ID", GroupId);
                        }
                        mixList.Add(ei);
                    }
                    try
                    {
                        int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                    }

                }
            }

            return Json(new { message = returnMessage });
        }

        /// <summary>
        /// 角色建档画面查询
        /// </summary>
        /// <returns></returns>
        public ActionResult RoleSetInquiryData()
        {
            //string model = Request["model"];
            //string hasm = Request["hasm"];//是否包含实体定义
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //string sql = ModelFactory.GetHeadSql("SysAcctModel");
            //sql = sql + " ORDER BY UPDATE_PRI_DATE DESC";
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            /*
            string page_str = Request.Params["page"];//第几页  从1开始
            string limit_str = Request.Params["rows"];//每页大小
            if (!int.TryParse(page_str, out page)) page = 0;
            if (!int.TryParse(limit_str, out limit)) limit = 20;             
            */

            DataTable dt = ModelFactory.InquiryData("SysRoleModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "SysRoleModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public JsonResult RoleSetUpdateData()
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
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            List<Role> roleList = new List<Role>();
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SysRoleModel");

                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.PutKey("STN", "*");
                            string sql = string.Format(@"SELECT COUNT(1) FROM SYS_ROLE WHERE FID={0} AND GROUP_ID={1} AND CMP={2} AND STN={3}",
                                SQLUtils.QuotedStr(ei.Get("FID")), SQLUtils.QuotedStr(ei.Get("GROUP_ID")), SQLUtils.QuotedStr(ei.Get("CMP")),
                                SQLUtils.QuotedStr(ei.Get("STN")));
                            int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (count > 0)
                                return Json(new { message = @Resources.Locale.L_System_Controllers_474 }); 
                            
                            Business.CommonHelp.AddRoles(ei.Get("FID"), ei.Get("GROUP_ID"), ei.Get("CMP"), ei.Get("STN"), roleList, ei.Get("FDESCP"));
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            EditInstruct ei2 = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("FROLE_ID", Prolink.Math.GetValueAsString(ei.Get("FID")));
                            ei2.PutKey("CMP", Prolink.Math.GetValueAsString(ei.Get("CMP")));
                            ei2.PutKey("GROUP_ID", Prolink.Math.GetValueAsString(ei.Get("GROUP_ID")));
                            mixList.Add(ei2);
                        }

                        mixList.Add(ei);
                    }
                }

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                    Prolink.V3.PermissionManager.SetRolePermission(roleList);
                    Business.CommonHelp.NotifyRebuidPermission(roleList);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }

            return Json(new { message = returnMessage });
        }

        public JsonResult CheckRoleRelation()
        {
            string fid = Request.Params["fid"];
            string returnMessage = "success";

            try
            {
                string sql = " SELECT COUNT(FACCT_ID) FROM SYS_ACCT_ROLE WHERE FROLE_ID = " + SQLUtils.QuotedStr(fid);
                int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (count > 0)
                {
                    return Json(new { message = "hasAccount" });
                }
                
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
            }

            return Json(new { message = returnMessage });
        }


        //此為範例
        public ActionResult GetSysRoleSchema()
        {
            string sql = "SELECT * FROM TYPETEST WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "FID", "GROUP_ID", "CMP", "STN" });
            return ToContent(schemas);
        }

        public ActionResult GetSysSiteInfo(string id)
        {
            string inquiryType = id;
            string groupId = Request.Params["groupId"];
            string companyId = Request.Params["companyId"];
            string stnId = Request.Params["stnId"];
            string inquirySql = "SELECT * FROM SYS_SITE";
            string conditions = " WHERE";
            if ("0".Equals(inquiryType))
            {
                conditions += " TYPE='0'";
            }
            else if ("1".Equals(inquiryType))
            {
                conditions += " TYPE='1' AND GROUP_ID=" + SQLUtils.QuotedStr(groupId);
            }
            else if ("2".Equals(inquiryType))
            {
                conditions += " TYPE='2' AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(companyId); ;
            }
            else if ("3".Equals(inquiryType))
            {
                conditions += " TYPE='3'  AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(companyId) + " AND STN=" + SQLUtils.QuotedStr(stnId);
            }
            inquirySql += conditions;
            DataTable groupDt = OperationUtils.GetDataTable(inquirySql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            ModelFactory.ToTableJson(groupDt, "SysSiteModel");
            return ToContent(ModelFactory.ToTableJson(groupDt, "SysSiteModel"));
        }

        public class OptionsItem
        {
            public string cd { get; set; }
            public string cdDescp { get; set; }
        }


        public class UserOptions
        {
            public List<OptionsItem> De = new List<OptionsItem>();
        }

        public ActionResult GetUserSysSiteInfo(string id)
        {
            string inquiryType = id;
            string groupId = Request.Params["groupId"];
            string companyId = Request.Params["companyId"];
            string stnId = Request.Params["stnId"];
            string inquirySql = "SELECT * FROM SYS_SITE";
            string conditions = " WHERE";
            if ("0".Equals(inquiryType))
            {
                conditions += " TYPE='0'";
            }
            else if ("1".Equals(inquiryType))
            {
                conditions += " TYPE='1' AND GROUP_ID=" + SQLUtils.QuotedStr(groupId);
            }
            else if ("2".Equals(inquiryType))
            {
                conditions += " TYPE='2' AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(companyId); ;
            }
            else if ("3".Equals(inquiryType))
            {
                conditions += " TYPE='3'  AND GROUP_ID=" + SQLUtils.QuotedStr(groupId) + " AND CMP=" + SQLUtils.QuotedStr(companyId) + " AND STN=" + SQLUtils.QuotedStr(stnId);
            }
            inquirySql += conditions;
            DataTable dt = OperationUtils.GetDataTable(inquirySql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            UserOptions iOptions = new UserOptions();
            string cd, cdDescp;
            foreach (DataRow dr in dt.Rows)
            {
                switch (inquiryType)
                {
                    case "0":
                        cd = cdDescp = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                        iOptions.De.Add(new OptionsItem
                        {
                            cd = cd,
                            cdDescp = cdDescp
                        });
                        break;
                    case "1":
                        cd = cdDescp = Prolink.Math.GetValueAsString(dr["CMP"]);
                        iOptions.De.Add(new OptionsItem
                        {
                            cd = cd,
                            cdDescp = cdDescp
                        });
                        break;
                    case "2":
                        cd = cdDescp = Prolink.Math.GetValueAsString(dr["STN"]);
                        iOptions.De.Add(new OptionsItem
                        {
                            cd = cd,
                            cdDescp = cdDescp
                        });
                        break;
                    case "3":
                        cd = cdDescp = Prolink.Math.GetValueAsString(dr["DEP"]);
                        iOptions.De.Add(new OptionsItem
                        {
                            cd = cd,
                            cdDescp = cdDescp
                        });
                        break;
                }
            }
            return Json(iOptions);
        }

        public class PartyTypeOptionsItem
        {
            public string cd { get; set; }
            public string cdDescp { get; set; }
        }


        public class PartyTypeIppomOptions
        {
            public List<PartyTypeOptionsItem> De = new List<PartyTypeOptionsItem>();
        }


        public ActionResult GetPartyTypeSelects()
        {
            string sql = "SELECT * FROM BSCODE WHERE GROUP_ID='" + GroupId + "' AND CD_TYPE='PT' ";
            DataTable dt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            PartyTypeIppomOptions iOptions = new PartyTypeIppomOptions();
            string cd, cdDescp;
            foreach (DataRow dr in dt.Rows)
            {
                cd = Prolink.Math.GetValueAsString(dr["CD"]);
                cdDescp = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                iOptions.De.Add(new PartyTypeOptionsItem
                {
                    cd = cd,
                    cdDescp = cdDescp
                });
            }
            return Json(iOptions);
        }


        public JsonResult GroupRelationUpdateData()
        {
            int status = 1;
            string changeData = Request.Params["changedData"];
            //Stream s = System.Web.HttpContext.Current.Request.InputStream;
            string returnMessage = "success";
            if (changeData != null)
            {
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            }
            else
            {
                status = 0;
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            object[] jsmodels = js.DeserializeObject(changeData) as object[];
            MixedList list = ModelFactory.JsonToEditMixedList(jsmodels, "SysSiteModel");

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                returnMessage = ex.ToString();
                status = 0;
            }

            return Json(new { status = status });
        }


        public ActionResult TestRequiryFunc1()
        {
            //System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
            //nvc.Add("page", "1");
            //nvc.Add("rows", "20");
            //nvc.Add("table", "SYS_ACCT");
            //string columnStr = "{\"ColumnList\":[{\"FieldName\":\"UId\",\"Caption\":\"ID\",\"Width\":60,\"Alignment\":\"left\",\"DbType\":0,\"MaxLength\":20,\"Scale\":0},{\"FieldName\":\"UName\",\"Caption\":\"User Name\",\"Width\":120,\"Alignment\":\"left\",\"DbType\":0,\"MaxLength\":100,\"Scale\":0},{\"FieldName\":\"UStatus\",\"Caption\":\"Status\",\"Width\":60,\"Alignment\":\"right\",\"DbType\":1,\"MaxLength\":16,\"Scale\":0}]}";
            //nvc.Add("columnList", columnStr);
            //nvc.Add("totalColumns", "UStatus");

            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);


            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "SysAcctModel")
            };
            return result.ToContent();
        }

        public ActionResult IpGoodsRequiry()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;

            DataTable dt = ModelFactory.InquiryData("IpGoodsModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "IpGoodsModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public JsonResult IpGoodscRequiry()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string goods = Prolink.Math.GetValueAsString(Request.Params["Goods"]);
            string groupid = Prolink.Math.GetValueAsString(Request.Params["GroupId"]);
            string condtions = " WHERE GROUP_ID=" + SQLUtils.QuotedStr(groupid) + " AND GOODS=" + SQLUtils.QuotedStr(goods);
            DataTable detailDt = ModelFactory.InquiryData("*", "IPGOODSC", condtions, "", pageIndex, pageSize, ref recordsCount);
            BootstrapResult resultDetail = null;
            resultDetail = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(detailDt)
            };
            return Json(new { mainTable = resultDetail.ToContent() });


        }

        public JsonResult IpGoodscUpdate()
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

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "IpGoodsModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            EditInstruct ei2 = new EditInstruct("IPGOODSC", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("GOODS", Prolink.Math.GetValueAsString(ei.Get("GOODS")));
                            mixList.Add(ei2);
                        }

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION || (ei.Get("GOODS") != ""))
                        {
                            mixList.Add(ei);
                        }

                    }


                }
                else if (item.Key == "goodscinfo")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "IpGoodscModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.Get("CNTRY_CD") != "" && ei.Get("TAX_RATE") != "")
                        {
                            mixList.Add(ei);
                        }

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
                }
            }
            return Json(new { message = returnMessage });
        }


        /// <summary>
        ///料号建档放大镜查询
        /// </summary>
        /// <returns></returns>
        public ActionResult IpPartInquiryData()
        {

            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            //NameValueCollection nameValues = Request.Params;
            //nameValues["conditions"] += "sopt_GroupId=eq&sopt_GroupId=" + GroupId;
            DataTable dt = ModelFactory.InquiryData("IpPartModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                page = pageIndex,
                records = recordsCount,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt, "IpPartModel")
            };

            return result.ToContent();
        }

        public JsonResult IpPartUpdateData()
        {
            string changeData = Request.Params["changedData"];
            string SupplierCd = Request.Params["SupplierCd"];
            string PartNo = Request.Params["PartNo"];
            string Cntry = Request.Params["Cntry"];
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            MixedList mixList = new MixedList();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);

            int recordsCount = 0, pageIndex = 1, pageSize = 20;
            List<Dictionary<string, object>> ippartData = new List<Dictionary<string, object>>();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "IpPartModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                        }
                        else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                            ei.PutKey("STN", Station);
                            ei.PutKey("CMP", CompanyId);
                        }

                        ei.PutKey("GROUP_ID", GroupId);
                        ei.PutKey("SUPPLIER_CD", SupplierCd);
                        ei.PutKey("PART_NO", PartNo);
                        ei.PutKey("CNTRY", Cntry);


                        mixList.Add(ei);
                    }

                }
                else if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "IppartdModel");

                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            if (GroupId == null || Station == null)
                            {
                                continue;
                            }
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("STN", Station);
                        }

                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            if (GroupId == null || Station == null)
                            {
                                continue;
                            }
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("STN", Station);
                        }

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            if (GroupId == null || Station == null)
                            {
                                continue;
                            }
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("STN", Station);
                        }
                        mixList.Add(ei);
                    }
                }
            }
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    DataTable dt = ModelFactory.InquiryData("*", "IPPART", "", "", pageIndex, pageSize, ref recordsCount);
                    ippartData = ModelFactory.ToTableJson(dt, "IppartdModel");
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }

            return Json(new { message = returnMessage, ippartData = ippartData });
        }

        #region 省份建檔
        public ActionResult BsStateQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("BSSTATE");
            return View();
        }
        public ActionResult BsStateSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM BSSTATE WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BSSTATE");
            return View();
        }
        public ActionResult GetBsStateDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSSTATE WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BsStateModel");
            return ToContent(data);
        }
        public ActionResult GetStateDataForSummary()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = GetBaseCmp();
            condition = GetCreateDateCondition("BSSTATE", condition);
            DataTable dt = ModelFactory.InquiryData("*", "BSSTATE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BsStateModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult BsStateUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            List<Dictionary<string, object>> ipstsData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsStateModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql = string.Format("SELECT * FROM BSSTATE WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ipstsData = ModelFactory.ToTableJson(mainDt, "BsStateModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = ipstsData });
        }
        #endregion

        #region tpv 港口代碼
        public ActionResult TpvPortQuery()
        {
            ViewBag.MenuBar = false;
            ViewBag.pmsList = GetBtnPms("TPVPORT");
            return View();
        }
        public ActionResult TpvPortSetup(string flag = null, string cnty = null, string port = null)
        {
            string sql = "SELECT * FROM TPVPORT WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "FLAG", "CNTY", "PORT" });
            ViewBag.schemas = ToContent(schemas);
            ViewBag.Flag = flag;
            ViewBag.Cnty = cnty;
            ViewBag.Port = port;
            ViewBag.pmsList = GetBtnPms("TPVPORT");
            return View();
        }
        public ActionResult GetTpvPortDetail()
        {
            string flag = Request["Flag"];
            string cnty = Request["Cnty"];
            string port = Request["Port"];
            string sql = string.Format("SELECT * FROM TPVPORT WHERE FLAG={0} AND CNTY={1} AND PORT={2}",
                SQLUtils.QuotedStr(flag), SQLUtils.QuotedStr(cnty), SQLUtils.QuotedStr(port));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "TpvPortModel");
            return ToContent(data);
        }
        public ActionResult GetTpvPortDataForSummary()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = GetCreateDateCondition("TPVPORT", "");
            DataTable dt = ModelFactory.InquiryData("*", "TPVPORT", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            string resultType = Request.Params["resultType"];
            if (resultType == "excel")
                return ExportExcelFile(dt);
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "TpvPortModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult TpvPortUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string flag = Prolink.Math.GetValueAsString(Request.Params["Flag"]);
            string cnty = Prolink.Math.GetValueAsString(Request.Params["Cnty"]);
            string port = Prolink.Math.GetValueAsString(Request.Params["Port"]);
            List<Dictionary<string, object>> ipstsData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TpvPortModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("FLAG", ei.Get("FLAG"));
                            ei.PutKey("CNTY", ei.Get("CNTY"));
                            ei.PutKey("PORT", ei.Get("PORT"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    string sql = string.Format("SELECT * FROM TPVPORT WHERE FLAG={0} AND CNTY={1} AND PORT={2}",
                SQLUtils.QuotedStr(flag), SQLUtils.QuotedStr(cnty), SQLUtils.QuotedStr(port));
                    DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ipstsData = ModelFactory.ToTableJson(mainDt, "TpvPortModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = ipstsData });
        }

        #endregion

        #region 快递公司账号建档
        public ActionResult ExpressQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            //DataTable dt = ModelFactory.InquiryData("ExchangeRateModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //string baseCondition = "GROUP_ID='" + GroupId + "'" + "AND CMP='" + CompanyId + "'";
            DataTable dt = ModelFactory.InquiryData("*", "SMEXM", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ExpressModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }
        public ActionResult ExpressUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            string date = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ExpressModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            string UId = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", ei.Get("U_ID"));

                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
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
                }
            }
            return Json(new { message = returnMessage });
        }

        #endregion

        #region 重置密碼
        public JsonResult updatePwd()
        {
            string UId = Request.Params["UId"];
            string PartyNo = Request.Params["PartyNo"];
            string PartyName = "";
            string UName = Request.Params["UName"];
            string UPassword = GetRandomString4(8);
            string UEmail = Request.Params["UEmail"];
            string g = Request.Params["GroupId"];
            string c = Request.Params["Cmp"];
            string ModiPwDate = Request.Params["ModiPwDate"]; 
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["UId"] = UId;
            parm["PartyNo"] = PartyNo;
            parm["UName"] = UName;
            parm["UEmail"] = UEmail;
            parm["g"] = g;
            parm["c"] = c;
            parm["ModiPwDate"] = ModiPwDate;
            parm["groupId"] = GroupId;
            parm["companyId"] = CompanyId;
            parm["userId"] = UserId;
            List<Dictionary<string, object>> userData = new List<Dictionary<string, object>>();
            string msg = UpdatePassword(parm, ref userData);
            return Json(new { message = msg, userData = userData });
        }

        public string UpdatePassword(Dictionary<string, object> parm, ref List<Dictionary<string, object>> userData)
        {
            string UId = Prolink.Math.GetValueAsString(parm["UId"]);
            string PartyNo = Prolink.Math.GetValueAsString(parm["PartyNo"]);
            string UName = Prolink.Math.GetValueAsString(parm["UName"]);
            string UEmail = Prolink.Math.GetValueAsString(parm["UEmail"]);
            string g = Prolink.Math.GetValueAsString(parm["g"]);
            string c = Prolink.Math.GetValueAsString(parm["c"]);
            string ModiPwDate = Prolink.Math.GetValueAsString(parm["ModiPwDate"]);
            string groupId = Prolink.Math.GetValueAsString(parm["groupId"]);
            string companyId = Prolink.Math.GetValueAsString(parm["companyId"]);
            string userId = Prolink.Math.GetValueAsString(parm["userId"]);
            string msg = "success";

            string UPassword = GetRandomString4(8);
            string sql = "SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(PartyNo) + " AND STATUS='U'";
            string PartyName = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            EvenFactory.AddEven(UId + "#" + PartyName + "#" + UName + "#" + UPassword + "#" + groupId + "#" + companyId + "#" + c + "#" + Guid.NewGuid().ToString(), UId, MailManager.UserInfo, null, 1, 0, UEmail, "User Infomation", "");
            MixedList ml = new MixedList();
            UPassword = genMD5(UPassword);

            EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", UId);
            ei.PutKey("GROUP_ID", g);
            ei.PutKey("CMP", c);
            ei.Put("U_PASSWORD", UPassword);
            int date = 90;
            if (!string.IsNullOrEmpty(ModiPwDate))
                date = int.Parse(ModiPwDate);
            DateTime d = TimeZoneHelper.GetTimeZoneDate(companyId).AddDays(date);
            ei.PutDate("UPDATE_PRI_DATE", d);
            ei.Put("MODIFY_BY", userId);
            ei.PutDate("MODIFY_DATE", TimeZoneHelper.GetTimeZoneDate(companyId));
            ml.Add(ei);
            EditInstruct log = new EditInstruct("SYS_ACCT_LOG", EditInstruct.INSERT_OPERATION);
            log.Put("GROUP_ID", "TPV");
            log.Put("CMP", c);
            log.Put("USER_ID", UId);
            log.Put("FIELD_CODE", "U_PASSWORD");
            log.Put("FIELD_NAME", "重置密码");
            log.PutDate("MODIFY_DATE", DateTime.Now);
            log.Put("MODIFY_BY", UserId);
            ml.Add(log);
            try
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                int recordsCount = 0, pageIndex = 1, pageSize = 20;
                DataTable dt = ModelFactory.InquiryData("*", "SYS_ACCT", string.Format("U_ID={0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr(g), SQLUtils.QuotedStr(c)), "", pageIndex, pageSize, ref recordsCount);
                userData = ModelFactory.ToTableJson(dt, "SysAcctModel");
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }
            return msg;
        }
        #endregion

        #region 檢查user是否存在
        public bool chkUserExsit(string UserId, string Cmp)
        {
            string sql = "SELECT COUNT(*) FROM SYS_ACCT WHERE U_ID=" + SQLUtils.QuotedStr(UserId) + " AND CMP=" + SQLUtils.QuotedStr(Cmp);
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (num > 0)
            {
                return true;
            }

            return false;
        }

        public bool chkSapIdExist(string SapId,string UserId="")
        {
            if (string.IsNullOrEmpty(SapId)) return false;
            string sql = "SELECT COUNT(*) FROM SYS_ACCT WHERE U_ID !=" + SQLUtils.QuotedStr(UserId) + " AND SAP_ID like '%" + SapId + "%'";
            if (string.IsNullOrEmpty(UserId))
            {
                sql="SELECT COUNT(*) FROM SYS_ACCT WHERE SAP_ID like '%" + SapId + "%'";
            }
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (num > 0)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 圖章上傳
        public JsonResult picFileUpload()
        {
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string rootPath = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~/zh-CN/System/Image/")); ;
            
            if(UId != "")
            {
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    returnMessage = @Resources.Locale.L_System_Controllers_480;
                    return Json(new { message = returnMessage });
                }
                else
                {
                    string path = Server.MapPath("~/FileUploads/") + UId + "/";
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.ToLower();
                    strExt=strExt.Replace(".","");
                    if (!strExt.EndsWith("jpg") && !strExt.EndsWith("png"))
                    {
                        returnMessage = @Resources.Locale.L_SystemController_Controllers_213;
                        return Json(new { message = returnMessage });
                    }
                    try
                    {
                        //string strExt = file.FileName.Split('.')[1].ToUpper();
                        string FileName = string.Format("{0}.{1}", UId, strExt);

                        // Determine whether the directory exists.
                        if (Directory.Exists(path))
                        {
                            file.SaveAs(path + FileName);
                        }
                        else
                        {
                            // Try to create the directory.
                            DirectoryInfo di = Directory.CreateDirectory(path);
                            file.SaveAs(path + FileName);
                        }

                        if (UId != "")
                        {
                            string sql = "UPDATE SMPTY SET PIC=" + SQLUtils.QuotedStr(path + FileName) + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception e)
                            {
                                returnMessage = e.ToString();
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        returnMessage = e.ToString();
                    }

                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_SystemController_Controllers_215;
            }
            
            return Json(new {message = returnMessage });
        }
        public ActionResult Image(string id)
        {
            var dir = Server.MapPath("~/FileUploads/" + id + "/");
            var path = Path.Combine(dir, id + ".jpg");
            return base.File(path, "image/jpeg");
        }
        public JsonResult delPic()
        {
            string UId = Request.Params["UId"];
            string returnMessage = "success";
            string sql = "UPDATE SMPTY SET PIC='' WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception e)
            {
                returnMessage = e.ToString();
            }

            return Json(new { message = returnMessage });
        }
        #endregion

        public static string GetRandomString4(int length)
        {
            var str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var next = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                builder.Append(str[next.Next(0, str.Length)]);
            }
            return builder.ToString();
        }

        public JsonResult SynchronizationProfile()
        {
            string sapId = Business.TPV.Helper.GetSapId(CompanyId);
            if (string.IsNullOrEmpty(sapId)) return Json(new { msg = @Resources.Locale.L_SystemController_Controllers_216});
            string profileCode = Request.Params["ProfileCode"];
            ProfileManager profileManager = new ProfileManager();
            ResultInfo result = profileManager.Import(sapId, profileCode,"");
            if (!result.IsSucceed)
                return Json(new { errMsg = result.Description });
            return Json(new { });
        }

        public JsonResult GetPartner()
        {
            string sapId = Business.TPV.Helper.GetSapId(CompanyId);
            if (string.IsNullOrEmpty(sapId)) return Json(new { msg = @Resources.Locale.L_SystemController_Controllers_216 });
            string partyNo = Request.Params["PartyNo"];
            PartnerManager pm = new PartnerManager();
            ResultInfo result = pm.Import(sapId, CompanyId, partyNo);
            return Json(new { scceed = result.IsSucceed, msg = result.Description });
        }

        #region 基本方法
        static Dictionary<string, object> SchemasCache = new Dictionary<string, object>();
        public void SetSchema(string name)
        {
            if (!SchemasCache.ContainsKey(name))
            {
                List<string> kyes = null;
                string sql = string.Empty;
                switch (name)
                {
                    case "CntySetup":
                        kyes = new List<string> { "U_ID" };
                        sql = "SELECT * FROM BSCNTY WHERE 1=0";
                        break;
                }
                SchemasCache[name] = ToContent(ModelFactory.GetSchemaBySql(sql, kyes));
            }
            ViewBag.schemas = "[]";
            if (SchemasCache.ContainsKey(name))
                ViewBag.schemas = SchemasCache[name];
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

        private ActionResult GetBaseData(string table, string condition, string colNames = "*", string orderBy = "", string qType = "")
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            switch (qType)
            {
                case "1":
                    dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
                    break;
                case "2":
                    dt = ModelFactory.InquiryData("*", table, condition, orderBy, pageIndex, pageSize, ref  recordsCount);
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

        #endregion

        public ActionResult IpLogQueryData()
        {
            string condition = GetCreateDateCondition("IP_LOG", "");
            return GetBootstrapData("IP_LOG", condition);
        }


        public ActionResult SCMReqSetup()
        {
            ViewBag.pmsList = GetBtnPms("SCMReqSetup");
            return View();
        }

        public ActionResult SCMReqQuery()
        {
            string condition = string.Empty;
            condition = GetBaseCmp();
            if ("G".Equals(UPri))
                condition = GetBaseGroup();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SCMREF", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ScmrefModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult SCMReqUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ScmrefModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                        }
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
                }
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult InboundBillPostSetup()
        {
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("InboundBillPostSetup");
            return View();
        }

        public ActionResult InboundPostBillQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SCMPB", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            data["main"] = ModelFactory.ToTableJson(dt, "ScmpbModel");
            return ToContent(data);
        }

        //过账建档
        public ActionResult InboundPostBillUpdate()
        {
            string cmp = Request.Params["Cmp"];
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ScmpbModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_Id", Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", cmp);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
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
                }
            }
            return Json(new { Cmp = cmp, message = returnMessage });
        }


        public ActionResult CostAllocationRuleSetup()
        {
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("CostAllocationRule");
            return View();
        }

        public ActionResult CostAllocationRuleQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SCALLO", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            data["main"] = ModelFactory.ToTableJson(dt, "ScalloModel");
            return ToContent(data);
        }

        //过账建档
        public ActionResult CostAllocationRuleUpdate()
        {
            string cmp = Request.Params["Cmp"];
            string changeData = Request.Params["changedData"];
            string uid = string.Empty;
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ScalloModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_Id", Guid.NewGuid().ToString());
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", cmp);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                        }
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
                }
            }
            return Json(new { Cmp = cmp, message = returnMessage });
        }

        public ActionResult GetMenuItemByRole()
        {
            string data = Request["allData"];
            string[] roleValue = data.Split(';');
            List<string> list = new List<string>();
            Dictionary<string, string> roleKey = new Dictionary<string, string>();
            List<Dictionary<string, string>> roleKeys = new List<Dictionary<string, string>>();
            string GroupId = string.Empty, roleId = string.Empty, Station = string.Empty, CompanyId = string.Empty;
            string sql = "";
            foreach (string val in roleValue)
            {
                roleKey = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(sql))
                    sql += " UNION ALL ";
                string[] values = val.Split('_');
                if (values.Length > 3)
                {
                    roleKey["GroupId"] = values[0];
                    roleKey["CompanyId"] = values[1];
                    roleKey["Station"] = values[2];
                    roleKey["roleId"] = values[3];
                    for (int i = 4; i < values.Length; i++)
                    {
                        roleKey["roleId"] += "_" + values[i];
                    }
                }
                else
                    continue;
                sql += string.Format(" SELECT FOBJ_ID FROM SYS_ROLE_OBJ_PMS WHERE FROLE_ID={0} AND GROUP_ID={1} AND CMP={2} AND STN={3} ",
                    SQLUtils.QuotedStr(roleKey["roleId"]), SQLUtils.QuotedStr(roleKey["GroupId"]), SQLUtils.QuotedStr(roleKey["CompanyId"]), SQLUtils.QuotedStr(roleKey["Station"]));
                roleKeys.Add(roleKey);
            }
            if (!string.IsNullOrEmpty(sql))
            {
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in dt.Rows)
                {
                    string fobjId = Prolink.Math.GetValueAsString(dr["FOBJ_ID"]);
                    if (!list.Contains(fobjId))
                        list.Add(fobjId);
                }
            }
            string lang = "zh-CN";
            string path = MenuManager.GetMenuPath(Prolink.Web.WebContext.GetInstance());
            string menuName = "GB_MENU.xml";
            switch (SiteLang)
            {
                case "zh-TW":
                    menuName = "BIG5_MENU.xml";
                    lang = SiteLang;
                    break;
                case "en-US":
                    menuName = "ENG_MENU.xml";
                    lang = SiteLang;
                    break;
            }
            string menuPath = System.IO.Path.Combine(path, menuName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(menuPath);
            XmlNodeList xnl = xmlDoc.SelectNodes("/menu/menu");
            List<string> pmsIdList = new List<string>();
            List<Dictionary<string, object>> firstLevel = new List<Dictionary<string, object>>();
            Dictionary<string, object> level = new Dictionary<string, object>();
            List<Dictionary<string, object>> secondLevel = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            Dictionary<string, object> pmsResult = new Dictionary<string, object>();
            Dictionary<string, object> roleResult = new Dictionary<string, object>();
            Dictionary<string, object> menuResult = new Dictionary<string, object>();
            List<Dictionary<string, object>> pmsList = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> roleList = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> menuList = new List<Dictionary<string, object>>();
            foreach (XmlNode xn in xnl)
            {
                level = new Dictionary<string, object>();
                level["text"] = "";
                if (xn.FirstChild.Value.Split('@').Length > 0)
                    level["text"] = xn.FirstChild.Value.Split('@')[0].ToString().Replace("\r\n", "");
                level["href"] = "#";
                level["icon"] = "glyphicon glyphicon-folder-open";
                secondLevel = new List<Dictionary<string, object>>();
                XmlNodeList menus = xn.SelectNodes("menu");
                foreach (XmlNode menu in menus)
                {
                    item = new Dictionary<string, object>();
                    item["text"] = menu.FirstChild.Value;
                    item["href"] = GetObjectValue(menu.Attributes, "pmsId");
                    secondLevel.Add(item);
                }
                level["nodes"] = secondLevel;
                firstLevel.Add(level);
            }
            CultureInfo cul = CultureInfo.CreateSpecificCulture(lang);
            int num = 0;
            foreach (Dictionary<string, object> jObject in firstLevel)
            {
                num++;
                roleList = new List<Dictionary<string, object>>();
                menuResult = new Dictionary<string, object>();
                string hText = jObject["text"].ToString();
                List<Dictionary<string, object>> jArray = (List<Dictionary<string, object>>)jObject["nodes"];
                foreach (var jList in jArray)
                {
                    roleResult = new Dictionary<string, object>();
                    pmsList = new List<Dictionary<string, object>>();
                    pmsIdList = new List<string>();
                    string id = jList["href"].ToString();
                    string text = jList["text"].ToString();
                    if (!list.Contains(id))
                        continue;
                    foreach (Dictionary<string, string> dic in roleKeys)
                    {
                        Permission p = PermissionManager.GetMenuItemPermissionByRole(dic["roleId"] + "|" + dic["GroupId"] + "|" + dic["CompanyId"] + "|" + dic["Station"], id);
                        if (p != null)
                        {
                            foreach (var kv in p.Children)
                            {
                                pmsResult = new Dictionary<string, object>();
                                if (!"Y".Equals(kv.Value.Allowed))
                                    continue;
                                if (kv.Value.Text.IndexOf("TLB_") > -1)
                                {
                                    pmsResult["caption"] = resources.GetString(kv.Value.Text, cul);
                                }
                                else
                                    pmsResult["caption"] = kv.Value.Text;
                                pmsResult["pmsId"] = kv.Value.PmsId;
                                if (!pmsIdList.Contains(kv.Value.PmsId))
                                {
                                    pmsIdList.Add(kv.Value.PmsId);
                                    pmsList.Add(pmsResult);
                                }
                            }
                        }
                    }
                    roleResult["roleId"] = id;
                    roleResult["roleText"] = text;
                    roleResult["permision"] = pmsList;
                    roleList.Add(roleResult);
                }
                menuResult["hId"] = num.ToString();
                menuResult["hText"] = hText;
                menuResult["menu"] = roleList;
                menuList.Add(menuResult);
            }
            return ToContent(menuList);
        }
        public ActionResult SaveUserPermission()
        {
            string returnMsg = "success";
            string delRole = Prolink.Math.GetValueAsString(Request["delRoles"]);
            string newRole = Prolink.Math.GetValueAsString(Request["newRoles"]);
            string GroupId = Prolink.Math.GetValueAsString(Request["GroupId"]);
            string uid = Prolink.Math.GetValueAsString(Request["UId"]);
            string cmp = Prolink.Math.GetValueAsString(Request["Cmp"]);
            string stn = Prolink.Math.GetValueAsString(Request["Stn"]);
            string sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0} AND GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(cmp));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count==0)
                return Json(new { message = @Resources.Locale.L_ForecastQueryData_Views_243 });
            string[] delRoles = delRole.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] addRoles = newRole.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            MixedList ml = new MixedList();
            UserRoleUpdate(ml, delRoles, uid, cmp, stn, true);
            UserRoleUpdate(ml, addRoles, uid, cmp, stn);
            DateTime odt = DateTime.Now;
            DateTime ndt = Business.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            if (ml.Count > 0)
            {
                EditInstruct logEi = new EditInstruct("SYS_ROLE_LOG", EditInstruct.INSERT_OPERATION);
                logEi.Put("GROUP_ID", GroupId);
                logEi.Put("CMP", cmp);
                logEi.Put("STN", stn);
                logEi.Put("FROLE_ID", uid);
                logEi.Put("ROLE_TYPE", "user");
                logEi.PutDate("CREATE_DATE", odt);
                logEi.Put("CREATE_BY", UserId);
                logEi.Put("ROLE_ADD", newRole);
                logEi.Put("ROLE_DEL", delRole);
                EditInstruct userEi = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
                userEi.PutKey("U_ID", uid);
                userEi.PutKey("GROUP_ID", GroupId);
                userEi.PutKey("CMP", cmp);
                userEi.PutDate("PERMISSION_DATE", odt);
                userEi.PutDate("PERMISSION_DATE_L", ndt);
                userEi.Put("PERMISSION_BY", UserId);
                ml.Add(userEi);
                ml.Add(logEi);
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMsg = ex.ToString();
                }
            }
            string myRole = getUserRole(uid, GroupId, cmp);
            return Json(new { message = returnMsg, userRole = myRole });
        }
        public void UserRoleUpdate(MixedList ml, string[] userRoles, string uid, string ucmp, string ustn, bool isDelete = false)
        {
            EditInstruct ei = null;
            string roleId = "";
            foreach (string role in userRoles)
            {
                ei = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.INSERT_OPERATION);
                if (isDelete)
                    ei = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.DELETE_OPERATION);
                string[] values = role.Split('_');
                if (values.Length > 3)
                {
                    ei.PutKey("FACCT_ID", uid);
                    ei.PutKey("CMP", ucmp);
                    ei.PutKey("STN", ustn);
                    ei.PutKey("GROUP_ID", values[0]);
                    ei.PutKey("RCMP", values[1]);
                    ei.PutKey("RSTN", values[2]);
                    roleId = values[3];
                    for (int i = 4; i < values.Length; i++)
                    {
                        roleId += "_" + values[i];
                    }
                    ei.PutKey("FROLE_ID", roleId);
                    ml.Add(ei);
                }
            }
        }

        [HttpPost]
        public ActionResult UploadCustomer(FormCollection form)
        {
            string returnMessage = "success";
            if (Request.Files.Count == 0)
            {
                return Json(new { message = "error" });
            }
            try
            {
                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { message = "error" });
                }
                else
                {
                    string strExt = System.IO.Path.GetExtension(file.FileName);
                    strExt = strExt.Trim('.');
                    string dirpath = GetDirPath(Server.MapPath("~/FileUploadsNew/") + "UploadCustomer\\");
                    string excelFileName = string.Format("{0}{1}.{2}", dirpath, TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss"), strExt);

                    file.SaveAs(excelFileName);
                    int starRow = 0;
                    string mapping = "CustomerMapping";
                    MixedList ml = new MixedList();
                    MixedList mlist = new MixedList();
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    parm["CREATE_BY"] = UserId;
                    parm["CREATE_DATE"] = TimeZoneHelper.GetTimeZoneDate(CompanyId).ToString("yyyyMMddHHmmss");
                    parm["GROUP_ID"] = GroupId;
                    parm["CMP"] = CompanyId;
                    parm["STN"] = Station;
                    parm["DEP"] = Dep;
                    parm["PARTY_NO"] = GetAutoPartyNo(GroupId);
                    ExcelParser.RegisterEditInstructFunc(mapping, HandleCustomer);
                    ExcelParser ep = new ExcelParser();
                    ep.Save(mapping, excelFileName, ml, parm, starRow);
                    if (ml.Count <= 0)
                    {
                        returnMessage = @Resources.Locale.L_BookingAction_Controllers_203;
                    }
                    else
                    {
                        ml = CheckPartyNo(ml, parm);
                        try
                        {
                            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        catch (Exception ex)
                        {
                            returnMessage = ex.ToString();
                            return Json(new { message = ex.Message, message1 = returnMessage });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnMessage = ex.Message;
            }
            return Json(new { message = returnMessage });
        }

        /// <summary>
        /// 导入多条数据时,去除重复客户代码
        /// </summary>
        /// <param name="ml"></param>
        /// <returns></returns>
        public MixedList CheckPartyNo(MixedList ml, Dictionary<string, object> parm)
        {
            string partyNo = Prolink.Math.GetValueAsString(parm["PARTY_NO"]);
            for (int i = 0; i < ml.Count; i++)
            {
                EditInstruct ei = ml[i] as EditInstruct;
                ei.Put("PARTY_NO", partyNo);
                ei.Put("HEAD_OFFICE", partyNo);
                ei.Put("BILL_TO", partyNo);

                int num = int.Parse(partyNo.Substring(7)) + 1;
                partyNo = partyNo.Substring(0, 7) + num.ToString().PadLeft(3, '0');
            }
            return ml;
        }

        public static string HandleCustomer(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string createBy = Prolink.Math.GetValueAsString(parm["CREATE_BY"]);
            DateTime? createDate = Prolink.Math.GetValueAsDateTime(parm["CREATE_DATE"]);
            string groupId = Prolink.Math.GetValueAsString(parm["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(parm["CMP"]);
            string stn = Prolink.Math.GetValueAsString(parm["STN"]);
            string dep = Prolink.Math.GetValueAsString(parm["DEP"]);
            string partyNo = Prolink.Math.GetValueAsString(parm["PARTY_NO"]);

            ei.Put("PARTY_NO", partyNo);
            ei.Put("CREATE_BY", createBy);
            ei.PutDate("CREATE_DATE", createDate);
            ei.Put("GROUP_ID", groupId);
            ei.Put("CMP", cmp);
            ei.Put("STN", stn);
            ei.Put("DEP", dep);
            ei.PutKey("U_ID", System.Guid.NewGuid().ToString("N"));
            ei.Put("HEAD_OFFICE", partyNo);
            ei.Put("BILL_TO", partyNo);
            ei.Put("STATUS", "U");
            ei.Put("FILTER", "N");

            string partyName = Prolink.Math.GetValueAsString(ei.Get("PARTY_NAME"));
            if (string.IsNullOrEmpty(partyNo)) throw new Exception("Party Code自动生成失败请重新导入!");
            if (string.IsNullOrEmpty(partyName)) throw new Exception("Party Name1不可为空!");

            string sql;
            string msg = string.Empty;
            DataTable dt;
            string cnty = Prolink.Math.GetValueAsString(ei.Get("CNTY"));
            bool cntyFlag = true;
            if (!string.IsNullOrEmpty(cnty))
            {
                sql = string.Format("SELECT CNTRY_NM FROM BSCNTY WHERE GROUP_ID={0} AND CMP={1} AND CNTRY_CD={2}", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(cnty));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0)
                {
                    msg += string.Format("Party Name1:{0} 的County:{1} cannot match it from set up！", partyName, cnty);
                    cntyFlag = false;
                }
                else
                {
                    ei.Put("CNTY_NM", dt.Rows[0]["CNTRY_NM"]);
                    cntyFlag = true;
                }
            }

            string city = Prolink.Math.GetValueAsString(ei.Get("CITY"));
            if (!string.IsNullOrEmpty(city) && cntyFlag)
            {
                sql = string.Format("SELECT PORT_NM FROM BSCITY WHERE GROUP_ID={0} AND CMP={1} AND CNTRY_CD={2} AND PORT_CD={3}",
                SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(cnty), SQLUtils.QuotedStr(city));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0)
                {
                    msg += string.Format("Party Name1:{0} 的City:{1} cannot match it from set up！", partyName, city);
                }
                else
                {
                    ei.Put("CITY_NM", dt.Rows[0]["PORT_NM"]);
                }
            }
            string state = Prolink.Math.GetValueAsString(ei.Get("STATE"));
            if (!string.IsNullOrEmpty(state))
            {
                sql = string.Format("SELECT 1 FROM BSSTATE WHERE GROUP_ID={0} AND CMP={1} AND STATE_CD={2}", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(state));
                if (cntyFlag)
                    sql += string.Format(" AND CNTRY_CD={0}", SQLUtils.QuotedStr(cnty));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0)
                {
                    msg += string.Format("Party Name1:{0} 的State/Province:{1} cannot match it from set up！", partyName, state);
                }
            }

            if (!string.IsNullOrEmpty(msg))
            {
                throw new Exception(msg);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取客户代码流水号
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPartyNo()
        {
            string groupId = string.IsNullOrEmpty(GroupId) ? "TPV" : GroupId;
            string partyNo = GetAutoPartyNo(groupId);
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["partyNo"] = partyNo;
            list.Add(item);
            return ToContent(list);
        }

        /// <summary>
        /// 自动生成客户代码 固定值：ESP，年份：YY，月份：MM，流水号：3码 例：ESP0826000
        /// </summary>
        /// <returns></returns>
        public static string GetAutoPartyNo(string groupId)
        {
            DateTime date = DateTime.Now;
            string time = date.Year.ToString().Substring(2, 2).PadLeft(2, '0') + date.Month.ToString().PadLeft(2, '0');
            string firstNo = "ESP" + time;
            string lastNo = "000";
            int num;
            string sql = string.Format("SELECT PARTY_NO FROM SMPTY WHERE GROUP_ID={0} AND PARTY_NO LIKE{1} ORDER BY SMPTY.PARTY_NO DESC",
                SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(firstNo + "%"));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                lastNo = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_NO"]).Substring(7);
                num = int.Parse(lastNo) + 1;
                lastNo = num.ToString().PadLeft(3, '0');
            }
            string partyNo = firstNo + lastNo;
            return partyNo;
        }

        public JsonResult UserResume()
        {
            string UId = Request.Params["UId"];
            string Cmp = Request.Params["Cmp"];
            if (string.IsNullOrEmpty(UId) || string.IsNullOrEmpty(Cmp))
                return Json(new { message = "error" });
            try
            {
                EditInstruct ei = new EditInstruct("SYS_ACCT", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", UId);
                ei.PutKey("CMP", Cmp);
                ei.Put("U_STATUS", 0);
                ei.PutDate("RESUME_DATE", DateTime.Now);
                ei.Put("MODIFY_BY", UserId);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
                ei.Put("CARD_NO", null);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }
            return Json(new { message = "success" });
        }

        
        public ActionResult GetRoleLog()
        {
            string UId = Request["UId"];
            string Cmp = Request["Cmp"];
            string condition = string.Format("FROLE_ID = {0} AND ROLE_TYPE={1} AND CMP={2}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr("user"), SQLUtils.QuotedStr(Cmp));
            string sql1 = string.Format("SELECT * FROM SYS_ROLE_LOG WHERE {0} ORDER BY CREATE_DATE DESC", condition);
            DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Func<string, string> GetRoleDescp = (val) => {
                string descp = string.Empty;
                if (!string.IsNullOrEmpty(val))
                {
                    string[] roles = val.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string role in roles)
                    {
                        string[] rolede = role.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                        string sql = string.Format("SELECT TOP 1 FDESCP FROM SYS_ROLE WHERE FID={0} AND GROUP_ID={1} AND CMP={2}",
                            SQLUtils.QuotedStr(rolede[3]), SQLUtils.QuotedStr(rolede[0]), SQLUtils.QuotedStr(rolede[1]));
                        string descptemp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (string.IsNullOrEmpty(descptemp))
                        {
                            sql = string.Format("SELECT TOP 1 FDESCP FROM SYS_ROLE WHERE FID={0} AND GROUP_ID={1}",
                               SQLUtils.QuotedStr(rolede[3]), SQLUtils.QuotedStr(rolede[0]));
                            descptemp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }

                        descp += string.IsNullOrEmpty(descptemp) ? rolede[3] : descptemp + ";";
                    }
                }
                return descp;
            };
            SetMaxLen(dt);
            foreach (DataRow dr in dt.Rows)
            {
                string role = Prolink.Math.GetValueAsString(dr["ROLE_ADD"]);
                string role2 = Prolink.Math.GetValueAsString(dr["ROLE_DEL"]);
                dr["ROLE_ADD"] = GetRoleDescp(role);
                dr["ROLE_DEL"] = GetRoleDescp(role2);
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            return ToContent(ModelFactory.ToTableJson(dt));
        }

        public ActionResult GetRoleAuthorityLog()
        {
            string UId = Request["UId"];
            string Cmp = Request["Cmp"];
            string condition = string.Format("FROLE_ID = {0} AND ROLE_TYPE={1} AND CMP={2}", SQLUtils.QuotedStr(UId), SQLUtils.QuotedStr("Permission"), SQLUtils.QuotedStr(Cmp));
            string sql1 = string.Format("SELECT * FROM SYS_ROLE_LOG WHERE {0} ORDER BY CREATE_DATE DESC", condition);
            DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (_pmsList == null)
                SetPMSList();
            Dictionary<string, Dictionary<string, string>> AllpmsList = _pmsList;
            DataTable edtDt = OperationUtils.GetDataTable("SELECT DISTINCT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE = 'EDT'", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in edtDt.Rows)
            {
                Dictionary<string, string> docpms = new Dictionary<string, string>();
                if (AllpmsList.ContainsKey("EDOC"))
                {
                    docpms = AllpmsList["EDOC"];
                }
                else
                {
                    AllpmsList.Add("EDOC", docpms);
                }
                string key = "EDOC_EDT_U_" + Prolink.Math.GetValueAsString(dr["CD"]);
                if (!docpms.ContainsKey(key))
                {
                    docpms.Add("EDOC_EDT_U_" + Prolink.Math.GetValueAsString(dr["CD"]), Prolink.Math.GetValueAsString(dr["CD_DESCP"]) + "(Upload)");
                    docpms.Add("EDOC_EDT_D_" + Prolink.Math.GetValueAsString(dr["CD"]), Prolink.Math.GetValueAsString(dr["CD_DESCP"]) + "(Delete)");
                    docpms.Add("EDOC_EDT_V_" + Prolink.Math.GetValueAsString(dr["CD"]), Prolink.Math.GetValueAsString(dr["CD_DESCP"]) + "(View)");
                }
            }
            string lang = "zh-CN";
            string path = MenuManager.GetMenuPath(Prolink.Web.WebContext.GetInstance());
            string menuName = "GB_MENU.xml";
            switch (SiteLang)
            {
                case "zh-TW":
                    menuName = "BIG5_MENU.xml";
                    lang = SiteLang;
                    break;
                case "en-US":
                    menuName = "ENG_MENU.xml";
                    lang = SiteLang;
                    break;
            }
            string menuPath = System.IO.Path.Combine(path, menuName);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(menuPath);
            XmlNodeList xnl = xmlDoc.SelectNodes("/menu/menu");
            Dictionary<string, object> level = null;
            List<Dictionary<string, object>> firstLevel = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> secondLevel = new List<Dictionary<string, object>>();
            CultureInfo cul = CultureInfo.CreateSpecificCulture(lang);
            foreach (XmlNode xn in xnl)
            {
                level = new Dictionary<string, object>();
                level["text"] = "";
                if (xn.FirstChild.Value.Split('@').Length > 0)
                    level["text"] = xn.FirstChild.Value.Split('@')[0].ToString().Replace("\r\n", "");
                level["href"] = "#";
                level["icon"] = "glyphicon glyphicon-folder-open";
                secondLevel = new List<Dictionary<string, object>>();
                XmlNodeList menus = xn.SelectNodes("menu");
                foreach (XmlNode menu in menus)
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["text"] = menu.FirstChild.Value;
                    item["href"] = GetObjectValue(menu.Attributes, "pmsId");
                    secondLevel.Add(item);
                }
                level["nodes"] = secondLevel;
                firstLevel.Add(level);
            }
            Func<string, string> GetMenuDescp = (roleid) => {
                string roledesp = roleid;
                if (firstLevel != null)
                {
                    foreach (Dictionary<string, object> menuitem in firstLevel)
                    {
                        if (menuitem.ContainsKey("nodes"))
                        {
                            foreach (var menukey in menuitem["nodes"] as List<Dictionary<string, object>>)
                            {
                                string href = Prolink.Math.GetValueAsString(menukey["href"]);
                                if (href == roleid)
                                {
                                    return Prolink.Math.GetValueAsString(menukey["text"]);
                                }
                            }
                        }
                    }
                }
                return roledesp;
            };

            Func<string, string, string> GetPmsDescp = (roleid, Pmsid) => {
                string Pmsdesp = Pmsid;
                if (AllpmsList.ContainsKey(roleid))
                {
                    Dictionary<string, string> pmsList = AllpmsList[roleid];
                    foreach (var pmsItem in pmsList)
                    {
                        if (pmsItem.Key == Pmsid)
                        {
                            Pmsdesp = pmsItem.Value; break;
                        }
                    }
                }
                if (Pmsdesp.IndexOf("TLB_") > -1)
                {
                    Pmsdesp = resources.GetString(Pmsdesp, cul);
                }
                return Pmsdesp;
            };
            Func<string, string> GetRoleDescp = (val) => {
                string descp = string.Empty;
                Dictionary<string, List<string>> roledic = new Dictionary<string, List<string>>();
                if (!string.IsNullOrEmpty(val))
                {
                    string[] roles = val.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string role in roles)
                    {
                        string[] rolede = role.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                        string roleid = rolede[0];
                        string roledesp = GetMenuDescp(roleid);
                        List<string> pmslist = new List<string>();
                        if (roledic.ContainsKey(roledesp))
                        {
                            pmslist = roledic[roledesp];
                        }
                        else
                        {
                            roledic.Add(roledesp, pmslist);
                        }
                        pmslist.Add(GetPmsDescp(roleid, role));
                    }
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    descp = js.Serialize(roledic);
                }
                return descp;
            };
            SetMaxLen(dt);
            foreach (DataRow dr in dt.Rows)
            {
                string role = Prolink.Math.GetValueAsString(dr["ROLE_ADD"]);
                string role2 = Prolink.Math.GetValueAsString(dr["ROLE_DEL"]);
                dr["ROLE_ADD"] = GetRoleDescp(role);
                dr["ROLE_DEL"] = GetRoleDescp(role2);
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            return ToContent(ModelFactory.ToTableJson(dt));
        }

        public void SetPMSList()
        {
            _pmsList = new Dictionary<string, Dictionary<string, string>>();
            string path = MenuManager.GetPMSPath(Prolink.Web.WebContext.GetInstance());
            //string menuPath = System.IO.Path.Combine(path, "PMS_OBJ.XML");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList xnl = xmlDoc.SelectNodes("/list/programe-list/object");
            foreach (XmlNode xn in xnl)
            {
                string mpmsId = GetObjectValue(xn.Attributes, "pmsId");
                if (_pmsList.ContainsKey(mpmsId))
                    continue;
                Dictionary<string, string> childNode = new Dictionary<string, string>();
                XmlNodeList permissions = xn.SelectNodes("object");
                foreach (XmlNode pms in permissions)
                {
                    string pmsId = GetObjectValue(pms.Attributes, "pmsId");
                    string languageId = GetObjectValue(pms.Attributes, "languageId");
                    if (!childNode.ContainsKey(pmsId))
                        childNode.Add(pmsId, languageId);
                }
                if (childNode.Count > 0)
                    _pmsList.Add(mpmsId, childNode);
            }
        }

        public void SetMaxLen(DataTable dt)
        {
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName == "ROLE_ADD" || col.ColumnName == "ROLE_DEL")
                {
                    col.MaxLength = int.MaxValue;
                }
            }
        }


        public JsonResult PermissionCopy()
        {
            string UId = Request.Params["UId"];
            string Cmp = Request.Params["Cmp"];
            string CopyAccount = Request.Params["CopyAccount"];
            string CopyCmp = Request.Params["CopyCmp"];
            string copyItsd = Request.Params["CopyItsd"];
            if (string.IsNullOrEmpty(UId) || string.IsNullOrEmpty(Cmp))
                return Json(new { message = "error" });
            MixedList ml = new MixedList();
            try
            {
                string sql = string.Format(@"SELECT * FROM SYS_ACCT_ROLE WHERE FACCT_ID={0} AND CMP={1}",
                    SQLUtils.QuotedStr(CopyAccount), SQLUtils.QuotedStr(CopyCmp));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count <= 0) return Json(new { message = "No Data" });

                EditInstruct ei = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.DELETE_OPERATION);
                ei.PutKey("FACCT_ID", UId);
                ei.PutKey("CMP", Cmp);
                ml.Add(ei);

                foreach (DataRow dr in dt.Rows)
                {
                    ei = new EditInstruct("SYS_ACCT_ROLE", EditInstruct.INSERT_OPERATION);
                    ei.Put("FACCT_ID", UId);
                    ei.Put("FROLE_ID", dr["FROLE_ID"]);
                    ei.Put("GROUP_ID", "TPV");
                    ei.Put("CMP", Cmp);
                    ei.Put("STN", "*");
                    ei.Put("RCMP", Cmp);
                    ei.Put("RSTN", "*");
                    ml.Add(ei);
                }

                EditInstruct log = new EditInstruct("SYS_ACCT_LOG", EditInstruct.INSERT_OPERATION);
                log.Put("GROUP_ID", "TPV");
                log.Put("CMP", Cmp);
                log.Put("USER_ID", UId);
                log.Put("FIELD_NAME", "复制权限");
                log.Put("UPDATE_VALUE", "从账号:" + CopyAccount + "(" + CopyCmp + ")" + "复制权限");
                log.PutDate("MODIFY_DATE", DateTime.Now);
                log.Put("MODIFY_BY", UserId);
                log.Put("IT_SD", copyItsd);
                ml.Add(log);

                if (ml.Count > 0)
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message });
            }
            return Json(new { message = "success" });
        }


        public ActionResult GetItsdAr()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;

            string resultType = Request.Params["resultType"];

            DataTable dt = new DataTable("ITSD_AR");
            dt.Columns.Add("IT_SD", typeof(string));
            dt.Columns.Add("REQUEST_DATE", typeof(DateTime));
            dt.Columns.Add("ITSD_STATUS", typeof(string));
            dt.Columns.Add("MODEL", typeof(string));
            dt.Columns.Add("FUNCTION_CLASS", typeof(string));
            dt.Columns.Add("SITE", typeof(decimal));

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


        public ActionResult ItsdArGetInfo()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 20;
            string Model = Request.Params["Model"];
            string Itsd = Prolink.Math.GetValueAsString(Request.Params["Itsd"]);
            string FunctionClass = Prolink.Math.GetValueAsString(Request.Params["FunctionClass"]);
            string ItsdStatus = Prolink.Math.GetValueAsString(Request.Params["ItsdStatus"]);
            string RequestDateFrom = Prolink.Math.GetValueAsString(Request.Params["RequestDateFrom"]);
            string RequestDateTo = Prolink.Math.GetValueAsString(Request.Params["RequestDateTo"]);
            string Site = Prolink.Math.GetValueAsString(Request.Params["Site"]);

            ItsdArInfo info = new ItsdArInfo();
            info.module = Model;
            info.functional = FunctionClass;
            info.appDateFrom = Convert.ToDateTime(RequestDateFrom).ToString("yyyy-MM-dd HH:mm:ss");
            info.appDateTo = Convert.ToDateTime(RequestDateTo).ToString("yyyy-MM-dd HH:mm:ss");
            info.appNo = Itsd;
            if (!string.IsNullOrEmpty(ItsdStatus))
                info.state = Prolink.Math.GetValueAsInt(ItsdStatus);
            info.site = Site;

            ItsdArResult itsdArResult = ApiController.SendToItsdAr(info);

            DataTable dt = new DataTable("ITSD_AR");


            if (itsdArResult.statusCode == "200")
            {
                dt.Columns.Add("IT_SD", typeof(string));
                dt.Columns.Add("APP_DATE", typeof(DateTime));
                dt.Columns.Add("ITSD_STATE", typeof(string));
                dt.Columns.Add("MODULE", typeof(string));
                dt.Columns.Add("FUNCTIONAL", typeof(string));
                dt.Columns.Add("AREA_NAME", typeof(string));
                dt.Columns.Add("APP_CONTENT", typeof(string));
                dt.Columns.Add("U_ID", typeof(string));
                dt.Columns.Add("U_NAME", typeof(string));
                dt.Columns.Add("U_PHONE", typeof(string));
                dt.Columns.Add("U_EXT", typeof(string));
                dt.Columns.Add("U_EMAIL", typeof(string));
                dt.Columns.Add("U_MANAGER", typeof(string));
                dt.Columns.Add("SAP_ID", typeof(string));
                dt.Columns.Add("CARD_NO", typeof(string));
                dt.Columns.Add("CONTENT", typeof(string));
                foreach (var arResult in itsdArResult.data)
                {
                    string appNo = arResult.AppNO;
                    string status = arResult.State;
                    DateTime appDate = Prolink.Math.GetValueAsDateTime(arResult.AppDate);
                    string site = arResult.AreaName;
                    string appContent = arResult.AppContent;
                    foreach (var item in arResult.Items)
                    {
                        DataRow row = dt.NewRow();
                        row["IT_SD"] = appNo;
                        row["APP_DATE"] = appDate;
                        row["ITSD_STATE"] = status;
                        row["MODULE"] = item.Module;
                        row["FUNCTIONAL"] = item.Functional;
                        row["AREA_NAME"] = site;
                        row["APP_CONTENT"] = appContent;
                        row["U_ID"] = item.AD;
                        row["U_NAME"] = item.UserName;
                        row["U_PHONE"] = item.Tel;
                        row["U_EXT"] = item.Extension;
                        row["U_EMAIL"] = item.Email;
                        row["U_MANAGER"] = item.Manager;
                        row["SAP_ID"] = item.SAPId;
                        row["CARD_NO"] = item.CardNo;
                        row["CONTENT"] = item.Content;
                        dt.Rows.Add(row);
                    }
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
            return Json(new { code = itsdArResult.statusCode, msg = "statusCode:" + itsdArResult.statusCode + ",message:" + itsdArResult.message, data = result.ToContent() });
        }

        public ActionResult ArProcess()
        {
            string ArProcessData = Request.Params["ArProcessData"];
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(ArProcessData);
            string sql = string.Empty;
            string result = string.Empty;
            List<string> returnMsg = new List<string>();
            MixedList mixList = new MixedList();
            try
            {
                foreach (var item in dict)
                {
                    ArrayList objList = item.Value as ArrayList;
                    foreach (Dictionary<string, object> json in objList)
                    {

                        ItsdArData data = new ItsdArData();
                        data.AppNO = json["ItSd"].ToString();
                        data.State = json["ItsdState"].ToString();
                        data.AppDate = json["AppDate"].ToString();
                        data.AreaName = json["AreaName"].ToString();
                        data.AppContent = json["AppContent"].ToString();


                        ItsdSubData subData = new ItsdSubData();
                        subData.Module = json["Module"].ToString();
                        subData.Functional = json["Functional"].ToString();
                        subData.AD = json["UId"].ToString();
                        subData.UserName = json["UName"].ToString();
                        subData.Tel = json["UPhone"].ToString();
                        subData.Extension = json["UExt"].ToString();
                        subData.Email = json["UEmail"].ToString();
                        subData.Manager = json["UManager"].ToString();
                        subData.SAPId = json["SapId"].ToString();
                        subData.CardNo = json["CardNo"].ToString();
                        subData.Content = json["Content"].ToString();

                        ApiController.DoItsdAr(data, subData, mixList);
                    }
                }
                OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { IsOk = "N", message = ex.ToString() });
            }
            return Json(new { IsOk = "Y", message = "Success" });
        }

    }
}
