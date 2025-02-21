var $MainGrid;
function initQTGrid() {
    $MainGrid = $("#MainGrid");
    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (_name === "PolCd")
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    setGridVal($grid, selRowId, _name, map.CntryCd + map.PortCd, "lookup");
                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
                function ($grid, rd, elem, rowid) {
                    $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
                    if (_name === "PolCd")
                        setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                    else
                        setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                    setGridVal($grid, rowid, _name, rd.CNTRY_CD + rd.PORT_CD, "lookup");
                }), { param: "" });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var unit_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TALNUrl,
                config: LookUpConfig.BSCodeLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TALN", $MainGrid,
                function ($grid, rd, elem, rowid) {
                    $(elem).val(rd.CD);
                }), { param: "" });
        return unit_op;
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
            }, LookUpConfig.GetCurAuto(groupId, "", $MainGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
                    $(elem).val(rd.CUR);
                }), { param: "" });
        return cur_op;
    }

    //function getcust(name) {
    //    var _name = name;
    //    var cust_op = getLookupOp("MainGrid",
    //        {
    //            url: rootPath + LookUpConfig.PartyNoUrl,
    //            config: LookUpConfig.PartyNoLookup1,
    //            returnFn: function (map, $grid) {
    //                return map.PartyNo;
    //            }
    //        }, LookUpConfig.GetPartyTypeNoAuto(groupId,undefined, $MainGrid,
    //        function ($grid, rd, elem,rowid) {
    //            setGridVal($grid, rowid, _name, rd.PARTY_NO,"lookup");
    //            $(elem).val(rd.PARTY_NO);
    //        }), {
    //            param: ""
    //            //baseConditionFunc: function () {
    //            //    return "PARTY_TYPE='CA'";
    //            //}
    //        });
    //    return cust_op;
    //}

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
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    return map.ChgCd;
                }
            }, LookUpConfig.GetChgAuto1(groupId, undefined, $MainGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                    setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, null);
                    setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                    $(elem).val(rd.CHG_CD);
                }, function () {
                    var tranType = _handler.topData["TranMode"];
                    var OutIn = _handler.topData["OutIn"];
                    return "TRAN_MODE@@" + tranType + ";O&IO_TYPE=" + OutIn;
                }), {
            param: "",
            baseConditionFunc: function () {
                var tranType = _handler.topData["TranMode"];
                var OutIn = _handler.topData["OutIn"];
                return "TRAN_MODE IN ('" + tranType + "','O') AND IO_TYPE='" + OutIn + "'";
            }
        });
        return chg_op;
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

    function getTermop(name) {
        var _name = name;
        var term_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TermUrl,
                config: LookUpConfig.TermLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TD", $MainGrid,
                function ($grid, rd, elem) {
                }), { param: "" });
        return term_op;
    }

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: '@Resources.Locale.L_AirQuery_AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: '@Resources.Locale.L_AirQuery_LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: false, editoptions: gridLookup(getTermop("Incoterm")), edittype: 'custom' },
        { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolCd', title: '@Resources.Locale.L_AirBookingSetup_PolCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PolNm', title: '@Resources.Locale.L_AirBookingSetup_PolCd' + " Name", index: 'PolNm', sorttype: 'string', width: 150, editable: false, hidden: false },
        { name: 'PodCd', title: '@Resources.Locale.L_AirSetup_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PodNm', title: '@Resources.Locale.L_AirSetup_PodCd' + " Name", index: 'PodNm', sorttype: 'string', width: 150, editable: false, hidden: false },
        { name: 'Carrier', title: '@Resources.Locale.L_AirQuery_Carrier', index: 'Carrier', editoptions: gridLookup(getcust("Carrier")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 80, hidden: false, editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgType', index: 'ChgType', sorttype: 'string', width: 130, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Tt', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'Tt', width: 60, align: 'right', formatter: 'integer', hidden: false, editable: true },
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 100, align: 'left', hidden: false, editable: true },
        { name: 'MinAmt', title: '@Resources.Locale.L_AirSetup_MinAmt', index: 'MinAmt', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'AllIn', title: '@Resources.Locale.L_AirSetup_AllIn', index: 'AllIn', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true, formatter: "select", editoptions: { value: 'Y:Yes;N:No' }, edittype: 'select' },
        { name: 'F1', title: "-45", index: 'F1', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F2', title: "+45", index: 'F2', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F3', title: "+100", index: 'F3', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F4', title: "+300", index: 'F4', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F5', title: "+500", index: 'F5', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F6', title: "+1000", index: 'F6', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F7', title: "+2000", index: 'F7', width: 60, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 200, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        { name: 'ContractNo', title: '@Resources.Locale.L_QtManage_ContractNo', index: 'ContractNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];


    $.ajax({
        async: false,
        url: rootPath + "QTManage/GetTransTypeInfo",
        type: 'POST',
        data: { VenderCd: _LspCd, type: "A", qtId: _uid },
        async: false,
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            if (result.message !== "success")
                return;

            var transTypeCols = result.chgTypeStr.split(";");
            var transTypeColsN = result.chgTypeColsStr.split(";");
            if (transTypeCols.length <= 0)
                return;

            var f_cols = {};
            for (var i = 0; i < 100; i++) {
                f_cols["F" + i] = "Y";
            }
            var f_start = 0;
            for (var i = 0; i < colModel.length; i++) {
                if (f_cols[colModel[i].name] === "Y") {
                    if (f_start == 0)
                        f_start = i;
                    colModel.splice(i, 1);
                    i--;
                }
            }

            $.each(transTypeCols, function (index, val) {
                var item = { name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false };
                if (f_start > 0)
                    colModel.splice(f_start, 0, item);
                else
                    colModel.push(item);
                f_start++;
            });
        }
    });

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_QTManage_AirQuo', delKey: ["UId"], height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($MainGrid, rowid, { "PolCd": true, "PodCd": true });
            //setGridVal($MainGrid, rowid, "Carrier",cmp);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
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
        nullCols.push({ name: "PolCd", index: 7, text: 'Origin' });
        nullCols.push({ name: "PodCd", index: 9, text: 'Destination' });
        //nullCols.push({ name: "Carrier", index: 11, text: 'Airline' });
        nullCols.push({ name: "ChgType", index: 14, text: '@Resources.Locale.L_SMCHGSetup_ChgType' });
        nullCols.push({ name: "Punit", index: 16, text: '@Resources.Locale.L_AirQuery_Punit' });
        return _handler.checkData($MainGrid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
        data["contractType"] = encodeURIComponent($("#ContractType").val());
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

    //registBtnLookup($("#IncoTermLookup"), {
    //    item: '#IncoTerm', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
    //        $("#IncoTerm").val(map.Cd);
    //    }
    //}, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {

    //}));

    //registBtnLookup($("#PolCdLookup"), {
    //    item: '#PolCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#PolCd").val(map.CntryCd + map.PortCd);
    //        $("#PolName").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#PolName").val(rd.PORT_NM);
    //}));

    //registBtnLookup($("#PodCdLookup"), {
    //    item: '#PodCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#PodCd").val(map.CntryCd + map.PortCd);
    //        $("#PodName").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#PodName").val(rd.PORT_NM);
    //}));

    //registBtnLookup($("#CurLookup"), {
    //    item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
    //        $("#Cur").val(map.Cur);
    //    }
    //}, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $(elem).val(rd.CUR);
    //}));
    getSelectOptions();
    loadQtView();
});

