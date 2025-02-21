var $MainGrid;
var $TruckMainGrid;
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
}

function initQTGrid(MainData) {
    $MainGrid = $("#MainGrid");
    function getcityop(name) {
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

    function getTruckPort(name) {
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
            }), { param: "" });
        return city_op;
    }

    function getMcust(name) {
        var _name = name;
        var op = getLookupOpNew("MainGrid",
            {
                url: rootPath + LookUpConfig.RCARUrl,
                config: LookUpConfig.BSCodeLookup,
                autoCompDt: ConditionParam("", "Cd", "", "bw"),
                selectRowFn: function (map, $grid, selRowId) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, _name, map.Cd, "lookup");
                    return map.Cd;
                },
                autoClearFunc: function (item, event, selRowId) {
                    var selRowId = $MainGrid.jqGrid('getGridParam', 'selrow');
                    setGridVal($MainGrid, selRowId, _name, "", "lookup");
                }, baseConditionFunc: function () {
                    var tranType = $("#TranType").val();
                    if (tranType == "R") {
                        return ConditionParam("", "CdType", "RCAR", "eq");
                    }
                    return ConditionParam("", "CdType", "TCAR", "eq");
                }
            });
        return op;
    }

    function getpartyop() { 
        var party_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (returnObj, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "SpCd", returnObj.PartyNo, 'lookup');
                    setGridVal($grid, selRowId, "SpNm", returnObj.PartyName3, null);
                    return returnObj.PartyNo;
                }
            }, {
            baseConditionFunc: function () { 
                    return " (PARTY_TYPE LIKE 'SP;%' OR PARTY_TYPE LIKE'%;SP%' OR PARTY_TYPE='SP')"
            }
        }, LookUpConfig.GetPartyNoAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) { 
                setGridVal($grid, rowid, "SpCd", rd.PARTY_NO, 'lookup');
                setGridVal($grid, rowid, "SpNm", rd.PARTY_NAME3, null);
            }, function ($grid, elem, rowid) {
                var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                setGridVal($grid, selRowId, "SpCd", "", 'lookup');
                setGridVal($grid, selRowId, "SpNm", "", null);
            })
        );
        party_op.param = ""; 
        return party_op;
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
                //return "&sopt_TranMode=in&TranMode=O;" + tranType;
                //return " TRAN_MODE IN ('" + tranType + "','O')";
                return "IO_TYPE=" + inType+ "&TRAN_MODE@" + tranType+";O";
            }), {
                param: "",
                baseConditionFunc: function () {
                    var tranType = $("#TranType").val();
                    var inType = $("#OutIn").val();
                    return "IO_TYPE='" + inType+"' AND TRAN_MODE IN ('" + tranType + "','O')";
                }
            },function($grid, elem, rowid){
                setGridVal($grid, rowid, "ChgCd", "", "lookup");
                setGridVal($grid, rowid, "ChgDescp", "", null);
                setGridVal($grid, rowid, "Repay", "", null);
                setGridVal($grid, rowid, "ChgType", "", null);
            }
            );
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

    function getsmpty(name) {
        var _name = name;
        var smpty_op = getLookupOp("MainGrid",
            {
                url: rootPath + "TPVCommon/GetSmptyDataForLookup",
                config: LookUpConfig.SmptyLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "TerminalAgentNm", map.PartyName, "lookup");
                    setGridVal($grid, selRowId, "TerminalAgentNo", map.PartyNo, "lookup");
                    return map.PartyName;
                },
                params: ""
            }, LookUpConfig.GetSmptyAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                var selRowId = rowid;
                setGridVal($grid, selRowId, "TerminalAgentNm", rd.PARTY_NAME, "lookup");
                setGridVal($grid, selRowId, "TerminalAgentNo", rd.PARTY_NO, "lookup");
                $(elem).val(rd.PARTY_NAME);
            }), { param: "" });
        return smpty_op;
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
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        //{ name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        //{ name: 'TranType', title: 'TranType',init:true, index: 'TranType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'A:A.Air;L:L.LCL;F:F.FCL;R:R.RailWay;D:D.Inland Express;T:T.Truck;E:E.Express' }, edittype: 'select' },
        { name: 'PodCd', title: 'POD', index: 'PodCd', editoptions: gridLookup(getcityop("PodCd")), edittype: 'custom', sorttype: 'string', width: 50, hidden: false, editable: getIoFlag() },
        { name: 'BackLocation', title: 'Empty return location', index: 'BackLocation', sorttype: 'string', width: 50, edittype: 'custom', editoptions: gridLookup(getTruckPort("BackLocation")), width: 80, hidden: false, editable: getIoFlag() },
        //{ name: 'BackLocationNm', title: 'Empty return location name', index: 'BackLocationNm', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd','Charge CD'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: getIoFlag() },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', 'Description'), index: 'ChgDescp', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'ChgType', title: _getLang('L_SMCHGSetup_ChgType', 'Charge Type'), index: 'ChgType', sorttype: 'string', width: 125, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: _getLang('L_QTSetup_chgCd', 'Cost classify'), index: 'Repay', sorttype: 'string', width: 100, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _getLang('L_FCLChgSetup_Script_166', '') }, edittype: 'select' },
        //{ name: 'Punit', title: _getLang('L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Punit', title: _getLang('L_AirQuery_Punit', 'Chargable Unit'), index: 'Punit', sorttype: 'string', width: 140, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },
        { name: 'SpCd', title: 'Forwarder', index: 'SpCd', edittype: 'custom', editoptions: gridLookup(getpartyop()), sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'SpNm', title: 'Forwarder Name', index: 'SpNm', sorttype: 'string', width: 120, hidden: false, editable: false }, 
        { name: 'Carrier', title: _getLang('L_DNApproveManage_CaCd', 'Carrier'), index: 'Carrier', edittype: 'custom', editoptions: gridLookup(getMcust("Carrier")), sorttype: 'string', width: 70, hidden: false, editable: true },
        { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 80, align: 'left', hidden: false, editable: true },
        { name: 'MinAmt', title: 'Min amount', index: 'MinAmt', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'MaxAmt', title: 'Max amount', index: 'MaxAmt', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F3', title: _getLang('L_FCLFSetup_F3', 'Cost'), index: 'F3', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F4', title: '20GP', index: 'F4', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F5', title: '40GP', index: 'F5', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F6', title: '40HQ', index: 'F6', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'IsShare', title: _getLang('L_QTManage_IsShare', 'went dutch or not'), index: 'IsShare', sorttype: 'string', width: 70, hidden: false, editable: true, formatter: "select", editoptions: { value: ': ;Y:Y' }, edittype: 'select' },
        //{ name: 'F4', title: '20GP', index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        //{ name: 'F5', title: '40GP', index: 'F5', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        //{ name: 'F6', title: '40HQ', index: 'F6', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        { name: 'TerminalAgentNo', title: 'Terminal Agent No', index: 'TerminalAgentNo', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(getsmpty("TerminalAgentNo")) },
        { name: 'TerminalAgentNm', title: 'Terminal Agent', index: 'TerminalAgentNm', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true },
        //{ name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: _getLang('L_NRSSetup_SeqNo', 'Serial number'), index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'QuotNo', title: _getLang('L_QTQuery_QuotNo', 'Quotation NO.'), index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];
    var ignoD = { "UId": true, "RfqNo": true, "SeqNo": true, "QuotNo": true };
    for (var i = 0; i < colModel.length; i++) {
        if (!ignoD[colModel[i].name])
            _fields.push(colModel[i].name);
    }

    var thisVenderCd = "";
    var thisLocation = "";
    if (typeof MainData == "object") {
        thisVenderCd = MainData[0]["LspCd"];
        thisLocation = MainData[0]["Cmp"];
    }
    else {
        thisVenderCd = _LspCd;
        thisLocation = cmp;
    }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: false,
            url: rootPath + "IQTManage/GetLocalColumInfo",
            type: 'POST',
            data: { VenderCd: thisVenderCd, Cmp: thisLocation },
            async: false,
            beforeSend: function () {
               
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message == "success") {
                    var transTypeCols = result.chgTypeStr.split(";");
                    var transTypeColsN = result.chgTypeColsStr.split(";");
                    if (transTypeCols.length <= 0) {
                        return;
                    }

                    var f_cols = {};
                    for (var i = 9; i < 100; i++) {
                        f_cols["F" + i] = "Y";
                    }
                    var f_start = 0;
                    for (var i = 0; i < colModel.length; i++) {
                        if (f_cols[colModel[i].name] === "Y") {
                            if (f_start == 0)
                                f_start = i;
                            colModel.splice(i, 1);
                            i--;
                        }
                    }

                    $.each(transTypeCols, function (index, val) {
                        var item = { name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false };
                        if (f_start > 0) {
                            colModel.splice(f_start, 0);
                            colModel.push(item);
                        }
                        else
                            colModel.push(item);
                        f_start++;
                    });

                    _handler.grids = [];
                    $("#MainGrid").jqGrid("GridDestroy");
                    $("#grid_div").append('<table id="MainGrid" class="_tableGrid" style="width: 100%"><tr></tr></table>');
                    $MainGrid = $("#MainGrid");
                }
            }
        });

    colModel = SetColModel(FMOption, colModel, 24);

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: _getLang('L_QuotLocalSetup_Scripts_local314', 'Local Charge Quotation Information'), delKey: ["UId"], height: 300,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($MainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($MainGrid, rowid, { "PodCd": true, "Cur": true });
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });
}

