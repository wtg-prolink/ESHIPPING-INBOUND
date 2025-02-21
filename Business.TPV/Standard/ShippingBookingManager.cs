using Business.Service;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public abstract class ShippingBookingManager<T> : BookingManager<T, ShippingBookingRuntime<T>> where T : ShippingBookingResponse
    {
        protected const string BaseCode_Carrier = "TCAR";
        protected override EditInstruct CreateEi(ShippingBookingRuntime<T> runtime)
        {
            EditInstruct ei = base.CreateEi(runtime);
            ei.Put("CARRIER", runtime.Info.CarrierID);
            ei.Put("CARRIER_NM", runtime.GetBaseCodeNM(BaseCode_Carrier, runtime.Info.CarrierID));
            ei.Put("HOUSE_NO", runtime.Info.HouserNO);
            ei.Put("MASTER_NO", runtime.Info.MasterNO);
            ei.Put("SO_NO", runtime.Info.BookingNO);
            return ei;
        }
        DataTable QueryPartyDT(IEnumerable<T> infos)
        {
            List<string> codes = infos.Select(item => item.CarrierID).ToList();
            if (codes.Count <= 0) return null;
            string sql = string.Format("SELECT PARTY_NO,PARTY_NAME FROM SMPTY WHERE PARTY_NO IN({0})",
                string.Join(",", codes.Select(p => SQLUtils.QuotedStr(p))));
            return DB.GetDataTable(sql, new string[] { });
        }
        protected abstract void FillPort(IEnumerable<T> infos, Action<string> addPort);
        DataTable QueryPortDT(IEnumerable<T> infos)
        {
            List<string> codes = new List<string>();
            Action<string> add = code =>
            {
                if (!codes.Contains(code))
                    codes.Add(code);
            };
            FillPort(infos, add);
            if (codes.Count <= 0) return null;
            string sql = string.Format("SELECT PORT_CD,CNTRY_CD,PORT_NM FROM BSCITY WHERE CNTRY_CD+PORT_CD IN({0})",
                string.Join(",", codes.Select(p => SQLUtils.QuotedStr(p))));
            return DB.GetDataTable(sql, new string[] { });
        }

        protected override dynamic BeforeCreateRuntime(List<T> infos)
        {
            return new
            {
                partyDT = QueryPartyDT(infos),
                portDT = QueryPortDT(infos),
                tvstDT = QueryBaseCodeDT(infos)
            };
        }

        protected virtual DataTable QueryBaseCodeDT(List<T> infos)
        {
            return null;
        }

        protected override ShippingBookingRuntime<T> CreateRuntime(T info, dynamic runtimeObj)
        {
            return new ShippingBookingRuntime<T>
            {
                Info = info,
                PartyDT = runtimeObj.partyDT,
                PortDT = runtimeObj.portDT,
                BaseCodeDT = runtimeObj.tvstDT,
            };
        }
    }

    public class ShippingBookingRuntime<T> : BookingRuntime<T> where T : ShippingBookingResponse
    {
        public ShippingBookingRuntime()
        {
            Func<string, string, string> getPortInfo = (code, column) =>
            {
                if (PortDT == null) return null;
                if (string.IsNullOrEmpty(code)) return null;
                Tuple<string, string> port = Helper.GetPortCode(code);
                if (port == null) return null;
                DataRow[] rows = PortDT.Select(string.Format("CNTRY_CD={0} AND PORT_CD={1}", SQLUtils.QuotedStr(port.Item1), SQLUtils.QuotedStr(port.Item2)));
                if (rows == null || rows.Length <= 0) return null;
                return Prolink.Math.GetValueAsString(rows[0][column]);
            };
            GetPortNM = code =>
            {
                return getPortInfo(code, "PORT_NM");
            };
            GetPartyNM = code =>
            {
                if (PartyDT == null) return null;
                DataRow[] rows = PartyDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(code)));
                if (rows == null || rows.Length <= 0) return null;
                return Prolink.Math.GetValueAsString(rows[0]["PARTY_NAME"]);
            };
            GetBaseCodeNM = (type, code) =>
                {
                    if (BaseCodeDT == null) return null;
                    DataRow[] rows = BaseCodeDT.Select(string.Format("CD_TYPE={0} AND CD={1}", SQLUtils.QuotedStr(type), SQLUtils.QuotedStr(code)));
                    if (rows == null || rows.Length <= 0) return null;
                    return Prolink.Math.GetValueAsString(rows[0]["CD_DESCP"]);
                };
        }
        public DataTable PartyDT { get; set; }
        public DataTable PortDT { get; set; }
        public DataTable BaseCodeDT { get; set; }

        public Func<string, string, string> GetBaseCodeNM;
        public Func<string, string> GetPortNM;
        public Func<string, string> GetPartyNM;
    }
}
