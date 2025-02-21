var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var _uid = "";
var groupId = getCookie("plv3.passport.groupid"),
    _cmp = getCookie("plv3.passport.companyid");

jQuery(document).ready(function ($) {
    $("#Cmp").val(_cmp);
    $("#CmpNm").val(CmpNm);
    var $CostAllocationGrid = $("#CostAllocationGrid");
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 260;

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', width: 100, sorttype: 'string', hidden: true },
        { name: 'Cmp', title: 'location', index: 'Cmp', width: 100, sorttype: 'string', editable: false },
        { name: 'ByCbm', title: 'By CBM', index: 'ByCbm', sorttype: 'int', editrules: { integer: true, minValue: 1 }, editoptions: { size: 10, maxlength: 4 }, width: 100, hidden: false, editable: true },
        { name: 'ByGw', title: 'By Gross Weight', index: 'ByGw', sorttype: 'int', editrules: { integer: true, minValue: 1 }, editoptions: { size: 10, maxlength: 4 }, width: 100, hidden: false, editable: true },
        { name: 'ByNw', title: 'By Net Weight', index: 'ByNw', sorttype: 'int', editrules: { integer: true, minValue: 1 }, editoptions: { size: 10, maxlength: 4 }, width: 100, hidden: false, editable: true },
        { name: 'ByValue', title: 'By Cargo Value', index: 'ByValue', sorttype: 'int', editrules: { integer: true, minValue: 1 }, editoptions: { size: 10, maxlength: 4 }, width: 100, hidden: false, editable: true },
        { name: 'Remark', title: 'Remark', index: 'Remark', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'CreateBy', title: _getLang("L_Bsdate_CreateBy", "创建人"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CreateDate', title: _getLang("L_Bsdate_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', sorttype: 'date', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } },
        { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", "修改人"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } }
    ];
    _handler.saveUrl = rootPath + "System/CostAllocationRuleUpdate";
    _handler.inquiryUrl = rootPath + "System/CostAllocationRuleQuery";
    _handler.search = function () {
    };

    _handler.afterEdit = function () {
        $('#Cmp').attr("disabled", true);
        $('#CmpLookup').attr("disabled", true);
        //$('#areaSelect').attr("disabled", "disabled");
        //$("#CmpLookup").Disabled();
    }

    _handler.intiGrid("CostAllocationGrid", $CostAllocationGrid, {
        colModel: colModel, caption: 'Cost Allocation Rule', delKey: ["UId", "Cmp"],
        onAddRowFunc: function (rowid) {
            var cmpvalue = $("#Cmp").val();
            var cmpnmvalue = $("#CmpNm").val();
            $CostAllocationGrid.jqGrid('setCell', rowid, "Cmp", cmpvalue);
            $CostAllocationGrid.jqGrid('setCell', rowid, "CmpNm", cmpnmvalue);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$CostAllocationGrid.setColProp('Cmp', { editable: true });
            //$CostAllocationGrid.setColProp('CmpNm', { editable: true });
        }
    });

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.loadGridData("CostAllocationGrid", $CostAllocationGrid[0], data["main"], [""]);
        else
            _handler.loadGridData("CostAllocationGrid", $CostAllocationGrid[0], [], [""]);
        //setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        $('#Cmp').attr("disabled", false);
        $('#CmpLookup').attr("disabled", false);
    }


    _handler.saveData = function (dtd) {
        var containerArray = $('#CostAllocationGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;

        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "Cmp": _cmp, autoReturnData: false },
            function (result) {
                _handler.topData = { UId: result.Cmp };
                //_topData = keyData["mt"];
                if (result.message != 'success') {
                    alert(result.message);
                    return false;
                }

                MenuBarFuncArr.MBCancel();
                return true;
            });
    }

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            _cmp = map.Cmp;
            $("#Cmp").val(_cmp);
            $("#CmpNm").val(map.Name);
            _handler.topData = { UId: _cmp };
            MenuBarFuncArr.MBCancel();
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        _cmp = rd.CMP;
        $("#Cmp").val(_cmp);
        $("#CmpNm").val(rd.NAME);
        _handler.topData = { UId: _cmp };
        MenuBarFuncArr.MBCancel();
    }));

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);

        gridEditableCtrl({ editable: false, gridId: "CostAllocationGrid" });
        var _mainCmpData = { UId: _cmp }; //_uid
        _handler.loadMainData(_mainCmpData);
        editable = false;
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(_handler.inquiryUrl, { Cmp: map.UId },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }
    _initUI(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);

    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);
    if (!isEmpty(_cmp)) {
        _handler.topData = { UId: _cmp };
        MenuBarFuncArr.MBCancel();
    }
});
