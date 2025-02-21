
var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的

$(function () {
    schemas = JSON.parse(decodeHtml(schemas));
    CommonFunc.initField(schemas);
    setdisabled(true);

    _initMenu();
    initLoadData(_uid);

    if (pmsList.indexOf("InUploadBtn") == -1) {
        //按钮隐藏
        $("#UploadIn").hide();
        $("#UploadInName").hide();
    }
    if (pmsList.indexOf("OutUploadBtn") == -1) {
        $("#UploadOut").hide();
        $("#UploadOutName").hide();
    }
    if (pmsList.indexOf("PodUploadBtn") == -1) {
        $("#UploadPod").hide();
        $("#UploadPodName").hide();
    }

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
    options.gridFunc = function (map) {
        $("#SearchWsCd").val(map.WsCd);
    }

    options.lookUpConfig = LookUpConfig.SMWHLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#SearchWsCd", 1, "", "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=", "WS_CD=showValue,WS_CD,WS_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.WS_CD);
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
            CommonFunc.Notify("", _getLang("L_DNManage_CanUseGate", "此月台无法使用"), 1000, "warning");
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
            CommonFunc.Notify("", _getLang("L_DNManage_CanUseGate", "此月台无法使用"), 1000, "warning");
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

    //月台放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmwhgtForLookup";
    options.param = "";
    options.registerBtn = $("#TempGatenoLookup");
    options.focusItem = $("#TempGateno");
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var WsCd = $("#WsCd").val()

        return " WS_CD='"+WsCd+"'";
    }
    options.gridFunc = function (map) {
        var status = map.Status;

        if(status != "Y")
        {
            CommonFunc.Notify("", _getLang("L_DNManage_CanUseGate", "此月台无法使用"), 1000, "warning");
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
            CommonFunc.Notify("", _getLang("L_DNManage_CanUseGate", "此月台无法使用"), 1000, "warning");
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

    //setSmptyData("CmpLookup", "SearchCmp", "CmpNm", "LC");
    //QTYU unit Lookup
    setBscData("QtyuLookup", "Qtyu", "", "UB");

    //NWU unit Lookup
    setBscData("NwuLookup", "Nwu", "", "UT");

    //GWU unit Lookup
    setBscData("GwuLookup", "Gwu", "", "UT");

    //TWU unit Lookup
    setBscData("TwuLookup", "Twu", "", "UT");

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

    //CommonFunc.AutoComplete("#TruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
    //    $(this).val(ui.item.returnValue.TRUCK_NO);
        
    //    return false;
    //}, function(){
    //    var Trucker = $("#Trucker").val()

    //    return " PARTY_NO="+Trucker;
    //});

    //司機放大鏡
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
    //Driver  TRUCK_NO
    //CommonFunc.AutoComplete("#Driver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_ID,DRIVER_PHONE", function (event, ui) {
    //    $(this).val(ui.item.returnValue.DRIVER_NAME);
    //    $("#Tel").val(ui.item.returnValue.DRIVER_PHONE);
    //    $("#DriverId").val(ui.item.returnValue.DRIVER_ID);
    //    return false;
    //}, function(){
    //    var Trucker = $("#Trucker").val()

    //    return " PARTY_NO="+Trucker;
    //}
    //);

    //車號放大鏡
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

    //CommonFunc.AutoComplete("#LtruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
    //    $(this).val(ui.item.returnValue.TRUCK_NO);
        
    //    return false;
    //}, function(){
    //    var Trucker = $("#Trucker").val()

    //    return " PARTY_NO="+Trucker;
    //});

    //司機放大鏡
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

    //CommonFunc.AutoComplete("#Ldriver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_ID,DRIVER_PHONE", function (event, ui) {
    //    $(this).val(ui.item.returnValue.DRIVER_NAME);
    //    $("#Ltel").val(ui.item.returnValue.DRIVER_PHONE);
    //    $("#LdriverId").val(ui.item.returnValue.DRIVER_ID);
    //    return false;
    //}, function(){
    //    var Trucker = $("#Trucker").val()

    //    return " PARTY_NO="+Trucker;
    //}
    //, function () {
    //    $("#Ltel").val("");
    //    $("#LdriverId").val("");
    //}
    //);

    $("#CntrNo").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
        var chk = checkCtnNo(val);
        var TruckCntrno = $("#TruckCntrno").val();

        if(TruckCntrno != val)
        {
            alert(_getLang("L_DNManage_SlNoDiff", "提醒：预报货柜号与货柜号码不同"));
        }

        if(chk === false)
        {
            if(!confirm(_getLang("L_DNManage_WrongCntrNo", "您输入的货柜号码规则有误，请问要用此号码吗？")))
            {
                $(this).val("");
            }
        }

    });

    $("#SealNo1").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
        var TruckSealno = $("#TruckSealno").val();

        if(TruckSealno != val)
        {
            alert(_getLang("L_SmrvSetup_Script_163", "提醒：预报封条号与封条号码不同"));
        }
    });

    $("#SealNo2").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
    });

    $("#Gw").on("change", function(){
        calcuVgm();
    });

    $("#TareWeight").on("change", function(){
        calcuVgm();
    });
});

