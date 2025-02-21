var $MainGrid;
var _fields = [];
function _checkEdit(type) {
    if (type !== 2) {
        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_Connot", 500, "warning");
            return false;
        }
    }
    if (ioflag == "O") {
        $("#LspCd").attr('disabled', true);
        $("#LspCdLookup").attr('disabled', true);
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
    function getcityop1(name) {
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
                    return map.PortCd;
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
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyTypeNoAuto(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.PARTY_NO, "lookup");
                $(elem).val(rd.PARTY_NO);
            }), {
                param: ""
                //baseConditionFunc: function () {
                //    return "PARTY_TYPE='CA'";
                //}
            });
        return cust_op;
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
                var tranType = $("#TranType").val();
                return "TRAN_MODE@@" + tranType;
                //return " TRAN_MODE IN ('" + tranType + "','O')";
            }), {
                param: "",
                baseConditionFunc: function () {
                    var tranType = $("#TranType").val();
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
            }, LookUpConfig.GetCurAuto(groupId, "", $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
                $(elem).val(rd.CUR);
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
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', sorttype: 'string', width: 120, hidden: true, editable: true },
        { name: 'PolCd', title: '@Resources.Locale.L_AirQuery_PolCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PolNm', title: '@Resources.Locale.L_AirQuery_PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: false, hidden: false },
       { name: 'PodCd', title: '@Resources.Locale.L_AirQuery_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop1("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'PodNm', title: '@Resources.Locale.L_DTQuery_PodNm', index: 'PodNm', sorttype: 'string', width: 180, align: 'left', editable: false, hidden: false },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },

        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },
        { name: 'F3', title: '@Resources.Locale.L_FCLFSetup_F3', index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: true },


        { name: 'F1', title: '20"@Resources.Locale.L_OTManage_foot', index: 'F1', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'F2', title: '40"@Resources.Locale.L_OTManage_foot', index: 'F2', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
         { name: 'ChgType', title: '@Resources.Locale.L_SMCHGSetup_ChgType', index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: '@Resources.Locale.L_QTSetup_chgCd', index: 'Repay', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: '@Resources.Locale.L_FCLChgSetup_Script_166' }, edittype: 'select' },
        { name: 'IsShare', title: '@Resources.Locale.L_QTManage_IsShare', index: 'IsShare', sorttype: 'string', width: 70, hidden: false, editable: true, formatter: "select", editoptions: { value: ': ;Y:Y' }, edittype: 'select' },
        //{ name: 'ChgType', title: '费用别', index: 'ChgType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'M:M.必收費用;C:C.發生才收;Y:Y.代收代付' }, edittype: 'select' },
         { name: 'Tt', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'Tt', width: 60, align: 'right', formatter: 'integer', hidden: false, editable: true },
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
        colModel: colModel, caption: '@Resources.Locale.L_QuotTrailerSetup_Scripts_316', delKey: ["UId"], height: 300,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($MainGrid, rowid, { "PodCd": true, "Cur": true });
            setGridVal($MainGrid, rowid, "PolCd", $("#Rlocation").val(), "lookup");
            setGridVal($MainGrid, rowid, "PolNm", $("#RlocationNm").val(), null);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });

    if (ioflag == "O") {
        $("#LspCd").attr('disabled', true);
        $("#LspCdLookup").attr('disabled', true);
        $("#LspCd").attr('readonly', 'readonly');
        $("#LspCdLookup").attr('readonly', 'readonly');
    }
}

