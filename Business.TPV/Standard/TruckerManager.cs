using Business.Service;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public class TransloadingResponse : TruckManage<TruckerInfo>
    {
        protected override EditInstructList ToEi(TruckerInfo obj,out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            EditInstructList eiList = new EditInstructList();
            string cntrno = obj.ContainerNo;
            string cntrqty = obj.EachCntrQty;
            string[] cntrnos=cntrno.Split(';');
            string[] qtys = cntrqty.Split(';');
            int qtycount=0;
            foreach(string qty in qtys){
                qtycount+=Prolink.Math.GetValueAsInt(qty);
            }
            MixedList mlist = new MixedList();
            ri.RefNo = cntrno;
            string sql = string.Format("SELECT RESERVE_NO FROM SMIRV WHERE RV_TYPE='I' AND TRUCK_NO={0} AND STATUS NOT IN ('U','O','V')", SQLUtils.QuotedStr(obj.NewTrailerNo));
            string creserveno = DB.GetValueAsString(sql);
            if (string.IsNullOrEmpty(creserveno))
            {
                if ("A".Equals(obj.Mode) || "C".Equals(obj.Mode))
                {
                    ri.RefNo = obj.ContainerNo;
                    ri.IsSucceed = false;
                    ri.Description = "This NewTrailerNo:" + obj.NewTrailerNo + " cann't find,So you cann't add or cancel this info!";
                    return null;
                }
            }
            else if ("N".Equals(obj.Mode))
            {
                ri.RefNo = obj.ContainerNo;
                ri.IsSucceed = false;
                ri.Description = "The system already exists This NewTrailerNo:"+ obj.NewTrailerNo +",So cann't be created!";
                return null;
            }
            string combineReserveno = creserveno;
            sql = string.Format("SELECT base_CMP FROM GFcompany where cmp={0}", SQLUtils.QuotedStr(obj.Sender));
            string BaseCompanyId = DB.GetValueAsString(sql);
            if (string.IsNullOrEmpty(BaseCompanyId))
            {
                ri.RefNo = obj.ContainerNo;
                ri.IsSucceed = false;
                ri.Description = "The company information:"+obj.Sender+" corresponding to the sender could not be found!";
                return null;
            }

            TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
            {
                UserId = obj.Sender,
                CompanyId = obj.Sender,
                GroupId = "TPV",
                IoFlag = "O",
                BaseCompanyId = BaseCompanyId
            };

            sql = string.Format(@"SELECT (SELECT TOP 1 PLANT FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS PLANT1,SMRCNTR.*,SMIRV.STATUS,SMIRV.CALL_DATE FROM SMRCNTR,SMIRV WHERE SMIRV.RV_TYPE='I' AND
                        SMRCNTR.RESERVE_NO=SMIRV.RESERVE_NO AND
                        SMRCNTR.CNTR_NO IN {0}  AND (SMRCNTR.OLD_RESERVE_NO IS NULL OR SMRCNTR.OLD_RESERVE_NO='')", SQLUtils.Quoted(cntrnos));
            string sqlfinal = sql + " AND SMRCNTR.FINAL_WH='Final'";
            DataTable smrvdt = DB.GetDataTable(sqlfinal, new string[] { });
            if (smrvdt.Rows.Count <= 0)
            {
                ri.RefNo = cntrno;
                ri.IsSucceed = false;
                ri.Description = string.Format("This ContainerNo.{0} cann't find,So you cann't add or cancel this info!", cntrno);
                smrvdt = DB.GetDataTable(sql, new string[] { });
                if (smrvdt.Rows.Count > 0)
                {
                    string plant1 = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["PLANT1"]);
                    string OrdNo = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["ORD_NO"]);
                    if (!string.IsNullOrEmpty(plant1))
                    {
                        string company = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CMP"]);
                        TrackingEDI.InboundBusiness.IBUserInfo ibuserinfo = new TrackingEDI.InboundBusiness.IBUserInfo
                        {
                            UserId = obj.Sender,
                            CompanyId = company,
                            GroupId = "TPV"
                        };
                        MixedList mixList = new MixedList();
                        Dictionary<string, object> json = new Dictionary<string, object>();

                        string sqlsmwh = string.Format("SELECT * FROM SMWH WHERE WS_CD={0}", SQLUtils.QuotedStr(plant1));
                        DataTable smwhdt = DB.GetDataTable(sqlsmwh, new string[] { });
                        string DestCd = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA"]);
                        string DestName = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA_NM"]);
                        string AddrCode = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR"]);
                        string Addr = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR_NM"]);

                        string sql1 = string.Format("SELECT TOP 1 CUSTOMER_CODE FROM BSADDR WHERE ADDR_CODE={0}", SQLUtils.QuotedStr(AddrCode));
                        string CustermCode1 = DB.GetValueAsString(sql1);
                        sql1 = string.Format("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(CustermCode1));
                        string CustermName1 = DB.GetValueAsString(sql1);

                        sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                        string ArrivalDate = DB.GetValueAsString(sql);
                        json.Add("OrdNo", OrdNo);
                        json.Add("DestCd", DestCd);
                        json.Add("DestName", DestName);
                        json.Add("AddrCode", AddrCode);
                        json.Add("Addr", Addr);
                        json.Add("CustermCode1", CustermCode1);
                        json.Add("CustermName1", CustermName1);
                        json.Add("ArrivalDate1", ArrivalDate);
                        List<string> returnMsg = new List<string>();
                        List<string> ordnos = new List<string>();
                        string updatesql = string.Format("UPDATE SMORD SET CSTATUS='T' WHERE ORD_NO={0}", SQLUtils.QuotedStr(OrdNo));
                        DB.ExecuteUpdate(updatesql);
                        updatesql = string.Format("UPDATE SMIRV SET STATUS='O' WHERE ORD_NO={0}", SQLUtils.QuotedStr(OrdNo));
                        DB.ExecuteUpdate(updatesql);
                        TrackingEDI.InboundBusiness.SMSMIHelper.ReBuildOrderIndex(json, mixList, returnMsg, ordnos, ibuserinfo);
                        try
                        {
                            OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                            //这里调用重新叫车的操作
                            if (ordnos.Count > 0)
                            {
                                OrdNo = ordnos[0];
                                sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                                DataTable orddt = DB.GetDataTable(sql, new string[] { });

                                sql = "SELECT * FROM SMRCNTR WHERE ORD_NO={0} AND CNTR_NO={1}";
                                sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(obj.ContainerNo));
                                DataTable cdt = DB.GetDataTable(sql, new string[] { });

                                sql = string.Format("SELECT TOP 1 PICKUP_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} AND PICKUP_DATE IS NOT NULL ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                                string PickupDate = DB.GetValueAsString(sql);
                                string CallData = Prolink.Math.GetValueAsString(smrvdt.Rows[0]["CALL_DATE"]);
                                if (string.IsNullOrEmpty(PickupDate))
                                    PickupDate = CallData;
                                if (string.IsNullOrEmpty(PickupDate))
                                {
                                    sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0}  AND ARRIVAL_DATE IS NOT NULL ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                                    PickupDate = DB.GetValueAsString(sql);
                                }
                                List<string> EtaMsl = new List<string>();
                                //EtaMsl.Add(EtaMsl1);
                                //EtaMsl.Add(EtaMslTime1);

                                string WsCd = plant1;
                                string CntrNo = obj.ContainerNo;
                                List<string> idList = new List<string>();
                                List<string> newReserveNo = new List<string>();
                                TrackingEDI.InboundBusiness.SMSMIHelper.InboundFclOrderTrucker(orddt.Rows[0], cdt.Rows[0], PickupDate,EtaMsl, ArrivalDate, WsCd, obj.Sender, "", "", idList, newReserveNo);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                return null;
            }
            string reserveno = smrvdt.Rows[0]["RESERVE_NO"].ToString();
            List<string> reservelist = new List<string>();
            foreach (DataRow dr in smrvdt.Rows)
            {
                reservelist.Add(Prolink.Math.GetValueAsString(dr["RESERVE_NO"]));
            }

            if ("A".Equals(obj.Mode))
            {
                string reservenos=string.Join(",", reservelist);
                mlist=TrackingEDI.InboundBusiness.SMSMIHelper.TSAAddContainerInRV(creserveno, reservenos,out combineReserveno, userinfo);
            }
            else if ("N".Equals(obj.Mode))
            {
                string msg = TrackingEDI.InboundBusiness.SMSMIHelper.TSATransloading(reserveno, reservelist.ToArray(), out mlist, out combineReserveno, userinfo);
            }
            for (int i = 0; i < mlist.Count; i++)
            {
                eiList.Add((EditInstruct)mlist[i]);
            }
             EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format(" RESERVE_NO={0} ", SQLUtils.QuotedStr(combineReserveno));
            ei.PutDate("ARRIVAL_DATE", obj.ReceivedDate);
            ei.Put("TRUCK_NO", obj.NewTrailerNo);
            ei.Put("CNTR_NO", obj.ContainerNo);
            ei.Put("PALLET_QTY", qtycount);
            ei.PutDate("USE_DATE", obj.ShipDate);
            ei.PutDate("RESERVE_DATE", obj.NewEtaHub);
            ei.PutDate("ETA_RAILRAMP_DATE", obj.EtaRailRampDate);
            ei.Put("DRIVER", obj.Driver);
            ei.Put("LDRIVER", obj.Driver);
            ei.Put("DRIVER_ID", obj.CardId);
            ei.Put("LDRIVER_ID", obj.CardId);
            ei.Put("TEL", obj.Mobile);
            ei.Put("LTEL", obj.Mobile);
            eiList.Add(ei);
            return eiList;
        }
    }
    public class DeliveryTruckResponse : TruckManage<TruckerInfoR>
    {
        protected override EditInstructList ToEi(TruckerInfoR obj, out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            ri.RefNo = obj.ContainerNo;
            EditInstructList eiList = new EditInstructList();
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            string[] shipmentids = Prolink.Math.GetValueAsString(obj.ShipmentId).Split('|');
            string shipment_id = shipmentids[0];  //因为传回来的Shipment为  shipmentid|containerNo  所以用|分割开来
            string sql = string.Format(@"SELECT (SELECT TOP 1 PLANT FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS PLANT1 FROM SMIRV 
                    WHERE RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('O','V')",
                     SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
            string plant1 = DB.GetValueAsString(sql);
            string condition = string.Format(" RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('V')", SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
            ei.Condition = condition;

            sql = string.Format("SELECT DISTINCT ORD_NO,SHIPMENT_ID FROM SMIRV WHERE {0} ", condition);
            DataTable dt = DB.GetDataTable(sql, new string[] { });

            bool isupdate = false;

            EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            EditInstruct smrcntrei = new EditInstruct("SMRCNTR", EditInstruct.UPDATE_OPERATION);
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            if (dt.Rows.Count > 0 && dt.Rows.Count < 20)
            {
                List<string> ordlist = new List<string>();
                List<string> smlist = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    ordlist.Add(Prolink.Math.GetValueAsString(dr["ORD_NO"]));
                    smlist.Add(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                }
                smordei.Condition = string.Format("ORD_NO IN {0}", SQLUtils.Quoted(ordlist.ToArray()));
                smrcntrei.Condition = string.Format("ORD_NO IN {0}", SQLUtils.Quoted(ordlist.ToArray()));
                smsmiei.Condition = string.Format("SHIPMENT_ID IN {0}", SQLUtils.Quoted(smlist.ToArray()));
            }
            else
            {
                smordei.Condition = "1=0";
                smsmiei.Condition = "1=0";
                smrcntrei.Condition = "1=0";
            }

            ei.PutDate("HEAVY_PICKUP_TIME", obj.HeavyPickupDate);
            ei.PutDate("EMPTY_RETURN_TIME", obj.EmptyReturnDate);
            smrcntrei.PutDate("EMPTY_TIME", obj.EmptyReturnDate);
            ei.PutDate("AT_YARD_TIME", obj.AtYardDate);
            ei.PutDate("RESERVE_DATE", obj.PortDate);
            //ei.Put("RESERVE_FROM", obj.PortTime);
            //ei.PutDate("EMPTY_RETURN_TIME",obj.TerminDate);
            ei.Put("TRUCK_RMK", obj.Remark);
            ei.Put("DRIVER", obj.Driver);
            ei.Put("LDRIVER", obj.Driver);
            ei.Put("DRIVER_ID", obj.CardId);
            ei.Put("LDRIVER_ID", obj.CardId);
            ei.Put("TEL", obj.Mobile);
            ei.Put("LTEL", obj.Mobile);

            Business.TPV.Standard.ChageSmRVStatus cs = new Business.TPV.Standard.ChageSmRVStatus();

            string filter = "SELECT RESERVE_NO FROM SMIRV WHERE " + condition +" AND STATUS NOT IN ('O')";
            DataTable rvdt = cs.GetSMRVDetail(filter);
            bool isupdatesmrvStatus = true;
            foreach (DataRow smrvD in rvdt.Rows)
            {
                if ("Temp".Equals(Prolink.Math.GetValueAsString(smrvD["FINAL_WH"])) && "Y".Equals(Prolink.Math.GetValueAsString(smrvD["OUTER_FLAG"])))
                {
                    isupdatesmrvStatus = false;
                }
            }

            if (string.IsNullOrEmpty(plant1))//如果没有值，表示走Delivery 的情况，有值那就是Transloading
            {
                ei.Put("STATUS", "W");
                ei.Put("IS_COMBINE_DP", "D");
                //ei.PutExpress("TRUCK_NO", "CNTR_NO");
                if (obj.AtYardDate.HasValue)
                {
                    ei.PutDate("IN_DATE_L", obj.AtYardDate);
                    ei.Put("STATUS", "I");
                    smordei.Put("CSTATUS", "I");
                    smsmiei.Put("STATUS", "G");
                    isupdate = true;
                }
                if (obj.LeaveYardDate.HasValue)
                {
                    ei.PutDate("OUT_DATE_L", obj.LeaveYardDate);
                    if (isupdatesmrvStatus)
                    {
                        ei.Put("STATUS", "O");
                    }
                    smordei.Put("CSTATUS", "O");
                    smsmiei.Put("STATUS", "O");
                    isupdate = true;
                }
            }
            if (isupdate)
            {
                eiList.Add(smordei);
                eiList.Add(smsmiei);
            }
            eiList.Add(smrcntrei);
            eiList.Add(ei);
            return eiList;
        }
    }
    public class DeliveryWareHouseResponse : TruckManage<TruckerInfoW>
    {
        protected override EditInstructList ToEi(TruckerInfoW obj, out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            ri.RefNo = obj.NewTrailerNo;
            string condition=string.Format("RV_TYPE='I' AND TRUCK_NO={0}", SQLUtils.QuotedStr(obj.NewTrailerNo));
            EditInstructList eiList = new EditInstructList();
            EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
            ei.Condition = condition;
            ei.Put("TRUCK_NO", obj.NewTrailerNo);
            string status=string.Empty;
            string smistatus = string.Empty;
            string sql = string.Format("SELECT DISTINCT ORD_NO,SHIPMENT_ID FROM SMIRV WHERE {0}", condition);
            DataTable dt= DB.GetDataTable(sql, new string[] { });

            bool isupdate = false;

            EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
            EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
            if (dt.Rows.Count > 0 && dt.Rows.Count < 20)
            {
                List<string> ordlist = new List<string>();
                List<string> smlist = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    ordlist.Add(Prolink.Math.GetValueAsString(dr["ORD_NO"]));
                    smlist.Add(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                }
                smordei.Condition = string.Format("ORD_NO IN {0}", SQLUtils.Quoted(ordlist.ToArray()));
                smsmiei.Condition = string.Format("SHIPMENT_ID IN {0}", SQLUtils.Quoted(smlist.ToArray()));
            }
            else
            {
                smordei.Condition = "1=0";
                smsmiei.Condition = "1=0";
            }


            if (obj.GateInDate.HasValue)
            {
                ei.PutDate("IN_DATE_L", obj.GateInDate);
                status = "I";
                smistatus = "G";
                isupdate = true;
            }
            if (obj.PodDate.HasValue)
            {
                ei.PutDate("POD_UPDATE_DATE", obj.PodDate);
                status = "U";
                smistatus = "P";
                isupdate = true;
            }
            if (obj.GateOutDate.HasValue)
            {
                ei.PutDate("OUT_DATE_L", obj.GateOutDate);
                status = "O";
                smistatus = "O";
                isupdate = true;
            }
            if (!string.IsNullOrEmpty(status))
            {
                ei.Put("STATUS", status);
                smordei.Put("CSTATUS", status);
                smsmiei.Put("STATUS", smistatus);
            }
            eiList.Add(ei);
            if (isupdate)
            {
                eiList.Add(smordei);
                eiList.Add(smordei);
            }
            return eiList;
        }
    }

    public class TruckerManagerF : TruckManage<TruckerInfoF>
    {
        protected override EditInstructList ToEi(TruckerInfoF obj, out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            ri.RefNo = obj.ContainerNo;
            EditInstructList eiList = new EditInstructList();
            string[] shipments=obj.ShipmentId.Split('|');
            string shipment_id = string.Empty;
            if (shipments.Length > 0)
                shipment_id = shipments[0];
            string sql = string.Format(@"SELECT (SELECT TOP 1 PLANT FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS PLANT1,* FROM SMIRV 
                    WHERE RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('O','V')",
                     SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count == 0)
            {
                ri.IsSucceed = false;
                ri.Description = "This ContainerNo cann't find,So you cann't Upload POD!";
                return null;
            }
            DataRow smRow = dt.Rows[0];
            byte[] imageBytes = Convert.FromBase64String(obj.OutputImage);
            string fileName = string.Format("{0}_{1}", obj.ContainerNo, DateTime.Now.ToString("HHmmssfff"));
            string ext = Prolink.Math.GetValueAsString(obj.OutputFormat).Replace(".", "");
            if (string.IsNullOrEmpty(ext) || ext.Contains("image") || ext.Contains("jpeg"))
            {
                ext = "jpg";
            }
            string filePath = CreateBaseDirectoryFileName(new List<string> { "Export", "PodFile" }, fileName, ext);
            CreateDir(filePath);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }

            if (string.IsNullOrEmpty(filePath)) return null;
            string userid = Prolink.Math.GetValueAsString(smRow["CREATE_BY"]);
            string companyid = Prolink.Math.GetValueAsString(smRow["CMP"]);
            string plant1 = Prolink.Math.GetValueAsString(smRow["PLANT1"]);
            EDocInfo info = Helper.CreateShipmentEDocInfo(smRow);

            info.UserId = userid;
            info.DocType = "POD";
            info.FilePath = filePath;
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    //context.Result.IsSucceed = false;
                    //context.Result.ResultCode = uploadResult.Status.ToString();
                    //context.Result.Description = "运单申请成功，但上传运单文档失败！";
                    ri.IsSucceed = false;
                    ri.Description = "Upload Pod Image failed!";
                    return null;
                }
                else
                {
                    //更新上传文件的文件者
                    //sql = string.Format("update Files set uploader={0} WHERE FileID={1}",
                       // SQLUtils.QuotedStr(userid),
                        //SQLUtils.QuotedStr(uploadResult.FileInfo.FileID));
                    //DB.ExecuteUpdate(sql);
                }
                //POD上传完毕，要执行Rebuild Order：
                string OrdNo = Prolink.Math.GetValueAsString(smRow["ORD_NO"]);
                if (!string.IsNullOrEmpty(plant1))
                {
                    //更新之前的状态：
                    string reserveno = Prolink.Math.GetValueAsString(smRow["RESERVE_NO"]);
                    string updatestatussql = string.Format("UPDATE SMIRV SET  STATUS ='O' WHERE RESERVE_NO={0}",
                    SQLUtils.QuotedStr(reserveno));
                    DB.ExecuteUpdate(updatestatussql);

                    TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
                    {
                        UserId = userid,
                        CompanyId = companyid,
                        GroupId = "TPV"
                    };
                    MixedList mixList = new MixedList();
                    Dictionary<string, object> json = new Dictionary<string, object>();

                    string sqlsmwh = string.Format("SELECT * FROM SMWH WHERE WS_CD={0}", SQLUtils.QuotedStr(plant1));
                    DataTable smwhdt = DB.GetDataTable(sqlsmwh, new string[] { });
                    string DestCd = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA"]);
                    string DestName = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA_NM"]);
                    string AddrCode = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR"]);
                    string Addr = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR_NM"]);

                    string sql1 = string.Format("SELECT TOP 1 CUSTOMER_CODE FROM BSADDR WHERE ADDR_CODE={0}", SQLUtils.QuotedStr(AddrCode));
                    string CustermCode1 = DB.GetValueAsString(sql1);
                    sql1 = string.Format("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(CustermCode1));
                    string CustermName1 = DB.GetValueAsString(sql1);

                    sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                    string ArrivalDate = DB.GetValueAsString(sql);
                    json.Add("OrdNo", OrdNo);
                    json.Add("DestCd", DestCd);
                    json.Add("DestName", DestName);
                    json.Add("AddrCode", AddrCode);
                    json.Add("Addr", Addr);
                    json.Add("CustermCode1", CustermCode1);
                    json.Add("CustermName1", CustermName1);
                    json.Add("ArrivalDate1", ArrivalDate);
                    List<string> returnMsg = new List<string>();
                    List<string> ordnos = new List<string>();
                    TrackingEDI.InboundBusiness.SMSMIHelper.ReBuildOrderIndex(json, mixList, returnMsg, ordnos, userinfo);
                    try
                    {
                        OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //这里调用重新叫车的操作
                        if (ordnos.Count > 0)
                        {
                            OrdNo = ordnos[0];
                            sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                            DataTable orddt = DB.GetDataTable(sql, new string[] { });

                            sql = "SELECT * FROM SMRCNTR WHERE ORD_NO={0} AND CNTR_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(obj.ContainerNo));
                            DataTable cdt = DB.GetDataTable(sql, new string[] { });

                            sql = string.Format("SELECT TOP 1 PICKUP_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                            string PickupDate = DB.GetValueAsString(sql);
                            string CallData = Prolink.Math.GetValueAsString(smRow["CALL_DATE"]);
                            if (string.IsNullOrEmpty(PickupDate))
                                PickupDate = CallData;
                            if (string.IsNullOrEmpty(PickupDate))
                            {
                                sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                                PickupDate = DB.GetValueAsString(sql);
                            }
                            List<string> EtaMsl = new List<string>();
                            //EtaMsl.Add(EtaMsl1);
                            //EtaMsl.Add(EtaMslTime1);

                            string WsCd = plant1;
                            string CntrNo = obj.ContainerNo;
                            List<string> idList = new List<string>();
                            List<string> newReserveNo = new List<string>();
                            TrackingEDI.InboundBusiness.SMSMIHelper.InboundFclOrderTrucker(orddt.Rows[0], cdt.Rows[0], PickupDate, EtaMsl, ArrivalDate, WsCd, userid, "", "", idList, newReserveNo);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                    string condition = string.Format(" RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('O','V')", SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
                    ei.Condition = condition; 
                    ei.PutDate("POD_UPDATE_DATE", DateTime.Now);
                    ei.Put("STATUS", "U");
                    eiList.Add(ei);

                    sql = string.Format("SELECT DISTINCT ORD_NO,SHIPMENT_ID FROM SMIRV WHERE {0}", condition);
                    DataTable dt1 = DB.GetDataTable(sql, new string[] { });

                    bool isupdate = false;

                    EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                    EditInstruct smsmiei = new EditInstruct("SMSMI", EditInstruct.UPDATE_OPERATION);
                    if (dt1.Rows.Count > 0 && dt1.Rows.Count < 20)
                    {
                        List<string> ordlist = new List<string>();
                        List<string> smlist = new List<string>();
                        foreach (DataRow dr in dt1.Rows)
                        {
                            ordlist.Add(Prolink.Math.GetValueAsString(dr["ORD_NO"]));
                            smlist.Add(Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]));
                        }
                        smordei.Condition = string.Format("ORD_NO IN {0}", SQLUtils.Quoted(ordlist.ToArray()));
                        smsmiei.Condition = string.Format("SHIPMENT_ID IN {0}", SQLUtils.Quoted(smlist.ToArray()));
                    }
                    else
                    {
                        smordei.Condition = "1=0";
                        smsmiei.Condition = "1=0";
                    }
                    smordei.Put("CSTATUS", "U");
                    smsmiei.Put("STATUS", "P");
                    eiList.Add(smordei);
                    eiList.Add(smsmiei);
                }
            }
            catch (Exception ex)
            {
                string msg = "POD文档上传失败！";
                //context.Result.IsSucceed = false;
                //context.Result.ResultCode = "EDOCUploadFileErr";
                //context.Result.Description = string.Format("{0}{1}{2}", msg, Environment.NewLine, ex.Message);
                Logger.WriteLog(msg, ex);
            }
            return eiList;
        }
        protected string ImgToBase64String(string ImagePath)
        {
            try
            {
                //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(System.Web.HttpContext.Current.Server.MapPath(ImagePath));
                //MemoryStream ms = new MemoryStream();
                //bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //byte[] arr = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(arr, 0, (int)ms.Length);
                //ms.Close();
                var arr = File.ReadAllBytes(ImagePath);
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class TruckerUploadTempImg : TruckManage<TruckerInfoF>
    {
        protected override EditInstructList ToEi(TruckerInfoF obj, out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            ri.RefNo = obj.ContainerNo;
            EditInstructList eiList = new EditInstructList();
            string[] shipments = obj.ShipmentId.Split('|');
            string shipment_id = string.Empty;
            if (shipments.Length > 0)
                shipment_id = shipments[0];
            string sql = string.Format(@"SELECT (SELECT TOP 1 PLANT FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS PLANT1,* FROM SMIRV 
                    WHERE RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('O','V')",
                     SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count == 0)
            {
                ri.IsSucceed = false;
                ri.Description = obj.ContainerNo+"：This ContainerNo cann't find,So you cann't Upload POD!";
                return null;
            }
            DataRow smRow = dt.Rows[0];
            byte[] imageBytes = Convert.FromBase64String(obj.OutputImage);
            string fileName = string.Format("{0}_{1}", obj.ContainerNo, DateTime.Now.ToString("HHmmssfff"));
            string ext = Prolink.Math.GetValueAsString(obj.OutputFormat).Replace(".", "");
            if (string.IsNullOrEmpty(ext) || ext.Contains("image") || ext.Contains("jpeg"))
            {
                ext = "jpg";
            }
            string filePath = CreateBaseDirectoryFileName(new List<string> { "Export", "PodFile" }, fileName, ext);
            CreateDir(filePath);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }

            if (string.IsNullOrEmpty(filePath)) return null;
            string userid = Prolink.Math.GetValueAsString(smRow["CREATE_BY"]);
            string companyid = Prolink.Math.GetValueAsString(smRow["CMP"]);
            string plant1 = Prolink.Math.GetValueAsString(smRow["PLANT1"]);
            EDocInfo info = Helper.CreateShipmentEDocInfo(smRow);

            info.UserId = userid;
            info.DocType = "POD";
            info.FilePath = filePath;
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    //context.Result.IsSucceed = false;
                    //context.Result.ResultCode = uploadResult.Status.ToString();
                    //context.Result.Description = "运单申请成功，但上传运单文档失败！";
                    ri.IsSucceed = false;
                    ri.Description = "Upload Temp Pod Image failed!";
                    //return null;
                }

                Business.TPV.Standard.ChageSmRVStatus cs = new Business.TPV.Standard.ChageSmRVStatus();
                cs.UpdateSMRV("TSA", new string[] { smRow["U_ID"].ToString() }, companyid);
                //POD上传完毕，要执行Rebuild Order：
                string OrdNo = Prolink.Math.GetValueAsString(smRow["ORD_NO"]);
                if (!string.IsNullOrEmpty(plant1))
                {
                    TrackingEDI.InboundBusiness.IBUserInfo userinfo = new TrackingEDI.InboundBusiness.IBUserInfo
                    {
                        UserId = userid,
                        CompanyId = companyid,
                        GroupId = "TPV"
                    };
                    MixedList mixList = new MixedList();
                    Dictionary<string, object> json = new Dictionary<string, object>();

                    string sqlsmwh = string.Format("SELECT * FROM SMWH WHERE WS_CD={0}", SQLUtils.QuotedStr(plant1));
                    DataTable smwhdt = DB.GetDataTable(sqlsmwh, new string[] { });
                    string DestCd = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA"]);
                    string DestName = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_AREA_NM"]);
                    string AddrCode = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR"]);
                    string Addr = Prolink.Math.GetValueAsString(smwhdt.Rows[0]["DLV_ADDR_NM"]);

                    string sql1 = string.Format("SELECT TOP 1 CUSTOMER_CODE FROM BSADDR WHERE ADDR_CODE={0}", SQLUtils.QuotedStr(AddrCode));
                    string CustermCode1 = DB.GetValueAsString(sql1);
                    sql1 = string.Format("SELECT TOP 1 PARTY_NAME FROM SMPTY WHERE PARTY_NO={0}", SQLUtils.QuotedStr(CustermCode1));
                    string CustermName1 = DB.GetValueAsString(sql1);

                    sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                    string ArrivalDate = DB.GetValueAsString(sql);
                    json.Add("OrdNo", OrdNo);
                    json.Add("DestCd", DestCd);
                    json.Add("DestName", DestName);
                    json.Add("AddrCode", AddrCode);
                    json.Add("Addr", Addr);
                    json.Add("CustermCode1", CustermCode1);
                    json.Add("CustermName1", CustermName1);
                    json.Add("ArrivalDate1", ArrivalDate);
                    List<string> returnMsg = new List<string>();
                    List<string> ordnos = new List<string>();
                    TrackingEDI.InboundBusiness.SMSMIHelper.ReBuildOrderIndex(json, mixList, returnMsg, ordnos, userinfo);
                    try
                    {
                        OperationUtils.ExecuteUpdate(mixList, Prolink.Web.WebContext.GetInstance().GetConnection());
                        //这里调用重新叫车的操作
                        if (ordnos.Count > 0)
                        {
                            OrdNo = ordnos[0];
                            sql = "SELECT * FROM SMORD WHERE ORD_NO=" + SQLUtils.QuotedStr(OrdNo);
                            DataTable orddt = DB.GetDataTable(sql, new string[] { });

                            sql = "SELECT * FROM SMRCNTR WHERE ORD_NO={0} AND CNTR_NO={1}";
                            sql = string.Format(sql, SQLUtils.QuotedStr(OrdNo), SQLUtils.QuotedStr(obj.ContainerNo));
                            DataTable cdt = DB.GetDataTable(sql, new string[] { });

                            sql = string.Format("SELECT TOP 1 PICKUP_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0} AND PICKUP_DATE IS NOT NULL ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                            string PickupDate = DB.GetValueAsString(sql);
                            string CallData = Prolink.Math.GetValueAsString(smRow["CALL_DATE"]);
                            if (string.IsNullOrEmpty(PickupDate))
                                PickupDate = CallData;
                            if (string.IsNullOrEmpty(PickupDate))
                            {
                                sql = string.Format("SELECT TOP 1 ARRIVAL_DATE FROM SMRCNTR WHERE SMRCNTR.ORD_NO={0}  AND ARRIVAL_DATE IS NOT NULL ORDER BY ARRIVAL_DATE DESC", SQLUtils.QuotedStr(OrdNo));
                                PickupDate = DB.GetValueAsString(sql);
                            }
                            List<string> EtaMsl = new List<string>();
                            //EtaMsl.Add(EtaMsl1);
                            //EtaMsl.Add(EtaMslTime1);

                            string WsCd = plant1;
                            string CntrNo = obj.ContainerNo;
                            List<string> idList = new List<string>();
                            List<string> newReserveNo = new List<string>();
                            TrackingEDI.InboundBusiness.SMSMIHelper.InboundFclOrderTrucker(orddt.Rows[0], cdt.Rows[0], PickupDate, EtaMsl, ArrivalDate, WsCd, userid, "", "", idList, newReserveNo);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "POD文档上传失败！";
                //context.Result.IsSucceed = false;
                //context.Result.ResultCode = "EDOCUploadFileErr";
                //context.Result.Description = string.Format("{0}{1}{2}", msg, Environment.NewLine, ex.Message);
                Logger.WriteLog(msg, ex);
            }
            return eiList;
        }
        protected string ImgToBase64String(string ImagePath)
        {
            try
            {
                //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(System.Web.HttpContext.Current.Server.MapPath(ImagePath));
                //MemoryStream ms = new MemoryStream();
                //bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //byte[] arr = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(arr, 0, (int)ms.Length);
                //ms.Close();
                var arr = File.ReadAllBytes(ImagePath);
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class TruckerUploadFinalImg : TruckManage<TruckerInfoF>
    {
        protected override EditInstructList ToEi(TruckerInfoF obj, out ResultInfo ri)
        {
            ri = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            ri.RefNo = obj.ContainerNo;
            EditInstructList eiList = new EditInstructList();
            string[] shipments = obj.ShipmentId.Split('|');
            string shipment_id = string.Empty;
            if (shipments.Length > 0)
                shipment_id = shipments[0];
            string sql = string.Format(@"SELECT (SELECT TOP 1 PLANT FROM SMORD WHERE SMORD.ORD_NO=SMIRV.ORD_NO) AS PLANT1,* FROM SMIRV 
                    WHERE RV_TYPE='I' AND SHIPMENT_ID={0} AND CNTR_NO={1} AND STATUS NOT IN ('V') ORDER BY PLANT1 ASC ",
                     SQLUtils.QuotedStr(shipment_id), SQLUtils.QuotedStr(obj.ContainerNo));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count == 0)
            {
                ri.IsSucceed = false;
                ri.Description = obj.ContainerNo+"：This ContainerNo cann't find,So you cann't Upload POD!";
                return null;
            }
            DataRow smRow = dt.Rows[0];
            byte[] imageBytes = Convert.FromBase64String(obj.OutputImage);
            string fileName = string.Format("{0}_{1}", obj.ContainerNo, DateTime.Now.ToString("HHmmssfff"));
            string ext = Prolink.Math.GetValueAsString(obj.OutputFormat).Replace(".", "");
            if (string.IsNullOrEmpty(ext) || ext.Contains("image") || ext.Contains("jpeg"))
            {
                ext = "jpg";
            }
            string filePath = CreateBaseDirectoryFileName(new List<string> { "Export", "PodFile" }, fileName, ext);
            CreateDir(filePath);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }

            if (string.IsNullOrEmpty(filePath)) return null;
            string userid = Prolink.Math.GetValueAsString(smRow["CREATE_BY"]);
            string companyid = Prolink.Math.GetValueAsString(smRow["CMP"]);
            string plant1 = Prolink.Math.GetValueAsString(smRow["PLANT1"]);
            EDocInfo info = Helper.CreateShipmentEDocInfo(smRow);

            info.UserId = userid;
            info.DocType = "POD";
            info.FilePath = filePath;
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    //context.Result.IsSucceed = false;
                    //context.Result.ResultCode = uploadResult.Status.ToString();
                    //context.Result.Description = "运单申请成功，但上传运单文档失败！";
                    ri.IsSucceed = false;
                    ri.Description = obj.ContainerNo+ "：Upload Destination Pod Image failed!";
                    //return null;
                }
                //POD上传完毕，要执行Rebuild Order：

                Business.TPV.Standard.ChageSmRVStatus cs = new Business.TPV.Standard.ChageSmRVStatus();
                cs.UpdateSMRV("TSA", new string[] { smRow["U_ID"].ToString() }, companyid);
            }
            catch (Exception ex)
            {
                string msg = "POD文档上传失败！";
                //context.Result.IsSucceed = false;
                //context.Result.ResultCode = "EDOCUploadFileErr";
                //context.Result.Description = string.Format("{0}{1}{2}", msg, Environment.NewLine, ex.Message);
                Logger.WriteLog(msg, ex);
            }
            return eiList;
        }
        protected string ImgToBase64String(string ImagePath)
        {
            try
            {
                //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(System.Web.HttpContext.Current.Server.MapPath(ImagePath));
                //MemoryStream ms = new MemoryStream();
                //bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //byte[] arr = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(arr, 0, (int)ms.Length);
                //ms.Close();
                var arr = File.ReadAllBytes(ImagePath);
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class ChageSmRVStatus {
        public void UpdateSMRV(string istsa, string[] smsmiarray, string CompanyId)
        {
            MixedList ml = new MixedList();
            foreach (string smuid in smsmiarray)
            {
                if (string.IsNullOrEmpty(smuid))
                    continue;
                DateTime odt = DateTime.Now;
                DateTime ndt = TimeZoneHelper.GetTimeZoneDate(odt, CompanyId);
                string reserve_no_filter = string.Format("SELECT RESERVE_NO FROM SMIRV WHERE U_ID={0}", SQLUtils.QuotedStr(smuid));
                DataTable smrvDetail = GetSMRVDetail(reserve_no_filter);
                List<string> ordList = new List<string>();
                foreach (DataRow smrvD in smrvDetail.Rows)
                {
                    string ord_no = Prolink.Math.GetValueAsString(smrvD["ORD_NO"]);
                    string old_ord_no = Prolink.Math.GetValueAsString(smrvD["OLD_ORD_NO"]);
                    if (string.IsNullOrEmpty(ord_no) || ordList.Contains(ord_no))
                        continue;

                    if ("Temp".Equals(Prolink.Math.GetValueAsString(smrvD["FINAL_WH"])) && "Y".Equals(Prolink.Math.GetValueAsString(smrvD["OUTER_FLAG"])))
                    {
                        EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        if (!string.IsNullOrEmpty(old_ord_no))
                        {
                            smordei.PutKey("ORD_NO", old_ord_no);
                        }
                        else
                        {
                            smordei.PutKey("ORD_NO", ord_no);
                        }
                        smordei.Put("CSTATUS", "T");
                        ml.Add(smordei);
                    }
                    else if ("TSA".Equals(istsa))
                    {
                        EditInstruct smordei = new EditInstruct("SMORD", EditInstruct.UPDATE_OPERATION);
                        if (!string.IsNullOrEmpty(old_ord_no))
                        {
                            smordei.PutKey("ORD_NO", old_ord_no);
                        }
                        else
                        {
                            smordei.PutKey("ORD_NO", ord_no);
                        }
                        smordei.Put("CSTATUS", "O");
                        ml.Add(smordei);
                    }
                }

                string sql = string.Format(@"UPDATE SMRCNTR SET POD_MDATE={0}, POD_MDATE_L={1} WHERE RESERVE_NO IN (SELECT RESERVE_NO 
                        FROM SMIRV WHERE U_ID={2}) ", SQLUtils.QuotedStr(odt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(ndt.ToString("yyyy-MM-dd HH:mm:ss")), SQLUtils.QuotedStr(smuid));
                ml.Add(sql);
                EditInstruct ei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                ei.PutKey("U_ID", smuid);
                ei.Put("STATUS", "O");
                ml.Add(ei);

                if ("TSA".Equals(istsa))
                {
                    sql = string.Format(@"SELECT DISTINCT OLD_RESERVE_NO FROM SMRCNTR WHERE RESERVE_NO IN (SELECT RESERVE_NO 
                        FROM SMIRV WHERE U_ID={0}) ", SQLUtils.QuotedStr(smuid));
                    DataTable cntrdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    foreach (DataRow dr in cntrdt.Rows)
                    {
                        string oldrserno = Prolink.Math.GetValueAsString(dr["OLD_RESERVE_NO"]);
                        if (string.IsNullOrEmpty(oldrserno))
                            continue;
                        EditInstruct smrvei = new EditInstruct("SMIRV", EditInstruct.UPDATE_OPERATION);
                        smrvei.PutKey("RESERVE_NO", oldrserno);
                        smrvei.Put("STATUS", "O");
                        ml.Add(smrvei);

                    }
                }
            }

            try
            {
                OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch (Exception ex)
            {
                //return Json(new { msg = ex.ToString() });
            }
        }

        public DataTable GetSMRVDetail(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                filter = "'@@@@@@@@@@@@@@@'";
            string gateOutSQL = string.Format("SELECT SHIPMENT_ID,OLD_ORD_NO,ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD,SMRVSTATUS FROM (SELECT SHIPMENT_ID,OLD_ORD_NO,ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,(SELECT TOP 1 STATUS FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRCNTR.RESERVE_NO) AS SMRVSTATUS,CMP FROM SMRCNTR WITH (NOLOCK) WHERE SMRCNTR.RESERVE_NO IN ({0}))A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", filter);
            gateOutSQL += string.Format(" UNION SELECT SHIPMENT_ID,OLD_ORD_NO,ORD_NO,IDATE,CNTR_NO,DN_NO,ADDR_CODE,ADDR,OUTER_FLAG,FINAL_WH,RESERVE_NO,WS_CD,SMRVSTATUS FROM (SELECT SHIPMENT_ID,OLD_ORD_NO,ORD_NO,IDATE,'' AS CNTR_NO,DN_NO,ADDR_CODE,RESERVE_NO,WS_CD,(SELECT TOP 1 STATUS FROM SMIRV WHERE SMIRV.RESERVE_NO=SMRDN.RESERVE_NO) AS SMRVSTATUS,CMP FROM SMRDN WITH (NOLOCK) WHERE SMRDN.RESERVE_NO IN ({0}))A OUTER APPLY (SELECT TOP 1 ADDR,OUTER_FLAG,FINAL_WH FROM BSADDR WITH (NOLOCK) WHERE BSADDR.ADDR_CODE = A.ADDR_CODE AND BSADDR.CMP=A.CMP)B", filter);

            DataTable dt = OperationUtils.GetDataTable(gateOutSQL, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            return dt;
        }
    }
}

namespace Business.TPV.Standard
{
    public abstract class TruckManage<T> : Business.Import.ImportBase where T :Truck
    {
        public ResultInfo ImportXml(XmlDocument doc)
        {
            string fileName = BackupData(new List<string> { BackupDirName_Import, this.GetType().Name, "XmlDocument" }, doc);
            Logger.WriteLog("接收讯息", this.GetType().Name, "", "", fileName);

            T info = XmlUtil.Deserialize<T>(doc.InnerXml);
            //WriteEdiLog(sapId, dnno, info);
            return ImportInstanceList(new List<T>() { info });
        }

        public void WriteEdiLog(IEnumerable<T> infos,ResultInfo ri, string typename)
        {
            string str = ToJsonString(infos);
            string uid = System.Guid.NewGuid().ToString();
            Business.TPV.Utils.EdiInfo ediInfo = new Business.TPV.Utils.EdiInfo();
            ediInfo.ID = ri.RefNo;
            string ediid = typename;
            switch (typename)
            {
                case "TransloadingResponse":
                    ediid = "Transloading Response";
                    break;
                case "TruckerManagerF":
                    ediid = "PODImage Response";
                    break;
                case "DeliveryTruckResponse":
                    ediid = "Delivery Truck Response";
                    break;
                case "DeliveryWareHouseResponse":
                    ediid = "Delivery WareHouseResponse";
                    break;
                case "TruckerUploadFinalImg":
                    ediid = "PODImage Upload Final Response";
                    break;
                case "TruckerUploadTempImg":
                    ediid = "PODImage Upload Temp Response";
                    break;
            }

            ediInfo.EdiId = ediid;
            ediInfo.Remark = ri.Description;
            ediInfo.CreateBy = "EDI";
            ediInfo.Rs = "Receive";
            ediInfo.Status = "Succeed";
            ediInfo.FromCd = "UNIS";
            ediInfo.ToCd = "eShipping";
            ediInfo.DataFolder = uid; ;
            ediInfo.RefNO = ri.RefNo;
            ediInfo.GroupId = Context.GroupId;
            ediInfo.Cmp = "TS";
            ediInfo.Stn = "";

            MixedList ml = new MixedList();
            EditInstruct ei = new EditInstruct("UNIS_EDI_LOG", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", uid);
            ei.Put("EDI_ID", ediInfo.EdiId);
            ei.PutExpress("EVENT_DATE", "getdate()");
            if (!string.IsNullOrEmpty(ediInfo.Remark) && ediInfo.Remark.Length > 500)
                ediInfo.Remark = ediInfo.Remark.Substring(0, 500);
            ei.Put("REMARK", ediInfo.Remark);
            ei.Put("SENDER", ediInfo.CreateBy);
            ei.Put("RS", ediInfo.Rs);
            ei.Put("STATUS", ediInfo.Status);
            ei.Put("FROM_CD", ediInfo.FromCd);
            ei.Put("TO_CD", ediInfo.ToCd);
            ei.Put("DATA_FOLDER", ediInfo.DataFolder);
            ei.Put("REF_NO", ediInfo.RefNO);
            ei.Put("GROUP_ID", ediInfo.GroupId);
            ei.Put("CMP", ediInfo.Cmp);
            ei.Put("STN", ediInfo.Stn);
            ml.Add(ei);
            if (!string.IsNullOrEmpty(str))
            {
                EditInstruct eidata = new EditInstruct("EDI_DATA", EditInstruct.INSERT_OPERATION);
                eidata.Put("U_ID", uid);
                eidata.Put("EDI_DATE", str);
                eidata.PutExpress("CREATE_DATE", "getdate()");
                ml.Add(eidata);
            }
            try
            {
                DB.ExecuteUpdate(ml);
            }
            catch
            {
            }
        }
        

        public virtual ResultInfo ImportInstanceList(IEnumerable<T> infos)
        {
            ResultInfo nullErrorResult = new ResultInfo { ResultCode = ResultCode.ValidateException, Description = "资料不可为空！" };
            if (infos == null) return nullErrorResult;
            List<T> items = infos.ToList();
            if (items == null || items.Count <= 0) return nullErrorResult;
            ResultInfo result = null;
            try
            {
                result = HandlerIns(items);
                //try
                //{
                //    WriteEdiLog(infos, result, this.GetType().Name);
                //}
                //catch (Exception ex) { }
                if (!result.IsSucceed)
                {
                    return result;
                }
                
            }
            catch (Exception e)
            {
                return new ResultInfo { IsSucceed = false, ResultCode = "fail", Description = e.Message };
            }
            return result;
        }

         ResultInfo HandlerIns(List<T> infos)
        {
            EntityValidationResult result = null;
            if (!Check<T>(infos, ref result))
            {
                var v = new ResultInfo()
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = string.Join(Environment.NewLine, result.Errors.Select(item => item.ErrorMessage))
                };
                var l = Logger.CreateLog("规格验证不通过", this.GetType().Name, "", "", v.Description);
                Logger.WriteLog(l);
                return v;
            }
            return HandlerAdd(infos);
        }
        protected virtual ResultInfo HandlerAdd(List<T> infos)
        {
            EditInstructList eiList = new EditInstructList();
            ResultInfo ri = new ResultInfo();
            foreach (var item in infos)
            {
                eiList.MergeEditInstructList(ToEi(item,out ri));
                try
                {
                    WriteEdiLog(infos, ri, this.GetType().Name);
                }
                catch (Exception ex) { }
                if (ri.IsSucceed==false)
                {
                    return ri;
                }
            }
            return Execute(eiList);
        }

        protected virtual EditInstructList ToEi(T obj,out ResultInfo rf)
        {
            rf = ResultInfo.SucceedResult();
            if (obj == null)
                return null;
            EditInstructList eiList = new EditInstructList();
            return eiList;
        }
    }
}
