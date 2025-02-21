using Business.Service;
using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class PlantManager : ManagerBase
    {
        public ResultInfo Import(string sapId, string location,string planCode = null)
        {
            PlantEDI edi = new PlantEDI();
            List<PlantInfo> infos = edi.GetPlantInfo(sapId,location, planCode).ToList();
            DataTable dt = Query(sapId, planCode);
            return Save(infos, dt);
        }

        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "SMPTY" };
        }

        DataTable Query(string sapId, string planCode)
        {
            string condition = GetCondition(planCode);
            string sql = string.Format("SELECT * FROM SMPTY{0}", string.IsNullOrEmpty(condition) ? string.Empty : string.Format(" WHERE {0}", condition));
            return DB.GetDataTable(sql, new string[] { });
        }

        string GetCondition(string planCode)
        {
            List<ConditionItem> items = new List<ConditionItem>();
            items.Add(new ConditionItem("PARTY_NO", planCode));
            items.Add(new ConditionItem("PARTY_TYPE", PartyType));
            return DBManager.CreateCondition(items);
        }

        const string PartyType = "PL";

        ResultInfo Save(List<PlantInfo> infos, DataTable preDT)
        {
            Func<PlantInfo, bool> checkHas = info =>
            {
                DataRow[] rows = preDT.Select(GetCondition(info.PlantCode));
                return rows != null && rows.Length > 0;
            };
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.PlantCode, GetCurrentTimeString()));
                EditInstruct ei = null;
                if (checkHas(item))
                {
                    ei = new EditInstruct("SMPTY", EditInstruct.UPDATE_OPERATION);
                    ei.Condition = GetCondition(item.PlantCode);
                    ei.PutDate("MODIFY_DATE", DateTime.Now);
                }
                else
                {
                    ei = new EditInstruct("SMPTY", EditInstruct.INSERT_OPERATION);
                    ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    ei.PutDate("CREATE_DATE", DateTime.Now);
                }
                ei.Put("STATUS", "U");
                ei.Put("PARTY_NO", item.PlantCode);
                ei.Put("GROUP_ID", Business.TPV.Context.GroupId);
                ei.Put("PARTY_TYPE", PartyType);
                ei.Put("PARTY_NAME ", item.Description);
                ei.Put("BILL_TO", item.CompanyCode);
                eiList.Add(ei);
            }
            return Execute(eiList);
        }
    }
}