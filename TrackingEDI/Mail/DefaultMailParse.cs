using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using Prolink.Web.Utils;

namespace TrackingEDI.Mail
{
    /// <summary>
    /// create by fish  2013-2-23
    /// mail模板格式化
    /// </summary>
    public class DefaultMailParse : IMailTemplateParse
    {
        static readonly string FieldRegex = @"{.*?}";//@"{=.*?}"
        /// <summary>
        /// 关键字符
        /// </summary>
        protected static readonly string[][] CHANGE_STR = new string[][]{
            new string[]{"=",""},
            new string[]{"{",""},
            new string[]{"}",""}
        };

        public string Parse(DataTable data, DataSet dataSet, string htmlTemp, Dictionary<string, string> map, string type)
        {
            return Parse(data, dataSet, htmlTemp, map, type, null);
        }

        public string Parse(DataTable data, DataSet dataSet, string htmlTemp, Dictionary<string, string> map, string type, Dictionary<string, Func<string,string,DataRow,string>> render)
        {
            if (data != null && data.Rows.Count <= 0)
                data.Rows.Add(data.NewRow());

            string[] htmlTemps = GetRegexResult(htmlTemp, "{=foreach\\([\\S\\s]*?foreach\\)}");//获取循环模板
            string[] fieldName = null;
            string newhtmltemp = string.Empty;
            if (htmlTemps != null)
            {
                int i = 0;
                DataTable childData = null;
                foreach (string temp in htmlTemps)
                {
                    if (dataSet.Tables.Count > i)
                        childData = dataSet.Tables[i];
                    newhtmltemp = getHtmlContent(temp);
                    fieldName = GetRegexResult(newhtmltemp, FieldRegex);
                    htmlTemp = htmlTemp.Replace(temp, FormatHtml(childData, temp.Substring(10, temp.Length - 19), fieldName, map,render));
                    i++;
                }
            }

            #region foreach循环体系
            htmlTemps = GetRegexResult(htmlTemp, "<foreach>[\\S\\s]*?</foreach>");//获取循环模板
            if (htmlTemps != null)
            {
                int i = 0;
                DataTable childData = null;
                foreach (string temp in htmlTemps)
                {
                    if (dataSet.Tables.Count > i)
                        childData = dataSet.Tables[i];
                    newhtmltemp = getHtmlContent(temp);
                    fieldName = GetRegexResult(newhtmltemp, FieldRegex);
                    htmlTemp = htmlTemp.Replace(temp, FormatHtml(childData, temp.Substring(9, temp.Length - 19), fieldName, map, render));
                    i++;
                }
            }
            #endregion
           
            #region foreach循环体系
            htmlTemps = GetRegexResult(htmlTemp, "<tr((?!<tr)[\\S\\s])*foreach=[\"|']Y[\"|']((?!>)[\\S\\s])*>[\\S\\s]*?</tr>");//获取循环模板
            if (htmlTemps != null)
            {
                int i = 0;
                DataTable childData = null;
                foreach (string temp in htmlTemps)
                {
                    if (dataSet.Tables.Count > i)
                        childData = dataSet.Tables[i];
                    newhtmltemp = getHtmlContent(temp);
                    fieldName = GetRegexResult(newhtmltemp, FieldRegex);
                    htmlTemp = htmlTemp.Replace(temp, FormatHtml(childData, temp, fieldName, map, render));
                    i++;
                }
            }
            #endregion

            newhtmltemp = getHtmlContent(htmlTemp);
            fieldName = GetRegexResult(newhtmltemp, FieldRegex);
            htmlTemp = FormatHtml(data, htmlTemp, fieldName, map,render);
            return htmlTemp;
        }
        public string getHtmlContent(string htmlTemp)
        {
            string newhtmlContent = string.Empty;
            Regex reg1 = new Regex("<style(.*?)</style>");
            Regex reg2 = new Regex("(<\\s*?style).*|\\s*(</style>)");
            var mc = reg1.Matches(htmlTemp);
            var mc2 = reg2.Matches(htmlTemp);
            if (mc.Count > 0)
            {
                newhtmlContent = reg1.Replace(htmlTemp, "");
            }
            else if (mc2.Count > 0)
            {
                newhtmlContent = reg2.Replace(htmlTemp, "");
            }
            else newhtmlContent = htmlTemp;
            return newhtmlContent;
        }

