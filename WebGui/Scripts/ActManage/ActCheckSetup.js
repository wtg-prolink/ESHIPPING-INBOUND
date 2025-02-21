var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid;
var fssplang = getCookie("language");
$(function () {

    _handler.saveUrl = rootPath + "ActManage/SaveData";
    _handler.inquiryUrl = rootPath + "ActManage/GetActQueryData";
    _handler.config = {};

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        var col = $MainGrid.jqGrid('getCol', 'Status', false);//获取批文号码列的值
        var isexist = false;
        $.each(col, function (index, colname) {
            if (colname == "N") {
                $MainGrid.jqGrid('setRowData', index + 1, false, 'gridTagClass');
            }
        });
        var bCol = $MainGrid.jqGrid('getCol', 'Bamt', false);
        var qCol = $MainGrid.jqGrid('getCol', 'Qamt', false);
        $.each(bCol, function (index, colname) {
            if (parseFloat(colname) > parseFloat(qCol[index])) {
                $MainGrid.jqGrid('setRowData', index + 1, false, 'myRed');
            } else if (parseFloat(colname) < parseFloat(qCol[index])) {
                $MainGrid.jqGrid('setRowData', index + 1, false, 'myOrange');
            }

        });

        var tvMntTypes = tvMntSelect.split(';');
        $.each(tvMntTypes, function (idx, value) {
            appendSelect(value, value);
        });

        setFieldValue(data["main"] || [{}]);
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["SEND_BTN"]);
        MenuBarFuncArr.Enabled(["MBPreview", "btn09", "btn02","UPLOAD_BTN"]);
        $("#CheckDescp").prop("disabled", false);
        $("#Remark").prop("disabled", false);
        var status = $("#Status").val();
        var Receiver = $("#Receiver").val();

        /*var Bamt = $MainGrid.jqGrid("getCol", "Bamt", false, "sum");
        var Qamt = $MainGrid.jqGrid("getCol", "Qamt", false, "sum");

        $("#Amt").val(CommonFunc.formatFloat(Bamt, 2));
        $("#Qamt").val(CommonFunc.formatFloat(Qamt, 2));*/

        /*问题单：账单录入，账单比对画面物流业者下方增加收款对象，默认与物流业者一样，可以允许修改*/
        if(Receiver == "")
        {
            if(typeof data["main"][0] !== "undefined")
            {
                $("#Receiver").val(data["main"][0]["LspNo"]);
                $("#ReceiverNm").val(data["main"][0]["LspNm"]);
            }
        }

        if(typeof data["main"][0] !== "undefined")
        {
            if (data["main"][0].ApproveTo != "A" && data["main"][0].ApproveTo != "LSTM")
            {
                MenuBarFuncArr.Disabled(["btn02", "btn09"]);
            }
        }
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "ActManage/GetActSetupDataItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
        function (data) {
            if (_handler.setFormData)
                _handler.setFormData(data);
        });
    }


    $MainGrid = $("#MainGrid");
    

    // var colModel = [
    // { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
    // { name: 'ShipmentId', title: _getLang('L_DNApproveManage_ShipmentId', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false },
    // { name: 'DebitNo', title: _getLang('L_ActQuery_DebitNo', index: 'DebitNo', width: 120, align: 'left', sorttype: 'string', hidden: false },
    // { name: 'LspNo', title: _getLang('L_DRule_LspNo', index: 'LspNo', width: 100, align: 'left', sorttype: 'string', hidden: false },
    // { name: 'BillTo', title: '付款者', index: 'BillTo', width: 100, align: 'left', sorttype: 'string', hidden: false },
    // { name: 'Status', title: _getLang('L_RQQuery_Status', index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Y:符合;N:不符;'} },
    // { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 80, hidden: false, editable: true },
    // { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 120, hidden: false, editable: false },
    // { name: 'Qcur', title: '報價幣別', index: 'Qcur', sorttype: 'string', width: 80, hidden: false, editable: true },
    // {
    //     name: 'QunitPrice', title: '報價單價', index: 'QunitPrice',
    //     width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
    //     hidden: false, editable: true
    // },
    // { name: 'QchgUnit', title: '報價計價單位', index: 'QchgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
    // { name: 'Qqty', title: '報價數量', index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
    // { name: 'Qamt', title: '報價金額', index: 'Qamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: true },
    // { name: 'Qtax', title: '報價稅率', index: 'Qtax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
    // { name: 'Cur', title: _getLang('L_IpPart_Crncy', index: 'Cur', sorttype: 'string', width: 80, hidden: false, editable: true },
    // {
    //     name: 'UnitPrice', title: _getLang('L_DNApproveManage_UnitPrice', index: 'UnitPrice',
    //     width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
    //    hidden: false, editable: true
    // },
    // { name: 'ChgUnit', title: _getLang('L_ActSetup_ChgUnit', index: 'ChgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
    // { name: 'Qty', title: _getLang('L_BaseLookup_Qty', index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
    // { name: 'Amt', title: _getLang('L_ActSetup_Amt', index: 'Amt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: true },
    // { name: 'Tax', title: _getLang('L_ActSetup_Tax', index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
    // { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
    // { name: 'CheckDescp', title: _getLang('L_ActSetup_CheckDescp', index: 'CheckDescp', sorttype: 'string', width: 200, hidden: false, editable: true },
    // { name: 'CostCenter', title: '成本中心', index: 'CostCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
    // { name: 'ProfitCenter', title: '利潤中心', index: 'ProfitCenter', sorttype: 'string', width: 200, hidden: false, editable: true }

    // ];
    
    var colModel = [
    { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'ShipmentId', title: _getLang("L_DNApproveManage_ShipmentId", "Shipment ID"), index: 'ShipmentId', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'DebitNo', title: _getLang('L_ActQuery_DebitNo', 'LSP Debit NO'), index: 'DebitNo', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Status', title: _getLang('L_ActManage_MatchorNot', 'if Match') , index: 'Status', width: 50, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Y:Yes;N:No' } },
        { name: 'BlNo', title: _getLang('L_ActSetup_BlNo', 'B/L#'), index: 'BlNo', sorttype: 'string', width: 120, hidden: true, editable: true },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', 'Fee Code'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', 'Description') , index: 'ChgDescp', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), index: 'Cur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
    {
        name: 'UnitPrice', title: _getLang('L_DNApproveManage_UnitPrice', 'Unit Price'), index: 'UnitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
        hidden: false, editable: true
    },
        { name: 'ChgUnit', title: _getLang('L_ActSetup_ChgUnit', 'Chargeable unit'), index: 'ChgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', 'QTY'), index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Tax', title: _getLang('L_ActSetup_Tax', 'Tax rate/%'), index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 80, hidden: false, editable: true },
    //{ name: 'QunitPrice', title: '预提单价', index: 'QunitPrice', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
        { name: 'Bamt', title: _getLang('L_ActSetup_Bamt', 'Payment amount'), index: 'Bamt', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 100, hidden: false, editable: true },

        { name: 'ExRate', title: _getLang('L_QTManage_ExRate', 'Exchange rate of application'), index: 'ExRate', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, width: 100, hidden: false, editable: true },

        { name: 'Lamt', title: _getLang('L_ActManage_LocIvAmt', 'Local payment amount'), index: 'Lamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 100, hidden: false, editable: false },
        { name: 'Qcur', title: _getLang('L_BaseLookup_WithholdCur','Estimated Currency'), index: 'Qcur', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Qamt', title: _getLang('L_ActSetup_Qamt', 'Estimated amount'), index: 'Qamt', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 100, editable: true },
        { name: 'QexRate', title: _getLang('L_ActSetup_Scripts_63', 'Estimated exchange rate'), index: 'QexRate', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, width: 100, hidden: false, editable: false },
        { name: 'Qlamt', title: _getLang('L_ActManage_LocWithAmt', 'Local Estimated amount'), index: 'Qlamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 100, hidden: false, editable: false },
        { name: 'BiRemark', title: _getLang('L_ActDeatilManage_BiRemark', 'LSP Reference'), index: 'BiRemark', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'CheckDescp', title: _getLang('L_ActSetup_CheckDescp', 'Rejected reason/Remark by TPV LST:'), index: 'CheckDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'Remark', title: _getLang('L_IpPart_WithholdingRemark', 'Remark for Accrued Expenses'), index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
    {
        name: 'QunitPrice', title: _getLang('L_BaseLookup_WithholdPrice', 'Estimated Price'), index: 'QunitPrice',
        width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
        hidden: true, editable: true
    },
        { name: 'QchgUnit', title: _getLang('L_ActCheckSetup_Scripts_26', 'Chargable unit per Quotation'), index: 'QchgUnit', sorttype: 'string', width: 80, hidden: true, editable: true },
        { name: 'Qqty', title: _getLang('L_BaseLookup_WithholdQty', 'Estimated Quantity'), index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: true, editable: true },
   
    //{ name: 'Tax', title: _getLang('L_ActSetup_Tax', index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
    //{ name: 'VatNo', title: _getLang('L_ActSetup_VatNo', index: 'VatNo', sorttype: 'string', width: 250, hidden: false, editable: false },
        { name: 'Qtax', title: _getLang('L_ActCheckSetup_Scripts_27', 'Tax rate per Quotation'), index: 'Qtax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, width: 80, hidden: true, editable: true },

        { name: 'CostCenter', title: _getLang('L_SMCHGSetup_ChgLevel', 'Cost Center'), index: 'CostCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'ProfitCenter', title: _getLang('L_ActManage_ProCenter', 'Profit center'), index: 'ProfitCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
    { name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 85, hidden: false, editable: false },
    { name: 'DebitNm', title: 'Debit To Name', index: 'DebitNm', sorttype: 'string', width: 180, hidden: false, editable: false },
    { name: 'InvoiceInfo', title: 'Invoice Info.', index: 'InvoiceInfo', sorttype: 'string', width: 200, hidden: false, editable: false }
    ];


    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: _getLang('L_Layout_FeeDetail', 'Cost Details'),
        showcolumns: true,
        savelayout: true,
        delKey: ["UId"], height: 250,
        afterSaveCellWithIdFunc: function(rowid, name, val, iRow, iCol, toolId)
        {
            var Bamt = $MainGrid.jqGrid("getCol", "Bamt", false, "sum");
            var Qamt = $MainGrid.jqGrid("getCol", "Qamt", false, "sum");
            $("#Amt").val(CommonFunc.formatFloat(Bamt, 2));
            $("#Qamt").val(CommonFunc.formatFloat(Qamt, 2));
        },
        onAddRowFunc: function (rowid) {
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    },["UId"], false); 


    registBtnLookup($("#LspNoLookup"), {
        item: "#LspNo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, {}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNo").val(rd.PARTY_NO);
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#BillToLookup"), {
        item: "#BillTo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#BillTo").val(map.PartyNo);
            $("#BillNm").val(map.PartyName);
        }
    }, {}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#BillTo").val(rd.PARTY_NO);
        $("#BillNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#ReceiverLookup"), {
        item: "#BillTo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Receiver").val(map.PartyNo);
            $("#ReceiverNm").val(map.PartyName);
        }
    }, {}, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Receiver").val(rd.PARTY_NO);
        $("#ReceiverNm").val(rd.PARTY_NAME);
    }));


    registBtnLookup($("#PolLookup"), {
        item: "#Pol", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pol").val(map.PortCd + map.CntryCd);
            $("#PolNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pol").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodLookup"), {
        item: "#Pod", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pod").val(map.PortCd + map.CntryCd);
            $("#PodNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pod").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    registBtnLookupNew($("#PayTermLookup"), {
        item: '#PayTerm',
        url: rootPath + "TPVCommon/GetBscodeDataForLookup",
        config: LookUpConfig.BSCodeLookup,
        autoCompDt: ConditionParam("", "Cd", "", "bw"),
        selectRowFn: function (map) {
            $("#PayTerm").val(map.Cd);
            $("#PayTermNm").val(map.CdDescp);
        },
        autoClearFunc: function () {
            $("#PayTerm").val("");
            $("#PayTermNm").val("");
        },
        baseConditionFunc: function () {
            var cmp = $("#Cmp").val();
            var condition = "";
            if (cmp != '') {
                condition = ConditionParam("", "Cmp", cmp + ";*", "in");
            }
            return ConditionParam(condition, "CdType", "BPT", "eq");
        }
    });

    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }

    _handler.beforLoadView = function () {
        var keys = ["ShipmentId", "DebitNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }
        var requires = ["PayStartDate", "PayTerm"];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        // var requires = ["ShipmentId", "DebitNo", "DebitDate", "LspNo", "Pol", "BillTo", "Pod", "Cur"];
        // for (var i = 0; i < requires.length; i++) {
        //     $("#" + requires[i]).attr('required', true);
        //     $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        // }
        // var readonlys = [];
        // for (var i = 0; i < readonlys.length; i++) {
        //     $("#" + readonlys[i]).attr('readonly', true);
        // }
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["DebitNo"] = encodeURIComponent($("#DebitNo").val());
        //data["recal_amt"] = "Y";
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

    MenuBarFuncArr.AddMenu("btn02", "glyphicon glyphicon-ok", _getLang("L_ActManage_Pass","Passed"), function () {
        if (_approve === "Y") {
            Approve_click();
            return;
        }
        var Status = $("#Status").val();
        if(Status == "D")
        {
            CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_28", "Approved Already"), 500, "warning");
            return false;
        }
        if(_uid)
        {
            $.ajax({
                async: true,
                url: rootPath + "ActManage/ActPass",
                type: 'POST',
                data: { UId: _uid },
                dataType: "json",
                beforeSend: function()
                {
                    CommonFunc.ToogleLoading(true);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_29", "System Error!"), 500, "warning");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    /*if (result.message != "success") 
                    {
                        CommonFunc.Notify("", result.message, 500, "warning");
                        CommonFunc.ToogleLoading(false);
                        return false;
                    }*/

                    var formData = $.parseJSON(result.returnData.Content);
                    _handler.setFormData(formData);
                    CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_30", "Closed"), 500, "success");
                    CommonFunc.ToogleLoading(false);
                }
            });
        }
    });
    MenuBarFuncArr.AddMenu("btn09", "glyphicon glyphicon-remove", _getLang("L_ActManage_Refuse", "Reject"), function () {
        if (_approve === "Y") {
            BackApprove_click();
            return;
        }

        var CheckDescp = $("#CheckDescp").val();
        var Remark = $("#Remark").val();
        var Status = $("#Status").val();
        if(Status == "C")
        {
            CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_31", "Rejected Already"), 500, "warning");
            return false;
        }
        if(CheckDescp == "" || CheckDescp == null)
        {
            CommonFunc.Notify("", _getLang("L_ActCheckSetup_Script_6", "Rejected reasons was required!"), 1300, "warning");
            $("#CheckDescp").setfocus();
            return false;
        }
        else
        {
            if(_uid)
            {
                $.ajax({
                    async: true,
                    url: rootPath + "ActManage/ActReject",
                    type: 'POST',
                    data: {UId: _uid, CheckDescp: CheckDescp, Remark: Remark },
                    dataType: "json",
                    beforeSend: function()
                    {
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_29", "System Error!"), 500, "warning");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        if (result.message != "success") 
                        {
                            CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_34", "Application submitted!"), 500, "success");
                            $("#Status").val("D");
                            CommonFunc.ToogleLoading(false);
                            return false;
                        }


                        var formData = $.parseJSON(result.returnData.Content);
                        _handler.setFormData(formData);
                        CommonFunc.Notify("", _getLang("L_ActCheckSetup_Scripts_30", "Closed"), 500, "success");
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }
        
    });

    MenuBarFuncArr.AddMenu("btnLSTINVDescp", "glyphicon glyphicon-bell", "LST INV. Description", function () {
        $("#InvDescpWin").modal("show");
    });

    MenuBarFuncArr.AddMenu("UPLOAD_BTN", "glyphicon glyphicon-bell", _getLang("TLB_Upload", '上传'), function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        if ($("#FSSPiframeDetail").length <= 0) {
            $("body").append(_showFSSPiframeModal);
        }
        var TpvDebitNo = $("#TpvDebitNo").val();
        var Stn = $("#Stn").val();
        InitFSSPiframe(TpvDebitNo, Stn);

        $('#FSSPiframeDetail').modal('show'); //顯示彈出視窗
        ajustamodal("#FSSPiframeDetail");
    });

    $("#btnInvDescp").on("click", function () {
        var uids = $("#UId").val();
        var LstInvDescp = $("#LstInvDescpbk").val();
        var data = { "Uids": uids, "LstInvDescp": LstInvDescp };

        $.ajax({
            async: true,
            url: rootPath + "ActManage/UpdateLstInvDescpInfo",
            type: 'POST',
            data: data,
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
                if (result.message == "success") {
                    CommonFunc.Notify("", result.message, 500, "success");
                    $("#LstInvDescp").val(LstInvDescp);
                }
                else {
                    alert(result.message);
                }
                $("#InvDescpWin").modal("hide");
            }
        });

    });


    _initUI(["MBAdd","MBDel","MBSearch","MBCopy", "MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    MenuBarFuncArr.Enabled(["MBEdoc"]);
    getSelectOptions();

    AddMBPrintFunc();

    $( "#SmIdSearch" ).keypress(function( event ) {
        $("#ChgCdSearch").val("");
      if ( event.which == 13 ) {
         var obj = SearchObj(_dm.getDs("MainGrid")._data, "ShipmentId", $(this).val());
         $.each(obj, function(i, v){
            $.each(obj["show"], function(k, val){
               $MainGrid.find("tr#"+obj["show"][k]).show();
            });

            $.each(obj["hide"], function(k, val){
               $MainGrid.find("tr#"+obj["hide"][k]).hide();
            });
         });
      }
    });

    $("#ChgCdSearch").keypress(function (event) {
        $("#SmIdSearch").val("");
      if ( event.which == 13 ) {
          var obj = SearchObj(_dm.getDs("MainGrid")._data, "ChgCd", $(this).val());
         $.each(obj, function(i, v){
            $.each(obj["show"], function(k, val){
               $MainGrid.find("tr#"+obj["show"][k]).show();
            });

            $.each(obj["hide"], function(k, val){
               $MainGrid.find("tr#"+obj["hide"][k]).hide();
            });
         });
      }
    });

    $('#MBEdit').unbind("click");
    $("#MBEdit").click(function () {
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }

        _editData = _dataSource[0];

        var result = MenuBarFuncArr.MBEdit();
        $("#RemarkS").prop("disabled", false);
        $("#ContractNo").prop("disabled", false);
        if (typeof (_editData) == "undefined") {
            _editData = { __state: '2' };
        } else {
            _editData["__state"] = 2;
        }
        if (!result && typeof result != "undefined") {
            StatusBarArr.nowStatus(_getLang('L_SYS_FAIL', '失败'));
        } else {
            StatusBarArr.nowStatus(_getLang('L_SYS_EDIT', '编辑'));
            MenuBarFuncArr.DisableAllItem();
            MenuBarFuncArr.Enabled(["MBCancel", "MBSave"]);
        }
        var approveTo = $("#ApproveTo").val();
        if (approveTo == "A") {
            var readonlys = ["PayStartDate", "PayTerm"];
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).prop("disabled", false);
                $("#" + readonlys[i]).attr('readonly', false);
                $("#" + readonlys[i]).parent().find("button").prop("disabled", false);
            }
        }
        MenuBarFuncArr.EndFunc();
        $("#wrapper").focusFirst();
    });

    $(".searchOpenClose").on("click", function () {
        var docHeight = $(window).height();
        if ($("#PaymentBody").is(":visible")) {
            $MainGrid.jqGrid("setGridHeight", docHeight - 180);
            $("#PaymentBody").parent().hide();
            $(".searchOpenClose span").attr("class", "glyphicon glyphicon-chevron-down");
        } else {
            $("#PaymentBody").parent().show();
            $MainGrid.jqGrid("setGridHeight", 250);
            $(".searchOpenClose span").attr("class", "glyphicon glyphicon-chevron-up");
        }

    });
});

function appendSelect(optioncd, optionDescp) {
    $("#TvMnt").append("<option value=\"" + optioncd + "\">" + optionDescp + "</option>");
}

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("ActSetup") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TNT || [];
            var pkOptions = data.PK || [];
            var ddtOptions = data.DTT || [];
            var rmkOptions = data.RMK || [];

            appendSelectOption($("#TranMode"), trnOptions);
            appendSelectOption($("#ServiceMode"), pkOptions);

            var tdltOptions = data.TDLT || [];
            appendSelectOption($("#BillTo"), tdltOptions);

            if (_handler.topData) {
                $("#BillTo").val(_handler.topData["BillTo"]);
            }

            appendSelectOption($("#DebitType"), ddtOptions);
            if (_handler.topData) {
                if (_handler.topData["DebitType"] == null || _handler.topData["DebitType"] == "" || _handler.topData["DebitType"] == undefined) {
                    //$("#DebitType").val(_handler.topData["DebitType"]);
                } else {
                    $("#DebitType").val(_handler.topData["DebitType"]);
                }

            }

            appendSelectOption($("#ChgType"), rmkOptions);
            if (_handler.topData) {
                $("#ChgType").val(_handler.topData["ChgType"]);
            }
        }
    });
}

