var $SubGrid = $("#SubGrid");
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var subStatus = "";
var mainStatus = "";
var gridHeight = 0;
var docHeight = $(document).height();
gridHeight = docHeight - 420;

$(function () {   
    mtSchemas = JSON.parse(decodeHtml(mtSchemas));
    //stSchemas = JSON.parse(decodeHtml(stSchemas));

    CommonFunc.initField(mtSchemas, "mt");
    setdisabled(true);
    //setSTdisabled(true);
    
    _initMainMenu();
    //_initSubMenu();
    _initLookup();
    _initGenGrid();
    initLoadData(_uid);

    $("#goToAdd").click(function(){
        var uid = $("#UId").val();

        if(!uid)
        {
            CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Main", 500, "warning");
            return;
        }

        top.topManager.openPage({
            href: rootPath + "System/BSTDataSetup/" + uid,
            title: '@Resources.Locale.L_SystemController_CusDS',
            id: 'BSTDataSetup'
        });
    });

    $("#reloadGrid").click(function(){
        $SubGrid.jqGrid("setGridParam", {
            url: rootPath + "SMPTY/GetSmsidGridData",
            rowNum: 999,
            postData:  {UId: $("#UId").val()},
            page: 1,
            datatype: "json",
            sortname: "SeqNo",
            sortorder: "asc",
        }).trigger("reloadGrid");
    });
    
});

function _initMainMenu()
{
    MenuBarFuncArr.DelMenu(["MBApprove", "MBPrint", "MBErrMsg", "MBEdoc", "MBInvalid", "MBSearch"]);

    MenuBarFuncArr.MBAdd = function(){
        mainStatus = "add";
        $("#UId").removeAttr('required');
        //stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy", "STDel"]);
        //$SubGrid.jqGrid("clearGridData");
        //$("#subFormData")[0].reset();
    };

    MenuBarFuncArr.MBCopy = function (thisItem) {
        mainStatus = "copy";
        $("#UId").removeAttr('required');
        $("#UId").val("");
        //stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy", "STDel"]);
        //$SubGrid.jqGrid("clearGridData");
        //localDB.createTable("SMSID", "UId", []);
        //$("#subFormData")[0].reset();
    }

    MenuBarFuncArr.MBEdit = function(){
        mainStatus = "edit";
        //stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy", "STDel"]);
    };

    MenuBarFuncArr.MBCancel = function(){

        $SubGrid.jqGrid("setGridParam", {
            url: rootPath + "SMPTY/GetSmsidGridData",
            rowNum: 999,
            postData:  {UId: $("#UId").val()},
            page: 1,
            datatype: "json"
        }).trigger("reloadGrid");

        //setSTdisabled(true);
        //stMenuBarFunc.DisabledAll();
    };

    MenuBarFuncArr.MBSave = function (dtd) {
        if(subStatus == "copy" || subStatus == "edit" || subStatus == "add")
        {
            $("#STSave").click();
        }
        var changeData = getChangeValue();
        //var SubGridChageArray = $SubGrid.jqGrid('getGridParam', "arrangeGrid")();
        var SubGridChageArray = localDB.getChangeData("SMSID");

        //表示值沒變
        if ($.isEmptyObject(changeData)) {
            CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
            MenuBarFuncArr.SaveResult = true;
            dtd.resolve();
            setdisabled(true);
            return;
        }


        changeData["st"] = [];
        changeData["st"] = SubGridChageArray;

        console.log(changeData);
        $.post(rootPath + 'SMPTY/chkSmsimKey', {CustCd: $("#CustCd").val(), Cmp: $("#Location").val(), InvFlow:$("#InvFlow").val()}, function(data, textStatus, xhr) {
            var msg = data.msg;
            if(msg == "success" || mainStatus == "edit")
            {
                $.ajax({
                    async: true,
                    url: rootPath + "SMPTY/SmsimUpdateData",
                    type: 'POST',
                    data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true, UId: $("input[dt='mt'][name='UId']").val() },
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
                        //stMenuBarFunc.DisabledAll(); //關掉子表操作

                        //localDB.createTable("SMSID", "UId", result.subData1); //創建local DB
                        $SubGrid.jqGrid("setGridParam", {
                            datatype: 'local',
                            sortorder: "asc",
                            sortname: "SeqNo",
                            data: result.subData1
                        }).trigger("reloadGrid");


                        gridEditableCtrl({ editable: false, gridId: "SubGrid" });
                        setdisabled(true);
                        setToolBtnDisabled(true);
                        CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                        MenuBarFuncArr.SaveResult = true;
                        //$("#subFormData")[0].reset();
                        dtd.resolve();
                    }
                });
            }
            else
            {
                CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Repeat", 500, "warning");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
                return false;
            }
        }, "JSON");
        
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
                $SubGrid.jqGrid("clearGridData");

                //stMenuBarFunc.DisabledAll(); //關掉子表操作
                //localDB.createTable("SMSID", "UId", result.subData1); //創建local DB

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
            }
        });
        
    }

    initMenuBar(MenuBarFuncArr);
}

