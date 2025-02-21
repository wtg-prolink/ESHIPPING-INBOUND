using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Prolink.DataOperation;
using Business;
using Models.EDI;
using TrackingEDI.Business;
using Business.TPV.DHL;
using System.Xml;
using Prolink.Data;
using System.IO;
using System.Web.Script.Serialization;
using Business.TPV.LSP;
using Business.TPV.Standard;
using System.ComponentModel;
using System.Text;

namespace WebGui
{
    public partial class test : System.Web.UI.Page
    {
        /// <summary>
        /// test.aspx?U_ID=&HOUSE_NO=H2&CD=A&DESP=Pickup&LOCATION=TWTPE&GROUP_ID=SD&CMP=SD&STN=DLC
        /// test.aspx?t=duser&user=&group=ADS&cmp=ADSCAN&stn=CAN
        /// test.aspx?t=auser&user=&group=ADS&cmp=ADSCAN&stn=CAN
        /// test.aspx?t=iport&SHIPMENT_ID=
        /// http://218.66.59.12:8035/EDITEST/Test.aspx?TraceCode=ATD&HouseNO=2E89233&MasterNO=2388342
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            //XmlDocument doc = new XmlDocument();
            //string filePath = @"C:\Users\Will.Wan\Desktop\UNIS\TruckerManager\XmlDocument\20190116\transloading.xml";
            //filePath = @"C:\Users\Will.Wan\Desktop\UNIS\TruckerManagerF\XmlDocument\20181101170605210.xml";
            //string xml = new System.IO.StreamReader(filePath).ReadToEnd();
            //doc.LoadXml(xml);
            //WebGui.Interface.WebService.TruckWebService s = new WebGui.Interface.WebService.TruckWebService();
            //s.Login("test100", "test100");
            //Business.Service.ResultInfo rsult = s.PostTranUSTSCinfoXml(doc.InnerXml);
            //Business.Service.ResultInfo rsult = s.PostPODImagebyXml(doc.InnerXml);
            //return;
            //Test.SAPTest.TestPostICAInfo();
            //Test.SAPTest.TestDNPosting();
            //string sql = "SELECT U_ID FROM SMSM WHERE CORDER='C' AND TRAN_TYPE='F' AND CMP='FQ' and  (iscombine_bl is null or iscombine_bl='S')";
            //DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            //string uid=string.Empty;
            //foreach (DataRow dr in dt.Rows)
            //{
            //    uid= Prolink.Math.GetValueAsString(dr["U_ID"]);
            //    try
            //    {
            //        Business.TPV.Financial.Bill bill = new Business.TPV.Financial.Bill();
            //        bill.Create(uid, DateTime.Now);
                    
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\john\Desktop\TPV\billtest.txt", true))
            //    {
            //        file.WriteLine(uid);// 直接追加文件末尾，换行   
            //    }
            //}


            //string url = @"C:\Users\john\Desktop\TPV\EDI对接部分汇总\外代\Declaration\test\20160810164912709.xml";
            //Test.LSPTest.TestDR(url);
            //return ;
            
            //Test.TNTTest.Test();
            //return;
            btn_import.ServerClick += btn_import_ServerClick;
            //var v = Business.TPV.Context.OrgService.GetEmploye("sq.He".ToUpper());
            //var x = Business.TPV.Context.OrgService.GetSuperior(v);
            //Test.TestEDI.Test();
           // string sql = string.Empty;
           // string type = Request["t"];
           // string u_id = Request["U_ID"];
           // string shipment_id = string.Empty;
           // if ("auser".Equals(type))
           // {
           //     AddAdmin(Request["user"], Request["group"], Request["cmp"], Request["stn"]);
           //     return;
           // }
           // else if ("duser".Equals(type))
           // {
           //     deleteAdmin(Request["user"], Request["group"], Request["cmp"], Request["stn"]);
           //     return;
           // }
           // else if ("iport".Equals(type))
           // {
           //     shipment_id = Request["SHIPMENT_ID"];
           //     sql = string.Format("SELECT * FROM SMSM WHERE SHIPMENT_ID={0}", Prolink.Data.SQLUtils.QuotedStr(shipment_id));
           //     DataTable dt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
           //     if (dt2.Rows.Count <= 0)
           //     {
           //         Response.Write("无此订舱单号");
           //         return;
           //     }
           //     u_id = Prolink.Math.GetValueAsString(dt2.Rows[0]["U_ID"]);
           //     BookingParser bp = new BookingParser();
           //     bp.SaveToTracking(u_id);
           //     Response.Write("订舱单转提单成功");
           //     return;
           // }


           
           // string house_no = Request["HOUSE_NO"];
           // string cd = Server.UrlDecode(Request["CD"]);
           // string desp = Server.UrlDecode(Request["DESP"]);
           // string location = Server.UrlDecode(Request["LOCATION"]);
          
