var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid = null;
var $SubGrid2 = null;
var $SubGrid3 = null;
var $SubGrid4 = null;
var $DnGrid = null;
var $CcGrid = null;
var $TcGrid = null;
var $CntrGrid = null; 
var Bstatus = "";
$(function(){
	$SubGrid = $("#SubGrid");
	$SubGrid2 = $("#SubGrid2");
	$SubGrid3 = $("#SubGrid3");
	$DnGrid = $("#DnGrid");
	$CcGrid = $("#CcGrid");
	$TcGrid = $("#TcGrid");
	$CntrGrid = $("#CntrGrid");
	$SubGrid4 = $("#SubGrid4"); 
	genPartyGrid();
	var containerArray = "";
	var Ptlist = { pt: PTlist };
	_handler.saveUrl = rootPath + "SMSMI/ModifyBookingUpdateData";
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
	}

	_handler.editData = function () {
		
	}

	_handler.afterEdit = function () {
	    SetBookingEdit();
	}


	MenuBarFuncArr.EndFunc = function(){
		if(Bstatus === "C" || Bstatus === "H")
		{
			$("#ccinfo").find("input").prop("disabled", true);
			$("#ccinfo").find("button").prop("disabled", true);
			$("#ccinfo").find("textarea").prop("disabled", true);
			$("#ccinfo").find("select").prop("disabled", true);
		}
	}

	_handler.saveData = function (dtd) {
       		var SubGrid3Data = $SubGrid3.jqGrid('getGridParam', "arrangeGrid")();
       		var SubGrid2Data = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
       		
       		var CcData = $CcGrid.jqGrid('getGridParam', "arrangeGrid")();
       		var TcData = $TcGrid.jqGrid('getGridParam', "arrangeGrid")();
       		var changeData = getChangeValue();//获取所有改变的值
       		if (isEmpty(containerArray))
       		    containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
       		changeData["sub"] = containerArray;
       		changeData["sub2"] = SubGrid2Data;
       		changeData["sub3"] = SubGrid3Data;
       		if(TRAN_TYPE == 'A' || TRAN_TYPE == 'E')
       		{
       			var SubGrid4Data = $SubGrid4.jqGrid('getGridParam', "arrangeGrid")();
       			changeData["sub4"] = SubGrid4Data;
       		}
       		
       		changeData["cc"] = CcData;
       		changeData["tc"] = TcData;
       		if(TRAN_TYPE == 'F' || TRAN_TYPE == 'L' || TRAN_TYPE == 'R')
       		{
       			var CntrData = $CntrGrid.jqGrid('getGridParam', "arrangeGrid")();
       			changeData["cntr"] = CntrData;
       		}
       		
		var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
		data["u_id"] = encodeURIComponent($("#UId").val());
		data["ShipmentId"] = encodeURIComponent($("#ShipmentId").val());
		data["n"] = encodeURIComponent($("#CntrN").val());
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

		setFieldValue(data["main"] || [{}]);

		if (data["sub"])
			_handler.loadGridData("SubGrid", $SubGrid[0], data["sub"], [""]);
		else
			_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

		if (data["sub2"])
		{
		    _handler.loadGridData("SubGrid2", $SubGrid2[0], data["sub2"], [""]);
		}
		else
		    _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);

		if (data["sub3"])
		{
		    _handler.loadGridData("SubGrid3", $SubGrid3[0], data["sub3"], [""]);
		    
		}
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
		{
		    _handler.loadGridData("CntrGrid", $CntrGrid[0], data["smicntr"], [""]);
		}
		else
		    _handler.loadGridData("CntrGrid", $CntrGrid[0], [], [""]);

		if (data["sub4"])
		    _handler.loadGridData("SubGrid4", $SubGrid4[0], data["sub4"], [""]);
		else
		    _handler.loadGridData("SubGrid4", $SubGrid4[0], [], [""]);

		if(data["n"])
		{
			$("#CntrN").val(data["n"]);
		}

		SetAddrModal();

		setFieldValue(data["main"] || [{}]);
		_handler.beforLoadView();
		setdisabled(true);
		setToolBtnDisabled(true);
		MenuBarFuncArr.Enabled(["MBCopy", "MBEdoc","MBAllocation","MBCconfirm","MBTconfirm", "MBSendAddr"]);
		if (data["main"].length > 0) {
		    Bstatus = data["main"][0]["Bstatus"];
		    var Inspection = data["main"][0]["Inspection"];
		    var TcInspection = data["main"][0]["TcInspection"];
		    if (Bstatus === "C" || Bstatus === "H") {
		        MenuBarFuncArr.Disabled(["MBDel", "MBCconfirm"]);
		        $("#ccinfo").find("input").prop("disabled", true);
		        $("#ccinfo").find("button").prop("disabled", true);
		        $("#ccinfo").find("textarea").prop("disabled", true);
		        $("#ccinfo").find("select").prop("disabled", true);
		    }

		    if (Bstatus === "I") {
		        MenuBarFuncArr.Disabled(["MBDel", "MBTconfirm"]);
		    }

		    if (Inspection == "N") {
		        $("#CerNo").prop("disabled", true);
		    }

		    if (TcInspection == "N") {
		        $("#CerNo").prop("disabled", true);
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
                multiEdocData.push({ jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': cmp, 'STN': '*' });
                MenuBarFuncArr.initEdoc($("#OUid").val(), _handler.topData["GroupId"], $("#OLocation").val(), "*", multiEdocData, callBackFunc);
            } else {
                MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], cmp, "*", multiEdocData, callBackFunc);
            }
        });
		if(HASRV == "true")
		{
			MenuBarFuncArr.Disabled(["MBSendAddr", "MBEdit"]);
		}

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
		
	}

	$("#Inspection").on("change", function(){
		var val = $(this).val();

		if(val == "N")
		{
			$("#CerNo").val("");
			$("#CerNo").prop("disabled", true);
		}
		else
		{
			$("#CerNo").prop("disabled", false);
		}
	});

	$("#TcInspection").on("change", function(){
		var val = $(this).val();

		if(val == "N")
		{
			$("#TcCerNo").val("");
			$("#TcCerNo").prop("disabled", true);
		}
		else
		{
			$("#TcCerNo").prop("disabled", false);
		}
	});



	_initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏
	if (!isEmpty(_uid)) {
		_handler.topData = { UId: _uid };

		_handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
		MenuBarFuncArr.MBCancel();
	}

	var multiEdocData = [];
	
	MenuBarFuncArr.AddMenu("MBSendAddr", "glyphicon glyphicon-th-large",commonLang["L_SMSMI_MBSendAddr"], function () {
		$("#AddrModal").modal("show");
		gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
		gridEditableCtrl({ editable: true, gridId: "CntrGrid" });
	});
});