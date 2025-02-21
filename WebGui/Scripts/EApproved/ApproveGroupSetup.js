var $MainGrid = $("#MainGrid");
var $SubGrid1 = $("#SubGrid1");
var $SubGrid2 = $("#SubGrid2");
var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
_changeDatArray["SubGrid1"] = [];
_changeDatArray["SubGrid2"] = [];
var mainKeyValue = {};
var sub1KeyValue = {};
var gridSetting = {};
var gridSetting2 = {};


//user lookup for grid
var options = {};
options.gridUrl = rootPath + "System/UserSetInquiryData";
options.param = "";
options.baseCondition = " GROUP_ID='" + groupId + "' AND CMP='" + cmp + "'";
options.gridReturnFunc = function (map) {
    var value = map.UId,
        UEmail = map.UEmail;
    var rowid = $SubGrid2.getGridParam("selrow");
    setGridVal($("#SubGrid2"), rowid, "UEmail", UEmail, null);
    return value;
}
options.selfSite = true;
options.lookUpConfig = LookUpConfig.UserLookup;
options.autoCompKeyinNum = 1;
options.gridId = "SubGrid2";
options.autoCompUrl = "";
options.autoCompDt = "dt=user&GROUP_ID=" + groupId + "&U_ID%";
options.autoCompParams = "U_ID&U_EMAIL=showValue,U_ID,U_EMAIL";
options.autoCompFunc = function (elem, event, ui, rowid) {
    $(elem).val(ui.item.returnValue.U_ID);
    setGridVal($("#SubGrid2"), rowid, "UEmail", ui.item.returnValue.U_EMAIL, null);
}

var colModel1 = [
    { name: 'UId', title: 'UId', index: 'UId', hidden: true },
    { name: 'ApproveAttr', title: _getLang("L_ApproveGroupSetup_ApproveAttr", "签核属性"), index: 'ApproveAttr', width: 130, sorttype: 'string', editable: true },
];

