using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebGui.App_Start;
using WebGui.Models;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json;
using Prolink.Model;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using TrackingEDI.Business;
namespace WebGui.Controllers
{
    public class InvoiceController : BaseController
    {

        public JsonResult GetDnptAndShipmentData()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);

            string sql = "SELECT * FROM SMDNPT WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE')";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            sql = "SELECT SHIPMENT_ID FROM SMSM WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT SMSM.SO_NO,SMSM.REF_NO,SMSM.ETD,SMSM.ETA,SMSM.HOUSE_NO,SMSM.VESSEL1  FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult shipResult = null;
            shipResult = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(sdt)
            };

            return Json(new { dnData = result.ToContent(), shipmentData = shipResult.ToContent() });
        }

        public JsonResult ReloadInvoice()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT DN_NO FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string etd = string.Empty;
            sql = "SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            BootstrapResult shipResult = new BootstrapResult();
            BootstrapResult partyResult = new BootstrapResult();
            BootstrapResult dnResult = new BootstrapResult();
            string InvoiceIntruction = "", InvoiceRemark = "", PackingRemark = "", PackingIntruction = "", firstDn = "", DnNoCmpRef = "";

            if (ShipmentId != "")
            {
                sql = "SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                string sm_uid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                bill.Share(sm_uid);
            }

            EdocHelper edochelper = new EdocHelper();
            if (DnNo == "")
            {
                sql = "SELECT SHIPMENT_ID FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 A.*, 
                        (SELECT TOP 1 MEMO FROM SMDN where MEMO IS NOT NULL AND SMDN.SHIPMENT_ID =A.SHIPMENT_ID) AS MEMO ,
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);

                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    edochelper.GetRef(ref sdt);
                    edochelper.Getblno(ref sdt);
                    shipResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(sdt)
                    };
                    if (sdt.Rows.Count > 0)
                    {
                        if (sdt.Rows[0]["ETD"] != DBNull.Value)
                            etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                    }
                    sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR1,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                PARTY_ATTN, PARTY_TEL, FAX_NO 
                                FROM SMSMPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG', 'WE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    edochelper.GetDt("2", ShipmentId, ref dt);

                    partyResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(dt)
                    };
                    sql = "SELECT INVOICE_RMK,PACKING_RMK,INV_INTRU,PKG_INTRU,BANK_MSG FROM SMINM  A WHERE EXISTS( SELECT B.* FROM SMINM B WHERE B.SHIPMENT_ID='{0}' AND B.COMBINE_INFO LIKE '%'+ A.DN_NO+'%'  )";
                    sql = string.Format(sql, ShipmentId);
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    InvoiceIntruction = edochelper.GetRemark(dt, "INV_INTRU");
                    InvoiceRemark = edochelper.GetRemark(dt, "INVOICE_RMK");
                    PackingRemark = edochelper.GetRemark(dt, "PACKING_RMK");
                    PackingIntruction = edochelper.GetRemark(dt, "PKG_INTRU");
                }
            }
            else if (DnNo != "")
            {
                sql = "SELECT PROFILE_CD FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                string _profilecode = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);

                if (sipInfo != null)
                {
                    InvoiceIntruction = sipInfo.InvoiceIntruction;
                    InvoiceRemark = sipInfo.InvoiceRemark;
                    PackingRemark = sipInfo.PackingRemark;
                    PackingIntruction = sipInfo.PackingIntruction;
                }
                sql = @"SELECT TOP 1 
                            VAT_NO, 
                            DN_NO_CMP_REF, 
                            ACT_POST_DATE, 
                            MEMO,
                            GOODS,
                            LGOODS,
                            SHIP_MARK AS MARKS, 
                            ETD AS DN_ETD, 
                            INCOTERM, 
                            INCOTERM_DESCP, 
                            TRADE_TERM, 
                            TRADETERM_DESCP,
                            PKG_UNIT_DESC,
                            PKG_NUM,
                            FREIGHT_AMT,
                            ISSUE_FEE,
                            TRANSACTE_MODE FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dnDt.Rows.Count > 0)
                {
                    string dn_no_cmp_ref = Prolink.Math.GetValueAsString(dnDt.Rows[0]["DN_NO_CMP_REF"]);
                    if (!string.IsNullOrEmpty(dn_no_cmp_ref))
                    {
                        string sql1 = string.Format("SELECT INCOTERM,INCOTERM_DESCP FROM SMDN WHERE DN_NO='{0}'", dn_no_cmp_ref);
                        DataTable oth = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (oth.Rows.Count > 0)
                        {
                            string incoterm = Prolink.Math.GetValueAsString(oth.Rows[0]["INCOTERM"]);
                            string incoterm_descp = Prolink.Math.GetValueAsString(oth.Rows[0]["INCOTERM_DESCP"]);
                            foreach (DataRow dr in dnDt.Rows)
                            {
                                dr["INCOTERM"] = incoterm;
                                dr["INCOTERM_DESCP"] = incoterm_descp;
                            }
                        }
                    }
                }
                dnResult = new BootstrapResult()
                {
                    rows = ModelFactory.ToTableJson(dnDt)
                };
                sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                CONTACT AS PARTY_ATTN, TEL AS PARTY_TEL, FAX_NO 
                                FROM SMDNPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG','WE') AND DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                edochelper.GetDt("1", DnNo, ref dt);

                partyResult = new BootstrapResult()
                {
                    rows = ModelFactory.ToTableJson(dt)
                };

                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    edochelper.GetRef(ref sdt);
                    edochelper.Getblno(ref sdt);
                    if (sdt.Rows.Count > 0)
                    {
                        if (sdt.Rows[0]["ETD"] != DBNull.Value)
                            etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                    }
                    shipResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(sdt)
                    };
                }
                else
                {
                    for (int k = 0; k < 99; k++)
                    {
                        sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                        DnNoCmpRef = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (DnNoCmpRef == "")
                        {
                            break;
                        }
                        else
                        {
                            firstDn = DnNoCmpRef;
                            DnNo = DnNoCmpRef;
                            continue;
                        }
                    }

                    if (firstDn != "")
                    {
                        sql = "SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(firstDn);
                        ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                        edochelper.GetRef(ref sdt);
                        edochelper.Getblno(ref sdt);
                        if (sdt.Rows.Count > 0)
                        {
                            if (sdt.Rows[0]["ETD"] != DBNull.Value)
                                etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                        }
                        shipResult = new BootstrapResult()
                        {
                            rows = ModelFactory.ToTableJson(sdt)
                        };
                    }
                }
            }

            return Json(new { partyData = partyResult.ToContent(), shipmentData = shipResult.ToContent(), dnData = dnResult.ToContent(), InvRemark = InvoiceRemark, InvIntro = InvoiceIntruction, PkgRemark = PackingRemark, PkgIntro = PackingIntruction, DnNo = DnNo, etd = etd });
        }


        public JsonResult InboundReloadInvoice()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT DN_NO FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string etd = string.Empty;
            sql = "SELECT SHIPMENT_ID FROM SMIDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            BootstrapResult shipResult = new BootstrapResult();
            BootstrapResult partyResult = new BootstrapResult();
            BootstrapResult dnResult = new BootstrapResult();
            string InvoiceIntruction = "", InvoiceRemark = "", PackingRemark = "", PackingIntruction = "", firstDn = "", DnNoCmpRef = "";

            EdocHelper edochelper = new EdocHelper();
            if (DnNo == "")
            {
                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 A.*, 
                        (SELECT TOP 1 MEMO FROM SMDN where MEMO IS NOT NULL AND SMDN.SHIPMENT_ID =A.SHIPMENT_ID) AS MEMO ,
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSMI A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);

                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    //edochelper.GetRef(ref sdt);
                    //edochelper.Getblno(ref sdt);
                    shipResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(sdt)
                    };
                    if (sdt.Rows.Count > 0)
                    {
                        if (sdt.Rows[0]["ETD"] != DBNull.Value)
                            etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                    }
                    sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PARTY_ADDR1,' ',PARTY_ADDR2,' ',PARTY_ADDR3,' ',PARTY_ADDR4,' ',PARTY_ADDR5) AS PART_ADDR,
                                PARTY_ATTN, PARTY_TEL, FAX_NO 
                                FROM SMSMIPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG', 'WE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    edochelper.GetDt("2", ShipmentId, ref dt);

                    partyResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(dt)
                    };
                    sql = "SELECT INVOICE_RMK,PACKING_RMK,INV_INTRU,PKG_INTRU,BANK_MSG FROM SMINM  A WHERE EXISTS( SELECT B.* FROM SMINM B WHERE B.SHIPMENT_ID='{0}' AND B.COMBINE_INFO LIKE '%'+ A.DN_NO+'%'  )";
                    sql = string.Format(sql, ShipmentId);
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    InvoiceIntruction = edochelper.GetRemark(dt, "INV_INTRU");
                    InvoiceRemark = edochelper.GetRemark(dt, "INVOICE_RMK");
                    PackingRemark = edochelper.GetRemark(dt, "PACKING_RMK");
                    PackingIntruction = edochelper.GetRemark(dt, "PKG_INTRU");
                }
            }
            else if (DnNo != "")
            {
                sql = "SELECT PROFILE_CD FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                string _profilecode = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);

                if (sipInfo != null)
                {
                    InvoiceIntruction = sipInfo.InvoiceIntruction;
                    InvoiceRemark = sipInfo.InvoiceRemark;
                    PackingRemark = sipInfo.PackingRemark;
                    PackingIntruction = sipInfo.PackingIntruction;
                }
                sql = @"SELECT TOP 1 
                            VAT_NO, 
                            DN_NO_CMP_REF, 
                            ACT_POST_DATE, 
                            MEMO,
                            GOODS,
                            LGOODS,
                            SHIP_MARK AS MARKS, 
                            ETD AS DN_ETD, 
                            INCOTERM, 
                            INCOTERM_DESCP, 
                            TRADE_TERM, 
                            TRADETERM_DESCP,
                            PKG_UNIT_DESC,
                            PKG_NUM,
                            FREIGHT_AMT,
                            ISSUE_FEE,
                            TRANSACTE_MODE FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dnDt.Rows.Count > 0)
                {
                    string dn_no_cmp_ref = Prolink.Math.GetValueAsString(dnDt.Rows[0]["DN_NO_CMP_REF"]);
                    if (!string.IsNullOrEmpty(dn_no_cmp_ref))
                    {
                        string sql1 = string.Format("SELECT INCOTERM,INCOTERM_DESCP FROM SMDN WHERE DN_NO='{0}'", dn_no_cmp_ref);
                        DataTable oth = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (oth.Rows.Count > 0)
                        {
                            string incoterm = Prolink.Math.GetValueAsString(oth.Rows[0]["INCOTERM"]);
                            string incoterm_descp = Prolink.Math.GetValueAsString(oth.Rows[0]["INCOTERM_DESCP"]);
                            foreach (DataRow dr in dnDt.Rows)
                            {
                                dr["INCOTERM"] = incoterm;
                                dr["INCOTERM_DESCP"] = incoterm_descp;
                            }
                        }
                    }
                }
                dnResult = new BootstrapResult()
                {
                    rows = ModelFactory.ToTableJson(dnDt)
                };
                sql = string.Format(@"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PARTY_ADDR1,' ',PARTY_ADDR2,' ',PARTY_ADDR3,' ',PARTY_ADDR4,' ',PARTY_ADDR5) AS PART_ADDR,
                                PARTY_ATTN, PARTY_TEL, FAX_NO 
                                FROM SMSMIPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG', 'WE') AND 
								PARTY_TYPE NOT IN (SELECT DISTINCT PARTY_TYPE FROM SMDNPT WHERE DN_NO={0})AND SHIPMENT_ID={1}
UNION
SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                CONTACT AS PARTY_ATTN, TEL AS PARTY_TEL, FAX_NO 
                                FROM SMDNPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG','WE')  AND DN_NO={0}", SQLUtils.QuotedStr(DnNo), SQLUtils.QuotedStr(ShipmentId));
                DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                edochelper.GetDt("1", DnNo, ref dt);

                partyResult = new BootstrapResult()
                {
                    rows = ModelFactory.ToTableJson(dt)
                };

                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSMI A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    //edochelper.GetRef(ref sdt);
                    //edochelper.Getblno(ref sdt);
                    if (sdt.Rows.Count > 0)
                    {
                        if (sdt.Rows[0]["ETD"] != DBNull.Value)
                            etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                    }
                    shipResult = new BootstrapResult()
                    {
                        rows = ModelFactory.ToTableJson(sdt)
                    };
                }
                else
                {
                    for (int k = 0; k < 99; k++)
                    {
                        sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                        DnNoCmpRef = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (DnNoCmpRef == "")
                        {
                            break;
                        }
                        else
                        {
                            firstDn = DnNoCmpRef;
                            DnNo = DnNoCmpRef;
                            continue;
                        }
                    }

                    if (firstDn != "")
                    {
                        sql = "SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(firstDn);
                        ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                        DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                        edochelper.GetRef(ref sdt);
                        edochelper.Getblno(ref sdt);
                        if (sdt.Rows.Count > 0)
                        {
                            if (sdt.Rows[0]["ETD"] != DBNull.Value)
                                etd = Prolink.Math.GetValueAsDateTime(sdt.Rows[0]["ETD"]).ToString("yyyy-MM-dd");
                        }
                        shipResult = new BootstrapResult()
                        {
                            rows = ModelFactory.ToTableJson(sdt)
                        };
                    }
                }
            }

            return Json(new { partyData = partyResult.ToContent(), shipmentData = shipResult.ToContent(), dnData = dnResult.ToContent(), InvRemark = InvoiceRemark, InvIntro = InvoiceIntruction, PkgRemark = PackingRemark, PkgIntro = PackingIntruction, DnNo = DnNo, etd = etd });
        }

        public JsonResult GetInvByDn()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string ShipmentId = "", sql = "";

            if (DnNo != "")
            {
                sql = "SELECT SHIPMENT_ID FROM SMSM WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            else
            {
                ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            }
            //string ShipmentId = "";
            //string sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            //string DnNoCmpRef = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string DnNoCmpRef = "";
            string firstDn = "";
            string InvoiceIntruction = "", InvoiceRemark = "", PackingRemark = "", PackingIntruction = "";

            sql = "SELECT PROFILE_CD FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string _profilecode = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);

            string con = "";
            /*
            if (DnNoCmpRef == "")
            {
                DnNoCmpRef = DnNo;
                con = "A.SHIPMENT_ID=B.SHIPMENT_ID";
            }
            else
            {
                con = "A.DN_NO=B.DN_NO_CMP_REF";
            }
            */

            if (ShipmentId == "")
            {
                for (int k = 0; k < 99; k++)
                {
                    sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                    DnNoCmpRef = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (DnNoCmpRef == "")
                    {
                        break;
                    }
                    else
                    {
                        firstDn = DnNoCmpRef;
                        DnNo = DnNoCmpRef;
                        continue;
                    }
                }
                sql = "SELECT SHIPMENT_ID FROM SMSM WHERE DN_NO=" + SQLUtils.QuotedStr(firstDn);
                ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            sql = @"SELECT TOP 1 
                            VAT_NO, 
                            DN_NO_CMP_REF, 
                            ACT_POST_DATE, 
                            MEMO,
                            LGOODS, 
                            ETD AS DN_ETD, 
                            INCOTERM, 
                            INCOTERM_DESCP, 
                            TRADE_TERM, 
                            TRADETERM_DESCP, 
                            TRANSACTE_MODE FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
           DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

           BootstrapResult dnResult = null;
           dnResult = new BootstrapResult()
           {
               rows = ModelFactory.ToTableJson(dnDt)
           };

            sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult shipResult = null;
            shipResult = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(sdt)
            };

            if (sipInfo != null)
            {
                InvoiceIntruction = sipInfo.InvoiceIntruction;
                InvoiceRemark = sipInfo.InvoiceRemark;
                PackingRemark = sipInfo.PackingRemark;
                PackingIntruction = sipInfo.PackingIntruction;
            }

            

            //sql = "SELECT SMPTY.PARTY_NO, SMPTY.PARTY_NAME, SMPTY.PART_ADDR1 FROM SMDNPT LEFT JOIN SMPTY ON SMDNPT.PARTY_NO=SMPTY.PARTY_NO WHERE SMDNPT.PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND SMDNPT.DN_NO=" + SQLUtils.QuotedStr(DnNo);

            if (DnNo != "")
            {
                sql = "SELECT PARTY_TYPE, PARTY_NO, PARTY_NAME, PART_ADDR FROM SMDNPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND DN_NO=" + SQLUtils.QuotedStr(DnNo);
            }
            else
            {
                sql = "SELECT PARTY_TYPE, PARTY_NO, PARTY_NAME, PART_ADDR1 AS PART_ADDR FROM SMSMPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            }
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };
            /*
            sql = "SELECT SMDNPT.PARTY_TYPE, SMDNPT.PARTY_NO, SMDNPT.PARTY_NAME, SMDNPT.PART_ADDR FROM SMDNPT  WHERE SMDNPT.PARTY_TYPE IN('RE') AND DN_NO=" + SQLUtils.QuotedStr(DnNoCmpRef);
            DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result1= null;
            result1 = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt1)
            };
             * */

            return Json(new { partyData = result.ToContent(), shipmentData = shipResult.ToContent(), dnData = dnResult.ToContent(), InvRemark = InvoiceRemark, InvIntro = InvoiceIntruction, PkgRemark = PackingRemark, PkgIntro = PackingIntruction });
        }

        public JsonResult GetInvByShipment()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);

            string sql = "SELECT PROFILE_CD FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string _profilecode = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);

            sql = "SELECT SMSMPT.PARTY_TYPE, SMPTY.PARTY_NO, SMPTY.PARTY_NAME, SMPTY.PART_ADDR1 FROM SMSMPT LEFT JOIN SMPTY ON SMSMPT.PARTY_NO=SMPTY.PARTY_NO WHERE SMSMPT.PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(dt)
            };

            sql = "SELECT *  FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
            DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            BootstrapResult shipResult = null;
            shipResult = new BootstrapResult()
            {
                rows = ModelFactory.ToTableJson(sdt)
            };

            return Json(new { dnData = result.ToContent(), shipmentData = shipResult.ToContent(), InvRemark = sipInfo.InvoiceRemark, InvIntro = sipInfo.InvoiceIntruction, PkgRemark = sipInfo.PackingRemark, PkgIntro = sipInfo.PackingIntruction });
        }

        public JsonResult updateShipment()
        {
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string returnMessage = "success";
            string sql = "SELECT TOP 1 * FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            MixedList mixList = new MixedList();
            EditInstruct ei;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                    decimal TtlNw = Prolink.Math.GetValueAsDecimal(item["TTL_NW"]);
                    decimal TtlGw = Prolink.Math.GetValueAsDecimal(item["TTL_GW"]);
                    decimal TtlCbm = Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]);
                    decimal TtlPlt = Prolink.Math.GetValueAsDecimal(item["TTL_PLT"]);
                    string ShipmentId = Prolink.Math.GetValueAsString(item["SHIPMENT_ID"]);
                    ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);

                    ei.PutKey("SHIPMENT_ID", ShipmentId);
                    //ei.Put("QTY", TtlQty);
                    ei.Put("NW", TtlNw);
                    ei.Put("GW", TtlGw);
                    ei.Put("CBM", TtlCbm);
                    //ei.Put("PLT_NUM", TtlPlt);
                    ei.Put("MODIFY_BY", UserId);
                    ei.PutDate("MODIFY_DATE", DateTime.Now);

                    mixList.Add(ei);
                }

                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }

            return Json(new {message = returnMessage });
        }

        public ActionResult InvQueryData()
        {
            string condition = "";
            //return GetBaseData("SMRV", condition, "*", " WS_CD ASC", "2");
            return GetBootstrapData("SMINM", condition);
        }

        private ActionResult GetBootstrapData(string table, string condition, string colNames = "*")
        {
            int recordsCount = 0, pageIndex = 0, pageSize = 0;

            string resultType = Request.Params["resultType"];
            DataTable dt = null;
            dt = ModelFactory.InquiryData("*", table, condition, Request.Params, ref recordsCount, ref pageIndex, ref pageSize);
            if (resultType == "excel")
                return ExportExcelFile(dt);
            BootstrapResult result = null;
            result = new BootstrapResult()
            {
                records = recordsCount,
                page = pageIndex,
                total = recordsCount % pageSize == 0 ? recordsCount / pageSize : (recordsCount / pageSize) + 1,
                rows = ModelFactory.ToTableJson(dt)
            };
            return result.ToContent();
        }

        #region Packing转Invoice
        public JsonResult transfer2Inv()
        {
            string changeData = Request.Params["changedData"];
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string InvNo = Prolink.Math.GetValueAsString(Request.Params["InvNo"]);
            string InvoiceType = Prolink.Math.GetValueAsString(Request.Params["InvoiceType"]);
            List<Dictionary<string, object>> indData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> pkgData = new List<Dictionary<string, object>>();
            string returnMessage = "success";
            if (changeData != null)
                changeData = System.Web.HttpUtility.UrlDecode(changeData);
            else
            {
                return Json(new { message = "No valid Data!" });
            }
            JavaScriptSerializer js = new JavaScriptSerializer();

            Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(changeData);
            MixedList mixList = new MixedList();
            foreach (var item in dict)
            {
                if (item.Key == "st2")
                {
                    string sql = "";
                    ArrayList objList = item.Value as ArrayList;
                    MixedList list = ModelFactory.JsonToEditMixedList(objList, "SmInpModel");
                    for (int i = 0; i < list.Count; i++)
                    {
                        EditInstruct ei = (EditInstruct)list[i];
                        if (ei.OperationType == EditInstruct.INSERT_OPERATION)
                        {
                            ei.Put("U_ID", Guid.NewGuid().ToString());
                            ei.Put("U_FID", UId);
                            ei.Put("SHIPMENT_ID", ShipmentId);
                            ei.Put("DN_NO", DnNo);
                            ei.Put("INV_NO", InvNo);
                            ei.Put("INVOICE_TYPE", InvoiceType);
                        }
                        else if (ei.OperationType == EditInstruct.UPDATE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                            ei.Put("SHIPMENT_ID", ShipmentId);
                            ei.Put("DN_NO", DnNo);
                            ei.Put("INV_NO", InvNo);
                            ei.Put("INVOICE_TYPE", InvoiceType);
                        }
                        else if (ei.OperationType == EditInstruct.DELETE_OPERATION)
                        {
                            ei.AddKey("U_ID");
                        }
                        mixList.Add(ei);
                    }
                }
            }

            EditInstruct ei2;
            MixedList mx = new MixedList();
            int[] resulst;
            if (mixList.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }

            string sql1 = "SELECT IPART_NO, GOODS_DESCP, QTYU, SUM(TTL＿QTY) AS TTL_QTY, AVG(AMT) AS AMT, SUM(TTL_NW) AS TTL_NW, SUM(TTL_GW) AS TTL_GW, SUM(TTL_CBM) AS TTL_CBM  FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(UId) + " GROUP BY IPART_NO, GOODS_DESCP, QTYU";
            DataTable dt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt1.Rows.Count > 0)
            {
                ei2 = new EditInstruct("SMIND", EditInstruct.DELETE_OPERATION);
                ei2.PutKey("U_FId", UId);
                mx.Add(ei2);
                int IndSeqNo = 0;
                foreach (DataRow item in dt1.Rows)
                {
                    string IpartNo = Prolink.Math.GetValueAsString(item["IPART_NO"]);
                    string GoodsDescp = Prolink.Math.GetValueAsString(item["GOODS_DESCP"]);
                    string Qtyu = Prolink.Math.GetValueAsString(item["QTYU"]);
                    decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                    IndSeqNo++;
                    ei2 = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION); ;
                    ei2.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei2.Put("U_FID", UId);
                    ei2.Put("INVOICE_TYPE", InvoiceType);
                    ei2.Put("SHIPMENT_ID", ShipmentId);
                    ei2.Put("DN_NO", DnNo);
                    ei2.Put("INV_NO", InvNo);
                    ei2.Put("IPART_NO", IpartNo);
                    ei2.Put("QTY", TtlQty);
                    //ei2.Put("QTYU", Qtyu);
                    ei2.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                    ei2.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                    ei2.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                    //ei2.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                    ei2.Put("SEQ_NO", IndSeqNo);
                    ei2.Put("GOODS_DESCP", GoodsDescp);
                    ei2.Put("QTYU", Qtyu);

                    mx.Add(ei2);

                }
                resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
            }

            sql1 = string.Format("SELECT * FROM SMIND WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
            DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            indData = ModelFactory.ToTableJson(dt, "SmIndModel");

            sql1 = string.Format("SELECT * FROM SMINP WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
            DataTable dt2 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            pkgData = ModelFactory.ToTableJson(dt2, "SmPkgModel");

            return Json(new { message = returnMessage, subData1 = indData, subData2 = pkgData });
        }
        #endregion

        #region 获取保費費率
        public JsonResult getTir()
        {
            string year = Prolink.Math.GetValueAsString(Request.Params["Etd"]);
            string sql = "SELECT CD_DESCP FROM BSCODE WHERE GROUP_ID='TPV' AND CD_TYPE='TIR' AND CD=" + SQLUtils.QuotedStr(year);
            string CdDescp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            return Json(new { tir = CdDescp});
        }

        public string getTirForBackend(string year)
        {
            string sql = "SELECT CD_DESCP FROM BSCODE WHERE GROUP_ID='TPV' AND CD_TYPE='TIR' AND CD=" + SQLUtils.QuotedStr(year);
            string CdDescp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            return CdDescp;
        }
        #endregion

        #region 合并Invoice
        public JsonResult chkCombineO()
        {
            string sm_id = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string sql = "SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            string msg = "N";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string dn_no = Prolink.Math.GetValueAsString(item["DN_NO"]);

                    sql = "SELECT COUNT(*) FROM SMINM WHERE COMBINE_FLAG='Y' AND DN_NO = " + SQLUtils.QuotedStr(dn_no);
                    int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if(num > 0)
                    {
                        msg = "Y";
                        break;
                    }
                }
            }
            return Json(new {msg = msg});
        }

        public JsonResult chkCombineI()
        {
            string sm_id = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string c_com = Prolink.Math.GetValueAsString(Request.Params["c_com"]);
            string sql = "SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id) + " AND STN=" + SQLUtils.QuotedStr(c_com);
            string msg = "N";
            string cmp_dn_no_ref = "";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    string dn_no = Prolink.Math.GetValueAsString(item["DN_NO"]);
                    for (int k = 0; k < 99; k++)
                    {
                        sql = "SELECT DN_NO_CMP_REF FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dn_no);
                        cmp_dn_no_ref = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (cmp_dn_no_ref == "")
                        {
                            break;
                        }
                        else
                        {
                            dn_no = cmp_dn_no_ref;
                            continue;
                        }
                    }

                    sql = "SELECT COUNT(*) FROM SMINM WHERE COMBINE_FLAG='Y' AND DN_NO = " + SQLUtils.QuotedStr(dn_no);
                    int num = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (num > 0)
                    {
                        msg = "Y";
                        break;
                    }
                }
            }
            return Json(new { msg = msg });
        }
        public JsonResult CombineInvO()
        {
            string returnMessage = "success";
            string dn_ids = Prolink.Math.GetValueAsString(Request.Params["dn_ids"]);
            string sm_id = Prolink.Math.GetValueAsString(Request.Params["sm_id"]);
            string cover = Prolink.Math.GetValueAsString(Request.Params["cover"]);
            string sql = "SELECT TOP 1 * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int[] resulst;
            MixedList ml = new MixedList();
            MixedList delml = new MixedList();
            string o_uid = "";
            string con = "";
            //if (CheckReserve(sm_id)) return Json(new { msg = "该笔资料已经预约，不能进行合并操作，请查验！" });
            sql = "SELECT TOP 1 COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            string CombineInfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string[] c = CombineInfo.Split(',');
            string new_c = string.Empty;
            for (int i = 0; i < c.Length; i++)
            {
                new_c += SQLUtils.QuotedStr(c[i]) + ",";
            }

            new_c = new_c.Remove(new_c.Length - 1);

            if (cover == "Y")
            {
                sql = "SELECT TOP 1 U_ID FROM SMINM WHERE PACKING_FROM='C' AND DN_NO='' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                string UId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                try
                {

                    if (UId != "")
                    {
                        sql = "DELETE FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                        delml.Add(sql);
                        sql = "DELETE FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                        delml.Add(sql);
                        sql = "DELETE FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                        delml.Add(sql);

                        OperationUtils.ExecuteUpdate(delml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    
                }
                catch (Exception ex)
                {
                    return Json(new { msg = @Resources.Locale.L_Invoice_Controllers_371 });
                }
            }

            /*产生invoice Inbound, Outbound表头*/
            o_uid = CreateInv(sm_id, "", "O", "SMSMPT");

            if (o_uid != "false")
            {
                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ml = new MixedList();
                    sql = "SELECT DN_NO FROM SMDN WHERE DN_NO IN(" + new_c + ")";
                    DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dnDt.Rows.Count > 0)
                    {
                        int i = 0;
                        foreach (DataRow dnitem in dnDt.Rows)
                        {
                            string dn_no = Prolink.Math.GetValueAsString(dnitem["DN_NO"]);
                            if (i == 0)
                            {
                                con += SQLUtils.QuotedStr(dn_no);
                            }
                            else
                            {
                                con += "," + SQLUtils.QuotedStr(dn_no);
                            }
                            if (dn_no != "")
                            {
                                /*复制出口packing到合并invoice*/
                                sql = "SELECT SMINP.* FROM SMINP, SMINM WHERE SMINP.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINM.INVOICE_TYPE='O' ORDER BY SMINP.SEQ_NO ASC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                DataTable inpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMessage = Business.InvHelper.insertPacking(inpdt, o_uid, sm_id);

                                /*复制出口invoice明细到合并invoice*/
                                sql = "SELECT SMIND.* FROM SMIND, SMINM WHERE SMIND.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINM.INVOICE_TYPE='O' ORDER BY SMIND.SEQ_NO ASC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                DataTable inddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMessage = Business.InvHelper.insertInvoice(inddt, o_uid, sm_id);

                            }
                            i++;
                        }
                    }

                    sql = "SELECT * FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(o_uid) + "ORDER BY DN_NO ASC";
                    DataTable seqpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (seqpdt.Rows.Count > 0)
                    {
                        int seq = 0;
                        foreach (DataRow item in seqpdt.Rows)
                        {
                            string uid = Prolink.Math.GetValueAsString(item["U_ID"]);
                            sql = "UPDATE SMINP SET SEQ_NO=" + seq + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            seq++;
                        }
                    }

                    sql = "SELECT * FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(o_uid) + "ORDER BY DN_NO ASC";
                    DataTable seqidt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    if (seqidt.Rows.Count > 0)
                    {
                        int seq = 0;
                        foreach (DataRow item in seqidt.Rows)
                        {
                            string uid = Prolink.Math.GetValueAsString(item["U_ID"]);
                            sql = "UPDATE SMIND SET SEQ_NO=" + seq + " WHERE U_ID=" + SQLUtils.QuotedStr(uid);
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            seq++;
                        }
                    }


                    sql = "UPDATE SMINM SET SHIPMENT_ID='', COMBINE_FLAG='Y' WHERE DN_NO IN ({0}) AND INVOICE_TYPE='O'";
                    sql = string.Format(sql, con);
                    try
                    {
                        if(con != "")
                        {
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        
                        EditInstruct ei2;
                        MixedList mx = new MixedList();
                        string sql1 = "SELECT SUM(QTY) AS TTL_QTY, SUM(TTL_NW) AS TTL_NW, SUM(TTL_GW) AS TTL_GW, SUM(TTL_CBM) AS TTL_CBM, SUM(CASE_NUM) AS TTL_PLT  FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(o_uid);
                        DataTable dt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (dt1.Rows.Count > 0)
                        {

                            foreach (DataRow item in dt1.Rows)
                            {
                                decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                                ei2 = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION); ;
                                ei2.PutKey("U_ID", o_uid);
                                ei2.Put("TTL_QTY", TtlQty);
                                ei2.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                                ei2.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                                ei2.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                                ei2.Put("TTL_PLT", Prolink.Math.GetValueAsDecimal(item["TTL_PLT"]));
                                mx.Add(ei2);
                            }
                            try
                            {
                                resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());

                                string Dlv_Way = string.Empty;
                                string trade_term = string.Empty;
                                string cur = string.Empty;
                                string tradeterm_descp = string.Empty;
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    string trantype = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                                    if ("F".Equals(trantype) || "L".Equals(trantype))
                                    {
                                        Dlv_Way = "By Sea";
                                    }
                                    else if ("A".Equals(trantype))
                                    {
                                        Dlv_Way = "By Air";
                                    }
                                    else if ("D".Equals(trantype) || "E".Equals(trantype))
                                    {
                                        Dlv_Way = "By Express";
                                    }
                                    else if ("R".Equals(trantype))
                                    {
                                        Dlv_Way = "By Railroad";
                                    }
                                    trade_term = Prolink.Math.GetValueAsString(dt.Rows[0]["TRADE_TERM"]);
                                    tradeterm_descp = Prolink.Math.GetValueAsString(dt.Rows[0]["TRADETERM_DESCP"]);
                                    cur = Prolink.Math.GetValueAsString(dt.Rows[0]["CUR"]);
                                }
                                sql = "UPDATE SMINM SET TTL_VALUE=(SELECT SUM(AMT) FROM SMIND WHERE SMIND.U_FID={0}),Dlv_Way={1},TRADE_TERM={2},TRADETERM_DESCP={3}, CUR={4} WHERE U_ID={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(o_uid), SQLUtils.QuotedStr(Dlv_Way), SQLUtils.QuotedStr(trade_term), SQLUtils.QuotedStr(tradeterm_descp), SQLUtils.QuotedStr(cur));
                                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                returnMessage = ex.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_InvoiceController_Controllers_160;
            }
            

            return Json(new { msg = returnMessage });
        }

        public bool CheckReserve(string id)
        {
            bool result=false;
            string sql =string.Format("SELECT * FROM SMRV  WHERE  SHIPMENT_ID='{0}'",id);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
                result = true;
            return result;
        }
        public JsonResult CombineInvI()
        {
            /*进口invoice需找到最后一段关联DN*/
            string returnMessage = "success";
            string Company = Prolink.Math.GetValueAsString(Request.Params["c_com"]);
            string sm_id = Prolink.Math.GetValueAsString(Request.Params["sm_id"]);
            string cover = Prolink.Math.GetValueAsString(Request.Params["cover"]);
            string sql = "SELECT TOP 1 * FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            int[] resulst;
            MixedList ml = new MixedList();
            MixedList delml = new MixedList();
            string o_uid = "";
            string con = "";
            string cmp_dn_no_ref = "";

            //if (CheckReserve(sm_id)) return Json(new { msg = "该笔资料已经预约，不能进行合并操作，请查验！" });

            /*产生invoice Inbound, Outbound表头*/
            o_uid = CreateInv(sm_id, "", "I", "SMSMPT", Company);

            sql = "SELECT TOP 1 COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            string CombineInfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string[] c = CombineInfo.Split(',');
            string new_c = string.Empty;
            for (int i = 0; i < c.Length; i++)
            {
                new_c += SQLUtils.QuotedStr(c[i]) + ",";
            }

            new_c = new_c.Remove(new_c.Length - 1);

            //sql = "SELECT COUNT(*) FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id) + " AND STN=" + SQLUtils.QuotedStr(Company);
            sql = "SELECT COUNT(*) FROM SMDN WHERE DN_NO IN (" + new_c + ")";
            int dns = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if(dns == 0)
            {
                return Json(new { msg = @Resources.Locale.L_InvoiceController_Controllers_161 });
            }

            if (cover == "Y")
            {
                sql = "SELECT TOP 1 U_ID FROM SMINM WHERE PACKING_FROM='C' AND DN_NO='' AND SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id) + " AND SHPR_CD=" + SQLUtils.QuotedStr(Company);
                string UId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                try
                {
                    sql = "DELETE FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
                    delml.Add(sql);
                    sql = "DELETE FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                    delml.Add(sql);
                    sql = "DELETE FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(UId);
                    delml.Add(sql);

                    OperationUtils.ExecuteUpdate(delml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    return Json(new { msg = @Resources.Locale.L_Invoice_Controllers_371 });
                }
            }

            if (o_uid != "false")
            {
                try
                {
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    ml = new MixedList();
                    string dnInfo = "";
                    //sql = "SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id) + " AND STN=" + SQLUtils.QuotedStr(Company);
                    sql = "SELECT DN_NO, STN FROM SMDN WHERE DN_NO IN (" + new_c + ")";
                    DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dnDt.Rows.Count > 0)
                    {
                        int i = 0;
                        foreach (DataRow dnitem in dnDt.Rows)
                        {
                            string dn_no = Prolink.Math.GetValueAsString(dnitem["DN_NO"]);
                            string firstStn = Prolink.Math.GetValueAsString(dnitem["STN"]);
                            if (i == 0)
                            {
                                con += SQLUtils.QuotedStr(dn_no);
                            }
                            else
                            {
                                con += "," + SQLUtils.QuotedStr(dn_no);
                            }

                            if (firstStn == Company)
                            {
                                /*复制出口packing到合并invoice*/
                                sql = "SELECT SMINP.* FROM SMINP, SMINM WHERE SMINP.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINP.INVOICE_TYPE='O' ORDER BY SMINP.SEQ_NO ASC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                DataTable inpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMessage = Business.InvHelper.insertPacking(inpdt, o_uid, sm_id);

                                /*复制出口invoice明细到合并invoice*/
                                sql = "SELECT SMIND.* FROM SMIND, SMINM WHERE SMIND.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMIND.INVOICE_TYPE='O' ORDER BY SMIND.SEQ_NO ASC";
                                sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                DataTable inddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                returnMessage = Business.InvHelper.insertInvoice(inddt, o_uid, sm_id);

                                dnInfo = string.Join(",", dnInfo, dn_no);
                                continue;
                            }

                            /*进口invoice需找到最后一段关联DN*/
                            for (int k = 0; k < 99; k++)
                            {
                                sql = "SELECT REF_NO, STN FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dn_no);
                                DataTable refDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (refDt.Rows.Count > 0)
                                { 
                                    string ref_no = Prolink.Math.GetValueAsString(refDt.Rows[0]["REF_NO"]);
                                    string stn = Prolink.Math.GetValueAsString(refDt.Rows[0]["STN"]);
                                    if (stn == Company)
                                    {
                                        /*复制出口packing到合并invoice*/
                                        sql = "SELECT SMINP.* FROM SMINP, SMINM WHERE SMINP.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINP.INVOICE_TYPE='O' ORDER BY SMINP.SEQ_NO ASC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                        DataTable inpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        returnMessage = Business.InvHelper.insertPacking(inpdt, o_uid, sm_id);

                                        /*复制出口invoice明细到合并invoice*/
                                        sql = "SELECT SMIND.* FROM SMIND, SMINM WHERE SMIND.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMIND.INVOICE_TYPE='O' ORDER BY SMIND.SEQ_NO ASC";
                                        sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                                        DataTable inddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                        returnMessage = Business.InvHelper.insertInvoice(inddt, o_uid, sm_id);

                                        dnInfo = string.Join(",", dnInfo, dn_no);
                                    }
                                    dn_no = ref_no;

                                    if(ref_no == "")
                                    {
                                        break;
                                    }

                                    
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            
                            //if (dn_no != "")
                            //{
                            //    /*复制出口packing到合并invoice*/
                            //    sql = "SELECT SMINP.* FROM SMINP, SMINM WHERE SMINP.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINM.INVOICE_TYPE='O' ORDER BY SMINP.SEQ_NO ASC";
                            //    sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                            //    DataTable inpdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //    returnMessage = Business.InvHelper.insertPacking(inpdt, o_uid, sm_id);

                            //    /*复制出口invoice明细到合并invoice*/
                            //    sql = "SELECT SMIND.* FROM SMIND, SMINM WHERE SMIND.U_FID=SMINM.U_ID AND SMINM.DN_NO={0} AND SMINM.INVOICE_TYPE='O' ORDER BY SMIND.SEQ_NO ASC";
                            //    sql = string.Format(sql, SQLUtils.QuotedStr(dn_no));
                            //    DataTable inddt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //    returnMessage = Business.InvHelper.insertInvoice(inddt, o_uid, sm_id);

                            //    dnInfo = string.Join(",", dnInfo, dn_no);
                            //}
                            
                            i++;
                        }
                    }


                    sql = "UPDATE SMINM SET SHIPMENT_ID='', COMBINE_FLAG='Y' WHERE DN_NO IN ({0}) AND INVOICE_TYPE='I'";
                    sql = string.Format(sql, con);
                    try
                    {
                        if (con != "")
                        {
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                        EditInstruct ei2;
                        MixedList mx = new MixedList();
                        string sql1 = "SELECT SUM(QTY) AS TTL_QTY, SUM(TTL_NW) AS TTL_NW, SUM(TTL_GW) AS TTL_GW, SUM(TTL_CBM) AS TTL_CBM, SUM(CASE_NUM) AS TTL_PLT  FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(o_uid);
                        DataTable dt1 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                        if (dt1.Rows.Count > 0)
                        {

                            foreach (DataRow item in dt1.Rows)
                            {
                                decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                                ei2 = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION); ;
                                ei2.PutKey("U_ID", o_uid);
                                ei2.Put("TTL_QTY", TtlQty);
                                ei2.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                                ei2.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                                ei2.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                                ei2.Put("TTL_PLT", Prolink.Math.GetValueAsDecimal(item["TTL_PLT"]));
                                mx.Add(ei2);
                            }
                            try
                            {
                                resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());

                                //sql = "UPDATE SMINM SET TTL_VALUE=(SELECT SUM(AMT) FROM SMIND WHERE SMIND.U_FID={0}) WHERE U_ID={0}";
                                sql = "UPDATE SMINM SET TTL_VALUE=(SELECT SUM(AMT) FROM SMIND WHERE SMIND.U_FID={0})  ,INV_COMBINE_FLAG={1} WHERE U_ID={0}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(o_uid), SQLUtils.QuotedStr(Company));
                                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                            }
                            catch (Exception ex)
                            {
                                returnMessage = ex.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        returnMessage = ex.ToString();
                    }
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_InvoiceController_Controllers_160;
            }


            return Json(new { msg = returnMessage });
        }
        #endregion

        #region 取消合并
        public JsonResult CancelCombineInvO()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string returnMessage = "success";
            string sql = "SELECT U_ID FROM SMINM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND (DN_NO='' OR DN_NO IS NULL)";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                MixedList ml = new MixedList();
                foreach (DataRow item in dt.Rows)
                {
                    string u_id = Prolink.Math.GetValueAsString(item["U_ID"]);
                    sql = "DELETE FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                    sql = "DELETE FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                    sql = "DELETE FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                }

                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                    sql = "UPDATE SMINM SET SHIPMENT_ID={0}, COMBINE_FLAG='' WHERE DN_NO IN (SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0} AND COMBINE_FLAG='Y')";
                    sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_InvoiceController_Controllers_162;
            }
            


            return Json(new { msg = returnMessage });
        }

        public JsonResult CancelCombineInvI()
        {
            string ShipmentId = Prolink.Math.GetValueAsString(Request.Params["ShipmentId"]);
            string c_com = Prolink.Math.GetValueAsString(Request.Params["c_com"]);
            string returnMessage = "success";
            string sql = "SELECT U_ID FROM SMINM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND SHPR_CD=" + SQLUtils.QuotedStr(c_com);
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (dt.Rows.Count > 0)
            {
                MixedList ml = new MixedList();
                foreach (DataRow item in dt.Rows)
                {
                    string u_id = Prolink.Math.GetValueAsString(item["U_ID"]);
                    sql = "DELETE FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                    sql = "DELETE FROM SMIND WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                    sql = "DELETE FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(u_id);
                    ml.Add(sql);
                }

                try
                {
                    OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                    sql = "SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId) + " AND STN=" + SQLUtils.QuotedStr(c_com);
                    DataTable dnDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (dnDt.Rows.Count > 0)
                    {
                        foreach(DataRow item in dnDt.Rows)
                        {
                            string dn_no = Prolink.Math.GetValueAsString(item["DN_NO"]);
                            sql = "UPDATE SMINM SET SHIPMENT_ID={0}, COMBINE_FLAG='' WHERE DN_NO="+SQLUtils.QuotedStr(dn_no)+" AND COMBINE_FLAG='Y'";
                            sql = string.Format(sql, SQLUtils.QuotedStr(ShipmentId));
                            OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    returnMessage = ex.ToString();
                }
            }
            else
            {
                returnMessage = @Resources.Locale.L_InvoiceController_Controllers_162;
            }



            return Json(new { msg = returnMessage });
        }
        #endregion

        #region 获取Combine DN
        public JsonResult getCombinDn()
        { 
            string returnMessage = "success";
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string condition = "";

            string sql = "SELECT TOP 1 COMBINE_INFO FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string CombineInfo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            string[] c = CombineInfo.Split(',');
            string new_c = string.Empty;
            for (int i = 0; i < c.Length; i++ )
            {
                new_c += SQLUtils.QuotedStr(c[i]) + ",";
            }

            new_c = new_c.Remove(new_c.Length - 1);

            condition = " DN_NO IN (" + new_c + ")";
            ActionResult result = null;
            result = GetBootstrapData("SMDN", condition);
            return Json(new { message = returnMessage, returnData = result });
        }
        #endregion

        #region Upload 成品Packing
        [HttpPost]
        public ActionResult UploadLntPacking(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            if (StartRow == 0)
            {
                returnMessage = @Resources.Locale.L_GateManage_Controllers_299;
                return Json(new { message = returnMessage });
            }

            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            else
            {
                string strExt = file.FileName.Split('.')[1].ToUpper();
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                EditInstruct ei;
                EditInstruct ei2;
                int[] resulst;
                /*
                 * dr[0]: DN_NO
                 * dr[1]: CASE_NO
                 * dr[2]: CASE_NUM
                 * dr[3]: IPART_NO
                 * dr[4]: MODEL
                 * dr[5]: PART_NO
                 * dr[6]: SMODEL
                 * dr[7]: PLACE
                 * dr[8]: QTY
                 * dr[9]: QTYU
                 * dr[10]: TTL_QTY
                 * dr[11]: PROD_DESCP
                 * dr[12]: GOODS_DESCP
                 * dr[13]: NW
                 * dr[14]: TTL_NW
                 * dr[15]: GW
                 * dr[16]: TTL_GW
                 * dr[17]: CBM
                 * dr[18]: TTL_CBM
                 * dr[19]: SHIPPING_MARK1
                 * dr[20]: SHIPPING_MARK2
                 * dr[21]: SHIPPING_MARK3
                 * dr[22]: SHIPPING_MARK4
                 * dr[23]: SHIPPING_MARK5
                 * dr[24]: SHIPPING_MARK6
                 * dr[25]: REMARK
                 */
                string odnno = "";
                string sminm_uid = "";
                string sm_id = "";
                string dn_no = "";
                try
                {
                    int SeqNo = 1;
                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];

                        
                        string DnNo = Prolink.Math.GetValueAsString(dr[0]);
                        
                        if (DnNo == "")
                        {
                            break; // 沒有DnNo就結束匯入
                        }

                        if(odnno == "" || (DnNo != odnno))
                        {
                            string sql = "";

                            if (odnno != "")
                            {
                                resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

                                sql = "SELECT IPART_NO, GOODS_DESCP, QTYU, SUM(QTY) AS TTL_QTY, AVG(AMT) AS AMT, SUM(TTL_NW) AS TTL_NW, SUM(TTL_GW) AS TTL_GW, SUM(TTL_CBM) AS TTL_CBM  FROM SMINP WHERE U_FID=" + SQLUtils.QuotedStr(sminm_uid) + " GROUP BY IPART_NO, OPART_NO, QTYU, GOODS_DESCP";
                                DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                                if (dt1.Rows.Count > 0)
                                {
                                    int IndSeqNo = 0;
                                    foreach (DataRow item in dt1.Rows)
                                    {
                                        string IpartNo = Prolink.Math.GetValueAsString(item["IPART_NO"]);
                                        string Qtyu = Prolink.Math.GetValueAsString(item["QTYU"]);
                                        decimal TtlQty = Prolink.Math.GetValueAsDecimal(item["TTL_QTY"]);
                                        IndSeqNo++;
                                        ei2 = new EditInstruct("SMIND", EditInstruct.INSERT_OPERATION); ;
                                        ei2.Put("U_ID", System.Guid.NewGuid().ToString());
                                        ei2.Put("U_FID", sminm_uid);
                                        ei2.Put("INVOICE_TYPE", "I");
                                        ei2.Put("DN_NO", DnNo);
                                        ei2.Put("IPART_NO", IpartNo);
                                        ei2.Put("QTY", TtlQty);
                                        ei2.Put("QTYU", Qtyu);
                                        ei2.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                                        ei2.Put("TTL_NW", Prolink.Math.GetValueAsDecimal(item["TTL_NW"]));
                                        ei2.Put("TTL_GW", Prolink.Math.GetValueAsDecimal(item["TTL_GW"]));
                                        ei2.Put("TTL_CBM", Prolink.Math.GetValueAsDecimal(item["TTL_CBM"]));
                                        ei2.Put("GOODS_DESCP", Prolink.Math.GetValueAsString(item["GOODS_DESCP"]));
                                        ei2.Put("SEQ_NO", IndSeqNo);

                                        mx.Add(ei2);

                                    }
                                    resulst = OperationUtils.ExecuteUpdate(mx, Prolink.Web.WebContext.GetInstance().GetConnection());
                                }
                            }
                            sql = "SELECT DN_NO, SHIPMENT_ID FROM SMSM WHERE SUB_DN_NO=" + SQLUtils.QuotedStr(DnNo);
                            DataTable sm_dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                            if (sm_dt.Rows.Count > 0)
                            {
                                sm_id = Prolink.Math.GetValueAsString(sm_dt.Rows[0]["SHIPMENT_ID"]);
                                dn_no = Prolink.Math.GetValueAsString(sm_dt.Rows[0]["DN_NO"]);

                                if(sm_id != "" && dn_no != "")
                                {
                                    sminm_uid = CreateInv(sm_id, dn_no, "I", "SMDNPT");
                                }
                            }
                            SeqNo = 1;

                        }
                        if (sminm_uid != "false" && sminm_uid != "")
                        {
                            string CaseNo = Prolink.Math.GetValueAsString(dr[1]);
                            decimal CaseNum = Prolink.Math.GetValueAsDecimal(dr[2]);
                            string IpartNo = Prolink.Math.GetValueAsString(dr[3]);
                            string Model = Prolink.Math.GetValueAsString(dr[4]);
                            string PartNo = Prolink.Math.GetValueAsString(dr[5]);
                            string Smodel = Prolink.Math.GetValueAsString(dr[6]);
                            string Place = Prolink.Math.GetValueAsString(dr[7]);
                            decimal Qty = Prolink.Math.GetValueAsDecimal(dr[8]);
                            string Qtyu = Prolink.Math.GetValueAsString(dr[9]);
                            decimal TtlQty = Prolink.Math.GetValueAsDecimal(dr[10]);
                            string ProdDescp = Prolink.Math.GetValueAsString(dr[11]);
                            string GoodsDescp = Prolink.Math.GetValueAsString(dr[12]);
                            decimal Nw = Prolink.Math.GetValueAsDecimal(dr[13]);
                            decimal TtlNw = Prolink.Math.GetValueAsDecimal(dr[14]);
                            decimal Gw = Prolink.Math.GetValueAsDecimal(dr[15]);
                            decimal TtlGw = Prolink.Math.GetValueAsDecimal(dr[16]);
                            decimal Cbm = Prolink.Math.GetValueAsDecimal(dr[17]);
                            decimal TtlCbm = Prolink.Math.GetValueAsDecimal(dr[18]);
                            string Sm1 = Prolink.Math.GetValueAsString(dr[19]);
                            string Sm2 = Prolink.Math.GetValueAsString(dr[20]);
                            string Sm3 = Prolink.Math.GetValueAsString(dr[21]);
                            string Sm4 = Prolink.Math.GetValueAsString(dr[22]);
                            string Sm5 = Prolink.Math.GetValueAsString(dr[23]);
                            string Sm6 = Prolink.Math.GetValueAsString(dr[24]);
                            string Remark = Prolink.Math.GetValueAsString(dr[25]);
                            string ShippingMark = "";

                            ShippingMark = Sm1 + "\n" + Sm2 + "\n" + Sm3 + "\n" + Sm4 + "\n" + Sm5 + "\n" + Sm6;
                            ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                            ei.Put("U_ID", System.Guid.NewGuid().ToString());
                            ei.Put("U_FID", sminm_uid);
                            ei.Put("SHIPMENT_ID", sm_id);
                            ei.Put("DN_NO", DnNo);
                            ei.Put("INVOICE_TYPE", "I");
                            ei.Put("INV_NO", dn_no.Substring(3, dn_no.Length-3));
                            ei.Put("SEQ_NO", SeqNo++);
                            ei.Put("CASE_NO", CaseNo);
                            ei.Put("CASE_NUM", CaseNum);
                            ei.Put("IPART_NO", IpartNo);
                            ei.Put("MODEL", Model);
                            ei.Put("PART_NO", PartNo);
                            ei.Put("SMODEL", Smodel);
                            ei.Put("PLACE", Place);
                            ei.Put("QTY", Qty);
                            ei.Put("QTYU", Qtyu);
                            ei.Put("TTL_QTY", TtlQty);
                            ei.Put("PROD_DESCP", ProdDescp);
                            ei.Put("GOODS_DESCP", GoodsDescp);
                            ei.Put("NW", Nw);
                            ei.Put("TTL_NW", TtlNw);
                            ei.Put("GW", Gw);
                            ei.Put("TTL_GW", TtlGw);
                            ei.Put("CBM", Cbm);
                            ei.Put("TTL_CBM", TtlCbm);
                            ei.Put("SHIPPING_MARK", ShippingMark);
                            ml.Add(ei);
                            odnno = DnNo;
                        }
                        
                    }
                    
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }
        #endregion

        #region Create Invoice 表头
        public string CreateInv(string sm_id, string dn_no, string InvType, string party, string c_com="")
        {
            string sql = "SELECT TOP 1 *,(SELECT TOP 1 CNTRY_CD FROM BSCITY WHERE BSCITY.CNTRY_CD+BSCITY.PORT_CD=SMSM.POL_CD) AS CNTRY_ORN,(SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=SMSM.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP  FROM SMSM WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
            DataTable sm_dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            sql = "SELECT TOP 1 * FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(dn_no);
            DataTable dn_dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct ei;
            int[] resulst;
            MixedList ml = new MixedList();
            string uid = System.Guid.NewGuid().ToString();
            ei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", uid);
            ei.Put("DN_NO", dn_no);
            if (dn_no != "")
            {
                ei.Put("INV_NO", dn_no.Substring(3, dn_no.Length - 3));
                ei.Put("PACKING_NO", dn_no.Substring(3, dn_no.Length - 3));
            }
            else
            {
                ei.Put("INV_NO", sm_id);
                ei.Put("PACKING_NO", sm_id);
            }
            
            ei.Put("INVOICE_TYPE", InvType);
            if (sm_dt.Rows.Count > 0)
            {
                ei.Put("FROM_CD", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["POL_CD"]));
                ei.Put("FROM_DESCP", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["POL_NAME"]));
                ei.Put("TO_CD", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["POD_CD"]));
                ei.Put("TO_DESCP", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["POD_NAME"]));
                ei.Put("INCOTERM", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["INCOTERM_CD"]));
                ei.Put("INCOTERM_DESCP", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["INCOTERM_DESCP"]));
                ei.Put("COMBINE_INFO", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["COMBINE_INFO"]));
                ei.PutDate("SHIPPING_DATE", Prolink.Math.GetValueAsDateTime(sm_dt.Rows[0]["ETD"]));
                ei.PutDate("ETD", Prolink.Math.GetValueAsDateTime(sm_dt.Rows[0]["ETD"]));
                ei.PutDate("ETA", Prolink.Math.GetValueAsDateTime(sm_dt.Rows[0]["ETA"]));
                ei.Put("NWU", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["GWU"]));
                ei.Put("SHIPMENT_ID", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["SHIPMENT_ID"]));
                
                ei.Put("CMDTY_CD", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["GOODS"]));
                ei.Put("LGOODS", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["LGOODS"]));
                ei.Put("TTL_PLT", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["PKG_NUM"]));
                ei.Put("PLTU", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["PKG_UNIT"]));
                ei.Put("MARKS", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["MARKS"]));
                ei.Put("TRANSACTE_MODE", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["TRANSACTE_MODE"]));
                ei.Put("FREIGHT_FEE", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["FREIGHT_AMT"]));
                ei.Put("ISSUE_FEE", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["INSURANCE_AMT"]));
                ei.Put("PKG_UNIT_DESC", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["PKG_UNIT_DESC"]));
                ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["GROUP_ID"]));
                ei.Put("CMP", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["CMP"]));
                ei.Put("CNTRY_ORN", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["CNTRY_ORN"]));
                ei.Put("CNTRY_DESCP", Prolink.Math.GetValueAsString(sm_dt.Rows[0]["CNTRY_DESCP"]));

                if (party == "SMDNPT")
                {
                    ei.Put("PACKING_FROM", "L");
                    //sql = "SELECT PARTY_TYPE, PARTY_NO, PARTY_NAME, PART_ADDR FROM SMDNPT WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND DN_NO=" + SQLUtils.QuotedStr(dn_no);
                    sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR, 
                                PARTY_ATTN, TEL AS PARTY_TEL, FAX_NO 
                                FROM SMDNPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT','RE', 'AG', 'WE') AND DN_NO=" + SQLUtils.QuotedStr(dn_no);
                }
                else
                {
                    ei.Put("PACKING_FROM", "C");
                    //sql = "SELECT PARTY_TYPE, PARTY_NO, PARTY_NAME, PART_ADDR1 AS PART_ADDR FROM SMSMPT WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                    sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR1,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR, 
                                PARTY_ATTN, PARTY_TEL, FAX_NO 
                                FROM SMSMPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT',  'RE', 'AG', 'WE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(sm_id);
                }
                
                DataTable party_dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (party_dt.Rows.Count > 0)
                {
                    foreach (DataRow rows in party_dt.Rows)
                    {
                        string PartyType = Prolink.Math.GetValueAsString(rows["PARTY_TYPE"]);

                        if (PartyType == "SH")
                        {
                            if (c_com != "")
                            {
                                sql = @"SELECT TOP 1 PARTY_NO, 
                                           CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME, 
                                           CONCAT(PART_ADDR1,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,  
                                           PARTY_ATTN, PARTY_TEL, PARTY_FAX 
                                           FROM SMPTY WHERE PARTY_NO=" + SQLUtils.QuotedStr(c_com) + " AND STATUS='U' AND CMP=" + SQLUtils.QuotedStr(CompanyId);
                                DataTable sh_dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                                if (sh_dt.Rows.Count > 0)
                                {
                                    foreach (DataRow item in sh_dt.Rows)
                                    {
                                        ei.Put("SHPR_CD", Prolink.Math.GetValueAsString(item["PARTY_NO"]));
                                        ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(item["PARTY_NAME"]));
                                        ei.Put("SHPR_ADDR", Prolink.Math.GetValueAsString(item["PART_ADDR"]));
                                        ei.Put("SHPR_ATTN", Prolink.Math.GetValueAsString(item["PARTY_ATTN"]));
                                        ei.Put("SHPR_TEL", Prolink.Math.GetValueAsString(item["PARTY_TEL"]));
                                        ei.Put("SHPR_FAX", Prolink.Math.GetValueAsString(item["PARTY_FAX"]));
                                    }

                                }
                            }
                            else
                            {
                                ei.Put("SHPR_CD", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                                ei.Put("SHPR_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                                ei.Put("SHPR_ADDR", Prolink.Math.GetValueAsString(rows["PART_ADDR"]));
                                ei.Put("SHPR_ATTN", Prolink.Math.GetValueAsString(rows["PARTY_ATTN"]));
                                ei.Put("SHPR_TEL", Prolink.Math.GetValueAsString(rows["PARTY_TEL"]));
                                ei.Put("SHPR_FAX", Prolink.Math.GetValueAsString(rows["FAX_NO"]));
                            }
                        }
                        

                        if (PartyType == "CS")
                        {
                            ei.Put("CNEE_CD", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                            ei.Put("CNEE_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                            ei.Put("CNEE_ADDR", Prolink.Math.GetValueAsString(rows["PART_ADDR"]));
                            ei.Put("CNEE_ATTN", Prolink.Math.GetValueAsString(rows["PARTY_ATTN"]));
                            ei.Put("CNEE_TEL", Prolink.Math.GetValueAsString(rows["PARTY_TEL"]));
                            ei.Put("CNEE_FAX", Prolink.Math.GetValueAsString(rows["FAX_NO"]));
                        }

                        if (PartyType == "NT")
                        {
                            ei.Put("NOTIFY_NO", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                            ei.Put("NOTIFY_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                        }

                        if (PartyType == "RE")
                        {
                            ei.Put("BILL_TO", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                            ei.Put("BILL_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                            ei.Put("BILL_ADDR", Prolink.Math.GetValueAsString(rows["PART_ADDR"]));
                            ei.Put("BILL_ATTN", Prolink.Math.GetValueAsString(rows["PARTY_ATTN"]));
                            ei.Put("BILL_TEL", Prolink.Math.GetValueAsString(rows["PARTY_TEL"]));
                            ei.Put("BILL_FAX", Prolink.Math.GetValueAsString(rows["FAX_NO"]));
                        }

                        if (PartyType == "WE")
                        {
                            ei.Put("SHIP_TO", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                            ei.Put("SHIP_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                            ei.Put("SHIP_ADDR", Prolink.Math.GetValueAsString(rows["PART_ADDR"]));
                            ei.Put("SHIP_ATTN", Prolink.Math.GetValueAsString(rows["PARTY_ATTN"]));
                            ei.Put("SHIP_TEL", Prolink.Math.GetValueAsString(rows["PARTY_TEL"]));
                            ei.Put("SHIP_FAX", Prolink.Math.GetValueAsString(rows["FAX_NO"]));
                        }

                        if (PartyType == "AG")
                        {
                            ei.Put("CUST_CD", Prolink.Math.GetValueAsString(rows["PARTY_NO"]));
                            ei.Put("CUST_NM", Prolink.Math.GetValueAsString(rows["PARTY_NAME"]));
                            ei.Put("CUST_ADDR", Prolink.Math.GetValueAsString(rows["PART_ADDR"]));
                            ei.Put("CUST_ATTN", Prolink.Math.GetValueAsString(rows["PARTY_ATTN"]));
                            ei.Put("CUST_TEL", Prolink.Math.GetValueAsString(rows["PARTY_TEL"]));
                            ei.Put("CUST_FAX", Prolink.Math.GetValueAsString(rows["FAX_NO"]));
                        }
                    }
                }
            }

            if (dn_dt.Rows.Count > 0 && dn_no != "")
            {
                ei.Put("BANK_MSG", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["MEMO"]));
                ei.Put("VAT_NO", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["VAT_NO"]));
                ei.Put("REF_NO", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["DN_NO_CMP_REF"]));
                ei.Put("INV_DATE", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["ACT_POST_DATE"]));
                ei.Put("INCOTERM_DESCP", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["INCOTERM_DESCP"]));
                ei.Put("INCOTERM", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["INCOTERM"]));
                ei.Put("TRADE_TERM", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["TRADE_TERM"]));
                ei.Put("TRADETERM_DESCP", Prolink.Math.GetValueAsString(dn_dt.Rows[0]["TRADETERM_DESCP"]));
            }

            ei.Put("CREATE_BY", UserId);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ml.Add(ei);

            try
            {
                resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return "false";
            }
            
            return uid;
        }
        #endregion

        #region 复制首段DN
        public JsonResult copyFirstDn2Packing()
        {
            string DnNo = Prolink.Math.GetValueAsString(Request.Params["DnNo"]);
            string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);

            List<Dictionary<string, object>> indData = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> pkgData = new List<Dictionary<string, object>>();

            string msg = Business.InvHelper.copyFirstDn2Pkg(DnNo, UId);

            if(msg == "success")
            {
                string sql1 = string.Format("SELECT * FROM SMIND WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
                DataTable dt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                indData = ModelFactory.ToTableJson(dt, "SmIndModel");

                sql1 = string.Format("SELECT * FROM SMINP WHERE U_FID={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(UId));
                DataTable dt2 = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                pkgData = ModelFactory.ToTableJson(dt2, "SmPkgModel");
                return Json(new { message = msg, subData1 = indData, subData2 = pkgData });
            }

            return Json(new {message = msg });
        }
        #endregion

        #region 上传运费
        
        [HttpPost]
        public ActionResult UploadFreightFee(FormCollection form)
        {
            MixedList ml = new MixedList();
            MixedList mx = new MixedList();
            string returnMessage = "success";
            int StartRow = Prolink.Math.GetValueAsInt(Request.Params["StartRow"]);

            //if (StartRow == 0)
            //{
            //    returnMessage = "請輸入啟始行數";
            //    return Json(new { message = returnMessage });
            //}

            List<Dictionary<string, object>> smdnpData = new List<Dictionary<string, object>>();
            if (Request.Files.Count == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                returnMessage = "error";
                return Json(new { message = returnMessage });
            }
            else
            {
                string strExt = System.IO.Path.GetExtension(file.FileName);
                strExt = strExt.Replace(".", "");
                string excelFileName = string.Format("{0}.{1}", Server.MapPath("~/FileUploads/") + DateTime.Now.ToString("yyyyMMddHHmmss"), strExt);
                file.SaveAs(excelFileName);

                DataTable dt = ImportExcelToDataTable(excelFileName, strExt, StartRow);

                EditInstruct ei;
                EditInstruct ei2;
                int[] resulst;
                /*
                 * dr[0]: DN_NO
                 * dr[1]: FREIGHT_FEE
                 */
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ei = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                        string DnNo = Prolink.Math.GetValueAsString(dr[0]);
                        decimal FreightFee = Prolink.Math.GetValueAsDecimal(dr[1]);
                        decimal FobValue = 0;
                        decimal IssueFee = 0;
                        decimal TtlValue = 0;
                        string TradeTerm = string.Empty;
                        DateTime etd = DateTime.Now;
                        string sql = "SELECT ETD, TRADE_TERM, AMOUNT1 FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                        DataTable etdDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        if (etdDt.Rows.Count > 0)
                        {
                            etd = Prolink.Math.GetValueAsDateTime(etdDt.Rows[0]["ETD"]);
                            TradeTerm = Prolink.Math.GetValueAsString(etdDt.Rows[0]["TRADE_TERM"]);
                            TtlValue = Prolink.Math.GetValueAsDecimal(etdDt.Rows[0]["AMOUNT1"]);
                        }
                        string year = etd.ToString("yyyy");
                        string str_tir = getTirForBackend(year);
                        decimal tir = Convert.ToDecimal(str_tir);

                        #region 計算保費
                        IssueFee = TtlValue * Convert.ToDecimal(1.1) * tir;
                        if (TradeTerm != "FOB" && TradeTerm != "FCA" && TradeTerm != "CFR" && TradeTerm != "EXW")
                        {
                            if (IssueFee > 0 && IssueFee <= 1)
                            {
                                IssueFee = 1;
                            }
                            else if (IssueFee > 1)
                            {
                                IssueFee = System.Math.Round(IssueFee);
                            }
                        }
                        else
                        {
                            IssueFee = 0;
                        }

                        switch (TradeTerm)
                        {
                            case "CFR":
                                IssueFee = 0;
                                break;
                            case "CPT":
                                IssueFee = 0;
                                break;
                            case "EXW":
                                IssueFee = 0;
                                break;
                            case "FCA":
                                IssueFee = 0;
                                break;
                            case "FAS":
                                IssueFee = 0;
                                break;
                            case "FH":
                                IssueFee = 0;
                                break;
                            case "FOB":
                                IssueFee = 0;
                                break;
                            default:
                                break;
                        }

                        #endregion

                        if (DnNo != "")
                        {
                            ei.PutKey("DN_NO", DnNo);
                            ei.Put("FREIGHT_FEE", FreightFee);
                            FobValue = TtlValue - FreightFee - IssueFee;
                            ei.Put("ISSUE_FEE", IssueFee);
                            ei.Put("TTL_VALUE", TtlValue);
                            ei.Put("FOB_VALUE", FobValue);
                            ml.Add(ei);
                        }

                        if (i + 1 % 100 == 0)
                        {
                            resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                            ml = new MixedList();
                        }
                        
                    }
                    resulst = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                    return Json(new { message = returnMessage });
                }
            }

            return Json(new { message = returnMessage });
        }
        #endregion

        #region 运费抛SAP
        public JsonResult Fee2Sap()
        {
            string uid = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string[] uids = uid.Trim(',').Split(',');

            string sql = string.Format("SELECT SEND_FRT,DN_NO,SHIPMENT_ID,U_ID FROM SMINM WHERE U_ID IN {0}", SQLUtils.Quoted(uids));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0)
            {
                return Json(new { message = @Resources.Locale.L_InvoiceController_Controllers_164, IsSucceed = false });
            }
            MixedList ml = new MixedList();
            string dnno = string.Empty;
            string message = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                uid = Prolink.Math.GetValueAsString(dr["U_ID"]);
                if (Prolink.Math.GetValueAsString(dr["SEND_FRT"]) == "Y")
                    return Json(new { message =dnno+"："+ @Resources.Locale.L_InvoiceController_Controllers_163, IsSucceed = false });

                if (string.IsNullOrEmpty(dnno))
                    return Json(new { message =dnno+"："+ @Resources.Locale.L_InvoiceController_Controllers_164, IsSucceed = false });

                string sapId = Business.TPV.Helper.GetSapId(CompanyId);
                Business.TPV.Export.FeeManager m = new Business.TPV.Export.FeeManager();
                List<Business.TPV.Export.FeeInfo> items = GetFeeInfo(uid);
                Business.Service.ResultInfo result = m.TryPostFeeInfo(sapId, items);
                if (result == null){
                     return Json(new { message =dnno+"："+ @Resources.Locale.L_InvoiceController_Controllers_166, IsSucceed = false });
                }
                if(!result.IsSucceed){
                    return Json(new { message = dnno + "：" + result.Description + string.Format("(ResultCode:{0})", result.ResultCode), Description = result.Description, IsSucceed = result.IsSucceed, ResultCode = result.ResultCode });
                }
                if (!UpdateSMINM(uid, dr, ml))
                {
                    message += dnno + "：" + @Resources.Locale.L_ActManage_Send + @Resources.Locale.L_SYS_FAIL + "/n";
                }else{
                    message += dnno + ":Send Success;/n";
                }
            }
            return Json(new { message = message, Description = @Resources.Locale.L_InvoiceController_Controllers_165, IsSucceed = false, ResultCode = "Y" });
            //return Json(new { message = @Resources.Locale.L_InvoiceController_Controllers_165 + result.Description, Description = result.Description, IsSucceed = result.IsSucceed, ResultCode = result.ResultCode });
        }

        private bool UpdateSMINM(string uid, DataRow dr,MixedList ml)
        {
            EditInstruct ei = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("U_ID", uid);
            ei.Put("FORDER", "S");
            ei.Put("SEND_FRT", "Y");
            ei.PutDate("SEND_FRT_DATE", DateTime.Now);
            ml.Add(ei);

            ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.PutKey("SHIPMENT_ID", Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
            ei.PutDate("SEND_FRT_DATE", DateTime.Now);
            ml.Add(ei);
            try
            {
                int[] result1 = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

        List<Business.TPV.Export.FeeInfo> GetFeeInfo(string uid)
        {
            string sql = string.Format(@"SELECT SHIPMENT_ID,DN_NO,FREIGHT_FEE,ISSUE_FEE,FOB_VALUE,
             (SELECT TOP 1 BU FROM SMDN WHERE SMDN.DN_NO=SMINM.DN_NO) AS BU FROM SMINM WHERE U_ID={0}", SQLUtils.QuotedStr(uid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt == null || dt.Rows.Count <= 0) return null;
            return dt.Rows.Cast<DataRow>().Select(row => new Business.TPV.Export.FeeInfo
            {
                DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                FreightFee = Prolink.Math.GetValueAsDouble(row["FREIGHT_FEE"]),
                InsuranceFee = Prolink.Math.GetValueAsDouble(row["ISSUE_FEE"]),
                FOBFee = Prolink.Math.GetValueAsDouble(row["FOB_VALUE"]),
                IsPost = true,
                BU = Prolink.Math.GetValueAsString(row["BU"])
            }).Distinct().Where(item => !string.IsNullOrEmpty(item.DNNO)).ToList();
        }

        //批量归档Invoice
        public ActionResult BatchFileToEdoc()
        {
            string returnMsg = "";
            string filetype= Prolink.Math.GetValueAsString(Request.Params["FileType"]);
            if(string.IsNullOrEmpty(filetype))
            {
                 return Json(new { message = @Resources.Locale.L_Invoice_Controllers_389, IsOk = "N" });
            }
            string uid = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            uid = uid.Trim(',');
            string[] uids = uid.Split(',');

            string sql = string.Format("SELECT * FROM SMINM WHERE U_ID IN {0}", SQLUtils.Quoted(uids));
            DataTable inmDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            MixedList ml = new MixedList();
            string shipmentid=string.Empty;
            string jobno = string.Empty;

            string msg = string.Empty;
            EDOCController edo = new EDOCController();
            for (int i = 0; i < inmDt.Rows.Count; i++)
            {
                DataRow dr = inmDt.Rows[i];
                jobno = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string groupID = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                bool Have = edo.CheckEdoc(jobno, groupID, cmp, filetype, "");
                if (Have)
                {
                    string DN = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    string _msg = string.Empty;
                    if (!string.IsNullOrEmpty(shipmentid))
                    {
                        _msg = DN + ":";
                    }
                    else
                        _msg = shipmentid + ":";
                    switch (filetype)
                    {
                        case "PACKO": _msg += @Resources.Locale.L_InvPkgQuery_BATCHFLAGP; break;
                        case "INVO": _msg += @Resources.Locale.L_InvPkgQuery_BATCHFLAGI; break;
                        case "CONTRACT": _msg += @Resources.Locale.L_InvPkgQuery_BATCHFLAGC; break;
                    }
                    msg += _msg + "\n";
                }
            }
            if (!string.IsNullOrEmpty(msg))
                return Json(new { message = msg, IsOk = "N" });
            for (int i = 0; i < inmDt.Rows.Count; i++)
            {
                DataRow dr=inmDt.Rows[i];
                jobno=Prolink.Math.GetValueAsString(dr["U_ID"]);

                DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT 1 FROM SM_AUTOEDOC WHERE JOB_NO={0} AND FILE_TYPE={1} AND (IS_OK!='Y' OR IS_OK IS NULL)", 
                    SQLUtils.QuotedStr(jobno), 
                    SQLUtils.QuotedStr(filetype)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    continue;
                }
                string dnno=Prolink.Math.GetValueAsString(dr["DN_NO"]);
                shipmentid =Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                sql=string.Format("SELECT SHIPMENT_ID,U_ID FROM SMDN WHERE DN_NO={0}",SQLUtils.QuotedStr(dnno));
                DataTable dndt=OperationUtils.GetDataTable(sql,null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if(dndt.Rows.Count==0)
                    continue;
                shipmentid =Prolink.Math.GetValueAsString(dndt.Rows[0]["SHIPMENT_ID"]);
                string dnuid = Prolink.Math.GetValueAsString(dndt.Rows[0]["U_ID"]);
                sql=string.Format("SELECT U_ID FROM SMSM WHERE SHIPMENT_ID=(SELECT TOP 1 SHIPMENT_ID FROM SMDN WHERE DN_NO={0})",SQLUtils.QuotedStr(dnno));
                string smuid = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (!string.IsNullOrEmpty(smuid))
                {
                    if (!"CONTRACT".Equals(filetype))
                    {
                        Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
                        bill.Share(smuid);
                    }
                }

                EditInstruct ei = new EditInstruct("SM_AUTOEDOC", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", Guid.NewGuid().ToString());
                ei.Put("DN_UID",dnuid );
                ei.Put("SHIPMENT_ID", shipmentid);
                ei.Put("FILE_TYPE", filetype);
                ei.Put("GROUP_ID", Prolink.Math.GetValueAsString(dr["GROUP_ID"]));
                ei.Put("CMP", Prolink.Math.GetValueAsString(dr["CMP"]));
                ei.Put("JOB_NO", jobno);
                string[] fileinfo=EdocHelper.GetEdocDescp(filetype);
                ei.Put("REPORT_NAME", fileinfo[1]); 
                ei.Put("REPORT_ID", fileinfo[2]);
                ei.Put("EXPORT_FILE_TYPE", "pdf");
                //ei.Put("ADD_RESON", );
                ei.Put("CREATE_BY", UserId);
                ei.Put("REMARK", "DN NO: " + dnno);
                ei.PutDate("CREATE_DATE", DateTime.Now);
                ml.Add(ei);
                EditInstruct _ei = new EditInstruct("SMINM", EditInstruct.UPDATE_OPERATION);
                _ei.PutKey("U_ID", jobno);
                switch (filetype)
                {
                    case "PACKO": _ei.Put("BATCH_FLAGP", "B"); break;
                    case "INVO": _ei.Put("BATCH_FLAGI", "B"); break;
                    case "CONTRACT": _ei.Put("BATCH_FLAGC", "B"); break;
                }
                ml.Add(_ei);
            }
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, IsOk = "N" });
            }
            return Json(new { message = returnMsg, IsOk = "Y" });
        }

        public ActionResult BatchReloadInvoice()
        {
            string jobnos = Prolink.Math.GetValueAsString(Request.Params["Uid"]);
            jobnos = jobnos.Trim(',');
            string[] uids = jobnos.Split(',');
            string result = string.Empty;
            foreach(string uid in uids){
            EdocHelper edochelper = new EdocHelper();
            result+=edochelper.AutoReloadInvoice(uid);
            }
            return Json(new { message = "Reload Success!", IsOk = "Y" });
        }
    }

}
