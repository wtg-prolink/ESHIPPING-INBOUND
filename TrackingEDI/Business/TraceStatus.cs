using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using Prolink.DataOperation;
using Prolink.Data;
using Prolink;
using TrackingEDI.Utils;
using TrackingEDI.Model;
namespace TrackingEDI.Business
{
    /// <summary>
    /// 处理edi hub过来的货况信息
    /// </summary>
    public class TraceStatus:Parser
    {
        public Dictionary<string, object> Data
        {
            get;
            set;
        }

        public TraceStatus()
        {
        }

        public TraceStatus(string xml)
        {
            //XmlParser.SaveFileLog(xml);
            Data = Xml2Json(xml);
        }

        List<Status> _list = new List<Status>();
        public TraceStatus(Dictionary<string, object> data)
        {
            Data = data;
        }

       
        /// <summary>
        /// 新增货况
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public string AddStatus(Status st)
        {
            if (st == null)
                return "null";
            if (string.IsNullOrEmpty(st.HouseNo) && string.IsNullOrEmpty(st.MasterNo) && string.IsNullOrEmpty(st.ShipmentId) && string.IsNullOrEmpty(st.JobNo))
            {
                return "HouseNo,MasterNo,ShipmentId,JobNo 不可都为空";
            }

            if (string.IsNullOrEmpty(st.StsCd))
            {
                return "货况代码不可为空";
            }

            //if (string.IsNullOrEmpty(st.Location))
            //{
            //    return "Location不可为空";
            //}

            if (string.IsNullOrEmpty(st.EventTime))
            {
                st.EventTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            _list.Add(st);
            return string.Empty;
        }

        /// <summary>
        /// 发送来源
        /// </summary>
        string _senderType = "PROLINK";
        /// <summary>
        /// 只能保存同一笔提单下的数据   或者会出错
        /// </summary>
        /// <returns></returns>
        public bool SaveModel(Status status = null, TraceModes mode= TraceModes.Prolink)
        {

            _senderType = GetTraceCode(mode);
            if (status != null)
                AddStatus(status);
            Data = new Dictionary<string, object>();
            List<object> list = new List<object>();
            Dictionary<string, object> item = null;
            foreach (Status st in _list)
            {
                item = new Dictionary<string, object>();
                Data["MASTER_NO"] = st.MasterNo;
                Data["AWBNo"] = st.HouseNo;
                Data["ShipmentId"] = st.ShipmentId;
                Data["JobNo"] = st.JobNo;
                Data["SENDER"] = st.Sender;

                item["Code"] = st.StsCd;
                item["Descp"] = st.StsDescp;
                item["Locatioin"] = st.Location;
                item["Locatioin_Name"] = st.LocationName;
                item["ContainerNo"] = st.ContainerNo;
                item["SDate"] = "";
                item["STime"] ="";
                item["REMARK"] = st.Remark;
                if (!string.IsNullOrEmpty(st.EventTime) && st.EventTime.Length >= 8)
                {
                    item["SDate"] = st.EventTime.Substring(0, 8);
                    if (st.EventTime.Length >= 14)
                        item["STime"] = st.EventTime.Substring(8, 6);
                }
                list.Add(item);
            }
            Data["StatusList"] = list;
            return Save();
        }

        string GetTraceCode(TraceModes mode)
        {
            switch (mode)
            {
                case TraceModes.DHL: return "DHL";
                case TraceModes.EMS: return "EMS";
                case TraceModes.TNT: return "TNT";
                default: return "PROLINK";
            }
        }

        /// <summary>
        /// 批量保存货况
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            DateTime create_date = DateTime.Now;
            List<object> list = Data["StatusList"] as List<object>;
            List<object> flyList = null;
            if (Data.ContainsKey("FlightInfo"))
                flyList = Data["FlightInfo"] as List<object>;
            if (flyList == null) flyList = new List<object>();

            //List<string> jobno = new List<string>();
            Dictionary<string, object> status = null;
            Dictionary<string, object> fly = null;
            string master_no = GetValueAsString(Data, "MASTER_NO");
            string hbl_no = GetValueAsString(Data, "AWBNo");
            string sender = GetValueAsString(Data, "SENDER");

            string shipmentId = GetValueAsString(Data, "ShipmentId");
            string bl_key = GetValueAsString(Data, "JobNo");

            //if (string.IsNullOrEmpty(master_no))
            //    master_no = hbl_no;
            DataTable dt = GetTkblRow(master_no, hbl_no, shipmentId, bl_key);
            //DataTable dt = GetTkblRow(master_no);
            if (dt.Rows.Count <= 0)
                throw new Exception(string.Format("MASTER NO：{0} Not  Find", master_no));

            //PushFactory.SaveLog("", "", "", master_no, "从EDIHUB接收货况", "");
            EditInstruct ei = null;
            MixedList ml = new MixedList();
            master_no = Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);

