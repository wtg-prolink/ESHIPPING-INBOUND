var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

$(function () {
	schemas = JSON.parse(decodeHtml(schemas));
	CommonFunc.initField(schemas);
	setdisabled(true);

	
	$("#StsCd").on("change", function(){
		var val = $(this).val();

		$(this).val(val.toUpperCase());
	});

	_initMenu();
	initLoadData(_uid);

	//位置放大鏡
	options = {};
	options.gridUrl = rootPath + "Common/GetLocationTypeData";
	options.registerBtn = $("#LocationLookup");
	options.focusItem = $("#Location");
	options.isMutiSel = true;
	options.gridFunc = function (map) {
	    var Cd = map.Cd,
	        OrderBy = map.OrderBy;
	    $("#Location").val(Cd);
	    $("#OrderBy").val(OrderBy);
	}

	options.lookUpConfig = LookUpConfig.LocationTypeLookup;
	initLookUp(options);

	CommonFunc.AutoComplete("#Location", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CMP=" + cmp + "&CD_TYPE~TKLC&CD=", "CD=showValue,CD,CD_DESCP,ORDER_BY", function (event, ui) {
	    $(this).val(ui.item.returnValue.CD);
	    $("#OrderBy").val(ui.item.returnValue.ORDER_BY);
	    return false;
	});
});

function initLoadData(Uid)
{
	if (!Uid)
	    return;
	var param = "sopt_UId=eq&UId=" + Uid;
	$.ajax({
	    async: true,
	    url: rootPath + "TKBL/GetTkItem",
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
	MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch", "MBCopy"]);

	MenuBarFuncArr.MBSave = function (dtd) {
	    var changeData = getChangeValue();
	    //表示值沒變
	    if ($.isEmptyObject(changeData)) {
	        CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
	        MenuBarFuncArr.SaveResult = true;
	        dtd.resolve();
	        setdisabled(true);
	        return;
	    }
	    $.ajax({
	        async: true,
	        url: rootPath + "/System/TKSetupUpdate",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, StsCd: $("#StsCd").val() },
	        dataType: "json",
	        "error": function (xmlHttpRequest, errMsg) {
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();
	        },
	        success: function (result) {
	            if (result.message != "success") 
	            {
	                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
	                MenuBarFuncArr.SaveResult = false;
	                dtd.resolve();
	                return false;
	            }

	            setFieldValue(result.mainData);
	            setdisabled(true);
	            setToolBtnDisabled(true);
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
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
	        url: rootPath + "/System/TKSetupUpdate",
	        type: 'POST',
	        data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, StsCd: $("#StsCd").val() },
	        dataType: "json",
	        "error": function (xmlHttpRequest, errMsg) {
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
	            MenuBarFuncArr.SaveResult = false;
	            dtd.resolve();
	        },
	        success: function (result) {
	            if (result.message != "success") 
	            {
	                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
	                MenuBarFuncArr.SaveResult = false;
	                dtd.resolve();
	                return false;
	            }

	            setFieldValue(result.mainData);
	            setdisabled(true);
	            setToolBtnDisabled(true);
	            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
	            MenuBarFuncArr.SaveResult = true;

	            dtd.resolve();
	        }
	    });
	    return dtd.promise();
	}

	initMenuBar(MenuBarFuncArr);
}