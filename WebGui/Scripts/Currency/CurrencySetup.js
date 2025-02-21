var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;
    //All column：StsCd,Edescp,Ldescp,Pict1,Pict2,Location,Issingle,RefBy,CreateBy,
    //CreateDate, ModifyBy, ModifyDate

    //Key：StsCd

    var colModel = [
             { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', width: 180, sorttype: 'string', classes: "uppercase", editable: true },
             { name: 'CurDescp', title: '@Resources.Locale.L_NoticeSetup_Caption', index: 'CurDescp', width: 180, sorttype: 'string', editable: true },
             { name: 'DecimalNum', title: '@Resources.Locale.L_CurrencySetup_DecimalNum', index: 'DecimalNum', width: 180, align: 'right', formatter: 'integer', editable: true },
             { name: 'RoundType', title: '@Resources.Locale.L_CurrencySetup_RoundType', index: 'RoundType', width: 180, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: '0:@Resources.Locale.L_Currency_RoundD;1:@Resources.Locale.L_Currency_RoundU;2:@Resources.Locale.L_Currency_RoundO', defaultValue: '0' } },
             { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 200, sorttype: 'string', editable: true }
    ];
    var gop = {};
    //alert(gridHeight);
    gop.gridColModel = colModel;
    gop.AddUrl = { "url": rootPath + "DNManage/FCLBooking", "title": "@Resources.Locale.L_DNManage_FCLBk", "id": "FCLBooking" };
    gop.gridId = "containerInfoGrid";
    gop.multiselect = true;
    gop.gridAttr = { caption: "@Resources.Locale.L_BSTSetup_Book_fcl", height: gridHeight, refresh: true, exportexcel: true, conditions: encodeURI(loadCondition) };
    gop.gridSearchUrl = rootPath + "DNManage/ShippingBookingQueryData";
    gop.searchColumns = getSelectColumn(gop.gridColModel);
    var _baseCondition = "1=1 AND TRAN_TYPE ='F'";
    gop.baseCondition = _baseCondition;
    gop.multiboxonly = true;

    gop.searchColModel = [
        //{ name: 'VT_PartyNo|sp', title: '@Resources.Locale.L_DNApproveManage_OhsCode', sorttype: 'string' },//货代
        //{ name: 'VT_PartyNo|cr', title: '@Resources.Locale.L_DNApproveManage_HisCode', sorttype: 'string' },//卡车
        //{ name: 'VT_PartyNo|br', title: '@Resources.Locale.L_DNApproveManage_HisCode', sorttype: 'string' },//报关行
        //{ name: 'VT_PartyNo|fs', title: '@Resources.Locale.L_DNApproveManage_HisCode', sorttype: 'string' },//船公司
    ];

    var mergeColModel = [];
    $.merge(mergeColModel, gop.gridColModel);
    $.merge(mergeColModel, gop.searchColModel);
    gop.searchColumns = getSelectColumn(mergeColModel);

    //SAVE CONDITION 為避免以後須調整畫面，ID都要傳到元件
    gop.searchFormId = "ConditionArea";
    gop.searchDivId = "SearchArea";
    gop.StatusAreaId = "StatusArea";
    gop.BtnGroupId = "BtnGroupArea";



    $grid = $("#CurrencyGrid");
    _dm.addDs("CurrencyGrid", [], ["Cur"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    data: [],
    	    loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "SYSTEM/CurrencyQuery",
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
    	    caption: "@Resources.Locale.L_CurrencySetup_CurSetup",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("CurrencyGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: "Cur",
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#CurrencyGrid").setColProp('Cur', { editable: true });
    	        } else {
    	            $("#CurrencyGrid").setColProp('Cur', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#CurrencyGrid").setColProp('Cur', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "CurrencyGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "CurrencyGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#CurrencyGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/CurrencyUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_TKSetup_Success", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                gridEditableCtrl({ editable: false, gridId: "CurrencyGrid" });
                editable = false;

                dtd.resolve();
                //location.reload();
            }
        });
        return dtd.promise();
    }


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

