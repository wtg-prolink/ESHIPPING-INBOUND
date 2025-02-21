var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var IoFlag = getCookie("plv3.passport.ioflag");
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user");
var _trigger = true;
$(function() {
    SetCntUnit();
    //var pmsIds = ["RQST_FWD", "RQST_SEND", "RQST_QDET", "RQST_PCAL", "RQST_PDET", "RQST_BNOT", "RQST_NNOT", "RQST_FBNOT"];
    //for (var i = 0; i < pmsIds.length; i++) {
    //    pmsBtnCheck(pmsIds[i]);
    //}
    //if (pmsList.indexOf("NOTEDIT_INFO") == -1) {
    //    noteditInfoPms = true;
    //}

    _handler.afterEdit = function () {
        var UId = $("#UId").val();
        if (!isEmpty(UId)) {
            var readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "IpartNo", "Cur", "UnitPrice", "ChgUnit", "Bamt", "Tax", "CheckDescp", "BlNo", "IpartNo", "Qty", "BiRemark", "ChgType"];
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).attr('readonly', false);
                $("#" + readonlys[i]).parent().find("button").attr('disabled', false);
            }
            if (_op === "1") {
                readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "IpartNo", "Cur", "UnitPrice", "ChgUnit", "Bamt", "Tax", "Qty", "ChgType"];
            }
            else
                readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "CheckDescp", "IpartNo", "BiRemark", "ChgType"];
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).attr('readonly', true);
                $("#" + readonlys[i]).parent().find("button").attr('disabled', true);
            }
        }
    }

    var BookingLookup = {
        caption: _getLang("L_ActUpdate_Scripts_78","账单费用查询"),
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'UId', sorttype: 'string', hidden: true, viewable: false }
        ]
    };

    _handler.delData = function () {
        var changeData = getAllKeyValue();//获取所有主键值
        ajaxHttp(_handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false },
            function (result) {
                if (result.message) {
                    CommonFunc.Notify("", result.message, 1000, "warning");
                    return false;
                }
                else if (_handler.setFormData) {
                    if (result["main"])
                        _handler.setFormData(result);
                    else
                        _handler.setFormData([{}]);
                }
                return true;
            });
    }

    _handler.saveUrl = rootPath + "ActManage/SaveActItemData";
    _handler.inquiryUrl = rootPath + "ActManage/GetActDataItem";
    _handler.config = BookingLookup;

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
        if (!isEmpty($("#UFid").val())) {
            alert(_getLang("L_ActSetup_Scripts_49","已关联账单的费用不可删除"));
            return false;
        }

        if ($("#ApproveStatus").val()==="Y") {
            alert(_getLang("L_ActUpdate_Scripts_79","审核通过不可删除"));
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        //var dep = getCookie("plv3.passport.dep"), ext = getCookie("plv3.passport.ext");
        var data = { LspNo: cmp };
        $("#UId").val("");
        $("#UFid").val("");
        _handler.beforLoadView();

        $("#UId").val(uuid("-"));
        $("#Qty").val(1);
        $("#LspNo").val(cmp);
        $("#LspNo").blur();
    }

    for (var name in _units) {
        if (name === "%")
            continue;
        $("#QchgUnit").append('<option value="' + name + '">' + _units[name] + '</option>');
        $("#ChgUnit").append('<option value="' + name + '">' + _units[name] + '</option>');
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        data["Bamt"] = $("#Bamt").val();
        data["UnitPrice"] = $("#UnitPrice").val();
        data["Qty"] = $("#Qty").val();
        data["Cur"] = $("#Cur").val();
        data["debit_date"] = $("#DebitDate").val();
        data["ChgCd"] = $("#ChgCd").val();
        data["LspNo"] = $("#LspNo").val();
        data["IpartNo"] = $("#IpartNo").val();
        data["ShipmentId"] = $("#ShipmentId").val();

        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
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

        if (!isEmpty($("#UFid").val())) {
            alert(_getLang("L_DNManage_RefNlCtEt","已关联账单的费用不可修改"));
            return false;
        }

        if (_op !== "1") {
            if ($("#ApproveStatus").val() === "Y") {
                alert(_getLang("L_ActUpdate_Scripts_80","审核通过不可修改"));
                return false;
            }
        }
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        _trigger = false;
        setFieldValue(data["main"] || [{}]);
        _trigger = true;
        //_handler.beforLoadView();

        MenuBarFuncArr.Enabled(["MBCopy"]);
        MenuBarFuncArr.Enabled(["UnApprove"]);
        ShowSMStatus(_handler.topData["SmStatus"]);
        setdisabled(true);
        setToolBtnDisabled(true);

        var UId = $("#UId").val();
        if (!isEmpty(UId)) {
            var readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "IpartNo", "Cur", "UnitPrice", "ChgUnit", "Bamt", "Tax", "CheckDescp", "BlNo", "IpartNo", "Qty", "BiRemark", "ChgType"];
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).attr('readonly', false);
                $("#" + readonlys[i]).parent().find("button").attr('disabled', false);
            }
            if (_op === "1") {
                readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "IpartNo", "Cur", "UnitPrice", "ChgUnit", "Bamt", "Tax", "Qty", "ChgType"];
            }
            else
                readonlys = ["ShipmentId", "LspNo", "LspNm", "ChgCd", "ChgDescp", "DebitNo", "BlNo", "CheckDescp", "IpartNo", "BiRemark", "ChgType"];
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).attr('readonly', true);
                $("#" + readonlys[i]).parent().find("button").attr('disabled', true);
            }
        }
    }


    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "ActManage/GetActDataItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    registBtnLookup($("#LspNoLookup"), {
        item: "#LspNo", url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
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

    registBtnLookup($("#ChgCdLookup"), {
        item: "#ChgCd", url: rootPath + LookUpConfig.ChgUrl, config: LookUpConfig.ChgLookup, param: "", selectRowFn: function (map) {
            $("#ChgCd").val(map.ChgCd);
            $("#ChgDescp").val(map.ChgDescp);
            $("#ChgType").val(map.ChgType);
        }
    }, {
        baseConditionFunc: function () {
            var condi = "IO_TYPE='I' AND CMP='" + cmp + "'";
            var tranType = $("#TranType").val();
            if (!isEmpty(tranType))
                condi+= " AND TRAN_MODE='" + tranType + "'";
            return condi;
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ChgCd").val(rd.CHG_CD);
        $("#ChgDescp").val(rd.CHG_DESCP);
    }));

    registBtnLookup($("#CurLookup"), {
        item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
            $("#Cur").val(map.Cur);
        }
    }, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
        $("#Cur").val(rd.CUR);
    }));

    //registBtnLookup($("#CurLookup"), {
    //    item: '#Cur', url: rootPath + LookUpConfig.CurUrl, config: LookUpConfig.CurLookup, param: "", selectRowFn: function (map) {
    //        $("#Cur").val(map.Cur);
    //    }
    //}, undefined, LookUpConfig.GetCurAuto(groupId, undefined, undefined, function ($grid, rd, elem) {
    //    $("#Cur").val(rd.CUR);
    //}));


    registBtnLookup($("#ChgUnitLookup"), {
        item: '#ChgUnit', url: rootPath + LookUpConfig.QtyuUrl, config: LookUpConfig.QtyuLookup, param: "", selectRowFn: function (map) {
            $("#ChgUnit").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "UB", undefined, function ($grid, rd, elem) {
        $("#ChgUnit").val(rd.CD);
    }));

    if (_op === "1") {
        MenuBarFuncArr.AddMenu("UnApprove", "glyphicon glyphicon-bell", "@Resources.Locale.L_ActDeatilManage_Views_46", function () {
            if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
                CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
                return false;
            }

            if (!isEmpty($("#UFid").val())) {
                alert(_getLang("L_ActUpdate_Scripts_81","已关联账单的费用不可操作"));
                return false;
            }

            var msg = prompt("@Resources.Locale.L_ChgApproveManage_Views_179", "");
            if (msg === null || msg === "" || msg === undefined) {
                CommonFunc.Notify("", "@Resources.Locale.L_ChgApproveManage_Views_180", 500, "warning");
                return; //break out of the function early
            }

            CommonFunc.ToogleLoading(true);
            $.ajax({
                async: true,
                url: rootPath + "ActManage/UnApproveSMBID",
                type: 'POST',
                dataType: "json",
                data: {
                    "id": _handler.topData["UId"],
                    "msg": msg
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
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        $("#q_body").find("[fieldname]").each(function () {
            $(this).val("");
        });
        $("#UId").val("");
        $("#UFid").val("");
        _handler.beforLoadView();

        $("#UId").val(uuid("-"));
        $("#CheckDescp").val("");
        $("#CheckDescp").val("");
        $("#BiRemark").val("");
        $("#DebitNo").val("");
        $("#Status").val("");
        $("#ApproveStatus").val("");
    }

    _handler.beforLoadView = function (setCur) {
        var UId = $("#UId").val();
        //$("#RfqNo").attr('isKey', true);
        //$("#RfqNo").attr('disabled', true);
        $("#main_body").find("[fieldname]").each(function () {
            var jq = $(this);
            jq.attr('readonly', false);
            jq.parent().find("button").prop("disabled", false);
        });

        if (!isEmpty(UId)) {
            $("#main_body").find("[fieldname]").each(function () {
                var jq = $(this);
                jq.attr('readonly', true);
                jq.parent().find("button").prop("disabled", true);
            });
        }
        $("#DebitNm").attr('readonly', false);
        $("#DebitTo").attr('readonly', false);

        if (_op === "1") {
            var readonlys0 = ["CheckDescp"];
            for (var i = 0; i < readonlys0.length; i++) {
                $("#" + readonlys0[i]).removeAttr('readonly');
                $("#" + readonlys0[i]).parent().find("button").removeAttr("disabled");
            }
        }

        var readonlys = ["Cur", "DebitNo"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', false);
            $("#" + readonlys[i]).parent().find("button").prop("disabled", false);
        }
        if (!isEmpty($("#Qcur").val())) {
            for (var i = 0; i < readonlys.length; i++) {
                $("#" + readonlys[i]).attr('readonly', true);
                $("#" + readonlys[i]).parent().find("button").prop("disabled", true);
            }
            if (setCur) {
                $("#Cur").val($("#Qcur").val());
                getRate();
            }
        }
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
                var tdltOptions = data.TDLT || [];
                appendSelectOption($("#DebitTo"), tdltOptions);

                if (_handler.topData) {
                    $("#DebitTo").val(_handler.topData["DebitTo"]);
                }
            }
        });
    }
    $("#DebitTo").change(function () {
        if (!_trigger)
            return;
        var txt = $("#DebitTo").find("option:selected").text();
        var val = $("#DebitTo").val();
        if (txt && txt.replace)
            $("#DebitNm").val(txt.replace(val + ":", ""));
        else
            $("#DebitNm").val(txt);
    });



    getSelectOptions();

    function getValueParseFloat(val) {
        if (val == null || val == undefined || val === "")
            val = 0;
        val = parseFloat(val + '');
        return val;
    }

    function calBamt() {
        _trigger = false;
        var rate = getValueParseFloat($("#ExRate").val());
        var Qty = getValueParseFloat($("#Qty").val());
        var UnitPrice = getValueParseFloat($("#UnitPrice").val());
        var bamt = (Qty * UnitPrice).toFixed(2);
        if (bamt == 0) {
            bamt = getValueParseFloat($("#Bamt").val());
        }
        var lamt = (bamt * rate).toFixed(2);
        $("#Bamt").val(bamt);
        $("#Lamt").val(lamt);
        _trigger = true;
    }

    $("#UnitPrice").change(function () {
        if (!_trigger)
            return;
        calBamt();
    });

    $("#Cur").change(function () {
        if (!_trigger)
            return;
        getRate();

    });

    $("#Bamt").change(function () {
        if (!_trigger)
            return;
        _trigger = false;
        var rate = getValueParseFloat($("#ExRate").val());
        var bamt = getValueParseFloat($("#Bamt").val());
        var lamt = (bamt * rate).toFixed(2);
        $("#Lamt").val(lamt);
        _trigger = true;

    });

    $("#Qty").change(function () {
        if (!_trigger)
            return;
        calBamt();
    });

    //if (_add === "N")
    //    _initUI(["MBAdd", "MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBEdoc", "MBSearch", "MBCopy"]);//初始化UI工具栏
    //else
    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg", "MBEdoc", "MBSearch"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    var _curMap = {};
    function getRate() {
        var params = {};
        params["Cur"] = $("#Cur").val() || '';
        params["Cmp"] = $("#Cmp").val() || '';
        params["debit_date"] = $("#DebitDate").val() || getDate(0);
        var key = params["Cur"] + params["Cmp"] + params["debit_date"];

        if (_curMap[key]) {
            $("#ExRate").val(_curMap[key]);
            calBamt();
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "ActManage/Get2CNYRate",
            type: 'POST',
            data: params,
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {

            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.msg)
                    alert(result.msg);
                else {
                    $("#ExRate").val(result.rate);
                    _curMap[key] = result.rate;
                    calBamt();
                }
            }
        });
    }
    function ShowSMStatus(_status) {
        var statsmap = {
            'A': _getLang('c', '未處理'), 'B': _getLang('L_BSTSetup_Book', '订舱'), 'C': _getLang('L_UserQuery_ComBA', '订舱确认'), 'D': _getLang('L_UserQuery_Call', '叫柜'), 'I': _getLang('L_UserQuery_In', '入厂'),
            'P': _getLang('L_UserQuery_SealCnt', '封柜'), 'O': _getLang('L_UserQuery_Out', '离厂'), 'G': _getLang('L_UserQuery_Declara', '报关'), 'H': _getLang('L_UserQuery_Release', '放行'),
            'V': _getLang('L_BSCSDateQuery_Cancel', '取消'), 'Z': _getLang('L_UserQuery_Return', '退运'), 'U': _getLang('L_DNManage_NtConfBL', '未确认提单'), 'Y': _getLang('L_DNManage_ConfBL', '已确认提单')
        };
        var descp = statsmap[_status] || _status;
        $("#Status_descp").text(descp);
    }
});



