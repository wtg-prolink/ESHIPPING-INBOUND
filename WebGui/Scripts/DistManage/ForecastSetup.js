var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
$(function () {
    var searchColumns = {
        caption: "Forecast Search",
        sortname: "UId",
        refresh: false,
        columns: [{ name: "UId", title: "UId", width: 120, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Year', title: '@Resources.Locale.L_BookingQuery_Views_284', width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Month', title: '@Resources.Locale.L_common_Scripts_19', width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Week', title: '@Resources.Locale.L_ContainUsageSetup_Scripts_118', width: 40, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Cmp', title: '@Resources.Locale.L_MailGroupSetup_Cmp', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Pod', title: 'Pod', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Pol', title: 'Pol', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Dest', title: 'Dest', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Term', title: 'Term', width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'FreightTerm', title: 'FreightTerm', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    }
    var ForecastItemurl = "DistManage/GetForecastItem"; 
    _handler.saveUrl = rootPath + "DistManage/ForecastUpdateData";
    _handler.inquiryUrl = rootPath + "DistManage/ForecastInquiryData";
    _handler.config = searchColumns;

    _handler.addData = function () {
        //初始化新增数据
        var data = { "CreateBy": userId, "Cmp": cmp };
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }



    _handler.beforSave = function () {
        //var $grid = $SubGrid;
        //var nullCols = [], sameCols = [];
        //nullCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        //nullCols.push({ name: "PartyName", index: 3, text: 'PartyName' });
        //sameCols.push({ name: "PartyType", index: 2, text: 'PartyType' });
        //return _handler.checkData($grid, nullCols, sameCols);
    }

    registBtnLookup($("#PolLookup"), {
        item: '#Pol', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            var value = map.CntryCd + map.PortCd;
            $("#Pol").val(value);
        }
    }, undefined, LookUpConfig.GetTpvPortAuto(groupId, 'L', undefined, function ($grid, rd, elem) {
        var value = map.CNTRY_CD + map.PORT_CD;
        $("#Pol").val(value);
    }));

    registBtnLookup($("#TermLookup"), {
        item: '#Term', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#Term").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#Term").val(rd.CD);
    }));

    registBtnLookup($("#PolLookup"), {
        item: '#Pol', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pol").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pol").val(rd.CNTRY_CD + rd.PORT_CD);
    }));

    registBtnLookup($("#PodLookup"), {
        item: '#Pod', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pod").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pod").val(rd.CNTRY_CD + rd.PORT_CD);
    }));

    registBtnLookup($("#DestLookup"), {
        item: '#Dest', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Dest").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Dest").val(rd.CNTRY_CD + rd.PORT_CD);
    }));

    registBtnLookup($("#CustNoLookup"), {
        item: '#CustNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#CustNo").val(map.PartyNo);
            $("#CustNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CustNo").val(rd.PARTY_NO);
        $("#CustNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
    }));

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + ForecastItemurl, { UId: map.UId },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var uid = $("#UId").val();
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, autoReturnData: false },
            function (result) {
                _handler.topData = { _uid: result.UId };
                _uid = result.UId;
                if (_handler.setFormData) {
                    _handler.setFormData(result);
                }
                MenuBarFuncArr.MBCancel();
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
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _initUI(["MBApply", "MBInvalid", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBEdoc", "MBApprove", "MBErrMsg"]);

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    MenuBarFuncArr.MBCancel = function () {
        var _mainCmpData = { UId: _uid }; //_uid
        _handler.loadMainData(_mainCmpData);
        editable = false;
        _subEdit = 0;
    }

    MenuBarFuncArr.MBCopy = function () {
        editable = false;
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        $("#Cmp").removeAttr('required');

        //var data = { "CreateBy": userId, "Cmp": cmp };
        //data[_handler.key] = uuid();
        //setFieldValue([data]);
    }

    $("#F20").blur(function () {
        calculaFeu();
    });
    $("#F40").blur(function () {
        calculaFeu();
    });
    $("#F40hq").blur(function () {
        calculaFeu();
    });
    function calculaFeu() {

        var f20 = parseInt($("#F20").val()) / 2;
        if (!f20) {
            f20 = 0;
        }
        f20 = Math.ceil(f20)
        var f40 = parseInt($("#F40").val());
        var f40hq = parseInt($("#F40hq").val());
        if (!f40) {
            f40 = 0;
        }
        if (!f40hq) {
            f40hq = 0;
        }
        var feuf = f20 + f40 + f40hq;
        $("#Ffeu").val(feuf);
    }

    $("#A20").blur(function () {
        calculaAFeu();
    });
    $("#A40").blur(function () {
        calculaAFeu();
    });
    $("#A40hq").blur(function () {
        calculaAFeu();
    });
    function calculaAFeu() {

        var a20 = parseInt($("#A20").val()) / 2;
        if (!a20) {
            a20 = 0;
        }
        a20 = Math.ceil(a20)
        var a40 = parseInt($("#A40").val());
        var a40hq = parseInt($("#A40hq").val());
        if (!a40) {
            a40 = 0;
        }
        if (!a40hq) {
            a40hq = 0;
        }
        var feu = a20 + a40 + a40hq;
        $("#Afeu").val(feu);
    }

    var _ywm = {};
    function setMonth() {
        //alert($("#Year").atrr("disabled"));
        if ($("#Year[disabled]").length > 0)
            return;
        //$("input").css("background-color", "#D6D6FF");
        var key = $("#Year").val() + "_" + $("#Week").val();
        if (_ywm[key]) {
            $("#Month").val(_ywm[key]);
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "DistManage/GetMonth",
            type: 'POST',
            data: {
                y: encodeURIComponent($("#Year").val()),
                w: encodeURIComponent($("#Week").val())
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success") return null;
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message) {
                    alert(result.message);
                    return;
                }
                _ywm[key] = result.m;
                $("#Month").val(result.m);
            }
        });
    }
    $("#Year").blur(setMonth);
    $("#Week").blur(setMonth);
});

