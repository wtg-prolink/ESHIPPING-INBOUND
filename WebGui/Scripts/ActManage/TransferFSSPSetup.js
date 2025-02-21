var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid;
$(function () {

    _handler.saveUrl = rootPath + "ActManage/FSSPSaveData";
    _handler.inquiryUrl = rootPath + "ActManage/GetFSSPSetupDataItem";
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

        setFieldValue(data["main"] || [{}]);
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["SEND_BTN"]);
        MenuBarFuncArr.Enabled(["Send2FSSP"]);
        var status = $("#Status").val();
        var Receiver = $("#Receiver").val();

        /*问题单：账单录入，账单比对画面物流业者下方增加收款对象，默认与物流业者一样，可以允许修改*/
        if (Receiver == "") {
            if (typeof data["main"][0] !== "undefined") {
                $("#Receiver").val(data["main"][0]["LspNo"]);
                $("#ReceiverNm").val(data["main"][0]["LspNm"]);
            }
        }

        if (typeof data["main"][0] !== "undefined") {
            if (data["main"][0].ApproveTo != "A" && data["main"][0].ApproveTo != "LSTM") {
                MenuBarFuncArr.Disabled(["PASS_BTN", "REFUSE_BTN"]);
            }
        }
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(_handler.inquiryUrl, { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }


    $MainGrid = $("#MainGrid");

    var tvMntTypes = tvMntSelect.split(';');
    $.each(tvMntTypes, function (idx, value) {
        appendSelect(value, value);
    });

    var actFormatter1 = function (cellvalue) {
        var val = "";
        if (cellvalue == null || cellvalue === undefined || cellvalue == 0)
            val = "";
        else {
            val = cellvalue.replace(/,/g, '，');
        }
        return val;
    };

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'ShipmentId', title: _getLang('L_DNApproveManage_ShipmentId', 'Shipment ID'), index: 'ShipmentId', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'DebitNo', title: _getLang('L_ActQuery_DebitNo', '物流业者帐单号'), index: 'DebitNo', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Status', title: _getLang('L_ActManage_MatchorNot', '符合否'), index: 'Status', width: 50, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Y:Yes;N:No' } },
        { name: 'BlNo', title: _getLang('L_ActSetup_BlNo', '提单号码'), index: 'BlNo', sorttype: 'string', width: 120, hidden: true, editable: true },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', '费用代码'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', '费用说明'), index: 'ChgDescp', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Cur', title: _getLang('L_IpPart_Crncy', '币别'), index: 'Cur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
        {
            name: 'UnitPrice', title: _getLang('L_DNApproveManage_UnitPrice', '单价'), index: 'UnitPrice',
            width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
            hidden: false, editable: true
        },
        { name: 'ChgUnit', title: _getLang('L_ActSetup_ChgUnit', '计价单位'), index: 'ChgUnit', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Qty', title: _getLang('L_BaseLookup_Qty', '数量'), index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Tax', title: _getLang('L_ActSetup_Tax', '税率%'), index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
        //{ name: 'QunitPrice', title: '预提单价', index: 'QunitPrice', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
        { name: 'Bamt', title: _getLang('L_ActSetup_Amt', '请款金额'), index: 'Bamt', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: true },
        { name: 'Lamt', title: _getLang('L_ActManage_LocIvAmt', '本地请款金额'), index: 'Lamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },
        { name: 'Qcur', title: _getLang('L_BaseLookup_WithholdCur', '预提币别'), index: 'Qcur', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'Qamt', title: _getLang('L_ActSetup_Qamt', '预提金额'), index: 'Qamt', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: true },
        { name: 'Qlamt', title: _getLang('L_ActManage_LocWithAmt', '本地预提金额'), index: 'Qlamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },
        { name: 'BiRemark', title: _getLang('L_BSCSSetup_Remark', '备注'), index: 'BiRemark', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'CheckDescp', title: _getLang('L_ActSetup_CheckDescp', '拒绝原因'), index: 'CheckDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'Remark', title: _getLang('L_IpPart_WithholdingRemark', '预提备注'), index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true },
        {
            name: 'QunitPrice', title: _getLang('L_BaseLookup_WithholdPrice', '预提单价'), index: 'QunitPrice',
            width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
            hidden: true, editable: true
        },
        { name: 'QchgUnit', title: _getLang('L_ActCheckSetup_Scripts_26', '报价计价单位'), index: 'QchgUnit', sorttype: 'string', width: 80, hidden: true, editable: true },
        { name: 'Qqty', title: _getLang('L_BaseLookup_WithholdQty', '预提数量'), index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: true, editable: true },

        //{ name: 'Tax', title: _getLang('L_ActSetup_Tax',''), index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: true },
        //{ name: 'VatNo', title: _getLang('L_ActSetup_VatNo',''), index: 'VatNo', sorttype: 'string', width: 250, hidden: false, editable: false },
        { name: 'Qtax', title: _getLang('L_ActCheckSetup_Scripts_27', '报价税率'), index: 'Qtax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: true, editable: true },

        { name: 'CostCenter', title: _getLang('L_SMCHGSetup_ChgLevel', '成本中心'), index: 'CostCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'ProfitCenter', title: _getLang('L_ActManage_ProCenter', '利润中心'), index: 'ProfitCenter', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 85, hidden: false, editable: false },
        { name: 'DebitNm', title: 'Debit To Name', index: 'DebitNm', sorttype: 'string', width: 180, hidden: false, editable: false },
        { name: 'CombineRefno', title: 'DN NO2', index: 'CombineRefno', width: 250, align: 'left', sorttype: 'string', formatter: actFormatter1, hidden: false, classes: "normal-white-space" },
        { name: 'BankType', title: _getLang('L_BankInfo_BankType', '银行类型'), index: 'BankType', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Bu', title: _getLang('L_PostBillSetup_Bu', '销售组织'), index: 'Bu', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'DnType', title: _getLang('L_DNApproveManage_DnType', '类别'), index: 'DnType', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'DnNo', title: _getLang('L_DNApproveManage_DnNo', 'Dn No'), index: 'DnNo', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'CostCenter', title: _getLang('L_SMCHGSetup_ChgLevel', '成本中心'), index: 'CostCenter', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ProfitCenter', title: _getLang('L_ActManage_Profitcenter', '利润中心'), index: 'ProfitCenter', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Distribute', title: _getLang('L_CostStatistics_Allocation', 'Allocation'), index: 'Distribute', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'RefkeyTwo', title: 'Ref Key 2', index: 'RefkeyTwo', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PostMonth', title: _getLang('L_TransferFSSPSetup_PostMonth', 'Billing Month'), index: 'PostMonth', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'FsspEstno', title: _getLang('L_CostStatistics_FsspEstno', 'FSSP Estimate No'), index: 'FsspEstno', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Naamt', title: _getLang('L_ShowCostInfo_NTaxAmunt', '未税金额'), index: 'Naamt', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Alevel', title: _getLang('L_TransferFSSPSetup_Alevel', 'Tax Code'), index: 'Alevel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Atamt', title: _getLang('L_TransferFSSPSetup_Atamt', 'Tax Amount'), index: 'Atamt', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Neamt', title: _getLang('L_CostStatistics_Neamt', 'Estimated AMT Without Tax'), index: 'Neamt', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Elevel', title: _getLang('L_CostStatistics_Elevel', 'Estimated Tax Code'), index: 'Elevel', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Etamt', title: _getLang('L_CostStatistics_Etamt', 'Estimated Tax Amount'), index: 'Etamt', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Ecur', title: _getLang('L_CostStatistics_Ecur', '预估币别'), index: 'Ecur', sorttype: 'string', width: 80, hidden: false, editable: true },
        { name: 'PreNaamt', title: _getLang('L_TransferFSSPSetup_PreNaamt', 'Estimated and Actual before Tax Difference'), index: 'PreNaamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },
        { name: 'PreAamt', title: _getLang('L_TransferFSSPSetup_PreAamt', 'Estimated and Actual Tax Difference'), index: 'PreAamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },
        { name: 'PrdSum', title: _getLang('L_TransferFSSPSetup_PrdSum', 'Estimated and Actual Difference Amount'), index: 'PrdSum', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false }

    ];

    var postdata = { "conditions": "sopt_1=ne&1=0" };
    if (_uid)
        postdata = { "conditions": "sopt_UFid=eq&UFid=" + _uid };

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: 'FSSP ' + _getLang('L_Layout_FeeDetail', '费用明细'), delKey: ["UId"], height: 250,
        savelayout: true, showcolumns: true, exportexcel: true,
        url: rootPath + "ActManage/GetFSSPActDetail",
        postData: postdata,
        afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
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
    }, ["UId"], false);


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

    MenuBarFuncArr.AddMenu("Send2FSSP", "glyphicon glyphicon-bell", "Send To Fssp", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        var UId = $("#UId").val();
        var Cmp = $("#Cmp").val();
        var TpvDebitNo = $("#TpvDebitNo").val();
        var recalculate = window.confirm("是否需要重新计算");
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "ActManage/Send2FSSP",
            type: 'POST',
            data: { UId: UId, Cmp: Cmp, TpvDebitNo: TpvDebitNo, recalculate: recalculate },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                CommonFunc.ToogleLoading(false);
                if (data.IsOk)
                    CommonFunc.Notify("", "FSSP 发送成功:" + data.message, 1000, "warning");
                if (!data.IsOk)
                    CommonFunc.Notify("", "FSSP 发送失败:" + data.message, 1000, "warning");
            }
        });
    });

    _handler.beforLoadView = function () {
        var keys = ["ShipmentId", "DebitNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }
    }

    _handler.beforSave = function () {
        var remark = $("#Remark").val();
        if (isEmpty(remark)) {
            alert(_getLang("L_ActSetUp_setRemark", ''));
            return false;
        }
        return true;
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["DebitNo"] = encodeURIComponent($("#DebitNo").val());
        data["recal_amt"] = "Y";
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

    _initUI(["MBAdd", "MBEdit", "MBSave", "MBCancel", "MBDel", "MBSearch", "MBCopy", "MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    MenuBarFuncArr.Enabled(["MBEdoc", "Send2FSSP"]);
    setTimeout(function () {
        MenuBarFuncArr.DelMenu(["MBPreview"]);
    }, 300);
    getSelectOptions();

    $("#SmIdSearch").keypress(function (event) {
        $("#ChgCdSearch").val("");
        if (event.which == 13) {
            var obj = SearchObj(_dm.getDs("MainGrid")._data, "ShipmentId", $(this).val());
            $.each(obj, function (i, v) {
                $.each(obj["show"], function (k, val) {
                    $MainGrid.find("tr#" + obj["show"][k]).show();
                });

                $.each(obj["hide"], function (k, val) {
                    $MainGrid.find("tr#" + obj["hide"][k]).hide();
                });
            });
        }
    });

    $("#ChgCdSearch").keypress(function (event) {
        $("#SmIdSearch").val("");
        if (event.which == 13) {
            var obj = SearchObj(_dm.getDs("MainGrid")._data, "ChgCd", $(this).val());
            $.each(obj, function (i, v) {
                $.each(obj["show"], function (k, val) {
                    $MainGrid.find("tr#" + obj["show"][k]).show();
                });

                $.each(obj["hide"], function (k, val) {
                    $MainGrid.find("tr#" + obj["hide"][k]).hide();
                });
            });
        }
    });

    setselectdata(_selects);
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
            appendSelectOption($("#TranMode"), trnOptions);
            appendSelectOption($("#ServiceMode"), pkOptions);

            var tdltOptions = data.TDLT || [];
            appendSelectOption($("#BillTo"), tdltOptions);

            if (_handler.topData) {
                $("#BillTo").val(_handler.topData["BillTo"]);
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
                return map.ChgCd;
            }
        }, LookUpConfig.GetChgAuto1(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
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
        }, LookUpConfig.GetCurAuto(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.CUR, "lookup");
                $(elem).val(rd.CUR);
            }));
    return cur_op;
}

function SearchObj(obj, SearchCol, SearchString) {
    SearchString = SearchString.toUpperCase();
    var returnObj = {
        "show": [],
        "hide": []
    };
    $.each(obj, function (i, v) {
        if (obj[i][SearchCol].indexOf(SearchString) != -1) {
            returnObj["show"].push(i + 1);
        }
        else {
            returnObj["hide"].push(i + 1);
        }
    });

    console.log(returnObj);
    return returnObj;
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

function setselectdata(data) {
    var rmkOptions = data.RMK || [];
    appendSelectOption($("#ChgType"), rmkOptions);
    if (_handler.topData) {
        $("#ChgType").val(_handler.topData["ChgType"]);
    }
}