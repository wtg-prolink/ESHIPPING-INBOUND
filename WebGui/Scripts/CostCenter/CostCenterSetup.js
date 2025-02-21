var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){

	_handler.saveUrl = rootPath + "CostCenter/UpdateData";
	_handler.inquiryUrl = rootPath + "CostCenter/GetDetail";
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

	}

	_handler.saveData = function (dtd) {
	    var changeData = getChangeValue();//获取所有改变的值
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
	    ajaxHttp(rootPath + "CostCenter/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
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

	registBtnLookup($("#CmpLookup"), {
	    item: '#Cmp', 
	    url: rootPath + "TPVCommon/GetSiteCmpData", 
	    config: LookUpConfig.SiteLookup, 
	    param: "", 
	    selectRowFn: function (map) {
	        $("#Cmp").val(map.Cd);
	    }
	}, {focusItem:$("#Cmp")}, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        _cmp = rd.CMP;
        $("#Cmp").val(_cmp);
    }));

	registBtnLookup($("#DepLookup"), {
	    item: '#Dep', 
	    url: rootPath + "TPVCommon/GetDepDataForLookup", 
	    config: LookUpConfig.BSCodeLookup,
	    param: "", 
	    selectRowFn: function (map) {
	        $("#Dep").val(map.Cd);
	    }
	}, {focusItem:$("#Dep")}, LookUpConfig.GetCodeTypeAuto(groupId, "DE", undefined, function ($grid, rd, elem) {
	    $("#Dep").val(rd.CD);
	}));

	registBtnLookup($("#PrincipalLookup"), {
	    item: '#Principal', 
	    url: rootPath + "TPVCommon/GetUserData", 
	    config: LookUpConfig.UserLookup, 
	    param: "", 
	    selectRowFn: function (map) {
	        $("#Principal").val(map.UName);
	    }
	}, {focusItem:$("#Principal")}, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Principal").val(U_NAME);
    }));

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
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