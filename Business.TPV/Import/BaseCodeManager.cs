using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class BaseCodeManager : ManagerBase
    {
        /// <summary>
        /// 导入基本建档
        /// </summary>
        /// <param name="sapId"></param>
        /// <param name="mode"></param>
        public ResultInfo Import(string sapId, BaseCodeModes mode, string location)
        {
            List<BaseCodeInfo> infoList = new List<BaseCodeInfo>();
            RFC.BaseCodeEDI edi = new BaseCodeEDI();
            List<string> types = GetBaseCodes(mode);
            foreach (var type in types)
            {
                List<BaseCodeInfo> items = edi.Distinct(edi.GetBaseCode(sapId, type, location));
                infoList.AddRange(items);
            }
            DataTable dt = QueryBaseCode(types);
            var result=SaveBaseCode(infoList, dt);
            WriteEDILog(infoList,result);
            return result;
        }

        void WriteEDILog(List<BaseCodeInfo> infoList, ResultInfo result)
        {
            Utils.EdiInfo info = null;
            BaseCodeEDILog log = new BaseCodeEDILog(infoList, "System");
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
        List<string> GetBaseCodes(BaseCodeModes mode)
        {
            List<string> list = new List<string>();
            Action<string> addCode = code =>
                {
                    if (!list.Contains(code))
                        list.Add(code);
                };
            foreach (var item in Enum.GetValues(typeof(BaseCodeModes)).OfType<BaseCodeModes>().Where(t => ((t & mode) == t)))
            {
                switch (item)
                {
                    case BaseCodeModes.All:
                        foreach (var name in Enum.GetNames(typeof(BaseCodeModes)))
                        {
                            BaseCodeModes m;
                            if (!Enum.TryParse<BaseCodeModes>(name, out m)) continue;
                            if (m == BaseCodeModes.All) continue;
                            addCode(GetBaseCode(m));
                        }
                        break;
                    default: addCode(GetBaseCode(item)); break;
                }
            }
            return list;
        }
        string GetBaseCode(BaseCodeModes mode)
        {
            switch (mode)
            {
                case BaseCodeModes.Category: return "TVPT";
                case BaseCodeModes.ContainerType: return "VERP";
                case BaseCodeModes.DistributionChannel: return "TVTW";
                case BaseCodeModes.OrderType: return "TVAK";
                case BaseCodeModes.Port: return "TVST";
                case BaseCodeModes.ProductLine: return "TSPA";
                case BaseCodeModes.SalesOrganization: return "TVKO";
                case BaseCodeModes.TradeTerms: return "TINC";
            }
            return null;
        }

        ResultInfo SaveBaseCode(IEnumerable<BaseCodeInfo> infos, DataTable preDT)
        {
            Func<string, string, bool> checkHas = (type, code) =>
                {
                    DataRow[] rows = preDT.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
                    return rows != null && rows.Length > 0;
                };
            Func<BaseCodeInfo, bool> portCheckHas = info =>
            {
                DataTable portDT = GetPortDT();
                DataRow[] rows = portDT.Select(UnloadingPortManager.GetCondition("CN", info.Code));
                return rows != null && rows.Length > 0;
            };
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.Code, GetCurrentTimeString()));
                EditInstruct ei = null;
                switch (item.Type)
                {
                    case "TVST": ei = CreatePortEi(item, portCheckHas(item) ? EditInstruct.UPDATE_OPERATION : EditInstruct.INSERT_OPERATION);
                        break;
                    default:
                        if (checkHas(item.Type, item.Code))
                        {
                            ei = new EditInstruct("BSCODE", EditInstruct.UPDATE_OPERATION);
                            ei.Condition = string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(item.Type), SQLUtils.QuotedStr(item.Code));
                        }
                        else
                        {
                            ei = new EditInstruct("BSCODE", EditInstruct.INSERT_OPERATION);
                            ei.PutDate("CREATE_DATE", DateTime.Now);
                        }
                        ei.Put("CMP", "*");
                        ei.Put("STN", "*");
                        ei.Put("GROUP_ID", Context.GroupId);
                        ei.Put("GROUP_ID", Business.TPV.Context.GroupId);
                        ei.Put("CD_TYPE", item.Type);
                        ei.Put("CD", item.Code);
                        ei.Put("CD_DESCP", item.Description);
                        break;
                }
                eiList.Add(ei);
            }
            return Execute(eiList, 100);
        }

        EditInstruct CreatePortEi(BaseCodeInfo info, int operation)
        {
            return UnloadingPortManager.CreatePortEi(new ProtInfo
            {
                Country = "CN",
                Mode = PortModes.POL,
                Name = info.Description,
                Port = info.Code,
                Remark = info.Type
            }, operation);
        }

        DataTable _portDT;
        DataTable GetPortDT()
        {
            if (_portDT == null)
            {
                string sql = "SELECT CNTY,PORT FROM TPVPORT";
                _portDT = DB.GetDataTable(sql, new string[] { });
            }
            return _portDT;
        }

        DataTable QueryBaseCode(List<string> types)
        {
            string sql = string.Format("SELECT CD_TYPE,CD FROM BSCODE WHERE CD_TYPE IN({0})",
                string.Join(",", types.Select(t => SQLUtils.QuotedStr(BaseCodeEDI.GetSaveTypeCode(t)))));
            return DB.GetDataTable(sql, new string[] { });
        }
    }
    /// <summary>
    /// 基本建档
    /// </summary>
    [Flags]
    public enum BaseCodeModes
    {
        All = 1,
        /// <summary>
        /// 销售渠道
        /// </summary>
        DistributionChannel = 2,
        /// <summary>
        /// 产品组
        /// </summary>
        ProductLine = 4,
        /// <summary>
        /// 贸易条款
        /// </summary>
        TradeTerms = 8,
        /// <summary>
        /// 类别
        /// </summary>
        Category = 16,
        /// <summary>
        /// 港口(启运港)
        /// </summary>
        Port = 32,
        /// <summary>
        /// 货柜尺寸
        /// </summary>
        ContainerType = 64,
        /// <summary>
        /// 销售组织
        /// </summary>
        SalesOrganization = 128,
        /// <summary>
        /// 订单类型
        /// </summary>
        OrderType = 256
    }
}
