var $SubGrid = $("#SubGrid");
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var subStatus = "";
var mainStatus = "";
var gridHeight = 0;

$(function () {   
    stSchemas = JSON.parse(decodeHtml(stSchemas));
    CommonFunc.initField(stSchemas, "mt");

    setdisabled(true);
    setSTdisabled(true);
    
    _initMainMenu();
    _initLookup();
    initLoadData(_uid);
    
});

function _initMainMenu()
{
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch"]);

    MenuBarFuncArr.MBAdd = function(){
        $("#UId").removeAttr('required');
    };

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#UId").removeAttr('required');
        $("#UId").val("");
    }

    MenuBarFuncArr.AddMenu("MBReloadProfile", "glyphicon glyphicon-refresh", "Reload Profile", function () {
        var confirmReload = window.confirm("@Resources.Locale.L_SystemController_Reload");
        if (!confirmReload) return;
        var profileCode = $("#Profile").val();
        var uid = $("#UId").val();
        $.ajax({
            async: true,
            url: rootPath + "System/SynchronizationProfile",
            data:
                {
                    "ProfileCode": profileCode
                },
            type: 'POST',
            "error": function (xmlHttpRequest, errMsg) {
                var resJson = $.parseJSON(errMsg)
                CommonFunc.Notify("", resJson.errMsg, 500, "warning");
            },
            success: function (data) {
                if (data.errMsg) {
                    alert(data.errMsg);
                    return false;
                }
                initLoadData(uid);
            }
        });
    });

    MenuBarFuncArr.MBSave = function (dtd) {
        var changeData = getChangeValue();
        //var SubGridChageArray = localDB.getChangeData("SMSID");

        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }

        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "SMPTY/SmsimUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val()},
            dataType: "json",
            error: function (xmlHttpRequest, errMsg) {
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
                dtd.resolve();
            }
        });
        
        /*
        $.post(rootPath + 'SMPTY/chkSmsimKey', {CustCd: $("#CustCd").val(), Cmp: $("#Location").val(), InvFlow:$("#InvFlow").val()}, function(data, textStatus, xhr) {
            var msg = data.msg;
            if(msg == "success" || mainStatus == "edit")
            {
                
            }
            else
            {
                CommonFunc.Notify("", "Customer + Location + Invoice Flow有重覆，不能保存", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return false;
            }
        }, "JSON");
        */
        return dtd.promise();
    }

    MenuBarFuncArr.MBDel = function () {
        var changeData = getChangeValue();
        //表示值沒變
        $.ajax({
            async: true,
            url: rootPath + "SMPTY/SmsimUpdateData",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("#UId").val() },
            dataType: "json",
            "error": function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                //dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") 
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelF", 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    //dtd.resolve();
                    return false;
                }

                setFieldValue(result.mainData);

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
            }
        });
        
    }

    initMenuBar(MenuBarFuncArr);
}

