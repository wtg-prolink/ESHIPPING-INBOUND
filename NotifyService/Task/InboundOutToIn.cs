using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NotifyService.Task
{
    public class InboundOutToIn : IPlanTask
    {

        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            _messenger = messenger;
            SetAutoIn2out();
        }
        public void SetAutoIn2out()
        {
            string datacondition = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");

            //TP unreach  去除ATD卡控时间
            string sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE STATUS!='Z' AND BL_CHECK='Y' 
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND AP_CD='TP')  OR WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND AP_CD='TP')
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND AP_CD='TP'))
		   AND (IS_OK='N' OR IS_OK IS NULL) 
             AND (ISCOMBINE_BL='C' OR IS_SPLIT_BILL='S' OR ((ISCOMBINE_BL='' OR ISCOMBINE_BL IS NULL) AND (IS_SPLIT_BILL='' OR IS_SPLIT_BILL IS NULL) AND STATUS in('F','R','O','H')))  
			", SQLUtils.QuotedStr(datacondition));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendToInbound(dt);

            sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE STATUS!='Z' AND BL_CHECK='Y' AND ATD IS NOT NULL
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))  OR WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP)))
		   AND (IS_OK='N' OR IS_OK IS NULL) AND ATD>{0}
             AND (ISCOMBINE_BL='C' OR IS_SPLIT_BILL='S' OR ((ISCOMBINE_BL='' OR ISCOMBINE_BL IS NULL) AND (IS_SPLIT_BILL='' OR IS_SPLIT_BILL IS NULL) AND STATUS in('F','R','O','H')))  
			", SQLUtils.QuotedStr(datacondition));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendToInbound(dt);

            sql = string.Format(@"SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS
                         FROM SMSM WHERE STATUS!='Z' AND IORDER='Y'  AND (ISCOMBINE_BL='C' OR ISCOMBINE_BL='' OR ISCOMBINE_BL IS NULL)
            	AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))  OR WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))
                         OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP)))
                         AND ETD>{0} AND (IS_OK='N' OR IS_OK IS NULL) AND (IS_OK_ISF='N' OR IS_OK_ISF IS NULL) ORDER BY ATD DESC ", SQLUtils.QuotedStr(datacondition));
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendToInbound(dt);

            datacondition = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
            sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE
           (STATUS='F' OR STATUS='R' OR STATUS='O' OR STATUS='H') AND TRAN_TYPE='T' 
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))  OR 
		   WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP))
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP)))
            AND (IS_OK='N' OR IS_OK IS NULL)
            AND EXISTS(SELECT 1 FROM SMRV WHERE SMRV.SHIPMENT_ID=SMSM.SHIPMENT_ID AND OUT_DATE>='2021-11-29')");
            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendToInbound(dt);


             sql = "SELECT CD,AP_CD FROM BSCODE WHERE CD_TYPE='EALS'";
            DataTable bscodedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> ealertcmp = new List<string>();
            int changehour = 0;
            foreach (DataRow dr in bscodedt.Rows)
            {
                string cmp = Prolink.Math.GetValueAsString(dr["CD"]);
                changehour = Prolink.Math.GetValueAsInt(dr["AP_CD"]);
                if ("TP".Equals(cmp))
                    continue;
                if (!ealertcmp.Contains(cmp))
                {
                    ealertcmp.Add(cmp);
                }
            }
            if (ealertcmp.Count > 0)
            {
                if (changehour == 0)
                {
                    changehour = 8;
                }
                string ealertcondition = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd");
                string createtime = DateTime.Now.AddHours(-changehour).ToString("yyyy-MM-dd HH:MM:sss");

                sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE (STATUS!='Z' AND STATUS!='V')
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD in {1})  OR 
		   WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD in {1})
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD in {1}))
		   AND (IS_OK='N' OR IS_OK IS NULL) AND  ((ATD>{0} and TRAN_TYPE in ('F','L','R')) 
		   OR(ETD>{0} and TRAN_TYPE in ('A','E'))) 
              AND (ISCOMBINE_BL='C' OR ISCOMBINE_BL='' OR ISCOMBINE_BL IS NULL)  AND (ICREATE_DATE is null OR ICREATE_DATE<{2}) 
			    ", SQLUtils.QuotedStr(ealertcondition), SQLUtils.Quoted(ealertcmp.ToArray()), SQLUtils.QuotedStr(createtime));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                SendToInbound(dt,"E");

                sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE
            (STATUS!='V' AND STATUS!='Z' AND STATUS!='F' AND STATUS!='R' AND STATUS!='O' AND STATUS!='H') AND TRAN_TYPE='T' AND TORDER='C'
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='CNI')  OR 
		   WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='CNI')
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='CNI')) 
            AND (IS_OK='N' OR IS_OK IS NULL)
           AND EXISTS(SELECT 1 FROM SMRV WHERE SMRV.SHIPMENT_ID=SMSM.SHIPMENT_ID AND CALL_DATE>='2021-11-29')");
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                SendToInbound(dt, "E");

                sql = string.Format(@" SELECT SMSM.ISCOMBINE_BL,SMSM.SHIPMENT_ID,SMSM.CMP,SMSM.IS_OK,SMSM.ATD,SMSM.STATUS FROM SMSM WHERE (STATUS!='Z' AND STATUS!='V')
           AND  (CS_CD IN (SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='TP')  OR 
		   WE_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='TP')
            OR FC_CD IN(SELECT CD FROM BSCODE WHERE  CD_TYPE='CULO' AND (CMP='*' OR CMP=SMSM.CMP) AND ap_CD='TP'))
		   AND (IS_OK='N' OR IS_OK IS NULL) AND  (ETD>{0} and TRAN_TYPE in ('F','L','R','A','E')) 
              AND (ISCOMBINE_BL='C' OR ISCOMBINE_BL='' OR ISCOMBINE_BL IS NULL)  AND (ICREATE_DATE is null OR ICREATE_DATE<{1}) 
			    ", SQLUtils.QuotedStr(ealertcondition), SQLUtils.QuotedStr(createtime));
                dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                SendToInbound(dt, "E");
            }
        }


        private void SendToInbound(DataTable dt, string elart = "")
        {
            string sql = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string status= Prolink.Math.GetValueAsString(dr["STATUS"]);
                if ("Z".Equals(status))
                {
                    continue;
                }
                EditInstruct smei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                smei.PutKey("SHIPMENT_ID", ShipmentId);
                string is_ok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc(ShipmentId,elart,smei);
                Prolink.DataOperation.OperationUtils.Logger.WriteLog(string.Format("ShipmentId:{0} {1}", ShipmentId, is_ok));

               
                if (is_ok == "Y")
                {
                    smei.Put("IS_OK", "Y");
                }
                if (is_ok == "S")
                {
                    smei.Put("IS_OK_ISF", "Y");
                }
                if (is_ok == "N")
                {
                    smei.Put("IS_OK", "N");
                    smei.PutDate("ICREATE_DATE", DateTime.Now);
                    smei.Put("ICREATE_BY", "JOB");
                }
                try
                {
                    OperationUtils.ExecuteUpdate(smei, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (is_ok == "Y")
                    {
                        sql = string.Format("SELECT COMBINE_INFO,SHIPMENT_ID,CMP,IS_OK FROM SMSM WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(ShipmentId));
                        DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        foreach (DataRow dritem in dt1.Rows)
                        {
                            Business.TPV.Helper.SendICACargoInfo(dritem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog(string.Format("ShipmentId:{0} {1}", ShipmentId, ex.ToString()));
                }
            }
        }
    }
}
