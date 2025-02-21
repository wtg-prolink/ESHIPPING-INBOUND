var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$SubGrid = $("#SubGrid");

$(function () {
    _handler.saveUrl = rootPath + "TKBL/SavePDocData";
    _handler.inquiryUrl = rootPath + LookUpConfig.PDocUrl;
    _handler.config = LookUpConfig.PDocLookup(select_tranmode, select_term);

    _handler.addData = function () {
        //初始化新增数据
        var data = { "TranMode": _tran,Cmp:cmp };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
    }

    _handler.beforSave = function () {
        var $grid = $SubGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "DocType", index: 2, text: '@Resources.Locale.L_PartyDocSetup_DocType' });
        nullCols.push({ name: "DocDescp", index: 3, text: '@Resources.Locale.L_PartyDocSetup_DocDescp' });
        sameCols.push({ name: "DocType", index: 2, text: '@Resources.Locale.L_PartyDocSetup_DocType' });
        return _handler.checkData($grid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false, UId: $("#UId").val(), Cmp: $("#Cmp").val() },
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        if (data["sub"])
            _handler.loadGridData("SubGrid", $SubGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + LookUpConfig.PDocItemUrl, { uId: map.UId,Cmp:map.Cmp, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    // editrules:{custom:true, custom_func:mypricecheck} 
    function mypricecheck(value, colname) {
        //var rowIds = $SubGrid.jqGrid('getDataIDs');
        return [false, "@Resources.Locale.L_PartyDocSetup_RecordExistence"];
    }

    function getFileTypeop(name) {
        var _name = name;
        var filetyp_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.FileTypeUrl,
                config: LookUpConfig.FileTypeLookup,
                returnFn: function (map, $grid) {
                    var rowid = $SubGrid.jqGrid('getGridParam', 'selrow');
                    //setGridVal($SubGrid, selRowId, "DocDescp", rd.CdDescp, 'lookup');
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "DocDescp", "lookup", map.CdDescp);
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "EDT", $SubGrid,
            function ($grid, rd, elem, rowid) {
                //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                //setGridVal($grid, selRowId, "DocDescp", rd.CD_DESCP, 'lookup');
                setGridVal($grid, rowid, "DocDescp", rd.CD_DESCP, null);
                setGridVal($grid, rowid, "DocType", rd.CD, 'lookup');
                $(elem).val(rd.CD);
                }), {
            param: "",
            baseConditionFunc: function () {
                return "";
            }
        });
        return filetyp_op;
    }

    var colModel1 = [
        { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', editable: false, hidden: true },
	    { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'DocType', title: '@Resources.Locale.L_PartyDocSetup_DocType', index: 'DocType', sorttype: 'string', hidden: false, editable: true, width: 90, editoptions: gridLookup(getFileTypeop("DocType")), edittype: 'custom' },
        { name: 'DocDescp', title: '@Resources.Locale.L_PartyDocSetup_DocDescp', index: 'DocDescp', sorttype: 'string', width: 200, hidden: false, editable: false },
        { name: 'Remark', title: '@Resources.Locale.L_PartyDocSetup_Remark', index: 'Remark', sorttype: 'string', width: 300, hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel1, caption: '@Resources.Locale.L_PartyDocSetup_EDocDescp', delKey: ["UId", "DocType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            var sbcmp = $("#Cmp").val();
            $SubGrid.jqGrid('setCell', rowid, "Cmp", sbcmp);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('DocType', { editable: true });
            } else {
                $SubGrid.setColProp('DocType', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $SubGrid.setColProp('DocType', { editable: true });
        }
    });

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + "Common/GetCompanyData", config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        //elem.val(rd.NAME);
    }));

    registBtnLookup($("#StsCdLookup"), {
        item: '#StsCd', url: rootPath + LookUpConfig.StatusUrl, config: LookUpConfig.StatusLookup, param: "", selectRowFn: function (map) {
            $("#StsCd").val(map.StsCd);
            $("#StsDescp").val(map.Edescp);
        }
    }, undefined, LookUpConfig.GetStatusAuto(groupId, undefined, function ($grid, rd, elem) {
        //elem.val(rd.NAME);
        $("#StsDescp").val(rd.EDESCP);
    }));

    registBtnLookup($("#TermLookup"), {
        item: '#Term', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Term").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {

    }));

    registBtnLookup($("#PartyTypeLookup"), {
        item: '#PartyType', url: rootPath + LookUpConfig.PartyTypeUrl, config: LookUpConfig.PartyTypeLookup, param: "", selectRowFn: function (map) {
            $("#PartyType").val(map.Cd);
            $("#PartyDescp").val(map.CdDescp);
            //$("#PartyNo").val("");
            //$("#PartyNm").val("");
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "PT", undefined, function ($grid, rd, elem) {
        $("#PartyDescp").val(rd.CD_DESCP);
        //$("#PartyNo").val("");
        //$("#PartyNm").val("");
    }));


    registBtnLookup($("#PartyNoLookup"), {
        item: '#PartyNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#PartyNo").val(map.PartyNo);
            $("#PartyNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            //return "PartyType_spot=like&PartyType=" + $("#PartyType").val();
            return "PARTY_TYPE LIKE '%" + $("#PartyType").val() + "%'";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PartyNm").val(rd.PARTY_NAME);
    }));

    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBEdoc", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid,Cmp:_cmp };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
});


var _tran = "", _bu = "P";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("PartyDocSetup") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TNT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);
            if (_handler.topData) {
                $("#TranMode").val(_handler.topData["TranMode"]);
            }
        }
    });
}