//Container Grid
$(function () {
    var colModel1 = [
            { name: 'Ouid', title: 'Ouid', index: 'Ouid', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'GroupId', title: 'GroupId', index: 'GroupId', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'Cmp', title: 'Cmp', index: 'Cmp', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'Stn', title: 'Stn', index: 'Stn', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'JobNo', title: 'Job No', index: 'JobNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'ReserveNo', title: 'ReserveNo', index: 'ReserveNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'OrdNo', title: 'OrdNo', index: 'OrdNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
            {
                name: 'PickupDate', title: 'Pickup Date', index: 'PickupDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                editoptions: myEditDateTimeInit,
                formatter: 'date',
                formatoptions: {
                    srcformat: 'ISO8601Long',
                    newformat: 'Y-m-d H:i:s',
                    defaultValue: ""
                }
            },
             {
                 name: 'ArrivalDate', title: 'Arrival Date', index: 'ArrivalDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                 editoptions: myEditDateTimeInit,
                 formatter: 'date',
                 formatoptions: {
                     srcformat: 'ISO8601Long',
                     newformat: 'Y-m-d H:i:s',
                     defaultValue: ""
                 }
             },
        { name: 'WsCd', title: 'Warehouse', index: 'WsCd', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatter: "select", editoptions: { value: WsCol }, edittype: 'select' },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CntrNo', title: 'Container No', index: 'CntrNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CntrType', title: 'Container Type', index: 'CntrType', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'DlvArea', title: '', index: 'DlvArea', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
        { name: 'DlvAreaNm', title: 'Delivery Area Name', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
        { name: 'DlvAddr', title: 'Delivery Address', index: 'DlvAddr', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'SealNo1', title: 'Seal No1', index: 'SealNo1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'SealNo2', title: 'Seal No2', index: 'SealNo2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Partof', title: 'Part of', index: 'Partof', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Ingate', title: 'Part of', index: 'Ingate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        {
            name: 'Qty', title: 'QTY', index: 'Qty', width: 70, align: 'right', sorttype: 'float', editable: false,
            formatter: 'number',
            formatoptions: {
                decimalSeparator: ".",
                thousandsSeparator: ",",
                decimalPlaces: 0,
                defaultValue: '0'
            }
        },
            {
                name: 'Gw', title: 'GW', index: 'Gw', width: 70, align: 'right', sorttype: 'float', editable: false,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 3,
                    defaultValue: '0.000'
                }
            },
            { name: 'Gwu', title: 'GW Unit', index: 'Gwu', width: 120, align: 'left', sorttype: 'string', hidden: false },
            {
                name: 'Cbm', title: 'CBM', index: 'Cbm', width: 70, align: 'right', sorttype: 'float', editable: false,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 3,
                    defaultValue: '0.000'
                }
            }, { name: 'Wo', title: 'WO', index: 'Wo', width: 70, align: 'left', sorttype: 'string', hidden: false }

    ];
    new genGrid(
        $("#FclMainGrid"),
        {
            datatype: "local",
            loadonce: true,
            colModel: colModel1,
            caption: "Container List",
            height: "auto",
            rows: 999999,
            refresh: false,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: false,
            pgbuttons: false,
            exportexcel: false,
            toppager: false,
            multiselect: false,
            multiboxonly: true,
            footerrow: false,
            sortname: "SubordNo",
            afterSaveCellFunc: function (rowid, name, val, iRow) {

            },
            loadComplete: function (data) {

            }
        }
    );
});

