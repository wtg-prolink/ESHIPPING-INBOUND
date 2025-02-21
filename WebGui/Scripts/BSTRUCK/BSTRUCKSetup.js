var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
$MainGrid = $("#MainGrid");
$SubGrid = $("#SubGrid");
gridHeight = 0;

$(function () {
	var docHeight = $(document).height();
	gridHeight = docHeight - 200;
	_initBar();
	_initGenGrid();
	initLoadData($("#UFid").val());

	$("#SearchBtn").click(function(){
		var Cmp = $("#Cmp").val();
		var PartyNo = $("#PartyNo").val();
		$MainGrid.jqGrid("clearGridData");
		$SubGrid.jqGrid("clearGridData");
		$.post(rootPath + "BSTRUCK/TruckQuery", {"Cmp": Cmp, "PartyNo": PartyNo}, function(data, textStatus, xhr) {
			

			$MainGrid.jqGrid("setGridParam", {
			    datatype: 'local',
			    data: data.mainGridData
			}).trigger("reloadGrid");

			$SubGrid.jqGrid("setGridParam", {
			    datatype: 'local',
			    data: data.subGridData
			}).trigger("reloadGrid");
		}, "JSON");
	});

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
	    $("#CmpNm").val(CmpNm);
	};
	CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
	initLookUp(CmpOpt);
	CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
	    $(this).val(ui.item.returnValue.CMP);
	    $("#CmpNm").val(ui.item.returnValue.NAME);
	    return false;
	});

	setSmptyData("PartyNoLookup", "PartyNo", "PartyNm", "CR");
});

function _initBar()
{
	MenuBarFuncArr.MBEdit = function(){
		var MainSelectedRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
		gridEditableCtrl({ editable: true, gridId: "MainGrid" }); 
		gridEditableCtrl({ editable: true, gridId: "SubGrid" });
		$MainGrid.jqGrid('setSelection', MainSelectedRowId, true);
	}

	MenuBarFuncArr.MBCancel = function(){
		MenuBarFuncArr.Enabled(["MBEdit"]);
		gridEditableCtrl({ editable: false, gridId: "MainGrid" });
		gridEditableCtrl({ editable: false, gridId: "SubGrid" });
	}

	MenuBarFuncArr.MBSave = function(dtd){

		var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
		$MainGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
		$MainGrid.jqGrid('getGridParam', "endEdit")();

		selRowId = $SubGrid.jqGrid('getGridParam', 'selrow');
		$SubGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
		$SubGrid.jqGrid('getGridParam', "endEdit")();

		var MainArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
		var SubArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = {};
		changeData["mt"] = MainArray;
		changeData["sub"] = SubArray;
		console.log(changeData);

		var ids = $SubGrid.jqGrid('getDataIDs');
		var SpPlant = "";
		var msg = "";
		for (var i = 0 ; i < ids.length; i++) {
		    var _SpPlant = getGridVal($SubGrid, ids[i], "SpPlant", null);
		    if (_SpPlant != "" && _SpPlant.length < 4)
		        if (SpPlant.indexOf(_SpPlant) > -1)
		        {
		            //alert("@Resources.Locale.L_BSTRUCKSetup_TruckInfo" + "：" + "@Resources.Locale.L_Bstrucksetup_OnlyLocat");
		            msg = "@Resources.Locale.L_BSTRUCKSetup_TruckInfo" + "：" + "@Resources.Locale.L_BSTRUCKSetup_OnlyLocal\n";
		            break;
		        }
		        SpPlant += _SpPlant + ";";
		}
		var _ids = $MainGrid.jqGrid('getDataIDs');
		SpPlant = "";
		for (var i = 0 ; i < _ids.length; i++) {
		    var _SpPlant = getGridVal($MainGrid, _ids[i], "SpPlantD", null);
		    if (_SpPlant != "" && _SpPlant.length < 4)
		        if (SpPlant.indexOf(_SpPlant) > -1) {
		            //alert("@Resources.Locale.L_BSTRUCKSetup_DriverInfo"+"："+"@Resources.Locale.L_Bstrucksetup_OnlyLocat");
		            msg += "@Resources.Locale.L_BSTRUCKSetup_DriverInfo" + "：" + "@Resources.Locale.L_BSTRUCKSetup_OnlyLocal";
		            break;
		        }
		    SpPlant += _SpPlant + ";";
		}
		if (msg != "")
		{
		    CommonFunc.Notify("", msg, 500, "warning");
		    MenuBarFuncArr.SaveResult = false;
		    dtd.resolve();
		}
		else {
		    $.ajax({
		        async: true,
		        url: rootPath + "BSTRUCK/SaveBstruckData",
		        type: 'POST',
		        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, Cmp: $("#Cmp").val(), PartyNo: $("#PartyNo").val(), UFid: $("#UFid").val() },
		        dataType: "json",
		        "error": function (xmlHttpRequest, errMsg) {
		            CommonFunc.Notify("", errMsg, 500, "danger");
		            MenuBarFuncArr.SaveResult = false;
		            dtd.resolve();
		        },
		        success: function (result) {
		            if (result.message == null) return;

		            if (result.message == "success") {
		                setdisabled(true);
		                setToolBtnDisabled(true);
		                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
		                MenuBarFuncArr.SaveResult = true;
		                dtd.resolve();

		                $MainGrid.jqGrid("setGridParam", {
		                    datatype: 'local',
		                    data: result.mainData
		                }).trigger("reloadGrid");

		                $SubGrid.jqGrid("setGridParam", {
		                    datatype: 'local',
		                    data: result.subData
		                }).trigger("reloadGrid");

		                MenuBarFuncArr.MBCancel();
		            }
		            else {
		                CommonFunc.Notify("", result.message, 500, "warning");
		                MenuBarFuncArr.SaveResult = false;
		                dtd.resolve();
		            }

		            gridEditableCtrl({ editable: false, gridId: "MainGrid" });
		            gridEditableCtrl({ editable: false, gridId: "SubGrid" });

		        }
		    });

		    return dtd.promise();
		}
	}

	initMenuBar(MenuBarFuncArr);

	MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBSearch", "MBErrMsg"]);  //Delete不需要的Menu
	MenuBarFuncArr.Disabled(["MBSave"]); //Disable Menu
	MenuBarFuncArr.Enabled(["MBEdit"]);  //Enabled Menu
}

