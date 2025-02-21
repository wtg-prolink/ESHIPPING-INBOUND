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
using System.Web.Script.Serialization;
using Business;
namespace Business
{
    public class ToInboundHelp
    {

        #region 出口轉進口
        public string O2IFunc(string ShipmentId, DelegateConnection conn)
        {
            return TrackingEDI.InboundBusiness.InboundHelper.O2IFunc(ShipmentId);
        }
        #endregion

        
        #region Inbound Allocation
        public string InboundAllcByShipment(string shipmentid)
        {
            string sql = "SELECT M.U_ID, M.SHIPMENT_ID,M.TRAN_TYPE,M.FRT_TERM,M.INCOTERM_CD,M.INCOTERM_DESCP,D.PARTY_NO AS CONN_CD,M.POD_CD,M.POD_NAME,M.TERMINAL_CD FROM SMSMI M, SMSMIPT D WHERE M.U_ID=D.U_FID AND D.PARTY_TYPE='CS' AND M.SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            DataTable dt = getDataTableFromSql(sql);
            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            return TrackingEDI.InboundBusiness.InboundHelper.InboundAllocation(dt, conn);
        }

        public string InboudAllcBySuid(string uid)
        {
            string sql = "SELECT M.U_ID, M.SHIPMENT_ID,M.TRAN_TYPE,M.FRT_TERM,M.INCOTERM_CD,M.INCOTERM_DESCP,D.PARTY_NO AS CONN_CD,M.POD_CD,M.POD_NAME,M.TERMINAL_CD FROM SMSMI M, SMSMIPT D WHERE M.U_ID=D.U_FID AND D.PARTY_TYPE='CS' AND M.U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable dt = getDataTableFromSql(sql);
            DelegateConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection();
            return TrackingEDI.InboundBusiness.InboundHelper.InboundAllocation(dt, conn);
        }

        #endregion

        #region 同步party檔的IBCR到主檔
        public void SetPartyToIBCR(DataTable mainDt1)
        {
            
            if (mainDt1.Rows.Count > 0)
            {
                string u_id = mainDt1.Rows[0]["U_ID"].ToString();
                string sql1 = "SELECT PARTY_TYPE, PARTY_NO,PARTY_NAME FROM SMSMIPT WHERE PARTY_TYPE='IBCR' AND U_FID=" + SQLUtils.QuotedStr(u_id) + " ORDER BY PARTY_NO ASC";
                DataTable ptdt = getDataTableFromSql(sql1);
                string trantype = mainDt1.Rows[0]["TRAN_TYPE"].ToString();
                string ShipmentId = mainDt1.Rows[0]["SHIPMENT_ID"].ToString();
                string podcd = mainDt1.Rows[0]["POD_CD"].ToString();
                string podnm = mainDt1.Rows[0]["POD_NAME"].ToString();
                string trantype1 = mainDt1.Rows[0]["TRAN_TYPE1"].ToString();
                string pol1 = mainDt1.Rows[0]["POL1"].ToString();
                string polnm1 = mainDt1.Rows[0]["POL_NM1"].ToString();
                string trucker1 = Prolink.Math.GetValueAsString(mainDt1.Rows[0]["TRUCKER1"]);
                string trucker_nm1 = Prolink.Math.GetValueAsString(mainDt1.Rows[0]["TRUCKER_NM1"]);
                if (ptdt.Rows.Count > 0)
                {
                    string PartyNo = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NO"]);
                    string partyNm = Prolink.Math.GetValueAsString(ptdt.Rows[0]["PARTY_NAME"]);

                    EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", u_id);
                    ei.Put("TRUCK_CD", PartyNo);
                    if (string.IsNullOrEmpty(trucker1))
                        ei.Put("TRUCKER1", PartyNo);
                    if (string.IsNullOrEmpty(trucker_nm1))
                        ei.Put("TRUCKER_NM1", partyNm);
                    MixedList mlist = new MixedList();
                    mlist.Add(ei);

                    if ("F".Equals(trantype) || "R".Equals(trantype))
                    {
                        //EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                        //smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
                        //smicntrei.Put("TRUCKER1", PartyNo);
                        //smicntrei.Put("TRUCKER_NM1", partyNm);
                        //smicntrei.Put("TRAN_TYPE1", "T");
                        //smicntrei.Put("POL1", podcd);
                        //smicntrei.Put("POL_NM1", podnm);
                        sql1 = string.Format("UPDATE SMICNTR SET TRUCKER1={0},TRUCKER_NM1={1} WHERE SHIPMENT_ID={2} AND (TRUCKER1 IS NULL OR TRUCKER1='')",
                        SQLUtils.QuotedStr(PartyNo), SQLUtils.QuotedStr(partyNm), SQLUtils.QuotedStr(ShipmentId));
                        mlist.Add(sql1);
                    }

                    try
                    {
                        OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                    }
                    catch (Exception)
                    {
                    }
                }
            }

           
        }
         #endregion

        public void SetPartyToIBCRByShipID(string shipmentid)
        {
            string sql=string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}",SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string Pol1=dt.Rows[0]["POL1"].ToString();
            string cmp=dt.Rows[0]["CMP"].ToString();
            sql = string.Format(@"SELECT BSADDR.ADDR_CODE,BSADDR.ADDR FROM BSADDR,BSTPORT WHERE BSTPORT.Port_Cd=BSADDR.Port_Cd
                AND BSTPORT.Cntry_Cd=BSADDR.Cntry_Cd AND BSTPORT.CMP={0} and BSTPORT.port_cd={1} ORDER BY ADDR_CODE ASC",
                  SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(Pol1));
            DataTable portDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (portDt.Rows.Count > 0)
            {
                string addrcd = portDt.Rows[0]["ADDR_CODE"].ToString();
                string addr = portDt.Rows[0]["ADDR"].ToString();
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR_CD1", addrcd);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());

                ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            SetPartyToIBCR(dt);
        }

        public DataTable getDataTableFromSql(string sql)
        {
            return OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public string getOneValueAsStringFromSql(string sql)
        {
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public int getOneValueAsIntFromSql(string sql)
        {
            return OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }
    }
}