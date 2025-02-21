var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
var _uid = "";
var clearFunc = function () {
    $("#CostCenter").val("");
    $("#CostCenterdescp").val("");
}

$(function () {
    $("#Cost").attr("Type", "password");
    $("#Amount1").attr("Type", "password");
    $("#FreightAmt").attr("Type", "password");
    $("#IssueFee").attr("Type", "password");
    $("#FobValue").attr("Type", "password");
    if (_rc == "B") {
        $("#Cost").attr("Type", "text");
        $("#Amount1").attr("Type", "text");
        $("#FreightAmt").attr("Type", "text");
        $("#IssueFee").attr("Type", "text");
        $("#FobValue").attr("Type", "text");
    }
    else if (_rc == "R") {
        $("#Amount1").attr("Type", "text");
    } else if (_rc == "C") {
        $("#Cost").attr("Type", "text");
        $("#FreightAmt").attr("Type", "text");
        $("#IssueFee").attr("Type", "text");
        $("#FobValue").attr("Type", "text");
    }
    var approvetype = approvetypes.split(';');
    var _statusgroup = [];
    $.each(approvetype, function (index, val) {
        var _val = val.split(':');
        var _object = {};
        if (_val.length >= 2) {
            _object.cd = _val[0];
            _object.cdDescp = _val[1];
        }
        _statusgroup.push(_object);
    });
    appendSelectOption($("#ApproveType"), _statusgroup);

    var approvetype = approveroles.split(';');
    var _statusgroup = [];
    $.each(approvetype, function (index, val) {
        var _val = val.split(':');
        var _object = {};
        if (_val.length >= 2) {
            _object.cd = _val[0];
            _object.cdDescp = _val[1];
        }
        _statusgroup.push(_object);
    });
    appendSelectOption($("#ApproveTo"), _statusgroup);

    var $SubGrid = $("#SubGrid");
    var $SerialGrid = $("#SerialGrid");
    var $PartyGrid = $("#PartyGrid");
    var $ScuftGrid = $("#ScuftGrid");

    var DnDetailLookup = {
        caption: "@Resources.Locale.L_DNManage_SerDN",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: "UId", title: "Uid", width: 250, sorttype: "string", hidden: true, viewable: false ,sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnType', title: '@Resources.Locale.L_ShipmentID_DNType', index: 'DnType', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'ApproveTo', title: '@Resources.Locale.L_DNApproveManage_ApproveTo', index: 'ApproveTo', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnShipType', title: '@Resources.Locale.L_DNApproveManage_DNCargoType', index: 'DnShipType', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'CombineInfo', title: '@Resources.Locale.L_DNApproveManage_CombineInfo', index: 'CombineInfo', width: 150, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cmp', title: '@Resources.Locale.L_GroupRelation_comID', index: 'Cmp', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Stn', title: '@Resources.Locale.L_DNApproveManage_Plant', index: 'Stn', width: 80, align: 'left', sorCmpLookupttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'PriceTerm', title: '@Resources.Locale.L_ShipmentID_PayTermCd', index: 'PriceTerm', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'TranType', title: '@Resources.Locale.L_DNDetailView_Scripts_187', index: 'TranType', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'TranDescp', title: '@Resources.Locale.L_DNApproveManage_TranDescp', index: 'TranDescp', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Port', title: 'Port', index: 'Port', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'PortDescp', title: 'PortDescp', index: 'PortDescp', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Dest', title: '@Resources.Locale.L_DNApproveManage_Pod', index: 'Dest', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'CntrQty', title: '@Resources.Locale.L_DNApproveManage_CntrQty', index: 'CntrQty', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Feu', title: '@Resources.Locale.L_DNApproveManage_Feu', index: 'Feu', width: 80, align: 'right', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Teu', title: '@Resources.Locale.L_AirBookingSetup_Script_86', index: 'Teu', width: 80, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cur', title: '@Resources.Locale.L_InvPkgSetup_Cur', index: 'Cur', width: 80, align: 'left', sorttype: 'string',  hidden: false , sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };
    var DruleItemUrl = "DNManage/GetDNDetailItem";

    _handler.saveUrl = rootPath + "DNManage/SaveDN";
    _handler.inquiryUrl = rootPath + "DNManage/GetDNDetail";;//LookUpConfig.NotifyUrl
    _handler.config = DnDetailLookup;

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        $("#UId").val("12342134");
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        _handler.loadGridData("SerialGrid", $SerialGrid[0], [], [""]);
        _handler.loadGridData("PartyGrid", $PartyGrid[0], [], [""]);
        _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);
        
    }

    _handler.beforEdit = function () {

        //var status = $("#Status").val();
        //var approveto = $("#ApproveTo").val();
        //if ("A" == approveto) {
        //    setdisabled(true);
        //    $("#ApproveType").attr('disabled', false);
        //    gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
        //}
    }

    _handler.editData = function () {
        _handler.gridEditableCtrl(false);
        showSmcuftGrid($("#TranType").val());
        gridEditableCtrl({ editable: true, gridId: 'PartyGrid' });
        var status = $("#Status").val();
        var approveto = $("#ApproveTo").val();
        if ("B" == status) {
            if ("A" == approveto) {
                setdisabled(true);
                $("#ApproveType").attr('disabled', false);
            }
        }
        return true;
    }

    _handler.beforSave = function () {
        var co = $("#CombineOther").val();
        if (co == "Y") {
            var tcbm = $("#Tcbm").val();
            if (isEmpty(tcbm)) {
                alert("@Resources.Locale.L_DNManage_TotalCbm");
                return false;
            }
        }
        var tcbm = $("#Tcbm").val();
        if (!isEmpty(tcbm) && co == "") {
            alert("@Resources.Locale.L_CombineOther_tip");
            return false;
        }
        var bandtype = $('#BandType').val();
        if (bandtype == "Y") {
            var bandcd = $('#BandCd').val();
            if (isEmpty(bandcd)) {
                alert("@Resources.Locale.L_DNManage_BandEbpty");
                return false;
            }
        }
        var frtterm = $("#FreightTerm").val();
        if (isEmpty(frtterm)) {
            alert("Freight Term is not null");
            return false;
        }
    }

    _handler.saveData = function (dtd) {
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var containerArray2 = $SerialGrid.jqGrid('getGridParam', "arrangeGrid")();
        var containerArray3 = $PartyGrid.jqGrid('getGridParam', "arrangeGrid")();
        var containerArray4 = $ScuftGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub2"] = containerArray2;
        changeData["sub3"] = containerArray3;
        changeData["sub4"] = containerArray4;
        var uid = $("#UId").val();
        var dnno = $("#DnNo").val();
        var CombineOther = $("#CombineOther").val();
        var Tcbm = $("#Tcbm").val();
        if (Tcbm != "")
        {

        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData":encodeURIComponent(JSON.stringify(changeData)), "uid": uid,"dnno":dnno, autoReturnData: false,loading:true },
            function (result) {

                _handler.topData = { UId: result.UId };
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
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
        if (data["sub2"])
            _handler.loadGridData("SerialGrid", $SerialGrid[0], data["sub2"], [""]);
        else
            _handler.loadGridData("SerialGrid", $SerialGrid[0], [], [""]);
        if (data["sub3"])
            _handler.loadGridData("PartyGrid", $PartyGrid[0], data["sub3"], [""]);
        else
            _handler.loadGridData("PartyGrid", $PartyGrid[0], [], [""]);
        if (data["sub4"])
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], data["sub4"], [""]);
        else
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        showtruckdiv($("#TranType").val());
        showSmcuftGrid($("#TranType").val());
        requirePayTerm($("#FreightTerm").val());
        setFix();
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + DruleItemUrl, { uId: map.UId ,loading:true},
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    var colModel = [
	    { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'PartyType', title: '@Resources.Locale.L_DNDetailVeiw_PartyType', index: 'PartyType', editoptions: gridLookup(getop("TypeDescp")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'TypeDescp', title: '@Resources.Locale.L_DNDetailVeiw_TypeDescp', index: 'TypeDescp', sorttype: 'string', width: 120, hidden: false, editable: true },
        { name: 'PartyNo', title: '@Resources.Locale.L_BSCSSetup_PartyNo', index: 'PartyNO', editoptions: gridLookup(getpartyop("PartyName")), edittype: 'custom', sorttype: 'string', width: 140, hidden: false, editable: true },
        { name: 'PartyName', title: '@Resources.Locale.L_DNDetailVeiw_PartyName', index: 'PartyName', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'PartyName2', title: '@Resources.Locale.L_DNDetailVeiw_PartyName2', index: 'PartyName2', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'PartyName3', title: '@Resources.Locale.L_DNDetailVeiw_PartyName3', index: 'PartyName3', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'PartyName4', title: '@Resources.Locale.L_DNDetailVeiw_PartyName4', index: 'PartyName4', sorttype: 'string', width: 200, hidden: false, editable: true },
        //{ name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Contact', title: '@Resources.Locale.L_BSCSDataQuery_PartyAttn', index: 'Contact', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Tel', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'Tel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'FaxNo', title: '@Resources.Locale.L_BSCSSetup_CmpFax', index: 'FaxNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Mail', title: '@Resources.Locale.L_DNDetailVeiw_Mail', index: 'Mail', sorttype: 'string', width: 300, hidden: false, editable: true },
        { name: 'PartAddr', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr1', index: 'PartAddr', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartAddr2', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr2', index: 'PartAddr2', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartAddr3', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr3', index: 'PartAddr3', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartAddr4', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr4', index: 'PartAddr4', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PartAddr5', title: '@Resources.Locale.L_BSCSDataQuery_PartAddr5', index: 'PartAddr5', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Cnty', title: '@Resources.Locale.L_BSCSDataQuery_Cnty', index: 'Cnty', editoptions: { maxlength: 2 }, width: 80, hidden: false, editable: true },
        { name: 'CntyNm', title: '@Resources.Locale.L_BSCSDataQuery_CntyNm', index: 'CntyNm', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'City', title: '@Resources.Locale.L_DNDetailVeiw_City', index: 'City', editoptions: { maxlength: 3 }, width: 80, hidden: false, editable: true },
        { name: 'CityNm', title: '@Resources.Locale.L_DNDetailVeiw_CityNm', index: 'CityNm', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'State', title: 'State', index: 'State', sorttype: 'string', width: 100, editoptions: { maxlength: 2 }, hidden: false, editable: true },
        { name: 'Zip', title: 'Zip', index: 'Zip', sorttype: 'string', width: 100, editoptions: { maxlength: 10}, hidden: false, editable: true },
        { name: 'TaxNo', title: '@Resources.Locale.L_BSCSDataQuery_TaxNo', index: 'TaxNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'OrderBy', title: '@Resources.Locale.L_DNDetailVeiw_OrderBy', index: 'OrderBy', sorttype: 'string', width: 100, hidden: true, editable: true }
    ];

    _handler.intiGrid("PartyGrid", $PartyGrid, {
        colModel: colModel, caption: 'DN Party', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $PartyGrid.jqGrid("getCol", "PartyNO", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $PartyGrid.setColProp('PartyType', { editable: true });
            } else {
                $PartyGrid.setColProp('PartyType', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $PartyGrid.setColProp('PartyType', { editable: true });
        }
    });

    function Scufgetop(name) {
        var _name = name;
        var city_op = getLookupOp("ScuftGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'PkgUnit', map.Cd, null);
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $PartyGrid, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'PkgUnit', rd.CD, null);
                $(elem).val(rd.CD);
            }));
        return city_op;
    }

    var ScufcolModel = [
	    { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'UFid', title: 'UFid', index: 'UFid', sorttype: 'string', editable: false, hidden: true },
        { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 100, hidden: true, editable: false },
        //{ name: 'Cuft', title: '类型', index: 'Cuft', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'L', title: '@Resources.Locale.L_AirBookingSetup_Script_87', index: 'L', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'W', title: '@Resources.Locale.L_AirBookingSetup_Script_88', index: 'W', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'H', title: '@Resources.Locale.L_AirBookingSetup_Script_89', index: 'H', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'Pkg', title: '@Resources.Locale.L_DNManage_PkgNum', index: 'Pkg', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
        { name: 'PkgUnit', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'PkgUnit', editoptions: gridLookup(Scufgetop("TypeDescp")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Vw', title: '@Resources.Locale.L_AirBookingSetup_Script_90', index: 'Vw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false }
    ];
    _handler.intiGrid("ScuftGrid", $ScuftGrid, {
        colModel: ScufcolModel, caption: '@Resources.Locale.L_DNManage_DNSizeInfo', delKey: ["UId", "UFid"],
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        },
        afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
            var BkdData = $('#ScuftGrid').jqGrid("getGridParam", "data");
            var Lamt = 0, StBoAmt = 0, BlAmt = 0, MtBoAmt = 0;
            var SumStBlAmt = $('#BkpGrid').jqGrid("getCol", "BlAmt", false, "sum");

            var l = $("#ScuftGrid").jqGrid('getCell', rowid, "L");
            var w = $("#ScuftGrid").jqGrid('getCell', rowid, "W");
            var h = $("#ScuftGrid").jqGrid('getCell', rowid, "H");
            var Pkg = $("#ScuftGrid").jqGrid('getCell', rowid, "Pkg");

            var vw = l * w * h * Pkg / 1000000;
            $('#ScuftGrid').jqGrid('setCell', rowid, "Vw", CommonFunc.formatFloatNoComma(vw, 4));

            var SumVw = $ScuftGrid.jqGrid("getCol", "Vw", false, "sum");
            if (SumVw != 0) {
                $("#Cbm").val(CommonFunc.formatFloatNoComma(SumVw, 4));
            }
        }
    });
    function getop(name) {
        var _name = name;
        var city_op = getLookupOp("PartyGrid",
            {
                url: rootPath + LookUpConfig.PartyTypeUrl,
                config: LookUpConfig.PartyTypeLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'TypeDescp', map.CdDescp, null);
                    setGridVal($grid, selRowId, 'OrderBy', map.OrderBy, null);
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "PT", $PartyGrid, function ($grid, rd, elem, rowid) {
                //$("#PartyType").val(rd.CD);
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'TypeDescp', rd.CD_DESCP, null);
                setGridVal($grid, selRowId, 'OrderBy', rd.ORDER_BY, null);
                $(elem).val(rd.CD);
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
        var city_op = getLookupOp("PartyGrid",
            {
                url: rootPath +  "Common/GetPartyNoDataN",
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (returnObj, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'PartyName', returnObj.PartyName, null);
                    setGridVal($grid, selRowId, 'Mail', returnObj.PartyMail, null);
                    setGridVal($grid, selRowId, 'PartAddr', returnObj.PartAddr1, null);
                    setGridVal($grid, selRowId, 'PartAddr2', returnObj.PartAddr2, null);
                    setGridVal($grid, selRowId, 'PartAddr3', returnObj.PartAddr3, null);
                    setGridVal($grid, selRowId, 'Contact', returnObj.PartyAttn, null);
                    setGridVal($grid, selRowId, 'Tel', returnObj.PartyTel, null);
                    setGridVal($grid, selRowId, 'State', returnObj.State, null);
                    setGridVal($grid, selRowId, 'Zip', returnObj.Zip, null);
                    setGridVal($grid, selRowId, 'Cnty', returnObj.Cnty, null);
                    setGridVal($grid, selRowId, 'CntyNm', returnObj.CntyNm, null);
                    setGridVal($grid, selRowId, 'City', returnObj.City, null);
                    setGridVal($grid, selRowId, 'CityNm', returnObj.CityNm, null);

                    setGridVal($grid, selRowId, 'PartAddr4', returnObj.PartAddr4, null);
                    setGridVal($grid, selRowId, 'PartAddr5', returnObj.PartAddr5, null);
                    setGridVal($grid, selRowId, 'PartyName2', returnObj.PartyName2, null);
                    setGridVal($grid, selRowId, 'PartyName3', returnObj.PartyName3, null);
                    setGridVal($grid, selRowId, 'PartyName4', returnObj.PartyName4, null);
                    setGridVal($grid, selRowId, 'FaxNo', returnObj.PartyFax, null);
                    setGridVal($grid, selRowId, 'TaxNo', returnObj.TaxNo, null);
                    return returnObj.PartyNo;
                }
            }, {
                baseConditionFunc: function () {
                    var selRowId = $("#PartyGrid").jqGrid('getGridParam', 'selrow');
                    var PartyType = $("#PartyGrid").jqGrid('getCell', selRowId, 'PartyType');
                    var condition = "PARTY_TYPE LIKE '%" + PartyType + "%'";
                    if (PartyType == "SP" || PartyType == "FS") {
                        var ProfileCd = $("#ProfileCd").val();
                        condition = "PARTY_TYPE LIKE '%" + PartyType + "%' &PROFILE=" + ProfileCd ;
                    }
                    return condition;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $PartyGrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'PartyName', rd.PARTY_NAME, null);
                setGridVal($grid, selRowId, 'Mail', rd.PARTY_MAIL, null);
                setGridVal($grid, selRowId, 'PartAddr', rd.PART_ADDR1, null);
                setGridVal($grid, selRowId, 'PartAddr2', rd.PART_ADDR2, null);
                setGridVal($grid, selRowId, 'PartAddr3', rd.PART_ADDR3, null);
                setGridVal($grid, selRowId, 'Contact', rd.PARTY_ATTN, null);
                setGridVal($grid, selRowId, 'Tel', rd.PARTY_TEL, null);
                setGridVal($grid, selRowId, 'State', rd.STATE, null);
                setGridVal($grid, selRowId, 'Zip', rd.ZIP, null);
                setGridVal($grid, selRowId, 'Cnty', rd.CNTY, null);
                setGridVal($grid, selRowId, 'CntyNm', rd.CNTY_NM, null);
                setGridVal($grid, selRowId, 'City', rd.CITY, null);
                setGridVal($grid, selRowId, 'CityNm', rd.CITY_NM, null);
                setGridVal($grid, selRowId, 'PartyNo', rd.PARTY_NO, 'lookup');

                setGridVal($grid, selRowId, 'PartAddr4', rd.PART_ADDR4, null);
                setGridVal($grid, selRowId, 'PartAddr5', rd.PART_ADDR5, null);
                setGridVal($grid, selRowId, 'PartyName2', rd.PARTY_NAME2, null);
                setGridVal($grid, selRowId, 'PartyName3', rd.PARTY_NAME3, null);
                setGridVal($grid, selRowId, 'PartyName4', rd.PARTY_NAME4, null);
                setGridVal($grid, selRowId, 'FaxNo', rd.PARTY_FAX, null);
                setGridVal($grid, selRowId, 'TaxNo', rd.TAX_NO, null);
                //$(elem).val(rd.PartyNo);
            }));
        city_op.param = '';
        return city_op;
    }

    $('#BandType').change(function () {
        var bandtype = $(this).children('option:selected').val()
        if (bandtype == "Y") {
            var pod = $('#Pod').val();
            $('#BandCd').val(pod);
        }
    })

    var colModel = [
                        { name: 'UId', title: 'UId', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'UFid', title: 'UFid', index: 'UFid', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'DnNo', title: 'DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'Battery', title: '@Resources.Locale.L_AirBookingSetup_Battery', index: 'Battery', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'IpartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'IpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'OpartNo', title: '@Resources.Locale.L_DNApproveManage_OpartNo', index: 'OpartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'GoodsDescp', title: '@Resources.Locale.L_DNApproveManage_GoodsDescp', index: 'GoodsDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'ProdDescp', title: '@Resources.Locale.L_DNApproveManage_ProdDescp', index: 'ProdDescp', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'ProdSpec', title: '@Resources.Locale.L_DNApproveManage_ProdSpec', index: 'ProdSpec', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Size', title: '@Resources.Locale.L_DNApproveManage_Size', index: 'Size', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'BrandType', title: '@Resources.Locale.L_DNApproveManage_BrandType', index: 'BrandType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Category', title: '@Resources.Locale.L_DNApproveManage_Category', index: 'Category', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'ProdType', title: '@Resources.Locale.L_DNApproveManage_ProdType', index: 'ProdType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'AssemblyId', title: '@Resources.Locale.L_DNApproveManage_Assembly', index: 'AssemblyId', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'MaterialType', title: '@Resources.Locale.L_DNApproveManage_MaterialType', index: 'MaterialType', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        {
                            name: 'ProductDate', title: '@Resources.Locale.L_DNApproveManage_ProduceDate', index: 'ProductDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                            editoptions: myEditDateInit,
                            formatter: 'date',
                            formatoptions: {
                                srcformat: 'ISO8601Long',
                                newformat: 'Y-m-d',
                                defaultValue: ""
                            }
                        },
                        {
                            name: 'EstimateDate', title: '@Resources.Locale.L_DNApproveManage_EstimateDate', index: 'EstimateDate', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                            editoptions: myEditDateInit,
                            formatter: 'date',
                            formatoptions: {
                                srcformat: 'ISO8601Long',
                                newformat: 'Y-m-d',
                                defaultValue: ""
                            }
                        },
                        {
                            name: 'ActureDate', title: '@Resources.Locale.L_DNApproveManage_ActureDate', index: 'ActureDate', width: 150, align: 'left',
                            sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                            editoptions: myEditDateInit,
                            formatter: 'date',
                            formatoptions: {
                                srcformat: 'ISO8601Long',
                                newformat: 'Y-m-d',
                                defaultValue: ""
                            }
                        },
                        { name: 'Jqty', title: '@Resources.Locale.L_DNApproveManage_Jqty', index: 'Jqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Pqty', title: '@Resources.Locale.L_DNApproveManage_Pqty', index: 'Pqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Iqty', title: '@Resources.Locale.L_DNApproveManage_Iqty', index: 'Iqty', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Wqty', title: '@Resources.Locale.L_DNApproveManage_Wqty', index: 'Wqty', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'BstkdE', title: 'ShipTo PoNo', index: 'BstkdE', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'PoNo', title: '@Resources.Locale.L_DNApproveManage_PoNo', index: 'PoNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'PartNo', title: '@Resources.Locale.L_DNApproveManage_PartNo', index: 'PartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'SafetyModel', title: '@Resources.Locale.L_DNApproveManage_SafetyModel', index: 'SafetyModel', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Resolution', title: '@Resources.Locale.L_DNApproveManage_Resolution', index: 'Resolution', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Brand', title: '@Resources.Locale.L_DNApproveManage_Brand', index: 'Brand', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                       
                        { name: 'OhsCode', title: '@Resources.Locale.L_DNApproveManage_OhsCode', index: 'OhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'IhsCode', title: '@Resources.Locale.L_DNApproveManage_HisCode', index: 'IhsCode', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Cur1', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur1', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'UnitPrice1', title: '@Resources.Locale.L_DNApproveManage_UnitPrice 1', index: 'UnitPrice1', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        //{ name: 'UnitPrice2', title: '單價2', index: 'UnitPrice2', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Value1', title: '@Resources.Locale.L_DNDetailVeiw_Value', index: 'Value1', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'PkgNum', title: '@Resources.Locale.L_DNApproveManage_PkgNum', index: 'PkgNum', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Cbm', title: '@Resources.Locale.L_DNApproveManage_Cbm', index: 'Cbm', width: 70, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
                        { name: 'Gw', title: '@Resources.Locale.L_DNApproveManage_Gw', index: 'Gw', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
                        { name: 'OdnNo', title: '@Resources.Locale.L_DNApproveManage_OdnNo', index: 'OdnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                        { name: 'Plant', title: 'Plant', index: 'Plant', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'SoNo', title: 'So No', index: 'SoNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'SoItem', title: 'So Item', index: 'SoItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'OneTime', title: 'One Time', index: 'OneTime', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'DelItem', title: 'Del Item', index: 'DelItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Qty', title: 'QTY', index: 'Qty', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Qtyu', title: '@Resources.Locale.L_BaseLookup_Qtyu', index: 'Qtyu', width: 70, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Cbmu', title: 'Cbmu', index: 'Cbmu', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'CntrStdQty', title: '@Resources.Locale.L_CntrStdQty', index: 'CntrStdQty', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'JobNo', title: 'Job No', index: 'JobNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'ProductLine', title: 'Product Line', index: 'ProductLine', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Cost', title: 'Cost', index: 'Cost', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'ItemNo', title: 'Item No', index: 'ItemNo', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Pmatn', title: 'PMATN', index: 'Pmatn', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'ReplacePart', title: 'replace part', index: 'ReplacePart', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Ihrez', title: 'Ihrez', index: 'Ihrez', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'IhrezE', title: 'IhrezE', index: 'IhrezE', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'DeliveryItem', title: 'Delivery Item', index: 'DeliveryItem', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Inspect', title: 'INSPECT', index: 'Inspect', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Ul', title: '@Resources.Locale.L_FCLBooking_Ul', index: 'Ul', width: 100, align: 'left', sorttype: 'string', hidden: true, editable: true  },
                        { name: 'Adds', title: 'Adds', index: 'Adds', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Du', title: '@Resources.Locale.L_FCLBooking_Du', index: 'Du', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true, formatter: "select", editoptions: { value: 'Y:Yes;N:No' } },
                        { name: 'DownFlag', title: 'Down Flag', index: 'DownFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'SendSfisFlag', title: 'SendSfis Flag', index: 'SendSfisFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'RefPart', title: 'Ref Part', index: 'RefPart', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'PrepareRmk', title: 'Prepare Rmk', index: 'PrepareRmk', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Sloc', title: '@Resources.Locale.L_DNFlowManage_Views_348', index: 'Sloc', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'InterfaceCd', title: '@Resources.Locale.L_InterfaceCd', index: 'InterfaceCd', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Nw', title: '@Resources.Locale.L_BaseLookup_Nw', index: 'Nw', width: 100, align: 'right', sorttype: 'string', hidden: false }
    ];

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel, caption: '@Resources.Locale.L_DNManage_DNPartNo', delKey: ["UFid", "UId"],
        savelayout: true,
        showcolumns: true,
        onAddRowFunc: function (rowid) {

            //var maxSeqNo = $SubGrid.jqGrid("getCol", "DnNo", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            var _dnno=$("#DnNo").val();
            $SubGrid.jqGrid('setCell', rowid, "DnNo", _dnno);
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
        },
        afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {

        }
    });

    var colModel2 = [
        { name: 'UId', title: 'ID', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
        { name: 'UFid', title: 'ID', index: 'UFid', width: 150, align: 'left', sorttype: 'string', hidden: true },
        { name: 'DnNo', title: '@Resources.Locale.L_DNApproveManage_DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'PartNo', title: '@Resources.Locale.L_DNApproveManage_IpartNo', index: 'PartNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'SerialNumber', title: '@Resources.Locale.L_DNApproveManage_SerialNumber', index: 'Jqty', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'JobNo', title: '@Resources.Locale.L_DNApproveManage_JobNo', index: 'Pqty', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];
    _handler.intiGrid("SerialGrid", $SerialGrid, {
        colModel: colModel2, caption: '@Resources.Locale.L_DNManage_DNSerial', delKey: ["UFid", "UId"],
        onAddRowFunc: function (rowid) {
            //var maxSeqNo = $SubGrid.jqGrid("getCol", "DnNo", false, "max");
            //if (typeof maxSeqNo === "undefined")
            // maxSeqNo = 0;
            //$SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);

            var _dnno = $("#DnNo").val();
            $SerialGrid.jqGrid('setCell', rowid, "DnNo", _dnno);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SerialGrid.setColProp('PartNo', { editable: true });
            } else {
                $SerialGrid.setColProp('PartNo', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $SerialGrid.setColProp('PartNo', { editable: true });
        }
    });

    registBtnLookup($("#PorteCdLookup"), {
        item: '#PorteCd', url: rootPath + LookUpConfig.PorteUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#PorteCd").val(map.Cd);
            $("#PorteDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TVST", undefined, function ($grid, rd, elem) {
        $("#PorteCd").val(rd.CD);
        $("#PorteDescp").val(rd.CD_DESCP);
    }));


    //Region
    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
            $("#RegionNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
        $("#RegionNm").val(rd.CD_DESCP);
    }));


    //啟運港
    registBtnLookup($("#PortLookup"), {
        item: '#Port', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Port").val(map.CntryCd + map.PortCd);
            $("#PortDescp").val(map.PortNm);
        }
    }, { focusItem: $("#Port") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Port").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PortDescp").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PolTruckLookup"), {
        item: '#PolTruck', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PolTruck").val(map.PortCd);
            $("#PoltruckNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolTruck").val(rd.PORT_CD);
        $("#PoltruckNm").val(rd.PORT_NM);
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

    //LookUpConfig.WhLookup = {

    registBtnLookup($("#BandCdLookup"), {
        item: '#BandCd', url: rootPath + LookUpConfig.WhUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#BandCd").val(map.Cd);
            $("#BandDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "WH", undefined, function ($grid, rd, elem) {
        $("#BandCd").val(rd.CD);
        $("#BandDescp").val(rd.CD_DESCP);
    }));

    registBtnLookup($("#PodTruckLookup"), {
        item: '#PodTruck', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodTruck").val(map.PortCd);
            $("#PodNmTruck").val(map.PortNm);
            $("#State").val(map.State);
            $("#Region").val(map.Region);
            $("#Region").trigger('autocompletechangefunc', map.Region);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodTruck").val(rd.PORT_CD);
        $("#PodNmTruck").val(rd.PORT_NM);
        $("#State").val(rd.STATE);
        $("#Region").val(rd.REGION);
        $("#Region").trigger('autocompletechangefunc', rd.REGION);
    }));


    //目的地
    registBtnLookup($("#PodLookup"), {
        item: '#Pod', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pod").val(map.CntryCd + map.PortCd);
            $("#PodNm").val(map.PortNm);
        }
    }, { focusItem: $("#Pod") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pod").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PolLookup"), {
        item: '#Pol', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pol").val(map.CntryCd + map.PortCd);
            $("#PolNm").val(map.PortNm);
        }
    }, { focusItem: $("#Pol") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pol").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PaytermCdLookup"), {
        item: '#PaytermCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PaytermCd").val(map.CntryCd + map.PortCd);
            $("#PaytermNm").val(map.PortNm);
        }
    }, { focusItem: $("#PaytermCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PaytermCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PaytermNm").val(rd.PORT_NM);
    }));

    $('#TrackWay').change(function () {
        $("#CarType").removeAttr('required');
        var _trackway = $(this).children('option:selected').val();//这就是selected的值 
        if (_trackway == "F") {
            var _tranmode = $("#TranType").val();
            if (_tranmode == "T") {
                //$("#CarType").attr('required', true);
            }
        }
       
    });

    $("#FreightTerm").change(function () {
        requirePayTerm($(this).val());
    });


    var TermLookup = {
        caption: "@Resources.Locale.L_DNManage_SerTranType",
        sortname: "Cd",
        refresh: false,
        columns: [{ name: "Cd", title: "@Resources.Locale.L_DNApproveManage_TranMode", width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: "CdDescp", title: "@Resources.Locale.L_BaseLookup_Nm", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    };

    registBtnLookup($("#TranModeLookup"), {
        item: '#TranType', url: rootPath + LookUpConfig.TrackingTranModeUrl, config: TermLookup, param: "", selectRowFn: function (map) {
            $("#TranType").val(map.Cd);
            $("#TranTypeDescp").val(map.CdDescp);
            showtruckdiv(map.Cd);
            showSmcuftGrid(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TNT", undefined, function ($grid, rd, elem) {
        $("#TranType").val(rd.CD);
        $("#TranTypeDescp").val(rd.CD_DESCP);
        showtruckdiv(rd.CD);
        showSmcuftGrid(rd.CD);
    }));

    registBtnLookup($("#IncotermLookup"), {
        item: '#Incoterm', url: rootPath + LookUpConfig.DlvTermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Incoterm").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#Incoterm").val(rd.CD);
    }, function ($grid, elem) {
        $("#Incoterm").val("");
    }));

    registBtnLookup($("#StateLookup"), {
        item: '#State', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#State").val(map.StateCd);
            //$("#Region").val(map.RegionCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#State").val(map.STATE_CD);
        //$("#Region").val(rd.REGION_CD);
    }));

    registBtnLookup($("#CostCenterLookup"), {
        item: '#CostCenter', url: rootPath + LookUpConfig.CostCenterUrl, config: LookUpConfig.CostCenterLookup, param: "", selectRowFn: function (map) {
            $("#CostCenter").val(map.CostCenter);
            $("#CostCenterdescp").val(map.Dep);
        }
    }, {
        baseConditionFunc: function () {
            return "1=1";
        }
    }, LookUpConfig.GetCostCenterAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CostCenter").val(rd.COST_CENTER);
        $("#CostCenterdescp").val(rd.DEP);
    }));
    

    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
    }));
    
    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBSearch","MBDel", "MBCopy", "MBApply", "MBApprove"]);
    MenuBarFuncArr.Enabled(["MBEdoc"]);

    //$("#SeqNo").removeAttr('required');
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBSHow","MBBooking","MBEdit","MBExcepted","MBQuad","MBVoid","MBCancelB","MBBookingInfo","MBShowFee","MBSpellCTN","MBCancelSpellCTN","MBReloadDN","MBReloadQTY"]);
        _endGrid($SubGrid);
        _endGrid($SerialGrid);
        _endGrid($PartyGrid);
        var _mainCmpData = { UId: _uid }; //_uid
        gridEditableCtrl({ editable: false, gridId: "SubGrid" });
        gridEditableCtrl({ editable: false, gridId: "SerialGrid" });
        gridEditableCtrl({ editable: false, gridId: "PartyGrid" });
        gridEditableCtrl({ editable: false, gridId: "ScuftGrid" });
        _handler.loadMainData(_mainCmpData);
        editable = false;
        _subEdit = 0;
    }

    function _endGrid($grid) {//结束grid的编辑状态
        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
        $grid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $grid.jqGrid('getGridParam', "endEdit")();
    }

    MenuBarFuncArr.AddMenu("MBQuad", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_Quar", function () {

        var dn_no = $("#DnNo").val();
        if (dn_no.indexOf("_F") > -1) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_IsQua", 500, "success");
            return;
        }
        var _confirm = confirm("@Resources.Locale.L_DNManage_SureQua");
        if (!_confirm) {
            return;
        }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "DNManage/QuadrupleSingle",
            type: 'POST',
            data: { dnno: dn_no },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            success: function (data) {
                CommonFunc.Notify("", data.message, 500, "success");
                _uid = data.DnNo;
                _handler.topData = { UId: data.DnNo };
                MenuBarFuncArr.MBCancel();
            }
        });
    });

    MenuBarFuncArr.AddMenu("MBReloadDN", "glyphicon glyphicon-bell", "Reload DN", function () {
        var confirmReload = window.confirm("@Resources.Locale.L_DNManage_Reload");
        if (!confirmReload) return;
        var dnNo = $("#DnNo").val();
        var sapId = $("#SapId").val();
        var uid = $("#UId").val();
        $.ajax({
            async: true,
            url: rootPath + "DNManage/ReloadDN",
            type: 'POST',
            data: {
                "DnNo": dnNo, "SapId": sapId,"uid":uid
            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.errMsg, 500, "warning");
            },
            success: function (data) {
                _handler.topData = { UId: uid };
                //_topData = keyData["mt"];
                if (data.errMsg) {
                    alert(data.errMsg);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(data);
                MenuBarFuncArr.MBCancel();
            }
        });
    });

    MenuBarFuncArr.AddMenu("MBReloadQTY", "glyphicon glyphicon-bell", "Reload @Resources.Locale.L_BaseLookup_Qty", function () {
        var status = $("#Status").val();
        var approveto = $("#ApproveTo").val();
        if (status == "D" && approveto == "A") {
            alert("@Resources.Locale.L_DNManage_InitialState");
            return;
        }
        if (status == "D") {
            alert("@Resources.Locale.L_DNManage_NIntBk");
            return;
        }
        var smstatus = $("#SmStatus").val();
        var cmp = $("#Cmp").val();
        
        if (smstatus != "I" && smstatus != "C" && smstatus != "D") {
            alert("@Resources.Locale.L_DNManage_CannotDo");
            return;
        }
      
        var confirmReload = window.confirm("@Resources.Locale.L_DNDetailView_Scripts_208");
        if (!confirmReload) return;
        var dnNo = $("#DnNo").val();
        var sapId = $("#SapId").val();
        var uid = $("#UId").val();
        $.ajax({
            async: true,
            url: rootPath + "DNManage/ReloadQuantity",
            type: 'POST',
            data: {
                "DnNo": dnNo, "SapId": sapId, "uid": uid
            },
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.errMsg, 500, "warning");
            },
            success: function (data) {
                _handler.topData = { UId: uid };
                //_topData = keyData["mt"];
                if (data.errMsg) {
                    alert(data.errMsg);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(data);
                MenuBarFuncArr.MBCancel();
            }
        });
    });

    _handler.afterEdit = function () {
        SetPanelReadonly();
    }

    MenuBarFuncArr.AddMenu("MBBooking", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_LaunchBk", function () {
        var dnitems = $("#DnNo").val();
        var uids = $("#UId").val();
        var trantype = $("#TranType").val();
        if (isEmpty(trantype)) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_TranTypeEmpty", 500, "warning");
            return;
        }
        if (trantype == "A") {
            var length = $('#ScuftGrid').length
            if (length == 0) {
                alert("@Resources.Locale.L_DNManageController_tip1");
                return false;
            }
            for (var i = 1 ; i <= length; i++) {
                var l = $("#ScuftGrid").jqGrid('getCell', i, "L");
                var w = $("#ScuftGrid").jqGrid('getCell', i, "W");
                var h = $("#ScuftGrid").jqGrid('getCell', i, "H");
                if (l == "" || w == "" || h == "") {
                    alert("@Resources.Locale.L_DNDetailView_DimTip");
                    return false;
                }
            }
        }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "DNManage/InitiateBooking",
            type: 'POST',
            data: {
                "pushdata": dnitems,
                "uids": uids
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

    var listBar = [];
    listBar.push({
        menuId: "MBBookingInfo", menuName: "@Resources.Locale.L_DNManage_BookingInfo", menuFunc: function () {
            var shipmentid = $("#ShipmentId").val();
            if (shipmentid == "") {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_DataNoBKInfo", 500, "warning");
                return;
            }
            $.ajax({
                async: true,
                url: rootPath + "DNManage/GetUidBySmId",
                type: 'POST',
                data: { shipmentid: shipmentid },
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                },
                success: function (data) {
                    var _trantype = data.trantype;
                    var _url = "DNManage/FCLBConfirmSetup/";
                    if (data.trantype == "A") {
                        var _url = "DNManage/AirBookingSetup/";
                    }
                    if (data.trantype == "D") {
                        var _url = "DNManage/DEBookingSetup/";
                    }
                    if (data.trantype == "T") {
                        var _url = "DNManage/DTBookingSetup/";
                    }
                    if (data.trantype == "F") {
                        var _url = "DNManage/FCLBooking/";
                    }
                    if (data.trantype == "E") {
                        var _url = "DNManage/IEBookingSetup/";
                    }
                    if (data.trantype == "L") {
                        var _url = "DNManage/LCLBooking/";
                    }
                    if (data.trantype == "R") {
                        var _url = "DNManage/RailwayBookingSetup/";
                    }

                    if (data.uid) { //data.trantype
                        top.topManager.openPage({
                            href: rootPath + _url + data.uid,
                            title: '@Resources.Locale.L_DNManage_BookingInfo',
                            id: 'BookingDetailinfo'
                        });
                    }
                }
            });
        }, menuCss: " glyphicon glyphicon-bell"
    });
    listBar.push({
        menuId: "MBExcepted", menuName: "@Resources.Locale.L_ActManage_AnM", menuFunc: function () {
            var dnno = $("#DnNo").val();
            if (!dnno) {
                CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                return;
            }
            initErrMsg($("#MBExcepted"), { 'GROUP_ID': groupId, 'CMP': cmp, 'STN': stn, 'UId': '', 'JobNo': dnno }, true);
        }, menuCss: " glyphicon glyphicon-bell"
    });
    listBar.push({
        menuId: "MBCancelB", menuName: "@Resources.Locale.L_DNManage_Sus", menuFunc: function () {
            var dnno = $("#DnNo").val();
            var approvetype = $("#ApproveType").val();
            var ApproveBack = $("#ApproveBack").val();

            if (ApproveBack == 'Y' || dnno.indexOf('_H') > -1) {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasSusDelv", 500, "warning");
                return;
            }
            var _confirm = confirm("@Resources.Locale.L_DNManage_ImpSusDelv");
            if (!_confirm) {
                return;
            }

            $.ajax({
                async: true,
                url: rootPath + "DNManage/DNViewCancelB",
                type: 'POST',
                data: { dnno: dnno, approvetype: approvetype },
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                },
                success: function (data) {
                    if (data.IsOk == "Y") {
                        CommonFunc.Notify("", data.message, 500, "success");
                    } else {
                        CommonFunc.Notify("", data.message, 500, "warning");
                    }
                    if (!isEmpty(_uid)) {
                        _handler.topData = { UId: _uid };
                        MenuBarFuncArr.MBCancel();
                    }
                }
            });
        }, menuCss: " glyphicon glyphicon-bell"
    });
    listBar.push({
        menuId: "MBVoid", menuName: "@Resources.Locale.L_MenuBar_Audit", menuFunc: function () {
            var dnno = $("#DnNo").val();
            var _confirm = confirm("@Resources.Locale.L_DNManage_ConfDiscDN");
            if (!_confirm) {
                return;
            }
            $.ajax({
                async: true,
                url: rootPath + "DNManage/VoidDN",
                type: 'POST',
                data: { dnno: dnno },
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                },
                success: function (data) {
                    if (data.IsOk == "Y") {
                        CommonFunc.Notify("", data.message, 500, "success");
                    } else {
                        CommonFunc.Notify("", data.message, 500, "warning");
                    }
                    var _mainCmpData = { UId: _uid }; //_uid
                    _handler.loadMainData(_mainCmpData);
                }
            });
        }, menuCss: " glyphicon glyphicon-bell"
    });

    MenuBarFuncArr.AddDDLMenu("MBSHow", " glyphicon glyphicon-print", "@Resources.Locale.L_DNManage_Ma", function () { }, null, listBar);


    var DnLookup = {
        caption: "DnNo",
        sortname: "DnNo",
        refresh: false,
        columns: [{ name: "DnNo", title: "DnNo", width: 140, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: "PortPol", title: "@Resources.Locale.L_DNDetailView_Scripts_204", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: "PortPolnm", title: "@Resources.Locale.L_SMIPM_PolNm", width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: "PortDest", title: "@Resources.Locale.L_DNDetailView_Scripts_205", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: "PortDescp", title: "@Resources.Locale.L_DNDetailView_Scripts_206", width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: 'Cnt20', title: '@Resources.Locale.L_DNDetailVeiw_Cnt20', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: 'Cnt40', title: '@Resources.Locale.L_DNDetailVeiw_Cnt40', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: 'Cnt40hq', title: '@Resources.Locale.L_DNDetailVeiw_Cnt40hq', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: 'CntType', title: '@Resources.Locale.L_DNApproveManage_CntrType', width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: 'CntNumber', title: '@Resources.Locale.L_DNFlowManage_CntNumber', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                { name: "CombineInfo", title: "@Resources.Locale.L_BaseLookup_CombineInfo", width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };
    var DnNoUrl = "DNManage/DNSpellQueryData"

    MenuBarFuncArr.AddMenu("MBSpellCTN", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_Consol", function () {
        var shipmentid = $("#ShipmentId").val();
        if (shipmentid != "") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_RdHasBk", 500, "warning");
            return;
        }
    });
    var _config = $.extend({ multiselect: true }, DnLookup);
    registBtnLookup($("#MBSpellCTN"), {
        url: rootPath + DnNoUrl, config: _config, param: "", selectRowFn: function (map) {
        }
    }, {
        baseConditionFunc: function () {
            var PortDest = $("#PortDest").val();
            var _TranType = $("#TranType").val();
            var _DnNo = $("#TranType").val();
            return " PORT_DEST='" + PortDest + "' AND DN_NO ! ='" + $("#DnNo").val() + "' AND TRAN_TYPE='" + $("#TranType").val() + "'";
        },
        responseMethod: function (data) {
            var shipmentid = $("#ShipmentId").val();
            if (shipmentid != "") {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_RdHasBk", 500, "warning");
                return;
            }
            
            var combineinfo = $("#CombineInfo").val();
            if (combineinfo != "") {

                var truthBeTold = window.confirm("@Resources.Locale.L_DNManage_HasCon");
                if (!truthBeTold) {
                    return;
                }
            }

            var dnno=$("#DnNo").val();
            var dnnos = '';
            var cmp = "";
            $.each(data, function (index, val) {
                if (index == 0) {
                    cmp = val.Cmp;
                }
                else {
                    if (cmp != val.Cmp) {
                        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SelcSameCmp", 500, "warning");
                        return;
                    }
                }
                dnnos += val.DnNo + ",";
            });
            $.ajax({
                async: true,
                url: rootPath + "DNManage/SpellCTN",
                type: 'POST',
                data: {
                    "dnno":dnno,
                    "pushdata": dnnos
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
                    $("#CombineInfo").val(result.CombineInfo)
                }
            });
        }
    });

    MenuBarFuncArr.AddMenu("MBCancelSpellCTN", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_CanCon", function () {
        var shipmentid = $("#ShipmentId").val();
        if (shipmentid != "") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_RdHasBk", 500, "warning");
            return;
        }
        var dnno = $("#DnNo").val();
        $.ajax({
            async: true,
            url: rootPath + "DNManage/CancelSpellCTN",
            type: 'POST',
            data: {
                "pushdata": dnno
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
                $("#CombineInfo").val('')
            }
        });

    });
    showtruckdiv();
    requirePayTerm();
    getSelectOptions();
});

