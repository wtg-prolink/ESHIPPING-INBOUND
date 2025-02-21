var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var _tran = "3";
$(function () {
    var $SubGrid = $("#SubGrid");
    
    var DruleLookup = {
        caption: "@Resources.Locale.L_DNManage_DelcSetupSer",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', showname: 'ID', sorttype: 'string', hidden: true, viewable: false },
            { name: 'ShipmentId', title: 'ShipmentId', index: 'ShipmentId', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Year ', title: 'Year', index: 'Year', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Weekly', title: '@Resources.Locale.L_ContainUsage_Week', index: 'Weekly', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Status', title: '@Resources.Locale.L_GateReserve_Status', index: 'Status', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'CntrQty', title: '@Resources.Locale.L_BaseLookup_CntNumber', index: 'CntrQty', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'PriceTerm', title: '@Resources.Locale.L_ShipmentID_PayTermCd', index: 'PriceTerm', width: 150, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'FreightTerm', title: 'FreightTerm', index: 'FreightTerm', width: 80, align: 'left', sorttype: 'string', hidden: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Carrier', title: 'Carrier', index: 'Carrier', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POR', title: 'POR', index: 'POR', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'POL', title: 'POL', index: 'POL', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Dest', title: 'Dest', index: 'Dest', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Feu', title: '@Resources.Locale.L_DNApproveManage_Feu', index: 'Feu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Teu', title: '@Resources.Locale.L_AirBookingSetup_Script_86', index: 'Teu', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cur', title: '@Resources.Locale.L_InvPkgSetup_Cur', index: 'Cur', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: 'Cost', title: '@Resources.Locale.L_DNApproveManage_Cost', index: 'Cost', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };
    var DruleItemUrl = "DNManage/GetChangeInfo";

    _handler.saveUrl = rootPath + "DNManage/SaveChangePodBook";
   // _handler.inquiryUrl = rootPath + "DNManage/GetCustomsBookData";;//LookUpConfig.NotifyUrl
    _handler.config = DruleLookup;

    _handler.addData = function () {
        var data = {};
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        data[_handler.key] = uuid();
        setFieldValue([data]);
    }
    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];
        if (data["sub"])
            _handler.loadGridData("SubGrid", $SubGrid[0], data["sub"], [""]);
        else
            _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        showtruckdiv($("#TranType").val());
        setToolBtnDisabled(true);
        //init edoc for get all dn and shipment edco all in one view
        //var multiEdocData = [];
        //ajaxHttp(rootPath + "DNManage/GetDNData", { DnNo: $("#CombineInfo").val(), loading: true },
        //function (data) {
        //    if (data == null) {
        //        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //    } else {
        //        $(data.dn).each(function (index) {
        //            multiEdocData.push({ jobNo: data.dn[index].UId, 'GROUP_ID': data.dn[index].GroupId, 'CMP': data.dn[index].Cmp, 'STN': '*' });
        //        });
        //        MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*", multiEdocData);
        //    }
        //});
        ////MenuBarFuncArr.initEdoc($("#UId").val(), _handler.topData["GroupId"], _handler.topData["Cmp"], "*");
        //MenuBarFuncArr.Enabled([ "MBEdoc"]);
    }
    _handler.beforSave = function () {
        var iscontinue = window.confirm("确认保存，并发送改港申请吗？");
        if (!iscontinue) {
            return false;
        }
    }

    _handler.saveData = function (dtd) {
        var changereason = $("#ChangeReason").val();
        var changeData = getChangeValue();//获取所有改变的值
        var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        changeData["ChangeGrid"] = containerArray;
        var uid = $("#UId").val();
        var shipmentid = $("#ShipmentId").val();
        ajaxHttpSaveBar(dtd, _handler.saveUrl, { "changedData": encodeURIComponent(JSON.stringify(changeData)), "UId": uid, "ShipmentId": shipmentid, "changereason": changereason, autoReturnData: false, "TranType": "F" },
            function (result) {
                //_topData = keyData["mt"];
                if (result.message) {
                    alert(result.message);
                    return false;
                }
                MenuBarFuncArr.MBCancel();
                //else if (_handler.setFormData)
                //    _handler.setFormData(result);
                return true;
            });
    }

    _handler.cancelData = function () {
        //alert("asdf");
        $("#ChangeReason").val("");
        $("#ChangeReason").attr('disabled', true);
        if (_handler.loadMainData)
            _handler.loadMainData(_handler.topData);
    }

    _handler.editData = function () {
        //gridEditableCtrl({ editable: false, gridId: 'SubGrid' });
        //if ($("#Border").val() != "C") {
        //    gridEditableCtrl({ editable: true, gridId: 'SubGrid' });
        //}
    }

    _handler.beforEdit = function () {
    }

    _handler.afterEdit = function () {

        setdisabled(true);
        SetBrokerDisable(false);
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
            });
    }

    _handler.intiGrid("SubGrid", $SubGrid, {
        colModel: returnPartyModel("SubGrid", $SubGrid), caption: 'Party', delKey: ["UId", "PartyType"],
        onAddRowFunc: function (rowid) {
            var maxSeqNo = $SubGrid.jqGrid("getCol", "PartyNo", false, "max");
        },
        beforeSelectRowFunc: function (rowid) {
            if (rowid != null && rowid.indexOf("jqg") >= 0) {
                $SubGrid.setColProp('PartyType', { editable: true });
            } else {
                $SubGrid.setColProp('PartyType', { editable: false });
            }
        },
        beforeAddRowFunc: function (rowid) {
            $SubGrid.setColProp('PartyType', { editable: true });
        }
    });

    var Dn_colmodel = [
        { name: 'UId', title: 'Uid', index: 'UId', sorttype: 'string', width: 120, editable: false, hidden: true },
        { name: 'OldInfo', title: '', index: 'Old Info', sorttype: 'string', editable: false, hidden: false, editable: true },
        { name: 'NewInfo', title: 'New info', index: 'ExportNo', sorttype: 'string', width: 120, hidden: false, editable: false },
        { name: 'ChangeBy', title: 'Change User', index: 'ApproveNo', sorttype: 'string', width: 120, hidden: false, editable: false },
       {
           name: 'ChangeDate', title: 'Change Date', index: 'AskTim', width: 150, align: 'left', hidden: false, editable: true, sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
           editoptions: myEditDateInit,
           formatter: 'date',
           formatoptions: {
               srcformat: 'ISO8601Long',
               newformat: 'Y-m-d',
               defaultValue: ""
           }
       }
    ];

    registBtnLookup($("#PpodCdLookup"), {
        item: '#PpodCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PpodCd").val(map.CntryCd + map.PortCd);
            $("#PpodName").val(map.PortNm);
        }
    }, { focusItem: $("#PpodCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpodCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PpodName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PdestCdLookup"), {
        item: '#PdestCd', url: rootPath + LookUpConfig.CityPortUrl, config: LookUpConfig.CityPortLookup, param: "", selectRowFn: function (map) {
            $("#PdestCd").val(map.CntryCd + map.PortCd);
            $("#PdestName").val(map.PortNm);
        }
    }, { focusItem: $("#PdestCd") }, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PdestCd").val(rd.CNTRY_CD + rd.PORT_CD);
        $("#PdestName").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PpodTruckLookup"), {
        item: '#PpodTruck', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PpodTruck").val(map.PortCd);
            $("#PpodtruckNm").val(map.PortNm);
        }
    }, { focusItem: $("#PpodTruck") }, LookUpConfig.TruckPortCdAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PpodTruck").val(rd.PORT_CD);
        $("#PpodtruckNm").val(rd.PORT_NM);
    }));

    registBtnLookup($("#PdestTruckLookup"), {
        item: '#PdestTruck', url: rootPath + LookUpConfig.TruckPortCdUrl, config: LookUpConfig.TruckPortCdLookup, param: "", selectRowFn: function (map) {
            $("#PdestTruck").val(map.PortCd);
            $("#PdesttruckNm").val(map.PortNm);
        }
    }, undefined, LookUpConfig.GetCityPortAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#PdestTruck").val(rd.PORT_CD);
        $("#PdesttruckNm").val(rd.PORT_NM);
    }));

    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBApprove", "MBErrMsg"]);//初始化UI工具栏
    MenuBarFuncArr.DelMenu(["MBAdd", "MBDel", "MBInvalid", "MBSearch"]);

    //$("#SeqNo").removeAttr('required');
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    $("#Voyage1Lookup").click(function () {
        var data = _handler.topData || {};
        var vessel = data.Vessel1 || "";
        showVessel(vessel);
    });
    function showVessel(vessel) {
        if (isEmpty(vessel)) {
            CommonFunc.Notify("", "@Resources.Locale.L_SeaTransport_NoVesRe", 500, "warning");
            return;
        }
        $('#VesselMapFrame').attr("src", "https://ipdev.lsphub.com/iport/ShowVessel.aspx?vsl_nm=" + encodeURIComponent(vessel));
        $('#VesselMap').modal('show');
    };
});

