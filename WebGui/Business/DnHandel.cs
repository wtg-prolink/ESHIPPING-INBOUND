using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebGui.Controllers;

namespace Business
{
    public class DnHandel
    {

        public string Quadruple(string dnno, string newdnno)
        {
            string message=string.Empty;
            
            string sql = string.Format("SELECT * FROM SMDN WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDND WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable smdnddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNP WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable smdnpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNPT WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable smdnptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMDNS WHERE DN_NO={0}", SQLUtils.QuotedStr(dnno));
            DataTable smdnsdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            string newguid = System.Guid.NewGuid().ToString();

            ToEi(dt, "SMDN", ml, newdnno, newguid);
            ToEi(smdnddt, "SMDND", ml, newdnno, newguid);
            ToEi(smdnpdt, "SMDNP", ml, newdnno, newguid);
            ToEi(smdnptdt, "SMDNPT", ml, newdnno, newguid);
            ToEi(smdnsdt, "SMDNS", ml, newdnno, newguid);

            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    message = ex.ToString();
                }
            }
            return message;
        }

        private static void ToEi(DataTable dt, string tableName, MixedList ml,string dnNo,string uid)
        {
            EditInstruct ei = null;
            string name = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                ei = new EditInstruct(tableName, EditInstruct.INSERT_OPERATION);
                foreach (DataColumn col in dt.Columns)
                {
                    name = col.ColumnName;
                    switch (name.ToUpper())
                    {
                        case "U_ID":
                            if (tableName.Equals("SMDN") || tableName.Equals("SMDNPT"))
                            {
                                ei.Put("U_ID", uid);
                            }
                            else
                            {
                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            }
                            continue;
                        case "DN_NO":
                            ei.Put("DN_NO", dnNo);
                            continue;
                        case "U_FID":
                            ei.Put("U_FID", uid);
                            continue;
                    }
                    if (dr[name] is DateTime)
                    {
                        if (dr[name] != null && dr[name] != DBNull.Value)
                            ei.PutDate(name, (DateTime)dr[name]);
                    }
                    else
                        ei.Put(name, dr[name]);
                }
                ml.Add(ei);
            }
        }

        /// <summary>
        /// 更改DN后，重新Reload数量金额毛净体等
        /// </summary>
        /// <param name="combin"></param>
        /// <param name="shipmentid"></param>
        /// <param name="smstatus"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string DnCombineSM(string combin,string shipmentid,string smstatus, UserInfo userinfo,bool isprice=false )
        {
            TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
            TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
            tqc = cm.CaCulateCombinBySM(shipmentid);
            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            cm.CaCulatePutEi(ref ei, tqc,shipmentid,ml);
            ei.Remove("INSTRUCTION");
            ml.Add(ei);

            //写入异常
            TmexpHandler th=new TmexpHandler();
            TmexpInfo tpi = new TmexpInfo();
            tpi.UFid = Guid.NewGuid().ToString();
            tpi.WrId = userinfo.UserId;
            tpi.WrDate = DateTime.Now;
            tpi.Cmp = userinfo.CompanyId;
            tpi.GroupId = userinfo.GroupId;
            tpi.JobNo = shipmentid;
            tpi.ExpType = "DN";
            tpi.ExpReason = "DNQ";
            if(isprice){
                tpi.ExpText = @Resources.Locale.L_DnHandel_Business_57 + userinfo.UserId + @Resources.Locale.L_DnHandel_Business_59;
            }else{
                tpi.ExpText = @Resources.Locale.L_DnHandel_Business_57 + userinfo.UserId + @Resources.Locale.L_DnHandel_Business_60;
            }
            tpi.ExpObj = userinfo.UserId;
            ml.Add(th.SetTmexpEi(tpi));
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    switch (smstatus)
                    {
                        case "C":
                        case "D":
                        case "I":
                            ReloadBill(shipmentid);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message.ToString();
                }
            }
            return string.Empty;
        }

        public void ReloadBill(string shipmentid)
        {
            DataTable dt= SMHandle.GetSMByShipmentId(shipmentid);
            try
            {
                string uid = dt.Rows[0]["U_ID"].ToString();
                Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                bill.Create(uid, DateTime.Now);
            }
            catch (Exception ex)
            {
            }
        }

        public DataTable GetVSMDNByDnNo(string dnno)
        {
            string sql = "SELECT * FROM V_SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dnno);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt; 
        }
    }

    public class TmexpInfo
    {
        public string UId { get; set; }
        public string UFid { get; set; }
        public string JobNo { get; set; }
        public string SeqNo { get; set; }
        public string Dep { get; set; }
        /// <summary>
        /// 异常种类* DN=DN異常，SM=訂艙異常，BILL=帳單異常
        /// </summary>
        public string ExpType { get; set; }
        /// <summary>
        /// 填写人
        /// </summary>
        public string WrId { get; set; }
        /// <summary>
        /// 填写时间
        /// </summary>
        public DateTime? WrDate { get; set; }
        /// <summary>
        /// 解除人
        /// </summary>
        public string CancelBy { get; set; }
        /// <summary>
        /// 解除时间
        /// </summary>
        public string CancelDate { get; set; }
        public string GroupId { get; set; }
        public string Cmp { get; set; }
        public string Stn { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string ExpCd { get; set; }
        /// <summary>
        /// 责任对象*
        /// </summary>
        public string ExpObj { get; set; }

        /// <summary>
        ///异常原因*DNC=DN 修改,DNA=DN 審核異常,DND=DN 取消,DNQ=DN改數量,DNN=加減DN數
        ///	BK=訂艙異常,BKC=修改航班,BKD=取消訂艙,BL=改提單,BKP=港口差异
        /// </summary>
        public string ExpReason { get; set; }
        /// <summary>
        /// 异常原因描述
        /// </summary>
        public string ExpText { get; set; }
        public string ExpDescp { get; set; }
    }

    public class TmexpHandler
    {
        public EditInstruct SetTmexpEi(TmexpInfo tpi)
        {
            if (string.IsNullOrEmpty(tpi.SeqNo))
            {
                string sql = string.Format("SELECT COUNT(1)+1 FROM TMEXP WHERE JOB_NO={0}", SQLUtils.QuotedStr(tpi.JobNo));
                tpi.SeqNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            EditInstruct ei=new EditInstruct("TMEXP", EditInstruct.INSERT_OPERATION);
            if (string.IsNullOrEmpty(tpi.UId))
                tpi.UId = Guid.NewGuid().ToString();
            ei.Put("U_ID", tpi.UId);
            ei.Put("U_FID", tpi.UFid);
            ei.Put("JOB_NO", tpi.JobNo);
            ei.Put("SEQ_NO", tpi.SeqNo);
            ei.Put("DEP", tpi.Dep);
            ei.Put("EXP_TYPE", tpi.ExpType);
            ei.Put("EXP_TEXT", tpi.ExpText);
            ei.Put("WR_ID", tpi.WrId);
            ei.PutDate("WR_DATE", tpi.WrDate);
            ei.Put("CANCEL_BY", tpi.CancelBy);
            ei.PutDate("CANCEL_DATE", tpi.CancelDate);
            ei.Put("GROUP_ID", tpi.GroupId);
            ei.Put("CMP", tpi.Cmp);
            ei.Put("STN", tpi.Stn);
            ei.Put("CREATE_BY", tpi.CreateBy);
            ei.PutDate("CREATE_DATE", tpi.CreateDate);
            ei.Put("MODIFY_BY", tpi.ModifyBy);
            ei.PutDate("MODIFY_DATE", tpi.ModifyDate);
            ei.Put("EXP_CD", tpi.ExpCd);
            ei.Put("EXP_OBJ", tpi.ExpObj);
            ei.Put("EXP_REASON", tpi.ExpReason);
            ei.Put("EXP_DESCP", tpi.ExpDescp);
            return ei;
        }
    }
}