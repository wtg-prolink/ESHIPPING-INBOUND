var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

var noteditInfoPms = false;
var rqst_fwd_pms = false;
$(function () {
    var pmsIds = ["RQST_FWD", "RQST_SEND", "RQST_QDET", "RQST_PCAL", "RQST_PDET", "RQST_BNOT", "RQST_NNOT", "RQST_FBNOT"];
    for (var i = 0; i < pmsIds.length; i++) {
        pmsBtnCheck(pmsIds[i]);
    }
    if (pmsList.indexOf("NOTEDIT_INFO") == -1) {
        noteditInfoPms = true;
    }
    if (pmsList.indexOf("RQST_FWD") > -1) {
        rqst_fwd_pms = true;
    }

    _handler.afterEdit = function () {
        if (noteditInfoPms == false)
            setdisabled(true);
        if (rqst_fwd_pms == false) {
            gridEditableCtrl({ editable: false, gridId: "MainGrid" });
        }
    }
    var $MainGrid = $("#MainGrid");
    var BookingLookup = {
        caption: "@Resources.Locale.L_BaseLookup_SerInquiry",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false },
            { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RfqFrom', title: '@Resources.Locale.L_RQQuery_RfqFrom', index: 'RfqFrom', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RfqTo', title: '@Resources.Locale.L_ActQuery_PayDate', index: 'RfqTo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RfqDate', title: '@Resources.Locale.L_QTQuery_QuotDate', index: 'RfqDate', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'TranMode', title: '@Resources.Locale.L_BaseLookup_TranMode', index: 'TranMode', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Rlocation', title: '@Resources.Locale.L_RQQuery_Rlocation', index: 'Rlocation', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'RlocationNm', title: '@Resources.Locale.L_BaseLookup_Nm', index: 'RlocationNm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'FreightTerm', title: '@Resources.Locale.L_BaseLookup_FreightPayer', index: 'FreightTerm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Incoterm', title: 'Incoterm', index: 'Incoterm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'ServiceMode', title: 'Server Mode', index: 'ServiceMode', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
            //,{ name: 'Remark', title: '備註', index: 'Remark', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }
        ]
    };

    _handler.saveUrl = rootPath + "RQManage/SaveRQSetupData";
    _handler.inquiryUrl = rootPath + "RQManage/GetRQSetupData";
    _handler.config = BookingLookup;

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "C") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_IqEnd", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] !== "A") {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_318", 500, "warning");
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var dep = getCookie("plv3.passport.dep"),ext = getCookie("plv3.passport.ext");
        var data = { "FreightTerm": "", "TranMode": "", Cur: "USD", "Period": "", "OutIn": "O", "FreightTerm": "PP", "Period": "R", "RfqFrom": getDate(0, "-"), RfqTo: getDate(5, "-"), Rlocation: cmp, RfqDate: getDate(5, "-"), CreateDep: dep, CreateExt: ext,Cmp:cmp };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        $("#Status").val("A");
        getAutoNo("RfqNo", "rulecode=RFQ_NO&cmp=" + cmp);
        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        $("#RFQ_INFO").show();
        $("#Rlocation").blur();
    }

    _handler.saveData = function (dtd) {
        var containerArray = $MainGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        changeData["sub"] = containerArray;
        for (var i = 0; i < changeData.length; i++) {
            if (changeData[i]["NotifyDate"] === undefined)
                changeData[i]["NotifyDate"] = "";
        }
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        //data["rfq_no"] = encodeURIComponent($("#RfqNo").val());
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

    _handler.beforEdit = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }

        //if (!_handler.topData["SysEdit"]) {
        //    CommonFunc.Notify("", "已超过询价到期日期", 500, "warning");
        //    return false;
        //}
        //if (_handler.topData["Status"] === "C") {
        //    CommonFunc.Notify("", "已询价结束", 500, "warning");
        //    return false;
        //}

        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
            return false;
        }
        SetBtnView(true);
    }

    _handler.beforAdd = function () {//新增前设定
        SetBtnView(true);
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
        $("#Period").triggerChange = false;
        $("#TranMode").triggerChange = false;
        _handler.beforLoadView();
        $("#Period").triggerChange = true;
        $("#TranMode").triggerChange = true;
        setdisabled(true);
        setToolBtnDisabled(true);
        SetBtnView(false);
        //MenuBarFuncArr.initEdoc($("#UId").val(), groupId, cmp, "*");
        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //var multiEdocData = [
        //    { jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': _handler.topData["Cmp"], 'STN': '*' },
        //    { jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': _handler.topData["Cmp"], 'STN': '*' },
        //    { jobNo: $("#UId").val(), 'GROUP_ID': _handler.topData["GroupId"], 'CMP': _handler.topData["Cmp"], 'STN': '*' }
        //];
        //MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);
        MenuBarFuncArr.Enabled(["MBEdoc"]);
        MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
        MenuBarFuncArr.Enabled(["BACK_BTN"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "RQManage/GetRQSetupDataItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforSave = function () {
        var $grid = $MainGrid;
        var nullCols = [], sameCols = [];
        nullCols.push({ name: "LspCd", index: 4, text: '@Resources.Locale.L_RQSetup_LspCd' });
        nullCols.push({ name: "LspNm", index: 5, text: '@Resources.Locale.L_RQSetup_LspNm' });
        //nullCols.push({ name: "NotifyDate", index: 7, text: '通知日期' });
        nullCols.push({ name: "NotifyGroup", index: 8, text: '@Resources.Locale.L_NoticeSetup_Role' });
        sameCols.push({ name: "LspCd", index: 4, text: '@Resources.Locale.L_RQSetup_LspCd' });
        return _handler.checkData($grid, nullCols, sameCols);
    }

    function getop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'LspNm', map.PartyName, null);
                    setGridVal($grid, selRowId, 'LspCd', map.PartyNo, "lookup");
                    setGridVal($grid, selRowId, 'NotifyGroup', map.PartyNo, null);
                    return map.PartyNo;

                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $MainGrid,
            function ($grid, rd, elem, selRowId) {
                setGridVal($grid, selRowId, 'LspNm', rd.PARTY_NAME, null);
                setGridVal($grid, selRowId, 'LspCd', rd.PARTY_NO, "lookup");
                setGridVal($grid, selRowId, 'NotifyGroup', rd.PARTY_NO, null);
                $(elem).val(rd.PARTY_NO);
            }), { param: "" });
        return city_op;
    }

    function getMailop(name) {
        var _name = name;
        var city_op = getLookupOp("MainGrid",
            {
                url: rootPath + LookUpConfig.PartyNoUrl,
                config: LookUpConfig.PartyNoLookup,
                returnFn: function (map, $grid) {
                    var selRowId = $grid.jqGrid('getGridParam', 'selrow');
                    setGridVal($grid, selRowId, 'NotifyGroup', map.PartyNo, null);
                    return map.PartyNo;
                }
            }, LookUpConfig.GetPartyNoAuto(groupId, $MainGrid,
                function ($grid, rd, elem) {
                    $(elem).val(rd.PARTY_NO);
                }));
        return city_op;
    }

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', width: 100, editable: false, hidden: true },
        { name: 'Bidder', title: '@Resources.Locale.L_RQSetup_Bidder', index: 'Bidder', sorttype: 'string', width: 100, hidden: false, editable: false, formatter: "select", edittype: 'select', editoptions: { value: "@Resources.Locale.L_RQSetup_Script_176", defaultValue: 'N' } },
        { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', sorttype: 'string', width: 100, hidden: false, editable: false, formatter: "select", edittype: 'select', editoptions: { value: "@Resources.Locale.L_RQSetup_Script_177", defaultValue: 'N' } },
        { name: 'LspCd', title: '@Resources.Locale.L_DRule_LspNo', index: 'LspCd', sorttype: 'string', editoptions: gridLookup(getop("LspCd")), edittype: 'custom', width: 100, hidden: false, editable: true },
        { name: 'LspNm', title: '@Resources.Locale.L_DRule_LspNm', index: 'LspNm', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'BestPrice', title: '@Resources.Locale.L_RQSetup_BestPrice', index: 'BestPrice', align: 'right', sorttype: 'string', formatter: 'integer', width: 100, hidden: false, editable: false },
        {
            name: 'NotifyDate', title: '@Resources.Locale.L_RQSetup_NotifyDate', index: 'NotifyDate',
            width: 150, align: 'left', sorttype: 'string',
            hidden: false, editable: false
        },
        //{ name: 'NotifyDate', title: '通知日期', index: 'NotifyDate', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'NotifyGroup', title: '@Resources.Locale.L_NoticeSetup_Role', index: 'NotifyGroup', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'SeqNo', title: '@Resources.Locale.L_NRSSetup_SeqNo', index: 'SeqNo', sorttype: 'string', width: 250, hidden: true, editable: false },
        { name: 'RfqNo', title: '@Resources.Locale.L_RQQuery_RfqNo', index: 'RfqNo', sorttype: 'string', width: 250, hidden: true, editable: false }
    ];

    _handler.intiGrid("MainGrid", $MainGrid, {
        multiselect: true,
        colModel: colModel, caption: 'RFQ', delKey: ["UId"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
            if (typeof maxSeqNo === "undefined")
                maxSeqNo = 0;
            $MainGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo + 1);
            var RfqNo = $("#RfqNo").val();
            $MainGrid.jqGrid('setCell', rowid, "RfqNo", RfqNo);
            $MainGrid.jqGrid('setCell', rowid, "Bidder", "N");
            $MainGrid.jqGrid('setCell', rowid, "Status", "N");
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

  
    registBtnLookup($("#IncotermLookup"), {
        item: "#Incoterm", url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Incoterm").val(map.Cd);
            //$("#IncotermDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        //$("#IncotermDescp").val(rd.CD_DESCP);
        $("#Incoterm").val(rd.CD);
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

    //registBtnLookup($("#RlocationLookup"), {
    //    item:"#Rlocation", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
    //        $("#Rlocation").val(map.PartyNo);
    //        $("#RlocationNm").val(map.PartyName);
    //    }
    //}, {
    //    //baseConditionFunc: function () { return "PARTY_TYPE='LC'"; }
    //},
    //LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
    //    $("#RlocationNm").val(rd.PARTY_NAME);
    //}));

    registBtnLookup($("#PolCdLookup"), {
        item: "#PolCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            $("#PolNm").val(map.PortNm);
            $("#PolCd1").val(map.CntryCd + map.PortCd);
            $("#PolNm1").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.CNTRY_CD+rd.PORT_CD);
        $("#PolNm").val(rd.PORT_NM);
        $("#PolCd1").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolNm1").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup"), {
        item: "#PodCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd);
            $("#PodNm").val(map.PortNm);
            $("#PodCd1").val(map.CntryCd + map.PortCd);
            $("#PodNm1").val(map.PortNm);
            $("#Region").val(map.CntryCd);
            
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
        $("#PodCd1").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PodNm1").val(rd.PORT_NM);
        $("#Region").val(rd.CNTRY_CD);
    }));

    registBtnLookup($("#PolCdLookup1"), {
        item: "#PolCd1", url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PolCd1").val(map.PortCd);
            $("#PolNm1").val(map.PortNm);
            $("#PolCd").val(map.PortCd);
            $("#PolNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.PORT_CD);
        $("#PolNm").val(rd.PORT_NM);
        $("#PolCd1").val(rd.PORT_CD);
        $("#PolNm1").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PodCdLookup1"), {
        item: "#PodCd1", url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PodCd1").val( map.PortCd);
            $("#PodNm1").val(map.PortNm);
            $("#PodCd").val(map.PortCd);
            $("#PodNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd1").val(rd.PORT_CD);
        $("#PodNm1").val(rd.PORT_NM);
        $("#PodCd").val(rd.PORT_CD);
        $("#PodNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#ServiceModeLookup"), {
        item: '#ServiceMode', url: rootPath + LookUpConfig.ServiceModeUrl, config: LookUpConfig.ServiceModeLookup, param: "", selectRowFn: function (map) {
            $("#ServiceMode").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "PK", undefined, function ($grid, rd, elem) {

    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    registBtnLookup($("#GwuLookup"), {
        item: '#Gwu', url: rootPath + LookUpConfig.NwuUrl, config: LookUpConfig.NwuLookup, param: "", selectRowFn: function (map) {
            $("#Gwu").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UT", undefined, function ($grid, rd, elem) {
        $("#Gwu").val(rd.CD);
    }, function ($grid, elem) {
        $("#Gwu").val("");
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.CountryUrl, config: LookUpConfig.CountryLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.CntryCd);
        }
    }, undefined, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CNTRY_CD);
    }, function ($grid, elem) {
        $("#Region").val("");
    }));

    /*
    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }
    */
    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        getAutoNo("RfqNo", "rulecode=RFQ_NO&cmp=" + cmp);
        var data = { "FreightTerm": _bu, "TranMode": _tran };
        data[_handler.key] = uuid();

        $("#RfqFrom").val(getDate(0, "-"));
        $("#RfqTo").val(getDate(5, "-"));
        $("#EffectFrom").val("");
        $("#EffectTo").val("");
        $("#CreateBy").val("");
        $("#CreateDate").val("");
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");

        $("#Status").val("A");
        /*var allData = $MainGrid.jqGrid("getGridParam", "data");
        $.each(allData, function (i, val) {
            val["_id_"] = "jqg" + i;
        });*/
      
       

        var dataRow, addData = [];
        var rowIds = $MainGrid.getDataIDs();
        for (var i = 0; i < rowIds.length; i++) {
            var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
            if (rowDatas["Bidder"] === "F" || rowDatas["Bidder"] === "B") {
                dataRow = {
                    UId: "",
                    Bidder: 'N',
                    Status: 'N',
                    LspCd: rowDatas.LspCd,
                    LspNm: rowDatas.LspNm,
                    BestPrice: '',
                    //NotifyDate: getDate(0),
                    NotifyGroup: rowDatas.NotifyGroup,
                    Remark: '',
                    SeqNo: '',
                    RfqNo: '',
                };
                addData.push(dataRow);
            }
        }

        _handler.loadGridData("MainGrid", $MainGrid[0], [], [""]);
        for (var i = 0; i < addData.length; i++) {
            $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        }
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
        SetBtnView(true);
    }

    _handler.beforLoadView = function () {
        var tranMode = $("#TranMode").val();
        if (tranMode === "T" || tranMode === "D") {
            $("#iport_city").show();
            $("#oport_city").hide();
        }
        else {
            $("#oport_city").show();
            $("#iport_city").hide();
        }

        $("#RFQ_INFO").show();
        var requires = ["RfqNo", "RfqFrom", "RfqTo", "EffectFrom", "EffectTo", "ServiceMode", "RfqDate", "Cur", "LoadingFrom", "LoadingTo", "TranMode"];
        if ($("#Period").val() === "B") {
            //requires = ["RfqNo", "RfqFrom", "RfqTo", "EffectFrom", "EffectTo", "ServiceMode", "RfqDate", "Incoterm", "Rlocation", "PolCd", "PodCd", "Cur"];
            $("#RFQ_INFO").hide();
        }
        if ($("#Period").val() === "R") {
            requires.push("PolCd");
            requires.push("PodCd");
        }

        if ($("#TranMode").val() === "E" && $("#Period").val() === "R") {
            requires.push("Region");
        }
        var initFild = ["Region", "PolCd", "PodCd"];
        for (var i = 0; i < initFild.length; i++) {
            $("#" + initFild[i]).removeAttr("required"); 
            $("[for=" + initFild[i] + "]").css("color", "black");
        }

        $("#RfqNo").attr('isKey', true);
        $("#RfqNo").attr('disabled', true);
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }

        var readonlys = ["Status"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
            //$("#" + disableds[i]).attr('isKey', true);
        }
    }
    /*
    MenuBarFuncArr.MBEdoc = function (thisItem) {
        initEdoc(thisItem, { jobNo: _uid, GROUP_ID: groupId, CMP: cmp, STN: "*" });
    }
    */
    getSelectOptions();

    MenuBarFuncArr.AddMenu("VoidBtn", "glyphicon glyphicon-bell", "@Resources.Locale.L_MenuBar_Audit", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
 
        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }

        if (userId !== _handler.topData["CreateBy"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_RQManageController_Controllers_209", 500, "warning");
            return false;
        }

        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "RQManage/VoidRQ",
            type: 'POST',
            dataType: "json",
            data: {
                "UId": _handler.topData["UId"]
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                CommonFunc.Notify("", result.message, 2000, "warning");
                if (result.flag) {
                    MenuBarFuncArr.MBCancel();
                }
            }
        });
    });

    MenuBarFuncArr.AddMenu("EXCEL_BTN", "glyphicon glyphicon-bell", "@Resources.Locale.L_RQSetup_Script_178", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (!_handler.topData["SysEdit"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_PassInquiry", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "C") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_IqEnd", 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        if (_handler.topData["Status"] === "V") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasDiscd", 500, "warning");
            return false;
        }
        var id = _handler.topData[_handler.key];
        $("#FileUploadIframe").contents().find("#jobNo").val(id);
        $("#ExcelWindow").modal("show");
    });

    MenuBarFuncArr.AddMenu("BACK_BTN", "glyphicon glyphicon-log-out", "@Resources.Locale.L_qtBase_Scripts_309", function () {

        setNavTabActive({ id: "RQ010", href: rootPath + "QTManage/RQQuery", title: '@Resources.Locale.L_RQQuery_Inquiring' });
    });

    $("#Period").triggerChange = false;
    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    $("#Period").triggerChange = true;
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    MenuBarFuncArr.Enabled(["MBEdoc"]);
    MenuBarFuncArr.Enabled(["EXCEL_BTN"]);
    MenuBarFuncArr.Enabled(["BACK_BTN"]);
    
    SetInlineBtn();
    SetBtnView(false);
    $("#FileUploadIframe").attr("src", rootPath + "RQManage/FileUpload");
    $("#Period").change(function () {
        if ($("#Period").triggerChange===false)
            return;
        //alert($("#Period").val());
        _handler.beforLoadView();

    });

    $("#TranMode").change(function () {
        if ($("#TranMode").triggerChange === false)
            return;
        //setTimeout(function () {
        //    $("#PodCd1")[0].value = "";
        //    $("#PodNm1")[0].value = "";
        //    $("#PodCd")[0].value = "";
        //    $("#PodNm")[0].value = "";
        //}, 500);
        _handler.beforLoadView();
    });
   
});

