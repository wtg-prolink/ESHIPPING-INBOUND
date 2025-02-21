var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid, $SubGrid2;
$(function () {

    $SubGrid = $("#SubGrid");
    
    function getUnit(name) {
        var _name = name;
        var unit_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $SubGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return unit_op;
    }



    var BookingLookup = {
        caption: "@Resources.Locale.L_QTSetup_Partial",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'ShipmentId', title: '@Resources.Locale.L_QTSetup_ShipNo', index: 'ShipmentId', width: 120, init:true , align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
            //{ name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierNm', title: 'Supplier Name', index: 'SupplierNm', width: 250, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    };

    _handler.saveUrl = rootPath + "DNManage/SMBDUpdate";
    _handler.inquiryUrl = rootPath + "DNManage/SMBDQueryData";
    _handler.config = BookingLookup;

    _handler.addData = function () {
        var data = {
            "CreateBy": userId, "CreateDate": getDate(0, "-"), "Cmp": cmp, "Status": "D",
            "Cur": "USD", "Stn": stn, "Dep": dep, "CreateExt": ext
        };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        //getAutoNo("PoNo", "rulecode=SMRV_NO&cmp=" + cmp);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();
        var shipmentid=$("#ShipmentId").val();
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), "shipmentid": shipmentid, autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                _handler.topData = { UId: result.UId };
                MenuBarFuncArr.MBCancel();
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
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", null);
        MenuBarFuncArr.Enabled(["MBCopy", "MBBooking", "MBEdoc", "MBInvalid"]);
        showtruckdiv($("#TranMode").val());
    }

    _handler.beforEdit = function () {

        var status = $("#Status").val();

        if ("B" == status) {
            alert("@Resources.Locale.L_DNManage_HasBkCtEt");
            return false;
        } else if ("V" == status) {
            alert("@Resources.Locale.L_DNManage_CtEtDis");
            return false;
        }
        return true;
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "DNManage/GetSMBDItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforDel = function (map) {
        var status=$("#Status").val();
        if(status=="B")
        {
            alert("@Resources.Locale.L_DNManage_CtDel");
            return false;
        }
    }

    _handler.beforSave = function () {
        var tranmode=$("#TranMode").val();
        if(isEmpty(tranmode)) 
        {
            alert("@Resources.Locale.L_DNManage_EntTranTp");
            return false;
        }
        var plant = $("#Plant").val();
        if (isEmpty(plant)) {
            alert("Plant"+"@Resources.Locale.L_ActManage_Controllers_30");
            return false;
        }

        var co = $("#CombineOther").val();
        if (co == "Y") {
            var tcbm = $("#Tcbm").val();
            if (isEmpty(tcbm)) {
                alert("@Resources.Locale.L_DNManage_TotalCbm");
                return false;
            }
        }
    }

    var colModel1 = [
        { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        { name: 'UFid', showname: 'UFid', sorttype: 'string', hidden: true, viewable: false },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: true },
        { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'OpartNo', title: '@Resources.Locale.L_DNApproveManage_OpartNo', index: 'OpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'GoodsDescp', title: '@Resources.Locale.L_DNManage_GoodsDescp', index: 'GoodsDescp', width: 250, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Qty', title: '@Resources.Locale.L_BaseLookup_Qty', index: 'Qty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false ,formatter: 'integer' },
        { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Qtyu', index: 'Qtyu', editoptions: gridLookup(getUnit("Unit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: "Nw", title: "@Resources.Locale.L_BaseLookup_Nw", index: "Nw", width: 100, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
        { name: "Gw", title: "@Resources.Locale.L_BaseLookup_Gw", index: "Gw", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
        { name: "Gwu", title: "@Resources.Locale.L_BaseLookup_NwUnit", index: "Gwu", width: 65, align: "left", sorttype: "string", hidden: false, editable: true },
        { name: "Cbm", title: "CBM", index: "Cbm", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: "0.0000", editable: true }, hidden: false, editable: true },
        { name: "Ucbm", title: "UCBM", index: "Ucbm", width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: "GwAvg", title: "GwAvg", index: "GwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
        { name: "NwAvg", title: "NwAvg", index: "NwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
        { name: "CbmAvg", title: "CbmAvg", index: "CbmAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
        { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'right', sorttype: 'int', editable: true, hidden: false, formatter: 'integer', editable: true },
    ];

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel1, caption: '@Resources.Locale.L_DNManage_ShipDet', delKey: ["UId"], height: 100,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
             maxSeqNo = 0;
            $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid, rowid, "Ucbm", "M3");
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('IpartNo', { editable: true });
                $SubGrid.setColProp('OpartNo', { editable: true });
                $SubGrid.setColProp('GoodsDescp', { editable: true });
                $SubGrid.setColProp('Qtyu', { editable: true });
                $SubGrid.setColProp('Gwu', { editable: true });
                $SubGrid.setColProp('Ucbm', { editable: true });
                $SubGrid.setColProp('Nw', { editable: true });
                $SubGrid.setColProp('Gw', { editable: true });
                $SubGrid.setColProp('Cbm', { editable: true });
            } else {
                $SubGrid.setColProp('IpartNo', { editable: false });
                $SubGrid.setColProp('OpartNo', { editable: false });
                $SubGrid.setColProp('GoodsDescp', { editable: false });
                $SubGrid.setColProp('Qtyu', { editable: false });
                $SubGrid.setColProp('Gwu', { editable: false });
                $SubGrid.setColProp('Ucbm', { editable: false });
                $SubGrid.setColProp('Nw', { editable: false });
                $SubGrid.setColProp('Gw', { editable: false });
                $SubGrid.setColProp('Cbm', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $SubGrid.setColProp('PoNo', { editable: true });
        },
        onAddRowFunc: function (rowid) {
            var UId = getGridVal($SubGrid, rowid, "UId", null);
            if (UId == "" || UId == null) {
                UId = genUid(uuid());
                $SubGrid.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
            }
        },
        afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
            var smbdd = $('#SubGrid').jqGrid("getGridParam", "data");
            var subttgw = 0, subttnw = 0, subttcbm = 0;
            var qty = $("#SubGrid").jqGrid('getCell', rowid, "Qty");
            var gwavg = $("#SubGrid").jqGrid('getCell', rowid, "GwAvg");
            var nwavg = $("#SubGrid").jqGrid('getCell', rowid, "NwAvg");
            var cbmavg = $("#SubGrid").jqGrid('getCell', rowid, "CbmAvg");

            subttgw = qty * gwavg;
            subttnw = qty * nwavg;
            subttcbm = qty * cbmavg;
            //alert("subttgw:" + subttgw + " subttnw:" + subttnw + " subttgw:" + subttcbm)
            if (subttgw != 0)
                $('#SubGrid').jqGrid('setCell', rowid, "Gw", CommonFunc.formatFloatNoComma(subttgw, 4));
            if (subttnw != 0)
                $('#SubGrid').jqGrid('setCell', rowid, "Nw", CommonFunc.formatFloatNoComma(subttnw, 4));
            if (subttcbm != 0)
            $('#SubGrid').jqGrid('setCell', rowid, "Cbm", CommonFunc.formatFloatNoComma(subttcbm, 4));

            var SumGw = $SubGrid.jqGrid("getCol", "Gw", false, "sum");
            var SumNw = $SubGrid.jqGrid("getCol", "Nw", false, "sum");
            var SumCbm = $SubGrid.jqGrid("getCol", "Cbm", false, "sum");
            var SumQty = $SubGrid.jqGrid("getCol", "Qty", false, "sum");
            if (SumQty != 0) {
                $("#Qty").val(CommonFunc.formatFloatNoComma(SumQty, 0));
            }
            if (SumGw != 0) {
                $("#Gw").val(CommonFunc.formatFloatNoComma(SumGw, 4));
            }
            if (SumNw != 0) {
                $("#Nw").val(CommonFunc.formatFloatNoComma(SumNw, 4));
            }
            if (SumCbm != 0) {
                $("#Cbm").val(CommonFunc.formatFloatNoComma(SumCbm, 4));
            }
        }
    });

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#Status").val('D');
        $("#CreateBy").val(userId);
        $("#CreateDate").val(getDate(0, "-"));
        $("#Cmp").val(cmp);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);

        var dataRow, addData = [];
        var rowIds = $SubGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $SubGrid.jqGrid('getRowData', rowIds[i]);
            dataRow = {
                UId: "",
                UFId: "",
                ShipmentId: '',
                IpartNo: rowDatas.IpartNo,
                OpartNo: rowDatas.OpartNo,
                GoodsDescp: rowDatas.GoodsDescp,
                Qty: rowDatas.Qty,
                Qtyu: rowDatas.Qtyu,
                Nw: rowDatas.Nw,
                Gw: rowDatas.Gw,
                Gwu: rowDatas.Gwu,
                Cbm: rowDatas.Cbm,
                Ucbm: rowDatas.Cbmu,
                GwAvg: rowDatas.GwAvg,
                NwAvg: rowDatas.NwAvg,
                CbmAvg: rowDatas.CbmAvg
            };
            addData.push(dataRow);
        }

        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#SubGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }
        gridEditableCtrl({ editable: true, gridId: "SubGrid" });
    }

    smbdResterBtn();
    getSelectOptions();

    MenuBarFuncArr.AddMenu("MBBooking", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_LaunchBk", function () {
        var status = $("#Status").val();
        if (status == "B") {
            alert("@Resources.Locale.L_SMBDSetup_Script_148");
            return false;
        } else if (status == "V") {
            alert("@Resources.Locale.L_SMBDSetup_Script_149");
            return false;
        }

        var dnitems = $("#DnNo").val();
        _uid = $("#UId").val();
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "DNManage/InitiSMBDBooking",
            type: 'POST',
            data: {
                "pushdata": dnitems,
                "uids": _uid,
                "DnNo": dnitems
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                if (result.IsOk == "Y") {
                    CommonFunc.Notify("", dnitems + "@Resources.Locale.L_DNManage_BKS", 500, "success");
                } else {
                    CommonFunc.Notify("", result.message, 500, "warning");
                }
                var _mainCmpData = { UId: _uid }; //_uid
                _handler.loadMainData(_mainCmpData);
            }
        });
    });

    MenuBarFuncArr.MBInvalid = function () {
        var status = $("#Status").val();
        if (status == "B") {
            alert("@Resources.Locale.L_SMBDSetup_Script_150");
            return false;
        }
        var iscontinue = window.confirm("@Resources.Locale.L_SMBDSetup_Script_151");
        if (!iscontinue) {
            return;
        }

        var uid = $("#UId").val();
        $.ajax({
            async: true,
            url: rootPath + "DNManage/VoidSMBD",
            type: 'POST',
            data: { UId: uid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (data.IsOk == "Y") {
                    CommonFunc.Notify("", data.message, 500, "success");
                }
                else {
                    CommonFunc.Notify("", data.message, 500, "warning");
                }
                MenuBarFuncArr.MBCancel();
            }
        });
    }

    //MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
    //MenuBarFuncArr.Enabled(["MBEdoc"]);

    //MenuBarFuncArr.MBEdoc = function (thisItem) {
    //    var cmpitem = $("#Cmp").val();
    //    initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmpitem, STN: "*" });
    //}

    _handler.beforLoadView = function () {
        var keys = ["Uid","DnNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }

        var requires = ["Uid"];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        var readonlys = [];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        }
    }

    
    _initUI(["MBApply", "MBApprove", "MBErrMsg"]);//初始化UI工具栏

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    showtruckdiv();
});

