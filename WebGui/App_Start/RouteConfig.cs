using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebGui
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Helpers/{resource}.ashx/{*pathInfo}");
            //routes.MapRoute(
            //    "Default", // 路由名称
            //    "{lang}/{controller}/{action}/{args}", // 带有参数的 URL
            //    new { lang = "zh-CN", controller = "Home", action = "Index", args = UrlParameter.Optional } // 参数默认值
            //);//.RouteHandler = new MultiLangRouteHandler();
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
            routes.MapRoute(
                name: "Default",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { lang = "zh-CN", controller = "Home", action = "Login", id = UrlParameter.Optional }
            ).RouteHandler = new MultiLangRouteHandler(); ;
        }
    }

    public class MultiLangRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string lang = requestContext.RouteData.Values["lang"].ToString().ToLower();
            //string lang = string.Empty;
            //if (HttpContext.Current.Request["language"] == null)
            //{
            //    lang = HttpContext.Current.Request.Cookies["pl.passport.language"] == null ? "zh-CN" : HttpContext.Current.Request.Cookies["pl.passport.language"].Value;
            //}
            //else
            //    lang = HttpContext.Current.Request["language"];
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture =
                  new System.Globalization.CultureInfo(lang);
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo(lang);
            }
            catch
            {
                return base.GetHttpHandler(requestContext);
            }
            return base.GetHttpHandler(requestContext);
        }
    }
}