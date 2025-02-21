var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var IoFlag = getCookie("plv3.passport.ioflag");
$(function(){
	var $MainGrid = $("#MainGrid");

	_handler.saveUrl = rootPath + "BSCAA/UpdateData";
	_handler.inquiryUrl = "";
	_handler.config = [];

	_handler.beforDel = function () {
	    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
	        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
	        return false;
	    }
	}

	_handler.beforSave = function () {
	    var Carrier = $("#Carrier").val();
	    var Area = $("#Area").val();
	    if (Carrier == "" || Area == "") {
	        alert("@Resources.Locale.L_QTtManage_PleEnt");
	        return false;
	    }
	}
	_handler.saveData = function (dtd) {
	    var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    changeData["sub"] = containerArray;

	    var Carrier = $("#Carrier").val();
	    var Area = $("#Area").val();
	    var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false, Carrier: Carrier, Area: Area };
	    data["u_id"] = encodeURIComponent($("#UId").val());

	    console.log(changeData);
	    //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
	    ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
	        function (result) {
	            if (result.message) {
	                alert(result.message);
	                return false;
	            }
	            else if (_handler.setFormData)
	                _handler.setFormData(result);
	            return true;
	        });
	}

	_handler.beforEdit = function () {
	    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
	        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
	        return false;
	    }
	}

	_handler.setFormData = function (data) {
		if (data["main"])
	        _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
	    else
	        _handler.topData = [{}];
	    if (data["sub"])
	        _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
	    else
	        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

	    setFieldValue(data["main"] || [{}]);
	    setdisabled(true);
	    setToolBtnDisabled(true);
	    MenuBarFuncArr.Enabled(["MBCopy"]);
	}

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();
	    var dataRow, addData = [];
	    $MainGrid.jqGrid("clearGridData");
	    _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
	    gridEditableCtrl({ editable: true, gridId: "MainGrid" });
	}

	_handler.loadMainData = function (map) {
		console.log(map);
	    if (!map || !map[_handler.key]) {
	        setFieldValue([{}]);
	        return;
	    }
	    ajaxHttp(rootPath + "BSCAA/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
	        function (data) {
	            if (_handler.setFormData)
	                _handler.setFormData(data);
        });
	}

	function getop(name) {
	    var _name = name;
	    var city_op = getLookupOp("MainGrid",
	        {
	            url: rootPath + LookUpConfig.CityPortUrl,
	            config: LookUpConfig.CityPortLookup,
	            returnFn: function (map, $grid) {
	                var selRowId = $grid.jqGrid('getGridParam', 'selrow');
	                //setGridVal($grid, selRowId, 'Port', map.CntryCd + map.PortCd, "lookup");
	                setGridVal($grid, selRowId, 'PortDescp', map.PortNm, null);
	                return map.CntryCd + map.PortCd;

	            }
	        }, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
	        function ($grid, rd, elem, selRowId) {
	            setGridVal($grid, selRowId, 'Port', rd.CNTRY_CD + rd.PORT_CD, "lookup");
	            setGridVal($grid, selRowId, 'PortDescp', rd.PORT_NM, null);
	        }), { param: "" });
	    return city_op;
	}

	var colModel = [
	    { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
	    { name: 'UFid', title: 'U FID', index: 'UFid', sorttype: 'string', width: 100, editable: false, hidden: true },
	    { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', width: 100, editable: false, hidden: true },
	    { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', width: 100, editable: false, hidden: true },
	    { name: 'Area', title: 'Area', index: 'Area', sorttype: 'string', width: 100, editable: false, hidden: true },
	    { name: 'Port', title: 'Port', index: 'Port', sorttype: 'string', editoptions: gridLookup(getop("Port")), edittype: 'custom', width: 150, hidden: false, editable: true },
	    { name: 'PortDescp', title: 'Port Description', index: 'PortDescp', sorttype: 'string', width: 200, hidden: false, editable: false }
	];

	_handler.intiGrid("MainGrid", $MainGrid, {
	    colModel: colModel, caption: 'Port List', delKey: ["UId"],
	    beforeAddRowFunc: function(){
	    	if($("#Carrier").val() == "" || $("#Area").val() == "")
	    	{
	    		alert("@Resources.Locale.L_QTtManage_PleEnt");
	    		return false;
	    	}
	    },
	    onAddRowFunc: function (rowid) {

	    },
	    beforeSelectRowFunc: function (rowid) {
	    }
	});


	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();	   
	    $("#EffectDate").val("");
	    $("#Area").val("");
	    var dataRow, addData = [];
	    $MainGrid.jqGrid("clearGridData");
	    _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
	    gridEditableCtrl({ editable: true, gridId: "MainGrid" });
	}

	registBtnLookup($("#CarrierLookup"), {
	    item: '#Carrier', url: rootPath + LookUpConfig.TCARUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
	        $("#Carrier").val(map.Cd);
	        $("#CarrierNm").val(map.CdDescp);
	        var Area = $("#Area").val();
	        _handler.topData = {Carrier: map.Cd, Area: Area};
	        if(Area != "")
	        {
	        	MenuBarFuncArr.MBCancel();
	        }
	    }
	}, {focusItem:$("#Carrier")}, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined, function ($grid, rd, elem) {
		    $("#Carrier").val(rd.CD);
		    $("#CarrierNm").val(rd.CD_DESCP);
	},function($grid, elem, rowid){
    	$("#Carrier").val("");
	    $("#CarrierNm").val("");
	}));


	// registBtnLookup($("#AreaLookup"), {
	//     item: '#Area', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
	//         $("#Area").val(map.Cd);
	//         $("#AreaNm").val(map.CdDescp);
	//     }
	// }, {focusItem:$("#Area")}, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
	//     	$("#Area").val(rd.CD);
 //    	    $("#AreaNm").val(rd.CD_DESCP);
	// },function($grid, elem, rowid){
 //    	$("#Area").val("");
	//     $("#AreaNm").val("");
	// }));

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
	    _handler.topData = { UId: _uid };
	    MenuBarFuncArr.MBCancel();
	}

});

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