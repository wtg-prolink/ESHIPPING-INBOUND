using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using TrackingEDI.Utils;

namespace Business
{
    public class Multilanguagecontrol
    {
        public static string baidu_appid = "20161223000034570";
        public static string baidu_key = "Wekh67n52j5BqmKEBG_w";
        private static System.Resources.ResourceManager rm = Resources.Lang.ResourceManager;
        Regex chiness = new Regex("[\u4e00-\u9fa5]");
        public void ChangeFileTxt(string filePath, string diffpath, string strExt)
        {
            WebGui.Controllers.BaseController basecontroller = new WebGui.Controllers.BaseController();
            DataTable dt = basecontroller.ImportExcelToDataTable(diffpath, strExt, 0);
            var FileNames = Business.Multilanguagecontrol.GetDirectoryFileName(diffpath, false);
            foreach (string name in FileNames)
            {
                string txt = string.Empty;
                if (!string.IsNullOrEmpty(name))
                    txt = File.ReadAllText(name, System.Text.Encoding.UTF8);
            }
            //if (dt != null && dt.Rows.Count > 0)
            //{
                
            //    foreach (DataRow dr in dt.Rows)
            //    {
            //        string langid = Prolink.Math.GetValueAsString(dr[0]);
            //        if (string.IsNullOrEmpty(langid)) continue;
            //        string val = Prolink.Math.GetValueAsString(dr[3]);
            //        rrw.AddResource(langid.Trim(), val.Trim());
            //    }
            //    rrw.Close();
            //}
        }
        public void AddResource(string strResxPath, string diffpath, string strExt)
        {
            WebGui.Controllers.BaseController basecontroller = new WebGui.Controllers.BaseController();
            DataTable dt = basecontroller.ImportExcelToDataTable(diffpath, strExt, 0);
            var _dr = dt.AsEnumerable().Distinct(new DataTableRowCompare("languageId"));
            DataTable _dt = dt.Clone();
            if (_dr != null && _dr.Count() > 0)
            {
                _dt = _dr.CopyToDataTable();
            }
            if (_dt != null && _dt.Rows.Count > 0)
            {
                AssemblyName[] referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                ResXResourceReader rsxResource = new ResXResourceReader(strResxPath);
                rsxResource.UseResXDataNodes = true;
                IDictionaryEnumerator enumerator = rsxResource.GetEnumerator();
                ResXResourceWriter rrw = new ResXResourceWriter(strResxPath);
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry)enumerator.Current;
                    ResXDataNode node = (ResXDataNode)current.Value;
                    rrw.AddResource(node);
                    //string strKey = node.Name;    //资源项名  
                    //object strValue = node.GetValue(referencedAssemblies);  //值  
                    //string strComment = node.Comment;   //注释  
                    //rrw.AddResource(strKey, strValue);
                }