function SetInlineBtn() {
    //init: true, dv:SpId,
    var lookup = $.extend({}, LookUpConfig.PartyNoLookup);
    //for (var i = 0; i < lookup.columns.length; i++) {
    //    if (lookup.columns[i].name === "PartyType") {
    //        lookup.columns[i].init = true;
    //        lookup.columns[i].dv = "FS";
    //    }
    //}
    var _config = $.extend({ multiselect: true }, lookup);
    registBtnLookup($("#RQST_FWD"), {
        url: rootPath + LookUpConfig.PartyNoUrl, config: _config, param: "", selectRowFn: function (map) {
        }
    }, {
        responseMethod: function (data) {
            var str = "";
            var dataRow = {};
            var ids = $("#MainGrid").jqGrid('getDataIDs');
            var selectedId = $("#MainGrid").jqGrid("getGridParam", "selrow");
            var rowid = Math.max.apply(Math, ids);
            var newrowid = rowid + 1;
            var $grid= $("#MainGrid");
            var rowIds = $grid.getDataIDs();
            $.each(data, function (index, val) {
                //选取资料放入到Grid中
                /*dataRow.push({
                    UId: "",
                    Bidder: 'N',
                    Status: 'N',
                    LspCd: val.PartyNo,
                    LspNm: val.PartyName,
                    BestPrice: '',
                    NotifyDate: getDate(0),
                    NotifyGroup: val.PartyNo,
                    Remark: '',
                    SeqNo: '',
                    RfqNo: '',
                });*/
                for (var i = 0; i < rowIds.length; i++) {
                    var rowDatas = $grid.jqGrid('getRowData', rowIds[i]);
                    if (rowDatas["LspCd"] === val.PartyNo)
                        return;
                }
                dataRow = {
                    UId: "",
                    Bidder: 'N',
                    Status: 'N',
                    LspCd: val.PartyNo,
                    LspNm: val.PartyName,
                    BestPrice: '',
                    //NotifyDate: getDate(0),
                    NotifyGroup: val.PartyNo,
                    Remark: '',
                    SeqNo: '',
                    RfqNo: '',
                };
                $("#MainGrid").jqGrid("addRowData", undefined, dataRow, "last");
            });
            //$("#MainGrid").jqGrid("addRowData", newrowid, dataRow, "last");
            /*$("#MainGrid").jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "SeqNo",
                data: dataRow
            }).trigger("reloadGrid");*/
        }
    });

    $("#RQST_SEND").click(function () {
        //判断细档是否有资料
        if (!_handler.topData["SysEdit"]) {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_PassInquiry", 500, "warning");
            return false;
        }
        //if (_handler.topData["Status"] === "C" || _handler.topData["Status"] === "D") {
        //    CommonFunc.Notify("", "已超过询价到期日期", 500, "warning");
        //    return false;
        //}
        if (_handler.topData["Status"]==="C") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_IqEnd", 500, "warning");
            return false;
        }

        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        var ids = $("#MainGrid").jqGrid('getDataIDs');
        if (ids.length == 0) {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_322", 500, "warning");
            return;
        }

        var mygrid = $("#MainGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var datas = [];
        var dnitems = "";
        $.each(selRowId, function (index, val) {
            //LspCd
            datas.push(mygrid.getRowData(selRowId[index]));
        });
        if (datas.length < 1) {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_323", 1000, "warning");
            return;
        }

        var msg = "@Resources.Locale.L_RQSetup_Scripts_324";
        var lc = [];
        for (var i = 0; i < datas.length; i++) {
            msg += "【" + datas[i].LspCd + ":" + datas[i].LspNm + "】";
            lc.push(datas[i].LspCd);
        }
        msg += "@Resources.Locale.L_RQSetup_Script_179";
        var truthBeTold = window.confirm(msg);
        if (!truthBeTold) {
            return;
        }

        //判断是否是bid的如果是要检核电子文档中是否有稽核的Excel文档
        var _rfqno = $("#RfqNo").val();
        if ($("#Period").val() == "B") {
            var _flag = false;
            $.ajax({
                async: false,
                url: rootPath + "RQManage/CheckNoEdoc",
                type: 'POST',
                data: {
                    "JobNo": $("#UId").val()
                },
                success: function (result) {
                    if (result.message) {
                        _flag = true;
                        return;
                    }
                }
            });

            if (_flag) {
                //var iscontinue = window.confirm("不存在询价的Excel，是否继续发送?");
                //if (!iscontinue) {
                //    return;
                //}
                return CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Script_180", 500, "warning");
                //CommonFunc.Notify("", "不存在询价的Excel，不得发送", 500, "warning");
                //CommonFunc.Notify("", "不存在询价的Excel", 2000, "warning");
            }
        }
        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "RQManage/SendNotifi",
            type: 'POST',
            dataType: "json",
            data: {
                "rfqno": _rfqno,
                "LC": JSON.stringify(lc)
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                CommonFunc.Notify("", result.message, 2000, "warning");
                if (result.flag) {
                    MenuBarFuncArr.MBCancel();
                }
            }
        });
        return false;
    });

    $("#RQST_PCAL").click(function () {//比价计算
        if (_handler.topData["SysEdit"]) {
            var iscontinue = window.confirm("@Resources.Locale.L_RQSetup_Continued");
            if (!iscontinue) {
                return;
            }
        }
        //if (_handler.topData["Status"] === "D") {
        //    CommonFunc.Notify("", "已结案", 500, "warning");
        //    return false;
        //}

        var mygrid = $("#MainGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var datas = [];
        var dnitems = "";
        $.each(selRowId, function (index, val) {
            datas.push(mygrid.getRowData(selRowId[index]));
        });

        var lc = [];
        if (datas.length > 0) {
            if (datas.length < 2) {
                alert("请至少选取两笔记录才能进行比价");
                return;
            }
            var msg = "是否只针对";
            for (var i = 0; i < datas.length; i++) {
                msg += "【" + datas[i].LspCd + ":" + datas[i].LspNm + "】";
                lc.push(datas[i].LspCd);
            }
            msg += "进行比价计算";
            var truthBeTold = window.confirm(msg);
            if (!truthBeTold) {
                return;
            }
        }

        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + "RQManage/ComparePrice",
            type: 'POST',
            dataType: "json",
            data: {
                "U_ID": $("#UId").val(),
                "LC": JSON.stringify(lc)
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                CommonFunc.Notify("", result.message, 2000, "warning");
                if (result.flag) {
                    MenuBarFuncArr.MBCancel();
                    if (lc && lc.length > 0)
                        openDetail(lc);
                }
            }
        });
    });

    function openDetail(lc) {
        var RfqNo = $("#RfqNo").val();
        if (!RfqNo) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var title = "@Resources.Locale.L_RQSetup_Scripts_338";
        var tranmode = $("#TranMode").val();
        var _url = "SFCLQuery";
        if (tranmode == "A") {  //空运
            _url = "AirQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_339";
        } else if (tranmode == "F" || tranmode == "R") {   //海运整柜
            _url = "SFCLQuery";
            if (tranmode == "F")
                title = "@Resources.Locale.L_RQSetup_Scripts_340";
            else
                title = "@Resources.Locale.L_RQSetup_Scripts_341";
        } else if (tranmode == "D") {   //国内快递
            _url = "DEQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_342";
        } else if (tranmode == "E") {   //国际快递
            _url = "IEQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_343";
        } else if (tranmode == "L") {   //海运散货
            _url = "SLCLQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_344";
        } else if (tranmode == "T") {   //国内运输
            _url = "DTQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_345";
        }

        top.topManager.openPage({
            href: rootPath + "RQManage/" + _url + "?RfqNo=" + RfqNo + "&lc=" + encodeURIComponent(JSON.stringify(lc)),
            title: title,
            id: 'DN011',
            search: 'RfqNo=' + RfqNo
        });
    }
    
    $("#RQST_QDET").click(function () {//查看报价明细
        var RfqNo = $("#RfqNo").val();
        if (!RfqNo) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 1000, "warning");
            return;
        }
        var mygrid = $("#MainGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var datas = [];
        $.each(selRowId, function (index, val) {
            datas.push(mygrid.getRowData(selRowId[index]));
        });
        
        var id = $("#MainGrid").jqGrid('getGridParam', "selrow");
        var map = $("#MainGrid").jqGrid('getRowData', id);
        if (!map || !map.LspCd) {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_330", 1000, "warning");
            return;
        }

        if (datas.length > 1) {
            var iscontinue = window.confirm("@Resources.Locale.L_RQSetup_Script_181" + map.LspCd + ":" + map.LspNm + "】@Resources.Locale.L_RQSetup_Script_182");
            if (!iscontinue) {
                return;
            }
        }
        var LspCd = "";
        var title = "@Resources.Locale.L_RQQuery_QuoDetail";
        var tranmode = $("#TranMode").val();
        var _url = "RQSetup";
        var period = $("#Period").val();

        if (tranmode == "A") {  //空运
            _url = "AirSetup";
            title = "@Resources.Locale.L_RQSetup_AirQuoDl";
        } else if (tranmode == "F" || tranmode == "R") {   //海运整柜
            if (tranmode == "F")
                title = "@Resources.Locale.L_RQSetup_ShipFCLQuoDl";
            else
                title = "@Resources.Locale.L_RQSetup_RailQuoDl";

            if (period === "B")
                _url = "FCLFSFSetup";
            else
                _url = "FCLFSetup";
        } else if (tranmode == "D") {   //国内快递
            _url = "DESetup";
            title = "@Resources.Locale.L_RQSetup_Scripts_334";
        } else if (tranmode == "E") {   //国际快递
            _url = "IESetup";
            title = "@Resources.Locale.L_RQSetup_InterEpQuoDl";
        } else if (tranmode == "L") {   //海运散货
            _url = "LCLSetup";
            title = "@Resources.Locale.L_RQSetup_ShipBulkQuoDl";
        } else if (tranmode == "T") {   //国内运输
            _url = "DTSetup";
            title = "@Resources.Locale.L_RQSetup_DomesticQuoDl";
        }

        top.topManager.openPage({
            href: rootPath + "QTManage/" + _url + "?RfqNo=" + RfqNo + "&LspCd=" + map.LspCd + "&RQUid=" + $("#UId").val() + "&Op=N",
            title: title,
            id: 'RQ' + RfqNo,
            search: 'RfqNo=' + RfqNo
        });
    });

    $("#RQST_NNOT").click(function () {//中标通知
        if (_handler.topData["SysEdit"]) {
            var iscontinue = window.confirm("@Resources.Locale.L_RQSetup_Continued");
            if (!iscontinue) {
                return;
            }
            //return false;
        }
        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        sendRQMail("@Resources.Locale.L_RQSetup_LostBid");
    });

    $("#RQST_BNOT").click(function () {//中标通知
        if (_handler.topData["SysEdit"]) {
            var iscontinue = window.confirm("@Resources.Locale.L_RQSetup_Continued");
            if (!iscontinue) {
                return;
            }
            //return false;
        }
        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        sendRQMail("@Resources.Locale.L_RQSetup_Bidder");
    });

    $("#RQST_FBNOT").click(function () {//最終中標通知
        if (_handler.topData["SysEdit"]) {
            var iscontinue = window.confirm("@Resources.Locale.L_RQSetup_Continued");
            if (!iscontinue) {
                return;
            }
        }
        if (_handler.topData["Status"] === "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_QTSetup_HasClosed", 500, "warning");
            return false;
        }
        sendRQMail("@Resources.Locale.L_RQSetup_FinalBid");
    });

    function sendRQMail(tip) {
        var mygrid = $("#MainGrid");
        var selRowId = mygrid.jqGrid('getGridParam', 'selarrrow');
        var datas = [];
        var dnitems = "";
        $.each(selRowId, function (index, val) {
            //LspCd
            datas.push(mygrid.getRowData(selRowId[index]));
        });
        if (datas.length < 1) {
            CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_335" + tip + "@Resources.Locale.L_RQSetup_Scripts_336", 1000, "warning");
            return;
        }

        var msg = "@Resources.Locale.L_RQSetup_Scripts_324";
        var lc = [];
        for (var i = 0; i < datas.length; i++) {
            msg += "【" + datas[i].LspCd + ":" + datas[i].LspNm + "】";
            lc.push(datas[i].LspCd);
        }
        msg += "@Resources.Locale.L_ActManage_Send" + tip + "@Resources.Locale.L_RQSetup_Script_183";
        var truthBeTold = window.confirm(msg);
        if (!truthBeTold) {
            return;
        }
        var action = "RQManage/SendWinNotifiy";
        if ("@Resources.Locale.L_RQSetup_FinalBid" === tip)
            action = "RQManage/SendFinalNotifiy";
        else if ("@Resources.Locale.L_RQSetup_LostBid" === tip)
            action = "RQManage/SendLostNotifiy";

        CommonFunc.ToogleLoading(true);
        $.ajax({
            async: true,
            url: rootPath + action,
            type: 'POST',
            dataType: "json",
            data: {
                "UId": $("#UId").val(),
                "LC": JSON.stringify(lc)
            },
            "complete": function (xmlHttpRequest, successMsg) {
                CommonFunc.ToogleLoading(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.ToogleLoading(false);
            },
            success: function (result) {
                CommonFunc.Notify("", result.message, 5000, "warning");
                if (result.flag) {
                    MenuBarFuncArr.MBCancel();
                }
            }
        });
        return false;


        //CommonFunc.Notify("", "发送成功", 500, "warning");
    }

    $("#RQST_PDET").click(function () {
        var RfqNo = $("#RfqNo").val();
        if (!RfqNo) {
            CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
            return;
        }
        var title = "@Resources.Locale.L_RQSetup_Scripts_338";
        var tranmode = $("#TranMode").val();
        var _url = "SFCLQuery";
        if (tranmode == "A") {  //空运
            _url = "AirQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_339";
        } else if (tranmode == "F" || tranmode == "R") {   //海运整柜
            _url = "SFCLQuery";
            if (tranmode == "F")
                title = "@Resources.Locale.L_RQSetup_Scripts_340";
            else
                title = "@Resources.Locale.L_RQSetup_Scripts_341";
        } else if (tranmode == "D") {   //国内快递
            _url = "DEQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_342";
        } else if (tranmode == "E") {   //国际快递
            _url = "IEQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_343";
        } else if (tranmode == "L") {   //海运散货
            _url = "SLCLQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_344";
        } else if (tranmode == "T") {   //国内运输
            _url = "DTQuery";
            title = "@Resources.Locale.L_RQSetup_Scripts_345";
        }

        top.topManager.openPage({
            href: rootPath + "RQManage/" + _url + "/" + RfqNo,
            title: title,
            id: 'DN011',
            search: 'RfqNo=' + RfqNo
        });
    });
}


