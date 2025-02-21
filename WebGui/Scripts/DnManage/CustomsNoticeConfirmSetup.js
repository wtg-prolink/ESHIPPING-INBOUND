var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var _tran = "3";
$(function () {
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $DnGrid = $("#DnGrid");
    var DruleLookup = {
        caption: "@Resources.Locale.L_DNManage_DelcSetupSer",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
            { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Year ', title: 'Year', index: 'Year', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Weekly', title: '@Resources.Locale.L_ContainUsage_Week', index: 'Weekly', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
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
    var DruleItemUrl = "DNManage/GetCustomsBookItem";

    _handler.saveUrl = rootPath + "DNManage/SaveCustomsBook";
    _handler.inquiryUrl = rootPath + "DNManage/GetCustomsBookData";;//LookUpConfig.NotifyUrl
    _handler.config = DruleLookup;

    _handler.addData = function () {
        var data = {};
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        data[_handler.key] = uuid();
        setFieldValue([data]);
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
            _handler.loadGridData("SubGrid2", $SubGrid[1], data["sub2"], [""]);
        else
            _handler.loadGridData("SubGrid2", $SubGrid[1], [], [""]);
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
        MenuBarFuncArr.Enabled(["MBConfirm", "MBEdoc"]);
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var containerArray = $DnGrid.jqGrid('getGridParam', "arrangeGrid")();
        changeData["dngrid"] = containerArray;
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, "ShipmentId": shipmentid, autoReturnData: false, "TranType": "F" },
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                MenuBarFuncArr.MBCancel();
                return true;
            });
    }

    _handler.beforEdit = function () {
        
    }

    _handler.afterEdit = function () {
        //SetStatusToReadOnly();
        setdisabled(true);
        SetBrokerDisable(false);
        _handler.gridEditableCtrl(false);
        gridEditableCtrl({ editable: true, gridId: 'DnGrid' });
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + DruleItemUrl, { uId: map.UId },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: returnPartyModel("SubGrid", $SubGrid), caption: 'Party', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "PartyNo", false, "max");
        },
        beforeSelectRowFunc: function (rowid) {
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('PartyType', { editable: true });
            } else {
                $SubGrid.setColProp('PartyType', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
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
        { name: 'Du', title: '@Resources.Locale.L_FCLBooking_Du', index: 'Du', sorttype: 'string', width: 110, hidden: false, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_TKBLQuery_DNDet', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid2.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
    });

    var Dn_colmodel = [
	     { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 120, editable: false, hidden: false },
         { name: 'BlLevel', title: '@Resources.Locale.L_DNManage_BLType', index: 'BlLevel', sorttype: 'string', editable: false, hidden: false, editable: false },
         { name: 'ExportNo', title: '@Resources.Locale.L_DNApproveManage_ExportNo', index: 'ExportNo', sorttype: 'string', width: 120, hidden: false, editable: false },
         { name: 'Unicode', title: '@Resources.Locale.L_DNApproveManage_Unicode', index: 'Unicode', sorttype: 'string', width: 120, hidden: false, editable: false },
         { name: 'ApproveNo', title: '@Resources.Locale.L_DNApproveManage_ApprovalNo', index: 'ApproveNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        {
            name: 'AskTim', title: '@Resources.Locale.L_DNManage_RqDelaDate', index: 'AskTim', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
            editoptions: myEditDateInit,
            formatter: 'date',
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d',
                defaultValue: ""
            }
        },
         { name: 'EdeclNo', title: '@Resources.Locale.L_DNApproveManage_EdeclNo', index: 'EdeclNo', sorttype: 'string', width: 120, hidden: false, editable: true },
          {
              name: 'DeclDate', title: '@Resources.Locale.L_BaseLookup_DeclDate', index: 'DeclDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
              editoptions: myEditDateInit,
              formatter: 'date',
              formatoptions: {
                  srcformat: 'ISO8601Long',
                  newformat: 'Y-m-d',
                  defaultValue: ""
              }
          },
         {
             name: 'DeclRlsDate', title: '@Resources.Locale.L_DNManage_RelDate', index: 'DeclRlsDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
             editoptions: myEditDateInit,
             formatter: 'date',
             formatoptions: {
                 srcformat: 'ISO8601Long',
                 newformat: 'Y-m-d',
                 defaultValue: ""
             }
         },
          { name: 'NextNum', title: '@Resources.Locale.L_NEXT_NUMBER', index: 'NextNum', sorttype: 'string', width: 120, hidden: false, editable: true },
         { name: 'Dremark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Dremark', sorttype: 'string', width: 120, hidden: false, editable: true }
    ];

    _handler.intiGrid("DnGrid", $DnGrid, {
        colModel: Dn_colmodel, caption: '@Resources.Locale.L_BSTQuery_DNDeclaInfo', delKey: ["DnNo", "DnNo"],
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

    registBtnLookup($("#OimporterLookup"), {
        item: '#Oimporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Oimporter").val(map.PartyNo);
            $("#OimporterNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            //return "PartyType_spot=like&PartyType=" + $("#PartyType").val();
            return "PARTY_TYPE LIKE '%SH%'";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#OimporterNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#OexporterLookup"), {
        item: '#Oexporter', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Oexporter").val(map.PartyNo);
            $("#OexporterNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            //return "PartyType_spot=like&PartyType=" + $("#PartyType").val();
            return "PARTY_TYPE LIKE '%CS%'";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#OexporterNm").val(rd.PARTY_NAME);
    }));

    MenuBarFuncArr.Enabled(["MBEdoc"]);


    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBInvalid", "MBSearch"]);

    MenuBarFuncArr.AddMenu("MBConfirm", " glyphicon glyphicon-usd", "@Resources.Locale.L_DNManage_DeclaConf", function () {
        var decldate=$("#DeclDate").val();
        var exportno = $("#ExportNo").val();
        var edeclno = $("#EdeclNo").val();
        //if (isEmpty(decldate) || isEmpty(exportno) || isEmpty(edeclno)) {
        //    alert("报关时间，出口号码和报关单号不能为空!");
        //    return;
        //}
        if (!confirm("@Resources.Locale.L_DNManage_SureDeclara")) {
            return;
        }
        var declrlsdate = $("#DeclRlsDate").val();
        $.ajax({
            async: true,
            url: rootPath + "BookingAction/DECLBookConfirm",
            type: 'POST',
            data: {
                "Uid": _uid,
                "DeclRlsDate": declrlsdate
            },
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.message, 500, "warning");
            },
            success: function (result) {
                //var resJson = $.parseJSON(result)
                CommonFunc.Notify("", result.message, 500, "success");
                MenuBarFuncArr.MBCancel();
            }
        });
    });

    //$("#SeqNo").removeAttr('required');
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    $("#Voyage1Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel1 || "";
        showVessel(vessel);
    });
    function showVessel(vessel) {
        if (isEmpty(vessel)) {
            CommonFunc.Notify("", "@Resources.Locale.L_SeaTransport_NoVesRe", 500, "warning");
            return;
        }
        $('#VesselMapFrame').attr("src", "https://ipdev.lsphub.com/iport/ShowVessel.aspx?vsl_nm=" + encodeURIComponent(vessel));
        $('#VesselMap').modal('show');
    };

    $("#DeclRlsDate").parent().find("button").attr("display", "none");
    $("#DeclRlsDate").parent().hide();
    $("#DeclDate").parent().hide();
});

function SetBrokerDisable(disabled) {
    var readonlys = ["ExportNo", "EdeclNo", "ApproveNo", "Unicode", "DeclDate", "DeclRlsDate", "BrokerInfo", "Remark", "NextNum"];
    if ($("#Border").val() == "C") {
        alert("@Resources.Locale.L_DNManage_DeclarationConf");
        disabled = false;
        readonlys = ["DeclRlsDate", "ExportNo", "EdeclNo","DeclDate","Remark"];
    }
    if ($("#Border").val() == "H") {
        disabled = false;
        readonlys = ["Remark"];
    }
    for (var i = 0; i < readonlys.length; i++) {
        $("#" + readonlys[i]).attr('disabled', disabled);
        $("#" + readonlys[i]).parent().find("button").attr("disabled", disabled);
    }
}

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
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
            }
        }
    });
}
