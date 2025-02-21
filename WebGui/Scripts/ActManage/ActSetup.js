var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid, $SubGrid;
var IoFlag = getCookie("plv3.passport.ioflag");
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
    basecmp = getCookie("plv3.passport.basecompanyid");
var fssplang = getCookie("language");
var viewable = false;
var editable = true;
if (IoFlag == "O") {
    viewable = true;
    editable = false;
    $("#CheckDescp").prop("readonly", true);
}
$(function () {
    var BookingLookup = {
        caption: _getLang("L_DNManage_SerBl", ''),
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true, viewable: false },
            { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DebitNo', title: _getLang('L_ActQuery_TpvDebitNo', ''), index: 'DebitNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };

    _handler.saveUrl = rootPath + "ActManage/SaveData";
    _handler.inquiryUrl = rootPath + "ActManage/GetActQueryData";
    _handler.config = BookingLookup;

    MenuBarFuncArr.EndFunc = function () {
        SetDateReadonlys(true);
    }
    _handler.gridEditableCtrl = function (editable) {//结束grid的编辑状态
        var grids = _handler.grids;
        for (var i = 0; i < grids.length; i++) {
            try {
                if (grids[i].editable) {
                    gridEditableCtrl({ editable: editable, gridId: grids[i].id });
                    $('#' + grids[i].id).jqGrid('getGridParam', "removeAddRowButton")(grids[i].id);
                }
            }
            catch (e) {
            }
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = { DebitDate: getDate(0, "-"), "Status": "A", "DebitType": "A" };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        $MainGrid.jqGrid("clearGridData");
        $("#autoGetChg").prop("disabled", false);
        $("#getChg").prop("disabled", false);
        $("#SetERate").prop("disabled", false);

        $("#LspNo").val(cmp);
        $("#LspNo").blur();
        $("#Receiver").val(cmp);
        $("#DebitType").val("A");
        $("#Receiver").blur();
        getAutoNo("TpvDebitNo", "rulecode=DEBIT_NO&cmp=*&stn=*");
        if (IOFlag == "O") { getBankInfo(cmp); }
    }

    _handler.editData = function () {
        $("#autoGetChg").prop("disabled", false);
        $("#getChg").prop("disabled", false);
        $("#SetERate").prop("disabled", false);
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
    AddMBPrintFunc();


    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (!isEmpty($("#UFid").val())) {
            alert(_getLang("L_ActSetup_Scripts_49", ''));
            return false;
        }

        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_58", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "E") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_59", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_60", ''), 500, "warning");
            return false;
        }

        
    }

    _handler.afterEdit = function () {
        _handler.beforLoadView(true);
        
        //var status = _handler.topData["Status"] || "";
        //switch (status) {
        //    case "D":
        //    case "E":
        //    case "F":
        //        _handler.gridEditableCtrl(false);
        //        break;
        //}
    }

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        if (!isEmpty($("#UFid").val())) {
            alert(_getLang("L_DNManage_RefNlCtEt", ''));
            return false;
        }

        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_61", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "E") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_62", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_63", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_64", ''), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "R") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Reject", ''), 500, "warning");
            return false;
        }
    }

    _handler.setFormData = function (data) {
        console.log(data);
        if (data["main"]) {
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        }
        else {
            _handler.topData = [{}];
        }
        if (data["sub"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        var col = $MainGrid.jqGrid('getCol', 'Status', false);//获取批文号码列的值
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
        //MenuBarFuncArr.initEdoc($("#UId").val());
        //MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["SEND_BTN", "UPLOAD_BTN", "MBEdoc", "MBInvalid", "MBPreview", "MBVatNo"]);
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");

        if ($("#Status").val() == "V") {
            statusIsInvalid();
        }

        var curFrom = "";
        if (data["sub"]) {
            for (var i = 0; i < data["sub"].length; i++) {
                var map = data["sub"][i];
                if (curFrom.indexOf(map.Cur) < 0) {
                    if (map.Cur == null || map.Cur == "" || map.Cur == undefined) {
                        continue;
                    }
                    curFrom += map.Cur + ";";
                }
            }
        }
        var currarry = curFrom.split(';');
        $("#CurrencyFrom").empty();
        for (var i = 0; i < currarry.length; i++) {
            var currey = currarry[i];
            if (currey == null || currey == "" || currey == undefined) {
                continue;
            }
            $("#CurrencyFrom").append("<option value=\"" + currey + "\">" + currey + "</option>");
        }
        $("#CurrencyTo").val($("#Cur").val());


        if (typeof data["main"] !== "undefined" && typeof data["main"][0] !== "undefined") {
            if (data["main"][0].ApproveTo != "A") {
                //MenuBarFuncArr.Disabled(["MBEdit", "SEND_BTN", "MBDel", "MBInvalid"]);
                $("#autoGetChg").prop("disabled", true);
                $("#getChg").prop("disabled", true);
                $("#SetERate").prop("disabled", true);
            }
        }

        $("#autoGetChg").prop("disabled", true);
        $("#getChg").prop("disabled", true);
        $("#SetERate").prop("disabled", true);
        getSelectOptions();
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

    _handler.beforSave = function () {
        var containerArray = $MainGrid.getDataIDs();
        if (containerArray.length <= 0) {
            alert(_getLang("L_ActSetup_Scripts_54", ''));
            return false;
        }
        var checkinfo = CheckBeforConfirm();
        if (checkinfo != "Y") {
            alert(checkinfo);
            return false;
        }
        var LamtValue = $MainGrid.jqGrid("getCol", "Lamt", false, "sum");
        $("#Amt").val(CommonFunc.formatFloat(LamtValue, 2));

        var cur = $("#Cur").val();
        var ids = $MainGrid.jqGrid('getDataIDs');
        var QlamtValue = 0;
        for (var i = 0; i < ids.length; i++) {
            var qcur = getGridVal($MainGrid, ids[i], "Qcur", null);
            if (cur == qcur) {
                var qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                QlamtValue += parseFloat(qamt);
            } else {
                var qlamt = getGridVal($MainGrid, ids[i], "Qlamt", null);
                QlamtValue += parseFloat(qlamt);
            }
        }
        $("#Qamt").val(CommonFunc.formatFloat(QlamtValue, 2));
        var SubAmt = LamtValue - QlamtValue;
        $("#SubAmt").val(CommonFunc.formatFloat(SubAmt, 2));

        return _checkData();
    }

    function getValueParseFloat(val) {
        if (val == null || val == undefined || val === "")
            val = 0;
        val = parseFloat(val + '');
        return val;
    }

    function _checkData() {
        var BillTo = $("#BillTo").val();
        var rowIds = $MainGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            var bamt = getValueParseFloat(rowDatas["Bamt"]);
            var lamt1 = getValueParseFloat(rowDatas["Lamt"]);
            var rate = getValueParseFloat(rowDatas["ExRate"]);
            var lamt = toFixed(Mul(bamt, rate), 2);
            if (lamt1 !== lamt)
                $MainGrid.jqGrid('setCell', rowIds[i], "Lamt", lamt);
        }

        for (var i = 0; i < rowIds.length; i++) {
            if (rowIds[i].indexOf("jqg") < 0)
                continue;
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            //if (BillTo !== rowDatas["DebitTo"]) {
            //    alert("@Resources.Locale.L_ActSetup_Script_65 To(" + rowDatas["DebitTo"] + ")@Resources.Locale.L_ActSetup_Script_66" + BillTo + ")@Resources.Locale.L_ActSetup_Scripts_57");
            //    return false;
            //}
        }
        return true;
    }

    function SetDateReadonlys(readonly) {
        var readonlys = ["ActPayDate"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', readonly);
            $("#" + readonlys[i]).attr('disabled', readonly);
            $("#" + readonlys[i]).parent().find("button").attr('disabled', readonly);
        }
    }

    $MainGrid = $("#MainGrid");
    //$SubGrid = $("#SubGrid2");
    //
    $("#autoGetChg").click(function (event) {
        var ShipmentId = $("#ShipmentId").val();
        var LspNo = $("#LspNo").val();
        var Cur = $("#Cur").val();
        //var DebitTo = $("#BillTo").val() || '';
        var postData = { ShipmentId: ShipmentId, LspNo: LspNo };
        if (ShipmentId == "" || LspNo == "" || Cur == "") {//|| DebitTo === ""
            alert(_getLang("L_ActSetup_Script_67", '') + " id," + _getLang('L_ActSetup_Script_68', '') + '' + _getLang("L_ActSetup_Scripts_59", ''));
            return;
        }
        $.ajax({
            url: rootPath + "TPVCommon/GetSmbidByid",
            type: 'POST',
            data: postData,
            async: false,
            dataType: "JSON",
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            error: function () {
                CommonFunc.ToogleLoading(false);
                CommonFunc.Notify("", "error", 1300, "warning");
            },
            success: function (data) {
                //alert(data)
                CommonFunc.ToogleLoading(false);
                CommonFunc.Notify("", _getLang("L_ActSetup_Scripts_60", ''), 500, "success");
                var allData = $MainGrid.find("tr");
                var dataRow, addData = [];
                var fields = ["Cur", "Bamt", "Lamt", "ExRate", "UnitPrice", "Qty", "Tax", "ChgUnit", "Qlamt", "QunitPrice", "Qqty", "QchgUnit", "Qamt", "Qcur", "QexRate", "DebitTo", "DebitNm", "BiRemark", "DimensionsInfo"];
                var curFrom="";
                for (var i = 0; i < data["sub"].length; i++) {
                    var map = data["sub"][i];

                    if (curFrom.indexOf(map.Cur) < 0) {
                        if (map.Cur == null || map.Cur == "" || map.Cur == undefined) {
                            continue;
                        }
                        curFrom += map.Cur + ";";
                    }

                    dataRow = {
                        'UId': map.UId,
                        'Status': map.Status,
                        'LspNo': map.LspNo,
                        'ShipmentId': map.ShipmentId,
                        'BlNo': map.BlNo,
                        'ChgCd': map.ChgCd,
                        'ChgDescp': map.ChgDescp,
                        'Cur': map.Qcur,
                        'ChgUnit': map.ChgUnit,
                        'Qty': map.Qty,
                        'Qamt': map.Qamt,
                        'Bamt': map.Qamt,
                        'Tax': map.Qtax,
                        'Remark': map.Remark,
                        'CheckDescp': map.CheckDescp,
                        'QtData': 'Y'
                    };
                    for (var j = 0; j < fields.length; j++) {
                        if (map[fields[j]] === undefined)
                            continue;
                        dataRow[fields[j]] = map[fields[j]];
                    }
                    addData.push(dataRow);
                }
                var currarry = curFrom.split(';');
                $("#CurrencyFrom").empty();
                for (var i = 0; i < currarry.length; i++) {
                    var currey = currarry[i];
                    if (currey == null || currey == "" || currey == undefined) {
                        continue;
                    }
                    $("#CurrencyFrom").append("<option value=\"" + currey + "\">" + currey + "</option>");
                }

                for (var i = 0; i < addData.length; i++) {
                    var chk = 1;
                    for (var j = 0; j < allData.length; j++) {
                        if (typeof allData[j]["id"] != "undefined" && allData[j]["id"] != "") {
                            var ShipmentId = getGridVal($MainGrid, allData[j]["id"], "ShipmentId", null);
                            var ChgCd = getGridVal($MainGrid, allData[j]["id"], "ChgCd", null);

                            if (addData[i]["ShipmentId"] == ShipmentId && addData[i]["ChgCd"] == ChgCd) {
                                chk = 0;
                                break;
                            }

                        }
                    }
                    if (chk == 1) {
                        $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
                    }
                }
                var LamtValue = $MainGrid.jqGrid("getCol", "Lamt", false, "sum");
                $("#Amt").val(CommonFunc.formatFloat(LamtValue, 2));

                var cur = $("#Cur").val();
                var ids = $MainGrid.jqGrid('getDataIDs');
                var QlamtValue = 0;
                for (var i = 0; i < ids.length; i++) {
                    var qcur = getGridVal($MainGrid, ids[i], "Qcur", null);
                    if (cur == qcur) {
                        var qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                        QlamtValue += parseFloat(qamt);
                    } else {
                        var qlamt = getGridVal($MainGrid, ids[i], "Qlamt", null);
                        QlamtValue += parseFloat(qlamt);
                    }
                }

                $("#Qamt").val(CommonFunc.formatFloat(QlamtValue, 2));
                var SubAmt = LamtValue - QlamtValue;
                $("#SubAmt").val(CommonFunc.formatFloat(SubAmt, 2));
            },
            cache: false
        });
    });

    $("#ChgType").change(function () {
        var val = '';
        var strs = [];
        $('#ChgType :selected').each(function (i, selected) {
            strs[i] = selected.text;
        });
        if (strs.length > 0)
            val = strs[0];
        if (val != '') {
            $("#Remark").val(val);
        }
    });

    function getchg(name) {
        var _name = name;
        var chg_op = getLookupOp("MainGrid",
            {
                url: rootPath + "ActManage/SmbidQueryData",
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
            }, LookUpConfig.GetCurAuto(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.CUR, "lookup");
                $(elem).val(rd.CUR);
            }));
        return cur_op;
    }

    function getSm(name) {
        var _name = name;
        var sm_op = getLookupOp("MainGrid",
            {
                url: rootPath + "TPVCommon/GetSmsmForLookup",
                config: LookUpConfig.SmsmLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, "BlNo", map.MasterNo, null);
                    return map.ShipmentId;
                }
            }, LookUpConfig.GetSmsmAuto(groupId, undefined, $MainGrid,
            function ($grid, rd, elem, rowid) {
                setGridVal($grid, rowid, _name, rd.ShipmentId, "lookup");
                $(elem).val(rd.SHIPMENT_ID);
            }), {
                param: "",
                baseConditionFunc: function () {
                    return "";
                }
            });
        return sm_op;
    }


    function CheckBeforConfirm() {
        if (contrast($("#DebitDate").val(), $("#PayDate").val())) {
            return _getLang('L_ActSetup_PayDate', '') + " must be later than " + _getLang('L_ActQuery_DebitDate', '');
        }
        var debitdate=$("#DebitDate").val() 
        debitdate=debitdate.replace(new RegExp("/", "gm"), "-");

        var beginTimes = debitdate.substring(0, 10).split('-');
        var starttime = new Date(beginTimes[0], beginTimes[1], beginTimes[2]);

        if (starttime >= debitdate) {
            return _getLang('L_ActQuery_DebitDate', '') + " shouldn't less than today";
        }

        var cmp = $("#Cmp").val();
        var bankType = $("#BankType").val();
        if (fsspArray.indexOf(cmp) > 0  && !isEmpty(cmp) && isEmpty(bankType)) {
            return _getLang('L_BankInfo_BankType', '') + " Can't be empty";
        }
        return "Y";
    }

    function contrast(comparie, etd) {
        if (isEmpty(comparie)) return false;
        if (isEmpty(etd)) return false;
        comparie = comparie.replace(new RegExp("/", "gm"), "-");
        etd = etd.replace(new RegExp("/", "gm"), "-");
        //etd = etd.replace("/", "-");
        if (comparie < etd) return false;
        var beginTimes = comparie.substring(0, 10).split('-');
        var endTimes = etd.substring(0, 10).split('-');

        var starttime = new Date(beginTimes[0], beginTimes[1], beginTimes[2]);
        var starttimes = starttime.getTime();

        var lktime = new Date(endTimes[0], endTimes[1], endTimes[2]);
        var lktimes = lktime.getTime();

        if (starttimes > lktimes) {
            return true;
        }
        else
            return false;
    }


    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'QtData', title: 'QtData', index: 'QtData', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'LspNo', title: 'U ID', index: 'LspNo', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'Status', title: _getLang('L_ActManage_MatchorNot', ''), index: 'Status', width: 50, align: 'left', sorttype: 'string', hidden: false, formatter: "select", editoptions: { value: 'Y:Yes;N:No' } },
        { name: 'ShipmentId', title: 'Shipment', index: 'ShipmentId', sorttype: 'string', width: 100, editable: false, hidden: false, editoptions: gridLookup(getSm("ShipmentId")), edittype: 'custom' },
        { name: 'BlNo', title: _getLang('L_ActSetup_BlNo', ''), index: 'BlNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'IpartNo', title: _getLang('L_ActSetup_Script_69', ''), index: 'IpartNo', sorttype: 'string', width: 120, editable: false, hidden: false },
        { name: 'ChgCd', title: _getLang('L_SMCHGSetup_ChgCd', ''), index: 'ChgCd', editoptions: gridLookup(getchg("ChgCd")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'ChgDescp', title: _getLang('L_SMCHGSetup_ChgDescp', ''), index: 'ChgDescp', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'Cur', title: _getLang('L_ActQuery_Cur', ''), index: 'Cur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: false },
        {
            name: 'UnitPrice', title: _getLang('L_QTManage_IvUP', ''), index: 'UnitPrice',
            width: 100, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, sorttype: 'string',
            hidden: false, editable: editable
        },
        { name: 'ChgUnit', title: _getLang('L_ActSetup_Scripts_61', ''), index: 'ChgUnit', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'Qty', title: _getLang('L_ActCheck_Views_15', ''), index: 'Qty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: editable },

        { name: 'Bamt', title: _getLang('L_ActSetup_Bamt', ''), index: 'Bamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },

        { name: 'ExRate', title: _getLang('L_ChgApproveManage_Views_197', ''), index: 'ExRate', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, width: 100, hidden: viewable, editable: false },

        { name: 'Lamt', title: _getLang('L_ActDeatilManage_Views_68', ''), index: 'Lamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: false, editable: false },
        { name: 'Tax', title: _getLang('L_ActSetup_Tax', ''), index: 'Tax', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 80, hidden: false, editable: editable },
        //{ name: 'VatNo', title: _getLang('L_ActSetup_VatNo',''), index: 'VatNo', sorttype: 'string', width: 250, hidden: false, editable: false },
        { name: 'BiRemark', title: _getLang('L_BSCSSetup_Remark', ''), index: 'BiRemark', sorttype: 'string', width: 200, hidden: false, editable: true },
        { name: 'CheckDescp', title: _getLang('L_ActSetup_CheckDescp', ''), index: 'CheckDescp', sorttype: 'string', width: 200, hidden: false, editable: editable },
        { name: 'Remark', title: _getLang('L_IpPart_WithholdingRemark', ''), index: 'Remark', sorttype: 'string', width: 200, hidden: false, editable: false },
        { name: 'Qcur', title: _getLang('L_BaseLookup_WithholdCur', ''), index: 'Qcur', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'QunitPrice', title: _getLang('L_BaseLookup_WithholdPrice', ''), index: 'QunitPrice', sorttype: 'float', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, editable: false },
        { name: 'QchgUnit', title: _getLang('L_ActSetup_Scripts_62', ''), index: 'QchgUnit', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'Qqty', title: _getLang('L_BaseLookup_WithholdQty', ''), index: 'Qqty', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.0000' }, sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Qamt', title: _getLang('L_ActSetup_Qamt', ''), index: 'Qamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: viewable, editable: false },
        { name: 'QexRate', title: _getLang('L_ActSetup_Scripts_63', ''), index: 'QexRate', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 6, defaultValue: '0.000000' }, width: 100, hidden: viewable, editable: false },
        { name: 'Qlamt', title: _getLang('L_ActSetup_Scripts_64', ''), index: 'Qlamt', sorttype: 'string', align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, width: 100, hidden: viewable, editable: false },
        { name: 'DebitTo', title: 'Debit To', index: 'DebitTo', sorttype: 'string', width: 85, hidden: false, editable: false },
        { name: 'DebitNm', title: 'Debit To Name', index: 'DebitNm', sorttype: 'string', width: 180, hidden: false, editable: false },
        { name: 'InvoiceInfo', title: 'Invoice Info.', index: 'InvoiceInfo', sorttype: 'string', width: 200, hidden: false, editable: false }
    ];

    var colModel2 = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'ShipmentId', title: _getLang('L_DNApproveManage_ShipmentId', ''), index: 'ShipmentId', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'LspNo', title: _getLang('L_DRule_LspNo', ''), index: 'LspNo', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'LspNm', title: _getLang('L_DRule_LspNm', ''), index: 'LspNm', sorttype: 'string', width: 100, hidden: false, editable: false },
        {
            name: 'Etd', title: _getLang('L_AirTransport_Etd', ''), index: 'Etd',
            width: 100, align: 'left', sorttype: 'string',
            hidden: false, editable: false
        },
        //{ name: 'NotifyDate', title: '通知日期', index: 'NotifyDate', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'PolCd', title: _getLang('L_IESetup_PolCd', ''), index: 'PolCd', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'PodCd', title: _getLang('L_IESetup_PodCd', ''), index: 'PodCd', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'DebitNo', title: _getLang('L_ActQuery_DebitNo', ''), index: 'DebitNo', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Cur', title: _getLang('L_IpPart_Crncy', ''), index: 'Cur', sorttype: 'string', width: 80, hidden: false, editable: false },
        { name: 'ExRate', title: _getLang('L_ExchangeRate_ExRate', ''), index: 'ExRate', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F1', title: _getLang('L_ActSetup_F1', ''), index: 'F1', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F2', title: _getLang('L_ActSetup_F2', ''), index: 'F2', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F3', title: _getLang('L_ActSetup_F3', ''), index: 'F3', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F4', title: _getLang('L_ActSetup_F4', ''), index: 'F4', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'F5', title: _getLang('L_ActSetup_F5', ''), index: 'F5', sorttype: 'string', width: 100, hidden: false, editable: false }
    ];

    _handler.intiGrid("MainGrid", $MainGrid, {
        colModel: colModel, caption: _getLang('L_ActSetup_Scripts_65', '帳單費用明細'), delKey: ["UId", "QtData"], height: 250,
        savelayout: true,
        showcolumns: true,
        afterSaveCellWithIdFunc: function (rowid, name, val, iRow, iCol, toolId) {
            var ExRate, Bamt, Qamt;
            switch (name) {
                case "ExRate":
                    ExRate = val;
                    Bamt = getGridVal($MainGrid, rowid, "Bamt", null);
                    Qamt = getGridVal($MainGrid, rowid, "Qamt", null);
                    break;
                default:
                    return true;
                    break;
            }
            var Lamt = Mul(ExRate, Bamt);
            setGridVal($MainGrid, rowid, "Lamt", Lamt, null);
            setGridVal($MainGrid, rowid, "QexRate", ExRate, null);
            var Qlamt = Mul(ExRate, Qamt);
            setGridVal($MainGrid, rowid, "Qlamt", Qlamt, null);
            var LamtValue = $MainGrid.jqGrid("getCol", "Lamt", false, "sum");
            $("#Amt").val(CommonFunc.formatFloat(LamtValue, 2));
            var cur = $("#Cur").val();
            var ids = $MainGrid.jqGrid('getDataIDs');
            var QlamtValue = 0;
            for (var i = 0; i < ids.length; i++) {
                var qcur = getGridVal($MainGrid, ids[i], "Qcur", null);
                if (cur == qcur) {
                    var qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                    QlamtValue += parseFloat(qamt);
                } else {
                    var qlamt = getGridVal($MainGrid, ids[i], "Qlamt", null);
                    QlamtValue += parseFloat(qlamt);
                }
            }
            $("#Qamt").val(CommonFunc.formatFloat(QlamtValue, 2));

            var SubAmt = LamtValue - QlamtValue;
            $("#SubAmt").val(CommonFunc.formatFloat(SubAmt, 2));
        },
        //dblClickFunc: function (map) {
        //    //用于回调函数，例如赋值操作等
        //    var UId = map.UId;
        //    top.topManager.openPage({
        //        href: rootPath + "ActManage/ActUpdate?add=N&uid=" + UId,
        //        title: '帳單费用錄入',
        //        id: 'ActUpdate'
        //    });
        //},
        onAddRowFunc: function (rowid) {
            // var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            // if (typeof maxSeqNo === "undefined")
            //     maxSeqNo = 0;
            // $MainGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            var Cur = $("#Cur").val();
            $MainGrid.jqGrid('setCell', rowid, "Cur", Cur);
        },
        beforeSelectRowFunc: function (rowid) {
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $MainGrid.setColProp('StsCd', { editable: true });
            } else {
                $MainGrid.setColProp('StsCd', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            $MainGrid.setColProp('StsCd', { editable: true });
        }
    });

    // _handler.intiGrid("SubGrid2", $SubGrid, {
    //     colModel: colModel2, caption: '', delKey: ["UId"],
    //     onAddRowFunc: function (rowid) {
    //         var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
    //         if (typeof maxSeqNo === "undefined")
    //             maxSeqNo = 0;
    //         $SubGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
    //     },
    //     beforeSelectRowFunc: function (rowid) {
    //     },
    //     beforeAddRowFunc: function (rowid) {
    //     }
    // });
    var lookup = $.extend({}, LookUpConfig.SmbidModel);
    var _config = $.extend({ multiselect: true }, lookup);
    registBtnLookup($("#getChg"), {
        url: rootPath + "TPVCommon/SmbidQueryData", config: _config, param: "", openclick:false, selectRowFn: function (map) {
        }
    }, {
        columnID: "UId",
        onClickRegiBtnFunc: function () {
            var LspNo = $("#LspNo").val() || '';
            //var BillTo = $("#BillTo").val() || '';
            var Cur = $("#Cur").val() || '';
            var Cmp = $("#Cmp").val() || '';
            var tranType = $("#TranType").val() || '';
            //var postData = { ShipmentId: ShipmentId, LspNo: LspNo, Cur: Cur };
            var postData = { ShipmentId: ShipmentId, LspNo: LspNo };
            //
            if (LspNo === "" || Cmp === "" || Cur === "" || tranType === "") {//BillTo === "" ||
                alert(_getLang("L_ActSetup_Script_70", ''));
                return false;
            }
            return true;
        },
        responseMethod: function (data) {
            var LspNo = $("#LspNo").val();
            var allData = $MainGrid.find("tr");
            var dataRow = {}, addData = [], uidData = [];
            var curFrom = "";
            var fields = ["Cur", "Bamt", "Lamt", "ExRate", "UnitPrice", "Qty", "Tax", "ChgUnit", "Qlamt", "QunitPrice", "Qqty", "QchgUnit", "Qamt", "Qcur", "QexRate", "DebitTo", "DebitNm", "BiRemark"];
            for (var i = 0; i < data.length; i++) {
                var map = data[i];
                if (curFrom.indexOf(map.Cur) < 0) {
                    if (map.Cur == null || map.Cur == "" || map.Cur == undefined) {
                        continue;
                    }
                    curFrom += map.Cur + ";";
                }
                dataRow = {
                    'UId': map.UId,
                    'Status': map.Status,
                    'LspNo': map.LspNo,
                    'ShipmentId': map.ShipmentId,
                    'BlNo': map.BlNo,
                    'ChgCd': map.ChgCd,
                    'ChgDescp': map.ChgDescp,
                    'Cur': map.Qcur,
                    'ChgUnit': map.ChgUnit,
                    'Qty': map.Qty,
                    'Qamt': map.Qamt,
                    'Bamt': map.Qamt,
                    'Tax': map.Qtax,
                    'Remark': map.Remark,
                    'CheckDescp': map.CheckDescp,
                    'QtData': 'Y'
                };

                for (var j = 0; j < fields.length; j++) {
                    if (map[fields[j]] === undefined)
                        continue;
                    dataRow[fields[j]] = map[fields[j]];
                }
                addData.push(dataRow);
            }

            var currarry = curFrom.split(';');
            $("#CurrencyFrom").empty();
            for (var i = 0; i < currarry.length; i++) {
                var currey = currarry[i];
                if (currey == null || currey == "" || currey == undefined) {
                    continue;
                }
                $("#CurrencyFrom").append("<option value=\"" + currey + "\">" + currey + "</option>");
            }

            uidData = $MainGrid.jqGrid("getCol", "UId");
            for (var i = 0; i < addData.length; i++) {
                if ($.inArray(addData[i]["UId"], uidData) == -1) {
                    $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
                }
            }

            /*
            for (var i = 0; i < addData.length; i++) {
                var chk = 1;

                for (var j = 0; j < allData.length; j++) {
                    if (typeof allData[j]["id"] != "undefined" && allData[j]["id"] != "") {
                        var sUId = getGridVal($MainGrid, allData[j]["id"], "UId", null);
                        //var ChgCd = getGridVal($MainGrid, allData[j]["id"], "ChgCd", null);

                        if (addData[i]["UId"] == sUId ) 
                        {
                            chk = 0;
                            break;
                        }

                    }
                }

                if (chk == 1) {
                    $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
                }
            }*/
            var LamtValue = $MainGrid.jqGrid("getCol", "Lamt", false, "sum");
            $("#Amt").val(CommonFunc.formatFloat(LamtValue, 2));
            var cur = $("#Cur").val();
            var ids = $MainGrid.jqGrid('getDataIDs');
            var QlamtValue = 0;
            for (var i = 0; i < ids.length; i++) {
                var qcur = getGridVal($MainGrid, ids[i], "Qcur", null);
                if (cur == qcur) {
                    var qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                    QlamtValue += parseFloat(qamt);
                } else {
                    var qlamt = getGridVal($MainGrid, ids[i], "Qlamt", null);
                    QlamtValue += parseFloat(qlamt);
                }
            }
            $("#Qamt").val(CommonFunc.formatFloat(QlamtValue, 2));
            var SubAmt = LamtValue - QlamtValue;
            $("#SubAmt").val(CommonFunc.formatFloat(SubAmt, 2));
            
        },
        baseConditionFunc: function () {
            var LspNo = $("#LspNo").val() || '';
            //var DebitTo = $("#BillTo").val() || '';
            var Cur = $("#Cur").val() || '';
            var Cmp = $("#Cmp").val() || '';
            //var tranType = $("#TranType").val() || '';
            var filter = "";

            if (LspNo != "") {
                filter += " LSP_NO='" + LspNo + "'";
            }
            //if (DebitTo != "") {
            //    if (filter.length > 0)
            //        filter += " AND ";
            //    filter += " DEBIT_TO='" + DebitTo + "'";
            //}
            //if (Cur != "") {
            //    if (filter.length > 0)
            //        filter += " AND ";
            //    filter += " CUR='" + Cur + "'";
            //}

            if (Cmp != "") {
                if (filter.length > 0)
                    filter += " AND ";
                filter += " CMP='" + Cmp + "'";
            }

            //if (tranType != "") {
            //    if (filter.length > 0)
            //        filter += " AND ";
            //    filter += " TRAN_TYPE='" + tranType + "'";
            //}
            return filter;
        }
    });

    $("#SetERate").click(function () {
        if ($("#CurrencyFrom").val() == "" || $("#CurrencyFrom").val() == undefined || $("#CurrencyFrom").val() == null) {
            alert("Please Select the Currency From ");
            return;
        }
        if ($("#ChangeRate").val() == "" || $("#ChangeRate").val() == undefined || $("#ChangeRate").val() == null) {
            alert("Please Input the ChangeRate ");
            $("#ChangeRate").setfocus();
            return;
        }
        var ids = $MainGrid.jqGrid('getDataIDs');
        var ChangeRate=$("#ChangeRate").val();
        var currfrom = $("#CurrencyFrom").val();
        var currto = $("#CurrencyTo").val();
        var QlamtValue = 0;
        for (var i = 0; i < ids.length; i++) {
            var qcur = getGridVal($MainGrid, ids[i], "Cur", null);
            rowid = ids[0];
            if (qcur == currfrom) {
                setGridVal($MainGrid, ids[i], "ExRate", ChangeRate, null);
                setGridVal($MainGrid, ids[i], "QexRate", ChangeRate, null);
                var Bamt = getGridVal($MainGrid, ids[i], "Bamt", null);
                var Qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                var Lamt = Mul(ChangeRate, Bamt);
                setGridVal($MainGrid, ids[i], "Lamt", Lamt, null);

                var Qlamt = Mul(ChangeRate, Qamt);
                setGridVal($MainGrid, ids[i], "Qlamt", Qlamt, null);
            }
            if (qcur == currto) {
                setGridVal($MainGrid, ids[i], "ExRate", 1, null);
                setGridVal($MainGrid, ids[i], "QexRate", 1, null);
                var Bamt = getGridVal($MainGrid, ids[i], "Bamt", null);
                var Qamt = getGridVal($MainGrid, ids[i], "Qamt", null);
                setGridVal($MainGrid, ids[i], "Lamt", Bamt, null);
                setGridVal($MainGrid, ids[i], "Qlamt", Qamt, null);
                QlamtValue += parseFloat(Qamt);
            } else {
                var Qlamt = getGridVal($MainGrid, ids[i], "Qlamt", null);
                QlamtValue += parseFloat(Qlamt);
            }
        }
        var LamtValue = $MainGrid.jqGrid("getCol", "Lamt", false, "sum");
        $("#Amt").val(CommonFunc.formatFloat(LamtValue, 2));

        $("#Qamt").val(CommonFunc.formatFloat(QlamtValue, 2));

        var Bamt = $MainGrid.jqGrid("getCol", "Bamt", false, "sum");
        var Qamt = $MainGrid.jqGrid("getCol", "Qamt", false, "sum");
        var SubAmt = LamtValue - QlamtValue;
        $("#SubAmt").val(CommonFunc.formatFloat(SubAmt, 2));
    });

    registBtnLookup($("#SmLookup"), {
        item: "#ShipmentId", url: rootPath + "TPVCommon/GetSmsmiForLookup", config: LookUpConfig.SmsmLookup, param: "", selectRowFn: function (map) {
            $("#ShipmentId").val(map.ShipmentId);
            $("#Pol").val(map.PolCd);
            $("#PolNm").val(map.PolName);
            $("#Pod").val(map.PodCd);
            $("#PodNm").val(map.PodName);
            $("#Qty").val(map.PkgNum);
            $("#Qtyu").val(map.PkgUnit);
            $("#Gw").val(map.Gw);
            $("#Gwu").val(map.Gwu);
            $("#Cbm").val(map.Cbm);
            $("#Cw").val(map.Cw);
            $("#Cmp").val(map.Cmp);
            $("#TranType").val(map.TranType);

            if (IOFlag == "O") {
                $("#LspNo").val(LspNo);
                $("#LspNm").val(LspNm);
            }
            getSelectOptions(true);
        }
    }, {}, LookUpConfig.GetSmsmiAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ShipmentId").val(rd.SHIPMENT_ID);
        $.post(rootPath + "TPVCommon/GetSmsmiByid", { ShipmentId: rd.SHIPMENT_ID }, function (data, textStatus, xhr) {
            console.log(data);
            if (data.rows.length > 0) {
                var map = data.rows[0];
                $("#Pol").val(map.PolCd);
                $("#PolNm").val(map.PolName);
                $("#Pod").val(map.PodCd);
                $("#PodNm").val(map.PodName);
                $("#Qty").val(map.PkgNum);
                $("#Qtyu").val(map.PkgUnit);
                $("#Gw").val(map.Gw);
                $("#Gwu").val(map.Gwu);
                $("#Cbm").val(map.Cbm);
                $("#Cw").val(map.Cw);
                $("#Cmp").val(map.Cmp);
                $("#TranType").val(map.TranType);
                if (IOFlag == "O") {
                    $("#LspNo").val(LspNo);
                    $("#LspNm").val(LspNm);
                }
            }
            getSelectOptions(true);
        }, "JSON");
    }));

    registBtnLookup($("#LspNoLookup"), {
        item: "#LspNo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
            $("#Receiver").val(map.PartyNo);
            $("#ReceiverNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            if (IoFlag !== "I")
                return "PARTY_NO='" + cmp + "'";
            return "";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNo").val(rd.PARTY_NO);
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#BillToLookup"), {
        item: "#BillTo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#BillTo").val(map.PartyNo);
            $("#BillNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            if (IoFlag !== "I")
                return "PARTY_NO='" + cmp + "'";
            return "";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#BillTo").val(rd.PARTY_NO);
        $("#BillNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#ReceiverLookup"), {
        item: "#Receiver", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Receiver").val(map.PartyNo);
            $("#ReceiverNm").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            if (IoFlag !== "I")
                return "PARTY_NO='" + cmp + "'";
            return "";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
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

    registBtnLookup($("#BankInfoLookup"), {
        item: "#BankInfo", url: rootPath + LookUpConfig.BankInfoUrl, config: LookUpConfig.BankInfoLookup, param: "", selectRowFn: function (map) {
            $("#BankInfo").val(map.BankInfo);
            $("#CollectBank").val(map.CollectBank);
            $("#AccountName").val(map.AccountName);
            $("#SwiftCode").val(map.SwiftCode);
            $("#BankType").val(map.BankType);
        }
    }, {
        baseConditionFunc: function () {
            var con = "CMP='" + $("#LspNo").val() + "'";
            var cur = $("#Cur").val();
            if (cur != "") {
                con += " AND CRNCY='" + cur + "'";
            }
            return con;
        }
    }, null);


    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + "Common/GetCompanyData", config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
            getSelectOptions(true);
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
        getSelectOptions(true);
        //elem.val(rd.NAME);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
            $("#CurrencyTo").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
        $("#CurrencyTo").val(rd.CUR);
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

    _handler.beforLoadView = function () {
        var keys = ["DebitNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }

        var requires = ["DebitDate", "Cmp", "Cur", "CollectBank", "PayStartDate", "PayTerm"];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        var readonlys = ["Amt", "Qamt", "SubAmt"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        }
        SetDateReadonlys(true);
    }

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

    MenuBarFuncArr.AddMenu("SEND_BTN", "glyphicon glyphicon-bell", _getLang("L_ActManage_Send", '发送'), function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        var Status = $("#Status").val();
        var BillTo = $("#BillTo").val();
        var CollectBank = $("#CollectBank").val();
        var PayDate = $("#PayDate").val();

        if (BillTo == "" || CollectBank == "" || PayDate == "") {
            alert(_getLang("L_ActSetup_Script_71", '不能发送'));
            return;
        }

        if (Status == "D" || Status == "E" || Status == "F" || Status == "V" || Status == "R") {
            alert(_getLang("L_ActSetup_Script_72", '作废后就不能修改'));
            return;
        }
        var postData = { UId: $("#UId").val(), TpvInvNo: $("#TpvDebitNo").val(), Cmp: $("#Cmp").val() };
        $.ajax({
            url: rootPath + "ActManage/CheckInvoice",
            type: 'POST',
            data: postData,
            async: false,
            dataType: "JSON",
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            success: function (data) {
                CommonFunc.ToogleLoading(false);
                if (data.result) {
                    data.msg = data.msg.replace(new RegExp("<br/>", 'g'), "\n");
                    if (data.msg == "") {
                        SendAct(postData);
                    } else if (confirm(data.msg + "," + _getLang("L_RQSetup_Scripts_329", '是否继续操作')))
                        SendAct(postData);
                }
                else {
                    alert(data.msg);
                }
            },
            cache: false
        });
    });


    MenuBarFuncArr.MBInvalid = function () {
        if (!confirm(_getLang("L_ActSetup_Script_73", '确定要作废吗'))) {
            return;
        }
        var uid = $("#UId").val();
        if (!uid) {
            alert(_getLang("L_ActSetup_Script_74", '此账单未保存'));
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "ActManage/doInvalid",
            type: 'POST',
            data: { UId: uid },
            dataType: "json",
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "warning");
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {

                if (result.message != "success") {
                    CommonFunc.Notify("", result.message, 500, "warning");
                    CommonFunc.ToogleLoading(false);
                    return;
                }

                $("#Status").val("V");
                CommonFunc.Notify("", _getLang("L_ActSetup_Scripts_76", '作废成功'), 500, "success");
                statusIsInvalid();
                CommonFunc.ToogleLoading(false);
            }
        });
    }


    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBCopy", "MBSearch", "MBInvalid"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.MBDel = function () {
        if (_handler.beforDel && _handler.beforDel() === false) {
            return false;
        }
        var changeData = getAllKeyValue();//获取所有主键值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        CommonFunc.ToogleLoading(true);
        var delSuc = false;
        $.ajax({
            async: false,
            url: _handler.saveUrl,
            type: 'POST',
            data: data,
            dataType: "json",
            beforeSend: function () {
                CommonFunc.ToogleLoading(true);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                delSuc = result.IsDel;
                if (result.IsDel) {
                    _handler.setFormData([{}]);
                    CommonFunc.Notify("", result.message, 1000, "warning");
                    CommonFunc.ToogleLoading(false);
                    return true;
                }
                else {
                    CommonFunc.Notify("", result.message, 1000, "warning");
                    CommonFunc.ToogleLoading(false);
                    return false;
                }
            }
        });
        return delSuc;
    }
    //MenuBarFuncArr.Enabled(["MBEdoc"]);
    //getSelectOptions();
    getSelectOptions();

    $("#TranType").on("change", function () {
        if (typeof idsOfSelectedRows != "undefined") {
            idsOfSelectedRows["lookupGrid_getChg_undefined"] = [];
        }
    });

    $("#SmIdSearch").keypress(function (event) {
        $("#ChgCdSearch").val(""); //BlSearch
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

    $("#BillTo").change(function () {
        var billTo = "";
        $('#BillTo :selected').each(function (i, selected) {
            billTo = selected.value;
        });
        SetStn(billTo);
    });

    //$("#DebitType").change(function () {
    //    if (!isChinaFactory()) {
    //        return;
    //    }
    //    var debitType = "";
    //    $('#DebitType :selected').each(function (i, selected) {
    //        debitType = selected.value;
    //    });
    //    var cur = $("#Cur").val("");
    //    if (debitType == "T") {
    //        $("#Cur").val("CNY");
    //    } else if (debitType == "T" && cur == "CNY") {
    //        $("#Cur").val("");
    //    }
    //});

    //$("#Cur").change(function () {
    //    if (!isChinaFactory()) {
    //        return;
    //    }
    //    var cur = $("#Cur").val();
    //    if (cur == "") {
    //        return;
    //    } else if (cur == "CNY") {
    //        $('#DebitType').val("T");
    //    } else {
    //        $('#DebitType').val("V");
    //    }
    //});
});

function getSelectOptions(autoset) {
    var location = $("#Cmp").val();
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("ActSetup"), location: location },
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
            if (autoset == true) {
                var billTo = "";
                if (tdltOptions.length > 0) {
                    $("#BillTo").val(tdltOptions[0].cd)
                    billTo = tdltOptions[0].cd;
                };
                SetStn(billTo);
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
                var val = '';
                var strs = [];
                $('#ChgType :selected').each(function (i, selected) {
                    strs[i] = selected.text;
                });
                if (strs.length > 0)
                    val = strs[0];
                $("#Remark").val(val);
            }
        }
    });
}