function _initSubMenu()
{
    stMenuBarFunc.STAdd = function(){
        $("#Pol").setfocus();

        var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
        if (typeof maxSeqNo === "undefined")
        {
            maxSeqNo = 0;
        }

        $("#SeqNo").val(maxSeqNo + 1);
        $("input[dt='st'][name='UId']").val(uuid());
    }

    stMenuBarFunc.STCopy = function(){
        $("#Pol").setfocus();

        var maxSeqNo = $SubGrid.jqGrid("getCol", "SeqNo", false, "max");
        if (typeof maxSeqNo === "undefined")
        {
            maxSeqNo = 0;
        }

        if($("#SeqNo").val() == "")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Copy", 500, "warning");
            return false;
        }

        $("#SeqNo").val(maxSeqNo + 1);
        $("input[dt='st'][name='UId']").val(uuid());
    }

    stMenuBarFunc.STEdit = function(){
        if($("#SeqNo").val() == "")
        {
            CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Edit", 500, "warning");
            return false;
        }
        $("#Pol").setfocus();
    }

    stMenuBarFunc.STDel = function(){
        var selRowId = $SubGrid.jqGrid('getGridParam', 'selrow');
        var rowData = $SubGrid.getRowData(selRowId);
        if(selRowId == "" || selRowId == null)
        {
            CommonFunc.Notify("", "@Resources.Locale.L_SystemController_Delete", 500, "warning");
            return false;
        }
        localDB.actionFunc("SMSID", rowData, "DELETE");
        $SubGrid.delRowData(selRowId);
    }
    stMenuBarFunc.STSave = function(){
        var rowData = getSubData();
        rowData.InvFlow = $("#InvFlow").val();
        if(subStatus == "add" || subStatus == "copy")
        {
            localDB.actionFunc("SMSID", rowData, "INSERT");
            $SubGrid.jqGrid("addRowData", undefined, rowData, "last");
        }
        else if(subStatus == "edit")
        {
            var selRowId = $SubGrid.jqGrid('getGridParam', 'selrow');
            $SubGrid.jqGrid("setRowData", selRowId, rowData);
            localDB.actionFunc("SMSID", rowData, "UPDATE");
        }
        
        //setSTdisabled(true);
    };
    _initSubMenubar(stMenuBarFunc);
}

