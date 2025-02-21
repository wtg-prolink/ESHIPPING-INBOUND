var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid = null;
var $SubGrid2 = null;
var $SubGrid3 = null;
var $SubGrid4 = null;
var $ScuftGrid = null;
var $DnGrid = null;
var $SubGrid5 = null;
$(function () {

    $SubGrid = $("#SubGrid");

    $SubGrid2 = $("#SubGrid2");
    $SubGrid3 = $("#SubGrid3");
    $SubGrid4 = $("#SubGrid4");
     $ScuftGrid = $("#ScuftGrid");
     $DnGrid = $("#DnGrid");
    $SubGrid5 = $("#SubGrid5");

    genPartyGrid();

    var BookingLookup = {
        caption: _getLang("L_DNManage_BkDataSer", "訂艙資料查詢"),
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
            { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Year ', title: 'Year', index: 'Year', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Weekly', title: _getLang("L_ContainUsage_Week", "周別"), index: 'Weekly', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Status', title: _getLang("L_GateReserve_Status", "狀態"), index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: 'DN No', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'CntrQty', title: _getLang("L_BaseLookup_CntNumber", "櫃數"), index: 'CntrQty', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'PriceTerm', title: _getLang("L_ShipmentID_PayTermCd", "價格條款"), index: 'PriceTerm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'FreightTerm', title: 'FreightTerm', index: 'FreightTerm', width: 80, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POR', title: 'POR', index: 'POR', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POL', title: 'POL', index: 'POL', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Dest', title: 'Dest', index: 'Dest', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Feu', title: _getLang("L_DNApproveManage_Feu", "櫃量（Feu）"), index: 'Feu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Teu', title: _getLang("L_AirBookingSetup_Script_86", "櫃量（Teu）"), index: 'Teu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cur', title: _getLang("L_InvPkgSetup_Cur", "幣別"), index: 'Cur', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cost', title: _getLang("L_DNApproveManage_Cost", "物流成本"), index: 'Cost', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };

    _handler.saveUrl = rootPath + "SMSMI/BookingUpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];

    _handler.addData = function () {
        //初始化新增数据
        var data = { "FreightTerm": _bu, "TranType": _tran };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        _handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
        _handler.loadGridData("SubGrid4", $SubGrid4[0], [], [""]);
        _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);
        _handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
        gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
        gridEditableCtrl({ editable: false, gridId: 'DnGrid' });

        $SubGrid2.jqGrid('getGridParam', "removeAddRowButton")("SubGrid2");
        $SubGrid2.jqGrid('getGridParam', "removeDelRowButton")("SubGrid2");
        $SubGrid2.jqGrid('getGridParam', "removeCopyRowButton")("SubGrid2");
    }

    _handler.editData = function () {
        $SubGrid2.jqGrid('getGridParam', "removeAddRowButton")("SubGrid2");
        $SubGrid2.jqGrid('getGridParam', "removeDelRowButton")("SubGrid2");
        $SubGrid2.jqGrid('getGridParam', "removeCopyRowButton")("SubGrid2");
        MenuBarFuncArr.Disabled(["MBSendAddr"]);
    }

    function _setMyReadonlys() {
        var readonlys = ["ShipmentId", "CombineInfo", "BookingInfo",
            "PolCd", "PolName", "PorCd", "PorName", "PodCd", "PodName", "DestCd", "DestName", "Gw", "Cbm",
            "HouseNo", "Voyage1", "Etd", "Eta", "Atd", "Ata", "CreateBy", "CreateDate", "ModifyBy", "ModifyDate", "BConfirmDate"
        ];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
        //$("#BlRmk").attr('disabled', false);
    }

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.beforEdit = function () {
        return CheckStatus();
    }

    _handler.afterEdit = function () {
        SetStatusToReadOnly(_setMyReadonlys, false, "T");
        $("input[readonly][fieldname][dt='mt']").parent().find("button").attr("disabled", true);
        gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
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
        var containerArray3 = $SubGrid3.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub2"] = containerArray2;
        changeData["sub3"] = containerArray3;
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        var status = $("#Status").val();
        if (status == "B") {
            alert(_getLang("L_DTBookingSetup_Script_131", "請確認修改內容是否要通知訂艙Revise！！"));
        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "u_id": uid, ShipmentId: shipmentid, autoReturnData: false, "TranType": "T" },
            function (result) {
                //_topData = keyData["mt"];
                if (result.warning)
                    alert(result.warning);
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "MBSiInfo", "MBCopy", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod"]);
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
        {
            _handler.loadGridData("SubGrid2", $SubGrid2[0], data["sub2"], [""]);
            SetAddrModal();
        }
        else
            _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        if (data["sub3"])
            _handler.loadGridData("SubGrid3", $SubGrid3[0], data["sub3"], [""]);
        else
            _handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
        if (data["sub4"])
            _handler.loadGridData("SubGrid4", $SubGrid4[0], data["sub4"], [""]);
        else
            _handler.loadGridData("SubGrid4", $SubGrid4[0], [], [""]);
        if (data["sub4"])
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], data["sub4"], [""]);
        else
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);
        if (data["DnGrid"])
            _handler.loadGridData("DnGrid", $DnGrid[0], data["DnGrid"], [""]);
        else
            _handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);

        if (data["sub5"])
            _handler.loadGridData("SubGrid5", $SubGrid5[0], data["sub5"], [""]);
        else
            _handler.loadGridData("SubGrid5", $SubGrid5[0], [], [""]);

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
        if ($("[dt='mt'][chxName='ExtraSrv'][value='FORK']").is(':checked') == false) {
            $("#Fork").hide();
        } else {
            $("#Fork").show();
        }

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
        MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "MBSiInfo", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod", "MBSendAddr", "btn02"]);

        if (data["main"].length > 0)
        {
            var status = data["main"][0]["Status"];
            var confirmStatus = "CDEFGPOXHIJS";
            if (confirmStatus.indexOf(status) >= 0) {
                MenuBarFuncArr.Disabled(["btn02"]);
            } 
        } 

        ShowSMStatus(_handler.topData["Status"]);
        ChangeColor();
        setFix();
    }

    $("[dt='mt'][chxName='ExtraSrv'][value='FORK']").change(function () {
        if ($(this).is(':checked')) {
            $("#Fork").show();
        }
        else {
            $("#Fork").hide();
        }
    });

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "SMSMI/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
                //MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "MBSiInfo", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod"]);
            });
    }

    var BaseBooking_DnModel = [
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, editable: false, hidden: false },
        { name: 'BlLevel', title: _getLang("L_DNManage_BLType", "提单分类"), index: 'BlLevel', sorttype: 'string', editable: false, hidden: false, editable: true },
        { name: 'ExportNo', title: _getLang("L_DNApproveManage_ExportNo", "出口号码"), index: 'ExportNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Unicode', title: _getLang("L_DNApproveManage_Unicode", "统一编号"), index: 'Unicode', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'ApproveNo', title: _getLang("L_DNApproveManage_ApprovalNo", "批准文号"), index: 'ApproveNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        {
            name: 'AskTim', title: _getLang("L_DNManage_RqDelaDate", "要求报关时间"), index: 'AskTim', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d',
                defaultValue: ""
            }
        },
        { name: 'EdeclNo', title: _getLang("L_DNApproveManage_EdeclNo", "报关单号"), index: 'EdeclNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'DeclDate', title: _getLang("L_BaseLookup_DeclDate", "报关时间"), index: 'DeclDate', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'DeclRlsDate', title: _getLang("L_DNManage_RelDate", "放行时间"), index: 'DeclRlsDate', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'NextNum', title: _getLang("L_NEXT_NUMBER", "续页数"), index: 'NextNum', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Dremark', title: _getLang("L_BSCSSetup_Remark", "备注"), index: 'Dremark', sorttype: 'string', width: 120, hidden: false, editable: false }
    ];

    _handler.intiGrid("DnGrid", $DnGrid, {
        colModel: BaseBooking_DnModel, caption: _getLang("L_DNManage_DNDeclaInfo", "DN 報關信息"), delKey: ["DnNo", "DnNo"],
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

    var BaseBooking_ScufcolModel = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'UFid', title: 'UFid', index: 'UFid', sorttype: 'string', editable: false, hidden: true },
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        //{ name: 'Cuft', title: '类型', index: 'Cuft', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'L', title: _getLang("L_AirBookingSetup_Script_87", "长（cm）"), index: 'L', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'W', title: _getLang("L_AirBookingSetup_Script_88", "宽（cm）"), index: 'W', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'H', title: _getLang("L_AirBookingSetup_Script_89", "高（cm）"), index: 'H', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'Pkg', title: _getLang("L_DNManage_PkgNum", "件数"), index: 'Pkg', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'PkgUnit', title: _getLang("L_BaseLookup_Unit", "单位"), index: 'PkgUnit', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Vw', title: _getLang("L_AirBookingSetup_Script_90", "体积（m3）"), index: 'Vw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false },
        { name: 'Gw', title: _getLang("L_BaseLookup_Gw", "毛重"), index: 'Gw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false }
    ];

    _handler.intiGrid("ScuftGrid", $ScuftGrid, {
        colModel: BaseBooking_ScufcolModel, caption: _getLang("L_DNManage_DNSizeInfo", "DN尺寸資訊"), delKey: ["UId", "UFid"],
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });

    MenuBarFuncArr.AddMenu("MBVoid", "glyphicon glyphicon-bell", _getLang("L_DNManage_ReturnDeli", "退回出貨單位"), function () {
        $("#VoidsmRemark").val("");
        $("#VoidSM").modal("show");
        //DefaultVoidSM();
    });

    MenuBarFuncArr.AddMenu("btnFiBlRemark", "glyphicon glyphicon-bell", _getLang("L_MenuBar_BLRemarkEdit", "BL REMARK修改"), function () {
        BlRemarkModify();
    });


    MenuBarFuncArr.AddMenu("btn02", "glyphicon glyphicon-bell", _getLang("L_SMSMI_btn02", "Notify LSP"), function () {
        var uid = $("#UId").val();
        var status = $("#Status").val();
        if ("S" == status) {
            CommonFunc.Notify("", "Status in ISF Sending,so you cann't do this action!", 500, "warning");
            return false;
        }
        if (status == "I" || status == "C" || status == "D" || status == "H") {
            CommonFunc.Notify("", "You cann't operate this Action!", 500, "warning");
            return false;
        }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/notifytoLsp",
            type: 'POST',
            data: {
                "Uid": uid,
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                //var resJson = $.parseJSON(result)
                if (result.IsOk == "Y") {
                    CommonFunc.Notify("", result.message, 500, "success");
                }
                else {
                    alert(result.message);
                }
            }
        });
    });

    //MenuBarFuncArr.AddMenu("MBChangePod", "glyphicon glyphicon-th-list", _getLang("TLB_ChangePod", "COD/改港"), function () {
    //    var shipmentid = $("#ShipmentId").val();
    //    if (!shipmentid) {
    //        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "請先選擇一筆記錄"), 500, "warning");
    //        return;
    //    }
    //    var ata = $("#Ata").val();
    //    if (ata) {
    //        CommonFunc.Notify("", _getLang("L_BookingStatus_AleadyATA", "已经ATA，不允许改港!"), 500, "warning");
    //        return;
    //    }
    //    var uid = $("#UId").val();
    //    var isok = "N";
    //    var response = $.ajax({
    //        async: false,
    //        url: rootPath + "BookingAction/CheckEtdDate",
    //        type: 'POST',
    //        data: { shipmentid: shipmentid, checktype: "COD" },
    //        "error": function (xmlHttpRequest, errMsg) {
    //            alert(errMsg);
    //        },
    //        success: function (data) {
    //            isok = data.IsOk;
    //        }
    //    });
    //    if ("Y" == isok) {
    //        alert("Please Reciver LSP to change POD");
    //        return false;
    //    }
    //    if ("S" == isok) {
    //        alert(_getLang("L_BookingStatus_changePODmsg", "此笔已经合并过提单，请先解除合并提单后，再进行改港操作！"));
    //        return false
    //    }
    //    top.topManager.openPage({
    //        href: rootPath + "DNManage/ChangePodView/" + uid,
    //        title: 'Change Pod View',
    //        id: 'ChangePodView',
    //        reload: true
    //    });
    //    //$("#BookingCancel").modal("show");
    //});


    MenuBarFuncArr.AddMenu("pmsCombineInv", "glyphicon glyphicon-list-alt", _getLang("L_DNManage_CombIv", "合併Invoice"), function () {
        $("#CombineInvDialog").modal("show");
    });

    MenuBarFuncArr.AddMenu("MBSiInfo", "glyphicon glyphicon-th-list", "SI", function () {
        var ProfileCd = $("#ProfileCd").val();
        if (isEmpty(ProfileCd)) {
            alert(_getLang("L_AirBookingSetup_ProfileIDIsNull", "Profile ID 為空"));
            return;
        }
        if (ProfileCd.split(";").length == 1) {
            top.topManager.openPage({
                href: rootPath + "System/BSTDataSetup/" + UId + "?MenuBarPermiss=Y&Profile=" + ProfileCd,
                title: _getLang("L_BSTQuery_CustTranSet", "客戶交易設定"),
                id: 'BSTSetup',
                search: 'Profile=' + ProfileCd
            });
            return;
        }
        $("#SiQueryDialog").modal("show");
    });

    //var colModel2 = [
    //    { name: 'UId', title: 'UId', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
    //                    { name: 'UFid', title: 'UFid', index: 'UFid', width: 150, align: 'left', sorttype: 'string', hidden: true },
    //                    { name: 'DnNo', title: 'DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: true },
    //                    { name: 'Battery', title: _getLang("L_DNManage_Free", "免費"), index: 'Battery', width: 150, align: 'left', sorttype: 'string', hidden: true },
    //                    { name: 'IpartNo', title: _getLang("L_DNApproveManage_IpartNo", "對內機種名"), index: 'IpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'OpartNo', title: _getLang("L_DNApproveManage_OpartNo", "對外機種名"), index: 'OpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'GoodsDescp', title: _getLang("L_DNApproveManage_GoodsDescp", "商品名稱"), index: 'GoodsDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'ProdDescp', title: _getLang("L_DNApproveManage_ProdDescp", "商品品名"), index: 'ProdDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'ProdSpec', title: _getLang("L_DNApproveManage_ProdSpec", "商品描述"), index: 'ProdSpec', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Size', title: _getLang("L_DNApproveManage_Size", "寸別"), index: 'Size', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'BrandType', title: _getLang("L_DNApproveManage_BrandType", "品牌類型"), index: 'BrandType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Category', title: _getLang("L_DNApproveManage_Category", "類別"), index: 'Category', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'ProdType', title: _getLang("L_DNApproveManage_ProdType", "產品類型"), index: 'ProdType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'AssemblyId', title: _getLang("L_DNApproveManage_Assembly", "組裝型態"), index: 'AssemblyId', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'MaterialType', title: _getLang("L_DNApproveManage_MaterialType", "物料類型"), index: 'MaterialType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    {
    //                        name: 'ProductDate', title: _getLang("L_DNApproveManage_ProduceDate", "生產日期"), index: 'ProductDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
    //                        editoptions: myEditDateInit,
    //                        formatter: 'date',
    //                        formatoptions: {
    //                            srcformat: 'ISO8601Long',
    //                            newformat: 'Y-m-d',
    //                            defaultValue: ""
    //                        }
    //                    },
    //                    {
    //                        name: 'EstimateDate', title: _getLang("L_DNApproveManage_EstimateDate", "預計貨好時間"), index: 'EstimateDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
    //                        editoptions: myEditDateInit,
    //                        formatter: 'date',
    //                        formatoptions: {
    //                            srcformat: 'ISO8601Long',
    //                            newformat: 'Y-m-d',
    //                            defaultValue: ""
    //                        }
    //                    },
    //                    {
    //                        name: 'ActureDate', title: _getLang("L_DNApproveManage_ActureDate", "實際貨好時間"), index: 'ActureDate', width: 150, align: 'left',
    //                        sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
    //                        editoptions: myEditDateInit,
    //                        formatter: 'date',
    //                        formatoptions: {
    //                            srcformat: 'ISO8601Long',
    //                            newformat: 'Y-m-d',
    //                            defaultValue: ""
    //                        }
    //                    },
    //                    { name: 'Jqty', title: _getLang("L_DNApproveManage_Jqty", "工單量"), index: 'Jqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Pqty', title: _getLang("L_DNApproveManage_Pqty", "生產量"), index: 'Pqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Iqty', title: _getLang("L_DNApproveManage_Iqty", "投入量"), index: 'Iqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Wqty', title: _getLang("L_DNApproveManage_Wqty", "WIP"), index: 'Wqty', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'BstkdE', title: 'ShipTo PoNo', index: 'BstkdE', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'PoNo', title: _getLang("L_DNApproveManage_PoNo", "訂單號"), index: 'PoNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'PartNo', title: _getLang("L_DNApproveManage_PartNo", "客戶物料號"), index: 'PartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'SafetyModel', title: _getLang("L_DNApproveManage_SafetyModel", "舊物料號"), index: 'SafetyModel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Resolution', title: _getLang("L_DNApproveManage_Resolution", "解析度"), index: 'Resolution', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Brand', title: _getLang("L_DNApproveManage_Brand", "品牌"), index: 'Brand', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },

    //                    { name: 'OhsCode', title: _getLang("L_DNApproveManage_OhsCode", "出口國商品編碼"), index: 'OhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'IhsCode', title: _getLang("L_DNApproveManage_HisCode", "目的國商品編碼"), index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Cur1', title: _getLang("L_IpPart_Crncy", "幣別"), index: 'Cur1', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    //{ name: 'UnitPrice1', title: _getLang("L_DNApproveManage_UnitPrice", "單價")+' 1', index: 'UnitPrice1', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Value1', title: _getLang("L_DNDetailVeiw_Value", "金額"), index: 'Value1', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'PkgNum', title: _getLang("L_DNApproveManage_PkgNum", "大包裝件數"), index: 'PkgNum', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Cbm', title: _getLang("L_DNApproveManage_Cbm", "材積"), index: 'Cbm', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
    //                    { name: 'Gw', title: _getLang("L_DNApproveManage_Gw", "毛重"), index: 'Gw', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'OdnNo', title: _getLang("L_DNApproveManage_OdnNo", "原DN NO"), index: 'OdnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'FunType', title: _getLang("L_BaseLookup_FunType", "应用/功能分类"), index: 'FunType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
    //                    { name: 'Plant', title: 'Plant', index: 'Plant', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'SoNo', title: 'So No', index: 'SoNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'SoItem', title: 'So Item', index: 'SoItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'OneTime', title: 'One Time', index: 'OneTime', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'DelItem', title: 'Del Item', index: 'DelItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Qty', title: 'QTY', index: 'Qty', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Qtyu', title: _getLang("L_BaseLookup_Qtyu", "數量單位"), index: 'Qtyu', width: 70, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Cbmu', title: 'Cbmu', index: 'Cbmu', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'CntrStdQty', title: _getLang("L_CntrStdQty", "標準裝櫃量"), index: 'CntrStdQty', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'JobNo', title: 'Job No', index: 'JobNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'ProductLine', title: 'Product Line', index: 'ProductLine', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Cost', title: 'Cost', index: 'Cost', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'ItemNo', title: 'Item No', index: 'ItemNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Pmatn', title: 'PMATN', index: 'Pmatn', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'ReplacePart', title: 'replace part', index: 'ReplacePart', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Ihrez', title: 'Ihrez', index: 'Ihrez', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'IhrezE', title: 'IhrezE', index: 'IhrezE', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'DeliveryItem', title: 'Delivery Item', index: 'DeliveryItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Inspect', title: 'INSPECT', index: 'Inspect', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Ul', title: 'Ul', index: 'Ul', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Adds', title: 'Adds', index: 'Adds', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Du', title: 'DU', index: 'Du', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'DownFlag', title: 'Down Flag', index: 'DownFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'SendSfisFlag', title: 'SendSfis Flag', index: 'SendSfisFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'RefPart', title: 'Ref Part', index: 'RefPart', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'PrepareRmk', title: 'Prepare Rmk', index: 'PrepareRmk', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'Sloc', title: _getLang("L_DNFlowManage_Views_348", "儲位"), index: 'Sloc', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
    //                    { name: 'InterfaceCd', title: _getLang("L_InterfaceCd", "介面代碼"), index: 'InterfaceCd', width: 150, align: 'right', formatter: 'integer', hidden: true, editable: true }
    //];

    //_handler.intiGrid("SubGrid2", $SubGrid2, {
    //    colModel: colModel2, caption: _getLang("L_DNManage_Domestic", "內貿") + ' Booking-' + _getLang("L_TKBLQuery_DNDet", "DN明細"), delKey: ["UId", "PartyType"],
    //    savelayout: true,
    //    showcolumns: true, exportexcel: true, url: rootPath + "DNManage/DNDetailQuery", postData: { "conditions": _uid },
    //    onAddRowFunc: function (rowid) {
    //        var maxSeqNo = $SubGrid2.jqGrid("getCol", "PartyNO", false, "max");
    //        //if (typeof maxSeqNo === "undefined")
    //        // maxSeqNo = 0;
    //        //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
    //    },
    //});


    MenuBarFuncArr.Enabled(["MBEdoc"]);

    var colModel4 = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'Status', title: _getLang("L_GateReserve_Status", "狀態"), index: 'Status', width: 70, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: _getLang("L_DTBookingSetup_Script_132", "A:叫車;B:完成") } },
        { name: '', title: _getLang("L_DTBookingSetup_Date", "外倉名稱"), index: '', width: 100, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'TruckNo', title: _getLang("L_GateReserve_TruckNo", "入廠車號"), index: 'TruckNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'TruckNm', title: _getLang("L_GateReserve_Trucker", "卡車公司"), index: 'TruckNm', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Driver', title: _getLang("L_GateReserve_Driver", "司機"), index: 'Driver', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Tel', title: _getLang("L_BSCSSetup_CmpTel", "電話"), index: 'Tel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: '', title: _getLang("L_DTBookingSetup_Qty", "回補數量"), index: '', width: 120, align: 'right', formatter: 'integer', hidden: false }
    ];

    _handler.intiGrid("SubGrid4", $SubGrid4, {
        colModel: colModel4, caption: _getLang("L_DNManage_AdnormalInfo", "異常回補資訊"), delKey: ["UId", "PartyType"],

    });

    MenuBarFuncArr.MBCopy = function (thisItem) {
        CopyFunction(thisItem, $SubGrid);
    }

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

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CUR);
    }));

    //registBtnLookup($("#PodCdLookup"), {
    //    item: '#PodCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
    //        $("#PodCd").val(map.PortCd);
    //        $("#PodName").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#PodCd").val(rd.PORT_CD);
    //    $("#PodName").val(rd.PORT_NM);
    //}));

    //registBtnLookup($("#PolCdLookup"), {
    //    item: '#PolCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
    //        $("#PolCd").val(map.PortCd);
    //        $("#PolName").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#PolCd").val(rd.PORT_CD);
    //    $("#PolName").val(rd.PORT_NM);
    //}));

    registBtnLookup($("#PpolCdLookup"), {
        item: '#PpolCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PpolCd").val(map.PortCd);
            $("#PpolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpolCd").val(rd.PORT_CD);
        $("#PpolName").val(rd.PORT_NM);
    }));

    //registBtnLookup($("#BandCdLookup"), {
    //    item: '#BandCd', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
    //        $("#BandCd").val(map.PortCd);
    //        $("#BandDescp").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#BandCd").val(rd.PORT_CD);
    //    $("#BandDescp").val(rd.PORT_NM);
    //}));

    registBtnLookup($("#BandCdLookup"), {
        item: '#BandCd', url: rootPath + LookUpConfig.WhUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#BandCd").val(map.Cd);
            $("#BandDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "WH", undefined, function ($grid, rd, elem) {
        $("#BandCd").val(rd.CD);
        $("#BandDescp").val(rd.CD_DESCP);
    }));


    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
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

    MenuBarFuncArr.AddMenu("MBAllocation", "glyphicon glyphicon-font", "Allocation", function () {
        var uid = $("#UId").val();
        var status = $("#Status").val();
        if ("A" != status) {
            CommonFunc.Notify("", _getLang("L_DNManage_HasBeenBooking", "此笔资料已经订舱，不允许执行Allocation！"), 500, "warning");
            return;
        }

        if (!uid) {
            CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "请先选择一笔记录"), 500, "warning");
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "SMSMI/LspDistribution",
            type: 'POST',
            data: { suid: uid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (data.msg != "success") {
                    CommonFunc.Notify("", data.msg, 500, "warning");
                }
                else {
                    CommonFunc.Notify("", data.msg, 500, "success");
                }
                MenuBarFuncArr.MBCancel();
            }
        });
    });



    MenuBarFuncArr.AddMenu("MBSendAddr", "glyphicon glyphicon-th-large", commonLang["L_SMSMI_MBSendAddr"], function () {
        gridEditableCtrl({ editable: false, gridId: "SubGrid2" });
        $("#AddrModal").modal("show");
        gridEditableCtrl({ editable: true, gridId: "SubGrid2" });
    });

    $("#AddrClose").on("click", function () {
        gridEditableCtrl({ editable: false, gridId: "SubGrid2" }); 
    });
    $("#AddrClose2").on("click", function () {
        gridEditableCtrl({ editable: false, gridId: "SubGrid2" }); 
    });

    registBtnLookup($("#IncotermCdLookup"), {
        item: '#IncotermCd', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#IncotermCd").val(map.Cd);
            $("#IncotermDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#IncotermCd").val(rd.CD);
        $("#IncotermDescp").val(rd.CD_DESCP);
    }));

    registBtnLookup($("#StateLookup"), {
        item: '#State', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#State").val(map.StateCd);
            $("#Region").val(map.RegionCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#State").val(map.STATE_CD);
        $("#Region").val(rd.REGION_CD);
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
    }));

    //MenuBarFuncArr.AddMenu("btn01", "glyphicon glyphicon-th-list", _getLang("L_DNManage_ForeBk", "對外訂艙"), function () {

    //    var uid = $("#UId").val();
    //    if (!uid) {
    //        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "請先選擇一筆記錄"), 500, "warning");
    //        return;
    //    }
    //    var shipments = $("#ShipmentId").val();

    //    var iscontinue = window.confirm(_getLang("L_ActManage_is", "是否要對【") + shipments + "】" + _getLang("L_AirBookingSetup_Script_92", "發起對外訂艙？"));
    //    if (!iscontinue) {
    //        return;
    //    }

    //    ajaxHttp(rootPath + "BookingAction/FCLBookAction", { "Uid": uid, autoReturnData: false },
    //   function (result) {
    //       if (result.IsOk == "Y") {
    //           CommonFunc.Notify("", result.message, 500, "success");
    //       }
    //       else {
    //           CommonFunc.Notify("", result.message, 500, "warning");
    //       }
    //       MenuBarFuncArr.MBCancel();
    //       return true;
    //   });
    //});

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.Enabled(["MBCopy"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    //getSelectOptions();
    setSelectData(_selects);
    //AddMBPrintFunc();
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
            //var trnOptions = data.TTRN || [];
            //if (trnOptions.length > 0)
            //    _tran = trnOptions[0]["cd"];
            //appendSelectOption($("#TranType"), trnOptions);
            setSelectData(data);
        }
    });
}
function setSelectData(data) {
    var trnOptions = data.TCGT || [];
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
    var _shownull = { cd: '', cdDescp: '' };
    trnOptions.unshift(_shownull);
    appendSelectOption($("#CarType"), trnOptions);
    appendSelectOption($("#CarType1"), trnOptions);
    appendSelectOption($("#CarType2"), trnOptions);

    var tntOptions = data.TNT || [];
    appendSelectOption($("#TranType"), tntOptions);

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
        $("#CarType1").val(_handler.topData["CarType"]);
        $("#CarType2").val(_handler.topData["CarType"]);
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

function ShowSMStatus(_status, isosp) {
    var statsmap = {
        'A': _getLang("L_SMSMI_StatusA", "未到达"), 'B': _getLang("L_SMSMI_StatusB", "通知"), 'C': _getLang("L_SMSMI_StatusC", "通知报关"),
        'D': _getLang("L_UserQuery_Call", "报关确认"), 'I': _getLang("L_SMSMI_StatusI", "转关确认"), 'P': _getLang("L_SMSMI_StatusP", "POD"),
        'O': "Gate Out", 'G': _getLang("L_SMSMI_StatusG", "到厂"), 'H': _getLang("L_SMSMI_StatusH", "转关通知"),
        'V': "Void", 'Z': "Finish", 'R': "Archived", 'S': "ISF Sending", 'E':"E-Alert"
    };
    var descp = statsmap[_status] || _status;
    $("#Status_descp").text(descp);
    if (!isEmpty(isosp)) {
        $('#smrvTableDiv').css('display', 'block');
    } else {
        $('#smrvTableDiv').css('display', 'none');
    }
}


function setFix() {
    var LspCost = parseFloat($("#LspCost").val());
    var TruckCost = parseFloat($("#TruckCost").val());
    var FreightAmt = parseFloat($("#FreightAmt").val());
    var Gvalue = parseFloat($("#Gvalue").val());

    if (!isNaN(LspCost))
        $("#LspCost").val(LspCost.toFixed(2));
    if (!isNaN(TruckCost))
        $("#TruckCost").val(TruckCost.toFixed(2));
    if (!isNaN(FreightAmt))
        $("#FreightAmt").val(FreightAmt.toFixed(2));
    if (!isNaN(Gvalue))
        $("#Gvalue").val(Gvalue.toFixed(2));
}

function SetStatusToReadOnly(func, isconfirm, trantype) {
    if (isconfirm) {
        func();
        return;
    }
    var _status = $("#Status").val();

    if (_status == "A" || _status == "B" || isEmpty(_status)) {
        setdisabled(false);
        $("#TranType").attr('disabled', true);
        func();
    } else {
        setdisabled(true);
        _handler.gridEditableCtrl(false);
        if (isconfirm) {
            var readonlys = ["PodCd", "PodName", "HouseNo", "MasterNo", "Etd", "Eta", "Etd1", "Eta1", "Vessel1", "Voyage1",
                "Vessel2", "Voyage2", "Vessel3", "Voyage3", "Vessel4", "Voyage4", "BookingInfo", "PodCdLookup",
                "Etd2", "Eta2", "Etd3", "Eta3", "Etd4", "Eta4", "Carrier", "CarrierNm",
                "CutBlDate", "RlsCntrDate", "SignBack", "CutPortDate", "RcvDate", "CustomsDate", "PortRlsDate", "RcvDocDate"];
            SetArrayDisable(readonlys);
        } else {
            var readonlys = ["DnEtd", "Cw"];
            if (trantype == "L")
                readonlys = ["DnEtd", "Gw", "Cbm"];
            if (trantype == "F")
                readonlys = ["DnEtd", "Gw", "Tcbm"];
            if (trantype == "T")
                readonlys = ["DnEtd", "Gw", "Aqty", "Fqty"];
            SetArrayDisable(readonlys);
        }
    }
    //$("#BlRmk").attr('disabled', false);
}