using Business.TPV;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;

namespace NotifyService.Task
{
    public class InvPkgEdocAuto : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            string sql = "SELECT * FROM SM_AUTOEDOC WHERE IS_OK IS NULL OR IS_OK ='N' ORDER BY CREATE_DATE DESC";

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string uid = string.Empty;
            string shipmentid = string.Empty;
            MixedList ml = new MixedList();
            string message = string.Empty;
            string iscalcul = string.Empty;
            string filetype = string.Empty;
            string dnuid=string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                filetype = Prolink.Math.GetValueAsString(dr["FILE_TYPE"]);
                string jobno = Prolink.Math.GetValueAsString(dr["JOB_NO"]);
                dnuid = Prolink.Math.GetValueAsString(dr["DN_UID"]);
                try
                {
                    string reportid = Prolink.Math.GetValueAsString(dr["REPORT_ID"]);
                    EdocHelper edochelper = new EdocHelper();
                    if ("IPQ05".Equals(reportid) || "IPQ06".Equals(reportid))
                    {
                        edochelper.AutoReloadInvoice(jobno);
                    }
                    Dictionary<string, object> parm = new Dictionary<string, object>();
                    
                    parm["reportId"] = reportid;
                    parm["exportFileType"] = "pdf";
                    parm["reportName"] = Prolink.Math.GetValueAsString(dr["REPORT_NAME"]);
                    parm["fileType"] = filetype;
                    string condiStr="IuFid=" + jobno + "&sopt_IuFid=eq";
                    parm["conditionString"] = "IuFid=" + jobno + "&sopt_IuFid=eq";
                    if (reportid == "IPQ01" || reportid == "IPQ03" || reportid == "IPQ05")
                    {
                        condiStr = "PuFid=" + jobno + "&sopt_PuFid=eq";
                    }
                    if (reportid == "BF")
                    {
                        condiStr = "ShipmentId=" + shipmentid + "&sopt_ShipmentId=eq";
                        dnuid = jobno;
                    }
                    parm["jobNo"] = dnuid;
                    parm["GroupId"] = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                    parm["CMP"] = Prolink.Math.GetValueAsString(dr["CMP"]);
                    parm["STN"] = "*";
                    parm["ShipmentId"] = shipmentid;
                    //parm["TranType"] = trantype;
                    //parm["PartyNo"] = partyno;
                    parm["Remark"] = Prolink.Math.GetValueAsString(dr["REMARK"]);
                    parm["userid"] = Prolink.Math.GetValueAsString(dr["CREATE_BY"]);
                    //edochelper.SetParmByTranType(ref parm, filetype);
                    Result result = edochelper.AutoCreateReport(parm);
                    if (result.Success == false)
                    {
                        iscalcul = "F";
                        message = result.Message;
                    }
                    else
                    {
                        iscalcul = "Y";
                        EditInstruct _ei = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                        _ei.PutKey("U_ID", jobno);
                        switch (filetype)
                        {
                            case "PACKO": _ei.Put("BATCH_FLAGP", iscalcul); break;
                            case "INVO": _ei.Put("BATCH_FLAGI", iscalcul); break;
                            case "CONTRACT": _ei.Put("BATCH_FLAGC", iscalcul); break;
                        }
                        ml.Add(_ei);
                    }
                }
                catch (Exception ex)
                {
                    iscalcul = "N";
                    message = "自动归档失败：" + ex.Message;
                }
                EditInstruct ei = new EditInstruct("SM_AUTOEDOC", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", uid);
                ei.Put("IS_OK", iscalcul);
                if (!string.IsNullOrEmpty(message))
                    ei.Put("ADD_RESON", message);
                ei.PutDate("AUTO_DATE", DateTime.Now);
                ml.Add(ei);
            }
            if (ml.Count > 0)
            {
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
}
