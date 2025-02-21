using Prolink.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace InternalWeb
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            Hashtable prop = new Hashtable();
            string serverPath = Server.MapPath(@"~");
            string path = System.IO.Path.Combine(serverPath, "../publish/Config/Config.xml");
            if (!System.IO.File.Exists(path))
                path = System.IO.Path.Combine(serverPath, "../WebGui/Config/Config.xml");
            prop[WebContext.CONFIGURE_FILE_PATH] = path;
            prop[WebContext.APP_PATH] = serverPath;
            Prolink.Web.WebContext.Build(prop);
            Prolink.V6.Core.SystemManager.Build(Prolink.Web.WebContext.GetInstance());
            Application.Add(Prolink.Web.WebContext.CONTEXT_INSTANCE, Prolink.Web.WebContext.GetInstance());

            prop = new Hashtable();
            prop[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(serverPath, "Log/");
            Prolink.Log.DefaultLogger logger = new Prolink.Log.DefaultLogger(prop);
            Prolink.DataOperation.OperationUtils.Logger = logger;

            Hashtable mailProp = new Hashtable();
            mailProp[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(serverPath, "MailLog");
            Prolink.Log.DefaultLogger mailLogger = new Prolink.Log.DefaultLogger(mailProp);
            Business.Mail.MailServices.GetInstance().SetLogger(mailLogger);
            Business.TPV.Context.Bulid();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}