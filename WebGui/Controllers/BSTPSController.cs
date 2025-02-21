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
    public class BSTPSController : BaseController
    {
        public ActionResult BSTPSQuery()
        {
            ViewBag.MenuBar = false;
            //SetTranModeSelect();
            return View();
        }

        public ActionResult BSTPSSetup(string id = null, string uid = null)
        {
            if (uid == null)
            {
                uid = id;
            }
            string sql = "SELECT * FROM BSTPS WHERE 1=0";
            Dictionary<string, Dictionary<string, object>> mtSchemas = ModelFactory.GetSchemaBySql(sql, new List<string> { "U_ID" });
            ViewBag.schemas = ToContent(mtSchemas);
            ViewBag.Uid = id;
            ViewBag.pmsList = GetBtnPms("BSTPS");
            return View();
        }


        public ActionResult GetDetail()
        {
            string u_id = Request["UId"];
            string sql = string.Format("SELECT * FROM BSTPS WHERE U_ID={0}", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["main"] = ModelFactory.ToTableJson(mainDt, "BstpsModel");
            return ToContent(data);
        }

        public ActionResult BSTPSQueryData()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            int recordsCount = 0, pageIndex = 0, pageSize = 20;
            DataTable dt = ModelFactory.InquiryData("*", "BSTPS", Request.Params, ref recordsCount, ref pageIndex, ref pageSize, "");

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

        public ActionResult UpdateData()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            List<Dictionary<string, object>> simData = new List<Dictionary<string, object>>();
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "mt")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "BstpsModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {

                            ei.Put("CREATE_BY", UserId);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                            ei.Put("GROUP_ID", GroupId);
                            UId = Guid.NewGuid().ToString();
                            ei.Put("U_ID", UId);
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
                    return Json(new { message = ex.Message, message1 = returnMessage });
                }
            }
            string sql = string.Format("SELECT * FROM BSTPS WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            simData = ModelFactory.ToTableJson(mainDt, "BstpsModel");
            return Json(new { main = simData });
        }
    }
}
