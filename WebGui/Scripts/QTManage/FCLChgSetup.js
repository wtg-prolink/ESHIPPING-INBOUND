var $MainGrid;
var _fields = [];
function _checkEdit(type) {
    if (type !== 2) {
        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Connot", 500, "warning");
            return false;
        }
    }
    if (type === 1)
        return;
    if (_handler.topData["QuotType"] === "A") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Verified", 500, "warning");
        return false;
    }
    if (_handler.topData["QuotType"] === "V") {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
        return false;
    }
    if (type === 2)
        return;
    if (userId !== _handler.topData["CreateBy"]) {
        CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Connot", 500, "warning");
        return false;
    }
}

function initQTGrid() {
    $MainGrid = $("#MainGrid");
    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
            }), { param: "" });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var unit_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TCARUrl,
                config: LookUpConfig.BSCodeLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }));
        return unit_op;
    }

    function getchg(name) {
        var _name = name;
        var chg_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.ChgUrl,
                config: LookUpConfig.ChgLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "ChgDescp", map.ChgDescp, null);
                    setGridVal($grid, selRowId, "ChgType", map.ChgType, null);
                    setGridVal($grid, selRowId, "Repay", map.Repay, null);
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    return map.ChgCd;
                }
            }, LookUpConfig.GetChgAuto1(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, null);
                setGridVal($grid, rowid, "Repay", rd.REPAY, null);
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                $(elem).val(rd.CHG_CD);
            }, function () {
                //var tranType = $("#TranType").val();
                return " TRAN_MODE@@F;O";
            }), {
                param: "",
                baseConditionFunc: function () {
                    //var tranType = $("#TranType").val();
                    return "TRAN_MODE IN ('F','O')";
                }
            });
        return chg_op;
    }

    function getcur(name) {
        var _name = name;
        var cur_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CurUrl,
                config: LookUpConfig.CurLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Cur", map.Cur, "lookup");
                    return map.Cur;
                }
            }, LookUpConfig.GetCurAuto(groupId,"", $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
                $(elem).val(rd.CUR);
            }), { param: "" });
        return cur_op;
    }

    function get_regionop(name) {
        var _name = name;
        var region_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TrgnUrl,
                config: LookUpConfig.TrgnLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Region", map.Cd, "lookup");
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
                setGridVal($grid, rowid, "Region", map.CD, "lookup");
            }, function ($grid,elem, rowid) { }), { param: "" });
        return region_op;
    }

    function getUnit(name) {
        var _name = name;
        var unit_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return unit_op;
    }

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        //{ name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', editoptions: gridLookup(getcust("Carrier")), edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'PolCd', title: '@Resources.Locale.L_FCLFSetup_PodCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgType', index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: '@Resources.Locale.L_QTSetup_chgCd', index: 'Repay', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: '@Resources.Locale.L_FCLChgSetup_Script_166' }, edittype: 'select' },
        //{ name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 100, align: 'left', hidden: false, editable: true },
        { name: 'F3', title: '@Resources.Locale.L_FCLFSetup_F3', index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: true },
        { name: 'F4', title: '20GP', index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F5', title: '40GP', index: 'F5', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F6', title: '40HQ', index: 'F6', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', index: 'Region', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_regionop("Region")), width: 80, hidden: false, editable: true },
        { name: 'CollectBy', title: '@Resources.Locale.L_QTSetup_CollectBy', index: 'CollectBy', sorttype: 'string', width: 60, hidden: false, editable: true, formatter: "select", editoptions: { value: ': ;SP:SP;CR:CR' }, edittype: 'select' },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        //{ name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];
    var ignoD = { "UId": true, "RfqNo": true, "SeqNo": true, "QuotNo": true };
    for (var i = 0; i < colModel.length; i++) {
        if (!ignoD[colModel[i].name])
            _fields.push(colModel[i].name);
    }

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: 'FCL', delKey: ["UId"], height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($MainGrid, rowid, { "PodCd": true, "Cur": true });
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        },
        loadComplete: function(data){
            dynamicHeight();
        }
        
    });
}

