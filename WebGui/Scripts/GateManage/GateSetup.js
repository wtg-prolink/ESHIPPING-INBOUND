var $MainGrid = $("#MainGrid");
var $SubGrid  = $("#SubGrid");
var gridHeight = 0;
var colModel1, colModel2;

var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var editable = false;
var _mainCmpData = {},//由放大镜查询出来的公司数据
    mainKeyValue = {},
    gridSetting = {};

jQuery(document).ready(function($) {
	var winHeight = $(window).height() - 270;
	gridHeight = winHeight / 2;

	_initBar();
	gridSetup();
	_initLoadData();

	var CmpOpt = {};
	CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
	CmpOpt.param = "";
	CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
	CmpOpt.registerBtn = $("#CmpLookup");
	CmpOpt.focusItem = $("#Cmp");
	CmpOpt.gridFunc = function (map) {
	    var value = map.Cd;
	    var CmpNm = map.CdDescp;
	    $("#Cmp").val(value);
	};
	CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
	initLookUp(CmpOpt);
	CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP", function (event, ui) {
	    $(this).val(ui.item.returnValue.CMP);
	    return false;
	});

});

function gridSetup()
{
	//使用者id
	var options = {};
	options.gridUrl = rootPath + "System/UserSetInquiryData";
	options.gridReturnFunc = function (map) {
	    var value = map.UId;
	    return value;
	}
	options.selfSite = true;
	options.param = "";
	options.lookUpConfig = LookUpConfig.UserLookup;
	options.autoCompKeyinNum = 1;
	options.gridId = "MainGrid";
	options.autoCompUrl = "";
	options.autoCompDt = "dt=user&GROUP_ID=" + groupId + "&U_ID%";
	options.autoCompParams = "U_ID&U_EMAIL=showValue,U_ID,U_EMAIL";
	options.autoCompFunc = function (elem, event, ui, rowid) {
	    $(elem).val(ui.item.returnValue.U_ID);
	}

    colModel1 = [
    	{ name: 'UId', title: 'u id', index: 'UId', sorttype: 'string', hidden: true },
    	{ name: 'GroupId', title: 'Group ID', index: 'GroupId', sorttype: 'string', hidden: true },
    	{ name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
        { name: 'WsCd', title: '@Resources.Locale.L_GateAnalysis_WsCd', index: 'WsCd', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: true, classes: "uppercase" },
        { name: 'WsNm', title: '@Resources.Locale.L_GateSetup_WsNm', index: 'WsNm', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'GateNumber', title: '@Resources.Locale.L_GateSetup_GateNumber', index: 'GateNumber', width: 60, align: 'right', formatter: 'integer', hidden: false, editable: true },
        { name: 'ProductLine', title: '@Resources.Locale.L_GateReserveSetup_ProLine', index: 'ProductLine', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'RefGate', title: '@Resources.Locale.L_GateSetup_RefGate', index: 'RefGate', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
	    { name: 'Pic', title: '@Resources.Locale.L_GateSetup_Pic', index: 'Pic', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(options) },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

    colModel2 = [
    	{ name: 'UId', title: 'u id', index: 'UId', sorttype: 'string', hidden: true },
    	{ name: 'UFid', title: 'uf id', index: 'UFid', sorttype: 'string', hidden: true },
    	{ name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
	    { name: 'WsCd', title: 'WsCd', index: 'WsCd', sorttype: 'string', hidden: true, editable: false, width: 90},
   		{ name: 'GateNo', title: '@Resources.Locale.L_GateSetup_RefGate', index: 'GateNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
	    { name: 'Lift', title: '@Resources.Locale.L_GateSetup_Lift', index: 'Lift', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }, editable: true },
	    { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: '@Resources.Locale.L_FCLChgSetup_Script_168', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'SuspendFrom', 
            title: '@Resources.Locale.L_GateSetup_SuspendFrom',
            index: 'SuspendFrom', 
            width: 150, 
            align: 'left', 
            sorttype: 'string', 
            hidden: false, 
            editable: true, 
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
              srcformat: 'ISO8601Long',
              newformat: 'Y-m-d',
              defaultValue:null
            }
        },
        { name: 'SuspendTo', 
            title: '@Resources.Locale.L_GateSetup_SuspendTo',
            index: 'SuspendTo', 
            width: 150, 
            align: 'left', 
            sorttype: 'string', 
            hidden: false, 
            editable: true, 
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
              srcformat: 'ISO8601Long',
              newformat: 'Y-m-d',
              defaultValue:null
            }
        }
    ];

    gridSetting.gridId = "SubGrid";
    gridSetting.colModel = colModel2;

    var gridOpt1 = {
    	colModel: colModel1, 
    	caption: '@Resources.Locale.L_BaseLookup_WarehouseList', 
    	delKey: ["WsCd"],
    	datatype: "local",
        loadonce: true,
        data: [],
        height: gridHeight,
        refresh: true,//是否允许刷新
        cellEdit: false,//禁用grid编辑功能
        exportexcel: false,//是否可导出excel
        pginput: false,//禁止bar上有输入框
        sortable: false,//禁止排序
        pgbuttons: false,//是否有分页按钮
        toppager: true,//是否有头上那个分页的bar
        rows: 999999,
    	onSelectRowFunc: function (map) {

	        gridSetting.keyData = { WsCd: mainKeyValue.WsCd};
	        getGridChangeDataDS(gridSetting);
	        //_editGrid(true);
	        var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
	        //var WsCd = $MainGrid.jqGrid('getGridParam', "getGridCellValueCustom")($MainGrid, selRowId, "WsCd", "");
	        //var WsCd = getGridVal($MainGrid, selRowId, "WsCd", null);
	        mainKeyValue.WsCd = map.WsCd;
	        var WsCd = null;
	        if (typeof map.WsCd != "undefined") {
	            WsCd = map.WsCd.replace(/<.*?>/g, '');
	        }

	        if (WsCd != null && WsCd != "") {
	            //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
	            if (_oldDeatiArray[WsCd] != undefined || _oldDeatiArray[WsCd] != null) {
	                //将json设置给GoodsCInfoGrid
	                //移除_state状态为0的数据，，因为_state的数据是删除的数据
	                $.each(_oldDeatiArray[WsCd], function (i, val) {
	                    if (val._state == "0") {
	                        _oldDeatiArray[WsCd].splice(i, 1);
	                    }
	                });
	                _dm.getDs("SubGrid").setData(_oldDeatiArray[WsCd]);
	                return;
	            }
	            $.ajax({
	                async: true,
	                url: rootPath + "GateManage/GetWhGateData",
	                type: 'POST',
	                data: {
	                    "WsCd": WsCd,
	                },
	                dataType: "json",
	                "complete": function (xmlHttpRequest, successMsg) {
	                    if (successMsg != "success") return null;
	                },
	                success: function (result) {
	                    _oldDeatiArray[WsCd] = result.rows;
	                    if (_dm.getDs("SubGrid") == null || _dm.getDs("SubGrid") == undefined) {
	                        _dm.addDs("SubGrid", result.rows, ["UId"], $SubGrid[0]);
	                    } else {
	                        _dm.getDs("SubGrid").setData(result.rows);
	                    }
	                }
	            });

	        }
    	},
    	delRowFunc: function (rowid) {
    	    $SubGrid.jqGrid("clearGridData");
    	    return true;
    	},
	    beforeSelectRowFunc: function (rowid) {
	        //main key 修改時不允與修改
	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
	            $MainGrid.setColProp('WsCd', { editable: true });
	        } else {
	            $MainGrid.setColProp('WsCd', { editable: false });
	        }

	    },
	    beforeAddRowFunc: function () {
            //add row 時要可以編輯main key
	        $("#MainGrid").setColProp('WsCd', { editable: true });
        },
        onAddRowFunc: function(rowid){
        	var UId = $MainGrid.jqGrid('getCell', rowid, "UId");

        	if (UId == "" || UId == null) {
        	    UId = genUid(uuid());
        	    setGridVal($MainGrid, rowid, "UId", UId, null);
        	}
        	setGridVal($MainGrid, rowid, "Cmp",  $("#txt_Cmp").val(), null);

        },
    }
    _dm.addDs("SubGrid", [], ["UId"], $SubGrid[0]);
    var gridOpt2 = {
    	colModel: colModel2, 
    	caption: '@Resources.Locale.L_DNManage_GateList', 
    	delKey: ["UId"],
    	datatype: "local",
        loadonce: true,
        data: [],
        height: gridHeight,
        refresh: true,//是否允许刷新
        cellEdit: false,//禁用grid编辑功能
        exportexcel: false,//是否可导出excel
        pginput: false,//禁止bar上有输入框
        sortable: false,//禁止排序
        pgbuttons: false,//是否有分页按钮
        toppager: true,//是否有头上那个分页的bar
        rows: 999999,
        ds: _dm.getDs("SubGrid"),
        onAddRowFunc: function(rowid){
        	var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
        	var Cmp = $MainGrid.jqGrid('getCell', selRowId, "Cmp");
        	var UId = $MainGrid.jqGrid('getCell', selRowId, "UId");
        	setGridVal($SubGrid, rowid, "Cmp", Cmp, null);
        	setGridVal($SubGrid, rowid, "UFId", UId, null);

        },
	    beforeSelectRowFunc: function (rowid) {
	        //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('GateNo', { editable: true });
            } else {
                $SubGrid.setColProp('GateNo', { editable: false });
            }
        },
	    beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
	        $SubGrid.setColProp('GateNo', { editable: true });

	        //當抓到的品號為空時，必須重新回主檔抓，若還是抓不到則無法新增
	        if (mainKeyValue.WsCd == null || mainKeyValue.WsCd == "") {
	            var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
	            //var WsCd = $MainGrid.jqGrid('getCell', selRowId, 'WsCd');
	            //var WsCd = $MainGrid.jqGrid('getGridParam', "getGridCellValueCustom")($MainGrid, selRowId, "WsCd", "");
	            var WsCd = getGridVal($MainGrid, selRowId, "WsCd", null);

	            if (WsCd != null && WsCd != "") {
	                mainKeyValue.WsCd = WsCd;

	            } else {
	                //alert("主档品号有误，无法建立国别");
	                CommonFunc.Notify("", "@Resources.Locale.L_GateSetup_Script_162 @Resources.Locale.L_GateSetup_Scripts_281", 500, "danger");
	                //$("#GoodsCInfoGrid").jqGrid("clearGridData");
	                return false;
	            }
	        }
        }
    }

	new genGrid($MainGrid, gridOpt1);
	new genGrid($SubGrid, gridOpt2);
}

