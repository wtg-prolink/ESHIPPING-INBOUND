//var _handler = _handler || { search: null, beforDel: null, delData: null, beforAdd: null, addData: null, beforEdit: null, editData: null, cancelData: null, beforSave: null, saveData: null, loadMainData: null, topData: {}, key: "UId", inquiryUrl: null, config: null };
/*
var _refGate = [];
$(function () {
    var DruleLookup = {
        caption: "月台预约设定查询",
        sortname: "CreateDate",
        refresh: false,
        columns: [
            { name: "UId", title: "Uid", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "ReveseNo", title: "预约号码", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "CallDate", title: "叫柜时间", width: 250, sorttype: "string", hidden: true, viewable: false },
            { name: "UseDate", title: "用柜时间", width: 90, sorttype: "string", hidden: false, viewable:LtruckNo true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "Status", title: "状态", width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "ReveseDate", title: "预约时间", width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            //{ name: "Customer", title: "DN No", width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "GateNo", title: "确认月台", width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
            { name: "ReserveBy", title: "预约人", width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
    };
    var DruleItemUrl = "IbGateManage/GetGateItem";

    _handler.saveUrl = rootPath + "IbGateManage/SaveGateReseve";
    _handler.inquiryUrl = rootPath + "IbGateManage/GetGateData";;//LookUpConfig.NotifyUrl
    _handler.config = DruleLookup;
    //getSelectOptions();

    _handler.addData = function () {
        //初始化新增数据
        var data = {};
        $("#CreateBy").val(userId);
        $("#UId").removeAttr('required');
        $("#GateNo").removeAttr('required');
        data[_handler.key] = uuid();
        setFieldValue([data]);
        $("#Status").val("D");
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

    

    registBtnLookup($("#TruckerLookup"), {
        url: rootPath + LookUpConfig.GetCompanyByPartyTypeUrl, config: LookUpConfig.CompanyByPartyTypeLookup, param: "", selectRowFn: function (map) {
            $("#Trucker").val(map.Cmp);
        }
    });

    registBtnLookup($("#DeliveryPortLookup"), {
        url: rootPath + LookUpConfig.DeliveryUrl, config: LookUpConfig.DeliveryLookup, param: "", selectRowFn: function (map) {//sopt_PartyType=eq&PartyType=TK
            $("#State").val(map.State);
            var cd = map.CntryCd + map.PortCd;
            $("#DeliveryPort").val(cd);   
        }
    });


    _initUI(["MBApply", "MBInvalid", "MBCopy", "MBEdoc", "MBApprove", "MBErrMsg", "MBSearch", "MBAdd"]);//初始化UI工具栏

    //$("#SeqNo").removeAttr('required');
    if (!isEmpty(_uid)) {
        _handler.topData = { UId: _uid };
        MenuBarFuncArr.MBCancel();
    }

    getSelectOptions();

    $("#Cmp").on("change", function () {
        var val = $(this).val();
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/GetWareHouseByCmp",
            type: 'POST',
            dataType: "json",
            data: { cmp: encodeURIComponent(val) },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
            },
            success: function (data) {
                var mtOptions = data.rows || [];
                if (mtOptions.length > 0) {
                    _mt = mtOptions[0]["WsCd"];
                    $("#RefGate").val(mtOptions[0]["RefGate"]);
                }
                var _seleWsCd = $("#WsCd");
                _seleWsCd.empty();
                _refGate=[];
                $.each(data.rows, function (idx, option) {
                    _refGate.push(option.RefGate);
                    _seleWsCd.append("<option value=\"" + option.WsCd + "\">" + option.WsNm + "</option>");
                });
            }
        });
    });

    $("#WsCd").on("change", function () {
        var _index = $(this).context.selectedIndex;
        $("#RefGate").val(_refGate[_index]);
        
    });
});

var _mt = "";
function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + LookUpConfig.GetCmpUrl,
        type: 'POST',
        dataType: "json",
        data: { type: encodeURIComponent("NRSSetup") },
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var mtOptions = data.rows || [];
            if (mtOptions.length > 0)
                _mt = mtOptions[0]["Cmp"];
            _appendSelectOption($("#Cmp"), mtOptions);
        }
    });
}

function _appendSelectOption(selectId, options) {
    selectId.empty();
    $("#WsCd").empty();
    $.each(options, function (idx, option) {
        selectId.append("<option value=\"" + option.Cmp + "\">" + option.Name + "</option>");
    });
}
*/

