using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prolink.Data;
using System.Data;
using Prolink.DataOperation;
using Prolink.V6.Model;
using Prolink.Model;
using Prolink.Web.Services;
using System.Xml.Linq;
using System.Xml;

namespace Business
{
    public class POProcess
    {
        /// <summary>
        /// 获取PO List
        /// </summary>

        /// <returns></returns>
        SMPOMModel pomModel;
        SMPODModel podModel;
        RootModel rootModel;
        public RootModel GetPOList(string buyerCd, string supplierCd,string groupId,string cmp,string stn,string fromCd,string toCd)
        {
            //string sql = string.Format("SELECT * FROM SMPOM WHERE BUYER_CD={0} AND SUPPLIER_CD={1} AND GROUP_ID={2} AND CMP={3} AND STN={4} AND FROM_CD={5} AND TO_CD={6}"
            //    , SQLUtils.QuotedStr(buyerCd) 
            //    , SQLUtils.QuotedStr(supplierCd)
            //    , SQLUtils.QuotedStr(groupId)
            //    , SQLUtils.QuotedStr(cmp)
            //    , SQLUtils.QuotedStr(stn)
            //    , SQLUtils.QuotedStr(fromCd)
            //    , SQLUtils.QuotedStr(toCd)
            //    );

            string sql = "SELECT PO_NO,U_ID,STATUS,GROUP_ID,CMP,STN,FROM_CD,TO_CD FROM SMPOM WHERE 1=1";
            if (!string.IsNullOrEmpty(buyerCd))
            {
                sql += " AND BUYER_CD = " + SQLUtils.QuotedStr(buyerCd);
            }
            if (!string.IsNullOrEmpty(supplierCd))
            {
                sql += " AND SUPPLIER_CD = " + SQLUtils.QuotedStr(supplierCd);
            }
            if (!string.IsNullOrEmpty(groupId))
            {
                sql += " AND GROUP_ID = " + SQLUtils.QuotedStr(groupId);
            }
            if (!string.IsNullOrEmpty(cmp))
            {
                sql += " AND CMP = " + SQLUtils.QuotedStr(cmp);
            }
            if (!string.IsNullOrEmpty(stn))
            {
                sql += " AND STN = " + SQLUtils.QuotedStr(stn);
            }
            if (!string.IsNullOrEmpty(fromCd))
            {
                sql += " AND FROM_CD = " + SQLUtils.QuotedStr(fromCd);
            }
            if (!string.IsNullOrEmpty(toCd))
            {
                sql += " AND TO_CD = " + SQLUtils.QuotedStr(toCd);
            }

            DataTable pomDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            HierarchicalMap[] pomHM = ModelData.CreateHierarchicalMap(pomDt, typeof(SMPOMModel));

            rootModel = (RootModel)HierarchicalMap.CreateFromXml("<root></root>", typeof(RootModel));
            ModelList mlist = rootModel.NewChildren(typeof(SMPOMModel).FullName);

            for (int a = 0; a < pomDt.Rows.Count; a++)
            {
                pomModel = (SMPOMModel)HierarchicalMap.CreateFromXml(pomHM[a].ToXml(), typeof(SMPOMModel));
                pomModel.Name = "SMPOMModel";

                mlist.Name = "SMPOMModel";
                //mlist.Name = "Prolink.iFreight.Model.Ocean.Export.WayBillData";
                mlist.Add(pomModel);



                sql = string.Format("SELECT B.PO_NO,B.SUPPLIER_CD,B.SUPPLIER_NM,A.BPART_NO,A.SPART_NO,A.DESCRIPTION,A.BRAND,A.QTY,A.QTYU,A.OQTY,A.DQTY,A.BQTY,A.SPQTY FROM SMPOD A LEFT JOIN SMPOM B on A.PO_NO = B.PO_NO WHERE A.U_FID = {0} ", SQLUtils.QuotedStr(pomDt.Rows[a]["U_ID"].ToString()));
                DataTable podDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
                HierarchicalMap[] podHM = ModelData.CreateHierarchicalMap(podDt, typeof(SMPODModel));
                ModelList list = pomModel.NewChildren(typeof(SMPODModel).FullName);

                for (int i = 0; i < podDt.Rows.Count; i++) {
                    podModel = (SMPODModel)HierarchicalMap.CreateFromXml(podHM[i].ToXml(), typeof(SMPODModel));
                    podModel.Name = "SMPODModel";

                    list.Name = "SMPODModel";
                    list.Add(podModel);
                }
                pomModel.Nodes.Add(list);
            }
            //string xml = pomModel.ToXml();
            rootModel.Nodes.Add(mlist);
            return rootModel;
        
          }

        public RootModel GetPODetail(string poNo)
        {

            string sql = string.Format("SELECT B.PO_NO,B.SUPPLIER_CD,B.SUPPLIER_NM,A.BPART_NO,A.SPART_NO,A.DESCRIPTION,A.BRAND,A.QTY,A.QTYU,A.OQTY,A.DQTY,A.BQTY,A.SPQTY FROM SMPOD A LEFT JOIN SMPOM B on A.PO_NO = B.PO_NO WHERE A.PO_NO={0}", SQLUtils.QuotedStr(poNo));
            DataTable podDt = OperationUtils.GetDataTable(sql, null, Prolink.Web.WebContext.GetInstance().GetConnection());
            HierarchicalMap[] podHM = ModelData.CreateHierarchicalMap(podDt, typeof(SMPODModel));
            rootModel = (RootModel)HierarchicalMap.CreateFromXml("<root></root>", typeof(RootModel));
            ModelList list = new ModelList();

            for (int i = 0; i < podDt.Rows.Count; i++)
            {
                podModel = (SMPODModel)HierarchicalMap.CreateFromXml(podHM[i].ToXml(), typeof(SMPODModel));
                podModel.Name = "SMPODModel";

                list.Name = "SMPODModel";
                list.Add(podModel);
            }

            rootModel.Nodes.Add(list);
            return rootModel;
        }
    }


    public class RootModel : ModelData
    {
        public override Type GetChildType(string name)
        {
            name = GetClassNameFromName(name);
            if (typeof(SMPOMModel).FullName == name) return typeof(SMPOMModelList);
            return base.GetChildType(name);
        }
    }

    /// <summary>
    /// 取件联系人信息
    /// </summary>
    public class SMPOMModel : ModelData
    {
        public override Type GetChildType(string name)
        {
            name = GetClassNameFromName(name);
            if (typeof(SMPODModel).FullName == name) return typeof(SMPODModelList);
            return base.GetChildType(name);
        }
    }

    /// <summary>
    /// 取件联系人信息
    /// </summary>
    public class SMPODModel : ModelData
    {
        
    }

    public class SMPODModelList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(SMPODModel);
        }
    }
    public class SMPOMModelList : ModelList
    {
        public override Type GetChildType(string name)
        {
            return typeof(SMPOMModel);
        }
    }
}