                foreach (DataRow dr in _dt.Rows)
                {
                    string langid = Prolink.Math.GetValueAsString(dr[0]);
                    if (string.IsNullOrEmpty(langid)) continue;
                    string val = Prolink.Math.GetValueAsString(dr[3]);
                    rrw.AddResource(langid.Trim(), val.Trim());
                }
                rrw.Close();
            }
        }
        public class DataTableRowCompare : IEqualityComparer<DataRow>
        {

            #region IEqualityComparer<DataRow> 成员
            public DataTableRowCompare(string filedname)
            {
                _fieldname = filedname;
            }
            private string _fieldname = "languageId";
            public string fieldName
            {
                get { return _fieldname; }
                set { _fieldname = value; }
            } 
            public bool Equals(DataRow x, DataRow y)
            {

                var result = (x.Field<string>(fieldName) == y.Field<string>(fieldName));
                return result;
            }

            public int GetHashCode(DataRow obj)
            {
                return obj.ToString().GetHashCode();
            }

            #endregion
        }
        public void setlang(string strResxPath,string type,string fromtype="")
        {
            //string languageId = string.Empty;
            //string title = string.Empty;
            //ResXResourceWriter rrw = new ResXResourceWriter(@"F:\a.resx");
            //rrw.AddResource("obj1", "abcd");
            //rrw.Close();
            //string strResxPath = string.Empty;
            AssemblyName[] referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            ResXResourceReader rsxResource = new ResXResourceReader(strResxPath);
            rsxResource.UseResXDataNodes = true;
            IDictionaryEnumerator enumerator = rsxResource.GetEnumerator();
            ResXResourceWriter rrw = new ResXResourceWriter(strResxPath);
            while (enumerator.MoveNext())
            {
                DictionaryEntry current = (DictionaryEntry)enumerator.Current;
                ResXDataNode node = (ResXDataNode)current.Value;
                string strKey = node.Name;    //资源项名  
                object strValue = node.GetValue(referencedAssemblies);  //值  
                string strComment = node.Comment;   //注释  
                //strValue = "like";
                string val = GetLangTrans(strValue, type, baidu_appid, baidu_key, fromtype);
                rrw.AddResource(strKey, val);
            }
            rrw.Close();
        }
        //参数
        //url:http://fanyi.baidu.com/translate?;
        //from:zh
        //to:en
        //query:枚举
        //transtype:translang
        //simple_means_flag:3



        public string GetLangTrans(object oval, string lang, string appid, string key,string fromtype)
        {

            ////string url = "http://fanyi.baidu.com/v2transapi";


            //string result=WebclientSent(url, postString);
            //string jsonpost ="{\"from\":\"zh\",\"to\":\""+langtype+"\"query\":\"枚举\",\"transtype\":\"translang\",\"simple_means_flag\":\"3\"}";
            //Random random=new Random();
            //string test=DateTime.Now.ToFileTime().ToString();
            //string UTF8sign = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(sign));
            //MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //string md5Pwd = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(sign)), 4, 8);
            //md5Pwd = md5Pwd.Replace("-", "");
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            string langtype = lang;
            string postString = "from=zh&to=" + langtype + "&query=@Resources.Locale.L_Multilanguagecontrol_Business_81";//这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
            string salt = DateTime.Now.Ticks.ToString();
            string sign = appid + oval.ToString() + salt + key;
            string md5sign = MD5Hashing.HashString(sign);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("q", LanChangeGB2312ToUTF8(oval.ToString()));
            data.Add("from", string.IsNullOrEmpty(fromtype) ? "auto" : fromtype);
            data.Add("to", langtype);
            data.Add("appid", appid);
            data.Add("salt", salt);
            data.Add("sign", md5sign);
            postString = DicTransToJson(data);
            postString = DicTransToParm(data);
            string result = doPostMethodToObj(url, postString);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var resultlist= jss.Deserialize<Dictionary<string, object>>(result);
            string tranvalue = oval.ToString();
            foreach (KeyValuePair<string, object> item in resultlist)
            {
                if (item.Key.ToString() == "trans_result")//获取header数据
                {
                    var items =(ArrayList) item.Value;
                    foreach (var n in items)
                    {
                        var subItem = (Dictionary<string, object>)n;
                        if (subItem.Keys.Contains("dst"))
                        {
                            tranvalue = Prolink.Math.GetValueAsString(subItem["dst"]);
                        }
                    }
                }
            }
            //if (resultlist.Keys.Contains("trans_result"))
            //{

            //    var m = resultlist["trans_result"];
            //    var subItem = (Dictionary<string, object>)m;
            //    tranvalue = Prolink.Math.GetValueAsString(subItem["dst"]);
            //}
            //else
            //{
            //    tranvalue = Prolink.Math.GetValueAsString(oval); 
            //}
            return tranvalue;
        }
        public string GetLangTranslate(object oval, string lang, string appid, string key, string fromtype)
        {
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            string langtype = lang;
            string postString = "from=zh&to=" + langtype + "&query=@Resources.Locale.L_Multilanguagecontrol_Business_81";//这里即为传递的参数，可以用工具抓包分析，也可以自己分析，主要是form里面每一个name都要加进来  
            string salt = DateTime.Now.Ticks.ToString();
            string sign = appid + oval.ToString() + salt + key;
            string md5sign = MD5Hashing.HashString(sign);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("q", LanChangeGB2312ToUTF8(oval.ToString()));
            data.Add("from", string.IsNullOrEmpty(fromtype) ? "auto" : fromtype);
            data.Add("to", langtype);
            data.Add("appid", appid);
            data.Add("salt", salt);
            data.Add("sign", md5sign);
            postString = DicTransToJson(data);
            postString = DicTransToParm(data);
            string result = doPostMethodToObj(url, postString);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            var resultlist = jss.Deserialize<Dictionary<string, object>>(result);
            string tranvalue = oval.ToString();
            foreach (KeyValuePair<string, object> item in resultlist)
            {
                if (item.Key.ToString() == "trans_result")//获取header数据
                {
                    var items = (ArrayList)item.Value;
                    foreach (var n in items)
                    {
                        var subItem = (Dictionary<string, object>)n;
                        if (subItem.Keys.Contains("dst"))
                        {
                            tranvalue = Prolink.Math.GetValueAsString(subItem["dst"]);
                        }
                    }
                }
            }
            return tranvalue;
        }
        public static string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
                pwd = pwd + s[i].ToString("x2");

            }
            return pwd;
        }
        
        public string WebclientSent(string url, string parm, string method = "POST")
        {
            byte[] postData = Encoding.UTF8.GetBytes(parm);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
            byte[] responseData = webClient.UploadData(url, method, postData);//得到返回字符流  
            string srcString = Encoding.UTF8.GetString(responseData);//解码  
            return srcString;
        }
        public string DicTransToJson(Dictionary<string,object> data)
        {
            string json = (new JavaScriptSerializer()).Serialize(data);
            return json;
        }
        public string DicTransToParm(Dictionary<string, object> data)
        {
            string str = string.Empty;
            foreach (var item in data)
            {
                string key = item.Key;
                object value = item.Value;
                str = string.IsNullOrEmpty(str) ? "" : str + "&";
                str += key + "=" + value;
            }
            return str;
        }
        public string Utf8Str(string str)
        {
            //str=BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes("大西瓜"));
            //string str1 = '%' + str.Replace('-', '%');
            //byte[] c=Encoding.GetEncoding("gb2312").GetBytes(str);

            string s = System.Web.HttpUtility.UrlEncode(str, System.Text.Encoding.UTF8);
            //byte[] b = System.Text.Encoding.Default.GetBytes(str);
            //str =Encoding.UTF8.GetString(b);
            return s;
        }
        string LanChangeGB2312ToUTF8(string str)
        {
            if (chiness.Matches(str).Count > 0)
            {
                Encoding utf8;
                Encoding gb2312;
                utf8 = Encoding.GetEncoding("UTF-8");
                gb2312 = Encoding.GetEncoding("GB2312");
                byte[] gb = gb2312.GetBytes(str);
                gb = Encoding.Convert(gb2312, utf8, gb);
                return utf8.GetString(gb);
            }
            else
                return str;
        }
        string LanChangeUTF8ToGB2312(string text)
        {
            byte[] bs = Encoding.GetEncoding("UTF-8").GetBytes(text);
            bs = Encoding.Convert(Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("GB2312"), bs);
            return Encoding.GetEncoding("GB2312").GetString(bs);
        }  
        public static string UnicodeStr(string str)
        {
            //byte[] b = System.Text.Encoding.Default.GetBytes(str);
            //str = Encoding.Unicode.GetString(b);
            str = System.Web.HttpUtility.UrlEncode(str, System.Text.Encoding.Unicode);
            return str;
        }
        /// <summary>
        /// httprequest获取返回结果
        /// </summary>
        /// <param name="metodUrl">url</param>
        /// <param name="jsonBody">条件</param>
        /// <returns></returns>
        public static string doPostMethodToObj(string metodUrl, string parm)
        {
            //string url="http://api.fanyi.baidu.com/api/trans/vip/translate?q=apple&from=en&to=zh&appid=2015063000000001&salt=1435660288&sign=f89f9594663708c1605f3d736d01d2d4";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(metodUrl + parm);//metodUrl + parm
            request.Method = "post";
            request.ContentType = "application/json;charset=UTF-8";
            //request.ContentType = "application/x-www-form-urlencoded";
            
            var stream = request.GetRequestStream();
            using (var writer = new StreamWriter(stream))
            {
                //var requestbody = "";
                //var order = new Order();
                //order.Id = "aaaaa";
                //order.DocEntry = "bbbb";
                //order.FeedNum = "cccc";
                //jsonBody = JsonConvert.SerializeObject(order);

                writer.Write(parm);
                writer.Flush();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json = getResponseString(response);
            return json;
        }
        private string TransStr(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            Stream dd = writer.BaseStream;
            using (StreamReader reader = new StreamReader(dd, System.Text.Encoding.Unicode))
            {
                str = reader.ReadToEnd();
            }
            return str;
        }
        private string TransUrlStr(string str)
        {
            string val = HttpServerUtility.UrlTokenEncode(HttpServerUtility.UrlTokenDecode(str));

            return val;
        }
        private static string getResponseString(HttpWebResponse response)
        {
            string json = null;
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("UTF-8")))
            {
                json = reader.ReadToEnd();
            }
            return json;
        }
        //'zh': '中文','jp': '日语','jpka': '日语假名','th': '泰语','fra': '法语','en': '英语','spa': '西班牙语','kor': '韩语','tr': '土耳其语','vie': '越南语','ms': '马来语','de': '德语','ru': '俄语','ir': '伊朗语','ara': '阿拉伯语','est': '爱沙尼亚语','be': '白俄罗斯语','bul': '保加利亚语','hi': '印地语','is': '冰岛语','pl': '波兰语','fa': '波斯语','dan': '丹麦语','tl': '菲律宾语','fin': '芬兰语','nl': '荷兰语','ca': '加泰罗尼亚语','cs': '捷克语','hr': '克罗地亚语','lv': '拉脱维亚语','lt': '立陶宛语','rom': '罗马尼亚语','af': '南非语','no': '挪威语','pt_BR': '巴西语','pt': '葡萄牙语','swe': '瑞典语','sr': '塞尔维亚语','eo': '世界语','sk': '斯洛伐克语','slo': '斯洛文尼亚语','sw': '斯瓦希里语','uk': '乌克兰语','iw': '希伯来语','el': '希腊语','hu': '匈牙利语','hy': '亚美尼亚语','it': '意大利语','id': '印尼语','sq': '阿尔巴尼亚语','am': '阿姆哈拉语','as': '阿萨姆语','az': '阿塞拜疆语','eu': '巴斯克语','bn': '孟加拉语','bs': '波斯尼亚语','gl': '加利西亚语','ka': '格鲁吉亚语','gu': '古吉拉特语','ha': '豪萨语','ig': '伊博语','iu': '因纽特语','ga': '爱尔兰语','zu': '祖鲁语','kn': '卡纳达语','kk': '哈萨克语','ky': '吉尔吉斯语','lb': '卢森堡语','mk': '马其顿语','mt': '马耳他语','mi': '毛利语','mr': '马拉提语','ne': '尼泊尔语','or': '奥利亚语','pa': '旁遮普语','qu': '凯楚亚语','tn': '塞茨瓦纳语','si': '僧加罗语','ta': '泰米尔语','tt': '塔塔尔语','te': '泰卢固语','ur': '乌尔都语','uz': '乌兹别克语','cy': '威尔士语','yo': '约鲁巴语','yue': '粤语','wyw': '文言文','cht': '中文繁体'    },
        public enum langytpe
        {
            zh,
            en 
        }
        public static string CheckResourceHave(string cultureKey, string path, string diffpath, string savepath)
        {
            //SetCurrentUICulture(cultureKey);
            var n = diffpath.Split('.');
            string strExt = string.Empty;
            if (n != null && n.Length > 1)
            {
                strExt = n[n.Length - 1];
            }
            WebGui.Controllers.BaseController basecontroller = new WebGui.Controllers.BaseController();
            DataTable dt = basecontroller.ImportExcelToDataTable(diffpath, strExt, 0);
            //DataTable dt = Business.XExcelHelper.ImportExcelToDataTable(diffpath, strExt, 0);
            DataTable diff = dt.Clone();
            diff.Columns.Add("mark");
            foreach (DataRow dr in dt.Rows)
            {
                var newstr =Prolink.Math.GetValueAsString(dr[0]);
                var oldstr = Prolink.Math.GetValueAsString(dr[1]);
                string ex = "@Resources.Locale.";
                string languageId = newstr.Replace(ex, "");
                if (string.IsNullOrEmpty(languageId)) continue;
                string defaultstr= GetCaption(languageId,path);
                if (string.IsNullOrEmpty(defaultstr))
                {
                    DataRow diffdr = diff.NewRow();
                    diffdr[0] = languageId;
                    diffdr[1] = defaultstr;
                    diffdr["mark"] = "@Resources.Locale.L_Multilanguagecontrol_Business_82";
                    diff.Rows.Add(diffdr);
                }
                else if (!defaultstr.Equals(oldstr))
                {
                    DataRow diffdr = diff.NewRow();
                    diffdr[0] = languageId;
                    diffdr[1] = defaultstr;
                    diffdr["mark"] = "@Resources.Locale.L_Multilanguagecontrol_Business_83";
                    diff.Rows.Add(diffdr);
                }
            }
            if (diff.Rows.Count > 0)
            {
                Business.NPOIExcelHelp excel = new Business.NPOIExcelHelp();
                var work = excel.CreateWork();
                var sheet1 = excel.CreateSheet(work);
                excel.DataTableToExcel(diff, true, 0);
                Regex num = new Regex(@"\d");
                string strName = DateTime.Now.ToString("yyyyMMddHHmmss") + num.Replace(System.IO.Path.GetFileNameWithoutExtension(diffpath),"") + ".xlsx";
                string path2 = System.IO.Path.GetDirectoryName(savepath) + "\\backup\\";
                string strFile = path2 + strName;
                if (!Directory.Exists(path2))
                {
                    Directory.CreateDirectory(path2);
                }
                using (FileStream file = new FileStream(strFile, FileMode.Create))
                {
                    work.Write(file);
                    file.Close();
                }
            }
            return "";
        }
        public DataTable CheckResource(string cultureKey, string transtype, string path, string diffpath)
        {
            var n = diffpath.Split('.');
            string strExt = string.Empty;
            if (n != null && n.Length > 1)
            {
                strExt = n[n.Length - 1];
            }
            WebGui.Controllers.BaseController basecontroller = new WebGui.Controllers.BaseController();
            DataTable dt = basecontroller.ImportExcelToDataTable(diffpath, strExt, 0);
            //DataTable dt = Business.XExcelHelper.ImportExcelToDataTable(diffpath, strExt, 0);
            Regex numline=new Regex(@"^[1-9]\d*");
            string filename = System.IO.Path.GetFileName(diffpath);
            filename = numline.Replace(filename, "");
            DataTable diff = new DataTable();
            diff.Columns.Add("languageId");
            diff.Columns.Add("@Resources.Locale.L_Multilanguagecontrol_Business_84");
            diff.Columns.Add("excel@Resources.Locale.L_Multilanguagecontrol_Business_85");
            diff.Columns.Add("excel@Resources.Locale.L_Multilanguagecontrol_Business_86");
            diff.Columns.Add("mark");
            diff.Columns.Add("@Resources.Locale.L_Multilanguagecontrol_Business_87");
            foreach (DataRow dr in dt.Rows)
            {
                var newstr = Prolink.Math.GetValueAsString(dr[0]).Trim();
                var oldstr = Prolink.Math.GetValueAsString(dr[1]);
                string ex = "@Resources.Locale.";
                if (!newstr.Contains(ex)) { continue; }
                if (newstr.IndexOf(ex) > 0) { continue; }
                string oldstr_new=string.Empty;
                if (!string.IsNullOrEmpty(oldstr))
                    oldstr_new = GetLangTranslate(oldstr, transtype, baidu_appid, baidu_key, cultureKey);
                string languageId = newstr.Replace(ex, "");
                if (string.IsNullOrEmpty(languageId)) continue;
                string defaultstr = GetCaption(languageId, path);
                DataRow diffdr = null;
                if (!string.IsNullOrEmpty(defaultstr))
                {

                    if (!defaultstr.Equals(oldstr_new))
                    {
                        diffdr = diff.NewRow();
                        diffdr["mark"] = "@Resources.Locale.L_Multilanguagecontrol_Business_83";
                    }
                    else
                    {
                        diffdr = diff.NewRow();
                        diffdr["mark"] = "@Resources.Locale.L_Multilanguagecontrol_Business_88";
                    }
                }
                else
                {
                    diffdr = diff.NewRow();
                    diffdr["mark"] = "@Resources.Locale.L_Multilanguagecontrol_Business_82";
                }
                if (diffdr != null)
                {
                    diffdr["languageId"] = languageId;
                    diffdr["@Resources.Locale.L_Multilanguagecontrol_Business_84"] = defaultstr;
                    diffdr["excel@Resources.Locale.L_Multilanguagecontrol_Business_85"] = oldstr;
                    diffdr["excel@Resources.Locale.L_Multilanguagecontrol_Business_86"] = oldstr_new;
                    diffdr["@Resources.Locale.L_Multilanguagecontrol_Business_87"] = filename;
                    diff.Rows.Add(diffdr);
                }
            }
            if (diff.Rows.Count > 0)
                return diff;
            return null;
        }
        public static string GetCaption(string languageId,string strResxPath)
        {
            object strValue = null;
            AssemblyName[] referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            ResXResourceReader rsxResource = new ResXResourceReader(strResxPath);
            rsxResource.UseResXDataNodes = true;
            IDictionaryEnumerator enumerator = rsxResource.GetEnumerator();
            while (enumerator.MoveNext())
            {
                DictionaryEntry current = (DictionaryEntry)enumerator.Current;
                ResXDataNode node = (ResXDataNode)current.Value;
                string strKey = node.Name;    //资源项名  
                if (strKey.Equals(languageId)) {
                    strValue = node.GetValue(referencedAssemblies);
                    break;
                } 
            }
            return Prolink.Math.GetValueAsString(strValue);
        }
        public static string[] GetDirectoryFileName(string directoryPath, bool createDir)
        {
            if (createDir && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            string[] fileNames = null;
            try
            {
                fileNames = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            }
            catch (System.UnauthorizedAccessException) { }
            catch (System.ArgumentNullException) { }
            catch (System.ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (System.IO.DirectoryNotFoundException) { }
            catch (System.IO.IOException) { }
            if (fileNames == null)
                fileNames = new string[0];
            return fileNames;
        }
    }
}