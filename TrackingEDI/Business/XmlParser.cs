using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Persistence;
using System.IO;

namespace TrackingEDI.Business
{
    public class XmlParser : Parser
    {
        /// <summary>
        /// 警告消息
        /// </summary>
        private StringBuilder warningMessager = new StringBuilder();

        /// <summary>
        /// 获取警告
        /// </summary>
        public string Warning
        {
            get
            {
                return warningMessager.ToString();
            }
        }

        /// <summary>
        /// 分单号
        /// </summary>
        public string HouseNo
        {
            get;
            set;
        }

        /// <summary>
        /// 主单号
        /// </summary>
        public string MasterNo
        {
            get;
            set;
        }

        public string JobNo
        {
            get;
            set;
        }

        #region 基础数据
        /// <summary>
        /// 货况 栏位映射关系
        /// </summary>
        static readonly string StatusMapping = "StatusMapping";
        static readonly string StatusRoot = "Status";

        /// <summary>
        /// Po 栏位映射关系
        /// </summary>
        static readonly string PoMapping = "PoMapping";
        static readonly string PoRoot = "PO/Rec";

        /// <summary>
        /// Container 栏位映射关系
        /// </summary>
        static readonly string ContainerMapping = "ContainerMapping";
        /// <summary>
        /// Container 根节点
        /// </summary>
        static readonly string ContainerRoot = "Container/Rec";

        /// <summary>
        /// 空运提单 栏位映射关系
        /// </summary>
        static readonly string AirMapping = "AirMapping";
        /// <summary>
        /// 空运提单根节点
        /// </summary>
        static readonly string AirRoot = "AirCargoBill";

        /// <summary>
        /// 海运提单 栏位映射关系
        /// </summary>
        static readonly string OceanMapping = "OceanMapping";
        /// <summary>
        /// 海运提单根节点
        /// </summary>
        static readonly string OceanRoot = "OceanCargoBill";
        #endregion

        public static string RuntimePath
        {
            get
            {
                string path = "";
                if (System.Environment.CurrentDirectory == AppDomain.CurrentDomain.BaseDirectory)//Windows应用程序则相等
                {
                    path = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    path = AppDomain.CurrentDomain.BaseDirectory + @"bin\";
                }
                return path;
            }
        }

        #region 货况保存
        /// <summary>
        /// 货况保存
        /// </summary>
        /// <param name="xml"></param>
        public void SaveStatus(string xml)
        {
            //SaveFileLog(xml, "FromCust");

            XmlDocument receiverXML = new XmlDocument();
            receiverXML.LoadXml(xml);
            List<string> houseList = new List<string>();
            XmlNode xn = receiverXML.SelectSingleNode(StatusRoot);
            string houseNo = GetXmlNodeValue(xn, "AWBNo");
            HouseNo = houseNo;
            //MasterNo=GetXmlNodeValue(xn, "MasterNo");
            DataTable dt = GetTkblRow(houseNo);
            if (dt == null || dt.Rows.Count <= 0)
                throw new Exception(string.Format("HOUSE NO：{0} Not  Find", houseNo));
            DateTime create_date = DateTime.Now;
            foreach (DataRow dr in dt.Rows)
            {
                SaveStattus(xn, dr, create_date);
            }
            if (!string.IsNullOrEmpty(MasterNo))
                EvenFactory.AddOnceEven(MasterNo, MasterNo, EvenManager.StatusDateEven);
        }

