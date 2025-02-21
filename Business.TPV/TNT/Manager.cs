using Business.EDI;
using Business.Service;
using Business.TPV.Base;
using Business.TPV.Standard;
using Business.Utils;
using Models.EDI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.TNT
{
    public class Manager : WebRequstBase
    {
        TNT.TPVServerService CreateService(EDIConfig config)
        {
            TNT.TPVServerService service = new TNT.TPVServerService();
            service.Url = config.Server;
            return service;
        }

        public ResultInfo SendBooking(Runtime runtime)
        {
            Business.TPV.Standard.Manager m = new Standard.Manager();
            m.DateFormat = "dd/MM/yyyy";
            ExpressBooking template = null;
            EDIConfig config = null;
            DataRow smRow = null;
            ResultInfo r = m.CreateExpressBooking(runtime, out template, out config, out smRow);
            if (!r.IsSucceed) return r;
            var result = SendShipment(runtime, template, config);
            WriteEDILog(new TNTBookingEDILog(runtime, smRow), result);
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


        ResultInfo SendShipment(Runtime runtime, ExpressBooking template, EDIConfig config)
        {
            ResultInfo checkResult = CheckBooking(template);
            if (!checkResult.IsSucceed) return checkResult;
            string xml = XmlUtil.Serializer<ExpressBooking>(template);
            string fileName= Backup(new List<string> { BackupDirName_Requst, Mode.ToString() }, xml, string.Format("{0}_{1}", template.ShipmentID, GetCurrentTimeString()));
            runtime.Data = fileName;
            var s = CreateService(config);
            var result = s.callByTPV(xml);
            Backup(new List<string> { BackupDirName_Response, Mode.ToString() }, result, string.Format("{0}_{1}", template.ShipmentID, GetCurrentTimeString()));
            return ParseResult(result);
        }

        ResultInfo ParseResult(string result)
        {
            try
            {
                XmlDocument r = new XmlDocument();
                r.LoadXml(result);
                XmlNodeList errorList = r.GetElementsByTagName("ERROR");
                if (errorList != null && errorList.Count > 0) return ParseError(errorList[0]);
                var unR = UnknowResult();
                unR.Description = string.Join(Environment.NewLine, "TNT返回未知结果!", result);
                return unR;
            }
            catch (Exception ex)
            {
                return new ResultInfo
                   {
                       ResultCode = "ParseResultException",
                       Description = string.Format("TNT返回结果解析异常：{0}", ex.Message)
                   };
            }
        }

        ResultInfo ParseError(XmlNode node)
        {
            string code = string.Empty;
            string msg = string.Empty;
            foreach (XmlNode n in node.ChildNodes)
            {
                switch (n.Name)
                {
                    case "CODE": code = n.InnerText; break;
                    case "DESCRIPTION": msg = n.InnerText; break;
                }
            }
            bool IsSucceed = false;
            switch(code){
                case "Confirmed":
                case "05": IsSucceed = true; break;
                default: IsSucceed = false; break;
            };
            return new ResultInfo
            {
                IsSucceed = IsSucceed,
                ResultCode = code,
                Description = string.Format("TNT返回结果:{0},{1}", code, msg)
            };
        }

        protected override RequstModes Mode
        {
            get { return RequstModes.TNT; }
        }
    }
}
