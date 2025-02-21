var $grid,$grid2;
function initQTGrid() {
    $grid = $("#MainGrid");
    $grid2 = $("#SubGrid");

    function getTermop(name) {
        var _name = name;
        var term_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TermUrl,
                config: LookUpConfig.TermLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TD", $grid,
            function ($grid, rd, elem) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return term_op;
    }

    function get_cntryop(name) {
        var _name = name;
        var cntry_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CountryUrl,
                config: LookUpConfig.CountryLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Region", map.CntryCd, "lookup");
                    return map.CntryCd;
                }
            }, LookUpConfig.GetCountryAuto(groupId, $grid,
            function ($grid, rd, elem, selRowId) {
                setGridVal($grid, selRowId, "Region", rd.CNTRY_CD, "lookup");
                //$(elem).val(rd.CNTRY_CD);
            }), { param: "" });
        return cntry_op;
    }

    //function getcityop(name) {
    //    var _name = name;
    //    var city_op = getLookupOp("MainGrid",
    //        {
    //            url: rootPath + LookUpConfig.CityPortUrl,
    //            config: LookUpConfig.CityPortLookup,
    //            returnFn: function (map, $grid) {
    //                //var selRowId = $grid.jqGrid('getGridParam', 'selrow');
    //                //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
    //                return map.CntryCd + map.PortCd;
    //            }
    //        }, LookUpConfig.GetCityPortAuto(groupId, $grid,
    //        function ($grid, rd, elem) {
    //            $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
    //        }));
    //    return city_op;
    //}
    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    if ("PodCd" === _name) {
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                        setGridVal($grid, selRowId, "Region", map.CntryCd, "lookup");
                    }
                    else if ("PolCd" === _name)
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else if ("ViaCd" === _name)
                        setGridVal($grid, selRowId, "ViaNm", map.PortNm, null);

                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $grid,
            function ($grid, rd, elem, selRowId) {
                if ("PodCd" === _name) {
                    setGridVal($grid, selRowId, "PodNm", rd.PORT_NM, null);
                    setGridVal($grid, selRowId, "Region", rd.CNTRY_CD, "lookup");
                }
                else if ("PolCd" === _name)
                    setGridVal($grid, selRowId, "PolNm", rd.PORT_NM, null);
                else if ("ViaCd" === _name)
                    setGridVal($grid, selRowId, "ViaNm", rd.PORT_NM, null);
                $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
            }), { param: "" });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var cust_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup1,
                returnFn: function (map, $grid) {
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyTypeNoAuto(groupId,undefined, $grid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.PARTY_NO, "lookup");
                $(elem).val(rd.PARTY_NO);
            }), {
                param: ""
                //baseConditionFunc: function () {
                //    return "PARTY_TYPE='CA'";
                //}
            });
        return cust_op;
    }
    //function getcust(name) {
    //    var _name = name;
    //    var unit_op = getLookupOp("MainGrid",
    //        {
    //            url: rootPath + LookUpConfig.TALNUrl,
    //            config: LookUpConfig.BSCodeLookup,
    //            returnFn: function (map, $grid) {
    //                return map.Cd;
    //            }
    //        }, LookUpConfig.GetCodeTypeAuto(groupId, "TALN", $grid,
    //        function ($grid, rd, elem, rowid) {
    //            $(elem).val(rd.CD);
    //        }), { param: "" });
    //    return unit_op;
    //}


    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Cur', title: '@Resources.Locale.L_IpPart_Crncy', index: 'Cur', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: '@Resources.Locale.L_AirQuery_LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: '@Resources.Locale.L_AirQuery_AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'ViaNm', title: 'ViaNm', index: 'ViaNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolCd', title: '@Resources.Locale.L_IESetup_PolCd', index: 'PolCd', editoptions: gridLookup(getcityop("PolCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PolNm', title: '@Resources.Locale.L_IESetup_PolCd' + " Name", index: 'PolNm', sorttype: 'string', width: 140, editable: false, hidden: false },
        { name: 'PodCd', title: '@Resources.Locale.L_IESetup_PodCd', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'PodNm', title: '@Resources.Locale.L_IESetup_PodCd' + " Name", index: 'PodNm', sorttype: 'string', width: 140, editable: false, hidden: false },
        { name: 'Region', title: 'Country', index: 'Region', sorttype: 'string', width: 100, hidden: false, editable: true, editoptions: gridLookup(get_cntryop("Region")), edittype: 'custom' },
        { name: 'Carrier', title: '@Resources.Locale.L_DNApproveManage_CaCd', index: 'Carrier', editoptions: gridLookup(getcust("Carrier")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ViaNm', title: '@Resources.Locale.L_IEQuery_ViaNm', index: 'ViaNm', sorttype: 'string', width: 80, hidden: false, editable: true },
        //{
        //    name: 'EffectDate', title: '@Resources.Locale.L_IEQuery_EffectDate', index: 'EffectDate', hidden: false, editable: true, formatter: 'date',
        //    editoptions: myEditDateInit,
        //    formatoptions: {
        //        srcformat: 'ISO8601Long',
        //        newformat: 'Y-m-d',
        //        defaultValue: null
        //    }
        //},
        { name: 'Tt', title: '@Resources.Locale.L_RouteSetup_Tt', index: 'Tt', width: 80, align: 'right', formatter: 'integer', editable: true, hidden: false },
        { name: 'F11', title: "0.5", index: 'F11', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F12', title: "1", index: 'F12', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F13', title: "1.5", index: 'F13', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F14', title: "2", index: 'F14', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F15', title: "2.5", index: 'F15', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F16', title: "3", index: 'F16', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F17', title: "3.5", index: 'F17', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F18', title: "4", index: 'F18', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F19', title: "4.5", index: 'F19', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F20', title: "5", index: 'F20', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F21', title: "5.5", index: 'F21', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F22', title: "6", index: 'F22', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F23', title: "6.5", index: 'F23', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F24', title: "7", index: 'F24', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F25', title: "7.5", index: 'F25', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F26', title: "8", index: 'F26', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F27', title: "8.5", index: 'F27', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F28', title: "9", index: 'F28', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F29', title: "9.5", index: 'F29', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F30', title: "10", index: 'F30', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F31', title: "10.5", index: 'F31', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F32', title: "11", index: 'F32', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F33', title: "11.5", index: 'F33', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F34', title: "12", index: 'F34', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F35', title: "12.5", index: 'F35', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F36', title: "13", index: 'F36', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F37', title: "13.5", index: 'F37', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F38', title: "14", index: 'F38', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F39', title: "14.5", index: 'F39', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F40', title: "15", index: 'F40', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F41', title: "15.5", index: 'F41', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F42', title: "16", index: 'F42', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F43', title: "16.5", index: 'F43', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F44', title: "17", index: 'F44', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F45', title: "17.5", index: 'F45', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F46', title: "18", index: 'F46', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
         { name: 'F47', title: "18.5", index: 'F47', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F48', title: "19", index: 'F48', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F49', title: "19.5", index: 'F49', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F50', title: "20", index: 'F50', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F1', title: "+20", index: 'F1', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F2', title: "+30", index: 'F2', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F3', title: "+40", index: 'F3', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F4', title: "+50", index: 'F4', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F5', title: "+100", index: 'F5', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F6', title: "+200", index: 'F6', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'F7', title: "+300", index: 'F7', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, sorttype: 'string', hidden: false, editable: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
       { name: 'QuotNo', title: '@Resources.Locale.L_QTQuery_QuotNo', index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];

    function getchg(name) {
        var _name = name;
        var chg_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.ChgUrl,
                config: LookUpConfig.ChgLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "ChgDescp", map.ChgDescp, null);
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    return map.ChgCd;
                }
            }, LookUpConfig.GetChgAuto1(groupId, undefined, $grid2,
            function ($grid, rd, elem, rowid) {

                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                $(elem).val(rd.CHG_CD);
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "TRAN_MODE IN ('E','O')";
                }
            });
        return chg_op;
    }

    function getUnit(name) {
        var _name = name;
        var unit_op = getLookupOp("SubGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $grid2,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }));
        return unit_op;
    }

    var colModel2 = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Cur', title: 'Cur', index: 'Cur', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: '@Resources.Locale.L_DNApproveManage_Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: '@Resources.Locale.L_RQQuery_TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'ChgCd', title: '@Resources.Locale.L_SMCHGSetup_ChgCd', index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'ChgDescp', title: '@Resources.Locale.L_SMCHGSetup_ChgDescp', index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
        { name: 'Punit', title: '@Resources.Locale.L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: true },
        { name: 'F1', title: "@Resources.Locale.L_IpPart_UnitPrice", index: 'F1', width: 90, align: 'right', formatter: 'integer', editable: true, hidden: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 150, hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false }
    ];

    _handler.intiGrid("MainGrid", $grid, {
        colModel: colModel, caption: '@Resources.Locale.L_QTManage_IntEpQuo', delKey: ["UId"], sortorder: "asc", sortname: "CreateDate", height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $grid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $grid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setGridVal($grid, rowid, "SeqNo", maxSeqNo + 1);
            //setGridVal($grid, rowid, "EffectDate", getDate(0,"-"));
            setGridVal($grid, rowid, "ChgCd", "FRT");
            setGridVal($grid, rowid, "Region", $("#Region").val()||"", "lookup");
            setDefutltGridData($grid, rowid, { "PolCd": true, "PodCd": true });
            //setGridVal($grid, rowid, "Carrier", cmp);
        },
        beforeSelectRowFunc: function (rowid) {
            //main key 修改時不允與修改
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
            //$SubGrid.setColProp('DocType', { editable: true });
        }
    });

    _handler.intiGrid("SubGrid", $grid2, {
        colModel: colModel2, caption: '@Resources.Locale.L_QTManage_OthChg', delKey: ["UId"], sortorder: "asc", sortname: "CreateDate", height: 200,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $grid2.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $grid2.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($grid2, rowid);
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
            //add row 時要可以編輯main key
        }
    });
}

