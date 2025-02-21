using Business.TPV;
using Models.EDI;
using Prolink.Data;
using Prolink.DataOperation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TrackingEDI;
using TrackingEDI.Business;

namespace Business
{
    /*Allocation:啟動分配管理, FCL/國內運輸都有分配管理,其他的去Search 物流業者分配規則,抓取該筆shipment 使用的Carrier 或 物流業者. 
如果 Incoterm =FOB 則去客戶的交易規則檔抓取該Customer 的設定是否有指定的物流業者,直接帶出. 
2.  訂艙者要決定Shipper/Consignee/Ship to/Notify… 等訊息的正確性*/
    public class AllocationHelper
    {
        public string _uid;
        public string _shipmentid;
        public string _profilecode;
        public string _groupid;
        
        public string _incoterm;
        public string _pol;
        public string _region;
        public string _tranmode;
        public string _cmp;
        public string _term;
        public string _pod;
        public string _week;
        public string _year;
        public int _cntnumber;
        public string _cargotype;

        public string _customer;
        public string _shipper;
        MixedList _ml = null;
        public bool _IsFcCarrier = false;
        
        public AllocationHelper()
        {
        }
        public AllocationHelper(string uid)
        {
            _uid = uid;
            string sql = @"SELECT SMSM.*,(SELECT PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='FC')CUSTOMER,
(SELECT PARTY_NO FROM SMSMPT WHERE SMSMPT.SHIPMENT_ID=SMSM.SHIPMENT_ID AND SMSMPT.PARTY_TYPE='SH')SHIPPER FROM SMSM WHERE U_ID=" + SQLUtils.QuotedStr(uid);
            DataTable maindt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (maindt.Rows.Count <= 0)
            {
                return;
            }
            DataRow dr=maindt.Rows[0];
            _shipmentid = Prolink.Math.GetValueAsString(dr["SHIPMENT_ID"]);
            _profilecode = Prolink.Math.GetValueAsString(dr["PROFILE_CD"]);
            _incoterm = Prolink.Math.GetValueAsString(dr["INCOTERM_CD"]);
            //_pol = Prolink.Math.GetValueAsString(dr["POL_CD"]);
            _region = Prolink.Math.GetValueAsString(dr["REGION"]);
            _tranmode = Prolink.Math.GetValueAsString(dr["TRAN_TYPE"]);
            _cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
            _term = Prolink.Math.GetValueAsString(dr["FRT_TERM"]);
            _pol = Prolink.Math.GetValueAsString(dr["PPOL_CD"]);
            _pod = Prolink.Math.GetValueAsString(dr["PPOD_CD"]);
            _customer = Prolink.Math.GetValueAsString(dr["CUSTOMER"]);
            _shipper = Prolink.Math.GetValueAsString(dr["SHIPPER"]);
            _cntnumber = Prolink.Math.GetValueAsInt(dr["CNT_NUMBER"]);
            _week = Prolink.Math.GetValueAsString(dr["WEEKLY"]);
            _year = Prolink.Math.GetValueAsString(dr["YEAR"]);
            _cargotype = Prolink.Math.GetValueAsString(dr["CARGO_TYPE"]);
            _ml = new MixedList();
        }