function getchg(name) {
    var _name = name;
    var chg_op = getLookupOp("MainGrid",
        {
            url: rootPath + LookUpConfig.ChgUrl,
            config: LookUpConfig.ChgLookup,
            returnFn: function (map, $grid) {
                var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                setGridVal($grid, selRowId, "ChgDescp", map.ChgDescp, null);
                setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                //setGridVal($grid, selRowId, "ChgType", map.ChgType, "lookup");
                return map.ChgCd;
            }
        }, LookUpConfig.GetChgAuto1(groupId, undefined, $MainGrid,
        function ($grid, rd, elem, rowid) {
            setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
            setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
            //setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, "lookup");
            $(elem).val(rd.CHG_CD);
        }), {
            param: "",
            baseConditionFunc: function () {
                return "";
            }
        });
    return chg_op;
}

function getcur(name) {
    var _name = name;
    var cur_op = getLookupOp("MainGrid",
        {
            url: rootPath + LookUpConfig.CurUrl,
            config: LookUpConfig.CurLookup,
            returnFn: function (map, $grid) {
                return map.Cur;
            }
        }, LookUpConfig.GetCurAuto(groupId,undefined, $MainGrid,
        function ($grid, rd, elem, rowid) {
            setGridVal($grid, rowid, _name, rd.CUR, "lookup");
            $(elem).val(rd.CUR);
        }));
    return cur_op;
}