//Dn Grid
$(function () {
    var colModel1 = [
            { name: 'UId', title: 'UId', index: 'UId', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'Ouid', title: 'Ouid', index: 'Ouid', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'GroupId', title: 'GroupId', index: 'GroupId', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'Cmp', title: 'Cmp', index: 'Cmp', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'Stn', title: 'Stn', index: 'Stn', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'ReserveNo', title: 'ReserveNo', index: 'ReserveNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'OrdNo', title: 'OrdNo', index: 'OrdNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            {
                name: 'PickupDate', title: 'Pickup Date', index: 'PickupDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                editoptions: myEditDateTimeInit,
                formatter: 'date',
                formatoptions: {
                    srcformat: 'ISO8601Long',
                    newformat: 'Y-m-d H:i:s',
                    defaultValue: ""
                }
            },
             {
                 name: 'ArrivalDate', title: 'Arrival Date', index: 'ArrivalDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                 editoptions: myEditDateTimeInit,
                 formatter: 'date',
                 formatoptions: {
                     srcformat: 'ISO8601Long',
                     newformat: 'Y-m-d H:i:s',
                     defaultValue: ""
                 }
             },
        { name: 'WsCd', title: 'Warehouse', index: 'WsCd', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatter: "select", editoptions: { value: WsCol }, edittype: 'select' },
        { name: 'AddPoint', title: 'Add Point?', index: 'AddPoint', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false, formatter: "select", editoptions: { value: ':;Y:Yes;N:No' }, edittype: 'select' },
            { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'DnNo', title: 'DN No', index: 'DnNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'Goods', title: 'Commodity', index: 'Goods', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'DlvArea', title: '', index: 'DlvArea', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
            { name: 'DlvAreaNm', title: 'Delivery Area Name', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
            { name: 'DlvAddr', title: 'Delivery Address', index: 'DlvAddr', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            {
                name: 'Qty', title: 'QTY', index: 'Qty', width: 70, align: 'right', sorttype: 'float', editable: false,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 0,
                    defaultValue: '0'
                }
            },
            {
                name: 'Gw', title: 'GW', index: 'Gw', width: 70, align: 'right', sorttype: 'float', editable: false,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 3,
                    defaultValue: '0.000'
                }
            },
            { name: 'Gwu', title: 'GW Unit', index: 'Gwu', width: 120, align: 'left', sorttype: 'string', hidden: false },
            {
                name: 'Cbm', title: 'CBM', index: 'Cbm', width: 70, align: 'right', sorttype: 'float', editable: false,
                formatter: 'number',
                formatoptions: {
                    decimalSeparator: ".",
                    thousandsSeparator: ",",
                    decimalPlaces: 3,
                    defaultValue: '0.000'
                }
            }, { name: 'Wo', title: 'WO', index: 'Wo', width: 70, align: 'left', sorttype: 'string', hidden: false }
    ];
    new genGrid(
        $("#DnMainGrid"),
        {
            datatype: "local",
            loadonce: true,
            colModel: colModel1,
            caption: "Dn List",
            height: "auto",
            rows: 999999,
            refresh: false,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: false,
            pgbuttons: false,
            exportexcel: false,
            toppager: false,
            multiselect: false,
            multiboxonly: true,
            footerrow: false,
            sortname: "SubordNo",
            afterSaveCellFunc: function (rowid, name, val, iRow) {

            },
            loadComplete: function (data) {

            }
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
        url: rootPath + "IbGateManage/GetGateItem",
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
            //console.log(result);
            var maindata = result.main;
            //console.log(maindata);
            setFieldValue(maindata);
            if(result.InImg != "")
            {
                $("#inPic").attr("src", result.InImg);
                $("#inPicLink").attr("href", result.InImg);
            }
            else
            {
                $("#inPic").attr("src", noImage);
                $("#inPicLink").attr("href", noImage);
            }

            if(result.OutImg != "")
            {
                $("#outPic").attr("src", result.OutImg);
                $("#outPicLink").attr("href", result.OutImg);
            }
            else
            {
                $("#outPic").attr("src", noImage);
                $("#outPicLink").attr("href", noImage);
            }

            if (result.sub2) {
                $("#FclMainGrid").jqGrid("clearGridData");
                $("#FclMainGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.sub2
                }).trigger("reloadGrid");
            }

            if (result.sub1) {
                $("#DnMainGrid").jqGrid("clearGridData");
                $("#DnMainGrid").jqGrid("setGridParam", {
                    datatype: 'local',
                    data: result.sub1
                }).trigger("reloadGrid");
            }

            setdisabled(true);
            setToolBtnDisabled(true);

            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney", "btnSecurityRemark"]);
            CommonFunc.ToogleLoading(false);

            var multiEdocData = [];
            ajaxHttp(rootPath + "IbGateManage/GetSMAndDnData", { ShipmentInfo: $("#ShipmentInfo").val(), loading: true },
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


            var status = $("#Status").val();
            inputAction(status);
        }
    });
}

function _initMenu()
{
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBInvalid", "MBSearch", "MBAdd", "MBDel", "MBCopy"]);

    MenuBarFuncArr.MBEdit = function(){
        var Twu = $("#Twu").val();
        if(Twu == "")
        {
            $("#Twu").val("KGS");
        }
    }

    MenuBarFuncArr.EndFunc = function(){
        var status = $("#Status").val();
        inputAction(status);

    }

    MenuBarFuncArr.AddMenu("MBSeal", "glyphicon glyphicon-barcode", _getLang("L_DNManage_SlRl", "封柜/放行"), function(){
            var ReverseNo = $("#ReverseNo").val();
            var CntrNo = $("#CntrNo").val();
            var SealNo1 = $("#SealNo1").val();
            var TranType = $("#TranType").val();
            var Status = $("#Status").val();
            var PutDate = $("#PutDate").val();
            var TareWeight = $("#TareWeight").val();
            if (Status == "D" || Status == "R" || Status == "C" || Status == "A")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_CtSlIb", "尚未入厂，不能封柜"), 1000, "warning");
                return;
            }
            if(Status == "P")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_HasSlBf", "已封柜过！！"), 1000, "warning");
                return;
            }
            if(Status == "O")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_CtSlOut", "已离厂，不能封柜"), 1000, "warning");
                return;
            }
            if((TranType == "F" || TranType == "R") && (SealNo1 == "" || CntrNo == ""))
            {
                CommonFunc.Notify("", _getLang("L_DNManage_CCdSNo", "货柜号码，封条号码不能为空"), 1000, "warning");
                return;
            }

            if(TranType == "F" && TareWeight == "")
            {
                CommonFunc.Notify("", _getLang("L_SmrvSetup_Script_162", "FCL封柜时，必需输入『Tare Weight』"), 1000, "warning");
                return;
            }
                
            if(PutDate == "")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_CtSlNtLd", "没有装柜，无法封柜"), 1000, "warning");
                return;
            }

            $.ajax({
                async: true,
                url: rootPath + "Api/CheckQty",
                type: 'POST',
                data: { id: $("#UId").val() },
                dataType: "json",
                beforeSend: function () {
                    CommonFunc.ToogleLoading(true);
                },
                "complete": function (xmlHttpRequest, successMsg) {
                    CommonFunc.ToogleLoading(false);
                },
                "error": function (xmlHttpRequest, errMsg) {
                    CommonFunc.Notify("", errMsg, 1000, "danger");
                    CommonFunc.ToogleLoading(false);
                },
                success: function (result) {
                    CommonFunc.ToogleLoading(false);
                    if (result.flag === true) {
                        InFactoryConfirm();
                    }
                    else {
                        var truthBeTold = window.confirm(result.message + ","+_getLang("L_DNManage_SealorNot", "是否要封柜？"));
                        if (!truthBeTold) {
                            return;
                        }
                        InFactoryConfirm();
                    }
                }
            });

            function InFactoryConfirm() {
                var selRowId = $("#FclMainGrid").jqGrid('getGridParam', 'selrow');
                var wh = "";
                if (selRowId == null) {
                    selRowId = $("#DnMainGrid").jqGrid('getGridParam', 'selrow');
                    if (selRowId == null) {
                        alert("Please choose container or dn ");
                        return false;
                    }
                    wh = $("#DnMainGrid").jqGrid('getCell', selRowId, 'WsCd');
                }
                else {
                    wh = $("#FclMainGrid").jqGrid('getCell', selRowId, 'WsCd');
                }

                $.ajax({
                    async: true,
                    url: rootPath + "Api/InFactoryConfirm",
                    type: 'POST',
                    data: { id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val(), wh: wh },
                    dataType: "json",
                    beforeSend: function () {
                        CommonFunc.ToogleLoading(true);
                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        CommonFunc.Notify("", errMsg, 1000, "danger");
                        CommonFunc.ToogleLoading(false);
                    },
                    success: function (result) {
                        CommonFunc.ToogleLoading(false);
                        if (result.message == _getLang("L_ApiController_Controllers_28", "封柜成功！")) {
                            CommonFunc.Notify("", _getLang("L_DNManage_SealS", "封柜成功"), 1000, "success");
                            initLoadData(_uid);
                        }
                        else {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
    });

    MenuBarFuncArr.AddMenu("btnSecurityRemark", "glyphicon glyphicon-bell", _getLang("TLB_SecurityRemarkEdit", "Security Remark Modify"), function () {
        SecurityRemarkEdit();
    });

    function SecurityRemarkEdit() {
        $("#ScRemark").attr('disabled', false);
        MenuBarFuncArr.DisableAllItem();
        MenuBarFuncArr.Enabled(["MBCancel", "MBSave"]);
    }

        MenuBarFuncArr.MBCancel = function(){
            //MenuBarFuncArr.Disabled(["MBReload"]);
            MenuBarFuncArr.Enabled(["MBSeal", "btnSecurityRemark"]);
        };
    

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }
        if (contrast($("#PutDate").val(), $("#SealDate").val())) {
            alert(_getLang("L_BSCSSetup_SFail", "保存失败") + ":" + _getLang("L_PutTime_Tip", "装柜时间大于当前封柜时间"));
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/SaveGateReseve",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Enabled(["MBSeal", "MBEdoc", "btnSecurityRemark"]);
                dtd.resolve();
            }
        });
        return dtd.promise();
    }
    //addInOutMenu();
    initMenuBar(MenuBarFuncArr);


    $('#MBEdit').unbind("click");
    $("#MBEdit").click(function () {//自定义编辑
        var className = $(this).attr('class');
        if (className == "nav-disabled") {
            return false;
        }
        _editData = _dataSource[0];
        var result = MenuBarFuncArr.MBEdit();
        if (result === false) return;
        MenuBarFuncArr.EditStatus(result);
        var updatengc = ["ScRemark"];
        var status = $("#Status").val();
        if (status === 'O')
            updatengc = ["TruckNo", "Driver", "DriverId", "LtruckNo", "Ldriver", "LdriverId", "Ltel", "Tel", "ScRemark"];
        for (var i = 0; i < updatengc.length; i++) {
            $("#" + updatengc[i]).attr('disabled', true);
            $("#" + updatengc[i]).parent().find("button").attr("disabled", true);
        }
        $("#wrapper").focusFirst();

    });
}

