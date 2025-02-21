var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null, grids: [] };
var $SubGrid, $SubGrid2,addhand=false;
$(function () {
    Schemas = JSON.parse(decodeHtml(Schemas));
    CommonFunc.initField(Schemas);
    setdisabled(true);

    $SubGrid = $("#SubGrid");
    
    var approvetype = approveroles.split(';');
    var _statusgroup = [];
    $.each(approvetype, function (index, val) {
        var _val = val.split(':');
        var _object = {};
        if (_val.length >= 2) {
            _object.cd = _val[0];
            _object.cdDescp = _val[1];
        }
        _statusgroup.push(_object);
    });
    appendSelectOption($("#ApproveTo"), _statusgroup);

    var BookingLookup = {
        caption: "@Resources.Locale.L_QTSetup_Partial",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: 'UId', title: 'UId', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'ShipmentId', title: '@Resources.Locale.L_QTSetup_ShipNo', index: 'ShipmentId', width: 120, init:true , align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            { name: 'DnNo', title: '@Resources.Locale.L_QTSetup_DeliverNo', index: 'DnNo', width: 80, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
            //{ name: 'BuyerCd', title: 'Buyer', index: 'BuyerCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierCd', title: 'Supplier', index: 'SupplierCd', width: 120, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] },
            //{ name: 'SupplierNm', title: 'Supplier Name', index: 'SupplierNm', width: 250, align: 'left', sorttype: 'string', hidden: false, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'nu', 'nn', 'in', 'ni'] }
        ]
    };

    _handler.saveUrl = rootPath + "QTManage/SaveContractData";
    _handler.inquiryUrl = rootPath + "QTManage/GetSMCTMDetail";
    _handler.config = BookingLookup;

    _handler.addData = function () {
        //初始化新增数据
        var dep = getCookie("plv3.passport.dep"),ext = getCookie("plv3.passport.ext");
        var data = {
            "CreateBy": userId, "CreateDate": getDate(0, "-"), "Cmp": cmp , "CreateDep": dep, "CreateExt": ext,"CtDate": getDate(0, "-"),"EffectTo": getDate(30, "-"),"Status":"N","Exten":"N", "Location": cmp, "LocName": CmpNm
        };
        data[_handler.key] = uuid();
        setFieldValue([data]);
        _handler.loadGridData("SubGrid", $SubGrid[0], [], [""]);
        addhand = true;
        getAutoNo("JobNo", "rulecode=CT_NO&cmp=" + cmp);        
        $("#RfqNo").attr('readonly', false);
        $("#Region").attr('readonly', false);
        $("#Cnty").attr('readonly', false);
        $("#LspCt").attr('readonly', false);
        $("#LspPic").attr('readonly', false);
        $("#QuotNo").attr('readonly', false);
        $("#LocPic").attr('readonly', false);
    }

    _handler.saveData = function (dtd) {
        //var containerArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var changeData = getChangeValue();//获取所有改变的值
        //changeData["sub"] = containerArray;
        var data = { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: false };
        data["u_id"] = encodeURIComponent($("#UId").val());
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

    _handler.setFormData = function (data) {
        if (data["main"])
            _handler.topData = (data["main"].length > 0) ? data["main"][0] || {} : {};
        else
            _handler.topData = [{}];

        setFieldValue(data["main"] || [{}]);
        setdisabled(true);
        setToolBtnDisabled(true);
        addhand = false;
        MenuBarFuncArr.initEdoc($("#UId").val(), groupId, $("#Cmp").val(), "*");
        MenuBarFuncArr.Enabled(["MBCopy", "btn01", "btn02", "MBVoid","MBEdoc"]);
        //MenuBarFuncArr.Enabled(["SEND_BTN"]);
        var Exten = data.main[0].Exten;
        ExtenChange(Exten);
    }
    
    _handler.loadMainData = function (map) {
        if (!map || !map[_handler.key]) {
            setFieldValue([{}]);
            return;
        }
        ajaxHttp(rootPath + "QTManage/GetSMCTMDetail", { uId: map.UId, loading: true },// LookUpConfig.FCLBookingItemUrl
            function (data) {
                if (_handler.setFormData) {
                    _handler.setFormData(data);
                }
            });
        
    }

    _handler.beforSave = function () {
        var Exten = $("#Exten").val();
        if (Exten == "Y")
        {
            var ExtenDate = $("#ExtenDate").val();
            if (ExtenDate == "" || ExtenDate == undefined)
            {
                alert("@Resources.Locale.L_ContractSetup_ExtenDateWarning");
                return false;
            }
        }    
        return true;
    }

    registBtnLookup($("#LspNoLookup"), {
        item: '#LspNo', url: rootPath + LookUpConfig.PartyNoUrl, config: LookUpConfig.PartyNoLookup, param: "", selectRowFn: function (map) {
            $("#LspNo").val(map.PartyNo);
            $("#LspNm").val(map.PartyName);
        }
    }, undefined, LookUpConfig.GetPartyNoAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#LspNm").val(rd.PARTY_NAME);
    }));

    $("#LocationLookup").v3Lookup({
        url: rootPath + "TPVCommon/GetSiteCmpData",
        gridFunc: function (map) {
            $("#Location").val(map.Cd);
            $("#LocName").val(map.CdDescp);
        },
        lookUpConfig: LookUpConfig.SiteLookup,
        baseConditionFunc: function () { return ""; },
        responseMethod: function () { return ""; }
    });

    $("#Location").v3AutoComplete({
        params: "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP%",
        keyinNum: "1",
        returnValue: "CMP&NAME=showValue,CMP,NAME",
        callBack: function (event, ui) {
            var map = ui.item.returnValue;
            $("#Location").val(map.CMP);
            $("#LocName").val(map.NAME);
            return false;
        },
        dymcFunc: function () {
            return "";
        },
        clearFunc: function () {
            $("#Icur").val("");
        }
    });
    
    registBtnLookup($("#RegionLookup"), {
        item: '#Region', url: rootPath + LookUpConfig.ProvinceUrl, config: LookUpConfig.ProvinceLookup, param: "", selectRowFn: function (map) {
            $("#Region").val(map.RegionCd);
        }
    }, undefined, LookUpConfig.GetProvinceAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Region").val(rd.REGION_CD);
    }, function ($grid, elem) {
        $("#Region").val("");
    }));

    registBtnLookup($("#CntyLookup"), {
        item: '#Cnty', url: rootPath + LookUpConfig.CountryUrl, config: LookUpConfig.CountryLookup, param: "", selectRowFn: function (map) {
            $("#Cnty").val(map.CntryCd);
        }
    }, undefined, LookUpConfig.GetCountryAuto(groupId, undefined, function ($grid, rd, elem) {
        $("#Cnty").val(rd.CNTRY_CD);
    }, function ($grid, elem) {
        $("#Cnty").val("");
    }));
    

    //MenuBarFuncArr.MBEdoc = function (thisItem) {
    //    initEdoc(thisItem, { jobNo: $("#UId").val(), GROUP_ID: groupId, CMP: cmp, STN: "*" });
    //}

    MenuBarFuncArr.MBEdit = function (thisItem) {
        $("#JobNo").attr('readonly', true);
        $("#RfqNo").attr('readonly', true);
    }

    _handler.beforLoadView = function () {
        var keys = ["Uid","DnNo"];
        for (var i = 0; i < keys.length; i++) {
            $("#" + keys[i]).attr('isKey', true);
        }

        var requires = ["JobNo","LspNo","Location"];
        for (var i = 0; i < requires.length; i++) {
            $("#" + requires[i]).attr('required', true);
            $("[for=" + requires[i] + "]").css("color", "rgb(255, 0, 0)");
        }
        var readonlys = ["JobNo","Region","Cnty","Status"];
        for (var i = 0; i < readonlys.length; i++) {
            $("#" + readonlys[i]).attr('readonly', true);
        }
        
    }

    MenuBarFuncArr.AddMenu("btn01", "glyphicon glyphicon-ok", "@Resources.Locale.L_ActManage_Pass", function () {
       
        return Approve_click();
        
        var Status = $("#Status").val();
        if (Status == "D") {
            CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_28", 500, "warning");
            return false;
        }
        if (_uid) {
            $.ajax({
                async: true,
                url: rootPath + "ActManage/ActPass",
                type: 'POST',
                data: { UId: _uid },
                dataType: "json",
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_29", 500, "warning");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    /*if (result.message != "success") 
                    {
                        CommonFunc.Notify("", result.message, 500, "warning");
                        CommonFunc.ToogleLoading(false);
                        return false;
                    }*/

                    var formData = $.parseJSON(result.returnData.Content);
                    _handler.setFormData(formData);
                    CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_30", 500, "success");
                    CommonFunc.ToogleLoading(false);
                }
            });
        }
    });

    MenuBarFuncArr.AddMenu("btn02", "glyphicon glyphicon-remove", "@Resources.Locale.L_ActManage_Refuse", function () {

            BackApprove_click();
            return;

        var CheckDescp = $("#CheckDescp").val();
        var Remark = $("#Remark").val();
        var Status = $("#Status").val();
        if (Status == "C") {
            CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_31", 500, "warning");
            return false;
        }
        if (CheckDescp == "" || CheckDescp == null) {
            CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Script_6", 1300, "warning");
            $("#CheckDescp").setfocus();
            return false;
        }
        else {
            if (_uid) {
                $.ajax({
                    async: true,
                    url: rootPath + "ActManage/ActReject",
                    type: 'POST',
                    data: { UId: _uid, CheckDescp: CheckDescp, Remark: Remark },
                    dataType: "json",
                    beforeSend: function () {
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_29", 500, "warning");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        if (result.message != "success") {
                            CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_34", 500, "success");
                            $("#Status").val("D");
                            CommonFunc.ToogleLoading(false);
                            return false;
                        }


                        var formData = $.parseJSON(result.returnData.Content);
                        _handler.setFormData(formData);
                        CommonFunc.Notify("", "@Resources.Locale.L_ActCheckSetup_Scripts_30", 500, "success");
                        CommonFunc.ToogleLoading(false);
                    }
                });
            }
        }

    });

    MenuBarFuncArr.AddMenu("MBVoid", "glyphicon glyphicon-bell", "@Resources.Locale.L_MenuBar_Audit", function () {
        if (!_handler.topData || isEmpty(_handler.topData[_handler.key])) {
            CommonFunc.Notify("", _handler.lang.tip1, 500, "warning");
            return false;
        }
    });
    
    _initUI(["MBApply", "MBApprove", "MBErrMsg"]);//初始化UI工具栏

    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }
    
});
function ExtenChange(val)
{
    if (val == "Y") {
        $(".ExtenDate").css("display", "block");
    }
    else {
        $(".ExtenDate").css("display", "none");
    }
}
function Approve_click() {
    var jobno = [];
    var uid = [];
    var id = $("#UId").val();
    jobno.push($("#JobNo").val());
    uid.push(id);

    if (!id) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    var iscontinue = window.confirm("@Resources.Locale.L_ActManage_is" + jobno.toString() + "】@Resources.Locale.L_ActCheckSetup_Script_11");
    if (!iscontinue) {
        return;
    }
    CommonFunc.ToogleLoading(true);
    $.ajax({
        async: true,
        url: rootPath + "QTManage/ApproveContract",
        type: 'POST',
        data: {
            "UId": uid.toString(),
            "JobNo": jobno.toString()
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.ToogleLoading(false);
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
        },
        success: function (result) {
            //var resJson = $.parseJSON(result)
            if (result.message == "success") {

            }
            CommonFunc.Notify("", result.message, 500, "success");
            MenuBarFuncArr.MBCancel();
        }
    });
}