            DeleteStatus(master_no,sender);

            DataTable statusDt = GetStatus(master_no);


            DataTable flyDt = null;
            if (flyList.Count > 0)
            {
                flyDt = GetFlights(master_no);
            }

            List<string> blList = new List<string>();
            DataRow[] oldSdrs = null;
            foreach (DataRow dr in dt.Rows)
            {
                //DataRow dr = GetTkblRow(master_no);
                string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                string jobNO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string trans_flag = Prolink.Math.GetValueAsString(dr["TRANS_FLAG"]);
                string origin_forwarder = Prolink.Math.GetValueAsString(dr["ORIGIN_FORWARDER"]);
                master_no = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                hbl_no = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);

                #region 处理航空信息
                Dictionary<string, EditInstruct> map = new Dictionary<string, EditInstruct>();
                EditInstruct flyei = new EditInstruct("TKBL", EditInstruct.UPDATE_OPERATION);
                flyei.PutKey("U_ID", jobNO);
                int fly_index = 0;
                foreach (object item in flyList)
                {
                    fly_index++;
                    fly = item as Dictionary<string, object>;

                    string FlightNo = GetValueAsString(fly, "FlightNo");
                    string OriginCode = GetValueAsString(fly, "OriginCode");
                    string Orign = GetValueAsString(fly, "Orign");
                    string DestinationCode = GetValueAsString(fly, "DestinationCode");
                    string Destination = GetValueAsString(fly, "Destination");
                    string Index = GetValueAsString(fly, "Index");
                    string ETD = GetValueAsString(fly, "ETD");
                    string ETA = GetValueAsString(fly, "ETA");
                    string ATD = GetValueAsString(fly, "ATD");
                    string ATA = GetValueAsString(fly, "ATA");

                    string flightNo1 = Prolink.Math.GetValueAsString(FlightNo).Trim().Replace(" ", "");
                    if (map.ContainsKey(flightNo1))
                    {
                        ei = map[flightNo1];
                        ei.OperationType = EditInstruct.INSERT_OPERATION;
                    }
                    else
                    {
                        ei = new EditInstruct("TKBLFLNO", EditInstruct.INSERT_OPERATION);
                        map[flightNo1] = ei;
                        ml.Add(ei);
                    }
                    ei.Put("JOB_NO", jobNO);
                    ei.Put("FL_NO", FlightNo);

                    oldSdrs = flyDt.Select("JOB_NO=" + SQLUtils.QuotedStr(jobNO));
                    ei.Put("FTO", DestinationCode);

                    if (!string.IsNullOrEmpty(ETD))
                        ei.PutDate("ETD", ETD);

                    if (!string.IsNullOrEmpty(ETA))
                        ei.PutDate("ETA", ETA);

                    if (!string.IsNullOrEmpty(ATD))
                        ei.PutDate("ATD", ATD);

                    if (!string.IsNullOrEmpty(ATA))
                        ei.PutDate("ATA", ATA);

                    SetFlyKey(ei, oldSdrs, FlightNo, DestinationCode);

                    if (fly_index > 3)
                        continue;
                    flyei.Put("VESSEL" + fly_index, FlightNo);
                    flyei.PutDate("ETD" + fly_index, ETD);
                    flyei.PutDate("ETA" + fly_index, ETA);
                }
                if (fly_index > 0)
                    ml.Add(flyei);
                #endregion

