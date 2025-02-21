var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid, $SubGrid2;
$(function () {
    mtSchemas = JSON.parse(decodeHtml(mtSchemas));
    CommonFunc.initField(mtSchemas);
    setdisabled(true);

    $SubGrid = $("#SubGrid");
    $SubGrid2 = $("#SubGrid2");
    
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

    function getBuyer(name) {
        var _name = name;
        var Buyer_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, undefined, $SubGrid,
            function ($grid, rd, elem, rowid) {
                //setGridVal($grid, rowid, _name, rd.PARTY_NO, "lookup");
                $(elem).val(rd.PARTY_NO);                
            }), { param: "" });
        return Buyer_op;
    }

    function getSupplier(name) {
        var _name = name;
        var Supplier_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'SupplierCd', map.PartyNo, null);
                    setGridVal($grid, selRowId, 'SupplierNm', map.PartyName, null);
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, undefined, $SubGrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, 'SupplierCd', rd.PARTY_NO, null);
                setGridVal($grid, selRowId, 'SupplierNm', rd.PARTY_NAME, null);
                //$(elem).val(rd.PARTY_NO);
            }),{ param: "" });
        return Supplier_op;
    }

    var BookingLookup = {
        caption: "PoManagement",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'PoNo', title: 'PO NO', index: 'PoNo', width: 120, init:true , align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'BpartNo', title: 'Buyer Part', index: 'BpartNo', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'SupplierNm', title: 'Supplier Name', index: 'SupplierNm', width: 250, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    };

    _handler.saveUrl = rootPath + "TKBL/SmpomUpdateData";
    _handler.inquiryUrl = rootPath + "TKBL/SmpomQueryData";
    _handler.config = BookingLookup;

    _handler.addData = function () {
        //初始化新增数据
        var data = {
            "CreateBy": userId, "CreateDate": getDate(0, "-"), "Cmp": cmp, "Etd": getDate(0, "-"), "DeliveryWay": "A", "FreightTerm": "PP",
            "ModifyBy": userId, "ModifyDate": getDate(0, "-"), "CreadyDate": getDate(0, "-"), "FshippingDate": getDate(30, "-")
        };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        _handler.loadGridData("SubGrid2", $SubGrid2[0], [], [""]);
        getAutoNo("PoNo", "rulecode=SMRV_NO&cmp=" + cmp);
    }

    _handler.saveData = function (dtd) {
        var Qty = $("#SubGrid").jqGrid("getCol", "Qty", false, "sum");
        var Oqty = $("#SubGrid").jqGrid("getCol", "Oqty", false, "sum");
        var Dqty = $("#SubGrid").jqGrid("getCol", "Dqty", false, "sum");
        var Bqty = $("#SubGrid").jqGrid("getCol", "Bqty", false, "sum");
        $("#Qty").val(Qty);
        $("#Oqty").val(Oqty);
        $("#Dqty").val(Dqty);
        $("#Bqty").val(Bqty);
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var containerArray2 = $SubGrid2.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub2"] = containerArray2;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
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
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        //MenuBarFuncArr.initEdoc($("#UId").val());
        MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["MBInvalid"]);
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        //MenuBarFuncArr.Enabled(["SEND_BTN"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "TKBL/GetSMPOMDataItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
              
                var status = data.main[0].Status;
                if (status == "I") {
                    MenuBarFuncArr.Disabled(["MBEdit", "MBDel", "MBSave", "MBEdoc", "MBInvalid"]);
                } else {
                    MenuBarFuncArr.Enabled(["MBEdit", "MBDel", "MBEdoc", "MBInvalid"]);
                }
            });
        
    }

    _handler.beforSave = function () {
        return true;
    }

    


    var colModel1 = [
        { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        //{ name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', editoptions: gridLookup(getBuyer("")), edittype: 'custom', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
        //{ name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', editoptions: gridLookup(getSupplier("")), edittype: 'custom', width: 80, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', width: 120, align: 'left', sorttype: 'string', hidden: true },
        { name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', width: 120, align: 'left', sorttype: 'string', hidden: true },
        { name: 'SupplierNm', title: 'Supplier Name', index: 'SupplierNm', width: 180, align: 'left', sorttype: 'string', hidden: true, editable: true },
        { name: 'PoNo', title: 'PO NO', index: 'PoNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
        { name: 'SeqNo', title: 'SEQ NO', index: 'SeqNo', width: 80, align: 'left', sorttype: 'string', hidden: true },
        { name: 'BpartNo', title: 'Buyer Part', index: 'BpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'SpartNo', title: 'Supplier Part', index: 'SpartNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Description', title: 'Description', index: 'Description', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Brand', title: 'Brand', index: 'Brand', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Qty', title: '@Resources.Locale.L_BaseLookup_Qty', index: 'Qty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false ,formatter: 'integer' },
        { name: 'Qtyu', title: 'Unit', index: 'Qtyu', editoptions: gridLookup(getUnit("Unit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Bkqty', title: 'Booking Qty', index: 'Bkqty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: true ,formatter: 'integer'},
        { name: 'Oqty', title: 'On the way Qty', index: 'Oqty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false ,formatter: 'integer'},
        { name: 'Dqty', title: 'Delivery QTY', index: 'Dqty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false ,formatter: 'integer'},
        { name: 'Bqty', title: 'Balance QTY', index: 'Bqty', width: 100, align: 'right', sorttype: 'int', editable: true, hidden: false ,formatter: 'integer'},
        { name: 'RefNo1', title: 'REF. NO1', index: 'RefNo1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'RefNo2', title: 'REF. NO2', index: 'RefNo2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'RefNo3', title: 'REF. NO3', index: 'RefNo3', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: true },
        { name: 'Remark', title: 'Remark', index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true }
    ];

    

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: colModel1, caption: '@Resources.Locale.L_DNManage_PODetail', delKey: ["UId"], height: 100,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
             maxSeqNo = 0;
            $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            $SubGrid.jqGrid('setCell', rowid, "PoNo", $("#PoNo").val());
            //$SubGrid.jqGrid('setCell', rowid, "BuyerCd", $("#BuyerCd").val());
            //$SubGrid.jqGrid('setCell', rowid, "SupplierCd", $("#SupplierCd").val());
            //$SubGrid.jqGrid('setCell', rowid, "SupplierNm", $("#SupplierNm").val());
            setGridVal($SubGrid, rowid, "SeqNo", maxSeqNo + 1);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('BuyerCd', { editable: true });
            } else {
                $SubGrid.setColProp('BuyerCd', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            $SubGrid.setColProp('PoNo', { editable: true });
        }
    });

    var colModel2 = [
        { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        { name: 'HouseNo', title: 'BL NO', index: 'HouseNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MasterNo', title: 'MBL NO', index: 'MasterNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CarrierCd', title: 'Carrier', index: 'CarrierCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ScacCd', title: 'SCAC', index: 'ScacCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Vessel1', title: 'Vessel', index: 'Vessel1', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Voyage1', title: 'Voyage', index: 'Voyage1', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PorCd', title: 'POR', index: 'PorCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PolCd', title: 'POL', index: 'PolCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PodCd', title: 'POD', index: 'PodCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DestCd', title: 'DEST', index: 'DestCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Etd', title: 'ETD', index: 'Etd', width: 100, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'Eta', title: 'ETA', index: 'Eta', width: 100, align: 'left', sorttype: 'date', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
    ];

    _handler.intiGrid("SubGrid2", $SubGrid2, {
        colModel: colModel2, caption: 'BL INFO', delKey: ["UId"], height: 50,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid2.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $SubGrid2.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($SubGrid2, rowid, "SeqNo", maxSeqNo + 1);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                //$SubGrid2.setColProp('BuyerCd', { editable: true });
            } else {
                //$SubGrid2.setColProp('BuyerCd', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$SubGrid2.setColProp('PoNo', { editable: true });
        }
    });
    

    
    //Buyer

    registBtnLookup($("#BuyerCdLookup"), {
        item: '#BuyerCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#BuyerCd").val(map.PartyNo);
            $("#BuyerNm").val(map.PartyName);
        }
    }, { focusItem: $("#BuyerCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#BuyerNm").val(rd.PARTY_NAME);
    }));

    //Supplier

    registBtnLookup($("#SupplierCdLookup"), {
        item: '#SupplierCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#SupplierCd").val(map.PartyNo);
            $("#SupplierNm").val(map.PartyName);
        }
    }, { focusItem: $("#SupplierCd") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#SupplierNm").val(rd.PARTY_NAME);
    }));

    //Incorterm
    registBtnLookup($("#IncotermLookup"), {
        item: '#Incoterm', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Incoterm").val(map.Cd);
            $("#IncotermDescp").val(map.CdDescp);
        }
    }, { focusItem: $("#Incoterm") }, LookUpConfig.GetCodeTypeAuto(groupId, "TINC", undefined, function ($grid, rd, elem) {
        //$("#IncotermDescp").val(rd.CD_DESCP);
        $("#Incoterm").val(rd.CD);
    }));

    //QTYU
    registBtnLookup($("#QtyuLookup"), {
        item: '#Qtyu', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#Qtyu").val(map.Cd);           
        }
    }, { focusItem: $("#Qtyu") }, LookUpConfig.GetCodeTypeAuto(groupId, undefined, function ($grid, rd, elem) {
        //$("#IncotermDescp").val(rd.CD_DESCP);
        $("#Qtyu").val(rd.CD);
    }));

    //Original
    registBtnLookup($("#FromLookup"), {
        item: '#FromCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#FromCd").val(map.CntryCd + map.PortCd);
            $("#FromNm").val(map.PortNm);
        }
    }, { focusItem: $("#FromCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#FromCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#FromNm").val(rd.PORT_NM);
    }));

    //Destination
    registBtnLookup($("#ToLookup"), {
        item: '#ToCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#ToCd").val(map.CntryCd + map.PortCd);
            $("#ToNm").val(map.PortNm);
        }
    }, { focusItem: $("#ToCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ToCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#ToNm").val(rd.PORT_NM);
    }));

    //Original LSP
    
    registBtnLookup($("#OriginLspLookup"), {
        item: '#OriginLsp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#OriginLsp").val(map.PartyNo);
            $("#OriginLnm").val(map.PartyName);
        }
    }, { focusItem: $("#OriginLsp") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#OriginLnm").val(rd.PARTY_NAME);
    }));

    //DEST.LSP

    registBtnLookup($("#DestLspLookup"), {
        item: '#DestLsp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#DestLsp").val(map.PartyNo);
            $("#DestLnm").val(map.PartyName);
        }
    }, { focusItem: $("#DestLsp") }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#DestLnm").val(rd.PARTY_NAME);
    }));





    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }

    _handler.beforLoadView = function () {
        var keys = ["Uid","PoNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }

        var requires = ["Uid", "PoNo"];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        var readonlys = ["PoNo","Qty","Oqty", "Dqty", "Bqty"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        }
    }

    
    _initUI(["MBApply", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    


    MenuBarFuncArr.MBInvalid = function (thisItem) {
        if (confirm("@Resources.Locale.L_POManagementSetup_Script_191")) {
            var uid = $("#UId").val();
            $.ajax({
                async: false,
                url: rootPath + "/TKBL/SetInvalid",
                type: 'POST',
                data: { "UId": uid, autoReturnData: false },
                dataType: "json",
                "complete": function (xmlHttpRequest, successMsg) {
                    if (successMsg != "success")
                        return null;
                    else
                        CommonFunc.Notify("", "@Resources.Locale.L_POManagementSetup_Scripts_389", 500, "success");
                },
                "error": function (xmlHttpRequest, errMsg) {
                },
                success: function (result) {
                    //initLoadData(uid);
                    window.location.reload();
                    
                }
            });
        }
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        $("#QuotDate").val(getDate(0, "-"));
        $("#EffectFrom").val(getDate(0, "-"));
        $("#EffectTo").val(getDate(365, "-"));
        getAutoNo("PoNo", "rulecode=SMRV_NO&cmp=" + cmp);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        gridEditableCtrl({ editable: true, gridId: "SubGrid" });
    }
    
});


