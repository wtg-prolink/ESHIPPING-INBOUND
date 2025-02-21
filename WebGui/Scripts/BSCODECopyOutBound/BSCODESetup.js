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

    // init country grid lookup
    /*opt.gridUrl = rootPath + "System/RoleSetInquiryData";
    opt.gridReturnFunc = function (map) {

        var selRowId = $("#ApproveFlow").jqGrid('getGridParam', 'selrow');
        //$("#ApproveFlow").jqGrid('setCell', selRowId, 'Role', map.Fid);
        //$("#ApproveFlow").jqGrid("editCell", selRowId, "TaxRate", true);
        return map.Fid;
    };
    opt.lookUpConfig = LookUpConfig.RoleLookup;
    opt.autoCompKeyinNum = 1;
    opt.autoCompUrl = "";
    opt.autoCompDt = "dt=role&GROUP_ID=" + groupId + "&Fid%";
    opt.autoCompParams = "Fid=showValue,Fid";
    opt.autoCompFunc = function (elem, event, ui) {
        $(elem).val(ui.item.returnValue.Fid);
        var selRowId = $("#ApproveFlow").jqGrid('getGridParam', 'selrow');


    }*/

    var colModel = [
    	    { name: 'CdType', title: _getLang('L_BSCODESetup_Cd','代码'), index: 'CdType', width: 100, sorttype: 'string', editoptions: { size: 4, maxlength: 4 }, editable: false },
    	    { name: 'CdDescp', title: _getLang('L_MailGroupSetup_Name','名称'), index: 'CdDescp', width: 180, sorttype: 'string', editoptions: { size: 300, maxlength: 300 }, editable: true }
    ];

    var colModel2 = [
            { name: 'Cd', title: 'BSCODE', index: 'Cd', sorttype: 'string', editoptions: { size: 20, maxlength: 20 }, hidden: false },
	        { name: 'CdDescp', title: _getLang('L_MailGroupSetup_Name','名称'), index: 'CdDescp', width: 250, sorttype: 'string', editoptions: { size: 300, maxlength: 300 }, editable: true },
            { name: 'CdType', title: _getLang('L_BSCODESetup_Cd','代码'), index: 'CdType', sorttype: 'string', hidden: true },
            { name: 'OrderBy', title: _getLang('L_TKQuery_OrderBy','顺序'), index: 'OrderBy', sorttype: 'string', width: 70, editoptions: { size: 5, maxlength: 5 }, hidden: false, editable: true },
            { name: 'ApCd', title: 'SCAC', index: 'ApCd', sorttype: 'string', width: 70, hidden: false, editoptions: { size: 50, maxlength: 50 }, editable: true },
            { name: 'ArCd', title: _getLang('L_BSCSSetup_Remark','备注'), index: 'ArCd', sorttype: 'string', width: 120, hidden: false, editoptions: { size: 24, maxlength: 24 }, editable: true }
    ];

    //init subgrid change data setting
    gridSetting.gridId = "BsCodeGrid";
    gridSetting.colModel = colModel2;

    $grid = $("#CodeKindGrid");
    new genGrid(
    	$grid,
    	{
    	    datatype: "json",
    	    loadonce: true,
            url: rootPath + "BSCODECopyOutBound/CodeKindRequiry",
    	    colModel: colModel,
    	    caption: "BSCODE Kind",
    	    height: gridHeight,
    	    refresh: true,
    	    cellEdit: false,//禁用grid编辑功能
    	    pginput: false,
    	    sortable: true,
    	    pgbuttons: false,
    	    rownumWidth:50,
    	    //inlineEdit:true,
    	    rows: 200,
    	    exportexcel: false,
    	    delKey: ["CdType"],
    	    gridFunc: function (map) {

    	        console.log(map);


    	        gridSetting.keyData = { CdType: mainKeyValue.cdType };
    	        getGridChangeDataDS(gridSetting);

    	        mainKeyValue.cdType = map.CdType;
    	        var cdType = map.CdType;

    	        if (cdType != null && cdType != "") {
    	            //此处增加判断，如果tempDeatiArray存在该笔数据的资料，则加载缓存，如果没有则通过请求来更新
    	            if (_oldDeatiArray[cdType] != undefined || _oldDeatiArray[cdType] != null) {
    	                //将json设置给BsCodeGrid
    	                //移除_state状态为0的数据，，因为_state的数据是删除的数据
    	                $.each(_oldDeatiArray[cdType], function (i, val) {
    	                    if (val._state == "0") {
    	                        _oldDeatiArray[cdType].splice(i, 1);
    	                    }
    	                });
    	                _dm.getDs("BsCodeGrid").setData(_oldDeatiArray[cdType]);
    	                return;
    	            }
    	            $.ajax({
    	                async: true,
                        url: rootPath + "BSCODECopyOutBound/BscodeRequiry",
    	                type: 'POST',
    	                data: {
    	                    CdType: cdType,
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
    	                    var $grid = $("#BsCodeGrid");
    	                    _oldDeatiArray[cdType] = mainTable.rows;
    	                    if (_dm.getDs("BsCodeGrid") == null || _dm.getDs("BsCodeGrid") == undefined) {
    	                        _dm.addDs("BsCodeGrid", mainTable.rows, ["CdType"], $grid[0]);
    	                    } else {
    	                        _dm.getDs("BsCodeGrid").setData(mainTable.rows);
    	                    }
    	                }
    	            });
    	        }
    	    },
    	    beforeSelectRowFunc: function (rowid) {

    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#CodeKindGrid").setColProp('CdType', { editable: true });
    	        } else {
    	            $("#CodeKindGrid").setColProp('CdType', { editable: false });
    	        }
    	    },
    	    beforeAddRowFunc: function () {
    	        //add row 時要可以編輯main key
    	        $("#CodeKindGrid").setColProp('CdType', { editable: true });
    	    }
    	}
    );

    $BsCodegrid = $("#BsCodeGrid");
    _dm.addDs("BsCodeGrid", [], ["Cd", "CdType"], $BsCodegrid[0]);
    new genGrid(
    	$BsCodegrid,
    	{
    	    data: [],
            datatype: "local",
            //url: rootPath + "BSCODECopyOutBound/bscodeSortQuery",
    	    loadonce: true,
    	    colModel: colModel2,
    	    cellEdit: false,//禁用grid编辑功能
    	    caption: "BsCodeGrid",
    	    height: gridHeight,
    	    refresh: true,
    	    rows: 9999,
    	    exportexcel: false,
    	    pginput: false,
    	    pgbuttons: false,
            sortable: true,
            rownumWidth: 50,
            delKey: ["Cd", "CdType"],
    	    ds: _dm.getDs("BsCodeGrid"),
    	    beforeSelectRowFunc: function (rowid) {
    	        //main key 修改時不允與修改
    	        if (rowid != null && rowid.indexOf("jqg") >= 0) {
    	            $("#BsCodeGrid").setColProp('Cd', { editable: true });
    	        } else {
    	            $("#BsCodeGrid").setColProp('Cd', { editable: false });
    	        }
    	    },
    	    onAddRowFunc: function (rowid) {
    	       
    	    },
            beforeAddRowFunc: function (rowid) {
                //add row 時要可以編輯main key
                $("#BsCodeGrid").setColProp('Cd', { editable: true });

                //當抓到的品號為空時，必須重新回主檔抓，若還是抓不到則無法新增
                if (mainKeyValue.cdType == null || mainKeyValue.cdType == "") {
                    var selRowId = $("#CodeKindGrid").jqGrid('getGridParam', 'selrow');
                    var cdType = $("#CodeKindGrid").jqGrid('getCell', selRowId, 'cdType');
                    if (cdType != null && cdType != "") {
                        mainKeyValue.cdType = cdType;

                    } else {
                        //alert("主档代碼有误，无法建立代碼");
                        CommonFunc.Notify("", _getLang('L_ErrRelationSetup_Error', '主档代码有误，无法建立代码'), 500, "danger");
                        return false;
                    }
                }
            }, onSortCol: function (index, iCol, sortorder) {
                var selRowId = $("#CodeKindGrid").jqGrid('getGridParam', 'selrow');
                var cdType = $("#CodeKindGrid").jqGrid('getCell', selRowId, 'CdType');

                if(cdType != null && cdType != "")
                {
                    $BsCodegrid.jqGrid("setGridParam",{
                        url: rootPath + "BSCODECopyOutBound/bscodeSortQuery",
                        sortorder: sortorder,
                        sortname: index,
                        postData:{CdType: cdType},
                        datatype: "json"
                    }).trigger("reloadGrid");
                }

            }
    	}
    );
    /*$BsCodegrid.jqGrid('sortableRows', {
        update: function (ev, ui) {


            var rows = $BsCodegrid.jqGrid('getDataIDs');
            console.log(rows);
            for (i = 0; i < rows.length; i++) {
                var idx = $BsCodegrid.getInd(rows[i]);
                console.log(idx);

                var opt = {};
                opt.gridId = "#BsCodeGrid";
                opt.rowId = rows[i];
                opt.cellKey = "Cd";
                opt.cellValue = idx;
                setGridChange(opt);

            }
            //var rows = $BsCodegrid.jqGrid('getDataIDs');
            //console.log(rows);
        }
    });*/
    MenuBarFuncArr.MBCancel = function () {
        MenuBarFuncArr.Enabled(["MBEdit"]);

        gridEditableCtrl({ editable: false, gridId: "CodeKindGrid" });
        gridEditableCtrl({ editable: false, gridId: "BsCodeGrid" });
        editable = false;

    }

    MenuBarFuncArr.MBEdit = function () {
        var MainSelectedRowId = $("#CodeKindGrid").jqGrid('getGridParam', 'selrow');
        gridEditableCtrl({ editable: true, gridId: "CodeKindGrid" });
        gridEditableCtrl({ editable: true, gridId: "BsCodeGrid" });
        editable = true;

        $("#CodeKindGrid").jqGrid('setSelection', MainSelectedRowId, true);
    }

    MenuBarFuncArr.MBAdd = function () {
        gridEditableCtrl({ editable: false, gridId: "CodeKindGrid" });
        gridEditableCtrl({ editable: true, gridId: "BsCodeGrid" });
        editable = true;
    }

    MenuBarFuncArr.MBSave = function (dtd) {
        //获取子表的changeValue
        gridSetting.keyData = { CdType: mainKeyValue.cdType };
        getGridChangeDataDS(gridSetting);

        var containerArray = $('#CodeKindGrid').jqGrid('getGridParam', "arrangeGrid")();
        var changeData = {};
        changeData["mt"] = containerArray;
        changeData["st"] = _changeDatArray;
        console.log(containerArray);
        console.log(_changeDatArray);
        console.log(changeData);

        $.ajax({
            async: true,
            url: rootPath + "BSCODECopyOutBound/BsCodeUpdate",
            type: 'POST',
            data: { "changedData": JSON.stringify(changeData), autoReturnData: true },
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
                CommonFunc.Notify("", _getLang('L_MailFormatSetup_SaveS', '保存成功'), 500, "success");
                MenuBarFuncArr.SaveResult = true;
                dtd.resolve();
                location.reload();
            }
        });

        return dtd.promise();

    };


    initMenuBar(MenuBarFuncArr);

    MenuBarFuncArr.DelMenu(["MBSearch",  "MBDel", "MBCopy", "MBApply", "MBEdoc", "MBApprove", "MBInvalid"]);//"MBAdd",
    MenuBarFuncArr.Disabled(["MBSave"]);
    MenuBarFuncArr.Enabled(["MBEdit"]);

});

function _getLang(id, caption) {
    try {
        return GetLangCaption(id, caption);
    }
    catch (e) { }
    return caption || id;
}

