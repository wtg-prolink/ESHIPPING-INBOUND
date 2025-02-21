using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.TPV.RFC;
using Prolink.Task;
using System.Data;
using Prolink.Data;
using Prolink.DataOperation;
using SAP.Middleware.Connector;
using Business.Service;
using Business.TPV.Base;
using System.IO;

namespace Business.TPV.Export
{
    class CargoManager : ManagerBase, IPlanTask
    {
        string _location = "";
        public ResultInfo TryPostCargoInfo(IEnumerable<CargoInfo> infos, string location)
        {
            CargoEDI edi = new CargoEDI();
            List<CargoInfo> items = infos.ToList();
            ResultInfo result = null;
            foreach (var item in items)
            {
                PostSAPData data = null;
                result = edi.TryPostCargoInfo(new List<CargoInfo> { item }, out data,location);
                PostCargoEDILog log = new PostCargoEDILog(item, "TaskSystem");
                string name = string.Format("{0}_{1}", item.DNNO, GetCurrentTimeString());
                string fileName = BackupData(item, name);
                string fileName2 = string.Empty;
                if (data != null && data.Tables != null && data.Tables.Count > 0)
                {
                    fileName2 = string.Format("{0}/{1}", Path.GetDirectoryName(fileName), string.Format("{0}_SAP.txt", name));
                    BackupData(fileName2, string.Join(Environment.NewLine, data.Tables.Select(t => t.ToString())));
                }
                UpSMDT(item.SMID, result.IsSucceed);
                Utils.EdiInfo v = null;
                if (result.IsSucceed)
                {
                    v = log.CreateSucceed();
                }
                else
                {
                    v = log.CreateEx(string.Join(",", result.ResultCode, result.Description));
                }
                v.DataFolder = string.Join(Environment.NewLine, fileName, fileName2);
                Logger.WriteLog(v);
            }
            return result;
        }