function setBscData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
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

function calcuVgm()
{
    var Gw = isNaN(parseFloat($("#Gw").val()))?0:parseFloat($("#Gw").val());
    var Tw = isNaN(parseFloat($("#TareWeight").val()))?0:parseFloat($("#TareWeight").val());
    var TtlVgm = 0;

    TtlVgm = Gw + Tw;

    $("#TtlVgm").val(CommonFunc.formatFloat(TtlVgm, 3));
    return false;
}

function InConfirm(mode) {
    var msg = _getLang("L_DNManage_EmtCar", "空车");
    if (mode === 0) msg = _getLang("L_DNManage_NoEmtCar", "非空车");
    var iscontinue = window.confirm(_getLang("L_DNManage_Whether5", "是否要进行")+msg+_getLang("L_DNManage_InboundConf", "入厂确认？"));
    if (!iscontinue) {
        return;
    }

    var ReverseNo = $("#ReverseNo").val();
    var CntrNo = $("#CntrNo").val();
    var SealNo1 = $("#SealNo1").val();
    var TranType = $("#TranType").val();
    var Status = $("#Status").val();

    if (Status == "P") {
        CommonFunc.Notify("", _getLang("L_DNManage_HasSeal", "已封柜！！"), 1000, "warning");
        return;
    }
    if (Status == "O") {
        CommonFunc.Notify("", _getLang("L_DNManage_HasOut", "已离厂"), 1000, "warning");
        return;
    }
    if (Status !== "R" && Status !== "C" && Status !== "E" && Status !== "A") {
        CommonFunc.Notify("", _getLang("L_DNManage_HasIn", "已入厂"), 1000, "warning");
        return;
    }

    var selRowId = $("#FclMainGrid").jqGrid('getGridParam', 'selrow');
    var wh = "";
    //if (selRowId == null) {
    //    selRowId = $("#DnMainGrid").jqGrid('getGridParam', 'selrow');
    //    if (selRowId == null) {
    //        alert("Please choose container or dn ");
    //        return false;
    //    }
    //    wh = $("#DnMainGrid").jqGrid('getCell', selRowId, 'WsCd');
    //}
    //else {
    //    wh = $("#FclMainGrid").jqGrid('getCell', selRowId, 'WsCd');
    //}

    $.ajax({
        async: true,
        url: rootPath + "Api/RFactoryConfirm",
        type: 'POST',
        data: { mode: mode, id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val(), wh: wh },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 1000, "danger");
            CommonFunc.ToogleLoading(false);
        },
        success: function (result) {
            CommonFunc.ToogleLoading(false);
            if (result.flag ===true) {
                CommonFunc.Notify("", result.message, 1000, "success");
                initLoadData(_uid);
            }
            else {
                CommonFunc.Notify("", result.message, 1000, "warning");
            }
        }
    });
}