           // if (!string.IsNullOrEmpty(u_id))
           //     sql = string.Format("SELECT * FROM TKBL WHERE U_ID={0}", Prolink.Data.SQLUtils.QuotedStr(u_id));
           // else
           //     sql = string.Format("SELECT * FROM TKBL WHERE HOUSE_NO={0}", Prolink.Data.SQLUtils.QuotedStr(house_no));
           // DataTable dt1 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
           // if (dt1.Rows.Count <= 0)
           // {
           //     Response.Write("无此提单");
           //     return;
           // }
           // u_id = Prolink.Math.GetValueAsString(dt1.Rows[0]["U_ID"]);
           // shipment_id = Prolink.Math.GetValueAsString(dt1.Rows[0]["SHIPMENT_ID"]);

           // //sql = string.Format("SELECT * FROM TKBLST WHERE SHIPMENT_ID={0} ORDER BY EVEN_DATE DESC", Prolink.Data.SQLUtils.QuotedStr(u_id));
           // //dt2 = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

           //// EvenNotify.Notify(u_id);

           // MixedList ml = new MixedList();
           // EditInstruct ei = new EditInstruct("TKBLST", EditInstruct.DELETE_OPERATION);
           // ei.PutKey("U_ID", u_id);
           // ei.PutKey("STS_CD", cd);
           // ml.Add(ei);

           // DateTimeOffset now = DateTimeOffset.Now;
           // ei = new EditInstruct("TKBLST", EditInstruct.INSERT_OPERATION);
           // string seq_no= System.Guid.NewGuid().ToString();
           // ei.Put("SEQ_NO", seq_no);
           // ei.Put("U_ID", u_id);
           // ei.Put("SHIPMENT_ID", shipment_id);
           // ei.Put("STS_CD", cd);
           // ei.Put("STS_DESCP", desp);
           // ei.Put("LOCATION", location);
           // ei.PutDate("EVEN_DATE", now.DateTime);
           // ei.PutDateTimeOffset("EVEN_TMG", now);

           // string group_id = Server.UrlDecode(Request["GROUP_ID"]);
           // string cmp = Server.UrlDecode(Request["CMP"]);
           // string stn = Server.UrlDecode(Request["STN"]);
           // ei.Put("GROUP_ID", group_id);
           // ei.Put("CMP", cmp);
           // ei.Put("STN", stn);
           // ei.Put("DEP", "*");
           // ml.Add(ei);
         
           // int[] rsult = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());

           // XmlParser.UpdateProccessStatus(u_id);
           // DocSender.Send(u_id, seq_no, cd, dt1.Rows[0]);
           // Response.Write("新增货况" + (rsult[1] > 0 ? "成功" : "失败"));
        }


