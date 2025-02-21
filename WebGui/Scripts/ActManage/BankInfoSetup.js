//var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var $MainGrid = $("#MainGrid");
$(function () {
    _handler.saveUrl = rootPath + "ActManage/SaveBankInfoData";
    _handler.inquiryUrl = rootPath + "/ActManage/GetBankInfoData";
    _handler.config = LookUpConfig.CmpLookup;
    _handler.key = "Cmp";

    function getcur(name) {
        var _name = name;
        var cur_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CurUrl,
                config: LookUpConfig.CurLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Crncy", map.Cur, "lookup");
                    return map.Cur;
                }
            }, LookUpConfig.GetCurAuto(groupId, "", $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Crncy", rd.CUR, "lookup");
                $(elem).val(rd.CUR);
            }), { param: "" });
        return cur_op;
    }

    var colModel1 = [
	    { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true, editable: false },
        { name: 'LSP_NO', title: 'LSP_NO', index: 'LSP_NO', sorttype: 'string', hidden: true, editable: false },
        { name: 'LSP_NM', title: 'LSP_NM', index: 'LSP_NM', sorttype: 'string', hidden: true, editable: false },
        { name: 'Crncy', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Crncy', sorttype: 'string', editoptions: gridLookup(getcur("Crncy")), edittype: 'custom', width: 100, align: 'left', hidden: false, editable: true },
        { name: 'CollectBank', title: '@Resources.Locale.L_DNManage_ReBank', index: 'CollectBank', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'AccountName', title: '@Resources.Locale.L_DNManage_Ac', index: 'AccountName', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'BankInfo', title: '@Resources.Locale.L_DNManage_BkAc', index: 'BankInfo', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'SwiftCode', title: 'Swift Code', index: 'SwiftCode', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_NRSSetup_Remark', index: 'Remark', sorttype: 'string', width: 300, hidden: false, editable: true },
        { name: 'BankType', title: '@Resources.Locale.L_BankInfo_BankType', index: 'BankType', sorttype: 'string', width: 150, hidden: false, editable: true }
    ];

    _handler.beforSave = function () {
        var $grid = $MainGrid;
        var nullCols = [], sameCols = [];
        //nullCols.push({ name: "Name", index: 6, text: "名称" });
        //sameCols.push({ name: "Name", index: 6, text: "名称" });
        return _handler.checkData($grid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        if (containerArray.length <= 0) {
            return _handler.endSave(dtd);
        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
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
        colModel: colModel1, caption: _getLang("L_BankList", "银行列表"), delKey: ["UId", "Cmp"],
        onAddRowFunc: function (rowid) {
            $MainGrid.jqGrid('setCell', rowid, "UId", uuid());
            $MainGrid.jqGrid('setCell', rowid, "Cmp", _handler.topData.Cmp);
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $MainGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        }
    });

    _handler.setFormData = function (data) {
        _handler.loadGridData("MainGrid", $MainGrid[0], data || [], [""]);
        setdisabled(true);
        setToolBtnDisabled(true);

        $("#SeqNo").removeAttr('required');
        if (_upri === "I") {
            $("#txt_Cmp").removeAttr('disabled');
            $("#CmpLookup").removeAttr('disabled');
        }
    }

    _handler.loadMainData = function (map) {
        _dm.getDs("MainGrid").setData([]);
        _handler.topData = map || {};
        if (!map || !map[_handler.key]) {
            return;
        }
        $("#txt_Cmp").val(map.Cmp);
        $("#txt_CmpName").val(map.LspNm);
        ajaxHttp(rootPath + "ActManage/GetMailGroup", { cmp: map.Cmp, loading: true },
            function (data) {
                _handler.setFormData(data);
            });
    }

    _handler.afterEdit = function () {
        $("#CmpLookup").attr('disabled', true);
        $("#txt_Cmp").attr('disabled', true);
    }

    if (_upri === "I") {
        _initUI(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBErrMsg"]);
    }
    else {
        _initUI(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid", "MBSearch", "MBErrMsg"]);
        $("#CmpLookup").attr('disabled', true);
        $("#txt_Cmp").attr('disabled', true);
    }

    registBtnLookup($("#CmpLookup"), {
        item: '#txt_Cmp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#txt_Cmp").val(map.PartyNo);
            $("#txt_CmpName").val(map.PartyName);
            _handler.topData = { LspNo: map.PartyNo, Cmp: map.PartyNo, LspNm: map.PartyName };
            MenuBarFuncArr.MBCancel();
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#txt_Cmp").val(rd.PARTY_NO);
        $("#txt_CmpName").val(rd.PARTY_NAME);
        _handler.topData = { LspNo: rd.PARTY_NO, Cmp: rd.PARTY_NO, LspNm: rd.PARTY_NAME };
        MenuBarFuncArr.MBCancel();
    }));

    $("#txt_Cmp").lookuptrig = true;
    $("#txt_Cmp").val(cmp);
    $("#txt_Cmp").blur();
    if (_upri === "I")
        $("#CmpLookup").removeAttr('disabled');
    //_handler.topData = { PartyNo: cmp };
    //MenuBarFuncArr.MBCancel();
});