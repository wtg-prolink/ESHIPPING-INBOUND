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

function initLoadData(PortCd, CntryCd,Cmp) {


    if (!PortCd || !CntryCd)
        return;
    var param = "sopt_PortCd=eq&PortCd=" + PortCd;
    param += "&sopt_CntryCd=eq&CntryCd=" + CntryCd;
    param += "&sopt_Cmp=eq&Cmp=" + Cmp;
    //将获取的数据作为条件进行reload数据
    $.ajax({
        async: true,
        url: rootPath + "System/TruckPortSetupQuery",
        type: 'POST',
        data: {
            model: "CitySetupModel",
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
        url: rootPath + "System/BSADDRQuery",
        type: 'POST',
        data: {
            model: "BSADDRModel",
            sidx: 'CreateDate',
            'conditions': encodeURI(param),
            page: 1,
            rows: 100
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
            url: rootPath + "System/TruckPortSetupUpdate",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)), cmp: $("#Cmp").val(),
                portCd: $("#PortCd").val(),
                cntryCd: $("#CntryCd").val(),
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
        $("#ModifyBy").val("");
        $("#ModifyDate").val("");
        //var dataRow, addData = [];
        //var rowIds = $MainGrid.getDataIDs();
        //for (var i = 0; i < rowIds.length; i++) {
        //    var rowDatas = $MainGrid.jqGrid('getRowData', rowIds[i]);
        //    dataRow = {
        //        CntryCd: "",
        //        PortCd: "",
        //        SeqNo: rowDatas.SeqNo,
        //        AddrCode: rowDatas.AddrCode,
        //        Addr: rowDatas.Addr,
        //        OuterFlag: rowDatas.OuterFlag,
        //        FinalWh: rowDatas.FinalWh,
        //        CustomerCodel: rowDatas.CustomerCode,
        //    };
        //    addData.push(dataRow);
        //}
        //$("#MainGrid").jqGrid("clearGridData");
        //for (var i = 0; i < addData.length; i++) {
        //   $("#MainGrid").jqGrid("addRowData", undefined, addData[i], "last");
        //}
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
            url: rootPath + "System/TruckPortSetupUpdate",
            type: 'POST',
            data: {
                "changedData": encodeURIComponent(JSON.stringify(changeData)),
                portCd: $("#PortCd").val(),
                cntryCd: $("#CntryCd").val(),
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

    registBtnLookup($("#ShareToLookup"), {
        isMutiSel: true,
        item: "#ShareTo", url: rootPath + LookUpConfig.GetCmpUrl, config: LookUpConfig.MutiLocationLookup, param: "", selectRowFn: function (map) {
            $("#ShareTo").val(map.Cmp);
        }
    }, {
            baseCondition: " GROUP_ID='" + groupId + "' AND TYPE='1'",
            responseMethod: function (data) {
                console.log(data);
                var str = "";
                $.each(data, function (index, val) {
                    str = str + data[index].Cmp + ";";
                });
                $("#ShareTo").val(str);
            }
        });

    //城市代码放大镜
    /*Cityoptions = {};
    Cityoptions.gridUrl = rootPath + "Common/GetBscityData";
    Cityoptions.registerBtn = $("#PortCdLookup");
    Cityoptions.isMutiSel = true;
    Cityoptions.param = '';
    Cityoptions.baseCondition = "GROUP_ID='" + groupId + "'";
    Cityoptions.gridFunc = function (map) {
        var cd = map.PortCd,
           cn = map.PortNm;
        $("#PortCd").val(cd);
        $("#PortNm").val(cn);
    }
    Cityoptions.responseMethod = function () { }
    Cityoptions.lookUpConfig = LookUpConfig.CityPortLookup;
    initLookUp(Cityoptions);*/


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
    
    agOpt = {};
    agOpt.gridUrl = rootPath + "/Common/GetPartyNoData";
    agOpt.gridReturnFunc = function (map) {
        var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
        //params: string grid id,string rowid,string cell name,string value,string 'lookup' or null
        setGridVal($("#MainGrid"), selRowId, "CustomerCode", map.PartyNo, null);
        return map.PartyNo;
    };
    agOpt.lookUpConfig = LookUpConfig.PartyNoLookup;
    agOpt.gridId = "MainGrid";
    agOpt.param = "";
    agOpt.autoCompUrl = "";
    agOpt.autoCompDt = "dt=smpty&GROUP_ID=" + groupId + "&PARTY_NO%";
    agOpt.autoCompParams = "PARTY_NAME&PARTY_NO=showValue,STATE,ZIP,PARTY_NO,PARTY_NAME,PARTY_NAME2,PARTY_NAME3,PARTY_NAME4,PARTY_MAIL,PART_ADDR1,PART_ADDR2,PART_ADDR3,PART_ADDR4,PART_ADDR5,PARTY_FAX,PARTY_ATTN,PARTY_TEL,PARTY_TYPE,CNTY,CNTY_NM,CITY,CITY_NM,TAX_NO";
    agOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.PARTY_NO);
        //notice: auto comp not using get selRow method,because when mouse click out maybe get warm rowid
        setGridVal($("#MainGrid"), rowid, "CustomerCode", ui.item.returnValue.PARTY_NO, null);
    }

    WHOpt = {};
    WHOpt.gridUrl = rootPath + "TPVCommon/GetSmwhForLookup";
    WHOpt.gridReturnFunc = function (map) {
        var selRowId = $("#MainGrid").jqGrid('getGridParam', 'selrow');
        setGridVal($("#MainGrid"), selRowId, "Plant", map.WsCd, null);
        return map.WsCd;
    };
    WHOpt.lookUpConfig = LookUpConfig.SMWHLookup;
    WHOpt.gridId = "MainGrid";
    WHOpt.param = "";
    WHOpt.baseConditionFunc = function () {
        var Cmp = $("#Cmp").val();

        if (Cmp != "") {
            return " CMP='" + Cmp + "'";
        }
        else {
            return "";
        }
    }
    WHOpt.autoCompUrl = "";
    WHOpt.autoCompDt = "dt=smwh&GROUP_ID=" + groupId + "&WS_CD=";
    WHOpt.autoCompParams = "WS_CD&WS_CD=showValue,WS_CD,WS_NM";
    WHOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.WS_CD);
        setGridVal($("#MainGrid"), rowid, "Plant", ui.item.returnValue.WS_CD, null);
    }

    var ColModelLang = [
        '@Resources.Locale.L_TruckPort_CntryCd',
    	'@Resources.Locale.L_TruckPort_PortId',
    	'@Resources.Locale.L_TruckPort_SeqNo',
        '@Resources.Locale.L_TruckPort_AddrCode',
        '@Resources.Locale.L_TruckPort_Addr',
        '@Resources.Locale.L_TruckPort_OuterFlag',
        '@Resources.Locale.L_TruckPort_FinalWh',
        '@Resources.Locale.L_TruckPort_CustomerCode',
        'Plant',
        'Share From'
    ];

    var ColModel = [
		{ name: 'CntryCd', title: 'null', index: 'CntryCd', sorttype: 'string', editable: false, hidden: true },
		{ name: 'PortCd', title: 'null', index: 'PortCd', sorttype: 'string', editable: false, hidden: true },
		{ name: 'SeqNo', title: 'null', index: 'SeqNo', sorttype: 'string', width: 100, hidden: true, editable: false },
		{ name: 'AddrCode', title: 'null', index: 'AddrCode', sorttype: 'string', width: 100, hidden: false, editable: false },
		{ name: 'Addr', title: 'null', index: 'Addr', sorttype: 'string', width: 250, hidden: false, editable: true  },
		{ name: 'OuterFlag', title: 'null', index: 'OuterFlag', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Y:Yes;N:No', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
        { name: 'FinalWh', title: 'null', index: 'FinalWh', sorttype: 'string', width: 100, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: 'Final:Final;Temp:Temp', dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
        { name: 'CustomerCode', title: 'Customer Code', index: 'CustomerCode', width: 100, align: 'left', editable: true, hidden: false, editoptions: gridLookup(agOpt), edittype: 'custom' },
        { name: 'Plant', title: 'Plant', index: 'Plant', width: 100, align: 'left', editable: true, hidden: false, editoptions: gridLookup(WHOpt), edittype: 'custom' },
        { name: 'ShareFrom', title: 'null', index: 'ShareFrom', sorttype: 'string', width: 100, hidden: false, editable: false },
    ];

    for (var i = 0; i < ColModel.length; i++) {
        ColModel[i]['title'] = ColModelLang[i];
    }
   
    
    $MainGrid = $("#MainGrid");
    _dm.addDs("MainGrid", [], ["CntryCd", "PortCd", "AddrCode","ShareFrom"], $MainGrid[0]);
    new genGrid(
    	$MainGrid,
    	{
    	    data: [],
    	    datatype: "local",
    	    //url: rootPath + "BSCODE/bscodeSortQuery",
    	    loadonce: true,
    	    colModel: ColModel,
            cellEdit: true,//禁用grid编辑功能
            isModel:true,
    	    caption: '@Resources.Locale.L_TruckPort_CommonAddress',
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    sortable: false,
    	    rownumWidth: 50,
            delKey: ["CntryCd", "PortCd", "AddrCode", "ShareFrom"],
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
