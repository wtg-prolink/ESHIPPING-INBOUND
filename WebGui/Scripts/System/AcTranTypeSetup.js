var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var _uid = "";
var groupId = getCookie("plv3.passport.groupid"),
    _cmp = getCookie("plv3.passport.companyid");

jQuery(document).ready(function ($) {
    $("#Cmp").val(_cmp);
    $("#CmpNm").val(CmpNm);
    var $AcTranTypeGrid = $("#AcTranTypeGrid");
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 260;

    var colModel = [
            { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true },
            { name: 'Cmp', title: 'location', index: 'Cmp', sorttype: 'string', editable: false },
            { name: 'SaSp', title: 'SAP/SPMS', index: 'SaSp', width: 120, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'A:SAP;P:SPMS', defaultValue: 'SAP', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
            { name: 'CmpNm', title: 'Cmp Nm', index: 'ChgDescp', width: 100, sorttype: 'string', hidden: true },
            { name: 'Bu', title: 'Original DN', index: 'Bu', editoptions: gridLookup(getop("Bu")), edittype: 'custom', sorttype: 'string', width: 180, hidden: false, editable: true },
            //{ name: 'Bu', title: 'Bu', index: 'TranMode', width: 180, sorttype: 'string', editable: true, edittype: 'custom', editoptions: getpartyop(getpartyop) },
            { name: 'SoldTo', title: 'Sold To', index: 'SoldTo', editoptions: gridLookup(getpartyop("")), edittype: 'custom', sorttype: 'string', width: 150, hidden: false, editable: true },
            { name: 'AtranType', title: 'Sort', index: 'AtranType', width: 100, sorttype: 'string', editable: true, formatter: 'select', edittype: 'select', editoptions: { value: AtranType, defaultValue: DefaultAtran }, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } }
    ];
    _handler.saveUrl = rootPath + "ACTRANTYPE/AcTranTypeUpdate";
    _handler.inquiryUrl = rootPath + "ACTRANTYPE/AcTranTypeQuery";
    _handler.search = function () {
    };
    function getop(name) {
        var _name = name;
        var bu_op = getLookupOp("AcTranTypeGrid",
            {
                url: rootPath + LookUpConfig.TVKOUrl,
                config: LookUpConfig.BSCodeLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'Bu', map.Cd, 'lookup');
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TVKO", $AcTranTypeGrid, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'Bu', rd.CD, 'lookup');
                $(elem).val(rd.CD);
            }));
        return bu_op;
    }

    _handler.afterEdit=function()
    {
        $('#Cmp').attr("disabled", true);
        $('#CmpLookup').attr("disabled", true);
        //$('#areaSelect').attr("disabled", "disabled");
        //$("#CmpLookup").Disabled();
    }
    

    function getpartyop(name) {
        var _name = name;
        var city_op = getLookupOp("AcTranTypeGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (returnObj, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'SoldTo', returnObj.PartyNo, 'lookup');
                    return returnObj.PartyNo;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $AcTranTypeGrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'SoldTo', rd.PARTY_NO, 'lookup');
            }));
        return city_op;
    }

    _handler.intiGrid("AcTranTypeGrid", $AcTranTypeGrid, {
        colModel: colModel, caption: 'Ac Tran Type', delKey: ["UId","Cmp"],
        onAddRowFunc: function (rowid) {
            var cmpvalue = $("#Cmp").val();
            var cmpnmvalue = $("#CmpNm").val();
            $AcTranTypeGrid.jqGrid('setCell', rowid, "Cmp", cmpvalue);
            $AcTranTypeGrid.jqGrid('setCell', rowid, "CmpNm", cmpnmvalue);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$AcTranTypeGrid.setColProp('Cmp', { editable: true });
            //$AcTranTypeGrid.setColProp('CmpNm', { editable: true });
        }
    });

   
    //MenuBarFuncArr.MBEdit = function () {
    //    gridEditableCtrl({ editable: true, gridId: "AcTranTypeGrid" });
    //    editable = true;
   // }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.loadGridData("AcTranTypeGrid", $AcTranTypeGrid[0], data["main"], [""]);
        else
            _handler.loadGridData("AcTranTypeGrid", $AcTranTypeGrid[0], [], [""]);
        //setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        $('#Cmp').attr("disabled", false);
        $('#CmpLookup').attr("disabled", false);
    }


    _handler.saveData = function (dtd) {
        var containerArray = $('#AcTranTypeGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
      
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)),"Cmp":_cmp, autoReturnData: false },
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

        gridEditableCtrl({ editable: false, gridId: "AcTranTypeGrid" });
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
    
    //["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]
    _initUI(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);//初始化UI工具栏
    //MenuBarFuncArr.DelMenu(["MBSearch", "MBDel", "MBCopy", "MBApply", "MBApprove"]);

    //_initUI(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);//初始化UI工具栏
    //MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);
    if (!isEmpty(_cmp)) {
        _handler.topData = { UId: _cmp };
        MenuBarFuncArr.MBCancel();
    }
});
