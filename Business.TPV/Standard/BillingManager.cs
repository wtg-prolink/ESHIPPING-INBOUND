using Business.Service;
using Business.TPV.Base;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static TrackingEDI.InboundBusiness.InboundHelper;

namespace Business.TPV.Standard
{
    public class BillingManager : ImprotManager<BillingInfo>
    {
        Dictionary<string, string> lang = new Dictionary<string, string>();
        public BillingManager(Dictionary<string, string> langData)
        {
            this.lang = langData;
        }
        public override ResultInfo ImportInstanceList(IEnumerable<BillingInfo> infos)
        {
            var result = base.ImportInstanceList(infos);
            foreach (var item in infos)
            {
                if (item.BillingDetails == null || item.BillingDetails.Length <= 0) continue;
                foreach (var detail in item.BillingDetails)
                {
                    BillingEDILog log = new BillingEDILog(item, detail);
                    if (result.IsSucceed)
                    {
                        Logger.WriteLog(log.CreateSucceed());
                    }
                    else
                    {
                        Logger.WriteLog(log.CreateEx(result.Description));
                    }
                }
            }
            return result;
        }

        //protected override EditInstructList ToEi(BillingInfo obj)
        //{
        //    DataTable billDT = QueryBillDT_D(obj);
        //    //DataTable billDT_M = QueryBillDT_M(obj);
        //    DataTable smDT=null;
        //    if(obj!=null&&obj.BillingDetails!=null&&obj.BillingDetails.Length>0)
        //    {
        //        smDT=Helper.QuerySM(obj.BillingDetails.Select(d=>d.ShipmentID));
        //    }
        //    EditInstructList eiList = new EditInstructList();
        //    List<dynamic> createdM = new List<dynamic>();
        //    foreach (var v in obj.BillingDetails)
        //    {
        //        string fid = null;
        //        //eiList.MergeEditInstruct(CreateBillingEI_M(obj, v, billDT_M, smDT, createdM, out fid));
        //        eiList.MergeEditInstruct(CreateBillingEI(obj, v, billDT, fid));
                
        //        //MixedList ml = new MixedList();
               
        //        //DataTable dt= CreateBillingEI(obj, v, billDT);
        //        //string LspNo = obj.Sender;
        //        //ImportDatatableToBill(dt, LspNo, ml,1);
        //        //for (int i = 0; i < ml.Count; i++)
        //        //{
        //        //    if (ml[i] is EditInstruct)
        //        //    {
        //        //        EditInstruct ei = (EditInstruct)ml[i];
        //        //        eiList.MergeEditInstruct(ei);
        //        //    }
        //        //}
        //    }
        //    return eiList;
        //}

        //protected override EditInstructList ToEi(BillingInfo obj)
        //{
        //    DataTable billDT = QueryBillDT_D(obj);
        //    DataTable smDT = null;
        //    if (obj != null && obj.BillingDetails != null && obj.BillingDetails.Length > 0)
        //    {
        //        smDT = Helper.QuerySM(obj.BillingDetails.Select(d => d.ShipmentID));
        //    }
        //    EditInstructList eiList = new EditInstructList();
        //    foreach (var v in obj.BillingDetails)
        //    {
        //        string fid = null;
        //        eiList.MergeEditInstruct(CreateBillingEI(obj, v, billDT, fid));
        //    }
        //    return eiList;
        //}
        DataTable QueryBillDT_D(BillingInfo obj)
        {
            string sql = string.Format("SELECT U_ID,U_FID,DEBIT_NO,CHG_CD,SHIPMENT_ID FROM SMBID WHERE LSP_NO={0}", SQLUtils.QuotedStr(obj.Sender));
            return QueryBillDT(obj, sql);
        }
        DataTable QueryBillDT_M(BillingInfo obj)
        {
            string sql = string.Format("SELECT U_ID,DEBIT_NO,SHIPMENT_ID FROM SMBIM WHERE LSP_NO={0}", SQLUtils.QuotedStr(obj.Sender));
            return QueryBillDT(obj,sql);
        }
        DataTable QueryBillDT(BillingInfo obj,string sql)
        {
            List<string> list = GetShipmentIdList(obj).ToList();
            if (list != null && list.Count > 0)
            {
                sql = string.Join(" AND ", sql, string.Format("({0})", string.Join(" OR ", list.Select(c => string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(c))))));
            }
            return DB.GetDataTable(sql, new string[] { });
        }

        IEnumerable<string> GetShipmentIdList(BillingInfo obj)
        {
            if (obj.BillingDetails == null || obj.BillingDetails.Length <= 0) return new List<string>();
            return obj.BillingDetails.Select(x => x.ShipmentID).Distinct().Where(s => !string.IsNullOrEmpty(s));
        }

        protected override ResultInfo CheckForBusiness(BillingInfo item, DataRow row)
        {
            if (item.BillingDetails == null || item.BillingDetails.Length <= 0) return ResultInfo.NullDataResult();
            List<DataRow> rows = new List<DataRow>();
            List<string> list = item.BillingDetails.Select(x => x.ShipmentID).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            foreach (var v in list)
            {
                DataRow br = Helper.QuerySM(v);
                ResultInfo r = UnknowResult();
                if (br == null)
                {
                    r.ResultCode = ResultCode.ValidateException;
                    r.Description = string.Format("{0}:该笔Shipment ID系统不存在,请验证数据!", v);
                    return r;
                }
                rows.Add(br);
            }
            item.SMRows = rows;
            return SucceedResult();
        }

