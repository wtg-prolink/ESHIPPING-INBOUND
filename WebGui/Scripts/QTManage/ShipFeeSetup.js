var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var _status = "";
$(function(){

	_handler.saveUrl = rootPath + "QtManage/ShipFeeUpdateData";
	_handler.inquiryUrl = rootPath + "QtManage/GetDetail";
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
	}


	MenuBarFuncArr.EndFunc = function(){
	    inputAction();

	    if(_status == "add")
	    {
	    	$("#ShipmentId").val(SpId);
	    }
	}

	_handler.saveData = function (dtd) {
	    var changeData = getChangeValue();//获取所有改变的值
	    var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
	    data["u_id"] = encodeURIComponent($("#UId").val());

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
	    _status = "edit";
	}

	_handler.beforAdd = function () {//新增前设定
		_status = "add";
	}

	_handler.setFormData = function (data) {
	    if (data["main"])
	        _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
	    else
	        _handler.topData = [{}];

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
	    ajaxHttp(rootPath + "QtManage/GetShipFeeDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
	        function (data) {
	            if (_handler.setFormData)
	                _handler.setFormData(data);
        });
	}

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();	   
	    $("#EffectDate").val("");
	    $("#Area").val("");
	    var dataRow, addData = [];
	}

	registBtnLookup($("#LspNoLookup"), {
	    item: "#LspNo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
	        $("#LspNo").val(map.PartyNo);
	        $("#LspNm").val(map.PartyName);
	    }
	}, {},LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
	    $("#LspNo").val(rd.PARTY_NO);
	    $("#LspNm").val(rd.PARTY_NAME);
	}));

	registBtnLookup($("#ChgCdLookup"), {
	    item: "#ChgCd", url: rootPath + LookUpConfig.ChgUrl, config: LookUpConfig.ChgLookup, param: "", selectRowFn: function (map) {
	        $("#ChgCd").val(map.ChgCd);
	        $("#ChgDescp").val(map.ChgDescp);
	    }
	}, {},LookUpConfig.GetChgAuto1(groupId, undefined, function ($grid, rd, elem) {
	    $("#ChgCd").val(rd.CHG_CD);
	    $("#ChgDescp").val(rd.CHG_DESCP);
	}));

	registBtnLookup($("#QcurLookup"), {
	    item: '#Qcur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
	        $("#Qcur").val(map.Cur);
	    }
	}, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
	    $("#Qcur").val(rd.CUR);
	}));

	registBtnLookup($("#QchgUnitLookup"), {
	    item: '#QchgUnit', url: rootPath + LookUpConfig.NwuUrl, config: LookUpConfig.NwuLookup, param: "", selectRowFn: function (map) {
	        $("#QchgUnit").val(map.Cd);
	    }
	}, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, function ($grid, rd, elem) {
	    $("#QchgUnit").val(rd.CD);
	}, function ($grid, elem) {
	    $("#QchgUnit").val("");
	}));

	registBtnLookup($("#CostCenterLookup"), {
	    item: '#CostCenter', url: rootPath + LookUpConfig.CostCenterUrl, config: LookUpConfig.CostCenterLookup, param: "", selectRowFn: function (map) {
	         $("#CostCenter").val(map.CostCenter);
	         $("#CostTonm").val(map.ShortDescp);
	    }
	}, undefined, LookUpConfig.GetCostCenterAuto(groupId, undefined, function ($grid, rd, elem) {
	    $("#CostCenter").val(rd.COST_CENTER);
	    $("#CostTonm").val(rd.SHORT_DESCP);
	}));

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
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

function inputAction()
{
	$("#Qty").prop("disabled", true);
	$("#Cur").prop("disabled", true);
	$("#CurLookup").prop("disabled", true);
	$("#UnitPrice").prop("disabled", true);
	$("#ChgUnit").prop("disabled", true);
	$("#ChgUnitLookup").prop("disabled", true);
	$("#Bamt").prop("disabled", true);
	$("#ExRate").prop("disabled", true);
	$("#Lamt").prop("disabled", true);
	$("#Tax").prop("disabled", true);
}