        void btn_import_ServerClick(object sender, EventArgs e)
        {
            string COMPANYID_COOKIE_ID = "plv3.passport.companyid";
            string value = "";
            if (Request.Cookies[COMPANYID_COOKIE_ID] != null)
            {
                value = Request.Cookies[COMPANYID_COOKIE_ID].Value;
            }
            if (string.IsNullOrEmpty(value)) throw new Exception("Company获取为空!");
            string sapId = Business.TPV.Helper.GetSapId(value);
            string location = "";
            if (string.IsNullOrEmpty(sapId)) throw new Exception("获取失败,未找到当前所关链的SAP ID信息！");
            string v = import_mode.Value;
            switch (v)
            {
                case "A": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.Category); break;
                case "B": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.ContainerType); break;
                case "C": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.DistributionChannel); break;
                case "D": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.OrderType); break;
                case "E": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.Port); break;
                case "F": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.ProductLine); break;
                case "G": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.SalesOrganization); break;
                case "H": ImporterBaseCode(sapId, Business.TPV.Import.BaseCodeModes.TradeTerms); break;
                case "I": ImportExchangeRate(sapId,value); break;
                case "J": ImportPlant(sapId); break;
                case "K": ImportPOD(sapId); break;
                case "L": ImportDN(sapId); break;
                case "M": UpSeNO(sapId); break;
                case "N": GetProfile(sapId); break;
                case "O": ExportFee(sapId, location); break;
            }
        }

        void ExportFee(string sapId, string location)
        {
            if (string.IsNullOrEmpty(txt_DNNO.Value)) throw new Exception("Shipment ID不可为空!");
            Business.TPV.Export.FeeManager m = new Business.TPV.Export.FeeManager();
            var v = m.TryPostFeeInfo(sapId, txt_DNNO.Value.Trim(),location);
            if (v != null)
                result.InnerText = v.Description;
        }

        void GetProfile(string sapId)
        {
            if (string.IsNullOrEmpty(txt_DNNO.Value)) throw new Exception("Profile ID不可为空!");
            Business.TPV.Import.ProfileManager m = new Business.TPV.Import.ProfileManager();
            var v = m.Import(sapId, txt_DNNO.Value.Trim(),"FQ");
            if (v != null)
                result.InnerText = v.Description;
        }

        void UpSeNO(string sapId)
        {
            if (string.IsNullOrEmpty(txt_DNNO.Value)) throw new Exception("DN NO不可为空!");
            string sql = string.Format("UPDATE SMDN SET SEAL_QTY=QTY WHERE DN_NO={0}", SQLUtils.QuotedStr(txt_DNNO.Value.Trim()));
            try
            {
                OperationUtils.ExecuteUpdate(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
                result.InnerText = "成功!";
            }
            catch
            {
                result.InnerText = "失败!";
            }
        }

        void OnAnalyze(string sapId)
        {
            BackgroundWorker woker = new BackgroundWorker();
            woker.DoWork += woker_DoWork;
            woker.RunWorkerCompleted += woker_RunWorkerCompleted;
            woker.RunWorkerAsync(sapId);
        }

        void woker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        void woker_DoWork(object sender, DoWorkEventArgs e)
        {
            string sapId = e.Argument as string;
            ImportPartner(sapId);
        }

        void ImportPartner(string sapId)
        {
            Business.TPV.Import.PartnerManager m = new Business.TPV.Import.PartnerManager();
            var v = m.Import(sapId,"");
            if (v != null)
                result.InnerText = v.Description;
        }

        void ImportDN(string sapId)
        {
            if (string.IsNullOrEmpty(txt_DNNO.Value)) throw new Exception("DN NO不可为空!");
            Business.TPV.Import.DNManager m = new Business.TPV.Import.DNManager();
            var v = m.ImportDNForSAP(sapId, txt_DNNO.Value.Trim(), "");
            if (v != null)
                result.InnerText = v.Description;
        }
        
        void ImportPOD(string sapId)
        {
            string location = "";
            Business.TPV.Import.UnloadingPortManager m = new Business.TPV.Import.UnloadingPortManager();
            var v = m.Import(sapId, location);
            if (v != null)
                result.InnerText = v.Description;
        }

        void ImportPlant(string sapId)
        {
            Business.TPV.Import.PlantManager m = new Business.TPV.Import.PlantManager();
            var v = m.Import(sapId,"");
            if (v != null)
                result.InnerText = v.Description;
        }

        void ImportExchangeRate(string sapId, string location)
        {
            Business.TPV.Import.ExchangeRateManager m = new Business.TPV.Import.ExchangeRateManager();
            var v = m.Import(sapId, location, Business.TPV.RFC.ExchangeRateTypes.Standard);
            v = m.Import(sapId, location, Business.TPV.RFC.ExchangeRateTypes.EndOfTheMonth);
            if (v != null)
                result.InnerText = v.Description;
        }

        void ImporterBaseCode(string sapId, Business.TPV.Import.BaseCodeModes mode)
        {
            string location="";
            Business.TPV.Import.BaseCodeManager m = new Business.TPV.Import.BaseCodeManager();
            var v = m.Import(sapId, mode, location);
            if (v != null)
                result.InnerText = v.Description;
        }

        void AddAdmin(string user, string group, string cmp, string stn)
        {
            if ("TPV".Equals(group) || string.IsNullOrEmpty(user))
                return;
            MixedList ml = new MixedList();
            ml.Add(string.Format("DELETE FROM SYS_ACCT WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}'", group, cmp, stn));
            ml.Add(string.Format("DELETE FROM SYS_ACCT_ROLE WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FACCT_ID='{0}ADMIN'", group, cmp, stn));
            ml.Add(string.Format("DELETE FROM SYS_ROLE WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FID='{0}ADMIN'", group, cmp, stn));
            ml.Add(string.Format("DELETE FROM SYS_ROLE_OBJ_PMS WHERE FOBJ_ID LIKE 'SYS_PERMIS%' AND GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FROLE_ID='{0}ADMIN'", group, cmp, stn));
            ml.Add(string.Format("INSERT INTO SYS_ACCT(U_ID,GROUP_ID,CMP,STN,DEP,U_NAME,U_PASSWORD,U_STATUS,MODI_PW_DATE,UPDATE_PRI_DATE)VALUES('{0}','{1}','{2}','{3}','*','{0}' ,'{0}',0,'2015-12-01 13:59:00.000','2015-12-01 13:59:00.000')", user, group, cmp, stn));
            ml.Add(string.Format("INSERT INTO SYS_ACCT_ROLE(FACCT_ID,FROLE_ID,GROUP_ID,CMP,STN)VALUES('{0}','{1}ADMIN','{1}','{2}','{3}')", user, group, cmp, stn));
            ml.Add(string.Format("INSERT INTO SYS_ROLE(FID,GROUP_ID,CMP,STN,FDESCP)VALUES('{0}ADMIN','{0}','{1}','{2}','Admin')", group, cmp, stn));
            ml.Add(string.Format("INSERT INTO SYS_ROLE_OBJ_PMS(FROLE_ID,FOBJ_ID,GROUP_ID ,CMP,STN,FPMLIST) (SELECT DISTINCT '{0}ADMIN',FOBJ_ID,'{0}','{1}','{2}',FPMLIST FROM TPV.dbo.SYS_ROLE_OBJ_PMS WHERE GROUP_ID='ADS' AND CMP='YZYSZX' AND STN='YZY' AND FROLE_ID='ADSADMIN')", group, cmp, stn));
            int[] rsult = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            Prolink.V3.PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
        }

        void deleteAdmin(string user, string group, string cmp, string stn)
        {
            if ("TPV".Equals(group))
                return;
            MixedList ml = new MixedList(); ;
            ml.Add(string.Format("DELETE FROM SYS_ACCT WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}'", group, cmp, stn));
            ml.Add(string.Format("DELETE FROM SYS_ACCT_ROLE WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FACCT_ID='{0}ADMIN'", group, cmp, stn));
            ml.Add(string.Format("DELETE FROM SYS_ROLE WHERE GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FID='{0}ADMIN'", group, cmp, stn));

            ml.Add(string.Format("DELETE FROM SYS_ROLE_OBJ_PMS WHERE FOBJ_ID LIKE 'SYS_PERMIS%' AND GROUP_ID='{0}' AND CMP='{1}' AND STN='{2}' AND FROLE_ID='{0}ADMIN'", group, cmp, stn));
            int[] rsult = OperationUtils.ExecuteUpdate(ml, Prolink.Web.WebContext.GetInstance().GetConnection());
            Prolink.V3.PermissionManager.Build(Prolink.Web.WebContext.GetInstance());
        }
    }
}

