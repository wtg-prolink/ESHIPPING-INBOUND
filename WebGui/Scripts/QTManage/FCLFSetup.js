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

    //function getcust(name) {
    //    var _name = name;
    //    var cust_op = getLookupOp("MainGrid",
    //        {
    //            url: rootPath + LookUpConfig.PartyNoUrl,
    //            config: LookUpConfig.PartyNoLookup,
    //            returnFn: function (map, $grid) {
    //                return map.PartyNo;
    //            }
    //        }, LookUpConfig.GetPartyTypeNoAuto(groupId,undefined, $MainGrid,
    //        function ($grid, rd, elem, rowid) {
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
            }, LookUpConfig.GetChgAuto1(groupId, undefined , $MainGrid,
            function ($grid, rd, elem, rowid) {
              
                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, null);
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                $(elem).val(rd.CHG_CD);
            }, function () {
                var tranType = $("#TranMode").val();
                return " TRAN_MODE@@" + tranType;
            }), {
                param: "",
                baseConditionFunc: function () {
                    var tranType = $("#TranMode").val();
                    return "TRAN_MODE IN ('" + tranType + "','O')";
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
            }, LookUpConfig.GetCurAuto(groupId,undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CUR);
                setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
            }), { param: "" });
        return cur_op;
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
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', editoptions: gridLookup(getcust("Carrier")), edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'PodCd', title: '@Resources.Locale.L_FCLFSetup_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PodNm', title: '@Resources.Locale.L_FCLFSetup_PodCd'+" Name", index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: false },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgType', index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 100, align: 'left', hidden: false, editable: false },
        { name: 'F3', title: '@Resources.Locale.L_FCLFSetup_F3', index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: 'FCL', delKey: ["UId"], height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($MainGrid, rowid, { "PodCd": true });
            //setGridVal($MainGrid, rowid, "Carrier", cmp);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });
}

$(function () {
    intQtView();
    initQTGrid();

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        //nullCols.push({ name: "Carrier", index: 10, text: 'Carrier' });
        nullCols.push({ name: "ChgType", index: 14, text: '@Resources.Locale.L_SMCHGSetup_ChgRepay' });
        nullCols.push({ name: "PodCd", index: 10, text: '@Resources.Locale.L_FCLFSetup_PodCd' });
        nullCols.push({ name: "ChgCd", index: 12, text: '@Resources.Locale.L_QTSetup_chgCd' });
        nullCols.push({ name: "Punit", index: 15, text: '@Resources.Locale.L_AirQuery_Punit' });
        return _handler.checkData($MainGrid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
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

    //registBtnLookup($("#RlocationLookup"), {
    //    url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#Rlocation").val(map.PartyNo);
    //        $("#RlocationNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#RlocationNm").val(rd.PARTY_NAME);
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

    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: "123", GROUP_ID: "*", CMP: "SD", STN: "FQ" });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        getAutoNo("RfqNo", "rulecode=RFQ_NO&cmp=" + cmp + "&stn=" + stn);
        var data = { "FreightTerm": _bu, "TranMode": _tran };
        data[_handler.key] = uuid();
        $("#Status").val("A");
        /*var allData = $MainGrid.jqGrid("getGridParam", "data");
        $.each(allData, function (i, val) {
            val["_id_"] = "jqg" + i;
        });*/
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }
    getSelectOptions();
    loadQtView();
});



