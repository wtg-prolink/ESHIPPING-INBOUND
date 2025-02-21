using Business.EDI;
using Business.Service;
using Business.TPV.Base;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using TrackingEDI.Business;

namespace Business.TPV.DHL
{
    public class Manager : Export.ShipmentManager
    {
        public ResultInfo SendBookingInfo(Runtime runtime)
        {
            Runtime = runtime;
            if (runtime.OperationMode == OperationModes.Cancel) return null;
            DataRow smRow = QuerySM(runtime);
            EDIConfig config = Context.GetEDIConfig(runtime.PartyNo, runtime.Location);
            ShipmentTemplate template = CreateBookingTemplate(runtime, smRow, config);
            var result = SendShipment(template, runtime, smRow, config);
            WriteEDILog(new DHLBookingEDILog(runtime, smRow), result);
            return result;
        }

        ShipmentTemplate CreateBookingTemplate(Runtime runtime, DataRow smRow, EDIConfig config)
        {
            ShipmentTemplate template = new ShipmentTemplate();
            template.UniqueNumber = System.Guid.NewGuid().ToString().Replace("-", "");
            DataTable partyDT = QueryPartyDT(runtime.ShipmentID, new List<string> { PartyCode_ShipTo, PartyCode_Shipper });
            template.Certification = GetRequstCertificationInfo(config);
            template.BillingInfo = new BillingInfo
            {
                ShipperAccountNumber = Prolink.Math.GetValueAsString(smRow["SCAC_CD"]),
                PaymentType = PaymentTypes.Shipper,
                DutyAccountNumber = Prolink.Math.GetValueAsString(smRow["THIRD_PAYMENT"])
            };
            string incoterm = Prolink.Math.GetValueAsString(smRow["INCOTERM_CD"]);
            if (incoterm == "DDP")
            {
                template.BillingInfo.DutyPaymentType = PaymentTypes.Shipper;
                //template.BillingInfo.DutyAccountNumber = template.BillingInfo.ShipperAccountNumber;
            }
            
            template.Consignee = GetPartyInfo(PartyCode_ShipTo, partyDT);
            if (template.Consignee != null)
                template.Consignee.PartyDisplay = template.Consignee.ContactInfo.PartyDisplay = "Ship to";
            template.Shipper = GetPartyInfo(PartyCode_Shipper, partyDT);
            if (template.Shipper != null)
                template.Shipper.PartyDisplay = template.Shipper.ContactInfo.PartyDisplay = "Shipper";
            template.DutiableInfo = CreateDutiableInfo(runtime, smRow);
            var items= QueryDNInfos(runtime).ToList();
            string refNo=string.Empty;
            if (items == null || items.Count <= 0)
                refNo = string.Format("Shipment ID:{0}", runtime.ShipmentID);
            else
            {
                if (items.Count > 2)
                    items = items.Take(2).ToList();
                refNo = string.Format("DN NO:{0}", string.Join(",", items));
            }
            template.ReferenceInfos.Add(new ReferenceInfo { ReferenceNO = refNo });
            //template.CommodityInfos = QueryCommodityInfos(runtime).ToList();
            template.ShipmentDetailsInfo = CreateShipmentDetailsInfo(smRow);
            template.LabelImageFormat = LabelImageFormats.PDF;
            template.IsUseDHLStandardWaybill = true;
            template.LabelTemplateInfo = new LabelTemplateInfo
            {
                LabelTemplate = LabelTemplates.PDF_8X4_A4
            };
            return template;
        }
        IEnumerable<string> QueryDNInfos(Runtime runtime)
        {
            string sql = string.Format("SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                yield return Prolink.Math.GetValueAsString(row["DN_NO"]);
            }
        }

