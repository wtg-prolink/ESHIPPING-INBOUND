//var url = "";
var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var editMode = "";
function initLoadData(UId) {
    if (!UId)
        return;

    var param = "sopt_UId=eq&UId=" + UId;
    $.ajax({
        async: true,
        url: rootPath + "System/MailFormatDetailData",
        type: 'POST',
        data: {
            'UId': UId
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
            console.log(result.mainTable);
            maindata = result.mainTable //jQuery.parseJSON(result.mainTable);
            if (maindata.length < 0) {
                return false;
            }
            setFieldValue(maindata);
            var MtContent = maindata[0].MtContent;
            console.log(MtContent);
            CKEDITOR.instances['MtContent'].setData(MtContent);

            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy"]);

            CommonFunc.ToogleLoading(false);
        }
    });
}

jQuery(document).ready(function($) {

    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);

    setdisabled(true);

    MenuFunc(); //MenuBar控制

    BindLookup(); //所有放大鏡

    if(UId)
    {
        MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
        MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy"]);
    }
    
    //initLoadData(UId);
});

function MenuFunc()
{
    MenuBarFuncArr.DelMenu(["MBSearch", "MBErrMsg", "MBInvalid", "MBEdoc", "MBEapprove"]);


    MenuBarFuncArr.MBAdd = function (thisItem) {
        editMode = "A";
        CKEDITOR.instances['MtContent'].setReadOnly(false);
        CKEDITOR.instances['MtContent'].setData('');
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        editMode = "A";
        UId = "";
        CKEDITOR.instances['MtContent'].setReadOnly(false);
    }

    MenuBarFuncArr.MBEdit = function () {
        editMode = "E";
        CKEDITOR.instances['MtContent'].setReadOnly(false);

    }

    MenuBarFuncArr.MBCancel = function () {
        CKEDITOR.instances['MtContent'].setReadOnly(true);
        if (UId) {
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy"]);
        }
    }

    MenuBarFuncArr.MBDel = function () {
        editMode = "D";

        $.ajax({
            async: true,
            url: rootPath + "System/MailFormatUpdateData",
            type: 'POST',
            data: {
                "UId": UId,
                "editMode": editMode
            },
            dataType: "json",
            success: function (result) {
                if (result.message != "success")
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
                    return null;
                }
                else
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                    setFieldValue(undefined, "");
                    setdisabled(true);
                    setToolBtnDisabled(false);
                    MenuBarFuncArr.Enabled(["MBSummary"]);
                    CKEDITOR.instances['MtContent'].setReadOnly(true);
                    CKEDITOR.instances['MtContent'].setData('');
                    UId = "";
                }
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
            }
        });
        
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        if (!checkNoAllowNullFields()) {
            dtd.resolve();
            return false;
        }

        $.ajax({
            async: true,
            url: rootPath + "System/MailFormatUpdateData",
            type: 'POST',
            data: {
                "UId": UId,
                "editMode": editMode,
                "MtType": $("#MtType").val(),
                "MtName": $("#MtName").val(),
                "MtContent": CKEDITOR.instances.MtContent.getData()
            },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    dtd.resolve();
                    return false;
                }
                setdisabled(true);
                setToolBtnDisabled(true);

                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                CKEDITOR.instances['MtContent'].setReadOnly(true);
                UId = result.UId;
                dtd.resolve();
                initLoadData(UId);
            }
        });
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
}

function BindLookup()
{
    //Party Type放大镜
    options = {};
    options.gridUrl = rootPath + "Common/GetMailTypeData";
    options.registerBtn = $("#MtTypeLookup");
    options.focusItem = $("#MtType");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd,
            CdDescp = map.CdDescp;
        $("#MtType").val(Cd);
        $("#MtName").val(CdDescp);
    }
    options.param = "";
    options.lookUpConfig = LookUpConfig.MailTypeLookup;
    initLookUp(options);

    //CommonFunc.AutoComplete("#MtType", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN=" + stn + "&CD_TYPE~MT&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
    CommonFunc.AutoComplete("#MtType", 1, "", "dt=bsc&CD_TYPE~MT&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        //set return value to other field
        $("#MtName").val(ui.item.returnValue.CD_DESCP);
        //set return to register field 
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    var lookUp = "CmpLookup";
    var pType = "LC";
    var Cd = "Cmp";
    var Nm = "";

    /*options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
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
    });*/

    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#Cmp");
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Cmp").val(value);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#Cmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        return false;
    });
}