var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

$(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    setdisabled(true);

    _initMenu();
    initLoadData(_uid);

    //國家放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetCountryDataForLookup";
    options.registerBtn = $("#CntryCdLookup");
    options.focusItem = $("#CntryCd");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#CntryCd").val(map.CntryCd);
        $("#CntryNm").val(map.CntryNm);
    }

    options.lookUpConfig = LookUpConfig.CntryLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#CntryCd", 1, "", "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);
        $("#CntryNm").val(ui.item.returnValue.CNTRY_NM);
        return false;
    });

    //倉庫放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmwhForLookup";
    options.param = "";
    options.registerBtn = $("#WsCdLookup");
    options.focusItem = $("#SearchWsCd");
    options.isMutiSel = true;
    options.baseConditionFunc = function () {
        var searchcmp = $("#SearchCmp").val();
        return " CMP='" + searchcmp + "'";
    }
    options.gridFunc = function (map) {
        $("#SearchWsCd").val(map.WsCd);
        $("#SearchMfNo").val(map.MfNo)
    }

    options.lookUpConfig = LookUpConfig.SMWHLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#SearchWsCd", 1, "", "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=", "WS_CD=showValue,WS_CD,WS_NM,MF_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.WS_CD);
        $("#SearchMfNo").val(ui.item.returnValue.MF_NO);
        return false;
    }, function () { "CMP=" + $("#SearchCmp").val();});

    //倉庫放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmwhForLookup";
    options.param = "";
    options.registerBtn = $("#WsCdLookup1");
    options.focusItem = $("#WsCd");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#WsCd").val(map.WsCd);
        $("#MfNo").val(map.MfNo);
    }

    options.lookUpConfig = LookUpConfig.SMWHLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#WsCd", 1, "", "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=", "WS_CD=showValue,WS_CD,WS_NM,MF_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.WS_CD);
        $("#MfNo").val(ui.item.returnValue.MF_NO);
        return false;
    });

    //月台放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmwhgtForLookup";
    options.param = "";
    options.registerBtn = $("#GateNoLookup");
    options.focusItem = $("#GateNo");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var WsCd = $("#WsCd").val()

        return " WS_CD='"+WsCd+"'";
    }
    options.gridFunc = function (map) {
        var status = map.Status;

        if(status != "Y")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CanUseGate", 1000, "warning");
        }
        else
        {
            $("#GateNo").val(map.GateNo);
        }
    }

    options.lookUpConfig = LookUpConfig.SMWHGTLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#GateNo", 1, "", "dt=smwhgt&GATE_NO=", "GATE_NO=showValue,GATE_NO,STATUS", function (event, ui) {
        var status = ui.item.returnValue.STATUS;
        if(status != "Y")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CanUseGate", 1000, "warning");
        }
        else
        {
            $(this).val(ui.item.returnValue.GATE_NO);
        }
        
        return false;
    }, function(){
        var WsCd = $("#WsCd").val()

        return " WS_CD="+WsCd;
    });

    //車號放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBstruckcForLookup";
    options.param = "";
    options.registerBtn = $("#TruckNoLookup");
    options.focusItem = $("#TruckNo");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO='"+Trucker+"'";
    }
    options.gridFunc = function (map) {
        $("#TruckNo").val(map.TruckNo);
    }

    options.lookUpConfig = LookUpConfig.BstruckcLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#TruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.TRUCK_NO);
        
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

    //離廠車號放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBstruckcForLookup";
    options.param = "";
    options.registerBtn = $("#LtruckNoLookup");
    options.focusItem = $("#LtruckNo");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO='"+Trucker+"'";
    }
    options.gridFunc = function (map) {
        $("#LtruckNo").val(map.TruckNo);
    }

    options.lookUpConfig = LookUpConfig.BstruckcLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#LtruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.TRUCK_NO);
        
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

    //入厂司機放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBstruckdForLookup";
    options.param = "";
    options.registerBtn = $("#DriverLookup");
    options.focusItem = $("#Driver");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO='"+Trucker+"'";
    }
    options.gridFunc = function (map) {
        $("#Driver").val(map.DriverName);
        $("#Tel").val(map.DriverPhone);
        $("#DriverId").val(map.DriverId);
    }

    options.lookUpConfig = LookUpConfig.BstruckdLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Driver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_NAME,DRIVER_PHONE,DRIVER_ID", function (event, ui) {
        $(this).val(ui.item.returnValue.DRIVER_NAME);
        $("#Tel").val(ui.item.returnValue.DRIVER_PHONE);
        $("#DriverId").val(ui.item.returnValue.DRIVER_ID);
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

    //離厂司機放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBstruckdForLookup";
    options.param = "";
    options.registerBtn = $("#LdriverLookup");
    options.focusItem = $("#Ldriver");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO='"+Trucker+"'";
    }
    options.gridFunc = function (map) {
        $("#Ldriver").val(map.DriverName);
        $("#Ltel").val(map.DriverPhone);
        $("#LdriverId").val(map.DriverId);
    }

    options.lookUpConfig = LookUpConfig.BstruckdLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#Ldriver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_NAME,DRIVER_PHONE,DRIVER_ID", function (event, ui) {
        $(this).val(ui.item.returnValue.DRIVER_NAME);
        $("#Ltel").val(ui.item.returnValue.DRIVER_PHONE);
        $("#LdriverId").val(ui.item.returnValue.DRIVER_ID);
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

    var $ScuftGrid = $("#ScuftGrid");
    //Shipper放大鏡
    setSmptyData("TruckerLookup", "Trucker", "", "CR");

    if(page == "C")
    {
        //倉庫放大鏡
        var options = {};
        options.gridUrl = rootPath + "TPVCommon/GetSmwhForLookup";
        options.param = "";
        options.registerBtn = $("#WsCdLookup1");
        options.focusItem = $("#TempWscd");
        options.isMutiSel = true;
        options.gridFunc = function (map) {
            $("#TempWscd").val(map.WsCd);
        }

        options.lookUpConfig = LookUpConfig.SMWHLookup;
        initLookUp(options);

        CommonFunc.AutoComplete("#TempWscd", 1, "", "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=", "WS_CD=showValue,WS_CD,WS_NM", function (event, ui) {
            $(this).val(ui.item.returnValue.WS_CD);
            return false;
        });

        //月台放大鏡
        var options = {};
        options.gridUrl = rootPath + "TPVCommon/GetSmwhgtForLookup";
        options.param = "";
        options.registerBtn = $("#TempGatenoLookup");
        options.focusItem = $("#TempGateno");
        options.isMutiSel = true;
        options.baseConditionFunc = function(){
            var WsCd = $("#TempWscd").val()

            return " WS_CD='"+WsCd+"'";
        }
        options.gridFunc = function (map) {
            var status = map.Status;

            if(status != "Y")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CanUseGate", 1000, "warning");
            }
            else
            {
                $("#TempGateno").val(map.GateNo);
            }
        }

        options.lookUpConfig = LookUpConfig.SMWHGTLookup;
        initLookUp(options);

        CommonFunc.AutoComplete("#TempGateno", 1, "", "dt=smwhgt&GATE_NO=", "GATE_NO=showValue,GATE_NO,STATUS", function (event, ui) {
            var status = ui.item.returnValue.STATUS;
            if(status != "Y")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CanUseGate", 1000, "warning");
            }
            else
            {
                $(this).val(ui.item.returnValue.GATE_NO);
            }
            
            return false;
        }, function(){
            var WsCd = $("#TempWscd").val()

            return " WS_CD="+WsCd;
        });
    }

    //setSmptyData("CmpLookup", "SearchCmp", "CmpNm", "LC");
    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOpt.registerBtn = $("#CmpLookup");
    CmpOpt.focusItem = $("#SearchCmp");
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#SearchCmp").val(value);
        $("#CmpNm").val(map.CdDescp);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#SearchCmp", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        $("#CmpNm").val(ui.item.returnValue.NAME);
        return false;
    });
    setBscData("RegionCdLookup", "RegionCd", "RegionNm", "TRGN");
    setBscData("TwuLookup", "Twu", "", "UT");

    if(page == "R")
    {
        $("#GateNo").on("change", function(){
            $("#TempGateno").val($(this).val());
        });
    }

    var ScufcolModel = [
       { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
       { name: 'UFid', title: 'UFid', index: 'UFid', sorttype: 'string', editable: false, hidden: true },
       { name: 'DnNo', title: 'Dn No', index: 'DnNo', sorttype: 'string', width: 100, hidden: true, editable: false },
       //{ name: 'Cuft', title: '类型', index: 'Cuft', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'L', title: '@Resources.Locale.L_AirBookingSetup_Script_87', index: 'L', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'W', title: '@Resources.Locale.L_AirBookingSetup_Script_88', index: 'W', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'H', title: '@Resources.Locale.L_AirBookingSetup_Script_89', index: 'H', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'Pkg', title: '@Resources.Locale.L_DNManage_PkgNum', index: 'Pkg', width: 120, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 2, defaultValue: '0.00' }, editable: true, hidden: false },
       { name: 'PkgUnit', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'PkgUnit', edittype: 'custom', sorttype: 'string', width: 100, hidden: false, editable: true },
       { name: 'Vw', title: '@Resources.Locale.L_AirBookingSetup_Script_90', index: 'Vw', width: 140, align: 'right', formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 4, defaultValue: '0.00' }, editable: false, hidden: false }
    ];
    new genGrid(
        $ScuftGrid,
        {
            datatype: "local",
            data: [],
            loadonce: true,
            colModel: ScufcolModel,
            caption: '@Resources.Locale.L_DNManage_DNSizeInfo',
            height: "AUTO",
            refresh: true,
            cellEdit: false,//禁用grid编辑功能
            exportexcel: false,
            footerrow: false
        }
    );
});

