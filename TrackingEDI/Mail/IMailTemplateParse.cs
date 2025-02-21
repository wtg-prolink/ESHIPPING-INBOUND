using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace TrackingEDI.Mail
{
    /// <summary>
    /// IMailTemplateParse 的摘要说明。
    /// </summary>
    public interface IMailTemplateParse
    {
        /// <summary>
        /// 根据数据源格式化模板
        /// </summary>
        /// <param name="data"></param>
        /// <param name="childData"></param>
        /// <param name="htmlTemp"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        string Parse(DataTable data, DataTable childData, string htmlTemp,Dictionary<string,string> map);

        /// <summary>
        /// 获取模板主体内容
        /// </summary>
        /// <param name="htmlTemp"></param>
        /// <returns></returns>
        string GetBody(string htmlTemp);

        /// <summary>
        ///  根据数据源格式化模板   支持多个子表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataSet"></param>
        /// <param name="htmlTemp"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        string Parse(DataTable data, DataSet dataSet, string htmlTemp, Dictionary<string, string> map,string type);

        string Parse(DataTable data, DataSet dataSet, string htmlTemp, Dictionary<string, string> map, string type, Dictionary<string, Func<string, string, DataRow, string>> render);
        string Parse(DataTable data, DataTable childData, string htmlTemp, Dictionary<string, string> map, Dictionary<string, Func<string, string, DataRow, string>> render);
    }
}
