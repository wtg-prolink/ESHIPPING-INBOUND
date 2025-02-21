var $grid, $grid2;
function initQTGrid() {
    $grid = $("#MainGrid");
    $grid2 = $("#SubGrid2");

    function getTermop(name) {
        var _name = name;
        var term_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TrackWayUrl,
                config: LookUpConfig.TrackWayLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TDTK", $grid,
            function ($grid, rd, elem) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return term_op;
    }

    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TruckPortCdUrl,
                config: LookUpConfig.TruckPortCdLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (_name === "PolCd")
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else {
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                        setGridVal($grid, selRowId, "Region", map.Region, null);
                        setGridVal($grid, selRowId, "State", map.State, null);
                    }
                    setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                    return map.PortCd;
                }
            }, LookUpConfig.TruckPortCdAuto(groupId, $grid,
            function ($grid, rd, elem, rowid) {
                if (_name === "PolCd")
                    setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                else {
                    setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                    setGridVal($grid, rowid, "Region", rd.REGION, null);
                    setGridVal($grid, rowid, "State", rd.STATE, null);
                }
                //$(elem).val(rd.CNTRY_CD + rd.PORT_CD);
                setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
            }), { param: "" });
        return city_op;
    }

    function getTranModeop(name) {
        var _name = name;
        var tranmode_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TrackingTranModeUrl,
                config: LookUpConfig.TranModeLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TDTK", $grid,
            function ($grid, rd, elem) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return tranmode_op;
    }

    function get_cntryop(name) {
        var _name = name;
        var cntry_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CountryUrl,
                config: LookUpConfig.CountryLookup,
                returnFn: function (map, $grid) {
                    //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return map.CntryCd;
                }
            }, LookUpConfig.GetCountryAuto(groupId, $grid,
            function ($grid, rd, elem) {
                $(elem).val(rd.CNTRY_CD);
            }), { param: "" });
        return cntry_op;
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
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $grid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
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
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    return map.ChgCd;
                }
            }, LookUpConfig.GetChgAuto1(groupId, undefined, $grid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                $(elem).val(rd.CHG_CD);
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "TRAN_MODE IN ('D','O')";
                }
            });
        return chg_op;
    }

    function get_regionop(name) {
        var _name = name;
        var region_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TrgnUrl,
                config: LookUpConfig.TrgnLookup,
                returnFn: function (map, $grid) {
                    //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", $grid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return region_op;
    }
   
    var colModel = [
       { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'TranType', title: '@Resources.Locale.L_UserSetUp_TranType', index: 'TranType', editable: true, width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: select_trantype }, edittype: 'select' },
       //{ name: 'TranType', title: '@Resources.Locale.L_UserSetUp_TranType', index: 'TranType', editoptions: gridLookup(getTranModeop("TranType")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
       { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'LspCd', title: '@Resources.Locale.L_AirQuery_LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'AllIn', title: '@Resources.Locale.L_AirSetup_AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: false, hidden: true },
       { name: 'PolCd', title: '@Resources.Locale.L_AirQuery_PolCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PolNm', title: '@Resources.Locale.L_AirQuery_PolNm', index: 'PolNm', sorttype: 'string', width: 150, hidden: false },
       //{ name: 'Region', title: '@Resources.Locale.L_DTQuery_Region', index: 'Region', editoptions: gridLookup(get_regionop("Region")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       //{ name: 'State', title: '@Resources.Locale.L_DTQuery_State', index: 'State', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PodCd', title: '@Resources.Locale.L_AirQuery_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PodNm', title: '@Resources.Locale.L_DTQuery_PodNm', index: 'PodNm', sorttype: 'string', width: 150, hidden: false },
        { name: 'Region', title: '@Resources.Locale.L_DTQuery_Region', index: 'Region', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _selectTRGN }, edittype: 'select' },
       { name: 'State', title: '@Resources.Locale.L_DTQuery_State', index: 'State', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _selectSTATE }, edittype: 'select' },
       { name: 'Tt', title: '@Resources.Locale.L_DTQuery_Tt', index: 'Tt', width: 100, align: 'right', formatter: 'integer', hidden: false, editable: true },
       //{ name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
       //{ name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'BL:BL;CBM:CBM;T:T;车:车;GW:GW' }, edittype: 'select' },
       { name: 'F10', title: "@Resources.Locale.L_RQSetup_ChrWeight", index: 'F10', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, idden: false, editable: true },
       { name: 'F1', title: "@Resources.Locale.L_DTQuery_F1", index: 'F1', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, idden: false, editable: true },
       { name: 'F2', title: "@Resources.Locale.L_DTQuery_F2", index: 'F2', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F3', title: "@Resources.Locale.L_DTQuery_F3", index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F4', title: "@Resources.Locale.L_DTQuery_F4", index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F5', title: "@Resources.Locale.L_DTQuery_F5", index: 'F5', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F6', title: "@Resources.Locale.L_DTQuery_F6", index: 'F6', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F7', title: "@Resources.Locale.L_DTQuery_F7", index: 'F7', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F8', title: "@Resources.Locale.L_DTQuery_F8", index: 'F8', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'F9', title: "@Resources.Locale.L_ContractQuery_TFVehicle", index: 'F9', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
       { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
       { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];

    function getUnit1(name) {
        var _name = name;
        var unit_op = getLookupOp("SubGrid2",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $grid2,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return unit_op;
    }

    function getchg1(name) {
        var _name = name;
        var chg_op = getLookupOp("SubGrid2",
            {
                url: rootPath + LookUpConfig.ChgUrl,
                config: LookUpConfig.ChgLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    setGridVal($grid, selRowId, "ChgDescp", map.ChgDescp);
                    return map.ChgCd;
                }
            }, LookUpConfig.GetChgAuto1(groupId, undefined, $grid2,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP);
                $(elem).val(rd.CHG_CD);
            }, function () {
                return "TRAN_MODE@@T";
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "";
                }
            });
        return chg_op;
    }

    var colModel2 = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', editoptions: gridLookup(getchg1("ChgCd")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F1', title: '@Resources.Locale.L_DTSetup_F1', index: 'F1', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit1("Punit")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];


    _handler.intiGrid("MainGrid", $grid, {
        colModel: colModel, caption: '@Resources.Locale.L_QTSetup_DmEpQuo', delKey: ["UId"], height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $grid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($grid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($grid, rowid, { "PolCd": true, "PodCd": true,"TranMode":true });
            setGridVal($grid, rowid, "ChgCd", "FRT","lookup");
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改 
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$SubGrid.setColProp('DocType', { editable: true });
        }
    });

    _handler.intiGrid("SubGrid2", $grid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_QTSetup_AddValue', delKey: ["UId"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $grid2.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($grid2, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($grid2, rowid);
        },
        beforeSelectRowFunc: function (rowid) {

        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$SubGrid.setColProp('DocType', { editable: true });
        }
    });
}

