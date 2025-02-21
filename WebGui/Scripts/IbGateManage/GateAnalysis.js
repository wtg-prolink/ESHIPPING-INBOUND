var $SubGrid = $("#SubGrid");
var colModel2;

$(function(){
    _initGenGrid();
});

function _initGenGrid(){
    var gop = {};
    var numberTemplate = "2";
    var docHeight = $(document).height();
    gridHeight = docHeight - 330;
    gop.gridColModel = [
        { name: 'UId', title: 'UId', index: 'UId', hidden: true },
        { name: 'ShipmentId', title: _getLang("L_DNApproveManage_ShipmentId", "Shipment ID"), index: 'ShipmentId', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Consignee', title: _getLang("L_BSTSetup_CneeNm", "收货人名称"), index: 'Consignee', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'InvNo', title: _getLang("L_SMIDN_InvoiceNo", "Invoice No"), index: 'InvNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MasterNo', title: _getLang("L_BaseLookup_MasterNo", "Master B/L"), index: 'MasterNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'HouseNo', title: _getLang("L_SMORD_HouseNo", "House B/L"), index: 'HouseNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OpartNo', title: _getLang("L_SMIDNP_OpartNo", "对外机种名"), index: 'OpartNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Shipper', title: 'Shipper', index: 'Shipper', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PolCd', title: _getLang("L_AirQuery_PolCd", "启运地代码"), index: 'PolCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Sp', title: 'SP', index: 'Sp', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Fs', title: 'FS', index: 'Fs', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IbcrCode', title: 'IBCR Code', index: 'IbcrCode', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IbcrName', title: 'IBCR Name', index: 'IbcrName', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IbbrCode', title: 'IBBR Code', index: 'IbbrCode', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IbbrName', title: 'IBBR Name', index: 'IbbrName', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FcCode', title: 'FC Code', index: 'FcCode', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FcName', title: 'FC Name', index: 'FcName', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TranType', title: _getLang("L_BaseLookup_TranType", "运输方式"), index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:FCL;L:LCL;A:AIR;D:INLAND EXPRESS;E:EXPRESS;R:Railroad;T:TRUCK;' } },
        { name: 'IncotermCd', title: 'DLV Term', index: 'IncotermCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IncotermDescp', title: 'DLV TermDesp', index: 'IncotermDescp', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'InvQty', title: 'Invoice Qty', index: 'InvQty', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'FrtTerm', title: 'Frt Term', index: 'FrtTerm', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Pol1', title: 'Pol1', index: 'Pol1', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'PolNm1', title: 'Pol Nm1', index: 'PolNm1', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Vessel1', title: 'Vessel1', index: 'Vessel1', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Voyage1', title: 'Voyage1', index: 'Voyage1', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Vessel2', title: 'Vessel2', index: 'Vessel2', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Voyage2', title: 'Voyage2', index: 'Voyage2', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Vessel3', title: 'Vessel3', index: 'Vessel3', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Voyage3', title: 'Voyage3', index: 'Voyage3', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Vessel4', title: 'Vessel4', index: 'Vessel4', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'Voyage4', title: 'Voyage4', index: 'Voyage4', width: 100, align: 'right', sorttype: 'string', hidden: false },

        { name: 'Etd1', title: 'Etd1', index: 'Etd1', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Eta1', title: 'Eta1', index: 'Eta1', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Etd2', title: 'Etd2', index: 'Etd2', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Eta2', title: 'Eta2', index: 'Eta2', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Etd3', title: 'Etd3', index: 'Etd3', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Eta3', title: 'Eta3', index: 'Eta3', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Etd4', title: 'Etd4', index: 'Etd4', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Eta4', title: 'Eta4', index: 'Eta4', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Etd', title: 'Etd', index: 'Etd', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Eta', title: 'Eta', index: 'Eta', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Atp', title: 'Atp', index: 'Atp', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Atd', title: 'Atd', index: 'Atd', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Ata', title: 'Ata', index: 'Ata', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PonoInputDate', title: 'PonoInputDate', index: 'PonoInputDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PoNo', title: 'PoNo', index: 'PoNo', width: 100, align: 'right', sorttype: 'string', hidden: false },
        {
            name: 'Gw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'Gw', width: 70, align: 'right', sorttype: 'float', editable: true,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 3,
                defaultValue: '0.000'
            }
        },
        {
            name: 'Cbm', title: 'Cbm', index: 'Cbm', width: 70, align: 'right', sorttype: 'float', editable: true,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 3,
                defaultValue: '0.000'
            }
        },
        { name: 'CntrNo', title: _getLang("L_GateReserve_CntrNo", "货柜号码"), index: 'CntrNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntType', title: _getLang("L_GateAnalysis_CntrType", "柜型"), index: 'CntType', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DecNo', title: _getLang("L_SMICNTR_TcDecNo", "Declaration No."), index: 'DecNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DecDate', title: _getLang("L_SMICNTR_TcDecDate", "Decaration Date"), index: 'DecDate', width: 80, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'CcChannel', title: _getLang("L_SMIDN_TcCcChannel", "L_SMIDN_TcCcChannel"), index: 'CcChannel', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'RelDate', title: _getLang("L_SMIDN_TcRelDate", "Release Date"), index: 'RelDate', width: 80, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'LspAbnormalRmk', title: 'TPV Remark', index: 'LspAbnormalRmk', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckRmk', title: 'Truck Remark', index: 'TruckRmk', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'WsRmk', title: 'WareHose Remark', index: 'WsRmk', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DlvAreaNm', title: 'DlvAreaName/Temp', index: 'DlvAreaNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DlvAddr', title: 'DlvAddr/Temp', index: 'DlvAddr', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IbDate', title: 'Notify LSP Date', index: 'IbDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'LspConfirmBy', title: 'LSP Confirm By', index: 'LspConfirmBy', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'LspConfirmDate', title: 'Lsp Confirm Date', index: 'LspConfirmDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'CallDateL', title: 'Call Date', index: 'CallDateL', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'OrderDate', title: 'Order Date', index: 'OrderDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'ConfirmDate', title: 'Confirm Date', index: 'ConfirmDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'ArrivalFactDate', title: 'Arrival Fact Date', index: 'ArrivalFactDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'InDate', title: 'In Date', index: 'InDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PodDate', title: 'POD Date', index: 'PodDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'OutDate', title: 'Out Date', index: 'OutDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'BackLocation', title: 'Back Location', index: 'BackLocation', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'EmptyTime', title: 'EmptyTime', index: 'EmptyTime', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Goods', title: 'Goods', index: 'Goods', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ProductInfo', title: 'ProductInfo', index: 'ProductInfo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ReserveNo', title: 'ReserveNo', index: 'ReserveNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntryCd', title: 'CC Country', index: 'CntryCd', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PartNo', title: 'CustomerPN', index: 'PartNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'SoNo', title: 'SO per DN', index: 'SoNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        {name: 'PodCd', title: 'POD', index: 'PodCd', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'PolName', title: 'POL Nm', index: 'PolName', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'PodName', title: 'POD Nm', index: 'PodName', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'DestCd', title: 'Dest', index: 'DestCd', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'DestName', title: 'Dest Nm', index: 'DestName', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'IpartNo', title: 'TPV ModelName', index: 'IpartNo', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'WsCd', title: 'WH Code/Temp', index: 'WsCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DlvAreaNmF', title: 'DlvAreaName/Final', index: 'DlvAreaNmF', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DlvAddrF', title: 'DlvAddr/Final', index: 'DlvAddrF', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'WsCdF', title: 'WH Code/Final', index: 'WsCdF', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckNo', title: 'New Trailer No', index: 'TruckNo', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'SealNo1', title: 'SealNo1', index: 'SealNo1', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'SealNo2', title: 'SealNo2', index: 'SealNo2', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ReserveDate', title: 'Delivery Date', index: 'ReserveDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'UseDate', title: 'Pickup Date', index: 'UseDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'EtaRailrampDate', title: 'ETA RailRamp Date', index: 'EtaRailrampDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
    ];
    gop.AddUrl = false;
    gop.gridId = "containerInfoGrid";
    gop.gridAttr = { caption: _getLang("L_DNManage_GareMag", "月台管理分析"), height: gridHeight, refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.gridSearchUrl = rootPath + "IbGateManage/gateAnalysisQuery";
    gop.searchColumns = getSelectColumn(gop.gridColModel);
    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.sortname = "CreateDate";
    gop.gridAttr.sortname = "CreateDate";
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea"; 
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";
    gop.baseConditionFunc = function () {
        return getCreateDateParams("CreateDate", gop);
    }

    gop.reportFunc = function (item) {
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var bat_no = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'BatNo');
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var dnNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'DnNo');
        var tranType = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'TranType');
        var field = "BAT_NO";
        var val = bat_no;
        //if (tranType === "D" || tranType === "T") {
        //    field = "DN_NO";
        //    val = dnNo;
        //}
        field = "DN_NO";
        val = dnNo;
        if (!dnNo) {
            CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "请先选择一笔记录"), 500, "warning");
            return;
        }
        //alert(dnNo);
        var params = {
            currentCondition: "",
            val: val,
            field: field,
            rptdescp: _getLang("L_DNManage_Delve", "送货单"),
            rptName: item,
            formatType: 'xls',
            exportType: 'PREVIEW',
        };

        $.ajax({
            async: true,
            url: rootPath + "Report/CreateNewReport",
            type: 'POST',
            data: params,
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
                var xx = xmlHttpRequest.responseText;
                window.open(xmlHttpRequest.responseText);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {

            }
        });
    };
    //gop.reportItem = [
    //   {
    //       item: "SMRV01",
    //       name: "送货单",
    //   }
    //];
 

    gop.onSelectRowFunc = function(map){
        var BatNo = map.ReserveNo;
        $.ajax({
            url: rootPath + 'IbGateManage/getMoveInfo',
            type: 'POST',
            dataType: 'json',
            data: { "BatNo": BatNo},
            beforeSend: function(){
                //CommonFunc.ToogleLoading(true);
            },
            error: function(){
                CommonFunc.Notify("", _getLang("L_GateAnalysis_Scripts_265", "联机失败"), 500, "danger");
                $("#confirmMoveDailog").modal("hide");
                //CommonFunc.ToogleLoading(false);
            },
            success: function(result){
                //CommonFunc.ToogleLoading(false);
                $SubGrid.jqGrid("clearGridData");
                $SubGrid.jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.rows,
                }).trigger("reloadGrid");
                //CommonFunc.ToogleLoading(false);
            }
        });
    }

    gop.gridFunc = function (map) {
        var UId = map.UId;

        top.topManager.openPage({
            href: rootPath + "IbGateManage/SmrvSetup/" + UId,
            title: _getLang("L_DNManage_CntrInfoSet", "货柜信息设定"),
            id: 'SmrvSetup'
        });
    }

    initSearch(gop);

    colModel2 = [
        { name: 'CntrNo', title: _getLang("L_GateReserve_CntrNo", "货柜号码"), index: 'CntrNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NwsCd', title: _getLang("L_GateAnalysis_NwsCd", "新仓库"), index: 'NwsCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NgateNo', title: _getLang("L_GateAnalysis_NgateNo", "新月台"), index: 'NgateNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MovingDate', title: _getLang("L_GateAnalysis_MovingDate", "移动时间"), index: 'MovingDate', width: 150, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'OwsCd', title: _getLang("L_GateAnalysis_OwsCd", "旧仓库"), index: 'OwsCd', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OgateNo', title: _getLang("L_GateAnalysis_OgateNo", "旧月台"), index: 'OgateNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BatNo', title: _getLang("L_GateReserve_BatNo", "批次号码"), index: 'BatNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckNo', title: _getLang("L_GateReserve_TruckNo", "入厂车号"), index: 'TruckNo', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'GateNo', title: _getLang("L_GateAnalysis_GateNo", "月台"), index: 'GateNo', sorttype: 'string', hidden: true, editable: false, width: 100 },
        { name: 'Lift', title: _getLang("L_GateSetup_Lift", "是否升降"), index: 'Lift', sorttype: 'string', hidden: true, editable: false, width: 100 },
        //{ name: 'Status', title: _getLang("L_GateReserve_Status", "状态"), index: 'Status', sorttype: 'string', hidden: true, editable: false, width: 100 },
        //{ name: 'CntrNo', title: _getLang("L_GateReserve_CntrNo", "货柜号码"), index: 'CntrNo', sorttype: 'string', hidden: true, editable: true, width: 100 },
        { name: 'SuspendFrom', title: _getLang("L_GateSetup_SuspendFrom", "暂停开始时间"), index: 'SuspendFrom', sorttype: 'string', hidden: true, editable: true, width: 100 },
        { name: 'SuspendTo', title: _getLang("L_GateSetup_SuspendTo", "暂停截止时间"), index: 'SuspendTo', sorttype: 'string', hidden: true, editable: true, width: 100 }
    ];

     new genGrid(
        $SubGrid,
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel2,
            isModel:true,
            caption: _getLang("L_DNManage_Move", "移柜信息"),
            height: gridHeight - 130,
            rows: 9999,
            refresh: true,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            exportexcel: false,
            toppager:false,
            loadComplete: function(data){
                var num = data.rows.length;
                if(num > 0)
                {
                    var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
                    setGridVal($("#containerInfoGrid"), selRowId, "MoveNumber",  num, null);
                }
            }
        }
     );
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}