function initQTTruckGrid(MainData) {
    $TruckMainGrid = $("#TruckMainGrid");
    function getcityop(name) {
        var _name = name;
        var city_op = getLookupOp("TruckMainGrid",
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
            }, LookUpConfig.GetCityPortAuto(groupId, TruckMainGrid,
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

    function getTruckPort(name) {
        var _name = name;
        var city_op = getLookupOp("TruckMainGrid",
            {
                url: rootPath + LookUpConfig.TruckPortCdUrl,
                config: LookUpConfig.TruckPortCdLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    if (_name === "PolCd")
                        setGridVal($grid, selRowId, "PolNm", map.PortNm, null);
                    else if (_name === "PodCd")
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    else
                        setGridVal($grid, selRowId, "BackLocationNm", map.PortNm, null);

                    setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return map.PortCd;
                }
            }, LookUpConfig.TruckPortCdAuto(groupId, $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    if (_name === "PolCd")
                        setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                    else if (_name === "PodCd")
                        setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                    else
                        setGridVal($grid, rowid, "BackLocationNm", rd.PORT_NM, null);

                    $(elem).val(rd.PORT_CD);
                    setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
                }), { param: "" });
        return city_op;
    }

    function getcust(name) {
        var _name = name;
        var unit_op = getLookupOp("TruckMainGrid",
            {
                url: rootPath + LookUpConfig.TCARUrl,
                config: LookUpConfig.BSCodeLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, _name, map.Cd, "lookup");
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "TCAR", $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, _name, rd.CD, "lookup");
                    $(elem).val(rd.CD);
                }));
        return unit_op;
    }

    function getchg(name) {
        var _name = name;
        var chg_op = getLookupOp("TruckMainGrid",
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
            }, GetChgAuto1(groupId, undefined, $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "ChgDescp", rd.CHG_DESCP, null);
                    setGridVal($grid, rowid, "ChgType", rd.CHG_TYPE, null);
                    setGridVal($grid, rowid, "Repay", rd.REPAY, null);
                    setGridVal($grid, rowid, "ChgCd", rd.CHG_CD, "lookup");
                    $(elem).val(rd.CHG_CD);
                }, function () {
                    var tranType = $("#TranType").val();
                    var inType = $("#OutIn").val();
                    //return "&sopt_TranMode=in&TranMode=O;" + tranType;
                    //return " TRAN_MODE IN ('" + tranType + "','O')";
                    return "TRAN_MODE@@" + tranType + ";O";
                }), {
            param: "",
            baseConditionFunc: function () {
                var tranType = $("#TranType").val();
                var inType = $("#OutIn").val();
                return "IO_TYPE='" + inType + "' AND TRAN_MODE IN ('" + tranType + "','O')";
            }
        }, function ($grid, elem, rowid) {
            setGridVal($grid, rowid, "ChgCd", "", "lookup");
            setGridVal($grid, rowid, "ChgDescp", "", null);
            setGridVal($grid, rowid, "Repay", "", null);
            setGridVal($grid, rowid, "ChgType", "", null);
        }
        );
        return chg_op;
    }

    function getcur(name) {
        var _name = name;
        var cur_op = getLookupOp("TruckMainGrid",
            {
                url: rootPath + LookUpConfig.CurUrl,
                config: LookUpConfig.CurLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "Cur", map.Cur, "lookup");
                    return map.Cur;
                }
            }, LookUpConfig.GetCurAuto(groupId, "", $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    setGridVal($grid, rowid, "Cur", rd.CUR, "lookup");
                    $(elem).val(rd.CUR);
                }), { param: "" });
        return cur_op;
    }

    function getsmpty(name) {
        var _name = name;
        var smpty_op = getLookupOp("TruckMainGrid",
            {
                url: rootPath + "TPVCommon/GetSmptyDataForLookup",
                config: LookUpConfig.SmptyLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "TerminalAgentNm", map.PartyName, "lookup");
                    setGridVal($grid, selRowId, "TerminalAgentNo", map.PartyNo, "lookup");
                    return map.PartyName;
                },
                params: ""
            }, LookUpConfig.GetSmptyAuto(groupId, $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    var selRowId = rowid;
                    setGridVal($grid, selRowId, "TerminalAgentNm", rd.PARTY_NAME, "lookup");
                    setGridVal($grid, selRowId, "TerminalAgentNo", rd.PARTY_NO, "lookup");
                    $(elem).val(rd.PARTY_NAME);
                }), { param: "" });
        return smpty_op;
    }

    function getUnit(name) {
        var _name = name;
        var unit_op = getLookupOp("TruckMainGrid",
            {
                url: rootPath + LookUpConfig.QtyuUrl,
                config: LookUpConfig.QtyuLookup,
                returnFn: function (map, $grid) {
                    return map.Cd;
                }
            }, LookUpConfig.GetCodeTypeAuto(groupId, "UB", $TruckMainGrid,
                function ($grid, rd, elem, rowid) {
                    $(elem).val(rd.CD);
                }), { param: "" });
        return unit_op;
    }

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
        { name: 'RfqNo', title: 'RfqNo', index: 'RfqNo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LspCd', title: 'LspCd', index: 'LspCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'AllIn', title: 'AllIn', index: 'AllIn', sorttype: 'string', width: 100, editable: true, hidden: true },
        //{ name: 'PodCd', title: 'PodCd', index: 'PodCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolCd', title: 'PolCd', index: 'PolCd', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PolNm', title: 'PolNm', index: 'PolNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'PodNm', title: 'PodNm', index: 'PodNm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        //{ name: 'TranType', title: 'TranType',init:true, index: 'TranType', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", editoptions: { value: 'A:A.Air;L:L.LCL;F:F.FCL;R:R.RailWay;D:D.Inland Express;T:T.Truck;E:E.Express' }, edittype: 'select' },
        { name: 'PodCd', title: 'POD', index: 'PodCd', editoptions: gridLookup(getTruckPort("PodCd")), edittype: 'custom', sorttype: 'string', width: 50, hidden: false, editable: getIoFlag() },
        { name: 'BackLocation', title: 'Empty return location', index: 'BackLocation', sorttype: 'string', width: 50, edittype: 'custom', editoptions: gridLookup(getTruckPort("BackLocation")), width: 80, hidden: false, editable: getIoFlag() },
        //{ name: 'BackLocationNm', title: 'Empty return location name', index: 'BackLocationNm', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', 'Charge CD'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: getIoFlag() },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', 'Description'), index: 'ChgDescp', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'ChgType', title: _getLang('L_SMCHGSetup_ChgType', 'Charge Type'), index: 'ChgType', sorttype: 'string', width: 125, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: _getLang('L_QTSetup_chgCd', 'Cost classify'), index: 'Repay', sorttype: 'string', width: 100, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _getLang('L_FCLChgSetup_Script_166','') }, edittype: 'select' },
        //{ name: 'Punit', title: _getLang('L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Punit', title: _getLang('L_AirQuery_Punit', 'Chargable Unit'), index: 'Punit', sorttype: 'string', width: 140, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },
        { name: 'Carrier', title: _getLang('L_DNApproveManage_CaCd', 'Carrier'), index: 'Carrier', edittype: 'custom', editoptions: gridLookup(getcust("Carrier")), sorttype: 'string', width: 70, hidden: false, editable: true },
        { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 80, align: 'left', hidden: false, editable: true },
        { name: 'MinAmt', title: 'Min amount', index: 'MinAmt', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'MaxAmt', title: 'Max amount', index: 'MaxAmt', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F3', title: _getLang('L_FCLFSetup_F3', 'Cost'), index: 'F3', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F4', title: '20GP', index: 'F4', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F5', title: '40GP', index: 'F5', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'F6', title: '40HQ', index: 'F6', width: 75, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        { name: 'IsShare', title: _getLang('L_QTManage_IsShare', 'went dutch or not'), index: 'IsShare', sorttype: 'string', width: 70, hidden: false, editable: true, formatter: "select", editoptions: { value: ': ;Y:Y' }, edittype: 'select' },
        //{ name: 'F4', title: '20GP', index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        //{ name: 'F5', title: '40GP', index: 'F5', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        //{ name: 'F6', title: '40HQ', index: 'F6', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
        { name: 'TerminalAgentNo', title: 'Terminal Agent No', index: 'TerminalAgentNo', width: 70, align: 'left', sorttype: 'string', hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(getsmpty("TerminalAgentNo")) },
        { name: 'TerminalAgentNm', title: 'Terminal Agent', index: 'TerminalAgentNm', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true },
        //{ name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingFrom', title: 'Loading From', index: 'LoadingFrom', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'LoadingTo', title: 'Loading To', index: 'LoadingTo', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'SeqNo', title: _getLang('L_NRSSetup_SeqNo', 'Serial number'), index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'QuotNo', title: _getLang('L_QTQuery_QuotNo', 'Quotation NO.'), index: 'QuotNo', sorttype: 'string', width: 100, editable: false, hidden: true }
    ];
    var ignoD = { "UId": true, "RfqNo": true, "SeqNo": true, "QuotNo": true };
    for (var i = 0; i < colModel.length; i++) {
        if (!ignoD[colModel[i].name])
            _fields.push(colModel[i].name);
    }

    var thisVenderCd = "";
    var thisLocation = "";
    if (typeof MainData == "object") {
        thisVenderCd = MainData[0]["LspCd"];
        thisLocation = MainData[0]["Cmp"];
    }
    else {
        thisVenderCd = _LspCd;
        thisLocation = cmp;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: false,
        url: rootPath + "IQTManage/GetLocalColumInfo",
        type: 'POST',
        data: { VenderCd: thisVenderCd, Cmp: thisLocation },
        async: false,
        beforeSend: function () {

        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            if (result.message == "success") {
                var transTypeCols = result.chgTypeStr.split(";");
                var transTypeColsN = result.chgTypeColsStr.split(";");
                if (transTypeCols.length <= 0) {
                    return;
                }

                var f_cols = {};
                for (var i = 9; i < 100; i++) {
                    f_cols["F" + i] = "Y";
                }
                var f_start = 0;
                for (var i = 0; i < colModel.length; i++) {
                    if (f_cols[colModel[i].name] === "Y") {
                        if (f_start == 0)
                            f_start = i;
                        colModel.splice(i, 1);
                        i--;
                    }
                }

                $.each(transTypeCols, function (index, val) {
                    var item = { name: transTypeColsN[index], title: val, index: transTypeColsN[index], sorttype: 'float', width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false };
                    if (f_start > 0) {
                        colModel.splice(f_start, 0);
                        colModel.push(item);
                    }
                    else
                        colModel.push(item);
                    f_start++;
                });

                _handler.grids = [];
                $("#TruckMainGrid").jqGrid("GridDestroy");
                $("#grid_div").append('<table id="TruckMainGrid" class="_tableGrid" style="width: 100%"><tr></tr></table>');
                $TruckMainGrid = $("#TruckMainGrid");
            }
        }
    });

    colModel = SetColModel(FMOption, colModel, 24);

    _handler.intiGrid("TruckMainGrid", $TruckMainGrid, {
        colModel: colModel, caption: _getLang('L_QuotLocalSetup_Scripts_local314', 'Local Charge Quotation Information'), delKey: ["UId"], height: 300,
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $TruckMainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            setGridVal($TruckMainGrid, rowid, "SeqNo", maxSeqNo + 1);
            setDefutltGridData($TruckMainGrid, rowid, { "PodCd": true, "Cur": true });
        },
        beforeSelectRowFunc: function (rowid) {
        },
        beforeAddRowFunc: function (rowid) {
        }
    });
}