function OutConfirm(mode) {
    var msg = _getLang("L_DNManage_EmtCar", "空车");
    if (mode === 0) msg = _getLang("L_DNManage_NoEmtCar", "非空车");
    var iscontinue = window.confirm(_getLang("L_DNManage_Whether5", "是否要进行") + msg + _getLang("L_SmrvSetup_Script_165", "出厂确认？"));
    if (!iscontinue) {
        return;
    }
  
    var ReverseNo = $("#ReverseNo").val();
    var CntrNo = $("#CntrNo").val();
    var SealNo1 = $("#SealNo1").val();
    var TranType = $("#TranType").val();
    var Status = $("#Status").val();
    var PutDate = $("#PutDate").val();
    var SealDate = $("#SealDate").val();

    if (Status == "O") {
        CommonFunc.Notify("", _getLang("L_DNManage_HasOut", "已离厂"), 1000, "warning");
        return;
    }
    if (Status === "R" || Status === "C" || Status === "E") {
        CommonFunc.Notify("", _getLang("L_DNManage_NotIn", "尚未入厂"), 1000, "warning");
        return;
    }

    var check_SealNo = $("#SealNo1").val();
    if (mode == 0) {
        check_SealNo = prompt(_getLang("L_DNManage_EntSlNo", "如有封条号请输入封条号，谢谢合作"), "");
        check_SealNo = check_SealNo || "";
    }

    if(mode == 0)
    {
        if(PutDate == "" && SealDate == "")
        {
            CommonFunc.Notify("", _getLang("L_DNManage_CantOut", "没有装柜、封柜时间无法离厂"), 1000, "warning");
            return;
        }
    }

    var selRowId = $("#FclMainGrid").jqGrid('getGridParam', 'selrow');
    var wh = "";
    //if (selRowId == null) {
    //    selRowId = $("#DnMainGrid").jqGrid('getGridParam', 'selrow');
    //    if (selRowId == null) {
    //        alert("Please choose container or dn ");
    //        return false;
    //    }
    //    wh = $("#DnMainGrid").jqGrid('getCell', selRowId, 'WsCd');
    //}
    //else {
    //    wh = $("#FclMainGrid").jqGrid('getCell', selRowId, 'WsCd');
    //}

    $.ajax({
        async: true,
        url: rootPath + "Api/OutFactoryConfirm",
        type: 'POST',
        data: { mode: mode, id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val(), check_SealNo: encodeURIComponent(check_SealNo), wh: wh },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "complete": function (xmlHttpRequest, successMsg) {
            CommonFunc.ToogleLoading(false);
        },
        "error": function (xmlHttpRequest, errMsg) {
            CommonFunc.Notify("", errMsg, 1000, "danger");
            CommonFunc.ToogleLoading(false);
        },
        success: function (result) {
            CommonFunc.ToogleLoading(false);
            if (result.flag === true) {
                CommonFunc.Notify("", result.message, 1000, "success");
                initLoadData(_uid);
            }
            else {
                CommonFunc.Notify("", result.message, 1000, "warning");
            }
        }
    });
}

