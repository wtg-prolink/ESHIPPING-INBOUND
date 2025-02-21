using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Prolink.Web;
using Prolink.V3;
using System.Data;

namespace WebGui.App_Start
{
    public class BootstrapResult
    {
        /// <summary>
        /// 数据
        /// </summary>
        public object rows { get; set; }

        /// <summary>
        /// 子表资料
        /// </summary>
        public object childrenRows { get; set; }

        /// <summary>
        /// 列名集合
        /// </summary>
        public object colNames { get; set; }

        /// <summary>
        /// 列定义
        /// </summary>
        public object colModel { get; set; }

        /// <summary>
        /// 第几页
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int records { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int total { get; set; }

        /// <summary>
        /// 转成json
        /// </summary>
        /// <param name="ignoreGuid"></param>
        /// <returns></returns>
        public ContentResult ToContent()
        {
            JavaScriptSerializer jsSeri = new JavaScriptSerializer();

            string str = null;
            try
            {
                jsSeri.MaxJsonLength = 1024 * 1024;
                str = jsSeri.Serialize(this);
            }
            catch (System.InvalidOperationException e)
            {
                if (e != null && e.Message != null && e.Message.ToLower().Contains("max"))
                {
                    jsSeri.MaxJsonLength = 50 * 1024 * 1024;
                    str = jsSeri.Serialize(this);
                }
            }

            ContentResult result = new ContentResult();
            result.Content = str;
            return result;
        }

        public static BootstrapResult GetBootstrapResult(string name, DataTable dt, Dictionary<string, object> colKVS,int page, int total)
        {
            Dictionary<string, object> cm = new Dictionary<string, object>();
            Dictionary<string, object> cn = new Dictionary<string, object>();
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> list
               = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            ModelFactory.GetModelSchema(name, list);
            List<Dictionary<string, object>> collist = null;
            List<string> colNames = null;
            Dictionary<string, Dictionary<string, object>> colModels = null;
            if (colKVS != null)
            {
                foreach (var kv in colKVS)
                {
                    if (!list.ContainsKey(kv.Key))
                        continue;
                    colModels = list[kv.Key];
                    colNames = new List<string>();
                    Dictionary<string, object> col1 = null;
                    if (kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                    {
                        string[] cols = kv.Value.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        List<string> test = new List<string>();
                        test.AddRange(cols);
                        collist = new List<Dictionary<string, object>>();
                        Dictionary<string, object>[] cms = colModels.Values.ToList<Dictionary<string, object>>().ToArray();
                        //collist.AddRange();
                        string key = string.Empty;
                        foreach (string col in cols)
                        {
                            for (int i = 0; i < cms.Length; i++)
                            {
                                key = cms[i]["index"].ToString();
                                if (col.Equals(key))
                                {
                                    if (cms[i].ContainsKey("hidden")) cms[i].Remove("hidden");
                                    if (cms[i].ContainsKey("viewable")) cms[i].Remove("viewable");
                                    collist.Add(cms[i]);
                                    colNames.Add(key);
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < cms.Length; i++)
                        {
                            key = cms[i]["index"].ToString();
                            if (!colNames.Contains(key))
                            {
                                cms[i]["hidden"] = true;
                                cms[i]["viewable"] = false;
                                collist.Add(cms[i]);
                                colNames.Add(key);
                            }
                        }
                    }
                    else
                    {
                        collist = new List<Dictionary<string, object>>();
                        collist.AddRange(colModels.Values.ToList<Dictionary<string, object>>().ToArray());
                        colNames.AddRange(colModels.Keys.ToArray<string>());
                        for (int i = 0; i < collist.Count; i++)
                        {
                            col1 = collist[i] as Dictionary<string, object>;
                            if (col1.ContainsKey("hidden")) col1.Remove("hidden");
                            if (col1.ContainsKey("viewable")) col1.Remove("viewable");
                        }
                    }
                    cm[kv.Key] = collist;
                    cn[kv.Key] = colNames;
                }
            }

            BootstrapResult result = new BootstrapResult()
            {
                colModel = cm,
                colNames = cn,
                rows = ModelFactory.ToTableJson(dt,name),
                page = 1,
                records = dt.Rows.Count,
                total = total,
            };
            return result;
        }
    }
}