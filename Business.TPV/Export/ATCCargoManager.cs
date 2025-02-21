using Business.Service;
using Business.TPV.Base;
using Business.Utils;
using Models.EDI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Business.TPV.Export
{
    public class ATCCargoManager : ManagerBase
    {
        private string json = string.Empty;
        public ResultInfo ExportEDI(Runtime runtime, string mode)
        {
            ResultInfo result = null;
            try
            {
                EDIConfig config = GetEDIConfig(runtime.PartyNo, runtime.Location, "ATC");
                if (config == null)
                    result = NoneConfigResult(string.Format("EDI:{0}", runtime.PartyNo));
                if (result == null)
                {
                    ATCCargoSendInfo info = CreateEdiInfos(runtime);
                    if (info == null)
                        result = ResultInfo.NullDataResult();

                    if (result == null)
                    {
                        string resultmsg = OperateData(info, config);
                        if (string.IsNullOrEmpty(resultmsg))
                        {
                            result = new ResultInfo
                            {
                                IsSucceed = true,//result.flag,
                                ResultCode = "200",//result.code,
                                Description = "Successfully" //result.msg
                            };
                        }
                        else
                        {
                            result = new ResultInfo
                            {
                                IsSucceed = false,//result.flag,
                                ResultCode = ResultCode.UnKnow,//result.code,
                                Description = resultmsg //result.msg
                            };
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                result = new ResultInfo
                {
                    IsSucceed = false,
                    ResultCode = ResultCode.UnKnow,
                    Description = e.Message
                };
            }
            if (result.ResultCode != "ConfigError")
                SetLog(runtime, mode, result.Description, json, result.IsSucceed);
            return result;
        }

        protected string OperateData(ATCCargoSendInfo info, EDIConfig config)
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            json = JsonConvert.SerializeObject(info, jSetting);
            string loginUrl = string.Format("https://eapi.atc-cargo.pl:{0}/api/v1/users/login", config.Authorization);

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            try
            {
                // Step 1: Login and get the token
                string token = LoginAndGetToken(loginUrl, config.User, config.Psw);

                if (!string.IsNullOrEmpty(token))
                {
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog($"Token received: {token}");
                    // Step 2: Test the API with the token
                    SendApi(string.Format(config.Server, config.Authorization), token, json);
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog($"An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }

            return "";
        }

        protected ATCCargoSendInfo CreateEdiInfos(Runtime runtime)
        {
            ATCCargoSendInfo template = new ATCCargoSendInfo();
            DataTable dt = GetSMSMIDataTable(runtime.ShipmentID);
            DataTable containerdt = GetSMICNTRDataTable(runtime.ShipmentID);
            if (dt.Rows.Count <= 0)
                return null;
            FillATCCargoInfo(template, dt);
            string TranType = Prolink.Math.GetValueAsString(dt.Rows[0]["TRAN_TYPE"]);
            if(TranType == "F" || TranType == "R")
                FillATCContainerInfo(template, containerdt);
            return template;
        }

        void FillATCCargoInfo(ATCCargoSendInfo info, DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                info.ShipmentID = Prolink.Math.GetValueAsString(dt.Rows[0]["SHIPMENT_ID"]);
                info.BLNumber = Prolink.Math.GetValueAsString(dt.Rows[0]["HOUSE_NO"]);
                if(string.IsNullOrEmpty(info.BLNumber))
                    info.BLNumber = Prolink.Math.GetValueAsString(dt.Rows[0]["MASTER_NO"]);
                info.POD = Prolink.Math.GetValueAsString(dt.Rows[0]["POD_CD"]);
                info.Carrier = Prolink.Math.GetValueAsString(dt.Rows[0]["CARRIER"]);
                info.GrossWeight = Prolink.Math.GetValueAsString(dt.Rows[0]["GW"]);
                info.Vessel = Prolink.Math.GetValueAsString(dt.Rows[0]["M_VESSEL"] + " " + dt.Rows[0]["M_VOYAGE"]);
                info.Agent = Prolink.Math.GetValueAsString(dt.Rows[0]["PARTY_NAME3"]);
            }
        }

        void FillATCContainerInfo(ATCCargoSendInfo info, DataTable dt)
        {
            List<ATCCargoContainerInfo> List = new List<ATCCargoContainerInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                ATCCargoContainerInfo temp = new ATCCargoContainerInfo();
                temp.ContainerNumber = Prolink.Math.GetValueAsString(dr["CNTR_NO"]);
                temp.ContainerType = Prolink.Math.GetValueAsString(dr["CNTR_TYPE"]);
                temp.SealNumber = Prolink.Math.GetValueAsString(dr["SEAL_NO1"]);
                temp.DeliveryDate = Prolink.Math.GetValueAsDateTime(dr["DELIVERY_DATE"]).ToString("yyyy/MM/dd");
                temp.DeliveryTime = Prolink.Math.GetValueAsString(dr["RESERVE_FROM"]).PadLeft(2,'0') + ":00";
                temp.NameOfGoods = Prolink.Math.GetValueAsString(dr["DIVISION_DESCP"]);
                temp.Warehouse = Prolink.Math.GetValueAsString(dr["WS_CD"]);
                List.Add(temp);
            }
            info.ContainerList = List;
        }

        protected DataTable GetSMSMIDataTable(string ShipmentID)
        {
            string sql = string.Format("SELECT SHIPMENT_ID,TRAN_TYPE,HOUSE_NO,MASTER_NO,POD_CD,CARRIER,GW,M_VESSEL,M_VOYAGE,(SELECT TOP 1 PARTY_NAME3 FROM SMSMIPT WHERE SHIPMENT_ID=SMSMI.SHIPMENT_ID AND PARTY_TYPE='SP') AS PARTY_NAME3 FROM SMSMI WHERE SHIPMENT_ID = {0}", SQLUtils.QuotedStr(ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            return dt;
        }

        protected DataTable GetSMICNTRDataTable(string ShipmentID)
        {
            string sql = string.Format("SELECT A.CNTR_NO,A.CNTR_TYPE,SEAL_NO1,A.WS_CD,DIVISION_DESCP,B.DELIVERY_DATE,ISNULL(B.RESERVE_FROM, '0') AS RESERVE_FROM FROM SMICNTR A LEFT JOIN SMORD B ON A.SHIPMENT_ID = B.SHIPMENT_ID AND A.CNTR_NO = B.CNTR_NO WHERE A.SHIPMENT_ID = {0}", SQLUtils.QuotedStr(ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            return dt;
        }

        public void SetLog(Runtime runtime, string mode, string errorMsg, string json, bool isSuccess)
        {
            MixedList ml = new MixedList();
            ATCCargoSendEDILog log = new ATCCargoSendEDILog(runtime, mode);
            string message = errorMsg;
            var v = log.CreateSucceed(runtime.RefNo, json, message);
            if (!isSuccess)
                v = log.CreateEx(message, runtime.RefNo, json);
            Helper.WriteEdiLog(v, ml);
        }

        public string LoginAndGetToken(string url, string username, string password)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Add login payload
                var loginPayload = new
                {
                    email = username,
                    password = password
                };
                string payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(loginPayload);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(payloadJson);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseContent = streamReader.ReadToEnd();

                            // Parse JSON to extract the token
                            var jsonResponse = JObject.Parse(responseContent);
                            return jsonResponse["token"]?.ToString();
                        }
                    }
                    else
                    {
                        Prolink.DataOperation.OperationUtils.Logger.WriteLog($"Login failed! Status: {response.StatusCode}");
                        throw new Exception($"Login failed! Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("An error occurred during the HTTP request.");
                Prolink.DataOperation.OperationUtils.Logger.WriteLog($"Error Response: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }


        static void SendApi(string url, string token, string json)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Add Authorization header
                request.Headers.Add("Authorization", $"Bearer {token}");

                // Write payload to request body
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                // Get the response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseContent = streamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        throw new Exception($"TPV Booking request failed. Status Code: {response.StatusCode}");
                    }
                }
            }
            catch (WebException webEx)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog("An error occurred during the HTTP request.");
                using (var streamReader = new StreamReader(webEx.Response.GetResponseStream()))
                {
                    string errorResponse = streamReader.ReadToEnd();
                    Prolink.DataOperation.OperationUtils.Logger.WriteLog($"Error Response: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Prolink.DataOperation.OperationUtils.Logger.WriteLog($"An error occurred: {ex.Message}");
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }

        public static EDIConfig GetEDIConfig(string partyNo, string location = "", string ediMode = "")
        {
            string sql = string.Format("SELECT * FROM SMEXM WHERE EXPRESS={0}", SQLUtils.QuotedStr(partyNo));
            DataTable dt = DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow row = dt.Rows[0];
            if (!string.IsNullOrEmpty(location))
            {
                List<ConditionItem> items = new List<ConditionItem>();
                items.Add(new ConditionItem("CMP", location));
                items.Add(new ConditionItem("EDI_MODE", ediMode));
                string condition = DBManager.CreateCondition(items, true);
                DataRow[] rows = dt.Select(condition);
                if (rows.Length > 0)
                    row = rows[0];
            }
            return new EDIConfig
            {
                MsgCode = Prolink.Math.GetValueAsString(row["MSG_CODE"]),
                RecieveCode = Prolink.Math.GetValueAsString(row["RCV_ID"]),
                SenderCode = Prolink.Math.GetValueAsString(row["SEND_ID"]),
                Cmp = Prolink.Math.GetValueAsString(row["CMP"]),
                CmpName = Prolink.Math.GetValueAsString(row["CMP_NM"]),
                PartyNO = Prolink.Math.GetValueAsString(row["EXPRESS"]),
                Psw = Prolink.Math.GetValueAsString(row["PW_ID"]),
                Remark = Prolink.Math.GetValueAsString(row["REMARK"]),
                Server = Prolink.Math.GetValueAsString(row["WEB_URL"]),
                User = Prolink.Math.GetValueAsString(row["EX_NO"]),
                FunctionCode = Prolink.Math.GetValueAsString(row["EDI_MODE"]),
                Authorization = Prolink.Math.GetValueAsString(row["AUTHORI_ZATION"])
            };
        }
    }
}