        EditInstructList CreateEiList(Dictionary<string, PartnerInfo> parties)
        {
            EditInstructList el = new EditInstructList();
            DataTable partyTypeDT = Helper.QueryPartyType();    
            Func<string, DataRow> getPartyTypeDesc = code =>
            {
                DataRow[] rows = partyTypeDT.Select(string.Format("CD_TYPE='PT' AND CD={0}", SQLUtils.QuotedStr(code)));
                if (rows != null && rows.Length > 0) return rows[0];
                return null;
            };
            DataTable smPartyDT =SMHandle.GetPTBySMUid(_uid);
            Func<string,string, DataRow> getPartyDesc = (code,partyno) =>
            {
                DataRow[] rows = smPartyDT.Select(string.Format("PARTY_TYPE={0} AND PARTY_NO={1}", SQLUtils.QuotedStr(code), SQLUtils.QuotedStr(partyno)));
                if (rows != null && rows.Length > 0) return rows[0];
                return null;
            };
            foreach (var item in parties)
            {
                
                var p = item.Value;

                DataRow partyrow = getPartyDesc(item.Key, p.PartnerNumber);
                if (partyrow != null)
                {
                    continue;
                }

                EditInstruct ei = new EditInstruct("SMSMPT", EditInstruct.INSERT_OPERATION);
                ei.Put("U_ID", Guid.NewGuid().ToString());
                ei.Put("U_FID", _uid);
                ei.Put("SHIPMENT_ID", _shipmentid);
                ei.Put("PARTY_TYPE", item.Key);
                DataRow partyTypeRow = getPartyTypeDesc(item.Key);
                if (partyTypeRow != null)
                {
                    ei.Put("TYPE_DESCP", partyTypeRow["CD_DESCP"]);
                    ei.Put("ORDER_BY", partyTypeRow["ORDER_BY"]);
                }
                ei.Put("PARTY_NO", p.PartnerNumber);
                ei.Put("PARTY_NAME", p.Name1);
                ei.Put("PARTY_NAME2", p.Name2);
                ei.Put("PARTY_NAME3", p.Name3);
                ei.Put("PARTY_NAME4", p.Name4);
                ei.Put("PART_ADDR1", p.Street);
                ei.Put("PART_ADDR2", p.Street2);
                ei.Put("PART_ADDR3", p.Street3);
                ei.Put("PART_ADDR4", p.Street4);
                ei.Put("PART_ADDR5", p.Street5);
                ei.Put("CNTY", p.CountryKey);
                ei.Put("CNTY_NM", p.CountryName);
                ei.Put("CITY", p.CityCode);
                ei.Put("CITY_NM", p.City);
                ei.Put("STATE", p.Region);
                ei.Put("ZIP", p.PostalCode);
                ei.Put("PARTY_MAIL", p.EMail);
                ei.Put("PARTY_ATTN", p.FirstName);
                ei.Put("PARTY_TEL", p.Telephone1);
                string fax = p.Fax;
                if (!string.IsNullOrEmpty(p.FaxExtension))
                    fax = string.Join("#", p.Fax, p.FaxExtension);
                ei.Put("FAX_NO", fax);
                ei.Put("TAX_NO", p.VATNumber);

                if (item.Key.Equals("FS") && _IsFcCarrier)
                {
                    EditInstruct smei = new EditInstruct("SMSM", EditInstruct.UPDATE_OPERATION);
                    smei.PutKey("U_ID", _uid);
                    smei.PutKey("SHIPMENT_ID", _shipmentid);
                    smei.Put("CARRIER", p.PartnerNumber);
                    smei.Put("CARRIER_NM", p.Name1);
                    el.Add(smei);
                }
                el.Add(ei);
            }
            return el;
        }

