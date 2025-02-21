var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 160;

    function getLookupOp(gridId, op, op1, op2) {
        var $grid = $("#" + gridId);
        var opt = {};
        opt.gridUrl = op.url;
        opt.selfSite = false;
        opt.param = "";
        opt.gridReturnFunc = function (map) {
            if (op.returnFn)
                return op.returnFn(map, $grid);
        };
        opt.lookUpConfig = op.config;
        opt.gridId = gridId;

        //自动带入
        opt.autoCompKeyinNum = 1;
        opt.autoCompUrl = "";
        opt = $.extend(opt, op1);
        opt = $.extend(opt, op2);
        return opt;
    }

    //All column：StsCd,Edescp,Ldescp,Pict1,Pict2,Location,Issingle,RefBy,CreateBy,
    //CreateDate, ModifyBy, ModifyDate

    //Key：StsCd
    $grid = $("#MainGrid");
    function getLocationTypeop(name) {
        var _name = name;
        var locationtype_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.LocationTypeUrl,
                config: LookUpConfig.LocationTypeLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TKLC", $grid,
            function ($grid, rd, elem) {
                $(elem).val(rd.CD);
            }));
        return locationtype_op;
    }

    var colModel = [
    	{ name: 'StsCd', title: '@GetLangText("L_TKQuery_StsCd")', index: 'StsCd', align: 'left', width: 80, sorttype: 'string', editable: true },
    	{ name: 'Edescp', title: '@GetLangText("L_TKQuery_Edescp")', index: 'Edescp', align: 'left', width: 300, sorttype: 'string', editable: true },
    	{ name: 'Ldescp', title: '@GetLangText("L_TKQuery_Ldescp")', index: 'Ldescp', align: 'left', width: 150, sorttype: 'string', editable: true },
    	{ name: 'Location', title: '@GetLangText("L_TKQuery_Location")', index: 'Location', editoptions: gridLookup(getLocationTypeop("Location")), edittype: 'custom', align: 'left', width: 150, sorttype: 'string', editable: true },
    	{ name: 'OrderBy', title: '@GetLangText("L_TKQuery_OrderBy")', index: 'OrderBy', sorttype: 'string', width: 70, hidden: true, editable: false },
        { name: 'Issingle', title: '@GetLangText("L_TKQuery_Issingle")', index: 'Issingle', align: 'left', width: 70, sorttype: 'string', editable: true },
    	{ name: 'RefBy', title: 'Ref By', index: 'RefBy', sorttype: 'string', editable: true }
    ];


  
    _dm.addDs("MainGrid", [], ["StsCd"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    //data: [],
    	    loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "SYSTEM/TKSetupQuery",
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
    	    caption: "@Resources.Locale.L_TKSetup_Scripts_369",
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    //sortable: true,
    	    //ds: _dm.getDs("MainGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: "StsCd",
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#MainGrid").setColProp('StsCd', { editable: true });
    	        } else {
    	            $("#MainGrid").setColProp('StsCd', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {
    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#MainGrid").setColProp('StsCd', { editable: true });
    	    }
    	}
    );
  
    MenuBarFuncArr.MBCancel = function () {
        location.reload();
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#MainGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/TKSetupUpdate",
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
                    CommonFunc.Notify("", "@GetLangText('L_TKSetup_Fail')", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@GetLangText('L_TKSetup_Success')", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                location.reload();
            }
        });
        return dtd.promise();
    }


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

