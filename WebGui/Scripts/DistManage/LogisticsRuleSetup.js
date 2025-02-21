var _dm = new dm();
var url = "";
var groupId = getCookie("plv3.passport.groupid");
var cmp = getCookie["plv3.passport.basecompanyid"];
var stn = getCookie["plv3.passport.basestation"];
var userId = getCookie("plv3.passport.user");

$(document).ready(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    url = rootPath + "/DistManage/LogisticsRuleInquiryData";

    SetSearchOption();

    MenuBarFuncArr.DelMenu(["MBApply", "MBInvalid"]);

    MenuBarFuncArr.MBAdd = function () {
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#UId").removeAttr('required');
        $("#UId").val("");
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getAllKeyValue();
        $.ajax({
            async: true,
            url: rootPath + "/DistManage/LogisticsRuleUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
                else alert("success");
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                //成功后将页面的数据移除，并设置页面不可编辑
                setFieldValue(undefined, "");
                setdisabled(true);
                setToolBtnDisabled(false);
            }
        });
    }

    var searchColumns = {
        caption: "Shipment Search",
        sortname: "UId",
        refresh: false,//Year,Month,Week,Cmp,Region,Pod,No1,No2,No3,No4,No5
        columns: [{ name: "UId", title: "UId", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: "OdType", title: "@Resources.Locale.L_CntySetup_CntryNm", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Year', title: '@Resources.Locale.L_BookingQuery_Views_284', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Month', title: '@Resources.Locale.L_common_Scripts_19', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Week', title: '@Resources.Locale.L_ContainUsageSetup_Scripts_118', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Cmp', title: '@Resources.Locale.L_MailGroupSetup_Cmp', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Pod', title: 'Pod', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    }

    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        $.ajax({
            async: true,
            url: rootPath + "/DistManage/LogisticsRuleUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                //if (successMsg != "success") return null;
                //else alert("success");
                //setdisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    dtd.resolve();
                    return false;
                }
                //alert(result.message);
                setFieldValue(result.rows);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    $("#Rules").change(function () {
        var _rules = $(this).children('option:selected').val();//这就是selected的值 
        if (_rules == "C") {
            $("#LspNo").attr('required', true);
            $("#PartyType").attr('required', true);
        } else {
            $("#LspNo").removeAttr('required');
            $("#PartyType").removeAttr('required');
        }
    });

    MenuBarFuncArr.DelMenu(["MBEdoc","MBSearch", "MBApprove", "MBErrMsg"]);

    setdisabled(true);
    setToolBtnDisabled(true);
    getSelectOptions();
    initLoadData(Uid);
});

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "Common/GetTrackingTranModeData",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var data = eval('(' + data + ')');
            var TranModeOptions = data.rows;
            appendSelectOption($("#TranMode"), TranModeOptions);
        }
    });
}

function appendSelectOption(selectId, options) {
    selectId.empty();
    $.each(options, function (idx, option) {
        selectId.append("<option value=\"" + option.Cd + "\">" +option.Cd+ ":"+ option.CdDescp + "</option>");
    });
}


