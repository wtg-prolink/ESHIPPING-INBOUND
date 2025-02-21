using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;
namespace WebGui.Controllers
{
    public class ACTRANTYPEController : BaseController
    {
        //
        // GET: /ACTRANTYPE/

        public ActionResult AcTranTypeSetup()
        {
            SetAtranTypeSelect();
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("ACTRANTYPE");
            return View();
        }

        public ActionResult PostBillSetup()
        {
            SetAtranTypeSelect();
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("POSTBILL");
            return View();
        }

        private void SetAtranTypeSelect()
        {
            ViewBag.AtranType = "";
            ViewBag.DefaultAtran = "";

            #region Approve
            string sql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='TCT' AND GROUP_ID={0} ", SQLUtils.QuotedStr(GroupId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                else
                {
                    ViewBag.DefaultAtran = Prolink.Math.GetValueAsString(dr["CD"]);
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD"])+"."+Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            ViewBag.AtranType = select;
            #endregion
        }

        private void SetCargoTypeSelect()
        {
            ViewBag.CargoType = GetBscodeSelect("TCGT");
        }

        private string GetBscodeSelect(string Bscode)
        {
            string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE={0} AND {1} ", SQLUtils.QuotedStr(Bscode), string.Format("GROUP_ID={0} AND ( CMP = '*' OR CMP={1} )", SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(BaseCompanyId)));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string select = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (select.Length > 0)
                {
                    select += ";";
                }
                select += Prolink.Math.GetValueAsString(dr["CD"]);
                select += ":" + Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
            }
            return select;
        }

        public ActionResult AcTranTypeQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMACTN", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize,"CREATE_DATE ASC");
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            
            data["main"] = ModelFactory.ToTableJson(dt, "smactnModel");
            return ToContent(data);
        }

        public ActionResult AcTranTypeUpdate()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "smactnModel");
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
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("CUR", ei.Get("CUR"));
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
            //return Json(new { message = returnMessage });
        }



        public ActionResult PostBillQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMPOST", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            data["main"] = ModelFactory.ToTableJson(dt, "SmPostModel");
            return ToContent(data);
        }

        //过账建档
        public ActionResult PostBillUpdate()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmPostModel");
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
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("CUR", ei.Get("CUR"));
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
        public ActionResult DestMapSetup()
        {
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("DESTMAPPING");
            return View();
        }
        public ActionResult DestMapQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "DEST_MAP", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, "CREATE_DATE ASC");
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            data["main"] = ModelFactory.ToTableJson(dt, "DestMapModel");
            DataTable subdt = ModelFactory.InquiryData("*", "DIRECT_MAP", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, "CREATE_DATE ASC");
            data["sub"] = ModelFactory.ToTableJson(subdt, "DirectMapModel");
            return ToContent(data);
        }
        public ActionResult DestMapUpdate()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "DestMapModel");
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
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("CUR", ei.Get("CUR"));
                        }
                        mixList.Add(ei);
                    }
                }
                else if (item.Key == "sub")
                {
                    ArrayList objList = item.Value as ArrayList;

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "DirectMapModel");
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
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("CUR", ei.Get("CUR"));
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

        public ActionResult MaterialTypeSetup()
        {
            SetCargoTypeSelect();
            string sql = string.Format("SELECT NAME FROM SYS_SITE WHERE TYPE='1' AND {0}", GetBaseCmp());
            string cmpnm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ViewBag.CmpNm = cmpnm;
            ViewBag.pmsList = GetBtnPms("MATERIALTYPE");
            return View();
        }

        public ActionResult MaterialTypeQuery()
        {
            string cmp = Request.Params["Cmp"];
            string condition = GetBaseGroup();

            if (!string.IsNullOrEmpty(cmp))
            {
                condition += string.Format(" AND CMP ={0}", SQLUtils.QuotedStr(cmp));
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            int recordsCount = 0, pageIndex = 1, pageSize = 0;
            DataTable dt = ModelFactory.InquiryData("*", "SMMATERIAL", condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize, "CREATE_DATE ASC");
            //DataTable dt = ModelFactory.InquiryData("CurrencyModel", Request.Params, ref recordsCount, ref pageIndex, ref pageSize);

            data["main"] = ModelFactory.ToTableJson(dt, "SmmaterialModel");
            return ToContent(data);
        }

        public ActionResult MaterialTypeUpdate()
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

                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmmaterialModel");
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
                            ei.Put("STN", Station);
                            ei.Put("DEP", Dep);
                        }
                        if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.PutKey("U_ID", ei.Get("U_ID"));
                            ei.Put("MODIFY_BY", UserId);
                            ei.PutDate("MODIFY_DATE", DateTime.Now);
                            //ei.PutKey("CUR", ei.Get("CUR"));
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
            //return Json(new { message = returnMessage });
        }


    }
}
