var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };

$(function(){
    SetCntUnit(true);
    _handler.saveUrl = rootPath + "IQTManage/ExpressAddSave";
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
        data["Cmp"] = encodeURIComponent(cmp); 
        setFieldValue([data]);
    }

    _handler.beforSave = function () { 
    }

    MenuBarFuncArr.EndFunc = function(){

    }

    _handler.saveData = function (dtd) {
        var changeData = getChangeValue();//获取所有改变的值
        var data = { "changedData": JSON.stringify(changeData), autoReturnData: false };
        data["uid"] = encodeURIComponent($("#UId").val());
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
    _handler.delData = function () {
        var changeData = getAllKeyValue();//获取所有主键值
        var data = { "changedData": JSON.stringify(changeData), autoReturnData: false };
        data["uid"] = encodeURIComponent($("#UId").val());
        ajaxHttp(_handler.saveUrl, data,
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
        MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit"]);
    }

    _handler.beforLoadView = function () { 
        var requires = ["EffectDate", "ExpiratDate"]; 
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "IQTManage/ExpressAddDetail", { uId: map.UId, loading: true },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });

    }


    MenuBarFuncArr.MBCopy = function (thisItem) {
        //初始化新增数据
        var data = {};
        data[_handler.key] = uuid();
        $("#CreateBy").val("");
        $("#CreateDate").val("");
        $("#ModifyBy").val("");
        $("#ModifyDate").val(""); 
    }

    _initUI(["MBApply", "MBApprove", "MBErrMsg", "MBSearch", "MBInvalid", "MBPreview","MBEdoc"]);//初始化UI工具栏
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    registBtnLookup($("#LspCdLookup"), {
        item: '#LspCd', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspCd").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#PolCdLookup"), {
        item: "#PolCd", url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            $("#PolName").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PolName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd); 
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD); 
    }));

    registBtnLookup($("#CntryCdLookup"), {
        item: '#CntryCd', url: rootPath + LookUpConfig.CountryUrl, config: LookUpConfig.CountryLookup, param: "", selectRowFn: function (map) {
            $("#CntryCd").val(map.CntryCd);
            $("#CntryNm").val(map.CntryNm);
        }
    }, undefined, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#CntryCd").val(rd.CNTRY_CD);
        $("#CntryNm").val(rd.CNTRY_NM);
    }, function ($grid, elem) {
        $("#CntryCd").val("");
        $("#CntryNm").val("");
    }));
    
});