        public string Parse(DataTable data, DataTable childData, string htmlTemp, Dictionary<string, string> map)
        {
            return Parse(data, childData, htmlTemp, map,null);
        }

        /// <summary>
        /// 返回格式化后的html
        /// </summary>
        /// <param name="data"></param>
        /// <param name="childData"></param>
        /// <param name="htmlTemp"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public string Parse(DataTable data, DataTable childData, string htmlTemp, Dictionary<string, string> map, Dictionary<string, Func<string, string, DataRow, string>> render)
		{
            DataSet dataSet = new DataSet();
            if (childData == null)
                childData = new DataTable();
            dataSet.Tables.Add(childData);
            return Parse(data, dataSet, htmlTemp, map, "", render);
		}

        /// <summary>
        /// 格式化html模板
        /// </summary>
        /// <param name="data"></param>
        /// <param name="htmlTemp"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static string FormatHtml(DataTable data, string htmlTemp, string[] fieldName, Dictionary<string, string> map, Dictionary<string, Func<string,string,DataRow,string>> render)
        {
            string originalColName = String.Empty, colName = String.Empty, colValue = String.Empty;
            string copyHtml = String.Empty;//原始模板
            StringBuilder html = new StringBuilder();//返回后的组合模板
            foreach (DataRow dr in data.Rows)
            {
                copyHtml = htmlTemp;
                for (int i = 0; i < fieldName.Length; i++)
                {
                    colName = ProcessColName(fieldName[i]);
                    originalColName = ProcessOriginalColName(fieldName[i]);
                    if (originalColName.Contains("|") && originalColName.Contains("-"))//{=F.SHIP_BY|S-Sea|A-Air|E-Express} 
                    {
                        colValue = FormulaReplace(data, originalColName);
                    }
                    else if (originalColName.Contains("|") && originalColName.Contains(".")//{=L.TRAN_TYPE|S.MBL|A.MAWB} 
                       && colName.Contains("|") && colName.Contains("."))// |代表与非断言   是否的意思   .代表 成立条件      (F.TRAN_TYPE|S.A|A.B) 代表的含义是   当TRAN_TYPE为S时取A   当TRAN_TYPE为A时取B     其中F.的F代表为F取栏位值  L.代表取描述即可
                    {
                        colValue = FormulaIfValue(data, originalColName);
                    }
                    else if (colName.Contains(".SCALE."))//四舍五入  {=1.SCALE.GW}
                    {
                        string[] scale = colName.Split(new char[] { '.' });
                        if (GetValue(data, scale[2]) != null)
                        {
                            try
                            {
                                colValue = Round(Prolink.Math.GetValueAsFloat(GetValue(dr, scale[2])), int.Parse(scale[0]));
                            }
                            catch (Exception)
                            {
                                colValue = Prolink.Math.GetValueAsString(GetValue(data, scale[2]));
                            }
                        }
                        else
                            colValue = "";
                    }
                    else if (map != null && map.ContainsKey(colName))//附加映射值
                    {
                        colValue = map[colName];
                    }
                    else if (GetValue(dr, colName) != null)
                        colValue = FormatColValue(Prolink.Math.GetValueAsString(GetValue(dr, colName)), fieldName[i]);
                    else
                        colValue = "";
                    if (render != null && render.ContainsKey(colName))//附加映射值
                        colValue = render[colName](colName, colValue, dr);

                    if (!string.IsNullOrEmpty(colValue))
                        colValue = colValue.Replace(System.Environment.NewLine, "<br/>").Replace("\n", "<br/>");
                    copyHtml = copyHtml.Replace(fieldName[i], colValue);
                }
                html.Append(copyHtml);
            }
            return html.ToString();
        }

        /// <summary>
        /// 获取标准的栏位格式
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static string ProcessColName(string colName)
        {
            colName = colName.ToUpper();
            for (int i = 0; i < CHANGE_STR.Length; i++)
            {
                colName = colName.Replace(CHANGE_STR[i][0], CHANGE_STR[i][1]);
                if (colName.IndexOf("(") > -1)
                    colName = colName.Remove(colName.IndexOf("("), colName.LastIndexOf(")") - colName.IndexOf("(") + 1);
            }
            return colName;
        }

