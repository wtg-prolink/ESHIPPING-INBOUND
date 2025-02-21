using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Prolink.Task;
using Prolink.Web;
using TrackingEDI.Business;
using Business.TPV.Financial;

namespace NotifyService
{
    public class RuntimeContext
    {
        /// <summary>
        /// 获得运行路径
        /// </summary>
        public static string RuntimePath
        {
            get
            {
                string fileName = typeof(Service1).Assembly.CodeBase;
                int index = fileName.LastIndexOf("\\");
                if (index < 0) index = fileName.LastIndexOf("/");
                if (index >= 0) fileName = fileName.Substring(0, index);
                return fileName.Replace("file:///", "");
            }
        }
    }

    public partial class Service1 : ServiceBase
    {
        private PlanTaskManager _manager = null;
        public Service1()
        {
            InitializeComponent();

            Hashtable prop = new Hashtable();
            string serverPath = System.IO.Path.Combine(RuntimeContext.RuntimePath, "../../../publish");
            if (!System.IO.Directory.Exists(serverPath))
                serverPath = System.IO.Path.Combine(RuntimeContext.RuntimePath, "../../../WebGui");
            prop[WebContext.CONFIGURE_FILE_PATH] = System.IO.Path.Combine(serverPath, "Config/Config.xml");
            prop[WebContext.APP_PATH] = serverPath;

            //Prolink.Web.WebContext.Build(Server.MapPath("~\\Config\\Config.xml"));
            Prolink.Web.WebContext.Build(prop);

            Prolink.V6.Persistence.Context.ConfigurePath = System.IO.Path.Combine(serverPath, "../doc/xml-store/");
            Prolink.V6.Persistence.Factory.Build(System.IO.Path.Combine(serverPath, "../doc/xml-store/db/Company.xml"));
            Prolink.V6.Persistence.Entity.EntityFactory.Build();
            Prolink.V6.Core.SystemManager.Build(Prolink.Web.WebContext.GetInstance());

            prop = new Hashtable();
            prop[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(RuntimeContext.RuntimePath, "Logs/db");
            Prolink.Log.DefaultLogger logger = new Prolink.Log.DefaultLogger(prop);
            Prolink.DataOperation.OperationUtils.Logger = logger;
            TrackingEDI.Business.EvenNotify.RuntimePath = RuntimeContext.RuntimePath;
            this._manager = new PlanTaskManager(RuntimeContext.RuntimePath);
            //注册route mail的发送
            TrackingEDI.Business.EvenFactory.RegisterSendMail("ICS", MailManager.SendInboundCallCarMail1);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("INS", MailManager.SendInboundBookingMail1);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("IN", MailManager.SendInboundBookingMail);
            //TrackingEDI.Business.EvenFactory.RegisterSendMail("IRVTK", MailManager.SendRvNotifyMail);
            TrackingEDI.Business.EvenFactory.RegisterSendMail("CIRVTK", MailManager.SendInboundCancelRvNotifyMail);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.Inquery_Quotation, MailManager.SendIQQTvoid);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.INTERMODAL_CALLCAR, MailManager.SendInboundInterModalCallCarMail1);

            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLPassNotify, MailManager.SendInboundBillResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLRejectNotify, MailManager.SendInboundBillResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundBILLAp, MailManager.SendInboundBillApNotiyMail);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.InboundNotifyBrokerError, MailManager.SendMailByAutoNotifyBrokerError);

            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotRefuse_X, MailManager.SendQuotResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotRefuse_B, MailManager.SendQuotResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotRefuse_C, MailManager.SendQuotResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotNotify_B, MailManager.SendQuotResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotNotify_C, MailManager.SendQuotResult);
            TrackingEDI.Business.EvenFactory.RegisterSendMail(MailManager.IB_QuotNotify_X, MailManager.SendQuotResult);
            //TrackingEDI.Manager.AfterSetShipmentStatusTime = CallBill;
            Prolink.EDOC_API.initEDOC_API();

            this.EventLog.WriteEntry("Create tracking task Successfully,Application path:" + RuntimeContext.RuntimePath);
        }

        public static bool CallBill(string id)
        {
            Bill bill = new Bill();
            bill.Share(id);
            return true;
        }

        protected override void OnStart(string[] args)
        {
            this._manager.Start();
        }

        protected override void OnStop()
        {
            this._manager.Stop();
        }
    }
}
