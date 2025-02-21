var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

$(function () {
    _initCombineInv();
    var $SubGrid = $("#SubGrid");
    var $SubGrid2 = $("#SubGrid2");
    var $SubGrid3 = $("#SubGrid3");
    var $ScuftGrid = $("#ScuftGrid");
    var $subbddGrid = $("#subbddGrid");
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
        _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);
        getAutoNo("ShipmentId", "rulecode=SMRV_NO&cmp=" + cmp);
        gridEditableCtrl({ editable: false, gridId: 'ScuftGrid' });
        gridEditableCtrl({ editable: false, gridId: 'DnGrid' });
    }

    function _setMyReadonlys() {
        var readonlys = ["ShipmentId", "CombineInfo", "BookingInfo",
         "PolCd", "PolName", "PorCd", "PorName", "PodCd", "PodName", "DestCd", "DestName","Gw","Cbm",
         "HouseNo", "Voyage1", "Etd", "Eta", "Atd", "Ata", "CreateBy", "CreateDate", "ModifyBy", "ModifyDate"
        ];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('disabled', true);
            $("#" + readonlys[i]).parent().find("button").attr("disabled", true);
        }
        //$("#BlRmk").attr('disabled', false);
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
            alert("@Resources.Locale.L_DTBookingSetup_Script_131");
        }
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, ShipmentId: shipmentid, autoReturnData: false, "TranType": "T" },
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
                MenuBarFuncArr.Enabled(["MBEdoc", "MBExcepted", "MBAllocation", "btn01", "MBSiInfo", "MBCopy", "MBCallCar", "MBVoid", "MBPrint", "MBPreview", "btnFiBlRemark", "MBChangePod"]);
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
        if (data["sub3"])
            _handler.loadGridData("SubGrid3", $SubGrid3[0], data["sub3"], [""]);
        else
            _handler.loadGridData("SubGrid3", $SubGrid3[0], [], [""]);
        if (data["subbdd"])
            _handler.loadGridData("subbddGrid", $subbddGrid[0], data["subbdd"], [""]);
        else
            _handler.loadGridData("subbddGrid", $subbddGrid[0], [], [""]);
        if (data["sub3"])
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], data["sub3"], [""]);
        else
            _handler.loadGridData("ScuftGrid", $ScuftGrid[0], [], [""]);
        if (data["DnGrid"])
            _handler.loadGridData("DnGrid", $DnGrid[0], data["DnGrid"], [""]);
        else
            _handler.loadGridData("DnGrid", $DnGrid[0], [], [""]);
       
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

    _handler.intiGrid("ScuftGrid", $ScuftGrid, {
        colModel: BaseBooking_ScufcolModel, caption: '@Resources.Locale.L_DNManage_DNSizeInfo', delKey: ["UId", "UFid"],
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
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

    MenuBarFuncArr.AddMenu("MBVoid", "glyphicon glyphicon-bell", "@Resources.Locale.L_DNManage_ReturnDeli", function () {
        $("#VoidsmRemark").val("");
        $("#VoidSM").modal("show");
        //DefaultVoidSM();
    });

    MenuBarFuncArr.AddMenu("btnFiBlRemark", "glyphicon glyphicon-bell", "@Resources.Locale.L_MenuBar_BLRemarkEdit", function () {
        BlRemarkModify();
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
            alert("Please Reciver LSP to change POD");
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

    MenuBarFuncArr.AddMenu("MBSiInfo", "glyphicon glyphicon-th-list", "SI", function () {
        var ProfileCd = $("#ProfileCd").val();
        if (isEmpty(ProfileCd)) {
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

    var colModel2 = [
        { name: 'UId', title: 'UId', index: 'UId', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'UFid', title: 'UFid', index: 'UFid', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'DnNo', title: 'DnNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: true },
                        { name: 'Battery', title: '@Resources.Locale.L_DNManage_Free', index: 'Battery', width: 150, align: 'left', sorttype: 'string', hidden: true },
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
                        //{ name: 'UnitPrice1', title: '@Resources.Locale.L_DNApproveManage_UnitPrice 1', index: 'UnitPrice1', width: 150, align: 'right', sorttype: 'string', hidden: false, editable: true },
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
                        { name: 'Ul', title: 'Ul', index: 'Ul', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Adds', title: 'Adds', index: 'Adds', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Du', title: 'DU', index: 'Du', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'DownFlag', title: 'Down Flag', index: 'DownFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'SendSfisFlag', title: 'SendSfis Flag', index: 'SendSfisFlag', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'RefPart', title: 'Ref Part', index: 'RefPart', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'PrepareRmk', title: 'Prepare Rmk', index: 'PrepareRmk', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'Sloc', title: '@Resources.Locale.L_DNFlowManage_Views_348', index: 'Sloc', width: 150, align: 'left', sorttype: 'string', hidden: true, editable: true },
                        { name: 'InterfaceCd', title: '@Resources.Locale.L_InterfaceCd', index: 'InterfaceCd', width: 150, align: 'right', formatter: 'integer', hidden: true, editable: true }
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_DNManage_Domestic Booking-@Resources.Locale.L_TKBLQuery_DNDet', delKey: ["UId", "PartyType"],
        savelayout: true,
        showcolumns: true, exportexcel: true, url: rootPath + "DNManage/DNDetailQuery", postData: { "conditions": _uid },
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


    MenuBarFuncArr.Enabled(["MBEdoc"]);

    var colModel3 = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 70, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: "@Resources.Locale.L_DTBookingSetup_Script_132" } },
        { name: '', title: '@Resources.Locale.L_DTBookingSetup_Date', index: '', width: 100, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'TruckNo', title: '@Resources.Locale.L_GateReserve_TruckNo', index: 'TruckNo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'TruckNm', title: '@Resources.Locale.L_GateReserve_Trucker', index: 'TruckNm', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Driver', title: '@Resources.Locale.L_GateReserve_Driver', index: 'Driver', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Tel', title: '@Resources.Locale.L_BSCSSetup_CmpTel', index: 'Tel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: '', title: '@Resources.Locale.L_DTBookingSetup_Qty', index: '', width: 120, align: 'right', formatter: 'integer', hidden: false }
    ];

    _handler.intiGrid("SubGrid3", $SubGrid3, {
        colModel: colModel3, caption: '@Resources.Locale.L_DNManage_AdnormalInfo', delKey: ["UId", "PartyType"],
        
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

    MenuBarFuncArr.AddMenu("MBAllocation", "glyphicon glyphicon-bell", "Allocation", function () {
        var uid = $("#UId").val();
        if (!uid) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var status = $("#Status").val();
        if ("A" != status) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasBeenBooking", 500, "warning");
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "BookingAction/DTAllocation",
            type: 'POST',
            data: { UId: uid },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                if (data.IsOk == "Y") {
                    CommonFunc.Notify("", data.message, 500, "success");
                    _handler.topData = { UId: _uid };
                    MenuBarFuncArr.MBCancel();
                }
                else {
                    CommonFunc.Notify("", data.message, 500, "warning");
                }
            }
        });
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

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.Enabled(["MBCopy"]);
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
    AddMBPrintFunc();
    PpMonitor();
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
            var _shownull = { cd: '', cdDescp: '' };
            trnOptions.unshift(_shownull);
            appendSelectOption($("#CarType"), trnOptions);
            appendSelectOption($("#CarType1"), trnOptions);
            appendSelectOption($("#CarType2"), trnOptions);

            if (_handler.topData) {
                $("#TranType").val(_handler.topData["TranType"]);
                $("#CargoType").val(_handler.topData["CargoType"]);
                $("#TrackWay").val(_handler.topData["TrackWay"]);
                $("#CarType").val(_handler.topData["CarType"]);
                $("#CarType1").val(_handler.topData["CarType"]);
                $("#CarType2").val(_handler.topData["CarType"]);
            }
        }
    });
}
