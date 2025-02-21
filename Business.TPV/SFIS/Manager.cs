using Business.Utils;
using Prolink.Integrate.Ftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace Business.TPV.SFIS
{
    public class Manager
    {
        /// <summary>
        /// 发送工单号
        /// </summary>
        public static string SendOrderNO(IEnumerable<OrderInfo> orderList,string location)
        {
            if (orderList == null) return null;
            List<OrderInfo> list = orderList.Where(info => !string.IsNullOrEmpty(info.Number) && info.Number.ToUpper() != "X").ToList();
            if (list == null || list.Count <= 0) return null;
            string txt = string.Join(Environment.NewLine, orderList.Select(item => string.Format("{0}|{1}|", item.Number, item.Status)));
            if (string.IsNullOrEmpty(txt)) throw new Exception("Order list conn't be null;");
            string fileName = string.Format("{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "ESMO");
            FTPUploader uploader = new FTPUploader(ExportModes.OrderNubmer);
            switch (location)
            {
                case "FQ": uploader = new FTPUploader(ExportModes.FQOrderNubmer);
                    return uploader.Upload(txt, fileName, location);
                    break;
                case "XM": uploader = new FTPUploader(ExportModes.XMOrderNubmer);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "BJ": uploader = new FTPUploader(ExportModes.BJOrderNubmer);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "QD": uploader = new FTPUploader(ExportModes.QDOrderNubmer);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "WH": uploader = new FTPUploader(ExportModes.WHOrderNubmer);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "BH": uploader = new FTPUploader(ExportModes.BHOrderNubmer);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                default:
                    uploader = new FTPUploader(ExportModes.FQOrderNubmer);
                    return uploader.Upload(txt, fileName, location);
                    break;
            }
        }

        internal static string SendDNStatus(IEnumerable<DNStatusInfo> dnStatusList)
        {
            string txt = string.Join(Environment.NewLine, dnStatusList.Select(item => string.Format("{0}|{1}", item.DNNO, item.GetStatusCode())));
            if (string.IsNullOrEmpty(txt)) throw new Exception("DN Status list conn't be null;");
            string fileName = string.Format("{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "ESDN");
            FTPUploader uploader = new FTPUploader(ExportModes.DNStatus);
            
            return uploader.Upload(txt, fileName);
        }

        internal static string SendDNStatus(IEnumerable<DNStatusInfo> dnStatusList,string location)
        {
            string txt = string.Join(Environment.NewLine, dnStatusList.Select(item => string.Format("{0}|{1}", item.DNNO, item.GetStatusCode())));
            if (string.IsNullOrEmpty(txt)) throw new Exception("DN Status list conn't be null;");
            string fileName = string.Format("{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "ESDN");
            FTPUploader uploader=null;
            switch(location){
                case "FQ": uploader=new FTPUploader(ExportModes.FQDNStatus);
                    return uploader.Upload(txt, fileName, location);
                    break;
                case "XM": uploader=new FTPUploader(ExportModes.XMDNStatus);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "BJ": uploader = new FTPUploader(ExportModes.BJDNStatus);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "QD": uploader = new FTPUploader(ExportModes.QDDNStatus);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "WH": uploader = new FTPUploader(ExportModes.WHDNStatus);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                case "BH": uploader = new FTPUploader(ExportModes.BHDNStatus);
                    return uploader.Upload(txt, fileName, location, false, true);
                    break;
                default:
                    uploader = new FTPUploader(ExportModes.FQDNStatus);
                    return uploader.Upload(txt, fileName, location);
                    break;
            }
            
        }
    }

    /// <summary>
    /// 工单信息
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// 工单号码
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 工单状态
        /// </summary>
        public string Status { get; set; }
    }

    class DNStatusInfo
    {
        public string DNNO { get; set; }
        public DNStatus Status { get; set; }

        public string CmpDNNO { get; set; }

        public string GetStatusCode()
        {
            switch (Status)
            {
                case DNStatus.Finished: return "1";
                default: return "0";
            }
        }
    }

    enum DNStatus
    {
        /// <summary>
        /// 未完成的
        /// </summary>
        Unfinished,
        /// <summary>
        /// 已完成的
        /// </summary>
        Finished
    }
}