function _editGrid(tf)
{
	if(tf === true && editable == true)
	{
		gridEditableCtrl({ editable: false, gridId: "SubGrid" }); 
		gridEditableCtrl({ editable: true, gridId: "SubGrid" }); 

	}
	else
	{

		gridEditableCtrl({ editable: false, gridId: "SubGrid" }); 

	}
}

function _initBar()
{
	MenuBarFuncArr.MBEdit = function(){
		var MainSelectedRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
		editable = true;
		gridEditableCtrl({ editable: true, gridId: "MainGrid" }); 
		gridEditableCtrl({ editable: true, gridId: "SubGrid" });
		$MainGrid.jqGrid('setSelection', MainSelectedRowId, true);
	}

	MenuBarFuncArr.MBCancel = function(){
		editable = false;
		MenuBarFuncArr.Enabled(["MBEdit"]);
		gridEditableCtrl({ editable: false, gridId: "MainGrid" });
	}

	MenuBarFuncArr.MBSave = function(dtd){
		gridEditableCtrl({ editable: false, gridId: "MainGrid" });
		gridEditableCtrl({ editable: false, gridId: "SubGrid" });

		var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
		$MainGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
		$MainGrid.jqGrid('getGridParam', "endEdit")();

		selRowId = $SubGrid.jqGrid('getGridParam', 'selrow');
		$SubGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
		$SubGrid.jqGrid('getGridParam', "endEdit")();


		//获取子表的changeValue
		gridSetting.keyData = { WsCd: mainKeyValue.WsCd };
		getGridChangeDataDS(gridSetting);

		var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = {};
		changeData["mt"] = containerArray;
		changeData["sub"] = _changeDatArray;
		console.log(changeData);
		$.ajax({
		    async: true,
		    url: rootPath + "GateManage/SaveWhData",
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
		        if (result.message == null) return;
		        //alert(result.message);
		        if(result.message == "success")
		        {
		        	setdisabled(true);
		        	setToolBtnDisabled(true);
		        	CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
		        	MenuBarFuncArr.SaveResult = true;
		        	dtd.resolve();

		        	MenuBarFuncArr.MBCancel();
		        }
		        else
		        {
		        	CommonFunc.Notify("", result.message, 500, "danger");
		        	MenuBarFuncArr.SaveResult = false;
		        	dtd.resolve();
		        }
		        
		    }
		});

		return dtd.promise();
	}

	initMenuBar(MenuBarFuncArr);

	MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBSearch", "MBErrMsg"]);  //Delete不需要的Menu
	MenuBarFuncArr.Disabled(["MBSave"]); //Disable Menu
	MenuBarFuncArr.Enabled(["MBEdit"]);  //Enabled Menu
}

function _initLoadData()
{
	$.ajax({
	    async: true,
	    url: rootPath + "GateManage/GetWareHouseData",
	    type: 'POST',
	    data: {},
	    dataType: "json",
	    beforeSend: function () {
	        CommonFunc.ToogleLoading(true);
	    },
	    "error": function (xmlHttpRequest, errMsg) {
	    },
	    success: function (result) {
	    	console.log(result);
	    	var mainGridData = result.rows;

	    	//_dm.getDs("MainGrid").setData(mainGridData);
	        $MainGrid.jqGrid("setGridParam", {
	            datatype: 'local',
	            data: mainGridData
	        }).trigger("reloadGrid");

	        CommonFunc.ToogleLoading(false);
	    }
	});
}