        IEnumerable<CargoInfo> CreateInfos(DataTable smDT, DataTable partyDT, DataTable stDT)
        {
            if (smDT == null || smDT.Rows.Count <= 0) yield break;
            Func<object, DateTime?> getDateTime = obj =>
                {
                    if (obj == null || obj == DBNull.Value) return null;
                    return (DateTime)obj;
                };
            Func<string, DataRow, string> getPartyNO = (partyType, smRow) =>
                {
                    if (partyDT == null || partyDT.Rows.Count <= 0) return null;
                    string id = Prolink.Math.GetValueAsString(smRow["U_ID"]);
                    DataRow[] rows = partyDT.Select(string.Format("U_FID={0} AND PARTY_TYPE={1}",
                        SQLUtils.QuotedStr(id), SQLUtils.QuotedStr(partyType)));
                    if (rows == null || rows.Length <= 0) return null;
                    return Prolink.Math.GetValueAsString(rows[0]["PARTY_NO"]);
                };
            Func<DataRow, string, string, string> getVessel = (row, veC, voC) =>
                {
                    List<string> items = new List<string>();
                    Action<string> add = (v) =>
                        {
                            if (string.IsNullOrEmpty(v)) return;
                            items.Add(v);
                        };
                    add(Prolink.Math.GetValueAsString(row[veC]));
                    add(Prolink.Math.GetValueAsString(row[voC]));
                    return string.Join(" ", items);
                };
            foreach (DataRow row in smDT.Rows)
            {
                string billno=Prolink.Math.GetValueAsString(row["MASTER_NO"]);
                if("S".Equals(Prolink.Math.GetValueAsString(row["ISCOMBINE_BL"]))&& !string.IsNullOrEmpty(Prolink.Math.GetValueAsString(row["C_MASTER_NO"]))){
                     billno=Prolink.Math.GetValueAsString(row["C_MASTER_NO"]);
                }
                CargoInfo info = new CargoInfo()
                 {
                     SMID = Prolink.Math.GetValueAsString(row["U_ID"]),
                     GroupId = Prolink.Math.GetValueAsString(row["GROUP_ID"]),
                     CMP = Prolink.Math.GetValueAsString(row["CMP"]),
                     STN = Prolink.Math.GetValueAsString(row["STN"]),
                     DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),
                     SAP_ID = Prolink.Math.GetValueAsString(row["SAP_ID"]),
                     ATD = getDateTime(row["ATD"]),
                     Vessel = getVessel(row, "VESSEL1", "VOYAGE1"),
                     Broker = getPartyNO(PartyCode_Broker, row),
                     Trailer = getPartyNO(PartyCode_Trailer, row),
                     Transport = getPartyNO(PartyCode_Transport, row),
                     Forwarder = getPartyNO(PartyCode_Forwarder, row),
                     BookingAgent = getPartyNO(PartyCode_BookingAgent, row),
                     Vessel2 = getVessel(row, "VESSEL2", "VOYAGE2"),
                     BillNO =billno,
                     ETD2 = getDateTime(row["ETD2"]),
                     ETA2 = getDateTime(row["ETA2"]),
                     Vessel3 = getVessel(row, "VESSEL3", "VOYAGE3"),
                     ETD3 = getDateTime(row["ETD3"]),
                     ETA3 = getDateTime(row["ETA3"]),
                     POD = Prolink.Math.GetValueAsString(row["POD_NAME"]),
                     ETA = getDateTime(row["ETA"]),
                     ATA3 = getDateTime(row["ATA"]),
                     ATA = getDateTime(row["ATA_D"]),
                     //ContainerNO = Prolink.Math.GetValueAsString(row["CNTR_NO"]),
                     //SealNO = Prolink.Math.GetValueAsString(row["SEAL_NO1"]),
                     POL = Prolink.Math.GetValueAsString(row["DEST_NAME"]),
                     Remarks = DateTime.Now.ToString()
                     //info.DaysInTransit=Prolink.Math.GetValueAsString(row[""]);
                     //info.GapFor=Prolink.Math.GetValueAsString(row[""]);
                     //info.Remarks=Prolink.Math.GetValueAsString(row[""]);
                 };
                FillSMRV(info, row);
                SecondStatusInfo secondInfo = GetSecondCargoInfo(stDT, Prolink.Math.GetValueAsString(row["SHIPMENT_ID"]));
                if (secondInfo != null)
                    FillSecondStatusInfo(info, secondInfo, row);
                yield return info;
            }
        }
        void FillSMRV(CargoInfo info, DataRow smRow)
        {
            string shipmentId = Prolink.Math.GetValueAsString(smRow["SHIPMENT_ID"]);
            DataTable smrvDT = QuerySMRV(shipmentId, info.DNNO);
            List<CNTRInfo> items = CreateCNTRInfos(smrvDT,info).ToList();
            info.Containers = items;
            Func<Func<CNTRInfo, DateTime?>, DateTime?> getMinDate = getDate =>
                {
                    if (items == null) return null;
                    var values = items.Select(item => getDate(item)).Where(v => v.HasValue).ToList();
                    if (values == null || values.Count <= 0) return null;
                    return values.Min(x => x.Value);
                };
            info.InDate = getMinDate(item => item.InDate);
            info.OutDate = getMinDate(item => item.OutDate);
            info.SealDate = getMinDate(item => item.SealDate);
        }
        List<string> serList = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        IEnumerable<CNTRInfo> CreateCNTRInfos(DataTable smrvDT, CargoInfo info)
        {
            if (smrvDT == null || smrvDT.Rows.Count <= 0) yield break;
            Func<DataRow, string, DateTime?> getValueD = (row, column) =>
                {
                    object v = row[column];
                    if (v == null || v == DBNull.Value) return null;
                    return (DateTime)v;
                };
            int n = 0;
            foreach (DataRow row in smrvDT.Rows)
            {
                n++;
                yield return new CNTRInfo
                {
                    ContainerNO = Prolink.Math.GetValueAsString(row["CNTR_NO"]),
                    //DNNO = Prolink.Math.GetValueAsString(row["DN_NO"]),//这里面有合并多个的，SAP接收会有问题
                    DNNO=Prolink.Math.GetValueAsString(info.DNNO),
                    SealNO = Prolink.Math.GetValueAsString(row["SEAL_NO1"]),
                    SizeCode = Prolink.Math.GetValueAsString(row["SIZECODE"]),
                    SEQ = n.ToString(),
                    SerialNO = serList[n - 1],
                    InDate = getValueD(row, "IN_DATE"),
                    OutDate = getValueD(row, "OUT_DATE"),
                    SealDate = getValueD(row, "SEAL_DATE")
                };
            }
        }

