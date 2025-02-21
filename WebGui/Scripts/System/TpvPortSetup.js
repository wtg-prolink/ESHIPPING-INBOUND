var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

$(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    setdisabled(true);


    //$("#StateCd").on("change", function () {
    //    var val = $(this).val();

    //    $(this).val(val.toUpperCase());
    //});

    _initMenu();
    initLoadData(flag, cnty, port);

    //國家放大鏡
    var Cntyoptions = {};
    Cntyoptions.gridUrl = rootPath + "Common/GetCntryCdData";
    Cntyoptions.registerBtn = $("#CntyLookup");
    Cntyoptions.focusItem = $("#Cnty");
    Cntyoptions.isMutiSel = true;
    //Cntyoptions.param = '';
    //Cntyoptions.baseCondition = "GROUP_ID='" + groupId + "'";
    Cntyoptions.gridFunc = function(map) {
        $("#Cnty").val(map.CntryCd);
        $("#CntyNm").val(map.CntryNm);
    }

    Cntyoptions.lookUpConfig = LookUpConfig.CntryLookup;
    initLookUp(Cntyoptions);

    CommonFunc.AutoComplete("#Cnty", 2, "", "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $("input[name='CntyNm']").val(ui.item.returnValue.CNTRY_NM);
        $(this).val(ui.item.returnValue.CNTRY_CD);
        return false;
    });

    //prolink_cd
    prolinkCdOptions = {};
    prolinkCdOptions.gridUrl = rootPath + "Common/GetCityCdData";
    prolinkCdOptions.registerBtn = $("#ProlinkCdLookup");
    prolinkCdOptions.isMutiSel = true;
    prolinkCdOptions.focusItem = $("#ProlinkCd");
    prolinkCdOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //options.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    prolinkCdOptions.gridFunc = function (map) {
        var value = map.CntryCd + map.PortCd;
        $("#ProlinkCd").val(value);
    }
    //prolinkCdOptions.responseMethod = function () { }
    prolinkCdOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(prolinkCdOptions);
    CommonFunc.AutoComplete("#ProlinkCd", 2, "", "dt=port1&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD@@", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        var value = ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD;
        $(this).val(value);
        //$("#ProlinkCd").val(value);
        return false;
    });
    //, function () {
    //    var len = $("#ProlinkCd").val();
    //    if (len.length < 5) {
    //        CommonFunc.Notify("", "此代码不存在", 500, "warning");
    //        $("#ProlinkCd").val("");
    //    }
    //}


    //prolink_cd
    TruckPortOpt = {};
    TruckPortOpt.gridUrl = rootPath + "Common/GetTruckPortCdData";
    TruckPortOpt.registerBtn = $("#TruckPortLookup");
    TruckPortOpt.isMutiSel = true;
    TruckPortOpt.focusItem = $("#TruckPort");
    TruckPortOpt.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //options.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    TruckPortOpt.gridFunc = function (map) {
        var value = map.PortCd;
        $("#TruckPort").val(value);
    }
    //TruckPortOpt.responseMethod = function () { }
    TruckPortOpt.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(TruckPortOpt);
    CommonFunc.AutoComplete("#TruckPort", 2, "", "dt=bstport&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD,PORT_NM", function (event, ui) {
        var value = ui.item.returnValue.PORT_CD;
        $(this).val(value);
        return false;
    });
});

function initLoadData(flag, cnty, port) {
    if (!flag && !cnty && !port)
        return;
    var param = "sopt_Flag=eq&Flag=" + flag + "&sopt_Cnty=eq&Cnty=" + cnty + "&sopt_Port=eq&Port=" + port;
    $.ajax({
        async: true,
        url: rootPath + "System/GetTpvPortDetail?Flag=" + flag + "&Cnty=" + cnty + "&Port=" + port,
        type: 'POST',
        //data: {
        //    'conditions': encodeURI(param)
        //},
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

function _initMenu() {
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch"]);

    MenuBarFuncArr.MBAdd = function () {
    };

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#Flag").val("");
        $("#Cnty").val("");
        $("#Port").val("");
    }

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
            url: rootPath + "System/TpvPortUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, Flag: $("#Flag").val(), Cnty: $("#Cnty").val(), Port: $("#Port").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
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
            url: rootPath + "System/TpvPortUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, Flag: $("#Flag").val(), Cnty: $("#Cnty").val(), Port: $("#Port").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
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

