using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;

namespace Business.TPV.Financial
{
    public class InboundBill
    {
        /// <summary>
        /// 是否只计算第一条记录
        /// </summary>
        bool _topOne = true;

        Dictionary<string, string> OtherCntTypeMapping;
        Dictionary<string, string> _cntParm = new Dictionary<string, string> { { "20GP", "F4" }, { "40GP", "F5" }, { "40HQ", "F6" } };
        Dictionary<string, string> _cntParmt = new Dictionary<string, string> { { "20GP", "F1" }, { "40GP", "F2" }, { "40HQ", "F2" } };
        Dictionary<string, string> _cntParm1 = new Dictionary<string, string> { { "20GP", "GP20" }, { "40GP", "GP40" }, { "40HQ", "HQ40" } };
        List<string> _cntParmList = new List<string>();

        /// <summary>
        /// 代表预提成功个数
        /// </summary>
        int _qcount = 0;
        public InboundBill()
        {
            OtherCntTypeMapping = new Dictionary<string, string>();
            string sql = "SELECT distinct CD, CD_DESCP,CD_TYPE,AP_CD FROM BSCODE WHERE CD_TYPE IN ('RN_F','TDT') AND CMP = '*'";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow dr in dt.Rows)
            {
                string cnttype = Prolink.Math.GetValueAsString(dr["CD"]);
                string field = Prolink.Math.GetValueAsString(dr["CD_DESCP"]);
                string cdType = Prolink.Math.GetValueAsString(dr["CD_TYPE"]);
                string apCd = Prolink.Math.GetValueAsString(dr["AP_CD"]);
                if ("RN_F".Equals(cdType))
                {
                    OtherCntTypeMapping.Add(field, cnttype);
                    //RnfFields += "," + cnttype;
                    //_cntParm.Add(field, cnttype);
                    //_cntParmt.Add(field, cnttype);
                    //_cntParm1.Add(field, cnttype);
                    _cntParmList.Add(field);
                }
                else
                {
                    //if (!_truckType.ContainsKey(cnttype))
                    //    _truckType.Add(cnttype, apCd);
                    //if (!_truckDesc.ContainsKey(apCd))
                    //    _truckDesc.Add(apCd, field);
                }
            }
        }

        public InboundBill(bool topOne)
        {
            this._topOne = topOne;
        }
        /// <summary>
        /// 消息提示
        /// </summary>
        List<string> _messenger = new List<string>();
        /// <summary>
        /// 当前订舱数据源
        /// </summary>
        DataRow _current_smsm = null;
        /// <summary>
        /// 当前账单号
        /// </summary>
        string _current_debitno = string.Empty;

        DataTable _chgDt = null;

        DataTable _smbimDt = null;
        private static string _shipment_id = string.Empty;
        string _boundType = "I";
        string _table = "SMSMI";
        string _not_status = "'V','Z','A'";
        /// <summary>
        /// 本币类型
        /// </summary>
        string _localCur = "CNY";
        DataRow[] _partySH = null;
        DataTable _smbidDt = null;

        Dictionary<string, object> _qt_schems = new Dictionary<string, object>();

        private void Init()
        {
            _table = "SMSM";
            _not_status = "'V','Z','A'";
            if ("I".Equals(_boundType))
            {
                _table = "SMSMI";
                _not_status = "'X'";
            }
        }

