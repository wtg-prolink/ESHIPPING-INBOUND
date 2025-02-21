var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var gridSetting = {};

jQuery(document).ready(function ($) {
    function setDate(id) {
        $("#" + id).wrap('<div class="input-group">').datepicker({
            showOn: "button",
            changeYear: true,
            dateFormat: "yy-mm",
            beforeShow: function () {
                setTimeout(function () {
                    $('.ui-datepicker').css('z-index', 99999999999999);
                }, 0);
            },
            onClose: function (text, inst) {
                $(this).focus();
            }
        }).next("button").button({ icons: { primary: "ui-icon-calendar" }, label: "Select a date", text: false })
            .addClass("btn btn-sm btn-info").html("<span class='glyphicon glyphicon-calendar'></sapn>")
            .wrap('<span class="input-group-btn">')
            .find('.ui-button-text')
            .css({
                'visibility': 'hidden',
                'display': 'inline'
            });
    }
    setDate("FSearchRDate");
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;
    var EtypeOption = {};
    EtypeOption.gridUrl = rootPath + "Common/GetBscsBsCodeData";
    EtypeOption.param = "sopt_CdType=eq&CdType=EXTP" + "&sopt_GroupId=eq&GroupId="+groupId;
    EtypeOption.gridReturnFunc = function (map) {
        var $ExchangeRateGrid = $("#ExchangeRateGrid");
        var value = map.Cd;
        return value;
    };
    EtypeOption.lookUpConfig = LookUpConfig.TrgnLookup;
    EtypeOption.autoCompKeyinNum = 1;
    EtypeOption.gridId = "ExchangeRateGrid";
    EtypeOption.autoCompUrl = "";
    EtypeOption.autoCompDt = "dt=bsc";
    EtypeOption.autoCompParams = "CD=showValue,CD";
    EtypeOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.Cd);
        var $ExchangeRateGrid = $("#ExchangeRateGrid");
    }

    var colModel =[
    {
        name: 'Etype', title: "@Resources.Locale.L_ExchangeRate_Etype", index: 'Etype', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false, edittype: 'custom', editoptions: gridLookup(EtypeOption)
    },
        {
            name: 'Edate', title: "@Resources.Locale.L_ExchangeRate_Edate", index: 'Edate',
            width: 150, align: 'left', sorttype: 'string',
            hidden: false, editable: false, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y/m/d',
                defaultValue: null
            }

        },
        { name: 'Fcur', title: "@Resources.Locale.L_ExchangeRate_Fcur", index: 'Fcur', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Tcur', title: "@Resources.Locale.L_ExchangeRate_Tcur", index: 'Tcur', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'ExRate', title: "@Resources.Locale.L_ExchangeRate_ExRate", index: 'ExRate', width: 150,  align: 'right', formatter: 'number', hidden: false, editable: true, formatoptions: { decimalPlaces: 6 } },
        { name: 'Remark', title: "@Resources.Locale.L_BSCSSetup_Remark", index: 'Remark', width: 350, align: 'left', sorttype: 'string', hidden: false, editable: true }
        ];


    $grid = $("#ExchangeRateGrid");
    _dm.addDs("ExchangeRateGrid", [], ["Etype"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    data: [],
    	    loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "SYSTEM/ExchangeRateQuery",
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
    	    caption: "@Resources.Locale.L_ExchangeRate_ExRateSetup",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("ExchangeRateGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: ["Etype","Edate","Fcur","Tcur"],
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#ExchangeRateGrid").setColProp('Etype', { editable: true });
    	        } else {
    	            $("#ExchangeRateGrid").setColProp('Etype', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#ExchangeRateGrid").setColProp('Etype', { editable: true });
    	        $("#ExchangeRateGrid").setColProp('Edate', { editable: true });
    	        $("#ExchangeRateGrid").setColProp('Fcur', { editable: true });
    	        $("#ExchangeRateGrid").setColProp('Tcur', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit", "MBSync"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "ExchangeRateGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "ExchangeRateGrid" });
        editable = true;
        MenuBarFuncArr.Disabled(["MBSync"]);
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#ExchangeRateGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/ExchangeRateUpdate",
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
                CommonFunc.Notify("", "@Resources.Locale.L_RouteSetup_SaveSuccess", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                //location.reload();
                gridEditableCtrl({ editable: false, gridId: "ExchangeRateGrid" });
                editable = false;
                MenuBarFuncArr.Enabled(["MBSync"]);
            }
        });
        return dtd.promise();
    }


    MenuBarFuncArr.AddMenu("MBSync", "glyphicon glyphicon-repeat", "Sync ExChange Rate", function(){
        $.ajax({
            url: rootPath + "System/syncExchageRate",
            type: 'POST',
            data: {},
            async: false,
            beforeSend: function(){
                CommonFunc.ToogleLoading(true);
            },
            error: function() {
                CommonFunc.Notify("", "error", 1300, "warning");
                CommonFunc.ToogleLoading(false);
                return false;
            },
            success: function (data) {
                CommonFunc.ToogleLoading(false);
                var d_data = $.parseJSON(data);
                $("#ExchangeRateGrid").jqGrid("clearGridData");
                $("#ExchangeRateGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    sortorder: "asc",
                    sortname: "Fcur",
                    data: d_data,
                }).trigger("reloadGrid");
                CommonFunc.Notify("", "@Resources.Locale.L_ExchangeRate_SynS", 500, "success");
            },
            cache: false
        });
    });


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

    $("#searchBtn").click(function () {
        inqData();
    });

    function inqData() {
        //if (!_grid) return;
        //_cdata = {};
        //var pms = [];
      

        var params = "";
       
        var baseCondition = "";
        var vir_params = "";
       
        $('#ExchangeRateGrid').jqGrid('setGridParam', {
            url: rootPath + "System/ExchangeRateQuery", datatype: "json",
            postData: {
                FSearchRDate: encodeURIComponent($("#FSearchRDate").val() || ''),
                Fcur: encodeURIComponent($("#Fcur").val() || ''),
                Tcur: encodeURIComponent($("#Tcur").val() || ''),
                'baseCondition': baseCondition,
                'virConditions': vir_params
            }
        }).trigger("reloadGrid")
    }
});

