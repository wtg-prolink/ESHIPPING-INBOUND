using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Business.TPV.Export
{
    public class ICAInfoManager : ManagerBase, IPlanTask
    {
        public ResultInfo TryPostCargoInfo(string sapId, List<ICAInfo> items, string location, string Ibcompany, string shipmentId, string cmp)
        {
            try
            {
                if (items == null || items.Count <= 0)
                {
                    Helper.CreateICALog(shipmentId, cmp, null, "Exception", "itmes no data");
                    return ResultInfo.NullDataResult();
                } 
                ICAInfoEDI edi = new ICAInfoEDI();
                var result = edi.TryPostICAInfo(items, location, Ibcompany);
                EditInstructList el = new EditInstructList();

                if ("MX".Equals(Ibcompany))
                {
                    MXEdiLog(items, el, result);
                }
                else
                {
                    foreach (var item in items)
                    {
                        PostICAInfoEDILog log = new PostICAInfoEDILog(item, "TaskSystem");
                        var v = result.IsSucceed ? log.CreateSucceed(item.DNNO, item) : log.CreateEx(result.Description, item.DNNO, item);
                        v.Data = ToJsonString(item);
                        el.Add(Helper.CreateEDIEi(v, el));
                    }
                }

                if (el != null && el.Count > 0)
                {
                    Execute(el);
                }
                return result;
            }
            catch (Exception ex)
            {
                foreach (var item in items)
                    Helper.CreateICALog(item.DNNO, cmp, "Ex2:" + ex.Message);
                return UnknowResult(ex);
            }
        }
        public void MXEdiLog(List<ICAInfo> items, EditInstructList el, ResultInfo result)
        {
            foreach (var item in items)
            {
                string[] containerNoArry = item.ContainerNo.Split(new char[] { ';', ',' });
                string cont1 = containerNoArry.Length >= 1 ? containerNoArry[0] : null;
                string cont2 = containerNoArry.Length >= 2 ? containerNoArry[1] : null;
                string cont3 = containerNoArry.Length >= 3 ? containerNoArry[2] : null;
                string cont4 = containerNoArry.Length >= 4 ? containerNoArry[3] : null;
                string cont5 = containerNoArry.Length >= 5 ? containerNoArry[4] : null;
                PostICAInfoEDILog log = new PostICAInfoEDILog(item, "TaskSystem");
                var v = result.IsSucceed ? log.CreateSucceed(item.DNNO, item) : log.CreateEx(result.Description, item.DNNO, item);
                v.Data = "DN_E:" + item.DNNO + ",ATD:" + item.ATD + ",BL_NO:" + item.MasterBl + ",CONT1:" + cont1 + ",CONT2:" + cont2 + ",CONT3:" + cont3 + ",CONT4:" + cont4 + ",CONT5:" + cont5 + "\r\n";
                el.Add(Helper.CreateEDIEi(v, el));
            }
        }

        public List<ICAInfo> GetICAInfo(string[] dnnos, string isok = "")
        {

            Func<object, DateTime?> getDateTime = obj =>
            {
                if (obj == null || obj == DBNull.Value) return null;
                return (DateTime)obj;
            };
            string sql = string.Format(@"SELECT GROUP_ID,CMP,STN, (SELECT TOP 1 Cntry_Cd FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS COUNTRY,
                            (SELECT TOP 1 ETA FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ETA,
                            (SELECT TOP 1 ETD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ETD,
                            (SELECT TOP 1 ATD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ATD,
                            (SELECT TOP 1 POD_CD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POD,
                            (SELECT TOP 1 POD_NM FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POD_NM,
                            (SELECT TOP 1 POL_CD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POL,
                            (SELECT TOP 1 POL_NM FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POL_NM,
                            (SELECT TOP 1 MASTER_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS MASTER_NO,
                            (SELECT TOP 1 C_MASTER_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS C_MASTER_NO,
                            (SELECT TOP 1 HOUSE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS HOUSE_NO, 
                            (SELECT TOP 1 C_HOUSE_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS C_HOUSE_NO,
                            (SELECT TOP 1 ISCOMBINE_BL FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ISCOMBINE_BL,
                            (SELECT TOP 1 OUT_DATE FROM SMRV WHERE SMRV.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS OUT_DATE,
                            (SELECT TOP 1 IS_OK FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS IS_OK,DN_NO,SAP_ID,
                            (SELECT TOP 1 IS_OK FROM SMSM WHERE SMSM.SHIPMENT_ID in (SELECT combin_shipment FROM SMSM WHERE SHIPMENT_ID=SMDN.SHIPMENT_ID)) C_IS_OK,
                            FREIGHT_AMT,
                            (SELECT TOP 1 PKG_UNIT_DESC FROM SMINM WHERE SMINM.DN_NO=SMDN.DN_NO) AS PACKING_UNIT_INFO
                             FROM SMDN  WHERE DN_NO in {0}", SQLUtils.Quoted(dnnos));
            DataTable smdndt = DB.GetDataTable(sql, new string[] { });
            if (smdndt == null || smdndt.Rows.Count <= 0) return null;

            return smdndt.Rows.Cast<DataRow>().Select(row => new ICAInfo
            {
                GroupId = Prolink.Math.GetValueAsString(row["GROUP_ID"]),
                CMP = Prolink.Math.GetValueAsString(row["CMP"]),
                STN = Prolink.Math.GetValueAsString(row["STN"]),
                DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                SAP_ID = Prolink.Math.GetValueAsString(row["SAP_ID"]),

                Cntry = getCountry(row),
                ETA_MSL = getDateTime(row["ETA"]),
                ETA = getDateTime(row["ETA"]),
                ETD = getDateTime(row["ETD"]),
                POD = Prolink.Math.GetValueAsString(row["POD"]),
                POD_DESCP = Prolink.Math.GetValueAsString(row["POD_NM"]),
                POL = Prolink.Math.GetValueAsString(row["POL"]),
                POL_DESCP = Prolink.Math.GetValueAsString(row["POL_NM"]),
                MasterBl = GetBillno(row),
                ContainerNo = CreateCNTRInfos(Prolink.Math.GetValueAsString(row["DN_NO"])),
                OutDate = getDateTime(row["OUT_DATE"]),
                FreightAmt = Prolink.Math.GetValueAsString(row["FREIGHT_AMT"]),
                PackingUnitInfo = Prolink.Math.GetValueAsString(row["PACKING_UNIT_INFO"]),
                SendToInbound = GetSendToInbound(row, isok),
                OutSourcingToIB = Prolink.Math.GetValueAsString(row["IS_OK"]),
                ATD = getDateTime(row["ATD"])
            }).Distinct().Where(item => !string.IsNullOrEmpty(item.DNNO)).ToList();
        }

        string GetSendToInbound(DataRow row,string isok)
        {
            if (!string.IsNullOrEmpty(isok))
                return isok;
            string sendinbound = Prolink.Math.GetValueAsString(row["IS_OK"]);
            string cisok = Prolink.Math.GetValueAsString(row["C_IS_OK"]);

            if (string.IsNullOrEmpty(cisok))
                return sendinbound;
            else return cisok;
        }

        public List<ICAInfo> GetICAInfoInBound(string[] dnnos)
        {

            Func<object, DateTime?> getDateTime = obj =>
            {
                if (obj == null || obj == DBNull.Value) return null;
                return (DateTime)obj;
            };
            string sql = string.Format(@" SELECT GROUP_ID,CMP,STN, (SELECT TOP 1 Cntry_Cd FROM SMICNTR WHERE SMICNTR.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS COUNTRY,
                            (SELECT TOP 1 ETA FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS ETA,
                            (SELECT TOP 1 ETD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS ETD,
                            (SELECT TOP 1 ATD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS ATD,
                            (SELECT TOP 1 POD_CD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS POD,
                            (SELECT TOP 1 POD_NAME FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS POD_NM,
                            (SELECT TOP 1 POL_CD FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS POL,
                            (SELECT TOP 1 POL_NAME FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS POL_NM,
                            (SELECT TOP 1 MASTER_NO FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS MASTER_NO,
                            (SELECT TOP 1 CNTR_INFO FROM SMSMI WHERE SMSMI.SHIPMENT_ID=SMIDN.SHIPMENT_ID) AS CNTR_INFO,
                            (SELECT TOP 1 OUT_DATE FROM SMIRV WHERE SMIRV.SHIPMENT_ID=SMIDN.SHIPMENT_ID AND RV_TYPE='I') AS OUT_DATE,'' AS IS_OK,DN_NO,'' AS SAP_ID
                             FROM SMIDN  WHERE DN_NO in {0}", SQLUtils.Quoted(dnnos));
            DataTable smdndt = DB.GetDataTable(sql, new string[] { });
            if (smdndt == null || smdndt.Rows.Count <= 0) return null;

            return smdndt.Rows.Cast<DataRow>().Select(row => new ICAInfo
            {
                GroupId = Prolink.Math.GetValueAsString(row["GROUP_ID"]),
                CMP = Prolink.Math.GetValueAsString(row["CMP"]),
                STN = Prolink.Math.GetValueAsString(row["STN"]),
                DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                SAP_ID = Prolink.Math.GetValueAsString(row["SAP_ID"]),

                Cntry = getCountry(row),
                ETA_MSL = getDateTime(row["ETA"]),
                ETA = getDateTime(row["ETA"]),
                ETD = getDateTime(row["ETD"]),
                POD = Prolink.Math.GetValueAsString(row["POD"]),
                POD_DESCP = Prolink.Math.GetValueAsString(row["POD_NM"]),
                POL = Prolink.Math.GetValueAsString(row["POL"]),
                POL_DESCP = Prolink.Math.GetValueAsString(row["POL_NM"]),
                MasterBl = Prolink.Math.GetValueAsString(row["MASTER_NO"]),
                ContainerNo = Prolink.Math.GetValueAsString(row["CNTR_INFO"]),
                OutDate = getDateTime(row["OUT_DATE"]),
                SendToInbound = Prolink.Math.GetValueAsString(row["IS_OK"]),
                OutSourcingToIB = Prolink.Math.GetValueAsString(row["IS_OK"]),
                ATD= getDateTime(row["ATD"])
            }).Distinct().Where(item => !string.IsNullOrEmpty(item.DNNO)).ToList();
        }

        public IEnumerable<ICAInfo> CreateInfos(DataTable smDT)
        {
            if (smDT == null || smDT.Rows.Count <= 0) yield break;
            Func<object, DateTime?> getDateTime = obj =>
            {
                if (obj == null || obj == DBNull.Value) return null;
                return (DateTime)obj;
            };
         
            foreach (DataRow row in smDT.Rows)
            {
                ICAInfo info = new ICAInfo()
                {
                    DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                    SAP_ID = Prolink.Math.GetValueAsString(row["SAP_ID"]),
                    Cntry= getCountry(row),
                    ETA_MSL = getDateTime(row["ETD"]),
                    ETA = getDateTime(row["ETA"]),
                    ETD = getDateTime(row["ETD"]),
                    POD = Prolink.Math.GetValueAsString(row["POD"])+';'+Prolink.Math.GetValueAsString(row["POD_NM"]),
                    POL = Prolink.Math.GetValueAsString(row["POL"]) + ';' + Prolink.Math.GetValueAsString(row["POL_NM"]),
                    MasterBl = GetBillno(row),
                    ContainerNo= CreateCNTRInfos(Prolink.Math.GetValueAsString(row["DN_NO"])),
                    OutDate = getDateTime(row["ETD"]),
                    SendToInbound = Prolink.Math.GetValueAsString(row["DEST_NAME"]),
                    OutSourcingToIB = Prolink.Math.GetValueAsString(row["DEST_NAME"])
                };
                yield return info;
            }
        }

        string getCountry(DataRow row)
        {
             string country = Prolink.Math.GetValueAsString(row["COUNTRY"]);
            if (!string.IsNullOrEmpty(country))return country;
            string pod = Prolink.Math.GetValueAsString(row["POD"]);
            if (pod.Length >= 2)
                return pod.Substring(0, 2);
            return null;
        }

        string GetBillno(DataRow row) {
            string billno = Prolink.Math.GetValueAsString(row["MASTER_NO"]);
            string houseno = Prolink.Math.GetValueAsString(row["HOUSE_NO"]);
            if (!string.IsNullOrEmpty(houseno))
            {
                billno = houseno;
            }

            if ("S".Equals(Prolink.Math.GetValueAsString(row["ISCOMBINE_BL"])))
            {
                string cMasterNo = Prolink.Math.GetValueAsString(row["C_MASTER_NO"]);
                string cHouseNo = Prolink.Math.GetValueAsString(row["C_HOUSE_NO"]);
                if (!string.IsNullOrEmpty(cMasterNo))
                    billno = cMasterNo;
                if (!string.IsNullOrEmpty(cHouseNo))
                    billno = cHouseNo;
            }

            return billno;
        }
       
        string CreateCNTRInfos(string dnno)
        {
            string sql = string.Format(@"SELECT CNTR_NO FROM SMRV WHERE STATUS NOT IN ('V') AND DN_NO LIKE '%{0}%'", dnno);
            DataTable smrvDT= DB.GetDataTable(sql, new string[] { });
            if (smrvDT == null || smrvDT.Rows.Count <= 0) return null;
            List<string> cntrlist=new List<string>();
            foreach (DataRow row in smrvDT.Rows)
            {
                if (!cntrlist.Contains(Prolink.Math.GetValueAsString(row["CNTR_NO"])))
                {
                    cntrlist.Add(Prolink.Math.GetValueAsString(row["CNTR_NO"]));
                }
            }
            return string.Join(";", cntrlist);
        }



        DataTable QuerySMDT()
        {
            string sql = @"SELECT TOP 20 (SELECT TOP 1 CNTRY_NM FROM BSCITY WHERE CNTRY_CD+PORT_CD=SMDN.POD) AS COUNTRY,
                            (SELECT TOP 1 ETA FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ETA,
                            (SELECT TOP 1 ETD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS ETD,
                            (SELECT TOP 1 POD_CD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POD,
                            (SELECT TOP 1 POD_NM FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POD_NM,
                            (SELECT TOP 1 POL_CD FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POL,
                            (SELECT TOP 1 POL_NM FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS POL_NM,
                            (SELECT TOP 1 MASTER_NO FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS MASTER_NO,
                            (SELECT TOP 1 OUT_DATE FROM SMRV WHERE SMRV.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS OUT_DATE,
                            (SELECT TOP 1 IS_OK FROM SMSM WHERE SMSM.SHIPMENT_ID=SMDN.SHIPMENT_ID) AS IS_OK
                             FROM SMDN  WHERE 1=0";
            return DB.GetDataTable(sql, new string[] { });
        }
        
        List<string> GetIDList(DataTable smDT, string columnName)
        {
            if (smDT == null || smDT.Rows.Count <= 0) return null;
            List<string> idList = new List<string>();
            foreach (DataRow row in smDT.Rows)
            {
                string id = Prolink.Math.GetValueAsString(row[columnName]);// ""]);
                if (!idList.Contains(id))
                    idList.Add(id);
            }
            return idList;
        }

        void UpSMDT(List<string> idList)
        {
            if (idList == null || idList.Count <= 0) return;
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("U_ID IN({0})", string.Join(",", idList.Select(s => SQLUtils.QuotedStr(s))));
            ei.Put("POST_CARGO_FLAG", "Y");
            DB.ExecuteUpdate(ei);
        }
        void UpSMDT(string id, bool succeed)
        {
            if (string.IsNullOrEmpty(id)) return;
            EditInstruct ei = new EditInstruct("SMDN", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(id));
            //if (succeed)//不管失败与否都不要再次发送货况
            ei.Put("POST_CARGO_FLAG", "Y");
            ei.PutExpress("POST_CARGO_DATE", "getdate()");
            DB.ExecuteUpdate(ei);
        }
        string location = "";
        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            try
            {
                _messenger = messenger;
                DataTable smDT = QuerySMDT();
                List<string> idList = GetIDList(smDT, "U_ID");
                List<string> shipmentIDList = GetIDList(smDT, "SHIPMENT_ID");
                //var result = TryPostCargoInfo(CreateInfos(smDT), location);
            }
            catch (Exception ex)
            {
                PostICAInfoEDILog log = new PostICAInfoEDILog(null, "TaskSystem");
                Logger.WriteLog(log.CreateEx(ex));
            }
        }
    }
}
