using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class InboundASDReport : IPlanTask
    {
        private string _hour = null;
        public string[] cmps = { "PL", "MX", "TS", "RU", "BR" };
        public InboundASDReport(string hour)
        {
            _hour = hour;
        }
         IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            //查看需要归档的报表，然后执行归档
            InsertAutoAsdTask(_hour);

             string sql = "SELECT * FROM ASD_TASK WHERE IO_TYPE='I' AND RESULT_STATUS IS NULL ORDER BY CREATE_DATE DESC";
             DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

             string uid = string.Empty;
             string basecondition = string.Empty;
             string asdtype = string.Empty;
             MixedList ml = new MixedList();

             foreach (DataRow dr in dt.Rows)
             {
                 uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                 basecondition = Prolink.Math.GetValueAsString(dr["BASE_CONDITION"]);
                 string month = Prolink.Math.GetValueAsString(dr["STR_MONTH"]);
                 int year = Prolink.Math.GetValueAsInt(dr["YEAR"]);
                 asdtype = Prolink.Math.GetValueAsString(dr["ASD_TYPE"]);
                 string companyid = Prolink.Math.GetValueAsString(dr["CMP"]);
                 string xlsFile = string.Empty;
                 try
                 {
                     ASDUserInfo userinfo = new ASDUserInfo
                     {
                         UserId = Prolink.Math.GetValueAsString(dr["USERID"]),
                         CompanyId = companyid,
                         GroupId = "TPV"
                     };
                     InboundASDReportHelper asdreporthelper = new InboundASDReportHelper(asdtype);
                     xlsFile = asdreporthelper.GetTemplateData(year, month, userinfo);
                 }
                 catch (Exception ex)
                 {
                     Prolink.DataOperation.OperationUtils.Logger.WriteLog(string.Format("InboundASDReport {0}", ex.ToString()));
                     UpdateDB(uid, ex.Message.ToString(), false);
                     return;
                 }
                 UpdateDB(uid, xlsFile,true);
                 OperationUtils.Logger.WriteLog("xlsFile result=" + xlsFile);
                 //上传到EDOC
                 Result result = new Result();
                 string jobNo = "ASD Upload to edoc";
                 string groupId = "TPV";
                EdocHelper edochelper = new EdocHelper();
                EDOCApi.EDOCFileItem fileInfo = edochelper.UploadFile2EDOC(jobNo, xlsFile, groupId, companyid, "*", "*", "RFQ", "ESP");
                 if (fileInfo == null)
                 {
                     result.Success = false;
                 }
                 else
                 {
                     result.Success = true;
                     result.EdocUrl = fileInfo.FileID;
                 }
                 //Result result=edochelper.UpdateASD2Edoc(xlsFile, company);
                 OperationUtils.Logger.WriteLog("UpdateASD2Edoc result=" + result.Message);
                 if (result.Success)
                 {
                     UpadteFileid(uid, result.EdocUrl);
                 }
             }

        }

        public void InsertAutoAsdTask(string hourstr)
        {
            MixedList mlist = new MixedList();
            foreach (string company in cmps)
            {
                DateTime odt = DateTime.Now;
                DateTime ndt = TrackingEDI.Business.TimeZoneHelperCG.GetTimeZoneDate(odt, company);
                int lastyear = ndt.Year;
                int lastmonth = ndt.Month;
                int nowday = ndt.Day;
                int hour = ndt.Hour;
                int parmhour = Prolink.Math.GetValueAsInt(hourstr);
                if (string.IsNullOrEmpty(hourstr))
                    parmhour = 1;
                int firstmonth = lastmonth - 1;
                int firstyear = lastyear;
                if (firstmonth == 0)
                {
                    firstmonth = 12;
                    firstyear = lastyear - 1;
                }
                string strmonth = firstmonth.ToString();
                if (strmonth.Length == 1)
                    strmonth = "0" + strmonth;
                string strlsthmonth = lastmonth.ToString();
                if (strlsthmonth.Length == 1)
                    strlsthmonth = "0" + strlsthmonth;
                DateTime etdfrom = Prolink.Math.GetValueAsDateTime(firstyear + strmonth + "01");
                DateTime etdto = Prolink.Math.GetValueAsDateTime(lastyear + strlsthmonth + "01");
                string stryear = firstyear.ToString();
                if (nowday == 1 && hour == parmhour)
                {
                    string sql = string.Format("SELECT COUNT(1) FROM ASD_TASK WHERE CMP={0} AND YEAR={1} AND MONTH={2}",
                   SQLUtils.QuotedStr(company), SQLUtils.QuotedStr(stryear), SQLUtils.QuotedStr(strmonth));
                    int count = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (count > 0)
                        continue;
                    EditInstruct ei = null;
                    ei = new EditInstruct("ASD_TASK", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.Put("GROUP_ID", "TPV");
                    ei.Put("IO_TYPE", "I");
                    ei.Put("CMP", company);
                    ei.Put("USERID", "ASD_TASK");
                    ei.Put("BASE_CONDITION", string.Empty);
                    ei.Put("ASD_TYPE", "Q");
                    ei.Put("CREATE_BY", "ASD_TASK");
                    ei.Put("FILE_NAMES", "ASD Accrued Report_" + stryear + strmonth);
                    ei.PutDate("CREATE_DATE", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    ei.Put("MONTH", firstmonth);
                    ei.Put("YEAR", stryear);
                    ei.Put("STR_MONTH", strmonth);
                    ei.PutDate("ETD_FROM", etdfrom);
                    ei.PutDate("ETD_TO", etdto);
                    mlist.Add(ei);
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                }
            }
        }
        public void UpdateDB(string uid, string message, bool success = true)
        {
            EditInstruct ei = new EditInstruct("ASD_TASK", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            if (success)
            {
                ei.Put("RESULT_STATUS", "S");
                ei.Put("FILEPATH", message);
            }
            else
            {
                ei.Put("RESULT_STATUS", "F");
                ei.Put("REMARK", message);
            }
            MixedList ml = new MixedList();

            ml.Add(ei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
            }
        }

        public void UpadteFileid(string uid, string fieldid)
        {
            if (string.IsNullOrEmpty(fieldid))
                return;
            EditInstruct ei = new EditInstruct("ASD_TASK", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("RESULT_STATUS", "S");
            ei.Put("FILEID", fieldid);

            MixedList ml = new MixedList();

            ml.Add(ei);

            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
            }
        }
    }
}
