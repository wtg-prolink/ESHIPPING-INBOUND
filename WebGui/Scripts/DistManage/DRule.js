//var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };

$(function () {
    var DruleLookup = {
        caption: "@Resources.Locale.L_BaseLookup_SerCarNotifySet",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni']},
	            { name: 'Cmp', title: 'Location', index: 'Cmp', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'Term', title: '@Resources.Locale.L_DRule_Term', index: 'Term', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'PickupPort', title: '@Resources.Locale.L_DRule_PickupPort', index: 'PickupPort', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'ViaPort', title: '@Resources.Locale.L_DRule_ViaPort', index: 'ViaPort', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'Region', title: '@Resources.Locale.L_BaseLookup_Region', index: 'Region', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'State', title: '@Resources.Locale.L_DRule_State', index: 'State', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'DeliveryPort', title: '@Resources.Locale.L_DNApproveManage_Pod', index: 'DeliveryPort', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'Customer', title: 'Customer', index: 'Customer', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'Model', title: '@Resources.Locale.L_DRule_Model', index: 'Model', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'LspNo', title: '@Resources.Locale.L_DRule_LspNo', index: 'LspNo', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'LspNm', title: '@Resources.Locale.L_AirQuery_LspNm', index: 'LspNm', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'UseRate', title: '@Resources.Locale.L_DRule_UseRate', index: 'UseRate', width: 90, align: 'right', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
	            { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 90, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }]
    };
    var DruleItemUrl = "DistManage/GetDRuleItem";

    _handler.saveUrl = rootPath + "DistManage/SaveDRule";
    _handler.inquiryUrl = rootPath + "DistManage/GetDRuleData";;//LookUpConfig.NotifyUrl
    _handler.config = DruleLookup;
    getSelectOptions();

    _handler.addData = function () {
        //初始化新增数据

        var data = { "CreateBy": userId, "Cmp": cmp };
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }
    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + DruleItemUrl, { uId: map.UId },
            function (data) {
                if (_handler.setFormData)
                    _handler.setFormData(data);

                MenuBarFuncArr.Enabled(["MBCopy"]);
            });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#UId").removeAttr('required');
        $("#UId").val("");
    }


    registBtnLookup($("#PickupPortLookup"), {
        item: '#PickupPort', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PickupPort").val(map.PortCd);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PickupPort").val(rd.PORT_CD);
    }));

    registBtnLookup($("#DeliveryPortLookup"), {
        item: '#DeliveryPort', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#DeliveryPort").val(map.PortCd);
            $("#DeliveryPortnm").val(map.PortNm);
            $("#State").val(map.State);
            $("#Region").val(map.Region);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#DeliveryPort").val(rd.PORT_CD);
        $("#DeliveryPortnm").val(rd.PORT_NM);
        $("#State").val(rd.STATE);
        $("#Region").val(rd.REGION);
    }));

    registBtnLookup($("#ViaPortLookup"), {
        item: '#ViaPort', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#ViaPort").val(map.PortCd);
        }
    }, undefined, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#ViaPort").val(rd.PORT_CD);
    }));

    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.TrgnUrl, config: LookUpConfig.TrgnLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TRGN", undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.CD);
    }));


    registBtnLookup($("#StateLookup"), {
        item: '#State', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#State").val(map.StateCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#State").val(map.STATE_CD);
    }));

    registBtnLookup($("#LspNoLookup"), {
        item: '#LspNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNo").val(rd.PARTY_NO);
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#CustomerLookup"), {
        item: '#Customer', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#Customer").val(map.PartyNo);
            $("#CustomerNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Customer").val(rd.PARTY_NO);
        $("#CustomerNm").val(rd.PARTY_NAME);
    }));

    registBtnLookup($("#ModelLookup"), {
        item: '#Model', url: rootPath + LookUpConfig.DivisionUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#Model").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TSPA", null, function ($grid, rd, elem, rowid) {
        $("#Model").val(rd.CD);
    }));

    /*registBtnLookup($("#TermLookup"), {
        item: '#Term', url: rootPath + LookUpConfig.CargoTypeUrl, config: LookUpConfig.BSCodeLookup, param: "", selectRowFn: function (map) {
            $("#Term").val(map.Cd);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCGT", null, function ($grid, rd, elem, rowid) {//TVTW
        $("#Term").val(rd.CD);
    }));
    */

    _initUI(["MBApply", "MBInvalid", "MBEdoc", "MBApprove", "MBErrMsg", "MBSearch"]);//初始化UI工具栏

    //$("#SeqNo").removeAttr('required');
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

            var trnOptions = data.TCGT || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#Term"), trnOptions);

            trnOptions = data.TDTK || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TrackWay"), trnOptions);
        }
    });
}