var lookUpConfig = {
    caption: "@Resources.Locale.L_MenuBar_Search",
    sortname: "CreateDate",
    refresh: false,
    columns: [
            { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
            { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', width: 200, align: 'left', sorttype: 'string', hidden: false },
            { name: 'QuotDate', title: '@Resources.Locale.L_QTQuery_QuotDate', index: 'QuotDate', editable: false, formatter: 'date', editoptions: myEditDateInit, formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: null }, width: 130, align: 'left', sorttype: 'string', hidden: false },
            { name: 'RlocationNm', title: '@Resources.Locale.L_RQQuery_Rlocation', index: 'RlocationNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
            { name: 'LspNm', title: '@Resources.Locale.L_AirSetup_LspCd', index: 'LspNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
            { name: 'EffectFrom', title: '@Resources.Locale.L_RQQuery_EffectFrom', index: 'EffectFrom', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
            { name: 'EffectTo', title: '@Resources.Locale.L_RQQuery_EffectTo', index: 'EffectTo', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } }
    ]
}

$(function () {
    intQtView();
    initQTGrid();
    _handler.inquiryUrl = rootPath + "QTManage/GetFCLCHGData";
    _handler.config = lookUpConfig;

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
        //$("#Rlocation").parent().find("button").attr('disabled', true);
        if (!isEmpty(_handler.topData["LspCd"])) {
            $("#LspCd").attr('disabled', true);
            $("#LspCd").parent().find("button").attr('disabled', true);
        }
    }


    _handler.beforLoadView = function () {
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        var requires = ["QuotNo", "EffectFrom", "EffectTo", "Rlocation", "Cur", "TranType", "LspCd"];
        var readonlys = ["QuotDate", "RlocationNm", "LspNm", "QuotType"];

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
        var data = { "TranMode": "C", "TranType": "F", QuotType: "P", "Period": "B", "OutIn": "O", "FreightTerm": "PP", LspCd: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        //$("#Rlocation").blur();
        $("#LspCd").blur();
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");



        MenuBarFuncArr.EndFunc = function () {
            if (ioflag == "O") {
                $("#LspCd").attr('disabled', true);
                $("#LspCdLookup").attr('disabled', true);
                $("#LspCd").attr('readonly', 'readonly');
                $("#LspCdLookup").attr('readonly', 'readonly');
            }
        }
    }

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PolCd", index: 8, text: 'POL' });
        nullCols.push({ name: "PodCd", index: 10, text: 'POD' });
        nullCols.push({ name: "ChgCd", index: 12, text: '@Resources.Locale.L_SMCHGSetup_ChgDescp' });
        nullCols.push({ name: "ChgType", index: 18, text: '@Resources.Locale.L_QTSetup_chgCd' });
        var result = _handler.checkData($MainGrid, nullCols, sameCols);
        if (result === false) return false;
        //return true;
        return checkDoubleData($MainGrid);
    }

    function checkDoubleData($grid) {
        var rowIds = $grid.getDataIDs();
        var checkKey = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
            var key = rowDatas["PolCd"] + "#" + rowDatas["PodCd"] + "#" + rowDatas["ChgCd"] + "#" + rowDatas["ChgType"];
            for (var x = 0; x < checkKey.length; x++) {
                if (key === checkKey[x]) {
                    try {
                        $grid.jqGrid("editCell", rowIds[i], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", "@Resources.Locale.L_QuotTrailerSetup_Script_175", 2000, "warning");
                    return false;
                }
            }
            for (var j = 0; j < rowIds.length; j++) {
                if (rowIds[i] === rowIds[j])
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                var key1 = rowDatas1["Carrier"] + "#" + rowDatas1["PodCd"] + "#" + rowDatas1["ChgCd"];
                if (key1 === key) {
                    try {
                        $grid.jqGrid("editCell", rowIds[j], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", "@Resources.Locale.L_QuotTrailerSetup_Script_175", 2000, "warning");
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
        //data["term"] = encodeURIComponent($("#Incoterm").val());
        //data["mode"] = "1";
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
        if (_handler.topData.QuotType == "Q")
            MenuBarFuncArr.Disabled(["MBEdit"]);
        $("#CloseBackWin").removeAttr("disabled");
    }

    registBtnLookup($("#LspCdLookup"), {
        item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspCd").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
    }));

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

    //registBtnLookup($("#RlocationLookup"), {
    //    item: '#Rlocation', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#Rlocation").val(map.PartyNo);
    //        $("#RlocationNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#RlocationNm").val(rd.PARTY_NAME);
    //}));

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
        $("#QuotDate").val("");
        $("#EffectFrom").val(getDate(0, "-"));
        $("#EffectTo").val(getDate(365, "-"));
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");
        //_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#ApproveBy").val("");
        $("#ApproveDate").val("");
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
    //getSelectOptions();
    loadQtView(2);
    MenuBarFuncArr.DelMenu(["MBSearch"]);

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
    if (isa == "Y") {
        MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBCopy", "MBSave", "MBAdd", "MBDel", "VoidBtn", "QuotTypeBtn", "MBPreview"]);
        MenuBarFuncArr.Disabled(["MBCancel"]);
        MenuBarFuncArr.Enabled(["btn03", "btn04", "btn06"]);
    }
});