        public static void SaveFileLog(string xml,string type="FromItrace",string filename="")
        {
            try
            {
                string path = System.IO.Path.Combine(RuntimePath, "../EDI_back");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                if (!string.IsNullOrEmpty(type))
                {
                    path = System.IO.Path.Combine(path, type);
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
                if (string.IsNullOrEmpty(filename))
                    filename = DateTime.Now.ToString("yyyyMMddHHmmfff");
                string file = System.IO.Path.Combine(path, filename + ".xml");
                File.WriteAllText(file, xml, Encoding.UTF8);
            }
            catch { }
        }

        private void SaveStattus(XmlNode xn, DataRow dr,DateTime date)
        {
            MixedList ml = new MixedList();
            EditInstruct ei = null;

            string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            string jobNO = Prolink.Math.GetValueAsString(dr["U_ID"]);
            string trans_flag = Prolink.Math.GetValueAsString(dr["TRANS_FLAG"]);
            string master_no = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
            string hbl_no = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);
            string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            MasterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
            JobNo = jobNO;
            //ei = ParseEditInstruct(xn, StatusMapping);

            ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
            DataTable containerDt =GetContainerByJobNo(jobNO);
            string containerNo = GetXmlNodeValue(xn, "containerNo");
            containerNo = TraceStatus.SetContainerNo(containerNo, ei);
            //if (!string.IsNullOrEmpty(containerNo) && !MasterNo.Equals(hbl_no))
            //{
            //    if (containerDt.Rows.Count > 0 && containerDt.Select(string.Format("CNTR_NO={0}", SQLUtils.QuotedStr(containerNo))).Length <= 0)
            //    {
            //        warningMessager.Append(string.Format("{0} CONTAINER NO is not in MASTER_NO:{0},HOUSE_NO:{1}", containerNo, master_no, jobNO));
            //        return;
            //    }
            //}

            DataTable statusDt = TraceStatus.GetStatusByJobNo(jobNO);
            string code = GetXmlNodeValue(xn, "Code");
            string event_descp = GetXmlNodeValue(xn, "Descp");
            string issingle = "N";
            DataTable stsDt = GetStatusInfo(code);

            if (stsDt.Rows.Count > 0)
            {
                code = Prolink.Math.GetValueAsString(stsDt.Rows[0]["STS_CD"]);
                issingle = GetBoolValue(Prolink.Math.GetValueAsString(stsDt.Rows[0]["ISSINGLE"]));          
            }
            if (string.IsNullOrEmpty(event_descp))
            {
                if (stsDt.Rows.Count > 0)
                    event_descp = Prolink.Math.GetValueAsString(stsDt.Rows[0]["EDESCP"]);
            }

            //if (string.IsNullOrEmpty(event_descp))
            //    event_descp = TrackingEDI.Utils.Database.GetValueAsString(string.Format("SELECT EDESCP FROM TKSTSCD WHERE STS_CD={0}", SQLUtils.QuotedStr(code)));
            //if ("A".Equals(tran_type))//1:Air  2:Sea LCL  3:Sea FCL 4:OExpress  5:DExpress 6:Dosmatics
            //{
            //    //问题单：97392  MODFIY BY FISH  2015-10-21 如果有包含貨況代碼是DEP的，要去覆蓋S14，ARR則覆蓋S15。
            //    if ("DEP".Equals(code))
            //    {
            //        code = "S14";
            //        event_descp = "ATD";
            //        EditInstruct ei1 = new EditInstruct("TKBLST", EditInstruct.DELETE_OPERATION);
            //        ei1.PutKey("STS_CD", "DEP");
            //        ei1.PutKey("U_ID", jobNO);
            //        ml.Add(ei1);
            //    }
            //    else if ("ARR".Equals(code))
            //    {
            //        code = "S15";
            //        event_descp = "ATA";
            //        EditInstruct ei1 = new EditInstruct("TKBLST", EditInstruct.DELETE_OPERATION);
            //        ei1.PutKey("STS_CD", "ARR");
            //        ei1.PutKey("U_ID", jobNO);
            //        ml.Add(ei);
            //    }
            //}

            ei.Put("SHIPMENT_ID", shipment_id);
            string remark = ei.Get("REMARK");
            if (string.IsNullOrEmpty(remark))
                ei.Remove("REMARK");
          
            //string statusSql = string.Format("SELECT U_ID,SEQ_NO FROM TKBLST WHERE STS_CD={0} AND U_ID={1}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(jobNO));
            string statusSql = string.Format("STS_CD={0} AND U_ID={1}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(jobNO));
            if (!"Y".Equals(issingle))
            {
                if (!string.IsNullOrEmpty(containerNo))
                    statusSql += " AND CNTR_NO=" + SQLUtils.QuotedStr(containerNo);
                else
                    statusSql += " AND (CNTR_NO='' OR CNTR_NO IS NULL)";
            }
            DataRow[] oldSdrs = statusDt.Select(statusSql);

            string SDate = GetXmlNodeValue(xn, "SDate");
            string STime = GetXmlNodeValue(xn, "STime");
            string locatioin = GetXmlNodeValue(xn, "Locatioin");
            string Locatioin_Name = GetXmlNodeValue(xn, "Locatioin_Name");
            string[] locatioinTimezone = TraceStatus.SetLocatioinTimezone(SDate, STime, locatioin, Locatioin_Name, ei);
            ei.Put("STS_CD", code);
            ei.Put("STS_DESCP", event_descp);

            string seq_no = TraceStatus.SetStatusKey(ei, oldSdrs, jobNO, locatioinTimezone[0], issingle, date);
            string u_id = ei.Get("U_ID");
            ParseDbEditInstruct(ei);
            ml.Add(ei);
            EvenFactory.AddEven(u_id + "#" + seq_no, u_id, "ST", ml);
            EvenFactory.AddOnceEven(string.Format("{0}#{1}#{2}", u_id, seq_no, code), u_id, EvenManager.StatusEven, ml);
            //EvenNotify.Notify(u_id, ml);
            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            //XmlParser.UpdateProccessStatus(u_id);
            //DocSender.Send(u_id, seq_no, code, dr);
        }

        #endregion

        #region 数据保存
        /// <summary>
        /// 更新提单Cargo Status 用ml的前提是要有数据
        /// 这里名字错了
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="ml"></param>
        public static void UpdateProccessStatus(string u_id, MixedList ml=null)
        {
            string sql = string.Format("SELECT * FROM (SELECT STS_CD,EVEN_DATE,EVEN_TMG,STS_DESCP FROM TKBLST WITH (NOLOCK) WHERE U_ID={0}) T  OUTER APPLY (SELECT LOCATION,LDESCP,EDESCP,ORDER_BY FROM TKSTSCD WITH (NOLOCK) WHERE LOCATION IS NOT NULL AND LOCATION<>'' AND TKSTSCD.STS_CD=T.STS_CD)M ORDER BY EVEN_DATE", SQLUtils.QuotedStr(u_id));
            DataTable mainDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            string final_location = string.Empty;
            string final_order_by = string.Empty;
            foreach (DataRow dr in mainDt.Rows)
            {
                string location = Prolink.Math.GetValueAsString(dr["LOCATION"]);
                string order_by = Prolink.Math.GetValueAsString(dr["ORDER_BY"]);
                if (string.IsNullOrEmpty(location))
                    continue;
                if (string.IsNullOrEmpty(final_location))
                {
                    final_location = location;
                    final_order_by = order_by;
                }
                else if (order_by.CompareTo(final_order_by) > 0)
                {
                    final_location = location;
                    final_order_by = order_by;
                }
            }
            if (string.IsNullOrEmpty(final_location))
                return;
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT TOP 1 CD,ORDER_BY FROM BSCODE WITH (NOLOCK) WHERE CD_TYPE='TKLC' AND CD IN (SELECT CSTATUS FROM TKBL WHERE U_ID={0})", SQLUtils.QuotedStr(u_id)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            string cstatus = string.Empty, corder_by = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                cstatus = Prolink.Math.GetValueAsString(dt.Rows[0]["CD"]);
                corder_by = Prolink.Math.GetValueAsString(dt.Rows[0]["ORDER_BY"]);
            }
            //string cstatus = OperationUtils.GetValueAsString(string.Format("SELECT TOP 1 LOCATION,LDESCP,EDESCP,ORDER_BY FROM TKSTSCD WITH (NOLOCK) WHERE STS_CD IN (SELECT CSTATUS FROM TKBL WHERE U_ID={0})", SQLUtils.QuotedStr(u_id)), Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(final_order_by))
            {
                if (string.IsNullOrEmpty(cstatus) || final_order_by.CompareTo(corder_by) > 0)
                {
                    cstatus = final_location;
                    EditInstruct ei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
                    ei.PutKey("U_ID", u_id);
                    ei.Put("CSTATUS", cstatus);
                    if (ml == null)
                        OperationUtils.ExecuteUpdate(ei, Prolink.Web.WebContext.GetInstance().GetConnection());
                    else
                        ml.Add(ei);
                }
            }
        }

