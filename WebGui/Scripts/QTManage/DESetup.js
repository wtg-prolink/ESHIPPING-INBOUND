var $MainGrid;
function initQTGrid() {
    $MainGrid = $("#MainGrid");
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
                    else
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return  map.PortCd;
                }
            }, LookUpConfig.TruckPortCdAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                if (_name === "PolCd")
                    setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                else
                    setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                $(elem).val(rd.PORT_CD);
                setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
            }), { param: "" });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var cust_op = getLookupOp("MainGrid",
            {
                url: rootPath + "Common/GetCompanyData",
                config: LookUpConfig.CmpLookup,
                returnFn: function (map, $grid) {
                    return map.Cmp;
                }
            }, LookUpConfig.GetCmpAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.CMP, "lookup");
                $(elem).val(rd.CMP);
            }), { param: "" });
        return cust_op;
    }

    var colModel = [
       { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'LspCd', title: '@Resources.Locale.L_AirQuery_LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'AllIn', title: '@Resources.Locale.L_AirQuery_AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'PolCd', title: '@Resources.Locale.L_DEQuery_PolCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PolNm', title: '@Resources.Locale.L_DEQuery_PolCd'+" Name", index: 'PolNm', sorttype: 'string', width: 100, editable: false, hidden: false },
       { name: 'PodCd', title: '@Resources.Locale.L_DEQuery_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PodNm', title: '@Resources.Locale.L_DEQuery_PodCd' + " Name", index: 'PodNm', sorttype: 'string', width: 180, align: 'left', editable: false, hidden: false },
       { name: 'Tt', title: '@Resources.Locale.L_DEQuery_Tt', index: 'Tt', width: 80, align: 'right', formatter: 'integer', editable: true, hidden: false },
       { name: 'F1', title: '@Resources.Locale.L_DEQuery_F1', index: 'F1', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'F2', title: '@Resources.Locale.L_DEQuery_F2', index: 'F2', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'F3', title: '@Resources.Locale.L_DEQuery_F3', index: 'F3', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'F4', title: '@Resources.Locale.L_DEQuery_F4', index: 'F4', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
       { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_RQSetup_DomesticEpQuoDL', delKey: ["UId"],height:200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($MainGrid, rowid, "ChgCd", "FRT");
            setDefutltGridData($MainGrid, rowid, { "PodCd": true,"PolCd":true });
            setGridVal($MainGrid, rowid, "Carrier", cmp,"lookup");
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
        nullCols.push({ name: "PolCd", index: 10, text: 'Location' });
        nullCols.push({ name: "PodCd", index: 11, text: '@Resources.Locale.L_DEQuery_PodCd' });
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
