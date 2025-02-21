var $MainGrid;
var _fields = [];
function _checkEdit(type) { 
    /*if (type !== 2) {
        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", _getLang("L_QTSetup_Connot","unable to  modify other\'s quotation"), 500, "warning");
            return false;
        }
    }*/
    if (type === 1)
        return;
    if (_handler.topData["QuotType"] === "A") {
        CommonFunc.Notify("", _getLang("L_QTSetup_Verified", "approved"), 500, "warning");
        return false;
    }
    if (_handler.topData["QuotType"] === "V") {
        CommonFunc.Notify("", _getLang("L_QTSetup_HasDiscd", "Cancelled"), 500, "warning");
        return false;
    }
    if (type === 2)
        return;
    /*if (userId !== _handler.topData["CreateBy"]) {
        CommonFunc.Notify("", _getLang("L_QTSetup_Connot","unable to  modify other\'s quotation"), 500, "warning");
        return false;
    }*/
}

function initQTGrid(MainData) {
    $MainGrid = $("#MainGrid");
    function getcityop1(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.CityPortUrl,
                config: LookUpConfig.CityPortLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (_name === "PolCd")
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    setGridVal($grid, selRowId, _name, map.CntryCd + map.PortCd, "lookup");

                    return map.CntryCd + map.PortCd;
                }
            }, LookUpConfig.GetCityPortAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CNTRY_CD + rd.PORT_CD);
                if (_name === "PolCd")
                    setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                else
                    setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                setGridVal($grid, rowid, _name, rd.CNTRY_CD + rd.PORT_CD, "lookup");
            }), { param: "" });
        return city_op;
    }

    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.TruckPortCdUrl,
                config: LookUpConfig.TruckPortCdLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (_name === "PolCd")
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else if(_name === "PodCd")
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    else
                        setGridVal($grid, selRowId, "BackLocationNm", map.PortNm, null);

                    setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return map.PortCd;
                }
            }, LookUpConfig.TruckPortCdAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                if (_name === "PolCd")
                    setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                else if(_name === "PodCd")
                    setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                else
                    setGridVal($grid, rowid, "BackLocationNm", rd.PORT_NM, null);

                $(elem).val(rd.PORT_CD);
                setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
            }, function () {
                if (_name === "PolCd" || _name === "PodCd")
                {
                    var cmp = $("#Rlocation").val();
                    if (cmp == undefined)
                        var cmp = _handler.topData["Rlocation"];
                    return " CMP=" + cmp;
                }
            }), {
                param: "",
                baseConditionFunc: function () {
                    if (_name === "PolCd" || _name === "PodCd") {
                        var cmp = $("#Rlocation").val();
                        if (cmp == undefined)
                            var cmp = _handler.topData["Rlocation"];
                        return " CMP='" + cmp + "'";
                    }
                }
            });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var cust_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyTypeNoAuto(groupId, undefined, $MainGrid,
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

    function getchg(name) {
        var _name = name;
        var chg_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.ChgUrl1,
                config: LookUpConfig.ChgLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "ChgDescp", map.ChgDescp, null);
                    setGridVal($grid, selRowId, "ChgType", map.ChgType, null);
                    setGridVal($grid, selRowId, "Repay", map.Repay, null);
                    setGridVal($grid, selRowId, "ChgCd", map.ChgCd, "lookup");
                    return map.ChgCd;
                }
            }, GetChgAuto1(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, null);
                setGridVal($grid, rowid, "Repay", rd.REPAY, null);
                setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                $(elem).val(rd.CHG_CD);
            }, function () {
                var tranType = $("#TranType").val();
                var inType = $("#OutIn").val();
                return "IO_TYPE=" + inType + "&TRAN_MODE@" + tranType + ";O";
                //return " TRAN_MODE IN ('" + tranType + "','O')";
            },undefined,function($grid, elem, rowid){
                setGridVal($grid, rowid, "ChgCd", "", "lookup");
                setGridVal($grid, rowid, "ChgDescp", "", null);
                setGridVal($grid, rowid, "Repay", "", null);
                setGridVal($grid, rowid, "ChgType", "", null);
            }
            ), {
                param: "",
                baseConditionFunc: function () {
                    var tranType = $("#TranType").val();
                    var inType = $("#OutIn").val();
                    return "IO_TYPE='" + inType + "' AND TRAN_MODE IN ('" + tranType + "','O')";
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
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Cur", map.Cur, "lookup");
                    return map.Cur;
                }
            }, LookUpConfig.GetCurAuto(groupId, "", $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
                $(elem).val(rd.CUR);
            }), { param: "" });
        return cur_op;
    }

    function getUnit(name) {
        var _name = name;
        var unit_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            }), { param: "" });
        return unit_op;
    }

    function getbscode(name) {
        var _name = name;
        var bscode_op = getLookupOp("MainGrid",
            {
                url: rootPath + "TPVCommon/GetTCARDataForLookup",
                config: LookUpConfig.TcarLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "CarrierCode", map.Cd, "lookup");
                    setGridVal($grid, selRowId, "CarrierName", map.CdDescp, "lookup");
                    return map.Cd;
                },
                params: ""
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", $MainGrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, "CarrierCode", rd.CD, "lookup");
                setGridVal($grid, selRowId, "CarrierName", rd.CD_DESCP, "lookup");
                $(elem).val(rd.CD);
                $(elem).val(rd.CD_DESCP);
            }), { param: "" });
        return bscode_op;
    }

    var thisVenderCd = "";
    var thisLocation = "";
    if(typeof MainData == "object")
    {
        thisVenderCd = MainData[0]["LspCd"];
        thisLocation = MainData[0]["Cmp"];
    }
    else
    {
        thisVenderCd = _LspCd;
        thisLocation = Location;
    }


    $.ajax({
        async: true,
        url: rootPath + "IQTManage/GetTransTypeInfo",
        type: 'POST',
        data: { VenderCd: thisVenderCd, Cmp: thisLocation },
        async: false,
        "complete": function (xmlHttpRequest, successMsg) {
            //CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            //CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {

            /*function getIoFlag_O() {
                if (ioflag == "O")
                    return true;
                else
                    return false; 
            }*/

            function getIoFlag() {
                if (ioflag == "O")
                    return false;
                else
                    return true;
            }

            var colModel = [
                { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: true, hidden: true },
                { name: 'RfqNo', title: _getLang('L_RQQuery_RfqNo', 'enquiry NO.'), index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
                { name: 'AllIn', title: _getLang('L_AirQuery_AllIn', 'All in'), index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
                { name: 'TranMode', title: _getLang('L_RQQuery_TranMode', 'Transport type'), index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
                { name: 'TranType', title: _getLang('L_UserSetUp_TranType', 'Transportation Type'), index: 'TranType', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: 'R:Railway;S:Ocean Shipping;A:Air;T:Truck;I:Intermodal' }, edittype: 'select' },
                //{ name: 'Region', title: _getLang('L_BaseLookup_Region', index: 'Region', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_regionop("Region")), width: 80, hidden: false, editable: true },
                //{ name: 'TranType', title: _getLang('L_UserSetUp_TranType', index: 'TranType', editable: true, width: 80, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: select_trantype }, edittype: 'select' },
                { name: 'PolCd', title: _getLang('L_BaseLookup_PolCd', 'POL'), index: 'PolCd', sorttype: 'string', width: 30, edittype: 'custom', editoptions: gridLookup(getcityop("PolCd")), width: 80, hidden: false, editable: getIoFlag() },
                { name: 'PolNm', title: _getLang('L_BaseLookup_PolName', 'POL Name'), index: 'PolNm', sorttype: 'string', width: 80, hidden: false, editable: false },
                //{ name: 'ViaCd', title: 'Via', index: 'ViaCd', sorttype: 'string', width: 80, sorttype: 'string', width: 120, hidden: false, editable: false },
                { name: 'PodCd', title: _getLang('L_BaseLookup_PodCd', 'POD'), index: 'PodCd', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(getcityop("PodCd")), width: 80, hidden: false, editable: getIoFlag() },
                { name: 'PodNm', title: _getLang('L_BaseLookup_PodName', 'POD Name'), index: 'PodNm', sorttype: 'string', width: 100, hidden: false, editable: false },
                { name: 'BackLocation', title: 'Empty return location', index: 'BackLocation', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(getcityop("BackLocation")), width: 80, hidden: false, editable: getIoFlag() },
                { name: 'BackLocationNm', title: 'Empty return location name', index: 'BackLocationNm', sorttype: 'string', width: 100, hidden: false, editable: false },
                { name: 'CarrierCode', title: 'Carrier Code', index: 'CarrierCode', sorttype: 'string', width: 90, hidden: false, editable: getIoFlag(), edittype: 'custom', editoptions: gridLookup(getbscode("CarrierCode")) },
                { name: 'CarrierName', title: 'Carrier Name', index: 'CarrierName', sorttype: 'string', width: 150, hidden: true, editable: true },
                { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', 'Charge CD'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: getIoFlag() },
                { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', 'Description'), index: 'ChgDescp', sorttype: 'string', width: 150, hidden: false, editable: false },
                { name: 'ChgType', title: _getLang('L_SMCHGSetup_ChgType', 'Charge Type'), index: 'ChgType', sorttype: 'string', width: 125, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
                { name: 'Repay', title: _getLang('L_QTSetup_chgCd', 'Cost classify'), index: 'Repay', sorttype: 'string', width: 150, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _getLang('L_FCLChgSetup_Script_166', '') }, edittype: 'select' },
                { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), index: 'Cur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: true },
                { name: 'Tt', title: 'T/T(H)', index: 'Tt', width: 60, align: 'right', formatter: 'integer', hidden: false, editable: true },
                { name: 'MinAmt', title: 'Min amount', index: 'MinAmt', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
                { name: 'F70', title: _getLang('L_FCLFSetup_F3', 'Cost'), index: 'F70', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
                { name: 'SeqNo', title: _getLang('L_NRSSetup_SeqNo', 'Serial number'), index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: true },
            ];
            if (result.message == "success") {
                var transTypeCols = result.chgTypeStr.split(";");
                var transTypeColsN = result.chgTypeColsStr.split(";");

                //var colNames = ["rn", jslang["L_RQQuery_RfqNo"], jslang["L_AirQuery_AllIn"], jslang["L_RQQuery_TranMode"], jslang["L_SMCHGSetup_ChgCd"], jslang["L_BaseLookup_Region"], jslang["L_BaseLookup_PolCd"], jslang["L_BaseLookup_PolName"], "Via",jslang["L_BaseLookup_PodCd"],jslang["L_BaseLookup_PodName"],jslang["L_DNApproveManage_CaCd"],'Loading From','Loading To',jslang["L_IpPart_Crncy"],jslang["“L_RouteSetup_Tt”"]]; //grid.jqGrid("getGridParam", "colNames");
                $.each(transTypeCols, function (index, val) {
                    colModel.push({ name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, editable: true, hidden: false });
                    //colNames.push(val);
                });
            }

            colModel.push({ name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', align: 'left', width: 120, hidden: true });
            colModel.push({ name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } });
            var ignoD = { "UId": true, "RfqNo": true, "SeqNo": true, "QuotNo": true };
            for (var i = 0; i < colModel.length; i++) {
                if (!ignoD[colModel[i].name])
                    _fields.push(colModel[i].name);
            } 
            _handler.intiGrid("MainGrid", $MainGrid, {
                multiselect: true,
                colModel: colModel, caption: _getLang("L_QuotTrailerSetup_Scripts_316", "Trailer fee quotation information"), delKey: ["UId"],
                onAddRowFunc: function (rowid) {
                    var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
                    if (typeof maxSeqNo === "undefined")
                        maxSeqNo = 0;
                    //setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
                    setDefutltGridData($MainGrid, rowid, { "PolCd": true, "PodCd": true, "TranMode": true });
                    //setGridVal($MainGrid, rowid, "ChgCd", "FRT", "lookup");
                },
                beforeSelectRowFunc: function (rowid) {
                    //main key 修改時不允與修改 
                },
                beforeAddRowFunc: function (rowid) {
                    //add row 時要可以編輯main key
                    //$SubGrid.setColProp('DocType', { editable: true });
                }
            });

        }
    });
}

var lookUpConfig = {
    caption: _getLang("L_MenuBar_Search", "search"),
    sortname: "CreateDate",
    refresh: false,
    columns: [
            { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        { name: 'QuotNo', title: _getLang('L_QTQuery_QuotNo', 'Quotation NO.'), index: 'QuotNo', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'QuotDateL', title: _getLang('L_QTQuery_QuotDate', 'Quotation date'), index: 'QuotDateL', editable: false, formatter: 'date', editoptions: myEditDateInit, formatoptions: { srcformat: 'ISO8601Long', newformat: 'Y-m-d', defaultValue: null }, width: 130, align: 'left', sorttype: 'string', hidden: false },
        { name: 'RlocationNm', title: _getLang('L_RQQuery_Rlocation', 'Factory'), index: 'RlocationNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'LspNm', title: _getLang('L_AirSetup_LspCd', 'Company'), index: 'LspNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'EffectFrom', title: _getLang('L_RQQuery_EffectFrom', 'Effective date'), index: 'EffectFrom', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } },
        { name: 'EffectTo', title: _getLang('L_RQQuery_EffectTo', 'Expiration date'), index: 'EffectTo', width: 120, align: 'left', sorttype: 'string', hidden: false, formatter: 'date', formatoptions: { newformat: 'Y-m-d' } }
    ]
}

$(function () {
    intQtView();
    //initQTGrid();

     initQTGrid();
    _handler.inquiryUrl = rootPath + "QTManage/GetFCLCHGData";
    _handler.config = lookUpConfig;

    _handler.afterEdit = function () {
        if (ioflag == "O") {
            $MainGrid.jqGrid('getGridParam', "removeAddRowButton")("MainGrid");
            $MainGrid.jqGrid('getGridParam', "removeDelRowButton")("MainGrid");
            $("#gridMenu").attr("id", "_gridMenu");//右击隐藏
            //$("#_gridMenu").attr("id", "gridMenu");//右击显示
        }
        if (!isEmpty($("#QuotNo").val())) {
            $("#QuotNo").attr('disabled', true);
        }
        if (!isEmpty($("#QuotDateL").val())) {
            $("#QuotDateL").attr('disabled', true);
            $("#QuotDateL").parent().find("button").attr('disabled', true);
        }
        else {
            $("#QuotDateL").val(getDate());
        }
        //$("#Rlocation").parent().find("button").attr('disabled', true);
        if (!isEmpty(_handler.topData["LspCd"])) {
            $("#LspCd").attr('disabled', true);
            $("#LspCd").parent().find("button").attr('disabled', true);
        }
    }


    _handler.beforLoadView = function () {
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        var requires = ["QuotNo", "EffectFrom", "EffectTo", "Rlocation", "Cur", "TranType", "LspCd"];
        var readonlys = ["QuotDateL", "RlocationNm", "LspNm", "QuotType"];

        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
            $("#" + readonlys[i]).parent().find("button").attr('disabled', false);
        }

        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data;
        if (ioflag == "O")
        {
            data = { "TranMode": "C", "TranType": "F", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "PP", LspCd: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        else
        {
            data = { "TranMode": "C", "TranType": "F", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "PP", Rlocation: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#Rlocation").blur();
        $("#LspCd").blur();
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");
        getNowDate();
        MenuBarFuncArr.EndFunc = function () {
            if (ioflag == "O") {
                $("#LspCd").attr('disabled', true);
                $("#LspCdLookup").attr('disabled', true);
                $("#LspCd").attr('readonly', 'readonly');
                $("#LspCdLookup").attr('readonly', 'readonly');
            }
        }
    }

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "PolCd", index: 8, text: 'POL' });
        nullCols.push({ name: "PodCd", index: 10, text: 'POD' });
        nullCols.push({ name: "ChgCd", index: 12, text: _getLang('L_SMCHGSetup_ChgDescp', 'Description') });
        nullCols.push({ name: "ChgType", index: 18, text: _getLang('L_QTSetup_chgCd', 'Cost classify') });
        nullCols.push({ name: "Cur", index: 16, text: _getLang('L_IpPart_Crncy', 'Currency') });
        var result = _handler.checkData($MainGrid, nullCols, sameCols);
        if (result === false) return false;
        var rowIds = $MainGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            $MainGrid.jqGrid('setCell', rowIds[i], "SeqNo", i + 1);
        }
        //return true;
        return checkDoubleData($MainGrid);
    }

    function checkDoubleData($grid) {
        var rowIds = $grid.getDataIDs();
        var checkKey = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
            var key = rowDatas["PolCd"] + "#" + rowDatas["PodCd"] + "#" + rowDatas["ChgCd"] + "#" + rowDatas["ChgType"];
            for (var x = 0; x < checkKey.length; x++) {
                if (key === checkKey[x]) {
                    try {
                        $grid.jqGrid("editCell", rowIds[i], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", _getLang("L_QuotTrailerSetup_Script_175", " POL,POD,charge cost,cost category  already exists"), 2000, "warning");
                    return false;
                }
            }
            for (var j = 0; j < rowIds.length; j++) {
                if (rowIds[i] === rowIds[j])
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                var key1 = rowDatas1["Carrier"] + "#" + rowDatas1["PodCd"] + "#" + rowDatas1["ChgCd"];
                if (key1 === key) {
                    try {
                        $grid.jqGrid("editCell", rowIds[j], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", _getLang("L_QuotTrailerSetup_Script_175", " POL,POD,charge cost,cost category  already exists"), 2000, "warning");
                    return false;
                }
            }
        }
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值

        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        //data["term"] = encodeURIComponent($("#Incoterm").val());
        var rowIds = $MainGrid.getDataIDs();
        var seqjson = {};
        for (var i = 0; i < rowIds.length; i++) {
            var uid = $MainGrid.jqGrid('getCell', rowIds[i], "UId");
            if (!isEmpty(uid)) {
                var seqno = $MainGrid.jqGrid('getCell', rowIds[i], "SeqNo");
                seqjson[uid] = seqno;
            }
        }
        data["seqquery"] = JSON.stringify(seqjson);
        //data["mode"] = "1";
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


        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

        //initQTGrid(data["main"]);
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Rlocation"], "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
        MenuBarFuncArr.Enabled(["VoidBtn"]);
        MenuBarFuncArr.Enabled(["ApproveBtn"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
        MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["IQQT"]);
        setRQData(data);
    }

    registBtnLookup($("#LspCdLookup"), {
        item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspCd").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    var GetSiteCmpAuto = function (groupId, $grid, autoFn, clearFn) {
        var op =
        {
            autoCompDt: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=",
            autoCompParams: "CMP=showValue,CMP,NAME",
            autoCompFunc: function (elem, event, ui, rowid) {
                autoFn($grid, ui.item.returnValue, elem, rowid);
            },
            autoClearFunc: function (elem, event, rowid) {
                clearFn($grid, elem, rowid);
            }
        };
        return op;
    }
    registBtnLookup($("#RlocationLookup"), {
        item: "#Rlocation", url: rootPath + "TPVCommon/GetSiteCmpData", config: LookUpConfig.SiteLookup, param: "", selectRowFn: function (map) {
            $("#Rlocation").val(map.Cd);
            $("#RlocationNm").val(map.CdDescp);
        }
    }, { baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'" }, GetSiteCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Rlocation").val(rd.CMP);
        $("#RlocationNm").val(rd.NAME);
    }, function ($grid, elem, rowid) {
        $("#Rlocation").val("");
        $("#RlocationNm").val("");
        }));

    registBtnLookup($("#SharedToLookup"), {
        item: "#SharedTo", url: rootPath + "TPVCommon/GetSiteCmpData", config: LookUpConfig.SiteLookup, param: "", selectRowFn: function (map) {
            $("#SharedTo").val(map.Cd);
            $("#SharedNm").val(map.CdDescp);
        }
    }, { baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'" }, GetSiteCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#SharedTo").val(rd.CMP);
        $("#SharedNm").val(rd.NAME);
    }, function ($grid, elem, rowid) {
        $("#SharedTo").val("");
        $("#SharedNm").val("");
    }));

    //registBtnLookup($("#RlocationLookup"), {
    //    item: '#Rlocation', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#Rlocation").val(map.PartyNo);
    //        $("#RlocationNm").val(map.PartyName);
    //    }
    //}, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#RlocationNm").val(rd.PARTY_NAME);
    //}));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        getNowDate();
        $("#EffectFrom").val(getDate(0, "-"));
        $("#EffectTo").val(getDate(365, "-"));
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");
        //_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#ApproveBy").val("");
        $("#ApproveDateL").val("");
        $("#QuotType").val("P");
        $("#" + _handler.key).val(uuid());
        var dataRow, addData = [];
        var rowIds = $MainGrid.getDataIDs();
        var fields = [];
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            dataRow = {};
            for (var y = 0; y < _fields.length; y++) {
                dataRow[_fields[y]] = rowDatas[_fields[y]];
            } 
            addData.push(dataRow);
        }
        console.log(addData);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }

        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }
    //getSelectOptions();
    loadQtView(2);
    MenuBarFuncArr.DelMenu(["MBSearch"]);

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        if (_handler.topData["QuotType"] === "A") {
            CommonFunc.Notify("", _getLang("L_QTSetup_Verified", "approved"), 500, "warning");
            return false;
        }
        if (_handler.topData["QuotType"] === "Q") {
            CommonFunc.Notify("", _getLang("L_RQSetup_Quoted", "Offer already"), 500, "warning");
            return false;
        }
        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", _getLang("L_FCLChgSetup_Scripts_302", "No rights to delete other folks quotaiton"), 500, "warning");
            return false;
        }
    }

    if (isa == "Y") {
        MenuBarFuncArr.DelMenu(["MBEdit", "EXCEL_BTN", "MBCopy", "MBSave", "MBCancel", "MBAdd", "MBDel", "VoidBtn", "QuotTypeBtn", "MBPreview"]);
        MenuBarFuncArr.Enabled(["btn03", "btn04", "btn06"]);
    }

});

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}