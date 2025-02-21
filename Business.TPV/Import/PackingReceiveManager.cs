using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class PackingReceiveManager:ManagerBase
    {
        public ResultInfo Import(string sapId, string location, string dnno = null)
        {
            PackingReceiveEDI edi = new PackingReceiveEDI();
            List<PackingReceiveInfo> infos = edi.GetPackingReceiveInfo(sapId, location).ToList();
            DataTable dt = QueryM(infos);
            return Save(infos, dt);
        }

        public ResultInfo Import(IEnumerable<PackingReceiveInfo> infos)
        {
            List<PackingReceiveInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return new ResultInfo { IsSucceed = false, ResultCode = "NullData", Description = "未发现需要处理的数据!" };
            DataTable mDT = QueryM(items);
            var result = Save(items, mDT);

            var itemselects = infos.Select(item => item.DNNO).Distinct();
            foreach (var item in itemselects)
            {
                PackingReceiveEDILog log = new PackingReceiveEDILog(item, "System");
                if (result.IsSucceed)
                {
                    Logger.WriteLog(log.CreateSucceed());
                }
                else
                {
                    Logger.WriteLog(log.CreateEx(result.Description));
                }
            }
            return result;
        }

        DataTable QueryM(List<PackingReceiveInfo> infos)
        {
            string condition = GetCondition(infos);
            string sql = string.Format("SELECT * FROM SMINP{0}", string.IsNullOrEmpty(condition) ? string.Empty :
                string.Format(" WHERE {0}", condition));
            return DB.GetDataTable(sql, new string[] { });
        }

        string GetConditionM(PackingReceiveInfo info)
        {
            List<ConditionItem> conditions = new List<ConditionItem>();
            conditions.Add(new ConditionItem("DN_NO", info.DNNO));
            return DBManager.CreateCondition(conditions);
        }

        string GetCondition(List<PackingReceiveInfo> infos)
        {
            return string.Format("DN_NO IN({0})",
                string.Join(",", infos.Select(c => SQLUtils.QuotedStr(c.DNNO))));
        }

        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "SMINP" };
        }

        string GetItemCondition(PackingReceiveInfo info)
        {
            List<ConditionItem> items = new List<ConditionItem>();
            //DN NO	pallet	From	to	material
            items.Add(new ConditionItem("DN_NO", info.DNNO));
            items.Add(new ConditionItem("PLA_NO", info.Pallet));
            items.Add(new ConditionItem("CASE_FROM", info.From));
            items.Add(new ConditionItem("CASE_TO", info.To));
            items.Add(new ConditionItem("IPART_NO", info.Material));
            return DBManager.CreateCondition(items);
        }

        ResultInfo Save(List<PackingReceiveInfo> infos, DataTable preDT)
        {
            Func<PackingReceiveInfo, bool> checkHas = info =>
            {
                DataRow[] rows = preDT.Select(GetItemCondition(info));
                return rows != null && rows.Length > 0;
            };
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.DNNO, GetCurrentTimeString()));
                EditInstruct ei = null;
                if (checkHas(item))
                {
                    ei = new EditInstruct("SMINP", EditInstruct.UPDATE_OPERATION);
                    ei.Condition = GetItemCondition(item);
                    ei.PutDate("MODIFY_DATE", DateTime.Now);
                }
                else
                {
                    return new ResultInfo { IsSucceed = false, ResultCode = "NullData", Description = string.Format("未发现匹配的数据!Material：{0}", SQLUtils.QuotedStr(item.Material)) };
                    //ei = new EditInstruct("SMINP", EditInstruct.INSERT_OPERATION);
                    //ei.Put("U_ID", System.Guid.NewGuid().ToString());
                    //ei.PutDate("CREATE_DATE", DateTime.Now);
                }
                ei.Put("TTL_NW", item.NetWeight);
                ei.Put("GW_BY_PN", item.UnitWeight);
                //ei.Put("GROUP_ID", Business.TPV.Context.GroupId);
                ei.Put("GOODS_DESCP", item.EngDescp);
                string ncmno = item.NcmNo;
                if (!string.IsNullOrEmpty(item.NcmNo))
                {
                    ncmno = ncmno.Replace(".", "");
                    if (ncmno.Length > 6)
                        ncmno = ncmno.Substring(0, 6);
                }
                ei.Put("NCM_NO ", ncmno);
                ei.Put("CN_CODE", item.CnCode);
                ei.Put("LGOODS_DESCP", item.ChinaDescp);
                ei.Put("SAP_SEND", "Y");
                eiList.Add(ei);
            }
            return Execute(eiList);
        }
    }
}
