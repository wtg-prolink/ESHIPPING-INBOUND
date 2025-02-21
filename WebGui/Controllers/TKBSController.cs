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

namespace WebGui.Controllers
{
    public class TKBSController : BaseController
    {
        //
        // GET: /TKBS/
        #region View
        public ActionResult TKCMPQuery()
        {
            ViewBag.MenuBar = false;
            return View();
        }

        public ActionResult TKCMPSetup()
        {
            string CmpId = Prolink.Math.GetValueAsString(Request.Params["CmpId"]);
            string sql = "SELECT * FROM TK_CMP WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> schemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID"});
            ViewBag.schemas = ToContent(schemas);
            ViewBag.CmpId = CmpId;
            return View();
        }
        #endregion

        #region 查询
        public ActionResult CMPData()
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "TK_CMP", GetBaseCmp(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

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

        #region 基本操作
        public ActionResult TKCMPDetailData()
        { 
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            JavaScriptSerializer js = new JavaScriptSerializer();
            DataTable dt = ModelFactory.InquiryData("*", "TK_CMP", GetBaseGroup(), Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };

            return Json(new { mainTable = result.ToContent()});
        }
        public ActionResult TKCMPUpdateData()
        { 
            string changeData = Request.Params["changedData"];
            string CmpId = Request.Params["CmpId"];
            //int mainDtActionType = -1;
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
            List<Dictionary<string, object>> MainData = new List<Dictionary<string, object>>();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "TKCMPModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];

                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                            if (ei.Get("CMP_ID") == null)
                            {
                                continue;
                            }
                            CmpId = Prolink.Math.GetValueAsString(ei.Get("CMP_ID"));
                        }
                        else if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmm"));
                            ei.PutKey("GROUP_ID", GroupId);
                            ei.PutKey("CMP", CompanyId);
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
                    int recordsCount = 0, pageIndex = 1, pageSize = 20;
                    DataTable dt = ModelFactory.InquiryData("*", "TK_CMP", "CMP_ID='" + CmpId + "'", "", pageIndex, pageSize, ref recordsCount);
                    MainData = ModelFactory.ToTableJson(dt, "TKCMPModel");

                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            //return Json(new { message = returnMessage });
            return Json(new { message = returnMessage, MainData = MainData });
        }
        #endregion
    }
}