        /// <summary>
        /// 作废tracking数据
        /// </summary>
        /// <param name="houseNo"></param>
        public void DeleteTrackingData(string houseNo)
        {
            //存储需要做删除操作的sql语句
            System.Text.StringBuilder delSb = new StringBuilder();
            delSb.AppendLine("BEGIN");
            //删除主表(TKBL)
            delSb.AppendLine(string.Format("DELETE FROM TKBL WHERE HOUSE_NO='{0}';", houseNo));
            //删除表TKBLPTY
            delSb.AppendLine(string.Format("DELETE FROM TKBLPT WHERE JOB_NO IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0});", SQLUtils.QuotedStr(houseNo)));
            //删除表TKBLFLNO
            delSb.AppendLine(string.Format("DELETE FROM TKBLFLNO WHERE JOB_NO IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0});", SQLUtils.QuotedStr(houseNo)));
            //删除表TKBLPO
            delSb.AppendLine(string.Format("DELETE FROM TKBLPO WHERE JOB_NO IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0});", SQLUtils.QuotedStr(houseNo)));
            //删除表TKBLCNTR
            delSb.AppendLine(string.Format("DELETE FROM TKBLCNTR WHERE JOB_NO IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0});", SQLUtils.QuotedStr(houseNo)));
            //删除表TKBLSTS
            delSb.AppendLine(string.Format("DELETE FROM TKBLST WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0});", SQLUtils.QuotedStr(houseNo)));
            delSb.AppendLine("END;");
            //批量执行sql语句
            OperationUtils.ExecuteUpdate(delSb.ToString(), Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 是否已经上传数据
        /// </summary>
        /// <param name="cmpId"></param>
        /// <returns></returns>
        public static bool HaveUploadData(string cmpId)
        {
            int day = 2; //设置要查找多少天以内的数据
            int rowNum = TrackingEDI.Utils.Database.GetValueAsInt(string.Format("SELECT COUNT(1) FROM TKBL,TKBLPT WHERE TKBL.U_ID=TKBLPT.U_ID AND TKBLPT.PARTY_NO='{0}' AND DATEDIFF(day,DATEADD(DAY,-{1},GETDATE()),TKBL.CREATE_DATE) BETWEEN 0 AND {1}", new object[] { cmpId, day - 1 }));
            return rowNum > 0 ? true : false;
        }

        /// <summary>
        /// 保存空运提单信息
        /// </summary>
        /// <param name="xml"></param>
        public void SaveAirCargoBill(string xml)
        {
            XmlDocument receiverXML = new XmlDocument();
            receiverXML.LoadXml(xml);
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            //保存新的数据
            //XmlNodeList xnlist = receiverXML.SelectNodes(PoRoot);
            List<string> houseList = new List<string>();
            XmlNode xn = receiverXML.SelectSingleNode(AirRoot);
            string houseNo = GetXmlNodeValue(xn, "HAWBNo");
            string expOrImp = GetXmlNodeValue(xn, "IO");
            HouseNo = houseNo;
            MasterNo = GetXmlNodeValue(xn, "MAWBNo");


            DataRow dr = GetTkblRow(houseNo, expOrImp, GetXmlNodeValue(xn, "Agent_CODE"), GetXmlNodeValue(xn, "OS_CODE"));
            ei = ParseEditInstruct(xn, AirMapping);

            SetPorts(dr, ei);

            //string tran_type = GetTranType(ei.Get("TRAN_TYPE"), "1");
            //ei.Put("TRAN_TYPE", tran_type);
            string onboard_date = ei.Get("ETD");
            if (!string.IsNullOrEmpty(onboard_date))
                ei.PutDate("SHIPPING_DATE", onboard_date);
            //进出口栏位
          
            switch (expOrImp)
            {
                case "O": //出口提单
                    ei.Put("TRANS_FLAG", "E");
                    break;
                case "I": //进口提单
                default:
                    ei.Put("TRANS_FLAG", "N");
                    break;
            }

            string JOB_NO = System.Guid.NewGuid().ToString("N");
            if (dr != null)
            {
                JOB_NO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                ei.Put("MODIFY_BY", "System");
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                ei.Put("CREATE_BY", "System");
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            JobNo = JOB_NO;
            ei.PutKey("U_ID", JOB_NO);
            ml.Add(ei);

            AddFlightData(receiverXML, ml, AirRoot, JOB_NO,ei);
            AddPartys(receiverXML, ml, JOB_NO, AirRoot);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

            SendItrace(xn);
        }

        /// <summary>
        /// //1:Air  2:Sea LCL  3:Sea FCL 4:OExpress  5:DExpress 6:Dosmatics
        /// </summary>
        /// <param name="tran_type"></param>
        /// <param name="nullValue"></param>
        /// <returns></returns>
        public string GetTranType(string tran_type, string nullValue, string chg_type = "")
        {
            if (string.IsNullOrEmpty(tran_type))
                return nullValue;
            if (string.IsNullOrEmpty(chg_type))
                chg_type = string.Empty;
            else
                chg_type = chg_type.Trim();
            switch (tran_type)
            {
                case "A":
                    return "1";
                case "S"://如果 前兩碼事 CY就是FCL，其他就是 LC  2:SeaLCL;3:SeaFCL
                    if (!string.IsNullOrEmpty(chg_type) && "F".Equals(chg_type))
                        return "3";
                    else
                        return "2";
            }
            return tran_type;
        }

        /// <summary>
        /// 发送到EDI HUB
        /// </summary>
        /// <param name="xn"></param>
        public void SendItrace(XmlNode xn)
        {
            StringBuilder sb = new StringBuilder();
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                if (!"P".Equals(GetXmlNodeValue(xn, "ITRACE")))
                    sb.Append("ITRACE节点不为P;");
                EditInstruct ei = new EditInstruct("TK_TRACETASK", EditInstruct.INSERT_OPERATION);
                string path = Prolink.Web.WebContext.GetInstance().GetProperty("itracePath");
                xmldoc.Load(path);

                XmlNode node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageSender");
                node.InnerText = "tracking";
                ei.Put("SENDER", node.InnerText);
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageReceiver");
                node.InnerText = "itrace";
                ei.Put("RECIEVER", node.InnerText);
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "MessageCode");
                string tran_type = GetXmlNodeValue(xn, "Type");
                if ("A".Equals(tran_type))
                    node.InnerText = "ITRACE_RQ_AIR";
                else if ("S".Equals(tran_type))
                    node.InnerText = "ITRACE_RQ_SEA";
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "IdentifyNo");
                node.InnerText = Guid.NewGuid().ToString("N");
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Group");
                node.InnerText = GetXmlNodeValue(xn, "GROUP_ID");
                ei.Put("GROUP_ID", node.InnerText);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("GROUP_ID节点为空;");
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Company");
                node.InnerText = GetXmlNodeValue(xn, "CMP");
                ei.Put("CMP_ID", node.InnerText);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("CMP节点为空;");
                node = GetXmlNode(xmldoc, "Itrace/HeaderInfo", "Station");
                node.InnerText = GetXmlNodeValue(xn, "STN");
                ei.Put("STN", node.InnerText);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("STN节点为空;");

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "CarrierCode");
                string master_no = string.Empty;
                if ("A".Equals(tran_type))
                    master_no = GetXmlNodeValue(xn, "MAWBNo");
                else if ("S".Equals(tran_type))
                    master_no = GetXmlNodeValue(xn, "MasterNo");

                if ("A".Equals(tran_type))
                {
                    if (master_no != null && master_no.Length > 3)
                        node.InnerText = master_no.Substring(0, 3);
                    else
                        node.InnerText = master_no;
                }
                else
                {
                    string scac = GetXmlNodeValue(xn, "Carrier_SCAC");
                    if (string.IsNullOrEmpty(scac))
                    {
                        if (master_no != null && master_no.Length > 4)
                            scac = master_no.Substring(0, 4);
                        else
                            scac = master_no;
                    }
                    node.InnerText = scac;
                }
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("CarrierCode(空运MasterNo前3码,海运取Carrier_SCAC节点)节点为空;");

                //node.InnerText = GetTableValue(ei, dt, "STN");

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "PortCode");
                node.InnerText = GetXmlNodeValue(xn, "FromCountry") + GetXmlNodeValue(xn, "FromCity");
                ei.Put("PORT_CD", GetXmlNodeValue(xn, "FromCity"));
                ei.Put("PORT_CNTY", GetXmlNodeValue(xn, "FromCountry"));
                if ("S".Equals(tran_type) && string.IsNullOrEmpty(node.InnerText))//(海運必填)PortCode 和 CarriderCode 這兩個字段不能同時為空
                    sb.Append("海運PortCode节点为空;");

                //node = GetXmlNode(xmldoc, "Itrace/MessageBody", "DockCode");
                //node = GetXmlNode(xmldoc, "Itrace/MessageBody", "EndDate");
                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "DataNo");
                node.InnerText = master_no;
                ei.Put("MASTER_NO", node.InnerText);
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("MASTER_NO节点为空;");

