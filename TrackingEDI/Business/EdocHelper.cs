using EDOCApi;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Configuration;

namespace TrackingEDI.Business
{
    public class EdocHelper
    {
        private Prolink.EDOC_API _api = new Prolink.EDOC_API();
        private static int m_Timeout = 300000;
        //获取电子文档设定那边设定的发送夹档
        public string GetEdocSetTypes(string company, string partytype, string trantype, string partyno)
        {
            string sql = string.Format("SELECT * FROM TKPDM WHERE GROUP_ID='TPV' AND CMP={0} AND PARTY_TYPE={1} AND TRAN_MODE={2}", SQLUtils.QuotedStr(company), SQLUtils.QuotedStr(partytype), SQLUtils.QuotedStr(trantype));
            string unionsql = sql + " AND PARTY_NO=" + SQLUtils.QuotedStr(partyno) + " UNION " + sql;
            DataTable pdmDdt = OperationUtils.GetDataTable(unionsql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string stscd = string.Empty;
            if (pdmDdt.Rows.Count <= 0) 
                return string.Empty;
            DataRow dr = pdmDdt.Rows[0];
            string docId = Prolink.Math.GetValueAsString(dr["U_ID"]);
            sql = string.Format("SELECT DOC_TYPE FROM TKPDD WHERE U_ID={0} AND CMP={1}", SQLUtils.QuotedStr(docId), SQLUtils.QuotedStr(company));
            string type = string.Empty;
            DataTable docTypeDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            foreach (DataRow docType in docTypeDt.Rows)
            {
                string doc_type = Prolink.Math.GetValueAsString(docType["DOC_TYPE"]);
                if (string.IsNullOrEmpty(doc_type))
                    continue;
                doc_type = doc_type.Trim();
                if (!string.IsNullOrEmpty(type))
                    type += ";";
                type += doc_type;
            }
            return type;
        }

        public Result AutoCreateReport(Dictionary<string, object> parm)
        {
            return CreateNewReport2Edoc(parm);
        }

        //如果没有档案就新增一个对于FCL/LCL  内贸的用中文bookingfrom 要实现自动归档
        public Result CreateBFReport(string shipmentid,string edoctype)
        {
            //判断是否需要生产档案
            string sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentid));
            DataTable smdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string trantype = Prolink.Math.GetValueAsString(smdt.Rows[0]["TRAN_TYPE"]);
            string groupId = Prolink.Math.GetValueAsString(smdt.Rows[0]["GROUP_ID"]);
            string cmp = Prolink.Math.GetValueAsString(smdt.Rows[0]["CMP"]);
            string uid = Prolink.Math.GetValueAsString(smdt.Rows[0]["U_ID"]);

            string partytype=GetTypeByTranType(trantype);
            string[] partytypes = partytype.Split(';');
            sql=string.Format("SELECT * FROM SMSMPT WHERE SHIPMENT_ID={0} AND PARTY_TYPE IN('BO','SP','CR')", SQLUtils.QuotedStr(shipmentid));
            DataTable smsmptDT = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow []smptrows= smsmptDT.Select(string.Format("PARTY_TYPE IN {0}", SQLUtils.Quoted(partytypes)));
            
            //增加判断报表的模型
            if (smptrows.Length != 1)
            {
                Result result = new Result();
                result.Success = false;
                result.Message = "请检核Party档信息准确性";
                return result;
            }
            partytype = smptrows[0]["PARTY_TYPE"].ToString();
            string partyno = smptrows[0]["PARTY_NO"].ToString();


            //产生报表并归档
            Dictionary<string, object> parm = new Dictionary<string, object>();
            parm["PartyType"] = partytype;
            
            parm["exportFileType"] = "pdf";
            
            parm["fileType"] = "BF";
            if ("T".Equals(trantype))
            {
                parm["reportId"] = "FCL03";
                parm["reportName"] = "Booking form 中文";
            }
            else
            {
                parm["reportId"] = "FCL01";
                parm["reportName"] = "Booking Form";
            }

            parm["conditionString"] = "ShipmentId=" + shipmentid + "&sopt_ShipmentId=eq";
           // parm["conditionString"] = "SHIPMENT_ID=" + shipmentid;
            parm["jobNo"] = uid;
            parm["GroupId"] = groupId;
            parm["CMP"] = cmp;
            parm["STN"] = Prolink.Math.GetValueAsString(smdt.Rows[0]["STN"]);
            parm["ShipmentId"] = shipmentid;
            parm["TranType"] = trantype;
            parm["PartyNo"] = partyno;
            parm["Remark"] = "ESP";
            SetParmByTranType(ref parm, edoctype);

            return CreateNewReport2Edoc(parm);
        }

        