function smbdResterBtn() {
    registBtnLookup($("#ShprCdLookup"), {
        item: '#ShprCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#ShprCd").val(map.PartyNo);
            $("#ShprNm").val(map.PartyName);
        }
    }, { focusItem: $("#ShprCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ShprCd").val(rd.PARTY_NO);
        $("#ShprNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#CneeCdLookup"), {
        item: '#CneeCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#CneeCd").val(map.PartyNo);
            $("#CneeNm").val(map.PartyName);
        }
    }, { focusItem: $("#CneeCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CneeCd").val(rd.PARTY_NO);
        $("#CneeNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#NotifyCdLookup"), {
        item: '#NotifyCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#NotifyCd").val(map.PartyNo);
            $("#NotifyNm").val(map.PartyName);
        }
    }, { focusItem: $("#NotifyCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#NotifyCd").val(rd.PARTY_NO);
        $("#NotifyNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#ShiptoCdLookup"), {
        item: '#ShiptoCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#ShiptoCd").val(map.PartyNo);
            $("#ShiptoNm").val(map.PartyName);
        }
    }, { focusItem: $("#ShiptoCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ShiptoCd").val(rd.PARTY_NO);
        $("#ShiptoNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#BilltoCdLookup"), {
        item: '#BilltoCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#BilltoCd").val(map.PartyNo);
            $("#BilltoNm").val(map.PartyName);
        }
    }, { focusItem: $("#BilltoCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#BilltoCd").val(rd.PARTY_NO);
        $("#BilltoNm").val(rd.PARTY_NAME);
    }));

    //registBtnLookup($("#PickupWmsLookup"), {
    //    item: '#PickupWms', url: rootPath + LookUpConfig.WhUrl, config: LookUpConfig.WhLookup, param: "", selectRowFn: function (map) {
    //        $("#PickupWms").val(map.Cd);
    //        $("#PickupWmsNm").val(map.CdDescp);
    //    }
    //}, { focusItem: $("#PickupWms") }, LookUpConfig.GetCodeTypeAuto(groupId, "WH", function ($grid, rd, elem) {
    //    $("#PickupWms").val(rd.CD);
    //    $("#PickupWmsNm").val(rd.CD_DESCP);
    //}));

    registBtnLookup($("#PickupWmsLookup"), {
        item: '#PickupWms', url: rootPath + LookUpConfig.SMWHUrl, config: LookUpConfig.SMWHLookup, param: "", selectRowFn: function (map) {
            $("#PickupWms").val(map.WsCd);
            $("#PickupWmsNm").val(map.WsNm);
        }
    }, { focusItem: $("#PickupWms") }, LookUpConfig.GetSMWHAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PickupWms").val(rd.WS_CD);
        $("#PickupWmsNm").val(rd.WS_NM);
    }));

    registBtnLookup($("#PickupPortLookup"), {
        item: '#PickupPort', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PickupPort").val(map.PortCd);
            $("#PickupNm").val(map.PortNm);
        }
    }, { focusItem: $("#PickupPort") }, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PickupPort").val(rd.PORT_CD);
        $("#PickupNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodTruckLookup"), {
        item: '#PodTruck', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodTruck").val(map.PortCd);
            $("#PodTruckNm").val(map.PortNm);
        }
    }, { focusItem: $("#PodTruck") }, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodTruck").val(rd.PORT_CD);
        $("#PodTruckNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CUR);
    }));

    registBtnLookup($("#DeliveryPortLookup"), {
        item: '#DeliveryPort', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#DeliveryPort").val(map.CntryCd + map.PortCd);
            $("#DeliveryNm").val(map.PortNm);
        }
    }, undefined);

    //registBtnLookup($("#DeliveryPortLookup"), {
    //    item: '#DeliveryPort', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#DeliveryPort").val(map.CntryCd + map.PortCd);
    //        $("#DeliveryNm").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#DeliveryPort").val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#DeliveryNm").val(rd.PORT_NM);
    //}));

    registBtnLookup($("#PolTruckLookup"), {
        item: '#PolTruck', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolTruck").val(map.CntryCd + map.PortCd);
            $("#PoltruckNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolTruck").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PoltruckNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#StateLookup"), {
        item: '#State', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#State").val(map.StateCd);
            $("#Region").val(map.RegionCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#State").val(rd.STATE_CD);
        $("#Region").val(rd.REGION_CD);
    }));

    //registBtnLookup($("#EStateLookup"), {
    //    item: '#EState', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
    //        $("#EState").val(map.StateCd);
    //    }
    //}, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#EState").val(rd.STATE_CD);
    //}));

    registBtnLookup($("#EStateLookup"), {
        item: '#EState', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#EState").val(map.StateCd);
        }
    });

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
    }, function ($grid, rd, elem) {
        $("#Region").val('');
    }));

    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
    }));

    registBtnLookup($("#PlantLookup"), {
        item: '#Plant', url: rootPath + "Common/GetPltCode", config: LookUpConfig.BSCodeLookup, param: "&sopt_Cmp=eq&Cmp=" + cmp, selectRowFn: function (map) {
            $("#Plant").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "PLT", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
    }));

    registBtnLookup($("#GwuLookup"), {
        item: '#Gwu', url: rootPath + LookUpConfig.NwuUrl, config: LookUpConfig.NwuLookup, param: "", selectRowFn: function (map) {
            $("#Gwu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, function ($grid, rd, elem) {
        $("#Gwu").val(rd.CD);
    }));

    var TermLookup = {
        caption: "@Resources.Locale.L_DNManage_SerTranType",
        sortname: "Cd",
        refresh: false,
        columns: [{ name: "Cd", title: "@Resources.Locale.L_DNApproveManage_TranMode", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: "CdDescp", title: "@Resources.Locale.L_BaseLookup_Nm", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    };

    registBtnLookup($("#TranModeLookup"), {
        item: '#TranMode', url: rootPath + LookUpConfig.TrackingTranModeUrl, config: TermLookup, param: "", selectRowFn: function (map) {
            $("#TranMode").val(map.Cd);
            showtruckdiv(map.Cd);
            $("#TranmodeDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TNT", undefined, function ($grid, rd, elem) {
        $("#TranMode").val(rd.CD);
        showtruckdiv(rd.CD);
        $("#TranmodeDescp").val(rd.CD_DESCP);
    }));

    registBtnLookup($("#CntyLookup"), {
        item: '#Cnty', url: rootPath + LookUpConfig.CountryUrl, config: LookUpConfig.CountryLookup, param: "", selectRowFn: function (map) {
            $("#Cnty").val(map.CntryCd);
            $("#CntyNm").val(map.CntryNm);
        }
    }, undefined, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cnty").val(rd.CNTRY_CD);
        $("#CntyNm").val(rd.CNTRY_NM);
    }, function ($grid, elem) {
        $("#Cnty").val("");
        $("#CntyNm").val("");
    }));

    registBtnLookup($("#PkgUnitLookup"), {
        item: '#PkgUnit', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#PkgUnit").val(map.Cd);
            $("#PkgUnitDesc").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $("#PkgUnit").val(rd.CD);
        $("#PkgUnitDesc").val(map.CD_DESCP);
    }, function ($grid, elem) {
        $("#PkgUnit").val();
        $("#PkgUnitDesc").val();
    }));

    registBtnLookup($("#CostCenterLookup"), {
        item: '#CostCenter', url: rootPath + LookUpConfig.CostCenterUrl, config: LookUpConfig.CostCenterLookup, param: "", selectRowFn: function (map) {
            $("#CostCenter").val(map.CostCenter);
            $("#CostCenterdescp").val(map.Dep);
        }
    }, undefined, LookUpConfig.GetCostCenterAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CostCenter").val(rd.COST_CENTER);
        $("#CostCenterdescp").val(rd.DEP);
    }));

    registBtnLookup($("#TradeTermLookup"), {
        item: '#TradeTerm', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#TradeTerm").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#TradeTerm").val(rd.CD);
    }, function ($grid, elem) {
        $("#TradeTerm").val("");
    }));

    registBtnLookup($("#CityLookup"), {
        item: '#City', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#City").val(map.PortCd);
            $("#CityNm").val(map.PortNm);
        }
    }, undefined);

    //registBtnLookup($("#CityLookup"), {
    //    item: '#City', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#City").val(map.PortCd);
    //        $("#CityNm").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#City").val(rd.PORT_CD);
    //    $("#CityNm").val(rd.PORT_NM);
    //}, function ($grid, rd, elem) {
    //    $("#City").val('');
    //    $("#CityNm").val('');
    //}));

    showtruckdiv($("#TranMode").val());

    function SetParytInfo(combineval, fieldids) {
        if (isEmpty(combineval)) return;
        var combinevals = combineval.split('|');
        if (combinevals.length >= 2) {
            $.each(fieldids, function (index, val) {
                $("#"+val).val(combinevals[index]);
            });
        } else {
            $.each(fieldids, function (index, val) {
                $("#"+val).val('');
            });
        }
    }

    function ResetSmdnp(dnno) {
        ajaxHttp(rootPath + "DNManage/SMDNPQuyer", { DnNo: dnno },
            function (data) {
                if (data["smdnp"]) {
                    var smdnprows = data["smdnp"];
                    var dataRow, addData = [];
                    var rowIds = $SubGrid.getDataIDs();
                    for (var i = 0; i < smdnprows.length; i++) {
                        var rowDatas = smdnprows[i];
                        var gwavg = 0, nwavg = 0, cbmavg = 0;
                        if (rowDatas.Qty > 0) {
                            gwavg = rowDatas.Gw / rowDatas.Qty;
                            nwavg = rowDatas.Nw / rowDatas.Qty;
                            cbmavg = rowDatas.Cbm / rowDatas.Qty;
                        }

                        dataRow = {
                            UId: "",
                            UFId: "",
                            ShipmentId: '',
                            IpartNo: rowDatas.IpartNo,
                            OpartNo: rowDatas.OpartNo,
                            GoodsDescp: rowDatas.GoodsDescp,
                            Qty: rowDatas.Qty,
                            Qtyu: rowDatas.Qtyu,
                            Nw: rowDatas.Nw,
                            Gw: rowDatas.Gw,
                            Gwu: rowDatas.Gwu,
                            Cbm: rowDatas.Cbm,
                            Ucbm: rowDatas.Cbmu,
                            GwAvg: gwavg,
                            NwAvg: nwavg,
                            CbmAvg: cbmavg,
                            CntrStdQty: rowDatas.CntrStdQty
                        };
                        addData.push(dataRow);
                    }

                    _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
                    for (var i = 0; i < addData.length; i++) {
                        $("#SubGrid").jqGrid("addRowData", undefined, addData[i], "last");
                    }
                }
                else
                    _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
            });
    }

    registBtnLookup($("#DnNoLookup"), {
        item: '#DnNo', url: rootPath + LookUpConfig.DnInfoUrl, config: LookUpConfig.DnQueryLookup, param: "", selectRowFn: function (map) {
            $("#DnNo").val(map.DnNo);
            ResetSmdnp(map.DnNo);
            $("#CombineInfo").val(map.CombineInfo);
            $("#Marks").val(map.ShipMark);
            $("#Goods").val(map.Goods);
            $("#TranType").val(map.TranType);
            $("#TruckType").val(map.CarType);
            SetParytInfo(map.Notify, ["NotifyCd", "NotifyNm"]);
            SetParytInfo(map.Shipper, ["ShprCd", "ShprNm"]);
            SetParytInfo(map.Consignee, ["CneeCd", "CneeNm"]);
            SetParytInfo(map.Shipto, ["ShiptoCd", "ShiptoNm"]);
            SetParytInfo(map.Billto, ["BilltoCd", "BilltoNm"]);
            $("#TranMode").val(map.TranType);
            $("#TranmodeDescp").val(map.TranTypeDescp);
            $("#CargoType").val(map.CargoType);
            $("#PickupPort").val(map.Pol);
            $("#PolTruck").val(map.Pol);
            $("#PickupNm").val(map.PolNm);
            $("#PoltruckNm").val(map.PolNm);
            $("#Region").val(map.Region);
            $("#State").val(map.State);
            $("#DeliveryPort").val(map.Pod);
            $("#PodTruck").val(map.Pod);
            $("#DeliveryNm").val(map.PodNm);
            $("#PodTruckNm").val(map.PodNm);
            $("#Qtyu").val(map.Qtyu);
            $("#Gwu").val(map.Gwu);
            $("#FrtTerm").val(map.FreightTerm);
            $("#Horn").val(map.Horn);
            $("#Battery").val(map.Battery);
            $("#Qty").val(map.Qty);
            $("#Nw").val(map.Nw);
            $("#Cbm").val(map.Cbm);
            $("#Gw").val(map.Gw);
            $("#PkgNum").val(map.PkgNum);
            $("#PkgUnit").val(map.PkgUnit);
            $("#PkgUnitDesc").val(map.PkgUnitDesc);
            $("#CostCenter").val(map.CostCenter);
            $("#CostCenter").val(map.CostCenter);
            $("#TradeTerm").val(map.TradeTerm);
            $("#TradetermDescp").val(map.TradetermDescp);
            $("#Tcbm").val(map.Tcbm);
            $("#State").val(map.State);
            $("#TradetermDescp").val(map.TradetermDescp);
            $("#CombineOther").val(map.CombineOther);
            $("#CentDecl").val(map.CentDecl);
            $("#Gvalue").val(map.Amount1);
            $("#PickupWmsDate").val(map.Etd);
        }
    }, { focusItem: $("#DnNo") }, LookUpConfig.GetDnInfoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#DnNo").val(rd.DN_NO);
        ResetSmdnp(rd.DN_NO);
        $("#CombineInfo").val(rd.COMBINE_INFO);
        $("#Marks").val(rd.SHIP_MARK);
        $("#Goods").val(rd.GOODS);
        $("#TranType").val(rd.TRAN_TYPE);
        $("#TruckType").val(rd.CAR_TYPE);
        SetParytInfo(rd.NOTIFY, ["NotifyCd", "NotifyNm"]);
        SetParytInfo(rd.SHIPPER, ["ShprCd", "ShprNm"]);
        SetParytInfo(rd.CONSIGNEE, ["CneeCd", "CneeNm"]);
        SetParytInfo(rd.SHIPTO, ["ShiptoCd", "ShiptoNm"]);
        SetParytInfo(rd.BILLTO, ["BilltoCd", "BilltoNm"]);
        $("#TranMode").val(rd.TRAN_TYPE);
        $("#TranmodeDescp").val(rd.TRAN_TYPE_DESCP);
        $("#CargoType").val(rd.CARGO_TYPE);
        $("#PickupPort").val(rd.POL);
        $("#PolTruck").val(rd.POL);
        $("#PickupNm").val(rd.POL_NM);
        $("#PoltruckNm").val(rd.POL_NM);
        $("#Region").val(rd.REGION);
        $("#State").val(rd.STATE);
        $("#DeliveryPort").val(rd.POD);
        $("#PodTruck").val(rd.POD);
        $("#DeliveryNm").val(rd.POD_NM);
        $("#PodTruckNm").val(rd.POD_NM);
        $("#Qtyu").val(rd.QTYU);
        $("#Gwu").val(rd.GWU);
        
        $("#FrtTerm").val(rd.FREIGHT_TERM);
        $("#Horn").val(rd.HORN);
        $("#Battery").val(rd.BATTERY);
        $("#Qty").val(rd.QTY);
        $("#Nw").val(rd.NW);
        $("#Cbm").val(rd.CBM);
        $("#Gw").val(rd.GW);
        $("#PkgNum").val(rd.PKG_NUM);
        $("#PkgUnit").val(rd.PKG_UNIT);
        $("#PkgUnitDesc").val(rd.PKG_UNIT_DESC);
        $("#CostCenter").val(rd.COST_CENTER);
        $("#TradeTerm").val(rd.TRADE_TERM);
        $("#TradetermDescp").val(rd.TRADETERM_DESCP);
        $("#Tcbm").val(rd.TCBM);
        $("#State").val(rd.STATE);
        $("#TradetermDescp").val(rd.TRADETERM_DESCP);
        $("#CombineOther").val(rd.COMBINE_OTHER);
        $("#CentDecl").val(rd.CENT_DECL);
        $("#Gvalue").val(rd.AMOUNT1);
        $("#PickupWmsDate").val(rd.ETD);
    }));
}

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("DTBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            trnOptions = data.TCGT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#CargoType"), trnOptions);

            trnOptions = data.TDTK || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranType"), trnOptions);

            trnOptions = data.TDT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            var _shownull = { cd: '', cdDescp: '' };
            trnOptions.unshift(_shownull);
            appendSelectOption($("#TruckType"), trnOptions);

            if (_handler.topData) {
                $("#CargoType").val(_handler.topData["CargoType"]);
                $("#TranType").val(_handler.topData["TranType"]);
                $("#TruckType").val(_handler.topData["TruckType"]);
            }
        }
    });
}

function showtruckdiv(trantype) {
    if (trantype == "D" || trantype == "T") {
        $('#polbtndiv').css('display', 'block');
        $('#polnmdiv').css('display', 'block');
        $('#podbtndiv').css('display', 'block');
        $('#podnmdiv').css('display', 'block');

        $('#poltruckbtndiv').css('display', 'none');
        $('#polnmtruckdiv').css('display', 'none');
        $('#podtruckbtndiv').css('display', 'none');
        $('#podnmtruckdiv').css('display', 'none');
    } else {
        $('#polbtndiv').css('display', 'none');
        $('#polnmdiv').css('display', 'none');
        $('#podbtndiv').css('display', 'none');
        $('#podnmdiv').css('display', 'none');

        $('#poltruckbtndiv').css('display', 'block');
        $('#polnmtruckdiv').css('display', 'block');
        $('#podtruckbtndiv').css('display', 'block');
        $('#podnmtruckdiv').css('display', 'block');
    }
}


