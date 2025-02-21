var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var subStatus = "";
var mainStatus = "";
var gridHeight = 0;

$(function () {   
    Schemas = JSON.parse(decodeHtml(Schemas));
    CommonFunc.initField(Schemas, "mt");

    setdisabled(true);
    setSTdisabled(true);
    
    _initMainMenu();
    _initLookup();
    initLoadData(_uid);

    $("#Io").on("change", function(){
        var val = $(this).val();
        $("#PolDescp").val("");
        $("#Pol1").val("");
        $("#Pol2").val("");
        if(val == "I")
        {
            $("button.ii").show();
            $("button.oo").hide();

            $("#Pol1").hide();
            $("#Pol2").show();
        }
        else
        {
            $("button.ii").hide();
            $("button.oo").show();

            $("#Pol1").show();
            $("#Pol2").hide();
        }
    });
    
});

function _initMainMenu()
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
        //var SubGridChageArray = localDB.getChangeData("SMSID");

        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }

        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "BSLCPOL/BslcpolUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val(), Cmp:$("#Cmp").val(), Pol: $("#Pol").val()},
            dataType: "json",
            error: function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", result.message, 500, "warning");
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
            url: rootPath + "BSLCPOL/BslcpolUpdateData",
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
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    //dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
            }
        });
        
    }

    initMenuBar(MenuBarFuncArr);

}

function _initLookup()
{
    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#Cmp");
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Cmp").val(value);
        //$("#CmpNm").val(map.CdDescp);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        //$("#CmpNm").val(ui.item.returnValue.NAME);
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
        $("#Pol1").val(value);
        $("#PolDescp").val(map.PortNm);
        return value;
    }
    PodOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(PodOptions);
    CommonFunc.AutoComplete("#Pol1", 1, "", "dt=port1&GROUP_ID=" + groupId + "&PORT_CD%", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $("#Pol").val(ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD);
        $("#Pol1").val(ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD);
        $("#PolDescp").val(ui.item.returnValue.PORT_NM);
        return false;
    });

    //城市港口放大鏡
    var PodOptions = {};
    PodOptions.gridUrl = rootPath + LookUpConfig.TruckPortCdUrl;
    PodOptions.registerBtn = $("#oPolLookup");
    PodOptions.focusItem = $("#Pol");
    PodOptions.isMutiSel = true;
    PodOptions.param = "";
    PodOptions.gridFunc = function (map) {
        var value = map.PortCd;
        $("#Pol").val(value);
        $("#Pol2").val(value);
        $("#PolDescp").val(map.PortNm);
        return value;
    }
    PodOptions.lookUpConfig = LookUpConfig.TruckPortCdLookup;
    initLookUp(PodOptions);

    CommonFunc.AutoComplete("#Pol2", 1, "", "dt=bstport&GROUP_ID=" + groupId + "&PORT_CD%","REGION&STATE&PORT_NM&PORT_CD=showValue,PORT_CD,PORT_NM,STATE,REGION", function (event, ui) {
        $("#Pol").val(ui.item.returnValue.PORT_CD);
        $("#Pol2").val(ui.item.returnValue.PORT_CD);
        $("#PolDescp").val(ui.item.returnValue.PORT_NM);
        return false;
    });


    //setSmptyData("ExpressCdLookup", "ExpressCd", "ExpressNm", "");
}


function initLoadData(Uid)
{
    if (!Uid)
    {
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "BSLCPOL/GetBslcpolDetail",
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

            console.log(maindata);
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            
            CommonFunc.ToogleLoading(false);
        }
    });
}