function _initGenGrid()
{
		var colModel1 = [
	            { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
	            { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', hidden: true },
	            { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
	            { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
	            { name: 'PartyNo', title: 'PartyNo', index: 'PartyNo', sorttype: 'string', hidden: true },
	            { name: 'DriverName', title: '@Resources.Locale.L_GateReserve_Driver', index: 'DriverName', width: 70, sorttype: 'string', editable: true },
	            {
	                name: 'DriverId', title: '@Resources.Locale.L_GateReserve_DriverId', index: 'DriverId', width: 110, sorttype: 'string', editable: true, formatter: function (cellvalue, options, rowObject) {
	                    var val = "";
	                    if (cellvalue == null || cellvalue === undefined || cellvalue == 0)
	                        val = "";
	                    else {
	                        val = cellvalue;
	                        if (val.length > 30) {
	                            alert("@Resources.Locale.L_BSTRUCKSetup_Script_82" + '@Resources.Locale.L_GateReserve_DriverId' + "@Resources.Locale.L_BSTRUCKSetup_Script_83");
	                            val = "";
	                        } 
	                    }
	                    return val;
	                }
	            },
	            {
	                name: 'DriverPhone', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'DriverPhone', width: 100, sorttype: 'string', editable: true, formatter: function (cellvalue, options, rowObject) {
	                    var val = "";
	                    if (cellvalue == null || cellvalue === undefined || cellvalue == 0)
	                        val = "";
	                    else {
	                        val = cellvalue;
	                        if (val.length > 20) {
	                            alert("@Resources.Locale.L_BSTRUCKSetup_Script_82" + '@Resources.Locale.L_BSCSSetup_CmpTel' + "@Resources.Locale.L_BSTRUCKSetup_Script_84");
	                            val = "";
	                        }
	                    }
	                    return val;
	                }
	            },
	            { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 180, sorttype: 'string', editable: true },
		        { name: 'SpPlantD', title: '@Resources.Locale.L_AutoNoManage_Cmp', index: 'SpPlantD', width: 100, sorttype: 'string', editoptions: gridLookup(getcust("SpPlantD", "MainGrid")), edittype: 'custom', editable: true }
	    ];

		var colModel2 = [
	            { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
	            { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', hidden: true },
	            { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
	            { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
	            { name: 'PartyNo', title: 'PartyNo', index: 'PartyNo', sorttype: 'string', hidden: true },
	            { name: 'TruckNo', title: '@Resources.Locale.L_GateReserve_TruckNo', index: 'TruckNo', width: 180, sorttype: 'string', editable: true },
	            //{ name: 'DriverName', title: '@Resources.Locale.L_GateReserve_Driver', index: 'DriverName', width: 200, sorttype: 'string', editable: true },
	            { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 180, sorttype: 'string', editable: true },
		        { name: 'SpPlant', title: '@Resources.Locale.L_AutoNoManage_Cmp', index: 'SpPlant', width: 100, sorttype: 'string', editoptions: gridLookup(getcust("SpPlant", "SubGrid")), edittype: 'custom', editable: true }
	    ];


	    new genGrid(
	    	$MainGrid,
	    	{
	    	    data: [],
	    	    loadonce: true,
	    	    colModel: colModel1,
	    	    datatype: "local",
				cellEdit: false,//禁用grid编辑功能
				isModel:true,
	    	    caption: "@Resources.Locale.L_BSTRUCKSetup_DriverInfo",
	    	    height: gridHeight,
	    	    //rownumWidth: 50,
	    	    refresh: true,
	    	    rows: 9999,
	    	    exportexcel: false,
	    	    pginput: false,
	    	    pgbuttons: false,
	    	    sortorder: "asc",
	    	    sortname: "DriverName",
	    	    delKey: ["UId"],
	    	    beforeSelectRowFunc: function (rowid) {
	    	        //main key 修改時不允與修改
	    	       
	    	    },
	    	    onAddRowFunc: function (rowid) {
	    	    	//$MainGrid.jqGrid('setCell', rowid, "Cmp", $("#Cmp").val(), 'edit-cell dirty-cell');
	    	    	//$MainGrid.jqGrid('setCell', rowid, "GroupId", groupId, 'edit-cell dirty-cell');
	    	    	//$MainGrid.jqGrid('setCell', rowid, "PartyNo", $("#PartyNo").val(), 'edit-cell dirty-cell');

	    	    	setGridVal($MainGrid, rowid, "Cmp", $("#Cmp").val(), null);
	    	    	setGridVal($MainGrid, rowid, "GroupId", groupId, null);
	    	    	setGridVal($MainGrid, rowid, "PartyNo", $("#PartyNo").val(), null);
	    	    },
	    	    beforeAddRowFunc: function (rowid) {
	    	        //add row 時要可以編輯main key
	    	       
	    	    },
	    	    afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
	    	        if (name == "SpPlantD")
	    	        {
	    	            var SpPlant = "";
	    	            var ids = $MainGrid.jqGrid('getDataIDs');
	    	            for (var i = 0 ; i < ids.length; i++) {
	    	                if (rowid == ids[i]) continue;
	    	                var _SpPlant = getGridVal($MainGrid, ids[i], "SpPlantD", null);

	    	                if (val!=""&&val == _SpPlant)
	    	                {
	    	                    //alert(getGridVal($MainGrid, rowid, "SpPlantD", null));
	    	                    setTimeout(function () {
	    	                    setGridVal($MainGrid, rowid, "SpPlantD", "", "lookup"); //$MainGrid.jqGrid("setCell", rowid, "SpPlantD", "", "lookup");
	    	                    }, 500);
	    	                    //alert(getGridVal($MainGrid, rowid, "SpPlantD", null));
	    	                    msg = "@Resources.Locale.L_BSTRUCKSetup_TruckInfo" + "：" + "@Resources.Locale.L_BSTRUCKSetup_OnlyLocal\n";
	    	                    alert(msg);
	    	                }
	    	            }
	    	        }
	    	    }
	    	}
	    );

	    new genGrid(
	    	$SubGrid,
	    	{
	    	    data: [],
	    	    loadonce: true,
	    	    colModel: colModel2,
	    	    datatype: "local",
				cellEdit: false,//禁用grid编辑功能
				isModel: true,
	    	    caption: "@Resources.Locale.L_BSTRUCKSetup_TruckInfo",
	    	    height: gridHeight,
	    	    //rownumWidth: 50,
	    	    refresh: true,
	    	    rows: 9999,
	    	    exportexcel: false,
	    	    pginput: false,
	    	    pgbuttons: false,
	    	    sortorder: "asc",
	    	    sortname: "DriverName",
	    	    delKey: ["UId"],
	    	    beforeSelectRowFunc: function (rowid) {
	    	        //main key 修改時不允與修改
	    	       
	    	    },
	    	    onAddRowFunc: function (rowid) {

	    	        setGridVal($SubGrid, rowid, "Cmp", $("#Cmp").val(), null);
	    	        setGridVal($SubGrid, rowid, "GroupId", groupId, null);
	    	        setGridVal($SubGrid, rowid, "PartyNo", $("#PartyNo").val(), null);
	    	    },
	    	    beforeAddRowFunc: function (rowid) {
	    	        //add row 時要可以編輯main key
	    	       
	    	    },
	    	    afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
	    	        if (name == "SpPlant") {
	    	            var SpPlant = "";
	    	            var ids = $SubGrid.jqGrid('getDataIDs');
	    	            for (var i = 0 ; i < ids.length; i++) {
	    	                if (rowid == ids[i]) continue;
	    	                var _SpPlant = getGridVal($SubGrid, ids[i], "SpPlant", null);

	    	                if (val != "" && val == _SpPlant) {
	    	                    //alert(getGridVal($MainGrid, rowid, "SpPlantD", null));
	    	                    setTimeout(function () {
	    	                        setGridVal($SubGrid, rowid, "SpPlant", "", "lookup"); //$MainGrid.jqGrid("setCell", rowid, "SpPlantD", "", "lookup");
	    	                    }, 500);
	    	                    //alert(getGridVal($MainGrid, rowid, "SpPlantD", null));
	    	                    msg = "@Resources.Locale.L_BSTRUCKSetup_DriverInfo" + "：" + "@Resources.Locale.L_BSTRUCKSetup_OnlyLocal\n";
	    	                    alert(msg);
	    	                }
	    	            }
	    	        }
	    	    }
	    	}
	    );
}

function setSmptyData(lookUp, Cd, Nm, pType)
{
	//SMPTY放大鏡
	options = {};
	options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
	options.registerBtn = $("#"+lookUp);
	options.focusItem = $("#" + Cd);
	options.param = "";
	options.baseCondition = " PARTY_TYPE LIKE '%"+pType+"%'";
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    $("#" + Cd).val(map.PartyNo);

	    if(Nm != "")
	    	$("#" + Nm).val(map.PartyName);

	    initLoadData(map.UId);
	}

	options.lookUpConfig = LookUpConfig.SmptyLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~"+pType+"&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
	    $(this).val(ui.item.returnValue.PARTY_NO);

	    if(Nm != "")
	    	$("#" + Nm).val(ui.item.returnValue.PARTY_NAME);
	    return false;
	});
}

function initLoadData(Uid)
{
    if (!Uid)
    {
        return;
    }
    $("#UFid").val(Uid);
    $.ajax({
        async: true,
        url: rootPath + "BSTRUCK/TruckQuery",
        type: 'POST',
        data: {"Cmp": $("#Cmp").val(), "PartyNo": $("#PartyNo").val(), UFid: Uid},
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
        	$MainGrid.jqGrid("clearGridData");
            $MainGrid.jqGrid("setGridParam", {
			    datatype: 'local',
			    data: result.mainGridData
			}).trigger("reloadGrid");

            $SubGrid.jqGrid("clearGridData");
			$SubGrid.jqGrid("setGridParam", {
			    datatype: 'local',
			    data: result.subGridData
			}).trigger("reloadGrid");
            
            CommonFunc.ToogleLoading(false);
        }
    });
}

function getcust(name,id) {
    var _name = name;
    var cust_op = getLookupOp(id,
        {
            url: rootPath + LookUpConfig.GetCmpUrl,
            config: LookUpConfig.CmpLookup,
            returnFn: function (map, $grid) {
                return map.Cmp;
            }
        }, {
            baseConditionFunc: function () {
                var ids = $("#" + id).jqGrid('getDataIDs');
                var SpPlant="";
                    for (var i = 0 ; i < ids.length; i++) {
                        var _SpPlant = getGridVal($("#" + id), ids[i], name, null);
                        if (_SpPlant != "" && _SpPlant.length<4)
                            SpPlant += _SpPlant + ";";
                    }
                    if (SpPlant != "")
                        SpPlant = "&sopt_Cmp=ni&Cmp=" + SpPlant.substring(0,SpPlant.length-1);
                return "";
            }
        }, LookUpConfig.GetCmpAuto(groupId, $("#" + id),
        function ($grid, rd, elem, rowid) {
            setGridVal($grid, rowid, _name, rd.CMP, "lookup");
            $(elem).val(rd.CMP);
        }));
    cust_op.param = '';
    return cust_op;
}