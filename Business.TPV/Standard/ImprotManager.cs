using Business.Service;
using Business.TPV.Utils;
using Business.Utils;
using Models.EDI;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Business.TPV.Standard
{
    public abstract class ImprotManager<T> : Business.Import.ImportBase where T : InfoBase
    {
        public ResultInfo ImportXml(XmlDocument doc)
        {
            string fileName = BackupData(new List<string> { BackupDirName_Import, this.GetType().Name, "XmlDocument" }, doc);
            Logger.WriteLog("接收讯息", this.GetType().Name, "", "", fileName);
            T info = OperateData(doc);
            return ImportInstance(info);
        }

        protected virtual T OperateData(XmlDocument doc)
        {
            return XmlUtil.Deserialize<T>(doc.InnerXml);
        }

        public ResultInfo ImportInstance(T obj)
        {
            return ImportInstanceList(new List<T>() { obj });
        }

        public virtual ResultInfo ImportInstanceList(IEnumerable<T> infos)
        {
            ResultInfo nullErrorResult = new ResultInfo { ResultCode = ResultCode.ValidateException, Description = "资料不可为空！" };
            if (infos == null) return nullErrorResult;
            List<T> items = infos.ToList();
            if (items == null || items.Count <= 0) return nullErrorResult;
            List<T> cancelItems = items.Where(item => item.Mode == "C").ToList();
            List<T> insItems = items.Where(item => item.Mode != "C").ToList();
            ResultInfo result = null;
            if (insItems != null && insItems.Count > 0)
            {
                try
                {
                    result = HandlerIns1(insItems);
                    if (!result.IsSucceed)
                        return result;
                }
                catch (Exception e)
                {
                    return new ResultInfo { IsSucceed = false, ResultCode = "fail", Description = e.Message };
                }
            }
            if (cancelItems != null && cancelItems.Count > 0)
            {
                result = HandlerCancel(cancelItems);
                var l = Logger.CreateLog("撤消订舱结果", this.GetType().Name, "", "", result.Description);
                l.Data = cancelItems;
                Logger.WriteLog(l);
            }
            return result;
        }

        ResultInfo HandlerCancel(List<T> infos)
        {
            ResultInfo r = null;
            foreach (var item in infos)
            {
                string fileName = BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                item.Data = fileName;
                if (string.IsNullOrEmpty(item.Remark))
                    return new ResultInfo { ResultCode = ResultCode.ValidateException, Description = "拒绝/撤消原因(REMARK)不可为空!" };
                if (string.IsNullOrEmpty(item.ShipmentID))
                    return new ResultInfo { ResultCode = ResultCode.ValidateException, Description = "ShipmentID 不可为空!" };
                r = HandlerCancel(item);
                if (!r.IsSucceed)
                    return r;
            }
            return r;
        }

        protected abstract ResultInfo HandlerCancel(T info);
        protected virtual ResultInfo HandlerAdd(List<T> infos)
        {
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                eiList.MergeEditInstructList(ToEi(item));
            }
            return Execute(eiList);
        }
        protected abstract EditInstructList ToEi(T obj);

        protected ResultInfo CheckIsMatchBookingAgent(T info)
        {
            var items = Helper.GetBookingAgent(info.ShipmentID);
            if (items == null) return SucceedResult();
            var idList = items.Select(row => Prolink.Math.GetValueAsString(row["PARTY_NO"])).ToList();
            if (idList == null || idList.Count <= 0)
                return new ResultInfo
                {
                    ResultCode = "BookingAgentIsNull",
                    Description = "验证该笔订舱异常,未找到订舱代理!"
                };
            if (idList.Contains(info.Sender)) return SucceedResult();
            return new ResultInfo
            {
                ResultCode = "BookingAgentError",
                Description = "验证该笔订舱确认不通过,订舱代理不匹配，请验证节点Sender值是否有误!"
            };
        }

        protected virtual ResultInfo CheckPartyInfo(T info)
        {
            return CheckIsMatchBookingAgent(info);
        }

        ResultInfo HandlerIns(List<T> infos)
        {
            EntityValidationResult result = null;
            if (!Check<T>(infos, ref result))
            {
                var v = new ResultInfo()
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = string.Join(Environment.NewLine, result.Errors.Select(item => item.ErrorMessage))
                };
                var l = Logger.CreateLog("规格验证不通过", this.GetType().Name, "", "", v.Description);
                Logger.WriteLog(l);
                return v;
            }

            UnknowResult();
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                string fileName = BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                item.Data = fileName;
                string shipment_id= item.ShipmentID;
                if(string.IsNullOrEmpty(shipment_id)){
                    //shipment_id = ((BillingInfo)item).BillingDetails[0].ShipmentID;
                }
                DataRow row = Helper.QuerySM(item.ShipmentID);
                item.SMRows = new List<DataRow> { row };
                ResultInfo r = CheckForBusiness(item, row);
                if (!r.IsSucceed) return r;
                HandlerFile(item, row);
            }
            return HandlerAdd(infos);
        }

        ResultInfo HandlerIns1(List<T> infos)
        {
            EntityValidationResult result = null;
            if (!Check<T>(infos, ref result))
            {
                var v = new ResultInfo()
                {
                    ResultCode = ResultCode.ValidateException,
                    Description = string.Join(Environment.NewLine, result.Errors.Select(item => item.ErrorMessage))
                };
                var l = Logger.CreateLog("规格验证不通过", this.GetType().Name, "", "", v.Description);
                Logger.WriteLog(l);
                return v;
            }

            UnknowResult();
            EditInstructList eiList = new EditInstructList();
            foreach (var item in infos)
            {
                string fileName = BackupData(item, string.Format("{0}_{1}", item.ShipmentID, GetCurrentTimeString()));
                item.Data = fileName;
                DataRow row = Helper.QuerySM(item.ShipmentID);
                item.SMRows = new List<DataRow> { row };
                ResultInfo r = CheckForBusiness(item, row);
                if (!r.IsSucceed) return r;
                HandlerFile(item, row);
            }
            return HandlerAdd(infos);
        }
        protected virtual ResultInfo CheckForBusiness(T item, DataRow row)
        {
            ResultInfo r = UnknowResult();
            if (row == null)
            {
                r.ResultCode = ResultCode.ValidateException;
                r.Description = string.Format("{0}:该笔Shipment ID系统不存在,请验证数据!", item.ShipmentID);
                return r;
            }
            return CheckPartyInfo(item);
        }

        protected virtual string GetBackupName()
        {
            return "Document";
        }

        protected virtual ResultInfo HandlerFile(T info, DataRow smRow)
        {
            ResultInfo r = UnknowResult();
            string filePath = SaveFile(info);
            r = UploadFile(info, smRow, filePath);
            if (!r.IsSucceed)
            {
                r.IsSucceed = false;
                r.ResultCode = "SAVEFILE_ERROR";
                r.Description = string.Format("{0}{1}{2}", r.Description, Environment.NewLine, r.Description);
            }
            return r;
        }

        protected string SaveFile(string fileData, string fileExtension, string refNO)
        {
            if (string.IsNullOrEmpty(fileData)) return null;
            byte[] imageBytes = Convert.FromBase64String(fileData);
            string fileName = string.Format("{0}_{1}", refNO, DateTime.Now.ToString("HHmmssfff"));
            string filePath = CreateBaseDirectoryFileName(new List<string> { BackupDirName_Import, this.GetType().Name, GetBackupName() },
                fileName, fileExtension);
            CreateDir(filePath);
            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew))
            {
                stream.Write(imageBytes, 0, imageBytes.Length);
            }
            return filePath;
        }

        string SaveFile(T info)
        {
            return SaveFile(info.FileData, info.FileExtension, info.ShipmentID);
        }

        protected ResultInfo UploadFile(string refNo, DataRow smRow, string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || new FileInfo(filePath).Length <= 0)
                return new ResultInfo { ResultCode = "Error", Description = "文件解析失败！" };
            EDocInfo info = Helper.CreateShipmentEDocInfo(smRow);
            info.DocType = "EXBL";
            info.FilePath = filePath;
            try
            {
                EDOCApi.EDOCResultUploadFile uploadResult = EDocHelper.UploadFile2EDOC(info);
                if (uploadResult == null || uploadResult.Status != EDOCApi.DBErrors.DB_SUCCESS)
                {
                    return new ResultInfo
                    {
                        Description = string.Format("{0}：上传文档失败!", refNo)
                    };
                }
                var l = Logger.CreateLog("上传文档成功", this.GetType().Name, refNo);
                l.Data = uploadResult;
                Logger.WriteLog(l);
                return SucceedResult();
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}：上传文档失败!", refNo);
                var l = Logger.CreateLog(msg, this.GetType().Name, refNo);
                l.MsgInfo2 = ex.ToString();
                Logger.WriteLog(l);
                Logger.WriteLog(msg, ex);
                return new ResultInfo
                {
                    Description = string.Format("{0 {1}{2}", msg, Environment.NewLine, ex.Message)
                };
            }
        }

        ResultInfo UploadFile(T dInfo, DataRow smRow, string filePath)
        {
            return UploadFile(dInfo.ShipmentID, smRow, filePath);
        }
    }
}
