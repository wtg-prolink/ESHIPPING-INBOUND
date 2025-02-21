var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function () {
    _handler.saveUrl = rootPath + "Bsdate/TermVSChargeUpdate";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];
    var _NeedChgOption = { "FC": "Freight Charge", "BC": "Broker Charge", "TC": "Truck Charge", "LC": "Local Charge" };
    var NeedChgStr = "";
    $.each(_NeedChgOption, function (index, val) {
        NeedChgStr += "<input type='checkbox' name='NeedChgArea[]' value='" + index + "' disabled>" + val;
    });
    var _NeedChgOption2 = { "OBC": "Broker Charge", "OTC": "Truck Charge", "OLC": "Local Charge" };
    var NeedChgStr2 = "";
    $.each(_NeedChgOption2, function (index, val) {
        NeedChgStr2 += "<input type='checkbox' name='NeedChgArea[]' value='" + index + "' disabled>" + val;
    });

    $("#NeedChgArea").append("<div>Inbound:" + NeedChgStr + "</div>" + "<div>Outbound:" + NeedChgStr2 + "</div>");

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else {
            $("input[name='NeedChgArea[]']").each(function (index, el) {
                $(this).prop("checked", false);
            });
            _handler.topData = [{}];
        }
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        if (data["main"].length > 0) {
            var needChg = data["main"][0].NeedChg;
            if (needChg === null) {
                needChg = "";
            }
            var needChgArr = new Array();
            needChgArr = needChg.split(",");

            $("input[name='NeedChgArea[]']").each(function (index, el) {
                var cVal = $(this).val();
                var c = $(this);
                $.each(needChgArr, function (index, val) {
                    if (cVal === val) {
                        c.prop("checked", true);
                    }
                });
            });
        }
        MenuBarFuncArr.Enabled(["MBCopy"]);
    };

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
        ajaxHttpSaveBar(dtd, _handler.saveUrl, data,
            function (result) {
                $("input[name='NeedChgArea[]']").prop("disabled", true);
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                else if (_handler.setFormData)
                    _handler.setFormData(result);
                return true;
            });
    }

    _handler.beforSave = function (data) {
        var needChg = "";
        $("input[name='NeedChgArea[]']").each(function (index, el) {
            var val = $(this).val();
            var checked = $(this).prop("checked");
            if (checked === true) {
                if (needChg != "")
                    needChg += ",";
                needChg += val;
            }
        });
        $("#NeedChg").val(needChg);
    };

    _handler.beforEdit = function (data) {
        $("input[name='NeedChgArea[]']").prop("disabled", false);
    };

    _handler.cancelData = function () {
        $("input[name='NeedChgArea[]']").prop("disabled", true);
        if (_handler.loadMainData)
            _handler.loadMainData(_handler.topData);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "Bsdate/GetTermVSChargeDataItem", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    _handler.beforAdd = function (data) {
        $("input[name='NeedChgArea[]']").prop("disabled", false);
        $("input[name='NeedChgArea[]']").each(function (index, el) {
            $(this).prop("checked", false);
        });
    };

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        $("#CreateDate").val("");
        $("#CreateBy").val("");
        $("#ModifyDate").val("");
        $("#ModifyBy").val("");
        $("input[name='NeedChgArea[]']").prop("disabled", false);
    }

    registBtnLookup($("#PodCdLookup"), {
        item: "#PodCd", url: rootPath + LookUpConfig.CityAndTruckPortUrl, config: LookUpConfig.CityPortLookup2, param: "", selectRowFn: function (map) {
            $("#PodCd").val(map.Pod);
            $("#PodName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityAndTruckPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PodCd").val(rd.POD);
        $("#PodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#IncotermCdLookup"), {
        item: '#IncotermCd', url: rootPath + LookUpConfig.TermUrl, config: LookUpConfig.TermLookup, param: "", selectRowFn: function (map) {
            $("#IncotermCd").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TD", undefined, function ($grid, rd, elem) {
        $("#IncotermCd").val(rd.CD);
    }));

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
    }));

    _initUI();
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
});