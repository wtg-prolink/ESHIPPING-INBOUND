using Business.Mail;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;
using Prolink.V3;
using Prolink;
using System.Globalization;
using WebGui.Controllers;

namespace Business
{
    public class ActHelper
    {
        private static string o_debitno = string.Empty;
        private static string o_cur = string.Empty;
        /*帐单檢查*/
        public static string ChkAct(string UId)
        {
            string msg = "success";
            string sql = "SELECT * FROM SMBID WHERE U_FID={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(UId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mixList = new MixedList();
            int no = 1;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string _uid = Prolink.Math.GetValueAsString(item["U_ID"]);
                    decimal Qamt = Prolink.Math.GetValueAsDecimal(item["QAMT"]);
                    decimal Bamt = Prolink.Math.GetValueAsDecimal(item["BAMT"]);
                    string CheckDescp = "";
                    string Status = "Y";
                    
                    sql = "UPDATE SMBID SET CHECK_DESCP={0}, STATUS={1} WHERE U_ID={2}";

                    if (Qamt > 0 && Bamt == 0)
                    {
                        CheckDescp = @Resources.Locale.L_ActHelper_Business_0;
                        Status = "N";
                        no = 0;
                    }
                    else if(Qamt > Bamt)
                    {
                        CheckDescp = @Resources.Locale.L_ActManage_Controllers_69;
                        Status = "N";
                        no = 0;
                    }
                    else if (Qamt < Bamt)
                    {
                        CheckDescp = @Resources.Locale.L_ActManage_Controllers_70;
                        Status = "N";
                        no = 0;
                    }

                    sql = string.Format(sql, SQLUtils.QuotedStr(CheckDescp), SQLUtils.QuotedStr(Status), SQLUtils.QuotedStr(_uid));
                    mixList.Add(sql);
                }

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (no == 0)
                    {
                        msg = "no pass";
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }

            return msg;
        }

