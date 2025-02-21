using Business.Service;
using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;

namespace Business
{
    public class SMHandle
    {
        /*订舱管理： 
        1.多笔Shipment合并Shipment后，要产生一笔新的Shipment，增加一个BL Type栏位； 
        2.增加一个拆分Shipment的功能*/
        //public static string SUM_SMCOLUMN = "PKG_NUM,PGW,PCBM,PCNT_NUMBER,QTY,NW,GW,CBM,PLT_NUM,CNT20,CNT40,CNT40HQ,CNT_NUMBER,FREIGHT_AMT,INSURANCE_AMT,OF_COST,OT_COST,IF_COST,IT_COST,PCNT20,PCNT40,PCNT40HQ,PCNT_NUMBER,GVALUE,ETT,ATT,DTT,AQTY,DQTY,FQTY,FDQTY,NDQTY,";
        //public static string SUM_DNCOLUMN = "PKG_NUM,GW,NW,CBM,QTY,";//PGW,PCBM

        public static string  CombineShipment(string[] shipmentcombines,string groupid,string cmp,string stn,ref string _newshipmentid,string userid)
        {
            string Msg=string.Empty;
            bool isIStatus = true;
            ResultInfo resultinfo=CheckIsStatus(shipmentcombines);
            if (!resultinfo.IsSucceed)
            {
                return resultinfo.Description;
            }
            string firstshipment = shipmentcombines[0];
            string newshipmentId = resultinfo.Description;
            if (string.IsNullOrEmpty(newshipmentId))
            {
                isIStatus = false;
                newshipmentId = "S" + TransferBooking.GetSMAutoNo(groupid, cmp, stn);
                string smrvsql = string.Format(@"SELECT SHIPMENT_ID,RESERVE_NO FROM SMRV WHERE SHIPMENT_ID IN {0} ", SQLUtils.Quoted(shipmentcombines));
                DataTable dt = OperationUtils.GetDataTable(smrvsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["SHIPMENT_ID"].ToString() + @Resources.Locale.L_SMHandle_Business_98;
                }
                string _sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
                string type = OperationUtils.GetValueAsString(_sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if ("F".Equals(type))
                {
                    _sql = string.Format("SELECT TOP 1 SHIPMENT_ID FROM SMSM WHERE DEP='SS' AND SHIPMENT_ID IN ", SQLUtils.Quoted(shipmentcombines));
                    string _shipmentid = OperationUtils.GetValueAsString(_sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    firstshipment = string.IsNullOrEmpty(_shipmentid) ? firstshipment : _shipmentid;
                }
            }
            else
            {
                firstshipment = newshipmentId;
            }

            Dictionary<string, object> parm = new Dictionary<string, object>();
            MixedList ml = new MixedList();
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
            ei.PutKey("SHIPMENT_ID", firstshipment);
            ml.Add(ei);

            EditInstruct smei = new EditInstruct("SMSM", EditInstruct.DELETE_OPERATION);
            smei.PutKey("SHIPMENT_ID", firstshipment);
            ml.Add(smei);

            sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smsmdt.Rows.Count <= 0) return @Resources.Locale.L_SMHandle_Business_99 + firstshipment + @Resources.Locale.L_BookingActionController_Controllers_81;
            string ppolcd = Prolink.Math.GetValueAsString(smsmdt.Rows[0]["PPOL_CD"]);
            string newguid = System.Guid.NewGuid().ToString();
            if (isIStatus)
            {
                newguid = smsmdt.Rows[0]["U_ID"].ToString();
            }
           
            ToEi(smsmdt, "SMSM", ml, newshipmentId, newguid, parm);
            ToEi(smsmptdt, "SMSMPT", ml, newshipmentId, newguid,null);
            ChangeDNInfo(ml, shipmentcombines, newshipmentId);
            RemoveShipment(ml, shipmentcombines, newshipmentId);
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    try
                    {
                        BookingParser bp = new BookingParser();
                        bp.SaveToTrackingByShimentID(new string[] { newshipmentId });
                        TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ShipmentId = newshipmentId, StsCd = "012", Sender = userid, Location = ppolcd, LocationName = "", StsDescp = "Combine Shipment" });
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Msg = ex.ToString();
                }
            }
            _newshipmentid = newshipmentId;
            return Msg;
        }

        public static string CombineBill(string[] shipmentcombines, string groupid, string cmp, string userid, ref string _newshipmentid)
        {
            string Msg = string.Empty;
            Msg = CheckIsCombineSM(ref shipmentcombines);
            if (!string.IsNullOrEmpty(Msg)) return Msg;
            Dictionary<string, object> parm = new Dictionary<string, object>();

            string firstshipment = shipmentcombines[0];
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
            DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList ml = new MixedList();
            string newguid = System.Guid.NewGuid().ToString();
            string polcd = smsmdt.Rows[0]["POL_CD"].ToString();

            if (string.IsNullOrEmpty(_newshipmentid))
            {
                _newshipmentid = "B" + TransferBooking.GetSMAutoNo(groupid, cmp, "*");
            }
            else
            {
                sql = string.Format("SELECT MASTER_NO,HOUSE_NO,POL_CD FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(_newshipmentid));
                DataTable cbsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (cbsmdt.Rows.Count > 0)
                    polcd = cbsmdt.Rows[0]["POL_CD"].ToString();
            }
            DeleteShipmentInfo(_newshipmentid, ml);
            ToEi(smsmdt, "SMSM", ml, _newshipmentid, newguid, parm);
            ToEi(smsmptdt, "SMSMPT", ml, _newshipmentid, newguid, null);
            string shipmentinfo=string.Join(",",shipmentcombines);
            UpdateComineBill(ml, _newshipmentid, shipmentinfo);
            foreach (string shipmentid in shipmentcombines)
            {
                UpdateCombinSMInfo(_newshipmentid, ml, shipmentinfo, shipmentid);
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    try
                    {
                        BookingParser bp = new BookingParser();
                        bp.SaveToTrackingByShimentID(new string[] { _newshipmentid });
                        TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ShipmentId = _newshipmentid, StsCd = "014", Sender = userid, Location = polcd, LocationName = "", StsDescp = "Combine BL" });
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Msg = ex.ToString();
                }
            }
            return Msg;
        }

        private static void UpdateCombinSMInfo(string combineShipment, MixedList ml, string shipmentinfo, string keyshipmentid)
        {
            EditInstruct cbei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            cbei.PutKey("SHIPMENT_ID", keyshipmentid);
            cbei.Put("SHIPMENT_INFO", shipmentinfo);
            cbei.Put("COMBIN_SHIPMENT", combineShipment);
            //ei.PutDate("COMBINE_DATE", DateTime.Now);
            cbei.Put("ISCOMBINE_BL", "S");    //合并的提单，设置为Y
            //cbei.Put("BL_TYPE", "S");
            ml.Add(cbei);
            cbei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
            cbei.PutKey("SHIPMENT_ID", keyshipmentid);
            cbei.Put("COMBIN_SHIPMENT", combineShipment);
            cbei.Put("ISCOMBINE_BL", "S");    //合并的提单，设置为Y
            ml.Add(cbei);
        }

        public static void UpdateComineBill(MixedList ml,string newshipment,string shipmentinfo)
        {
            TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
            TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
            TrackingEDI.Business.CombinBLInfo cblinfo = new TrackingEDI.Business.CombinBLInfo();
            tqc = cm.CaCulateCombinBySM(shipmentinfo);
            cblinfo = cm.GetCombinBLInfo(shipmentinfo);

            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", newshipment);
            ei.Put("STATUS", "B");
            //ei.Put("CORDER", "S");
            ei.Put("BORDER","N");
            ei.Put("TORDER","N");
            ei.Put("SHIPMENT_INFO", shipmentinfo);
            ei.Put("COMBIN_SHIPMENT", newshipment);
            ei.Put("HOUSE_NO", "");
            ei.Put("MASTER_NO", "");
            ei.Put("ISCOMBINE_BL", "C");    //合并的提单，设置为Y
            //ei.Put("BL_TYPE", "C");
            cm.CaCulatePutEi(ref ei, tqc,newshipment,ml);
            cm.GetCombinBLEI(ref ei, cblinfo);
            
            ml.Add(ei);
        }

        public static string SpellCombineSM(string shipmentid,string removedn)
        {
            removedn = removedn.Trim(',');
            string[] removednitems = removedn.Split(',');
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if(dt.Rows.Count<0)
                return @Resources.Locale.L_BookingAction_Controllers_187 +shipmentid+@Resources.Locale.L_BookingActionController_Controllers_81;
            MixedList mlist = new MixedList();
            string combinedn = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);
            combinedn = combinedn.Trim(',');
            string[] combindArray = combinedn.Split(',');
            if (removednitems.Length == combindArray.Length) return @Resources.Locale.L_SMHandle_Business_100;
            string dnno= Prolink.Math.GetValueAsString(dt.Rows[0]["DN_NO"]);
            bool isolddn=false;
            if (removednitems.Contains(dnno)) isolddn = true;
            List<string> list = combindArray.ToList();
            list.RemoveAll(s=>removednitems.Contains(s));
            string[] restarray = list.ToArray();
            string rest = string.Join(",", list);

            TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
            TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
            tqc = cm.CaCulateCombineByDn(rest);
            //string marks = TrackingEDI.Business.TransferBooking.CombineMarks(rest);
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            cm.CaCulatePutEi(ref ei, tqc,shipmentid,mlist);
            if (restarray.Length > 0 && isolddn) ei.Put("DN_NO", restarray[0].ToString());
            ei.Put("COMBINE_INFO", rest);
            //ei.Put("MARKS", marks);
            mlist.Add(ei);

            EditInstruct smdnei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            smdnei.PutKey("SHIPMENT_ID", shipmentid);
            smdnei.Put("COMBINE_INFO", rest);
            smdnei.Put("BL_LEVEL", "");
            mlist.Add(smdnei);

            EditInstruct smrvei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
            smrvei.PutKey("SHIPMENT_ID", shipmentid);
            smrvei.Put("DN_NO", rest);
            mlist.Add(smrvei);
            foreach (string dn in removednitems)
            {
                ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("DN_NO", dn);
                ei.Put("SHIPMENT_ID", "");
                ei.Put("COMBINE_INFO", "");
                ei.Put("STATUS", "D");
                mlist.Add(ei);
            }

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            return string.Empty;
        }

        public static string SpellCombineBill(string shipmentids, ref string removesm, DataTable dt = null,string userid=null)
        {
            removesm = removesm.Trim(',');
            string[] removednitems = removesm.Split(',');
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentids));
            if(dt==null){
                 dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            if (dt.Rows.Count <= 0)
                return @Resources.Locale.L_BookingAction_Controllers_187 + shipmentids + @Resources.Locale.L_BookingActionController_Controllers_81;
            string combinedn = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_INFO"]);//SHIPMENT_INFO,COMBIN_SHIPMENT 
            string _newshipmentid = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBIN_SHIPMENT"]);//SHIPMENT_INFO,COMBIN_SHIPMENT
            combinedn = combinedn.Trim(',');
            string[] combindArray = combinedn.Split(',');
            string polcd = Prolink.Math.GetValueAsString(dt.Rows[0]["POL_CD"]);
            MixedList ml = new MixedList();
            if (combindArray.Length == removednitems.Length || combindArray.Length - removednitems.Length == 1)
            {
                removednitems = combindArray;
                foreach (string array in removednitems)
                {
                    CleareCombineInfo(ml, array);
                }
                DeleteShipmentInfo(_newshipmentid, ml);
            }
            else
            {
                List<string> list = combindArray.ToList();
                list.RemoveAll(s => removednitems.Contains(s));
                string[] restarray = list.ToArray();
                string rest = string.Join(",", list);
                Dictionary<string, object> parm = new Dictionary<string, object>();

                string firstshipment = restarray[0];
                sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
                DataTable smsmptdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(firstshipment));
                DataTable smsmdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                string newguid = System.Guid.NewGuid().ToString();
                if (string.IsNullOrEmpty(_newshipmentid))
                {
                    return @Resources.Locale.L_SMHandle_Business_101;
                }
                DeleteShipmentInfo(_newshipmentid, ml);
                ToEi(smsmdt, "SMSM", ml, _newshipmentid, newguid, parm);
                ToEi(smsmptdt, "SMSMPT", ml, _newshipmentid, newguid, null);
                UpdateComineBill(ml, _newshipmentid, rest);

                foreach (string array in removednitems)
                {
                    CleareCombineInfo(ml, array);
                }

                foreach (string array in restarray)
                {
                    UpdateCombinSMInfo(_newshipmentid, ml, rest, array);
                }
            }
            if (ml.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    try
                    {
                        BookingParser bp = new BookingParser();
                        bp.SaveToTrackingByShimentID(new string[] { _newshipmentid });
                        TrackingEDI.Manager.SaveStatus(new TrackingEDI.Model.Status() { ShipmentId = _newshipmentid, StsCd = "015", Sender = userid, Location = polcd, LocationName = "", StsDescp = "Release BL" });
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            return string.Empty;
        }

        private static void CleareCombineInfo(MixedList ml, string array)
        {
            EditInstruct sei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            sei.PutKey("SHIPMENT_ID", array);
            sei.Put("SHIPMENT_INFO", "");
            sei.Put("COMBIN_SHIPMENT", "");
            sei.Put("ISCOMBINE_BL", "");
            sei.Put("C_MASTER_NO", "");
            sei.Put("C_HOUSE_NO", "");
            ml.Add(sei);
            sei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
            sei.PutKey("SHIPMENT_ID", array);
            sei.Put("COMBIN_SHIPMENT", "");
            sei.Put("ISCOMBINE_BL", "");
            sei.PutExpress("MASTER_NO", "C_MASTER_NO");
            sei.PutExpress("HOUSE_NO", "C_HOUSE_NO");
            ml.Add(sei);
        }

        private static void DeleteShipmentInfo(string shipmentid, MixedList ml)
        {
            EditInstruct cei = new EditInstruct("SMSM", EditInstruct.DELETE_OPERATION);
            cei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(cei);
            EditInstruct ptei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
            ptei.PutKey("SHIPMENT_ID", shipmentid);
            ml.Add(ptei);
        }

        private static ResultInfo CheckIsStatus(string[] shipmentcombines)
        {
            string sql = string.Format(@"SELECT SHIPMENT_ID, STATUS,PPOL_CD,PPOD_CD,(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CS')CONSIGNEE,
                (SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FC')CUSTOMER FROM SMSM WHERE SHIPMENT_ID IN {0} ", SQLUtils.Quoted(shipmentcombines));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string pol_cd = string.Empty;
            string pod_cd = string.Empty;
            string customer = string.Empty;
            string consignee = string.Empty;

            List<string> a_statuslist = new List<string>();
            List<string> i_statuslist = new List<string>();
             Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr =dt.Rows[i];
                string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                switch (status)
                {
                    case "A":
                        onAdd(a_statuslist, Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        break;
                    case "B":
                        onAdd(i_statuslist, Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        break;
                    case "C":
                        onAdd(i_statuslist, Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        break;
                    case "D":
                        onAdd(i_statuslist, Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        break;
                    case "I":
                        onAdd(i_statuslist, Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        break;
                    default:
                        return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = @Resources.Locale.L_SMHandle_Business_102 + Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]) + @Resources.Locale.L_SMHandle_Business_103 };
                }
                if (i == 0)
                {
                    pol_cd = Prolink.Math.GetValueAsString(dr["PPOL_CD"]);
                    pod_cd = Prolink.Math.GetValueAsString(dr["PPOD_CD"]);
                    customer = Prolink.Math.GetValueAsString(dr["CUSTOMER"]);
                    consignee = Prolink.Math.GetValueAsString(dr["CONSIGNEE"]);
                }
                if (!pol_cd.Equals(Prolink.Math.GetValueAsString(dr["PPOL_CD"])))
                {
                    return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = @Resources.Locale.L_SMHandle_Business_104 +"POL，"+@Resources.Locale.L_SMHandle_Business_105 };
                }
                if (!pod_cd.Equals(Prolink.Math.GetValueAsString(dr["PPOD_CD"])))
                {
                    return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = @Resources.Locale.L_SMHandle_Business_104 +"POD，"+@Resources.Locale.L_SMHandle_Business_105 };
                }
                if (!customer.Equals(Prolink.Math.GetValueAsString(dr["CUSTOMER"])))
                {
                     return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = @Resources.Locale.L_SMHandle_Business_104 +@Resources.Locale.L_SMHandle_Business_106 };
                }
                if (!consignee.Equals(Prolink.Math.GetValueAsString(dr["CONSIGNEE"])))
                {
                    return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = @Resources.Locale.L_SMHandle_Business_104 +"Consignee，"+@Resources.Locale.L_SMHandle_Business_105 };
                }
            }
            if (i_statuslist.Count > 1){
                string description = string.Format(@Resources.Locale.L_SMHandle_Business_107, string.Join(Environment.NewLine, i_statuslist));
                return new ResultInfo { IsSucceed = false, ResultCode = "failed", Description = description };
            }
            if (i_statuslist.Count == 1)
            {
                return new ResultInfo { IsSucceed = true, ResultCode = "Succeed", Description = i_statuslist[0].ToString() };
            }
            return new ResultInfo { IsSucceed = true, ResultCode = "Succeed", Description = "" };
        }

        private static string CheckIsCombineSM(ref string[] shipmentcombines)   //检查合并Shipment功能
        {
            List<string> shipmentids =new List<string>(shipmentcombines);
            string sql = string.Format(@"SELECT SHIPMENT_ID,STATUS,SHIPMENT_INFO,ISCOMBINE_BL,(SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE IN('BO','SP'))FORWARDER,
        (SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='SH')SHIPPER,
        (SELECT TOP 1 PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='CS')CONSINGEE,CARRIER
        FROM SMSM WHERE SHIPMENT_ID IN {0} ", SQLUtils.Quoted(shipmentcombines));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string forwarder = string.Empty;
            string shipper = string.Empty;
            string consingee = string.Empty;
            string carrier=string.Empty;
            string shipmentinfo = string.Empty;
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                //if (!"A".Equals(Prolink.Math.GetValueAsString(dr["STATUS"])))
                //    return " 该笔shipmentID：" + Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]) + "还未对外订舱，不允许合并Shipment！";
                shipmentinfo = Prolink.Math.GetValueAsString(dr["SHIPMENT_INFO"]);
                if (!string.IsNullOrEmpty(shipmentinfo))
                {
                    string[] shiparray = shipmentinfo.Split(',');
                    foreach (string si in shiparray)
                    {
                        onAdd(shipmentids, si);
                    }
                }
                if ("Y".Equals(Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"])) || "C".Equals(Prolink.Math.GetValueAsString(dr["ISCOMBINE_BL"])))
                {
                    shipmentids.Remove(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                }
                if (i == 0)
                {
                    forwarder = Prolink.Math.GetValueAsString(dr["FORWARDER"]);
                    shipper = Prolink.Math.GetValueAsString(dr["SHIPPER"]);
                    consingee = Prolink.Math.GetValueAsString(dr["CONSINGEE"]);
                    carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
                }
                if (!carrier.Equals(Prolink.Math.GetValueAsString(dr["CARRIER"])))
                {
                    return @Resources.Locale.L_SMHandle_Business_104+ @Resources.Locale.L_SMHandle_Business_108;
                }
                if (!forwarder.Equals(Prolink.Math.GetValueAsString(dr["FORWARDER"])))
                {
                    return @Resources.Locale.L_SMHandle_Business_104+ @Resources.Locale.L_SMHandle_Business_109;
                }
                if (!shipper.Equals(Prolink.Math.GetValueAsString(dr["SHIPPER"])))
                {
                    return @Resources.Locale.L_SMHandle_Business_104+" Shipper，"+@Resources.Locale.L_SMHandle_Business_110;
                }
                if (!consingee.Equals(Prolink.Math.GetValueAsString(dr["CONSINGEE"])))
                {
                    return @Resources.Locale.L_SMHandle_Business_104+" Consignee，"+@Resources.Locale.L_SMHandle_Business_110;
                }
            }
            shipmentcombines = shipmentids.ToArray();
            return "";
        }

        private static void ChangeDNInfo(MixedList ml, string[] shipmentcombines,string newshipmentid)
        {
            string combindnno = string.Empty;
            string sql = string.Format("SELECT DN_NO,COMBINE_INFO,DEP FROM SMDN WHERE SHIPMENT_ID IN {0}", SQLUtils.Quoted(shipmentcombines));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> dnlist = new List<string>();
            Action<List<string>, string> onAdd = (items, txt) =>
            {
                if (string.IsNullOrEmpty(txt) || items.Contains(txt)) return;
                items.Add(txt);
            };
            foreach (DataRow dr in dt.Rows)
            {
                string dnno = dr["DN_NO"].ToString();
                onAdd(dnlist, dnno);
                string combine = dr["COMBINE_INFO"].ToString();
                string[] combines = combine.Split(',');
                foreach (string dn in combines)
                {
                    onAdd(dnlist, dn);
                }
            }
            if (dnlist.Count > 0)
                combindnno = string.Join(",", dnlist);

            combindnno=combindnno.Trim(',');
            foreach (string dnno in dnlist)
            {
                if (string.IsNullOrEmpty(dnno)) continue;
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("DN_NO", dnno);
                ei.Put("COMBINE_INFO", combindnno);
                ei.Put("SHIPMENT_ID", newshipmentid);
                ml.Add(ei);
            }

            EditInstruct smrvei = new EditInstruct("SMRV", EditInstruct.UPDATE_OPERATION);
            smrvei.PutKey("SHIPMENT_ID", newshipmentid);
            smrvei.Put("DN_NO", combindnno);
            ml.Add(smrvei);

            TrackingEDI.Business.CaCuLateManager cm = new TrackingEDI.Business.CaCuLateManager();
            TrackingEDI.Business.TotalQTyCaCuLate tqc = new TrackingEDI.Business.TotalQTyCaCuLate();
            tqc = cm.CaCulateCombinBySMArray(shipmentcombines);
            EditInstruct smei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            smei.PutKey("SHIPMENT_ID", newshipmentid);
            cm.CaCulatePutEi(ref smei, tqc,newshipmentid,ml);
            smei.Put("COMBINE_INFO", combindnno);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    var dep = dt.Select("DEP='SS'").CopyToDataTable();
            //    if (dep != null && dep.Rows.Count > 0)
            //    {
            //        smei.Put("DN_NO", dep.Rows[0]["DN_NO"]);
            //    }
            //}
            
            ml.Add(smei);
        }

        private static void RemoveShipment(MixedList ml, string[] shipmentcombines,string newshipmentid)
        {
            foreach (string shipemntid in shipmentcombines)
            {
                if (newshipmentid.Equals(shipemntid)) continue;
                EditInstruct ei = new EditInstruct("SMSM", EditInstruct.DELETE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipemntid);
                ml.Add(ei);
                EditInstruct smsmptei = new EditInstruct("SMSMPT", EditInstruct.DELETE_OPERATION);
                smsmptei.PutKey("SHIPMENT_ID", shipemntid);
                ml.Add(smsmptei);
            }
        }

        public static bool CheckEDoc(string shipmentid,string partyno,string partytype,string stn,string dep,bool containsdn=false)
        {
            string shipmentuid = string.Empty;
            string trantype = string.Empty;
            string groupId = string.Empty;
            string cmp= string.Empty;
            string combineinfo = string.Empty;
            DataTable dt = GetSMByShipmentId(shipmentid);
            if(dt.Rows.Count<=0)
            {
                return false;
            }
            shipmentuid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
            trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            groupId = Prolink.Math.GetValueAsString(dt.Rows[0]["GROUP_ID"]);
            cmp = Prolink.Math.GetValueAsString(dt.Rows[0]["CMP"]);
            combineinfo = Prolink.Math.GetValueAsString(dt.Rows[0]["COMBINE_INFO"]);

            EdocHelper edochelper = new EdocHelper();
            string type=edochelper.GetEdocSetTypes(cmp, partytype, trantype, partyno);
            if (string.IsNullOrEmpty(type))
                return true;

            string []types = type.Split(';');
            int typescount = types.Length;
            string[] dns=combineinfo.Split(',');
            string sql=string.Format("SELECT U_ID,GROUP_ID,CMP,STN,DEP FROM SMDN WHERE DN_NO IN {0}",SQLUtils.Quoted(dns));
            DataTable dndt=OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int serverNum = 0;
            List<EDOCFileItem> edocList = MailTemplate.GetEdoList(shipmentuid, groupId, cmp, "*", "", type, ref serverNum);
            if (containsdn) //如果设定为需要包含smdn的子档数据，则抓取如下
            {
                foreach (DataRow dnrow in dndt.Rows)
                {
                    string dnuid = dnrow["U_ID"].ToString();
                    string dngroupid = dnrow["GROUP_ID"].ToString();
                    string dncmp = dnrow["CMP"].ToString();
                    List<EDOCFileItem> dnedocList = MailTemplate.GetEdoList(dnuid, dngroupid, dncmp, "*", "", type,ref serverNum);
                    foreach (EDOCFileItem edocfiletime in dnedocList)
                    {
                        edocList.Add(edocfiletime);
                    }
                }
            }

            List<string> geteodctypes=new List<string>();
            foreach(EDOCFileItem edocfiletime in edocList){
                string edoctype=edocfiletime.EdocType;
                if(geteodctypes.Contains(edoctype))continue;
                geteodctypes.Add(edoctype);
            }
            if (geteodctypes.Count == typescount)
            {
                return true;
            }
            return false;
        }

        public static DataTable GetSMByUid(string uid)
        {
            string sql = "SELECT * FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static DataTable GetSMByShipmentId(string shipmentid)
        {
            string sql = "SELECT * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static DataTable GetPTBySMUid(string uid)
        {
            string sql = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID=(SELECT SHIPMENT_ID FROM SMSM WHERE U_ID={0})", SQLUtils.QuotedStr(uid));
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return maindt;
        }

        public static void ToEi(DataTable dt, string tableName, MixedList ml, string shipment_id, string uid, Dictionary<string, object> parm)
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
                            if (tableName.Equals("SMSM"))
                            {
                                ei.Put("U_ID", uid);
                            }
                            else
                            {
                                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            }
                            continue;
                        case "SHIPMENT_ID":
                            ei.Put("SHIPMENT_ID", shipment_id);
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

                    if (parm != null)
                    {
                        if (parm.ContainsKey(name))
                        {
                            ei.Put(name, parm[name]);
                        }
                    }
                }
                ml.Add(ei);
            }
        }

        public static string QAHoldBlMessage(string shipmentid,string expreason="BNS",bool isbat=false)
        {
            Result result = new Result();
            if(isbat)
                result = QAHoldShipment(shipmentid, expreason,isbat);
            else
                result = QAHoldShipment(shipmentid, expreason);
            if (result.Success)
            {
                return @Resources.Locale.L_BookingAction_Controllers_132 + @Resources.Locale.L_SMSMI_btn08 + "：" + result.Message;
            }
            return "";
        }

        public static Result QAHoldShipment(string jobno,string expreason,bool isbat=false)
        {
            Result result = new Result() { };
            if (isbat)
            {
                string sql = string.Format("SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMRV WHERE BAT_NO={0})", SQLUtils.QuotedStr(jobno));
                DataTable dt1=OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt1.Rows.Count > 0)
                {
                    jobno = string.Empty;
                    foreach (DataRow dr in dt1.Rows)
                    {
                        jobno += dr["DN_NO"].ToString()+";";
                    }
                    jobno=jobno.Trim(';');
                }
            }
            string[] jobnos = jobno.Split(';');
            DataTable dt = GetTmexpDt(jobnos, expreason);
            DataRow[] drs = dt.Select("CANCEL_BY IS NULL");
            if (drs.Length <= 0) return result;
            result.Success = true;
            result.Message = drs[0]["JOB_NO"].ToString() + " " + drs[0]["EXP_TEXT"].ToString();
            return result;
        }

        public static DataTable GetTmexpDt(string[] jobnos,string expreason)
        {
            //QAH QAhold货物  和 BNS 备货异常
            string sql = string.Format(@"SELECT * FROM TMEXP WHERE JOB_NO IN {0} AND EXP_REASON ={1}
            UNION SELECT * FROM TMEXP WHERE JOB_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID IN {0}) AND EXP_REASON ={1}",
            SQLUtils.Quoted(jobnos), SQLUtils.QuotedStr(expreason));
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void SetPartyToSm(string shipmentid)
        {
            EditInstruct ei=new EditInstruct("SMSM",EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID",shipmentid);
            Dictionary<string, object> parm = new Dictionary<string, object>();
            string sql = string.Format(@"SELECT U_ID,U_FID,SHIPMENT_ID,PARTY_TYPE,TYPE_DESCP
      ,ORDER_BY,PARTY_NO,PARTY_NAME,PART_ADDR1 AS PART_ADDR,PART_ADDR2,PART_ADDR3,CNTY,CNTY_NM
      ,CITY,CITY_NM,STATE,ZIP,PARTY_ATTN,PARTY_TEL,PARTY_MAIL
      ,DEBIT_TO,PART_ADDR4,PART_ADDR5,FAX_NO,PARTY_NAME2,PARTY_NAME3,PARTY_NAME4,TAX_NO FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN {1}", Prolink.Data.SQLUtils.QuotedStr(shipmentid), Prolink.Data.SQLUtils.Quoted(TransferBooking.PARTY_TYPES));
            DataTable partyDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            TransferBooking.SetPartyDatas(parm,partyDt,TransferBooking.SET_TYPES);
            TransferBooking.BatchSetEI( ei, TransferBooking.SET_TYPES, parm);

            MixedList ml = new MixedList();
            ml.Add(ei);
            if (ml.Count > 0)
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
        }

        public static string ChangePodAction(string uid, string changereason, UserInfo userinfo)
        {
            string partyno = string.Empty;
            string partytype = string.Empty;
            string returnMsg = string.Empty;
            string mail_to = string.Empty;
            string groupId = string.Empty;
            string cmp = string.Empty;
            string stn = string.Empty;
            string dep = string.Empty;
            string tran_type = string.Empty;
            DataTable maindt = GetSMByUid(uid);

            string iscombine_bl = string.Empty;
            string shipmentid = null;
            string instruction = string.Empty;
            if (maindt.Rows.Count > 0)
            {
                shipmentid = maindt.Rows[0]["SHIPMENT_ID"].ToString();
                uid = maindt.Rows[0]["U_ID"].ToString();
                groupId = maindt.Rows[0]["GROUP_ID"].ToString();
                cmp = maindt.Rows[0]["CMP"].ToString();
                tran_type = maindt.Rows[0]["TRAN_TYPE"].ToString();
                iscombine_bl = maindt.Rows[0]["ISCOMBINE_BL"].ToString();
                instruction = maindt.Rows[0]["INSTRUCTION"].ToString();
            }

            EdocHelper edochelper = new EdocHelper();
            partytype = edochelper.GetTypeByTranType(tran_type);
            string[] partytypes = partytype.Split(';');

            string sql1 = string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN {1} ", SQLUtils.QuotedStr(shipmentid), SQLUtils.Quoted(partytypes));
            DataTable maildt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (maildt.Rows.Count <= 0) return string.Format(@Resources.Locale.L_BookingActionController_Controllers_85, shipmentid);
            partyno = maildt.Rows[0]["PARTY_NO"].ToString();

            MixedList mlist = new MixedList();
            //记录异常
            TmexpHandler th1 = new TmexpHandler();
            TmexpInfo tpi1 = new TmexpInfo();
            tpi1.UId = Guid.NewGuid().ToString();
            tpi1.UFid = Guid.NewGuid().ToString();
            tpi1.WrId =userinfo.UserId;
            tpi1.WrDate = DateTime.Now;
            tpi1.Cmp = userinfo.CompanyId;
            tpi1.GroupId = userinfo.GroupId;
            tpi1.JobNo = shipmentid;
            tpi1.ExpType = "SM";
            tpi1.ExpReason = "COD";
            tpi1.ExpText = @Resources.Locale.L_BSTSetup_Book + @Resources.Locale.TLB_ChangePod + userinfo.UserId + @Resources.Locale.TLB_ChangePod +
                @Resources.Locale.L_BillApproveHelper_Business_30 + changereason;
            tpi1.ExpObj = userinfo.UserId;
            mlist.Add(th1.SetTmexpEi(tpi1));

            //写入订舱说明，并增加日期
            List<string> list = new List<string>()
            { "CARRIER", "CARRIER_NM","HOUSE_NO", "MASTER_NO","DEST_CD","DEST_NAME", "ETA", "ETD", "VOYAGE1", "VOYAGE2", "VOYAGE3", "VOYAGE4", "POD_CD", "POD_NAME",
               "POL_CD", "POL_NAME", "POR_CD", "POR_NAME", "DEST_CD", "DEST_NAME", "PORT_CD", "PORT_NM", 
                "ETD1", "ETD2", "ETD3", "ETD4", "ETA1", "ETA2","ETA3","ETA4","VESSEL1","VESSEL2","VESSEL3","VOYAGE4","SIGN_BACK","VESSEL4",
                "ATD","ATP"
            };
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", shipmentid);
            list.ForEach(column => ei.Put(column, null));
            ei.PutExpress("CNT20", "PCNT20");
            ei.PutExpress("CNT40", "PCNT40");
            ei.PutExpress("CNT40HQ", "PCNT40HQ");
            ei.PutExpress("CNT_TYPE", "PCNT_TYPE");
            ei.PutExpress("CNT_NUMBER", "PCNT_NUMBER");
            if (string.IsNullOrEmpty(instruction))
            {
                ei.PutExpress("INSTRUCTION", "convert(nvarchar(50),getdate(), 100) + ' Change POD Reason:' + '" + changereason + "'");
            }
            else
            {
                ei.PutExpress("INSTRUCTION", "'"+instruction+"'"+"+ convert(nvarchar(50),getdate(), 100) + ' Change POD Reason:' + '" + changereason + "'");
            }
            ei.PutExpress("POL_CD", "PPOL_CD");
            ei.PutExpress("POR_CD", "PPOR_CD");
            ei.PutExpress("POD_CD", "PPOD_CD");
            ei.PutExpress("DEST_CD", "PDEST_CD");
            ei.PutExpress("POL_NAME", "PPOL_NAME");
            ei.PutExpress("POR_NAME", "PPOR_NAME");
            ei.PutExpress("POD_NAME", "PPOD_NAME");
            ei.PutExpress("DEST_NAME", "PDEST_NAME");
            ei.PutExpress("CBM", "PCBM");
            ei.PutExpress("GW", "PGW");
            ei.Put("SCAC_CD", "");
            ei.Put("CORDER", "P");
            mlist.Add(ei);

            //SendMail To BO/SP
            DataTable mailGroupDt = MailTemplate.GetMailGroup(partyno, userinfo.GroupId, tran_type);
            if (mailGroupDt.Rows.Count <= 0)
            {
                returnMsg = @Resources.Locale.L_BookingActionController_Controllers_47 + partyno + @Resources.Locale.L_BookingActionController_Controllers_48;
                return returnMsg;
            }
            foreach (DataRow mailGroup in mailGroupDt.Rows)
            {
                string mailindex = Prolink.Math.GetValueAsString(mailGroup["MAIL_ID"]);
                if (!string.IsNullOrEmpty(mailindex))
                {
                    mail_to += mailindex + ";";
                }
                //Mail 模板
            }
            EvenFactory.AddEven(MailManager.CHANGE_POD + "#" + uid + "#" + partyno + "#" + userinfo.GroupId + "#" + userinfo.CompanyId + "#" + partytype + "#" + Guid.NewGuid().ToString(), uid, "MM", null, 1, 0, mail_to, @Resources.Locale.TLB_ChangePod + shipmentid, "");
            
            //自动计价
            EditInstruct AutoValuationTaskEi = new EditInstruct("AUTO_VALUATION_TASK", EditInstruct.INSERT_OPERATION);
            AutoValuationTaskEi.Put("U_ID", System.Guid.NewGuid().ToString());
            AutoValuationTaskEi.Put("SMU_ID", uid);
            AutoValuationTaskEi.Put("DONE", "N");
            AutoValuationTaskEi.Put("CREATE_BY", userinfo.UserId);
            AutoValuationTaskEi.PutDate("CREATE_DATE", DateTime.Now);
            mlist.Add(AutoValuationTaskEi);

            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return @Resources.Locale.L_BookingActionController_Controllers_90;
                }
            }
            return string.Empty;
        }
    }
}