function AddMBPrintFunc() {
    var listBar = [];

    $("#dialog_saveBtn").click(function () {
        doDownloadExcel();
    });
    $("#dialog_saveBtnN").click(function () {
        doDownloadExcelN();
    });
    //listBar.push({
    //    menuId: "RFB02", menuName: _getLang("L_ActCheckSetup_Script_8", menuFunc: function () {
    //        var DebitNo = $("#DebitNo").val();
    //        //var shipments = $("#ShipmentId").val();
    //        if (!DebitNo) {
    //            alert(_getLang("L_ActCheckSetup_Script_7");
    //            return;
    //        }
    //        $("#modifyDialog").modal("show");
    //    }, menuCss: "glyphicon glyphicon-list-alt"
    //});
    listBar.push({
        menuId: "RFB04", menuName: _getLang("L_ActCheckSetup_Script_8", "Statement Of Account/Excel") + " EN", menuFunc: function () {
            var DebitNo = $("#DebitNo").val();
            //var shipments = $("#ShipmentId").val();
            if (!DebitNo) {
                alert(_getLang("L_ActCheckSetup_Script_7","No application form for the selected bill!"));
                return;
            }
            $("#modifyDialogN").modal("show");
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    listBar.push({
        menuId: "RFB03", menuName: _getLang("L_ActManage_requestbillEn", "Application Form-EN"), menuFunc: function () {
            var DebitNo = $("#TpvDebitNo").val();
            //var shipments = $("#ShipmentId").val();
            if (!DebitNo) {
                alert(_getLang("L_ActCheckSetup_Script_7", "No application form for the selected bill!"));
                return;
            }
            var params = {
                //currentCondition: "",
                //val: dnNo,
                debitno: DebitNo,
                rptdescp: _getLang("L_ActManage_requestbill", "Application Form"),
                rptName: "RFB03",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });
    MenuBarFuncArr.AddDDLMenu("MBPreview", " glyphicon glyphicon-print", _getLang("L_ActManage_Preview", "Report preview"), function () { }, null, listBar);
}

function showReport(params) {
    $.ajax({
        async: true,
        url: rootPath + "Report/CreateNewReport",
        type: 'POST',
        data: params,
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
            var xx = xmlHttpRequest.responseText;
            window.open(xmlHttpRequest.responseText);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {

        }
    });
}


function doDownloadExcel() {
    var uid = $("#UId").val(), smid = $("#ShipmentId").val();
    var transType = $("#sel_tranType").val();
    var chgTypeStr = "", chgTypeColsStr = "";

    var rowIds = $MainGrid.getDataIDs();
    if (rowIds.length <= 0) {
        alert(_getLang("L_ActCheckSetup_Script_9", "No application form for the selected bill!"));
        return;
    }
    if (transType === "TTTTTT") {
        chgTypeStr = _getLang("L_ActCheckSetup_Script_10", "Cost items; Description of cost items; Amount");
        chgTypeColsStr = "ChgCd;ChgDescp;Bamt";
    }
    else {
        var chgCheck = {};
        chgTypeStr += "total";
        chgTypeColsStr += "ExtTotal";
        var rowIds = $MainGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            var checkKey = (rowDatas.ChgCd || '');
            if (checkKey === "" || checkKey === null)
                continue;
            if (chgCheck[checkKey])
                continue;
            chgCheck[checkKey] = true;

            if (chgTypeStr.length > 0) chgTypeStr += ";";
            if (chgTypeColsStr.length > 0) chgTypeColsStr += ";";
            var temp = rowDatas.ChgDescp || '';
            temp = temp.replace("\n", "");

            chgTypeStr += checkKey + "-" + temp;
            temp = rowDatas.ChgCd || '';
            if (temp.length > 1)
                temp = temp.substr(0, 1).toUpperCase() + temp.substr(1, temp.length - 1).toLowerCase();
            chgTypeColsStr += temp;
        }
    }

    var url = rootPath + "ActManage/ExportBBillExcel";
    var transTypeCols = chgTypeStr.split(";");
    var transTypeColsN = chgTypeColsStr.split(";");
    var colNames = [];

    var colModel = _getColModel(transType);
    $.each(colModel, function (index, val) {
        colNames.push(val["title"].split("||")[0]);
    });


    $.each(transTypeCols, function (index, val) {
        switch (transTypeColsN[index]) {
            case "ChgCd":
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 40, align: 'left', hidden: false });
                break;
            case "ChgDescp":
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 120, align: 'left', hidden: false });
                break;
            case "ExtTotal":
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 120, align: 'left', hidden: false });
                break;
            default:
                colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'string', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false });
                break;
        }
        colNames.push(val);
    });

    console.log(colModel);
    console.log(colNames);

    var caption = "Statement Of Account";
    var excelName = transType + "-Statement Of Account";


    var conditions = uid;
    var baseCondition = smid;
    var virtualCol = transType;
    ExportDataToExcelByParam(url, colModel, colNames, caption, excelName, conditions, baseCondition, virtualCol);
}