var _tran = "", _bu = "P";
function getSelectOptions() {
    $.ajax({
        async: false,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TNT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];

            var pkOptions = data.PK || [];

            trnOptions.push({ cd: "", cdDescp: "" });
            appendSelectOption($("#TranMode"), trnOptions);
            appendSelectOption($("#ServiceMode"), pkOptions);
            appendSelectOption($("#LoadingFrom"), pkOptions);
            appendSelectOption($("#LoadingTo"), pkOptions);
            if (_handler.topData) {
            
                $("#TranMode").val(_handler.topData["TranMode"]);
                $("#ServiceMode").val(_handler.topData["ServiceMode"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
            }
        }
    });
}

function SetBtnView(flag) {
    if (flag) {
        $("#RQST_FWD").removeAttr("disabled");
        $("#RQST_SEND").attr("disabled", true);
        $("#RQST_QDET").attr("disabled", true);
        $("#RQST_PCAL").attr("disabled", true);
        $("#RQST_PDET").attr("disabled", true); 
        $("#RQST_BNOT").attr("disabled", true);
        $("#RQST_NNOT").attr("disabled", true);
        $("#RQST_FBNOT").attr("disabled", true);
    }
    else {
        $("#RQST_FWD").attr("disabled", true);
        $("#RQST_SEND").removeAttr("disabled");
        $("#RQST_QDET").removeAttr("disabled");
        $("#RQST_PCAL").removeAttr("disabled");
        $("#RQST_PDET").removeAttr("disabled");
        $("#RQST_BNOT").removeAttr("disabled");
        $("#RQST_NNOT").removeAttr("disabled");
        $("#RQST_FBNOT").removeAttr("disabled");
    }
    if (flag) {
        //$("#RQST_QDET").parent().hide();
        //$("#RQST_PCAL").parent().hide();
        //$("#RQST_PDET").parent().hide();
    } else {
        //$("#RQST_QDET").parent().show();
        //$("#RQST_PCAL").parent().show();
        //$("#RQST_PDET").parent().show();
    }
}


function CallBack(returnMessage) {
    if (returnMessage === "Y") {
        CommonFunc.Notify("", "@Resources.Locale.L_RQSetup_Scripts_346", 500, "warning");
        $("#ExcelWindow").modal("hide");
    }
    else {
        CommonFunc.Notify("", returnMessage, 500, "warning");
        $("#ExcelWindow").modal("hide");
    }
}



