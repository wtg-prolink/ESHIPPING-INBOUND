var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

$(function () {
	schemas = JSON.parse(decodeHtml(schemas));
	CommonFunc.initField(schemas);
	setdisabled(true);

	
	$("#StateCd").on("change", function(){
		var val = $(this).val();

		$(this).val(val.toUpperCase());
	});

	_initMenu();
	initLoadData(_uid);

	//國家放大鏡
	var options = {};
	options.gridUrl = rootPath + "TPVCommon/GetCountryDataForLookup";
	options.registerBtn = $("#CntryCdLookup");
	options.focusItem = $("#CntryCd");
	options.param = "";
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    $("#CntryCd").val(map.CntryCd);
	    $("#CntryNm").val(map.CntryNm);
	}

	options.lookUpConfig = LookUpConfig.CntryLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#CntryCd", 1, "", "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
	    $(this).val(ui.item.returnValue.PARTY_NO);
	    $("#CntryNm").val(ui.item.returnValue.CNTRY_NM);
	    return false;
	});

	setBscData("RegionCdLookup", "RegionCd", "RegionNm", "TRGN");
});

function initLoadData(Uid)
{
	if (!Uid)
	    return;
	var param = "sopt_UId=eq&UId=" + Uid;
	$.ajax({
	    async: true,
	    url: rootPath + "System/GetBsStateDetail",
	    type: 'POST',
	    data: {
	        UId: Uid,
	        sidx: 'UId',
	        'conditions': encodeURI(param)
	    },
	    dataType: "json",
	    beforeSend: function () {
	        CommonFunc.ToogleLoading(true);
	    },
	    "error": function (xmlHttpRequest, errMsg) {
	    },
	    success: function (result) {
	    	var maindata = result.main;
	    	console.log(maindata);
	        setFieldValue(maindata);

	        setdisabled(true);
	        setToolBtnDisabled(true);

	        MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
	        MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
	        CommonFunc.ToogleLoading(false);
	    }
	});
}

function _initMenu()
{
	MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch"]);

	MenuBarFuncArr.MBAdd = function(){
		$("#UId").removeAttr('required');
	};

	MenuBarFuncArr.MBCopy = function (thisItem) {
	    $("#UId").removeAttr('required');
	    $("#UId").val("");
	}

	MenuBarFuncArr.MBSave = function (dtd) {
	    var changeData = getChangeValue();
	    //表示值沒變
	    if ($.isEmptyObject(changeData)) {
	        CommonFunc.Notify("", "@Resources.Locale.L_TKSetup_Success", 500, "success");
	        MenuBarFuncArr.SaveResult = true;
	        dtd.resolve();
	        setdisabled(true);
	        return;
	    }
	    $.ajax({
	        async: true,
	        url: rootPath + "System/BsStateUpdate",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
	        dataType: "json",
	        "error": function (xmlHttpRequest, errMsg) {
	            CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail", 500, "warning");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();
	        },
	        success: function (result) {
	            if (result.message != "success") 
	            {
	                CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail", 500, "warning");
	                MenuBarFuncArr.SaveResult = false;
	                dtd.resolve();
	                return false;
	            }

	            setFieldValue(result.mainData);
	            setdisabled(true);
	            setToolBtnDisabled(true);
	            CommonFunc.Notify("", "@Resources.Locale.L_TKSetup_Success", 500, "success");
	            MenuBarFuncArr.SaveResult = true;

	            dtd.resolve();
	        }
	    });
	    return dtd.promise();
	}

	MenuBarFuncArr.MBDel = function () {
	    var changeData = getChangeValue();
	    //表示值沒變
	    $.ajax({
	        async: true,
	        url: rootPath + "System/BsStateUpdate",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
	        dataType: "json",
	        "error": function (xmlHttpRequest, errMsg) {
	            CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_SFail", 500, "warning");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();
	        },
	        success: function (result) {
	            if (result.message != "success") 
	            {
	                CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelFail", 500, "warning");
	                MenuBarFuncArr.SaveResult = false;
	                dtd.resolve();
	                return false;
	            }

	            setFieldValue(result.mainData);
	            setdisabled(true);
	            setToolBtnDisabled(true);
	            CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelS", 500, "success");
	            MenuBarFuncArr.SaveResult = true;

	            dtd.resolve();
	        }
	    });
	    return dtd.promise();
	}

	initMenuBar(MenuBarFuncArr);
}

function setBscData(lookUp, Cd, Nm, pType)
{
	//SMPTY放大鏡
	options = {};
	options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
	options.registerBtn = $("#"+lookUp);
	options.focusItem = $("#" + Cd);
	options.baseCondition = " GROUP_ID='"+groupId+"' AND CMP='"+cmp+"' AND CD_TYPE='"+pType+"'";
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    $("#" + Cd).val(map.Cd);

	    if(Nm != "")
	    	$("#" + Nm).val(map.CdDescp);
	}

	options.lookUpConfig = LookUpConfig.BSCodeLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
	    $(this).val(ui.item.returnValue.CD);

	    if(Nm != "")
	    	$("#" + Nm).val(ui.item.returnValue.CD_DESCP);

	    return false;
	});
}