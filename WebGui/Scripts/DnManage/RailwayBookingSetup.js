var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

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
        var status = $("#Status").val();
        if (status == "B") {
            alert("@Resources.Locale.L_DTBookingSetup_Script_131");
        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, ShipmentId: shipmentid, autoReturnData: false, "TranType": "R" },
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation","btn01","MBSiInfo", "MBCallCar", "MBVoid","MBPrint","MBPreview","btnFiBlRemark"]);
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
        MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "btn01", "MBSiInfo", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod"]);
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
                MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "btn01", "MBSiInfo", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod"]);
            });
    }

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
        { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'right', formatter: 'integer', hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_RailwayBookingSetup_Script_146', delKey: ["UId", "PartyType"],
        savelayout: true,
        showcolumns: true,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid2.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
    });

    function _setMyReadonlys() {
        var readonlys = ["Cnt20", "Cnt40", "Cnt40hq", "CntType", "CntNumber",
            "ShipmentId", "CombineInfo", "BookingInfo", "PortDate", "HouseNo", "Vessel1","MasterNo",
        "PolCd", "PolName", "DestCd", "DestName","PodCd","PodName","CutBlDate","PorCd","PorName",
        "Atd", "Etd", "Eta", "Ata", "PortDate"];
        if ($("#Status").val() != "O" && $("#Status").val() != "H") {
            readonlys.push("Atd");
            readonlys.push("Ata");
        }
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
        //$("#BlRmk").attr('disabled', false);
    }

    _handler.afterEdit = function () {
        SetStatusToReadOnly(_setMyReadonlys);
        $("input[readonly][fieldname][dt='mt']").parent().find("button").attr("disabled", true);
        gridEditableCtrl({ editable: false, gridId: 'DnGrid' });
    }

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

    MenuBarFuncArr.AddMenu("MBAllocation", "glyphicon glyphicon-bell", "Allocation", function () {
        DefaultAllocation();
    });

    MenuBarFuncArr.AddMenu("MBExcepted", "glyphicon glyphicon-bell", "@Resources.Locale.L_ActManage_AnM", function () {
        var ShipmentId = $("#ShipmentId").val();
        if (!ShipmentId) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        initErrMsg($("#MBExcepted"), { 'GROUP_ID': groupId, 'CMP': cmp, 'STN': stn, 'UId': '', 'JobNo': ShipmentId }, true);
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

    MenuBarFuncArr.AddMenu("MBChangePod", "glyphicon glyphicon-th-list", "COD/改港", function () {
        var shipmentid = $("#ShipmentId").val();
        if (!shipmentid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var ata = $("#Ata").val();
        if (ata) {
            CommonFunc.Notify("", "已经ATA，不允许改港!", 500, "warning");
            return;
        }
        var uid = $("#UId").val();
        var isok = "N";
        var response = $.ajax({
            async: false,
            url: rootPath + "BookingAction/CheckEtdDate",
            type: 'POST',
            data: { shipmentid: shipmentid, checktype: "COD" },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                isok = data.IsOk;
            }
        });
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

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.Enabled(["MBCopy"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
    MenuBarFuncArr.Enabled(["MBEdoc"]);
    AddMBPrintFunc();
});

var _tran = "", _bu = "P";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking"), code: encodeURIComponent("Railway") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            var _shownull = { cd: '', cdDescp: '' };
            var ViaOptions = data.VIA || [];
            if (ViaOptions.length > 0)
                _mt = ViaOptions[0]["cd"];
            ViaOptions.unshift(_shownull);
            appendSelectOption($("#Via"), ViaOptions);

            trnOptions = data.PK || [];
            if (trnOptions.length > 0)
                _mt = trnOptions[0]["cd"];
            appendSelectOption($("#LoadingFrom"), trnOptions);
            appendSelectOption($("#LoadingTo"), trnOptions);

        }
    });
}
