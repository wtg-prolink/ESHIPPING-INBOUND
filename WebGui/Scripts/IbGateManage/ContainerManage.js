var _dm = new dm();
$MainGrid = $("#MainGrid");

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function loadSubData(ShipmentId) {
    CommonFunc.ToogleLoading(true);
    $.post(rootPath + 'GateManage/GetSmRvByShipmentId', {"ShipmentId": ShipmentId,"Pagesize":100}, function(data, textStatus, xhr) {
        CommonFunc.ToogleLoading(false);
        $("#SubGrid").jqGrid("clearGridData");
        $("#SubGrid").jqGrid("setGridParam", {
            datatype: 'local',
            sortorder: "asc",
            sortname: "ShipmentId",
            data: data.rows,
        }).trigger("reloadGrid");
    }, "JSON");
}

function getData(url, data, callBackFn) {
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            console.log(result);
            var resJson = $.parseJSON(result);
            callBackFn(resJson);
        }
    });
}

function DataSummary()
{
    var params = "";
    var gop = {};
    var numberTemplate = "2";
    var gop = {};
    var numberTemplate = "2";
    var docHeight = $(document).height();
    gridHeight = docHeight - 400;
    gop.gridColModel = [
        { name: 'UId', title: 'ID', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
        //{ name: 'UseDate', title: '@Resources.Locale.L_GateReserveSetup_UseDate', index: 'UseDate', width: 80, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'CombineInfo', title: '@Resources.Locale.L_DNApproveManage_CombineInfo', index: 'CombineInfo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DnNo', title: '@Resources.Locale.L_DNApproveManage_DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BlWin', title: '@Resources.Locale.L_GateReserve_SmcreateBy', index: 'BlWin', width: 150, align: 'left', sorttype: 'string', hidden: false },
        //{ name: 'TranType', title: '', index: 'TranType', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TranType', title: '@Resources.Locale.L_BaseLookup_TranType', index: 'TranType', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:FCL;L:LCL;A:AIR;D:INLAND EXPRESS;E:EXPRESS;R:Railroad;T:TRUCK;' }},
        { name: 'CargoType', title: '@Resources.Locale.L_DNApproveManage_CargoType', index: 'CargoType', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", remark: '@Resources.Locale.L_ContainerManage_Script_152', editoptions: { value: '@Resources.Locale.L_ContainerManage_Script_152' } },
        { name: 'CutPortDate', title: '@Resources.Locale.L_BaseLookup_CutPortDate', index: 'CutPortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PortDate', title: '@Resources.Locale.L_BaseLookup_PortDate', index: 'PortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'JobNo', title: '@Resources.Locale.L_DNApproveManage_JobNo', index: 'JobNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        //{ name: '', title: '到櫃時間', index: '', width: 80, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'Jqty', title: '@Resources.Locale.L_DNApproveManage_Jqty', index: 'Jqty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'Pqty', title: '@Resources.Locale.L_DNApproveManage_Pqty', index: 'Pqty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'CntrStdQty', title: '@Resources.Locale.L_ContainerManage_CntrStdQty', index: 'CntrStdQty', width: 80, align: 'right', formatter: 'integer', hidden: false },
        { name: 'CntNumber', title: '@Resources.Locale.L_BaseLookup_CntNumber', index: 'CntNumber', width: 70, align: 'right', formatter: 'integer', hidden: false },
        //{ name: 'CntrNo', title: '櫃號', index: 'CntrNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntType', title: '@Resources.Locale.L_GateAnalysis_CntrType', index: 'CntType', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ProductLine', title: '@Resources.Locale.L_GateReserveSetup_ProLine', index: 'ProductLine', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Qty', title: '@Resources.Locale.L_DNApproveManage_Qty', index: 'Qty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'Iqty', title: '@Resources.Locale.L_DNApproveManage_Iqty', index: 'Iqty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'Wqty', title: '@Resources.Locale.L_DNApproveManage_Wqty', index: 'Wqty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'SealQty', title: '@Resources.Locale.L_DNApproveManage_SealQty', index: 'SealQty', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'CaNo', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'CaNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CaNm', title: 'Carrier Name', index: 'CaNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TkNo', title: '@Resources.Locale.L_GateReserve_Trucker', index: 'TkNo', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TkNm', title: '@Resources.Locale.L_ContainerManage_TruckNm', index: 'TkNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'OpartNo', title: '@Resources.Locale.L_DNApproveManage_OpartNo', index: 'OpartNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'State', title: '@Resources.Locale.L_BsStateQuery_StateNm', index: 'State', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Qtyu', index: 'Qtyu', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DownUser', title: '@Resources.Locale.L_DNManage_Dwer', index: 'DownUser', width: 70, align: 'left', sorttype: 'string', hidden: false },
        //{ name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', width: 60, align: 'left', sorttype: 'string', hidden: false },
        //{ name: 'UnitPrice1', title: '@Resources.Locale.L_DNApproveManage_UnitPrice', index: 'UnitPrice1', width: 70, align: 'right', formatter: 'integer', hidden: false },
        //{ name: 'Price', title: '@Resources.Locale.L_DNDetailVeiw_Value', index: 'Price', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'PkgNum', title: '@Resources.Locale.L_DNApproveManage_PkgNum', index: 'PkgNum', width: 70, align: 'right', formatter: 'integer', hidden: false },
        { name: 'Cbm', title: '@Resources.Locale.L_DNApproveManage_Cbm', index: 'Cbm', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false },
        { name: 'Gw', title: '@Resources.Locale.L_BaseLookup_Gw', index: 'Gw', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false },
        { name: 'FcCd', title: '@Resources.Locale.L_DNApproveManage_FiCustCd', index: 'FcCd', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FcNm', title: '@Resources.Locale.L_DNApproveManage_FiCustNm', index: 'FcNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Etd', title: 'ETD', index: 'Etd', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } }
    ];

    gop.AddUrl = false;
    gop.gridId = "containerInfoGrid";
    gop.multiselect = false;
    gop.gridAttr = { caption: '@Resources.Locale.L_DNManage_CntrMagList', height: 300, refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.baseCondition = "";//smdnbaseCondition();
    gop.gridSearchUrl = rootPath + "GateManage/GetSMDNPData";

    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea"; 
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";
    
    gop.onSelectRowFunc = function (jsonMap) {
        //顯示子表
        var DnNo = jsonMap.DnNo;
        var ShipmentId = jsonMap.ShipmentId;
        $SubGrid.jqGrid("clearGridData");
        //console.log(DnNo);
        loadSubData(ShipmentId);
    }

    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea";

    gop.reportFunc = function (item, name) {
        var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
        var dnNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'DnNo');
        var shipment_id = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'ShipmentId');

        var count = $("#SubGrid").getGridParam("reccount");
        var selRowId1 = $("#SubGrid").jqGrid('getGridParam', 'selrow');
        var batno = $("#SubGrid").jqGrid('getCell', selRowId1, 'BatNo');
        $("#SubGrid");
        if (!dnNo) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        if (item=="SMRV03"&&(batno == null || batno == "")) {
            if (count == 1) {
                selRowId1 = 1;
                var batno = $("#SubGrid").jqGrid('getCell', selRowId1, 'BatNo');
            }
            else {
                CommonFunc.Notify("", "@Resources.Locale.L_ContainerManage_Scripts_255", 500, "warning");
                return;
            }
        }
        //alert(item+name);
        var params = {
            currentCondition: "",
            batno:batno,
            val: dnNo,
            uid: shipment_id,
            dnno: dnNo,
            field:'DN_NO',
            rptdescp: name,
            rptName: item,
            formatType: 'xls',
            exportType: 'PREVIEW',
        };

        showReport(params);
    };

    function showReport(params) {
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
    }

    gop.reportItem = [
       {
           item: "PACK01",
           name: "@Resources.Locale.L_DNManage_CntList"
       },
       {
           item: "SMRV01",
           name: "@Resources.Locale.L_DNManage_Delve"
       },
      {
          item: "SMRV03",
          name: "@Resources.Locale.L_ActDeatilManage_Views_153"
      }
    ];

    gop.searchColumns = getSelectColumn(gop.gridColModel);
    gop.btnGroup = [{
        id: "btn01",
        name: "@Resources.Locale.L_DNManage_NotDownload",
        func: function () {
            $("#containerInfoGrid").jqGrid("setGridParam", {
                url: rootPath + "GateManage/GetNonDownloadSMDNPData",
                datatype: 'json',
                sortorder: "asc",
                sortname: "DnNo"
            }).trigger("reloadGrid");
        }
    },
    {
        id: "btn02",
        name: "@Resources.Locale.L_BSTQuery_ExpExcel",
        func: function () {
            var url = rootPath + "GateManage/exportSmdnpToExcel";
            $.post(rootPath+"GateManage/chkSmdnpCount", {}, function(data, textStatus, xhr) {
                if(data.msg == "success")
                {
                    //window.open(url);
                    var colModel1 = [
                            { name: 'UseDate', title: '@Resources.Locale.L_DNManage_UseDate', index: 'UseDate', width: 150, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_UseDate' },
                            { name: 'ProductLine', title: '@Resources.Locale.L_GateReserveSetup_ProLine', index: 'ProductLine', width: 70, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_Line' },
                            { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_ParNo' },
                            { name: 'Jqty', title: '@Resources.Locale.L_DNApproveManage_Jqty', index: 'Jqty', width: 70, align: 'right', formatter: 'integer', hidden: false,caption:'@Resources.Locale.L_DNApproveManage_Jqty' },
                            { name: 'JobNo', title: '@Resources.Locale.L_DNApproveManage_JobNo', index: 'JobNo', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNApproveManage_JobNo' },
                            { name: 'ReserveHour', title: '@Resources.Locale.L_DNManage_ArrDate', index: 'ReserveHour', width: 70, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_ArrDate' },
                            { name: 'Remark1', title: '@Resources.Locale.L_ContainerManage_Script_153', index: 'Remark', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BSCSSetup_Remark' },
                            { name: 'Qty', title: '@Resources.Locale.L_DNManage_Qty', index: 'Qty', width: 100, align: 'right', formatter: 'integer', sorttype: 'float', hidden: false, caption: '@Resources.Locale.L_DNManage_Qty' },
                            { name: 'Carrier', title: '@Resources.Locale.L_BaseLookup_Carrier', index: 'Carrier', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BaseLookup_Carrier' },
                            { name: 'Trucker', title: '@Resources.Locale.L_BaseLookup_CrNm', index: 'Trucker', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BaseLookup_CrNm' },
                            { name: 'CntType', title: '@Resources.Locale.L_ContainerManage_CntType', index: 'CntType', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_ContainerManage_CntType' },
                            { name: 'CntNumber', title: '@Resources.Locale.L_DNFlowManage_CntNumber', index: 'CntNumber', width: 60, align: 'right', sorttype: 'float', formatter: 'integer', hidden: false, caption: '@Resources.Locale.L_DNFlowManage_CntNumber' },
                            { name: 'DnNo', title: '@Resources.Locale.L_EventSetup_ShipmentId', index: 'DnNo', width: 200, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_EventSetup_ShipmentId' },
                            { name: 'CombineInfo', title: '@Resources.Locale.L_DNApproveManage_CombineInfo', index: 'CombineInfo', width: 200, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNApproveManage_CombineInfo' },
                            { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'Qtyu', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BaseLookup_Unit' },
                            { name: 'Remark2', title: '@Resources.Locale.L_ContainerManage_Script_154', index: 'Remark2', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_ContainerManage_Script_154' },
                            { name: 'CustCd', title: '@Resources.Locale.L_ForecastSetup_CustNo', index: 'CustCd', width: 70, align: 'left', sorttype: 'string', hidden: false,caption:'@Resources.Locale.L_ForecastSetup_CustNo' },
                            { name: 'CntrNo', title: '@Resources.Locale.L_DNManage_CtNo', index: 'CntrNo', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_CtNo' },
                            { name: 'TruckNo', title: '@Resources.Locale.L_BaseLookup_TruckNo', index: 'TruckNo', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BaseLookup_TruckNo' },
                            { name: 'CreateBy', title: '@Resources.Locale.L_DNManage_CreateBy', index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_DNManage_CreateBy' },
                            { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false, caption: 'Shipment ID' },
                            { name: 'Dep', title: '@Resources.Locale.L_GateReserveSetup_CreateDep', index: 'Dep', width: 100, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_GateReserveSetup_CreateDep' },
                            { name: 'CargoType', title: '@Resources.Locale.L_DNApproveManage_CargoType', index: 'CargoType', width: 100, align: 'left', sorttype: 'string', hidden: false,formatter: "select", editoptions:{ value: '@Resources.Locale.L_ContainerManage_Script_152'},caption: '@Resources.Locale.L_DNApproveManage_CargoType' },
                            { name: 'CutPortDate', title: '@Resources.Locale.L_BaseLookup_CutPortDate', index: 'CutPortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" }, caption: '@Resources.Locale.L_BaseLookup_CutPortDate' },
                            { name: 'ProductDate', title: '@Resources.Locale.L_DNApproveManage_ProduceDate', index: 'ProductDate', width: 100, align: 'left', sorttype: 'date', hidden: false, hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" }, caption: '@Resources.Locale.L_DNApproveManage_ProduceDate' },
                            { name: 'Lgoods', title: '@Resources.Locale.L_BSTSetup_LCommodity', index: 'Lgoods', width: 120, align: 'left', sorttype: 'string', hidden: false, caption: '@Resources.Locale.L_BSTSetup_LCommodity' }
                        ];
                    var conditions = "";
                    var baseCondition = " DOWN_FLAG IS NOT NULL AND DOWN_FLAG = 'N'";
                    var caption = "@Resources.Locale.L_DNManage_JnoPro";
                    postAndRedirect(url, { "ColumnList": JSON.stringify(colModel1).replace(new RegExp('"', "gm"), "'"), "conditions": conditions, "baseCondition": baseCondition, "resultType": "excel", "ReportTitle": caption,"excelName":"FQ-USER" });
                }
                else
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_DNManage_DownBefor", 500, "warning");
                }
            }, "JSON");
            
            
        }
    },
/*    {
        id: "btn09",
        name: "工號導入",
        func: function () {
            $("#uploadExcelFile").modal("show");
        }
    
    },*/
    {
        id: "btn09",
        name: "@Resources.Locale.L_DNManage_IpPro",
        func: function () {
            //var selRowId =  mygrid.jqGrid('getGridParam', 'selarrrow');
            //var selRowId =  $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            //var DnNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'DnNo');
            /*var DnpUidArray = [], DnpUid="";
            if(selRowId.length == 0)
            {
                CommonFunc.Notify("", "请先选择一笔记录", 500, "warning");
                return;
            }
            for(var i=0; i<selRowId.length; i++)
            {
                DnpUid = mygrid.jqGrid("getCell", selRowId[i], "UId");
                DnpUidArray.push(DnpUid);
            }*/

            /*$.ajax({
                url: rootPath + 'GateManage/NewOrderTruck',
                type: 'POST',
                dataType: 'json',
                //data: {"DnpUidArray": DnpUidArray.join()},
                data: {"DN_NO": DnNo},
                beforeSend: function(){
                    StatusBarArr.nowStatus("叫車中...");
                    CommonFunc.ToogleLoading(true);
                },
                success: function(result){
                    if(result.message == "success")
                    {
                        CommonFunc.Notify("", "叫車成功", 1000, "success");
                    }
                    else
                    {
                        //CommonFunc.Notify("", result.message, 1000, "warning");
                        alert(result.message);
                    }

                    StatusBarArr.nowStatus("");
                    CommonFunc.ToogleLoading(false);
                },
                error: function(){
                    CommonFunc.Notify("", "连线失败", 1000, "danger");
                    CommonFunc.ToogleLoading(false);
                }
            });*/
            $("#uploadSmrvDailog").modal("show");
        }
    },
    /*{
        id: "btn04",
        name: "貨櫃資訊",
        func: function () {
            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
            var DnNo = $("#containerInfoGrid").jqGrid('getCell', selRowId, 'DnNo');
            CommonFunc.ToogleLoading(true);
            $.post(rootPath + 'GateManage/GetSmRvByDnNo', {"DnNo": DnNo}, function(data, textStatus, xhr) {
                CommonFunc.ToogleLoading(false);
                $("#SubGrid").jqGrid("clearGridData");
                $("#SubGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "DnNo",
                    data: data.rows,
                }).trigger("reloadGrid");
            }, "JSON");
        }
    },*/
    {
        id: "btn05",
        name: '@GetLangText("L_TKBLQuery_DNDet")',
        func: function () {
            var selRowId = mygrid.jqGrid('getGridParam', 'selrow');
            var uid = mygrid.jqGrid('getCell', selRowId, 'DnNo');
            if (!uid) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            top.topManager.openPage({
                href: rootPath + "DNManage/DNDetailVeiw/" + uid,
                title: '@GetLangText("L_TKBLQuery_DNDet")',
                id: 'DNDetailVeiw'
            });
        }
    },
    {
        id: "btn06",
        name: "@Resources.Locale.L_ActManage_ApDetail",
        func: function () {
            /*var selRowId = mygrid.jqGrid('getGridParam', 'selrow');
            var uid = mygrid.jqGrid('getCell', selRowId, 'DnNo');
            if (selRowId.length != 1) {
                CommonFunc.Notify("", "请选择一笔记录", 500, "warning");
                return;
            }*/
            CheckDetailed();
        }
    }
    ];

    gop.loadCompleteFunc = function () {
        //_dm.getDs("SubGrid").setData([]);
    }
    initSearch(gop);
}



$(function () {
    $("#smrvUploadExcel").bootstrapFileInput();
        
    DataSummary();
    var colModel = [
        { name: 'UId', title: 'ID', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
        { name: 'BatNo', title: '@Resources.Locale.L_GateReserve_BatNo', index: 'BatNo', width: 150, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Status', title: '@Resources.Locale.L_ContainerManage_Status', index: 'Status', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: '@Resources.Locale.L_ContainerManage_Script_155'} },
        { name: 'ReserveNo', title: '@Resources.Locale.L_GateAnalysis_ReserveNo', index: 'ReserveNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DnNo', title: '@Resources.Locale.L_DNApproveManage_DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckCntrno', title: '@Resources.Locale.L_ContainerManage_TruckCntrno', index: 'TruckCntrno', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckSealno', title: '@Resources.Locale.L_ContainerManage_TruckSealno', index: 'TruckSealno', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CntType', title: '@Resources.Locale.L_ContainerManage_CntType', index: 'CntType', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CallDate', title: '@Resources.Locale.L_GateReserveSetup_CallDate', index: 'CallDate', width: 70, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'UseDate', title: '@Resources.Locale.L_BaseLookup_PickupCdate', index: 'UseDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MfNo', title: '@Resources.Locale.L_GateAnalysis_MfNo', index: 'MfNo', width: 70, align: 'left', sorttype: 'string', hidden: false, classes: "uppercase" },
        { name: 'WsCd', title: '@Resources.Locale.L_GateReserve_WsCd', index: 'WsCd', width: 150, align: 'left', sorttype: 'string', hidden: false},
        { name: 'GateNo', title: '@Resources.Locale.L_GateAnalysis_GateNo', index: 'GateNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CutPortDate', title: '@Resources.Locale.L_BaseLookup_CutPortDate', index: 'CutPortDate', width: 90, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'PortDate', title: '@Resources.Locale.L_BaseLookup_PortDate', index: 'PortDate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        //{ name: '', title: '放櫃時間', index: '', width: 70, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
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
        { name: 'Trucker', title: '@Resources.Locale.L_GateReserve_Trucker', index: 'Trucker', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TruckNo', title: '@Resources.Locale.L_GateReserve_TruckNo', index: 'TruckNo', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Driver', title: '@Resources.Locale.L_GateReserve_Driver', index: 'Driver', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Tel', title: '@Resources.Locale.L_GateReserve_Tel', index: 'Tel', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MoveNumber', title: '@Resources.Locale.L_GateAnalysis_MoveNumber', index: 'MoveNumber', width: 70, align: 'right', sorttype: 'float', hidden: false },
        { name: 'SCode', title: '@Resources.Locale.L_ContainerManage_SCode', index: 'SCode', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'EptIdate', title: '@Resources.Locale.L_GateAnalysis_EptIdate', index: 'EptIdate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'EptOdate', title: '@Resources.Locale.L_GateAnalysis_EptOdate', index: 'EptOdate', width: 120, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } }
    ];
    $SubGrid = $("#SubGrid");
    new genGrid(
        $SubGrid,
        {
            datatype: "local",
            data: [],
            loadonce: true,
            colModel: colModel,
            caption: '@Resources.Locale.L_ContainerManage_Script_156',
            height: "AUTO",
            refresh: true,
            cellEdit: false,//禁用grid编辑功能
            exportexcel: false,
            footerrow: false,
            dblClickFunc: function(map){
                var UId = map.UId;

                top.topManager.openPage({
                    href: rootPath + "GateManage/SmrvSetup/" + UId,
                    title: '@Resources.Locale.L_DNManage_CntrInfoSet',
                    id: 'SmrvSetup'
                });

            }
        }
    );

    $("#EXCEL_UPLOAD_FROM").submit(function(){
        var postData = new FormData($(this)[0]);
        $.ajax({
            url: rootPath + "GateManage/UploadJobNo",
            type: 'POST',
            data: postData,
            async: true,
            beforeSend: function(){
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                if (data.message != "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_ActDeatilManage_Views_116" + data.message, 1300, "warning");
                    return false;
                }
                CommonFunc.Notify("", "@Resources.Locale.L_BSTQuery_ImpSuc", 500, "success");
                $("#uploadExcelFile").modal("hide");
               
                $("#SummarySearch").click();
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });

    $("#SMRV_UPLOAD_FROM").submit(function(){
        var postData = new FormData($(this)[0]);
        $.ajax({
            url: rootPath + "GateManage/UploadSmrv",
            type: 'POST',
            data: postData,
            async: true,
            beforeSend: function(){
                CommonFunc.ToogleLoading(true);
                StatusBarArr.nowStatus("@Resources.Locale.L_BSTQuery_Uploading");
            },
            error: function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "error", 500, "warning");
                CommonFunc.ToogleLoading(false);
                StatusBarArr.nowStatus("");
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                StatusBarArr.nowStatus("");
                if (data.errorMsg != "") {
                    //CommonFunc.Notify("", "汇入失败" + data.message, 1300, "warning");
                    alert(data.errorMsg);
                    return false;
                }
                CommonFunc.Notify("", "@Resources.Locale.L_BSTQuery_ImpSuc", 500, "success");
                $("#uploadExcelFile").modal("hide");
               
                $("#SummarySearch").click();
            },
            cache: false,
            contentType: false,
            processData: false
        });

        return false;
    });
});