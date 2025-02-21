using System;
using System.Collections.Generic;
using System.Text;
using Prolink.V6.Model;
using Prolink.DataOperation;
using System.Data;
using Prolink.V6.Persistence;
using System.IO;
using System.Net;
using Prolink.Data;
using log4net;
using log4net.Config;
using System.Collections;
using System.Net.Mail;
using System.Threading;
using Prolink.Web;
using NotifyService;
using Prolink.V6.Core;

namespace Task
{
    class Program
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(Program));

        private static Prolink.Log.DefaultLogger _logger = null;
        static void Main(string[] args)
        {
            string xx = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + "reportTitle" + ".xls";
            //@"C:\Users\Public\Documents/reportTitle.xls";

            xx = xx.Substring(xx.LastIndexOf(@"\"));
            Hashtable prop = new Hashtable();
            string serverPath = System.IO.Path.Combine(RuntimeContext.RuntimePath, "../../../publish");
            if (!System.IO.Directory.Exists(serverPath))
                serverPath = System.IO.Path.Combine(RuntimeContext.RuntimePath, "../../../../WebGui");
            prop[WebContext.CONFIGURE_FILE_PATH] = System.IO.Path.Combine(serverPath, "Config/Config.xml");
            prop[WebContext.APP_PATH] = serverPath;

            //Prolink.Web.WebContext.Build(Server.MapPath("~\\Config\\Config.xml"));
            Prolink.Web.WebContext.Build(prop);

            Prolink.V6.Persistence.Context.ConfigurePath = System.IO.Path.Combine(serverPath, "../doc/xml-store/");
            Prolink.V6.Persistence.Factory.Build(System.IO.Path.Combine(serverPath, "../doc/xml-store/db/Company.xml"));
            Prolink.V6.Persistence.Entity.EntityFactory.Build();
            Prolink.V6.Core.SystemManager.Build(Prolink.Web.WebContext.GetInstance());

            prop = new Hashtable();
            prop[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(RuntimeContext.RuntimePath, "Logs/db");
            Prolink.Log.DefaultLogger logger = new Prolink.Log.DefaultLogger(prop);
            Prolink.DataOperation.OperationUtils.Logger = logger;
            try
            {
                //o2i();
                NotifyService.Task.SenASNDateToSAPTask a = new NotifyService.Task.SenASNDateToSAPTask();
                a.Run(null);
                if (args.Length > 0)
                {
                    if (args[0].Equals("notice"))
                    {
                        //doCheckNotice();
                    }

                    if (args[0].Equals("tracking"))
                    {
                        doLoadTrackingXml();
                    }


                    if (args[0].Equals("test"))
                    {
                        doTest();
                    }

                    if (args[0].Equals("o2i"))
                    {
                        o2i();
                    }

                    if (args[0].Equals("SetSmStatus"))
                    {
                        SetSmStatus();
                    }

                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex.Message);
            }
        }

        static void doTest()
        {
        }

        

        static void doLoadTrackingXml()
        {
            //DO USING FTP GET XML FINE AND IMPORT TO TABLE
           
        }

        #region outbound to inbound
        static void o2i()
        {
            DatabaseFactory.Build(Path.Combine(Properties.Settings.Default.PersistanceFolder, "Company.xml"), Path.Combine(Properties.Settings.Default.PersistanceFolder, "ConnectionPool.xml"));
            DelegateConnection conn = DatabaseFactory.GetDatabase("default").GetConnection();
            string sql = "";
            logger.Error("Task start.....");

            string is_ok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc("BQDB2001005181");
            return ;
        }
        #endregion

        #region 判斷inbound shipment是否可為放行
        static void SetSmStatus()
        {
            DatabaseFactory.Build(Path.Combine(Properties.Settings.Default.PersistanceFolder, "Company.xml"), Path.Combine(Properties.Settings.Default.PersistanceFolder, "ConnectionPool.xml"));
            DelegateConnection conn = DatabaseFactory.GetDatabase("default").GetConnection();
            string sql = "";
            logger.Error("Task start.....");

            DateTime Today = DateTime.Now;
            string StrToday_F = Today.ToString("yyyy-MM-dd 00:00:00");
            string StrToday_T = Today.ToString("yyyy-MM-dd 23:59:59");

            #region 掃進口DN檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T));
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);

            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if(n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMIDN WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if(n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='J' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }
                    }
                    
                }
            }
            #endregion

            #region 掃進口貨櫃檔是否可放行的shipment
            sql = "SELECT SHIPMENT_ID, REL_DATE FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} GROUP BY SHIPMENT_ID, REL_DATE";
            sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T));
            dt = OperationUtils.GetDataTable(sql, null, conn);

            if(dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string ShipmentId = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                    sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    int n = OperationUtils.GetValueAsInt(sql, conn);

                    if(n > 0)
                    {
                        sql = "SELECT COUNT(SHIPMENT_ID) FROM SMICNTR WHERE REL_DATE >= {0} AND REL_DATE <= {1} AND SHIPMENT_ID={2}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(StrToday_F), SQLUtils.QuotedStr(StrToday_T), SQLUtils.QuotedStr(ShipmentId));
                        int m = OperationUtils.GetValueAsInt(sql, conn);

                        if(n == m)
                        {
                            sql = "UPDATE SMSMI SET STATUS='J' WHERE SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                            try
                            {
                                OperationUtils.ExecuteUpdate(sql, conn);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }
                    }
                    
                }
            }
            #endregion
        }
        #endregion

        #region 出口轉進口
        public static string O2IFunc(string ShipmentId, DelegateConnection conn, string CompanyId)
        {
            string is_ok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc(ShipmentId);
            return is_ok;
        }
        #endregion

        #region Inbound Allocation
        public static string InboundAllcByShipment(string shipmentid, DelegateConnection conn)
        {
            string sql = "SELECT M.U_ID, M.SHIPMENT_ID,M.TRAN_TYPE,M.FRT_TERM,M.INCOTERM_CD,M.INCOTERM_DESCP,D.PARTY_NO AS CONN_CD,M.POD_CD,M.POD_NAME,M.TERMINAL_CD FROM SMSMI M, SMSMIPT D WHERE M.U_ID=D.U_FID AND D.PARTY_TYPE='CS' AND M.SHIPMENT_ID=" + SQLUtils.QuotedStr(shipmentid);
            DataTable dt = OperationUtils.GetDataTable(sql,null, conn);
            return InboundAllocation(dt, conn);
        }

        public static string InboudAllcBySuid(string uid,DelegateConnection conn)
        {
            string sql = "SELECT M.U_ID, M.SHIPMENT_ID,M.TRAN_TYPE,M.FRT_TERM,M.INCOTERM_CD,M.INCOTERM_DESCP,D.PARTY_NO AS CONN_CD,M.POD_CD,M.POD_NAME,M.TERMINAL_CD FROM SMSMI M, SMSMIPT D WHERE M.U_ID=D.U_FID AND D.PARTY_TYPE='CS' AND M.U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);
            return InboundAllocation(dt, conn);
        }

        public static DataTable getDataTableFromSql(string sql, DelegateConnection conn)
        {
            return OperationUtils.GetDataTable(sql, null, conn);
        }

        public static void SetPartyToIBCRByShipID(string shipmentid, DelegateConnection conn)
        {
            string sql = string.Format("SELECT * FROM SMSMI WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable dt = OperationUtils.GetDataTable(sql, null, conn);
            string Pol1 = dt.Rows[0]["POL1"].ToString();
            string cmp = dt.Rows[0]["CMP"].ToString();
            sql = string.Format(@"SELECT BSADDR.ADDR_CODE,BSADDR.ADDR FROM BSADDR,BSTPORT WHERE BSTPORT.Port_Cd=BSADDR.Port_Cd
                AND BSTPORT.Cntry_Cd=BSADDR.Cntry_Cd AND BSTPORT.CMP={0} and BSTPORT.port_cd={1} ORDER BY ADDR_CODE ASC",
                  SQLUtils.QuotedStr(cmp), SQLUtils.QuotedStr(Pol1));
            DataTable portDt = OperationUtils.GetDataTable(sql, null, conn);
            if (portDt.Rows.Count > 0)
            {
                string addrcd = portDt.Rows[0]["ADDR_CODE"].ToString();
                string addr = portDt.Rows[0]["ADDR"].ToString();
                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR_CD1", addrcd);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, conn);

                ei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("SHIPMENT_ID", shipmentid);
                ei.Put("DEP_ADDR1", addr);
                OperationUtils.ExecuteUpdate(ei, conn);
            }
            SetPartyToIBCR(dt,conn);
        }

        public static void SetPartyToIBCR(DataTable mainDt1,DelegateConnection conn)
        {

            if (mainDt1.Rows.Count > 0)
            {
                string u_id = mainDt1.Rows[0]["U_ID"].ToString();
                string sql1 = "SELECT PARTY_TYPE, PARTY_NO,PARTY_NAME FROM SMSMIPT WHERE PARTY_TYPE='IBCR' AND U_FID=" + SQLUtils.QuotedStr(u_id) + " ORDER BY PARTY_NO ASC";
                DataTable ptdt = getDataTableFromSql(sql1,conn);
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
                        OperationUtils.ExecuteUpdate(mlist, conn);
                    }
                    catch (Exception)
                    {
                    }
                }
            }


        }

        public static string InboundAllocation(DataTable dt,DelegateConnection conn)
        {

            if (dt.Rows.Count > 0)
            {
                string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
                string FrtTerm = Prolink.Math.GetValueAsString(dt.Rows[0]["FRT_TERM"]);
                string ConnCd = Prolink.Math.GetValueAsString(dt.Rows[0]["CONN_CD"]);
                string PodCd = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                string Podnm = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_NAME"]);
                string IncotermCd = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_CD"]);
                string IncotermDescp = Prolink.Math.GetValueAsString(dt.Rows[0]["INCOTERM_DESCP"]);
                string ShipmentId = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                string TerminalCd = Prolink.Math.GetValueAsString(dt.Rows[0]["TERMINAL_CD"]);
                string suid = Prolink.Math.GetValueAsString(dt.Rows[0]["U_ID"]);
                string con = string.Empty;
                string returnMessage = string.Empty;
                if (TerminalCd != "")
                {
                    con = " AND TERMINAL_CD=" + SQLUtils.QuotedStr(TerminalCd);
                }

                string sql = "SELECT LSP_CD, PARTY_TYPE, DLV_AREA, DLV_AREA_NM, ADDR_CODE, DLV_ADDR FROM SMAL WHERE TRAN_TYPE={0} AND FREIGHT_TERM={1} AND CONN_CD={2} AND POD_CD={3} AND INCOTERM_CD={4} AND INCOTERM_DESCP={5}" + con;
                sql = string.Format(sql, SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp));
                DataTable dt1 = getDataTableFromSql(sql,conn);

                if (dt1.Rows.Count > 0)
                {
                    returnMessage = insertParty(dt1, suid, TranType, ShipmentId, PodCd, Podnm,conn);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }

                int k = 0;
                sql = "SELECT LSP_CD, PARTY_TYPE, DLV_AREA, DLV_AREA_NM, ADDR_CODE, DLV_ADDR FROM SMAL WHERE TRAN_TYPE={0} AND FREIGHT_TERM={1} AND (CONN_CD='' OR CONN_CD IS NULL) AND POD_CD={2} AND INCOTERM_CD={3} AND INCOTERM_DESCP={4}" + con;
                sql = string.Format(sql, SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(PodCd), SQLUtils.QuotedStr(IncotermCd), SQLUtils.QuotedStr(IncotermDescp));
                dt1 = getDataTableFromSql(sql,conn);

                if (dt1.Rows.Count > 0)
                {
                    returnMessage = insertParty(dt1, suid, TranType, ShipmentId, PodCd, Podnm,conn);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }

                sql = "SELECT LSP_CD, PARTY_TYPE, DLV_AREA, DLV_AREA_NM, ADDR_CODE, DLV_ADDR FROM SMAL WHERE TRAN_TYPE={0} AND FREIGHT_TERM={1} AND CONN_CD={2} AND POD_CD={3} AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL) AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL) " + con;
                sql = string.Format(sql, SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(ConnCd), SQLUtils.QuotedStr(PodCd));
                dt1 = getDataTableFromSql(sql,conn);

                if (dt1.Rows.Count > 0)
                {
                    returnMessage = insertParty(dt1, suid, TranType, ShipmentId, PodCd, Podnm,conn);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }

                sql = "SELECT LSP_CD, PARTY_TYPE, DLV_AREA, DLV_AREA_NM, ADDR_CODE, DLV_ADDR FROM SMAL WHERE TRAN_TYPE={0} AND FREIGHT_TERM={1} AND (CONN_CD='' OR CONN_CD IS NULL) AND POD_CD={2} AND (INCOTERM_CD='' OR INCOTERM_CD IS NULL)  AND (INCOTERM_DESCP='' OR INCOTERM_DESCP IS NULL)" + con;
                sql = string.Format(sql, SQLUtils.QuotedStr(TranType), SQLUtils.QuotedStr(FrtTerm), SQLUtils.QuotedStr(PodCd));
                dt1 = getDataTableFromSql(sql,conn);

                if (dt1.Rows.Count > 0)
                {
                    returnMessage = insertParty(dt1, suid, TranType, ShipmentId, PodCd, Podnm,conn);

                    if (returnMessage != "success")
                    {
                        return returnMessage;
                    }
                }
            }
            return "success";
        }

        public static int getOneValueAsIntFromSql(string sql)
        {
            DelegateConnection conn = DatabaseFactory.GetDatabase("default").GetConnection();
            return OperationUtils.GetValueAsInt(sql, conn);
        }

        public static string getOneValueAsStringFromSql(string sql,DelegateConnection conn)
        {
            return OperationUtils.GetValueAsString(sql, conn);
        }

        public static string insertParty(DataTable dt1, string suid, string TranType, string ShipmentId, string PodCd, string Podnm,DelegateConnection conn)
        {
            string sql = string.Empty;
            string returnMessage = "success";
            MixedList ml = new MixedList();
            int k = 0;
            if (dt1.Rows.Count > 0)
            {
                foreach (DataRow dr in dt1.Rows)
                {
                    string PartyType = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    //PartyType = PartyType.Remove(PartyType.Length - 1);
                    //string[] p = PartyType.Split(';');
                    string LspCd = Prolink.Math.GetValueAsString(dr["LSP_CD"]);
                    int OrderBy = getOneValueAsIntFromSql("SELECT ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD=" + SQLUtils.QuotedStr(PartyType));
                    string DlvArea = Prolink.Math.GetValueAsString(dr["DLV_AREA"]);
                    string DlvAreaNm = Prolink.Math.GetValueAsString(dr["DLV_AREA_NM"]);
                    string AddrCode = Prolink.Math.GetValueAsString(dr["ADDR_CODE"]);
                    string DlvAddr = Prolink.Math.GetValueAsString(dr["DLV_ADDR"]);

                    //for (int i = 0; i < p.Length; i++)
                    //{
                    //PartyType = p[i];
                    string TypeDescp = getOneValueAsStringFromSql("SELECT CD_DESCP FROM BSCODE WHERE CD_TYPE='PT' AND CD=" + SQLUtils.QuotedStr(PartyType),conn);
                    sql = "SELECT COUNT(*) FROM SMSMIPT WHERE U_FID={0} AND PARTY_TYPE={1}";
                    sql = string.Format(sql, SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(PartyType));

                    int n = getOneValueAsIntFromSql(sql);

                    if (n > 0)
                    {
                        return returnMessage = "Duplicated Party Type, Please check this";
                    }
                    else
                    {
                        #region 很长的sql
                        sql = @"INSERT INTO SMSMIPT
                                    (
                                        U_ID,
                                        U_FID,
                                        SHIPMENT_ID,
                                        PARTY_TYPE,
                                        TYPE_DESCP,
                                        PARTY_NO,
                                        PARTY_NAME,
                                        PARTY_NAME2,
                                        PARTY_NAME3,
                                        PARTY_NAME4,
                                        PARTY_ADDR1,
                                        PARTY_ADDR2,
                                        PARTY_ADDR3,
                                        PARTY_ADDR4,
                                        PARTY_ADDR5,
                                        CNTY,
                                        CNTY_NM,
                                        CITY,
                                        CITY_NM,
                                        STATE,
                                        ZIP,
                                        PARTY_ATTN,
                                        PARTY_TEL,
                                        PARTY_MAIL,
                                        DEBIT_TO,
                                        FAX_NO,
                                        TAX_NO,
                                        ORDER_BY
                                    )
                                    SELECT 
                                    {0},
                                    {1},
                                    {2},
                                    {3},
                                    {4},
                                    PARTY_NO,
                                    PARTY_NAME,
                                    PARTY_NAME2,
                                    PARTY_NAME3,
                                    PARTY_NAME4,
                                    PART_ADDR1,
                                    PART_ADDR2,
                                    PART_ADDR3,
                                    PART_ADDR4,
                                    PART_ADDR5,
                                    CNTY,
                                    CNTY_NM,
                                    CITY,
                                    CITY_NM,
                                    STATE,
                                    ZIP,
                                    PARTY_ATTN,
                                    PARTY_TEL,
                                    PARTY_MAIL,
                                    BILL_TO,
                                    PARTY_FAX,
                                    TAX_NO,
                                    {5}
                                    FROM SMPTY WHERE STATUS='U' AND PARTY_NO={6}";
                        #endregion

                        string ptuid = System.Guid.NewGuid().ToString();
                        sql = string.Format(sql, SQLUtils.QuotedStr(ptuid), SQLUtils.QuotedStr(suid), SQLUtils.QuotedStr(ShipmentId), SQLUtils.QuotedStr(PartyType), SQLUtils.QuotedStr(TypeDescp), OrderBy, SQLUtils.QuotedStr(LspCd));
                        ml.Add(sql);

                        if ("IBCR".Equals(PartyType))
                        {
                            string sql1 = string.Format("SELECT * FROM SMPTY WHERE STATUS='U' AND PARTY_NO={0}", SQLUtils.QuotedStr(LspCd));
                            DataTable smpty = getDataTableFromSql(sql1,conn);
                            if (smpty.Rows.Count > 0)
                            {
                                string lspnm = smpty.Rows[0]["PARTY_NAME"].ToString();
                                EditInstruct ei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                                ei.PutKey("U_ID", suid);
                                ei.Put("TRUCKER1", LspCd);
                                ei.Put("TRUCKER_NM1", lspnm);
                                ml.Add(ei);
                                if ("F".Equals(TranType) || "R".Equals(TranType))
                                {
                                    EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                                    smicntrei.PutKey("SHIPMENT_ID", ShipmentId);
                                    smicntrei.Put("TRUCKER1", LspCd);
                                    smicntrei.Put("TRUCKER_NM1", lspnm);
                                    smicntrei.Put("TRAN_TYPE1", "T");
                                    smicntrei.Put("POL1", PodCd);
                                    smicntrei.Put("POL_NM1", Podnm);
                                    ml.Add(smicntrei);
                                }
                            }
                        }

                        sql = "UPDATE SMIDN SET DLV_AREA={0}, DLV_AREA_NM={1}, ADDR_CODE={2}, DLV_ADDR={3} WHERE SHIPMENT_ID={4}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(DlvAreaNm), SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(DlvAddr), SQLUtils.QuotedStr(ShipmentId));
                        ml.Add(sql);

                        sql = "UPDATE SMICNTR SET DLV_AREA={0}, DLV_AREA_NM={1}, ADDR_CODE={2}, DLV_ADDR={3} WHERE SHIPMENT_ID={4}";
                        sql = string.Format(sql, SQLUtils.QuotedStr(DlvArea), SQLUtils.QuotedStr(DlvAreaNm), SQLUtils.QuotedStr(AddrCode), SQLUtils.QuotedStr(DlvAddr), SQLUtils.QuotedStr(ShipmentId));
                        ml.Add(sql);

                        k++;
                    }
                    // }


                }
            }

            if (k > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(ml, conn);
                }
                catch (Exception ex)
                {
                    returnMessage = "error";
                }
            }

            return returnMessage;
        }
        #endregion

    }
}
