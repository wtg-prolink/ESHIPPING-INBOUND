var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var gridSetting = {};

jQuery(document).ready(function ($) {
    var opt = {};
    getSelectOptions();
    var editable = false;

    // init country grid lookup
    opt.gridUrl = rootPath + "Common/GetCountryData";
    opt.gridReturnFunc = function (map) {

        var selRowId = $("#GoodsCInfoGrid").jqGrid('getGridParam', 'selrow');
        $("#GoodsCInfoGrid").jqGrid('setCell', selRowId, 'CntryNm', map.CntyNm);
        //$("#GoodsCInfoGrid").jqGrid("editCell", selRowId, "TaxRate", true);
        return map.CntyCd;
    };
    opt.lookUpConfig = LookUpConfig.CntryLookup;
    opt.autoCompKeyinNum = 1;
    opt.gridId = "GoodsCInfoGrid";
    opt.autoCompUrl = "";
    opt.autoCompDt = "dt=country&GROUP_ID=" + groupId + "&CNTY_CD%";
    opt.autoCompParams = "CNTY_NM&CNTY_CD=showValue,CNTY_CD,CNTY_NM";
    opt.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.CNTY_CD);
        var selRowId = $("#GoodsCInfoGrid").jqGrid('getGridParam', 'selrow');
        $("#GoodsCInfoGrid").jqGrid('setCell', selRowId, 'CntryNm', ui.item.returnValue.CNTY_NM);

        //$("#GoodsCInfoGrid").jqGrid("editCell", selRowId, 1, true);

    }

    var colModel = [
    	    { name: 'Goods', title: '@Resources.Locale.L_BaseLookup_Goods', index: 'Goods', sorttype: 'string', editable: true },
            { name: 'GroupId', title: '@Resources.Locale.L_GroupRelation_groupID', index: 'GroupId', sorttype: 'string', hidden: true, editable: false },
    	    { name: 'GoodsDescp', title: '@Resources.Locale.L_IpPart_GoodsDescp', index: 'GoodsDescp', sorttype: 'string', editable: true },
    	    { name: 'HsCode', title: 'HS Code', index: 'HsCode', sorttype: 'string', editable: true },
            { name: 'Bu', title: '@Resources.Locale.L_IpPart_Bu', index: 'Bu', width: 80, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: '@Resources.Locale.L_IpGoods_Script_185', defaultValue: 'C' } },
    	    { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', sorttype: 'string', editable: true }


    ];

    var colModel2 = [
	        { name: 'CntryCd', title: '@Resources.Locale.L_BSCSDataQuery_Cnty', index: 'CntryCd', width: 100, sorttype: 'string', edittype: 'custom', editable: true, editoptions: gridLookup(opt) },
            { name: 'Goods', title: '@Resources.Locale.L_BaseLookup_Goods', index: 'Goods', sorttype: 'string', hidden: true, editable: false },
            { name: 'GroupId', title: '@Resources.Locale.L_GroupRelation_groupID', index: 'GroupId', sorttype: 'string', hidden: true, editable: false },
            { name: 'CntryNm', title: '@Resources.Locale.L_BaseLookup_OrgCntNm', index: 'CntryNm', width: 150, sorttype: 'string', editable: false },
            { name: 'TaxRate', title: '@Resources.Locale.L_IpGoods_Scripts_358', index: 'TaxRate', width: 75, align: 'right', formatter: 'number', editable: true },
            { name: 'TaxUnit', title: '@Resources.Locale.L_BaseLookup_Unit', index: 'TaxUnit', width: 50, sorttype: 'string', editable: true, formatter: 'select', edittype: 'select', editoptions: { value: '%:%;T:T;K:K', defaultValue: '%' } },
            { name: 'Remark', title: '@Resources.Locale.L_BSCSSetup_Remark', index: 'Remark', width: 100, sorttype: 'string', editable: true }
    ];

    //init subgrid change data setting
    gridSetting.gridId = "GoodsCInfoGrid";
    gridSetting.colModel = colModel2;

    $grid = $("#containerInfoGrid");
    new genGrid(
    	$grid,
    	{
    	    datatype: "json",
    	    loadonce: true,
    	    url: rootPath + "SYSTEM/IpGoodsRequiry",
    	    colModel: colModel,
    	    caption: "@Resources.Locale.L_IpGoods_Scripts_359",
    	    height: "auto", refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
    	    //inlineEdit:true,
    	    exportexcel: false,
    	    rows: 200,
    	    delKey: ["Goods", "GroupId"],
    	    gridFunc: function (map) {

    	        //console.log(map);


    	        gridSetting.keyData = { Goods: mainKeyValue.goodsCd, GroupId: groupId };
    	        getGridChangeDataDS(gridSetting);

    	        var selRowId = $grid.jqGrid('getGridParam', 'selrow');
    	        var goodsCd = $grid.jqGrid('getGridParam', "getGridCellValueCustom")($grid, selRowId, "Goods", "");

    	        if (goodsCd != null && goodsCd != "") {
    	            //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
    	            if (_oldDeatiArray[goodsCd] != undefined || _oldDeatiArray[goodsCd] != null) {
    	                //将json设置给GoodsCInfoGrid
    	                //移除_state状态为0的数据，，因为_state的数据是删除的数据
    	                $.each(_oldDeatiArray[goodsCd], function (i, val) {
    	                    if (val._state == "0") {
    	                        _oldDeatiArray[goodsCd].splice(i, 1);
    	                    }
    	                });
    	                _dm.getDs("GoodsCInfoGrid").setData(_oldDeatiArray[goodsCd]);
    	                return;
    	            }
    	            $.ajax({
    	                async: true,
    	                url: rootPath + "SYSTEM/IpGoodscRequiry",
    	                type: 'POST',
    	                data: {
    	                    Goods: goodsCd,
    	                    GroupId: groupId,
    	                    page: 1,
    	                    rows: 200
    	                },
    	                dataType: "json",
    	                "complete": function (xmlHttpRequest, successMsg) {
    	                    if (successMsg != "success") return null;
    	                },
    	                "error": function (xmlHttpRequest, errMsg) {
    	                },
    	                success: function (result) {
    	                    var mainTable = $.parseJSON(result.mainTable.Content);
    	                    var $grid = $("#GoodsCInfoGrid");
    	                    _oldDeatiArray[goodsCd] = mainTable.rows;
    	                    if (_dm.getDs("GoodsCInfoGrid") == null || _dm.getDs("GoodsCInfoGrid") == undefined) {
    	                        _dm.addDs("GoodsCInfoGrid", mainTable.rows, ["Goods", "GroupId", "CntryCd"], $grid[0]);
    	                    } else {
    	                        _dm.getDs("GoodsCInfoGrid").setData(mainTable.rows);
    	                    }
    	                    //gridEditableCtrl({ editable: false, gridId: "GoodsCInfoGrid" });
    	                    /* if (editable) {
                                 gridEditableCtrl({ editable: true, gridId: "GoodsCInfoGrid" });
                             }*/

    	                }
    	            });

    	        }

    	    },
    	    beforeSelectRowFunc: function (rowid) {

    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#containerInfoGrid").setColProp('Goods', { editable: true });
    	        } else {
    	            $("#containerInfoGrid").setColProp('Goods', { editable: false });
    	        }

    	        //example if bu == D HS CODE 不可修改
    	        /*
    	        $grid = $("#containerInfoGrid");
    	        var bu = $grid.jqGrid('getGridParam', "getGridCellValueCustom")($grid, rowid, "Bu", "");
    	        alert(rowid);
    	        //alert(bu);
    	        if (bu != null && bu != "D") {
    	            $("#containerInfoGrid").setColProp('HsCode', { editable: true });
    	        } else {
    	            $("#containerInfoGrid").setColProp('HsCode', { editable: false });
    	        }
                */


    	    },
    	    beforeAddRowFunc: function () {
    	        //add row 時要可以編輯main key
    	        //$("#containerInfoGrid").setColProp('HsCode', { editable: true });
    	        $("#containerInfoGrid").setColProp('Goods', { editable: true });
    	    }
    	}
    );



    $GoodsInfogrid = $("#GoodsCInfoGrid");
    _dm.addDs("GoodsCInfoGrid", [], ["Goods", "GroupId", "CntryCd"], $GoodsInfogrid[0]);
    new genGrid(
    	$GoodsInfogrid,
    	{
    	    data: [],
    	    loadonce: true,
    	    rows: 200,
    	    colModel: colModel2,
    	    cellEdit: false,//禁用grid编辑功能
    	    caption: "@Resources.Locale.L_IpGoods_Scripts_360",
    	    height: "auto", refresh: true, exportexcel: false,
    	    ds: _dm.getDs("GoodsCInfoGrid"),
    	    beforeSelectRowFunc: function (rowid) {

    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#GoodsCInfoGrid").setColProp('CntryCd', { editable: true });
    	        } else {
    	            $("#GoodsCInfoGrid").setColProp('CntryCd', { editable: false });
    	        }
    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#GoodsCInfoGrid").setColProp('CntryCd', { editable: true });

    	        //當抓到的品號為空時，必須重新回主檔抓，若還是抓不到則無法新增
    	        if (mainKeyValue.goodsCd == null || mainKeyValue.goodsCd == "") {
    	            var selRowId = $("#containerInfoGrid").jqGrid('getGridParam', 'selrow');
    	            var goodsCd = $grid.jqGrid('getGridParam', "getGridCellValueCustom")($grid, selRowId, "Goods", "");
    	            if (goodsCd != null && goodsCd != "") {
    	                mainKeyValue.goodsCd = goodsCd;

    	            } else {
    	                //alert("主档品号有误，无法建立国别");
    	                CommonFunc.Notify("", "@Resources.Locale.L_IpGoods_Script_186 @Resources.Locale.L_GateSetup_Scripts_281", 500, "danger");
    	                //$("#GoodsCInfoGrid").jqGrid("clearGridData");
    	                return false;
    	            }
    	        }
    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);

        gridEditableCtrl({ editable: false, gridId: "containerInfoGrid" });
        gridEditableCtrl({ editable: false, gridId: "GoodsCInfoGrid" });
        editable = false;

    }

    MenuBarFuncArr.MBEdit = function () {
        gridEditableCtrl({ editable: true, gridId: "containerInfoGrid" });
        gridEditableCtrl({ editable: true, gridId: "GoodsCInfoGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        //获取子表的changeValue
        gridSetting.keyData = { Goods: mainKeyValue.goodsCd, GroupId: groupId };
        getGridChangeDataDS(gridSetting);

        var containerArray = $('#containerInfoGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        changeData["goodscinfo"] = _changeDatArray;
        console.log(_changeDatArray);
        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/IpGoodscUpdate",
            type: 'POST',
            data: { "changedData": encodeURIComponent(JSON.stringify(changeData)), autoReturnData: true },
            dataType: "json",
            "complete": function (xmlHttpRequest, successMsg) {
            },
            "error": function (xmlHttpRequest, errMsg) {
                alert(errMsg);
                CommonFunc.Notify("", errMsg, 500, "danger");
                MenuBarFuncArr.SaveResult = false;
                dtd.resolve();
            },
            success: function (result) {
                if (result.message == null) return;
                //alert(result.message);
                setdisabled(true);
                setToolBtnDisabled(true);
                CommonFunc.Notify("", "@Resources.Locale.L_MailFormatSetup_SaveS", 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                location.reload();
            }
        });

        return dtd.promise();
        /*
        gridEditableCtrl({ editable: false, gridId: "containerInfoGrid" });
        gridEditableCtrl({ editable: false, gridId: "GoodsCInfoGrid" });
        editable = false;
        */
    }


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

function getSelectOptions() {
    $.ajax({
        async: true,
        url: rootPath + "Common/GetSelectOptions",
        type: 'POST',
        "error": function (xmlHttpRequest, errMsg) {
            alert(errMsg);
        },
        success: function (data) {
            var deOptions = data.De,
                buOptions = data.Bu,
                luOptions = data.Lu;
        }
    });
}

