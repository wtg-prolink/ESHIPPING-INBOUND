var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

$(function () {
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $subbddGrid = $("#subbddGrid");
    var BookingLookup = {
        caption: "@Resources.Locale.L_DNManage_BkDataSer",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
            { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Year ', title: 'Year', index: 'Year', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Weekly', title: '@Resources.Locale.L_ContainUsage_Week', index: 'Weekly', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: 'DN No', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'CntrQty', title: '@Resources.Locale.L_BaseLookup_CntNumber', index: 'CntrQty', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'PriceTerm', title: '@Resources.Locale.L_ShipmentID_PayTermCd', index: 'PriceTerm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'FreightTerm', title: 'FreightTerm', index: 'FreightTerm', width: 80, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POR', title: 'POR', index: 'POR', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POL', title: 'POL', index: 'POL', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Dest', title: 'Dest', index: 'Dest', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Feu', title: '@Resources.Locale.L_DNApproveManage_Feu', index: 'Feu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Teu', title: '@Resources.Locale.L_AirBookingSetup_Script_86', index: 'Teu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cur', title: '@Resources.Locale.L_InvPkgSetup_Cur', index: 'Cur', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cost', title: '@Resources.Locale.L_DNApproveManage_Cost', index: 'Cost', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };

    _handler.saveUrl = rootPath + "DNManage/SaveFCLBookingData";
    _handler.inquiryUrl = rootPath + "DNManage/GetFCLBookingData";
    _handler.config = BookingLookup;

    _handler.beforEdit = function () {
        var shipmentid = $("#ShipmentId").val();
        var isok = "N";
        var response = $.ajax({
            async: false,
            url: rootPath + "BookingAction/CheckEtdDate",
            type: 'POST',
            data: { shipmentid: shipmentid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                isok = data.IsOk;
            }
        });
        if ("N" == isok) {
            alert("@Resources.Locale.L_DNManage_Cannotedit1day");
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = { "FreightTerm": _bu, "TranType": _tran };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
    }

    function _setMyReadonlys() {
        var readonlys = ["ShipmentId", "CombineInfo", "Marks", "Instruction", "Goods", "CostCenter","TransacteMode","CW",
         "PpolCd", "PpolName", "PporCd", "PporName", "PpodCd", "PpodName", "PdestCd", "PdestName","DnEtd","Cur","Gvalue",
         "PaytermCd", "PaytermNm", "BlType", "FrtTerm", "PkgNum", "PkgUnit", "PkgUnitDesc", "ProductDate",
         "ScacCd", "IncotermCd", "Qty", "Qtyu", "Pgw","Pvw","Gwu", "Pcbm", "ProfileCd", "Lgoods", "BlRmk", "TradeTerm", "TradetermDescp",
         "Oexporter", "OexporterNm", "OexporterAddr", "Oimporter", "OimporterNm","CentDecl","Battery","Horn","ThirdPayment",
            "OimporterAddr", "BrokerInstr", "BrokerInfo", "ExportNo", "DeclNum", "NextNum"
        ];
        if ($("#Status").val() != "O" && $("#Status").val() != "H") {
            readonlys.push("Atd");
            readonlys.push("Ata");
        }
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
    }

    _handler.afterEdit = function () {
        SetStatusToReadOnly(_setMyReadonlys,true);
        $("input[readonly][fieldname][dt='mt']").parent().find("button").attr("disabled", true);
    }

    _handler.beforSave = function () {
        var $grid = $SubGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        nullCols.push({ name: "PartyNo", index: 3, text: 'PartyNo' });
        sameCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        var gw = $("#Gw").val();
        var cbm = $("#Cbm").val();
        var ttlcbm = cbm * 200;
        $("#Vw").val(ttlcbm);
        if (parseFloat(gw) >= ttlcbm) {
            ttlcbm = gw;
        }
        ttlcbm = (ttlcbm * 1).toFixed(4);
        var intcbm = parseInt(ttlcbm);      //取整
        var minuite = ttlcbm - intcbm;
        if (ttlcbm>intcbm){
            if (ttlcbm > 20) {
                if (minuite > 0)
                    ttlcbm = intcbm + 1;
            } else {
                if (minuite < 0.5)
                    ttlcbm = intcbm + 0.5;
                else if(minuite > 0.5)
                    ttlcbm = intcbm + 1;
            }
        }
        $("#Cw").val(ttlcbm);

        return _handler.checkData($grid, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var containerArray2 = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub2"] = containerArray2;
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, "ShipmentId": shipmentid, autoReturnData: false, "TranType": "E", "IsConfirm": "Y" },
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
        if (data["sub2"])
            _handler.loadGridData("SubGrid2", $SubGrid2[0], data["sub2"], [""]);
        else
            _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        if (data["subbdd"])
            _handler.loadGridData("subbddGrid", $subbddGrid[0], data["subbdd"], [""]);
        else
            _handler.loadGridData("subbddGrid", $subbddGrid[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        if ("B" == _handler.topData["BlType"]) {
            $("#SMBDDGRID").show();
            $("#SMDNPGRID").hide();
        } else {
            $("#SMBDDGRID").hide();
            $("#SMDNPGRID").show();
        }
        setdisabled(true);
        setToolBtnDisabled(true);
        //init edoc for get all dn and shipment edco all in one view
        var multiEdocData = [];
        ajaxHttp(rootPath + "DNManage/GetDNData", { DnNo: $("#CombineInfo").val(), loading: true },
        function (data) {
            if (data == null) {
                MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
            } else {
                $(data.dn).each(function (index) {
                    multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*' });
                });
                MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);
            }
        });
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        MenuBarFuncArr.Enabled(["MBEdoc", "MBConfirm"]);
        ChangeColor();
    }

    _handler.editData=function ()
    {
        _handler.gridEditableCtrl(false);
        return true;
    }
    //_handler.gridEditableCtrl(false);

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "DNManage/GetFCLBookingItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: returnPartyModel("SubGrid", $SubGrid), caption: 'Party', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('PartyType', { editable: true });
            } else {
                $SubGrid.setColProp('PartyType', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $SubGrid.setColProp('PartyType', { editable: true });
        }
    });

    var colModel2 = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'DnNo', title: '@Resources.Locale.L_DNApproveManage_DnNo', index: 'DnNo', sorttype: 'string', width: 90, hidden: false, editable: true },
        { name: 'PoNo', title: '@Resources.Locale.L_DNApproveManage_PoNo', index: 'PoNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', sorttype: 'string', width: 90, hidden: false, editable: true },
        { name: 'ProductLine', title: '@Resources.Locale.L_GateReserveSetup_ProLine', index: 'ProductLine', sorttype: 'string', width: 90, hidden: false, editable: true },
        { name: 'ProductDate', title: '@Resources.Locale.L_DNApproveManage_ProduceDate', index: 'ProductDate', sorttype: 'string', width: 90, hidden: false, editable: true },
        { name: 'Qty', title: '@Resources.Locale.L_BaseLookup_Qty', index: 'Qty', width: 120, align: 'right', formatter: 'integer', hidden: false },
        { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Qtyu', index: 'Qtyu', sorttype: 'string', width: 70, hidden: false, editable: true },
        { name: 'MasterNo', title: '@Resources.Locale.L_BaseLookup_MasterNo', index: 'MasterNo', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'OpartNo', title: '@Resources.Locale.L_DNApproveManage_OpartNo', index: 'OpartNo', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'GoodsDescp', title: '@Resources.Locale.L_DNApproveManage_GoodsDescp', index: 'GoodsDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'Resolution', title: '@Resources.Locale.L_DNApproveManage_Resolution', index: 'Resolution', sorttype: 'string', width: 70, hidden: false, editable: true },
        { name: 'Ul', title: '@Resources.Locale.L_FCLBooking_Ul', index: 'Ul', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Du', title: '@Resources.Locale.L_FCLBooking_Du', index: 'Du', sorttype: 'string', width: 110, hidden: false, editable: true },
        { name: 'InterfaceCd', title: '@Resources.Locale.L_InterfaceCd', index: 'InterfaceCd', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
        { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'right', formatter: 'integer', hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_IEBookingConfirmSetup_Script_136', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid2.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
    });

    var colModel1 = [
      { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
      { name: 'UFid', showname: 'UFid', sorttype: 'string', hidden: true, viewable: false },
      { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: true },
      { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'OpartNo', title: '@Resources.Locale.L_DNApproveManage_OpartNo', index: 'OpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'GoodsDescp', title: '@Resources.Locale.L_DNManage_GoodsDescp', index: 'GoodsDescp', width: 250, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'Qty', title: '@Resources.Locale.L_BaseLookup_Qty', index: 'Qty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false, formatter: 'integer' },
      { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Qtyu', index: 'Qtyu', sorttype: 'string', width: 65, hidden: false, editable: true },
      { name: "Nw", title: "@Resources.Locale.L_BaseLookup_Nw", index: "Nw", width: 100, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
      { name: "Gw", title: "@Resources.Locale.L_BaseLookup_Gw", index: "Gw", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
      { name: "Gwu", title: "@Resources.Locale.L_BaseLookup_NwUnit", index: "Gwu", width: 65, align: "left", sorttype: "string", hidden: false, editable: true },
      { name: "Cbm", title: "CBM", index: "Cbm", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: "0.0000", editable: true }, hidden: false, editable: true },
      { name: "Ucbm", title: "UCBM", index: "Ucbm", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "GwAvg", title: "GwAvg", index: "GwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "NwAvg", title: "NwAvg", index: "NwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "CbmAvg", title: "CbmAvg", index: "CbmAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'right', sorttype: 'int', editable: true, hidden: false, formatter: 'integer', editable: true }
    ];

    _handler.intiGrid("subbddGrid", $subbddGrid, {
        colModel: colModel1, caption: '@Resources.Locale.L_DNManage_ShipDet', delKey: ["UId"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);
        }
    });

    MenuBarFuncArr.AddMenu("MBConfirm", "glyphicon glyphicon-bell", "@Resources.Locale.L_UserQuery_ComBA", function () {
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        $.ajax({
            async: true,
            url: rootPath + "DNManage/SaveFCLBConfirmData",
            type: 'POST',
            data: { "UId": uid, "ShipmentId": shipmentid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (data.IsOk == "Y") {
                    CommonFunc.Notify("", data.message, 500, "success");
                } else {
                    CommonFunc.Notify("", data.message, 500, "warning");
                    return;
                }
                //_handler.topData = { UId: _uid };
                MenuBarFuncArr.MBCancel();
            }
        });
    });


    registBtnLookup($("#PodCdLookup"), {
        item: '#PodCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
            $("#PodName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PolCdLookup"), {
        item: '#PolCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            //$("#PolCnty").val(map.CntryCd);
            $("#PolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.CNTRY_CD+rd.PORT_CD);
        $("#PolCnty").val(rd.CNTRY_CD);
        $("#PolName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
    }));

    registBtnLookup($("#GwuLookup"), {
        item: '#Gwu', url: rootPath + LookUpConfig.NwuUrl, config: LookUpConfig.NwuLookup, param: "", selectRowFn: function (map) {
            $("#Gwu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, function ($grid, rd, elem) {
        $("#Gwu").val(rd.CD);
    }));

    registBtnLookup($("#IncotermCdLookup"), {
        item: '#IncotermCd', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#IncotermCd").val(map.Cd);
            //$("#IncotermDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#IncotermCd").val(rd.CD);
        //$("#IncotermDescp").val(rd.CD_DESCP);
    }));

    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBAdd","MBSearch","MBDel", "MBCopy", "MBApply", "MBApprove"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
    PpMonitor();

    $("#Cw").blur(function () {
        var val = $(this).val();
        var type = "E";
        $(this).val(ResetValue(val, type));
    })
    $("#Cbm").blur(function () {
        var val = $(this).val();
        var type = "E";
        $(this).val(ResetValue(val, type));
    })
    $("#Gw").blur(function () {
        var val = $(this).val();
        var type = "E";
        $(this).val(ResetValue(val, type));
    })
});


var _tran = "", _bu = "P";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            trnOptions = data.PK || [];
            if (trnOptions.length > 0)
                _mt = trnOptions[0]["cd"];
            appendSelectOption($("#LoadingFrom"), trnOptions);
            appendSelectOption($("#LoadingTo"), trnOptions);

            if (_handler.topData) {
                $("#TranMode").val(_handler.topData["TranMode"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
            }
        }
    });
}
