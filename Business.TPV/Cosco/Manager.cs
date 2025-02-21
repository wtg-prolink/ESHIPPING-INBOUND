using Business.EDI;
using Business.Service;
using Business.TPV.Base;
using Business.TPV.Standard;
using Business.Utils;
using Models.EDI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Cosco
{
    public class Manager : WebRequstBase
    {
        Cosco.BookingService CreateService(EDIConfig config)
        {
            Cosco.BookingService service = new Cosco.BookingService();
            service.Url = config.Server;

            CosSoapHeader soapHeader = new CosSoapHeader();
            service.SoapHeaderValue = new CosSoapHeader { account = config.User, password = config.Psw, Authorization = config.Authorization };
            //service.Authorization = "Bearer af95d247-6a2d-3174-85d4-2a16e852bc0f";

            return service;
        }

        public ResultInfo SendBooking(Runtime runtime)
        {
            Business.TPV.Standard.Manager m = new Standard.Manager();
            OceanBooking template = null;
            EDIConfig config = null;
            DataRow smRow = null;
            ResultInfo r = m.CreateOceanBooking(runtime, out template, out config, out smRow);

            if (!r.IsSucceed) return r;
            var result = SendShipment(runtime, template, config, smRow);
            WriteEDILog(new CoscoBookingEDILog(runtime, smRow), result);
            return result;
        }

        ResultInfo CheckBooking(Booking template)
        {
            EntityValidationResult checkResult = null;
            if (!template.Check(out checkResult))
            {
                return new ResultInfo
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = new EntityValidationResultException(checkResult).ValidationMsg
                };
            }
            return SucceedResult();
        }

        ResultInfo SendShipment(Runtime runtime, OceanBooking template, EDIConfig config,DataRow smrow)
        {
            ResultInfo checkResult = CheckBooking(template);
            if (!checkResult.IsSucceed) return checkResult;
            string xml = XmlUtil.Serializer<OceanBooking>(template);
          
            if (OperationModes.Cancel.Equals(runtime.OperationMode))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlElement xesono = doc.CreateElement("SoNo");
                xesono.InnerText = smrow["SO_NO"].ToString();
                doc.DocumentElement.AppendChild(xesono);
                xml = ConvertXmlToString(doc);
            }

            string fileName = Backup(new List<string> { BackupDirName_Requst, Mode.ToString() }, xml, string.Format("{0}_{1}", template.ShipmentID, GetCurrentTimeString()));
            runtime.Data = fileName;
            var s = CreateService(config);
            try
            {
                var v = s.booking(xml);
                return new ResultInfo
               {
                   IsSucceed = v.flag,
                   ResultCode = v.code,
                   Description = string.Format("COSCO返回提示：{0}", v.msg)
               };
            }
            catch (Exception ex)
            {
                return new ResultInfo
                {
                    IsSucceed = false,
                    ResultCode = "Exception",
                    Description = string.Format("COSCO返回提示：{0}", ex.Message)
                };
            }

        }

        public string ConvertXmlToString(XmlDocument xmlDoc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);
            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();
            return xmlString;
        }


        protected override RequstModes Mode
        {
            get { return RequstModes.Cosco; }
        }
    }
}