function initLoadData(Uid)
{
    if (!Uid)
        return;
    var param = "sopt_UId=eq&UId=" + Uid;
    $.ajax({
        async: true,
        url: rootPath + "GateManage/GetGateItem",
        type: 'POST',
        data: {
            UId: Uid,
            sidx: 'UId',
            'conditions': encodeURI(param)
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var maindata = result.main;
            console.log(maindata);
            setFieldValue(maindata);

            setdisabled(true);
            setToolBtnDisabled(true);
            var multiEdocData = [];
            ajaxHttp(rootPath + "DNManage/GetSMAndDnData", { SmNo: $("#ShipmentId").val(), loading: true },
            function (data) {
                if (data == null) {
                    MenuBarFuncArr.initEdoc($("#UId").val(), groupId, $("#Cmp").val(), "*");
                } else {
                    $(data.sm).each(function (index) {
                        multiEdocData.push({ jobNo: data.sm[index].UId, 'GROUP_ID': data.sm[index].GroupId, 'CMP': data.sm[index].Cmp, 'STN': '*' });
                    });
                    MenuBarFuncArr.initEdoc($("#UId").val(), groupId, $("#Cmp").val(), "*", multiEdocData);
                }
            });

            $("#SmGw").val(result.GW);
            $("#SmGwu").val(result.GWU);
            $("#SmCbm").val(result.CBM);
            $("#SmTcbm").val(result.TCBM);
            $("#ScuftGrid").jqGrid("setGridParam", {
                datatype: 'local',
                sortorder: "asc",
                sortname: "UId",
                data: result.Scuft,
            }).trigger("reloadGrid");

            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            CommonFunc.ToogleLoading(false);
            var status = $("#Status").val();

            chkMenu(status);
        }
    });
}

