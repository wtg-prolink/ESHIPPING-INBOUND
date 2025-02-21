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

    var colModel = [
             { name: 'CmpCd', title: 'Company Code', index: 'CmpCd', width: 180, sorttype: 'string', editable: true },
             { name: 'PartyNo', title: 'Party No', index: 'PartyNo', width: 180, sorttype: 'string', editable: true },
             { name: 'GroupId', title: 'GroupId', index: 'GroupId', width: 180, align: 'right', editable: true },
             { name: 'Cmp', title: 'Cmp', index: 'Cmp', width: 180, sorttype: 'string', editable: true, hidden: true },
             { name: 'UId', title: 'Uid', index: 'UId', width: 200, sorttype: 'string', editable: true, hidden: true }
    ];

    $grid = $("#PartyFilterGrid");
    _dm.addDs("PartyFilterGrid", [], ["Cur"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    data: [],
    	    loadonce: true,
    	    colModel: colModel,
            datatype: "json",
    	    url: rootPath + "System/SmptyFilterQueryData",
            cellEdit: false,//禁用grid编辑功能 
            isModel: true,
    	    caption: "Party No Filter",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("PartyFilterGrid"),
    	    sortorder: "asc",
    	    sortname: "PartyNo",
    	    delKey: "UId",
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#PartyFilterGrid").setColProp('Cur', { editable: true });
    	        } else {
    	            $("#PartyFilterGrid").setColProp('Cur', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#PartyFilterGrid").setColProp('Cur', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "PartyFilterGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "PartyFilterGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#PartyFilterGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/SmptyFilterUpdate",
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
                gridEditableCtrl({ editable: false, gridId: "PartyFilterGrid" });
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

