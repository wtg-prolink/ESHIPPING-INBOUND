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
    /// <summary>
    /// 过账EDI
    /// </summary>
    public class PostingBillTask : IPlanTask
    {
        private string _hour = null;
        private bool flag = true;
        public PostingBillTask(string hour)
        {
            _hour = hour;
        }

        IPlanTaskMessenger _messenger;
        string _location = "";
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            if (IsExecute(_hour, ref flag) && !flag)
            {
                SetPostInboundBill();
            }
        }

        public bool IsExecute(string parmhour, ref bool flag)
        {
            string[] hours = parmhour.Split(';');  //7,12,15

            int hour = DateTime.Now.Hour;
            bool matchhour=false;
            foreach (string index in hours)
            {
                if (string.IsNullOrEmpty(index)) continue;
                if (Prolink.Math.GetValueAsInt(index) == hour)
                matchhour=true;
            }
            
            if (matchhour && flag)
            {
                flag = false;
                return true;
            }
            else if (!matchhour && !flag)
            {
                flag = true;
                return false;
            }
            else
                return false;
        }

        public void SetPostInboundBill()
        {
            string sql = "SELECT * FROM SCMPB ";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string dncon = string.Empty;
            string cntrcon = string.Empty;
            string smcon = string.Empty;
            DsiposeDestination();
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                DataRow dr = dt.Rows[i];
                string Incoterm = Prolink.Math.GetValueAsString(dr["INCOTERM"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string IncotermDescp = Prolink.Math.GetValueAsString(dr["INCOTERM_DESCP"]);
                string IsEtaMsl = Prolink.Math.GetValueAsString(dr["IS_ETA_MSL"]);
                string IsEta = Prolink.Math.GetValueAsString(dr["IS_ETA"]);

                dncon = " IS_POSTBILL IS NULL ";
                cntrcon = " IS_POSTBILL IS NULL ";
                smcon = "1=1 ";
                string field = "";
                if (!string.IsNullOrEmpty(Incoterm))
                {
                    smcon += string.Format(" AND SMSMI.INCOTERM_CD={0} ", SQLUtils.QuotedStr(Incoterm));
                }
                if (!string.IsNullOrEmpty(IncotermDescp))
                {
                    smcon += string.Format(" AND SMSMI.INCOTERM_DESCP={0} ", SQLUtils.QuotedStr(IncotermDescp));
                }

                int nowDay = DateTime.Now.Day;
                string datacondition = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                if (nowDay >= 3)
                {
                    datacondition = DateTime.Now.AddDays(1 - DateTime.Now.Day).ToString("yyyy-MM-dd");
                }
                if (IsEtaMsl.Equals("Y"))
                {
                    dncon = string.Format(@" EXISTS(SELECT 1 FROM SMIRV WHERE RESERVE_NO =SMRDN.RESERVE_NO AND RESERVE_DATE IS NOT NULL AND RESERVE_DATE > CONVERT(DATETIME,{0}) AND RESERVE_DATE<GETDATE()) 
						AND EXISTS(SELECT 1 FROM SMIDN WHERE IS_POSTBILL IS NULL AND DN_NO=SMRDN.DN_NO AND SHIPMENT_ID=SMRDN.SHIPMENT_ID)", SQLUtils.QuotedStr(datacondition));
                    cntrcon = string.Format(@" RESERVE_DATE IS NOT NULL AND RESERVE_DATE > CONVERT(DATETIME, {0}) AND RESERVE_DATE<GETDATE()", SQLUtils.QuotedStr(datacondition));
                    field = "DLV_DATE";

                    sql = string.Format(@"SELECT SMRDN.SHIPMENT_ID,SMRDN.CMP,SMRDN.DN_NO,(SELECT CD FROM BSCODE WHERE CD_TYPE = 'TSAP' AND CMP = {0}) AS SAP_ID,
                        (SELECT TOP 1 RESERVE_DATE FROM SMIRV WHERE SMIRV.RESERVE_NO = SMRDN.RESERVE_NO AND STATUS != 'V') AS DLV_DATE,
                        (SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID = SMRDN.SHIPMENT_ID) AS ETA,
                        (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID = SMRDN.SHIPMENT_ID) AS TRAN_TYPE
                    FROM SMRDN WHERE {1}
                    AND SMRDN.SHIPMENT_ID IN 
                        (SELECT SHIPMENT_ID FROM SMSMI WHERE SMSMI.CMP= {0} AND SMSMI.O_LOCATION IS NOT NULL  AND SMSMI.TRAN_TYPE IN ('A','L','E','T') AND  
                           {2} AND (IS_POSTBILL IS NULL OR IS_POSTBILL != 'Y'))",
                          SQLUtils.QuotedStr(cmp), dncon, smcon);
                    PostBillItem(sql, field);
                    string cntrsql = string.Format(@"SELECT SMICNTR.SHIPMENT_ID,SMICNTR.CMP,SMICNTR.DN_NO,(SELECT CD FROM BSCODE WHERE CD_TYPE='TSAP' AND CMP={0}) AS SAP_ID,
                        SMIRV.RESERVE_DATE AS DLV_DATE,
                        (SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID = SMIRV.SHIPMENT_ID) AS ETA,
                        (SELECT TOP 1 TRAN_TYPE FROM SMSMI WHERE SMSMI.SHIPMENT_ID = SMIRV.SHIPMENT_ID) AS TRAN_TYPE
                    FROM SMIRV, SMICNTR  
                    WHERE {1} AND SMIRV.SHIPMENT_ID IN(SELECT SHIPMENT_ID FROM SMSMI WHERE SMSMI.CMP= {0} AND SMSMI.O_LOCATION IS NOT NULL  AND SMSMI.TRAN_TYPE IN ('R','F') AND
                        {2} AND(IS_POSTBILL IS NULL OR IS_POSTBILL != 'Y')) 
                    AND SMICNTR.IS_POSTBILL IS NULL AND SMICNTR.CNTR_NO = SMIRV.CNTR_NO AND SMICNTR.SHIPMENT_ID = SMIRV.SHIPMENT_ID", SQLUtils.QuotedStr(cmp), cntrcon, smcon);
                    PostBillItem(cntrsql, field);
                }
                else if (IsEta.Equals("Y"))
                {
                    smcon += string.Format(" AND SMSMI.ETA IS NOT NULL AND SMSMI.ETA>={0} AND ETA<=GETDATE()", SQLUtils.QuotedStr(datacondition));
                    field = "ETA";

                    sql = string.Format(@"SELECT DN.SHIPMENT_ID,DN.CMP,DN.DN_NO,DN.SAP_ID,DN.DLV_DATE,SMSMI.ETA,SMSMI.TRAN_TYPE
                        FROM(SELECT  '' AS DLV_DATE, DN_NO, CMP, (SELECT CD FROM BSCODE WHERE CD_TYPE = 'TSAP' AND CMP = {0}) AS SAP_ID, SHIPMENT_ID FROM SMIDN
                        WHERE IS_POSTBILL IS NULL )AS DN JOIN SMSMI
                        ON DN.SHIPMENT_ID = SMSMI.SHIPMENT_ID AND SMSMI.CMP = {0} AND SMSMI.O_LOCATION IS NOT NULL AND SMSMI.TRAN_TYPE IN('A','L','E','T') 
                        WHERE {1} 
                         union              
                        SELECT CNTR.SHIPMENT_ID,CNTR.CMP,CNTR.DN_NO,CNTR.SAP_ID,CNTR.DLV_DATE,SMSMI.ETA,SMSMI.TRAN_TYPE
                        FROM(SELECT  '' AS DLV_DATE, DN_NO, CNTR_NO, CMP, (SELECT CD FROM BSCODE WHERE CD_TYPE = 'TSAP' AND CMP = {0}) AS SAP_ID, SHIPMENT_ID FROM SMICNTR
                        WHERE IS_POSTBILL IS NULL )AS CNTR JOIN SMSMI
                        ON CNTR.SHIPMENT_ID = SMSMI.SHIPMENT_ID AND SMSMI.CMP = {0} AND SMSMI.O_LOCATION IS NOT NULL  AND SMSMI.TRAN_TYPE IN('R','F') WHERE {1} ", SQLUtils.QuotedStr(cmp), smcon);
                    PostBillItem(sql, field);
                }
            }
        }

        public void PostBillItem(string sql,string field)
        {
            DataTable dntable = null;
            try
            {
                dntable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex) {
                //string subject = "Posting Bill Task过账异常 !";
                //string body = subject + ex.Message;
                //SendMail(Guid.NewGuid().ToString(), subject, body);
                return;
            }
            
            for (int j = 0; j < dntable.Rows.Count; j++)
            {
                DataRow dnrows = dntable.Rows[j];
                string dnno = Prolink.Math.GetValueAsString(dnrows["DN_NO"]);
                string sapid = Prolink.Math.GetValueAsString(dnrows["SAP_ID"]);
                string trantype = Prolink.Math.GetValueAsString(dnrows["TRAN_TYPE"]);

                string shipmentid = Prolink.Math.GetValueAsString(dnrows["SHIPMENT_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dnrows["CMP"]);
                DateTime postdate = new DateTime();
                if (dnrows[field] != null && dnrows[field] != DBNull.Value)
                {
                    postdate = Prolink.Math.GetValueAsDateTime(dnrows[field]);
                }
                else
                {
                    continue;
                }
                string[] dnnos = dnno.Split(',');
                if (dnnos.Length > 1)
                {
                    foreach (string dnitem in dnnos)
                    {
                        if (string.IsNullOrEmpty(dnitem)) continue;
                        Inboundpostbill(sapid, dnitem, postdate, cmp, shipmentid, trantype);
                    }
                }
                else
                {
                    Inboundpostbill(sapid, dnno, postdate, cmp, shipmentid, trantype);
                }
            }
        }

        private void Inboundpostbill(string sapId, string dnno, DateTime date, string cmp, string shipmentid,string trantype)
        {
            MixedList ml = new MixedList();
            try
            {
                Business.TPV.Import.DeliveryPostingManager dpManager = new Business.TPV.Import.DeliveryPostingManager();
                Business.TPV.Import.DeliveryPostingInfo dpInfo = new Business.TPV.Import.DeliveryPostingInfo();

                string sql1 = string.Format("SELECT ISNULL((SELECT TOP 1 DN_NO_CMP_REF FROM SMDN f1 WHERE F1.REF_NO=C.DN_NO),C.DN_NO_CMP_REF) " +
                    " FROM SMDN C WHERE DN_NO = {0}", SQLUtils.QuotedStr(dnno));
                string firstdn=OperationUtils.GetValueAsString(sql1, Prolink.Web.WebContext.GetInstance().GetConnection());
                if(string.IsNullOrEmpty(firstdn))
                {
                    firstdn = dnno;
                }
                dpInfo.DNNO = firstdn;
                
                dpInfo.GoodsMovementDate = date;
                dpInfo.CMP = cmp;
                Business.TPV.RFC.DPResultInfo result = null;
                bool isSucceed = dpManager.TryPostingDate(sapId, dpInfo, out result, "");
                if (isSucceed)
                {
                    EditInstruct ei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("DN_NO", dnno);
                    ei.Put("IS_POSTBILL", "Y");
                    ei.PutDate("POST_BILL_DATE", date);
                    ml.Add(ei);
                    try {
                        OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception e) { }

                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        string simcntrsql = string.Format("SELECT U_ID,DN_NO FROM SMICNTR WHERE SHIPMENT_ID={0} AND DN_NO LIKE '%{1}%'", SQLUtils.QuotedStr(shipmentid), dnno);
                        DataTable dt = OperationUtils.GetDataTable(simcntrsql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow dr in dt.Rows)
                        {
                            string cntruid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                            string dnnos = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                            string[] dnnolist = dnnos.Split(',');
                            simcntrsql = string.Format("SELECT COUNT(1) FROM SMIDN WHERE SHIPMENT_ID={0} AND (IS_POSTBILL!='Y' OR IS_POSTBILL IS NULL) AND DN_NO IN {1}", SQLUtils.QuotedStr(shipmentid), SQLUtils.Quoted(dnnolist));
                            int count = OperationUtils.GetValueAsInt(simcntrsql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            if (count == 0)
                            {
                                string updatesql = string.Format(@"UPDATE SMICNTR SET IS_POSTBILL='Y' WHERE U_ID={0}", SQLUtils.QuotedStr(cntruid));
                                OperationUtils.ExecuteUpdate(updatesql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                        }
                    }
                    TrackingEDI.InboundBusiness.SMSMIHelper.UpdatePostBill(shipmentid, dnno);
                }
                else
                {
                    //string subject = "DN:" + dnno + " 过账失败 ";
                    //string body = subject + result.MsgText;
                    //SendMail(dnno, subject, body);
                }
            }
            catch (Exception ex)
            {
                //string subject = "DN:" + dnno + " 过账异常 ";
                //string body = subject + ex.Message.ToString();
                //SendMail(dnno, subject, body);
            }
        }

        public void DsiposeDestination()
        {
            Business.TPV.Import.DeliveryPostingManager dpManager = new Business.TPV.Import.DeliveryPostingManager();
            dpManager.DsiposeDestination();
        }
    }
}
