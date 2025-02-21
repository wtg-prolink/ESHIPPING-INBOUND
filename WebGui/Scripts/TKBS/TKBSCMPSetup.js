var url = "";
var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var editMode = "";
function initLoadData(CmpId) {
    if (!CmpId)
        return;

    var param = "sopt_CmpId=eq&CmpId=" + CmpId;
    $.ajax({
        async: true,
        url: rootPath + "TKBS/TKCMPDetailData",
        type: 'POST',
        data: {
            sidx: 'CmpId',
            'conditions': encodeURI(param),
            page: 1,
            rows: 500
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
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            _dataSource = maindata.rows;
            setFieldValue(maindata.rows);

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
    
    initLoadData(CmpId);
});

function MenuFunc()
{
    MenuBarFuncArr.DelMenu(["MBSearch", "MBErrMsg", "MBInvalid", "MBEdoc", "MBApprove"]);


    MenuBarFuncArr.MBAdd = function () {
        $("#Cmp").val(cmp);
        $("#Stn").val(stn);
    }

    MenuBarFuncArr.MBCopy = function () {
        $("#CmpId").val("");
        $("#CmpName").val("");
    }

    MenuBarFuncArr.MBEdit = function () {

    }

    MenuBarFuncArr.MBCancel = function () {

    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getAllKeyValue();
        $.ajax({
            async: true,
            url: rootPath + "TKBS/TKCMPUpdateData",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false
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

        var changeData = getChangeValue();
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "TKBS/TKCMPUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
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

                dtd.resolve();
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
    options.gridUrl = rootPath + "Common/GetPartyTypeData";
    options.registerBtn = $("#PartyTypeLookup");
    options.focusItem = $("#PartyType");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.Cd,
            CdDescp = map.CdDescp;
        $("#PartyType").val(Cd);
    }
    options.param = "";
    options.lookUpConfig = LookUpConfig.PartyTypeLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#PartyType", 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CMP=" + cmp + "&STN=" + stn + "&CD_TYPE~PT&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);
        return false;
    });

    options = {};
    options.gridUrl = rootPath + "Common/GetCityPortData";
    options.registerBtn = $("#CityLookup");
    options.focusItem = $("#City");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        var Cd = map.PortCd;
        $("#City").val(Cd);
    }

    options.lookUpConfig = LookUpConfig.BSCITYLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#City", 1, "", "dt=port&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD", function (event, ui) {
        $(this).val(ui.item.returnValue.PORT_CD);
        return false;
    });
}


