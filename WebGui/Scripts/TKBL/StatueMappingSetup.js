//var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var $MainGrid = $("#MainGrid");
$(function () {
    _handler.saveUrl = rootPath + "TKBL/SaveStsMappingData";
    _handler.inquiryUrl = rootPath + "/TKBL/GetStsMappingGroupData";
    _handler.config = LookUpConfig.CmpLookup;
    _handler.key = "Cmp";

    function getStatusop(name) {
        var _name = name;
        var status_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.StatusUrl,
                config: LookUpConfig.StatusLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, _name, map.StsCd, "lookup");
                    setGridVal($grid, selRowId, "StsDescp", map.Ldescp, null);
                    return map.StsCd;
                }
            }, LookUpConfig.GetStatusAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "StsDescp", rd.LDESCP, null);
                setGridVal($grid, rowid, _name, rd.STS_CD, "lookup");
                $(elem).val(rd.STS_CD);
            }), { param: "" });
        return status_op;
    }
  
    var colModel1 = [
        { name: 'GroupId', title: 'GroupId', index: 'GroupId', sorttype: 'string', hidden: true, editable: false },
        { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true, editable: false },
        { name: 'Stn', title: 'Stn', index: 'Stn', sorttype: 'string', hidden: true, editable: false },
        { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'string', hidden: true, editable: false },
        { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true, editable: false },
        //{ name: 'SeqNo', title: '@GetLangText("L_NRSSetup_SeqNo")', index: 'SeqNo', sorttype: 'string', hidden: true, editable: false },
        { name: 'CstsCd', title: '@Resources.Locale.L_StatueMappingSetup_CstsCd', index: 'CstsCd', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'CstsDescp', title: '@Resources.Locale.L_StatueMappingSetup_CstsDescp', index: 'CstsDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'StsCd', title: '@Resources.Locale.L_StatueMappingSetup_StsCd', index: 'StsCd', editoptions: gridLookup(getStatusop("StsCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'StsDescp', title: '@Resources.Locale.L_StatueMappingSetup_StsDescp', index: 'StsDescp', sorttype: 'string', width: 200, hidden: false, editable: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 500, hidden: false, editable: true }
    ];

    _handler.beforSave = function () {
        var $grid = $MainGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "CstsCd", index: 6, text: "Statue" });
        nullCols.push({ name: "StsCd", index: 8, text: "Ours Status" });
        sameCols.push({ name: "CstsCd", index: 6, text: "Statue" });
        return _handler.checkData($grid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        console.log(changeData);
        var data = {
            "changedData": encodeURIComponent(JSON.stringify(changeData)), 
            Cmp: encodeURIComponent(_handler.topData[_handler.key]),
            CmpNm: encodeURIComponent(_handler.topData["PartyName"])
        }
        if (containerArray.length <= 0) {
            return _handler.endSave(dtd);
        }
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

    _handler.search = function (item) {
        registBtnLookup(item, {
            url: _handler.inquiryUrl, config: _handler.config, param: "", selectRowFn: function (map) {
                if (_handler.loadMainData)
                    _handler.loadMainData(map);
            }
        });
    }

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel1, caption: 'Statue Mapping ', delKey: ["UId"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;

            setGridVal($MainGrid, rowid, "UId", uuid(), null);
            setGridVal($MainGrid, rowid, "Cmp", _handler.topData[_handler.key], null);
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1, null);
            setGridVal($MainGrid, rowid, "GroupId",groupId, null);
        },
        beforeSelectRowFunc: function (rowid) {
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $MainGrid.setColProp('CstsCd', { editable: true });
            } else {
                $MainGrid.setColProp('CstsCd', { editable: false });
            }
        },
    });

    _handler.beforEdit = function () {
        if (isEmpty($("#txt_Cmp").val())) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.afterEdit = function () {
        $("#CmpLookup").attr('disabled', true);
        $("#txt_Cmp").attr('disabled', true);
    }

    _handler.setFormData = function (data) {
        _handler.loadGridData("MainGrid", $MainGrid[0], data || [], [""]);
        setdisabled(true);
        setToolBtnDisabled(true);

        $("#SeqNo").removeAttr('required');
        $("#txt_Cmp").removeAttr('disabled');
        $("#CmpLookup").removeAttr('disabled');
    }

    _handler.loadMainData = function (map) {
        _dm.getDs("MainGrid").setData([]);
        _handler.topData = map || {};
        if (!map || !map[_handler.key]) {
            return;
        }
        $("#txt_Cmp").val(map.Cmp);
        $("#txt_CmpName").val(map.Name);
        ajaxHttp(rootPath + "TKBL/GetStsMappingItem", { cmp: map.Cmp, loading: true },
            function (data) {
                _handler.setFormData(data);
            });
    }

    registBtnLookup($("#CmpLookup"), {
        item: '#txt_Cmp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#txt_Cmp").val(map.PartyNo);
            $("#txt_CmpName").val(map.PartyName);
            _handler.topData = { Cmp: map.PartyNo, Name: map.PartyName };
            MenuBarFuncArr.MBCancel();
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#txt_Cmp").val(rd.PARTY_NO);
        $("#txt_CmpName").val(rd.PARTY_NAME);
        _handler.topData = { Cmp: rd.PARTY_NO, Name: rd.PARTY_NAME };
        MenuBarFuncArr.MBCancel();
    }));

    _initUI(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);//初始化UI工具栏
    $("#txt_Cmp").lookuptrig = true;
    $("#txt_Cmp").val(cmp);
    $("#txt_Cmp").blur();

    $("#txt_Cmp").removeAttr('disabled');
    $("#CmpLookup").removeAttr('disabled');
    //_handler.topData = { PartyNo: cmp };
    //MenuBarFuncArr.MBCancel();
});