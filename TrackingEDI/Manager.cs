using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.InboundBusiness;
using TrackingEDI.Model;

namespace TrackingEDI
{
    public class Manager
    {
        /// <summary>
        /// 保存货况
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool SaveStatus(Status status,TraceModes mode= TraceModes.Prolink)
        {
            try
            {
                TraceStatus ts = new TraceStatus();
                return ts.SaveModel(status, mode);
            }
            catch { return false; }
        }

        public static bool IBSaveStatus(Status status, TraceModes mode = TraceModes.Prolink)
        {
            try
            {
                IBTraceStatus ts = new IBTraceStatus();
                return ts.SaveModel(status, mode);
            }
            catch { return false; }
        }

        /// <summary>
        /// 设置完日期后的触发回调方法
        /// </summary>
        public static Func<string, bool> AfterSetShipmentStatusTime = null;

        /// <summary>
        /// 设置shipment的货况时间
        /// </summary>
        /// <param name="u_id"></param>
        public static void SetShipmentStatusTime(string master_no)
        {
            string sql = string.Format("SELECT * FROM TKBLST WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE (MASTER_NO={0} OR HOUSE_NO={0}))", SQLUtils.QuotedStr(master_no));
            DataTable stsDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM TKBL WHERE (MASTER_NO={0} OR HOUSE_NO={0}))", SQLUtils.QuotedStr(master_no));
            DataTable smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = string.Format("SELECT * FROM SMRV WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM TKBL WHERE (MASTER_NO={0} OR HOUSE_NO={0}))", SQLUtils.QuotedStr(master_no));
            DataTable smrvDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            List<string> shipmentList = new List<string>();
            List<string> list = new List<string>();
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            EditInstruct tkblei = null;
            EditInstruct inboundei = null;