        EditInstruct CreateBillingEI_M(BillingInfo obj, BillingDetail detail, DataTable billDT_M, DataTable smDT,List<dynamic> createdList, out string fid)
        {
            Func<DataRow> checkHas = () =>
            {
                if (billDT_M == null || billDT_M.Rows.Count <= 0) return null;
                DataRow[] rows = billDT_M.Select(string.Format("DEBIT_NO={0} AND SHIPMENT_ID={1}", SQLUtils.QuotedStr(detail.DebitNO), SQLUtils.QuotedStr(detail.ShipmentID)));
                if (rows == null || rows.Length <= 0) return null;
                return rows[0];
            };
            DataRow row = checkHas();
            if (row != null)
            {
                fid = Prolink.Math.GetValueAsString(row["U_ID"]);
                return null;
            }
            dynamic d = createdList.Where(x => x.dNO == detail.DebitNO && x.sID == detail.ShipmentID).FirstOrDefault();
            if (d != null)
            {
                fid = d.id;
                return null;
            }
            Func<DataRow> getSMRow = () =>
                {
                    if (smDT == null || smDT.Rows.Count <= 0) return null;
                    DataRow[] rows = smDT.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(detail.ShipmentID)));
                    if (rows == null || rows.Length <= 0) return null;
                    return rows[0];
                };
            EditInstruct ei = new EditInstruct("SMBIM", EditInstruct.INSERT_OPERATION);
            fid = Guid.NewGuid().ToString();
            createdList.Add(new { dNO = detail.DebitNO, sID = detail.ShipmentID, id = fid });
            ei.Put("U_ID", fid);
            DataRow smRow = getSMRow();
            string debitNO = detail.DebitNO;
            if (smRow != null)
            {
                string cmp = Prolink.Math.GetValueAsString(smRow["CMP"]);
                ei.Put("GROUP_ID", smRow["GROUP_ID"]);
                ei.Put("CMP", cmp);
                ei.Put("SHIPMENT_ID", detail.ShipmentID);
                if (string.IsNullOrEmpty(debitNO))
                    debitNO = Business.TPV.Utils.AutoNOManager.GetAutoNo("DEBIT_NO", "TPV", cmp);
            }           
            ei.Put("DEBIT_NO", debitNO);
            ei.Put("STATUS", "A");//"A:錄製 B:發送 C:拒絕 D:通過 E:請款  F:已付款   V:作廢"
            ei.PutExpress("DEBIT_DATE", "getdate()");
            ei.Put("LSP_NO", obj.Sender);
            ei.Put("EX_RATE", 0);//匯率
            ei.Put("LAMT", 0);
            ei.Put("APPROVE_TO", "1");
            ei.Put("APPROVE_TYPE", "STD_BILL");
            ei.Put("CREATE_BY", obj.Sender);
            ei.PutExpress("CREATE_DATE", "getdate()");
            return ei;
        }

        EditInstruct CreateBillingEI(BillingInfo obj, BillingDetail detail, DataTable billDT,string fid)
        {
            Func<DataRow> checkHas = () =>
                {
                    if (billDT == null || billDT.Rows.Count <= 0) return null;
                    DataRow[] rows = billDT.Select(string.Format("DEBIT_NO={0} AND CHG_CD={1} AND SHIPMENT_ID={2}", SQLUtils.QuotedStr(detail.DebitNO),
                        SQLUtils.QuotedStr(detail.ChargeCode), SQLUtils.QuotedStr(detail.ShipmentID)));
                    if (rows == null || rows.Length <= 0) return null;
                    return rows[0];
                };
            DataRow br = checkHas();
            EditInstruct ei = null;
            if (br != null)
            {
                ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                ei.Put("MODIFY_BY", obj.Sender);
                ei.PutExpress("MODIFY_DATE", "getdate()");
                ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(br["U_ID"])));
            }
            else
            {
                ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("CREATE_BY", obj.Sender);
                ei.PutExpress("CREATE_DATE", "getdate()");
            }
            double bamt =Prolink.Math.GetValueAsDouble(detail.Amount);
                //ei.Put("U_FID", fid);
            string ChgCd = detail.ChargeCode;
            string ShipmentId= detail.ShipmentID;
            ei.Put("SHIPMENT_ID", ShipmentId);
            ei.Put("DEBIT_NO", detail.DebitNO);
            ei.Put("VAT_NO", detail.VatNO);
            string LspNo = obj.Sender;
            ei.Put("LSP_NO", LspNo);
            string sql = "SELECT CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ei.Put("LSP_NM", LspNm);
            ei.Put("BL_NO", detail.BLNO);
            ei.Put("CHG_CD", ChgCd);
            ei.Put("CHG_DESCP", GetSysChgDescp(ChgCd, ShipmentId));//ei.Put("CHG_DESCP", detail.ChargeDescp);
            ei.Put("BAMT", bamt);
            ei.Put("CUR", detail.Currency);
            ei.Put("CHG_UNIT", detail.ChargeUnit);
            
            ei.Put("QTY", detail.QTY);
            ei.Put("QTYU", detail.QTYUnit);
            ei.Put("UNIT_PRICE", detail.UnitPrice);
            ei.Put("BI_REMARK", detail.Remark);
            ei.Put("TAX", Prolink.Math.GetValueAsDouble(detail.Tax));
            string condition = string.Format(@"SHIPMENT_ID='{0}'", detail.ShipmentID);
            DataTable SMDT = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSM WHERE {0}", condition),null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable chgDt = OperationUtils.GetDataTable(string.Format("SELECT CHG_CD,CHG_TYPE FROM SMCHG WHERE CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable smptDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,PARTY_NO,PARTY_NAME,PARTY_TYPE FROM SMSMPT WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] partySH = Business.TPV.Financial.Helper.GetParty(ShipmentId, smptDt, new string[] { "SH" });

            string chg_type = string.Empty;
            if (chgDt.Rows.Count > 0)
            {
                chg_type = Prolink.Math.GetValueAsString(chgDt.Rows[0]["CHG_TYPE"]);
                ei.Put("CHG_TYPE", chg_type);
            }
            foreach (DataRow dr in SMDT.Rows)
            {
                ei.Put("GROUP_ID", dr["GROUP_ID"]);
                ei.Put("CMP", dr["CMP"]);

                //
                string my_debit_no = string.Empty;
                string my_debit_nm = string.Empty;
                switch (chg_type)
                {
                    case "F":
                    case "D":
                        my_debit_no = Prolink.Math.GetValueAsString(dr["DEBIT_TO"]);
                        my_debit_nm = Prolink.Math.GetValueAsString(dr["DEBIT_NM"]);
                        break;
                }
                if (string.IsNullOrEmpty(my_debit_no) && partySH != null && partySH.Length > 0)
                {
                    my_debit_no = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NO"]);
                    my_debit_nm = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NAME"]);
                }

                ei.Put("DEBIT_TO", my_debit_no);
                ei.Put("DEBIT_NM", my_debit_nm);
            }
            condition = string.Format(@"SHIPMENT_ID='{0}' AND CHG_CD='{1}' AND LSP_NO='{2}'", detail.ShipmentID, detail.ChargeCode, obj.Sender);
            DataTable qtTempDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,LSP_NO,QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT,DEBIT_NM,DEBIT_TO FROM SMBID_TEMP WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow temps in qtTempDt.Rows)
            {
                string qcur = Prolink.Math.GetValueAsString(temps["CUR"]);
                decimal qamt = Prolink.Math.GetValueAsDecimal(temps["QAMT"]);
                decimal qlamt = Prolink.Math.GetValueAsDecimal(temps["QLAMT"]);
                ei.Put("QUOT_NO", temps["QUOT_NO"]);
                ei.Put("CHG_TYPE", temps["CHG_TYPE"]);
                ei.Put("QCUR", qcur);
                ei.Put("QAMT", 0);
                ei.Put("QLAMT", 0);
                ei.Put("DEBIT_NM", temps["DEBIT_NM"]);
                ei.Put("DEBIT_TO", temps["DEBIT_TO"]);
                ei.Put("REMARK", string.Format("预提报价:{0}{1},预提CNY报价:{2}", qamt, qcur, qlamt));
                ChkAct((decimal)bamt,qamt,ei);
            }
            DataTable rateDt = OperationUtils.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (detail.DebitDate.HasValue)
            {
                DateTime DebitDate = detail.DebitDate.Value;
                ei.PutDate("DEBIT_DATE", detail.DebitDate.Value);
                DataTable rateDt1 = GetRate(rateDt, DebitDate);
                decimal Blamt = 0M;
                bool error = false;
                string cur = detail.Currency;
                decimal rate = Financial.Helper.GetTotal(rateDt1, null, (decimal)bamt, cur, ref Blamt, ref error, "CNY");
                ei.Put("EX_RATE", rate);//匯率
                ei.Put("LAMT", Blamt);
            }
            else
                ei.Put("EX_RATE", 0);//匯率
            return ei;
        }
        private string GetSysChgDescp(string Chg, string shipmentid)
        {
            string ChgDescp=string.Empty;
            string sql = string.Format(@"SELECT A.CHG_DESCP,A.* FROM  SMCHG A WHERE A.CHG_CD='{0}' AND
 EXISTS(SELECT CMP,TRAN_TYPE FROM SMSM B WHERE SHIPMENT_ID='{1}' 
 AND A.TRAN_MODE=B.TRAN_TYPE AND A.CMP=B.CMP)
", Chg, shipmentid);
            
            return ChgDescp;
        }
        private static DataTable GetRate(DataTable rateDt, DateTime billDate)
        {
            //DateTime billDate = Prolink.Math.GetValueAsDateTime(billDateStr);
            string rateFilter = string.Format("EDATE<={0}", SQLUtils.QuotedStr(billDate.ToString("yyyy-MM-dd")));
            rateDt.Select(rateFilter);
            DataTable rateDt1 = rateDt.Clone();
            foreach (DataRow r in rateDt1.Rows)
            {
                rateDt1.ImportRow(r);
            }
            return rateDt1;
        }
        private static void ChkAct(decimal Bamt, decimal qamt, EditInstruct ei = null)
        {
            string CheckDescp = "";
            string Status = "Y";
            if (qamt > Bamt)
            {
                CheckDescp = "请款金额小于报价金额";
                Status = "N";
            }
            else if (qamt < Bamt)
            {
                CheckDescp = "请款金额大于报价金额";
                Status = "N";
            }
            if (ei != null)
            {
                ei.Put("STATUS", Status);
                ei.Put("CHECK_DESCP", CheckDescp);
            }
        }
        DataTable CreateBillingEI(BillingInfo obj, BillingDetail detail, DataTable billDT)
        {
            DataTable dt=new DataTable();
            string[] head = { "LOCATION", "Shipment ID", "DN", "柜号", "关单号", "MBL NO", "House NO", "BOOKING提单号", "FC财务客户别", "DLV TERM", "POD", "POL", "TPV体积", "总体积", "开航期ETD", "船名航次", "业务窗口", "订舱窗口", "备注", "DEBIT DATE", "DEBIT NO", "Currency", "OF-Ocean Freight", "23" };
                foreach(string str in head)
                {
                    dt.Columns.Add(str);
                }
            Func<DataRow> checkHas = () =>
            {
                if (billDT == null || billDT.Rows.Count <= 0) return null;
                DataRow[] rows = billDT.Select(string.Format("DEBIT_NO={0} AND CHG_CD={1} AND SHIPMENT_ID={2}", SQLUtils.QuotedStr(detail.DebitNO),
                    SQLUtils.QuotedStr(detail.ChargeCode), SQLUtils.QuotedStr(detail.ShipmentID)));
                if (rows == null || rows.Length <= 0) return null;
                return rows[0];
            };
            DataRow br = checkHas();

            EditInstruct ei = null;
            DataRow dr=dt.NewRow();

            dr["LOCATION"] = "";
            dr["Shipment ID"] = detail.ShipmentID;
            dr["DN"] = ""; 
            dr["柜号"] = ""; 
            dr["关单号"] = ""; 
            dr["MBL NO"] = ""; 
            dr["House NO"] = ""; 
            dr["BOOKING提单号"] = "";
            dr["FC财务客户别"] = ""; 
            dr["DLV TERM"] = "";
            dr["POD"] = "";
            dr["POL"] = ""; 
            dr["TPV体积"] = ""; 
            dr["总体积"] = ""; 
            dr["开航期ETD"] = "";
            dr["船名航次"] = ""; 
            dr["业务窗口"] = "";
            dr["订舱窗口"] = ""; 
            dr["备注"] = "";
            if (detail.DebitDate.HasValue)
                dr["DEBIT DATE"] = detail.DebitDate.Value;
            else
                dr["DEBIT DATE"] = "";
            dr["DEBIT NO"] = detail.DebitNO;
            dr["Currency"] = detail.Currency; 
            dr["OF-Ocean Freight"] = "";
            dr["23"] = "";
            if (br != null)
            {
                ei = new EditInstruct("SMBID", EditInstruct.UPDATE_OPERATION);
                ei.Put("MODIFY_BY", obj.Sender);
                ei.PutExpress("MODIFY_DATE", "getdate()");
                ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(br["U_ID"])));
            }
            else
            {
                ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("CREATE_BY", obj.Sender);
                ei.PutExpress("CREATE_DATE", "getdate()");
            }
            ei.Put("VAT_NO", detail.VatNO);
            ei.Put("LSP_NO", obj.Sender);
            ei.Put("BL_NO", detail.BLNO);
            ei.Put("CHG_CD", detail.ChargeCode);
            ei.Put("CHG_DESCP", detail.ChargeDescp);
            ei.Put("BAMT", Prolink.Math.GetValueAsDouble(detail.Amount));
            ei.Put("CUR", detail.Currency);
            ei.Put("CHG_UNIT", detail.ChargeUnit);
            if (detail.DebitDate.HasValue)
                ei.PutDate("DEBIT_DATE", detail.DebitDate.Value);
            ei.Put("QTY", detail.QTY);
            ei.Put("QTYU", detail.QTYUnit);
            ei.Put("UNIT_PRICE", detail.UnitPrice);
            ei.Put("REMARK", detail.Remark);
            ei.Put("TAX", Prolink.Math.GetValueAsDouble(detail.Tax));
            return new DataTable();
        }
        
        protected override ResultInfo HandlerCancel(BillingInfo info)
        {
            throw new NotImplementedException();
        }

        protected DataTable CreateTableForBillImport()
        {
            DataTable dt = new DataTable();
            string[] column ={"LOCATION","Shipment ID","DN","柜号","关单号","MBL NO","House NO",
                              "BOOKING提单号","FC财务客户别","DLV TERM","POD","POL","CARRIER","20GP",
                              "40GP","HQ","TPV体积","总体积","开航期ETD","船名航次","业务窗口","订舱窗口",
                              "备注","DEBIT DATE","DEBIT NO"};//,"OF-Ocean Freight(USD)"
            foreach (string str in column)
            {
                dt.Columns.Add(str, typeof(object));
            }
            return dt;
        }

        protected override EditInstructList ToEi(BillingInfo obj)
        {
            DataTable billDT = QueryBillDT_D(obj);
            DataTable smDT = null;
            if (obj != null && obj.BillingDetails != null && obj.BillingDetails.Length > 0)
            {
                smDT = Helper.QuerySM(obj.BillingDetails.Select(d => d.ShipmentID));
            }
            EditInstructList eiList = new EditInstructList();
            List<string> chgcds = new List<string>();
            string LspNo = Prolink.Math.GetValueAsString(obj.Sender);
            string _chgcds=string.Empty;
            //get chgcd
            foreach (var v in obj.BillingDetails)
            {
                string fid = null;
                string chgcd = Prolink.Math.GetValueAsString(v.ChargeCode);
                if (!chgcds.Contains(chgcd))
                {
                    chgcds.Add(chgcd);
                    int count = OperationUtils.GetValueAsInt(string.Format("SELECT COUNT(CHG_CD) FROM SMCHG where CHG_CD = {0}", SQLUtils.QuotedStr(chgcd)), Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (count <= 0) throw new Exception("没有代码：" + chgcd + "，请联系相关部门。");
                }
            }
            string val = "";
            Func<List<BillingDetail>> getBillingDetail = () =>
                {
                    List<BillingDetail> li = new List<BillingDetail>();
                    foreach (var v in obj.BillingDetails)
                    {
                        string chg = Prolink.Math.GetValueAsString(v.ChargeCode);
                        if (chg.Equals(val))
                        {
                            li.Add(v);
                        }
                    }
                    return li;
                };

            foreach (string str in chgcds)
            {
                if (string.IsNullOrEmpty(str)) continue;
                val = str;
                List<BillingDetail> li = getBillingDetail();
                DataTable dt = null;
                MixedList ml = new MixedList();
                string amountname = string.Empty;
                foreach (var v in li)
                {

                    if (!string.IsNullOrEmpty(amountname))
                    {
                        string chg = Prolink.Math.GetValueAsString(v.ChargeCode);
                        string chgDescp = Prolink.Math.GetValueAsString(v.ChargeDescp);
                        string cur = Prolink.Math.GetValueAsString(v.Currency);
                        string _amountname = string.Format("{0}-{1}({2})", chg, chgDescp, cur);
                        if (!amountname.Equals(_amountname))
                        {
                            string[] columns = { _amountname };
                            AddColumn(dt, columns);
                            amountname = _amountname;
                        }
                    }
                    if (dt == null)
                    {
                        dt = CreateTableForBillImport();
                        string chg = Prolink.Math.GetValueAsString(v.ChargeCode);
                        string chgDescp = Prolink.Math.GetValueAsString(v.ChargeDescp);
                        string cur = Prolink.Math.GetValueAsString(v.Currency);
                        List<string> col = new List<string>();
                        col.Add(chg); col.Add(chgDescp); col.Add(cur);
                        amountname = string.Format("{0}-{1}({2})", chg, chgDescp, cur);
                        //dt.Columns.Add(amountname, typeof(object));//OF-Ocean Freight(USD)
                        string[] columns = { "UNIT_PRICE", "CHG_UNIT", "QTY", "QTYU", "VAT_NO", "TAX", "BL_NO", amountname };
                        AddColumn(dt, columns);
                    }
                    FillTable(ref dt, v, amountname, smDT);
                }
                try
                {
                    string msg = ImportDatatableToBill(dt, LspNo, ml, "Y");
                    if (msg != "")
                    {
                        throw new Exception(msg);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                if (ml.Count > 0)
                {
                    for (int i = 0; i < ml.Count; i++)
                    {
                        eiList.MergeEditInstruct((EditInstruct)ml[i]);
                    }
                }
            }
            return eiList;
        }
        protected DataTable AddColumn (DataTable dt ,string [] columns)
        {
            foreach (string col in columns)
            {
                dt.Columns.Add(col, typeof(string));
            }
            return dt;
        }

        protected void FillTable(ref DataTable dt, BillingDetail obj, string amountname,DataTable smDT)
        {
            bool dod=false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string shipmentid = Prolink.Math.GetValueAsString(dt.Rows[i]["Shipment ID"]);
                string BL_NO = Prolink.Math.GetValueAsString(dt.Rows[i]["BL_NO"]);
                string debitno = Prolink.Math.GetValueAsString(dt.Rows[i]["DEBIT NO"]);
                if (!string.IsNullOrEmpty(shipmentid) && !string.IsNullOrEmpty(BL_NO) && !string.IsNullOrEmpty(debitno))
                {
                    if (shipmentid.Equals(obj.ShipmentID) && BL_NO.Equals(obj.BLNO) && debitno.Equals(obj.DebitNO))
                    {
                        dt.Rows[i][amountname] = Prolink.Math.GetValueAsDouble(dt.Rows[i][amountname]) + Prolink.Math.GetValueAsDouble(obj.Amount);
                        dt.Rows[i]["UNIT_PRICE"] = Prolink.Math.GetValueAsDouble(dt.Rows[i]["UNIT_PRICE"]) + Prolink.Math.GetValueAsDouble(obj.UnitPrice);
                        dod = true;
                    }
                }
            }
            if (!dod)
            {
                string[] column ={"LOCATION","Shipment ID","DN","柜号","关单号","MBL NO","House NO",
                              "BOOKING提单号","FC财务客户别","DLV TERM","POD","POL","CARRIER","20GP",
                              "40GP","HQ","TPV体积","总体积","开航期ETD","船名航次","业务窗口","订舱窗口",
                              "备注","DEBIT DATE","DEBIT NO"};

                DataRow dr = dt.NewRow();
                dr["Shipment ID"] = obj.ShipmentID;
                dr["BL_NO"] = obj.BLNO;
                dr["DEBIT NO"] = obj.DebitNO;
                dr["DEBIT DATE"] = obj.DebitDate;
                dr["UNIT_PRICE"] = obj.UnitPrice;
                dr["CHG_UNIT"] = obj.ChargeUnit;
                dr["QTY"] = obj.QTY;
                dr["QTYU"] = obj.QTYUnit;
                dr[amountname] = obj.Amount;
                dr["VAT_NO"] = obj.VatNO;
                dr["TAX"] = obj.Tax;
                dr["备注"] = obj.Remark;
                var nu = smDT.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(obj.ShipmentID)));
                if (nu != null && nu.Count() > 0)
                {
                    dr["LOCATION"] = nu[0]["CMP"];
                }
                dt.Rows.Add(dr);
            }
            //dt.ImportRow(dr);
        }
        protected void CreateBillingEI(BillingInfo obj, BillingDetail detail, string fid)
        {
            
        }

        public string ImportDatatableToBill(DataTable dt, string LspNo, MixedList ml, string type = "", string userId = "", List<string> checkList=null)
        {
            string msg = string.Empty;
            //string sql = "SELECT CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string sql = "SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());


            int i = 0;
            #region 表头参数映射
            string[] fileds = new string[] { "LOCATION", "Shipment ID", "DEBIT DATE", "DEBIT NO", "CURRENCY", "机种/料号", "DN NO", "标准装柜量" };
            string[] filedNames = new string[] { "CMP", "SHIPMENT_ID", "DEBIT_DATE", "DEBIT_NO", "CUR", "IPART_NO", "DN_NO", "CNTR_STD_QTY" };
            Dictionary<string, string> map = new Dictionary<string, string>();
            Dictionary<string, string> map1 = new Dictionary<string, string>();
            for (i = 0; i < fileds.Length; i++)
            {
                string key = fileds[i].Replace(" ", "").Replace(System.Environment.NewLine, "").Trim().ToUpper();
                map[key] = filedNames[i];
            }

            foreach (DataColumn col in dt.Columns)
            {
                string name = col.ColumnName;
                string key = name.Replace(" ", "").Replace(System.Environment.NewLine, "").Trim().ToUpper();
                if (map.ContainsKey(key))
                    map1[map[key]] = name;
            }
            #endregion

            List<string> smList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                string ShipmentId = map1.ContainsKey("SHIPMENT_ID") ? Prolink.Math.GetValueAsString(dr[map1["SHIPMENT_ID"]]) : string.Empty;
                if (string.IsNullOrEmpty(ShipmentId) || smList.Contains(ShipmentId))
                    continue;
                smList.Add(ShipmentId);
            }
            string condition = smList.Count > 0 ? string.Format("SHIPMENT_ID IN ({0})", string.Join(",", smList.Select(str => SQLUtils.QuotedStr(str)))) : "1=0";
            DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE {0} AND LSP_NO={1} ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", condition, SQLUtils.QuotedStr(LspNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSM WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable smptDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,PARTY_NO,PARTY_NAME,PARTY_TYPE FROM SMSMPT WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable rateDt = OperationUtils.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable qtTempDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,LSP_NO,QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT,DEBIT_NM,DEBIT_TO FROM SMBID_TEMP WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());


            DataTable chgDt = OperationUtils.GetDataTable("SELECT VAT_RATE,CHG_CD,CHG_TYPE,TRAN_MODE FROM SMCHG", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            i = 0;

            List<string> testList = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                #region 初始化参数
                string cmp = map1.ContainsKey("CMP") ? Prolink.Math.GetValueAsString(dr[map1["CMP"]]) : string.Empty;
                string ShipmentId = map1.ContainsKey("SHIPMENT_ID") ? Prolink.Math.GetValueAsString(dr[map1["SHIPMENT_ID"]]) : string.Empty;
                //string CombineInfo = map1.ContainsKey("CMP") ? Prolink.Math.GetValueAsString(dr[map1["CMP"]]) : string.Empty;
                //string BlNo = map1.ContainsKey("CMP") ? Prolink.Math.GetValueAsString(dr[map1["CMP"]]) : string.Empty;
                DateTime DebitDate = map1.ContainsKey("DEBIT_DATE") ? Prolink.Math.GetValueAsDateTime(dr[map1["DEBIT_DATE"]]) : DateTime.Now;
                string DebitNo = map1.ContainsKey("DEBIT_NO") ? Prolink.Math.GetValueAsString(dr[map1["DEBIT_NO"]]) : string.Empty;
                string cur = map1.ContainsKey("CUR") ? Prolink.Math.GetValueAsString(dr[map1["CUR"]]) : string.Empty;
                string dn_no = map1.ContainsKey("DN_NO") ? Prolink.Math.GetValueAsString(dr[map1["DN_NO"]]) : string.Empty;
                string ipart_no = map1.ContainsKey("IPART_NO") ? Prolink.Math.GetValueAsString(dr[map1["IPART_NO"]]) : string.Empty;
                decimal cntr_std_qty = map1.ContainsKey("CNTR_STD_QTY") ? Prolink.Math.GetValueAsDecimal(dr[map1["CNTR_STD_QTY"]]) : 0M;
                #endregion

                #region 检查必填栏位Shipment ID, Debit Date
                if (string.IsNullOrEmpty(ShipmentId))
                {
                    //string localMsg = "第{0}行：Shipment ID不能为空！";
                    //localMsg = string.Format(localMsg, i);
                    //msg += localMsg;
                    continue;
                }

                if (DebitDate == null)
                {
                    string localMsg = "第{0}行：帐单日期不能为空！";
                    localMsg = string.Format(localMsg, i);
                    msg += localMsg;
                    continue;
                }

                if (cmp == "")
                {
                    string localMsg = "第{0}行：Location不能为空！";
                    localMsg = string.Format(localMsg, i);
                    msg += localMsg;
                    continue;
                }

                //if (string.IsNullOrEmpty(cur))
                //{
                //    string localMsg = "第{0}行：Currency不能为空！";
                //    localMsg = string.Format(localMsg, i);
                //    msg += localMsg;
                //    continue;
                //}
                #endregion

                DataRow[] sms = smDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                if (sms.Length <= 0)
                    continue;
                cmp = Prolink.Math.GetValueAsString(sms[0]["CMP"]);

                string bl_win = Prolink.Math.GetValueAsString(sms[0]["BL_WIN"]).Trim();
                if (!string.IsNullOrEmpty(bl_win) && bl_win.LastIndexOf(' ') > 0)
                    bl_win = bl_win.Substring(0, bl_win.LastIndexOf(' '));

                //2016/8/18新增
                string tran_type = Prolink.Math.GetValueAsString(sms[0]["TRAN_TYPE"]);
                string group_id = Prolink.Math.GetValueAsString(sms[0]["GROUP_ID"]);
                string BlNo = Prolink.Math.GetValueAsString(sms[0]["HOUSE_NO"]);

                DebitDate=Business.TPV.Financial.Bill.GetBillDate(DebitDate, sms[0], true);
                DataTable rateDt1 = GetRate(rateDt, DebitDate);
                DebitDate = Business.TPV.Financial.Bill.GetBillDate(DebitDate, sms[0], false);
                bool error = false;

                DataRow[] partySH = Business.TPV.Financial.Helper.GetParty(ShipmentId, smptDt, new string[] { "SH" });
                foreach (DataColumn col in dt.Columns)
                {
                    string[] ts = null;
                    string name = col.ColumnName;
                    if ("金额".Equals(name))
                    {
                        ts = new string[] { "", "" };
                        if (dr.Table.Columns.Contains("费用代码"))
                            ts[0] = Prolink.Math.GetValueAsString(dr["费用代码"]);
                        else
                            throw new Exception("导入失败!缺少费用代码");
                        // return "导入失败!缺少费用代码";
                        //return Json(new { message = "导入失败!缺少费用代码", excelMsg = "导入失败!缺少费用代码" });

                        if (dr.Table.Columns.Contains("费用描述"))
                            ts[1] = Prolink.Math.GetValueAsString(dr["费用描述"]);
                        else
                            throw new Exception("导入失败!缺少费用描述");
                        //return Json(new { message = "导入失败!缺少费用描述", excelMsg = "导入失败!缺少费用描述" });
                    }
                    else
                    {
                        if (!name.Contains("-"))
                            continue;
                        ts = name.Split(new string[] { "-" }, StringSplitOptions.None);
                        if (ts.Length < 2)
                            continue;
                    }

                    if (ts != null && ts.Length > 1)
                    {
                        Regex regex = new Regex("[(](?<cur>[\\s\\S]*?)[)]", RegexOptions.IgnoreCase);
                        MatchCollection MS = regex.Matches(ts[1]);
                        if (MS.Count > 0)
                            cur = MS[MS.Count - 1].Groups["cur"].Value;
                        int index = ts[1].LastIndexOf('(');
                        if (index > 0)
                            ts[1] = ts[1].Substring(0, index);
                    }

                    if (string.IsNullOrEmpty(cur))
                    {
                        string localMsg = "第{0}行：Currency不能为空！";
                        localMsg = string.Format(localMsg, i);
                        msg += localMsg;
                        continue;
                    }

                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                    ei.Put("SHIPMENT_ID", ShipmentId);

                    #region 设置订舱者
                    ei.Put("BOOKING_BY", bl_win);
                    #endregion

                    //2016/8/18新增
                    ei.Put("CMP", cmp);
                    ei.Put("TRAN_TYPE", tran_type);

                    ei.PutDate("DEBIT_DATE", DebitDate);
                    ei.Put("DEBIT_NO", DebitNo);

                    decimal Bamt = Prolink.Math.GetValueAsDecimal(dr[name]);
                    decimal Blamt = 0M;
                    decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt1, null, Bamt, cur, ref Blamt, ref error, "CNY");
                    if (Bamt == 0)
                        continue;
                    ei.Put("LAMT", Blamt);
                    ei.Put("EX_RATE", rate);
                    string ChgCd = ts[0];
                    DataRow[] chgs = chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)));
                    #region 檢查費用代碼是否存在e-shipping
                    if (chgs.Length <= 0)
                    {
                        msg += "费用代码不存在！\n";
                        continue;
                    }
                    #endregion

                    ei.Put("BAMT", Bamt);
                    ei.Put("CUR", cur);
                    //ei.Put("QCUR", cur);
                    string uid = string.Empty;

                    //string filter = string.Format("SHIPMENT_ID={0} AND CHG_CD={1}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd));
                    //string filter = string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND QCUR={2} AND U_FID IS NULL", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(cur));

                    //string filter1 = filter;
                    //DataRow[] drs = null;
                    //if (!string.IsNullOrEmpty(ipart_no))
                    //{
                    //    filter1 += cntr_std_qty > 0 ? string.Format(" AND IPART_NO={0}", SQLUtils.QuotedStr(ipart_no)) : string.Format(" AND IPART_NO={0}", SQLUtils.QuotedStr(dn_no));
                    //    drs = smbidDt.Select(filter1);
                    //}

                    DataRow[] drs = GetBillDataRows(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, "QCUR", " AND U_FID IS NULL");
                    if (drs == null || drs.Length <= 0)
                        drs = GetBillDataRows(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, "CUR", " AND U_FID IS NULL");

                    CheckBill(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, checkList);
                    string chg_type = string.Empty;

                    string qcur = string.Empty;
                    decimal qamt = 0m;
                    decimal qlamt = 0m;
                    foreach (DataRow obi in drs)
                    {
                        if (Prolink.Math.GetValueAsInt(obi["EX_UPDATE"]) != 0)
                            continue;
                        chg_type = Prolink.Math.GetValueAsString(obi["CHG_TYPE"]);
                        obi["EX_UPDATE"] = 1;
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        uid = Prolink.Math.GetValueAsString(obi["U_ID"]);
                        ei.PutKey("U_ID", uid);
                        if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(obi["QUOT_NO"])))
                            ei.Put("QT_DATA", "Y");
                        qcur = Prolink.Math.GetValueAsString(obi["QCUR"]);
                        qamt = Prolink.Math.GetValueAsDecimal(obi["QAMT"]);
                        qlamt = Prolink.Math.GetValueAsDecimal(obi["QLAMT"]);
                        ei.Put("QCUR", qcur);

                        if (qamt == 0 && qlamt == 0 && 
                            Prolink.Math.GetValueAsString(obi["REMARK"]).Contains("预提报价")&& Prolink.Math.GetValueAsString(obi["REMARK"]).Contains("预提CNY报价"))
                            SetSMBIDTemp(LspNo, qtTempDt, ShipmentId, cur, ei, ChgCd, ref qcur, ref qamt, ref qlamt);
                        break;
                    }

                    if (string.IsNullOrEmpty(uid))
                    {
                        uid = System.Guid.NewGuid().ToString();
                        ei.Put("U_ID", uid);
                        ei.OperationType = EditInstruct.INSERT_OPERATION;
                        ei.Put("LSP_NO", LspNo);
                        ei.Put("LSP_NM", LspNm);
                        ei.Put("CHG_CD", ts[0]);
                        ei.Put("CHG_DESCP", ts[1]);
                        ei.Put("QT_DATA", "");
                        ei.Put("BL_NO", BlNo);
                        chg_type = Prolink.Math.GetValueAsString(chgs[0]["CHG_TYPE"]);
                        ei.Put("CHG_TYPE", chg_type);


                        //SetChargeInfo(chgDt, tran_type, ei, ChgCd);
                      
                        #region 设置debit to
                        string my_debit_no = string.Empty;
                        string my_debit_nm = string.Empty;
                        switch (chg_type)
                        {
                            case "F":
                            case "D":
                                my_debit_no = Prolink.Math.GetValueAsString(sms[0]["DEBIT_TO"]);
                                my_debit_nm = Prolink.Math.GetValueAsString(sms[0]["DEBIT_NM"]);
                                break;
                        }

                        if (string.IsNullOrEmpty(my_debit_no) && partySH != null && partySH.Length > 0)
                        {
                            my_debit_no = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NO"]);
                            my_debit_nm = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NAME"]);
                        }
                        #endregion

                        ei.Put("DEBIT_TO", my_debit_no);
                        ei.Put("DEBIT_NM", my_debit_nm);
                        //DataRow[] temps = qtTempDt.Select(string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND LSP_NO={2}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(LspNo)));
                        SetSMBIDTemp(LspNo, qtTempDt, ShipmentId, cur, ei, ChgCd, ref qcur, ref qamt, ref qlamt);
                        if (!string.IsNullOrEmpty(group_id))
                            ei.Put("CMP", cmp);
                    }

                    if (!string.IsNullOrEmpty(group_id))
                        ei.Put("GROUP_ID", group_id);

                    if (cur.Equals(qcur))
                        ChkAct(uid, Bamt, qamt, ei);
                    else
                        ChkAct(uid, Blamt, qlamt, ei);
                    ei.Put("FSTATUS", "C");
                    testList.Add(ei.Get("U_ID"));
                    SetChargeInfo(chgDt, tran_type, ei, ChgCd);
                    #region edi 特殊处理
                    if (type.Equals("Y"))
                    {
                        //UNIT_PRICE", "CHG_UNIT", "QTY", "QTYU", "VAT_NO", "TAX"
                        ei.Put("UNIT_PRICE", dr["UNIT_PRICE"]);
                        ei.Put("CHG_UNIT", dr["CHG_UNIT"]);
                        ei.Put("QTY", dr["QTY"]);
                        ei.Put("QTYU", dr["QTYU"]);
                        ei.Put("VAT_NO", dr["VAT_NO"]);
                        ei.Put("TAX", dr["TAX"]);
                        ei.Put("BOOKING_BY", sms[0]["BL_WIN"]);
                        ei.Put("BL_NO", dr["BL_NO"]);
                        if (ei.OperationType.Equals(EditInstruct.INSERT_OPERATION))
                        {
                            ei.Put("CREATE_BY", LspNo);
                            ei.PutExpress("CREATE_DATE", "getdate()");
                        }
                        else
                        {
                            ei.Put("MODIFY_BY", LspNo);
                            ei.PutExpress("MODIFY_DATE", "getdate()");
                        }
                    }
                    #endregion

                    #region 登入用户的操作
                    if (!string.IsNullOrEmpty(userId))
                    {
                        ei.Put("UPLOAD_USER", userId);
                        DateTime odt = DateTime.Now;
                        
                        string CompanyId = cmp;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        ei.PutDate("UPLOAD_TIME", odt);
                        ei.PutDate("UPLOAD_TIME_L", ndt);
                        if (ei.OperationType.Equals(EditInstruct.INSERT_OPERATION))
                        {
                            ei.Put("CREATE_BY", userId);
                            ei.PutExpress("CREATE_DATE", "getdate()");
                        }
                        else
                        {
                            ei.Put("MODIFY_BY", userId);
                            ei.PutExpress("MODIFY_DATE", "getdate()");
                        }
                    }
                    #endregion
                    Financial.InboundBill.SetEamt(ei);
                    ml.Add(ei);
                    TrackingEDI.Business.CostStatistics.SetCStask(ShipmentId, group_id, cmp, userId, ml);
                }
            }
            return msg;
        }

        public string ImportDatatableToBillForIb(DataTable dt, string LspNo, MixedList ml, string type = "", string userId = "", List<string> checkList = null)
        {
            string msg = string.Empty;
            //string sql = "SELECT CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string sql = "SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(LspNo);
            string LspNm = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());


            int i = 0;
            #region 表头参数映射
            string[] fileds = new string[] { "Shipment ID", "DN INFO", "LSP No.", "ATD", "Container No", "MAWB", "HAWB", "FC Customer", "POL", "POD", "Dest.", "DLV  Term", "QTY", "Chargeable Weight", "CBM", "KGS", "DEBIT DATE", "DEBIT NO", "LSP Reference" };
            string[] filedNames = new string[] { "SHIPMENT_ID", "COMBINE_INFO", "LSP_NO", "ATD", "CNTR_NO", "MAWB", "HAWB", "PARTY_NAME", "POL_NAME", "POD_NAME", "DEST_NAME", "INCOTERM_CD", "QTY", "CW", "CBM", "GW", "DEBIT_DATE", "DEBIT_NO", "REMARK" };
            Dictionary<string, string> map = new Dictionary<string, string>();
            Dictionary<string, string> map1 = new Dictionary<string, string>();
            for (i = 0; i < fileds.Length; i++)
            {
                string key = fileds[i].Replace(" ", "").Replace(System.Environment.NewLine, "").Trim().ToUpper();
                map[key] = filedNames[i];
            }

            foreach (DataColumn col in dt.Columns)
            {
                string name = col.ColumnName;
                string key = name.Replace(" ", "").Replace(System.Environment.NewLine, "").Trim().ToUpper();
                if (map.ContainsKey(key))
                    map1[map[key]] = name;
            }
            #endregion
              
            List<string> smList = new List<string>();
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            foreach (DataRow dr in dt.Rows)
            { 
                string ShipmentId = map1.ContainsKey("SHIPMENT_ID") ? Prolink.Math.GetValueAsString(dr[map1["SHIPMENT_ID"]]) : string.Empty;
                string CntrNo = map1.ContainsKey("CNTR_NO") ? Prolink.Math.GetValueAsString(dr[map1["CNTR_NO"]]) : string.Empty;
                if (pairs.ContainsKey(ShipmentId))
                {
                    pairs[ShipmentId] += ";" + CntrNo;
                }
                else
                {
                    pairs.Add(ShipmentId, CntrNo);
                }

                if (string.IsNullOrEmpty(ShipmentId) || smList.Contains(ShipmentId))
                    continue;
                smList.Add(ShipmentId); 
            }
            string condition = smList.Count > 0 ? string.Format("SHIPMENT_ID IN ({0})", string.Join(",", smList.Select(str => SQLUtils.QuotedStr(str)))) : "1=0";
            DataTable smbidDt = OperationUtils.GetDataTable(string.Format("SELECT SMBID.*,0 AS EX_UPDATE FROM SMBID WHERE {0} AND LSP_NO={1} ORDER BY SHIPMENT_ID,CHG_CD,DEBIT_NO DESC,IPART_NO DESC", condition, SQLUtils.QuotedStr(LspNo)), null, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable smDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSMI WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable smptDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,PARTY_NO,PARTY_NAME,PARTY_TYPE FROM SMSMIPT WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable rateDt = OperationUtils.GetDataTable("SELECT ETYPE,EDATE,FCUR,TCUR,EX_RATE FROM BSERATE WHERE ETYPE='M' ORDER BY EDATE", new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable qtTempDt = OperationUtils.GetDataTable(string.Format("SELECT SHIPMENT_ID,LSP_NO,QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT,DEBIT_NM,DEBIT_TO FROM SMBID_TEMP WHERE {0}", condition), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            DataTable cntrDt = OperationUtils.GetDataTable(string.Format(@"SELECT CNTR_NO,SHIPMENT_ID FROM SMICNTR WHERE {0}", condition),new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());

            string cntrMsg = string.Empty;
            foreach (string item in pairs.Keys)
            {
                var nonNullValues = cntrDt.AsEnumerable()
                         .Where(row => row.Field<string>("SHIPMENT_ID") == item && row.Field<string>("CNTR_NO") != null)
                         .Select(row => row.Field<string>("CNTR_NO"));
                string[] str = pairs[item].Split(new char[] { ';'}, StringSplitOptions.RemoveEmptyEntries);
                if (str.Length > nonNullValues.Count())
                {
                    cntrMsg += GetlangCaption("L_SMSMI_CntrCountCheck", "柜数超过Shipment的总柜数。") + "\r\n";
                }

                foreach (var cntrNo in str)
                {
                    if (!nonNullValues.Contains(cntrNo))
                    {
                        cntrMsg += cntrNo + GetlangCaption("L_SMSMI_CntrNoCheck", "：柜号在这笔Shipment中不存在。") + "\r\n";
                    }
                } 
            }
            if (!string.IsNullOrEmpty(cntrMsg))
            {
                throw new Exception(cntrMsg);
            }
             
            string Location = string.Empty;
            string TranType = string.Empty;
            if(smDt.Rows.Count > 0)
            {
                Location = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                TranType = Prolink.Math.GetValueAsString(smDt.Rows[0]["TRAN_TYPE"]);
            }

            DataTable chgDt = OperationUtils.GetDataTable("SELECT VAT_RATE,CHG_CD,CHG_TYPE,TRAN_MODE,REPAY FROM SMCHG WHERE CMP=" + SQLUtils.QuotedStr(Location), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            i = 0;

            sql = string.Format(@"SELECT DISTINCT DLV_AREA FROM SMIDN WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE CMP = {1} AND SMBID.LSP_NO={0})", SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location));
            DataTable smidnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> DndlvAreaList = new List<string>() { };
            foreach (DataRow dr in smidnDt.Rows)
            {
                string dlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                if (!string.IsNullOrEmpty(dlvArea))
                    DndlvAreaList.Add(dlvArea);
            }

            sql = string.Format(@"SELECT DISTINCT DLV_AREA FROM SMICNTR WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMBID WHERE CMP = {1} AND SMBID.LSP_NO={0})", SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(Location));
            DataTable smicntrDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            List<string> TrdlvAreaList = new List<string>() { };
            foreach (DataRow dr in smicntrDt.Rows)
            {
                string dlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                if (!string.IsNullOrEmpty(dlvArea))
                    TrdlvAreaList.Add(dlvArea);
            }
            string dnPodCondtion = "";
            string trPodCondtion = "";
            if (DndlvAreaList.Count > 0)
            {
                dnPodCondtion = string.Format(@" OR POD_CD IN {0}", SQLUtils.Quoted(DndlvAreaList.ToArray()));
            }
            if (TrdlvAreaList.Count > 0)
            {
                trPodCondtion = string.Format(@" OR POD_CD IN {0}", SQLUtils.Quoted(TrdlvAreaList.ToArray()));
            }

            sql = string.Format(@"SELECT CHG_CD,QUOT_NO,CUR FROM (SELECT U_FID,QUOT_NO,CHG_CD,ISNULL(CUR,(SELECT TOP 1 CUR FROM SMQTM WHERE SMQTM.U_ID =SMQTD.U_FID)) CUR FROM SMQTD 
                WHERE EXISTS (SELECT RFQ_NO FROM SMQTM WITH (NOLOCK) WHERE SMQTM.QUOT_NO=SMQTD.QUOT_NO AND SMQTM.QUOT_TYPE='A' AND SMQTM.TRAN_MODE ='C' AND SMQTM.LSP_CD={0} AND SMQTM.TRAN_TYPE={1}) 
                AND ((POD_CD IS NULL {2}) OR 
                (POD_CD IS NULL {3})))A 
                OUTER APPLY (SELECT TOP 1 EFFECT_FROM AS QT_EFFECT_FROM FROM SMQTM WITH (NOLOCK) WHERE SMQTM.U_ID = A.U_FID) B ORDER BY QT_EFFECT_FROM DESC,QUOT_NO",
                SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(TranType), dnPodCondtion, trPodCondtion);
            DataTable chgcds = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            chgcds = GetNullCur(chgcds, string.Format(" AND CMP={0}", SQLUtils.QuotedStr(Location)));

            List<string> testList = new List<string>();

            Dictionary<string, string> curMap = new Dictionary<string, string>();
            Dictionary<string, PortFeeDate> portFeeDateDic = new Dictionary<string, PortFeeDate>();
            foreach (DataRow dr in dt.Rows)
            {
                #region 初始化参数
                string cmp = map1.ContainsKey("CMP") ? Prolink.Math.GetValueAsString(dr[map1["CMP"]]) : string.Empty;
                cmp = Prolink.Math.GetValueAsString(smDt.Rows[0]["CMP"]);
                string ShipmentId = map1.ContainsKey("SHIPMENT_ID") ? Prolink.Math.GetValueAsString(dr[map1["SHIPMENT_ID"]]) : string.Empty;
                DateTime DebitDate = map1.ContainsKey("DEBIT_DATE") ? Prolink.Math.GetValueAsDateTime(dr[map1["DEBIT_DATE"]]) : DateTime.Now;
                string DebitNo = map1.ContainsKey("DEBIT_NO") ? Prolink.Math.GetValueAsString(dr[map1["DEBIT_NO"]]) : string.Empty;
                string cur = map1.ContainsKey("CUR") ? Prolink.Math.GetValueAsString(dr[map1["CUR"]]) : string.Empty;
                string dn_no = map1.ContainsKey("DN_NO") ? Prolink.Math.GetValueAsString(dr[map1["DN_NO"]]) : string.Empty;
                string ipart_no = map1.ContainsKey("IPART_NO") ? Prolink.Math.GetValueAsString(dr[map1["IPART_NO"]]) : string.Empty;
                decimal cntr_std_qty = map1.ContainsKey("CNTR_STD_QTY") ? Prolink.Math.GetValueAsDecimal(dr[map1["CNTR_STD_QTY"]]) : 0M;
                string remark = map1.ContainsKey("REMARK") ? Prolink.Math.GetValueAsString(dr[map1["REMARK"]]) : string.Empty;
                string cntrno = map1.ContainsKey("CNTR_NO") ? Prolink.Math.GetValueAsString(dr[map1["CNTR_NO"]]) : string.Empty;
                #endregion

                #region 检查必填栏位Shipment ID, Debit Date
                if (string.IsNullOrEmpty(ShipmentId))
                {
                    continue;
                }

                if (DebitDate == null)
                {
                    string localMsg = GetlangCaption("L_ActManageController_Controllers_13", "第{0}行：帐单日期不能为空！");
                    localMsg = string.Format(localMsg, i);
                    msg += localMsg;
                    continue;
                }

                if (cmp == "")
                {
                    string localMsg = GetlangCaption("L_ActManageController_Controllers_14", "第{0}行：Location不能为空！");
                    localMsg = string.Format(localMsg, i);
                    msg += localMsg;
                    continue;
                }
                #endregion

                DataRow[] sms = smDt.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(ShipmentId)));
                if (sms.Length <= 0)
                    continue;
                cmp = Prolink.Math.GetValueAsString(sms[0]["CMP"]);

                string bl_win = Prolink.Math.GetValueAsString(sms[0]["BL_WIN"]).Trim();
                if (!string.IsNullOrEmpty(bl_win) && bl_win.LastIndexOf(' ') > 0)
                    bl_win = bl_win.Substring(0, bl_win.LastIndexOf(' '));

                //2016/8/18新增
                string tran_type = Prolink.Math.GetValueAsString(sms[0]["TRAN_TYPE"]);
                string group_id = Prolink.Math.GetValueAsString(sms[0]["GROUP_ID"]);
                string BlNo = Prolink.Math.GetValueAsString(sms[0]["MASTER_NO"]);

                DebitDate = Business.TPV.Financial.Bill.GetBillDateForIb(DebitDate, sms[0], true);
                DataTable rateDt1 = GetRate(rateDt, DebitDate);
                DebitDate = Business.TPV.Financial.Bill.GetBillDateForIb(DebitDate, sms[0], false);
                bool error = false;

                if (!curMap.ContainsKey(group_id + "#" + cmp))
                    curMap[group_id + "#" + cmp] = GetLocalCur(group_id, cmp);
                string localCur = curMap[group_id + "#" + cmp];

                DataRow[] partySH = Business.TPV.Financial.Helper.GetParty(ShipmentId, smptDt, new string[] { "CS" });
                foreach (DataColumn col in dt.Columns)
                {
                    string[] ts = null;
                    string name = col.ColumnName;
                    if ("金额".Equals(name))
                    {
                        ts = new string[] { "", "" };
                        if (dr.Table.Columns.Contains("费用代码"))
                            ts[0] = Prolink.Math.GetValueAsString(dr["费用代码"]);
                        else
                            throw new Exception(GetlangCaption("L_ActManageController_ChargeCode", "导入失败!缺少费用代码"));
                        // return "导入失败!缺少费用代码";
                        //return Json(new { message = "导入失败!缺少费用代码", excelMsg = "导入失败!缺少费用代码" });

                        if (dr.Table.Columns.Contains("费用描述"))
                            ts[1] = Prolink.Math.GetValueAsString(dr["费用描述"]);
                        else
                            throw new Exception(GetlangCaption("L_ActManageController_ChargeDesc", "导入失败!缺少费用描述"));
                        //return Json(new { message = "导入失败!缺少费用描述", excelMsg = "导入失败!缺少费用描述" });
                    }
                    else
                    {
                        if (!name.Contains("-"))
                            continue;
                        ts = name.Split(new string[] { "-" }, StringSplitOptions.None);
                        if (ts.Length < 2)
                            continue;
                    }

                    if (ts != null && ts.Length > 1)
                    {
                        Regex regex = new Regex("[(](?<cur>[\\s\\S]*?)[)]", RegexOptions.IgnoreCase);
                        MatchCollection MS = regex.Matches(ts[1]);
                        if (MS.Count > 0)
                            cur = MS[MS.Count - 1].Groups["cur"].Value;
                        int index = ts[1].LastIndexOf('(');
                        if (index > 0)
                            ts[1] = ts[1].Substring(0, index);
                    }

                    if (string.IsNullOrEmpty(cur))
                    {
                        string localMsg = GetlangCaption("L_ActManageController_Currency", "第{0}行：Currency不能为空！");
                        localMsg = string.Format(localMsg, i);
                        msg += localMsg;
                        continue;
                    }

                    EditInstruct ei = new EditInstruct("SMBID", EditInstruct.INSERT_OPERATION);
                    ei.Put("SHIPMENT_ID", ShipmentId); 
                    ei.Put("BOOKING_BY", bl_win);//设置订舱者  
                    ei.Put("SEC_CMP", Prolink.Math.GetValueAsString(sms[0]["SEC_CMP"]));
                    ei.Put("CMP", cmp);
                    ei.Put("TRAN_TYPE", tran_type);

                    ei.PutDate("DEBIT_DATE", DebitDate);
                    ei.Put("DEBIT_NO", DebitNo);

                    decimal Bamt = Prolink.Math.GetValueAsDecimal(dr[name]);
                    decimal Blamt = 0M;
                 
                    if (Bamt == 0)
                        continue;

                    string ChgCd = ts[0];
                    DataRow[] chgs = chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)));
                    
                    if (chgs.Length <= 0)
                    {
                        msg += ChgCd + ":" + GetlangCaption("L_ActManageController_ChargeNotExist", "费用代码不存在！") + "\n";
                        continue;
                    } 

                    ei.Put("BAMT", Bamt);
                    ei.Put("CUR", cur);
                    PortFeeDate portFeeDate = new PortFeeDate(ShipmentId, cntrno);
                    string key = ShipmentId + "#" + cntrno;
                    if (!portFeeDateDic.ContainsKey(key))
                        portFeeDateDic.Add(key, portFeeDate);
                    portFeeDate = portFeeDateDic[key];
                    switch (ChgCd)
                    {
                        case "DSTF":
                            portFeeDate.StoAmt = Bamt;
                            portFeeDate.StoCur = cur;
                            portFeeDate.StoUp = DEMtype.update;
                            break;
                        case "DDEM":
                            portFeeDate.DemAmt = Bamt;
                            portFeeDate.DemCur = cur;
                            portFeeDate.DemUp = DEMtype.update;
                            break;
                        case "DDET":
                            portFeeDate.DetAmt = Bamt;
                            portFeeDate.DetCur = cur;
                            portFeeDate.DetUp = DEMtype.update;
                            break;
                    }
                    string uid = string.Empty, ufid = string.Empty;
                    string approveStatus = string.Empty;
                    string Billcondition = " AND U_FID IS NULL";
                    if (string.IsNullOrEmpty(cntrno))
                    {
                        Billcondition += " AND CNTR_NO IS NULL";
                    }
                    else
                        Billcondition += string.Format(" AND DEC_NO ={0}", SQLUtils.QuotedStr(cntrno));

                    DataRow[] drs = GetBillDataRows(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, "QCUR", Billcondition);
                    if (drs == null || drs.Length <= 0)
                        drs = GetBillDataRows(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, "CUR", Billcondition);

                    CheckBill(smbidDt, ShipmentId, ChgCd, cur, ipart_no, dn_no, cntr_std_qty, checkList, string.IsNullOrEmpty(cntrno) ? "" : string.Format(" AND CNTR_NO={0}", SQLUtils.QuotedStr(cntrno)));
                    string chg_type = string.Empty;

                    string qcur = string.Empty;
                    string qlcur = string.Empty;
                    decimal qamt = 0m;
                    decimal qlamt = 0m;
                    foreach (DataRow obi in drs)
                    {
                        if (Prolink.Math.GetValueAsInt(obi["EX_UPDATE"]) != 0)
                            continue;
                        chg_type = Prolink.Math.GetValueAsString(obi["CHG_TYPE"]);
                        obi["EX_UPDATE"] = 1;
                        ei.OperationType = EditInstruct.UPDATE_OPERATION;
                        uid = Prolink.Math.GetValueAsString(obi["U_ID"]);
                        ufid = Prolink.Math.GetValueAsString(obi["U_FID"]);
                        approveStatus = Prolink.Math.GetValueAsString(obi["APPROVE_STATUS"]);
                        ei.PutKey("U_ID", uid);
                        if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(obi["QUOT_NO"])))
                            ei.Put("QT_DATA", "Y");
                        qcur = Prolink.Math.GetValueAsString(obi["QCUR"]);
                        qlcur = Prolink.Math.GetValueAsString(obi["QLCUR"]);
                        qamt = Prolink.Math.GetValueAsDecimal(obi["QAMT"]);
                        qlamt = Prolink.Math.GetValueAsDecimal(obi["QLAMT"]);
                        if (string.IsNullOrEmpty(qlcur) && !string.IsNullOrEmpty(qcur))
                        {
                            qlcur = localCur;
                            ei.Put("QLCUR", qlcur);
                            if (qamt != 0 && qlamt == 0)
                            {
                                qlamt = 0M;
                                decimal lrate = Business.TPV.Financial.Helper.GetTotal(rateDt1, null, qamt, qcur, ref qlamt, ref error, qlcur);
                                ei.Put("QLRATE", lrate);
                                ei.Put("QEX_RATE", lrate);
                                ei.Put("QLAMT", qlamt);
                            }
                        }

                        ei.Put("QCUR", qcur);
                        ei.Put("BI_REMARK", remark);
                        if (qamt == 0 && qlamt == 0 &&
                            Prolink.Math.GetValueAsString(obi["REMARK"]).Contains("预提报价") && Prolink.Math.GetValueAsString(obi["REMARK"]).Contains("预提CNY报价"))
                            SetSMBIDTemp(LspNo, qtTempDt, ShipmentId, cur, ei, ChgCd, ref qcur, ref qamt, ref qlamt);
                        break;
                    }

                    if (string.IsNullOrEmpty(qlcur))
                        qlcur = localCur;

                    decimal rate = Business.TPV.Financial.Helper.GetTotal(rateDt1, null, Bamt, cur, ref Blamt, ref error, qlcur);
                    ei.Put("LAMT", Blamt);
                    ei.Put("EX_RATE", rate);

                    if (!string.IsNullOrEmpty(cntrno))
                    {
                        if (!string.IsNullOrEmpty(ufid))
                        {
                            msg += ChgCd + ":" + GetlangCaption("L_DNManage_RefNlCtEt", "已关联账单的费用不可修改") + "\n";
                            continue;
                        }
                        else if ("Y".Equals(approveStatus))
                        {
                            msg += ChgCd + ":" + GetlangCaption("L_ActUpdate_Scripts_80", "审核通过不可修改") + "\n";
                            continue;
                        } 
                    } 

                    if (string.IsNullOrEmpty(uid))
                    { 
                        uid = System.Guid.NewGuid().ToString();
                        ei.Put("U_ID", uid);
                        ei.OperationType = EditInstruct.INSERT_OPERATION;
                        ei.Put("INVOICE_INFO", Prolink.Math.GetValueAsString(sms[0]["INVOICE_INFO"]));
                        ei.Put("LSP_NO", LspNo);
                        ei.Put("LSP_NM", LspNm);
                        ei.Put("CHG_CD", ts[0]);
                        ei.Put("CHG_DESCP", ts[1]);
                        ei.Put("QT_DATA", "");
                        ei.Put("BL_NO", BlNo);
                        chg_type = Prolink.Math.GetValueAsString(chgs[0]["CHG_TYPE"]);
                        ei.Put("CHG_TYPE", chg_type);
                        ei.Put("REPAY", chgs[0]["REPAY"]);
                        ei.Put("BI_REMARK", remark);
                        SetSMBIDTemp(LspNo, qtTempDt, ShipmentId, cur, ei, ChgCd, ref qcur, ref qamt, ref qlamt);
                        if (!string.IsNullOrEmpty(group_id))
                            ei.Put("CMP", cmp);
                        if (!string.IsNullOrEmpty(cntrno))
                        { 
                            ei.Put("DEC_NO", cntrno);  

                            EditInstruct delEi = new EditInstruct("SMBID", EditInstruct.DELETE_OPERATION);
                            delEi.PutKey("DEC_NO", cntrno);
                            delEi.PutKey("CHG_CD", ts[0]);
                            delEi.PutKey("SHIPMENT_ID", ShipmentId);
                            delEi.PutKey("CMP", cmp);
                            ml.Add(delEi);
                        } 
                    }


                    #region 设置debit to
                    string my_debit_no = string.Empty;
                    string my_debit_nm = string.Empty;

                    if (string.IsNullOrEmpty(my_debit_no) && partySH != null && partySH.Length > 0)
                    {
                        my_debit_no = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NO"]);
                        my_debit_nm = Prolink.Math.GetValueAsString(partySH[0]["PARTY_NAME"]);

                        ei.Put("DEBIT_TO", my_debit_no);
                        ei.Put("DEBIT_NM", my_debit_nm);
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(group_id))
                        ei.Put("GROUP_ID", group_id);

                    if (cur.Equals(qcur))
                        ChkAct(uid, Bamt, qamt, ei);
                    else
                        ChkAct(uid, Blamt, qlamt, ei);
                    ei.Put("FSTATUS", "C");
                    testList.Add(ei.Get("U_ID"));
                    SetChargeInfo(chgDt, tran_type, ei, ChgCd);
                    #region edi 特殊处理
                    if (type.Equals("Y"))
                    { 
                        ei.Put("UNIT_PRICE", dr["UNIT_PRICE"]);
                        ei.Put("CHG_UNIT", dr["CHG_UNIT"]);
                        ei.Put("QTY", dr["QTY"]);
                        ei.Put("QTYU", dr["QTYU"]);
                        ei.Put("VAT_NO", dr["VAT_NO"]);
                        ei.Put("TAX", dr["TAX"]);
                        ei.Put("BOOKING_BY", sms[0]["BL_WIN"]);
                        ei.Put("BL_NO", dr["BL_NO"]);
                        if (ei.OperationType.Equals(EditInstruct.INSERT_OPERATION))
                        {
                            ei.Put("CREATE_BY", LspNo);
                            ei.PutExpress("CREATE_DATE", "getdate()");
                        }
                        else
                        {
                            ei.Put("MODIFY_BY", LspNo);
                            ei.PutExpress("MODIFY_DATE", "getdate()");
                        }
                    }
                    #endregion

                    #region 登入用户的操作
                    if (!string.IsNullOrEmpty(userId))
                    {
                        ei.Put("UPLOAD_USER", userId);
                        DateTime odt = DateTime.Now;                 
                        string CompanyId = cmp;
                        DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                        
                        ei.PutDate("UPLOAD_TIME", odt);
                        ei.PutDate("UPLOAD_TIME_L", ndt);
                        if (ei.OperationType.Equals(EditInstruct.INSERT_OPERATION))
                        {
                            ei.Put("CREATE_BY", userId);
                            ei.PutExpress("CREATE_DATE", "getdate()");
                        }
                        else
                        {
                            ei.Put("MODIFY_BY", userId);
                            ei.PutExpress("MODIFY_DATE", "getdate()");
                        }
                    }
                    #endregion


                    ei.Put("CNTR_INFO", Prolink.Math.GetValueAsString(sms[0]["CNTR_INFO"]));
                    ei.Put("MASTER_NO", Prolink.Math.GetValueAsString(sms[0]["MASTER_NO"]));
                    ei.Put("POD_CD", Prolink.Math.GetValueAsString(sms[0]["POD_CD"]));
                     
                    if ("F".Equals(TranType) || "R".Equals(TranType))
                    {
                        foreach (DataRow Chgdr in chgcds.Rows)
                        {
                            string chgCd = Prolink.Math.GetValueAsString(Chgdr["CHG_CD"]);
                            string Cur = Prolink.Math.GetValueAsString(Chgdr["CUR"]);
                            if (chgCd.Equals(ts[0]) && Cur.Equals(cur))
                            {
                                if (!string.IsNullOrEmpty(cntrno))
                                    ei.Put("CNTR_NO", cntrno); 
                            }
                        }
                    }
                    Financial.InboundBill.SetEamt(ei);
                    ml.Add(ei); 
                    TrackingEDI.Business.CostStatistics.SetCStask(ShipmentId, group_id, cmp, userId, ml);
                }
            }
            if (!string.IsNullOrEmpty(msg))
                throw new Exception(msg);

            foreach (string key in portFeeDateDic.Keys)
                Financial.CalCualteFeeHandle.UpdateACTAmt(portFeeDateDic[key], ml);
            return msg;
        }

        private static void SetSMBIDTemp(string LspNo, DataTable qtTempDt, string ShipmentId, string cur, EditInstruct ei, string ChgCd, ref string qcur, ref decimal qamt, ref decimal qlamt)
        {
            DataRow[] temps = qtTempDt.Select(string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND LSP_NO={2} AND CUR={3}", SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(LspNo), SQLUtils.QuotedStr(cur)));
            if (temps.Length > 0)
            {
                qcur = Prolink.Math.GetValueAsString(temps[0]["CUR"]);
                //qamt = Prolink.Math.GetValueAsDecimal(temps[0]["QAMT"]);
                //qlamt = Prolink.Math.GetValueAsDecimal(temps[0]["QLAMT"]);
                qamt = 0;
                qlamt = 0;
                //QUOT_NO,CHG_CD,CHG_TYPE,CHG_DESCP,CUR,QAMT,QLAMT
                ei.Put("QUOT_NO", temps[0]["QUOT_NO"]);
                ei.Put("CHG_TYPE", temps[0]["CHG_TYPE"]);
                ei.Put("QCUR", qcur);
                ei.Put("QAMT", 0);
                ei.Put("QLAMT", 0);
                ei.Put("DEBIT_NM", temps[0]["DEBIT_NM"]);
                ei.Put("DEBIT_TO", temps[0]["DEBIT_TO"]);
                ei.Put("REMARK", string.Format("预提报价:{0}{1},预提CNY报价:{2}", Prolink.Math.GetValueAsDecimal(temps[0]["QAMT"]), Prolink.Math.GetValueAsString(temps[0]["CUR"]), Prolink.Math.GetValueAsDecimal(temps[0]["QLAMT"])));
            }
        }

        public string GetlangCaption(string id, string defaultCaption)
        {
            string value = string.Empty;
            if (lang != null && lang.ContainsKey(id))
            {
                value = lang[id];
            }
            return string.IsNullOrEmpty(value) ? defaultCaption : value;
        }

        private string ChkAct(string uid, decimal Bamt, decimal qamt, EditInstruct ei = null)
        {
            string CheckDescp = "";
            string Status = "Y";
            string sql = "UPDATE SMBID SET CHECK_DESCP={0}, STATUS={1} WHERE U_ID={2}";
            if (qamt > Bamt)
            {
                CheckDescp = GetlangCaption("L_ActManage_Controllers_69", "请款金额小于报价金额"); //lang["L_ActManage_Controllers_69"];
                Status = "N";
            }
            else if (qamt < Bamt)
            {
                CheckDescp = GetlangCaption("L_ActManage_Controllers_70", "请款金额大于报价金额"); //lang["L_ActManage_Controllers_70"];
                Status = "N";
            }
            if (ei != null)
            {
                ei.Put("STATUS", Status);
                ei.Put("CHECK_DESCP", CheckDescp);
            }
            sql = string.Format(sql, SQLUtils.QuotedStr(CheckDescp), SQLUtils.QuotedStr(Status), SQLUtils.QuotedStr(uid));
            return sql;
        }
        private static void SetChargeInfo(DataTable chgDt, string tran_type, EditInstruct ei, string ChgCd)
        {
            DataRow[] drs = chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr(tran_type)));
            if (drs.Length <= 0)
                drs = chgDt.Select(string.Format("CHG_CD={0} AND TRAN_MODE={1}", SQLUtils.QuotedStr(ChgCd), SQLUtils.QuotedStr("O")));
            if (drs.Length <= 0)
                drs = chgDt.Select(string.Format("CHG_CD={0}", SQLUtils.QuotedStr(ChgCd)));
            if (drs.Length > 0)
            {
                ei.Put("TAX", drs[0]["VAT_RATE"]);
                ei.Put("QTAX", drs[0]["VAT_RATE"]);
                if (string.IsNullOrEmpty(ei.Get("CHG_TYPE")))
                    ei.Put("CHG_TYPE", drs[0]["CHG_TYPE"]);
            }
        }

        public void CheckBill(DataTable smbidDt, string shipmentId, string chgCd, string cur, string ipart_no, string dn_no, decimal cntr_std_qty, List<string> msg, string othfiler = "")
        {
            if (msg == null)
                return;
            DataRow[] drs = GetBillDataRows(smbidDt, shipmentId, chgCd, cur, ipart_no, dn_no, cntr_std_qty, "CUR", othfiler);
            foreach (DataRow dr in drs)
            {
                string debitNo = Prolink.Math.GetValueAsString(dr["DEBIT_NO"]);
                string cntrNo = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                if (!string.IsNullOrEmpty(Prolink.Math.GetValueAsString(dr["U_FID"])))
                {
                    if (msg != null)
                        msg.Add(string.Format("{0},{1}{4}({2})" + GetlangCaption("L_ActManage_Controllers_85", "的请款以关连帐单") + "({3})," + GetlangCaption("L_ActManage_Controllers_87", "不得上传"), shipmentId, chgCd, cur, debitNo, string.IsNullOrEmpty(cntrNo) ? "" : "," + cntrNo));
                }
                if ("Y".Equals(Prolink.Math.GetValueAsString(dr["APPROVE_STATUS"])))
                {
                    if (msg != null)
                        msg.Add(string.Format("{0},{1}{3}({2})" + GetlangCaption("L_ActManage_Controllers_86", "的请款以审核通过") + "," + GetlangCaption("L_ActManage_Controllers_87", "不得上传"), shipmentId, chgCd, cur, string.IsNullOrEmpty(cntrNo) ? "" : "," + cntrNo));
                }
            }
        }

        private static DataRow[] GetBillDataRows(DataTable smbidDt, string shipmentId, string chgCd, string cur, string ipart_no, string dn_no, decimal cntr_std_qty, string curField = "CUR", string othfiler = "")
        {
            string filter = string.Format("SHIPMENT_ID={0} AND CHG_CD={1} AND {2}={3}{4}", SQLUtils.QuotedStr(shipmentId), SQLUtils.QuotedStr(chgCd), curField, SQLUtils.QuotedStr(cur),othfiler);

            string filter1 = filter;
            DataRow[] drs = null;
            if (!string.IsNullOrEmpty(ipart_no))
            {
                filter1 += cntr_std_qty > 0 ? string.Format(" AND IPART_NO={0} AND DN_NO={1}", SQLUtils.QuotedStr(ipart_no), SQLUtils.QuotedStr(dn_no)) : string.Format(" AND IPART_NO={0}", SQLUtils.QuotedStr(dn_no));
                drs = smbidDt.Select(filter1);
            }
            if (drs == null || drs.Length <= 0)
                drs = smbidDt.Select(filter);
            return drs;
        }

        /// <summary>
        /// 获取本地币别
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static string GetLocalCur(string groupId, string cmp)
        {
            string localCur = string.Empty;
            string sql = string.Format("SELECT AR_CD,CMP,CD FROM BSCODE WHERE CD_TYPE='LCUR' AND (CMP IN ('*',{0}) OR AR_CD={0}) AND GROUP_ID={1}", SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(groupId));
            DataTable curDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow[] drs = curDt.Select(string.Format("CMP={0}", SQLUtils.QuotedStr(cmp)));
            if (drs.Length > 0)
                localCur = Prolink.Math.GetValueAsString(drs[0]["CD"]);

            if (string.IsNullOrEmpty(localCur))
            {
                drs = curDt.Select(string.Format("AR_CD={0}", SQLUtils.QuotedStr(cmp)));
                if (drs.Length > 0)
                    localCur = Prolink.Math.GetValueAsString(drs[0]["CD"]);
            }

            if (string.IsNullOrEmpty(localCur))
            {
                drs = curDt.Select(string.Format("CMP={0}", SQLUtils.QuotedStr("*")));
                if (drs.Length > 0)
                    localCur = Prolink.Math.GetValueAsString(drs[0]["CD"]);
            }
            if (string.IsNullOrEmpty(localCur))
                localCur = "CNY";
            return localCur;
        }


        public static DataTable GetNullCur(DataTable dt,string condition)
        {
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
                        smqtiDt = OperationUtils.GetDataTable("SELECT DISTINCT I_TYPE,CUR FROM SMQTI WITH(NOLOCK) WHERE CUR IS NOT NULL " + condition, null, Prolink.Web.WebContext.GetInstance().GetConnection());

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
            return dt;
        }
    }
}