function Approve_click() {
    var debitno = [];
    var uid = [];
    var id = $("#UId").val();
    debitno.push( $("#DebitNo").val());
    uid.push(id);

    if (!id) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "Please Select a Record"), 500, "warning");
        return;
    }
    var iscontinue = window.confirm(_getLang("L_ActManage_is", "If check?") + debitno.toString() + "】" + _getLang("L_ActCheckSetup_Script_11","Pass or not?"));
    if (!iscontinue) {
        return;
    }
    $('#TVMNTSelectLabel').css('display', 'none');
    var approveto = $("#ApproveTo").val()
    if ("A" == approveto) {
        $('#TVMNTSelectLabel').css('display', 'block');
    }
    $('#Approve').modal('show');
}

function BackApprove_click() {
    $("#BackRemark").val("");
    var uid = $("#UId").val();
    if (!uid) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "Please Select a Record"), 500, "warning");
        return;
    }
    $("#ApproveBack").modal("show");
}

function BackApprove() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", _getLang("L_ActManage_EnterReason", "Please enter the reason for return"), 500, "warning");
        return;
    }

    var uid = $("#UId").val();
    var debitno = $("#DebitNo").val();
    var ApproveType = $("#ApproveType").val();
    var approveTo = $("#ApproveTo").val();

    if (!uid) {
        CommonFunc.Notify("", _getLang("L_TKBLQuery_Select", "Please Select a Record"), 500, "warning");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "ActManage/ApproveBackBill",
        type: 'POST',
        data: {
            "UId": uid,
            "DebitNo": debitno,
            "ApproveType": ApproveType,
            "ApproveTo": approveTo,
            "BackRemark": backremark
        },
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
            $("#CloseBackWin").trigger("click");
        },
        success: function (result) {
            //var resJson = $.parseJSON(result)
            CommonFunc.Notify("", result.message, 500, "warning");
            $("#CloseBackWin").trigger("click");
            //$("#SummarySearch").trigger("click");
            reloadStatus();
        }
    });
}