        public static void ChkActBySingle(string UId)
        {
            string msg = "success";
            string sql = "SELECT * FROM SMBID WHERE U_ID={0}";
            sql = string.Format(sql, SQLUtils.QuotedStr(UId));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mixList = new MixedList();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string _uid = Prolink.Math.GetValueAsString(item["U_ID"]);
                    decimal Qamt = Prolink.Math.GetValueAsDecimal(item["QAMT"]);
                    decimal Bamt = Prolink.Math.GetValueAsDecimal(item["BAMT"]);
                    string CheckDescp = "";
                    string Status = "Y";

                    sql = "UPDATE SMBID SET CHECK_DESCP={0}, STATUS={1} WHERE U_ID={2}";

                    if (Qamt > 0 && Bamt == 0)
                    {
                        CheckDescp = @Resources.Locale.L_ActHelper_Business_0;
                        Status = "N";
                    }
                    else if (Qamt > Bamt)
                    {
                        CheckDescp = @Resources.Locale.L_ActManage_Controllers_69;
                        Status = "N";
                    }
                    else if (Qamt < Bamt)
                    {
                        CheckDescp = @Resources.Locale.L_ActManage_Controllers_70;
                        Status = "N";
                    }

                    sql = string.Format(sql, SQLUtils.QuotedStr(CheckDescp), SQLUtils.QuotedStr(Status), SQLUtils.QuotedStr(_uid));
                    mixList.Add(sql);
                }

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }
        }

        /*帐单通过进入审核流程*/
        public static string CreateApprove(string UId, UserInfo userinfo, string Cmp)
        {
            string msg = "success";

            DateTime odt = DateTime.Now;
            string CompanyId = Cmp;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            //,VERIFY_CMP={0}, VERIFY_BY={1}, VERIFY_DATE={2}, VERIFY_DATE_L={3}
            string sql = "UPDATE SMBIM SET STATUS='D',APPROVE_TO='A' WHERE U_ID={4}";
            sql = string.Format(sql, SQLUtils.QuotedStr(userinfo.CompanyId), SQLUtils.QuotedStr(userinfo.UserId), SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(UId));
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                sql = "SELECT DEBIT_NO FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                string DebitNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if (DebitNo == "")
                {
                    msg = @Resources.Locale.L_ActHelper_Business_1 + "\n";
                }
                else
                {
                    
                    msg = Business.BillApproveHelper.ApproveBillItem(UId, DebitNo, userinfo) + "\n";
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }



            return msg;
        }

        #region 帐单excel导入相关操作
        public static string chkSmbimIsExist(string ShipmentId, string DebitNo, string LspNo, DateTime DebitDate)
        {

            string sql = "SELECT COUNT(*) FROM SMBIM WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND (DEBIT_NO='' OR DEBIT_NO IS NULL) AND STATUS IN('V', 'D', 'E', 'F')";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));

            int cnt = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if(cnt > 0)
            {
                return "";
            }

            sql = "SELECT TOP 1 U_ID FROM SMBIM WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND (DEBIT_NO='' OR DEBIT_NO IS NULL) AND STATUS NOT IN('V', 'D', 'E', 'F')";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));

            string smbimUid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            // Paul: 先用shipment_id + 空的帳單號碼+ lsp_no  去抓, 抓得到的都是同一筆.如果發現沒有空的帳單號碼, 再用 shipment_id+帳單號碼+lsp_no  抓一次,  抓得到就update , 抓不到就新增
            if(String.IsNullOrEmpty(smbimUid))
            {
                sql = "SELECT TOP 1 U_ID FROM SMBIM WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND DEBIT_NO={2} AND STATUS NOT IN('V', 'D', 'E', 'F')";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(DebitNo));
                smbimUid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                if(!String.IsNullOrEmpty(smbimUid))
                {
                    sql = "UPDATE SMBIM SET DEBIT_NO={0}, DEBIT_DATE={1} WHERE U_ID={2}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(DebitNo), SQLUtils.QuotedStr(DebitDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(smbimUid));
                    try
                    {
                        Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch
                    {
                        smbimUid = "";
                    }
                }
                else
                {
                    #region 判断帐单表头是否存在，不在在则产生帐单
                    if (String.IsNullOrEmpty(smbimUid))
                    {
                        sql = "SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        string sm_id = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                        bill.Create(sm_id, DateTime.Now);

                        sql = "SELECT TOP 1 U_ID FROM SMBIM WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND (DEBIT_NO='' OR DEBIT_NO IS NULL)";
                        sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));
                        smbimUid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (!String.IsNullOrEmpty(smbimUid))
                        {
                            sql = "UPDATE SMBIM SET DEBIT_NO={0}, DEBIT_DATE={1} WHERE SHIPMENT_ID={2} AND LSP_NO={3} AND (DEBIT_NO='' OR DEBIT_NO IS NULL)";
                            sql = string.Format(sql, SQLUtils.QuotedStr(DebitNo), SQLUtils.QuotedStr(DebitDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));
                            try
                            {
                                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch
                            {
                                smbimUid = "";
                            }
                        }
                    }
                    #endregion
                }
            }
            else
            {
                sql = "UPDATE SMBIM SET DEBIT_NO={0}, DEBIT_DATE={1} WHERE SHIPMENT_ID={2} AND LSP_NO={3} AND (DEBIT_NO='' OR DEBIT_NO IS NULL)";
                sql = string.Format(sql, SQLUtils.QuotedStr(DebitNo), SQLUtils.QuotedStr(DebitDate.ToString("yyyy-MM-dd")), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo));
                try
                {
                    Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch
                {
                    smbimUid = "";
                }
            }

            return smbimUid;
        }

        public static string easyCheckSmbim(string DebitNo, string LspNo, string Cmp)
        {
            string sql = "SELECT U_ID FROM SMBIM WHERE DEBIT_NO={0} AND LSP_NO={1} AND CMP={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(DebitNo), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Cmp));
            string uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            return uid;
        }
        
        public static string chkSmbidIsExist(string ShipmentId, string LspNo, string ChgCd)
        {

            string sql = "SELECT U_ID FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND CHG_CD={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(ChgCd));

            string UId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            return UId;
        }

        public static string chkUfidIsExist(string ShipmentId, string LspNo, string ChgCd)
        {

            string sql = "SELECT U_FID FROM SMBID WHERE SHIPMENT_ID={0} AND LSP_NO={1} AND CHG_CD={2}";
            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(ChgCd));

            string UFid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            return UFid;
        }

        public static bool chkChgCdIsExist(string ChgCd)
        {
            string sql = "SELECT COUNT(*) FROM SMCHG WHERE CHG_CD=" + SQLUtils.QuotedStr(ChgCd);
            int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if(num > 0)
            {
                return true;
            }

            return false;
        }

        /*
         * dr[0]: CMP
         * dr[1]: SHIPMENT_ID
         * dr[2]:
         * dr[2]: BL_NO
         * dr[3]: DEBIT_DATE
         * dr[4]: DEBIT_NO
         * dr[5]: CHG_CD
         * dr[6]: CHG_DESCP
         * dr[7]: CUR
         * dr[8]: UNIT_PRICE
         * dr[9]: CHG_UNIT
         * dr[10]: QTY
         * dr[11]: Qtyu
         * dr[12]: BAMT
         * dr[13]: EX_RATE
         * dr[14]: LAMT
         * dr[15]: TAX
         * dr[16]: VAT_NO
         * dr[17]: REMARK
         */
        public static string InsertOrUpdateSmbid(DataTable dt, string LspNo)
        {
            string msg = "success";
            EditInstruct ei;
            MixedList mx = new MixedList();
            int[] resulst;
            string smbidUid = string.Empty;
            string oldSmId = "";
            string smbimUid = "";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    string Cmp = Prolink.Math.GetValueAsString(dr[0]);
                    string ShipmentId = Prolink.Math.GetValueAsString(dr[1]);
                    string BlNo = Prolink.Math.GetValueAsString(dr[2]);
                    DateTime DebitDate = Prolink.Math.GetValueAsDateTime(dr[3]);
                    string DebitNo = Prolink.Math.GetValueAsString(dr[4]);
                    string ChgCd = Prolink.Math.GetValueAsString(dr[5]);
                    string ChgDescp = Prolink.Math.GetValueAsString(dr[6]);
                    string Cur = Prolink.Math.GetValueAsString(dr[7]);
                    decimal UnitPrice = Prolink.Math.GetValueAsDecimal(dr[8]);
                    string ChgUnit = Prolink.Math.GetValueAsString(dr[9]);
                    decimal Qty = Prolink.Math.GetValueAsDecimal(dr[10]);
                    string Qtyu = Prolink.Math.GetValueAsString(dr[11]);
                    decimal Bamt = Prolink.Math.GetValueAsDecimal(dr[12]);
                    decimal ExRate = Prolink.Math.GetValueAsDecimal(dr[13]);
                    decimal Lamt = Prolink.Math.GetValueAsDecimal(dr[14]);
                    decimal Tax = Prolink.Math.GetValueAsDecimal(dr[15]);
                    string VatNo = Prolink.Math.GetValueAsString(dr[16]);
                    string Remark = Prolink.Math.GetValueAsString(dr[17]);
                    

                    if (oldSmId != ShipmentId)
                    {
                        resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
                        mx = new MixedList();
                        if (i > 0)
                        {
                            string chk = ActHelper.ChkAct(smbimUid); //帐单比对
                        }
                        smbimUid = ActHelper.chkSmbimIsExist(ShipmentId, DebitNo, LspNo, DebitDate);
                        if (String.IsNullOrEmpty(smbimUid))
                        {
                            msg += "Shipment ID:" + ShipmentId + @Resources.Locale.L_ActHelper_Business_70 + "\n";
                            continue;
                        }
                    }

                    if(!ActHelper.chkChgCdIsExist(ChgCd))
                    {
                        msg += ChgCd + @Resources.Locale.L_ActHelper_Business_6 + "\n";
                        continue;
                    }

                    smbidUid = ActHelper.chkSmbidIsExist(ShipmentId, LspNo, ChgCd);

                    if (String.IsNullOrEmpty(smbidUid))
                    {
                        ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    }
                    else
                    {
                        ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                        ei.PutKey("U_ID", smbidUid);
                    }

                    ei.Put("U_FID", smbimUid);
                    ei.Put("SHIPMENT_ID", ShipmentId);
                    ei.Put("BL_NO", BlNo);
                    ei.Put("LSP_NO", LspNo);
                    ei.PutDate("DEBIT_DATE", DebitDate);
                    ei.Put("DEBIT_NO", DebitNo);
                    ei.Put("CHG_CD", ChgCd);
                    ei.Put("CHG_DESCP", ChgDescp);
                    ei.Put("CUR", Cur);
                    ei.Put("UNIT_PRICE", UnitPrice);
                    ei.Put("CHG_UNIT", ChgUnit);
                    ei.Put("QTY", Qty);
                    ei.Put("QTYU", Qtyu);
                    ei.Put("BAMT", Bamt);
                    ei.Put("EX_RATE", ExRate);
                    ei.Put("LAMT", Lamt);
                    ei.Put("TAX", Tax);
                    ei.Put("VAT_NO", VatNo);
                    ei.Put("REMARK", Remark);
                    mx.Add(ei);
                    oldSmId = ShipmentId;

                }
                resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }

            return msg;
        }

        public static string HandleBillDetail(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            int seq = Prolink.Math.GetValueAsInt(parm["seq"]);
            seq++;
            parm["seq"] = seq;
            string sql = string.Empty;
            string msg = string.Empty;
            
            string smbimUid = Prolink.Math.GetValueAsString(parm["smbimUid"]);
            string ShipmentId = Prolink.Math.GetValueAsString(ei.Get("SHIPMENT_ID"));
            string DebitNo = Prolink.Math.GetValueAsString(ei.Get("DEBIT_NO"));
            string ChgCd = Prolink.Math.GetValueAsString(ei.Get("CHG_CD"));
            string ChgDescp = Prolink.Math.GetValueAsString(ei.Get("CHG_DESCP"));
            decimal Bamt = Prolink.Math.GetValueAsDecimal(ei.Get("BAMT"));
            string Cur = Prolink.Math.GetValueAsString(ei.Get("CUR"));
            string LspNo = Prolink.Math.GetValueAsString(parm["LspNo"]);
            string LspNm = Prolink.Math.GetValueAsString(parm["LspNm"]);
            string autoChk = Prolink.Math.GetValueAsString(parm["autoChk"]);
            string new_debitno = string.Empty;
            sql = "SELECT GROUP_ID, CMP, ETD FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable smdt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            string gid = string.Empty;
            string cmp = string.Empty;
            DateTime etd = DateTime.Now;
            msg = "\n" + @Resources.Locale.L_ActHelper_Business_7 + seq.ToString() + @Resources.Locale.L_ActHelper_Business_8 + "\n";
            if (smdt.Rows.Count > 0)
            {
                gid = Prolink.Math.GetValueAsString(smdt.Rows[0]["GROUP_ID"]);
                cmp = Prolink.Math.GetValueAsString(smdt.Rows[0]["CMP"]);
                try
                {
                    etd = Convert.ToDateTime(smdt.Rows[0]["ETD"]);
                }
                catch (Exception ex)
                { 
                    
                }
            }
            else
            {
                msg += @Resources.Locale.L_ActHelper_Business_9 + "\n";
                parm["msg"] += msg; 
                return ExcelParser.ERROR;
            }
            
            DateTime DebitDate = DateTime.Now;
            
            #region 檢查是否有填帳單日期
            string chk_debit_date = Prolink.Math.GetValueAsString(ei.Get("DEBIT_DATE"));
                if (chk_debit_date != "")
                {
                    DateTime d_time = Prolink.Math.GetValueAsDateTime(ei.Get("DEBIT_DATE"));
                    string debit_date = d_time.Year.ToString() + "-" + d_time.Month.ToString().PadLeft(2, '0') + "-" + d_time.Day.ToString().PadLeft(2, '0');
                    DebitDate = Convert.ToDateTime(debit_date);
                }
                else
                {
                    DebitDate = etd;
                }
                #endregion

            #region 檢查有無shipment id, 費用代碼, 幣別，若沒有回傳error
            if (ShipmentId != "" && ChgCd != "" && Cur != "")
            {
                #region 檢查費用代碼存在否
                sql = "SELECT COUNT(*) FROM SMCHG WHERE CHG_CD={0} AND CMP={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(cmp));
                int chg_exist = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (chg_exist == 0)
                {
                    msg += @Resources.Locale.L_ActHelper_Business_10 + "\n";
                    parm["msg"] += msg; 
                    return ExcelParser.ERROR;
                }
                #endregion
                
                #region 檢查shipment ID存在否
                sql = "SELECT COUNT(*) FROM SMSM WHERE SHIPMENT_ID={0} AND CMP={1}";
                sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(cmp));
                int sm_exist = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (sm_exist == 0)
                {
                    msg += @Resources.Locale.L_ActHelper_Business_9;
                    parm["msg"] += msg; 
                    return ExcelParser.ERROR;
                }
                #endregion

                #region 帳單號與幣別 跟上一筆不同時，代表要產生新的表頭
                if (o_debitno != DebitNo || o_cur != Cur || seq == 1)
                {
                    if (seq > 1 && autoChk == "Y")
                    {
                        string chk = ActHelper.ChkAct(smbimUid); //帐单比对
                        #region 比對後更新主檔狀態
                        if (chk == "success")
                        {
                            sql = "UPDATE SMBIM SET STATUS='G' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                chk = ex.ToString();
                            }
                        }
                        else if (chk == "no pass")
                        {
                            sql = "UPDATE SMBIM SET STATUS='B' WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                chk = ex.ToString();
                            }
                        }
                        #endregion
                    }


                    string smbidUFid = ActHelper.chkUfidIsExist(ShipmentId, LspNo, ChgCd);
                    #region 明細的ufid不存在才可以產生表頭
                    if (smbidUFid == "")
                    {
                        if (DebitNo == "")
                        {
                            smbimUid = System.Guid.NewGuid().ToString();
                            new_debitno = Business.ReserveManage.getAutoNo("DEBIT_NO", gid, cmp);
                            parm["newDebitNo"] = new_debitno;
                            EditInstruct m_ei = ActHelper.CreateBillEditInstruct(smbimUid, LspNo, LspNm, new_debitno, gid, cmp, Cur);
                            try
                            {
                                int[] result = OperationUtils.ExecuteUpdate(m_ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                                parm["smbimUid"] = smbimUid;
                            }
                            catch (Exception ex)
                            {
                                msg += @Resources.Locale.L_ActHelper_Business_11 + "\n";
                                parm["msg"] += msg;
                                return ExcelParser.ERROR;
                            }
                        }
                        else
                        {
                            parm["newDebitNo"] = DebitNo;
                            string smbim_isexist = ActHelper.easyCheckSmbim(DebitNo, LspNo, cmp);
                            if (smbim_isexist != "")
                            {
                                smbimUid = smbim_isexist;
                                parm["smbimUid"] = smbimUid;
                            }
                            else
                            {
                                smbimUid = System.Guid.NewGuid().ToString();
                                EditInstruct m_ei = ActHelper.CreateBillEditInstruct(smbimUid, LspNo, LspNm, DebitNo, gid, cmp, Cur);
                                try
                                {
                                    int[] result = OperationUtils.ExecuteUpdate(m_ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                                    parm["smbimUid"] = smbimUid;
                                }
                                catch (Exception ex)
                                {
                                    msg += @Resources.Locale.L_ActHelper_Business_11;
                                    parm["msg"] += msg;
                                    return ExcelParser.ERROR;
                                }
                            }
                        }
                        ei.Put("U_FID", smbimUid);
                    }       
                    #endregion
                    #region 否則，只update該筆明細
                    else
                    {
                        smbimUid = smbidUFid;
                        parm["smbimUid"] = smbimUid;
                        #region 檢查該明細主檔是否已通過，若通過後該明細不能修改
                        sql = "SELECT STATUS FROM SMBIM WHERE U_ID=" + SQLUtils.QuotedStr(smbimUid);
                        string m_status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (m_status != "A" && m_status != "B" && m_status != "G")
                        {
                            msg += @Resources.Locale.L_ActHelper_Business_12 + "\n";
                            parm["msg"] += msg;
                            return ExcelParser.ERROR;
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    ei.Put("U_FID", smbimUid);
                }
                #endregion

                #region 檢查該費用是否存在費用明細，若不在則新增
                string smbidUid = ActHelper.chkSmbidIsExist(ShipmentId, LspNo, ChgCd);
                if (String.IsNullOrEmpty(smbidUid))
                {
                    ei.OperationType = EditInstruct.INSERT_OPERATION;
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.Put("LSP_NO", LspNo);
                    ei.Put("LSP_NM", LspNm);
                    ei.Put("DEBIT_NO", parm["newDebitNo"]);
                }
                else
                {
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.PutKey("U_ID", smbidUid);
                    ei.Put("DEBIT_NO", parm["newDebitNo"]);
                    ei.Put("QT_DATA", "Y");
                    ei.Remove("SHIPMENT_ID");
                    ei.Remove("BL_NO");
                    ei.Remove("CHG_CD");
                    ei.Remove("CHG_DESCP");
                }
                if (ChgDescp == "")
                {
                    sql = "SELECT CHG_DESCP FROM SMCHG WHERE CHG_CD={0} AND CMP={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(cmp));
                    ChgDescp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ei.Put("CHG_DESCP", ChgDescp);
                }
                #endregion
            }
            #endregion
            else
            {
                msg += @Resources.Locale.L_ActHelper_Business_13 + " \n";
                parm["msg"] += msg; 
                return ExcelParser.ERROR;
            }

            ei.PutDate("DEBIT_DATE", DebitDate);
            o_cur = Cur;
            o_debitno = DebitNo;
            return string.Empty;
        }
        #endregion

        public static EditInstruct CreateBillEditInstruct(string debit_no, string lsp_no, string lsp_nm, string DebitNo, string gid, string cmp, string Cur)
        {
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", debit_no);
            ei.Put("GROUP_ID", gid);
            ei.Put("CMP", cmp);
            //ei.Put("SHIPMENT_ID", smsm["SHIPMENT_ID"]);
            ei.Put("DEBIT_NO", DebitNo);
            ei.Put("STATUS", "A");//"A:錄製 B:發送 C:拒絕 D:通過 E:請款  F:已付款   V:作廢"
            DateTime odt = DateTime.Now;
            ei.PutDate("DEBIT_DATE", odt);//帳單日期除海運整櫃出貨到北美,(Region:NA)的地區依據進港日期結算外,其他的都是以Onboard date 為主,內貿是以離場日為主
            
            string CompanyId = cmp;
            DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
            
            ei.PutDate("DEBIT_DATE_L", ndt);

            //ei.Put("DEBIT_NO", debit_no);
            ei.Put("LSP_NO", lsp_no);
            ei.Put("LSP_NM", lsp_nm);

            //ei.Put("POL", smsm["POL_CD"]);
            //ei.Put("POL_NM", smsm["POL_NAME"]);
            //ei.Put("POD", smsm["POD_CD"]);
            //ei.Put("POD_NM", smsm["POD_NAME"]);
            //ei.Put("QTY", smsm["QTY"]);
            //ei.Put("QTYU", smsm["QTYU"]);
            //ei.Put("GW", smsm["GW"]);
            //ei.Put("GWU", smsm["GWU"]);
            ////ei.Put("CW", smsm["CW"]); 计费重
            //ei.Put("CBM", smsm["CBM"]);
            ei.Put("CUR", Cur);
            ei.Put("EX_RATE", 0);//匯率
            ei.Put("LAMT", 0);
            ei.Put("APPROVE_TO", "1");
            ei.Put("APPROVE_TYPE", "STD_BILL");
            ei.Put("CREATE_BY", "SYSTEM");
            ei.PutDate("CREATE_DATE", odt);
            ei.PutDate("CREATE_DATE_L", ndt);
            return ei;
        }
        
        public static bool GetInvPayCheck(string uid)
        {
            string sql = string.Format("SELECT TOP 1 INV_PAY_CHECK FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            string result = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            return result == "Y";
        }

        public static void UpdateInvPayCheck(string uid, string input)
        {
            string sql = string.Format("UPDATE SMBIM SET INV_PAY_CHECK = {1} WHERE U_ID={0}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(input));
            int result = OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string GetOperType(string UId)
        {
            string type = "all";
            if (string.IsNullOrEmpty(UId))
                return type;
            string sql = string.Format("SELECT STATUS FROM SMBIM WHERE U_ID={0}", SQLUtils.QuotedStr(UId));
            string status = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            switch (status)
            {
                case "D":
                case "E":
                case "F":
                    type = "attachment";
                    break;
            }
            return type;
        }
    }
}