function SetPanelReadonly()
{
    $("#PanelReadonly input").attr('disabled', true);
    $("#PanelReadonly select").attr('disabled', true);
    var uid = $("#UId").val();
    var isok = $.ajax({
        async: false,
        url: rootPath + "DNManage/DNCheckBooking",
        type: 'POST',
        data: { uid: uid },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            if (data.IsOk == "N") {
                var status = $("#Status").val();
                var approveto = $("#ApproveTo").val();
                setdisabled(true);
                if ("A" == approveto) {
                    $("#ApproveType").attr('disabled', false);
                }
                if ("C" == data.Torder) {
                    $("#DnRmark").attr('disabled', true);
                } else {
                    $("#DnRmark").attr('disabled', false);
                }
                gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
                gridEditableCtrl({ editable: false, gridId: 'PartyGrid' });
            } else {
                var updatengc = ["Nw", "Gw", "Cbm"];
                for (var i = 0; i < updatengc.length; i++) {
                    $("#" + updatengc[i]).attr('disabled', false);
                    $("#" + updatengc[i]).parent().find("button").attr("disabled", false);
                }
            }
        }
    });
    var readonlys = [ "Pol", "PolNm","PolTruck","PodTruck","Cur",
         "Pod", "PodNm", "State", "Region", "RegionNm", "Cost", "Amount1", "FreightAmt", "IssueFee", "FobValue"];

    var approveto = $("#ApproveTo").val();
    if ("A" != approveto) {
        readonlys.push("ApproveType");
    }
    for (var i = 0; i < readonlys.length; i++) {
        $("#" + readonlys[i]).attr('disabled', true);
        $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
    }
}