        public bool SetParmByTranType(ref Dictionary<string, object> parm,string edocType)
        {
            string cmp = parm["CMP"].ToString();
            string partytype = parm["PartyType"].ToString();
            string trantype = parm["TranType"].ToString();
            string partyno = parm["PartyNo"].ToString();
            string type = GetEdocSetTypes(cmp, partytype, trantype, partyno);
            type=type.Trim(';');
            string[] types = type.Split(';');
            int typescount = types.Length;
            if (typescount == 0 || string.IsNullOrEmpty(type))
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("电子文案没有设定，或设定的明细为空");
                return false;
            }
            foreach(string index in types){
                if(string.IsNullOrEmpty(index))continue;
                if (edocType != index) continue;
                string[] fileinfo = GetEdocDescp(edocType, trantype);
                parm["fileType"] = fileinfo[0];
                parm["reportName"] = fileinfo[1];
                parm["reportId"] = fileinfo[2];
            }
            return true;
        }

        /// <summary>
        /// 根据传入的Edoc Type 抓取对应报表的名称和对应的报表ID
        /// </summary>
        /// <param name="edocType">Doc Type</param>
        /// <returns>返回长为3的string字符串数组，第一个为fileType，第二个为reportName，第三个为reportId</returns>
        public static string[] GetEdocDescp(string edocType,string trantype="")
        {
            string[] fileInfo=new string[3];
            
            switch (edocType)
            {
                case "BF":
                    fileInfo[0]=edocType;
                    fileInfo[1]="Booking Form";
                    if ("C".Equals(trantype))
                        fileInfo[2] = "FCL03";
                    else
                        fileInfo[2]  = "FCL01";
                    break;
                case "INVO":
                    //归档出口invoice
                    fileInfo[0]=edocType;
                    fileInfo[1]= "出口报关 Invoice";
                    fileInfo[2]  = "IPQ06";
                    break;
                case "PACKO":
                    //归档出口packing
                    fileInfo[0]=edocType;
                    fileInfo[1]= "出口报关 Packing List";
                    fileInfo[2]  = "IPQ05";
                    break;
                case "CONTRACT":
                    //归档出口packing
                    fileInfo[0] = edocType;
                    fileInfo[1] = "合约";
                    fileInfo[2] = "IPQ06C";
                    break;
                default:
                    break;
            }
            return fileInfo;
        }

        public Result CreateNewReport2Edoc(Dictionary<string, object> parm)
        {
            Result result = new Result();
            Dictionary<string, object> item = null;
            item = new Dictionary<string, object>();
            item["url"] = WebConfigurationManager.AppSettings["EDOC_URL1"]; // get outer url for edoc 
            item["softID"] = WebConfigurationManager.AppSettings["EDOC_SOFTID"];
            item["account"] = WebConfigurationManager.AppSettings["EDOC_ACCOUNT"];
            item["password"] = WebConfigurationManager.AppSettings["EDOC_PASSWORD"];
            item["syncTime"] = Prolink.Math.GetValueAsString(WebConfigurationManager.AppSettings["EDOC_SYNC_TIME"]);
            item["syncDnCount"] = Prolink.Math.GetValueAsString(WebConfigurationManager.AppSettings["EDOC_SYNC_DN_COUNT"]);
            EdocParm edocparam = new EdocParm(parm);

            string baseUrl = WebConfigurationManager.AppSettings["LOCAL_URL"];

            int serverNum = 0;
            string edocFGUID = CheckDuplicatedFolder(edocparam.JobNo, edocparam.GroupId, edocparam.Cmp, edocparam.Stn, edocparam.Dep, ref serverNum);
            if (string.IsNullOrEmpty(edocFGUID))
            {
                result.Success = false;
                result.Message = "Upload to Edoc error,Folder GuID is null,Please check EDI LOG!";
                return result;
            }
            string tempExportFile = null;
            string reportTitle = System.DateTime.Now.ToString("yyyy-MM-dd HH mm ss ") + edocparam.ReportName;
            try
            {
                //string arg_count = Request["arg_count"];
                string arg_count = null;
                int count = 0;
                if (!string.IsNullOrEmpty(arg_count))
                    int.TryParse(arg_count, out count);

                byte[] bs =edocparam.Bytes;
                OperationUtils.Logger.WriteLog("DownloadFile=" + baseUrl + "/zh-CN/Report/CreateNewReport2Edoc");//+ Request.ApplicationPath.TrimEnd('/')
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(baseUrl + "/zh-CN/Report/CreateNewReport2Edoc");
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs.Length;
                req.Timeout = m_Timeout;
                //req.ContentType = "application/json; charset=utf-8";


                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bs, 0, bs.Length);
                }

                string text;
                using (WebResponse wr = req.GetResponse())
                {
                    using (Stream stream = wr.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        text = reader.ReadToEnd();
                        string v3DownloadRptUrl = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<V3Result>(text).URL.TrimStart('/');
                        OperationUtils.Logger.WriteLog("v3DownloadRptUrl=" + v3DownloadRptUrl);
                        switch (edocparam.ExportFileType.ToLower())
                        {
                            case "xls":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".xls";
                                    break;
                                }
                            case "doc":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".docx";
                                    break;
                                }
                            case "pdf":
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".pdf";
                                    break;
                                }
                            default:
                                {
                                    tempExportFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/" + reportTitle + ".xls";
                                    edocparam.ExportFileType = "excel";
                                    break;
                                }
                        }
                        OperationUtils.Logger.WriteLog("tempExportFile=" + tempExportFile);
                        OperationUtils.Logger.WriteLog("DownloadFile=" + baseUrl + v3DownloadRptUrl);
                        item["userid"] =edocparam.Userid;
                        result = doUpload2EDOC(true, item, edocFGUID, edocparam.FileType, edocparam.Remark, baseUrl, v3DownloadRptUrl, tempExportFile, serverNum);
                        System.IO.File.Delete(tempExportFile);
                    }
                }
            }
            catch (Exception e)
            {
                OperationUtils.Logger.WriteLog(e.ToString());
                result.Success = false;
                result.Message = e.Message;
            }
            return result;
        }

        static bool IsGuidByParse(string strSrc)
        {
            Guid g = Guid.Empty;
            return Guid.TryParse(strSrc, out g);
        }

        public string GetTypeByTranType(string trantype)
        {
            string partytype = string.Empty;
            switch (trantype)
            {
                case "A":
                case "L":
                case "R":
                    partytype = "SP";
                    break;
                case "F":
                    partytype = "BO;SP";
                    break;
                case "E":
                    partytype = "SP";
                    break;
                case "D":
                case "T":
                    partytype = "CR";
                    break;
            }
            return partytype;
        }


        //判断是否需要发送夹档


        //归档功能

        public Result doUpload2EDOC(bool sync, Dictionary<string, object> item, string edocFGUID, string fileType, string remark, string baseUrl, string v3DownloadRptUrl, string tempExportFilePath, int serverNum=0)
        {

            if (!sync)
            {
                Thread.Sleep(Int32.Parse(item["syncTime"].ToString()));
            }
            Result result = new Result();
            try
            {
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    webClient.DownloadFile(new Uri(baseUrl + v3DownloadRptUrl), tempExportFilePath);
                }
                string UserId = Prolink.Math.GetValueAsString(item["userid"]);
                var token = UploadFileToStorage(edocFGUID, tempExportFilePath, UserId, serverNum);

                OperationUtils.Logger.WriteLog("EdocUrl=" + item["url"].ToString() + "apis/apilaunchfile.ashx?token=" + token);
                if (token != "")
                {
                    string[] fileID = token.ToString().Split(new string[] { "&i=" }, StringSplitOptions.None);
                    List<UpdateFileItem> fileList = new List<UpdateFileItem>();

                    fileList.Add(new UpdateFileItem
                    {
                        FileID = fileID[1],
                        EdocType = fileType,
                        Remark = remark
                    });

                    bool isSuccess = _api.UpdateFiles(fileList, serverNum);

                    result.EdocUrl = item["url"].ToString() + "apis/apilaunchfile.ashx?token=" + token;
                    result.Success = true;
                    result.Message = "done.";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Upload to Edoc error";
                }
            }catch(Exception ex){
                result.Success = false;
                result.Message = ex.ToString();
            }

            return result;
        }

        private string UploadFileToStorage(string edocFGUID, string upLoadFile, string UserId,int serverNum=0)
        {


            Dictionary<string, object> item = null;
            item = new Dictionary<string, object>();
            item["url"] = WebConfigurationManager.AppSettings["EDOC_URL1"];
            item["softID"] = WebConfigurationManager.AppSettings["EDOC_SOFTID"];
            item["account"] = WebConfigurationManager.AppSettings["EDOC_ACCOUNT"];
            item["password"] = WebConfigurationManager.AppSettings["EDOC_PASSWORD"];

            EDOCResultUploadFile result = _api.UploadFile(edocFGUID, upLoadFile, "Remark", UserId, "D", "8", serverNum);
            if (result.Status == DBErrors.DB_SUCCESS)
            {
                return result.FileInfo.Token + "&i=" + result.FileInfo.FileID;
            }
            else
            {
                OperationUtils.Logger.WriteLog(result.Status.ToString());
                return "";
            }
        }


        //判断是否要生成文件功能

        //FCL，LCL 检核booking from 其它的检核中文版booking from


        //执行Reload Invoice 的动作
        public string AutoReloadInvoice(string UId)
        {
            //string UId = Prolink.Math.GetValueAsString(Request.Params["UId"]);
            string sql = "SELECT DN_NO FROM SMINM WHERE U_ID=" + SQLUtils.QuotedStr(UId);
            string DnNo = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            sql = "SELECT SHIPMENT_ID FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
            string ShipmentId = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            string InvoiceIntruction = "", InvoiceRemark = "", PackingRemark = "", PackingIntruction = "", firstDn = "", DnNoCmpRef = "";

            DataTable shipResult = new DataTable();
            DataTable partyResult = new DataTable();
            DataTable dnResult = new DataTable();

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

                    GetRef(ref sdt);
                    Getblno(ref sdt);
                    shipResult = sdt;

                    sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR1,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                PARTY_ATTN, PARTY_TEL, FAX_NO 
                                FROM SMSMPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG', 'WE') AND SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    GetDt("2", ShipmentId, ref dt);

                    partyResult = dt;
                    
                    sql = "SELECT INVOICE_RMK,PACKING_RMK,INV_INTRU,PKG_INTRU,BANK_MSG FROM SMINM  A WHERE EXISTS( SELECT B.* FROM SMINM B WHERE B.SHIPMENT_ID='{0}' AND B.COMBINE_INFO LIKE '%'+ A.DN_NO+'%'  )";
                    sql = string.Format(sql, ShipmentId);
                    dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    InvoiceIntruction = GetRemark(dt, "INV_INTRU");
                    InvoiceRemark = GetRemark(dt, "INVOICE_RMK");
                    PackingRemark = GetRemark(dt, "PACKING_RMK");
                    PackingIntruction = GetRemark(dt, "PKG_INTRU");

                }
            }
            else if (DnNo != "")
            {
                string sqlprofile = "SELECT PROFILE_CD FROM SMDN WHERE DN_NO=" + SQLUtils.QuotedStr(DnNo);
                string _profilecode = OperationUtils.GetValueAsString(sqlprofile, Prolink.Web.WebContext.GetInstance().GetConnection());
                sqlprofile = string.Format("SELECT * FROM SMSIM WHERE PROFILE={0}", SQLUtils.QuotedStr(_profilecode));
                DataTable dt = OperationUtils.GetDataTable(sqlprofile, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    InvoiceIntruction = Prolink.Math.GetValueAsString(row["INV_SPEC"]);
                    PackingIntruction = Prolink.Math.GetValueAsString(row["PK_SPEC"]);
                    InvoiceRemark = GetInvRemark(row);
                    PackingRemark = GetPackingRemark(row);
                }
               
                sql = @"SELECT TOP 1 
                            VAT_NO,
                            DN_NO,
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
                dnResult = dnDt;
                sql = @"SELECT PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                CONTACT AS PARTY_ATTN, TEL AS PARTY_TEL, FAX_NO 
                                FROM SMDNPT  WHERE PARTY_TYPE IN('SH', 'CS', 'NT', 'RE', 'AG','WE') AND DN_NO=" + SQLUtils.QuotedStr(DnNo);
                DataTable partdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());


                GetDt("1", DnNo, ref partdt);

                partyResult = partdt;
                if (ShipmentId != "")
                {
                    sql = @"SELECT TOP 1 A.*,  
                        (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD) AS CNTRY_ORN, 
                        (SELECT TOP 1 CNTRY_NM FROM BSCNTY D WHERE (SELECT TOP 1 CNTRY_CD FROM BSCITY C WHERE C.CNTRY_CD+C.PORT_CD=A.POL_CD)=D.CNTRY_CD) AS CNTRY_DESCP
                        FROM SMSM A  WHERE A.SHIPMENT_ID=" + SQLUtils.QuotedStr(ShipmentId);
                    DataTable sdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                    GetRef(ref sdt);
                    Getblno(ref sdt);
                    shipResult = sdt;
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

                        GetRef(ref sdt);
                        Getblno(ref sdt);
                        shipResult = sdt;
                    }
                }
            }

            //shipResult = new DataTable();
            //partyResult = new DataTable();
            //dnResult = new DataTable();
            EditInstruct invEi=new EditInstruct("SMINM",EditInstruct.UPDATE_OPERATION);
            invEi.PutKey("U_ID",UId);
            UpdateParty(partyResult,ref invEi);
            UpdateSminmByShipInfo(shipResult,ref invEi);
            Dictionary<string, object> parm=new Dictionary<string,object>();
            parm["InvoiceIntruction"]=InvoiceIntruction;
             parm["InvoiceRemark"]=InvoiceRemark;
             parm["PackingRemark"]=PackingRemark;
             parm["PackingIntruction"]=PackingIntruction;
            UpdateSminmByDnInfo(dnResult,ref invEi,parm);
            MixedList mlist = new MixedList();
            mlist.Add(invEi);
            try
            {
                int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                return DnNo+":"+ex.ToString()+";";
            }
            return "";
        }

        public void UpdateParty(DataTable partyData,ref EditInstruct invEi)
        {
            string partyno=string.Empty;
            string partynm=string.Empty;
            string partaddr=string.Empty;
            string partyattn=string.Empty;
            string partytel=string.Empty;
            string faxno=string.Empty;

            foreach(DataRow partyrow in partyData.Rows)
            {
                partyno=partyrow["PARTY_NO"].ToString();
                partynm=partyrow["PARTY_NAME"].ToString();
                partaddr=partyrow["PARTY_NAME"].ToString();
                partyattn=partyrow["PARTY_ATTN"].ToString();
                partytel=partyrow["PARTY_TEL"].ToString();
                faxno=partyrow["FAX_NO"].ToString();
                if (partyrow["PARTY_TYPE"].ToString() == "SH")//== "SH"
                {
                    invEi.Put("SHPR_CD", partyno);
                    invEi.Put("SHPR_NM",partynm);
                    invEi.Put("Shpr_Addr",partaddr);
                    invEi.Put("Shpr_Attn",partyattn);
                    invEi.Put("Shpr_Tel",partytel);
                    invEi.Put("Shpr_Fax",faxno);
                }
                if (partyrow["PARTY_TYPE"].ToString() == "CS")
		    		{
		    			invEi.Put("Cnee_Cd",partyno);
		    			invEi.Put("Cnee_Nm",partynm);
		    			invEi.Put("Cnee_Addr",partaddr);
		    			invEi.Put("Cnee_Attn",partyattn);
		    			invEi.Put("Cnee_Tel",partytel);
		    			invEi.Put("Cnee_Fax",faxno);
		    		}

                if (partyrow["PARTY_TYPE"].ToString() == "AG")
		    		{
		    			invEi.Put("Cust_Cd",partyno);
		    			invEi.Put("Cust_Nm",partynm);
		    			invEi.Put("Cust_Addr",partaddr);
		    			invEi.Put("Cust_Attn",partyattn);
		    			invEi.Put("Cust_Tel",partytel);
		    			invEi.Put("Cust_Fax",faxno);
		    		}

                if (partyrow["PARTY_TYPE"].ToString() == "NT")
		    		{
		    			invEi.Put("Notify_No",partyno);
		    			invEi.Put("Notify_Nm",partynm);
		    		}

                if (partyrow["PARTY_TYPE"].ToString() == "RE")
		    		{
		    			invEi.Put("Bill_To",partyno);
		    			invEi.Put("Bill_Nm",partynm);
		    			invEi.Put("Bill_Addr",partaddr);
		    			invEi.Put("Bill_Attn",partyattn);
		    			invEi.Put("Bill_Tel",partytel);
		    			invEi.Put("Bill_Fax",faxno);
		    		}

                if (partyrow["PARTY_TYPE"].ToString() == "WE")
		    		{
		    			invEi.Put("Ship_To",partyno);
		    			invEi.Put("Ship_Nm",partynm);
		    			invEi.Put("Ship_Addr",partaddr);
		    			invEi.Put("Ship_Attn",partyattn);
		    			invEi.Put("Ship_Tel",partytel);
		    			invEi.Put("Ship_Fax",faxno);
		    		}
            }

        }

        public void UpdateSminmByShipInfo(DataTable shipData, ref EditInstruct invEi)
        {
            if (shipData.Rows.Count <= 0) return;
            DataRow smrow = shipData.Rows[0];
            string DnNo = string.Empty;
            invEi.PutDate("ETD", smrow["ETD"]);
            invEi.PutDate("ETA", smrow["ETA"]);
            if (smrow["ETD"] != null && smrow["ETD"] != DBNull.Value)
            {
                invEi.PutDate("INV_DATE", Prolink.Math.GetValueAsDateTime(smrow["ETD"]));
            }
            if (smrow["VESSEL1"] != null)
            {
                invEi.Put("VESSEL_NM", smrow["VESSEL1"]);
            }

            if (smrow["VOYAGE1"] != null)
            {
                invEi.Put("VESSEL_NM", smrow["VOYAGE1"]);
            }

            if (smrow["VESSEL1"] != null && smrow["VOYAGE1"] != null)
            {
                invEi.Put("VESSEL_NM", smrow["VESSEL1"] + "/" + smrow["VOYAGE1"]);
            }
            if (string.IsNullOrEmpty(smrow["House_No"].ToString()))
            {
                invEi.Put("BL_NO", smrow["Master_No"]);
            }
            else
            {
                invEi.Put("BL_NO", smrow["House_No"]);
            }
		    		
            invEi.Put("REF_NO", smrow["REF_NO"]);
            invEi.Put("FROM_CD", smrow["Pol_Cd"]);
            invEi.Put("FROM_DESCP", smrow["Pol_Name"]);
            invEi.Put("TO_CD", smrow["Dest_Cd"]);
            invEi.Put("TO_DESCP", smrow["Dest_Name"]);
            invEi.Put("Incoterm", smrow["Incoterm_CD"]);
            invEi.Put("Incoterm_Descp", smrow["Incoterm_Descp"]);
            invEi.Put("Combine_Info", smrow["Combine_Info"]);
            invEi.PutDate("Shipping_Date", smrow["Etd"]);
            invEi.Put("Shipment_Id", smrow["Shipment_Id"]);
            invEi.Put("Cmdty_Cd", smrow["Goods"]);
            invEi.Put("Lgoods", smrow["Lgoods"]);
            invEi.Put("Trade_Term", smrow["Trade_Term"]);
            invEi.Put("Tradeterm_Descp", smrow["Tradeterm_Descp"]);
            invEi.Put("Ttl_Plt", smrow["Pkg_Num"]);
            invEi.Put("PLTU", smrow["PKG_UNIT"]);
            invEi.Put("QTYU", smrow["Qtyu"]);
            if (DnNo == "")
            {
                invEi.Put("Marks", smrow["Marks"]);
            }

            invEi.Put("Cntry_Orn", smrow["Cntry_Orn"]);
            invEi.Put("Cntry_Descp", smrow["Cntry_Descp"]);
            //invEi.Put("Bank_Msg", smrow["Memo"]);
            invEi.Put("Transacte_Mode", smrow["Transacte_Mode"]);
            invEi.Put("Pkg_Unit_Desc", smrow["Pkg_Unit_Desc"]);

            string trantype = Prolink.Math.GetValueAsString(smrow["TRAN_TYPE"]);
            if ("F".Equals(trantype) || "L".Equals(trantype))
            {
                invEi.Put("Dlv_Way", "By Sea");
            }
            else if ("A".Equals(trantype))
            {
                invEi.Put("Dlv_Way", "By Air");
            }
            else if ("D".Equals(trantype) ||  "E".Equals(trantype))
            {
                invEi.Put("Dlv_Way", "By Express");
            }
            else if ("R".Equals(trantype))
            {
                invEi.Put("DLV_WAY", "By Railroad");
            }

            var FreightAmt = Prolink.Math.GetValueAsDecimal(smrow["FREIGHT_AMT"]);
            if (FreightAmt == 0)
                FreightAmt = 0;
            var f_fee = getFreightFee(FreightAmt, smrow["TRADE_TERM"].ToString());
            //invEi.Put("FREIGHT_FEE", f_fee);
            setTtlValue(ref invEi, smrow["TRADE_TERM"].ToString());
        }

        public void UpdateSminmByDnInfo(DataTable dnData, ref EditInstruct invEi, Dictionary<string, object> parm)
        {
            DataRow dnRow = dnData.Rows[0];
            string DnNo = dnRow["DN_NO"].ToString();
            invEi.Put("Vat_No", dnRow["Vat_No"]);
            var RefNo = dnRow["Dn_No_Cmp_Ref"].ToString();
            if (RefNo != "" && RefNo != null)
            {
                invEi.Put("Ref_No", RefNo.Substring(4, RefNo.Length-4));
            }
            if (DnNo != "" && DnNo != null)
            {
                invEi.Put("Inv_No", DnNo.Substring(4, DnNo.Length-4));
                invEi.Put("PACKING_NO", DnNo.Substring(4, DnNo.Length - 4));
            }

            //if (dnRow["Act_Post_Date"] != null && dnRow["Act_Post_Date"] != DBNull.Value)
            //{
            //    invEi.PutDate("INV_DATE", Prolink.Math.GetValueAsDateTime(dnRow["Act_Post_Date"]));
            //}
            invEi.Put("Bank_Msg", dnRow["Memo"]);
            invEi.Put("Lgoods", dnRow["Lgoods"]);
            //invEi.Put("Etd",dnRow["DnEtd);
            invEi.Put("Incoterm", dnRow["Incoterm"]);
            invEi.Put("Incoterm_Descp", dnRow["Incoterm_Descp"]);
            invEi.Put("Trade_Term", dnRow["Trade_Term"]);
            invEi.Put("Tradeterm_Descp", dnRow["Tradeterm_Descp"]);
            invEi.Put("Transacte_Mode", dnRow["Transacte_Mode"]);
            if (DnNo != "")
            {
                string sqlup = string.Format("SELECT UPLOAD_BY FROM SMINM WHERE DN_NO={0}", SQLUtils.QuotedStr(DnNo));
                string uploadby = OperationUtils.GetValueAsString(sqlup, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (string.IsNullOrEmpty(uploadby))
                {
                    invEi.Put("Marks", dnRow["Marks"]);
                    invEi.Put("Pkg_Unit_Desc", dnRow["Pkg_Unit_Desc"]);
                    invEi.Put("Ttl_Plt", dnRow["Pkg_Num"]);
                }
                invEi.Put("Cmdty_Cd", dnRow["Goods"]);

                decimal FreightAmt = Prolink.Math.GetValueAsDecimal(dnRow["Freight_Amt"]);
                var f_fee = getFreightFee(FreightAmt, invEi.Get("Trade_Term"));
                invEi.Put("Freight_Fee", f_fee);
                setTtlValue(ref invEi, invEi.Get("Trade_Term"));
            }
            invEi.Put("Invoice_Rmk", parm["InvoiceRemark"].ToString());
            invEi.Put("Inv_Intru", parm["InvoiceIntruction"].ToString());
            invEi.Put("Packing_Rmk", parm["PackingRemark"].ToString());
            invEi.Put("Pkg_Intru", parm["PackingIntruction"].ToString());

            string sql = string.Format("SELECT SO_NO,AMT,CUR1 FROM SMIND WHERE DN_NO={0} ORDER BY SEQ_NO ASC", SQLUtils.QuotedStr(DnNo));
            DataTable indDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            double TtlValue = Prolink.Math.GetValueAsDouble(indDt.Compute("Sum(AMT)", "1=1"));
            string sono = string.Empty;
            string cur = string.Empty;
            if (indDt.Rows.Count > 0)
            {
                sono = indDt.Rows[0]["SO_NO"].ToString();
                cur = indDt.Rows[0]["CUR1"].ToString();
                invEi.Put("So_No", sono);
                invEi.Put("Cur", cur);
            }
            invEi.Put("TTL_VALUE", TtlValue);
            if (TtlValue != 0)
            {
                setTtlValue(ref invEi, invEi.Get("Trade_Term"));
            }
            caluFobValue(ref invEi);
            setUnit(ref invEi);
        }

        public void setUnit(ref EditInstruct invEi)
        {
            string Nwu = invEi.Get("NWU");
            string Pltu = invEi.Get("PLTU");
            string Qtyu = invEi.Get("QTYU");

            if (string.IsNullOrEmpty(Nwu))
                invEi.Put("NWU", "KG");
            if (string.IsNullOrEmpty(Nwu))
                invEi.Put("PLTU", "PLT");
            if (string.IsNullOrEmpty(Qtyu))
                invEi.Put("QTYU", "CTN");
        }

        public void setTtlValue(ref EditInstruct invEi,string tradeterm )
        {
            double TtlValue = Prolink.Math.GetValueAsDouble(invEi.Get("TTL_VALUE"));
            string year = Prolink.Math.GetValueAsString(invEi.Get("ETD"));//**********************************************Year
            if (year.Length > 4)
            {
                year = year.Substring(0, 4);
            }
            string sql = "SELECT CD_DESCP FROM BSCODE WHERE GROUP_ID='TPV' AND CD_TYPE='TIR' AND CD=" + SQLUtils.QuotedStr(year);
            string cddescp = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            double tir = Prolink.Math.GetValueAsDouble(cddescp);
            double IssueFee = TtlValue * 1.1 * tir;

            if (IssueFee > 0 && IssueFee <= 1)
            {
                IssueFee = 1;
            }
            else if (IssueFee > 1)
            {
                IssueFee = Math.Round(IssueFee);
            }
            var i_fee = getIssueFee(IssueFee, tradeterm);
            invEi.Put("ISSUE_FEE", i_fee);
            caluFobValue( ref invEi);
        }

public void caluFobValue(ref EditInstruct invEi)
{
    var FreightFee =Prolink.Math.GetValueAsDouble(invEi.Get("FREIGHT_FEE"));
	var IssueFee = Prolink.Math.GetValueAsDouble(invEi.Get("ISSUE_FEE"));
	var TtlValue = Prolink.Math.GetValueAsDouble(invEi.Get("TTL_VALUE"));
	double FobValue = 0;
	FobValue = TtlValue - FreightFee - IssueFee;
    invEi.Put("FOB_VALUE",FobValue);
}


        public decimal getFreightFee(decimal FrieghtFee, string tradeTerm)
        {
            switch (tradeTerm)
            {
                case "C&I":
                    return 0;
                    break;
                case "EXW":
                    return 0;
                    break;
                case "FAS":
                    return 0;
                    break;
                case "FCA":
                    return 0;
                    break;
                case "FH":
                    return 0;
                    break;
                case "FOB":
                    return 0;
                default:
                    return FrieghtFee;
                    break;
            }
        }

        public double getIssueFee(double IssueFee, string tradeTerm)
        {
            switch (tradeTerm)
            {
                case "CFR":
                    return 0;
                    break;
                case "CPT":
                    return 0;
                    break;
                case "EXW":
                    return 0;
                    break;
                case "FCA":
                    return 0;
                    break;
                case "FAS":
                    return 0;
                    break;
                case "FH":
                    return 0;
                    break;
                case "FOB":
                    return 0;
                    break;
                default:
                    return IssueFee;
                    break;
            }
        }

        public void GetRef(ref DataTable dt)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string combineinfo = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]);//CombineInfo/REF_NO
                string val = string.Empty;
                if (!string.IsNullOrEmpty(combineinfo))
                {
                    var list = combineinfo.Split(',');
                    foreach (string str in list)
                    {
                        if (str.Length > 10)
                        {
                            val += str.Substring(4, str.Length - 4) + ",";
                            continue;
                        }
                        val += str + ",";
                    }
                }
                if (!string.IsNullOrEmpty(val))
                {
                    val = val.Substring(0, val.Length - 1);
                    dr["REF_NO"] = val;
                }
                else
                {
                    string dnno = Prolink.Math.GetValueAsString(dr["DN_NO"]);
                    dr["REF_NO"] = dnno.Substring(4, dnno.Length - 4);
                }
            }
        }

        public void Getblno(ref DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string combineshipment = Prolink.Math.GetValueAsString(dt.Rows[i]["COMBIN_SHIPMENT"]);
                if (!string.IsNullOrEmpty(combineshipment))
                {
                    string sql = string.Format(@"SELECT TOP 1 MASTER_NO,HOUSE_NO FROM SMSM WHERE SHIPMENT_ID='{0}' ORDER BY ISCOMBINE_BL", combineshipment);
                    DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr1 in dt1.Rows)
                    {
                        string master_no = Prolink.Math.GetValueAsString(dr1["MASTER_NO"]);
                        string house_no = Prolink.Math.GetValueAsString(dr1["HOUSE_NO"]);
                        if (!string.IsNullOrEmpty(master_no) || !string.IsNullOrEmpty(house_no))
                        {
                            dt.Rows[i]["MASTER_NO"] = master_no;
                            dt.Rows[i]["HOUSE_NO"] = house_no;
                        }
                        break;
                    }
                }

            }
        }

        public void GetDt(string type, string data, ref DataTable dt)
        {
            string sql = "";
            switch (type)
            {
                case "1": sql = @"SELECT TOP 1 STN FROM SMDN WHERE DN_NO_CMP_REF IS NOT NULL AND DN_NO=" + SQLUtils.QuotedStr(data); break;
                case "2": sql = @"SELECT TOP 1 INV_COMBINE_FLAG FROM SMINM WHERE INV_COMBINE_FLAG IS NOT NULL AND SHIPMENT_ID=" + SQLUtils.QuotedStr(data); break;
            }
            string partno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());

            if (!string.IsNullOrEmpty(partno))
            {
                sql = @"SELECT TOP 1 PARTY_TYPE, PARTY_NO, CONCAT(PARTY_NAME, ' ', PARTY_NAME2, ' ', PARTY_NAME3, ' ', PARTY_NAME4) AS PARTY_NAME,
                                CONCAT(PART_ADDR1,' ',PART_ADDR2,' ',PART_ADDR3,' ',PART_ADDR4,' ',PART_ADDR5) AS PART_ADDR,
                                PARTY_ATTN, PARTY_TEL,PARTY_FAX AS FAX_NO  FROM SMPTY WHERE STATUS='U' AND PARTY_NO=" + SQLUtils.QuotedStr(partno);
                DataTable partdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                if (partdt.Rows.Count > 0)
                {
                    var newdt = from k in dt.AsEnumerable() where k.Field<string>("PARTY_TYPE") != "SH" select k;
                    DataTable dt2 = newdt.CopyToDataTable();
                    var st = partdt.Select("PARTY_TYPE LIKE '%SH%'");
                    if (st.Length > 0)
                    {
                        st[0]["PARTY_TYPE"] = "SH";
                        dt2.Rows.Add(st[0].ItemArray);
                        dt = dt2;
                        return;
                    }
                    else
                    {
                        partdt.Rows[0]["PARTY_TYPE"] = "SH";
                        dt2.Rows.Add(partdt.Rows[0].ItemArray);
                        dt = dt2;
                    }
                }
            }
        }

        public string GetRemark(DataTable dt, string type)
        {
            var invoiceremark = dt.Select(type + " IS NOT NULL ");
            string val = string.Empty;
            if (invoiceremark.Length > 0)
            {
                foreach (DataRow dr in invoiceremark)
                {
                    return val = Prolink.Math.GetValueAsString(dr[type]);
                }
            }
            return val;
        }

        protected string GetInvRemark(DataRow simRow)
        {
            List<string> items = new List<string>();
            Action<string> add = (txt) =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                if (items.Contains(txt)) return;
                items.Add(txt);
            };
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK1"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK2"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK3"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK4"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK5"]));
            add(Prolink.Math.GetValueAsString(simRow["INV_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }
        protected string GetPackingRemark(DataRow simRow)
        {
            List<string> items = new List<string>();
            Action<string> add = (txt) =>
            {
                if (string.IsNullOrEmpty(txt)) return;
                if (items.Contains(txt)) return;
                items.Add(txt);
            };
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK1"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK2"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK3"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK4"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK5"]));
            add(Prolink.Math.GetValueAsString(simRow["PK_REMARK6"]));
            return string.Join(Environment.NewLine, items);
        }


        public EDOCFileItem UploadFile2EDOC(string jobNo, string filePath, string groupId, string cmp, string stn, string userid, string fileType = "RFQ", string remark = "", string dep = "")
        {
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            EDOCFileItem data = null;
            EDOCResultUploadFile uploadResult = null;
            try
            {
                //_api.Login();.
                int serverNum = 0;
                string folderID = CheckDuplicatedFolder(jobNo, groupId, cmp, stn, "",ref serverNum);
                if (string.IsNullOrEmpty(folderID))
                {
                    return data;
                }
                uploadResult = _api.UploadFile(folderID, filePath, remark, userid, "D", "8",serverNum);
                //uploadResult = _api.UploadFile(jobNo, filePath, "", userId, "D", "8");
                OperationUtils.Logger.WriteLog("UploadFile2EDOC=" + jobNo + groupId + cmp + stn);
                if (uploadResult != null && uploadResult.Status == DBErrors.DB_SUCCESS)
                {
                    //InitGrid();
                    data = uploadResult.FileInfo;
                    //fileType
                    List<UpdateFileItem> fileList = new List<UpdateFileItem>();
                    fileList.Add(new UpdateFileItem
                    {
                        FileID = data.FileID,
                        EdocType = fileType,
                        Remark = remark
                    });

                    bool isSuccess = _api.UpdateFiles(fileList, serverNum);
                }
            }
            catch (Exception ex)
            {
                OperationUtils.Logger.WriteLog("UploadFile2EDOC=" + jobNo + groupId + cmp + stn + ex.ToString());
            }
            return data;
        }

        public string CheckDuplicatedFolder(string job_no, string groupID, string cmp, string stn, string dep, ref int serverNum)
        {
            string folderID = "";
            try
            {
                folderID = _api.GetFolderGUID(job_no, groupID, cmp, stn, dep, ref serverNum, "FOLDER_GUID,SERVER_NUM");
                if (string.IsNullOrEmpty(folderID))
                {
                    folderID = _api.SetNewFolder(ref serverNum); //沒查到才向EDOC索取並寫進table
                    if (!string.IsNullOrEmpty(folderID))
                        _api.SetFolderIDInDB(job_no, folderID, groupID, cmp, stn, dep, true, serverNum);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (message.Length > 500) message = message.Substring(0, 500);
                EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", System.Guid.NewGuid().ToString());
                ei.Put("EDI_ID", "GetFolderGuid");
                ei.PutExpress("EVENT_DATE", "getdate()");
                ei.Put("REMARK", message);
                ei.Put("SENDER", "Edoc Server");
                ei.Put("RS", "Receive");
                ei.Put("STATUS", "Exception");
                ei.Put("FROM_CD", "Edoc Server");
                ei.Put("TO_CD", "Eshipping");
                ei.Put("DATA_FOLDER", "");
                ei.Put("REF_NO", job_no);
                ei.Put("GROUP_ID", "TPV");
                ei.Put("CMP", cmp);
                ei.Put("STN", stn);
                OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
            }          
            return folderID;
        }

        public static void doPoEdocCopy(string selUids, string jobNo, string groupId, string cmp, string stn, string fileId, string edoctype, string remark)
        {
            string[] UIds = selUids.Split(',');
            string[] cmps = cmp.Split(',');
            MixedList mlist = new MixedList();
            Prolink.EDOC_API _api = new Prolink.EDOC_API();
            string sourceFileId = fileId;
            string targetGuid = "";

            //update temp type to PO
            List<EDOCApi.UpdateFileItem> fileList = new List<EDOCApi.UpdateFileItem>();
            fileList.Add(new EDOCApi.UpdateFileItem
            {
                FileID = sourceFileId,
                EdocType = edoctype,//"PO",
                Remark = remark//"BATCH PO UPLOAD"
            });
            _api.UpdateFiles(fileList);
            string company=cmp;
            for (int i = 0; i < UIds.Length; i++)
            {
                if (string.IsNullOrEmpty(UIds[i]))
                {
                    continue;
                }
                else
                {
                    if (cmps.Length > i + 1)
                        company = cmps[i];
                    else
                        company = cmp;
                    try
                    {
                        targetGuid = _api.GetFolderGUID(UIds[i], groupId, company, stn, "");
                        if (string.IsNullOrEmpty(targetGuid))
                        {
                            targetGuid = _api.SetNewFolder();
                            if (!string.IsNullOrEmpty(targetGuid))
                                _api.SetFolderIDInDB(UIds[i], targetGuid, groupId, company, stn, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        if (message.Length > 500) message = message.Substring(0, 500);
                        EditInstruct ei = new EditInstruct("EDI_LOG", EditInstruct.INSERT_OPERATION);
                        ei.Put("U_ID", System.Guid.NewGuid().ToString());
                        ei.Put("EDI_ID", "GetFolderGuid");
                        ei.PutExpress("EVENT_DATE", "getdate()");
                        ei.Put("REMARK", message);
                        ei.Put("SENDER", "Edoc Server");
                        ei.Put("RS", "Receive");
                        ei.Put("STATUS", "Exception");
                        ei.Put("FROM_CD", "Edoc Server");
                        ei.Put("TO_CD", "Eshipping");
                        ei.Put("DATA_FOLDER", "");
                        ei.Put("REF_NO", UIds[i]);
                        ei.Put("GROUP_ID", "TPV");
                        ei.Put("CMP", cmp);
                        ei.Put("STN", stn);
                        mlist.Add(ei);
                    }
                    if (string.IsNullOrEmpty(targetGuid))
                    {
                        continue;
                    }
                    EDOCApi.EDOCAgent.Agent.DoCopyFile(sourceFileId, targetGuid);
                }
            }
            if (mlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {

                }

            }
            //delete temp file
            _api.DeleteFile(sourceFileId);
        }

    }

    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string EdocUrl { get; set; }
    }

    public class EdocParm
    {
       public string ReportId { get; set; }
       public string ConditionString { get; set; }
       public string ExportFileType { get; set; }
       public string ReportName { get; set; }
       public string JobNo { get; set; }
       public string GroupId { get; set; }
       public string Cmp { get; set; }
       public string Stn { get; set; }
       public string Dep { get; set; }
       public string FileType { get; set; }
       public string Remark { get; set; }
       public string Shipmentid { get; set; }
       public string Sql  { get; set; }
       public string Userid { get; set; }
       public string UrlParms { get; set; }
       public byte[] Bytes { get; set; }

       public EdocParm(Dictionary<string, object> parm)
       {
           ReportId = Prolink.Math.GetValueAsString(parm["reportId"]);
           ConditionString = Prolink.Math.GetValueAsString(parm["conditionString"]);
           ExportFileType = Prolink.Math.GetValueAsString(parm["exportFileType"]);
           ReportName = Prolink.Math.GetValueAsString(parm["reportName"]);
           JobNo = Prolink.Math.GetValueAsString(parm["jobNo"]);
           GroupId = Prolink.Math.GetValueAsString(parm["GroupId"]);
           Cmp = Prolink.Math.GetValueAsString(parm["CMP"]);
           Stn = "*";
           Dep = "";// Request.Params["DEP"];
           FileType = Prolink.Math.GetValueAsString(parm["fileType"]);
           Remark = Prolink.Math.GetValueAsString(parm["Remark"]);
           Shipmentid = Prolink.Math.GetValueAsString(parm["ShipmentId"]);
           Sql = string.Empty;
           try
           {
               Userid = Prolink.Math.GetValueAsString(parm["userid"]);
           }catch(Exception ex){
               Userid = string.Empty;
           }

           if (!string.IsNullOrEmpty(Shipmentid))
           {
               string sql = string.Format("SELECT U_ID,GROUP_ID,CMP,BL_WIN,COMBINE_INFO FROM SMSM WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(Shipmentid).Trim());
               DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
               if (dt.Rows.Count > 0)
               {
                   DataRow dr = dt.Rows[0];
                   JobNo = Prolink.Math.GetValueAsString(dr["U_ID"]);
                   GroupId = Prolink.Math.GetValueAsString(dr["GROUP_ID"]);
                   Cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                   //CombineCount = Prolink.Math.GetValueAsString(dr["COMBINE_INFO"]).Split(',').Length;
                   string blwin = Prolink.Math.GetValueAsString(dr["BL_WIN"]).Split(' ')[0];
                   if (!string.IsNullOrEmpty(blwin))
                       Userid = blwin;
               }
           }
           UrlParms = string.Format("rptdescp={0}&rptName={1}&formatType={2}&exportType=DOWNLOAD&conditions={3}",
                   ReportName,
                   ReportId,
                   ExportFileType,
                   System.Web.HttpUtility.UrlEncode(ConditionString));
           Bytes = Encoding.ASCII.GetBytes(UrlParms);
       }
    }

    public class V3Result
    {
        public string URL { get; set; }
    }
}
