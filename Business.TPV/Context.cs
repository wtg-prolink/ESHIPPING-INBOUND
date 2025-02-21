using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Business.Utils;
using Prolink.Data;
using System.Data;

namespace Business.TPV
{
    public class Context
    {
        public const string GroupId = "TPV";

        public static void Bulid()
        {
            Business.Utils.Context.SecurityHandler = new TPV.Utils.TPVSecurityHandler();
            Business.Utils.Context.Logger = new Utils.Logger();
            //SetAllowUnsafeHeaderParsing(true);
        }

        static bool SetAllowUnsafeHeaderParsing(bool useUnsafe)
        {
            //Get the assembly that contains the internal class
            System.Reflection.Assembly aNetAssembly = System.Reflection.Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        System.Reflection.FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, useUnsafe);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        static OrgService _orgService;

        public static OrgService OrgService
        {
            get
            {
                if (_orgService == null)
                {
                    _orgService = new OrgService();
                }
                return _orgService;
            }
        }

        static XmlDocument _sysColumnsDoc;
        public static XmlDocument GetSysColumnsDoc()
        {
            if (_sysColumnsDoc != null) return _sysColumnsDoc;
            string filePath = "edi/SysColumns.xml";
            filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, filePath);
            _sysColumnsDoc = new XmlDocument();
            _sysColumnsDoc.Load(filePath);
            return _sysColumnsDoc;
        }

        public static XmlDocument GetSecurityDoc()
        {
            string filePath = "edi/Security.xml";
            filePath = System.IO.Path.Combine(Business.Utils.Context.XmlStorePath, filePath);
            if (!File.Exists(filePath)) throw new Exception("程式未配置！");
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            return doc;
        }

        public static EDIConfig GetEDIConfigFromList(string custcd, string EdiId)
        {
            string sql = string.Format("SELECT TOP(1) U_ID FROM EDI_TARGET WHERE CUST_CD = {0} AND CMP = 'FQ' AND GROUP_ID = 'TPV'", SQLUtils.QuotedStr(custcd));
            string uid = DBManager.DefaultDB.GetValueAsString(sql);
            if (string.IsNullOrEmpty(uid)) return null;
            sql = string.Format("SELECT * FROM EDI_LIST WHERE U_FID = {0} AND EDI_ID = {1}", SQLUtils.QuotedStr(uid), SQLUtils.QuotedStr(EdiId));
            DataTable dt = DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt.Rows.Count <= 0) return null;

            return new EDIConfig
            {
                RecieveCode = custcd,
                SenderCode = "ESP",
                PartyNO = custcd,
                Psw = Prolink.Math.GetValueAsString(dt.Rows[0]["SOURCE_PWD"]),
                Server = Prolink.Math.GetValueAsString(dt.Rows[0]["SERVICE_PATH"]),
                User = Prolink.Math.GetValueAsString(dt.Rows[0]["SOURCE_ACCOUNT"]),
                Authorization = Prolink.Math.GetValueAsString(dt.Rows[0]["EDI_NAMESPACE"]),
                Remark = Prolink.Math.GetValueAsString(dt.Rows[0]["REMARK"]),
                FunctionCode = Prolink.Math.GetValueAsString(dt.Rows[0]["EDI_NAMESPACE"])
            };
        }

        public static EDIConfig GetEDIConfig(string partyNo, string location = "", string msgCode = "")
        {
            string sql = string.Format("SELECT * FROM SMEXM WHERE EXPRESS={0}", SQLUtils.QuotedStr(partyNo));
            DataTable dt = DBManager.DefaultDB.GetDataTable(sql, new string[] { });
            if (dt == null || dt.Rows.Count <= 0) return null;
            DataRow row = dt.Rows[0];
            if (!string.IsNullOrEmpty(location))
            {
                List<ConditionItem> items = new List<ConditionItem>();
                items.Add(new ConditionItem("CMP", location));
                items.Add(new ConditionItem("MSG_CODE", msgCode));
                string condition = DBManager.CreateCondition(items, true);
                DataRow[] rows = dt.Select(condition);
                if (rows.Length > 0)
                    row = rows[0];
            }
            return new EDIConfig
            {
                MsgCode = Prolink.Math.GetValueAsString(row["MSG_CODE"]),
                RecieveCode = Prolink.Math.GetValueAsString(row["RCV_ID"]),
                SenderCode = Prolink.Math.GetValueAsString(row["SEND_ID"]),
                Cmp = Prolink.Math.GetValueAsString(row["CMP"]),
                CmpName = Prolink.Math.GetValueAsString(row["CMP_NM"]),
                PartyNO = Prolink.Math.GetValueAsString(row["EXPRESS"]),
                Psw = Prolink.Math.GetValueAsString(row["PW_ID"]),
                Remark = Prolink.Math.GetValueAsString(row["REMARK"]),
                Server = Prolink.Math.GetValueAsString(row["WEB_URL"]),
                User = Prolink.Math.GetValueAsString(row["EX_NO"]),
                FunctionCode = Prolink.Math.GetValueAsString(row["EDI_MODE"]),
                Authorization = Prolink.Math.GetValueAsString(row["AUTHORI_ZATION"])
            };
        }
    }

    public class EDIConfig
    {
        public string Cmp { get; set; }
        public string CmpName { get; set; }
        public string PartyNO { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public string Psw { get; set; }
        public string SenderCode { get; set; }
        public string RecieveCode { get; set; }
        public string MsgCode { get; set; }
        public string Remark { get; set; }
        public string FunctionCode { get; set; }
        public string Authorization { get; set; }
    }


    public enum FactoryCode
    {
        FQ
    }

    public static class RFCExtension
    {
        public static string GetFieldValueAsString(this IRfcStructure structure, string field)
        {
            if (structure.Metadata != null && structure.Metadata.TryNameToIndex(field) < 0) return null;
            IRfcField rfcField = structure[field];
            if (rfcField == null) return null;
            return rfcField.GetString();
        }
    }

    public class Runtime
    {
        [Required]
        public string ShipmentID { get; set; }
        public string Location { get; set; }
        [Required]
        public string PartyNo { get; set; }
        public OperationModes OperationMode { get; set; }
        public string OPUser { get; set; }
        public string RefNo { get; set; }
        public string GetOperationModeCode()
        {
            switch (OperationMode)
            {
                case OperationModes.Add: return "A";
                case OperationModes.Modify: return "M";
                case OperationModes.Cancel: return "C";
            }
            return null;
        }

        public object Data { get; set; }
    }

    public enum OperationModes { Add, Modify, Cancel }
}