function SearchObj(obj, SearchCol, SearchString)
{
    SearchString = SearchString.toUpperCase();
    var returnObj = {
        "show": [],
        "hide": []
    };
    $.each(obj, function(i, v){
        if(obj[i][SearchCol].indexOf(SearchString) != -1)
        {
            returnObj["show"].push(i+1);
        }
        else
        {
            returnObj["hide"].push(i+1);
        }
    });

    console.log(returnObj);
    return returnObj;
}
function doDownloadExcelN() {
    var uid = $("#UId").val(), smid = $("#ShipmentId").val();
    var transType = $("#sel_tranTypeN").val();
    var chgTypeStr = "", chgTypeColsStr = "";

    var rowIds = $MainGrid.getDataIDs();
    if (rowIds.length <= 0) {
        alert(_getLang("L_ActCheckSetup_Script_9", "No application form for the selected bill!"));
        return;
    }

    $.ajax({
        async: true,
        url: rootPath + "ActManage/ExportExcel",
        type: 'POST',
        data: {
            UId: uid,
            shipmentid: smid,
            type: transType,
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            if (result != null && result.IsOk == "Y") {
                //window.open(rootPath + "DNManage/DownLoadXls?FileType=GwCbm&filename=" + result.file);
                window.open(rootPath + "ActManage/DownLoadXls?type=" + transType + "&filename=" + result.file);
            }
            else {
                alert(result.msg);
            }
        }
    });
}