function SetBrokerDisable(disabled) {
    var readonlys = ["PpodCd", "PpodName", "PdestCd", "PdestName", "ChangeReason", "PpodTruck", "PpodtruckNm", "PdestTruck", "PdesttruckNm"];
    for (var i = 0; i < readonlys.length; i++) {
        $("#" + readonlys[i]).attr('disabled', disabled);
        $("#" + readonlys[i]).parent().find("button").attr("disabled", disabled);
    }
    _handler.gridEditableCtrl(disabled);
    gridEditableCtrl({ editable: true, gridId: 'SubGrid' });
}

var _bu = "P";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "TKBL/GetSelects",
        type: 'POST',
        data: { type: encodeURIComponent("FCLBooking") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            console.log(data);
            var trnOptions = data.TTRN || [];
            if (trnOptions.length > 0)
                _tran = trnOptions[0]["cd"];
            appendSelectOption($("#TranMode"), trnOptions);

            trnOptions = data.PK || [];
            if (trnOptions.length > 0)
                _mt = trnOptions[0]["cd"];
            appendSelectOption($("#LoadingFrom"), trnOptions);
            appendSelectOption($("#LoadingTo"), trnOptions);

            if (_handler.topData) {
                $("#TranMode").val(_handler.topData["TranMode"]);
                $("#LoadingFrom").val(_handler.topData["LoadingFrom"]);
                $("#LoadingTo").val(_handler.topData["LoadingTo"]);
            }
        }
    });
}


