using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.V3;
using System.Data;
using Prolink.DataOperation;
using System.Collections;
using Prolink.Web;

namespace WebGui.App_Start
{
    public class DBConfig
    {
        public static void Register(HttpServerUtility Server, HttpApplicationState Application)
        {
            Hashtable prop = new Hashtable();
            string serverPath = Server.MapPath(@"~");
            prop[WebContext.CONFIGURE_FILE_PATH] = System.IO.Path.Combine(serverPath, "Config/Config.xml");
            prop[WebContext.APP_PATH] = serverPath;

            //Prolink.Web.WebContext.Build(Server.MapPath("~\\Config\\Config.xml"));
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

            Prolink.EDOC_API.initEDOC_API();

            RegisterModel();

            try
            {
                OperationUtils.ExecuteUpdate("DELETE ROLE_REBUID WHERE IO_TYPE='I'", Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            catch { }

            #region 获取金银铜报价
            //错误日志log
            //WebGet.RestartUrl = HttpContext.Current.Request.Url.ToString();
            Prolink.Log.DefaultLogger weblogger = new Prolink.Log.DefaultLogger(System.IO.Path.Combine(serverPath, "Log/web"));
            //WebGet.Run(weblogger);
            #endregion
        }

        /// <summary>
        /// 子表先定义  然后定义主表
        /// </summary>
        public static void RegisterModel()
        {
            //注册名为  SysAcctModel的实体，主键是 JOB_NO 对应的表是 COBK  栏位为全部
            Model model = new Model("U_ID,GROUP_ID,CMP") { Name = "SysAcctModel", Table = "SYS_ACCT", Field = "*" };
            ModelFactory.Register(model);

            //注册名为  ModBulletinModel的实体，主键是 BULL_ID,GROUP_ID,CMP,STN" 对应的表是 MOD_BULLETIN  栏位为全部
            model = new Model("U_ID") { Name = "ModBulletinModel", Table = "MOD_BULLETIN", Field = "*" };
            ModelFactory.Register(model);

            //注册名为  SysRoleModel的实体，主键是 FID,GROUP_ID,CMP,STN" 对应的表是 MOD_BULLETIN  栏位为全部
            model = new Model("FID,GROUP_ID,CMP") { Name = "SysRoleModel", Table = "SYS_ROLE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveModel", Table = "APPROVE_FLOW_M", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CD_TYPE") { Name = "BscodeKindModel", Table = "BSCODE_KIND", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,CMP_ID") { Name = "ApprovedModel", Table = "APPROVE_FLOW_D", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CD_TYPE,CD") { Name = "BsCodeModel", Table = "BSCODE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("GROUP_ID,CMP,STN,DEP") { Name = "SysSiteModel", Table = "SYS_SITE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("ID,SYS_CODE,PROG_CODE,GROUP_ID,CMP,STN") { Name = "NoticeModel", Table = "SYS_NOTICE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "RouteModel", Table = "TKRUM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,SEQ_NO") { Name = "SubRouteModel", Table = "TKRUD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CUR") { Name = "CurrencyModel", Table = "BSCUR", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("ETYPE,EDATE,FCUR,TCUR") { Name = "ExchangeRateModel", Table = "BSERATE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("PORT_CD,CNTRY_CD") { Name = "CitySetupModel", Table = "BSCITY", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CNTRY_CD,PORT_CD") { Name = "TruckPortSetupModel", Table = "BSTPORT", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CNTRY_CD,PORT_CD,FACTORY,SHIP_TO") { Name = "BsDestModel", Table = "BSDEST", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CNTRY_CD,PORT_CD,FACTORY,SHIP_TO") { Name = "DestAddrModel", Table = "DEST_ADDR", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("GROUP_ID,CMP,STN,CNTRY_CD") { Name = "CntyModel", Table = "BSCNTY", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("GROUP_ID,CMP") { Name = "SysCmpModel", Table = "SYS_CMP", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("SHIPMENT_ID,STS_CD,EVEN_DATE,GROUP_ID,CMP,STN,DEP") { Name = "TkblstModel", Table = "TKBLST", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("STS_CD") { Name = "TkStscdModel", Table = "TKSTSCD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmcnpModel", Table = "SMCNP", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmcnuModel", Table = "SMCNU", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmlsprModel", Table = "SMLSPR", Field = "*" };
            ModelFactory.Register(model);

            //Table :TKPEM( Party Even File )貨況通知檔
            model = new Model("U_ID,SEQ_NO,GROUP_ID,CMP,PARTY_TYPE,NOTIFY_CD,RERQUEST_CD") { Name = "PartyEvenModel", Table = "TKPEM", Field = "*" };
            ModelFactory.Register(model);

            //9)	TKB030 Party Document Setup  (電子文檔設定) 
            model = new Model("U_ID") { Name = "PDocModel", Table = "TKPDM", Field = "*" };
            ModelFactory.Register(model);

            //電子文檔設定檔 系统
            model = new Model("U_ID,DOC_TYPE,GROUP_ID,CMP") { Name = "PDocSubModel", Table = "TKPDD", Field = "*" };
            ModelFactory.Register(model);

            //提单
            model = new Model("U_ID") { Name = "BillofLadingModel", Table = "TKBL", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID,PARTY_TYPE,PARTY_NO") { Name = "BlPartyModel", Table = "TKBLPT", Field = "*" };
            ModelFactory.Register(model);

            //TKPMG(郵件群組檔)
            model = new Model("U_ID") { Name = "PGroupMailModel", Table = "TKPMG", Field = "*" };
            ModelFactory.Register(model);

            //提单货况
            model = new Model("SHIPMENT_ID,STS_CD,EVEN_DATE") { Name = "StatusModel", Table = "TKBLST", Field = "*" };
            ModelFactory.Register(model);

            //提單貨櫃
            model = new Model("JOB_NO,SEQ_NO") { Name = "ContainerModel", Table = "TKBLCNTR", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmfcmModel", Table = "SMFCM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmfccModel", Table = "SMFCC", Field = "*" };
            ModelFactory.Register(model);


            model = new Model("EVEN_NO") { Name = "EvenModel", Table = "TKEVM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "EvenRecordModel", Table = "TKEVD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmptyModel", Table = "SMPTY", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID") { Name = "SmptycModel", Table = "SMPTYC", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID") { Name = "SmptysModel", Table = "SMPTYS", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID,SEQ_NO") { Name = "SmptydModel", Table = "SMPTYD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID,SEQ_NO") { Name = "TkcstsModel", Table = "TKCSTS", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmdnModel", Table = "SMDN", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmdnpModel", Table = "SMDNP", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID") { Name = "SmdnsModel", Table = "SMDNS", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmdnptModel", Table = "SMDNPT", Field = "*" };
            ModelFactory.Register(model);
            //Mail格式設定
            model = new Model("U_ID") { Name = "TKPMTModel", Table = "TKPMT", Field = "*" };
            ModelFactory.Register(model);
            //TRACKING 公司建档
            model = new Model("CMP_ID,GROUP_ID") { Name = "TKCMPModel", Table = "TK_CMP", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmdlsprModel", Table = "SMDLSPR", Field = "*" };  
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveRecordModel", Table = "APPROVE_RECORD", Field = "*" };
            ModelFactory.Register(model);

            //model = new Model("U_ID") { Name = "SmrvModel", Table = "SMRV", Field = "*" };
            //ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmsmptModel", Table = "SMSMPT", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmsmModel", Table = "SMSM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmwhModel", Table = "SMWH", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmwhgtModel", Table = "SMWHGT", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmrqmModel", Table = "SMRQM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmrqdModel", Table = "SMRQD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveAttrModel", Table = "APPROVE_ATTRIBUTE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveAttrDModel", Table = "APPROVE_ATTR_D", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveAttrDPModel", Table = "APPROVE_ATTR_DP", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmInmModel", Table = "SMINM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmIndModel", Table = "SMIND", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmInpModel", Table = "SMINP", Field = "*" };
            ModelFactory.Register(model); 

            model = new Model("U_ID") { Name = "SmqtmModel", Table = "SMQTM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmqtdModel", Table = "SMQTD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "BsStateModel", Table = "BSSTATE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("FLAG,CNTY,PORT") { Name = "TpvPortModel", Table = "TPVPORT", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ExpressModel", Table = "SMEXM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "BsTruckcModel", Table = "BSTRUCKC", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BsTruckdModel", Table = "BSTRUCKD", Field = "*" };
            ModelFactory.Register(model);

            //账单实体
            model = new Model("U_ID") { Name = "SmbimModel", Table = "SMBIM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmdndModel", Table = "SMDND", Field = "*" };
            ModelFactory.Register(model);
            

            //客戶交易主檔
            model = new Model("U_ID") { Name = "SmsimModel", Table = "SMSIM", Field = "*" };
            ModelFactory.Register(model);

            //客戶交易明細檔
            model = new Model("U_ID") { Name = "SmsidModel", Table = "SMSID", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "smactnModel", Table = "SMACTN", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "TkstmpModel", Table = "TKSTMP", Field = "*" };
            ModelFactory.Register(model);

            //过账建档Model
            model = new Model("U_ID") { Name = "SMCHGModel", Table = "SMCHG", Field = "*" };
            ModelFactory.Register(model);

            //PO Management
            model = new Model("U_ID") { Name = "SMPOMModel", Table = "SMPOM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SMPODModel", Table = "SMPOD", Field = "*" };
            ModelFactory.Register(model);


            //快遞建档Model
            model = new Model("U_ID") { Name = "SMEXModel", Table = "SMEX_ACCT", Field = "*" };
            ModelFactory.Register(model);

            //報價港口建档Model
            model = new Model("U_ID") { Name = "BSLCPOLModel", Table = "BSLCPOL", Field = "*" };
            ModelFactory.Register(model);

            //燃油费建档Model
            model = new Model("U_ID") { Name = "SMFSCModel", Table = "SMFSC", Field = "*" };
            ModelFactory.Register(model);

            //船公司AREAModel
            model = new Model("U_ID,U_FID") { Name = "BSCAAModel", Table = "BSCAA", Field = "*" };
            ModelFactory.Register(model);

            //船公司AREAModel
            model = new Model("U_ID") { Name = "BSCAAMModel", Table = "BSCAAM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmPostModel", Table = "SMPOST", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmgstmModel", Table = "SMGSTM", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,U_FID") { Name = "SmgstdModel", Table = "SMGSTD", Field = "*" };
            ModelFactory.Register(model);
            //異常關係Model
            model = new Model("U_ID") { Name = "ExprelaModel", Table = "EXPRELA", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID,U_FID") { Name = "SmcuftModel", Table = "SMCUFT", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID,U_FID") { Name = "SmicuftModel", Table = "SMICUFT", Field = "*" };
            ModelFactory.Register(model);

            //成本中心Model
            model = new Model("U_ID") { Name = "SMCCModel", Table = "SMCC", Field = "*" };
            ModelFactory.Register(model);

            //帐单明细Model
            model = new Model("U_ID") { Name = "SmbidModel", Table = "SMBID", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbdModel", Table = "SMBD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbddModel", Table = "SMBDD", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "EdiTargetModel", Table = "EDI_TARGET", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "EdiListModel", Table = "EDI_LIST", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SysQaModel", Table = "SYS_QA", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbkinfoModel", Table = "SMBKINFO", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmmaterialModel", Table = "SMMATERIAL", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "EDMTModel", Table = "EDM_TPLT", Field = "*" };
            ModelFactory.Register(model);
            
            //合約管理
            model = new Model("U_ID") { Name = "SMCTMModel", Table = "SMCTM", Field = "*" };
            ModelFactory.Register(model);
            //出險管理
            model = new Model("U_ID") { Name = "SMIPMModel", Table = "SMIPM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SMIPRModel", Table = "SMIPR", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SMIPCModel", Table = "SMIPC", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmptyFilterModel", Table = "SMPTY_FILTER", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "BsrptModel", Table = "BSRPT", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("CNTRY_CD,PORT_CD") { Name = "BSADDRModel", Table = "BSADDR", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ChangeInfoModel", Table = "CHANGE_INFO", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ApproveSignModel", Table = "APPROVE_SIGN", Field = "*" };
            ModelFactory.Register(model);

            #region 进口
            model = new Model("U_ID") { Name = "SmsmiModel", Table = "SMSMI", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmsmiptModel", Table = "SMSMIPT", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmalModel", Table = "SMAL", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BslightmModel", Table = "BSLIGHTM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BslightdModel", Table = "BSLIGHTD", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("RULE_CODE") { Name = "AutoNoRuleModel", Table = "SCS_AUTONO_RULE", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("RULE_CODE") { Name = "AutoNoItemModel", Table = "SCS_AUTONO_ITEM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "EcreffeeModel", Table = "ECREFFEE", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BsdistModel", Table = "BSDIST", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmwhtModel", Table = "SMWHT", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BsdateModel", Table = "BSDATE", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmqtiModel", Table = "SMQTI", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmidnModel", Table = "SMIDN", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmidnpModel", Table = "SMIDNP", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmicntrModel", Table = "SMICNTR", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmrcntrModel", Table = "SMRCNTR", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SmrdnModel", Table = "SMRDN", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "BstermModel", Table = "BSTERM", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "ScmrefModel", Table = "SCMREF", Field = "*" };
            ModelFactory.Register(model);
            #endregion

            model = new Model("U_ID") { Name = "ScmpbModel", Table = "SCMPB", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmirvModel", Table = "SMIRV", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "DestMapModel", Table = "DEST_MAP", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "DirectMapModel", Table = "DIRECT_MAP", Field = "*" };
            ModelFactory.Register(model);
            model = new Model("U_ID") { Name = "SysAcctWhModel", Table = "SYS_ACCT_WH", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("ID") { Name = "SysAcctLogModel", Table = "SYS_ACCT_LOG", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmordForecastModel", Table = "SMORD_FORECAST", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbidDnModel", Table = "SMBID_DN", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbfaModel", Table = "SMBFA", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmbwsModel", Table = "SMBWS", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "BstpsModel", Table = "BSTPS", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "SmidnplModel", Table = "SMIDNPL", Field = "*" };
            ModelFactory.Register(model);
             
            model = new Model("U_ID") { Name = "SminpoModel", Table = "SMINPO", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID,CMP") { Name = "SysCookieModel", Table = "SYS_COOKIE", Field = "*" };
            ModelFactory.Register(model);

            model = new Model("U_ID") { Name = "ScalloModel", Table = "SCALLO", Field = "*" };
            ModelFactory.Register(model);
        }
    }
}