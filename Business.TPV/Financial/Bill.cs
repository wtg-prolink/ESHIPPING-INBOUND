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
    /// <summary>
    /// 账单类
    /// </summary>
    public class Bill
    {
        /// <summary>
        /// 是否只计算第一条记录
        /// </summary>
        bool _topOne = true;

        /// <summary>
        /// 代表预提成功个数
        /// </summary>
        int _qcount = 0;
        public Bill()
        {
        }

        public Bill(bool topOne)
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
        /// <summary>
        /// 询价方案
        /// </summary>
        Dictionary<string, object> _qt_schems = new Dictionary<string, object>();
        #region 运费计算

        /// <summary>
        /// 拖车费用
        /// </summary>
        /// <param name="brokerDt"></param>
        /// <param name="parm"></param>
        private Dictionary<string, object> TrailerFreight(DataTable trailerDt, Dictionary<string, object> parm, DataTable rateDt, List<string> chgList)
        {
            Helper.AddOthColumns(trailerDt);
            decimal Cnt20 = Helper.GetDecimalValue(parm["Cnt20"]);
            decimal Cnt40 = Helper.GetDecimalValue(parm["Cnt40"]);
            decimal Cnt40hq = Helper.GetDecimalValue(parm["Cnt40hq"]);
            decimal cw = Helper.GetDecimalValue(parm["cw"]);
            decimal gw = Helper.GetDecimalValue(parm["gw"]);
            decimal cbm = Helper.GetDecimalValue(parm["cbm"]);
            decimal carType = Helper.GetDecimalValue(parm["carType"]);
            decimal car_cw = Helper.GetDecimalValue(parm["car_cw"]);
            decimal carType1 = Helper.GetDecimalValue(parm["carType1"]);
            decimal car_cw1 = Helper.GetDecimalValue(parm["car_cw1"]);
            decimal carType2 = Helper.GetDecimalValue(parm["carType2"]);
            decimal car_cw2 = Helper.GetDecimalValue(parm["car_cw2"]);
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            int index = 1;
            decimal total = 0M;
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = GetEiList(schems, index);

            List<string> msg = new List<string>();
            List<string> testList = new List<string>();
            foreach (DataRow trailer in trailerDt.Rows)
            {
                string quot_no = Prolink.Math.GetValueAsString(trailer["QUOT_NO"]);
                string repay = Prolink.Math.GetValueAsString(trailer["REPAY"]);
                string chg_cd = Prolink.Math.GetValueAsString(trailer["CHG_CD"]);
                string is_share = Prolink.Math.GetValueAsString(trailer["IS_SHARE"]);
                trailer["CUR"] = trailer["M_CUR"];
                string cur = Prolink.Math.GetValueAsString(trailer["CUR"]);

                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;

                if ("A".Equals(repay))
                {
                    if (!string.IsNullOrEmpty(chg_cd) && chgList != null)
                    {
                        if (chgList.Where(chg => !string.IsNullOrEmpty(chg) && chg.StartsWith(chg_cd)).Count() <= 0)
                            chgList.Add(chg_cd + "###" + is_share + "#" + cur);
                            //chgList.Add(chg_cd + "#" + is_share);
                    }
                    continue;
                }

                switch (repay)
                {
                    case "C":
                    case "Y":
                        if (!CheckTrailerCCharge(chg_cd, parm))
                        {
                            EditInstruct cei = CreateCEditInstruct(rateDt, tranMode, trailer, "", "", "F1");
                            elist.Add(cei);
                            continue;
                        }
                        break;
                    case "M":
                        break;
                    default:
                        continue;
                }

                //index++;              
                bool error = false;
             
                decimal F1 = Prolink.Math.GetValueAsDecimal(trailer["F1"]);//20'
                decimal F2 = Prolink.Math.GetValueAsDecimal(trailer["F2"]);//40'
                decimal gp20_total = 0m;
                decimal gp40_total = 0m;
                if ("MCO".Equals(chg_cd))
                {
                    string sql = string.Format("SELECT COUNT(1) FROM (SELECT U_ID FROM SMIRV WHERE SHIPMENT_ID={0}) A LEFT JOIN SMRVM B ON A.U_ID=B.U_FID WHERE (DATENAME(hour,B.MODIFY_DATE)>=21 OR DATENAME(hour,B.MODIFY_DATE)<3)", SQLUtils.QuotedStr(_shipment_id));
                    decimal qty = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    decimal price = 0M;
                    if (F1 > 0) price = F1;
                    else if (F2 > 0) price = F2;
                    gp20_total = price * qty;
                    trailer["QCHG_UNIT"] = "SET";
                    trailer["QUNIT_PRICE"] = price;
                    trailer["QQTY"] = qty;
                    trailer["QAMT"] = Helper.Get45AmtValue(price * qty);
                    trailer["C_FLAG"] = "Y";

                    decimal temp1 = 0M;
                    trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, gp20_total, cur, ref temp1, ref error, _localCur);
                    trailer["LOCALE_AMT"] = temp1;
                    total += temp1;
                    SetChargeInfo(trailer, "", tranMode);
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, trailer, _current_debitno, "", "", false, false));
                }
                else
                {
                    string punit = Prolink.Math.GetValueAsString(trailer["PUNIT"]);
                    //有单位by 单价计算      即使单价为0   报价也要出来
                    if (string.IsNullOrEmpty(punit))
                    {
                        if (Cnt20 > 0)
                        {
                            gp20_total = F1 * Cnt20;
                            trailer["QCHG_UNIT"] = Unit.CNT20GP;
                            trailer["QUNIT_PRICE"] = F1;
                            trailer["QQTY"] = Cnt20;
                            trailer["QAMT"] = Helper.Get45AmtValue(Cnt20 * F1);
                            trailer["C_FLAG"] = "Y";

                            decimal temp1 = 0M;
                            trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, gp20_total, cur, ref temp1, ref error, _localCur);
                            trailer["LOCALE_AMT"] = temp1;
                            total += temp1;
                            SetChargeInfo(trailer, "", tranMode);
                            if (_topOne)
                                elist.Add(CreateBillItem(_current_smsm, trailer, _current_debitno, "", "", false, false));
                        }

                        if ((Cnt40 > 0 || Cnt40hq > 0))
                        {
                            if (Cnt40 > 0 && Cnt40hq > 0)
                            {
                                gp40_total = F2 * (Cnt40 + Cnt40hq);
                                trailer["QQTY"] = Cnt40 + Cnt40hq;
                                trailer["QCHG_UNIT"] = "CTR";
                            }
                            else if (Cnt40 > 0)
                            {
                                gp40_total = F2 * Cnt40;
                                trailer["QQTY"] = Cnt40;
                                trailer["QCHG_UNIT"] = Unit.CNT40GP;
                            }
                            else
                            {
                                gp40_total = F2 * Cnt40hq;
                                trailer["QQTY"] = Cnt40hq;
                                trailer["QCHG_UNIT"] = Unit.CNT40HQ;
                            }
                            trailer["QUNIT_PRICE"] = F2;
                            trailer["C_FLAG"] = "Y";
                            trailer["QAMT"] = Helper.Get45AmtValue(gp40_total);

                            decimal temp1 = 0M;
                            trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, gp40_total, cur, ref temp1, ref error, _localCur);
                            trailer["LOCALE_AMT"] = temp1;
                            total += temp1;
                            SetChargeInfo(trailer, "", tranMode);
                            if (_topOne)
                                elist.Add(CreateBillItem(_current_smsm, trailer, _current_debitno, "", "", false, false));
                        }
                    }

                    decimal F3 = Prolink.Math.GetValueAsDecimal(trailer["F3"]);//费用
                    decimal qty = Helper.GetQty(punit, parm);
                    if (!string.IsNullOrEmpty(punit))
                    {
                        decimal cur_total = Helper.Get45AmtValue(qty * F3);
                        if ("%".Equals(punit))
                        {
                            cur_total = Helper.Get45AmtValue(qty * F3 * 0.01M);
                        }
                        //if (qty <= 0 || F1<=0)
                        //    continue;
                        decimal f3_l = 0M;
                        //if ("%".Equals(punit))
                        //    cur = "USD";
                        trailer["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref f3_l, ref error, _localCur);
                        if ("%".Equals(punit))
                        {
                            trailer["QEX_RATE"] = 1;
                            F3 = f3_l;
                            qty = 1;
                            cur_total = f3_l;
                            trailer["CUR"] = _localCur;
                        }
                        trailer["LOCALE_AMT"] = f3_l;
                        trailer["EX_REMARK"] = "";
                        SetChargeInfo(trailer, "", tranMode);

                        trailer["QCHG_UNIT"] = punit;
                        trailer["QUNIT_PRICE"] = F3;
                        trailer["QQTY"] = qty;
                        trailer["QAMT"] = cur_total;
                        trailer["C_FLAG"] = "Y";

                        elist.Add(CreateBillItem(_current_smsm, trailer, _current_debitno, "", "", false, false));
                    }

                }
            }
            schems["方案"] = index;
            schems["方案" + index] = elist;
            schems["方案" + index + "_amt"] = total;
            schems["方案" + index + "_remark"] = string.Join(";", msg);
            WriteLog(string.Join(";", msg));

            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// 报关费用计算
        /// </summary>
        /// <param name="brokerDt"></param>
        /// <param name="parm"></param>
        private Dictionary<string, object> BrokerFreight(DataTable brokerDt, Dictionary<string, object> parm, DataTable rateDt)
        {
            Helper.AddOthColumns(brokerDt);
            decimal Cnt20 = Helper.GetDecimalValue(parm["Cnt20"]);
            decimal Cnt40 = Helper.GetDecimalValue(parm["Cnt40"]);
            decimal Cnt40hq = Helper.GetDecimalValue(parm["Cnt40hq"]);
            decimal cw = Helper.GetDecimalValue(parm["cw"]);
            decimal gw = Helper.GetDecimalValue(parm["gw"]);
            decimal cbm = Helper.GetDecimalValue(parm["cbm"]);
            decimal carType = Helper.GetDecimalValue(parm["carType"]);
            decimal car_cw = Helper.GetDecimalValue(parm["car_cw"]);
            decimal carType1 = Helper.GetDecimalValue(parm["carType1"]);
            decimal car_cw1 = Helper.GetDecimalValue(parm["car_cw1"]);
            decimal carType2 = Helper.GetDecimalValue(parm["carType2"]);
            decimal car_cw2 = Helper.GetDecimalValue(parm["car_cw2"]);
            string wms = Prolink.Math.GetValueAsString(parm["WMS"]);
            //decimal pre_total = 0M;
            decimal total = 0M;

            int index = 1;
            Dictionary<string, object> schems = GetSchems();
            List<EditInstruct> elist = GetEiList(schems, index);
            List<string> msg = new List<string>();
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            List<string> testList = new List<string>();
            foreach (DataRow broker in brokerDt.Rows)
            {
                string quot_no = Prolink.Math.GetValueAsString(broker["QUOT_NO"]);
                string repay = Prolink.Math.GetValueAsString(broker["REPAY"]);
                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;
                //ECD
                string chg_cd = Prolink.Math.GetValueAsString(broker["CHG_CD"]);
                string carrier = Prolink.Math.GetValueAsString(broker["CARRIER"]);
                if (!string.IsNullOrEmpty(carrier) && !carrier.Equals(wms))
                    continue;

                if (!"M".Equals(repay))
                {
                    switch (repay)
                    {
                        case "C":
                        case "Y":
                            if (!CheckBrokerCCharge(chg_cd, parm))
                            {
                                EditInstruct cei = CreateCEditInstruct(rateDt, tranMode, broker, "", "");
                                elist.Add(cei);
                                continue;
                            }
                            break;
                        default:
                            continue;
                    }
                }

                bool error = false;
                string cur = Prolink.Math.GetValueAsString(broker["CUR"]);
                decimal F1 = Prolink.Math.GetValueAsDecimal(broker["F3"]);
                string punit = Prolink.Math.GetValueAsString(broker["PUNIT"]);
                decimal qty = Helper.GetQty(punit, parm);
                decimal cur_total = 0M;

                SetOthCharge(parm, chg_cd, F1, ref punit, ref qty, ref cur_total);

                //if (qty <= 0)
                //    continue;

                decimal temp1 = 0M;
                //if ("%".Equals(punit))
                //    cur = "USD";
                broker["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    broker["QEX_RATE"] = 1;
                    F1 = temp1;
                    qty = 1;
                    cur_total = temp1;
                    broker["CUR"] = _localCur;
                }

                broker["LOCALE_AMT"] = temp1;
                SetChargeInfo(broker, "", tranMode);

                broker["QCHG_UNIT"] = punit;
                broker["QUNIT_PRICE"] = F1;
                broker["QQTY"] = qty;
                broker["QAMT"] = cur_total;
                broker["C_FLAG"] = "Y";
                elist.Add(CreateBillItem(_current_smsm, broker, _current_debitno, "", "", false, false));
            }
            schems["方案"] = index;
            schems["方案" + index] = elist;
            schems["方案" + index + "_amt"] = total;
            schems["方案" + index + "_remark"] = string.Join(";", msg);
            WriteLog(string.Join(";", msg));
            _qt_schems[_current_debitno] = schems;
            return schems;
        }


        /// <summary>
        /// Local费用计算
        /// </summary>
        /// <param name="localDt"></param>
        /// <param name="parm"></param>
        private Dictionary<string, object> LocalFreight(DataTable localDt, Dictionary<string, object> parm, DataTable rateDt, List<string> d_to, DataTable partyDt, string shipment_id, List<string> chgList, DataRow thc, string partyType = "FC")
        {
            string sm_pod = Prolink.Math.GetValueAsString(parm["pod"]);
            string cout = Prolink.Math.GetValueAsString(parm["cout"]);
            Dictionary<string, object> schems = GetSchems();
            if (localDt == null) return schems;
            Helper.AddOthColumns(localDt);
            decimal total = 0M;
            int index = 1;
            List<EditInstruct> elist = GetEiList(schems, index);
            List<string> msg = new List<string>();
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            string sm_carrier = Helper.GetUrlDecodeValue(Prolink.Math.GetValueAsString(parm["carrier"]));
            List<string> charge_type = GetChargeType(d_to, new List<string> { DESTINATION_CHARGE }, new List<string> { "O" });

            string[] partyNos = Helper.GetPartyNo(shipment_id, partyDt, new string[] { partyType });

            string filter = string.Empty;
            //string filter1 = string.Empty;
            //if (!string.IsNullOrEmpty(sm_carrier))
            //    filter1 = " AND CARRIER=" + SQLUtils.QuotedStr(sm_carrier);
            if (!string.IsNullOrEmpty(partyNos[0]))
                filter = " AND CUST_CD=" + SQLUtils.QuotedStr(partyNos[0]);
            DataRow[] drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter);
            if (drs.Length <= 0 && !string.IsNullOrEmpty(filter))
            {
                filter = " AND (CUST_CD='' OR CUST_CD IS NULL)";
                drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter);
            }

            //string filter = string.Empty;
            //string filter1 = string.Empty;
            //if (!string.IsNullOrEmpty(sm_carrier))
            //    filter1 = " AND CARRIER=" + SQLUtils.QuotedStr(sm_carrier);
            //if (!string.IsNullOrEmpty(partyNos[0]))
            //    filter = " AND CUST_CD=" + SQLUtils.QuotedStr(partyNos[0]);
            //DataRow[] drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter + filter1);
            //if (drs.Length <= 0 && !string.IsNullOrEmpty(filter))
            //{
            //    drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter + " AND (CARRIER='' OR CARRIER IS NULL)");
            //}
            //if (drs.Length <= 0)
            //{
            //    filter = " AND (CUST_CD='' OR CUST_CD IS NULL)";
            //    drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter + filter1);
            //}

            //if (drs.Length <= 0 && !string.IsNullOrEmpty(filter))
            //{
            //    filter = " AND (CUST_CD='' OR CUST_CD IS NULL)";
            //    drs = localDt.Select("CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + filter);
            //}


            List<string> testList = new List<string>();
            List<string> chgList1 = new List<string>();

            //foreach (DataRow dr in drs)
            //{
            //    string carrier = Prolink.Math.GetValueAsString(dr["CARRIER"]);
            //    string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
            //    if (string.IsNullOrEmpty(sm_carrier))
            //    {
            //        if (string.IsNullOrEmpty(carrier))
            //        {
            //            testList.Add(quot_no);
            //            break;
            //        }
            //    }
            //    else if (sm_carrier.Equals(carrier))
            //    {
            //        testList.Add(quot_no);
            //        break;
            //    }
            //}
            //foreach (DataRow local in drs)
            List<string> test = new List<string>();
            for (int i = 0; i < drs.Length; i++)
            {
                DataRow local = drs[i];
                string uid = Prolink.Math.GetValueAsString(local["U_ID"]);
                string credit_to = Prolink.Math.GetValueAsString(local["CREDIT_TO"]);
                string credit_nm = Prolink.Math.GetValueAsString(local["CREDIT_NM"]);
                string repay = Prolink.Math.GetValueAsString(local["REPAY"]);
                string chg_cd = Prolink.Math.GetValueAsString(local["CHG_CD"]);
                string quot_no = Prolink.Math.GetValueAsString(local["QUOT_NO"]);
                string carrier = Prolink.Math.GetValueAsString(local["CARRIER"]);
                string is_share = Prolink.Math.GetValueAsString(local["IS_SHARE"]);
                string pod_cd = Prolink.Math.GetValueAsString(local["POD_CD"]);
                string cur = Prolink.Math.GetValueAsString(local["CUR"]);
                if (string.IsNullOrEmpty(quot_no))
                    continue;
                if (testList.Count <= 0) testList.Add(quot_no);
                if (!testList.Contains(quot_no)) break;

                if (!string.IsNullOrEmpty(pod_cd) && !pod_cd.Equals(sm_pod))
                    continue;

                if (!string.IsNullOrEmpty(carrier) && !sm_carrier.Equals(carrier))//carrier过滤
                {
                    continue;
                }

                local = ChangeLocal(sm_carrier, drs, i, local, chg_cd, quot_no, sm_pod);
                if (local == null)
                    continue;
                uid = Prolink.Math.GetValueAsString(local["U_ID"]);
                credit_to = Prolink.Math.GetValueAsString(local["CREDIT_TO"]);
                credit_nm = Prolink.Math.GetValueAsString(local["CREDIT_NM"]);
                repay = Prolink.Math.GetValueAsString(local["REPAY"]);
                chg_cd = Prolink.Math.GetValueAsString(local["CHG_CD"]);
                quot_no = Prolink.Math.GetValueAsString(local["QUOT_NO"]);
                carrier = Prolink.Math.GetValueAsString(local["CARRIER"]);
                is_share = Prolink.Math.GetValueAsString(local["IS_SHARE"]);
                pod_cd = Prolink.Math.GetValueAsString(local["POD_CD"]);
                cur = Prolink.Math.GetValueAsString(local["CUR"]);

                if (test.Contains(chg_cd))
                    continue;
                test.Add(chg_cd);

                bool error = false;
                if ("THC".Equals(chg_cd) && "A".Equals(repay))
                {
                    CalCNT(parm, local, rateDt, tranMode, ref error, msg, elist, thc);
                    continue;
                }

                if ("A".Equals(repay))
                {
                    if (!string.IsNullOrEmpty(chg_cd) && chgList != null)
                    {
                        if (chgList.Where(chg => !string.IsNullOrEmpty(chg) && chg.StartsWith(chg_cd)).Count() <= 0)
                            chgList.Add(chg_cd + "#" + credit_to + "#" + credit_nm + "#" + is_share + "#" + cur);
                    }
                    continue;
                }

                if (!"M".Equals(repay))
                {
                    switch (repay)
                    {
                        case "C":
                        case "Y":
                            if (!CheckLocalCCharge(chg_cd, parm))
                            {
                                EditInstruct cei = CalCYCNT(parm, local, rateDt, tranMode, ref error, msg, elist);
                                if (cei == null)
                                    cei = CreateCEditInstruct(rateDt, tranMode, local, credit_to, credit_nm);
                                elist.Add(cei);

                                continue;
                            }
                            break;
                        default:
                            continue;
                    }
                }
                _qcount++;


                if (CalCNT(parm, local, rateDt, tranMode, ref error, msg, elist))
                    continue;
                //if (CalCNT(parm, local, rateDt, tranMode, ref error, msg, elist, "THC".Equals(chg_cd) ? thc : null))
                //    continue;


                decimal F1 = Prolink.Math.GetValueAsDecimal(local["F3"]);
                string punit = Prolink.Math.GetValueAsString(local["PUNIT"]);
                decimal qty = Helper.GetQty(punit, parm);
                decimal cur_total = Helper.Get45AmtValue(qty * F1);
                if ("%".Equals(punit))
                {
                    cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                }
                //if (qty <= 0 || F1<=0)
                //    continue;
                decimal temp1 = 0M;
                //if ("%".Equals(punit))
                //    cur = "USD";
                local["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                if ("%".Equals(punit))
                {
                    local["QEX_RATE"] = 1;
                    F1 = temp1;
                    qty = 1;
                    cur_total = temp1;
                    local["CUR"] = _localCur;
                }
                local["LOCALE_AMT"] = temp1;
                local["EX_REMARK"] = "";
                SetChargeInfo(local, "", tranMode);

                local["QCHG_UNIT"] = punit;
                local["QUNIT_PRICE"] = F1;
                local["QQTY"] = qty;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                if (!"Y".Equals(cout))//4.	Local 費用報價增加是否分攤的欄位只能輸入Yes/空白, 只要是shipment SMSM,COUT=Y 的定倉,且local 費用是否分攤=Y ,費用算出後都要再分攤.
                    local["IS_SHARE"] = "";

                elist.Add(CreateBillItem(_current_smsm, local, _current_debitno, credit_to, credit_nm, false, false));
            }
            schems["方案"] = index;
            schems["方案" + index] = elist;
            schems["方案" + index + "_amt"] = total;
            schems["方案" + index + "_remark"] = string.Join(";", msg);
            WriteLog(string.Join(";", msg));
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// 新增C类数据
        /// </summary>
        /// <param name="rateDt"></param>
        /// <param name="msg"></param>
        /// <param name="tranMode"></param>
        /// <param name="local"></param>
        /// <param name="credit_to"></param>
        /// <param name="credit_nm"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private EditInstruct CreateCEditInstruct(DataTable rateDt, string tranMode, DataRow local, string credit_to, string credit_nm, string amtField = "F3")
        {
            bool error = false;
            decimal amt = Prolink.Math.GetValueAsDecimal(local[amtField]);
            local["QCHG_UNIT"] = "SET";
            local["QUNIT_PRICE"] = amt;
            local["QQTY"] = 1;
            local["QAMT"] = amt;
            local["C_FLAG"] = "Y";
            decimal amt1 = 0M;
            local["QEX_RATE"] = Helper.GetTotal(rateDt, null, amt, Prolink.Math.GetValueAsString(local["CUR"]), ref amt1, ref error, _localCur);
            local["LOCALE_AMT"] = amt1;
            SetChargeInfo(local, "", tranMode);
            EditInstruct cei = CreateBillItem(_current_smsm, local, _current_debitno, credit_to, credit_nm, false, false);
            cei.ID = "SMBID_TEMP";
            return cei;
        }

        /// <summary>
        /// 根据carrier 变更local
        /// </summary>
        /// <param name="sm_carrier"></param>
        /// <param name="drs"></param>
        /// <param name="index"></param>
        /// <param name="local"></param>
        /// <param name="chg_cd"></param>
        /// <param name="quot_no"></param>
        /// <returns></returns>
        private static DataRow ChangeLocal(string sm_carrier, DataRow[] drs, int index, DataRow local, string chg_cd, string quot_no, string sm_pod)
        {
            for (int i =0; i < drs.Length; i++)//同时满足carrier 和 POD
            {
                string chg_cd1 = Prolink.Math.GetValueAsString(drs[i]["CHG_CD"]);
                string u_id = Prolink.Math.GetValueAsString(drs[i]["U_ID"]);
                if (Prolink.Math.GetValueAsString(drs[i]["CARRIER"]).Equals(sm_carrier)
                    && Prolink.Math.GetValueAsString(drs[i]["POD_CD"]).Equals(sm_pod)
                     && chg_cd1.Equals(chg_cd)
                    && Prolink.Math.GetValueAsString(drs[i]["QUOT_NO"]).Equals(quot_no))
                {
                    string repay = Prolink.Math.GetValueAsString(drs[i]["REPAY"]);
                    return drs[i];
                }
            }
            for (int i = 0; i < drs.Length; i++)//满足carrier
            {
                string chg_cd1 = Prolink.Math.GetValueAsString(drs[i]["CHG_CD"]);
                string u_id = Prolink.Math.GetValueAsString(drs[i]["U_ID"]);
                if (Prolink.Math.GetValueAsString(drs[i]["CARRIER"]).Equals(sm_carrier)
                     && chg_cd1.Equals(chg_cd)
                     && string.IsNullOrEmpty(Prolink.Math.GetValueAsString(drs[i]["POD_CD"]))
                    && Prolink.Math.GetValueAsString(drs[i]["QUOT_NO"]).Equals(quot_no))
                {
                    string repay = Prolink.Math.GetValueAsString(drs[i]["REPAY"]);
                    return drs[i];
                }
            }
            for (int i = 0; i < drs.Length; i++)//满足POD
            {
                string chg_cd1 = Prolink.Math.GetValueAsString(drs[i]["CHG_CD"]);
                string u_id = Prolink.Math.GetValueAsString(drs[i]["U_ID"]);
                if ( Prolink.Math.GetValueAsString(drs[i]["POD_CD"]).Equals(sm_pod)
                     && chg_cd1.Equals(chg_cd)
                     && string.IsNullOrEmpty(Prolink.Math.GetValueAsString(drs[i]["CARRIER"]))
                    && Prolink.Math.GetValueAsString(drs[i]["QUOT_NO"]).Equals(quot_no))
                {
                    string repay = Prolink.Math.GetValueAsString(drs[i]["REPAY"]);
                    return drs[i];
                }
            }
            return local;
            //if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(local["CARRIER"])))
            //    return local;
            //return null;
        }

        /// <summary>
        ///  检查broker C类费用
        /// </summary>
        /// <param name="chg_cd"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        private static bool CheckBrokerCCharge(string chg_cd, Dictionary<string, object> parm)
        {
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            string customs_check = Prolink.Math.GetValueAsString(parm["customs_check"]);//是否查驗 
            decimal decl_num = Helper.GetDecimalValue(parm["decl_num"]);
            decimal cont_decl_num = Helper.GetDecimalValue(parm["CONT_DECL_NUM"]);
            decimal next_num = Prolink.Math.GetValueAsDecimal(parm["next_num"]); //续单
            string is_land = Prolink.Math.GetValueAsString(parm["is_land"]); //续单
            bool result = false;
            switch (chg_cd)
            {
                case "IHC"://1.	FCL/LCL  增加是否查驗 (Customs_check x(1) ),如果為Y 要收IHC 費用. 
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                            if ("Y".Equals(customs_check))
                                result = true;
                            break;
                    }
                    break;
                case "INC"://1.	AIR  增加是否查驗 (Customs_check x(1) ),如果為Y 要收INC 費用. 2.	FCL  增加是否查驗 (Customs_check x(1) ),如果為Y 要收INC 費用.
                    switch (tranMode)
                    {
                        //case "F":
                        case "A":
                            if ("Y".Equals(customs_check))
                                result = true;
                            break;
                    }
                    break;
                case "DDC"://3.	FCL/LCL/AIR 如果有續單(兩張報單以上),要收DDC 費用. 
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if (next_num > 0)
                                result = true;
                            break;
                    }
                    break;
                case "EDN"://續單費=报单数(N-1)*報關續單費EDC1单价  (報單數:如圖標1)
                case "ECD"://3.	FCL/LCL/AIR 如果有SMSM.DECL_NUM,要收EDC 費用. = DDC 費用 X DECL_NUIM (如果DECL_NUM 為 0 ,  請 Default 1 )  
                    switch (tranMode)
                    {
                        case "F":
                        case "L":
                        case "A":
                            if ("EDN".Equals(chg_cd))//續單費=报单数(N-1)*報關續單費EDC1单价  (報單數:如圖標1)
                            {
                                if (cont_decl_num > 0)
                                    result = true;
                            }
                            else
                                result = true;
                            break;
                    }
                    break;
                case "CTC"://9.	Air 判斷陸運? 如果該欄位=y , 要收取 CTC/SSU 費用.
                case "OTC"://报关转关
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

        private static bool CheckTrailerCCharge(string chg_cd, Dictionary<string, object> parm)
        {
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);
            bool result = false;
            switch (chg_cd)
            {
                case "MCO "://(夜間移櫃費,) 請到貨櫃管理,看一下該Shipment 下的貨櫃, 是否有移櫃紀錄,且是在晚上 21:00-03:00 之間, 每移一次就一個費用, 移3 次, 要 X 3 . 
                    result = true;
                    break;
            }
            return result;
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

        private Dictionary<string, object> CarrierFreight(DataTable org_carrierDt, Dictionary<string, object> parm, DataTable rateDt, List<string> d_to, List<string> chgList, List<string> can_collect_by, string pol)
        {
            List<string> charge_type = GetChargeType(d_to);
            if (chgList == null) chgList = new List<string>();
            DataTable carrierDt = GetFirstCarrierRow(org_carrierDt);

            Dictionary<string, object> schems = GetSchems();
            if (carrierDt == null) return schems;
            Helper.AddOthColumns(carrierDt);
            decimal total = 0M;
            int index = 1;
            List<EditInstruct> elist = GetEiList(schems, index);
            List<string> msg = new List<string>();
            string tranMode = Prolink.Math.GetValueAsString(parm["TranMode"]);

            //List<string> testList = new List<string>();
            foreach (string test_chg_cd in chgList)
            {
                string m_cur = string.Empty;
                string test_chg_cd1 = test_chg_cd;
                string credit_to = string.Empty, credit_nm = string.Empty;
                string[] test_chg_cds = test_chg_cd.Split(new string[] { "#" }, StringSplitOptions.None);
                if (test_chg_cds.Length > 0)
                    test_chg_cd1 = test_chg_cds[0];
                if (test_chg_cds.Length > 3)
                {
                    credit_to = test_chg_cds[1];
                    credit_nm = test_chg_cds[2];
                    m_cur = test_chg_cds[4];
                }

                if (test_chg_cds.Length > 4)
                    m_cur = test_chg_cds[4];

                DataRow[] drs = carrierDt.Select("CHG_CD=" + SQLUtils.QuotedStr(test_chg_cd1) + " AND CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + " AND POL_CD=" + SQLUtils.QuotedStr(pol), "QT_EFFECT_FROM DESC,QUOT_NO");
                if (drs.Length <= 0)
                    drs = carrierDt.Select("CHG_CD=" + SQLUtils.QuotedStr(test_chg_cd1) + " AND CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()) + " AND (POL_CD IS NULL OR POL_CD='')", "QT_EFFECT_FROM DESC,QUOT_NO");
                List<string> testList = new List<string>();
                foreach (DataRow carrier in drs)
                {
                    string quot_no = Prolink.Math.GetValueAsString(carrier["QUOT_NO"]);
                    string collect_by = Prolink.Math.GetValueAsString(carrier["COLLECT_BY"]);//代收
                    string repay = Prolink.Math.GetValueAsString(carrier["REPAY"]);
                    if (string.IsNullOrEmpty(quot_no))
                        continue;
                    if (testList.Count <= 0) testList.Add(quot_no);
                    if (!testList.Contains(quot_no)) break;
                    bool error = false;
                    string chg_cd = Prolink.Math.GetValueAsString(carrier["CHG_CD"]);
                    switch (repay)
                    {
                        case "C":
                        case "Y":
                            if (!CheckLocalCCharge(chg_cd, parm) && !CheckBrokerCCharge(chg_cd, parm) && !CheckTrailerCCharge(chg_cd, parm))
                            {
                                //bool error = false;
                                EditInstruct cei = CalCYCNT(parm, carrier, rateDt, tranMode, ref error, msg, elist);
                                if (cei == null)
                                    cei = CreateCEditInstruct(rateDt, tranMode, carrier, credit_to, credit_nm);
                                elist.Add(cei);


                                //EditInstruct cei = CreateCEditInstruct(rateDt, tranMode, carrier, credit_to, credit_nm);
                                //elist.Add(cei);

                                continue;
                            }
                            break;
                        case "M":
                            break;
                        default:
                            continue;
                    }

                    string isShare = test_chg_cd.Contains("#Y") ? "Y" : "N";
                    carrier["IS_SHARE"] = isShare;
                    if (!string.IsNullOrEmpty(collect_by) && !can_collect_by.Contains(collect_by))
                        continue;
                    //if ("Y".Equals(repay) && string.IsNullOrEmpty(collect_by))
                    //    continue;
                 

                    if (CalCNT(parm, carrier, rateDt, tranMode, ref error, msg, elist, null, false, credit_to, credit_nm))
                        break;

                    string cur = Prolink.Math.GetValueAsString(carrier["CUR"]);
                   
                    decimal F1 = Prolink.Math.GetValueAsDecimal(carrier["F3"]);
                    string punit = Prolink.Math.GetValueAsString(carrier["PUNIT"]);
                    decimal qty = Helper.GetQty(punit, parm);

                    decimal cur_total = 0M;
                    SetOthCharge(parm, chg_cd, F1, ref punit, ref qty, ref cur_total);

                    //if (qty <= 0 || F1 <= 0)
                    //    continue;
                    decimal temp1 = 0M;
                    //if ("%".Equals(punit))
                    //    cur = "USD";
                    if (!string.IsNullOrEmpty(m_cur) && !m_cur.Equals(cur))
                    {
                        Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, m_cur);
                        cur = m_cur;
                        carrier["CUR"] = m_cur;
                        cur_total = temp1;
                        temp1 = 0;
                        F1 = cur_total;
                        qty = 1;
                    }

                    carrier["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);

                    if ("%".Equals(punit))
                    {
                        carrier["QEX_RATE"] = 1;
                        F1 = temp1;
                        cur_total = temp1;
                        qty = 1;
                        carrier["CUR"] = _localCur;
                    }

                    carrier["LOCALE_AMT"] = temp1;
                    carrier["EX_REMARK"] = "";
                    SetChargeInfo(carrier, "", tranMode);

                    carrier["QCHG_UNIT"] = punit;
                    carrier["QUNIT_PRICE"] = F1;
                    carrier["QQTY"] = qty;
                    carrier["QAMT"] = cur_total;
                    carrier["C_FLAG"] = "Y";
                    elist.Add(CreateBillItem(_current_smsm, carrier, _current_debitno, credit_to, credit_nm, false, false));
                    break;
                }
            }
            schems["方案"] = index;
            schems["方案" + index] = elist;
            schems["方案" + index + "_amt"] = total;
            schems["方案" + index + "_remark"] = string.Join(";", msg);
            WriteLog(string.Join(";", msg));
            _qt_schems[_current_debitno] = schems;
            return schems;
        }

        /// <summary>
        /// 设置其他特殊费用的计算
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="chg_cd"></param>
        /// <param name="F1"></param>
        /// <param name="punit"></param>
        /// <param name="qty"></param>
        /// <param name="cur_total"></param>
        private void SetOthCharge(Dictionary<string, object> parm, string chg_cd, decimal F1, ref string punit, ref decimal qty, ref decimal cur_total)
        {
            decimal decl_num = Helper.GetDecimalValue(parm["decl_num"]);
            decimal cont_decl_num = Helper.GetDecimalValue(parm["CONT_DECL_NUM"]);
            
            switch (chg_cd)
            {
                case "MCO"://1.	MCO (夜間移櫃費,) 請到貨櫃管理,看一下該Shipment 下的貨櫃, 是否有移櫃紀錄,且是在晚上 21:00-03:00 之間, 每移一次就一個費用, 移3 次, 要 X 3 . 
                    //punit = "SET";
                    string sql = string.Format("SELECT COUNT(1) FROM (SELECT U_ID FROM SMIRV WHERE SHIPMENT_ID={0}) A LEFT JOIN SMRVM B ON A.U_ID=B.U_FID WHERE (DATENAME(hour,B.MODIFY_DATE)>=21 OR DATENAME(hour,B.MODIFY_DATE)<3)", SQLUtils.QuotedStr(_shipment_id));
                    qty = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    cur_total = Helper.Get45AmtValue(qty * F1);
                    WriteLog("移柜数:" + sql);
                    break;
                case "EDN"://續單費=报单数(N-1)*報關續單費EDC1单价  (報單數:如圖標1)
                    punit = "SET";
                    qty = cont_decl_num;
                    cur_total = Helper.Get45AmtValue(qty * F1);
                    break;
                case "ECD"://報關單費, 首单费=报单数*出口报关费EDC单价
                    punit = "SET";
                    decl_num = 1;
                    //if (decl_num <= 0) decl_num = 1;
                    cur_total = Helper.Get45AmtValue(decl_num * F1);
                    qty = decl_num;
                    break;
                case "DDC"://续单,
                    punit = "SET";
                    decimal next_num = Helper.GetDecimalValue(parm["next_num"]);
                    cur_total = Helper.Get45AmtValue(next_num * F1);
                    //broker["EX_REMARK"] = string.Format("报单:{0}*{1};续单:{2}*{3}", decl_num, p1, next_num, p2);
                    qty = next_num;
                    break;
                default:
                    cur_total = Helper.Get45AmtValue(qty * F1);
                    if ("%".Equals(punit))
                    {
                        cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                    }
                    break;
            }
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

                string cur = Prolink.Math.GetValueAsString(air["CUR"]);
                decimal F1 = Prolink.Math.GetValueAsDecimal(air["F3"]);
                string punit = Prolink.Math.GetValueAsString(air["PUNIT"]);
                decimal qty = Helper.GetQty(punit, parm);
                decimal cur_total = Helper.Get45AmtValue(qty * F1);
                if ("%".Equals(punit))
                {
                    cur_total = Helper.Get45AmtValue(qty * F1 * 0.01M);
                }
                if (qty <= 0 || F1 <= 0)
                    continue;
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

        static Dictionary<string, string> _cntParm = new Dictionary<string, string> { { "20GP", "F4" }, { "40GP", "F5" }, { "40HQ", "F6" } };
        static Dictionary<string, string> _cntParm1 = new Dictionary<string, string> { { "20GP", "GP20" }, { "40GP", "GP40" }, { "40HQ", "HQ40" } };
        /// <summary>
        /// 计算货柜价钱
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="local"></param>
        /// <param name="rateDt"></param>
        /// <param name="tranMode"></param>
        /// <param name="error"></param>
        /// <param name="msg"></param>
        /// <param name="elist"></param>
        /// <returns></returns>
        private bool CalCNT(Dictionary<string, object> parm, DataRow local, DataTable rateDt, string tranMode, ref bool error, List<string> msg, List<EditInstruct> elist, DataRow thc = null, bool isFreight = false, string credit_to = "", string credit_nm = "")
        {
            decimal cur_total = 0m;
            string cur = Prolink.Math.GetValueAsString(local["CUR"]);
            List<string> cntMsg = new List<string>();
            DataRow dr = local;
            Dictionary<string, string> cntParm = _cntParm;
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
                decimal qty = Helper.GetQty(punit, parm);
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


        private EditInstruct CalCYCNT(Dictionary<string, object> parm, DataRow local, DataTable rateDt, string tranMode, ref bool error, List<string> msg, List<EditInstruct> elist, DataRow thc = null, bool isFreight = false, string credit_to = "", string credit_nm = "")
        {
            decimal cur_total = 0m;
            string cur = Prolink.Math.GetValueAsString(local["CUR"]);
            List<string> cntMsg = new List<string>();
            DataRow dr = local;
            Dictionary<string, string> cntParm = _cntParm;
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
                decimal qty = Helper.GetQty(punit, parm);
                if (qty <= 0 || price <= 0)
                    continue;
                cntMsg.Add(string.Format("{0}({1}*{2}{3})", kv.Key, qty, price, cur));
                cur_total += Helper.Get45AmtValue(qty * price);
            }

            if (cur_total > 0)
            {
                local["QCHG_UNIT"] = "SET";
                local["QUNIT_PRICE"] = cur_total;
                local["QQTY"] = 1;
                local["QAMT"] = cur_total;
                local["C_FLAG"] = "Y";
                decimal temp1 = 0M;
                local["QEX_RATE"] = Helper.GetTotal(rateDt, msg, cur_total, cur, ref temp1, ref error, _localCur);
                local["LOCALE_AMT"] = temp1;
                SetChargeInfo(local, "", tranMode);
                EditInstruct cei = CreateBillItem(_current_smsm, local, _current_debitno, credit_to, credit_nm, false, false);
                cei.ID = "SMBID_TEMP";
                return cei;
            }
            return null;
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

            //SELECT SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM,BSCAA.* FROM SMFSC LEFT JOIN BSCAA ON SMFSC.U_ID=BSCAA.U_FID WHERE PORT='XX' AND SMFSC.CMP='' AND SMFSC.CARRIER=''

            switch (tranMode)
            {
                case "R":
                case "F"://海运FCL
                    //polList = Helper.GetValueList(dt, "POL_CD");
                    //if (othDt == null)
                    //    othDt = OperationUtils.GetDataTable("SELECT * FROM SMQTD WHERE TRAN_MODE='O'" + ((polList.Count > 0) ? (" AND POL_CD IN " + Helper.JoinString(polList.ToArray())) : ""), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                    FclFreight(parm, dt, othDt, rateDt, d_to, buDt);
                    break;
                case "L":
                    jobNoList = Helper.GetValueList(dt, "U_FID");
                    if (jobNoList.Count > 0)
                    {
                        othDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE U_FID IN {0})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM,QUOT_NO", Helper.JoinString(jobNoList.ToArray())), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //othDt = OperationUtils.GetDataTable("SELECT * FROM SMQTD WHERE U_FID IN " + Helper.JoinString(jobNoList.ToArray()), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        LCLFreight(parm, dt, othDt, rateDt, d_to);
                    }
                    break;
                case "T"://内陆运输
                    TruckFreight(parm, dt, null, rateDt);
                    break;
                case "A"://国际快递
                    AirFreight(parm, dt, null, rateDt, d_to);
                    break;
                case "D"://国内快递
                    InlandExpressFreight(parm, dt, null, rateDt);
                    break;
                case "E"://国际快递
                    ExpressFreight(parm, dt, null, rateDt);
                    break;
            }
        }

        /// <summary>
        /// 本币类型
        /// </summary>
        string _localCur = "CNY";
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

            DataRow[] drs = null;
            foreach (DataRow dr in dt.Rows)
            {
                string pol_cd = Prolink.Math.GetValueAsString(dr["POL_CD"]);
                string pod_cd = Prolink.Math.GetValueAsString(dr["POD_CD"]);
                string u_fid = Prolink.Math.GetValueAsString(dr["U_FID"]);

                if (string.IsNullOrEmpty(pol_cd))
                    continue;

                index++;
                elist = GetEiList(schems, index);
                string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
                List<string> msg = new List<string>();
                //string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO1"]);//报价号码
                decimal total = 0M;
                bool error = false;
                //string cur = Prolink.Math.GetValueAsString(dr["CUR"]).ToUpper();
                if (charge_type.Contains("无"))
                    drs = qtDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2}", SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));
                else
                    drs = qtDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2} AND CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()), SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));

                //drs = qtDt.Select(string.Format("POL_CD={0} AND POD_CD={1} AND U_FID={2}", SQLUtils.QuotedStr(pol_cd), SQLUtils.QuotedStr(pod_cd), SQLUtils.QuotedStr(u_fid)));
                Dictionary<string, decimal> otherChg = new Dictionary<string, decimal>();

                WriteLog("LCL运费报价:" + quot_no + ";笔数:" + drs.Length);
                foreach (DataRow chg in drs)
                {
                    string cur1 = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                    string chg_descp = Prolink.Math.GetValueAsString(chg["CHG_DESCP"]).ToUpper();
                    string punit = Prolink.Math.GetValueAsString(chg["PUNIT"]).ToUpper();
                    decimal min_amt = Prolink.Math.GetValueAsDecimal(chg["MIN_AMT"]);
                    decimal price = Prolink.Math.GetValueAsDecimal(chg["F1"]);

                    decimal qty = Helper.GetQty(punit, parm);
                    //decimal qty = Helper.GetQty(punit, gw, cbm, cnt);
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

                    SetChargeInfo(chg, "OF", "L");
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
                qts = dt.Select(string.Format("U_FID={0} AND CARRIER={1} AND CHG_CD='AF'", SQLUtils.QuotedStr(quot_id), SQLUtils.QuotedStr(carrier)));
                if (qts.Length <= 0) qts = dt.Select();
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
            Helper.AddOthColumns(othDt);
            decimal Cnt20 = Helper.GetDecimalValue(parm["Cnt20"]);
            decimal Cnt40 = Helper.GetDecimalValue(parm["Cnt40"]);
            decimal Cnt40hq = Helper.GetDecimalValue(parm["Cnt40hq"]);
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
            //SELECT SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM,BSCAA.* FROM SMFSC LEFT JOIN BSCAA ON SMFSC.U_ID=BSCAA.U_FID WHERE PORT='XX' AND SMFSC.CMP='' AND SMFSC.CARRIER=''
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
            qts = dt.Select(filter + car_filter + via_filter);
            if (qts.Length <= 0 && !string.IsNullOrEmpty(car_filter)) qts = dt.Select(filter + car_filter);
            if (qts.Length <= 0 && !string.IsNullOrEmpty(via_filter)) qts = dt.Select(filter + via_filter);
            if (qts.Length <= 0) qts = dt.Select(filter);
            foreach (DataRow dr in qts)
            {
                string via_cd = Prolink.Math.GetValueAsString(dr["VIA_CD"]);
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                //if (vias.Count > 0)
                //{
                //    if (!vias.Contains(via_cd))
                //        continue;
                //}
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

                //if (buDt != null && buDt.Rows.Count > 0)
                //{
                //    DataRow[] bus = buDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(polCd)), "EFFECT_DATE DESC");
                //    if (bus.Length > 0)
                //    {
                //        string bu_cur = Prolink.Math.GetValueAsString(bus[0]["CUR"]);
                //        F12 = 0;
                //        Helper.GetTotal(rateDt, msg, Prolink.Math.GetValueAsDecimal(bus[0]["GP20"]), bu_cur, ref F12, ref error, cur);

                //        F13 = 0;
                //        Helper.GetTotal(rateDt, msg, Prolink.Math.GetValueAsDecimal(bus[0]["GP40"]), bu_cur, ref F13, ref error, cur);

                //        F14 = 0;
                //        Helper.GetTotal(rateDt, msg, Prolink.Math.GetValueAsDecimal(bus[0]["HQ40"]), bu_cur, ref F14, ref error, cur);
                //    }
                //}

                //if (F12 <= 0)
                //    F12 = Prolink.Math.GetValueAsDecimal(dr["F12"]);//20' BUC/BAF/FAF
                //if (F13 <= 0)
                //    F13 = Prolink.Math.GetValueAsDecimal(dr["F13"]);//40' BUC/BAF/FAF
                //if (F14 <= 0)
                //    F14 = Prolink.Math.GetValueAsDecimal(dr["F14"]);//40HQ BUC/BAF/FAF

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

                decimal F4 = Prolink.Math.GetValueAsDecimal(dr["F4"]);//40HQ
                if (Cnt40hq > 0 && F4 <= 0)
                {
                    error = true;
                    msg.Add("无40HQ报价");
                }
                else if (Cnt40hq > 0 && F4 > 0)
                    msg.Add(string.Format("40HQ({0}+{1}{2})*{3}", F4, F14, Helper.GetLoalCurName(cur), Cnt40hq));

                msg.Add(string.Format("货柜费用{0}{1}", Helper.Get45AmtValue(cnt), Helper.GetLoalCurName(cur)));
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

                    if ("R".Equals(tranMode))
                        SetChargeInfo(dr, "RF", "R");
                    else
                        SetChargeInfo(dr, "OF", "F");

                    _qcount++;
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                }

                if (Cnt40 > 0)
                {
                    decimal temp = Helper.Get45AmtValue(Cnt40 * (F3 + F13));
                    cnt += temp;
                    decimal temp1 = 0;
                    dr["QEX_RATE"] = Helper.GetTotal(rateDt, msg, temp, cur, ref temp1, ref error, _localCur);
                    total += temp1;
                    dr["LOCALE_AMT"] = temp1;
                    if ("R".Equals(tranMode))
                        SetChargeInfo(dr, "RF", "F");
                    else
                        SetChargeInfo(dr, "OF", "F");

                    dr["QCHG_UNIT"] = Unit.CNT40GP;
                    dr["QUNIT_PRICE"] = (F3 + F13);
                    dr["QQTY"] = Cnt40;
                    dr["QAMT"] = temp;
                    dr["C_FLAG"] = "Y";

                    _qcount++;
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                }

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
                    if ("R".Equals(tranMode))
                        SetChargeInfo(dr, "RF", "F");
                    else
                        SetChargeInfo(dr, "OF", "F");
                    dr["QCHG_UNIT"] = Unit.CNT40HQ;
                    dr["QUNIT_PRICE"] = (F4 + F14);
                    dr["QQTY"] = Cnt40hq;
                    dr["QAMT"] = temp;
                    dr["C_FLAG"] = "Y";

                    _qcount++;
                    if (_topOne)
                        elist.Add(CreateBillItem(_current_smsm, dr, _current_debitno, "", "", true));
                }
                //Helper.GetTotal(rateDt, msg, cnt, cur, ref total, ref error);

                filter = string.Format("POL_CD={0}", SQLUtils.QuotedStr(polCd));
                if (!string.IsNullOrEmpty(carrier))
                {
                    filter += " AND (CARRIER=" + SQLUtils.QuotedStr(carrier) + " OR CARRIER IS NULL)";
                }

                #region 港口其他费用
                List<string> charge_type = GetChargeType(d_to);
                Dictionary<string, decimal> otherChg = new Dictionary<string, decimal>();
                if (othDt != null)
                {
                    if (charge_type.Contains("无"))
                        drs = othDt.Select(filter);
                    else
                        drs = othDt.Select(filter + " AND CHG_TYPE IN " + Helper.JoinString(charge_type.ToArray()));
                    foreach (DataRow chg in drs)
                    {
                        string cur1 = Prolink.Math.GetValueAsString(chg["CUR"]).ToUpper();
                        decimal oth_price = Prolink.Math.GetValueAsDecimal(chg["F3"]);
                        string oth_unit = Prolink.Math.GetValueAsString(chg["PUNIT"]).ToUpper();
                        decimal oth_qty = Helper.GetQty(oth_unit, parm);
                        decimal val = oth_qty * oth_price;
                        chg["QCHG_UNIT"] = Prolink.Math.GetValueAsString(chg["PUNIT"]).ToUpper();
                        chg["QUNIT_PRICE"] = oth_price;
                        chg["QQTY"] = oth_qty;
                        chg["QAMT"] = val;
                        chg["C_FLAG"] = "Y";
                        if (_topOne)
                            elist.Add(CreateBillItem(_current_smsm, chg, _current_debitno, "", "", true));

                        if (!otherChg.ContainsKey(cur1))
                            otherChg[cur1] = val;
                        else
                            otherChg[cur1] = otherChg[cur1] + val;
                    }
                }

                if (otherChg.Count > 0)
                {
                    string othMsg = "";
                    foreach (var kv in otherChg)
                    {
                        if (othMsg.Length > 0)
                            othMsg += ",";
                        othMsg += kv.Value + kv.Key;
                    }
                    msg.Add(string.Format("其他费用{0}", othMsg));
                    foreach (var kv in otherChg)
                    {
                        Helper.GetTotal(rateDt, msg, kv.Value, kv.Key, ref total, ref error);
                    }
                }
                #endregion


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

                WriteLog(index, string.IsNullOrEmpty(quot_no) ? qt_id : quot_no, msg, total);//

                dr["CHG_REMARK"] = string.Join("；", msg);
                if (_topOne) break;
            }
            _qt_schems[_current_debitno] = schems;
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
                        string dn_no =dnp.Table.Columns.Contains("DN_NO")? Prolink.Math.GetValueAsString(dnp["DN_NO"]):string.Empty;
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
                        CalTruck(rateDt, elist, CargoType, bandType, trackWay, gw, cbm, dr, string.Empty,true);
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

        private static DataRow SetTruckData(DataTable dt, DataRow dr, DataRow dr0,int type=0)
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
            if (parm.ContainsKey("nw"))
                cw = Helper.GetDecimalValue(parm["nw"]);
            if (cw <= 0)
                cw = Helper.GetCW(Helper.GetQty("KG", parm), Helper.GetDecimalValue(parm["cbm"]), 5000M, "E");

            Dictionary<string, decimal> map = new Dictionary<string, decimal>();
            for (int i = 11; i <= 50; i++)
            {
                map["F" + i] = -(0.5m + (i - 11) * 0.5m);
            }
            map["F7"] = 300;
            map["F6"] = 200;
            map["F5"] = 100;
            map["F4"] = 50;
            map["F3"] = 40;
            map["F2"] = 30;
            map["F1"] = 20;
            decimal pre_total = 0m;

            foreach (DataRow dr in dt.Rows)
            {
                string polCd = Prolink.Math.GetValueAsString(dr["POL_CD"]);//F2:20'  F3:40'  F4:40HQ
                if (string.IsNullOrEmpty(polCd))
                    continue;
                string lspCd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                int limit = 20;
                switch (lspCd)
                {
                    case "0008910019":
                        if (map.ContainsKey("F1"))
                            map.Remove("F1");
                        limit = 30;
                        break;
                    default:
                        if (!map.ContainsKey("F1"))
                            map.Add("F1", 20);
                        limit = 20;
                        break;
                }
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
                                msg.Add(string.Format("{0}>={1}({2}{3}){4}", cw, val, price, Helper.GetLoalCurName(cur), Helper.GetEexpressGw(cw)));
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
                    curTotal = price * Helper.GetEexpressGw(cw);
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
        #endregion

        #region 生成账单明细、log和相关报价对象
        /// <summary>
        /// 原始账单细档
        /// </summary>
        DataTable _smbidDt = null;
        /// <summary>
        /// 原始账单主档
        /// </summary>
        DataTable _smbimDt = null;
        private static string _shipment_id = string.Empty;
        string _boundType = "O";
        string _table = "SMSM";
        string _not_status = "'V','Z','A'";
        public void Share(string shipment_uid, string boundType = "O")
        {
            _boundType = boundType;
            Init();
            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE U_ID={0}", SQLUtils.QuotedStr(shipment_uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count <= 0) return;
            DataRow smsm = smDt.Rows[0];
            string cmp = Prolink.Math.GetValueAsString(smsm["CMP"]);
            string group_id = Prolink.Math.GetValueAsString(smsm["GROUP_ID"]);
            string tranMode = Prolink.Math.GetValueAsString(smsm["TRAN_TYPE"]);
            string shipment_id = Prolink.Math.GetValueAsString(smsm["SHIPMENT_ID"]);
            string combine_info = Prolink.Math.GetValueAsString(smsm["COMBINE_INFO"]);
            string shipment_info = Prolink.Math.GetValueAsString(smsm["SHIPMENT_INFO"]);


            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);

            DataTable delSmDt = null;
            string delete_sql = string.Empty;
            foreach (string dnNo in dns)
            {
                if (delete_sql.Length > 0) delete_sql += " UNION ";
                delete_sql += string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE COMBINE_INFO LIKE {0}", SQLUtils.QuotedStr("%" + dnNo + "%"));
            }

            //if(dns.Length<=0)return;
            string sql = string.Empty;
            foreach (string dnNo in dns)
            {
                if (sql.Length > 0) sql += " UNION ";
                sql += string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE STATUS NOT IN (" + _not_status + ") AND COMBINE_INFO LIKE {0}", SQLUtils.QuotedStr("%" + dnNo + "%"));
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

            DataTable dnDt = null;
            if (dns.Length > 0)
                dnDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMDN WHERE DN_NO IN {0}", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            else
                dnDt = OperationUtils.GetDataTable("SELECT * FROM SMDN WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable dnpDt = null;
            if (dns.Length > 0)
                dnpDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMDNP WHERE DN_NO IN {0} AND CATEGORY<>'TANN'", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            else
                dnpDt = OperationUtils.GetDataTable("SELECT * FROM SMDNP WHERE 1=0", null, Prolink.Web.WebContext.GetInstance().GetConnection());

            #region 删除多余的数据
            if (delSmDt != null)
            {
                MixedList ml1 = new MixedList();
                List<string> smList1 = new List<string>();
                foreach (DataRow dr in delSmDt.Rows)
                {
                    string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    if (!smList1.Contains(sm_id))
                        smList1.Add(sm_id);

                }
                if (smList1.Count > 0)
                {
                    ml1.Add(string.Format("DELETE FROM SMBID_TEMP WHERE SHIPMENT_ID IN {0}", Helper.JoinString(smList1.ToArray())));
                    ml1.Add(string.Format("DELETE FROM SMBID WHERE U_FID IS NULL AND (BAMT=0 OR BAMT IS NULL) AND SHIPMENT_ID IN {0}", Helper.JoinString(smList1.ToArray())));
                    OperationUtils.ExecuteUpdate(ml1, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
            }
            #endregion

            MixedList ml = new MixedList();
            List<string> smList = new List<string>();
            List<string> checkList = new List<string>();
            foreach (DataRow dr in smDt.Rows)
            {
                //STATUS NOT IN ('V','Z','A')
                //string status = Prolink.Math.GetValueAsString(dr["STATUS"]);
                //switch (status)
                //{
                //    case "V":
                //    case "Z":
                //    case "A":
                //        continue;
                //}
                string uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (checkList.Contains(uid)) continue;
                checkList.Add(uid);
                string sm_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                if (!smList.Contains(sm_id))
                    smList.Add(sm_id);
                CreateByShipmentId(uid, DateTime.Now, ml, dr);
            }

            int[] result = null;
            if (ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smList.Count <= 0)
                return;

            if ("I".Equals(_boundType))//进口不需要分摊
                return;
            ml = new MixedList();
            DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.* FROM SMBID WHERE SHIPMENT_ID IN {0}", Helper.JoinString(smList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.* FROM SMBID WHERE SHIPMENT_ID IN {0} AND EXISTS (SELECT 1 FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID AND SMBIM.VOID_FLAG IS NULL)",Helper.JoinString(smList.ToArray())), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            Dictionary<string, decimal> smamt_cost_dic = new Dictionary<string, decimal>();
            Dictionary<string, decimal> smamt_frt_dic = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dnamt_dic = new Dictionary<string, decimal>();

            EditInstruct oth_ei = null;
            #region 加总账单
            decimal rate = 0;
            foreach (DataRow bid in smbidDt.Rows)
            {
                string sm_id = Prolink.Math.GetValueAsString(bid["SHIPMENT_ID"]);
                decimal amt = Prolink.Math.GetValueAsDecimal(bid["QLAMT"]);
                if (!smamt_cost_dic.ContainsKey(sm_id))
                    smamt_cost_dic[sm_id] = 0m;
                smamt_cost_dic[sm_id] += amt;

                //ot_cost += ei.GetValueAsDecimal("QLAMT");
                //freight_amt += ei.GetValueAsDecimal("QAMT");
                if ("F".Equals(Prolink.Math.GetValueAsString(bid["CHG_TYPE"])))
                {
                    amt = Prolink.Math.GetValueAsDecimal(bid["QAMT"]);
                    if (!smamt_frt_dic.ContainsKey(sm_id)) smamt_frt_dic[sm_id] = 0m;
                    decimal temp = 0;
                    bool error = false;
                    rate = Helper.GetTotal(GetRate(smDt, sm_id), null, amt, Prolink.Math.GetValueAsString(bid["QCUR"]), ref temp, ref error, "USD");
                    smamt_frt_dic[sm_id] += temp;
                }
            }
            #endregion

            #region 运费、物流成本回写SMSM SMDN
            foreach (var kv in smamt_cost_dic)
            {
                decimal v1 = kv.Value;
                decimal v2 = 0m;
                if (smamt_frt_dic.ContainsKey(kv.Key))
                    v2 = smamt_frt_dic[kv.Key];
                //if (v1 <= 0M && v2 <= 0M)
                //    continue;
                oth_ei = new EditInstruct(_table, EditInstruct.UPDATE_OPERATION);
                oth_ei.PutKey("SHIPMENT_ID", kv.Key);
                oth_ei.Put("OF_COST", v1);//预提总运费
                //oth_ei.Put("LSP_COST", v1);
                oth_ei.Put("FREIGHT_AMT", v2);
                ml.Add(oth_ei);
            }

            _shipment_id = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt";

            checkList.Clear();
            foreach (DataRow dn in dnDt.Rows)
            {
                string dnNo = Prolink.Math.GetValueAsString(dn["DN_NO"]);

                if (checkList.Contains(dnNo)) continue;
                checkList.Add(dnNo);
                decimal dncbm = Prolink.Math.GetValueAsDecimal(dn["CBM"]);
                DataRow[] drs = smDt.Select(string.Format("COMBINE_INFO LIKE {0}", SQLUtils.QuotedStr("%" + dnNo + "%")));
                decimal amt = 0;
                decimal cost_amt = 0;
                foreach (DataRow dr0 in drs)
                {
                    decimal amt1 = 0;
                    decimal cost_amt1 = 0;

                    string sm_id = Prolink.Math.GetValueAsString(dr0["SHIPMENT_ID"]);
                    decimal cbm = GetDnAllCbm(dnDt, dr0);
                    //DataTable dnDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMDN WHERE DN_NO IN {0}", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (smamt_frt_dic.ContainsKey(sm_id))
                    {
                        if (dncbm < cbm)
                            amt1 = dncbm / cbm * smamt_frt_dic[sm_id];
                        else
                            amt1 = smamt_frt_dic[sm_id];
                        amt += amt1;
                    }

                    if (smamt_cost_dic.ContainsKey(sm_id))
                    {
                        if (dncbm < cbm)
                            cost_amt1 = dncbm / cbm * smamt_cost_dic[sm_id];
                        else
                            cost_amt1 = smamt_cost_dic[sm_id];
                        cost_amt += cost_amt1;
                    }
                    WriteLog(string.Format("{0}运费({1})分摊{2}运费{3}({4}/{5}){6}", shipment_id,
                        smamt_frt_dic.ContainsKey(sm_id) ? smamt_frt_dic[sm_id] : 0, dnNo, amt1, dncbm, cbm, rate));
                    WriteLog(string.Format("{0}物流成本({1})分摊{2}物流成本{3}({4}/{5}){6}", shipment_id,
                        smamt_cost_dic.ContainsKey(sm_id) ? smamt_cost_dic[sm_id] : 0, dnNo, cost_amt1, dncbm, cbm, rate));
                }


                oth_ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                oth_ei.PutKey("U_ID", Prolink.Math.GetValueAsString(dn["U_ID"]));
                oth_ei.Put("FREIGHT_AMT", amt);
                oth_ei.Put("COST", cost_amt);
                ml.Add(oth_ei);

                drs = dnpDt.Select(string.Format("DN_NO={0}", SQLUtils.QuotedStr(dnNo)));
                decimal dnpSum = 0M;
                foreach (DataRow dnp in drs)
                {
                    dnpSum += Prolink.Math.GetValueAsDecimal(dnp["VALUE1"]);
                }

                foreach (DataRow dnp in drs)
                {
                    decimal val = Prolink.Math.GetValueAsDecimal(dnp["VALUE1"]);
                    oth_ei = new EditInstruct("SMDNP", EditInstruct.UPDATE_OPERATION);
                    oth_ei.PutKey("U_ID", Prolink.Math.GetValueAsString(dnp["U_ID"]));
                    decimal qty = Prolink.Math.GetValueAsDecimal(dnp["QTY"]);
                    if (qty == 0) qty = 1;
                    if (dnpSum == 0)
                        oth_ei.Put("COST_PRICE", cost_amt / drs.Length / qty);
                    else
                        oth_ei.Put("COST_PRICE", cost_amt * val / dnpSum / qty);
                    ml.Add(oth_ei);
                }
            }
            if (ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 分摊DN FOB
            //if (dns.Length > 0)
            //    dnDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMDN WHERE DN_NO IN {0}", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //ml = new MixedList();
            //foreach (DataRow dn in dnDt.Rows)
            //{
            //    GetFOB(dn, ml);
            //}

            //if (ml.Count > 0)
            //    result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion
        }

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
        /// 创建账单
        /// </summary>
        /// <param name="shipment_uid"></param>
        /// <param name="billDate"></param>
        /// <param name="share"></param>
        /// <param name="ml"></param>
        /// <param name="smsm"></param>
        public void Create(string shipment_uid, DateTime billDate, bool share = true, MixedList ml = null, DataRow smsm = null, string boundType="O")
        {
            if (share)
                Share(shipment_uid, boundType);
            else
                CreateByShipmentId(shipment_uid, billDate, ml, smsm, boundType);

        }

        DataRow[] _partySH = null;
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
            billDate=GetBillDate(billDate, smsm);
            string shipment_id = Prolink.Math.GetValueAsString(smsm["SHIPMENT_ID"]);
            _shipment_id = shipment_id;
            WriteLogTagStart(string.Format("开始{0}计算,ETD:{1}", tranMode, billDate.ToString("yyyy-MM-dd")));

            string sql = string.Empty;
            #region 获取DN party SMSM  已开立账单
            string combine_info = Prolink.Math.GetValueAsString(smsm["COMBINE_INFO"]);
            string shipment_info = Prolink.Math.GetValueAsString(smsm["SHIPMENT_INFO"]);
            List<string> smList = Helper.SplitToList(shipment_info);
            //string partySql = "SELECT * FROM SMSMPT WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            string partySql = string.Format("SELECT * FROM (SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0}) A OUTER APPLY (SELECT TOP 1 HEAD_OFFICE,(SELECT TOP 1 PARTY_NAME FROM SMPTY C WHERE C.PARTY_NO=SMPTY.HEAD_OFFICE) AS HEAD_NAME FROM SMPTY WHERE SMPTY.PARTY_NO=A.PARTY_NO) B", SQLUtils.QuotedStr(shipment_id));
            if (!smList.Contains(shipment_id))
                smList.Add(shipment_id);
            if (smList.Count > 1)
            {
                sql = "SELECT * FROM SMDN WHERE SHIPMENT_ID IN " + Helper.JoinString(smList.ToArray());
            }
            else
            {
                sql = "SELECT * FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            }
            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (dns.Length > 0)
                sql += " UNION SELECT * FROM SMDN WHERE DN_NO IN " + Helper.JoinString(dns);
            WriteLog("party SQL:" + partySql);
            DataTable partyDt = OperationUtils.GetDataTable(partySql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            _smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE SHIPMENT_ID={0} ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            _smbimDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE SHIPMENT_ID={0} AND VOID_FLAG IS NULL", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //_smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE SHIPMENT_ID={0} AND EXISTS (SELECT 1 FROM SMBIM WHERE SMBIM.U_ID=SMBID.U_FID AND SMBIM.VOID_FLAG IS NULL)", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            //_smbimDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE SHIPMENT_ID={0} AND VOID_FLAG IS NULL", SQLUtils.QuotedStr(shipment_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 账单生成条件
            string cent_decl = Prolink.Math.GetValueAsString(smsm["CENT_DECL"]);//统报  Y/N
            //计费数量

            decimal dnNum = dns.Length;
            decimal Pcnt20 = Helper.GetValueAsDecimal(smsm, new string[] { "PCNT20" });//20'
            decimal Pcnt40 = Helper.GetValueAsDecimal(smsm, new string[] { "PCNT40" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT40"]);//40'
            decimal Pcnt40Hq = Helper.GetValueAsDecimal(smsm, new string[] { "PCNT40HQ" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT40HQ"]);//40'HQ
            string PcntType = Helper.GetValueAsString(smsm, new string[] { "PCNT_TYPE" });// Prolink.Math.GetValueAsString(smsm["PCNT_TYPE"]);//其他货柜
            decimal PcntNumber = Helper.GetValueAsDecimal(smsm, new string[] { "PCNT_NUMBER" }); //Prolink.Math.GetValueAsDecimal(smsm["PCNT_NUMBER"]);//其他货柜数量
            decimal PkgNum = Prolink.Math.GetValueAsDecimal(smsm["PKG_NUM"]);//件数
            string PkgUnit = Prolink.Math.GetValueAsString(smsm["PKG_UNIT"]);//件数单位
            decimal Gw = Helper.GetValueAsDecimal(smsm, new string[] { "GW", "PGW" }); //Prolink.Math.GetValueAsDecimal(smsm["PGW"]);//GW 毛重
            string Gwu = Prolink.Math.GetValueAsString(smsm["GWU"]);//毛重单位GWU
            decimal Cbm = Helper.GetValueAsDecimal(smsm, new string[] { "CBM", "PCBM" }); // Prolink.Math.GetValueAsDecimal(smsm["PCBM"]);//CBM体积
            string qtyu = Prolink.Math.GetValueAsString(smsm["QTYU"]);
            decimal qty = Prolink.Math.GetValueAsDecimal(smsm["QTY"]);

            //起运地 目的地
            string pol = Helper.GetValueAsString(smsm, new string[] { "POL_CD", "PPOL_CD" });// Prolink.Math.GetValueAsString(smsm["PPOL_CD"]);
            //string pod = Prolink.Math.GetValueAsString(smsm["PPOD_CD"]);
            string pod = Helper.GetValueAsString(smsm, new string[] { "DEST_CD", "PDEST_CD" });//Prolink.Math.GetValueAsString(smsm["PDEST_CD"]);
            //PDEST


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
            //switch (tranMode)
            //{
            //    case "R":
            //        qtTranMode = "F";
            //        break;
            //}

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

            //if ("F".Equals(tranMode))
            //{
            //    if (!string.IsNullOrEmpty(LoadingFrom))
            //        qt_condition += " AND LOADING_FROM=" + SQLUtils.QuotedStr(LoadingFrom);
            //    if (!string.IsNullOrEmpty(LoadingTo))
            //        qt_condition += " AND LOADING_TO=" + SQLUtils.QuotedStr(LoadingTo);
            //}

            //询价条件
            string rq_condition = string.Format(" AND SMRQM.EFFECT_FROM<={0} AND SMRQM.EFFECT_TO>={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            string qtdate_condition = string.Format(" AND SMQTM.EFFECT_FROM<={0} AND SMQTM.EFFECT_TO>={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            if (!string.IsNullOrEmpty(qtTranMode))
                rq_condition += " AND SMRQM.TRAN_MODE=" + SQLUtils.QuotedStr(qtTranMode);


            //if (!string.IsNullOrEmpty(IncotermCd))
            //    rq_condition += " AND SMRQM.INCOTERM=" + SQLUtils.QuotedStr(IncotermCd);

            //if (!string.IsNullOrEmpty(FrtTerm))
            //    rq_condition += " AND SMRQM.FREIGHT_TERM=" + SQLUtils.QuotedStr(FrtTerm);

            string bu_condition = string.Format("SMFSC.EFFECT_DATE<={0} AND SMFSC.CARRIER={1}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(Carrier));
            #endregion

            #region 初始化参数
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["decl_num"] = decl_num;
            parm["next_num"] = next_num;
            parm["CONT_DECL_NUM"] = Prolink.Math.GetValueAsDecimal(smsm["CONT_DECL_NUM"]);//续单数
            
            parm["Cnt20"] = Pcnt20;
            parm["Cnt40"] = Pcnt40;
            parm["Cnt40hq"] = Pcnt40Hq;

            decimal cntNum = Pcnt20 + Pcnt40 + Pcnt40Hq;
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
            string bandType = Prolink.Math.GetValueAsString(smsm["BAND_TYPE"]);
            parm["bandType"] = bandType;//绕物流园区
            parm["WMS"] = Prolink.Math.GetValueAsString(smsm["EXTERNAL_WMS"]);//外仓
            parm["Gvalue"] = Prolink.Math.GetValueAsDecimal(smsm["GVALUE"]);//货值

            parm["nw"] = Prolink.Math.GetValueAsDecimal(smsm["CW"]);

            string combine_other = Prolink.Math.GetValueAsString(smsm["COMBINE_OTHER"]);//外廠併櫃
            parm["cout"] = combine_other;
            parm["telex_rls"] = Prolink.Math.GetValueAsString(smsm["TELEX_RLS"]);//电放
            parm["battery"] = Prolink.Math.GetValueAsString(smsm["BATTERY"]);//喇叭
            parm["horn"] = Prolink.Math.GetValueAsString(smsm["HORN"]);//離電池
            parm["region"] = Prolink.Math.GetValueAsString(smsm["REGION"]);//区域
            parm["customs_check"] = Prolink.Math.GetValueAsString(smsm["CUSTOMS_CHECK"]);//是否查驗
            //parm["customs_check"] = "";//是否查驗
            parm["pod"] = pod;
            string bl_type = Prolink.Math.GetValueAsString(smsm["BL_TYPE"]);
            string iscombine_bl = Prolink.Math.GetValueAsString(smsm["ISCOMBINE_BL"]);

            parm["BrgType"] = smsm.Table.Columns.Contains("BRG_TYPE") ? Prolink.Math.GetValueAsString(smsm["BRG_TYPE"]) : string.Empty;//路桥方式
            parm["via"] = smsm.Table.Columns.Contains("VIA") ? Prolink.Math.GetValueAsString(smsm["VIA"]) : string.Empty;//via
            parm["is_land"] = smsm.Table.Columns.Contains("IS_LAND") ? Prolink.Math.GetValueAsString(smsm["IS_LAND"]) : string.Empty;//是否陆运

            parm["CntrStdQty"] = 0;
            parm["StdQty"] = 0;
            //获取标准装柜量

            DataTable dnDt = (dns.Length > 0) ?
               OperationUtils.GetDataTable(string.Format("SELECT GWU,DN_NO,GW,CBM FROM SMDN WHERE DN_NO IN {0} ORDER BY DN_NO", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection())
               : OperationUtils.GetDataTable("SELECT GWU,DN_NO,GW,CBM FROM SMDN WHERE 1=0 ORDER BY DN_NO", null, Prolink.Web.WebContext.GetInstance().GetConnection());
            parm["DnQtyDt"] = dnDt;
            WriteLog("DN Table数:" + dnDt.Rows.Count);

            string dnNo=Prolink.Math.GetValueAsString(smsm["DN_NO"]);
            if ("T".Equals(tranMode) && "A".Equals(CargoType))
            {
                DataTable dnpDt = null;
                if (dns.Length > 0)
                    dnpDt = OperationUtils.GetDataTable(string.Format("SELECT DN_NO,IPART_NO,QTY,CNTR_STD_QTY FROM SMDNP WHERE DN_NO IN {0} AND CATEGORY<>'TANN' ORDER BY DN_NO", Helper.JoinString(dns)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
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
                _chgDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMCHG WHERE GROUP_ID={1} AND CMP={2}", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(group_id), SQLUtils.QuotedStr(cmp)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 获取可能报价
            //sql = string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {1} AND RLOCATION={2}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUR AS M_CUR FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition, SQLUtils.QuotedStr(cmp));
            sql = string.Format("SELECT * FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUR AS M_CUR FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition);
            if ("L".Equals(tranMode))
                sql = string.Format("SELECT INCOTERM,QT_EFFECT_FROM,TRAN_MODE,RFQ_NO,QUOT_NO1 AS QUOT_NO,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1 AS LSP_NM,U_FID FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {2})) A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,QUOT_NO AS QUOT_NO1,LSP_NM AS LSP_NM1 FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID {1}) B  GROUP BY INCOTERM,TRAN_MODE,RFQ_NO,QUOT_NO1,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1,U_FID,QT_EFFECT_FROM ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition.Replace("SMRQM.", "SMQTM."), rq_condition);
            //sql = string.Format("SELECT INCOTERM,QT_EFFECT_FROM,TRAN_MODE,RFQ_NO,QUOT_NO1 AS QUOT_NO,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1 AS LSP_NM,U_FID FROM (SELECT * FROM SMQTD WHERE {0} AND EXISTS (SELECT RFQ_NO FROM SMRQM WHERE SMRQM.RFQ_NO=SMQTD.RFQ_NO {2} AND RLOCATION={3})) A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,QUOT_NO AS QUOT_NO1,LSP_NM AS LSP_NM1 FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID {1}) B  GROUP BY INCOTERM,TRAN_MODE,RFQ_NO,QUOT_NO1,POL_CD,POD_CD,POL_NM,POD_NM,CARRIER,LSP_CD,LSP_NM1,U_FID,QT_EFFECT_FROM ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", qt_condition, rq_condition.Replace("SMRQM.", "SMQTM."), rq_condition, SQLUtils.QuotedStr(cmp));
            WriteLog("报价SQL:" + sql);
            DataTable qtDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            #region 抓取费率
            string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE", rateFilter), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            WriteLog("费率SQL:" + string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND {0} ORDER BY EDATE", rateFilter));
            #endregion


            string dest = Helper.GetValueAsString(smsm, new string[] { "POD_CD", "PPOD_CD" });
            #region 抓取区域
            List<string> areList = new List<string>();
            areList.Add(SQLUtils.QuotedStr("ZZZZZ"));
            if (!string.IsNullOrEmpty(dest))
            {
                areList.Add(SQLUtils.QuotedStr(dest));
                if (dest.Length >= 2)
                {
                    areList.Add(SQLUtils.QuotedStr(dest.Substring(0, 2) + "ZZZ"));
                }
            }

            //if (!string.IsNullOrEmpty(pol))
            //{
            //    areList.Add(SQLUtils.QuotedStr(pol));
            //    if (pol.Length >= 2)
            //    {
            //        areList.Add(SQLUtils.QuotedStr(pol.Substring(0, 2) + "ZZZ"));
            //    }
            //}

            //if (!string.IsNullOrEmpty(pod))
            //{
            //    areList.Add(SQLUtils.QuotedStr(pod));
            //    if (pod.Length >= 2)
            //    {
            //        areList.Add(SQLUtils.QuotedStr(pod.Substring(0, 2) + "ZZZ"));
            //    }
            //}

            DataTable areDt = null;
            if (areList.Count > 0)
            {
                sql = string.Format("SELECT A.AREA,A.CARRIER,PORT FROM (SELECT AREA,CARRIER FROM BSCAAM WHERE CARRIER={1})A LEFT JOIN BSCAA ON A.AREA=BSCAA.AREA AND A.CARRIER=BSCAA.CARRIER WHERE (PORT IN ({0}) OR A.AREA='ZZ') AND A.CARRIER={1}", string.Join(",", areList), SQLUtils.QuotedStr(Carrier));
                WriteLog("区域SQL:" + sql);
                areDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            #endregion

            #region 抓取燃油附加费
            DataTable thcDt = null;
           
            DataRow[] thcRds = null;
            if ("F".Equals(qtTranMode))
            {
                //sql = string.Format("SELECT SMFSC.CUR,SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM,BSCAA.* FROM SMFSC LEFT JOIN BSCAA ON SMFSC.U_ID=BSCAA.U_FID WHERE {0} ORDER BY  EFFECT_DATE DESC", bu_condition);
                //sql = string.Format("SELECT SMFSC.FAREA,SMFSC.CUR,SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM FROM SMFSC WHERE {0} AND CARRIER={1} AND AREA={2} ORDER BY  EFFECT_DATE DESC", bu_condition, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(GetArea(dest, areDt)));
                List<string> areas = GetdQuoteAreaList(dest, areDt);
                sql = string.Format("SELECT SMFSC.AREA,SMFSC.FAREA,SMFSC.CUR,SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM FROM SMFSC WHERE {0} AND CARRIER={1} AND CMP={2} {3} ORDER BY  EFFECT_DATE DESC", bu_condition, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(cmp), string.Format(" AND AREA IN ({0})", string.Join(",", areas.ToArray())));

                thcDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (thcDt.Rows.Count > 0)
                    thcDt = GetAreaTHC(dest, areDt, thcDt, pol);
                WriteLog("THC SQL:" + sql);
                //if (thcDt.Rows.Count > 0)
                //    WriteLog("THC SQL:" + sql);
                //else
                //{
                //    sql = string.Format("SELECT SMFSC.AREA,SMFSC.FAREA,SMFSC.CUR,SMFSC.EFFECT_DATE,SMFSC.GP20,SMFSC.GP40,SMFSC.HQ40,SMFSC.OTH_RATE,SMFSC.CARRIER_NM,SMFSC.CMP_NM FROM SMFSC WHERE {0} AND CARRIER={1} AND CMP={2} AND (FAREA IS NULL OR FAREA='') {3} ORDER BY  EFFECT_DATE DESC", bu_condition, SQLUtils.QuotedStr(Carrier), SQLUtils.QuotedStr(cmp), string.Format(" AND AREA IN ({0})", string.Join(",", areas.ToArray())));
                //    thcDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                //    if (thcDt.Rows.Count > 0)
                //        thcDt = GetAreaTHC(dest, areDt, thcDt);
                //    WriteLog("THC SQL:" + sql);
                //}
                thcRds = thcDt.Select();
            }
            #endregion

            #region 获取取账单对象
            //Dictionary<string, string> ptList = new Dictionary<string, string> { { "SP", "Freight_Forwarder" }, { "FS", "Transportation_Party(Carrier)" }, { "CR", "Trailer" }, { "BR", "Broker" }, { "EX", "Express(EX)" }, { "XP", "Express(XP)" } };
            Dictionary<string, string> ptList = new Dictionary<string, string> { { "SP_CR", "Freight_Forwarder" }, { "CR", "Trailer" }, { "BR", "Broker(BR)" }, { "BC", "Broker(BC)" }, { "BM", "Broker(BM)" }, { "EX", "Express(EX)" }, { "XP", "Express(XP)" }, { "DF", "DF" } };

            List<string> d_to = GetDebitTo(tranMode, IncotermCd, FrtTerm, LoadingTo, cent_decl, bandType, TrackWay, bl_type, iscombine_bl, _boundType);
            WriteLog(string.Join("、", new string[] { "tranMode:" + tranMode, "IncotermCd:" + IncotermCd, "FrtTerm:" + FrtTerm, "LoadingTo:" + LoadingTo, "统报:" + cent_decl, "bl type:" + bl_type, "isCombine bl:" + iscombine_bl }));
            WriteLog("账单对象:" + string.Join("、", d_to.ToArray()));
            #endregion

            #region 运费计算
            DataTable othDt = null;
            List<string> msg = new List<string>();
            string sm_uid = Prolink.Math.GetValueAsString(smsm["U_ID"]);
            Dictionary<string, EditInstruct> mainEiMap = new Dictionary<string, EditInstruct>();
            _current_smsm = smsm;

            _partySH = Helper.GetParty(shipment_id, partyDt, new string[] { "SH" });
            foreach (var dtoType in d_to)
            {
                if ("O".Equals(_boundType) && "F".Equals(tranMode) && "EXW".Equals(IncotermCd))//FLC的EXW不产生任何费用
                    continue;
                #region 获取party类型
                List<KeyValuePair<string, string>> pts = new List<KeyValuePair<string, string>>();
                switch (dtoType)
                {
                    case "CarrierI"://货代/carrier
                    case "Carrier"://货代/carrier
                        pts = ptList.Where(a => "SP_CR".Equals(a.Key)).ToList();
                        break;
                    case "CR"://拖车
                    case "BR"://报关
                    case "BC"://报关
                    case "BM"://报关
                        pts = ptList.Where(a => dtoType.Equals(a.Key)).ToList();
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
                                case "F":
                                    partys = new string[] { "BO", "SP" };
                                    break;
                                case "T":
                                    partys = new string[] { "CR", "SP" };
                                    break;
                                default:
                                    partys = new string[] { "SP" };
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
                    //_current_debitno = System.Guid.NewGuid().ToString();
                    party_no = Prolink.Math.GetValueAsString(drs[0]["PARTY_NO"]);
                    party_nm = Prolink.Math.GetValueAsString(drs[0]["PARTY_NAME"]);
                    head_offices = Prolink.Math.GetValueAsString(drs[0]["HEAD_OFFICE"]);

                    _current_debitno = party_no;
                    string key = string.Format("{0}_{1}", pt.Key, party_no);
                    WriteLog("结账对象:" + party_nm);

                    EditInstruct m_ei = CreateBillEditInstruct(smsm, _current_debitno, party_no, party_nm, cmp, pt_type);
                    mainEiMap[_current_debitno] = m_ei;
                    Dictionary<string, object> schems = null;
                    List<string> chg_fsList = new List<string>();
                    switch (dtoType)
                    {
                        case "CarrierI"://货代/carrier
                            switch (tranMode)
                            {
                                case "F":
                                    pt_qtDt = Helper.CloneQTTableByType(shipment_id, partyDt, new string[] { "FS", "SP" }, qtDt, tranMode, IncotermCd, LoadingFrom, LoadingTo);//运费
                                    break;
                                default:
                                    pt_qtDt = Helper.CloneQTTableByPartyNos(shipment_id, partyDt, new string[] { party_no, head_offices }, qtDt, tranMode, IncotermCd);//运费
                                    break;
                            }
                            FreightCalculat(parm, pt_qtDt, null, rateDt, d_to, thcDt);

                            break;
                        case "Carrier"://货代/carrier
                            DataTable localDt = null;
                            DataTable qtlocalDt = null;
                            string local_sql = string.Format("SELECT * FROM(SELECT IS_SHARE,[U_ID],[RFQ_NO],[QUOT_NO],[QUOT_TYPE],[SEQ_NO],[TRAN_MODE],[OUT_IN],[LSP_CD],[EFFECT_DATE],[EFFECT_TO],[TRAN_TYPE],[REGION],[STATE],[POD_CD],[POD_NM],[POL_CD],[POL_NM],[VIA_CD],[VIA_NM],[CARRIER],[ALL_IN],[CUR],F1,F2,F3,F4,F5,F6,F7,F8,F9,[PUNIT],[CHG_CD],[CHG_DESCP],[CHG_TYPE],[SAILING_DAY],[FREE_ODT],[FREE_ODM],[FREE_DDT],[FREE_DDM],[TT],[NOTE],[REMARK],[MIN_AMT],[U_FID],[SERVICE_MODE],[LOADING_FROM],[LOADING_TO],[CUT_OFF],[ETD],[REPAY] FROM SMQTD WHERE TRAN_MODE='X'  AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.POL_CD={0} AND SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='X' AND RLOCATION={1} {2} AND TRAN_TYPE={3}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD,CUR AS M_CUR,INCOTERM,CREDIT_TO,CREDIT_NM,FREIGHT_TERM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(cmp), qtdate_condition, SQLUtils.QuotedStr(tranMode));

                            if ("F".Equals(tranMode))
                            {
                                localDt = OperationUtils.GetDataTable(local_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if ("P".Equals(FrtTerm))
                                {
                                    pt_qtDt = Helper.CloneQTTableByType(shipment_id, partyDt, new string[] { "FS", "SP" }, qtDt, tranMode, IncotermCd, LoadingFrom, LoadingTo);//运费
                                    qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "BO", "SP" }, localDt, tranMode, "P", "", "", true);//运费
                                }
                                else
                                {
                                    pt_qtDt = qtDt.Clone();
                                    qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "BO", "SP" }, localDt, tranMode, "C", "", "", true);//运费
                                }

                                FreightCalculat(parm, pt_qtDt, null, rateDt, d_to, thcDt);
                            }
                            else if ("P".Equals(FrtTerm))
                            {
                                pt_qtDt = Helper.CloneQTTableByPartyNos(shipment_id, partyDt, new string[] { party_no, head_offices }, qtDt, tranMode, IncotermCd);//运费
                                //pt_qtDt = Helper.CloneQTTable(qtDt, party_no, tranMode, IncotermCd);//运费
                                //pt_qtDt = Helper.CloneQTTable(qtDt, party_no, tranMode, IncotermCd);//运费
                                FreightCalculat(parm, pt_qtDt, null, rateDt, d_to, thcDt);

                                if (("E".Equals(tranMode)) && "DDP".Equals(IncotermCd))
                                {
                                    localDt = OperationUtils.GetDataTable(local_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "SP" }, localDt, tranMode, FrtTerm, "", "", true);//运费
                                }
                            }
                            else if (("R".Equals(tranMode) || "L".Equals(tranMode) || "A".Equals(tranMode)) && !"EXW".Equals(IncotermCd))
                            {
                                localDt = OperationUtils.GetDataTable(local_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "SP" }, localDt, tranMode, FrtTerm, "", "", true);//运费
                            }
                            chg_fsList.Clear();
                            if (qtlocalDt != null)//计算local费用
                            {
                                WriteLog("local费用SQL:" + local_sql);
                                LocalFreight(qtlocalDt, parm, rateDt, d_to, partyDt, shipment_id, chg_fsList, (thcRds != null && thcRds.Length > 0) ? thcRds[0] : null);
                            }

                            if ("F".Equals(tranMode) && chg_fsList.Count > 0)
                            {
                                if (othDt == null)
                                    othDt = GetOthCharge(Carrier, cmp, pol, qtdate_condition);//船公司费用
                                CarrierFreight(othDt, parm, rateDt, d_to, chg_fsList, new List<string> { "SP" }, pol);
                            }
                            
                            #region DF party
                            qtlocalDt = null;
                            if ("F".Equals(tranMode) || "L".Equals(tranMode))
                            {
                                if (localDt == null)
                                    localDt = OperationUtils.GetDataTable(local_sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "DF" }, localDt, tranMode, FrtTerm, "", "", true);//运费
                            }
                            chg_fsList.Clear();
                            if (qtlocalDt != null)//计算local费用
                            {
                                WriteLog("DF local费用SQL:" + local_sql);
                                LocalFreight(qtlocalDt, parm, rateDt, d_to, partyDt, shipment_id, chg_fsList, null);
                            }

                            if ("F".Equals(tranMode) && chg_fsList.Count > 0)
                            {
                                if (othDt == null)
                                    othDt = GetOthCharge(Carrier, cmp, pol, qtdate_condition);//船公司费用
                                CarrierFreight(othDt, parm, rateDt, d_to, chg_fsList, new List<string> { "DF" }, pol);
                            }
                            #endregion

                            break;
                        case "BR"://报关
                        case "BC"://报关
                        case "BM"://报关
                            sql = string.Format("SELECT * FROM(SELECT * FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='B' AND SMQTM.RLOCATION={0} AND SMQTM.POL_CD={1} AND SMQTM.LSP_CD={3} {2} AND SMQTM.TRAN_TYPE={4}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pol), qtdate_condition, SQLUtils.QuotedStr(party_no), SQLUtils.QuotedStr(tranMode));
                            WriteLog("报关SQL:" + sql);
                            DataTable brokerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ////MIN_AMT F1    F2    F3  F4  F5  F6  
                            schems = BrokerFreight(brokerDt, parm, rateDt);

                            //m_ei.Put("REMARK", "报关账单:" + pt);
                            //MergeBillEditInstruct(ml, schems, m_ei, pt_type);
                            break;
                        case "CR"://拖车
                            //switch (tranMode)
                            //{
                            //    case "L":
                            //    case "A":
                            //        DataRow[] sps = Helper.GetParty(shipment_id, partyDt, new string[] { "SP" });
                            //        DataRow[] crs = Helper.GetParty(shipment_id, partyDt, new string[] { "CR" });
                            //        if (sps.Length > 0 && crs.Length > 0 &&
                            //            Prolink.Math.GetValueAsString(sps[0]["PARTY_NO"]).Equals(Prolink.Math.GetValueAsString(crs[0]["PARTY_NO"])))
                            //        {
                            //            continue;
                            //        }
                            //        sql = string.Format("SELECT * FROM(SELECT IS_SHARE,[U_ID],[RFQ_NO],[QUOT_NO],[QUOT_TYPE],[SEQ_NO],[TRAN_MODE],[OUT_IN],[LSP_CD],[EFFECT_DATE],[EFFECT_TO],[TRAN_TYPE],[REGION],[STATE],[POD_CD],[POD_NM],[POL_CD],[POL_NM],[VIA_CD],[VIA_NM],[CARRIER],[ALL_IN],[CUR],F1,F2,F3,F4,F5,F6,F7,F8,F9,[PUNIT],[CHG_CD],[CHG_DESCP],[CHG_TYPE],[SAILING_DAY],[FREE_ODT],[FREE_ODM],[FREE_DDT],[FREE_DDM],[TT],[NOTE],[REMARK],[MIN_AMT],[U_FID],[SERVICE_MODE],[LOADING_FROM],[LOADING_TO],[CUT_OFF],[ETD],[REPAY] FROM SMQTD WHERE TRAN_MODE='X'  AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.POL_CD={0} AND SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='X' AND RLOCATION={1} {2} AND TRAN_TYPE={3}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD,CUR AS M_CUR,INCOTERM,CREDIT_TO,CREDIT_NM,FREIGHT_TERM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(pol), SQLUtils.QuotedStr(cmp), qtdate_condition, SQLUtils.QuotedStr(tranMode));
                            //        localDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //        qtlocalDt = Helper.CloneQTTableByTypeOnlyowner(shipment_id, partyDt, new string[] { "CR" }, localDt, tranMode, FrtTerm, "", "", true);//运费

                            //        chg_fsList.Clear();
                            //        WriteLog(tranMode + "卡车SQL:" + sql);
                            //        LocalFreight(qtlocalDt, parm, rateDt, d_to, partyDt, shipment_id, chg_fsList, null);
                            //        continue;
                            //}
                            sql = string.Format("SELECT * FROM(SELECT *,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID) M_CUR FROM SMQTD WHERE POL_CD={0} AND POD_CD={1} AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='C' AND LSP_CD={3} {2} AND SMQTM.TRAN_TYPE={4}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(pol), qtdate_condition, SQLUtils.QuotedStr(party_no), SQLUtils.QuotedStr(tranMode));
                            WriteLog("拖车SQL:" + sql);
                            DataTable trailerDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            chg_fsList.Clear();
                            schems = TrailerFreight(trailerDt, parm, rateDt, chg_fsList);
                            if ("F".Equals(tranMode) && chg_fsList.Count > 0)
                            {
                                if (othDt == null)
                                    othDt = GetOthCharge(Carrier, cmp, pol, qtdate_condition);//船公司费用
                                CarrierFreight(othDt, parm, rateDt, d_to, chg_fsList, new List<string> { "CR" }, pol);
                            }
                            //m_ei.Put("REMARK", "拖车账单:" + pt);
                            //MergeBillEditInstruct(ml, schems, m_ei, pt_type);
                            break;
                    }
                }
            }
            decimal tcbm = Prolink.Math.GetValueAsDecimal(smsm["TCBM"]);

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

            //设置订舱者
//            if (!string.IsNullOrEmpty(shipment_id))
//            {
//                ml.Add(string.Format(@"UPDATE SMBID SET BOOKING_BY=
//(SELECT TOP 1 CASE WHEN CHARINDEX(' ',BL_WIN)>0 THEN SUBSTRING(BL_WIN,1,CHARINDEX(' ',BL_WIN)-1) ELSE BL_WIN END FROM SMSM WHERE SMSM.SHIPMENT_ID=SMBID.SHIPMENT_ID)
//WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipment_id)));
//            }
            #endregion

            #region 保存数据
            int[] result = null;
            if (updateDataFlag && ml.Count > 0)
                result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            #endregion

            WriteLogTagStart(string.Format("结束{0}计算", tranMode));
        }

        #region 获取THC费用
        private DataTable GetAreaTHC(string dest, DataTable areDt, DataTable thcDt,string pol)
        {
            var result = thcDt.Clone();
            if (thcDt.Rows.Count > 0)
            {
                DataTable firstDt = GetFirstTHCRow(thcDt, thcDt.Rows[0]);
                if (thcDt.Rows.Count == 1)
                    return firstDt;
                thcDt = firstDt;
            }

            ImportTHCRow(thcDt, result, null,pol);
            if(result.Rows.Count==1)
                return result;
            else if (result.Rows.Count > 1)
            {
                thcDt = result;
                result = thcDt.Clone();
            }

            List<string> ares = new List<string>();
            DataRow[] drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest)));
            ares = GetArea(drs);
            ImportTHCRow(thcDt, result, ares);
            //AddArea(ares, drs);
            if (dest.Length >= 2)
            {
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest.Substring(0, 2) + "ZZZ")));
                //AddArea(ares, drs);
                ares = GetArea(drs);
                ImportTHCRow(thcDt, result, ares);
            }
            //ImportTHCRow(thcDt, result, ares);
            drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr("ZZZZZ")));
            ares = GetArea(drs);
            ImportTHCRow(thcDt, result, ares);

            //AddArea(ares, drs);
            //ares.Add("ZZ");
            ares = new List<string> { "ZZ" };
            ImportTHCRow(thcDt, result, ares);
            return result;
        }

        private static DataTable GetFirstTHCRow(DataTable thcDt, DataRow org)
        {
            DataTable result = thcDt.Clone();
            string org_effect_date = Prolink.Math.GetValueAsString(org["EFFECT_DATE"]);
            foreach (DataRow dr in thcDt.Rows)
            {
                try
                {
                    string effect_date = Prolink.Math.GetValueAsString(dr["EFFECT_DATE"]);
                    if (org_effect_date.Equals(effect_date))
                        result.ImportRow(dr);
                }
                catch { }
            }
            return result;
        }

        private static void ImportTHCRow(DataTable thcDt, DataTable result, List<string> ares,string pol="")
        {
            foreach (DataRow dr in thcDt.Rows)
            {
                try
                {
                    string farea = Prolink.Math.GetValueAsString(dr["FAREA"]);
                    string are = Prolink.Math.GetValueAsString(dr["AREA"]);
                    if (!string.IsNullOrEmpty(pol) && pol.Equals(farea))
                        result.ImportRow(dr);
                    else if (ares != null && ares.Contains(are))
                        result.ImportRow(dr);
                }
                catch { }
            }
        }

        private static List<string> GetArea(DataRow[] drs)
        {
            List<string> ares = new List<string>();
            foreach (DataRow dr in drs)
            {
                string are = Prolink.Math.GetValueAsString(dr["AREA"]);
                if (string.IsNullOrEmpty(are) || ares.Contains(are))
                    continue;
                ares.Add(are);
            }
            return ares;
        }

        private static void AddQuotedArea(List<string> ares, DataRow[] drs)
        {
            foreach (DataRow dr in drs)
            {
                string are = Prolink.Math.GetValueAsString(dr["AREA"]);
                if (string.IsNullOrEmpty(are))
                    continue;
                are = Prolink.Data.SQLUtils.QuotedStr(are);
                if (ares.Contains(are))
                    continue;
                ares.Add(are);
            }
        }

        private static void AddArea(List<string> ares, DataRow[] drs)
        {
            foreach (DataRow dr in drs)
            {
                string are = Prolink.Math.GetValueAsString(dr["AREA"]);
                if (string.IsNullOrEmpty(are) || ares.Contains(are))
                    continue;
                ares.Add(are);
            }
        }

        private static List<string> GetdQuoteAreaList(string dest, DataTable areDt)
        {
            List<string> ares = new List<string>();
            ares.Add("'ZZ'");
            if (string.IsNullOrEmpty(dest) || areDt == null)
                return ares;
            DataRow[] drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest)));
            AddQuotedArea(ares, drs);
            if (dest.Length >= 2)
            {
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest.Substring(0, 2) + "ZZZ")));
                AddQuotedArea(ares, drs);
            }
            drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr("ZZZZZ")));
            AddQuotedArea(ares, drs);
            return ares;
        }

        private DataTable GetAreaTHC1(string dest, DataTable areDt, DataTable thcDt)
        {
            //if (string.IsNullOrEmpty(dest) || areDt == null)
            //    return thc.Clone();
            DataRow[] thcs = thcDt.Select("1=0");
            DataRow[] drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest)));
            thcs = GetTHCRow(thcDt, drs, thcs);

            if (dest.Length >= 2 && thcs.Length <= 0)
            {
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest.Substring(0, 2) + "ZZZ")));
                thcs = GetTHCRow(thcDt, drs, thcs);
            }
            if (thcs.Length <= 0)
            {
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr("ZZZZZ")));
                thcs = GetTHCRow(thcDt, drs, thcs);
            }

            if (thcs.Length <= 0)
            {
                //drs = areDt.Select(string.Format("AREA={0}", SQLUtils.QuotedStr("ZZ")));
                thcs = GetTHCRow(thcDt, drs, thcs, "ZZ");
            }

            var result = thcDt.Clone();
            foreach (var dr in thcs)
            {
                result.ImportRow(dr);
            }
            return result;
        }

        private DataRow[] GetTHCRow(DataTable thcDt, DataRow[] drs, DataRow[] thcs, string are="")
        {
            if (!string.IsNullOrEmpty(are))
            {
                return thcDt.Select(string.Format("AREA={0}", SQLUtils.QuotedStr(are)));
            }

            if (drs.Length <= 0)
                return thcs;
            foreach (DataRow dr in drs)
            {
                are = Prolink.Math.GetValueAsString(dr["AREA"]);
                var data= thcDt.Select(string.Format("AREA={0}", SQLUtils.QuotedStr(are)));
                if (data.Length > 0)
                {
                    WriteLog("THC AREA:" + are);
                    return data;
                }
            }
            return thcs;
        }
        #endregion

        private static string GetArea(string dest, DataTable areDt)
        {
            if (string.IsNullOrEmpty(dest) || areDt == null)
                return string.Empty;
            DataRow[] drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest)));
            if (drs.Length <= 0 && dest.Length >= 2)
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr(dest.Substring(0, 2) + "ZZZ")));
            if (drs.Length <= 0)
                drs = areDt.Select(string.Format("PORT={0}", SQLUtils.QuotedStr("ZZZZZ")));
            if (drs.Length <= 0)
                drs = areDt.Select(string.Format("AREA={0}", SQLUtils.QuotedStr("ZZ")));
            if (drs.Length > 0)
            {
                foreach (DataRow dr in drs)
                {
                    string area = Prolink.Math.GetValueAsString(dr["AREA"]);
                    if (!"ZZ".Equals(area))
                        return area;
                }
                return Prolink.Math.GetValueAsString(drs[0]["AREA"]);
            }
            return string.Empty;
        }

        private DataTable GetOthCharge(string carrier, string cmp, string polCd, string qtdate_condition)
        {
            //List<string> polList = Helper.GetValueList(pt_qtDt, "POL_CD");
            //string sql = string.Format("SELECT * FROM(SELECT * FROM SMQTD WHERE TRAN_MODE='O' AND QUOT_TYPE='F' AND REPAY='M' AND POL_CD={2} AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.TRAN_MODE='O' AND RLOCATION={0} AND LSP_CD={1} {3}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(carrier), SQLUtils.QuotedStr(polCd), qtdate_condition);
            string sql = string.Format("SELECT * FROM(SELECT * FROM SMQTD WHERE TRAN_MODE='O' AND EXISTS (SELECT RFQ_NO FROM SMQTM WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='O' AND RLOCATION={0} AND LSP_CD={1} {2}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM,CUST_CD FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(carrier), qtdate_condition);
            WriteLog("船公司费用SQL:" + sql);
            //string sql = "SELECT * FROM SMQTD WHERE LSP_CD=" + SQLUtils.QuotedStr(cmp) + " AND TRAN_MODE='O'" + ((polList.Count > 0) ? (" AND POL_CD IN " + Helper.JoinString(polList.ToArray())) : "");
            DataTable othDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            return othDt;
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
            ei.Put("QCUR", qt["CUR"]);//從報價帶出的預提
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
            return ei;
        }

        private static string GetQTValue(DataRow qt, string qname, string name)
        {
            string val = qt.Table.Columns.Contains(qname) ? Prolink.Math.GetValueAsString(qt[qname]) : string.Empty;
            if (string.IsNullOrEmpty(val))
                val = qt.Table.Columns.Contains(name) ? Prolink.Math.GetValueAsString(qt[name]) : string.Empty;
            return val;
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
        /// 抓取需要的付款对象和规则
        /// </summary>
        /// <param name="tran_mode"></param>
        /// <param name="term"></param>
        /// <param name="freightTerm"></param>
        /// <param name="loadingTo"></param>
        /// <returns></returns>
        public static List<string> GetDebitTo(string tran_mode, string term, string freightTerm, string loadingTo, string cent_decl, string bandType, string TrackWay, string bl_type, string iscombine_bl,string bondType="O")
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
                        switch (term)
                        {
                            case "EXW":
                            case "FCA":
                            case "FAS":
                            case "FOB":
                                if ("C".Equals(freightTerm))
                                    types.Add("CarrierI");
                                break;
                        }
                        break;
                }
                if ("S".Equals(iscombine_bl))
                {
                    if (types.Contains("CarrierI")) types.Remove("CarrierI");
                }
                return types;
            }
            if ("EXW".Equals(term))
                return types;
            //判断是否有运费
            switch (freightTerm)
            {
                case "P":
                    types.Add("Carrier");
                    types.Add(LOCAL_CHARGE);
                    types.Add(FREIGHT);
                    break;
                default:
                    //2.	Railway/LCL/AIR  Freight term =P 的會抓  DLV_TERM=CIF 的報價.  Freight term 不為P , 且DLV term 不為EXW 的都會抓  報價單的DLV_term =FOB 的報價 . 
                    if (("R".Equals(tran_mode) || "L".Equals(tran_mode) || "A".Equals(tran_mode)) && !"EXW".Equals(term))
                    {
                        types.Add("Carrier");
                        types.Add(LOCAL_CHARGE);
                    }
                    else if ("F".Equals(tran_mode))
                    {
                        types.Add("Carrier");
                        types.Add(LOCAL_CHARGE);
                    }
                    else if (("T".Equals(tran_mode) && ("A".Equals(TrackWay) || "S".Equals(TrackWay))) || "D".Equals(TrackWay))
                    {
                        //內貿運輸(包含國內快遞/國內空運)
                        types.Add("Carrier");
                        types.Add(FREIGHT);
                    }
                    break;
            }

            switch (tran_mode)
            {
                case "D":
                    break;
                case "T":
                    if ("Y".Equals(bandType))//2.	繞物流園區的才要算報關費.BandType
                    {
                        types.Add("BR");
                        types.Add("BC");
                        types.Add("BM");
                    }
                    //types.Add("CR");
                    break;
                case "E":// 非統報的國際快遞單,要用 BR 
                    //if (!"Y".Equals(cent_decl.ToUpper()))
                    //    types.Add("BR");
                    break;
                default:
                    types.Add("BR");
                    types.Add("CR");
                    break;
            }

            switch (term)
            {
                case "CIP":
                case "DAP":
                case "DDP":
                    types.Add(DESTINATION_CHARGE);
                    break;
                case "EXW":
                    if ("F".Equals(tran_mode))//FCL一定会有这两个费用
                        break;
                    if (types.Contains("BR")) types.Remove("BR");
                    if (types.Contains("CR")) types.Remove("CR");
                    break;
            }

            // 計價時BL_type =S 的只能算拖車/報關,
            //if ("S".Equals(bl_type))
            //{
            //    if (types.Contains("Carrier")) types.Remove("Carrier");
            //}

            //  合併提單的shipment 只能計價 運費/Local
            if ("Y".Equals(iscombine_bl) || "C".Equals(iscombine_bl))
            {
                if (types.Contains("BR")) types.Remove("BR");
                if (types.Contains("CR")) types.Remove("CR");
            }
            else if ("S".Equals(iscombine_bl))
            {
                if (types.Contains("Carrier")) types.Remove("Carrier");
            }

            return types;

            //List<string> costlIST
            //if (!"DOOR".Equals(loadingTo) && !"CY".Equals(loadingTo))
            //    loadingTo = string.Empty;
            //if ("DR".Equals(loadingTo)) loadingTo = "DOOR";
            //Dictionary<string, List<string>> cost_mapping = CreateTermCostMapping();
            //string key = string.Format("{0}_{1}_{2}_{3}", tran_mode, term, freightTerm, loadingTo);
            //if (cost_mapping.ContainsKey(key))
            //    return cost_mapping[key];

            //if (!string.IsNullOrEmpty(loadingTo))
            //{
            //    key = string.Format("{0}_{1}_{2}_{3}", tran_mode, term, freightTerm, string.Empty);
            //    if (cost_mapping.ContainsKey(key))
            //        return cost_mapping[key];
            //}

            //return new List<string>();
        }

        public static Dictionary<string, List<string>> CreateTermCostMapping()
        {
            if (_cost_mapping != null)
                return _cost_mapping;

            string path = Prolink.Web.WebContext.GetInstance().GetProperty("TermBillCostMapping");
            DataTable dt = ExcelHelper.ImportExcelToDataTable(path);
            _cost_mapping = new Dictionary<string, List<string>>();

            //List<string> types = new List<string> { "貨代/Carrier", "報關", "拖車", "Local charge", "运费", "Destination charge", "Insurance", "目的港关税Duty" };
            List<string> types = new List<string> { "Carrier", "BR", "CR", "LocalCharge", "Freight", "DestinationCharge", "Insurance", "Duty" };
            foreach (DataRow dr in dt.Rows)
            {
                string key = string.Format("{0}_{1}_{2}_{3}", Prolink.Math.GetValueAsString(dr["TRAN_MODE"]), Prolink.Math.GetValueAsString(dr["Term"]), Prolink.Math.GetValueAsString(dr["FreightTerm"]), Prolink.Math.GetValueAsString(dr["LoadingTo"]));
                _cost_mapping[key] = new List<string>();
                foreach (var type in types)
                {
                    if ("Y".Equals(Prolink.Math.GetValueAsString(dr[type]).Trim().ToUpper()))
                        _cost_mapping[key].Add(type);
                }
            }
            return _cost_mapping;
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

        public void MergeBillEditInstruct1(MixedList ml, Dictionary<string, object> schems, EditInstruct main_ei)
        {
            if (schems == null || schems.Count <= 0)
                return;
            int index = Prolink.Math.GetValueAsInt(schems["方案"]);
            List<EditInstruct> list = schems["方案" + index] as List<EditInstruct>;
            decimal amt = Prolink.Math.GetValueAsDecimal(schems["方案" + index + "_amt"]);
            string remark = Prolink.Math.GetValueAsString(schems["方案" + index + "_remark"]);
            string lsp_nm = string.Empty;
            if (main_ei != null && !string.IsNullOrEmpty(main_ei.Get("LSP_NM")))
                lsp_nm = main_ei.Get("LSP_NM");

            #region 分组币别
            List<string> curList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                EditInstruct ei = list[i];
                string cur = ei.Get("QCUR");
                if (curList.Contains(cur))
                    continue;
                curList.Add(cur);
            }
            #endregion

            foreach (string cur in curList)
            {
                string u_id = System.Guid.NewGuid().ToString();
                EditInstruct m_ei = Helper.CopyEditInstruct(main_ei);
                m_ei.Put("U_ID", u_id);
                m_ei.Put("CUR", cur);
                ml.Add(m_ei);

                DataRow[] drs = _smbimDt.Select(string.Format("LSP_NO={0} AND CUR={1}", SQLUtils.QuotedStr(m_ei.Get("LSP_NO")), SQLUtils.QuotedStr(cur)));
                if (drs.Length > 0)
                {
                    u_id = Prolink.Math.GetValueAsString(drs[0]["U_ID"]);
                    m_ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    m_ei.PutKey("U_ID", u_id);
                    for (int i = 1; i < drs.Length; i++)
                    {
                        EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.DELETE_OPERATION);
                        ei.PutKey("U_ID", drs[i]["U_ID"]);
                        //ei.Put("VOID_FLAG", "Y");
                        ml.Add(ei);
                    }
                    drs = _smbidDt.Select(string.Format("U_FID={0}", SQLUtils.QuotedStr(u_id)));
                    //drs = _smbidDt.Select(string.Format("U_FID={0} AND PARTY_TYPE={1}", SQLUtils.QuotedStr(u_id), SQLUtils.QuotedStr(pt_type)));
                }

                for (int i = 0; i < list.Count; i++)
                {
                    EditInstruct ei = list[i];
                    if (!cur.Equals(ei.Get("QCUR")))
                        continue;
                    //ei.Put("PARTY_TYPE", pt_type);
                    if (!string.IsNullOrEmpty(lsp_nm))
                        ei.Put("LSP_NM", lsp_nm);

                    if (!string.IsNullOrEmpty(u_id))
                        ei.Put("U_FID", u_id);
                    for (int j = 0; j < drs.Length; j++)
                    {
                        if (Prolink.Math.GetValueAsInt(drs[j]["EX_UPDATE"]) != 0)
                            continue;

                        if (Prolink.Math.GetValueAsString(drs[j]["CHG_CD"]).Equals(
                            Prolink.Math.GetValueAsString(ei.Get("CHG_CD")))
                            && Prolink.Math.GetValueAsString(drs[j]["CHG_TYPE"]).Equals(
                            Prolink.Math.GetValueAsString(ei.Get("CHG_TYPE")))
                             && Prolink.Math.GetValueAsString(drs[j]["QCHG_UNIT"]).Equals(
                            Prolink.Math.GetValueAsString(ei.Get("QCHG_UNIT"))))
                        {
                            ei.Remove("CUR");
                            drs[j]["EX_UPDATE"] = 1;
                            ei.OperationType = EditInstruct.UPDATE_OPERATION;
                            ei.PutKey("U_ID", drs[i]["U_ID"]);
                            break;
                        }
                    }

                    ml.Add(ei);
                }

                //EX_UPDATE
                for (int i = 0; i < drs.Length; i++)
                {
                    if (Prolink.Math.GetValueAsInt(drs[i]["EX_UPDATE"]) != 0)
                        continue;
                    //if (!cur.Equals(Prolink.Math.GetValueAsString(drs[i]["QCUR"])))
                    //    continue;
                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.DELETE_OPERATION);
                    ei.PutKey("U_ID", drs[i]["U_ID"]);
                    if (Prolink.Math.GetValueAsDecimal(drs[i]["UNIT_PRICE"]) != 0 || Prolink.Math.GetValueAsDecimal(drs[i]["BAMT"]) != 0 || Prolink.Math.GetValueAsDecimal(drs[i]["QTY"]) > 0)
                    {
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        ei.Put("QCUR", "");//從報價帶出的預提
                        ei.Put("QUNIT_PRICE", 0);//從報價帶出的預提
                        ei.Put("QCHG_UNIT", "");//從報價帶出的預提
                        ei.Put("QQTY", 0);//报价数量  "從報價帶出的預提chg_unit如果為K, 就放SMSM.CW, 如果為CBM 就放SMSM.cbm 如果為SET 就放 1如果為CTN 就放SMSM.qty"
                        ei.Put("QAMT", 0);//"從報價帶出的預提 Unit_price X QTY"
                        ei.Put("QUOT_ID", "");//报价主键
                        ei.Put("QUOT_NO", "");
                        ei.Put("RFQ_NO", "");
                    }
                    ml.Add(ei);
                }
            }
        }
        /// <summary>
        /// 设置费用信息
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="chgCd"></param>
        /// <param name="tranMode"></param>
        private void SetChargeInfo(DataRow dr, string chgCd, string tranMode, bool setType = false)
        {

            if (_chgDt == null)
                return;
            string chgCd1 = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
            if ("FRT".Equals(chgCd1))
            {
                dr["CHG_CD"] = chgCd;
            }
            else
            {
                chgCd = chgCd1;
            }
            DataRow[] drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr(tranMode)));
            if (drs.Length <= 0)
                drs = _chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr("O")));
            if (drs.Length <= 0)
                drs = _chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(chgCd)));
            if (drs.Length > 0)
            {
                dr["QCHG_DESCP"] = drs[0]["CHG_DESCP"];
                dr["QTAX"] = drs[0]["VAT_RATE"];
                if (setType)
                    dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                else if (string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["CHG_TYPE"])))
                    dr["QCHG_TYPE"] = drs[0]["CHG_TYPE"];
                else
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

        public static void WriteLogTagStart(string tag,string shipmentId="")
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

        private void WriteLogTagEnd(string tag)
        {
            try
            {
                if (string.IsNullOrEmpty(_shipment_id))
                    return;
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("BillLog");
                path = System.IO.Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, _shipment_id + ".txt");

                System.IO.File.AppendAllText(path, string.Format("-------------------{0} {1}------------------", tag, DateTime.Now.ToString("yyyy-MM-dd HH:mm:fff")), Encoding.UTF8);
                System.IO.File.AppendAllText(path, System.Environment.NewLine + System.Environment.NewLine + System.Environment.NewLine, Encoding.UTF8);
                //System.IO.File.AppendAllText(path, System.Environment.NewLine + "", Encoding.UTF8);
            }
            catch { }
        }

        public static void WriteLog(string messenger,string shipmentId="")
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
        #endregion


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

        private static List<string> GetChargeType(List<string> d_to, List<string> types = null, List<string> charge_type = null)
        {
            if (charge_type == null)
                charge_type = new List<string>() { "无" };
            if (d_to == null)
                return charge_type;
            if (types == null)
                types = new List<string>() { LOCAL_CHARGE, DESTINATION_CHARGE, FREIGHT };

            foreach (string type in types)
            {
                if (!d_to.Contains(type))
                    continue;
                switch (type)
                {
                    case LOCAL_CHARGE:
                        if (!charge_type.Contains("O"))
                            charge_type.Add("O");
                        break;
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
        /// 获取shipment 关联的所有DN的CBM和
        /// </summary>
        /// <param name="dnDt"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static decimal GetDnAllCbm(DataTable dnDt, DataRow dr)
        {
            string combine_info = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);
            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            DataRow[] dnDrs = dnDt.Select(string.Format("DN_NO IN {0}", Helper.JoinString(dns)));
            decimal cbm = 0M;
            foreach (DataRow dn in dnDrs)
            {
                cbm += Prolink.Math.GetValueAsDecimal(dn["CBM"]);
            }
            return cbm;
        }
        Dictionary<string, DataTable> _rateMap = new Dictionary<string, DataTable>();
        private DataTable GetRate(DataTable smDt, string sm_id)
        {
            DateTime billDate = DateTime.Now;
            DataRow[] drs = smDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(sm_id)));
            if (drs.Length <= 0) return null;

            billDate = GetBillDate(billDate, drs[0]);
            string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            string sql = string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE {0} ORDER BY EDATE", rateFilter);
            if (_rateMap.ContainsKey(sql))
                return _rateMap[sql];
            DataTable rateDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            _rateMap[sql] = rateDt;
            return rateDt;
        }

        /// <summary>
        /// 获取账单日期
        /// </summary>
        /// <param name="billDate"></param>
        /// <param name="drs"></param>
        /// <returns></returns>
        public static DateTime GetBillDate(DateTime billDate, DataRow dr,bool haveATP=true)
        {
            string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            if (haveATP&&("F".Equals(tranType) || "L".Equals(tranType))//如果Region是NA則用ATP回寫到費用管理
                && dr["ATP"] != null && dr["ATP"] != DBNull.Value &&
                "NA".Equals(Prolink.Math.GetValueAsString(dr["REGION"])))
                billDate = (DateTime)dr["ATP"];
            else if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)//其他用ATD
                billDate = (DateTime)dr["ATD"];
            else if (dr["ETD"] != null && dr["ETD"] != DBNull.Value)
                billDate = (DateTime)dr["ETD"];
            else if (dr["DN_ETD"] != null && dr["DN_ETD"] != DBNull.Value)
                billDate = (DateTime)dr["DN_ETD"];
            return billDate;
        }

        public static DateTime GetBillDateForIb(DateTime billDate, DataRow dr, bool haveATP = true)
        {
            //string tranType = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            if (dr["ATA"] != null && dr["ATA"] != DBNull.Value)//其他用ATD
                billDate = (DateTime)dr["ATA"];
            else if (dr["ETA"] != null && dr["ETA"] != DBNull.Value)
                billDate = (DateTime)dr["ETA"];
            //if (haveATP && ("F".Equals(tranType) || "L".Equals(tranType))//如果Region是NA則用ATP回寫到費用管理
            //    && dr["ATP"] != null && dr["ATP"] != DBNull.Value)
            //    billDate = (DateTime)dr["ATP"];
            //else if (dr["ATD"] != null && dr["ATD"] != DBNull.Value)//其他用ATD
            //    billDate = (DateTime)dr["ATD"];
            //else if (dr["ETD"] != null && dr["ETD"] != DBNull.Value)
            //    billDate = (DateTime)dr["ETD"];
            //else if (dr["DN_ETD"] != null && dr["DN_ETD"] != DBNull.Value)
            //    billDate = (DateTime)dr["DN_ETD"];
            return billDate;
        }

        /// <summary>
        /// 获取所有的DN
        /// </summary>
        /// <param name="shipment_uid"></param>
        /// <returns></returns>
        public DataTable GetAllDN(string shipment_uid)
        {
            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT " + _table + ".* FROM " + _table + " WHERE U_ID={0}", SQLUtils.QuotedStr(shipment_uid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (smDt.Rows.Count <= 0) return null;
            DataRow smsm = smDt.Rows[0];
            string cmp = Prolink.Math.GetValueAsString(smsm["CMP"]);
            string group_id = Prolink.Math.GetValueAsString(smsm["GROUP_ID"]);
            string tranMode = Prolink.Math.GetValueAsString(smsm["TRAN_TYPE"]);
            string shipment_id = Prolink.Math.GetValueAsString(smsm["SHIPMENT_ID"]);
            string combine_info = Prolink.Math.GetValueAsString(smsm["COMBINE_INFO"]);
            string shipment_info = Prolink.Math.GetValueAsString(smsm["SHIPMENT_INFO"]);

            string sql = string.Empty;
            List<string> smList = Helper.SplitToList(shipment_info);
            if (!smList.Contains(shipment_id))
                smList.Add(shipment_id);

            sql = (smList.Count > 1) ? "SELECT * FROM SMDN WHERE SHIPMENT_ID IN " + Helper.JoinString(smList.ToArray()) : "SELECT * FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);

            string[] dns = combine_info.Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (dns.Length > 0)
                sql += " UNION SELECT * FROM SMDN WHERE STATUS<>'V' AND DN_NO IN " + Helper.JoinString(dns);

            DataTable smdnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return smdnDt;
        }

        decimal GetFOB(DataRow dnRd, MixedList ml = null)
        {
            string groupId = Prolink.Math.GetValueAsString(dnRd["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(dnRd["CMP"]);
            decimal FreightFee = Prolink.Math.GetValueAsDecimal(dnRd["FREIGHT_AMT"]);
            decimal TtlValue = Prolink.Math.GetValueAsDecimal(dnRd["AMOUNT1"]);
            string TradeTerm = Prolink.Math.GetValueAsString(dnRd["TRADE_TERM"]).ToUpper();

            DateTime Etd = DateTime.Now;
            if (dnRd["ETD"] != null && dnRd["ETD"] != DBNull.Value)
                Etd = (DateTime)dnRd["ETD"];
            string year = Etd.Year.ToString();

            decimal tir = GetTIR(groupId, cmp, year);
            var IssueFee = TtlValue * 1.1m * tir;
            if (!"FOB".Equals(TradeTerm) && !"FCA".Equals(TradeTerm) && !"CFR".Equals(TradeTerm) && !"EXW".Equals(TradeTerm))
            {
                if (IssueFee > 0 && IssueFee <= 1)
                {
                    IssueFee = 1;
                }
                else if (IssueFee > 1)
                {
                    IssueFee = Math.Round(IssueFee);
                }
                //IssueFee = Helper.Get45AmtValue(IssueFee);
            }
            else
            {
                IssueFee = 0;
            }
            decimal FobValue = TtlValue - FreightFee - IssueFee;

            if (ml != null)
            {
                EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", Prolink.Math.GetValueAsString(dnRd["U_ID"]));
                ei.Put("FOB_VALUE", FobValue);
                ei.Put("ISSUE_FEE", IssueFee);
                ml.Add(ei);
            }
            WriteLog(string.Format("FobValue({0}) = TtlValue({1}) - FreightFee({2}) - IssueFee({3})=>{4},{5}", FobValue, TtlValue, FreightFee, IssueFee, TradeTerm, tir));
            return FobValue;
        }

        Dictionary<string, decimal> _mapTir = new Dictionary<string, decimal>();
        decimal GetTIR(string groupId, string cmp, string year)
        {
            string sql = string.Format("SELECT CD_DESCP FROM BSCODE WHERE GROUP_ID={0} AND (CMP={1} OR CMP='*') AND CD={2} AND CD_TYPE='TIR'", SQLUtils.QuotedStr(groupId), SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(year));
            if (_mapTir.ContainsKey(sql))
                return _mapTir[sql];
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            decimal tir = 0M;
            if (dt.Rows.Count > 0)
            {
                string tir_cd = Prolink.Math.GetValueAsString(dt.Rows[0]["CD_DESCP"]).Trim();
                if (!decimal.TryParse(tir_cd, out tir)) tir = 0m;
            }
            _mapTir[sql] = tir;
            return tir;
        }
        /// <summary>
        ///  根据partyNo tranMode 抓关联到的全部费用代码  L:local F:运费 C:拖车  B:报关
        /// </summary>
        /// <param name="partyNo"></param>
        /// <param name="tranMode"></param>
        /// <param name="type">L:local F:运费 C:拖车  B:报关</param>
        /// <returns></returns>
        public static DataTable GetChargeCodesForIb(string partyNo, string tranMode, string type = "F", string condition = "")
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHG_CD", typeof(string));
            dt.Columns.Add("CHG_DESCP", typeof(string));
            dt.Columns.Add("TRAN_MODE", typeof(string));
            dt.Columns.Add("REPAY", typeof(string));
            dt.Columns["CHG_DESCP"].MaxLength = 30;
            dt.Columns.Add("CUR", typeof(string));

            #region 过滤POL条件
            string[] ccs = condition.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            string pol_filter = string.Empty;
            string filter = string.Empty;
            string cmpFilter = string.Empty;
            string chgFilter = string.Empty;
            foreach (string c in ccs)
            {
                string c1 = c.Trim().ToUpper();
                if (c1.StartsWith("POL_CD"))
                {
                    pol_filter = " AND " + c.Trim();
                    continue;
                }
                if (c1.StartsWith("SCAC_CD") || c1.StartsWith("HOUSE_NO") || c1.StartsWith("SM_STATUS") || c1.StartsWith("TPV_DEBIT_NO") || c1.StartsWith("OP"))
                    continue;
                if (filter.Length > 0)
                    filter += " AND ";
                filter += c;

                if (c1.StartsWith("CMP"))
                {
                    cmpFilter = " AND " + c.Trim();
                }

                if(c1.StartsWith("CHG_CD"))
                    chgFilter= c.Trim();
            }
            #endregion

            string head_office = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(partyNo)), Prolink.Web.WebContext.GetInstance().GetConnection());

            string filter1 = string.Empty;
            if (string.IsNullOrEmpty(filter))
                filter = "1=1";
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.Trim().ToUpper().StartsWith("AND"))
                    filter = "1=1 " + filter;
                filter1 = filter; 
                filter = setPodList("SMSMI", filter, partyNo, pol_filter);
                filter1 = setPodList("SMIDN;SMICNTR", filter1, partyNo, pol_filter, "DLV_AREA");
            }


            DataRow row = null;
            string sql = string.Empty;
            switch (type)
            {
                case "L":
                    sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B') AND (SMQTM.LSP_CD={0} OR SMQTM.CREDIT_TO={0})AND SMQTM.TRAN_TYPE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter);

                    if (!"F".Equals(tranMode))
                    {
                        sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='F' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_MODE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter);
                        if (!string.IsNullOrEmpty(head_office) && !head_office.Equals(partyNo))
                            sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='F' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_MODE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(head_office), SQLUtils.QuotedStr(tranMode), filter);
                    }
                    sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR, (SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID = SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS(SELECT 1 FROM SMBID WHERE SMBID.QUOT_NO = SMQTD.QUOT_NO AND LSP_NO= {0} AND EXISTS(SELECT 1 FROM SMQTM WITH(NOLOCK) WHERE QUOT_NO=SMBID.QUOT_NO AND QUOT_TYPE = N'F' AND TRAN_MODE = N'L' )))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH(NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B  ", SQLUtils.QuotedStr(partyNo));
                    sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE ='C' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter1);

                    
                     
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                default://运费
                    switch (tranMode)
                    {
                        case "F":
                            row = dt.NewRow();
                            row["CHG_CD"] = "OF";
                            row["CHG_DESCP"] = "Ocean Freight";
                            row["CUR"] = "USD";
                            dt.Rows.Add(row);
                            break;
                        case "R":
                            row = dt.NewRow();
                            row["CHG_CD"] = "RF";
                            row["CHG_DESCP"] = "Railway Freight";
                            row["CUR"] = "CNY";
                            dt.Rows.Add(row);
                            break;
                        //報關不傳類別
                        case "":
                            sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.LSP_CD={0})", SQLUtils.QuotedStr(partyNo));
                            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            break;
                        default:
                            //sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND (SMQTM.TRAN_MODE={0} OR TRAN_TYPE={0}) AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(partyNo));
                            sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.TRAN_MODE={0} AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(partyNo));

                            if (!string.IsNullOrEmpty(head_office) && !head_office.Equals(partyNo))
                                sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.TRAN_MODE={0} AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(head_office));

                            sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B','C') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));

                            sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='F' AND SMQTM.TRAN_MODE = {0} AND SMQTM.QUOT_NO IN(SELECT QUOT_NO FROM SMBID WHERE SMBID.LSP_NO={1}) ))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(partyNo));
                            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            break;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(chgFilter))
            {
                DataTable myDt = dt.Clone();
                DataRow [] drs = dt.Select(chgFilter);
                foreach (DataRow dr in drs)
                {
                    myDt.ImportRow(dr);
                }
                dt = myDt;
            }

            #region 设置空币别
            try
            {
                DataTable smqtiDt = null;
                DataTable dt1 = dt.Clone();
                foreach (DataRow dr in dt.Rows)
                {
                    string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
                    string chg_cd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                    if (!string.IsNullOrEmpty(cur))
                    {
                        dt1.ImportRow(dr);
                        continue;
                    }
                    if (smqtiDt == null)
                        smqtiDt = OperationUtils.GetDataTable("SELECT DISTINCT I_TYPE,CUR FROM SMQTI WITH(NOLOCK) WHERE CUR IS NOT NULL " + cmpFilter, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    DataRow[] smqtis = smqtiDt.Select(string.Format("I_TYPE={0}", SQLUtils.QuotedStr(chg_cd)));
                    foreach (DataRow smqti in smqtis)
                    {
                        dr["CUR"] = smqti["CUR"];
                        dt1.ImportRow(dr);
                    }
                }
                dt = dt1;
            }
            catch { }
            #endregion

            DataTable result = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                switch (type)//过滤运费
                {
                    case "L":
                        string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                        switch (chgCd)
                        {
                            case "FRT":
                                if (!ChangeChgCode(dr))
                                    continue;
                                break;
                        }
                        break;
                }
                AddFreightCode(result, dr, type);
            }
            return result;
        }

        public static string setPodList(string table,string filter,string partyNo,string pol_filter,string field="POD_CD") {
            List<string> podList = new List<string>();
            string[] tables = table.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string podSql = "";
            for (int i = 0; i < tables.Length; i++)
            {
                if (i > 0)
                    podSql += " UNION ";
                podSql += string.Format("SELECT DISTINCT {4} FROM {3} WHERE SHIPMENT_ID IN ((SELECT SHIPMENT_ID FROM SMBID WHERE {0} AND SMBID.LSP_NO={1})){2}", filter, SQLUtils.QuotedStr(partyNo), pol_filter, tables[i], field);
            }
            DataTable podDt = OperationUtils.GetDataTable(podSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow dr in podDt.Rows)
            {
                string podCd = Prolink.Math.GetValueAsString(dr[field]);
                if (!podList.Contains(podCd)&&!string.IsNullOrEmpty(podCd))
                    podList.Add(podCd);
            }
            string condition = " AND 1=1";
            if (podList.Count() > 0)
                condition = string.Format(" AND (POD_CD IS NULL OR POD_CD IN {0})", SQLUtils.Quoted(podList.ToArray()));
            return condition;
        }
        /// <summary>
        ///  根据partyNo tranMode 抓关联到的全部费用代码  L:local F:运费 C:拖车  B:报关
        /// </summary>
        /// <param name="partyNo"></param>
        /// <param name="tranMode"></param>
        /// <param name="type">L:local F:运费 C:拖车  B:报关</param>
        /// <returns></returns>
        public static DataTable GetChargeCodes(string partyNo, string tranMode, string type = "F", string condition = "")
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHG_CD", typeof(string));
            dt.Columns.Add("CHG_DESCP", typeof(string));
            dt.Columns.Add("TRAN_MODE", typeof(string));
            dt.Columns.Add("REPAY", typeof(string));
            dt.Columns["CHG_DESCP"].MaxLength = 30;
            dt.Columns.Add("CUR", typeof(string));
            
            #region 过滤POL条件
            string[] ccs = condition.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            string pol_filter = string.Empty;
            string filter = string.Empty;
            foreach (string c in ccs)
            {
                string c1 = c.Trim().ToUpper();
                if (c1.StartsWith("POL_CD"))
                {
                    pol_filter = " AND " + c.Trim();
                    continue;
                }
                if (filter.Length > 0)
                    filter += " AND ";
                filter += c;
            }
            #endregion

            string head_office = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 HEAD_OFFICE FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(partyNo)), Prolink.Web.WebContext.GetInstance().GetConnection());

            string filter1 = string.Empty;
            if (string.IsNullOrEmpty(filter))
                filter = "1=1";
            if (!string.IsNullOrEmpty(filter))
            {
                if (filter.Trim().ToUpper().StartsWith("AND"))
                    filter = "1=1 " + filter;
                filter1 = filter;
                filter = string.Format(" AND (POL_CD IS NULL OR POL_CD IN (SELECT POL_CD FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE {0} AND SMBID.LSP_NO={1}){2}))", filter, SQLUtils.QuotedStr(partyNo), pol_filter);

                filter1 = string.Format(" AND (POL_CD IS NULL OR POL_CD IN (SELECT CMP FROM SMSM WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE {0} AND SMBID.LSP_NO={1}){2}))", filter1, SQLUtils.QuotedStr(partyNo), pol_filter);
            }


            DataRow row = null;
            string sql = string.Empty;
            switch (type)
            {
                case "B":
                    sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('B') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                case "C":
                    sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR  FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('C') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                case "ALL":
                    sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B','C') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;

                case "L":
                    //case "C":
                    //case "B":
                    //if (!string.IsNullOrEmpty(tranMode))
                    //    sql = string.Format("SELECT CHG_CD,CHG_DESCP FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP FROM SMQTD WHERE  EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B','C') AND SMQTM.FREIGHT_TERM<>'P' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                    //else

                    //sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B','C') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter);
                    sql = string.Format("SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter);

                    if (!"F".Equals(tranMode))
                    {
                        sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='F' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_MODE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter);
                        if(!string.IsNullOrEmpty(head_office)&&!head_office.Equals(partyNo))
                            sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='F' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_MODE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(head_office), SQLUtils.QuotedStr(tranMode), filter);
                    }

                    sql += string.Format(" UNION SELECT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,QT_EFFECT_FROM,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE ='C' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}){2})A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode), filter1);


                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
                //    break;
                //case "C":
                //    switch (tranMode)
                //    {
                //        default:
                //            sql = string.Format("SELECT CHG_CD,CHG_DESCP FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='C' AND SMQTM.LSP_CD={0}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo));
                //            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //            break;
                //    }
                //    break;
                //case "B":
                //    switch (tranMode)
                //    {
                //        default:
                //            if (!string.IsNullOrEmpty(tranMode))
                //                sql = string.Format("SELECT CHG_CD,CHG_DESCP FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='B' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                //            else
                //                sql = string.Format("SELECT CHG_CD,CHG_DESCP FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE='B' AND SMQTM.LSP_CD={0}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO", SQLUtils.QuotedStr(partyNo));
                //            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                //            break;
                //    }
                //    break;
                default://运费
                    switch (tranMode)
                    {
                        case "F":
                            row = dt.NewRow();
                            row["CHG_CD"] = "OF";
                            row["CHG_DESCP"] = "Ocean Freight";
                            row["CUR"] = "USD";
                            dt.Rows.Add(row);
                            break;
                        case "R":
                            row = dt.NewRow();
                            row["CHG_CD"] = "RF";
                            row["CHG_DESCP"] = "Railway Freight";
                            row["CUR"] = "CNY";
                            dt.Rows.Add(row);
                            break;
                        //報關不傳類別
                        case "":
                            sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.LSP_CD={0})", SQLUtils.QuotedStr(partyNo));
                            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            break;
                        default:
                            //sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND (SMQTM.TRAN_MODE={0} OR TRAN_TYPE={0}) AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(partyNo));
                            sql = string.Format("SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.TRAN_MODE={0} AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(partyNo));

                            if (!string.IsNullOrEmpty(head_office) && !head_office.Equals(partyNo))
                                sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WITH (NOLOCK) WHERE  QUOT_TYPE='F' AND EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.TRAN_MODE={0} AND SMQTM.LSP_CD={1})", SQLUtils.QuotedStr(tranMode), SQLUtils.QuotedStr(head_office));

                            sql += string.Format(" UNION SELECT DISTINCT CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,CHG_DESCP,TRAN_MODE,REPAY,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID=SMQTD.U_FID)) CUR FROM SMQTD WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE IN ('X','B','C') AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}))A OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B", SQLUtils.QuotedStr(partyNo), SQLUtils.QuotedStr(tranMode));
                            dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            break;
                    }
                    break;
            }

            DataTable result = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                switch (type)//过滤运费
                {
                    case "L":
                    case "C":
                    case "B":
                        string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
                        switch (chgCd)
                        {
                            case "FRT":
                            //case "OF":
                                //case "RF":
                                //case "CF":
                                //case "DF":
                                //case "AF":
                                if (!ChangeChgCode(dr))
                                    continue;
                                break;
                        }
                        break;
                }
                AddFreightCode(result, dr, type);
            }
            if (!"F".Equals(tranMode))
                AddFreightCode(result, tranMode);
            return result;
            //return dt.Rows.Cast<DataRow>().Select(row => Prolink.Math.GetValueAsString(row["CHG_CD"])).ToArray();
        }

        private static bool ChangeChgCode(DataRow dr)
        {
            if (!dr.Table.Columns.Contains("TRAN_MODE"))
                return false;
            switch (Prolink.Math.GetValueAsString(dr["TRAN_MODE"]))
            {
                case "E":
                case "D":
                    dr["CHG_CD"] = "CF";
                    dr["CHG_DESCP"] = "COURIER CHARGE";
                    break;
                case "T":
                    dr["CHG_CD"] = "DF";
                    dr["CHG_DESCP"] = "Domestic Truck fee";
                    break;
                case "A":
                    dr["CHG_CD"] = "AF";
                    dr["CHG_DESCP"] = "Air Freight";
                    break;
                case "L":
                case "F":
                    dr["CHG_CD"] = "OF";
                    dr["CHG_DESCP"] = "Ocean Freight";
                    break;
                case "R":
                    dr["CHG_CD"] = "RF";
                    dr["CHG_DESCP"] = "Railway Freight";
                    break;
            }
            return true;
        }

        public static void AddFreightCode(DataTable result, DataRow dr, string type)
        {
            string tran_mode = string.Empty;
            if (dr.Table.Columns.Contains("TRAN_MODE"))
                tran_mode = Prolink.Math.GetValueAsString(dr["TRAN_MODE"]);
            string chgCd = Prolink.Math.GetValueAsString(dr["CHG_CD"]);
            string cur = Prolink.Math.GetValueAsString(dr["CUR"]);
            if (!"FRT".Equals(chgCd))
            {
                if (result.Select(string.Format("CHG_CD={0} AND CUR={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr(cur))).Length <= 0)
                { 
                    result.ImportRow(dr);
                    if ("CF".Equals(chgCd)) 
                        AddFscCode(result, tran_mode, cur, type); 
                }
                return;
            }

            AddFreightCode(result, tran_mode, cur);
        }
        private static void AddFscCode(DataTable result, string tran_mode, string cur, string type)
        {
            if (!"E".Equals(tran_mode) || !"L".Equals(type)) return;

            DataRow fscRow = result.NewRow();
            fscRow["CHG_CD"] = "FSC";
            fscRow["CHG_DESCP"] = "Local Fuel Surcharge";
            fscRow["CUR"] = string.IsNullOrEmpty(cur) ? "CNY" : cur; 
            if (result.Select(string.Format("CHG_CD='FSC' AND CUR={0}", SQLUtils.QuotedStr(cur))).Length <= 0)
                result.Rows.Add(fscRow);
        }
        private static void AddFreightCode(DataTable result, string tran_mode, string cur = "")
        {
            DataRow row = null;
            switch (tran_mode)
            {
                case "E": 
                case "D":
                    row = result.NewRow();
                    row["CHG_CD"] = "CF";
                    row["CHG_DESCP"] = "COURIER CHARGE";
                    row["CUR"] = "CNY";
                    break;
                case "T":
                    row = result.NewRow();
                    row["CHG_CD"] = "DF";
                    row["CHG_DESCP"] = "Domestic Truck fee";
                    row["CUR"] = "CNY";
                    break;
                case "A":
                    row = result.NewRow();
                    row["CHG_CD"] = "AF";
                    row["CHG_DESCP"] = "Air Freight";
                    row["CUR"] = "CNY";
                    break;
                case "L":
                case "F":
                    row = result.NewRow();
                    row["CHG_CD"] = "OF";
                    row["CHG_DESCP"] = "Ocean Freight";
                    row["CUR"] = "USD";
                    break;
                case "R":
                    row = result.NewRow();
                    row["CHG_CD"] = "RF";
                    row["CHG_DESCP"] = "Railway Freight";
                    row["CUR"] = "CNY";
                    break;
            }
            if (row != null)
            {
                if (!string.IsNullOrEmpty(cur))
                    row["CUR"] = cur;
                string chgCd = Prolink.Math.GetValueAsString(row["CHG_CD"]);
                cur = Prolink.Math.GetValueAsString(row["CUR"]);
                if (result.Select(string.Format("CHG_CD={0} AND CUR={1}", SQLUtils.QuotedStr(chgCd), SQLUtils.QuotedStr(cur))).Length <= 0)
                    result.Rows.Add(row);
            }
        }
         

        /// <summary>
        /// 重新计算报价汇总
        /// </summary>
        /// <param name="id"></param>
        public static void SumAmt(string id,string u_fid)
        {
            if (string.IsNullOrEmpty(id))
                return;

            try
            {
                DataTable smbimDt = !string.IsNullOrEmpty(id) ?
                    OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection()) :
                     OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBIM WHERE U_ID IN (SELECT U_FID FROM SMBID WHERE U_ID={0})", SQLUtils.QuotedStr(u_fid)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (smbimDt.Rows.Count <= 0)
                    return;
                DataRow mian = smbimDt.Rows[0];
                if (string.IsNullOrEmpty(id))
                    id = Prolink.Math.GetValueAsString(mian["U_ID"]);

                DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMBID WHERE U_FID={0} ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", SQLUtils.QuotedStr(id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

                DateTime debitDate = (mian["DEBIT_DATE"] == null || mian["DEBIT_DATE"] == DBNull.Value) ? DateTime.Now : (DateTime)mian["DEBIT_DATE"];
                DataTable rateDt = OperationUtils.GetDataTable(string.Format("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' AND EDATE<={0} ORDER BY EDATE", SQLUtils.QuotedStr(debitDate.ToString("yyyy-MM-dd"))), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

                string main_cur = Prolink.Math.GetValueAsString(mian["CUR"]);
                decimal sum_amt = 0m;
                decimal sum_qamt = 0m;
                foreach (DataRow smbid in smbidDt.Rows)
                {
                    decimal qlamt = Prolink.Math.GetValueAsDecimal(smbid["QLAMT"]);
                    decimal qamt = Prolink.Math.GetValueAsDecimal(smbid["QAMT"]);
                    decimal bamt = Prolink.Math.GetValueAsDecimal(smbid["BAMT"]);
                    decimal lamt = Prolink.Math.GetValueAsDecimal(smbid["LAMT"]);

                    string qcur = Prolink.Math.GetValueAsString(smbid["QCUR"]);
                    string cur = Prolink.Math.GetValueAsString(smbid["CUR"]);
                    if (main_cur.Equals(cur))//请款币别
                    {
                        sum_amt += bamt;
                    }
                    else if (main_cur.Equals("CNY") && bamt != 0 && lamt != 0)
                        sum_amt += lamt;
                    else
                    {
                        decimal temp = 0m;
                        bool error = false;
                        decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt, null, bamt, cur, ref temp, ref error, main_cur);
                        sum_amt += temp;
                    }

                    if (main_cur.Equals(qcur))//请款币别
                    {
                        sum_qamt += qamt;
                    }
                    else if (main_cur.Equals("CNY") && qamt != 0 && qlamt != 0)
                        sum_qamt += qlamt;
                    else
                    {
                        decimal temp = 0m;
                        bool error = false;
                        decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt, null, qamt, qcur, ref temp, ref error, main_cur);
                        sum_qamt += temp;
                    }
                }
                EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.UPDATE_OPERATION);
                ei.Put("AMT", sum_amt);
                ei.Put("QAMT", sum_qamt);
                ei.PutKey("U_ID", id);

                int[] result = OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }
        }


        private bool CalTruck(DataTable rateDt, List<EditInstruct> elist, string CargoType, string bandType, string trackWay, decimal gw, decimal cbm, DataRow dr, string dn_no,bool add_ei)
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

        private static DataTable GetFirstCarrierRow(DataTable carrerDt)
        {
            DataTable result = carrerDt.Clone();
            if (carrerDt.Rows.Count <= 0)
                return result;
            string org_quot_no = Prolink.Math.GetValueAsString(carrerDt.Select("", "QT_EFFECT_FROM DESC,QUOT_NO")[0]["QUOT_NO"]);
            foreach (DataRow dr in carrerDt.Rows)
            {
                try
                {
                    string quot_no = Prolink.Math.GetValueAsString(dr["QUOT_NO"]);
                    if (org_quot_no.Equals(quot_no))
                        result.ImportRow(dr);
                }
                catch { }
            }
            return result;
        }

        public static DataTable GetBillDataTableByUId(string uid)
        {
            string sql = string.Format("SELECT DEBIT_NO,TPV_DEBIT_NO,DEBIT_DATE,LSP_NO,LSP_NM,CUR,AMT FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }

    public class Unit
    {
        /// <summary>
        /// 20'
        /// </summary>
        public static readonly string CNT20GP = "20GP";
        /// <summary>
        /// 40'
        /// </summary>
        public static readonly string CNT40GP = "40GP";
        /// <summary>
        /// 40HQ
        /// </summary>
        public static readonly string CNT40HQ = "40HQ";
        /// <summary>
        /// 提单
        /// </summary>
        public static readonly string BL = "BL";
        /// <summary>
        /// shipment
        /// </summary>
        public static readonly string SHT = "SHT";
        /// <summary>
        /// 货柜数
        /// </summary>
        public static readonly string CNT = "CNT";

        /// <summary>
        /// 箱数 CARTON
        /// </summary>
        public static readonly string CTN = "CTN";

        /// <summary>
        /// 板数
        /// </summary>
        public static readonly string PLT = "PLT";

        /// <summary>
        /// 千克
        /// </summary>
        public static readonly string KGS = "KGS";

        /// <summary>
        /// 立方米
        /// </summary>
        public static readonly string CBM = "CBM";
    }
}