namespace Test
{
    public class ImportExcel
    {
        public static void ImportParty()
        {


            //DataTable dt = Business.XExcelHelper.ImportExcelToDataTable(@"C:\Users\buru\Desktop\TPV\建档\Import\partner_2013-2015.XLSX");
            //Dictionary<string, List<string>> items = new Dictionary<string, List<string>>();
            //foreach (DataRow row in dt.Rows)
            //{
            //    string partyNO = Prolink.Math.GetValueAsString(row["PARTY_NO"]);
            //    string partyType = Prolink.Math.GetValueAsString(row["PARTY_TYPE"]);
            //    if (!items.ContainsKey(partyNO))
            //        items[partyNO] = new List<string>();
            //    List<string> list = items[partyNO];
            //    if (!list.Contains(partyType))
            //        list.Add(partyType);
            //}

            //Business.Utils.DBManager.BulidDatabaseFactory();

            //EditInstruct upPtEi = new EditInstruct("SMPTY", EditInstruct.UPDATE_OPERATION);
            //upPtEi.Put("PARTY_TYPE", null);
            //upPtEi.Condition = "PARTY_TYPE!='PL' AND PARTY_TYPE!='CP'";
            //Prolink.V6.Persistence.DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(upPtEi);

            //EditInstructList eiList = new EditInstructList();
            //foreach (var item in items)
            //{                
            //    EditInstruct ei = new EditInstruct("SMPTY", EditInstruct.UPDATE_OPERATION);
            //    string partyType=string.Format("{0};",string.Join(";",item.Value));
            //    ei.Put("PARTY_TYPE", partyType);
            //    ei.Condition = string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(item.Key));
            //    eiList.Add(ei);
            //}
            //EditInstructList newEiList = new EditInstructList();
            //for (int i = 0; i < eiList.Count; i++)
            //{
            //    newEiList.Add(eiList[i]);
            //    if (newEiList.Count == 100 || i == eiList.Count - 1)
            //    {
            //        Prolink.V6.Persistence.DatabaseFactory.GetDefaultDatabase().ExecuteUpdate(newEiList);
            //        newEiList = new EditInstructList();
            //    }
            //}

        }
    }


