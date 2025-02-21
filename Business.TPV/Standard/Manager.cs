using Business.Service;
using Business.TPV.Utils;
using Prolink.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.TPV.Standard
{
    class Manager : Export.ShipmentManager
    {
        public ResultInfo CreateExpressBooking(Runtime runtime, out ExpressBooking template, out EDIConfig config, out DataRow smRow)
        {
            template = null;
            Runtime = runtime;
            smRow = QuerySM(runtime);
            config = Context.GetEDIConfig(runtime.PartyNo, runtime.Location);
            if (config == null) return NoneConfigResult(string.Format("EDI:{0}", runtime.PartyNo));
            template = CreateExpressBooking(runtime, smRow, config);
            return SucceedResult();
        }
        public ResultInfo CreateOceanBooking(Runtime runtime, out OceanBooking template, out EDIConfig config, out DataRow smRow)
        {
            template = null;
            Runtime = runtime;
            smRow = QuerySM(runtime);
            config = Context.GetEDIConfig(runtime.PartyNo, runtime.Location);
            if (config == null) return NoneConfigResult(string.Format("EDI:{0}", runtime.PartyNo));
            template = CreateOceanBooking(runtime, smRow, config);
            return SucceedResult();
        }
        public ResultInfo CreateOceanDeclaration(Runtime runtime, out OceanDeclaration template, out EDIConfig config, out DataRow smRow)
        {
            template = null;
            Runtime = runtime;
            smRow = QuerySM(runtime);
            config = Context.GetEDIConfig(runtime.PartyNo, runtime.Location);
            if (config == null) return NoneConfigResult(string.Format("EDI:{0}", runtime.PartyNo));
            template = CreateOceanDeclaration(runtime, smRow, config);
            return SucceedResult();
        }

        OceanBooking CreateOceanBooking(Runtime runtime, DataRow smRow, EDIConfig config)
        {
            OceanBooking template = new OceanBooking();
            FillOceanInfo(template, runtime, smRow, config);
            return template;
        }

        void FillOceanInfo(OceanBooking template, Runtime runtime, DataRow smRow, EDIConfig config)
        {
            template.Goods = EscapeSpecialString(smRow["GOODS"]);
            template.GoodsCN = EscapeSpecialString(smRow["LGOODS"]);
            template.POR = EscapeSpecialString(smRow["PPOR_CD"]);
            template.PORName = EscapeSpecialString(smRow["PPOR_NAME"]);
            template.POL = EscapeSpecialString(smRow["PPOL_CD"]);
            template.POL_Name = EscapeSpecialString(smRow["PPOL_NAME"]);
            template.POD = EscapeSpecialString(smRow["PPOD_CD"]);
            template.POD_Name = EscapeSpecialString(smRow["PPOD_NAME"]);
            template.DEST = EscapeSpecialString(smRow["PDEST_CD"]);
            template.DEST_Name = EscapeSpecialString(smRow["PDEST_NAME"]);
            template.Contract = EscapeSpecialString(smRow["SVC_CONTACT"]);
            template.GP20 = EscapeSpecialString(smRow["PCNT20"]);
            template.GP40 = EscapeSpecialString(smRow["PCNT40"]);
            template.HQ40 = EscapeSpecialString(smRow["PCNT40HQ"]);
            template.OtherCNTType = EscapeSpecialString(smRow["PCNT_TYPE"]);
            template.OtherCNTCount = EscapeSpecialString(smRow["PCNT_NUMBER"]);
            template.ServiceMode = string.Format("{0}-{1}", EscapeSpecialString(smRow["LOADING_FROM"]),
                    EscapeSpecialString(smRow["LOADING_TO"]));
            object edt = smRow["DN_ETD"];
            if (edt != null && edt != DBNull.Value)
            {
                template.ETD = ((DateTime)edt);
            }
            DataTable partyDT = QueryPartyDT(runtime.ShipmentID, new List<string> { PartyCode_Track });
            template.Carrier = new PartyInfo
            {
                Code = EscapeSpecialString(smRow["CARRIER"]),
                Name = EscapeSpecialString(smRow["CARRIER_NM"]),
                PartyDisplay = "Carrier"
            };
            template.Trucker = GetPartyInfo(PartyCode_Track, partyDT);
            if (template.Trucker != null)
                template.Trucker.PartyDisplay = "Trucker";
            DataTable bkPartyDT = null;
            FillBookingTemplate(template, runtime, smRow, config, out bkPartyDT);
        }

        OceanDeclaration CreateOceanDeclaration(Runtime runtime, DataRow smRow, EDIConfig config)
        {
            OceanDeclaration template = new OceanDeclaration();
            DataTable partyDT = QueryPartyDT(runtime.ShipmentID, new List<string> { PartyCode_BookingAgent_BO, PartyCode_BookingAgent_SP });
            template.BookingAgent = GetPartyInfo(PartyCode_BookingAgent_BO, partyDT);
            if (template.BookingAgent == null)
                template.BookingAgent = GetPartyInfo(PartyCode_BookingAgent_SP, partyDT);
            if (template.BookingAgent != null)
                template.BookingAgent.PartyDisplay = "Booking Agent";
            FillOceanInfo(template, runtime, smRow, config);
            template.Containers = QueryContainers(runtime).ToArray();
            template.POR = EscapeSpecialString(smRow["POR_CD"]);
            template.PORName = EscapeSpecialString(smRow["POR_NAME"]);
            template.POL = EscapeSpecialString(smRow["POL_CD"]);
            template.POL_Name = EscapeSpecialString(smRow["POL_NAME"]);
            template.POD = EscapeSpecialString(smRow["POD_CD"]);
            template.POD_Name = EscapeSpecialString(smRow["POD_NAME"]);
            template.DEST = EscapeSpecialString(smRow["DEST_CD"]);
            template.DEST_Name = EscapeSpecialString(smRow["DEST_NAME"]);
            template.Vessel = EscapeSpecialString(smRow["VESSEL1"]);
            template.Voyage = EscapeSpecialString(smRow["VOYAGE1"]);
            template.MasterNO = EscapeSpecialString(smRow["MASTER_NO"]);
            template.HouseNO = EscapeSpecialString(smRow["HOUSE_NO"]);
            template.EdiType = "D";
            template.MsgCode = "DECL";
            object edt = smRow["ETD"];
            if (edt != null && edt != DBNull.Value)
            {
                template.ETD = ((DateTime)edt);
            }
            if (string.IsNullOrEmpty(template.HouseNO))
                template.HouseNO = template.MasterNO;
            if (string.IsNullOrEmpty(template.MasterNO))
                template.MasterNO = template.HouseNO;
            FillDocInfo(template, runtime, smRow, config);

            return template;
        }

        void FillDocInfo(OceanDeclaration template, Runtime runtime, DataRow smRow, EDIConfig config)
        {
            EDocInfo docInfo = Helper.CreateShipmentEDocInfo(smRow);
            List<FileItemInfo> items = EDocHelper.GetFileItems(docInfo);
            if (items == null || items.Count <= 0)
                items = new List<FileItemInfo>();
            string sql = string.Format("SELECT U_ID,GROUP_ID,CMP,CREATE_BY FROM SMDN WHERE SHIPMENT_ID={0}", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dnDT = DB.GetDataTable(sql, new string[] { });
            if (dnDT != null && dnDT.Rows.Count > 0)
            {
                foreach (DataRow row in dnDT.Rows)
                {
                    var v = Helper.CreateShipmentEDocInfo(row);
                    var files = EDocHelper.GetFileItems(v);
                    if (files == null || files.Count <= 0) continue;
                    items.AddRange(files);
                }
            }
            if (items == null || items.Count <= 0) return;
            var invItem = items.Where(item => item.Item.EdocType == "INVO").ToList();
            if (invItem != null && invItem.Count > 0)
            {
                template.InvoiceFileUrl = string.Join(Environment.NewLine, invItem.Select(n => n.Url));
            }
            var packingItem = items.Where(item => item.Item.EdocType == "PACKO").ToList();
            if (packingItem != null && packingItem.Count > 0)
            {
                template.PackingFileUrl = string.Join(Environment.NewLine, packingItem.Select(n => n.Url));
            }
        }

        ExpressBooking CreateExpressBooking(Runtime runtime, DataRow smRow, EDIConfig config)
        {
            ExpressBooking template = new ExpressBooking()
            {
                Commodity = EscapeSpecialString(smRow["GOODS"]),
                PaymentAccountNumber = EscapeSpecialString(smRow["SCAC_CD"])
            };
            DataTable partyDT = null;
            FillBookingTemplate(template, runtime, smRow, config, out partyDT);
            var shiptoParty = GetPartyInfo(PartyCode_ShipTo, partyDT);
            if (shiptoParty != null)
            {
                shiptoParty.PartyDisplay = "Ship to";
                template.ShipTo = shiptoParty;
            }
            template.Option = EscapeSpecialString(smRow["OPTIONS"]);
            template.ServiceMode = EscapeSpecialString(smRow["SERVICE"]);
            object edt = smRow["DN_ETD"];
            if (edt != null && edt != DBNull.Value)
            {
                template.PickupDate = ((DateTime)edt).ToString(DateFormat);
            }

            return template;
        }


        public string DateFormat = "yyyyMMddHHmm";
        void FillBookingTemplate(Booking template, Runtime runtime, DataRow smRow, EDIConfig config, out DataTable partyDT)
        {
            template.ShipType = EscapeSpecialString(smRow["TRAN_TYPE"]);
            template.SenderCode = EscapeSpecialString(smRow["CMP"]);
            template.RecieveCode = config.PartyNO;
            template.EdiType = "B";
            template.MsgCode = "BOOKING";
            template.OPUser = GetOPUser(runtime);
            template.Mode = runtime.GetOperationModeCode();
            template.ShipmentID = runtime.ShipmentID;
            template.UniqueNumber = System.Guid.NewGuid().ToString().Replace("-", "");
            template.CBM = Prolink.Math.GetValueAsDouble(smRow["CBM"]);
            template.CTN = Prolink.Math.GetValueAsInt(smRow["PKG_NUM"]);
            template.GW = Prolink.Math.GetValueAsDouble(smRow["PGW"]);
            string tradeType = EscapeSpecialString(smRow["FRT_TERM"]);
            template.PaymentMode = tradeType == "P" ? PaymentModes.P : PaymentModes.C;
            template.PKGUnit = EscapeSpecialString(smRow["PKG_UNIT"]);
            template.PKGUnitDesc = EscapeSpecialString(smRow["PKG_UNIT_DESC"]);
            template.QTY = Prolink.Math.GetValueAsInt(smRow["QTY"]);
            template.GWUnit = EscapeSpecialString(smRow["GWU"]);
            template.DeliveryTerm = EscapeSpecialString(smRow["INCOTERM_CD"]);
            template.DeliveryTermDesc = EscapeSpecialString(smRow["INCOTERM_DESCP"]);
            template.DNInfos = QueryDNInfos(runtime).ToArray();
            Tuple<double, string> dutiableInfo = CreateDutiableInfo(runtime, smRow);
            if (dutiableInfo != null)
            {
                template.DutiableValue = System.Math.Round(dutiableInfo.Item1, 2);
                template.Currency = dutiableInfo.Item2;
            }
            partyDT = QueryPartyDT(runtime.ShipmentID, new List<string> { PartyCode_Consignee, PartyCode_ShipTo, PartyCode_Shipper, PartyCode_Notify1, PartyCode_Notify2, PartyCode_Notify3 });
            template.Consignee = GetPartyInfo(PartyCode_Consignee, partyDT);
            if (template.Consignee != null)
                template.Consignee.PartyDisplay = "Consignee";
            template.Shipper = GetPartyInfo(PartyCode_Shipper, partyDT);
            if (template.Shipper != null)
                template.Shipper.PartyDisplay = "Shipper";
            template.Notify = GetPartyInfo(PartyCode_Notify1, partyDT);
            if (template.Notify != null)
                template.Notify.PartyDisplay = "Notify1";
            template.Notify2 = GetPartyInfo(PartyCode_Notify2, partyDT);
            if (template.Notify2 != null)
                template.Notify2.PartyDisplay = "Notify2";
            template.Notify3 = GetPartyInfo(PartyCode_Notify3, partyDT);
            if (template.Notify3 != null)
                template.Notify3.PartyDisplay = "Notify3";
            template.Remark = EscapeSpecialString(smRow["INSTRUCTION"]);
            template.ShippingMark = EscapeSpecialString(smRow["MARKS"]);
            template.Commoditys = QueryCommodityInfos(runtime).ToArray();
        }

        IEnumerable<Container> QueryContainers(Runtime runtime)
        {
            string sql = string.Format("SELECT CNTY_TYPE,SEAL_NO1,CNTR_NO,CBM,GW,GWU,NW,TARE_WEIGHT,QTY,QTYU FROM SMRV WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})",
                SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                yield return new Container
                {
                    ContainerNO = EscapeSpecialString(row["CNTR_NO"]),
                    ContainerType = EscapeSpecialString(row["CNTY_TYPE"]),
                    SealsNO = EscapeSpecialString(row["SEAL_NO1"]),
                    GW = EscapeSpecialString(row["GW"]),
                    GWUnit = EscapeSpecialString(row["GWU"]),
                    NW = EscapeSpecialString(row["NW"]),
                    QTY = EscapeSpecialString(row["QTY"]),
                    QTYUnit = EscapeSpecialString(row["QTYU"]),
                    TareGW = EscapeSpecialString(row["TARE_WEIGHT"]),
                    CBM = EscapeSpecialString(row["CBM"])
                };
            }
        }
        IEnumerable<DNInfo> QueryDNInfos(Runtime runtime)
        {
            string ponoColumn = "(SELECT TOP 1 PO_NO FROM SMDNP P WHERE P.U_FID=SMDN.U_ID) AS PO_NO";
            string sql = string.Format("SELECT DN_NO,ORIGIN_NO,UNICODE,EXPORT_NO,{0} FROM SMDN WHERE SHIPMENT_ID={1}", ponoColumn,
                SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                string dnNo = EscapeSpecialString(row["ORIGIN_NO"]);
                if (string.IsNullOrEmpty(dnNo))
                    dnNo = EscapeSpecialString(row["DN_NO"]);
                yield return new DNInfo
                {
                    UniCode = EscapeSpecialString(row["UNICODE"]),
                    DNNO = dnNo,
                    ExportNO = EscapeSpecialString(row["EXPORT_NO"]),
                    PONO = EscapeSpecialString(row["PO_NO"])
                };
            }
        }
        string GetOPUser(Runtime runtime)
        {
            if (string.IsNullOrEmpty(runtime.OPUser)) return null;
            string sql = string.Format("SELECT * FROM SYS_ACCT WHERE U_ID={0}", SQLUtils.QuotedStr(runtime.OPUser));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return runtime.OPUser;
            return string.Format("{0}/{1}", runtime.OPUser, Prolink.Math.GetValueAsString(dt.Rows[0]["U_NAME"]));
        }
        Tuple<double, string> CreateDutiableInfo(Runtime runtime, DataRow smRow)
        {
            string sql = string.Format("SELECT VALUE1,CUR FROM SMDNP WHERE U_FID IN(SELECT U_ID FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            double amount = 0;
            string currency = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                amount += Prolink.Math.GetValueAsDouble(row["VALUE1"]);
                string cur = Prolink.Math.GetValueAsString(row["CUR"]);
                if (!string.IsNullOrEmpty(cur))
                    currency = cur;
            }
            if (string.IsNullOrEmpty(currency))
                currency = Prolink.Math.GetValueAsString(smRow["CUR"]);
            return new Tuple<double, string>(amount, currency);
        }
        IEnumerable<Commodity> QueryCommodityInfos(Runtime runtime)
        {
            string sql = string.Format("SELECT OHS_CODE,GOODS_DESCP,PROD_DESCP,QTY,QTYU,CBM,GW FROM SMDNP WHERE DN_NO IN(SELECT DN_NO FROM SMDN WHERE SHIPMENT_ID={0})", SQLUtils.QuotedStr(runtime.ShipmentID));
            DataTable dt = DB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) yield break;
            foreach (DataRow row in dt.Rows)
            {
                yield return new Commodity
                {
                    HTSCode = EscapeSpecialString(row["OHS_CODE"]),
                    ProductName = EscapeSpecialString(row["GOODS_DESCP"]),
                    ProductNameCN = EscapeSpecialString(row["PROD_DESCP"]),
                    QTY = EscapeSpecialString(row["QTY"]),
                    QTYUnit = EscapeSpecialString(row["QTYU"]),
                    CBM = EscapeSpecialString(row["CBM"]),
                    GW = EscapeSpecialString(row["GW"])
                };
            }
        }
        PartyInfo GetPartyInfo(string partyCode, DataTable partyDT)
        {
            DataRow[] rows = partyDT.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(partyCode)));
            if (rows == null || rows.Length <= 0) return null;
            DataRow row = rows[0];
            string cmpName = EscapeSpecialString(row["PARTY_NAME"]);
            PartyInfo info = new PartyInfo
            {
                ID =EscapeSpecialString(row["PARTY_NO"]),
                Name = cmpName,
                CityName = EscapeSpecialString(row["CITY_NM"]),
                CountryCode = EscapeSpecialString(row["CNTY"]),
                CountryName = EscapeSpecialString(row["CNTY_NM"]),
                PostalCode = EscapeSpecialString(row["ZIP"]),
                Tel = EscapeSpecialString(row["PARTY_TEL"]),
                Contact = EscapeSpecialString(row["PARTY_ATTN"]),
                Email = EscapeSpecialString(row["PARTY_MAIL"])
            };
            List<string> addressList = new List<string>();
            Action<string> addAddress = address =>
            {
                if (string.IsNullOrEmpty(address)) return;
                if (addressList.Contains(address)) return;
                addressList.Add(address);
            };
            addAddress(EscapeSpecialString(row["PART_ADDR1"]));
            addAddress(EscapeSpecialString(row["PART_ADDR2"]));
            addAddress(EscapeSpecialString(row["PART_ADDR3"]));
            foreach (var s in addressList)
            {
                if (string.IsNullOrEmpty(info.Address))
                {
                    info.Address = s;
                    continue;
                }
                if (string.IsNullOrEmpty(info.Address2))
                {
                    info.Address2 = s;
                    continue;
                }
                if (string.IsNullOrEmpty(info.Address3))
                {
                    info.Address3 = s;
                    continue;
                }
            }
            return info;
        }

        protected override RequstModes Mode
        {
            get { return RequstModes.None; }
        }

        public string EscapeSpecialString(object value)
        {
            string val = Prolink.Math.GetValueAsString(value);
            Regex Special = new Regex("[&|<|>|'|\"]");
            var mc = Special.Matches(val);
            List<string> have = new List<string>();

            for (int i = 0; i < mc.Count; i++)
            {
                var n = mc[i];
                string name = n.Value;
                if (!have.Contains(name))
                {
                    have.Add(name);
                    val = val.Replace(name, SpecialString(name));
                }
            }
            return val;
        }
        public string SpecialString(string value)
        {
            //&lt; < 小于号 
            //&gt; > 大于号 
            //&amp; & 和 
            //&apos; ' 单引号 
            //&quot; " 双引号
            switch (value)
            {
                case "&": return "&amp;";
                case "<": return "&lt;";
                case ">": return "&gt;";
                case "'": return "&apos;";
                case "\"": return "&quot;";

            }
            return string.Empty;
        }
    }
}