function statusIsInvalid() {
    MenuBarFuncArr.Disabled(["MBEdoc", "MBEdit", "MBCopy", "MBDel", "MBSave", "MBCancel", "MBInvalid", "SEND_BTN", "UPLOAD_BTN"]);
}

function AddMBPrintFunc() {
    var listBar = [];
    /*listBar.push({
        menuId: "RFB01", menuName: "请款单", menuFunc: function () {
            var DebitNo = $("#DebitNo").val();
            //var shipments = $("#ShipmentId").val();
            if (!DebitNo) {
                alert("该笔资料没有请款单！");
                return;
            }
            var params = {
                //currentCondition: "",
                //val: dnNo,
                debitno: DebitNo,
                rptdescp: "请款单",
                rptName: "RFB01",
                formatType: 'xls',
                exportType: 'PREVIEW',
            };
            showReport(params);
        }, menuCss: "glyphicon glyphicon-list-alt"
    });*/

    $("#dialog_saveBtn").click(function () {
        doDownloadExcel();
    });

    //listBar.push({
    //    menuId: "RFB02", menuName: _getLang("L_ActCheckSetup_Script_8", '下载请款excel'), menuFunc: function () {
    //        var DebitNo = $("#DebitNo").val();
    //        //var shipments = $("#ShipmentId").val();
    //        if (!DebitNo) {
    //            alert(_getLang("L_ActCheckSetup_Script_7", ''));
    //            return;
    //        }
    //        $("#modifyDialog").modal("show");
    //    }, menuCss: "glyphicon glyphicon-list-alt"
    //});
    MenuBarFuncArr.AddDDLMenu("MBPreview", " glyphicon glyphicon-print", _getLang("L_ActManage_Preview", '报表预览'),function () { }, null, listBar);

    MenuBarFuncArr.AddMenu("MBVatNo", "glyphicon glyphicon-bell", _getLang("L_ActSetup_Views_160", '发票号码输入'), function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "E") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_62", '请款中，不得修改'), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "F") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_63", '已付款，不得修改'), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Script_64", '已作废，不得修改'), 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "R") {
            CommonFunc.Notify("", _getLang("L_ActSetup_Reject", '已驳回，不得修改'), 500, "warning");
            return false;
        }

        $("#ckNo_modifyDialog").modal("show");
    });

    $("#ckno_saveBtn").click(function () {

        var uid = $("#UId").val(), checkNo = $("#txt_newCheckNo").val();
        if (isEmpty(checkNo)) {
            alert(_getLang("L_ActSetup_Scripts_77", '请输入发票号码'));
            return;
        }
        var params = { id: uid, no: checkNo };
        $.ajax({
            async: true,
            url: rootPath + "ActManage/PutCheckNo",
            type: 'POST',
            data: params,
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                CommonFunc.Notify("", result.message, 500, "success");
                $("#ckNo_modifyDialog").modal("hide");
                MenuBarFuncArr.MBCancel();
            }
        });
    });
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
        alert(_getLang("L_ActCheckSetup_Script_9", '暂无请款！'));
        return;
    }

    if (transType === "TTTTTT") {
        chgTypeStr = _getLang("L_ActCheckSetup_Script_10", '费用代码；费用描述；金额');
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

    var colModel = _getColModel(transType, undefined, true);
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

function getBankInfo(cmp) {
    $.ajax({
        async: true,
        url: rootPath + "COMMON/GetBankInfo",
        type: 'POST',
        data: { "cmp": cmp },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
        },
        "error": function (xmlHttpRequest, errMsg) {
            return "";
        },
        success: function (result) {
            if (result[0].success) {
                $("#CollectBank").val(result[0].CollectBank);
                $("#AccountName").val(result[0].AccountName);
                $("#BankInfo").val(result[0].BankInfo);
                $("#SwiftCode").val(result[0].SwiftCode);
                $("#BankType").val(result[0].BankType);
                $("#Cur").val(result[0].Crncy);
                $("#DebitType").val(result[0].DebitType);
            }
            return "";
        }
    });
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

function SendAct(postData) {
    $.ajax({
        url: rootPath + "ActManage/sendAct",
        type: 'POST',
        data: postData,
        async: false,
        dataType: "JSON",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        success: function (data) {
            CommonFunc.ToogleLoading(false);
            $("#Status").val("D");
            var formData = $.parseJSON(data.returnData.Content);
            console.log(formData);
            _handler.setFormData(formData);
            CommonFunc.Notify("", _getLang("L_BookingQuery_Views_271", ''), 500, "success");
        },
        cache: false
    });
}

function SetStn(billTo) {
    $("#Stn").val("");
    var cmp = $("#Cmp").val();
    if (billTo != "") {
        $.ajax({
            async: true,
            url: rootPath + "ActManage/GetBscodeArcd",
            type: 'POST',
            data: { cdType: "TDLT", cd: billTo, cmp: cmp },
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                $("#Stn").val(result.message);
            }
        });
    }
}

function Mul(a, b) {
    return new Decimal(a).mul(new Decimal(b)).toNumber();
}

function toFixed(num, s) {
    var times = Math.pow(10, s);
    var des = num * times + 0.5;
    des = parseInt(des, 10) / times;
    return des + '';
}