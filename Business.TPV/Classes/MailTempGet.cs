using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.TPV.Classes
{
    public class MailTempGet
    {
        public string GroupId = "";
        public string Station = "";
        public string Company = "";
        public string Dep = "";
        public string MailType = "";

        public MailTempGet(string groupid,string station,string dep,string company,string mailtype)
        {
            GroupId=groupid;
            Station = station;
            Company = company;
            Dep = dep;
            MailType = mailtype;
        }

        public string GetMailTemp()
        {
            string sql = string.Format("SELECT MT_CONTENT FROM TKPMT WHERE GROUP_ID={0} AND STN={1} AND DEP={2} AND CMP={3} AND MT_TYPE={4}",
                SQLUtils.QuotedStr(GroupId), SQLUtils.QuotedStr(Station), SQLUtils.QuotedStr(Dep), SQLUtils.QuotedStr(Company), SQLUtils.QuotedStr(MailType));
            return OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
        }

        public string Parse(DataTable dt)
        {
            if (dt.Rows.Count == 0) return "";

            HierarchicalMap[] listData = HierarchicalMap.CreateHierarchicalMap(dt, typeof(HierarchicalMap), false);
            HierarchicalMap siData = new HierarchicalMap();
            if (listData != null && listData.Length > 0)
            {
                siData = (HierarchicalMap)listData[0];
            }
            else
            {
                return "";
            }
            string htmlTemp = GetMailTemp();

            string colName = "";
            string colValue = "";
            string[] FieldName = GetRegexResult(htmlTemp, @"{.*?}");
            for (int i = 0; i < FieldName.Length; i++)
            {
                colName = ProcessColName(FieldName[i]);
                if (colName.StartsWith("URL_"))
                {
                    colName = colName.Replace("URL_", "");
                    if (siData.GetNode(colName) != null)
                    {
                        colValue = FormatColValue(siData.GetNode(colName).GetValueAsString(), FieldName[i]);
                        colValue = colValue.Replace("%", "%25");// % 指定特殊字符 %25 
                        colValue = colValue.Replace("&", "%26");//& URL 中指定的参数间的分隔符 %26 
                        colValue = colValue.Replace("+", "%2B");// + URL 中+号表示空格 %2B 
                        colValue = colValue.Replace("?", "%3F");// ? 分隔实际的 URL 和参数 %3F 
                        colValue = colValue.Replace("#", "%23");// # 表示书签 %23 
                    }
                }

                else
                    colValue = FormatColValue(siData.GetNode(colName).GetValueAsString(), FieldName[i]);
                htmlTemp = htmlTemp.Replace(FieldName[i], colValue);
            }
            return htmlTemp;

        }

        private string[] GetRegexResult(string temp, string pattern)
        {
            MatchCollection mc = Regex.Matches(temp, pattern, RegexOptions.IgnoreCase);
            string[] result = new string[mc.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = mc[i].ToString();
            }
            return result;
        }

        private string ProcessColName(string colName)
        {
            string[][] changeStr = new string[][]{new string[]{"=",""},
											new string[]{"{",""},
											new string[]{"}",""}
											};
            colName = colName.ToUpper();
            for (int i = 0; i < changeStr.Length; i++)
            {
                colName = colName.Replace(changeStr[i][0], changeStr[i][1]);
                if (colName.IndexOf("(") > -1)
                    colName = colName.Remove(colName.IndexOf("("), colName.LastIndexOf(")") - colName.IndexOf("(") + 1);
            }
            return colName;
        }

        private string FormatColValue(string colValue, string express)
        {
            if (express.IndexOf("(D:") != -1) //D: 指时间格式
            {
                string format = Regex.Replace(express, ".*?\\((.*?)\\)", "$1");
                format = format.Replace("}", "");
                colValue = formatDateValue(colValue, format);
            }
            if (express.IndexOf("(P:") != -1) //P: 分割值 如P:/,0；/指分割符，0指索引
            {
                string format = Regex.Replace(express, ".*?\\((.*?)\\)", "$1");
                format = format.Replace("}", "");
                colValue = formatSplitValue(colValue, format);
            }
            if (express.IndexOf("(H:") != -1)
            {
                colValue = Prolink.Web.Utils.WebUtils.FormatStringForHTML(colValue);
            }
            return colValue;
        }

        private string formatDateValue(string colValue, string format)
        {
            if (colValue == null || colValue == "") return "";
            try
            {
                //“(D: ... )”为日期格式
                if (format.IndexOf("D:") != -1)
                {
                    int Year = 0;
                    int Month = 0;
                    int Day = 0;
                    int Hour = 0;
                    int Minu = 0;
                    int Second = 0;
                    int len = colValue.Length;
                    for (int i = 0; i < 14 - len; i++)
                    {
                        colValue += "0";
                    }

                    Year = int.Parse(colValue.Substring(0, 4));
                    Month = int.Parse(colValue.Substring(4, 2));
                    Day = int.Parse(colValue.Substring(6, 2));
                    Hour = int.Parse(colValue.Substring(8, 2));
                    Minu = int.Parse(colValue.Substring(10, 2));
                    Second = int.Parse(colValue.Substring(12, 2));

                    format = format.Replace("D:", "");

                    DateTime dateTime = new DateTime(Year, Month, Day, Hour, Minu, Second);

                    colValue = dateTime.ToString(format, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                }
            }
            catch (Exception e) { }

            return colValue;
        }

        private string formatSplitValue(string colValue, string format)
        {
            if (colValue == null || colValue == "") return "";
            try
            {
                //“(P:/,0)” /指分割符，0指索引
                if (format.IndexOf("P:") == -1) return colValue;
                if (format.IndexOf(",") == -1) return colValue;
                string separator = format.Substring(format.IndexOf("P:") + 2, 1);

                int index = int.Parse(format.Substring(format.IndexOf(",") + 1, 1));
                string[] list = colValue.Split(new char[] { char.Parse(separator) });
                colValue = list[index];
            }
            catch (Exception e) { }

            return colValue;
        }
    }
}