function Approve() {
    var ApproveRemark = $("#ApproveRemark").val();
    var debitno = [];
    var uid = [];
    var id = $("#UId").val();
    debitno.push($("#DebitNo").val());
    uid.push(id);
    if (_approve === "Y") {
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "ActManage/ApproveBill",
            type: 'POST',
            data: {
                "UId": uid.toString(),
                "DebitNo": debitno.toString(),
                "ApproveRemark": ApproveRemark
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
                if (result.IsOk == "N") {
                    CommonFunc.Notify("", result.message, 500, "warning");
                }
                if (result.IsOk == "Y") {
                    CommonFunc.Notify("", result.message, 500, "success");
                }
                $('#Approve').modal('hide');
            }
        });
    }
    else {
        $.ajax({
            async: true,
            url: rootPath + "ActManage/ActPass",
            type: 'POST',
            data: { UId: _uid, "ApproveRemark": ApproveRemark },
            dataType: "json",
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang('L_ActCheckSetup_Scripts_29', '系统错误'), 500, "warning");
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                $('#Approve').modal('hide');
                var formData = $.parseJSON(result.returnData.Content);
                _handler.setFormData(formData);
                CommonFunc.Notify("", _getLang('L_ActCheckSetup_Scripts_30', '已处理'), 500, "success");
                CommonFunc.ToogleLoading(false);
            }
        });
    }
}

