using Models.EDI;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.TPV.RFC
{
    class DNEDI : EDIBase
    {
        public DNInfo GetDN(string sapId, string dnno, string location)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("SRCSYSTEM", sapId);
            parameters.Add("VBELN", dnno);
            IRfcFunction function = GetOperator("ZRFC_ESP_DNINFO", parameters, location);
            DNInfo info = new DNInfo();
            try
            {
                ParseHeader(function, info);
                ParseItem(function, info);
                ParsePrice(function, info);
                ParsePartner(function, info);
                ParseClassfication(function, info);
                ParsePacking(function, info);
            }
            catch
            {
                
            }
            return info;
        }

        void ParseHeader(IRfcFunction function, DNInfo info)
        {
            IRfcStructure header = function.GetStructure("HEADER");
            HeaderInfo headerInfo = new HeaderInfo();
            headerInfo.OriginalCompany = header.GetFieldValueAsString("BUKRS_VKORG_VL");
            headerInfo.MaterialType = header.GetFieldValueAsString("MTART");
            headerInfo.SourceSAP = header.GetFieldValueAsString("SRCSYS");
            headerInfo.DNNOWithCompanyCode = header.GetFieldValueAsString("VBELN");
            headerInfo.CompanyCode = header.GetFieldValueAsString("BUKRS_VF");
            headerInfo.DNNO = header.GetFieldValueAsString("VBELN_VL");
            headerInfo.RefSourceSAP = header.GetFieldValueAsString("SRCSYS_RF");
            headerInfo.RefDNNOWithCompanyCode = header.GetFieldValueAsString("VBELN_RF");
            headerInfo.RefCompanyCode = header.GetFieldValueAsString("BUKRS_VF_RF");
            headerInfo.RefDNNO = header.GetFieldValueAsString("VBELN_VL_RF");
            headerInfo.CreateDate = header.GetFieldValueAsString("ERDAT_VL");
            headerInfo.CreateTime = header.GetFieldValueAsString("ERZET_VL");
            headerInfo.CreateBy = header.GetFieldValueAsString("ERNAM_VL");
            headerInfo.ChangedDate = header.GetFieldValueAsString("AEDAT_VL");
            headerInfo.ChangedTime = header.GetFieldValueAsString("AEZET_VL");
            headerInfo.ChangedBy = header.GetFieldValueAsString("AENAM_VL");
            headerInfo.Uniqueidentifier = header.GetFieldValueAsString("HANDLE");
            headerInfo.SalesDocumentType = header.GetFieldValueAsString("AUART");
            headerInfo.OriginalSalesDocumentType = header.GetFieldValueAsString("AUART_VL");
            headerInfo.OrderReason = header.GetFieldValueAsString("AUGRU");
            headerInfo.SalesOrderNetValue = header.GetFieldValueAsString("NETWR");
            headerInfo.SDCurrency = header.GetFieldValueAsString("WAERK");
            headerInfo.SalesOrganization = header.GetFieldValueAsString("VKORG");
            headerInfo.DistributionChannel = header.GetFieldValueAsString("VTWEG");
            headerInfo.DistributionChannelDescription = header.GetFieldValueAsString("VTTXT");
            headerInfo.Division = header.GetFieldValueAsString("SPART");
            headerInfo.DivisionDescription = header.GetFieldValueAsString("SPTXT");
            headerInfo.SalesGroup = header.GetFieldValueAsString("VKGRP");
            headerInfo.SalesOffice = header.GetFieldValueAsString("VKBUR");
            headerInfo.BusinessArea = header.GetFieldValueAsString("GSBER");
            headerInfo.BusinessAreaFromCostCenter = header.GetFieldValueAsString("GSKST");
            headerInfo.ValidFromDate = header.GetFieldValueAsString("GUEBG");
            headerInfo.ValidToDate = header.GetFieldValueAsString("GUEEN");
            headerInfo.NumberOfTheDocumentCondition = header.GetFieldValueAsString("KNUMV");
            headerInfo.RequestedDeliveryDate = header.GetFieldValueAsString("VDATU");
            headerInfo.ShippingConditions = header.GetFieldValueAsString("VSBED");
            headerInfo.CostCenter = header.GetFieldValueAsString("KOSTL");
            headerInfo.ControllingArea = header.GetFieldValueAsString("KOSTL");
            headerInfo.DocumentNumber = header.GetFieldValueAsString("VGBEL");
            headerInfo.ShippingPoint = header.GetFieldValueAsString("VSTEL");
            headerInfo.ShippingPointDescription = header.GetFieldValueAsString("VSTXT");
            headerInfo.DeliveryType = header.GetFieldValueAsString("LFART");
            headerInfo.PlannedGoodsMovementDate = header.GetFieldValueAsString("WADAT");
            headerInfo.LoadingDate = header.GetFieldValueAsString("LDDAT");
            headerInfo.TransportationPlanningDate = header.GetFieldValueAsString("TDDAT");
            headerInfo.DeliveryDate = header.GetFieldValueAsString("LFDAT");
            headerInfo.PickingDate = header.GetFieldValueAsString("KODAT");
            headerInfo.UnloadingPoint = header.GetFieldValueAsString("ABLAD");
            headerInfo.TermsOfPaymentKey = header.GetFieldValueAsString("ZTERM");
            headerInfo.TermsOfPaymentKeyDescription = header.GetFieldValueAsString("ZTTXT");
            headerInfo.Incoterms1 = header.GetFieldValueAsString("INCO1");
            headerInfo.Incoterms2 = header.GetFieldValueAsString("INCO2");
            headerInfo.TotalWeight = header.GetFieldValueAsString("BTGEW");
            headerInfo.NetWeight = header.GetFieldValueAsString("NTGEW");
            headerInfo.WeightUnit = header.GetFieldValueAsString("GEWEI");
            headerInfo.Volume = header.GetFieldValueAsString("VOLUM");
            headerInfo.VolumeUnit = header.GetFieldValueAsString("VOLEH");
            //headerInfo.TotalNumber = header.GetFieldValueAsString("ANZPK");
            headerInfo.TimeOfDelivery = header.GetFieldValueAsString("LFUHR");
            headerInfo.LoadingPoint = header.GetFieldValueAsString("LSTEL");
            headerInfo.BillingDate = header.GetFieldValueAsString("FKDAT");
            headerInfo.BillOfLading = header.GetFieldValueAsString("BOLNR");
            headerInfo.MeansOfTransportType = header.GetFieldValueAsString("TRATY");
            headerInfo.MeansOfTransportTypeDescription = header.GetFieldValueAsString("TRTXT");
            headerInfo.MeansOfTransportID = header.GetFieldValueAsString("TRAID");
            headerInfo.InsuranceType = header.GetFieldValueAsString("VSART");
            headerInfo.InsuranceTypeDescription = header.GetFieldValueAsString("VATXT");
            headerInfo.DocumentDate = header.GetFieldValueAsString("BLDAT");
            headerInfo.ActualGoodsMovementDate = header.GetFieldValueAsString("WADAT_IST");
            headerInfo.ReceivingPlant = header.GetFieldValueAsString("WERKS");
            headerInfo.TimeZone = header.GetFieldValueAsString("TZONIS");
            headerInfo.TimeZoneOfRecipient = header.GetFieldValueAsString("TZONRC");
            headerInfo.TransactionCode = header.GetFieldValueAsString("TCODE");
            headerInfo.SpecialProcID = header.GetFieldValueAsString("SDABW");
            headerInfo.SpecialDescp = header.GetFieldValueAsString("SWTXT");
            headerInfo.ExportNO = header.GetFieldValueAsString("EXPNR");
            headerInfo.DeclarationNO = header.GetFieldValueAsString("DLFNR");
            headerInfo.ApprovalNO = header.GetFieldValueAsString("RATNO");
            headerInfo.ExportCode = header.GetFieldValueAsString("EXARE");
            headerInfo.ExportDescp = header.GetFieldValueAsString("CUSTD");
            headerInfo.UniCode = header.GetFieldValueAsString("DAVAT");
            headerInfo.Mark1 = header.GetFieldValueAsString("MARK1");
            headerInfo.Mark2 = header.GetFieldValueAsString("MARK2");
            headerInfo.Mark3 = header.GetFieldValueAsString("MARK3");
            headerInfo.Mark4 = header.GetFieldValueAsString("MARK4");
            headerInfo.Mark5 = header.GetFieldValueAsString("MARK5");
            headerInfo.Mark6 = header.GetFieldValueAsString("MARK6");
            headerInfo.OriginalSalesOrg = header.GetFieldValueAsString("VKORG_VL");
            headerInfo.OriginalSoldTo = header.GetFieldValueAsString("KUNAG_VL");
            headerInfo.ProfileCode = header.GetFieldValueAsString("PROFILE");
            headerInfo.DeclarationDate = header.GetFieldValueAsString("CUDAT");
            headerInfo.BankShortKey = header.GetFieldValueAsString("HBKID");
            headerInfo.BankCountryKey = header.GetFieldValueAsString("BANKS ");
            headerInfo.BankKeys = header.GetFieldValueAsString("BANKL");
            headerInfo.BankAccountDetails = header.GetFieldValueAsString("HKTID");
            headerInfo.BankAccountNumber = header.GetFieldValueAsString("BANKN");
            headerInfo.BankAlternativeNumber = header.GetFieldValueAsString("BNKN2");
            headerInfo.BankName = header.GetFieldValueAsString("BANKA");
            headerInfo.BankRegion = header.GetFieldValueAsString("PROVZ");
            headerInfo.BankStreet = header.GetFieldValueAsString("STRAS");
            headerInfo.BankCity = header.GetFieldValueAsString("ORT01");
            headerInfo.BankSwift = header.GetFieldValueAsString("SWIFT");
            headerInfo.BankNumber = header.GetFieldValueAsString("BNKLZ");
            headerInfo.BankCurrentAccountNumber = header.GetFieldValueAsString("PSKTO");
            headerInfo.BankBranch = header.GetFieldValueAsString("BRNCH");
            headerInfo.CustomerIncomingNumber = header.GetFieldValueAsString("VBBKZD71");
            info.HeaderInfo = headerInfo;
        }

        void ParseItem(IRfcFunction function, DNInfo info)
        {
            List<ItemInfo> list = new List<ItemInfo>();
            IRfcTable table = function.GetTable("ITEM_TAB");
            foreach (IRfcStructure row in table)
            {
                ItemInfo itemInfo = new ItemInfo();
                itemInfo.SourceSAP = row.GetFieldValueAsString("SRCSYS");
                itemInfo.DNNOWithCompanyCode = row.GetFieldValueAsString("VBELN");
                itemInfo.CompanyCode = row.GetFieldValueAsString("BUKRS_VF");
                itemInfo.DNNO = row.GetFieldValueAsString("VBELN_VL");
                itemInfo.DeliveryItem = row.GetFieldValueAsString("POSNR_VL");
                itemInfo.CreateDate = row.GetFieldValueAsString("ERDAT_VL");
                itemInfo.CreateTime = row.GetFieldValueAsString("ERZET_VL");
                itemInfo.CreateBy = row.GetFieldValueAsString("ERNAM_VL");
                itemInfo.ChangedDate = row.GetFieldValueAsString("AEDAT_VL");
                itemInfo.ChangedTime = row.GetFieldValueAsString("AEZET_VL");
                itemInfo.ChangedBy = row.GetFieldValueAsString("AENAM_VL");
                itemInfo.SalesDocument = row.GetFieldValueAsString("VBELN_VA");
                itemInfo.SalesDocumentItem = row.GetFieldValueAsString("POSNR_VA");
                itemInfo.SOCreateDate = row.GetFieldValueAsString("ERDAT_VA");
                itemInfo.SOCreateTime = row.GetFieldValueAsString("ERZET_VA");
                itemInfo.SOCreateBy = row.GetFieldValueAsString("ERNAM_VA");
                itemInfo.SOChangedDate = row.GetFieldValueAsString("AEDAT_VA");
                itemInfo.SOChangedTime = row.GetFieldValueAsString("AEZET_VA");
                itemInfo.SOChangedBy = row.GetFieldValueAsString("AENAM_VA");
                itemInfo.Uniqueidentifier = row.GetFieldValueAsString("HANDLE");
                itemInfo.DeliveryItemCategory = row.GetFieldValueAsString("PSTYV");
                itemInfo.MaterialNumber = row.GetFieldValueAsString("MATNR");
                itemInfo.CustomerModelName = row.GetFieldValueAsString("FERTH");
                itemInfo.MaterialBelonging = row.GetFieldValueAsString("KDMAT");
                itemInfo.OldMaterialNumber = row.GetFieldValueAsString("BISMT");
                itemInfo.MaterialGroup = row.GetFieldValueAsString("MATKL");
                itemInfo.Plant = row.GetFieldValueAsString("WERKS");
                itemInfo.StorageLocation = row.GetFieldValueAsString("LGORT");
                itemInfo.ActualQuantityDelivered = row.GetFieldValueAsString("LFIMG");
                itemInfo.BaseUnitOfMeasure = row.GetFieldValueAsString("MEINS");
                itemInfo.SalesUnit = row.GetFieldValueAsString("VRKME");
                itemInfo.NetWeight = row.GetFieldValueAsString("NTGEW");
                itemInfo.GrossWeight = row.GetFieldValueAsString("BRGEW");
                itemInfo.WeightUnit = row.GetFieldValueAsString("GEWEI");
                itemInfo.Volume = row.GetFieldValueAsString("VOLUM");
                itemInfo.VolumeUnit = row.GetFieldValueAsString("VOLEH");
                itemInfo.ActualQuantityDeliveredInStockkeepingUnits = row.GetFieldValueAsString("LGMNG");
                itemInfo.CustomerPurchaseOrderNumber = row.GetFieldValueAsString("BSTKD");
                itemInfo.ShipToPurchaseOrderNumber = row.GetFieldValueAsString("BSTKD_E");
                itemInfo.YourReference = row.GetFieldValueAsString("IHREZ");
                itemInfo.ShipToCharacter = row.GetFieldValueAsString("IHREZ_E");
                itemInfo.PricingReferenceMaterial = row.GetFieldValueAsString("PMATN");
                itemInfo.AlternativeMaterials = row.GetFieldValueAsString("VBBP0009");
                itemInfo.PreparationConditions = row.GetFieldValueAsString("VBBP0010");
                itemInfo.CommodityDescription = row.GetFieldValueAsString("DESCCN");
                itemInfo.CommodityCodeOfTheGoods = row.GetFieldValueAsString("MODLEN");
                itemInfo.ExporterCommodityCode = row.GetFieldValueAsString("HSCODE_SRC");
                itemInfo.DestinationCountriesCommodityCode = row.GetFieldValueAsString("HSCODE_DST");
                itemInfo.ShortText = row.GetFieldValueAsString("ARKTX");
                itemInfo.StorageBin = row.GetFieldValueAsString("LGPBE");
                itemInfo.OriginatingDocument = row.GetFieldValueAsString("VBELV");
                itemInfo.OriginatingItem = row.GetFieldValueAsString("POSNV");
                itemInfo.SDDocumentCategory = row.GetFieldValueAsString("VBTYV");
                itemInfo.DocumentNumber = row.GetFieldValueAsString("VGBEL");
                itemInfo.ItemNumber = row.GetFieldValueAsString("VGPOS");
                itemInfo.MovementType = row.GetFieldValueAsString("BWART");
                itemInfo.RequirementType = row.GetFieldValueAsString("BDART");
                itemInfo.PlanningType = row.GetFieldValueAsString("PLART");
                itemInfo.MaterialType = row.GetFieldValueAsString("MTART");
                itemInfo.ValuationType = row.GetFieldValueAsString("BWTAR");
                itemInfo.BusinessArea = row.GetFieldValueAsString("GSBER");
                itemInfo.SalesOffice = row.GetFieldValueAsString("VKBUR");
                itemInfo.SalesGroup = row.GetFieldValueAsString("VKGRP");
                itemInfo.DistributionChannel = row.GetFieldValueAsString("VTWEG");
                itemInfo.Division = row.GetFieldValueAsString("SPART");
                itemInfo.DeliveryGroup = row.GetFieldValueAsString("GRKOR");
                itemInfo.QuantityIsFixed = row.GetFieldValueAsString("FMENG");
                itemInfo.InternationalArticleNumber = row.GetFieldValueAsString("EAN11");
                itemInfo.CostCenter = row.GetFieldValueAsString("KOSTL");
                itemInfo.ControllingArea = row.GetFieldValueAsString("KOKRS");
                itemInfo.ProfitCenter = row.GetFieldValueAsString("PRCTR");
                itemInfo.OrderNumber = row.GetFieldValueAsString("AUFNR");
                itemInfo.OrderItemNumber = row.GetFieldValueAsString("POSNR_PP");
                itemInfo.SalesOrderNumber = row.GetFieldValueAsString("KDAUF");
                itemInfo.ItemNumberInSalesOder = row.GetFieldValueAsString("KDPOS");
                itemInfo.ReceivingPoint = row.GetFieldValueAsString("EMPST");
                itemInfo.ConditionPricingUnit = row.GetFieldValueAsString("KPEIN");
                itemInfo.ConditionUnit = row.GetFieldValueAsString("KMEIN");
                itemInfo.NetPrice = row.GetFieldValueAsString("NETPR");
                itemInfo.NetValue = row.GetFieldValueAsString("NETWR");
                itemInfo.StatisticalValues = row.GetFieldValueAsString("KOWRR");
                itemInfo.MovementIndicator = row.GetFieldValueAsString("KZBEW");
                itemInfo.PODIndicator = row.GetFieldValueAsString("KZPOD");
                itemInfo.OriginalQuantityOfDeliveryItem = row.GetFieldValueAsString("ORMNG");
                itemInfo.SDDocumentCurrency = row.GetFieldValueAsString("WAERS");
                itemInfo.ContainerQTY = row.GetFieldValueAsString("NORMT");
                itemInfo.UL = row.GetFieldValueAsString("ZTXT1");
                list.Add(itemInfo);
            }
            info.ItemInfos = list.ToArray();
        }
        void ParsePrice(IRfcFunction function, DNInfo info)
        {
            List<PriceInfo> list = new List<PriceInfo>();
            IRfcTable table = function.GetTable("PRICE_TAB");
            foreach (IRfcStructure row in table)
            {
                PriceInfo priceInfo = new PriceInfo();
                priceInfo.SourceSAP = row.GetFieldValueAsString("SRCSYS");
                priceInfo.DNNOWithCompanyCode = row.GetFieldValueAsString("VBELN");
                priceInfo.CompanyCode = row.GetFieldValueAsString("BUKRS_VF");
                priceInfo.DNNO = row.GetFieldValueAsString("VBELN_VL");
                priceInfo.DeliveryItem = row.GetFieldValueAsString("POSNR_VL");
                priceInfo.ConditionType = row.GetFieldValueAsString("KSCHL");
                priceInfo.NumberOfTheDocumentCondition = row.GetFieldValueAsString("KNUMV");
                priceInfo.ConditionItemNumber = row.GetFieldValueAsString("KPOSN");
                priceInfo.StepNumber = row.GetFieldValueAsString("STUNR");
                priceInfo.ConditionCounter = row.GetFieldValueAsString("ZAEHK");
                priceInfo.Application = row.GetFieldValueAsString("KAPPL");
                priceInfo.BaseValue = row.GetFieldValueAsString("KAWRT");
                priceInfo.Rate = row.GetFieldValueAsString("KBETR");
                priceInfo.CurrencyKey = row.GetFieldValueAsString("WAERS");
                priceInfo.LocalCurrency = row.GetFieldValueAsString("KKURS");
                priceInfo.PricingUnit = row.GetFieldValueAsString("KPEIN");
                priceInfo.Category = row.GetFieldValueAsString("KNTYP");
                priceInfo.Statistics = row.GetFieldValueAsString("KSTAT");
                priceInfo.Accrual = row.GetFieldValueAsString("KRUEK");
                priceInfo.RecordNumber = row.GetFieldValueAsString("KNUMH");
                priceInfo.SEQNumber = row.GetFieldValueAsString("KOPOS");
                priceInfo.AccountKey = row.GetFieldValueAsString("KVSL1");
                priceInfo.AccountNumber = row.GetFieldValueAsString("SAKN1");
                priceInfo.PurchasesCode = row.GetFieldValueAsString("MWSK1");
                priceInfo.WithholdingTaxCode = row.GetFieldValueAsString("MWSK2");
                priceInfo.Value = row.GetFieldValueAsString("KWERT");
                priceInfo.Inactive = row.GetFieldValueAsString("KINAK");
                priceInfo.Class = row.GetFieldValueAsString("KOAID");
                priceInfo.BaseValue2 = row.GetFieldValueAsString("KAWRT_K");
                priceInfo.Currency = row.GetFieldValueAsString("KWAEH");
                priceInfo.Value2 = row.GetFieldValueAsString("KWERT_K");
                list.Add(priceInfo);
            }
            info.PriceInfos = list.ToArray();
        }
        void ParsePartner(IRfcFunction function, DNInfo info)
        {
            List<PartnerInfo> list = new List<PartnerInfo>();
            IRfcTable table = function.GetTable("PARTNER_TAB");
            foreach (IRfcStructure row in table)
            {
                PartnerInfo parterInfo = new PartnerInfo();
                parterInfo.SourceSAP = row.GetFieldValueAsString("SRCSYS");
                parterInfo.DNNOWithCompanyCode = row.GetFieldValueAsString("VBELN");
                parterInfo.CompanyCode = row.GetFieldValueAsString("BUKRS_VF");
                parterInfo.DNNO = row.GetFieldValueAsString("VBELN_VL");
                parterInfo.PartnerFunction = row.GetFieldValueAsString("PARVW");
                parterInfo.PartnerNumber = row.GetFieldValueAsString("PARTNER");
                CompanyEDI.FillCompanyInfo(row, parterInfo);
                list.Add(parterInfo);
            }
            info.PartnerInfos = list.ToArray();
        }
        void ParseClassfication(IRfcFunction function, DNInfo info)
        {
            List<ClassificationInfo> list = new List<ClassificationInfo>();
            IRfcTable table = function.GetTable("CLASSIFICATION_TAB");
            foreach (IRfcStructure row in table)
            {
                ClassificationInfo cInfo = new ClassificationInfo();
                cInfo.SourceSAP = row.GetFieldValueAsString("SRCSYS");
                cInfo.DNNOWithCompanyCode = row.GetFieldValueAsString("VBELN");
                cInfo.CompanyCode = row.GetFieldValueAsString("BUKRS_VF");
                cInfo.DNNO = row.GetFieldValueAsString("VBELN_VL");
                cInfo.DeliveryItem = row.GetFieldValueAsString("POSNR_VL");
                cInfo.MaterialNumber = row.GetFieldValueAsString("MATNR");
                cInfo.CharacteristicName = row.GetFieldValueAsString("CHARACT");
                cInfo.CharacteristicValue = row.GetFieldValueAsString("VALUE_CHAR");
                cInfo.Indicator = row.GetFieldValueAsString("INHERITED");
                cInfo.InstanceCounter = row.GetFieldValueAsString("INSTANCE");
                cInfo.CharacteristicValue2 = row.GetFieldValueAsString("VALUE_NEUTRAL");
                cInfo.CharacteristicDescription = row.GetFieldValueAsString("CHARACT_DESCR");
                list.Add(cInfo);
            }
            info.ClassificationInfos = list.ToArray();
        }
        void ParsePacking(IRfcFunction function, DNInfo info)
        {
            List<PalletInfo> list = new List<PalletInfo>();
            IRfcTable table = function.GetTable("PACKING_TAB");
            foreach (IRfcStructure row in table)
            {
                PalletInfo cInfo = new PalletInfo();
                cInfo.SourceSAP = row.GetFieldValueAsString("SRCSYS");
                cInfo.DNNOWithCompanyCode = row.GetFieldValueAsString("VBELN");
                cInfo.CompanyCode = row.GetFieldValueAsString("BUKRS_VF");
                cInfo.DNNO = row.GetFieldValueAsString("VBELN_VL");
                cInfo.InternalHandlingUnitNumber = row.GetFieldValueAsString("VENUM");
                cInfo.HandlingUnitItem = row.GetFieldValueAsString("VEPOS");
                cInfo.ItemNumber = row.GetFieldValueAsString("POSNR");
                cInfo.CreateDate = row.GetFieldValueAsString("ERDAT");
                cInfo.EntryTime = row.GetFieldValueAsString("ERZET");
                cInfo.CreatedBy = row.GetFieldValueAsString("ERNAM");
                cInfo.ChangedDate = row.GetFieldValueAsString("AEDAT");
                cInfo.ChangedTime = row.GetFieldValueAsString("AEZET");
                cInfo.ChangedBy = row.GetFieldValueAsString("AENAM");
                cInfo.ExternalHandlingUnitIdentification = row.GetFieldValueAsString("EXIDV");
                cInfo.PackagingMaterials = row.GetFieldValueAsString("VHILM");
                cInfo.PackagingMaterialsDescription = row.GetFieldValueAsString("VEBEZ");
                cInfo.HandlingUnitContent = row.GetFieldValueAsString("INHALT");
                cInfo.PackagingMaterialType = row.GetFieldValueAsString("VHART");
                cInfo.Description = row.GetFieldValueAsString("VHTXT");
                cInfo.HigherLevelHandlingUnit = row.GetFieldValueAsString("UEVEL");
                cInfo.MaterialNumber = row.GetFieldValueAsString("MATNR");
                cInfo.QTY = row.GetFieldValueAsString("VEMNG");
                cInfo.QTYUnit = row.GetFieldValueAsString("VEMEH");
                cInfo.GrossWeight = row.GetFieldValueAsString("BRGEW");
                cInfo.NetWeight = row.GetFieldValueAsString("NTGEW");
                cInfo.NetWeightUnit = row.GetFieldValueAsString("NEWEI");
                cInfo.AllowedLoadingWeight = row.GetFieldValueAsString("MAGEW");
                cInfo.TareWeight = row.GetFieldValueAsString("TARAG");
                cInfo.WeightUnit = row.GetFieldValueAsString("GEWEI");
                cInfo.TotalVolume = row.GetFieldValueAsString("BTVOL");
                cInfo.LoadingVolume = row.GetFieldValueAsString("NTVOL");
                cInfo.LoadingVolumeUnit = row.GetFieldValueAsString("MAVOL");
                cInfo.TareVolume = row.GetFieldValueAsString("TAVOL");
                cInfo.Volume = row.GetFieldValueAsString("VOLUM");
                cInfo.VolumeUnit = row.GetFieldValueAsString("VOLEH");
                cInfo.ExternalHandlingUnitIdentification2 = row.GetFieldValueAsString("EXIDV2");
                cInfo.SortField = row.GetFieldValueAsString("SORTL");
                cInfo.HandlingUnitGroup1 = row.GetFieldValueAsString("VEGR1");
                cInfo.HandlingUnitGroup2 = row.GetFieldValueAsString("VEGR2");
                cInfo.HandlingUnitGroup3 = row.GetFieldValueAsString("VEGR3");
                cInfo.HandlingUnitGroup4 = row.GetFieldValueAsString("VEGR4");
                cInfo.HandlingUnitGroup5 = row.GetFieldValueAsString("VEGR5");
                list.Add(cInfo);
            }
            info.PalletInfos = list.ToArray();
        }
    }
}


















































































