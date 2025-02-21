using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Data;
using TrackingEDI.InboundBusiness;

namespace NotifyService.Task
{
    public class ExportFileToFTPTask : IPlanTask
    {
        private static Prolink.EDOC_API _api = new Prolink.EDOC_API();
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            GettaskInfo();
        }

        public void GettaskInfo()
        {
            string deletesql = "DELETE FROM EXPORT_FILE WHERE CREATE_DATE <DATEADD(month, -3, getDate()) ";
            OperationUtils.ExecuteUpdate(deletesql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string sql = "SELECT * FROM EXPORT_FILE WHERE STATUS IS NULL OR STATUS='' OR (STATUS = 'N' AND (ERROR_COUNT IS NULL OR ERROR_COUNT<5) AND PROCESS_DATE<DATEADD(minute, -30, getDate())) ORDER BY CREATE_DATE DESC";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow asddr in dt.Rows)
            {
                string exportfileid = Prolink.Math.GetValueAsString(asddr["EXPORT_FILE_ID"]);
                string shipmentid = Prolink.Math.GetValueAsString(asddr["SHIPMENT_ID"]);
                int errorCount = Prolink.Math.GetValueAsInt(asddr["ERROR_COUNT"]);
                if (errorCount >= 5)
                    continue;
                string company = Prolink.Math.GetValueAsString(asddr["CMP"]);
                string type = Prolink.Math.GetValueAsString(asddr["EDOC_TYPE"]);
                string userid = Prolink.Math.GetValueAsString(asddr["USER_ID"]);
                string foldername= Prolink.Math.GetValueAsString(asddr["FOLDER_NAME"]);
                bool sucess = false;
                string message = string.Empty ;
                try
                {
                    sql = string.Format("SELECT O_UID,O_LOCATION, COMBINE_INFO,SHIPMENT_ID,MASTER_NO,(SELECT TOP 1 ISCOMBINE_BL FROM SMSM WHERE SMSM.SHIPMENT_ID=SMSMI.SHIPMENT_ID) AS ISCOMBINE_BL FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(shipmentid));
                    DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in smsmdt.Rows)
                    {
                        string shipmentd = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                        string ouid = Prolink.Math.GetValueAsString(dr["O_UID"]);
                        string olocation = Prolink.Math.GetValueAsString(dr["O_LOCATION"]);
                        string masterno = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                        DataTable Dt = GetEdocData(shipmentd, company, ouid, true);
                        if (Dt.Rows.Count <= 0) continue;
                        string jobNo = string.Empty;
                        string fileNo = string.Empty;
                        string cmp = string.Empty;
                        if (!string.IsNullOrEmpty(ouid))
                        {
                            jobNo = ouid;
                            fileNo = string.IsNullOrEmpty(masterno) ? shipmentd : masterno;
                            cmp = olocation;
                        }
                        string dep = "";
                        foreach (DataRow fllowdr in Dt.Rows)
                        {
                            string flowuid = Prolink.Math.GetValueAsString(fllowdr["U_ID"]);
                            string dnno = Prolink.Math.GetValueAsString(fllowdr["DN_NO"]);
                            string flowcmp = Prolink.Math.GetValueAsString(fllowdr["CMP"]);
                            if (string.IsNullOrEmpty(dnno))
                                dnno = string.IsNullOrEmpty(masterno) ? shipmentd : masterno;
                            if (string.IsNullOrEmpty(dnno))
                                continue;
                            jobNo += "," + flowuid;
                            fileNo += "," + dnno;
                            cmp += "," + flowcmp;
                        }
                        jobNo = jobNo.Trim(',');
                        fileNo = fileNo.Trim(',');
                        cmp = cmp.Trim(',');

                        IBUserInfo userinfo = new IBUserInfo
                        {
                            UserId = userid,
                            CompanyId = company,
                            GroupId = "TPV"
                        };
                        message += "inbound doc qty:" + Business.TPV.Utils.DownLoadFile.DownLoadFileToDir(jobNo, fileNo, cmp, dep, type, true, userinfo, shipmentd, foldername, masterno);
                    }
                    sucess = true;
                }
                catch (Exception ex)
                {
                    message = ex.Message.ToString();
                    sucess = false;
                }
                if (message.EndsWith("DownloadAgain"))
                    sucess = false;
                message = message.Length >= 500 ? message.Substring(0, 499) : message;
                EditInstruct ei = new EditInstruct("EXPORT_FILE", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("EXPORT_FILE_ID", exportfileid);
                if (sucess)
                {
                    ei.Put("STATUS", "Y");
                    ei.Put("MESSAGE", message);
                }
                else
                {
                    ei.Put("STATUS", "N");
                    ei.Put("ERROR_COUNT", errorCount + 1);
                    ei.Put("MESSAGE", message);
                }
                ei.PutDate("PROCESS_DATE", DateTime.Now);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }


        public DataTable GetEdocData(string shipmentid, string companyid, string OUid = "", bool hasinbound = false)
        {
            string dnsql = string.Format("SELECT COMBINE_INFO,O_LOCATION,MASTER_NO FROM SMSMI WHERE SHIPMENT_ID={0}",
                SQLUtils.QuotedStr(shipmentid));
            string dninfo = string.Empty;
            string masterno = string.Empty;
            DataTable smDt = OperationUtils.GetDataTable(dnsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count > 0)
            {
                dninfo = Prolink.Math.GetValueAsString(smDt.Rows[0]["COMBINE_INFO"]);
                masterno = Prolink.Math.GetValueAsString(smDt.Rows[0]["MASTER_NO"]);
            }

            string sql = string.Format(@"SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMDN.DN_NO, 7,8)AS DN_NO FROM SMDN WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMINM' AS D_TYPE,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMIRV' AS D_TYPE,{3} AS DN_NO FROM SMIRV WHERE SHIPMENT_INFO LIKE '%{1}%' AND CMP={2}",
               SQLUtils.QuotedStr(shipmentid), shipmentid, SQLUtils.QuotedStr(companyid), SQLUtils.QuotedStr(masterno));
            if (string.IsNullOrEmpty(OUid))
            {
                sql += string.Format(@"UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO  FROM SMSM WHERE SHIPMENT_ID ={0}
                UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE COMBIN_SHIPMENT ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }
            else
            {
                sql += string.Format("UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE COMBIN_SHIPMENT ={0} AND COMBIN_SHIPMENT!=SHIPMENT_ID",
                    SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }
            string[] dninfos = dninfo.Split(',');
            if (dninfos.Length > 0)
            {
                foreach (string dnno in dninfos)
                {
                    sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,SUBSTRING(SMDN.DN_NO, 7,8)AS DN_NO FROM SMDN WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
                    sql += string.Format(" UNION SELECT U_ID,GROUP_ID,CMP,'SMINM' AS D_TYPE,SUBSTRING(SMINM.DN_NO, 7,8)AS DN_NO FROM SMINM WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
                }
            }

            if (hasinbound)
            {
                sql += string.Format(" UNION select U_ID,GROUP_ID,CMP,'SMSMI' AS D_TYPE,{1} AS DN_NO  FROM SMSMI WHERE SHIPMENT_ID ={0}", SQLUtils.QuotedStr(shipmentid), SQLUtils.QuotedStr(masterno));
            }

            if (shipmentid.Length > 4 && shipmentid.Substring(0, 4).Equals("MHK_"))
            {
                sql += string.Format("UNION SELECT U_ID,GROUP_ID,CMP,'SMDN' AS D_TYPE,{1} AS DN_NO FROM SMSM WHERE SHIPMENT_ID={0}",
                    SQLUtils.QuotedStr(shipmentid.Substring(4)), SQLUtils.QuotedStr(masterno));
            }

            DataTable Dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return Dt;
        }
    }
}
