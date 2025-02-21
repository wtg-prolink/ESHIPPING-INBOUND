var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid = null;
var $ShipmentGrid = null;
var $CntrGrid = null;
var $SubGrid5 = null;
var $SubGrid2 = null;
var $SubGrid3 = null;
var $DnGrid = null;
var $CcGrid = null;
var $TcGrid = null;
var $CntrGrid = null;
$(function () {
    $SubGrid = $("#SubGrid");
	$ShipmentGrid = $("#ShipmentGrid");
	$CntrGrid = $("#CntrGrid");
    $SubGrid5 = $("#SubGrid5");
	genTransloadGrid();
	var Ptlist = { pt: PTlist };
    _handler.saveUrl = rootPath + "SMSMI/BookingConfirmUpdateData";
    _handler.inquiryUrl = rootPath + "";
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

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		_handler.loadGridData("ShipmentGrid", $ShipmentGrid[0], [], [""]);
		_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
		_handler.loadGridData("CntrGrid", $CntrGrid[0], [], [""]);
		getAutoNo("CsmNo", "rulecode=IBCSM_NO&cmp=" + cmp);
		$("#ShipmentId").val($("#CsmNo").val());
	}

	MenuBarFuncArr.EndFunc = function () {

	}

	_handler.afterEdit = function () {
		setdisabled(true);
		//gridEditableCtrl({ editable: false, gridId: 'ShipmentGrid' });
		gridEditableCtrl({ editable: false, gridId: 'SubGrid5' });
	}

	_handler.saveData = function (dtd) {
		var containerArray = $CntrGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = getChangeValue();//获取所有改变的值
		changeData["smicntr"] = containerArray;
		if (changeData["mt"]) {
			changeData["mt"][0]["TranType"] = "F";
		}
		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		data["ShipmentId"] = encodeURIComponent($("#ShipmentId").val());

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

	_handler.beforSave = function () {
		containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
		if (!IsRepeat(containerArray, Ptlist)) {
			CommonFunc.Notify("", "Duplicate PartyType are not allowed", 500, "warning");
			return false;
		}
	}

	_handler.setFormData = function (data) {
		if (data["main"])
			_handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
		else
			_handler.topData = [{}];

		setFieldValue(data["main"] || [{}]);

		if (data["sub"])
			_handler.loadGridData("SubGrid", $SubGrid[0], data["sub"], [""]);
		else
			_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

		if (data["shipment"])
			_handler.loadGridData("ShipmentGrid", $ShipmentGrid[0], data["shipment"], [""]);
		else
			_handler.loadGridData("ShipmentGrid", $ShipmentGrid[0], [], [""]);

		if (data["smicntr"]) {
			_handler.loadGridData("CntrGrid", $CntrGrid[0], data["smicntr"], [""]);
			//SetAddrModal();
		}
		else
			_handler.loadGridData("CntrGrid", $CntrGrid[0], [], [""]);

		if (data["sub5"])
			_handler.loadGridData("SubGrid5", $SubGrid5[0], data["sub5"], [""]);
		else
			_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);

		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);
		MenuBarFuncArr.Enabled(["MBCopy", "MBEdoc", "MBAllocation", "btn02", "MBSendAddr"]);
	}

	_handler.loadMainData = function (map) {
		if (!map || !map[_handler.key]) {
			setFieldValue([{}]);
			return;
		}
		ajaxHttp(rootPath + "SMSMI/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
			function (data) {
				if (_handler.setFormData)
					_handler.setFormData(data);
			});
	}

	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		MenuBarFuncArr.MBCancel();
	}

});