$(function () {
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $SubGrid3 = $("#SubGrid3");
    _handler.saveUrl = rootPath + "/";
    _handler.inquiryUrl = rootPath + LookUpConfig.ShipmentLookup;
    _handler.config = LookUpConfig.ShipmentUrl;

    _handler.addData = function () {
     
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
       
    }

    _handler.beforSave = function () {
        var $grid = $SubGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        nullCols.push({ name: "PartyName", index: 3, text: 'PartyName' });
        sameCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        return _handler.checkData($grid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
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
        if (data["con"])
            _handler.loadGridData("SubGrid2", $SubGrid2[0], data["con"], [""]);
        else
            _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);

        if (data["status"])
            _handler.loadGridData("SubGrid3", $SubGrid3[0], data["status"], [""]);
        else
            _handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        //MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        MenuBarFuncArr.initEdoc($("#OUid").val(), _handler.topData["GroupId"], $("#OLocation").val(), "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + LookUpConfig.ShipmentItemUrl, { uId: map.UId, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    function getop(name) {
        var _name = name;
        var city_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.PartyTypeUrl,
                config: LookUpConfig.PartyTypeLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "PT", $SubGrid, function ($grid, rd, elem) {
                $("#PartyType").val(rd.CD);
            }), {
            param: "",
            baseConditionFunc: function () {
                return "";
            }
        });
        return city_op;
    }


    function getpartyop(name) {
        var _name = name;
        var city_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (returnObj, $grid) {
                    var rowid = $grid.jqGrid('getGridParam', 'selrow');
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartyName", "lookup", returnObj.PartyName);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartyMail", "lookup", returnObj.PartyMail);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartAddr1", "lookup", returnObj.PartAddr1);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartAddr2", "lookup", returnObj.PartAddr2);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartAddr3", "lookup", returnObj.PartAddr3);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartyAttn", "lookup", returnObj.PartyAttn);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartyType", "lookup", returnObj.PartyType);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "PartyTel", "lookup", returnObj.PartyTel);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "DebitTo", "lookup", returnObj.DebitTo);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "Cnty", "lookup", returnObj.Cnty);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "CntyNm", "lookup", returnObj.CntyNm);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "City", "lookup", returnObj.City);
                    $grid.jqGrid('getGridParam', "setGridCellValueCustom")("SubGrid", rowid, "CityNm", "lookup", returnObj.CityNm);

                    return returnObj.PartyNo;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $SubGrid,
            function ($grid, rd, elem) {
                var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                setGridVal($grid, selRowId, 'PartyName', rd.PARTY_NAME, 'PartyName');
                setGridVal($grid, selRowId, 'PartyMail', rd.PARTY_MAIL, 'PartyMail');
                setGridVal($grid, selRowId, 'PartAddr1', rd.PARTY_ADDR1, 'PartAddr1');
                setGridVal($grid, selRowId, 'PartAddr2', rd.PARTY_ADDR2, 'PartAddr2');
                setGridVal($grid, selRowId, 'PartAddr3', rd.PARTY_ADDR3, 'PartAddr3');
                setGridVal($grid, selRowId, 'PartyAttn', rd.PARTY_ATTN, 'PartyAttn');
                setGridVal($grid, selRowId, 'PartyType', rd.PARTY_TYPE, 'PartyType');
                setGridVal($grid, selRowId, 'PartyTel', rd.PARTY_TEL, 'PartyTel');
                setGridVal($grid, selRowId, 'PartyNo', rd.PARTY_NO, 'PartyNo');
                //$(elem).val(rd.PartyNo);
            }));
        return city_op;
    }

    var colModel1 = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        //{ name: 'JobNo', title: 'Shipment ID', index: 'JobNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartyType', title: '@Resources.Locale.L_DNDetailVeiw_PartyType', index: 'PartyType', editoptions: gridLookup(getop("")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'TypeDescp', title: '@Resources.Locale.L_DNDetailVeiw_TypeDescp', index: 'TypeDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'OrderBy', title: '@Resources.Locale.L_DNDetailVeiw_OrderBy', index: 'OrderBy', sorttype: 'string', width: 80, hidden: true, editable: true },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNo', index: 'PartyNO', editoptions: gridLookup(getpartyop("PartyName")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartyName', title: '@Resources.Locale.L_DNDetailVeiw_PartyName', index: 'PartyName', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'PartyAddr', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr', index: 'PartAddr', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'PartyAttn', title: '@Resources.Locale.L_BSCSDataQuery_PartyAttn', index: 'PartyAttn', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartyTel', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'PartyTel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartyMail', title: '@Resources.Locale.L_DNDetailVeiw_Mail', index: 'PartyMail', sorttype: 'string', width: 250, hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel1, caption: 'Party', delKey: ["UId", "PartyType"]
        
    });

    var colModel2 = [
       { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
       { name: 'SeqNo', title: 'SEQ NO', index: 'SeqNo', sorttype: 'string', width: 100, hidden: true, editable: true },
       { name: 'CntrNo', title: '@Resources.Locale.L_GateReserve_CntrNo', index: 'CntrNo', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'SealNo1', title: '@Resources.Locale.L_GateReserveSetup_SealNo 1', index: 'SealNo1', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'SealNo2', title: '@Resources.Locale.L_GateReserveSetup_SealNo 2', index: 'SealNo2', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'CntrType', title: '@Resources.Locale.L_BaseLookup_CntType', index: 'CntrType', sorttype: 'string', width: 80, hidden: false, editable: true },
       { name: 'LoadingFrom', title: '@Resources.Locale.L_TKBLQuery_LoadingFrom', index: 'LoadingFrom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'LoadingTo', title: '@Resources.Locale.L_TKBLQuery_LoadingFrom', index: 'LoadingTo', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'Qty', title: '@Resources.Locale.L_ForecastQueryData_Qty', index: 'Qty', align: 'right', formatter: 'integer', width: 100, hidden: false, editable: true },
       { name: 'Gw', title: '@Resources.Locale.L_BaseLookup_Gw', index: 'Gw', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: true },
       { name: 'Cbm', title: '@Resources.Locale.L_ForecastQueryData_Cbm', index: 'Cbm', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: true },
       { name: 'PtruckNo', title: '@Resources.Locale.L_AirTransport_PtruckNo', index: 'PtruckNo', sorttype: 'string', width: 150, hidden: false, editable: true },
       { name: 'DtruckNo', title: '@Resources.Locale.L_AirTransport_DtruckNo', index: 'DtruckNo', sorttype: 'string', width: 150, hidden: false, editable: true }];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_TKBLQuery_CntrDetInf', delKey: ["UId", "PartyType"]
    });

    var colModel3 = [
     { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
     { name: 'StsCd', title: '@Resources.Locale.L_AirTransport_StsCd', index: 'StsCd', sorttype: 'string', width: 100, hidden: false, editable: true },
     { name: 'StsDescp', title: '@Resources.Locale.L_AirTransport_StsDescp', index: 'StsDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
     { name: 'EvenDate', title: '@Resources.Locale.L_AirTransport_EvenDate', index: 'EvenDate', sorttype: 'string', width: 150, hidden: false, editable: true },
     { name: 'EvenTmg', title: '@Resources.Locale.L_AirTransport_EvenTmg', index: 'EvenTmg', sorttype: 'string', width: 170, hidden: false, editable: true },
     { name: 'CntrNo', title: 'Container No', index: 'CntrNo', sorttype: 'string', width: 120, hidden: false, editable: true },
     { name: 'Location', title: '@Resources.Locale.L_AirTransport_Location', index: 'Location', sorttype: 'string', width: 100, hidden: false, editable: true },
     { name: 'LocationDescp', title: 'Location Name', index: 'LocationDescp', sorttype: 'string', width: 120, hidden: false, editable: true },
     { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'CreateDate', title: 'Send Time', index: 'CreateDate', sorttype: 'string', width: 150, hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid3", $SubGrid3, {
        colModel: colModel3, caption: '@Resources.Locale.L_TKBLQuery_StsDetInf', delKey: ["UId", "PartyType"]
    });

    registBtnLookup($("#PartyTypeLookup"), {
        url: rootPath + LookUpConfig.PartyTypeUrl, config: LookUpConfig.PartyTypeLookup, param: "", selectRowFn: function (map) {
            $("#PartyType").val(map.Cd);
            $("#PartyDescp").val(map.CdDescp);
        }
    });

    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: _uid, GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }

    _initUI(["MBAdd", "MBDel", "MBCopy", "MBApply", "MBApprove", "MBInvalid"]);//初始化UI工具栏

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBApprove", "MBInvalid", "MBEdit", "MBSave", "MBCancel", "MBErrMsg"]);
    MenuBarFuncArr.Enabled(["MBEdoc"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
});

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("Shipment") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var options = data.TKLC || [];
            appendSelectOption($("#Cstatus"), options);

            var tnt_options = data.TNT || [];
            appendSelectOption($("#TranType"), tnt_options);
            if (_handler.topData) {
                $("#Cstatus").val(_handler.topData["Cstatus"]);
                $("#TranType").val(_handler.topData["TranType"]);
            }
        }
    });
}