function _initGenGrid()
{
    var colModel1 = [
        { name: 'UId', title: 'uid', index: 'UId', sorttype: 'string', editable: false, hidden: true },
        //{ name: 'SeqNo', title: 'SeqNo', index: 'SeqNo', sorttype: 'int', editable: false, width: 80, hidden: false },
        { name: 'InvFlow', title: '@Resources.Locale.L_BSTQuery_InvFlow', index: 'InvFlow', width: 250, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Pol', title: '@Resources.Locale.L_BSTSetup_Pol', index: 'Pol', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Pod', title: '@Resources.Locale.L_BSTSetup_Pod', index: 'Pod', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Dest', title: '@Resources.Locale.L_BSTSetup_Dest', index: 'Dest', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CneeCd', title: '@Resources.Locale.L_BSTSetup_CneeCd', index: 'CneeCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CneeNm', title: '@Resources.Locale.L_BSTSetup_CneeNm', index: 'CneeNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShprCd', title: '@Resources.Locale.L_BSTSetup_ShprCd', index: 'ShprCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShprNm', title: '@Resources.Locale.L_BSTSetup_ShprNm', index: 'ShprNm', width: 200, align: 'left', sorttype: 'string', hidden: false },
        //{ name: 'ShippingMode', title: '@Resources.Locale.L_BSTSetup_ShippingMode', index: 'ShippingMode', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'TranMode', title: '@Resources.Locale.L_BSTSetup_TranMode', index: 'TranMode', width: 70, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FreightTerm', title: '@Resources.Locale.L_BSTSetup_FreightTerm', index: 'FreightTerm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify1', title: '@Resources.Locale.L_BSTSetup_Notify 1', index: 'Notify1', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify1Nm', title: '@Resources.Locale.L_BSTSetup_NotifyNm 1', index: 'Notify1Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify2', title: '@Resources.Locale.L_BSTSetup_Notify 2', index: 'Notify2', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify2Nm', title: '@Resources.Locale.L_BSTSetup_NotifyNm 2', index: 'Notify2Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify3', title: '@Resources.Locale.L_BSTSetup_Notify 3', index: 'Notify3', width: 90, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Notify3Nm', title: '@Resources.Locale.L_BSTSetup_NotifyNm 3', index: 'Notify3Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder1', title: '@Resources.Locale.L_BSTSetup_Forwarder 1', index: 'Forwarder1', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder1Nm', title: '@Resources.Locale.L_BSTSetup_ForwarderNm 1', index: 'Forwarder1Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FwdRmk1', title: '@Resources.Locale.L_BSTSetup_FwdRmk 1', index: 'FwdRmk1', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder2', title: '@Resources.Locale.L_BSTSetup_Forwarder 2', index: 'Forwarder2', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder2Nm', title: '@Resources.Locale.L_BSTSetup_ForwarderNm 2', index: 'Forwarder2Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FwdRmk2', title: '@Resources.Locale.L_BSTSetup_FwdRmk 2', index: 'FwdRmk2', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder3', title: '@Resources.Locale.L_BSTSetup_Forwarder 3', index: 'Forwarder3', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder3Nm', title: '@Resources.Locale.L_BSTSetup_ForwarderNm 3', index: 'Forwarder3Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FwdRmk3', title: '@Resources.Locale.L_BSTSetup_FwdRmk 3', index: 'FwdRmk3', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder4', title: '@Resources.Locale.L_BSTSetup_Forwarder 4', index: 'Forwarder4', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder4Nm', title: '@Resources.Locale.L_BSTSetup_ForwarderNm 4', index: 'Forwarder4Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FwdRmk4', title: '@Resources.Locale.L_BSTSetup_FwdRmk 4', index: 'FwdRmk4', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder5', title: '@Resources.Locale.L_BSTSetup_Forwarder 5', index: 'Forwarder5', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Forwarder5Nm', title: '@Resources.Locale.L_BSTSetup_ForwarderNm 5', index: 'Forwarder5Nm', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'FwdRmk5', title: '@Resources.Locale.L_BSTSetup_FwdRmk 5', index: 'FwdRmk5', width: 120, align: 'left', sorttype: 'string', hidden: false },  
        { name: 'DtCd', title: '@Resources.Locale.L_BSTDataSetup_DtCd', index: 'DtCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DtNm', title: '@Resources.Locale.L_BSTDataSetup_Nm', index: 'DtNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CcCd', title: '@Resources.Locale.L_BSTDataSetup_CcCd', index: 'CcCd', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'CcNm', title: '@Resources.Locale.L_BSTDataSetup_Nm', index: 'CcNm', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'ShippingMark', title: '@Resources.Locale.L_BSTSetup_ShippingMark', index: 'ShippingMark', width: 200, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BillType', title: '@Resources.Locale.L_BSTSetup_BillType', index: 'BillType', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Combine', title: '@Resources.Locale.L_BSTSetup_Combine', index: 'Combine', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Lc', title: '@Resources.Locale.L_BSTSetup_Lc', index: 'Lc', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'Commodity', title: '@Resources.Locale.L_BSTSetup_Commodity', index: 'Commodity', width: 150, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BlRequest', title: '@Resources.Locale.L_BSTSetup_BlRequest', index: 'BlRequest', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DocReq1', title: '@Resources.Locale.L_BSTSetup_DocReq 1', index: 'DocReq1', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MailGroup1', title: '@Resources.Locale.L_BSTSetup_MailGroup 1', index: 'MailGroup1', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DocReq2', title: '@Resources.Locale.L_BSTSetup_DocReq 2', index: 'DocReq2', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MailGroup2', title: '@Resources.Locale.L_BSTSetup_MailGroup 2', index: 'MailGroup2', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DocReq3', title: '@Resources.Locale.L_BSTSetup_DocReq 3', index: 'DocReq3', width: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MailGroup3', title: '@Resources.Locale.L_BSTSetup_MailGroup 3', index: 'MailGroup3', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DocReq4', title: '@Resources.Locale.L_BSTSetup_DocReq 4', index: 'DocReq2', widt4: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MailGroup4', title: '@Resources.Locale.L_BSTSetup_MailGroup 4', index: 'MailGroup4', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'DocReq5', title: '@Resources.Locale.L_BSTSetup_DocReq 5', index: 'DocReq2', widt5: 100, align: 'left', sorttype: 'string', hidden: false },
        { name: 'MailGroup5', title: '@Resources.Locale.L_BSTSetup_MailGroup 5', index: 'MailGroup5', width: 120, align: 'left', sorttype: 'string', hidden: false },
        { name: 'InvIns', title: '@Resources.Locale.L_BSTSetup_InvIns', index: 'InvIns', width: 250, align: 'left', sorttype: 'string', hidden: false },
        { name: 'PkgIns', title: '@Resources.Locale.L_BSTSetup_PacIns', index: 'PkgIns', width: 250, align: 'left', sorttype: 'string', hidden: false },
        { name: 'BlIns', title: '@Resources.Locale.L_BSTSetup_BlIns', index: 'BlIns', width: 250, align: 'left', sorttype: 'string', hidden: false }
    ];
    new genGrid(
        $SubGrid,
        {
            datatype: "local",
            loadonce:true,
            colModel: colModel1,
            caption: "@Resources.Locale.L_BSTSetup_caption",
            height: gridHeight,
            rows: 9999,
            refresh: false,
            cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            exportexcel: false,
            toppager:false,
            delKey: ["UId"],
            dblClickFunc: function(map){
                var mainUid = $("#UId").val();
                if(!mainUid)
                {
                    CommonFunc.Notify("", "@Resources.Locale.L_SystemController_SaveMain", 500, "warning");
                    return;
                }

                var UId = map.UId;
                if (!UId) {
                    CommonFunc.Notify("", "@Resources.Locale.L_TKBLQuery_Select", 500, "warning");
                    return;
                }
                top.topManager.openPage({
                    href: rootPath + "System/BSTDataSetup?UFid=" + mainUid + "&UId=" + UId,
                    title: '@Resources.Locale.L_SystemController_CusDS',
                    id: 'BSTDataSetup'
                });
            }
            /*onSelectRowFunc: function (map) {
                var rowData = map;
                $("#subFormData").setFormByJson(map);

                if(mainStatus == "add" || mainStatus == "edit" || mainStatus == "copy")
                {
                    stMenuBarFunc.Disabled(["STSave", "STCancel"]);
                    stMenuBarFunc.Enabled(["STAdd", "STEdit", "STCopy", "STDel"]);
                }
            }*/
        }
    );
    $(".ui-jqgrid-titlebar").hide();

}

