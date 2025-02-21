var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var IoFlag = getCookie("plv3.passport.ioflag");
$(function(){
	var $MainGrid = $("#MainGrid");
	var $SubGrid = $("#SubGrid");

	_handler.saveUrl = rootPath + "IbGateManage/GateSetupUpdateData";
	_handler.inquiryUrl = "";
	_handler.config = [];

    //Area(Port)放大鏡
	var options = {};
	options.gridUrl = rootPath + "TPVCommon/GetPortData";
	options.registerBtn = $("#PortLookup");
	options.focusItem = $("#DlvArea");
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    $("#DlvArea").val(map.PortCd);
	    $("#DlvAreaNm").val(map.PortNm);
	}

	options.lookUpConfig = LookUpConfig.BSTPORTLookup;
	initLookUp(options);
	CommonFunc.AutoComplete("#DlvArea", 1, "", "dt=bstport&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD,PORT_NM", function (event, ui) {
	    $(this).val(ui.item.returnValue.PORT_CD);
	    $("#DlvAreaNm").val(ui.item.returnValue.PORT_NM);
	    return false;
	});

    //Address放大鏡
	var options = {};
	options.gridUrl = rootPath + "TPVCommon/GetAddrData";
	options.registerBtn = $("#AddrCodeLookup");
	options.focusItem = $("#DlvAddr");
	options.isMutiSel = true;
	options.param = ""; 
	options.baseConditionFunc = function () {
	    var PortCode = $("#DlvArea").val();
	    var Cmp = $("#Cmp").val();
	    return " PORT_CD = '" + PortCode + "' AND CMP ='" + Cmp + "'";
	}
	options.gridFunc = function (map) {
	    $("#DlvAddr").val(map.AddrCode);
	    $("#DlvAddrNm").val(map.Addr);
	}

	options.lookUpConfig = LookUpConfig.BSADDRLookup;
	initLookUp(options);
	CommonFunc.AutoComplete("#DlvAddr", 1, "", "dt=bsaddr&ADDR_CODE=", "ADDR_CODE=showValue,ADDR_CODE,ADDR", function (event, ui) {
	    $(this).val(ui.item.returnValue.ADDR_CODE);
	    $("#DlvAddrNm").val(ui.item.returnValue.ADDR);
	    return false;
	});

	_handler.beforDel = function () {
	    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
	        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
	        return false;
	    }
	}

	_handler.addData = function () {
        //初始化新增数据
        $("#Cmp").prop("readonly", false);
        $("#WsCd").prop("readonly", false);
	    var data = {};
	    data[_handler.key] = uuid();
	    setFieldValue([data]);
	    _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
	    _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
	}

	//_handler.beforSave = function () {
	//    var uid = $("#UId").val();
	//    var cmp = encodeURIComponent($("#Cmp").val());
	//    var wscd = encodeURIComponent($("#WsCd").val());
	//    var isok = false;
	//    $.ajax({
	//        async: false,
	//        url: rootPath + "IbGateManage/CheckHasWsCd",
	//        type: 'POST',
	//        data: { uid: uid,cmp:cmp,wscd:wscd },
	//        "error": function (xmlHttpRequest, errMsg) {
	//            alert(errMsg);
	//        },
	//        "complete": function (xmlHttpRequest, successMsg) {
	//            CommonFunc.ToogleLoading(false);
	//        },
	//        success: function (data) {
	//            if (data.message == "Y")
	//                isok = true;
	//            else {
	//                isok = false;
	//                alert("The Warehouse code has been repeated!" )
	//            }
	//        }
	//    });
 //       return isok;
	//}

	_handler.saveData = function (dtd) {
	    var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var containerArray1 = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    changeData["sub"] = containerArray;
	    changeData["sub1"] = containerArray1;
	    console.log(changeData);
	    var postData = JSON.stringify(changeData);
	    postData = postData.replace(/&#160;/g, "");
	    console.log(postData);
		var data = { "changedData": encodeURIComponent(postData), autoReturnData: false };
	    data["u_id"] = encodeURIComponent($("#UId").val());
	    data["Cmp"] = encodeURIComponent($("#Cmp").val());
	    data["WsCd"] = encodeURIComponent($("#WsCd").val());

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

    var _setMyReadonlysfunc= function () {
        var readonlys = ["WsCd","Cmp"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
    }

    _handler.afterEdit = function () {
        _setMyReadonlysfunc();
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

	    if (data["sub1"])
	        _handler.loadGridData("SubGrid", $SubGrid[0], data["sub1"], [""]);
	    else
	        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);

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
	    ajaxHttp(rootPath + "IbGateManage/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
	        function (data) {
	            if (_handler.setFormData)
	                _handler.setFormData(data);
        });
	}


	var colModel = [
		{ name: 'UId', title: 'u id', index: 'UId', sorttype: 'string', hidden: true },
		{ name: 'UFid', title: 'uf id', index: 'UFid', sorttype: 'string', hidden: true },
		{ name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
		//{ name: 'WsCd', title: 'WsCd', index: 'WsCd', sorttype: 'string', hidden: true, editable: false, width: 90},
		{ name: 'GateNo', title: _getLang("L_GateSetup_RefGate", "月台号码"), index: 'GateNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'Lift', title: _getLang("L_GateSetup_Lift", "是否升降"), index: 'Lift', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }, editable: true },
		{ name: 'Status', title: _getLang("L_GateReserve_Status", "状态"), index: 'Status', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Available;S:Temporary Closed;A:In use;N:Out of Service', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'CntrNo', title: _getLang("L_GateReserve_CntrNo", "货柜号码"), index: 'CntrNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: false },
		{ name: 'Remark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
		{ name: 'SuspendFrom', 
			title: _getLang("L_GateSetup_SuspendFrom", "暂停开始时间"),
			index: 'SuspendFrom', 
			width: 150, 
			align: 'left', 
			sorttype: 'string', 
			hidden: false, 
			editable: true, 
			editoptions: myEditDateInit,
			formatter: 'date',
			formatoptions: {
				srcformat: 'ISO8601Long',
				newformat: 'Y-m-d',
				defaultValue:null
			}
		},
		{ name: 'SuspendTo', 
			title: _getLang("L_GateSetup_SuspendTo", "暂停截止时间"),
			index: 'SuspendTo', 
			width: 150, 
			align: 'left', 
			sorttype: 'string', 
			hidden: false, 
			editable: true, 
			editoptions: myEditDateInit,
			formatter: 'date',
			formatoptions: {
				srcformat: 'ISO8601Long',
				newformat: 'Y-m-d',
				defaultValue:null
			}
		},
		{ name: 'SeqNo', title: 'Seq No.', index: 'SeqNo', width: 70, align: 'right', formatter: 'integer', hidden: false, editable: true }
	];

	_handler.intiGrid("MainGrid", $MainGrid, {
	    colModel: colModel, caption: 'Gate List', delKey: ["UId"],
	    onAddRowFunc: function (rowid) {
	    },
	    beforeSelectRowFunc: function (rowid) {
	    },
	    beforeAddRowFunc: function (rowid) {
	    }
	});

	var colModel1 = [
		{ name: 'UId', title: 'u id', index: 'UId', sorttype: 'string', hidden: true },
		{ name: 'UFid', title: 'uf id', index: 'UFid', sorttype: 'string', hidden: true },
		{ name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'string', hidden: true },
		{ name: 'TFrom', title: 'From', index: 'TFrom', sorttype: 'float', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: '0:00\:00;1:01\:00;2:02\:00;3:03\:00;4:04\:00;5:05\:00;6:06\:00;7:07\:00;8:08\:00;9:09\:00;10:10\:00;11:11\:00;12:12\:00;13:13\:00;14:14\:00;15:15\:00;16:16\:00;17:17\:00;18:18\:00;19:19\:00;20:20\:00;21:21\:00;22:22\:00;23:23\:00', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'TTo', title: 'To', index: 'TTo', sorttype: 'float', width: 150, hidden: false, editoptions: { maxlength: 20 }, editable: true, formatter: "select", edittype: 'select', editoptions: { value: '1:01\:00;2:02\:00;3:03\:00;4:04\:00;5:05\:00;6:06\:00;7:07\:00;8:08\:00;9:09\:00;10:10\:00;11:11\:00;12:12\:00;13:13\:00;14:14\:00;15:15\:00;16:16\:00;17:17\:00;18:18\:00;19:19\:00;20:20\:00;21:21\:00;22:22\:00;23:23\:00', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'Remark', title: 'Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
	];

	_handler.intiGrid("SubGrid", $SubGrid, {
	    colModel: colModel1, caption: 'Time', delKey: ["UId"],
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
	    _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
	    gridEditableCtrl({ editable: true, gridId: "MainGrid" });
	    gridEditableCtrl({ editable: true, gridId: "SubGrid" });
	}

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
	};
	CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
	initLookUp(CmpOpt);
	CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP", function (event, ui) {
	    $(this).val(ui.item.returnValue.CMP);
	    return false;
	});
	registBtnLookup($("#PicLookup"), {
	    item: '#Pic', url: rootPath + "System/UserSetInquiryData", config: LookUpConfig.UserLookup, param: "", selectRowFn: function (map) {
	        $("#Pic").val(map.UId);
	    }
	}, {focusItem:$("#Pic")}, LookUpConfig.GetUserAuto(groupId, "TCAR", undefined, function ($grid, rd, elem) {
	    $("#Pic").val(rd.U_I);
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
function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}