        void FillSecondStatusInfo(CargoInfo info, SecondStatusInfo secondInfo, DataRow smRow)
        {
            info.LoadingPort2 = secondInfo.LoadingPort;
            info.ATD2 = secondInfo.ATD;
            info.ATA2 = secondInfo.ATA;
            info.UnLoadingPort2 = secondInfo.UnLoadingPort;
            if (secondInfo.NextInfo != null)
            {
                info.LoadingPort3 = secondInfo.NextInfo.LoadingPort;
                info.UnLoadingPort3 = secondInfo.NextInfo.UnLoadingPort;
                info.ATD3 = secondInfo.NextInfo.ATD;
                info.ATA3 = secondInfo.NextInfo.ATA;
            }
        }

        const string PartyCode_Trailer = "CR";
        const string PartyCode_Transport = "FS";
        const string PartyCode_Broker = "BR";
        const string PartyCode_Forwarder = "SP";
        const string PartyCode_BookingAgent = "BO";
        DataTable QuerySMDT()
        {
            string sql = @"SELECT TOP 20 SM.GROUP_ID,SM.CMP,SM.STN,SM.SHIPMENT_ID,SM.ATA,SM.ATA_D,SM.POL_NAME,SM.U_ID,SM.ATD,SM.OEXPORTER,
SM.VESSEL1,SM.VESSEL2,SM.VESSEL3,SM.VOYAGE1,SM.VOYAGE2,SM.VOYAGE3,SM.VOYAGE4,SM.MASTER_NO,SM.C_MASTER_NO,SM.ISCOMBINE_BL,SM.POD_NAME,SM.DEST_CD,SM.DEST_NAME,
SM.ETA,SM.ETA2,SM.ETA3,SM.ETD,SM.ETD2,SM.ETD3,DN.DN_NO,DN.SAP_ID FROM SMSM SM,SMDN DN WHERE (POST_CARGO_FLAG IS NULL OR POST_CARGO_FLAG='N')
 AND DN.SHIPMENT_ID=SM.SHIPMENT_ID AND (SM.ATD IS NOT NULL OR SM.ATA IS NOT NULL OR SM.ATA_D IS NOT NULL OR SM.STATUS='O') AND SM.POST_CARGO_DATE IS NULL";
            return DB.GetDataTable(sql, new string[] { });
        }
        DataTable QuerySMRV(string shipmentId, string dnNo)
        {
            string sql = string.Format(@"SELECT DN_NO,CNTR_NO,SEAL_NO1,IN_DATE,SEAL_DATE,OUT_DATE,(SELECT TOP 1 AP_CD FROM BSCODE B WHERE B.CD_TYPE='VERP' AND 
B.CD=SMRV.CNT_TYPE) AS SIZECODE FROM SMRV WHERE CNTR_NO IS NOT NULL AND SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId));
            return DB.GetDataTable(sql, new string[] { });
        }
        DataTable QueryPartyDT(List<string> idList)
        {
            if (idList == null || idList.Count <= 0) return null;
            string sql = string.Format("SELECT U_FID,PARTY_TYPE,PARTY_NO FROM SMSMPT WHERE U_FID IN({0}) AND PARTY_TYPE IN({1})",
                 string.Join(",", idList.Select(s => SQLUtils.QuotedStr(s))), string.Join(",", new List<string>{
                 PartyCode_Broker,PartyCode_Trailer,PartyCode_Transport,PartyCode_Forwarder,PartyCode_BookingAgent}.Select(s => SQLUtils.QuotedStr(s))));
            return DB.GetDataTable(sql, new string[] { });
        }
        DataTable QueryStatus(List<string> shipmentIDList)
        {
            if (shipmentIDList == null || shipmentIDList.Count <= 0) return null;
            string sql = string.Format("SELECT * FROM TKBLST WHERE SHIPMENT_ID IN({0})", string.Join(",", shipmentIDList.Select(s => SQLUtils.QuotedStr(s))));
            return DB.GetDataTable(sql, new string[] { });
        }

        class SecondStatusInfo
        {
            public string UnLoadingPort { get; set; }
            public string LoadingPort { get; set; }
            public DateTime? ATD { get; set; }
            public DateTime? ATA { get; set; }
            public SecondStatusInfo NextInfo { get; set; }
        }
        SecondStatusInfo GetSecondCargoInfo(DataTable stDT, string shipmentId)
        {
            if (stDT == null || stDT.Rows.Count <= 0) return null;
            DataRow[] rows = stDT.Select(string.Format("SHIPMENT_ID={0}", SQLUtils.QuotedStr(shipmentId)), "EVEN_DATE");
            if (rows == null || rows.Length <= 0) return null;
            const string loadedOnBoardCode = "330";
            const string dischargedCode = "510";
            int loadedOnBoardCount = 0;
            int dischargedCount = 0;
            Func<object, DateTime?> getDateV = obj =>
                {
                    if (obj == null || obj == DBNull.Value) return null;
                    return (DateTime)obj;
                };
            Action<SecondStatusInfo, SecondStatusInfo, DataRow> setInfo = (pInfo, info, row) =>
            {
                string stsCode = Prolink.Math.GetValueAsString(row["STS_CD"]);
                string port = Prolink.Math.GetValueAsString(row["LOCATION"]);
                DateTime? time = getDateV(row["EVEN_DATE"]);
                switch (stsCode)
                {
                    case loadedOnBoardCode: info.ATD = time;
                        info.LoadingPort = port; break;
                    case dischargedCode: info.ATA = time;
                        info.UnLoadingPort = port; break;
                }
                if (pInfo != null)
                    pInfo.NextInfo = info;
            };
            SecondStatusInfo child = null;
            SecondStatusInfo parent = new SecondStatusInfo();
            SecondStatusInfo rootParent = parent;
            foreach (var row in rows)
            {
                string stsCode = Prolink.Math.GetValueAsString(row["STS_CD"]);
                switch (stsCode)
                {
                    case loadedOnBoardCode: loadedOnBoardCount++; break;
                    case dischargedCode: dischargedCount++; break;
                    default: continue;
                }
                switch (loadedOnBoardCount)
                {
                    case 1: continue;
                    case 2: setInfo(null, parent, row); break;
                    default:
                        switch (stsCode)
                        {
                            case loadedOnBoardCode:
                                if (child != null)
                                {
                                    parent = child;
                                    child = new SecondStatusInfo();
                                }
                                break;
                        }
                        if (child == null)
                            child = new SecondStatusInfo();
                        setInfo(parent, child, row);
                        break;
                }
            }
            return rootParent;
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
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(id));
            //if (succeed)//不管失败与否都不要再次发送货况
            ei.Put("POST_CARGO_FLAG", "Y");
            ei.PutExpress("POST_CARGO_DATE", "getdate()");
            DB.ExecuteUpdate(ei);
        }

        IPlanTaskMessenger _messenger;
        public void Run(IPlanTaskMessenger messenger)
        {
            try
            {
                _messenger = messenger;
                DataTable smDT = QuerySMDT();
                List<string> idList = GetIDList(smDT, "U_ID");
                DataTable partyDT = QueryPartyDT(idList);
                List<string> shipmentIDList = GetIDList(smDT, "SHIPMENT_ID");
                DataTable stDT = QueryStatus(shipmentIDList);
                var result = TryPostCargoInfo(CreateInfos(smDT, partyDT, stDT),_location);
            }
            catch (Exception ex)
            {
                PostCargoEDILog log = new PostCargoEDILog(null, "TaskSystem");
                Logger.WriteLog(log.CreateEx(ex));
            }
        }
    }
}