function addInOutMenu() {
    if (_flag !== "0" && _flag !== "1")
        return;
    if (_flag === "0") {
        MenuBarFuncArr.AddMenu("MBIn0", "glyphicon glyphicon-barcode", _getLang("L_DNManage_InConfNtEmp", "进厂确认（非空车）"), function () {
            InConfirm(0);
        });

        MenuBarFuncArr.AddMenu("MBIn1", "glyphicon glyphicon-barcode", _getLang("L_DNManage_InCinfEmp", "进厂确认（空车）"), function () {
            InConfirm(1);
        });
    }
    if (_flag === "1") {
        MenuBarFuncArr.AddMenu("MBOut0", "glyphicon glyphicon-barcode", _getLang("L_DNManage_OutConfNtEmp", "出厂确认（非空车）"), function () {
            OutConfirm(0);
        });

        MenuBarFuncArr.AddMenu("MBOut1", "glyphicon glyphicon-barcode", _getLang("L_DNManage_OutConfEmp", "出厂确认（空车）"), function () {
            OutConfirm(1);
        });
    }
}

function inputAction(status)
{
    //if(status == "P")
    //{
    //    $("input[dt='mt']").each(function(index, el) {
    //        var fieldname = $(this).attr("fieldname");
    //        if(fieldname != "Ldriver" && fieldname != "LtruckNo" && fieldname != "Ltel")
    //        {
    //            $("#" + fieldname + "Lookup").prop("disabled", true);
    //            $(this).prop("disabled", true);
    //        }
    //    });

    //    $("button.ui-datepicker-trigger").each(function(index, el) {
    //        $(this).prop("disabled", true);
    //    });
    //}
    //else
    if (status == "O" || status == "V")
    {
        MenuBarFuncArr.Disabled(["MBSeal"]);
    }
    //if (status == "D" || status == "R" || status == "C")
    //{
    //    MenuBarFuncArr.Enabled(["MBEdit"]);
    //}
    //else
    //{
    //    MenuBarFuncArr.Disabled(["MBEdit"]);
    //}

    var TranType = $("#Smtype").val();
    if (!(TranType == "F" || TranType == "R")) {
        $("#FclMainGriddiv").css("display", "none");
    }
}

function contrast(comparie, etd) {
    if (isEmpty(comparie)) return false;
    if (isEmpty(etd)) return false;
    comparie = comparie.replace(new RegExp("/", "gm"), "-");
    etd = etd.replace(new RegExp("/", "gm"), "-");
    //etd = etd.replace("/", "-");
    if (comparie < etd) return false;
    var beginTimes = comparie.substring(0, 10).split('-');
    var endTimes = etd.substring(0, 10).split('-');

    var starttime = new Date(beginTimes[0], beginTimes[1], beginTimes[2]);
    var starttimes = starttime.getTime();

    var lktime = new Date(endTimes[0], endTimes[1], endTimes[2]);
    var lktimes = lktime.getTime();

    if (starttimes >= lktimes) {
        return true;
    }
    else
        return false;
}

function isEmpty(val) {
    if (val === undefined || val === "" || val == null)
        return true;
    return false;
}

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
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
