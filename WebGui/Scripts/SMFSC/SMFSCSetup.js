var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var IoFlag = getCookie("plv3.passport.ioflag");
$(function(){
	var $MainGrid = $("#MainGrid");

	_handler.saveUrl = rootPath + "SMFSC/SmfscUpdateData";
	_handler.inquiryUrl = "";
	_handler.config = [];

	_handler.beforDel = function () {
	    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
	        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
	        return false;
	    }
	}

	_handler.addData = function () {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();
	    setFieldValue([data]);
	    _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
	}


	MenuBarFuncArr.EndFunc = function(){
		SetBtnView();
	}

	_handler.saveData = function (dtd) {
	    //var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    //changeData["sub"] = containerArray;
	    var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
	    data["u_id"] = encodeURIComponent($("#UId").val());
	    data["Carrier"] = encodeURIComponent($("#Carrier").val());
	    data["EffectDate"] = encodeURIComponent($("#EffectDate").val());
	    data["Area"] = encodeURIComponent($("#Area").val());
	    data["Cmp"] = encodeURIComponent($("#Cmp").val());

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

	_handler.beforAdd = function () {//新增前设定
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
	    _handler.beforLoadView();
	    setdisabled(true);
	    setToolBtnDisabled(true);
	    MenuBarFuncArr.Enabled(["MBCopy"]);
	}

	_handler.loadMainData = function (map) {
	    if (!map || !map[_handler.key]) {
	        setFieldValue([{}]);
	        return;
	    }
	    ajaxHttp(rootPath + "SMFSC/GetSmfscDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
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
	    { name: 'Port', title: 'Port', index: 'Port', sorttype: 'string', width: 150, hidden: false, editable: false },
	    { name: 'PortDescp', title: 'Port Description', index: 'PortDescp', sorttype: 'string', width: 200, hidden: false, editable: false }
	];

	_handler.intiGrid("MainGrid", $MainGrid, {
	    colModel: colModel, caption: 'Port List', delKey: ["UId"], pgbuttons: false, exportexcel: false, toppager:false,
	    onAddRowFunc: function (rowid) {
	    },
	    beforeSelectRowFunc: function (rowid) {
	    },
	    beforeAddRowFunc: function (rowid) {
	    }
	});

	_handler.beforLoadView = function () {
		if(IoFlag == "O")
		{
			$("#Cmp").prop("readonly", true);
		}
	}

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

	var GetSiteCmpAuto = function (groupId, $grid, autoFn, clearFn) {
	    var op =
        {
            autoCompDt: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=",
            autoCompParams: "CMP=showValue,CMP,NAME",
            autoCompFunc: function (elem, event, ui, rowid) {
                autoFn($grid, ui.item.returnValue, elem, rowid);
            },
            autoClearFunc: function (elem, event, rowid) {
                clearFn($grid, elem, rowid);
            }
        };
	    return op;
	}
    //setSmptyData("CmpLookup", "Cmp", "CmpNm", "");

	registBtnLookup($("#CmpLookup"), {
	    item: "#Cmp", url: rootPath + "TPVCommon/GetSiteCmpData", config: LookUpConfig.SiteLookup, param: "", selectRowFn: function (map) {
	        $("#Cmp").val(map.Cd);
	        $("#CmpNm").val(map.CdDescp);
	    }
	}, { baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'" }, GetSiteCmpAuto(groupId, undefined, function ($grid, rd, elem) {
	    $("#Cmp").val(rd.CMP);
	    $("#CmpNm").val(rd.NAME);
	}, function ($grid, elem, rowid) {
	    $("#Cmp").val("");
	    $("#CmpNm").val("");
	}));
	
	registBtnLookup($("#CarrierLookup"), {
	    item: '#Carrier', url: rootPath + LookUpConfig.TCARUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
	        $("#Carrier").val(map.Cd);
	        $("#CarrierNm").val(map.CdDescp);
	        var Area = $("#Area").val();
	        getPortData(map.Cd, Area);
	    }
	}, {focusItem:$("#Carrier")}, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined, function ($grid, rd, elem) {
	    $("#Carrier").val(rd.CD);
	    $("#CarrierNm").val(rd.CD_DESCP);

	    var Area = $("#Area").val();
        getPortData(rd.CD, Area);
	}));

	registBtnLookup($("#AreaLookup"), {
	    item: '#Area', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
	        $("#Area").val(map.Cd);
	        $("#AreaNm").val(map.CdDescp);
	        var Carrier = $("#Carrier").val();
        	getPortData(Carrier, map.Cd);
	    }
	}, {focusItem:$("#Area")}, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
	    	$("#Area").val(rd.CD);
		    $("#AreaNm").val(rd.CD_DESCP);
		    var Carrier = $("#Carrier").val();
        	getPortData(Carrier, rd.CD);
	},function($grid, elem, rowid){
		$("#Area").val("");
	    $("#AreaNm").val("");
	}));

	//registBtnLookup($("#FareaLookup"), {
	//    item: '#Farea', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
	//        $("#Farea").val(map.Cd);
	//        var Carrier = $("#Carrier").val();
	//    }
	//}, {focusItem:$("#Farea")}, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
	//    	$("#Farea").val(rd.CD);
	//	    var Carrier = $("#Carrier").val();
	//},function($grid, elem, rowid){
	//	$("#FArea").val("");
	//}));

	registBtnLookup($("#FareaLookup"), {
	    item: "#Farea", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
	        $("#Farea").val(map.CntryCd + map.PortCd);
	        var Carrier = $("#Carrier").val();
	    }
	},  undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
	    $("#FArea").val(rd.CNTRY_CD + rd.PORT_CD);
	    var Carrier = $("#Carrier").val();
	}));

	registBtnLookup($("#TareaLookup"), {
	    item: "#Tarea", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
	        $("#Tarea").val(map.CntryCd + map.PortCd);
	        var Carrier = $("#Carrier").val();
	    }
	},  undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
	    $("#TArea").val(rd.CNTRY_CD + rd.PORT_CD);
	    var Carrier = $("#Carrier").val();
	}));

	//幣別放大鏡
	var options = {};
	options.gridUrl = rootPath + "TPVCommon/GetBsCurDataForLookup";
	options.registerBtn = $("#CurLookup");
	options.focusItem = $("#Cur");
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    $("#Cur").val(map.Cur);
	}

	options.lookUpConfig = LookUpConfig.CurLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#Cur", 1, "", "dt=crn&GROUP_ID=" + groupId + "&CUR=", "CUR=showValue,CUR", function (event, ui) {
	    $(this).val(ui.item.returnValue.CUR);
	    return false;
	});

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    function getPortData(Carrier, Area)
    {
    	$.post(rootPath + 'BSCAA/GetDetail', {Carrier: Carrier, Area: Area}, function(data, textStatus, xhr) {
    		$MainGrid.jqGrid("clearGridData");
    		$MainGrid.jqGrid("setGridParam", {
                datatype: 'local',
                data: data["sub"],
            }).trigger("reloadGrid");
    	}, "JSON");
    }

    $("#IoType").on("change", function(){
    	var val = $(this).val();

    	if(val == "I")
    	{
    		$(".pod").show();
    		$(".pol").hide();
    	}
    	else
    	{
    		$(".pod").hide();
    		$(".pol").show();
    	}
    });
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

function SetBtnView() {
    if(IoFlag == "O")
    {
    	$("#Cmp").val(cmp);
    	$("#CmpNm").val($('<div/>').html(CmpNm).text());
    	$("#Cmp").prop("disabled", true);
    	$("#CmpLookup").prop("disabled", true);
    	$("#Carrier").setfocus();
    }
}