                Dictionary<string, EditInstruct> _eiMap = new Dictionary<string, EditInstruct>();
                foreach (object item in list)
                {
                    ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
                    status = item as Dictionary<string, object>;
                    string containerNo = GetValueAsString(status, "ContainerNo");
                    containerNo = SetContainerNo(containerNo, ei);
                    DataTable containerDt = GetContainerByJobNo(jobNO);
                    //if (!hbl_no.Equals(master_no)&&containerDt.Rows.Count > 0 && !string.IsNullOrEmpty(containerNo))// && containerDt.Rows.Count > 0
                    //{
                    //    if (containerDt.Select(string.Format("CNTR_NO={0}", SQLUtils.QuotedStr(containerNo))).Length <= 0)
                    //    {
                    //        continue;
                    //    }
                    //}

                    string code = GetValueAsString(status, "Code").Trim();
                    string event_descp = GetValueAsString(status, "Descp");
                    string issingle = "N";
                    DataTable stsDt = GetStatusInfo(code, _senderType);
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

                    //if ("A".Equals(tran_type))
                    //{
                    //    //问题单：97392  MODFIY BY FISH  2015-10-21 如果有包含貨況代碼是DEP的，要去覆蓋S14，ARR則覆蓋S15。
                    //    if ("DEP".Equals(code))
                    //    {
                    //        code = "S14";
                    //        event_descp = "ATD";
                    //        EditInstruct ei1 = new EditInstruct("TKBLST", EditInstruct.DELETE_OPERATION);
                    //        ei1.PutKey("STATUS_CD", "DEP");
                    //        ei1.PutKey("JOB_NO", jobNO);
                    //        ml.Add(ei1);
                    //    }
                    //    else if ("ARR".Equals(code))
                    //    {
                    //        code = "S15";
                    //        event_descp = "ATA";
                    //        EditInstruct ei1 = new EditInstruct("TKBLST", EditInstruct.DELETE_OPERATION);
                    //        ei1.PutKey("STATUS_CD", "ARR");
                    //        ei1.PutKey("JOB_NO", jobNO);
                    //        ml.Add(ei);
                    //    }
                    //}

                    string SDate = GetValueAsString(status, "SDate");
                    string STime = GetValueAsString(status, "STime");
                    string Locatioin_Name = GetValueAsString(status, "Locatioin_Name");
                    string locatioin = GetValueAsString(status, "Locatioin");
                    string[] locatioinTimezone = SetLocatioinTimezone(SDate, STime, locatioin, Locatioin_Name, ei);

                    string statusSql = string.Format("STS_CD={0} AND U_ID={1}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(jobNO));
                    if (!"Y".Equals(issingle))
                    {
                        if (!string.IsNullOrEmpty(containerNo))
                            statusSql += " AND CNTR_NO=" + SQLUtils.QuotedStr(containerNo);
                        else
                            statusSql += " AND (CNTR_NO='' OR CNTR_NO IS NULL)";
                    }

                    oldSdrs = statusDt.Select(statusSql);


                    ei.Put("SHIPMENT_ID", shipment_id);
                    ei.Put("STS_CD", code);
                    ei.Put("STS_DESCP", event_descp);

                    string seq_no = SetStatusKey(ei, oldSdrs, jobNO, locatioinTimezone[0], issingle, create_date,sender);
                    string u_id = ei.Get("U_ID");

                    statusSql += " AND LOCATION=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ei.Get("LOCATION"))) + " AND EVEN_DATE=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ei.Get("EVEN_DATE"))) + " AND STS_DESCP=" + SQLUtils.QuotedStr(event_descp);
                    StringBuilder sb = new StringBuilder();
                    if (!_eiMap.ContainsKey(statusSql))
                    {
                        _eiMap[statusSql] = ei;
                        ml.Add(ei);
                        EvenFactory.AddEven(u_id + "#" + seq_no, u_id, "ST", ml);
                        EvenFactory.AddOnceEven(string.Format("{0}#{1}#{2}", u_id, seq_no, code), u_id, EvenManager.StatusEven, ml);
                        if (!blList.Contains(u_id))
                            blList.Add(u_id);
                    }
                    else
                    {
                        ei = _eiMap[statusSql];
                        sb.Append(ei.Get("REMARK"));
                    }
    
