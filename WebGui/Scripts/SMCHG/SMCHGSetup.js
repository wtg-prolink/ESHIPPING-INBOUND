var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];

jQuery(document).ready(function ($) {
    //$("#Cmp").val(cmp);
    mtSchemas = JSON.parse(decodeHtml(mtSchemas));
    CommonFunc.initField(mtSchemas, "mt");

    setdisabled(true);
    setSTdisabled(true);
    getSelectOptions();
    
    _initMainMenu();
    _initLookup();
    initLoadData(_uid);
    $("#ChgType").attr('required', true);
    $("#TranMode").attr('required', true);
    $("#ChgCd").attr('required', true);
    $("#ChgDescp").attr('required', true);

    /*$("#TranMode").on("change", function () {
        var val = $(this).val();
        var cmp = $("#Cmp").val();
        if (val == "") {
            GetCondition(cmp, "");
        } 
    });*/

    $("#ChgCd").on("change", function(){
        var val = $(this).val();

        $(this).val(val.toUpperCase());
    });
});

function _initMainMenu()
{
    MenuBarFuncArr.MBAdd = function(){
        $("#UId").removeAttr('required');
        $("#Repay").val("M");
        $("#IoType").val("I");
    };

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
    }

    MenuBarFuncArr.MBEdit = function () {
        //$("#Repay").val("N");
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#UId").removeAttr('required');
        $("#UId").val("");
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;

        var changeData = getChangeValue();
        var ChgDescp = $("#ChgDescp").val();
        var ChgEdescp = $("#ChgEdescp").val();
        //var regex = new RegExp("[^a-zA-Z0-9]", "i");
        //if (regex.test(ChgDescp) == true) {
        //    alert("费用说明不规范");
        //    MenuBarFuncArr.SaveResult = false;
        //    dtd.resolve();
        //    return;
        //}
        //if (regex.test(ChgEdescp) == true) {
        //    alert("英文说明不规范");
        //    MenuBarFuncArr.SaveResult = false;
        //    dtd.resolve();
        //    return;
        //}
        var regex = new RegExp("[()&-]", "i");
        if (regex.test(ChgDescp) == true || regex.test(ChgEdescp) == true) {
            alert("System cannot accept ANY description of cost items including the symbol like (),&,-. Thank you");
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return;
        }
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "SMCHG/SMCHGUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                console.log(result.message);
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }
                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;

                dtd.resolve();
                //location.reload();
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getChangeValue();
        //表示值沒變
        $.ajax({
            async: true,
            url: rootPath + "SMCHG/SMCHGUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                //dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelFail", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    //dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
            }
        });
        
    }
    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBErrMsg"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
}

function _initLookup()
{
    commonSetBscData("TranModeLookup", "TranMode", "TranDescp", "TNT");
    commonBscAuto("TranModeLookup", "TranMode", "TranDescp", "TNT");

    commonSetBscData("ChgLevelLookup", "ChgLevel", "", "TCS")
    commonBscAuto("ChgLevelLookup", "ChgLevel", "", "TCS");

    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#Cmp");
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Cmp").val(value);
        $("#CmpNm").val(map.CdDescp);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        $("#CmpNm").val(ui.item.returnValue.NAME);
        return false;
    });
}

function GetCondition(Cmp, TranMode) {
    var postdata = "";
    if (TranMode != null) {
        postdata = { "conditions": "sopt_Cmp=eq&Cmp=" + Cmp + "&sopt_TranMode=eq&TranMode=" + TranMode, "baseCondition": "" };
    }
    else {
        postdata = { "conditions": "sopt_Cmp=eq&Cmp=" + Cmp, "baseCondition": "" };
    }
        $("#SMCHGGrid").jqGrid("setGridParam", {
        url: rootPath + "SMCHG/SMCHGSetupInquiryData",
        postData: postdata,
        page: 1,
        datatype: "json"
    }).trigger("reloadGrid");

}


function initLoadData(Uid)
{
    if (!Uid)
    {
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "SMCHG/GetSmchgDetail",
        type: 'POST',
        data: {
            UId: Uid
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            setFieldValue(maindata.rows);


            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            
            CommonFunc.ToogleLoading(false);
        }
    });
}

function getSelectOptions() {
    $.ajax({
        async: false,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TNT || [];
            var obj = {"cd": "O", "cdDescp": "ALL"};
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];

            trnOptions.push(obj);
            appendSelectOption($("#TranMode"), trnOptions);
        }
    });
}