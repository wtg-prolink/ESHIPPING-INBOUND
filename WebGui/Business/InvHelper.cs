using Business.Mail;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using TrackingEDI.Business;
using TrackingEDI.Mail;
using WebGui.Models;
using Prolink.V3;
using Prolink;
using System.Globalization;

namespace Business
{
    public class InvHelper
    {

        #region 复制首段DN
        public static string copyFirstDn2Pkg(string DnNo, string this_uid)
        {
            string msg = "success";
            string sql = "";
            string DnNoCmpRef = "";
            string dn_no = DnNo;
            EditInstruct ei;
            int[] resulst;
            MixedList ml = new MixedList();
            /*进口invoice需找到首段关联DN*/
            for (int k = 0; k < 99; k++)
            {
                sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dn_no);
                DnNoCmpRef = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (DnNoCmpRef == "")
                {
                    break;
                }
                else
                {
                    dn_no = DnNoCmpRef;
                    continue;
                }
            }

            if (dn_no == DnNo)
            {
                return "DN NO: "+DnNo+@Resources.Locale.L_InvHelper_Business_80;
            }
            else
            {
                sql = "DELETE FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(this_uid);
                try
                {
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT SMINP.* FROM SMINP, SMINM WHERE SMINP.U_FID=SMINM.U_ID AND SMINM.INVOICE_TYPE='O' AND SMINM.DN_NO=" + SQLUtils.QuotedStr(dn_no);
                    DataTable inpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ml = new MixedList();
                    if (inpdt.Rows.Count > 0)
                    {
                        int seq = 0;
                        foreach (DataRow item in inpdt.Rows)
                        {
                            ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                            string puid = System.Guid.NewGuid().ToString();
                            ei.Put("U_ID", puid);
                            ei.Put("U_FID", this_uid);
                            ei.Put("INVOICE_TYPE", "O");
                            ei.Put("DN_NO", Prolink.Math.GetValueAsString(item["DN_NO"]));
                            ei.Put("SEQ_NO", seq);
                            ei.Put("CASE_NO", Prolink.Math.GetValueAsString(item["CASE_NO"]));
                            ei.Put("CASE_NUM", Prolink.Math.GetValueAsDecimal(item["CASE_NUM"]));
                            ei.Put("QTY", Prolink.Math.GetValueAsDecimal(item["QTY"]));
                            ei.Put("QTYU", Prolink.Math.GetValueAsString(item["QTYU"]));
                            ei.Put("TTL_QTY", Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]));
                            ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                            ei.Put("PROD_DESCP", Prolink.Math.GetValueAsString(item["PROD_DESCP"]));
                            ei.Put("OPART_NO", Prolink.Math.GetValueAsString(item["OPART_NO"]));
                            ei.Put("NW", Prolink.Math.GetValueAsDecimal(item["NW"]));
                            ei.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                            ei.Put("GW", Prolink.Math.GetValueAsDecimal(item["GW"]));
                            ei.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                            ei.Put("CBM", Prolink.Math.GetValueAsDecimal(item["CBM"]));
                            ei.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                            ei.Put("DIMENSION", Prolink.Math.GetValueAsString(item["DIMENSION"]));
                            ei.Put("REMARK", Prolink.Math.GetValueAsString(item["REMARK"]));
                            ei.Put("IPART_NO", Prolink.Math.GetValueAsString(item["IPART_NO"]));
                            ei.Put("CNTR_NO", Prolink.Math.GetValueAsString(item["CNTR_NO"]));
                            ei.Put("PO_NO", Prolink.Math.GetValueAsString(item["PO_NO"]));
                            ei.Put("PART_NO", Prolink.Math.GetValueAsString(item["PART_NO"]));
                            ei.Put("UNIT_PRICE", Prolink.Math.GetValueAsDecimal(item["UNIT_PRICE"]));
                            ei.Put("AMT", Prolink.Math.GetValueAsDecimal(item["AMT"]));
                            ei.Put("VEN_CD", Prolink.Math.GetValueAsString(item["VEN_CD"]));
                            ei.Put("VEN_NM", Prolink.Math.GetValueAsString(item["VEN_NM"]));
                            ei.Put("VEN_ADDR", Prolink.Math.GetValueAsString(item["VEN_ADDR"]));
                            ei.Put("PLA_NO", Prolink.Math.GetValueAsString(item["PLA_NO"]));
                            ei.Put("PLA_SIZE", Prolink.Math.GetValueAsString(item["PLA_SIZE"]));
                            ei.Put("NWU", Prolink.Math.GetValueAsString(item["NWU"]));
                            ei.Put("GWU", Prolink.Math.GetValueAsString(item["GWU"]));
                            ei.Put("CBMU", Prolink.Math.GetValueAsString(item["CBMU"]));
                            ei.Put("NCM_NO", Prolink.Math.GetValueAsString(item["NCM_NO"]));
                            ei.Put("SHIPPING_MARK", Prolink.Math.GetValueAsString(item["SHIPPING_MARK"]));
                            ei.Put("PLACE", Prolink.Math.GetValueAsString(item["PLACE"]));
                            ei.Put("SMODEL", Prolink.Math.GetValueAsString(item["SMODEL"]));
                            ei.Put("PART_NUM", Prolink.Math.GetValueAsString(item["PART_NUM"]));
                            ei.Put("MODEL", Prolink.Math.GetValueAsString(item["MODEL"]));
                            seq++;
                            ml.Add(ei);

                            if (seq + 1 % 100 == 0)
                            {
                                resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                                ml = new MixedList();
                            }
                        }
                        resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }


