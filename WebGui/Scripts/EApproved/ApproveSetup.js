var _dm = new dm();
var _oldDeatiArray = {};//存放所有的值，包括修改和没有修改的
var _changeDatArray = [];
var mainKeyValue = {};
var groupId = getCookie("plv3.passport.groupid");
var cmp = getCookie("plv3.passport.companyid");
var stn = getCookie["plv3.passport.station"];
var gridSetting = {};

jQuery(document).ready(function ($) {
    var opt = {}, aprOpt={};
   
    var editable = false;
    var docHeight = $(document).height();
    gridHeight = docHeight - 130;

    var sel = "";
    $.each(DeAData, function(i, val) {
        if(DeAData.length == i + 1)
        {
            sel += DeAData[i].UId + ":" + DeAData[i].ApproveAttr;
        }
        else
        {
            sel += DeAData[i].UId + ":" + DeAData[i].ApproveAttr + ";";
        }
        
    });

    var colModel = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'CmpId', title: 'CmpId', index: 'CmpId', width: 70, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: false, hidden: true },
        { name: 'ApproveCode', title: '@Resources.Locale.L_ApproveSetup_ApproveCode', index: 'ApproveCode', width: 70, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: true },
        { name: 'ApproveName', title: '@Resources.Locale.L_ApproveSetup_ApproveName', index: 'ApproveName', width: 170, sorttype: 'string', editoptions: { size: 50, maxlength: 50 }, editable: true },
        { name: 'AuId', title: '@Resources.Locale.L_ApproveSetup_AuId', index: 'AuId', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: sel, dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
        { name: 'TvMnt', title: 'BU', index: 'TvMnt', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: "ALL:ALL;TV:TV;MNT:MNT;PD:PD", dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
    ];

    agOpt = {};
    agOpt.gridUrl = rootPath + "TPVCommon/GetAppDData";
    agOpt.baseConditionFunc = function () {
        var selRowId = $("#ApproveGroup").jqGrid('getGridParam', 'selrow');
        var AuId = getGridVal($("#ApproveGroup"), selRowId, "AuId", null);
        return " U_FID = '"+AuId+"' AND CMP='" + cmp + "'";
    }
    agOpt.gridReturnFunc = function (map) {
        var selRowId = $("#ApproveFlow").jqGrid('getGridParam', 'selrow');
        //params: string grid id,string rowid,string cell name,string value,string 'lookup' or null
        setGridVal($("#ApproveFlow"), selRowId, "GuId", map.UId, null);
        setGridVal($("#ApproveFlow"), selRowId, "GroupDescp", map.GroupDescp, null);
        setGridVal($("#ApproveFlow"), selRowId, "SeniorStaff", map.SeniorStaff, null);
        return map.ApproveGroup;
    };
    agOpt.lookUpConfig = LookUpConfig.ApproveDLookup;
    agOpt.gridId = "ApproveFlow";
    agOpt.param = "";
    agOpt.autoCompUrl = "";
    agOpt.autoCompDt = "dt=apprad&GROUP_ID=" + groupId + "&APPROVE_GROUP=";
    agOpt.autoCompParams = "GROUP_DESCP=showValue,APPROVE_GROUP,GROUP_DESCP,SENIOR_STAFF";
    agOpt.autoCompGetValueFunc = function () {
        var selRowId = $("#ApproveGroup").jqGrid('getGridParam', 'selrow');
        var AuId = getGridVal($("#ApproveGroup"), selRowId, "AuId", null);
        return "U_FID=" + AuId + "";
    }
    agOpt.autoCompFunc = function (elem, event, ui, rowid) {
        $(elem).val(ui.item.returnValue.APPROVE_GROUP);
        //notice: auto comp not using get selRow method,because when mouse click out maybe get warm rowid
        setGridVal($("#ApproveFlow"), rowid, "GroupDescp", ui.item.returnValue.GROUP_DESCP, null);
        setGridVal($("#ApproveFlow"), rowid, "SeniorStaff", ui.item.returnValue.SENIOR_STAFF, null);
    }

    var colModel2 = [
        { name: 'UId', title: 'U ID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'UFid', title: 'U FID', index: 'UId', sorttype: 'string', hidden: true },
        { name: 'TvMnt', title: 'BU', index: 'TvMnt', width: 70, sorttype: 'string', editable: false, hidden: true },
        { name: 'ApproveCode', title: '@Resources.Locale.L_ApproveSetup_ApproveCode', index: 'ApproveCode', sorttype: 'string', hidden: true },
        { name: 'ApproveLevel', title: '@Resources.Locale.L_ApproveSetup_ApproveLevel', index: 'ApproveLevel', sorttype: 'int', editable: true, hidden: false, formatter: 'number', formatoptions: { decimalSeparator: ".", thousandsSeparator: ",", decimalPlaces: 0, defaultValue: '0' } },
        { name: 'Role', title: '@Resources.Locale.L_ApproveSetup_Role', index: 'Role', width: 100, sorttype: 'string', editoptions: { size: 10, maxlength: 10 }, editable: true, edittype: 'custom', editoptions: gridLookup(agOpt) },
        { name: 'GroupDescp', title: '@Resources.Locale.L_ApproveSetup_ApproveName', index: 'GroupDescp', width: 180, sorttype: 'string', editoptions: { size: 70, maxlength: 70 }, editable: false },
        { name: 'SeniorStaff', title: _getLang("L_SeniorStaff", "是否高层"), index: 'SeniorStaff', sorttype: 'string', width: 70, hidden: false, editable: true, formatter: "select", edittype: 'select', editoptions: { value: ':;Y:Yes;N:No' } },
        { name: 'GuId', title: '@Resources.Locale.L_ApproveSetup_GuId', index: 'GuId', sorttype: 'string', hidden: true },
        { name: 'BackFlag', title: '@Resources.Locale.L_ApproveSetup_BackFlag', index: 'BackFlag', width: 70, sorttype: 'string', editable: true, formatter: "select", edittype: 'select', editoptions: { value: "Y:Yes;N:No", dataInit: function (elem) { $(elem).addClass('form-control input-sm'); } } },
        { name: 'ApproveTime', title: '@Resources.Locale.L_ApproveSetup_ApproveTime', index: 'ApproveTime', width: 70, sorttype: 'int', formatter: "integer", editoptions: { size: 5, maxlength: 5 }, hidden: false, editable: true }
    ];

    //init subgrid change data setting
	gridSetting.gridId = "ApproveFlow";
	gridSetting.colModel = colModel2;

	$grid = $("#ApproveGroup");
    new genGrid(
    	$grid,
    	{
    	    datatype: "json",
            loadonce:true,
            url: rootPath + "EAPPROVED/ApproveRequiry",
            colModel: colModel,
            isModel:true,
    	    caption: "Approve Group",
    	    height: "auto",
    	    rows: 9999,
            refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
            pginput: false,
            sortable: true,
            pgbuttons: false,
            //editOnly:true,
            height: gridHeight,
    	    exportexcel: false,
            delKey: ["UId"],
            onSelectRowFunc: function (map) {
                gridSetting.keyData = { ApproveCode: mainKeyValue.approveCode, TvMnt: mainKeyValue.tvMnt, UFid: mainKeyValue.uid };
    	        getGridChangeDataDS(gridSetting);

                mainKeyValue.approveCode = map.ApproveCode;
                mainKeyValue.tvMnt = map.TvMnt;
                mainKeyValue.uid = map.UId;
    	        //notice when post to backend it have to strip html tag avoid error
    	        var approveCode = null;
                if (typeof map.UId != "undefined") {
                    approveCode = map.UId;
                }
    	        

    	        if (approveCode != null && approveCode != "") {
    	            //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
    	            if (_oldDeatiArray[approveCode] != undefined || _oldDeatiArray[approveCode] != null) {
    	                //将json设置给ApproveFlow
    	                //移除_state状态为0的数据，，因为_state的数据是删除的数据
    	                $.each(_oldDeatiArray[approveCode], function (i, val) {
    	                    if (val._state == "0") {
    	                        _oldDeatiArray[approveCode].splice(i, 1);
    	                    }
    	                });
    	                _dm.getDs("ApproveFlow").setData(_oldDeatiArray[approveCode]);
    	                return;
    	            }
    	            $.ajax({
    	                async: true,
    	                url: rootPath + "EAPPROVED/ApprovedRequiry",
    	                type: 'POST',
    	                data: {
    	                    ApproveCode: approveCode,
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
    	                    var mainTable = $.parseJSON(result.mainTable.Content);
    	                    var $grid = $("#ApproveFlow");
    	                    _oldDeatiArray[approveCode] = mainTable.rows;
    	                    if (_dm.getDs("ApproveFlow") == null || _dm.getDs("ApproveFlow") == undefined) {
    	                        _dm.addDs("ApproveFlow", mainTable.rows, ["ApproveCode","ApproveLevel"], $grid[0]);
    	                    } else {
    	                        _dm.getDs("ApproveFlow").setData(mainTable.rows);
    	                    }
    	                }
    	            });
    	        }
    	    },
    	    beforeSelectRowFunc: function (rowid) {

    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
                    $("#ApproveGroup").setColProp('ApproveCode', { editable: true });
                    //$("#ApproveGroup").setColProp('TvMnt', { editable: true });
                } else {
                    $("#ApproveGroup").setColProp('ApproveCode', { editable: false }); 
                    //$("#ApproveGroup").setColProp('TvMnt', { editable: false });
    	        }
    	    },
    	    beforeAddRowFunc: function () {
    	        //add row 時要可以編輯main key
    	        $("#ApproveGroup").setColProp('ApproveCode', { editable: true });
    	    },
    	    onAddRowFunc: function (rowid) {
                $("#ApproveGroup").jqGrid('setCell', rowid, "CmpId", cmp, 'edit-cell dirty-cell');
    	    }
   		}
    );

    $ApproveFlowgrid = $("#ApproveFlow");
    _dm.addDs("ApproveFlow", [], ["UId"], $ApproveFlowgrid[0]);
    new genGrid(
    	$ApproveFlowgrid,
    	{ 
    	    data: [],
    	    loadonce: true,
            colModel: colModel2,
            cellEdit: false,//禁用grid编辑功能
            isModel: true,
            caption: "Approve Flow",
            height: "auto",
            rows: 9999,
            refresh: true,
            exportexcel: false,
            pginput: false,
            pgbuttons: false,
            height: gridHeight,
            ds: _dm.getDs("ApproveFlow"),
            sortable:false, //using ds grid have to close sortable 
            //sortorder: "asc",
            //sortname: "ApproveLevel",
            onAddRowFunc: function (rowid) {
    	        var maxSeqNo = $('#ApproveFlow').jqGrid("getCol", "ApproveLevel", false, "max");
    	        if(typeof maxSeqNo === "undefined")
    	            maxSeqNo = 0;

    	        $('#ApproveFlow').jqGrid('setCell', rowid, "ApproveLevel", maxSeqNo + 1);
            },
            beforeAddRowFunc:function(){
                if (mainKeyValue.approveCode == null || mainKeyValue.approveCode == "") {
                    var selRowId = $("#ApproveGroup").jqGrid('getGridParam', 'selrow');
                    var approveCode = $("#ApproveGroup").jqGrid('getCell', selRowId, 'ApproveCode');
                    var tvmnt = $("#ApproveGroup").jqGrid('getCell', selRowId, 'TvMnt');
                    var uid = $("#ApproveGroup").jqGrid('getCell', selRowId, 'UId');
                    if (approveCode != null && approveCode != "") {
                        mainKeyValue.approveCode = approveCode;
                        mainKeyValue.tvMnt = tvmnt;
                        mainKeyValue.uid = uid;
                    } else {
                        //alert("主档代碼有误，无法建立Flow");
                        CommonFunc.Notify("", "@Resources.Locale.L_ApproveSetup_Wrong", 500, "danger");

                        return false;
                    }
                }
            },
    	    onSelectRowFunc: function (rowid) {
            }
   		}
    );
    
    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);
        location.reload();
        gridEditableCtrl({ editable: false, gridId: "ApproveGroup" });
        gridEditableCtrl({ editable: false, gridId: "ApproveFlow" });
        editable = false;


    }

    MenuBarFuncArr.MBEdit = function () {
        var MainSelectedRowId = $("#ApproveGroup").jqGrid('getGridParam', 'selrow');
        gridEditableCtrl({ editable: true, gridId: "ApproveGroup" });
        gridEditableCtrl({ editable: true, gridId: "ApproveFlow" });
        editable = true;

        $("#ApproveGroup").jqGrid('setSelection', MainSelectedRowId, true);
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        //获取子表的changeValue
        gridSetting.keyData = { ApproveCode: mainKeyValue.approveCode, TvMnt: mainKeyValue.tvMnt, UFid: mainKeyValue.uid };
        getGridChangeDataDS(gridSetting);
       
        var containerArray = $('#ApproveGroup').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        changeData["approvedinfo"] = _changeDatArray;
        /*console.log(containerArray);
        console.log(_changeDatArray);
        console.log(changeData);*/
        $.ajax({
            async: true,
            url: rootPath + "EAPPROVED/ApprovedUpdate",
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

    MenuBarFuncArr.DelMenu(["MBSearch", "MBAdd", "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

