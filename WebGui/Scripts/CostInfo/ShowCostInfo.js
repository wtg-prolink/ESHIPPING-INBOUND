var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};

jQuery(document).ready(function ($) {

    var feeOpt = {};
    feeOpt.gridUrl = rootPath + "Common/GetCloseFeeData";
    feeOpt.gridReturnFunc = function (map) {
        var cd = map.ChgCd;
        var localdescp = map.ChgCnm;
        var rowid = $("#AccountsGrid").jqGrid('getGridParam', 'selrow');
        // $("#AccountsGrid").jqGrid('setCell', selRowId, 'ChgCd', cd);
        //$("#AccountsGrid").jqGrid('setCell', selRowId, 'ChgDescp', localdescp);
        //$("#" + selRowId + "_ChgCd" + "Input").val(cd);
        //$("#" + selRowId + "_ChgDescp" + "").val(localdescp);
        $("#AccountsGrid").jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "ChgDescp", null, localdescp);

        return cd;

    };
    feeOpt.lookUpConfig = LookUpConfig.BSCHGLookup;
    feeOpt.selfSite = true;
    feeOpt.autoCompKeyinNum = 1;
    feeOpt.gridId = "AccountsGrid";
    feeOpt.autoCompUrl = "";
    feeOpt.autoCompDt = "dt=bgn&GROUP_ID=" + groupId + "&CMP=" + basecmp + "&STN=" + basestn + "&DEP=WH&CHG_CD~";
    feeOpt.autoCompParams = "CHG_CD=showValue,CHG_CD,CHG_CNM";
    feeOpt.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CHG_CD);
        var rowid = $("#AccountsGrid").jqGrid('getGridParam', 'selrow');
        //$("#" + selRowId + "_ChgDescp" + "").val(ui.item.returnValue.CHG_CNM);
        $("#AccountsGrid").jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "ChgDescp", null, ui.item.returnValue.CHG_CNM);
    }
    var billOpt = {};

    billOpt.gridUrl = rootPath + "Common/GetBillCd";
    billOpt.gridReturnFunc = function (map) {
        var cd = map.CustCd;
        var rowid = $("#AccountsGrid").jqGrid('getGridParam', 'selrow');
        setGridChange({ "gridId": "#AccountsGrid", "rowId": rowid, "cellKey": "BillNm", "cellValue": map.LocalNm });
        return cd;

    };
    billOpt.lookUpConfig = LookUpConfig.BSCSMPLookup;
    billOpt.selfSite = true;
    billOpt.autoCompKeyinNum = 1;
    billOpt.gridId = "AccountsGrid";
    billOpt.autoCompUrl = "";
    //billOpt.baseConditionFunc = function () {
    //    return " (CUST_TYPE '%M%' OR CUST_TYPE LIKE '%P%') ";
    //}
    billOpt.autoCompDt = "dt=cus&CUST_CD=";
    billOpt.autoCompParams = "CUST_CD=showValue,CUST_CD,LOCAL_NM";
    billOpt.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CUST_CD);
        var rowid = $("#AccountsGrid").jqGrid('getGridParam', 'selrow');
        setGridChange({ "gridId": "#AccountsGrid", "rowId": rowid, "cellKey": "BillNm", "cellValue": ui.item.returnValue.LOCAL_NM });
    }

    var params = "";
    var numberTemplate = "2";
    //var colNames = ['费用代码', '费用描述', '币别', '金额', '汇率', 'RMB金额', '收款对象', '帐单号码', '备注'];
    var colModel = [
                { name: 'Seq', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'Seq', width: 50, align: 'left', sorttype: 'string', hidden: false, editable: false },
                { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(feeOpt) },
                { name: 'ChgDescp', title: '@Resources.Locale.L_ShowCostInfo_ChgDescp', index: 'ChgDescp', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true },
                { name: 'Cur', title: '@Resources.Locale.L_InvPkgSetup_Cur', index: 'Cur', width: 60, align: 'left', sorttype: 'string', hidden: false, editable: true },
                {
                    name: 'ActualAmt', title: '@Resources.Locale.L_DNDetailVeiw_Value', index: 'ActualAmt', width: 100, align: 'right', sorttype: 'currency', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#AccountsGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        val = ConvertToFloat($(e.target).val());
                                    var exchrt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "Exchrt", null);
                                    var lamt = floatMul(val, CommonFunc.formatFloat(exchrt, 4));
                                    lamt = CommonFunc.formatFloat(lamt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "LocalAmt", null, ConvertToFloat(lamt));

                                    var taxrate = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "TaxRate", null);
                                    var temp1 = floatMul(val, taxrate);
                                    var temp2 = floatAdd(100, taxrate);
                                    var taxamt = floatDiv(temp1, temp2);
                                    taxamt = CommonFunc.formatFloat(taxamt, 2);
                                    var ntaxAmt = floatSub(val, taxamt);
                                    ntaxAmt = CommonFunc.formatFloat(ntaxAmt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "TaxAmt", null, ConvertToFloat(taxamt));
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "NtaxAmt", null, ConvertToFloat(ntaxAmt));
                                }
                            }
                        ]
                    }
                },
                {
                    name: 'Exchrt', title: '@Resources.Locale.L_ActSetup_ExRate', index: 'Exchrt', width: 100, align: 'right', sorttype: 'currency', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#AccountsGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        val = ConvertToFloat($(e.target).val());
                                    var actualAmt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "ActualAmt", null);
                                    var lamt = floatMul(actualAmt, CommonFunc.formatFloat(val, 4));
                                    lamt = CommonFunc.formatFloat(lamt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "LocalAmt", null, ConvertToFloat(lamt));
                                }
                            }
                        ]
                    }
                },
                { name: 'LocalAmt', title: '@Resources.Locale.L_ActSetup_Lamt', index: 'LocalAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                {
                    name: 'TaxRate', title: '@Resources.Locale.L_ActDeatilManage_Views_69', index: 'TaxRate', width: 100, align: 'right', sorttype: 'number', hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#AccountsGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        taxrate = ConvertToFloat($(e.target).val());
                                    var actualAmt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "ActualAmt", null);
                                    var temp1 = floatMul(actualAmt, taxrate);
                                    var temp2 = floatAdd(100, taxrate);
                                    var taxamt = floatDiv(temp1, temp2);
                                    taxamt = CommonFunc.formatFloat(taxamt, 2);
                                    var ntaxAmt = floatSub(actualAmt, taxamt);
                                    ntaxAmt = CommonFunc.formatFloat(ntaxAmt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "TaxAmt", null, ConvertToFloat(taxamt));
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "NtaxAmt", null, ConvertToFloat(ntaxAmt));
                                }
                            }
                        ]
                    }
                },
                { name: 'TaxAmt', title: '@Resources.Locale.L_ShowCostInfo_TaxAmunt', index: 'TaxAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                { name: 'NtaxAmt', title: '@Resources.Locale.L_ShowCostInfo_NTaxAmunt', index: 'NtaxAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                { name: 'BillCd', title: '@Resources.Locale.L_ActManage_Receiver', index: 'BillCd', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(billOpt) },
                { name: 'BillNm', title: '@Resources.Locale.L_ForecastQueryData_CustNm', index: 'BillNm', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
                { name: 'InvNo', title: '@Resources.Locale.L_ActQuery_InvNo', index: 'InvNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
                {
                    name: 'DnDate', title: '@Resources.Locale.L_BaseLookup_DebitDate', index: 'DnDate', width: 140, align: 'left', sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                    editoptions: myEditDateInit,
                    formatter: 'date',
                    formatoptions: {
                        srcformat: 'ISO8601Long',
                        newformat: 'Y-m-d',
                        defaultValue: ""
                    }
                },
                { name: 'Rmk', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Rmk', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
                { name: 'GroupId', title: 'Group Id', index: 'GroupId', sorttype: 'string', hidden: true },
                { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
                { name: 'Stn', title: 'Stn', index: 'Stn', sorttype: 'string', hidden: true },
                { name: 'Dep', title: 'Dep', index: 'Dep', sorttype: 'string', hidden: true },
                { name: 'BlNo', title: 'BlNo', index: 'BlNo', sorttype: 'string', hidden: true },
                { name: 'ChgType', title: 'ChgType', index: 'ChgType', sorttype: 'string', hidden: true },
                { name: 'Dep', title: 'Dep', index: 'Dep', sorttype: 'string', hidden: true },
                { name: 'JobNo', title: 'Job No', index: 'JobNo', sorttype: 'string', hidden: true }
    ];


    $grid = $("#AccountsGrid");
    //_dm.addDs("AccountsGrid", [], ["ChgCd", "ChgType", "JobNo", "Seq", "UId"], $grid[0]);
    _dm.addDs("AccountsGrid", [], ["UId"], $grid[0]);
    var postdata = { "conditions": "sopt_1=ne&1=1" };
    /*
    if ($("#PoNo").text() != "") {
        var jobNo = $("#PoNo").text();
    }
    if ($("#BlNo").text() != "") {
        var jobNo = $("#BlNo").text();
    }
    if ($("#WiNo").text() != "") {
        var jobNo = $("#WiNo").text();
    }
    console.log(jobNo);
    */
    if (JobNo) {
        postdata = { "conditions": "sopt_JobNo=eq&JobNo=" + JobNo + "&sopt_ChgType=eq&ChgType=D" };
    }
    if (CHMSewqNo != null && CHMSewqNo != "") {
        postdata = { "conditions": "sopt_JobNo=eq&JobNo=" + JobNo + "&sopt_ChgType=eq&ChgType=D" + "&sopt_Seq=eq&Seq=" + CHMSewqNo };
    }
    new genGrid($grid, {
        colModel: colModel,
        datatype: "json",
        url: rootPath + "CostInfo/ChargeRequiry",
        refresh: true,
        postData: postdata,
        cellEdit: false,
        rowNum: 10,
        loadonce: true,
        pginput: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        caption: "@Resources.Locale.L_ShowCostInfo_Scripts_111",
        height: "auto",
        //multiboxonly: true,
        multiselect: true,
        // refresh: true,
        loadComplete: function (data) {
            if ($("#AccountsGrid").jqGrid('getGridParam', 'datatype') == "json" && data.rows.length == 0) {

                appendCHMFlamtData(CHMUId, CHMFlamt, "#AccountsGrid");
            }
            //var celldata = $("#AccountsGrid").jqGrid('getCell', 1, "LocalAmt");
            var records = $("#AccountsGrid").jqGrid('getRowData'),
                chargeAmt = 0;
            $(records).each(function () {
                chargeAmt += parseFloat(this.LocalAmt);
            });
            $("#chargeAmt").text(chargeAmt);
            var costAmt = $("#costAmt").text();
            $("#profitAmt").text(chargeAmt - parseFloat(costAmt));
        },
        onAddRowFunc: function (rowid) {

            $("#" + rowid + "_Cur" + "").val("RMB");
            $("#" + rowid + "_Exchrt" + "").val("1");
            var maxSeqNo = $('#AccountsGrid').jqGrid("getCol", "Seq", false, "max");           
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $("#AccountsGrid").jqGrid('getGridParam', "setGridCellValueCustom")("AccountsGrid", rowid, "Seq", null, maxSeqNo + 1);
        },
        afterAddRowFunc: function (rowid) {
            //CountLamt(rowid);
        },
        afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
            //CountLamt(rowid);
        },
        //開啟footer row
        //footerrow: true,
        //scroll: false,
        conditions: encodeURI(params),
        //beforeSelectRow: function () {
        //    return false;
        //},
        gridFunc: function (map) {
            //用于回调函数，例如赋值操作等
            //return false;
            //dblClick(map);
            //console.log(map);
        },
        //定義欄位計算
        //loadCompleteFunc: function ($grid) {

        //    var sum = $grid.jqGrid("getCol", "LocalAmt", false, "sum");
        //    // alert(sum);
        //    $grid.jqGrid("footerData", "set", { Exchrt: "Total:", LocalAmt: sum });

        //},
        beforeSelectRowFunc: function (rowid) {
            var $thisgrid = $("#AccountsGrid");
            var invno = $thisgrid.jqGrid("getCell", rowid, "InvNo");
            if (invno) {
                $.each(colModel, function (idx, element) {
                    if (element.editable === true)
                        $thisgrid.setColProp(element.name, { editable: false });
                });
            } else {
                $.each(colModel, function (idx, element) {
                    if (element.editable)
                        $thisgrid.setColProp(element.name, { editable: element.editable });
                });
            }
        },
        brforeDelRowFunc: function (rowid)
        {
            var $thisgrid = $("#AccountsGrid");
            var invno = $thisgrid.jqGrid("getCell", rowid, "InvNo");
            if (invno)
            {
                CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Post", 3500, "warning");
                return false;
            }
            return true;
        },
        ds: _dm.getDs("AccountsGrid"),
        sortname: "Seq",
        sortorder: "asc"
    });

    var feedOpt1 = {};

    feedOpt1.gridUrl = rootPath + "Common/GetCloseFeeData";
    feedOpt1.gridReturnFunc = function (map) {
        var cd = map.ChgCd;
        var localdescp = map.ChgCnm;
        var rowid = $("#CopeGrid").jqGrid('getGridParam', 'selrow');
        //setGridChange({ "gridId": "#CopeGrid", "rowId": rowid, "cellKey": "ChgDescp", "cellValue": localdescp });
        $("#CopeGrid").jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "ChgDescp", null, localdescp);
        return cd;

    };
    feedOpt1.lookUpConfig = LookUpConfig.BSCHGLookup;
    feedOpt1.selfSite = true;
    feedOpt1.autoCompKeyinNum = 1;
    feedOpt1.gridId = "CopeGrid";
    feedOpt1.autoCompUrl = "";
    feedOpt1.autoCompDt = "dt=bgn&GROUP_ID=" + groupId + "&CMP=" + basecmp + "&STN=" + basestn + "&DEP=WH&CHG_CD~";
    feedOpt1.autoCompParams = "CHG_CD=showValue,CHG_CD,CHG_CNM";
    feedOpt1.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CHG_CD);
        var rowid = $("#CopeGrid").jqGrid('getGridParam', 'selrow');
        //setGridChange({ "gridId": "#CopeGrid", "rowId": rowid, "cellKey": "ChgDescp", "cellValue": ui.item.returnValue.CHG_CNM });
        $("#CopeGrid").jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "ChgDescp", null, ui.item.returnValue.CHG_CNM);
    }

    var billdOpt1 = {};
    billdOpt1.gridUrl = rootPath + "Common/GetBillCd";
    billdOpt1.gridReturnFunc = function (map) {
        var cd = map.CustCd;
        var rowid = $("#CopeGrid").jqGrid('getGridParam', 'selrow');
        setGridChange({ "gridId": "#CopeGrid", "rowId": rowid, "cellKey": "BillNm", "cellValue": map.LocalNm });
        return cd;

    };
    billdOpt1.lookUpConfig = LookUpConfig.BSCSMPLookup;
    billdOpt1.selfSite = true;
    billdOpt1.autoCompKeyinNum = 1;
    billdOpt1.gridId = "CopeGrid";
    billdOpt1.autoCompUrl = "";
    billdOpt1.autoCompDt = "dt=cus&CUST_CD=";
    billdOpt1.autoCompParams = "CUST_CD=showValue,CUST_CD,LOCAL_NM";
    billdOpt1.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CUST_CD);
        var rowid = $("#CopeGrid").jqGrid('getGridParam', 'selrow');
        setGridChange({ "gridId": "#CopeGrid", "rowId": rowid, "cellKey": "BillNm", "cellValue": ui.item.returnValue.LOCAL_NM });
    }
    //var colNames2 = ['费用代码', '费用描述', '币别', '金额', '汇率', 'RMB金额', '付款对象', '帐单号码', '备注'];
    var colModel2 = [
                { name: 'Seq', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'Seq', width: 50, align: 'left', sorttype: 'string', hidden: false, editable: false },
                { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(feedOpt1) },
                { name: 'ChgDescp', title: '@Resources.Locale.L_ShowCostInfo_ChgDescp', index: 'ChgDescp', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true },
                { name: 'Cur', title: '@Resources.Locale.L_InvPkgSetup_Cur', index: 'Cur', width: 60, align: 'left', sorttype: 'string', hidden: false, editable: true },
                {
                    name: 'ActualAmt', title: '@Resources.Locale.L_DNDetailVeiw_Value', index: 'ActualAmt', width: 100, align: 'right', sorttype: 'currency', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#CopeGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        val = ConvertToFloat($(e.target).val());
                                    var exchrt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "Exchrt", null);
                                    var lamt = floatMul(val, CommonFunc.formatFloat(exchrt, 4));
                                    lamt = CommonFunc.formatFloat(lamt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "LocalAmt", null, ConvertToFloat(lamt));

                                    var taxrate = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "TaxRate", null);
                                    var temp1 = floatMul(val, taxrate);
                                    var temp2 = floatAdd(100, taxrate);
                                    var taxamt = floatDiv(temp1, temp2);
                                    taxamt = CommonFunc.formatFloat(taxamt, 2);
                                    var ntaxAmt = floatSub(val, taxamt);
                                    ntaxAmt = CommonFunc.formatFloat(ntaxAmt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "TaxAmt", null, ConvertToFloat(taxamt));
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "NtaxAmt", null, ConvertToFloat(ntaxAmt));                                   

                                }
                            }
                        ]
                    }
                },
                {
                    name: 'Exchrt', title: '@Resources.Locale.L_ActSetup_ExRate', index: 'Exchrt', width: 100, align: 'right', sorttype: 'currency', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#CopeGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        val = ConvertToFloat($(e.target).val());
                                    var actualAmt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "ActualAmt", null);
                                    var lamt = floatMul(actualAmt, CommonFunc.formatFloat(val, 4));
                                    lamt = CommonFunc.formatFloat(lamt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "LocalAmt", null, ConvertToFloat(lamt));
                                }
                            }
                        ]
                    }
                },
                { name: 'LocalAmt', title: '@Resources.Locale.L_ActSetup_Lamt', index: 'LocalAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                {
                    name: 'TaxRate', title: '@Resources.Locale.L_ActDeatilManage_Views_69', index: 'TaxRate', width: 100, align: 'right', sorttype: 'number', hidden: false, editable: true,
                    editoptions: {
                        dataInit: function (el) {
                            setTimeout(function () {
                                $(el).attr("enterType", "number");
                            })
                        },
                        dataEvents: [
                            {
                                type: "keyup",
                                fn: function (e) {
                                    var $pttGrid = $("#CopeGrid"),
                                        rowid = $pttGrid.getGridParam("selrow"),
                                        taxrate = ConvertToFloat($(e.target).val());
                                    var actualAmt = $pttGrid.jqGrid('getGridParam', "getGridCellValueCustom")($pttGrid, rowid, "ActualAmt", null);
                                    var temp1 = floatMul(actualAmt, taxrate);
                                    var temp2 = floatAdd(100, taxrate);
                                    var taxamt = floatDiv(temp1, temp2);
                                    taxamt = CommonFunc.formatFloat(taxamt, 2);
                                    var ntaxAmt = floatSub(actualAmt, taxamt);
                                    ntaxAmt = CommonFunc.formatFloat(ntaxAmt, 2);
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "TaxAmt", null, ConvertToFloat(taxamt));
                                    $pttGrid.jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "NtaxAmt", null, ConvertToFloat(ntaxAmt));
                                }
                            }
                        ]
                    }
                },
                { name: 'TaxAmt', title: '@Resources.Locale.L_ShowCostInfo_TaxAmunt', index: 'TaxAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                { name: 'NtaxAmt', title: '@Resources.Locale.L_ShowCostInfo_NTaxAmunt', index: 'NtaxAmt', width: 100, align: 'right', sorttype: 'number', formatter: 'currency', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
                { name: 'BillCd', title: '@Resources.Locale.L_ActManage_Receiver', index: 'BillCd', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(billdOpt1) },
                { name: 'BillNm', title: '@Resources.Locale.L_ForecastQueryData_CustNm', index: 'BillNm', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
                { name: 'InvNo', title: '@Resources.Locale.L_ActQuery_InvNo', index: 'InvNo', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: false },
                {
                    name: 'DnDate', title: '@Resources.Locale.L_BaseLookup_DebitDate', index: 'DnDate', width: 140, align: 'left', sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                    editoptions: myEditDateInit,
                    formatter: 'date',
                    formatoptions: {
                        srcformat: 'ISO8601Long',
                        newformat: 'Y-m-d',
                        defaultValue: ""
                    }
                },
                { name: 'Rmk', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Rmk', width: 150, align: 'left', sorttype: 'string', hidden: false, editable: true },
                { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
                { name: 'GroupId', title: 'Group Id', index: 'GroupId', sorttype: 'string', hidden: true },
                { name: 'Cmp', title: 'Cmp', index: 'Cmp', sorttype: 'string', hidden: true },
                { name: 'Stn', title: 'Stn', index: 'Stn', sorttype: 'string', hidden: true },
                { name: 'Dep', title: 'Dep', index: 'Dep', sorttype: 'string', hidden: true },
                { name: 'BlNo', title: 'BlNo', index: 'BlNo', sorttype: 'string', hidden: true },
                { name: 'ChgType', title: 'ChgType', index: 'ChgType', sorttype: 'string', hidden: true },
                { name: 'Dep', title: 'Dep', index: 'Dep', sorttype: 'string', hidden: true },
                { name: 'JobNo', title: 'Job No', index: 'JobNo', sorttype: 'string', hidden: true }
    ];

    $grid = $("#CopeGrid");
    //_dm.addDs("CopeGrid", [], ["ChgCd", "ChgType", "Seq", "JobNo", "UId"], $grid[0]);
    _dm.addDs("CopeGrid", [], ["UId"], $grid[0]);
    postdata = { "conditions": "sopt_1=ne&1=1" };
    if (JobNo) {
        postdata = { "conditions": "sopt_JobNo=eq&JobNo=" + JobNo + "&sopt_ChgType=eq&ChgType=C" };
    }
    if (CHMSewqNo != null && CHMSewqNo != "") {
        postdata = { "conditions": "sopt_JobNo=eq&JobNo=" + JobNo + "&sopt_ChgType=eq&ChgType=C" + "&sopt_Seq=eq&Seq=" + CHMSewqNo };
    }
    new genGrid($grid, {
        colModel: colModel2,
        datatype: "json",
        postData: postdata,
        url: rootPath + "CostInfo/ChargeRequiry",
        refresh: true,
        postData: postdata,
        cellEdit: false,
        rowNum: 10,
        loadonce: true,
        pginput: false,
        pgbuttons: false,
        viewrecords: false,
        exportexcel: false,
        caption: "@Resources.Locale.L_ShowCostInfo_Payable",
        height: "auto",
        multiselect: true,
        conditions: encodeURI(params),
        loadComplete: function (data) {
            //var celldata = $("#CopeGrid").jqGrid('getCell', 1, "LocalAmt");
            console.log(data);
            if ($("#CopeGrid").jqGrid('getGridParam', 'datatype') == "json" && data.rows.length == 0) {

                appendCHMPlamtData(CHMUId, CHMPlamt, "#CopeGrid");

            }
            var records = $("#CopeGrid").jqGrid('getRowData'),
                costAmt = 0;
            $(records).each(function () {
                costAmt += parseFloat(this.LocalAmt);
            });
            $("#costAmt").text(costAmt);
            var chargeAmt = $("#chargeAmt").text();
            $("#profitAmt").text(parseFloat(chargeAmt) - costAmt);

            //alert("222");
        },
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $('#CopeGrid').jqGrid("getCol", "Seq", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            //$("#" + rowid + "_SeqNo" + "").val(maxSeqNo + 1);
            $("#CopeGrid").jqGrid('getGridParam', "setGridCellValueCustom")("CopeGrid", rowid, "Seq", null, maxSeqNo + 1);
            $("#" + rowid + "_Cur" + "").val("RMB");
            $("#" + rowid + "_Exchrt" + "").val("1");

        },
        afterAddRowFunc: function (rowid) {
            //CountLamtd(rowid);
        },
        afterSaveCellFunc: function (rowid, name, val, iRow, iCol) {
            //CountLamtd(rowid);
        },
        gridFunc: function (map) {
        },
        beforeSelectRowFunc: function (rowid) {
            var $thisgrid = $("#CopeGrid");
            var invno = $thisgrid.jqGrid("getCell", rowid, "InvNo");
            if (invno) {
                $.each(colModel2, function (idx, element) {
                    if (element.editable === true)
                        $thisgrid.setColProp(element.name, { editable: false });
                });
            } else {
                $.each(colModel2, function (idx, element) {
                    if (element.editable)
                        $thisgrid.setColProp(element.name, { editable: element.editable });
                });                
            }
        },
        brforeDelRowFunc: function (rowid) {
            var $thisgrid = $("#CopeGrid");
            var invno = $thisgrid.jqGrid("getCell", rowid, "InvNo");
            if (invno) {
                CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Post", 3500, "warning");
                return false;
            }
            return true;
        },
        ds: _dm.getDs("CopeGrid"),
        sortname: "Seq",
        sortorder: "asc"
    });

    MenuBarFuncArr.MBEdit = function () {
        //$('#AccountsGrid').jqGrid('setGridParam', { cellEdit: true, cellsubmit: 'clientArray' })
        //$('#AccountsGrid').jqGrid('getGridParam', "addRowButton")();
        if (CHMSewqNo != null && CHMSewqNo != "") {
            gridEditableCtrl({ editable: false, gridId: "AccountsGrid" });
            gridEditableCtrl({ editable: false, gridId: "CopeGrid" });
        }
        else {
            gridEditableCtrl({ editable: true, gridId: "AccountsGrid" });
            gridEditableCtrl({ editable: true, gridId: "CopeGrid" });
        }
        $("#btncreatebill").prop("disabled", true);
        $("#btndeletebill").prop("disabled", true);
    }

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBGoBack"]);
        var $chargeGrid = $("#AccountsGrid"),
           $costGrid = $("#CopeGrid");
        var selRowId = $chargeGrid.jqGrid('getGridParam', 'selrow');
        $chargeGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $chargeGrid.jqGrid('getGridParam', "endEdit")();
        selRowId = $costGrid.jqGrid('getGridParam', 'selrow');
        $costGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $costGrid.jqGrid('getGridParam', "endEdit")();
        gridEditableCtrl({ editable: false, gridId: "AccountsGrid" });
        gridEditableCtrl({ editable: false, gridId: "CopeGrid" });
        $("#btncreatebill").prop("disabled", false);
        $("#btndeletebill").prop("disabled", false);

    }

    MenuBarFuncArr.MBSave = function (dtd) {
       
        console.log(BlNo);
        if (BlNo == null || BlNo == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_BLNo", 2500, "danger");
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return;
        }
        var tempJobNo = "";
        var $chargeGrid = $("#AccountsGrid"),
            $costGrid = $("#CopeGrid");
        var selRowId = $chargeGrid.jqGrid('getGridParam', 'selrow');
        $chargeGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $chargeGrid.jqGrid('getGridParam', "endEdit")();
        selRowId = $costGrid.jqGrid('getGridParam', 'selrow');
        $costGrid.jqGrid('saveRow', selRowId, false, 'clientArray');
        $costGrid.jqGrid('getGridParam', "endEdit")();
        if ($("#PoNo").text() != "") {
            tempJobNo = $("#PoNo").text();
        }
        if ($("#WiNo").text() != "") {
            tempJobNo = $("#WiNo").text();
        }
        //处理应收费用
        var chargeArray = _dm.getDs("AccountsGrid").getChangeValue();
        var allData = $('#AccountsGrid').jqGrid("getGridParam", "data");
        console.log(allData);
        if (CmpStn.length != 5) {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Agent", 3500, "danger");
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return;
        }
        //var isexistarr = new Array();
        $.each(allData, function (i, val) {
            //isexistarr.push(val["ChgCd"]);
            if (val._id_ && val._id_.indexOf("jqg") >= 0) {
                val["__state"] = "1";
                val["GroupId"] = groupId;
                val["Cmp"] = CmpStn.substr(0, 2);
                val["Stn"] = CmpStn.substr(2, 3);
                //val["Dep"] = Dep;
                //val["JobNo"] = tempJobNo;
                val["JobNo"] = JobNo;
                val["BlNo"] = BlNo;
                val["ChgType"] = "D";
                val["Dep"] = ChargeDep;
                delete val["_id_"];
                delete val["rn"];                
                chargeArray.push(val);
            }
        });
        //if (isexistarr.duplicated()) {
        //    CommonFunc.Notify("", "存在相同的应收费用", 3500, "danger");
        //    MenuBarFuncArr.SaveResult = false;
        //    dtd.resolve();
        //    return;
        //}

        var changeData = {};
        changeData["chargeDt"] = chargeArray;
        //changeData["JobNo"] = tempJobNo;
        changeData["JobNo"] = JobNo;

        //处理应付费用
        var costArray = _dm.getDs("CopeGrid").getChangeValue();
        allData = $('#CopeGrid').jqGrid("getGridParam", "data");
        console.log(allData);
        isexistarr = new Array();
        $.each(allData, function (i, val) {
            //isexistarr.push(val["ChgCd"]);
            if (val._id_ && val._id_.indexOf("jqg") >= 0) {
                val["__state"] = "1";
                val["GroupId"] = groupId;
                val["Cmp"] = CmpStn.substr(0, 2);
                val["Stn"] = CmpStn.substr(2, 3);
                //val["Dep"] = Dep;
                //val["JobNo"] = tempJobNo;
                val["JobNo"] = JobNo;
                val["BlNo"] = BlNo;
                val["ChgType"] = "C";
                val["Dep"] = ChargeDep;
                delete val["_id_"];
                delete val["rn"];                
                costArray.push(val);
            }
        });
        //if (isexistarr.duplicated()) {
        //    CommonFunc.Notify("", "存在相同的应收费用", 3500, "danger");
        //    MenuBarFuncArr.SaveResult = false;
        //    dtd.resolve();
        //    return;
        //}
        changeData["costDt"] = costArray;

        $.ajax({
            async: true,
            url: rootPath + "CostInfo/ChargeUpdateData",
            type: 'POST',
            data: { "changedData": escape(JSON.stringify(changeData)), autoReturnData: true, 'CHMSewqNo': CHMSewqNo },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return;
                }
                $("#AccountsGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.chargeData
                }).trigger("reloadGrid");
                $("#CopeGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.costData
                }).trigger("reloadGrid");
                setdisabled(true);
                setToolBtnDisabled(true);
                gridEditableCtrl({ editable: false, gridId: "AccountsGrid" });
                gridEditableCtrl({ editable: false, gridId: "CopeGrid" });
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                CommonFunc.ToogleLoading(false);
                MenuBarFuncArr.Enabled(["MBGoBack"]);
                $("#btncreatebill").prop("disabled", false);
                $("#btndeletebill").prop("disabled", false);
                dtd.resolve();
            }
        });

        //$("#AccountsGrid").jqGrid('getGridParam', "endEdit")();
        //var containerArray = $('#AccountsGrid').getChangedCells();
        //var dirtyRows = $('#AccountsGrid').jqGrid('getChangedCells', 'dirty');
        return dtd.promise();
    }
    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);


    MenuBarFuncArr.AddMenu("MBGoBack", "glyphicon glyphicon-search", "@Resources.Locale.L_ShowCostInfo_Return", function () {
        if (WicUId != "") {
            location.href = rootPath + "IPWIC/WICSetup?UId=" + WicUId;
        }
        if (CHMUId != "") {
            location.href = rootPath + "IPCHM/CHMSetup?PoNo=" + $("#PoNo").text();
        }
        if (PomcUid != "") {
            location.href = rootPath + "IPPOM/POMCSetup?uid=" + PomcUid;
        }
        if (SomUid != "") {
            location.href = rootPath + "IPPOM/SOMSetup?uid=" + SomUid;
        }
        if (PomUid != "") {
            location.href = rootPath + "IPPOM/POMSetup?uid=" + PomUid;
        }
        if (LcUid != "") {
            location.href = rootPath + "IPPOL/POLSetup?uid=" + LcUid;
        }
    });

    $("#btncreatebill").click(function () {
        if ($("#MBSave").prop("disabled") == false) {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Save", 500, "danger");
            return;
        }
        var invnos = "";
       
        var rowids1 = $("#AccountsGrid").getGridParam("selarrrow");
        $.each(rowids1, function (idx, item) {
            var rowData = $("#AccountsGrid").getRowData(item);
            //invnos += rowData.JobNo + "," + rowData.ChgCd + "," + rowData.ChgType + "," + rowData.Seq + ";";
            invnos += rowData.UId + ";";
        });
        var rowids2 = $("#CopeGrid").getGridParam("selarrrow");
        $.each(rowids2, function (idx, item) {
            var rowData = $("#CopeGrid").getRowData(item);
            //invnos += rowData.JobNo + "," + rowData.ChgCd + "," + rowData.ChgType + "," + rowData.Seq + ";";
            invnos += rowData.UId + ";";
        });

        var conditions = {};
        conditions.invnos = invnos;

        if (invnos == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Select", 500, "danger");
            return;
        }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            data: conditions,
            url: rootPath + "CostInfo/CreateBill",
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.ToogleLoading(false);
            },
            success: function (data) {
                if (data.result == true && data.message=="") {
                    CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_GBS", 1500, "success");
                    setTimeout(function () {
                        location.reload();
                    }, 2500);                   
                } else {
                    var message = data.message;
                    if (message) {
                        CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_GBF" + "," + message, 3500, "warning");
                    } else {
                        CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_GBF", 500, "warning");
                    }
                }
                CommonFunc.ToogleLoading(false);
            }
        });
    });

    $("#btncreatebill").prop("disabled", false);
    $("#btndeletebill").prop("disabled", false);

    $("#btndeletebill").click(function () {
        if ($("#MBSave").prop("disabled") == false) {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Save", 500, "danger");
            return;
        }
        if (!confirm("@Resources.Locale.L_ShowCostInfo_ConfBill")) {
            //dtd.resolve();
            return;
        }
        var invnos = "";
        var uids = "";
        var rowids1 = $("#AccountsGrid").getGridParam("selarrrow");
        $.each(rowids1, function (idx, item) {
            var rowData = $("#AccountsGrid").getRowData(item);
            //invnos += rowData.JobNo + "," + rowData.ChgCd + "," + rowData.ChgType + "," + rowData.Seq + ";";
            uids += rowData.UId + ";";
            invnos += rowData.InvNo + ";";
        });
        var rowids2 = $("#CopeGrid").getGridParam("selarrrow");
        $.each(rowids2, function (idx, item) {
            var rowData = $("#CopeGrid").getRowData(item);
            //invnos += rowData.JobNo + "," + rowData.ChgCd + "," + rowData.ChgType + "," + rowData.Seq + ";";
            uids += rowData.UId + ";";
            invnos += rowData.InvNo + ";";
        });

        var conditions = {};
        conditions.invnos = invnos;
        conditions.uids = uids;
        if (invnos == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_Select", 500, "danger");
            return;
        }
        CommonFunc.ToogleLoading(true);
        if (JobNo) {
            $.ajax({
                async: true,
                cache: false,
                dataType: "json",
                data: conditions,
                url: rootPath + "CostInfo/checkLockDate",
                type: 'POST',
                "error": function (xmlHttpRequest, errMsg) {
                    alert(errMsg);
                    CommonFunc.ToogleLoading(false);
                },
                success: function (data) {
                    if (data.message == "") {
                        CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_DelS", 1500, "success");
                        setTimeout(function () {
                            location.reload();
                        }, 2500);                   
                    } else {
                        var message = data.message;
                        CommonFunc.Notify("", "@Resources.Locale.L_ShowCostInfo_DelF" + "," + message, 3500, "warning");
                        
                    }
                    CommonFunc.ToogleLoading(false);
                }
            });   
        }

    });
});// end function