$(function () {
    intQtView();
    initQTGrid();

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PolCd", index: 10, text: 'From' });
        nullCols.push({ name: "Region", index: 14, text: 'Country' });
        //nullCols.push({ name: "PodCd", index: 13, text: 'To' });
        //nullCols.push({ name: "Carrier", index: 14, text: 'Carrier' });
        //nullCols.push({ name: "EffectDate", index: 16, text: 'EffictiveDate' });
        if (_handler.checkData($grid, nullCols, sameCols) === false)
            return false;

        nullCols = [];
        sameCols = [];
        nullCols.push({ name: "ChgCd", index: 12, text: '@Resources.Locale.L_SMCHGSetup_ChgCd' });
        nullCols.push({ name: "Punit", index: 14, text: '@Resources.Locale.L_ActSetup_ChgUnit' });
        return _handler.checkData($grid2, nullCols, sameCols);
    }

    _handler.saveData = function (dtd) {
        var containerArray = $grid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        changeData["sub1"] = $grid2.jqGrid('getGridParam', "arrangeGrid")();
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
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
            _handler.loadGridData("MainGrid", $grid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $grid[0], [], [""]);

        if (data["sub1"])
            _handler.loadGridData("SubGrid", $grid2[0], data["sub1"], [""]);
        else
            _handler.loadGridData("SubGrid", $grid2[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        //MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
        MenuBarFuncArr.Enabled(["VoidBtn"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        var multiEdocData = [
        { jobNo: _handler.topData["RqUid"], 'GROUP_ID': _handler.topData["RqGroupid"], 'CMP': _handler.topData["RqCmp"], 'STN': '*' }
        ];
        //alert(_handler.topData["RqUid"] + _handler.topData["RqGroupid"] + _handler.topData["RqCmp"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);
        setRQData(data);
    }

    //registBtnLookup($("#LspCdLookup"), {
    //    item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#LspCd").val(map.PartyNo);
    //        $("#LspNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#LspNm").val(rd.PARTY_NAME);
    //}));

    //registBtnLookup($("#CurLookup"), {
    //    item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
    //        $("#Cur").val(map.Cur);
    //    }
    //}, undefined, LookUpConfig.GetCurAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $(elem).val(rd.CUR);
    //}));

    loadQtView();
});




