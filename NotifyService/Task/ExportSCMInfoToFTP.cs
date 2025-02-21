using Business.TPV.Utils;
using Prolink.Data;
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
    public class ExportSCMInfoToFTP : IPlanTask
    {
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            GetDownSCMModleInfo();
        }

        public void GetDownSCMModleInfo()
        {
            string deletesql = "DELETE FROM EXPORT_SCMMODFILE WHERE CREATE_DATE <DATEADD(month, -3, getDate()) ";
            OperationUtils.ExecuteUpdate(deletesql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string sql = "SELECT * FROM EXPORT_SCMMODFILE WHERE STATUS IS NULL OR STATUS='' ORDER BY CREATE_DATE DESC";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow asddr in dt.Rows)
            {
                string exportfileid = Prolink.Math.GetValueAsString(asddr["EXPORT_FILE_ID"]);
                string fileName = Prolink.Math.GetValueAsString(asddr["FILE_NAME"]);
                string userid = Prolink.Math.GetValueAsString(asddr["CREATE_BY"]);
                string company = Prolink.Math.GetValueAsString(asddr["CMP"]);

                bool sucess = false;
                string message = string.Empty;
                try
                {
                    //产生文档：
                    BaseUserInfo userinfo = new BaseUserInfo { UserId = userid, CompanyId = company };
                    string xlsfile = CreateModelFile(asddr, userinfo);
                    UploadToFTP(xlsfile, userinfo);
                    sucess = true;
                }
                catch(Exception ex)
                {
                    message = ex.Message.ToString();
                }
                EditInstruct ei = new EditInstruct("EXPORT_SCMMODFILE", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("EXPORT_FILE_ID", exportfileid);
                if (sucess)
                {
                    ei.Put("STATUS", "Y");
                    ei.Put("MESSAGE", message);
                }
                else
                {
                    ei.Put("STATUS", "N");
                    ei.Put("MESSAGE", message);
                }
                ei.PutDate("PROCESS_DATE", DateTime.Now);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }

        private string CreateModelFile(DataRow dr,BaseUserInfo userinfo)
        {
            string conditions = Prolink.Math.GetValueAsString(dr["CONDITIONS"]);
            string table = Prolink.Math.GetValueAsString(dr["S_TABLE"]);
            string fileName = Prolink.Math.GetValueAsString(dr["FILE_NAME"]);

            string sql = string.Format("SELECT TRAN_TYPE,SHIPMENT_ID FROM {0} WHERE {1}",
               table, conditions);
            OperationUtils.Logger.WriteLog("S_TABLE开始:" + DateTime.Now);
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("S_TABLE结束:" + DateTime.Now);
            DataRow[] drs = smdt.Select("TRAN_TYPE IN ('R','F')");
            List<string> fcllist = new List<string>();
            foreach (DataRow smdr in drs)
            {
                fcllist.Add(smdr["SHIPMENT_ID"].ToString());
            }

            drs = smdt.Select("TRAN_TYPE NOT IN ('R','F')");
            List<string> normallist = new List<string>();
            foreach (DataRow smdr in drs)
            {
                normallist.Add(smdr["SHIPMENT_ID"].ToString());
            }

            string formate1 = "yyyy-MM-dd HH:mm:ss";
            string formate2 = "yyyy-MM-dd";
            DataTable dt = null;
            DataTable dtAll = null;
            if (fcllist.Count > 0 )
            {
                sql = string.Format(@"SELECT I.SHIPMENT_ID,I.BU,WEEKLY AS ETD_WK,I.O_LOCATION,I.SH_NM,P.INV_NO,P.PART_NO,P.QTY,P.CNTR_NO,I.MASTER_NO,
I.TRAN_TYPE,I.CARRIER_NM,I.POL_NAME,I.ETD_L,I.BOOKING_INFO,I.M_VESSEL,I.DEST_NAME,I.ETA,I.PORT_FREE_TIME,P.ASN_DATE,P.ASN_NO,P.PRIORITY,P.SCMREQUEST_DATE,P.GR_STATUS,P.GR_QTY,P.GR_DATE,P.DN_NO,  
I.STATUS,I.ZT_NM,I.MONTH,I.WE_NM,I.PONO_INFO,P.GOODS_DESCP,I.CNT20,I.CNT40,I.CNT40HQ,I.OCNT_NUMBER,I.TRADE_TERM,I.POL_CD,I.ETD,I.ATD,I.ETD1,I.VESSEL1,I.ETA1,I.ETD2,
I.VESSEL2,I.ETA2,I.DEST_CD,I.FACT_FREE_TIME,I.ASN_STATUS,I.ETA_L,
CASE WHEN DATEDIFF(DAY, I.ETA_L, I.ATA)>0 THEN 1 WHEN DATEDIFF(DAY, I.ETA_L, I.ATA)<=0 THEN 0 
WHEN DATEDIFF(DAY, I.ETA_L, I.ETA)>0 THEN 1 WHEN DATEDIFF(DAY, I.ETA_L, I.ETA) <=0 THEN 0 ELSE NULL END AS LTS,
DATEDIFF(DAY, I.ETD_L, I.ETA_L) AS ELT_L,DATEDIFF(DAY, I.ATD, I.ATA) AS ALT_L,DATEDIFF(DAY, I.ETA_L, I.ATA) AS ADD_L,DATEDIFF(DAY, I.ETA_L, I.ETA) AS EDD_L
 FROM SMIDNP P LEFT JOIN SMSMI I ON I.SHIPMENT_ID=P.SHIPMENT_ID WHERE P.SHIPMENT_ID IN {0}", SQLUtils.Quoted(fcllist.ToArray()));
                OperationUtils.Logger.WriteLog("fcllist开始:" + DateTime.Now);
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
                OperationUtils.Logger.WriteLog("fcllist结束:" + DateTime.Now);

                DataTable smicntr = GetSmicntrTable(fcllist); 
                DataTable smrcntr = GetSmrcntrTable(fcllist); 
                DataTable smirv = GetSmirvTable(fcllist); 

                dt.Columns["INV_NO"].MaxLength = 400;

                dt.Columns.Add("ETA_MSL", typeof(string));
                dt.Columns.Add("ETA_MSL_TIME", typeof(string));
                dt.Columns.Add("RESERVE_DATE", typeof(string));
                dt.Columns.Add("APPOINTMENT_TIME", typeof(string));
                dt.Columns.Add("ARRIVAL_FACT_DATE", typeof(string)); 
                dtAll = dt.Clone();
                string shipmentId, cntrNo, reserveNo;
                DataRow smicntrRow, smirvRow;
                foreach (DataRow row in dt.Rows)
                {
                    shipmentId = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]);
                    cntrNo = Prolink.Math.GetValueAsString(row["CNTR_NO"]); 
                    smicntrRow = GetSmicntrRow(smicntr, shipmentId, cntrNo);
                    if (smicntrRow != null)
                    {
                        row["ETA_MSL"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smicntrRow["ETA_MSL"]), formate2);
                        row["ETA_MSL_TIME"] = Prolink.Math.GetValueAsString(smicntrRow["ETA_MSL_TIME"]);
                    } 
                    reserveNo = GetReserveNoByCntrNo(smrcntr, shipmentId, cntrNo);
                    if (!string.IsNullOrEmpty(reserveNo))
                    {
                        smirvRow = GetSmirvRow(smirv, reserveNo);
                        if (smirvRow != null)
                        {
                            row["RESERVE_DATE"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["RESERVE_DATE"]), formate2);
                            row["APPOINTMENT_TIME"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["APPOINTMENT_TIME"]), formate1);
                            row["ARRIVAL_FACT_DATE"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["ARRIVAL_FACT_DATE"]), formate1);
                        }
                    }
                }
                dtAll.Merge(dt);
            }

            if (normallist.Count > 0)
            {
                sql = string.Format(@"SELECT I.SHIPMENT_ID,I.BU,WEEKLY AS ETD_WK,I.O_LOCATION,I.SH_NM,P.INV_NO,P.PART_NO,P.QTY,'' AS CNTR_NO,I.MASTER_NO,
I.TRAN_TYPE,I.CARRIER_NM,I.POL_NAME,I.ETD_L,I.BOOKING_INFO,I.M_VESSEL,I.DEST_NAME,I.ETA,I.PORT_FREE_TIME,P.ASN_DATE,P.ASN_NO,P.PRIORITY,P.SCMREQUEST_DATE,P.GR_STATUS,P.GR_QTY,P.GR_DATE,P.DN_NO,
I.STATUS,I.ZT_NM,I.MONTH,I.WE_NM,I.PONO_INFO,P.GOODS_DESCP,I.CNT20,I.CNT40,I.CNT40HQ,I.OCNT_NUMBER,I.TRADE_TERM,I.POL_CD,I.ETD,I.ATD,I.ETD1,I.VESSEL1,I.ETA1,I.ETD2,
I.VESSEL2,I.ETA2,I.DEST_CD,I.FACT_FREE_TIME,I.ASN_STATUS,I.ETA_L,
CASE WHEN DATEDIFF(DAY, I.ETA_L, I.ATA)>0 THEN 1 WHEN DATEDIFF(DAY, I.ETA_L, I.ATA)<=0 THEN 0 
WHEN DATEDIFF(DAY, I.ETA_L, I.ETA)>0 THEN 1 WHEN DATEDIFF(DAY, I.ETA_L, I.ETA) <=0 THEN 0 ELSE NULL END AS LTS,
DATEDIFF(DAY, I.ETD_L, I.ETA_L) AS ELT_L,DATEDIFF(DAY, I.ATD, I.ATA) AS ALT_L,DATEDIFF(DAY, I.ETA_L, I.ATA) AS ADD_L,DATEDIFF(DAY, I.ETA_L, I.ETA) AS EDD_L
FROM SMIDNP P LEFT JOIN SMSMI I ON I.SHIPMENT_ID=P.SHIPMENT_ID WHERE P.SHIPMENT_ID IN {0}", SQLUtils.Quoted(normallist.ToArray()));
                OperationUtils.Logger.WriteLog("normallist开始:" + DateTime.Now);
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
                OperationUtils.Logger.WriteLog("normallist结束:" + DateTime.Now);

                DataTable smidn = GetSmidnTable(normallist); 
                DataTable smrdn = GetSmrdn(normallist); 
                DataTable smirv = GetSmirvTable(normallist); 

                dt.Columns.Add("ETA_MSL", typeof(string));
                dt.Columns.Add("ETA_MSL_TIME", typeof(string));
                dt.Columns.Add("RESERVE_DATE", typeof(string));
                dt.Columns.Add("APPOINTMENT_TIME", typeof(string));
                dt.Columns.Add("ARRIVAL_FACT_DATE", typeof(string));

                string shipmentId, dnNo, reserveNo;
                DataRow smidnRow, smirvRow;
                foreach (DataRow row in dt.Rows)
                {
                    shipmentId = Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]);
                    dnNo = Prolink.Math.GetValueAsString(row["DN_NO"]);
                    smidnRow = GetSmidnRow(smidn, shipmentId, dnNo);
                    if (smidnRow != null)
                    {
                        row["ETA_MSL"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smidnRow["ETA_MSL"]), formate2);
                        row["ETA_MSL_TIME"] = Prolink.Math.GetValueAsString(smidnRow["ETA_MSL_TIME"]);
                    }
                    reserveNo = GetReserveNoByDnNo(smrdn, shipmentId, dnNo);
                    if (!string.IsNullOrEmpty(reserveNo))
                    {
                        smirvRow = GetSmirvRow(smirv, reserveNo);
                        if (smirvRow != null)
                        {
                            row["RESERVE_DATE"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["RESERVE_DATE"]), formate2);
                            row["APPOINTMENT_TIME"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["APPOINTMENT_TIME"]), formate1);
                            row["ARRIVAL_FACT_DATE"] = GetDateFormat(Prolink.Math.GetValueAsDateTime(smirvRow["ARRIVAL_FACT_DATE"]), formate1);
                        }
                    }
                }
                 
                if (dtAll == null || dtAll.Rows.Count < 0)
                    dtAll = dt.Clone();
                dtAll.Merge(dt);
            }
            dtAll.DefaultView.Sort = "SHIPMENT_ID ASC,CNTR_NO ASC,DN_NO ASC";
            dtAll = dtAll.DefaultView.ToTable();
            SCMModelInfoToExcel scminfotoexcel = new SCMModelInfoToExcel();
            string xlsFile = scminfotoexcel.ResetXls(userinfo, dtAll, fileName);
            return xlsFile;
        }

        public string GetDateFormat(DateTime? dateTime, string format)
        {
            if (new DateTime(2000, 1, 1).CompareTo(dateTime) > 0)
            {
                return string.Empty;
            }
            return Prolink.Math.GetValueAsDateTime(dateTime).ToString(format);
        }

        private string GetReserveNoByCntrNo(DataTable smrcntr, string shipmentId, string cntrNo)
        {
            DataRow[] rows = smrcntr.Select($"SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND CNTR_NO={SQLUtils.QuotedStr(cntrNo)}");
            if (rows == null || rows.Length <= 0) return null;
            return rows.Select(row => Prolink.Math.GetValueAsString(row["RESERVE_NO"])).FirstOrDefault();
        }

        private string GetReserveNoByDnNo(DataTable smrcntr, string shipmentId, string dnNo)
        {
            DataRow[] rows = smrcntr.Select($"SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND DN_NO={SQLUtils.QuotedStr(dnNo)}");
            if (rows == null || rows.Length <= 0) return null;
            return rows.Select(row => Prolink.Math.GetValueAsString(row["RESERVE_NO"])).FirstOrDefault();
        }


        private DataRow GetSmirvRow(DataTable smirv, string reserveNo)
        {
            DataRow[] rows = smirv.Select($"RESERVE_NO={SQLUtils.QuotedStr(reserveNo)}");
            if (rows.Length <= 0) return null;
            return rows[0];
        }

        private DataRow GetSmicntrRow(DataTable smicntr, string shipmentId, string cntrNo)
        {
            DataRow[] rows = smicntr.Select($"SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND CNTR_NO={SQLUtils.QuotedStr(cntrNo)}");
            if (rows.Length <= 0) return null;
            return rows[0];
        }

        private DataRow GetSmidnRow(DataTable smidn, string shipmentId, string dnNo)
        {
            DataRow[] rows = smidn.Select($"SHIPMENT_ID={SQLUtils.QuotedStr(shipmentId)} AND DN_NO={SQLUtils.QuotedStr(dnNo)}");
            if (rows.Length <= 0) return null;
            return rows[0];
        }

        private DataTable GetSmicntrTable(List<string> fcllist)
        {
            OperationUtils.Logger.WriteLog("SmicntrTable开始:" + DateTime.Now);
            string sql = string.Format(@"SELECT ETA_MSL,ETA_MSL_TIME,CNTR_NO,SHIPMENT_ID FROM SMICNTR WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(fcllist.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("SmicntrTable结束:" + DateTime.Now);
            return dt;
        }

        private DataTable GetSmrcntrTable(List<string> fcllist)
        {
            OperationUtils.Logger.WriteLog("SmrcntrTable开始:" + DateTime.Now);
            string sql = string.Format(@"SELECT RESERVE_NO,CNTR_NO,SHIPMENT_ID FROM SMRCNTR WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(fcllist.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("SmrcntrTable结束:" + DateTime.Now);
            return dt;
        }

        private DataTable GetSmirvTable(List<string> fcllist)
        {
            OperationUtils.Logger.WriteLog("SmirvTable开始:" + DateTime.Now);
            string sql = string.Format(@" SELECT RESERVE_DATE,ORDER_DATE_L AS APPOINTMENT_TIME,ARRIVAL_FACT_DATE_L AS ARRIVAL_FACT_DATE,RESERVE_NO
            FROM SMIRV WHERE SHIPMENT_INFO IN{0}", SQLUtils.Quoted(fcllist.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("SmirvTable结束:" + DateTime.Now);
            return dt;
        }

        private DataTable GetSmidnTable(List<string> normallist)
        {
            OperationUtils.Logger.WriteLog("SmidnTable结束:" + DateTime.Now);
            string sql = string.Format(@"SELECT ETA_MSL,ETA_MSL_TIME,DN_NO,SHIPMENT_ID FROM SMIDN WHERE SHIPMENT_ID IN{0}", SQLUtils.Quoted(normallist.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("SmirvTable结束:" + DateTime.Now);
            return dt;
        }

        private DataTable GetSmrdn(List<string> normallist)
        {
            OperationUtils.Logger.WriteLog("SmrdnTable结束:" + DateTime.Now);
            string sql = string.Format(@"SELECT RESERVE_NO,DN_NO,SHIPMENT_ID FROM SMRDN WHERE SHIPMENT_ID IN{0}", SQLUtils.Quoted(normallist.ToArray()));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection(), 0, 0, 1000);
            OperationUtils.Logger.WriteLog("SmrdnTable结束:" + DateTime.Now);
            return dt;
        }

        private void UploadToFTP(string tempFilePath, BaseUserInfo userinfo)
        {
            FtpHelp dd = new FtpHelp();
            dd.FtpWeb(userinfo.CompanyId + "/" + userinfo.UserId + "/SCMByModel");
            OperationUtils.Logger.WriteLog("Ftp=开始:" + tempFilePath);
            bool isupload = false;
            try
            {
                isupload = dd.Upload(tempFilePath);
                isupload = true;
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("Ftp 上传错误，错误异常：" + ex.ToString());
            }

            if (isupload)
            {
                OperationUtils.Logger.WriteLog("上传文档成功=" + tempFilePath);
            }
            //删除文件
            try
            {
                System.IO.File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("删除文件异常=" + tempFilePath + "Exception:" + ex.ToString());
            }

        }
    }

    
}