function _initLookup()
{
    //Customer Lookup
    setSmptyData("CustCdLookup", "CustCd", "CustNm", "");

    //Buyer1 Lookup
    setSmptyData("Buyer1Lookup", "Buyer1", "Buyer1Nm", "");

    //Buyer2 Lookup
    setSmptyData("Buyer2Lookup", "Buyer2", "Buyer2Nm", "");

    //Buyer3 Lookup
    setSmptyData("Buyer3Lookup", "Buyer3", "Buyer3Nm", "");

    //Buyer4 Lookup
    setSmptyData("Buyer4Lookup", "Buyer4", "Buyer4Nm", "");

    //Buyer5 Lookup
    setSmptyData("Buyer5Lookup", "Buyer5", "Buyer5Nm", "");

    //Location Lookup
    //setSmptyData("LocationLookup", "Location", "LocationNm", "LC");

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

    //Seller Lookup
    setSmptyData("SellerLookup", "Seller", "SellerNm", "");

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


    /*子表放大鏡*/
    //POL Lookup
    setCityData("PolLookup", "Pol", "PolNm", "");
    //POD Lookup
    setCityData("PodLookup", "Pod", "PodNm", "");
    //Dest Lookup
    setCityData("DestLookup", "Dest", "DestNm", "");
    //Tran Mode Lookup
    setBscData("TranModeLookup", "TranMode", "", "TNT");
    //Shipper Lookup
    setSmptyData("ShprCdLookup", "ShprCd", "ShprNm", "");
    //Consignee Lookup
    setSmptyData("CneeCdLookup", "CneeCd", "CneeNm", "CN");
    //Notify1 Lookup
    setSmptyData("Notify1Lookup", "Notify1", "Notify1Nm", "");
    //Notify2 Lookup
    setSmptyData("Notify2Lookup", "Notify2", "Notify2Nm", "");
    //Notify3 Lookup
    setSmptyData("Notify3Lookup", "Notify3", "Notify3Nm", "");
    //Notify1 Lookup
    setSmptyData("Notify4Lookup", "Notify4", "Notify4Nm", "");
    //TRUCKER Lookup
    setSmptyData("DtCdLookup", "DtCd", "DtNm", "DT");
    //CC Lookup
    setSmptyData("CcCdLookup", "CcCd", "CcNm", "CC");
    //Forwarder1 Lookup
    setSmptyData("Forwarder1Lookup", "Forwarder1", "Forwarder1Nm", "");
    //Forwarder2 Lookup
    setSmptyData("Forwarder2Lookup", "Forwarder2", "Forwarder2Nm", "");
    //Forwarder3 Lookup
    setSmptyData("Forwarder3Lookup", "Forwarder3", "Forwarder3Nm", "");
    //Forwarder4 Lookup
    setSmptyData("Forwarder4Lookup", "Forwarder4", "Forwarder4Nm", "");
    //Forwarder5 Lookup
    setSmptyData("Forwarder5Lookup", "Forwarder5", "Forwarder5Nm", "");
    //DocReq1 Lookup
    setBscData("DocReq1Lookup", "DocReq1", "", "TDRQ");
    //DocReq2 Lookup
    setBscData("DocReq2Lookup", "DocReq2", "", "TDRQ");
    //DocReq3 Lookup
    setBscData("DocReq3Lookup", "DocReq3", "", "TDRQ");
    //DocReq4 Lookup
    setBscData("DocReq4Lookup", "DocReq4", "", "TDRQ");
    //DocReq5 Lookup
    setBscData("DocReq5Lookup", "DocReq5", "", "TDRQ");
    
}

function setSmptyData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
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

function setBscData(lookUp, Cd, Nm, pType)
{
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetBscodeDataForLookup";
    options.registerBtn = $("#"+lookUp);
    options.focusItem = $("#" + Cd);
    options.baseCondition = " GROUP_ID='"+groupId+"' AND CD_TYPE='"+pType+"'";
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
        localDB.createTable("SMSID", "UId", []);
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
            var inData = jQuery.parseJSON(result.subData1.Content);
            setFieldValue(maindata.rows);

            localDB.createTable("SMSID", "UId", inData.rows); //創建local DB

            $SubGrid.jqGrid("setGridParam", {
                datatype: 'local',
                data: inData.rows
            }).trigger("reloadGrid");

            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave", "MBCancel"]);
            MenuBarFuncArr.Enabled(["MBDel", "MBEdit", "MBCopy", "MBEdoc", "MBApprove", "MBCHM", "MBPomc", "MBPrint", "MBMoney"]);
            
            CommonFunc.ToogleLoading(false);
        }
    });
}

function getSubData()
{
    var rowData = $("#subFormData").serializeObject();

    return rowData;
}