        /// <summary>
        /// 获取账单日期
        /// </summary>
        /// <param name="billDate"></param>
        /// <param name="drs"></param>
        /// <returns></returns>
        public static DateTime GetBillDate(DateTime billDate, DataRow dr, bool haveATP = true)
        {
            string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            if (!"T".Equals(tranType) && dr["ETA"] != null && dr["ETA"] != DBNull.Value)
            {
                billDate = (DateTime)dr["ETA"];
            }
            else
            {   
                if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)//其他用ATD
                    billDate = (DateTime)dr["ATD"];
                else if (dr["ETD"] != null && dr["ETD"] != DBNull.Value)
                    billDate = (DateTime)dr["ETD"];
                else if (dr["DN_ETD"] != null && dr["DN_ETD"] != DBNull.Value)
                    billDate = (DateTime)dr["DN_ETD"]; 
            }
            return billDate;

        }


        static Dictionary<string, List<string>> _cost_mapping = null;
        /// <summary>
        /// Local Charge
        /// </summary>
        const string LOCAL_CHARGE = "LocalCharge";

        /// <summary>
        /// 目的地费用
        /// </summary>
        const string DESTINATION_CHARGE = "DestinationCharge";

        /// <summary>
        /// 运费
        /// </summary>
        const string FREIGHT = "Freight";

        /// <summary>
        /// 旺季附加费
        /// </summary>
        const string ESS = "ESS";

        /// <summary>
        /// 超重
        /// </summary>
        const string OWC = "OWC";

        /// <summary>
        /// 超规格
        /// </summary>
        const string OSC = "OSC";

        /// <summary>
        /// 燃油附加费
        /// </summary>
        const string FSC = "FSC";

        /// <summary>
        /// 抓取需要的付款对象和规则
        /// </summary>
        /// <param name="tran_mode"></param>
        /// <param name="term"></param>
        /// <param name="freightTerm"></param>
        /// <param name="loadingTo"></param>
        /// <returns></returns>
        public static List<string> GetDebitTo(string tran_mode, string term, string freightTerm, string bandType, string TrackWay, string iscombine_bl, string bondType = "O")
        {
            List<string> types = new List<string>();
            if ("I".Equals(bondType))
            {

                switch (tran_mode)
                {
                    case "F":
                    case "L":
                    case "A":
                    case "E":
                    case "R":
                    case "T":
                        switch (term)
                        {
                            case "EXW":
                            case "FCA":
                            case "FAS":
                            case "FOB":
                                if ("C".Equals(freightTerm))
                                { 
                                    types.Add("CarrierI");
                                    types.Add(FREIGHT);
                                }
                                break;
                        }
                        break;
                        
                }
                switch (term)
                {
                    case "CIP":
                    case "DAP":
                    case "DDP":
                        break;
                    default:
                        types.Add(DESTINATION_CHARGE);
                        break;
                }


                if ("S".Equals(iscombine_bl))
                {
                    if (types.Contains("CarrierI")) types.Remove("CarrierI");
                }
                return types;
            }
            //if ("EXW".Equals(term))
            //    return types;
            ////判断是否有运费
            //switch (freightTerm)
            //{
            //    case "P":
            //        types.Add("Carrier");
            //        types.Add(LOCAL_CHARGE);
            //        types.Add(FREIGHT);
            //        break;
            //    default:
            //        //2.	Railway/LCL/AIR  Freight term =P 的會抓  DLV_TERM=CIF 的報價.  Freight term 不為P , 且DLV term 不為EXW 的都會抓  報價單的DLV_term =FOB 的報價 . 
            //        if (("R".Equals(tran_mode) || "L".Equals(tran_mode) || "A".Equals(tran_mode)) && !"EXW".Equals(term))
            //        {
            //            types.Add("Carrier");
            //            types.Add(LOCAL_CHARGE);
            //        }
            //        else if ("F".Equals(tran_mode))
            //        {
            //            types.Add("Carrier");
            //            types.Add(LOCAL_CHARGE);
            //        }
            //        else if (("T".Equals(tran_mode) && ("A".Equals(TrackWay) || "S".Equals(TrackWay))) || "D".Equals(TrackWay))
            //        {
            //            //內貿運輸(包含國內快遞/國內空運)
            //            types.Add("Carrier");
            //            types.Add(FREIGHT);
            //        }
            //        break;
            //}

            //switch (tran_mode)
            //{
            //    case "D":
            //        break;
            //    case "T":
            //        if ("Y".Equals(bandType))//2.	繞物流園區的才要算報關費.BandType
            //        {
            //            types.Add("BR");
            //            types.Add("BC");
            //            types.Add("BM");
            //        }
            //        //types.Add("CR");
            //        break;
            //    case "E":// 非統報的國際快遞單,要用 BR 
            //        //if (!"Y".Equals(cent_decl.ToUpper()))
            //        //    types.Add("BR");
            //        break;
            //    default:
            //        types.Add("BR");
            //        types.Add("CR");
            //        break;
            //}

            //switch (term)
            //{
            //    case "CIP":
            //    case "DAP":
            //    case "DDP":
            //        types.Add(DESTINATION_CHARGE);
            //        break;
            //    case "EXW":
            //        if ("F".Equals(tran_mode))//FCL一定会有这两个费用
            //            break;
            //        if (types.Contains("BR")) types.Remove("BR");
            //        if (types.Contains("CR")) types.Remove("CR");
            //        break;
            //}

            //if ("Y".Equals(iscombine_bl) || "C".Equals(iscombine_bl))
            //{
            //    if (types.Contains("BR")) types.Remove("BR");
            //    if (types.Contains("CR")) types.Remove("CR");
            //}
            //else if ("S".Equals(iscombine_bl))
            //{
            //    if (types.Contains("Carrier")) types.Remove("Carrier");
            //}

            return types;
        }

        public void Share(string shipment_uid, string boundType = "I")
        {
            try
            {
                //List<string> ChgType = CheckBsTerm(sid);
                //if (ChgType == null)
                //{

                //}
                //    emptyMessage.Add(sid + ":cann't find term vs charge");


                DoShare(shipment_uid, boundType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void DoShare(string shipment_id, string boundType)
        {
            _boundType = boundType;
            Init();
            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count <= 0) return;
            DataRow smsm = smDt.Rows[0];
            string cmp = Prolink.Math.GetValueAsString(smsm["CMP"]);
            string shipment_uid = Prolink.Math.GetValueAsString(smsm["U_ID"]);
            string group_id = Prolink.Math.GetValueAsString(smsm["GROUP_ID"]);
            string tranMode = Prolink.Math.GetValueAsString(smsm["TRAN_TYPE"]);
            string combine_info = Prolink.Math.GetValueAsString(smsm["COMBINE_INFO"]);
            string shipment_info = Prolink.Math.GetValueAsString(smsm["SHIPMENT_INFO"]);

            _localCur = Business.TPV.Standard.BillingManager.GetLocalCur(group_id, cmp);


            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);

            DataTable delSmDt = null;
            string delete_sql = string.Empty;
            foreach (string dnNo in dns)
            {
                if (delete_sql.Length > 0) delete_sql += " OR ";
                delete_sql += string.Format("COMBINE_INFO LIKE {0}", SQLUtils.QuotedStr("%" + dnNo + "%"));
            }
            if (string.IsNullOrEmpty(delete_sql))
                delete_sql = "1=0";
            delete_sql = "SELECT SHIPMENT_ID,CMP FROM " + _table + " WITH(NOLOCK) WHERE " + delete_sql;

            //if(dns.Length<=0)return;
            string sql = string.Empty;
            foreach (string dnNo in dns)
            {
                if (sql.Length > 0) sql += " OR ";
                if (sql.Length == 0) sql += "SELECT * FROM (SELECT * FROM " + _table + " WITH(NOLOCK) WHERE 1=1 AND STATUS NOT IN (" + _not_status + "))T WHERE ";
                sql += string.Format(" COMBINE_INFO LIKE {0}", SQLUtils.QuotedStr("%" + dnNo + "%"));
            }

            List<string> dnList = new List<string>();
            if (dns.Length > 0)
            {
                smDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                delSmDt = OperationUtils.GetDataTable(delete_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                foreach (DataRow dr in smDt.Rows)
                {
                    string combine_info1 = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
                    string[] dns1 = combine_info1.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string temp in dns1)
                    {
                        if (dnList.Contains(temp)) continue;
                        dnList.Add(temp);
                    }
                }
                if (dnList.Count > 0)
                    dns = dnList.ToArray();
            }


            MixedList ml = new MixedList();
            #region 删除多余的数据
            if (delSmDt != null)
            {
                List<string> smList1 = new List<string>();
                foreach (DataRow dr in delSmDt.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    string sm_cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                    if (!smList1.Contains(sm_id))
                        smList1.Add(sm_id);
                    ml.Add(string.Format("DELETE FROM SMBID WHERE U_FID IS NULL AND (BAMT=0 OR BAMT IS NULL) AND SHIPMENT_ID = {0} AND CMP={1}  AND CHG_CD='DF' ", SQLUtils.QuotedStr(sm_id), SQLUtils.QuotedStr(sm_cmp)));
                }
                if (smList1.Count > 0)
                {
                    ml.Add(string.Format("DELETE FROM SMBID_TEMP WHERE SHIPMENT_ID IN {0}", Helper.JoinString(smList1.ToArray())));
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            #endregion

            List<string> smList = new List<string>();
            List<string> checkList = new List<string>();
            foreach (DataRow dr in smDt.Rows)
            {
                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (checkList.Contains(uid)) continue;
                checkList.Add(uid);
                string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (!smList.Contains(sm_id))
                    smList.Add(sm_id);
                CreateByShipmentId(uid, TimeZoneHelper.GetTimeZoneDate(DateTime.Now, Prolink.Math.GetValueAsString(dr["CMP"])), ml, dr,boundType);
            }

            int[] result = null;
            if (ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smList.Count <= 0)
                return;

        }



        /// <summary>
        /// 根据shipment的UID创建账单及费用
        /// </summary>
        /// <param name="shipment_uid">SMSM.U_ID</param>
        public void CreateByShipmentId(string shipment_uid, DateTime billDate, MixedList ml = null, DataRow smsm = null, string boundType = "O")
        {
            _boundType = boundType;
            Init();

            _qcount = 0;
            _qt_schems.Clear();
            if (string.IsNullOrEmpty(shipment_uid))
                return;
            bool updateDataFlag = false;
            if (ml == null)
            {
                ml = new MixedList();
                updateDataFlag = true;
            }

            if (smsm == null)
            {
                DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE U_ID={0}", SQLUtils.QuotedStr(shipment_uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (smDt.Rows.Count <= 0) return;
                smsm = smDt.Rows[0];
            }
            string cmp = Prolink.Math.GetValueAsString(smsm["CMP"]);
            string group_id = Prolink.Math.GetValueAsString(smsm["GROUP_ID"]);
            string tranMode = Prolink.Math.GetValueAsString(smsm["TRAN_TYPE"]);

            //5.生效日用 ETD(空白用DN_ETD) 來判斷.
            billDate = GetBillDate(billDate, smsm);
            string shipment_id = Prolink.Math.GetValueAsString(smsm["SHIPMENT_ID"]);
            _shipment_id = shipment_id;
            WriteLogTagStart(string.Format("开始{0}计算,ETD:{1}", tranMode, billDate.ToString("yyyy-MM-dd")));

            string sql = string.Empty;
            #region 获取DN party SMSM  已开立账单
            string combine_info = Prolink.Math.GetValueAsString(smsm["COMBINE_INFO"]);
            string shipment_info = Prolink.Math.GetValueAsString(smsm["SHIPMENT_INFO"]);
            List<string> smList = Helper.SplitToList(shipment_info);
            //string partySql = "SELECT * FROM SMSMPT WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            string partySql = string.Format("SELECT * FROM (SELECT * FROM SMSMIPT WHERE SHIPMENT_ID={0}) A OUTER APPLY (SELECT TOP 1 HEAD_OFFICE,BILL_TO,(SELECT TOP 1 PARTY_NAME FROM SMPTY C WHERE C.PARTY_NO=SMPTY.HEAD_OFFICE) AS HEAD_NAME FROM SMPTY WHERE SMPTY.PARTY_NO=A.PARTY_NO) B", SQLUtils.QuotedStr(shipment_id));
            if (!smList.Contains(shipment_id))
                smList.Add(shipment_id);
            if (smList.Count > 1)
            {
                sql = "SELECT * FROM SMIDN WHERE SHIPMENT_ID IN " + Helper.JoinString(smList.ToArray());
            }
            else
            {
                sql = "SELECT * FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            }
            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (dns.Length > 0)
                sql += " UNION SELECT * FROM SMIDN WHERE DN_NO IN " + Helper.JoinString(dns);
            WriteLog("party SQL:" + partySql);
            DataTable partyDt = OperationUtils.GetDataTable(partySql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            _smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE SHIPMENT_ID={0} AND CMP={1} ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(cmp)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            _smbimDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE SHIPMENT_ID={0} AND CMP={1} AND VOID_FLAG IS NULL", SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(cmp)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

             #endregion

            #region 账单生成条件
            string cent_decl ="";//统报  Y/N
            //计费数量

            decimal dnNum = dns.Length;
            decimal Pcnt20 = Helper.GetValueAsDecimal(smsm, new string[] { "CNT20" });//20'
            decimal Pcnt40 = Helper.GetValueAsDecimal(smsm, new string[] { "CNT40" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT40"]);//40'
            decimal Pcnt40Hq = Helper.GetValueAsDecimal(smsm, new string[] { "CNT40HQ" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT40HQ"]);//40'HQ
            string PcntType = Helper.GetValueAsString(smsm, new string[] { "CNT_TYPE" });// Prolink.Math.GetValueAsString(smsm["PCNT_TYPE"]);//其他货柜
            decimal PcntNumber = Helper.GetValueAsDecimal(smsm, new string[] { "CNT_NUMBER" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT_NUMBER"]);//其他货柜数量

            string PocntType = Helper.GetValueAsString(smsm, new string[] { "OCNT_TYPE" });//其他货柜
            decimal PocntNumber = Helper.GetValueAsDecimal(smsm, new string[] { "OCNT_NUMBER" }); //其他货柜数量

            decimal PkgNum = Prolink.Math.GetValueAsDecimal(smsm["PKG_NUM"]);//件数
            string PkgUnit = Prolink.Math.GetValueAsString(smsm["PKG_UNIT"]);//件数单位
            decimal Gw = Helper.GetValueAsDecimal(smsm, new string[] { "GW", "PGW" }); //Prolink.Math.GetValueAsDecimal(smsm["PGW"]);//GW 毛重
            string Gwu = Prolink.Math.GetValueAsString(smsm["GWU"]);//毛重单位GWU
            decimal Cbm = Helper.GetValueAsDecimal(smsm, new string[] { "CBM", "PCBM" }); // Prolink.Math.GetValueAsDecimal(smsm["PCBM"]);//CBM体积
            string qtyu = Prolink.Math.GetValueAsString(smsm["QTYU"]);
            decimal qty = Prolink.Math.GetValueAsDecimal(smsm["QTY"]);

            //起运地 目的地
            string pol = Helper.GetValueAsString(smsm, new string[] { "POL_CD", "PPOL_CD" });// Prolink.Math.GetValueAsString(smsm["PPOL_CD"]);
            string pod = Helper.GetValueAsString(smsm, new string[] { "DEST_CD", "PDEST_CD" });//Prolink.Math.GetValueAsString(smsm["PDEST_CD"]);
            string smsmpod = Prolink.Math.GetValueAsString(smsm["POD_CD"]);

            //報單數  续单数
            decimal decl_num = Prolink.Math.GetValueAsDecimal(smsm["DECL_NUM"]);
            decimal next_num = Prolink.Math.GetValueAsDecimal(smsm["NEXT_NUM"]);//续单数

            //车型
            string CarType = Prolink.Math.GetValueAsString(smsm["CAR_TYPE"]);
            decimal CarQty = Prolink.Math.GetValueAsDecimal(smsm["CAR_QTY"]);
            string CarType1 = Prolink.Math.GetValueAsString(smsm["CAR_TYPE1"]);
            decimal CarQty1 = Prolink.Math.GetValueAsDecimal(smsm["CAR_QTY1"]);
            string CarType2 = Prolink.Math.GetValueAsString(smsm["CAR_TYPE2"]);
            decimal CarQty2 = Prolink.Math.GetValueAsDecimal(smsm["CAR_QTY2"]);
            string TrackWay = Prolink.Math.GetValueAsString(smsm["TRACK_WAY"]);
            string CargoType = Prolink.Math.GetValueAsString(smsm["CARGO_TYPE"]);
            string qtTranMode = tranMode;

            //报价抓取条件
            string IncotermCd = Prolink.Math.GetValueAsString(smsm["INCOTERM_CD"]);//贸易条款
            string FrtTerm = Prolink.Math.GetValueAsString(smsm["FRT_TERM"]);//Freight Term
            string LoadingFrom = Prolink.Math.GetValueAsString(smsm["LOADING_FROM"]);
            string LoadingTo = Prolink.Math.GetValueAsString(smsm["LOADING_TO"]);
            //FreightAmt 运费

            string Carrier = Prolink.Math.GetValueAsString(smsm["CARRIER"]);

            //报价条件
            string qt_condition = string.Format("QUOT_TYPE='F' AND POL_CD={0} AND POD_CD={1} AND TRAN_MODE={2}", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(qtTranMode));
            string region = string.Empty;
            //if ("E".Equals(qtTranMode))
            switch (qtTranMode)
            {
                case "E":
                    pod = Helper.GetValueAsString(smsm, new string[] { "POD_CD", "PPOD_CD" });
                    if (!string.IsNullOrEmpty(pod) && pod.Length >= 2)
                    {
                        region = pod.Substring(0, 2);
                        qt_condition = string.Format("QUOT_TYPE='F' AND POL_CD={0} AND REGION={1} AND TRAN_MODE={2}", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(region), SQLUtils.QuotedStr(qtTranMode));
                    }
                    break;
                case "T":
                    qt_condition = string.Format("QUOT_TYPE='F' AND POL_CD={0} AND POD_CD={1} AND (TRAN_TYPE={2} OR TRAN_TYPE IS NULL)  AND TRAN_MODE={3}", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(pod), SQLUtils.QuotedStr(TrackWay), SQLUtils.QuotedStr(qtTranMode));
                    break;
            }

          

            //询价条件
            string rq_condition = string.Format(" AND SMRQM.EFFECT_FROM<={0} AND SMRQM.EFFECT_TO>={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            string qtdate_condition = string.Format(" AND SMQTM.EFFECT_FROM<={0} AND SMQTM.EFFECT_TO>={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            if (!string.IsNullOrEmpty(qtTranMode))
                rq_condition += " AND SMRQM.TRAN_MODE=" + SQLUtils.QuotedStr(qtTranMode);


           
            string bu_condition = string.Format("SMFSC.EFFECT_DATE<={0} AND SMFSC.CARRIER={1}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(Carrier));
            #endregion

            #region 初始化参数
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["decl_num"] = decl_num;
            parm["next_num"] = next_num;
            parm["CONT_DECL_NUM"] = "";// Prolink.Math.GetValueAsDecimal(smsm["CONT_DECL_NUM"]);//续单数
            parm["FrtTerm"] = FrtTerm;
            parm["Cnt20"] = Pcnt20;
            parm["Cnt40"] = Pcnt40;
            parm["Cnt40hq"] = Pcnt40Hq;
            parm["OcntType"] = PocntType;
            parm["OcntNumber"] = PocntNumber;
            parm["CntNumber"] = PcntNumber;

            parm["CntType"] = PcntType;

            decimal cntNum = Pcnt20 + Pcnt40 + Pcnt40Hq + PocntNumber;
            if (!string.IsNullOrEmpty(PcntType))
            {
                if (!"LCL".Equals(PcntType.ToUpper()))
                {
                    cntNum += PcntNumber;
                }
            } 

            parm["CntNum"] = cntNum;
            parm["DnNum"] = dnNum;

            parm["qty"] = qty;
            parm["cw"] = Gw;
            parm["gw"] = Gw;
            parm["cbm"] = Cbm;

            parm["gwu"] = Gwu;
            parm["qtyu"] = qtyu;

            parm["carType"] = Helper.GetCarTypeField(CarType, cmp);
            parm["car_cw"] = CarQty;
            parm["carType1"] = Helper.GetCarTypeField(CarType1, cmp);
            parm["car_cw1"] = CarQty1;
            parm["carType2"] = Helper.GetCarTypeField(CarType2, cmp);
            parm["car_cw2"] = CarQty2;
            if ("PLT".Equals(PkgUnit) || "CTN".Equals(PkgUnit))
                parm["cnt"] = PkgNum;
            else
                parm["cnt"] = 0;
            parm["TranMode"] = tranMode;
            if ("A".Equals(tranMode))
            {
                Carrier = Prolink.Math.GetValueAsString(smsm["Vessel1"]);
                if (!string.IsNullOrEmpty(Carrier) && Carrier.Length > 2)
                    Carrier = Carrier.Substring(0, 2);
            }
            parm["carrier"] = Carrier;
            parm["TrackWay"] = TrackWay;
            parm["CargoType"] = CargoType;
            string bandType = "";// Prolink.Math.GetValueAsString(smsm["BAND_TYPE"]);
            parm["bandType"] = bandType;//绕物流园区
            parm["WMS"] = "";// Prolink.Math.GetValueAsString(smsm["EXTERNAL_WMS"]);//外仓
            parm["Gvalue"] = Prolink.Math.GetValueAsDecimal(smsm["GVALUE"]);//货值

            parm["nw"] = Prolink.Math.GetValueAsDecimal(smsm["CW"]);

            string combine_other = "";// Prolink.Math.GetValueAsString(smsm["COMBINE_OTHER"]);//外廠併櫃
            parm["cout"] = combine_other;
            parm["telex_rls"] = "";// Prolink.Math.GetValueAsString(smsm["TELEX_RLS"]);//电放
            parm["battery"] = Prolink.Math.GetValueAsString(smsm["BATTERY"]);//喇叭
            parm["horn"] = Prolink.Math.GetValueAsString(smsm["HORN"]);//離電池
            parm["region"] = Prolink.Math.GetValueAsString(smsm["REGION"]);//区域
            parm["customs_check"] = "";// Prolink.Math.GetValueAsString(smsm["CUSTOMS_CHECK"]);//是否查驗
            //parm["customs_check"] = "";//是否查驗
            parm["pod"] = pod;
            string bl_type = "";// Prolink.Math.GetValueAsString(smsm["BL_TYPE"]);
            string iscombine_bl = "";// Prolink.Math.GetValueAsString(smsm["ISCOMBINE_BL"]);

            parm["BrgType"] = smsm.Table.Columns.Contains("BRG_TYPE") ? Prolink.Math.GetValueAsString(smsm["BRG_TYPE"]) : string.Empty;//路桥方式
            parm["via"] = smsm.Table.Columns.Contains("VIA") ? Prolink.Math.GetValueAsString(smsm["VIA"]) : string.Empty;//via
            parm["is_land"] = "";// smsm.Table.Columns.Contains("IS_LAND") ? Prolink.Math.GetValueAsString(smsm["IS_LAND"]) : string.Empty;//是否陆运

            parm["CntrStdQty"] = 0;
            parm["StdQty"] = 0;
            parm["smsmpod"] = smsmpod;
            parm["billDate"] = billDate.ToString("yyyy-MM-dd");
            parm["ShipmentId"] = shipment_id;
            //获取标准装柜量

            DataTable dnDt = (dns.Length > 0) ?
               OperationUtils.GetDataTable(string.Format("SELECT GWU,DN_NO,GW,CBM FROM SMIDN WHERE DN_NO IN {0} ORDER BY DN_NO", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection())
               : OperationUtils.GetDataTable("SELECT GWU,DN_NO,GW,CBM FROM SMIDN WHERE 1=0 ORDER BY DN_NO", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            parm["DnQtyDt"] = dnDt;
            WriteLog("DN Table数:" + dnDt.Rows.Count);

            string dnNo = Prolink.Math.GetValueAsString(smsm["DN_NO"]);
            if ("T".Equals(tranMode) && "A".Equals(CargoType))
            {
                DataTable dnpDt = null;
                if (dns.Length > 0)
                    dnpDt = OperationUtils.GetDataTable(string.Format("SELECT DN_NO,IPART_NO,QTY,CNTR_STD_QTY FROM SMIDNP WHERE DN_NO IN {0} AND (NEW_CATEGORY<>'TANN' OR NEW_CATEGORY IS NULL) ORDER BY DN_NO", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                else if (string.IsNullOrEmpty(dnNo))
                    dnpDt = OperationUtils.GetDataTable(string.Format("SELECT '' AS DN_NO,IPART_NO,QTY,CNTR_STD_QTY FROM SMBDD WHERE SHIPMENT_ID={0} ORDER BY DN_NO", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (dnpDt != null)
                {
                    decimal cntrStdQty = 0;
                    decimal StdQty = 0;
                    foreach (DataRow dnp in dnpDt.Rows)
                    {
                        decimal val1 = Prolink.Math.GetValueAsDecimal(dnp["QTY"]);
                        decimal val2 = Prolink.Math.GetValueAsDecimal(dnp["CNTR_STD_QTY"]);
                        StdQty += val1;
                        if (val2 > 0)
                            cntrStdQty += val1 / val2;
                    }
                    parm["StdQty"] = StdQty;
                    parm["CntrStdQty"] = cntrStdQty;
                    parm["CntrStdDt"] = dnpDt;
                    WriteLog("SUM(数量/标准装柜量):" + cntrStdQty);
                    WriteLog("标准装柜量 Table数:" + dnpDt.Rows.Count);
                }
            }

            #endregion

            #region 获取费用建档
            if (_chgDt == null)
                _chgDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMCHG WHERE GROUP_ID={1} AND (CMP={2} OR CMP='*')", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 获取可能报价
            //sql = string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {1} AND RLOCATION={2}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUR AS M_CUR FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition, SQLUtils.QuotedStr(cmp));
            sql = string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT 1 FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND OUT_IN LIKE'%I%') AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUR AS M_CUR,PERIOD,CONSTRACT FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition);
            if ("L".Equals(tranMode))
                sql = string.Format("SELECT INCOTERM,QT_EFFECT_FROM,TRAN_MODE,RFQ_NO,QUOT_NO1 AS QUOT_NO,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1 AS LSP_NM,U_FID FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE OUT_IN LIKE'%I%' AND SMRQM.RFQ_NO=SMQTD.RFQ_NO {2})) A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,QUOT_NO AS QUOT_NO1,LSP_NM AS LSP_NM1 FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID {1}) B  GROUP BY INCOTERM,TRAN_MODE,RFQ_NO,QUOT_NO1,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1,U_FID,QT_EFFECT_FROM ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition.Replace("SMRQM.", "SMQTM."), rq_condition);
             
            WriteLog("报价SQL:" + sql);
            DataTable qtDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            if ("E".Equals(qtTranMode) && qtDt.Rows.Count <= 0)
            {
                if (pol.Length >= 2)
                {
                    sql = sql.Replace(string.Format("POL_CD={0}", SQLUtils.QuotedStr(pol)), string.Format("POL_CD={0}", SQLUtils.QuotedStr(pol.Substring(0, 2) + "ZZZ")));
                    qtDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                } 
            } 

            #endregion

            #region 抓取费率
            string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE", rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            WriteLog("费率SQL:" + string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE", rateFilter));
            #endregion


            string dest = Helper.GetValueAsString(smsm, new string[] { "POD_CD" }); 

            #region 抓取燃油附加费
            DataTable thcDt = null;
            #endregion

            #region 获取取账单对象
            Dictionary<string, string> ptList = new Dictionary<string, string> { { "SP_CR", "Freight_Forwarder" }, { "CR", "Trailer" }, { "BR", "Broker(BR)" }, { "BC", "Broker(BC)" }, { "BM", "Broker(BM)" }, { "EX", "Express(EX)" }, { "XP", "Express(XP)" }, { "DF", "DF" } };

            List<string> d_to = GetDebitTo(tranMode, IncotermCd, FrtTerm, bandType, TrackWay, iscombine_bl,boundType);
            WriteLog(string.Join("、", new string[] { "tranMode:" + tranMode, "IncotermCd:" + IncotermCd, "FrtTerm:" + FrtTerm, "LoadingTo:" + LoadingTo, "统报:" + cent_decl, "bl type:" + bl_type, "isCombine bl:" + iscombine_bl }));
            WriteLog("账单对象:" + string.Join("、", d_to.ToArray()));
            #endregion

            #region 运费计算

            List<string> msg = new List<string>();
            string sm_uid = Prolink.Math.GetValueAsString(smsm["U_ID"]);
            Dictionary<string, EditInstruct> mainEiMap = new Dictionary<string, EditInstruct>();
            _current_smsm = smsm;

            _partySH = Helper.GetParty(shipment_id, partyDt, new string[] { "CS" }); // Debit NO Inboudn 需要抓取Consignee
            foreach (var dtoType in d_to)
            {
                if ("O".Equals(_boundType) && "F".Equals(tranMode) && "EXW".Equals(IncotermCd))//FLC的EXW不产生任何费用
                    continue;
                #region 获取party类型
                List<KeyValuePair<string, string>> pts = new List<KeyValuePair<string, string>>();
                switch (dtoType)
                {
                    case "CarrierI"://货代/carrier
                        pts = ptList.Where(a => "SP_CR".Equals(a.Key)).ToList();
                        break;
                    default:
                        continue;
                }
                #endregion

                foreach (var pt in pts)
                {
                    #region 获取party
                    DataRow[] drs = null;
                    string pt_type = pt.Key;
                    string party_no = string.Empty, party_nm = string.Empty, head_offices = string.Empty;
                    DataTable pt_qtDt = null;
                    string[] partys = null;
                    switch (pt.Key)
                    {
                        case "SP_CR":
                            switch (tranMode)
                            {
                                case "T":
                                    partys = new string[] { "CR", "SP" };
                                    break;
                                default:
                                    partys = new string[] { "IBSP", "IBBR" };
                                    break;
                            }
                            break;
                        default:
                            partys = new string[] { pt.Key };
                            break;
                    }
                    drs = Helper.GetParty(shipment_id, partyDt, partys);
                    #endregion

                    if (drs.Length <= 0)
                    {
                        msg.Add(string.Format("无{0}party", pt.Value));
                        continue;
                    }
                    string partyType = Prolink.Math.GetValueAsString(drs[0]["PARTY_TYPE"]);
                    party_no = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                    party_nm = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                    head_offices = Prolink.Math.GetValueAsString(drs[0]["HEAD_OFFICE"]);
                    if ("IBSP" == partyType)
                    {
                        string billTo = Prolink.Math.GetValueAsString(drs[0]["BILL_TO"]);
                        if (string.IsNullOrEmpty(billTo))
                        {
                            drs = Helper.GetParty(shipment_id, partyDt, new string[] { "IBBR" });
                            if (drs.Length <= 0)
                            {
                                msg.Add("无IBBR party");
                                continue;
                            }
                            party_no = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                            party_nm = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                        }
                        else if (!party_no.Equals(billTo))                     
                        {
                             sql = string.Format("SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(billTo));
                             party_no = billTo;
                             party_nm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
 


                    _current_debitno = party_no;
                    string key = string.Format("{0}_{1}", pt.Key, party_no);
                    WriteLog("结账对象:" + party_nm);

                    EditInstruct m_ei = CreateBillEditInstruct(smsm, _current_debitno, party_no, party_nm, cmp, pt_type);
                    mainEiMap[_current_debitno] = m_ei; 
                    switch (dtoType)
                    {

                        case "CarrierI"://货代/carrier 
                            string local_sql = string.Format("SELECT * FROM(SELECT IS_SHARE,[U_ID],[RFQ_NO],[QUOT_NO],[QUOT_TYPE],[SEQ_NO],[TRAN_MODE],[OUT_IN],[LSP_CD],[EFFECT_DATE],[EFFECT_TO],[TRAN_TYPE],[REGION],[STATE],[POD_CD],[POD_NM],[POL_CD],[POL_NM],[VIA_CD],[VIA_NM],[CARRIER],[ALL_IN],[CUR],F1,F2,F3,F4,F5,F6,F7,F8,F9,[PUNIT],[CHG_CD],[CHG_DESCP],[CHG_TYPE],[SAILING_DAY],[FREE_ODT],[FREE_ODM],[FREE_DDT],[FREE_DDM],[TT],[NOTE],[REMARK],[MIN_AMT],[U_FID],[SERVICE_MODE],[LOADING_FROM],[LOADING_TO],[CUT_OFF],[ETD],[REPAY] FROM SMQTD WHERE TRAN_MODE='X'  AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.POL_CD={0} AND SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='X' AND RLOCATION={1} {2} AND TRAN_TYPE={3}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD,CUR AS M_CUR,INCOTERM,CREDIT_TO,CREDIT_NM,FREIGHT_TERM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(cmp), qtdate_condition, SQLUtils.QuotedStr(tranMode));

                            switch (tranMode)
                            {
                                case "F":
                                    pt_qtDt = Helper.CloneQTTableByType(shipment_id, partyDt, new string[] { "FS", "SP" }, qtDt, tranMode, IncotermCd, LoadingFrom, LoadingTo);//运费
                                    break;
                                case "T":
                                    pt_qtDt = Helper.CloneQTTableByPartyNos(shipment_id, partyDt, new string[] { party_no, head_offices }, qtDt, tranMode, IncotermCd);//运费
                                    break;
                                default:
                                    pt_qtDt = Helper.CloneQTTableByType(shipment_id, partyDt, new string[] { "SP" }, qtDt, tranMode, IncotermCd, LoadingFrom, LoadingTo);//运费
                                    break;
                            }
                             
                            FreightCalculat(parm, pt_qtDt, null, rateDt, d_to, thcDt);
                            break;
                    }
                }
            }
            #endregion
            decimal tcbm = 0M; // Prolink.Math.GetValueAsDecimal(smsm["TCBM"]);

            string bl_win = Prolink.Math.GetValueAsString(smsm["BL_WIN"]).Trim();
            if (!string.IsNullOrEmpty(bl_win) && bl_win.LastIndexOf(' ') > 0)
                bl_win = bl_win.Substring(0, bl_win.LastIndexOf(' '));

            //问题单:115819   FQ/Sheila，对于账务部分的困扰，原则上是：Regions=NA的时候用ATP (账单计价日期), 其他用ATD（Shipment ATD）； 但是，账单的ATD(即Shipment ATD)不能被ATP覆盖，目前ATP会覆盖掉ATD，给月结造成很大困扰。=>如0213會議上說明， region=NA的時候，帳單計價日期是根據ATP，但是帳單日期仍然根據ATD； 需要安排2月份更版。 ADD BY FISH  2017/02/13 
            billDate = GetBillDate(billDate, smsm, false);
            foreach (var kv in mainEiMap)
            {
                string key = kv.Key;
                EditInstruct ei = kv.Value;
                if (!_qt_schems.ContainsKey(key))
                    continue;
                Dictionary<string, object> schems = _qt_schems[key] as Dictionary<string, object>;
                MergeBillEditInstruct(ml, schems, ei, billDate, tranMode, combine_other, Cbm, tcbm, bl_win);
            }

            ClearSMBID(ml, _smbidDt.Select());


            #region 保存数据
            int[] result = null;
            if (updateDataFlag && ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            WriteLogTagStart(string.Format("结束{0}计算", tranMode));
        }



        /// <summary>
        /// 清除账单明细
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="drs"></param>
        private static void ClearSMBID(MixedList ml, DataRow[] drs)
        {
            foreach (DataRow dr in drs)
            {
                if (Prolink.Math.GetValueAsInt(dr["EX_UPDATE"]) != 0)
                    continue;
                dr["EX_UPDATE"] = 1;
                //if (!cur.Equals(Prolink.Math.GetValueAsString(drs[i]["QCUR"])))
                //    continue;
                EditInstruct ei = new EditInstruct("SMBID", EditInstruct.DELETE_OPERATION);
                ei.PutKey("CHG_CD", "DF");
                ei.PutKey("U_ID", dr["U_ID"]);
                if (Prolink.Math.GetValueAsDecimal(dr["UNIT_PRICE"]) != 0 || Prolink.Math.GetValueAsDecimal(dr["BAMT"]) != 0 || Prolink.Math.GetValueAsDecimal(dr["QTY"]) > 0
                  )//  || !string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["U_FID"]))
                {
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.Put("QCUR", "");//從報價帶出的預提
                    ei.Put("QUNIT_PRICE", 0);//從報價帶出的預提
                    ei.Put("QCHG_UNIT", "");//從報價帶出的預提
                    ei.Put("QLAMT", 0);
                    ei.Put("QQTY", 0);//报价数量  "從報價帶出的預提chg_unit如果為K, 就放SMSM.CW, 如果為CBM 就放SMSM.cbm 如果為SET 就放 1如果為CTN 就放SMSM.qty"
                    ei.Put("QAMT", 0);//"從報價帶出的預提 Unit_price X QTY"
                    ei.Put("QUOT_ID", "");//报价主键
                    ei.Put("QUOT_NO", "");
                    ei.Put("RFQ_NO", "");
                    ei.Put("IPART_NO", "");
                    ei.Put("CNTR_STD_QTY", 0);
                    //ei.Put("DEBIT_TO", "");
                    //ei.Put("DEBIT_NM", "");
                }
                ml.Add(ei);
            }
        }


        /// <summary>
        /// 合并费用的EditInstruct
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="schems"></param>
        /// <param name="m_ei"></param>
        public void MergeBillEditInstruct(MixedList ml, Dictionary<string, object> schems, EditInstruct m_ei, DateTime debit_date, string tranMode, string cout, decimal Cbm, decimal tcbm, string bl_win)
        {
            if (schems == null || schems.Count <= 0)
                return;
            int index = Prolink.Math.GetValueAsInt(schems["方案"]);
            List<EditInstruct> list = schems["方案" + index] as List<EditInstruct>;
            decimal amt = Prolink.Math.GetValueAsDecimal(schems["方案" + index + "_amt"]);
            string remark = Prolink.Math.GetValueAsString(schems["方案" + index + "_remark"]);
            string lsp_nm = string.Empty;
            string lsp_no = string.Empty;
            string group_id = string.Empty;
            string cmp = string.Empty;

            if (m_ei != null && !string.IsNullOrEmpty(m_ei.Get("LSP_NM")))
                lsp_nm = m_ei.Get("LSP_NM");
            if (m_ei != null && !string.IsNullOrEmpty(m_ei.Get("LSP_NO")))
                lsp_no = m_ei.Get("LSP_NO");
            if (m_ei != null && !string.IsNullOrEmpty(m_ei.Get("GROUP_ID")))
                group_id = m_ei.Get("GROUP_ID");
            if (m_ei != null && !string.IsNullOrEmpty(m_ei.Get("CMP")))
                cmp = m_ei.Get("CMP");

            DataRow[] drs = _smbidDt.Select(string.Format("LSP_NO={0}", SQLUtils.QuotedStr(m_ei.Get("LSP_NO"))));

            for (int i = 0; i < list.Count; i++)
            {
                EditInstruct ei = list[i];
                ei.PutDate("DEBIT_DATE", debit_date);
                //ei.Put("PARTY_TYPE", pt_type);
                if (!"Y".Equals(ei.Get("HAS_CREDIT_TO")))//无代收
                {
                    if (!string.IsNullOrEmpty(lsp_no))
                    {
                        ei.Put("LSP_NO", lsp_no);
                        ei.Put("LSP_NM", lsp_nm);
                    }
                }
                else
                    ei.Put("REMARK", JoinStr(ei.Get("REMARK"), "Original:" + lsp_no));

                ei.Remove("HAS_CREDIT_TO");
                if (!string.IsNullOrEmpty(group_id))
                    ei.Put("GROUP_ID", group_id);
                if (!string.IsNullOrEmpty(cmp))
                    ei.Put("CMP", cmp);
                if ("F".Equals(tranMode) && "Y".Equals(cout))//只有fcl才会有分摊
                {
                    if ("Y".Equals(ei.Get("IS_SHARE")) && Cbm > 0 && tcbm > 0 && tcbm > Cbm)
                    {
                        decimal offset = Cbm / tcbm;
                        decimal qamt = ei.GetValueAsDecimal("QAMT");
                        decimal qlamt = ei.GetValueAsDecimal("QLAMT");
                        ei.Put("REMARK", JoinStr(ei.Get("REMARK"), string.Format("Share:{0}/{1}={2}", Cbm, tcbm, Helper.Get45AmtValue(offset))));
                        ei.Put("QAMT", qamt * offset);
                        ei.Put("QLAMT", qlamt * offset);
                    }
                }
                else
                    ei.Put("IS_SHARE", "");

                if ("SMBID_TEMP".Equals(ei.ID))
                {
                    //SHIPMENT_ID,LSP_NO,QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT
                    CopyTempBid(ml, ei);
                    continue;
                }

                for (int j = 0; j < drs.Length; j++)
                {
                    DataRow dr = drs[j];
                    if (Prolink.Math.GetValueAsInt(dr["EX_UPDATE"]) != 0)
                        continue;
                    string debitNo = Prolink.Math.GetValueAsString(ei.Get("DEBIT_NO"));
                    string ipart_no = Prolink.Math.GetValueAsString(ei.Get("IPART_NO"));
                    string dn_no = Prolink.Math.GetValueAsString(ei.Get("DN_NO"));
                    //if (string.IsNullOrEmpty(debitNo))
                    //{
                    //    if (Prolink.Math.GetValueAsString(dr["CHG_CD"]).Equals(
                    //        Prolink.Math.GetValueAsString(ei.Get("CHG_CD")))
                    //        && Prolink.Math.GetValueAsString(dr["CHG_TYPE"]).Equals(
                    //        Prolink.Math.GetValueAsString(ei.Get("CHG_TYPE")))
                    //         && Prolink.Math.GetValueAsString(dr["QCHG_UNIT"]).Equals(
                    //        Prolink.Math.GetValueAsString(ei.Get("QCHG_UNIT"))))
                    //    {
                    //        ei.Remove("CUR");
                    //        dr["EX_UPDATE"] = 1;
                    //        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    //        ei.PutKey("U_ID", dr["U_ID"]);
                    //        break;
                    //    }
                    //}
                    if (string.IsNullOrEmpty(ipart_no))
                    {
                        if (Prolink.Math.GetValueAsString(dr["CHG_CD"]).Equals(
                              Prolink.Math.GetValueAsString(ei.Get("CHG_CD"))))
                        {
                            ei.Remove("CUR");
                            dr["EX_UPDATE"] = 1;
                            ei.OperationType = EditInstruct.UPDATE_OPERATION;
                            ei.PutKey("U_ID", dr["U_ID"]);
                            break;
                        }
                    }
                    else if (Prolink.Math.GetValueAsString(dr["CHG_CD"]).Equals(
                              Prolink.Math.GetValueAsString(ei.Get("CHG_CD")))
                        &&
                        Prolink.Math.GetValueAsString(dr["IPART_NO"]).Equals(ipart_no)
                        &&
                        Prolink.Math.GetValueAsString(dr["DN_NO"]).Equals(dn_no))
                    {
                        ei.Remove("CUR");
                        dr["EX_UPDATE"] = 1;
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        ei.PutKey("U_ID", dr["U_ID"]);
                        break;
                    }
                }

                if (ei.OperationType != EditInstruct.DELETE_OPERATION)
                    ei.Put("BOOKING_BY", bl_win);
                SetEstInfo(ei);
                ml.Add(ei);
            }

            //EX_UPDATE
            ClearSMBID(ml, drs);
        }

        /// <summary>
        /// 复制临时表
        /// </summary>
        /// <param name="ml"></param>
        /// <param name="ei"></param>
        private static void CopyTempBid(MixedList ml, EditInstruct ei)
        {
            EditInstruct cdei = new EditInstruct("SMBID_TEMP", EditInstruct.DELETE_OPERATION);
            cdei.PutKey("SHIPMENT_ID", ei.Get("SHIPMENT_ID"));
            cdei.PutKey("LSP_NO", ei.Get("LSP_NO"));
            cdei.PutKey("CHG_CD", ei.Get("CHG_CD"));

            string[] fields = new string[] { "SHIPMENT_ID", "LSP_NO", "QUOT_NO", "CHG_CD", "CHG_TYPE", "QAMT", "QLAMT", "CHG_DESCP", "DEBIT_TO", "DEBIT_NM", "REPAY" };
            EditInstruct cei = new EditInstruct("SMBID_TEMP", EditInstruct.INSERT_OPERATION);
            foreach (string f in fields)
                cei.Put(f, ei.Get(f));
            cei.Put("CUR", ei.Get("QCUR"));
            cei.PutDate("CREATE_DATE", DateTime.Now);
            ml.Add(cdei);
            ml.Add(cei);
        }

        /// <summary>
        /// 合并字符串
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private static string JoinStr(string str1, string str2)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(str1))
                list.Add(str1);
            if (!string.IsNullOrEmpty(str2))
                list.Add(str2);
            if (list.Count <= 0)
                return string.Empty;
            return string.Join(";", list.ToArray());
        }


        /// <summary>
        /// 创建账单主信息
        /// </summary>
        /// <param name="smsm"></param>
        /// <param name="debit_no"></param>
        /// <param name="lsp_no"></param>
        /// <returns></returns>
        private EditInstruct CreateBillEditInstruct(DataRow smsm, string debit_no, string lsp_no, string lsp_nm, string CompanyId, string party_type = "")
        {
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.INSERT_OPERATION);
            //ei.Put("U_ID", smsm["U_ID"]);
            ei.Put("U_ID", debit_no);
            //ei.Put("PARTY_TYPE", party_type);
            ei.Put("GROUP_ID", smsm["GROUP_ID"]);
            ei.Put("CMP", smsm["CMP"]);
            ei.Put("SHIPMENT_ID", smsm["SHIPMENT_ID"]);
            //ei.Put("DEBIT_NO", "INV" + DateTime.Now.Ticks);
            ei.Put("STATUS", "A");//"A:錄製 B:發送 C:拒絕 D:通過 E:請款  F:已付款   V:作廢"
            DateTime odt = DateTime.Now;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);

            ei.PutDate("DEBIT_DATE", odt);//帳單日期除海運整櫃出貨到北美,(Region:NA)的地區依據進港日期結算外,其他的都是以Onboard date 為主,內貿是以離場日為主
            ei.PutDate("DEBIT_DATE_L", ndt);

            //ei.Put("DEBIT_NO", debit_no);
            ei.Put("LSP_NO", lsp_no);
            ei.Put("LSP_NM", lsp_nm);
            ei.Put("BILL_TO", smsm["CMP"]);

            ei.Put("POL", smsm["POL_CD"]);
            ei.Put("POL_NM", smsm["POL_NAME"]);
            ei.Put("POD", smsm["POD_CD"]);
            ei.Put("POD_NM", smsm["POD_NAME"]);
            ei.Put("QTY", smsm["QTY"]);
            ei.Put("QTYU", smsm["QTYU"]);
            ei.Put("GW", smsm["GW"]);
            ei.Put("GWU", smsm["GWU"]);
            //ei.Put("CW", smsm["CW"]); 计费重
            ei.Put("CBM", smsm["CBM"]);
            ei.Put("CUR", smsm["CUR"]);
            ei.Put("EX_RATE", 0);//匯率
            ei.Put("LAMT", 0);
            return ei;
        }




        /// <summary>
        /// 运费计算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt">运费主明细</param>
        /// <param name="othDt">其他费用明细</param>
        /// <param name="rateDt">费率明细</param>
        public void FreightCalculat(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt, List<string> d_to, DataTable buDt)
        {
            List<string> jobNoList = new List<string>();
            List<string> polList = new List<string>();
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            switch (tranMode)
            {
                case "R":
                case "F"://海运FCL
                    FclFreight(parm, dt, othDt, rateDt, d_to, buDt);
                    break;
                case "L":
                    jobNoList = Helper.GetValueList(dt, "U_FID");
                    if (jobNoList.Count > 0)
                    {
                        othDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE U_FID IN {0})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM,QUOT_NO", Helper.JoinString(jobNoList.ToArray())), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        LCLFreight(parm, dt, othDt, rateDt, d_to);
                    }
                    break;
                case "T"://内陆运输
                    TruckFreight(parm, dt, null, rateDt);
                    break;
                case "A"://国际快递
                    AirFreight(parm, dt, null, rateDt, d_to);
                    break;
                //case "D"://国内快递
                //    InlandExpressFreight(parm, dt, null, rateDt);
                //    break;
                case "E"://国际快递
                    ExpressFreight(parm, dt, null, rateDt);
                    break;
            }
        }

        private static List<string> GetChargeType(List<string> d_to, List<string> types = null, List<string> charge_type = null)
        {
            if (charge_type == null)
                charge_type = new List<string>() { "无" };
            if (d_to == null)
                return charge_type;
            if (types == null)
                types = new List<string>() {  DESTINATION_CHARGE, FREIGHT };
            //types = new List<string>() { LOCAL_CHARGE, DESTINATION_CHARGE, FREIGHT };

            foreach (string type in types)
            {
                if (!d_to.Contains(type))
                    continue;
                switch (type)
                {
                    //case LOCAL_CHARGE:
                    //    if (!charge_type.Contains("O"))
                    //        charge_type.Add("O");
                    //    break;
                    case DESTINATION_CHARGE:
                        if (!charge_type.Contains("D"))
                            charge_type.Add("D");
                        break;
                    case FREIGHT:
                        if (!charge_type.Contains("F"))
                            charge_type.Add("F");
                        break;
                }
            }

            return charge_type;
        }


        /// <summary>
        /// FCL RFQ报价同LCL计算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="qtDt"></param>
        /// <param name="rateDt"></param>
        /// <param name="elist"></param>
        /// <param name="charge_type"></param>
        /// <param name="dr"></param>
        /// <param name="msg"></param>
        /// <param name="error"></param>
        /// <param name="otherChg"></param>
        private void AddLCLFreight(Dictionary<string, object> parm, DataTable qtDt, DataTable rateDt, List<EditInstruct> elist, List<string> charge_type, DataRow dr, List<string> msg, out bool error, out decimal total, string tranType = "L")
        {
            string carrier = string.Empty;
            if (parm.ContainsKey("carrier"))
                carrier = Helper.GetUrlDecodeValue(Prolink.Math.GetValueAsString(parm["carrier"]));
            string FrtTerm = parm.ContainsKey("FrtTerm") ? Prolink.Math.GetValueAsString(parm["FrtTerm"]) : string.Empty;

            Dictionary<string, decimal> otherChg = new Dictionary<string, decimal>();
            total = 0M;
            string pol_cd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
            string pod_cd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
            string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);

            DataRow[] drs = null;
            string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
            error = false;
            if (charge_type.Contains("无"))
                drs = qtDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2}", SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));
            else
                drs = qtDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2} AND CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()), SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));
            switch (tranType)
            {
                case "F":
                    WriteLog("FCL RFQ运费报价:" + quot_no + ";笔数:" + drs.Length);
                    break;
                case "R":
                    WriteLog("铁路 RFQ运费报价:" + quot_no + ";笔数:" + drs.Length);
                    break;
                default:
                    WriteLog("LCL运费报价:" + quot_no + ";笔数:" + drs.Length);
                    break;
            }

            foreach (DataRow chg in drs)
            {
                string rq_carrier = Prolink.Math.GetValueAsString(chg["CARRIER"]);
                string cur1 = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                string chg_cd = Prolink.Math.GetValueAsString(chg["CHG_CD"]);
                string chg_descp = Prolink.Math.GetValueAsString(chg["CHG_DESCP"]).ToUpper();
                string punit = Prolink.Math.GetValueAsString(chg["PUNIT"]).ToUpper();
                decimal min_amt = Prolink.Math.GetValueAsDecimal(chg["MIN_AMT"]);
                decimal price = Prolink.Math.GetValueAsDecimal(chg["F1"]);
                decimal qty = Helper.GetQty(punit, parm);
                if (string.IsNullOrEmpty(chg_cd) || "L".Equals(tranType))
                    chg_cd = "OF";
                if (!"L".Equals(tranType))
                {
                    if (qty <= 0)
                    {
                        continue;
                    }
                    //7. Shipment 上的Carrier比對報價細檔的Carrier，若FCL RFQ報價細檔的Carrier為空，則不需比對這一條(表示無條件通過這一個條件，該費用代碼適用所有Carrier)
                    if (!string.IsNullOrEmpty(rq_carrier))
                    {
                        if (!rq_carrier.Equals(carrier))
                            continue;
                    }
                    //与outbound相反 FreightTerm=C时，进行计算
                    if (!"C".Equals(FrtTerm) && "OF".Equals(chg_cd))
                    {
                        continue;
                    }
                    price = Prolink.Math.GetValueAsDecimal(chg["F3"]);

                    switch (chg_cd)//判断是否是特殊费用
                    {
                        case "ENS"://1.	FCL/LCL/AIR shipment 的Rigion 為 EU 的才會有ENS 費用
                        case "ACI"://2.	FCL/LCL/AIR shipment 的目的地國別為 CA 的才會有 ACI 費用
                        case "AMS"://3.	FCL/LCL/AIR  shipment 的目的地國別為 US 的才會有 AMS 費用
                        case "AFR"://4.	FCL/LCL shipment 的目的地國別為 JP 的才會有 AFS 費用
                        case "WEC"://5.	FCL shipment 的目的地國別為 BR 的才會有 WEC 費用
                        case "TRC"://5.	FCL/LCL shipment的電放?=Y (Telex_RLS) 的才會有 TRC 的費用
                        case "CBF"://6.	FCL Shipment上有超過兩個DN 的就要收取CBF 費用
                        case "DGMS"://危險品测磁费--喇叭 DGMS
                        case "DGMB"://危險品测磁费--电池 DGMB
                        case "CTC"://9.	Air 判斷陸運? 如果該欄位=y , 要收取 CTC/SSU 費用.
                        case "SSU ":
                            if (!CheckLocalCCharge(chg_cd, parm))
                                continue;
                            break;
                    }
                }
                decimal val = price * qty;
                if ("%".Equals(punit))
                {
                    val = Helper.Get45AmtValue(qty * price * 0.01M);
                }

                if (val < min_amt)
                {
                    val = min_amt;
                    msg.Add(string.Format("{0}:({1}*{2}{6}<最低价:{3}){4}{5}", chg_descp, price, qty, min_amt, val, Helper.GetLoalCurName(cur1), punit));
                }
                else
                    msg.Add(string.Format("{0}:({1}*{2}{6}>=最低价{3}){4}{5}", chg_descp, price, qty, min_amt, val, Helper.GetLoalCurName(cur1), punit));
                decimal temp1 = 0M;
                //if ("%".Equals(punit))
                //    cur1 = "USD";
                chg["QEX_RATE"] = Helper.GetTotal(rateDt, msg, val, cur1, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    chg["QEX_RATE"] = 1;
                    price = temp1;
                    qty = 1;
                    val = temp1;
                    chg["CUR"] = _localCur;
                }

                chg["LOCALE_AMT"] = temp1;
                chg["QCHG_UNIT"] = punit;
                chg["QUNIT_PRICE"] = price;
                chg["QQTY"] = qty;
                chg["QAMT"] = Helper.Get45AmtValue(val);
                chg["C_FLAG"] = "Y";

                SetChargeInfo(chg, chg_cd, tranType);
                _qcount++;
                if (_topOne)
                    elist.Add(CreateBillItem(_current_smsm, chg, _current_debitno));
                if (!otherChg.ContainsKey(cur1))
                    otherChg[cur1] = val;
                else
                    otherChg[cur1] = otherChg[cur1] + val;
            }

            if (otherChg.Count > 0)
            {
                string othMsg = "";
                foreach (var kv in otherChg)
                {
                    if (othMsg.Length > 0)
                        othMsg += ",";
                    othMsg += kv.Value + kv.Key;
                    Helper.GetTotal(rateDt, msg, kv.Value, kv.Key, ref total, ref error);
                }
                msg.Add(string.Format("费用：{0}", othMsg));
            }
        }

        private string getfieldFromOtherMapping(string cnttype)
        {
            if (OtherCntTypeMapping == null || OtherCntTypeMapping.Count <= 0)
                return string.Empty;
            if (OtherCntTypeMapping.ContainsKey(cnttype))
                return OtherCntTypeMapping[cnttype];
            return string.Empty;
        }

        /// <summary>
        /// 检查local C类费用
        /// </summary>
        /// <param name="chg_cd"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static bool CheckLocalCCharge(string chg_cd, Dictionary<string, object> parm)
        {
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            string pod = Prolink.Math.GetValueAsString(parm["pod"]);
            string telex_rls = Prolink.Math.GetValueAsString(parm["telex_rls"]);//电放
            string region = Prolink.Math.GetValueAsString(parm["region"]);//区域
            string horn = Prolink.Math.GetValueAsString(parm["horn"]);//喇叭
            string battery = Prolink.Math.GetValueAsString(parm["battery"]); //離電池
            string is_land = Prolink.Math.GetValueAsString(parm["is_land"]); //離電池
            int DnNum = Prolink.Math.GetValueAsInt(parm["DnNum"]); //dn 数量
            string pod_rigion = string.Empty;
            if (!string.IsNullOrEmpty(pod) && pod.Length >= 2)
                pod_rigion = pod.Substring(0, 2);
            bool result = false;
            switch (chg_cd)
            {
                case "ENS"://1.	FCL/LCL/AIR shipment 的Rigion 為 EU 的才會有ENS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("EU".Equals(region))
                                result = true;
                            break;
                    }
                    break;
                case "ACI"://2.	FCL/LCL/AIR shipment 的目的地國別為 CA 的才會有 ACI 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("CA".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "AMS"://3.	FCL/LCL/AIR  shipment 的目的地國別為 US 的才會有 AMS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("US".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "AFR"://4.	FCL/LCL shipment 的目的地國別為 JP 的才會有 AFS 費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                            if ("JP".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "WEC"://5.	FCL shipment 的目的地國別為 BR 的才會有 WEC 費用
                    switch (tranMode)
                    {
                        case "F":
                            if ("BR".Equals(pod_rigion))
                                result = true;
                            break;
                    }
                    break;
                case "TRC"://5.	FCL/LCL shipment的電放?=Y (Telex_RLS) 的才會有 TRC 的費用
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                            if ("Y".Equals(telex_rls))
                                result = true;
                            break;
                    }
                    break;
                case "CBF"://6.	FCL Shipment上有超過兩個DN 的就要收取CBF 費用
                    if ("F".Equals(tranMode) && DnNum > 2)
                    {
                        result = true;
                    }
                    break;
                case "DGMS"://危險品测磁费--喇叭 DGMS
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(horn))
                            result = true;
                    }
                    break;
                case "DGMB"://危險品测磁费--电池 DGMB
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(battery))
                            result = true;
                    }
                    break;
                //case "DMG"://7.	AIR DN增加喇叭? horn x(1)  / 離電池? Battery x(1) 要帶到 Shipment 如果為Y 要計算DMG 的費用.
                //    if ("A".Equals(tranMode))
                //    {
                //        if ("Y".Equals(horn) || "Y".Equals(battery))
                //            result = true;
                //    }
                //    break;
                case "CTC"://9.	Air 判斷陸運? 如果該欄位=y , 要收取 CTC/SSU 費用.
                case "SSU ":
                    if ("A".Equals(tranMode))
                    {
                        if ("Y".Equals(is_land))
                            result = true;
                    }
                    break;
            }
            return result;
        }

        private Dictionary<string, object> AirOthFreight(DataTable airDt, Dictionary<string, object> parm, DataTable rateDt, List<string> d_to)
        {
            Dictionary<string, object> schems = GetSchems();
            if (airDt == null || airDt.Rows.Count <= 0) return schems;

            Helper.AddOthColumns(airDt);
            decimal total = 0M;
            int index = 1;
            List<EditInstruct> elist = GetEiList(schems, index);
            List<string> msg = new List<string>();
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);

            List<string> charge_type = GetChargeType(d_to);
            string topquot_no = Prolink.Math.GetValueAsString(airDt.Rows[0]["QUOT_NO"]);
            DataRow[] drs = airDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()));
            List<string> testList = new List<string>();
            foreach (DataRow air in drs)
            {
                string quot_no = Prolink.Math.GetValueAsString(air["QUOT_NO"]);
                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (!topquot_no.Equals(quot_no))
                    break;
                if ("AF".Equals(Prolink.Math.GetValueAsString(air["CHG_CD"])))
                    continue;
               
                bool error = false;

                if (CalCNT(parm, air, rateDt, tranMode, ref error, msg, elist, null, true))
                    continue;
                decimal min = Prolink.Math.GetValueAsDecimal(air["MIN_AMT"]);
                string cur = Prolink.Math.GetValueAsString(air["CUR"]);
                decimal F1 = Prolink.Math.GetValueAsDecimal(air["F3"]);
                string punit = Prolink.Math.GetValueAsString(air["PUNIT"]);
                decimal qty = Helper.GetQty(punit, parm);
                decimal cur_total = Helper.Get45AmtValue(qty * F1);
                if ("%".Equals(punit))
                {
                    cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                }
                //if (qty <= 0 || F1 <= 0)
                //    continue;
                decimal temp1 = 0M;
                //if ("%".Equals(punit))
                //    cur = "USD";
                air["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    air["QEX_RATE"] = 1;
                    F1 = temp1;
                    qty = 1;
                    cur_total = temp1;
                    air["CUR"] = _localCur;
                }
                 
                if (min > 0)
                    msg.Add(string.Format("最低费用{0}{1}", Helper.Get45AmtValue(min), Helper.GetLoalCurName(cur))); 
                if (cur_total < min)
                {
                    msg.Add(string.Format("费用{0}小于最低费用", Helper.Get45AmtValue(cur_total), Helper.GetLoalCurName(cur)));
                    cur_total = min;
                }
                air["LOCALE_AMT"] = temp1;
                air["EX_REMARK"] = "";
                SetChargeInfo(air, "", tranMode);

                air["QCHG_UNIT"] = punit;
                air["QUNIT_PRICE"] = F1;
                air["QQTY"] = qty;
                air["QAMT"] = cur_total;
                air["C_FLAG"] = "Y";
                elist.Add(CreateBillItem(_current_smsm, air, _current_debitno, "", "", false));
            }
            schems["方案"] = index;
            schems["方案" + index] = elist;
            schems["方案" + index + "_amt"] = total;
            schems["方案" + index + "_remark"] = string.Join(";", msg);
            WriteLog(string.Join(";", msg));
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        private bool CalCNT(Dictionary<string, object> parm, DataRow local, DataTable rateDt, string tranMode, ref bool error, List<string> msg, List<EditInstruct> elist, DataRow thc = null, bool isFreight = false, string credit_to = "", string credit_nm = "", bool isTrailer = false)
        {
            decimal cur_total = 0m;
            string cur = Prolink.Math.GetValueAsString(local["CUR"]);
            List<string> cntMsg = new List<string>();
            DataRow dr = local;
            Dictionary<string, string> cntParm = _cntParm;
            if (isTrailer)
                cntParm = _cntParmt;
            if (thc != null)
            {
                dr = thc;
                cntParm = _cntParm1;
                cur = Prolink.Math.GetValueAsString(thc["CUR"]);
                if (string.IsNullOrEmpty(cur))
                    cur = Prolink.Math.GetValueAsString(local["CUR"]);
                else
                    local["CUR"] = cur;
            }

            foreach (var kv in cntParm)
            {
                decimal price = Prolink.Math.GetValueAsDecimal(dr[kv.Value]);
                string punit = kv.Key;
                decimal qty = GetQty(punit, parm, null, _cntParmList);
                if (qty <= 0 || price <= 0)
                    continue;
                cntMsg.Add(string.Format("{0}({1}*{2}{3})", kv.Key, qty, price, cur));
                cur_total += Helper.Get45AmtValue(qty * price);
            }

            if (cur_total > 0)
            {
                msg.Add(string.Join("+", cntMsg));
                local["EX_REMARK"] = string.Join("+", cntMsg);
                local["QCHG_UNIT"] = "CTR";
                local["QUNIT_PRICE"] = cur_total;
                local["QQTY"] = 1;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                decimal temp1 = 0M;
                local["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                local["LOCALE_AMT"] = temp1;
                SetChargeInfo(local, "", tranMode);
                elist.Add(CreateBillItem(_current_smsm, local, _current_debitno, credit_to, credit_nm, false, isFreight));
                local["EX_REMARK"] = "";
            }
            return cur_total > 0;
        }

        public static decimal GetQty(string punit, Dictionary<string, object> parm, List<string> msg = null, List<string> cntparmlist = null)
        {
            decimal qty = 1;
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            decimal gw = 0M;
            decimal cmb = 0M;
            string trackWay = string.Empty;
            if (parm.ContainsKey("TrackWay"))
                trackWay = Prolink.Math.GetValueAsString(parm["TrackWay"]);

            string iscombine_bl = string.Empty;
            if (parm.ContainsKey("ISCOMBINE_BL"))
                iscombine_bl = Prolink.Math.GetValueAsString(parm["ISCOMBINE_BL"]);
            string ocnttype = string.Empty;
            if (parm.ContainsKey("OcntType"))
                ocnttype = Prolink.Math.GetValueAsString(parm["OcntType"]);
            string pcnttype = string.Empty;
            if (parm.ContainsKey("CntType"))
                pcnttype = Prolink.Math.GetValueAsString(parm["CntType"]);
            if (cntparmlist != null && cntparmlist.Contains(punit))
            {
                if (punit == ocnttype)
                {
                    if (parm.ContainsKey("OcntNumber"))
                        return Prolink.Math.GetValueAsDecimal(parm["OcntNumber"]);
                }
                if (punit == pcnttype)
                {
                    if (parm.ContainsKey("CntNumber"))
                        return Prolink.Math.GetValueAsDecimal(parm["CntNumber"]);
                }
                return 0;
            }

            switch (punit)
            {
                case "20GP":
                    qty = Helper.GetDecimalValue(parm["Cnt20"]);
                    break;
                case "40GP":
                    qty = Helper.GetDecimalValue(parm["Cnt40"]);
                    break;
                case "40HQ":
                    qty = Helper.GetDecimalValue(parm["Cnt40hq"]);
                    break;
                case "CNT":
                case "CTR":
                    qty = Helper.GetDecimalValue(parm["CntNum"]);
                    break;
                case "DN":
                    qty = Helper.GetDecimalValue(parm["DnNum"]);
                    break;
                case "BL":
                    qty = 1;
                    break;
                case "SHT"://by shipment
                    qty = 1;
                    if ("Y".Equals(iscombine_bl) || "C".Equals(iscombine_bl))//现在用C
                    {
                        if (parm.ContainsKey("SHIPMENT_INFO"))
                        {
                            string[] temps = Prolink.Math.GetValueAsString(parm["SHIPMENT_INFO"]).Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                            if (temps.Length > 0)
                                qty = temps.Distinct().ToList().Count;
                        }
                    }
                    break;
                case "M3":
                case "CBM":
                    qty = Helper.GetDecimalValue(parm["cbm"]);
                    switch (tranMode)
                    {
                        case "L":
                        case "F":
                            if (qty < 1M)
                                qty = 1M;
                            break;
                    }
                    break;
                case "CTN":
                case "PLT":
                    qty = Helper.GetDecimalValue(parm["cnt"]);
                    break;
                case "CW":
                    decimal cw = 0;
                    if (parm.ContainsKey("nw"))
                    {
                        cw = Helper.GetDecimalValue(parm["nw"]);
                        return cw;
                    }

                    gw = GetQty("KG", parm, msg);
                    cmb = Helper.GetDecimalValue(parm["cbm"]);
                    switch (tranMode)
                    {
                        case "T":
                            if ("A".Equals(trackWay))
                                qty = Helper.GetCW(gw, cmb, 6000m, trackWay);
                            else
                                qty = Helper.GetCW(gw, cmb, 6000m);
                            break;
                        default:
                            qty = Helper.GetCW(gw, cmb, 6000m, tranMode);
                            break;
                    }
                    break;
                case "L":
                case "G":
                case "LB":
                case "K":
                case "KG":
                case "KGM":
                case "KGS":
                    string gwu = Prolink.Math.GetValueAsString(parm["gwu"]).ToUpper();
                    qty = Helper.GetDecimalValue(parm["gw"]);
                    qty = Helper.GetKGWeight(punit, qty, ref gwu, msg);
                    break;
                case "%":
                    qty = parm.ContainsKey("Gvalue") ? Prolink.Math.GetValueAsDecimal(parm["Gvalue"]) : 0;
                    break;
                default:
                    qty = 1;
                    break;
            }
            return qty;
        }


        /// <summary>
        /// FLC 运费试算
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="parm"></param>
        private Dictionary<string, object> FclFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt, List<string> d_to, DataTable buDt)
        {
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = null;


            Helper.AddOthColumns(dt);
            decimal Cnt20 = Helper.GetDecimalValue(parm["Cnt20"]);
            decimal Cnt40 = Helper.GetDecimalValue(parm["Cnt40"]);
            decimal Cnt40hq = Helper.GetDecimalValue(parm["Cnt40hq"]);

            decimal Cnt40Other = Helper.GetDecimalValue(parm["OcntNumber"]);
            string Cnt40OtherType = Prolink.Math.GetValueAsString(parm["OcntType"]);

            decimal Cnt40Other2 = 0M;
            if (parm.ContainsKey("CntNumber"))
            {
                Cnt40Other2 = Helper.GetDecimalValue(parm["CntNumber"]);
            }
            string Cnt40OtherType2 = string.Empty;
            if (parm.ContainsKey("CntType"))
            {
                Cnt40OtherType2 = Prolink.Math.GetValueAsString(parm["CntType"]);
            }
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            string BrgType = parm.ContainsKey("BrgType") ? Prolink.Math.GetValueAsString(parm["BrgType"]) : string.Empty;
            string via = parm.ContainsKey("via") ? Prolink.Math.GetValueAsString(parm["via"]) : string.Empty;

            List<string> vias = new List<string>();
            if (!string.IsNullOrEmpty(BrgType)) vias.Add(BrgType);
            if (!string.IsNullOrEmpty(via)) vias.Add(via);

            string carrier = string.Empty;
            if (parm.ContainsKey("carrier"))
                carrier = Helper.GetUrlDecodeValue(Prolink.Math.GetValueAsString(parm["carrier"]));
            decimal pre_total = 0m;
            DataRow[] drs = null;
            int index = 0;
            DataRow[] qts = dt.Select();
            string topquot_no = string.Empty;
            string quot_id = string.Empty;
            string filter = "1=1";
            if (dt.Rows.Count > 0)
            {
                topquot_no = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_NO"]);
                quot_id = Prolink.Math.GetValueAsString(dt.Rows[0]["U_FID"]);
                filter += string.Format(" AND U_FID={0}", SQLUtils.QuotedStr(quot_id));
            }
            string car_filter = string.Empty;
            if (!string.IsNullOrEmpty(carrier))
            {
                car_filter = string.Format(" AND CARRIER={0}", SQLUtils.QuotedStr(carrier));
            }
            string via_filter = string.Empty;
            if (vias.Count > 0)
            {
                via_filter = string.Format(" AND VIA_CD IN ({0})", string.Join(",", vias.Select(v => SQLUtils.QuotedStr(v))));
            }
            else
                via_filter = string.Format(" AND (VIA_CD IS NULL OR VIA_CD='')");
            qts = dt.Select(filter + car_filter + via_filter);
            if (qts.Length <= 0 && !string.IsNullOrEmpty(car_filter)) qts = dt.Select(filter + car_filter);
            if (qts.Length <= 0 && !string.IsNullOrEmpty(via_filter)) qts = dt.Select(filter + via_filter);
            if (qts.Length <= 0) qts = dt.Select(filter);

            List<string> charge_type = GetChargeType(d_to);
            List<string> LCLcharge_type = GetChargeType(d_to, null, new List<string>());
            foreach (DataRow dr in qts)
            {
                string via_cd = Prolink.Math.GetValueAsString(dr["VIA_CD"]);
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                index++;
                elist = GetEiList(schems, index);

                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                string qt_id = Prolink.Math.GetValueAsString(dr["U_ID"]);//报价号码
                decimal cnt = 0M;
                decimal total = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                decimal F12 = 0m, F13 = 0m, F14 = 0m;

                string Period = dr.Table.Columns.Contains("PERIOD") ? Prolink.Math.GetValueAsString(dr["PERIOD"]) : string.Empty;//询价类别
                if ("R".Equals(Period))//RFQ计算
                {
                    AddLCLFreight(parm, dt, rateDt, elist, LCLcharge_type, dr, msg, out error, out total, tranMode);
                }
                else
                {
                    Func<string, string> ChargeInfo = value =>
                    {
                        if ("R".Equals(tranMode)) 
                            SetChargeInfo(dr, "RF", "R"); 
                        else
                            SetChargeInfo(dr, "OF", "F");
                        return string.Empty;
                    };


                    #region 报价消息提示
                    decimal F2 = Prolink.Math.GetValueAsDecimal(dr["F2"]);//20' 
                    if (Cnt20 > 0 && F2 <= 0)
                    {
                        error = true;
                        msg.Add("无20'报价");
                    }
                    else if (Cnt20 > 0 && F2 > 0)
                        msg.Add(string.Format("20'({0}+{1}{2})*{3}", F2, F12, Helper.GetLoalCurName(cur), Cnt20));

                    decimal F3 = Prolink.Math.GetValueAsDecimal(dr["F3"]);//40'
                    if (Cnt40 > 0 && F3 <= 0)
                    {
                        error = true;
                        msg.Add("无40'报价");
                    }
                    else if (Cnt40 > 0 && F3 > 0)
                        msg.Add(string.Format("40'({0}+{1}{2})*{3}", F3, F13, Helper.GetLoalCurName(cur), Cnt40));

                    decimal Ffiled = 0m;
                    if (Cnt40Other > 0)
                    {
                        string field = getfieldFromOtherMapping(Cnt40OtherType);
                        if (!string.IsNullOrEmpty(field))
                        {
                            Ffiled = Prolink.Math.GetValueAsDecimal(dr[field]);//40HQ
                            if (Ffiled <= 0)
                            {
                                error = true;
                                msg.Add("无" + Cnt40OtherType + "报价");
                            }
                            else
                            {
                                msg.Add(string.Format("{0}({1}+{2}{3})*{4}", Cnt40OtherType, Ffiled, F14, Helper.GetLoalCurName(cur), Cnt40OtherType));
                            }
                        }
                    }

                    if (Cnt40Other2 > 0)
                    {
                        string field = getfieldFromOtherMapping(Cnt40OtherType2);
                        if (!string.IsNullOrEmpty(field))
                        {
                            Ffiled = Prolink.Math.GetValueAsDecimal(dr[field]);//40HQ
                            if (Ffiled <= 0)
                            {
                                error = true;
                                msg.Add("无" + Cnt40OtherType2 + "报价");
                            }
                            else
                            {
                                msg.Add(string.Format("{0}({1}+{2}{3})*{4}", Cnt40OtherType2, Ffiled, F14, Helper.GetLoalCurName(cur), Cnt40OtherType2));
                            }
                        }
                    }

                    decimal F4 = Prolink.Math.GetValueAsDecimal(dr["F4"]);//40HQ
                    if (Cnt40hq > 0 && F4 <= 0)
                    {
                        error = true;
                        msg.Add("无40HQ报价");
                    }
                    else if (Cnt40hq > 0 && F4 > 0)
                        msg.Add(string.Format("40HQ({0}+{1}{2})*{3}", F4, F14, Helper.GetLoalCurName(cur), Cnt40hq));

                    msg.Add(string.Format("货柜费用{0}{1}", Helper.Get45AmtValue(cnt), Helper.GetLoalCurName(cur)));
                    #endregion

                    #region 计算20尺柜
                    if (Cnt20 > 0)
                    {
                        decimal temp = Helper.Get45AmtValue(Cnt20 * (F2 + F12));
                        cnt += temp;
                        decimal temp1 = 0;
                        dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                        total += temp1;
                        dr["LOCALE_AMT"] = temp1;

                        dr["QCHG_UNIT"] = Unit.CNT20GP;
                        dr["QUNIT_PRICE"] = (F2 + F12);
                        dr["QQTY"] = Cnt20;
                        dr["QAMT"] = temp;
                        dr["C_FLAG"] = "Y";

                        ChargeInfo(null);

                        _qcount++;
                        if (_topOne)
                            elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                    }
                    #endregion

                    #region 计算40尺柜
                    if (Cnt40 > 0)
                    {
                        decimal temp = Helper.Get45AmtValue(Cnt40 * (F3 + F13));
                        cnt += temp;
                        decimal temp1 = 0;
                        dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                        total += temp1;
                        dr["LOCALE_AMT"] = temp1;

                        ChargeInfo(null);

                        dr["QCHG_UNIT"] = Unit.CNT40GP;
                        dr["QUNIT_PRICE"] = (F3 + F13);
                        dr["QQTY"] = Cnt40;
                        dr["QAMT"] = temp;
                        dr["C_FLAG"] = "Y";

                        _qcount++;
                        if (_topOne)
                            elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                    }
                    #endregion

                    #region 计算40HQ
                    if (Cnt40hq > 0)
                    {
                        decimal temp = Helper.Get45AmtValue(Cnt40hq * (F4 + F14));
                        cnt += temp;

                        decimal temp1 = 0;
                        dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                        total += temp1;
                        //decimal temp1 = Helper.GetRateAmt(rateDt, temp, cur, _localCur, ref error, msg);
                        total += temp1;
                        dr["LOCALE_AMT"] = temp1;

                        ChargeInfo(null);

                        dr["QCHG_UNIT"] = Unit.CNT40HQ;
                        dr["QUNIT_PRICE"] = (F4 + F14);
                        dr["QQTY"] = Cnt40hq;
                        dr["QAMT"] = temp;
                        dr["C_FLAG"] = "Y";

                        _qcount++;
                        if (_topOne)
                            elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                    }
                    #endregion

                    #region 计算40 Other
                    if (Cnt40Other > 0)
                    {
                        string field = getfieldFromOtherMapping(Cnt40OtherType);
                        if (!string.IsNullOrEmpty(field))
                        {
                            Ffiled = Prolink.Math.GetValueAsDecimal(dr[field]);//40HQ

                            decimal temp = Helper.Get45AmtValue(Cnt40Other * (Ffiled));
                            cnt += temp;

                            decimal temp1 = 0;
                            dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                            total += temp1;
                            //decimal temp1 = Helper.GetRateAmt(rateDt, temp, cur, _localCur, ref error, msg);
                            total += temp1;
                            dr["LOCALE_AMT"] = temp1;

                            ChargeInfo(null);

                            dr["QCHG_UNIT"] = Cnt40OtherType;
                            dr["QUNIT_PRICE"] = (Ffiled);
                            dr["QQTY"] = Cnt40Other;
                            dr["QAMT"] = temp;
                            dr["C_FLAG"] = "Y";

                            _qcount++;
                            if (_topOne)
                                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                        }
                    }

                    if (Cnt40Other2 > 0)
                    {
                        string field = getfieldFromOtherMapping(Cnt40OtherType2);
                        if (!string.IsNullOrEmpty(field))
                        {
                            Ffiled = Prolink.Math.GetValueAsDecimal(dr[field]);//40HQ
                            decimal temp = Helper.Get45AmtValue(Cnt40Other2 * (Ffiled));
                            cnt += temp;

                            decimal temp1 = 0;
                            dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                            total += temp1;
                            //decimal temp1 = Helper.GetRateAmt(rateDt, temp, cur, _localCur, ref error, msg);
                            total += temp1;
                            dr["LOCALE_AMT"] = temp1;

                            ChargeInfo(null);

                            dr["QCHG_UNIT"] = Cnt40OtherType2;
                            dr["QUNIT_PRICE"] = (Ffiled);
                            dr["QQTY"] = Cnt40Other2;
                            dr["QAMT"] = temp;
                            dr["C_FLAG"] = "Y";

                            _qcount++;
                            if (_topOne)
                                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                        }
                    }
                    #endregion

                    #region 港口其他费用
                    filter = string.Format("POL_CD={0}", SQLUtils.QuotedStr(polCd));
                    if (!string.IsNullOrEmpty(carrier))
                    {
                        filter += " AND (CARRIER=" + SQLUtils.QuotedStr(carrier) + " OR CARRIER IS NULL)";
                    }
                     
                    #endregion

                    if (error)
                        msg.Add("无法计算本地CNY费用");
                    else
                    {
                        dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                        msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                    }
                }
                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }
                schems["方案" + index + "_remark"] = string.Join("；", msg);

                WriteLog(index, string.IsNullOrEmpty(quot_no) ? qt_id : quot_no, msg, total);//

                dr["CHG_REMARK"] = string.Join("；", msg);

                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// LCL运费试算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="qtDt"></param>
        /// <param name="rateDt"></param>
        private Dictionary<string, object> LCLFreight(Dictionary<string, object> parm, DataTable dt, DataTable qtDt, DataTable rateDt, List<string> d_to)
        {
            Helper.AddOthColumns(dt);
            Helper.AddOthColumns(qtDt);
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = null;
            int index = 0;
            decimal pre_total = 0m;
            //decimal cw = GetDecimalValue(parm["cw"]);
            decimal gw = Helper.GetDecimalValue(parm["gw"]);
            decimal cbm = Helper.GetDecimalValue(parm["cbm"]);
            decimal cnt = Helper.GetDecimalValue(parm["cnt"]);

            List<string> charge_type = GetChargeType(d_to);


            foreach (DataRow dr in dt.Rows)
            {
                string pol_cd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
                string pod_cd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);

                if (string.IsNullOrEmpty(pol_cd))
                    continue;

                bool error = false;
                index++;
                elist = GetEiList(schems, index);
                List<string> msg = new List<string>();
                decimal total = 0m;

                AddLCLFreight(parm, qtDt, rateDt, elist, charge_type, dr, msg, out error, out total);

                if (error)
                    msg.Add("无法计算本地CNY费用");
                else
                {
                    dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                    msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                }

                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }
                schems["方案" + index + "_remark"] = string.Join("；", msg);
                dr["CHG_REMARK"] = string.Join("；", msg);

                WriteLog(index, Prolink.Math.GetValueAsString(dr["QUOT_NO"]), msg, total);
                if (_topOne) break;
                //QCHG_UNIT,QUNIT_PRICE,QQTY,QAMT,C_FLAG
                //"從報價帶出的預提 CHG_UNIT   如果為K, 就放SMSM.CW, 如果為CBM 就放SMSM.cbm 如果為SET 就放 1,如果為CTN 就放SMSM.qty"
            }
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// 空运试算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private Dictionary<string, object> AirFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt, List<string> d_to)
        {
            Dictionary<string, object> schems = GetSchems();

            List<EditInstruct> elist = null;
            int index = 0;
            decimal pre_total = 0m;
            Helper.AddOthColumns(dt);
            //decimal cw = Helper.GetCW(Helper.GetQty("KG", parm), Helper.GetDecimalValue(parm["cbm"]), 6000M,"A");
            decimal cw = 0;
            if (parm.ContainsKey("nw"))
                cw = Helper.GetDecimalValue(parm["nw"]);
            else if (parm.ContainsKey("cw"))
                cw = Helper.GetDecimalValue(parm["cw"]);
            else
                cw = Helper.GetCW(Helper.GetQty("KG", parm), Helper.GetDecimalValue(parm["cbm"]), 6000M, "A");
            //decimal cw = Helper.GetDecimalValue(parm["gw"]);
            string order = "QUOT_NO";
            if (dt.Columns.Contains("QT_EFFECT_FROM"))
                order = "QT_EFFECT_FROM DESC,QUOT_NO";

            Dictionary<string, decimal> map = new Dictionary<string, decimal>();
            map["F1"] = -45M;
            map["F7"] = 2000M;
            map["F6"] = 1000M;
            map["F5"] = 500M;
            map["F4"] = 300M;
            map["F3"] = 100M;
            map["F2"] = 45M;

            string carrier = string.Empty;
            if (parm.ContainsKey("carrier"))
                carrier = Helper.GetUrlDecodeValue(Prolink.Math.GetValueAsString(parm["carrier"]));
            DataRow[] qts = dt.Select();
            string topquot_no = string.Empty;
            string quot_id = string.Empty;
            string polCd = string.Empty;
            if (!string.IsNullOrEmpty(carrier))
            {
                if (dt.Rows.Count > 0)
                {
                    topquot_no = Prolink.Math.GetValueAsString(dt.Rows[0]["QUOT_NO"]);
                    quot_id = Prolink.Math.GetValueAsString(dt.Rows[0]["U_FID"]);
                }
                qts = dt.Select(string.Format("U_FID={0} AND CARRIER={1} AND CHG_CD='AF'", SQLUtils.QuotedStr(quot_id), SQLUtils.QuotedStr(carrier)), order);
                if (qts.Length <= 0) qts = dt.Select("", order);
            }
            List<string> testList = new List<string>();

            foreach (DataRow dr in qts)
            {
                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                if (_topOne)
                {
                    if (!"AF".Equals(Prolink.Math.GetValueAsString(dr["CHG_CD"])))
                        continue;
                    polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
                    if (string.IsNullOrEmpty(quot_no))
                        continue;
                    if (testList.Count <= 0) testList.Add(quot_no);
                    if (!testList.Contains(quot_no)) break;
                }
                quot_id = Prolink.Math.GetValueAsString(dr["U_FID"]);
                decimal total = 0M;
                decimal curTotal = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                decimal min = Prolink.Math.GetValueAsDecimal(dr["MIN_AMT"]);

                index++;
                elist = GetEiList(schems, index);

                map = Helper.GetEcreffee("A", Prolink.Math.GetValueAsString(dr["LSP_CD"]), map);

                #region 获取报价
                decimal price = 0m;
                foreach (var kv in map)
                {
                    decimal val = kv.Value;
                    if (val < 0)
                    {
                        val = System.Math.Abs(val);
                        if (cw < val)
                        {

                            price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                            if (price != 0)
                            {
                                msg.Add(string.Format("{0}<{1}({2}{3})*{4}", cw, val, price, Helper.GetLoalCurName(cur), cw));
                                break;
                            }
                        }
                    }
                    else if (cw >= val)
                    {
                        price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                        if (price != 0)
                        {
                            msg.Add(string.Format("{0}>={1}({2}{3})*{4}", cw, val, price, Helper.GetLoalCurName(cur), cw));
                            break;
                        }
                    }
                }
                #endregion

                if (price <= 0)
                    msg.Add("无对应的报价");

                if (min > 0)
                    msg.Add(string.Format("最低费用{0}{1}", Helper.Get45AmtValue(min), Helper.GetLoalCurName(cur)));

                curTotal = price * cw;
                if (curTotal < min)
                {
                    msg.Add(string.Format("费用{0}小于最低费用", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur)));
                    curTotal = min;
                }

                dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format("总费用{0}{1}", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur)));
                if (error)
                    msg.Add("无法计算本地CNY费用");
                else
                {
                    dr["C_FLAG"] = "Y";
                    dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                    msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                }
                dr["QCHG_UNIT"] = "K";
                dr["QUNIT_PRICE"] = price;
                dr["QQTY"] = cw;
                dr["QAMT"] = Helper.Get45AmtValue(curTotal);
                _qcount++;
                SetChargeInfo(dr, Prolink.Math.GetValueAsString(dr["CHG_CD"]), "A");
                if (_topOne)
                    elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));

                dr["CHG_REMARK"] = string.Join("；", msg);

                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                schems["方案" + index + "_remark"] = string.Join(";", msg);
                WriteLog(index, quot_no, msg, total);
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }
                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;

            if (!string.IsNullOrEmpty(quot_id))
            {
                othDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND POL_CD={1} AND CHG_CD<>'AF'", SQLUtils.QuotedStr(quot_id), SQLUtils.QuotedStr(polCd)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                AirOthFreight(othDt, parm, rateDt, d_to);
            }
            return schems;
        }


        /// <summary>
        /// 国内快递
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private Dictionary<string, object> InlandExpressFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            Helper.AddOthColumns(dt);
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = null;
            int index = 0;
            decimal cw = Helper.GetDecimalValue(parm["gw"]);
            decimal pre_total = 0m;
            foreach (DataRow dr in dt.Rows)
            {
                index++;
                elist = GetEiList(schems, index);

                decimal total = 0m;
                decimal curTotal = 0m;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                bool error = false;

                List<string> msg = new List<string>();
                decimal first = 0.5m;
                decimal pre = 0.5m;
                decimal price = Prolink.Math.GetValueAsDecimal(dr["F1"]);//0.5 首重
                decimal price_more = Prolink.Math.GetValueAsDecimal(dr["F3"]);//0.5 继重
                #region 计算首重和续重单价
                if (price <= 0)
                {
                    price = Prolink.Math.GetValueAsDecimal(dr["F2"]);//1首重
                    first = 1M;
                }
                if (price_more <= 0)
                {
                    price_more = Prolink.Math.GetValueAsDecimal(dr["F4"]);//1 继重
                    if (price_more > 0)
                    {
                        pre = 1M;
                    }
                    else
                        price_more = price;
                }
                if (price <= 0)
                {
                    error = false;
                    msg.Add("首重无对应的报价");
                }
                if (price_more <= 0)
                {
                    error = false;
                    msg.Add("继重无对应的报价");
                }
                #endregion

                #region 计算运费
                if (cw <= first)
                {
                    msg.Add(string.Format("({0}{1}){2}KG<=首重{3}KG", price, Helper.GetLoalCurName(cur), cw, first));
                    curTotal = price;
                    //cw = first;
                }
                else
                {
                    decimal cw_mode = (cw - first) % pre;
                    if (cw_mode > 0) cw_mode = pre - cw_mode;
                    int cw_0 = (int)((cw - first + cw_mode) / pre);

                    msg.Add(string.Format("首重{0}KG({1}{2})", first, price, Helper.GetLoalCurName(cur)));// 
                    msg.Add(string.Format("继重{0}{1}/{2}kg", price_more, Helper.GetLoalCurName(cur), pre));// 
                    msg.Add(string.Format("继重{0}KG({1}*{2}{3})", (cw - first + cw_mode), cw_0, price_more, Helper.GetLoalCurName(cur)));// 
                    curTotal = price + price_more * cw_0;
                    //cw = pre * cw_0 + first;
                }
                dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                #endregion

                msg.Add(string.Format("总费用{0}{1}", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur)));
                if (error)
                    msg.Add("无法计算本地CNY费用");
                else if (curTotal != total)
                {
                    dr["C_FLAG"] = "Y";
                    dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                    msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                }
                dr["QUNIT_PRICE"] = (cw == 0) ? curTotal : curTotal / cw;
                dr["QQTY"] = (cw == 0) ? 1 : cw;
                dr["QCHG_UNIT"] = Unit.KGS;
                dr["QAMT"] = Helper.Get45AmtValue(curTotal);
                SetChargeInfo(dr, "CF", "D");

                _qcount++;
                if (_topOne)
                    elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));


                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                schems["方案" + index + "_remark"] = string.Join(";", msg);

                WriteLog(index, Prolink.Math.GetValueAsString(dr["QUOT_NO"]), msg, total);
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }

                dr["CHG_REMARK"] = string.Join("；", msg);
                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// 国际快递 运费试算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private Dictionary<string, object> ExpressFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            Helper.AddOthColumns(dt);
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = null;
            int index = 0;
            decimal cw = 0;
            decimal qty = 0;
            if (parm.ContainsKey("nw"))
                cw = Helper.GetDecimalValue(parm["nw"]);
            if (cw <= 0)
            {
                qty = Helper.GetQty("KG", parm);
            }                       
            decimal pre_total = 0m;
            string billDate = "";
            if (parm.ContainsKey("billDate"))
                billDate = parm["billDate"].ToString();
            foreach (DataRow dr in dt.Rows)
            {
                string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                if ("FRT" != chgCd) continue;
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                string lspCd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);

                //191519
                string sql = string.Format(@"SELECT BW,PCW FROM SMBWS WHERE TRAN_TYPE={0} AND LSP_CD={1}", SQLUtils.QuotedStr("E"), SQLUtils.QuotedStr(lspCd));
                DataTable smbwsDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                decimal bw = 0;
                int limit = 30;
                if (smbwsDt.Rows.Count > 0)
                {
                    bw = Prolink.Math.GetValueAsDecimal(smbwsDt.Rows[0]["BW"]);
                    limit = Prolink.Math.GetValueAsInt(smbwsDt.Rows[0]["PCW"]);
                } 
                if (parm.ContainsKey("nw"))
                {
                    cw = Helper.GetDecimalValue(parm["nw"]);
                }
                if (bw > 0)
                {                                           
                    if (cw <= 0)
                    {
                        cw = Helper.GetCW(qty, Helper.GetDecimalValue(parm["cbm"]), 5000M, "E",bw);                      
                    }
                    else
                    {
                        int cw1 = (int)cw;
                        if (cw > bw)
                        {
                            if ((cw - cw1) > 0)
                                cw = cw1 + 1;
                        }
                        else if (cw < bw)
                        {
                            if ((cw - cw1) > 0.5m)
                                cw = cw1 + 1;
                            else
                                cw = cw1 + 0.5m;
                        }
                    }
                        
                }
                else
                {                    
                    if (cw <= 0)
                    {
                        cw = Helper.GetCW(qty, Helper.GetDecimalValue(parm["cbm"]), 5000M, "E");
                    }
                    cw = Helper.GetEexpressGw(cw);
                }                
                //end

                Dictionary<string, decimal> map = new Dictionary<string, decimal>();

                for (int i = 11; i <= 70; i++)
                {
                    map["F" + i] = -(0.5m + (i - 11) * 0.5m);
                }
 
                switch (limit)
                {
                    case 30:
                        //map["F1"] = -30;
                        map["F7"] = 300;
                        map["F6"] = 200;
                        map["F5"] = 100;
                        map["F4"] = 50;
                        map["F3"] = 40;
                        map["F2"] = 30;
                        break;
                    default:
                        map["F7"] = 300;
                        map["F6"] = 200;
                        map["F5"] = 100;
                        map["F4"] = 50;
                        map["F3"] = 40;
                        map["F2"] = 30;
                        map["F1"] = 20;
                        break;
                }
                map = Helper.GetEcreffee("E", lspCd, map);
                index++;
                elist = GetEiList(schems, index);
                List<string> msg = new List<string>();
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);//报价号码
                msg.Add(string.Format("报价号:{0}", quot_no));
                decimal total = 0M;
                decimal curTotal = 0M;
                bool error = false;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                //decimal min = Prolink.Math.GetValueAsDecimal(dr["MIN_AMT"]);
                decimal price = 0m;
                foreach (var kv in map)
                {
                    decimal val = kv.Value;
                    if (val < 0)
                    {
                        val = System.Math.Abs(val);
                        if (cw <= val)
                        {
                            price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                            if (price != 0)
                            {
                                msg.Add(string.Format("{0}<={1}({2}{3}){4}", cw, val, price, Helper.GetLoalCurName(cur), cw));
                                break;
                            }
                        }
                    }
                    else
                    {
                        if ((val == limit && cw > val) || cw > val)
                        {
                            price = Prolink.Math.GetValueAsDecimal(dr[kv.Key]);
                            if (price != 0)
                            {
                                msg.Add(string.Format("{0}>={1}({2}{3}){4}", cw, val, price, Helper.GetLoalCurName(cur), cw));
                                break;
                            }
                        }
                    }
                }

                if (price <= 0)
                    msg.Add("无对应的报价");
                if (cw <= limit)
                    curTotal = price;
                else
                {                    
                    curTotal = price * cw;
                }

                dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format("总费用{0}{1}", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur)));
                if (error)
                    msg.Add("无法计算本地CNY费用");
                else
                {
                    dr["C_FLAG"] = "Y";
                    dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                    msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                }

                SetChargeInfo(dr, "CF", "E");
                dr["QCHG_UNIT"] = "SET";
                dr["QUNIT_PRICE"] = curTotal;
                dr["QQTY"] = 1;
                dr["QAMT"] = curTotal;
                _qcount++;
                if (_topOne)
                    elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno)); 
                ExpressOtherCharge(elist, rateDt, quot_no, billDate, cw, parm, dr, curTotal);
                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                schems["方案" + index + "_remark"] = string.Join(";", msg);
                WriteLog(index, Prolink.Math.GetValueAsString(dr["QUOT_NO"]), msg, total);
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }

                dr["CHG_REMARK"] = string.Join("；", msg);
                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        private void ExpressOtherCharge(List<EditInstruct> elist, DataTable rateDt, string quotNo, string date, decimal cw, Dictionary<string, object> parm, DataRow fcDr, decimal cfTotal)
        {
            decimal oscPrice = 0;
            decimal owcPrice = 0;
            decimal essPrice = 0;
            decimal oscLimit = 0;
            decimal owcLimit = 0;
            DataRow oscDr = null;
            DataRow owcDr = null;
            DataRow essDr = null;
            string lspCd = string.Empty;
            string region = "";

            if (parm.ContainsKey("smsmpod"))
            {
                string smsmpod = parm["smsmpod"].ToString();
                region = smsmpod.Length >= 2 ? smsmpod.Substring(0, 2) : smsmpod;
            }
            string sql = string.Format(@"SELECT * FROM SMQTD WHERE QUOT_NO={0} AND CHG_CD!='FRT'", SQLUtils.QuotedStr(quotNo));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return;
            Helper.AddOthColumns(dt);
            DataRow[] oscRows = dt.Select(string.Format(@"CHG_CD={0}", SQLUtils.QuotedStr(OSC)));
            DataRow[] owcRows = dt.Select(string.Format(@"CHG_CD={0}", SQLUtils.QuotedStr(OWC)));
            DataRow[] essRows = dt.Select(string.Format(@"CHG_CD={0} AND REGION={1}", SQLUtils.QuotedStr(ESS), SQLUtils.QuotedStr(region)));

            List<string> regionChgCd = dt.AsEnumerable()
            .Where(row => row.Field<string>("CHG_CD") != OSC && row.Field<string>("CHG_CD") != OWC && row.Field<string>("CHG_CD") != ESS && row.Field<string>("REGION") == region)
            .Select(row => row.Field<string>("CHG_CD"))
            .Distinct()
            .ToList();

            List<string> noRegionChgCd = dt.AsEnumerable()
                        .Where(row => row.Field<string>("CHG_CD") != OSC && row.Field<string>("CHG_CD") != OWC && row.Field<string>("CHG_CD") != ESS && row.Field<string>("REGION") == null)
                        .Select(row => row.Field<string>("CHG_CD"))
                        .Distinct()
                        .ToList();

            // 获取在noRegionChgCd中而不在regionChgCd中的值
            noRegionChgCd = noRegionChgCd.Except(regionChgCd).ToList();


            if (oscRows.Length > 0)
            {
                oscDr = oscRows[0];
                lspCd = Prolink.Math.GetValueAsString(oscDr["LSP_CD"]);
                oscPrice = Prolink.Math.GetValueAsDecimal(oscDr["F1"]);
                oscLimit = Prolink.Math.GetValueAsDecimal(oscDr["LIMIT_SIZE"]);
            }
            if (owcRows.Length > 0)
            {
                owcDr = owcRows[0];
                lspCd = Prolink.Math.GetValueAsString(owcDr["LSP_CD"]);
                owcPrice = Prolink.Math.GetValueAsDecimal(owcDr["F1"]);
                owcLimit = Prolink.Math.GetValueAsDecimal(owcDr["LIMIT_SIZE"]);
            }
            if (essRows.Length > 0)
            {
                essDr = essRows[0];
                lspCd = Prolink.Math.GetValueAsString(essDr["LSP_CD"]);
                essPrice = Prolink.Math.GetValueAsDecimal(essDr["F1"]);
            }


            string punit = "";
            decimal otherPrice = 0;
            decimal cbm = Helper.GetDecimalValue(parm["cbm"]);
            decimal otherTotal = 0;
            decimal min = 0;
            decimal max = 0;
            string addFsc = "";
            decimal qty = 0;
            decimal vatRate = 0;
            decimal fsc = Helper.GetFsc(lspCd, date);
            DataRow otherRow = null;
            Func<string, bool, string> initData = (ChgCd, flag) =>
            {
                decimal total = 0;
                otherTotal = 0;
                DataRow[] otherRows = null;
                if (flag)
                    otherRows = dt.Select(string.Format(@"CHG_CD={0} AND REGION={1}", SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(region)));
                else
                    otherRows = dt.Select(string.Format(@"CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)));
                otherRow = otherRows[0];
                punit = Prolink.Math.GetValueAsString(otherRow["PUNIT"]);
                otherPrice = Prolink.Math.GetValueAsDecimal(otherRow["F1"]);
                vatRate = Prolink.Math.GetValueAsDecimal(otherRow["VAT_RATE"]);
                min = Prolink.Math.GetValueAsDecimal(otherRow["MIN_AMT"]);
                max = Prolink.Math.GetValueAsDecimal(otherRow["MAX_AMT"]);
                addFsc = Prolink.Math.GetValueAsString(otherRow["ADD_FSC"]);

                qty = Helper.GetQty(punit, parm);
                total = qty * otherPrice;
                if ("%".Equals(punit))
                {
                    total = qty * (vatRate / 100);
                }
                if (min > 0 && total <= min)
                {
                    total = min;
                }
                else if (max > 0 && total >= max)
                {
                    total = max;
                }
                if (addFsc.Equals("Y"))
                    otherTotal = total * (1 + fsc);
                else
                    otherTotal = total;
                return "";
            };

            foreach (string chgCd in regionChgCd)
            {
                initData(chgCd, true);
                ExpressOtherCharge(elist, rateDt, otherRow, otherTotal, chgCd);
            }
            foreach (string chgCd in noRegionChgCd)
            {
                initData(chgCd, false);
                ExpressOtherCharge(elist, rateDt, otherRow, otherTotal, chgCd);
            }

            decimal overLW = 0;
            decimal sbwNum = 0;
            decimal oscTotal = 0;
            decimal sbwTotal = 0;
            string shipmentId = "";
            if (parm.ContainsKey("ShipmentId"))
                shipmentId = parm["ShipmentId"].ToString();

            sql = string.Format("SELECT * FROM SMICUFT WHERE DN_NO IN(SELECT DISTINCT DN_NO FROM SMIDNP WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(shipmentId));
            DataTable smcuftDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow smcuftRow in smcuftDt.Rows)
            {
                decimal w = Prolink.Math.GetValueAsDecimal(smcuftRow["W"]);
                decimal h = Prolink.Math.GetValueAsDecimal(smcuftRow["H"]);
                decimal l = Prolink.Math.GetValueAsDecimal(smcuftRow["L"]);
                decimal sbw = Prolink.Math.GetValueAsDecimal(smcuftRow["SBW"]);
                decimal num = Prolink.Math.GetValueAsDecimal(smcuftRow["PKG"]); 
                if (w > oscLimit || h > oscLimit || l > oscLimit)
                {
                    overLW += num;
                }
                if (sbw > owcLimit)
                    sbwNum += num;
            }
            if (owcPrice > 0 && sbwNum > 0)
            {
                sbwTotal = owcPrice * sbwNum;
                ExpressOtherCharge(elist, rateDt, owcDr, sbwTotal, OWC);//超重
            }
            else if (oscPrice > 0 && overLW > 0)
            {
                oscTotal = oscPrice * overLW;
                ExpressOtherCharge(elist, rateDt, oscDr, oscTotal, OSC);//超规格
            }
            if (essPrice > 0)
            {
                decimal curTotal = Helper.GetEexpressGw(cw) * essPrice * (1 + fsc);
                if (curTotal <= 0) return;
                ExpressOtherCharge(elist, rateDt, essDr, curTotal, ESS);//旺季附加费
            }

            decimal fscTotal = (cfTotal + oscTotal + sbwTotal) * fsc;
            ExpressOtherCharge(elist, rateDt, fcDr, fscTotal, FSC);//燃油附加费
        }

        private void ExpressOtherCharge(List<EditInstruct> elist, DataTable rateDt, DataRow dr, decimal curTotal, string chgcd)
        {
            string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
            string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);

            List<string> msg = new List<string>();
            Dictionary<string, object> schems = GetSchems();
            msg.Add(string.Format("报价号:{0}", quot_no));
            bool error = false;
            decimal total = 0M;
            dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
            msg.Add(string.Format("{2}总费用{0}{1}", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur), chgcd));
            if (error)
                msg.Add("无法计算本地CNY费用");
            else
            {
                dr["C_FLAG"] = "Y";
                dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
            }

            SetChargeInfo(dr, chgcd, "E", false, true);//附加费调整
            dr["QCHG_UNIT"] = "SET";
            dr["QUNIT_PRICE"] = curTotal;
            dr["QQTY"] = 1;
            dr["QAMT"] = curTotal;
            _qcount++;
            if (_topOne)
                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
        }


        /// <summary>
        /// 内陆运输
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="dt"></param>
        /// <param name="othDt"></param>
        /// <param name="rateDt"></param>
        private Dictionary<string, object> TruckFreight(Dictionary<string, object> parm, DataTable dt, DataTable othDt, DataTable rateDt)
        {
            Helper.AddOthColumns(dt);
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = null;
            int index = 0;
            string CargoType = string.Empty;
            string bandType = string.Empty;
            decimal qty = 0M;
            if (parm.ContainsKey("CargoType"))
                CargoType = Prolink.Math.GetValueAsString(parm["CargoType"]);
            if (parm.ContainsKey("bandType"))
                bandType = Prolink.Math.GetValueAsString(parm["bandType"]);
            if (parm.ContainsKey("StdQty"))
                qty = Helper.GetDecimalValue(parm["StdQty"]);
            decimal CntrStdQty = parm.ContainsKey("CntrStdQty") ? Prolink.Math.GetValueAsDecimal(parm["CntrStdQty"]) : 0M;
            string trackWay = Prolink.Math.GetValueAsString(parm["TrackWay"]);
            decimal kggw = Helper.GetQty("KG", parm);
            decimal gw = kggw / 1000m;//按吨报价
            decimal cbm = Helper.GetDecimalValue(parm["cbm"]);
            //decimal cw = Helper.GetCW(kggw, cbm, 6000M, "A");
            decimal cw = 0;
            if (parm.ContainsKey("nw"))
                cw = Helper.GetDecimalValue(parm["nw"]);
            DataTable dnDt = parm.ContainsKey("DnQtyDt") ? (parm["DnQtyDt"] as DataTable) : null;
            if (dnDt == null) dnDt = new DataTable();

            decimal pre_total = 0m;
            Dictionary<string, string> cts = new Dictionary<string, string> { { "car_cw", "carType" }, { "car_cw1", "carType1" }, { "car_cw2", "carType2" } };
            foreach (DataRow dr0 in dt.Rows)
            {
                DataRow dr = dr0;
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
                List<string> msg = new List<string>();

                index++;
                elist = GetEiList(schems, index);
                decimal total = 0m;
                decimal cbm_total = 0m;
                decimal gw_total = 0m;
                decimal curTotal = 0m;
                string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                if (string.IsNullOrEmpty(cur))
                    cur = Prolink.Math.GetValueAsString(dr["M_CUR"]).ToUpper();

                bool error = false;

                #region 车型计费
                if (string.IsNullOrEmpty(trackWay) || "F".Equals(trackWay))//专车才计价
                {
                    WriteLog("专车报价:" + quot_no);
                    List<string> carList = new List<string>();
                    Dictionary<string, decimal> cars = new Dictionary<string, decimal>();
                    foreach (var ct in cts)
                    {
                        if (!parm.ContainsKey(ct.Key))
                            continue;
                        decimal car_cw = Helper.GetDecimalValue(parm[ct.Key]);
                        string carType = Prolink.Math.GetValueAsString(parm[ct.Value]);
                        if (string.IsNullOrEmpty(carType))
                            continue;

                        if (dr.Table.Columns.Contains(carType))
                        {
                            //if (car_cw <= 0) car_cw = 1;
                            if (car_cw <= 0)
                                continue;
                            decimal price = Prolink.Math.GetValueAsDecimal(dr[carType]);
                            msg.Add(string.Format("车型:{0}对应的报价{1}{2},数量:{3}", Helper.GetCarName(carType), price, cur, car_cw));
                            if (!cars.ContainsKey(carType))
                                cars[carType] = 0M;
                            carList.Add(string.Format("({0}:{1}{2}*{3})", Helper.GetCarName(carType), price, cur, car_cw));
                            //if (price <= 0)
                            //    msg.Add(string.Format("车型:{0}无对应的报价", Helper.GetCarName(carType)));
                            decimal temp = price * car_cw;
                            curTotal += temp;
                            msg.Add(string.Format("({0}{1})*{2}", price, Helper.GetLoalCurName(cur), car_cw));
                        }
                        else
                        {
                            msg.Add(string.Format("车型:{0}无对应的报价", Helper.GetCarName(carType)));
                        }
                        //decimal temp1 = Helper.GetRateAmt(rateDt, temp, cur, _localCur, ref error, msg);
                    }

                    if (curTotal > 0)
                    {
                        decimal temp1 = 0;
                        dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, curTotal, cur, ref temp1, ref error, _localCur);
                        total += temp1;
                        dr["LOCALE_AMT"] = temp1;
                        SetChargeInfo(dr, "DF", "T");
                        dr["QUNIT_PRICE"] = curTotal;
                        dr["QQTY"] = 1;
                        dr["QCHG_UNIT"] = "SET";
                        dr["QAMT"] = curTotal;
                        dr["EX_REMARK"] = string.Join("+", carList);
                        if (_topOne)
                            elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
                    }
                }
                #endregion

                #region 空运计费
                if ("A".Equals(trackWay))//专车才计价
                {
                    decimal price = Prolink.Math.GetValueAsDecimal(dr["F10"]);
                    decimal temp = price * cw;
                    curTotal += temp;
                    decimal temp1 = 0;
                    dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                    total += temp1;
                    dr["LOCALE_AMT"] = temp1;
                    SetChargeInfo(dr, "DF", "T");
                    dr["QUNIT_PRICE"] = price;
                    dr["QQTY"] = cw;
                    dr["QCHG_UNIT"] = "CW";
                    dr["QAMT"] = temp;
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
                    msg.Add(string.Format("计费重:{0}*{1}={2}{3}", cw, price, temp, Helper.GetLoalCurName(cur)));
                }
                #endregion

                #region 计算材积重
                List<string> test = new List<string>();
                if (!"F".Equals(trackWay) && !"A".Equals(trackWay))//非专车才计价
                {
                    WriteLog("非专车报价:" + quot_no);
                    DataTable dnpDt = parm.ContainsKey("CntrStdDt") ? (parm["CntrStdDt"] as DataTable) : null;
                    if (dnpDt == null) dnpDt = new DataTable();
                    dr["CNTR_STD_QTY"] = 0;
                    dr["IPART_NO"] = string.Empty;
                    dr["DN_NO"] = string.Empty;
                    dr["EX_REMARK"] = "";

                    int mode = 0;//计算模式
                    foreach (DataRow dnp in dnpDt.Rows)
                    {
                        string dn_no = dnp.Table.Columns.Contains("DN_NO") ? Prolink.Math.GetValueAsString(dnp["DN_NO"]) : string.Empty;
                        if (test.Contains(dn_no))
                            continue;
                        decimal cntr_std_qty = Prolink.Math.GetValueAsDecimal(dnp["CNTR_STD_QTY"]);
                        if (cntr_std_qty <= 0)
                            continue;
                        test.Add(dn_no);
                        DataRow[] dnps = dnp.Table.Columns.Contains("DN_NO") ? dnpDt.Select(string.Format("DN_NO={0}", SQLUtils.QuotedStr(dn_no))) : dnpDt.Select();
                        foreach (DataRow dnp0 in dnps)
                        {

                            if (_topOne)
                            {
                                #region 修正交期问题单 问题单：109465  add by fish 2016/9/23
                                dr = SetTruckData(dt, dr0, dr);
                                #endregion
                            }


                            decimal price = Prolink.Math.GetValueAsDecimal(dr["F9"]);
                            if (price <= 0)
                                price = Prolink.Math.GetValueAsDecimal(dr["F7"]);

                            decimal val1 = Prolink.Math.GetValueAsDecimal(dnp0["QTY"]);
                            decimal val2 = Prolink.Math.GetValueAsDecimal(dnp0["CNTR_STD_QTY"]);
                            if (val2 <= 0)
                                continue;

                            dr["EX_REMARK"] = "整車費用:" + price;
                            price = price / val2;
                            cbm_total = price * val1;
                            dr["CNTR_STD_QTY"] = val2;
                            dr["IPART_NO"] = Prolink.Math.GetValueAsString(dnp0["IPART_NO"]);
                            dr["DN_NO"] = Prolink.Math.GetValueAsString(dnp0["DN_NO"]);

                            curTotal += cbm_total;
                            decimal temp1 = 0M;
                            dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cbm_total, cur, ref temp1, ref error, _localCur);
                            total += temp1;
                            dr["LOCALE_AMT"] = temp1;
                            dr["QUNIT_PRICE"] = price;
                            dr["QQTY"] = val1;
                            dr["QCHG_UNIT"] = "SET";
                            dr["QAMT"] = cbm_total;
                            SetChargeInfo(dr, "DF", "T");

                            _qcount++;
                            if (_topOne)
                            {
                                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
                                mode++;
                            }
                            msg.Add(string.Format("零担成品:{0}*{1}={2}{3}", price, val1, cbm_total, Helper.GetLoalCurName(cur)));
                        }
                    }

                    if (mode <= 0 && dnDt.Rows.Count > 0)
                    {
                        kggw = Helper.GetQty("KG", parm);
                        gw = kggw / 1000m;//按吨报价
                        cbm = Helper.GetDecimalValue(parm["cbm"]);
                        #region 修正交期问题单 问题单：109465  add by fish 2016/9/23
                        dr = SetTruckData(dt, dr0, dr, 1);
                        #endregion

                        dr["LOCALE_AMT"] = 0;
                        dr["QQTY"] = 0;
                        dr["QAMT"] = 0;
                        bool isCbm = CalTruck(rateDt, elist, CargoType, bandType, trackWay, gw, cbm, dr, string.Empty, false);
                        decimal locale_amt_tt = Prolink.Math.GetValueAsDecimal(dr["LOCALE_AMT"]);
                        decimal qqty_tt = Prolink.Math.GetValueAsDecimal(dr["QQTY"]);
                        decimal qamt_tt = Prolink.Math.GetValueAsDecimal(dr["QAMT"]);
                        curTotal += qamt_tt;
                        total += locale_amt_tt;
                        decimal dn_qty_tt = 0M;

                        foreach (DataRow dn in dnDt.Rows)
                        {
                            dn_qty_tt += GetDBQty(isCbm, dn);
                        }

                        foreach (DataRow dn in dnDt.Rows)
                        {
                            string dn_no = Prolink.Math.GetValueAsString(dn["DN_NO"]);
                            if (test.Contains(dn_no))
                                continue;
                            test.Add(dn_no);
                            DataRow[] dns = dnDt.Select(string.Format("DN_NO={0}", SQLUtils.QuotedStr(dn_no)));
                            dr["IPART_NO"] = dn_no;
                            dr["DN_NO"] = dn_no;
                            decimal dn_qty = 0M;
                            foreach (DataRow dn0 in dns)
                            {
                                dn_qty += GetDBQty(isCbm, dn);
                            }

                            dr["EX_REMARK"] = isCbm ? string.Format("CMB:{0}(合计{1})立方米,Shipment CBM:{2},{3},{4}", dn_qty, dn_qty_tt, cbm, qamt_tt, locale_amt_tt) : string.Format("GW:{0}(合计{1})吨,Shipment GW:{2},{3},{4}", dn_qty, dn_qty_tt, gw, qamt_tt, locale_amt_tt);

                            dr["LOCALE_AMT"] = dn_qty_tt > 0 ? locale_amt_tt * dn_qty / dn_qty_tt : 0M;
                            dr["QQTY"] = dn_qty_tt > 0 ? qqty_tt * dn_qty / dn_qty_tt : 0M;
                            dr["QAMT"] = dn_qty_tt > 0 ? qamt_tt * dn_qty / dn_qty_tt : 0M;

                            _qcount++;
                            if (_topOne)
                            {
                                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
                                mode++;
                            }
                        }
                    }

                    #region  没有DN就按正常方式计算
                    if (mode <= 0 && _topOne)
                    {
                        kggw = Helper.GetQty("KG", parm);
                        gw = kggw / 1000m;//按吨报价
                        cbm = Helper.GetDecimalValue(parm["cbm"]);
                        #region 修正交期问题单 问题单：109465  add by fish 2016/9/23
                        dr = SetTruckData(dt, dr0, dr, 1);
                        #endregion
                        CalTruck(rateDt, elist, CargoType, bandType, trackWay, gw, cbm, dr, string.Empty, true);
                    }
                    #endregion
                }
                #endregion

                //Helper.GetTotal(rateDt, msg, curTotal, cur, ref total, ref error);
                msg.Add(string.Format("总费用{0}{1}", Helper.Get45AmtValue(curTotal), Helper.GetLoalCurName(cur)));
                if (error)
                    msg.Add("无法计算本地CNY费用");
                else if (curTotal != total)
                {
                    dr["LOCALE_AMT"] = Helper.Get45AmtValue(total);
                    msg.Add(string.Format("本地CNY费用{0}{1}", Helper.Get45AmtValue(total), Helper.GetLoalCurName(_localCur)));
                }
                dr["CHG_REMARK"] = string.Join("；", msg);

                total = Helper.Get45AmtValue(total);
                schems["方案" + index] = elist;
                schems["方案" + index + "_amt"] = total;
                schems["方案" + index + "_remark"] = string.Join(";", msg);
                WriteLog(index, Prolink.Math.GetValueAsString(dr["QUOT_NO"]), msg, total);
                if (pre_total <= 0 || total < pre_total)
                {
                    schems["方案"] = index;
                    pre_total = total;
                }
                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        private static List<EditInstruct> GetEiList(Dictionary<string, object> schems, int index)
        {
            List<EditInstruct> elist = null;
            if (schems.ContainsKey("方案" + index))
                elist = schems["方案" + index] as List<EditInstruct>;
            if (elist == null)
                elist = new List<EditInstruct>();
            return elist;
        }


        /// <summary>
        /// 创建账单明细
        /// </summary>
        /// <param name="smsm"></param>
        /// <param name="qt"></param>
        /// <param name="debit_no"></param>
        /// <returns></returns>
        private EditInstruct CreateBillItem(DataRow smsm, DataRow qt, string debit_no, string lsp_no = "", string lsp_nm = "", bool share = false, bool isFreight = true)
        {
            EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);

            if (!string.IsNullOrEmpty(lsp_no))
            {
                ei.Put("LSP_NO", lsp_no);
                ei.Put("HAS_CREDIT_TO", "Y");
            }
            if (!string.IsNullOrEmpty(lsp_nm))
                ei.Put("LSP_NM", lsp_nm);
            //ei.Put("U_ID", smsm["U_ID"]);
            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("QUOT_ID", qt["U_ID"]);//报价主键
            ei.Put("QUOT_NO", qt["QUOT_NO"]);
            if (share)
                ei.Put("IS_SHARE", "Y");
            else if (qt.Table.Columns.Contains("IS_SHARE"))
                ei.Put("IS_SHARE", qt["IS_SHARE"]);
            string rfq_no = Prolink.Math.GetValueAsString(qt["RFQ_NO"]);
            if ("undefined".Equals(rfq_no) || "null".Equals(rfq_no) || string.IsNullOrEmpty(rfq_no))
                rfq_no = string.Empty;

            ei.Put("RFQ_NO", rfq_no);
            ei.Put("SHIPMENT_ID", smsm["SHIPMENT_ID"]);
            //ei.Put("DEBIT_NO", debit_no);
            //ei.Put("U_FID", debit_no);
            //if (qt.Table.Columns.Contains("LSP_CD"))
            //    ei.Put("LSP_NO", qt["LSP_CD"]);
            ei.Put("BILL_TO", smsm["CMP"]);//付款者

            //2016/8/18新增
            ei.Put("CMP", smsm["CMP"]);
            ei.Put("TRAN_TYPE", smsm["TRAN_TYPE"]);
            ei.Put("GROUP_ID", smsm["GROUP_ID"]);

            ei.Put("BL_NO", Helper.GetValueAsString(smsm, new string[] { "HOUSE_NO", "MASTER_NO" }));

            //ei.Put("STATUS ", "Y");//符合 
            ei.Put("CHG_CD", GetQTValue(qt, "QCHG_CD", "CHG_CD"));
            ei.Put("CHG_DESCP", GetQTValue(qt, "QCHG_DESCP", "CHG_DESCP"));
            ei.Put("CHG_TYPE", GetQTValue(qt, "QCHG_TYPE", "CHG_TYPE"));
            ei.Put("REPAY", qt["REPAY"]);

            string chg_type = ei.Get("CHG_TYPE");
            string my_debit_no = string.Empty;
            string my_debit_nm = string.Empty;
            if (isFreight)
            {
                switch (chg_type)
                {
                    case "F":
                    case "D":
                        my_debit_no = smsm.Table.Columns.Contains("DEBIT_TO") ? Prolink.Math.GetValueAsString(smsm["DEBIT_TO"]) : string.Empty;
                        my_debit_nm = smsm.Table.Columns.Contains("DEBIT_NM") ? Prolink.Math.GetValueAsString(smsm["DEBIT_NM"]) : string.Empty;
                        break;
                }
            }
            if (string.IsNullOrEmpty(my_debit_no) && _partySH != null && _partySH.Length > 0)
            {
                my_debit_no = Prolink.Math.GetValueAsString(_partySH[0]["PARTY_NO"]);
                my_debit_nm = Prolink.Math.GetValueAsString(_partySH[0]["PARTY_NAME"]);
            }
            ei.Put("DEBIT_TO", my_debit_no);
            ei.Put("DEBIT_NM", my_debit_nm);

            if (string.IsNullOrEmpty(ei.Get("CHG_DESCP")) && "FRT".Equals(ei.Get("CHG_CD")))
                ei.Put("CHG_DESCP", "Freight charge");
            string qcur = Prolink.Math.GetValueAsString(qt["CUR"]);
            ei.Put("QCUR", qcur);//從報價帶出的預提

            ei.Put("QLCUR", _localCur);

            if (qcur==_localCur)
            {
                ei.Put("QLRATE", 1);
            }

            ei.Put("QUNIT_PRICE", qt["QUNIT_PRICE"]);//從報價帶出的預提
            ei.Put("QCHG_UNIT", qt["QCHG_UNIT"]);//從報價帶出的預提
            ei.Put("QQTY", qt["QQTY"]);//报价数量  "從報價帶出的預提chg_unit如果為K, 就放SMSM.CW, 如果為CBM 就放SMSM.cbm 如果為SET 就放 1如果為CTN 就放SMSM.qty"
            ei.Put("QAMT", qt["QAMT"]);//"從報價帶出的預提 Unit_price X QTY"
            ei.Put("QLAMT", qt["LOCALE_AMT"]);
            ei.Put("QEX_RATE", qt["QEX_RATE"]);

            //ei.Put("CUR", qt["CUR"]);
            ei.Put("REMARK", qt["EX_REMARK"]);
            //ei.Put("UNIT_PRICE", qt["QUNIT_PRICE"]);
            //ei.Put("CHG_UNIT", qt["QCHG_UNIT"]);
            //ei.Put("QTY", qt["QQTY"]);
            ei.Put("QTAX", qt["QTAX"]);//预提税率
            ei.Put("TAX", qt["QTAX"]);//请款税率
            //ei.Put("BAMT", qt["BAMT"]);//物流業者填寫
            //string chg_cd = Prolink.Math.GetValueAsString(qt["CHG_CD"]);
            //if (string.IsNullOrEmpty(chg_cd)) chg_cd = "FRT";

            if (qt.Table.Columns.Contains("CNTR_STD_QTY"))
                ei.Put("CNTR_STD_QTY", qt["CNTR_STD_QTY"]);
            if (qt.Table.Columns.Contains("IPART_NO"))
                ei.Put("IPART_NO", qt["IPART_NO"]);
            if (qt.Table.Columns.Contains("DN_NO"))
                ei.Put("DN_NO", qt["DN_NO"]);
            string decNo = smsm.Table.Columns.Contains("BAT_NO") ? Prolink.Math.GetValueAsString(smsm["BAT_NO"]) : string.Empty;//SMSMI.BAT_NO 运输批次号码
            if ("DF".Equals(ei.Get("CHG_CD")) && !string.IsNullOrEmpty(decNo))//内贸运费
            {
                ei.Put("DEC_NO", decNo);
            }
            ei.Put("MASTER_NO", smsm["MASTER_NO"]);
            ei.Put("CNTR_INFO", smsm["CNTR_INFO"]);
            ei.Put("POD_CD", smsm["POD_CD"]);
            ei.Put("SEC_CMP", smsm["SEC_CMP"]);
            ei.Put("INVOICE_INFO", smsm["INVOICE_INFO"]);
            return ei;
        }


        /// <summary>
        /// 获取已创建的报价
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetSchems()
        {
            Dictionary<string, object> schems = null;
            if (!string.IsNullOrEmpty(_current_debitno) && _qt_schems.ContainsKey(_current_debitno))
                schems = _qt_schems[_current_debitno] as Dictionary<string, object>;
            if (schems == null)
                schems = new Dictionary<string, object>();
            return schems;
        }

        /// <summary>
        /// 设置费用信息
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="chgCd"></param>
        /// <param name="tranMode"></param>
        private void SetChargeInfo(DataRow dr, string chgCd, string tranMode, bool setType = false, bool defaultChgCd = false)
        {

            if (_chgDt == null)
                return;
            string chgCd1 = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
            if ("FRT".Equals(chgCd1) || defaultChgCd)
            {
                dr["CHG_CD"] = chgCd; 

                DataRow[] drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr(tranMode)));
                if (drs.Length <= 0)
                    drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr("O")));
                if (drs.Length <= 0)
                    drs = _chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(chgCd)));
                if (drs.Length > 0)
                {
                    dr["QCHG_DESCP"] = drs[0]["CHG_DESCP"];
                    dr["QTAX"] = drs[0]["VAT_RATE"];
                    dr["REPAY"] = drs[0]["REPAY"];
                    if (setType)
                        dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                    else if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["CHG_TYPE"])))
                        dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                    else
                        dr["QCHG_TYPE"] = dr["CHG_TYPE"];
                }
            }
            else
            {
                dr["QCHG_DESCP"] = dr["CHG_DESCP"];
                dr["QTAX"] = dr["VAT_RATE"];
                dr["REPAY"] = dr["REPAY"];
                dr["QCHG_TYPE"] = dr["CHG_TYPE"];
            }
            
        }


        /// <summary>
        /// 记录log
        /// </summary>
        /// <param name="index"></param>
        /// <param name="quot_no"></param>
        /// <param name="msg"></param>
        /// <param name="total"></param>
        private void WriteLog(int index, string quot_no, List<string> msg, decimal total)
        {
            try
            {
                if (string.IsNullOrEmpty(_shipment_id))
                    return;
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("BillLog");
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, _shipment_id + ".txt");
                System.IO.File.AppendAllText(path, System.Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + string.Format("方案{0}:{1},报价:{2}", index, total, quot_no), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + string.Join(System.Environment.NewLine, msg), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + "-------------------------------------", Encoding.UTF8);
            }
            catch { }
        }

        public static void WriteLogTagStart(string tag, string shipmentId = "")
        {
            string _shipmentId = shipmentId;
            if (string.IsNullOrEmpty(shipmentId))
                _shipmentId = _shipment_id;
            try
            {
                if (string.IsNullOrEmpty(_shipmentId))
                    return;
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("BillLog");
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, _shipmentId + ".txt");
                System.IO.File.AppendAllText(path, System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine, Encoding.UTF8);
                System.IO.File.AppendAllText(path, string.Format("-------------------{0} {1}------------------" + System.Environment.NewLine, tag, DateTime.Now.ToString("yyyy-MM-dd HH:mm:fff")), Encoding.UTF8);
                //System.IO.File.AppendAllText(path, System.Environment.NewLine + "", Encoding.UTF8);
            }
            catch { }
        }

        public static void WriteLog(string messenger, string shipmentId = "")
        {
            string _shipmentId = shipmentId;
            if (string.IsNullOrEmpty(shipmentId))
                _shipmentId = _shipment_id;
            try
            {
                if (string.IsNullOrEmpty(_shipmentId))
                    return;
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("BillLog");
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, _shipmentId + ".txt");
                System.IO.File.AppendAllText(path, System.Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:fff") + "=>", Encoding.UTF8);
                System.IO.File.AppendAllText(path, messenger + System.Environment.NewLine, Encoding.UTF8);
                //System.IO.File.AppendAllText(path, System.Environment.NewLine + "-------------------------------------", Encoding.UTF8);
            }
            catch { }
        }

        private static DataRow SetTruckData(DataTable dt, DataRow dr, DataRow dr0, int type = 0)
        {
            string qt_condition = string.Empty;
            DataRow[] zcs = null;
            if (type == 1)
            {
                qt_condition = string.Format("(F1>0 OR F2>0) AND POL_CD={0} AND POD_CD={1} AND (TRAN_TYPE={2} OR TRAN_TYPE IS NULL) AND U_FID={3}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["TRAN_TYPE"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["U_FID"])));
                zcs = dt.Select(qt_condition);
                if (zcs.Length > 0)
                    dr0 = zcs[0];
                return dr0;
            }
            qt_condition = string.Format("F9>0 AND POL_CD={0} AND POD_CD={1} AND (TRAN_TYPE={2} OR TRAN_TYPE IS NULL) AND U_FID={3}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["TRAN_TYPE"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["U_FID"])));
            zcs = dt.Select(qt_condition);
            if (zcs.Length > 0)
                dr0 = zcs[0];
            else
            {
                qt_condition = string.Format("F7>0 AND POL_CD={0} AND POD_CD={1} AND (TRAN_TYPE={2} OR TRAN_TYPE IS NULL) AND U_FID={3}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POL_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["POD_CD"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["TRAN_TYPE"])), SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(dr["U_FID"])));
                zcs = dt.Select(qt_condition);
                if (zcs.Length > 0)
                    dr0 = zcs[0];
            }
            return dr0;
        }

        private bool CalTruck(DataTable rateDt, List<EditInstruct> elist, string CargoType, string bandType, string trackWay, decimal gw, decimal cbm, DataRow dr, string dn_no, bool add_ei)
        {
            List<string> msg = new List<string>();
            decimal total = 0;
            decimal gw_total = 0;
            decimal cbm_total = 0;
            decimal curTotal = 0;
            int mode = 0;
            bool error = false;
            string cur = string.Empty;
            bool isCbm = true;

            decimal gw_price = Prolink.Math.GetValueAsDecimal(dr["F1"]);
            decimal cbm_price = Prolink.Math.GetValueAsDecimal(dr["F2"]);

            #region 每立方米大于等于333，按吨报价
            //isCbm = cbm_total > 0;

            //isCbm = (cbm_price * cbm) > 0;
            if (cbm > 0 && gw > 0 && gw_price > 0)
            {
                if (gw * 1000M / cbm >= 333)
                {
                    msg.Add(string.Format("重抛:{0}>=333公斤", gw * 1000M / cbm));
                    dr["EX_REMARK"] = "重抛";
                    isCbm = false;
                }
            }
            #endregion
            if (cbm < 0.5m) cbm = 0.5m;
            if (gw < 0.5m) gw = 0.5m;

            cbm_total = cbm_price * cbm;
            gw_total = gw_price * gw;

            #region 按立方报价 按吨报价
            if (isCbm)
            {
                //isCbm = true;
                curTotal += cbm_total;
                decimal temp1 = 0M;
                dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cbm_total, cur, ref temp1, ref error, _localCur);
                total += temp1;
                dr["LOCALE_AMT"] = temp1;
                dr["QUNIT_PRICE"] = cbm_price;
                dr["QQTY"] = cbm;
                dr["QCHG_UNIT"] = "CBM";
                dr["QAMT"] = cbm_total;

                msg.Add(string.Format("CBM({0}*{1}){2}{3}", cbm_price, cbm, cbm_total, Helper.GetLoalCurName(cur))
                    + ">" + string.Format("GW({0}*{1}){2}{3}", gw_price, gw, gw_total, Helper.GetLoalCurName(cur)));
            }
            else
            {
                isCbm = false;
                curTotal += gw_total;
                decimal temp1 = 0M;
                dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, gw_total, cur, ref temp1, ref error, _localCur);
                total += temp1;
                dr["LOCALE_AMT"] = temp1;

                dr["QUNIT_PRICE"] = gw_price;
                dr["QQTY"] = gw;
                dr["QCHG_UNIT"] = "T";
                dr["QAMT"] = gw_total;

                //curTotal += gw_total;
                msg.Add(string.Format("CBM({0}*{1}){2}{3}", cbm_price, cbm, cbm_total, Helper.GetLoalCurName(cur))
             + "<=" + string.Format("GW({0}*{1}){2}{3}", gw_price, gw, gw_total, Helper.GetLoalCurName(cur)));
            }
            #endregion

            _qcount++;
            SetChargeInfo(dr, "DF", "T");
            if (_topOne && add_ei)
            {
                elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno));
                mode++;
            }

            #region 增值费 提货费
            List<string> chgTypeList = new List<string>();
            string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);
            if ("T".Equals(trackWay) && ((isCbm && cbm < 5M) || (!isCbm && gw < 2M)))
            {
                //B.	5立方以下或2吨以下,需另外加收增值服務費
                //如果是繞物流園,抓取增值服務費 費用代碼=DELB 的費用(目的地送货费200元/票)。其他則抓  DELC 的費用代碼 (100/票) 
                string filter = string.Empty;
                if ("Y".Equals(bandType))//判断是否是绕物流园区
                    chgTypeList.Add("DELB");
                else
                    chgTypeList.Add("DELC");
            }

            if ((isCbm && cbm < 5M) || (!isCbm && gw < 2M))
            {
                chgTypeList.Add("PUC");
            }

            if (chgTypeList.Count > 0)
            {
                DataTable chgDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMQTD WHERE U_FID={0} AND CHG_CD IN {1}", SQLUtils.QuotedStr(u_fid), Helper.JoinString(chgTypeList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                Helper.AddOthColumns(chgDt);
                foreach (DataRow chg in chgDt.Rows)
                {
                    cur = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                    if (string.IsNullOrEmpty(cur))
                        cur = Prolink.Math.GetValueAsString(dr["M_CUR"]).ToUpper();

                    decimal price = Prolink.Math.GetValueAsDecimal(chg["F1"]);
                    decimal temp1 = 0M;

                    chg["CUR"] = cur;
                    chg["IPART_NO"] = dn_no;
                    chg["QEX_RATE"] = Helper.GetTotal(rateDt, msg, price, cur, ref temp1, ref error, _localCur);
                    total += temp1;
                    chg["LOCALE_AMT"] = temp1;
                    chg["QUNIT_PRICE"] = price;
                    chg["QQTY"] = 1;
                    //chg["QCHG_UNIT"] = "SET";
                    chg["QAMT"] = price;
                    SetChargeInfo(chg, "", "T");
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, chg, _current_debitno));
                }
            }
            #endregion

            return isCbm;
        }

        private static string GetQTValue(DataRow qt, string qname, string name)
        {
            string val = qt.Table.Columns.Contains(qname) ? Prolink.Math.GetValueAsString(qt[qname]) : string.Empty;
            if (string.IsNullOrEmpty(val))
                val = qt.Table.Columns.Contains(name) ? Prolink.Math.GetValueAsString(qt[name]) : string.Empty;
            return val;
        }

        private static decimal GetDBQty(bool isCbm, DataRow dn)
        {
            decimal dn_qty = 0M;
            decimal dn_cbm = Prolink.Math.GetValueAsDecimal(dn["CBM"]);
            decimal dn_gw = Prolink.Math.GetValueAsDecimal(dn["GW"]);
            string gwu = Prolink.Math.GetValueAsString(dn["GWU"]);
            decimal dn_kggw = Helper.GetKGWeight("KG", dn_gw, ref gwu);
            dn_gw = dn_kggw / 1000m;//按吨报价
            if (isCbm)
                dn_qty = dn_cbm;
            else
                dn_qty = dn_gw;
            return dn_qty;
        }

        public static void SetEamt(EditInstruct ei, bool ispanl = true)
        {
            if (ei.ID != "SMBID") return;
            if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
            {
                if (ei.IsExist("U_ID") && !string.IsNullOrEmpty(ei.Get("U_ID")))
                {
                    string uid = ei.Get("U_ID");
                    DataTable dt = OperationUtils.GetDataTable(string.Format(@"SELECT D.QAMT,D.QTAX,D.QCUR,D.BAMT,D.TAX,D.CUR,
                        D.APPROVE_STATUS,D.U_FID FROM SMBID D LEFT JOIN SMSMI M ON D.SHIPMENT_ID=M.SHIPMENT_ID WHERE D.U_ID={0}",
                        SQLUtils.QuotedStr(uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dt.Rows.Count > 0)
                    {
                        string uFid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_FID"]);
                        if (!string.IsNullOrEmpty(uFid))
                        {
                            ei.Remove("TAX");
                        } 
                    }
                    SetEamtInfo(ei, dt);
                }
            }
            else
            {
                if (!ispanl)
                    ei.Put("TAX", 0);
                SetEamtInfo(ei);
            }
        }

        public static void SetEstInfo(EditInstruct ei)
        {
            SetEamt(ei, false);
        }

        private static decimal SetEamtInfo(EditInstruct ei, DataTable dt = null)
        {
            decimal eamt = Prolink.Math.GetValueAsDecimal(GetColValue(ei, "QAMT", dt));
            decimal etax = Prolink.Math.GetValueAsDecimal(GetColValue(ei, "QTAX", dt));
            string approvestatus = GetColValue(ei, "APPROVE_STATUS", dt);
            string cur = GetColValue(ei, "QCUR", dt);
            if ("Y".Equals(approvestatus))
            {
                eamt = Prolink.Math.GetValueAsDecimal(GetColValue(ei, "BAMT", dt));
                etax = Prolink.Math.GetValueAsDecimal(GetColValue(ei, "TAX", dt));
                cur = GetColValue(ei, "CUR", dt);
            }
            ei.Put("ECUR", cur);
            ei.Put("EAMT", eamt);
            ei.Put("ETAX", etax);
            return eamt;
        }
        public static string GetColValue(EditInstruct ei, string col, DataTable dt = null)
        {
            string colvalue = "";
            if (ei.IsExist(col))
            {
                colvalue = Prolink.Math.GetValueAsString(ei.Get(col));
            }
            else if (dt != null && dt.Rows.Count > 0)
            {
                colvalue = Prolink.Math.GetValueAsString(dt.Rows[0][col]);
            }
            return colvalue;
        }
    }
}
