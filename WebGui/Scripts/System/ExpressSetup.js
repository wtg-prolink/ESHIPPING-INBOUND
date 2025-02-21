var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
//var groupId = getCookie("plv3.passport.groupid"),
//    cmp = getCookie("plv3.passport.companyid"),
//    stn = getCookie("plv3.passport.station"),
//    userId = getCookie("plv3.passport.user");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;

    var CmpOption = {};
    CmpOption.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOption.param = "";
    CmpOption.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOption.gridReturnFunc = function (map) {
        var $ExpressSetupGrid = $("#ExpressSetupGrid");
        var selRowId = $ExpressSetupGrid.jqGrid('getGridParam', 'selrow');
        var value = map.Cd;
        setGridVal($("#ExpressSetupGrid"), selRowId, "CmpNm", map.CdDescp, null);
        return value;
    };
    
    CmpOption.registerBtn = $("#CmpLookup");
    CmpOption.focusItem = $("#Cmp");
    CmpOption.gridFunc = function (map) {
        var value = map.Cd;
        $("#Cmp").val(value);
    };
    CmpOption.lookUpConfig = LookUpConfig.SiteLookup;
    CmpOption.autoCompKeyinNum = 1;
    CmpOption.gridId = "ExpressSetupGrid";
    CmpOption.autoCompUrl = "";
    CmpOption.autoCompDt ="dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=";
    CmpOption.autoCompParams = "CMP=showValue,CMP,NAME";
    CmpOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CMP);
        var $ExpressSetupGrid = $("#ExpressSetupGrid");
        var selRowId = $ExpressSetupGrid.jqGrid('getGridParam', 'selrow');
        setGridVal($("#ExpressSetupGrid"), selRowId, "CmpNm", ui.item.returnValue.NAME, null);
        //$ExpressSetupGrid.jqGrid('setCell', selRowId, "CmpNm", ui.item.returnValue.CdDescp, 'edit-cell dirty-cell');
    }
    

    var ExpressOption = {};
    ExpressOption.gridUrl = rootPath + "TPVCommon/GetGroupData";
    ExpressOption.param = '';//"sopt_PartyType=eq&PartyType LIKE '%EX%'" + "&sopt_GroupId=eq&GroupId=" + groupId;
    ExpressOption.gridReturnFunc = function (map) {
        var value = map.PartyNo;
        return value;
    };
    ExpressOption.lookUpConfig = LookUpConfig.ExpressLookup;
    ExpressOption.autoCompKeyinNum = 1;
    ExpressOption.gridId = "ExpressSetupGrid";
    ExpressOption.autoCompUrl = "";
    ExpressOption.autoCompDt = "dt=smpty&PARTY_NO=";
    ExpressOption.autoCompParams = "PARTY_NO=showValue,PARTY_NO";
    ExpressOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.PARTY_NO);
    }

    var funcOpt = {};
    funcOpt.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    funcOpt.param = "G";
    funcOpt.baseCondition = " CD_TYPE='TEDI'";
    funcOpt.gridReturnFunc = function (map) {
        var value = map.Cd
        return value;
    }
    funcOpt.selfSite = true;
    funcOpt.lookUpConfig = LookUpConfig.BSCodeLookup;
    funcOpt.autoCompKeyinNum = 1;
    funcOpt.gridId = "ExpressSetupGrid";
    funcOpt.autoCompUrl = "";
    funcOpt.autoCompDt = "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~TEDI&CD=";
    funcOpt.autoCompParams = "CD=showValue,CD,CD_DESCP";
    funcOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CD);
    }


    var colModel = [
        { name: 'UId', title: 'UId', index: 'UId', width: 70, sorttype: 'string', hidden: true },
        { name: 'Cmp', title: "@Resources.Locale.L_MailGroupSetup_Cmp", index: 'Cmp', width: 120, align: 'left', sorttype: 'string', editable: true, hidden: false, edittype: 'custom', editoptions: gridLookup(CmpOption) },
        { name: 'CmpNm', title: "@Resources.Locale.L_MailGroupSetup_Name", index: 'CmpNm', width: 150, align: 'left', sorttype: 'string', editable: false, hidden: false },
        { name: 'Express', title: "@Resources.Locale.L_ExpressSetup_Express", index: 'Express', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false, edittype: 'custom', editoptions: gridLookup(ExpressOption) },
        { name: 'ExNo', title: "@Resources.Locale.L_ExpressSetup_ExNo", index: 'ExNo', width: 100, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'PwId', title: "@Resources.Locale.L_ExpressSetup_PwId", index: 'PwId', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'WebUrl', title: "URL", index: 'WebUrl', width: 300, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'SendId', title: "Send ID", index: 'SendId', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'RcvId', title: "Receive id", index: 'RcvId', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'EdiMode', title: "EDI Mode", index: 'EdiMode', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false,edittype: 'custom', editoptions: gridLookup(funcOpt)  },
        { name: 'MsgCode', title: "Message Code", index: 'MsgCode', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'Remark', title: "@Resources.Locale.L_BSCSSetup_Remark", index: 'Remark', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'AuthoriZation', title: "Authorization", index: 'AuthoriZation', width: 300, align: 'left', sorttype: 'string', editable: true, hidden: false }
    ];


    $grid = $("#ExpressSetupGrid"); _dm.addDs("ExpressSetupGrid", [], ["Cmp", "ExNo", "ExpressType"], $grid[0]);
    new initGrid(
        $grid,
        {
            data: [],
            colModel: colModel
        },
        {
            datatype: "json",
            url: rootPath + "SYSTEM/ExpressQuery",
            loadonce: true,
            cellEdit: false,//禁用grid编辑功能
            caption: "@Resources.Locale.L_ExpressSetup_ExpressQuery",
            height: gridHeight,
            refresh: true,
            rows: 9999,
            exportexcel: false,
            pginput: false,
            pgbuttons: false,
            sortorder: "asc",
            sortname: "CreateDate",
            delKey: "UId",
            savelayout: true,
            showcolumns: true,
            footerrow: true,
            ds: _dm.getDs("ExpressSetupGrid"),
            dblClickFunc: function (map) {
            },
            beforeSelectRowFunc: function (rowid) {
                //main key 修改時不允與修改
                if (rowid != null && rowid.indexOf("jqg") >= 0) {
                    $("#ExpressSetupGrid").setColProp('Cmp', { editable: true });
                    $("#ExpressSetupGrid").setColProp('ExNo', { editable: true });
                } else {
                    $("#ExpressSetupGrid").setColProp('Cmp', { editable: false });
                    $("#ExpressSetupGrid").setColProp('ExNo', { editable: false });
                }
            },
            onAddRowFunc: function (rowid) {

            },
            beforeAddRowFunc: function (rowid) {
                //add row 時要可以編輯main key
                $("#ExpressSetupGrid").setColProp('Cmp', { editable: true });
                $("#ExpressSetupGrid").setColProp('CmpNm', { editable: false });
                $("#ExpressSetupGrid").setColProp('Express', { editable: true });
                $("#ExpressSetupGrid").setColProp('ExNo', { editable: true });
                $("#ExpressSetupGrid").setColProp('PwId', { editable: true });
                $("#ExpressSetupGrid").setColProp('SendId', { editable: true });
                $("#ExpressSetupGrid").setColProp('RcvId', { editable: true });
                $("#ExpressSetupGrid").setColProp('Remark', { editable: true });
            }
        }
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "ExpressSetupGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "ExpressSetupGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#ExpressSetupGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/ExpressUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
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

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                //location.reload();
                gridEditableCtrl({ editable: false, gridId: "ExpressSetupGrid" });
                editable = false;

            }
        });
        return dtd.promise();
    }


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

