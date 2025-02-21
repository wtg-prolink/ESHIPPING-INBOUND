var _dm = new dm();
var url = "";//  rootPath + "/Shipment/InquiryBootstrapData";
jQuery(document).ready(function ($) {
    url = rootPath + "System/RoleSetInquiryData";

    MenuBarFuncArr.DelMenu(["MBApply", "MBInvalid"]);

    //集團放大鏡
    var GroupOpt = {};
    GroupOpt.gridUrl = rootPath + "TPVCommon/GetSiteGroupData";
    GroupOpt.param = "";
    GroupOpt.baseCondition = " TYPE='0'";
    GroupOpt.registerBtn = $("#groupIdLookup");
    GroupOpt.focusItem = $("#GroupId");
    GroupOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#GroupId").val(value);
    };
    GroupOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(GroupOpt);
    CommonFunc.AutoComplete("#GroupId", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });

    //公司放大鏡
    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#Cmp");
    CmpOpt.baseConditionFunc = function () {
        var GroupId = $("#GroupId").val();

        if(GroupId == "")
        {
            alert(_getLang("L_UserSetUp_PlSelectGroup", '请先选择集团'));
            return "";
        }
        else
        {
            return " AND GROUP_ID = '"+GroupId+"'";
        }
    }
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Cmp").val(value);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
   /* CommonFunc.AutoComplete("#Cmp", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        return false;
    });
    */
    MenuBarFuncArr.MBDel = function () {

        var changeData = getAllKeyValue();

        $.ajax({
            async: true,
            url: rootPath + "System/CheckRoleRelation",
            type: 'POST',
            data: { "fid": $("#Fid").val() },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_BSCSSetup_DelFail", '删除失败'), 500, "warning");
            },
            success: function (result) {
                if (result.message != "success")
                {
                    var r = confirm(_getLang("L_RoleSetUp_Confirm", '确定删除此角色？系统侦测到此角色拥有用户，若删除将连动取消关连！'));
                    if (r != true) {
                        return;
                    }
                }

                $.ajax({
                    async: true,
                    url: rootPath + "System/RoleSetUpdateData",
                    type: 'POST',
                    data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
                    dataType: "json",
                    "complete": function (xmlHttpRequest, successMsg) {
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", _getLang("L_BSCSSetup_DelFail", '删除失败'), 500, "warning");
                    },
                    success: function (result) {
                        if (result.message != "success") {
                            //notice ajax warning 一定要放入下面三行
                            CommonFunc.Notify("", _getLang("L_BSCSSetup_DelFail", '删除失败') + result.message, 500, "warning");
                            return;
                        }
                        //成功后将页面的数据移除，并设置页面不可编辑 
                        setFieldValue(undefined, "");
                        setdisabled(true);
                        setToolBtnDisabled(false);
                        CommonFunc.Notify("", _getLang("L_BSCSSetup_DelS", '删除成功'), 500, "success");
                    }
                });
            }
        });   
    }

    var RoleSearchOption = {};
    RoleSearchOption.param = "";
    RoleSearchOption.baseCondition = " GROUP_ID='"+groupId+"' AND (CMP='"+cmp+"' OR CMP='*') AND (STN='"+stn+"' OR STN='*')";
    RoleSearchOption.gridUrl = url;
    RoleSearchOption.registerBtn = $("#MBSearch");
    RoleSearchOption.isMutiSel = true;
    RoleSearchOption.gridFunc = function (map) {
        _dataSource = map;
        map = [map];
        setFieldValue(map);
        MenuBarFuncArr.Enabled(["MBRoleLog"]);
    }
    RoleSearchOption.responseMethod = function () { }
    RoleSearchOption.lookUpConfig = LookUpConfig.RoleLookup;
    MenuBarFuncArr.MBSearch = function (thisItem) {
        initLookUp(RoleSearchOption);
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        editable = true;

        $("#Fid").val("");
        $("#Fdescp").val("");
    }

    MenuBarFuncArr.MBAdd = function () {
        $("#GroupId").val(groupId);
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        if (checkNoAllowNullFields() == false)
            return false;
        var changeData = getChangeValue();
        //var changeData = getChangeValue();
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "System/RoleSetUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success")
                {
                    //notice ajax warning 一定要放入下面三行
                    CommonFunc.Notify("", result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();

                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", '保存成功'), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Enabled(["MBRoleLog"]);
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    var RoleLogQueryurl = rootPath + "/System/GetRoleAuthorityLog";
    $RoleLogDialogGrid = $("#RoleLogDialogGrid");

    var RoleLogcolModel = [
        { name: 'RoleAdd', title: _getLang("L_UserSetUp_RoleAdd", "增加角色"), index: 'RoleAdd', width: 180, align: 'left', sorttype: 'string', editable: false },
        { name: 'RoleDel', title: _getLang("L_UserSetUp_RoleDel", "移除角色"), index: 'RoleDel', width: 180, align: 'left', sorttype: 'string', editable: false },
        { name: 'CreateBy', title: _getLang("L_Bsdate_ModifyBy", ""), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', editable: false },
        {
            name: 'CreateDate', title: _getLang("L_Bsdate_ModifyDate", ""), index: 'CreateDate',
            width: 150, align: 'left', sorttype: 'string',
            hidden: false, editable: true, formatter: 'date',
            editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d h:i',
                defaultValue: null
            }

        }
    ];
    new genGrid($RoleLogDialogGrid, {
        datatype: "json",
        loadonce: true,
        cellEdit: false,//禁用grid编辑功能
        colModel: RoleLogcolModel,
        delKey: [""],
        url: RoleLogQueryurl,
        postData: null,
        ds: _dm.getDs("SubGrid"),
        caption: _getLang("TLB_PermissionLog", "角色权限修改Log"),
        sortorder: "Desc",
        sortname: "CreateDate",
        height: "auto",
        refresh: true,
        pginput: false,
        sortable: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        onAddRowFunc: function (rowid) {
        }
    });

    MenuBarFuncArr.AddMenu("MBRoleLog", "glyphicon glyphicon-repeat", _getLang("TLB_PermissionLog", "角色权限修改Log"), function () {
        var uid = $("#Fid").val();
        if (uid == "") {
            CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "请先选择一笔"), 500, "warning");
            return false;
        }
        var cmp = $("#Cmp").val();
        var gridheight = $(window).height() - 300;
        $RoleLogDialogGrid.setGridHeight(gridheight);
        $RoleLogDialogGrid.jqGrid('setGridParam', {
            url: RoleLogQueryurl, datatype: "json",
            postData: { UId: uid, Cmp: cmp },
            loadCompleteFunc: function () {
                $("#RoleLogDetail").modal("show"); //顯示彈出視窗
                ajustamodal("#RoleLogDetail");
            }
        }).trigger("reloadGrid", [{ page: 1 }]);
    });

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBRoleLog"]);
    }
    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApply", "MBApprove", "MBInvalid"]);
    setdisabled(true);
    setToolBtnDisabled(true);
    initMenuBar(MenuBarFuncArr);

    $("#Fid").on("change", function(){
        var val = $(this).val();
        val = val.replace(new RegExp('[\u4e00-\u9fa5]'), '');
        $(this).val(val.toUpperCase());
    });
    
});

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}