function InitFSSPiframe(TpvDebitNo, Stn) {
    //191705 
    var newType = "all";
    var strStatus = $("#Status").val();
    if (strStatus == "D" || strStatus == "E" || strStatus == "F")
        newType = "attachment";
    //end
    const iframe = document.getElementById("iframe");
    const subWindow = iframe.contentWindow;
    //为了iframe加载完成再进行通信传参，设定定时任务轮询iframe是否正常完成初始化，完成的话返回状态信息 ‘success’
    const timer = setInterval(() => {
        subWindow.postMessage({ action: "init" }, _target);
    }, 1000);
    window.addEventListener("message", (event) => {
        const data = event.data;
        if (data.status && data.status === "success") {
            clearInterval(timer); //iframe加载完毕之后发送用户信息以及需要的权限
            const param = {
                TIME_STAMP: Date.now(), //系统当前时间, 和iframe状态同步之后，如果系统在5s内没有完成正常的通信，需要重新初始化,
                PROCESS_TYPE: "SSC-AP-14",
                INTERACTION_ID: TpvDebitNo,
                COMPANY_CODE: Stn,
                SYSTEM_TAG: "ESP",
                LINK_IDS: "", // 关联的影像INTERACTION_ID，以逗号分隔
                USER_ID: UserId,
                USER_NAME: UserId,
                LANGUAGE: fssplang,
                OPER_TYPE_INFO: [newType]
            };
            var json = JSON.stringify(param)
            try {
                $.base64.utf8encode = true;
                json = $.base64.btoa(json);
            }
            catch (e) {

            }
            subWindow.postMessage(
                {
                    action: "update",
                    data: json,
                },
                _target
            );
        }
    });

}

var _showFSSPiframeModal = '<div class="modal fade" id="FSSPiframeDetail" Sid="" >\
  <div class="modal-dialog modal-lg">\
    <div class="modal-content">\
      <div class="modal-header">\
        <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
        <h4 class="modal-title">FSSP</h4>\
      </div>\
      <div class="modal-body" id="iframeContent">\
        <iframe src="'+ _target + '" id="iframe" height="100%" width="100%"></iframe>\
      </div>\
      <div class="modal-footer">\
        <button type="button" class="btn btn-default" data-dismiss="modal" id="ModalClose">Close</button>\
      </div>\
    </div>\
  </div>\
</div>';