var _mt = "";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("DNDetailView"), dn:"Y"},
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var _shownull = { cd: '', cdDescp: '' };
            var mtOptions = data.TDTK || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            mtOptions.unshift(_shownull);
            appendSelectOption($("#TrackWay"), mtOptions);

            mtOptions = data.TVAK || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            mtOptions.unshift(_shownull);
            appendSelectOption($("#DnType"), mtOptions);

            mtOptions = data.TCGT || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            mtOptions.unshift(_shownull);
            appendSelectOption($("#CargoType"), mtOptions);

            mtOptions = data.TCT || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            mtOptions.unshift(_shownull);
            appendSelectOption($("#AtranType"), mtOptions);

            mtOptions = data.PK || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            mtOptions.unshift(_shownull);
            appendSelectOption($("#LoadingFrom"), mtOptions);
            appendSelectOption($("#LoadingTo"), mtOptions);

            trnOptions = data.TDT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            trnOptions.unshift(_shownull);
            appendSelectOption($("#CarType"), trnOptions);

            ViaOptions = data.VIA || [];
            if (ViaOptions.length > 0)
                _mt = ViaOptions[0]["cd"];
            ViaOptions.unshift(_shownull);
            appendSelectOption($("#Via"), ViaOptions);

            if (_handler.topData) {
                $("#TrackWay").val(_handler.topData["TrackWay"]);
                $("#DnType").val(_handler.topData["DnType"]);
                $("#CargoType").val(_handler.topData["CargoType"]);
                $("#AtranType").val(_handler.topData["AtranType"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
                $("#CarType").val(_handler.topData["CarType"]);
                $("#Via").val(_handler.topData["Via"]);
            }
        }
    });
}

