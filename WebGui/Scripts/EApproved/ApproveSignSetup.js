var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};

var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;

    var CmpOption = {};
    CmpOption.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOption.param = "";
    CmpOption.baseCondition = " GROUP_ID='" + groupId + "' AND TYPE='1'";
    CmpOption.gridReturnFunc = function (map) {
        var $ExpressSetupGrid = $("#ExpressSetupGrid");
        var selRowId = $ExpressSetupGrid.jqGrid('getGridParam', 'selrow');
        var value = map.Cd;
        setGridVal($("#ExpressSetupGrid"), selRowId, "CmpName", map.CdDescp, null);
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
    CmpOption.autoCompDt = "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=";
    CmpOption.autoCompParams = "CMP=showValue,CMP,NAME";
    CmpOption.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CMP);
        var $ExpressSetupGrid = $("#ExpressSetupGrid");
        var selRowId = $ExpressSetupGrid.jqGrid('getGridParam', 'selrow');
        setGridVal($("#ExpressSetupGrid"), selRowId, "CmpName", ui.item.returnValue.NAME, null);
        //$ExpressSetupGrid.jqGrid('setCell', selRowId, "CmpName", ui.item.returnValue.CdDescp, 'edit-cell dirty-cell');
    }

    //ExpressSetupGrid
    function getuserop(_expreasongrid, _$expreasongrid, name) {
        var _name = name;
        var user_op = getLookupOp(_expreasongrid,
            {
                url: rootPath + "/System/UserSetInquiryData",
                config: LookUpConfig.MultiUserLookup,
                isMutiSel: true,
                focusColumnID: _name,
                columnID: "UId"
            }, {
                responseMethod: function (data) {
                    var selRowId = $("#" + _expreasongrid).jqGrid('getGridParam', 'selrow');
                    var str = "";
                    $.each(data, function (index, val) {
                        str = str + data[index].UId + ";";
                    });
                    setGridVal($("#" + _expreasongrid), selRowId, _name, str, "lookup");
                    return str;
                }
            }, function (_$expreasongrid, elem, rowid) {
            });
        // user_op.param = "";
        return user_op;
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

    function getLookupOp(gridId, op, op1, op2) {
        var $grid = $("#" + gridId);
        var opt = {};
        opt.gridUrl = op.url;
        opt.selfSite = false;
        opt.gridReturnFunc = function (map) {
            if (op.returnFn)
                return op.returnFn(map, $grid);
        };
        opt.lookUpConfig = op.config;
        opt.gridId = gridId;

        //自动带入
        opt.autoCompKeyinNum = 1;
        opt.autoCompUrl = "";
        opt.isMutiSel = op.isMutiSel;
        opt.focusColumnID = op.focusColumnID;
        opt.columnID = op.columnID;
        //opt.autoCompGetValueFunc = op1.autoCompGetValueFunc;
        opt = $.extend(opt, op1);
        opt = $.extend(opt, op2);
        return opt;
    }

    $grid = $("#ExpressSetupGrid");

    var colModel = [
        { name: 'UId', title: 'UId', index: 'UId', width: 70, sorttype: 'string', hidden: true },
        { name: 'Cmp', title: _getLang("L_MailGroupSetup_Cmp", "Cmp"), index: 'Cmp', width: 120, align: 'left', sorttype: 'string', editable: true, hidden: false, edittype: 'custom', editoptions: gridLookup(CmpOption) },
        { name: 'CmpName', title: _getLang("L_MailGroupSetup_Name", "Company Name"), index: 'CmpName', width: 150, align: 'left', sorttype: 'string', editable: false, hidden: false },
        { name: 'ProductType', title: _getLang("L_DRuleSetup_Views_237", "产品别"), index: 'ProductType', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false },
        { name: 'SignId', title: _getLang("L_Sign_User", "会签人员"), index: 'SignId', width: 150, align: 'left', sorttype: 'string', editable: true, hidden: false, edittype: 'custom', editoptions: gridLookup(getuserop("ExpressSetupGrid", $grid, "SignId")) },
        { name: 'SignType', title: _getLang("L_Sign_Type", "会签类型"), index: 'SignType', width: 200, align: 'left', sorttype: 'string', editable: true, hidden: false, formatter: "select", edittype: 'select', editoptions: { value: _getLang("L_Sing_TypeSelect", "A:50层级会签;B:55层级会签") } },
        { name: 'SignOrder', title: _getLang("SignOrder", "Sign Order"), index: 'SignOrder', width: 80, align: 'left', sorttype: 'string', editable: true, hidden: false }
        ];

    _dm.addDs("ExpressSetupGrid", [], ["Cmp", "UId"], $grid[0]);
    new genGrid(
        $grid,
        {
            //data: [],
            //loadonce: true,
            colModel: colModel,
            datatype: "json",
            url: rootPath + "EApproved/ApproveSignQuery", 
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
            caption: _getLang("L_Sign_Type_SETUP", "账单会签人员设定"),
            height: gridHeight,
            rownumWidth: 50,
            refresh: true,
            rows: 9999,
            exportexcel: false,
            pginput: false,
            pgbuttons: false,
            ds: _dm.getDs("ExpressSetupGrid"),
            sortable: false,
            sortorder: "asc",
            sortname: "Cmp,SignType,SignOrder",
            delKey: "UId",
            beforeSelectRowFunc: function (rowid) {
                //main key 修改時不允與修改
                if (rowid != null && rowid.indexOf("jqg") >= 0) {
                    $("#ExpressSetupGrid").setColProp('Cmp', { editable: true });
                } else {
                    $("#ExpressSetupGrid").setColProp('Cmp', { editable: false });
                }
            },
            onAddRowFunc: function (rowid) {

            },
            beforeAddRowFunc: function (rowid) {
                //add row 時要可以編輯main key
                $("#ExpressSetupGrid").setColProp('Cmp', { editable: true });
                $("#ExpressSetupGrid").setColProp('CmpName', { editable: false });
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
            url: rootPath + "EApproved/ApproveSignUpdate",
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
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning"); 
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success"); 
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