    class DHLTest
    {
        public static void TestTracking()
        {
            //Business.TPV.DHL.Manager m = new Business.TPV.DHL.Manager();
            //TrackingTemplate t = new TrackingTemplate();
            //t.AWBNumber.Add("3021566866");
            //t.UniqueNumber = System.Guid.NewGuid().ToString().Replace("-", "");
            //m.SendTrackcing(t);
        }

        public static void TestShipment()
        {
            Business.TPV.DHL.Manager m = new Manager();
            //m.SendBookingInfo("FQR1602000338");
        }

        public static void TestPikupShipment()
        {
            Business.TPV.DHL.Manager m = new Business.TPV.DHL.Manager();
            BookPickupTemplate t = new BookPickupTemplate();

            t.UniqueNumber = System.Guid.NewGuid().ToString().Replace("-", "");

            PickupShipmentDetailsInfo detail = new PickupShipmentDetailsInfo();
            detail.Weight = "99.9";
            detail.WeightUnit = WeightUnitCodes.K;
            detail.NumberOfPieces = 1;

            PickupInfo pInfo = new PickupInfo();
            pInfo.LocationType = LocationTypes.Business;
            pInfo.CompanyName = "TPV";
            pInfo.Address1 = "福州博连";
            pInfo.PackageLocation = "时代经典901#";
            pInfo.City = "福州";
            pInfo.CountryCode = "CN";
            pInfo.PostalCode = "350800";
            pInfo.PickupDate = DateTime.Now;
            pInfo.CloseTime = DateTime.Now.AddHours(4);
            pInfo.Weight = "99.9";
            pInfo.WeightUnit = WeightUnitCodes.K;
            pInfo.Pieces = 1;

            RequestorInfo rInfo = new RequestorInfo();
            rInfo.AccountNumber = "550000055";
            rInfo.Name = "sfsdfsdf";
            rInfo.Phone = "1223413";

            PickupContactInfo cInfo = new PickupContactInfo();
            cInfo.Name = "test001";
            cInfo.Phone = "5642122421";

            t.PickupShipmentDetailsInfo = detail;
            t.PickupInfo = pInfo;
            t.RequestorInfo = rInfo;
            t.PickupContactInfo = cInfo;
            //m.SendBookPickup(t);
        }
    }