function initLoadData(Uid) {
    if (!Uid)
        return;
    var param = "sopt_UId=eq&UId=" + Uid;
    $.ajax({
        async: true,
        url: rootPath + "DistManage/LogisticsRuleInquiryData",
        type: 'POST',
        data: {
            sidx: 'UId',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {

            _dataSource = result.rows;
            setFieldValue(result.rows);

            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove", "MBEdoc"]);
            CommonFunc.ToogleLoading(false);
        }
    });
}

//设置放大镜
function SetSearchOption() {
    //总公司放大鏡
    var CmpOptions = {};
    CmpOptions.gridUrl = rootPath + "Common/GetCompanyData";
    CmpOptions.registerBtn = $("#CmpLookup");
    CmpOptions.focusItem = $("#Cmp");
    CmpOptions.isMutiSel = true;
    CmpOptions.param = "";
    CmpOptions.gridFunc = function (map) {
        var value = map.Cmp;
        $("#Cmp").val(value);
        return;
    }
    CmpOptions.lookUpConfig = LookUpConfig.CmpLookup;
    initLookUp(CmpOptions);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&CMP%", "STN=showValue,STN", function (event, ui) {
        $(this).val(ui.item.returnValue.Stn);
        return false;
    });

    //Region放大鏡
    var options = {};
    options.gridUrl = rootPath + LookUpConfig.TrgnUrl;
    options.registerBtn = $("#RegionLookup");
    CmpOptions.focusItem = $("#Region");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        var value = map.Cd;
        $("#Region").val(value);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.TrgnLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#Region", 1, "", "dt=bsc&CD_TYPE=TRGN&GROUP_ID=" + groupId + "&CD%", "CD=showValue,CD", function (event, ui) {
        $(this).val(ui.item.returnValue.Stn);
        return false;
    });

    //Term贸易条款放大鏡
    var PodOptions = {};
    PodOptions.gridUrl = rootPath + LookUpConfig.TermUrl;
    PodOptions.param = '';
    PodOptions.registerBtn = $("#TermLookup");
    PodOptions.focusItem = $("#Term");
    PodOptions.isMutiSel = true;
    PodOptions.gridFunc = function (map) {
        var value = map.Cd;
        $("#Term").val(value);
        return value;
    }
    PodOptions.lookUpConfig = LookUpConfig.TermLookup;
    initLookUp(PodOptions);
    CommonFunc.AutoComplete("#Term", 1, "", "dt=bsc&&CD_TYPE=TD&GROUP_ID=" + groupId + "&CD%",  "CD&CD_DESCP=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.Stn);
        return false;
    });
   

    //城市港口放大鏡
    var PodOptions = {};
    PodOptions.gridUrl = rootPath + LookUpConfig.CityPortUrl;
    PodOptions.registerBtn = $("#PolLookup");
    PodOptions.focusItem = $("#Pol");
    PodOptions.isMutiSel = true;
    PodOptions.param = "";
    PodOptions.gridFunc = function (map) {
        var value = map.CntryCd + map.PortCd;
        $("#Pol").val(value);
        return value;
    }
    PodOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(PodOptions);
    CommonFunc.AutoComplete("#Pol", 1, "", "dt=port1&GROUP_ID=" + groupId + "&PORT_CD%", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.Stn);
        return false;
    });

    var PodOptions = {};
    PodOptions.gridUrl = rootPath + LookUpConfig.CityPortUrl;
    PodOptions.registerBtn = $("#PodLookup");
    PodOptions.focusItem = $("#Pod");
    PodOptions.param = "";
    PodOptions.isMutiSel = true;
    PodOptions.gridFunc = function (map) {
        var value = map.CntryCd + map.PortCd;
        $("#Pod").val(value);
        return value;
    }
    PodOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(PodOptions);
    CommonFunc.AutoComplete("#Pod", 1, "", "dt=port1&GROUP_ID=" + groupId + "&PORT_CD%", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.Stn);
        return false;
    });

    //供应商放大镜
    options = {};
    options.gridUrl = rootPath + LookUpConfig.PartyNoUrl;
    options.registerBtn = $("#ShipperCdLookup");
    options.param = "";
    options.isMutiSel = true;
    options.selfSite = true;
    options.focusItem = $("#Shipper");
    options.gridFunc = function (map) {
        var custCd = map.PartyNo,
            localNm = map.PartyName;
        $("#Shipper").val(custCd);
        //$("#SupplierNm").val(localNm);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartyNoLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Shipper", 2, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        //$("input[name='SupplierNm']").val(ui.item.returnValue.LOCAL_NM);
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    options = {};
    options.gridUrl = rootPath + LookUpConfig.PartyNoUrl;
    options.registerBtn = $("#CustCdLookup");
    options.param = "";
    options.isMutiSel = true;
    options.selfSite = true;
    options.focusItem = $("#CustCd");
    options.gridFunc = function (map) {
        var custCd = map.PartyNo,
            localNm = map.PartyName;
        $("#Customer").val(custCd);
        //$("#SupplierNm").val(localNm);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartyNoLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Customer", 2, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        //$("input[name='SupplierNm']").val(ui.item.returnValue.LOCAL_NM);
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    var options = {};
    options.gridUrl = rootPath + LookUpConfig.PartyTypeUrl;
    options.registerBtn = $("#PartyTypeLookup");
    CmpOptions.focusItem = $("#PartyType");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
        var value = map.Cd;
        $("#PartyType").val(value);
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartyTypeLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#PartyType", 1, "", "dt=bsc&CD_TYPE=PT&GROUP_ID=" + groupId + "&CD%", "CD=showValue,CD", function (event, ui) {
        //$(this).val(ui.item.returnValue.Stn);
        $("#PartyType").val(ui.item.returnValue.CD);
        return false;
    });

    options = {};
    options.gridUrl = rootPath + LookUpConfig.PartyNoUrl;
    options.registerBtn = $("#LspNoLookup");
    options.param = "";
    options.isMutiSel = true;
    options.selfSite = true;
    options.focusItem = $("#LspNo");
    options.gridFunc = function (map) {
        var custCd = map.PartyNo,
            localNm = map.PartyName;
        $("#LspNo").val(custCd);
        $("#LspNm").val(localNm);
    }
    options.baseConditionFunc = function () {
        var _partytype = $("#PartyType").val();
        return "PARTY_TYPE LIKE '%" + _partytype + ";%'";
    }
    options.responseMethod = function () { }
    options.lookUpConfig = LookUpConfig.PartyNoLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#LspNo", 2, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        //$("input[name='SupplierNm']").val(ui.item.returnValue.LOCAL_NM);
        $(this).val(ui.item.returnValue.PARTY_NO);
        $("input[name='LspNm']").val(ui.item.returnValue.PARTY_NAME);
        return false;
    });
}

