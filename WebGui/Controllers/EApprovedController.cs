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

namespace WebGui.Controllers
{
    public class EApprovedController : BaseController
    {
        //
        // GET: /IPCTM/
        #region View
        public ActionResult ApproveSetup()
        {

            ViewBag.pmsList = GetBtnPms("EA010");
            string condition = " GROUP_ID=" +  SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            ViewBag.AData = GetBootstrapData("APPROVE_ATTRIBUTE", condition);
            return View();
        }

        public ActionResult ApproveGroupSetup()
        {
            ViewBag.pmsList = GetBtnPms("EA020");
            string condition = " GROUP_ID=" +  SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            ViewBag.AData = GetBootstrapData("APPROVE_ATTRIBUTE", condition);
            return View();
        }
        #endregion

        public ActionResult ApproveSignSetup()
        {
            ViewBag.pmsList = GetBtnPms("EA030");
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
            ViewBag.AData = GetBootstrapData("APPROVE_SIGN", condition);
            return View();
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            if (resultType == "count")
            {
                //string statusField = Request.Params["statusField"];
                //dt = GetStatusCountData(statusField, table, condition, Request.Params);
                //pageSize = 1;
            }
            else
            {
                dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
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

        /*獲取approve attr d data*/
        [ValidateInput(false)]
        public JsonResult ApproveDQuery()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string condtions = String.Format("GROUP_ID = {0} AND CMP = {1} AND U_FID = {2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UFid));
            DataTable detailDt = ModelFactory.InquiryData("*", "APPROVE_ATTR_D", condtions, "", pageIndex, pageSize, ref recordsCount);
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

        public ActionResult ApproveSortQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string condition = String.Format("GROUP_ID = {0} AND CMP = {1} AND U_FID = {2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UFid));
            DataTable dt = ModelFactory.InquiryData("*", "APPROVE_ATTR_D", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ApproveAttrDModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        [ValidateInput(false)]
        public JsonResult ApproveDPQuery()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string UFid = Prolink.Math.GetValueAsString(Request.Params["UFid"]);
            string condtions = String.Format("GROUP_ID = {0} AND CMP = {1} AND U_FID = {2}", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(UFid));
            DataTable detailDt = ModelFactory.InquiryData("*", "APPROVE_ATTR_DP", condtions, "", pageIndex, pageSize, ref recordsCount);
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

        public ActionResult ApproveRequiry()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condtions = String.Format("GROUP_ID = {0} AND CMP_ID = {1}",SQLUtils.QuotedStr(GroupId),SQLUtils.QuotedStr(CompanyId));
            DataTable dt = ModelFactory.InquiryData("*", "APPROVE_FLOW_M", condtions, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ApproveModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public JsonResult ApprovedRequiry()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string ufid = Prolink.Math.GetValueAsString(Request.Params["ApproveCode"]);

            string condtions = String.Format("GROUP_ID = {0} AND CMP_ID = {1} AND U_FID = {2} ORDER BY CAST(APPROVE_LEVEL AS INT) ASC",
                SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(CompanyId), SQLUtils.QuotedStr(ufid));
            DataTable detailDt = ModelFactory.InquiryData("*", "APPROVE_FLOW_D", condtions, "", pageIndex, pageSize, ref recordsCount);
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

        public JsonResult ApprovedUpdate()
        {
            string changeData = Request.Params["changedData"];
            string uid = string.Empty, duid = string.Empty;
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApproveModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        uid = Guid.NewGuid().ToString();
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP_ID", CompanyId);
                            ei.Put("SITE_ID", Station);
                            ei.Put("U_ID", uid);
                        }
                        uid = ei.Get("U_ID");
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("U_ID", uid);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("U_ID", uid);
                            EditInstruct ei2 = new EditInstruct("APPROVE_FLOW_D", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("U_FID", uid);
                            mixList.Add(ei2);
                        }
                        mixList.Add(ei);
                    }


                }
                else if (item.Key == "approvedinfo")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApprovedModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            duid = Guid.NewGuid().ToString();
                            ei.PutKey("U_ID", duid);
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP_ID", CompanyId);
                            if (!string.IsNullOrEmpty(uid))
                                ei.Put("U_FID", uid);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            if (ei.Get("U_ID") == null)
                            {
                                continue;
                            }
                            duid = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
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

        public JsonResult SaveEAGroupData()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApproveAttrModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string pk = Prolink.Math.GetValueAsString(ei.Get("U_ID"));

                            if (pk == "")
                            {
                                continue;
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                            
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);

                            EditInstruct ei2 = new EditInstruct("APPROVE_ATTR_D", EditInstruct.DELETE_OPERATION);
                            ei2.PutKey("U_FID", Prolink.Math.GetValueAsString(ei.Get("U_ID")));
                            ei2.PutKey("GROUP_ID", GroupId);
                            ei2.PutKey("CMP", CompanyId);
                            mixList.Add(ei2);

                            EditInstruct ei3 = new EditInstruct("APPROVE_ATTR_DP", EditInstruct.DELETE_OPERATION);
                            ei3.PutKey("U_FFID", Prolink.Math.GetValueAsString(ei.Get("U_ID")));
                            ei3.PutKey("GROUP_ID", GroupId);
                            ei3.PutKey("CMP", CompanyId);
                            mixList.Add(ei3);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st1")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApproveAttrDModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string pk = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            string fk = Prolink.Math.GetValueAsString(ei.Get("U_FID"));
                            string mk = Prolink.Math.GetValueAsString(ei.Get("APPROVE_GROUP"));
                            if (pk == "" || fk == "" || mk == "")
                            {
                                continue;
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);

                            EditInstruct ei3 = new EditInstruct("APPROVE_ATTR_DP", EditInstruct.DELETE_OPERATION);
                            ei3.PutKey("U_FID", Prolink.Math.GetValueAsString(ei.Get("U_ID")));
                            ei3.PutKey("GROUP_ID", GroupId);
                            ei3.PutKey("CMP", CompanyId);
                            mixList.Add(ei3);
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "st2")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApproveAttrDPModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            string pk = Prolink.Math.GetValueAsString(ei.Get("U_ID"));
                            string fk = Prolink.Math.GetValueAsString(ei.Get("U_FID"));
                            string ffk = Prolink.Math.GetValueAsString(ei.Get("U_FFID"));
                            string userId = Prolink.Math.GetValueAsString(ei.Get("USER_ID"));
                            if (pk == "" || fk == "" || ffk == "" || userId == "")
                            {
                                continue;
                            }
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", CompanyId);
                            ei.Put("STN", Station);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
                        }
                        mixList.Add(ei);
                    }
                }
            }

