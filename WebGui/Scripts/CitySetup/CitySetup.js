var url = ""
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user"),
    Supplier = "",
    PartNo = "";

var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];

function initLoadData(PortCd, CntryCd) {

    if (!PortCd || !CntryCd)
        return;
    var param = "sopt_PortCd=eq&PortCd=" + PortCd;
    param += "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "/System/CitySetupQuery",
        type: 'POST',
        data: {
            model: "CitySetupModel",
            sidx: 'CreateDate',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            _dataSource = result.rows;
            setFieldValue(result.rows);
            //绑定Grid
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove"]);
        }
    });
}
function check()
{
    var val = $("#Gm").val();
    if (val >= -23 && val <= 23)
    { var val = Math.round(val); $("#Gm").val(val); }
    else
    { alert("@Resources.Locale.L_CitySetup_GMO"); $("#Gm").val(''); }
    
}

function PortcdCheck() {
    $("#PortCd").blur(function () {
        var len = $(this).val().length;
        if (len != 3)
        {
            CommonFunc.Notify("", "@Resources.Locale.L_CitySetup_Script_84", 500, "warning");
            $("#PortCd").val("");
            //$("#PortCd").focus();
        }
    });
}
jQuery(document).ready(function ($) {

    url = rootPath + "System/CityPortInquiryData";
    MenuBarFuncArr.MBCancel = function () {
        var NowSupplier = $("#PortCd").val();
        var NowPartNo = $("#CntryCd").val();
        var postdata = { "conditions": "sopt_1=ne&1=1" };
        if (groupId && NowSupplier && NowPartNo && stn) {
            postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_PortCd=eq&PortCd=" + NowSupplier + "&sopt_CntryCd=eq&CntryCd=" + NowPartNo + "&sopt_Stn=eq&Stn=" + stn };
        }
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getAllKeyValue();
        $.ajax({
            async: true,
            url: rootPath + "System/CitySetupUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success")
                    return null;
                else
                    CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelS", 500, "success");
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    return;
                }
                //成功后将页面的数据移除，并设置页面不可编辑
                //setFieldValue();
                //setdisabled(true);
                //setToolBtnDisabled(false);

                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_BSCSSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Disabled(["MBEdit", "MBDel", "MBCopy"]);
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBAdd = function (thisItem) {
        $("#Cmp").val(cmp);
        $("#Stn").val(stn);
        $("#CreateBy").val(userId);
    }

    MenuBarFuncArr.MBEdit = function () {
        editable = true;
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#PortCd").val("");
        $("#CntryCd").val("");
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
    }

    //放大镜
    /*var CitySearchOptions = {};
    CitySearchOptions.gridUrl = url;
    CitySearchOptions.registerBtn = $("#MBSearch");
    CitySearchOptions.isMutiSel = true;
    CitySearchOptions.param = '';
    CitySearchOptions.gridFunc = function (map) {
        var PortCd = map.PortCd,
                CntryCd = map.CntryCd;

        var param = "sopt_PortCd=eq&PortCd=" + PortCd;
        param += "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
        

        //将获取的数据作为条件进行reload数据
        $.ajax({
            async: true,
            url: url,
            type: 'POST',
            data: {
                model: "CitySetupModel",
                sidx: 'PortCd',
                'conditions': encodeURI(param),
                page: 1,
                rows: 20
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                _dataSource = result.rows;
                setFieldValue(result.rows);
                PortCd = result.rows[0].PortCd;
                CntryCd = result.rows[0].CntryCd;                
                var postdata = { "conditions": "sopt_1=ne&1=1" };
                if (groupId && PortCd && CntryCd) {
                    postdata = { "conditions": "sopt_PortCd=eq&PortCd=" + PortCd + "&sopt_CntryCd=eq&CntryCd=" + CntryCd};
                }
            }
        });
    }
    CitySearchOptions.responseMethod = function () { }
    CitySearchOptions.lookUpConfig = LookUpConfig.CitySetupLookup;

    MenuBarFuncArr.MBSearch = function (thisItem) {
        initLookUp(CitySearchOptions);
    }*/

    //notice MBSave一定要傳入dtd
    MenuBarFuncArr.MBSave = function (dtd) {
        var len = $("#PortCd").val().length;
        if (len != 3) {
            CommonFunc.Notify("", "@Resources.Locale.L_CitySetup_Script_84", 500, "warning");
            $("#PortCd").val("");
            return false;
        }
        if (checkNoAllowNullFields() == false)
            return false;
        var changeData = getChangeValue();
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "System/CitySetupUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), portCd: $("#PortCd").val(), cntryCd: $("#CntryCd").val(), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    //notice ajax warning 一定要放入下面三行
                    CommonFunc.Notify("", result.message, 500, "warning");
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
            }
        });
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.DelMenu(["MBApply", "MBApprove", "MBInvalid", "MBErrMsg", "MBEdoc", "MBSearch"]);

    setdisabled(true);
    setToolBtnDisabled(true);

    //国家代码放大镜
    Cntyoptions = {};
    Cntyoptions.gridUrl = rootPath + "Common/GetCntryCdData";
    Cntyoptions.registerBtn = $("#CntryCdLookup");
    Cntyoptions.isMutiSel = true;
    Cntyoptions.param = '';
    Cntyoptions.baseCondition = "GROUP_ID='" + groupId + "'";
    Cntyoptions.gridFunc = function (map) {
        var cd = map.CntryCd,
           cn = map.CntryNm;
        $("#CntryCd").val(cd);
        $("#CntryNm").val(cn);
    }
    Cntyoptions.responseMethod = function () { }
    Cntyoptions.lookUpConfig = LookUpConfig.CntyCdLookup;
    initLookUp(Cntyoptions);
    CommonFunc.AutoComplete("#CntryCd", 2, "", "dt=country&GROUP_ID=" + groupId+ "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $("input[name='CntryNm']").val(ui.item.returnValue.CNTRY_NM);
        $(this).val(ui.item.returnValue.CNTRY_CD);
        return false;
    });
    
    ////城市代码
    //CommonFunc.AutoComplete("#PortCd", 2, "", "dt=port&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD,PORT_NM", function (event, ui) {
    //    $(this).val(ui.item.returnValue.PORT_CD);
    //    $("#PortNm").val(ui.item.returnValue.PORT_NM);
    //    return false;
    //}, function () {
    //    $("#PortNm").val("");
    //});
    //城市代码放大镜
    /*Cityoptions = {};
    Cityoptions.gridUrl = rootPath + "Common/GetBscityData";
    Cityoptions.registerBtn = $("#PortCdLookup");
    Cityoptions.isMutiSel = true;
    Cityoptions.param = '';
    Cityoptions.baseCondition = "GROUP_ID='" + groupId + "'";
    Cityoptions.gridFunc = function (map) {
        var cd = map.PortCd,
           cn = map.PortNm;
        $("#PortCd").val(cd);
        $("#PortNm").val(cn);
    }
    Cityoptions.responseMethod = function () { }
    Cityoptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(Cityoptions);*/


    //State Lookup
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetStateDataForLookup";
    options.registerBtn = $("#StateLookup");
    options.focusItem = $("#State");
    options.param = "";
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var CntryCd = $("#CntryCd").val();

        if(CntryCd != "")
        {
            return " CNTRY_CD='" + CntryCd + "'"; 
        }
        else
        {
            return "";
        }
    }
    options.gridFunc = function (map) {
        $("#State").val(map.StateCd);
        $("#Region").val(map.RegionCd);
    }

    options.lookUpConfig = LookUpConfig.StateLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#State", 1, "", "dt=state&GROUP_ID=" + groupId + "&STATE_CD=", "STATE_CD=showValue,STATE_CD,STATE_NM,REGION_CD", function (event, ui) {
        var map = ui.item.returnValue;
        $(this).val(ui.item.returnValue.STATE_CD);
        $("#Region").val(ui.item.returnValue.REGION_CD);
        return false;
    }, function(){
        var CntryCd = $("#CntryCd").val();

        if(CntryCd != "")
        {
            return " CNTRY_CD=" + CntryCd; 
        }
        else
        {
            return "";
        }
    });

    commonSetBscData("RegionLookup", "Region", "", "TRGN", null);
    commonBscAuto("RegionLookup", "Region", "", "TRGN", null);

});
