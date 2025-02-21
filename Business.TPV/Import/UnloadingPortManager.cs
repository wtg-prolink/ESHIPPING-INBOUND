using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class UnloadingPortManager : ManagerBase
    {
        public ResultInfo Import(string sapId, string location)
        {
            UnloadingPortEDI edi = new UnloadingPortEDI();
            List<UnloadingPortInfo> infos = edi.Distinct(edi.GetUnloadingPortInfo(sapId,location));
            DataTable dt = Query(sapId);
            var result= Save(infos, dt);
            WriteEDILog(infos, sapId, result);
            return result;
        }
        void WriteEDILog(List<UnloadingPortInfo> infoList, string refNO, ResultInfo result)
        {
            Utils.EdiInfo info = null;
            UnLoadingPortEDILog log = new UnLoadingPortEDILog(infoList, refNO, "System");
            if (result.IsSucceed)
            {
                info = log.CreateSucceed();
            }
            else
            {
                info = log.CreateEx(result.Description);
            }
            Logger.WriteLog(info);
        }
        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "TPVPORT" };
        }

        DataTable Query(string sapId)
        {
            string sql = "SELECT CNTY,PORT FROM TPVPORT";
            return DB.GetDataTable(sql, new string[] { });
        }

        internal static string GetCondition(string country, string prot)
        {
            List<ConditionItem> items = new List<ConditionItem>();
            items.Add(new ConditionItem("CNTY", country));
            items.Add(new ConditionItem("PORT", prot));
            return DBManager.CreateCondition(items);
        }

        ResultInfo Save(List<UnloadingPortInfo> infos, DataTable preDT)
        {
            Func<UnloadingPortInfo, bool> checkHas = info =>
            {
                DataRow[] rows = preDT.Select(GetCondition(info.CountryKey, info.Code));
                return rows != null && rows.Length > 0;
            };
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.Code, GetCurrentTimeString()));
                EditInstruct ei = CreatePortEi(new ProtInfo
                {
                    Country = item.CountryKey,
                    Mode = PortModes.POD,
                    Name = item.Description,
                    Port = item.Code,
                    Remark = item.LanguageCode
                }, checkHas(item) ? EditInstruct.UPDATE_OPERATION : EditInstruct.INSERT_OPERATION);
                eiList.Add(ei);
            }
            return Execute(eiList, 100);
        }

        static DataTable _cntyDT;
        static DataTable GetCntyDT()
        {
            if (_cntyDT == null)
            {
                string sql = "SELECT CNTRY_CD,CNTRY_NM FROM BSCNTY";
                _cntyDT = Prolink.V6.Persistence.DatabaseFactory.GetDefaultDatabase().GetDataTable(sql, new string[] { });
            }
            return _cntyDT;
        }

        internal static EditInstruct CreatePortEi(ProtInfo info, int opeartion)
        {
            DataTable dt = GetCntyDT();
            Func<string, object> getCntyNM = code =>
                {
                    if (dt == null || dt.Rows.Count <= 0) return null;
                    DataRow[] rows = dt.Select(string.Format("CNTRY_CD={0}", SQLUtils.QuotedStr(code)));
                    if (rows == null || rows.Length <= 0) return null;
                    return rows[0]["CNTRY_NM"];
                };
            EditInstruct ei = new EditInstruct("TPVPORT", opeartion);
            if (opeartion == EditInstruct.UPDATE_OPERATION)
            {
                ei.Condition = GetCondition(info.Country, info.Port);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            ei.Put("FLAG", info.Mode == PortModes.POD ? "D" : "L");
            ei.Put("CNTY", info.Country);
            ei.Put("CNTY_NM", getCntyNM(info.Country));
            ei.Put("PORT ", info.Port);
            ei.Put("PORT_NM", info.Name);
            ei.Put("REMARK", info.Remark);
            return ei;
        }
    }

    class ProtInfo
    {
        public PortModes Mode{get;set;}
        public string Country{get;set;}
        public string Port{get;set;}
        public string Name{get;set;}
        public string Remark { get; set; }
    }

    enum PortModes{POL,POD}
}