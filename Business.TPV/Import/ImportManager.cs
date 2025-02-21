using Prolink.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    class ImportManager : IPlanTask
    {
        public ImportManager() { }

        private string _code = null;
        public ImportManager(string code)
        {
            _code = code;
        }

        string _location = "";
        public ImportManager(string code, string location)
        {
            _code = code;
            _location = location;
        }

        public ImportManager(string code, string location, string sapid)
        {
            _code = code;
            _location = location;
            SAPID = sapid;
        }

        //private const string SAPID = "QA1888";
        //private const string SAPID = "DEV150";
        //private const string SAPID = "TPV888";
        private string SAPID = "TPV888";
     
        public void Run(IPlanTaskMessenger messenger)
        {
            try
            {
                switch (_code)
                {
                    case "A": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.Category); break;
                    case "B": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.ContainerType); break;
                    case "C": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.DistributionChannel); break;
                    case "D": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.OrderType); break;
                    case "E": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.Port); break;
                    case "F": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.ProductLine); break;
                    case "G": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.SalesOrganization); break;
                    case "H": ImporterBaseCode(SAPID, Business.TPV.Import.BaseCodeModes.TradeTerms); break;
                    case "I": ImportExchangeRate(SAPID, "FQ"); break;
                    case "J": ImportPlant(SAPID); break;
                    case "K": ImportPOD(SAPID); break;
                    case "L": ImportPartner(SAPID); break;
                    case "M": ImportCompany(SAPID); break;
                    case "N": ImportUnloadingPort(SAPID); break;
                }

                //ProfileManager profileManager = new ProfileManager();
                //profileManager.Import(SAPID);
                //BaseCodeManager manager = new BaseCodeManager();
                //manager.Import(SAPID, BaseCodeModes.Category);
                //CompanyManager cManager = new CompanyManager();
                //cManager.Import(SAPID);
                //ExchangeRateManager exManager = new ExchangeRateManager();
                //exManager.Import(SAPID);
                //PlantManager pManager = new PlantManager();
                //pManager.Import(SAPID);
                //pManager.Import(SAPID, "1090");

                //UnloadingPortManager unLoadingManager = new UnloadingPortManager();
                //unLoadingManager.Import(SAPID);
                //PartnerManager ptManager = new PartnerManager();
                //ptManager.Import(SAPID);
                //ReloadCompany();

            }
            catch(Exception ex)
            {
                Log.DLogger log = new Log.DLogger();
                log.WriteLog("执行代码：_code=" + _code.ToString() + " Exception:" + ex.ToString());
            }
        }

        void ImportUnloadingPort(string sapId)
        {
            UnloadingPortManager unLoadingManager = new UnloadingPortManager();
            unLoadingManager.Import(SAPID,_location);
        }


        void ImportCompany(string sapId)
        {
            Business.TPV.Import.CompanyManager m = new Business.TPV.Import.CompanyManager();
            var v = m.Import(sapId,_location);
        }

        void ImportPartner(string sapId)
        {
            Business.TPV.Import.PartnerManager m = new Business.TPV.Import.PartnerManager();
            //var v = m.Import(sapId,_location, null, DateTime.Now.AddDays(-1), DateTime.Now);
            var v = m.Import(sapId, _location, null, null, null);
        }

        void ImportPOD(string sapId)
        {
            Business.TPV.Import.UnloadingPortManager m = new Business.TPV.Import.UnloadingPortManager();
            var v = m.Import(sapId,_location);
        }

        void ImportPlant(string sapId)
        {
            Business.TPV.Import.PlantManager m = new Business.TPV.Import.PlantManager();
            var v = m.Import(sapId,_location);
        }

        void ImportExchangeRate(string sapId, string location)
        {
            int nowday = DateTime.Now.Day;
            int hour = DateTime.Now.Hour;
            Log.DLogger log = new Log.DLogger();
            log.WriteLog("执行时间：nowday=" + nowday.ToString() + " hour:" + hour.ToString());
            if (nowday == 1&&hour==1)
            {
                Business.TPV.Import.ExchangeRateManager m = new Business.TPV.Import.ExchangeRateManager();
                var v = m.Import(sapId, location, Business.TPV.RFC.ExchangeRateTypes.Standard);
                v = m.Import(sapId, location, Business.TPV.RFC.ExchangeRateTypes.EndOfTheMonth);
            }
        }

        void ImporterBaseCode(string sapId, Business.TPV.Import.BaseCodeModes mode)
        {
            Business.TPV.Import.BaseCodeManager m = new Business.TPV.Import.BaseCodeManager();
            var v = m.Import(sapId, mode, _location);
        }

        void ReloadCompany()
        {
            string sql = "SELECT PARTY_NO FROM SMPTY WHERE PARTY_TYPE<>'PL' AND PARTY_TYPE<>'CP'";
            PartnerManager m = new PartnerManager();
            DataTable dt = Business.Utils.DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            List<string> fails=new List<string>();
            List<string> list=new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                string partyNO = Prolink.Math.GetValueAsString(row["PARTY_NO"]);
                if (string.IsNullOrEmpty(partyNO)) continue;
                var v= m.Import(SAPID,_location, partyNO);
                if (!v.IsSucceed)
                {
                    if (!fails.Contains(partyNO))
                        fails.Add(partyNO);
                }
                else
                {
                    if (!list.Contains(partyNO))
                        list.Add(partyNO);
                }
            }
        }
    }
}
