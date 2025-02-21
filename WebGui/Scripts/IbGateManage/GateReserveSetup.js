var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var TranType = "";
var CallType = "";
var _dm = new dm();

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

    $('#ChangeReserveNo').change(function () {
        var ouid = $(this).val();

        initLoadData(ouid);
    });

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
    }

    options.lookUpConfig = LookUpConfig.SMWHLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#SearchWsCd", 1, "", "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=", "WS_CD=showValue,WS_CD,WS_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.WS_CD);
        return false;
    }, function () { "CMP=" + $("#SearchCmp").val();});

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

    

    var options = {};
    options.gridUrl = rootPath + LookUpConfig.TruckPortCdUrl;
    options.param = "";
    options.registerBtn = $("#BackLocationLookup");
    options.focusItem = $("#BackLocation");
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#BackLocation").val(map.PortCd);
    }

    options.lookUpConfig = LookUpConfig.TruckPortCdLookup;
    initLookUp(options);

    $("#BackLocation").v3AutoComplete({
        params: "dt=bstport&GROUP_ID=" + groupId + "&PORT_CD%",
        returnValue: "REGION&STATE&PORT_NM&PORT_CD=showValue,PORT_CD,PORT_NM,STATE,REGION",
        callBack: function (event, ui) {
            $("#BackLocation").val(ui.item.returnValue.PORT_CD);
            return false;
        },
        clearFunc: function () {
        }
    });

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

    $("#TruckNo").on("change", function(){
        $("#LtruckNo").val($(this).val());
    });

    $("#Driver").on("change", function(){
        $("#Ldriver").val($(this).val());
    });

    $("#DriverId").on("change", function(){
        $("#LdriverId").val($(this).val());
    });

    $("#Tel").on("change", function(){
        $("#Ltel").val($(this).val());
    });

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
        $("#LtruckNo").val(map.TruckNo);
    }

    options.lookUpConfig = LookUpConfig.BstruckcLookup;
    initLookUp(options);

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
        $("#Ldriver").val(map.DriverName);
        $("#Ltel").val(map.DriverPhone);
        $("#LdriverId").val(map.DriverId);
    }

    options.lookUpConfig = LookUpConfig.BstruckdLookup;
    initLookUp(options);

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
            var WsCd = $("#TempWscd").val()

            return " WS_CD="+WsCd;
        });
    }

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
});