            EditInstruct ei2 = null;
            foreach (DataRow dr in smDt.Rows)
            {
                string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string iscombinbl = Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"]);
                string id = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (shipmentList.Contains(shipment_id))
                    continue;
                shipmentList.Add(shipment_id);

                DataRow[] stss = stsDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)), "EVEN_DATE ASC");
                DataRow[] strvs = smrvDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)), "SEAL_DATE DESC");
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
                string podCd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                DateTime? atp = GetStatusDate(stsDt, shipment_id, "'300','310'", "STS_CD,EVEN_DATE");
                DateTime? ata = GetStatusDate(stsDt, shipment_id, "'500','510','S15'", "STS_CD,EVEN_DATE DESC");
                DateTime? emptytime = GetStatusDate(stsDt, shipment_id, "'740'", "STS_CD,EVEN_DATE DESC");
                //if (ata == null)
                //    ata = GetLocationStatusDate(stsDt, shipment_id, podCd, "EVEN_DATE");

                DateTime? atd = GetStatusDate(stsDt, shipment_id, "'400','420','S14'", "STS_CD,EVEN_DATE ASC");
                //if (atd == null)
                //    atd = GetStatusDate(stsDt, shipment_id, "420", "EVEN_DATE");
                //if (atd == null)
                //    atd = GetLocationStatusDate(stsDt, shipment_id, polCd);

                DateTime? ata_d = GetStatusDate(stsDt, shipment_id, "'700'");


                ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipment_id);

                tkblei= new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
                tkblei.PutKey("SHIPMENT_ID", shipment_id);

                inboundei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                inboundei.PutKey("SHIPMENT_ID", shipment_id);
                ei2 = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
                ei2.PutKey("SHIPMENT_ID", shipment_id);

                EditInstruct inboundCntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                inboundCntrei.PutKey("SHIPMENT_ID", shipment_id);

                DateTime now = DateTime.Now;
                bool up = false;
                bool inboudcntrup = false;
                //DateTime checkDate=new DateTime(now.Year,now.Month,now.Day,0,0,0);
                if (ata != null && ata.Value.CompareTo(now) <= 0 && (dr["ATA"] == null || dr["ATA"] == DBNull.Value) && CheckSealDate(strvs, ata, iscombinbl))
                {
                    up = true;
                    ei.PutDate("ATA", ata.Value);
                    tkblei.PutDate("ATA", ata.Value);
                    inboundei.PutDate("ATA", ata.Value);
                }
                if( emptytime != null && emptytime.Value.CompareTo(now) <= 0 ){
                    inboundCntrei.PutDate("EMPTY_TIME", emptytime.Value);
                    inboudcntrup = true;
                }

                if (atd != null && atd.Value.CompareTo(now) <= 0 && (dr["ATD"] == null || dr["ATD"] == DBNull.Value) && CheckSealDate(strvs, atd, iscombinbl))
                {

                    up = true;
                    ei.PutDate("ATD", atd.Value);
                    tkblei.PutDate("ATD", atd.Value);
                    inboundei.PutDate("ATD", atd.Value);
                    if (!list.Contains(id)) list.Add(id);
                    ei2.PutDate("ATD", atd.Value);
                }

                if (ata_d != null && ata_d.Value.CompareTo(now) <= 0 && (dr["ATA_D"] == null || dr["ATA_D"] == DBNull.Value) && CheckSealDate(strvs, ata_d, iscombinbl))
                {
                    up = true;
                    ei.PutDate("ATA_D", ata_d.Value);
                }

                if (atp != null && atp.Value.CompareTo(now) <= 0 && (dr["ATP"] == null || dr["ATP"] == DBNull.Value) && CheckSealDate(strvs, atp, iscombinbl))
                {
                    up = true;
                    ei.PutDate("ATP", atp.Value);
                    if (!list.Contains(id)) list.Add(id);
                }

                List<string> Vessel = new List<string>();
                List<string> Voyage = new List<string>();
                List<string> Remark = new List<string>();
                foreach (DataRow stdr in stss)
                {
                    string remarkinfo = Prolink.Math.GetValueAsString(stdr["REMARK"]);
                    if (remarkinfo.Contains("Mode:") && remarkinfo.Contains("("))
                    {
                        string remarks = remarkinfo.Replace("Mode:", "");
                        if (Vessel.Contains(remarks.Split('(')[0])) continue;
                        Vessel.Add(remarks.Split('(')[0]);
                        Voyage.Add(remarks.Split('(')[1].Replace(")", ""));
                        Remark.Add(remarkinfo);
                    }
                }

                for (int i = 0; i < Remark.Count; i++)
                {
                    if (i > 3) continue;
                    string field = "ATD" + (i + 1);
                    DateTime? vvatd = GetVessealStatusDate(stsDt, shipment_id, "'400','VD'",Remark[i],"EVEN_DATE DESC");
                    if (vvatd != null && vvatd.Value.CompareTo(now) <= 0 && (dr[field] == null || dr[field] == DBNull.Value))
                    {
                        up = true;
                        ei.PutDate(field, vvatd.Value);
                    }

                    DateTime? vvata = GetVessealStatusDate(stsDt, shipment_id, "'500','VA'", Remark[i], "EVEN_DATE DESC");
                    field = "ATA" + (i + 1);
                    if (vvata != null && vvata.Value.CompareTo(now) <= 0 && (dr[field] == null || dr[field] == DBNull.Value))
                    {
                        up = true;
                        ei.PutDate(field, vvata.Value);
                    }
                }
                int count=Vessel.Count;
                for (int i = 1; i <= count; i++)
                {
                    if (i > 4) continue;
                    string field = "VESSEL" + i;
                    string dbvessel = Prolink.Math.GetValueAsString(dr[field]);
                    if ((dbvessel == null || string.IsNullOrEmpty(dbvessel) || "NULL".Equals(dbvessel.ToUpper())) && !string.IsNullOrEmpty(Vessel[i - 1]))
                    {
                        up = true;
                        ei.Put(field, Vessel[i - 1].ToString());
                        ei.Put("VOYAGE" + i, Voyage[i - 1].ToString());

                        tkblei.Put(field, Vessel[i - 1].ToString());
                        tkblei.Put("VOYAGE" + i, Voyage[i - 1].ToString());

                        inboundei.Put(field, Vessel[i - 1].ToString());
                        inboundei.Put("VOYAGE" + i, Voyage[i - 1].ToString());
                    }
                }
                if (up)
                {
                    ml.Add(ei);
                    ml.Add(ei2);
                    ml.Add(inboundei);
                    ml.Add(tkblei);
                }
                if (inboudcntrup){
                    ml.Add(inboundCntrei);
                }
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (AfterSetShipmentStatusTime != null)
            {
                foreach (string id in list)
                    AfterSetShipmentStatusTime(id);
            }
        }

        private static bool CheckSealDate(DataRow[] strvs, DateTime? datetime,string iscombinbl)
        {
            if ("C".Equals(iscombinbl) || "Y".Equals(iscombinbl))
                return true;
            bool isupata = false;
            
            if (strvs.Length > 0)
            {
                string sealdate = Prolink.Math.GetValueAsString(strvs[0]["SEAL_DATE"]);
                if (string.IsNullOrEmpty(sealdate))
                {
                    isupata = false;
                    if ("O".Equals(Prolink.Math.GetValueAsString(strvs[0]["STATUS"])))  //外仓出货可能Seal_Date为空，但是状态为离厂
                    {
                        isupata = true ;
                    }
                }
                else
                {
                    DateTime sealtime = (DateTime)strvs[0]["SEAL_DATE"];
                    if (datetime.Value.CompareTo(sealtime) <= 0)
                    {
                        isupata = false;
                    }
                    else
                    {
                        isupata = true;
                    }
                }
            }
            return isupata;
        }

        private static DateTime? GetLocationStatusDate(DataTable stsDt, string shipment_id, string port, string sort = "EVEN_DATE DESC")
        {
            DateTime? date = null;
            if (string.IsNullOrEmpty(port))
                return date;

            List<string> checkList = new List<string>() { "100", "040", "035", "020", "010", "000"};
            DataRow[] stss = stsDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)), sort);
            if (stss.Length > 0)
            {
                foreach (DataRow st in stss)
                {
                    string code = Prolink.Math.GetValueAsString(st["STS_CD"]);
                    if (string.IsNullOrEmpty(code) || checkList.Contains(code))
                        continue;

                    if (st["EVEN_DATE"] == null || st["EVEN_DATE"] == DBNull.Value)
                        continue;
                    string location = Prolink.Math.GetValueAsString(st["LOCATION"]);
                    if (string.IsNullOrEmpty(location))
                        continue;
                    if (port.Contains(location))
                    {
                        date = (DateTime)st["EVEN_DATE"];
                        break;
                    }
                }
            }
            return date;
        }

        private static DateTime? GetStatusDate(DataTable stsDt, string shipment_id, string code, string sort = "STS_CD,EVEN_DATE DESC", string place = "")
        {
            DateTime? date=null;
            string filter = string.Format("SHIPMENT_ID={0} AND STS_CD IN ({1})", SQLUtils.QuotedStr(shipment_id), code);
            DataRow[] stss = stsDt.Select(filter, sort);
            if (stss.Length > 0)
            {
                foreach (DataRow st in stss)
                {
                    string location = Prolink.Math.GetValueAsString(st["LOCATION"]);
                    if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(place) && !place.ToUpper().Contains(location.ToUpper()))
                        continue;
                    if (st["EVEN_DATE"] == null || st["EVEN_DATE"] == DBNull.Value)
                        continue;
                    date = (DateTime)st["EVEN_DATE"];
                    break;
                }
            }
            return date;
        }

        private static DateTime? GetVessealStatusDate(DataTable stsDt, string shipment_id, string code,string remark, string sort = "EVEN_DATE DESC")
        {
            DateTime? date = null;
            string filter = string.Format("SHIPMENT_ID={0} AND REMARK={1} AND STS_CD IN ({2}) AND STS_DESCP LIKE 'Actual%'", SQLUtils.QuotedStr(shipment_id),SQLUtils.QuotedStr(remark),code);
            DataRow[] stss = stsDt.Select(filter, sort);
            if (stss.Length > 0)
            {
                foreach (DataRow st in stss)
                {
                    if (st["EVEN_DATE"] == null || st["EVEN_DATE"] == DBNull.Value)
                        continue;
                    date = (DateTime)st["EVEN_DATE"];
                    break;
                }
            }
            return date;
        }
        public static bool CheckFSSPSite(string cmp)
        {
            string sql = string.Format("SELECT * FROM BSCODE WHERE CD_TYPE='FSSP'");
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
                return true;
            foreach (DataRow dr in dt.Rows)
            {
                string cd = Prolink.Math.GetValueAsString(dr["CD"]).ToUpper();
                if (cmp.ToUpper().Equals(cd))
                    return true;
            }
            return false;
        }
    }
}
