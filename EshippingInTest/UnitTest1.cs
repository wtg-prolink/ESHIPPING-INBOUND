using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using Business.Service;
using Business.TPV;
using Business.TPV.Import;
using Business.TPV.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolink.Data;
using Prolink.DataOperation;
using Prolink.Web;
namespace EshippingInTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void InitializeTestRun()
        {
            Initialize.Initialized();
        }

        [TestMethod]
        public void TestO2IFunc()
        {
            string is_ok = TrackingEDI.InboundBusiness.InboundHelper.O2IFunc("SZB2208001395");
        }
        [TestMethod]
        public void TestSetAsnNoDetai()
        {
            Dictionary<string, XmlDocument> docxml = GetXmlDocfromPath("AsnDoc");
            foreach(var item in docxml)
            {
                bool success= ASNManager.SetAsnDetail(item.Value, SetEdiInfo(), false, item.Key);
                Assert.IsTrue(success);
            }
        }

        [TestMethod]
        public void TestSetGRNoDetai()
        {
            Dictionary<string, XmlDocument> docxml = GetXmlDocfromPath("GrDoc");
            foreach (var item in docxml)
            {
                bool success = ASNManager.SetGrDetail(item.Value, SetEdiInfo());
                Assert.IsTrue(success);
            }
        }

        private EdiInfo SetEdiInfo()
        {
            EdiInfo ediinfo = new EdiInfo();
            ediinfo.DataFolder = "";
            ediinfo.CreateBy = "System Auto";
            ediinfo.FromCd = "ftp";
            ediinfo.ToCd = "Eshipping";
            ediinfo.Rs = "Receive";
            ediinfo.Cmp = "MN";
            ediinfo.GroupId = "TPV";
            return ediinfo;
        }

        private Dictionary<string,XmlDocument> GetXmlDocfromPath(string docpath)
        {
            Dictionary<string, XmlDocument> xmldocList = new Dictionary<string, XmlDocument>();
            string avapath = System.IO.Path.Combine(Initialize.TEST_PATH, docpath);
            List<string> filepathlist = PathName(avapath);
            foreach (string filepath in filepathlist)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filepath);
                string[] files = fileName.Split('_');
                string invNo = "";
                if (files.Length > 4)
                {
                    invNo = files[3];
                }
                if (string.IsNullOrEmpty(invNo))
                    invNo = fileName;
                string xml = new System.IO.StreamReader(filepath).ReadToEnd();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                xmldocList.Add(invNo, doc);
            }
            return xmldocList;
        }

        private List<string> PathName(string folderFullName)
        {
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            List<string> pathname = new List<string>();
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                pathname.Add(NextFile.FullName);
            }
            return pathname;
        }

    }
}

