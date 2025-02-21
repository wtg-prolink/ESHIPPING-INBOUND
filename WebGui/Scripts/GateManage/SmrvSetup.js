
var url = "";
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var Dndata = null;
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

    CommonFunc.AutoComplete("#TruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.TRUCK_NO);
        
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

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
    CommonFunc.AutoComplete("#Driver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_ID,DRIVER_PHONE", function (event, ui) {
        $(this).val(ui.item.returnValue.DRIVER_NAME);
        $("#Tel").val(ui.item.returnValue.DRIVER_PHONE);
        $("#DriverId").val(ui.item.returnValue.DRIVER_ID);
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    }
    //, function () {
    //    $("#Tel").val("");
    //    $("#DriverId").val("");
    //}
    );

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

    CommonFunc.AutoComplete("#LtruckNo", 1, "", "dt=bstruckc&TRUCK_NO=", "TRUCK_NO=showValue,TRUCK_NO", function (event, ui) {
        $(this).val(ui.item.returnValue.TRUCK_NO);
        
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    });

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

    CommonFunc.AutoComplete("#Ldriver", 1, "", "dt=bstruckd&DRIVER_NAME=", "DRIVER_NAME=showValue,DRIVER_ID,DRIVER_PHONE", function (event, ui) {
        $(this).val(ui.item.returnValue.DRIVER_NAME);
        $("#Ltel").val(ui.item.returnValue.DRIVER_PHONE);
        $("#LdriverId").val(ui.item.returnValue.DRIVER_ID);
        return false;
    }, function(){
        var Trucker = $("#Trucker").val()

        return " PARTY_NO="+Trucker;
    }
    //, function () {
    //    $("#Ltel").val("");
    //    $("#LdriverId").val("");
    //}
    );

    $("#CntrNo").on("change", function(){
        var val = $(this).val();
        val = val.toUpperCase();
        $(this).val(val);
        var chk = checkCtnNo(val);
        var TruckCntrno = $("#TruckCntrno").val();

        if(TruckCntrno != val)
        {
            alert("@Resources.Locale.L_DNManage_SlNoDiff");
        }

        if(chk === false)
        {
            if(!confirm("@Resources.Locale.L_DNManage_WrongCntrNo"))
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
            alert("@Resources.Locale.L_SmrvSetup_Script_163");
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


            setdisabled(true);
            setToolBtnDisabled(true);

            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            CommonFunc.ToogleLoading(false);

            var multiEdocData = [];
            console.log(result.SmData);

            if (typeof result.SmData !== "undefined") {
                if (result.SmData.length == 1) {
                    MenuBarFuncArr.initEdoc(result.SmData[0].UId, result.SmData[0].GroupId, result.SmData[0].Cmp, "*");
                }
                else {
                    $.each(result.SmData, function (index, val) {
                        multiEdocData.push({ jobNo: result.SmData[index].UId, 'GROUP_ID': result.SmData[index].GroupId, 'CMP': result.SmData[index].Cmp, 'STN': '*' });
                    });
                    if (result.SmData.length != 0) {
                        MenuBarFuncArr.initEdoc(result.SmData[0].UId, result.SmData[0].GroupId, result.SmData[0].Cmp, "*", multiEdocData);
                    }
                }
            }

            if (typeof result.DnData !== "undefined")
            {
                Dndata = result.DnData;
                InitSealMode(Dndata);
            }
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

    MenuBarFuncArr.AddMenu("MBSeal", "glyphicon glyphicon-barcode", "@Resources.Locale.L_DNManage_SlRl", function () {
        var ReverseNo = $("#ReverseNo").val();
        var CntrNo = $("#CntrNo").val();
        var SealNo1 = $("#SealNo1").val();
        var TranType = $("#TranType").val();
        var Status = $("#Status").val();
        var PutDate = $("#PutDate").val();
        var TareWeight = $("#TareWeight").val();
        if (Status == "D" || Status == "R" || Status == "C") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CtSlIb", 1000, "warning");
            return;
        }
        if (Status == "P") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasSlBf", 1000, "warning");
            return;
        }
        if (Status == "O") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CtSlOut", 1000, "warning");
            return;
        }
        if ((TranType == "F" || TranType == "R") && (SealNo1 == "" || CntrNo == "")) {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CCdSNo", 1000, "warning");
            return;
        }

        if (TranType == "F" && TareWeight == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_SmrvSetup_Script_162", 1000, "warning");
            return;
        }

        if (PutDate == "") {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CtSlNtLd", 1000, "warning");
            return;
        }
        var all = $("#checkall").is(':checked');
        if (Dndata.length ==0) {
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
                        var truthBeTold = window.confirm(result.message + ",@Resources.Locale.L_DNManage_SealorNot");
                        if (!truthBeTold) {
                            return;
                        }
                        InFactoryConfirm();
                    }
                }
            });

            function InFactoryConfirm() {
                $.ajax({
                    async: true,
                    url: rootPath + "Api/InFactoryConfirm",
                    type: 'POST',
                    data: { id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val() },
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
                        if (result.message == "@Resources.Locale.L_ApiController_Controllers_28") {
                            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SealS", 1000, "success");
                            initLoadData(_uid);
                        }
                        else {
                            CommonFunc.Notify("", result.message, 1000, "warning");
                        }
                    }
                })
            }
        }
        else if (Dndata.length == 1 || all) {
            SealDo();
        }
        else {
            //var all = $("#checkall").is(':checked');
            //if (Dndata.length <= 1 || all) {
            //    $.ajax({
            //        async: true,
            //        url: rootPath + "Api/CheckQty",
            //        type: 'POST',
            //        data: { id: $("#UId").val() },
            //        dataType: "json",
            //        beforeSend: function () {
            //            CommonFunc.ToogleLoading(true);
            //        },
            //        "complete": function (xmlHttpRequest, successMsg) {
            //            CommonFunc.ToogleLoading(false);
            //        },
            //        "error": function (xmlHttpRequest, errMsg) {
            //            CommonFunc.Notify("", errMsg, 1000, "danger");
            //            CommonFunc.ToogleLoading(false);
            //        },
            //        success: function (result) {
            //            CommonFunc.ToogleLoading(false);
            //            if (result.flag === true) {
            //                InFactoryConfirm();
            //            }
            //            else {
            //                var truthBeTold = window.confirm(result.message + ",@Resources.Locale.L_DNManage_SealorNot");
            //                if (!truthBeTold) {
            //                    return;
            //                }
            //                InFactoryConfirm();
            //            }
            //        }
            //    });

            //    function InFactoryConfirm() {
            //        $.ajax({
            //            async: true,
            //            url: rootPath + "Api/InFactoryConfirm",
            //            type: 'POST',
            //            data: { id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val() },
            //            dataType: "json",
            //            beforeSend: function () {
            //                CommonFunc.ToogleLoading(true);
            //            },
            //            "error": function (xmlHttpRequest, errMsg) {
            //                CommonFunc.Notify("", errMsg, 1000, "danger");
            //                CommonFunc.ToogleLoading(false);
            //            },
            //            success: function (result) {
            //                CommonFunc.ToogleLoading(false);
            //                if (result.message == "@Resources.Locale.L_ApiController_Controllers_28") {
            //                    CommonFunc.Notify("", "@Resources.Locale.L_DNManage_SealS", 1000, "success");
            //                    initLoadData(_uid);
            //                }
            //                else {
            //                    CommonFunc.Notify("", result.message, 1000, "warning");
            //                }
            //            }
            //        });
            //    }
            //} else {
            //    $("#SealDetail").modal("show");
            //}
            $("#SealDetail").modal("show");
        }
    }
    );
        MenuBarFuncArr.MBCancel = function(){
            //MenuBarFuncArr.Disabled(["MBReload"]);
            MenuBarFuncArr.Enabled(["MBSeal"]);
        };
    

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }
        if (contrast($("#PutDate").val(), $("#SealDate").val())) {
            alert("@Resources.Locale.L_BSCSSetup_SFail" + ":" + "@Resources.Locale.L_PutTime_Tip");
            MenuBarFuncArr.SaveResult = false;
            dtd.resolve();
            return false;
        }
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
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Enabled(["MBSeal"]);
                dtd.resolve();
            }
        });
        return dtd.promise();
    }
    addInOutMenu();
    initMenuBar(MenuBarFuncArr);
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
    var msg = "@Resources.Locale.L_DNManage_EmtCar";
    if (mode === 0) msg = "@Resources.Locale.L_DNManage_NoEmtCar";
    var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Whether5"+msg+"@Resources.Locale.L_DNManage_InboundConf");
    if (!iscontinue) {
        return;
    }

    var ReverseNo = $("#ReverseNo").val();
    var CntrNo = $("#CntrNo").val();
    var SealNo1 = $("#SealNo1").val();
    var TranType = $("#TranType").val();
    var Status = $("#Status").val();

    if (Status == "P") {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasSeal", 1000, "warning");
        return;
    }
    if (Status == "O") {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasOut", 1000, "warning");
        return;
    }
    if (Status !== "R" && Status !== "C" && Status !== "E") {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasIn", 1000, "warning");
        return;
    }

    $.ajax({
        async: true,
        url: rootPath + "Api/RFactoryConfirm",
        type: 'POST',
        data: { mode:mode,id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val() },
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
    var msg = "@Resources.Locale.L_DNManage_EmtCar";
    if (mode === 0) msg = "@Resources.Locale.L_DNManage_NoEmtCar";
    var iscontinue = window.confirm("@Resources.Locale.L_DNManage_Whether5" + msg + "@Resources.Locale.L_SmrvSetup_Script_165");
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
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_HasOut", 1000, "warning");
        return;
    }
    if (Status === "R" || Status === "C" || Status === "E") {
        CommonFunc.Notify("", "@Resources.Locale.L_DNManage_NotIn", 1000, "warning");
        return;
    }

    var check_SealNo = $("#SealNo1").val();
    if (mode == 0) {
        check_SealNo = prompt("@Resources.Locale.L_DNManage_EntSlNo", "");
        check_SealNo = check_SealNo || "";
    }

    if(mode == 0)
    {
        if(PutDate == "" && SealDate == "")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_DNManage_CantOut", 1000, "warning");
            return;
        }
    }

    $.ajax({
        async: true,
        url: rootPath + "Api/OutFactoryConfirm",
        type: 'POST',
        data: { mode: mode, id: $("#UId").val(), status: $("#Status").val(), LtruckNo: $("#LtruckNo").val(), Ldriver: $("#Ldriver").val(), tranType: $("#TranType").val(), cntr_no: $("#CntrNo").val(), seal_no1: $("#SealNo1").val(), check_SealNo: encodeURIComponent(check_SealNo) },
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
        MenuBarFuncArr.AddMenu("MBIn0", "glyphicon glyphicon-barcode", "@Resources.Locale.L_DNManage_InConfNtEmp", function () {
            InConfirm(0);
        });

        MenuBarFuncArr.AddMenu("MBIn1", "glyphicon glyphicon-barcode", "@Resources.Locale.L_DNManage_InCinfEmp", function () {
            InConfirm(1);
        });
    }
    if (_flag === "1") {
        MenuBarFuncArr.AddMenu("MBOut0", "glyphicon glyphicon-barcode", "@Resources.Locale.L_DNManage_OutConfNtEmp", function () {
            OutConfirm(0);
        });

        MenuBarFuncArr.AddMenu("MBOut1", "glyphicon glyphicon-barcode", "@Resources.Locale.L_DNManage_OutConfEmp", function () {
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
        MenuBarFuncArr.Disabled(["MBEdit", "MBSeal"]);
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