function showSmcuftGrid(trantype) {
    gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
    if (trantype == "A" || trantype == "E" || trantype == "D" || trantype == "T"|| trantype=="L") {
        gridEditableCtrl({ editable: true, gridId: 'ScuftGrid' });
    }
}

function showtruckdiv(trantype) {
    $("#Battery").removeAttr('required');
    if (trantype == "A") {
        $("#Battery").attr('required', true);
    }
    $("#LoadingFrom").removeAttr('required');
    $("#LoadingTo").removeAttr('required');
   
    if (trantype == "F" || trantype == "L") {
        $("#LoadingFrom").attr('required', true);
        $("#LoadingTo").attr('required', true);
        $("#Via option[value=\"SBT\"]").remove();
        $("#Via option[value=\"PBT\"]").remove();
    } else {
        $("#LoadingFrom").removeAttr('required');
        $("#LoadingTo").removeAttr('required');
    }
    if (trantype == "T") {
        $('#CarTypelabelDiv').css('display', 'block');
        $('#CarTypeDiv').css('display', 'block');
        $('#TrackWay').attr('required', true);
        $('#BandType').attr('required', true);
        //$('#CarType').attr('required', true);
        $('#PolTruck').attr('required', true);
    } else {
        $('#CarTypelabelDiv').css('display', 'none');
        $('#CarTypeDiv').css('display', 'none');
        $("#TrackWay").removeAttr('required');
        $("#CarType").removeAttr('required');
        $("#PolTruck").removeAttr('required');
        $('#BandType').removeAttr('required');
    }
    
    if (trantype == "D" || trantype == "T") {
        $('#polbtndiv').css('display', 'none');
        $('#polnmdiv').css('display', 'none');
        $('#podbtndiv').css('display', 'none');
        $('#podnmdiv').css('display', 'none');

        $('#poltruckbtndiv').css('display', 'block');
        $('#polnmtruckdiv').css('display', 'block');
        $('#podtruckbtndiv').css('display', 'block');
        $('#podnmtruckdiv').css('display', 'block');
    } else {
        $('#polbtndiv').css('display', 'block');
        $('#polnmdiv').css('display', 'block');
        $('#podbtndiv').css('display', 'block');
        $('#podnmdiv').css('display', 'block');

        $('#poltruckbtndiv').css('display', 'none');
        $('#polnmtruckdiv').css('display', 'none');
        $('#podtruckbtndiv').css('display', 'none');
        $('#podnmtruckdiv').css('display', 'none');
    }


}

function requirePayTerm(freightterm) {
    if (freightterm == "O") {
        $("#PaytermCd").attr('required', true);
        $("#PaytermNm").attr('required', true);
    } else {
        $("#PaytermCd").removeAttr('required');
        $("#PaytermNm").removeAttr('required');
    }
}

function setFix() {
    var amount1 =   parseFloat($("#Amount1").val());
    var FreightAmt = parseFloat($("#FreightAmt").val());
    var IssueFee = parseFloat($("#IssueFee").val());
    var FobValue = parseFloat($("#FobValue").val());
    var Cost = parseFloat($("#Cost").val());

    if (!isNaN(amount1))
        $("#Amount1").val(amount1.toFixed(2));
    if (!isNaN(FreightAmt))
        $("#FreightAmt").val(FreightAmt.toFixed(2));
    if (!isNaN(IssueFee))
        $("#IssueFee").val(IssueFee.toFixed(2));
    if (!isNaN(FobValue))
        $("#FobValue").val(FobValue.toFixed(2));
    if (!isNaN(Cost))
        $("#Cost").val(Cost.toFixed(2));
}