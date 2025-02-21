var url = ""
var groupId = getCookie("plv3.passport.groupid"),
    cmp = getCookie("plv3.passport.companyid"),
    stn = getCookie("plv3.passport.station"),
    userId = getCookie("plv3.passport.user"),
    Supplier = "",
    PartNo = "";

var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];

function initLoadData(PortCd, CntryCd, Factory, ShipTo, Cmp) {


    if (!PortCd || !CntryCd || !Factory || !ShipTo)
        return;
    var param = "sopt_PortCd=eq&PortCd=" + PortCd;
    param += "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
    param += "&sopt_Factory=eq&Factory=" + Factory;
    param += "&sopt_ShipTo=eq&ShipTo=" + ShipTo;
    param += "&sopt_Cmp=eq&Cmp=" + Cmp;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "System/DestinationSetupQuery",
        type: 'POST',
        data: {
            model: "BsDestModel",
            sidx: 'CreateDate',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            _dataSource = result.rows;
            setFieldValue(result.rows);
            //var maindata = result.main;
            //console.log(maindata);
            //setFieldValue(maindata);

            //绑定Grid
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove"]);
            gridEditableCtrl({ editable: false, gridId: "MainGrid" });
        }
    });

    $.ajax({
        async: true,
        url: rootPath + "System/DestAddrQuery",
        type: 'POST',
        data: {
            model: "DestAddrModel",
            sidx: 'CreateDate',
            'conditions': encodeURI(param),
            page: 1,
            rows: 20
        },
        dataType: "json",
        "complete": function (xmlHttpRequest, successMsg) {
            if (successMsg != "success") return null;
        },
        "error": function (xmlHttpRequest, errMsg) {
        },
        success: function (result) {
            //console.log(result);
            //setFieldValue(result.rows);
            var subTable = $.parseJSON(result.subTable.Content);

            console.log(subTable);
            var $grid = $("#MainGrid");
            //_oldDeatiArray[cdType] = subTable.rows;
            if (_dm.getDs("MainGrid") == null || _dm.getDs("MainGrid") == undefined) {
                _dm.addDs("MainGrid", subTable.rows, ["CntryCd","PortCd","AddrCode"], $grid[0]);
            } else {
                _dm.getDs("MainGrid").setData(subTable.rows);
            }
            //gridEditableCtrl({ editable: false, gridId: "MainGrid" });

            //绑定Grid
            setdisabled(true);
            setToolBtnDisabled(true);
            MenuBarFuncArr.Disabled(["MBSave"]);
            MenuBarFuncArr.Enabled(["MBAdd", "MBCopy", "MBDel", "MBEdit", "MBApprove"]);
        }
    });
 
}