    //class CoscoTest
    //{
    //    public static void Test()
    //    {
    //        Business.TPV.TNT.Manager m = new Business.TPV.TNT.Manager();
    //        m.SendBookingForCosco(new Business.TPV.Runtime() { ShipmentID = "FQB1604001122", OPUser = "TEST110", PartyNo = "0008950035", Location = "FQ" });
    //    }
    //}

    class TNTTest
    {
        public static void Test()
        {
            Business.TPV.TNT.Manager m = new Business.TPV.TNT.Manager();
            m.SendBooking(new Business.TPV.Runtime() { ShipmentID = "FQB1606000788", OPUser = "TEST110", PartyNo = "0008910024", Location = "FQ" });
        }

        //public static void TestShipment()
        //{
        //    Business.TPV.TNT.ShipmentTemplate t = new Business.TPV.TNT.ShipmentTemplate();
        //    t.UniqueNumber = "123456";
        //    Business.TPV.TNT.PartyInfo p = new Business.TPV.TNT.PartyInfo();
        //    p.CompanyName = "TPV";
        //    p.ContactDialCode = "+86";
        //    p.ContactTelephone = "124563241";
        //    p.ContactName = "TPV";
        //    p.PostCode = "350800";
        //    p.Province = "FQ";
        //    p.Country = "CN";
        //    p.Address1 = "XIAMEN";
        //    p.City = "FQ";
        //    p.Account = "35124421";

        //    Business.TPV.TNT.PartyInfo p2 = new Business.TPV.TNT.PartyInfo();
        //    p2.CompanyName = "TPV";
        //    p2.ContactDialCode = "+86";
        //    p2.ContactTelephone = "124563241";
        //    p2.ContactName = "roben";
        //    p2.PostCode = "350800";
        //    p2.Province = "FQ";
        //    p2.Country = "CN";
        //    p2.Address1 = "XIAMEN";
        //    p2.City = "FQ";

        //    t.Sender = p;
        //    t.Receiver = p2;

        //    Business.TPV.TNT.PackageInfo pk = new Business.TPV.TNT.PackageInfo();
        //    pk.Count = 10;
        //    pk.Description = "TV";
        //    pk.Height = "0.52";
        //    pk.Width = "1.00";
        //    pk.Length = "1.000";
        //    pk.Weight = "1.12";

        //    t.PackageInfo = pk;
        //    t.TotalItems = "2";
        //    t.TotalVolume = "50";
        //    t.TotalWeight = "10";
        //    t.GroupCode = "TPV";
        //    t.ShipDate = DateTime.Now;
        //    t.ConNumber = "DISKS";
        //    t.Service = "TNT";
        //    t.CollinsTructions = "test";


        //    Business.TPV.TNT.BetweenTime bt= new Business.TPV.TNT.BetweenTime() { From = DateTime.Now, To = DateTime.Now.AddHours(4) };
        //    t.PrefCollectTime = bt;
        //    t.AltCollectTime = bt;

        //    Business.TPV.TNT.Manager m = new Business.TPV.TNT.Manager();
        //    m.SendShipment(t);

        //}
    }

