using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Prolink.Data;
using Prolink.DataOperation;
using System.Security.Cryptography;
using System.Text;

namespace WebGui.View
{
    public partial class Status : System.Web.Mvc.ViewPage
    {
        public string MasterNo = "";
        public string HouseNo = "";
        public string GroupId = "";
        public string ShipmentId = "";
        public string jobno = "";
        public DataTable DetailDT = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            string dnNo = Request["dnNo"];
            //DetailDT = OperationUtils.GetDataTable(string.Format("SELECT U_ID,STS_DESCP,EVEN_DATE,LOCATION,EVEN_TMG,REMARK,CREATE_BY,CREATE_DATE,LOCATION_DESCP,CNTR_NO,SEQ_NO,STS_CD FROM TKBLST WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE DN_NO={0}) ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(dnNo)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (!string.IsNullOrEmpty(dnNo))
            {
                HouseNo = dnNo;
                string ip = Request["ip"];
                string time = Request["time"];
                string token = Request["token"];
                string hostIp = string.Empty;
                if (!string.IsNullOrEmpty(Request.UserHostAddress))
                    hostIp=Request.UserHostAddress.Replace(":", "").Replace(".", "");
                //time = DateTime.Now.ToString("yyyyMMddHHmmss");
                string passWord = "99aa3f48-6d59-492e-8c2f-3ff52f64a9c8";
                string token1 = MD5EncryptHash(dnNo + time + passWord);

                DateTime checkTime = Prolink.Utils.FormatUtils.ParseDateTime(time, "yyyyMMddHHmmss");
                TimeSpan ts = DateTime.Now - checkTime;
                if (ts.Minutes >= 0 && ts.Minutes<=10)
                {
                    if (token1.Equals(token))
                    {
                        DetailDT = OperationUtils.GetDataTable(string.Format("SELECT * FROM (SELECT SHIPMENT_ID,U_ID,STS_DESCP,EVEN_DATE,LOCATION,EVEN_TMG,REMARK,CREATE_BY,CREATE_DATE,LOCATION_DESCP,CNTR_NO,SEQ_NO,STS_CD FROM TKBLST WHERE SHIPMENT_ID IN (SELECT SHIPMENT_ID FROM SMSM WHERE DN_NO={0}))A OUTER APPLY (SELECT TOP 1 TRAN_TYPE,HOUSE_NO FROM TKBL BL WITH (NOLOCK) WHERE BL.U_ID = A.U_ID) B ORDER BY EVEN_DATE DESC", SQLUtils.QuotedStr(dnNo)), new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
                        SetDateString();
                    }
                }
                return;
            }

            HouseNo = Request["HouseNo"];
            GroupId = Request["GroupId"];
            string tranType = Request["TranType"];
            //HouseNo = "NGBWD008532";
            string conditions=string.Empty;
            if (!string.IsNullOrEmpty(HouseNo))
            {
                conditions += "AND HOUSE_NO=" + SQLUtils.QuotedStr(HouseNo);
            }

            if(!string.IsNullOrEmpty(tranType))
            {
                conditions+="AND TRAN_TYPE="+SQLUtils.QuotedStr(tranType);
            }
            if(!string.IsNullOrEmpty(GroupId))
            {
                conditions += "AND GROUP_ID=" + SQLUtils.QuotedStr(GroupId);
            }
            string shipment_id = Request["ShipmentId"];
            if (!string.IsNullOrEmpty(shipment_id))
            {
                conditions += "AND SHIPMENT_ID=" + SQLUtils.QuotedStr(shipment_id);
            }
            if (!string.IsNullOrEmpty(conditions))
                conditions = " WHERE 1=1 " + conditions;
            else
                conditions = " WHERE 1=0 ";
            string sql = "SELECT U_ID,HOUSE_NO,MASTER_NO,GROUP_ID,TRAN_TYPE,SHIPMENT_ID FROM TKBL";
            sql += conditions;
            DataTable blDt = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (blDt.Rows.Count > 0)
            {
                MasterNo = blDt.Rows[0]["MASTER_NO"].ToString();
                ShipmentId = blDt.Rows[0]["SHIPMENT_ID"].ToString();
                tranType = blDt.Rows[0]["TRAN_TYPE"].ToString();
                jobno = blDt.Rows[0]["U_ID"].ToString();
            }
            sql = "SELECT U_ID,STS_DESCP,EVEN_DATE,LOCATION,EVEN_TMG,REMARK,CREATE_BY,CREATE_DATE,LOCATION_DESCP,CNTR_NO,SEQ_NO,STS_CD FROM TKBLST WHERE U_ID=" + SQLUtils.QuotedStr(jobno);
            sql += " ORDER BY EVEN_DATE DESC";
            DetailDT = OperationUtils.GetDataTable(sql, new string[] { }, Prolink.Web.WebContext.GetInstance().GetConnection());
            SetDateString();
            if (DetailDT.Rows.Count==0)
            {
                DataRow newRow = DetailDT.NewRow();
                newRow["STS_DESCP"] = "&nbsp;";
                newRow["CNTR_NO"] = "&nbsp;";
                //newRow["EVEN_DATE"] = " ";
                newRow["LOCATION_DESCP"] = "&nbsp;";
                newRow["REMARK"] = "&nbsp;";
                DetailDT.Rows.Add(newRow);
            }
        }

        /// <summary>
        /// 获取抓取货况的参数
        /// </summary>
        /// <param name="dnNo">DN NO</param>
        /// <param name="passWord">授权密码</param>
        /// <returns></returns>
        public static string GetStatusTokenUrl(string dnNo,string passWord = "99aa3f48-6d59-492e-8c2f-3ff52f64a9c8")
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmmss");
            string token = MD5EncryptHash(dnNo + time + passWord);
            return string.Format("dnNo={0}&time={1}&token={2}", dnNo, time, token);
        }

        public static string MD5EncryptHash(String input)
        {
            //byte[] result = Encoding.UTF8.GetBytes(input);
            //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            //byte[] output = md5.ComputeHash(result);
            //string xx= BitConverter.ToString(output).Replace("-", "");

            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            Byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private void SetDateString()
        {
            DetailDT.Columns.Add("EVEN_DATE_STR", typeof(string));
            DetailDT.Columns["EVEN_DATE_STR"].MaxLength = 20;
            foreach (DataRow dr in DetailDT.Rows)
            {
                if (dr["EVEN_DATE"] == null || dr["EVEN_DATE"] == DBNull.Value)
                    continue;
                dr["EVEN_DATE_STR"] = ((DateTime)dr["EVEN_DATE"]).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}