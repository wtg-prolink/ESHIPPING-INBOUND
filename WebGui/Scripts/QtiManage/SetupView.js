var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $MainGrid, $SubGrid;
var IoFlag = getCookie("plv3.passport.ioflag");
var viewable = false;
var editable = true;
_handler.key = "UId";
var _fields = [];
var colModel = [];
$(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    _handler.saveUrl = rootPath + "QtiManage/UpdateDataNew";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = {};

    _handler.addData = function () {
        //初始化新增数据
        var data = {};

        setFieldValue([data]);
        $MainGrid.jqGrid("clearGridData");
    }

    _handler.saveData = function (dtd) {
        var saveData = [];

        var allData = $MainGrid.find("tr");
        $.each(allData, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") 
            {
                var IType = getGridVal($MainGrid, val.id, "IType", null);
                var ChgDayType= getGridVal($MainGrid, val.id, "ChgDayType", null);
                var ChgCd = getGridVal($MainGrid, val.id, "ChgCd", null);
                var CarrierCd = getGridVal($MainGrid, val.id, "CarrierCd", null);
                var CarrierNm = getGridVal($MainGrid, val.id, "CarrierNm", null);
                var SDay = getGridVal($MainGrid, val.id, "SDay", null);
                var EDay = getGridVal($MainGrid, val.id, "EDay", null);
                var CntType = getGridVal($MainGrid, val.id, "CntType", null);
                var CntDescp = getGridVal($MainGrid, val.id, "CntDescp", null);
                var FeePerDay = getGridVal($MainGrid, val.id, "FeePerDay", null);
                var Cur = getGridVal($MainGrid, val.id, "Cur", null);
                var EmptyReturn = getGridVal($MainGrid, val.id, "EmptyReturn", null);
                var CalType = getGridVal($MainGrid, val.id, "CalType", null);
                var Percentage = getGridVal($MainGrid, val.id, "Percentage", null);
                var FobCif = getGridVal($MainGrid, val.id, "FobCif", null);
                var CalDate = getGridVal($MainGrid, val.id, "CalDate", null);
                var EffectDate = getGridVal($MainGrid, val.id, "EffectDate", null);
                var ExpiratDate = getGridVal($MainGrid, val.id, "ExpiratDate", null);
                var LspNo = getGridVal($MainGrid, val.id, "LspNo", null);
                var LspNm = getGridVal($MainGrid, val.id, "LspNm", null);
                var rowUId = getGridVal($MainGrid, val.id, "UId", null);
                var Data = {
                    "IType": IType,
                    "ChgDayType": ChgDayType,
                    "ChgCd": ChgCd,
                    "CarrierCd": CarrierCd,
                    "CarrierNm": CarrierNm,
                    "SDay": SDay,
                    "EDay": EDay,
                    "CntType": CntType,
                    "CntDescp": CntDescp,
                    "FeePerDay": FeePerDay,
                    "Cur": Cur,
                    "EmptyReturn": EmptyReturn,
                    "CalType": CalType,
                    "Percentage": Percentage,
                    "FobCif": FobCif,
                    "CalDate": CalDate,
                    "EffectDate": EffectDate,
                    "ExpiratDate": ExpiratDate,
                    "LspNo": LspNo,
                    "LspNm": LspNm,
                    "UId": rowUId,
                    "__state": 1
                };

                saveData.push(Data);
            }
        });

        var changeData = {mt: []};
        changeData["mt"] = saveData;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["Cmp"] = encodeURIComponent($("#Cmp").val());
        data["PodCd"] = encodeURIComponent($("#PodCd").val());
        data["TerminalCd"] = encodeURIComponent($("#TerminalCd").val());
        data["TranType"] = encodeURIComponent($("#TranType").val());
        data["ShareTo"] = encodeURIComponent($("#ShareTo").val());
        data["MapUId"] = encodeURIComponent(_uid);
        console.log(changeData);
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

    _handler.delData = function () {
        var changeData = getAllKeyValue();//获取所有主键值
        //var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        //data["Cmp"] = encodeURIComponent($("#Cmp").val());
        //data["PodCd"] = encodeURIComponent($("#PodCd").val());
        //data["TerminalCd"] = encodeURIComponent($("#TerminalCd").val());
        //data["TranType"] = encodeURIComponent($("#TranType").val());
        var saveData = [];
        var allData = $MainGrid.find("tr");
        $.each(allData, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                var rowUId = getGridVal($MainGrid, val.id, "UId", null);
                var Data = {
                    "UId": rowUId,
                    "__state": 0
                };
                saveData.push(Data);
            }
        });
        var mtData = { "mt": saveData };
        var data = { "changedData": JSON.stringify(mtData), autoReturnData: false };
        ajaxHttp(_handler.saveUrl, data,
            function (result) {
                if (result.message) {
                    CommonFunc.Notify("", result.message, 1000, "warning");
                    return false;
                }
                else if (_handler.setFormData) {
                    _handler.setFormData([{}]);
                }
                return true;
            });
    }

    _handler.setFormData = function (data) {
        console.log(data);
        if (data["main"]) {
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        }
        else {
            _handler.topData = [{}];
        }
        if (data["items"])
            _handler.loadGridData("MainGrid", $MainGrid[0], data["items"], [""]);
        else
            _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);

        var col = $MainGrid.jqGrid('getCol', 'Status', false);//获取批文号码列的值
        $.each(col, function (index, colname) {
            if (colname == "N") {
                $MainGrid.jqGrid('setRowData', index + 1, false, 'gridTagClass');
            }
        });
        setFieldValue(data["main"] || [{}]);

        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBEdoc", "MBInvalid"]);

        if (typeof data["main"][0] !== "undefined") {
            //if (!isEmpty(data["main"][0].FromCmp)) {
            //    MenuBarFuncArr.Disabled(["MBDel"]);
            //}
        }
        _uid = "";
        var allData = $MainGrid.find("tr");
        $.each(allData, function (i, val) {
            if (typeof val.id != "undefined" && val.id != "") {
                var rowUId = getGridVal($MainGrid, val.id, "UId", null);
                _uid = _uid + rowUId + ";";
            }
        });
        _handler.topData["UId"] = _uid;
        var keys = ["LspNo", "LspNm", "CarrierCd", "CalType", "Cur", "CntType", "CntDescp",
            "CalDate", "EffectDate", "ExpiratDate","ChgDayType"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).val("");
        }
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _handler.loadMainData = function (map) {
        console.log(_handler.key);
        console.log(map);
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "QtiManage/GetDetail", { UId: map.UId, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforSave = function () {
        return true;
    }

    function CheckEdit(isedit) {
        var fromcmp = $("#FromCmp").val();
        var podcd = $("#PodCd").val();
        var tipinfo = "delete!";
        if (isedit) {
            tipinfo = "edit!";
        }
        if (!isEmpty(fromcmp)) {
            alert("Has been shared POD:" + podcd + " Port Free Date Info from " + fromcmp + " site, so you cann't " + tipinfo);
            return false;
        }
    }

    _handler.beforEdit = function () {
        return CheckEdit(true);
    }

    _handler.beforDel = function () {
        return CheckEdit(false);
    }


    $MainGrid = $("#MainGrid");

    var colModel = [];

    genMainGrid();

    $("#TranType").change(function(){
        $MainGrid.jqGrid("clearGridData");
    });

    _handler.beforLoadView = function () {

    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc", "MBCopy", "MBInvalid"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }


    registBtnLookup($("#PodCdLookup"), {
        item: "#PodCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
            setGridValue("Cur", map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
        setGridValue("Cur", rd.CUR);
    }));

    registBtnLookup($("#LspNoLookup"), {
        item: '#LspNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName3);
            setGridValue("LspNo", map.PartyNo);
            setGridValue("LspNm", map.PartyName3);
        }
    }, {
        baseConditionFunc: function () {
            return " (PARTY_TYPE LIKE 'SP;%' OR PARTY_TYPE LIKE '%;SP%' OR PARTY_TYPE='SP')";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNo").val(rd.PARTY_NO);
        $("#LspNm").val(rd.PARTY_NAME3);
        setGridValue("LspNo", rd.PARTY_NO);
        setGridValue("LspNm", rd.PARTY_NAME3);
    }, function () {
        $("#LspNo").val("");
        $("#LspNm").val("");
    }));

    function CheckPodInfo(podcd, cmp) {
        if (!isEmpty(cmp) && !isEmpty(podcd)) {
            $.ajax({
                async: false,
                url: rootPath + "QtiManage/CheckPod",
                type: 'POST',
                data: { Cmp: cmp, PodCD: podcd },
                "error": function (xmlHttpRequest, errMsg) {
                    console(errMsg);
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                success: function (data) {
                    if (!(isEmpty(data.FromCmp))) {
                        alert("Has been shared POD:" + podcd + " Port Free Date Info from " + data.FromCmp+" site, so you cann't add!");
                        $("#PodCd").val("");
                    }
                }
            });
        }
    }

    $('#PodCd').change(function () {
        var podcd = $(this).val();
        var cmp = $('#Cmp').val();
        CheckPodInfo(podcd, cmp);
    });

    $('#Cmp').change(function () {
        var podcd = $('#PodCd').val();
        var cmp = $(this).val();
        CheckPodInfo(podcd, cmp);
    });
    $('#CalDate').change(function () {
        setGridValue("CalDate", $(this).val());
    });
    $('#EffectDate').change(function () {
        setGridValue("EffectDate", $(this).val());
    });
    $('#ExpiratDate').change(function () {
        setGridValue("ExpiratDate", $(this).val());
    });
    $('#CalType').change(function () {
        setGridValue("CalType", $(this).val());
    }); 
    $('#ChgDayType').change(function () {
        setGridValue("ChgDayType", $(this).val());
    });

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

    registBtnLookup($("#CmpLookup"), {
        item: "#Cmp", url: rootPath + "TPVCommon/GetSiteCmpData", config: LookUpConfig.SiteLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cd);
        }
    }, { baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'" }, GetSiteCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
    }, function ($grid, elem, rowid) {
        $("#Cmp").val("");
    }));

    registBtnLookup($("#ShareToLookup"), {
        isMutiSel: true,
        item: "#ShareTo", url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.MutiLocationLookup, param: "", selectRowFn: function (map) {
            $("#ShareTo").val(map.Cmp);
        }
    }, {
        baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'",
            responseMethod: function (data) {
                console.log(data);
                var str = "";
                $.each(data, function (index, val) {
                    str = str + data[index].Cmp + ";";
                });
                $("#ShareTo").val(str);
            }
    });

    registBtnLookup($("#CntTypeLookup"), {
        isMutiSel: true,
        item: "#CntType", url: rootPath + LookUpConfig.MutiCntrTypeUrl, config: LookUpConfig.MutiCntrTypeLookup, param: "", selectRowFn: function (map) {
            $("#CntType").val(map.ChgCd);
            $("#CntDescp").val(map.ChgDescp);
            setGridValue("CntType", map.ChgCd);
            setGridValue("CntDescp", map.ChgDescp);
        }
    }, {
        baseCondition: "",
        responseMethod: function (data) {
            var str = "";
            var descp = "";
            $.each(data, function (index, val) {
                str = str + data[index].ChgCd + ";";
                descp = descp + data[index].ChgDescp + ";";
            });
            $("#CntType").val(str);
            $("#CntDescp").val(descp);
            setGridValue("CntType", str);
            setGridValue("CntDescp", descp);
        }
    }, {
        autoClearFunc: function (data) {
            $("#CntType").val("");
            $("#CntDescp").val("");
        }
    });

    registBtnLookup($("#CarrierCdLookup"), {
        item: '#CarrierCd', url: rootPath + LookUpConfig.RCARUrl, config: LookUpConfig.BSCodeLookup, param: "",
        selectRowFn: function (map) {
            $("#CarrierCd").val(map.Cd);
            setGridValue("CarrierCd", map.Cd);
        }
    }, {
        baseConditionFunc: function () {
            var tranType = $("#TranType").val();
            var condition = "CD_TYPE='TCAR'";
            if (tranType == "R") {
                condition = "CD_TYPE='RCAR'";
            }
            return condition;
        }
    }, LookUpConfig.GetCodeTypeAuto1(groupId, undefined, function ($grid, rd, elem) {
        $("#CarrierCd").val(rd.CD);
        setGridValue("CarrierCd", rd.CD);
    }));

    setBscData("TerminalCdLookup", "TerminalCd", "TerminalNm", "TMN")
});




    function setBscData(lookUp, Cd, Nm, pType)
    {
        //SMPTY放大鏡
        options = {};
        options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
        options.registerBtn = $("#"+lookUp);
        options.focusItem = $("#" + Cd);
        options.baseCondition = " GROUP_ID='"+groupId+"' AND CMP='"+cmp+"' AND CD_TYPE='"+pType+"'";
        options.baseConditionFunc = function(){
            return " AND AP_CD='"+$("#PodCd").val()+"'";
        }
        options.isMutiSel = true;
        options.gridFunc = function (map) {
            $("#" + Cd).val(map.Cd);

            if(Nm != "")
                $("#" + Nm).val(map.CdDescp);
        }

        options.lookUpConfig = LookUpConfig.BSCodeLookup;
        initLookUp(options);

        CommonFunc.oAutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
            $(this).val(ui.item.returnValue.CD);

            if(Nm != "")
                $("#" + Nm).val(ui.item.returnValue.CD_DESCP);

            return false;
        });
    }

