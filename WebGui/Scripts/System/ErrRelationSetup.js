var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var cmp = getCookie["plv3.passport.basecompanyid"];
var stn = getCookie["plv3.passport.basestation"];
var gridSetting = {};

jQuery(document).ready(function ($) {
    var opt = {};

    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 130;

    agOpt = {};
    agOpt.gridUrl = rootPath + "SYSTEM/GetData";
    agOpt.baseConditionFunc = function () {
        var selRowId = $("#ExpTypeGroup").jqGrid('getGridParam', 'selrow');
        var GroupId = getGridVal($("#ExpTypeGroup"), selRowId, "GroupId", null);
        return "GROUP_ID=" + GroupId + "";
    }
    agOpt.gridReturnFunc = function (map) {
        var selRowId = $("#ExpReasonGroup").jqGrid('getGridParam', 'selrow');
        //params: string grid id,string rowid,string cell name,string value,string 'lookup' or null
        //setGridVal($("#ExpReasonGroup"), selRowId, "ExpReason", map.Cd, 'lookup');
        setGridVal($("#ExpReasonGroup"), selRowId, "ExpDescp", map.CdDescp, null);        
        return map.Cd;
    };
    agOpt.lookUpConfig = LookUpConfig.BSCodeLookup;
    agOpt.gridId = "ExpReasonGroup";  
    agOpt.autoCompUrl = "";
    agOpt.autoCompDt = "dt=bsc&GROUP_ID=" + groupId + "&CD_TYPE=ASR&CD=";
    agOpt.autoCompParams = "CD_DESCP=showValue,CD,CD_DESCP";
    agOpt.autoCompGetValueFunc = function () {
        var selRowId = $("#ExpTypeGroup").jqGrid('getGridParam', 'selrow');
        var GroupId = getGridVal($("#ExpTypeGroup"), selRowId, "GroupId", null);
        return "GROUP_ID=" + GroupId + "";
    }
    agOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.CD);
      //  notice: auto comp not using get selRow method,because when mouse click out maybe get warm rowid

        setGridVal($("#ExpReasonGroup"), rowid, "ExpDescp", ui.item.returnValue.CD_DESCP, null);
    };


    var colModel = [
            { name: 'GroupId', title: 'Group_ID', index: 'GroupId', sorttype: 'string', hidden: true },
    	    { name: 'CdType', title: 'CD_TYPE', index: 'CdType', width: 70, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: true, hidden: true },
            { name: 'Cd', title: '@Resources.Locale.L_ErrRelationSetup_Cd', index: 'Cd', width: 70, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: true },
            { name: 'CdDescp', title: '@Resources.Locale.L_ErrRelationSetup_CdDescp', index: 'CdDescp', width: 170, sorttype: 'string', editoptions: { size: 50, maxlength: 50 }, editable: true }
    ];

    var colModel2 = [
            { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
            { name: 'ExpType', title: '@Resources.Locale.L_ErrRelationSetup_ExpType', index: 'ExpType', width: 70, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, hidden: true, editable: false },
            { name: 'ExpReason', title: '@Resources.Locale.L_ErrRelationSetup_ExpReason', index: 'ExpReason', width: 100, sorttype: 'string', editoptions: { size: 50, maxlength: 50 }, hidden: false, editable: true, edittype: 'custom', editoptions: gridLookup(agOpt) },
            { name: 'ExpDescp', title: '@Resources.Locale.L_ErrRelationSetup_ExpDescp', index: 'ExpDescp', width: 150, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: true },
            { name: 'IsRelieve', title: '@Resources.Locale.L_ErrRelationSetup_IsRelieve', index: 'IsRelieve', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: "Y:YES;N:NO", dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
    ];

    //init subgrid change data setting
    gridSetting.gridId = "ExpReasonGroup";
    gridSetting.colModel = colModel2;

    $grid = $("#ExpTypeGroup");
    new genGrid(
    	$grid,
    	{
    	    datatype: "json",
    	    loadonce: true,
    	    url: rootPath + "SYSTEM/GetErrTypeData",
            colModel: colModel,
            isModel:true,
    	    caption: "@Resources.Locale.L_ErrRelationSetup_ExpType",
    	    height: gridHeight,
    	    refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
    	    pginput: false,
    	    sortable: true,
    	    pgbuttons: false,
    	    rownumWidth: 50,
    	    //inlineEdit:true,
    	    rows: 200,
    	    exportexcel: false,
    	    delKey: ["Cd"],
    	    gridFunc: function (map) {

    	        console.log(map);
    	        gridSetting.keyData = { Cd: mainKeyValue.cd };
    	        getGridChangeDataDS(gridSetting);

    	        mainKeyValue.cd = map.Cd;
    	       
    	        var cd = null;
    	        if (typeof map.Cd != "undefined") {
    	            cd = map.Cd.replace(/<.*?>/g, '');
    	        }

    	        if (cd != null && cd != "") {
    	            //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
    	            if (_oldDeatiArray[cd] != undefined || _oldDeatiArray[cd] != null) {
    	                //将json设置给BsCodeGrid
    	                //移除_state状态为0的数据，，因为_state的数据是删除的数据
    	                $.each(_oldDeatiArray[cd], function (i, val) {
    	                    if (val._state == "0") {
    	                        _oldDeatiArray[cd].splice(i, 1);
    	                    }
    	                });
    	                _dm.getDs("ExpReasonGroup").setData(_oldDeatiArray[cd]);
    	                return;
    	            }
    	            $.ajax({
    	                async: true,
    	                url: rootPath + "SYSTEM/ErrReasonRequiry",
    	                type: 'POST',
    	                data: {
    	                    Cd: cd,
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

    	                    console.log(mainTable);
    	                    var $grid = $("#ExpReasonGroup");
    	                    _oldDeatiArray[cd] = mainTable.rows;
    	                    if (_dm.getDs("ExpReasonGroup") == null || _dm.getDs("ExpReasonGroup") == undefined) {
    	                        _dm.addDs("ExpReasonGroup", mainTable.rows, ["Cd"], $grid[0]);
    	                    } else {
    	                        _dm.getDs("ExpReasonGroup").setData(mainTable.rows);
    	                    }
    	                }
    	            });
    	        }
    	    },
    	    beforeSelectRowFunc: function (rowid) {

    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#ExpTypeGroup").setColProp('Cd', { editable: true });
    	        } else {
    	            $("#ExpTypeGroup").setColProp('Cd', { editable: false });
    	        }
    	    },
    	    beforeAddRowFunc: function () {
    	        //add row 時要可以編輯main key
    	        $("#ExpTypeGroup").setColProp('Cd', { editable: true });
    	    }
    	}
    );

    $ExpReasonGroupgrid = $("#ExpReasonGroup");
    _dm.addDs("ExpReasonGroup", [], ["UId"], $ExpReasonGroupgrid[0]);
    new genGrid(
    	$ExpReasonGroupgrid,
    	{
    	    data: [],
    	    datatype: "local",
    	    //url: rootPath + "BSCODE/bscodeSortQuery",
    	    loadonce: true,
    	    colModel: colModel2,
            cellEdit: false,//禁用grid编辑功能
            isModel:true,
    	    caption: "@Resources.Locale.L_ErrRelationSetup_ExpReason",
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
    	    sortable: false,
    	    rownumWidth: 50,
    	    delKey: ["UId","ExpType"],
    	    ds: _dm.getDs("ExpReasonGroup"),
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#ExpReasonGroup").setColProp('Cd', { editable: true });
    	        } else {
    	            $("#ExpReasonGroup").setColProp('Cd', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {
    	        var maxSeqNo = $('#ExpReasonGroup').jqGrid("getCol", "ExpType", false, "max");
    	        if (typeof maxSeqNo === "undefined")
    	            maxSeqNo = 0;
    	        
    	        var selRowId = $("#ExpTypeGroup").jqGrid('getGridParam', 'selrow');
    	        var cd = $("#ExpTypeGroup").jqGrid('getCell', selRowId, 'Cd');
    	        setGridVal($("#ExpReasonGroup"), rowid, "ExpType", cd, null);
    	        
    	    },
    	    beforeAddRowFunc: function (rowid) {
    	        //add row 時要可以編輯main key
    	        $("#ExpReasonGroup").setColProp('Cd', { editable: true });

    	        //當抓到的品號為空時，必須重新回主檔抓，若還是抓不到則無法新增
    	        if (mainKeyValue.cd == null || mainKeyValue.cd == "") {
    	            var selRowId = $("#ExpTypeGroup").jqGrid('getGridParam', 'selrow');
    	            var cd = $("#ExpTypeGroup").jqGrid('getCell', selRowId, 'cd');    	      
    	            if (cd != null && cd != "") {
    	                mainKeyValue.cd = cd;

    	            } else {
    	                //alert("主档代碼有误，无法建立代碼");
    	                CommonFunc.Notify("", "@Resources.Locale.L_ErrRelationSetup_Error", 500, "danger");
    	                return false;
    	            }
    	        }
    	    },
    	    onSortCol: function (index, iCol, sortorder) {



    	    }
    	}
    );

    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);

        //gridEditableCtrl({ editable: false, gridId: "ExpTypeGroup" });
        gridEditableCtrl({ editable: false, gridId: "ExpReasonGroup" });
        editable = false;

    }

    MenuBarFuncArr.MBEdit = function () {
        var MainSelectedRowId = $("#ExpTypeGroup").jqGrid('getGridParam', 'selrow');
        //gridEditableCtrl({ editable: true, gridId: "ExpTypeGroup" });
        gridEditableCtrl({ editable: true, gridId: "ExpReasonGroup" });
        editable = true;

        $("#ExpTypeGroup").jqGrid('setSelection', MainSelectedRowId, true);
    }

    MenuBarFuncArr.MBAdd = function () {
        //gridEditableCtrl({ editable: false, gridId: "ExpTypeGroup" });
        gridEditableCtrl({ editable: true, gridId: "ExpReasonGroup" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        //获取子表的changeValue
        gridSetting.keyData = { CdType: mainKeyValue.cdType };
        getGridChangeDataDS(gridSetting);

        //var containerArray = $('#ExpTypeGroup').jqGrid('getGridParam', "arrangeGrid")();
        //var _changeDatArray = $('#ExpReasonGroup').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        //changeData["mt"] = containerArray;
        changeData["Errinfo"] = _changeDatArray;
        //console.log(containerArray);
        console.log(_changeDatArray);
        console.log(changeData);

        $.ajax({
            async: true,
            url: rootPath + "SYSTEM/ErrRelaUpdate",
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
    }

    initMenuBar(MenuBarFuncArr);
    MenuBarFuncArr.DelMenu(["MBSearch", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);//"MBAdd",
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);
});

