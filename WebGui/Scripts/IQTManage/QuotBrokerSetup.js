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

function initQTGrid() {
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
                    else
                        setGridVal($grid, selRowId, "PodNm", map.PortNm, null);
                    setGridVal($grid, selRowId, _name, map.PortCd, "lookup");
                    //$grid.jqGrid('setCell', selRowId, _name, map.CntyCd + map.CityCd);
                    return map.PortCd;
                }
            }, LookUpConfig.TruckPortCdAuto(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                if (_name === "PolCd")
                    setGridVal($grid, rowid, "PolNm", rd.PORT_NM, null);
                else
                    setGridVal($grid, rowid, "PodNm", rd.PORT_NM, null);
                $(elem).val(rd.PORT_CD);
                setGridVal($grid, rowid, _name, rd.PORT_CD, "lookup");
            }), { param: "" });
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
            }), {
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

    if(sel_protype == '')
    {
        sel_protype = ':' + sel_protype;
    }
    else
    {
        sel_protype = ':;' + sel_protype;
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
       { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', sorttype: 'string', width: 100, editable: true, hidden: true },
       { name: 'TranMode', title: 'TranMode', index: 'TranMode', sorttype: 'string', width: 100, editable: true, hidden: true },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd','Charge CD'), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 90, hidden: false, editable: getIoFlag() },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', 'Description'), index: 'ChgDescp', sorttype: 'string', width: 170, hidden: false, editable: false },
        { name: 'ChgType', title: _getLang('L_SMCHGSetup_ChgType', 'Charge Type'), index: 'ChgType', sorttype: 'string', width: 125, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: 'F:F.Freight Charge;O:O.Original Fee;D:D.Destination Fee' }, edittype: 'select' },
        { name: 'Repay', title: _getLang('L_QTSetup_chgCd', 'Cost classify'), index: 'Repay', sorttype: 'string', width: 170, hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _getLang('L_FCLChgSetup_Script_166','') }, edittype: 'select' },
        //{ name: 'PolCd', title: _getLang('L_BaseLookup_PolCd', index: 'PolCd', sorttype: 'string', edittype: 'custom', editoptions: gridLookup(get_cityop("PolCd")), width: 80, hidden: false, editable: true },
        //{ name: 'PolNm', title: _getLang('L_BaseLookup_PolName', index: 'PolNm', sorttype: 'string', width: 120, hidden: false, editable: false },
       //{ name: 'Carrier', title: _getLang('L_QTSetup_Carrier', index: 'Carrier', width: 100, align: 'left', sorttype: 'string', hidden: false, editable: getIoFlag(), formatter: "select", editoptions: { value: _selectWH }, edittype: 'select' },
       //{ name: 'Punit', title: _getLang('L_AirQuery_Punit', index: 'Punit', editoptions: gridLookup(getUnit("Punit")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'Punit', title: _getLang('L_AirQuery_Punit', 'Chargable Unit'), index: 'Punit', sorttype: 'string', width: 140, hidden: false, editable: true, formatter: "select", editoptions: { value: _unitSelect }, edittype: 'select' },

        { name: 'Cur', title: _getLang('L_IpPart_Crncy', 'Currency'), index: 'Cur', sorttype: 'string', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', width: 80, align: 'left', hidden: false, editable: true },
        { name: 'ProductType', title: _getLang('L_IQTManage_ProductType', 'Product Type'), index: 'ProductType', sorttype: 'string', width: 135, hidden: false, editable: true, formatter: "select", editoptions: { value: ':;' + sel_protype }, edittype: 'select' },
       { name: 'HsQtyFrom', title: 'QTY/HS Code From', index: 'HsQtyFrom', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
       { name: 'HsQtyTo', title: 'QTY/HS Code To', index: 'HsQtyTo', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
       { name: 'CntrQtyFrom', title: 'Container From', index: 'CntrQtyFrom', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
       { name: 'CntrQtyTo', title: 'Container To', index: 'CntrQtyTo', width: 80, align: 'right', formatter: 'integer', formatoptions: { thousandsSeparator: " ", defaultValue: '0' }, hidden: false, editable: true },
        { name: 'F3', title: _getLang('L_FCLFSetup_F3', 'Cost'), index: 'F3', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, hidden: false, editable: true },
        //{ name: 'F4', title: '续单费', index: 'F4', width: 80, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, hidden: false, editable: true },
        { name: 'Holiday', title: _getLang('L_IQTManage_Holiday', 'Holiday'), index: 'Holiday', sorttype: 'string', width: 60, hidden: false, editable: true, formatter: "select", editoptions: { value: ':;Y:Yes;N:No' }, edittype: 'select' },
        { name: 'Remark', title: _getLang('L_BSCSSetup_Remark', 'Remark'), index: 'Remark', width: 300, align: 'left', sorttype: 'string', hidden: false, editable: true, editoptions: { size: 500, maxlength: 500 } },
       { name: 'ServiceMode', title: 'Service Mode', index: 'ServiceMode', sorttype: 'string', width: 100, editable: true, hidden: true },
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

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: _getLang('L_QuotBrokerSetup_Scripts_312','Custom Quotation Information'), delKey: ["UId"], height: 300,
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

var lookUpConfig = {
    caption: _getLang("L_MenuBar_Search","search"),
    sortname: "CreateDate",
    refresh: false,
    columns: [
            { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
        { name: 'QuotNo', title: _getLang('L_QTQuery_QuotNo','Quotation NO.'), index: 'QuotNo', width: 200, align: 'left', sorttype: 'string', hidden: false },
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
        var requires = ["QuotNo", "EffectFrom", "EffectTo", "Rlocation", "Cur", "TranType", "LspCd", "PolCd"];
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
            data = { "TranMode": "B", "TranType": "F", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "PP", LspCd: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        else
        {
            data = { "TranMode": "B", "TranType": "F", QuotType: "P", "Period": "B", "OutIn": "I", "FreightTerm": "PP", Rlocation: cmp, "EffectFrom": getDate(0, "-"), "EffectTo": getDate(365, "-") };
        }
        
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#Rlocation").blur();
        $("#LspCd").blur();
        getNowDate();

        //getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=" + cmp + "&stn=" + stn);
        getAutoNo("QuotNo", "rulecode=QUOT_NO&cmp=*&stn=*");

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
        nullCols.push({ name: "ChgCd", index: 7, text: _getLang('L_SMCHGSetup_ChgCd', 'Charge CD') });
        nullCols.push({ name: "ChgType", index: 9, text: _getLang('L_SMCHGSetup_ChgType', 'Charge Type') });
        nullCols.push({ name: "Punit", index: 12, text: _getLang('L_AirQuery_Punit', 'Chargable Unit') });
        nullCols.push({ name: "Cur", index: 13, text: _getLang('L_IpPart_Crncy', 'Currency') });
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
            var key = rowDatas["Punit"] + "#" + rowDatas["ChgCd"] + "#" + rowDatas["ChgType"] + "#" + rowDatas["Carrier"] + "#" + rowDatas["Cur"];
            for (var x = 0; x < checkKey.length; x++) {
                if (key === checkKey[x]) {
                    try {
                        $grid.jqGrid("editCell", rowIds[i], 10, true);
                    }
                    catch (e) {
                    }
                    CommonFunc.Notify("", _getLang("L_QuotBrokerSetup_Script_174", "charge unit, charge code, cost classify already exists"), 2000, "warning");
                    return false;
                }
            }
            for (var j = 0; j < rowIds.length; j++) {
                if (rowIds[i] === rowIds[j])
                    continue;
                var rowDatas1 = $grid.jqGrid('getRowData', rowIds[j]);
                var key1 = rowDatas1["Punit"] + "#" + rowDatas1["ChgCd"] + "#" + rowDatas1["ChgType"];
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
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);

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
        $("#TranType").triggerChange = false;
        _handler.beforLoadView();
        $("#TranType").triggerChange = true;

        if(data["main"].length > 0)
        {
            if(data["main"][0]["OutIn"] == "I")
            {
                $("#iport_city").show();
                $("#oport_city").hide();
            }
            else
            {
                $("#oport_city").show();
                $("#iport_city").hide();
            }
        }

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

    registBtnLookup($("#PolCdLookup"), {
        item: "#PolCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            $("#PolNm").val(map.PortNm);
            $("#PolCd1").val(map.CntryCd + map.PortCd);
            $("#PolNm1").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolNm").val(rd.PORT_NM);
        $("#PolCd1").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolNm1").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup"), {
        item: "#PodCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
            $("#PodNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
    }));

    $("#OutIn").on("change", function(){
        var val = $(this).val();

        if(val == "I")
        {
            $("#iport_city").show();
            $("#oport_city").hide();
        }
        else
        {
            $("#oport_city").show();
            $("#iport_city").hide();
        }
    })

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

        //_handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
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