$(function () {
    intQtView();
    initQTGrid();

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "TranType", index: 2, text: '@Resources.Locale.L_BaseLookup_TranMode' });
        nullCols.push({ name: "PolCd", index: 10, text: '@Resources.Locale.L_AirQuery_PolCd' });
        nullCols.push({ name: "Region", index: 14, text: '@Resources.Locale.L_DTQuery_Region' });
        nullCols.push({ name: "State", index: 15, text: '@Resources.Locale.L_DTQuery_State' });
        nullCols.push({ name: "PodCd", index: 12, text: '@Resources.Locale.L_AirQuery_PodCd' });
        //nullCols.push({ name: "Punit", index: 17, text: '計費單位' });
        if (_handler.checkData($grid, nullCols, sameCols) === false)
            return false;

        nullCols = [];
        sameCols = [];
        nullCols.push({ name: "ChgCd", index: 11, text: '@Resources.Locale.L_SMCHGSetup_ChgCd' });
        nullCols.push({ name: "Punit", index: 14, text: '@Resources.Locale.L_ActSetup_ChgUnit' });
        return _handler.checkData($grid2, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $grid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub1"] = $grid2.jqGrid('getGridParam', "arrangeGrid")();
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
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
            _handler.loadGridData("MainGrid", $grid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $grid[0], [], [""]);

        if (data["sub1"])
            _handler.loadGridData("SubGrid2", $grid2[0], data["sub1"], [""]);
        else
            _handler.loadGridData("SubGrid2", $grid2[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        var multiEdocData = [
        { jobNo: _handler.topData["RqUid"], 'GROUP_ID': _handler.topData["RqGroupid"], 'CMP': _handler.topData["RqCmp"], 'STN': '*' }
        ];
        //alert(_handler.topData["RqUid"] + _handler.topData["RqGroupid"] + _handler.topData["RqCmp"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);

        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
        MenuBarFuncArr.Enabled(["VoidBtn"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
        setRQData(data);
    }

    //registBtnLookup($("#LspCdLookup"), {
    //    item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#LspCd").val(map.PartyNo);
    //        $("#LspNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#LspNm").val(rd.PARTY_NAME);
    //}));

    //registBtnLookup($("#CurLookup"), {
    //    item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
    //        $("#Cur").val(map.Cur);
    //    }
    //}, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $(elem).val(rd.CUR);
    //}));

    loadQtView();
});