var lookUpConfig = {
    caption: "@Resources.Locale.L_FCLChgSetup_Scripts_0",
    sortname: "CreateDate",
    refresh: false,
    columns: [
            { name: 'UId', title:'id',index: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', width: 200, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'QuotDate', title: '@Resources.Locale.L_QTQuery_QuotDate', index: 'QuotDate', editable: false, formatter: 'date', editoptions: myEditDateInit, formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: null }, width: 130, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RlocationNm', title: '@Resources.Locale.L_RQQuery_Rlocation', index: 'RlocationNm', width: 200, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'LspNm', title: '@Resources.Locale.L_AirSetup_LspCd', index: 'LspNm', width: 200, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'EffectFrom', title: '@Resources.Locale.L_RQQuery_EffectFrom', index: 'EffectFrom', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'EffectTo', title: '@Resources.Locale.L_RQQuery_EffectTo', index: 'EffectTo', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
}

$(function() {
    intQtView();
    initQTGrid();
    _handler.inquiryUrl = rootPath + "QTManage/GetFCLCHGData";
    _handler.config = lookUpConfig;

    //_handler.beforEdit = function () {
    //    if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
    //        CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
    //        return false;
    //    }
    //}

    _handler.afterEdit = function () {
        if (!isEmpty($("#QuotNo").val())) {
            $("#QuotNo").attr('disabled', true);
        }
        if (!isEmpty($("#QuotDate").val())) {
            $("#QuotDate").attr('disabled', true);
            $("#QuotDate").parent().find("button").attr('disabled', true);
        }
        else {
            $("#QuotDate").val(getDate());
        }
        $("#Rlocation").parent().find("button").attr('disabled', true);
        if (!isEmpty(_handler.topData["LspCd"])) {
            $("#LspCd").attr('disabled', true);
            $("#LspCd").parent().find("button").attr('disabled', true);
        }

        //if (isEmpty(_handler.topData["LspCd"])) {
        //    $("#LspCd").val(cmp);
        //    $("#LspCd").blur();
        //    $("#QuotType").val("F");
        //}
    }


    _handler.beforLoadView = function () {
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        var requires = ["QuotNo", "QuotDate", "EffectFrom", "EffectTo", "Rlocation", "LspCd"];
        var readonlys = ["Rlocation", "RlocationNm", "LspNm", "QuotType"];
      
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
            $("#" + readonlys[i]).parent().find("button").attr('disabled', false);
        }

        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
    }
  
    _handler.addData = function () {
        //初始化新增数据
        var data = { "TranMode": "O", QuotType: "P", "TranType": "F", "Period": "B", "OutIn": "O", "FreightTerm": "PP", "QuotDate": getDate(0, "-"), Rlocation: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#Rlocation").blur();
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");
    }

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        //nullCols.push({ name: "Carrier", index: 10, text: 'Carrier' });
        nullCols.push({ name: "ChgType", index: 13, text: '@Resources.Locale.L_SMCHGSetup_ChgRepay' });
        //nullCols.push({ name: "PolCd", index: 10, text: '港口' });
        nullCols.push({ name: "ChgCd", index: 11, text: '@Resources.Locale.L_SMCHGSetup_ChgCd' });
        nullCols.push({ name: "Punit", index: 15, text: '@Resources.Locale.L_AirQuery_Punit' });
        var result = _handler.checkData($MainGrid, nullCols, sameCols);
        if (result===false) return false;
        return checkDoubleData($MainGrid);
    }

    function checkDoubleData($grid) {
        var rowIds = $grid.getDataIDs();
        var checkKey = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
            var key = rowDatas["PolCd"] + "#" + rowDatas["ChgCd"] + "#" + rowDatas["Region"] + "#" + rowDatas["CollectBy"];
            for (var x = 0; x < checkKey.length; x++) {
                if (key === checkKey[x]) {
                    try {
                        $grid.jqGrid("editCell", rowIds[i], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", "@Resources.Locale.L_FCLChgSetup_Script_167", 2000, "warning");
                    return false;
                }
            }
            for (var j = 0; j < rowIds.length; j++) {
                if (rowIds[i] === rowIds[j])
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                var key1 = rowDatas1["PolCd"] + "#" + rowDatas1["ChgCd"] + "#" + rowDatas1["Region"] + "#" + rowDatas1["CollectBy"];
                if (key1 === key) {
                    try {
                        $grid.jqGrid("editCell", rowIds[j], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", "@Resources.Locale.L_FCLChgSetup_Script_167", 2000, "warning");
                    return false;
                }
            }
        }
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        data["term"] = encodeURIComponent($("#Incoterm").val());
        data["mode"] = "1";
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
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
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        MenuBarFuncArr.initEdoc($("#UId").val());
        //MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
        MenuBarFuncArr.Enabled(["VoidBtn"]);
        MenuBarFuncArr.Enabled(["ApproveBtn"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
        MenuBarFuncArr.Enabled(["MBCopy"]);
        setRQData(data);
    }

    registBtnLookup($("#LspCdLookup"), {
        item: '#LspCd', url: rootPath + LookUpConfig.TCARUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#LspCd").val(map.Cd);
            $("#LspNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", undefined, function ($grid, rd, elem) {
        $("#LspCd").val(rd.CD);
        $("#LspNm").val(rd.CD_DESCP);
    }));


    //registBtnLookup($("#RlocationLookup"), {
    //    item: '#Rlocation', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#Rlocation").val(map.PartyNo);
    //        $("#RlocationNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#RlocationNm").val(rd.PARTY_NAME);
    //}));

    var GetSiteCmpAuto = function (groupId, $grid, autoFn, clearFn) {
        var op =
        {
            autoCompDt: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=",
            autoCompParams: "CMP=showValue,CMP,NAME",
            autoCompFunc: function (elem, event, ui, rowid) {
                autoFn($grid, ui.item.returnValue, elem, rowid);
            },
            autoClearFunc: function (elem, event, rowid) {
                clearFn($grid, elem, rowid);
            }
        };
        return op;
    }
    registBtnLookup($("#RlocationLookup"), {
        item: "#Rlocation", url: rootPath + "TPVCommon/GetSiteCmpData", config: LookUpConfig.SiteLookup, param: "", selectRowFn: function (map) {
            $("#Rlocation").val(map.Cd);
            $("#RlocationNm").val(map.CdDescp);
        }
    }, { baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'" }, GetSiteCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Rlocation").val(rd.CMP);
        $("#RlocationNm").val(rd.NAME);
    }, function ($grid, elem, rowid) {
        $("#Rlocation").val("");
        $("#RlocationNm").val("");
    }));

    

    registBtnLookup($("#IncotermLookup"), {
        item: "#Incoterm", url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Incoterm").val(map.Cd);
            //$("#IncotermDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        //$("#IncotermDescp").val(rd.CD_DESCP);
        $("#Incoterm").val(rd.CD);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: "123", GROUP_ID: groupId, CMP: cmp, STN: stn });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        $("#QuotDate").val(getDate(0, "-"));
        $("#EffectFrom").val(getDate(0, "-"));
        $("#EffectTo").val(getDate(365, "-"));
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        
        $("#ApproveBy").val("");
        $("#ApproveDate").val("");
        $("#TranType").val("F");
        $("#QuotType").val("P");
        $("#" + _handler.key).val(uuid());
        var dataRow, addData = [];
        var rowIds = $MainGrid.getDataIDs();
        var fields = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            dataRow = {};
            for (var y = 0; y < _fields.length; y++) {
                dataRow[_fields[y]] = rowDatas[_fields[y]];
            }
            addData.push(dataRow);
        }

        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        if (_handler.topData["QuotType"] === "A") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Verified", 500, "warning");
            return false;
        }
        if (_handler.topData["QuotType"] === "Q") {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Quoted", 500, "warning");
            return false;
        }
        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_FCLChgSetup_Scripts_302", 500, "warning");
            return false;
        }
    }
    //getSelectOptions();
    loadQtView(2);
    MenuBarFuncArr.DelMenu(["MBSearch"]);
});



