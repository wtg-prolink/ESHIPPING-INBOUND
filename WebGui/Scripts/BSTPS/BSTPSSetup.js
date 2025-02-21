var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
$(function () {

    _handler.saveUrl = rootPath + "BSTPS/UpdateData";
    _handler.inquiryUrl = rootPath + "";
    _handler.config = [];

    _handler.beforDel = function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    }

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }

    _handler.beforSave = function () {

        return true;
    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["UId"] = encodeURIComponent($("#UId").val());
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
    }

    _handler.beforAdd = function () {//新增前设定
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        _handler.beforLoadView();
        setdisabled(true);
        setToolBtnDisabled(true);
        MenuBarFuncArr.Enabled(["MBCopy"]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "BSTPS/GetDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        var dataRow, addData = [];
    }

    registBtnLookup($("#PortCdLookup"), {
        item: '#PortCd',
        url: rootPath + LookUpConfig.CityPortUrl,
        config: LookUpConfig.CityPortLookup,
        param: "",
        selectRowFn: function (map) {
            $("#PortCd").val(map.CntryCd + map.PortCd)
            $("#PortNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined,
        function ($grid, rd, elem) {
            $("#PortCd").val(rd.CNTRY_CD + rd.PORT_CD);
            $("#PortNm").val(rd.PORT_NM);
        }
    ));


    registBtnLookup($("#DlvAreaLookup"), {
        item: '#DlvArea',
        url: rootPath + 'TPVCommon/GetBstportDataForLookup',
        config: LookUpConfig.TruckPortCdLookup,
        param: "",
        selectRowFn: function (map) {
            $("#DlvArea").val(map.PortCd)
            $("#DlvAreaNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined,
        function ($grid, rd, elem) {
            $("#DlvArea").val(rd.PORT_CD);
            $("#DlvAreaNm").val(rd.PORT_NM);
        }
    ));

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
            $("#CmpNm").val(map.Name);
        }
    }, undefined, LookUpConfig.GetCmpAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
        $("#CmpNm").val(rd.NAME);
    }));

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    getSelectOptions();
});


function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("BSTPS") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var trnOptions = data.TNT || [];

            appendSelectOption($("#TranType"), trnOptions);
        }
    });
}