var lookUpConfig = {
    caption: _getLang("L_MenuBar_Search", ""),
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

$(function() {
    SetCntUnit();
    intQtView();
    initQTGrid();
    initQTTruckGrid();
    _handler.inquiryUrl = rootPath + "QTManage/GetFCLCHGData";
    _handler.config = lookUpConfig;

    _handler.afterEdit = function () {
        _changLocalGrid();
        if (ioflag == "O") {
            $MainGrid.jqGrid('getGridParam', "removeAddRowButton")("MainGrid");
            $MainGrid.jqGrid('getGridParam', "removeDelRowButton")("MainGrid");
            $TruckMainGrid.jqGrid('getGridParam', "removeAddRowButton")("TruckMainGrid");
            $TruckMainGrid.jqGrid('getGridParam', "removeDelRowButton")("TruckMainGrid");
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
        var requires = ["QuotNo", "EffectFrom", "EffectTo", "Rlocation",  "TranType", "PodCd", "LspCd", "TranType"];
        var readonlys = ["QuotDateL", "RlocationNm", "LspNm", "QuotType"];

        readonlys = ["QuotDateL", "RlocationNm", "LspNm", "QuotType", "CustNm"];


        var tranMode = $("#TranType").val();
        if (tranMode == "T" || tranMode == "D") {
            $("#oport_city").hide();
            $("#oport_truck_city").show();
            $("#truck_main_grid").show();
            $("#main_grid").hide();
        }
        else {
            $("#oport_city").show();
            $("#oport_truck_city").hide();
            $("#truck_main_grid").hide();
            $("#main_grid").show();
        }

        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
            $("#" + readonlys[i]).siblings("span").find("button").prop("disabled", true);
        }

        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        
         $(".pod").show();
         $(".pol").hide();
    }
     
    _handler.addData = function () {
        //初始化新增数据
        var data;
        if (ioflag == "O")
        {
            data = { "TranMode": "X", "TranType": "A", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "P", LspCd: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        else
        {
            data = { "TranMode": "X", "TranType": "A", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "P", Rlocation: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        _handler.loadGridData("TruckMainGrid", $TruckMainGrid[0], [], [""]);
        $("#LspCd").blur();
        //$("#LspCd").val(_LspCd);
        $("#Rlocation").blur();
        getNowDate();
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");

        _changLocalGrid();
    }
     
    function _changLocalGrid(LspCd) {
        if ($("#LspCd").val() == "" && (LspCd == "" || LspCd == null || LspCd == undefined))
            return;
        var QuotType = $("#QuotType").val();
        gridEditableCtrl({ editable: false, gridId: "MainGrid" });
        if ("Q" != QuotType && "A" != QuotType && "V" != QuotType) {
            gridEditableCtrl({ editable: true, gridId: "MainGrid" });
        }
    }

    _handler.beforSave = function () {
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "ChgCd", index: 12, text: _getLang('L_SMCHGSetup_ChgCd', 'Charge CD') });
        nullCols.push({ name: "ChgType", index: 14, text: _getLang('L_SMCHGSetup_ChgType', 'Charge Type') });
        nullCols.push({ name: "Repay", index: 15, text: _getLang('L_QTSetup_chgCd', 'Cost classify')});
        nullCols.push({ name: "Punit", index: 16, text: _getLang('L_AirQuery_Punit', 'Chargable Unit') });
        nullCols.push({ name: "Cur", index: 16, text: _getLang('L_IpPart_Crncy', 'Currency') });
        var TranType = $("#TranType").val();
        var $grid = $MainGrid;
        if (TranType == "T" || TranType == "D")
            $grid = $TruckMainGrid;
        var result = _handler.checkData($grid, nullCols, sameCols);
        if (result === false) return false;

            var rowIds = $grid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            $grid.jqGrid('setCell', rowIds[i], "SeqNo", i + 1);
        }
        //return true;
        return checkDoubleData($grid);
    }

    function checkDoubleData($grid) {
        var rowIds = $grid.getDataIDs(); 
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
            var key = rowDatas["Punit"] + "#" + rowDatas["ChgCd"] + "#" + rowDatas["ChgType"] + "#" + rowDatas["Carrier"] + "#" + rowDatas["PodCd"] + "#" + rowDatas["SpCd"];
            for (var j = 0; j < rowIds.length; j++) {
                if (rowIds[i] === rowIds[j])
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                var key1 = rowDatas1["Punit"] + "#" + rowDatas1["ChgCd"] + "#" + rowDatas1["ChgType"] + "#" + rowDatas1["Carrier"] + "#" + rowDatas1["PodCd"] + "#" + rowDatas1["SpCd"];
                if (key1 === key) {
                    try {
                        $grid.jqGrid("editCell", rowIds[j], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", _getLang("L_QuotBrokerSetup_Script_174", "charge unit, charge code, cost classify already exists"), 2000, "warning");
                    return false;
                }
            }
        }
    }

    _handler.saveData = function (dtd) {
        var tranType = $("#TranType").val();
        var $grid = $MainGrid;
        if (tranType == "T" || tranType == "D")
            $grid = $TruckMainGrid;
        var containerArray = $grid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
        data["lspCd"] = encodeURIComponent($("#LspCd").val());
        data["quot_no"] = encodeURIComponent($("#QuotNo").val());
        data["dataType"] = "L";
        //data["term"] = encodeURIComponent($("#Incoterm").val());
        var rowIds = $grid.getDataIDs();
        var seqjson = {};
        for (var i = 0; i < rowIds.length; i++) {
            var uid = $grid.jqGrid('getCell', rowIds[i], "UId");
            if (!isEmpty(uid)) {
                var seqno = $grid.jqGrid('getCell', rowIds[i], "SeqNo");
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
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        if (data["sub"])
            _handler.loadGridData("TruckMainGrid", $TruckMainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("TruckMainGrid", $TruckMainGrid[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

        if (_handler.topData.Period === "B") {
            $("#EXCEL_BTN").show();
        }
        else
            $("#EXCEL_BTN").hide();
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        MenuBarFuncArr.Enabled(["IQQT"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Rlocation"], "*");
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["QuotTypeBtn"]);
        MenuBarFuncArr.Enabled(["VoidBtn"]);
        MenuBarFuncArr.Enabled(["ApproveBtn"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
        MenuBarFuncArr.Enabled(["MBCopy"]);
        setRQData(data);

        $("#TranType").triggerChange = false;
        _handler.beforLoadView();
        /*_changLocalGrid();*/
        gridEditableCtrl({ editable: false, gridId: "MainGrid" });
        gridEditableCtrl({ editable: false, gridId: "TruckMainGrid" });
        $("#TranType").triggerChange = true;

        if(data["main"].length > 0)
        {
            if(data["main"][0]["OutIn"] == "I")
            {
                $(".pod").show();
                $(".pol").hide();
            }
            else
            {
                $(".pod").hide();
                $(".pol").show();
            }
        }
    }

    registBtnLookup($("#LspCdLookup"), {
        item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspCd").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);

            _changLocalGrid(map.PartyNo);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
        _changLocalGrid(rd.PARTY_NO);
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

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    //registBtnLookup($("#PolCdLookup"), {
    //    item: "#PolCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
    //        $("#PolCd").val(map.CntryCd + map.PortCd);
    //        $("#PolNm").val(map.PortNm);
    //        $("#PolCd1").val(map.CntryCd + map.PortCd);
    //        $("#PolNm1").val(map.PortNm);
    //    }
    //}, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#PolNm").val(rd.PORT_NM);
    //    $("#PolCd1").val(rd.CNTRY_CD + rd.PORT_CD);
    //    $("#PolNm1").val(rd.PORT_NM);
    //}));

    registBtnLookup($("#PodCdLookup"), {
        item: "#PodCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
            $("#PodNm").val(map.PortNm);
            $("#PodCd2").val(map.CntryCd + map.PortCd);
            $("#PodNm2").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
        $("#PodCd2").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm2").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup2"), {
        item: "#PodCd2", url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodCd2").val(map.PortCd);
            $("#PodNm2").val(map.PortNm);
            $("#PodCd").val(map.PortCd);
            $("#PodNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
        $("#PodCd2").val(rd.PORT_CD);
        $("#PodNm2").val(rd.PORT_NM);
    }));

    registBtnLookup($("#CustCdLookup"), {
        item: '#CustCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#CustCd").val(map.PartyNo);
            $("#CustNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CustCd").val(rd.PARTY_NO);
        $("#CustNm").val(rd.PARTY_NAME);
    }, function ($grid, elem, rowid) {
        $("#CustCd").val("");
        $("#CustNm").val("");
    }));

    registBtnLookup($("#CreditToLookup"), {
        item: '#CreditTo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#CreditTo").val(map.PartyNo);
            $("#CreditNm").val(map.PartyName);
            $("#CreditTo2").val(map.PartyNo);
            $("#CreditNm2").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CreditTo").val(rd.PARTY_NO);
        $("#CreditNm").val(rd.PARTY_NAME);
        $("#CreditTo2").val(rd.PARTY_NO);
        $("#CreditNm2").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#CreditToLookup2"), {
        item: '#CreditTo2', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#CreditTo").val(map.PartyNo);
            $("#CreditNm").val(map.PartyName);
            $("#CreditTo2").val(map.PartyNo);
            $("#CreditNm2").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CreditTo").val(rd.PARTY_NO);
        $("#CreditNm").val(rd.PARTY_NAME);
        $("#CreditTo2").val(rd.PARTY_NO);
        $("#CreditNm2").val(rd.PARTY_NAME);
    }));

    //registBtnLookup($("#IncotermLookup"), {
    //    item: "#Incoterm", url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
    //        $("#Incoterm").val(map.Cd);
    //        //$("#IncotermDescp").val(map.CdDescp);
    //    }
    //}, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
    //    //$("#IncotermDescp").val(rd.CD_DESCP);
    //    $("#Incoterm").val(rd.CD);
    //}));

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        getNowDate();
        $("#EffectFrom").val(getDate(0, "-"));
        $("#EffectTo").val(getDate(365, "-"));
        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");
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
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });


        _handler.loadGridData("TruckMainGrid", $TruckMainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#TruckMainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }
        gridEditableCtrl({ editable: true, gridId: "TruckMainGrid" });
    }
    //getSelectOptions();
    loadQtView(2);
    MenuBarFuncArr.DelMenu(["MBSearch"]);

    $("#TranType").change(function () {
        if ($("#TranType").triggerChange === false)
            return;
        _handler.beforLoadView();
    });

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

    $("#OutIn").on("change", function(){
        var val = $(this).val();

        if(val == "I")
        {
            $(".pod").show();
            $(".pol").hide();
        }
        else
        {
            $(".pod").hide();
            $(".pol").show();
        }
    });

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