    class TraceTest
    {
        public static void TestTrace()
        {
            TraceInfo traceInfo = new TraceInfo();
            traceInfo.RefNO = "5121412121";
            traceInfo.Code = "TN";
            traceInfo.Descp = "货况描述";
            traceInfo.EventDate = DateTime.Now;// "货况时间";
            traceInfo.Location = "ERKEF";
            traceInfo.LocationName = "港口名称";
            traceInfo.Remark = "备注信息11112";

            WebGui.Interface.WebService.CPLWebService cpl = new WebGui.Interface.WebService.CPLWebService();
        }
    }

    class CPLTest
    {
        public static void TestImport()
        {
            try
            {
                Business.TPV.CPL.ImprotManager m = new Business.TPV.CPL.ImprotManager();

                foreach (var s in AnalyzeCPLBooking())
                {
                    m.ImportBookingResponse(new List<Models.EDI.CPL.BookingResponse> { s });
                }
            }
            catch
            {

            }
        }

        static IEnumerable<Models.EDI.CPL.BookingResponse> AnalyzeCPLBooking()
        {
            string path = @"C:\Users\buru\Desktop\TPV\UAT\中邮\test\";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles())
            {
                using (StreamReader reder = new StreamReader(item.FullName))
                {
                    string txt = reder.ReadToEnd();
                    Models.EDI.CPL.BookingResponse info = ToJsonObj<Models.EDI.CPL.BookingResponse>(txt);
                    yield return info;
                }
            }
        }

        static T ToJsonObj<T>(string jsonStr) where T : class
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Deserialize(jsonStr, typeof(T)) as T;
        }



