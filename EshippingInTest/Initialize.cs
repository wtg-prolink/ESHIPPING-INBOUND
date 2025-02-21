using Prolink.Web;
using System.Collections;

namespace EshippingInTest
{
    public static class Initialize
    {
        private static bool isInitialise = false;
        public static string TEST_PATH="";
        public static string WEB_PATH = "";

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialized()
        {
            if (!isInitialise)
                InitializeHandle();
        }

        private static void InitializeHandle()
        {
            Hashtable prop = new Hashtable();
            string RuntimePath = Business.Utils.Context.RuntimePath;
            WEB_PATH = System.IO.Path.Combine(RuntimePath, "../../../WebGui");
            if (!System.IO.Directory.Exists(WEB_PATH))
                WEB_PATH = System.IO.Path.Combine(RuntimePath, "../../../publish");
            TEST_PATH= System.IO.Path.Combine(RuntimePath, "../../TestPath");
            

            string path = System.IO.Path.Combine(WEB_PATH, "Config/Config.xml"); ;
            prop[WebContext.CONFIGURE_FILE_PATH] = path;
            prop[WebContext.APP_PATH] = WEB_PATH;
            Prolink.Web.WebContext.Build(prop);
            Prolink.V6.Core.SystemManager.Build(Prolink.Web.WebContext.GetInstance());

            prop = new Hashtable();
            prop[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(WEB_PATH, "Log/");
            Prolink.Log.DefaultLogger logger = new Prolink.Log.DefaultLogger(prop);
            Prolink.DataOperation.OperationUtils.Logger = logger;

            Hashtable mailProp = new Hashtable();
            mailProp[Prolink.Log.DefaultLogger.LOG_DIRECTORY] = System.IO.Path.Combine(WEB_PATH, "MailLog");
            Prolink.Log.DefaultLogger mailLogger = new Prolink.Log.DefaultLogger(mailProp);
            Business.Mail.MailServices.GetInstance().SetLogger(mailLogger);
            Business.TPV.Context.Bulid();
            isInitialise = true;
        }
    }
}
