using Business.TPV.RFC;
using Business.Utils;
using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.TPV.Import
{
    public class PartnerManager : CompanyManagerBase
    {
        //public void Import()
        //{
        //    try
        //    {
        //        DataTable dt = Query();
        //        foreach (var s in AnalyzeFile2())
        //        {
        //            Save(new List<CompanyInfo>() { s }, dt);
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}


        //IEnumerable<CompanyInfo> AnalyzeFile2()
        //{
        //    string path = @"D:\Web\TPV\TrackingService\bin\Import\CompanyInfo\20160117_Base\";
        //    DirectoryInfo dir = new DirectoryInfo(path);
        //    foreach (var item in dir.GetFiles())
        //    {
        //        using (StreamReader reder = new StreamReader(item.FullName))
        //        {
        //            string txt = reder.ReadToEnd();
        //            CompanyInfo info = ToJsonObj<CompanyInfo>(txt);
        //            yield return info;
        //        }
        //    }
        //}

        //T ToJsonObj<T>(string jsonStr) where T : class
        //{
        //    JavaScriptSerializer Serializer = new JavaScriptSerializer();
        //    return Serializer.Deserialize(jsonStr, typeof(T)) as T;
        //}
    }
}