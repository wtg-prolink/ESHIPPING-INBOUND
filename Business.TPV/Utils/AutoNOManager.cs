using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.Utils
{
    public class AutoNOManager
    {
        public static string GetAutoNo(string ruleCode, string GroupId, string CompanyId)
        {
            System.Collections.Hashtable hash = new System.Collections.Hashtable();
            hash.Add(CompanyId, ruleCode);
            string ReserveNo = Prolink.AutoNo.GetNo(ruleCode, hash, GroupId, CompanyId, "*");
            ReserveNo = CompanyId + ReserveNo;
            return ReserveNo;
        }
    }
}