        public static void TestCPL()
        {
            //Business.TPV.CPL.LogisticsWaybillAddTemplate t = new Business.TPV.CPL.LogisticsWaybillAddTemplate();
            //t.Company = "FJCPL";
            //t.BLNO = "12421454512";
            //t.CustomerOrderID = "S6500114104";
            //t.CustomerName = "冠捷";
            //t.BusinessType = "1";
            //t.QTY = "2";
            //t.Volume = "2.3";
            //t.Weight = "4.05";
            //t.LoadQTY = "20";
            //t.TransportMode = "1";

            //Business.TPV.CPL.Address address = new Business.TPV.CPL.Address();
            //address.Value = "湖里大道1号";
            //address.Province = "福建省";
            //address.City = "厦门市";
            //address.County = "湖里区";
            //address.Zip = "363000";

            //Business.TPV.CPL.PartyInfo sender = new Business.TPV.CPL.PartyInfo();
            //sender.Name = "始发地";
            //sender.Phone = "54212461";
            //sender.Mobile = "13526425822";
            //sender.Address = address;
            //t.Sender = sender;
            //t.Receiver = sender;
            //t.Customer = sender;

            Business.TPV.CPL.Manager m = new Business.TPV.CPL.Manager();
            //m.SendBooking("FQR1601000317");
            //m.SendBooking("FQR1601000324");            
        }
    }

    class SAPTest
    {
        public static void TestWebService()
        {
            //实例化一个DNInfo的对象
            Models.EDI.DNInfo dnInfo = new Models.EDI.DNInfo();
            {
                //对DN赋值
                //dnInfo.HeaderInfo=...
                //dnInfo.ItemInfos=....
                //...
            }
            //WebGui.Interface.WebService.SAPWebService service = new WebGui.Interface.WebService.SAPWebService();
            //service.PostDN(dnInfo);

            string dnNo = "10200082898760";
            string sapId = "QA1888";
            string location = "";
            Business.TPV.Import.DNManager dnManager = new Business.TPV.Import.DNManager();
            Business.Service.ResultInfo result = dnManager.ImportDNForSAP(sapId, dnNo, location);
        }

        public static void TestDNPosting()
        {
            string location = "";
            Business.TPV.Import.DeliveryPostingManager dp = new Business.TPV.Import.DeliveryPostingManager();
            Business.TPV.RFC.DPResultInfo result = null;
            string dnno = "11000085696794";
            Business.TPV.Import.DeliveryPostingInfo dpInfo = new Business.TPV.Import.DeliveryPostingInfo()
            {
                DNNO = dnno,
                GoodsMovementDate = DateTime.Now
            };
            bool isSucced = dp.TryPostingDate("QA1888", dpInfo, out result, location);
        }

        public static void TestPostICAInfo()
        {
            string sql = string.Format("SELECT CMP,SHIPMENT_ID,DN_NO AS COMBINE_INFO FROM SMIDN WHERE DN_NO={0}", SQLUtils.QuotedStr("20100085696691"));
            DataTable dndt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (dndt.Rows.Count > 0)
            {
                Business.TPV.Helper.SendICACargoInfoByInbound(dndt.Rows[0]);
            }
        }
    }

    public class BookingTest
    {
        public static void TestBookingRsp()
        {
            WebGui.Interface.WebService.BookingWebService se = new WebGui.Interface.WebService.BookingWebService();
            OceanBookingResponse rs = new OceanBookingResponse();
            rs.Port = rs.POD = rs.POL = rs.POR = rs.DEST = "USSFO";
            rs.ETA = rs.ETD = rs.VesselETA1 = rs.VesselETD1 = rs.PortClose1 = rs.PortClose2 = DateTime.Now;
            se.PostOceanBookingResponse(rs);
        }
    }

    public class LSPTest
    {
        public static void TestBooking()
        {
            XmlDocument doc = new XmlDocument();
            string filePath = @"C:\Users\buru\Desktop\TPV\外代EDI\外代测试\resp20160202.xml";
            string xml = new System.IO.StreamReader(filePath).ReadToEnd();
            doc.LoadXml(xml);
            WebGui.Interface.WebService.BookingWebService s = new WebGui.Interface.WebService.BookingWebService();
            s.Login("test100", "test100");
            Business.Service.ResultInfo rsult = s.PostOceanBookingResponseXml(doc.InnerXml);
        }

        public static void TestDR(string url)
        {
            WebGui.Interface.WebService.DeclarationWebService dw = new WebGui.Interface.WebService.DeclarationWebService();
            XmlDocument doc = new XmlDocument();
            doc.Load(url);
            dw.PostDeclarationInfoXml(doc.InnerXml);
        }

        public static void Test()
        {
            DeclarationInfo info = new DeclarationInfo();
            //info.DeclarationNumber = "TEST12345";
            info.Mode = "A";
            info.FileExtension = "PDF";

            using (Stream stream = new FileStream(@"C:\Users\buru\Desktop\TPV\UAT\DHL\responed\FQR1603000616_152045798.PDF", FileMode.Open))
            {
                byte[] bs = new byte[stream.Length];
                stream.Read(bs, 0, bs.Length);
                string data = Convert.ToBase64String(bs);
                info.FileData = data;
            }

            byte[] imageBytes = Convert.FromBase64String(info.FileData);
            string fileName = DateTime.Now.ToString("HHmmssfff");
            string filePath = string.Format(@"C:\Users\buru\Desktop\TPV\UAT\DHL\responed\{0}.PDF", fileName);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }


            Business.TPV.LSP.ExportManager m = new Business.TPV.LSP.ExportManager();
            //m.SendBooking(new Business.TPV.Runtime
            //{
            //    ShipmentID = "FQR1601000324" FQR1602000338
            //});      

            m.SendBooking(new Business.TPV.Runtime
            {
                ShipmentID = "FQR1602000338"
            });


        }
    }

    public class TestEDI
    {
        public static void Test()
        {
            //CoscoTest.Test();
            //TNTTest.Test();
            //ImportExcel.ImportParty();
            // LSPTest.TestBooking();
            //LSPTest.TestDR();
            //CPLTest.TestCPL();
            //BookingTest.TestBookingRsp();
            //SAPTest.TestDNPosting();
            //DHLTest.TestPikupShipment();
            //TNTTest.TestShipment();
            //DHLTest.TestShipment();
            // SAPTest.TestWebService();
            // CPLTest.TestImport();
            // Business.TPV.Context.OrgService.GetSuperior("DYLAN.CHO");
        }
    }
}