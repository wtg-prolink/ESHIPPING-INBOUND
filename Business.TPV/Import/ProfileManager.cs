using Business.Service;
using Business.TPV.Base;
using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Business.TPV.Import
{
    public class ProfileManager : ManagerBase
    {
        public ResultInfo Import(string sapId, string profileCode,string location)
        {
            CustomerProfileEDI edi = new CustomerProfileEDI();
            var infos = edi.GetCustomerProfileInfo(sapId, profileCode, location);
            return Import(infos);
        }

        public ResultInfo Import(IEnumerable<ProfileInfo> infos)
        {
            List<ProfileInfo> items = infos.ToList();
            if (items == null || items.Count <= 0) return new ResultInfo { IsSucceed = false, ResultCode = "NullData", Description = "未发现需要处理的数据!" };
            DataTable mDT = QueryM(items);
            var result = Save(items, mDT);
            foreach (var item in infos)
            {
                ImportProfileEDILog log = new ImportProfileEDILog(item, "System");
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

        protected override List<string> GetCheckTables()
        {
            return new List<string>() { "SMSIM" };
        }
        string GetConditionM(ProfileInfo info)
        {
            List<ConditionItem> conditions = new List<ConditionItem>();
            conditions.Add(new ConditionItem("PROFILE", info.ProfileCode));
            return DBManager.CreateCondition(conditions);
        }

        string GetCondition(List<ProfileInfo> infos)
        {
            return string.Format("PROFILE IN({0})",
                string.Join(",", infos.Select(c => SQLUtils.QuotedStr(c.ProfileCode))));
        }
        DataTable QueryM(List<ProfileInfo> infos)
        {
            string condition = GetCondition(infos);
            string sql = string.Format("SELECT * FROM SMSIM{0}", string.IsNullOrEmpty(condition) ? string.Empty :
                string.Format(" WHERE {0}", condition));
            return DB.GetDataTable(sql, new string[] { });
        }

        ResultInfo Save(List<ProfileInfo> infos, DataTable mDT)
        {
            Func<ProfileInfo, DataRow> checkHasM = info =>
            {
                string condition = GetConditionM(info);
                DataRow[] rows = mDT.Select(condition);
                if (rows != null && rows.Length > 0)
                    return rows[0];
                return null;
            };
            DataTable transDT = QueryTransModeDT(infos);
            DataTable portDT = QueryPortDT(infos);
            DataTable partyDT = QueryPartyDT(infos);
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                BackupData(item, string.Format("{0}_{1}", item.ProfileCode, GetCurrentTimeString()));
                EditInstruct ei = null;
                DataRow mRow = checkHasM(item);
                if (mRow == null)
                {
                    ei = CreateM(item, partyDT, portDT, transDT, EditInstruct.INSERT_OPERATION);
                }
                else
                {
                    ei = CreateM(item, partyDT, portDT, transDT, EditInstruct.UPDATE_OPERATION);
                }
                eiList.Add(ei);
            }
            return Execute(eiList);
        }

        DataTable QueryPartyDT(List<ProfileInfo> infos)
        {
            List<string> partyCodes = new List<string>();
            Action<string> addParty = code =>
                {
                    if (string.IsNullOrEmpty(code)) return;
                    if (!partyCodes.Contains(code))
                        partyCodes.Add(code);
                };
            infos.ForEach(item =>
                {
                    addParty(item.Customer);
                    addParty(item.Seller);
                    addParty(item.Consignee);
                    addParty(item.Buyer1);
                    addParty(item.Buyer2);
                    addParty(item.Buyer3);
                    addParty(item.Buyer4);
                    addParty(item.Buyer5);
                    addParty(item.Notify1);
                    addParty(item.Notify2);
                    addParty(item.Notify3);
                });
            if (partyCodes.Count <= 0) return null;
            string sql = string.Format("SELECT PARTY_NO,PARTY_NAME FROM SMPTY WHERE PARTY_NO IN({0})",
                string.Join(",", partyCodes.Select(p => SQLUtils.QuotedStr(p))));
            return DB.GetDataTable(sql, new string[] { });
        }
        DataTable QueryPortDT(List<ProfileInfo> infos)
        {
            List<string> portCodes = new List<string>();
            infos.ForEach(item =>
                     {
                         if (string.IsNullOrEmpty(item.PortOfDischarge)) return;
                         if (!portCodes.Contains(item.PortOfDischarge))
                             portCodes.Add(item.PortOfDischarge);
                     });
            if (portCodes.Count <= 0) return null;
            string sql = string.Format("SELECT PORT_NM,PROLINK_CD FROM TPVPORT WHERE PORT_NM IN({0})",
               string.Join(",", portCodes.Select(p => SQLUtils.QuotedStr(p))));
            return DB.GetDataTable(sql, new string[] { });
        }
        DataTable QueryTransModeDT(List<ProfileInfo> infos)
        {
            List<string> codes = new List<string>();
            infos.ForEach(item =>
                     {
                         if (string.IsNullOrEmpty(item.ShippingMode)) return;
                         if (!codes.Contains(item.ShippingMode))
                             codes.Add(item.ShippingMode);
                     });
            if (codes.Count <= 0) return null;
            string sql = string.Format("SELECT AP_CD,CD,CD_DESCP FROM BSCODE WHERE CD_TYPE='TTRN' AND CD IN({0})",
               string.Join(",", codes.Select(p => SQLUtils.QuotedStr(p))));
            return DB.GetDataTable(sql, new string[] { });
        }
        Tuple<string, string> GetTransCode(string code, DataTable transDT)
        {
            if (transDT == null || transDT.Rows.Count <= 0) return null;
            DataRow[] rows = transDT.Select(string.Format("CD={0}", SQLUtils.QuotedStr(code)));
            if (rows == null || rows.Length <= 0) return null;
            DataRow row = rows[0];
            string item1 = Prolink.Math.GetValueAsString(row["AP_CD"]);
            string item2 = Prolink.Math.GetValueAsString(row["CD_DESCP"]);
            return new Tuple<string, string>(item1, item2);
        }

        string GetPort(string port, DataTable portDT)
        {
            if (portDT == null || portDT.Rows.Count <= 0) return null;
            DataRow[] rows = portDT.Select(string.Format("PORT_NM={0}", SQLUtils.QuotedStr(port)));
            if (rows == null || rows.Length <= 0) return null;
            return Prolink.Math.GetValueAsString(rows[0]["PROLINK_CD"]);
        }
        string GetPartyName(string partyCode, DataTable partyDT)
        {
            if (partyDT == null || partyDT.Rows.Count <= 0) return null;
            DataRow[] rows = partyDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(partyCode)));
            if (rows == null || rows.Length <= 0) return null;
            return Prolink.Math.GetValueAsString(rows[0]["PARTY_NAME"]);
        }

        string GetCMP(ProfileInfo info)
        {
            if (info == null) return null;
            if (string.IsNullOrEmpty(info.ProfileCode)) return null;
            if (info.ProfileCode.Length < 3) return null;
            string code = info.ProfileCode.Substring(0, 3);
            switch (code)
            {
                case "PRF": return "FQ";
                case "PXM": return "XM";
                case "PBJ": return "BJ";
                case "PQD": return "QD";
                case "PWH": return "WH";
                case "PBH": return "BH";
            }
            return null;
        }

        EditInstruct CreateM(ProfileInfo info, DataTable partyDT, DataTable portDT, DataTable transDT, int operation)
        {
            EditInstruct ei = new EditInstruct("SMSIM", operation);
            if (operation == EditInstruct.UPDATE_OPERATION)
            {
                ei.Condition = GetConditionM(info);
                ei.PutDate("MODIFY_DATE", DateTime.Now);
            }
            else
            {
                string uid = System.Guid.NewGuid().ToString();
                ei.Put("U_ID", uid);
                ei.PutDate("CREATE_DATE", DateTime.Now);
            }
            ei.Put("PROFILE", info.ProfileCode);
            ei.Put("PROFILE_NM", info.ProfileName);
            ei.Put("INV_FLOW", info.InvoiceFlow);
            if (!string.IsNullOrEmpty(info.Customer))
            {
                ei.Put("CUST_CD", info.Customer);
                ei.Put("CUST_NM", GetPartyName(info.Customer, partyDT));
            }
            ei.Put("MODEL_NAME", info.ModelName);
            string cmp = GetCMP(info);
            if (!string.IsNullOrEmpty(cmp))
                ei.Put("CMP", cmp);
            ei.Put("STN", info.CompanyCode);
            if (!string.IsNullOrEmpty(info.Seller))
            {
                ei.Put("SELLER", info.Seller);
                ei.Put("SELLER_NM", GetPartyName(info.Seller, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Buyer1))
            {
                ei.Put("BUYER1", info.Buyer1);
                ei.Put("BUYER1_NM", GetPartyName(info.Buyer1, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Buyer2))
            {
                ei.Put("BUYER2", info.Buyer2);
                ei.Put("BUYER2_NM", GetPartyName(info.Buyer2, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Buyer3))
            {
                ei.Put("BUYER3", info.Buyer3);
                ei.Put("BUYER3_NM", GetPartyName(info.Buyer3, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Buyer4))
            {
                ei.Put("BUYER4", info.Buyer4);
                ei.Put("BUYER4_NM", GetPartyName(info.Buyer4, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Buyer5))
            {
                ei.Put("BUYER5", info.Buyer5);
                ei.Put("BUYER5_NM", GetPartyName(info.Buyer5, partyDT));
            }
            ei.Put("INCOTERM1", info.Incoterm1);
            ei.Put("INCOTERM2", info.Incoterm2);
            ei.Put("INCOTERM3", info.Incoterm3);
            ei.Put("INCOTERM4", info.Incoterm4);
            ei.Put("INCOTERM5", info.Incoterm5);

            ei.Put("POD", GetPort(info.PortOfDischarge, portDT));
            ei.Put("POD_NM", info.PortOfDischarge);
            if (!string.IsNullOrEmpty(info.Consignee))
            {
                ei.Put("CNEE_CD", info.Consignee);
                ei.Put("CNEE_NM", GetPartyName(info.Consignee, partyDT));
            }
            Tuple<string, string> trans = GetTransCode(info.ShippingMode, transDT);
            if (trans != null)
            {
                ei.Put("SHIPPING_MODE", trans.Item1);
                ei.Put("SHIPPING_NM", trans.Item2);
            }
            if (!string.IsNullOrEmpty(info.Notify1))
            {
                ei.Put("NOTIFY1", info.Notify1);
                ei.Put("NOTIFY1_NM", GetPartyName(info.Notify1, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Notify2))
            {
                ei.Put("NOTIFY2", info.Notify2);
                ei.Put("NOTIFY2_NM", GetPartyName(info.Notify2, partyDT));
            }
            if (!string.IsNullOrEmpty(info.Notify3))
            {
                ei.Put("NOTIFY3", info.Notify3);
                ei.Put("NOTIFY3_NM", GetPartyName(info.Notify3, partyDT));
            }
            return ei;
        }
    }
}