        /// <summary>
        /// 获取原始的栏位格式
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static string ProcessOriginalColName(string colName)
        {
            for (int i = 0; i < CHANGE_STR.Length; i++)
            {
                colName = colName.Replace(CHANGE_STR[i][0], CHANGE_STR[i][1]);
                if (colName.IndexOf("(") > -1)
                    colName = colName.Remove(colName.IndexOf("("), colName.LastIndexOf(")") - colName.IndexOf("(") + 1);
            }
            return colName;
        }

        /// <summary>
        /// 格式化栏位的值   {=ETD(D:yyyy/MM/dd)}
        /// </summary>
        /// <param name="colValue"></param>
        /// <param name="express"></param>
        /// <returns></returns>
		public static string FormatColValue(string colValue, string express)
		{
			if(express.IndexOf("(D:")!=-1) //D: 指时间格式
			{
				string format = Regex.Replace(express,".*?\\((.*?)\\)","$1");
				format = format.Replace("}","");
				colValue = FormatDateValue(colValue,format);
			}
			if(express.IndexOf("(P:")!=-1) //P: 分割值 如P:/,0；/指分割符，0指索引
			{
				string format = Regex.Replace(express,".*?\\((.*?)\\)","$1");
				format = format.Replace("}","");
				colValue = FormatSplitValue(colValue,format);
			}
			if(express.IndexOf("(H:")!=-1)
			{
                colValue = WebUtils.FormatStringForHTML(colValue);
			}
			return colValue;
		}

        /// <summary>
        /// 获取需要格式化的所有的栏位信息
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] GetRegexResult(string temp, string pattern)
		{
			MatchCollection mc = Regex.Matches(temp, pattern, RegexOptions.IgnoreCase);
			string[] result = new string[mc.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = mc[i].ToString();
			}
			return result;
		}

        /// <summary>
        /// 拆分栏位的关键字   组合格式化信息
        /// </summary>
        /// <param name="colValue"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static string FormatSplitValue(string colValue, string format)
		{
			if(colValue==null||colValue=="") return "";
			try
			{
				//“(P:/,0)” /指分割符，0指索引
				if(format.IndexOf("P:")==-1) return colValue;
				if(format.IndexOf(",")==-1) return colValue;
				string separator = format.Substring(format.IndexOf("P:")+2,1);
				
				int index =  int.Parse(format.Substring(format.IndexOf(",")+1,1));
				string[] list = colValue.Split(new char[]{char.Parse(separator)});
				colValue = list[index];
			}
			catch(Exception e){}

			return colValue;
		}

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="colValue"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string FormatDateValue(string colValue, string format)
		{	
			if(colValue==null||colValue=="")return "";
			try
			{
				//“(D: ... )”为日期格式
				if(format.IndexOf("D:")!=-1)
				{
					int Year = 0;
					int Month = 0;
					int Day = 0;
					int Hour = 0;
					int Minu = 0;
					int Second = 0;
					int len = colValue.Length;
					for(int i=0;i<14-len;i++)
					{
						colValue += "0";
					}

					Year = int.Parse(colValue.Substring(0,4));
					Month = int.Parse(colValue.Substring(4,2));
					Day = int.Parse(colValue.Substring(6,2));
					Hour = int.Parse(colValue.Substring(8,2));
					Minu = int.Parse(colValue.Substring(10,2));
					Second = int.Parse(colValue.Substring(12,2));

					format = format.Replace("D:","");
					
					DateTime dateTime = new DateTime(Year,Month,Day,Hour,Minu,Second);

					colValue = dateTime.ToString(format,System.Globalization.DateTimeFormatInfo.InvariantInfo);
				}
			}
			catch(Exception e){}

			return colValue.Replace("上午","AM").Replace("下午","PM");
		}

