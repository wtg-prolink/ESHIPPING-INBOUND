var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid5 = null;
$(function () {
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $subbddGrid = $("#subbddGrid");
    $SubGrid5 = $("#SubGrid5");
    _handler.saveUrl = rootPath + "SMSMI/BookingConfirmUpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
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
        _handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
    }

    _handler.beforSave = function () {
        var $grid = $SubGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        nullCols.push({ name: "PartyNo", index: 3, text: 'PartyNo' });
        sameCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
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

        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "u_id": uid, ShipmentId: shipmentid, autoReturnData: false, "TranType": "T", "IsConfirm": "Y" },
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

        if (data["sub5"])
            _handler.loadGridData("SubGrid5", $SubGrid5[0], data["sub5"], [""]);
        else
            _handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
       
        if ("B" == _handler.topData["BlType"]) {
            $("#SMBDDGRID").show();
            $("#SMDNPGRID").hide();
        } else {
            $("#SMBDDGRID").hide();
            $("#SMDNPGRID").show();
        } 
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        //init edoc for get all dn and shipment edco all in one view
        var multiEdocData = [];
        //ajaxHttp(rootPath + "SMSMI/GetDNData", { DnNo: $("#CombineInfo").val(), loading: true, shipmentuid: $("#UId").val() },
        //function (data) {
        //    if (data == null) {
        //        MenuBarFuncArr.initEdocCus($("#ShipmentId").val(),$("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", $("#Atd").val(), undefined, callBackFunc);
        //    } else {
        //        $(data.dn).each(function (index) {
        //            multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*', 'atd': $("#Atd").val() });
        //        });
        //        MenuBarFuncArr.initEdocCus($("#ShipmentId").val(),$("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", $("#Atd").val(), multiEdocData, callBackFunc);
        //    }
        // });

        ajaxHttp(rootPath + "SMSMI/GetDNData", { ShipmentId: $("#ShipmentId").val(), loading: true, OUid: $("#OUid").val() },
            function (data) {
                if (data != null) {
                    $(data.dn).each(function (index) {
                        multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*' });
                    });
                }
                if ($("#OUid").val() != "") {
                    multiEdocData.push({ jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': _handler.topData["Cmp"], 'STN': '*' });
                    MenuBarFuncArr.initEdoc($("#OUid").val(), _handler.topData["GroupId"], $("#OLocation").val(), "*", multiEdocData, callBackFunc);
                } else {
                    MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData, callBackFunc);
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

    function _setMyReadonlys() {
        var readonlys = ["ShipmentId", "CombineInfo", "Marks", "Instruction", "Goods", "DnEtd", "CostCenter","Cur","Gvalue",
            "CarType", "CarQty", "CarType1", "CarQty1", "CarType2", "CarQty2", "State", "Region", "TradeTerm", "TradetermDescp",
         "PpolCd", "PpolName", "PporCd", "PporName", "PpodCd", "PpodName", "PdestCd", "PdestName", "ProductDate", "PkgNum", "PkgUnit", "PkgUnitDesc",
         "ScacCd", "IncotermCd", "Qty", "Qtyu", "Pgw", "Gwu", "Pcbm", "ProfileCd", "Lgoods", "BlRmk", "TrackWay",
         "CargoType", "IncotermDescp", "PltNo", "DeepProcess", "TransacteMode", "BandCd", "Ett", "Att", "Dtt","Dqty",
          "Separate", "Fdqty", "Ndqty", "Cmp", "CreateDep", "CreateExt", "CreateBy", "ModifyBy",
         "Oexporter", "OexporterNm", "OexporterAddr", "Oimporter", "OimporterNm",
            "OimporterAddr", "BrokerInstr", "BrokerInfo", "ExportNo", "DeclNum", "NextNum", "BConfirmDate"
        ];
        if ($("#Status").val() != "O" && $("#Status").val() != "H" && $("#Status").val() != "R" && $("#Status").val() != "F") {
            readonlys.push("Atd");
            readonlys.push("Ata");
        }
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
    }

    _handler.afterEdit = function () {
        //SetStatusToReadOnly(_setMyReadonlys, true);
        //SetArrayDisable(_setMyReadonlys);
        //$("input[readonly][fieldname][dt='mt']").parent().find("button").attr("disabled", true);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "SMSMI/GetDetail", { uId: map.UId, type:"C", loading: true },// LookUpConfig.FCLBookingItemUrl
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
        { name: 'DnNo', title: 'DN NO', index: 'DnNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PoNo', title: _getLang("L_DNApproveManage_PoNo", "订单号"), index: 'PoNo', width: 220, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "对内机种名"), index: 'IpartNo', sorttype: 'string', width: 150, hidden: false, editable: true },
        { name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'MasterNo', title: _getLang("L_BaseLookup_MasterNo", "Master B/L"), index: 'MasterNo', sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'OpartNo', title: _getLang("L_DNApproveManage_OpartNo", "对外机种名"), index: 'OpartNo', sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'GoodsDescp', title: _getLang("L_DNApproveManage_GoodsDescp", "商品名称"), index: 'GoodsDescp', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'CntrStdQty', title: _getLang("L_CntrStdQty", "标准装柜量"), index: 'CntrStdQty', width: 150, align: 'right', formatter: 'integer', hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: _getLang("L_DTBookingConfirmSetup_Script_130", "内贸Confirm-DN明细"), delKey: ["UId", "PartyType"],
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
      { name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "对内机种名"), index: 'IpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'OpartNo', title: _getLang("L_DNApproveManage_OpartNo", "对外机种名"), index: 'OpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'GoodsDescp', title: _getLang("L_DNManage_GoodsDescp", "品名描述"), index: 'GoodsDescp', width: 250, align: 'left', sorttype: 'string', hidden: false, editable: true },
      { name: 'Qty', title: _getLang("L_BaseLookup_Qty", "数量"), index: 'Qty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false, formatter: 'integer' },
      { name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "数量单位"), index: 'Qtyu', sorttype: 'string', width: 65, hidden: false, editable: true },
      { name: "Nw", title: _getLang("L_BaseLookup_Nw", "净重"), index: "Nw", width: 100, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
      { name: "Gw", title: _getLang("L_BaseLookup_Gw", "毛重"), index: "Gw", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: "0.00" }, hidden: false, editable: true },
      { name: "Gwu", title: _getLang("L_BaseLookup_NwUnit", "重量单位"), index: "Gwu", width: 65, align: "left", sorttype: "string", hidden: false, editable: true },
      { name: "Cbm", title: "CBM", index: "Cbm", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: "0.0000", editable: true }, hidden: false, editable: true },
      { name: "Ucbm", title: "UCBM", index: "Ucbm", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "GwAvg", title: "GwAvg", index: "GwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "NwAvg", title: "NwAvg", index: "NwAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: "CbmAvg", title: "CbmAvg", index: "CbmAvg", width: 80, align: "right", sorttype: "float", formatter: "number", formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: "0.000000" }, hidden: true, editable: true },
      { name: 'CntrStdQty', title: _getLang("L_CntrStdQty", "标准装柜量"), index: 'CntrStdQty', width: 150, align: 'right', sorttype: 'int', editable: true, hidden: false, formatter: 'integer', editable: true }
    ];

    _handler.intiGrid("subbddGrid", $subbddGrid, {
        colModel: colModel1, caption: _getLang("L_DNManage_ShipDet", "出货明细"), delKey: ["UId"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);
        }
    });

    //registBtnLookup($("#PpodCdLookup"), {
    //    item: '#PpodCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
    //        $("#PpodCd").val(map.PortCd);
    //        $("#PpodName").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#PpodCd").val(rd.PORT_CD);
    //    $("#PpodName").val(rd.PORT_NM);
    //}));

    registBtnLookup($("#PdestCdLookup"), {
        item: '#PdestCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PdestCd").val(map.PortCd);
            $("#PdestName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PdestCd").val(rd.PORT_CD);
        $("#PdestName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup"), {
        item: '#PodCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.PortCd);
            $("#PodName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.PORT_CD);
        $("#PodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#CostCenterLookup"), {
        item: '#CostCenter', url: rootPath + LookUpConfig.CostCenterUrl, config: LookUpConfig.CostCenterLookup, param: "", selectRowFn: function (map) {
            $("#CostCenter").val(map.CostCenter);
        }
    }, undefined, LookUpConfig.GetCostCenterAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CostCenter").val(rd.COST_CENTER);
    }));

    registBtnLookup($("#PolCdLookup"), {
        item: '#PolCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.PortCd);
            $("#PolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.PORT_CD);
        $("#PolName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PpolCdLookup"), {
        item: '#PpolCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PpolCd").val(map.PortCd);
            $("#PpolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpolCd").val(rd.PORT_CD);
        $("#PpolName").val(rd.PORT_NM);
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


    registBtnLookup($("#PorCdLookup"), {
        item: '#PorCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PorCd").val(map.PortCd);
            $("#PorName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PorCd").val(rd.PORT_CD);
        $("#PorName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PolCdLookup"), {
        item: '#PolCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.PortCd);
            $("#PolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.PORT_CD);
        $("#PolName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup"), {
        item: '#PodCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.PortCd);
            $("#PodName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.PORT_CD);
        $("#PodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#DestCdLookup"), {
        item: '#DestCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#DestCd").val(map.PortCd);
            $("#DestName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#DestCd").val(rd.PORT_CD);
        $("#DestName").val(rd.PORT_NM);
    }));

    MenuBarFuncArr.AddMenu("MBConfirm", "glyphicon glyphicon-bell", _getLang("L_UserQuery_ComBA", "订舱确认"), function () {
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        var status = $("#Status").val();
        if (status == "I" || status == "C" || status == "D" || status == "H") {
            CommonFunc.Notify("", "You cann't operate this Action!", 500, "warning");
            return false;
        }
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/doFclConfirmShipment",
            type: 'POST',
            data: { "UId": uid, "ShipmentId": shipmentid },
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
                alert(errMsg);
            },
            success: function (data) {
                CommonFunc.ToogleLoading(false);
                if (data.msg == "success") {
                    CommonFunc.Notify("", "Booking Confirm Successful!", 500, "success");
                } else {
                    CommonFunc.Notify("", data.msg, 500, "warning");
                    return;
                }
                MenuBarFuncArr.MBCancel();
            }
        });
    });


    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBAdd","MBSearch","MBDel", "MBCopy", "MBApply", "MBApprove"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    //getSelectOptions();
    setSelectData(_selects);
    //PpMonitor();
    CalculatedDays();
});

function CalculatedDays() {
    /*Ett预计天数(ETA-ETD)，Att实际天数(ATA-ATD)，Dtt延误天数(实际天数-预计天数)，
    Aqty异常数量（LSP回填），Fqty已补数量（LSP回填），Dqty差异数量（异常数量-已补数量），
    Ndqty未送数量(Qty-已送数量)，Fdqty已送数量（Qty-异常数量）*/
    $("#Aqty").change(function () {
        calculateNdqty();
    });
    $("#Fqty").change(function () {
        calculateNdqty();
    });

    function calculateNdqty() {
        //$("#Dqty").val($("#Aqty").val() - $("#Fqty").val());
        $("#Fdqty").val($("#Qty").val() - $("#Aqty").val());
        $("#Ndqty").val($("#Aqty").val() - $("#Fqty").val());
    };

    $("#Eta").change(function () {
        calculateett();
    });
    $("#Etd").change(function () {
        calculateett();
    });

    $("#Ata").change(function () {
        calculateatt();
    });
    $("#Atd").change(function () {
        calculateatt();
    });

    function calculateett() {
        var ett = GetChangeDate($("#Etd").val(), $("#Eta").val());
        if (isEmpty(ett)) return;
        $("#Ett").val(ett);
        $("#Dtt").val(ett-$("#Att").val());
    }
    function calculateatt() {
        var att = GetChangeDate($("#Atd").val(), $("#Ata").val());
        if (isEmpty(att)) return ;
        $("#Att").val(att);
        $("#Dtt").val($("#Ett").val() - att);
    }
}
function GetChangeDate(comparie,etd) {
    if (isEmpty(comparie)) return null;
    if (isEmpty(etd)) return null;
    comparie = comparie.replace(new RegExp("/", "gm"), "-");
    etd = etd.replace(new RegExp("/", "gm"), "-");
    //etd = etd.replace("/", "-");
    //if (comparie < etd) return false;
    var beginTimes = comparie.substring(0, 10).split('-');
    var endTimes = etd.substring(0, 10).split('-');

    var starttime = new Date(beginTimes[0], beginTimes[1], beginTimes[2]);
    var starttimes = starttime.getTime();

    var lktime = new Date(endTimes[0], endTimes[1], endTimes[2]);
    var lktimes = lktime.getDate();
    return DateDiff(lktime, starttime);
}

function DateDiff(sDate1, sDate2) {    //sDate1和sDate2是2006-12-18格式  
    iDays = parseInt(Math.abs(sDate1 - sDate2) / 1000 / 60 / 60 / 24)    //把相差的毫秒数转换为天数  
    return iDays
}




var _tran = "", _bu = "P";
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
            setSelectData(data);
        }
    });
}
function setSelectData(data) {
    var _shownull = { cd: '', cdDescp: '' };
    var trnOptions = data.TTRN || [];
    if (trnOptions.length > 0)
        _tran = trnOptions[0]["cd"];
    appendSelectOption($("#TranType"), trnOptions);

    trnOptions = data.TCGT || [];
    if (trnOptions.length > 0)
        _tran = trnOptions[0]["cd"];
    appendSelectOption($("#CargoType"), trnOptions);

    trnOptions = data.TDTK || [];
    if (trnOptions.length > 0)
        _tran = trnOptions[0]["cd"];
    appendSelectOption($("#TrackWay"), trnOptions);

    trnOptions = data.TDT || [];
    if (trnOptions.length > 0)
        _tran = trnOptions[0]["cd"];
    appendSelectOption($("#CarType"), trnOptions);

    var TModOptions = data.TMOD || [];
    if (TModOptions.length > 0)
        _mt = TModOptions[0]["cd"];
    TModOptions.unshift(_shownull);
    appendSelectOption($("#TransacteMode"), TModOptions);

    if (_handler.topData) {
        $("#TranType").val(_handler.topData["TranType"]);
        $("#CargoType").val(_handler.topData["CargoType"]);
        $("#TrackWay").val(_handler.topData["TrackWay"]);
        $("#CarType").val(_handler.topData["CarType"]);
        $("#TransacteMode").val(_handler.topData["TransacteMode"]);
    }
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

function ChangeColor() {
    var _Estimate = ["PpolCd", "PporCd", "PpodCd", "PdestCd", "PpolName", "PporName", "PpodName", "PdestName", "Pgw", "Pcbm", "Pvw"];
    var _Actual = ["PolCd", "PorCd", "PodCd", "DestCd", "PolName", "PorName", "PodName", "DestName", "Gw", "Cbm", "Vw"];
    for (var i = 0; i < _Estimate.length; i++) {
        var _estimateval = $("#" + _Estimate[i]).val();
        var _actualval = $("#" + _Actual[i]).val();
        if (isEmpty(_estimateval) || isEmpty(_actualval)) continue;
        _estimateval = _estimateval.toUpperCase();
        _actualval = _actualval.toUpperCase();
        if (_estimateval != _actualval) {
            $("#" + _Actual[i]).css("color", "red");
        } else {
            $("#" + _Actual[i]).css("color", "black");
        }
    }
}