using Business.Import;
using Business.TPV.Base;
using Business.Utils;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.V6.Persistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.SFIS
{
    class ProductionLineInfoImport : ImportBase
    {
        public ProductionLineInfoImport(string location)
            : base(location)
        {
            this.AfterBackup += ProductionLineInfoImport_AfterBackup;
            this.OccuredError += ProductionLineInfoImport_OccuredError;
        }

        void ProductionLineInfoImport_OccuredError(object sender, FtpImportEvertArgs e)
        {
            SFISProdLineEDILog log = new SFISProdLineEDILog(e);
            Logger.WriteLog(log.CreateEx(e.EX));
        }

        void ProductionLineInfoImport_AfterBackup(object sender, FtpImportEvertArgs e)
        {
            SFISProdLineEDILog log = new SFISProdLineEDILog(e);
            Logger.WriteLog(log.CreateSucceed());
        }

        protected override ImportModes Mode
        {
            get { return ImportModes.ProductionLine; }
        }

        protected override bool OperateFile(FtpImportEvertArgs args)
        {
            var items = CreateSerialNumberInfo(CreateLines(args.LocalFileName)).ToList();
            args.Data = items;
            bool succed=Update(items, "SMDNP");
            if(succed){
                List<OrderInfo> infos = GetOrderInfo(items).ToList();
                string filePath = Manager.SendOrderNO(infos, Location);
                SendOrderNOEDILog log = new SendOrderNOEDILog(infos, filePath);
                Business.Utils.Context.Logger.WriteLog(log.CreateSucceed());
            }
            return succed;
        }

        IEnumerable<OrderInfo> GetOrderInfo(IEnumerable<ProductionLineInfo> dt)
        {
            if (dt != null)
            {
                foreach (ProductionLineInfo pfi in dt)
                {
                    OrderInfo info = new OrderInfo();
                    info.Number = Prolink.Math.GetValueAsString(pfi.OrderNumber);
                    info.Status = "1";
                    yield return info;
                }
            }
        }

        IEnumerable<ProductionLineInfo> CreateSerialNumberInfo(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string[] strs = line.Split(new string[] { "|" }, StringSplitOptions.None);
                if (strs.Length <= 5) continue;
                yield return new ProductionLineInfo
                {
                    OrderNumber = strs[0],
                    ProductLine = strs[1],
                    JobQTY = strs[2],
                    ProductQTY = strs[3],
                    InputQTY = strs[4],
                    WIP = strs[5]
                };
            }
        }

        EditInstruct CreateEi(ProductionLineInfo info)
        {
            EditInstruct ei = new EditInstruct("SMDNP", EditInstruct.UPDATE_OPERATION);
            ei.Condition = string.Format("JOB_NO={0}", SQLUtils.QuotedStr(info.OrderNumber));
            ei.Put("JQTY", info.JobQTY);
            ei.Put("PRODUCT_LINE", info.ProductLine);
            ei.Put("IQTY", info.InputQTY);
            ei.Put("PQTY", info.ProductQTY);
            ei.Put("WQTY", info.WIP);
            return ei;
        }

        DataTable QueryTable(List<ProductionLineInfo> items)
        {
            if (items == null || items.Count <= 0) return null;
            List<string> list = items.Select(x => x.OrderNumber).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (list == null || list.Count <= 0) return null;
            string sql = string.Format("SELECT U_ID,JOB_NO,JQTY,PRODUCT_LINE,IQTY,PQTY,WQTY FROM SMDNP WHERE {0} ",
                string.Join(" OR ", list.Select(x => string.Format("JOB_NO={0}", SQLUtils.QuotedStr(x)))));
            return DB.GetDataTable(sql, new string[] { });
        }

        DataTable CreateUpTable(List<ProductionLineInfo> items)
        {
            DataTable table = QueryTable(items);
            if (table == null || table.Rows.Count <= 0) return null;
            table.AcceptChanges();
            foreach (var item in items)
            {
                DataRow[] rows = table.Select(string.Format("JOB_NO={0}", SQLUtils.QuotedStr(item.OrderNumber)));
                if (rows == null || rows.Length <= 0) continue;
                foreach (var row in rows)
                {
                    row["JQTY"] = Prolink.Math.GetValueAsDecimal(item.JobQTY);
                    row["PRODUCT_LINE"] = Prolink.Math.GetValueAsString(item.ProductLine);
                    row["IQTY"] = Prolink.Math.GetValueAsDecimal(item.InputQTY);
                    row["PQTY"] = Prolink.Math.GetValueAsDecimal(item.ProductQTY);
                    row["WQTY"] = Prolink.Math.GetValueAsDecimal(item.WIP);
                }
            }
            return table;
        }

        bool Update(List<ProductionLineInfo> items, string commandText)
        {
            DataTable table = CreateUpTable(items);
            if (table == null) return true;    //return false
            return Update(table, "SELECT U_ID,JOB_NO,JQTY,PRODUCT_LINE,IQTY,PQTY,WQTY FROM SMDNP WHERE 1=0");
        }

        protected override EditInstructList CreateEiList(IEnumerable<string> lines, FtpImportEvertArgs args)
        {
            throw new NotImplementedException();
        }
    }


    class ProductionLineInfo
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderNumber { get; set; }
        /// <summary>
        /// 工单量
        /// </summary>
        public string JobQTY { get; set; }
        /// <summary>
        /// 生产线
        /// </summary>
        public string ProductLine { get; set; }
        /// <summary>
        /// 生产量
        /// </summary>
        public string ProductQTY { get; set; }
        /// <summary>
        /// 投入量
        /// </summary>
        public string InputQTY { get; set; }
        /// <summary>
        /// WIP
        /// </summary>
        public string WIP { get; set; }
    }
}
