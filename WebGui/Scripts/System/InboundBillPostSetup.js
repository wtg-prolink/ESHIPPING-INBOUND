var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var _uid = "";
var groupId = getCookie("plv3.passport.groupid"),
    _cmp = getCookie("plv3.passport.companyid");

jQuery(document).ready(function ($) {
    $("#Cmp").val(_cmp);
    $("#CmpNm").val(CmpNm);
    var $PostBillGrid = $("#PostBillGrid");
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 260;

    var colModel = [
            { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'GroupId', title: 'GroupId', index: 'GroupId', width: 100, sorttype: 'string', hidden: true },
            { name: 'Cmp', title: 'location', index: 'Cmp', width: 100, sorttype: 'string', editable: false },
            { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', editoptions: gridLookup(getop("Incoterm")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
            { name: 'IncotermDescp', title: 'Description of Incoterm', index: 'IncotermDescp', width: 250, align: 'left', sorttype: 'string', hidden: false, editable: true },
            { name: 'IsEtaMsl', title: 'Delivery Date', index: 'IsOutdate', width: 100, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'N:N;Y:Y', defaultValue: 'N', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
            { name: 'IsEta', title: 'ETA(D)', index: 'IsBroker', width: 100, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'N:N;Y:Y', defaultValue: 'N', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
            { name: 'Remark', title: 'Remark', index: 'Remark', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'CreateBy', title: _getLang("L_Bsdate_CreateBy", "创建人"), index: 'CreateBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CreateDate', title: _getLang("L_Bsdate_CreateDate", "创建时间"), index: 'CreateDate', width: 120, align: 'left', sorttype: 'date', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } },
        { name: 'ModifyBy', title: _getLang("L_Bsdate_ModifyBy", "修改人"), index: 'ModifyBy', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'ModifyDate', title: _getLang("L_Bsdate_ModifyDate", "修改时间"), index: 'ModifyDate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatoptions: { srcformat: "ISO8601Long", newformat: "Y-m-d h:i A" } }
    ];
    _handler.saveUrl = rootPath + "System/InboundPostBillUpdate";
    _handler.inquiryUrl = rootPath + "System/InboundPostBillQuery";
    _handler.search = function () {
    };

    function getop(name) {
        var _name = name;
        var lookupurl = LookUpConfig.OrderTypeUrl;
        var lookupcode = 'TVAK';
        if (_name == "Bu") {
            lookupurl = LookUpConfig.TVKOUrl;
            lookupcode = 'TVKO';
        } else if (_name == "ChannelCd") {
            lookupurl = LookUpConfig.ChannelUrl;
            lookupcode = 'TVTW';
        } else if (_name == "Incoterm") {
            lookupurl = LookUpConfig.DlvTermUrl;
            lookupcode = 'TD';
        }
        var bu_op = getLookupOp("PostBillGrid",
            {
                url: rootPath + lookupurl,
                config: LookUpConfig.BSCodeLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, _name, map.Cd, 'lookup');
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, lookupcode, $PostBillGrid, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, _name, rd.CD, 'lookup');
                $(elem).val(rd.CD);
            }), { param: "" });
        return bu_op;
    }

    _handler.afterEdit = function () {
        $('#Cmp').attr("disabled", true);
        $('#CmpLookup').attr("disabled", true);
        //$('#areaSelect').attr("disabled", "disabled");
        //$("#CmpLookup").Disabled();
    }

    _handler.intiGrid("PostBillGrid", $PostBillGrid, {
        colModel: colModel, caption: 'Post Bill', delKey: ["UId", "Cmp"],
        onAddRowFunc: function (rowid) {
            var cmpvalue = $("#Cmp").val();
            var cmpnmvalue = $("#CmpNm").val();
            $PostBillGrid.jqGrid('setCell', rowid, "Cmp", cmpvalue);
            $PostBillGrid.jqGrid('setCell', rowid, "CmpNm", cmpnmvalue);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$PostBillGrid.setColProp('Cmp', { editable: true });
            //$PostBillGrid.setColProp('CmpNm', { editable: true });
        }
    });


    //MenuBarFuncArr.MBEdit = function () {
    //    gridEditableCtrl({ editable: true, gridId: "PostBillGrid" });
    //    editable = true;
    // }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.loadGridData("PostBillGrid", $PostBillGrid[0], data["main"], [""]);
        else
            _handler.loadGridData("PostBillGrid", $PostBillGrid[0], [], [""]);
        //setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        $('#Cmp').attr("disabled", false);
        $('#CmpLookup').attr("disabled", false);
    }


    _handler.saveData = function (dtd) {
        var containerArray = $('#PostBillGrid').jqGrid('getGridParam', "arrangeGrid")();
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

        gridEditableCtrl({ editable: false, gridId: "PostBillGrid" });
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
