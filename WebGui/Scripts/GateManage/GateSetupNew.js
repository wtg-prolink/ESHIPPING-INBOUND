var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var IoFlag = getCookie("plv3.passport.ioflag");
$(function(){
	var $MainGrid = $("#MainGrid");

	_handler.saveUrl = rootPath + "GateManage/UpdateData";
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


	_handler.saveData = function (dtd) {
	    var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
	    var changeData = getChangeValue();//获取所有改变的值
	    changeData["sub"] = containerArray;
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
	    ajaxHttp(rootPath + "GateManage/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
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
   		{ name: 'GateNo', title: '@Resources.Locale.L_GateSetup_RefGate', index: 'GateNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
	    { name: 'Lift', title: '@Resources.Locale.L_GateSetup_Lift', index: 'Lift', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }, editable: true },
	    { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 80, sorttype: 'string', hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: '@Resources.Locale.L_FCLChgSetup_Script_168', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
		{ name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'SuspendFrom', 
            title: '@Resources.Locale.L_GateSetup_SuspendFrom',
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
            title: '@Resources.Locale.L_GateSetup_SuspendTo',
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
        }
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