jQuery(document).ready(function ($) {

    var docHeight = $(document).height();
    gridHeight = docHeight - 130;

    url = rootPath + "System/TruckPortSetupInquiryData";
   

    MenuBarFuncArr.MBCancel = function () {
        var NowSupplier = $("#PortCd").val();
        var NowPartNo = $("#CntryCd").val();
        var postdata = { "conditions": "sopt_1=ne&1=1" };
        if (groupId && NowSupplier && NowPartNo && stn) {
            postdata = { "conditions": "sopt_GroupId=eq&GroupId=" + groupId + "&sopt_PortCd=eq&PortCd=" + NowSupplier + "&sopt_CntryCd=eq&CntryCd=" + NowPartNo + "&sopt_Stn=eq&Stn=" + stn };
        }
        MenuBarFuncArr.Enabled(["MBEdit"]);
        gridEditableCtrl({ editable: false, gridId: "MainGrid" });
    }

    MenuBarFuncArr.MBDel = function (dtd) {
        var changeData = getAllKeyValue();
        $.ajax({
            async: true,
            url: rootPath + "System/DestinationSetupUpdate",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), cmp: $("#Cmp").val(),
                portCd: $("#PortCd").val(),
                cntryCd: $("#CntryCd").val(),
                factory:$("#Factory").val(),
                shipTo: $("#ShipTo").val(),
                ShareTo: $("#ShareTo").val(),
                autoReturnData: false
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
                if (successMsg != "success")
                    return null;
                else
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                setdisabled(false);
                setToolBtnDisabled(false);
            },
            "error": function (xmlHttpRequest, errMsg) {
            },
            success: function (result) {
                if (result.message !== "success") {
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF", 500, "warning");
                    return;
                }
                //成功后将页面的数据移除，并设置页面不可编辑
                //setFieldValue();
                //setdisabled(true);
                //setToolBtnDisabled(false);

                setFieldValue(result.mainData);

                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_DelS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                MenuBarFuncArr.Disabled(["MBEdit", "MBDel", "MBCopy"]);
                location.reload();
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    MenuBarFuncArr.MBAdd = function (thisItem) {
        $("#Cmp").val(cmp);
        $("#Stn").val(stn);
        $("#CreateBy").val(userId);
        $("#MainGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }

    MenuBarFuncArr.MBCopy = function (thisItem) {
        $("#PortCd").val("");
        $("#CntryCd").val("");
        $("#Factory").val("");
        $("#ShipTo").val("");
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
        $("#MainGrid").jqGrid("clearGridData");
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }

    MenuBarFuncArr.MBEdit = function () {        
        gridEditableCtrl({ editable: true, gridId: "MainGrid" });
    }

    
    //notice MBSave一定要傳入dtd
    MenuBarFuncArr.MBSave = function (dtd) {
        if (checkNoAllowNullFields() == false)
            return false;
        var changeData = {};
        changeData = getChangeValue();
        var containerArray = $('#MainGrid').jqGrid('getGridParam', "arrangeGrid")();
        changeData["st"] = containerArray;
        console.log(changeData);
        $.ajax({
            async: true,
            url: rootPath + "System/DestinationSetupUpdate",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)),
                portCd: $("#PortCd").val(),
                cntryCd: $("#CntryCd").val(),
                factory: $("#Factory").val(),
                shipTo: $("#ShipTo").val(),
                cmp: $("#Cmp").val(),
                ShareTo: $("#ShareTo").val(),
                autoReturnData: true
            },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            error: function (xmlHttpRequest, errMsg) {
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message != "success") {
                    //notice ajax warning 一定要放入下面三行
                    CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveF" +" "+ result.message, 500, "warning");
                    MenuBarFuncArr.SaveResult = false;
                    dtd.resolve();                    
                    return;
                }
                setFieldValue(result.mainData);
                setdisabled(true);
                setToolBtnDisabled(true);
                gridEditableCtrl({ editable: false, gridId: "MainGrid" });
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
            }
        });
        return dtd.promise();
    }

    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.DelMenu(["MBApply", "MBApprove", "MBInvalid", "MBErrMsg", "MBEdoc", "MBSearch"]);

    setdisabled(true);
    setToolBtnDisabled(true);

    //国家代码放大镜
    Cntyoptions = {};
    Cntyoptions.gridUrl = rootPath + "Common/GetCntryCdData";
    Cntyoptions.registerBtn = $("#CntryCdLookup");
    Cntyoptions.isMutiSel = true;
    Cntyoptions.param = '';
    Cntyoptions.baseCondition = "GROUP_ID='" + groupId + "'";
    Cntyoptions.gridFunc = function (map) {
        var cd = map.CntryCd,
           cn = map.CntryNm;
        $("#CntryCd").val(cd);
        $("#CntryNm").val(cn);
    }
    Cntyoptions.responseMethod = function () { }
    Cntyoptions.lookUpConfig = LookUpConfig.CntyCdLookup;
    initLookUp(Cntyoptions);
    CommonFunc.AutoComplete("#CntryCd", 2, "", "dt=country&GROUP_ID=" + groupId + "&CNTRY_CD=", "CNTRY_CD=showValue,CNTRY_CD,CNTRY_NM", function (event, ui) {
        $("input[name='CntryNm']").val(ui.item.returnValue.CNTRY_NM);
        $(this).val(ui.item.returnValue.CNTRY_CD);
        return false;
    });

    setSmptyData("ShipToLookup", "ShipTo", "ShipToNm", "WE");

    //State Lookup
    var options = {};
    options.gridUrl = rootPath + "TPVCommon/GetStateDataForLookup";
    options.registerBtn = $("#StateLookup");
    options.focusItem = $("#State");
    options.param = "";
    options.isMutiSel = true;
    options.baseConditionFunc = function () {
        var CntryCd = $("#CntryCd").val();

        if (CntryCd != "") {
            return " CNTRY_CD='" + CntryCd + "'";
        }
        else {
            return "";
        }
    }
    options.gridFunc = function (map) {
        $("#State").val(map.StateCd);
        $("#Region").val(map.RegionCd);
    }

    options.lookUpConfig = LookUpConfig.StateLookup;
    initLookUp(options);
    CommonFunc.AutoComplete("#State", 1, "", "dt=state&GROUP_ID=" + groupId + "&STATE_CD=", "STATE_CD=showValue,STATE_CD,STATE_NM,REGION_CD", function (event, ui) {
        var map = ui.item.returnValue;
        $(this).val(ui.item.returnValue.STATE_CD);
        $("#Region").val(ui.item.returnValue.REGION_CD);
        return false;
    }, function () {
        var CntryCd = $("#CntryCd").val();

        if (CntryCd != "") {
            return " CNTRY_CD=" + CntryCd;
        }
        else {
            return "";
        }
    });

    commonSetBscData("RegionLookup", "Region", "", "TRGN", null);
    commonBscAuto("RegionLookup", "Region", "", "TRGN", null);
    genMainGrid();

});