            return msg;
        }
        #endregion

        #region 复制Packing
        public static string insertPacking(DataTable inpdt, string UFid, string sm_id)
        {
            string msg = "success";
            EditInstruct ei;
            int[] resulst;
            MixedList ml = new MixedList();
            ml = new MixedList();
            string sql = "SELECT MAX(SEQ_NO) FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(UFid);
            int seq = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (inpdt.Rows.Count > 0)
            {
                foreach (DataRow item in inpdt.Rows)
                {
                    seq = seq + 1;
                    ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.Put("U_FID", UFid);
                    ei.Put("INVOICE_TYPE", "I");
                    ei.Put("SHIPMENT_ID", sm_id);
                    ei.Put("DN_NO", Prolink.Math.GetValueAsString(item["DN_NO"]));
                    ei.Put("INV_NO", Prolink.Math.GetValueAsString(item["INV_NO"]));
                    ei.Put("SEQ_NO", seq);
                    ei.Put("CASE_NO", Prolink.Math.GetValueAsString(item["CASE_NO"]));
                    ei.Put("CASE_NUM", Prolink.Math.GetValueAsDecimal(item["CASE_NUM"]));
                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(item["QTY"]));
                    ei.Put("QTYU", Prolink.Math.GetValueAsString(item["QTYU"]));
                    ei.Put("TTL_QTY", Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]));
                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                    ei.Put("PROD_DESCP", Prolink.Math.GetValueAsString(item["PROD_DESCP"]));
                    ei.Put("OPART_NO", Prolink.Math.GetValueAsString(item["OPART_NO"]));
                    ei.Put("NW", Prolink.Math.GetValueAsDecimal(item["NW"]));
                    ei.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                    ei.Put("GW", Prolink.Math.GetValueAsDecimal(item["GW"]));
                    ei.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                    ei.Put("CBM", Prolink.Math.GetValueAsDecimal(item["CBM"]));
                    ei.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                    ei.Put("DIMENSION", Prolink.Math.GetValueAsString(item["DIMENSION"]));
                    ei.Put("REMARK", Prolink.Math.GetValueAsString(item["REMARK"]));
                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(item["IPART_NO"]));
                    ei.Put("CNTR_NO", Prolink.Math.GetValueAsString(item["CNTR_NO"]));
                    ei.Put("PO_NO", Prolink.Math.GetValueAsString(item["PO_NO"]));
                    ei.Put("PART_NO", Prolink.Math.GetValueAsString(item["PART_NO"]));
                    ei.Put("UNIT_PRICE", Prolink.Math.GetValueAsDecimal(item["UNIT_PRICE"]));
                    ei.Put("AMT", Prolink.Math.GetValueAsDecimal(item["AMT"]));
                    ei.Put("VEN_CD", Prolink.Math.GetValueAsString(item["VEN_CD"]));
                    ei.Put("VEN_NM", Prolink.Math.GetValueAsString(item["VEN_NM"]));
                    ei.Put("VEN_ADDR", Prolink.Math.GetValueAsString(item["VEN_ADDR"]));
                    ei.Put("PLA_NO", Prolink.Math.GetValueAsString(item["PLA_NO"]));
                    ei.Put("PLA_SIZE", Prolink.Math.GetValueAsString(item["PLA_SIZE"]));
                    ei.Put("NWU", Prolink.Math.GetValueAsString(item["NWU"]));
                    ei.Put("GWU", Prolink.Math.GetValueAsString(item["GWU"]));
                    ei.Put("CBMU", Prolink.Math.GetValueAsString(item["CBMU"]));
                    ei.Put("NCM_NO", Prolink.Math.GetValueAsString(item["NCM_NO"]));
                    ei.Put("SHIPPING_MARK", Prolink.Math.GetValueAsString(item["SHIPPING_MARK"]));
                    ei.Put("PLACE", Prolink.Math.GetValueAsString(item["PLACE"]));
                    ei.Put("SMODEL", Prolink.Math.GetValueAsString(item["SMODEL"]));
                    ei.Put("PART_NUM", Prolink.Math.GetValueAsString(item["PART_NUM"]));
                    ei.Put("MODEL", Prolink.Math.GetValueAsString(item["MODEL"]));
                    ei.Put("IHS_CODE", Prolink.Math.GetValueAsString(item["IHS_CODE"]));
                    ml.Add(ei);
                }
                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }

            return msg;
        }
        #endregion

        #region 复制Invoice
        public static string insertInvoice(DataTable inddt, string UFid, string sm_id)
        {
            string msg = "success";
            EditInstruct ei;
            int[] resulst;
            MixedList ml = new MixedList();
            string sql = "SELECT MAX(SEQ_NO) FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(UFid);
            int seq = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (inddt.Rows.Count > 0)
            {
                foreach (DataRow item in inddt.Rows)
                {
                    seq = seq + 1;
                    ei = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION);
                    string selfuid = System.Guid.NewGuid().ToString();
                    ei.Put("U_ID", selfuid);
                    ei.Put("U_FID", UFid);
                    ei.Put("INVOICE_TYPE", "I");
                    ei.Put("SHIPMENT_ID", sm_id);
                    ei.Put("DN_NO", Prolink.Math.GetValueAsString(item["DN_NO"]));
                    ei.Put("INV_NO", Prolink.Math.GetValueAsString(item["INV_NO"]));
                    ei.Put("SEQ_NO", seq);
                    ei.Put("PO_NO", Prolink.Math.GetValueAsString(item["PO_NO"]));
                    ei.Put("SO_NO", Prolink.Math.GetValueAsString(item["SO_NO"]));
                    ei.Put("OPART_NO", Prolink.Math.GetValueAsString(item["OPART_NO"]));
                    ei.Put("PART_NO", Prolink.Math.GetValueAsString(item["PART_NO"]));
                    ei.Put("CATEGORY", Prolink.Math.GetValueAsString(item["CATEGORY"]));
                    ei.Put("IPART_NO", Prolink.Math.GetValueAsString(item["IPART_NO"]));
                    ei.Put("IHS_CODE", Prolink.Math.GetValueAsString(item["IHS_CODE"]));
                    ei.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                    ei.Put("BRAND", Prolink.Math.GetValueAsString(item["BRAND"]));
                    ei.Put("QTY", Prolink.Math.GetValueAsDecimal(item["QTY"]));
                    ei.Put("QTYU", Prolink.Math.GetValueAsString(item["QTYU"]));
                    ei.Put("CUR1", Prolink.Math.GetValueAsString(item["CUR1"]));
                    ei.Put("UNIT_PRICE1", Prolink.Math.GetValueAsString(item["UNIT_PRICE1"]));
                    ei.Put("AMT", Prolink.Math.GetValueAsDecimal(item["AMT"]));
                    ei.Put("REMARK", Prolink.Math.GetValueAsString(item["REMARK"]));
                    ei.Put("VEN_NM", Prolink.Math.GetValueAsString(item["VEN_NM"]));
                    ei.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                    ei.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                    //
                    ei.Put("OHS_CODE", Prolink.Math.GetValueAsString(item["OHS_CODE"]));
                    ei.Put("PROD_DESCP", Prolink.Math.GetValueAsString(item["PROD_DESCP"]));
                    ml.Add(ei);
                }

                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch(Exception ex)
                {
                    msg = ex.ToString();
                }
            }

            return msg;
        }
        #endregion

        #region 重算运保费
        public static decimal caluFreightFee(string UId)
        {
            string sql = "SELECT DN_NO FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string firstDn = "", DnNoCmpRef = "";
            decimal FreightFee = 0;
            if (DnNo == "")
            {
                sql = "SELECT SHIPMENT_ID FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 FREIGHT_AMT FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    FreightFee = Prolink.Math.GetValueAsDecimal(sdt.Rows[0]["FREIGHT_AMT"]);
                }
            }
            else if (DnNo != "")
            {
                sql = @"SELECT TOP 1 FREIGHT_AMT FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                FreightFee = Prolink.Math.GetValueAsDecimal(dnDt.Rows[0]["FREIGHT_AMT"]);
            }

            
            sql = "SELECT TTL_VALUE, ISSUE_FEE, FREIGHT_FEE FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable v = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            decimal TtlValue = Prolink.Math.GetValueAsDecimal(v.Rows[0]["TTL_VALUE"]);
            decimal IssueFee = Prolink.Math.GetValueAsDecimal(v.Rows[0]["ISSUE_FEE"]);

            decimal FobValue = TtlValue - FreightFee - IssueFee;

            sql = "UPDATE SMINM SET FOB_VALUE=" + FobValue + ", FREIGHT_FEE=" + FreightFee + " WHERE U_ID=" + SQLUtils.QuotedStr(UId);

            try 
            {
                Prolink.DataOperation.OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch(Exception ex)
            {
                
            }

            return FobValue;
        }
        #endregion
    }
}