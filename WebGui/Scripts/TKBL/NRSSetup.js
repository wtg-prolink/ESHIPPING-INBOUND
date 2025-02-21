//var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };

$(function () {
    _handler.saveUrl = rootPath + "TKBL/SaveNotifyData";
    _handler.inquiryUrl = rootPath + LookUpConfig.NotifyUrl;
    _handler.config = LookUpConfig.NotifyLookup;
    getSelectOptions();

    _handler.addData = function () {
        //初始化新增数据
        var data = { NotifyFormat: _mt, NotifyTimes: 1, NotifyPeriod: 30, StartHour: 0,Cmp:cmp };
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }

    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + LookUpConfig.NotifyItemUrl, { uId: map.UId },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);
            });
    }

    registBtnLookup($("#NotifyCdLookup"), {
        item: '#NotifyCd', url: rootPath + LookUpConfig.StatusUrl, config: LookUpConfig.StatusLookup, param: "", selectRowFn: function (map) {
            $("#NotifyCd").val(map.StsCd);
            $("#NotifyDescp").val(map.Edescp);
        }
    }, undefined, LookUpConfig.GetStatusAuto(groupId, undefined, function ($grid, rd, elem,rowid) {
        //elem.val(rd.NAME);
        $("#NotifyDescp").val(rd.EDESCP);
    }));

    registBtnLookup($("#RequestCdLookup"), {
        item: '#RequestCd', url: rootPath + LookUpConfig.StatusUrl, config: LookUpConfig.StatusLookup, param: "", selectRowFn: function (map) {
            $("#RequestCd").val(map.StsCd);
            $("#RequestDescp").val(map.Edescp);
        }
    }, undefined, LookUpConfig.GetStatusAuto(groupId, undefined, function ($grid, rd, elem, rowid) {
        //elem.val(rd.NAME);
        $("#RequestDescp").val(rd.EDESCP);
    }));

    registBtnLookup($("#PartyTypeLookup"), {
        item: '#PartyType', url: rootPath + LookUpConfig.PartyTypeUrl, config: LookUpConfig.PartyTypeLookup, param: "", selectRowFn: function (map) {
            $("#PartyType").val(map.Cd);
            $("#PartyDescp").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "PT", undefined, function ($grid, rd, elem, rowid) {
        $("#PartyDescp").val(rd.CD_DESCP);
    }));

    registBtnLookup($("#CmpLookup"), {
        item: '#Cmp', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Cmp").val(map.PartyNo);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem, rowid) {

    }));

    registBtnLookup($("#PartyNoLookup"), {
        item: '#PartyNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#PartyNo").val(map.PartyNo);
            $("#PartyName").val(map.PartyName);
        }
    }, {
        baseConditionFunc: function () {
            //return "PartyType_spot=like&PartyType=" + $("#PartyType").val();
            return "PARTY_TYPE LIKE '%" + $("#PartyType").val() + "%'";
        }
    }, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PartyName").val(rd.PARTY_NAME);
    }));


    registBtnLookup($("#PolCdLookup"), {
        item: '#PolCd',
        url: rootPath + LookUpConfig.CityPortUrl,
        config: LookUpConfig.CityPortLookup,
        param: "",
        selectRowFn: function (map) {
            $("#PolCd").val(map.CntryCd + map.PortCd);
            $("#PolNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#PolCd").val(rd.CNTRY_CD + rd.PORT_CD);
		    $("#PolNm").val(rd.PORT_NM);
		}
	));

    registBtnLookup($("#PodCdLookup"), {
        item: '#PodCd',
        url: rootPath + LookUpConfig.CityPortUrl,
        config: LookUpConfig.CityPortLookup,
        param: "",
        selectRowFn: function (map) {
            $("#PodCd").val(map.CntryCd + map.PortCd)
            $("#PodNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined,
		function ($grid, rd, elem) {
		    $("#PodCd").val(rd.CNTRY_CD + rd.PORT_CD);
		    $("#PodNm").val(rd.PORT_NM);
		}
	));

    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBEdoc", "MBApprove", "MBErrMsg"]);//初始化UI工具栏

    $("#SeqNo").removeAttr('required');
    $("#RequestCd").removeAttr('required');
    $("#StartHour").removeAttr('isnumber');
    $("#StartHour").removeAttr('maxlength');
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
});

var _mt = "";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("NRSSetup") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var mtOptions = data.MT || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["cd"];
            appendSelectOption($("#NotifyFormat"), mtOptions);
            if (_handler.topData) {
                $("#NotifyFormat").val(_handler.topData["NotifyFormat"]);
            }
        }
    });
}
