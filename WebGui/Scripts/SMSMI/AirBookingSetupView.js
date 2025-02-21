var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid = null;
var $SubGrid2 = null;
var $SubGrid3 = null;
var $DnGrid = null;
var $CcGrid = null;
var $TcGrid = null;
var $SubGrid4 = null;
var $SubGrid5 = null;
$(function () {
	$SubGrid = $("#SubGrid");
	$SubGrid2 = $("#SubGrid2");
	$SubGrid3 = $("#SubGrid3");
	$DnGrid = $("#DnGrid");
	$CcGrid = $("#CcGrid");
	$TcGrid = $("#TcGrid");
	$SubGrid4 = $("#SubGrid4");
	$SubGrid5 = $("#SubGrid5");
	genPartyGrid();
	var containerArray = "";
	var Ptlist = { pt: PTlist };
    _handler.saveUrl = rootPath + "SMSMI/BookingUpdateData";
	_handler.inquiryUrl = rootPath + "";
	_handler.config = [];

	_handler.beforDel = function () {
		if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
			CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
			return false;
		}
		var status = $("#Status").val();
		if ("A" != status && "S" != status) {
		    CommonFunc.Notify("", "Delete only when the status in Unreach,ISF Sending!", 500, "warning");
		    return false;
		}
	}

	_handler.addData = function () {
		//初始化新增数据
		var data = {};
		data[_handler.key] = uuid();
		setFieldValue([data]);

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		_handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
		_handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
		_handler.loadGridData("SubGrid4", $SubGrid4[0], [], [""]);
		_handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);
		_handler.loadGridData("CcGrid", $CcGrid[0], [], [""]);
		_handler.loadGridData("TcGrid", $TcGrid[0], [], [""]);
		_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
		getAutoNo("ShipmentId", "rulecode=SHIB_NO&cmp=" + cmp);

		$SubGrid2.jqGrid('getGridParam', "removeAddRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeAddRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeAddRowButton")("TcGrid");

		$SubGrid2.jqGrid('getGridParam', "removeDelRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeDelRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeDelRowButton")("TcGrid");

		$SubGrid2.jqGrid('getGridParam', "removeCopyRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeCopyRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeCopyRowButton")("TcGrid");

		//gridEditableCtrl({ editable: false, gridId: "SubGrid4" });
	}

	_handler.afterEdit = function () {
	    SetBookingEdit();
	}

	_handler.editData = function () {
		$SubGrid2.jqGrid('getGridParam', "removeAddRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeAddRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeAddRowButton")("TcGrid");

		$SubGrid2.jqGrid('getGridParam', "removeDelRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeDelRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeDelRowButton")("TcGrid");

		$SubGrid2.jqGrid('getGridParam', "removeCopyRowButton")("SubGrid2");
		$CcGrid.jqGrid('getGridParam', "removeCopyRowButton")("CcGrid");
		$TcGrid.jqGrid('getGridParam', "removeCopyRowButton")("TcGrid");
		MenuBarFuncArr.Disabled(["MBSendAddr"]);
		//gridEditableCtrl({ editable: false, gridId: "SubGrid4" });
	}


	MenuBarFuncArr.EndFunc = function () {

	}

	_handler.saveData = function (dtd) {
		var SubGrid2Data = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
		var SubGrid4Data = $SubGrid4.jqGrid('getGridParam', "arrangeGrid")();
		var CcData = $CcGrid.jqGrid('getGridParam', "arrangeGrid")();
		var TcData = $TcGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = getChangeValue();//获取所有改变的值
		if (isEmpty(containerArray))
		    containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
		changeData["sub"] = containerArray;
		changeData["sub2"] = SubGrid2Data;
		changeData["sub4"] = SubGrid4Data;
		changeData["cc"] = CcData;
		changeData["tc"] = TcData;
		if (changeData["mt"]) {
			changeData["mt"][0]["TranType"] = "A";
		}

		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		data["ShipmentId"] = encodeURIComponent($("#ShipmentId").val());


		//data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
		CommonFunc.ToogleLoading(true);
		ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
					function (result) {
						CommonFunc.ToogleLoading(false);
						if (result.message) {
							alert(result.message);
							return false;
						}
						else if (_handler.setFormData)
							_handler.setFormData(result);

						
						return true;
					});
		containerArray = "";
	}

	_handler.beforEdit = function () {
		if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
			CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
			return false;
		}
	}

	_handler.beforAdd = function () {//新增前设定
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

		if (data["sub"])
			_handler.loadGridData("SubGrid", $SubGrid[0], data["sub"], [""]);
		else
			_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

		if (data["sub2"]){
			_handler.loadGridData("SubGrid2", $SubGrid2[0], data["sub2"], [""]);
			SetAddrModal();
		}
		else
			_handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);

		if (data["sub3"])
			_handler.loadGridData("SubGrid3", $SubGrid3[0], data["sub3"], [""]);
		else
			_handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);

		if (data["DnGrid"])
			_handler.loadGridData("DnGrid", $DnGrid[0], data["DnGrid"], [""]);
		else
			_handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);

		if (data["CcGrid"])
			_handler.loadGridData("CcGrid", $CcGrid[0], data["CcGrid"], [""]);
		else
			_handler.loadGridData("CcGrid", $CcGrid[0], [], [""]);

		if (data["TcGrid"])
			_handler.loadGridData("TcGrid", $TcGrid[0], data["TcGrid"], [""]);
		else
		    _handler.loadGridData("TcGrid", $TcGrid[0], [], [""]);

		if (data["sub4"])
		    _handler.loadGridData("SubGrid4", $SubGrid4[0], data["sub4"], [""]);
		else
			_handler.loadGridData("SubGrid4", $SubGrid4[0], [], [""]);

		if (data["sub5"])
			_handler.loadGridData("SubGrid5", $SubGrid5[0], data["sub5"], [""]);
		else
			_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);

		setFieldValue(data["main"] || [{}]);
		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);
		MenuBarFuncArr.Enabled(["MBCopy", "MBEdoc", "MBAllocation", "btn02", "MBSendAddr"]);
		if (data["main"].length > 0) {
			var status = data["main"][0]["Status"];
            if (status === "V" || status == "O" || status == "Z") {
				MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
			}

			var confirmStatus = "CDEFGPOXHIJS";
			if (confirmStatus.indexOf(status) >= 0)
			{
			    MenuBarFuncArr.Disabled(["btn02"]);
			}
			
			var olocation = data["main"][0]["OLocation"];
			if (olocation != "" && olocation != null) {
				$("#Horn").attr('readonly', true);
				$("#Battery").attr('readonly', true);
			};
			var ExtraSrv = data['main'][0]["ExtraSrv"];
			var b = [];
			if (ExtraSrv != "" && ExtraSrv != null) {
				b = ExtraSrv.split(";");
			}
			$("[dt='mt'][chxName='ExtraSrv']").each(function (index, el) {
				for (var j = 0; j < b.length; j++) {
					if ($(this).val() == b[j]) {
						$(this).prop("checked", true);
					}
				}
			});
		} else
		{
			$("[dt='mt'][chxName]").each(function (index, el) {
				$(this).prop("checked", false);
			});
		}

		if ($("[dt='mt'][chxName='ExtraSrv'][value='FORK']").is(':checked') == false) {
			$("#Fork").hide();
		} else {
			$("#Fork").show();
		}

		var multiEdocData = [];
		ajaxHttp(rootPath + "SMSMI/GetDNData", { ShipmentId: $("#ShipmentId").val(), loading: true, OUid: $("#OUid").val() },
        function (data) {
            if (data != null) {
                $(data.dn).each(function (index) {
                    multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*' });
                });
            }
            if ($("#OUid").val() != "") {
                multiEdocData.push({ jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': _handler.topData["Cmp"], 'STN': '*' });
                MenuBarFuncArr.initEdoc($("#OUid").val(), _handler.topData["GroupId"], $("#OLocation").val(), "*", multiEdocData, callBackFunc);
            } else {
                MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData, callBackFunc);
            }
        });
	}

	$("[dt='mt'][chxName='ExtraSrv'][value='FORK']").change(function () {
		if ($(this).is(':checked')) {
			$("#Fork").show();
		}
		else {
			$("#Fork").hide();
		}
	});

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

	MenuBarFuncArr.MBCopy = function (thisItem) {
		//初始化新增数据
		var data = {};
		data[_handler.key] = uuid();
		var dataRow, addData = [];

		getAutoNo("ShipmentId", "rulecode=SHIB_NO&cmp=" + cmp);
	}



	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		MenuBarFuncArr.MBCancel();
	}

	$("#DriverLookup").v3Lookup({
		url: rootPath + "EcallCommon/getUserIdByLookup",
		gridFunc: function (map) {
			var UId = map.UId,
				UName = map.UName;
			$("#Driver").val(UId);
			$("#DriverNm").val(UName);
		},
		lookUpConfig: LookUpConfig.UserLookup,
		baseConditionFunc: function () { return ""; },
		responseMethod: function () { return ""; }
	});

	$("#Driver").v3AutoComplete({
		params: "dt=user&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN=" + stn + "&U_ID%",
		returnValue: "U_ID&U_NAME=showValue,U_ID,U_NAME",
		callBack: function (event, ui) {
			console.log(ui);
			$(this).val(ui.item.returnValue.U_ID);
			$("#DriverNm").val(ui.item.returnValue.U_NAME);

			return false;
		},
		dymcFunc: function () {
			return "";
		},
		clearFunc: function () {
			$("#Driver").val("");
			$("#DriverNm").val("");
		}
	});
	var multiEdocData = [];

	/*
	ajaxHttp(rootPath + "DNManage/GetDNData", { DnNo: $("#CombineInfo").val(), loading: true },
	function (data) {
		if (data == null) {
			MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*",null,callBackFunc);
		} else {
			$(data.dn).each(function (index) {
				multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*' });
			});
			MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData,callBackFunc);
		}
	});
	*/
	MenuBarFuncArr.AddMenu("MBAllocation", "glyphicon glyphicon-font", "Allocation", function () {
		var uid = $("#UId").val();
		var status = $("#Status").val();
		if ("A" != status) {
			CommonFunc.Notify("", _getLang("L_DNManage_HasBeenBooking", "此笔资料已经订舱，不允许执行Allocation！"), 500, "warning");
			return;
		}

		if (!uid) {
			CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "请先选择一笔记录"), 500, "warning");
			return;
		}
		$.ajax({
			async: true,
			url: rootPath + "SMSMI/LspDistribution",
			type: 'POST',
			data: { suid: uid },
			"error": function (xmlHttpRequest, errMsg) {
				alert(errMsg);
			},
			success: function (data) {
				if (data.msg != "success") {
					CommonFunc.Notify("", data.msg, 500, "warning");
				}
				else {
					CommonFunc.Notify("", data.msg, 500, "success");
				}
				MenuBarFuncArr.MBCancel();
			}
		});
	});

	//MenuBarFuncArr.AddMenu("MBSEND", " glyphicon glyphicon-usd", "发送", function () {
	//	$.ajax({
	//		async: true,
	//		url: rootPath + "SMSMI/DECLBookAction",
	//		type: 'POST',
	//		data: {
	//			"Uid": _uid
	//		},
	//		"complete": function (xmlHttpRequest, successMsg) {

	//		},
	//		"error": function (xmlHttpRequest, errMsg) {
	//			var resJson = $.parseJSON(errMsg)
	//			CommonFunc.Notify("", resJson.message, 500, "warning");
	//		},
	//		success: function (result) {
	//			if (result.IsOk == "Y") {
	//				CommonFunc.Notify("", result.message, 500, "success");
	//			} else {
	//				CommonFunc.Notify("", result.message, 500, "warning");
	//			}
	//			$("#SummarySearch").trigger("click");
	//		}
	//	});
	//});

	MenuBarFuncArr.AddMenu("MBSendAddr", "glyphicon glyphicon-th-large", commonLang["L_SMSMI_MBSendAddr"], function () {
	    gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
		$("#AddrModal").modal("show");
		gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
	});

	$("#AddrClose").on("click", function () {
		gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
	});
	$("#AddrClose2").on("click", function () {
		gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
	});

	MenuBarFuncArr.AddMenu("btn02", "glyphicon glyphicon-bell", commonLang["L_SMSMI_btn02"], function () {
		var uid = $("#UId").val();
		var status = $("#Status").val();
		CommonFunc.ToogleLoading(true);
		if (status == "I" || status == "C" || status == "D" || status == "H") {
		    CommonFunc.Notify("", "You cann't operate this Action!", 500, "warning");
		    return false;
		}
		$.ajax({
			async: true,
			url: rootPath + "SMSMI/notifytoLsp",
			type: 'POST',
			data: {
				"Uid": uid,
			},
			"complete": function (xmlHttpRequest, successMsg) {
				CommonFunc.ToogleLoading(false);
			},
			"error": function (xmlHttpRequest, errMsg) {
				CommonFunc.ToogleLoading(false);
				var resJson = $.parseJSON(errMsg)
				CommonFunc.Notify("", resJson.message, 500, "warning");
			},
			success: function (result) {
				//var resJson = $.parseJSON(result)
				if (result.IsOk == "Y") {
					CommonFunc.Notify("", result.message, 500, "success");
				}
				else {
					alert(result.message);
				}
				$("#SummarySearch").trigger("click");
			}
		});
	});

	setSelectData(_selects);
});

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

function setSelectData(data) {
	var _shownull = { cd: '', cdDescp: '' };
	var delayOptions = data.DELY || [];
	if (delayOptions.length > 0)
		_mt = delayOptions[0]["cd"];
	delayOptions.unshift(_shownull);
	appendSelectOption($("#DelayReason"), delayOptions);

	var delayOptions = data.DELAY_SOLUTION || [];
	if (delayOptions.length > 0)
		_mt = delayOptions[0]["cd"];
	delayOptions.unshift(_shownull);
    appendSelectOption($("#DelaySolution"), delayOptions);
}