function statusIsInvalid() {
    MenuBarFuncArr.Disabled(["MBEdoc", "MBEdit", "MBDel", "MBSave", "MBCancel", "MBInvalid", "SEND_BTN"]);
}

function getcust(name) {
    var _name = name;
    var url = rootPath + LookUpConfig.RCARUrl;
    var unit_op = getLookupOp("MainGrid",
        {
            url: url,
            config: LookUpConfig.BSCodeLookup,
            returnFn: function (map, $grid) {
                //setGridVal($MainGrid, rowid, "CarrierNm", map);
                return map.Cd;
            }, baseConditionFunc: function () {
                var tranType = $("#TranType").val();
                var condition = "CD_TYPE='TCAR'";
                if (tranType == "R") {
                    condition = "CD_TYPE='RCAR'";
                }
                return condition;
            }
        }, LookUpConfig.GetCodeTypeAuto1(groupId, $MainGrid,
            function ($grid, rd, elem, rowid) {
                $(elem).val(rd.CD);
            },
            function () {
                var tranType = $("#TranType").val();
                var condition = "CD_TYPE=TCAR";
                if (tranType == "R") {
                    condition = "CD_TYPE=RCAR";
                }
                return condition;
            }),
        {
                param: "1=1", baseConditionFunc: function() {
                    var tranType = $("#TranType").val();
                    var condition = "CD_TYPE='TCAR'";
                    if (tranType == "R") {
                        condition = "CD_TYPE='RCAR'";
                    }
                    return condition;
                }
        });
    return unit_op;
}