function ajaxHttp(url, data, successFn, completeFn, errorFn) {
    var loading = data.loading;
    if (loading == true) {
        CommonFunc.ToogleLoading(true);
    }
    $.ajax({
        async: true,
        url: url,
        type: 'POST',
        data: data,
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (loading == true) {
                CommonFunc.ToogleLoading(false);
            }
            if (completeFn) completeFn(xmlHttpRequest, successMsg);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 500, "danger");
            if (errorFn) errorFn();
        },
        success: function (result) {
            if (successFn) successFn(result);
        }
    });
}

function _initMenu()
{
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBInvalid", "MBSearch", "MBAdd", "MBDel", "MBCopy"]);

    MenuBarFuncArr.MBEdit = function(){
        _status = "edit";
        $("#ReserveHour").val(2);
        /*if($("#BatNo").val() == "")
        {
            var CreateCmp = $("#CreateCmp").val();
            if(CreateCmp == "")
            {
                CreateCmp = "FQ"
            }
            getAutoNo("BatNo", "rulecode=BAT_NO&cmp=" + CreateCmp);
        }*/
        if ($("#Twu").val() == "")
            $("#Twu").val("KGS");
    }

    MenuBarFuncArr.EndFunc = function(){
        var Status = $("#Status").val();

        if(Status != "D" && page == "R")
        {
            //$("#ReserveDate").prop("disabled", true);
           //$("#ReserveDate").siblings("span").find("button").prop("disabled", true);
            $("#ReserveFrom").prop("disabled", true);
            $("#ReserveHour").prop("disabled", true);
            $("#GateNo").prop("disabled", true);
            $("#GateNoLookup").prop("disabled", true);

            $("#Trucker").prop("disabled", true);
            $("#TruckerLookup").prop("disabled", true);
        }
        else if(page == "R")
        {
            $("#Trucker").prop("disabled", true);
            $("#TruckerLookup").prop("disabled", true);
        }
        else if(page == "C")
        {
            $("#WsCd").prop("disabled", false);
            $("#WsCd").prop("readonly", false);
            $("#WsCdLookup1").prop("disabled", false);
        }

        if(Status == 'I' || Status == 'G' || Status == 'V' || Status == 'P')
        {
            $("#TruckNo").prop("disabled", true);
            $("#TruckNo").prop("readonly", true);
            $("#TruckNoLookup").prop("disabled", true);
            $("#Driver").prop("disabled", true);
            $("#Driver").prop("readonly", true);
            $("#DriverLookup").prop("disabled", true);

            $("#LtruckNo").prop("disabled", true);
            $("#LtruckNo").prop("readonly", true);
            $("#LtruckNoLookup").prop("disabled", true);
            $("#Ldriver").prop("disabled", true);
            $("#Ldriver").prop("readonly", true);
            $("#LdriverLookup").prop("disabled", true);

            //$("#TareWeight").prop("disabled", true);
            //$("#TareWeight").prop("readonly", true);
            //$("#Twu").prop("disabled", true);
            //$("#Twu").prop("readonly", true);
            //$("#TwuLookup").prop("disabled", true);
        }
    }

    if ($("#Status").val())
    MenuBarFuncArr.MBCancel = function(){
        var Status = $("#Status").val();
        MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm", "MBModifyConfirm", "MBWHDelivery"]);
        chkMenu(Status);
        //MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm"]);
        _status = "";
    }

    if(page == "R")
    {
        MenuBarFuncArr.AddMenu("MBReserve", "glyphicon glyphicon-time", "@Resources.Locale.L_DNManage_Reserve", function(){
            var ReverseNo = $("#ReverseNo").val();
            var TruckNo = $("#TruckNo").val();
            var Driver = $("#Driver").val();
            var Tel = $("#Tel").val();
            var GateNo = $("#GateNo").val();
            var BatNo = $("#BatNo").val();
            if(TruckNo == "" || Driver == "" || Tel == "")
            {
                alert("@Resources.Locale.L_DNManage_TNoCell");
                return false;
            }

            if(GateNo == "")
            {
                alert("@Resources.Locale.L_DNManage_EnterDate");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "GateManage/ReverseGate",
                    type: 'POST',
                    data: {UId: $("#UId").val()},
                    dataType: "json",
                    beforeSend: function(){
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", errMsg, 1000, "danger");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        CommonFunc.ToogleLoading(false);
                        if(result.message == "success")
                        {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_ReserS", 1000, "success");
                            $("#Status").val("R");
                            $("#BatNo").val(result.BatNo)
                            //MenuBarFuncArr.Disabled(["MBReserve"]);
                        }
                        else
                        {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
            else
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NoReserNo", 1000, "warning");
            }
        });
    }
    else
    {
        MenuBarFuncArr.AddMenu("MBReserveConfirm", "glyphicon glyphicon-ok", "@Resources.Locale.L_DNManage_ReserveConf", function(){
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();

            if(Status != "R")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_ClickConfRwe", 1000, "warning");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "GateManage/ConfirmReverseGate",
                    type: 'POST',
                    data: {UId: $("#UId").val()},
                    dataType: "json",
                    beforeSend: function(){
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", errMsg, 1000, "danger");
                    },
                    success: function (result) {
                        CommonFunc.ToogleLoading(false);
                        if(result.message == "success")
                        {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SerConfS", 1000, "success");
                            $("#Status").val("C");
                            MenuBarFuncArr.Disabled(["MBReserveConfirm"]);
                            initLoadData(_uid);
                        }
                        else
                        {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
            else
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NoReserNo", 1000, "warning");
            }
        });

        MenuBarFuncArr.AddMenu("MBModifyConfirm", "glyphicon glyphicon-refresh", "@Resources.Locale.L_DNManage_EditConf", function(){
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();

            if(Status != "C")
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CannoteditCinf", 1000, "warning");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "GateManage/ModifyReverseGate",
                    type: 'POST',
                    data: {UId: $("#UId").val()},
                    dataType: "json",
                    beforeSend: function(){
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", errMsg, 1000, "danger");
                    },
                    success: function (result) {
                        CommonFunc.ToogleLoading(false);
                        if(result.message == "success")
                        {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_EdConfS", 1000, "success");
                            MenuBarFuncArr.Disabled(["MBReserveConfirm"]);
                            initLoadData(_uid);
                        }
                        else
                        {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
            else
            {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NoReserNo", 1000, "warning");
            }
        });

        MenuBarFuncArr.AddMenu("MBWHDelivery", "glyphicon glyphicon-ok", "@Resources.Locale.L_DNManage_EtW", function () {
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();
            switch (Status) {
                case "P":
                    alert("@Resources.Locale.L_GateReserveSetup_Script_157");
                    return false;
                    break;
                case "O":
                    alert("@Resources.Locale.L_GateReserveSetup_Script_158");
                    return false;
                    break;
                case "V":
                    alert("@Resources.Locale.L_GateReserveSetup_Script_159");
                    return false;
                    break;
                case "E":
                    alert("@Resources.Locale.L_GateReserveSetup_Script_160");
                    return false;
                    break;
            }
            //if (Status != "R") {
            //    CommonFunc.Notify("", "只有「已预约」状态才能点击外仓出货", 1000, "warning");
            //    return false;
            //}

            if (!confirm("@Resources.Locale.L_DNManage_SureEtW")) {
                return false;
            }

            if (ReverseNo != "") {

                $.ajax({
                    async: true,
                    url: rootPath + "GateManage/WHouseDelivery",
                    type: 'POST',
                    data: { UId: $("#UId").val() },
                    dataType: "json",
                    beforeSend: function () {
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", errMsg, 1000, "danger");
                    },
                    success: function (result) {
                        CommonFunc.ToogleLoading(false);
                        if (result.message == "success") {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_EtWS", 1000, "success");
                            initLoadData(_uid);
                        }
                        else {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
            else {
                CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NoReserNo", 1000, "warning");
            }
        });

    }
    

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
            MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm"]);
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }
        $.ajax({
            async: true,
            url: rootPath + "GateManage/SaveGateReseve",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val(), RelationId: $("#RelationId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF "+ result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                console.log(result.mainData);
                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm", "MBModifyConfirm", "MBWHDelivery"]);
                _status = "edit";
                chkMenu($("#Status").val());
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getChangeValue();
        //表示值沒變
        $.ajax({
            async: true,
            url: rootPath + "GateManage/SaveGateReseve",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;

                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);

    $("#TruckCntrno").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
        var chk = checkCtnNo(val);

        if(chk === false)
        {
            if(!confirm("@Resources.Locale.L_DNManage_WrongCntrNo"))
            {
                $(this).val("");
            }
        }

    });

    $("#TruckSealno").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
    });
}

function setBscData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.baseCondition = " GROUP_ID='"+groupId+"' AND CMP='"+cmp+"' AND CD_TYPE='"+pType+"'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.Cd);

        if(Nm != "")
            $("#" + Nm).val(map.CdDescp);
    }

    options.lookUpConfig = LookUpConfig.BSCodeLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.CD_DESCP);

        return false;
    });
}

function setSmptyData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%"+pType+"%'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.PartyNo);

        if(Nm != "")
            $("#" + Nm).val(map.PartyName);
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~"+pType+"&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);
        return false;
    });
}

function chkMenu(status)
{
    if(status == "O")
    {
        MenuBarFuncArr.Disabled(["MBReserve", "MBEdit", "MBReserveConfirm","MBModifyConfirm","MBWHDelivery"]);
    }
    if (status != "D") {
        MenuBarFuncArr.Disabled(["MBReserve"]);
    }
}

