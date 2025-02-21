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


    var dlvterOpt = {};
    dlvterOpt.gridUrl = rootPath + LookUpConfig.CityPortUrl;
    dlvterOpt.param = "";
    dlvterOpt.baseCondition = " 1=1";
    dlvterOpt.isMutiSel = true;
    dlvterOpt.columnID = "CntryCd";
    dlvterOpt.columnID2 = "PortCd";
    dlvterOpt.gridReturnFunc = function (map, rowid) {
        var podcd = map.CntryCd + map.PortCd;
        var value = podcd;
        return value;
    }
    dlvterOpt.lookUpConfig = LookUpConfig.CityPortsLookup;
    dlvterOpt.autoCompKeyinNum = 1;
    dlvterOpt.gridId = "SCMReqGrid";
    dlvterOpt.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].CntryCd + data[index].PortCd + "/";
        });
        str = str.substr(0, str.length - 1);
        var selectedId = $("#SCMReqGrid").jqGrid("getGridParam", "selrow");
        var $grid = $("#SCMReqGrid");
        setGridVal($grid, selectedId, 'Pod', str, "lookup");
    }

    var truckOpt = {};
    truckOpt.gridUrl = rootPath + "TPVCommon/GetBstportDataForLookup";
    truckOpt.param = "";
    truckOpt.baseCondition = " GROUP_ID='" + groupId + "'";
    truckOpt.isMutiSel = true;
    truckOpt.columnID = "CntryCd";
    truckOpt.columnID2 = "PortCd";
    truckOpt.gridReturnFunc = function (map, rowid) {
        var value =  map.PortCd;
        return value;
    }
    truckOpt.lookUpConfig = LookUpConfig.CityPortsLookup;
    truckOpt.autoCompKeyinNum = 1;
    truckOpt.gridId = "SCMReqGrid";
    truckOpt.responseMethod = function (data) {
        var str = "";
        $.each(data, function (index, val) {
            str = str + data[index].PortCd + "/";
        });
        str = str.substr(0, str.length - 1);
        var selectedId = $("#SCMReqGrid").jqGrid("getGridParam", "selrow");
        var $grid = $("#SCMReqGrid");
        setGridVal($grid, selectedId, 'SPod', str, "lookup");
    }

    function gettruckInfoOpt(name) {
        var _name = name;
        var cnty_op = getLookupOp("SCMReqGrid",
            {
                url: rootPath + "TPVCommon/GetBstportDataForLookup",
                config: LookUpConfig.CityPortsLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
                    setGridVal($grid, selRowId, 'CntryCd', map.CntryCd, "lookup");
                    return map.CntryCd;
                }
            }, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'CntryCd', rd.CNTRY_CD, null);
                $(elem).val(rd.CNTRY_CD);
            }));
        return cnty_op;
    }

    function getCmpop(name) {
        var Cmp_op = getLookupOp("SCMReqGrid",
            {
                url: rootPath + LookUpConfig.GetCmpUrl,
                config: LookUpConfig.CmpLookup,
                returnFn: function (map, $grid, selRowId) {
                    selRowId = selRowId || $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Cmp", map.Cd, "lookup");
                    return map.Cmp;
                }
            }, LookUpConfig.GetCmpAuto(groupId, $("#SCMReqGrid"), function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Cmp", rd.CMP, "lookup");
                $(elem).val(rd.CMP);
            },
            function () {
                return "GROUP_ID=" + groupId + " AND TYPE='1'";
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "GROUP_ID='" + groupId + "' AND TYPE='1'";
                }
            });
        return Cmp_op;
    }


    var colModel = [
        { name: 'UId', title: '', index: 'UId', sorttype: 'string', editable: false, hidden: true },
	    { name: 'GroupId', title: '', index: 'GroupId', sorttype: 'string', editable: false, hidden: true },
	    { name: 'Cmp', title: 'Location', index: 'Cmp', sorttype: 'string', width: 80, hidden: false, editable: true, editoptions: gridLookup(getCmpop("Cmp")), edittype: 'custom' },
        { name: 'Pod', title: 'POD', index: 'Pod', edittype: 'custom', sorttype: 'string', width: 300, hidden: false, editable: true, editoptions: gridLookup(dlvterOpt) },
        { name: 'SPod', title: 'Final Delivery Location', index: 'SPod', edittype: 'custom', sorttype: 'string', width: 300, hidden: false, editable: true, editoptions: gridLookup(truckOpt) },
        { name: 'AddDate', title: 'ADD Date', index: 'AddDate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'CreateBy', title: _getLang("L_Bsdate_CreateBy", "创建人"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CreateDate', title: _getLang("L_Bsdate_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', sorttype: 'date', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } },
        { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", "修改人"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } }
    ];

    $grid = $("#SCMReqGrid");
    _dm.addDs("SCMReqGrid", [], ["Cur"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    //datatype:  "json",
    	    //loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "System/SCMReqQuery",
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
    	    caption: "SCM Requested",
    	    height: gridHeight,
    	    rownumWidth: 50,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("SCMReqGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: "UId",
    	    beforeSelectRowFunc: function (rowid) {
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        // $("#SCMReqGrid").setColProp('UId', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "SCMReqGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "SCMReqGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $("#SCMReqGrid").jqGrid('getGridParam', "arrangeGrid")();

        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "System/SCMReqUpdate",
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
                gridEditableCtrl({ editable: false, gridId: "SCMReqGrid" });
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