        DutiableInfo CreateDutiableInfo(Runtime runtime, DataRow smRow)
        {
            double amount = Prolink.Math.GetValueAsDouble(smRow["GVALUE"]);
            string currency = Prolink.Math.GetValueAsString(smRow["CUR"]);
            if (amount <= 0)
            {
                string sql = string.Format("SELECT VALUE1,CUR FROM SMDNP WHERE U_FID IN(SELECT U_ID FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(runtime.ShipmentID));
                DataTable dt = DB.GetDataTable(sql, new string[] { });
                if (dt == null || dt.Rows.Count <= 0) return new DutiableInfo { Value = 1, Currency = "USD" };
                foreach (DataRow row in dt.Rows)
                {
                    amount += Prolink.Math.GetValueAsDouble(row["VALUE1"]);
                    string cur = Prolink.Math.GetValueAsString(row["CUR"]);
                    if (!string.IsNullOrEmpty(cur))
                        currency = cur;
                }
                if (string.IsNullOrEmpty(currency))
                    currency = Prolink.Math.GetValueAsString(smRow["CUR"]);
            }
            return new DutiableInfo { Value = amount, Currency = currency };
        }

        ShipmentDetailsInfo CreateShipmentDetailsInfo(DataRow smRow)
        {
            ShipmentDetailsInfo info = new ShipmentDetailsInfo();
            info.Weight = Prolink.Math.GetValueAsString(smRow["PGW"]);
            string wunit = Prolink.Math.GetValueAsString(smRow["GWU"]);
            info.WeightUnit = wunit == "KG" ? WeightUnitCodes.K : WeightUnitCodes.L;
            info.CurrencyCode = Prolink.Math.GetValueAsString(smRow["CUR"]);
            info.DimensionUnit = DimensionUnitModes.CM;
            info.Date = DateTime.Now;
            info.Contents = Prolink.Math.GetValueAsString(smRow["GOODS"]);
            info.GlobalProductMode = GlobalProductModes.Parcel;
            int qty = Prolink.Math.GetValueAsInt(smRow["PKG_NUM"]);
            info.IsDutiable = true;
            info.TotalQTY = qty;
            for (int i = 1; i <= qty; i++)
            {
                info.Pieces.Add(new PiecesInfo { PieceID = i.ToString() });
            }
            return info;
        }

        IEnumerable<CommodityInfo> QueryCommodityInfos(Runtime runtime)
        {
            string sql = string.Format("SELECT OHS_CODE,GOODS_DESCP FROM SMDNP WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                yield return new CommodityInfo
                {
                    Code = Prolink.Math.GetValueAsString(row["OHS_CODE"]),
                    Description = Prolink.Math.GetValueAsString(row["GOODS_DESCP"])
                };
            }
        }

        PartyInfo GetPartyInfo(string partyCode, DataTable partyDT)
        {
            DataRow[] rows = partyDT.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(partyCode)));
            if (rows == null || rows.Length <= 0) return null;
            DataRow row = rows[0];
            string attn = Prolink.Math.GetValueAsString(row["PARTY_ATTN"]);
            string cmpName = Prolink.Math.GetValueAsString(row["PARTY_NAME"]);
            if (!string.IsNullOrEmpty(cmpName))
            {
                if (cmpName.Length > 35)
                {
                    string c = cmpName.Substring(35);
                    attn = string.Join(" ", c, attn);
                    cmpName = cmpName.Substring(0, 35);
                }
            }
            PartyInfo info = new PartyInfo
            {
                CompanyName = cmpName,
                City = Prolink.Math.GetValueAsString(row["CITY_NM"]),
                CountryCode = Prolink.Math.GetValueAsString(row["CNTY"]),
                CountryName = Prolink.Math.GetValueAsString(row["CNTY_NM"]),
                DivisionCode = Prolink.Math.GetValueAsString(row["STATE"]),
                PostalCode = Prolink.Math.GetValueAsString(row["ZIP"]),
                ContactInfo = new ContactInfo
                {
                    Email = Prolink.Math.GetValueAsString(row["PARTY_MAIL"]),
                    PhoneNumber = Prolink.Math.GetValueAsString(row["PARTY_TEL"]),
                    Name = string.IsNullOrEmpty(attn) ? cmpName : attn
                }
            };
            Action<string> addAddress = address =>
                {
                    if (string.IsNullOrEmpty(address)) return;
                    if (info.Address == null)
                        info.Address = new List<string>();
                    if (info.Address.Contains(address)) return;
                    info.Address.Add(address);
                };
            addAddress(Prolink.Math.GetValueAsString(row["PART_ADDR1"]));
            addAddress(Prolink.Math.GetValueAsString(row["PART_ADDR2"]));
            addAddress(Prolink.Math.GetValueAsString(row["PART_ADDR3"]));
            return info;
        }