        /// <summary>
        /// 四色五入  返回字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string Round(double value, int decimals)
        {
            string str = "";
            if (value < 0)
            {
                value = System.Math.Round(value + 5 / System.Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                value = System.Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
            str = value.ToString();
            string[] scale = str.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            if (scale.Length > 1)
                i = scale[1].Length;
            if (decimals <= 0)
                str = scale[0];
            else
            {
                str = scale[0] + ".";
                if (scale.Length > 1)
                    str += scale[1];
            }

            for (; i < decimals; i++)
            {
                str += "0";
            }
            return str;
        }

        /// <summary>
        /// 栏位等价替换  例：{=F.SHIP_BY|S-Sea|A-Air|E-Express} 
        /// </summary>
        public static string FormulaReplace(DataTable data, string colName)
        {
            string colValue = "";
            string[] ifCondition = colName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] temps = null;
            string field = "";
            string caseStr = "";
            string caseValue = "";
            for (int index = 0; index < ifCondition.Length; index++)
            {
                caseStr = "";
                caseValue = "";
                temps = ifCondition[index].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                #region  获取默认值、条件、结果值
                if (temps.Length > 0)
                {
                    caseStr = temps[0];
                    if (index == 0)
                    {
                        field = temps[0];
                        if (GetValue(data, field) != null)
                            field =Prolink.Math.GetValueAsString(GetValue(data, field));
                        continue;
                    }
                }
                if (temps.Length > 1)
                {
                    caseValue = temps[1];
                }
                if (temps.Length < 2)
                {
                    caseValue = caseStr;
                    caseStr = "";
                }

                #endregion

                if (field == caseStr || caseStr == "")//caseStr为空时取默认值
                {
                    colValue = caseValue;
                    break;//结束断言
                }
            }
            return colValue;
        }

        /// <summary>
        /// 条件断言  |代表与非断言   是否的意思   .代表 成立条件      (F.TRAN_TYPE|S.A|A.B|C) 代表的含义是   当TRAN_TYPE为S时取A   当TRAN_TYPE为A时取B     其中F.的F代表为F取栏位值  L.代表取描述即可
        /// </summary>
        private static string FormulaIfValue(DataTable data, string colName)
        {
            string colValue="";
            string[] ifCondition = colName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] temps = null;
            string type = "";
            string field = "";
            string caseStr = "";
            string caseValue = "";
            for (int index = 0; index < ifCondition.Length; index++)
            {
                caseStr = "";
                caseValue = "";
                temps = ifCondition[index].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                #region  获取默认值、条件、结果值
                if (temps.Length > 0)
                {
                    caseStr = temps[0];
                    if (index == 0)
                        type = temps[0];
                }
                if (temps.Length > 1)
                {
                    caseValue = temps[1];
                    if (index == 0)
                    {
                        field = temps[1];
                        if (GetValue(data,field) != null)
                            field = Prolink.Math.GetValueAsString(GetValue(data, field));
                        continue;
                    }
                }
                if (temps.Length < 2)
                {
                    caseValue = caseStr;
                    caseStr = "";
                }

                #endregion

                if (field == caseStr || caseStr == "")//caseStr为空时取默认值
                {
                    switch (type)
                    {
                        case "L": colValue = caseValue; break;
                        case "F":

                            if (GetValue(data,caseValue) != null)
                                colValue =Prolink.Math.GetValueAsString(GetValue(data,caseValue));
                            else
                                colValue = "";
                            break;
                    }
                    break;//结束断言
                }
            }
            return colValue;
        }

        /// <summary>
        /// 获取DataTable中栏位的值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static object GetValue(DataTable data, string colName)
        {
            if (data == null || data.Rows.Count < 0 || !data.Columns.Contains(colName))
                return null;
            return data.Rows[0][colName];
        }

        /// <summary>
        /// 获取DataTable中栏位的值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static object GetValue(DataRow data, string colName)
        {
            if (data == null ||  !data.Table.Columns.Contains(colName))
                return null;
            return data[colName];
        }

        /// <summary>
        /// //获取模板主体内容
        /// </summary>
        /// <param name="htmlTemp"></param>
        /// <returns></returns>
        public string GetBody(string htmlTemp)
        {
            string[] htmlTemps = GetRegexResult(htmlTemp, "{=body\\([\\S\\s]*?\\)body}");//获取模板主体内容
            if (htmlTemps != null && htmlTemps.Length > 0)
                return htmlTemps[0];
            return string.Empty;
        }
    }	
}