function _initLookup()
{
    var CmpOpt = {};
    CmpOpt.gridUrl = rootPath + "TPVCommon/GetSiteCmpData";
    CmpOpt.param = "";
    CmpOpt.baseCondition = " GROUP_ID='"+groupId+"' AND TYPE='1'";
    CmpOpt.registerBtn = $("#LocationLookup");
    CmpOpt.focusItem = $("#Location");
    CmpOpt.gridFunc = function (map) {
        var value = map.Cd;
        $("#Location").val(value);
        $("#LocationNm").val(map.CdDescp);
    };
    CmpOpt.lookUpConfig = LookUpConfig.SiteLookup;
    initLookUp(CmpOpt);
    CommonFunc.AutoComplete("#Location", 1, "", "dt=stn&GROUP_ID=" + groupId + "&TYPE=1&CMP=", "CMP=showValue,CMP,NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.CMP);
        $("#LocationNm").val(ui.item.returnValue.NAME);
        return false;
    });


    /*子表放大鏡*/
    //POL Lookup
    setCityData("PolLookup", "Pol", "PolNm", "");
    //POD Lookup
    setCityData("PodLookup", "Pod", "PodNm", "");
    //Dest Lookup
    setCityData("DestLookup", "Dest", "DestNm", "");
    //Tran Mode Lookup
    setBscData("TranModeLookup", "TranMode", "", "TNT");

    //Customer Lookup
    setSmptyData("CustCdLookup", "CustCd", "CustNm", "FC");
    //Shipper Lookup
    setSmptyData("SellerLookup", "Seller", "SellerNm", "SH");

    //Buyer1 Lookup
    setSmptyData("Buyer1Lookup", "Buyer1", "Buyer1Nm", "CS");
    //Buyer2 Lookup
    setSmptyData("Buyer2Lookup", "Buyer2", "Buyer2Nm", "CS");
    //Buyer3 Lookup
    setSmptyData("Buyer3Lookup", "Buyer3", "Buyer3Nm", "CS");
    //Buyer4 Lookup
    setSmptyData("Buyer4Lookup", "Buyer4", "Buyer4Nm", "CS");
    //Buyer5 Lookup
    setSmptyData("Buyer5Lookup", "Buyer5", "Buyer5Nm", "CS");

    setSmptyData("ShipToLookup", "ShipTo", "ShipToNm", "WE");

    setSmptyData("FiCustomerLookup", "FiCustomer", "FiCustomerNm", "FC");

    setSmptyData("SubBgLookup", "SubBg", "SubBgNm", "ZT");

    setSmptyData("BillToLookup", "BillTo", "BillToNm", "RE");

    setSmptyData("PayerPartyLookup", "PayerParty", "PayerPartyNm", "RG");

    setSmptyData("OrderReceiverLookup", "OrderReceiver", "OrderReceiverNm", "RO");

    setSmptyData("SoldToLookup", "SoldTo", "SoldToNm", "AG");

    setSmptyData("SalesCustomerLookup", "SalesCustomer", "SalesCustomerNm", "ZE");

    //Incoterm1 Lookup
    setBscData("Incoterm1Lookup", "Incoterm1", "", "TINC");
    //Incoterm2 Lookup
    setBscData("Incoterm2Lookup", "Incoterm2", "", "TINC");
    //Incoterm3 Lookup
    setBscData("Incoterm3Lookup", "Incoterm3", "", "TINC");
    //Incoterm4 Lookup
    setBscData("Incoterm4Lookup", "Incoterm4", "", "TINC");
    //Incoterm5 Lookup
    setBscData("Incoterm5Lookup", "Incoterm5", "", "TINC");

    //Consignee Lookup
    setSmptyData("CneeCdLookup", "CneeCd", "CneeNm", "CS");

    //Notify1 Lookup
    setSmptyData("Notify1Lookup", "Notify1", "Notify1Nm", "NT");
    //Notify2 Lookup
    setSmptyData("Notify2Lookup", "Notify2", "Notify2Nm", "NT");
    //Notify3 Lookup
    setSmptyData("Notify3Lookup", "Notify3", "Notify3Nm", "NT");
    //Notify1 Lookup
    setSmptyData("Notify4Lookup", "Notify4", "Notify4Nm", "NT");

    //CARRIERY1 Lookup
    setSmptyData("Carrier1Lookup", "Carrier1", "CarrierNm1", "FS");
    //CARRIERY1 Lookup
    setSmptyData("Carrier2Lookup", "Carrier2", "CarrierNm2", "FS");
    //CARRIERY1 Lookup
    setSmptyData("Carrier3Lookup", "Carrier3", "CarrierNm3", "FS");

    //TRUCKER Lookup
    setSmptyData("DtCdLookup", "DtCd", "DtNm", "DT");
    //DocReq1 Lookup
    setBscData("Doc1Lookup", "Doc1", "", "EDT");
    //Doc2 Lookup
    setBscData("Doc2Lookup", "Doc2", "", "EDT");
    //Doc3 Lookup
    setBscData("Doc3Lookup", "Doc3", "", "EDT");
    //Doc4 Lookup
    setBscData("Doc4Lookup", "Doc4", "", "EDT");
    //Doc5 Lookup
    setBscData("Doc5Lookup", "Doc5", "", "EDT");

    //CC Lookup
    setSmptyData("CcCdLookup", "CcCd", "CcNm", "CC");

    //Booking Agent Lookup
    setSmptyDataN("LspNo1Lookup", "LspNo1", "LspNm1", "SP;BO");
    //Booking Agent Lookup
    setSmptyDataN("LspNo2Lookup", "LspNo2", "LspNm2", "SP;BO");
    //Booking Agent Lookup
    setSmptyDataN("LspNo3Lookup", "LspNo3", "LspNm3", "SP;BO");


    //SMPTY放大鏡
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#IsfSellerLookup");
    options.focusItem = $("#IsfSeller");
    options.isMutiSel = true;
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%SL%'";
    options.gridFunc = function (map) {
        var addr_str = (map.PartAddr1 || "") + " " + (map.PartAddr2 || "") + " " + (map.PartAddr3 || "") + " " + (map.PartAddr4 || "") + " " + (map.PartAddr5 || "");
        $("#IsfSeller").val(map.PartyNo);
        $("#IsfSellernm").val(map.PartyName);

        var a1 = cutWord(addr_str, 35);
        var a2 = cutWord(addr_str.substr(a1.length, addr_str.length), 35);
        var a3 = cutWord(addr_str.substr(a1.length+a2.length, addr_str.length), 35);
        $("#IsfSelleraddr1").val(a1);
        $("#IsfSelleraddr2").val(a2);
        $("#IsfSelleraddr3").val(a3);
        $("#IsfSellercnty").val(map.Cnty);
        $("#IsfSellercntyNm").val(map.CntyNm);
        $("#IsfSellercity").val(map.City);
        $("#IsfSellercityNm").val(map.CityNm);
        $("#IsfSellerstate").val(map.State);
        $("#IsfSellerzip").val(map.Zip);
        $("#ImRecord").val(map.ImRecord);
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#IsfSeller", 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO~", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME,PART_ADDR1,PART_ADDR2,PART_ADDR3,PART_ADDR4,PART_ADDR5,CNTY,CNTY_NM,CITY,CITY_NM,STATE,ZIP,IMRECORD", function (event, ui) {
        var map = ui.item.returnValue;
        var PartyNo = ui.item.returnValue.PARTY_NO;
        PartyNo = padLeft(PartyNo, 10);
        $("#IsfSeller").val(ui.item.returnValue.PARTY_NO);
        $("#IsfSellernm").val(ui.item.returnValue.PARTY_NAME);
        var addr_str = (map.PART_ADDR1 || "") + " " + (map.PART_ADDR2 || "") + " " + (map.PART_ADDR3 || "") + " " + (map.PART_ADDR4 || "") + " " + (map.PART_ADDR5 || "");
        var a1 = cutWord(addr_str, 35);
        var a2 = cutWord(addr_str.substr(a1.length, addr_str.length), 35);
        var a3 = cutWord(addr_str.substr(a1.length+a2.length, addr_str.length), 35);
        $("#IsfSelleraddr1").val(a1);
        $("#IsfSelleraddr2").val(a2);
        $("#IsfSelleraddr3").val(a3);
        $("#IsfSellercnty").val(map.CNTY);
        $("#IsfSellercntyNm").val(map.CNTY_NM);
        $("#IsfSellercity").val(map.CITY);
        $("#IsfSellercityNm").val(map.CITY_NM);
        $("#IsfSellerstate").val(map.STATE);
        $("#IsfSellerzip").val(map.ZIP);
        $("#ImRecord").val(map.IMRECORD);
        return false;
    });

    //国家代码
    cntryOptions = {};
    cntryOptions.gridUrl = rootPath + "Common/GetCntryCdData";
    cntryOptions.registerBtn = $("#IsfSellercntyLookup");
    cntryOptions.isMutiSel = true;
    cntryOptions.focusItem = $("#IsfSellercnty");
    cntryOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //cntryOptions.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    cntryOptions.gridFunc = function (map) {
        var cntryCd = map.CntryCd,
            cntryNm = map.CntryNm;
        $("#IsfSellercnty").val(cntryCd);
        $("#IsfSellercntyNm").val(cntryNm.substr(0,20));
    }
    cntryOptions.responseMethod = function () { }
    cntryOptions.lookUpConfig = LookUpConfig.CntyCdLookup;
    initLookUp(cntryOptions);
    CommonFunc.AutoComplete("#IsfSellercnty", 2, "", "dt=country&GROUP_ID=" + groupId+ "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $("input[name='IsfSellercntyNm']").val(ui.item.returnValue.CNTRY_NM);
        $(this).val(ui.item.returnValue.CNTRY_CD);
        return false;
    });

    ////城市港口
    cityOptions = {};
    cityOptions.gridUrl = rootPath + "Common/GetCityCdData";
    cityOptions.registerBtn = $("#IsfSellercityLookup");
    cityOptions.isMutiSel = true;
    cityOptions.focusItem = $("#IsfSellercity");
    cityOptions.param = "&sopt_GroupId=eq&GroupId=" + groupId;
    //options.baseCondition = " (BU='C' OR BU='D' OR BU='E')";
    cityOptions.gridFunc = function (map) {
        var cd = map.PortCd,
            nm = map.PortNm;
        $("#IsfSellercity").val(cd);
        $("#IsfSellercityNm").val(nm);
    }
    cityOptions.responseMethod = function () { }
    cityOptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(cityOptions);
    CommonFunc.AutoComplete("#IsfSellercity", 2, "", "dt=port&GROUP_ID=" + groupId + "&PORT_CD=", "PORT_CD=showValue,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.PORT_CD);
        $("#IsfSellercityNm").val(ui.item.returnValue.PORT_NM);
        return false;
    });

    //State Lookup
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetStateDataForLookup";
    options.registerBtn = $("#IsfSellerstateLookup");
    options.focusItem = $("#IsfSellerstate");
    options.param = "";
    options.isMutiSel = true;
    options.baseConditionFunc = function(){
        var CntryCd = $("#IsfSellercnty").val();

        if(CntryCd != "")
        {
            //return " CNTRY_CD='" + CntryCd + "'"; 
            return "CntryCd=" + CntryCd + "&sopt_CntryCd=eq";
        }
        else
        {
            return "";
        }
    }
    options.gridFunc = function (map) {
        $("#IsfSellerstate").val(map.StateCd);
    }

    options.lookUpConfig = LookUpConfig.StateLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#IsfSellerstate", 1, "", "dt=state&GROUP_ID=" + groupId + "&STATE_CD=", "STATE_CD=showValue,STATE_CD,STATE_NM", function (event, ui) {
        var map = ui.item.returnValue;
        $(this).val(ui.item.returnValue.STATE_CD);
        return false;
    }, function(){
        var CntryCd = $("#IsfSellercnty").val();

        if(CntryCd != "")
        {
            return " CNTRY_CD=" + CntryCd; 
        }
        else
        {
            return "";
        }
    });
    
}

