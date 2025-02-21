using Prolink;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using TrackingEDI.Utils;

namespace TrackingEDI.Business
{
    public class InboundUploadExcelManager
    {
        #region 注册类型
        /// <summary>
        /// 确认Air Booking Status：代码AIRBSMapping
        /// </summary>
        /// 
        public static string InboundSCMInfoMapping = "InboundSCMInfoMapping";

        /// <summary>
        /// SCM Request Date By Model 导入方式注册IbSCMModelInfoMapping
        /// </summary>
        public static string IbSCMModelInfoMapping = "IbSCMModelInfoMapping";

        /// <summary>
        /// 导入PortFree Mapping
        /// </summary>
        public static string InboundPortFreeMapping = "InboundPortFreeMapping";

        /// <summary>
        /// 导入Create Appoint Mapping
        /// </summary>
        public static string InboundCreateAppointMapping = "InboundCreateAppointMapping";

        public static string InboundSMSMIMapping = "InboundSMSMIMapping";

        public static string SmicuftMapping = "SmicuftMapping";
        #endregion

        public static DataTable CarrierDataTable = new DataTable();
        public static DataTable CityDataTable = new DataTable();
        public static DataTable SmptyDataTable = new DataTable();
        public static DataTable DestMapDataTable = new DataTable();
        public static DataTable DirectMapDataTable = new DataTable();