function BackApprove_click() {
    $("#BackRemark").val("");
    var uid = $("#UId").val();
    if (!uid) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    $("#ApproveBack").modal("show");
}

function BackContApprove() {
    var backremark = $("#BackRemark").val();
    if (backremark == "") {
        CommonFunc.Notify("", "@Resources.Locale.L_ActManage_EnterReason", 500, "warning");
        return;
    }

    var uid = $("#UId").val();
    var debitno = $("#DebitNo").val();
    var ApproveType = $("#ApproveType").val();
    var approveTo = $("#ApproveTo").val();

    if (!uid) {
        CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "QTManage/ApproveBackContract",
        type: 'POST',
        data: {
            "UId": uid,
            "DebitNo": debitno,
            "ApproveType": ApproveType,
            "ApproveTo": approveTo,
            "BackRemark": backremark
        },
        "complete": function (xmlHttpRequest, successMsg) {

        },
        "error": function (xmlHttpRequest, errMsg) {
            var resJson = $.parseJSON(errMsg)
            CommonFunc.Notify("", resJson.message, 500, "warning");
            $("#CloseBackWin").trigger("click");
        },
        success: function (result) {
            //var resJson = $.parseJSON(result)
            CommonFunc.Notify("", result.message, 500, "warning");
            $("#CloseBackWin").trigger("click");
            //$("#SummarySearch").trigger("click");
            MenuBarFuncArr.MBCancel();
        }
    });
}

