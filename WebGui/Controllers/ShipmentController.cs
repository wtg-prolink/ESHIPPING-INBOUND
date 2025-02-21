using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;

namespace WebGui.Controllers
{
    public class ShipmentController : Controller
    {
        //
        // GET: /Shipment/
        public ActionResult SeaTransport(string id = null)
        {
            ViewBag.Uid = id;
            ViewBag.pmsList = "IportBtn|MBEdoc";
            return View();
        }

        public ActionResult AirTransport(string id = null)
        {
            ViewBag.Uid = id;
            ViewBag.pmsList = "IportBtn|MBEdoc";
            return View();
        }

        public ActionResult ExpressDelivery(string id = null)
        {
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult DTransport(string id = null)
        {
            ViewBag.Uid = id;
            return View();
        }

        public ActionResult ShipmentInput1()
        {
            return View();
        }

        public ActionResult ShipmentDemo()
        {
            return View();
        }

        public ActionResult InquiryData()
        {
            string hasm = Request["hasm"];//是否包含实体定义
            JavaScriptSerializer js = new JavaScriptSerializer();
            //Dictionary<string, object> colKVS = js.DeserializeObject(Request["cols"]) as Dictionary<string, object>;
            string sql = ModelFactory.GetHeadSql("VickyCobkModel");
            sql = sql + " ORDER BY CREATE_DATE DESC";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            BootstrapResult result = null;
            //if (!"Y".Equals(hasm))
                //result = BootstrapResult.GetBootstrapResult("VickyCobkModel", dt, colKVS,0,0);
            //else
            //{
                result = new BootstrapResult() { rows = ModelFactory.ToTableJson(dt, "VickyCobkModel") };
            //}

            return result.ToContent();
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //object[] jsmodels = js.DeserializeObject(changeData) as object[];
        }

        public void UpdateData()
        {
            string changeData = Request.Params["changedData"];
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            JavaScriptSerializer js = new JavaScriptSerializer();
            object[] jsmodels = js.DeserializeObject(changeData) as object[];
            MixedList list = ModelFactory.JsonToEditMixedList(jsmodels, "VickyCobkModel");
            OperationUtils.ExecuteUpdate(list, Prolink.Web.WebContext.GetInstance().GetConnection());
            //string sql = ModelFactory.GetHeadSql("提单");
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //BootstrapResult result = new BootstrapResult()
            //{
            //    colModel = null,
            //    colNames = null,
            //    rows = ModelFactory.ToTableJson(dt),
            //    page = 1,
            //    records = 2,
            //    total = 3
            //};
            //return result.ToContent();
        }
    }
}