                    if (sb.Length > 0)
                        sb.Append(System.Environment.NewLine);
                    string remark = GetValueAsString(status, "REMARK");
                    string pieces = GetValueAsString(status, "Pieces");
                    string weight = GetValueAsString(status, "Weight");
                    if (!string.IsNullOrEmpty(remark))
                    {
                        sb.Append(remark);
                        sb.Append(System.Environment.NewLine);
                    }
                    if (!string.IsNullOrEmpty(pieces) || !string.IsNullOrEmpty(weight))
                        sb.Append(string.Format("pieces:{0},weight:{1}", pieces, weight));

                    ei.Put("REMARK", GetRemark(sb.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(master_no)) { EvenFactory.AddOnceEven(master_no, master_no, EvenManager.StatusDateEven); }
            else if (!string.IsNullOrEmpty(hbl_no)) EvenFactory.AddOnceEven(hbl_no, hbl_no, EvenManager.StatusDateEven);
            if (ml.Count > 0)
                Database.ExecuteUpdate(ml);
            return true;
        }

        /// <summary>
        /// 移除重复的项的remark
        /// </summary>
        /// <param name="remarks"></param>
        /// <returns></returns>
        private static string GetRemark(string remarks)
        {
            if (string.IsNullOrEmpty(remarks))
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            string[] rs = remarks.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>();
            string r = string.Empty;
            for (int i = 0; i < rs.Length; i++)
            {
                r = rs[i];
                if (string.IsNullOrEmpty(r))
                    continue;
                r = r.Trim();
                if (list.Contains(r))
                    continue;
                list.Add(r);
                if (sb.Length > 0)
                    sb.Append(System.Environment.NewLine);
                sb.Append(r);
            }
            return sb.ToString();
        }

        #region 获取提单相关货况数据
        /// <summary>
        /// MASTER_NO相关航班信息
        /// </summary>
        /// <param name="MASTER_NO"></param>
        /// <returns></returns>
        public DataTable GetFlights(string MASTER_NO)
        {
            DataTable dt = Database.GetDataTable(string.Format("SELECT JOB_NO,SEQ_NO,FL_NO,FTO,ETD,ETA,ATD,ATA FROM TKBLFLNO WHERE JOB_NO IN (SELECT JOB_NO FROM TKBL WHERE MASTER_NO={0})", Prolink.Data.SQLUtils.QuotedStr(MASTER_NO)), null);
            return dt;
        }

        /// <summary>
        /// MASTER_NO相关货况信息
        /// </summary>
        /// <param name="MASTER_NO"></param>
        /// <returns></returns>
        public DataTable GetStatus(string MASTER_NO)
        {
            DataTable dt = Database.GetDataTable(string.Format("SELECT CNTR_NO,STS_CD,STS_DESCP,U_ID,LOCATION,SEQ_NO,LOCATION_DESCP,EVEN_DATE FROM TKBLST WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE MASTER_NO={0})", Prolink.Data.SQLUtils.QuotedStr(MASTER_NO)), null);
            return dt;
        }

