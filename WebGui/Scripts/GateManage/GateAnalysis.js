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
        { name: 'Cmp', title: '@Resources.Locale.L_RoleSetUp_CMP', index: 'Cmp', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'IsBatch', title: '@Resources.Locale.L_DNManage_BatchPick', index: 'IsBatch', width: 70, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: "Y:Yes;N:No;" } },
        { name: 'Status', title: '@Resources.Locale.L_ContainerManage_Status', index: 'Status', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select",remark: '@Resources.Locale.L_ContainerManage_Script_155', editoptions: { value: '@Resources.Locale.L_ContainerManage_Script_155' } },
        { name: 'TranType', title: '@Resources.Locale.L_BaseLookup_TranType', index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:FCL;L:LCL;A:AIR;D:INLAND EXPRESS;E:EXPRESS;R:Railroad;T:TRUCK;' } },
        { name: 'ReserveNo', title: '@Resources.Locale.L_GateAnalysis_ReserveNo', index: 'ReserveNo', width: 110, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BatNo', title: '@Resources.Locale.L_GateReserve_BatNo', index: 'BatNo', width: 110, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShipmentId', title: '@Resources.Locale.L_DNApproveManage_ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DnNo', title: '@Resources.Locale.L_DNApproveManage_DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CargoType', title: '@Resources.Locale.L_DNApproveManage_CargoType', index: 'CargoType', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: '@Resources.Locale.L_ContainerManage_Script_152', editoptions: { value: '@Resources.Locale.L_ContainerManage_Script_152' } },
        { name: 'ReserveDate', title: '@Resources.Locale.L_GateReserve_ReveseDate', index: 'ReserveDate', width: 80, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'ReserveFrom', title: '@Resources.Locale.L_GateStatus_ReserveFrom', index: 'ReserveFrom', width: 100, sorttype: 'string', editable: false, formatter: "select", editoptions: { value: '0:00\:00;1:01\:00;2:02\:00;3:03\:00;4:04\:00;5:05\:00;6:06\:00;7:07\:00;8:08\:00;9:09\:00;10:10\:00;11:11\:00;12:12\:00;13:13\:00;14:14\:00;15:15\:00;16:16\:00;17:17\:00;18:18\:00;19:19\:00;20:20\:00;21:21\:00;22:22\:00;23:23\:00' } },
        { name: 'TruckCntrno', title: '@Resources.Locale.L_ContainerManage_TruckCntrno', index: 'TruckCntrno', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckSealno', title: '@Resources.Locale.L_ContainerManage_TruckSealno', index: 'TruckSealno', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MfNo', title: '@Resources.Locale.L_GateAnalysis_MfNo', index: 'MfNo', width: 70, align: 'left', sorttype: 'string', hidden: false, classes: "uppercase" },
        { name: 'WsCd', title: '@Resources.Locale.L_GateReserve_WsCd', index: 'WsCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'GateNo', title: '@Resources.Locale.L_GateAnalysis_GateNo', index: 'GateNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PolCd', title: '@Resources.Locale.L_BaseLookup_PolCd', index: 'PolCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PolName', title: '@Resources.Locale.L_BaseLookup_PolName', index: 'PolName', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CutPortDate', title: '@Resources.Locale.L_BaseLookup_CutPortDate', index: 'CutPortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PortDate', title: '@Resources.Locale.L_BaseLookup_PortDate', index: 'PortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        //{ name: '', title: '放櫃時間', index: '', width: 70, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'UseDate', title: '@Resources.Locale.L_GateReserveSetup_UseDate', index: 'UseDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PickDate', title: '@Resources.Locale.L_DNManage_PickCntr', index: 'PickDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'InDate', title: '@Resources.Locale.L_ContainerManage_InBy', index: 'InDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'SealDate', title: '@Resources.Locale.L_ContainerManage_SealDate', index: 'SealDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'SealNo1', title: '@Resources.Locale.L_GateReserveSetup_SealNo 1', index: 'SealNo1', width: 100, align: 'left', sorttype: 'string', hidden: false },//
        { name: 'SealNo2', title: '@Resources.Locale.L_GateReserveSetup_SealNo 2', index: 'SealNo2', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OutDate', title: '@Resources.Locale.L_ContainerManage_OutBy', index: 'OutDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Yard', title: '@Resources.Locale.L_ContainerManage_Yard', index: 'Yard', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'InyardDate', title: '@Resources.Locale.L_GateAnalysis_InyardDate', index: 'InyardDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'OutyardDate', title: '@Resources.Locale.L_GateAnalysis_OutyardDate', index: 'OutyardDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'AportDate', title: '@Resources.Locale.L_ContainerManage_AportDate', index: 'AportDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'Atd', title: '@Resources.Locale.L_BaseLookup_Atd', index: 'Atd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Carrier', title: '@Resources.Locale.L_ForecastQueryData_Carrier', index: 'Carrier', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CarrierNm', title: '@Resources.Locale.L_BaseLookup_CarrierNm', index: 'CarrierNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Trucker', title: '@Resources.Locale.L_GateReserve_Trucker', index: 'Trucker', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckerNm', title: '@Resources.Locale.L_BSCSSetup_CmpName111', index: 'TruckerNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckNo', title: '@Resources.Locale.L_GateReserve_TruckNo', index: 'TruckNo', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Driver', title: '@Resources.Locale.L_GateReserve_Driver', index: 'Driver', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Tel', title: '@Resources.Locale.L_GateReserve_Tel', index: 'Tel', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MoveNumber', title: '@Resources.Locale.L_GateAnalysis_TotalMoveNumber', index: 'MoveNumber', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NightMove', title: '@Resources.Locale.L_ContainerManage_NightCount', index: 'NightMove', width: 70, align: 'right', sorttype: 'float', hidden: false },
        { name: 'SCode', title: '@Resources.Locale.L_ContainerManage_SCode', index: 'SCode', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'EptIdate', title: '@Resources.Locale.L_GateAnalysis_EptIdate', index: 'EptIdate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'EptOdate', title: '@Resources.Locale.L_GateAnalysis_EptOdate', index: 'EptOdate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'TtlVgm', title: 'Total VGM', index: 'TtlVgm', width: 100, align: 'right', sorttype: 'string', hidden: false },
        { name: 'TareWeight', title: 'Tare Weight', index: 'TareWeight', width: 100, align: 'right', sorttype: 'string', hidden: false },
        {
            name: 'Gw', title: '@Resources.Locale.L_BaseLookup_Gw', index: 'Gw', width: 70, align: 'right', sorttype: 'float', editable: true,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 3,
                defaultValue: '0.000'
            }
        },
        { name: 'CntType', title: '@Resources.Locale.L_GateAnalysis_CntrType', index: 'CntType', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'SmcreateBy', title: '@Resources.Locale.L_BaseLookup_BlWin', index: 'SmcreateBy', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Etd', title: 'ETD', index: 'Etd', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d " } },
        { name: 'CallDate', title: '@Resources.Locale.L_GateReserveSetup_CallDate', index: 'CallDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'IncotermCd', title: 'DLV term', index: 'IncotermCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FcCd', title: '@Resources.Locale.L_DNApproveManage_FiCustCd', index: 'FcCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FcNm', title: '@Resources.Locale.L_GateAnalysis_Scripts_261', index: 'FcNm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntrBeDay', title: '@Resources.Locale.L_GateAnalysis_Scripts_262', index: 'CntrBeDay', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntrInfacDay', title: ' @Resources.Locale.L_GateAnalysis_Scripts_263', index: 'CntrInfacDay', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FacHoldDay', title: '@Resources.Locale.L_GateAnalysis_Scripts_264', index: 'FacHoldDay', width: 150, align: 'left', sorttype: 'string', hidden: false }
        //{ name: '', title: '使用時間', index: '', width: 80, align: 'right', formatter: 'integer', hidden: false }

    ];
    gop.AddUrl = false;
    gop.gridId = "containerInfoGrid";
    gop.gridAttr = { caption: "@Resources.Locale.L_DNManage_GareMag", height: gridHeight, refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.gridSearchUrl = rootPath + "GateManage/gateAnalysisQuery";
    gop.searchColumns = getSelectColumn(gop.gridColModel);
    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.sortname = "CreateDate";
    gop.gridAttr.sortname = "UId";
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea"; 
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";
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
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        //alert(dnNo);
        var params = {
            currentCondition: "",
            val: val,
            field: field,
            rptdescp: "@Resources.Locale.L_DNManage_Delve",
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
        var UFid = map.UId;
        var BatNo = map.BatNo;

        $.ajax({
            url: rootPath + 'GateManage/getMoveInfo',
            type: 'POST',
            dataType: 'json',
            data: { "UFid": UFid, "BatNo": BatNo},
            beforeSend: function(){
                //CommonFunc.ToogleLoading(true);
            },
            error: function(){
                CommonFunc.Notify("", "@Resources.Locale.L_GateAnalysis_Scripts_265", 500, "danger");
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
            href: rootPath + "GateManage/SmrvSetup/" + UId,
            title: '@Resources.Locale.L_DNManage_CntrInfoSet',
            id: 'SmrvSetup'
        });
    }

    initSearch(gop);

    colModel2 = [
        { name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NwsCd', title: '@Resources.Locale.L_GateAnalysis_NwsCd', index: 'NwsCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NgateNo', title: '@Resources.Locale.L_GateAnalysis_NgateNo', index: 'NgateNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MovingDate', title: '@Resources.Locale.L_GateAnalysis_MovingDate', index: 'MovingDate', width: 150, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'OwsCd', title: '@Resources.Locale.L_GateAnalysis_OwsCd', index: 'OwsCd', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OgateNo', title: '@Resources.Locale.L_GateAnalysis_OgateNo', index: 'OgateNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BatNo', title: '@Resources.Locale.L_GateReserve_BatNo', index: 'BatNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckNo', title: '@Resources.Locale.L_GateReserve_TruckNo', index: 'TruckNo', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'GateNo', title: '@Resources.Locale.L_GateAnalysis_GateNo', index: 'GateNo', sorttype: 'string', hidden: true, editable: false, width: 100 },
        { name: 'Lift', title: '@Resources.Locale.L_GateSetup_Lift', index: 'Lift', sorttype: 'string', hidden: true, editable: false, width: 100 },
        //{ name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', sorttype: 'string', hidden: true, editable: false, width: 100 },
        //{ name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', sorttype: 'string', hidden: true, editable: true, width: 100 },
        { name: 'SuspendFrom', title: '@Resources.Locale.L_GateSetup_SuspendFrom', index: 'SuspendFrom', sorttype: 'string', hidden: true, editable: true, width: 100 },
        { name: 'SuspendTo', title: '@Resources.Locale.L_GateSetup_SuspendTo', index: 'SuspendTo', sorttype: 'string', hidden: true, editable: true, width: 100 }
    ];

     new genGrid(
        $SubGrid,
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel2,
            caption: "@Resources.Locale.L_DNManage_Move",
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