        class ParseContext
        {
            public XmlDocument Doc { get; set; }
            public ResultInfo Result { get; set; }
            public string FilePath { get; set; }
            public Runtime Runtime { get; set; }
            public string BLNO { get; set; }
            public string DEST { get; set; }
            public string Original { get; set; }
        }

        void ParseShipmentResponse(ParseContext context)
        {
            ResultInfo result = context.Result;
            XmlNodeList list = context.Doc.GetElementsByTagName("Note");
            if (list == null || list.Count <= 0)
                return;
            for (int i = 0; i < list.Count; i++)
            {
                XmlNode stausNode = list[i];
                foreach (XmlNode node in stausNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "ActionNote":
                            if (node.InnerText == "Success")
                            {
                                result.IsSucceed = true;
                                result.ResultCode = ResultCode.Succeed;
                            }
                            break;
                    }
                }
            }
            if (result.IsSucceed)
            {
                ParseShipmentResponseSuccessBLNO(context);
                ParseShipmentResponseSuccessLabel(context);
                ParseShipmentResponseSuccessDEST(context);
                ParseShipmentResponseSuccessOrigin(context);
            }
        }
        void ParseShipmentResponseSuccessDEST(ParseContext context)
        {
            context.DEST = ParseShipmentResponseSuccessServiceAreaCode(context, "DestinationServiceArea");
        }
        void ParseShipmentResponseSuccessOrigin(ParseContext context)
        {
            context.Original = ParseShipmentResponseSuccessServiceAreaCode(context, "OriginServiceArea");
        }
        string ParseShipmentResponseSuccessServiceAreaCode(ParseContext context, string nodeName)
        {
            try
            {
                ResultInfo result = context.Result;
                XmlNodeList list = context.Doc.GetElementsByTagName(nodeName);
                if (list == null || list.Count <= 0)
                    return null;
                foreach (XmlNode node in list[0])
                {
                    switch (node.Name)
                    {
                        case "ServiceAreaCode": return node.InnerText;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("解析DHL反馈信息出错！", ex);
                return null;
            }
        }
        void ParseShipmentResponseSuccessBLNO(ParseContext context)
        {
            ResultInfo result = context.Result;
            XmlNodeList list = context.Doc.GetElementsByTagName("AirwayBillNumber");
            if (list == null || list.Count <= 0)
                return;
            context.BLNO = list[0].InnerText;
        }
        void ParseShipmentResponseSuccessLabel(ParseContext context)
        {
            ParseShipmentResponseSuccessBLNO(context);
            ResultInfo result = context.Result;
            XmlNodeList lableNode = context.Doc.DocumentElement.SelectNodes("LabelImage/OutputImage");
            result.IsSucceed = false;
            result.ResultCode = ResultCode.ValidateException;
            if (lableNode == null || lableNode.Count <= 0)
            {
                result.Description = "LableImage node can't not find!";
                return;
            }
            string lableStr = lableNode[0].InnerText;
            if (string.IsNullOrEmpty(lableStr))
            {
                result.Description = "LableImage node value is null!";
                return;
            }
            byte[] imageBytes = Convert.FromBase64String(lableStr);
            string fileName = string.Format("{0}_{1}", context.Runtime.ShipmentID, DateTime.Now.ToString("HHmmssfff"));
            string filePath = CreateBaseDirectoryFileName(new List<string> { BackupDirNameRoot, Mode.ToString(), "LabelFile" }, fileName, "PDF");
            CreateDir(filePath);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }
            result.IsSucceed = true;
            result.ResultCode = ResultCode.Succeed;
            result.Description = ResultCode.GetDescription(ResultCode.Succeed);
            context.FilePath = filePath;
        }

        void ParseErrorResponse(ParseContext context)
        {
            XmlNodeList errList = context.Doc.GetElementsByTagName("Condition");
            if (errList == null || errList.Count <= 0) return;
            List<Tuple<string, string>> err = new List<Tuple<string, string>>();
            for (int i = 0; i < errList.Count; i++)
            {
                XmlNode conditionNode = errList[i];
                string code = string.Empty;
                string desc = string.Empty;
                foreach (XmlNode node in conditionNode.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "ConditionCode": code = node.InnerText; break;
                        case "ConditionData": desc = node.InnerText; break;
                    }
                }
                err.Add(new Tuple<string, string>(code, desc));
            }
            context.Result.ResultCode = "ErrorResponse";
            context.Result.Description = string.Join(Environment.NewLine, err.Select(eItem => string.Format("{0}:{1}", eItem.Item1, eItem.Item2)));
        }
        void ParseResult(ParseContext context)
        {
            XmlDocument doc = context.Doc;
            context.Result.Description = string.Format("Response result:{0}{1}", Environment.NewLine, doc.InnerXml);
            string rootName = doc.DocumentElement.LocalName;
            switch (rootName)
            {
                case "ShipmentResponse": ParseShipmentResponse(context); break;
                case "ErrorResponse":
                default: ParseErrorResponse(context); break;
            }
            if (context.Result != null)
            {
                context.Result.Description = string.Format("DHL返回提示:{0}", SQLUtils.QuotedStr(context.Result.Description));
            }
        }

        /// <summary>
        /// 发送物流运单信息
        /// </summary>
        /// <param name="template"></param>
        ResultInfo SendShipment(ShipmentTemplate template, Runtime runtime, DataRow smRow, EDIConfig config)
        {
            CheckTemplate(template);
            XmlDocument doc = template.ToXml();

            RequstResult r = OnRequst(doc, config);
            //XmlDocument resultDoc = new XmlDocument();
            //resultDoc.Load(@"C:\Users\buru\Desktop\TPV\DHL\DHL\测试结果\shipment\成功.xml");
            runtime.Data = r.RequstBackupFile;
            ResultInfo result = new ResultInfo { IsSucceed = false, ResultCode = ResultCode.UnKnow, Description = ResultCode.GetDescription(ResultCode.UnKnow) };
            if (doc == null) return result;
            ParseContext context = new ParseContext { Doc = r.ResultDoc, Result = result, Runtime = runtime };
            ParseResult(context);
            var v = UpSM(runtime, smRow, context);
            if (v != null && !v.IsSucceed)
            {
                if (context.Result != null && !string.IsNullOrEmpty(context.Result.Description))
                    return new ResultInfo { ResultCode = v.ResultCode, Description = string.Join(Environment.NewLine, context.Result.Description, v.Description) };
                else return v;
            }
            UploadLableFile(runtime, smRow, context);
            return context.Result;
        }

        void ToTacking(Runtime runtime, ParseContext context, List<string> shipmentIDList)
        {
            if (string.IsNullOrEmpty(context.BLNO)) return;
            if (shipmentIDList.Count > 0)
            {
                try
                {
                    BookingParser bp = new BookingParser();
                    bp.SaveToTrackingByShimentID(shipmentIDList);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog(string.Format("转入Tracking异常，Shipment ID：{0}", string.Join(",", shipmentIDList)), ex);
                }
            }
        }

        ConfirmBookingResultInfo UpSM(Runtime runtime, DataRow smRow, ParseContext context)
        {
            if (string.IsNullOrEmpty(context.BLNO)) return null;
            EditInstruct ei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("U_ID={0}", SQLUtils.QuotedStr(Prolink.Math.GetValueAsString(smRow["U_ID"])));
            ei.Put("HOUSE_NO", context.BLNO);
            string status = Prolink.Math.GetValueAsString(smRow["STATUS"]);
            if ("A".Equals(status) || "B".Equals(status) || string.IsNullOrEmpty(status))
            {
                ei.Put("STATUS", "B");
            }
            ei.Put("CORDER", "S");
            ei.PutDate("ETD", DateTime.Now);
            ei.PutDate("ETA", DateTime.Now.AddDays(2));   
            List<string> codes = new List<string>();
            if (!string.IsNullOrEmpty(context.DEST))
                codes.Add(context.DEST);
            if (!string.IsNullOrEmpty(context.Original))
                codes.Add(context.Original);
            DataTable portDT = QueryPortDT(codes);
            Func<string, DataRow> getPortRow = port =>
                {
                    if (string.IsNullOrEmpty(port)) return null;
                    if (portDT == null) return null;
                    DataRow[] rows = portDT.Select(string.Format("PORT_CD={0}", SQLUtils.QuotedStr(port)));
                    if (rows == null || rows.Length <= 0) return null;
                    return rows[0];
                };
            DataRow destRow = getPortRow(context.DEST);
            if (destRow != null)
            {
                ei.Put("DEST_CD", string.Format("{0}{1}", Prolink.Math.GetValueAsString(destRow["CNTRY_CD"]), Prolink.Math.GetValueAsString(destRow["PORT_CD"])));
                ei.Put("DEST_NAME", destRow["PORT_NM"]);
            }
            DataRow original = getPortRow(context.Original);
            if (original != null)
            {
                ei.Put("POL_CD", string.Format("{0}{1}", Prolink.Math.GetValueAsString(original["CNTRY_CD"]), Prolink.Math.GetValueAsString(original["PORT_CD"])));
                ei.Put("POL_NAME", original["PORT_NM"]);
            }
            DB.ExecuteUpdate(ei);
            return Helper.ConfirmBooking(runtime.ShipmentID, runtime.PartyNo);
        }

        DataTable QueryPortDT(List<string> dhlCodes)
        {
            if(dhlCodes==null||dhlCodes.Count<=0) return null;
            string condition = string.Format("PORT_CD IN({0})", string.Join(",", dhlCodes.Select(s => SQLUtils.QuotedStr(s))));
            string sql=string.Format("SELECT * FROM BSCITY WHERE {0}",condition);
            return DB.GetDataTable(sql, new string[] { });
        }

        void UploadLableFile(Runtime runtime, DataRow smRow, ParseContext context)
        {
            if (string.IsNullOrEmpty(context.FilePath)) return;
            EDocInfo info = Helper.CreateShipmentEDocInfo(smRow);
            string userid=Prolink.Math.GetValueAsString(smRow["BL_WIN"]);
            string []userids=userid.Split(' ');
            if(userids.Length>0){
                userid=userids[0].ToString();
            }else{
                userid=runtime.OPUser;
            }
            if (string.IsNullOrEmpty(userid))
                userid = runtime.OPUser;
            info.UserId = userid;
            info.DocType = "EXBL";
            info.FilePath = context.FilePath;
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    context.Result.IsSucceed = false;
                    context.Result.ResultCode = uploadResult.Status.ToString();
                    context.Result.Description = "运单申请成功，但上传运单文档失败！";
                }
                else
                {
                    //更新上传文件的文件者
                    string sql = string.Format("update Files set uploader={0} WHERE FileID={1}",
                        SQLUtils.QuotedStr(userid),
                        SQLUtils.QuotedStr(uploadResult.FileInfo.FileID));
                    DB.ExecuteUpdate(sql);
                }
            }
            catch (Exception ex)
            {
                string msg = "运单申请成功，但上传运单文档失败！";
                context.Result.IsSucceed = false;
                context.Result.ResultCode = "EDOCUploadFileErr";
                context.Result.Description = string.Format("{0}{1}{2}", msg, Environment.NewLine, ex.Message);
                Logger.WriteLog(msg, ex);
            }
        }

        /// <summary>
        /// 发送预约取件请求信息
        /// </summary>
        /// <param name="template"></param>
        public BookPickupResult SendBookPickup(BookPickupTemplate template, EDIConfig config)
        {
            CheckTemplate(template);
            XmlDocument doc = template.ToXml();
            RequstResult result = OnRequst(doc, config);
            return new BookPickupResult(result.ResultDoc);
        }
        /// <summary>
        /// 发送货况追踪请求信息
        /// </summary>
        /// <param name="template"></param>
        public void SendTrackcing(TrackingTemplate template, EDIConfig config)
        {
            CheckTemplate(template);
            XmlDocument doc = template.ToXml();
            OnRequst(doc,config);
        }

        DHLCertification _certificationInfo;
        DHLCertification GetRequstCertificationInfo(EDIConfig config)
        {
            if (_certificationInfo != null) return _certificationInfo;
            XmlDocument doc = Business.TPV.Context.GetSecurityDoc();
            var node = doc.SelectSingleNode("/root/DHL/Requst");
            if (node == null) throw new Exception("请求信息未配置！");
            //string user = node.Attributes["user"].Value;
            //string psw = node.Attributes["psw"].Value;
            //string url = node.Attributes["url"].Value;
            string accountNumber = node.Attributes["DefaultAccountNumber"].Value;
            //_certificationInfo = new DHLCertification(user, psw) { Url = url, DefaultAccountNumber = accountNumber };
            _certificationInfo = new DHLCertification(config.User, config.Psw) { Url = config.Server, DefaultAccountNumber = accountNumber };
            return _certificationInfo;
        }

        RequstResult OnRequst(XmlDocument doc, EDIConfig config)
        {
            return OnRequst(doc, GetRequstCertificationInfo(config).Url);
        }

        protected override RequstModes Mode
        {
            get { return RequstModes.DHL; }
        }
    }

    public class DHLCertification : CertificationInfo
    {
        public DHLCertification(string user, string psw)
            : base(user, psw)
        {

        }
        public string Url { get; set; }
        public string DefaultAccountNumber { get; set; }
    }

    public class BookPickupResult : Result
    {
        public BookPickupResult(XmlDocument doc)
            : base(doc)
        {
            Init();
        }
        void Init()
        {
            Status = DHL.Status.Error;
            InitHeader();
            InitError();
            InitSucceed();
        }

        void InitHeader()
        {
            XmlNodeList nodeList = Doc.GetElementsByTagName("ServiceHeader");
            if (nodeList == null || nodeList.Count <= 0) return;
            XmlNode mNode = nodeList[0];
            ResultHeader = new ResultHeader();
            foreach (XmlNode node in mNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "MessageTime":
                        string str = node.InnerText;
                        if (string.IsNullOrEmpty(str)) continue;
                        DateTime dt;
                        if (DateTime.TryParse(str, out dt))
                            ResultHeader.MessageTime = dt;
                        break;
                    case "MessageReference": ResultHeader.RefNO = node.InnerText; break;
                    case "SiteID": ResultHeader.ID = node.InnerText; break;
                    case "Password": ResultHeader.Password = node.InnerText; break;
                }
            }
        }
        void InitError()
        {
            XmlNodeList nodeList = Doc.GetElementsByTagName("Status");
            foreach (XmlNode item in nodeList)
            {
                foreach (XmlNode node in item.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "ActionStatus":
                            switch (node.InnerText)
                            {
                                case "Error": Status = DHL.Status.Error; break;
                            }
                            break;
                        case "Condition":
                            ResultErrorCode code = new ResultErrorCode();
                            foreach (XmlNode nNode in node.ChildNodes)
                            {
                                switch (nNode.Name)
                                {
                                    case "ConditionCode": code.Code = nNode.InnerText; break;
                                    case "ConditionData": code.Message = nNode.InnerText; break;
                                }
                            }
                            if (ErrorResults == null)
                                ErrorResults = new List<ResultErrorCode>();
                            ErrorResults.Add(code);
                            break;
                    }
                }
            }
        }
        void InitSucceed()
        {
            XmlNodeList nodeList = Doc.GetElementsByTagName("ConfirmationNumber");
            if (nodeList == null || nodeList.Count <= 0) return;
            Status = DHL.Status.Succeed;
            ConfirmationNumber = nodeList[0].InnerText;
        }
        public Status Status { get; private set; }
        public List<ResultErrorCode> ErrorResults { get; private set; }
        public ResultHeader ResultHeader { get; private set; }

        /// <summary>
        /// 成功返回的号码
        /// </summary>
        public string ConfirmationNumber { get; private set; }
    }

    public class ResultHeader
    {
        public DateTime? MessageTime{get;set;}
        public string RefNO{get;set;}
        public string ID{get;set;}
        public string Password{get;set;}
    }

    public class ResultErrorCode
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
    public enum Status { Error, Succeed }

    public class Result
    {
        public Result(XmlDocument doc)
        {
            Doc = doc;
        }
        public XmlDocument Doc { get; private set; }
    }

}
