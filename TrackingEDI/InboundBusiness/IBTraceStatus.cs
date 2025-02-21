using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackingEDI.Business;
using TrackingEDI.Model;
using TrackingEDI.Utils;

namespace TrackingEDI.InboundBusiness
{
    public class IBTraceStatus : Parser
    {
        public Dictionary<string, object> Data
        {
            get;
            set;
        }
        List<Status> _list = new List<Status>();
        public IBTraceStatus(Dictionary<string, object> data)
        {
            Data = data;
        }

        public IBTraceStatus()
        {
        } 

        string _senderType = "PROLINK";
        public bool SaveModel(Status status = null, TraceModes mode = TraceModes.Prolink)
        {
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
                Data["CMP"] = st.Cmp;

                item["Code"] = st.StsCd;
                item["Descp"] = st.StsDescp;
                item["Locatioin"] = st.Location;
                item["Locatioin_Name"] = st.LocationName;
                item["ContainerNo"] = st.ContainerNo;
                item["SDate"] = "";
                item["STime"] = "000000";
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
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(DateTime.Now, st.Cmp);
                st.EventTime = ndt.ToString("yyyyMMddHHmmss");
            }

            _list.Add(st);
            return string.Empty;
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
        public bool Save()
        {
            DateTime create_date = DateTime.Now;
            List<object> list = Data["StatusList"] as List<object>;

            Dictionary<string, object> status = null;
            Dictionary<string, object> fly = null;
            string master_no = GetValueAsString(Data, "MASTER_NO");
            string hbl_no = GetValueAsString(Data, "AWBNo");
            string sender = GetValueAsString(Data, "SENDER");
            string cmp = GetValueAsString(Data, "CMP");

            string shipmentId = GetValueAsString(Data, "ShipmentId");
            string bl_key = GetValueAsString(Data, "JobNo");

            DataTable dt = GetTkblRow(master_no, hbl_no, shipmentId, bl_key,cmp);

            if (dt.Rows.Count <= 0)
                throw new Exception(string.Format("MASTER NO：{0} Not  Find", master_no));

            EditInstruct ei = null;
            MixedList ml = new MixedList();
            master_no = Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);

            DataTable statusDt = GetStatus(master_no);

            List<string> blList = new List<string>();
            DataRow[] oldSdrs = null;
            foreach (DataRow dr in dt.Rows)
            {
                string shipment_id = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
                string tran_type = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
                string jobNO = Prolink.Math.GetValueAsString(dr["U_ID"]);
                string trans_flag = Prolink.Math.GetValueAsString(dr["TRANS_FLAG"]);
                string origin_forwarder = Prolink.Math.GetValueAsString(dr["ORIGIN_FORWARDER"]);
                master_no = Prolink.Math.GetValueAsString(dr["MASTER_NO"]);
                hbl_no = Prolink.Math.GetValueAsString(dr["HOUSE_NO"]);

                Dictionary<string, EditInstruct> _eiMap = new Dictionary<string, EditInstruct>();
                foreach (object item in list)
                {
                    ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
                    status = item as Dictionary<string, object>;
                    string containerNo = GetValueAsString(status, "ContainerNo");
                    containerNo = SetContainerNo(containerNo, ei);
                    DataTable containerDt = GetContainerByJobNo(jobNO);
      
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

                    string seq_no = SetStatusKey(ei, oldSdrs, jobNO, locatioinTimezone[0], issingle, create_date, sender);
                    string u_id = ei.Get("U_ID");

                    statusSql += " AND LOCATION=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ei.Get("LOCATION"))) + " AND EVEN_DATE=" + SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(ei.Get("EVEN_DATE")));
                    StringBuilder sb = new StringBuilder();
                    if (!_eiMap.ContainsKey(statusSql))
                    {
                        _eiMap[statusSql] = ei;
                        ml.Add(ei);
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
            if (ml.Count > 0)
                Database.ExecuteUpdate(ml);
            return true;
        }

        private static string GetValueAsString(Dictionary<string, object> status, string name)
        {
            if (status.ContainsKey(name))
                return Prolink.Math.GetValueAsString(status[name]);
            return string.Empty;
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

    }
}