function appendCHMPlamtData(CHMUId, Plamt, tempgrid) {
    if (!CHMUId)
        return;
    var param = "sopt_UId=eq&UId=" + CHMUId;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "CostInfo/CHMSetupInquiryData",
        type: 'POST',
        data: {
            sidx: 'UId',
            'conditions': encodeURI(param),
            'UId': CHMUId,
            'tempGrid': tempgrid,
            page: 1,
            rows: 20
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var $grid = $(tempgrid);
            CommonFunc.ToogleLoading(false);

            $grid.jqGrid("addRowData", undefined, result.rows, "last");

            if (tempgrid == "#CopeGrid") {
                var costAmt = 0;
                costAmt = result.rows[0].ActualAmt;
                chargeAmt = result.rows[0].ActualAmt2;
                $("#costAmt").text(costAmt);
                $("#profitAmt").text(chargeAmt - parseFloat(costAmt));
            }
            MenuBarFuncArr.Enabled(["MBSave"]);
        }
    });
}
function appendCHMFlamtData(CHMUId, Flamt, tempgrid) {
    if (!CHMUId)
        return;
    var param = "sopt_UId=eq&UId=" + CHMUId;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "CostInfo/CHMSetupInquiryData",
        type: 'POST',
        data: {
            sidx: 'UId',
            'conditions': encodeURI(param),
            'UId': CHMUId,
            'tempGrid': tempgrid,
            page: 1,
            rows: 20
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (result) {
            var $grid = $(tempgrid);
            CommonFunc.ToogleLoading(false);

            $grid.jqGrid("addRowData", undefined, result.rows, "last");
            if (tempgrid == "#AccountsGrid") {
                var chargeAmt = 0;
                chargeAmt = result.rows[0].ActualAmt;
                $("#chargeAmt").text(chargeAmt);

            }
            MenuBarFuncArr.Enabled(["MBSave"]);
        }
    });
}

