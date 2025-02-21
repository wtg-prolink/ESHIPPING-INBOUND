var _dm = new dm();
$MainGrid = $("#MainGrid");
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function loadSubData(UId) {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetEvenRecords",
        type: 'POST',
        data: {
            uid: UId
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            //var rows = $.parseJSON(result);
            var $grid = $SubGrid;
            //_oldDeatiArray[UId] = mainTable.rows;
            if (_dm.getDs("SubGrid") == null || _dm.getDs("SubGrid") == undefined) {
                _dm.addDs("SubGrid", result, ["UId"], $grid[0]);
            } else {
                _dm.getDs("SubGrid").setData(result);
            }
        }
    });
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
    gop.gridColModel = [
        { name: 'BlNo', title: 'BlNo', showname: 'BlNo', sorttype: 'string', hidden: true, viewable: false },
        { name: 'EvenNo', title: '@Resources.Locale.L_EventSetup_UId', showname: 'EvenNo', sorttype: 'string', hidden: true, viewable: false },
        { name: 'Status', title: '@Resources.Locale.L_UserPermission_State', index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'S:S.Stand By;F:F.Finish' } },
        { name: 'TranMode', title: '@Resources.Locale.L_EventSetup_TranMode', index: 'TranMode', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: select_tranmode } },
        { name: 'ShipmentId', title: '@Resources.Locale.L_EventSetup_ShipmentId', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyTo', title: '@Resources.Locale.L_EventSetup_NotifyTo', index: 'NotifyTo', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyNm', title: '@Resources.Locale.L_EventSetup_NotifyNm', index: 'NotifyNm', width: 250, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyCd', title: '@Resources.Locale.L_EventSetup_NotifyCd', index: 'NotifyCd', width: 65, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyDescp', title: '@Resources.Locale.L_NRSDataQuery_NotifyDescp', index: 'NotifyDescp', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyDate', title: '@Resources.Locale.L_EventSetup_NotifyDate', index: 'NotifyDate', width: 140, align: 'left', sorttype: 'string', hidden: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'NotifyTimes', title: '@Resources.Locale.L_NRSDataQuery_NotifyTimes', index: 'NotifyTimes', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ProcessTimes', title: '@Resources.Locale.L_EventSetup_ProcessTimes', index: 'ProcessTimes', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyPeriod', title: '@Resources.Locale.L_NRSDataQuery_NotifyPeriod', index: 'NotifyPeriod', width: 80, align: 'left', sorttype: 'string', hidden: false },
        { name: 'NotifyFormat', title: '@Resources.Locale.L_NRSDataQuery_NotifyFormat', index: 'NotifyFormat', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: selectNotifyFormat } },
        { name: 'RequestCd', title: '@Resources.Locale.L_NRSDataQuery_RequestCd', index: 'RequestCd', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'RequestDescp', title: '@Resources.Locale.L_NRSDataQuery_RequestDescp', index: 'RequestDescp', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Remark', title: '@Resources.Locale.L_NRSDataQuery_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false }
    ];

    gop.gridId = "MainGrid";
    gop.gridAttr = { caption: '@Resources.Locale.L_EventSetup_ScheduleNotify', height: "auto", refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.gridSearchUrl = rootPath + "TKBL/GetEvenData";
    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea"; 
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";
    
    gop.onSelectRowFunc = function (jsonMap) {
        //顯示子表
        var EvenNo = jsonMap.EvenNo;
        loadSubData(EvenNo);
    }

    gop.baseConditionFunc = function () {
        return getCreateDateParams("CreateDate", gop);
    }

    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea";
    gop.searchColumns = getSelectColumn(gop.gridColModel);
    gop.multiselect = false;
    gop.btnGroup = [{
        id: "btn01",
        name: "@Resources.Locale.L_EventSetup_ReSend",
        func: function () {
            var id = $("#MainGrid").jqGrid('getGridParam', "selrow");
            var map = $("#MainGrid").jqGrid('getRowData', id);
            var UId;
            if (map) UId = map.EvenNo;

            if (isEmpty(UId)) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return false;
            }

            getData(rootPath + "TKBL/RestEven", { UId: UId }, function (result) {
                if (result && result.msg)
                    CommonFunc.Notify("", result.msg, 500, "warning");
                $("#SummarySearch").trigger("click");
            });

        }
    }];

    gop.loadCompleteFunc = function () {
        var $grid = $SubGrid;
        if (_dm.getDs("SubGrid") == null || _dm.getDs("SubGrid") == undefined) {
            _dm.addDs("SubGrid", [], ["UId"], $grid[0]);
        } else {
            _dm.getDs("SubGrid").setData([]);
        }
    }
    initSearch(gop);
    _dm.addDs("MainGrid", [], ["EvenNo"], $MainGrid[0]);
}



$(function () {
    $SubGrid = $("#SubGrid");
    DataSummary();
    var colModel = [
        { name: 'UId', title: '@Resources.Locale.L_EventSetup_UId', index: 'ID', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', title: '@Resources.Locale.L_EventSetup_UFid', index: 'UFid', sorttype: 'string', hidden: true, viewable: false },
        { name: 'EvenNo', title: '@Resources.Locale.L_EventSetup_EvenNo', index: 'EvenNo', width: 60, align: 'left', sorttype: 'string', hidden: true },
        { name: 'Status', title: '@Resources.Locale.L_UserPermission_State', index: 'Status', width: 100, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'F:@GetLangText("L_EventSetup_Complete");R:Error;E:Error' } },
        { name: 'ProcessDate', title: '@Resources.Locale.L_EventSetup_NotifyDate', index: 'ProcessDate', width: 150, align: 'left', sorttype: 'string', hidden: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d H:i" } },
        { name: 'NotifyMail', title: '@Resources.Locale.L_EventSetup_NotifyMail', index: 'NotifyMail', width: 700, align: 'left', sorttype: 'string', hidden: false }
    ];
    $SubGrid = $("#SubGrid");
    new genGrid(
        $SubGrid,
        {
            datatype: "local",
            data: [],
            loadonce: true,
            colModel: colModel,
            caption: '@Resources.Locale.L_EventSetup_Schedetails',
            height: "auto",
            refresh: true,
            cellEdit: false,//禁用grid编辑功能
            exportexcel: false,
            footerrow: true,
            sortname: "EvenNo",
            sortorder: "asc"
        }
    );
    _dm.addDs("SubGrid", [], ["UId"], $SubGrid[0]);
});