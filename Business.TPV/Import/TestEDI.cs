using Business.TPV.RFC;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Task;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.TPV.Import
{
    class TestEDI : IPlanTask
    {
        string _location = "";
        public void Run(IPlanTaskMessenger messenger)
        {

            //try
            //{
            //    PartnerManager pm = new PartnerManager();
            //    foreach (var s in AnalyzeFilePartner())
            //    {
            //        pm.ImportPartner("QA1888", s);
            //    }
            //}
            //catch
            //{

            //}

            //try
            //{
            //    LSP.OceanBookingManager m = new LSP.OceanBookingManager();
            //    foreach (var s in AnalyzeLspBooking())
            //    {
            //        m.ImportBookingResponse(s);
            //    }
            //}
            //catch
            //{

            //}
            try
            {
                string sapId = "TPV888";
                //string sapId = "QA1888";
                DNManager dnManager = new DNManager();
                List<string> list = new List<string>();
                List<string> exList = new List<string>();
                foreach (var s in AnalyzeFile2())
                {
                    try
                    {
                        var r = dnManager.ImportDNForSAP(sapId, s, "", DNImportModes.ReloadQuantity);
                        if (!r.IsSucceed)
                            list.Add(s);
                    }
                    catch
                    {
                        exList.Add(s);
                    }
                }
                if (list.Count > 0)
                {
                    UTF8Encoding utf8 = new UTF8Encoding(false);
                    File.WriteAllText(@"D:\text.txt", string.Join(",", list), utf8);
                    File.WriteAllText(@"D:\Web\u_disk\edihub\EDITestPublish\Import\新建文本文档.txt", string.Join(Environment.NewLine, exList), utf8);
                }
            }
            catch
            {

            }
            //Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("VBELN", "0080000246");
            //parameters.Add("WADAT", "20151029");
            //EDIBase obj = new EDIBase();
            //IRfcFunction function = GetOperator("ZRFC_DEMO_DN_PGI", parameters);
            //IRfcTable partnerTable = function.GetTable("ET_PARTNER");
            //return null;

            //RFC.BaseCodeEDI edi = new BaseCodeEDI();
            //try
            //{
            //    List<BaseCodeInfo> infos1 = edi.GetBaseCode("DEV150", "TVTW").ToList();
            //    List<BaseCodeInfo> infos2 = edi.GetBaseCode("DEV150", "TSPA").ToList();
            //    List<BaseCodeInfo> infos3 = edi.GetBaseCode("DEV150", "TINC").ToList();
            //    List<BaseCodeInfo> infos4 = edi.GetBaseCode("DEV150", "TVPT").ToList();
            //    List<BaseCodeInfo> infos5 = edi.GetBaseCode("DEV150", "TVST").ToList();
            //    List<BaseCodeInfo> infos6 = edi.GetBaseCode("DEV150", "VERP").ToList();
            //    List<BaseCodeInfo> infos7 = edi.GetBaseCode("DEV150", "TVKO").ToList();
            //    List<BaseCodeInfo> infos8 = edi.GetBaseCode("DEV150", "TVAK").ToList();
            //}
            //catch
            //{

            //}

            //TVTW: 销售渠道
            //TSPA: 产品组
            //TINC: 贸易条款
            //TVPT: 类别
            //TVST: 港口(启运港)
            //VERP: 货柜尺寸
            //TVKO: 销售组织
            //TVAK: 订单类型

            //ExchangeRateEDI edi = new ExchangeRateEDI();
            //edi.GetExchangeRateInfo("DEV150").ToList();

            //DNManager dnManager = new DNManager();
            //dnManager.ImportDNForSAP("DEV150", "10200080000447");
            //dnManager.ImportDNForSAP("DEV150", "10200080000448");
            //dnManager.ImportDNForSAP("DEV150", "10200080000481");
            //dnManager.ImportDNForSAP("DEV150", "10200080000482");
            //dnManager.ImportDNForSAP("DEV150", "10200080000485");
            //dnManager.ImportDNForSAP("DEV150", "10200080000529");
            //dnManager.ImportDNForSAP("DEV150", "10200080000751");
            //dnManager.ImportDNForSAP("DEV150", "10200080000805");
            //dnManager.ImportDNForSAP("DEV150", "10200080000868");
            //dnManager.ImportDNForSAP("DEV150", "10200080000879");
            //dnManager.ImportDNForSAP("DEV150", "10200080000886");
            //dnManager.ImportDNForSAP("DEV150", "10200080000892");
            //dnManager.ImportDNForSAP("DEV150", "10200080000899");
            //dnManager.ImportDNForSAP("DEV150", "10200080000949");
            //dnManager.ImportDNForSAP("DEV150", "10200080001020");
            //dnManager.ImportDNForSAP("DEV150", "10200080001035");
            //dnManager.ImportDNForSAP("DEV150", "10200080001337");
            //dnManager.ImportDNForSAP("DEV150", "10200080001355");
            //dnManager.ImportDNForSAP("DEV150", "10200080001364");
            //dnManager.ImportDNForSAP("DEV150", "10200080001377");
            //dnManager.ImportDNForSAP("DEV150", "10200080001378");
            //dnManager.ImportDNForSAP("DEV150", "10200080001385");
            //dnManager.ImportDNForSAP("DEV150", "10200080001392");
            //dnManager.ImportDNForSAP("DEV150", "10200080001393");
            //dnManager.ImportDNForSAP("DEV150", "10200080001396");
            //dnManager.ImportDNForSAP("DEV150", "10200080001397");
            //dnManager.ImportDNForSAP("DEV150", "10200080001408");
            //dnManager.ImportDNForSAP("DEV150", "10200080001410");
            //dnManager.ImportDNForSAP("DEV150", "10200080001514");
            //dnManager.ImportDNForSAP("DEV150", "10200080001530");
            //dnManager.ImportDNForSAP("DEV150", "10200080001559");
            //dnManager.ImportDNForSAP("DEV150", "10200080001560");
            //dnManager.ImportDNForSAP("DEV150", "10200080001561");
            //dnManager.ImportDNForSAP("DEV150", "10200080001562");
            //dnManager.ImportDNForSAP("DEV150", "10200080001563");
            //dnManager.ImportDNForSAP("DEV150", "10200080001564");
            //dnManager.ImportDNForSAP("DEV150", "10200080001565");
            //dnManager.ImportDNForSAP("DEV150", "10200080001566");
            //dnManager.ImportDNForSAP("DEV150", "10200080001625");
            //dnManager.ImportDNForSAP("DEV150", "10200080001626");
            //dnManager.ImportDNForSAP("DEV150", "10200080001627");
            //dnManager.ImportDNForSAP("DEV150", "10200080002212");
            //dnManager.ImportDNForSAP("DEV150", "10200080002213");
            //dnManager.ImportDNForSAP("DEV150", "10200080002215");
            //dnManager.ImportDNForSAP("DEV150", "10200080002259");
            //dnManager.ImportDNForSAP("DEV150", "10200080002260");
            //dnManager.ImportDNForSAP("DEV150", "10200080002295");
            //dnManager.ImportDNForSAP("DEV150", "10200080002296");
            //dnManager.ImportDNForSAP("DEV150", "10200080002298");
            //dnManager.ImportDNForSAP("DEV150", "10200080002299");
        }

        IEnumerable<string> AnalyzeFilePartner()
        {
            string path = @"D:\Web\u_disk\EDIHub\EDITestPublish\Import\partner";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles())
            {
                using (StreamReader reder = new StreamReader(item.FullName))
                {
                    string txt = string.Empty;
                    while (!string.IsNullOrEmpty((txt = reder.ReadLine())))
                    {
                        yield return txt;
                    }
                }
            }
        }

        IEnumerable<string> AnalyzeFile2()
        {
            string path = @"D:\Web\u_disk\edihub\EDITestPublish\Import\";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles())
            {
                using (StreamReader reder = new StreamReader(item.FullName))
                {
                    string txt = string.Empty;
                    while (!string.IsNullOrEmpty((txt = reder.ReadLine())))
                    {
                        yield return txt;
                    }
                }
            }
        }


        IEnumerable<Business.TPV.Standard.OceanBookingResponse> AnalyzeLspBooking()
        {
            string path = @"C:\Users\buru\Desktop\TPV\UAT\外代\20160219";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles())
            {
                using (StreamReader reder = new StreamReader(item.FullName))
                {
                    string txt = reder.ReadToEnd();
                    Business.TPV.Standard.OceanBookingResponse info = ToJsonObj<Business.TPV.Standard.OceanBookingResponse>(txt);
                    yield return info;
                }
            }
        }


        IEnumerable<CompanyInfo> AnalyzeCompanyInfo()
        {
            string path = @"D:\Web\TPV\TrackingService\bin\Import\CompanyInfo\20160427";
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles())
            {
                using (StreamReader reder = new StreamReader(item.FullName))
                {
                    string txt = reder.ReadToEnd();
                    CompanyInfo info = ToJsonObj<CompanyInfo>(txt);
                    yield return info;
                }
            }
        }


        T ToJsonObj<T>(string jsonStr) where T : class
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            return Serializer.Deserialize(jsonStr, typeof(T)) as T;
        }
    }
}
