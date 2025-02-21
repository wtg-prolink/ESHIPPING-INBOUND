var $SubGrid = $("#SubGrid");
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
            url: rootPath + "SMEXACCT/SmexacctUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val()},
            dataType: "json",
            error: function (xmlHttpRequest, errMsg) {
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
            url: rootPath + "SMEXACCT/SmexacctUpdateData",
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
        $("#CmpNm").val(map.CdDescp);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        $("#CmpNm").val(ui.item.returnValue.NAME);
        return false;
    });    

    setSmptyData("ExpressCdLookup", "ExpressCd", "ExpressNm", "");
}

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

function setBscData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.baseCondition = " GROUP_ID='"+groupId+"' AND CD_TYPE='"+pType+"'";
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

function setCityData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBsCityDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.CntryCd + map.PortCd);

        if(Nm != "")
            $("#" + Nm).val(map.PortNm);
    }

    options.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=port&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD=", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PORT_NM);

        return false;
    });
}

function initLoadData(Uid)
{
    if (!Uid)
    {
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "SMEXACCT/GetSmexacctDetail",
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