var colModel2 = [
			{ name: 'UId', title: 'UId', index: 'UId', hidden: true },
			{ name: 'UFid', title: 'UFid', index: 'UFid', hidden: true },
			{ name: 'ApproveAttr', title: 'ApproveAttr', index: 'ApproveAttr', width: 130, hidden: true, editable:false },
            { name: 'ApproveGroup', title: _getLang("L_ApproveGroupSetup_ApproveGroup", "群组代码"), index: 'ApproveGroup', width: 70, sorttype: 'string', editable: true },
            { name: 'GroupDescp', title: _getLang("L_ApproveGroupSetup_GroupDescp", "叙述"), index: 'GroupDescp', width: 180, sorttype: 'string', editable: true },
            { name: 'SeniorStaff', title: _getLang("L_SeniorStaff", "是否高层"), index: 'SeniorStaff', width: 70, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No' } },
            { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'number', hidden: false, editable: true },
	    ];
var colModel3 = [
    { name: 'UId', title: 'UId', index: 'UId', hidden: true },
    { name: 'UFid', title: 'UFid', index: 'UFid', hidden: true },
    { name: 'UFfid', title: 'UFfid', index: 'UFfid', hidden: true },
    { name: 'UserId', title: _getLang("L_ApproveGroupSetup_UserId", "User ID"), index: 'UserId', width: 150, sorttype: 'string', editable: true, edittype: 'custom', editoptions: gridLookup(options) },
    { name: 'UEmail', title: _getLang("L_ApproveGroupSetup_UEmail", "E-Mail"), index: 'UEmail', width: 180, sorttype: 'string', editable: true },
    { name: 'ByEmail', title: _getLang("L_ApproveGroupSetup_ByEmail", "邮件通知"), index: 'ByEmail', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
    { name: 'ByMsg', title: _getLang("L_ApproveGroupSetup_ByMsg", "讯息通知"), index: 'ByMsg', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
    { name: 'TvMnt', title: 'BU', index: 'TvMnt', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: "ALL:ALL;TV:TV;MNT:MNT;PD:PD", dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } }
];

jQuery(document).ready(function ($) {
    _menuSetup();
    var docHeight = $(document).height();
    gridHeight = docHeight - 130;

    gridSetting.gridId = "SubGrid1";
    gridSetting.colModel = colModel2;

    gridSetting2.gridId = "SubGrid2";
    gridSetting2.colModel = colModel3;

    new genGrid($MainGrid, {
        datatype: "local",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel1,
        isModel: true,
        caption: _getLang("L_ApproveGroupSetup_ApproveAttr", "签核属性"),
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        rows: 999,
        viewrecords: false,
        exportexcel: false,
        height: gridHeight,
        delKey: ["UId", "TvMnt"],
        onAddRowFunc: function (rowid) {
            var UId = $MainGrid.jqGrid('getCell', rowid, "UId");

            if (UId == "" || UId == null) {
                UId = genUid(uuid());
                $MainGrid.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
            }
        },
        onSelectRowFunc: function (map) {
            $SubGrid2.jqGrid("clearGridData");

            gridSetting.keyData = { "UFid": mainKeyValue.UFid, "ApproveAttr": mainKeyValue.ApproveAttr };
            getGridChangeDataDS(gridSetting);

            var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
            var UId = getGridVal($MainGrid, selRowId, "UId", null);
            if (UId != null && UId != "") {
                if (_oldDeatiArray[UId] != undefined || _oldDeatiArray[UId] != null) {
                    $.each(_oldDeatiArray[UId], function (i, val) {
                        if (typeof val._state !== "undefined") {
                            if (val._state == "0") {
                                _oldDeatiArray[UId].splice(i, 1);
                            }
                        }

                    });
                    _dm.getDs("SubGrid1").setData(_oldDeatiArray[UId]);
                    return;
                }
                $.ajax({
                    async: true,
                    url: rootPath + "EApproved/ApproveDQuery",
                    type: 'POST',
                    data: { "UFid": UId, "page": 1, "rows": 200 },
                    dataType: "json",
                    "error": function (xmlHttpRequest, errMsg) {
                    },
                    success: function (result) {
                        var mainTable = $.parseJSON(result.mainTable.Content);
                        var $grid = $("#SubGrid1");
                        _oldDeatiArray[UId] = mainTable.rows;
                        if (_dm.getDs("SubGrid1") == null || _dm.getDs("SubGrid1") == undefined) {
                            _dm.addDs("SubGrid1", mainTable.rows, ["UId"], $grid[0]);
                        } else {
                            _dm.getDs("SubGrid1").setData(mainTable.rows);
                        }
                    }
                });

            }
        },
        afterAddRowWithIdFunc: function (rowid, toolid) {
        },
        afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
        }
    });

    _dm.addDs("SubGrid1", [], ["UId"], $SubGrid1[0]);
    new genGrid($SubGrid1, {
        datatype: "local",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel2,
        isModel: true,
        caption: _getLang("L_ApproveGroupSetup_ApproveDep", "签核部门"),
        refresh: true,
        pginput: false,
        sortable: true,
        rows: 999,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        height: gridHeight,
        delKey: ["UId"],
        ds: _dm.getDs("SubGrid1"),
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $("#SubGrid1").setColProp('ApproveGroup', { editable: true });
            } else {
                $("#SubGrid1").setColProp('ApproveGroup', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            $("#SubGrid1").setColProp('ApproveGroup', { editable: true });
            if (mainKeyValue.UFid == null || mainKeyValue.UFid == "") {
                var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
                var UId = $("#MainGrid").jqGrid('getCell', selRowId, 'UId');
                var ApproveAttr = getGridVal($MainGrid, selRowId, 'ApproveAttr');
                if (UId != null && UId != "") {
                    mainKeyValue.UFid = UId;
                    mainKeyValue.ApproveAttr = ApproveAttr;
                } else {
                    CommonFunc.Notify("", _getLang("L_ApproveGroupSetup_Selc", "请先选择属性，再新增明细"), 500, "danger");
                    return false;
                }
            }
        },
        onAddRowFunc: function (rowid) {
            var UId = getGridVal($SubGrid1, rowid, "UId", null);

            if (UId == "" || UId == null) {
                UId = genUid(uuid());
                $SubGrid1.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
            }
            $SubGrid1.jqGrid('setCell', rowid, "SeniorStaff", "N");
        },
        onSelectRowFunc: function (map) {
            gridSetting2.keyData = { "UFid": sub1KeyValue.UFid, "UFfid": sub1KeyValue.UFfid };
            getGridChangeDataDS(gridSetting2);

            var selRowId = $SubGrid1.jqGrid('getGridParam', 'selrow');
            var UId = getGridVal($SubGrid1, selRowId, "UId", null);

            if (UId != null && UId != "") {
                if (_oldDeatiArray[UId] != undefined || _oldDeatiArray[UId] != null) {
                    $.each(_oldDeatiArray[UId], function (i, val) {
                        if (val._state == "0") {
                            _oldDeatiArray[UId].splice(i, 1);
                        }
                    });
                    _dm.getDs("SubGrid2").setData(_oldDeatiArray[UId]);
                    return;
                }
                $.ajax({
                    async: true,
                    url: rootPath + "EApproved/ApproveDPQuery",
                    type: 'POST',
                    data: { "UFid": UId, "page": 1, "rows": 200 },
                    dataType: "json",
                    "error": function (xmlHttpRequest, errMsg) {
                    },
                    success: function (result) {
                        var mainTable = $.parseJSON(result.mainTable.Content);
                        var $grid = $("#SubGrid2");
                        _oldDeatiArray[UId] = mainTable.rows;
                        if (_dm.getDs("SubGrid2") == null || _dm.getDs("SubGrid2") == undefined) {
                            _dm.addDs("SubGrid2", mainTable.rows, ["UId", "UFid"], $grid[0]);
                        } else {
                            _dm.getDs("SubGrid2").setData(mainTable.rows);
                        }
                    }
                });

            }
        },
        onSortCol: function (index, iCol, sortorder) {
            var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
            var UId = $("#MainGrid").jqGrid('getCell', selRowId, 'UId');

            if (UId != null && UId != "") {
                $SubGrid1.jqGrid("setGridParam", {
                    url: rootPath + "EApproved/ApproveSortQuery",
                    sortorder: sortorder,
                    sortname: index,
                    postData: { UFid: UId },
                    datatype: "json"
                }).trigger("reloadGrid");
            }
        }
    });
    _dm.addDs("SubGrid2", [], ["UId"], $SubGrid2[0]);
    new genGrid($SubGrid2, {
        datatype: "local",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: colModel3,
        isModel:true,
        caption: _getLang("L_ApproveGroupSetup_ApproveMem", "签核人员"),
        refresh: true,
        pginput: false,
        sortable: false,
        rows: 999,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        height: gridHeight,
        ds: _dm.getDs("SubGrid2"),
        delKey: ["UId"],
        beforeAddRowFunc: function (rowid) {

            $("#SubGrid2").setColProp('ApproveGroup', { editable: true });
            var selRowId = $SubGrid1.jqGrid('getGridParam', 'selrow');
            var selMainRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
            var UFid = getGridVal($SubGrid1, selRowId, "UId", null);
            var UFfid = getGridVal($MainGrid, selMainRowId, "UId", null);
            if (sub1KeyValue.UFid != UFid || sub1KeyValue.UFfid != UFfid) {
                if (UFfid != null && UFfid != "") {
                    sub1KeyValue.UFid = UFid;
                    sub1KeyValue.UFfid = UFfid;
                } else {
                    CommonFunc.Notify("", _getLang("L_ApproveGroupSetup_Selc", "请先选择属性，再新增明细"), 500, "danger");
                    return false;
                }
            }
        },
        onAddRowFunc: function (rowid) {
            var UId = getGridVal($SubGrid2, rowid, "UId", null);

            if (UId == "" || UId == null) {
                UId = genUid(uuid());
                $SubGrid2.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
            }
        }
    });
       

	
	$MainGrid.jqGrid("setGridParam", {
	    datatype: 'local',
	    sortorder: "asc",
	    sortname: "SeqNo",
	    data: DeAData
	}).trigger("reloadGrid");
	
});

function _menuSetup()
{
	MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBSearch", "MBEdoc", "MBCopy", "MBApprove", "MBInvalid", "MBErrMsg"]);

	MenuBarFuncArr.MBEdit = function () {
	    mainKeyValue.UFid = "";
	    sub1KeyValue.UFid = "";
	    sub1KeyValue.UFfid = "";
		gridEditableCtrl({ editable: true, gridId: "MainGrid" });
		gridEditableCtrl({ editable: true, gridId: "SubGrid1" });
		gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
	}

	MenuBarFuncArr.MBCancel = function(){
		$MainGrid.trigger("reloadGrid");
		$SubGrid1.trigger("reloadGrid");
		$SubGrid2.trigger("reloadGrid");
		MenuBarFuncArr.Enabled(["MBEdit"]);
		gridEditableCtrl({ editable: false, gridId: "MainGrid" });
		gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
		gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
	}

	MenuBarFuncArr.MBSave = function(dtd){
		var selRowId = $SubGrid1.jqGrid('getGridParam', 'selrow');
		var selMainRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
		var dUid = getGridVal($SubGrid1, selRowId, 'UId')
		
		var ApproveAttr = getGridVal($MainGrid, selMainRowId, 'ApproveAttr');
		var mUId = getGridVal($MainGrid, selMainRowId, 'UId');
		var allData1 = $SubGrid1.find("tr");
		$.each(allData1, function (i, val) {
		    if (typeof val.id != "undefined" && val.id != "") {
		        if (val.id.indexOf("jqg") == -1) {
		            setGridVal($SubGrid1, val.id, 'rn', val.id, null);
		        } 
		    }
		});

		if (sub1KeyValue.UFid != dUid || sub1KeyValue.UFid != mUId) {
            
            if (mUId != null && mUId != "") {
                sub1KeyValue.UFid = dUid;
                sub1KeyValue.UFfid = mUId;
            }
        }

        if (mainKeyValue.UFid == null || mainKeyValue.UFid == "") {
            if (mUId != null && mUId != "") {
                mainKeyValue.UFid = mUId;
                mainKeyValue.ApproveAttr = ApproveAttr;
            }
        }
        gridSetting.keyData = { "UFid" : mainKeyValue.UFid,"ApproveAttr" : mainKeyValue.ApproveAttr };
		getGridChangeDataDS(gridSetting);

		gridSetting2.keyData = { "UFid": sub1KeyValue.UFid, "UFfid": sub1KeyValue.UFfid };
		getGridChangeDataDS(gridSetting2);
		
		var containerArray = $('#MainGrid').jqGrid('getGridParam', "arrangeGrid")();
		var changeData = {};
		changeData["mt"] = containerArray;
		changeData["st1"] = _changeDatArray["SubGrid1"];
		changeData["st2"] = _changeDatArray["SubGrid2"];
		console.log(changeData);
		$.ajax({
		    async: true,
		    url: rootPath + "EApproved/SaveEAGroupData",
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
		    	console.log(result);
		        if (result.message != "success"){
                    CommonFunc.Notify(_getLang("L_MailFormatSetup_SaveF", "保存失败"), result.message, 500, "danger");
		        	MenuBarFuncArr.SaveResult = false;
		        	dtd.resolve();
		        	return;
		        } 

		        setdisabled(true);
		        setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
		        MenuBarFuncArr.SaveResult = true;
		        dtd.resolve();

		        gridEditableCtrl({ editable: false, gridId: "MainGrid" });
		        gridEditableCtrl({ editable: false, gridId: "SubGrid1" });
		        gridEditableCtrl({ editable: false, gridId: "SubGrid2" });

		        _initDs();
		        $MainGrid.jqGrid("setGridParam", {
		            datatype: 'local',
		            sortorder: "asc",
		            sortname: "SeqNo",
		            data: result.mainData
		        }).trigger("reloadGrid");
		    }
		});

		return dtd.promise();

	}

	initMenuBar(MenuBarFuncArr);
	MenuBarFuncArr.Enabled(["MBEdit"]);
}

    function _getLang(id, caption) {
        try {
            return GetLangCaption(id, caption);
        }
        catch (e) { }
        return caption || id;
    }


function _initDs()
{
	_dm.getDs("SubGrid1").setData([]);
	_dm.getDs("SubGrid2").setData([]);
	_oldDeatiArray = {};
	_changeDatArray = [];
	_changeDatArray["SubGrid1"] = [];
	_changeDatArray["SubGrid2"] = [];
}