            List<Dictionary<string, object>> eaData = new List<Dictionary<string, object>>();
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId);
                    DataTable dt = ModelFactory.InquiryData("*", "APPROVE_ATTRIBUTE", condition, "", pageIndex, pageSize, ref recordsCount);
                    eaData = ModelFactory.ToTableJson(dt, "ApproveAttrModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage, mainData = eaData });
        }

        public JsonResult getApprovedData()
        {
            string RefNo = Prolink.Math.GetValueAsString(Request.Params["RefNo"]);
            string ApproveCode = Prolink.Math.GetValueAsString(Request.Params["ApproveCode"]);
            
            string sql = "SELECT COUNT(*) FROM APPROVE_RECORD WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + " AND REF_NO=" + SQLUtils.QuotedStr(RefNo);
            int totalRecord =  OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            //int recordsCount = 0, pageIndex = 0, pageSize = 20;
            string cmd = "";
            if (totalRecord > 0)
            {
                //cmd = "SELECT * FROM APPROVE_RECORD WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + " AND REF_NO=" + SQLUtils.QuotedStr(RefNo);
                cmd = "SELECT A.APPROVE_CODE, A.APPROVE_LEVEL, A.APPROVE_NAME, C.FID, C.FDESCP, A.REF_NO, A.ROLE, A.ROLE_NM, A.STATUS, A.CREATE_BY, A.CREATE_DATE, D.U_MANAGER FROM APPROVE_RECORD A ";
                cmd += " LEFT JOIN SYS_ACCT D ON A.CREATE_BY = D.U_ID AND D.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND D.CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND D.STN=" + SQLUtils.QuotedStr(Station);
                cmd += " LEFT JOIN SYS_ROLE C ON A.ROLE = C.FID AND C.GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND C.CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND C.STN=" + SQLUtils.QuotedStr(Station);
                cmd += " WHERE A.REF_NO=" + SQLUtils.QuotedStr(RefNo) + " AND A.APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + "ORDER BY A.APPROVE_LEVEL";
            }
            else
            {
                cmd = "SELECT * FROM APPROVE_FLOW_D A LEFT JOIN SYS_ROLE B ON  A.ROLE = B.FID WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode);
            }

            DataTable dt = OperationUtils.GetDataTable(cmd, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string RoleSql = "SELECT * FROM SYS_ROLE WHERE GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CMP=" + SQLUtils.QuotedStr(CompanyId) + " AND STN=" + SQLUtils.QuotedStr(Station);
            DataTable Roledt = OperationUtils.GetDataTable(RoleSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //int recordsCount = 0, pageIndex = 0, pageSize = 20;
            //string coditions = "WHERE A.APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + "ORDER BY A.APPROVE_LEVEL";
            //DataTable RecordDt = OperationUtils.GetDataTable("SELECT COUNT(*) FROM APPROVE_RECORD" + coditions, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable dt = OperationUtils.GetDataTable("SELECT A.APPROVE_LEVEL as Alevel, A.APPROVE_NAME as Aname, A.APPROVE_CODE as Acode, A.ROLE as Arole, C.FDESCP as Arolenm, B.* FROM APPROVE_FLOW_D A INNER JOIN SYS_ROLE C ON A.ROLE = C.FID LEFT JOIN APPROVE_RECORD B ON A.APPROVE_CODE=B.APPROVE_CODE AND A.APPROVE_LEVEL=B.APPROVE_LEVEL and REF_NO=" + SQLUtils.QuotedStr(RefNo) + coditions, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            /*string Max = dt.Compute("max(APPROVE_LEVEL)", "").ToString();
            if (Max == "")
            {
                Max = "0";
            }*/
            return Json(new { returnData = ModelFactory.ToTableJson(dt), RoleResult = ModelFactory.ToTableJson(Roledt), Max = totalRecord });
        }

        public JsonResult iApproved()
        {
            /*Status: 1申请，2審核，0退回 */
            string Status = Prolink.Math.GetValueAsString(Request.Params["Status"]);
            string ApproveCode = Prolink.Math.GetValueAsString(Request.Params["ApproveCode"]);
            string ApproveName = Prolink.Math.GetValueAsString(Request.Params["ApproveName"]);
            int ApproveLevel = Prolink.Math.GetValueAsInt(Request.Params["ApproveLevel"]);
            string RefNo = Prolink.Math.GetValueAsString(Request.Params["RefNo"]);
            string Role = Prolink.Math.GetValueAsString(Request.Params["Role"]);
            string RoleNm = Prolink.Math.GetValueAsString(Request.Params["RoleNm"]);
            int NowStatus = Prolink.Math.GetValueAsInt(Request.Params["NowStatus"]);
            string returnMessage = "success";
            //int retrunStatus = 1;
            MixedList mixedlist = new MixedList();
            int tmp = 1;
            if (Status == "1")
            {
                string sql1 = "SELECT * FROM APPROVE_FLOW_D WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + " ORDER BY APPROVE_LEVEL ASC";
                DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    
                    string AppCode = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                    string AppNm = Prolink.Math.GetValueAsString(dr["APPROVE_NAME"]);
                    int AppLv = Prolink.Math.GetValueAsInt(dr["APPROVE_LEVEL"]);
                    string AppRole = Prolink.Math.GetValueAsString(dr["Role"]);

                    string sql2 = "";
                    if (tmp == 1)
                    {
                        sql2 = "INSERT INTO APPROVE_RECORD (REF_NO,APPROVE_CODE,APPROVE_LEVEL,APPROVE_NAME,ROLE,ROLE_NM,STATUS,CREATE_BY,CREATE_DATE,MODIFY_BY,MODIFY_DATE) VALUES (" + SQLUtils.QuotedStr(RefNo) + "," + SQLUtils.QuotedStr(AppCode)
                           + "," + AppLv + "," + SQLUtils.QuotedStr(AppNm) + "," + SQLUtils.QuotedStr(AppRole) + ",null,1," + SQLUtils.QuotedStr(UserId) + ",getdate()" + "," + SQLUtils.QuotedStr(UserId) + ",getdate())";
                    }
                    else if (ApproveLevel + 1 == tmp)
                    {
                        sql2 = "INSERT INTO APPROVE_RECORD (REF_NO,APPROVE_CODE,APPROVE_LEVEL,APPROVE_NAME,ROLE,ROLE_NM,STATUS,CREATE_BY,CREATE_DATE,MODIFY_BY,MODIFY_DATE) VALUES (" + SQLUtils.QuotedStr(RefNo) + "," + SQLUtils.QuotedStr(AppCode)
                           + "," + AppLv + "," + SQLUtils.QuotedStr(AppNm) + "," + SQLUtils.QuotedStr(Role) + "," + SQLUtils.QuotedStr(RoleNm) + ",2,null,null,null,null)";
                    }
                    else
                    {
                        sql2 = "INSERT INTO APPROVE_RECORD (REF_NO,APPROVE_CODE,APPROVE_LEVEL,APPROVE_NAME,ROLE,ROLE_NM,STATUS,CREATE_BY,CREATE_DATE,MODIFY_BY,MODIFY_DATE) VALUES (" + SQLUtils.QuotedStr(RefNo) + "," + SQLUtils.QuotedStr(AppCode)
                           + "," + AppLv + "," + SQLUtils.QuotedStr(AppNm) + ",null,null,0,null,null,null,null)";
                    }
                    tmp++;
                    mixedlist.Add(sql2);
                }
            }
            else if (Status == "2")
            {
                string Recordsql = "SELECT * FROM APPROVE_RECORD WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + " AND REF_NO=" + SQLUtils.QuotedStr(RefNo) + " ORDER BY APPROVE_LEVEL ASC";
                DataTable dt1 = OperationUtils.GetDataTable(Recordsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow dr = dt1.Rows[i];
                    int AppStatus = Prolink.Math.GetValueAsInt(dr["Status"]);
                    int AppLv = Prolink.Math.GetValueAsInt(dr["APPROVE_LEVEL"]);
                    string AppCode = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                    string AppRefNo = Prolink.Math.GetValueAsString(dr["REF_NO"]);
                    String sql3 = "";
                    if (ApproveLevel == AppLv)
                    {
                        AppStatus = 1;
                        sql3 = "UPDATE APPROVE_RECORD SET STATUS=" + AppStatus + ", CREATE_BY=" + SQLUtils.QuotedStr(UserId) + ", CREATE_DATE=getdate(), MODIFY_BY=" + SQLUtils.QuotedStr(UserId) + ",MODIFY_DATE=getdate() WHERE REF_NO=" + SQLUtils.QuotedStr(AppRefNo) + " AND APPROVE_CODE=" + SQLUtils.QuotedStr(AppCode) + " AND APPROVE_LEVEL=" + AppLv;
                        mixedlist.Add(sql3);
                    }
                    else if (ApproveLevel + 1 == AppLv)
                    {
                        AppStatus = 2;
                        sql3 = "UPDATE APPROVE_RECORD SET ROLE=" + SQLUtils.QuotedStr(Role) + ", ROLE_NM=" + SQLUtils.QuotedStr(RoleNm) + ", STATUS=" + AppStatus + ", CREATE_BY=" + SQLUtils.QuotedStr(UserId) + ", CREATE_DATE=getdate(), MODIFY_BY=" + SQLUtils.QuotedStr(UserId) + ",MODIFY_DATE=getdate() WHERE REF_NO=" + SQLUtils.QuotedStr(AppRefNo) + " AND APPROVE_CODE=" + SQLUtils.QuotedStr(AppCode) + " AND APPROVE_LEVEL=" + AppLv;
                        mixedlist.Add(sql3);
                    }
                }
            }
            else if (Status == "0")
            {
                
                string Recordsql = "SELECT * FROM APPROVE_RECORD WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + " AND REF_NO=" + SQLUtils.QuotedStr(RefNo) + " ORDER BY APPROVE_LEVEL ASC";
                DataTable dt1 = OperationUtils.GetDataTable(Recordsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow dr = dt1.Rows[i];
                    int AppStatus = Prolink.Math.GetValueAsInt(dr["Status"]);
                    int AppLv = Prolink.Math.GetValueAsInt(dr["APPROVE_LEVEL"]);
                    string AppCode = Prolink.Math.GetValueAsString(dr["APPROVE_CODE"]);
                    string AppRefNo = Prolink.Math.GetValueAsString(dr["REF_NO"]);
                    string sql4 = "";
                    if (ApproveLevel - 1 == 1)
                    {
                        sql4 = "DELETE FROM APPROVE_RECORD  WHERE REF_NO=" + SQLUtils.QuotedStr(AppRefNo) + " AND APPROVE_CODE=" + SQLUtils.QuotedStr(AppCode);
                        mixedlist.Add(sql4);
                        continue;
                    }
                    else if (ApproveLevel == AppLv)
                    {
                        sql4 = "UPDATE APPROVE_RECORD SET STATUS=0 WHERE REF_NO=" + SQLUtils.QuotedStr(AppRefNo) + " AND APPROVE_CODE=" + SQLUtils.QuotedStr(AppCode) + " AND APPROVE_LEVEL=" + AppLv;
                        mixedlist.Add(sql4);
                    }
                    else if (ApproveLevel - 1 == AppLv)
                    {
                        sql4 = "UPDATE APPROVE_RECORD SET STATUS=2 WHERE REF_NO=" + SQLUtils.QuotedStr(AppRefNo) + " AND APPROVE_CODE=" + SQLUtils.QuotedStr(AppCode) + " AND APPROVE_LEVEL=" + AppLv;
                        mixedlist.Add(sql4);
                    }
                   
                }
                //string sql2 = "DELETE FROM APPROVE_RECORD WHERE APPROVE_CODE=" + SQLUtils.QuotedStr(ApproveCode) + "AND APPROVE_LEVEL=" + ApproveLevel + "AND REF_NO=" + SQLUtils.QuotedStr(RefNo);
               // mixedlist.Add(sql2);
            }
            if (mixedlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }

        public ActionResult ApproveSignQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "APPROVE_SIGN", "", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "ApproveSignModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult ApproveSignUpdate()
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
            MixedList mixList = new MixedList();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);

            string date = string.Empty;
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "ApproveSignModel");
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
                            mixList.Add(ei);
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
    }
}