function getPartyop() {
    var party_op = getLookupOp("MainGrid",
        {
            url: rootPath + LookUpConfig.PartyNoUrl,
            config: LookUpConfig.PartyNoLookup,
            returnFn: function (map, $grid) {
                var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
                setGridVal($grid, selRowId, 'LspNo', map.PartyNo, "lookup");
                setGridVal($grid, selRowId, 'LspNm', map.PartyName3, null);
                return map.PartyNo;
            }
        }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
            var selRowId = rowid;
            $grid = $("#MainGrid");
            setGridVal($grid, selRowId, 'LspNo', rd.PARTY_NO, null);
            setGridVal($grid, selRowId, 'LspNm', rd.PARTY_NAME3, null);
            $(elem).val(rd.PARTY_NO);
        }, function ($grid, rd, elem, rowid) {
            $grid = $("#MainGrid");
            var selRowId = $grid.jqGrid('getGridParam', 'selrow');
            setGridVal($grid, selRowId, 'LspNo', "", null);
            setGridVal($grid, selRowId, 'LspNm', "", null);
        }), {
        param: "",
        baseConditionFunc: function () {
            return " (PARTY_TYPE LIKE 'SP;%' OR PARTY_TYPE LIKE '%;SP%' OR PARTY_TYPE='SP')";
        }
    });
    return party_op;
}

