using Business.Service;
using Business.TPV.Base;
using Business.TPV.Standard;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Business.TPV.LSP
{
    public class ExportManager : Export.ShipmentManager
    {
        LspWS.TPVService CreateService(EDIConfig config)
        {
            LspWS.TPVService service = new LspWS.TPVService();
            service.Url = config.Server;
            LspWS.TpvSoapHeader soapHeader = new LspWS.TpvSoapHeader();
            soapHeader.username = config.User;
            soapHeader.password = config.Psw;
            service.TpvSoapHeaderValue = soapHeader;
            return service;
        }
        enum TmplateModes { Booking, Declaration }
        ResultInfo SendTemplate<T>(T template, Runtime runtime, EDIConfig config, TmplateModes mode, bool sendToBookingAgent = false) where T : OceanBooking
        {
            ResultInfo checkResult = CheckBooking(template);
            if (!checkResult.IsSucceed) return checkResult;
            string xml = XmlUtil.Serializer<T>(template);
            LspWS.TPVService service = CreateService(config);
            string fileName = Backup(new List<string> { config.MsgCode }, xml, string.Format("{0}_{1}", template.ShipmentID, GetCurrentTimeString()));
            runtime.Data = fileName;
            LspWS.ResultInfo result = null;
            switch (mode)
            {
                case TmplateModes.Booking: result = service.TPVDataExchange("BookingInformation", xml); break;
                case TmplateModes.Declaration:
                    if (sendToBookingAgent)
                        result = service.TPVDataExchange("OceanDeclaration", xml);
                    else
                        result = service.TPVBrokerExchange("OceanDeclaration", xml); break;
            }
            if (result.ResultCode == LspWS.ResultCodeInfo.Succeed)
            {
                if (template.Mode == "C" && mode == TmplateModes.Booking)
                {
                    Helper.CancelBooking(template.ShipmentID);
                }
            }
            return new ResultInfo
            {
                IsSucceed = result.ResultCode == LspWS.ResultCodeInfo.Succeed,
                ResultCode = result.ResultCode.ToString(),
                Description = string.Format("对方返回提示：{0}", result.ResultDesc)
            };
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

        public ResultInfo SendBooking(Runtime runtime)
        {
            Runtime = runtime;
            Standard.Manager m = new Manager();
            OceanBooking template = null;
            EDIConfig config = null;
            DataRow smRow = null;
            var r = m.CreateOceanBooking(runtime, out template, out config, out smRow);
            if (!r.IsSucceed) return r;
            var result = SendTemplate(template, runtime, config, TmplateModes.Booking);
            WriteEDILog(new XPIBookingEDILog(runtime, smRow), result);
            return result;
        }

        public ResultInfo SendDeclaration(Runtime runtime, bool sendToBookingAgent = false)
        {
            Standard.Manager m = new Manager();
            OceanDeclaration template = null;
            EDIConfig config = null;
            DataRow smRow = null;
            var r = m.CreateOceanDeclaration(runtime, out template, out config, out smRow);
            if (!r.IsSucceed) return r;
            var result = SendTemplate(template, runtime, config, TmplateModes.Declaration, sendToBookingAgent);
            WriteEDILog(new XPIDeclarationEDILog(runtime, smRow), result);
            return result;
        }
        protected override RequstModes Mode
        {
            get { return RequstModes.LSP; }
        }
    }
}