function getNowFormatDate() {

    var date = new Date();

    var seperator1 = "-";

    var seperator2 = ":";

    var month = date.getMonth() + 1;

    var strDate = date.getDate();

    if (month >= 1 && month <= 9) {

        month = "0" + month;

    }

    if (strDate >= 0 && strDate <= 9) {

        strDate = "0" + strDate;

    }

    var currentdate = date.getFullYear() + seperator1 + month + seperator1 + strDate

            + " " + date.getHours() + seperator2 + date.getMinutes()

            + seperator2 + date.getSeconds();

    return currentdate;

}

function showtruckdiv(trantype) {
    if (trantype == "D" || trantype == "T") {
        $('#ppoddiv').css('display', 'none');
        $('#ppodbtndiv').css('display', 'none');
        $('#pdestdiv').css('display', 'none');
        $('#pdestbtndiv').css('display', 'none');

        $('#pdesttruckbtndiv').css('display', 'block');
        $('#pdesttruckdiv').css('display', 'block');
        $('#ppodtruckbtndiv').css('display', 'block');
        $('#ppodtruckdiv').css('display', 'block');
    } else {
        $('#ppoddiv').css('display', 'block');
        $('#ppodbtndiv').css('display', 'block');
        $('#pdestdiv').css('display', 'block');
        $('#pdestbtndiv').css('display', 'block');

        $('#pdesttruckbtndiv').css('display', 'none');
        $('#pdesttruckdiv').css('display', 'none');
        $('#ppodtruckbtndiv').css('display', 'none');
        $('#ppodtruckdiv').css('display', 'none');
    }
}