function genMainGrid() {

    registBtnLookup($("#FactoryLookup"), {
        item: '#Factory', url: rootPath + "Common/GetTcmpCode", config: LookUpConfig.BSCodeLookup, param:"", selectRowFn: function (map) {
            $("#Factory").val(map.Cd);
            $("#FactoryNm").val(map.CdDescp);
        }
    }, undefined, LookUpConfig.GetCodeTypeAuto(groupId, "TCMP", undefined, function ($grid, rd, elem) {
        $(elem).val(rd.CD);
        $("#FactoryNm").val(rd.CD_DESCP);
    }));


    var ColModelLang = [
        '@Resources.Locale.L_TruckPort_CntryCd',
    	'@Resources.Locale.L_TruckPort_PortId',
    	'@Resources.Locale.L_TruckPort_SeqNo',
        '@Resources.Locale.L_TruckPort_AddrCode',
        '@Resources.Locale.L_TruckPort_Addr',
        'WH Code',
        'WH Name',
        '@Resources.Locale.L_TruckPort_OuterFlag',
        '@Resources.Locale.L_TruckPort_FinalWh',
    ];

    var ColModel = [
		{ name: 'CntryCd', title: 'null', index: 'CntryCd', sorttype: 'string', editable: false, hidden: true },
		{ name: 'PortCd', title: 'null', index: 'PortCd', sorttype: 'string', editable: false, hidden: true },
		{ name: 'SeqNo', title: 'null', index: 'SeqNo', sorttype: 'string', width: 100, hidden: true, editable: false },
		{ name: 'AddrCode', title: 'null', index: 'AddrCode', sorttype: 'string', width: 100, hidden: false, editable: false },
        { name: 'Addr', title: 'null', index: 'Addr', sorttype: 'string', width: 250, hidden: false, editable: true },
        { name: 'WhCode', title: 'null', index: 'WhCode', sorttype: 'string', width: 100, hidden: false, editable: true },
        { name: 'WhName', title: 'null', index: 'WhName', sorttype: 'string', width: 250, hidden: false, editable: true },
		{ name: 'OuterFlag', title: 'null', index: 'OuterFlag', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
        { name: 'FinalWh', title: 'null', index: 'FinalWh', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Final:Final;Temp:Temp', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
    ];

    for (var i = 0; i < ColModel.length; i++) {
        ColModel[i]['title'] = ColModelLang[i];
    }
   
    
    $MainGrid = $("#MainGrid");
    _dm.addDs("MainGrid", [], ["CntryCd", "PortCd", "AddrCode","WhCode"], $MainGrid[0]);
    new genGrid(
    	$MainGrid,
    	{
    	    data: [],
    	    datatype: "local",
    	    loadonce: true,
    	    colModel: ColModel,
            cellEdit: true,//禁用grid编辑功能
            isModel: true,
    	    caption: '@Resources.Locale.L_TruckPort_CommonAddress',
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    sortable: false,
    	    rownumWidth: 50,
            delKey: ["CntryCd", "PortCd", "AddrCode","WhCode"],
    	    ds: _dm.getDs("MainGrid"),
    	    beforeSelectRowFunc: function (rowid) {
    	        ////main key 修改時不允與修改
    	        //if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	        //    $("#BsCodeGrid").setColProp('Cd', { editable: true });
    	        //} else {
    	        //    $("#BsCodeGrid").setColProp('Cd', { editable: false });
    	        //}
    	    },
    	    onAddRowFunc: function (rowid) {
    	        var maxSeqNo = $MainGrid.jqGrid("getCol", "SeqNo", false, "max");
    	        if (typeof maxSeqNo === "undefined")
    	            maxSeqNo = 0;
    	        var PortCd = $("#PortCd").val();
    	        var CntryCd = $("#CntryCd").val();
    	        maxSeqNo += 1
    	        $MainGrid.jqGrid('setCell', rowid, "SeqNo", maxSeqNo);
    	        var str = "" + maxSeqNo;
    	        var pad = "000";
    	        var no = pad.substring(0, pad.length - str.length) + str;
    	        var AddrCode = PortCd + no
    	        $MainGrid.jqGrid('setCell', rowid, "AddrCode", AddrCode);
    	        $MainGrid.jqGrid('setCell', rowid, "PortCd", PortCd);
    	        $MainGrid.jqGrid('setCell', rowid, "CntryCd", CntryCd);
    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key

    	        $("#MainGrid").setColProp('AddrCode', { editable: false });
    	        //var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
    	        //var cdType = $("#MainGrid").jqGrid('getCell', selRowId, 'cdType');
    	        //if (cdType != null && cdType != "") {
    	        //    mainKeyValue.cdType = cdType;

    	        //} else {
    	        //    //alert("主档代碼有误，无法建立代碼");
    	        //    CommonFunc.Notify("", "@Resources.Locale.L_ErrRelationSetup_Error", 500, "danger");
    	        //    return false;
    	        //}
    	    },
    	    onSortCol: function (index, iCol, sortorder) {

    	    }
    	}
    );

}

function registBtnLookup(item, op, op1, op2) {
    //{url:"",config:""}
    var options = {};
    options.gridUrl = op.url;
    options.registerBtn = item;
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
            });
    }
}

function setSmptyData(lookUp, Cd, Nm, pType) {
    //SMPTY放大鏡
    options = {};
    options.gridUrl = rootPath + "TPVCommon/GetSmptyDataForLookup";
    options.registerBtn = $("#" + lookUp);
    options.focusItem = $("#" + Cd);
    options.param = "";
    options.baseCondition = " PARTY_TYPE LIKE '%" + pType + "%'";
    options.isMutiSel = true;
    options.gridFunc = function (map) {
        $("#" + Cd).val(map.PartyNo);

        if (Nm != "")
            $("#" + Nm).val(map.PartyName);
    }

    options.lookUpConfig = LookUpConfig.SmptyLookup;
    initLookUp(options);

    CommonFunc.AutoComplete("#" + Cd, 1, "", "dt=smpty&GROUP_ID=" + groupId + "&PARTY_TYPE~" + pType + "&PARTY_NO=", "PARTY_NO=showValue,PARTY_NO,PARTY_NAME", function (event, ui) {
        $(this).val(ui.item.returnValue.PARTY_NO);

        if (Nm != "")
            $("#" + Nm).val(ui.item.returnValue.PARTY_NAME);
        return false;
    });
}