        public static void InitCarrierData()
        {
            string sql = "SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE = 'TCAR' ";// (CMP={0} OR CMP='*') AND GROUP_ID={1} ";
            CarrierDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void InitCityData()
        {
            string sql = "SELECT CNTRY_CD, PORT_CD, PORT_NM,PORT_NM2 FROM BSCITY WITH(NOLOCK)";
            CityDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void InitSmptyData()
        {
            string sql = "SELECT PARTY_NO,PART_ADDR1,PARTY_NAME,PARTY_NAME,DEP FROM SMPTY WITH(NOLOCK) ";
            SmptyDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static void InitDestMapData()
        {
            string sql = "SELECT * FROM DEST_MAP";// CMP={0} AND GROUP_ID={1} ";
            DestMapDataTable = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public static string HandleSCMInfoExcel(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            List<string> shipmentids = (List<string>)parm["shipmentlist"];

            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = ei.Get("SHIPMENT_ID");
            if (string.IsNullOrEmpty(shipmentid))
            {
                return BaseParser.ERROR;
            }
            string trantype = ei.Get("TRAN_TYPE");
            ei.PutKey("SHIPMENT_ID", shipmentid);
            string cntrno = ei.Get("CNTR_NO");
            string invno = ei.Get("INV_NO");
            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            if (!shipmentids.Contains(shipmentid))
            {
                shipmentids.Add(shipmentid);
            }
            parm["shipmentlist"] = shipmentids;
            string priority = Prolink.Math.GetValueAsString(ei.Get("PRIORITY"));
            if (!string.IsNullOrEmpty(priority))
            {
                string condition = parm["condition"].ToString();
                string sql = string.Format("SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PRI' AND {0} AND  CD='{1}'", condition, priority);
                DataTable bsdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (bsdt.Rows.Count <= 0)
                {
                    throw new Exception(string.Format("The Excel  has a wrong priority where Shipment is {0} ", shipmentid));
                }
            }
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                //加入container
                EditInstruct cntrei = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                cntrei.PutKey("SHIPMENT_ID", shipmentid);
                cntrei.PutKey("CNTR_NO", cntrno);
                if (!string.IsNullOrEmpty(ei.Get("SCMREQUEST_DATE").ToString()))
                {
                    cntrei.PutDate("SCMREQUEST_DATE", ei.Get("SCMREQUEST_DATE").ToString());
                }
                if (!string.IsNullOrEmpty(priority))
                {
                    cntrei.Put("PRIORITY", priority);
                }
                ml.Add(cntrei);
            }
            //加入Invoice dn
            EditInstruct idnei = new EditInstruct("SMIDN", EditInstruct.UPDATE_OPERATION);
            idnei.PutKey("SHIPMENT_ID", shipmentid);
            idnei.PutKey("INV_NO", invno);
            if (!string.IsNullOrEmpty(ei.Get("SCMREQUEST_DATE").ToString()))
            {
                idnei.PutDate("SCMREQUEST_DATE", ei.Get("SCMREQUEST_DATE").ToString());
            }
            if (!string.IsNullOrEmpty(priority))
            {
                idnei.Put("PRIORITY", priority);
            }
            ml.Add(idnei);
            string inboundpono = Prolink.Math.GetValueAsString(ei.Get("INBOUND_PO_NO"));
            if (!string.IsNullOrEmpty(inboundpono))
            {
                EditInstruct msei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                msei.PutKey("SHIPMENT_ID", shipmentid);
                msei.Put("INBOUND_PO_NO", inboundpono);
                ml.Add(msei);
            }
            return string.Empty;
        }

        public static string HandleSCMInfoByModel(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            List<string> shipmentids = (List<string>)parm["shipmentlist"];
            ei.OperationType = EditInstruct.UPDATE_OPERATION;
            string shipmentid = Prolink.Math.GetValueAsString(ei.Get("SHIPMENT_ID"));
            if (string.IsNullOrEmpty(shipmentid))
            {
                return BaseParser.ERROR;
            }
            string trantype = Prolink.Math.GetValueAsString(ei.Get("HTML_TRAN_TYPE"));
            string cntrno = Prolink.Math.GetValueAsString(ei.Get("CNTR_NO"));
            string invno = Prolink.Math.GetValueAsString(ei.Get("INV_NO"));
            string partno = Prolink.Math.GetValueAsString(ei.Get("PART_NO"));
            int Qty = Prolink.Math.GetValueAsInt(ei.Get("QTY"));
            ei.PutKey("SHIPMENT_ID", shipmentid);
            ei.PutKey("QTY", Qty);
            if (!string.IsNullOrEmpty(partno))
            {
                ei.PutKey("PART_NO", partno);
            }
            if (!string.IsNullOrEmpty(cntrno))
            {
                ei.PutKey("CNTR_NO", cntrno);
            }
            if (!string.IsNullOrEmpty(invno))
            {
                ei.PutKey("INV_NO", invno);
            }

            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            if (!shipmentids.Contains(shipmentid))
            {
                shipmentids.Add(shipmentid);
            }
            parm["shipmentlist"] = shipmentids;


            List<string> cntrshipments = (List<string>)parm["cntrshipment"];
            List<string> dnshipments = (List<string>)parm["dnshipment"];
            if ("FCL".Equals(trantype.ToUpper()) || "RAILWAY".Equals(trantype.ToUpper()))
            {
                if (!cntrshipments.Contains(shipmentid))
                {
                    cntrshipments.Add(shipmentid);
                }
            }
            else
            {
                if (!dnshipments.Contains(shipmentid))
                {
                    dnshipments.Add(shipmentid);
                }
            }

            string priority = Prolink.Math.GetValueAsString(ei.Get("PRIORITY"));
            if (!string.IsNullOrEmpty(priority))
            {
                string condition = parm["condition"].ToString();
                string sql = string.Format("SELECT CD,CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PRI' AND {0} AND  CD='{1}'", condition, priority);
                DataTable bsdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (bsdt.Rows.Count <= 0)
                {
                    throw new Exception(string.Format("The Excel  has a wrong priority where Shipment is {0} ", shipmentid));
                }
            }
            string[] fileds = ei.getNameSet();
            foreach (string filed in fileds)
            {
                switch (filed)
                {
                    case "SCMREQUEST_DATE":
                    case "PRIORITY":
                    case "SHIPMENT_ID":
                    case "QTY":
                        break;
                    case "PART_NO":
                        if(string.IsNullOrEmpty(partno))
                            ei.Remove(filed);
                        break;
                    case "CNTR_NO":
                        if (string.IsNullOrEmpty(cntrno))
                            ei.Remove(filed);
                        break;
                    case "INV_NO":
                        if (string.IsNullOrEmpty(invno))
                            ei.Remove(filed);
                        break;
                    default:
                        ei.Remove(filed);
                        break;
                }
            }
            return string.Empty;
        }

        public static string HandlePortFreeExcel(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            string groupid = Prolink.Math.GetValueAsString(parm["groupid"]);
            string cmp= Prolink.Math.GetValueAsString(parm["companyid"]);
            string station = Prolink.Math.GetValueAsString(parm["station"]);
            string dep = Prolink.Math.GetValueAsString(parm["dep"]);
            string userId = Prolink.Math.GetValueAsString(parm["userId"]);
            ei.OperationType = EditInstruct.INSERT_OPERATION;
            string cnttype = ei.Get("CNT_TYPE");
            string tranType = Prolink.Math.GetValueAsString(ei.Get("TRAN_TYPE"));
            string calDate = Prolink.Math.GetValueAsString(ei.Get("HTML_CAL_DATE"));
            if (string.IsNullOrEmpty(cnttype) || string.IsNullOrEmpty(tranType))
            {
                return BaseParser.ERROR;
            }

            string sql = string.Format(@"SELECT CHG_DESCP FROM ECREFFEE WHERE CHG_CD={0}", SQLUtils.QuotedStr(cnttype));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count > 0)
            {
                ei.Put("CNT_DESCP", dt.Rows[0]["CHG_DESCP"]);
            }
            else
            {
                throw new Exception("Container Type Name does not exist in the Tons Setup"); 
            }

            ei.Put("U_ID", System.Guid.NewGuid().ToString());
            ei.Put("GROUP_ID", groupid);
            ei.Put("CMP", cmp);
            ei.Put("STN", station);
            ei.Put("DEP", dep);
            ei.Put("CREATE_BY", userId);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            if (calDate.Equals("ATD"))
                calDate = "D";
            else if (calDate.Equals("ATA"))
                calDate = "A";
            ei.Remove("HTML_CAL_DATE");
            ei.Put("CAL_DATE", calDate);
            string lspNo = ei.Get("LSP_NO");
            sql = string.Format(@"SELECT PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(lspNo));
            string partyName = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            ei.Put("LSP_NM", partyName); 
            return string.Empty;
        }

        public static string HandleCreateAppoint(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            string CompanyId=Prolink.Math.GetValueAsString(parm["CompanyId"]);
            string GroupId=Prolink.Math.GetValueAsString(parm["GroupId"]);
            string UserId=Prolink.Math.GetValueAsString(parm["UserId"]);
            string trantype = ei.Get("TRAN_TYPE");
            if (string.IsNullOrEmpty(trantype))
            {
                return BaseParser.ERROR;
            }
            string cntrno = ei.Get("CNTR_NO");
            if ("F".Equals(trantype)||"R".Equals(trantype))
            {
                if (string.IsNullOrEmpty(cntrno))
                    throw new Exception("Container No Must Be Entered When TranType Is FCL OR Railway !");
            }
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            string ruleCode = "SHIB_NO";
            hash.Add("CMP", CompanyId);
            string shipmentid = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");

            //产生预约号
            hash = new System.Collections.Hashtable();
            ruleCode="RV_NO";
            hash.Add("CMP", CompanyId);
            string ReserveNo = AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");

            string usedate = ei.Get("USE_DATE");
            string arrivaldate = ei.Get("ARRIVALTIMESTRING");
            //处理arrivaldate  an Time
            string arrivalstring = string.Empty;
            int time = 0;
            try
            {
                arrivalstring = arrivaldate.Substring(0, 8);
                time = Prolink.Math.GetValueAsInt(arrivaldate.Substring(8, 2));
            }
            catch (Exception ex)
            {
                throw new Exception(arrivaldate+ " Format error ! The Rright Format like this (yyyyMMddhhmm)");
            }
            ei.PutDate("RESERVE_DATE",arrivalstring);//日期部分
            ei.Put("RESERVE_FROM",time);//小时部分
            string wscd = ei.Get("WS_CD");
            string invoiceno = ei.Get("INV_NO");
            string shippernm= ei.Get("SHIPPER_NM");
            string shipper = string.Empty;
            
            ei.OperationType = EditInstruct.INSERT_OPERATION;
            ei.Put("TRS_MODE","Y");
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("GROUP_ID", GroupId);
            ei.Put("CMP", CompanyId);
            ei.Put("CREATE_BY", UserId);
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ei.Put("SHIPMENT_INFO", shipmentid);
            ei.Put("STATUS","R");
            ei.Put("RV_TYPE", "I");
            ei.Put("DN_NO", invoiceno);
            ei.Put("RESERVE_NO", ReserveNo);
            ei.Put("LTRUCK_NO", ei.Get("TRUCK_NO"));
            ei.Put("LDRIVER", ei.Get("DRIVER"));
            ei.Put("LDRIVER_ID", ei.Get("DRIVER_ID"));
            ei.Put("LTEL", ei.Get("TEL"));
            ei.Put("IS_AUTOCREATE", "Y");   //是否是自动创建的Appointment  Y为是
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                ei.Put("CALL_TYPE", "C");
            }
            else
            {
                ei.Put("CALL_TYPE", "D");
            }
            //产生ShipmentId
            
            if (string.IsNullOrEmpty(shipmentid)) return BaseParser.ERROR;
            if (string.IsNullOrEmpty(ReserveNo)) return BaseParser.ERROR;
            TrackingEDI.InboundBusiness.SMSMIHelper.setReserveValue(shipmentid, ei);
            TrackingEDI.InboundBusiness.SMSMIHelper.CreateSubData(trantype, shipmentid, ReserveNo, ml, invoiceno, wscd, cntrno, GroupId, CompanyId, shippernm, shipper,UserId);
            ei.Put("SHIPPER", shippernm);
            ei.Remove("INV_NO");
            ei.Remove("SHIPPER_NM");
            ei.Remove("ARRIVALTIMESTRING");
            return string.Empty;
        }

        public static string HandleCreateSMSMIInfo(DataRow dr, EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            string CompanyId = ei.Get("SMSMI_CMP");
            string masterno = ei.Get("SMSMI_MASTER_NO");
            string frtTerm = ei.Get("SMSMI_FRT_TERM");

            string ruslut = CheckResult(ei);
            if (!string.IsNullOrEmpty(ruslut))
            {
                throw new Exception("Master No :" + masterno + ruslut);
            }

            if (frtTerm.Length >= 1)
            {
                frtTerm = frtTerm.Substring(0, 1).ToUpper();
            }
            if (!string.IsNullOrEmpty(frtTerm))
            {
                if (!"C".Equals(frtTerm) && !"P".Equals(frtTerm) && !"O".Equals(frtTerm))
                    throw new Exception("Master No :" + masterno + ",the Freight Term incorrect,please check!");
            }
            string CntrTypeTemp = ei.Get("SMICNTR_CNTR_TYPE");
            if (!string.IsNullOrEmpty(CntrTypeTemp))
            {
                string sql = string.Format(@"SELECT COUNT(*) FROM BSCODE WHERE CD_TYPE='TCNT' AND CD_DESCP={0} AND 
                (CMP='*' OR CMP={1})", SQLUtils.QuotedStr(CntrTypeTemp), SQLUtils.QuotedStr(CompanyId));
                int cntrcount = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (cntrcount <= 0)
                    throw new Exception("Master No :" + masterno + " ContainerType:" + CntrTypeTemp + ", the container type does not exist in the Master Data");
            }

            CreateSMSMIMasterinfo(ei, parm);
            CreateSMIDNInfo(ei, parm);
            Dictionary<string, string> masternoDic = (Dictionary<string, string>)parm["masternoDic"];
            Dictionary<string, string> invnoDic = (Dictionary<string, string>)parm["invnoDic"];
            Dictionary<string, string> cntrDic = (Dictionary<string, string>)parm["cntrDic"];
            Dictionary<string, ShipmentFreeInfo> freeDic = (Dictionary<string, ShipmentFreeInfo>)parm["freeDic"];
            string shipmentid = masternoDic[masterno];
            string invoiceno = ei.Get("SMIDN_INV_NO");
            string ipartno = ei.Get("IPART_NO");
            string trantype = ei.Get("SMSMI_TRAN_TYPE"); 
            string cntrno = ei.Get("CNTR_NO");
            string shipCntr = shipmentid + "+" + cntrno;

            if ("E".Equals(trantype) && "TP".Equals(CompanyId))
            {
                parm["dnDimensionFlag"] = true;
            }

            if ("R".Equals(trantype) )
            {
                string carrier = ei.Get("SMSMI_CARRIER");
                string sql = string.Format(@"SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='RCAR' AND CD={0}", SQLUtils.QuotedStr(carrier));
                DataTable bscodedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (bscodedt.Rows.Count <= 0)
                    throw new Exception("Master No :" + masterno + ", the Rail Carrier Code " + carrier + " is not exist in Master Data.");
            } 

            if (!string.IsNullOrEmpty(cntrno) && !cntrDic.Keys.Contains(shipCntr))
            {
                string CntrType = ei.Get("SMICNTR_CNTR_TYPE");
                string SealNo1 = ei.Get("SMICNTR_SEAL_NO1");
                EditInstruct smicntrei = new EditInstruct("SMICNTR", EditInstruct.INSERT_OPERATION);
                smicntrei.Put("U_ID", System.Guid.NewGuid().ToString());
                smicntrei.Put("CMP", CompanyId);
                smicntrei.Put("SHIPMENT_ID", shipmentid);
                smicntrei.Put("DN_NO", invoiceno); 
                smicntrei.Put("CNTR_NO", cntrno);
                smicntrei.Put("CNTR_TYPE", CntrType);
                smicntrei.Put("SEAL_NO1", SealNo1);
                smicntrei.Put("CMP", CompanyId);
                string ConsigneeNo = ei.Get("HTML_CS");
                string sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(ConsigneeNo));
                string dnimportno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                smicntrei.Put("IMPORT_NO", dnimportno);
                smicntrei.Put("TC_IMPORT_NO", dnimportno);

                string podcd = ei.Get("SMSMI_POD_CD");
                string podname = GetTableValueByName(CityDataTable, string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(podcd.Substring(0, 2)), SQLUtils.QuotedStr(podcd.Substring(2, 3))), "PORT_NM");
                if (trantype == "F" || trantype == "R")
                {
                    smicntrei.Put("INV_NO", invoiceno);
                    smicntrei.Put("DIVISION_DESCP", ei.Get("SMIDN_DIVISION_DESCP"));
                    smicntrei.Put("TRAN_TYPE1", "T");
                    smicntrei.Put("POL1", podcd);
                    smicntrei.Put("POL_NM1", podname);
                    smicntrei.Put("PRIORITY", "2");
                    smicntrei.Put("BACK_LOCATION", podcd);
                    if (freeDic.ContainsKey(shipmentid))
                    {
                        smicntrei.PutDate("STORAGE_DUE_DATE", freeDic[shipmentid].StorageDueDate);
                        smicntrei.Put("CON_CHG_TYPE", freeDic[shipmentid].CON_CHG_TYPE);
                        smicntrei.PutDate("DEMURRAGE_DUE_DATE", freeDic[shipmentid].DemurrageDueDate);
                        smicntrei.Put("PORT_CHG_TYPE", freeDic[shipmentid].PORT_CHG_TYPE);
                    }
                }
                smicntrei.Put("CNTRY_CD", podcd.Substring(0, 2));
                smicntrei.Put("TC_CNTRY_CD", podcd.Substring(0, 2));
                ml.Add(smicntrei);
            }
            if (!cntrDic.Keys.Contains(shipCntr))
            {
                cntrDic.Add(shipCntr, invoiceno);
            }
            else
            {
                string strDnNo = cntrDic[shipCntr];
                if (!strDnNo.Contains(invoiceno) && !string.IsNullOrEmpty(cntrno))
                {
                    string newStrDnNo = strDnNo + "," + invoiceno;
                    EditInstruct updatesmicntr = new EditInstruct("SMICNTR", EditInstruct.UPDATE_OPERATION);
                    updatesmicntr.PutKey("SHIPMENT_ID", shipmentid);
                    updatesmicntr.PutKey("CNTR_NO", cntrno);
                    updatesmicntr.Put("DN_NO", newStrDnNo);
                    if (trantype == "F" || trantype == "R")
                    {
                        updatesmicntr.Put("INV_NO", newStrDnNo);
                    }
                    ml.Add(updatesmicntr);
                    cntrDic.Remove(shipCntr);
                    cntrDic.Add(shipCntr, newStrDnNo);
                }
            }

            EditInstruct idnpei = new EditInstruct("SMIDNP", EditInstruct.INSERT_OPERATION);
            string Dnp_UId = System.Guid.NewGuid().ToString();
            idnpei.Put("U_ID", Dnp_UId);
            idnpei.Put("U_FID", invnoDic[invoiceno]);
            idnpei.Put("SHIPMENT_ID", shipmentid);
            idnpei.Put("DN_NO", invoiceno);
            idnpei.Put("INV_NO", invoiceno);
            idnpei.Put("IPART_NO", ipartno);
            idnpei.Put("CATEGORY", "TAN");
            idnpei.Put("NEW_CATEGORY", "TAN");
            string opartno = ei.Get("OPART_NO");
            if (!string.IsNullOrEmpty(opartno))
            {
                idnpei.Put("PART_NO", opartno);
            }
            idnpei.Put("OPART_NO", opartno);
            if (trantype == "F" || trantype == "R")
            {
                idnpei.Put("CNTR_NO", cntrno);
            }
            int dmpqty = Prolink.Math.GetValueAsInt(ei.Get("QTY"));
            idnpei.Put("QTY", dmpqty);
            idnpei.Put("GW", ei.Get("GW"));
            idnpei.Put("CBM", ei.Get("CBM"));
            idnpei.Put("PO_NO", ei.Get("PO_NO"));
            string asnNo = Prolink.Math.GetValueAsString(idnpei.Get("ASN_NO"));
            if (!string.IsNullOrEmpty(asnNo))
                AddEDIInfoToDB(ml, asnNo, CompanyId, "Upload Shipment Mapping");
            ml.Add(idnpei);


            string uid = Guid.NewGuid().ToString();
            EditInstruct sminmei = new EditInstruct("SMINM", EditInstruct.INSERT_OPERATION);
            sminmei.Put("U_ID", uid);
            sminmei.Put("DN_NO", invoiceno);
            sminmei.Put("INV_NO", invoiceno);
            sminmei.Put("INVOICE_TYPE", "I");
            sminmei.Put("SHIPMENT_ID", shipmentid);
            sminmei.Put("CMP", CompanyId);
            sminmei.Put("GROUP_ID", "TPV");
            sminmei.PutDate("CREATE_DATE", TimeZoneHelper.GetTimeZoneDate(DateTime.Now, CompanyId));
            ml.Add(sminmei);
            
            string sminpoSql = string.Format(@"SELECT U_ID FROM SMINPO WHERE INVOICE_TYPE='I' AND U_FID IS NULL AND INV_NO={0}", SQLUtils.QuotedStr(invoiceno));
            DataTable dt = OperationUtils.GetDataTable(sminpoSql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow row in dt.Rows)
            {
                string poUid = Prolink.Math.GetValueAsString(row["U_ID"]);
                EditInstruct sminpoei = new EditInstruct("SMINPO", EditInstruct.UPDATE_OPERATION);
                sminpoei.PutKey("U_ID", poUid);
                sminpoei.Put("U_FID", uid);
                ml.Add(sminpoei);
            } 
            return BaseParser.ERROR;
        }
        public static string CheckResult(EditInstruct ei)
        {
            string result = string.Empty;
            Dictionary<string, string> strList = new Dictionary<string, string>();
            strList.Add("SMSMI_CNT20", "Container 20");
            strList.Add("SMSMI_CNT40", "Container 40");
            strList.Add("SMSMI_CNT40HQ", "Container 40HQ");
            strList.Add("SMSMI_CNT_NUMBER", "Other Container Number");
            strList.Add("SMSMI_PKG_NUM", "TTL PackageNo");
            strList.Add("SMSMI_GW", "TTL GW");
            strList.Add("SMSMI_VW", "TTL VW");
            strList.Add("SMSMI_CBM", "TTL CBM");
            strList.Add("SMSMI_CW", "TTL CW");
            strList.Add("SMIDN_INV_PRICE", "Inv_Unit_Price");
            strList.Add("SMIDN_NW", "Inv_NW");
            strList.Add("SMIDN_GW", "Inv_GW");
            strList.Add("SMIDN_CBM", "Inv_CBM");
            strList.Add("SMIDN_QTY", "Inv_Qty");
            strList.Add("SMIDN_PKG_NUM", "Inv_PKG");
            strList.Add("QTY", "Q'ty/ModelName");
            strList.Add("GW", "GW/ModelName");
            strList.Add("CBM", "CBM/ModelName");
            List<string> checkResult=new List<string>();
            foreach (KeyValuePair<string, string> kvp in strList)
            {
                string value = ei.Get(kvp.Key);
                if (!IsNum(value))
                    checkResult.Add(" the " + kvp.Value + " must be numeric!");
            }
            if (checkResult.Count>0)
            {
                result= string.Join(Environment.NewLine, checkResult);
            }
            return result;
        }
        public static bool IsNum(string str, bool point = true)
        {
            for (int i = 0; i < str.Length; i++)
            {
                byte b = Convert.ToByte(str[i]);
                if (b < 48 || b > 57)
                {
                    if (b == 46 && point)
                    {
                        point = false;
                        continue;
                    }
                    else
                        return false;
                }
            }
            return true;
        }

        public static void AddEDIInfoToDB(MixedList ml, string asnNo, string cmp,string remark)
        {
            try
            {
                EditInstruct ei = new EditInstruct("EDI_LOG", Prolink.DataOperation.EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("EDI_ID", "ASNFTP");
                ei.PutExpress("EVENT_DATE", "getdate()");
                ei.Put("REMARK", remark);
                ei.Put("SENDER", "Auto System");
                ei.Put("RS", "Receive");
                ei.Put("STATUS", "Succeed");
                ei.Put("FROM_CD", "ftp");
                ei.Put("TO_CD", "Eshipping");
                ei.Put("DATA_FOLDER", "");
                ei.Put("REF_NO", asnNo);
                ei.Put("GROUP_ID", "TPV");
                ei.Put("CMP", cmp);
                ei.Put("STN", "*");
                if (ml != null)
                {
                    ml.Add(ei);
                }
            }
            catch (Exception e) { }
        }

        private static void CreateSMIDNInfo(EditInstruct ei, Dictionary<string, object> parm)
        {
            MixedList ml = (MixedList)parm["mixedlist"];
            List<string> shipmentList = (List<string>)parm["shipmentList"];
            string CompanyId = ei.Get("SMSMI_CMP");
            string GroupId = Prolink.Math.GetValueAsString(parm["GroupId"]);
            string invno = ei.Get("SMIDN_INV_NO");
            Dictionary<string, string> invnoDic = (Dictionary<string, string>)parm["invnoDic"];
            Dictionary<string, string> masternoDic = (Dictionary<string, string>)parm["masternoDic"];
            string masterno = ei.Get("SMSMI_MASTER_NO");
            if (invnoDic.Keys.Contains(invno))
            {
                return;
            }
            string shipmentid = masternoDic[masterno];
            string[] names = ei.getNameSet();
            List<string> smidnfields = new List<string>();
            foreach (string fildname in names)
            {
                if (fildname.StartsWith("SMIDN_"))
                {
                    smidnfields.Add(fildname.Replace("SMIDN_", ""));
                }
            }

            #region 
            DataTable invDt = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT SMIDN.SHIPMENT_ID,SMIDN.INV_NO FROM SMIDN WHERE INV_NO = {0})T OUTER APPLY (SELECT MASTER_NO,STATUS FROM SMSMI WITH (NOLOCK) WHERE SMSMI.SHIPMENT_ID=T.SHIPMENT_ID)M", SQLUtils.QuotedStr(invno)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(invno) && !string.IsNullOrEmpty(masterno))
            {
                DataRow[] drs = invDt.Select(string.Format("INV_NO={0}", SQLUtils.QuotedStr(invno)));
                if (drs.Length > 0)
                {
                    DataRow[] drs1 = invDt.Select(string.Format("INV_NO={0} AND MASTER_NO={1}", SQLUtils.QuotedStr(invno), SQLUtils.QuotedStr(masterno)));
                    if (drs1.Length <= 0)
                        throw new Exception(string.Format("{0} is used by mbl({1}),import faild!", invno, drs[0]["MASTER_NO"]));
                }
            }

            #endregion

            EditInstruct smidnei = new EditInstruct("SMIDN", EditInstruct.INSERT_OPERATION);
            string Dn_UId = System.Guid.NewGuid().ToString();
            smidnei.Put("U_ID", Dn_UId);
            smidnei.Put("GROUP_ID", GroupId);
            smidnei.Put("CMP", CompanyId);
            smidnei.Put("STN", "*");
            smidnei.Put("DEP", "*");
            smidnei.Put("SHIPMENT_ID", shipmentid);
            smidnei.Put("DN_NO", invno);
            Func<string, EditInstruct, bool> setsmidnFiledvalue = (field, funcei) =>
            {
                string gvalue = "";
                try
                {
                    gvalue = ei.Get("SMIDN_" + field);
                }
                catch (Exception e) { return false; }
                funcei.Put(field, gvalue);
                return true;
            };
            foreach (string smidnfiled in smidnfields)
            {
                setsmidnFiledvalue(smidnfiled, smidnei);
            }
            string trantype = ei.Get("SMSMI_TRAN_TYPE");
            string podcd = ei.Get("SMSMI_POD_CD");
            string PodCnty = podcd.Substring(0, 2);
            string ConsigneeNo = ei.Get("HTML_CS");
            string sql = string.Format(@"SELECT TOP 1 TAX_NO FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(ConsigneeNo));
            string dnimportno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            smidnei.Put("IMPORT_NO", dnimportno);
            smidnei.Put("TC_IMPORT_NO", dnimportno);
            if (trantype == "A" || trantype == "L" || trantype == "E")
            {
                smidnei.Put("DIVISION_DESCP", ei.Get("SMIDN_DIVISION_DESCP"));
                smidnei.Put("PRIORITY", "2");
            }
            //smidnei.Put("INSERT_NO", InsertNo);
            smidnei.Put("CNTRY_CD", PodCnty);
            smidnei.Put("TC_CNTRY_CD", PodCnty);
            ml.Add(smidnei);
            if (invnoDic.Keys.Contains(invno))
            {
                invnoDic[masterno] = Dn_UId;
            }
            else
            {
                invnoDic.Add(invno, Dn_UId);
            }
        }

        private static string CreateSMSMIMasterinfo(EditInstruct ei, Dictionary<string, object> parm)
        {
            List<string> shipmentList = (List<string>)parm["shipmentList"];
            string CompanyId = ei.Get("SMSMI_CMP");
            string GroupId = Prolink.Math.GetValueAsString(parm["GroupId"]);
            string UserId = Prolink.Math.GetValueAsString(parm["UserId"]);
            string masterno = ei.Get("SMSMI_MASTER_NO");
            Dictionary<string, string> masternoDic = (Dictionary<string, string>)parm["masternoDic"];
            Dictionary<string, ShipmentFreeInfo> freeDic = (Dictionary<string, ShipmentFreeInfo>)parm["freeDic"];
            if (masternoDic.Keys.Contains(masterno))
                return "";

            MixedList ml = (MixedList)parm["mixedlist"];
            string trantype = ei.Get("SMSMI_TRAN_TYPE");
            string[] names = ei.getNameSet();
            List<string> smsmifields = new List<string>();
            string shipmentid = string.Empty;
            if (CityDataTable.Rows.Count <= 0) InitCityData();
            foreach (string fildname in names)
            {
                if (fildname.StartsWith("SMSMI_"))
                {
                    smsmifields.Add(fildname.Replace("SMSMI_", ""));
                }
            }


            string shipmentuid = string.Empty; 
            DataTable smsmidt = OperationUtils.GetDataTable(string.Format("SELECT * FROM SMSMI WHERE MASTER_NO={0}", SQLUtils.QuotedStr(masterno)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.INSERT_OPERATION);
            if (smsmidt.Rows.Count > 0)
            {
                if (!"A".Equals(smsmidt.Rows[0]["STATUS"].ToString()))
                {
                    throw new Exception("Master No :" + masterno + " is already exist and status is not unreach,So you cannot import it!");
                }
                shipmentid = smsmidt.Rows[0]["SHIPMENT_ID"].ToString();

                shipmentuid = smsmidt.Rows[0]["U_ID"].ToString();
                smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                smsmiei.PutKey("U_ID", shipmentuid);
                smsmiei.PutKey("GROUP_ID", smsmidt.Rows[0]["GROUP_ID"].ToString());
                smsmiei.PutKey("CMP", smsmidt.Rows[0]["CMP"].ToString());
            }
            else
            {
                if ("F".Equals(trantype))
                {
                    string scarrier = ei.Get("SMSMI_CARRIER");
                    string sql = string.Format("SELECT CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TCAR' AND CD={0}", SQLUtils.QuotedStr(scarrier));
                    DataTable bscodedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (bscodedt.Rows.Count <= 0)
                        throw new Exception("Master No :" + masterno + ", the Carrier Code " + scarrier + " is not exist in Master Data.");
                }
                shipmentid = TrackingEDI.InboundBusiness.ReserveHelper.getAutoNo("SHIB_NO", GroupId, CompanyId);
                smsmiei.Put("TRAN_TYPE1", "T");
                DateTime odt = DateTime.Now;
                DateTime ndt = TrackingEDI.TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                smsmiei.PutDate("CREATE_DATE", odt);
                smsmiei.PutDate("CREATE_DATE_L", ndt);
                smsmiei.Put("CREATE_BY", UserId);
                smsmiei.Put("SHIPMENT_ID", shipmentid);
                smsmiei.Put("STATUS", "A");
                smsmiei.Put("GROUP_ID", GroupId);
                shipmentuid = System.Guid.NewGuid().ToString();
                smsmiei.Put("U_ID", shipmentuid);
            }

            if (!shipmentList.Contains(shipmentid))
            {
                shipmentList.Add(shipmentid);

                EditInstruct smidnpdel = new EditInstruct("SMIDNP", EditInstruct.DELETE_OPERATION);
                smidnpdel.Condition = string.Format(" SHIPMENT_ID = {0} ", SQLUtils.QuotedStr(shipmentid));
                ml.Add(smidnpdel);

                EditInstruct smidndel = new EditInstruct("SMIDN", EditInstruct.DELETE_OPERATION);
                smidndel.Condition = string.Format(" SHIPMENT_ID = {0} ", SQLUtils.QuotedStr(shipmentid));
                ml.Add(smidndel);

                EditInstruct smicntrdel = new EditInstruct("SMICNTR", EditInstruct.DELETE_OPERATION);
                smicntrdel.Condition = string.Format(" SHIPMENT_ID = {0} ", SQLUtils.QuotedStr(shipmentid));
                ml.Add(smicntrdel);
            }
            if (masternoDic.Keys.Contains(masterno))
            {
                masternoDic[masterno] = shipmentid;
            }
            else
            {
                masternoDic.Add(masterno, shipmentid);
            }

            Func<string, EditInstruct, bool> setsmsiFiledvalue = (field, funcei) =>
            {
                string gvalue = "";
                if (smsmidt.Rows.Count > 0)
                {
                    gvalue = Prolink.Math.GetValueAsString(smsmidt.Rows[0][field]);
                }
                else
                {
                    try
                    {
                        gvalue = ei.Get("SMSMI_" + field);
                        if ("FRT_TERM".Equals(field) && gvalue.Length >= 1)
                        {
                            gvalue = gvalue.Substring(0, 1).ToUpper();
                        }
                    }
                    catch (Exception e) { return false; }
                }
                funcei.Put(field, gvalue);
                return true;
            };

            foreach (string smsmifiled in smsmifields)
            {
                setsmsiFiledvalue(smsmifiled, smsmiei);
            }
            string[] dates = ei.GetDateSet();
            foreach (string datefield in dates)
            {
                if (datefield.StartsWith("SMSMI_"))
                {
                    smsmiei.AddDate(datefield.Replace("SMSMI_", ""));
                }
            }
            #region CheckInfoMation
            decimal Year = smsmiei.GetValueAsDecimal("YEAR");
            decimal Month = smsmiei.GetValueAsDecimal("MONTH");
            decimal Weekly = smsmiei.GetValueAsDecimal("WEEKLY");
            DateTime Etd = Prolink.Math.GetValueAsDateTime(ei.Get("SMSMI_ETD1"));
            DateTime Eta = Prolink.Math.GetValueAsDateTime(ei.Get("SMSMI_ETA1"));

            List<String> msgs = new List<string>();
            if (DateTime.Compare(Etd, Eta) > 0)
            {
                msgs.Add(string.Format("{0}: Import failed! ETA is not less than ETD, please check!", masterno));
            }
            DateTime eta = Eta;
            if (!string.IsNullOrEmpty(ei.Get("SMSMI_ETA4")))
            {
                eta = Prolink.Math.GetValueAsDateTime(ei.Get("SMSMI_ETA4"));
            }
            if (!string.IsNullOrEmpty(ei.Get("SMSMI_ETA3")))
            {
                eta = Prolink.Math.GetValueAsDateTime(ei.Get("SMSMI_ETA3"));
            }
            if (!string.IsNullOrEmpty(ei.Get("SMSMI_ETA2")))
            {
                eta = Prolink.Math.GetValueAsDateTime(ei.Get("SMSMI_ETA2"));
            }
            smsmiei.PutDate("ETD", Etd);
            smsmiei.PutDate("ETA", eta);

            string date = ei.Get("SMSMI_ETD1");
            //if (string.IsNullOrEmpty(date))
            //    date = ei.Get("SMSMI_ATD1");
            if (Year == 0 && Month == 0 && Weekly == 0)
            {
                if (!string.IsNullOrEmpty(date))
                {
                    DateTime dateT = Prolink.Math.GetValueAsDateTime(date);
                    Year = dateT.Year;
                    Month = dateT.Month;
                    Weekly = TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(dateT, DayOfWeek.Monday);
                }
            }
            smsmiei.Put("YEAR", Year);
            smsmiei.Put("MONTH", Month);
            smsmiei.Put("WEEKLY", Weekly);
            smsmiei.Put("ETA_WK", TrackingEDI.Business.DateTimeUtils.GetWeekOfYear(eta, DayOfWeek.Monday));

            List<string> msg = new List<string>();
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                if (string.IsNullOrEmpty(ei.Get("CNTR_NO")))
                {
                    msg.Add("Container#");
                }
                if (string.IsNullOrEmpty(ei.Get("SMICNTR_CNTR_TYPE")))
                {
                    msg.Add("ContainerType");
                }
                if (string.IsNullOrEmpty(ei.Get("SMICNTR_SEAL_NO1")))
                {
                    msg.Add("Seal#");
                }
            }
            if (msg.Count > 0)
                msgs.Add(string.Format("{0}: Import failed! {1} is key item, cannot be kept blank! Please check again.", masterno, string.Join(",", msg)));


            List<string> msgStr = new List<string>();
            string porcd = smsmiei.Get("POR_CD");
            Tuple<bool, string> tuple = GetPortName(CityDataTable, porcd);
            if (!tuple.Item1)
            {
                msgStr.Add("POR:" + porcd);
            }
            else
            {
                smsmiei.Put("POR_NAME", tuple.Item2);
                smsmiei.Put("POR_CNTY", porcd.Substring(0, 2));
            }
            string polcd = smsmiei.Get("POL_CD");
            tuple = GetPortName(CityDataTable, polcd);
            if (!tuple.Item1)
            {
                msgStr.Add("POL:" + polcd);
            }
            else
            {
                smsmiei.Put("POL_NAME", tuple.Item2);
                smsmiei.Put("POL_CNTY", polcd.Substring(0, 2));
            }
            string podcd = smsmiei.Get("POD_CD");
            tuple = GetPortName(CityDataTable, podcd, true);
            if (!tuple.Item1)
            {
                msgStr.Add("POD:" + podcd);
            }
            else
            { 
                smsmiei.Put("POD_NAME", tuple.Item2);
                smsmiei.Put("POD_CNTY", podcd.Substring(0, 2));
            }
            string destcd = smsmiei.Get("DEST_CD");
            tuple = GetPortName(CityDataTable, destcd, true);
            if (!tuple.Item1)
            {
                msgStr.Add("DEST:" + destcd);
            }
            else
            {
                smsmiei.Put("DEST_NAME", tuple.Item2);
                smsmiei.Put("DEST_CNTY", destcd.Substring(0, 2));
            }
            if (msgStr.Count > 0)
            {
                msgs.Add(string.Format("{0}: Import failed!{1} is not exist in PortCodeList Setup!Please check again.", masterno, string.Join(",", msgStr)));
            }

            if (msgs.Count > 0)
            {
                throw new Exception(string.Join(System.Environment.NewLine, msgs));
            }
            #endregion

            smsmiei.Put("OUTSOURCING_TO_INBOUND", "N");
            smsmiei.Put("TRADE_TERM", smsmiei.Get("INCOTERM_CD"));
            smsmiei.Put("TRADETERM_DESCP", smsmiei.Get("INCOTERM_DESCP"));
            smsmiei.Put("SVC_CONTACT", smsmiei.Get("SVC_CONTACT"));


            string shipperno = ei.Get("HTML_SH");
            string consignee = ei.Get("HTML_CS");
            string notifyNo = ei.Get("HTML_NT");
            smsmiei.Put("OEXPORTER", shipperno);
            smsmiei.Put("TC_IMPORTER", consignee);
            smsmiei.Put("OIMPORTER", consignee);
            if (SmptyDataTable.Rows.Count == 0) InitSmptyData();

            smsmiei.Put("OEXPORTER_NM", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(shipperno)), "PARTY_NAME"));
            smsmiei.Put("OEXPORTER_ADDR", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(shipperno)), "PART_ADDR1"));
            smsmiei.Put("TC_IMPORTER_NM", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(consignee)), "PARTY_NAME"));
            smsmiei.Put("TC_IMPORTER_ADDR", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(consignee)), "PART_ADDR1"));
            smsmiei.Put("OIMPORTER_NM", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(consignee)), "PARTY_NAME"));
            smsmiei.Put("OIMPORTER_ADDR", GetTableValueByName(SmptyDataTable, string.Format("PARTY_NO = {0}", SQLUtils.QuotedStr(consignee)), "PART_ADDR1"));
            string extrasrv = TrackingEDI.InboundBusiness.SMSMIHelper.GetExtraSrvInfo(trantype, masterno, CompanyId);
            if (!string.IsNullOrEmpty(extrasrv))
            {
                smsmiei.Put("EXTRA_SRV", extrasrv);
            }
            if (DestMapDataTable.Rows.Count == 0) InitDestMapData();
            string secCmp = GetTableValueByName(DestMapDataTable, string.Format("DEST_CODE={0} AND (CMP={1} OR CMP='*')",
                SQLUtils.QuotedStr(ei.Get("SMSMI_DEST_CD")), SQLUtils.QuotedStr(CompanyId)), "SEC_CMP");

            string directSql = string.Format("SELECT COUNT(*) C FROM DIRECT_MAP WHERE (PARTY_NO={0} OR PARTY_NO={1}) AND (CMP={2} OR CMP='*')",
                SQLUtils.QuotedStr(consignee), SQLUtils.QuotedStr(notifyNo), SQLUtils.QuotedStr(CompanyId));
            int counts = OperationUtils.GetValueAsInt(directSql, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (counts > 0)
                secCmp = "";
            smsmiei.Put("SEC_CMP", secCmp);


            string carrier = ei.Get("SMSMI_CARRIER");
            if (CarrierDataTable.Rows.Count == 0) InitCarrierData();
            string carriername = GetTableValueByName(CarrierDataTable, string.Format("CD={0}", SQLUtils.QuotedStr(carrier)), "CD_DESCP");
            smsmiei.Put("CARRIER_NM", carriername);
            //string LspNo = InboundBusiness.InboundHelper.GetForwarderNo(shipmentid);
            string HandlingAgen = ei.Get("HTML_SP");
            Tuple<string, int, int, int, string, string, string,Tuple<string>> portFreeItem = new Tuple<string, int, int, int, string, string, string,Tuple<string> >("", 0, 0, 0, "", "", "",new Tuple<string>(""));
            if ("F".Equals(trantype) || "R".Equals(trantype))
            {
                portFreeItem = InboundBusiness.InboundHelper.getFreeTime(HandlingAgen, podcd, CompanyId, carrier, trantype, eta, Etd);
            }
            smsmiei.Put("PORT_FREE_TIME", portFreeItem.Item4);
            smsmiei.Put("FACT_FREE_TIME", portFreeItem.Item3);
            smsmiei.Put("CON_FREE_TIME", portFreeItem.Item2);

            smsmiei.Put("COMBINE_DET", portFreeItem.Item1);
            smsmiei.Put("SHOW_COMBINE_DET", portFreeItem.Item5);
            ShipmentFreeInfo shiFree = new ShipmentFreeInfo(shipmentid);
            if (!freeDic.ContainsKey(shipmentid))
                freeDic.Add(shipmentid, shiFree);
            shiFree = freeDic[shipmentid];
            if (eta > DateTime.MinValue)
            {
                DataTable bsDateDt = DayHelper.GetBsdate(CompanyId); 
                shiFree.StorageDueDate = DayHelper.AddWorkHolidays(eta, portFreeItem.Item3, bsDateDt,portFreeItem.Item7);
                shiFree.CON_CHG_TYPE = portFreeItem.Item7;
                shiFree.DemurrageDueDate = DayHelper.AddWorkHolidays(eta, portFreeItem.Item4, bsDateDt, portFreeItem.Rest.Item1);
                shiFree.PORT_CHG_TYPE = portFreeItem.Rest.Item1;
            }
            string IsfSeller = ei.Get("HTML_SISF");
            string BookingParty = ei.Get("HTML_IBBK");
            string ShipToNo = ei.Get("HTML_WE");
            string FiCustomer = ei.Get("HTML_FC");
            string SubBG = ei.Get("HTML_ZT");
            string Importer = ei.Get("HTML_RE");
            
            CreateSmsmiptinfo(shipmentuid, shipmentid, "SISF", IsfSeller, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "IBBK", BookingParty, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "SH", shipperno, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "CS", consignee, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "NT", notifyNo, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "WE", ShipToNo, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "FC", FiCustomer, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "ZT", SubBG, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "RE", Importer, parm, CompanyId);
            CreateSmsmiptinfo(shipmentuid, shipmentid, "SP", HandlingAgen, parm, CompanyId);

            CreateParty(CompanyId, shipmentuid, shipmentid, parm);

            if (string.IsNullOrEmpty(trantype))
            {
                return BaseParser.ERROR;
            }
            ml.Add(smsmiei);
            return "";
        }

        public static void CreateParty(string CompanyId, string shipmentuid, string shipmentid, Dictionary<string, object> parm)
        {
            string sql = string.Format(@"SELECT SELLER,SELLER_SYN,CNEE_CD,CNEE_CD_SYN,NOTIFY1,NOTIFY1_SYN,
            NOTIFY2,NOTIFY2_SYN,NOTIFY3,NOTIFY3_SYN,SHIP_TO,SHIP_TO_SYN,SUB_BG,SUB_BG_SYN,
            FI_CUSTOMER, FI_CUSTOMER_SYN,BILL_TO,BILL_TO_SYN,PAYER_PARTY,PAYER_PARTY_SYN,ORDER_RECEIVER,ORDER_RECEIVER_SYN,
            SOLD_TO,SOLD_TO_SYN,SALES_CUSTOMER,SALES_CUSTOMER_SYN FROM SMSIM WHERE PROFILE={0}", SQLUtils.QuotedStr(CompanyId));

            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return;
            DataRow dr = dt.Rows[0];
            Dictionary<string, string> pairs = new Dictionary<string, string> { { "SELLER", "SH" },{ "CNEE_CD","CS" },{ "NOTIFY1","NT" },
                {"NOTIFY2","Z4" },{ "NOTIFY3","Z5"},{ "SHIP_TO","WE"},{ "SUB_BG","ZT"},{ "FI_CUSTOMER","FC"},{ "BILL_TO","RE"},
                { "PAYER_PARTY","RG"},{"ORDER_RECEIVER","RO" },{ "SOLD_TO","AG"},{"SALES_CUSTOMER","ZE" } };

            Dictionary<string, string> insertDic = new Dictionary<string, string>();
            string code, type, syn;
            foreach (string key in pairs.Keys)
            {
                code = Prolink.Math.GetValueAsString(dr[key]);
                type = pairs[key];
                syn = Prolink.Math.GetValueAsString(dr[key + "_SYN"] );
                if (syn.Equals("Y") && !insertDic.ContainsKey(type))
                {
                    insertDic.Add(type, code);
                }
            } 
            foreach (string key in insertDic.Keys)
            {
                string value = insertDic[key];
                CreateSmsmiptinfo(shipmentuid, shipmentid, key, value, parm, CompanyId);
            }
        }
        

        public static void CreateSmsmiptinfo(string U_ID, string shipmentid, string partytype, string partyno, Dictionary<string, object> parm,string cmp)
        {
            if (string.IsNullOrEmpty(partyno)) return ;
            string sql = string.Format("SELECT * FROM SMPTY WHERE PARTY_NO = {0}", SQLUtils.QuotedStr(partyno));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dt.Rows.Count <= 0) return ;
            MixedList ml = (MixedList)parm["mixedlist"];

            DataRow dr = dt.Rows[0];
            EditInstruct smsmiptei = new EditInstruct("SMSMIPT", EditInstruct.DELETE_OPERATION);
            smsmiptei.PutKey("SHIPMENT_ID", shipmentid);
            smsmiptei.PutKey("PARTY_TYPE", partytype);
            ml.Add(smsmiptei);

            string typedescp = string.Empty;
            string orderby = string.Empty;
            sql = string.Format("SELECT CD_DESCP,ORDER_BY FROM BSCODE WHERE CD_TYPE='PT' AND CD={0} AND (CMP={1} OR CMP='*')", SQLUtils.QuotedStr(partytype), SQLUtils.QuotedStr(cmp));
            DataTable bscodedt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (bscodedt.Rows.Count > 0)
            {
                typedescp = Prolink.Math.GetValueAsString(bscodedt.Rows[0]["CD_DESCP"]);
                orderby = Prolink.Math.GetValueAsString(bscodedt.Rows[0]["ORDER_BY"]);
            }
            if (partytype == "IBBK")
            {
                typedescp = "Outsite Vender";
            }
            string UFid = System.Guid.NewGuid().ToString();
            EditInstruct ei = new EditInstruct("SMSMIPT", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", UFid);
            ei.Put("U_FID", U_ID);
            ei.Put("SHIPMENT_ID", shipmentid);
            ei.Put("PARTY_TYPE", partytype);
            ei.Put("TYPE_DESCP", typedescp);
            ei.Put("ORDER_BY", orderby);
            ei.Put("PARTY_NO", partyno);
            ei.Put("PARTY_NAME", Prolink.Math.GetValueAsString(dr["PARTY_NAME"]));
            ei.Put("PARTY_NAME2", Prolink.Math.GetValueAsString(dr["PARTY_NAME2"]));
            ei.Put("PARTY_NAME3", Prolink.Math.GetValueAsString(dr["PARTY_NAME3"]));
            ei.Put("PARTY_NAME4", Prolink.Math.GetValueAsString(dr["PARTY_NAME4"]));
            ei.Put("PARTY_ADDR1", Prolink.Math.GetValueAsString(dr["PART_ADDR1"]));
            ei.Put("PARTY_ADDR2", Prolink.Math.GetValueAsString(dr["PART_ADDR2"]));
            ei.Put("PARTY_ADDR3", Prolink.Math.GetValueAsString(dr["PART_ADDR3"]));
            ei.Put("PARTY_ADDR4", Prolink.Math.GetValueAsString(dr["PART_ADDR4"]));
            ei.Put("PARTY_ADDR5", Prolink.Math.GetValueAsString(dr["PART_ADDR5"]));
            ei.Put("CNTY", Prolink.Math.GetValueAsString(dr["CNTY"]));
            ei.Put("CNTY_NM", Prolink.Math.GetValueAsString(dr["CNTY_NM"]));
            ei.Put("CITY", Prolink.Math.GetValueAsString(dr["CITY"]));
            ei.Put("CITY_NM", Prolink.Math.GetValueAsString(dr["CITY_NM"]));
            ei.Put("STATE", Prolink.Math.GetValueAsString(dr["STATE"]));
            ei.Put("ZIP", Prolink.Math.GetValueAsString(dr["ZIP"]));
            ei.Put("PARTY_ATTN", Prolink.Math.GetValueAsString(dr["PARTY_ATTN"]));
            ei.Put("PARTY_TEL", Prolink.Math.GetValueAsString(dr["PARTY_TEL"]));
            ei.Put("PARTY_MAIL", Prolink.Math.GetValueAsString(dr["PARTY_MAIL"]));
            //ei.Put("DEBIT_TO",Prolink.Math.GetValueAsString(dr["STATE"]));
            ei.Put("FAX_NO", Prolink.Math.GetValueAsString(dr["PARTY_FAX"]));
            ei.Put("TAX_NO", Prolink.Math.GetValueAsString(dr["TAX_NO"]));
            ml.Add(ei);
        }


        private static string GetTableValueByName(DataTable dt, string filter, string filename)
        {
            string value = string.Empty;
            if (!dt.Columns.Contains(filename))
                return value;
            DataRow[] drs = dt.Select(filter);
            if (drs.Length > 0)
                value = Prolink.Math.GetValueAsString(drs[0][filename]);
            return value;
        }

        private static Tuple<bool, string> GetPortName(DataTable dt, string portCd, bool portName2Flag = false)
        {
            string filter = string.Format("CNTRY_CD = {0} AND PORT_CD = {1}", SQLUtils.QuotedStr(portCd.Substring(0, 2)), SQLUtils.QuotedStr(portCd.Substring(2, 3))); 
            if (!dt.Columns.Contains("PORT_NM"))
                return new Tuple<bool, string>(false,string.Empty);
            DataRow[] drs = dt.Select(filter);
            if (drs.Length <= 0)
                return new Tuple<bool, string>(false, string.Empty);
            else
            {
                string portNm2 = !dt.Columns.Contains("PORT_NM2") ? "" : Prolink.Math.GetValueAsString(drs[0]["PORT_NM2"]);
                return new Tuple<bool, string>(true, portName2Flag && !string.IsNullOrEmpty(portNm2) ? portNm2 : Prolink.Math.GetValueAsString(drs[0]["PORT_NM"]));
            }
        }

        public class ShipmentFreeInfo {
            public string ShipmentId { get; set; }
            public DateTime StorageDueDate { get; set; }
            public string CON_CHG_TYPE { get; set; }
            public DateTime DemurrageDueDate { get; set; }
            public string PORT_CHG_TYPE { get; set; }
            public ShipmentFreeInfo(string shipmentId)
            {
                this.ShipmentId = shipmentId;
                this.StorageDueDate = DateTime.MinValue;
                this.CON_CHG_TYPE = string.Empty;
                this.DemurrageDueDate = DateTime.MinValue;
                this.PORT_CHG_TYPE = string.Empty;
            }
        }
    }
}
