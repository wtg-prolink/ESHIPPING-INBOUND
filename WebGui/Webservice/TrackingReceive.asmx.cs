using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Prolink.Web.Services;
using System.Data;
using TrackingEDI.Business;
namespace WebGui.Webservice
{
    /// <summary>
    /// TrackingReceive 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class TrackingReceive : System.Web.Services.WebService
    {
        #region 登陆判断
        /// <summary>
        /// 登陆判断,如果用户有存在就将信息存入session中
        /// </summary>
        /// <param name="Uid">用户名</param>
        /// <param name="Pwd">密码</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public string isUserExist(String Uid, String Pwd)
        {
            //return BaseTrackingReceiver.isUserExist(Uid, Pwd);
            return "";
        }
        #endregion

        #region 接收空运提单资料
        [WebMethod(EnableSession = true)]
        public String fillAirCargoBillXMLINTOdb(String xmlvalue)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.SaveAirCargoBill(xmlvalue);
                EvenFactory.SaveLog("", xmlvalue, "空运信息传送成功", xp.HouseNo, "AB", xp.JobNo);
                return GetMsg("空运信息传送成功!", true); //以xml信息的形式返回信息
            }
            catch (Exception ex)
            {
                EvenFactory.SaveLog("", xmlvalue, "空运信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "AB", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Receive AirTracking Data Error:" + xp.Warning, ex);
            }
        }
        #endregion

        #region 接收海运提单资料
        [WebMethod(EnableSession = true)]
        public String fillOceanCargoBillXMLINTOdb(String xmlvalue)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.SaveOceanCargoBill(xmlvalue);
                EvenFactory.SaveLog("", xmlvalue,"海运信息传送成功", xp.HouseNo, "OB", xp.JobNo);
                return GetMsg("海运信息传送成功!", true); //以xml信息的形式返回信息

            }
            catch (Exception ex)
            {
                EvenFactory.SaveLog("", xmlvalue, "海运信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "OB", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Receive OceanTracking Data Error:" + xp.Warning, ex);
            }
        }
        #endregion

        #region 接收集装箱提单资料
        [WebMethod(EnableSession = true)]
        public String fillContainerXMLINTOdb(String xmlvalue)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.SaveContainer(xmlvalue);
                EvenFactory.SaveLog("", xmlvalue, "集装箱信息传送成功", xp.HouseNo, "CO", xp.JobNo);
                return GetMsg("集装箱信息传送成功", true);
            }
            catch (Exception ex)
            {
                //将信息写入日志
                WriteLog(ex);
                EvenFactory.SaveLog("", xmlvalue, "集装箱信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "CO", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Receive ContainerTracking Data Error:" + xp.Warning, ex);
            }
        }
        #endregion

        #region 接收PO资料
        [WebMethod(EnableSession = true)]
        public String fillPOXMLINTOdb(String xmlvalue)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.SavePo(xmlvalue);
                EvenFactory.SaveLog("", xmlvalue, "PO信息传送成功", xp.HouseNo, "PO", xp.JobNo);
                return GetMsg("PO信息传送成功", true);
            }
            catch (Exception ex)
            {
                //将信息写入日志
                WriteLog(ex);
                EvenFactory.SaveLog("", xmlvalue, "PO信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "PO", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Receive POTracking Data Error:" + xp.Warning, ex);
            }
        }
        #endregion

        #region 接收货况资料
        [WebMethod(EnableSession = true)]
        public String fillSTATUSINTOdb(String xmlvalue)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.SaveStatus(xmlvalue);
                EvenFactory.SaveLog("", xmlvalue, "货况信息传送成功:", xp.HouseNo, "SS", xp.JobNo);
                return GetMsg("货况信息传送成功", true);
            }
            catch (Exception ex)
            {
                //将信息写入日志
                WriteLog(ex);
                EvenFactory.SaveLog("", xmlvalue, "货况信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "SS", xp.JobNo,"N");
                return ServiceUtils.GetErrorMessage("Receive StatusTracking Data Error:"+xp.Warning, ex);

            }
        }

        [WebMethod(EnableSession = true, Description = "批量更新主单货况时分单的货况也要同时更新")]
        public String fillStatusListByItrace(String xmlvalue)
        {
            try
            {
                xmlvalue = Prolink.IO.CompressUtils.DecompressFromString(xmlvalue);
                TraceStatus ts = new TraceStatus(xmlvalue);
                ts.Save();
                //EvenFactory.SaveLog("", xmlvalue, "货况信息传送成功:", ts.HouseNo, "SS", xp.JobNo);
                return GetMsg("货况信息传送成功", true);
            }
            catch (Exception ex)
            {
                //将信息写入日志
                WriteLog(ex);
                //EvenFactory.SaveLog("", xmlvalue, "货况信息传送失败:" + ex.Message + xp.Warning, xp.HouseNo, "SS", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Receive Status List Data Error:", ex);

            }
        }

        [WebMethod(EnableSession = true, Description = "更新主单货况时分单的货况也要同时更新")]
        public String fillSTATUSByItrace(String xmlvalue)
        {
            return "该方法已作废,请改用fillStatusListByItrace";
        }
        #endregion

        #region  删除货况
        /// <summary>
        /// 删除货况数据
        /// </summary>
        /// <param name="houseNO">houseNo</param>
        /// <param name="statusCode">货况代码</param>
        /// <param name="containerNO">柜号</param>
        /// <returns>返回货况删除成功或失败的信息</returns>
        [WebMethod(EnableSession = true)]
        public String deleteStatus(string houseNO, string statusCode, string containerNO)
        {
            return "该功能正因需求有变，正在调整中...";
        }
        #endregion

        #region 作废提单
        [WebMethod(EnableSession = true)]
        public String voidTrackingData(String house_no)
        {
            XmlParser xp = new XmlParser();
            try
            {
                xp.DeleteTrackingData(house_no);
                EvenFactory.SaveLog("", house_no, "作废提单成功", house_no, "BL", xp.JobNo);
                return GetMsg("提单删除成功", true);
            }
            catch (Exception ex)
            {
                //将信息写入日志
                WriteLog(ex);
                EvenFactory.SaveLog("", house_no, "作废提单失败:" + ex.Message + xp.Warning, house_no, "BL", xp.JobNo, "N");
                return ServiceUtils.GetErrorMessage("Delete TrackingData Error:" + xp.Warning, ex);
            }
        }
        #endregion

        #region 判断某一客户是否有两天时间都没上传数据
        /// <summary>
        /// 判断某一客户是否有两天时间都没上传数据
        /// </summary>
        /// <param name="cutomer_no">公司id</param>
        /// <param name="programe_id">项目代码</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public String notifyServerLife(string cutomer_no, string programe_id)
        {
            //BaseTrackingReceiver obj = new BaseTrackingReceiver();
            bool result = XmlParser.HaveUploadData(cutomer_no);
            return GetMsg(result ? "OK" : "NO", true);

        }
        #endregion

        #region 写日志
        private void WriteLog(Exception ex)
        {

            //将信息写入日志
            new Prolink.Log.DefaultLogger(Server.MapPath("~/Log/Webservice")).WriteLog(ex);
        }
        #endregion

        #region 接收进来的xml写到另外一个文件中
        /// <summary>
        /// 接收进来的xml写到另外一个文件中
        /// </summary>
        /// <param name="type">当前操作的类型，比如是空运还是海运等</param>
        /// <param name="Xml">传过来的xml源串</param>
        private void WriteXmlLog(string type, String Xml)
        {
            //将xml资料写入日志文件
            new Prolink.Log.DefaultLogger(Server.MapPath(string.Format("~/Log/Webservice/{0}", type))).WriteLog(Xml);
        }
        #endregion

        #region 包装消息返回
        public static string GetMsg(string Msg, bool Action)
        {
            WSResult Res = new WSResult(Action);
            Res.PutMsgData(Msg, false);
            return Res.ToXML();
        }
        #endregion
    }
}
