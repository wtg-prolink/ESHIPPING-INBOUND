var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
$(function () {
    var searchColumns = {
        caption: "Contain Search",
        sortname: "UId",
        refresh: false,//Year,Month,Week,Cmp,Region,Pod,No1,No2,No3,No4,No5
        columns: [{ name: "UId", title: "UId", width: 120, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: "OdType", title: "@Resources.Locale.L_CntySetup_CntryNm", width: 200, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Year', title: '@Resources.Locale.L_BookingQuery_Views_284', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Month', title: '@Resources.Locale.L_common_Scripts_19', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Week', title: '@Resources.Locale.L_ContainUsageSetup_Scripts_118', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Cmp', title: '@Resources.Locale.L_MailGroupSetup_Cmp', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
                  { name: 'Pod', title: 'Pod', width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    }

    var ForecastItemurl = "DistManage/GetContainUtiliItem";
    _handler.saveUrl = rootPath + "DistManage/ContainUtiliUpdateData";
    _handler.inquiryUrl = rootPath + "DistManage/ContainUtiliInquiryData";
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

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.CmpLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.Cmp);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cmp").val(rd.CMP);
    }));

    registBtnLookup($("#PodLookup"), {
        item: '#Pod', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pod").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pod").val(rd.CNTRY_CD + rd.PORT_CD);
    }));

    registBtnLookup($("#PolLookup"), {
        item: '#Pol', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#Pol").val(map.CntryCd + map.PortCd);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Pol").val(rd.CNTRY_CD + rd.PORT_CD);
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
        _handler.topData = { UId: map.UId };
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
                _handler.topData = { UId: result.UId };
                _uid = result.UId;
                MenuBarFuncArr.MBCancel();
                return true;
            });
    }

    MenuBarFuncArr.MBCopy = function () {
        editable = false;
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        $("#Cmp").removeAttr('required');
    }

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
    }

    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
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
});