function cutWord(yourString, maxLength)
{
    //trim the string to the maximum length
    var trimmedString = yourString.substr(0, maxLength);

    //re-trim if we are in the middle of a word
    trimmedString = trimmedString.substr(0, Math.min(trimmedString.length, trimmedString.lastIndexOf(" ")));

    return trimmedString;
}

function setSmptyData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.baseConditionFunc = function () {
        return "&sopt_PartyType=cn&PartyType=" + pType;
    }
    options.isMutiSel = true;
    options.param = "";
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.PartyNo);

        if(Nm != "")
            $("#" + Nm).val(map.PartyName);
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~"+pType+"&PARTY_NO~", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        var PartyNo = ui.item.returnValue.PARTY_NO;
        PartyNo = padLeft(PartyNo, 10);
        $("#" + Cd).val(ui.item.returnValue.PARTY_NO);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);
        return false;
    }, null, function() {
         if(Nm != "")
         $("#" +Nm).val("");
});
}

function setSmptyDataN(lookUp, Cd, Nm, pType) {
    //SMPTY放大鏡
    options = {
    };
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#" +lookUp);
    options.focusItem = $("#" +Cd);
    options.baseConditionFunc = function () {
        return "&sopt_PartyType=li&PartyType=" +pType;
}
options.isMutiSel = true;
options.param = "";
options.gridFunc = function (map) {
    $("#" +Cd).val(map.PartyNo);

    if(Nm != "")
        $("#" +Nm).val(map.PartyName);
        }

options.lookUpConfig = LookUpConfig.SmptyLookup;
initLookUp(options);

CommonFunc.AutoComplete("#" +Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE^"+pType+"&PARTY_NO~", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        var PartyNo = ui.item.returnValue.PARTY_NO;
        PartyNo = padLeft(PartyNo, 10);
        $("#" +Cd).val(ui.item.returnValue.PARTY_NO);

        if (Nm != "")
            $("#" +Nm).val(ui.item.returnValue.PARTY_NAME);
        return false;
        }, null, function() {
            if(Nm != "")
            $("#" +Nm).val("");
            });
            }
    function setBscData(lookUp, Cd, Nm, pType)
        {
        //SMPTY放大鏡
        options = { };
        options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
        options.registerBtn = $("#"+lookUp);
        options.focusItem = $("#" + Cd);
        options.baseCondition = " GROUP_ID='"+groupId+"' AND CD_TYPE='"+pType+"'";
        options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" +Cd).val(map.Cd);

        if(Nm != "")
            $("#" +Nm).val(map.CdDescp);
            }

        options.lookUpConfig = LookUpConfig.BSCodeLookup;
        initLookUp(options);

        CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=bsc&GROUP_ID=" +groupId + "&CD_TYPE~"+pType+"&CD=", "CD=showValue,CD,CD_DESCP", function (event, ui) {
        $(this).val(ui.item.returnValue.CD);

        if(Nm != "")
                $("#" +Nm).val(ui.item.returnValue.CD_DESCP);

        return false;
    });
}

function setCityData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBsCityDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.CntryCd + map.PortCd);

        if(Nm != "")
            $("#" + Nm).val(map.PortNm);
    }

    options.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#"+Cd, 1, "", "dt=port&GROUP_ID=" + groupId + "&CNTRY_CD+PORT_CD=", "CNTRY_CD&PORT_CD=showValue,CNTRY_CD,PORT_CD,PORT_NM", function (event, ui) {
        $(this).val(ui.item.returnValue.CNTRY_CD + ui.item.returnValue.PORT_CD);

        if(Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PORT_NM);

        return false;
    });
}

function initLoadData(Uid)
{
    if (!Uid)
    {
        return;
    }
    $.ajax({
        async: true,
        url: rootPath + "SMPTY/GetSmsimDetail",
        type: 'POST',
        data: {
            UId: Uid
        },
        dataType: "json",
        beforeSend: function () {
            CommonFunc.ToogleLoading(true);
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            var maindata = jQuery.parseJSON(result.mainTable.Content);
            setFieldValue(maindata.rows);

            console.log(maindata);
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            
            CommonFunc.ToogleLoading(false);
        }
    });
}

function padLeft(str,lenght){
    if(str.length >= lenght)
    return str;
    else
    return padLeft("0" +str,lenght);
}