        /// <summary>
        /// 删除旧有iport货况
        /// </summary>
        /// <param name="MASTER_NO"></param>
        /// <param name="type"></param>
        public void DeleteStatus(string MASTER_NO,string type="CARRIER")
        {
            switch (type)
            {
                case "CARRIER":
                case "AIR":
                    string sql = string.Format("DELETE FROM TKBLST WHERE U_ID IN (SELECT U_ID FROM TKBL WHERE MASTER_NO={0}) AND CREATE_BY={1}", SQLUtils.QuotedStr(MASTER_NO), SQLUtils.QuotedStr(type));
                    OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                    break;
            }
        }
        #endregion

        #region xml解析
        public static Dictionary<string, object> Xml2Json(string xml)
        {
            XmlDocument myXmlDoc = new XmlDocument();
            myXmlDoc.LoadXml(xml);
            XmlNode rootNode = myXmlDoc.SelectSingleNode("StatusList");
            //XmlNode rootNode = myXmlDoc.FirstChild.NextSibling;
            XmlNodeList nodeList = rootNode.ChildNodes;
            return CreateItem(rootNode, nodeList);
        }

        private static string GetValueAsString(Dictionary<string, object> status, string name)
        {
            if (status.ContainsKey(name))
                return Prolink.Math.GetValueAsString(status[name]);
            return string.Empty;
        }

        /// <summary>
        /// 创建json数据
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        private static Dictionary<string, object> CreateItem(XmlNode rootNode, XmlNodeList nodeList)
        {
            Dictionary<string, object> json = new Dictionary<string, object>();
            List<object> list = new List<object>();
            foreach (XmlNode node in nodeList)
            {
                if (node.HasChildNodes)
                {
                    if (node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText)
                    {
                        json[node.Name] = node.InnerText;
                        continue;
                    }
                    Dictionary<string, object> item = CreateItem(node, node.ChildNodes);
                    if ("Route".Equals(node.Name))
                    {
                        foreach (XmlAttribute attr in node.Attributes)
                        {
                            item[attr.Name] = attr.Value;
                        }
                    }
                    if (item.ContainsKey("FlightInfo"))
                        json["FlightInfo"] = item["FlightInfo"];
                    else
                        list.Add(item);
                    continue;
                }
                json[node.Name] = node.InnerText;
            }
            if (list.Count > 0)
                json[rootNode.Name] = list;
            else if (nodeList.Count <= 0)
                json[rootNode.Name] = rootNode.InnerText;
            return json;
        }
        #endregion

        #region 对外方法
        /// <summary>
        /// 设置航空信息的主键
        /// </summary>
        /// <param name="ei"></param>
        /// <param name="oldSdrs"></param>
        /// <param name="FlightNo"></param>
        public static void SetFlyKey(EditInstruct ei, DataRow[] oldSdrs, string FlightNo, string location)
        {
            string temp1 = Prolink.Math.GetValueAsString(FlightNo).Trim().Replace(" ", "");
            for (int i = 0; i < oldSdrs.Length; i++)
            {
                string temp2 = Prolink.Math.GetValueAsString(oldSdrs[i]["FL_NO"]).Trim().Replace(" ", "");
                string temp3 = temp2;
                //modify by fish  2015-11-3 问题单：97701   milo
                if (temp3.Length == 5)
                {
                    temp3 = temp3.Substring(0, 2) + "0" + temp3.Substring(2, 3);
                }
                else if (temp3.Length == 6)
                {
                    string temp_3_0 = temp3.Substring(0, 2);
                    string temp_3_1 = temp3.Substring(2, 4);
                    if (temp_3_1.StartsWith("0"))
                        temp3 = temp_3_0 + temp_3_1.Substring(1, 3);
                }

                if (temp1.Equals(temp2) || temp1.Equals(temp3))
                {
                    if (LocationIsEquals(Prolink.Math.GetValueAsString(location).Trim(), Prolink.Math.GetValueAsString(oldSdrs[i]["FTO"]).Trim()))
                    {
                        ei.Put("FTO", oldSdrs[i]["FTO"]);
                    }
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.Put("FL_NO", oldSdrs[i]["FL_NO"]);//保持原先的
                    ei.AddKey("JOB_NO");
                    ei.PutKey("SEQ_NO", Prolink.Math.GetValueAsString(oldSdrs[i]["SEQ_NO"]));
                    break;
                }
            }
        }