//Container Grid
$(function(){
    var colModel1 = [
            { name: 'UId', title: 'UId', index: 'UId', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'ReserveNo', title: 'ReserveNo', index: 'ReserveNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'OrdNo', title: 'OrdNo', index: 'OrdNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
           // { name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', width: 120, align: 'left', sorttype: 'string', hidden: true, editable: false },
            { name: 'PickupDate', title: 'Pickup Date', index: 'PickupDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                     editoptions: myEditDateTimeInit,
                     formatter: 'date',
                     formatoptions: {
                         srcformat: 'ISO8601Long',
                         newformat: 'Y-m-d H:i:s',
                         defaultValue: ""
                     }
             },
             { name: 'ArrivalDate', title: 'Arrival Date', index: 'ArrivalDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                     editoptions: myEditDateTimeInit,
                     formatter: 'date',
                     formatoptions: {
                         srcformat: 'ISO8601Long',
                         newformat: 'Y-m-d H:i:s',
                         defaultValue: ""
                     }
              },
              { name: 'DischargeDate', title: 'Discharge Date', index: 'DischargeDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: true,
                      editoptions: myEditDateInit,
                      formatter: 'date',
                      formatoptions: {
                          srcformat: 'ISO8601Long',
                          newformat: 'Y-m-d',
                          defaultValue: ""
                      }
               },
               { name: 'RelDate', title: 'Release Date', index: 'RelDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'datetime', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: true,
                      editoptions: myEditDateTimeInit,
                      formatter: 'date',
                      formatoptions: {
                          srcformat: 'ISO8601Long',
                          newformat: 'Y-m-d H:i:s',
                          defaultValue: ""
                      }
               },
        { name: 'WsCd', title: 'Warehouse', index: 'WsCd', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false,  formatter: "select", editoptions: { value: WsCol }, edittype: 'select' },
        { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CntrNo', title: 'Container No', index: 'CntrNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'CntrType', title: 'Container Type', index: 'CntrType', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'DlvArea', title: 'Delivery Area ID', index: 'DlvArea', width: 120, align: 'left', sorttype: 'string', hidden: false, editable:false },
        { name: 'DlvAreaNm', title: 'Delivery Area Name', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string',   hidden: false, editable:false },
        { name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left',  sorttype: 'string',  hidden: true, editable:false },
        { name: 'DlvAddr', title: 'Delivery Address', index: 'DlvAddr', width: 120, align: 'left',  sorttype: 'string',  hidden: false, editable:false },
        { name: 'SealNo1', title: 'Seal No1', index: 'SealNo1', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'SealNo2', title: 'Seal No2', index: 'SealNo2', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        //{ name: 'Partof', title: 'Part of', index: 'Partof', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        //{ name: 'Ingate', title: 'Part of', index: 'Ingate', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
        { name: 'Qty', title: 'QTY', index: 'Qty', width: 70, align:'right', sorttype: 'float', editable: false,
            formatter: 'number', 
            formatoptions: { 
                decimalSeparator: ".", 
                thousandsSeparator: ",", 
                decimalPlaces: 0, 
                defaultValue: '0'
            }
        },
            { name: 'Gw', title: 'GW', index: 'Gw', width: 70, align:'right', sorttype: 'float', editable: false,
                formatter: 'number', 
                formatoptions: { 
                    decimalSeparator: ".", 
                    thousandsSeparator: ",", 
                    decimalPlaces: 3, 
                    defaultValue: '0.000'
                }
            },
            { name: 'Gwu', title: 'GW Unit', index: 'Gwu', width: 120, align: 'left', sorttype: 'string', hidden: false },
            { name: 'Cbm', title: 'CBM', index: 'Cbm', width: 70, align:'right', sorttype: 'float', editable: false,
                formatter: 'number', 
                formatoptions: { 
                    decimalSeparator: ".", 
                    thousandsSeparator: ",", 
                    decimalPlaces: 3, 
                    defaultValue: '0.000'
                }
            },{
                name: 'EmptyTime', title: 'Empty Return Time', index: 'EmptyTime', sorttype: 'string', width: 110, hidden: false, editable: true,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i' }, hidden: false, editable: true,
                editoptions: myEditDateTimeInit,
                formatter: 'date',
                formatoptions: {
                    srcformat: 'ISO8601Long',
                    newformat: 'Y-m-d H:i',
                    defaultValue: ""
                }
            }
     ];

     _dm.addDs("FclMainGrid", [], ["UId"], $("#FclMainGrid")[0]);
    new genGrid(
        $("#FclMainGrid"),
        {
            datatype: "local",
            loadonce:true,
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
            toppager:false,
            multiselect: false,
            multiboxonly: true,
            footerrow: false,
            sortname: "SubordNo",
            delKey: ["UId"],
            afterSaveCellFunc: function(rowid, name, val, iRow)
            {
                
            },
            loadComplete: function(data)
            {

            }
        }
    );
});

//Dn Grid
$(function(){
    var colModel1 = [
            { name: 'UId', title: 'UId', index: 'UId', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'ReserveNo', title: 'ReserveNo', index: 'ReserveNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'OrdNo', title: 'OrdNo', index: 'OrdNo', width: 120, align: 'left', sorttype: 'string', hidden: true },
            { name: 'PickupDate', title: 'Pickup Date', index: 'PickupDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d' }, hidden: false, editable: false,
                     editoptions: myEditDateTimeInit,
                     formatter: 'date',
                     formatoptions: {
                         srcformat: 'ISO8601Long',
                         newformat: 'Y-m-d H:i:s',
                         defaultValue: ""
                     }
             },
             { name: 'ArrivalDate', title: 'Arrival Date', index: 'ArrivalDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: false,
                     editoptions: myEditDateInit,
                     formatter: 'date',
                     formatoptions: {
                         srcformat: 'ISO8601Long',
                         newformat: 'Y-m-d H:i:s',
                         defaultValue: ""
                     }
              },
              { name: 'DischargeDate', title: 'Discharge Date', index: 'DischargeDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'date', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: true,
                      editoptions: myEditDateTimeInit,
                      formatter: 'date',
                      formatoptions: {
                          srcformat: 'ISO8601Long',
                          newformat: 'Y-m-d',
                          defaultValue: ""
                      }
               },
               { name: 'RelDate', title: 'Release Date', index: 'RelDate', sorttype: 'string', width: 110, hidden: false, editable: false,
                 formatter: 'datetime', formatoptions: { newformat: 'Y-m-d H:i:s' }, hidden: false, editable: true,
                      editoptions: myEditDateTimeInit,
                      formatter: 'date',
                      formatoptions: {
                          srcformat: 'ISO8601Long',
                          newformat: 'Y-m-d H:i:s',
                          defaultValue: ""
                      }
               },
        { name: 'WsCd', title: 'Warehouse', index: 'WsCd', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false,  formatter: "select", editoptions: { value: WsCol }, edittype: 'select' },
        { name: 'AddPoint', title: 'Add Point?', index: 'AddPoint', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false,  formatter: "select", editoptions: { value: ':;Y:Yes;N:No' }, edittype: 'select' },
            { name: 'ShipmentId', title: 'Shipment ID', index: 'ShipmentId', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'DnNo', title: 'DN No', index: 'DnNo', width: 120, align: 'left', sorttype: 'string', hidden: false, editable: false },
            { name: 'DlvArea', title: 'Delivery Area ID', index: 'DlvArea', width: 120, align: 'left',  sorttype: 'string',  hidden: false, editable:false },
            { name: 'DlvAreaNm', title: 'Delivery Area Name', index: 'DlvAreaNm', width: 120, align: 'left', sorttype: 'string',   hidden: false, editable:false },
            { name: 'AddrCode', title: '', index: 'AddrCode', width: 120, align: 'left',  sorttype: 'string',  hidden: true, editable:false },
            { name: 'DlvAddr', title: 'Delivery Address', index: 'DlvAddr', width: 120, align: 'left',  sorttype: 'string',  hidden: false, editable:false },
            { name: 'Qty', title: 'QTY', index: 'Qty', width: 70, align:'right', sorttype: 'float', editable: false,
                formatter: 'number', 
                formatoptions: { 
                    decimalSeparator: ".", 
                    thousandsSeparator: ",", 
                    decimalPlaces: 0, 
                    defaultValue: '0'
                }
            },
            { name: 'Gw', title: 'GW', index: 'Gw', width: 70, align:'right', sorttype: 'float', editable: false,
                formatter: 'number', 
                formatoptions: { 
                    decimalSeparator: ".", 
                    thousandsSeparator: ",", 
                    decimalPlaces: 3, 
                    defaultValue: '0.000'
                }
            },
            { name: 'Gwu', title: 'GW Unit', index: 'Gwu', width: 120, align: 'left', sorttype: 'string', hidden: false },
            { name: 'Cbm', title: 'CBM', index: 'Cbm', width: 70, align:'right', sorttype: 'float', editable: false,
                formatter: 'number', 
                formatoptions: { 
                    decimalSeparator: ".", 
                    thousandsSeparator: ",", 
                    decimalPlaces: 3, 
                    defaultValue: '0.000'
                }
            }
     ];
     _dm.addDs("DnMainGrid", [], ["UId"], $("#DnMainGrid")[0]);
    new genGrid(
        $("#DnMainGrid"),
        {
            datatype: "local",
            loadonce:true,
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
            toppager:false,
            multiselect: false,
            multiboxonly: true,
            footerrow: false,
            sortname: "SubordNo",
            delKey: ["UId"],
            afterSaveCellFunc: function(rowid, name, val, iRow)
            {
                
            },
            loadComplete: function(data)
            {

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
            var maindata = result.main;
            console.log(result);
            setFieldValue(maindata);

            if(result.sub2)
            {
                if (_dm.getDs("FclMainGrid") == null || _dm.getDs("FclMainGrid") == undefined) {
                    _dm.addDs("FclMainGrid", result.sub2, ["UId"], $grid[0]);
                } else {
                    _dm.getDs("FclMainGrid").setData(result.sub2);
                }

            }

            if(result.sub1)
            {
                if (_dm.getDs("DnMainGrid") == null || _dm.getDs("DnMainGrid") == undefined) {
                    _dm.addDs("DnMainGrid", result.sub1, ["UId"], $grid[0]);
                } else {
                    _dm.getDs("DnMainGrid").setData(result.sub1);
                }
            }

            setdisabled(true);
            setToolBtnDisabled(true);
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

            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            CommonFunc.ToogleLoading(false);
            var status = $("#Status").val();

            if(maindata[0])
            {
                TranType = maindata[0]["TranType"];
                CallType = maindata[0]["CallType"];
                $("#SearchCmp").val(maindata[0]["Cmp"]);
                $("#SearchWsCd").val(maindata[0]["WsCd"]);
                $("#SearchRDate").val(maindata[0]["UseDate"]);
            }
            

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
        $("#ChangeReserveNo").prop("disabled", true);
        if (CallType != "C")
        {
            $("#CarType").removeAttr("readonly");
            $("#TrsMode").removeAttr("readonly");
        }
        if ($("#Twu").val() == "")
            $("#Twu").val("KGS");

        gridEditableCtrl({ editable: true, gridId: "FclMainGrid" });
        gridEditableCtrl({ editable: true, gridId: "DnMainGrid" });
    }

    MenuBarFuncArr.EndFunc = function(){
        var Status = $("#Status").val();
        var reservedate = $("#ReserveDate").val();
        if (reservedate != "" && reservedate != undefined && reservedate != null) {
            $("#ReserveDate").prop("disabled", true);
            $("#ReserveDate").siblings("span").find("button").prop("disabled", true);
            $("#ReserveFrom").prop("disabled", true);
        }
        
        if(Status != "D" && page == "R")
        {
            $("#ReserveDate").prop("disabled", true);
            $("#ReserveDate").siblings("span").find("button").prop("disabled", true);
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
        } else if ("C" == page)
        {
            $("#WsCd").prop("disabled", false);
            $("#WsCd").prop("readonly", false);
        }
        $("#TruckRmk").prop("disabled", true);
        $("#WsRmk").prop("disabled", true);
        if (_ioflag == "I" && (Status == "D" || Status == "R" || Status == "C")) {
            $("#ReserveDate").prop("disabled", false);
            $("#ReserveFrom").prop("disabled", false);
            $("#ReserveDate").parent().find("button").attr("disabled", false);
            if ("C" == page) {
                $("#WsRmk").prop("disabled", false);
                $("#WsRmk").removeAttr("readonly");
            }
            $("#TruckRmk").removeAttr("readonly");
            $("#TruckRmk").prop("disabled", false);
        } else {
            $("#ReserveDate").prop("disabled", true);
            $("#ReserveFrom").prop("disabled", true);
            $("#ReserveDate").parent().find("button").attr("disabled", true);
            $("#ReserveHour").prop("disabled", true);
            $("#TruckRmk").prop("disabled", true);
            $("#WsRmk").prop("disabled", true);
        }
        if (Status == 'I' || Status == 'G' || Status == 'V' || Status == 'P') {
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

            $("#HeavyPickupTime").prop("disabled", true);
            $("#HeavyPickupTime").parent().find("button").attr("disabled", true);
            $("#EmptyReturnTime").prop("disabled", true);
            $("#EmptyReturnTime").parent().find("button").attr("disabled", true);
            $("#AtYardTime").prop("disabled", true);
            $("#AtYardTime").parent().find("button").attr("disabled", true);
        }

        if(TranType == "F")
        {
            $("#TrsMode").prop("disabled", true);
        }
        if (_ioflag == "O") {
            $("#TruckRmk").removeAttr("readonly");
            $("#TruckRmk").prop("disabled", false);
        }
        $("#NewSeal").prop("disabled", true);
        $("#WsCd").prop("disabled", true);
        $("#CntrNo").prop("disabled", true);
        $("#SealNo1").prop("disabled", true);
        $("#ArrivalDate").prop("disabled", true);
        $("#ArrivalDate").siblings("span").find("button").prop("disabled", true);
        $("#NewCntrno").prop("disabled", true);
        $("#TareWeight").prop("disabled", true);
        $("#Twu").prop("disabled", true);
        $("#TrainFlight").prop("disabled", true);
        $("#PalletQty").prop("disabled", true);
        $("#TwuLookup").prop("disabled", true);
        $("#WsCdLookup1").prop("disabled", true);
        $("#ReserveHour").prop("disabled", true);
        if ("O" == Status && "C" == page)
        {
            $("#BatchId").prop("disabled", true);
            $("#BackLocation").prop("disabled", true);
            $("#BackLocationLookup").prop("disabled", true);
            $("#LdriverLookup").prop("disabled", true);
            $("#Ldriver").prop("disabled", true);
            $("#LtruckNoLookup").prop("disabled", true);
            $("#LtruckNo").prop("disabled", true);
            $("#TruckNo").prop("disabled", true);
            $("#TruckNoLookup").prop("disabled", true);
            $("#Driver").prop("disabled", true);
            $("#DriverLookup").prop("disabled", true);
            $("#TrsMode").prop("disabled", true);
            $("#CarType").prop("disabled", true);
            $("#UseDate").prop("disabled", true);
            $("#Trucker").prop("disabled", true);
            $("#TruckerLookup").prop("disabled", true);
            $("#UseDate").siblings("span").find("button").prop("disabled", true);
            $("#ReserveDate").siblings("span").find("button").prop("disabled", true);

            $("#HeavyPickupTime").prop("disabled", true);
            $("#HeavyPickupTime").parent().find("button").attr("disabled", true);
            $("#EmptyReturnTime").prop("disabled", true);
            $("#EmptyReturnTime").parent().find("button").attr("disabled", true);
            $("#AtYardTime").prop("disabled", true);
            $("#AtYardTime").parent().find("button").attr("disabled", true);

        }
        if ("C" == page) {
            $("#WsRmk").prop("disabled", false);
            $("#WsRmk").removeAttr("readonly");
        }
    }

    //注册按钮型的放大镜  op=》{url:"",config:"",param:""} op基本格式 item是jq对象  url是api  config是colom数组 param防止带入底层的siti那个乱七八糟的东西
    function registBtnLookup(item, op, op1, op2) {
        //{url:"",config:""}
        var options = {};
        options.gridUrl = op.url;
        options.registerBtn = item;
        options.param = "";
        if (op.isMutiSel) options.isMutiSel = true;
        options.param = op.param;
        options.gridFunc = function (map) {
            //获取Map中的值，然后将值传递给后台重新去抓取整个画面的url
            if (op.selectRowFn)
                op.selectRowFn(map);
        }
        options.responseMethod = function () { }
        options.lookUpConfig = op.config;
        if (op1)
            options = $.extend(options, op1);
        initLookUp(options);

        if (op2 && op.item) {
            CommonFunc.AutoComplete(op.item, 1, "", op2.autoCompDt, op2.autoCompParams,
                function (event, ui) {
                    if (op2.autoCompFunc) {
                        op2.autoCompFunc($(op.item), event, ui);
                    }
                    return false;
                }, function () {
                    if (op2.dymcFunc) {
                        return op2.dymcFunc();
                    }
                    return "";
                }, function () {
                    if (op2.autoClearFunc) {
                        op2.autoClearFunc($(op.item));
                    }
                    return false;
                });
        }
    }


    if ($("#Status").val())
    MenuBarFuncArr.MBCancel = function(){
        var Status = $("#Status").val();
        MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm", "MBModifyConfirm", "MBWHDelivery","MBAddConatiner"]);
        chkMenu(Status);
        _status = "";
    }

    if(page == "R")
    {
        MenuBarFuncArr.AddMenu("MBReserve", "glyphicon glyphicon-time", _getLang("L_DNManage_Reserve", "预约"), function(){
            var ReverseNo = $("#ReverseNo").val();
            var TruckNo = $("#TruckNo").val();
            var Driver = $("#Driver").val();
            var Tel = $("#Tel").val();
            var GateNo = $("#GateNo").val();
            var BatNo = $("#BatNo").val();

            var usedate = $("#UseDate").val();
            if (usedate === undefined || usedate === "" || usedate == null) {
                alert("Pick Up Date is Null!");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "IbGateManage/ReverseGate",
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
                            CommonFunc.Notify("", _getLang("L_DNManage_ReserS", "预约成功"), 1000, "success");
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
                CommonFunc.Notify("", _getLang("L_DNManage_NoReserNo", "预约号码不存在"), 1000, "warning");
            }
        });
        
        var SmrcntrLookup = {
            caption: "DnNo",
            sortname: "DnNo",
            refresh: false,
            columns: [{ name: "JobNo", title: "Job No", width: 120, sorttype: "string", hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: "ReserveNo", title: "Reserve No", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: "OrdNo", title: "Ordder No", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: "SeqNo", title: "Seq No", width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: "PortDescp", title: _getLang("L_DNDetailView_Scripts_206", "Name of Port of Discharge"), width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Cnt20', title: _getLang("L_SMORD_Cnt20", "20GP"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Cnt40', title: _getLang("L_SMORD_Cnt40", "40GP"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Cnt40hq', title: _getLang("L_SMORD_Cnt40hq", "40HQ"), width: 80, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'CntType', title: _getLang("L_DNApproveManage_CntrType", "Container Type"), width: 90, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'CntNumber', title: _getLang("L_DNFlowManage_CntNumber", "Container Quantity"), width: 120, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: "CombineInfo", title: _getLang("L_BaseLookup_CombineInfo", "Combined DN Info."), width: 180, sorttype: "string", hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'WsCd', title: 'Warehouse', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'ShipmentId', title: 'Shipment ID', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'CntrNo', title: 'Container No', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'DecNo', title: 'Declaration No', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'CntrType', title: 'Container Type', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'DlvArea', title: '', width: 120, align: 'left', sorttype: 'string', hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'DlvAreaNm', title: 'Delivery Area Name', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'AddrCode', title: '', width: 120, align: 'left', sorttype: 'string', hidden: true, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'DlvAddr', title: 'Delivery Address', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'SealNo1', title: 'Seal No1', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'SealNo2', title: 'Seal No2', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Partof', title: 'Part of', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Ingate', title: 'Part of', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'PinNo', title: 'Pin No.', index: 'PinNo', width: 120, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] },
                    { name: 'Wo', title: 'WO', index: 'Wo', width: 80, align: 'left', sorttype: 'string', hidden: false, viewable: true, sopt: ['eq', 'ne', 'cn', 'cn', 'lt', 'le', 'gt', 'ge', 'cn', 'nu', 'nn', 'in', 'ni'] }]
        };
        var SmrcntrUrl = "IbGateManage/GetSmrcntrInfo"
        MenuBarFuncArr.AddMenu("MBAddConatiner", "glyphicon glyphicon-ok", _getLang("Add Container", "Add ContainerInfo"), function () {
            var status = $("#Status").val(); 
            if (status == "O") {
                CommonFunc.Notify("", "This appointment has finish！", 500, "warning");
                return;
            }
        });
        var _config = $.extend({ multiselect: true }, SmrcntrLookup);
        registBtnLookup($("#MBAddConatiner"), {
            url: rootPath + SmrcntrUrl, config: _config, param: "", selectRowFn: function (map) {
            }
        }, {
            baseConditionFunc: function () {
                var cntrno = $("#CntrNo").val();
                var cntrarray = "'" + cntrno.replace(",", "','") + "'";
                return " CNTR_NO NOT IN ("+cntrarray+")";
            },
            responseMethod: function (data) {
                var reserveno = $("#ReserveNo").val();
                var reservenos = '';
                var cmp = "";
                $.each(data, function (index, val) {
                    reservenos += val.ReserveNo + ",";
                });
                $.ajax({
                    async: true,
                    url: rootPath + "IbGateManage/SpellCTN",
                    type: 'POST',
                    data: {
                        "reserveno": reserveno,
                        "pushdata": reservenos
                    },
                    "complete": function (xmlHttpRequest, successMsg) {

                    },
                    "error": function (xmlHttpRequest, errMsg) {
                        var resJson = $.parseJSON(errMsg)
                        CommonFunc.Notify("", resJson.message, 500, "warning");
                    },
                    success: function (result) {
                        //var resJson = $.parseJSON(result)
                        CommonFunc.Notify("", result.message, 500, "success");
                        initLoadData(_uid);
                    }
                });
            }
        });

        MenuBarFuncArr.AddMenu("MBRemoveConatiner", "glyphicon glyphicon-bell", _getLang("Remove Container", "Remove ContainerInfo"), function () {
            var reserveno = $("#ReserveNo").val();
            if (!reserveno) {
                CommonFunc.Notify("", "No Data!", 500, "warning");
                return;
            }
            $("#spellCntrNoSModel").modal("show");
        });
    }
    else
    {
        MenuBarFuncArr.AddMenu("MBReserveConfirm", "glyphicon glyphicon-ok", _getLang("L_DNManage_ReserveConf", "预约确认"), function(){
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();

            if(Status != "R")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_ClickConfRwe", "只有「已预约」状态才能点击预约确认"), 1000, "warning");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "IbGateManage/ConfirmReverseGate",
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
                            CommonFunc.Notify("", _getLang("L_DNManage_SerConfS", "预约确认成功"), 1000, "success");
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
                CommonFunc.Notify("", _getLang("L_DNManage_NoReserNo", "预约号码不存在"), 1000, "warning");
            }
        });

        MenuBarFuncArr.AddMenu("MBModifyConfirm", "glyphicon glyphicon-refresh", _getLang("L_DNManage_EditConf", "修改确认"), function(){
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();

            if(Status != "C")
            {
                CommonFunc.Notify("", _getLang("L_DNManage_CannoteditCinf", "尚未预约确认，无法修改确认"), 1000, "warning");
                return false;
            }

            if(ReverseNo != "")
            {

                $.ajax({
                    async: true,
                    url: rootPath + "IbGateManage/ModifyReverseGate",
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
                            CommonFunc.Notify("", _getLang("L_DNManage_EdConfS", "修改确认成功"), 1000, "success");
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
                CommonFunc.Notify("", _getLang("L_DNManage_NoReserNo", "预约号码不存在"), 1000, "warning");
            }
        });

        MenuBarFuncArr.AddMenu("MBWHDelivery", "glyphicon glyphicon-ok", _getLang("L_DNManage_EtW", "外仓出货"), function () {
            var ReverseNo = $("#ReverseNo").val();
            var Status = $("#Status").val();
            switch (Status) {
                case "P":
                    alert(_getLang("L_GateReserveSetup_Script_157", "已经封柜，不能执行外仓出货！"));
                    return false;
                    break;
                case "O":
                    alert(_getLang("L_GateReserveSetup_Script_158", "已经离厂，不能执行外仓出货！"));
                    return false;
                    break;
                case "V":
                    alert(_getLang("L_GateReserveSetup_Script_159", "已经取消，不能执行外仓出货！"));
                    return false;
                    break;
                case "E":
                    alert(_getLang("L_GateReserveSetup_Script_160", "暂时离厂，不能执行外仓出货！"));
                    return false;
                    break;
            }

            if (!confirm(_getLang("L_DNManage_SureEtW", "您确认执行外仓出货吗？"))) {
                return false;
            }

            if (ReverseNo != "") {

                $.ajax({
                    async: true,
                    url: rootPath + "IbGateManage/WHouseDelivery",
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
                            CommonFunc.Notify("", _getLang("L_DNManage_EtWS", "外仓出货成功"), 1000, "success");
                            initLoadData(_uid);
                        }
                        else {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                });
            }
            else {
                CommonFunc.Notify("", _getLang("L_DNManage_NoReserNo", "预约号码不存在"), 1000, "warning");
            }
        });

    }
    

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
            MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm"]);
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }

        var FclData = $("#FclMainGrid").jqGrid('getGridParam', "arrangeGrid")();
        var DnData = $("#DnMainGrid").jqGrid('getGridParam', "arrangeGrid")();
        changeData["sub"] = FclData;
        changeData["sub2"] = DnData;
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/SaveGateReseve",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val(), RelationId: $("#RelationId").val(),
                ReserveDate: $("#ReserveDate").val(), ReserveFrom: $("#ReserveFrom").val(), ShipmentInfo:$("#ShipmentInfo").val()
            },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败")+" "+ result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                console.log(result.mainData);
                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveS", "保存成功"), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Enabled(["MBReserve", "MBReserveConfirm", "MBModifyConfirm", "MBWHDelivery","MBAddConatiner"]);
                _status = "edit";
                chkMenu($("#Status").val());
                dtd.resolve();
                initLoadData(_uid);
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getChangeValue();
        $.ajax({
            async: true,
            url: rootPath + "IbGateManage/SaveGateReseve",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val(), ReserveDate: $("#ReserveDate").val(), ReserveFrom: $("#ReserveFrom").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_SaveF", "保存失败"), 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelF", "删除失败"), 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", _getLang("L_MailFormatSetup_DelS", "删除成功"), 500, "success");
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
            if(!confirm(_getLang("L_DNManage_WrongCntrNo", "您输入的货柜号码规则有误，请问要用此号码吗？")))
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
        MenuBarFuncArr.Disabled(["MBReserve", "MBEdit", "MBReserveConfirm","MBModifyConfirm","MBWHDelivery","MBAddConatiner"]);
    }
    if ("O" == status && "C" == page)
    {
        MenuBarFuncArr.Enabled(["MBEdit"]);
    }
    if (status != "D") {
        MenuBarFuncArr.Disabled(["MBReserve","MBAddConatiner"]);
    }
}


function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}