                //node = GetXmlNode(xmldoc, "Itrace/MessageBody", "IATACode");
                if ("A".Equals(tran_type))
                {
                    node = GetXmlNode(xmldoc, "Itrace/MessageBody", "FlightNo");
                    node.InnerText = GetXmlNodeValue(xn, "FlightNo1");
                    ei.Put("FLIGHT_NO", node.InnerText);
                    if (string.IsNullOrEmpty(node.InnerText))
                        sb.Append("空运FlightNo1节点为空;");
                    node = GetXmlNode(xmldoc, "Itrace/MessageBody", "FlightDt");
                    node.InnerText = GetXmlNodeValue(xn, "ETD_DATE1");
                    //ei.Put("FLIGHT_DATE", node.InnerText);
                    if (string.IsNullOrEmpty(node.InnerText))
                        sb.Append("空运FlightDt节点为空;");
                }

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "POLCode");
                node.InnerText = GetXmlNodeValue(xn, "FromCity");

                node = GetXmlNode(xmldoc, "Itrace/MessageBody", "MessageType");
                if ("A".Equals(tran_type))
                    node.InnerText = "A";
                else if ("S".Equals(tran_type))
                    node.InnerText = "O";
                if (string.IsNullOrEmpty(node.InnerText))
                    sb.Append("MessageType节点为空;");
                //发送
                if (sb.Length <= 0)
                {
                    string result = Serviceface.Serviceface.GetItrace().Login();
                    result = Serviceface.Serviceface.GetItrace().SendToItrace(xmldoc.InnerXml);
                    EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, "EDI HUB传送成功", this.HouseNo, "IP", this.JobNo);
                }
                else
                    EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, sb.ToString() + "=>取消发送EDI HUB", this.HouseNo, "IP", this.JobNo);
            }
            catch (Exception e)
            {
                sb.Append(e.Message + ";");
                EvenFactory.SaveLog(Serviceface.Serviceface.GetItrace().Url, xmldoc.InnerXml, sb.ToString() + "=>发送EDI HUB异常:" + sb.ToString(), this.HouseNo, "IP", this.JobNo, "N");
            }
        }

        /// <summary>
        /// 保存海运提单信息
        /// </summary>
        /// <param name="xml"></param>
        public void SaveOceanCargoBill(string xml)
        {
            XmlDocument receiverXML = new XmlDocument();
            receiverXML.LoadXml(xml);
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            //保存新的数据
            List<string> houseList = new List<string>();
            XmlNode xn = receiverXML.SelectSingleNode(OceanRoot);
            string houseNo = GetXmlNodeValue(xn, "HouseNo");
            HouseNo = houseNo;
            MasterNo = GetXmlNodeValue(xn, "MasterNo");
            string expOrImp = GetXmlNodeValue(xn, "IO");
            DataRow dr = GetTkblRow(houseNo, expOrImp, GetXmlNodeValue(xn, "Agent_CODE"), GetXmlNodeValue(xn, "OS_CODE"));
            ei = ParseEditInstruct(xn, OceanMapping);

            SetPorts(dr, ei);

            //string tran_type = GetTranType(ei.Get("TRAN_TYPE"), "2", GetXmlNodeValue(xn, "CHG_TYPE"));
            //ei.Put("TRAN_TYPE", tran_type);
            string onboard_date = ei.Get("ETD");
            if (!string.IsNullOrEmpty(onboard_date))
                ei.PutDate("SHIPPING_DATE", onboard_date);

            SetContainerQty(ei, xn);
            //进出口栏位
          
            switch (expOrImp)
            {
                case "O": //出口提单
                    ei.Put("TRANS_FLAG", "E");
                    //ei.Remove("POD_CD");
                    //ei.Remove("POD_CNTY");
                    //ei.Remove("POD_NAME");
                    //ei.Remove("DLV_CD");
                    //ei.Remove("DLV_CNTY");
                    //ei.Remove("DLV_NAME");
                    break;
                case "I": //进口提单
                default:
                    ei.Put("TRANS_FLAG", "N");
                    break;
            }

            string JOB_NO = System.Guid.NewGuid().ToString("N");
            if (dr != null)
            {
                JOB_NO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                ei.OperationType = EditInstruct.UPDATE_OPERATION;
                ei.Put("MODIFY_BY", "EDI");
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                ei.Put("CREATE_BY", "EDI");
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            JobNo = JOB_NO;
            ei.PutKey("U_ID", JOB_NO);
            ml.Add(ei);

            AddPartys(receiverXML, ml, JOB_NO, OceanRoot);

            OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            SendItrace(xn);
        }

        /// <summary>
        /// 设置货柜数量
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="xn"></param>
        private static void SetContainerQty(EditInstruct ei, XmlNode xn)
        {
            string containerQty = GetXmlNodeValue(xn, "ContainerQty");
            if (!string.IsNullOrEmpty(containerQty))
            {
                ei.Remove("CNT_NUMBER");
                string[] cons = containerQty.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string con in cons)
                {
                    string[] cs = con.Split(new string[] { "X" }, StringSplitOptions.RemoveEmptyEntries);
                    if (cs.Length < 2)
                        continue;
                    cs[0] = cs[0].Trim().ToUpper();
                    cs[1] = cs[1].Trim();
                    if (cs[0].Contains("40'") && cs[0].Contains("HQ"))//40'HQ
                    {
                        ei.Put("CNT40HQ", cs[1]);
                    }
                    else if (cs[0].Contains("40'"))//40'
                    {
                        ei.Put("CNT40", cs[1]);
                    }
                    else if (cs[0].Contains("20'"))//20'
                    {
                        ei.Put("CNT20", cs[1]);
                    }
                    else
                    {
                        ei.Put("CNT_TYPE", cs[0]);
                        ei.Put("CNT_NUMBER", cs[1]);
                    }
                }
            }
        }

        /// <summary>
        /// po 保存
        /// </summary>
        /// <param name="xml"></param>
        public void SavePo(string xml)
        {
            XmlDocument receiverXML = new XmlDocument();
            receiverXML.LoadXml(xml);
            MixedList ml = new MixedList();
            EditInstruct ei = null;
            //保存新的数据
            XmlNodeList xnlist = receiverXML.SelectNodes(PoRoot);
            List<string> houseList = new List<string>();
            DataTable dt = null;
            foreach (XmlNode xn in xnlist)
            {
                if (xn.NodeType != XmlNodeType.Element)
                {
                    continue; //只针对元素操作
                }
                string houseNo = GetXmlNodeValue(xn, "HouseNo");
                HouseNo = houseNo;
                if (dt == null)
                {
                    dt = GetTkblRow(houseNo);
                    if (dt == null || dt.Rows.Count <= 0)
                        throw new Exception(string.Format("HOUSE NO：{0} Not  Find", houseNo));
                }

                foreach (DataRow dr in dt.Rows)
                {
                    MasterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);// HousNo = houseNo; MasterNo=GetXmlNodeValue(xn, "MasterNo");

                    string JOB_NO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    JobNo = JOB_NO;
                    if (!houseList.Contains(JOB_NO))
                    {
                        houseList.Add(JOB_NO);
                        ml.Add(string.Format("DELETE FROM TKBLPO WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(JOB_NO)));
                    }
                    ei = ParseEditInstruct(xn, PoMapping);
                    ei.PutKey("JOB_NO", JOB_NO);
                    ml.Add(ei);
                }
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// Container 保存
        /// </summary>
        /// <param name="xml"></param>
        public void SaveContainer(string xml)
        {
            XmlDocument receiverXML = new XmlDocument();
            receiverXML.LoadXml(xml);
            MixedList ml = new MixedList();
            MixedList cntrMl = new MixedList();
            EditInstruct ei = null;
            //保存新的数据
            XmlNodeList xnlist = receiverXML.SelectNodes(ContainerRoot);
            List<string> houseList = new List<string>();
            DataTable dt =null;
            foreach (XmlNode xn in xnlist)
            {
                if (xn.NodeType != XmlNodeType.Element)
                {
                    continue; //只针对元素操作
                }
                string houseNo = GetXmlNodeValue(xn, "HouseNo");
                HouseNo = houseNo;
                if(dt==null){
                    dt = GetTkblRow(houseNo);
                    if(dt==null||dt.Rows.Count<=0)
                        throw new Exception(string.Format("HOUSE NO：{0} Not  Find", houseNo));
                }

                foreach (DataRow dr in dt.Rows)
                {
                    MasterNo = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                    string JOB_NO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                    JobNo = JOB_NO;
                    if (!houseList.Contains(JOB_NO))
                    {
                        houseList.Add(JOB_NO);
                        ml.Add(string.Format("DELETE FROM TKBLCNTR WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(JOB_NO)));
                        cntrMl.Add(string.Format("UPDATE TKBL SET CNTR_NO=(SELECT DISTINCT CNTR_NO+';' FROM TKBLCNTR WHERE JOB_NO={0} AND CNTR_NO IS NOT NULL FOR XML PATH('')) WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(JOB_NO)));
                    }
                    ei = ParseEditInstruct(xn, ContainerMapping);
                    ei.PutKey("JOB_NO", JOB_NO);
                    ml.Add(ei);
                }
            }
            if (ml.Count > 0)
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (cntrMl.Count > 0)
                OperationUtils.ExecuteUpdate(cntrMl, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        /// <summary>
        /// 添加航班信息
        /// </summary>
        /// <param name="receiverXML"></param>
        /// <param name="ml"></param>
        /// <param name="rootId"></param>
        /// <param name="job_no"></param>
        private void AddFlightData(XmlDocument receiverXML, MixedList ml, string rootId, string job_no, EditInstruct blE)
        {
            ml.Add(string.Format("DELETE FROM TKBLFLNO WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(job_no)));
            //判断FL_NO的值是否有传值进来
            EditInstruct flightData = null;
            string flightNo = string.Empty;
            for (int i = 1; i <= 3; i++)//最多3笔航班信息
            {
                flightNo = GetXmlNodeValue(receiverXML, rootId, "FlightNo" + i);
                if (string.IsNullOrEmpty(flightNo))
                    continue;
                if (blE != null && i <= 3)
                {
                    blE.Put("VESSEL" + i, flightNo);
                    blE.PutDate("ETD" + i, GetXmlNodeValue(receiverXML, rootId, "ETD_DATE" + i) +GetXmlNodeValue(receiverXML, rootId, "ETD_TIME" + i));
                    blE.PutDate("ETA" + i, GetXmlNodeValue(receiverXML, rootId, "ETA_DATE" + i) + GetXmlNodeValue(receiverXML, rootId, "ETA_TIME" + i));
                }
                flightData = new EditInstruct("TKBLFLNO", EditInstruct.INSERT_OPERATION);
                flightData.Put("JOB_NO", job_no);
                flightData.Put("FL_NO", flightNo);
                flightData.PutDate("ETD", GetXmlNodeValue(receiverXML, rootId, "ETD_DATE" + i) + GetXmlNodeValue(receiverXML, rootId, "ETD_TIME" + i));
                flightData.PutDate("ATD", GetXmlNodeValue(receiverXML, rootId, "ETD_DATE" + i) + GetXmlNodeValue(receiverXML, rootId, "ETD_TIME" + i));
                flightData.Put("FTO", GetXmlNodeValue(receiverXML, rootId, "ToCountry" + i) + GetXmlNodeValue(receiverXML, rootId, "ToCity" + i));
                flightData.PutDate("ETA", GetXmlNodeValue(receiverXML, rootId, "ETA_DATE" + i) + GetXmlNodeValue(receiverXML, rootId, "ETA_TIME" + i));
                flightData.PutDate("ATA", GetXmlNodeValue(receiverXML, rootId, "ETA_DATE" + i) + GetXmlNodeValue(receiverXML, rootId, "ETA_TIME" + i));
                ParseDbEditInstruct(flightData);
                ml.Add(flightData);
            }
        }

        /// <summary>
        /// 添加party信息
        /// </summary>
        /// <param name="receiverXML"></param>
        /// <param name="ml"></param>
        /// <param name="job_no"></param>
        /// <param name="rootId"></param>
        private void AddPartys(XmlDocument receiverXML, MixedList ml, string job_no, string rootId)
        {
            CreatePartyMapping();
            foreach (var pv in partyMapping)
            {
                AddPartyData(receiverXML, ml, job_no, pv.Key, rootId);
            }
        }

        /// <summary>
        /// 添加单个party信息
        /// </summary>
        /// <param name="receiverXML"></param>
        /// <param name="ml"></param>
        /// <param name="job_no"></param>
        /// <param name="dpType"></param>
        /// <param name="rootId"></param>
        private void AddPartyData(XmlDocument receiverXML,MixedList ml,string job_no, string dpType,string rootId)
        {
            CreatePartyMapping();

            string code = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["code"]);
            string name = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["name"]);
            string address = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["address"]);
            string party_attn = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["party_attn"]);
            string party_tel = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["party_tel"]);
            string party_mail = GetXmlNodeValue(receiverXML, rootId, partyMapping[dpType]["party_mail"]);
            //如果code的值为空，则不写入数据
            if (string.IsNullOrEmpty(code))
                return;

            EditInstruct delPartyData = new EditInstruct("TKBLPT", EditInstruct.DELETE_OPERATION);
            delPartyData.Put("U_ID", job_no, true);
            delPartyData.Put("PARTY_TYPE", dpType, true);
            ml.Add(delPartyData);
            //执行插入操作
            EditInstruct partyData = new EditInstruct("TKBLPT", EditInstruct.INSERT_OPERATION);
            string PartyTypeName = GetPartyDesp(dpType);
            partyData.Put("U_ID", job_no, true);
            partyData.Put("PARTY_TYPE", dpType, true);
            partyData.Put("PARTY_NO", code, true);
            partyData.Put("TYPE_DESCP", PartyTypeName);
            partyData.Put("PARTY_NAME", name);
            partyData.Put("PARTY_ADDR", address);
            partyData.Put("PARTY_ATTN", party_attn);
            partyData.Put("PARTY_TEL", party_tel);
            partyData.Put("PARTY_MAIL", party_mail);

            ParseDbEditInstruct(partyData);
            ml.Add(partyData);
        }

        /// <summary>
        /// 设置港口
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="ei"></param>
        private static void SetPorts(DataRow dr, EditInstruct ei)
        {
            SetPort("POL", dr, ei);
            SetPort("POD", dr, ei);
            SetPort("POR", dr, ei);
            SetPort("VIA", dr, ei);
            SetPort("DEST", dr, ei);
            SetPort("DLV", dr, ei);
        }

        private static void SetPort(string name, DataRow dr, EditInstruct ei)
        {
            List<string> fileds = new List<string>(ei.getNameSet());
            if (!fileds.Contains((name + "_CNTY")))
                return;
            string cd = string.Empty, cnty = string.Empty;
            if (fileds.Contains((name + "_CD")))
                cd = ei.Get(name + "_CD");
            if (fileds.Contains((name + "_CNTY")))
                cnty = ei.Get(name + "_CNTY");

            ei.Put(name + "_CD", cnty + cd);
            ei.Remove(name + "_CNTY");
        }

        #endregion

        #region 工具方法
        static Dictionary<string, Dictionary<string, string>> partyMapping = null;
        /// <summary>
        /// 创建party映射
        /// </summary>
        private static void CreatePartyMapping()
        {
            if (partyMapping != null)
                return;
            partyMapping = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<string, string> partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "SHIP_CODE";
            partyFiled["name"] = "SHIP_Name";
            partyFiled["address"] = "SHIP_ADDR";
            partyFiled["party_attn"] = "Shipper_PartyAttn";
            partyFiled["party_tel"] = "Shipper_PartyTel";
            partyFiled["party_mail"] = "Shipper_PartyMail";
            partyMapping["SP"] = partyFiled;//Shipper

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "CNEE_CODE";
            partyFiled["name"] = "CNEE_Name";
            partyFiled["address"] = "CNEE_ADDR";
            partyFiled["party_attn"] = "Consignee_PartyAttn";
            partyFiled["party_tel"] = "Consignee_PartyTel";
            partyFiled["party_mail"] = "Consignee_PartyMail";
            partyMapping["CN"] = partyFiled;//Consignee

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "NOTIFY";
            partyFiled["name"] = "NOTIFY_Name";
            partyFiled["address"] = "NOTIFY_ADDR";
            partyFiled["party_attn"] = "NOTIFY_PartyAttn";
            partyFiled["party_tel"] = "NOTIFY_PartyTel";
            partyFiled["party_mail"] = "NOTIFY_PartyMail";
            partyMapping["NF"] = partyFiled;//notify

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "ALS_NOTIFY";
            partyFiled["name"] = "ALS_NOTIFY_NAME";
            partyFiled["address"] = "ALS_NOTIFY_ADDR";
            partyFiled["party_attn"] = "ALS_NOTIFY_PartyAttn";
            partyFiled["party_tel"] = "ALS_NOTIFY_PartyTel";
            partyFiled["party_mail"] = "ALS_NOTIFY_PartyMail";
            partyMapping["ANF"] = partyFiled;//also notify

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "OS_CODE";
            partyFiled["name"] = "OS_Name";
            partyFiled["address"] = "OS_ADDRRESS";
            partyFiled["party_attn"] = "OS_PartyAttn";
            partyFiled["party_tel"] = "OS_PartyTel";
            partyFiled["party_mail"] = "OS_PartyMail";
            partyMapping["OAG"] = partyFiled;//OAG(即Origin Forwarder)

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "Agent_CODE";
            partyFiled["name"] = "Agent_Name";
            partyFiled["address"] = "Agent_ADDR";
            partyFiled["party_attn"] = "Agent_PartyAttn";
            partyFiled["party_tel"] = "Agent_PartyTel";
            partyFiled["party_mail"] = "Agent_PartyMail";
            partyMapping["DAG"] = partyFiled;//DAG(即Destination Forwarder）

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "Carrier";
            partyFiled["name"] = "Carrier_Name";
            partyFiled["address"] = "Carrier_ADDR";
            partyFiled["party_attn"] = "Carrier_PartyAttn";
            partyFiled["party_tel"] = "Carrier_PartyTel";
            partyFiled["party_mail"] = "Carrier_PartyMail";
            partyMapping["CA"] = partyFiled;//Carrier

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "EXPTruckCode";
            partyFiled["name"] = "EXPTruck_NAME";
            partyFiled["address"] = "EXPTruck_ADDR";
            partyFiled["party_attn"] = "EXPTruck_PartyAttn";
            partyFiled["party_tel"] = "EXPTruck_PartyTel";
            partyFiled["party_mail"] = "EXPTruck_PartyMail";
            partyMapping["PT"] = partyFiled;//出口提单卡车

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "IMPTruckCode";
            partyFiled["name"] = "IMPTruck_NAME";
            partyFiled["address"] = "IMPTruck_ADDR";
            partyFiled["party_attn"] = "IMPTruck_PartyAttn";
            partyFiled["party_tel"] = "IMPTruck_PartyTel";
            partyFiled["party_mail"] = "IMPTruck_PartyMail";
            partyMapping["DT"] = partyFiled;//进口提单卡车

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "EXPBrokerCode";
            partyFiled["name"] = "EXPBrokerName";
            partyFiled["address"] = "EXPBrokerAddress";
            partyFiled["party_attn"] = "EXPBroker_PartyAttn";
            partyFiled["party_tel"] = "EXPBroker_PartyTel";
            partyFiled["party_mail"] = "EXPBroker_PartyMail";
            partyMapping["OBK"] = partyFiled;//出口提单报关

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "IMPBrokerCode";
            partyFiled["name"] = "IMPBrokerName";
            partyFiled["address"] = "IMPBrokerAddress";
            partyFiled["party_attn"] = "IMPBroker_PartyAttn";
            partyFiled["party_tel"] = "IMPBroker_PartyTel";
            partyFiled["party_mail"] = "IMPBroker_PartyMail";
            partyMapping["DBK"] = partyFiled; //进口提单报关行

            partyFiled = new Dictionary<string, string>();
            partyFiled["code"] = "WHSCode";
            partyFiled["name"] = "WHSName";
            partyFiled["address"] = "WHSAddress";
            partyFiled["party_attn"] = "WHS_PartyAttn";
            partyFiled["party_tel"] = "WHS_PartyTel";
            partyFiled["party_mail"] = "WHS_PartyMail";
            partyMapping["DWS"] = partyFiled; //进口提单仓库
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static List<string> GetKeys(Dictionary<string, object> mapping)
        {
            return mapping["keys"] as List<string>;
        }

        /// <summary>
        /// 获取栏位
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetFields(Dictionary<string, object> mapping)
        {
            return mapping["fields"] as Dictionary<string, object>;
        }

        public static void ParseDbEditInstruct(EditInstruct ei,StringBuilder msg=null)
        {
            string[] names = ei.getNameSet();
            foreach (string filed in names)
            {
                if (!ei.IsKey(filed) && !ei.IsDateTimeOffset(filed) && !ei.IsLong(filed) && !ei.IsNClob(filed)
                    && !ei.IsDate(filed) && !ei.IsBlob(filed) && !ei.IsClob(filed) && !ei.IsExpress(filed))
                    GetDbValue(ei.ID, filed, ei.Get(filed), msg, ei);
            }
        }

        /// <summary>
        /// 获取提单信息 默认的数据来自缓存
        /// </summary>
        /// <param name="HOUSE_NO">提单号</param>
        /// <param name="fromCache">数据是否来自缓存</param>
        /// <returns></returns>
        public DataRow GetTkblRow(string HOUSE_NO, string expOrImp, string agent, string originForwarder, bool fromCache = true)
        {
            string sql0 = string.Format("SELECT U_ID,PARTY_TYPE,PARTY_NO FROM TKBLPT WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0}) AND PARTY_TYPE IN ('OAG','DAG')", Prolink.Data.SQLUtils.QuotedStr(HOUSE_NO));
            DataTable partyDt = GetData(sql0, fromCache);
            DataRow[] drs = null;
            string u_id = string.Empty;
            switch (expOrImp)
            {
                case "O": //出口提单
                    foreach (DataRow dr in partyDt.Rows)
                    {
                        string temp = Prolink.Math.GetValueAsString(dr["U_ID"]);
                        drs = partyDt.Select(string.Format("PARTY_TYPE='DAG' AND PARTY_NO={0} AND U_ID={1}", SQLUtils.QuotedStr(agent), SQLUtils.QuotedStr(temp)));
                        if (drs.Length <= 0)
                            continue;
                        drs = partyDt.Select(string.Format("PARTY_TYPE='OAG' AND PARTY_NO={0} AND U_ID={1}", SQLUtils.QuotedStr(originForwarder), SQLUtils.QuotedStr(temp)));
                        if (drs.Length <= 0)
                            continue;
                        u_id = temp;
                        break;
                    }
                    break;
                case "I": //进口提单
                default:
                    drs = partyDt.Select(string.Format("PARTY_TYPE='DAG' AND PARTY_NO={0}", SQLUtils.QuotedStr(agent)));
                    if (drs.Length > 0)
                        u_id = Prolink.Math.GetValueAsString(drs[0]["U_ID"]);
                    else
                        return null;
                    break;
            }
            //string sql = string.Format("SELECT SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO FROM TKBL WHERE U_ID IN (SELECT U_ID FROM TKBLPT WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE HOUSE_NO={0}) AND PARTY_TYPE IN ('OAG','DAG'))", Prolink.Data.SQLUtils.QuotedStr(HOUSE_NO));
            string sql = string.Format("SELECT SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO FROM TKBL WHERE HOUSE_NO={0}", Prolink.Data.SQLUtils.QuotedStr(HOUSE_NO));
            if (!string.IsNullOrEmpty(u_id))
                sql = string.Format("SELECT SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
            DataTable dt = GetData(sql, fromCache);
            if (dt.Rows.Count > 0)
                return dt.Rows[0];
            return null;
        }

        public DataTable GetTkblRow(string HOUSE_NO,bool fromCache = true)
        {
            string sql = string.Format("SELECT * FROM TKBL WHERE HOUSE_NO={0}", Prolink.Data.SQLUtils.QuotedStr(HOUSE_NO));
            //string sql = string.Format("SELECT SHIPMENT_ID,HOUSE_NO,TRAN_TYPE,U_ID,TRANS_FLAG,MASTER_NO FROM TKBL WHERE HOUSE_NO={0}", Prolink.Data.SQLUtils.QuotedStr(HOUSE_NO));
            DataTable dt = GetData(sql, fromCache);
            return dt;
            //if (dt.Rows.Count > 0)
            //{
            //    return dt.Rows[0];
            //}
            //return null;
        }

        /// <summary>
        /// 获取party描述
        /// </summary>
        /// <param name="dpType"></param>
        /// <returns></returns>
        private string GetPartyDesp(string dpType)
        {
            DataTable partyTypeDt = GetData("SELECT DP_DESCP,DP_TYPE FROM EDDPTYPE WHERE DP='P'");
            string PartyTypeName = string.Empty;
            DataRow[] drs = partyTypeDt.Select(string.Format("DP_TYPE={0}", Prolink.Data.SQLUtils.QuotedStr(dpType)));
            if (drs.Length > 0)
                PartyTypeName = Prolink.Math.GetValueAsString(drs[0]["DP_DESCP"]);
            return PartyTypeName;
        }

        public System.Xml.XmlNode GetXmlNode(XmlDocument xmldoc, string RootName, string NodeName)
        {
            try
            {
                System.Xml.XmlNode XnNode = xmldoc.SelectSingleNode(string.Format("//{0}/node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", new object[] { RootName, NodeName.ToLower() }));
                if (XnNode != null)
                {
                    return XnNode;
                }
                else
                    return null;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return null;
            }
        }

        public string GetXmlNodeValue(XmlDocument receiverXML,string RootName, string NodeName)
        {
            try
            {
                string value = string.Empty;
                System.Xml.XmlNode XnNode = receiverXML.SelectSingleNode(string.Format("//{0}/node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{1}']", new object[] { RootName, NodeName.ToLower() }));
                if (XnNode != null)
                {
                    value = XnNode.InnerText;
                    if (!string.IsNullOrEmpty(value))
                        value = value.Trim();
                    return value;
                }
                else
                    return value;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return string.Empty;
            }
        }

        public static string GetXmlNodeValue(XmlNode xn, string NodeName)
        {
            try
            {
                string value = string.Empty;
                System.Xml.XmlNode XnNode = xn.SelectSingleNode(string.Format("node()[translate(local-name(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '{0}']", NodeName.ToLower()));
                if (XnNode != null)
                {
                    value = XnNode.InnerText;
                    if (!string.IsNullOrEmpty(value))
                        value = value.Trim();
                    return value;
                }
                else
                    return value;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public static string GetTableName(Dictionary<string, object> mapping)
        {
            return mapping["table"] as string;
        }

        static Dictionary<string, object> _tables = new Dictionary<string, object>();
        /// <summary>
        /// <column name="PONO" fieldname="PO_NO" dataType="string" defalutValue="" length="40">
        /// </summary>
        /// <param name="ConfigXml"></param>
        public static Dictionary<string, object> GetMapping(string mappingName, bool isXls=false)
        {
            if (_tables.ContainsKey(mappingName))
                return _tables[mappingName] as Dictionary<string, object>;
            XmlDocument ConfigXml = new XmlDocument();
            string path = Prolink.Web.WebContext.GetInstance().GetProperty(mappingName);
            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    string basePath = Prolink.Web.WebContext.GetInstance().GetProperty("InboundSCMInfoMapping");
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        basePath = System.IO.Path.GetDirectoryName(basePath);
                        path = System.IO.Path.Combine(basePath, mappingName + ".xml");
                    }
                }
                catch { }
            }
            ConfigXml.Load(path);

            Dictionary<string, object> tableMap = new Dictionary<string, object>();
            Dictionary<string, object> field = null;
            Dictionary<string, object> fields = new Dictionary<string,object>();
            XmlElement configEl=null;
            XmlElement root = (XmlElement)ConfigXml.SelectSingleNode("table");
            string table = root.GetAttribute("name").ToUpper();
            GetDataSchema(table);
            string name=string.Empty;
            List<string> keys = new List<string>();
            foreach (XmlNode innerxn in root.ChildNodes)
            {
                if (innerxn.NodeType != XmlNodeType.Element)
                {
                    continue; //只针对元素操作
                }
                field = new Dictionary<string, object>();
                configEl = (XmlElement)innerxn;
                name = configEl.GetAttribute("name").Trim();
                if (!isXls)
                    name = name.ToUpper();
                field["name"] =name;//节点名
                field["fieldname"] = configEl.GetAttribute("fieldname").ToUpper();//栏位名
                field["isnotnull"] = string.Empty;
                field["key"] = false;
                field["defalutValue"] = string.Empty;
                field["dataType"] ="string";
                field["cellCode"] = string.Empty;
                if (!string.IsNullOrEmpty(configEl.GetAttribute("key"))) //主键处理
                {
                    field["key"] = true;
                    keys.Add(name);
                }
                if (!string.IsNullOrEmpty(configEl.GetAttribute("cellCode")))
                {
                    field["cellCode"] = configEl.GetAttribute("cellCode");
                }
                if (!string.IsNullOrEmpty(configEl.GetAttribute("defalutValue")))
                {
                    field["defalutValue"] = configEl.GetAttribute("defalutValue");
                }
                if (!string.IsNullOrEmpty(configEl.GetAttribute("dataType")))
                {
                    field["dataType"] = configEl.GetAttribute("dataType");
                }

                if (!string.IsNullOrEmpty(configEl.GetAttribute("isnotnull")))
                {
                    field["isnotnull"] = configEl.GetAttribute("isnotnull");
                }
                fields[name] = field;
            }
            tableMap["fields"] = fields;
            tableMap["table"] = table;
            tableMap["keys"] = keys;
           
            _tables[mappingName] = tableMap;
            return tableMap;
        }

        private EditInstruct ParseEditInstruct(XmlNode SourceparentNo, string mappingName)
        {
            Dictionary<string, object> mapping = GetMapping(mappingName);
            StringBuilder KeyWhereSb = new StringBuilder();
            //获取表名
            string tableName = GetTableName(mapping);
            EditInstruct ei = new EditInstruct(tableName, EditInstruct.INSERT_OPERATION);
            Dictionary<string, object> fields = GetFields(mapping);
            Dictionary<string, object> field=null;
            List<string> keys = GetKeys(mapping);

            XmlElement sourceElement=null;
            string fieldValue = string.Empty, fieldname = string.Empty, defalutValue = string.Empty,dataType= string.Empty; //字段值
            bool isKey = false;
            foreach (System.Xml.XmlNode xn in SourceparentNo.ChildNodes)
            {
                if (xn.NodeType != System.Xml.XmlNodeType.Element)
                {
                    continue; //只针对元素操作
                }
                //取出数据源中的xml元素
                sourceElement = (System.Xml.XmlElement)xn;
                fieldValue = sourceElement.InnerText;
                string name = sourceElement.Name.ToUpper().Trim();
                if (!fields.ContainsKey(name))
                {
                    warningMessager.Append(string.Format("{0}未处理节点;", sourceElement.Name));
                    continue;
                }

                field=fields[name] as Dictionary<string, object>;
                isKey = keys.Contains(name);
                fieldname= Prolink.Math.GetValueAsString(field["fieldname"]);
                defalutValue= Prolink.Math.GetValueAsString(field["defalutValue"]);
                dataType= Prolink.Math.GetValueAsString(field["dataType"]);

                if (isKey && string.IsNullOrEmpty(fieldValue))
                {
                    throw new Exception(string.Format("Field:{0} is key,key must is not null", sourceElement.Name));
                }
                PutToEditInstruct(ei, fieldname, fieldValue, dataType, isKey, warningMessager);
            }
            return ei;
        }

        public static void PutToEditInstruct(EditInstruct Edi, string FieldName, string FieldValue, string DataType, bool isKey, StringBuilder warningMessager = null)
        {
            if (!string.IsNullOrEmpty(FieldValue))
                FieldValue = FieldValue.Trim();
            switch (DataType.ToLower())
            {
                case "date": //日期类型
                    if (!string.IsNullOrEmpty(FieldValue))
                        Edi.PutDate(FieldName, Prolink.Math.GetValueAsDateTime(FieldValue));
                    else
                        Edi.PutDate(FieldName, FieldValue);
                    break;
                case "blob": //二进制
                    Edi.PutBlob(FieldName, System.Text.Encoding.UTF8.GetBytes(FieldValue));
                    break;
                case "clob": //备注
                    Edi.PutClob(FieldName, FieldValue);
                    break;
                case "long":
                    Edi.PutLong(FieldName, FieldValue);
                    break;
                case "datetimeoffset":
                    Edi.PutDateTimeOffset(FieldName, FieldValue);
                    break;
                case "number":
                    decimal numValue=0;
                    if (decimal.TryParse(FieldValue, out numValue))
                        Edi.Put(FieldName, FieldValue);
                    else
                    {
                        Edi.Put(FieldName, 0);
                        if (!string.IsNullOrEmpty(FieldValue))
                            warningMessager.Append(string.Format("{0}值为{1},不是标准的数字类型;", FieldName, FieldValue));
                    }
                    break;
                default: //其它类型
                    Edi.Put(FieldName, GetDbValue(Edi.ID, FieldName, FieldValue, warningMessager));
                    break;
            }

            //如果是主键，则添加主键
            if (isKey)
            {
                Edi.AddKey(FieldName);
            }
        }

        static Dictionary<string, object> _map = new Dictionary<string, object>();
        static Dictionary<string, object> _filtermap = new Dictionary<string, object>();
        /// <summary>
        /// 获取符合table长度的数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="ei"></param>
        /// <returns></returns>
        public static string GetDbValue(string table, string field, string value, StringBuilder warningMessager = null, EditInstruct ei = null)
        {
            Dictionary<string, int> json = GetDataSchema(table);
            if (json != null && !string.IsNullOrEmpty(value) && json.ContainsKey(field)
                       && json[field] > 0 && value.Length > json[field])
            {
                if (warningMessager != null)
                    warningMessager.Append(string.Format("{0}值为{1},超过{2}码的长度限制,自动截断", field, value, json[field]));
                value = value.Substring(0, json[field]);
                if (ei != null)
                    ei.Put(field, value);
            }
            return value;
        }

        public static string GetDbValueChecklength(string table, string field, string value, StringBuilder warningMessager = null)
        {
            Dictionary<string, int> json = GetDataSchemaFilterString(table);
            if (json != null && !string.IsNullOrEmpty(value) && json.ContainsKey(field)
                       && json[field] > 0 && value.Length > json[field])
            {
                if (warningMessager != null)
                    warningMessager.Append(string.Format("{0} values is {1},more than {2} Code length limits!", field, value, json[field]));
            }
            return value;
        }

        /// <summary>
        /// 获取table的表结构  的长度限制
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static Dictionary<string, int> GetDataSchema(string table)
        {
            Dictionary<string, int> json = null;
            if (!_map.ContainsKey(table))
            {
                DataTable schemaTable = null;
                using (IDbConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection().GetConnection())
                {
                    string sql = "SELECT * FROM " + table + " WHERE 1=0";
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (IDataReader dataReader = cmd.ExecuteReader())
                        {
                            schemaTable = dataReader.GetSchemaTable();
                        }
                    }
                }
                json = new Dictionary<string, int>();
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    string fieldName = schemaTable.Rows[i]["ColumnName"].ToString();
                    int maxLength = (int)schemaTable.Rows[i]["ColumnSize"];
                    string dataTypeName = Prolink.Math.GetValueAsString(schemaTable.Rows[i]["DataTypeName"]);
                    switch (dataTypeName)
                    {
                        case "uniqueidentifier":
                            maxLength = 999;
                            break;
                    }
                    if (string.IsNullOrEmpty(fieldName)) continue;
                    json[fieldName] = maxLength;
                    //json[fieldName + "@type"] = 0;
                }
                _map[table] = json;
            }
            if (_map.ContainsKey(table))
                json = _map[table] as Dictionary<string, int>;
            return json;
        }

        private static Dictionary<string, int> GetDataSchemaFilterString(string table)
        {
            Dictionary<string, int> json = null;
            if (!_filtermap.ContainsKey(table))
            {
                DataTable schemaTable = null;
                using (IDbConnection conn = Prolink.Web.WebContext.GetInstance().GetConnection().GetConnection())
                {
                    string sql = "SELECT * FROM " + table + " WHERE 1=0";
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (IDataReader dataReader = cmd.ExecuteReader())
                        {
                            schemaTable = dataReader.GetSchemaTable();
                        }
                    }
                }
                json = new Dictionary<string, int>();
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    string fieldName = schemaTable.Rows[i]["ColumnName"].ToString();
                    int maxLength = 999;
                    string dataTypeName = Prolink.Math.GetValueAsString(schemaTable.Rows[i]["DataTypeName"]);
                    switch (dataTypeName)
                    {
                        case "nvarchar":
                        case "varchar":
                        case "char":
                        case "nchar":
                        case "text":
                            maxLength = (int)schemaTable.Rows[i]["ColumnSize"];
                            break;
                    }
                    if (string.IsNullOrEmpty(fieldName)) continue;
                    json[fieldName] = maxLength;
                }
                _filtermap[table] = json;
            }
            if (_filtermap.ContainsKey(table))
                json = _filtermap[table] as Dictionary<string, int>;
            return json;
        }
        #endregion
        public static XmlDocument LoadSQLXml(string name)
        {
            string path = Path.Combine(System.Web.Configuration.WebConfigurationManager.AppSettings["WEB_PATH"], "Config", "SqlXml", name);
            XmlDocument sqldoc = new XmlDocument();
            if (File.Exists(path))
                sqldoc.Load(path);
            return sqldoc;
        }
        public static string GetXmlDocSql(XmlDocument sqldoc, string xmlnodename)
        {
            XmlNode rootNode = sqldoc.SelectSingleNode("root");
            XmlNodeList nodeList = rootNode.ChildNodes;
            foreach (XmlNode node in nodeList)
            {
                if (xmlnodename.Equals(node.Name))
                    return node.InnerText;
            }
            return string.Empty;
        }
    }
}