        /// <summary>
        /// 判断两个Location是否相同
        /// </summary>
        /// <param name="temp1"></param>
        /// <param name="temp2"></param>
        /// <returns></returns>
        public static bool LocationIsEquals(string temp1, string temp2)
        {
            bool update = false;
            if (temp2.Equals(temp1))
            {
                update = true;
            }
            else if (temp1.Length != temp2.Length && temp1.Length >= 3 && temp2.Length >= 3)
            {
                if (temp1.Substring(temp1.Length - 3, 3).Equals(temp2.Substring(temp2.Length - 3, 3)))
                {
                    update = true;
                }
            }
            return update;
        }

        public static string SetContainerNo(string containerNo, EditInstruct ei)
        {
            if (!string.IsNullOrEmpty(containerNo))
            {
                containerNo = containerNo.Trim();
            }

            //if (!string.IsNullOrEmpty(containerNo) && containerNo.Length > 12)
            //{
            //    containerNo = containerNo.Substring(0, 12);
            //}
            ei.PutKey("CNTR_NO", containerNo);
            return containerNo;
        }

        public static DataTable GetStatusByJobNo(string job_no)
        {
            DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT EVEN_DATE,CNTR_NO,STS_CD,STS_DESCP,U_ID,LOCATION,SEQ_NO,LOCATION_DESCP FROM TKBLST WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(job_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //DataTable dt = OperationUtils.GetDataTable(string.Format("SELECT CNTR_NO,STATUS_CD,DESCP,JOB_NO,LOCATION,SEQ_NO FROM TKBLSTS WHERE JOB_NO={0}", Prolink.Data.SQLUtils.QuotedStr(job_no)), null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }

        public static string SetStatusKey(EditInstruct ei, DataRow[] oldSdrs, string jobNO, string location, string issingle, DateTime date, string sender = "")
        {
            DateTime create_date = DateTime.Now;
            bool update = false;
            string seq_no = string.Empty;
            string new_location = string.Empty;
            string temp1 = Prolink.Math.GetValueAsString(location).Trim();
            for (int i = 0; i < oldSdrs.Length; i++)
            {
                string temp2 = Prolink.Math.GetValueAsString(oldSdrs[i]["LOCATION"]).Trim();
                update = LocationIsEquals(temp1, temp2);
                if ("Y".Equals(issingle) && update)
                {
                    update = true;
                }
                else
                {
                    string sts_cd = Prolink.Math.GetValueAsString(oldSdrs[i]["STS_CD"]).Trim();
                    if (update && Prolink.Math.GetValueAsString(oldSdrs[i]["EVEN_DATE"]).Equals(ei.Get("EVEN_DATE")) && sts_cd.Equals(ei.Get("STS_CD")))
                        update = true;
                    else
                        update = false;
                }

                if (update)
                {
                    seq_no = Prolink.Math.GetValueAsString(oldSdrs[i]["SEQ_NO"]);
                    new_location = Prolink.Math.GetValueAsString(oldSdrs[i]["LOCATION"]);
                    ei.OperationType = EditInstruct.UPDATE_OPERATION;
                    ei.Put("LOCATION", new_location);//保持原先的
                    ei.PutKey("U_ID", jobNO);
                    ei.PutKey("SEQ_NO", seq_no);
                    break;
                }
            }
            if (!update)
            {
                seq_no = Guid.NewGuid().ToString("N");
                ei.Put("U_ID", jobNO);
                ei.Put("SEQ_NO", seq_no);
            }

            if (string.IsNullOrEmpty(sender)) sender = "System";
            ei.Put("CREATE_BY", sender);
            ei.PutDate("CREATE_DATE", date);
            return seq_no;
        }

        /// <summary>
        /// 设置 Locatioin 和 时区
        /// 0：Locatioin  1：SDate  2：STime   4：TIMEZONE
        /// </summary>
        /// <param name="status"></param>
        /// <param name="ei"></param>
        public static string[] SetLocatioinTimezone(string SDate, string STime, string locatioin, string Locatioin_Name, EditInstruct ei)
        {
            ei.Put("LOCATION_DESCP", Locatioin_Name);
            if (string.IsNullOrEmpty(locatioin))
                locatioin = Locatioin_Name;
            string country_Id = string.Empty, city_Id = string.Empty, TIMEZONE = string.Empty;
            if (!string.IsNullOrEmpty(locatioin))
            {
                if (locatioin.Length >= 5)
                {
                    locatioin = locatioin.Substring(0, 5);
                    country_Id = locatioin.Substring(0, 2); //国家代码
                    city_Id = locatioin.Substring(2, 3); //城市代码
                }
                else if (locatioin.Length == 3)
                {
                    city_Id = locatioin.Substring(0, 3);
                    //用作确认时区用的
                    DataTable portDt = Database.GetDataTable(string.Format("SELECT TOP 1 CNTRY_CD,GM FROM BSCITY WHERE PORT_CD='{0}'", city_Id), null);
                    country_Id = string.Empty;
                    if (portDt != null && portDt.Rows.Count > 0)
                    {
                        country_Id = Prolink.Math.GetValueAsString(portDt.Rows[0]["CNTRY_CD"]);
                        TIMEZONE = Prolink.Math.GetValueAsString(portDt.Rows[0]["GM"]);
                    }
                }
                else
                {
                    country_Id = ""; //国家代码
                    city_Id = locatioin; //城市代码
                }
            }
            if (string.IsNullOrEmpty(TIMEZONE))
                TIMEZONE = Database.GetValueAsString(string.Format("SELECT GM FROM BSCITY WHERE CNTRY_CD='{0}' AND PORT_CD='{1}'", new object[] { country_Id, city_Id }));
            //string TIMEZONE = Database.GetValueAsString(string.Format("SELECT GM FROM BSCITY WHERE CNTRY_CD='{0}' AND PORT_CD='{1}'", new object[] { country_Id, city_Id }));

            TIMEZONE = GetTimeZone(TIMEZONE);
            if (!string.IsNullOrEmpty(TIMEZONE))
            {
                ei.PutDateTimeOffset("EVEN_TMG", string.Format("{0}{1} {2}", new object[] { SDate, STime, TIMEZONE }));
            }

            ei.Put("LOCATION", locatioin);
            ei.PutDate("EVEN_DATE", string.Format("{0}{1}", new object[] { SDate, STime }));

            string[] locatioinTimezone = new string[4];
            locatioinTimezone[0] = locatioin;
            locatioinTimezone[1] = SDate;
            locatioinTimezone[2] = STime;
            locatioinTimezone[3] = TIMEZONE;
            return locatioinTimezone;
        }

        private static string GetTimeZone(string TIMEZONE)
        {
            if (string.IsNullOrEmpty(TIMEZONE))
                return string.Empty;
            if (TIMEZONE.Contains(":"))
                return TIMEZONE;

            TIMEZONE = TIMEZONE.Trim();
            int int_timezone = 0;
            if (int.TryParse(TIMEZONE, out int_timezone))
            {
                if (int_timezone < 0)
                {
                    if (int_timezone > -10)
                        TIMEZONE = "0" + System.Math.Abs(int_timezone);
                    TIMEZONE = "-" + TIMEZONE + ":00";
                }
                else
                {
                    if (int_timezone < 10)
                        TIMEZONE = "0" + int_timezone;
                    TIMEZONE = "+" + TIMEZONE + ":00";
                }

            }
            return TIMEZONE;
        }
        #endregion
    }

    public enum TraceModes { Prolink, DHL, TNT, EMS }
}
