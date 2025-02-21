var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var _uid = "";
var groupId = getCookie("plv3.passport.groupid"),
    _cmp = getCookie("plv3.passport.companyid");

jQuery(document).ready(function ($) {
    $("#Cmp").val(_cmp);
    $("#CmpNm").val(CmpNm);
    var $DestMapGrid = $("#DestMapGrid");
    var $DirectGrid = $("#DirectGrid");
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 260;

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
        { name: 'Cmp', title: 'location', index: 'Cmp', sorttype: 'string', editable: false },
        { name: 'CmpNm', title: 'Cmp Nm', index: 'CmpNm', width: 100, sorttype: 'string', hidden: true },
        { name: 'DestCode', title: 'Dest', index: 'DestCode', width: 120, sorttype: 'string', hidden: false, editable: true, editoptions: gridLookup(getcityop()), edittype: 'custom' },
        { name: 'DestName', title: 'Dest Name', index: 'DestName', width: 200, sorttype: 'string', hidden: false },
        { name: 'SecCmp', title: 'Sec Location', index: 'SecCmp', sorttype: 'string', width: 120, hidden: false, editable: true, editoptions: gridLookup(getCmpop()), edittype: 'custom' }
    ];
    var subColModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
        { name: 'Cmp', title: 'location', index: 'Cmp', sorttype: 'string', editable: false },
        { name: 'CmpNm', title: 'Cmp Nm', index: 'CmpNm', width: 100, sorttype: 'string', hidden: true },
        { name: 'PartyNo', title: 'Party No', index: 'PartyNo', width: 120, sorttype: 'string', hidden: false, editable: true, editoptions: gridLookup(getPartyNoop()), edittype: 'custom' },
        { name: 'PartyName', title: 'Party Name', index: 'PartyName', width: 200, sorttype: 'string', hidden: false }
        //{ name: 'SecCmp', title: 'Sec Location', index: 'SecCmp', sorttype: 'string', width: 120, hidden: false, editable: true, editoptions: gridLookup(getCmpop()), edittype: 'custom' }
    ];
    _handler.saveUrl = rootPath + "ACTRANTYPE/DestMapUpdate";
    _handler.inquiryUrl = rootPath + "ACTRANTYPE/DestMapQuery";
    _handler.search = function () {
    };

    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("DestMapGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "DestName", map.PortNm, null);
                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $DestMapGrid,
                function ($grid, rd, elem, selRowId) {
                    setGridVal($grid, selRowId, "DestName", rd.PORT_NM, null);
                    $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
                }), { param: "" });
        return city_op;
    }

    function getCmpop() {
        var Cmp_op = getLookupOp("DestMapGrid",
            {
                url: rootPath + LookUpConfig.GetCmpUrl,
                config: LookUpConfig.CmpLookup,
                returnFn: function (map, $grid, selRowId) {
                    selRowId = selRowId || $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "SecCmp", map.Cd, "lookup");
                    return map.Cmp;
                }
            }, LookUpConfig.GetCmpAuto(groupId, $("#DestMapGrid"), function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "SecCmp", rd.CMP, "lookup");
                $(elem).val(rd.CMP);
            },
                function () {
                    return "GROUP_ID=" + groupId + " AND TYPE='1'";
                }), {
                param: "",
                baseConditionFunc: function () {
                    return "GROUP_ID='" + groupId + "' AND TYPE='1'";
                }
            });
        return Cmp_op;
    }

    function getPartyNoop() {
        var PartyNo_op = getLookupOp("DirectGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid, selRowId) {
                    selRowId = selRowId || $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'PartyName', Cmp.PartyName, null);
                    setGridVal($grid, selRowId, 'PartyNo', map.PartyNo, "lookup");
                    return map.PartyNo;

                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $DirectGrid,
                function ($grid, rd, elem, selRowId) {
                    setGridVal($grid, selRowId, 'PartyName', rd.PARTY_NAME, null);
                    setGridVal($grid, selRowId, 'PartyNo', rd.PARTY_NO, "lookup");
                    $(elem).val(rd.PARTY_NO);
                }), { param: "" });
        return PartyNo_op;
    }

    _handler.afterEdit = function () {
        $('#Cmp').attr("disabled", true);
        $('#CmpLookup').attr("disabled", true);
    }


    _handler.intiGrid("DestMapGrid", $DestMapGrid, {
        colModel: colModel, caption: 'Destination Map', delKey: ["UId", "Cmp"],
        onAddRowFunc: function (rowid) {
            var cmpvalue = $("#Cmp").val();
            var cmpnmvalue = $("#CmpNm").val();
            $DestMapGrid.jqGrid('setCell', rowid, "Cmp", cmpvalue);
            $DestMapGrid.jqGrid('setCell', rowid, "CmpNm", cmpnmvalue);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$DestMapGrid.setColProp('Cmp', { editable: true });
            //$DestMapGrid.setColProp('CmpNm', { editable: true });
        }
    });

    _handler.intiGrid("DirectGrid", $DirectGrid, {
        colModel: subColModel, caption: 'Directshipment Map', delKey: ["UId","Cmp"],
        onAddRowFunc: function (rowid) {
            var cmpvalue = $("#Cmp").val();
            var cmpnmvalue = $("#CmpNm").val();
            $DirectGrid.jqGrid('setCell', rowid, "Cmp", cmpvalue);
            $DirectGrid.jqGrid('setCell', rowid, "CmpNm", cmpnmvalue);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$DestMapGrid.setColProp('Cmp', { editable: true });
            //$DestMapGrid.setColProp('CmpNm', { editable: true });
        }
    });

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.loadGridData("DestMapGrid", $DestMapGrid[0], data["main"], [""]);
        else
            _handler.loadGridData("DestMapGrid", $DestMapGrid[0], [], [""]);
        if (data["sub"])
            _handler.loadGridData("DirectGrid", $DirectGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("DirectGrid", $DirectGrid[0], [], [""]);
        //setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        $('#Cmp').attr("disabled", false);
        $('#CmpLookup').attr("disabled", false);
    }


    _handler.saveData = function (dtd) {
        var containerArray = $('#DestMapGrid').jqGrid('getGridParam', "arrangeGrid")();
        var directArray = $('#DirectGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        changeData["sub"] = directArray;
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "Cmp": _cmp, autoReturnData: false },
            function (result) {
                _handler.topData = { UId: result.Cmp };
                //_topData = keyData["mt"];
                if (result.message != 'success') {
                    alert(result.message);
                    return false;
                }
                //else if (_handler.setFormData)
                //    _handler.setFormData(result);
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

        gridEditableCtrl({ editable: false, gridId: "DestMapGrid" });
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
    _initUI(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid","MBPreview"]);//初始化UI工具栏
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);
    if (!isEmpty(_cmp)) {
        _handler.topData = { UId: _cmp };
        MenuBarFuncArr.MBCancel();
    }
});