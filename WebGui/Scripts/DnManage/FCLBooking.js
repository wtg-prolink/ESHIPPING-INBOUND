var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var _tran = "3";
$(function () {
    _initCombineInv();
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $DnGrid = $("#DnGrid");
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

    _handler.addData = function () {
        //初始化新增数据
        var data = { "FreightTerm": _bu, "TranType": _tran };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
    }
    var _setMyReadonlys=function(){
        var readonlys = ["Cnt20", "Cnt40", "Cnt40hq", "CntType", "CntNumber",//"BlType",
         "MasterNo", "HouseNo", "Carrier", "CarrierNm", "BookingInfo", "CombineInfo",// "Etd", "Eta",//"PortCd",
         "PortNm", "PorCd", "PorName", "PolCd", "PolName", "PodCd", "PodName", "DestCd", "DestName",
         "Vessel1", "Voyage1", "Vessel2", "Voyage2", "Vessel3", "Voyage3", "Vessel4", "Voyage4", "Voyage4",
         "Etd1", "Etd2", "Etd3", "Etd4", "Eta1", "Eta2", "Eta3", "Eta4", "CutBlDate", "Atd", "Ata", "Atp",
         "Atd1", "Atd2", "Atd3", "Atd4", "Ata1", "Ata2", "Ata3", "Ata4",
        "RlsCntrDate", "SignBack", "CutPortDate", "RcvDate", "CustomsDate", "PortRlsDate", "PortDate"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
        $("#Voyage1").parent().find("button").attr("disabled", false);
        $("#Voyage2").parent().find("button").attr("disabled", false);
        $("#Voyage3").parent().find("button").attr("disabled", false);
        $("#Voyage4").parent().find("button").attr("disabled", false);
        //$("#BlRmk").attr('disabled', false);
    }

    _handler.beforEdit = function () {
        return CheckStatus();
    }

    _handler.afterEdit = function () {
        SetStatusToReadOnly(_setMyReadonlys, false, "F");
        MenuBarFuncArr.Enabled(["MBSiInfo"]);
        gridEditableCtrl({ editable: false, gridId: 'DnGrid' });
        $("#panelbody [readonly]").parent().find("button").attr("disabled", true);
    }

    _handler.beforSave = function () {
        var $grid = $SubGrid;
        var nullCols = [], sameCols = [];
        var co = $("#CombineOther").val();
        if (co == "Y") {
            var tcbm = $("#Tcbm").val();
            if (isEmpty(tcbm)) {
                alert("@Resources.Locale.L_DNManage_TotalCbm");
                return false;
            }
        }
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
        var status = $("#Status").val();
        if (status == "B") {
            alert("@Resources.Locale.L_DTBookingSetup_Script_131");
        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, "ShipmentId": shipmentid, autoReturnData: false, "TranType": "F" },
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
        if (data["DnGrid"])
            _handler.loadGridData("DnGrid", $DnGrid[0], data["DnGrid"], [""]);
        else
            _handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        //var rvstatus= data["rvstatus"];
        //$("#rvstatus").val(rvstatus);
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
        MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "MBSiInfo", "btn01", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "MBChangeDate", "btnFiBlRemark","MBChangePod"]);

        $("#Voyage1").parent().find("button").attr("disabled", false);
        $("#Voyage2").parent().find("button").attr("disabled", false);
        $("#Voyage3").parent().find("button").attr("disabled", false);
        $("#Voyage4").parent().find("button").attr("disabled", false);
        ShowSMStatus(_handler.topData["Status"]);
        ChangeColor();
        setFix();
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "DNManage/GetFCLBookingItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
                MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "MBSiInfo", "btn01", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark","MBChangePod"]);
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
            //var modify=$("#MBEdit").attr("disabled");
            ////if()
            //var torder = $("#Torder").val();
            //var PartyType = getGridVal($SubGrid, rowid, "PartyType", null);
            //if (rowid != null && torder == "N" && PartyType == "CR") {
            //    $SubGrid.editRow(rowid, true);
            //    $SubGrid.setColProp('PartyType', { editable: false });
            //    //$SubGrid.setColProp('PartyType', { editable: true });
            //}
            ////else {
            ////    $SubGrid.editRow(rowid, false); //$SubGrid.setColProp('PartyType', { editable: false });
            ////}
            //var Border = $("#Border").val();
            //if (rowid != null && Border == "M" && PartyType == "BR") {
            //    $SubGrid.editRow(rowid, true);
            //    $SubGrid.setColProp('PartyType', { editable: false });
            //    //$SubGrid.setColProp('PartyType', { editable: true });
            //}

            //var rvstatus = $("#rvstatus").val();
            //var statusb = false;
            //switch (rvstatus)
            //{
            //    case "D":
            //    case "R":
            //    case "C":
            //    case "I":
            //    case "G":
            //    case "V":
            //    case "":
            //        statusb = true; break;
            //}
            //if (rowid != null && statusb && (PartyType == "FS" || PartyType == "BO" || PartyType == "SP")) {
            //    $SubGrid.editRow(rowid, true);
            //    $SubGrid.setColProp('PartyType', { editable: false });
            //    //$SubGrid.setColProp('PartyType', { editable: true });
            //}
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('PartyType', { editable: true });
            } else {
                $SubGrid.setColProp('PartyType', { editable: false });
            }
            
        },
        onAddRowFunc: function (rowid) {
            var UId = getGridVal($SubGrid, rowid, "UId", null);
            if (UId == "" || UId == null) {
                UId = genUid(uuid());
                $SubGrid.jqGrid('setCell', rowid, "UId", UId, 'edit-cell dirty-cell');
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
        { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', sorttype: 'string', width: 120, hidden: false, editable: true },
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
        { name: 'OhsCode', title: '@Resources.Locale.L_DNApproveManage_OhsCode', index: 'OhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'IhsCode', title: '@Resources.Locale.L_DNApproveManage_HisCode', index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'right', formatter: 'integer', hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_DNManage_FCLBDnDetail', delKey: ["UId", "PartyType"],
        savelayout: true,
        showcolumns: true,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid2.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
    });

    _handler.intiGrid("DnGrid", $DnGrid, {
        colModel: BaseBooking_DnModel, caption: '@Resources.Locale.L_DNManage_DNDeclaInfo', delKey: ["DnNo", "DnNo"],
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('DnNo', { editable: true });
            } else {
                $SubGrid.setColProp('DnNo', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //$SubGrid.setColProp('PartyType', { editable: true });
        }
    });

    RegisterBookingBtn();

    MenuBarFuncArr.MBCopy = function (thisItem) {
        CopyFunction(thisItem, $SubGrid);
    }

    MenuBarFuncArr.AddMenu("MBVoid", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_ReturnDeli", function () {
        //$("#VoidsmRemark").val("");
        //$("#VoidSM").modal("show");
        DefaultVoidSM();
    });

    MenuBarFuncArr.AddMenu("btnFiBlRemark", "glyphicon glyphicon-bell", "@Resources.Locale.L_MenuBar_BLRemarkEdit", function () {
        BlRemarkModify();
    });
    MenuBarFuncArr.AddMenu("MBExcepted", "glyphicon glyphicon-bell", "@Resources.Locale.L_ActManage_AnM", function () {
        var ShipmentId = $("#ShipmentId").val();
        if (!ShipmentId) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        initErrMsg($("#MBExcepted"), { 'GROUP_ID': groupId, 'CMP': cmp, 'STN': stn, 'UId': '', 'JobNo': ShipmentId }, true);
    });

    MenuBarFuncArr.AddMenu("MBAllocation", "glyphicon glyphicon-bell", "Allocation", function () {
        var uid = $("#UId").val();
        var carrier = $("#Carrier").val();
        var status = $("#Status").val();
        if ("A" != status) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasBeenBooking", 500, "warning");
            return;
        }

        if (!isEmpty(carrier)) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_TurnAllocation", 500, "warning");
            return;
        }
        if (!uid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "BookingAction/Allocation",
            type: 'POST',
            data: { UId: uid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (data.IsOk == "N") {
                    CommonFunc.Notify("", data.message, 500, "warning");
                } else {
                    CommonFunc.Notify("", data.message, 500, "success"); 
                }
                MenuBarFuncArr.MBCancel();
            }
        });
    });

    MenuBarFuncArr.AddMenu("MBCallCar", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_CallCar", function () {
        var shipmentid = $("#ShipmentId").val();
        if (!shipmentid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "BookingAction/CallCar",
            type: 'POST',
            data: { ShipmentId: shipmentid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                CommonFunc.Notify("", data.message, 500, "success");
            }
        });
    });

    MenuBarFuncArr.AddMenu("MBSiInfo", "glyphicon glyphicon-th-list", "SI", function () {
        var ProfileCd = $("#ProfileCd").val();
        if (isEmpty(ProfileCd))
        {
            alert("@Resources.Locale.L_AirBookingSetup_ProfileIDIsNull");
            return;
        }
        if (ProfileCd.split(";").length == 1) {
            top.topManager.openPage({
                href: rootPath + "System/BSTDataSetup/" + UId + "?MenuBarPermiss=Y&Profile=" + ProfileCd,
                title: '@Resources.Locale.L_BSTQuery_CustTranSet',
                id: 'BSTSetup',
                search: 'Profile=' + ProfileCd
            });
            return;
        }
        $("#SiQueryDialog").modal("show");
    });

    MenuBarFuncArr.AddMenu("btn01", "glyphicon glyphicon-th-list", "@Resources.Locale.L_DNManage_ForeBk", function () {

        var uid = $("#UId").val();
        if (!uid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var shipments = $("#ShipmentId").val();

        var iscontinue = window.confirm("@Resources.Locale.L_ActManage_is" + shipments + "】@Resources.Locale.L_AirBookingSetup_Script_92");
        if (!iscontinue) {
            return;
        }

        ajaxHttp(rootPath + "BookingAction/FCLBookAction", { "Uid": uid, autoReturnData: false },
       function (result) {
           if (result.IsOk == "Y") {
               CommonFunc.Notify("", result.message, 500, "success");
           }
           else {
               CommonFunc.Notify("", result.message, 500, "warning");
           }
           MenuBarFuncArr.MBCancel();
           return true;
       });
    });

    MenuBarFuncArr.AddMenu("MBChangeDate", "glyphicon glyphicon-th-list", "@Resources.Locale.L_DNManage_Change", function () {
        $("#BackRemark").val("");
        $("#BookingCancel").modal("show");
    });

    MenuBarFuncArr.AddMenu("MBChangePod", "glyphicon glyphicon-th-list", "COD/改港", function () {
        var shipmentid = $("#ShipmentId").val();
        if (!shipmentid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var uid = $("#UId").val();
        var isok = "N";
        var response = $.ajax({
            async: false,
            url: rootPath + "BookingAction/CheckEtdDate",
            type: 'POST',
            data: { shipmentid: shipmentid,checktype:"COD"},
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                isok = data.IsOk;
            }
        });
        var ata = $("#Ata").val();
        if (ata) {
            CommonFunc.Notify("", "已经ATA，不允许改港!", 500, "warning");
            return;
        }
        if ("N" != isok) {
            alert("Please Contact LSP to change POD");
            return false;
        }
        top.topManager.openPage({
            href: rootPath + "DNManage/ChangePodView/" + uid,
            title: 'Change Pod View',
            id: 'ChangePodView',
            reload: true
        });
        //$("#BookingCancel").modal("show");
    });

    MenuBarFuncArr.AddMenu("pmsCombineInv", "glyphicon glyphicon-list-alt", "@Resources.Locale.L_DNManage_CombIv", function () {
        $("#CombineInvDialog").modal("show");
    });

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"], ["MBSiInfo"]);//初始化UI工具栏
    MenuBarFuncArr.Enabled(["MBCopy"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
    SetEtdEta();

    $("#Voyage1Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel1 || "";
        showVessel(vessel);
    });
    $("#Voyage2Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel2 || "";
        showVessel(vessel);
    });
    $("#Voyage3Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel3 || "";
        showVessel(vessel);
    });
    $("#Voyage4Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel4 || "";
        showVessel(vessel);
    });

    MenuBarFuncArr.Enabled(["MBEdoc"]);

    function showVessel(vessel) {
        if (isEmpty(vessel)) {
            CommonFunc.Notify("", "@Resources.Locale.L_SeaTransport_NoVesRe", 500, "warning");
            return;
        }
        $('#VesselMapFrame').attr("src", "https://ipdev.lsphub.com/iport/ShowVessel.aspx?vsl_nm=" + encodeURIComponent(vessel));
        $('#VesselMap').modal('show');
    };

    AddMBPrintFunc();
    ChangeColor();
});

var _bu = "P";
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
            console.log(data);
            var _shownull = { cd: '', cdDescp: '' };
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            trnOptions = data.PK || [];
            if (trnOptions.length > 0)
                _mt = trnOptions[0]["cd"];
            appendSelectOption($("#LoadingFrom"), trnOptions);
            appendSelectOption($("#LoadingTo"), trnOptions);

            ViaOptions = data.VIA || [];
            if (ViaOptions.length > 0)
                _mt = ViaOptions[0]["cd"];
            ViaOptions.unshift(_shownull);
            appendSelectOption($("#Via"), ViaOptions);

            if (_handler.topData) {
                $("#TranMode").val(_handler.topData["TranMode"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
                $("#Via").val(_handler.topData["Via"]);
            }
        }
    });
}

