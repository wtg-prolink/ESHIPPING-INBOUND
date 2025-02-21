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
    public class BSCODEController : BaseController
    {
        //
        // GET: /IPCTM/

        public ActionResult BSCODESetup()
        {
            ViewBag.pmsList = GetBtnPms("BSCODE");
            return View();
        }

        public ActionResult CodeKindRequiry()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string condition = GetBaseCmp();
            DataTable dt = ModelFactory.InquiryData("*","BSCODE_KIND",condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BscodeKindModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public ActionResult bscodeSortQuery()
        {
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            string CdType = Prolink.Math.GetValueAsString(Request.Params["CdType"]);
            string condition = " GROUP_ID=" + SQLUtils.QuotedStr(GroupId) + " AND CD_TYPE=" + SQLUtils.QuotedStr(CdType) + " AND " + GetBaseCmp();
            DataTable dt = ModelFactory.InquiryData("*", "BSCODE", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt, "BscodeKindModel"),
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1
            };
            return result.ToContent();
        }

        public JsonResult BscodeRequiry()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 9999;
            string cdType = Prolink.Math.GetValueAsString(Request.Params["CdType"]);
            string orderBy = " CD ASC";
            if(cdType.StartsWith("RN_") || "ATST".Equals(cdType))
                orderBy = "  ORDER_BY";
            
            string condtions = " WHERE CD_TYPE=" + SQLUtils.QuotedStr(cdType);
            condtions += " AND "+GetBaseCmp();
            DataTable detailDt = ModelFactory.InquiryData("*", "BSCODE", condtions, orderBy, pageIndex, pageSize, ref recordsCount);
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

        public JsonResult BsCodeUpdate()
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
                if (item.Key == "st")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BsCodeModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", returncmp());
                            ei.Put("STN", BaseStation);

                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {

                            ei.Put("MODIFY_BY", UserId);
                            ei.PutKey("CD_TYPE", ei.Get("CD_TYPE"));
                            ei.PutKey("CD", ei.Get("CD"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", BaseStation);
                        }
                        if (ei.Get("CD") != "" && ei.Get("CD_TYPE") != "")
                        {
                            ei.PutKey("CMP", returncmp());
                            mixList.Add(ei);
                        }

                        if (ei.Get("CD_TYPE") == "EDT")
                        {
                            rebuildPms = true;
                        }
              
                     }
                }
                else if (item.Key == "mt")
                { 
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BscodeKindModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {

                            ei.Put("MODIFY_BY", UserId);
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", BaseStation);
                        }
                        if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", ei.Get("CMP"));
                            ei.PutKey("STN", BaseStation);
                        }
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            ei.Put("CMP", returncmp());
                            ei.Put("STN", BaseStation);
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
                    if (rebuildPms)
                    {
                        //PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
                        Prolink.V3.PermissionManager.GetEdocPermission();
                        Business.CommonHelp.NotifyRebuidPermission(null, true);
                    }   
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            return Json(new { message = returnMessage });
        }
        public string returncmp() {
            string cmp = BaseCompanyId;
            string upri = this.UPri;
            if ("G".Equals(upri))
                cmp = "*";
            return cmp;
        }
    }
}
