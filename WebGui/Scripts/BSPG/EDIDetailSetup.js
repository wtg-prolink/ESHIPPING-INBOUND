var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function(){
	_handler.saveUrl = rootPath + "BSPG/EdiDetailUpdateData";
	_handler.inquiryUrl = rootPath + "BSPG/EdiGetDetail";
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
	    //var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    //changeData["sub"] = containerArray;
	    var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
	    data["u_id"] = encodeURIComponent($("#UId").val());
	    data["u_fid"] = encodeURIComponent(_ufid);

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

	    // if (data["sub"])
	    //     _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
	    // else
	    //     _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

	    setFieldValue(data["main"] || [{}]);
	    _handler.beforLoadView();
	    setdisabled(true);
	    setToolBtnDisabled(true);
	    
	    MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
	    MenuBarFuncArr.Enabled(["MBEdoc"]);
	    MenuBarFuncArr.Enabled(["MBCopy"]);
	}

	_handler.loadMainData = function (map) {
	    if (!map || !map[_handler.key]) {
	        setFieldValue([{}]);
	        return;
	    }
	    ajaxHttp(rootPath + "BSPG/EdiDetailGetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
	        function (data) {
	            if (_handler.setFormData)
	                _handler.setFormData(data);
        });
	}

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    //初始化新增数据
	    var data = {};
	    data[_handler.key] = uuid();	   
	    var dataRow, addData = [];
	}

	_initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.Enabled(["MBEdoc"]);

    
});