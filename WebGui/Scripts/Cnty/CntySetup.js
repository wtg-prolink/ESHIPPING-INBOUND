/*var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var editable = false;

    //All column：StsCd,Edescp,Ldescp,Pict1,Pict2,Location,Issingle,RefBy,CreateBy,
    //CreateDate, ModifyBy, ModifyDate

    //Key：StsCd
    var docHeight = $(document).height();
    gridHeight = docHeight - 150;
    var colModel = [
             { name: 'CntryCd', title: '@Resources.Locale.L_CitySetup_CntryCd', index: 'CntryCd', width: 120, sorttype: 'string', classes: "uppercase", editable: false },
             { name: 'CntryNm', title: '@Resources.Locale.L_CntySetup_CntryNm', index: 'CntryNm', width: 150, sorttype: 'string', classes: "uppercase", editable: true },
             { name: 'ShippingInstruction', title: '@Resources.Locale.L_CntySetup_ShippingInstruction', index: 'ShippingInstruction', width: 380, sorttype: 'string', classes: "uppercase", editable: true },
             { name: 'GroupId', title: '@Resources.Locale.L_UserSetUp_GroupId', index: 'GroupId', width: 180, sorttype: 'string', editable: true, hidden: true },
             { name: 'Cmp', title: '@Resources.Locale.L_PartyDocSetup_Cmp', index: 'Cmp', width: 180, sorttype: 'string', editable: true, hidden: true },
             { name: 'Stn', title: '@Resources.Locale.L_IpPart_MafNo', index: 'Stn', width: 200, sorttype: 'string', editable: true, hidden: true }
    ];


    $grid = $("#CntyGrid");
    _dm.addDs("CntyGrid", [], ["CntryCd"], $grid[0]);
    new genGrid(
    	$grid,
    	{
    	    data: [],
    	    loadonce: true,
    	    colModel: colModel,
    	    datatype: "json",
    	    url: rootPath + "SYSTEM/CntyQuery",
    	    cellEdit: false,//禁用grid编辑功能
    	    caption: "国家代码建档",
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 9999,
    	    rownumWidth: 50,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    ds: _dm.getDs("CntyGrid"),
    	    sortorder: "asc",
    	    sortname: "CreateDate",
    	    delKey: ["CntryCd","GroupId","Cmp","Stn"],
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#CntyGrid").setColProp('CntryCd', { editable: true });
    	        } else {
    	            $("#CntyGrid").setColProp('CntryCd', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {

    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#CntyGrid").setColProp('CntryCd', { editable: true });
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "CntyGrid" });
        editable = false;
    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "CntyGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {

        editable = false;
        var containerArray = $('#CntyGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/CntyUpdate",
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
                    CommonFunc.Notify("", "保存失败", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "保存成功", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                //location.reload();
                gridEditableCtrl({ editable: false, gridId: "CntyGrid" });
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

*/

var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
_handler.key = "CntryCd";
$(function(){

    _handler.saveUrl = rootPath + "System/CntyUpdate";
    _handler.inquiryUrl = rootPath + "System/GetDetail";
    _handler.config = [];

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        var groupId = getCookie("plv3.passport.groupid"),
        cmp = getCookie("plv3.passport.companyid");
        //$("#GroupId").val(groupId);
        //$("#Cmp").val(cmp)
        data["GroupId"] = encodeURIComponent(groupId);
        data["Cmp"] = encodeURIComponent(cmp);
        data["Stn"] = encodeURIComponent("*");
        setFieldValue([data]);
    }


    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["CntryCd"] = encodeURIComponent($("#CntryCd").val());
        data["gid"] = encodeURIComponent($("#GroupId").val());
        data["cid"] = encodeURIComponent($("#Cmp").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.beforAdd = function () {//新增前设定
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _handler.loadMainData = function (map) {
        if (!_uid) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "System/CntyGetDetail", { uId: map.UId, gid:gid, cid:cid, CntryCd: _uid, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
        });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        var dataRow, addData = [];
    }

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBEdoc", "MBSearch"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

});
