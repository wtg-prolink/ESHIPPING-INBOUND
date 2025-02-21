var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid = null;
var $SubGrid2 = null;
var $SubGrid3 = null;
var $DnGrid = null;
var $CcGrid = null;
var $TcGrid = null;
var $CntrGrid = null;
var $SubGrid5 = null;
$(function(){
	$SubGrid = $("#SubGrid");
	$SubGrid2 = $("#SubGrid2");
	$SubGrid3 = $("#SubGrid3");
	$DnGrid = $("#DnGrid");
	$CcGrid = $("#CcGrid");
	$TcGrid = $("#TcGrid");
	$CntrGrid = $("#CntrGrid");
	$SubGrid5 = $("#SubGrid5");
	genPartyGrid();

	_handler.saveUrl = rootPath + "SMSMI/BookingConfirmUpdateData";
	_handler.inquiryUrl = rootPath + "";
	_handler.config = [];


	$("#Voyage1Lookup").click(function () {
	    var data = _handler.topData || {};
	    var vessel = data.Vessel1 || "";
	    showVessel(vessel);
	});
	$("#Voyage2Lookup").click(function () {
	    var data = _handler.topData || {};
	    var vessel = data.Vessel2 || "";
	    showVessel(vessel);
	});
	$("#Voyage3Lookup").click(function () {
	    var data = _handler.topData || {};
	    var vessel = data.Vessel3 || "";
	    showVessel(vessel);
	});
	$("#Voyage4Lookup").click(function () {
	    var data = _handler.topData || {};
	    var vessel = data.Vessel4 || "";
	    showVessel(vessel);
	});

	function showVessel(vessel) {
	    if (isEmpty(vessel)) {
	        CommonFunc.Notify("", _getLang("L_SeaTransport_NoVesRe", "无船舶信息"), 500, "warning");
	        return;
	    }
	    $('#VesselMapFrame').attr("src", "https://ipdev.lsphub.com/ShowVessel.aspx?vsl_nm=" + encodeURIComponent(vessel));
	    $('#VesselMap').modal('show');
	};


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
		_handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
		_handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
		_handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);
		_handler.loadGridData("CcGrid", $CcGrid[0], [], [""]);
		_handler.loadGridData("TcGrid", $TcGrid[0], [], [""]);
		_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
		getAutoNo("ShipmentId", "rulecode=SHIB_NO&cmp=" + cmp);
	}

	_handler.afterEdit = function () {
		setdisabled(true);
		$("#DelayReason").attr('disabled', true);
		$("#DelaySolution").attr('disabled', true);
	    var readonlys = ["LspAbnormalRmk"];
	    SetArrayDisable(readonlys, false);
	    gridEditableCtrl({ editable: false, gridId: 'SubGrid2' });
	    gridEditableCtrl({ editable: false, gridId: 'SubGrid3' });
	    gridEditableCtrl({ editable: false, gridId: 'DnGrid' });
	    gridEditableCtrl({ editable: false, gridId: 'CcGrid' });
		gridEditableCtrl({ editable: false, gridId: 'TcGrid' });
		$("#Voyage1").parent().find("button").attr("disabled", false);
		$("#Voyage2").parent().find("button").attr("disabled", false);
		$("#Voyage3").parent().find("button").attr("disabled", false);
		$("#Voyage4").parent().find("button").attr("disabled", false);
	}


	MenuBarFuncArr.EndFunc = function(){

	}

	_handler.saveData = function (dtd) {
	    var containerArray = $CntrGrid.jqGrid('getGridParam', "arrangeGrid")();
		var changeData = getChangeValue();//获取所有改变的值
		changeData["smicntr"] = containerArray;
		if(changeData["mt"])
		{
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

	_handler.beforAdd = function () {//新增前设定
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

		if (data["sub2"])
		    _handler.loadGridData("SubGrid2", $SubGrid2[0], data["sub2"], [""]);
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

		if (data["smicntr"])
		    _handler.loadGridData("CntrGrid", $CntrGrid[0], data["smicntr"], [""]);
		else
			_handler.loadGridData("CntrGrid", $CntrGrid[0], [], [""]);

		if (data["sub5"])
			_handler.loadGridData("SubGrid5", $SubGrid5[0], data["sub5"], [""]);
		else
			_handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);

		setFieldValue(data["main"] || [{}]);
		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);

		$("#Voyage1").parent().find("button").attr("disabled", false);
		$("#Voyage2").parent().find("button").attr("disabled", false);
		$("#Voyage3").parent().find("button").attr("disabled", false);
		$("#Voyage4").parent().find("button").attr("disabled", false);


		MenuBarFuncArr.Enabled(["MBCopy", "MBEdoc","MBConfirm"]);
		if(data["main"].length > 0)
		{
			var status = data["main"][0]["Status"];
            if (status === "V" || status == "O" || status == "Z") {
				MenuBarFuncArr.Disabled(["MBEdit", "MBDel"]);
			}

			var confirmStatus = "CDEFGPOXIJZ";
			if(confirmStatus.indexOf(status)>=0)
			{
			    MenuBarFuncArr.Disabled(["MBConfirm"]);
			}

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

        } else {
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
		
	}



	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		MenuBarFuncArr.MBCancel();
	}

	$("#DriverLookup").v3Lookup({
		url: rootPath + "EcallCommon/getUserIdByLookup",
		gridFunc: function(map){
			var UId = map.UId,
				UName = map.UName;
			$("#Driver").val(UId);
			$("#DriverNm").val(UName);
		},
		lookUpConfig: LookUpConfig.UserLookup,
		baseConditionFunc: function(){ return "";},
		responseMethod: function(){ return "";}
	});

	$("#Driver").v3AutoComplete({
		params: "dt=user&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN=" + stn + "&U_ID%",
		returnValue: "U_ID&U_NAME=showValue,U_ID,U_NAME",
		callBack: function(event, ui){
			console.log(ui);
			$(this).val(ui.item.returnValue.U_ID);
			$("#DriverNm").val(ui.item.returnValue.U_NAME);

			return false;
		},
		dymcFunc: function(){
			return "";
		},
		clearFunc: function(){
			$("#Driver").val("");
			$("#DriverNm").val("");
		}
	});
	var multiEdocData = [];
	
	MenuBarFuncArr.AddMenu("MBConfirm", "glyphicon glyphicon-bell", commonLang["L_UserQuery_ComBA"], function () {
		var uid = $("#UId").val();
		var shipmentid = $("#ShipmentId").val();
		var status = $("#Status").val();
		if (status == "I" || status == "D") {
		    CommonFunc.Notify("", "You cann't operate this Action!", 500, "warning");
		    return false;
		}
		$.ajax({
			async: true,
			url: rootPath + "SMSMI/doFclConfirmShipment",
			type: 'POST',
			data: { "UId": uid, "ShipmentId": shipmentid },
			beforeSend: function () {
			    CommonFunc.ToogleLoading(true);
			},
			"error": function (xmlHttpRequest, errMsg) {
				CommonFunc.ToogleLoading(false);
				alert(errMsg);
			},
			success: function (data) {
				CommonFunc.ToogleLoading(false);
				if (data.msg == "success") {
				    CommonFunc.Notify("", "Booking Confirm Successful!", 500, "success");
				} else {
					CommonFunc.Notify("", data.msg, 500, "warning");
				return;
				}
				MenuBarFuncArr.MBCancel();
			}
		});
	});

	MenuBarFuncArr.DelMenu(["MBDel", "MBAdd"]);

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