function getcur(name) {
    var _name = name;
    var cur_op = getLookupOp("MainGrid",
        {
            url: rootPath + LookUpConfig.MutiCntrTypeUrl,
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

function getCntrop() {
    var expreasongrid = "MainGrid";
    var cntr_op = getLookupOp("MainGrid",
        {
            url: rootPath + LookUpConfig.MutiCntrTypeUrl,
            config: LookUpConfig.MutiCntrTypeLookup,
            returnFn: function (map, $grid) {
                var selRowId = $grid.jqGrid('getGridParam', 'selrow');//selrow
                setGridVal($grid, selRowId, 'CntType', map.ChgCd, "lookup");
                setGridVal($grid, selRowId, 'CntDescp', map.ChgDescp, null);
                return map.ChgCd;
            }
        },
        {
            responseMethod: function (data) {
                var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
                var str = "";
                var descp = "";
                $.each(data, function (index, val) {
                    str = str + data[index].ChgCd + ";";
                    descp = descp + data[index].ChgDescp + ";";
                });
                setGridVal($("#MainGrid"), selRowId, 'CntType', str, "lookup");
                setGridVal($("#MainGrid"), selRowId, 'CntDescp', descp, null);
            }, autoClearFunc: function(data) {
                var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
                setGridVal($("#MainGrid"), selRowId, 'CntType', "", null);
                setGridVal($("#MainGrid"), selRowId, 'CntDescp', "", null);
            }
        });
    return cntr_op;
}



function genMainGrid() {
    var colModelSetting = [
        { name: 'UId', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'Cmp', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'PodCd', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'TranType', editable: false, hidden: true, width: 50, order: 0 },
        { name: 'IType', editable: true, hidden: false, width: 100, order: 1, formatter: "select", editoptions: { value: 'DSTF:Storage;DDEM:Demurrage;DDET:Detention;BOTH:DEM\/DET;USAGE:Usage' }, edittype: 'select' },
        { name: 'LspNo', index: 'Forwarder No', editable: true, hidden: false, editoptions: gridLookup(getPartyop()), edittype: 'custom', sorttype: 'string', width: 100, order: 2 },
        { name: 'LspNm', index: 'Forwarder Name', editable: false, hidden: false, width: 100, order: 3 },
        { name: 'ChgCd', editable: true, hidden: true, width: 100, order: 33 },
        { name: 'CarrierCd', index: 'Carrier', editoptions: gridLookup(getcust("CarrierCd")), edittype: 'custom', sorttype: 'string', width: 120, hidden: false, editable: true, order: 4 },
        //{ name: 'CarrierCd', editable: true, hidden: false, width: 150, order: 4 },
        { name: 'CarrierNm', editable: false, hidden: true, width: 150, order: 55 },
        { name: 'CntType', editable: true, hidden: false, editoptions: gridLookup(getCntrop()), edittype: 'custom', sorttype: 'string', width: 100, order: 6 },
        { name: 'CntDescp', index: 'Container Type Name', editable: false, hidden: false, width: 100, order: 7 },
        { name: 'ChgDayType', title: 'Charged Days Type', index: 'ChgDayType',  editable: true, hidden: false, width: 150, order: 1, formatter: "select", editoptions: { value: 'C:Calendar Day;W:Working Day' }, edittype: 'select' },
        { name: 'SDay', editable: true, hidden: false, width: 100, order: 8 },
        { name: 'EDay', editable: true, hidden: false, width: 100, order: 9 },
        { name: 'Cur', index: 'CUR', editoptions: gridLookup(getcur("Cur")), edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true, order: 10 },
        { name: 'FeePerDay', editable: true, hidden: false, width: 100, order: 11 },
        { name: 'Percentage', editable: true, hidden: false, width: 100, order: 12 },
        { name: 'TerminalCd', editable: false, hidden: true, width: 100, order: 99 },
        { name: 'EmptyReturn', editable: false, hidden: true, width: 150, order: 99, formatter: "select", editoptions: { value: BscodeSel }, edittype: 'select' },
        { name: 'CalType', editable: true, hidden: false, width: 130, formatter: "select", editoptions: { value: 'C:Summation;F:Constant' }, edittype: 'select' },
        { name: 'FobCif', editable: true, hidden: false, width: 100, formatter: "select", editoptions: { value: ':;F:FOB;C:CIF;B:FOB&CIF' }, edittype: 'select' },
        { name: 'GroupId', hidden: true },
        { name: 'OrderSeq', hidden: true },
        { name: 'Stn', hidden: true },
        { name: 'Dep', hidden: true },
        { name: 'CalDate', editable: true, hidden: false, width: 100, formatter: "select", editoptions: { value: 'D:ATD;A:ATA' }, edittype: 'select' },
        {
            name: 'EffectDate', editable: true, sorttype: 'string', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, align: "left",
            formatter: 'date', editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d'
            }
        },
        {
            name: 'ExpiratDate', editable: true, sorttype: 'string', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, align: "left",
            formatter: 'date', editoptions: myEditDateInit,
            formatoptions: {
                srcformat: 'ISO8601Long',
                newformat: 'Y-m-d'
            }
        }
    ]
    
    genColModel("SMQTI", "U_ID", "L_SMQTI", colModelSetting).done(function (result) {
        colModel = result;
        _handler.intiGrid("MainGrid", $MainGrid, {
            colModel: colModel, caption: jslang["L_SMQTI_List"], delKey: ["UId"], footerrow: false,
            onAddRowFunc: function (rowid) {
                if ($("#Cmp").val() == "") {
                    alert(jslang["L_SetupView_Msg1"]);
                    $MainGrid.jqGrid('delRowData', rowid);
                    return false;
                }

                if ($("#PodCd").val() == "") {
                    alert(jslang["L_SetupView_Msg2"]);
                    $MainGrid.jqGrid('delRowData', rowid);
                    return false;
                }

                if ($("#TranType").val() == "" || $("#TranType").val() ==null) {
                    alert(jslang["L_SetupView_Msg3"]);
                    $MainGrid.jqGrid('delRowData', rowid);
                    return false;
                }

                setGridVal($MainGrid, rowid, "Cmp", $("#Cmp").val());
                setGridVal($MainGrid, rowid, "PodCd", $("#PodCd").val());
                setGridVal($MainGrid, rowid, "TranType", $("#TranType").val());
            },
            beforeSelectRowFunc: function (rowid) {
            },
            afterSaveCellFunc: function (rowid) {
            },
            loadComplete: function (data) {

            }
        });
    });
}

function setGridValue(colId,val) {
    var ids = $MainGrid.jqGrid('getDataIDs');
    for (var i = 0; i < ids.length; i++) {
        setGridVal($MainGrid, ids[i], colId, val);
    }
}