        public string GetAllocation()
        {
            bool isContainsFS = false;
            Dictionary<string, PartnerInfo> parties = GetBoOrCrProfile();
            if (parties.ContainsKey("FS"))
            {
                isContainsFS = true;
            }
            DataTable dt = QueryLSPR();
            foreach (DataRow dr in dt.Rows)
            {
                if (Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]).Equals("FS"))
                {
                    isContainsFS = true;
                }
            }
            if (!isContainsFS)
            {
                DataTable fcldt = FCLGetCarrier();
                string partytypecr = string.Empty;
                string partynocr = string.Empty;
                foreach (DataRow dr in fcldt.Rows)
                {
                    DataRow newrow = dt.NewRow();

                    partytypecr = Prolink.Math.GetValueAsString(dr["PARTY_TYPE"]);
                    partynocr = Prolink.Math.GetValueAsString(dr["PARTY_NO"]);
                    newrow["PARTY_TYPE"] = partytypecr;
                    newrow["LSP_NO"] = partynocr;
                    dt.Rows.Add(newrow);
                    if (partytypecr.Equals("FS") && partynocr.Equals("0008920007"))
                    {
                        DataRow newsprow = dt.NewRow();
                        newsprow["PARTY_TYPE"] = "SP";
                        newsprow["LSP_NO"] = partynocr;
                        dt.Rows.Add(newsprow);
                    }
                }
            }
            Func<string, string> getPartyCode = t =>
                {
                     DataRow[] rows=dt.Select(string.Format("PARTY_TYPE={0}",SQLUtils.QuotedStr(t)));
                    if(rows==null||rows.Length<=0) return null;
                    return Prolink.Math.GetValueAsString(rows[0]["LSP_NO"]);
                };  
            List<string> list = new List<string>() { "FS", "BO", "CR", "BR","SP" };
            var codes=list.Where(s=>
                {
                    if (parties.ContainsKey(s) && parties[s] != null) return false;
                    return true;
                });
            var items =codes.Select(s=>
                {
                   string code=getPartyCode(s);
                    if(string.IsNullOrEmpty(code)) return null;
                   return new Tuple<string,string>(s, code);
                }).Where(s=>s!=null);
            DataTable ptDT = Helper.QueryPartyDT(items.Select(item => item.Item2));
            Func<string,DataRow> getRow=code=>
                {
                    DataRow[] rows = ptDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(code)));
                    if(rows==null||rows.Length<=0) return null;
                    return rows[0];
                };
            foreach (var item in items)
            {
                DataRow row = getRow(item.Item2);
                if (row == null) continue;
                parties.Add(item.Item1, Helper.CreatePartnerInfo(item.Item1, row));
            }

             EditInstructList el=   CreateEiList(parties);
             for (int i = 0; i < el.Count; i++)
             {
                 _ml.Add(el[i]);
             }
             if (_ml.Count > 0)
             {
                 try
                 {
                     Business.Utils.DBManager.DefaultDB.ExecuteUpdate(_ml);
                     return string.Empty;
                 }
                 catch
                 {
                     return @Resources.Locale.L_AllocationHelper_Business_15;
                 }
             }
             else
             {
                 return @Resources.Locale.L_AllocationHelper_Business_16;
             }
        }

        DataTable QueryLSPR()
        {
            string conditions=string.Format("TRAN_MODE={0} AND CMP={1} AND POL={2} AND TERM={3}",
                SQLUtils.QuotedStr(_tranmode),SQLUtils.QuotedStr(_cmp),SQLUtils.QuotedStr(_pol),SQLUtils.QuotedStr(_term));

            string sql = "SELECT * FROM SMLSPR WHERE 1=1 AND " + conditions;

            if (_term.Equals("C"))
            {
                string newcondition = string.Empty;
                if (!string.IsNullOrEmpty(_customer))
                {
                    newcondition = conditions + string.Format(" AND CUSTOMER={0}", SQLUtils.QuotedStr(_customer));
                    string sql1 = "SELECT * FROM SMLSPR WHERE 1=1 AND " + newcondition;
                    DataTable Cdt = OperationUtils.GetDataTable(sql1, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                    if (Cdt.Rows.Count > 0)
                        return Cdt;
                }
            }

            //处理 _term 为P或O的
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dtNew = new DataTable();
            Func<DataTable, DataRow> getPartyCode=(datatable) =>
            {
                DataRow[] rows = datatable.Select("PARTY_TYPE='SP'");
                if (rows == null || rows.Length <= 0)
                    rows = datatable.Select("PARTY_TYPE='BR'");
                if (rows == null || rows.Length <= 0)
                    rows = datatable.Select("PARTY_TYPE='CR'");
                if (rows == null || rows.Length <= 0)
                    return null;
                return rows[0];
            };

            Func<string,DataTable,DataRow,DataRow> getalParty=(filed,datatable,defaultrow) =>{
                DataRow[] rows = datatable.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(filed)));
                if (rows == null || rows.Length <= 0)
                    return defaultrow;
                return rows[0];
            };

            Action<string, DataTable, DataRow,DataTable> setalParty = (filed, datatable, defaultrow,newdt) =>
            {
                DataRow dr = getalParty(filed, datatable, defaultrow);
                if(dr==null)
                    dr=defaultrow;
                dr["PARTY_TYPE"] = filed;
                newdt.Rows.Add(dr.ItemArray);
            };
            if ("E".Equals(_tranmode))  //如果是国际快递
            {
                
                string countrycd = string.Empty;
                if (_pod.Length > 2) countrycd = _pod.Substring(0, 2);
                sql = string.Format("SELECT * FROM SMLSPR WHERE TRAN_MODE={0} AND CMP={1} AND POD='{2}ZZZ'",
                    SQLUtils.QuotedStr(_tranmode), SQLUtils.QuotedStr(_cmp), countrycd);
                DataTable expdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

                dtNew = expdt.Copy();  //复制dt表数据结构
                dtNew.Clear();  //清楚数据 
                if (expdt.Rows.Count > 0)
                {
                    DataRow spdr=getPartyCode(expdt);
                    if (spdr != null)
                    {
                        setalParty("SP", expdt, spdr, dtNew);
                        setalParty("BR", expdt, spdr, dtNew);
                        setalParty("CR", expdt, spdr, dtNew);
                    }
                }
                return dtNew;
            }
            else
            {
                dtNew = RestDtTable(dt, string.Format(" POD={0}", SQLUtils.QuotedStr(_pod)));
            }
            if (dtNew.Rows.Count > 0) return dtNew;
            dtNew = RestDtTable(dt, string.Format(" CUSTOMER={0}", SQLUtils.QuotedStr(_customer)));
            if (dtNew.Rows.Count > 0) return dtNew;
            return dt;
        }

        DataTable RestDtTable(DataTable dt,string condition)
        {
            DataRow [] drs=dt.Select(condition);
            DataTable newdt = dt.Clone();
            for (int i = 0; i < drs.Length; i++)
            {
                newdt.Rows.Add(drs[i].ItemArray);
            }
            return newdt;
        }

        Dictionary<string, PartnerInfo> GetBoOrCrProfile()
        {
            Dictionary<string, PartnerInfo> parties=new Dictionary<string,PartnerInfo>();
            Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);
            if (sipInfo == null)
            {
                return parties;
            }
            if (_tranmode.Equals("F"))
            {
                if (sipInfo.Parties.ContainsKey("FS"))
                {
                    parties.Add("FS", sipInfo.Parties["FS"]);
                }
            }
            if (sipInfo.Parties.ContainsKey("BO"))
                parties.Add("BO", sipInfo.Parties["BO"]);
            return parties;
        }

        DataTable FCLGetCarrier()
        {
            string sql = "SELECT PARTY_TYPE,PARTY_NO FROM SMSMPT WHERE 1=0";
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (_tranmode!="F")
            {
                return dt;
            }
            string conditions = string.Format("AND TRAN_MODE={0} AND YEAR={1} AND WEEK={2} AND POL={3} AND POD={4} ",
                    SQLUtils.QuotedStr(_tranmode),
                    SQLUtils.QuotedStr(_year),
                    SQLUtils.QuotedStr(_week),
                    SQLUtils.QuotedStr(_pol),
                    SQLUtils.QuotedStr(_pod));
            string orderby = " ORDER BY ORDER_BY ASC";
            sql = "SELECT * FROM SMFCC WHERE (CAST(FFEU AS INT)>CASE WHEN AFEU IS NULL THEN 0 ELSE CAST(AFEU AS INT) END) ";
            sql += conditions + orderby;
            DataTable smfccDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataRow smfccrow = null;
            if (smfccDt.Rows.Count > 0)
            {
                smfccrow = smfccDt.Rows[0];
            }
            else
            {
                return dt;
            }
            DataRow[] smfccrows = smfccDt.Select(string.Format("FREIGHT_TERM ={0} AND REGION={1}", SQLUtils.QuotedStr(_term), SQLUtils.QuotedStr(_region)));
            if (smfccrows.Length > 0)
            {
                smfccrow = smfccrows[0];
                conditions += string.Format("FREIGHT_TERM ={0} AND REGION={1}", SQLUtils.QuotedStr(_term), SQLUtils.QuotedStr(_region));
            }
            else
            {
                smfccrows = smfccDt.Select(string.Format("FREIGHT_TERM ={0}", SQLUtils.QuotedStr(_term)));
                if (smfccrows.Length > 0)
                {
                    smfccrow = smfccrows[0];
                    conditions += string.Format("FREIGHT_TERM ={0}", SQLUtils.QuotedStr(_term));
                }
            }
            string partyno = Prolink.Math.GetValueAsString(smfccrow["LSP_CD"]);
            string carrier = Prolink.Math.GetValueAsString(smfccrow["CARRIER"]);
            int smfccAfeu = Prolink.Math.GetValueAsInt(smfccrow["AFEU"]);

            DataRow newrow = dt.NewRow();
            newrow["PARTY_TYPE"]="FS";
            newrow["PARTY_NO"] = carrier;
            dt.Rows.Add(newrow);
            _IsFcCarrier = true;

           
            int count = smfccAfeu + _cntnumber;
            string updatesmfcc = "UPDATE SMFCC SET AFEU='" + count + "' WHERE 1=1 " + conditions + " AND CARRIER='" + carrier + "' AND LSP_CD='" + partyno + "'";
            _ml.Add(updatesmfcc);
            return dt;
        }

        public string GetDTAllocation()
        {
            Dictionary<string, PartnerInfo> parties = GeTruckProfile();
            DataTable dt = QuerySMDLSPR();
         
            Func<string, string> getPartyCode = t =>
            {
                DataRow[] rows = dt.Select(string.Format("PARTY_TYPE={0}", SQLUtils.QuotedStr(t)));
                if (rows == null || rows.Length <= 0) return null;
                return Prolink.Math.GetValueAsString(rows[0]["LSP_NO"]);
            };
            List<string> list = new List<string>() { "CR" };
            var codes = list.Where(s =>
            {
                if (parties.ContainsKey(s) && parties[s] != null) return false;
                return true;
            });
            var items = codes.Select(s =>
            {
                string code = getPartyCode(s);
                if (string.IsNullOrEmpty(code)) return null;
                return new Tuple<string, string>(s, code);
            }).Where(s => s != null);
            DataTable ptDT = Helper.QueryPartyDT(items.Select(item => item.Item2));
            Func<string, DataRow> getRow = code =>
            {
                DataRow[] rows = ptDT.Select(string.Format("PARTY_NO={0}", SQLUtils.QuotedStr(code)));
                if (rows == null || rows.Length <= 0) return null;
                return rows[0];
            };
            foreach (var item in items)
            {
                DataRow row = getRow(item.Item2);
                if (row == null) continue;
                parties.Add(item.Item1, Helper.CreatePartnerInfo(item.Item1, row));
            }

            EditInstructList el = CreateEiList(parties);
            if (el.Count > 0)
            {
                try
                {
                    Business.Utils.DBManager.DefaultDB.ExecuteUpdate(el);
                    return string.Empty;
                }
                catch
                {
                    return @Resources.Locale.L_AllocationHelper_Business_17;
                }
            }
            else
            {
                return @Resources.Locale.L_AllocationHelper_Business_17;
            }
        }

        DataTable QuerySMDLSPR()
        {
            DataTable smdlsprdt = new DataTable();
            string conditons = string.Format("PICKUP_PORT={0} AND DELIVERY_PORT={1} AND TERM={2}", SQLUtils.QuotedStr(_pol), SQLUtils.QuotedStr(_pod), SQLUtils.QuotedStr(_cargotype));
            string sql = string.Empty;
            if (!string.IsNullOrEmpty(_customer))
            {
                sql = string.Format("SELECT LSP_NO,LSP_NM,'CR' AS PARTY_TYPE FROM SMDLSPR WHERE {0} AND CUSTOMER={1}",
                    conditons,
               SQLUtils.QuotedStr(_customer));
                smdlsprdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            if (smdlsprdt.Rows.Count <= 0)
            {
                sql = string.Format("SELECT LSP_NO,LSP_NM,'CR' AS PARTY_TYPE FROM SMDLSPR WHERE {0} AND (CUSTOMER IS NULL OR CUSTOMER='')",
               conditons);
                smdlsprdt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            }
            return smdlsprdt;


            //处理 _term 为P或O的
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable dtNew = RestDtTable(dt, string.Format(" POD={0}", SQLUtils.QuotedStr(_pod)));
            if (dtNew.Rows.Count > 0) return dtNew;
            dtNew = RestDtTable(dt, string.Format(" CUSTOMER={0}", SQLUtils.QuotedStr(_customer)));
            if (dtNew.Rows.Count > 0) return dtNew;
            return dt;
        }

        Dictionary<string, PartnerInfo> GeTruckProfile()
        {
            Dictionary<string, PartnerInfo> parties = new Dictionary<string, PartnerInfo>();
            Business.TPV.SIProfileInfo sipInfo = Business.TPV.Helper.QuerySIProfileInfo(_profilecode);
            if (sipInfo == null)
            {
                return parties;
            }
            if (_tranmode.Equals("T"))
            {
                if (sipInfo.Parties.ContainsKey("BO"))
                {
                    parties.Add("CR", sipInfo.Parties["BO"]);
                }
            }
            return parties;
        }

        public string ChangeAllocation(string uid)
        {
            string msg = string.Empty;
            string[] uids = uid.Split(',');
            string sql = string.Format("SELECT DISTINCT YEAR,WEEK,CMP,REGION,POD,POL FROM SMFCM WHERE U_ID IN {0}", SQLUtils.Quoted(uids));
            DataTable dt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());

            foreach (DataRow dr in dt.Rows)
            {
                string year = Prolink.Math.GetValueAsString(dr["YEAR"]);
                string week = Prolink.Math.GetValueAsString(dr["WEEK"]);
                string cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                string region = Prolink.Math.GetValueAsString(dr["REGION"]);
                string pod = Prolink.Math.GetValueAsString(dr["POD"]);
                string pol = Prolink.Math.GetValueAsString(dr["POL"]);
                msg += ChangeCarrierItem(year,week,cmp,region,pod,pol);
            }
            return msg;
        }

        private string ChangeCarrierItem(string year, string week, string cmp, string region, string pod, string pol)
        {
            string msg = string.Empty;
            int fcmffeu = 0;
            
            string sql=string.Format("SELECT SUM(FFEU) FROM SMFCM WHERE YEAR={0} AND WEEK={1} AND CMP={2} AND REGION={3} AND POD={4} AND POL={5}",
                SQLUtils.QuotedStr(year),
                SQLUtils.QuotedStr(week),
                SQLUtils.QuotedStr(cmp),
                SQLUtils.QuotedStr(region),
            SQLUtils.QuotedStr(pod),
            SQLUtils.QuotedStr(pol));
            fcmffeu = OperationUtils.GetValueAsInt(sql, Prolink.Web.WebContext.GetInstance().GetConnection());


            sql = string.Format("SELECT TOP 1 * FROM SMFCM WHERE YEAR={0} AND WEEK={1} AND CMP={2} AND REGION={3} AND POD={4} AND POL={5}",
                SQLUtils.QuotedStr(year),
                SQLUtils.QuotedStr(week),
                SQLUtils.QuotedStr(cmp),
                SQLUtils.QuotedStr(region),
            SQLUtils.QuotedStr(pod),
            SQLUtils.QuotedStr(pol));
            DataTable fcmData = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (fcmData.Rows.Count > 0)
            {
                DataRow dr = fcmData.Rows[0];
                year = Prolink.Math.GetValueAsString(dr["YEAR"]);
                week = Prolink.Math.GetValueAsString(dr["WEEK"]);
                cmp = Prolink.Math.GetValueAsString(dr["CMP"]);
                region = Prolink.Math.GetValueAsString(dr["REGION"]);
                pod = Prolink.Math.GetValueAsString(dr["POD"]);
                pol = Prolink.Math.GetValueAsString(dr["POL"]);
                //fcmffeu = Prolink.Math.GetValueAsInt(dr["FFEU"]);
            }

            sql = string.Format("SELECT * FROM SMCNP WHERE YEAR={0} AND WEEK={1} AND CMP={2} AND REGION={3} AND POD={4} AND POL={5}",
                SQLUtils.QuotedStr(year),
                SQLUtils.QuotedStr(week),
                SQLUtils.QuotedStr(cmp),
                SQLUtils.QuotedStr(region),
            SQLUtils.QuotedStr(pod),
            SQLUtils.QuotedStr(pol));
            DataTable cnpData = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            if (cnpData.Rows.Count <= 0)
            {
                msg = @Resources.Locale.L_AllocationHelper_Business_18;
                return msg;
            }
            DataRow cnprow = cnpData.Rows[0];
            int[] no = new int[5] { 0, 0, 0, 0, 0 };
            no[0] = Prolink.Math.GetValueAsInt(cnprow["NO1"]);
            no[1] = Prolink.Math.GetValueAsInt(cnprow["NO2"]);
            no[2] = Prolink.Math.GetValueAsInt(cnprow["NO3"]);
            no[3] = Prolink.Math.GetValueAsInt(cnprow["NO4"]);
            no[4] = Prolink.Math.GetValueAsInt(cnprow["NO5"]);

            //sql = string.Format("SELECT RFQ_NO FROM SMRQM WHERE TRAN_MODE='F' AND RLOCATION={0}  AND STATUS='D' ORDER BY RFQ_TO DESC",// AND PERIOD='R' AND POD_CD={1} AND POL_CD={2}",
            //    SQLUtils.QuotedStr(cmp));


            ////sql = string.Format("SELECT RFQ_NO FROM SMRQM WHERE 1=1 AND RLOCATION={0} AND PERIOD='R' AND POD_CD={1} AND POL_CD={2}",
            ////    SQLUtils.QuotedStr(cmp),
            // //   SQLUtils.QuotedStr(pod),
            ////    SQLUtils.QuotedStr(pol));
            //string rfqno = OperationUtils.GetValueAsString(sql, Prolink.Web.WebContext.GetInstance().GetConnection());
            //sql = string.Format("SELECT  TOP 5 * FROM SMQTD  WHERE  RFQ_NO={0} AND POD_CD={1} AND  POL_CD={2} ORDER BY L3 ",
            //    SQLUtils.QuotedStr(rfqno),
            //    SQLUtils.QuotedStr(pod),
            //     SQLUtils.QuotedStr(pol));
            //DataTable QTMData = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            DataTable QTMData = CommonManager.GetLowestPrice(int.Parse(year), int.Parse(week), pol, pod, region);
            if (QTMData.Rows.Count <= 0)
            {
                return @Resources.Locale.L_AllocationHelper_Business_19;
            }
            int j = 0;
            MixedList mixedlist = new MixedList();
            for (int i = 0; i < no.Length; i++)
            {
                int number = no[i];
                if (number == 0)
                {
                    continue;
                }
                string carrrier = GetColumnValue(QTMData, j, "CARRIER");
                string lspno = GetColumnValue(QTMData, j, "LSP_CD");
                if (string.IsNullOrEmpty(carrrier))
                {
                    continue;
                }
                InsertInToSMFC(mixedlist, fcmData, number, carrrier,lspno, fcmffeu,j+1);
                j++;
            }
            if (mixedlist.Count > 0)
            {
                try
                {
                    int[] result = OperationUtils.ExecuteUpdate(mixedlist, Prolink.Web.WebContext.GetInstance().GetConnection());
                }
                catch (Exception ex)
                {
                    msg = ex.ToString();
                }
            }
            return msg;
        }

        public string GetColumnValue(DataTable QTMData,int rowindex,string column)
        {
            if (rowindex < QTMData.Rows.Count)
            {
                return Prolink.Math.GetValueAsString(QTMData.Rows[rowindex]["LSP_CD"]);
            }
            else
            {
                return "";
            }
        }

        public void InsertInToSMFC(MixedList ml, DataTable fcmData, int number, string carrrier,string lspno,int fcmffeu,int seq)
        {
            DataRow fcmDatarow = fcmData.Rows[0];
            string year = Prolink.Math.GetValueAsString(fcmDatarow["YEAR"]);
            string month = Prolink.Math.GetValueAsString(fcmDatarow["MONTH"]);
            string week = Prolink.Math.GetValueAsString(fcmDatarow["WEEK"]);
            string pol = Prolink.Math.GetValueAsString(fcmDatarow["POL"]);
            string region = Prolink.Math.GetValueAsString(fcmDatarow["REGION"]);
            string pod = Prolink.Math.GetValueAsString(fcmDatarow["POD"]);
            string freightterm = Prolink.Math.GetValueAsString(fcmDatarow["FREIGHT_TERM"]);
            string odtype = Prolink.Math.GetValueAsString(fcmDatarow["OD_TYPE"]);
            if (odtype.Equals("O"))
            {
                odtype = "F";
            }
            //string carrier = Prolink.Math.GetValueAsString(fcmDatarow["FREIGHT_TERM"]);
            //int fcmffeu = Prolink.Math.GetValueAsInt(fcmDatarow["FFEU"]);
            int ffeu = number * fcmffeu / 100;

            EditInstruct ei = new EditInstruct("SMFCC", EditInstruct.INSERT_OPERATION);
            ei.Put("U_ID", Guid.NewGuid().ToString());
            ei.Put("YEAR", year);
            ei.Put("MONTH", month);
            ei.Put("TRAN_MODE", odtype);
            ei.Put("WEEK", week);
            ei.Put("POL", pol);
            ei.Put("REGION", region);
            ei.Put("POD", pod);
            ei.Put("FREIGHT_TERM", freightterm);
            ei.Put("FFEU", ffeu);
            ei.Put("CARRIER", carrrier);
            ei.Put("LSP_CD", lspno);
            ei.Put("ORDER_BY", seq); //排名
            ei.Put("CREATE_BY", "Sys");
            ei.PutDate("CREATE_DATE", DateTime.Now);
            ml.Add(ei);
        }
    }
}