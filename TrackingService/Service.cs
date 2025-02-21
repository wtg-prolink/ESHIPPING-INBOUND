using Prolink.Task;
using Prolink.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace TrackingService
{
    partial class Service : ServiceBase
    {
        PlanTaskManager _manager;
        public Service()
        {
            InitializeComponent();
            this.ServiceName = "TrackingServiceInbound";
            Hashtable prop = new Hashtable();
            string webGuiPath = System.IO.Path.Combine(RuntimePath, "../../../WebGui");
            if (!System.IO.Directory.Exists(webGuiPath))
                webGuiPath = System.IO.Path.Combine(RuntimePath, "../../../publish");
            prop[WebContext.CONFIGURE_FILE_PATH] = System.IO.Path.Combine(webGuiPath, "Config/Config.xml"); ;
            prop[WebContext.APP_PATH] = webGuiPath;

            Prolink.Web.WebContext.Build(prop);
            Prolink.Web.WebContext ctx = Prolink.Web.WebContext.GetInstance();

            Prolink.V6.Persistence.Context.ConfigurePath = System.IO.Path.Combine(webGuiPath, "../doc/xml-store/");
            Prolink.V6.Persistence.Factory.Build(System.IO.Path.Combine(webGuiPath, "../doc/xml-store/db/Company.xml"));
            Prolink.V6.Persistence.Entity.EntityFactory.Build();

            Prolink.V6.Core.SystemManager.Build(ctx);

            Hashtable hs_dataOperation = new Hashtable();
            hs_dataOperation[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(RuntimePath, "Logs/db");
            Prolink.Log.DefaultLogger logger = new Prolink.Log.DefaultLogger(hs_dataOperation);
            Prolink.DataOperation.OperationUtils.Logger = logger;

            Hashtable mailProp = new Hashtable();
            mailProp[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = webGuiPath + @"\MailLog\";
            Prolink.Log.DefaultLogger mailLogger = new Prolink.Log.DefaultLogger(mailProp);
            Business.Mail.MailServices.GetInstance().SetLogger(mailLogger);

            this._manager = new PlanTaskManager(RuntimePath);
            this.EventLog.WriteEntry("Create PlanTaskManager Successfully,Application path:" + RuntimePath
                );
            this.EventLog.WriteEntry("InitializeService Successfully");
            Business.TPV.Context.Bulid();
        }

        string RuntimePath
        {
            get
            {
                return Business.Utils.Context.RuntimePath;
            }
        }

        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            _manager.Start();
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            _manager.Stop();
        }

        static void Main()
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun;
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service() };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }
    }
}
